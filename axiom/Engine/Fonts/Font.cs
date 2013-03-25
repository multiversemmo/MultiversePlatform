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
using System.Text;
using Axiom.Core;

using Axiom.Graphics;

namespace Axiom.Fonts {
	/// <summary>
	///		This class is simply a way of getting a font texture into the engine and
	///		to easily retrieve the texture coordinates required to accurately render them.
	///		Fonts can either be loaded from precreated textures, or the texture can be generated
	///		using a truetype font. You can either create the texture manually in code, or you
	///		can use an XML font script to define it (probably more practical since you can reuse
	///		the definition more easily)
	/// </summary>
	public class Font : Axiom.Core.Resource {
		#region Constants

		const int BITMAP_HEIGHT = 512;
		const int BITMAP_WIDTH = 512;
		const int START_CHAR = 33;
		const int END_CHAR = 127;

		#endregion

		#region Member variables

		/// <summary>
		///    Type of font, either imag based or TrueType.
		/// </summary>
		protected FontType fontType;
		/// <summary>
		///    Source of the font (either an image name or a TrueType font).
		/// </summary>
		protected string source;
		/// <summary>
		///    Size of the truetype font, in points.
		/// </summary>
		protected int ttfSize;
		/// <summary>
		///    Resolution (dpi) of truetype font.
		/// </summary>
		protected int ttfResolution;
		/// <summary>
		///    For TrueType fonts only.
		/// </summary>
		protected bool antialiasColor;
		/// <summary>
		///    Material create for use on entities by this font.
		/// </summary>
		protected Material material;

		// arrays for storing texture and display data for each character
		protected float[] texCoordU1 = new float[END_CHAR - START_CHAR];
		protected float[] texCoordU2 = new float[END_CHAR - START_CHAR];
		protected float[] texCoordV1 = new float[END_CHAR - START_CHAR];
		protected float[] texCoordV2 = new float[END_CHAR - START_CHAR];
		protected float[] aspectRatio = new float[END_CHAR - START_CHAR];

		protected bool showLines = false;

		#endregion

		#region Constructor

		/// <summary>
		///		Constructor, should be called through FontManager.Create.
		/// </summary>
		public Font(string name) {
			this.name = name;
		}

		#endregion Constructor

		#region Implementation of Resource

        public override void Preload() {
            throw new NotImplementedException();
        }

		/// <summary>
		///    Loads either an image based font, or creates one on the fly from a TrueType font file.
		/// </summary>
        protected override void LoadImpl() {
			// create a material for this font
			material = (Material)MaterialManager.Instance.Create("Fonts/" + name);

			TextureUnitState unitState = null;
			bool blendByAlpha = false;

			if (fontType == FontType.TrueType) {
				// create the font bitmap on the fly
				CreateTexture();
				// a texture layer was added in CreateTexture
				unitState = material.GetTechnique(0).GetPass(0).GetTextureUnitState(0);
				blendByAlpha = true;
			}
			else {
				// pre-created font images
				unitState = material.GetTechnique(0).GetPass(0).CreateTextureUnitState(source);
				// load this texture
				// TODO: In general, modify any methods like this that throw their own exception rather than returning null, so the caller can decide how to handle a missing resource.
				Texture texture = TextureManager.Instance.Load(source);
				blendByAlpha = texture.HasAlpha;
			}

			// set texture addressing mode to Clamp to eliminate fuzzy edges
			unitState.TextureAddressing = TextureAddressing.Clamp;

			// set up blending mode
			if(blendByAlpha) {
				material.SetSceneBlending(SceneBlendType.TransparentAlpha);
			}
			else {
				// assume black background here
				material.SetSceneBlending(SceneBlendType.Add);
			}
		}

        protected override void UnloadImpl() {
            // texture.Unload();
        }

		#endregion Implementation of Resource

		#region Methods

