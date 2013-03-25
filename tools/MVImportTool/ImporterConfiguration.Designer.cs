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

namespace MVImportTool
{
    partial class ImporterConfiguration
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExeFileControl = new MVImportTool.FileControl();
            this.WorkingFolderControl = new MVImportTool.DirectoryControl();
            this.SuspendLayout();
            // 
            // ExeFileControl
            // 
            this.ExeFileControl.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ExeFileControl.ButtonToolTip = "Browse for the program that converts COLLADA files.";
            this.ExeFileControl.FileName = "";
            this.ExeFileControl.Filter = "";
            this.ExeFileControl.InitialDirectory = "";
            this.ExeFileControl.Label = "Conversion Tool executable file";
            this.ExeFileControl.Location = new System.Drawing.Point( 3, 3 );
            this.ExeFileControl.Multiselect = false;
            this.ExeFileControl.Name = "ExeFileControl";
            this.ExeFileControl.Size = new System.Drawing.Size( 320, 46 );
            this.ExeFileControl.TabIndex = 2;
            this.ExeFileControl.Title = "Conversion Tool Selector";
            // 
            // WorkingFolderControl
            // 
            this.WorkingFolderControl.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.WorkingFolderControl.ButtonToolTip = "Browse for a folder to contain intermediate results from the conversion process.";
            this.WorkingFolderControl.Description = "";
            this.WorkingFolderControl.FolderPath = "";
            this.WorkingFolderControl.Label = "Working Folder (intermediate files)";
            this.WorkingFolderControl.Location = new System.Drawing.Point( 3, 55 );
            this.WorkingFolderControl.Name = "WorkingFolderControl";
            this.WorkingFolderControl.Size = new System.Drawing.Size( 320, 47 );
            this.WorkingFolderControl.TabIndex = 1;
            // 
            // ImporterConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.WorkingFolderControl );
            this.Controls.Add( this.ExeFileControl );
            this.Name = "ImporterConfiguration";
            this.Size = new System.Drawing.Size( 326, 123 );
            this.Load += new System.EventHandler( this.ImporterConfiguration_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private DirectoryControl WorkingFolderControl;
        private FileControl ExeFileControl;

    }
}
