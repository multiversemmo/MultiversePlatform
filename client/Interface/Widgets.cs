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
using System.Text;
using System.Drawing.Text;
using System.Xml;

using CrayzEdsGui.Base;
using CrayzEdsGui.Base.Widgets;
// using CrayzEdsGui.Renderers.AxiomEngine;

using FontFamily = System.Drawing.FontFamily;
using Vector3 = CrayzEdsGui.Base.Vector3;


namespace Multiverse.Interface
{
	public enum LayerLevel
	{
		//Unknown,
		//Border,
		//Artwork,
		//Background,
		//Overlay,
		Zero, // this is not exposed through xml
		Background, // this is used for the menubar
		Border,     // this is used for rested bar
		Artwork,    // this is used for the endcaps
		Overlay,
		Unknown,
	}

	public enum SubLevel
	{
		Background,
		Shadow,
		Normal,
		Caret,
	}

	/// <summary>
	///   Variant of StaticImage that allows me to control which widget
	///   is in front.
	/// </summary>
	public class LayeredStaticImage : StaticImage
	{
		const float GuiZLevelStep = 0.1f;
		// Since each window (from CrayzEdsGui.Base.Renderer.cs) advances the
		// z value by .001, we can support 10 elements at each LayerLevel.
		protected LayerLevel layerLevel = LayerLevel.Unknown;

		public LayeredStaticImage(string name)
			: base(name)
		{
		}

		protected override void DrawSelf(float z)
		{
			// Trace.TraceInformation("drawing {0} at {1} with level {2}", name, z - (int)layerLevel * GuiZLevelStep, layerLevel);
			base.DrawSelf(z - (int)layerLevel * GuiZLevelStep);
		}

		public LayerLevel Layer
		{
			get { return layerLevel; }
			set { layerLevel = value; }
		}
	}

	public class TextChunk
	{
		public string text;
		public TextStyle style;

		public TextChunk(string text, TextStyle style)
		{
			this.text = text;
			this.style = style;
		}
	}
	public class TextStyle {
		public Color textColor = new Color();
		public bool shadowEnabled = false;
		public Color shadowColor = new Color();
		public bool bgEnabled = false;
		public Color bgColor = new Color();

		public TextStyle()
		{
		}
		public TextStyle(TextStyle other)
		{
			textColor = other.textColor;
			bgColor = other.bgColor;
			shadowColor = other.shadowColor;
			shadowEnabled = other.shadowEnabled;
			bgEnabled = other.bgEnabled;
		}

		public override int GetHashCode() {
			throw new NotImplementedException();
		}

		public override bool Equals(object other) {
			if (!(other is TextStyle))
				return false;
			TextStyle otherStyle = other as TextStyle;
			if (shadowEnabled != otherStyle.shadowEnabled ||
				bgEnabled != otherStyle.bgEnabled ||
				textColor != otherStyle.textColor)
				return false;
			if (bgEnabled &&
				bgColor != otherStyle.bgColor)
				return false;
			if (shadowEnabled &&
				shadowColor != otherStyle.shadowColor)
				return false;
			return true;
		}
	}

	/// <summary>
	///   Variant of StaticText that allows me to control which widget
	///   is in front.
	/// </summary>
	public class LayeredStaticText : Static
	{
		// Fake scrollbar class so that I can create a static text widget
		class DummyScrollbar : Scrollbar
		{
			public DummyScrollbar(string name)
				: base(name)
			{
				position = 0.0f;
			}

			protected override PushButton CreateIncreaseButton()
			{
				return null;
			}
			protected override PushButton CreateDecreaseButton()
			{
				return null;
			}
			protected override Thumb CreateThumb()
			{
				return null;
			}
			protected override void LayoutComponentWidgets()
			{
			}
			protected override void DrawSelf(float z)
			{
			}
			protected override void UpdateThumb()
			{
			}
			protected override float GetPositionFromThumb()
			{
				return 0;
			}
			protected override float GetAdjustDirectionFromPoint(Point pt)
			{
				return 0;
			}
		}

		public const float GuiZLevelStep = 0.1f;
		public const float GuiZSubLevelStep = 0.01f;

		Scrollbar horzScrollbar;
		Scrollbar vertScrollbar;

		HorizontalTextFormat horzFormat = HorizontalTextFormat.Left;
		VerticalTextFormat vertFormat = VerticalTextFormat.Centered;

		bool scrollFromBottom = true;

		List<TextChunk> textChunks = new List<TextChunk>();
		TextStyle normalTextStyle = new TextStyle();
		bool nonSpaceWrap = true;
		Point shadowOffset;

		Image bgImage = null;

