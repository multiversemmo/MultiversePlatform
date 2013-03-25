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
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(TaxiRouteFrameType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(MinimapType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CooldownType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(GameTooltipType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(WorldFrameType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(MovieFrameType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ScrollFrameType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ScrollingMessageFrameType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageFrameType))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrowserType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ModelType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(PlayerModelType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(TabardModelType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(DressUpModelType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ColorSelectType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EditBoxType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(SliderType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(StatusBarType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(ButtonType))]
//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(UnitButtonType))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CheckButtonType))]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "3.5.20706.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.multiverse.net/ui")]
	[System.Xml.Serialization.XmlRootAttribute("Frame", Namespace = "http://www.multiverse.net/ui", IsNullable = false)]
	public partial class FrameType : LayoutFrameType
	{
		public FrameType()
		{

		}

		/// <remarks/>
		[XmlAttribute]
		[DefaultValueAttribute(1f)] //typeof(float), "1")]
		[Category("Appearance")]
		public float alpha
		{
			get
			{
				return this.Properties.GetValue<float>("alpha");
			}
			set
			{
				this.Properties["alpha"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[Category("Design")]
		public string parent
		{
			get
			{
				return this.Properties.GetValue<string>("parent");
			}
			set
			{
				this.Properties["parent"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Appearance")]
		public bool toplevel
		{
			get
			{
				return this.Properties.GetValue<bool>("toplevel");
			}
			set
			{
				this.Properties["toplevel"] = value;
			}
		}

		/// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        [Category("Behavior")]
        public bool movable
        {
            get
            {
                return this.Properties.GetValue<bool>("movable");
            }
            set
            {
                this.Properties["movable"] = value;
            }
        }

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(false)]
        //[Category("Behavior")]
        //public bool resizable
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<bool>("resizable");
        //    }
        //    set
        //    {
        //        this.Properties["resizable"] = value;
        //    }
        //}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(FRAMESTRATA.HIGH)]
		[Category("Appearance")]
		public FRAMESTRATA frameStrata
		{
			get
			{
				return this.Properties.GetValue<FRAMESTRATA>("frameStrata");
			}
			set
			{
				this.Properties["frameStrata"] = value;
			}
		}

        ///// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[Category("Appearance")]
        //public int frameLevel
        //{
        //    get
        //    {
        //        return this.frameLevelField;
        //    }
        //    set
        //    {
        //        this.frameLevelField = value;
        //    }
        //}

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //[Category("Appearance")]
        //public bool frameLevelSpecified
        //{
        //    get
        //    {
        //        return this.frameLevelFieldSpecified;
        //    }
        //    set
        //    {
        //        this.frameLevelFieldSpecified = value;
        //    }
        //}

        /// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(0)]
        //[Category("Design")]
        //public int id
        //{
        //    get
        //    {
        //        return this.idField;
        //    }
        //    set
        //    {
        //        this.idField = value;
        //    }
        //}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Behavior")]
		public bool enableMouse
		{
			get
			{
				return this.Properties.GetValue<bool>("enableMouse");
			}
			set
			{
				this.Properties["enableMouse"] = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		[System.ComponentModel.DefaultValueAttribute(false)]
		[Category("Behavior")]
		public bool enableKeyboard
		{
			get
			{
				return this.Properties.GetValue<bool>("enableKeyboard");
			}
			set
			{
				this.Properties["enableKeyboard"] = value;
			}
		}

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(false)]
        //[Category("Behavior")]
        //public bool clampedToScreen
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<bool>("clampedToScreen");
        //    }
        //    set
        //    {
        //        this.Properties["clampedToScreen"] = value;
        //    }
        //}

		/// <remarks/>
        //[System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(false)]
        //[Category("Appearance")]
        //public bool @protected
        //{
        //    get
        //    {
        //        return this.Properties.GetValue<bool>("protected");
        //    }
        //    set
        //    {
        //        this.Properties["protected"] = value;
        //    }
        //}

        //[XmlElement("ResizeBounds", typeof(FrameTypeResizeBounds))]
        //[Browsable(false)]
        //public FrameTypeResizeBounds[] ResizeBounds
        //{
        //    get
        //    {
        //        FrameTypeResizeBounds resizeBounds = null;
        //        if (this.Properties.HasValue("MinResize"))
        //        {
        //            resizeBounds = new FrameTypeResizeBounds();
        //            resizeBounds.MinResizes = 
        //                this.Properties.GetArray<Dimension.minResize>("MinResize");
        //        }
        //        if (this.Properties.HasValue("MaxResize"))
        //        {
        //            if (resizeBounds == null)
        //                resizeBounds = new FrameTypeResizeBounds();
        //            resizeBounds.MaxResizes = 
        //                this.Properties.GetArray<Dimension.maxResize>("MaxResize");
        //        }
        //        return resizeBounds != null ?
        //            new FrameTypeResizeBounds[] { resizeBounds } :
        //            new FrameTypeResizeBounds[] { };
        //    }
        //    set 
        //    {
        //        if (value != null && value.Length > 0)
        //        {
        //            this.Properties.SetArray<Dimension.minResize>("MinResize", value[0].MinResizes);
        //            this.Properties.SetArray<Dimension.maxResize>("MaxResize", value[0].MaxResizes);
        //        }
        //        else
        //        {
        //            this.Properties.Remove("MinResize");
        //            this.Properties.Remove("MaxResize");
        //        }
        //    }
        //}

		[XmlElement("Frames", typeof(FrameTypeFrames))]
		[Browsable(false)]
		public List<SerializationObject> FramesList
		{
			get { return this.Controls; }
		}

		private List<ScriptsType> scripts = new List<ScriptsType>();

		[XmlIgnore]
		[Browsable(false)]
		public List<ScriptsType> Scripts
		{
			get { return this.scripts; }
		}

		protected override List<ScriptsType> GetScripts()
		{
			return this.Scripts;
		}

		private SerializationMap<TextureChoice, TextureType> textureItems = new SerializationMap<TextureChoice, TextureType>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<TextureChoice, TextureType> Textures
		{
			get { return this.textureItems; }
		}

		/// <summary>
		/// Items of type TextureType with different element names.
		/// </summary>
        [XmlElement("BarTexture", typeof(TextureType))]
        [XmlElement("CheckedTexture", typeof(TextureType))]
        [XmlElement("DisabledCheckedTexture", typeof(TextureType))]
        [XmlElement("DisabledTexture", typeof(TextureType))]
        [XmlElement("HighlightTexture", typeof(TextureType))]
        [XmlElement("NormalTexture", typeof(TextureType))]
        [XmlElement("PushedTexture", typeof(TextureType))]
        ////[XmlElement("ThumbTexture", typeof(TextureType))]
        [XmlChoiceIdentifier(MemberName = "TextureNames")]
        [Browsable(false)]
        public TextureType[] TextureItems
        {
            get { return this.textureItems.ValuesArray; }
            set { this.textureItems.ValuesArray = value; }
        }

		/// <summary>
		/// Element names for items of type TextureType.
		/// </summary>
        [XmlElement("TextureNames")]
        [XmlIgnore]
        [Browsable(false)]
        public TextureChoice[] TextureNames
        {
            get { return this.textureItems.KeysArray; }
            set { this.textureItems.KeysArray = value; }
        }
	}
}
