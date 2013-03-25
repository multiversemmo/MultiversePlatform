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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Windows.Forms;

using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;


namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
    /// <summary>
    /// Represents the MultiverseInterface AddOn element "Button"
    /// </summary>
	[ToolboxBitmap(typeof(System.Windows.Forms.Button), "Button.bmp")]
    [ToolboxItemFilter("MultiverseInterfaceStudioFilter", ToolboxItemFilterType.Require)]
    public partial class Button : GenericFrameControl<ButtonType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        public Button()
            : base(CreateInnerControl())
        {
			this.HasBorder = true;
		
			this.TypedSerializationObject.inherits = "UIPanelButtonTemplate";
			OnUpdateControl();
		}

        public override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            switch (e.PropertyName)
            {
				case "text":
					OnTextChanged();
					break;

            }
        }

		private void OnTextChanged()
		{
			InnerControl.Text = TypedSerializationObject.text;
		}

        protected override void InitLayout()
        {
            base.InitLayout();
			if (String.IsNullOrEmpty(this.TypedSerializationObject.text))
            {
				this.TypedSerializationObject.text = this.name;
				this.DesignerDefaultValues["text"] = this.name;
            }
			OnTextChanged();
        }

        /// <summary>
        /// Creates the inner control and initializes its visual properties
        /// </summary>
        /// <returns></returns>
        private static System.Windows.Forms.Control CreateInnerControl()
        {
            var innerControl = new System.Windows.Forms.Button();
            innerControl.Font = new System.Drawing.Font(innerControl.Font, FontStyle.Bold);

            return
                innerControl;
        }

		protected override bool DrawName
		{
			get
			{
				return String.IsNullOrEmpty(this.TypedSerializationObject.text);
			}
		}

		private System.Windows.Forms.Button TypedInnerControl
		{
			get { return (System.Windows.Forms.Button)InnerControl; }
		}

		protected override void OnUpdateControl()
		{
			base.OnUpdateControl();

			// visual properties that should be reflected on the control
			switch (this.TypedSerializationObject.inherits)
			{
				case "":
				case null:
					InnerControl.BackColor = Color.Transparent;
					InnerControl.ForeColor = Color.White;
					TypedInnerControl.FlatStyle = FlatStyle.Flat;
					TypedInnerControl.FlatAppearance.BorderSize = 0;
					break;
				case "UIPanelCloseButton":
					this.Size = new Size(26, 26);
					this.TypedSerializationObject.text = "X";
					this.DesignerDefaultValues["text"] = "X";
					goto default;
				default:
					InnerControl.BackColor = Color.DarkRed;
					InnerControl.ForeColor = Color.Gold;
					TypedInnerControl.FlatStyle = FlatStyle.Standard;
					TypedInnerControl.FlatAppearance.BorderSize = 1;
					break;
			}
			OnTextChanged();
		}

		private static string[] inheritsList = new string[] {
			"UIPanelButtonTemplate",
			"UIPanelButtonTemplate2",
			"UIPanelButtonGrayTemplate",
			"UIPanelCloseButton",
			"UIPanelScrollUpButtonTemplate",
			"UIPanelScrollDownButtonTemplate",
			"TabButtonTemplate",
			"GameMenuButtonTemplate",		
		};

		public override List<string> InheritsList
		{
			get
			{
				List<string> result = base.InheritsList;
				result.AddRange(inheritsList);
				return result;
			}
		}

		public override EventChoice? DefaultEventChoice
		{
			get
			{
				return EventChoice.OnClick;
			}
		}
	}
}
