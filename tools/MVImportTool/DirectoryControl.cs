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

namespace MVImportTool
{
    /// <summary>
    /// Custom control that lets you browse for a directory. It displays the
    /// selected directory in a text box.
    /// </summary>
    public partial class DirectoryControl : UserControl
    {
        /// <summary>
        /// The selected directory
        /// </summary>
        public string FolderPath
        {
            get { return folderBrowserDialog.SelectedPath; }
            set 
            {
                if( String.Empty == value ||
                    Directory.Exists( value ) )
                {
                    folderBrowserDialog.SelectedPath = value;
                    directoryPathTextBox.Text = value;
                }
                else
                {
                    throw new ArgumentException(
                        String.Format( "Directory does not exist: '{0}'", value ) );
                }
            }
        }

        /// <summary>
        /// This is the label displayed above the text-box that shows the 
        /// selected directory. You'd typically use it to explain what the
        /// selection represents.
        /// </summary>
        public string Label
        {
            get { return controlLabel.Text; }
            set { controlLabel.Text = value; }
        }

        /// <summary>
        /// This is the description presented in the browser after you click 
        /// on the browse button.
        /// </summary>
        public string Description
        {
            get { return folderBrowserDialog.Description; }
            set { folderBrowserDialog.Description = value; }
        }

        /// <summary>
        /// ToolTip displayed for the browser button
        /// </summary>
        public string ButtonToolTip
        {
            get { return BrowserToolTip.GetToolTip( this.BrowseButton ); }
            set { BrowserToolTip.SetToolTip( this.BrowseButton, value ); }
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
        /// This event fires after the selected directory changes. The event
        /// arguments contain the new path.
        /// </summary>
        public event StringEventHandler FolderChanged;

        private void FireFolderChanged()
        {
            if( null != FolderChanged )
            {
                FolderChanged.Invoke( this, new StringEventArgs( FolderPath ) );
            }
        }

        #endregion Custom Events

        public DirectoryControl()
        {
            InitializeComponent();

            Label = String.Empty;

            FolderPath = String.Empty;
        }

        private void browseButton_Click( object sender, EventArgs e )
        {
            string oldPath = folderBrowserDialog.SelectedPath;

            if( DialogResult.OK == folderBrowserDialog.ShowDialog() )
            {
                FolderPath = folderBrowserDialog.SelectedPath;

                if( FolderPath != oldPath )
                {
                    FireFolderChanged();
                }
            }
        }
    }
}
