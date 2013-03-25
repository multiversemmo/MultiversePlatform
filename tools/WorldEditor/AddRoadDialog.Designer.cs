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
	partial class AddRoadDialog
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
            this.roadNameTextBox = new System.Windows.Forms.TextBox();
            this.roadNameLabel = new System.Windows.Forms.Label();
            this.halfWidthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.roadCreateButton = new System.Windows.Forms.Button();
            this.cancelRoadButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.halfWidthNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // roadNameTextBox
            // 
            this.roadNameTextBox.Location = new System.Drawing.Point(53, 26);
            this.roadNameTextBox.Name = "roadNameTextBox";
            this.roadNameTextBox.Size = new System.Drawing.Size(260, 20);
            this.roadNameTextBox.TabIndex = 0;
            // 
            // roadNameLabel
            // 
            this.roadNameLabel.AutoSize = true;
            this.roadNameLabel.Location = new System.Drawing.Point(12, 29);
            this.roadNameLabel.Name = "roadNameLabel";
            this.roadNameLabel.Size = new System.Drawing.Size(38, 13);
            this.roadNameLabel.TabIndex = 1;
            this.roadNameLabel.Text = "Name:";
            // 
            // halfWidthNumericUpDown
            // 
            this.halfWidthNumericUpDown.Location = new System.Drawing.Point(75, 56);
            this.halfWidthNumericUpDown.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.halfWidthNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.halfWidthNumericUpDown.Name = "halfWidthNumericUpDown";
            this.halfWidthNumericUpDown.Size = new System.Drawing.Size(48, 20);
            this.halfWidthNumericUpDown.TabIndex = 1;
            this.halfWidthNumericUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Half Width:";
            // 
            // roadCreateButton
            // 
            this.roadCreateButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.roadCreateButton.Location = new System.Drawing.Point(15, 94);
            this.roadCreateButton.Name = "roadCreateButton";
            this.roadCreateButton.Size = new System.Drawing.Size(75, 23);
            this.roadCreateButton.TabIndex = 2;
            this.roadCreateButton.Text = "Add";
            this.roadCreateButton.UseVisualStyleBackColor = true;
            // 
            // cancelRoadButton
            // 
            this.cancelRoadButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelRoadButton.Location = new System.Drawing.Point(235, 93);
            this.cancelRoadButton.Name = "cancelRoadButton";
            this.cancelRoadButton.Size = new System.Drawing.Size(75, 23);
            this.cancelRoadButton.TabIndex = 3;
            this.cancelRoadButton.Text = "&Cancel";
            this.cancelRoadButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.CausesValidation = false;
            this.helpButton.Location = new System.Drawing.Point(154, 93);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 4;
            this.helpButton.Tag = "Road";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // AddRoadDialog
            // 
            this.AcceptButton = this.roadCreateButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelRoadButton;
            this.ClientSize = new System.Drawing.Size(325, 122);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelRoadButton);
            this.Controls.Add(this.roadCreateButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.halfWidthNumericUpDown);
            this.Controls.Add(this.roadNameLabel);
            this.Controls.Add(this.roadNameTextBox);
            this.Name = "AddRoadDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Road";
            ((System.ComponentModel.ISupportInitialize)(this.halfWidthNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox roadNameTextBox;
		private System.Windows.Forms.Label roadNameLabel;
		private System.Windows.Forms.NumericUpDown halfWidthNumericUpDown;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button roadCreateButton;
		private System.Windows.Forms.Button cancelRoadButton;
		private System.Windows.Forms.Button helpButton;
	}
}
