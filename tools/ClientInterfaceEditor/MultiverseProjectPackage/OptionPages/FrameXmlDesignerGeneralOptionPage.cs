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

using System.Windows.Forms;

using Microsoft.VisualStudio.Shell;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.MultiverseInterfaceStudio
{
    [GuidAttribute(GuidStrings.FrameXmlDesignerOptionPage)]
    public class FrameXmlDesignerGeneralOptionPage : DialogPage
    {
        private FrameXmlDesignerGeneralOptionPageControl control = new FrameXmlDesignerGeneralOptionPageControl();

        protected override IWin32Window Window
        {
            get
            {
                return control;
            }
        }

        public string BackgroundImageFile
        {
            get { return control.BackgroundImageFile; }
            set { control.BackgroundImageFile = value; }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.BackgroundImageFile) &&
                !File.Exists(this.BackgroundImageFile))
            {
                e.ApplyBehavior = ApplyKind.Cancel;
                MessageBox.Show(Window, "The specified path for the background image file is invalid. Please enter a valid path.", "AddOn Studio for World of Warcraft", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
                base.OnApply(e);
        }
    }
}
