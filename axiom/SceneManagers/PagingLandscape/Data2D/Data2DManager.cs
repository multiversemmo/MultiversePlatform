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



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;



using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Tile;

using Axiom.SceneManagers.PagingLandscape.Page;



#endregion Using Directives



#region Versioning Information

/// File								Revision

/// ===============================================

/// OgrePagingLandScapeData2DManager.h		1.9

/// OgrePagingLandScapeData2DManager.cpp	1.14

/// 

#endregion



namespace Axiom.SceneManagers.PagingLandscape.Data2D

{

	/// <summary>

	/// Summary description for Data2DManager.

	/// </summary>

	public class Data2DManager: IDisposable

	{

		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		private Data2DManager() 

		{

			long w = Options.Instance.World_Width;
			long h = Options.Instance.World_Height;

			//Setup the page array
			long i, j;
			data2D = new Data2DPages();
			for ( i = 0; i < w; i++)
			{
				Data2DRow dr = new Data2DRow();
				data2D.Add( dr );
				for ( j = 0; j < w; j++ ) dr.Add( null );
			}

			//Populate the page array
			if ( Options.Instance.Data2DFormat == "HeightField" )
			{
				for ( j = 0; j < h; j++ )
				{
					for ( i = 0; i < w; i++ )
					{
						data2D[ i ][ j ] = new Data2D_HeightField();
					}
				}
			}

            else if (Options.Instance.Data2DFormat == "ClientGen") {

                for (j = 0; j < h; j++) {

                    for (i = 0; i < w; i++) {

                        data2D[i][j] = new Data2D_ClientGen((int)i, (int)j);

                    }

                }

            }

/*
			else if ( Options.Instance.Data2DFormat == "HeightFieldTC" )
			{
				for ( j = 0; j < h; j++ )
				{
					for ( i = 0; i < w; i++ )
					{
							Data2D data;

							data	= new Data2D_HeightFieldTC();
							data2D[ i ][ j ] = data;
					}
				}
			}
			else if ( Options.Instance.Data2DFormat == "SplineField" )
			{
				for ( j = 0; j < h; j++ )
				{
					for ( i = 0; i < w; i++ )
					{
						eData2D data;

						data = new Data2D_Spline();
						data2D[ i ][ j ] = data;
					}
				}
			}
*/
			else
			{
		       
				throw new Exception( "PageData2D not supplied!");
		        
			}
			// when data is not yet loaded it gives the absolute maximum possible
			maxHeight = data2D[ 0 ][ 0 ].MaxHeight;
		}





		private static Data2DManager instance = null;



		public static Data2DManager Instance 

		{

			get 

			{

				if ( instance == null ) instance = new Data2DManager();

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				data2D.Clear();

				instance = null;

			}

		}



		#endregion IDisposable Implementation



		#region Fields



		Data2DPages data2D;
		float maxHeight;

		#endregion Fields



		public void Load(  long dataX,  long dataZ )
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( !data.IsLoaded )
			{
				data.Load( dataX, dataZ );
			}		

		}
        
