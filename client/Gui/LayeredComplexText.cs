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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

using Axiom.MathLib;
using Axiom.Utility;

namespace Multiverse.Gui {
    /// <summary>
    ///   Variant of StaticText that allows me to control which widget
    ///   is in front.  This is the core window object that is able to
    ///   draw text.
    /// </summary>
    public class LayeredComplexText : LayeredText, IDisposable
    {
        public static TimingMeter generateTextRegionsMeter = MeterManager.GetMeter("GenerateTextRegions", "LayeredComplexText");
        public static TimingMeter clearMeter = MeterManager.GetMeter("Clear", "LayeredComplexText");
        public static TimingMeter drawMeter = MeterManager.GetMeter("DrawString", "LayeredComplexText");
        public static TimingMeter bitmapSaveMeter = MeterManager.GetMeter("Save Bitmap", "LayeredComplexText");
        public static TimingMeter imageFromStreamMeter = MeterManager.GetMeter("Image.FromStream", "LayeredComplexText");
        public static TimingMeter loadImageMeter = MeterManager.GetMeter("LoadImage", "LayeredComplexText");
        public static TimingMeter measureStringMeter = MeterManager.GetMeter("MeasureString", "LayeredComplexText");
        public static TimingMeter measureMeter = MeterManager.GetMeter("MeasureCharacterRanges", "LayeredComplexText");
        public static TimingMeter drawTextMeter = MeterManager.GetMeter("DrawText", "LayeredComplexText");

        bool rightToLeft = false;
        Font font;
        FontFamily fontFamily;
        StringFormat format;
        float emSize;
        bool textDirty = true;
        bool regionsDirty = true;
        TextureAtlas chunkAtlas;
        Bitmap bitmap;
        bool dynamic = false;
        Region[] regions;
        TextureInfo[] chunks;
        Graphics staticGraphics = null;

        /// <summary>
        ///		Needed to offset the start of the in memory bitmap to exclude this data.
        /// </summary>
        const int BitmapHeaderSize = 54;

        public LayeredComplexText(string name, Window clipWindow, bool dynamic, bool rightToLeft)
            : base(name)
        {
            clipParent = clipWindow;
            this.dynamic = dynamic;
            int bitmapWidth = 128;
            int bitmapHeight = 128;
 
            format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.Trimming = StringTrimming.None;
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            if (rightToLeft)
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            CreateImage(bitmapWidth, bitmapHeight);
        }

        protected void CreateImage(int width, int height)
        {
            string imgName = String.Format("_font_string_{0}", this.name);
            Axiom.Core.Texture fontTexture = null;
            if (Axiom.Core.TextureManager.Instance.HasResource(imgName))
            {
                fontTexture = Axiom.Core.TextureManager.Instance.GetByName(imgName);
                chunkAtlas = AtlasManager.Instance.GetTextureAtlas(imgName);
            }
            else
            {
                fontTexture = Axiom.Core.TextureManager.Instance.LoadImage(imgName, null);
                chunkAtlas = AtlasManager.Instance.CreateAtlas(imgName, fontTexture);
            }

            System.Diagnostics.Debug.Assert(bitmap == null);
            if (dynamic)
                bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            else
                bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            CopyBitmapToTexture(bitmap, fontTexture);
            textDirty = true;
        }
        protected override void DrawText(float z)
        {
            if (textDirty || regionsDirty)
                GenerateTextRegions();
            drawTextMeter.Enter();       
            // throw new NotImplementedException();
            Graphics g = GetGraphics();
            // Rect renderRect = TextRenderArea;
            Rect renderRect = GetVisibleTextArea();
            for (int i = 0; i < regions.Length; ++i)
            {
                RectangleF rect = regions[i].GetBounds(g);
                rect.Offset(renderRect.Left, renderRect.Top);
                TextureInfo chunk = chunks[i];
                chunk.Draw(new Vector3(rect.Left, rect.Top, z), this.TextRenderArea);
            }
            ReleaseGraphics(g);
            drawTextMeter.Exit();
        }

        /// <summary>
        ///   It looks like GDI+ will draw into an area, and then tell me the 
        ///   area that it used.  If I try to draw the text into that returned
        ///   area, it is often too small, so this method lets me know when I
        ///   need to grow the area before I draw into it.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="range"></param>
        /// <param name="font"></param>
        /// <param name="r"></param>
        /// <param name="origWidth"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        protected bool CheckTextRegion(Graphics g, string text, CharacterRange range, Font font, RectangleF r, float origWidth, StringFormat format)
        {
            CharacterRange[] testRanges = new CharacterRange[] { range };
            format.SetMeasurableCharacterRanges(testRanges);
            Region[] test = g.MeasureCharacterRanges(text.Substring(range.First, range.Length), font, r, format);
            System.Diagnostics.Debug.Assert(test.Length == 1);
            RectangleF testRect = test[0].GetBounds(g);
            if (testRect.Width == origWidth)
                return true;
            return false;
        }

