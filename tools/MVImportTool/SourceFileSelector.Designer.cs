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
    partial class SourceFileSelector
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
            this.components = new System.ComponentModel.Container();
            this.SourceFileListBox = new System.Windows.Forms.CheckedListBox();
            this.ClearAllButton = new System.Windows.Forms.Button();
            this.CheckAllButton = new System.Windows.Forms.Button();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SourceFolderControl = new MVImportTool.DirectoryControl();
            this.ColladaFilesToolTip = new System.Windows.Forms.ToolTip( this.components );
            this.SuspendLayout();
            // 
            // SourceFileListBox
            // 
            this.SourceFileListBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SourceFileListBox.CheckOnClick = true;
            this.SourceFileListBox.FormattingEnabled = true;
            this.SourceFileListBox.Location = new System.Drawing.Point( 3, 56 );
            this.SourceFileListBox.Name = "SourceFileListBox";
            this.SourceFileListBox.Size = new System.Drawing.Size( 433, 304 );
            this.SourceFileListBox.TabIndex = 1;
            // 
            // ClearAllButton
            // 
            this.ClearAllButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClearAllButton.Location = new System.Drawing.Point( 84, 400 );
            this.ClearAllButton.Name = "ClearAllButton";
            this.ClearAllButton.Size = new System.Drawing.Size( 75, 23 );
            this.ClearAllButton.TabIndex = 5;
            this.ClearAllButton.Text = "Clear All";
            this.ClearAllButton.UseVisualStyleBackColor = true;
            this.ClearAllButton.Click += new System.EventHandler( this.ClearAllButton_Click );
            // 
            // CheckAllButton
            // 
            this.CheckAllButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CheckAllButton.Location = new System.Drawing.Point( 3, 400 );
            this.CheckAllButton.Name = "CheckAllButton";
            this.CheckAllButton.Size = new System.Drawing.Size( 75, 23 );
            this.CheckAllButton.TabIndex = 4;
            this.CheckAllButton.Text = "Check All";
            this.CheckAllButton.UseVisualStyleBackColor = true;
            this.CheckAllButton.Click += new System.EventHandler( this.CheckAllButton_Click );
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.AcceptsReturn = true;
            this.FilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterTextBox.Location = new System.Drawing.Point( 95, 371 );
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size( 341, 20 );
            this.FilterTextBox.TabIndex = 3;
            this.FilterTextBox.Text = "*.dae";
            this.FilterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler( this.FilterTextBox_KeyDown );
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 0, 374 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 89, 13 );
            this.label1.TabIndex = 2;
            this.label1.Text = "File Filter (regexp)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SourceFolderControl
            // 
            this.SourceFolderControl.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SourceFolderControl.ButtonToolTip = "Select a folder containing COLLADA (.dae) files.";
            this.SourceFolderControl.Description = "";
            this.SourceFolderControl.FolderPath = "";
            this.SourceFolderControl.Label = "COLLADA (.dae) Folder";
            this.SourceFolderControl.Location = new System.Drawing.Point( 3, 3 );
            this.SourceFolderControl.Name = "SourceFolderControl";
            this.SourceFolderControl.Size = new System.Drawing.Size( 433, 47 );
            this.SourceFolderControl.TabIndex = 0;
            // 
            // SourceFileSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.ClearAllButton );
            this.Controls.Add( this.CheckAllButton );
            this.Controls.Add( this.FilterTextBox );
            this.Controls.Add( this.SourceFolderControl );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.SourceFileListBox );
            this.Name = "SourceFileSelector";
            this.Size = new System.Drawing.Size( 439, 426 );
            this.Load += new System.EventHandler( this.SourceFileSelector_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox SourceFileListBox;
        private DirectoryControl SourceFolderControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.Button ClearAllButton;
        private System.Windows.Forms.Button CheckAllButton;
        private System.Windows.Forms.ToolTip ColladaFilesToolTip;
    }
}