		public void Unload(  long dataX,  long dataZ )
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
				data.Unload();
			}
		}
        
		public bool IsLoaded(  long dataX,  long dataZ )
		{
			return data2D[ dataX ][ dataZ ].IsLoaded;
		}
        
        
		public float GetHeight(  long dataX,  long dataZ, float x,  float z )
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
				return data.GetHeight(x, z);
			}
			return 0.0f;
		}
        
		public float GetHeight(  long dataX,  long dataZ, long x,  long z )
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
				return data.GetHeight(x, z);
			}
			return 0.0f;
		}
        
		public float GetHeightAtPage( long dataX,  long dataZ, float x,  float z)
		{
			Data2D data = null;

			float lX = x;
			float lZ = z;
			float pSize = Options.Instance.PageSize;
			//check if we have to change current page
			if ( lX < 0.0f )
			{ 
				if ( dataX == 0 )
					lX = 0.0f;
				else
				{
					data = data2D[ dataX - 1 ][ dataZ ];
					if  ( data.IsLoaded )
					{
						lX = (float) (pSize - 1);
					}
					else
					{
						lX = 0.0f;
						data = data2D[ dataX ][ dataZ ];
					}
				}
        
			}
			else if (lX > pSize)
			{
				if ( dataX == Options.Instance.World_Width - 1 )
					lX = (float) (pSize);
				else
				{
					data = data2D[ dataX + 1 ][ dataZ ];
					if  ( data.IsLoaded )
					{
						lX = 0.0f;
					}
					else
					{
						lX = (float) (pSize);
						data = data2D[ dataX ][ dataZ ];
					}
				}
			}

			if ( lZ < 0.0f )
			{
				if ( dataZ == 0 )
					lZ = 0.0f;
				else
				{
					data = data2D[ dataX ][ dataZ - 1 ];
					if  ( data.IsLoaded )
					{
						lZ = (float)(pSize);
					}
					else
					{
						lZ = 0.0f;
						data = data2D[ dataX ][ dataZ ];
					}
				}
			}
			else if (lZ > pSize)
			{
				if ( dataZ == Options.Instance.World_Height - 1)
					lZ = (float) (pSize);
				else
				{
					data = data2D[ dataX ][ dataZ + 1 ];
					if  ( data.IsLoaded )
					{
						lZ = 0.0f;
					}
					else
					{
						lZ = (float) (pSize);
						data = data2D[ dataX ][ dataZ ];
					}
				}
			}
			if ( data == null )
				data = data2D[ dataX ][ dataZ ];
    
			if ( data.IsLoaded )
			{
				return data.GetHeight (lX, lZ);
			}
			return 0.0f;
		}
        
		public float GetHeightAtPage( long dataX,  long dataZ, int x,  int z)
		{
			Data2D data = data2D[ dataX ][ dataZ ];
    
			if ( data.IsLoaded )
			{
				int lX = x;
				int lZ = z;
				int pSize = (int)Options.Instance.PageSize;
				//check if we have to change current page
				if ( lX < 0 )
				{ 
					if ( dataX == 0 )
						lX = 0;
					else
					{
						data = data2D[ dataX - 1 ][ dataZ ];
						if  ( data.IsLoaded )
						{
							lX = pSize;
						}
						else
						{
							lX = 0;
							data = data2D[ dataX ][ dataZ ];
						}
					}
          
				}
				else if (lX > pSize)
				{
					if ( dataX == Options.Instance.World_Width )
																						 lX = pSize;
				else
				{
					data = data2D[ dataX + 1 ][ dataZ ];
					if  ( data.IsLoaded )
					{
						lX = 0;
					}
					else
					{
						lX = pSize;
						data = data2D[ dataX ][ dataZ ];
					}
				}
				}

				if ( lZ < 0 )
				{
					if ( dataZ == 0 )
						lZ = 0;
					else
					{
						data = data2D[ dataX ][ dataZ - 1 ];
						if  ( data.IsLoaded )
						{
							lZ = pSize;
						}
						else
						{
							lZ = 0;
							data = data2D[ dataX ][ dataZ ];
						}
					}
				}
				else if (lZ > pSize)
				{
					if ( dataZ == Options.Instance.World_Height )
																						  lZ = pSize;
				else
				{
					data = data2D[ dataX ][ dataZ + 1 ];
					if  ( data.IsLoaded )
					{
						lZ = 0;
					}
					else
					{
						lZ = pSize;
						data = data2D[ dataX ][ dataZ ];
					}
				}
				}
				return data.GetHeight (x, z);
			}
			return 0.0f;
		}
        
		public void DeformHeight( Vector3 deformationPoint,	float modificationHeight, TileInfo info)
		{
			long pSize = Options.Instance.PageSize;
			long pX = info.PageX;
			long pZ = info.PageZ;
			long wL = Options.Instance.World_Width;
			long hL = Options.Instance.World_Height;
			// adjust x and z to be local to page
			long x = (long) ((deformationPoint.x ) - ((pX - wL * 0.5f) * (pSize)));
			long z = (long) ((deformationPoint.z ) - ((pZ - hL * 0.5f) * (pSize)));

			Data2D data = data2D[ pX ][ pZ ];
			if ( data.IsLoaded )
			{
				float h = data.DeformHeight (x, z, modificationHeight);
		   
				// If we're on a page edge, we must duplicate the change on the 
				// neighbour page (if it has one...)
				if (x == 0 && pX != 0)
				{
					data = data2D[ pX - 1 ][ pZ ];
					if ( data.IsLoaded )
					{
						data.SetHeight (Options.Instance.PageSize - 1, z, h);
					} 
				}
				if (x == pSize - 1 && pX < wL - 1)
				{
					data = data2D[ pX + 1 ][ pZ ];
					if ( data.IsLoaded )
					{
						data.SetHeight (0, z, h);
					}    
				}

				if (z == 0 && pZ != 0)
				{
		            
					data = data2D[ pX ][ pZ - 1 ];
					if ( data.IsLoaded )
					{
						data.SetHeight (x, Options.Instance.PageSize - 1, h);
					} 
				}
				if (z == pSize - 1 && pZ < hL - 1)
				{
					data = data2D[ pX ][ pZ  + 1];
					if ( data.IsLoaded )
					{
						data.SetHeight (x, 0, h);
					} 
				} 
			}
		}
        
		public bool AddNewHeight( Sphere newSphere )
		{
			long x, z;
			// Calculate where is going to be placed the new height
			this.GetPageIndices( newSphere.Center, out x, out z);
			// TODO: DeScale and add the sphere to all the necessary pages

			//place it there
			return data2D[ x ][ z ].AddNewHeight(newSphere);
		}
        
		public bool RemoveNewHeight( Sphere oldSphere )
		{
			long x, z;
			// Calculate where is going to be placed the new height
			GetPageIndices( oldSphere.Center, out x,  out z);
			// TODO: DeScale and add the sphere to all the necessary pages

			//remove it
			return data2D[ x ][ z ].RemoveNewHeight(oldSphere);
		}
        
		//This function will return the max possible value of height base on the current 2D Data implementation
		public float GetMaxHeight( long x, long z)
		{
			Data2D data = data2D[ x ][ z ];  
			if ( data.IsLoaded )
			{
				return data2D[ x ][ z ].MaxHeight;
			}
			return maxHeight;
		}
        
		public float GetMaxHeight()
		{
				return maxHeight;
		}


		/** Get the real world height at a particular position
			@remarks
			Method is used to get the terrain height at a world position based on x and z.
			This method just figures out what page the position is on and then asks the page node
			to do the dirty work of getting the height.
            
			@par
			the float returned is the real world height based on the scale of the world.  If the height could
			not be determined then -1 is returned and this would only occur if the page was not preloaded or loaded
            
			@param x  x world position
			@param z  z world position
		*/
		public float GetRealWorldHeight( float x,  float z)
		{
			// figure out which page the point is on
			Vector3 pos = new Vector3(x, 0, z);
			long dataX, dataZ;
			GetPageIndices(pos, out dataX, out dataZ);

			if ( data2D[dataX][dataZ].Dynamic ) 

			{
				return GetRealPageHeight (x, z, dataX, dataZ, 0);
			} 

			else 

			{
				if ( !(data2D[ dataX ][ dataZ ].IsLoaded ))
					return 0.0f;

				// figure out which tile the point is on
				Tile.Tile t = PageManager.Instance.GetTile (pos, dataX, dataZ);
				long Lod = 0;
				if (t != null && t.IsLoaded )
					Lod = t.Renderable.RenderLevel;

				return GetRealPageHeight (x, z, dataX, dataZ, Lod);
			}
		}

		public float GetRealWorldHeight( float x,  float z, TileInfo info)
		{
			// figure out which page the point is on
			long dataX = info.PageX;
			long dataZ = info.PageZ;

			if ( ! (data2D[ dataX ][ dataZ ].IsLoaded ))
				return 0.0f;

			Tile.Tile t = PageManager.Instance.GetPage (dataX, dataZ ).GetTile (info.TileX, info.TileZ);
			long Lod = 0;
			if (t != null && t.IsLoaded )
				Lod = t.Renderable.RenderLevel;

			return GetRealPageHeight (x, z, dataX, dataZ, Lod);
		}
        
		public float GetRealPageHeight ( float x,  float z,  long pageX,  long pageZ,  long Lod)
		{
			// scale position from world to page scale
			float localX = x / Options.Instance.Scale.x;
			float localZ = z / Options.Instance.Scale.z;

			// adjust x and z to be local to page
			long pSize = Options.Instance.PageSize - 1;
			localX -= (float)(pageX - Options.Instance.World_Width  * 0.5f) * pSize;
			localZ -= (float)(pageZ - Options.Instance.World_Height * 0.5f) * pSize;

			// make sure x and z do not go outside the world boundaries
			if (localX < 0)
				localX = 0;
			else if (localX > pSize) 
				localX = pSize;

			if (localZ < 0)
				localZ = 0;
			else if (localZ > pSize)
				localZ = pSize;

			// find the 4 vertices that surround the point
			// use LOD info to determine vertex spacing - this is passed into the method
			// determine vertices on left and right of point and top and bottom
			// don't access VBO since a big performance hit when only 4 vertices are needed
			int vertex_spread = 1 << (int)Lod;

			// find the vertex to the bottom left of the point
			int bottom_left_x = ((int)(localX / vertex_spread)) * vertex_spread;
			int bottom_left_z = ((int)(localZ / vertex_spread)) * vertex_spread;

			// find the 4 heights around the point
			Data2D data = data2D[ pageX ][ pageZ ];
			float bottom_left_y  = data.GetHeight(bottom_left_x                , bottom_left_z);
			float bottom_right_y = data.GetHeight(bottom_left_x + vertex_spread, bottom_left_z);
			float top_left_y     = data.GetHeight(bottom_left_x                , bottom_left_z + vertex_spread);
			float top_right_y    = data.GetHeight(bottom_left_x + vertex_spread, bottom_left_z + vertex_spread);

			float x_pct = (localX - (float)bottom_left_x) / (float)vertex_spread;
			float z_pct = (localZ - (float)bottom_left_z) / (float)vertex_spread;

			//bilinear interpolate to find the height.

			// figure out which 3 vertices are closest to the point and use those to form triangle plane for intersection
			// Triangle strip has diagonal going from bottom left to top right
			if ((x_pct - z_pct) >= 0)
			{
				return ( (bottom_left_y  + (bottom_right_y - bottom_left_y ) *  x_pct + (top_right_y - bottom_right_y) * z_pct));
			}
			else
			{
				return ( (top_left_y  + (top_right_y - top_left_y ) *  x_pct + (bottom_left_y - top_left_y) * (1 - z_pct)));
			}
		}
        
		public ColorEx GetCoverageAt(  long dataX,  long dataZ,  float x,  float z )
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
				return data.GetCoverage(x, z);
			}
			return ColorEx.White;
		}

		public ColorEx GetBaseAt(  long dataX,  long dataZ,  float x,  float z )
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
				return data.GetBase(x, z);
			}
			return ColorEx.White;
		}

        
		public Vector3 GetNormalAt(  long dataX,  long dataZ,  float x,  float z)
		{
			Data2D data = data2D[ dataX ][ dataZ ];
			if ( data.IsLoaded )
			{
#if !_LOADEDNORM
				return data.GetNormalAt (x, z);
#else
			{
					// First General method : (9 adds and 6 muls + a normalization)
					//        *---v3--*
					//        |   |   |
					//        |   |   |
					//        v1--X--v2
					//        |   |   |
					//        |   |   |
					//        *---v4--*
					//
					//        U = v2 - v1;
					//        V = v4 - v3;
					//        N = Cross(U, V);
					//        N.normalise;
					//
					// BUT IN CASE OF A HEIGHTMAP : 
					//
					//   if you do some math by hand before you code, 
					//   you can see that N is immediately given by 
					//  Approximation (2 adds and a normalization)
					// 
					//        N = Vector3(z[x-1][y] - z[x+1][y], z[x][y-1] - z[x][y+1], 2); 
					//        N.normalise();
					//
					// or even using SOBEL operator VERY accurate! 
					// (14 adds and a normalization)
					//
					//       N = Vector3 (z[x-1][y-1] + z[x-1][y] + z[x-1][y] + z[x-1][y+1] - z[x+1][y-1] - z[x+1][y] - z[x+1][y] - z[x+1][y+1], 
					//                     z[x-1][y-1] + z[x][y-1] + z[x][y-1] + z[x+1][y-1] - z[x-1][y+1] - z[x][y+1] - z[x][y+1] - z[x+1][y+1], 
					//                     8);
					//       N.normalize();

		        
					// Fast SOBEL filter
					Vector3 result = new Vector3
									(this.GetHeightAtPage( dataX, dataZ, x - 1.0F, z    ) - this.GetHeightAtPage( dataX, dataZ, x + 1.0F, z    ),
									 2.0f,
									 this.GetHeightAtPage( dataX, dataZ, x,      z - 1.0F) - this.GetHeightAtPage( dataX, dataZ, x    , z + 1.0F)); 

					result.Normalize();

					return result;
				}
#endif
			}
			return Vector3.UnitY;
		}
        
		// JEFF
		/** Get the Page indices from a world position vector
			@remarks
			Method is used to find the Page indices using a world position vector.
			Beats having to iterate through the Page list to find a page at a particular
			position in the world.
			@param pos the world position vector. Only components x and z are used
			@param x	result placed in reference to the x index of the page
			@param z	result placed in reference to the z index of the page
		*/
		public void GetPageIndices( Vector3 pos, out long x, out long z)
		{
			long w = Options.Instance.World_Width;
			long h = Options.Instance.World_Height;

			x = (long)(pos.x / Options.Instance.Scale.x / (Options.Instance.PageSize - 1.0) + w * 0.5f);
			z = (long)(pos.z / Options.Instance.Scale.z / (Options.Instance.PageSize - 1.0) + h * 0.5f);

			// make sure indices are not negative or outside range of number of pages
			if (x >= w) 
			{
				x = w - 1;
			} 

			else if ( x < 0 ) 

			{
				x = 0;
			}
			if (z >= h) 
			{
				z = h - 1;
			} 

			else if ( z < 0 ) 

			{
				z = 0;
			}
		}
        
		public Data2D GetData2D ( long x,  long z)
		{
			Data2D data = data2D[ x ][ z ];
			return ( data.IsLoaded )? data: null;
		}
        
	}

}

