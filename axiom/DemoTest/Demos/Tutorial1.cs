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
#endregion

using System;
using System.Drawing;
using Axiom.Controllers;
using Axiom.Controllers.Canned;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    /// <summary>
    /// 	Sample class which shows the classic spinning triangle, done in the Axiom engine.
    /// </summary>
    public class Tutorial1 : DemoBase {	
        #region Methods

        private Vector4 color = new Vector4(1, 0, 0, 1);

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            base.OnFrameStarted (source, e);

            color.x += e.TimeSinceLastFrame * .6f;
            if(color.x > 1) color.x = 0;

            color.y += e.TimeSinceLastFrame * .6f;
            if(color.y > 1) color.y = 0;

            color.z += e.TimeSinceLastFrame * .6f;
            if(color.z > 1) color.z = 0;
        }

        protected override void CreateScene() {
            // create a 3d line
            Line3d line = new Line3d(new Vector3(0, 0, 30), Vector3.UnitY, 50, ColorEx.Blue);

            Triangle tri = new Triangle(
                new Vector3(-25, 0, 0),
                new Vector3(0, 50, 0),
                new Vector3(25, 0, 0),
                ColorEx.Red,
                ColorEx.Blue,
                ColorEx.Green);

            // create a node for the line
            SceneNode node = scene.RootSceneNode.CreateChildSceneNode();
            SceneNode lineNode = node.CreateChildSceneNode();
            SceneNode triNode = node.CreateChildSceneNode();
            triNode.Position = new Vector3(50, 0, 0);

            // add the line and triangle to the scene
            lineNode.AttachObject(line);
            triNode.AttachObject(tri);

            // create a node rotation controller value, which will mark the specified scene node as a target of the rotation
            // we want to rotate along the Y axis for the triangle and Z for the line (just for the hell of it)
            NodeRotationControllerValue rotate = new NodeRotationControllerValue(triNode, Vector3.UnitY);
            NodeRotationControllerValue rotate2 = new NodeRotationControllerValue(lineNode, Vector3.UnitZ);

            // the multiply controller function will multiply the source controller value by the specified value each frame.
            MultipyControllerFunction func = new MultipyControllerFunction(50);

            // create a new controller, using the rotate and func objects created above.  there are 2 overloads to this method.  the one being
            // used uses an internal FrameTimeControllerValue as the source value by default.  The destination value will be the node, which 
            // is implemented to simply call Rotate on the specified node along the specified axis.  The function will mutiply the given value
            // against the source value, which in this case is the current frame time.  The end result in this demo is that if 50 is specified in the 
            // MultiplyControllerValue, then the node will rotate 50 degrees per second.  since the value is scaled by the frame time, the speed
            // of the rotation will be consistent on all machines regardless of CPU speed.
            ControllerManager.Instance.CreateController(rotate, func);
            ControllerManager.Instance.CreateController(rotate2, func);

            // place the camera in an optimal position
            camera.Position = new Vector3(30, 30, 220);

            window.DebugText = "Spinning triangle - Using custom built geometry";
        }

        #endregion
    }

    /// <summary>
    ///		A class for rendering lines in 3d.
    /// </summary>
    public class Line3d : SimpleRenderable {
        // constants for buffer source bindings
        const int POSITION = 0;
        const int COLOR = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint">Point where the line will start.</param>
        /// <param name="direction">The direction the vector is heading in.</param>
        /// <param name="length">The length (magnitude) of the line vector.</param>
        /// <param name="color">The color which this line should be.</param>
        public Line3d(Vector3 startPoint, Vector3 direction, float length, ColorEx color) {
            // normalize the direction vector to ensure all elements fall in [0,1] range.
            direction.Normalize();

            // calculate the actual endpoint
            Vector3 endPoint = startPoint + (direction * length);

            vertexData = new VertexData();
            vertexData.vertexCount = 2;
            vertexData.vertexStart = 0;
			
            VertexDeclaration decl = vertexData.vertexDeclaration;
            VertexBufferBinding binding = vertexData.vertexBufferBinding;

            // add a position and color element to the declaration
            decl.AddElement(POSITION, 0, VertexElementType.Float3, VertexElementSemantic.Position);
            decl.AddElement(COLOR, 0, VertexElementType.Color, VertexElementSemantic.Diffuse);

            // create a vertex buffer for the position
            HardwareVertexBuffer buffer  =
                HardwareBufferManager.Instance.CreateVertexBuffer(
                decl.GetVertexSize(POSITION), 
                vertexData.vertexCount, 
                BufferUsage.StaticWriteOnly);

            Vector3[] pos = new Vector3[] { startPoint, endPoint };

            // write the data to the position buffer
            buffer.WriteData(0, buffer.Size, pos, true);

            // bind the position buffer
            binding.SetBinding(POSITION, buffer);

            // create a color buffer
            buffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                decl.GetVertexSize(COLOR), 
                vertexData.vertexCount, 
                BufferUsage.StaticWriteOnly);

            uint colorValue = Root.Instance.RenderSystem.ConvertColor(color);

            uint[] colors = new uint[] { colorValue, colorValue };

            // write the data to the position buffer
            buffer.WriteData(0, buffer.Size, colors, true);

            // bind the color buffer
            binding.SetBinding(COLOR, buffer);

            // MATERIAL
            // grab a copy of the BaseWhite material for our use
            Material material = MaterialManager.Instance.GetByName("BaseWhite");
            material = material.Clone("LineMat");
            // disable lighting to vertex colors are used
            material.Lighting = false;
            // set culling to none so the triangle is drawn 2 sided
            material.CullingMode = CullingMode.None;

            this.Material = material;

            // set the bounding box of the line
            this.box = new AxisAlignedBox(startPoint, endPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public override float GetSquaredViewDepth(Camera camera) {
            Vector3 min, max, mid, dist;
            min = box.Minimum;
            max = box.Maximum;
            mid = ((min - max) * 0.5f) + min;
            dist = camera.DerivedPosition - mid;

            return dist.LengthSquared;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        public override void GetRenderOperation(RenderOperation op) {
            op.vertexData = vertexData;
            op.indexData = null;
            op.operationType = OperationType.LineList;
            op.useIndices = false;
        }

        public override float BoundingRadius {
            get {
                return 0;
            }
        }

    }


    /// <summary>
    ///		A class for rendering a simple triangle with colored vertices.
    /// </summary>
    public class Triangle : SimpleRenderable {
        // constants for buffer source bindings
        const int POSITION = 0;
        const int COLOR = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, ColorEx c1, ColorEx c2, ColorEx c3) {
            vertexData = new VertexData();
            vertexData.vertexCount = 3;
            vertexData.vertexStart = 0;
			
            VertexDeclaration decl = vertexData.vertexDeclaration;
            VertexBufferBinding binding = vertexData.vertexBufferBinding;

            // add a position and color element to the declaration
            decl.AddElement(POSITION, 0, VertexElementType.Float3, VertexElementSemantic.Position);
            decl.AddElement(COLOR, 0, VertexElementType.Color, VertexElementSemantic.Diffuse);

            // POSITIONS
            // create a vertex buffer for the position
            HardwareVertexBuffer buffer  =
                HardwareBufferManager.Instance.CreateVertexBuffer(
                decl.GetVertexSize(POSITION), 
                vertexData.vertexCount, 
                BufferUsage.StaticWriteOnly);

            Vector3[] positions = new Vector3[] { v1, v2, v3 };

            // write the positions to the buffer
            buffer.WriteData(0, buffer.Size, positions, true);

            // bind the position buffer
            binding.SetBinding(POSITION, buffer);

            // COLORS
            // create a color buffer
            buffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                decl.GetVertexSize(COLOR), 
                vertexData.vertexCount, 
                BufferUsage.StaticWriteOnly);

            // create an int array of the colors to use.
            // note: these must be converted to the current API's
            // preferred packed int format
            uint[] colors = new uint[] {
                Root.Instance.RenderSystem.ConvertColor(c1),
                Root.Instance.RenderSystem.ConvertColor(c2),
                Root.Instance.RenderSystem.ConvertColor(c3)
            };

            // write the colors to the color buffer
            buffer.WriteData(0, buffer.Size, colors, true);

            // bind the color buffer
            binding.SetBinding(COLOR, buffer);

            // MATERIAL
            // grab a copy of the BaseWhite material for our use
            Material material = MaterialManager.Instance.GetByName("BaseWhite");
            material = material.Clone("TriMat");

            // disable lighting to vertex colors are used
            material.Lighting = false;
            // set culling to none so the triangle is drawn 2 sided
            material.CullingMode = CullingMode.None;

			materialName = "TriMat";

			this.Material = material;

            // set the bounding box of the tri
            // TODO: not right, but good enough for now
            this.box = new AxisAlignedBox(new Vector3(25, 50, 0), new Vector3(-25, 0, 0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public override float GetSquaredViewDepth(Camera camera) {
            Vector3 min, max, mid, dist;
            min = box.Minimum;
            max = box.Maximum;
            mid = ((min - max) * 0.5f) + min;
            dist = camera.DerivedPosition - mid;

            return dist.LengthSquared;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        public override void GetRenderOperation(RenderOperation op) {
            op.vertexData = vertexData;
            op.indexData = null;
            op.operationType = OperationType.TriangleList;
            op.useIndices = false;
        }

        public override float BoundingRadius {
            get {
                return 0;
            }
        }
    }
}
