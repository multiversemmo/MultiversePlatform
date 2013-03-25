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
	public partial class FontType
	{

		private object[] itemsField;

		private string nameField;

		private string inheritsField;

		private bool virtualField;

		private string fontField;

		private float spacingField;

		private OUTLINETYPE outlineField;

		private bool monochromeField;

		private JUSTIFYVTYPE justifyVField;

		private JUSTIFYHTYPE justifyHField;

		public FontType()
		{
			this.virtualField = false;
			this.spacingField = ((float)(0F));
			this.outlineField = OUTLINETYPE.NONE;
			this.monochromeField = false;
			this.justifyVField = JUSTIFYVTYPE.MIDDLE;
			this.justifyHField = JUSTIFYHTYPE.CENTER;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Color", typeof(ColorType))]
		[System.Xml.Serialization.XmlElementAttribute("FontHeight", typeof(Value))]
		[System.Xml.Serialization.XmlElementAttribute("Shadow", typeof(ShadowType))]
		public object[] Items
		{
			get
			{
				return this.itemsField;
			}
			set
			{
				this.itemsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string inherits
		{
			get
			{
				return this.inheritsField;
			}
			set
			{
				this.inheritsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Layout")]
		public bool @virtual
		{
			get
			{
				return this.virtualField;
			}
			set
			{
				this.virtualField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string font
		{
			get
			{
				return this.fontField;
			}
			set
			{
				this.fontField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(typeof(float), "0")]
		public float spacing
		{
			get
			{
				return this.spacingField;
			}
			set
			{
				this.spacingField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(OUTLINETYPE.NONE)]
		public OUTLINETYPE outline
		{
			get
			{
				return this.outlineField;
			}
			set
			{
				this.outlineField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		public bool monochrome
		{
			get
			{
				return this.monochromeField;
			}
			set
			{
				this.monochromeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(JUSTIFYVTYPE.MIDDLE)]
		public JUSTIFYVTYPE justifyV
		{
			get
			{
				return this.justifyVField;
			}
			set
			{
				this.justifyVField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(JUSTIFYHTYPE.CENTER)]
		public JUSTIFYHTYPE justifyH
		{
			get
			{
				return this.justifyHField;
			}
			set
			{
				this.justifyHField = value;
			}
		}
	}
}
