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
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;
using Axiom.Graphics;

namespace Axiom.RenderSystems.DirectX9 {
    /// <summary>
    /// 	Summary description for D3DVertexDeclaration.
    /// </summary>
    public class D3DVertexDeclaration : Axiom.Graphics.VertexDeclaration {
        #region Member variables

        protected D3D.Device device;
        protected D3D.VertexDeclaration d3dVertexDecl;
        protected bool needsRebuild;

        #endregion
		
        #region Constructors
		
        public D3DVertexDeclaration(D3D.Device device) {
            this.device = device;
        }
		
        #endregion
		
        #region Methods

        public override Axiom.Graphics.VertexElement AddElement(ushort source, int offset, VertexElementType type, VertexElementSemantic semantic, int index) {
            Axiom.Graphics.VertexElement element = base.AddElement(source, offset, type, semantic, index);

            needsRebuild = true;

            return element;
        }

		public override Axiom.Graphics.VertexElement InsertElement(int position, ushort source, int offset, VertexElementType type, VertexElementSemantic semantic, int index)
		{
			Axiom.Graphics.VertexElement element = base.InsertElement(position, source, offset, type, semantic, index);

			needsRebuild = true;

			return element;
		}

		public override void ModifyElement(int elemIndex, ushort source, int offset, VertexElementType type, VertexElementSemantic semantic, int index) {
			base.ModifyElement (elemIndex, source, offset, type, semantic, index);

			needsRebuild = true;
		}


		public override void RemoveElement(VertexElementSemantic semantic, int index) {
			base.RemoveElement(semantic, index);

			needsRebuild = true;
		}

		public override void RemoveElement(int index) {
			base.RemoveElement(index);

			needsRebuild = true;
		}


        #endregion
		
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        /// DOC
        public D3D.VertexDeclaration D3DVertexDecl 
		{
            get {
                // rebuild declaration if things have changed
                if(needsRebuild) {
                    if(d3dVertexDecl != null)
                        d3dVertexDecl.Dispose();

                    // create elements array
                    D3D.VertexElement[] d3dElements = new D3D.VertexElement[elements.Count + 1];
	
                    // loop through and configure each element for D3D
                    for(int i = 0; i < elements.Count; i++) {
                        Axiom.Graphics.VertexElement element = 
                            (Axiom.Graphics.VertexElement)elements[i];

                        d3dElements[i].DeclarationMethod = D3D.DeclarationMethod.Default;
                        d3dElements[i].Offset = (short)element.Offset;
                        d3dElements[i].Stream = (short)element.Source;
                        d3dElements[i].DeclarationType = D3DHelper.ConvertEnum(element.Type);
                        d3dElements[i].DeclarationUsage = D3DHelper.ConvertEnum(element.Semantic);

                        // set usage index explicitly for diffuse and specular, use index for the rest (i.e. texture coord sets)
                        switch(element.Semantic) {
                            case VertexElementSemantic.Diffuse:
                                d3dElements[i].UsageIndex = 0;
                                break;

                            case VertexElementSemantic.Specular:
                                d3dElements[i].UsageIndex = 1;
                                break;

                            default:
                                d3dElements[i].UsageIndex = (byte)element.Index;
                                break;
                        } //  switch

                    } // for

                    // configure the last element to be the end
                    d3dElements[elements.Count] = D3D.VertexElement.VertexDeclarationEnd;

                    // create the new declaration
                    d3dVertexDecl = new D3D.VertexDeclaration(device, d3dElements);

                    // reset the flag
                    needsRebuild = false;
                }

                return d3dVertexDecl; 
            }
        }
		
        #endregion

    }
}