        protected void GenerateTextRegions()
        {
            generateTextRegionsMeter.Enter();

            Graphics g = GetGraphics();

            string text = GetAllText();
            SetAlignment();
            // this first call is to initialize the scroll data, so the 
            // visible text area will be correct.
            // UpdateScrollPosition(textDirty);
            float textHeight = GetTextHeight(g, text, true);
            float textWidth = GetTextWidth(g, text);
            // Update the scrollbars to take the new text into account
            ConfigureScrollbars(textWidth, textHeight);
            if (MaybeGrowTexture())
            {
                // If we resized the texture, we will need to use a new graphics object.
                ReleaseGraphics(g);
                g = GetGraphics();
            }

            RectangleF localRect = this.GetVisibleTextArea().ToRectangleF();
            //string tmp = GetAllText();
            //if (tmp != null && tmp.Length > 0)
            //    System.Diagnostics.Debug.Assert(tmp[0] != '[');
            localRect.Offset(-localRect.Left, -localRect.Top);

            measureMeter.Enter();
            CharacterRange[] charRanges = new CharacterRange[] { new CharacterRange(0, text.Length) };
            format.SetMeasurableCharacterRanges(charRanges);
            regions = g.MeasureCharacterRanges(text, font, localRect, format);
            measureMeter.Exit();

            clearMeter.Enter();
            // Ideally, gdi would handle alpha correctly, but it doesn't seem to
            // g.Clear(normalTextStyle.bgColor.ToColor());
            Color clearColor = normalTextStyle.bgColor.ToColor();
            if (chunkAtlas.texture.HasAlpha && !normalTextStyle.bgEnabled)
                // They didn't ask for a background, and we don't require one
                clearColor = Color.FromArgb(0, clearColor);
            g.Clear(clearColor);
            // g.CompositingMode = CompositingMode.SourceCopy;
            // g.CompositingQuality = CompositingQuality.HighQuality;
            // Region fillRegion = new Region(RectangleF.FromLTRB(0, 0, chunkAtlas.texture.Width, chunkAtlas.texture.Height));
            // g.FillRegion(new SolidBrush(clearColor), fillRegion);
            // g.CompositingMode = CompositingMode.SourceOver;
            clearMeter.Exit();

            if (regions.Length == 0)
            {
                log.DebugFormat("Region dimensions for '{0}': None", text);
                // nothing to draw
                textDirty = false;
                regionsDirty = false;
                ReleaseGraphics(g);
                generateTextRegionsMeter.Exit();
                return;
            }
            chunks = new TextureInfo[regions.Length];
            Brush brush = new SolidBrush(normalTextStyle.textColor.ToColor());
            chunkAtlas.Clear();
            drawMeter.Enter();
            System.Diagnostics.Debug.Assert(charRanges.Length == regions.Length);
            for (int i = 0; i < regions.Length; ++i)
            {
                Region region = regions[i];
                CharacterRange range = charRanges[i];
                RectangleF r = region.GetBounds(g);
                float origWidth = r.Width;
                // This loop basically keeps resizing the region we are drawing into
                // until we get the same size result we got in our original region.
                if (!CheckTextRegion(g, text, range, font, r, origWidth, format))
                {
                    log.InfoFormat("Increasing size to fit string.  Old size: {0}; New size: {1}", r, RectangleF.FromLTRB(r.Left, r.Top, r.Right + 1, r.Bottom));
                    r = RectangleF.FromLTRB(r.Left, r.Top, r.Right + 1, r.Bottom);
                    if (!CheckTextRegion(g, text, range, font, r, origWidth, format))
                    {
                        log.ErrorFormat("Increasing size to fit string.  Old size: {0}; New size: {1}", r, RectangleF.FromLTRB(r.Left, r.Top, r.Right + 1, r.Bottom));
                        r = RectangleF.FromLTRB(r.Left, r.Top, r.Right + 1, r.Bottom);
                        if (!CheckTextRegion(g, text, range, font, r, origWidth, format))
                            log.Error("Even after increasing size twice, the string doesn't fit");
                    }

                }
            
#if TEST_BOUNDS
                Region[] test = g.MeasureCharacterRanges(text.Substring(range.First, range.Length), font, localRect, format);
                System.Diagnostics.Debug.Assert(test.Length == 1);
                RectangleF debugRect = test[0].GetBounds(g);
                System.Diagnostics.Debug.Assert(debugRect == r);
                test = g.MeasureCharacterRanges(text.Substring(range.First, range.Length), font, r, format);
                System.Diagnostics.Debug.Assert(test.Length == 1);
                debugRect = test[0].GetBounds(g);
                System.Diagnostics.Debug.Assert(debugRect == r);
#endif
#if PAD_BOUNDS
                // I have a hunch that for some reason, my text doesn't always 
                // render right unless I provide a little extra space.
                RectangleF r2 = RectangleF.FromLTRB(r.Left, r.Top, r.Right + 1, r.Bottom + 1);
                g.DrawString(text.Substring(range.First, range.Length), font, brush, r2, format);
#endif
                g.DrawString(text.Substring(range.First, range.Length), font, brush, r, format);
                // Construct and populate the atlas and chunk data structures
                chunks[i] = chunkAtlas.DefineImage(string.Format("_region{0}", i), r);
                log.DebugFormat("Region dimensions for '{0}': {1}", text.Substring(range.First, range.Length), r);
            }
            drawMeter.Exit();
            // Now that I have updated my area, fetch the text height and width again
            textHeight = GetTextHeight(g, text, true);
            textWidth = GetTextWidth(g, text);
            UpdateScrollPosition(textHeight, textWidth, textDirty);
            textDirty = false;
            regionsDirty = false;
            ReleaseGraphics(g);
            if (!dynamic)
                // If we aren't dynamic, the underlying texture isn't 
                // automatically updated, so we need to copy the bitmap 
                // onto the texture.  This also flips the bitmap, but
                // since we clear it between draws, that should be fine.
                CopyBitmapToTexture(bitmap, chunkAtlas.texture);
            generateTextRegionsMeter.Exit();
        }