		// Since each window (from CrayzEdsGui.Base.Renderer.cs) advances the
		// z value by .001, we can support 100 elements at each LayerLevel.
		protected LayerLevel layerLevel = LayerLevel.Unknown;
		protected SubLevel subLevel = SubLevel.Normal;

		public LayeredStaticText(string name)
			: base(name)
		{
			horzScrollbar = CreateHorzScrollbar();
			vertScrollbar = CreateVertScrollbar();

			this.TextChanged += new WindowEventHandler(HandleTextChanged);
			this.FontChanged += new GuiEventHandler(HandleFontChanged);

			vertScrollbar.Visible = false;
			horzScrollbar.Visible = false;
			ConfigureScrollbars(0, 0);
		}

		public virtual void SetText(string str) {
			GenerateTextChunks(str);
		}

		protected string GetAllText() {
			StringBuilder allText = new StringBuilder();
			foreach (TextChunk chunk in textChunks)
				allText.Append(chunk.text);
			return allText.ToString();
		}

		/// <summary>
		///   How tall would it be if we drew all the text, but wrapped based on window width
		/// </summary>
		/// <returns></returns>
		protected float GetTextHeight()
		{
			List<TextRange> lines = GetTextLines();
			return lines.Count * this.Font.LineSpacing;
		}

		protected List<TextRange> GetTextLines() {
			Font textFont = this.Font;
			Rect absArea = this.TextRenderArea;
			string allTextString = GetAllText();
			List<TextRange> lines = 
				TextWrapHelper.GetLineBreaks(textFont, allTextString, absArea.Width, nonSpaceWrap);
			return lines;
		}

		protected float GetTextWidth() {
			Font textFont = this.Font;
			Rect absArea = this.TextRenderArea;
			string allTextString = GetAllText();
			float maxWidth = 0.0f;
			List<TextRange> lines = 
				TextWrapHelper.GetLineBreaks(textFont, allTextString, absArea.Width, nonSpaceWrap);
			foreach (TextRange range in lines) {
				float extent = font.GetTextExtent(allTextString.Substring(range.start, range.Length));
				maxWidth = (float)Math.Max(maxWidth, extent);
			}
			return maxWidth;
		}

		protected override void DrawSelf(float z)
		{
			// Draw the sub windows
			base.DrawSelf(z - (int)layerLevel * GuiZLevelStep);
			// Draw the text
			DrawText(z - (int)layerLevel * GuiZLevelStep);
		}

		/// <summary>
		///   This is a poorly named function, since it really returns a large
		///   rectangle that can hold all our text, offset so that the portion
		///   of the text that is visible is in the top left region.
		/// </summary>
		/// <returns></returns>
		protected Rect GetVisibleTextArea() {
			Rect absRect = this.TextRenderArea;
			Size dims = new Size(absRect.Width, absRect.Height);
			absRect.left -= horzScrollbar.ScrollPosition;
			absRect.Width = Math.Max(dims.width, horzScrollbar.DocumentSize);
			absRect.top -= vertScrollbar.ScrollPosition;
			absRect.Height = Math.Max(dims.height, vertScrollbar.DocumentSize);
 
			// see if we may need to adjust horizontal position
			//if (horzScrollbar.Visible) {
			//    switch (horzFormat) {
			//        case HorizontalTextFormat.Left:
			//        case HorizontalTextFormat.WordWrapLeft:
			//            absRect.Offset(new Point(-horzScrollbar.ScrollPosition, 0));
			//            break;

			//        case HorizontalTextFormat.Center:
			//        case HorizontalTextFormat.WordWrapCentered:
			//            absRect.Width = horzScrollbar.DocumentSize;
			//            absRect.Offset(new Point(-horzScrollbar.ScrollPosition, 0));
			//            break;

			//        case HorizontalTextFormat.Right:
			//        case HorizontalTextFormat.WordWrapRight:
			//            absRect.Offset(new Point(horzScrollbar.ScrollPosition, 0));
			//            break;
			//    }
			//}
			// adjust y positioning according to formatting options
			//switch (vertFormat) {
			//    case VerticalTextFormat.Top:
			//        break;
			//    case VerticalTextFormat.Centered:
			//        absRect.top -= vertScrollbar.ScrollPosition;
			//        break;
			//    case VerticalTextFormat.Bottom:
			//        absRect.top -= vertScrollbar.ScrollPosition;
			//        break;
			//}

			return absRect;
		}

