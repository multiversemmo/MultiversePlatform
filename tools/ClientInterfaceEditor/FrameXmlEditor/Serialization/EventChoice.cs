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
using System.Xml.Serialization;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	/// <summary>
	/// Element name choices for Button events.
	/// </summary>
	[Serializable]
	[XmlType(Namespace = XmlSettings.MultiverseNamespace, IncludeInSchema = false)]
	public enum EventChoice
	{
        ///// <remarks/>
        //OnAnimFinished,

        ///// <remarks/>
        //OnAttributeChanged,

		/// <remarks/>
		OnChar,

        ///// <remarks/>
        //OnCharComposition,

		/// <remarks/>
		OnClick,

        ///// <remarks/>
        //OnColorSelect,

        ///// <remarks/>
        //OnCursorChanged,

		/// <remarks/>
		OnDoubleClick,

		/// <remarks/>
		OnDragStart,

		/// <remarks/>
		OnDragStop,

        ///// <remarks/>
        //OnEditFocusGained,

        ///// <remarks/>
        //OnEditFocusLost,

		/// <remarks/>
		OnEnter,

        ///// <remarks/>
        //OnEnterPressed,

        ///// <remarks/>
        //OnEscapePressed,

		/// <remarks/>
		OnEvent,

		/// <remarks/>
		OnHide,

		/// <remarks/>
		OnHorizontalScroll,

        ///// <remarks/>
        //OnHyperlinkClick,

        ///// <remarks/>
        //OnHyperlinkEnter,

        ///// <remarks/>
        //OnHyperlinkLeave,

        ///// <remarks/>
        //OnInputLanguageChanged,

		/// <remarks/>
		OnKeyDown,

		/// <remarks/>
		OnKeyUp,

		/// <remarks/>
		OnLeave,

		/// <remarks/>
		OnLoad,

        ///// <remarks/>
        //OnMessageScrollChanged,

        /// <remarks/>
        OnMouseDown,

        /// <remarks/>
        OnMouseUp,

        /// <remarks/>
        OnMouseWheel,

        ///// <remarks/>
        //OnMovieFinished,

        ///// <remarks/>
        //OnMovieHideSubtitle,

        ///// <remarks/>
        //OnMovieShowSubtitle,

        ///// <remarks/>
        OnReceiveDrag,

		/// <remarks/>
		OnScrollRangeChanged,

		/// <remarks/>
		OnShow,

		/// <remarks/>
		OnSizeChanged,

        ///// <remarks/>
        //OnSpacePressed,

        /////// <remarks/>
        ////OnTabPressed,

        /////// <remarks/>
        ////OnTextChanged,

        /////// <remarks/>
        ////OnTextSet,

        /////// <remarks/>
        ////OnTooltipAddMoney,

        ///// <remarks/>
        //OnTooltipCleared,

        ///// <remarks/>
        //OnTooltipSetDefaultAnchor,

        ///// <remarks/>
        //OnTooltipSetItem,

        ///// <remarks/>
        //OnTooltipSetSpell,

        ///// <remarks/>
        //OnTooltipSetUnit,

        /// <remarks/>
        OnUpdate,

        ///// <remarks/>
        //OnUpdateModel,

		/// <remarks/>
		OnValueChanged,

		/// <remarks/>
		OnVerticalScroll

        ///// <remarks/>
        //PostClick,

        ///// <remarks/>
        //PreClick

	}
	
}
