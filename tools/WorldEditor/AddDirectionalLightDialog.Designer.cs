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
    partial class AddDirectionalLightDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.diffuseSelectButton = new System.Windows.Forms.Button();
            this.specularSelectButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Click to choose lights difuse color:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(179, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Click to choose lights specular color:";
            // 
            // diffuseSelectButton
            // 
            this.diffuseSelectButton.Location = new System.Drawing.Point(193, 12);
            this.diffuseSelectButton.Name = "diffuseSelectButton";
            this.diffuseSelectButton.Size = new System.Drawing.Size(75, 37);
            this.diffuseSelectButton.TabIndex = 99;
            this.diffuseSelectButton.UseVisualStyleBackColor = true;
            this.diffuseSelectButton.Click += new System.EventHandler(this.diffuseSelectButton_Click);
            // 
            // specularSelectButton
            // 
            this.specularSelectButton.Location = new System.Drawing.Point(193, 55);
            this.specularSelectButton.Name = "specularSelectButton";
            this.specularSelectButton.Size = new System.Drawing.Size(75, 37);
            this.specularSelectButton.TabIndex = 100;
            this.specularSelectButton.UseVisualStyleBackColor = true;
            this.specularSelectButton.Click += new System.EventHandler(this.specularSelectButton_Click);
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(142, 111);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(60, 20);
            this.helpButton.TabIndex = 103;
            this.helpButton.Tag = "Directional_Light";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.onHelpButtonClicked);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(208, 111);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 20);
            this.cancelButton.TabIndex = 102;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(16, 111);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(60, 20);
            this.okButton.TabIndex = 101;
            this.okButton.Text = "&Add";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // AddDirectionalLightDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 136);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.specularSelectButton);
            this.Controls.Add(this.diffuseSelectButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "AddDirectionalLightDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Directional Light";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button diffuseSelectButton;
        private System.Windows.Forms.Button specularSelectButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}
