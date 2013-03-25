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

using System;
using System.IO;
using System.Diagnostics;
using Axiom.MathLib;
using Multiverse;
using Multiverse.CollisionLib;

namespace Multiverse.CollisionLib
{
    
    /// Coordinate systems used here:
    ///
    ///   World Space - the coordinate system used by the game.
    ///   Current multiverse client/server use a millimeter scale.
    ///
    ///   Collision tile coordinates.  These are single-precision
    ///   integers, in units of tileSize, with 0,0 at the World Space
    ///   0,0.  To get from World Space coords to collision tile
    ///   coords, divide x and z by oneMeter * tileSize
    ///   and truncate to the nearest integer.
    ///

    // The tile manager manages conceptual tiles, whose identity is
    // given by their implicit position.  The only actual data kept
    // for each tile is the count of collision objects in the tile
    public class CollisionTileManager
    {
        #region Private State And Methods
        
        private CollisionAPI collisionAPI;
        private float tileSize;
        private float collisionHorizon;

        // The current center of the tile array in world coordinates
        private Vector3 tileWorldCenter;
        // These are tile coordinates of the tileWorldCenter
        private int tileXCenter;
        private int tileZCenter;

        // The new center of the tile array
        private Vector3 newTileWorldCenter;
        // These are new tile coordinates of the center
        private int newTileXCenter;
        private int newTileZCenter;

        // The number of tiles wide and high in the array
        private int tileCount;
        // Half that number
        private int halfTileCount;
        // The array representing the tiles
        private int[,] tiles;

        // Double 4,000 kilometers
        private const float maximumCoordinate = 4000f * 1000f * 2f;

        private void WorldToTileCoords(Vector3 position, out int tileX, out int tileZ)
        {
            tileX = (int)Math.Floor((double)(position.x / tileSize));
            tileZ = (int)Math.Floor((double)(position.z / tileSize));
        }
        
        private Vector3 TileToWorldCoords(int tileX, int tileZ)
        {
            return new Vector3(tileX * tileSize, 0.0f, tileZ * tileSize);
        }
        
        private int TileToArrayIndex(int tileXorZ, int tileCenterXorZ)
        {
            int i = tileXorZ - (tileCenterXorZ - halfTileCount);
            Debug.Assert(i >= 0 && i < tileCount);
            return i;
        }
        
        private int ArrayIndexToTile(int index, int tileCenter)
        {
            return  tileCenter + index - halfTileCount;
        }
        
        private long ComposeHandle(Vector3 position)
        {
            int tileX, tileZ;
            WorldToTileCoords(position, out tileX, out tileZ);
            return ComposeHandle(tileX, tileZ);
        }

        private long ComposeHandle(int tileX, int tileZ)
        {
            
			return -((((long)tileX) & 0x7fffffff) << 32 | (((long)tileZ) & 0x7fffffff));
        }
        
        private void ZeroTiles(int[,] tileArray)
        {
            for (int i=0; i<tileCount; i++) {
                for (int j=0; j<tileCount; j++) {
                    tileArray[i,j] = 0;
                }
            }
        }

        private enum CollisionObjectType { ColSphere, ColCylinder, ColBox };
        
