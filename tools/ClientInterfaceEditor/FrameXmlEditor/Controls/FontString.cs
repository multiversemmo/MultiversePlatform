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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Drawing;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
    /// <summary>
    /// Represents the FontString control.
    /// </summary>
#if AUTOSIZE
    [Designer(typeof(AutoSizeDesigner))]
#endif
	[ToolboxBitmap(typeof(System.Windows.Forms.Label), "Label.bmp")]
    [ToolboxItemFilter("MultiverseInterfaceStudioFilter", ToolboxItemFilterType.Require)]
    public partial class FontString : GenericControl<FontStringType>, ILayerable	
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FontString"/> class.
        /// </summary>
        public FontString()
            : base(CreateInnerControl())
        {
            this.BackColor = Color.Transparent;
            this.LayerLevel = DRAWLAYER.ARTWORK;
            this.TypedSerializationObject.inherits = "GameFontNormalSmall";
#if AUTOSIZE
            // workarounding autosize
			this.InnerControl.Dock = System.Windows.Forms.DockStyle.None;
			this.InnerControl.SizeChanged += new EventHandler(InnerControl_SizeChanged);
#endif
            this.text = this.name;
			this.DesignerDefaultValues["text"] = this.name;
			this.HasBorder = true;
		}

		/// <summary>
		/// Handles the SizeChanged event of the InnerControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void InnerControl_SizeChanged(object sender, EventArgs e)
		{
			this.Size = this.InnerControl.Size;
		}

        /// <summary>
        /// Called after the control has been added to another container.
        /// </summary>
        protected override void InitLayout()
        {
            base.InitLayout();

            if (String.IsNullOrEmpty(this.text))
            {
                this.text = this.name;
				this.DesignerDefaultValues["text"] = this.name;
            }
			ChangeJustify();
#if AUTOSIZE
            this.Size = this.InnerControl.Size;
#endif
        }

#if AUTOSIZE
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.Size = this.InnerControl.Size;
        }
#endif

		protected override bool DrawName
		{
			get
			{
				return String.IsNullOrEmpty(this.text);
			}
		}

        /// <summary>
        /// Creates the inner Label control and sets its visual properties
        /// </summary>
        /// <returns></returns>
        private static System.Windows.Forms.Control CreateInnerControl()
        {
            var innerControl = new System.Windows.Forms.Label();
            innerControl.BackColor = Color.Transparent;
            innerControl.ForeColor = Color.Gold;
#if AUTOSIZE
			innerControl.AutoSize = true;
#endif
			innerControl.Location = new Point();
            innerControl.Font = new System.Drawing.Font(innerControl.Font, FontStyle.Bold);

            return innerControl;
        }

		[Category("Appearance")]
		public string text
		{
			get { return TypedSerializationObject.text; }
			set
			{
				InnerControl.Text = value;
				TypedSerializationObject.text = value;
			}
		}

		protected override void OnUpdateControl()
		{
            base.OnUpdateControl();

			// visual properties that should be reflected on the control
			this.text = TypedSerializationObject.text;
		}

		#region ILayerable Members

		[XmlIgnore]
		[Category("Layout")]
		public DRAWLAYER LayerLevel { get; set; }

		#endregion

		private static string[] inheritsList = new string[] {
			"MasterFont",
			"SystemFont",
			"GameFontNormal",
			"GameFontHighlight",
			"GameFontDisable",
			"GameFontGreen",
			"GameFontRed",
			"GameFontBlack",
			"GameFontWhite",
			"GameFontNormalSmall",
			"GameFontHighlightSmall",
			"GameFontDisableSmall",
			"GameFontDarkGraySmall",
			"GameFontGreenSmall",
			"GameFontRedSmall",
			"GameFontHighlightSmallOutline",
			"GameFontNormalLarge",
			"GameFontHighlightLarge",
			"GameFontDisableLarge",
			"GameFontGreenLarge",
			"GameFontRedLarge",
			"GameFontNormalHuge",
			"NumberFontNormal",
			"NumberFontNormalYellow",
			"NumberFontNormalSmall",
			"NumberFontNormalSmallGray",
			"NumberFontNormalLarge",
			"NumberFontNormalHuge",
			"ChatFontNormal",
			"ChatFontSmall",
			"QuestTitleFont",
			"QuestFont",
			"QuestFontNormalSmall",
			"QuestFontHighlight",
			"ItemTextFontNormal",
			"MailTextFontNormal",
			"SubSpellFont",
			"DialogButtonNormalText",
			"DialogButtonHighlightText",
			"ZoneTextFont",
			"SubZoneTextFont",
			"PVPInfoTextFont",
			"ErrorFont",
			"TextStatusBarText",
			"TextStatusBarTextSmall",
			"CombatLogFont",
			"GameTooltipText",
			"GameTooltipTextSmall",
			"GameTooltipHeaderText",
			"WorldMapTextFont",
			"InvoiceTextFontNormal",
			"InvoiceTextFontSmall",
			"CombatTextFont",
		};

		public override List<string> InheritsList
		{
			get
			{
				List<string> list = base.InheritsList;
				list.AddRange(inheritsList);
				return list;
			}
		}

		public override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
            base.OnPropertyChanged(e);

			switch (e.PropertyName)
			{
				case "justifyH":
				case "justifyV":
					ChangeJustify();
					break;
			}
		}

		private void ChangeJustify()
		{
			Label label = InnerControl as Label;
			if (label == null)
				return;

			string justificationText = 
				TypedSerializationObject.justifyV.ToString() + 
				TypedSerializationObject.justifyH.ToString();

			ContentAlignment alignment = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), justificationText, true);
			label.TextAlign = alignment;
		}
	}
}
