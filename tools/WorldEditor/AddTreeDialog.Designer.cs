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
	partial class TreeDialog
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
            this.treeFilenameLabel = new System.Windows.Forms.Label();
            this.scaleLabel = new System.Windows.Forms.Label();
            this.addTreeScaleTextBox = new System.Windows.Forms.TextBox();
            this.scaleVarianceLabel = new System.Windows.Forms.Label();
            this.addTreeScaleVarianceTextBox = new System.Windows.Forms.TextBox();
            this.Instances = new System.Windows.Forms.Label();
            this.addTreeInstancesTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.TreeFileNameComboBox = new System.Windows.Forms.ComboBox();
            this.helpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // treeFilenameLabel
            // 
            this.treeFilenameLabel.AutoSize = true;
            this.treeFilenameLabel.Location = new System.Drawing.Point(12, 26);
            this.treeFilenameLabel.Name = "treeFilenameLabel";
            this.treeFilenameLabel.Size = new System.Drawing.Size(55, 13);
            this.treeFilenameLabel.TabIndex = 0;
            this.treeFilenameLabel.Text = "Tree type:";
            // 
            // scaleLabel
            // 
            this.scaleLabel.AutoSize = true;
            this.scaleLabel.Location = new System.Drawing.Point(12, 57);
            this.scaleLabel.Name = "scaleLabel";
            this.scaleLabel.Size = new System.Drawing.Size(37, 13);
            this.scaleLabel.TabIndex = 2;
            this.scaleLabel.Text = "Scale:";
            // 
            // addTreeScaleTextBox
            // 
            this.addTreeScaleTextBox.Location = new System.Drawing.Point(100, 54);
            this.addTreeScaleTextBox.Name = "addTreeScaleTextBox";
            this.addTreeScaleTextBox.Size = new System.Drawing.Size(138, 20);
            this.addTreeScaleTextBox.TabIndex = 3;
            this.addTreeScaleTextBox.Text = "10000";
            // 
            // scaleVarianceLabel
            // 
            this.scaleVarianceLabel.AutoSize = true;
            this.scaleVarianceLabel.Location = new System.Drawing.Point(12, 88);
            this.scaleVarianceLabel.Name = "scaleVarianceLabel";
            this.scaleVarianceLabel.Size = new System.Drawing.Size(81, 13);
            this.scaleVarianceLabel.TabIndex = 4;
            this.scaleVarianceLabel.Text = "Scale variance:";
            // 
            // addTreeScaleVarianceTextBox
            // 
            this.addTreeScaleVarianceTextBox.Location = new System.Drawing.Point(100, 85);
            this.addTreeScaleVarianceTextBox.Name = "addTreeScaleVarianceTextBox";
            this.addTreeScaleVarianceTextBox.Size = new System.Drawing.Size(138, 20);
            this.addTreeScaleVarianceTextBox.TabIndex = 5;
            this.addTreeScaleVarianceTextBox.Text = "1000";
            // 
            // Instances
            // 
            this.Instances.AutoSize = true;
            this.Instances.Location = new System.Drawing.Point(12, 116);
            this.Instances.Name = "Instances";
            this.Instances.Size = new System.Drawing.Size(56, 13);
            this.Instances.TabIndex = 6;
            this.Instances.Text = "Instances:";
            // 
            // addTreeInstancesTextBox
            // 
            this.addTreeInstancesTextBox.Location = new System.Drawing.Point(100, 113);
            this.addTreeInstancesTextBox.Name = "addTreeInstancesTextBox";
            this.addTreeInstancesTextBox.Size = new System.Drawing.Size(138, 20);
            this.addTreeInstancesTextBox.TabIndex = 7;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(29, 152);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "Add";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(299, 152);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "&Cancel";
            // 
            // TreeFileNameComboBox
            // 
            this.TreeFileNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TreeFileNameComboBox.FormattingEnabled = true;
            this.TreeFileNameComboBox.Location = new System.Drawing.Point(100, 23);
            this.TreeFileNameComboBox.Name = "TreeFileNameComboBox";
            this.TreeFileNameComboBox.Size = new System.Drawing.Size(249, 21);
            this.TreeFileNameComboBox.Sorted = true;
            this.TreeFileNameComboBox.TabIndex = 1;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(218, 152);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 9;
            this.helpButton.Tag = "Tree_Type";
            this.helpButton.Text = "Help";
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // TreeDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(391, 187);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.TreeFileNameComboBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.addTreeInstancesTextBox);
            this.Controls.Add(this.Instances);
            this.Controls.Add(this.addTreeScaleVarianceTextBox);
            this.Controls.Add(this.scaleVarianceLabel);
            this.Controls.Add(this.addTreeScaleTextBox);
            this.Controls.Add(this.scaleLabel);
            this.Controls.Add(this.treeFilenameLabel);
            this.Name = "TreeDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Tree Type";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label treeFilenameLabel;
		private System.Windows.Forms.Label scaleLabel;
		public System.Windows.Forms.TextBox addTreeScaleTextBox;
		private System.Windows.Forms.Label scaleVarianceLabel;
		public System.Windows.Forms.TextBox addTreeScaleVarianceTextBox;
		private System.Windows.Forms.Label Instances;
		public System.Windows.Forms.TextBox addTreeInstancesTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		public System.Windows.Forms.ComboBox TreeFileNameComboBox;
		private System.Windows.Forms.Button helpButton;

	}
}
