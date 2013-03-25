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
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;

using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;


namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	/// Generated from Ui.xsd into two pieces and merged here.
	/// Manually modified later - DO NOT REGENERATE

	/// <remarks/>
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FrameType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(TaxiRouteFrameType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(MinimapType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(CooldownType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(GameTooltipType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(WorldFrameType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(MovieFrameType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ScrollFrameType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ScrollingMessageFrameType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageFrameType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrowserType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(ModelType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(PlayerModelType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(TabardModelType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(DressUpModelType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ColorSelectType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EditBoxType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(SliderType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(StatusBarType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ButtonType))]
	//[System.Xml.Serialization.XmlIncludeAttribute(typeof(UnitButtonType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CheckButtonType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(FontStringType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(TextureType))]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "3.5.20706.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.multiverse.net/ui")]
	[System.Xml.Serialization.XmlRootAttribute("LayoutFrame", Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class LayoutFrameType : SerializationObject
	{
		private string inheritsField;

		private bool virtualField;

		public LayoutFrameType()
		{
			this.virtualField = false;

			this.Properties = new PropertyBag(this);
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[Browsable(false)]
		public string name 
		{ 
			get; 
			set; 
		}

		/// <remarks/>
		[XmlAttribute]
		[TypeConverter(typeof(InheritsTypeConverter))]
		[Browsable(false)]
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
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Behavior")]
		public bool setAllPoints
		{
			get
			{
				return this.Properties.GetValue<bool>("setAllPoints");
			}
			set
			{
				this.Properties["setAllPoints"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Layout")]
		public bool hidden
		{
			get
			{
				return this.Properties.GetValue<bool>("hidden");
			}
			set
			{
				this.Properties["hidden"] = value;
			}
		}

		private List<LayoutFrameTypeAnchors> anchors = new List<LayoutFrameTypeAnchors>();

		[XmlIgnore]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public List<LayoutFrameTypeAnchors> AnchorsCollection
		{
			get { return anchors; }
		}

		protected override List<LayoutFrameTypeAnchors> GetAnchors()
		{
			return this.AnchorsCollection;
		}

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		[Browsable(false)]
		[XmlIgnore]
		public LayoutFrameType Parent { get; set; }


		[Browsable(false)]
		[XmlIgnore]
		public IEnumerable<LayoutFrameType> Children
		{
			get
			{
				var result = new List<LayoutFrameType>();
				foreach (var frames in this.Controls.OfType<FrameTypeFrames>())
				{
					result.AddRange(frames.Controls.OfType<LayoutFrameType>());
				}
				foreach (var layers in this.LayersList)
				{
					foreach (var layer in layers.Layer)
					{
						result.AddRange(layer.Layerables.OfType<LayoutFrameType>());
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Gets the expanded name of the object (placeholders are replaced)
		/// </summary>
		/// <value>The expanded name of the object.</value>
		[Browsable(false)]
		[XmlIgnore]
		public string ExpandedName
		{
			get
			{
				string parentName = this.Parent != null ?
					this.Parent.ExpandedName : null;

				return GetExpandedName(name, parentName);
			}
		}

		public static string GetExpandedName(string name, string parentName)
		{
			if (name == null)
				return null;

			if (parentName == null)
				return name;

			return name.Replace("$parent", parentName);
		}
	}
}
