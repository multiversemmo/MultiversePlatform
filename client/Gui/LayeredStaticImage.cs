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
using System.Text;
using System.Drawing;

using Axiom.Core;
using Axiom.MathLib;

namespace Multiverse.Gui {

    /// <summary>
    ///   This class lets me draw an image at some size at some position.
    ///   The image does not need to be rectangular, but it will be drawn into 
    ///   a rectangular, axis-aligned region.  The image will be stretched to 
    ///   fill the destination rectangle.
    /// </summary>
    public class ImageWindow : Window {
        #region Fields

        protected TextureInfo image;
        protected ColorRect imageColors = new ColorRect();
        HorizontalImageFormat horzFormat = HorizontalImageFormat.Stretched;
        VerticalImageFormat vertFormat = VerticalImageFormat.Stretched;

        #endregion

        #region Constructor
        public ImageWindow(string name)
            : base(name) {
        }
        #endregion


        #region Window methods
        /// <summary>
        ///		Perform the actual rendering for this Window.
        /// </summary>
        protected override void DrawImpl(Rect drawArea, Rect clipArea, float z) {
            if (image == null)
                return;

            // render the image
            Rect clippedArea = null;
            if (clipArea != null)
                clippedArea = clipArea.GetIntersection(PixelRect);
            // update our alpha values
            imageColors.SetAlpha(this.EffectiveAlpha);
            image.Draw(drawArea, z, clippedArea, imageColors);
        }
        #endregion

        public void SetImage(TextureInfo value) {
            image = value;
        }

        public void SetImageColors(ColorRect colors) {
            imageColors = colors;
            // RequestRedraw();
        }

        #region Properties

        public TextureInfo Image {
            get {
                return image;
            }
        }

        public HorizontalImageFormat HorizontalFormat {
            get {
                return horzFormat;
            }
            set {
                if (horzFormat != value) {
                    Dirty = true;
                    horzFormat = value;
                    RequestRedraw();
                }
            }
        }
        public VerticalImageFormat VerticalFormat {
            get {
                return vertFormat;
            }
            set {
                if (vertFormat != value) {
                    Dirty = true;
                    vertFormat = value;
                    RequestRedraw();
                }
            }
        }



        public ColorRect Colors {
            get {
                return colors;
            }
            set {
                if (!ColorRect.CompareTo(colors, value)) {
                    Dirty = true;
                    colors = value;
                    RequestRedraw();
                }
            }
        }

        #endregion Properties
    }

    public class TiledImageWindow : ImageWindow {
        #region Fields
        protected HorizontalImageFormat horizontalFormat = HorizontalImageFormat.Stretched;
        protected VerticalImageFormat verticalFormat = VerticalImageFormat.Stretched;
        #endregion

        #region Constructor
        public TiledImageWindow(string name)
            : base(name) {
        }
        #endregion

