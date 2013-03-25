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

// #define DEBUG_FONT_GLYPH

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;

using Axiom.MathLib;
using Axiom.Core;

namespace Multiverse.Gui {
#if NOT
    public class ComplexFont
    {
        string name;
        FontFamily fontFamily;

        public List<TextRange> GetLines(string text, float wrapWidth, HorizontalTextFormat format, bool nonSpaceWrap)
        {
        }

        public float GetWrappedTextExtent(string text, float wrapWidth, bool nonSpaceWrap)
        {
        }

        public float GetFormattedLineCount(string text, Rect area, HorizontalTextFormat format)
        {
        }

        public float GetMaximumTextExtent(string text, List<TextRange> lines)
        {
        }

        public void DrawTextLine(string text, int offset, int count, Vector3 pos, Rect clip, ColorRect colors)
        {
        }
    }
#endif
    /// <summary>
    ///   This is the Font class for rendering text.
    ///   There is some skeletal support for leftToRight vs. rightToLeft, 
    ///   but I don't expect that to work.
    /// </summary>
    public class SimpleFont {
   		/// <summary>
		///		Struct for hold per-glyph data.
		/// </summary>
		public struct GlyphData {
			public TextureInfo TextureInfo;
			public float HorizontalAdvance;
			public char Character;
		}

        public class LineInfo {
            public int start;
            public int end;
            public float lineWidth;
        }

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SimpleFont));
        
        // Amount of spacing to put between glyphs in the font bitmap.
        // This padding prevents portions of one glyph from showing up in 
        // other glyphs.  It must be a whole number.
        const float InterGlyphPadSpace = 2.0f;

        /// <summary>
        ///		Needed to offset the start of the in memory bitmap to exclude this data.
        /// </summary>
        const int BitmapHeaderSize = 54;

        string name;
        System.Drawing.Font font;
        FontFamily fontFamily;

        bool leftToRight = true;
        /// <summary>
        ///		Descent of font in pixels.
        /// </summary>
        float cellDescent;
        /// <summary>
        ///		Height of font in pixels, a.k.a Line spacing.
        /// </summary>
        float ySpacing;

        // TODO: To support chinese characters efficiently, I should probably
        //       have a map from character to index.
        Dictionary<char, int> glyphMap = new Dictionary<char, int>();
        List<TextureAtlas> glyphAtlasList = new List<TextureAtlas>();
        GlyphData[] glyphData;

        // TextureAtlas glyphAtlas;

        internal SimpleFont(string name, FontFamily fontFamily, int size, FontStyle fontStyle, string glyphSet) {
            this.name = name;
            this.fontFamily = fontFamily;
            CreateFontGlyphSet(glyphSet, size, fontStyle);
        }

        internal SimpleFont(string name, FontFamily fontFamily, int size, FontStyle fontStyle, char firstChar, char lastChar) {
            this.name = name;
            this.fontFamily = fontFamily;
            CreateFontGlyphSet(firstChar, lastChar, size, fontStyle);
        }

#if NOT
        /// <summary>
        ///   Return the horizontal pixel extent given text would be formatted to.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formatArea"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public float GetFormattedTextExtent(string text, Rect area, HorizontalTextFormat format) {
            float lineWidth;
            float widest = 0;
            int lineStart = 0, lineEnd = 0;
            string currLine;

            Rect tmpDrawArea = area;

            while (lineEnd < text.Length) {
                if ((lineEnd = text.IndexOf('\n', lineStart)) == -1)
                    lineEnd = text.Length;
                currLine = text.Substring(lineStart, lineEnd - lineStart);
                lineStart = lineEnd + 1;		// +1 to skip \n char

                switch (format) {
                    case HorizontalTextFormat.Centered:
                    case HorizontalTextFormat.Right:
                    case HorizontalTextFormat.Left:
                        lineWidth = GetTextExtent(currLine);
                        break;

                    case HorizontalTextFormat.WordWrapLeft:
                    case HorizontalTextFormat.WordWrapRight:
                    case HorizontalTextFormat.WordWrapCentered:
                        lineWidth = GetWrappedTextExtent(currLine, area.Width, false);
                        break;

                    default:
                        throw new NotImplementedException("Font.GetFormattedTextExtent - Unknown or unsupported TextFormatting value specified.");
                }

                if (lineWidth > widest)
                    widest = lineWidth;
            }

            return widest;
        }