        // This method is the callback by which the forests controlled
        // by the scene manager tell us about SpeedTrees.
        private void AddTreeObstacles(SpeedTreeWrapper tree)
        {
            // If this tree didn't use to be in range but now is
            V3 vp = tree.TreePosition;
            Vector3 p = new Vector3(vp.x, vp.y, vp.z);
            // Find the tile in question
            int tileX, tileZ;
            WorldToTileCoords(p, out tileX, out tileZ);
            bool oir = InRange(tileXCenter, tileZCenter, tileX, tileZ);
            bool nir = InRange(newTileXCenter, newTileZCenter, tileX, tileZ);
//             if (MO.DoLog)
//                 MO.Log(String.Format("For tree at {0}, testing !InRange({1},{2},{3},{4}) = {5} && InRange({6},{7},{8},{9}) = {10}",
//                                      MO.MeterString(p),tileXCenter, tileZCenter, tileX, tileZ, MO.Bool(!oir),
//                                      newTileXCenter, newTileZCenter, tileX, tileZ, MO.Bool(nir)));
            if (!oir && nir) {
                int tX = TileToArrayIndex(tileX, newTileXCenter);
                int tZ = TileToArrayIndex(tileZ, newTileZCenter);
                uint count = tree.CollisionObjectCount;
                long handle = ComposeHandle(tileX, tileZ);
                CollisionShape shape = null;
                if (MO.DoLog) {
                    MO.Log(string.Format("Adding tree at {0}, tiles[{1},{2}] = {3}, tile coords[{4},{5}], obj. new count {6}",
                                      MO.MeterString(p), tX, tZ, tiles[tX,tZ], tileX, tileZ, count));
                    MO.Log(string.Format("Handle {0}, oldcenter {1}, newcenter {2}",
										 MO.HandleString(handle), MO.MeterString(tileWorldCenter), MO.MeterString(newTileWorldCenter)));
                }
                float size = 0f;
                float variance = 0f;
                tree.GetTreeSize(ref size, ref variance);
                float scaleFactor = size / tree.OriginalSize;
                for (uint i=0; i<count; i++) {
                    TreeCollisionObject tco = tree.CollisionObject(i);
                    Vector3 cp = new Vector3(tco.position.x, tco.position.y, tco.position.z) * scaleFactor + p;
                    Vector3 cd = new Vector3(tco.dimensions.x, tco.dimensions.y, tco.dimensions.z) * scaleFactor;
                    switch ((CollisionObjectType)tco.type) {
                    case CollisionObjectType.ColSphere:
                        shape = new CollisionSphere(cp, cd.x);
                        break;
                    case CollisionObjectType.ColCylinder:
                        // We treat it as a capsule, but we raise the
                        // capsule up by the capRadius, and shorten
                        // the segment by the double the capRadius
                        Vector3 top = cp;
                        top.y += cd.y - cd.x * 2f;
                        cp.y += cd.x;
                        shape = new CollisionCapsule(cp, top, cd.x);
                        break;
                    case CollisionObjectType.ColBox:
                        Vector3 tp = cp;
                        tp.x -= cd.x * .5f;
                        tp.y -= cd.y * .5f;
                        shape = new CollisionAABB(tp, tp + cd);
                        break;
                    }
                    collisionAPI.AddCollisionShape(shape, handle);
					tiles[tX, tZ]++;
                    objectsAdded++;
					
					if (MO.DoLog) {
                        MO.Log(string.Format(" tiles[{0},{1}] = {2}, tile at [{3},{4}] after adding shape {5}",
                                          tX, tZ, tiles[tX, tZ], tileX, tileZ, shape.ToString()));
                    }
                }
            }
        }

        private bool InRange(int tXCenter, int tZCenter, int tX, int tZ)
        {
            bool goodX = tX >= tXCenter - halfTileCount && tX < tXCenter + halfTileCount;
            bool goodZ = tZ >= tZCenter - halfTileCount && tZ < tZCenter + halfTileCount;
            return goodX && goodZ;
        }

        private void InitializeBasedOnCollisionHorizon(float collisionHorizon)
        {
            this.collisionHorizon = collisionHorizon;
            // Initialize to a location that can never be the first location
            tileWorldCenter = new Vector3(maximumCoordinate, 0, maximumCoordinate);
            WorldToTileCoords(tileWorldCenter, out tileXCenter, out tileZCenter);
            // Set up the 2-d array of ints representing the counts of
            // objects in each conceptual tile.
            halfTileCount = (int)Math.Ceiling((double)(collisionHorizon / tileSize));
            tileCount = 2 * halfTileCount;
            tiles = new int[tileCount, tileCount];
            ZeroTiles(tiles);
        }

        private void FlushAllCollisionCounts()
        {
            if (tiles == null)
                return;
            if (MO.DoLog)
                MO.Log("Flushing all collision counts");
            for (int i=0; i<tileCount; i++) {
                for (int j=0; j<tileCount; j++) {
                    if (tiles[i,j] != 0) {
                        int tX = ArrayIndexToTile(i, tileXCenter);
                        int tY = ArrayIndexToTile(j, tileZCenter);
                        long handle = ComposeHandle(tX, tY);
                        if (MO.DoLog) {
                            MO.Log(string.Format("Flushing tileX={0}, tileY={1}, tiles[{2},{3}], handle {4}, center {5} in FlushAllCollisioncounts",
												 tX, tY, i, j, MO.HandleString(handle), MO.MeterString(tileWorldCenter)));
                        }
                        int count = collisionAPI.RemoveCollisionShapesWithHandle(handle);
                        objectsRemoved += count;
						tiles[i,j] = 0;
                    }
                }
            }
        }
        
