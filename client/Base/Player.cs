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

using log4net;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Animating;

using Multiverse.Config;

using TimeTool = Multiverse.Utility.TimeTool;

#endregion

namespace Multiverse.Base
{
	/// <summary>
	///   This class actually represents the player's active character.
	/// </summary>
    public class Player : MobNode
    {
		public DirUpdate dirUpdate;
		public OrientationUpdate orientUpdate;

		public long lastDirSent;
		public long lastOrientSent;

        // Our version of direction
        Vector3 direction;

		// Position = dirUpdate.position + (now - dirUpdate.timestamp) * dirUpdate.direction + posDelta;
		// posDelta = 

//        public Player(int playerId, WorldManager worldManager) {
//            this.playerId = playerId;
//            Console.WriteLine("Creating player object");
//            worldManager.AddMob(playerId, "Player", "foo.mesh", "fake.texture");
//            Console.WriteLine("Created player object");
//
//            entity = worldManager.GetEntity(playerId); 
//            
//            worldManager.SetPosition(playerId, Vector3.Zero);
//            modelNode = worldManager.GetSceneNode(playerId);
//
//        }

		public struct DirUpdate 
		{
			public bool dirty;
			public long timestamp; // in local time
			public Vector3 direction;
			public Vector3 position;
		}

		public struct OrientationUpdate
		{
			public bool dirty;
			public Quaternion orientation;
		}

		public Player(long oid, string name, SceneNode sceneNode, WorldManager worldManager) :
			base(oid, name, sceneNode, worldManager)
		{
            // force object type to User for the player
            objType = Multiverse.Network.ObjectNodeType.User;

			dirUpdate.dirty = false;
			orientUpdate.dirty = false;
            this.Targetable = false;
		}

		/// <summary>
        ///		Rotates the camera about an arbitrary axis.
        /// </summary>
        /// <param name="quat"></param>
		public void SetDirection(Vector3 dir, Vector3 pos, long now) {
			if (dir != Direction) {
				log.DebugFormat("In Player.SetDirection, marking direction dirty: pos {0}: dir {1}", pos, dir);
				dirUpdate.timestamp = now;
				dirUpdate.direction = dir;
				dirUpdate.position = pos;
				dirUpdate.dirty = true;
			}
			direction = dir;
            OnDirectionChange();
		}
		
		/// <summary>
		///   Compute the current position based on the time and updates.
		/// </summary>
		/// <param name="timestamp">local time in ticks (milliseconds)</param>
		/// <returns></returns>
		public override Vector3 ComputePosition(long timestamp) {
			Vector3 pos;
			long delta = 0;
            if (dirUpdate.timestamp - lastDirTimestamp > 0) {
				// we have a more up to date direction than what we got from
				// the server -- apply the server's idea of direction until 
				// we get to our timestamp, then apply our direction for the 
				// rest of the time.
				// Vector3 serverPos = base.ComputePosition(timestamp);
				delta = timestamp - dirUpdate.timestamp;
				pos = dirUpdate.position + ((delta / 1000.0f) * dirUpdate.direction);
			} else {
				delta = timestamp - lastLocTimestamp;
				pos = lastPosition + ((delta / 1000.0f) * LastDirection);
			}
			Vector3 displacement = pos - Position;
			// Move the player
			Position = worldManager.MoveMobNode(this, displacement, delta);
            dirUpdate.timestamp = timestamp;
            dirUpdate.position = Position;
            return Position;
		}

		public override Quaternion Orientation {
            get
            {
                return base.Orientation;
            }
            set
            {
                if (value != base.Orientation)
                {
                    orientUpdate.orientation = value;
                    orientUpdate.dirty = true;
                }
                base.Orientation = value;
            }
		}

        /// <summary>
        /// This property represents the direction vector that comes from the player, and is used
        /// to interpolate the position of the player's avatar.
        /// 
        /// MobNode's LastDirection property is the last direction provided by the server.
        /// </summary>
        public override Vector3 Direction
        {
            get
            {
                return direction;
            }
            set
            {
                if (value != direction)
                {
                    SetDirection(value, this.Position, TimeTool.CurrentTime);
                }
            }
        }

        public bool CanMove() {
            if (this.CheckBooleanProperty("world.nomove"))
                return false;
            return true;
        }

        public bool CanTurn() {
            if (this.CheckBooleanProperty("world.noturn"))
                return false;
            return true;
        }

		/// <summary>
		///   Invoke the base SetDirLoc, and then the dirUpdate fields
		///   to be consistent with the new position and direction
		/// </summary>
		/// <param name="timestamp"></param>
		/// <param name="dir"></param>
		/// <param name="pos"></param>
		public override void SetDirLoc(long timestamp, Vector3 dir, Vector3 pos) {
            base.SetDirLoc(timestamp, dir, pos);
            dirUpdate.position = pos;
            dirUpdate.direction = dir;
            dirUpdate.dirty = false;
		}

	}

}
