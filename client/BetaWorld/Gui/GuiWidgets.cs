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
using System.Diagnostics;

using Axiom.MathLib;
using Axiom.Core;
using Axiom.Input;

using Multiverse.Gui;

/// This file is for the gui widgets that are in the 3d world.
/// Some examples are the bubble text and the names of characters.
namespace Multiverse.BetaWorld.Gui
{

#if NOT_ME
	public class ZOrderedStatic : Window
	{
		protected float zValue;

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="name">Name of this widget.</param>
		public ZOrderedStatic(string name)
			: base(name)
		{
		}

		protected override void DrawSelf(float z)
		{
			base.DrawSelf(zValue);
		}

		public float ZValue
		{
			get
			{
				return zValue;
			}
			set
			{
				zValue = value;
			}
		}
	}

	public class ZOrderedStaticText : ZOrderedStatic
	{
		HorizontalTextFormat horzFormatting;
		ColorRect textColors = new ColorRect();

        protected Font font;
        protected string text;

		public ZOrderedStaticText(string name)
			: base(name)
		{
		}

        protected void UpdateText() {
        }

        public string Text {
            get {
                return text;
            }
            set {
                text = value;
                UpdateText();
            }
        }

		/// <summary>
		///		Perform the actual rendering for this Window.
		/// </summary>
		/// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
		protected override void DrawSelf(float z)
		{
			// Don't do anything if we don't have any text
			if (this.Text == string.Empty)
				return;
			// render what base class needs to render first
			base.DrawSelf(z);

			// render text
			Font textFont = this.Font;

			Size max = this.MaximumSize;
            Rect maxRect = new Rect(0, max.width, 0, max.height);

			string message = this.Text;
			// get total pixel height of the text based on its format
			float textHeight =
				textFont.GetFormattedLineCount(message, maxRect, horzFormatting) * textFont.LineSpacing;
			float textWidth =
				textFont.GetFormattedTextExtent(message, maxRect, horzFormatting);
            float height = Math.Min(textHeight, max.height);
            float width = textWidth;

			Rect absRect = this.UnclippedPixelRect;
			int newTop = (int)(absRect.Bottom - height);
            int newLeft = (int)(absRect.Left + (absRect.Width - width) / 2);
            int newBottom = newTop + (int)height;
            int newRight = newLeft + (int)width;
            Rect newAbsRect = new Rect(newLeft, newRight, newTop, newBottom);
			SetAreaRect(newAbsRect);

			Rect clipper = newAbsRect.GetIntersection(this.PixelRect);

			textColors.SetAlpha(EffectiveAlpha);

			// The z value for this will be slightly less, so that the text 
			// is in front of the background
			textFont.DrawText(
				message,
				this.UnclippedInnerRect,
				this.ZValue - Renderer.GuiZLayerStep,
				clipper,
				horzFormatting,
				textColors);
		}

		public void SetTextColor(ColorEx color)
		{
            textColors = new ColorRect(color);
		}

		public HorizontalTextFormat HorizontalFormat
		{
			get
			{
				return horzFormatting;
			}
			set
			{
				horzFormatting = value;
			}
		}

        public Font Font {
            get { return font; }
        }
	}
#endif
    /// <summary>
	/// Summary description for WLNameText.
	/// </summary>
	public class WLNameText : Window
	{
        LayeredStaticText textWidget;
 		
        #region Constructor
		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="name">Name of this widget.</param>
		public WLNameText(string name)
			: base(name)
		{
            textWidget = new LayeredStaticText(name, this);
            // textWidget.Visible = true;
            AddChild(textWidget);
		}

		#endregion Constructor

		/// <summary>
		///		Init this widget.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			colors = new ColorRect(ColorEx.Red);
            textWidget.SetTextColor(ColorEx.Cyan);
            textWidget.HorizontalFormat = HorizontalTextFormat.Centered;
		}

