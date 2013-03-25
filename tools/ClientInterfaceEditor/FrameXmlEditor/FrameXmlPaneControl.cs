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
using System.Windows.Forms;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
    public partial class FrameXmlPaneControl : UserControl
    {
        private Dictionary<string, TabPage> tabPages = new Dictionary<string, TabPage>();

        public FrameXmlPaneControl()
        {
            InitializeComponent();
        }

        public string SelectedPane
        {
            get
            {
                if (tabControl.SelectedTab == tabPageMain)
                    return null;

                return tabControl.SelectedTab.Name;
            }
            set
            {
				if (value == null)
					tabControl.SelectedTab = tabPageMain;
				else
					tabControl.SelectTab(value);
            }
        }

        public event EventHandler SelectedPaneChanged;

		public void AddPane(string name)
		{
			if (name != null && !tabPages.ContainsKey(name))
			{
				TabPage tabPage = new TabPage { Name = name, Text = name };

				// Add the tab page
				tabControl.TabPages.Add(tabPage);
				tabPages.Add(name, tabPage);
			}
		}

        public void RemovePane(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (tabPages.ContainsKey(name))
            {
                tabControl.TabPages.Remove(tabPages[name]);
                tabPages.Remove(name);
            }
        }

		public void RecreatePanes(IEnumerable<string> virtualPaneNames, string selectedPaneName)
		{
			tabControl.SelectedIndexChanged -= new EventHandler(OnSelectedIndexChanged);
			try
			{
				foreach (var pane in tabPages.Keys.ToList<string>())
					RemovePane(pane);

				// sort names
				var paneNames = virtualPaneNames.ToList<string>();
				paneNames.Sort();

				foreach (string virtualPaneName in paneNames)
					AddPane(virtualPaneName);

				this.SelectedPane = selectedPaneName;
			}
			finally
			{
				tabControl.SelectedIndexChanged += new EventHandler(OnSelectedIndexChanged);
			}
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedPaneChanged != null)
                SelectedPaneChanged(this, e);
        }
	}
}
