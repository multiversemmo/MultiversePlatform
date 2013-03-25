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
    partial class RepositoryControl
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
            this.ChangeRepositoryButton = new System.Windows.Forms.Button();
            this.RepositoryListBox = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ChangeRepositoryButton
            // 
            this.ChangeRepositoryButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChangeRepositoryButton.AutoSize = true;
            this.ChangeRepositoryButton.Location = new System.Drawing.Point( 281, 278 );
            this.ChangeRepositoryButton.Name = "ChangeRepositoryButton";
            this.ChangeRepositoryButton.Size = new System.Drawing.Size( 126, 23 );
            this.ChangeRepositoryButton.TabIndex = 2;
            this.ChangeRepositoryButton.Text = "Change Repository List";
            this.ChangeRepositoryButton.UseVisualStyleBackColor = true;
            this.ChangeRepositoryButton.Click += new System.EventHandler( this.ChangeRepositoryButton_Click );
            // 
            // RepositoryListBox
            // 
            this.RepositoryListBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RepositoryListBox.CheckOnClick = true;
            this.RepositoryListBox.FormattingEnabled = true;
            this.RepositoryListBox.Location = new System.Drawing.Point( 3, 31 );
            this.RepositoryListBox.Name = "RepositoryListBox";
            this.RepositoryListBox.Size = new System.Drawing.Size( 404, 244 );
            this.RepositoryListBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 3, 9 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 162, 13 );
            this.label1.TabIndex = 0;
            this.label1.Text = "Check Target Asset Repositories";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RepositoryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.ChangeRepositoryButton );
            this.Controls.Add( this.RepositoryListBox );
            this.Controls.Add( this.label1 );
            this.Name = "RepositoryControl";
            this.Size = new System.Drawing.Size( 410, 304 );
            this.Load += new System.EventHandler( this.RepositoryControl_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ChangeRepositoryButton;
        private System.Windows.Forms.CheckedListBox RepositoryListBox;
        private System.Windows.Forms.Label label1;
    }
}
