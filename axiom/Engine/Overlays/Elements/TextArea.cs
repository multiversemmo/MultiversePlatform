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
using Axiom.Core;
using Axiom.Fonts;
using Axiom.Scripting;
using Axiom.Graphics;
using Font = Axiom.Fonts.Font;

namespace Axiom.Overlays.Elements {
	/// <summary>
	/// 	Label type control that can be used to display text using a specified font.
	/// </summary>
	public class TextArea : OverlayElement {
		#region Member variables
		
        protected Font font;
        protected HorizontalAlignment alignment;
        protected float charHeight;
        protected int pixelCharHeight;
        protected float spaceWidth;
        protected int pixelSpaceWidth;
        protected int allocSize;
        protected bool isTransparent;
        protected RenderOperation renderOp = new RenderOperation();

        /// Colors to use for the vertices
        protected ColorEx colorBottom;
        protected ColorEx colorTop;
        protected bool haveColorsChanged;

        const int DEFAULT_INITIAL_CHARS = 12;
        const int POSITION_TEXCOORD = 0;
        const int COLORS = 1;

		#endregion
		
		#region Constructors
		
        /// <summary>
        ///    Basic constructor, internal since it should only be created by factories.
        /// </summary>
        /// <param name="name"></param>
		internal TextArea(string name) : base(name) {
            isTransparent = false;
            alignment = HorizontalAlignment.Left;

            colorTop = ColorEx.White;
            colorBottom = ColorEx.White;
            haveColorsChanged = true;

            charHeight = 0.02f;
            pixelCharHeight = 12;
		}
		
		#endregion
		
		#region Methods

