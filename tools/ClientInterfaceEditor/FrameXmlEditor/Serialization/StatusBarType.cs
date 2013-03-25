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
using System;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	/// Generated from Ui.xsd into two pieces and merged here.
	/// Manually modified later - DO NOT REGENERATE

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "3.5.20706.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.multiverse.net/ui")]
	[System.Xml.Serialization.XmlRootAttribute("StatusBar", Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class StatusBarType : FrameType
	{
		public StatusBarType()
		{
		}

		/// <remarks/>
		[XmlAttribute]
		[Category("Behavior")]
		[System.ComponentModel.DefaultValueAttribute(DRAWLAYER.ARTWORK)]
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

		[XmlIgnore]
		[Category("Behavior")]
		[DisplayName("MinValue")]
		public float? MinValueNullable
		{
			get
			{
				return this.Properties.GetValue<float?>("MinValueNullable");
			}
			set
			{
                this.Properties["MinValueNullable"] = value;
			}
		}

		/// <remarks/>
		[XmlAttribute]
		[Browsable(false)]
		public float minValue
		{
			get
			{
				return MinValueNullable.Value;
			}
			set
			{
				this.MinValueNullable = value;
			}
		}

		/// <remarks/>
		[XmlIgnore]
		[Browsable(false)]
		public bool minValueSpecified
		{
			get
			{
				return MinValueNullable.HasValue;
			}
			set
			{
				if (value)
					MinValueNullable = minValue;
				else
					MinValueNullable = null;
			}
		}

		[XmlIgnore]
		[Category("Behavior")]
		[DisplayName("MaxValue")]
		public float? MaxValueNullable
		{
			get
			{
                return this.Properties.GetValue<float?>("MaxValueNullable");
			}
			set
			{
                this.Properties["MaxValueNullable"] = value;
			}
		}

		/// <remarks/>
		[XmlAttribute]
		[Browsable(false)]
		public float maxValue
		{
			get
			{
				return this.MaxValueNullable.Value;
			}
			set
			{
				this.MaxValueNullable = value;
			}
		}

		/// <remarks/>
		[XmlIgnore]
		[Browsable(false)]
		public bool maxValueSpecified
		{
			get
			{
				return MaxValueNullable.HasValue;
			}
			set
			{
				if (value)
					MaxValueNullable = maxValue;
				else
					MaxValueNullable = null;
			}
		}

		[XmlIgnore]
		[Category("Behavior")]
		[DisplayName("DefaultValue")]
		public float? DefaultValueNullable
		{
			get
			{
                return this.Properties.GetValue<float?>("DefaultValueNullable");
			}
			set
			{
                this.Properties["DefaultValueNullable"] = value;
			}
		}

		/// <remarks/>
		[XmlAttribute]
		[Browsable(false)]
		public float defaultValue
		{
			get
			{
				return DefaultValueNullable.Value;
			}
			set
			{
				DefaultValueNullable = value;
			}
		}

		/// <remarks/>
		[XmlIgnore]
		[Browsable(false)]
		public bool defaultValueSpecified
		{
			get
			{
				return DefaultValueNullable.HasValue;
			}
			set
			{
				if (value)
					DefaultValueNullable = defaultValue;
				else
					DefaultValueNullable = null;
			}
		}

		/// <remarks/>
		[XmlAttribute]
		[System.ComponentModel.DefaultValueAttribute(ORIENTATION.HORIZONTAL)]
		[Category("Behavior")]
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

		[XmlElement("BarColor", typeof(ColorType))]
		[Browsable(false)]
		public ColorType[] BarColors
		{
			get { return this.BarColor.AsColorTypeArray(); }
			set { this.BarColor = value.AsColor(); }
		}

	}
}
