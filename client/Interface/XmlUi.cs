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

#region Using directives

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

using Axiom.MathLib;
using Axiom.Core;

using Multiverse.Gui;

#endregion

namespace Multiverse.Interface
{
	public class XmlUi {

		public static ColorEx ReadColor(XmlNode node) {
			ColorEx rv = new ColorEx();
			rv.a = 1.0f;
			rv.r = float.Parse(node.Attributes["r"].Value);
			rv.g = float.Parse(node.Attributes["g"].Value);
			rv.b = float.Parse(node.Attributes["b"].Value);
			if (node.Attributes["a"] != null)
				rv.a = float.Parse(node.Attributes["a"].Value);
			return rv;
		}

		public static float ReadRelValue(XmlNode node) {
			return float.Parse(node.Attributes["val"].Value);
		}
		public static int ReadAbsValue(XmlNode node) {
			return int.Parse(node.Attributes["val"].Value);
		}
		public static float ReadValue(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "AbsValue":
						return (float)ReadAbsValue(childNode);
					case "RelValue":
						return ReadRelValue(childNode);
					default:
						break;
				}
			}
			Debug.Assert(false, "Got Value node without AbsValue or RelValue child");
			throw new Exception("Got Value node without AbsValue or RelValue child");
		}

		private static SizeF ReadSize(XmlNode node) {
			SizeF rv = new SizeF();
			rv.Width = float.Parse(node.Attributes["x"].Value);
			rv.Height = float.Parse(node.Attributes["y"].Value);
			return rv;
		}

		public static SizeF ReadAbsDimension(XmlNode node) {
			// These are int type, but I treat as float
			return ReadSize(node);
		}
		public static SizeF ReadRelDimension(XmlNode node) {
			return ReadSize(node);
		}
		public static SizeF ReadDimension(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "AbsDimension":
						return ReadAbsDimension(childNode);
					case "RelDimension":
						return ReadRelDimension(childNode);
					default:
						break;
				}
			}
			Debug.Assert(false, "Got Dimension node without AbsDimension or RelDimension child");
			throw new Exception("Got Dimension node without AbsDimension or RelDimension child");
		}


		private static Rect ReadRect(XmlNode node) {
			Rect rv = new Rect();
			rv.Left = float.Parse(node.Attributes["left"].Value);
			rv.Right = float.Parse(node.Attributes["right"].Value);
			rv.Top = float.Parse(node.Attributes["top"].Value);
			rv.Bottom = float.Parse(node.Attributes["bottom"].Value);
			return rv;
		}

		public static PointF[] ReadTexCoords(XmlNode node) {
            Rect rect = ReadRect(node);
            PointF[] texCoords = new PointF[4];
            texCoords[0].X = rect.Left;
            texCoords[0].Y = rect.Top;
            texCoords[1].X = rect.Right;
            texCoords[1].Y = rect.Top;
            texCoords[2].X = rect.Left;
            texCoords[2].Y = rect.Bottom;
            texCoords[3].X = rect.Right;
            texCoords[3].Y = rect.Bottom;
            return texCoords;
		}
		public static Rect ReadAbsInset(XmlNode node) {
			// These are really int type, but I just use floats for now
			return ReadRect(node);
		}
		public static Rect ReadRelInset(XmlNode node) {
			return ReadRect(node);
		}
		public static Rect ReadInset(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "AbsInset":
						return ReadAbsInset(childNode);
					case "RelInset":
						return ReadRelInset(childNode);
					default:
						break;
				}
			}
			Debug.Assert(false, "Got Inset node without AbsInset or RelInset child");
			throw new Exception("Got Inset node without AbsInset or RelInset child");
		}

		public static ColorRect ReadGradient(XmlNode node) {
			ColorEx minColor = new ColorEx();
			ColorEx maxColor = new ColorEx();
			bool minColorSet = false;
			bool maxColorSet = false;
			bool horizontal = true;
            if (node.Attributes["orientation"] != null &&
			    node.Attributes["orientation"].Value == "VERTICAL")
				horizontal = false;
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "MinColor":
						minColor = ReadColor(childNode);
						minColorSet = true;
						break;
					case "MaxColor":
						maxColor = ReadColor(childNode);
						maxColorSet = true;
						break;
					default:
						break;
				}
			}
			Debug.Assert(minColorSet && maxColorSet,
						 "Got Gradient node without MinColor and MaxColor child");
			ColorRect rv = new ColorRect();
			if (horizontal) {
				rv.TopLeft = minColor;
				rv.TopRight = maxColor;
				rv.BottomLeft = minColor;
				rv.BottomRight = maxColor;
			} else {
				rv.TopLeft = minColor;
				rv.TopRight = minColor;
				rv.BottomLeft = maxColor;
				rv.BottomRight = maxColor;
			}
			return rv;
		}
	}
}
