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

namespace MVImportTool
{
    public partial class FileControl : UserControl
    {
        public string FileName
        {
            get { return FileBrowser.FileName; }
            set
            {
                if( !String.IsNullOrEmpty( value ) )
                {
                    FileBrowser.FileName = value;
                    FileNameTextBox.Text = value;
                }
            }
        }

        public string[] FileNames
        {
            get { return FileBrowser.FileNames; }
        }

        public bool Multiselect
        {
            get { return FileBrowser.Multiselect; }
            set { FileBrowser.Multiselect = value; }
        }

        public string InitialDirectory
        {
            get { return FileBrowser.InitialDirectory; }
            set { FileBrowser.InitialDirectory = value; }
        }

        public string Filter
        {
            get { return FileBrowser.Filter; }
            set { FileBrowser.Filter = value; }
        }

        public string Title
        {
            get { return FileBrowser.Title; }
            set { FileBrowser.Title = value; }
        }

        /// <summary>
        /// ToolTip displayed for the browser button
        /// </summary>
        public string ButtonToolTip
        {
            get { return BrowserToolTip.GetToolTip( this.BrowseButton ); }
            set { BrowserToolTip.SetToolTip( this.BrowseButton, value ); }
        }

        /// <summary>
        /// This is the label displayed above the text-box that shows the 
        /// selected directory. You'd typically use it to explain what the
        /// selection represents.
        /// </summary>
        public string Label
        {
            get { return ControlLabel.Text; }
            set { ControlLabel.Text = value; }
        }


        #region Custom Events

        public class StringEventArgs : EventArgs 
        {
            public StringEventArgs( string value )
            {
                Value = value;
            }
            public readonly string Value;
        }

        public delegate void StringEventHandler( object sender, StringEventArgs args );

        /// <summary>
        /// This event fires after the selected file name changes. The event
        /// arguments contain the new file. If Multiselect is enabled, only 
        /// the first selected file is sent.
        /// </summary>
        public event StringEventHandler FileNameChangedEvent;

        private void FireFileNameChanged()
        {
            if( null != FileNameChangedEvent )
            {
                FileNameChangedEvent.Invoke( this, new StringEventArgs( FileName ) );
            }
        }

        #endregion Custom Events



        public FileControl()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click( object sender, EventArgs e )
        {
            string oldFile = FileName;

            if( DialogResult.OK == FileBrowser.ShowDialog() )
            {
                string newName = this.Multiselect
                            ? FileBrowser.FileNames[ 0 ]
                            : FileBrowser.FileName;

                if( oldFile != newName )
                {
                    FileName = newName;

                    FireFileNameChanged();
                }
            }
        }
    }
}
