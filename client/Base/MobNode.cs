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

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Animating;

using Multiverse.Config;
using Multiverse.Network;
using Multiverse.CollisionLib;

using TimeTool = Multiverse.Utility.TimeTool;

#endregion

namespace Multiverse.Base
{
    public delegate void DirectionChangeEventHandler(object sender, EventArgs args);

    /// <summary>
    ///   MobNodes are variants of ObjectNode that are mobile, and have 
    ///   interpolated movement.
    /// </summary>
    public class MobNode : ObjectNode
    {
		protected long lastDirTimestamp = -1;

		protected Vector3 lastPosition;
		protected Vector3 lastDirection;

		protected PathInterpolator pathInterpolator = null;
        
        /// <summary>
        ///   True if the system should call FMOD to notify about
        ///   position and velocity updates.  Velocity only matters
        ///   for doppler.
        /// </summary>
        protected bool positionalSoundEmitter = false;
        
        /// <summary>
        ///   Adjust the position gradually if it is less than MaxAdjustment, 
        ///   or just set it directly if it is more than MaxAdjustment
        /// </summary>
        const float MaxAdjustment = 3 * Client.OneMeter;
        /// <summary>
        ///   Converge on the destination at one meter per second
        /// </summary>
        const float AdjustVelocity = Client.OneMeter;

        public event DirectionChangeEventHandler DirectionChange;

        public MobNode(long oid, string name, SceneNode sceneNode, WorldManager worldManager) : 
			base(oid, name, sceneNode, ObjectNodeType.Npc, worldManager) 
		{
            this.Targetable = true;
		}

		/// <summary>
        ///   Update the node
        /// </summary>
        /// <param name="timeSinceLastFrame">time since last frame (in seconds)</param>
        public override void Tick(float timeSinceLastFrame) {
			long now = TimeTool.CurrentTime;
			// Set position based on direction vectors.
			this.Position = ComputePosition(now);
			base.Tick(timeSinceLastFrame);
#if DISABLED
            if (position != sceneNode.Position) {
                Vector3 posDelta = position - sceneNode.Position;
                float adjustDistance = timeSinceLastFrame * AdjustVelocity;
				if ((posDelta.Length > MaxAdjustment) ||
					(posDelta.Length <= adjustDistance))
					// If they are too far away, teleport them there
					// If they are close enough that we can adjust them there, do that
					sceneNode.Position = position;
				else {
					posDelta.Normalize();
					sceneNode.Position += adjustDistance * posDelta;
				}
			}
#endif
		}

		public override void SetDirLoc(long timestamp, Vector3 dir, Vector3 pos) {
			log.DebugFormat("SetDirLoc: dir for node {0} = {1} timestamp = {2}/{3}", this.Oid, dir, timestamp, TimeTool.CurrentTime);
			if (timestamp <= lastDirTimestamp) {
				log.DebugFormat("Ignoring dirloc,since timestamps are too close {0}, {1}", timestamp, lastDirTimestamp);
				return;
			}
			// timestamp is newer.
			lastDirTimestamp = timestamp;
			lastPosition = pos;
			LastDirection = dir;
			SetLoc(timestamp, pos);
		}

		// Provide a way to return to the old behavior in which mobs
		// are supported by collision volumes.  Later, when we have
		// full mob pathing support, we can turn this off again.
        public static bool useMoveMobNodeForPathInterpolator = true;
        
        /// <summary>
		///   Compute the current position based on the time and updates.
		/// </summary>
		/// <param name="timestamp">local time in ticks (milliseconds)</param>
		/// <returns></returns>
		public virtual Vector3 ComputePosition(long timestamp) {
            long timeDifference = (timestamp - lastLocTimestamp);
            if (pathInterpolator != null)
            {
                PathLocAndDir locAndDir = pathInterpolator.Interpolate(timestamp);
                log.DebugFormat("MobNode.ComputePosition: oid {0}, followTerrain {1}, pathInterpolator {2}",
                                oid, followTerrain, locAndDir == null ? "null" : locAndDir.ToString());
                if (locAndDir != null)
                {
                    if (locAndDir.LengthLeft != 0f)
                        SetOrientation(Vector3.UnitZ.GetRotationTo(LastDirection.ToNormalized()));
                    LastDirection = locAndDir.Direction;
                    lastDirTimestamp = timestamp;
                    lastLocTimestamp = timestamp;
                    Vector3 loc = locAndDir.Location;
                    // If we don't have full pathing support, use
                    // MoveMobNode so the mob is supported by
                    // collision volumes.
                    if (useMoveMobNodeForPathInterpolator && collider != null && collider.PartCount > 0)
                    {
                        Vector3 diff = loc - Position;
                        diff.y = 0;
                        Vector3 newpos = worldManager.MoveMobNode(this, diff, timeDifference);
                        lastPosition = newpos;
                        return newpos;
                    }
                    else
                    {
                        loc = worldManager.ResolveLocation(this, loc);
                        if (collider != null)
                        {
                            Vector3 diff = loc - lastPosition;
                            collider.AddDisplacement(diff);
                        }
                        lastPosition = loc;
                        return loc;
                    }
                }
                else
                {
                    // This interpolator has expired, so get rid of it
                    pathInterpolator = null;
                    LastDirection = Vector3.Zero;
                }
            }
            else
            {
                lastLocTimestamp = timestamp;
            }
            Vector3 pos = lastPosition + ((timeDifference / 1000.0f) * LastDirection);
            Vector3 displacement = pos - Position;
            // Move the mob node - - given pathing, this should
            // _only_ happen for mobnodes that are other clients'
            // players.
            pos = worldManager.MoveMobNode(this, displacement, timeDifference);
            lastPosition = pos;
            return pos;
		}

        protected virtual void OnDirectionChange()
        {
            DirectionChangeEventHandler handler = DirectionChange;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
		
        #region Properties

        /// <summary>
        /// This property is the movement direction vector last sent by the server.  It is used to interpolate
        /// the mob's position.
        /// 
        /// The Direction property on the Player class represents the direction as set by the player.
        /// </summary>
        public Vector3 LastDirection {
            get {
                return lastDirection;
            }
            set {
                if (value != lastDirection)
                {
                    lastDirection = value;
                    OnDirectionChange();
                }
            }
        }

        /// <summary>
        /// This property represents the direction vector that comes from the player, and is used
        /// to interpolate the position of the player's avatar.
        /// 
        /// MobNode's LastDirection property is the last direction provided by the server.
        /// </summary>
        public virtual Vector3 Direction
        {
            get
            {
                return LastDirection;
            }
            set
            {
                lastDirTimestamp = TimeTool.CurrentTime;
                lastPosition = this.Position;
                LastDirection = value;
            }
        }

        public PathInterpolator Interpolator {
            get {
                return pathInterpolator;
            }
            set {
                pathInterpolator = value;
            }
        }

        public virtual bool PositionalSoundEmitter {
            get {
                return positionalSoundEmitter;
            }
            set {
                positionalSoundEmitter = value;
            }
        }

        #endregion
    }
}
