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
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class OceanPage : SimpleRenderable
    {
        private AxisAlignedBox waveBounds;

        private new VertexData vertexData = null;
        private new IndexData indexData;

        private int innerWaveDistance = 150;
        private int outerWaveDistance = 256;
        private int outerViewDistance = 1280;
        private int innerMetersPerSample = 4;
        private int outerMetersPerSample = 32;
        private bool useShaders;

        protected OceanConfig config;

        #region Buffer Management Methods

        public OceanPage(OceanConfig config)
        {
            this.config = config;
            config.ConfigChange += new ConfigChangeHandler(ConfigChanged);

            material = (Material)MaterialManager.Instance.GetByName("MVSMOcean");
            if (material == null) {
                config.ShowOcean = false;
                return;
            }

            material.Load();

            // if the shader technique is supported, then use shaders
            useShaders = material.GetTechnique(0).IsSupported;

            // build the vertex buffer
            BuildVertexBuffer();

            // build the index buffer
            BuildIndexBuffer();

            // force update of shader variables in material
            UpdateMaterial();

            UpdateBounds();

            CastShadows = false;
        }

        private float WaveAmpScale(int x, int z)
        {
            float ret = 1;

            x = Math.Abs(x);
            z = Math.Abs(z);

            int max = Math.Max(x, z);

            if ((max > innerWaveDistance) && (max <outerWaveDistance))
            {
                ret = ((float)(outerWaveDistance - max)) / (float)((outerWaveDistance - innerWaveDistance));
            }
            if (max >= outerWaveDistance)
            {
                ret = 0;
            }

            return ret;            
        }

        private unsafe void outerVertex(float* pData, int x, int z)
        {
            *pData++ = x * TerrainManager.oneMeter;
            if (useShaders)
            {
                *pData++ = 0;
            }
            else
            {
                // if we are not using a shader, then set y coord to sea level
                *pData = config.SeaLevel * TerrainManager.oneMeter;
            }
            *pData++ = z * TerrainManager.oneMeter;

            *pData++ = x;
            *pData++ = z;
        }

        private int numVerticesForPlane(int x1, int x2, int y1, int y2, int metersPerSample)
        {
            int x = x2 - x1;
            int y = y2 - y1;

            return ((x / metersPerSample) + 1) * ((y / metersPerSample) + 1);
        }

        private int numIndicesForPlane(int x1, int x2, int y1, int y2, int metersPerSample)
        {
            int x = x2 - x1;
            int y = y2 - y1;

            return ((x / metersPerSample)) * ((y / metersPerSample)) * 6;
        }

        private unsafe float* fillVertexPlane(float *pData, int x1, int x2, int y1, int y2, int metersPerSample)
        {
            for (int y = y1; y <= y2; y += metersPerSample)
            {
                for (int x = x1; x <= x2; x += metersPerSample)
                {
                    // position
                    *pData++ = x * TerrainManager.oneMeter;
                    if (useShaders)
                    {
                        *pData++ = WaveAmpScale(x, y);
                    }
                    else
                    {
                        *pData++ = config.SeaLevel * TerrainManager.oneMeter;
                    }
                    *pData++ = y * TerrainManager.oneMeter;

                    // texture coords
                    *pData++ = x;
                    *pData++ = y; // 1 - y ??
                }
            }
            return pData;
        }

        private unsafe short* fillIndexPlane(short* pIndex, short startIndex, int x1, int x2, int y1, int y2, int metersPerSample)
        {
            // make tris in a zigzag pattern (strip compatible)

            int width = (short)((x2 - x1) / metersPerSample);
            int height = (short)((y2 - y1) / metersPerSample);
            int stride = width + 1;

            short v1, v2, v3;

            for (int v = 0; v < height; v++)
            {

                for (int u = 0; u < width; u++)
                {
                    // First Tri in cell
                    // -----------------
                    v1 = (short)(((v + 1) * stride) + u);
                    v2 = (short)(((v + 1) * stride) + (u + 1));
                    v3 = (short)((v * stride) + u);
                    // Output indexes
                    *pIndex++ = (short)(v1 + startIndex);
                    *pIndex++ = (short)(v2 + startIndex);
                    *pIndex++ = (short)(v3 + startIndex);
                    // Second Tri in cell
                    // ------------------
                    v1 = (short)(((v + 1) * stride) + (u + 1));
                    v2 = (short)((v * stride) + (u + 1));
                    v3 = (short)((v * stride) + u);
                    // Output indexes
                    *pIndex++ = (short)(v1 + startIndex);
                    *pIndex++ = (short)(v2 + startIndex);
                    *pIndex++ = (short)(v3 + startIndex);
                }
            }
            return pIndex;
        }

        private void BuildVertexBuffer()
        {
            if (vertexData != null)
            {
                // if we already have a buffer, free the hardware buffer
                vertexData.vertexBufferBinding.GetBuffer(0).Dispose();
            }

            //
            // create vertexData object
            //
            vertexData = new VertexData();

            //
            // Set up the vertex declaration
            //
            VertexDeclaration vertexDecl = vertexData.vertexDeclaration;
            int currOffset = 0;

            // add position data
            vertexDecl.AddElement(0, currOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            currOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // add texture coords
            vertexDecl.AddElement(0, currOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            currOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            //
            // create hardware vertex buffer
            //
            int innerVertexCount = numVerticesForPlane(-outerWaveDistance, outerWaveDistance, -outerWaveDistance, outerWaveDistance, innerMetersPerSample);
            int outerVertexCount =
                numVerticesForPlane(-outerViewDistance, outerViewDistance, -outerViewDistance, -outerWaveDistance, outerMetersPerSample) +
                numVerticesForPlane(-outerViewDistance, -outerWaveDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample) +
                numVerticesForPlane(outerWaveDistance, outerViewDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample) +
                numVerticesForPlane(-outerViewDistance, outerViewDistance, outerWaveDistance, outerViewDistance, outerMetersPerSample);

            vertexData.vertexCount = innerVertexCount + outerVertexCount;

            // create a new vertex buffer (based on current API)
            HardwareVertexBuffer vbuf =
                HardwareBufferManager.Instance.CreateVertexBuffer(vertexDecl.GetVertexSize(0), vertexData.vertexCount, BufferUsage.StaticWriteOnly, false);

            // get a reference to the vertex buffer binding
            VertexBufferBinding binding = vertexData.vertexBufferBinding;

            // bind the first vertex buffer
            binding.SetBinding(0, vbuf);

            // generate vertex data
            unsafe
            {
                // lock the vertex buffer
                IntPtr data = vbuf.Lock(BufferLocking.Discard);

                float* pData = (float*)data.ToPointer();

                pData = fillVertexPlane(pData, -outerWaveDistance, outerWaveDistance, -outerWaveDistance, outerWaveDistance, innerMetersPerSample);

                pData = fillVertexPlane(pData, -outerViewDistance, outerViewDistance, -outerViewDistance, -outerWaveDistance, outerMetersPerSample);
                pData = fillVertexPlane(pData, -outerViewDistance, -outerWaveDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample);
                pData = fillVertexPlane(pData, outerWaveDistance, outerViewDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample);
                pData = fillVertexPlane(pData, -outerViewDistance, outerViewDistance, outerWaveDistance, outerViewDistance, outerMetersPerSample);

                // unlock the buffer
                vbuf.Unlock();
            } // unsafe
        }

        private void BuildIndexBuffer()
        {
            //
            // create vertex and index data objects
            //
            indexData = new IndexData();

            //
            // create hardware index buffer
            //
            int innerIndexCount = numIndicesForPlane(-outerWaveDistance, outerWaveDistance, -outerWaveDistance, outerWaveDistance, innerMetersPerSample);
            int outerIndexCount =
                numIndicesForPlane(-outerViewDistance, outerViewDistance, -outerViewDistance, -outerWaveDistance, outerMetersPerSample) +
                numIndicesForPlane(-outerViewDistance, -outerWaveDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample) +
                numIndicesForPlane(outerWaveDistance, outerViewDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample) +
                numIndicesForPlane(-outerViewDistance, outerViewDistance, outerWaveDistance, outerViewDistance, outerMetersPerSample);

            indexData.indexCount = innerIndexCount + outerIndexCount;

            // create the index buffer using the current API
            indexData.indexBuffer =
                HardwareBufferManager.Instance.CreateIndexBuffer(IndexType.Size16, indexData.indexCount, BufferUsage.StaticWriteOnly, false);

            // short v1, v2, v3; (unused)

            // grab a reference for easy access
            HardwareIndexBuffer idxBuffer = indexData.indexBuffer;

            // lock the whole index buffer
            IntPtr data = idxBuffer.Lock(BufferLocking.Discard);
            int startIndex = 0;

            unsafe
            {
                short* pIndex = (short*)data.ToPointer();

                // make tris in a zigzag pattern (strip compatible)

                pIndex = fillIndexPlane(pIndex, (short)startIndex, -outerWaveDistance, outerWaveDistance, -outerWaveDistance, outerWaveDistance, innerMetersPerSample);
                startIndex += numVerticesForPlane(-outerWaveDistance, outerWaveDistance, -outerWaveDistance, outerWaveDistance, innerMetersPerSample);

                pIndex = fillIndexPlane(pIndex, (short)startIndex, -outerViewDistance, outerViewDistance, -outerViewDistance, -outerWaveDistance, outerMetersPerSample);
                startIndex += numVerticesForPlane(-outerViewDistance, outerViewDistance, -outerViewDistance, -outerWaveDistance, outerMetersPerSample);

                pIndex = fillIndexPlane(pIndex, (short)startIndex, -outerViewDistance, -outerWaveDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample);
                startIndex += numVerticesForPlane(-outerViewDistance, -outerWaveDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample);

                pIndex = fillIndexPlane(pIndex, (short)startIndex, outerWaveDistance, outerViewDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample);
                startIndex += numVerticesForPlane(outerWaveDistance, outerViewDistance, -outerWaveDistance, outerWaveDistance, outerMetersPerSample);

                pIndex = fillIndexPlane(pIndex, (short)startIndex, -outerViewDistance, outerViewDistance, outerWaveDistance, outerViewDistance, outerMetersPerSample);

            }
            // unlock the buffer
            idxBuffer.Unlock();
        }

        private unsafe void outerQuad(short *pIndex, short innerVertexCount, short nw, short ne, short se, short sw)
        {
            // First Tri in cell
            // -----------------
            // Output indexes
            *pIndex++ = (short)(innerVertexCount + sw);
            *pIndex++ = (short)(innerVertexCount + nw);
            *pIndex++ = (short)(innerVertexCount + se);
            // Second Tri in cell
            // Output indexes
            *pIndex++ = (short)(innerVertexCount + se);
            *pIndex++ = (short)(innerVertexCount + nw);
            *pIndex++ = (short)(innerVertexCount + ne);
        }

        private void UpdateBounds()
        {
            float radius = outerViewDistance * TerrainManager.oneMeter;
            // set bounding box
            waveBounds = new AxisAlignedBox(new Vector3(-radius, config.SeaLevel - config.WaveHeight, -radius),
                new Vector3(radius, config.SeaLevel + config.WaveHeight, radius));

            this.box = waveBounds;
        }

        #endregion Buffer Management Methods


        protected void UpdateMaterial()
        {
            if (useShaders && config.UseParams)
            {
                GpuProgramParameters vertexParams = material.GetTechnique(0).GetPass(0).VertexProgramParameters;
                vertexParams.SetNamedConstant("seaLevel", new Vector3(config.SeaLevel, 0, 0));
                vertexParams.SetNamedConstant("waveAmp", new Vector3(config.WaveHeight, 0, 0));
                vertexParams.SetNamedConstant("BumpScale", new Vector3(config.BumpScale, 0, 0));
                vertexParams.SetNamedConstant("bumpSpeed", new Vector3(config.BumpSpeedX, config.BumpSpeedZ, 0));
                vertexParams.SetNamedConstant("textureScale", new Vector3(config.TextureScaleX, config.TextureScaleZ, 0));

                GpuProgramParameters fragmentParams = material.GetTechnique(0).GetPass(0).FragmentProgramParameters;
                fragmentParams.SetNamedConstant("deepColor", config.DeepColor);
                fragmentParams.SetNamedConstant("shallowColor", config.ShallowColor);
            }
        }

        protected void ConfigChanged(object sender, EventArgs e)
        {
            UpdateMaterial();
        }

        #region Properties

        public float WaveTime
        {
            set
            {
                // set the gpu parameter for the current time
                if (useShaders)
                {
                    material.GetTechnique(0).GetPass(0).VertexProgramParameters.SetNamedConstant("time", new Vector3(value, 0, 0));
                }
            }
        }

        #endregion Properties

        public override AxisAlignedBox BoundingBox
        {
            get
            {
                return (AxisAlignedBox)waveBounds.Clone();
            }
        }

        public override Sphere GetWorldBoundingSphere(bool derive)
        {
            if (derive)
            {
                worldBoundingSphere.Radius = this.BoundingRadius;
                worldBoundingSphere.Center = parentNode.DerivedPosition;
            }
            else
            {
                worldBoundingSphere.Radius = waveBounds.Maximum.Length;
                worldBoundingSphere.Center = waveBounds.Center;
            }

            return worldBoundingSphere;
        }

        public override void GetRenderOperation(RenderOperation op)
        {
            op.useIndices = true;
            op.operationType = OperationType.TriangleList;
            op.vertexData = vertexData;
            op.indexData = indexData;
        }

        public override float GetSquaredViewDepth(Camera camera)
        {
            // Use squared length to avoid square root
            return (this.ParentNode.DerivedPosition - camera.DerivedPosition).LengthSquared;
        }

        public override float BoundingRadius
        {
            get
            {
                return 0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        public override void UpdateRenderQueue(RenderQueue queue)
        {
            // add ourself to the render queue
            // we render late, since most of the ocean will be rendered over by terrain, and
            // we will benefit from early out in the pixel shader due to z-test
            queue.AddRenderable(this, 1, RenderQueueGroupID.Nine);
        }
    }
}
