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

#region Namespace Declarations

using System;
using System.Collections;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;

using Axiom.SceneManagers;
using Axiom.SceneManagers.PagingLandscape.Data2D;
using Axiom.SceneManagers.PagingLandscape.Renderable;
using Axiom.SceneManagers.PagingLandscape.Tile;
using Axiom.SceneManagers.PagingLandscape.Page;
using Axiom.SceneManagers.PagingLandscape.Query;
using Axiom.SceneManagers.PagingLandscape.Texture;

#endregion Using Directives

#region Versioning Information
/// File								Revision
/// ===============================================
/// OgrePagingLandScapeSceneManager.h		1.7
/// OgrePagingLandScapeSceneManager.cpp		1.9
/// 
#endregion

namespace Axiom.SceneManagers.PagingLandscape
{
	public enum Neighbor : int
	{
		North = 0,
		South = 1,
		East = 2,
		West = 3,
		Here = 4
	};

	/// <summary>
	///		This is a basic SceneManager for organizing LandscapeRenderables into a total Landscape.
	///		It loads a Landscape from a XML config file that specifices what textures/scale/virtual window/etc to use.
	/// </summary>
	public class SceneManager :  Axiom.Core.SceneManager
	{
		#region Fields

		/// <summary>
		///  All the plugin options are handle here.
		/// </summary>
		protected Options options;

		/// <summary>
		///  Landscape 2D Data manager.
		/// This class encapsulate the 2d data loading and unloading
		/// </summary>
		protected Data2DManager data2DManager;

		/// <summary>
		/// Landscape Texture manager.
		/// This class encapsulate the texture loading and unloading
		/// </summary>
		protected Texture.TextureManager textureManager;

		/// <summary>
		/// Landscape tiles manager to avoid creating and deleting terrain tiles.
		/// They are created at the plugin start and destroyed at the plugin unload.
		/// </summary>
		protected TileManager tileManager;

		/// <summary>
		/// Landscape Renderable manager to avoid creating and deleting renderables.
		/// They are created at the plugin start and destroyed at the plugin unload.
		/// </summary>
		protected Renderable.RenderableManager renderableManager;

		/// <summary>
		/// Landscape pages for the terrain.
		/// </summary>
		protected Page.PageManager pageManager;

		protected bool needOptionsUpdate;

		/// <summary>
		///  flag to indicate if the world geometry was setup
		/// </summary>
		protected bool worldGeomIsSetup;

		protected TileInfo impactInfo;
		protected Vector3 impact;

		#endregion Fields

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public SceneManager( )
		{
			tileManager = null;
			renderableManager = null;
			textureManager = null;
			data2DManager = null;
			pageManager = null;
			needOptionsUpdate = false;
			worldGeomIsSetup = false;
		}
		//~PagingLandScapeSceneManager( );
		#endregion Constructor

		#region Methods

		/// <summary>
		/// Creates a specialized Camera 
		/// </summary>
		/// <param name="name">Camera name</param>
		/// <returns>camera</returns>
		public override Axiom.Core.Camera CreateCamera( string name )
		{
			Axiom.Core.Camera c = new Camera( name, this );
			this.cameraList.Add( name, c  );
			return c;
		}


		/// <summary>
		/// Loads the LandScape using parameters in the given config file.
		/// </summary>
		/// <param name="filename"></param>
		public override void LoadWorldGeometry( string filename )
		{
			if ( worldGeomIsSetup == true )
			{
				ClearScene();
			}
			// Load the configuration file
			options = PagingLandscape.Options.Instance;
			options.Load( filename );

			// Create the Tile and Renderable and 2D Data Manager
			tileManager = TileManager.Instance;
			renderableManager = RenderableManager.Instance;
			textureManager = Texture.TextureManager.Instance;
			data2DManager = Data2DManager.Instance;
			pageManager = new PageManager( this.rootSceneNode );
			worldGeomIsSetup = true;		
		}


