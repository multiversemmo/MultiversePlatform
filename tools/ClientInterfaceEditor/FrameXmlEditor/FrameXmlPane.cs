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
using System.Windows.Forms;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Design;
using System.ComponentModel.Design;
using System.Collections.Generic;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
    public class FrameXmlPane : DesignerWindowPane
    {
        private FrameXmlPaneControl frameXmlPaneControl = new FrameXmlPaneControl();

        private Control designerWindowPaneControl;

        public FrameXmlPane(DesignSurface surface)
            : base(surface)
        {
            InitializeControl();
        }

        private void InitializeControl()
        {
            //try
            //{
                designerWindowPaneControl = (Control)Surface.View;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK);
            //    MessageBox.Show(ex.InnerException.ToString(), "Error", MessageBoxButtons.OK);
            //}
            designerWindowPaneControl.Dock = DockStyle.Fill;
            frameXmlPaneControl.Controls.Add(designerWindowPaneControl);
        }

		public void AddPane(string name)
		{
			frameXmlPaneControl.AddPane(name);
		}

        public void RemovePane(string name)
        {
            frameXmlPaneControl.RemovePane(name);
        }

		public void RecreatePanes(IEnumerable<string> virtualPaneNames, string selectedPaneName)
		{
			frameXmlPaneControl.RecreatePanes(virtualPaneNames, selectedPaneName);
		}

        public string SelectedPane
        {
            get { return frameXmlPaneControl.SelectedPane; }
            set { frameXmlPaneControl.SelectedPane = value; }
        }

        public event EventHandler SelectedPaneChanged
        {
            add
            {
                frameXmlPaneControl.SelectedPaneChanged += value;
            }
            remove
            {
                frameXmlPaneControl.SelectedPaneChanged -= value;
            }
        }

        public override IWin32Window Window
        {
            get { return frameXmlPaneControl; }
        }
	}
}
