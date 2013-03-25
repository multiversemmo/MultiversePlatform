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

namespace Multiverse.Tools.TerrainAssembler
{
    partial class LoadLayerDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.zoneNameLabel = new System.Windows.Forms.Label();
            this.zoneNameValueLabel = new System.Windows.Forms.Label();
            this.layerLabel = new System.Windows.Forms.Label();
            this.layerComboBox = new System.Windows.Forms.ComboBox();
            this.imageNameLabel = new System.Windows.Forms.Label();
            this.imageNameTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.loadButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.metersPerPixelLabel = new System.Windows.Forms.Label();
            this.metersPerPixelComboBox = new System.Windows.Forms.ComboBox();
            this.validationErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.validationErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // zoneNameLabel
            // 
            this.zoneNameLabel.AutoSize = true;
            this.zoneNameLabel.Location = new System.Drawing.Point(25, 25);
            this.zoneNameLabel.Name = "zoneNameLabel";
            this.zoneNameLabel.Size = new System.Drawing.Size(66, 13);
            this.zoneNameLabel.TabIndex = 0;
            this.zoneNameLabel.Text = "Zone Name:";
            // 
            // zoneNameValueLabel
            // 
            this.zoneNameValueLabel.AutoSize = true;
            this.zoneNameValueLabel.Location = new System.Drawing.Point(121, 25);
            this.zoneNameValueLabel.Name = "zoneNameValueLabel";
            this.zoneNameValueLabel.Size = new System.Drawing.Size(12, 13);
            this.zoneNameValueLabel.TabIndex = 1;
            this.zoneNameValueLabel.Text = "x";
            // 
            // layerLabel
            // 
            this.layerLabel.AutoSize = true;
            this.layerLabel.Location = new System.Drawing.Point(25, 62);
            this.layerLabel.Name = "layerLabel";
            this.layerLabel.Size = new System.Drawing.Size(36, 13);
            this.layerLabel.TabIndex = 2;
            this.layerLabel.Text = "Layer:";
            // 
            // layerComboBox
            // 
            this.layerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layerComboBox.FormattingEnabled = true;
            this.layerComboBox.Location = new System.Drawing.Point(124, 59);
            this.layerComboBox.Name = "layerComboBox";
            this.layerComboBox.Size = new System.Drawing.Size(135, 21);
            this.layerComboBox.TabIndex = 3;
            // 
            // imageNameLabel
            // 
            this.imageNameLabel.AutoSize = true;
            this.imageNameLabel.Location = new System.Drawing.Point(25, 107);
            this.imageNameLabel.Name = "imageNameLabel";
            this.imageNameLabel.Size = new System.Drawing.Size(70, 13);
            this.imageNameLabel.TabIndex = 4;
            this.imageNameLabel.Text = "Image Name:";
            // 
            // imageNameTextBox
            // 
            this.imageNameTextBox.Location = new System.Drawing.Point(124, 104);
            this.imageNameTextBox.Name = "imageNameTextBox";
            this.imageNameTextBox.Size = new System.Drawing.Size(361, 20);
            this.imageNameTextBox.TabIndex = 5;
            this.imageNameTextBox.Validated += new System.EventHandler(this.imageNameTextBox_Validated);
            this.imageNameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.imageNameTextBox_Validating);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(517, 102);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 6;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // loadButton
            // 
            this.loadButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.loadButton.Enabled = false;
            this.loadButton.Location = new System.Drawing.Point(28, 300);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 23);
            this.loadButton.TabIndex = 7;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(410, 300);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 8;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(517, 300);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // metersPerPixelLabel
            // 
            this.metersPerPixelLabel.AutoSize = true;
            this.metersPerPixelLabel.Location = new System.Drawing.Point(25, 152);
            this.metersPerPixelLabel.Name = "metersPerPixelLabel";
            this.metersPerPixelLabel.Size = new System.Drawing.Size(86, 13);
            this.metersPerPixelLabel.TabIndex = 10;
            this.metersPerPixelLabel.Text = "Meters Per Pixel:";
            // 
            // metersPerPixelComboBox
            // 
            this.metersPerPixelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.metersPerPixelComboBox.FormattingEnabled = true;
            this.metersPerPixelComboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16",
            "32",
            "64"});
            this.metersPerPixelComboBox.Location = new System.Drawing.Point(124, 149);
            this.metersPerPixelComboBox.Name = "metersPerPixelComboBox";
            this.metersPerPixelComboBox.Size = new System.Drawing.Size(135, 21);
            this.metersPerPixelComboBox.TabIndex = 11;
            this.metersPerPixelComboBox.SelectedIndexChanged += new System.EventHandler(this.metersPerPixelComboBox_SelectedIndexChanged);
            // 
            // validationErrorProvider
            // 
            this.validationErrorProvider.ContainerControl = this;
            // 
            // LoadLayerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 347);
            this.Controls.Add(this.metersPerPixelComboBox);
            this.Controls.Add(this.metersPerPixelLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.imageNameTextBox);
            this.Controls.Add(this.imageNameLabel);
            this.Controls.Add(this.layerComboBox);
            this.Controls.Add(this.layerLabel);
            this.Controls.Add(this.zoneNameValueLabel);
            this.Controls.Add(this.zoneNameLabel);
            this.Name = "LoadLayerDialog";
            this.Text = "Load a layer";
            ((System.ComponentModel.ISupportInitialize)(this.validationErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label zoneNameLabel;
        private System.Windows.Forms.Label zoneNameValueLabel;
        private System.Windows.Forms.Label layerLabel;
        private System.Windows.Forms.ComboBox layerComboBox;
        private System.Windows.Forms.Label imageNameLabel;
        private System.Windows.Forms.TextBox imageNameTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label metersPerPixelLabel;
        private System.Windows.Forms.ComboBox metersPerPixelComboBox;
        private System.Windows.Forms.ErrorProvider validationErrorProvider;
    }
}
