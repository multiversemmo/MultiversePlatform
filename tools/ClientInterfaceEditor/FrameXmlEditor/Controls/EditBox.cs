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
using System.Text;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Drawing;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
    /// <summary>
    /// Represents the EditBox control in WoW.
    /// </summary>
	[ToolboxBitmap(typeof(System.Windows.Forms.TextBox), "TextBox.bmp")]
    [ToolboxItemFilter("MultiverseInterfaceStudioFilter", ToolboxItemFilterType.Require)]
    public partial class EditBox : GenericFrameControl<EditBoxType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditBox"/> class.
        /// </summary>
        public EditBox() : base(CreateInnerControl())
        {
        }

        /// <summary>
        /// Creates the inner control and sets its visual properties.
        /// </summary>
        /// <returns></returns>
        private static System.Windows.Forms.TextBox CreateInnerControl()
        {
            var innerControl = new System.Windows.Forms.TextBox();
            innerControl.Text = "Sample Text";
            innerControl.BackColor = Color.DarkGray;
            innerControl.ForeColor = Color.Gold;
            innerControl.Font = new System.Drawing.Font(innerControl.Font, FontStyle.Bold);

            return innerControl;
        }

		private static string[] inheritsList = new string[] {
			"InputBoxTemplate",
		};

		public override List<string> InheritsList
		{
			get
			{
				var result = base.InheritsList;
				result.AddRange(inheritsList);
				return result;
			}
		}

		public override EventChoice? DefaultEventChoice
		{
			get
			{
                return EventChoice.OnChar;
			}
		}
	}
}