		public Point GetOffset(int textIndex) {
			Rect absRect = GetVisibleTextArea();
			Font textFont = this.Font;

			// textColors.SetAlpha(EffectiveAlpha);
			string allText = GetAllText();
			List<TextRange> lines =
				TextWrapHelper.GetLineBreaks(textFont, allText, absRect.Width, nonSpaceWrap);

			Point drawPos;
			drawPos.x = absRect.left;
			drawPos.y = absRect.top;

			switch (vertFormat) {
				case VerticalTextFormat.Top:
					drawPos.y += 0.0f; // start at the top
					break;
				case VerticalTextFormat.Bottom:
					drawPos.y += absRect.Height - lines.Count * textFont.LineSpacing;
					break;
				case VerticalTextFormat.Centered:
					drawPos.y += (absRect.Height - lines.Count * textFont.LineSpacing) / 2;
					break;
			}


			for (int i = 0; i < lines.Count; ++i) {
				TextRange range = lines[i];
				if (range.end <= textIndex && (i < (lines.Count - 1))) {
					drawPos.y += font.LineSpacing;
					continue;
				}
				string line = allText.Substring(range.start, range.Length);
				switch (horzFormat) {
					case HorizontalTextFormat.Left:
					case HorizontalTextFormat.WordWrapLeft:
						drawPos.x += 0.0f; // start at the left
						break;
					case HorizontalTextFormat.Right:
					case HorizontalTextFormat.WordWrapRight:
						drawPos.x += absRect.Width - textFont.GetTextExtent(line);
						break;
					case HorizontalTextFormat.Center:
					case HorizontalTextFormat.WordWrapCentered:
						drawPos.x += (absRect.Width - textFont.GetTextExtent(line)) / 2;
						break;
				}
				if (range.start > textIndex)
					return drawPos;
				// some of the text precedes the offset
				string portion = line.Substring(0, textIndex - range.start);
				drawPos.x += textFont.GetTextExtent(portion);
			}
			return drawPos;
		}

		protected void DrawText(float z) {
			Rect absRect = GetVisibleTextArea();
			Rect clipRect = absRect.GetIntersection(this.PixelRect);
			Font textFont = this.Font;

			// textColors.SetAlpha(EffectiveAlpha);
			string allText = GetAllText();
			List<TextRange> lines = 
				TextWrapHelper.GetLineBreaks(textFont, allText, absRect.Width, nonSpaceWrap);

			Vector3 drawPos = new Vector3();
			drawPos.x = absRect.left;
			drawPos.y = absRect.top;
			drawPos.z = z;

			switch (vertFormat) {
				case VerticalTextFormat.Top:
					drawPos.y += 0.0f; // start at the top
					break;
				case VerticalTextFormat.Bottom:
					drawPos.y += absRect.Height - lines.Count * textFont.LineSpacing;
					break;
				case VerticalTextFormat.Centered:
					drawPos.y += (absRect.Height - lines.Count * textFont.LineSpacing) / 2;
					break;
			}

			List<TextChunk>.Enumerator chunkEnum = textChunks.GetEnumerator();
			// the offset of the last character that we have drawn
			int lastChar = 0;
			// the offset of the first character in the chunk we are working with
			int chunkStart = 0;
			// the offset of the last character in the chunk we are working with
			int chunkEnd = 0;
			// offset into the active chunk of the first undrawn character
			int startOffset;
			string portion;
			TextChunk chunk = chunkEnum.Current;

			foreach (TextRange range in lines) {
				drawPos.x = absRect.left;
				string line = allText.Substring(range.start, range.Length);
				switch (horzFormat) {
					case HorizontalTextFormat.Left:
					case HorizontalTextFormat.WordWrapLeft:
						drawPos.x += 0.0f; // start at the left
						break;
					case HorizontalTextFormat.Right:
					case HorizontalTextFormat.WordWrapRight:
						drawPos.x += absRect.Width - textFont.GetTextExtent(line);
						break;
					case HorizontalTextFormat.Center:
					case HorizontalTextFormat.WordWrapCentered:
						drawPos.x += (absRect.Width - textFont.GetTextExtent(line)) / 2;
						break;
				}
				// while the rest of the current chunk fits completely on this line
				while (chunkEnd <= range.end) {
					// offset into this chunk of the first undrawn character
					startOffset = lastChar - chunkStart;
					// do we have anything to process in this chunk?
					if (chunkEnd - lastChar > 0) {
						portion = chunk.text.Substring(startOffset, chunkEnd - lastChar);
						DrawText(portion, drawPos, clipRect, chunk.style);
						drawPos.x += textFont.GetTextExtent(portion);
						lastChar = chunkEnd;
					}

					if (!chunkEnum.MoveNext())
						// we have finished processing the last chunk
						return;
					chunk = chunkEnum.Current;
					chunkStart = chunkEnd;
					chunkEnd += chunk.text.Length;
				}
				// at this point, chunk end > range end, so the whole chunk
				// does not belong on this line
				startOffset = lastChar - chunkStart;
				portion = chunk.text.Substring(startOffset, range.end - lastChar);

				DrawText(portion, drawPos, clipRect, chunk.style);
				drawPos.x += textFont.GetTextExtent(portion);
				lastChar = range.end;

				drawPos.y += font.LineSpacing;
			}
		}

