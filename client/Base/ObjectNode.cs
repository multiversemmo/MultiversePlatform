/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Input;
using Axiom.Graphics;
using Axiom.Animating;
using Axiom.Collections;

using Multiverse.Config;
using Multiverse.Network;
using Multiverse.CollisionLib;
using Multiverse.Lib.LogUtil;

using Vector3 = Axiom.MathLib.Vector3;

#endregion

namespace Multiverse.Base
{
	// Marker interface
	public class WorldEntity : IDisposable {
		protected long oid;

		public WorldEntity(long oid) {
			this.oid = oid;
		}

		public virtual void Dispose() {
		}

		public long Oid {
			get {
				return oid;
			}
		}
	}

	public class LightEntity : WorldEntity {
		Light light;

		public LightEntity(long oid, Light light)
			: base(oid) {
			this.light = light;
		}

		public Light Light {
			get {
				return light;
			}
		}

		public override string ToString() {
			return string.Format("Light {0}: {1} light at {2} to {3}", 
								 oid, light.Type, light.Position, light.Direction);
		}
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="triggerTime">the time offset within the animation that the event was set to trigger at</param>
    public delegate void AnimationTimeHandler(AnimationStateInfo state, float triggerTime);

    public class AnimationStateInfo
    {
        protected AnimationState animationState;
        protected float animationSpeed;
        protected bool looping;
        protected float startOffset;
        protected float endOffset;

        SortedList<float, List<AnimationTimeHandler>> timeEvents;

        public AnimationStateInfo(AnimationState state, float speed, bool loop)
            : this(state, 0, state.Length, speed, loop)
        {
        }

        public AnimationStateInfo(AnimationState state, float start, float end, float speed, bool loop)
        {
            animationState = state;
            animationSpeed = speed;
            looping = loop;
            startOffset = start;
            endOffset = end;

            timeEvents = new SortedList<float, List<AnimationTimeHandler>>();
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}:{3}:{4}", animationState.Name,
                                 animationState.IsEnabled, 
                                 animationState.Weight, animationSpeed,
                                 looping);
        }

        public void RegisterTimeEventHandler(float time, AnimationTimeHandler handler)
        {
            if (!timeEvents.ContainsKey(time))
            {
                timeEvents[time] = new List<AnimationTimeHandler>();
            }
            List<AnimationTimeHandler> handlers = timeEvents[time];

            handlers.Add(handler);
        }

