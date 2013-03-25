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
using System.Xml;
using System.Diagnostics;
using Axiom.Graphics;
using Axiom.Core;
using Axiom.MathLib;


namespace Axiom.SceneManagers.Multiverse
{
    public class WaterPlane : SimpleRenderable, IBoundarySemantic
    {

        private Boundary boundary;
        private SceneNode waterSceneNode;
        private float height;
        private RenderOperation renderOp;

        private bool currentVisible = false;
        private bool inBoundary = false;
        private bool isAttached = false;

        public WaterPlane(float height, String name, SceneNode parentSceneNode)
        {
            this.height = height;
            this.name = name;

            // create a scene node
            if (parentSceneNode == null)
            {
                parentSceneNode = TerrainManager.Instance.RootSceneNode;
            }
            waterSceneNode = parentSceneNode.CreateChildSceneNode(name);

            // set up material
            material = TerrainManager.Instance.WaterMaterial;

            CastShadows = false;
        }

        public string Type
        {
            get
            {
                return ("WaterPlane");
            }
        }

        public WaterPlane(SceneNode parentSceneNode, XmlTextReader r)
        {
            FromXML(r);

            // create a scene node
            if (parentSceneNode == null)
            {
                parentSceneNode = TerrainManager.Instance.RootSceneNode;
            }
            waterSceneNode = parentSceneNode.CreateChildSceneNode(name);

            // set up material
            material = TerrainManager.Instance.WaterMaterial;
        }

        public void ToXML(XmlTextWriter w)
        {
            w.WriteStartElement("boundarySemantic");
            w.WriteAttributeString("type", "WaterPlane");
            w.WriteElementString("height", height.ToString());
            w.WriteElementString("name", name);

            w.WriteEndElement();
        }

        private void FromXML(XmlTextReader r)
        {
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    ParseElement(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the terrain description
                    return;
                }
            }
        }

        protected void ParseElement(XmlTextReader r)
        {
            // set the field in this object based on the element we just read
            switch (r.Name)
            {
                case "height":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }

                    height = float.Parse(r.Value);
                    break;

                case "name":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }
                    name = r.Value;

