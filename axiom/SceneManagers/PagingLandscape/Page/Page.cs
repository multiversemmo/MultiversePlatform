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

using Axiom.Graphics;



using Axiom.SceneManagers.PagingLandscape;

using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Data2D;

using Axiom.SceneManagers.PagingLandscape.Tile;

using Axiom.SceneManagers.PagingLandscape.Texture;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Page

{

	/// <summary>

	/// Summary description for Page.

	/// </summary>

	public class Page : IDisposable

	{


		#region Fields
		protected SceneNode pageNode;

		protected Tiles tiles;

		protected bool isLoaded;
		protected bool isPreLoaded;

		protected long tableX;	// Position of this Terrain Page in the Terrain Page Array
		protected long tableZ;

		protected long numTiles;

		protected float iniX;	//, mEndX;	// Max and Min values of the terrain
		protected float iniZ;	//, mEndZ;

		protected Page[] neighbors;

		// Change Zone values
		protected AxisAlignedBox boundsExt;

		protected AxisAlignedBox boundsInt;
		#endregion Fields

		#region Constructor
		public Page(long TableX, long TableZ)

		{

			isLoaded = false;
			isPreLoaded = false;

			tableX = TableX;
			tableZ = TableZ;

			numTiles = (long) ((float) Options.Instance.PageSize / Options.Instance.TileSize);
			pageNode = null;

			long size = Options.Instance.PageSize - 1;
			// Boundaries of this page
			// the middle page is at world coordinates 0,0
			float factorX = size * Options.Instance.Scale.x;
			float factorZ = size * Options.Instance.Scale.z;
			iniX =  (tableX + tableX - Options.Instance.World_Width) / 2.0f * factorX ;		
			iniZ =  (tableZ + tableZ - Options.Instance.World_Height) / 2.0f * factorZ ;		
			float EndX = iniX + factorX;
			float EndZ = iniZ + factorZ;
			float MaxHeight = Data2DManager.Instance.GetMaxHeight(tableX, tableZ);
			float chgfactor = Options.Instance.Change_Factor;
			boundsExt = new AxisAlignedBox();
			boundsExt.SetExtents( new Vector3(( float )( iniX ), 
									0, 
								( float )( iniZ )), 
								new Vector3(( float )( EndX ), 
								MaxHeight, 
								( float )( EndZ ) ));
			//Change Zone of this page
			boundsInt = new AxisAlignedBox();
			boundsInt.SetExtents( new Vector3(( float )( iniX + chgfactor ), 
									0, 
								( float )( iniZ + chgfactor )),
								new Vector3(( float )( EndX - chgfactor ), 
									MaxHeight, 
									( float )( EndZ - chgfactor )	));

			neighbors = new Page[4];
			for ( long i = 0; i < 4; i++ )
			{
				neighbors[ i ] = null;
			}
		}

		#endregion Constructor

		#region IDisposable Members



		public void Dispose()

		{

			Unload();	 
			PostUnload ();
		}



		#endregion


		/** Sets the appropriate neighbor for this TerrainRenderable.  Neighbors are necessary
	to know when to bridge between LODs.
	*/

		public void SetNeighbor( Neighbor n, Page t )
		{
			neighbors[ (int)n ] = t;
		}

		/** Returns the neighbor TerrainRenderable.
		*/
		public Page GetNeighbor( Neighbor n )
		{
			return neighbors[ (int)n ];
		}

		public Tile.Tile GetTile  ( long i ,  long j)
		{
			if ( isLoaded )
			{
				if ( tiles[i][j] != null && tiles[i][j].IsLoaded )
				{
					return tiles[i][j];

				}

			}

			return null;

		}

		public Tile.Tile GetTile  ( Vector3 pos)
		{
			if ( isLoaded )
			{
				Vector3 TileRefPos = pos;

				TileRefPos.x = (pos.x / Options.Instance.Scale.x / (Options.Instance.TileSize));
				TileRefPos.z = (pos.z / Options.Instance.Scale.z / (Options.Instance.TileSize));

				int x = (int) TileRefPos.x;
				int z = (int) TileRefPos.z;

				if (tiles[x][z] != null && tiles[x][z].IsLoaded)
					return tiles[x][z];
			}
			return null;
		}

		/** Pre-loads the landscape using parameters in the given in the constructor. */
		public void Preload( )
		{
			if ( isPreLoaded == true )
			{
				return;
			}

			Data2DManager.Instance.Load(tableX, tableZ);

			isPreLoaded = true;
		}

		/** Loads the landscape using parameters in the given in the constructor. */
		public void Load( ref SceneNode RootNode )
		{
			if ( isPreLoaded == false )
			{
				return;
			}

			if ( isLoaded == true )
			{
				return;
			}

			Texture.TextureManager.Instance.Load(tableX, tableZ);
			//create a root landscape node.
			pageNode = RootNode.CreateChildSceneNode( "Page." +  tableX.ToString() + "." + tableZ.ToString()  );
			// Set node position
			pageNode.Position = new Vector3( (float)iniX , 0.0f, (float)iniZ );

			tiles = new Tiles();
			for (long  i = 0; i < numTiles; i++ )
			{
				tiles.Add( new TileRow() );

				for (long  j = 0; j < numTiles; j++ )
				{
					//Debug.WriteLine(String.Format("Page[{0},{1}][{2},{3}]",tableX,tableZ,i,j));

					Tile.Tile tile = TileManager.Instance.GetTile();
					if ( tile != null )
					{
						tiles[ i ].Add( tile );
						tile.Init(ref pageNode,(int) tableX, (int)tableZ,(int) i, (int)j );
					}
					else
					{
						string err = "Error: Invalid Tile: Make sure the default TileManager size is set to WorldWidth * WorldHeight * 4. Try increasing MaxNumTiles in the configuration file.";
						throw new ApplicationException(err);
					}
				}
			}

			for (long  j = 0; j < numTiles; j++ )
			{
				for (long  i = 0; i < numTiles; i++ )
				{		
					if ( j != numTiles - 1 )
					{
						tiles[ i ][ j ].SetNeighbor( Neighbor.South, tiles[ i ][ j + 1 ] );
						tiles[ i ][ j + 1 ].SetNeighbor( Neighbor.North, tiles[ i ][ j ] );
					}
					if ( i != numTiles - 1 )
					{
						tiles[ i ][ j ].SetNeighbor( Neighbor.East, tiles[ i + 1 ][ j ] );
						tiles[ i + 1 ][ j ].SetNeighbor( Neighbor.West, tiles[ i ][ j ] );    
					}           
				}
			}

			LinkTileNeighbors();
			pageNode.NeedUpdate();
			isLoaded = true;
		}

		public void LinkTileNeighbors()
		{
			if (neighbors[(int)Neighbor.East] != null) 
			{
				if ( neighbors[(int)Neighbor.East].IsLoaded)
				{
					long i = numTiles - 1;
					for (long  j = 0; j < numTiles; j++ )
					{	
						Tile.Tile t_nextpage =  neighbors[(int)Neighbor.East].GetTile  ( 0 , j );
						Tile.Tile t_currpage =  tiles[ i ][ j ];
						//Debug.Assert(t_nextpage != null && t_currpage != null);
						if (t_nextpage != null && t_currpage != null)
						{
							t_currpage.SetNeighbor( Neighbor.East, t_nextpage );
							t_nextpage.SetNeighbor( Neighbor.West, t_currpage );
						}
					}
				}
			}
			if (neighbors[(int)Neighbor.West] != null)
			{
				if (neighbors[(int)Neighbor.West].IsLoaded)
				{
					long i = numTiles - 1;
					for (long  j = 0; j < numTiles; j++ )
					{	
						Tile.Tile t_nextpage =  neighbors[(int)Neighbor.West].GetTile  ( i , j );
						Tile.Tile t_currpage =  tiles[ 0 ][ j ];
						//Debug.Assert(t_nextpage != null && t_currpage != null);
						if (t_nextpage != null && t_currpage != null)
						{
							t_currpage.SetNeighbor( Neighbor.West, t_nextpage );
							t_nextpage.SetNeighbor( Neighbor.East, t_currpage );
						}
					}
				}
			}

			if (neighbors[(int)Neighbor.South] != null)
			{       
				if (neighbors[(int)Neighbor.South].IsLoaded)
				{
					long j = numTiles - 1;
					for (long  i = 0; i < numTiles; i++ )
					{	
						Tile.Tile t_nextpage =  neighbors[(int)Neighbor.South].GetTile  ( i , 0 );
						Tile.Tile t_currpage =  tiles[ i ][ j ];
						//Debug.Assert(t_nextpage != null  && t_currpage != null);
						if (t_nextpage != null && t_currpage != null)
						{
							t_currpage.SetNeighbor( Neighbor.South, t_nextpage);
							t_nextpage.SetNeighbor( Neighbor.North, t_currpage );
						}
					}
				}
			}
			if (neighbors[(int)Neighbor.North] != null)
			{       
				if (neighbors[(int)Neighbor.North].IsLoaded)
				{
					long j = numTiles - 1;
					for (long  i = 0; i < numTiles; i++ )
					{	
						Tile.Tile t_nextpage =  neighbors[(int)Neighbor.North].GetTile  ( i , j );
						Tile.Tile t_currpage =  tiles[ i ][ 0 ];
						//Debug.Assert(t_nextpage != null && t_currpage != null);
						if (t_nextpage != null && t_currpage != null)
						{
							t_currpage.SetNeighbor(Neighbor.North, t_nextpage );
							t_nextpage.SetNeighbor(Neighbor.South, t_currpage );
						}
					}
				}
			}

		}

		/** Unloads the landscape data, but doesn´t destroy the landscape page. */
		public void Unload( )
		{
			if ( isLoaded == false )
				return;
    
			if (neighbors[(int)Neighbor.South] != null)
				neighbors[(int)Neighbor.South].SetNeighbor (Neighbor.North, null);
			if (neighbors[(int)Neighbor.North] != null)
				neighbors[(int)Neighbor.North].SetNeighbor (Neighbor.South, null);
			if (neighbors[(int)Neighbor.East] != null)
				neighbors[(int)Neighbor.East].SetNeighbor (Neighbor.West, null);
			if (neighbors[(int)Neighbor.West] != null)
				neighbors[(int)Neighbor.West].SetNeighbor (Neighbor.East, null);
			for ( long i = 0; i < 4; i++ )
			{
				neighbors[ i ] = null;
			}

			// Unload the Tiles
			for ( long i = 0; i < numTiles; i++ )
			{
				for ( long j = 0; j < numTiles; j++ )
				{
					Debug.Assert (tiles[ i ][ j ] != null);
					tiles[ i ][ j ].Release();
					tiles[ i ][ j ] = null;
				}
				//mTiles[ i ].clear();
			}
			//mTiles.clear ();

			if ( pageNode != null )
			{
				// Unload the nodes
				pageNode.Clear();
				//pageNoderemoveAndDestroyAllChildren();

				//mPageNode->getParent()->removeChild( mPageNode->getName() );
				//delete mPageNode;

				// jsw - we need to call both of these to delete the scene node.  The first
				//  one removes it from the SceneManager's list of all nodes, and the 2nd one 
				//  removes it from the tree of scene nodes.
				pageNode.Creator.DestroySceneNode(pageNode.Name);
				pageNode.Parent.RemoveChild( pageNode.Name );
				pageNode = null;
			}
			Texture.TextureManager.Instance.Unload(tableX, tableZ);
			isLoaded = false;
		}

		/** Post Unloads the landscape data, but doesn´t destroy the landscape page. */
		public void PostUnload( )
		{
			if ( isLoaded == true )
			{
				return;
			}
			if ( isPreLoaded == true )
			{
				isPreLoaded = false;

				Data2DManager.Instance.Unload( tableX, tableZ );
			}
		}

		public bool IsLoaded
		{
			get
			{
				return isLoaded;
			}
		}

		public bool IsPreLoaded
		{
			get
			{
				return isPreLoaded;
			}
		}

		/** Returns if the camera is over this landscape page.
		*/
		public CameraPageState IsCameraIn( Vector3  pos )
		{
			if ( boundsExt.Intersects( pos ) == true )
			{
				if ( boundsInt.Intersects( pos ) == true )
				{
					// Full into this page
					return CameraPageState.Inside;
				}
				else
				{
					// Over the change zone
					return CameraPageState.Change;
				}
			}
			else
			{
				// Not in this page
				return CameraPageState.Outside;
			}
		}

		public void Notify(Vector3 pos, Camera Cam)
		{
			if (isLoaded
				&& Cam.IsObjectVisible(pageNode.WorldAABB))
			//-----------------
				//&& Cam->getVisibility (mBoundsExt))
			{
        
				for ( long i = 0; i < numTiles; i++ )
				{
					for ( long j = 0; j < numTiles; j++ )
					{
						tiles[ i ][ j].Notify( pos, Cam);
					}
				}
			}
		}

		/// Gets all the patches within an AABB in world coordinates as GeometryData structs
		public virtual void GetRenderOpsInBox( AxisAlignedBox box, ArrayList opList)
		{
			if ( MathUtil.Intersects(box, boundsExt ) != Intersection.None )
			{
				for ( long i = 0; i < numTiles; i++ )
				{
					for ( long j = 0; j < numTiles; j++ )
					{
						tiles[ i ][ j].GetRenderOpsInBox( box, opList );
					}
				}
			}
		}

	


	}

}

