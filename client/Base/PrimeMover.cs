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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;
using Axiom.Animating;
using Axiom.Input;
using Axiom.Graphics;

using Multiverse.CollisionLib;

#endregion

namespace Multiverse.Base
{

	public class PrimeMover
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PrimeMover));

		public static void InitPrimeMover(WorldManager worldMgr, CollisionAPI collisionMgr)
		{
			worldManager = worldMgr;
			collisionManager = collisionMgr;
            playerStuck = false;
		}
		
		private static CollisionAPI collisionManager;
		private static WorldManager worldManager;
        private static bool playerStuck;
        private static DateTime stuckGotoTime;
		
		private static void TraceMOBottom(MovingObject mo, string description)
		{
			if (MO.DoLog) {
				CollisionShape obstacle = mo.parts[0].shape;
				CollisionCapsule moCapsule = null;
				if (obstacle is CollisionCapsule)
					moCapsule = (CollisionCapsule)obstacle;
				string rest = (moCapsule != null ?
							   string.Format("mo bottom is {0}",
											 moCapsule.bottomcenter - new Vector3(0f, moCapsule.capRadius, 0f)) :
							   string.Format("obstacle {0}", obstacle));
				MO.Log("{0}, {1}", description, rest);
			}
		}
		
		private static void TraceObstacle(CollisionShape obstacle)
		{
			if (obstacle is CollisionOBB) {
				CollisionOBB box = (CollisionOBB)obstacle;
				MO.Log(" obstacle top {0} center {1} {2}",
					   box.center.y + box.extents[1], box.center, box);
			}
			else
				MO.Log(" obstacle center {0} {1}", obstacle.center, obstacle);
		}
		
		private static bool NowColliding(MovingObject mo, string description)
		{
			CollisionParms parms = new CollisionParms();
            bool colliding = collisionManager.CollideAfterAddingDisplacement(mo, Vector3.Zero, parms);
			CollisionShape obstacle = mo.parts[0].shape;
			CollisionCapsule moCapsule = null;
			if (obstacle is CollisionCapsule)
				moCapsule = (CollisionCapsule)obstacle;
			string rest = (moCapsule != null ?
						   string.Format("mo bottom is {0}",
										 moCapsule.bottomcenter - new Vector3(0f, moCapsule.capRadius, 0f)) :
						   string.Format("obstacle {0}", obstacle));
            if (MO.DoLog)
				MO.Log("{0}, now colliding = {1}; {2}", description, colliding, rest);
			log.DebugFormat("{0}, now colliding = {1}; {2}", description, colliding, rest);
            return colliding;
		}
		
		// Move the desired displacement, limited by hitting an
		// obstacle.  Then, if we're not already at the terrain level,
		// "fall" until we are either at the terrain level, or hit an
		// obstacle
		public static Vector3 MoveMobNode(MobNode mobNode, Vector3 requestedDisplacement, Client client)
		{
//             Logger.Log(0, "MoveMobNode oid {0} requestedDisplacement {1}", mobNode.Oid, requestedDisplacement);
//             log.DebugFormat("MoveMobNode: mobNode oid {0}, name {1}, followTerrain {2}, position {3}, disp {4}",
//                             mobNode.Oid, mobNode.Name, mobNode.FollowTerrain, mobNode.Position, requestedDisplacement);
            Vector3 start = mobNode.Position;
			MovingObject mo = mobNode.Collider;
			bool collided = false;
            // Zero the y coordinate of displacement, because it seems
            // that it can be arbitrarily large
			Vector3 desiredDisplacement = requestedDisplacement;
			if (mobNode.FollowTerrain)
                desiredDisplacement.y = 0;
			if (desiredDisplacement.LengthSquared <= float.Epsilon)
				return start;
			if (MO.DoLog)
				MO.Log("MoveMobNode called with mobNode {0} at {1}, disp of {2}",
					   mobNode.Oid, start, requestedDisplacement);
			if (collisionManager == null) {
                log.Info("MoveMobNode: returning because collisionManager isn't initialized");
				return start + desiredDisplacement;
            }
            if (mo == null || mo.parts.Count == 0) {
				if (MO.DoLog)
					MO.Log("MoveMobNode returning because no collision volume for node");
				return start + requestedDisplacement;
			}
            if (mobNode is Player && NowColliding(mo, "Testing player collision on entry")) {
                if (client.MillisecondsStuckBeforeGotoStuck != 0) {
                    if (!playerStuck) {
                        stuckGotoTime = DateTime.Now.AddMilliseconds(client.MillisecondsStuckBeforeGotoStuck);
                        playerStuck = true;
                    }
                    else if (DateTime.Now >= stuckGotoTime) {
                        // We issue the goto command to move us out of the
                        // collision volume
                        client.Write("Executing /stuck command because player has been in a collision volume for " + client.MillisecondsStuckBeforeGotoStuck + " milliseconds");
                        client.NetworkHelper.SendTargettedCommand(client.Player.Oid, "/stuck");
                        playerStuck = false;
                        return start;
                    }
                }
            }
            else
                playerStuck = false;
			// If we haven't completed setup to this extent, just give up
			CollisionParms parms = new CollisionParms();
            Vector3 pos = FindMobNodeDisplacement(mobNode, parms, desiredDisplacement, out collided);
//             log.DebugFormat("MoveMobNode: mobNode oid {0}, name {1}, mob node position {2}, displacement {3}",
//                 mobNode.Oid, mobNode.Name, pos, requestedDisplacement);
            float h = worldManager.GetHeightAt(pos);
            // If we're already below ground level, just set our
            // level to ground level.  This will have to be modified
            // if we deal with caves
            if (pos.y - h < 0) {
//                 log.DebugFormat("MoveMobNode: mobNode oid {0}, name {1} below terrain level", mobNode.Oid, mobNode.Name);
                mo.AddDisplacement(new Vector3(0f, h - pos.y, 0f));
                pos.y = h;
                if (MO.DoLog && (pos.y - h) < -.001 * Client.OneMeter)
                    MO.Log(string.Format(" MobNode at {0} is below ground height {1}!", 
                                         pos, h));
            } // else {
                if (mobNode.FollowTerrain) {
// 					NowColliding(mo, " Before falling loop");
					// Fall toward the terrain or an obstacle, whichever comes
					// first
					float step = mo.StepSize(new Vector3(0, h, 0));
					while (true) {
						if (Math.Abs(pos.y - h) < CollisionAPI.VerticalTerrainThreshold * Client.OneMeter) {
							mo.AddDisplacement(new Vector3(0f, h - pos.y, 0f));
							pos.y = h;
							break;
						} else {
							float dy = -Math.Min(pos.y - h, step);
							Vector3 displacement = new Vector3(0, dy, 0);
                            Vector3 cd = displacement;
							if (MO.DoLog) {
								MO.Log(" Testing for collision falling {0}", dy);
								TraceMOBottom(mo, " Before falling");
							}
							if (collisionManager.TestCollision(mo, ref displacement, parms)) {
								if (MO.DoLog) {
									TraceMOBottom(mo, " After TestCollision after falling");
									NowColliding(mo, " After TestCollision after falling");
									MO.Log(" Collision when object {0} falls from {1} to {2}", 
										   parms.part.handle, pos, pos + cd);
									TraceObstacle(parms.obstacle);
									MO.Log(" Adding dy {0} - displacement.y {1} to pos {2}",
										   dy, displacement.y, pos);
								}
								pos.y += dy - displacement.y;
								break;
							}
							if (MO.DoLog)
								MO.Log(" Didn't collide falling; dy {0}, pos {1}",
									   dy, pos);
							pos.y += dy;
						}
					}
                } else {
                    if (MO.DoLog)
                        MO.Log(" Not falling because mobNode {0} doesn't have FollowTerrain",
                            mobNode.Oid);
                }
// 			}

			if (MO.DoLog) {
				NowColliding(mo, " Leaving MoveMobNode");
				MO.Log("MoveMobNode returning pos {0}", pos);
				MO.Log("");
			}
			if (collided)
                log.DebugFormat("MoveMobNode collided: mobNode oid {0}, name {1}, orig pos {2}, displacement {3}, new pos {4}",
                    mobNode.Oid, mobNode.Name, start, requestedDisplacement, pos);
            else
                log.DebugFormat("MoveMobNode didn't collide: mobNode oid {0}, name {1}, orig pos {2}, displacement {3}, new pos {4}",
                    mobNode.Oid, mobNode.Name, start, requestedDisplacement, pos);
            return pos;
		}
		
		// We only call this if both the collisionManager and the collider exist
		private static Vector3 FindMobNodeDisplacement(MobNode mobNode, CollisionParms parms, Vector3 desiredDisplacement, out bool collided)
		{
			Vector3 start = mobNode.Position;
			Vector3 pos = start + desiredDisplacement;
			Vector3 displacement = desiredDisplacement;
			MovingObject mo = mobNode.Collider;
			Vector3 moStart = mo.parts[0].shape.center;
			if (MO.DoLog) {
				MO.Log(" moStart = {0}, start = {1}", moStart, start);
				MO.Log(" pos = {0}, displacement = {1}", pos, displacement);
				TraceMOBottom(mo, " On entry to FindMobNodeDisplacement");
			}
			collided = false;
            if (collisionManager.TestCollision(mo, ref displacement, parms)) {
				collided = true;
                if (MO.DoLog) {
					MO.Log(" Collision when moving object {0} from {1} to {2}", 
						   parms.part.handle, start, pos);
					NowColliding(mo, " After first TestCollision in FindMobNodeDisplacement");
					TraceObstacle(parms.obstacle);
					MO.Log(" Before collision moved {0}", desiredDisplacement - displacement);
				}
				// Decide if the normals are such that we want
				// to slide along the obstacle
				Vector3 remainingDisplacement = displacement;
				Vector3 norm1 = parms.normObstacle.ToNormalized();
				if (DecideToSlide(mo, start + displacement, parms, ref remainingDisplacement)) {
					if (MO.DoLog)
						MO.Log(" After DecideToSlide, remainingDisplacement {0}", remainingDisplacement);
					// We have to test the displacement
					if (collisionManager.TestCollision(mo, ref remainingDisplacement, parms)) {
						if (MO.DoLog) {
							NowColliding(mo, " After first try TestCollision");
							MO.Log(" Slid into obstacle on the first try; remainingDisplacement = {0}", 
								   remainingDisplacement);
							TraceObstacle(parms.obstacle);
						}
						if (remainingDisplacement.LengthSquared > 0) {
							Vector3 norm2 = parms.normObstacle.ToNormalized();
							// Find the cross product of the of norm1 and
							// norm2, and dot with displacement.  If
							// negative, reverse.
							Vector3 newDir = norm1.Cross(norm2);
							float len = newDir.Dot(remainingDisplacement);
							if (len < 0) {
								newDir = - newDir;
								len = - len;
							}
							Vector3 slidingDisplacement = len * newDir;
							Vector3 originalSlidingDisplacement = slidingDisplacement;
                            if (MO.DoLog) {
								MO.Log(" norm1 = {0}, norm2 = {1}, len = {2}",
									   norm1, norm2, len);
								MO.Log(" Cross product slidingDisplacement is {0}", slidingDisplacement);
							}
							if (collisionManager.TestCollision(mo, ref slidingDisplacement, parms)) {
								if (MO.DoLog) {
									NowColliding(mo, " After second try TestCollision");
									MO.Log(" Slid into obstacle on the second try; slidingDisplacement = {0}", 
										   slidingDisplacement);
								}
							}
							else
								if (MO.DoLog)
									MO.Log(" Didn't slide into obstacle on the second try");
                            remainingDisplacement -= (originalSlidingDisplacement - slidingDisplacement);
						}
					}
				}
                else
                    remainingDisplacement = displacement;
				if (MO.DoLog)
                    MO.Log(" Before checking hop, remainingDisplacement is {0}", remainingDisplacement);
                if (remainingDisplacement.Length > 30f) {
					// Try to hop over the obstacle
					Vector3 c = remainingDisplacement;
					mo.AddDisplacement(new Vector3(0f, CollisionAPI.HopOverThreshold * Client.OneMeter, 0f));
					if (MO.DoLog) {
						TraceMOBottom(mo, " Before trying to hop");
						MO.Log(" remainingDisplacement {0}", remainingDisplacement);
					}
					if (collisionManager.TestCollision(mo, ref remainingDisplacement, parms)) {
						if (MO.DoLog) {
							MO.Log(" Even after hopping up {0} meters, can't get over obstacle; disp {1}",
								   CollisionAPI.HopOverThreshold, remainingDisplacement);
						}
						c = c - remainingDisplacement;
				 		c.y = 0;
						c += new Vector3(0f, CollisionAPI.HopOverThreshold * Client.OneMeter, 0f);
						if (MO.DoLog)
							MO.Log(" After failed hop, subtracting {0}", c);
						mo.AddDisplacement(- c);
					}
					else if (MO.DoLog) {
						MO.Log(" Hopping up {0} meters got us over obstacle; disp {1}",
							   CollisionAPI.HopOverThreshold, remainingDisplacement);
						TraceMOBottom(mo, " After hopping");
					}
					NowColliding(mo, " After hopping");
				}
			}
			Vector3 moPos = mo.parts[0].shape.center;
			pos = start + moPos - moStart;
			if (MO.DoLog) {
				MO.Log(" mo location = {0}, moPos - moStart {1}", moPos, moPos - moStart);
				NowColliding(mo, " Leaving FindMobNodeDisplacement");
				MO.Log(" pos = {0}", pos);
			}
			return pos;
		}
		
		// Normalize the angle so that it's contained in +/- 180 degrees
		private static float NormalizeAngle(float angle)
		{
			float floatPI = (float)Math.PI;
			if (angle > floatPI)
				return angle - 2.0f * floatPI;
			else if (angle < - floatPI)
				return angle + 2.0f * floatPI;
			else
				return angle;
		}
		
		// Decide, based on the normal to the collision object, if the 
		// moving object can slide across the obstacle, and if it can,
		// return the updated displacement.  This displacement may in fact
		// run into _another_ obstacle, however, so the call must again 
		// run the collision test.
		private static bool DecideToSlide(MovingObject mo, Vector3 mobNodePosition, 
										  CollisionParms parms, ref Vector3 displacement) 
		{
			Vector3 normDisplacement = displacement.ToNormalized();
			Vector3 normObstacle = parms.normObstacle.ToNormalized();
			if (MO.DoLog) {
				MO.Log(" DecideToSlide: normObstacle {0}, normDisplacement {1}",
					   normObstacle.ToString(), normDisplacement.ToString());
				MO.Log(" DecideToSlide: displacement {0}", displacement);
			}
			// First we find the angle between the normal and the
			// direction of travel, and reject the displacement if
			// it's too small
			float slideAngle = (NormalizeAngle((float)Math.Acos((double)normDisplacement.Dot(normObstacle))) -
								.5f * (float)Math.PI);
			if (Math.Abs(slideAngle) > CollisionAPI.MinSlideAngle) {
				if (MO.DoLog)
					MO.Log(" After collision, displacement {0}, won't slide because slideAngle {1} > minSlideAngle {2}",
						   displacement.ToString(), slideAngle, CollisionAPI.MinSlideAngle);
				displacement = Vector3.Zero;
				return false;
			}
			// Then we find the angle with the y axis, and reject the
			// displacement if it's too steep
			float verticalAngle = NormalizeAngle((float)Math.Acos((double)normDisplacement[1]));
			if (Math.Abs(verticalAngle) > CollisionAPI.MaxVerticalAngle) {
				if (MO.DoLog)
					MO.Log(" After collision, displacement {0}, won't slide because verticalAngle {1} <= maxVerticalAngle {2}",
						   displacement.ToString(), verticalAngle, CollisionAPI.MaxVerticalAngle);
				displacement = Vector3.Zero;
				return false;
			}
			// Else, we can slide, so return a displacement that
			// points in the direction we're sliding, and has length
			// equal to a constant times the displacement length 

			// Rotate displacement so that it's 90 degress from the
			// obstacle normal
			Vector3 cross = normObstacle.Cross(normDisplacement);
			Quaternion q = Quaternion.FromAngleAxis(.5f * (float)Math.PI, cross);
            Matrix4 transform = q.ToRotationMatrix();
			Vector3 transformedNorm = transform * normObstacle.ToNormalized();
			float len = displacement.Length;
			displacement = transformedNorm * len;
// 			Vector3 alignedPart = normObstacle * (normObstacle.Dot(displacement));
// 			displacement -= alignedPart;
			
			Vector3 p = mobNodePosition + displacement;
			float h = worldManager.GetHeightAt(p);
			// If sliding would put us below ground, limit the displacement
			if (h > p.y) {
				if (MO.DoLog)
					MO.Log(" Sliding up because terrain height is {0} is higher than projected mobNode height {1}",
						   h, p.y);
 				displacement.y += h - p.y;
			}
			if (MO.DoLog) {
				MO.Log(" Exiting DecideToSlide, sliding displacement {0}, slideAngle {1},  verticalAngle {2}",
					   displacement.ToString(), slideAngle, verticalAngle);
				MO.Log(" Exiting DecideToSlide, cross product {0}, quaternion {1}, transformedNorm {2}", 
					   cross, q, transformedNorm);
// 				MO.Log(" Exiting DecideToSlide, alignedPart {0}", alignedPart);
			}
			return true;
		}

	}
}
