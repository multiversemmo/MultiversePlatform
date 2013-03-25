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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MVImportTool
{
    public partial class InstallationPreviewDialog : Form
    {
        #region Properties

        public string[] CheckedItems
        {
            get
            {
                string[] items = new string[ TargetsCheckListBox.CheckedItems.Count ];

                TargetsCheckListBox.CheckedItems.CopyTo( items, 0 );

                return items;
            }
        }

        public string[] AllItems
        {
            get
            {
                string[] items = new string[ TargetsCheckListBox.Items.Count ];

                TargetsCheckListBox.Items.CopyTo( items, 0 );

                return items;
            }
        }

        public string TargetRepository
        {
            get { return m_TargetRepository; }
            set 
            { 
                m_TargetRepository = value;

                ExplanitoryLabel.Text =
                    "Check items you want copied to the target Asset Repository: \n\n" +
                    "    " + value;
            }
        }
        string m_TargetRepository;

        #endregion Properties

        public InstallationPreviewDialog()
        {
            InitializeComponent();
        }

        internal void Add( string filename, bool isChecked )
        {
            TargetsCheckListBox.Items.Add( filename, isChecked );
        }

        private void AcceptButton_Click( object sender, EventArgs e )
        {
            this.DialogResult = ( sender as Button ).DialogResult;
        }

        private void CancelButton_Click( object sender, EventArgs e )
        {
            this.DialogResult = ( sender as Button ).DialogResult;
        }
    }
}
