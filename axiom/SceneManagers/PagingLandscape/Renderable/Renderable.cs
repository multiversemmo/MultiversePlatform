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

using Axiom.Media;

using Axiom.Graphics;



using Axiom.SceneManagers.PagingLandscape;

using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Data2D;

using Axiom.SceneManagers.PagingLandscape.Tile;



#endregion Using Directives



#region Versioning Information

/// File								Revision

/// ===============================================

/// OgrePagingLandScapeSceneRenderable.h	1.5

/// OgrePagingLandScapeRenderable.cpp		1.13?

/// 

#endregion



namespace Axiom.SceneManagers.PagingLandscape.Renderable

{

	/// <summary>

	/// Summary description for Renderable.

	/// </summary>

	public class Renderable : SimpleRenderable, IDisposable

	{


		#region Fields
		/// Connection to tiles four neighbours
		protected RenderOperation renderOp;

		protected Renderable[] neighbors;
		//unsigned short *mIndex;
		protected long numIndex;

		protected bool inUse;
		protected bool isLoaded;
		protected bool inFrustum;
		protected bool needReload;

		protected int materialLODIndex;

		protected Vector3 coneNormal;
		protected float angle;
		protected bool mustRender;
		// for loading
		protected TileInfo info;

		const int POSITION = 0;

		const int NORMAL = 1;

		const int TEXCOORD = 2;

		const int COLORS = 3;

		/// The current LOD level
		public long RenderLevel;   

		#endregion Fields

		/** Sets the appropriate neighbor for this TerrainRenderable.  Neighbors are necessary
		to know when to bridge between LODs.
		*/
		public void SetNeighbor( Neighbor n, Renderable t )
		{
			neighbors[(int) n ] = t;
		}
		/** Returns the neighbor TerrainRenderable.
		*/
		public Renderable GetNeighbor( Neighbor n )
		{
			return neighbors[ (int)n ];
		}


		// if a neighbour changes its RenderLevel.
		// get a new Indexbuffer.
		public void Update()
		{
			renderOp.indexData = getIndexData();
		}

