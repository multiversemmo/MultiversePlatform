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
using System.Drawing;
using System.Drawing.Text;
using System.Xml;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Input;

using FontFamily = System.Drawing.FontFamily;

namespace Multiverse.Gui
{
    // Later values should be in front of earlier values
    // This is the most significant ordering aspect
    public enum FrameStrata
    {
        Background,
        Low,
        Medium,
        High,
        Dialog,
        Fullscreen,
        FullscreenDialog,
        Tooltip,
        Unknown,
        Maximum, // Used for offsets
    }

    
    // Later values should be in front of earlier values
	public enum LayerLevel
	{
		Zero,       // this is not exposed through xml
		Background, // this is used for the actionbar
        Border,     // this is used for rested bar, inventory buttons, etc.
        Artwork,    // this is used for the endcaps - perhaps this should be default
        Overlay,    // text, buttons and objects should be here
        Highlight,  // button highlights, etc..
        Unknown,    // this is what i use when i don't know
	}


	public enum SubLevel
	{
		Background,
		Shadow,
		Normal,
		Caret,
	}

    public struct TextRange {
        public int start; // included
        public int end; // not included

        public TextRange(int start, int end) {
            this.start = start;
            this.end = end;
        }
        public int Length {
            get { return end - start; }
        }
    }

    public class TextChunk {
        public TextRange range;
        public TextStyle style;

        public TextChunk(TextRange range, TextStyle style) {
            this.range = range; 
            this.style = style;
        }
    }

	public class TextStyle {
        public ColorEx textColor = new ColorEx(1.0f, 1.0f, 1.0f);
		public bool shadowEnabled = false;
        public ColorEx shadowColor = new ColorEx(1.0f, 1.0f, 1.0f);
		public bool bgEnabled = false;
        public ColorEx bgColor = new ColorEx(1.0f, 1.0f, 1.0f);

		public TextStyle()
		{
		}
		public TextStyle(TextStyle other)
		{
			textColor = new ColorEx(other.textColor);
			bgColor = new ColorEx(other.bgColor);
			shadowColor = new ColorEx(other.shadowColor);
			shadowEnabled = other.shadowEnabled;
			bgEnabled = other.bgEnabled;
		}

		public override string ToString() {
			return string.Format("color {0}; bgColor {1}/{2}; shadowColor {3}/{4}",
								 textColor, bgColor, bgEnabled, shadowColor, shadowEnabled);
		}
		public override int GetHashCode() {
			throw new NotImplementedException();
		}

		public override bool Equals(object other) {
			if (!(other is TextStyle))
				return false;
			TextStyle otherStyle = other as TextStyle;
			if (shadowEnabled != otherStyle.shadowEnabled ||
				bgEnabled != otherStyle.bgEnabled)
                return false;
            if (textColor.CompareTo(otherStyle.textColor) != 0)
				return false;
			if (bgEnabled &&
				bgColor.CompareTo(otherStyle.bgColor) != 0)
				return false;
			if (shadowEnabled &&
				shadowColor.CompareTo(otherStyle.shadowColor) != 0)
				return false;
			return true;
		}
	}


    public class ColorRect {
        ColorEx[] colors = new ColorEx[4];

        public ColorRect() {
            for (int i = 0; i < colors.Length; ++i)
                colors[i] = ColorEx.White;
        }

        public static bool CompareTo(ColorRect c1, ColorRect c2) {
            if (c1 == null && c2 == null)
                return true;
            if (c1 == null || c2 == null)
                return false;
            for (int i=0; i<4; i++) {
                ColorEx color1 = c1.colors[i];
                ColorEx color2 = c2.colors[i];
                if (color1 == null && color2 == null)
                    continue;
                if (color1 == null || color2 == null)
                    return false;
                if (color1.CompareTo(color2) != 0)
                    return false;
            }
            return true;
        }
        
        public ColorRect(ColorEx color) {
            for (int i = 0; i < colors.Length; ++i)
                colors[i] = color;
        }

        public void SetAlpha(float alpha) {
            for (int i = 0; i < colors.Length; ++i)
                colors[i] = new ColorEx(alpha, colors[i].r, colors[i].g, colors[i].b);
        }

