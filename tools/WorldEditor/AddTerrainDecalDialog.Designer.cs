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

namespace Multiverse.Tools.WorldEditor
{
    partial class AddTerrainDecalDialog
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
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sizeXTextBox = new System.Windows.Forms.TextBox();
            this.sizeXLabel = new System.Windows.Forms.Label();
            this.sizeZTextBox = new System.Windows.Forms.TextBox();
            this.sizeZlabel = new System.Windows.Forms.Label();
            this.priorityLabel = new System.Windows.Forms.Label();
            this.priorityTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.pickDecalImageButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(78, 36);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(192, 20);
            this.filenameTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Filename:";
            // 
            // sizeXTextBox
            // 
            this.sizeXTextBox.Location = new System.Drawing.Point(78, 64);
            this.sizeXTextBox.Name = "sizeXTextBox";
            this.sizeXTextBox.Size = new System.Drawing.Size(125, 20);
            this.sizeXTextBox.TabIndex = 2;
            this.sizeXTextBox.TextChanged += new System.EventHandler(this.floatVerifyevent);
            // 
            // sizeXLabel
            // 
            this.sizeXLabel.AutoSize = true;
            this.sizeXLabel.Location = new System.Drawing.Point(12, 67);
            this.sizeXLabel.Name = "sizeXLabel";
            this.sizeXLabel.Size = new System.Drawing.Size(40, 13);
            this.sizeXLabel.TabIndex = 11;
            this.sizeXLabel.Text = "Size X:";
            // 
            // sizeZTextBox
            // 
            this.sizeZTextBox.Location = new System.Drawing.Point(78, 91);
            this.sizeZTextBox.Name = "sizeZTextBox";
            this.sizeZTextBox.Size = new System.Drawing.Size(125, 20);
            this.sizeZTextBox.TabIndex = 3;
            this.sizeZTextBox.TextChanged += new System.EventHandler(this.floatVerifyevent);
            // 
            // sizeZlabel
            // 
            this.sizeZlabel.AutoSize = true;
            this.sizeZlabel.Location = new System.Drawing.Point(12, 94);
            this.sizeZlabel.Name = "sizeZlabel";
            this.sizeZlabel.Size = new System.Drawing.Size(40, 13);
            this.sizeZlabel.TabIndex = 12;
            this.sizeZlabel.Text = "Size Z:";
            // 
            // priorityLabel
            // 
            this.priorityLabel.AutoSize = true;
            this.priorityLabel.Location = new System.Drawing.Point(12, 121);
            this.priorityLabel.Name = "priorityLabel";
            this.priorityLabel.Size = new System.Drawing.Size(41, 13);
            this.priorityLabel.TabIndex = 13;
            this.priorityLabel.Text = "Priority:";
            // 
            // priorityTextBox
            // 
            this.priorityTextBox.Location = new System.Drawing.Point(78, 118);
            this.priorityTextBox.Name = "priorityTextBox";
            this.priorityTextBox.Size = new System.Drawing.Size(125, 20);
            this.priorityTextBox.TabIndex = 4;
            this.priorityTextBox.TextChanged += new System.EventHandler(this.intVerifyevent);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(15, 144);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "&Add";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(208, 144);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 6;
            this.helpButton.Tag = "TerrainDecal";
            this.helpButton.Text = "&Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.onHelpButtonClicked);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(289, 144);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 13);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(38, 13);
            this.nameLabel.TabIndex = 8;
            this.nameLabel.Text = "Name:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(78, 10);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(286, 20);
            this.nameTextBox.TabIndex = 0;
            // 
            // pickDecalImageButton
            // 
            this.pickDecalImageButton.Location = new System.Drawing.Point(276, 34);
            this.pickDecalImageButton.Name = "pickDecalImageButton";
            this.pickDecalImageButton.Size = new System.Drawing.Size(70, 23);
            this.pickDecalImageButton.TabIndex = 14;
            this.pickDecalImageButton.Text = "Pick Image";
            this.pickDecalImageButton.UseVisualStyleBackColor = true;
            this.pickDecalImageButton.Click += new System.EventHandler(this.pickDecalImageButton_Click);
            // 
            // AddTerrainDecalDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(376, 172);
            this.Controls.Add(this.pickDecalImageButton);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.priorityTextBox);
            this.Controls.Add(this.priorityLabel);
            this.Controls.Add(this.sizeZlabel);
            this.Controls.Add(this.sizeZTextBox);
            this.Controls.Add(this.sizeXLabel);
            this.Controls.Add(this.sizeXTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.filenameTextBox);
            this.Name = "AddTerrainDecalDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Terrain Decal";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sizeXTextBox;
        private System.Windows.Forms.Label sizeXLabel;
        private System.Windows.Forms.TextBox sizeZTextBox;
        private System.Windows.Forms.Label sizeZlabel;
        private System.Windows.Forms.Label priorityLabel;
        private System.Windows.Forms.TextBox priorityTextBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button pickDecalImageButton;
    }
}
