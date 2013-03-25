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
using System.Diagnostics;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.Scripting;
using Axiom.Graphics;

namespace Axiom.Overlays.Elements {
	/// <summary>
	/// 	GuiElement representing a flat, single-material (or transparent) panel which can contain other elements.
	/// </summary>
	/// <remarks>
	/// 	This class subclasses GuiContainer because it can contain other elements. Like other
	/// 	containers, if hidden it's contents are also hidden, if moved it's contents also move etc. 
	/// 	The panel itself is a 2D rectangle which is either completely transparent, or is rendered 
	/// 	with a single material. The texture(s) on the panel can be tiled depending on your requirements.
	/// 	<p/>
	/// 	This component is suitable for backgrounds and grouping other elements. Note that because
	/// 	it has a single repeating material it cannot have a discrete border (unless the texture has one and
	/// 	the texture is tiled only once). For a bordered panel, see it's subclass BorderPanel.
	/// 	<p/>
	/// 	Note that the material can have all the usual effects applied to it like multiple texture
	/// 	layers, scrolling / animated textures etc. For multiple texture layers, you have to set 
	/// 	the tiling level for each layer.
	/// </remarks>
	public class Panel : OverlayElementContainer {
		#region Member variables
		
        protected float[] tileX = new float[Config.MaxTextureLayers];
        protected float[] tileY = new float[Config.MaxTextureLayers];
        protected bool isTransparent;
        protected int numTexCoords;
        protected RenderOperation renderOp = new RenderOperation();

        // source bindings for vertex buffers
        const int POSITION = 0;
        const int TEXTURE_COORDS = 1;

		#endregion
		
		#region Constructors
		
		internal Panel(string name) : base(name) {
            // initialize the default tiling to 1 for all layers
            for(int i = 0; i < Config.MaxTextureLayers; i++) {
                tileX[i] = 1.0f;
                tileY[i] = 1.0f;
            }
		}
		
		#endregion
		
		#region Methods
		