        #region Window method
        protected override void DrawSelf(float z) {
            if (image == null)
                return;

            // render the image
            Rect drawArea = UnclippedInnerRect;
            // update our alpha values
            imageColors.SetAlpha(this.EffectiveAlpha);

            SizeF imageSize = new SizeF(image.Width, image.Height);

            // calculate number of times to tile image based of formatting options
            int horzTiles = (horizontalFormat == HorizontalImageFormat.Tiled) ?
                (int)((drawArea.Width + (imageSize.Width - 1)) / imageSize.Width) : 1;

            int vertTiles = (verticalFormat == VerticalImageFormat.Tiled) ?
                (int)((drawArea.Height + (imageSize.Height - 1)) / imageSize.Height) : 1;

            // calculate 'base' X co-ordinate, depending upon formatting
            float baseX = 0, baseY = 0;
            PointF position = new PointF(drawArea.Left, drawArea.Top);

            // calc horizontal base position
            switch (horizontalFormat) {
                case HorizontalImageFormat.Stretched:
                    imageSize.Width = drawArea.Width;
                    baseX = position.X;
                    break;

                case HorizontalImageFormat.Tiled:
                case HorizontalImageFormat.LeftAligned:
                    baseX = position.X;
                    break;

                case HorizontalImageFormat.Centered:
                    baseX = position.X + ((drawArea.Width - imageSize.Width) * 0.5f);
                    break;

                case HorizontalImageFormat.RightAligned:
                    baseX = position.X + drawArea.Width - imageSize.Width;
                    break;

                default:
                    throw new NotImplementedException("An unknown horizontal format was specified for a RenderableImage.");
            }

            // calc vertical base position
            switch (verticalFormat) {
                case VerticalImageFormat.Stretched:
                    imageSize.Height = drawArea.Height;
                    baseY = position.Y;
                    break;

                case VerticalImageFormat.Tiled:
                case VerticalImageFormat.Top:
                    baseY = position.Y;
                    break;

                case VerticalImageFormat.Centered:
                    baseY = position.Y + ((drawArea.Height - imageSize.Height) * 0.5f);
                    break;

                case VerticalImageFormat.Bottom:
                    baseY = position.Y + drawArea.Height - imageSize.Height;
                    break;

                default:
                    throw new NotImplementedException("An unknown vertical format was specified for a RenderableImage.");
            }

            Vector3 drawPos = new Vector3(baseX, baseY, z);
            Rect clipRect = null;
            if (clipParent != null)
                // We may be clipped by our parent
                clipRect = clipParent.PixelRect;
            // perform actual rendering
            for (int row = 0; row < vertTiles; row++) {
                drawPos.x = baseX;

                for (int col = 0; col < horzTiles; col++) {
                    Rect destArea = new Rect(drawPos.x,
                                             drawPos.x + imageSize.Width,
                                             drawPos.y,
                                             drawPos.y + imageSize.Height);
                    image.Draw(destArea, z, clipRect, imageColors);
                    drawPos.x += imageSize.Width;
                }

                drawPos.y += imageSize.Height;
            }
        }
        #endregion
    }

    /// <summary>
    ///   Variant of StaticImage that allows me to control which widget
    ///   is in front.  This is the core window object that is able to
    ///   draw images.
    /// </summary>
    public class LayeredStaticImage : TiledImageWindow {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LayeredStaticImage));
        
        // Delta between different layer levels
        // Enough for 16 elements
        public const float GuiZLevelOffset = 4 * Renderer.GuiZElementStep;
        public const float GuiZLayerLevelStep = 4 * GuiZLevelOffset;
        public const float GuiZFrameLevelStep = 16 * GuiZLayerLevelStep;
        public const float GuiZFrameStrataStep = 16 * GuiZFrameLevelStep;

        protected FrameStrata frameStrata = FrameStrata.Medium;
        protected LayerLevel layerLevel = LayerLevel.Unknown;
        protected int frameLevel;
        protected int levelOffset = 0;

        public LayeredStaticImage(string name)
            : base(name) {
            throw new NotImplementedException();
        }

        public LayeredStaticImage(string name, Window clipWindow)
            : base(name) {
            clipParent = clipWindow;
        }

        protected override void DrawSelf(float z) {
            float zOffset = (int)frameStrata * GuiZFrameStrataStep +
                            (int)frameLevel * GuiZFrameLevelStep +
                            (int)layerLevel * GuiZLayerLevelStep +
                            (int)levelOffset * GuiZLevelOffset;

            float maxOffset = (int)FrameStrata.Maximum * GuiZFrameStrataStep;
            float curOffset = maxOffset - zOffset;

           if (log.IsDebugEnabled) {
               string imageName = (image != null) ? image.Name : null;
               log.DebugFormat("Drawing {0} from {1} at {2} with level {3}/{4}/{5}/{6}", name, imageName,
                               z + curOffset, frameStrata, frameLevel, layerLevel, levelOffset);
            }
            
            base.DrawSelf(z + curOffset);
        }
        public int LevelOffset {
            get { return levelOffset; }
            set { 
                if (levelOffset != value) {
                    Dirty = true;
                    levelOffset = value;
                }
            }
        }
        public int FrameLevel {
            get { return frameLevel; }
            set { 
                if (frameLevel != value) {
                    Dirty = true;
                    frameLevel = value;
                }
            }
        }
        public LayerLevel LayerLevel {
            get { return layerLevel; }
            set { 
                if (layerLevel != value) {
                    Dirty = true;
                    layerLevel = value;
                }
            }
        }
        public FrameStrata FrameStrata {
            get { return frameStrata; }
            set { 
                if (frameStrata != value) {
                    Dirty = true;
                    frameStrata = value;
                }
            }
        }
    }
}
