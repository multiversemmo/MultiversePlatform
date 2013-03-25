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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Drawing;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	public partial class ColorType
	{
		public override string ToString()
		{
			return String.Format("{0}; {1}; {2}; {3}", r, g, b, a);
		}

		public static ColorType FromString(string colorString)
		{
			if (String.IsNullOrEmpty(colorString))
				return null;

			colorString = colorString.Replace(" ", "");
			if (String.IsNullOrEmpty(colorString))
				return null;

			ColorType colorType = new ColorType();

			Regex regEx = new Regex(@"^(\d+(\.\d+)?);(\d+(\.\d+)?);(\d+(\.\d+)?)(;(\d+(\.\d+)?))?$");
			Match match = regEx.Match(colorString);
			if (!match.Success)
			{
				regEx = new Regex(@"^(\w+)(;(\d+(.\d+)?))?$");
				match = regEx.Match(colorString);
				if (!match.Success)
					throw new ArgumentException("Use one of the following formats: 'r;g;b;a' or 'ColorName;a'.");

				string colorName = match.Groups[1].Value;
				KnownColor knownColor;
				try
				{
					knownColor = (KnownColor)Enum.Parse(typeof(KnownColor), colorName, true);
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException("Color name is not known: " + ex.Message, ex);
				}

				Color color = Color.FromKnownColor(knownColor);

				colorType = FromColor(color);
				if (match.Groups[3].Success)
					colorType.a = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
				return colorType;
			}

			colorType.r = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
			colorType.g = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
			colorType.b = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
			if (match.Groups[8].Success)
				colorType.a = float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
			else
				colorType.a = 1;

			return colorType;
		}

		private static int ColorFloatToByte(float value)
		{
			return (int)Math.Round(value * 255);
		}

		private static float ColorByteToFloat(byte value)
		{
			return (float)value / 255;
		}

		public Color ToColor()
		{
			Color color = Color.FromArgb(
				ColorFloatToByte(a),
				ColorFloatToByte(r),
				ColorFloatToByte(g),
				ColorFloatToByte(b)
				);
			return color;
		}

		public static Color ToColor(ColorType colorType)
		{
			if (colorType == null)
				return Color.Empty;

			return colorType.ToColor();
		}

		public static ColorType FromColor(Color color)
		{
			if (color.IsEmpty)
				return null;

			ColorType colorType = new ColorType();
			colorType.r = ColorByteToFloat(color.R);
			colorType.g = ColorByteToFloat(color.G);
			colorType.b = ColorByteToFloat(color.B);
			colorType.a = ColorByteToFloat(color.A);

			return colorType;
		}

		#region equals

		//// override object.Equals
		//public override bool Equals(object obj)
		//{
		//    if (obj == null || GetType() != obj.GetType())
		//    {
		//        return false;
		//    }

		//    ColorType other = obj as ColorType;
		//    return
		//        this.a == other.a &&
		//        this.r == other.r &&
		//        this.g == other.g &&
		//        this.b == other.b;
		//}

		//// override object.GetHashCode
		//public override int GetHashCode()
		//{
		//    return 
		//        a.GetHashCode() ^ 
		//        r.GetHashCode() ^ 
		//        g.GetHashCode() ^ 
		//        b.GetHashCode();
		//}

		#endregion
	}
}
