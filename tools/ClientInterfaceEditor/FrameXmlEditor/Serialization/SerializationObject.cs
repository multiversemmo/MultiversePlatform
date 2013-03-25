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

using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{

	/// <summary>
	/// Base for classes containing objects that are mapped as child elements in serialization.
	/// </summary>
	public abstract partial class SerializationObject
	{
		protected SerializationObject() { }

		/// <summary>
		/// Items of type Dimension with different element names.
		/// </summary>
		[XmlElement("Size", typeof(Dimension.Size))]
		[Browsable(false)]
		public virtual Dimension.Size[] Sizes { get; set; }

		protected virtual List<LayoutFrameTypeAnchors> GetAnchors() { return null; }

		[XmlElement("Anchors", typeof(LayoutFrameTypeAnchors))]
		[Browsable(false)]
		public List<LayoutFrameTypeAnchors> AnchorsXml
		{
			get { return this.GetAnchors(); }
		}

		private List<SerializationObject> controls = new List<SerializationObject>();

		[Browsable(false)]
		[XmlIgnore]
		public List<SerializationObject> Controls
		{
			get { return controls; }
		}

        protected virtual List<UiScript> GetUiScripts() { return null; }

        [XmlElement("Script", typeof(UiScript))]
        [Browsable(false)]
        public List<UiScript> UiScriptsXml
        {
            get { return this.GetUiScripts(); }
        }

		protected virtual List<ScriptsType> GetScripts() { return null; }

		[XmlElement("Scripts", typeof(ScriptsType))]
		[Browsable(false)]
		public List<ScriptsType> ScriptsXml
		{
			get { return this.GetScripts(); }
		}

		private List<FrameTypeLayers> layersList = new List<FrameTypeLayers>();

		[XmlElement("Layers", typeof(FrameTypeLayers))]
		[Browsable(false)]
		public List<FrameTypeLayers> LayersList
		{
			get { return this.layersList; }
		}

		private IList<object> items = new List<object>();

		[XmlIgnore]
		[Browsable(false)]
		public IList<object> Items
		{
			get { return this.items; }
		}

		/// <summary>
		/// Items that have fixed element names.
		/// </summary>
		[XmlElement("Attributes", typeof(AttributesType))]
		//[XmlElement("Backdrop", typeof(BackdropType))]
		//[XmlElement("Cooldown", typeof(CooldownType))]
		//[XmlElement("DressUpModel", typeof(DressUpModelType))]
		//[XmlElement("Font", typeof(Font))]
		//[XmlElement("Layers", typeof(FrameTypeLayers))]
		// [XmlElement("ResizeBounds", typeof(FrameTypeResizeBounds))]
		// [XmlElement("GameTooltip", typeof(GameTooltipType))]
		//[XmlElement("Gradient", typeof(GradientType))]
		//[XmlElement("TitleRegion", typeof(LayoutFrameType))]
		//[XmlElement("Anchors", typeof(LayoutFrameTypeAnchors))]
		//[XmlElement("Minimap", typeof(MinimapType))]
		//[XmlElement("MovieFrame", typeof(MovieFrameType))]
		//[XmlElement("PlayerModel", typeof(PlayerModelType))]
		//[XmlElement("Scripts", typeof(ScriptsType))]
		//[XmlElement("Shadow", typeof(ShadowType))]
		//[XmlElement("Slider", typeof(SliderType))]
		//[XmlElement("TabardModel", typeof(TabardModelType))]
		//[XmlElement("TaxiRouteFrame", typeof(TaxiRouteFrameType))]
		//[XmlElement("TexCoords", typeof(TextureTypeTexCoords))]
		//[XmlElement("Include", typeof(UiInclude))]
		//[XmlElement("Script", typeof(UiScript))]
		//[XmlElement("WorldFrame", typeof(WorldFrameType))]
		[Browsable(false)]
		public object[] ItemsArray
		{
			get
			{
				return this.items.ToArray();
			}
			set
			{
				this.items = (value == null) ? new List<object>() : new List<object>(value);
			}
		}

		private SerializationMap<ColorChoice, ColorType> colorItems = new SerializationMap<ColorChoice, ColorType>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<ColorChoice, ColorType> Colors
		{
			get { return this.colorItems; }
		}

		/// <summary>
		/// Items of type ColorType with different element names.
		/// </summary>
		[XmlElement("BarColor", typeof(ColorType))]
		//[XmlElement("BorderColor", typeof(ColorType))]
		[XmlElement("Color", typeof(ColorType))]
		//[XmlElement("DisabledColor", typeof(ColorType))]
		[XmlElement("HighlightColor", typeof(ColorType))]
		//[XmlElement("NormalColor", typeof(ColorType))]
		[XmlChoiceIdentifier(MemberName = "ColorNames")]
		[Browsable(false)]
		public ColorType[] ColorItems
		{
			get { return this.colorItems.ValuesArray; }
			set { this.colorItems.ValuesArray = value; }
		}

		/// <summary>
		/// Element names for items of type ColorType.
		/// </summary>
        [XmlElement("ColorNames")]
        [XmlIgnore]
        [Browsable(false)]
        public ColorChoice[] ColorNames
        {
            get { return this.colorItems.KeysArray; }
            set { this.colorItems.KeysArray = value; }
        }

		private SerializationMap<InsetChoice, Inset> insetItems = new SerializationMap<InsetChoice, Inset>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<InsetChoice, Inset> Insets
		{
			get { return this.insetItems; }
		}

		/// <summary>
		/// Items of type Inset with different names.
		/// </summary>
		[XmlElement("BackgroundInsets", typeof(Inset))]
		[XmlElement("HitRectInsets", typeof(Inset))]
		[XmlElement("TextInsets", typeof(Inset))]
		[XmlChoiceIdentifier(MemberName = "InsetNames")]
		[Browsable(false)]
		public Inset[] InsetItems
		{
			get { return this.insetItems.ValuesArray; }
			set { this.insetItems.ValuesArray = value; }
		}

		/// <summary>
		/// Element names for items of type Inset.
		/// </summary>
		[XmlElement("InsetNames")]
		[XmlIgnore]
		[Browsable(false)]
		public InsetChoice[] InsetNames
		{
			get { return this.insetItems.KeysArray; }
			set { this.insetItems.KeysArray = value; }
		}

		private SerializationMap<FontStringChoice, FontStringType> fontStrings = new SerializationMap<FontStringChoice, FontStringType>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<FontStringChoice, FontStringType> FontStrings
		{
			get { return this.fontStrings; }
		}

		/// <summary>
		/// Items of type FontStringType with different names.
		/// </summary>
		[XmlElement("ButtonText", typeof(FontStringType))]
		//[XmlElement("FontString", typeof(FontStringType))]
		[XmlElement("FontStringHeader1", typeof(FontStringType))]
		[XmlElement("FontStringHeader2", typeof(FontStringType))]
		[XmlElement("FontStringHeader3", typeof(FontStringType))]
		[XmlChoiceIdentifier("FontStringNames")]
		[Browsable(false)]
		public FontStringType[] FontStringItems
		{
			get { return this.fontStrings.ValuesArray; }
			set { this.fontStrings.ValuesArray = value; }
		}

		/// <summary>
		/// Element names for items of type FontStringType.
		/// </summary>
		[XmlElement("FontStringNames")]
		[XmlIgnore]
		[Browsable(false)]
		public FontStringChoice[] FontStringNames
		{
			get { return this.fontStrings.KeysArray; }
			set { this.fontStrings.KeysArray = value; }
		}

		private SerializationMap<ValueChoice, Value> valueItems = new SerializationMap<ValueChoice, Value>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<ValueChoice, Value> Values
		{
			get { return this.valueItems; }
		}

		/// <summary>
		/// Items of type Value with different element names.
		/// </summary>
		[XmlElement("EdgeSize", typeof(Value))]
		[XmlElement("FontHeight", typeof(Value))]
		[XmlElement("TileSize", typeof(Value))]
		[XmlChoiceIdentifier(MemberName = "ValueNames")]
		[Browsable(false)]
		public Value[] ValueItems
		{
			get { return this.valueItems.ValuesArray; }
			set { this.valueItems.ValuesArray = value; }
		}

		/// <summary>
		/// Element names for items of type Value.
		/// </summary>
		[XmlElement("ValueNames")]
		[XmlIgnore]
		[Browsable(false)]
		public ValueChoice[] ValueNames
		{
			get { return this.valueItems.KeysArray; }
			set { this.valueItems.KeysArray = value; }
		}

		private SerializationMap<FontChoice, FontType> fontItems = new SerializationMap<FontChoice, FontType>();

		[XmlIgnore]
		[Browsable(false)]
		public IDictionary<FontChoice, FontType> Fonts
		{
			get { return this.fontItems; }
		}

		/// <summary>
		/// Items of type FontType with different element names.
		/// </summary>
        //[XmlElement("DisabledFont", typeof(FontType))]
        //[XmlElement("HighlightFont", typeof(FontType))]
        //[XmlElement("NormalFont", typeof(FontType))]
        //[XmlChoiceIdentifier(MemberName = "FontNames")]
        //[Browsable(false)]
        //public FontType[] FontItems
        //{
        //    get { return this.fontItems.ValuesArray; }
        //    set { this.fontItems.ValuesArray = value; }
        //}

		/// <summary>
		/// Element names for items of type FontType.
		/// </summary>
		[XmlElement("FontNames")]
		[XmlIgnore]
		[Browsable(false)]
		public FontChoice[] FontNames
		{
			get { return this.fontItems.KeysArray; }
			set { this.fontItems.KeysArray = value; }
		}

        //private SerializationMap<ModelChoice, ModelType> modelItems = new SerializationMap<ModelChoice, ModelType>();

        //[XmlIgnore]
        //[Browsable(false)]
        //public IDictionary<ModelChoice, ModelType> Models
        //{
        //    get { return this.modelItems; }
        //}

		/// <summary>
		/// Items of type ModelType with different element names.
		/// </summary>
		// [XmlElement("Model", typeof(ModelType))]
        //[XmlElement("ModelFFX", typeof(ModelType))]
        //[XmlChoiceIdentifier("ModelNames")]
        //[Browsable(false)]
        //public ModelType[] ModelItems
        //{
        //    get { return this.modelItems.ValuesArray; }
        //    set { this.modelItems.ValuesArray = value; }
        //}

		/// <summary>
		/// Element names for items of type ModelType.
		/// </summary>
        //[XmlElement("ModelNames")]
        //[XmlIgnore]
        //[Browsable(false)]
        //public ModelChoice[] ModelNames
        //{
        //    get { return this.modelItems.KeysArray; }
        //    set { this.modelItems.KeysArray = value; }
        //}

	}
}