		/// <summary>
		/// Empties the entire scene, inluding all SceneNodes, Cameras, Entities and Lights etc.
		/// </summary>
		public override void ClearScene()
		{
			if ( worldGeomIsSetup == true )
			{
				worldGeomIsSetup = false;

				// Delete the Managers
				if ( pageManager != null )
				{
					pageManager.Dispose();
					pageManager = null;
				}
				if ( tileManager != null ) 
				{
					tileManager.Dispose();
					tileManager = null;
				}
				if ( renderableManager != null)
				{
					renderableManager.Dispose();
					renderableManager = null;
				}
				if ( textureManager != null )
				{
					textureManager.Dispose();
					textureManager = null;
				}
				if ( data2DManager != null)
				{
					data2DManager.Dispose();
					data2DManager = null;
				}
			}
			//Call the default
			base.ClearScene();
		}


		/// <summary>
		/// Method for setting a specific option of the Scene Manager. These options are usually
		/// specific for a certain implemntation of the Scene Manager class, and may (and probably
		/// will) not exist across different implementations.
		/// </summary>
		/// <param name="strKey">The name of the option to set</param>
		/// <param name="pValue">A pointer to the value - the size should be calculated by the scene manager based on the key</param>
		/// <returns>On success, true is returned. On failure, false is returned.</returns>
		public bool SetOption( string strKey, object pValue )
		{
			if ( strKey == "AddNewHeight" )
			{
				return data2DManager.AddNewHeight( (Sphere) pValue );
			}
			if ( strKey == "RemoveNewHeight" )
			{
				return data2DManager.RemoveNewHeight( (Sphere)  pValue );
			}
			needOptionsUpdate = options.setOption( strKey, pValue );
			return needOptionsUpdate;
		}


		/// <summary>
		/// Method for getting the value of an implementation-specific Scene Manager option.
		/// </summary>
		/// <param name="strKey">The name of the option</param>
		/// <param name="pDestValue">A pointer to a memory location where the value will	be copied. Currently, the memory will be allocated by the scene manager, but this may change</param>
		/// <returns>
		/// On success, true is returned and pDestValue points to the value of the given 
		/// option.
		/// On failure, false is returned and pDestValue is set to NULL.
		/// </returns>
		public bool GetOption( string strKey, ref object pDestValue )
		{
			#region Page Info Options
			pDestValue = 0;
			// PAGEINFO
			if ( strKey == "CurrentCameraPageX" )
			{
				pDestValue = pageManager.GetCurrentCameraPageX();
				return true;
			}
			if ( strKey == "CurrentCameraPageZ" )
			{
				pDestValue = pageManager.GetCurrentCameraPageZ();
				return true;
			}
			if ( strKey == "PagePreloadQueue" )
			{
				pDestValue = pageManager.GetPagePreloadQueueSize();
				return true;
			}
			if ( strKey == "PageLoadQueue" )
			{
				pDestValue = pageManager.GetPageLoadQueueSize();
				return true;
			}
			if ( strKey == "PageUnloadQueue" )
			{
				pDestValue = pageManager.GetPageUnloadQueueSize();
				return true;
			}
			if ( strKey == "PagePostUnloadQueue" )
			{
				pDestValue = pageManager.GetPagePostUnloadQueueSize();
				return true;
			}
			if ( strKey == "PagePostUnloadQueue" )
			{
				pDestValue = pageManager.GetPagePostUnloadQueueSize();
				return true;
			}
			#endregion Page Info Options

			#region Tile Info Options
			//TILES INFO
			if ( strKey == "MaxNumTiles" )
			{
				pDestValue = tileManager.NumTiles();
				return true;
			}
			if ( strKey == "TileFree" )
			{
				pDestValue = tileManager.NumFree();
				return true;
			}
			if ( strKey == "CurrentCameraTileX" )
			{
				pDestValue = pageManager.GetCurrentCameraTileX();
				return true;
			}
			if ( strKey == "CurrentCameraTileZ" )
			{
				pDestValue = pageManager.GetCurrentCameraTileZ();
				return true;
			}
			#endregion Tile Info Options

			#region Renderables Info Options
			//RENDERABLES INFO
			if ( strKey == "MaxNumRenderables" )
			{
				pDestValue = renderableManager.RenderablesCount;
				return true;
			}
			if ( strKey == "RenderableFree" )
			{
				pDestValue = renderableManager.FreeCount;
				return true;
			}
			if ( strKey == "RenderableLoading" )
			{
				pDestValue = renderableManager.LoadingCount;
				return true;
			}
			if ( strKey == "VisibleRenderables" )
			{
				pDestValue = renderableManager.VisibleCount;
				return true;
			}

			#endregion

			#region Impact Info
			// IMPACT INFO
			if ( strKey == "Impact" )
			{
				pDestValue = impact;
				return true;
			}
			if ( strKey == "ImpactPageX" )
			{
				pDestValue = impactInfo.PageX;
				return true;
			}
			if ( strKey == "ImpactPageZ" )
			{
				pDestValue = impactInfo.PageZ;
				return true;
			} 
			if ( strKey == "ImpactTileX" )
			{
				pDestValue = impactInfo.TileZ;
				return true;
			} 
			if ( strKey == "ImpactTileZ" )
			{
				pDestValue = impactInfo.TileZ;
				return true;
			}
			if ( strKey == "numModifiedTile" )
			{
				pDestValue = renderableManager.RenderablesCount;
				return true;
			}
			return false;
			#endregion
			//return options.getOption( strKey, pDestValue );
		}


