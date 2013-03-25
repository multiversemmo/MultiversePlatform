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
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing.Design;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	/// Generated from Ui.xsd into two pieces and merged here.
	/// Manually modified later - DO NOT REGENERATE

	/// <remarks/>
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(UnitButtonType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CheckButtonType))]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "3.5.20706.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.multiverse.net/ui")]
	[System.Xml.Serialization.XmlRootAttribute("Button", Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class ButtonType : FrameType
	{

		/// <remarks/>
		[XmlAttribute]
		[Category("Appearance")]
		// [Browsable(false)]
		public string text
		{
			get
			{
				return Properties.GetValue<string>("text");
			}
			set
			{
				this.Properties["text"] = value;
			}
		}

		/// <summary>
		/// Items of type Dimension with different element names.
		/// </summary>
		[XmlElement("PushedTextOffset", typeof(Dimension.PushedTextOffset))]
		[Browsable(false)]
		public Dimension.PushedTextOffset[] PushedTextOffsets
		{
			get { return this.Properties.GetArray<Dimension.PushedTextOffset>("PushedTextOffset"); }
			set { this.Properties.SetArray<Dimension.PushedTextOffset>("PushedTextOffset", value); }
		}

		[XmlElement("DisabledColor", typeof(ColorType))]
		[Browsable(false)]
		public ColorType[] DisabledColors
		{
			get { return this.DisabledColor.AsColorTypeArray(); }
			set { this.DisabledColor = value.AsColor(); }
		}

		[XmlElement("HighlightColor", typeof(ColorType))]
		[Browsable(false)]
		public ColorType[] HighlightColors
		{
			get { return this.HighlightColor.AsColorTypeArray(); }
			set { this.HighlightColor = value.AsColor(); }
		}

		[XmlElement("NormalColor", typeof(ColorType))]
		[Browsable(false)]
		public ColorType[] NormalColors
		{
			get { return this.NormalColor.AsColorTypeArray(); }
			set { this.NormalColor = value.AsColor(); }
		}


	}
}