        public void RemoveTimeEventHandler(float time, AnimationTimeHandler handler)
        {
            List<AnimationTimeHandler> handlers = timeEvents[time];
            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                timeEvents.Remove(time);
            }
        }

        protected void TriggerTimeEvents(float startTime, float endTime)
        {
            Debug.Assert(startTime >= 0);
            Debug.Assert(startTime <= endTime);
            Debug.Assert(endTime <= this.EndOffset);

            IList<float> keys = timeEvents.Keys;

            if (keys.Count > 0)
            {
                int startIndex;

                if ((endTime < keys[0]) || (startTime > keys[keys.Count - 1]))
                {
                    // time segment doesn't overlap triggers
                    return;
                }
                // special case trying to trigger on the start of the animation
                if (startTime == 0 && keys[0] == 0)
                {
                    startIndex = 0;
                }
                else
                {
                    for (startIndex = 0; startIndex < timeEvents.Count; startIndex++)
                    {
                        if (keys[startIndex] > startTime)
                        {
                            break;
                        }
                    }
                }

                // only continue if there are event triggers after startTime
                if (startIndex < timeEvents.Count)
                {
                    int endIndex;
                    for (endIndex = startIndex + 1; endIndex < timeEvents.Count; endIndex++)
                    {
                        if (keys[endIndex] > endTime)
                        {
                            break;
                        }
                    }

                    // Call handlers for every time trigger in the range
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        float triggerTime = keys[i];
                        List<AnimationTimeHandler> handlers = timeEvents.Values[i];

                        foreach (AnimationTimeHandler handler in handlers)
                        {
                            handler(this, triggerTime);
                        }
                    }
                }
            }
        }

        public bool AddTime(float t, out float overflow)
        {
            float timeStep = t * this.Speed;
            float oldTime = this.State.Time;
            float newTime = timeStep + oldTime;
            
            if ((this.Speed >= 0 && (newTime <= this.EndOffset)) ||
                (this.Speed < 0 && (newTime >= this.StartOffset))) {
                this.State.Time = newTime;
                overflow = 0.0f;
                TriggerTimeEvents(oldTime, newTime);
                return false;
            } else if (this.Looping) {
                // we need to loop, and we are going to pass one of the ends
                float animSpan = this.EndOffset - this.StartOffset;
                float time = newTime - this.StartOffset;
                float remainder = time - animSpan * (float)Math.Floor(time / animSpan);
                this.State.Time = this.StartOffset + remainder;
                TriggerTimeEvents(oldTime, this.EndOffset);
                TriggerTimeEvents(this.StartOffset, this.State.Time);
                Debug.Assert(this.State.Time >= this.StartOffset && this.State.Time <= this.EndOffset);
                overflow = 0.0f;
                return false;
            } else {
                // if we are at the end of the animation, then set the time to the final
                // keyframe of the animation, and then queue the removal of the animation.
                this.State.Time = (this.Speed > 0) ? this.EndOffset : this.StartOffset;
                TriggerTimeEvents(oldTime, this.State.Time);
                overflow = newTime - this.State.Time;
                return true;
            }
        }

        public AnimationState State
        {
            get
            {
                return animationState;
            }
        }
        public float Speed
        {
            get
            {
                return animationSpeed;
            }
        }
        public bool Looping
        {
            get
            {
                return looping;
            }
        }

        public float StartOffset {
            get {
                return startOffset;
            }
        }
        public float EndOffset {
            get {
                return endOffset;
            }
        }

    }

	public class AnimatedEntity : WorldEntity
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AnimatedEntity));

		protected Entity entity;

		// List of currently active animation states
		protected List<List<AnimationStateInfo>> animationStatesQueue = null;

		public AnimatedEntity(long oid) : base(oid) { }



		public void ClearAnimationQueue() {
			// Clear current animations
			Monitor.Enter(animationStatesQueue);
			try {
				// Remove all of the animation states
				while (animationStatesQueue.Count > 0) {
					ClearAnimations(animationStatesQueue[0]);
					animationStatesQueue.RemoveAt(0);
				}
			} finally {
				Monitor.Exit(animationStatesQueue);
			}
		}

		public AnimationStateInfo QueueAnimation(AnimationEntry animEntry) {
			return QueueAnimation(animEntry.animationName, 0.0f, 0.0f, animEntry.animationSpeed, 1.0f, animEntry.loop);
		}

        public AnimationStateInfo QueueAnimation(string animation, float startOffset, float endOffset, float speed, float weight, bool looping) {
            AnimationStateInfo newAnim = CreateAnimationInfo(animation, startOffset, endOffset, speed, weight, looping);
			// TODO: Make sure these don't happen often
			if (newAnim == null)
				return null;
			Monitor.Enter(animationStatesQueue);
			// First, check to see if we are adding a looping animation
			// to an animation queue whose only entry is that looping animation
			// In that case, we don't want to restart the animation.
			try {
				if (newAnim.Looping &&
					animationStatesQueue.Count == 1 &&
					animationStatesQueue[0].Count == 1 &&
					animationStatesQueue[0][0] == newAnim)
					return null;

				List<AnimationStateInfo> oldAnimStates = this.AnimationStates;
				// Remove all looping animations from existing 
				// animation states
				foreach (List<AnimationStateInfo> animStates in animationStatesQueue) {
					int j = 0;
					while (j < animStates.Count) {
						AnimationStateInfo animInfo = animStates[j];
						if (animInfo.Looping)
							RemoveAnimation(animStates, animInfo);
						else
							j++;
					}
				}
				// Remove any empty animation states sets
				int i = 0;
				while (i < animationStatesQueue.Count) {
					if (animationStatesQueue[i].Count == 0)
						animationStatesQueue.RemoveAt(i);
					else
						i++;
				}

				// Queue our animation
				List<AnimationStateInfo> newAnimStates = new List<AnimationStateInfo>();
				animationStatesQueue.Add(newAnimStates);
				AddAnimation(newAnimStates, newAnim);
				// Since I may have removed the first set of states, activate
				// the current set of states.
				EnableAnimations(this.AnimationStates);
				// If we have changed states, reset the time.
				if (this.AnimationStates != oldAnimStates)
					ResetAnimations(this.AnimationStates);
			} finally {
				Monitor.Exit(animationStatesQueue);
			}
            return newAnim;
		}

		private AnimationStateInfo CreateAnimationInfo(string animation, float startOffset, float endOffset, 
                                                       float speed, float weight, bool looping) {
			log.InfoFormat("entity: {0}, animation: {1}", entity, animation);
			AnimationStateSet states = entity.GetAllAnimationStates();
			if (states == null) {
				log.Warn("Entity has no animation states");
				return null;
			}
			AnimationState state = states.GetAnimationState(animation);
			if (state == null) {
				log.WarnFormat("Invalid animation: {0}", animation);
				return null;
			}
            if (endOffset == 0)
                endOffset = state.Length;
            state.Time = startOffset;
            state.Weight = weight;
			return new AnimationStateInfo(state, startOffset, endOffset, speed, looping);
		}

		private static void AddAnimation(List<AnimationStateInfo> animStates,
								         AnimationStateInfo animInfo) {
			RemoveAnimation(animStates, animInfo.State.Name);
			if (animInfo == null)
				return;
			animStates.Add(animInfo);
		}

		public void AddAnimation(AnimationStateInfo animInfo) {
	    	Monitor.Enter(animationStatesQueue);
			try {
		    	AddAnimation(this.AnimationStates, animInfo);
                // EnableAnimations(this.AnimationStates);
                animInfo.State.IsEnabled = true;
	    		// For now, always reset the animation with this call
		    	// ResetAnimations(this.AnimationStates);
                // animInfo.State.Time = 0.0f;
            } finally {
                Monitor.Exit(animationStatesQueue);
            }
        }

		public void AddAnimation(string animation, float startOffset, float endOffset, float speed, float weight, bool looping) {
			AnimationStateInfo animInfo = CreateAnimationInfo(animation, startOffset, endOffset, speed, weight, looping);
			// TODO: Make sure these don't happen often
			if (animInfo == null)
				return;
			AddAnimation(animInfo);
		}

		/// <summary>
		///   Remove an animation from a set of animation states.
		///   The animation will not be enabled, so if it was active,
		///   it will be stopped.
		/// </summary>
		/// <param name="animStates"></param>
		/// <param name="animation"></param>
		private static void RemoveAnimation(List<AnimationStateInfo> animStates,
									        string animation) {
			foreach (AnimationStateInfo animInfo in animStates) {
				if (animInfo.State.Name == animation) {
					animInfo.State.IsEnabled = false;
					animStates.Remove(animInfo);
					break;
				}
			}
		}
		/// <summary>
		///   Remove an animation from a set of animation states.
		///   The animation will not be enabled, so if it was active,
		///   it will be stopped.
		/// </summary>
		/// <param name="animStates"></param>
		/// <param name="animation"></param>
		private static void RemoveAnimation(List<AnimationStateInfo> animStates,
									        AnimationStateInfo animToRemove) {
			foreach (AnimationStateInfo animInfo in animStates) {
				if (animInfo == animToRemove) {
					animInfo.State.IsEnabled = false;
					animStates.Remove(animInfo);
					break;
				}
			}
		}

		public void RemoveAnimation(string animation) {
   	    	Monitor.Enter(animationStatesQueue);
            try {
			    RemoveAnimation(this.AnimationStates, animation);
			} finally {
				Monitor.Exit(animationStatesQueue);
			}

		}

		private static void DisableAnimations(List<AnimationStateInfo> animStates) {
            foreach (AnimationStateInfo animInfo in animStates) {
                animInfo.State.IsEnabled = false;
                log.InfoFormat("EnableAnimations - animInfo = {0}", animInfo);
            }
		}

		private static void EnableAnimations(List<AnimationStateInfo> animStates) {
            foreach (AnimationStateInfo animInfo in animStates) {
                animInfo.State.IsEnabled = true;
                log.InfoFormat("EnableAnimations - animInfo = {0}", animInfo);
            }
		}

		private static void ResetAnimations(List<AnimationStateInfo> animStates) {
			foreach (AnimationStateInfo animInfo in animStates)
                animInfo.State.Time = animInfo.StartOffset;
		}

		/// <summary>
		///  Clear the current set of animations
		/// </summary>
		private static void ClearAnimations(List<AnimationStateInfo> animStates) {
			DisableAnimations(animStates);
			animStates.Clear();
		}

		public void ClearAnimations() {
	    	Monitor.Enter(animationStatesQueue);
			try {
			    ClearAnimations(this.AnimationStates);
            } finally {
                Monitor.Exit(animationStatesQueue);
            }
        }

		protected void Init() {
			animationStatesQueue = new List<List<AnimationStateInfo>>();
			animationStatesQueue.Add(new List<AnimationStateInfo>());
		}

		protected void TickAnimation(float timeSinceLastFrame) {
			Monitor.Enter(animationStatesQueue);
			try {
                // copy the animStates into a new list, because time event handlers that trigger inside
                //  of AddTime might change the current set of playing animations.
				List<AnimationStateInfo> animStates = new List<AnimationStateInfo>(this.AnimationStates);
				float timeOverflow = timeSinceLastFrame;
                
				foreach (AnimationStateInfo info in animStates) {
                    float overflow = 0f;
                    info.AddTime(timeSinceLastFrame, out overflow);
                    if (overflow != 0 && overflow < timeOverflow)
                        timeOverflow = overflow;
				}
				if (timeOverflow > 0 && animationStatesQueue.Count > 1)
					// We have no more animations running in this set.
					// Move on to the next set.
					if (animationStatesQueue.Count > 0) {
						List<AnimationStateInfo> oldStates = animationStatesQueue[0];
						DisableAnimations(animationStatesQueue[0]);
						animationStatesQueue.RemoveAt(0);
						if (animationStatesQueue.Count > 0) {
							EnableAnimations(animationStatesQueue[0]);
							ResetAnimations(animationStatesQueue[0]);
							TickAnimation(timeOverflow);
						}
					}
			} finally {
				Monitor.Exit(animationStatesQueue);
			}
		}

		public string GetAnimationStates() {
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < animationStatesQueue.Count; ++i) {
				sb.Append("Animation Queue ");
				sb.Append(i);
				sb.Append(": ");
				List<AnimationStateInfo> animStates = animationStatesQueue[i];
				foreach (AnimationStateInfo animInfo in animStates) {
					sb.Append("\n\t");
					sb.Append(animInfo.State.Name);
					sb.Append(":");
					sb.Append(animInfo.State.IsEnabled);
					sb.Append(":");
					sb.Append(animInfo.Looping);
				}
				sb.Append("\n");
			}
			return sb.ToString();
		}

		/// <summary>
		///   Get the currently active animation states
		/// </summary>
		/// <value></value>
		public List<AnimationStateInfo> AnimationStates {
			get {
				Monitor.Enter(animationStatesQueue);
				try {
					if (animationStatesQueue.Count == 0)
						animationStatesQueue.Add(new List<AnimationStateInfo>());
					return animationStatesQueue[0];
				} finally {
					Monitor.Exit(animationStatesQueue);
				}
			}
		}

		public override void Dispose() {
			ClearAnimations();
		}
	}

    public class PropertyChangeEventArgs : EventArgs
    {
        protected string propName;

        public PropertyChangeEventArgs(string name)
        {
            propName = name;
        }

        public string PropertyName
        {
            get
            {
                return propName;
            }
        }
    }

    public delegate void ObjectNodeDisposed(ObjectNode objNode);
    public delegate void PositionChangeEventHandler(object sender, EventArgs args);
    public delegate void OrientationChangeEventHandler(object sender, EventArgs args);
    public delegate void PropertyChangeEventHandler(object sender, PropertyChangeEventArgs args);

	public class ObjectNode : AnimatedEntity
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ObjectNode));

        protected SceneNode sceneNode;
		protected WorldManager worldManager;

		protected ObjectNodeType objType;
        protected MovingObject collider;
        protected List<CollisionShape> collisionShapes = null;
        protected int perceptionRadius = 0;
        // Dictionary mapping an attached object's id to a node for attaching 
        // objects.  The type of Node may be SceneNode or TagPoint.
        Dictionary<long, Node> attachments = new Dictionary<long, Node>();
        // Dictionary mapping an attached object's id to the movable object
        // that is attached there.
        Dictionary<long, MovableObject> attachedObjects = new Dictionary<long, MovableObject>();
        // Dictionary of local attachments (such as the name or bubble) that do
        // not have real object ids.
        Dictionary<string, long> localAttachments = new Dictionary<string, long>();
        Dictionary<MovableObject, long> scriptAttachments = new Dictionary<MovableObject, long>();
        Dictionary<Node, long> scriptNodeAttachments = new Dictionary<Node, long>();

        protected string name;

        protected string entityName;

		// Does this object stick to the ground?  If so, we handle a number
		// of things differently.  The orientation data that is set is only
		// applied to the yaw - roll and pitch are determined by the terrain
		// for obects which are shorter than they are wide.  On objects that
		// are taller than they are wide, the roll and pitch are fixed.
		// On structure objects, we do actually still honor the roll and pitch
		// specified by the server, since those are presumably set correctly.
		protected bool followTerrain;
		protected bool isUpright;

		// Are we using the variant material with the emissive set
		protected bool glowing = false;

        protected bool targetable;

		protected long lastLocTimestamp = -1;

        private Dictionary<string, object> properties = new Dictionary<string, object>();

		protected Dictionary<string, AttachmentPoint> attachmentPoints;

		// List of attached sound sources
		protected List<SoundSource> soundSources;

        // Is the ObjectNode part of a StaticGeometry, and therefore
        // not in the scene graph?
        protected bool inStaticGeometry = false;

		/// <summary>
		///   The OnMouseClicked event (called when the user clicks on the object)
		/// </summary>
		public event MouseEventHandler MouseClicked;

		/// <summary>
		///   The OnMouseEnter event (called when the user mouses over the object)
		/// </summary>
		public event MouseEventHandler MouseEnter;

		/// <summary>
		///   The OnMouseExit event (called when the user mouses off of the object)
		/// </summary>
		public event MouseEventHandler MouseExit;

        public event ObjectNodeDisposed Disposed;

        public event PositionChangeEventHandler PositionChange;
        public event OrientationChangeEventHandler OrientationChange;
        public event PropertyChangeEventHandler PropertyChange;

        /// <summary>
        ///   True if the MobNode should have a collision object;
        ///   false otherwise.
        /// </summary>
		protected bool useCollisionObject = true;