		protected void CreateTexture() {
			// TODO: Revisit after checking current Imaging support in Mono.
			//            // create a new bitamp with the size defined
			//            Bitmap bitmap = new Bitmap(BITMAP_WIDTH, BITMAP_HEIGHT, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			//
			//            // get a handles to the graphics context of the bitmap
			//            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
			//
			//            // get a font object for the specified font
			//            System.Drawing.Font font = new System.Drawing.Font(name, 18);
			//			
			//            // create a pen for the grid lines
			//            Pen linePen = new Pen(Color.Red);
			//
			//            // clear the image to transparent
			//            g.Clear(Color.Transparent);
			//
			//            // nice smooth text
			//            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			//            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			//
			//            // used for calculating position in the image for rendering the characters
			//            int x, y, maxHeight;
			//            x = y = maxHeight = 0;
			//
			//            // loop through each character in the glyph string and draw it to the bitmap
			//            for(int i = START_CHAR; i < END_CHAR; i++) {
			//                char c = (char)i;
			//
			//                // are we gonna wrap?
			//                if(x + font.Size > BITMAP_WIDTH - 5) {
			//                    // increment the y coord and reset x to move to the beginning of next line
			//                    y += maxHeight;
			//                    x = 0;
			//                    maxHeight = 0;
			//
			//                    if(showLines) {
			//                        // draw a horizontal line underneath this row
			//                        g.DrawLine(linePen, 0, y, BITMAP_WIDTH, y);
			//                    }
			//                }
			//
			//                // draw the character
			//                g.DrawString(c.ToString(), font, Brushes.White, x - 3, y);
			//
			//                // measure the width and height of the character
			//                SizeF metrics = g.MeasureString(c.ToString(), font);
			//
			//                // calculate the texture coords for the character
			//                // note: flip the y coords by subtracting from 1
			//                float u1 = (float)x / (float)BITMAP_WIDTH;
			//                float u2 = ((float)x  + metrics.Width - 4) / (float)BITMAP_WIDTH;
			//                float v1 = 1 - ((float)y / (float)BITMAP_HEIGHT);
			//                float v2 = 1 - (((float)y + metrics.Height) / (float)BITMAP_HEIGHT);
			//                SetCharTexCoords(c, u1, u2, v1, v2);
			//
			//                // increment X by the width of the current char
			//                x += (int)metrics.Width - 3;
			//
			//                // keep track of the tallest character on this line
			//                if(maxHeight < (int)metrics.Height)
			//                    maxHeight = (int)metrics.Height;
			//
			//                if(showLines) {
			//                    // draw a vertical line after this char
			//                    g.DrawLine(linePen, x, y, x, y + font.Height);
			//                }
			//            }  // for
			//
			//            if(showLines) {
			//                // draw the last horizontal line
			//                g.DrawLine(linePen, 0, y + font.Height, BITMAP_WIDTH, y + font.Height);
			//            }
			//
			//            string textureName = name + "FontTexture";
			//
			//            // load the created image using the texture manager
			//            //TextureManager.Instance.LoadImage(textureName, bitmap); 
			//
			//            // add a texture layer with the name of the texture
			//            TextureUnitState unitState = material.GetTechnique(0).GetPass(0).CreateTextureUnitState(textureName);
			//
			//            // use min/mag filter, but no mipmapping
			//            unitState.SetTextureFiltering(FilterOptions.Linear, FilterOptions.Linear, FilterOptions.None);
		}

		/// <summary>
		///		Retreives the texture coordinates for the specifed character in this font.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="u1"></param>
		/// <param name="u2"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		public void GetGlyphTexCoords(char c, out float u1, out float u2, out float v1, out float v2) {
			int idx = (int)c - START_CHAR;
			u1 = texCoordU1[idx];
			u2 = texCoordU2[idx];
			v1 = texCoordV1[idx];
			v2 = texCoordV2[idx];
		}

		/// <summary>
		///		Finds the aspect ratio of the specified character in this font.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public float GetGlyphAspectRatio(char c) {
			int idx = (int)c - START_CHAR;

			return aspectRatio[idx];
		}

		public void SetGlyphTexCoords(char c, float u1, float v1, float u2, float v2) {
			int idx = (int)c - START_CHAR;
			texCoordU1[idx] = u1;
			texCoordU2[idx] = v1;
			texCoordV1[idx] = u2;
			texCoordV2[idx] = v2;
			aspectRatio[idx] = (u2 - u1) / (v2 - v1);
		}

		#endregion Methods

		#region Properties

		/// <summary>
		///    Sets whether or not the color of this font is antialiased as it is generated
		///    from a TrueType font.
		/// </summary>
		/// <remarks>
		///    This is valid only for a TrueType font. If you are planning on using 
		///    alpha blending to draw your font, then it is a good idea to set this to
		///    false (which is the default), otherwise the darkening of the font will combine
		///    with the fading out of the alpha around the edges and make your font look thinner
		///    than it should. However, if you intend to blend your font using a color blending
		///    mode (add or modulate for example) then it's a good idea to set this to true, in
		///    order to soften your font edges.
		/// </remarks>
		public bool AntialiasColor {
			get {
				return antialiasColor;
			}
			set {
				antialiasColor = value;
			}
		}

		/// <summary>
		///    Gets a reference to the material being used for this font.
		/// </summary>
		public Material Material {
			get {
				return material;
			}
		}

		/// <summary>
		///    Source of the font (either an image name or a truetype font)
		/// </summary>
		public string Source {
			get {
				return source;
			}
			set {
				source = value;
			}
		}

		/// <summary>
		///    Resolution (dpi) of truetype font.
		/// </summary>
		public int TrueTypeResolution {
			get {
				return ttfResolution;
			}
			set { 
				ttfResolution = value;
			}
		}

		/// <summary>
		///    Size of the truetype font, in points.
		/// </summary>
		public int TrueTypeSize {
			get {
				return ttfSize;
			}
			set { 
				ttfSize = value;
			}
		}

		/// <summary>
		///    Type of font.
		/// </summary>
		public FontType Type {
			get {
				return fontType;
			}
			set {
				fontType = value;
			}
		}

		#endregion Properties
	}
}
