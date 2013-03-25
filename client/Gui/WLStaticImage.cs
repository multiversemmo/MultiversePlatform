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

using Axiom.Core;
using Axiom.MathLib;

namespace Multiverse.Gui {

	/// <summary>
	/// Summary description for WLStaticText.
	/// </summary>
	public class WLStaticImage : FrameWindow {

		protected const string ImagesetName = "WindowsLook";

		protected const string TopLeftFrameImageName = "StaticFrameTopLeft";
		protected const string TopRightFrameImageName = "StaticFrameTopRight";
		protected const string BottomLeftFrameImageName = "StaticFrameBottomLeft";
		protected const string BottomRightFrameImageName = "StaticFrameBottomRight";

		protected const string LeftFrameImageName = "StaticFrameLeft";
		protected const string RightFrameImageName = "StaticFrameRight";
		protected const string TopFrameImageName = "StaticFrameTop";
		protected const string BottomFrameImageName = "StaticFrameBottom";

		protected const string BackgroundImageName = "Background";

		#region Fields

		#endregion

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="name">Name of this widget.</param>
		public WLStaticImage(string name) : base(name) {
		}

		/// <summary>
		///		Init this widget.
		/// </summary>
		public override void Initialize() {
			base.Initialize ();

			TextureAtlas imageset = AtlasManager.Instance.GetTextureAtlas(ImagesetName);

            topLeft = imageset.GetTextureInfo(TopLeftFrameImageName);
			topRight = imageset.GetTextureInfo(TopRightFrameImageName);
			bottomLeft = imageset.GetTextureInfo(BottomLeftFrameImageName);
			bottomRight = imageset.GetTextureInfo(BottomRightFrameImageName);
			left = imageset.GetTextureInfo(LeftFrameImageName);
			right = imageset.GetTextureInfo(RightFrameImageName);
            top = imageset.GetTextureInfo(TopFrameImageName);
            bottom = imageset.GetTextureInfo(BottomFrameImageName);

			background = imageset.GetTextureInfo(BackgroundImageName);
            colors = new ColorRect(ColorEx.White);

			// StoreFrameSizes();
		}


		#endregion Constructor

		/// <summary>
		///		overridden so derived classes are auto-clipped to within the 
		///		inner area of the frame when it's active.
		/// </summary>
		public override Rect UnclippedInnerRect {
			get {
				Rect temp = base.UnclippedInnerRect;
                return new Rect(temp.Left + left.Width, temp.Right - right.Width, 
                                temp.Top + top.Height, temp.Bottom - bottom.Height);
			}
		}

		protected override void DrawSelf(float z) {
			// do base class rendering first
			base.DrawSelf(z);

			// render the image
            Point pt = this.DerivedPosition;
            // FIXME: This z offset should use something like the FrameStrata system
            background.Draw(new Vector3(pt.x, pt.y, z - .0001f), 
                            this.PixelRect);
		}
	}
}