                    break;
            }

            // error out if we dont see an end element here
            r.Read();
            if (r.NodeType != XmlNodeType.EndElement)
            {
                return;
            }
        }

        private void BuildBoundingBox()
        {
            // set up bounding box
            Vector3 minBounds = boundary.Bounds.Minimum;
            Vector3 maxBounds = boundary.Bounds.Maximum;
            minBounds.y = height;
            maxBounds.y = height;
            this.box = new AxisAlignedBox(minBounds, maxBounds);
        }

        public void AddToBoundary(Boundary boundary)
        {
            inBoundary = true;
            this.boundary = boundary;

            BuildBoundingBox();

            BuildBuffers();

            PageShift();
        }

        public void RemoveFromBoundary()
        {
            inBoundary = false;
            boundary = null;

            if (isAttached)
            {
                waterSceneNode.DetachObject(this);
                isAttached = false;
            }
            box = null;

            DisposeBuffers();
        }

        private void BuildBuffers()
        {
            //
            // Build the vertex buffer
            //

            List<Vector2> points = boundary.Points;
            List<int[]> indices = boundary.Triangles;

            VertexData vertexData = new VertexData();

            vertexData.vertexCount = boundary.Points.Count;
            vertexData.vertexStart = 0;

            // set up the vertex declaration
            int vDecOffset = 0;
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // create the hardware vertex buffer and set up the buffer binding
            HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,
                BufferUsage.StaticWriteOnly, false);

            vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

            // lock the vertex buffer
            IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

            int bufferOff = 0;

            float minx = boundary.Bounds.Minimum.x;
            float minz = boundary.Bounds.Minimum.z;

            unsafe
            {
                float* buffer = (float*)ipBuf.ToPointer();

                for (int v = 0; v < vertexData.vertexCount; v++)
                {

                    // Position
                    buffer[bufferOff++] = points[v].x;
                    buffer[bufferOff++] = height;
                    buffer[bufferOff++] = points[v].y;

                    // normals
                    buffer[bufferOff++] = 0;
                    buffer[bufferOff++] = 1;
                    buffer[bufferOff++] = 0;

                    // Texture
                    float tmpu = ( points[v].x - minx ) / (128 * TerrainManager.oneMeter);
                    float tmpv = ( points[v].y - minz )/ (128 * TerrainManager.oneMeter);

                    buffer[bufferOff++] = tmpu;
                    buffer[bufferOff++] = tmpv;
                }
            }
            hvBuffer.Unlock();

            //
            // build the index buffer
            //
            IndexData indexData = new IndexData();

            int numIndices = indices.Count * 3;

            indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                IndexType.Size16, numIndices, BufferUsage.StaticWriteOnly);

            IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0, indexData.indexBuffer.Size, BufferLocking.Discard);

            unsafe
            {
                ushort* indexBuffer = (ushort*)indexBufferPtr.ToPointer();
                for (int i = 0; i < indices.Count; i++)
                {
                    indexBuffer[i * 3] = (ushort)indices[i][0];
                    indexBuffer[i * 3 + 1] = (ushort)indices[i][1];
                    indexBuffer[i * 3 + 2] = (ushort)indices[i][2];
                }
            }

            indexData.indexBuffer.Unlock();

            indexData.indexCount = numIndices;
            indexData.indexStart = 0;

            renderOp = new RenderOperation();

            renderOp.vertexData = vertexData;
            renderOp.indexData = indexData;
            renderOp.operationType = OperationType.TriangleList;
            renderOp.useIndices = true;
        }

        #region IBoundarySemantic Members

        public void PerFrameProcessing(float time, Camera camera)
        {
            Debug.Assert(inBoundary);
            return;
        }

        public void PageShift()
        {
            Debug.Assert(inBoundary);

            if (boundary.Visible != currentVisible)
            {
                currentVisible = boundary.Visible;
                if (currentVisible)
                {
                    waterSceneNode.AttachObject(this);
                    isAttached = true;
                }
                else
                {
                    waterSceneNode.DetachObject(this);
                    isAttached = false;
                }
            }
            
        }

        public void BoundaryChange()
        {
            Debug.Assert(inBoundary);
            DisposeBuffers();
            renderOp = null;

            BuildBuffers();
        }

        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                BuildBoundingBox();
                waterSceneNode.NeedUpdate();
                BoundaryChange();
            }
        }

        #endregion

        #region IDisposable Members

        
        private void DisposeBuffers()
        {
            if (renderOp.vertexData != null)
            {
                renderOp.vertexData.vertexBufferBinding.GetBuffer(0).Dispose();
                renderOp.vertexData = null;
            }

            if (renderOp.indexData != null)
            {
                renderOp.indexData.indexBuffer.Dispose();
                renderOp.indexData = null;
            }
        }

        public void Dispose()
        {
            DisposeBuffers();

            waterSceneNode.Creator.DestroySceneNode(waterSceneNode.Name);
        }

        #endregion

        public override void GetRenderOperation(RenderOperation op)
        {
            Debug.Assert(inBoundary);
            Debug.Assert(renderOp.vertexData != null, "attempting to render heightField with no vertexData");
            Debug.Assert(renderOp.indexData != null, "attempting to render heightField with no indexData");

            op.useIndices = this.renderOp.useIndices;
            op.operationType = this.renderOp.operationType;
            op.vertexData = this.renderOp.vertexData;
            op.indexData = this.renderOp.indexData;
        }

        public override float GetSquaredViewDepth(Axiom.Core.Camera camera)
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

        public override void NotifyCurrentCamera(Axiom.Core.Camera cam)
        {
            Debug.Assert(inBoundary);

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
            Debug.Assert(inBoundary);

            if (isVisible)
            {
                queue.AddRenderable(this);
            }
        }

    }
}
