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
    partial class AddPointLightDialog
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
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.specularLabel = new System.Windows.Forms.Label();
            this.specularSelectButton = new System.Windows.Forms.Button();
            this.diffuseSelectButton = new System.Windows.Forms.Button();
            this.diffuseLabel = new System.Windows.Forms.Label();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(13, 13);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(54, 10);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(204, 20);
            this.nameTextBox.TabIndex = 1;
            // 
            // specularLabel
            // 
            this.specularLabel.AutoSize = true;
            this.specularLabel.Location = new System.Drawing.Point(13, 47);
            this.specularLabel.Name = "specularLabel";
            this.specularLabel.Size = new System.Drawing.Size(152, 13);
            this.specularLabel.TabIndex = 2;
            this.specularLabel.Text = "Click to choose specular color:";
            // 
            // specularSelectButton
            // 
            this.specularSelectButton.Location = new System.Drawing.Point(183, 35);
            this.specularSelectButton.Name = "specularSelectButton";
            this.specularSelectButton.Size = new System.Drawing.Size(75, 37);
            this.specularSelectButton.TabIndex = 97;
            this.specularSelectButton.UseVisualStyleBackColor = true;
            this.specularSelectButton.Click += new System.EventHandler(this.specularSelectButton_Click);
            // 
            // diffuseSelectButton
            // 
            this.diffuseSelectButton.Location = new System.Drawing.Point(183, 79);
            this.diffuseSelectButton.Name = "diffuseSelectButton";
            this.diffuseSelectButton.Size = new System.Drawing.Size(75, 37);
            this.diffuseSelectButton.TabIndex = 98;
            this.diffuseSelectButton.UseVisualStyleBackColor = true;
            this.diffuseSelectButton.Click += new System.EventHandler(this.diffuseSelectButton_Click);
            // 
            // diffuseLabel
            // 
            this.diffuseLabel.AutoSize = true;
            this.diffuseLabel.Location = new System.Drawing.Point(13, 91);
            this.diffuseLabel.Name = "diffuseLabel";
            this.diffuseLabel.Size = new System.Drawing.Size(144, 13);
            this.diffuseLabel.TabIndex = 99;
            this.diffuseLabel.Text = "Click to choose diffuse Color:";
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(132, 123);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(60, 20);
            this.helpButton.TabIndex = 102;
            this.helpButton.Tag = "Point_Light";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(198, 123);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 20);
            this.cancelButton.TabIndex = 101;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(16, 123);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(60, 20);
            this.okButton.TabIndex = 100;
            this.okButton.Text = "Add";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // AddPointLightDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(270, 150);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.diffuseLabel);
            this.Controls.Add(this.diffuseSelectButton);
            this.Controls.Add(this.specularSelectButton);
            this.Controls.Add(this.specularLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.nameLabel);
            this.Name = "AddPointLightDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Point Light";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label specularLabel;
        private System.Windows.Forms.Button specularSelectButton;
        private System.Windows.Forms.Button diffuseSelectButton;
        private System.Windows.Forms.Label diffuseLabel;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}
