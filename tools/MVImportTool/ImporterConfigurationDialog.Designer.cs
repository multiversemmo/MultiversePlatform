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
    partial class ImporterConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ImporterConfigurationDialog ) );
            this.importerConfiguration1 = new MVImportTool.ImporterConfiguration();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // importerConfiguration1
            // 
            this.importerConfiguration1.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.importerConfiguration1.ConversionToolExeFile = "";
            this.importerConfiguration1.Location = new System.Drawing.Point( 0, 0 );
            this.importerConfiguration1.Name = "importerConfiguration1";
            this.importerConfiguration1.Size = new System.Drawing.Size( 342, 110 );
            this.importerConfiguration1.TabIndex = 0;
            this.importerConfiguration1.WorkingFolder = "";
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point( 255, 111 );
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size( 75, 23 );
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // OkButton
            // 
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkButton.Location = new System.Drawing.Point( 174, 111 );
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size( 75, 23 );
            this.OkButton.TabIndex = 2;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler( this.OkButton_Click );
            // 
            // ImporterConfigurationDialog
            // 
            this.ClientSize = new System.Drawing.Size( 342, 146 );
            this.Controls.Add( this.OkButton );
            this.Controls.Add( this.CancelButton );
            this.Controls.Add( this.importerConfiguration1 );
            this.Icon = ((System.Drawing.Icon) (resources.GetObject( "$this.Icon" )));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size( 300, 180 );
            this.Name = "ImporterConfigurationDialog";
            this.Load += new System.EventHandler( this.ImporterConfigurationDialog_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private global::MVImportTool.ImporterConfiguration importerConfiguration1;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OkButton;
    }
}