#if OLD_ATTACHMENT
		public struct AttachmentPoint
		{
			public Quaternion offsetOrientation;
			public Vector3 offsetPosition;
			public string boneName;
		}
#endif

		public ObjectNode(long oid, string name, SceneNode sceneNode, ObjectNodeType objType, WorldManager worldManager) 
			: base(oid) 
		{
			Init(name, sceneNode, objType, worldManager);
        }

		/// <summary>
		///   Fire the MouseClicked event (call the methods registered for that event)
		/// </summary>
		/// <param name="args"></param>
		public void OnMouseClicked(MouseEventArgs args) {
			if (MouseClicked != null)
				MouseClicked(this, args);
		}

		/// <summary>
		///   Fire the MouseEnter event (call the methods registered for that event)
		/// </summary>
		/// <param name="args"></param>
		public void OnMouseEnter(MouseEventArgs args) {
			if (MouseEnter != null)
				MouseEnter(this, args);
		}

		/// <summary>
		///   Fire the MouseExit event (call the methods registered for that event)
		/// </summary>
		/// <param name="args"></param>
		public void OnMouseExit(MouseEventArgs args) {
			if (MouseExit != null)
				MouseExit(this, args);
		}

        protected virtual void OnPositionChange()
        {
            PositionChangeEventHandler handler = PositionChange;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected virtual void OnOrientationChange()
        {
            OrientationChangeEventHandler handler = OrientationChange;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

		public static SceneNode CreateSceneNode(SceneManager sceneManager, 
												long oid, Vector3 location, Quaternion orientation)
		{
			SceneNode sceneNode = null;
			Monitor.Enter(sceneManager);
			try {
				sceneNode = sceneManager.RootSceneNode.CreateChildSceneNode("object." + oid, location, orientation);
                log.Info("Created sceneNode");
            } finally {
                Monitor.Exit(sceneManager);
            }
			return sceneNode;
		}

		protected void Init(string name, SceneNode sceneNode, ObjectNodeType objType, WorldManager worldManager) {
            this.name = name;
            this.sceneNode = sceneNode;
            this.objType = objType;
			this.worldManager = worldManager;
            this.Targetable = false;

			attachmentPoints = new Dictionary<string, AttachmentPoint>();
			soundSources = new List<SoundSource>();

			base.Init();
		}

		/// <summary>
        ///   Update the node
        /// </summary>
        /// <param name="timeSinceLastFrame">time since last frame (in seconds)</param>
        public virtual void Tick(float timeSinceLastFrame) {
			TickAnimation(timeSinceLastFrame);

            // update position of positonal sounds
            Vector3 position = sceneNode.DerivedPosition;
            foreach (SoundSource source in soundSources)
                source.Position = position;
		}

        /// <summary>
        ///   Sets up the data needed for interpolated movement of an object.
        ///   In this case, it is the direction, location, and timestamp when 
        ///   that location was the current location.
        /// </summary>
        /// <param name="timestamp">time that the message was created (in client time)</param>
        /// <param name="dir">direction of motion of the object</param>
        /// <param name="loc">initial location of the object (at the time the message was created)</param>
		public virtual void SetDirLoc(long timestamp, Vector3 dir, Vector3 pos) {
			SetLoc(timestamp, pos);
		}

		protected void SetLoc(long timestamp, Vector3 loc) {
			if (timestamp <= lastLocTimestamp)
				return;
			// timestamp is newer.
			lastLocTimestamp = timestamp;
			// Use the property to set position (which side effects the 
			// sounds, namebar, and bubble chat)
			// This will be applied on the next tick
			Vector3 newLoc = worldManager.ResolveLocation(this, loc);
            log.DebugFormat("loc for node {0} = newLoc {1} oldLoc {2}", this.Oid, newLoc, this.Position);
            Vector3 displacement = newLoc - this.Position;
            this.Position = newLoc;
			if (collider != null)
                collider.AddDisplacement(displacement);
		}

		public virtual void SetOrientation(Quaternion orient) {
			// directly modify the orientation (instead of using property) to
			// avoid marking the orientation dirty. 
			// This will be applied on the next tick
			Orientation = worldManager.ResolveOrientation(this, orient);
		}

		public void ClearAttachments() {
            foreach (long objectId in attachments.Keys)
                DetachObject(objectId, false);
            attachments.Clear();
            attachedObjects.Clear();
            localAttachments.Clear();
            scriptAttachments.Clear();
            scriptNodeAttachments.Clear();
        }

        public Matrix4 AttachmentPointTransform(string slotName)
        {
            AttachmentPoint socket;
            bool found = attachmentPoints.TryGetValue(slotName, out socket);
            if (found)
            {
                if (socket.ParentBone != null)
                {
                    // its a bone attachment
                    Bone parentBone = Entity.Skeleton.GetBone(socket.ParentBone);
                    // Bone.FullTransform isn't really the full transform, and 
                    // I don't want to fix that right now.  Just multiply by
                    // the SceneNode's full transform.
                    return sceneNode.FullTransform * parentBone.FullTransform * socket.Transform;
                }
                else
                {
                    return sceneNode.FullTransform * socket.Transform;
                }
            }
            else
            {
                return sceneNode.FullTransform;
            }            
        }

        public Vector3 AttachmentPointPosition(string slotName)
        {
            Matrix4 transform = AttachmentPointTransform(slotName);
            Vector3 position, scale;
            Quaternion orientation;
            Matrix4.DecomposeMatrix(ref transform, out position, out orientation, out scale);
            return position;
        }

        public Quaternion AttachmentPointOrientation(string slotName)
        {
            Matrix4 transform = AttachmentPointTransform(slotName);
            Vector3 position, scale;
            Quaternion orientation;
            Matrix4.DecomposeMatrix(ref transform, out position, out orientation, out scale);
            return orientation;
        }

        // Used for attaching internal attachments
        public Node AttachScriptObject(string slotName, MovableObject sceneObj, Quaternion orientation, Vector3 offset)
        {
            long objectId = worldManager.GetLocalOid();
            scriptAttachments[sceneObj] = objectId;
            return AttachObject(slotName, objectId, sceneObj, orientation, offset);
        }

        // Used for attaching internal attachments
        public Node AttachScriptNode(string slotName)
        {
            long objectId = worldManager.GetLocalOid();
            Node node = AttachNode(slotName, objectId);
            scriptNodeAttachments[node] = objectId;

            return node;
        }

        public Node AttachNode(string slotName, long objectId)
        {
            log.InfoFormat("Attaching to {0} on {1}", slotName, this.Name);
            AttachmentPoint socket = null;
            if (attachmentPoints.ContainsKey(slotName))
                socket = attachmentPoints[slotName];
            else
                socket = new AttachmentPoint(slotName, null, Quaternion.Identity, Vector3.Zero);
            return AttachNode(socket, objectId);
        }

        // Used for attaching internal attachments
        public Node AttachLocalObject(string slotName, MovableObject sceneObj) {
            long objectId = worldManager.GetLocalOid();
            localAttachments[slotName] = objectId;
            return AttachObject(slotName, objectId, sceneObj);
        }

        public Node AttachLocalObject(AttachmentPoint attachPoint, MovableObject sceneObj) {
            long objectId = worldManager.GetLocalOid();
            localAttachments[objectId.ToString()] = objectId;
            return AttachObject(attachPoint, objectId, sceneObj);
        }

        public Node AttachObject(string slotName, long objectId, MovableObject sceneObj)
        {
            return AttachObject(slotName, objectId, sceneObj, Quaternion.Identity, Vector3.Zero); 
        }

        public Node AttachObject(string slotName, long objectId, MovableObject sceneObj, Quaternion orientation, Vector3 offset) {
            log.InfoFormat("Attaching to {0} on {1}", slotName, this.Name);
            AttachmentPoint socket = null;
            if (attachmentPoints.ContainsKey(slotName))
                socket = attachmentPoints[slotName];
            else
                socket = new AttachmentPoint(slotName, null, Quaternion.Identity, Vector3.Zero);
            return AttachObject(socket, objectId, sceneObj, orientation, offset);
        }

        public Node AttachObject(AttachmentPoint attachPoint, long objectId, MovableObject sceneObj)
        {
            return AttachObject(attachPoint, objectId, sceneObj, Quaternion.Identity, Vector3.Zero);
        }

        public Node AttachObject(AttachmentPoint attachPoint, long objectId,  MovableObject sceneObj, Quaternion orientation, Vector3 offset) {
            Node attachNode;
            Quaternion derivedOrientation = attachPoint.Orientation * orientation;
            Vector3 derivedOffset = (attachPoint.Orientation * offset) + attachPoint.Position;
            if (attachPoint.ParentBone != null) {
                attachNode = this.Entity.AttachObjectToBone(attachPoint.ParentBone, sceneObj, derivedOrientation, derivedOffset);
            } else {
                string attachNodeName = string.Format("attachment.{0}.{1}", attachPoint.Name, objectId);
                SceneNode attachedSceneNode = sceneNode.Creator.CreateSceneNode(attachNodeName);
                attachedSceneNode.Orientation = derivedOrientation;
                attachedSceneNode.Position = derivedOffset;
                sceneNode.AddChild(attachedSceneNode);
                attachedSceneNode.AttachObject(sceneObj);
                attachNode = attachedSceneNode;
            }
            attachments[objectId] = attachNode;
            attachedObjects[objectId] = sceneObj;
            return attachNode;
		}


        /// <summary>
        ///   Creates a node for use in future attachments (based on the attachPoint structure)
        /// </summary>
        /// <param name="attachPoint"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public Node AttachNode(AttachmentPoint attachPoint, long objectId) {
            Node attachNode;
            if (attachPoint.ParentBone != null) {
                attachNode = this.Entity.AttachNodeToBone(attachPoint.ParentBone, attachPoint.Orientation, 
														  attachPoint.Position);
            } else {
                SceneNode attachedSceneNode = sceneNode.Creator.CreateSceneNode();
                attachedSceneNode.Orientation = attachPoint.Orientation;
                attachedSceneNode.Position = attachPoint.Position;
                sceneNode.AddChild(attachedSceneNode);
                attachNode = attachedSceneNode;
            }
            attachments[objectId] = attachNode;
            // attachedObjects[objectId] = sceneObj;
            return attachNode;
        }



        public MovableObject DetachLocalObject(string slotName) {
            if (!localAttachments.ContainsKey(slotName)) {
                log.WarnFormat("Got DetachLocalObject without local object at {0}", slotName);
                return null;
            }
            long objectId = localAttachments[slotName];
            MovableObject rv = DetachObject(objectId);
            localAttachments.Remove(slotName);
            return rv;
        }

        public void DetachScriptObject(MovableObject attachObj)
        {
            if (!scriptAttachments.ContainsKey(attachObj))
            {
                log.Warn("Got DetachScriptObject without valid object");
                return;
            }
            long objectId = scriptAttachments[attachObj];
            DetachObject(objectId);
            scriptAttachments.Remove(attachObj);
            return;
        }

        public void DetachScriptNode(Node node)
        {
            if (!scriptNodeAttachments.ContainsKey(node))
            {
                log.Warn("Got DetachScriptNode without valid node");
                return;
            }
            long objectId = scriptNodeAttachments[node];
            DetachNode(objectId, true);
            scriptNodeAttachments.Remove(node);
            return;
        }

        public MovableObject DetachObject(long objectId) {
            return DetachObject(objectId, true);
        }

        /// <summary>
        ///   Remove an attached object.  The object may be attached to a TagPoint
        ///   or it may be attached to a SceneNode.  If it is attached to a TagPoint,
        ///   we will remove the TagPoint as well, since TagPoint objects can only
        ///   have one child.  If it is attached to a SceneNode, we will leave the
        ///   SceneNode, since SceneNode objects can have multiple children.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="removeFromDict">flag to indicate that the entry should be removed from the attachment dictionary</param>
        /// <returns>the MovableObject that was attached or null if none was found</returns>
        private MovableObject DetachObject(long objectId, bool removeFromDict) {
            if (!attachments.ContainsKey(objectId)) {
                log.WarnFormat("No match for attached object: {0}", objectId);
                return null;
            }
            if (!attachedObjects.ContainsKey(objectId))
            {
                log.WarnFormat("No match for attached object: {0}", objectId);
                return null;
            }
            Node attachNode = attachments[objectId];
            MovableObject obj = attachedObjects[objectId];
            if (attachNode is TagPoint) {
                // TagPoint objects will be attached to the skeleton
                TagPoint tagPoint = (TagPoint)attachNode;
                // this will remove the tagpoint as well
                this.Entity.DetachObjectFromBone(tagPoint);
            } else if (attachNode is SceneNode) {
                SceneNode attachedSceneNode = (SceneNode)attachNode;
                attachedSceneNode.DetachObject(obj);
                sceneNode.RemoveChild(attachedSceneNode);
                // we need to remove the scene node from the scene as well
                try {
                    sceneNode.Creator.DestroySceneNode(attachedSceneNode.Name);
                } catch (Exception e) {
                    LogUtil.ExceptionLog.WarnFormat("Caught exception in DestroySceneNode: {0}", e);
                    // Ignore this.. we don't want to throw from Dispose
                }
            } else {
                Debug.Assert(false, "Invalid object type for attachNode");
            }

            if (removeFromDict) {
                attachments.Remove(objectId);
                attachedObjects.Remove(objectId);
            }

            return obj;
        }

		/// <summary>
		///  Detach the node with oid objectID from the object node
		/// </summary>
        public void DetachNode(long objectId, bool removeFromDict) {
            if (!attachments.ContainsKey(objectId)) {
                log.WarnFormat("No match for attached object: {0}", objectId);
                return;
            }
            Node attachNode = attachments[objectId];
            if (attachNode is TagPoint) {
                // TagPoint objects will be attached to the skeleton
                TagPoint tagPoint = (TagPoint)attachNode;
                // this will remove the tagpoint as well
                this.Entity.DetachNodeFromBone(tagPoint.Parent.Name, tagPoint);
            } else if (attachNode is SceneNode) {
                SceneNode attachedSceneNode = (SceneNode)attachNode;
                sceneNode.RemoveChild(attachedSceneNode);
            } else {
                Debug.Assert(false, "Invalid object type for attachNode");
            }

            if (removeFromDict) {
                attachments.Remove(objectId);
            }
        }

		/// <summary>
		///  Clear the current set of sounds
		/// </summary>
		public void ClearSounds() {
            foreach (SoundSource source in soundSources)
                SoundManager.Instance.Release(source);
            soundSources.Clear();
		}

        protected void SoundDoneHandler(object sender, EventArgs args)
        {
            SoundSource source = sender as SoundSource;
            if (source != null) 
                DetachSound(source);
        }

        public void AttachSound(SoundSource source) {
            // Unlike animations, we will play all the sounds at the same time
            soundSources.Add(source);
            source.SoundDone += new SoundDoneEvent(SoundDoneHandler);
        }

        public void DetachSound(SoundSource source) {
            soundSources.Remove(source);
            SoundManager.Instance.Release(source);
        }

        /// <summary>
        /// Set multiple properties at once.
        /// We set all the properties first, and then raise the PropertyChange() events
        ///  so that if multiple properties change in once message, they appear atomic
        ///  to the scripts.
        /// </summary>
        /// <param name="newProperties"></param>
        public void UpdateProperties(Dictionary<string, object> newProperties)
        {
            foreach (string prop in newProperties.Keys)
            {
                // set the property 
                properties[prop] = newProperties[prop];
                // Special case some properties
                switch (prop)
                {
                    case "follow_terrain":
                        this.FollowTerrain = CheckBooleanProperty(prop);
                        break;
                }
            }

            foreach (string prop in newProperties.Keys)
            {
                OnPropertyChange(prop);
            }
        }

        public bool CheckBooleanProperty(string propName)
        {
            if (PropertyExists(propName))
            {
                object val = GetProperty(propName);
                if (val is bool)
                {
                    return (bool)val;
                }
            }

            return false;
        }

        public bool PropertyExists(string propName)
        {
            return properties.ContainsKey(propName);
        }

        public object GetProperty(string propName)
        {
            return properties[propName];
        }

        public void SetProperty(string propName, object value)
        {
            properties[propName] = value;
            OnPropertyChange(propName);
        }

        public List<string> PropertyNames
        {
            get
            {
                return new List<string>(properties.Keys);
            }
        }

        protected virtual void OnPropertyChange(string propName)
        {
            PropertyChangeEventHandler handler = PropertyChange;

            if (handler != null)
            {
                handler(this, new PropertyChangeEventArgs(propName));
            }

            // invoke any global events on the property change
            worldManager.OnObjectPropertyChange(propName, this);
        }

#if OLD_ATTACHMENTS
		private void SetupShieldAttachment() {
			// TODO - Hack to set up fake attachment points;
			AttachmentPoint socket = new AttachmentPoint();

			switch (this.Entity.Mesh.Skeleton.Name) {
				case "human_male.skeleton":
					socket.boneName = "LeftForeArm";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(220, 50, 0);
					break;
                case "human_female.skeleton":
                    socket.boneName = "L_Wrist_BIND_jjj";
                    socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
                        * Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                    socket.offsetPosition = new Vector3(65, 20, 0);
                    break;
                default:
					Logger.Log(0, "EntityName: {0}", this.Entity.Mesh.Skeleton.Name);
					socket.boneName = "Left_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 35, 0);
					break;
			}

			if (!this.Entity.Mesh.Skeleton.ContainsBone(socket.boneName))
				return;

			attachmentPoints["shield"] = socket;
		}

		private void SetupSecondaryWeaponAttachment() {
			// TODO - Hack to set up fake attachment points;
			AttachmentPoint socket = new AttachmentPoint();

			switch (this.Entity.Mesh.Skeleton.Name) {
				case "orc.skeleton":
					socket.boneName = "Left_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(170, 60, 0);
					break;
				case "girl.skeleton":
					socket.boneName = "Left_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 20, 0);
					break;
				case "hero.skeleton":
					socket.boneName = "Left_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 35, 0);
					break;
				case "human_male.skeleton":
					socket.boneName = "LeftHand";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(75, -20, 0);
					break;
                case "human_female.skeleton":
                    socket.boneName = "L_Wrist_BIND_jjj";
                    socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
                        * Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                    socket.offsetPosition = new Vector3(65, 20, 0);
                    break;
                default:
					Logger.Log(2, "EntityName: {0}", this.Entity.Mesh.Skeleton.Name);
					socket.boneName = "Left_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 35, 0);
					break;
			}

			if (!this.Entity.Mesh.Skeleton.ContainsBone(socket.boneName))
				return;

			attachmentPoints["secondaryWeapon"] = socket;
		}

		private void SetupPrimaryWeaponAttachment() {
			// TODO - Hack to set up fake attachment points;
			AttachmentPoint socket = new AttachmentPoint();

			switch (this.Entity.Mesh.Skeleton.Name) {
				case "orc.skeleton":
					socket.boneName = "Right_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(170, 60, 0);
					break;
				case "girl.skeleton":
					socket.boneName = "Right_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 20, 0);
					break;
				case "hero.skeleton":
					socket.boneName = "Right_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 35, 0);
					break;
                case "human_male.skeleton":
					socket.boneName = "RightHand";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1)) 
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(-75, -20, 0);
					break;
                case "human_female.skeleton":
                    socket.boneName = "R_Wrist_BIND_jjj";
                    socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                    socket.offsetPosition = new Vector3(65, 20, 0);
                    break;
                default:
					Logger.Log(0, "EntityName: {0}", this.Entity.Mesh.Skeleton.Name);
					socket.boneName = "Right_Wrist_BIND_jjj";
					socket.offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
					socket.offsetPosition = new Vector3(65, 35, 0);
					break;
			}

			if (!this.Entity.Mesh.Skeleton.ContainsBone(socket.boneName))
				return;

			attachmentPoints["primaryWeapon"] = socket;
		}

		private void SetupNameAttachment() {
			AttachmentPoint socket = new AttachmentPoint();

			switch (this.Entity.Mesh.Skeleton.Name) {
				case "human_male.skeleton":
					socket.boneName = "Head";
					break;
                case "human_female.skeleton":
                default:
					socket.boneName = "Head_BIND_jjj";
					break;
			}

			if (!this.Entity.Mesh.Skeleton.ContainsBone(socket.boneName))
				return;

			attachmentPoints["name"] = socket;
		}

		private void SetupBubbleAttachment() {
			AttachmentPoint socket = new AttachmentPoint();

			switch (this.Entity.Mesh.Skeleton.Name) {
				case "human_male.skeleton":
					socket.boneName = "Head";
					break;
                case "human_female.skeleton":
                default:
					socket.boneName = "Head_BIND_jjj";
					break;
			}

			if (!this.Entity.Mesh.Skeleton.ContainsBone(socket.boneName))
				return;

			attachmentPoints["bubble"] = socket;
		}

		private void SetupQuestAvailableAttachment() {
			AttachmentPoint socket = new AttachmentPoint();

			switch (this.Entity.Mesh.Skeleton.Name) {
				case "human_male.skeleton":
					socket.offsetPosition = new Vector3(0, 350, 0);
					socket.boneName = "Head";
					break;
                case "human_female.skeleton":
                    socket.offsetOrientation = Quaternion.FromAngleAxis(-1 * (float)Math.PI / 2, Vector3.UnitZ);
                    socket.offsetPosition = new Vector3(300, 0, 0);
                    socket.boneName = "Head_BIND_jjj";
                    break;
                default:
					socket.offsetOrientation = Quaternion.FromAngleAxis(-1 * (float)Math.PI / 2, Vector3.UnitZ);
					socket.offsetPosition = new Vector3(500, 0, 0);
					socket.boneName = "Head_BIND_jjj";
					break;
			}

			if (!this.Entity.Mesh.Skeleton.ContainsBone(socket.boneName))
				return;

			attachmentPoints["questavailable"] = socket;
		}