		/// <summary>
		///  Method for verifying weather the scene manager has an implementation-specific option.
		/// </summary>
		/// <param name="strKey">The name of the option to check for.</param>
		/// <returns>If the scene manager contains the given option, true is returned.</returns>
		/// <remarks>If it does not, false is returned.</remarks>
		public bool HasOption( string strKey )
		{
			if ( strKey == "AddNewHeight" )
			{
				return true;
			}
			if ( strKey == "RemoveNewHeight" )
			{
				return true;
			}
			if ( strKey == "CurrentCameraPageX" )
			{
				return true;
			}
			if ( strKey == "CurrentCameraPageZ" )
			{
				return true;
			}
			if ( strKey == "MaxNumTiles" )
			{
				return true;
			}
			if ( strKey == "TileFree" )
			{
				return true;
			}
			if ( strKey == "MaxNumRenderables" )
			{
				return true;
			}
			if ( strKey == "RenderableFree" )
			{
				return true;
			}
			if ( strKey == "RenderableLoading" )
			{
				return true;
			}
			if ( strKey == "PagePreloadQueue" )
			{
				return true;
			}
			if ( strKey == "PageLoadQueue" )
			{
				return true;
			}
			if ( strKey == "PageUnloadQueue" )
			{
				return true;
			}
			if ( strKey == "PagePostUnloadQueue" )
			{
				return true;
			}
			return options.hasOption( strKey );
		}


		/// <summary>
		/// Method for getting all possible values for a specific option. When this list is too large
		/// (i.e. the option expects, for example, a float), the return value will be true, but the
		/// list will contain just one element whose size will be set to 0.
		/// Otherwise, the list will be filled with all the possible values the option can
		/// accept.
		/// </summary>
		/// <param name="strKey">The name of the option to get the values for.</param>
		/// <param name="refValueList">A reference to a list that will be filled with the available values.</param>
		/// <returns>On success (the option exists), true is returned.On failure, false is returned.</returns>
		public bool GetOptionValues( string strKey, ArrayList refValueList )
		{
			if ( strKey == "CurrentCameraPageX" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "CurrentCameraPageZ" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "MaxNumTiles" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "TileFree" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "MaxNumRenderables" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "RenderableFree" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "RenderableLoading" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "PagePreloadQueue" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "PageLoadQueue" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "PageUnloadQueue" )
			{
				refValueList.Add(new object());
				return true;
			}
			if ( strKey == "PagePostUnloadQueue" )
			{
				refValueList.Add(new object());
				return true;
			}
			return options.getOptionValues( strKey, refValueList );
		}