        public void SetText(string str) {
            textWidget.SetText(str);
            float height = textWidget.GetTextHeight(false);
            float width = textWidget.GetTextWidth();
            Rect newArea = new Rect(0, width, 0, height);
            textWidget.SetAreaRect(newArea);
            textWidget.SetVScrollPosition(0);
            this.SetAreaRect(newArea);
        }
        public void SetFont(Font font) {
            textWidget.Font = font;
            textWidget.UpdateText();
            float height = textWidget.GetTextHeight(false);
            float width = textWidget.GetTextWidth();
            Rect newArea = new Rect(0, width, 0, height);
            textWidget.SetAreaRect(newArea);
            textWidget.SetVScrollPosition(0);
            this.SetAreaRect(newArea);
        }
	}


    public class FrameWindow : Window
    {
        public FrameWindow()
        {
        }
        public FrameWindow(string name)
            : base(name)
        {
        }

        protected TextureInfo top;
        protected TextureInfo bottom;
        protected TextureInfo left;
        protected TextureInfo right;
        protected TextureInfo topLeft;
        protected TextureInfo topRight;
        protected TextureInfo bottomLeft;
        protected TextureInfo bottomRight;
        protected TextureInfo background;

        protected ColorRect backgroundColors;

        public ColorRect BackgroundColors
        {
            get
            {
                return backgroundColors;
            }
            set
            {
                backgroundColors = value;
            }
        }
    }

	/// <summary>
	///   Variant of the RenderableFrame that also has a bottom center image
	///   (for the bubble tail).
	/// </summary>
	public class ComplexFrameWindow : FrameWindow {
		protected TextureInfo bottomCenter;

        public ComplexFrameWindow(string name)
            : base(name) {
        }

        protected override void DrawSelf(float z) {
            float maxOffset = (int)FrameStrata.Maximum * LayeredStaticText.GuiZFrameStrataStep;
            Point pos = this.DerivedPosition;
            DrawSelf(new Vector3(pos.x, pos.y, z + maxOffset), null);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="clipRect"></param>
		protected override void DrawSelf(Vector3 position, Rect clipRect) {
			Vector3 finalPos = position;
			float origWidth = area.Width;
			float origHeight = area.Height;
			Size finalSize = new Size();
            // Pass our alpha settings to the textures we draw
            colors.SetAlpha(this.EffectiveAlpha);

			// calculate 'adjustments' required to accommodate corner pieces.
			float coordAdj = 0, sizeAdj = 0;

			// draw top-edge, if required
			if (top != null) {
				// calculate adjustments required if top-left corner will be rendered.
				if (topLeft != null) {
					sizeAdj	= topLeft.Width;
					coordAdj = topLeft.Width;
				}
				else {
					coordAdj = 0;
					sizeAdj	= 0;
				}

				// calculate adjustments required if top-right corner will be rendered.
				if (topRight != null) {
					sizeAdj += topRight.Width;
				}

				finalSize.width		= origWidth - sizeAdj;
                finalSize.height    = top.Height;
                finalPos.x          = position.x + coordAdj;

				top.Draw(finalPos, finalSize, clipRect, colors);
			}

			// draw bottom-edge, if required
			if (bottom != null) {
				// calculate adjustments required if bottom-left corner will be rendered.
				if (bottomLeft != null) {
					sizeAdj = bottomLeft.Width;
					coordAdj = bottomLeft.Width;
				}
				else {
					coordAdj = 0;
					sizeAdj	= 0;
				}

				// calculate adjustments required if bottom-right corner will be rendered.
				if (bottomRight != null) {
					sizeAdj += bottomRight.Width;
				}

				if (bottomCenter != null) {
					float leftPortion = (float)Math.Floor((origWidth - sizeAdj) / 2);
					float rightPortion = origWidth - sizeAdj - bottomCenter.Width - leftPortion;

					// Draw the left portion of the bottom edge
					finalSize.width = leftPortion;
					finalSize.height = bottom.Height;
					finalPos.x = position.x + coordAdj;
					finalPos.y = position.y + origHeight - finalSize.height;

					bottom.Draw(finalPos, finalSize, clipRect, colors);

					finalSize.width = bottomCenter.Width;
					finalSize.height = bottomCenter.Height;
					finalPos.x = position.x + coordAdj + leftPortion;
					finalPos.y = position.y + origHeight - finalSize.height;

					bottomCenter.Draw(finalPos, finalSize, clipRect, colors);

					finalSize.width = rightPortion;
					finalSize.height = bottom.Height;
					finalPos.x = position.x + coordAdj + leftPortion + bottomCenter.Width;
					finalPos.y = position.y + origHeight - finalSize.height;

					bottom.Draw(finalPos, finalSize, clipRect, colors);
				} else {
					finalSize.width = origWidth - sizeAdj;
					finalSize.height = bottom.Height;
					finalPos.x = position.x + coordAdj;
					finalPos.y = position.y + origHeight - finalSize.height;

					bottom.Draw(finalPos, finalSize, clipRect, colors);
				}
			}

			// reset x co-ordinate to input value
			finalPos.x = position.x;

			// draw left-edge, if required
			if (left != null) {
				// calculate adjustments required if top-left corner will be rendered.
				if (topLeft != null) {
					sizeAdj = topLeft.Height;
					coordAdj = topLeft.Height;
				}
				else {
					coordAdj = 0;
					sizeAdj	= 0;
				}

				// calculate adjustments required if bottom-left corner will be rendered.
				if (bottomLeft != null) {
					sizeAdj += bottomLeft.Height;
				}

				finalSize.height	= origHeight - sizeAdj;
				finalSize.width		= left.Width;
				finalPos.y			= position.y + coordAdj;

				left.Draw(finalPos, finalSize, clipRect, colors);
			}

			// draw right-edge, if required
			if (right != null) {
				// calculate adjustments required if top-left corner will be rendered.
				if (topRight != null) {
					sizeAdj = topRight.Height;
					coordAdj = topRight.Height;
				}
				else {
					coordAdj = 0;
					sizeAdj	= 0;
				}

				// calculate adjustments required if bottom-right corner will be rendered.
				if (bottomRight != null) {
					sizeAdj += bottomRight.Height;
				}

				finalSize.height	= origHeight - sizeAdj;
				finalSize.width		= left.Width;
				finalPos.y			= position.y + coordAdj;
				finalPos.x			= position.x + origWidth - finalSize.width;

				right.Draw(finalPos, finalSize, clipRect, colors);
			}

			// draw required corner pieces...
			if (topLeft != null) {
				topLeft.Draw(position, clipRect, colors);
			}

			if (topRight != null) {
				finalPos.x = position.x + origWidth - topRight.Width;
				finalPos.y = position.y;
				topRight.Draw(finalPos, clipRect, colors);
			}

			if (bottomLeft != null) {
				finalPos.x = position.x;
				finalPos.y = position.y + origHeight - bottomLeft.Height;
				bottomLeft.Draw(finalPos, clipRect, colors);
			}

			if (bottomRight != null) {
				finalPos.x = position.x + origWidth - bottomRight.Width;
				finalPos.y = position.y + origHeight - bottomRight.Height;
				bottomRight.Draw(finalPos, clipRect, colors);
			}

            if (background != null) {
                float sizeAdjX = 0;
                float coordAdjX = 0;
                float sizeAdjY = 0;
                float coordAdjY = 0;
                if (top != null) {
                    sizeAdjY += top.Height;
                    coordAdjY += top.Height;
                }
                if (bottom != null) {
                    sizeAdjY += bottom.Height;
                }
                if (left != null) {
                    sizeAdjX += left.Width;
                    coordAdjX += left.Width;
                }
                if (right != null) {
                    sizeAdjX += right.Width;
                }
                finalSize.height = origHeight - sizeAdjY;
                finalSize.width = origWidth - sizeAdjX;
                finalPos.y = position.y + coordAdjY;
                finalPos.x = position.x + coordAdjX;

                background.Draw(finalPos, finalSize, clipRect, colors);
            }
		}

		public TextureInfo BottomCenter {
			get {
				return bottomCenter;
			}
			set {
				bottomCenter = value;
			}
		}

	}


	/// <summary>
	/// Summary description for WLBubbleText.
	/// </summary>
	public class WLBubbleText : ComplexFrameWindow
	{
		protected const string ImagesetName = "WindowsLook";

		protected const string BackgroundImageName = "Background";

		protected const string TopLeftBubbleImageName = "TopLeftBubble";
		protected const string TopRightBubbleImageName = "TopRightBubble";
		protected const string BottomLeftBubbleImageName = "BottomLeftBubble";
		protected const string BottomRightBubbleImageName = "BottomRightBubble";

		protected const string LeftBubbleImageName = "LeftBubble";
		protected const string RightBubbleImageName = "RightBubble";
		protected const string TopBubbleImageName = "TopBubble";
		protected const string BottomBubbleImageName = "BottomBubble";
		
		protected const string TailBubbleImageName = "TailBubble";

        LayeredStaticText textWidget;

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="name">Name of this widget.</param>
		public WLBubbleText(string name)
			: base(name)
		{
            textWidget = new LayeredStaticText(name, this);
            textWidget.Visible = true;
            AddChild(textWidget);
		}

		#endregion Constructor

		/// <summary>
		///		Init this widget.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

            colors = new ColorRect(ColorEx.WhiteSmoke);
            
            TextureAtlas atlas = AtlasManager.Instance.GetTextureAtlas(ImagesetName);

            topLeft = atlas.GetTextureInfo(TopLeftBubbleImageName);
            topRight = atlas.GetTextureInfo(TopRightBubbleImageName);
            bottomLeft = atlas.GetTextureInfo(BottomLeftBubbleImageName);
            bottomRight = atlas.GetTextureInfo(BottomRightBubbleImageName);

            left = atlas.GetTextureInfo(LeftBubbleImageName);
            right = atlas.GetTextureInfo(RightBubbleImageName);
            top = atlas.GetTextureInfo(TopBubbleImageName);
            bottom = atlas.GetTextureInfo(BottomBubbleImageName);

            background = atlas.GetTextureInfo(BackgroundImageName);
            bottomCenter = atlas.GetTextureInfo(TailBubbleImageName);

			textWidget.HorizontalFormat = HorizontalTextFormat.WordWrapCentered;
            textWidget.VerticalFormat = VerticalTextFormat.Bottom;
            textWidget.NormalTextStyle = new TextStyle();
            textWidget.NormalTextStyle.textColor = ColorEx.Black;
		}

        public void SetText(string str) {
            // Maximum dimensions
            Rect newArea = new Rect(0, 200, 0, 200);
            // Initial size to see how we wrap
            textWidget.SetAreaRect(newArea);
            textWidget.SetText(str);
            float textHeight = textWidget.GetTextHeight(false);
            float textWidth = textWidget.GetTextWidth();
            // smallest size that will still fit all the text
            textWidget.SetAreaRect(new Rect(left.Width, left.Width + textWidth, top.Height, top.Height + textHeight));
            newArea.Height = textHeight + top.Height + Math.Max(bottom.Height, bottomCenter.Height);
            newArea.Width = textWidth + left.Width + right.Width;
            // textWidget.SetVScrollPosition(0);
            this.SetAreaRect(newArea);
        }
        public void SetFont(Font font) {
            textWidget.Font = font;
            // Maximum dimensions
            Rect newArea = new Rect(0, 200, 0, 200);
            textWidget.SetAreaRect(newArea);
            textWidget.UpdateText();
            // textWidget.SetVScrollPosition(0);
            float textHeight = textWidget.GetTextHeight(false);
            float textWidth = textWidget.GetTextWidth();
            // smallest size that will still fit all the text
            textWidget.SetAreaRect(new Rect(left.Width, left.Width + textWidth, top.Height, top.Height + textHeight));
            newArea.Height = textHeight + top.Height + Math.Max(bottom.Height, bottomCenter.Height);
            newArea.Width = textWidth + left.Width + right.Width;
            this.SetAreaRect(newArea);
        }
    }
}
