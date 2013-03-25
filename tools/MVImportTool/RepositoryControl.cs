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
using System.Text;
using System.Windows.Forms;

using Multiverse.AssetRepository;

namespace MVImportTool
{
    /// <summary>
    /// Custom control for browsing Repository systems. It presents a list of
    /// known Repositories in a checked list box.  Changes you make to the 
    /// repository list are reflected in the registry used by all MV applications.
    /// </summary>
    public partial class RepositoryControl : UserControl
    {
        #region Properties set by the user

        /// <summary>
        /// This is the set of repository paths checked by the user.  When you 
        /// set this property, it visits every item in the controls list box and
        /// sets it to 'checked' if the item is in the value list; else it set
        /// the item to 'unchecked'.  The idea is to try to restore a saved state,
        /// but to behave reasonably if the repository list were changed elsewhere
        /// since the last save.
        /// </summary>
        public List<string> CheckedRespositoryPaths
        {
            get
            {
                List<string> paths = new List<string>();

                foreach( string path in RepositoryListBox.CheckedItems )
                {
                    paths.Add( path );
                }

                return paths;
            }
            set
            {
                for( int i = 0; i < RepositoryListBox.Items.Count; i++ )
                {
                    if( value.Contains( RepositoryListBox.Items[ i ].ToString() ) )
                    {
                        RepositoryListBox.SetItemCheckState( i, CheckState.Checked );
                    }
                    else
                    {
                        RepositoryListBox.SetItemCheckState( i, CheckState.Unchecked );
                    }
                }
            }
        }
        #endregion Properties set by the user

        public RepositoryControl()
        {
            InitializeComponent();

            // We don't need this string, but it forces the RepositoryClass
            // to initialize it's internal list.
            // TODO: Need a better way to get the RepositoryClass right.
            RepositoryClass.Instance.GetRepositoryDirectoriesString();

            PopulateRepositoryList();
        }

        private void RepositoryControl_Load( object sender, EventArgs e )
        {
        }

        private void PopulateRepositoryList()
        {
            RepositoryListBox.BeginUpdate();

            RepositoryListBox.Items.Clear();

            foreach( string path in RepositoryClass.Instance.RepositoryDirectoryList )
            {
                RepositoryListBox.Items.Add( path );
            }

            RepositoryListBox.EndUpdate();
        }

        private void ChangeRepositoryButton_Click( object sender, EventArgs e )
        {
            DesignateRepositoriesDialog dialog = new DesignateRepositoriesDialog();

            if( DialogResult.OK == dialog.ShowDialog() )
            {
                PopulateRepositoryList();
            }
        }
    }
}
