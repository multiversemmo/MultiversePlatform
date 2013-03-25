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
    /// <summary>
    /// Custom control for settings that configure the application (as opposed
    /// to the process the application runs).
    /// </summary>
    public partial class ImporterConfiguration : UserControl
    {
        #region Properties set by the user

        /// <summary>
        /// This is the full, absolute path to the ConversionTool executable.
        /// </summary>
        public string ConversionToolExeFile
        {
            get { return ExeFileControl.FileName; }
            set { ExeFileControl.FileName = value; }
        }

        /// <summary>
        /// This is the working folder, the location for intermediate files.
        /// </summary>
        public string WorkingFolder
        {
            get { return WorkingFolderControl.FolderPath; }
            set
            {
                if( ! String.IsNullOrEmpty( value ) )
                {
                    DirectoryInfo info = new DirectoryInfo( value );

                    if( info.Exists )
                    {
                        WorkingFolderControl.FolderPath = value;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show(
                            String.Format( 
                                "Working folder '{0}' does not exist;\n" +
                                "Do you want to create the folder?", value ),
                                "Multiverse COLLADA Import Tool",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning );

                        if( result.Equals( DialogResult.Yes ) )
                        {
                            info.Create();
                            WorkingFolderControl.FolderPath = value;
                        }
                    }
                }
            }
        }
        #endregion Properties set by the user

        public ImporterConfiguration()
        {
            InitializeComponent();
        }

        private void ImporterConfiguration_Load( object sender, EventArgs e )
        {
            WorkingFolderControl.Description =
                "Select the folder where the conversion results get generated.\n" +
                "The COLLADA '.dae' files get copied to this folder, where the conversion takes place.\n" +
                "The conversion output is created here, then copied to the repository.";

            WorkingFolderControl.FolderChanged += OnWorkingFolderChanged;
        }

        private void OnWorkingFolderChanged( object sender, DirectoryControl.StringEventArgs args )
        {
            // TODO: Hmmm, maybe nothing to do... This is just a hack until config is saved.
        }
    }
}
