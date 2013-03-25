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
using System.ComponentModel;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	public partial class ScriptsType
	{
		private SerializationMap<EventChoice, string> events = new SerializationMap<EventChoice, string>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<EventChoice, string> Events
		{
			get { return this.events; }
		}

		/// <summary>
		/// Items of type string with different element names specified in EventNames.
		/// </summary>
        //[XmlElement("OnAnimFinished", typeof(string))]
        //[XmlElement("OnAttributeChanged", typeof(string))]
		[XmlElement("OnChar", typeof(string))]
        //[XmlElement("OnCharComposition", typeof(string))]
		[XmlElement("OnClick", typeof(string))]
        //[XmlElement("OnColorSelect", typeof(string))]
        //[XmlElement("OnCursorChanged", typeof(string))]
		[XmlElement("OnDoubleClick", typeof(string))]
		[XmlElement("OnDragStart", typeof(string))]
		[XmlElement("OnDragStop", typeof(string))]
        //[XmlElement("OnEditFocusGained", typeof(string))]
        //[XmlElement("OnEditFocusLost", typeof(string))]
        [XmlElement("OnEnter", typeof(string))]
        //[XmlElement("OnEnterPressed", typeof(string))]
        //[XmlElement("OnEscapePressed", typeof(string))]
        [XmlElement("OnEvent", typeof(string))]
		[XmlElement("OnHide", typeof(string))]
		[XmlElement("OnHorizontalScroll", typeof(string))]
        //[XmlElement("OnHyperlinkClick", typeof(string))]
        //[XmlElement("OnHyperlinkEnter", typeof(string))]
        //[XmlElement("OnHyperlinkLeave", typeof(string))]
        //[XmlElement("OnInputLanguageChanged", typeof(string))]
		[XmlElement("OnKeyDown", typeof(string))]
		[XmlElement("OnKeyUp", typeof(string))]
		[XmlElement("OnLeave", typeof(string))]
		[XmlElement("OnLoad", typeof(string))]
        //[XmlElement("OnMessageScrollChanged", typeof(string))]
		[XmlElement("OnMouseDown", typeof(string))]
		[XmlElement("OnMouseUp", typeof(string))]
		[XmlElement("OnMouseWheel", typeof(string))]
        //[XmlElement("OnMovieFinished", typeof(string))]
        //[XmlElement("OnMovieHideSubtitle", typeof(string))]
        //[XmlElement("OnMovieShowSubtitle", typeof(string))]
		[XmlElement("OnReceiveDrag", typeof(string))]
		[XmlElement("OnScrollRangeChanged", typeof(string))]
        [XmlElement("OnShow", typeof(string))]
		[XmlElement("OnSizeChanged", typeof(string))]
        //[XmlElement("OnSpacePressed", typeof(string))]
        //[XmlElement("OnTabPressed", typeof(string))]
        //[XmlElement("OnTextChanged", typeof(string))]
        //[XmlElement("OnTextSet", typeof(string))]
        //[XmlElement("OnTooltipAddMoney", typeof(string))]
        //[XmlElement("OnTooltipCleared", typeof(string))]
        //[XmlElement("OnTooltipSetDefaultAnchor", typeof(string))]
        //[XmlElement("OnTooltipSetItem", typeof(string))]
        //[XmlElement("OnTooltipSetSpell", typeof(string))]
        //[XmlElement("OnTooltipSetUnit", typeof(string))]
		[XmlElement("OnUpdate", typeof(string))]
        //[XmlElement("OnUpdateModel", typeof(string))]
		[XmlElement("OnValueChanged", typeof(string))]
		[XmlElement("OnVerticalScroll", typeof(string))]
        //[XmlElement("PostClick", typeof(string))]
        //[XmlElement("PreClick", typeof(string))]
		[XmlChoiceIdentifier(MemberName = "EventNames")]
		[Browsable(false)]
		public string[] EventsArray
		{
			get
            {
                return this.events.ValuesArray;
            }
			set
            {
                this.events.ValuesArray = value;
            }
		}

		/// <summary>
		/// Element names for items of type string.
		/// </summary>
		[XmlElement("EventNames")]
		[XmlIgnore]
		[Browsable(false)]
		public EventChoice[] EventNames
		{
			get { return this.events.KeysArray; }
			set { this.events.KeysArray = value; }
		}

	}
}