#endif

        public List<TextRange> GetLines(string text, float wrapWidth, HorizontalTextFormat format, bool nonSpaceWrap) {
             switch (format) {
                case HorizontalTextFormat.Left:
                case HorizontalTextFormat.Right:
                case HorizontalTextFormat.Centered:
                    return TextWrapHelper.GetLines(text);
                case HorizontalTextFormat.WordWrapLeft:
                case HorizontalTextFormat.WordWrapRight:
                case HorizontalTextFormat.WordWrapCentered:
                    return TextWrapHelper.GetWrappedLines(this, text, wrapWidth, nonSpaceWrap);
                default:
                    throw new NotImplementedException(string.Format("Unknown text format option '{0}'", format));
            }
        }
        /// <summary>
        ///   Returns extent of widest line of wrapped text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wrapWidth"></param>
        /// <returns></returns>
        public float GetWrappedTextExtent(string text, float wrapWidth, bool nonSpaceWrap) {
            List<TextRange> lines = 
                TextWrapHelper.GetWrappedLines(this, text, wrapWidth, nonSpaceWrap);
            return GetMaximumTextExtent(text, lines);
        }

        public float GetFormattedLineCount(string text, Rect area, HorizontalTextFormat format) {
            List<TextRange> lines = GetLines(text, area.Width, format, false);
            return lines.Count;
        }

        public float GetMaximumTextExtent(string text, List<TextRange> lines) {
            float widest = 0;
            foreach (TextRange line in lines) {
                float lineWidth = this.GetTextExtent(text, line.start, line.Length);
                if (lineWidth > widest)
                    widest = lineWidth;
            }
            return widest;
        }

        public float GetTextExtent(string text) {
            return GetTextExtent(text, 0, text.Length);
        }

        public float GetTextExtent(string text, int offset, int count) {
            float currentX = 0;
            for (int i = offset; i < offset + count; ++i) {
                char c = text[i];
                if (!IsCharacterAvailable(c))
                    continue;
                int glyphIndex = glyphMap[c];
                currentX += glyphData[glyphIndex].HorizontalAdvance;
            }
            return currentX;
        }

        public bool IsCharacterAvailable(char c) {
            // Debug.Assert(glyphMap.ContainsKey(c));
            return glyphMap.ContainsKey(c);
        }

        /// <summary>
        ///		Draw text into a specified area of the display.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="area">
        ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
        ///		using this Rect depending upon the option specified in <paramref name="format"/>.
        /// </param>
        /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
        /// <param name="clip">Rect object describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
        /// <param name="format">The text formatting required.</param>
        /// <param name="colors">
        ///		ColorRect object describing the colors to be applied when drawing the text.
        ///		The colors specified in here are applied to each glyph, rather than the text as a whole.
        /// </param>
        /// <returns>The number of lines output.  This does not consider clipping, so if all text was clipped, this would still return >=1.</returns>
        public int DrawText(string text, Rect area, float z, Rect clip, 
                            HorizontalTextFormat horzFormat, 
                            VerticalTextFormat vertFormat, 
                            ColorRect colors) {
            List<TextRange> lines = GetLines(text, area.Width, horzFormat, false);
            return DrawText(text, lines, area, z, clip, horzFormat, vertFormat, colors);
        }

        /// <summary>
        ///		Draw text into a specified area of the display.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="area">
        ///		Rect object describing the area of the display where the text is to be rendered.  The text is not clipped to this Rect, but is formatted
        ///		using this Rect depending upon the option specified in <paramref name="format"/>.
        /// </param>
        /// <param name="z">float value specifying the z co-ordinate for the drawn text.</param>
        /// <param name="clip">Rect object describing the clipping area for the drawing.  No drawing will occur outside this Rect.</param>
        /// <param name="format">The text formatting required.</param>
        /// <param name="colors">
        ///		ColorRect object describing the colors to be applied when drawing the text.
        ///		The colors specified in here are applied to each glyph, rather than the text as a whole.
        /// </param>
        /// <returns>The number of lines output.  This does not consider clipping, so if all text was clipped, this would still return >=1.</returns>
        public int DrawText(string text, List<TextRange> lines, Rect area, float z, Rect clip, HorizontalTextFormat horzFormat, VerticalTextFormat vertFormat, ColorRect colors) {
            foreach (TextRange line in lines) {
                PointF offset = GetOffset(text, lines, line.start, horzFormat, vertFormat, area.Width, area.Height, leftToRight);
                DrawTextLine(text, line.start, line.Length,
                             new Vector3(area.Left + offset.X, area.Top + offset.Y, z),
                             clip, colors);
            }
            return lines.Count;
        }

        /// <summary>
        ///   This is the basic method to draw text.  
        ///   It does not do word or line wrapping.
        /// </summary>
        /// <param name="text">the string to write</param>
        /// <param name="pos">the position to start the draw</param>
        /// <param name="clip">the rectangle used to clip the draw calls or null to avoid clipping</param>
        /// <param name="colors">the color to use for the text</param>
        public void DrawTextLine(string text, Vector3 pos, Rect clip, ColorRect colors) {
            DrawTextLine(text, 0, text.Length, pos, clip, colors);
        }
        
        /// <summary>
        ///   This is a fairly basic method to draw text.  
        ///   It does not do word or line wrapping.
        /// </summary>
        /// <param name="text">the string to write</param>
        /// <param name="offset">number of characters from str that should be ignored</param>
        /// <param name="count">number of characters from text that should be considered</param>
        /// <param name="pos">the position to start the draw</param>
        /// <param name="clip">the rectangle used to clip the draw calls or null to avoid clipping</param>
        /// <param name="colors">the color to use for the text</param>
        public void DrawTextLine(string text, int offset, int count, Vector3 pos, Rect clip, ColorRect colors) {
            Vector3 currentPos = pos;
            for (int i = offset; i < offset + count; i++) {
                char c = text[i];
                if (!IsCharacterAvailable(c)) {
                    log.InfoFormat("Character '{0}' not present in font {1}", c, this.font);
                    continue;
                }
                int glyphIndex = glyphMap[c];
                TextureInfo glyphImage = glyphData[glyphIndex].TextureInfo;
                // log.DebugFormat("Drawing character {0} from texture atlas {1}", c, glyphImage.Atlas.Name);
                if (!leftToRight)
                    currentPos.x -= glyphData[glyphIndex].HorizontalAdvance;
                glyphImage.Draw(currentPos, clip, colors);
                if (leftToRight)
                    currentPos.x += glyphData[glyphIndex].HorizontalAdvance;
            }
        }

        /// <summary>
        ///   Get the index of the character at position pos
        /// </summary>
        /// <param name="pos">the position of the character within the text draw area</param>
        /// <param name="text"></param>
        /// <param name="lines"></param>
        /// <param name="horzFormat"></param>
        /// <param name="vertFormat"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="leftToRight"></param>
        /// <returns></returns>
        public int GetTextIndexFromPosition(PointF pos, string text, 
                                            List<TextRange> lines,
                                            HorizontalTextFormat horzFormat,
                                            VerticalTextFormat vertFormat,
                                            float width, float height, 
                                            bool leftToRight) {
            float y = GetTextStartOffset(lines.Count, 0, vertFormat, height);
            float deltaY = pos.Y - y;
            if (deltaY < 0)
                return 0;
            int lineIndex = (int)(deltaY / this.LineSpacing);
            if (lineIndex >= lines.Count)
                return text.Length;
            float x = GetTextStartOffset(text, lines[lineIndex], horzFormat, width, leftToRight);
            float deltaX = pos.X - x;
            if (!leftToRight)
                throw new NotImplementedException("rightToLeft layout not fully supported");
            if (deltaX < 0)
                return lines[lineIndex].start;
            return GetCharAtPixel(text, lines[lineIndex].start, lines[lineIndex].end - lines[lineIndex].start, deltaX);
        }

        protected float GetTextStartOffset(int lineCount, int lineIndex,
                                           VerticalTextFormat format,
                                           float height) {
            switch (format) {
                case VerticalTextFormat.Top:
                    return lineIndex * this.LineSpacing;
                case VerticalTextFormat.Bottom:
                    return height - (lineCount - lineIndex) * this.LineSpacing;
                case VerticalTextFormat.Centered:
                    return (height - lineCount * this.LineSpacing) / 2 + lineIndex * this.LineSpacing;
                default:
                    throw new NotImplementedException(string.Format("Unknown text format option '{0}'", format));
            }
        }
        
        protected float GetTextStartOffset(string text, TextRange line, 
                                           HorizontalTextFormat format, 
                                           float width, bool leftToRight) {
            float baseX = 0;
            switch (format) {
                case HorizontalTextFormat.Left:
                case HorizontalTextFormat.WordWrapLeft:
                    if (!leftToRight)
                        baseX += GetTextExtent(text, line.start, line.Length);
                    break;
                case HorizontalTextFormat.Right:
                case HorizontalTextFormat.WordWrapRight:
                    baseX = width;
                    if (leftToRight)
                        baseX -= GetTextExtent(text, line.start, line.Length);
                    break;
                case HorizontalTextFormat.Centered:
                case HorizontalTextFormat.WordWrapCentered:
                    if (leftToRight)
                        baseX = (float)Math.Floor((width - GetTextExtent(text, line.start, line.Length)) * 0.5f);
                    else
                        baseX = width - (float)Math.Floor((width - GetTextExtent(text, line.start, line.Length)) * 0.5f);
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unknown text format option '{0}'", format));
            }
            return baseX;
        }
        /// <summary>
        ///   Determine the line that owns the character at textIndex
        /// </summary>
        /// <param name="lines">the pre-wrapped list of lines</param>
        /// <param name="index">the index of the character</param>
        /// <returns></returns>
        public int GetLineIndex(List<TextRange> lines, int index) {
            // Find which line we care about
            for (int lineIndex = 0; lineIndex < lines.Count; ++lineIndex) {
                TextRange line = lines[lineIndex];
                if (line.start > index) {
                    // The character wasn't on this line, so it must have been
                    // on the previous line.  If this is the first line, we 
                    // will return -1.
                    return lineIndex - 1;
                } else if (line.end > index) {
                    // our text index points to a characer on this line
                    return lineIndex;
                }
            }
            // we ran out of lines, so return the last line
            // if there were no lines, we return -1.
            return lines.Count - 1;
        }

        /// <summary>
        ///  Gets the offset into the rectangle of the top left (or top right 
        ///  if right to left) for the character at textIndex.
        /// </summary>
        /// <param name="textIndex"></param>
        /// <returns></returns>
        public PointF GetOffset(string text, List<TextRange> lines, 
                               int textIndex,
                               HorizontalTextFormat horzFormat, 
                               VerticalTextFormat vertFormat, 
                               float width, float height, bool leftToRight) {
            int lineIndex = GetLineIndex(lines, textIndex);
            if (lineIndex == -1)
                throw new Exception("Invalid text index");
            TextRange line = lines[lineIndex];
            PointF rv = new PointF();
            // Get the top left of the text area
            rv.X = GetTextStartOffset(text, line, horzFormat, width, leftToRight);
            rv.Y = GetTextStartOffset(lines.Count, lineIndex, vertFormat, height);
            // Move to the left past some characters
            if (textIndex > line.end)
                textIndex = line.end;
            rv.X += GetTextExtent(text, line.start, textIndex - line.start);
            return rv;
        }


        public int GetCharAtPixel(string text, float x) {
            return GetCharAtPixel(text, 0, text.Length, x);
        }
   
        /// <summary>
        ///   Get the index into the string that corresponds to a click
        ///   that is x pixels from the start of the text draw area.
        ///   If we are drawing from left to right, this offset should be
        ///   from the left.  If we are drawing from right to left, this
        ///   offset should be from the right.
        ///   This code assumes HorizontalTextFormat.Left 
        ///   (or HorizontalTextFormat.Right if leftToRight is false)
        ///   justified text.
        /// </summary>
        /// <param name="text">string that is drawn into the widget</param>
        /// <param name="offset">number of characters from str that should be ignored</param>
        /// <param name="count">number of characters from text that should be considered</param>
        /// <param name="x">number of pixels from the start of the text draw area</param>
        /// <returns></returns>
        public int GetCharAtPixel(string text, int offset, int count, float x) {
            float currentX = 0;
            for (int i = offset; i < offset + count; ++i) {
                char c = text[i];
                if (!IsCharacterAvailable(c))
                    continue;
                int glyphIndex = glyphMap[c];
                currentX += glyphData[glyphIndex].HorizontalAdvance;
                if (currentX > x)
                    return i;
            }
            return count;
        }

        public float LineSpacing {
            get { return ySpacing; }
        }

        //protected System.Drawing.Font GetFontBySpacing(float lineSpacingPixel, FontStyle fontStyle) {
        //    // Compute size based on the regular font.  If we are using bold 
        //    // and italic inline with the normal font, I don't want to switch 
        //    // font sizes mid-line.
        //    int emHeight = fontFamily.GetEmHeight(FontStyle.Regular);
        //    int lineSpacing = fontFamily.GetLineSpacing(FontStyle.Regular);

        //    // this is the largest emSize that will fit
        //    float emSize = lineSpacingPixel * ((float)emHeight / lineSpacing);
        //    return GetFont(emSize, fontFlags);
        //}

        protected void CreateFontGlyphSet(string chars, int size, FontStyle fontStyle)
        {
            glyphData = new GlyphData[chars.Length];

            // used for calculating position in the image for rendering the characters
            float x = 0, y = 0;

            StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

            int emHeight = fontFamily.GetEmHeight(FontStyle.Regular);
            int lineSpacing = fontFamily.GetLineSpacing(FontStyle.Regular);
            int descent = fontFamily.GetCellDescent(FontStyle.Regular);

            this.font = new System.Drawing.Font(fontFamily, size, fontStyle, GraphicsUnit.Pixel);
            log.InfoFormat("Code page for {0} = {1}", font, font.GdiCharSet);

            float lineSpacingPixel = font.Size * (float)lineSpacing / emHeight;
            float descentPixel = font.Size * (float)descent / emHeight;

            this.cellDescent = descentPixel;
            this.ySpacing = (float)Math.Ceiling(lineSpacingPixel);

            float height = ySpacing;

            int descentPixelCeil = (int)Math.Ceiling(descentPixel);

            int bitmapHeight = 512;
            int bitmapWidth = 512;

            int charIndex = 0;
            int glyphSet = 0;

            while (charIndex < chars.Length)
            {
                Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // get a handles to the graphics context of the bitmap
                Graphics g = Graphics.FromImage(bitmap);
                g.PageUnit = GraphicsUnit.Pixel;
                // Ideally, gdi would handle alpha correctly, but it doesn't seem to
                g.Clear(System.Drawing.Color.FromArgb(0, 1, 0, 0));
                // these fonts better look good!
                // g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                // g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                // g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                // g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                // g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                // g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                // g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                // g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

                // Initialize up our x and y offset in this image
                x = 0;
                y = descentPixel;
                int firstChar = charIndex;

                Dictionary<char, Rect> glyphRects = new Dictionary<char, Rect>();
                // loop through each character in the glyph string and draw it to the bitmap
                while (charIndex < chars.Length)
                {
                    char c = chars[charIndex];

                    SizeF metrics = g.MeasureString(c.ToString(), font, 1024, format);
                    float width = (float)Math.Ceiling(metrics.Width);

                    // are we gonna wrap?
                    if (x + width > bitmapWidth)
                    {
                        // increment the y coord and reset x to move to the beginning of next line
                        y += (height + InterGlyphPadSpace);
                        x = 0;
                    }

                    if (y + height > bitmapHeight)
                        // need to break to a new image
                        break;

                    // draw the character
                    g.DrawString(c.ToString(), font, Brushes.White, x, y, format);

                    Rect rect = new Rect();

                    // calculate the texture coords for the character
                    // I think that these rectangles are not inclusive, 
                    rect.Left = x;
                    rect.Right = rect.Left + width;
                    rect.Top = y;
                    rect.Bottom = rect.Top + height;

                    // Stash the area that we drew into, so that we can
                    // use it later to construct the atlas.
                    glyphRects[c] = rect;

                    // increment X by the width of the current char
                    x += (width + InterGlyphPadSpace);
                    charIndex++;
                }
                // Ok, we are done with this texture
#if DEBUG_FONT_GLYPH
                {
                    // Save this for debugging
                    System.Drawing.Imaging.ImageFormat imgformat = System.Drawing.Imaging.ImageFormat.Bmp;
                    string debugFileName = string.Format("{0}_{1}_{2}.{3}", fontFamily.Name, size, glyphSet, imgformat.ToString().ToLower());
                    FileStream debugStream = new FileStream(debugFileName, FileMode.OpenOrCreate);
                    bitmap.Save(debugStream, imgformat);
                    debugStream.Close();
                    log.InfoFormat("Saved font file: {0}: {1}x{2}", debugFileName, bitmap.Width, bitmap.Height);
                }
#endif
                // save the image to a memory stream
                Stream stream = new MemoryStream();
                // flip the image
                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                // destroy the bitmap
                bitmap.Dispose();

                // Bitmap headers are 54 bytes.. skip them
                stream.Position = BitmapHeaderSize;

                Axiom.Media.Image image = Axiom.Media.Image.FromRawStream(stream, bitmapWidth, bitmapHeight, Axiom.Media.PixelFormat.A8R8G8B8);
                string imgName = String.Format("_font_glyphs_{0}_{1}", this.name, glyphSet);
                Texture fontTexture = TextureManager.Instance.LoadImage(imgName, image);

                // Construct and populate the atlas and glyph data structures
                TextureAtlas glyphAtlas = AtlasManager.Instance.CreateAtlas(imgName, fontTexture);
                glyphAtlasList.Add(glyphAtlas);
                for (int i = firstChar; i < charIndex; i++)
                {
                    Rect rect = glyphRects[chars[i]];
                    TextureInfo textureInfo = glyphAtlas.DefineImage(chars[i].ToString(), rect);
                    glyphData[i].TextureInfo = textureInfo;
                    glyphData[i].HorizontalAdvance = rect.Width;
                    glyphData[i].Character = chars[i];
                    glyphMap[chars[i]] = i;
                }
                glyphSet++;
            }

#if DEBUG_FONT_GLYPH
            {
                Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // get a handles to the graphics context of the bitmap
                Graphics g = Graphics.FromImage(bitmap);
                g.PageUnit = GraphicsUnit.Pixel;
                // Ideally, gdi would handle alpha correctly, but it doesn't seem to
                g.Clear(System.Drawing.Color.FromArgb(0, 1, 0, 0));
                // these fonts better look good!
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                string test_text = "\u0031\u0644\u0032\u0644\u0627\u0645\u0020\u0639\u0644\u064a\u0028\u0643\u0029\u0645\u002e\u0020\u0643\u064a\u0641\u0020\u0627\u0644\u062d\u0627\u0644\u0039";
                CharacterRange[] ranges = new CharacterRange[test_text.Length];
                for (int i = 0; i < test_text.Length; ++i)
                    ranges[i] = new CharacterRange(i, 1);

                // format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.DirectionRightToLeft;
                format.SetMeasurableCharacterRanges(ranges);
                System.Drawing.Region[] regions = g.MeasureCharacterRanges(test_text, font, new RectangleF(0, 0, 600, 400), format);
                foreach (System.Drawing.Region region in regions)
                    log.InfoFormat("Region: {0}", region.GetBounds(g));

                g.DrawString(test_text, font, Brushes.White, 100, 0, format);
                // save the image to a memory stream
                Stream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                // Save this for debugging
                System.Drawing.Imaging.ImageFormat imgformat = System.Drawing.Imaging.ImageFormat.Bmp;
                string debugFileName = string.Format("{0}_{1}_{2}.{3}", fontFamily.Name, size, glyphSet, imgformat.ToString().ToLower());
                FileStream debugStream = new FileStream(debugFileName, FileMode.OpenOrCreate);
                bitmap.Save(debugStream, imgformat);
                debugStream.Close();
                log.InfoFormat("Saved font file: {0}: {1}x{2}", debugFileName, bitmap.Width, bitmap.Height);

                // destroy the bitmap
                bitmap.Dispose();
                stream.Close();
            }
#endif
        }

        /// <summary>
        ///		Creates a font glyph set for the given range of characters.
        /// </summary>
        /// <param name="firstCodePoint">Starting character.</param>
        /// <param name="lastCodePoint">Ending character.</param>
        /// <param name="size">Size of the font.</param>
        protected void CreateFontGlyphSet(char firstChar, char lastChar, int size, FontStyle fontStyle) {
            StringBuilder sb = new StringBuilder();
            // build a string from the range of characters
            for (int i = firstChar; i <= lastChar; i++) 
                sb.Append((char)i);
            // create the glyph set using the string built from the glyph range
            CreateFontGlyphSet(sb.ToString(), size, fontStyle);
        }
    }
}