        private string TileArrayString(int [,] tileArray, int tileXCenter, int tileZCenter)
        {
            string s = "";
            for (int i=0; i<tileCount; i++) {
                for (int j=0; j<tileCount; j++) {
                    if (tileArray[i,j] != 0) {
                        if (s != "")
                            s += ", ";
                        s += string.Format("{0}:{1} [{2},{3}] = {4}", 
                                           ArrayIndexToTile(i, tileXCenter), 
                                           ArrayIndexToTile(j, tileZCenter),
                                           i, j, tileArray[i,j]);
                    }
                }
            }
            return s;
        }
        
        private void ShiftTileArrayToNewCenter()
        {
//          if (MO.DoLog) {
//              MO.Log("ShiftTileArrayToNewCenter");
//              MO.Log(string.Format(" Nonzero tiles {0}", 
//                                   TileArrayString(tiles, tileXCenter, tileZCenter)));
//          }
            // Iterate over the tiles, flushing objects that are no
            // longer in range
            arrayShifts++;
			for (int i=0; i<tileCount; i++) {
                for (int j=0; j<tileCount; j++) {
                    if (tiles[i,j] != 0) {
                        int tX = ArrayIndexToTile(i, tileXCenter);
                        int tZ = ArrayIndexToTile(j, tileZCenter);
                        if (!InRange(newTileXCenter, newTileZCenter, tX, tZ)) {
                            long handle = ComposeHandle(tX, tZ);
							if (MO.DoLog)
								MO.Log(string.Format(" Flushing tile coords[{0},{1}], tiles[{2},{3}] = {4}, handle {5}, center {6}",
													 tX, tZ, i, j, tiles[i,j], MO.HandleString(handle), MO.MeterString(newTileWorldCenter)));
                            int count = collisionAPI.RemoveCollisionShapesWithHandle(handle);
							objectsRemoved += count;
//                          if (MO.DoLog)
//                              MO.Log(string.Format(" Flushed {0} shapes at tiles[{1},{2}]",
//                                                   count, i, j));
                            tiles[i,j] = 0;
                        }
                    }
                }
            }
//          if (MO.DoLog) {
//              MO.Log(string.Format(" After range flush, nonzero tiles {0}", 
//                                   TileArrayString(tiles, tileXCenter, tileZCenter)));
//          }
            
            // Now shift the array of counts.
            // If either coord difference moves the tiles so that none
            // of the entries are in range, just zero the array
            if (Math.Abs(newTileXCenter - tileXCenter) >= tileCount ||
                Math.Abs(newTileZCenter - tileZCenter) >= tileCount) {
                if (MO.DoLog)
                    MO.Log(" Moved more than tileCount, so zeroing");
                ZeroTiles(tiles);
            }
            // Else we have to move the contents of the array.  Do
            // this in a temporary array, because of the overlap
            // constraints.
            else {
                int xDelta = newTileXCenter - tileXCenter;
                int zDelta = newTileZCenter - tileZCenter;
                if (MO.DoLog)
                    MO.Log(string.Format(" xDelta {0}, zDelta {1}", xDelta, zDelta));
                int[,] newTiles = new int[tileCount, tileCount];
                ZeroTiles(newTiles);
                for (int i=0; i<tileCount; i++) {
                    for (int j=0; j<tileCount; j++) {
                        int xSource = i + xDelta;
                        int zSource = j + zDelta;
                        int source = ((xSource >= 0) && (xSource < tileCount) &&
                                      (zSource >= 0) && (zSource < tileCount)
                                      ? tiles[xSource,zSource] : 0);
                        if (source != 0) {
//                          if (MO.DoLog)
//                              MO.Log(string.Format(" Moving tiles[{0},{1}] to tiles[{2},{3}], count {4}, center {5}, newcenter {6}",
//                                                   xSource, zSource, i, j, source, 
//                                                   MO.MeterString(tileWorldCenter), MO.MeterString(newTileWorldCenter)));
                            newTiles[i,j] = source;
                        }
                    }
                }
//              if (MO.DoLog) {
//                  MO.Log(string.Format(" newTiles nonzero tiles {0}", 
//                                       TileArrayString(newTiles, tileXCenter, tileZCenter)));
//              }
                // Now copy back
                for (int i=0; i<tileCount; i++) {
                    for (int j=0; j<tileCount; j++) {
                        tiles[i,j] = newTiles[i,j];
                    }
                }
            }
        }
        
