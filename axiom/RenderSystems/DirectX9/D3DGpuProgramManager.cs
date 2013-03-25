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
using Axiom.Core;
using Axiom.Graphics;
using Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
	/// <summary>
	/// 	Summary description for D3DGpuProgramManager.
	/// </summary>
	public class D3DGpuProgramManager : GpuProgramManager {
        protected D3D.Device device;
		
		internal D3DGpuProgramManager(D3D.Device device) {
            this.device = device;
		}

        /// <summary>
        ///    Create the specified type of GpuProgram.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override GpuProgram Create(string name, GpuProgramType type, string syntaxCode) {
            switch(type) {
                case GpuProgramType.Vertex:
                    return new D3DVertexProgram(name, device, syntaxCode);

                case GpuProgramType.Fragment:
                    return new D3DFragmentProgram(name, device, syntaxCode);
				default:
					throw new NotSupportedException("The program type is not supported.");
            }
        }

        /// <summary>
        ///    Returns a specialized version of GpuProgramParameters.
        /// </summary>
        /// <returns></returns>
        public override GpuProgramParameters CreateParameters() {
            return new GpuProgramParameters();
        }
	}
}