		private void DrawText(string portion, Vector3 drawPos, Rect clipRect, TextStyle style) {
			// Trace.TraceInformation("Drawing '{0}' at [{1},{2}] clipped by {3}", portion, drawPos.x, drawPos.y, clipRect);
			float textZ = drawPos.z - (int)SubLevel.Normal * GuiZSubLevelStep;
			float shadowZ = drawPos.z - (int)SubLevel.Shadow * GuiZSubLevelStep;
			float bgZ = drawPos.z - (int)SubLevel.Background * GuiZSubLevelStep;
			Font textFont = this.Font;
			// Draw these on integer boundaries
			drawPos.x = (int)drawPos.x;
			drawPos.y = (int)drawPos.y;
			drawPos.z = textZ;
			ColorRect colors;
			if (style.bgEnabled) {
				float x = textFont.GetTextExtent(portion);
				float y = textFont.LineSpacing;
				colors = new ColorRect(style.bgColor);
				Rect bgRect = new Rect(drawPos.x, drawPos.y, 
									   drawPos.x + x, drawPos.y + y);
				bgImage.Draw(bgRect, bgZ, clipRect, colors);
			}
			colors = new ColorRect(style.textColor);
			textFont.DrawTextLine(portion, drawPos, clipRect, colors);
			if (style.shadowEnabled) {
				drawPos.x += shadowOffset.x;
				drawPos.y += shadowOffset.y;
				drawPos.z = shadowZ;
				colors = new ColorRect(style.shadowColor);
				textFont.DrawTextLine(portion, drawPos, clipRect, colors);
			}
		}

		protected Scrollbar CreateHorzScrollbar()
		{
			return new DummyScrollbar("_auto" + name + "HorzScrollbar");
		}
		protected Scrollbar CreateVertScrollbar()
		{
			return new DummyScrollbar("_auto" + name + "VertScrollbar");
		}

		public virtual void HandleFontChanged() {
			this.OnFontChanged(null);
		}

		public virtual void HandleTextChanged() {
			this.OnTextChanged(null);
		}

		/// <summary>
		///   Updates the scrollbar's page size to be one page, and step size to be one line
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandleFontChanged(object sender, GuiEventArgs e)
		{
			vertScrollbar.PageSize = TextRenderArea.Height / this.Font.LineSpacing;
			vertScrollbar.StepSize = this.Font.LineSpacing;
			vertScrollbar.DocumentSize = GetTextHeight();
		}
		/// <summary>
		///   Updates the scrollbar's document size to include all the lines, 
		///   and updates the scroll position so that the last line will fit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandleTextChanged(object sender, WindowEventArgs e)
		{
			// set our text to the text of the editbox
			bool atBottom = false;
			if (vertScrollbar.ScrollPosition == vertScrollbar.DocumentSize)
				atBottom = true;

			float textHeight = GetTextHeight();
			float textWidth = GetTextWidth();
			// Update the scrollbars to take the new text into account
			ConfigureScrollbars(textWidth, textHeight);

			if (scrollFromBottom)
				vertScrollbar.ScrollPosition = textHeight - TextRenderArea.Height;
			// if we were at the bottom, and we need to scroll, scroll to the bottom
			if (atBottom &&
				vertScrollbar.DocumentSize > TextRenderArea.Height)
				vertScrollbar.ScrollPosition = textHeight - TextRenderArea.Height;
		}

		protected virtual void GenerateTextChunks() {
			GenerateTextChunks(GetAllText());
		}

		protected virtual void GenerateTextChunks(string str) {
			List<TextChunk> chunks = this.TextChunks;
			chunks.Clear();
			chunks.Add(new TextChunk(str, new TextStyle(this.NormalTextStyle)));
		}

		protected void ConfigureScrollbars(float textWidth, float textHeight) {
			Font textFont = this.Font;
			Rect renderArea = TextRenderArea;

			vertScrollbar.DocumentSize = textHeight;
			horzScrollbar.DocumentSize = textWidth;
		}

		//protected void ConfigureScrollbars() {
		//    ConfigureScrollbars(GetTextHeight(), GetTextWidth());
		//}		
	
