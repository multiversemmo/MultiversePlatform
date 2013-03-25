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
    partial class NewZoneDialog
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
            this.heightmapImageLabel = new System.Windows.Forms.Label();
            this.heightmapImageTextBox = new System.Windows.Forms.TextBox();
            this.browseHeightmapButton = new System.Windows.Forms.Button();
            this.mpsLabel = new System.Windows.Forms.Label();
            this.mpsComboBox = new System.Windows.Forms.ComboBox();
            this.zoneNameLabel = new System.Windows.Forms.Label();
            this.zoneNameTextBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.maxHeightTextBox = new System.Windows.Forms.TextBox();
            this.minHeightTextBox = new System.Windows.Forms.TextBox();
            this.maxHeightLabel = new System.Windows.Forms.Label();
            this.minHeightLabel = new System.Windows.Forms.Label();
            this.zoneSizeValueLabel = new System.Windows.Forms.Label();
            this.zoneSizeLabel = new System.Windows.Forms.Label();
            this.heightmapSizeValueLabel = new System.Windows.Forms.Label();
            this.heightmapSizeLabel = new System.Windows.Forms.Label();
            this.createButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.validationErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.validationErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // heightmapImageLabel
            // 
            this.heightmapImageLabel.AutoSize = true;
            this.heightmapImageLabel.Location = new System.Drawing.Point(22, 64);
            this.heightmapImageLabel.Name = "heightmapImageLabel";
            this.heightmapImageLabel.Size = new System.Drawing.Size(92, 13);
            this.heightmapImageLabel.TabIndex = 2;
            this.heightmapImageLabel.Text = "Heightmap image:";
            // 
            // heightmapImageTextBox
            // 
            this.heightmapImageTextBox.Location = new System.Drawing.Point(135, 61);
            this.heightmapImageTextBox.Name = "heightmapImageTextBox";
            this.heightmapImageTextBox.Size = new System.Drawing.Size(345, 20);
            this.heightmapImageTextBox.TabIndex = 3;
            this.heightmapImageTextBox.Validated += new System.EventHandler(this.heightmapImageTextBox_Validated);
            this.heightmapImageTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.heightmapImageTextBox_Validating);
            // 
            // browseHeightmapButton
            // 
            this.browseHeightmapButton.CausesValidation = false;
            this.browseHeightmapButton.Location = new System.Drawing.Point(508, 59);
            this.browseHeightmapButton.Name = "browseHeightmapButton";
            this.browseHeightmapButton.Size = new System.Drawing.Size(75, 23);
            this.browseHeightmapButton.TabIndex = 4;
            this.browseHeightmapButton.Text = "Browse...";
            this.browseHeightmapButton.UseVisualStyleBackColor = true;
            this.browseHeightmapButton.Click += new System.EventHandler(this.browseHeightmapButton_Click);
            // 
            // mpsLabel
            // 
            this.mpsLabel.AutoSize = true;
            this.mpsLabel.Location = new System.Drawing.Point(22, 104);
            this.mpsLabel.Name = "mpsLabel";
            this.mpsLabel.Size = new System.Drawing.Size(84, 13);
            this.mpsLabel.TabIndex = 5;
            this.mpsLabel.Text = "Meters per pixel:";
            // 
            // mpsComboBox
            // 
            this.mpsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mpsComboBox.FormattingEnabled = true;
            this.mpsComboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16",
            "32"});
            this.mpsComboBox.Location = new System.Drawing.Point(135, 101);
            this.mpsComboBox.Name = "mpsComboBox";
            this.mpsComboBox.Size = new System.Drawing.Size(121, 21);
            this.mpsComboBox.TabIndex = 6;
            this.mpsComboBox.SelectedIndexChanged += new System.EventHandler(this.mpsComboBox_SelectedIndexChanged);
            // 
            // zoneNameLabel
            // 
            this.zoneNameLabel.AutoSize = true;
            this.zoneNameLabel.Location = new System.Drawing.Point(22, 23);
            this.zoneNameLabel.Name = "zoneNameLabel";
            this.zoneNameLabel.Size = new System.Drawing.Size(64, 13);
            this.zoneNameLabel.TabIndex = 0;
            this.zoneNameLabel.Text = "Zone name:";
            // 
            // zoneNameTextBox
            // 
            this.zoneNameTextBox.Location = new System.Drawing.Point(135, 20);
            this.zoneNameTextBox.Name = "zoneNameTextBox";
            this.zoneNameTextBox.Size = new System.Drawing.Size(175, 20);
            this.zoneNameTextBox.TabIndex = 1;
            this.zoneNameTextBox.Validated += new System.EventHandler(this.zoneNameTextBox_Validated);
            this.zoneNameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.zoneNameTextBox_Validating);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.maxHeightTextBox);
            this.panel1.Controls.Add(this.minHeightTextBox);
            this.panel1.Controls.Add(this.maxHeightLabel);
            this.panel1.Controls.Add(this.minHeightLabel);
            this.panel1.Controls.Add(this.zoneSizeValueLabel);
            this.panel1.Controls.Add(this.zoneSizeLabel);
            this.panel1.Controls.Add(this.heightmapSizeValueLabel);
            this.panel1.Controls.Add(this.heightmapSizeLabel);
            this.panel1.Location = new System.Drawing.Point(12, 144);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(571, 138);
            this.panel1.TabIndex = 7;
            // 
            // maxHeightTextBox
            // 
            this.maxHeightTextBox.Location = new System.Drawing.Point(228, 45);
            this.maxHeightTextBox.Name = "maxHeightTextBox";
            this.maxHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.maxHeightTextBox.TabIndex = 10;
            this.maxHeightTextBox.Validated += new System.EventHandler(this.maxHeightTextBox_Validated);
            this.maxHeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.maxHeightTextBox_Validating);
            // 
            // minHeightTextBox
            // 
            this.minHeightTextBox.Location = new System.Drawing.Point(228, 16);
            this.minHeightTextBox.Name = "minHeightTextBox";
            this.minHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.minHeightTextBox.TabIndex = 8;
            this.minHeightTextBox.Validated += new System.EventHandler(this.minHeightTextBox_Validated);
            this.minHeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.minHeightTextBox_Validating);
            // 
            // maxHeightLabel
            // 
            this.maxHeightLabel.AutoSize = true;
            this.maxHeightLabel.Location = new System.Drawing.Point(10, 48);
            this.maxHeightLabel.Name = "maxHeightLabel";
            this.maxHeightLabel.Size = new System.Drawing.Size(204, 13);
            this.maxHeightLabel.TabIndex = 9;
            this.maxHeightLabel.Text = "Maximum height of imported map (meters):";
            // 
            // minHeightLabel
            // 
            this.minHeightLabel.AutoSize = true;
            this.minHeightLabel.Location = new System.Drawing.Point(10, 19);
            this.minHeightLabel.Name = "minHeightLabel";
            this.minHeightLabel.Size = new System.Drawing.Size(201, 13);
            this.minHeightLabel.TabIndex = 7;
            this.minHeightLabel.Text = "Minimum height of imported map (meters):";
            // 
            // zoneSizeValueLabel
            // 
            this.zoneSizeValueLabel.AutoSize = true;
            this.zoneSizeValueLabel.Location = new System.Drawing.Point(225, 107);
            this.zoneSizeValueLabel.Name = "zoneSizeValueLabel";
            this.zoneSizeValueLabel.Size = new System.Drawing.Size(24, 13);
            this.zoneSizeValueLabel.TabIndex = 14;
            this.zoneSizeValueLabel.Text = "0x0";
            // 
            // zoneSizeLabel
            // 
            this.zoneSizeLabel.AutoSize = true;
            this.zoneSizeLabel.Location = new System.Drawing.Point(10, 107);
            this.zoneSizeLabel.Name = "zoneSizeLabel";
            this.zoneSizeLabel.Size = new System.Drawing.Size(85, 13);
            this.zoneSizeLabel.TabIndex = 13;
            this.zoneSizeLabel.Text = "Zone Size (tiles):";
            // 
            // heightmapSizeValueLabel
            // 
            this.heightmapSizeValueLabel.AutoSize = true;
            this.heightmapSizeValueLabel.Location = new System.Drawing.Point(225, 77);
            this.heightmapSizeValueLabel.Name = "heightmapSizeValueLabel";
            this.heightmapSizeValueLabel.Size = new System.Drawing.Size(24, 13);
            this.heightmapSizeValueLabel.TabIndex = 12;
            this.heightmapSizeValueLabel.Text = "0x0";
            // 
            // heightmapSizeLabel
            // 
            this.heightmapSizeLabel.AutoSize = true;
            this.heightmapSizeLabel.Location = new System.Drawing.Point(10, 77);
            this.heightmapSizeLabel.Name = "heightmapSizeLabel";
            this.heightmapSizeLabel.Size = new System.Drawing.Size(117, 13);
            this.heightmapSizeLabel.TabIndex = 11;
            this.heightmapSizeLabel.Text = "Heightmap size (pixels):";
            // 
            // createButton
            // 
            this.createButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.createButton.Enabled = false;
            this.createButton.Location = new System.Drawing.Point(25, 303);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(75, 23);
            this.createButton.TabIndex = 15;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(405, 303);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 16;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(508, 303);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 17;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // validationErrorProvider
            // 
            this.validationErrorProvider.ContainerControl = this;
            // 
            // NewZoneDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 347);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.zoneNameTextBox);
            this.Controls.Add(this.zoneNameLabel);
            this.Controls.Add(this.mpsComboBox);
            this.Controls.Add(this.mpsLabel);
            this.Controls.Add(this.browseHeightmapButton);
            this.Controls.Add(this.heightmapImageTextBox);
            this.Controls.Add(this.heightmapImageLabel);
            this.Name = "NewZoneDialog";
            this.Text = "Create a new zone";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.validationErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label heightmapImageLabel;
        private System.Windows.Forms.TextBox heightmapImageTextBox;
        private System.Windows.Forms.Button browseHeightmapButton;
        private System.Windows.Forms.Label mpsLabel;
        private System.Windows.Forms.ComboBox mpsComboBox;
        private System.Windows.Forms.Label zoneNameLabel;
        private System.Windows.Forms.TextBox zoneNameTextBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label heightmapSizeLabel;
        private System.Windows.Forms.Label heightmapSizeValueLabel;
        private System.Windows.Forms.Label zoneSizeLabel;
        private System.Windows.Forms.Label zoneSizeValueLabel;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ErrorProvider validationErrorProvider;
        private System.Windows.Forms.Label minHeightLabel;
        private System.Windows.Forms.Label maxHeightLabel;
        private System.Windows.Forms.TextBox maxHeightTextBox;
        private System.Windows.Forms.TextBox minHeightTextBox;
    }
}