		/**	Initializes the LandScapeRenderable with the given options and the starting coordinates of this block.
		*/
		public Renderable():base()
		{
			info = null;
			materialLODIndex = 0;

			neighbors = new Renderable[4];
			for ( long i = 0; i < 4; i++ )
				neighbors[ i ] = null;

			inUse = false;
			isLoaded = false;
			// Setup render op
			renderOp = new RenderOperation();
			renderOp.vertexData = new VertexData();
			renderOp.vertexData.vertexStart = 0;
			long tileSize = Options.Instance.TileSize;
			renderOp.vertexData.vertexCount =  (int)((tileSize + 1) * (tileSize + 1));

			box = new AxisAlignedBox();

			// Vertex declaration
			VertexDeclaration decl = renderOp.vertexData.vertexDeclaration;
			VertexBufferBinding bind = renderOp.vertexData.vertexBufferBinding;

			HardwareVertexBuffer vbuf;
			// Vertex buffer #1, position
			// positions
			int offset = 0;

			decl.AddElement(POSITION, offset, VertexElementType.Float3, VertexElementSemantic.Position);
			offset += VertexElement.GetTypeSize(VertexElementType.Float3);
			if ( Options.Instance.Lit )
			{
				decl.AddElement(POSITION, offset, VertexElementType.Float3, VertexElementSemantic.Normal);
				offset += VertexElement.GetTypeSize(VertexElementType.Float3);
			}
			decl.AddElement(POSITION, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
			offset += VertexElement.GetTypeSize(VertexElementType.Float2);
			vbuf = HardwareBufferManager.Instance.CreateVertexBuffer(
				decl.GetVertexSize(POSITION), 
				renderOp.vertexData.vertexCount,
				BufferUsage.StaticWriteOnly,false);

			bind.SetBinding(POSITION, vbuf);

//			if (Options.Instance.Lit)
//			{
//				offset = 0;
//				decl.AddElement(NORMAL, offset, VertexElementType.Float3, VertexElementSemantic.Normal);
//				offset += VertexElement.GetTypeSize(VertexElementType.Float3);
//				vbuf = HardwareBufferManager.Instance.CreateVertexBuffer(
//					decl.GetVertexSize(NORMAL), 
//					renderOp.vertexData.vertexCount,
//					BufferUsage.StaticWriteOnly);
//
//				bind.SetBinding(NORMAL, vbuf);
//			}
//
//			offset = 0;
//			decl.AddElement(TEXCOORD, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
//			offset += VertexElement.GetTypeSize(VertexElementType.Float2);
//			vbuf = HardwareBufferManager.Instance.CreateVertexBuffer(
//				decl.GetVertexSize(TEXCOORD), 
//				renderOp.vertexData.vertexCount,
//				BufferUsage.StaticWriteOnly);
//
//			bind.SetBinding(TEXCOORD, vbuf);
//
//
//			if (Options.Instance.Colored  ||
//				Options.Instance.Coverage_Vertex_Color || 
//				Options.Instance.Base_Vertex_Color)
//			{
//				offset = 0;
//				decl.AddElement(COLORS, offset,  VertexElementType.Float3, VertexElementSemantic.Diffuse);
//				offset += VertexElement.GetTypeSize(VertexElementType.Color);
//				vbuf = HardwareBufferManager.Instance.CreateVertexBuffer(
//					decl.GetVertexSize(COLORS), 
//					renderOp.vertexData.vertexCount,
//					BufferUsage.StaticWriteOnly);
//
//				bind.SetBinding(COLORS, vbuf);
//			}

			//No need to set the indexData since it is shared from IndexBuffer class
			renderOp.operationType = OperationType.TriangleList;
			renderOp.useIndices = true;

			renderOp.indexData = null;

			RenderLevel = Options.Instance.MaxRenderLevel / 2;
		}


		public void Dispose()
		{
			Release();

			if ( renderOp.indexData != null )
				renderOp.indexData = null;

			if ( renderOp.vertexData != null )
			{
				renderOp.vertexData = null;
			}
		}


		public void Init( TileInfo Info )
		{
			mustRender = false;
			info = Info;
			inFrustum = false;
			inUse = true;
		}

		public void Load()
		{
			if ( inUse == false )
			{
				return;
			}

			bool bLit = Options.Instance.Lit;
			bool bColored = Options.Instance.Colored;
			bool bCoverage = Options.Instance.Coverage_Vertex_Color;
			bool bBase = Options.Instance.Base_Vertex_Color;
			bool bShadowed = Options.Instance.Vertex_Shadowed;
			bool bInstant_colored = Options.Instance.Vertex_Instant_Colored;
		  
			float scale_x = Options.Instance.Scale.x;
			float scale_z = Options.Instance.Scale.z;

//			VertexDeclaration decl = renderOp.vertexData.vertexDeclaration;
			VertexBufferBinding bind = renderOp.vertexData.vertexBufferBinding;

//		    float[] matHeight = new float[2];
//		    matHeight[0] = Options.Instance.MatHeight[0];
//			matHeight[1] = Options.Instance.MatHeight[1];
//			float absmaxHeight = Data2DManager.Instance.GetMaxHeight();

			// Calculate the offset in the data
			long tileSize = Options.Instance.TileSize;
//			long offSetX = info.TileX * tileSize;
//			long offSetZ = info.TileZ * tileSize;
			long offSetX = 0;
			long offSetZ = 0;
			long endx = offSetX + tileSize;
			long endz = offSetZ + tileSize;

			//calculate min and max heights;
			float min = 99999999.9f;
			float max = 0.0f;

			float Aux1 =  ( float ) 1.0f / ( Options.Instance.PageSize - 1 );

			long pageSize = Options.Instance.PageSize;
			float[] HeightData = Data2DManager.Instance.GetData2D( info.PageX, info.PageZ).HeightData;
//			long HeightDataPos = offSetZ * pageSize;
			long HeightDataPos = (info.TileZ * tileSize * pageSize) + ( info.TileX * tileSize );
			float Tex1DataPos = info.TileX * tileSize;
			float Tex2DataPos = info.TileZ * tileSize;

			HardwareVertexBuffer vVertices = bind.GetBuffer(POSITION);
//			HardwareVertexBuffer vNormals;
//			HardwareVertexBuffer vTextures = bind.GetBuffer(TEXCOORD);
//			HardwareVertexBuffer vColors;
//			if (  Options.Instance.Lit) vNormals  = bind.GetBuffer(NORMAL);;
//			if (Options.Instance.Colored  ||
//				Options.Instance.Coverage_Vertex_Color || 
//				Options.Instance.Base_Vertex_Color)
//			{
//				vColors = bind.GetBuffer(COLORS);
//			}
//
			IntPtr ipPos = vVertices.Lock(BufferLocking.Discard);
//			IntPtr ipTex = vTextures.Lock(BufferLocking.Discard);
//			IntPtr ipNrm = vNormals.Lock(BufferLocking.Discard);
//			IntPtr ipClr = vColors.Lock(BufferLocking.Discard);

			int cntPos = 0;
//			int cntTex = 0;
//			int cntNrm = 0;
//			int cntClr = 0;

			unsafe
			{
				float* pPos = (float *) ipPos.ToPointer();
//				float* pNrm = (float *)ipNrm.ToPointer();
//				float* pTex = (float *)ipTex.ToPointer();
//				float* pClr = (float *)ipClr.ToPointer();
				
				for (long k = offSetZ; k <= endz; k ++ )
				{
					float posZ = ( float )( k - offSetZ ) * scale_z;

					for (long i = offSetX; i <= endx; i ++ )
					{
						float height =  HeightData[ i + HeightDataPos ];
//
//						min = Math.Min(height, min);  
//						max = Math.Max(height, max);  
						min = height < min ? height : min;
						max = height > max ? height : max;
//
//						// Position
						pPos[cntPos++] = ( float )( ( i - offSetX ) * scale_x );	//X
						pPos[cntPos++] = height;									//Y
						pPos[cntPos++] = posZ;										//Z
//						
						// normals
						Vector3 norm;
						if ( bLit == true )
						{			
							norm = Data2DManager.Instance.GetNormalAt( info.PageX, info.PageZ, (info.TileX * tileSize) + i, ( info.TileZ * tileSize) + k );
							pPos[cntPos++] = norm.x;
							pPos[cntPos++] = norm.y;
							pPos[cntPos++] = norm.z;
#if _VisibilityCheck
							//TODO: This must be moved to Preprocessing phase
							mTmpAngle = mConeNormal.dotProduct( norm );
							if ( mTmpAngle > mAngle )
							{
								mAngle = mTmpAngle;
							}
							mConeNormal += norm;
							mConeNormal.normalise();
#endif
						}

						// Texture
						pPos[cntPos++] = (Tex1DataPos + i) * Aux1; //Tex1DataPos;
						pPos[cntPos++] = (Tex2DataPos + k) * Aux1; //Tex2DataPos;	
//						pTex[cntTex++] = (Tex1DataPos + k) * Aux1; //Tex2DataPos;	
//						pTex[cntTex++] = (Tex2DataPos + i) * Aux1; //Tex1DataPos;

						// Colors
					    //Tex2DataPos += Aux1;
					}
				//Tex2DataPos += Aux1;
				HeightDataPos += pageSize ;
				}

			}
			vVertices.Unlock();
//			vTextures.Unlock();

			box.SetExtents( new Vector3( 0.0F, 
				min, 
				0.0F), 
				new Vector3( ((float)tileSize) * scale_x ,
				max, 
				((float)tileSize) * scale_z ) );

			worldBoundingSphere.Center = box.Center ;
			worldBoundingSphere.Radius = box.Maximum.Length ;

			isLoaded = true;
			needReload = false;
		}

		public void NeedUpdate ()
		{
			needReload = true;
		}


		public void Release()
		{
			if (inUse && isLoaded)
			{
				if (neighbors[(int)Neighbor.South] != null)
					neighbors[(int)Neighbor.South].SetNeighbor(Neighbor.North, null);
				if (neighbors[(int)Neighbor.North] != null)
					neighbors[(int)Neighbor.North].SetNeighbor(Neighbor.South, null);
				if (neighbors[(int)Neighbor.West] != null)
					neighbors[(int)Neighbor.West].SetNeighbor(Neighbor.East, null);
				if (neighbors[(int)Neighbor.East] != null)
					neighbors[(int)Neighbor.East].SetNeighbor(Neighbor.West, null);
				for (uint i = 0; i < 4; i++ )
				{
					neighbors[i] = null;
				}       
				RenderableManager.Instance.FreeRenderable( this );
				inUse = false;
				isLoaded = false;
				info = null;
			}
		}

		public override void GetRenderOperation(RenderOperation op)

		{

			op.useIndices = this.renderOp.useIndices;	

			op.operationType = this.renderOp.operationType;

			op.vertexData = this.renderOp.vertexData;

			op.indexData = this.renderOp.indexData;

		}


		public override void NotifyCurrentCamera( Axiom.Core.Camera cam )
		{
			if ( inUse == false || isLoaded == false)
				return;

			if (((Camera)(cam)).IsObjectVisible(this.worldAABB))
			{ 
				inFrustum = true;
				isVisible = true;
			}
			else
			{
				inFrustum = false;
				isVisible = false;
				return;
			}

			/* set*/
#if _VisibilityCheck

			//Check if the renderable need to be rendered based on the Cone normal approach
			//TODO: use the cone aperture not a fixed value
			if ( Options.Instance.Lit == true )
			{
				mustRender = (bool)( cam.Direction.Dot( coneNormal ) < Options.Instance.VisibilityAngle );
			}
#endif

			float d = (parentNode.DerivedPosition - cam.DerivedPosition).LengthSquared;
			// Now adjust it by the camera bias
			d = d * cam.LodBias;
			// Material LOD

			//TODO:: Axiom Doesn't Support Material LOD yet.
			//if ( this.material.getNumLodLevels() > 1 )
			//	materialLODIndex = m_pMaterial->getLodIndexSquaredDepth(d);

			// Check if we need to decrease the LOD
			float factor = Options.Instance.LOD_Factor;
			float curr_lod =  factor * ( 1 +  RenderLevel );
			bool changeLOD = false;
			if (d < curr_lod)
			{		
				if (RenderLevel != 0)
				{
					RenderLevel--;
					curr_lod -= factor;
					changeLOD = true;
				}
			}
			else if (RenderLevel < Options.Instance.MaxRenderLevel && d >= (curr_lod * 2))
			{
				curr_lod += factor;
				RenderLevel++;
				changeLOD = true;
			}

			//**************
			//RenderLevel = Options.Instance.MaxRenderLevel;
			//changeLOD = true;
			//**************

			if (changeLOD || this.renderOp.indexData == null)
			{
				Update();
				for (long i = 0; i < 4; i++)
				{
					if (neighbors[i] != null && neighbors[i].IsLoaded)
						neighbors[i].Update ();
				}         
			}
			if (this.needReload)
				Load();
		}

		public override void UpdateRenderQueue(RenderQueue queue)
		{
#if _VisibilityCheck
			if ( mustRender == false )
			{
				return;
			}
#endif
			if ( inUse && isLoaded && inFrustum)
			{   
				RenderableManager.Instance.AddVisible();
				queue.AddRenderable( this );
			}
		}

		/** Overridden, see Renderable */

		public override float GetSquaredViewDepth( Axiom.Core.Camera cam) 
		{
			// Use squared length to avoid square root
			return (this.ParentNode.DerivedPosition - cam.DerivedPosition).LengthSquared;
		}

		public override float BoundingRadius 
		{
			get
			{
				return 0f;
			}
		}

		public  bool IsLoaded 
		{
			get
			{
				return isLoaded;
			}
		}

		public float MaxHeight 
		{
			get
			{
				return box.Maximum.y;
			}
		}

		/// Gets the index data for this tile based on current settings
		protected IndexData getIndexData()
		{
			long stitchFlags = 0;

			if ( neighbors[ (int)Neighbor.East ] != null && neighbors[ (int)Neighbor.East ].IsLoaded && 
				neighbors[ (int)Neighbor.East ].RenderLevel > this.RenderLevel)
			{
				stitchFlags |= (long)Stitch_Direction.East;
				stitchFlags |= (long)
					(neighbors[ (int)Neighbor.East ].RenderLevel - this.RenderLevel) << (int)Stitch_Shift.East;
			}

			if ( neighbors[ (int)Neighbor.West ] != null && neighbors[ (int)Neighbor.West ].IsLoaded && 
				neighbors[ (int)Neighbor.West ].RenderLevel > this.RenderLevel)
			{
				stitchFlags |= (long)Stitch_Direction.West;
				stitchFlags |= (long)
					(neighbors[ (int)Neighbor.West ].RenderLevel - this.RenderLevel) << (int)Stitch_Shift.West;
			}

			if ( neighbors[ (int)Neighbor.North ] != null && neighbors[ (int)Neighbor.North ].IsLoaded && 
				neighbors[ (int)Neighbor.North ].RenderLevel > this.RenderLevel)
			{
				stitchFlags |= (long)Stitch_Direction.North;
				stitchFlags |= (long)
					(neighbors[ (int)Neighbor.North ].RenderLevel - this.RenderLevel) << (int)Stitch_Shift.North;
			}

			if ( neighbors[ (int)Neighbor.South ] != null && neighbors[ (int)Neighbor.South ].IsLoaded && 
				neighbors[ (int)Neighbor.South ].RenderLevel > this.RenderLevel)
			{
				stitchFlags |= (long)Stitch_Direction.South;
				stitchFlags |= (long)
					(neighbors[ (int)Neighbor.South ].RenderLevel - this.RenderLevel) << (int)Stitch_Shift.South;
			}

			return IndexBuffer.Instance.GetIndexData( stitchFlags, 
												   RenderLevel , 
												   neighbors);		

		}

	}

}