		public void SetVScrollPosition(float scrollPosition)
		{
			float textHeight = GetTextHeight();
			Rect textRect = this.TextRenderArea;
			if (scrollPosition > textHeight - textRect.Height)
				scrollPosition = textHeight - textRect.Height;
			else if (scrollPosition < 0)
				scrollPosition = 0;
			vertScrollbar.ScrollPosition = scrollPosition;
		}

		public void ScrollUp()
		{
			float scrollPosition = vertScrollbar.ScrollPosition;
			SetVScrollPosition(scrollPosition - font.LineSpacing);
		}

		public void ScrollDown() {
			float scrollPosition = vertScrollbar.ScrollPosition;
			SetVScrollPosition(scrollPosition + font.LineSpacing);
		}

		public void ScrollToTop() {
			float scrollPosition = vertScrollbar.ScrollPosition;
			SetVScrollPosition(0);
		}

		public void ScrollToBottom() {
			float scrollPosition = vertScrollbar.ScrollPosition;
			SetVScrollPosition(GetTextHeight());
		}

		#region Properties

		public bool ScrollFromBottom
		{
			get { return scrollFromBottom; }
			set { scrollFromBottom = value; }
		}

		public Scrollbar VertScrollbar
		{
			get	{ return vertScrollbar; }
		}
		public Scrollbar HorzScrollbar
		{
			get	{ return horzScrollbar;	}
		}
		public new Image BackgroundImage
		{
			set { bgImage = value; }
		}
		public LayerLevel Layer
		{
			get { return layerLevel; }
			set { layerLevel = value; }
		}
		public virtual TextStyle NormalTextStyle {
			get { return normalTextStyle; }
			set {
				normalTextStyle = value;
				GenerateTextChunks();
			}
		}
		//public SubLevel SubLevel
		//{
		//    get { return subLevel; }
		//    set { subLevel = value; }
		//}

		public Point ShadowOffset
		{
			get { return shadowOffset; }
			set { shadowOffset = value; }
		}
		public List<TextChunk> TextChunks
		{
			get { return textChunks; }
		}
		/// <summary>
		///   Return a Rect object describing, in un-clipped pixels, the window
		///   relative area that the text should be rendered in to.
		/// </summary>
		/// <value></value>
		public Rect TextRenderArea
		{
			get	{
				Rect area = this.UnclippedInnerRect;

				if (horzScrollbar.Visible)
					area.bottom -= horzScrollbar.AbsoluteHeight;

				if (vertScrollbar.Visible)
					area.right -= vertScrollbar.AbsoluteWidth;

				return area;
			}
		}

		public HorizontalTextFormat HorizontalFormat
		{
			get	{ return horzFormat; }
			set	{ horzFormat = value; }
		}

		public VerticalTextFormat VerticalFormat
		{
			get	{ return vertFormat; }
			set	{ vertFormat = value; }
		}

		#endregion
	}

	/// <summary>
	///   Variant of EditBox that allows me to control which widget
	///   is in front.
	/// </summary>
	public class LayeredEditBox : LayeredStaticText
	{
		protected bool textMasked = false;
		protected char maskChar = '*';
		protected float lastTextOffset = 0;
		protected string editText = string.Empty;

		bool readOnly = false;
		int selectionStartIndex = 0;
		int selectionEndIndex = 0;
		TextStyle selectedTextStyle;
		
		protected int caretIndex = 0;
		// caret (cursor)
		protected Image caret;

		public LayeredEditBox(string name)
			: base(name)
		{
			selectedTextStyle = new TextStyle(this.NormalTextStyle);
			selectedTextStyle.bgColor.a = 0.5f;
			selectedTextStyle.bgColor.r = 0.5f;
			selectedTextStyle.bgColor.g = 0.5f;
			selectedTextStyle.bgColor.b = 0.5f;
		}

		public override void SetText(string str) {
			this.EditText = str;
		}

		/// <summary>
		///   This finds the point in the text corresponding to the point in 
		///   window space.
		/// </summary>
		/// <param name="point">the point in window space</param>s
		/// <returns>the index into the text string for that point</returns>
		protected int GetTextIndexFromPosition(Point point)
		{
			// TODO: This currently only handles one line, but could easily 
			//       be extended to handle more
			// calculate final window position to be checked
			float wndx = ScreenToWindowX(point.x);

			if (this.MetricsMode == MetricsMode.Relative)
				wndx = RelativeToAbsoluteX(wndx);
			// wndx is now the pixel offset of the point into this window.
			wndx -= lastTextOffset;
			// wndx is now the pixel offset of the point into the font string
			// return the proper index
			if (textMasked)
				// The characters are all drawn using the mask character, so use that instead of text
				return font.GetCharAtPixel(new string(maskChar, text.Length), 0, wndx);
			else
				// the characters are drawn normally, so we can use the text object.
				return font.GetCharAtPixel(text, 0, wndx);
		}

