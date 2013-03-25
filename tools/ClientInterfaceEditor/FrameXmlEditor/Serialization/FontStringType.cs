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

using System.ComponentModel;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing.Design;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	/// Generated from Ui.xsd into two pieces and merged here.
	/// Manually modified later - DO NOT REGENERATE

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "3.5.20706.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.multiverse.net/ui")]
	[System.Xml.Serialization.XmlRootAttribute("FontString", Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class FontStringType : LayoutFrameType
	{
		public FontStringType()
		{
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[TypeConverter(typeof(FontTypeConverter))]
		[Category("Appearance")]
		public string font
		{
			get
			{
				return this.Properties.GetValue<string>("font");
			}
			set
			{
				this.Properties["font"] = value;
			}
		}

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(255)]
        //[Category("Appearance")]
        //public int bytes
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<int>("bytes");
        //    }
        //    set
        //    {
        //        this.Properties["bytes"] = value;
        //    }
        //}

		/// <remarks/>
		[XmlAttribute]
		[Browsable(false)]
		public string text
		{
			get
			{
				return this.Properties.GetValue<string>("text");
			}
			set
			{
				this.Properties["text"] = value;
			}
		}

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(0f)]
        //[Category("Appearance")]
        //public float spacing
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<float>("spacing");
        //    }
        //    set
        //    {
        //        this.Properties["spacing"] = value;
        //    }
        //}

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(OUTLINETYPE.NONE)]
        //[Category("Appearance")]
        //public OUTLINETYPE outline
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<OUTLINETYPE>("outline");
        //    }
        //    set
        //    {
        //        this.Properties["outline"] = value;
        //    }
        //}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Appearance")]
		public bool monochrome
		{
			get
			{
				return this.Properties.GetValue<bool>("monochrome");
			}
			set
			{
				this.Properties["monochrome"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Behavior")]
		public bool nonspacewrap
		{
			get
			{
				return this.Properties.GetValue<bool>("nonspacewrap");
			}
			set
			{
				this.Properties["nonspacewrap"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(JUSTIFYVTYPE.MIDDLE)]
		[Category("Appearance")]
		public JUSTIFYVTYPE justifyV
		{
			get
			{
				return this.Properties.GetValue<JUSTIFYVTYPE>("justifyV");
			}
			set
			{
				this.Properties["justifyV"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(JUSTIFYHTYPE.CENTER)]
		[Category("Appearance")]
		public JUSTIFYHTYPE justifyH
		{
			get
			{
				return this.Properties.GetValue<JUSTIFYHTYPE>("justifyH");
			}
			set
			{
				this.Properties["justifyH"] = value;
			}
		}

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(typeof(uint), "0")]
        //[Category("Appearance")]
        //public uint maxLines
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<uint>("maxLines");
        //    }
        //    set
        //    {
        //        this.Properties["maxLines"] = value;
        //    }
        //}
	}
}
