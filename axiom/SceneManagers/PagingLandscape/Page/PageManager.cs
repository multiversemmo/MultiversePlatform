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

#endregion LGPL License



#region Using Directives



using System;

using System.Collections;

using System.Diagnostics;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;

using Axiom.Media;



using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Tile;

using Axiom.SceneManagers.PagingLandscape.Data2D;

using Axiom.SceneManagers.PagingLandscape.Renderable;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Page

{

	public enum CameraPageState

	{

		Inside   = 1,

		Outside  = 2,

		Change   = 4

	}



	/// <summary>

	/// Summary description for PageManager.

	/// </summary>

	public class PageManager

	{

		#region Fields

		/// Root scene node
		protected SceneNode sceneRoot;

		/** LandScape pages for the terrain.
		*/
		protected Pages pages;

		/** Queues to batch the process of loading and unloading Pages.
			This avoid the plugin to load a lot of Pages in a single Frame, droping the FPS.
		*/
		protected PageQueue pageLoadQueue;
		protected PageQueue pageUnloadQueue;
		protected PageQueue pagePreloadQueue;
		protected PageQueue pagePostunloadQueue;

		/** LandScapePage index where the camera is.
		*/
		protected long currentCameraPageX, currentCameraPageZ;
		protected long currentCameraTileX, currentCameraTileZ;


		/** The last estate for the camera.
		*/
		protected CameraPageState lastCameraPageState;

		protected long width ;
		protected long heigth;

		protected int pause;
		protected Vector3 lastCameraPos;


		#endregion Fields



		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		public PageManager(SceneNode rootNode) 

		{

			if (instance != null) 

			{

				throw new ApplicationException("PageManager.Constructor() called twice!");

			}

			instance = this;



			sceneRoot = rootNode;

			currentCameraPageX = 0;
			currentCameraPageZ = 0;
			currentCameraTileX = 0;
			currentCameraTileZ = 0;

			lastCameraPageState = CameraPageState.Change;

			lastCameraPos = new Vector3(-999999,9999999,-999999);

			pause = 99;

			width = Options.Instance.World_Width;
			heigth = Options.Instance.World_Height;

			pageLoadQueue = new PageQueue();
			pageUnloadQueue = new PageQueue();
			pagePreloadQueue = new PageQueue();
			pagePostunloadQueue = new PageQueue();
			//setup the page array.
			//    mPages.reserve (mWidth);
			//    mPages.resize (mWidth);
			pages = new Pages( width );
			for (long  i = 0; i < width; i++ )
			{
				PageRow pr = new PageRow( heigth );

				//        mPages[ i ].reserve (mHeigth);
				//        mPages[ i ].resize (mHeigth);
				for (long  j = 0; j < heigth; j++ )
				{
					pr.Add( new Page( i, j ) );
				}

				pages.Add( pr );
			}

			for (long  j = 0; j < heigth; j++ )
			{
				for (long  i = 0; i < width; i++ )
				{		
					if ( j != heigth - 1)
					{
						pages[ i ][ j     ].SetNeighbor( Neighbor.South, pages[ i ][ j + 1 ] );
						pages[ i ][ j + 1 ].SetNeighbor( Neighbor.North, pages[ i ][ j     ] );
					}

					if ( i != width - 1)
					{
						pages[ i     ][ j ].SetNeighbor( Neighbor.East, pages[ i + 1 ][ j ] );
						pages[ i + 1 ][ j ].SetNeighbor( Neighbor.West, pages[ i     ][ j ] );
					}
				}
			}
		}





		private static PageManager instance = null;



		public static PageManager Instance 

		{

			get 

			{

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				instance = null;

			}

		}



		#endregion IDisposable Implementation


		public void Update( Camera cam )
		{
			// Here we have to look if we have to load, unload any of the LandScape Pages
			//Vector3 pos = cam.getPosition();
			// Fix from Praetor, so the camera used gives you "world-relative" coordinates
			Vector3 pos = cam.DerivedPosition;

			//updateStats( pos );

			// Update only if the camera was moved
			// make sure in the bounding box of landscape	
			pos.y = 127.0f;
			if ( Options.Instance.CameraThreshold <= (lastCameraPos - pos).LengthSquared )
			{		
				// Check for the camera position in the LandScape Pages list, we check if we are in the inside the security zone
				//  if not, launch the change routine and update the camera position currentCameraX, currentCameraY.
				if ( pages[ currentCameraPageX ][ currentCameraPageZ ] != null && 
					 (
						pages[ currentCameraPageX ][ currentCameraPageZ ].IsLoaded == false ||
						pages[ currentCameraPageX ][ currentCameraPageZ ].IsCameraIn( pos ) != CameraPageState.Inside  
					  )
					)
				{
					// JEFF
					// convert camera pos to page index
					long i, j;
					Data2DManager.Instance.GetPageIndices(pos, out i, out j);
					//if (((currentCameraPageX != i) || (currentCameraPageZ != j)) &&
					Debug.Assert( i < width && j < heigth , "Page Indices out of bounds" );
					if ((pages[ i ] [ j ] == null) ||
						(pages[ i ][ j ].IsCameraIn ( pos ) != CameraPageState.Outside) 
					   )
					{
						long adjpages = Options.Instance.Max_Adjacent_Pages;
						long prepages = Options.Instance.Max_Preload_Pages;
						long w = width;
						long h = heigth;

						// We must load the next visible landscape pages, 
						// and unload the last visibles
						// check the landscape boundaries	

						long iniX = ((int)(i - adjpages) > 0)? i - adjpages: 0;
						long iniZ = ((int)(j - adjpages) > 0)? j - adjpages: 0;
						long finX = (i + adjpages >= w )? w - 1: i + adjpages;
						long finZ = (j + adjpages >= h )? h - 1: j + adjpages;

						long preIniX = ((int)(i - prepages) > 0)? i - prepages: 0;
						long preIniZ = ((int)(j - prepages) > 0)? j - prepages: 0;
						long preFinX = (i + prepages >= w)?  w - 1: i + prepages;
						long preFinZ = (j + prepages >= h )? h - 1: j + prepages;


						// update the camera page position
						currentCameraPageX = i;
						currentCameraPageZ = j;
				

						// Have the current page be loaded now
						if ( pages[ currentCameraPageX ][ currentCameraPageZ ].IsPreLoaded == false )
						{
							pages[ currentCameraPageX ][ currentCameraPageZ ].Preload();
						}
						if ( pages[ currentCameraPageX ][ currentCameraPageZ ].IsLoaded == false )
						{
							pages[ currentCameraPageX ][ currentCameraPageZ ].Load(ref sceneRoot );
						}
						// Queue the rest

						// Loading and unloading must be done one by one to avoid FPS drop, so they are queued.
						// No need to queue for preload since _ProcessLoading will do it in Load if required.								
		               
						// post unload as required
						for( j  = 0; j < preIniZ; j++)
						{
							for( i = 0 ; i < preIniX; i++)
							{
								pageUnloadQueue.Push( pages[ i ][ j ] );
							}
						}
						for( j  = preFinZ + 1; j < h; j++)
						{
							for( i = preFinX + 1; i < w; i++)
							{
								pageUnloadQueue.Push( pages[ i ][ j ] );
							}
						}            

						// Preload as required
						for ( j = preIniZ; j < iniZ; j++ )
						{
							for ( i = preIniX; i < iniX; i++ )
							{			
								pagePreloadQueue.Push( pages[ i ][ j ] );
							}
						}
						for ( j = finZ; j <= preFinZ; j++ )
						{
							for ( i = finX; i <= preFinX; i++ )
							{			
								pagePreloadQueue.Push( pages[ i ][ j ] );
							}
						}                    
						// load as required
						for ( j = iniZ; j <= finZ; j++ )
						{
							for ( i = iniX; i <= finX; i++ )
							{			
								if ( !pages[ i ][ j ].IsLoaded )
									pageLoadQueue.Push( pages[ i ][ j ] );
							}
						}
					}
				}
				// Update the last camera position
				lastCameraPos = pos;
				Tile.Tile t = GetTile (pos,(long)currentCameraPageX, (long)currentCameraPageZ);
				if (t != null)
				{
					Tile.TileInfo CurrentTileInfo  = t.Info;
					if (CurrentTileInfo != null)
					{
						currentCameraTileX = CurrentTileInfo.TileX;
						currentCameraTileZ = CurrentTileInfo.TileZ;
					}
				}
		       
			}

			// Check for visibility
			Camera plsmCam = (cam);
			for ( long j = 0; j < heigth; j++ )
			{
				for ( long i = 0; i < width; i++ )
				{
					pages[ i ][ j ].Notify (pos, plsmCam);
				}
			}

			// Preload, load, unload and post unload as required
			this.processLoading();
		}


		public long GetCurrentCameraPageX()		{ return this.currentCameraPageX;		}
		public long GetCurrentCameraPageZ()		{ return this.currentCameraPageZ;		}
		public long GetCurrentCameraTileX()		{ return this.currentCameraTileX;		}
		public long GetCurrentCameraTileZ()		{ return this.currentCameraTileZ;		}


		public int GetPagePreloadQueueSize()    { return this.pagePreloadQueue.Size;    }
		public int GetPageLoadQueueSize()       { return this.pageLoadQueue.Size;       }
		public int GetPageUnloadQueueSize()     { return this.pageUnloadQueue.Size;     }
		public int GetPagePostUnloadQueueSize() { return this.pagePostunloadQueue.Size; }


		public Page GetPage  ( long i ,  long j)
		{
			return pages[i][j];
		}

		public Tile.Tile GetTile  ( Vector3 pos)
		{
			int pSize = (int)Options.Instance.PageSize - 1;
			Vector3 scale = Options.Instance.Scale;
			int w = (int) (Options.Instance.World_Width * 0.5f);
			int h = (int) (Options.Instance.World_Height * 0.5f);

			Vector3 TileRefPos = new Vector3   (pos.x / scale.x,
												pos.y,
												pos.z / scale.z);

			int pagex = (int) (TileRefPos.x / pSize + w);
			int pagez = (int) (TileRefPos.z / pSize + h );

			// make sure indices are not negative or outside range of number of pages
			if (pagex < 0 || pagex >= w*2 || pagez >= h*2 || pagez < 0)
				return null;
			else
			{
					int tSize = (int)Options.Instance.TileSize;

					int tilex = (int) ((TileRefPos.x - ((pagex - w) * pSize)) / tSize);
					int tilez = (int) ((TileRefPos.z - ((pagez - h) * pSize)) / tSize);

					pSize = (pSize / tSize) - 1;
					if (tilex > pSize || tilex < 0 || 
						tilez > pSize || tilez < 0)
						return null;               

					return pages[pagex][pagez].GetTile ((long)tilex, (long)tilez);
			}
		}

		public Tile.Tile GetTile(Vector3 pos, long pagex, long pagez)
		{
 			int pageSize = (int) Options.Instance.PageSize - 1;
			Vector3 scale = Options.Instance.Scale;
			int w = (int)(Options.Instance.World_Width * 0.5F);
			int h = (int)(Options.Instance.World_Height * 0.5F);
			Vector3 TileRefPos = new Vector3 ( pos.x / scale.x , pos.y, pos.z / scale.z );
			int pageX = (int) (TileRefPos.x / pageSize + w );
			int pageZ = (int) (TileRefPos.z / pageSize + h );

			// make sure indices are not negative or outside range of number of pages
			if ( pageX < 0 || pageX >= w * 2 || pageZ >= h * 2 || pageZ < 0 )
				return null;
			else
			{
				int tileSize = (int) Options.Instance.TileSize;
				int tilex = (int) ( ( TileRefPos.x - ( ( pageX  - w ) * pageSize ) ) / tileSize );
				int tilez = (int) ( ( TileRefPos.z - ( ( pageZ - h ) * pageSize ) ) / tileSize );

				pageSize = ( pageSize / tileSize ) -1;
				if ( tilex > pageSize || tilex < 0 || tilez > pageSize || tilez < 0 )
					return null;
				return pages[pageX][pageZ].GetTile(tilex, tilez);
			}
		}

		public Tile.Tile GetTileUnscaled(Vector3 pos)
		{
			long pSize = Options.Instance.PageSize;
			long w = (long) ((float)Options.Instance.World_Width * 0.5f);
			long h = (long) ((float)Options.Instance.World_Height * 0.5f);

			Vector3 TileRefPos = pos;

			long pagex =  (long)(TileRefPos.x / pSize + w);
			long pagez =  (long)(TileRefPos.z / pSize + h);

			// make sure indices are not negative or outside range of number of pages
			if (pagex < 0 || pagex >= w*2 || pagez >= h*2 || pagez < 0)
				return null;
			else
			{
				long tSize = Options.Instance.TileSize;

				long tilex = (long)((TileRefPos.x - ((pagex - w) * pSize)) / tSize);
				long tilez = (long)((TileRefPos.z - ((pagez - h) * pSize)) / tSize);

				pSize = (pSize / tSize) - 1;
				if (tilex > pSize)
					tilex = pSize;
				else if (tilex < 0) 
					tilex = 0;

				if (tilez > pSize)
					tilez = pSize;
				else if (tilez < 0) 
					tilez = 0;  

				return pages[pagex][pagez].GetTile( tilex, tilez );
			} 
		}


		protected void processLoading( )
		{
			// Preload, load, unload and post unload as required
			/*		We try to PreLoad
				If no preload is required, then try to Load
				If no load is required, then try to UnLoad
				If no unload is required, then try to PostUnLoad.
			*/
			Page e;
			e = pagePreloadQueue.Pop();
			if ( e != null )
			{
				e.Preload();
			}
			else
			{
				e = pageLoadQueue.Pop();
				if ( e != null )
				{
					e.Load( ref sceneRoot );
					if ( e.IsLoaded == false )
					{
						if ( e.IsPreLoaded == false )
						{
							// If we are not PreLoaded, then we must preload
							pagePreloadQueue.Push( e );
						}
						// If we are not loaded then queue again, since maybe we are not preloaded.
						pageLoadQueue.Push( e );
					}
				}
				else
				{
					e = pageUnloadQueue.Pop();
					if ( e != null )
					{
						e.Unload();
					}
					else
					{
						e = pagePostunloadQueue.Pop();
						if ( e != null )
						{
							e.PostUnload();
							if ( e.IsPreLoaded)
							{
								// If we are not post unloaded the queue again
								pagePostunloadQueue.Push( e );
							}
						}
					}
				}
			}
			// load some renderables
			RenderableManager.Instance.ExecuteRenderableLoading();
		}
	}

}

