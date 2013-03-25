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
	partial class AddFogDialog
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.FogFarTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.FogNearTextBox = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.helpButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.colorSelectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(381, 109);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 20);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // FogFarTextBox
            // 
            this.FogFarTextBox.AcceptsTab = true;
            this.FogFarTextBox.Location = new System.Drawing.Point(294, 82);
            this.FogFarTextBox.Name = "FogFarTextBox";
            this.FogFarTextBox.Size = new System.Drawing.Size(147, 20);
            this.FogFarTextBox.TabIndex = 4;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(19, 109);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(60, 20);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "&Add";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // FogNearTextBox
            // 
            this.FogNearTextBox.AcceptsTab = true;
            this.FogNearTextBox.Location = new System.Drawing.Point(294, 55);
            this.FogNearTextBox.Name = "FogNearTextBox";
            this.FogNearTextBox.Size = new System.Drawing.Size(147, 20);
            this.FogNearTextBox.TabIndex = 3;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(16, 85);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(262, 13);
            this.label21.TabIndex = 93;
            this.label21.Text = "Maximum fog effect at this distance from camera (mm):";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 59);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(210, 13);
            this.label10.TabIndex = 92;
            this.label10.Text = "Start fog at this distance from camera (mm):";
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(315, 109);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(60, 20);
            this.helpButton.TabIndex = 94;
            this.helpButton.Tag = "Fog";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.onHelpButtonClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 13);
            this.label1.TabIndex = 95;
            this.label1.Text = "Click to choose fog color:";
            // 
            // colorSelectButton
            // 
            this.colorSelectButton.Location = new System.Drawing.Point(151, 10);
            this.colorSelectButton.Name = "colorSelectButton";
            this.colorSelectButton.Size = new System.Drawing.Size(75, 37);
            this.colorSelectButton.TabIndex = 96;
            this.colorSelectButton.UseVisualStyleBackColor = true;
            this.colorSelectButton.Click += new System.EventHandler(this.colorSelectButton_Click);
            // 
            // AddFogDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 141);
            this.Controls.Add(this.colorSelectButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.FogFarTextBox);
            this.Controls.Add(this.FogNearTextBox);
            this.Name = "AddFogDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Fog";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		public System.Windows.Forms.TextBox FogFarTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox FogNearTextBox;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button helpButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button colorSelectButton;
	}
}
