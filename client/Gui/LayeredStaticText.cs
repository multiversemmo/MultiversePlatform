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
using Axiom.Utility;

namespace Multiverse.Gui {
    /// <summary>
    ///   This class is designed to capture most of the logic needed to 
    ///   display text (including scrollbars), but without the actual
    ///   implementation details (e.g. Font class).
    /// </summary>
    public class LayeredText : Window
    {
        // Create a logger for use in this class and derived classes
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LayeredText));

        // Delta between different layer levels
        // Enough for 16 elements
        public const float GuiZLevelOffset = 4 * Renderer.GuiZElementStep;
        public const float GuiZLayerLevelStep = 4 * GuiZLevelOffset;
        public const float GuiZFrameLevelStep = 16 * GuiZLayerLevelStep;
        public const float GuiZFrameStrataStep = 16 * GuiZFrameLevelStep;

        // Put the text one half of a layer level above the textures.
        // If there are more than 512 elements, the text from the back ones 
        // will be obscured by the elements in front.
        // Shadows are behind the text. Use this to handle sublayers.
        public const float GuiZSubLevelStep = Renderer.GuiZElementStep;

        /// <summary>
        ///   The horizontal formatting that will be used for this text area.
        ///   The default is to left align the text.
        /// </summary>
        protected HorizontalTextFormat horzFormat = HorizontalTextFormat.Left;
        /// <summary>
        ///   The vertical formatting that will be used for this text area.
        ///   The default is to center the text, which works well for single
        ///   lines of text in an area.
        /// </summary>
        protected VerticalTextFormat vertFormat = VerticalTextFormat.Centered;

        /// <summary>
        ///   Determines whether the text is anchored to the bottom of the 
        ///   render area or to the top.
        /// </summary>
        protected bool scrollFromBottom = false;

        /// <summary>
        ///   This is the offset of the text we are drawing.  A positive offset
        ///   means that a portion of our text is to the left of our rectangle.
        ///   A negative offset means that we are actually inset from the left.
        /// </summary>
        protected float horzScrollPosition = 0;
        /// <summary>
        ///   This is the offset of the text we are drawing.  A positive offset
        ///   means that a portion of our text is above the top of our 
        ///   rectangle.  A negative offset means that we are actually inset 
        ///   from the top.
        /// </summary>
        protected float vertScrollPosition = 0;

        protected float horzDocumentSize = 0;
        protected float vertDocumentSize = 0;

        public event EventHandler TextChanged;
        public event EventHandler FontChanged;

        protected FrameStrata frameStrata = FrameStrata.Medium;
        protected int frameLevel = 0;
        protected LayerLevel layerLevel = LayerLevel.Overlay;
        protected SubLevel subLevel = SubLevel.Normal;

        /// <summary>
        ///   This buffer holds our text.  The information about style is
        ///   held in the textChunks member instead.
        /// </summary>
        protected StringBuilder textBuffer = new StringBuilder();
        /// <summary>
        ///   List of text chunks.  This lets us associate styles with regions
        ///   of text.
        /// </summary>
        protected List<TextChunk> textChunks = new List<TextChunk>();

        protected string fontName = null;
        protected TextStyle normalTextStyle = new TextStyle();
        protected TextureInfo bgImage = null;

        protected PointF shadowOffset;
        
        public LayeredText(string name)
            : base(name)
        {
            ConfigureScrollbars(0, 0);
        }

        // Sets the text to be drawn
        public virtual void SetText(string str)
        {
            if (textBuffer.ToString() == str)
                return;
            Dirty = true;
            textBuffer.Length = 0;
            textBuffer.Append(str);
            GenerateTextChunks(str);
            UpdateText();
        }

        /// <summary>
        ///  Get all the text that is displayed
        /// </summary>
        /// <returns></returns>
        public string GetAllText()
        {
            return textBuffer.ToString();
        }

        public void SetTextColor(ColorEx color)
        {
            if (normalTextStyle.textColor.CompareTo(color) == 0)
                return;
            Dirty = true;
            normalTextStyle.textColor = new ColorEx(color);
        }

        public virtual void UpdateText()
        {
            Dirty = true;
            // Trigger an event, in case anyone else cares
            if (TextChanged != null)
                TextChanged(this, new EventArgs());
            OnTextChanged(new EventArgs());
        }
        protected virtual void UpdateFont()
        {
            Dirty = true;
            // Trigger an event, in case anyone else cares
            if (FontChanged != null)
                FontChanged(this, new EventArgs());
            OnFontChanged(new EventArgs());
        }


        public void ApplyStyle(TextStyle style)
        {
            foreach (TextChunk chunk in textChunks) {
                if (chunk.style.Equals(style))
                    continue;
                chunk.style = style;
                Dirty = true;
            }
        }

        protected void ReplaceStyle(TextStyle oldStyle, TextStyle newStyle)
        {
            if (oldStyle.Equals(newStyle))
                return;
            Dirty = true;
            foreach (TextChunk chunk in textChunks)
            {
                if (chunk.style == oldStyle)
                    chunk.style = newStyle;
            }
        }

        public void AddText(string textString, TextStyle style)
        {
            TextChunk lastChunk = null;
            if (textChunks.Count > 0)
                lastChunk = textChunks[textChunks.Count - 1];
            int chunk_start = textBuffer.Length;
            textBuffer.Append(textString);
            int chunk_end = textBuffer.Length;
            if (lastChunk != null &&
                lastChunk.style.Equals(style))
            {
                // Add to the existing chunk
                lastChunk.range.end = chunk_end;
            }
            else
            {
                // Add a new chunk
                textChunks.Add(new TextChunk(new TextRange(chunk_start, chunk_end), new TextStyle(style)));
            }
            UpdateText();
        }

        protected virtual void GenerateTextChunks(string str)
        {
            List<TextChunk> chunks = this.TextChunks;
            chunks.Clear();
            if (str == null)
                str = string.Empty;
            chunks.Add(new TextChunk(new TextRange(0, str.Length), new TextStyle(this.NormalTextStyle)));
        }

        protected void ConfigureScrollbars(float textWidth, float textHeight)
        {
            log.DebugFormat("Called ConfigureScrollbars with dimensions {0}x{1}", textWidth, textHeight);
            if (textWidth == vertDocumentSize && textHeight == horzDocumentSize)
                return;
            Dirty = true;
            vertDocumentSize = textHeight;
            horzDocumentSize = textWidth;
        }

        protected void UpdateScrollPosition(bool textAdded)
        {
            Dirty = true;
            float textHeight = GetTextHeight(true);
            float textWidth = GetTextWidth();
            UpdateScrollPosition(textHeight, textWidth, textAdded);
        }

        protected void UpdateScrollPosition(float textHeight, float textWidth, bool textAdded) 
        {
            // set our text to the text of the editbox
            bool atBottom = false;
            if (vertScrollPosition == vertDocumentSize - TextRenderArea.Height)
                atBottom = true;

            // Update the scrollbars to take the new text into account
            ConfigureScrollbars(textWidth, textHeight);

            // If we just added text, and we are supposed to scroll from the 
            // bottom, or if we were already at the bottom, and we need to 
            // scroll, scroll to the bottom.
            if ((textAdded && scrollFromBottom) ||
                (atBottom && vertDocumentSize > TextRenderArea.Height))
                vertScrollPosition = textHeight - TextRenderArea.Height;
           
            // log.DebugFormat("In update scroll position: vertScollPosition = {0}/{1}/{2}/{3}/{4}", vertScrollPosition, vertDocumentSize, textHeight, TextRenderArea.Height, atBottom);
        }

        protected override void DrawSelf(float z)
        {
            float zOffset = (int)frameStrata * GuiZFrameStrataStep +
                            (int)layerLevel * GuiZLayerLevelStep +
                            (int)frameLevel * GuiZFrameLevelStep;
            float maxOffset = (int)FrameStrata.Maximum * GuiZFrameStrataStep;
            float curOffset = maxOffset - zOffset;

            log.DebugFormat("drawing {0} at {1} with level {2}/{3}/{4}", name, z + curOffset,
                            frameStrata, layerLevel, frameLevel);

            DrawText(z + curOffset - GuiZLevelOffset);
        }

        //protected void ConfigureScrollbars() {
        //    ConfigureScrollbars(GetTextHeight(), GetTextWidth());
        //}		
        /// <summary>
        ///   This is a poorly named function, since it really returns a large
        ///   rectangle that can hold all our text, offset so that the portion
        ///   of the text that is visible is in the top left region.
        /// </summary>
        /// <returns></returns>
        /// FIXME: I'm confused in my usage of this method.  At some points, 
        ///        I expect to determine the wrapping based on this area.
        ///        This isn't appropriate, since I may be scrolling the area.
        ///        For now, return the max of the area covered by text (used 
        ///        when we are scrolling), and the area we draw into (used when
        ///        the text doesn't fill the area).  This lets us still do
        ///        intelligent wrapping.
        protected Rect GetVisibleTextArea()
        {
            Rect temp = this.TextRenderArea;
            float top = temp.Top - vertScrollPosition;
            float left = temp.Left - horzScrollPosition;
            float width = Math.Max(temp.Width, horzDocumentSize);
            float height = Math.Max(temp.Height, vertDocumentSize);
            return new Rect(left, left + width, top, top + height);
        }

        public void SetVScrollPosition(float scrollPosition)
        {
            float textHeight = GetTextHeight(true);
            Rect textRect = this.TextRenderArea;
            // Limit the scroll position to have the text going 
            // all the way down to the bottom
            if (scrollPosition > textHeight - textRect.Height)
                scrollPosition = textHeight - textRect.Height;
            // Limit the scroll position to have the text at 
            // the very top
            if (scrollPosition < 0)
                scrollPosition = 0;
            if (vertScrollPosition != scrollPosition) {
                Dirty = true;
                vertScrollPosition = scrollPosition;
            }
        }

        public void ScrollToTop()
        {
            SetVScrollPosition(0);
        }

        public void ScrollToBottom()
        {
            SetVScrollPosition(GetTextHeight(true));
        }
        public void HandleFontChanged()
        {
            OnFontChanged(new EventArgs());
        }
        public void HandleTextChanged()
        {
            OnTextChanged(new EventArgs());
        }
        public void HandleAreaChanged()
        {
            OnAreaChanged(new EventArgs());
        }
        public void HandleFormatChanged()
        {
            OnFormatChanged(new EventArgs());
        }

        #region Not Implemented methods
        protected virtual void DrawText(float z)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        ///   This computes how much horizontal space would be required to draw
        ///   all of the text.  This is the width of the widest line.
        /// </summary>
        /// <returns>number of pixels of horizontal space required to draw the text</returns>
        public virtual float GetTextWidth()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        ///   This computes how much vertical space would be required to draw 
        ///   all the text, wrapping based on window width.
        /// </summary>
        /// <returns>number of pixels of vertical space required to draw the text</returns>
        public virtual float GetTextHeight(bool includeEmpty)
        {
            throw new NotImplementedException();
        }
        public virtual int GetStringWidth()
        {
            throw new NotImplementedException();
        }

        public virtual void SetFont(string fontFace, int fontHeight, string characterSet)
        {
            throw new NotImplementedException();
        }

        public virtual void ScrollDown() {
            throw new NotImplementedException();
        }

        public virtual void ScrollUp()
        {
            throw new NotImplementedException();
        }
        protected internal virtual void OnTextChanged(EventArgs args)
        {
            throw new NotImplementedException();
        }
        protected internal virtual void OnFontChanged(EventArgs args)
        {
            throw new NotImplementedException();
        }
        protected internal virtual void OnAreaChanged(EventArgs args)
        {
            throw new NotImplementedException();
        }
        protected internal virtual void OnFormatChanged(EventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties
        public List<TextChunk> TextChunks
        {
            get { return textChunks; }
        }
        /// <summary>
        ///   Return a Rect object describing, in un-clipped pixels, the window
        ///   relative area that the text should be rendered in to.
        /// </summary>
        /// <value></value>
        protected Rect TextRenderArea
        {
            get
            {
                return this.UnclippedInnerRect;
            }
        }

        public string Text
        {
            get
            {
                return textBuffer.ToString();
            }
        }

        public TextStyle NormalTextStyle
        {
            get { return normalTextStyle; }
            set
            {
                ReplaceStyle(normalTextStyle, value);
                normalTextStyle = value;
            }
        }
        public bool ScrollFromBottom
        {
            get { return scrollFromBottom; }
            set { scrollFromBottom = value; }
        }

        public TextureInfo BackgroundImage
        {
            set { bgImage = value; }
        }

        public FrameStrata FrameStrata
        {
            get { return frameStrata; }
            set { 
                if (frameStrata != value) {
                    Dirty = true;
                    frameStrata = value; 
                }
            }
        }
        public int FrameLevel
        {
            get { return frameLevel; }
            set { 
                if (frameLevel != value) {
                    Dirty = true;
                    frameLevel = value;
                }
            }
        }
        public LayerLevel LayerLevel
        {
            get { return layerLevel; }
            set {
                if (layerLevel != value) {
                    Dirty = true;
                    layerLevel = value; 
                }
            }
        }
        public HorizontalTextFormat HorizontalFormat
        {
            get { return horzFormat; }
            set { 
                if (horzFormat != value) {
                    Dirty = true;
                    horzFormat = value;
                }
            }
        }

        public VerticalTextFormat VerticalFormat
        {
            get { return vertFormat; }
            set { vertFormat = value; }
        }
        public virtual string FontName
        {
            get { return fontName; }
            set { if (fontName != value) {
                    Dirty = true;
                    fontName = value;
                }
            }
        }

        public PointF ShadowOffset
        {
            get { return shadowOffset; }
            set { if (shadowOffset != value) {
                    Dirty = true;
                    shadowOffset = value;
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///   Variant of StaticText that allows me to control which widget
    ///   is in front.  This is the core window object that is able to
    ///   draw text.
    /// </summary>
    public class LayeredStaticText : LayeredText {
        protected SimpleFont font;
        protected string text = string.Empty;

        /// <summary>
        ///   List of the lines.  This will generally be updated by methods 
        ///   that update our <code>text</code> variable.  These lines have
        ///   been soft wrapped if needed.
        /// </summary>
        protected List<TextRange> lines = null;
        int maxLines = 0; // uncapped

        public LayeredStaticText(string name, Window clipWindow)
            : base(name) {
            clipParent = clipWindow;
            this.TextChanged += new EventHandler(HandleTextChanged);
            this.FontChanged += new EventHandler(HandleFontChanged);
        }


        /// <summary>
        ///   This computes how much vertical space would be required to draw 
        ///   all the text, wrapping based on window width.
        /// </summary>
        /// <returns>number of pixels of vertical space required to draw the text</returns>
        public override float GetTextHeight(bool includeEmpty) {
            int lineCount = lines.Count;
            if (lines.Count >= 1 && lines[lines.Count - 1].Length == 0 && !includeEmpty)
                // remove the last line, since it is empty
                lineCount--;
            if (log.IsDebugEnabled)
                log.DebugFormat("Lines: {0}: text: {1}", lines.Count, this.GetAllText());
            return lineCount * this.Font.LineSpacing;
        }
        /// <summary>
        ///   This computes how much horizontal space would be required to draw
        ///   all of the text.  This is the width of the widest line.
        /// </summary>
        /// <returns>number of pixels of horizontal space required to draw the text</returns>
        public override float GetTextWidth() {
            return this.Font.GetMaximumTextExtent(text, lines);
        }

        public override int GetStringWidth()
        {
            return (int)Math.Ceiling(font.GetTextExtent(GetAllText()));
        }

        static TimingMeter updateTextTimer = MeterManager.GetMeter("Update Text", "LayeredStaticText");

        /// <summary>
        ///   This should be called whenever we update the text.  It 
        ///   recomputes the line wrapping, and notifies anyone else 
        ///   that has a callback registered.
        /// </summary>
        public override void UpdateText() {
            updateTextTimer.Enter();
            // Regenerate the text lines (based on the new text)
            GenerateTextLines();
            text = GetAllText();
            base.UpdateText();
            updateTextTimer.Exit();
        }
        /// <summary>
        ///   This should be called whenever we update the font.  It 
        ///   recomputes the line wrapping, and notifies anyone else 
        ///   that has a callback registered.
        /// </summary>
        protected override void UpdateFont() {
            // Regenerate the text lines (based on the new font)
            GenerateTextLines();
            base.UpdateFont();
        }

        /// <summary>
        ///  Gets the offset of the character at textIndex.  If textIndex
        ///  is outside of the bounds of the string, cap it to the string.
        /// </summary>
        /// <param name="textIndex"></param>
        /// <returns>absolute offset of the top left of the character at textIndex</returns>
        public PointF GetOffset(int textIndex) {
            Rect absRect = GetVisibleTextArea();

            if (textIndex > text.Length)
                textIndex = text.Length;
            else if (textIndex < 0)
                textIndex = 0;

            PointF drawPos = new PointF(absRect.Left, absRect.Top);
            PointF offset = font.GetOffset(text, lines, textIndex, horzFormat, vertFormat, absRect.Width, absRect.Height, true);
            drawPos.X += offset.X;
            drawPos.Y += offset.Y;

            return drawPos;
        }

        public override void SetFont(string fontFace, int fontHeight, string characterSet)
        {
            string fontName = string.Format("{0}-{1}", fontFace, fontHeight);
			if (!FontManager.Instance.ContainsKey(fontName))
                FontManager.Instance.CreateFont(fontName, fontFace, fontHeight, 0, characterSet);
            font = FontManager.Instance.GetFont(fontName);
        }


        /// <summary>
        ///   Finds the index of the character at a given point (capped to the range of the text)
        /// </summary>
        /// <param name="pos">the offset within the text area</param>
        /// <returns></returns>
        protected int GetTextIndexFromPosition(PointF pos) {
            Rect absRect = GetVisibleTextArea();
            // TODO: I need to handle scrollbar offsets.
            return font.GetTextIndexFromPosition(pos, text, lines, horzFormat, vertFormat, absRect.Width, absRect.Height, true);
        }

        protected override void DrawText(float z) {
            Rect absRect = GetVisibleTextArea();
            Rect clipRect = absRect.GetIntersection(this.PixelRect);
            SimpleFont textFont = this.Font;

            // textColors.SetAlpha(EffectiveAlpha);

            Vector3 drawPos = new Vector3();
            drawPos.x = absRect.Left;
            drawPos.y = absRect.Top;
            drawPos.z = z;

            // int debug_count = 0;
            foreach (TextChunk chunk in textChunks) {
                // find the intersection of chunks and lines
                foreach (TextRange line in lines) {
                    if (line.end <= chunk.range.start)
                        // this line comes before the chunk - skip it
                        continue;
                    else if (line.start >= chunk.range.end)
                        // this line comes after the chunk, so we must be done
                        // with the chunk.
                        break;
                    // some portion of this line is in this chunk
                    int start = line.start > chunk.range.start ? line.start : chunk.range.start;
                    int end = line.end > chunk.range.end ? chunk.range.end : line.end;
                    if (end <= start)
                        continue;
                    TextRange range = new TextRange(start, end);
                    PointF pt = font.GetOffset(text, lines, range.start, horzFormat, vertFormat, absRect.Width, absRect.Height, true);
                    drawPos.x = absRect.Left + pt.X;
                    drawPos.y = absRect.Top + pt.Y;
                    DrawText(text, start, end - start, drawPos, clipRect, chunk.style);
                    // debug_count++;
                }
            }
            // log.DebugFormat("Wrote {0} lines", debug_count);
        }

        private void DrawText(string text, int offset, int count, Vector3 drawPos, Rect clipRect, TextStyle style) {
            if (log.IsDebugEnabled)
                log.DebugFormat("Drawing '{0}' at [{1},{2}] clipped by {3}", text.Substring(offset, count), drawPos.x, drawPos.y, clipRect);
            float textZ = drawPos.z - (int)SubLevel.Normal * GuiZSubLevelStep;
            float shadowZ = drawPos.z - (int)SubLevel.Shadow * GuiZSubLevelStep;
            float bgZ = drawPos.z - (int)SubLevel.Background * GuiZSubLevelStep;
            SimpleFont textFont = this.Font;
            float x = textFont.GetTextExtent(text, offset, count);
            float y = textFont.LineSpacing;
            if (clipRect != null) {
                if (drawPos.x > clipRect.Right || drawPos.x + x < clipRect.Left ||
                    drawPos.y > clipRect.Bottom || drawPos.y + y < clipRect.Top) {
                    // this line is entirely out of bounds - technically, the drop shadow
                    // could extend outside this area (as could the font), but I think
                    // that if we wouldn't have drawn the background for the text, we don't
                    // need to draw the text.
                    log.DebugFormat("text with dimensions ({4},{5}) fully clipped by rect [{0},{1} - {2},{3}]", clipRect.Left, clipRect.Top, clipRect.Right, clipRect.Bottom, x, y);
                    return;
                }
            }
            // Draw these on integer boundaries
            drawPos.x = (int)drawPos.x;
            drawPos.y = (int)drawPos.y;
            drawPos.z = textZ;
            ColorRect colorRect;
            if (style.bgEnabled) {
                colorRect = new ColorRect(style.bgColor);
                colorRect.SetAlpha(this.EffectiveAlpha);
                Rect bgRect = new Rect(drawPos.x, drawPos.x + x,
                                       drawPos.y, drawPos.y + y);
                bgImage.Draw(bgRect, bgZ, clipRect, colorRect);
            }
            colorRect = new ColorRect(style.textColor);
            colorRect.SetAlpha(this.EffectiveAlpha);
            textFont.DrawTextLine(text, offset, count, drawPos, clipRect, colorRect);
            if (style.shadowEnabled) {
                drawPos.x += shadowOffset.X;
                drawPos.y += shadowOffset.Y;
                drawPos.z = shadowZ;
                colorRect = new ColorRect(style.shadowColor);
                colorRect.SetAlpha(this.EffectiveAlpha);
                textFont.DrawTextLine(text, offset, count, drawPos, clipRect, colorRect);
            }
        }

        protected void HandleTextChanged(object sender, EventArgs args)
        {
            UpdateScrollPosition(true);
        }

        protected internal override void OnTextChanged(EventArgs args)
        {
            text = GetAllText();
            GenerateTextLines(text);
            HandleTextChanged(this, args);
        }
        protected internal override void OnFontChanged(EventArgs args)
        {
            GenerateTextLines(text);
            HandleTextChanged(this, args);
        }
        protected internal override void OnAreaChanged(EventArgs args)
        {
            GenerateTextLines(text);
            HandleTextChanged(this, args);
        }
        protected internal override void OnFormatChanged(EventArgs args) {
            GenerateTextLines(text);
            HandleTextChanged(this, args);
        }

        /// <summary>
        ///   Updates the scrollbar's page size to be one page, and step size to be one line
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleFontChanged(object sender, EventArgs e) {
            vertDocumentSize = GetTextHeight(true);
        }

        protected void GenerateTextLines() {
            GenerateTextLines(GetAllText());
        }

        protected void GenerateTextLines(string str) {
            lines = font.GetLines(str, this.TextRenderArea.Width, this.HorizontalFormat, false);
            if (maxLines != 0 && lines.Count > maxLines)
                lines.RemoveRange(0, lines.Count - maxLines);
        }

        public override void ScrollUp()
        {
            SetVScrollPosition(vertScrollPosition - Font.LineSpacing);
        }

        public override void ScrollDown()
        {
            SetVScrollPosition(vertScrollPosition + Font.LineSpacing);
        }

        #region Properties
        //public SubLevel SubLevel
        //{
        //    get { return subLevel; }
        //    set { subLevel = value; }
        //}
        protected SimpleFont Font {
            get {
                return font;
            }
            set {
                if (font != value) {
                    Dirty = true;
                    font = value;
                }
            }
        }

        #endregion
    }
}
