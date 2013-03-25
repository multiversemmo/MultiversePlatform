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
    partial class NewMapDialog
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
            this.mapNameLabel = new System.Windows.Forms.Label();
            this.mapNameTextBox = new System.Windows.Forms.TextBox();
            this.mapWidthLabel = new System.Windows.Forms.Label();
            this.mapWidthTextBox = new System.Windows.Forms.TextBox();
            this.widthUnitsComboBox = new System.Windows.Forms.ComboBox();
            this.mapHeightLabel = new System.Windows.Forms.Label();
            this.mapHeightTextBox = new System.Windows.Forms.TextBox();
            this.heightUnitsComboBox = new System.Windows.Forms.ComboBox();
            this.minHeightLabel = new System.Windows.Forms.Label();
            this.minHeightTextBox = new System.Windows.Forms.TextBox();
            this.minHeightUnitsComboBox = new System.Windows.Forms.ComboBox();
            this.maxHeightLabel = new System.Windows.Forms.Label();
            this.maxHeightTextBox = new System.Windows.Forms.TextBox();
            this.maxHeightUnitsComboBox = new System.Windows.Forms.ComboBox();
            this.defaultHeightLabel = new System.Windows.Forms.Label();
            this.defaultHeightTextBox = new System.Windows.Forms.TextBox();
            this.defaultHeightUnitsComboBox = new System.Windows.Forms.ComboBox();
            this.createButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // mapNameLabel
            // 
            this.mapNameLabel.AutoSize = true;
            this.mapNameLabel.Location = new System.Drawing.Point(28, 35);
            this.mapNameLabel.Name = "mapNameLabel";
            this.mapNameLabel.Size = new System.Drawing.Size(62, 13);
            this.mapNameLabel.TabIndex = 0;
            this.mapNameLabel.Text = "Map Name:";
            // 
            // mapNameTextBox
            // 
            this.mapNameTextBox.Location = new System.Drawing.Point(169, 28);
            this.mapNameTextBox.Name = "mapNameTextBox";
            this.mapNameTextBox.Size = new System.Drawing.Size(164, 20);
            this.mapNameTextBox.TabIndex = 1;
            this.mapNameTextBox.Validated += new System.EventHandler(this.mapNameTextBox_Validated);
            this.mapNameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mapNameTextBox_Validating);
            // 
            // mapWidthLabel
            // 
            this.mapWidthLabel.AutoSize = true;
            this.mapWidthLabel.Location = new System.Drawing.Point(28, 77);
            this.mapWidthLabel.Name = "mapWidthLabel";
            this.mapWidthLabel.Size = new System.Drawing.Size(62, 13);
            this.mapWidthLabel.TabIndex = 2;
            this.mapWidthLabel.Text = "Map Width:";
            // 
            // mapWidthTextBox
            // 
            this.mapWidthTextBox.Location = new System.Drawing.Point(169, 74);
            this.mapWidthTextBox.Name = "mapWidthTextBox";
            this.mapWidthTextBox.Size = new System.Drawing.Size(100, 20);
            this.mapWidthTextBox.TabIndex = 3;
            this.mapWidthTextBox.Text = "32";
            this.mapWidthTextBox.Validated += new System.EventHandler(this.mapWidthTextBox_Validated);
            this.mapWidthTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mapWidthTextBox_Validating);
            // 
            // widthUnitsComboBox
            // 
            this.widthUnitsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.widthUnitsComboBox.FormattingEnabled = true;
            this.widthUnitsComboBox.Items.AddRange(new object[] {
            "Meters",
            "Kilometers"});
            this.widthUnitsComboBox.Location = new System.Drawing.Point(301, 74);
            this.widthUnitsComboBox.Name = "widthUnitsComboBox";
            this.widthUnitsComboBox.Size = new System.Drawing.Size(121, 21);
            this.widthUnitsComboBox.TabIndex = 4;
            // 
            // mapHeightLabel
            // 
            this.mapHeightLabel.AutoSize = true;
            this.mapHeightLabel.Location = new System.Drawing.Point(28, 121);
            this.mapHeightLabel.Name = "mapHeightLabel";
            this.mapHeightLabel.Size = new System.Drawing.Size(65, 13);
            this.mapHeightLabel.TabIndex = 5;
            this.mapHeightLabel.Text = "Map Height:";
            // 
            // mapHeightTextBox
            // 
            this.mapHeightTextBox.Location = new System.Drawing.Point(169, 118);
            this.mapHeightTextBox.Name = "mapHeightTextBox";
            this.mapHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.mapHeightTextBox.TabIndex = 6;
            this.mapHeightTextBox.Text = "32";
            this.mapHeightTextBox.Validated += new System.EventHandler(this.mapHeightTextBox_Validated);
            this.mapHeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mapHeightTextBox_Validating);
            // 
            // heightUnitsComboBox
            // 
            this.heightUnitsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.heightUnitsComboBox.FormattingEnabled = true;
            this.heightUnitsComboBox.Items.AddRange(new object[] {
            "Meters",
            "Kilometers"});
            this.heightUnitsComboBox.Location = new System.Drawing.Point(301, 118);
            this.heightUnitsComboBox.Name = "heightUnitsComboBox";
            this.heightUnitsComboBox.Size = new System.Drawing.Size(121, 21);
            this.heightUnitsComboBox.TabIndex = 7;
            // 
            // minHeightLabel
            // 
            this.minHeightLabel.AutoSize = true;
            this.minHeightLabel.Location = new System.Drawing.Point(28, 167);
            this.minHeightLabel.Name = "minHeightLabel";
            this.minHeightLabel.Size = new System.Drawing.Size(125, 13);
            this.minHeightLabel.TabIndex = 8;
            this.minHeightLabel.Text = "Minumum Terrain Height:";
            // 
            // minHeightTextBox
            // 
            this.minHeightTextBox.Location = new System.Drawing.Point(169, 164);
            this.minHeightTextBox.Name = "minHeightTextBox";
            this.minHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.minHeightTextBox.TabIndex = 9;
            this.minHeightTextBox.Text = "-100";
            this.minHeightTextBox.Validated += new System.EventHandler(this.minHeightTextBox_Validated);
            this.minHeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.minHeightTextBox_Validating);
            // 
            // minHeightUnitsComboBox
            // 
            this.minHeightUnitsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.minHeightUnitsComboBox.FormattingEnabled = true;
            this.minHeightUnitsComboBox.Items.AddRange(new object[] {
            "Millimeters",
            "Meters"});
            this.minHeightUnitsComboBox.Location = new System.Drawing.Point(301, 163);
            this.minHeightUnitsComboBox.Name = "minHeightUnitsComboBox";
            this.minHeightUnitsComboBox.Size = new System.Drawing.Size(121, 21);
            this.minHeightUnitsComboBox.TabIndex = 10;
            // 
            // maxHeightLabel
            // 
            this.maxHeightLabel.AutoSize = true;
            this.maxHeightLabel.Location = new System.Drawing.Point(28, 210);
            this.maxHeightLabel.Name = "maxHeightLabel";
            this.maxHeightLabel.Size = new System.Drawing.Size(124, 13);
            this.maxHeightLabel.TabIndex = 11;
            this.maxHeightLabel.Text = "Maximum Terrain Height:";
            // 
            // maxHeightTextBox
            // 
            this.maxHeightTextBox.Location = new System.Drawing.Point(169, 207);
            this.maxHeightTextBox.Name = "maxHeightTextBox";
            this.maxHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.maxHeightTextBox.TabIndex = 12;
            this.maxHeightTextBox.Text = "2000";
            this.maxHeightTextBox.Validated += new System.EventHandler(this.maxHeightTextBox_Validated);
            this.maxHeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.maxHeightTextBox_Validating);
            // 
            // maxHeightUnitsComboBox
            // 
            this.maxHeightUnitsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maxHeightUnitsComboBox.FormattingEnabled = true;
            this.maxHeightUnitsComboBox.Items.AddRange(new object[] {
            "Millimeters",
            "Meters"});
            this.maxHeightUnitsComboBox.Location = new System.Drawing.Point(301, 207);
            this.maxHeightUnitsComboBox.Name = "maxHeightUnitsComboBox";
            this.maxHeightUnitsComboBox.Size = new System.Drawing.Size(121, 21);
            this.maxHeightUnitsComboBox.TabIndex = 13;
            // 
            // defaultHeightLabel
            // 
            this.defaultHeightLabel.AutoSize = true;
            this.defaultHeightLabel.Location = new System.Drawing.Point(28, 255);
            this.defaultHeightLabel.Name = "defaultHeightLabel";
            this.defaultHeightLabel.Size = new System.Drawing.Size(114, 13);
            this.defaultHeightLabel.TabIndex = 14;
            this.defaultHeightLabel.Text = "Default Terrain Height:";
            // 
            // defaultHeightTextBox
            // 
            this.defaultHeightTextBox.Location = new System.Drawing.Point(169, 252);
            this.defaultHeightTextBox.Name = "defaultHeightTextBox";
            this.defaultHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.defaultHeightTextBox.TabIndex = 15;
            this.defaultHeightTextBox.Text = "50";
            this.defaultHeightTextBox.Validated += new System.EventHandler(this.defaultHeightTextBox_Validated);
            this.defaultHeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.defaultHeightTextBox_Validating);
            // 
            // defaultHeightUnitsComboBox
            // 
            this.defaultHeightUnitsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultHeightUnitsComboBox.FormattingEnabled = true;
            this.defaultHeightUnitsComboBox.Items.AddRange(new object[] {
            "Millimeters",
            "Meters"});
            this.defaultHeightUnitsComboBox.Location = new System.Drawing.Point(301, 251);
            this.defaultHeightUnitsComboBox.Name = "defaultHeightUnitsComboBox";
            this.defaultHeightUnitsComboBox.Size = new System.Drawing.Size(121, 21);
            this.defaultHeightUnitsComboBox.TabIndex = 16;
            // 
            // createButton
            // 
            this.createButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.createButton.Location = new System.Drawing.Point(31, 322);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(75, 23);
            this.createButton.TabIndex = 17;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(243, 322);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 18;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(347, 322);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 19;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // NewMapDialog
            // 
            this.AcceptButton = this.createButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 374);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.defaultHeightUnitsComboBox);
            this.Controls.Add(this.defaultHeightTextBox);
            this.Controls.Add(this.defaultHeightLabel);
            this.Controls.Add(this.maxHeightUnitsComboBox);
            this.Controls.Add(this.maxHeightTextBox);
            this.Controls.Add(this.maxHeightLabel);
            this.Controls.Add(this.minHeightUnitsComboBox);
            this.Controls.Add(this.minHeightTextBox);
            this.Controls.Add(this.minHeightLabel);
            this.Controls.Add(this.heightUnitsComboBox);
            this.Controls.Add(this.mapHeightTextBox);
            this.Controls.Add(this.mapHeightLabel);
            this.Controls.Add(this.widthUnitsComboBox);
            this.Controls.Add(this.mapWidthTextBox);
            this.Controls.Add(this.mapWidthLabel);
            this.Controls.Add(this.mapNameTextBox);
            this.Controls.Add(this.mapNameLabel);
            this.Name = "NewMapDialog";
            this.Text = "Create a new map";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mapNameLabel;
        private System.Windows.Forms.TextBox mapNameTextBox;
        private System.Windows.Forms.Label mapWidthLabel;
        private System.Windows.Forms.TextBox mapWidthTextBox;
        private System.Windows.Forms.ComboBox widthUnitsComboBox;
        private System.Windows.Forms.Label mapHeightLabel;
        private System.Windows.Forms.TextBox mapHeightTextBox;
        private System.Windows.Forms.ComboBox heightUnitsComboBox;
        private System.Windows.Forms.Label minHeightLabel;
        private System.Windows.Forms.TextBox minHeightTextBox;
        private System.Windows.Forms.ComboBox minHeightUnitsComboBox;
        private System.Windows.Forms.Label maxHeightLabel;
        private System.Windows.Forms.TextBox maxHeightTextBox;
        private System.Windows.Forms.ComboBox maxHeightUnitsComboBox;
        private System.Windows.Forms.Label defaultHeightLabel;
        private System.Windows.Forms.TextBox defaultHeightTextBox;
        private System.Windows.Forms.ComboBox defaultHeightUnitsComboBox;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}
