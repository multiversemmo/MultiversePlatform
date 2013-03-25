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

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for TerrainTile.
	/// </summary>
	public class Tile
	{
		// the size of the tile in sample space
		private int size;

		// the page that owns this tile
		private Page page;

		// indices of this tile within the parent page's tile array
		private int indexX;
		private int indexZ;

		// the location of this tile in world space, relative to the location
		// of the page containing the camera
		private Vector3 relativeLocation;

		private Tile neighborNorth1;
		private Tile neighborNorth2;
		private Tile neighborSouth1;
		private Tile neighborSouth2;
		private Tile neighborEast1;
		private Tile neighborEast2;
		private Tile neighborWest1;
		private Tile neighborWest2;

		public Tile(Vector3 relativeLocation, int tileSize, Page p, int tileX, int tileZ)
		{
			this.relativeLocation = relativeLocation;
			page = p;
			size = tileSize;
			indexX = tileX;
			indexZ = tileZ;

		}

		public void AttachNeighbors() 
		{
			// Find Western neighbor
			if ( indexX == 0 ) 
			{
				Page nextPage = page.FindNeighbor(Direction.West);
				if ( nextPage == null ) 
				{
					neighborWest1 = null;
					neighborWest2 = null;
				}
				else 
				{
					if ( nextPage.NumTiles == page.NumTiles ) 
					{
						neighborWest1 = nextPage.LookupTile(nextPage.NumTiles - 1, indexZ);
						neighborWest2 = null;
					} 
					else if ( nextPage.NumTiles < page.NumTiles ) 
					{
						Debug.Assert(((nextPage.NumTiles * 2) == page.NumTiles), "Adjacent page numtiles not half");
						neighborWest1 = nextPage.LookupTile(nextPage.NumTiles - 1, indexZ / 2);
						neighborWest2 = null;
					} 
					else 
					{
						Debug.Assert((nextPage.NumTiles == ( page.NumTiles * 2)), "Adjacent page numtiles not double");
						neighborWest1 = nextPage.LookupTile(nextPage.NumTiles - 1, indexZ * 2);
						neighborWest2 = nextPage.LookupTile(nextPage.NumTiles - 1, ( indexZ * 2 ) + 1);
					}
				}
			} 
			else 
			{
				neighborWest1 = page.LookupTile(indexX - 1, indexZ);
				neighborWest2 = null;
			}

			// Find Eastern neighbor
			if ( indexX == ( page.NumTiles - 1 ) ) 
			{
				Page nextPage = page.FindNeighbor(Direction.East);
				if ( nextPage == null ) 
				{
					neighborEast1 = null;
					neighborEast2 = null;
				}
				else 
				{
					if ( nextPage.NumTiles == page.NumTiles ) 
					{
						neighborEast1 = nextPage.LookupTile(0, indexZ);
						neighborEast2 = null;
					} 
					else if ( nextPage.NumTiles < page.NumTiles ) 
					{
						Debug.Assert(((nextPage.NumTiles * 2) == page.NumTiles), "Adjacent page numtiles not half");
						neighborEast1 = nextPage.LookupTile(0, indexZ / 2);
						neighborEast2 = null;
					} 
					else 
					{
						Debug.Assert((nextPage.NumTiles == ( page.NumTiles * 2)), "Adjacent page numtiles not double");
						neighborEast1 = nextPage.LookupTile(0, indexZ * 2);
						neighborEast2 = nextPage.LookupTile(0, ( indexZ * 2 ) + 1);
					}
				}
			} 
			else 
			{
				neighborEast1 = page.LookupTile(indexX + 1, indexZ);
				neighborEast2 = null;
			}

			// Find Northern neighbor
			if ( indexZ == 0 ) 
			{
				Page nextPage = page.FindNeighbor(Direction.North);
				if ( nextPage == null ) 
				{
					neighborNorth1 = null;
					neighborNorth2 = null;
				}
				else 
				{
					if ( nextPage.NumTiles == page.NumTiles ) 
					{
						neighborNorth1 = nextPage.LookupTile(indexX, nextPage.NumTiles - 1);
						neighborNorth2 = null;
					} 
					else if ( nextPage.NumTiles < page.NumTiles ) 
					{
						Debug.Assert(((nextPage.NumTiles * 2) == page.NumTiles), "Adjacent page numtiles not half");
						neighborNorth1 = nextPage.LookupTile(indexX / 2, nextPage.NumTiles - 1);
						neighborNorth2 = null;
					} 
					else 
					{
						Debug.Assert((nextPage.NumTiles == ( page.NumTiles * 2)), "Adjacent page numtiles not double");
						neighborNorth1 = nextPage.LookupTile(indexX * 2, nextPage.NumTiles - 1);
						neighborNorth2 = nextPage.LookupTile( ( indexX * 2 ) + 1, nextPage.NumTiles - 1);
					}
				}
			} 
			else 
			{
				neighborNorth1 = page.LookupTile(indexX, indexZ - 1);
				neighborNorth2 = null;
			}

			// Find Southern neighbor
			if ( indexZ == ( page.NumTiles - 1 ) ) 
			{
				Page nextPage = page.FindNeighbor(Direction.South);
				if ( nextPage == null ) 
				{
					neighborSouth1 = null;
					neighborSouth2 = null;
				}
				else 
				{
					if ( nextPage.NumTiles == page.NumTiles ) 
					{
						neighborSouth1 = nextPage.LookupTile(indexX, 0);
						neighborSouth2 = null;
					} 
					else if ( nextPage.NumTiles < page.NumTiles ) 
					{
						Debug.Assert(((nextPage.NumTiles * 2) == page.NumTiles), "Adjacent page numtiles not half");
						neighborSouth1 = nextPage.LookupTile(indexX / 2, 0);
						neighborSouth2 = null;
					} 
					else 
					{
						Debug.Assert((nextPage.NumTiles == ( page.NumTiles * 2)), "Adjacent page numtiles not double");
						neighborSouth1 = nextPage.LookupTile(indexX * 2, 0);
						neighborSouth2 = nextPage.LookupTile(( indexX * 2 ) + 1, 0);
					}
				}
			} 
			else 
			{
				neighborSouth1 = page.LookupTile(indexX, indexZ + 1);
				neighborSouth2 = null;
			}
		}

		public int Size 
		{
			get 
			{
				return size;
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

		public Page Page
		{
			get 
			{
				return page;
			}
		}

        //public bool Hilight
        //{
        //    get
        //    {
        //        return page.Hilight;
        //    }
        //}

        //public Texture HilightMask
        //{
        //    get
        //    {
        //        return page.HilightMask;
        //    }
        //}

        //public Material HilightMaterial
        //{
        //    get
        //    {
        //        return Page.HilightMaterial;
        //    }
        //}
	}
}