        protected void SetAlignment() {
            format.Alignment = StringAlignment.Near;
            switch (this.HorizontalFormat)
            {
                case HorizontalTextFormat.Left:
                    format.Alignment = rightToLeft ? StringAlignment.Far : StringAlignment.Near;
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    break;
                case HorizontalTextFormat.WordWrapLeft:
                    format.Alignment = rightToLeft ? StringAlignment.Far : StringAlignment.Near;
                    format.FormatFlags &= ~StringFormatFlags.NoWrap;
                    break;
                case HorizontalTextFormat.Centered:
                    format.Alignment = StringAlignment.Center;
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    break;
                case HorizontalTextFormat.WordWrapCentered:
                    format.Alignment = StringAlignment.Center;
                    format.FormatFlags &= ~StringFormatFlags.NoWrap;
                    break;
                case HorizontalTextFormat.Right:
                    format.Alignment = rightToLeft ? StringAlignment.Near : StringAlignment.Far;
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    break;
                case HorizontalTextFormat.WordWrapRight:
                    format.Alignment = rightToLeft ? StringAlignment.Near : StringAlignment.Far;
                    format.FormatFlags &= ~StringFormatFlags.NoWrap;
                    break;
            }

            format.LineAlignment = StringAlignment.Near;
            switch (this.VerticalFormat)
            {
                case VerticalTextFormat.Top:
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case VerticalTextFormat.Centered:
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case VerticalTextFormat.Bottom:
                    format.LineAlignment = StringAlignment.Far;
                    break;
            }
        }

        protected void CopyBitmapToTexture(Bitmap fontBitmap, Axiom.Core.Texture fontTexture)
        {
            // save the image to a memory stream
            Stream stream = new MemoryStream();
            // flip the image
            fontBitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
            fontBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

            // Bitmap headers are 54 bytes.. skip them
            stream.Position = BitmapHeaderSize;
            loadImageMeter.Enter();
            if (dynamic)
                fontTexture.LoadRawData(stream, fontBitmap.Width, fontBitmap.Height, Axiom.Media.PixelFormat.R8G8B8);
            else
                fontTexture.LoadRawData(stream, fontBitmap.Width, fontBitmap.Height, Axiom.Media.PixelFormat.A8R8G8B8);
            loadImageMeter.Exit();
        }

        /// <summary>
        ///   Check to see if we need to resize the texture, and return true if
        ///   we do resize it.
        /// </summary>
        /// <returns></returns>
        protected bool MaybeGrowTexture() {
            // I actually need the entire area for the computation of how much
            // space I need, so instead of GetVisibleArea, like I would 
            // normally use, I use TextRenderArea here.
            Rect renderArea = this.GetVisibleTextArea(); 
            Size textureDims = new Size(chunkAtlas.texture.Width, chunkAtlas.texture.Height);
            while (renderArea.Height > textureDims.Height)
                textureDims.Height = textureDims.Height * 2;
            while (renderArea.Width > textureDims.Width)
                textureDims.Width = textureDims.Width * 2;
            if (chunkAtlas.texture.Width != textureDims.Width ||
                chunkAtlas.texture.Height != textureDims.Height)
            {
                log.DebugFormat("Recreating texture with larger size: {0}", textureDims);
                DisposeBitmap();
                CreateImage(textureDims.Width, textureDims.Height);
                return true;
            }
            return false;
        }

