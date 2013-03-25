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
using System.IO;

using Microsoft.Win32;


namespace MVImportTool
{
    public partial class SourceFileSelector : UserControl
    {
        #region Properties set by the user

        /// <summary>
        /// This is the folder selected by the user for source files. The
        /// property is writable to restore saved settings.
        /// </summary>
        public string Folder
        {
            get { return SourceFolderControl.FolderPath; }
            set { SetSourceFileFolder( value ); }
        }

        /// <summary>
        /// This is a regular expression that filters files selected in the
        /// source folder. The property is writable to restore saved settings.
        /// </summary>
        public string FileFilter
        {
            get { return FilterTextBox.Text; }
            set 
            { 
                FilterTextBox.Text = value;
                PopulateSourceFileList();
            }
        }

        /// <summary>
        /// This gets the files that are checked in the UI. The list is not
        /// saved in the settings, so it is not writable.
        /// </summary>
        public List<FileInfo> CheckedFiles
        {
            get 
            {
                List<FileInfo> files = new List<FileInfo>();

                foreach( FileInfo info in SourceFileListBox.CheckedItems )
                {
                    files.Add( info );
                }

                return files;
            }
        }

        #endregion Properties set by the user

        FileSystemWatcher m_FileWatcher = new FileSystemWatcher();

        public SourceFileSelector()
        {
            InitializeComponent();
        }

        private void SourceFileSelector_Load( object sender, EventArgs e )
        {
            SourceFolderControl.Description = "Select a folder containing COLLADA (.dae) files to convert";
            SourceFolderControl.FolderChanged += OnFolderChanged;

            ColladaFilesToolTip.SetToolTip( this.FilterTextBox,
                "Regular Expression that filters the files \n" + 
                "displayed from the selected folder." );

            m_FileWatcher.BeginInit();

            m_FileWatcher.Created += OnFileAddedOrRemoved;
            m_FileWatcher.Deleted += OnFileAddedOrRemoved;
            m_FileWatcher.Renamed += OnFileRenamed;

            m_FileWatcher.EndInit();
         }

        private void OnFolderChanged( object sender, DirectoryControl.StringEventArgs args )
        {
            SetSourceFileFolder( args.Value );
        }

        private void OnFileAddedOrRemoved( object source, FileSystemEventArgs args )
        {
            CrossThreadPopulateSourceFileList();
        }

        private void OnFileRenamed( object source, RenamedEventArgs args )
        {
            CrossThreadPopulateSourceFileList();
        }

        private void SetSourceFileFolder( string folderPath )
        {
            m_FileWatcher.EnableRaisingEvents = false;

            SourceFolderControl.FolderPath = folderPath;

            if( Directory.Exists( folderPath ) )
            {
                PopulateSourceFileList();

                m_FileWatcher.Path = folderPath;
                m_FileWatcher.EnableRaisingEvents = true;
            }
        }

        // Add files in the selected directory to the checked list box; items 
        // are added unchecked, the assumption being that the common action is
        // to work on one file at a time.
        //
        // Files in the selected directory are filtered by the 'File Filter' 
        // string, which is treated as a regular expression.  If the filter
        // is empty, it is considered to be the same as '*'.
        private void PopulateSourceFileList()
        {
            if( SourceFolderControl.FolderPath.Equals( String.Empty ) )
            {
                // Early exit for uninitialized path
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo( SourceFolderControl.FolderPath );

            if( dirInfo.Exists )
            {
                SourceFileListBox.BeginUpdate();

                SourceFileListBox.Items.Clear();

                string fileFilter = 
                    FilterTextBox.Text.Equals( String.Empty )
                    ? "*" : FilterTextBox.Text;

                foreach( FileInfo fileInfo in dirInfo.GetFiles( fileFilter ) )
                {
                    SourceFileListBox.Items.Add( fileInfo, false );
                }

                SourceFileListBox.EndUpdate();
            }
        }

        // This is a wrapper that allows a control to be updated in response to
        // an event on a different thread from the one that owns the list box.
        // This comes up when the file watcher issues events.
        private delegate void PopulateCallback();

        private void CrossThreadPopulateSourceFileList()
        {
            if( this.SourceFileListBox.InvokeRequired )
            {
                PopulateCallback pcb = new PopulateCallback( CrossThreadPopulateSourceFileList );
                this.Invoke( pcb );
            }
            else
            {
                PopulateSourceFileList();
            }
        }

        private void FilterTextBox_KeyDown( object sender, KeyEventArgs e )
        {
            if( e.KeyValue.Equals( '\r' ) )
            {
                PopulateSourceFileList();
            }
        }

        private void CheckAllButton_Click( object sender, EventArgs e )
        {
            for( int i = 0; i < SourceFileListBox.Items.Count; i++ )
            {
                SourceFileListBox.SetItemCheckState( i, CheckState.Checked );
            }
        }

        private void ClearAllButton_Click( object sender, EventArgs e )
        {
            for( int i = 0; i < SourceFileListBox.Items.Count; i++ )
            {
                SourceFileListBox.SetItemCheckState( i, CheckState.Unchecked );
            }
        }
    }
}
