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
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;
using System.Diagnostics;

namespace Axiom.SceneManagers.Multiverse
{
    public class TerrainPage : SimpleRenderable, IDisposable
    {
        public enum PageHilightType
        {
            None,
            Colorized,
            EdgeBlend,
            EdgeSharpBlend,
            EdgeSharp
        }

        private Vector3 location;
        private PageCoord pageCoord;
        private Page currentPage;
        private PageHeightMap pageHeightMap;
        private TerrainPatch[,] terrainPatches;
        private int numTiles;
        private int patchSize;

        private int subPageSize;
        private SceneNode sceneNode;

        protected bool hilight;
        protected ITerrainMaterial terrainMaterial;

        protected RenderOperation renderOp;

        protected static VertexDeclaration terrainVertexDeclaration;

        protected static int vertexSize;

        public TerrainPage(Vector3 location, Page page)
        {
            this.location = location;
            pageCoord = new PageCoord(location, TerrainManager.Instance.PageSize);

            terrainMaterial = TerrainManager.Instance.TerrainMaterialConfig.NewTerrainMaterial(pageCoord.X, pageCoord.Z);

            currentPage = page;
            Debug.Assert(location == currentPage.Location, "creating TerrainPage with page at different location");
            numTiles = currentPage.NumTiles;
            patchSize = TerrainManager.Instance.PageSize / numTiles;

            // set up the page height maps for this page of terrain
            subPageSize = TerrainManager.Instance.SubPageSize;
            int subPagesPerPage = TerrainManager.Instance.PageSize / subPageSize;

            pageHeightMap = new PageHeightMap(subPagesPerPage, TerrainManager.Instance.PageSize,
                TerrainManager.Instance.MaxMetersPerSample, TerrainManager.Instance.MinMetersPerSample);

            pageHeightMap.Location = location;

            // create and position a scene node for this terrain page
            string nodeName = String.Format("TerrainPage[{0},{1}]", (int)(location.x / TerrainManager.oneMeter),
                (int)(location.z / TerrainManager.oneMeter));

            // DEBUG - Console.WriteLine("Creating {0}", name);
            sceneNode = TerrainManager.Instance.WorldRootSceneNode.CreateChildSceneNode(nodeName);

            sceneNode.Position = location;

            sceneNode.AttachObject(this);

            // create the render operation
            renderOp = new RenderOperation();
            renderOp.operationType = OperationType.TriangleList;
            renderOp.useIndices = true;

            CreatePatches();

            SetCastShadows();

            UpdateBounds();

            TerrainManager.Instance.ShadowConfig.ShadowTechniqueChange += ShadowTechniqueChangeHandler;
        }

        static TerrainPage()
        {
            terrainVertexDeclaration = HardwareBufferManager.Instance.CreateVertexDeclaration();

            // set up the vertex declaration
            int vDecOffset = 0;
            terrainVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            terrainVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            terrainVertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords);

            vertexSize = terrainVertexDeclaration.GetVertexSize(0) / 4;
        }

        private void SetCastShadows()
        {
            if (TerrainManager.Instance.ShadowConfig.ShadowTechnique == ShadowTechnique.Depth)
            {
                CastShadows = true;
            }
            else
            {
                CastShadows = false;
            }
        }

        private void ShadowTechniqueChangeHandler(object sender, EventArgs e)
        {
            SetCastShadows();
        }

        public void UpdateMaterial()
        {
            terrainMaterial.Dispose();
            terrainMaterial = TerrainManager.Instance.TerrainMaterialConfig.NewTerrainMaterial(pageCoord.X, pageCoord.Z);
        }

        private void UpdateBounds()
        {
            float minHeight;
            float maxHeight;

            // update heights from the heightmaps
            pageHeightMap.GetSubPageHeightBounds(0, 0, TerrainManager.Instance.PageSize, TerrainManager.Instance.PageSize, out minHeight, out maxHeight);

            // set bounding box
            box = new AxisAlignedBox(new Vector3(0, minHeight, 0),
                new Vector3(TerrainManager.Instance.PageSize * TerrainManager.oneMeter, maxHeight, TerrainManager.Instance.PageSize * TerrainManager.oneMeter));

            // set bounding sphere
            worldBoundingSphere.Center = box.Center;
            worldBoundingSphere.Radius = box.Maximum.Length;
        }

        private TerrainPatch NewPatch(int x, int z, int metersPerSample)
        {
            TerrainPatch patch = new TerrainPatch(this, patchSize,
                metersPerSample, x * patchSize, z * patchSize);
            return patch;
        }

