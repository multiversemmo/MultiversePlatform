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
using System.Diagnostics;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Media;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for Page.
    ///
    /// Terminology:
    ///
    ///     A page a square region of terrain, of constant size across the
	///     world, typically 256 meters on a side.  An entire page
	///     uses the same material, and usually the adjacent pages as
	///     well.
    ///
    ///     A subpage is the smallest square power-of-2 division of a
    ///     page that has its own meters-per-sample.
    ///
    ///     A tile is a square power-of-2 division of a page which in
    ///     the existing implementation is the unit of rendering.  The
    ///     smallest tile is a subpage; the largest tile is a page,
    ///     and the tile size is chosen based on the distance from the
    ///     camera, based on the ILODSpec method 
    ///            int TilesPerPage(int pagesFromCamera)
    ///
    /// </summary>
	public class Page
	{
		// size of the page in "meters", this is the length of the (square) page along one axis
		protected int size;

		// how many pages from this one to the page containing the camera
		protected int pagesFromCamera;

		// index of this page in the TerrainManager's page array 
		protected int indexX;
		protected int indexZ;

		// location of the page in world space, relative to the page containing the camera
		protected Vector3 relativeLocation;

        protected SceneNode sceneNode;

		// number of tiles (per side) on this page
		protected int numTiles;

		// size of a tile in "sample space"
		protected int tileSize;

		// tiles for this page
		protected Tile [,] tiles;

		protected Page neighborNorth;
		protected Page neighborSouth;
		protected Page neighborEast;
		protected Page neighborWest;

		protected bool neighborsAttached;

        protected static Texture shadeMask;

        protected TerrainPage terrainPage;

		/// <summary>
		/// Page Constructor
		/// </summary>
		/// <param name="relativeLocation">The location of the page in world space, relative to the page containing the camera</param>
		/// <param name="indexX">index into the TerrainManager's page array</param>
		///	<param name="indexZ">index into the TerrainManager's page array</param>
		public Page(Vector3 relativeLocation, int indexX, int indexZ)
		{
			size = TerrainManager.Instance.PageSize;
			this.relativeLocation = relativeLocation;
			this.indexX = indexX;
			this.indexZ = indexZ;

			// Figure out which page the camera is in
			PageCoord cameraPageCoord = new PageCoord(TerrainManager.Instance.VisPageRadius, TerrainManager.Instance.VisPageRadius);
			
			// compute the distance (in pages) from this page to the camera page
			pagesFromCamera = cameraPageCoord.Distance(new PageCoord(indexX, indexZ));

            // create a scene node for this page
            String nodeName = String.Format("Page[{0},{1}]", indexX, indexZ);
            sceneNode = TerrainManager.Instance.WorldRootSceneNode.CreateChildSceneNode(nodeName);

            sceneNode.Position = relativeLocation;

			// ask the world manager how many tiles we need in this page based on the distance from the camera
			numTiles = TerrainManager.Instance.TilesPerPage(pagesFromCamera);

			// compute size of a tile in sample space
			tileSize = size / numTiles;

			// allocate the array of tiles
			tiles = new Tile[numTiles,numTiles];

			// this is the size of the tile in "world space"
			float tileWorldSize = tileSize * TerrainManager.oneMeter;

			// create the tiles for this page
			for ( int tilex = 0; tilex < numTiles; tilex++ ) 
			{
				float tileWorldX = tilex * tileWorldSize + relativeLocation.x;

				for ( int tilez = 0; tilez < numTiles; tilez++ ) 
				{
					float tileWorldZ = tilez * tileWorldSize + relativeLocation.z;

					Tile t = new Tile(new Vector3(tileWorldX, 0, tileWorldZ), tileSize, this, tilex, tilez);
					tiles[tilex, tilez] = t;
				}
			}

		}

        static byte GetSample(byte[] mask, int pageSize, int x, int y)
        {
            if (x < 0)
            {
                x = 0;
            }
            if (x >= pageSize)
            {
                x = pageSize - 1;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (y >= pageSize)
            {
                y = pageSize - 1;
            }

            return mask[x + y * pageSize];
        }

        static byte [] FilterShadeMask(byte[] sourceMask, int pageSize)
        {
            byte[] retMask = new byte[pageSize * pageSize];

            for (int x = 0; x < pageSize; x++)
            {
                for (int y = 0; y < pageSize; y++)
                {
                    int val = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            val += GetSample(sourceMask, pageSize, x + i, y + j);
                        }
                    }

                    retMask[x + y * pageSize] = (byte)(val / 9);
                }
            }

            return retMask;
        }

        static void BuildShadeMask()
        {
            int pageSize = TerrainManager.Instance.PageSize;
            byte[] byteMask = new byte[pageSize * pageSize];

            // fill the mask with 128, which is the 100% value
            for (int i = 0; i < pageSize * pageSize; i++)
            {
                byteMask[i] = 128;
            }

            // create the random number generator
            Random rand = new Random(1234);

            // create a number of "blobs"
            for (int i = 0; i < 200; i++)
            {
                // starting point for this blob
                int x = rand.Next(pageSize);
                int y = rand.Next(pageSize);

                // value for this blob
                int valueRange = 32;
                byte value = (byte)rand.Next(128-valueRange, 128+valueRange);

                // number of points in this blob
                int n = rand.Next(10, 500);

                while (n >= 0)
                {
                    if (byteMask[x + y * pageSize] == 128)
                    {
                        n--;
                        byteMask[x + y * pageSize] = value;
                    }

                    // move in a random direction
                    int dir = rand.Next(8);
                    switch (dir)
                    {
                        case 0:
                            x++;
                            break;
                        case 1:
                            x--;
                            break;
                        case 2:
                            y++;
                            break;
                        case 3:
                            y--;
                            break;
                        case 4:
                            x++;
                            y++;
                            break;
                        case 5:
                            x++;
                            y--;
                            break;
                        case 6:
                            x--;
                            y++;
                            break;
                        case 7:
                            x--;
                            y--;
                            break;
                    }
                    
                    // clamp x and y to the page
                    if (x < 0)
                    {
                        x = 0;
                    }
                    if (x > pageSize - 1)
                    {
                        x = pageSize - 1;
                    }
                    if (y < 0)
                    {
                        y = 0;
                    }
                    if (y > pageSize - 1)
                    {
                        y = pageSize - 1;
                    }
                }
            }

            // simple box filter on the shade mask
            byteMask = FilterShadeMask(byteMask, pageSize);

            // generate a texture from the mask
            Image maskImage = Image.FromDynamicImage(byteMask, pageSize, pageSize, PixelFormat.A8);
            String texName = "PageShadeMask";
            shadeMask = TextureManager.Instance.LoadImage(texName, maskImage);
        }

        public static void SetShadeMask(Material mat, int texunit)
        {
            if (shadeMask == null)
            {
                BuildShadeMask();
            }
            mat.GetTechnique(0).GetPass(0).GetTextureUnitState(texunit).SetTextureName(shadeMask.Name);
        }

		/// <summary>
		/// Set the neighbor pointers for this page.  Must be done after all the pages
		/// in the TerrainManager pages array have been constructed.
		/// </summary>
		public void AttachNeighbors() 
		{
			if ( indexX > 0 ) 
			{
				neighborWest = TerrainManager.Instance.LookupPage(indexX - 1, indexZ);
			} 
			else 
			{
				neighborWest = null;
			}

			if ( indexX < ( TerrainManager.Instance.PageArraySize - 1 ) ) 
			{
				neighborEast = TerrainManager.Instance.LookupPage(indexX + 1, indexZ);
			} 
			else 
			{
				neighborEast = null;
			}

			if ( indexZ > 0 ) 
			{
				neighborNorth = TerrainManager.Instance.LookupPage(indexX, indexZ - 1);
			} 
			else 
			{
				neighborNorth = null;
			}

			if ( indexZ < ( TerrainManager.Instance.PageArraySize - 1 ) ) 
			{
				neighborSouth = TerrainManager.Instance.LookupPage(indexX, indexZ + 1);
			} 
			else 
			{
				neighborSouth = null;
			}

			neighborsAttached = true;

			return;
		}


		public void AttachTiles()
		{
			Debug.Assert(neighborsAttached, "Trying to attach tiles when pages haven't been attached");

			foreach ( Tile t in tiles ) 
			{
				t.AttachNeighbors();
			}
		}

		public int NumTiles 
		{
			get 
			{
				return numTiles;
			}
		}

		public Vector3 Location 
		{
			get 
			{
				return relativeLocation + TerrainManager.Instance.CameraPageLocation;
			}
		}

		public int IndexX 
		{
			get 
			{
				return indexX;
			}
		}

		public int IndexZ 
		{
			get 
			{
				return indexZ;
			}
		}

        public TerrainPage TerrainPage
        {
            get
            {
                return terrainPage;
            }
            set
            {
                terrainPage = value;
                if (terrainPage != null)
                {
                    Debug.Assert(Location == terrainPage.Location, "assigning terrainPage to page with different location");
                }
            }
        }

		/// <summary>
		/// This method is called whenever the camera moves, so that the page can update itself.
		/// </summary>
		/// <param name="camera">the camera for this scene.  used to supply the current camera location</param>
		public void UpdateCamera(Camera camera)
		{
		}

		/// <summary>
		/// Find the tile for a given location
		/// </summary>
		/// <param name="location"></param>
		/// <returns>Returns the tile.  Will raise an array bounds exception if the location is not in this page</returns>
		public Tile LookupTile(Vector3 location)
		{
            PageCoord locPageCoord = new PageCoord(location, size / numTiles);
            PageCoord tilePageCoord = new PageCoord(Location, size / numTiles);
            PageCoord tileOffset = locPageCoord - tilePageCoord;

			return tiles[tileOffset.X, tileOffset.Z];
		}

		public Tile LookupTile(int x, int z) 
		{
			return tiles[x,z];
		}

		public Page FindNeighbor(Direction dir)
		{
			Page p = null;

			switch (dir) 
			{
				case Direction.North:
					p = neighborNorth;
					break;
				case Direction.South:
					p = neighborSouth;
					break;
				case Direction.East:
					p = neighborEast;
					break;
				case Direction.West:
					p = neighborWest;
					break;
			}

			return p;
		}
	}
}