        #endregion Private State And Methods

        #region External Interface To CollisionTileManager
        
        public static int arrayShifts = 0;
		public static int objectsAdded = 0;
		public static int objectsRemoved = 0;
		
		public CollisionTileManager(CollisionAPI API, float tileSize,
                                    FindObstaclesInBoxCallback ObstacleFinder)
        {
            collisionAPI = API;
            this.tileSize = tileSize;
            this.ObstacleFinder = ObstacleFinder;
            collisionHorizon = 0.0f;
            tiles = null;
        }
        
        // This is the callback used by elements of the boundary
        public delegate void AddTreeObstaclesCallback(SpeedTreeWrapper tree);
        
        // This is what we get from the WorldManager
        public delegate void FindObstaclesInBoxCallback(AxisAlignedBox box, 
                                                        AddTreeObstaclesCallback callback);
        
        private FindObstaclesInBoxCallback ObstacleFinder;

        public void RecreateCollisionTiles ()
		{
			if (MO.DoLog)
				MO.Log("Flushing all tiles due to call to RecreateTiles");
			FlushAllCollisionCounts();
			InitializeBasedOnCollisionHorizon(collisionHorizon);
		}
		
		// This is the principle interface to the upper levels of the
		// // client.  When the client realizes that the center of the
		// // collision visibility circle has changed, it calls
		// SetCollisionArea // to let us know.
        //
        // Nearly always the collisionHorizon is the same as the original
        // collisionHorizon.  If it's not, we flush all collision counts and
        // rebuild everything.
        public void SetCollisionArea(Vector3 newTileWorldCenterValue, float newCollisionHorizon)
        {
            newTileWorldCenter = newTileWorldCenterValue;
            WorldToTileCoords(newTileWorldCenter, out newTileXCenter, out newTileZCenter);
            if (tiles == null || Math.Abs(newCollisionHorizon - collisionHorizon) > .0001) {
                // We need to start from scratch again
                if (MO.DoLog)
                    MO.Log("Horizon changed; flushing everything");
                FlushAllCollisionCounts();
                InitializeBasedOnCollisionHorizon(newCollisionHorizon);
            }
            else {
                
                if (newTileXCenter == tileXCenter && newTileZCenter == tileZCenter) {
                    // the tile center didn't change, so there is nothing
                    // to do put set the world center
                    tileWorldCenter = newTileWorldCenter;
                    return;
                }
                else {
                    if (MO.DoLog) {
                        MO.Log(string.Format("old center {0}[{1},{2}], new center {3}[{4},{5}], old horizon {6}, new horizon {7}",
                                             MO.MeterString(tileWorldCenter), tileXCenter, tileZCenter,
                                             MO.MeterString(newTileWorldCenter), newTileXCenter, newTileZCenter,
                                             MO.MeterString(collisionHorizon), MO.MeterString(newCollisionHorizon)));
                    }
                    ShiftTileArrayToNewCenter();
                }
            }
            // Now ask the WorldManager to iterate over boundary
            // objects, passing a box representing the world coords of
            // the new bounds of the entire tile array, and a callback
            // to actually add the objects
            
            // Choose a bound that is guaranteed to cover all the
            // trees in the tile array
            float bound = 2 * tileSize + collisionHorizon;
            Vector3 d = new Vector3(bound, 0.0f, bound);
            Vector3 min = newTileWorldCenter - d;
            Vector3 max = newTileWorldCenter + d;
            // Make the range of Y "infinitely" large
            min.y -= 10000.0f * MO.Meter;
            max.y += 10000.0f * MO.Meter;
            AxisAlignedBox b = new AxisAlignedBox(min, max);
            if (MO.DoLog)
                MO.Log(string.Format("Searching for obstacles in box min = {0}, max = {1}", 
                                     MO.MeterString(min), MO.MeterString(max)));
            ObstacleFinder(b, AddTreeObstacles);

            // Finally, update the tileWorldCenter
            tileWorldCenter = newTileWorldCenter;
            tileXCenter = newTileXCenter;
            tileZCenter = newTileZCenter;
        }
    
        #endregion External Interface To CollisionTileManager
    
    }
}
