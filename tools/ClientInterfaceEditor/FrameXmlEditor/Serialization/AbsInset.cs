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
	public partial class AbsInset
	{

		private int leftField;

		private bool leftFieldSpecified;

		private int rightField;

		private bool rightFieldSpecified;

		private int topField;

		private bool topFieldSpecified;

		private int bottomField;

		private bool bottomFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int left
		{
			get
			{
				return this.leftField;
			}
			set
			{
				this.leftField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool leftSpecified
		{
			get
			{
				return this.leftFieldSpecified;
			}
			set
			{
				this.leftFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int right
		{
			get
			{
				return this.rightField;
			}
			set
			{
				this.rightField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool rightSpecified
		{
			get
			{
				return this.rightFieldSpecified;
			}
			set
			{
				this.rightFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int top
		{
			get
			{
				return this.topField;
			}
			set
			{
				this.topField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool topSpecified
		{
			get
			{
				return this.topFieldSpecified;
			}
			set
			{
				this.topFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int bottom
		{
			get
			{
				return this.bottomField;
			}
			set
			{
				this.bottomField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool bottomSpecified
		{
			get
			{
				return this.bottomFieldSpecified;
			}
			set
			{
				this.bottomFieldSpecified = value;
			}
		}
	}
}