        public ColorEx TopLeft {
            get { return colors[0]; }
            set { colors[0] = value; }
        }
        public ColorEx TopRight {
            get { return colors[1]; }
            set { colors[1] = value; }
        }
        public ColorEx BottomLeft {
            get { return colors[2]; }
            set { colors[2] = value; }
        }
        public ColorEx BottomRight {
            get { return colors[3]; }
            set { colors[3] = value; }
        }

        public ColorRect Clone() {
            ColorRect rv = new ColorRect();
            rv.TopLeft = this.TopLeft;
            rv.TopRight = this.TopRight;
            rv.BottomLeft = this.BottomLeft;
            rv.BottomRight = this.BottomRight;
            return rv;
        }
    }

    /// <summary>
    ///   In this class, the top left corner of the screen would have coordinates of 0, 0
    /// </summary>
    public class Rect {
        float left;
        float right;
        float top;
        float bottom;

        public Rect(System.Drawing.RectangleF rectF)
            : this(rectF.Left, rectF.Right, rectF.Top, rectF.Bottom)
        {
        }
            
        public Rect(float left, float right, float top, float bottom) {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public Rect() {
        }

        public float Left {
            get { return left; }
            set { left = value; }
        }

        public float Right {
            get { return right; }
            set { right = value; }
        }

        public float Top {
            get { return top; }
            set { top = value; }
        }

        public float Bottom {
            get { return bottom; }
            set { bottom = value; }
        }

        public float Height {
            get {
                return bottom - top;
            }
            set {
                bottom = top + value;
            }
        }

        public float Width {
            get {
                return right - left;
            }
            set {
                right = left + value;
            }
        }

        /// <summary>
        ///   Get the intersection of two rectangles.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Rect GetIntersection(Rect other) {
            Rect rv = new Rect();
            rv.left = (left > other.left) ? left : other.left;
            rv.right = (right < other.right) ? right : other.right;
            rv.top = (top > other.top) ? top : other.top;
            rv.bottom = (bottom < other.bottom) ? bottom : other.bottom;
            if (rv.bottom < rv.top)
                rv.bottom = rv.top;
            if (rv.right < rv.left)
                rv.right = rv.left;
            return rv;
        }

        public SizeF Size {
            get { return new SizeF(right - left, bottom - top); }
        }

        public PointF Position {
            get { return new PointF(left, top); }
        }

        public System.Drawing.RectangleF ToRectangleF()
        {
            return System.Drawing.RectangleF.FromLTRB(left, top, right, bottom);
        }
    }

    public enum HorizontalTextFormat {
        Left,
        Centered,
        Right,
        WordWrapLeft,
        WordWrapCentered,
        WordWrapRight
    }

    public enum VerticalTextFormat {
        Top,
        Centered,
        Bottom
    }

    public enum HorizontalImageFormat {
        LeftAligned,
        Centered,
        Stretched,
        Tiled,
        RightAligned
    }

    public enum VerticalImageFormat {
        Top,
        Centered,
        Stretched,
        Tiled,
        Bottom
    }

    public class GenericEventArgs : EventArgs {
        public string eventType;
        public string[] eventArgs;
    }

    public class FloatEventArgs : EventArgs {
        public float data;
    }

    public class ExtendedEventArgs : EventArgs {
        public object[] data;
    }

    public class GuiEventArgs : EventArgs {
    }

    public class WindowEventArgs : EventArgs {
    }
    
    // Don't need this, since there's a reference in System
    // public delegate void EventHandler(object sender, EventArgs args);

    // Don't need these, since they are defined in Axiom.Input
    // public delegate void MouseEventHandler(object sender, MouseEventArgs args);
    // public delegate void KeyboardEventHandler(object sender, KeyEventArgs args);

    // This is really probably only used by the Multiverse.Interface code for
    // generic events.
    public delegate void GenericEventHandler(object sender, GenericEventArgs args);
    // This is used by the Multiverse.Interface code for events such as the OnTick or OnUpdate
    // It is also used for OnHorizontalScroll and OnVerticalScroll
    public delegate void FloatEventHandler(object sender, FloatEventArgs args);
    /// <summary>
    ///   This handler is used by the Multiverse.Interface code for the OnScrollRangeChanged
    ///   event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ExtendedEventHandler(object sender, ExtendedEventArgs args);
}
