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
    partial class InstallationPreviewDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( InstallationPreviewDialog ) );
            this.TargetsCheckListBox = new System.Windows.Forms.CheckedListBox();
            this.ExplanitoryLabel = new System.Windows.Forms.Label();
            this.CancelButton = new System.Windows.Forms.Button();
            this.AcceptButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TargetsCheckListBox
            // 
            this.TargetsCheckListBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TargetsCheckListBox.CheckOnClick = true;
            this.TargetsCheckListBox.FormattingEnabled = true;
            this.TargetsCheckListBox.Location = new System.Drawing.Point( 3, 74 );
            this.TargetsCheckListBox.Name = "TargetsCheckListBox";
            this.TargetsCheckListBox.Size = new System.Drawing.Size( 362, 259 );
            this.TargetsCheckListBox.TabIndex = 0;
            // 
            // ExplanitoryLabel
            // 
            this.ExplanitoryLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ExplanitoryLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ExplanitoryLabel.Location = new System.Drawing.Point( 3, 9 );
            this.ExplanitoryLabel.Name = "ExplanitoryLabel";
            this.ExplanitoryLabel.Size = new System.Drawing.Size( 362, 56 );
            this.ExplanitoryLabel.TabIndex = 1;
            this.ExplanitoryLabel.Text = "Explanation Text Goes Here";
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point( 290, 346 );
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size( 75, 23 );
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "&Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler( this.CancelButton_Click );
            // 
            // AcceptButton
            // 
            this.AcceptButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AcceptButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.AcceptButton.Location = new System.Drawing.Point( 210, 346 );
            this.AcceptButton.Name = "AcceptButton";
            this.AcceptButton.Size = new System.Drawing.Size( 74, 23 );
            this.AcceptButton.TabIndex = 3;
            this.AcceptButton.Text = "&OK";
            this.AcceptButton.UseVisualStyleBackColor = true;
            this.AcceptButton.Click += new System.EventHandler( this.AcceptButton_Click );
            // 
            // InstallationPreviewDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 367, 371 );
            this.Controls.Add( this.AcceptButton );
            this.Controls.Add( this.CancelButton );
            this.Controls.Add( this.ExplanitoryLabel );
            this.Controls.Add( this.TargetsCheckListBox );
            this.Icon = ((System.Drawing.Icon) (resources.GetObject( "$this.Icon" )));
            this.Name = "InstallationPreviewDialog";
            this.Text = "Copy Files Preview";
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.CheckedListBox TargetsCheckListBox;
        private System.Windows.Forms.Label ExplanitoryLabel;
        private new System.Windows.Forms.Button CancelButton;
        private new System.Windows.Forms.Button AcceptButton;
    }
}
