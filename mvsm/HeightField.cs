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
using System.Collections;
using System.Diagnostics;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for HeightField.
	/// </summary>
	public class HeightField : SimpleRenderable, IDisposable
	{

		// how many meters (units in sample space) per actual sample in this tile.  This will be one
		// near the camera and larger powers of two at lower levels of detail.
		private int metersPerSample;

		// when we are changing LOD, this is the LOD we want to change to
		private int targetMetersPerSample;

		// actual size of the sample array for the current level of detail.
		// NOTE - this is the number of samples along one dimension
		private int numSamples;

		// location of the height map in world space
		private Vector3 location;

		// which tile is this height field attached to
		private Tile tile;

		// height samples for this tile
		private float [] heightMap;
		private float minHeight;
		private float maxHeight;
        private Material normalMaterial;

		// stuff related to rendering of the tile
		private RenderOperation renderOp;

		private StitchRenderable stitchRenderable;

		private void Init(int metersPerSample)
		{
			// set up the material
            normalMaterial = WorldManager.Instance.DefaultTerrainMaterial;

            material = normalMaterial;

			// create the render operation
			renderOp = new RenderOperation();
			renderOp.operationType = OperationType.TriangleList;
			renderOp.useIndices = true;

			location = tile.Location;

			// ask the world manager what LOD we should use
			this.metersPerSample = metersPerSample;

			targetMetersPerSample = metersPerSample;

			// figure out the number of actual samples we need in the tile based on the size
			// and level of detail
			numSamples = tile.Size / metersPerSample;

			// allocate the storage for the height map
			heightMap = new float[numSamples * numSamples];

			return;
		}

        public void FogNotify()
        {
            WorldManager.Instance.SetFogGPUParams(normalMaterial, "fogSettings", "fogColour");
        }

		private void UpdateBounds()
		{
			// set bounding box
			this.box = new AxisAlignedBox( new Vector3(0, minHeight, 0), 
				new Vector3(Size * WorldManager.oneMeter, maxHeight, Size * WorldManager.oneMeter) );

			// set bounding sphere
			worldBoundingSphere.Center = box.Center;
			worldBoundingSphere.Radius = box.Maximum.Length;
		}

		public HeightField(Tile t):base()
		{
			tile = t;

			Init(WorldManager.Instance.MetersPerSample(tile.Location));

			// generate the height map
			WorldManager.Instance.TerrainGenerator.GenerateHeightField(location, Size, metersPerSample, 
				heightMap, out minHeight, out maxHeight);

			UpdateBounds();
		}

		/// <summary>
		/// create a new heightField that has the same LOD as the source,
		/// and 1/4 the area of the source.  Height points are copied from one
		/// quadrant of the source.
		/// If the heightField needs to be higher LOD, it will be adjusted
		/// later and the required points will be generated.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="src"></param>
		/// <param name="quad"></param>
		public HeightField(Tile t, HeightField src, Quadrant quad):base()
		{
			tile = t;

			// in this case we will make the new heightField have the same LOD as
			// the source.  The LOD will be adjusted upward later if necessary.
			Init(src.metersPerSample);
			
			Debug.Assert((numSamples * metersPerSample * 2) == ( src.metersPerSample * src.numSamples), "creating heightfield from source that is not next lower LOD");

			int xoff = 0;
			int zoff = 0;

			switch ( quad )
			{
				case Quadrant.Northeast:
					xoff = src.numSamples / 2;
					zoff = 0;
					break;
				case Quadrant.Northwest:
					xoff = 0;
					zoff = 0;
					break;
				case Quadrant.Southeast:
					xoff = src.numSamples / 2;
					zoff = src.numSamples / 2;
					break;
				case Quadrant.Southwest:
					xoff = 0;
					zoff = src.numSamples / 2;
					break;
			}

			// copy from the source heightMap
			this.CopyHeightFieldScaleUp(src.heightMap, 1, xoff, zoff, src.numSamples); 
			
			// compute the min and max extents of this heightField, since nobody else is
			// going to do it.
			// XXX - do we need to do this?
			minHeight = float.MaxValue;
			maxHeight = float.MinValue;
			foreach ( float h in heightMap ) 
			{
				if ( h < minHeight ) 
				{
					minHeight = h;
				}
				if ( h > maxHeight ) 
				{
					maxHeight = h;
				}
			}

			// generate the height points that were not filled in above
			//WorldManager.Instance.TerrainGenerator.GenerateHeightField(location, Size, metersPerSample, 
			//	heightMap, out minHeight, out maxHeight, src.metersPerSample);

			UpdateBounds();
		}


		/// <summary>
		/// Create a new heightField that is (equal or) lower LOD than the four sources,
		/// Height points are copied from the four sources.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="srcNW"></param>
		/// <param name="srcNE"></param>
		/// <param name="srcSW"></param>
		/// <param name="srcSE"></param>
		public HeightField(Tile t, HeightField srcNW, HeightField srcNE, HeightField srcSW, HeightField srcSE ):base()
		{
			tile = t;

			Init(WorldManager.Instance.MetersPerSample(tile.Location));
			
			// copy from the source heightMap
			int halfSamples = numSamples/2;
			CopyHeightFieldScaleDown(srcSW.heightMap, metersPerSample / srcSW.metersPerSample, srcSW.numSamples, 0, halfSamples);
			CopyHeightFieldScaleDown(srcSE.heightMap, metersPerSample / srcSE.metersPerSample, srcSE.numSamples, halfSamples, halfSamples);
			CopyHeightFieldScaleDown(srcNW.heightMap, metersPerSample / srcNW.metersPerSample, srcNW.numSamples, 0, 0);
			CopyHeightFieldScaleDown(srcNE.heightMap, metersPerSample / srcNE.metersPerSample, srcNE.numSamples, halfSamples, 0);

			// update minHeight and maxHeight by taking the most extreme from the 4 sources.
			// this could result in a bounding box slightly larger than necessary, but its
			// not enough of an issue to warrant scanning the entire heightField looking for
			// the true min and max.
			minHeight = srcSW.minHeight;
			if ( srcSE.minHeight < minHeight ) 
			{
				minHeight = srcSE.minHeight;
			}
			if ( srcNW.minHeight < minHeight ) 
			{
				minHeight = srcNW.minHeight;
			}
			if ( srcNE.minHeight < minHeight ) 
			{
				minHeight = srcNE.minHeight;
			}

			maxHeight = srcSW.maxHeight;
			if ( srcSE.maxHeight > maxHeight ) 
			{
				maxHeight = srcSE.maxHeight;
			}
			if ( srcNW.maxHeight > maxHeight ) 
			{
				maxHeight = srcNW.maxHeight;
			}
			if ( srcNE.maxHeight > maxHeight ) 
			{
				maxHeight = srcNE.maxHeight;
			}
			UpdateBounds();
		}

		private VertexData buildVertexData()
		{
			VertexData vertexData = new VertexData();

			vertexData.vertexCount = numSamples * numSamples;
			vertexData.vertexStart = 0;

			// set up the vertex declaration
			int vDecOffset = 0;
			vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
			vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

			vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
			vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

			vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords);
			vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

			// create the hardware vertex buffer and set up the buffer binding
			HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
				vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,	
				BufferUsage.StaticWriteOnly, false);

			vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

			// lock the vertex buffer
			IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

			int bufferOff = 0;

			unsafe
			{
				float* buffer = (float *) ipBuf.ToPointer();
				int heightMapOffset = 0;
				
				for (int zIndex = 0; zIndex < numSamples; zIndex++ )
				{
					float z = ( zIndex * metersPerSample * WorldManager.oneMeter );

					for (int xIndex = 0; xIndex < numSamples; xIndex++ )
					{
						float height = heightMap[heightMapOffset++];

						// Position
						float x = ( xIndex * metersPerSample * WorldManager.oneMeter );
						buffer[bufferOff++] = x;
						buffer[bufferOff++] = height;
						buffer[bufferOff++] = z;
			
						// normals
						// XXX - this can be optimized quite a bit
						Vector3 norm = tile.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));

						buffer[bufferOff++] = norm.x;
						buffer[bufferOff++] = norm.y;
						buffer[bufferOff++] = norm.z;

						// Texture
						// XXX - assumes one unit of texture space is one page.
						//   how does the vertex shader deal with texture coords?
						buffer[bufferOff++] = ( x + location.x - tile.Page.Location.x ) / 
							( WorldManager.Instance.PageSize * WorldManager.oneMeter );
						buffer[bufferOff++] = ( z + location.z - tile.Page.Location.z ) / 
							( WorldManager.Instance.PageSize * WorldManager.oneMeter );

					}
				}

			}
			hvBuffer.Unlock();

			return vertexData;
		}

		public void AdjustLOD()
		{
			Debug.Assert(location == tile.Location, "AdjustLOD: tile and heightField locations don't match");

			// ask the world manager what LOD we should use
			targetMetersPerSample = WorldManager.Instance.MetersPerSample(location);

			if ( targetMetersPerSample != metersPerSample ) 
			{
				// figure out the number of actual samples we need in the tile based on the size
				// and level of detail
				int oldNumSamples = numSamples;
				int oldMetersPerSample = metersPerSample;
				float [] oldHeightMap = heightMap;

				metersPerSample = targetMetersPerSample;
				numSamples = tile.Size / metersPerSample;

				// allocate the storage for the height map
				heightMap = new float[numSamples * numSamples];

				if ( metersPerSample < oldMetersPerSample ) 
				{
					// go to a higher level of detail
					int scale = oldMetersPerSample / metersPerSample;
					CopyHeightFieldScaleUp(oldHeightMap, scale, 0, 0, oldNumSamples); 

					// generate the height map
					WorldManager.Instance.TerrainGenerator.GenerateHeightField(location, Size, metersPerSample, 
						heightMap, out minHeight, out maxHeight, oldMetersPerSample);
				} 
				else 
				{
					// go to a lower level of detail
					int scale = metersPerSample / oldMetersPerSample;
					CopyHeightFieldScaleDown(oldHeightMap, scale, oldNumSamples, 0, 0);
				}

				// get rid of the old vertex and index buffers
				DisposeBuffers();

				UpdateBounds();
			}
		}

		/// <summary>
		/// Used to copy heightMap samples from a lower to higher level of detail.
		/// Can also be used for equal level of detail.
		/// Will work when source covers same or larger area than dest.
		/// 
		/// The entire destination will be filled sparsely with height samples, and will need to be
		/// filled in with generated points if scale > 1.
		/// 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="scale"></param>
		/// <param name="srcXStart"></param>
		/// <param name="srcZStart"></param>
		protected void CopyHeightFieldScaleUp(float[] src, int scale, int srcXStart, int srcZStart, int srcNumSamples)
		{
            Debug.Assert(scale != 0);
			int srcZ = srcZStart;
			for ( int z = 0; z < numSamples; z += scale ) 
			{
				int srcOff = srcZ * srcNumSamples + srcXStart;
				int destOff = z * numSamples;
				int srcX = srcXStart;
				for ( int x = 0; x < numSamples; x += scale ) 
				{
					heightMap[destOff] = src[srcOff];
					srcX++;
					srcOff++;
					destOff += scale;
				}
				srcZ++;
			}
		}

		/// <summary>
		/// Used to copy heightMap samples from a higher to lower level of detail.
		/// Can also be used for equal level of detail.
		/// Will work when source covers same or smaller area than dest.
		/// The area of the dest that is filled in will have all the samples filled.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="scale"></param>
		/// <param name="srcNumSamples"></param>
		/// <param name="destXStart"></param>
		/// <param name="destZStart"></param>
		protected void CopyHeightFieldScaleDown(float[] src, int scale, int srcNumSamples, int destXStart, int destZStart)
		{
			int destZ = destZStart;

            Debug.Assert(scale != 0);

			for ( int z = 0; z < srcNumSamples; z += scale ) 
			{
				int destX = destXStart;
				int destOff = destZ * numSamples + destX;
				int srcOff = z * srcNumSamples;
				for ( int x = 0; x < srcNumSamples; x += scale ) 
				{
					heightMap[destOff] = src[srcOff];
					destX++;
					destOff++;
					srcOff += scale;
				}
				destZ++;
			}
		}

		private static readonly int floatsPerVert = 8;

		unsafe private void fillVerts(HeightField src, float* buffer, int bufferOff, int numVerts,
			int xIndex, int zIndex, 
			int xIndexIncr, int zIndexIncr,
			float xSampOffset, float zSampOffset, Vector3 pageLocation)
		{
			// convert from vert offset to float offset in the buffer
			bufferOff *= floatsPerVert;

			float unitsPerSample = src.metersPerSample * WorldManager.oneMeter;
			float xWorldOffset = xSampOffset * WorldManager.oneMeter;
			float zWorldOffset = zSampOffset * WorldManager.oneMeter;

			for (int n = 0; n < numVerts; n++ )
			{
				float x = ( xIndex * unitsPerSample ) + xWorldOffset;
				float z = ( zIndex * unitsPerSample ) + zWorldOffset;
				float height = src.heightMap[zIndex * src.numSamples + xIndex];

				// Position
				buffer[bufferOff++] = x;
				buffer[bufferOff++] = height;
				buffer[bufferOff++] = z;
		
				// normals
				// XXX - need to calculate normal here.  Use straight up for now

				Vector3 norm = WorldManager.Instance.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));

				buffer[bufferOff++] = norm.x;
				buffer[bufferOff++] = norm.y;
				buffer[bufferOff++] = norm.z;

				// Texture
				// XXX - assumes one unit of texture space is one page.
				//   how does the vertex shader deal with texture coords?
				buffer[bufferOff++] = ( x + location.x - pageLocation.x ) / 
					( WorldManager.Instance.PageSize * WorldManager.oneMeter );
				buffer[bufferOff++] = ( z + location.z - pageLocation.z ) / 
					( WorldManager.Instance.PageSize * WorldManager.oneMeter );

				xIndex += xIndexIncr;
				zIndex += zIndexIncr;
			}
		}


		private int neighborSamples(StitchType type)
		{
			int ret;

			switch ( type ) 
			{
				case StitchType.ToSame:
					ret = numSamples;
					break;
				case StitchType.ToLower:
					ret = numSamples / 2;
					break;
				case StitchType.ToHigher:
					ret = numSamples * 2;
					break;
				case StitchType.None:
				default:
					Debug.Assert(false, "stitching:attempted to get samples from non-existent neighbor");
					ret = 0;
					break;
			}

			return ret;
		}

		/// <summary>
		/// If the stitchRenderable is not valid, then throw it away.  A new one will
		/// be generated when needed.
		/// </summary>
		/// <param name="southMetersPerSample"></param>
		/// <param name="eastMetersPerSample"></param>
		public void ValidateStitch(int southMetersPerSample, int eastMetersPerSample)
		{
			if ( stitchRenderable != null ) 
			{
				if ( ! stitchRenderable.IsValid(numSamples, southMetersPerSample, eastMetersPerSample) ) 
				{
					stitchRenderable.Dispose();
					stitchRenderable = null;
				}
			}
		}

		public void Stitch(HeightField south1, HeightField south2, HeightField east1, 
			HeightField east2, HeightField southEast, bool stitchMiddleSouth, bool stitchMiddleEast)
		{
			Debug.Assert( ( south2 == null || south2.metersPerSample == south1.metersPerSample ), "south neighbors have different LOD");
			Debug.Assert( ( east2 == null || east2.metersPerSample == east1.metersPerSample ), "east neighbors have different LOD");

			//
			// Determine the stitch types for south and east direction
			//
			StitchType southStitchType;
			StitchType eastStitchType;

			int southMetersPerSample = 0;

			if ( south1 == null ) 
			{
				southStitchType = StitchType.None;
			} 
			else 
			{
				southMetersPerSample = south1.metersPerSample;

				if ( south1.metersPerSample == metersPerSample ) 
				{
					southStitchType = StitchType.ToSame;
				}
				else if ( south1.metersPerSample > metersPerSample ) 
				{
					Debug.Assert(south1.metersPerSample == ( 2 * metersPerSample ), "stitching:south LOD not half");
					southStitchType = StitchType.ToLower;	
				} 
				else 
				{
					Debug.Assert( ( south1.metersPerSample * 2 ) == metersPerSample, "stitching: south LOD not double");
					southStitchType = StitchType.ToHigher;
				}
			}

			int eastMetersPerSample = 0;

			if ( east1 == null ) 
			{
				eastStitchType = StitchType.None;
			}
			else
			{
				eastMetersPerSample = east1.metersPerSample;

				if ( east1.metersPerSample == metersPerSample ) 
				{
					eastStitchType = StitchType.ToSame;
				}
				else if ( east1.metersPerSample > metersPerSample ) 
				{
					Debug.Assert(east1.metersPerSample == ( 2 * metersPerSample ), "stitching:east LOD not half");
					eastStitchType = StitchType.ToLower;	
				} 
				else 
				{
					Debug.Assert( ( east1.metersPerSample * 2 ) == metersPerSample, "stitching:east LOD not double");
					eastStitchType = StitchType.ToHigher;
				}
			}

			if ( stitchRenderable != null ) 
			{
				if ( stitchRenderable.IsValid(numSamples, southMetersPerSample, eastMetersPerSample) ) 
				{ // existing stitchRenderable is still ok, so just use it
					return;
				}
				else	
				{
					stitchRenderable.Dispose();
					stitchRenderable = null;
				}
			}

			//
			// The following combinations are acceptable:
			//  1) both same
			//  2) one same and one lower
			//  3) one same and one higher
			//  4) both lower
			//
			Debug.Assert( ( ( southStitchType == StitchType.ToSame ) || ( eastStitchType == StitchType.ToSame ) ) ||
				( ( southStitchType == StitchType.ToLower ) && ( eastStitchType == StitchType.ToLower ) ) ||
				( ( southStitchType == StitchType.None ) && ( eastStitchType == StitchType.None ) ),
				"stitching:invalid stitchType combination");

			bool bothSame = false;
			bool bothLower = false;
			int vertexCount;

			if ( southStitchType == eastStitchType ) 
			{
				if ( southStitchType == StitchType.ToSame ) 
				{
					bothSame = true;
					vertexCount = numSamples * 4; 
				} 
				else if ( southStitchType == StitchType.ToLower ) 
				{
					bothLower = true;
					vertexCount = numSamples * 3;
				} 
				else 
				{ 
					// both are StitchType.None, which means we are at the NE corner, so we dont need stitching
					return;
				}
			}
			else 
			{
				if ( ( southStitchType == StitchType.ToLower ) || ( eastStitchType == StitchType.ToLower ) ) 
				{
					// one same and one lower
					vertexCount = numSamples * 3 + ( numSamples / 2 );
				} 
				else if ( ( southStitchType == StitchType.ToHigher ) || ( eastStitchType == StitchType.ToHigher ) )
				{
					// one same and one higher
					vertexCount = numSamples * 5;
				}
				else 
				{
					// one same and one none
					vertexCount = numSamples * 2;
				}
			}

			VertexData vertexData = new VertexData();

			vertexData.vertexCount = vertexCount;
			vertexData.vertexStart = 0;

			// set up the vertex declaration
			int vDecOffset = 0;
			vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
			vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

			vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
			vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

			vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords);
			vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

			// create the hardware vertex buffer and set up the buffer binding
			HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
				vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,	
				BufferUsage.StaticWriteOnly, false);

			vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

			// lock the vertex buffer
			IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

			int bufferOff = 0;

			int southSamples = 0;
			int eastSamples = 0;
			int vertOff = 0;

			unsafe
			{
				float* buffer = (float *) ipBuf.ToPointer();

				if ( southStitchType != StitchType.None ) 
				{
					//
					// First the south edge of this tile
					//
					fillVerts(this, buffer, vertOff, numSamples, 0, numSamples - 1, 1, 0, 0, 0, tile.Page.Location);
					vertOff += numSamples;
				}

				if ( eastStitchType != StitchType.None ) 
				{
					int adjust = 0;
					if ( southStitchType == StitchType.None )
					{
						// include the inner corner because it wasnt done with the south edge
						adjust = 1;
					}
					//
					// Now the east edge of the tile
					//
					fillVerts(this, buffer, vertOff, numSamples - 1 + adjust, numSamples - 1, numSamples - 2 + adjust, 0, -1, 0, 0, tile.Page.Location);
					vertOff += ( numSamples - 1 + adjust );
				}

				if ( southStitchType != StitchType.None ) 
				{
					//
					// fill the verts from the south neighbor
					//
					southSamples = neighborSamples(southStitchType);

					if ( south2 == null ) 
					{ // only one southern neighbor

						int xStart = 0;
						int xSampOff = 0;
						if ( stitchMiddleSouth ) 
						{
							// go from bottom to middle of east tile, rather than middle to top
							xStart = southSamples;
							xSampOff = -tile.Size;
						}

						fillVerts(south1, buffer, vertOff, southSamples, xStart, 0, 1, 0, xSampOff, tile.Size, tile.Page.Location);
						vertOff += southSamples;
					} 
					else
					{ // two southern neighbors
						int halfSamples = southSamples / 2;
						fillVerts(south1, buffer, vertOff, halfSamples, 0, 0, 1, 0, 0, tile.Size, tile.Page.Location);
						vertOff += halfSamples;

						fillVerts(south2, buffer, vertOff, halfSamples, 0, 0, 1, 0, south1.tile.Size, tile.Size, tile.Page.Location);
						vertOff += halfSamples;
					}
				}

				if ( ( southStitchType != StitchType.None ) && ( eastStitchType != StitchType.None ) )
				{
					//
					// fill the single sample from the SE neighbor
					//
					if ( southEast == east1 ) 
					{
						fillVerts(southEast, buffer, vertOff, 1, 0, neighborSamples(eastStitchType), 0, 0, tile.Size, 0, tile.Page.Location);
					}
					else if ( southEast == south1 ) 
					{
						fillVerts(southEast, buffer, vertOff, 1, neighborSamples(southStitchType), 0, 0, 0, 0, tile.Size, tile.Page.Location);
					}
					else
					{
						fillVerts(southEast, buffer, vertOff, 1, 0, 0, 0, 0, tile.Size, tile.Size, tile.Page.Location);
					}
					vertOff++;
				}

				if ( eastStitchType != StitchType.None ) 
				{
					//
					// fill the verts from the east neighbor
					//
					eastSamples = neighborSamples(eastStitchType);

					if ( east2 == null ) 
					{ // only one eastern neighbor

						int zStart = eastSamples - 1;
						int zSampOff = 0;
						if ( stitchMiddleEast ) 
						{
							// go from bottom to middle of east tile, rather than middle to top
							zStart = ( eastSamples * 2 ) - 1;
							zSampOff = -tile.Size;
						}

						fillVerts(east1, buffer, vertOff, eastSamples, 0, zStart, 0, -1, tile.Size, zSampOff, tile.Page.Location);
						vertOff += eastSamples;
					}
					else 
					{ // two eastern neighbors
						int halfSamples = eastSamples / 2;

						fillVerts(east2, buffer, vertOff, halfSamples, 0, halfSamples - 1, 0, -1, tile.Size, east1.tile.Size, tile.Page.Location);
						vertOff += halfSamples;

						fillVerts(east1, buffer, vertOff, halfSamples, 0, halfSamples - 1, 0, -1, tile.Size, 0, tile.Page.Location);
						vertOff += halfSamples;
					}
				}

				Debug.Assert(vertexCount == vertOff, "stitching: generated incorrect number of vertices");


			}
			hvBuffer.Unlock();

			IndexData indexData = IndexBufferManager.Instance.GetStitchIndexBuffer(numSamples, southSamples, eastSamples);

			stitchRenderable = new StitchRenderable(this, vertexData, indexData, numSamples, southMetersPerSample, eastMetersPerSample);
		}

		public override void GetRenderOperation(RenderOperation op)
		{
			Debug.Assert(renderOp.vertexData != null, "attempting to render heightField with no vertexData");
			Debug.Assert(renderOp.indexData != null, "attempting to render heightField with no indexData");

			op.useIndices = this.renderOp.useIndices;	
			op.operationType = this.renderOp.operationType;
			op.vertexData = this.renderOp.vertexData;
			op.indexData = this.renderOp.indexData;
		}

		public override void NotifyCurrentCamera( Axiom.Core.Camera cam )
		{
			if (((Camera)(cam)).IsObjectVisible(this.worldAABB))
			{ 
				isVisible = true;
			}
			else
			{
				isVisible = false;
				return;
			}
		}

		public override void UpdateRenderQueue(RenderQueue queue)
		{
			if ( isVisible )
			{
                if (tile.Hilight)
                {
                    material = tile.HilightMaterial;
                }
                else
                {
                    material = normalMaterial;
                }

				if ( renderOp.vertexData == null ) 
				{
					// the object is visible so we had better make sure it has vertex and index buffers
					renderOp.vertexData = buildVertexData();
					renderOp.indexData = IndexBufferManager.Instance.GetTileIndexBuffer(numSamples);
				}

				if ( WorldManager.Instance.DrawTiles ) 
				{
					queue.AddRenderable( this );
				}

				if ( stitchRenderable == null ) 
				{
					tile.Stitch();
				}

				if ( WorldManager.Instance.DrawStitches && ( stitchRenderable != null ) ) 
				{
					queue.AddRenderable( stitchRenderable );
				}
			}
		}

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

		public Tile Tile 
		{
			get 
			{
				return tile;
			}
			set 
			{
				tile = value;
			}
		}

		public int MetersPerSample 
		{
			get 
			{
				return metersPerSample;
			}
		}

		public int NumSamples
		{
			get
			{
				return numSamples;
			}
		}

		public float this[int x,int z]
		{
			get 
			{
				return heightMap[z * numSamples + x];
			}
		}

		public int Size 
		{
			get 
			{
				return metersPerSample * numSamples;
			}
		}

		#region IDisposable Members

		private void DisposeBuffers()
		{
			renderOp.indexData = null;

			if ( renderOp.vertexData != null ) 
			{
				renderOp.vertexData.vertexBufferBinding.GetBuffer(0).Dispose();
				renderOp.vertexData.vertexBufferBinding.SetBinding(0, null);
				renderOp.vertexData = null;
			}
			if ( stitchRenderable != null ) 
			{
				stitchRenderable.Dispose();
				stitchRenderable = null;
			}
		}

		public void Dispose()
		{
            WorldManager.Instance.FogNotify -= new FogNotifyEventHandler(FogNotify);
			DisposeBuffers();
		}

		#endregion
	}

	public enum StitchType 
	{
		None,
		ToLower,
		ToSame,
		ToHigher
	}
}