		protected override void GenerateTextChunks() {
			GenerateTextChunks(editText);
		}

		protected override void GenerateTextChunks(string str) {
			List<TextChunk> chunks = this.TextChunks;
			chunks.Clear();
			// The portion before the highlight section
			if (selectionStartIndex > 0) {
				int len = selectionStartIndex;
				string chunkText;
				if (this.TextMasked)
					chunkText = new string(this.MaskChar, len);
				else
					chunkText = str.Substring(0, len);
				TextChunk chunk = new TextChunk(chunkText, new TextStyle(this.NormalTextStyle));
				chunks.Add(chunk);
			}

			if (selectionEndIndex > selectionStartIndex) {
				int len = selectionEndIndex - selectionStartIndex;
				string chunkText;
				if (this.TextMasked)
					chunkText = new string(this.MaskChar, len);
				else
					chunkText = str.Substring(selectionStartIndex, len);
				TextChunk chunk = new TextChunk(chunkText, new TextStyle(this.SelectedTextStyle));
				chunk.style = this.SelectedTextStyle;
				chunks.Add(chunk);
			}

			if (selectionEndIndex <= str.Length) {
				int len = str.Length - selectionEndIndex;
				string chunkText;
				if (this.TextMasked)
					chunkText = new string(this.MaskChar, len);
				else
					chunkText = str.Substring(selectionEndIndex, len);
				TextChunk chunk = new TextChunk(chunkText, new TextStyle(this.NormalTextStyle));
				chunks.Add(chunk);
			}
		}

		public override void HandleTextChanged() {
			GenerateTextChunks();
			base.HandleTextChanged();
		}

		protected override void DrawSelf(float z) {
			base.DrawSelf(z);
			Rect clipRect = this.PixelRect;
			// Draw the caret
			string allText = GetAllText();
			if (caretIndex > allText.Length)
				caretIndex = allText.Length;
			Point pt = GetOffset(caretIndex);
			Vector3 drawPos = new Vector3(pt.x, pt.y, z);
			drawPos.z -= (int)layerLevel * GuiZLevelStep;
			drawPos.z -= (int)SubLevel.Caret * GuiZSubLevelStep;
			if (drawPos.x < clipRect.left)
				drawPos.x = clipRect.left;
			else if (drawPos.x + caret.Width > clipRect.right)
				drawPos.x = clipRect.right - caret.Width;
			Size caretSize = new Size(caret.Width, font.LineSpacing);
			ColorRect caretColorRect = new ColorRect(new Color(1, 1, 1));
			caret.Draw(drawPos, caretSize, clipRect, caretColorRect);
		}


#if NOT_ME
		protected void DrawSelf(float z) {
		}
			z -= (int)layerLevel * GuiZLevelStep;

			float caretZ = z - (int)SubLevel.Caret * GuiZSubLevelStep;

			float highlightZ = z - (int)SubLevel.Background * GuiZSubLevelStep;
			float textZ = z - (int)SubLevel.Normal * GuiZSubLevelStep;

			Rect clipper = PixelRect;

			// do nothing if the widget is totally clipped
			if (clipper.Width == 0)
				return;

			Renderer renderer = GuiSystem.Instance.Renderer;
			bool hasFocus = this.IsActive;

			// get the destination screen rect for this window
			Rect absRect = this.UnclippedPixelRect;

			// Required preliminary work for main rendering operations
			//
			// Create a 'masked' version of the string if needed.
			string editText;

			if (textMasked)
				editText = new string(maskChar, text.Length);
			else
				editText = text;

			// calculate best position to render text to ensure carat is always visible
			float textOffset;
			float extentToCaret = font.GetTextExtent(editText.Substring(0, caretIndex));

			// if box is inactive
			if (!hasFocus)
				textOffset = lastTextOffset;
			// if carat is to the left of the box
			else if ((lastTextOffset + extentToCaret) < 0)
				textOffset = -extentToCaret;
			// if carat is off to the right.
			else if ((lastTextOffset + extentToCaret) >= (absRect.Width - caret.Width))
				textOffset = absRect.Width - extentToCaret - caret.Width;
			// else carat is already within the box
			else
				textOffset = lastTextOffset;

			// adjust clipper for new target area
			clipper = absRect.GetIntersection(clipper);

			// render carat
			if ((!this.ReadOnly) && hasFocus)
			{
				Vector3 pos = new Vector3(absRect.left + textOffset + extentToCaret, absRect.top, caretZ);
				Size sz = new Size(caret.Width, absRect.Height);
				caret.Draw(pos, sz, clipper, colors);
			}


