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

using Axiom.SceneManagers.PagingLandscape.Tile;

using Axiom.SceneManagers.PagingLandscape.Page;

using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Data2D;

using Axiom.SceneManagers.PagingLandscape.Renderable;

using Axiom.SceneManagers.PagingLandscape.Texture;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Tile

{

	/// <summary>

	/// Summary description for Tile.

	/// </summary>

	public class Tile : MovableObject

	{

		#region Fields

		protected static string type = "PagingLandscapeTile";

		//movable object variables
		protected AxisAlignedBox bounds;
		protected AxisAlignedBox boundsExt;
		// if the tile is initialized
		protected bool init;
		// if the renderable is loaded
		protected bool loaded;	

		protected Renderable.Renderable renderable;

		protected SceneNode tileSceneNode;

		protected Tile[] neighbors;

		protected TileInfo info;



		#endregion Fields



		#region Constructor



		public Tile()
		{
			info = new TileInfo();
			tileSceneNode = null;
			renderable = null;
			init = false;
			loaded = false;

			bounds = new AxisAlignedBox();
			boundsExt = new AxisAlignedBox();
			//worldBounds = new AxisAlignedBox();
			neighbors = new Tile[4];
			
		}


		#endregion Constructor

		#region IDisposable Members



		public void Dispose()

		{

		}



		#endregion



		public TileInfo Info 
		{
			get
			{
				return info;
			}
		}


		/** Sets the appropriate neighbor for this TerrainRenderable.  Neighbors are necessary
		to know when to bridge between LODs.
		*/
		public void SetNeighbor( Neighbor n, Tile t )
		{
			neighbors[(int) n ] = t;
		}

		/** Returns the neighbor TerrainRenderable.
		*/
		public Tile GetNeighbor( Neighbor n )
		{
			return neighbors[ (int)n ];
		}

		/// <summary>

		/// Intersects Mainly with Landscape

		/// </summary>

		/// <param name="start">begining of the segment</param>

		/// <param name="dir">direction of the secment</param>

		/// <param name="result">where the segment intersects with the terrain</param>

		/// <returns></returns>
		public bool IntersectSegment( Vector3 start, Vector3 dir, Vector3 result )
		{
			Vector3 ray = start; 
			Vector3[] corners = worldAABB.Corners;

			// Addd the direction to the segment
			ray += dir;
			Data2D.Data2D data = Data2DManager.Instance.GetData2D(info.PageX, info.PageZ);
    
			if (data == null)
			{
				// just go until next tile.
				while ( ! ( ( ray.x < corners[ 0 ].x ) ||
					( ray.x > corners[ 4 ].x ) ||
					( ray.z < corners[ 0 ].z ) ||
					( ray.z > corners[ 4 ].z ) ) )
				{           
					ray += dir;
				}
			}
			else
			{
				float pageMax = 0f;
				if (renderable != null)
				{
					if (renderable.IsLoaded )
					{
						pageMax = renderable.MaxHeight ;
					}
				}
				else
				{
					pageMax = data.MaxHeight;
				}

				while ( ! ( ( ray.x < corners[ 0 ].x ) ||
					( ray.x > corners[ 4 ].x ) ||
					( ray.z < corners[ 0 ].z ) ||
					( ray.z > corners[ 4 ].z ) ) )
				{
           
					if ( ray.y <= pageMax &&  // until under the max possible for this page/tile
						ray.y <= data.GetHeightAbsolute( ray.x, ray.z, info )// until under the max 
						)
					{

						// Found intersection range 
						// zone (ray -  dir < intersection < ray.y +  dir )
						// now do a precise check using a normalised Direction
						// vector. (that is < 0.1f)
						//                ray -= dir;
						//                Vector3 s = PagingLandScapeOptions::getSingleton().scale;                
						//                Vector3 PrecisionDir (dir.x / s.x,
						//                                        dir.y / s.y,
						//                                        dir.z / s.z);
						//                while( ray.y >= pageMax &&
						//                        ray.y >= tileMax)
						//                {
						//                    ray += PrecisionDir;
						//                }
						//                // until under the interpolated upon current LOD max 
						//                while( ray.y > PagingLandScapeData2DManager::getSingleton().getRealWorldHeight( ray.x, ray.z ))
						//                {
						//                    ray += PrecisionDir;
						//                }
						result = ray;
						return true;
					}          
					ray += dir;
				}
			}
        

			if ( ray.x < corners[ 0 ].x && neighbors[ (int)Neighbor.West ] != null )
				return neighbors[ (int)Neighbor.West ].IntersectSegment( ray, dir, result );
			else if ( ray.z < corners[ 0 ].z && neighbors[ (int)Neighbor.North ] != null )
				return neighbors[ (int)Neighbor.North ].IntersectSegment( ray, dir, result );
			else if ( ray.x > corners[ 4 ].x && neighbors[ (int)Neighbor.East ] != null )
				return neighbors[ (int)Neighbor.East ].IntersectSegment( ray, dir, result );
			else if ( ray.z > corners[ 4 ].z && neighbors[ (int)Neighbor.South ] != null )
				return neighbors[ (int)Neighbor.South ].IntersectSegment( ray, dir, result );
			else
			{
				if ( result != Vector3.Zero )
					result = new Vector3( -1, -1, -1 );

				return false;
			}
		}

		/// <summary>

		/// Make the Tile reload its vertices and normals (upon a modification of the height data)

		/// </summary>
		public void UpdateTerrain ()
		{
			Debug.Assert (renderable != null);
			renderable.NeedUpdate();    
		}
  

		public Renderable.Renderable Renderable
		{
			get
			{
				return renderable;    
			}
		}


		public void LinkRenderableNeighbor()
		{
			// South
			if (neighbors[(int)Neighbor.South] != null)
			{
				if (neighbors[(int)Neighbor.South].IsLoaded)
				{
					Renderable.Renderable n = neighbors[(int)Neighbor.South].Renderable;
					Debug.Assert(n != null);
					renderable.SetNeighbor (Neighbor.South, n);
					n.SetNeighbor (Neighbor.North, renderable);
				}
			}

			//North
			if (neighbors[(int)Neighbor.North] != null)
			{
				if (neighbors[(int)Neighbor.North].IsLoaded)
				{
					Renderable.Renderable n = neighbors[(int)Neighbor.North].Renderable;
					Debug.Assert(n != null);
					renderable.SetNeighbor (Neighbor.North, n);
					n.SetNeighbor (Neighbor.South, renderable);
				}
			}

			//East
			if (neighbors[(int)Neighbor.East] != null)
			{
				if (neighbors[(int)Neighbor.East].IsLoaded)
				{
					Renderable.Renderable n = neighbors[(int)Neighbor.East].Renderable;
					Debug.Assert(n != null);
					renderable.SetNeighbor (Neighbor.East, n);
					n.SetNeighbor (Neighbor.West, renderable);
				}
			}

			//West
			if (neighbors[(int)Neighbor.West] != null)
			{
				if (neighbors[(int)Neighbor.West].IsLoaded)
				{
					Renderable.Renderable n = neighbors[(int)Neighbor.West].Renderable;
					Debug.Assert(n != null);
					renderable.SetNeighbor (Neighbor.West, n);
					n.SetNeighbor (Neighbor.East, renderable);
				}
			}

		}



		public void Init( ref SceneNode ParentSceneNode, int tableX, int tableZ, int tileX, int tileZ )
		{
			init = true;

			Vector3 ParentPos = ParentSceneNode.DerivedPosition;

			info.PageX = tableX;
			info.PageZ = tableZ;
			info.TileX = tileX;
			info.TileZ = tileZ;
			// Calculate the offset from the parent for this tile

			Vector3 scale = Options.Instance.Scale;
			float endx = Options.Instance.TileSize * scale.x;
			float endz = Options.Instance.TileSize * scale.z;
			info.PosX = info.TileX * endx;
			info.PosZ = info.TileZ * endz;

			name = String.Format("tile[{0},{1}][{2},{3}]",info.PageX, info.PageZ, info.TileX, info.TileZ);
			tileSceneNode = (SceneNode)ParentSceneNode.CreateChild( name );

			// figure out scene node position within parent
			tileSceneNode.Position = new Vector3( info.PosX, 0, info.PosZ );

			tileSceneNode.AttachObject( this );

			float MaxHeight = Data2DManager.Instance.GetMaxHeight(info.PageX, info.PageZ);

			bounds.SetExtents( new Vector3(0,0,0) , new Vector3((float)( endx ), MaxHeight, (float)( endz )) );
		  
			//Change Zone of this page
			boundsExt.SetExtents( new Vector3( - endx * 0.5f, - MaxHeight * 0.5f, - endz * 0.5f) , new Vector3(endx * 1.5f, MaxHeight * 1.5f , endz * 1.5f));

			//Change Zone of this page

			this.worldAABB.SetExtents( new Vector3(info.PosX + ParentPos.x ,0, info.PosZ + ParentPos.z), new Vector3((float)( info.PosX + ParentPos.x + endx), MaxHeight, (float)( info.PosZ + ParentPos.z + endz) ));
			//this.worldBounds.SetExtents( new Vector3(info.PosX + ParentPos.x ,0, info.PosZ + ParentPos.z), new Vector3((float)( info.PosX + ParentPos.x + endx), MaxHeight, (float)( info.PosZ + ParentPos.z + endz) ));

			for ( long i = 0; i < 4; i++ )
			{
				neighbors[ i ] = null;
			}
			//force update in scene node
			//tileSceneNode.update( true, true );
			tileSceneNode.NeedUpdate();
			loaded = false;
		}

		public void Release()
		{
			if (init)
			{
				init = false;

				if ( loaded && renderable != null )
				{        
					renderable.Release();
					renderable = null;
					loaded = false;         
				}
				if (neighbors[(int)Neighbor.North] != null)
					neighbors[(int)Neighbor.North].SetNeighbor(Neighbor.South, null);
				if (neighbors[(int)Neighbor.South] != null)
					neighbors[(int)Neighbor.South].SetNeighbor(Neighbor.North, null);
				if (neighbors[(int)Neighbor.East] != null)
					neighbors[(int)Neighbor.East].SetNeighbor(Neighbor.West, null);
				if (neighbors[(int)Neighbor.West] != null)
					neighbors[(int)Neighbor.West].SetNeighbor(Neighbor.East, null);
				for ( long i = 0; i < 4; i++ )
				{
					neighbors[ i ] = null;
				}

				if ( tileSceneNode != null )
				{
					//mTileNode->getParent()->removeChild( mTileNode->getName() );
					//delete mTileNode;
					//assert (mTileSceneNode->getParent());

					// jsw - we need to call both of these to delete the scene node.  The first
					//  one removes it from the SceneManager's list of all nodes, and the 2nd one 
					//  removes it from the tree of scene nodes.
					tileSceneNode.Creator.DestroySceneNode(tileSceneNode.Name);
					tileSceneNode.Parent.RemoveChild( tileSceneNode );
					tileSceneNode = null;
				}

				TileManager.Instance.FreeTile( this );
			}
		}


		/** Returns the bounding box of this LandScapeRenderable 
		*/
		public override AxisAlignedBox BoundingBox
		{ 
			get
			{
				return bounds;
			}
		}


		/** Updates the level of detail to be used for rendering this PagingLandScapeRenderable based on the passed in Camera 
		*/
		public override void NotifyCurrentCamera( Axiom.Core.Camera cam )
		{
		    PagingLandscape.Camera plsmcam = (PagingLandscape.Camera) (cam);
			this.isVisible = (init && loaded && plsmcam.GetVisibility(tileSceneNode.WorldAABB));
//			this.isVisible = (init && loaded );
		}


		public virtual void NotifyAttached(Node parent)
		{
			this.parentNode = parent;
			if ( parent != null )
			{
				tileSceneNode = (SceneNode)( parent.CreateChild( name ) );
				//mTileNode.setPosition( (Real)mTableX , 0.0, (Real)mTableZ );
				if ( renderable != null)
					if (renderable.IsLoaded)
				{
					tileSceneNode.AttachObject( (MovableObject) renderable );
				}
				tileSceneNode.NeedUpdate();
			}
		}


		public override void UpdateRenderQueue( RenderQueue queue ){/* not needed */}

		/** Overridden from SceneObject */
		public override float BoundingRadius { get { return 0F; /* not needed */ } }

		public void GetWorldTransforms( Matrix4[] xform ) 
		{
			parentNode.GetWorldTransforms(xform);
		}

		public Quaternion WorldOrientation
		{
			get
			{
				return parentNode.DerivedOrientation;
			}
		}

		public Vector3 WorldPosition
		{
			get
			{
				return parentNode.DerivedPosition;
			}
		}

		public void Notify( Vector3 pos, PagingLandscape.Camera Cam)
		{

			if ((( pos - tileSceneNode.DerivedPosition).LengthSquared <= 
							Options.Instance.Renderable_Factor))
			{
		    
				if ( loaded == false
					//&& Cam.getVisibility (mBoundsExt)) 
//					&& Cam.GetVisibility (tileSceneNode.WorldAABB ))
					&& Cam.GetVisibility (this.worldAABB ))
					{
						// Request a renderable
						renderable = RenderableManager.Instance.GetRenderable();
						if ( renderable != null )
						{
							//TODO: We may remove the PosX and PosZ since it is not necessary
							renderable.Init( info );
							// Set the material
							renderable.Material = Texture.TextureManager.Instance.GetMaterial( info.PageX, info.PageZ ) ;
						    
							if ( tileSceneNode != null)
							{
								tileSceneNode.Clear();
							}                   
							//Queue it for loading
							RenderableManager.Instance.QueueRenderableLoading( renderable, this );
							loaded = true;

							PageManager.Instance.GetPage(info.PageX,info.PageZ).LinkTileNeighbors();
						}                
					}

			}
			else
			{
				if ( renderable != null )
				{
					loaded = false;
					if ( tileSceneNode != null && renderable.IsLoaded )
					{
						tileSceneNode.DetachObject( renderable );
					}
					renderable.Release();
					renderable = null;
				}
			}
		}

		/// Gets all the patches within an AABB in world coordinates as GeometryData structs
		public virtual void GetRenderOpsInBox( AxisAlignedBox box, ArrayList opList)
		{
			if ( MathUtil.Intersects(box, bounds ) != Intersection.None )
			{
				RenderOperation rend = new RenderOperation();
				renderable.GetRenderOperation( rend );
				opList.Add( rend );
			}
		}

		public SceneNode TileNode 
		{
			get
			{
				return tileSceneNode;
			}
		}

		public bool IsLoaded
		{
			get
			{
				return loaded;
			}
		}

	}

}