        public void ValidateStitches()
        {
            foreach (TerrainPatch patch in terrainPatches)
            {
                patch.ValidateStitch();
            }
        }

        private void CreatePatches()
        {
            Debug.Assert(numTiles == currentPage.NumTiles);

            terrainPatches = new TerrainPatch[numTiles, numTiles];

            Vector3 patchLoc = new Vector3(location.x, 0, location.z);

            // size (in meters) of a patch

            for (int z = 0; z < numTiles; z++)
            {
                patchLoc.x = location.x;
                for (int x = 0; x < numTiles; x++)
                {
                    int metersPerSample = TerrainManager.Instance.MetersPerSample(patchLoc);

                    terrainPatches[x, z] = NewPatch(x, z, metersPerSample);

                    patchLoc.x += (patchSize * TerrainManager.oneMeter);
                }

                patchLoc.z += (patchSize * TerrainManager.oneMeter);
            }
        }

        public void ResetHeightMaps()
        {
            foreach (TerrainPatch patch in terrainPatches)
            {
                patch.ResetHeightMaps();
            }

            FreeBuffers();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ValidateLOD()
        {
            Vector3 patchLoc = new Vector3(location.x, 0, location.z);
            int metersPerSample;
            bool patchUpdated = false;
            for (int z = 0; z < numTiles; z++)
            {
                patchLoc.x = location.x;
                for (int x = 0; x < numTiles; x++)
                {
                    metersPerSample = TerrainManager.Instance.MetersPerSample(patchLoc);
                    if (metersPerSample != terrainPatches[x, z].MetersPerSample)
                    { // if the patch has the wrong metersPerSample, then get rid of it and make a new one
                        terrainPatches[x, z] = NewPatch(x, z, metersPerSample);

                        patchUpdated = true;
                    }
                    // for last column and row only, check lod of next page
                    else if ((x == (numTiles - 1)) || (z == ( numTiles - 1)) )
                    {
                        if (!terrainPatches[x, z].ValidateStitch())
                        {
                            terrainPatches[x, z] = NewPatch(x, z, metersPerSample);
                            patchUpdated = true;
                        }
                    }

                    patchLoc.x += (patchSize * TerrainManager.oneMeter);
                }

                patchLoc.z += (patchSize * TerrainManager.oneMeter);
            }

            if (patchUpdated)
            {
                // free the vertex and index buffers.  This will force a rebuild at next render
                FreeBuffers();
            }
        }

        /// <summary>
        /// As the camera shifts around, the terrainPage will be associated with different Pages
        /// (which are relative to the camera).
        /// </summary>
        public Page CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                currentPage = value;
                Debug.Assert(currentPage.Location == location);
                if (currentPage.NumTiles != numTiles)
                {
                    numTiles = currentPage.NumTiles;
                    patchSize = TerrainManager.Instance.PageSize / currentPage.NumTiles;
                    CreatePatches();
                    UpdateBounds();

                    // free the vertex buffer to force a rebuild with new patches/lods
                    FreeBuffers();
                }
            }
        }

        public Vector3 Location
        {
            get
            {
                return location;
            }
        }

        public PageHeightMap PageHeightMap
        {
            get
            {
                return pageHeightMap;
            }
        }

        public bool Hilight
        {
            get
            {
                return ( terrainMaterial.HighlightType != PageHilightType.None );
            }
        }

        public PageHilightType HilightType
        {
            get
            {
                return terrainMaterial.HighlightType;
            }
            set
            {
                terrainMaterial.HighlightType = value;
            }
        }

        public Texture HilightMask
        {
            get
            {
                return terrainMaterial.HighlightMask;
            }
            set
            {
                terrainMaterial.HighlightMask = value;
            }
        }

        public override Material Material
        {
            get
            {
                return terrainMaterial.Material;
            }
        }

        public static int VertexSize
        {
            get
            {
                return vertexSize;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
             string nodeName = String.Format("TerrainPage[{0},{1}]", (int)(location.x / TerrainManager.oneMeter),
                (int)(location.z / TerrainManager.oneMeter));

            sceneNode.DetachObject(this);
            // DEBUG - Console.WriteLine("Disposing {0}", nodeName);
            sceneNode.Creator.DestroySceneNode(nodeName);

            terrainMaterial.Dispose();
            terrainMaterial = null;

            FreeBuffers();
        }

        #endregion

        protected void buildVertexData()
        {
            // accumulate the number of verts and triangles from the patches
            int numVerts = 0;
            int numTriangles = 0;
            for (int z = 0; z < numTiles; z++)
            {
                for (int x = 0; x < numTiles; x++)
                {
                    numVerts += terrainPatches[x, z].NumVerts;
                    numTriangles += terrainPatches[x, z].NumTriangles;
                }
            }

            //
            // Create the vertex buffer
            //
            VertexData localVertexData = new VertexData();

            localVertexData.vertexCount = numVerts;
            localVertexData.vertexStart = 0;

            // free the original vertex declaration to avoid a leak
            HardwareBufferManager.Instance.DestroyVertexDeclaration(localVertexData.vertexDeclaration);

            localVertexData.vertexDeclaration = terrainVertexDeclaration;

            // create the hardware vertex buffer and set up the buffer binding
            HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                localVertexData.vertexDeclaration.GetVertexSize(0), localVertexData.vertexCount,
                BufferUsage.StaticWriteOnly, false);

            localVertexData.vertexBufferBinding.SetBinding(0, hvBuffer);
            renderOp.vertexData = localVertexData;

            //
            // Create the index buffer
            //
            IndexData localIndexData = new IndexData();

            localIndexData.indexCount = numTriangles * 3;
            localIndexData.indexStart = 0;

            localIndexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                IndexType.Size16, localIndexData.indexCount, BufferUsage.StaticWriteOnly);

            renderOp.indexData = localIndexData;

            // lock the vertex and index buffers
            IntPtr vertexBufferPtr = hvBuffer.Lock(BufferLocking.Discard);
            IntPtr indexBufferPtr = localIndexData.indexBuffer.Lock(BufferLocking.Discard);

            int vertOff = 0;
            int indexOff = 0;

            for (int z = 0; z < numTiles; z++)
            {
                for (int x = 0; x < numTiles; x++)
                {
                    TerrainPatch patch = terrainPatches[x, z];
                    int nv = patch.NumVerts;
                    int nt = patch.NumTriangles;
                    patch.BuildVertexIndexData(vertexBufferPtr, vertOff, indexBufferPtr, indexOff);

                    // update buffer offsets
                    vertOff += nv * VertexSize;
                    indexOff += (3 * nt);
                }
            }

            // unlock the buffers
            localIndexData.indexBuffer.Unlock();
            hvBuffer.Unlock();
        }

        public override void UpdateRenderQueue(RenderQueue queue)
        {
            if (isVisible)
            {
                if (TerrainManager.Instance.DrawTerrain)
                {
                    if (renderOp.vertexData == null)
                    {
                        // the object is visible so we had better make sure it has vertex and index buffers
                        buildVertexData();
                    }

                    // put terrain in its own render queue for easier profiling
                    queue.AddRenderable(this, RenderQueue.DEFAULT_PRIORITY, RenderQueueGroupID.Main);
                }
            }
        }

        protected void FreeBuffers()
        {
            if (renderOp.vertexData != null)
            {
                HardwareBufferManager.Instance.DisposeVertexBuffer(renderOp.vertexData.vertexBufferBinding.GetBuffer(0));
                renderOp.vertexData = null;
            }
            if (renderOp.indexData != null)
            {
                HardwareBufferManager.Instance.DisposeIndexBuffer(renderOp.indexData.indexBuffer);
                renderOp.indexData = null;
            }
        }

        public override void GetRenderOperation(RenderOperation op)
        {
            Debug.Assert(renderOp.vertexData != null, "attempting to render heightField with no vertexData");
            Debug.Assert(renderOp.indexData != null, "attempting to render heightField with no indexData");

            op.useIndices = renderOp.useIndices;
            op.operationType = renderOp.operationType;
            op.vertexData = renderOp.vertexData;
            op.indexData = renderOp.indexData;
        }

        public override void NotifyCurrentCamera(Camera cam)
        {
            if (cam.IsObjectVisible(worldAABB))
            {
                isVisible = true;
            }
            else
            {
                isVisible = false;
                return;
            }
        }

        public override float GetSquaredViewDepth(Camera fromCamera)
        {
            // Use squared length to avoid square root
            return (ParentNode.DerivedPosition - fromCamera.DerivedPosition).LengthSquared;
        }

        public void DumpLOD()
        {
            LogManager.Instance.Write("TerrainPage({0}, {1})", pageCoord.X, pageCoord.Z);
            for (int z = 0; z < numTiles; z++)
            {
                for (int x = 0; x < numTiles; x++)
                {
                    TerrainPatch patch = terrainPatches[x, z];
                    LogManager.Instance.Write("  Patch({0}, {1})", x, z);
                    patch.DumpLOD();
                }
            }
        }

        public override float BoundingRadius
        {
            get
            {
                return 0f;
            }
        }
    }
}