		/// <summary>
		/// Method for getting all the implementation-specific options of the scene manager.
		/// </summary>
		/// <param name="refKeys">A reference to a list that will be filled with all the available options.</param>
		/// <returns>On success, true is returned. On failiure, false is returned.</returns>
		public bool GetOptionKeys( ArrayList refKeys )
		{
			refKeys.Clear();
			refKeys.Add( "AddNewHeight" );
			refKeys.Add( "RemoveNewHeight" );
			refKeys.Add( "CurrentCameraPageX" );
			refKeys.Add( "CurrentCameraPageZ" );
			refKeys.Add( "MaxNumTiles" );
			refKeys.Add( "TileFree" );
			refKeys.Add( "MaxNumRenderables" );
			refKeys.Add( "RenderableFree" );
			refKeys.Add( "RenderableLoading" );
			refKeys.Add( "PagePreloadQueue" );
			refKeys.Add( "PageLoadQueue" );
			refKeys.Add( "PageUnloadQueue" );
			refKeys.Add( "PagePostUnloadQueue" );
			return options.getOptionKeys( refKeys );
		}


		/// <summary>
		/// Internal method for updating the scene graph ie the tree of SceneNode instances managed by this class.
		/// </summary>
		/// <param name="cam"></param>
		/// <remarks>
		///	This must be done before issuing objects to the rendering pipeline, since derived transformations from
		///	parent nodes are not updated until required. This SceneManager is a basic implementation which simply
		///	updates all nodes from the root. This ensures the scene is up to date but requires all the nodes
		///	to be updated even if they are not visible. Subclasses could trim this such that only potentially visible
		///	nodes are updated.
		/// </remarks>
		protected override void UpdateSceneGraph( Axiom.Core.Camera cam )
		{
			// entry into here could come before SetWorldGeometry 
			// got called which could be disasterous
			// so check for init

			if(worldGeomIsSetup)
			{
				pageManager.Update( (Camera)cam );
				renderableManager.ResetVisibles();
			}

			// Call the default
			base.UpdateSceneGraph( cam );
		}


		/// <summary>
		/// Creates a RaySceneQuery for this scene manager.
		/// </summary>
		/// <param name="ray">Details of the ray which describes the region for this query.</param>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out certain objects; see SceneQuery for details.</param>
		/// <returns>
		///	The instance returned from this method must be destroyed by calling
		///	SceneManager::destroyQuery when it is no longer required.
		/// </returns>
		/// <remarks>
		///	This method creates a new instance of a query object for this scene manager, 
		///	looking for objects which fall along a ray. See SceneQuery and RaySceneQuery 
		///	for full details.
		/// </remarks>
		public override Axiom.Core.RaySceneQuery CreateRayQuery(Ray ray, ulong mask)
		{
			Axiom.Core.RaySceneQuery q = new Query.RaySceneQuery(this);
			q.Ray = ray;
			q.QueryMask = mask;
			return q;
		}