        /// <summary>
        ///    
        /// </summary>
        /// <param name="size"></param>
        protected void CheckMemoryAllocation(int numChars) {
            if(allocSize < numChars) {
                // Create and bind new buffers
                // Note that old buffers will be deleted automatically through reference counting
            
                // 6 verts per char since we're doing tri lists without indexes
                // Allocate space for positions & texture coords
                VertexDeclaration decl = renderOp.vertexData.vertexDeclaration;
                VertexBufferBinding binding = renderOp.vertexData.vertexBufferBinding;

                renderOp.vertexData.vertexCount = numChars * 6;

                // Create dynamic since text tends to change alot
                // positions & texcoords
                HardwareVertexBuffer buffer = 
                    HardwareBufferManager.Instance.CreateVertexBuffer(
                        decl.GetVertexSize(POSITION_TEXCOORD),
                        renderOp.vertexData.vertexCount,
                        BufferUsage.Dynamic);

                // bind the pos/tex buffer
                binding.SetBinding(POSITION_TEXCOORD, buffer);

                // colors
                buffer = 
                    HardwareBufferManager.Instance.CreateVertexBuffer(
                    decl.GetVertexSize(COLORS),
                    renderOp.vertexData.vertexCount,
                    BufferUsage.Dynamic);

                // bind the color buffer
                binding.SetBinding(COLORS, buffer);

                allocSize = numChars;
                // force color buffer regeneration
                haveColorsChanged = true; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        public override void GetRenderOperation(RenderOperation op) {
            op.vertexData = renderOp.vertexData;
            op.useIndices = false;
            op.operationType = renderOp.operationType;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize() {
            // Set up the render operation
            // Combine positions and texture coords since they tend to change together
            // since character sizes are different
           renderOp.vertexData = new VertexData();
            VertexDeclaration decl = renderOp.vertexData.vertexDeclaration;

            int offset = 0;

            // positions
            decl.AddElement(POSITION_TEXCOORD, offset, VertexElementType.Float3, VertexElementSemantic.Position);
            offset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // texcoords
            decl.AddElement(POSITION_TEXCOORD, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            
            // colors, stored in seperate buffer since they change less often
            decl.AddElement(COLORS, 0, VertexElementType.Color, VertexElementSemantic.Diffuse);

            renderOp.operationType = OperationType.TriangleList;
            renderOp.useIndices = false;
            renderOp.vertexData.vertexStart = 0;

            // buffers are created in CheckMemoryAllocation
            CheckMemoryAllocation(DEFAULT_INITIAL_CHARS);
        }

        /// <summary>
        ///    Override to update char sizing based on current viewport settings.
        /// </summary>
        public override void Update() {
            if(metricsMode == MetricsMode.Pixels &&
                (OverlayManager.Instance.HasViewportChanged || geomPositionsOutOfDate)) {
            
                float vpHeight = OverlayManager.Instance.ViewportHeight;

                charHeight = (float)pixelCharHeight  / vpHeight;
                spaceWidth = (float)pixelSpaceWidth / vpHeight;

                geomPositionsOutOfDate = true;
            }

            base.Update ();
        }

        /// <summary>
        /// 
        /// </summary>
        protected unsafe void UpdateColors() {
            if(!haveColorsChanged) {
                // nothing to see here folks
                return;
            }

            // convert to API specific color values
            uint topColor = Root.Instance.ConvertColor(colorTop);
            uint bottomColor = Root.Instance.ConvertColor(colorBottom);

            // get the seperate color buffer
            HardwareVertexBuffer buffer =
                renderOp.vertexData.vertexBufferBinding.GetBuffer(COLORS);

            IntPtr data = buffer.Lock(BufferLocking.Discard);
            uint* colPtr = (uint*)data.ToPointer();
            int index = 0;

            for(int i = 0; i < allocSize; i++) {
                // first tri (top, bottom, top);
                colPtr[index++] = topColor;
                colPtr[index++] = bottomColor;
                colPtr[index++] = topColor;

                // second tri (top, bottom, bottom);
                colPtr[index++] = topColor;
                colPtr[index++] = bottomColor;
                colPtr[index++] = bottomColor;
            }

            // unlock this bad boy
            buffer.Unlock();

            haveColorsChanged = false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected unsafe void UpdateGeometry() {
            if(font == null || text == null || !geomPositionsOutOfDate) {
                // must not be initialized yet, probably due to order of creation in a template
                return;
            }

            int charLength = text.Length;
            // make sure the buffers are big enough
            CheckMemoryAllocation(charLength);

            renderOp.vertexData.vertexCount = charLength * 6;

            float largestWidth = 0.0f;
            float left = this.DerivedLeft * 2.0f - 1.0f;
            float top = -((this.DerivedTop * 2.0f) - 1.0f);

            // derive space width from the size of a capital A
            if(spaceWidth == 0) {
                spaceWidth = font.GetGlyphAspectRatio('A') * charHeight * 2.0f;
            }

            // get pos/tex buffer
            HardwareVertexBuffer buffer = renderOp.vertexData.vertexBufferBinding.GetBuffer(POSITION_TEXCOORD);
            IntPtr data = buffer.Lock(BufferLocking.Discard);
            float* vertPtr = (float*)data.ToPointer();
            int index = 0;

            bool newLine = true;

            // go through each character and process
            for(int i = 0; i < text.Length; i++) {
                char c = text[i];

                if(newLine) {
                    float length = 0.0f;

                    // precalc the length of this line
                    for(int j = i; j < text.Length && text[j] != '\n'; j++) {
                        if(text[j] == ' ') {
                            length += spaceWidth;
                        }
                        else {
                            length += font.GetGlyphAspectRatio(text[j]) * charHeight * 2;
                        }
                    } // for j

                    if(alignment == HorizontalAlignment.Right) {
                        left -= length;
                    }
                    else if(alignment == HorizontalAlignment.Center) {
                        left -= length * 0.5f;
                    }

                    newLine = false;
                } // if newLine

                if(c == '\n') {
                    left = this.DerivedLeft * 2.0f - 1.0f;
                    top -= charHeight * 2.0f;
                    newLine = true;
					// reduce tri count
					renderOp.vertexData.vertexCount -= 6;
                    continue;
                }

                if(c == ' ') {
                    // leave a gap, no tris required
                    left += spaceWidth;
                    // reduce tri count
                    renderOp.vertexData.vertexCount -= 6;
                    continue;
                }

                float horizHeight = font.GetGlyphAspectRatio(c);
                float u1, u2, v1, v2;

                // get the texcoords for the specified character
                font.GetGlyphTexCoords(c, out u1, out v1, out u2, out v2);

                // each vert is (x, y, z, u, v)
                // first tri
                // upper left
                vertPtr[index++] = left;
                vertPtr[index++] = top;
                vertPtr[index++] = -1.0f;
                vertPtr[index++] = u1;
                vertPtr[index++] = v1;

                top -= charHeight * 2.0f;

                // bottom left
                vertPtr[index++] = left;
                vertPtr[index++] = top;
                vertPtr[index++] = -1.0f;
                vertPtr[index++] = u1;
                vertPtr[index++] = v2;

                top += charHeight * 2.0f;
                left += horizHeight * charHeight * 2.0f;

                // top right
                vertPtr[index++] = left;
                vertPtr[index++] = top;
                vertPtr[index++] = -1.0f;
                vertPtr[index++] = u2;
                vertPtr[index++] = v1;

                // second tri

                // top right (again)
                vertPtr[index++] = left;
                vertPtr[index++] = top;
                vertPtr[index++] = -1.0f;
                vertPtr[index++] = u2;
                vertPtr[index++] = v1;

                top -= charHeight * 2.0f;
                left -= horizHeight * charHeight * 2.0f;

                // bottom left (again)
                vertPtr[index++] = left;
                vertPtr[index++] = top;
                vertPtr[index++] = -1.0f;
                vertPtr[index++] = u1;
                vertPtr[index++] = v2;

                left += horizHeight * charHeight * 2.0f;

                // bottom right
                vertPtr[index++] = left;
                vertPtr[index++] = top;
                vertPtr[index++] = -1.0f;
                vertPtr[index++] = u2;
                vertPtr[index++] = v2;

                // go back up with top
                top += charHeight * 2.0f;

                float currentWidth = (left + 1) / 2 - this.DerivedLeft;
                
                if(currentWidth > largestWidth) {
                    largestWidth = currentWidth;
                }
            } // for i

            // unlock vertex buffer
            buffer.Unlock();

            if(metricsMode == MetricsMode.Pixels) {
                // Derive parametric version of dimensions
                float vpWidth = OverlayManager.Instance.ViewportWidth;

                largestWidth *= vpWidth;
            }

            // record the width as the longest width calculated for any of the lines
            if(this.Width < largestWidth) {
                this.Width = largestWidth;
            }

            // update colors
            UpdateColors();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UpdatePositionGeometry() {
            UpdateGeometry();
        }
		
		#endregion
		
		#region Properties
		    
        /// <summary>
        /// 
        /// </summary>
        public float CharHeight {
            get {
                return charHeight;
            }
            set {
                if(metricsMode == MetricsMode.Pixels) {
                    pixelCharHeight = (int)value;
                }
                else {
                    charHeight = value;
                }
                UpdateGeometry();
            }
        }

        /// <summary>
        ///    Gets/Sets the color value of the text when it is all the same color.
        /// </summary>
        public override ColorEx Color {
            get {
                // doesnt matter if they are both the same
                return colorTop;
            }
            set {
                colorTop = value;
                colorBottom = value;
                haveColorsChanged = true;
                UpdateColors();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ColorEx ColorTop {
            get {
                return colorTop;
            }
            set {
                colorTop = value;
                haveColorsChanged = true;
                UpdateColors();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ColorEx ColorBottom {
            get {
                return colorBottom;
            }
            set {
                colorBottom = value;
                haveColorsChanged = true;
                UpdateColors();
            }
        }

        /// <summary>
        ///    Gets/Sets the name of the font currently being used when rendering the text.
        /// </summary>
        public string FontName {
            get {
                return font.Name;
            }
            set {
                font = (Font)FontManager.Instance.GetByName(value);
                font.Load();

                // note: font materials are created with lighting and depthcheck disabled by default
                material = font.Material;

                // TODO: See if this can be eliminated
                material.Lighting = false;
                material.DepthCheck = false;

                geomPositionsOutOfDate = true;
            }
        }

        /// <summary>
        ///    Override to update geometry when new material is assigned.
        /// </summary>
        public override string MaterialName {
            get {
                return base.MaterialName;
            }
            set {
                base.MaterialName = value;
                UpdateGeometry();
            }
        }

        /// <summary>
        ///    Override to handle character spacing
        /// </summary>
        public override MetricsMode MetricsMode {
            get {
                return base.MetricsMode;
            }
            set {
                base.MetricsMode = value;

                // configure pixel variables based on current viewport
                if(metricsMode == MetricsMode.Pixels) {
                    float vpHeight = OverlayManager.Instance.ViewportHeight;

                    pixelCharHeight = (int)(charHeight * vpHeight);
                    pixelSpaceWidth = (int)(spaceWidth * vpHeight);
                }
            }
        }

        /// <summary>
        ///    
        /// </summary>
        public float SpaceWidth {
            get {
                return spaceWidth;
            }
            set {
                if(metricsMode == MetricsMode.Pixels) {
                    pixelSpaceWidth = (int)value;
                }
                else {
                    spaceWidth = value;
                }

                geomPositionsOutOfDate = true;
            }
        }
            
        /// <summary>
        ///    Alignment of text specifically.
        /// </summary>
        public HorizontalAlignment TextAlign {
            get {
                return alignment;
            }
            set {
                alignment = value;
                UpdateGeometry();
            }
        }

        /// <summary>
        ///    Override to update string geometry.
        /// </summary>
        public override string Text {
            get {
                return base.Text;
            }
            set {
                base.Text = value;
                UpdateGeometry();
            }
        }


        /// <summary>
        ///    Overridden to return this elements type.
        /// </summary>
        public override string Type {
            get {
                return "TextArea";
            }
        }

		#endregion

        #region Script parser methods

        [AttributeParser("char_height", "TextArea")]
        public static void  ParseCharHeight(string[] parms, params object[] objects) {
            TextArea textArea = (TextArea)objects[0];

            textArea.CharHeight = StringConverter.ParseFloat(parms[0]);
        }

        [AttributeParser("space_width", "TextArea")]
        public static void  ParseSpaceWidth(string[] parms, params object[] objects) {
            TextArea textArea = (TextArea)objects[0];

            textArea.SpaceWidth = StringConverter.ParseFloat(parms[0]);
        }

        [AttributeParser("font_name", "TextArea")]
        public static void  ParseFontName(string[] parms, params object[] objects) {
            TextArea textArea = (TextArea)objects[0];

            textArea.FontName = parms[0];
        }

        [AttributeParser("color", "TextArea")]
        [AttributeParser("colour", "TextArea")]
        public static void  ParseColor(string[] parms, params object[] objects) {
            TextArea textArea = (TextArea)objects[0];

            textArea.Color = StringConverter.ParseColor(parms);
        }

        [AttributeParser("color_top", "TextArea")]
        [AttributeParser("colour_top", "TextArea")]
        public static void  ParseColorTop(string[] parms, params object[] objects) {
            TextArea textArea = (TextArea)objects[0];

            textArea.ColorTop = StringConverter.ParseColor(parms);
        }

        [AttributeParser("color_bottom", "TextArea")]
        [AttributeParser("colour_bottom", "TextArea")]
        public static void  ParseColorBottom(string[] parms, params object[] objects) {
            TextArea textArea = (TextArea)objects[0];

            textArea.ColorBottom = StringConverter.ParseColor(parms);
        }

        #endregion
	}
}