        /// <summary>
        ///   This computes how much horizontal space would be required to draw
        ///   all of the text.  This is the width of the widest line.
        /// </summary>
        /// <returns>number of pixels of horizontal space required to draw the text</returns>
        public override float GetTextWidth()
        {
            string text = GetAllText();
            Graphics g = GetGraphics();
            float rv = GetTextWidth(g, text);
            ReleaseGraphics(g);
            return rv;
        }
        /// <summary>
        ///   This computes how much horizontal space would be required to draw
        ///   all of the text.  This is the width of the widest line.
        /// </summary>
        /// <returns>number of pixels of horizontal space required to draw the text</returns>
        protected float GetTextWidth(Graphics g, string text)
        {
            SizeF size = g.MeasureString(text, font, (int)GetVisibleTextArea().Width, format);
            return (float)Math.Ceiling(size.Width + shadowOffset.X);
        }
        /// <summary>
        ///   This computes how much vertical space would be required to draw 
        ///   all the text, wrapping based on window width.
        /// </summary>
        /// <returns>number of pixels of vertical space required to draw the text</returns>
        public override float GetTextHeight(bool includeEmpty)
        {
            string text = GetAllText();
            Graphics g = GetGraphics();
            float rv = GetTextHeight(g, text, includeEmpty);
            ReleaseGraphics(g);
            return rv;
        }        
        /// <summary>
        ///   This computes how much vertical space would be required to draw 
        ///   all the text, wrapping based on window width.
        /// </summary>
        /// <returns>number of pixels of vertical space required to draw the text</returns>
        protected float GetTextHeight(Graphics g, string text, bool includeEmpty)
        {
            SizeF size = g.MeasureString(text, font, (int)GetVisibleTextArea().Width, format);
            log.DebugFormat("Size of text: {0}, {1}", text.Length, size);
            return (float)Math.Ceiling(size.Height + shadowOffset.Y);
        }
        public override int GetStringWidth()
        {
            return (int)Math.Ceiling(GetTextWidth());
        }

        public override void ScrollUp()
        {
            SetVScrollPosition(vertScrollPosition - font.Height);
            OnAreaChanged(new EventArgs());
        }

        public override void ScrollDown()
        {
            SetVScrollPosition(vertScrollPosition + font.Height);
            OnAreaChanged(new EventArgs());
        }

        protected internal override void OnTextChanged(EventArgs args)
        {
            log.Debug("In LayeredComplexText.OnTextChanged");
            textDirty = true;
        }
        protected internal override void OnFontChanged(EventArgs args)
        {
            log.Debug("In LayeredComplexText.OnFontChanged");
            regionsDirty = true;
        }
        protected internal override void OnAreaChanged(EventArgs args)
        {
            log.Debug("In LayeredComplexText.OnAreaChanged");
            regionsDirty = true;
        }
        protected internal override void OnFormatChanged(EventArgs args)
        {
            log.Debug("In LayeredComplexText.OnFormatChanged");
            regionsDirty = true;
        }
        public override void SetFont(string fontFace, int fontHeight, string characterSet)
        {
            // string fontName = string.Format("{0}-{1}", fontFace, fontHeight);
            fontFamily = FontManager.GetFontFamily(fontFace);
            this.emSize = (float)fontHeight;
            this.font = new Font(fontFamily, emSize, FontStyle.Regular, GraphicsUnit.Pixel);
            log.DebugFormat("Code page for {0} = {1}", font, font.GdiCharSet);
            //if (!FontManager.Instance.ContainsKey(fontName))
            //    FontManager.Instance.CreateFont(fontName, fontFace, fontHeight, 0, characterSet);
        }
        protected Graphics GetGraphics()
        {
            Graphics g = null;
            if (staticGraphics != null)
                return staticGraphics;
            if (dynamic)
                g = chunkAtlas.texture.GetGraphics();
            else if (staticGraphics == null) {
                staticGraphics = Graphics.FromImage(bitmap);
                g = staticGraphics;
            }
            g.PageUnit = GraphicsUnit.Pixel;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            return g;
        }
        protected void ReleaseGraphics(Graphics g) {
            if (dynamic) {
                g.Dispose();
                chunkAtlas.texture.ReleaseGraphics();
            }
        }
        public void Dispose()
        {
            DisposeBitmap();
        }
        protected void DisposeBitmap()
        {
            if (staticGraphics != null)
            {
                staticGraphics.Dispose();
                staticGraphics = null;
            }
            // dispose of the bitmap
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }
        }
    }
}