        /// <summary>
        ///    Returns the geometry to use during rendering.
        /// </summary>
        /// <param name="op"></param>
        public override void GetRenderOperation(Axiom.Graphics.RenderOperation op) {
            op.vertexData = renderOp.vertexData;
            op.operationType = renderOp.operationType;
            op.useIndices = renderOp.useIndices;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize() {
            // setup the vertex data
            renderOp.vertexData = new VertexData();

            // Vertex declaration: 1 position, add texcoords later depending on #layers
            // Create as separate buffers so we can lock & discard separately
            VertexDeclaration decl = renderOp.vertexData.vertexDeclaration;
            decl.AddElement(POSITION, 0, VertexElementType.Float3, VertexElementSemantic.Position);
            renderOp.vertexData.vertexStart = 0;
            renderOp.vertexData.vertexCount = 4;
            
            // create the first vertex buffer, mostly static except during resizing
            HardwareVertexBuffer buffer =
                HardwareBufferManager.Instance.CreateVertexBuffer(
                     decl.GetVertexSize(POSITION),
                     renderOp.vertexData.vertexCount,
                     BufferUsage.StaticWriteOnly);

            // bind the vertex buffer
            renderOp.vertexData.vertexBufferBinding.SetBinding(POSITION, buffer);

            // no indices, and issue as a tri strip
            renderOp.useIndices = false;
            renderOp.operationType = OperationType.TriangleStrip;
        }

        /// <summary>
        ///    
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="layer"></param>
        public void SetTiling(float x, float y, int layer) {
            Debug.Assert(layer < Config.MaxTextureLayers, "layer < Config.MaxTextureLayers");
            Debug.Assert(x != 0 && y != 0, "tileX != 0 && tileY != 0");

            tileX[layer] = x;
            tileY[layer] = y;

            UpdateTextureGeometry();
        }

        /// <summary>
        ///    Internal method for setting up geometry, called by GuiElement.Update
        /// </summary>
        protected override void UpdatePositionGeometry() {
            /*
                0-----2
                |    /|
                |  /  |
                |/    |
                1-----3
            */
            float left, right, top, bottom;
            left = right = top = bottom = 0.0f;

            /* Convert positions into -1, 1 coordinate space (homogenous clip space).
                - Left / right is simple range conversion
                - Top / bottom also need inverting since y is upside down - this means
                  that top will end up greater than bottom and when computing texture
                  coordinates, we have to flip the v-axis (ie. subtract the value from
                  1.0 to get the actual correct value).
            */

            left = this.DerivedLeft * 2 - 1;
            right = left + (width * 2);
            top = -((this.DerivedTop * 2) - 1);
            bottom = top - (height * 2);

            // get a reference to the position buffer
            HardwareVertexBuffer buffer =
                renderOp.vertexData.vertexBufferBinding.GetBuffer(POSITION);

            // lock the buffer
            IntPtr data = buffer.Lock(BufferLocking.Discard);
            int index = 0;

            // modify the position data
            unsafe {
                float* posPtr = (float*)data.ToPointer();

                posPtr[index++] = left;
                posPtr[index++] = top;
                posPtr[index++] = -1;

                posPtr[index++] = left;
                posPtr[index++] = bottom;
                posPtr[index++] = -1;

                posPtr[index++] = right;
                posPtr[index++] = top;
                posPtr[index++] = -1;

                posPtr[index++] = right;
                posPtr[index++] = bottom;
                posPtr[index++] = -1;
            }

            // unlock the position buffer
            buffer.Unlock();
        }

        public override void UpdateRenderQueue(RenderQueue queue) {
            if(isVisible) {
                // only add this panel to the render queue if it is not transparent
                // that would mean the panel should be a virtual container of sorts,
                // and the children would still be rendered
                if(!isTransparent && material != null) {
                    base.UpdateRenderQueue (queue);
                }

                for(int i = 0; i < childList.Count; i++) {
                    ((OverlayElement)childList[i]).UpdateRenderQueue(queue);
                }
            }
        }


        /// <summary>
        ///    Called to update the texture coords when layers change.
        /// </summary>
        protected virtual void UpdateTextureGeometry() {
            if(material != null) {
                int numLayers = material.GetTechnique(0).GetPass(0).NumTextureUnitStages;

                VertexDeclaration decl = renderOp.vertexData.vertexDeclaration;

                // if the required layers is less than the current amount of tex coord buffers, remove
                // the extraneous buffers
                if(numTexCoords > numLayers) {
                    for(int i = numTexCoords; i > numLayers; --i) {
                        // TODO: Implement RemoveElement
                        //decl.RemoveElement(TEXTURE_COORDS, i);
                    }
                }
                else if(numTexCoords < numLayers) {
                    // we need to add more buffers
                    int offset = VertexElement.GetTypeSize(VertexElementType.Float2) * numTexCoords;

                    for(int i = numTexCoords; i < numLayers; i++) {
                        decl.AddElement(TEXTURE_COORDS, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords, i);
                        offset += VertexElement.GetTypeSize(VertexElementType.Float2);
                    } // for
                } // if

                // if the number of layers changed at all, we'll need to reallocate buffer
                if(numTexCoords != numLayers) {
                    HardwareVertexBuffer newBuffer =
                        HardwareBufferManager.Instance.CreateVertexBuffer(
                            decl.GetVertexSize(TEXTURE_COORDS),
                            renderOp.vertexData.vertexCount,
                            BufferUsage.StaticWriteOnly);

                    // Bind buffer, note this will unbind the old one and destroy the buffer it had
                    renderOp.vertexData.vertexBufferBinding.SetBinding(TEXTURE_COORDS, newBuffer);

                    // record the current number of tex layers now
                    numTexCoords = numLayers;
                } // if

                // get the tex coord buffer
                HardwareVertexBuffer buffer = renderOp.vertexData.vertexBufferBinding.GetBuffer(TEXTURE_COORDS);
                IntPtr data = buffer.Lock(BufferLocking.Discard);

                unsafe {

                    float* texPtr = (float*)data.ToPointer();
                    int texIndex = 0;

                    int uvSize = VertexElement.GetTypeSize(VertexElementType.Float2) / sizeof(float);
                    int vertexSize = decl.GetVertexSize(TEXTURE_COORDS) / sizeof(float);

                    for(int i = 0; i < numLayers; i++) {
                        // Calc upper tex coords
                        float upperX = 1.0f * tileX[i];
                        float upperY = 1.0f * tileY[i];
                
                        /*
                            0-----2
                            |    /|
                            |  /  |
                            |/    |
                            1-----3
                        */
                        // Find start offset for this set
                        texIndex = (i * uvSize);

                        texPtr[texIndex] = 0.0f;
                        texPtr[texIndex + 1] = 0.0f;

                        texIndex += vertexSize; // jump by 1 vertex stride
                        texPtr[texIndex] = 0.0f;
                        texPtr[texIndex + 1] = upperY;

                        texIndex += vertexSize;
                        texPtr[texIndex] = upperX;
                        texPtr[texIndex + 1] = 0.0f;

                        texIndex += vertexSize;
                        texPtr[texIndex] = upperX;
                        texPtr[texIndex + 1] = upperY;
                    } // for
                } // unsafev

                // unlock the buffer
                buffer.Unlock();
            } // if material != null
        }

		#endregion
		
		#region Properties
		
        /// <summary>
        /// 
        /// </summary>
        public bool IsTransparent {
            get {
                return isTransparent;
            }
            set {
                isTransparent = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string MaterialName {
            set {
                base.MaterialName = value;
                UpdateTextureGeometry();
            }
            get {
                return base.MaterialName;
            }
        }

		#endregion

        #region Script parser methods

        [AttributeParser("tiling", "Panel")]
        public static void ParseTiling(string[] parms, params object[] objects) {
            Panel panel = (Panel)objects[0];

            panel.SetTiling(StringConverter.ParseFloat(parms[0]), StringConverter.ParseFloat(parms[1]), int.Parse(parms[2]));
        }

        [AttributeParser("transparent", "Panel")]
        public static void ParseTransparent(string[] parms, params object[] objects) {
            Panel panel = (Panel)objects[0];

            panel.IsTransparent = bool.Parse(parms[0]);
        }

        #endregion Script parser methods

	}
}
