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
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class TreeBillboardRenderOp
    {
        public RenderOperation renderOp;
        public int alpha;
        public string textureName;

        public TreeBillboardRenderOp(string textureName, int alpha, RenderOperation renderOp)
        {
            this.textureName = textureName;
            this.alpha = alpha;
            this.renderOp = renderOp;
        }
    }

    public class TreeBillboardRenderer : IRenderable
    {
        protected Dictionary<string, Dictionary<int, List<float[]>>> billboards = null;

        protected Material billboardMaterial;

        protected List<TreeBillboardRenderOp> renderOps = new List<TreeBillboardRenderOp>();

        public SceneNode parentNode;

        public TreeBillboardRenderer()
        {
            billboardMaterial = MaterialManager.Instance.GetByName("SpeedTree/Billboard");
            billboardMaterial.Load();
            parentNode = TerrainManager.Instance.RootSceneNode;
        }

        public void StartRebuild()
        {
            billboards = new Dictionary<string, Dictionary<int, List<float[]>>>();

            FreeRenderOps();
        }

        /// <summary>
        /// free all the billboard vertex buffers, and clear renderOps
        /// </summary>
        protected void FreeRenderOps()
        {
            foreach (TreeBillboardRenderOp op in renderOps)
            {
                HardwareVertexBuffer vb = op.renderOp.vertexData.vertexBufferBinding.GetBuffer(0);
                vb.Dispose();
            }
            renderOps.Clear();
        }

        /// <summary>
        /// generates renderOps from billboards.  nulls billboards when done.
        /// </summary>
        public void FinishRebuild()
        {
            foreach (string textureName in billboards.Keys)
            {
                foreach (int alpha in billboards[textureName].Keys)
                {
                    List<float[]>bbList = billboards[textureName][alpha];

                    RenderOperation renderOp = new RenderOperation();
                    renderOp.operationType = OperationType.TriangleList;
                    renderOp.useIndices = false;

                    VertexData vertexData = new VertexData();

                    vertexData.vertexCount = 6 * bbList.Count;
                    vertexData.vertexStart = 0;

                    // free the original vertex declaration to avoid a leak
                    HardwareBufferManager.Instance.DestroyVertexDeclaration(vertexData.vertexDeclaration);

                    // use common vertex declaration
                    vertexData.vertexDeclaration = TreeGroup.BillboardVertexDeclaration;

                    // create the hardware vertex buffer and set up the buffer binding
                    HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                        vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,
                        BufferUsage.StaticWriteOnly, false);

                    vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

                    renderOp.vertexData = vertexData;

                    // lock the vertex buffer
        			IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

	        		int bufferOff = 0;

                    unsafe
                    {
                        float* buffer = (float*)ipBuf.ToPointer();
                        foreach (float[] src in bbList)
                        {
                            buffer[bufferOff++] = src[0];
                            buffer[bufferOff++] = src[1];
                            buffer[bufferOff++] = src[2];
                            buffer[bufferOff++] = src[3];
                            buffer[bufferOff++] = src[4];

                            buffer[bufferOff++] = src[5];
                            buffer[bufferOff++] = src[6];
                            buffer[bufferOff++] = src[7];
                            buffer[bufferOff++] = src[8];
                            buffer[bufferOff++] = src[9];

                            buffer[bufferOff++] = src[10];
                            buffer[bufferOff++] = src[11];
                            buffer[bufferOff++] = src[12];
                            buffer[bufferOff++] = src[13];
                            buffer[bufferOff++] = src[14];

                            buffer[bufferOff++] = src[10];
                            buffer[bufferOff++] = src[11];
                            buffer[bufferOff++] = src[12];
                            buffer[bufferOff++] = src[13];
                            buffer[bufferOff++] = src[14];

                            buffer[bufferOff++] = src[15];
                            buffer[bufferOff++] = src[16];
                            buffer[bufferOff++] = src[17];
                            buffer[bufferOff++] = src[18];
                            buffer[bufferOff++] = src[19];

                            buffer[bufferOff++] = src[0];
                            buffer[bufferOff++] = src[1];
                            buffer[bufferOff++] = src[2];
                            buffer[bufferOff++] = src[3];
                            buffer[bufferOff++] = src[4];
                        }
                    }
                    hvBuffer.Unlock();

                    TreeBillboardRenderOp op = new TreeBillboardRenderOp(textureName, alpha, renderOp);

                    renderOps.Add(op);
                }
            }

            billboards = null;
        }

        public void AddBillboard(string textureName, int alpha, float[] billboard)
        {
            if (!billboards.ContainsKey(textureName))
            {
                billboards[textureName] = new Dictionary<int, List<float[]>>();
            }
            if (!billboards[textureName].ContainsKey(alpha))
            {
                billboards[textureName][alpha] = new List<float[]>();
            }
            billboards[textureName][alpha].Add(billboard);
        }

        public void Render(RenderSystem targetRenderSystem)
        {
            ((Axiom.SceneManagers.Multiverse.SceneManager)TerrainManager.Instance.SceneManager).SetTreeRenderPass(billboardMaterial.GetBestTechnique(0).GetPass(0), this);

            string lastTextureName = null;
            foreach (TreeBillboardRenderOp op in renderOps)
            {
                // set the texture if necessary
                if (op.textureName != lastTextureName)
                {
                    targetRenderSystem.SetTexture(0, true, op.textureName);
                    lastTextureName = op.textureName;
                }

                targetRenderSystem.SetAlphaRejectSettings(CompareFunction.Greater, (byte)op.alpha);

                targetRenderSystem.Render(op.renderOp);
            }
        }

        public void PerFrameProcessing()
        {

        }

        #region Singleton
        private static TreeBillboardRenderer instance = null;

        public static TreeBillboardRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TreeBillboardRenderer();
                }
                return instance;
            }
        }
        #endregion Singleton


        #region IRenderable Members

        public bool CastsShadows
        {
            get
            {
                return false;
            }
        }

        public Material Material
        {
            get
            {
                return billboardMaterial;
            }
        }

        public Technique Technique
        {
            get
            {
                return Material.GetBestTechnique();
            }
        }

        public void GetRenderOperation(RenderOperation op)
        {
            throw new NotImplementedException();
        }

        public void GetWorldTransforms(Axiom.MathLib.Matrix4[] matrices)
        {
            matrices[0] = Matrix4.Identity;
        }

        public List<Light> Lights
        {
            get
            {
                return parentNode.Lights;
            }
        }

        public bool NormalizeNormals
        {
            get
            {
                return false;
            }
        }

        public ushort NumWorldTransforms
        {
            get
            {
                return 1;
            }
        }

        public bool UseIdentityProjection
        {
            get
            {
                return false;
            }
        }

        public bool UseIdentityView
        {
            get
            {
                return false;
            }
        }

        public SceneDetailLevel RenderDetail
        {
            get
            {
                return SceneDetailLevel.Solid;
            }
        }

        public Axiom.MathLib.Quaternion WorldOrientation
        {
            get
            {
                return Quaternion.Identity;
            }
        }

        public Axiom.MathLib.Vector3 WorldPosition
        {
            get
            {
                return Vector3.Zero;
            }
        }

        public float GetSquaredViewDepth(Camera camera)
        {
            throw new NotImplementedException();
        }

        public Axiom.MathLib.Vector4 GetCustomParameter(int index)
        {
            throw new NotImplementedException();
        }

        public void SetCustomParameter(int index, Axiom.MathLib.Vector4 val)
        {
            throw new NotImplementedException();
        }

        public void UpdateCustomGpuParameter(GpuProgramParameters.AutoConstantEntry constant, GpuProgramParameters parameters)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
