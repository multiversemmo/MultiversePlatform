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

using System;
using Axiom.Graphics;

namespace Axiom.Core {
    /// <summary>
    ///     Patch specialization of <see cref="Mesh" />.
    /// </summary>
    /// <remarks>
    ///     Instances of this class should be created by calling
    ///     <see cref="MeshManager.CreateBezierPatch" />.
    /// </remarks>
    public class PatchMesh : Mesh {
        // ------------------------------------
        #region Fields
        /// <summary>
        ///     Internal surface definition.
        /// </summary>
        protected PatchSurface patchSurface = new PatchSurface();
        /// <summary>
        ///     Vertex declaration, cloned from the input.
        /// </summary>
        protected VertexDeclaration vertexDeclaration;

        #endregion Fields
        // ------------------------------------

        // ------------------------------------
        /// <summary>
        ///     Creates a new PatchMesh.
        /// </summary>
        /// <remarks>
        ///     As defined in <see cref="MeshManager.CreateBezierPatch" />.
        /// </remarks>
        public PatchMesh(string name, System.Array controlPointBuffer, VertexDeclaration declaration,
            int width, int height, int uMaxSubdivisionLevel, int vMaxSubdivisionLevel, VisibleSide visibleSide,
            BufferUsage vbUsage, BufferUsage ibUsage, bool vbUseShadow, bool ibUseShadow) : base(name) {

            vertexBufferUsage = vbUsage;
            useVertexShadowBuffer = vbUseShadow;
            indexBufferUsage = ibUsage;
            useIndexShadowBuffer = ibUseShadow;

            // Init patch builder
            // define the surface
            // NB clone the declaration to make it independent
            vertexDeclaration = (VertexDeclaration)declaration.Clone();
            patchSurface.DefineSurface(controlPointBuffer, vertexDeclaration, width, height,
                PatchSurfaceType.Bezier, uMaxSubdivisionLevel, vMaxSubdivisionLevel, visibleSide);
        }

        public void SetSubdivision(float factor) {
            patchSurface.SubdivisionFactor = factor;
            SubMesh sm = GetSubMesh(0);
            sm.indexData.indexCount = patchSurface.CurrentIndexCount;
        }

        protected override void LoadImpl() {
            SubMesh sm = CreateSubMesh();
            sm.vertexData = new VertexData();
            sm.useSharedVertices = false;

            // Set up the vertex buffer
            sm.vertexData.vertexStart = 0;
            sm.vertexData.vertexCount = patchSurface.RequiredVertexCount;
            sm.vertexData.vertexDeclaration = vertexDeclaration;

            HardwareVertexBuffer buffer = 
                HardwareBufferManager.Instance.CreateVertexBuffer(
                    vertexDeclaration.GetVertexSize(0),
                    sm.vertexData.vertexCount,
                    vertexBufferUsage,
                    useVertexShadowBuffer);

            // bind the vertex buffer
            sm.vertexData.vertexBufferBinding.SetBinding(0, buffer);

            // create the index buffer
            sm.indexData.indexStart = 0;
            sm.indexData.indexCount = patchSurface.RequiredIndexCount;
            sm.indexData.indexBuffer = 
                HardwareBufferManager.Instance.CreateIndexBuffer(
                IndexType.Size16,
                sm.indexData.indexCount,
                indexBufferUsage,
                useIndexShadowBuffer);

            // build the path
            patchSurface.Build(buffer, 0, sm.indexData.indexBuffer, 0);

            // set the bounds
            this.BoundingBox = patchSurface.Bounds;
            this.BoundingSphereRadius = patchSurface.BoundingSphereRadius;
        }
    }
}
