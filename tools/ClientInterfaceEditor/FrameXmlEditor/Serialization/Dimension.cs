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
using System.ComponentModel;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{

	/// Generated from Ui.xsd into two pieces and merged here.
	/// Manually modified later - DO NOT REGENERATE

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "3.5.20706.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.multiverse.net/ui")]
	public partial class Dimension
	{
		[TypeConverter(typeof(DimensionConverter<Dimension.Size>))]
		[Serializable]
		public class Size : Dimension { }

		[TypeConverter(typeof(DimensionConverter<Dimension.maxResize>))]
		[Serializable]
		public class maxResize : Dimension { }

		[TypeConverter(typeof(DimensionConverter<Dimension.minResize>))]
		[Serializable]
		public class minResize : Dimension { }

		[TypeConverter(typeof(DimensionConverter<Dimension.PushedTextOffset>))]
		[Serializable]
		public class PushedTextOffset : Dimension { }

		private object itemField;

		private int xField;

		private bool xFieldSpecified;

		private int yField;

		private bool yFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("AbsDimension", typeof(AbsDimension))]
		[System.Xml.Serialization.XmlElementAttribute("RelDimension", typeof(RelDimension))]
		public object Item
		{
			get
			{
				return this.itemField;
			}
			set
			{
				this.itemField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int x
		{
			get
			{
				return this.xField;
			}
			set
			{
				this.xField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool xSpecified
		{
			get
			{
				return this.xFieldSpecified;
			}
			set
			{
				this.xFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int y
		{
			get
			{
				return this.yField;
			}
			set
			{
				this.yField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ySpecified
		{
			get
			{
				return this.yFieldSpecified;
			}
			set
			{
				this.yFieldSpecified = value;
			}
		}
	}
}
