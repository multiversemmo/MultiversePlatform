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
	partial class AddParticleFXDialog
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
            this.particleEffectComboBox = new System.Windows.Forms.ComboBox();
            this.particleEffectComboBoxLabel = new System.Windows.Forms.Label();
            this.velocityScaleTextBox = new System.Windows.Forms.TextBox();
            this.positionScaleTextBox = new System.Windows.Forms.TextBox();
            this.velocityScaleLabel = new System.Windows.Forms.Label();
            this.positionScaleLabel = new System.Windows.Forms.Label();
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.attachmentPointLabel = new System.Windows.Forms.Label();
            this.attachmentPointComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // particleEffectComboBox
            // 
            this.particleEffectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.particleEffectComboBox.FormattingEnabled = true;
            this.particleEffectComboBox.Location = new System.Drawing.Point(129, 13);
            this.particleEffectComboBox.Name = "particleEffectComboBox";
            this.particleEffectComboBox.Size = new System.Drawing.Size(182, 21);
            this.particleEffectComboBox.TabIndex = 1;
            // 
            // particleEffectComboBoxLabel
            // 
            this.particleEffectComboBoxLabel.AutoSize = true;
            this.particleEffectComboBoxLabel.Location = new System.Drawing.Point(12, 16);
            this.particleEffectComboBoxLabel.Name = "particleEffectComboBoxLabel";
            this.particleEffectComboBoxLabel.Size = new System.Drawing.Size(76, 13);
            this.particleEffectComboBoxLabel.TabIndex = 0;
            this.particleEffectComboBoxLabel.Text = "Particle Effect:";
            // 
            // velocityScaleTextBox
            // 
            this.velocityScaleTextBox.Location = new System.Drawing.Point(129, 41);
            this.velocityScaleTextBox.Name = "velocityScaleTextBox";
            this.velocityScaleTextBox.Size = new System.Drawing.Size(182, 20);
            this.velocityScaleTextBox.TabIndex = 3;
            this.velocityScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // positionScaleTextBox
            // 
            this.positionScaleTextBox.Location = new System.Drawing.Point(129, 67);
            this.positionScaleTextBox.Name = "positionScaleTextBox";
            this.positionScaleTextBox.Size = new System.Drawing.Size(182, 20);
            this.positionScaleTextBox.TabIndex = 5;
            this.positionScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // velocityScaleLabel
            // 
            this.velocityScaleLabel.AutoSize = true;
            this.velocityScaleLabel.Location = new System.Drawing.Point(12, 44);
            this.velocityScaleLabel.Name = "velocityScaleLabel";
            this.velocityScaleLabel.Size = new System.Drawing.Size(77, 13);
            this.velocityScaleLabel.TabIndex = 2;
            this.velocityScaleLabel.Text = "Velocity Scale:";
            // 
            // positionScaleLabel
            // 
            this.positionScaleLabel.AutoSize = true;
            this.positionScaleLabel.Location = new System.Drawing.Point(12, 70);
            this.positionScaleLabel.Name = "positionScaleLabel";
            this.positionScaleLabel.Size = new System.Drawing.Size(75, 13);
            this.positionScaleLabel.TabIndex = 4;
            this.positionScaleLabel.Text = "Particle Scale:";
            // 
            // okbutton
            // 
            this.okbutton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okbutton.Location = new System.Drawing.Point(13, 129);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(75, 23);
            this.okbutton.TabIndex = 8;
            this.okbutton.Text = "Add";
            this.okbutton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(234, 129);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(153, 129);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 9;
            this.helpButton.Tag = "Particle_Effect";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // attachmentPointLabel
            // 
            this.attachmentPointLabel.AutoSize = true;
            this.attachmentPointLabel.Location = new System.Drawing.Point(12, 96);
            this.attachmentPointLabel.Name = "attachmentPointLabel";
            this.attachmentPointLabel.Size = new System.Drawing.Size(91, 13);
            this.attachmentPointLabel.TabIndex = 6;
            this.attachmentPointLabel.Text = "Attachment Point:";
            this.attachmentPointLabel.Visible = false;
            // 
            // attachmentPointComboBox
            // 
            this.attachmentPointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.attachmentPointComboBox.FormattingEnabled = true;
            this.attachmentPointComboBox.Location = new System.Drawing.Point(129, 93);
            this.attachmentPointComboBox.Name = "attachmentPointComboBox";
            this.attachmentPointComboBox.Size = new System.Drawing.Size(182, 21);
            this.attachmentPointComboBox.TabIndex = 7;
            this.attachmentPointComboBox.Visible = false;
            // 
            // AddParticleFXDialog
            // 
            this.AcceptButton = this.okbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 164);
            this.Controls.Add(this.attachmentPointComboBox);
            this.Controls.Add(this.attachmentPointLabel);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.positionScaleLabel);
            this.Controls.Add(this.velocityScaleLabel);
            this.Controls.Add(this.positionScaleTextBox);
            this.Controls.Add(this.velocityScaleTextBox);
            this.Controls.Add(this.particleEffectComboBoxLabel);
            this.Controls.Add(this.particleEffectComboBox);
            this.Name = "AddParticleFXDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Particle Effects";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox particleEffectComboBox;
		private System.Windows.Forms.Label particleEffectComboBoxLabel;
		private System.Windows.Forms.TextBox velocityScaleTextBox;
		private System.Windows.Forms.TextBox positionScaleTextBox;
		private System.Windows.Forms.Label velocityScaleLabel;
		private System.Windows.Forms.Label positionScaleLabel;
		private System.Windows.Forms.Button okbutton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Label attachmentPointLabel;
        private System.Windows.Forms.ComboBox attachmentPointComboBox;
	}
}
