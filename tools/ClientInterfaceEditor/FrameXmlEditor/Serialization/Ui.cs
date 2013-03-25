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
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.multiverse.net/ui")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class Ui : SerializationObject
	{
		private List<UiScript> uiScripts = new List<UiScript>();
		protected override List<UiScript> GetUiScripts()
		{
			return uiScripts;
		}

		[XmlElement("Button", typeof(ButtonType))]
		[XmlElement("CheckButton", typeof(CheckButtonType))]
		[XmlElement("ColorSelect", typeof(ColorSelectType))]
		[XmlElement("EditBox", typeof(EditBoxType))]
		[XmlElement("Frame", typeof(FrameType))]
		[XmlElement("ScrollFrame", typeof(ScrollFrameType))]
		[XmlElement("ScrollingMessageFrame", typeof(ScrollingMessageFrameType))]
		[XmlElement("StatusBar", typeof(StatusBarType))]
		[XmlElement("Browser", typeof(BrowserType))]
		//[XmlElement("GameTooltip", typeof(GameTooltipType))]
		//[XmlElement("Model", typeof(ModelType))]
		[XmlElement("FontString", typeof(FontStringType))]
		[XmlElement("Texture", typeof(TextureType))]
		[XmlElement("LayoutFrame", typeof(LayoutFrameType))]
		//[XmlElement("MessageFrame", typeof(MessageFrameType))]
		[XmlElement("Slider", typeof(SliderType))]
		[Browsable(false)]
		public List<SerializationObject> ControlsXML
		{
			get { return base.Controls; }
		}
	}
}
