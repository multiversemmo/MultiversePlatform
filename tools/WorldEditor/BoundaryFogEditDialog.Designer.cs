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

namespace Multiverse.MapTool
{
	partial class BoundaryFogEditDialog
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
			this.RedTextBox = new System.Windows.Forms.TextBox();
			this.GreenTextBox = new System.Windows.Forms.TextBox();
			this.BlueTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.FarTextBox = new System.Windows.Forms.TextBox();
			this.NearTextBox = new System.Windows.Forms.TextBox();
			this.BoundaryFogEditDialogOKButton = new System.Windows.Forms.Button();
			this.BoundaryFogEditDialogCancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Fog Color:";
			// 
			// RedTextBox
			// 
			this.RedTextBox.Location = new System.Drawing.Point(51, 34);
			this.RedTextBox.Name = "RedTextBox";
			this.RedTextBox.Size = new System.Drawing.Size(52, 20);
			this.RedTextBox.TabIndex = 0;
			// 
			// GreenTextBox
			// 
			this.GreenTextBox.Location = new System.Drawing.Point(130, 34);
			this.GreenTextBox.Name = "GreenTextBox";
			this.GreenTextBox.Size = new System.Drawing.Size(52, 20);
			this.GreenTextBox.TabIndex = 1;
			// 
			// BlueTextBox
			// 
			this.BlueTextBox.Location = new System.Drawing.Point(209, 34);
			this.BlueTextBox.Name = "BlueTextBox";
			this.BlueTextBox.Size = new System.Drawing.Size(52, 20);
			this.BlueTextBox.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(30, 37);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(18, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "R:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(109, 37);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(18, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "G:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(188, 37);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(17, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "B:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(13, 72);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(25, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Far:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(13, 103);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(33, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Near:";
			// 
			// FarTextBox
			// 
			this.FarTextBox.Location = new System.Drawing.Point(44, 69);
			this.FarTextBox.Name = "FarTextBox";
			this.FarTextBox.Size = new System.Drawing.Size(179, 20);
			this.FarTextBox.TabIndex = 3;
			// 
			// NearTextBox
			// 
			this.NearTextBox.Location = new System.Drawing.Point(44, 103);
			this.NearTextBox.Name = "NearTextBox";
			this.NearTextBox.Size = new System.Drawing.Size(179, 20);
			this.NearTextBox.TabIndex = 4;
			// 
			// BoundaryFogEditDialogOKButton
			// 
			this.BoundaryFogEditDialogOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.BoundaryFogEditDialogOKButton.Location = new System.Drawing.Point(16, 134);
			this.BoundaryFogEditDialogOKButton.Name = "BoundaryFogEditDialogOKButton";
			this.BoundaryFogEditDialogOKButton.Size = new System.Drawing.Size(75, 23);
			this.BoundaryFogEditDialogOKButton.TabIndex = 5;
			this.BoundaryFogEditDialogOKButton.Text = "&OK";
			this.BoundaryFogEditDialogOKButton.UseVisualStyleBackColor = true;
			// 
			// BoundaryFogEditDialogCancelButton
			// 
			this.BoundaryFogEditDialogCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BoundaryFogEditDialogCancelButton.Location = new System.Drawing.Point(205, 134);
			this.BoundaryFogEditDialogCancelButton.Name = "BoundaryFogEditDialogCancelButton";
			this.BoundaryFogEditDialogCancelButton.Size = new System.Drawing.Size(75, 23);
			this.BoundaryFogEditDialogCancelButton.TabIndex = 6;
			this.BoundaryFogEditDialogCancelButton.Text = "&Cancel";
			this.BoundaryFogEditDialogCancelButton.UseVisualStyleBackColor = true;
			// 
			// BoundaryFogEditDialog
			// 
			this.AcceptButton = this.BoundaryFogEditDialogOKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.BoundaryFogEditDialogCancelButton;
			this.ClientSize = new System.Drawing.Size(292, 169);
			this.Controls.Add(this.BoundaryFogEditDialogCancelButton);
			this.Controls.Add(this.BoundaryFogEditDialogOKButton);
			this.Controls.Add(this.NearTextBox);
			this.Controls.Add(this.FarTextBox);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.BlueTextBox);
			this.Controls.Add(this.GreenTextBox);
			this.Controls.Add(this.RedTextBox);
			this.Controls.Add(this.label1);
			this.Name = "BoundaryFogEditDialog";
			this.Text = "Boundary Fog Edit Dialog";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button BoundaryFogEditDialogOKButton;
		private System.Windows.Forms.Button BoundaryFogEditDialogCancelButton;
		public System.Windows.Forms.TextBox RedTextBox;
		public System.Windows.Forms.TextBox GreenTextBox;
		public System.Windows.Forms.TextBox BlueTextBox;
		public System.Windows.Forms.TextBox FarTextBox;
		public System.Windows.Forms.TextBox NearTextBox;
	}
}
