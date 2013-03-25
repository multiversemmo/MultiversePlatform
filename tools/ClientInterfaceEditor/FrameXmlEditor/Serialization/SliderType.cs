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
	[System.Xml.Serialization.XmlRootAttribute("Slider", Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class SliderType : FrameType
	{
		public SliderType()
		{
			this.drawLayer = DRAWLAYER.OVERLAY;
			this.orientation = ORIENTATION.VERTICAL;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(DRAWLAYER.OVERLAY)]
		[Category("Appearance")]
		public DRAWLAYER drawLayer
		{
			get
			{
				return this.Properties.GetValue<DRAWLAYER>("drawLayer");
			}
			set
			{
				this.Properties["drawLayer"] = value;
			}
		}

		/// <remarks/>
		[XmlAttribute]
		[Browsable(false)]
		public float minValue
		{
			get
			{
				return this.MinValue.Value;
			}
			set
			{
				this.MinValue = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[Browsable(false)]
		public bool minValueSpecified
		{
			get
			{
				return this.MinValue.HasValue;
			}
			set
			{
				if (value)
					this.MinValue = this.MinValue.Value;
				else
					this.MinValue = null;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[Browsable(false)]
		public float maxValue
		{
			get
			{
				return this.MaxValue.Value;
			}
			set
			{
				this.MaxValue = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[Browsable(false)]
		public bool maxValueSpecified
		{
			get
			{
				return this.MaxValue.HasValue;
			}
			set
			{
				if (value)
					this.MaxValue = this.MaxValue.Value;
				else
					this.MaxValue = null;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[Browsable(false)]
		public float defaultValue
		{
			get
			{
				return this.DefaultValue.Value;
			}
			set
			{
				this.DefaultValue = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[Browsable(false)]
		public bool defaultValueSpecified
		{
			get
			{
				return this.DefaultValue.HasValue;
			}
			set
			{
				if (value)
					this.DefaultValue = this.DefaultValue.Value;
				else
					this.DefaultValue = null;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[Browsable(false)]
		public float valueStep
		{
			get
			{
				return this.ValueStep.Value;
			}
			set
			{
				this.ValueStep = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[Browsable(false)]
		public bool valueStepSpecified
		{
			get
			{
				return this.ValueStep.HasValue;
			}
			set
			{
				if (value)
					this.ValueStep = this.ValueStep.Value;
				else
					this.ValueStep = null;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(ORIENTATION.VERTICAL)]
		[Category("Appearance")]
		public ORIENTATION orientation
		{
			get
			{
				return this.Properties.GetValue<ORIENTATION>("orientation");
			}
			set
			{
				this.Properties["orientation"] = value;
			}
		}
	}
}
