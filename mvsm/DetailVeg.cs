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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Multiverse
{
	
	/// <summary>
    /// Implement billboarded detail vegitation (grass, small shrubs, flowers, etc) in the area
    /// around the camera.
    /// 
    /// A single vegitation block is created, with plants laid out within that block in 2d(no height).
    /// 9 Instances of the block are created, with heights computed to match the landscape.
    /// 
    /// The vegitation textures are all taken from a single texture atlas, which is organized
    /// with all the textures in a single row.
    /// </summary>
    public class DetailVeg
    {
        private int blockSize;

        // These values define the distances from the camera where the vegetation begins and
        // ends its fade out of visibility.  Anything closer than nearTaperDist is fully
        // visible, and anything past farTaperDist is invisible.
        // private float nearTaperDist; (unused)
        // private float farTaperDist; (unused)

        private Random rand;

        private bool enabled = true;

        private Vector3 cameraLocation;
        private PageCoord cameraPage;

        private SceneNode parentSceneNode;

        private VegetationTile[,] vegetationTiles;

        private float roadClearRadius = 5 * TerrainManager.oneMeter;

        private List<VegetationSemantic> vegetationBoundaries;
		
		private MVImageSet detailVegImageSet;
		
		public DetailVeg(int blockSize, SceneNode parent)
        {
            rand = new Random(0);
            this.blockSize = blockSize;

            parentSceneNode = parent;

			vegetationBoundaries = new List<VegetationSemantic>();
		
			detailVegImageSet = MVImageSet.FindMVImageSet("DetailVeg.imageset");
		}

        public void AddVegetationSemantic(VegetationSemantic vegetationBoundary)
		{
			vegetationBoundaries.Add(vegetationBoundary);
            RefreshVegetation();
		}

        public void RemoveVegetationSemantic(VegetationSemantic vegetationBoundary)
        {
            vegetationBoundaries.Remove(vegetationBoundary);
            RefreshVegetation();
        }
		
		private void FreeVegetationTile(VegetationTile tile)
        {
            if (tile != null)
            {
                tile.Dispose();
            }
        }

        protected void Cleanup()
        {
            if (vegetationTiles != null)
            {
                foreach (VegetationTile tile in vegetationTiles)
                {
                    FreeVegetationTile(tile);
                }
                vegetationTiles = null;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        public void RefreshVegetation()
        {
            // clear out any old vegetation
            Cleanup();

            NewCameraPage(cameraPage);
        }

        public void PerFrameProcessing(Camera camera)
        {
            if (enabled && (camera.DerivedPosition != cameraLocation))
            {
                CameraLocation = camera.DerivedPosition;
            }
        }

        private void NewCameraPage(PageCoord newPage)
        {
            if (enabled && TerrainManager.Instance.CameraSet)
            {
                // free the old vegetation tiles

                VegetationTile[,] newVegetationTiles = new VegetationTile[3, 3];
                if (vegetationTiles != null)
                {
                    foreach (VegetationTile tile in vegetationTiles)
                    {
                        PageCoord pc = tile.pc;
                        int x = pc.X - (newPage.X - 1);
                        int z = pc.Z - (newPage.Z - 1);
                        if ((x >= 0) && (x < 3) && (z >= 0) && (z < 3))
                        {
                            newVegetationTiles[x, z] = tile;
                        }
                        else
                        {
                            FreeVegetationTile(tile);
                        }
                    }
                }

                vegetationTiles = newVegetationTiles;

                for (int x = 0; x < 3; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (vegetationTiles[x, z] == null)
                        {
                            vegetationTiles[x, z] = new VegetationTile(parentSceneNode, vegetationBoundaries, x, z,
                                                                       newPage, blockSize,
                                                                       roadClearRadius, detailVegImageSet, rand);
                        }
                    }
                }

                cameraPage = newPage;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public Vector3 CameraLocation
        {
            get
            {
                return cameraLocation;
            }
            set
            {
                cameraLocation = value;

                PageCoord newCameraPage = new PageCoord(cameraLocation, blockSize);

                if (newCameraPage != cameraPage)
                {
                    NewCameraPage(newCameraPage);
                }
            }
        }

        public float RoadClearRadius
        {
            get
            {
                return roadClearRadius;
            }
            set
            {
                roadClearRadius = value;
            }
        }
    }

	/// <summary>
	/// Provide a class to encapsulate the renderables, the index
	/// buffer and vertex buffer for a tile 
	/// </summary>
	internal class VegetationTile : IDisposable
	{
        private float roadClearRadius;

		internal PlantRenderable plantRenderable = null;

        private List<PlantType> plantTypes;

        internal PageCoord pc;
		
		internal IndexData indexData = null;

        internal int totalInstances = 0;
	
		internal VegetationTile(SceneNode parentSceneNode,
								List<VegetationSemantic> vegetationBoundaries, int x, int z, 
								PageCoord newPage, int blockSize, 
								float roadClearRadius,
								MVImageSet imageSet,
								Random rand)
		{
			this.roadClearRadius = roadClearRadius;
	   
			plantTypes = new List<PlantType>();

			PageCoord tmpPage = new PageCoord(x + newPage.X - 1, z + newPage.Z - 1);
			pc = tmpPage;

			Vector3 blockLoc = tmpPage.WorldLocation(blockSize);
			Vector2 blockMin = new Vector2(blockLoc.x, blockLoc.z);
			Vector2 blockMax = new Vector2(blockMin.x + blockSize * TerrainManager.oneMeter,
										   blockMin.y + blockSize * TerrainManager.oneMeter);

			List<Vector2> roadSegments = TerrainManager.Instance.Roads.IntersectBlock(blockMin, blockMax);

			foreach (VegetationSemantic vegetationBoundary in vegetationBoundaries) 
            {
				Boundary boundary = vegetationBoundary.BoundaryObject;
				if (boundary.IntersectSquare(new Vector3(blockMin.x, 0f, blockMin.y), blockSize)) 
				{
					foreach(PlantType boundaryPlantType in vegetationBoundary.PlantTypes) 
					{
						PlantType plantType = boundaryPlantType.Clone();
						plantType.MaybeUseImageSet(imageSet);
						plantTypes.Add(plantType);
						totalInstances += plantType.AddPlantInstances(boundary, blockMin, blockSize, rand);
					}
				}
			}
			if (totalInstances > 0) 
			{
				BuildIndexBuffer();
				AxisAlignedBox bounds;
				VertexData vertexData = BuildVertexData(blockLoc, blockSize, roadSegments, out bounds);
				plantRenderable = new PlantRenderable(indexData, vertexData, bounds, tmpPage, parentSceneNode, blockLoc);
			}
		}
		
        /// <summary>
        /// Build an index buffer based on the total number of plant instances in a block
        /// </summary>
        private void BuildIndexBuffer()
        {
            // get rid of previous buffer if one exists
            if ( indexData != null ) 
            {
                indexData.indexBuffer.Dispose();
            } 
            else 
            {
			    indexData = new IndexData();
            }

			int bufLength = totalInstances * 6;
			
			indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
				IndexType.Size16, bufLength, BufferUsage.StaticWriteOnly);

			IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0,indexData.indexBuffer.Size,BufferLocking.Discard);
			int indexCount = 0;

			int pos = 0;
			unsafe
			{	
				ushort* indexBuffer = (ushort *)indexBufferPtr.ToPointer();
                for (int i = 0; i < totalInstances; i++)
                {
                    int baseInd = i * 4; // 4 verts per quad
                    indexBuffer[pos++] = (ushort)baseInd;
                    indexBuffer[pos++] = (ushort)(baseInd + 1);
                    indexBuffer[pos++] = (ushort)(baseInd + 2);

                    indexBuffer[pos++] = (ushort)(baseInd);
                    indexBuffer[pos++] = (ushort)(baseInd + 2);
                    indexBuffer[pos++] = (ushort)(baseInd + 3);

                    indexCount += 6;
                } 
			}

			indexData.indexBuffer.Unlock();
			indexData.indexCount = indexCount;
			indexData.indexStart = 0;

            return;
        }

        private VertexData InitVertexBuffer(int numVerts)
        {
            VertexData vertexData = new VertexData();

            vertexData.vertexCount = numVerts;
            vertexData.vertexStart = 0;

            // set up the vertex declaration
            int vDecOffset = 0;
            // Position
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // Normal
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // Texture Coords
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // Vertex offset from base of plant
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 1);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // Wind Params
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 2);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // color
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Color, VertexElementSemantic.Diffuse);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Color);

            // create the hardware vertex buffer and set up the buffer binding
            HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,
                BufferUsage.StaticWriteOnly, false);

            vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

            return vertexData;
        }

		private VertexData BuildVertexData(Vector3 blockLoc, int blockSize, List<Vector2> segments, out AxisAlignedBox bounds)
        {
            VertexData vertexData = InitVertexBuffer(totalInstances * 4);
            
            HardwareVertexBuffer hvBuffer = vertexData.vertexBufferBinding.GetBuffer(0);

            // lock the vertex buffer
            IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

            int bufferOff = 0;
            int count = 0;
            float maxHeight = float.MinValue;
            float minHeight = float.MaxValue;
			float roadClearRadiusSquared = roadClearRadius * roadClearRadius;

            unsafe
            {
                float* buffer = (float*)ipBuf.ToPointer();
                uint* colPtr = (uint*)ipBuf.ToPointer();

                foreach (PlantType ptype in plantTypes)
                {
                    float startX = ptype.AtlasStartX;
                    float endX = ptype.AtlasEndX;
                    float startY = ptype.AtlasStartY;
					float endY = ptype.AtlasEndY;

                    foreach (PlantInstance plant in ptype.Instances)
                    {
                        float x = plant.Location.x;
                        float z = plant.Location.y;
                        bool hidePlant = false;

                        if (segments.Count > 0)
                        {
                            for (int i = 0; i < segments.Count; i += 2)
                            {
                                float distSq = DistanceSquaredPointLine(new Vector2(x + blockLoc.x, z + blockLoc.z),
																		segments[i], segments[i + 1]);
                                if (distSq < roadClearRadiusSquared)
                                {
                                    hidePlant = true;
                                    break;
                                }
                            }
                        }

						if (hidePlant)
                        {
                            // quash billboards that are out of the specified height (altitude) bounds
                            for (int i = 0; i < 4; i++)
                            {
                                // point 0
                                // Position
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;

                                // Normal
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;

                                // Texture
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;

                                // Point offset
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;

                                // Wind Params
                                buffer[bufferOff++] = 0;
                                buffer[bufferOff++] = 0;

                                // Diffuse Color
                                colPtr[bufferOff++] = 0;
                            }
                        }
                        else
                        {
							Vector3 worldLoc = new Vector3(x + blockLoc.x, 0, z + blockLoc.z);
							float height = TerrainManager.Instance.GetTerrainHeight(worldLoc,
												GetHeightMode.Interpolate, GetHeightLOD.MaxLOD);
                            Vector3 norm = TerrainManager.Instance.GetNormalAt(worldLoc);

							// point 0
							// Position
							buffer[bufferOff++] = x;
							buffer[bufferOff++] = height;
							buffer[bufferOff++] = z;

							// Normal
							buffer[bufferOff++] = norm.x;
							buffer[bufferOff++] = norm.y;
							buffer[bufferOff++] = norm.z;

							// Texture
							buffer[bufferOff++] = startX;
							buffer[bufferOff++] = endY;

							// Point offset
							buffer[bufferOff++] = -plant.ScaleWidth / 2;
							buffer[bufferOff++] = 0;

							// Wind Params
							buffer[bufferOff++] = 0;
							buffer[bufferOff++] = 0;

							// Diffuse Color
							colPtr[bufferOff++] = plant.Color.ToARGB();

							// point 1
							// Position
							buffer[bufferOff++] = x;
							buffer[bufferOff++] = height;
							buffer[bufferOff++] = z;

							// Normal
							buffer[bufferOff++] = norm.x;
							buffer[bufferOff++] = norm.y;
							buffer[bufferOff++] = norm.z;

							// Texture
							buffer[bufferOff++] = startX;
							buffer[bufferOff++] = startY;

							// Point offset
							buffer[bufferOff++] = -plant.ScaleWidth / 2;
							buffer[bufferOff++] = plant.ScaleHeight;

							// Wind Params
							buffer[bufferOff++] = plant.WindMagnitude;
							buffer[bufferOff++] = 0;

							// Diffuse Color
							colPtr[bufferOff++] = plant.Color.ToARGB();

							// point 2
							// Position
							buffer[bufferOff++] = x;
							buffer[bufferOff++] = height;
							buffer[bufferOff++] = z;

							// Normal
							buffer[bufferOff++] = norm.x;
							buffer[bufferOff++] = norm.y;
							buffer[bufferOff++] = norm.z;

							// Texture
							buffer[bufferOff++] = endX;
							buffer[bufferOff++] = startY;

							// Point offset
							buffer[bufferOff++] = plant.ScaleWidth / 2;
							buffer[bufferOff++] = plant.ScaleHeight;

							// Wind Params
							buffer[bufferOff++] = plant.WindMagnitude;
							buffer[bufferOff++] = 0;

							// Diffuse Color
							colPtr[bufferOff++] = plant.Color.ToARGB();

							// point 3
							// Position
							buffer[bufferOff++] = x;
							buffer[bufferOff++] = height;
							buffer[bufferOff++] = z;

							// Normal
							buffer[bufferOff++] = norm.x;
							buffer[bufferOff++] = norm.y;
							buffer[bufferOff++] = norm.z;

							// Texture
							buffer[bufferOff++] = endX;
							buffer[bufferOff++] = endY;

							// Point offset
							buffer[bufferOff++] = plant.ScaleWidth / 2;
							buffer[bufferOff++] = 0;

							// Wind Params
							buffer[bufferOff++] = 0;
							buffer[bufferOff++] = 0;

							// Diffuse Color
							colPtr[bufferOff++] = plant.Color.ToARGB();

							// update height bounds
							if (height < minHeight)
								minHeight = height;

							if ((height + plant.ScaleHeight) > maxHeight)
								maxHeight = height + plant.ScaleHeight;

							// swap texture x start and end coords, so that we get billboards in both directions
							float tmp = endX;
							endX = startX;
							startX = tmp;
						}
                        count++;
                    }
                }
            }
            hvBuffer.Unlock();

            Debug.Assert(count == totalInstances);

            bounds = new AxisAlignedBox(new Vector3(0f, minHeight, 0f),
										new Vector3(blockSize * TerrainManager.oneMeter,
													maxHeight, 
													blockSize * TerrainManager.oneMeter));
            return vertexData;
        }

		private static float MagSquared(Vector2 p1, Vector2 p2)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            return dx * dx + dy * dy;
        }

        private static float DistanceSquaredPointLine(Vector2 pt, Vector2 p1, Vector2 p2)
        {
            float ret;
            float magSq = MagSquared(p1, p2);

            float u = (((pt.x - p1.x) * (p2.x - p1.x)) +
                ((pt.y - p1.y) * (p2.y - p1.y))) / magSq;

            if (u < 0)
            {
                ret = MagSquared(p1, pt);
            }
            else if (u > 1)
            {
                ret = MagSquared(p2, pt);
            }
            else
            {
                Vector2 intPt;

                intPt.x = p1.x + u * (p2.x - p1.x);
                intPt.y = p1.y + u * (p2.y - p1.y);

                ret = MagSquared(intPt, pt);
            }

            return ret;
        }

        public void Dispose()
        {
            if (plantRenderable != null)
				plantRenderable.Dispose();
        }

	}
	
	public class PlantRenderable : SimpleRenderable, IDisposable
    {
        private PageCoord pageCoord;
        private SceneNode parentSceneNode;
        private SceneNode sceneNode;

        public PlantRenderable(IndexData ind, VertexData vert, AxisAlignedBox bbox, PageCoord pc, SceneNode parent, Vector3 blockLoc)
        {
            indexData = ind;
            vertexData = vert;
            material = MaterialManager.Instance.GetByName("Multiverse/DetailVeg");
            box = bbox;
			pageCoord = pc;
            parentSceneNode = parent;
            sceneNode = parentSceneNode.CreateChildSceneNode(string.Format("DetailVeg-{0}", pageCoord));
            sceneNode.AttachObject(this);
            sceneNode.Position = new Vector3(blockLoc.x, 0, blockLoc.z);

            this.ShowBoundingBox = false;

            CastShadows = true;
        }


        public override void GetRenderOperation(RenderOperation op)
        {
            op.indexData = indexData;
            op.vertexData = vertexData;
            op.useIndices = true;
            op.operationType = OperationType.TriangleList;

            return;
        }

        public override float GetSquaredViewDepth(Camera camera)
        {
            return (this.ParentNode.DerivedPosition - camera.DerivedPosition).LengthSquared;
        }

        public override float BoundingRadius
        {
            get
            {
                return 0;
            }
        }

        public PageCoord PageCoord
        {
            get
            {
                return pageCoord;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // clean up the scene node
            sceneNode.DetachObject(this);
            sceneNode.Creator.DestroySceneNode(string.Format("DetailVeg-{0}", pageCoord));

            // free the vertex buffer
            if (vertexData != null)
            {
                vertexData.vertexBufferBinding.GetBuffer(0).Dispose();
                vertexData = null;
            }
        }

        #endregion
    }

    public class PlantType
    {
        protected uint numInstances;
        protected string imageName;
        protected float atlasStartX;
        protected float atlasEndX;
        protected float atlasStartY;
        protected float atlasEndY;
        protected float scaleWidthLow;
        protected float scaleWidthHi;
        protected float scaleHeightLow;
        protected float scaleHeightHi;
        protected ColorEx color;
        protected float colorMultLow;
        protected float colorMultHi;
        protected float windMagnitude;
        protected List<PlantInstance> instances;

        public PlantType(uint numInstances, 
						 string imageName, 
						 float atlasStartX, float atlasEndX,
						 float atlasStartY, float atlasEndY,
						 float scaleWidthLow, float scaleWidthHi, 
						 float scaleHeightLow, float scaleHeightHi,
						 ColorEx color, float colorMultLow, float colorMultHi, 
						 float windMagnitude)
        {
            this.numInstances = numInstances;
            this.imageName = imageName;
			this.atlasStartX = atlasStartX;
            this.atlasEndX = atlasEndX;
			this.atlasStartY = atlasStartY;
            this.atlasEndY = atlasEndY;
            this.scaleWidthLow = scaleWidthLow;
            this.scaleWidthHi = scaleWidthHi;
            this.scaleHeightLow = scaleHeightLow;
            this.scaleHeightHi = scaleHeightHi;
            this.color = color;
            this.colorMultLow = colorMultLow;
            this.colorMultHi = colorMultHi;
			this.windMagnitude = windMagnitude;
		}

        // This constructor omits the atlas values, since they will be
        // gotten from the imageName record of the DetailVeg.imageset 
		public PlantType(uint numInstances, string imageName, 
						 float scaleWidthLow, float scaleWidthHi, 
						 float scaleHeightLow, float scaleHeightHi,
						 ColorEx color, float colorMultLow, float colorMultHi, 
						 float windMagnitude)
        {
            this.numInstances = numInstances;
            this.imageName = imageName;
			this.atlasStartX = 0f;
            this.atlasEndX = 0f;
			this.atlasStartY = 0f;
            this.atlasEndY = 0f;
            this.scaleWidthLow = scaleWidthLow;
            this.scaleWidthHi = scaleWidthHi;
            this.scaleHeightLow = scaleHeightLow;
            this.scaleHeightHi = scaleHeightHi;
            this.color = color;
            this.colorMultLow = colorMultLow;
            this.colorMultHi = colorMultHi;
			this.windMagnitude = windMagnitude;
		}

        public PlantType Clone()
		{
			return new PlantType(numInstances, 
								 imageName, 
								 atlasStartX, atlasEndX, atlasStartY, atlasEndY,
								 scaleWidthLow, scaleWidthHi, 
								 scaleHeightLow, scaleHeightHi,
								 color, colorMultLow, colorMultHi, 
								 windMagnitude);
		}
		
		public void MaybeUseImageSet(MVImageSet imageSet)
		{
			if (imageSet != null && imageName != "")
			{
				Vector2 start, size;
				if (imageSet.FindImageStartAndSize(imageName, out start, out size)) {
					atlasStartX = start.x;
					atlasStartY = start.y;
					atlasEndX = start.x + size.x;
					atlasEndY = start.y + size.y;
				}
			}
		}
		
		// Returns the number of instances _actually_ created
		public int AddPlantInstances(Boundary boundary, Vector2 tileLocation, int pageSize, Random rand)
		{
            float scaleWidthRange = scaleWidthHi - scaleWidthLow;
            float scaleHeightRange = scaleHeightHi - scaleHeightLow;
            float colorRange = colorMultHi - colorMultLow;
			instances = new List<PlantInstance>();
			for (int i = 0; i < numInstances; i++)
            {
                // Generate random locations within the block. = 
				Vector2 coordsInTile = new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()) * 
					pageSize * TerrainManager.oneMeter;;
				Vector2 worldCoords =  coordsInTile + tileLocation;
				if (boundary.PointIn(new Vector3(worldCoords.x, 0, worldCoords.y)))
				{
					float colorScale = (((float)rand.NextDouble() * colorRange) + colorMultLow);
					ColorEx tmpColor = new ColorEx(color.r * colorScale, color.g * colorScale, color.b * colorScale);
					instances.Add(new PlantInstance(coordsInTile,
													(float)rand.NextDouble() * scaleWidthRange + scaleWidthLow,
													(float)rand.NextDouble() * scaleHeightRange + scaleHeightLow,
													tmpColor, windMagnitude));
				}
            }
			return instances.Count;
		}
		
		public uint NumInstances
        {
            get 
            {
                return numInstances;
            }
            set
            {
                numInstances = value;
                TerrainManager.Instance.RefreshVegetation();
            }
        }

        public List<PlantInstance> Instances
        {
            get
            {
                return instances;
            }
        }

        public float AtlasStartX
        {
            get
            {
                return atlasStartX;
            }
        }

        public float AtlasEndX
        {
            get
            {
                return atlasEndX;
            }
        }

        public float AtlasStartY
        {
            get
            {
                return atlasStartY;
            }
        }

        public float AtlasEndY
        {
            get
            {
                return atlasEndY;
            }
        }

        public float ScaleWidthLow
        {
            get
            {
                return scaleWidthLow;
            }
			set
			{
				scaleWidthLow = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public float ScaleWidthHi
        {
            get
            {
                return scaleWidthHi;
            }
			set
			{
				scaleWidthHi = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public float ScaleHeightLow
        {
            get
            {
                return scaleHeightLow;
            }
			set
			{
				scaleHeightLow = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public float ScaleHeightHi
        {
            get
            {
                return scaleHeightHi;
            }
			set
			{
				scaleHeightHi = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public string ImageName
        {
            get
            {
                return imageName;
            }
			set
			{
				imageName = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public float ColorMultLow
        {
            get
            {
                return colorMultLow;
            }
			set
			{
				colorMultLow = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public float ColorMultHi
        {
            get
            {
                return colorMultHi;
            }
			set
			{
				colorMultHi = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public float WindMagnitude
        {
            get
            {
				return windMagnitude;
            }
			set
			{
				windMagnitude = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }

        public ColorEx Color
        {
            get
            {
                return color;
            }
			set
			{
				color = value;
				TerrainManager.Instance.RefreshVegetation();
			}
        }
    }

    public class PlantInstance
    {
        private Vector2 location;
        private float scaleHeight;
        private float scaleWidth;
        private ColorEx color;
		private float windMagnitude;

        public PlantInstance(Vector2 loc, float scaleW, float scaleH, ColorEx color, float windMagnitude)
        {
            location = loc;
            scaleHeight = scaleH;
            scaleWidth = scaleW;
            this.color = color;
            this.windMagnitude = windMagnitude;
        }

        public Vector2 Location
        {
            get
            {
                return location;
            }
        }

        public float ScaleHeight
        {
            get
            {
                return scaleHeight;
            }
        }

        public float ScaleWidth
        {
            get
            {
                return scaleWidth;
            }
        }

        public ColorEx Color
        {
            get
            {
                return color;
            }
        }

        public float WindMagnitude
        {
            get
            {
                return windMagnitude;
            }
            set
            {
                windMagnitude = value;
            }
        }
    }
}