		/// <summary>
		/// Creates an IntersectionSceneQuery for this scene manager.
		/// </summary>
		/// <param name="mask">The query mask to apply to this query; can be used to filter out
		///	certain objects; see SceneQuery for details.
		///	</param>
		/// <returns>
		///	The instance returned from this method must be destroyed by calling
		///	SceneManager::destroyQuery when it is no longer required.
		/// </returns>
		/// <remarks>
		///	This method creates a new instance of a query object for locating
		///	intersecting objects. See SceneQuery and IntersectionSceneQuery
		///	for full details.
		/// </remarks>
		public override Axiom.Core.IntersectionSceneQuery CreateIntersectionQuery(ulong mask)
		{
			Axiom.Core.IntersectionSceneQuery q = new Query.IntersectionSceneQuery(this);
			q.QueryMask = mask;
			return q;		
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">begining of the segment </param>
		/// <param name="end">where it ends</param>
		/// <param name="result">where it intersects with terrain</param>
		/// <returns></returns>
		/// <remarks>Intersect mainly with Landscape</remarks>
		public bool IntersectSegment( Vector3 start, Vector3 end, ref Vector3 result)
		{
			return IntersectSegment( start, end, ref result, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">begining of the segment </param>
		/// <param name="end">where it ends</param>
		/// <param name="result">where it intersects with terrain</param>
		/// <param name="modif">If it does modify the terrain</param>
		/// <returns></returns>
		/// <remarks>Intersect mainly with Landscape</remarks>
		public bool IntersectSegment( Vector3 start, Vector3 end, ref Vector3 result, bool modif )
		{
			Vector3 begin = start;
			Vector3 dir = end - start;
			dir.Normalize();
			Tile.Tile t = pageManager.GetTile (start);
			if ( t != null )
			{
				// if you want to be able to intersect from a point outside the canvas
				int pageSize = (int)options.PageSize - 1;
				float W = options.World_Width * 0.5f  * pageSize * options.Scale.x;
				float H = options.World_Height * 0.5f * pageSize * options.Scale.z;
				while (start.y > 0.0f && start.y < 999999.0f &&
					((start.x < -W || start.x > W) || 
					(start.z < -H || start.z > H)))
					start += dir; 

				if (start.y < 0.0f || start.y > 999999.0f)
				{
					result = new Vector3( -1, -1, -1 );
					return false;
				}
				t = pageManager.GetTile (start);
        
				// if you don't want to be able to intersect from a point outside the canvas
				//        *result = Vector3( -1, -1, -1 );
				//        return false;
			}

			bool impact = false;

			//special case...
			if ( dir.x == 0 && dir.z == 0 )
			{
				if (start.y <= data2DManager.GetRealWorldHeight( start.x, start.z, t.Info ))
				{
					result = start;
					impact = true;
				}
			}      
			else
			{
				//    dir.x = dir.x * mOptions.scale.x;
				//    dir.y = dir.y * mOptions.scale.y;
				//    dir.z = dir.z * mOptions.scale.z;
				impact = t.IntersectSegment( start, dir, result );
			}


			// deformation
			if (impact && modif)
			{

				int X = (int) (result.x / options.Scale.x);
				int Z = (int) ( result.z / options.Scale.z);
        
				int pageSize = (int)options.PageSize - 1;
        
				int W =  (int) (options.World_Width * 0.5f  * pageSize);
				int H =  (int) (options.World_Height * 0.5f  * pageSize);

				if (X < -W || X > W || Z < -H || Z > H)
					return true;

				impact = (result != Vector3.Zero);
				const int radius = 7;

				// Calculate the minimum X value 
				// make sure it is still on the height map
				int Xmin = -radius;
				if (Xmin + X < -W)
					Xmin = - X - W;

				// Calculate the maximum X value
				// make sure it is still on the height map
				int Xmax = radius;
				if (Xmax + X > W)
					Xmax = W - X;


				// Main loop to draw the circle on the height map 
				// (goes through each X value)

				for (int Xcurr = Xmin; Xcurr <= radius; Xcurr++)
				{
					float Precalc = (radius * radius) - (Xcurr * Xcurr);
					if (Precalc > 1.0f)
					{
						// Determine the minimum and maximum Z value for that 
						// line in the circle (that X value)
						int Zmax = (int) Math.Sqrt(  Precalc  );            
						int Zmin = -Zmax;

						// Makes sure the values found are both on the height map
						if (Zmin + Z < -H)
							Zmin = - Z -H;

						if (Zmax + Z > H)
							Zmax = H - Z;

						// For each of those Z values, calculate the new Y value
						for (int Zcurr = Zmin; Zcurr < Zmax; Zcurr++)
						{
							// get results by page index ?
							Vector3 currpoint = new Vector3((X + Xcurr),	0.0f,(Z + Zcurr));
							Tile.Tile  p = pageManager.GetTileUnscaled (currpoint);                
							if (p != null && p.IsLoaded)
							{ 
								// Calculate the new theoretical height for the current point on the circle
								float dY =  (float)Math.Sqrt ((float)(Precalc - (Zcurr * Zcurr))) * 10.0f;//* 0.01f

								impactInfo = p.Info;
								data2DManager.DeformHeight (currpoint, dY, p.Info);
								p.Renderable.NeedUpdate();                       
							}                    
						}
					}
            
				}
       
			}
			return true;
		}

		#endregion Methods

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		internal EntityList Entities
		{
			get 
			{
				return base.entityList; 
			}
		}

		#endregion
	}

}
