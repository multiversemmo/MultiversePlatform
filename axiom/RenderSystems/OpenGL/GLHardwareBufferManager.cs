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
using Axiom.Graphics;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL {
    /// <summary>
    /// 	Summary description for GLHardwareBufferManager.
    /// </summary>
    public class GLHardwareBufferManager : HardwareBufferManager {
        #region Member variables
		
        #endregion
		
        #region Constructors
		
        public GLHardwareBufferManager() {
        }
		
        #endregion
		
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numIndices"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public override HardwareIndexBuffer CreateIndexBuffer(IndexType type, int numIndices, BufferUsage usage) {
            return CreateIndexBuffer(type, numIndices, usage, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numIndices"></param>
        /// <param name="usage"></param>
        /// <param name="useShadowBuffer"></param>
        /// <returns></returns>
        public override HardwareIndexBuffer CreateIndexBuffer(IndexType type, int numIndices, BufferUsage usage, bool useShadowBuffer) {
            GLHardwareIndexBuffer buffer = new GLHardwareIndexBuffer(type, numIndices, usage, useShadowBuffer);
            indexBuffers.Add(buffer);
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexSize"></param>
        /// <param name="numVerts"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public override HardwareVertexBuffer CreateVertexBuffer(int vertexSize, int numVerts, BufferUsage usage) {
            return CreateVertexBuffer(vertexSize, numVerts, usage, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexSize"></param>
        /// <param name="numVerts"></param>
        /// <param name="usage"></param>
        /// <param name="useShadowBuffer"></param>
        /// <returns></returns>
        public override HardwareVertexBuffer CreateVertexBuffer(int vertexSize, int numVerts, BufferUsage usage, bool useShadowBuffer) {
            GLHardwareVertexBuffer buffer = new GLHardwareVertexBuffer(vertexSize, numVerts, usage, useShadowBuffer);
            vertexBuffers.Add(buffer);
            return buffer;
        }

		
        #endregion
    }

    public class GLSoftwareBufferManager : SoftwareBufferManager {
    }
}