#endif

        private void SetupAttachmentPoints() {
            if (this.Entity.Skeleton != null) {
                foreach (AttachmentPoint ap in this.Entity.Skeleton.AttachmentPoints)
                    attachmentPoints[ap.Name] = ap;
            }
            if (this.Entity.Mesh != null) {
                foreach (AttachmentPoint ap in this.Entity.Mesh.AttachmentPoints)
                    attachmentPoints[ap.Name] = ap;
            }
            if (log.IsDebugEnabled) {
                foreach (AttachmentPoint ap in attachmentPoints.Values)
                    log.DebugFormat("Set up attachment point: {0}", ap.Name);
            }
		}

		private void ClearAttachmentPoints() {
			attachmentPoints.Clear();
		}

        protected void OnDisposed()
        {
            ObjectNodeDisposed handler = Disposed;

            if (handler != null)
            {
                handler(this);
            }
        }

		public override void Dispose() {
            OnDisposed();

			// Clear animations
			base.Dispose();

			if (collider != null)
				collider.Dispose();
			
			// Clear attachments
			ClearAttachments();

			// Clear sounds
			ClearSounds();

			if (sceneNode == null) {
				log.WarnFormat("SceneNode is null in Dispose for {0}", oid);
				return;
			}

			Debug.Assert(sceneNode.ChildCount == 0, "Invalid removal of object with children");
			log.InfoFormat("Detaching {0} objects", sceneNode.ObjectCount);
			sceneNode.DetachAllObjects();
            if (entity != null)
			    sceneNode.Creator.RemoveEntity(this.Entity);
			// this removes the sceneNode from the parent as well
            try {
                sceneNode.Creator.DestroySceneNode("object." + oid);
            } catch (Exception e) {
                LogUtil.ExceptionLog.WarnFormat("Caught exception in DestroySceneNode: {0}", e);
                // Ignore this.. we don't want to throw from Dispose
            }
			// Clear the entity and sceneNode
			entity = null;
			sceneNode = null;
		}

		public void Glow(bool glow) {
			if (glow == glowing)
				return;
			for (int i = 0; i < entity.SubEntityCount; ++i) {
				SubEntity subEntity = entity.GetSubEntity(i);
				if (glow) {
					string glowMaterial = subEntity.MaterialName + ".glow";
					if (MaterialManager.Instance.GetByName(glowMaterial) != null)
						subEntity.MaterialName = glowMaterial;
				} else {
					if (subEntity.MaterialName.EndsWith(".glow")) {
						string glowMaterial = subEntity.MaterialName;
						subEntity.MaterialName = glowMaterial.Substring(0, glowMaterial.Length - 5);
					}
				}
			}
			glowing = glow;
		}

        public AttachmentPoint GetAttachmentPoint(string name) {
            if (attachmentPoints.ContainsKey(name))
                return attachmentPoints[name];
            return null;
        }

		#region Properties

		public SceneNode SceneNode {
			get {
				return sceneNode;
			}
            set {
                sceneNode = value;
            }
		}

		public Entity Entity {
			get {
				return entity;
			}
            set {
                // Should this call ClearAnimations?
                Monitor.Enter(this);
                try {
                    if (entity != null) {
                        sceneNode.DetachObject(entity);
                        entity = null;
                    }
					if (value != null)
						sceneNode.AttachObject(value);
                    entity = value;
					if (entity != null)
						entity.UserData = this;
					// Determine if we should make the object upright
					if (entity != null) {
						Vector3 obbDims = entity.BoundingBox.Maximum - entity.BoundingBox.Minimum;
						if (obbDims.y > obbDims.x && obbDims.y > obbDims.z)
							isUpright = true;
						else
							isUpright = false;
					}
					if (entity != null)
						SetupAttachmentPoints();
				} finally {
                    Monitor.Exit(this);
                }
            }
        }

		/// <summary>
		///   Set and get the object's target position
        /// </summary>
        /// <value></value>
        public virtual Vector3 Position {
            get {
                return sceneNode.Position;
            }
            set {
                if (value != sceneNode.Position)
                {
                    sceneNode.Position = value;
                    OnPositionChange();
                }
			}
        }

        public MovingObject Collider
        {
            get
            {
                return collider;
            }
            set
            {
                collider = value;
            }
        }

        public List<CollisionShape> CollisionShapes
        {
            get
            {
                return collisionShapes;
            }
            set
            {
                collisionShapes = value;
            }
        }

		public List<SoundSource> SoundSources {
			get {
				return soundSources;
			}
		}

		public virtual Quaternion Orientation {
            get {
                return sceneNode.Orientation;
            }
            set {
                if (sceneNode.Orientation != value)
                {
                    sceneNode.Orientation = value;
                    if (collider != null)
                    {
                        collider.Transform(Vector3.UnitScale, sceneNode.DerivedOrientation, sceneNode.DerivedPosition);
                    }

                    OnOrientationChange();
                }
            }
        }

		public string Name {
			get {
				return name;
			}
		}

        public string EntityName {
            get {
                return entityName;
            }
			set {
				entityName = value;
			}
        }

        public ObjectNodeType ObjectType {
			get {
				return objType;
			}
		}

        public bool Targetable
        {
            get
            {
                return targetable;
            }
            set
            {
                targetable = value;
            }
        }

		public bool FollowTerrain {
			get {
				return followTerrain;
			}
			set {
				followTerrain = value;
			}
		}

		public bool IsUpright {
			get {
				return isUpright;
			}
			set {
				isUpright = value;
			}
		}

        public Dictionary<long, Node> Attachments {
            get {
                return attachments;
            }
        }

        public Dictionary<string, AttachmentPoint> AttachmentPoints {
            get {
                return attachmentPoints;
            }
        }

        public int PerceptionRadius {
            get {
                return perceptionRadius;
            }
        }
        
        public bool InStaticGeometry
        {
            get
            {
                return inStaticGeometry;
            }
            set
            {
                inStaticGeometry = value;
            }
        }

        public virtual bool UseCollisionObject {
            get {
                return useCollisionObject;
            }
            set {
                if (useCollisionObject != value) {
                    useCollisionObject = value;
                    if (useCollisionObject)
                        MarsWorld.Instance.AddCollisionObject(this);
                    else
                        MarsWorld.Instance.RemoveCollisionObject(this);
                }
            }
        }

        #endregion
    }
}
