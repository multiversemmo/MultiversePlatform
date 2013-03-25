#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Axiom.Collections;
using Axiom.Core;
using Axiom.Graphics;


namespace Axiom.Animating {
    /// <summary>
    ///		An animation sequence. 
    /// </summary>
    /// <remarks>
    ///		This class defines the interface for a sequence of animation, whether that
    ///		be animation of a mesh, a path along a spline, or possibly more than one
    ///		type of animation in one. An animation is made up of many 'tracks', which are
    ///		the more specific types of animation.
    ///		<p/>
    ///		You should not create these animations directly. They will be created via a parent
    ///		object which owns the animation, e.g. Skeleton, SceneManager, etc.
    /// </remarks>
    public class Animation {
        #region Member variables
		
        /// <summary>Name of this animation.</summary>
        protected string name;
        /// <summary>The total length of this animation (sum of the tracks).</summary>
        public float length;
        /// <summary>Collection of NodeAnimationTracks.</summary>
		protected Dictionary<ushort, NodeAnimationTrack> nodeTrackList;
        /// <summary>Collection of NumericAnimationTracks.</summary>
        protected Dictionary<ushort, NumericAnimationTrack> numericTrackList;
        /// <summary>Collection of VertexAnimationTracks.</summary>
        protected Dictionary<ushort, VertexAnimationTrack> vertexTrackList;
        /// <summary>Interpolation mode of this animation.</summary>
        public InterpolationMode interpolationMode;
        /// <summary>Rotation interpolation mode of this animation.</summary>
        public RotationInterpolationMode rotationInterpolationMode;
        /// <summary>Default interpolation mode of any animations.</summary>
        static protected InterpolationMode defaultInterpolationMode;
        /// <summary>default rotation interpolation mode of this animation.</summary>
        static protected RotationInterpolationMode defaultRotationInterpolationMode;

        #endregion

        #region Constructors

        /// <summary>Static constructor.</summary>
        static Animation() {
            // set default interpolation mode to Linear
            defaultInterpolationMode = InterpolationMode.Linear;
			defaultRotationInterpolationMode = RotationInterpolationMode.Linear;
        }

