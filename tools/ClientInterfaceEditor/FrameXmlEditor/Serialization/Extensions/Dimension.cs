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
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	public partial class Dimension
	{
		/// <summary>
		/// Returns the textual representation of a dimension object.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the Dimension object.
		/// </returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (this.xSpecified)
			{
				sb.Append(this.x);
			}

			if (this.xSpecified || this.ySpecified)
			{
				sb.Append(';');
			}
			if (this.ySpecified)
			{
				sb.Append(this.y);
			}

			if (sb.Length > 0)
				sb.Append(' ');

			AbsDimension absDimension = this.Item as AbsDimension;
			if (absDimension != null)
			{
				sb.Append(String.Format("Abs {0};{1}", absDimension.x, absDimension.y));
			}

			RelDimension relDimension = this.Item as RelDimension;
			if (relDimension != null)
			{
				sb.Append(String.Format(CultureInfo.InvariantCulture, "Rel {0};{1}", relDimension.x, relDimension.y));
			}

			return
				sb.ToString();
		}

		/// <summary>
		/// Creates a new Dimension of a given type from the string argument.
		/// </summary>
		/// <typeparam name="TDimension">The type of the dimension.</typeparam>
		/// <param name="textValue">The text value.</param>
		/// <returns>The dimension object</returns>
		public static TDimension FromString<TDimension>(string textValue)
			where TDimension: Dimension, new()
		{
			TDimension dimension = new TDimension();

			if (String.IsNullOrEmpty(textValue))
				return null;

			textValue = textValue.Replace(" ", "");
			if (textValue.Length == 0)
				return dimension;

			Regex regEx = new Regex(@"(([-+]?\d+)?;([-+]?\d+)?)?((Rel|Abs)([-+]?\d+(.\d+)?);([-+]?\d+(.\d+)?))?");
			Match match = regEx.Match(textValue);
			if (!match.Success)
				throw new ArgumentException("Please use the following BNF format: {{<x>};{<y>}} {(Rel|Abs}<x>;<y>}");

			if (!match.Groups[1].Success && !match.Groups[4].Success)
				throw new ArgumentException("Either x;y attributes or Abs/Rel x;y items should be completed!");

			// optional x attribute
			if (match.Groups[2].Success)
			{
				dimension.x = Int32.Parse(match.Groups[2].Value);
				dimension.xSpecified = true;
			}

			// optional y attribute
			if (match.Groups[3].Success)
			{
				dimension.y = Int32.Parse(match.Groups[3].Value);
				dimension.ySpecified = true;
			}

			// dimension modifier (abs or rel) and both values are specified
			if (match.Groups[5].Success && match.Groups[6].Success && match.Groups[8].Success)
			{
				switch (match.Groups[5].Value)
				{
					case "Rel":
						RelDimension relDimension = new RelDimension();
						relDimension.x = float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
						relDimension.y = float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture);
						dimension.Item = relDimension;
						break;
					case "Abs":
                    default:
						// only if there are no decimals (absolute coordinates are integers)
						if (match.Groups[7].Success || match.Groups[9].Success)
							throw new ArgumentException("Absolute coordinates should be integers!");

						AbsDimension absDimension = new AbsDimension();
						absDimension.x = Int32.Parse(match.Groups[6].Value);
						absDimension.y = Int32.Parse(match.Groups[8].Value);
                        dimension.Item = absDimension;
                        break;
				}
			}
			return dimension;
		}

		/// <summary>
		/// Extension method of Dimension. Retrieves the size in pixels.
		/// </summary>
		/// <param name="dimension">The dimension.</param>
		/// <returns>Size in pixels</returns>
		public System.Drawing.Size GetSize()
		{
			var size = System.Drawing.Size.Empty;
			if (this.xSpecified)
				size.Width = this.x;
			if (this.ySpecified)
				size.Height = this.y;

			AbsDimension absDimension = this.Item as AbsDimension;
			if (absDimension != null)
				size = new System.Drawing.Size(absDimension.x, absDimension.y);

			RelDimension relDimension = this.Item as RelDimension;
			if (relDimension != null)
			{
				var screenSize = Screen.PrimaryScreen.Bounds.Size;
				size = new System.Drawing.Size(
					(int)(screenSize.Width * relDimension.x),
					(int)(screenSize.Height * relDimension.y));
			}

			return
				size;
		}

		public void Update(int? x, int? y)
		{
			if (this.Item == null)
			{
				if (x.HasValue)
				{
					this.x = x.Value;
					this.xSpecified = true;
				}
				if (y.HasValue)
				{
					this.y = y.Value;
					this.yFieldSpecified = true;
				}
			}

			AbsDimension absDimension = this.Item as AbsDimension;
			if (absDimension != null)
			{
				if (x.HasValue)
					absDimension.x = x.Value;
				if (y.HasValue)
					absDimension.y = y.Value;
			}

			RelDimension relDimension = this.Item as RelDimension;
			if (relDimension != null)
			{
				var screenSize = Screen.PrimaryScreen.Bounds.Size;
				if (x.HasValue)
					relDimension.x = (float) x.Value / screenSize.Width;
				if (y.HasValue)
					relDimension.y = (float)y.Value / screenSize.Height;
			}
            if (absDimension == null && relDimension == null)
            {
                this.Item = new AbsDimension();
                (this.Item as AbsDimension).x = x.Value;
                (this.Item as AbsDimension).y = y.Value;
            }
		}

        public static TDimension FromSize<TDimension>(System.Drawing.Size size)
            where TDimension: Dimension, new()
        {
            TDimension dimension = new TDimension();
            dimension.Update(size.Width, size.Height);
            return
                dimension;
        }

		#region ICloneable Members

		public static TDimension Clone<TDimension>(TDimension original)
			where TDimension: Dimension
		{
			if (original == null)
				return null;

			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream())
			{
				bf.Serialize(stream, original);
				stream.Position = 0;
				return bf.Deserialize(stream) as TDimension;
			}
		}

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			return this.ToString().Equals(obj.ToString());
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public static bool Equals(Dimension one, Dimension two)
		{
			if (one == null)
				return two == null;

			return one.Equals(two);
		}

		#endregion
	}
}