				// setup colors
				if (hasFocus && !ReadOnly)
					colors = new ColorRect(selectBrushColor);
				else
					colors = new ColorRect(inactiveSelectBrushColor);
				// colors.SetAlpha(this.EffectiveAlpha);
				// render the highlight
				selection.Draw(hlarea, highlightZ, clipper, colors);
			}

			lastTextOffset = textOffset;
		}
#endif
		#region Overridden Event Trigger Methods

		protected internal override void OnMouseButtonDown(MouseEventArgs e) {
			// base class handling
			base.OnMouseButtonDown(e);

			if(e.Button == MouseButton.Left) {
				// grab inputs
				CaptureInput();

				// handle mouse down
				ClearSelection();
				dragging = true;
				dragAnchorIdx = GetTextIndexFromPosition(e.Position);
				CaratIndex = dragAnchorIdx;

				e.Handled = true;
			}
		}

		protected internal override void OnMouseButtonUp(MouseEventArgs e) {
			// base class processing
			base.OnMouseButtonUp(e);

			if(e.Button == MouseButton.Left) {
				ReleaseInput();

				e.Handled = true;
			}
		}

		protected internal override void OnMouseDoubleClicked(MouseEventArgs e) {
			// base class processing
			base.OnMouseDoubleClicked(e);

			if(e.Button == MouseButton.Left) {
				// if masked, set up to select all
				if(TextMasked) {
					dragAnchorIdx = 0;
					CaratIndex = text.Length;
				}
				else {
					// not masked, so select the word that was double clicked
					dragAnchorIdx = TextUtil.GetWordStartIdx(text, (caratPos == text.Length) ? caratPos : caratPos + 1);
					caratPos = TextUtil.GetNextWordStartIdx(text, caratPos);
				}

				// perform actual selection operation
				SetSelection(dragAnchorIdx, caratPos);

				e.Handled = true;
			}
		}

		protected internal override void OnMouseTripleClicked(MouseEventArgs e) {
			// base class processing
			base.OnMouseTripleClicked(e);

			if(e.Button == MouseButton.Left) {
				dragAnchorIdx = 0;
				CaratIndex = text.Length;
				SetSelection(dragAnchorIdx, caratPos);
				e.Handled = true;
			}
		}

		protected internal override void OnMouseMove(MouseEventArgs e) {
			// base class processing
			base.OnMouseMove(e);

			if(dragging) {
				CaratIndex = GetTextIndexFromPosition(e.Position);
				SetSelection(caratPos, dragAnchorIdx);
			}

			e.Handled = true;
		}

		protected internal override void OnCaptureLost(GuiEventArgs e) {
			dragging = false;

			// base class processing
			base.OnCaptureLost(e);

			e.Handled = true;
		}

		protected internal override void OnCharacter(KeyEventArgs e) {
			// base class processing
			base.OnCharacter(e);

			// only need to take notice if we have focus
			if(HasInputFocus && this.Font.IsCharacterAvailable(e.Character) && !ReadOnly) {
				// backup current text
				string tmp = text;

				tmp = tmp.Remove(SelectionStartIndex, SelectionLength);

				// if there is room
				if(tmp.Length < maxTextLength) {
					tmp = tmp.Insert(SelectionStartIndex, e.Character.ToString());

					if(IsStringValid(tmp)) {
						// erase selection using mode that does not modify 'text' (we just want to update state)
						EraseSelectedText(false);

						// set text to the newly modified string
						Text = tmp;

						// advance carat
						caratPos++;
					}
					else {
						// trigger invalid modification attempted event
						OnInvalidEntryAttempted(new WindowEventArgs(this));
					}
				}
				else {
					// trigger text box full event
					OnEditboxFull(new WindowEventArgs(this));
				}
			}
			PostCharacter(this, e);
			e.Handled = true;
		}

		protected internal override void OnKeyDown(KeyEventArgs e) {
			// base class processing
			base.OnKeyDown(e);

			if(HasInputFocus && !ReadOnly) {
				WindowEventArgs args = new WindowEventArgs(this);

				switch(e.KeyCode) {
					case KeyCodes.LeftShift:
					case KeyCodes.RightShift:
						if(SelectionLength == 0) {
							dragAnchorIdx = CaratIndex;
						}
						break;

					case KeyCodes.Backspace:
						HandleBackspace();
						break;

					case KeyCodes.Delete:
						HandleDelete();
						break;

					case KeyCodes.Tab:
					case KeyCodes.Return:
					case KeyCodes.Enter:
						// fire input accepted event
						OnTextAccepted(args);
						break;

					case KeyCodes.Left:
						if((e.Modifiers & SystemKey.Control) > 0) {
							HandleWordLeft(e.Modifiers);
						}
						else {
							HandleCharLeft(e.Modifiers);
						}
						break;

					case KeyCodes.Right:
						if((e.Modifiers & SystemKey.Control) > 0) {
							HandleWordRight(e.Modifiers);
						}
						else {
							HandleCharRight(e.Modifiers);
						}
						break;

					case KeyCodes.Home:
						HandleHome(e.Modifiers);
						break;

					case KeyCodes.End:
						HandleEnd(e.Modifiers);
						break;
				} // switch

				e.Handled = true;
			}
		}

		protected internal override void OnTextChanged(WindowEventArgs e) {
			// base class processing
			base.OnTextChanged(e);

			// clear selection
			ClearSelection();

			// make sure carat is within the text
			if(CaratIndex > text.Length) {
				CaratIndex = text.Length;
			}

			e.Handled = true;
		}

		#endregion Overridden Event Trigger Methods


		public bool TextMasked
		{
			get { return textMasked; }
			set {
				textMasked = value;
				GenerateTextChunks();
			}
		}
		public char MaskChar
		{
			get { return maskChar; }
			set {
				maskChar = value;
				GenerateTextChunks();
			}
		}
		public bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}
		public int SelectionStartIndex {
			get { return selectionStartIndex; }
			set {
				selectionStartIndex = value;
				GenerateTextChunks();
			}
		}
		public int SelectionEndIndex {
			get { return selectionEndIndex; }
			set {
				selectionEndIndex = value;
				GenerateTextChunks();
			}
		}
		public int SelectionLength {
			get { return selectionEndIndex - selectionStartIndex; }
		}
		public TextStyle SelectedTextStyle {
			get { return selectedTextStyle; }
			set { 
				selectedTextStyle = value;
				GenerateTextChunks();
			}
		}
		public string EditText {
			get { return editText; }
			set {
				editText = value;
				if (selectionStartIndex > editText.Length)
					selectionStartIndex = editText.Length;
				if (selectionEndIndex > editText.Length)
					selectionEndIndex = editText.Length;
				HandleTextChanged();
			}
		}
		public int CaretIndex {
			get {
				return caretIndex;
			}
			set {
				caretIndex = value;
			}
		}
		public Image Caret {
			get {
				return caret;
			}
			set {
				caret = value;
			}
		}
	}

	public struct TextRange {
		public int start; // included
		public int end; // not included
		public int Length {
			get { return end - start; }
		}
	}

	public class TextWrapHelper {
		public static bool IsNewline(char c) {
			return c == '\n' || c == '\r';
		}
		public static bool IsWhitespace(char c) {
			return c == ' ' || c == '\t';
		}

		public static List<TextRange> GetLineBreaks(Font font, string text, float width, bool nonSpaceWrap)
		{
			List<TextRange> lines = new List<TextRange>();

			TextRange range = new TextRange();
			int cursor = 0;
			
			range.start = cursor;
			range.end = cursor;
			while (cursor < text.Length) {
				if (IsNewline(text[cursor])) {
					range.end = cursor;
					lines.Add(range);
					cursor++;
					range.start = cursor;
					range.end = cursor;
				} else if (!IsWhitespace(text[cursor]) &&
						   (font.GetTextExtent(text.Substring(range.start, range.Length)) > width)) {
					bool broke = false;
					for (int tmp = cursor; tmp >= range.start; --tmp)
						if (IsWhitespace(text[tmp])) {
							cursor = tmp + 1;
							range.end = cursor; // first character after the whitespace
							lines.Add(range);
							range.start = cursor;
							range.end = cursor;
							broke = true;
							break;
						}
					if (!broke) {
						if (nonSpaceWrap) {
							// we would have to back out the whole line, so just break mid-word.
							range.end = cursor;
							lines.Add(range);
						} else {
							// include the rest of the word
							while (!IsNewline(text[cursor]) && !IsWhitespace(text[cursor]) && cursor < text.Length)
								cursor++;
							if (IsNewline(text[cursor])) {
								range.end = cursor;
								lines.Add(range);
								cursor++;
							} else if (IsWhitespace(text[cursor])) {
								while (IsWhitespace(text[cursor])) {
									range.end = cursor;
									cursor++;
								}
								range.end = cursor;
								lines.Add(range);
								if (IsNewline(text[cursor]))
									cursor++;
							}
						}
						range.start = cursor;
						range.end = cursor;
					}
				} else {
					cursor++;
					range.end = cursor;
				}
			}
			lines.Add(range);
			return lines;
		}
	}
}