        /// <summary>
        ///		Internal constructor, to prevent from using new outside of the engine.
        ///		<p/>
        ///		Animations should be created within objects that can own them (skeletons, scene managers, etc).
        /// </summary>
        internal Animation(string name, float length) {
            this.name = name;
            this.length = length;

            // use the default interpolation modes
            this.interpolationMode = Animation.DefaultInterpolationMode;
            this.rotationInterpolationMode = Animation.DefaultRotationInterpolationMode;
			
			// Create the track lists
			this.nodeTrackList = new Dictionary<ushort, NodeAnimationTrack>();
			this.numericTrackList = new Dictionary<ushort, NumericAnimationTrack>();
			this.vertexTrackList = new Dictionary<ushort, VertexAnimationTrack>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Gets the name of this animation.
        /// </summary>
        public string Name {
            get { return name; }
        }

        /// <summary>
        ///		Gets the total length of this animation.
        /// </summary>
        public float Length {
            get { return length; }
        }

        /// <summary>
        ///		Gets/Sets the current interpolation mode for this animation.
        /// </summary>
        public InterpolationMode InterpolationMode {
            get { return interpolationMode; }
            set { interpolationMode = value; }
        }

        /// <summary>
        ///		Gets/Sets the current interpolation mode for this animation.
        /// </summary>
        public RotationInterpolationMode RotationInterpolationMode {
            get { return rotationInterpolationMode; }
            set { rotationInterpolationMode = value; }
        }

        /// <summary>
        ///		A collection of the node tracks in this animation.
        /// </summary>
        // TODO: See if we can ensure that the track list is not modified somehow.
        public Dictionary<ushort, NodeAnimationTrack> NodeTracks {
            get { return nodeTrackList; }
        }

        /// <summary>
        ///		A collection of the numeric tracks in this animation.
        /// </summary>
        // TODO: See if we can ensure that the track list is not modified somehow.
        public Dictionary<ushort, NumericAnimationTrack> NumericTracks {
            get { return numericTrackList; }
        }

        /// <summary>
        ///		A collection of the vertex tracks in this animation.
        /// </summary>
        // TODO: See if we can ensure that the track list is not modified somehow.
        public Dictionary<ushort, VertexAnimationTrack> VertexTracks {
            get { return vertexTrackList; }
        }

        /// <summary>
        ///		Gets/Sets the default interpolation mode to be used for all animations.
        /// </summary>
        public static InterpolationMode DefaultInterpolationMode {
            get { return defaultInterpolationMode; }
            set { defaultInterpolationMode = value; }
        }

        /// <summary>
        ///		Gets/Sets the default interpolation mode to be used for all animations.
        /// </summary>
        public static RotationInterpolationMode DefaultRotationInterpolationMode {
            get { return defaultRotationInterpolationMode; }
            set { defaultRotationInterpolationMode = value; }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///		Creates an NodeAnimationTrack. 
        /// </summary>
        /// <param name="handle">Handle to give the track, used for accessing the track later.</param>
        /// <returns></returns>
        public NodeAnimationTrack CreateNodeTrack(ushort handle) {
            NodeAnimationTrack track = new NodeAnimationTrack(this, handle);

            // add the track to the list
            nodeTrackList[handle] = track;

            return track;
        }

        /// <summary>
        ///		Creates a new NodeAnimationTrack automatically associated with a Node. 
        /// </summary>
        /// <param name="index">Handle to give the track, used for accessing the track later.</param>
        /// <param name="targetNode">Node object which will be affected by this track.</param>
        /// <returns></returns>
        public NodeAnimationTrack CreateNodeTrack(ushort handle, Node targetNode) {
            // create a new track and set it's target
            NodeAnimationTrack track = CreateNodeTrack(handle);
            track.TargetNode = targetNode;

            return track;
        }

        /// <summary>
        ///		Creates an NumericAnimationTrack. 
        /// </summary>
        /// <param name="handle">Handle to give the track, used for accessing the track later.</param>
        /// <returns></returns>
        public NumericAnimationTrack CreateNumericTrack(ushort handle) {
            NumericAnimationTrack track = new NumericAnimationTrack(this, handle);

            // add the track to the list
            numericTrackList[handle] = track;

            return track;
        }

        /// <summary>
        ///		Creates a new NumericAnimationTrack automatically associated with a Numeric. 
        /// </summary>
        /// <param name="index">Handle to give the track, used for accessing the track later.</param>
        /// <param name="animable">AnimableValue which will be affected by this track.</param>
        /// <returns></returns>
        public NumericAnimationTrack CreateNumericTrack(ushort handle, AnimableValue animable) {
            // create a new track and set it's target
            NumericAnimationTrack track = CreateNumericTrack(handle);
            track.TargetAnimable = animable;

            return track;
        }

        /// <summary>
        ///		Creates an VertexAnimationTrack. 
        /// </summary>
        /// <param name="handle">Handle to give the track, used for accessing the track later.</param>
        /// <returns></returns>
		public VertexAnimationTrack CreateVertexTrack(ushort handle, VertexAnimationType animType) {
            VertexAnimationTrack track = new VertexAnimationTrack(this, handle, animType);

            // add the track to the list
            vertexTrackList[handle] = track;

            return track;
        }

        /// <summary>
        ///		Creates a new VertexAnimationTrack automatically associated with a Vertex. 
        /// </summary>
        /// <param name="index">Handle to give the track, used for accessing the track later.</param>
        /// <param name="target">Vertex object which will be affected by this track.</param>
        /// <returns></returns>
		public VertexAnimationTrack CreateVertexTrack(ushort handle, VertexData targetVertexData,
													  VertexAnimationType type) {
            // create a new track and set it's target
			VertexAnimationTrack track = CreateVertexTrack(handle, type);
            track.TargetVertexData = targetVertexData;
			return track;
        }

        /// <summary>
        ///		Applies an animation given a specific time point and weight.
        /// </summary>
        /// <remarks>
        ///		Where you have associated animation tracks with Node objects, you can eaily apply
        ///		an animation to those nodes by calling this method.
        /// </remarks>
        /// <param name="time">The time position in the animation to apply.</param>
        /// <param name="weight">The influence to give to this track, 1.0 for full influence, less to blend with
        ///		other animations.</param>
        /// <param name="accumulate"></param>
		public void Apply(float time, float weight, bool accumulate, float scale) {
            // loop through tracks and update them all with current time
			foreach(NodeAnimationTrack nodeTrack in nodeTrackList.Values) {
				nodeTrack.Apply(time, weight, accumulate, scale);
			}
			foreach(NumericAnimationTrack numericTrack in numericTrackList.Values) {
				numericTrack.Apply(time, weight, accumulate, scale);
			}
			foreach(VertexAnimationTrack vertexTrack in vertexTrackList.Values) {
				vertexTrack.Apply(time, weight, accumulate, scale);
			}
        }

		public void Apply(Skeleton skeleton, float time, float weight, bool accumulate, float scale) {
			// loop through tracks and update them all with current time
			foreach(KeyValuePair<ushort, NodeAnimationTrack> pair in nodeTrackList) {
				NodeAnimationTrack track = pair.Value;
				Bone bone = skeleton.GetBone(pair.Key);
				track.ApplyToNode(bone, time, weight, accumulate, scale);
			}
		}

		public void Apply(Entity entity, float time, float weight, 
						  bool software, bool hardware) {
			foreach(KeyValuePair<ushort, VertexAnimationTrack> pair in vertexTrackList) {
				int handle = pair.Key;
				VertexAnimationTrack track = pair.Value;

				VertexData swVertexData;
				VertexData hwVertexData;
				VertexData origVertexData;
				bool firstAnim = false;
				if (handle == 0) {
					// shared vertex data
					firstAnim = !entity.BuffersMarkedForAnimation;
					swVertexData = entity.SoftwareVertexAnimVertexData;
					hwVertexData = entity.HardwareVertexAnimVertexData;
					origVertexData = entity.Mesh.SharedVertexData;
					entity.MarkBuffersUsedForAnimation();
				}
				else {
					// sub entity vertex data (-1)
					SubEntity s = entity.GetSubEntity(handle - 1);
					firstAnim = !s.BuffersMarkedForAnimation;
					swVertexData = s.SoftwareVertexAnimVertexData;
					hwVertexData = s.HardwareVertexAnimVertexData;
					origVertexData = s.SubMesh.vertexData;
					s.MarkBuffersUsedForAnimation();
				}
				// Apply to both hardware and software, if requested
				if (software) {
					Debug.Assert(!EqualityComparer<VertexData>.ReferenceEquals(origVertexData, swVertexData));
					if (firstAnim && track.AnimationType == VertexAnimationType.Pose) {
						// First time through for a piece of pose animated vertex data
						// We need to copy the original position values to the temp accumulator
						VertexElement origelem = 
							origVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
						VertexElement destelem = 
							swVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
						HardwareVertexBuffer origBuffer = 
							origVertexData.vertexBufferBinding.GetBuffer(origelem.Source);
						HardwareVertexBuffer destBuffer = 
							swVertexData.vertexBufferBinding.GetBuffer(destelem.Source);
// 						Debug.Assert(!EqualityComparer<HardwareVertexBuffer>.ReferenceEquals(origBuffer, destBuffer));
						if (!EqualityComparer<HardwareVertexBuffer>.ReferenceEquals(origBuffer, destBuffer))
							destBuffer.CopyData(origBuffer, 0, 0, destBuffer.Size, true);
					}
					track.TargetMode = VertexAnimationTargetMode.Software;
					track.ApplyToVertexData(swVertexData, time, weight, entity.Mesh.PoseList);
				}
				if (hardware) {
					track.TargetMode = VertexAnimationTargetMode.Hardware;
					track.ApplyToVertexData(hwVertexData, time, weight, entity.Mesh.PoseList);
				}
			}
		}

        #endregion

        #region Event handlers

        private void TrackAdded(object source, System.EventArgs e) {
			
        }

        private void TracksCleared(object source, System.EventArgs e) {
            // clear the tangents list when the points are cleared
            //tangentList.Clear();
        }

        #endregion
    }
}
