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
	partial class ForestDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		public System.ComponentModel.IContainer components = null;

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
		public void InitializeComponent()
		{
            this.speedWindFileNameLabel = new System.Windows.Forms.Label();
            this.windSpeedLabel = new System.Windows.Forms.Label();
            this.forestAddDialogWindSpeedTextBox = new System.Windows.Forms.TextBox();
            this.addForestDialogWindDirectionXTextBox = new System.Windows.Forms.Label();
            this.addForestDialogWindDirectionYTextBox = new System.Windows.Forms.Label();
            this.addForestDialogWindDirectionZTextBox = new System.Windows.Forms.Label();
            this.forestAddDialogWindDirectionXTextBox = new System.Windows.Forms.TextBox();
            this.forestAddDialogWindDirectionYTextBox = new System.Windows.Forms.TextBox();
            this.forestAddDialogWindDirectionZTextBox = new System.Windows.Forms.TextBox();
            this.forestAddDialogOkButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();

            this.ForestDialogSpeedWindFilenameComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ForestSeedTextBox = new System.Windows.Forms.TextBox();
            this.helpButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // speedWindFileNameLabel
            // 
            this.speedWindFileNameLabel.AutoSize = true;
            this.speedWindFileNameLabel.Location = new System.Drawing.Point(12, 30);
            this.speedWindFileNameLabel.Name = "speedWindFileNameLabel";
            this.speedWindFileNameLabel.Size = new System.Drawing.Size(104, 13);
            this.speedWindFileNameLabel.TabIndex = 9;
            this.speedWindFileNameLabel.Text = "SpeedTree wind file:";
            // 
            // windSpeedLabel
            // 
            this.windSpeedLabel.AutoSize = true;
            this.windSpeedLabel.Location = new System.Drawing.Point(12, 87);
            this.windSpeedLabel.Name = "windSpeedLabel";
            this.windSpeedLabel.Size = new System.Drawing.Size(67, 13);
            this.windSpeedLabel.TabIndex = 11;
            this.windSpeedLabel.Text = "Wind speed:";
            // 
            // forestAddDialogWindSpeedTextBox
            // 
            this.forestAddDialogWindSpeedTextBox.AcceptsTab = true;
            this.forestAddDialogWindSpeedTextBox.AllowDrop = true;
            this.forestAddDialogWindSpeedTextBox.Location = new System.Drawing.Point(128, 84);
            this.forestAddDialogWindSpeedTextBox.Name = "forestAddDialogWindSpeedTextBox";
            this.forestAddDialogWindSpeedTextBox.Size = new System.Drawing.Size(202, 20);
            this.forestAddDialogWindSpeedTextBox.TabIndex = 2;
            this.forestAddDialogWindSpeedTextBox.Text = ".3";
            this.forestAddDialogWindSpeedTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // addForestDialogWindDirectionXTextBox
            // 
            this.addForestDialogWindDirectionXTextBox.AutoSize = true;
            this.addForestDialogWindDirectionXTextBox.Location = new System.Drawing.Point(7, 22);
            this.addForestDialogWindDirectionXTextBox.Name = "addForestDialogWindDirectionXTextBox";
            this.addForestDialogWindDirectionXTextBox.Size = new System.Drawing.Size(17, 13);
            this.addForestDialogWindDirectionXTextBox.TabIndex = 13;
            this.addForestDialogWindDirectionXTextBox.Text = "X:";
            // 
            // addForestDialogWindDirectionYTextBox
            // 
            this.addForestDialogWindDirectionYTextBox.AutoSize = true;
            this.addForestDialogWindDirectionYTextBox.Location = new System.Drawing.Point(7, 50);
            this.addForestDialogWindDirectionYTextBox.Name = "addForestDialogWindDirectionYTextBox";
            this.addForestDialogWindDirectionYTextBox.Size = new System.Drawing.Size(17, 13);
            this.addForestDialogWindDirectionYTextBox.TabIndex = 14;
            this.addForestDialogWindDirectionYTextBox.Text = "Y:";
            // 
            // addForestDialogWindDirectionZTextBox
            // 
            this.addForestDialogWindDirectionZTextBox.AutoSize = true;
            this.addForestDialogWindDirectionZTextBox.Location = new System.Drawing.Point(7, 80);
            this.addForestDialogWindDirectionZTextBox.Name = "addForestDialogWindDirectionZTextBox";
            this.addForestDialogWindDirectionZTextBox.Size = new System.Drawing.Size(17, 13);
            this.addForestDialogWindDirectionZTextBox.TabIndex = 15;
            this.addForestDialogWindDirectionZTextBox.Text = "Z:";
            // 
            // forestAddDialogWindDirectionXTextBox
            // 
            this.forestAddDialogWindDirectionXTextBox.Location = new System.Drawing.Point(26, 19);
            this.forestAddDialogWindDirectionXTextBox.Name = "forestAddDialogWindDirectionXTextBox";
            this.forestAddDialogWindDirectionXTextBox.Size = new System.Drawing.Size(134, 20);
            this.forestAddDialogWindDirectionXTextBox.TabIndex = 3;
            this.forestAddDialogWindDirectionXTextBox.Text = "1";
            this.forestAddDialogWindDirectionXTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // forestAddDialogWindDirectionYTextBox
            // 
            this.forestAddDialogWindDirectionYTextBox.Location = new System.Drawing.Point(26, 47);
            this.forestAddDialogWindDirectionYTextBox.Name = "forestAddDialogWindDirectionYTextBox";
            this.forestAddDialogWindDirectionYTextBox.Size = new System.Drawing.Size(134, 20);
            this.forestAddDialogWindDirectionYTextBox.TabIndex = 4;
            this.forestAddDialogWindDirectionYTextBox.Text = "0";
            this.forestAddDialogWindDirectionYTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // forestAddDialogWindDirectionZTextBox
            // 
            this.forestAddDialogWindDirectionZTextBox.Location = new System.Drawing.Point(26, 77);
            this.forestAddDialogWindDirectionZTextBox.Name = "forestAddDialogWindDirectionZTextBox";
            this.forestAddDialogWindDirectionZTextBox.Size = new System.Drawing.Size(134, 20);
            this.forestAddDialogWindDirectionZTextBox.TabIndex = 5;
            this.forestAddDialogWindDirectionZTextBox.Text = "0";
            this.forestAddDialogWindDirectionZTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // forestAddDialogOkButton
            // 
            this.forestAddDialogOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.forestAddDialogOkButton.Location = new System.Drawing.Point(12, 276);
            this.forestAddDialogOkButton.Name = "forestAddDialogOkButton";
            this.forestAddDialogOkButton.Size = new System.Drawing.Size(75, 23);
            this.forestAddDialogOkButton.TabIndex = 6;
            this.forestAddDialogOkButton.Text = "Add";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(257, 276);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel";
            // 
            // ForestDialogSpeedWindFilenameComboBox
            // 
            this.ForestDialogSpeedWindFilenameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ForestDialogSpeedWindFilenameComboBox.FormattingEnabled = true;
            this.ForestDialogSpeedWindFilenameComboBox.Location = new System.Drawing.Point(128, 27);
            this.ForestDialogSpeedWindFilenameComboBox.Name = "ForestDialogSpeedWindFilenameComboBox";
            this.ForestDialogSpeedWindFilenameComboBox.Size = new System.Drawing.Size(202, 21);
            this.ForestDialogSpeedWindFilenameComboBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Random number seed:";
            // 
            // ForestSeedTextBox
            // 
            this.ForestSeedTextBox.Location = new System.Drawing.Point(128, 54);
            this.ForestSeedTextBox.Name = "ForestSeedTextBox";
            this.ForestSeedTextBox.Size = new System.Drawing.Size(202, 20);
            this.ForestSeedTextBox.TabIndex = 1;
            this.ForestSeedTextBox.Text = "0";
            this.ForestSeedTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.intVerifyevent);
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(176, 276);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 16;
            this.helpButton.Tag = "Forest";
            this.helpButton.Text = "Help";
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.addForestDialogWindDirectionXTextBox);
            this.groupBox1.Controls.Add(this.addForestDialogWindDirectionYTextBox);
            this.groupBox1.Controls.Add(this.addForestDialogWindDirectionZTextBox);
            this.groupBox1.Controls.Add(this.forestAddDialogWindDirectionXTextBox);
            this.groupBox1.Controls.Add(this.forestAddDialogWindDirectionYTextBox);
            this.groupBox1.Controls.Add(this.forestAddDialogWindDirectionZTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 110);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 108);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Wind direction";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 235);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(317, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Click Add to add forest, then right-click on forest and choose Add ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 250);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Tree Type to add trees.";
            // 
            // ForestDialog
            // 
            this.AcceptButton = this.forestAddDialogOkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(344, 311);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.ForestSeedTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ForestDialogSpeedWindFilenameComboBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.forestAddDialogOkButton);
            this.Controls.Add(this.forestAddDialogWindSpeedTextBox);
            this.Controls.Add(this.windSpeedLabel);
            this.Controls.Add(this.speedWindFileNameLabel);
            this.Name = "ForestDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Forest";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label speedWindFileNameLabel;
		private System.Windows.Forms.Label windSpeedLabel;
        public System.Windows.Forms.TextBox forestAddDialogWindSpeedTextBox;
		private System.Windows.Forms.Label addForestDialogWindDirectionXTextBox;
		private System.Windows.Forms.Label addForestDialogWindDirectionYTextBox;
		private System.Windows.Forms.Label addForestDialogWindDirectionZTextBox;
		public System.Windows.Forms.TextBox forestAddDialogWindDirectionXTextBox;
		public System.Windows.Forms.TextBox forestAddDialogWindDirectionYTextBox;
		public System.Windows.Forms.TextBox forestAddDialogWindDirectionZTextBox;
		public System.Windows.Forms.Button forestAddDialogOkButton;
		public System.Windows.Forms.Button cancelButton;
		public System.Windows.Forms.ComboBox ForestDialogSpeedWindFilenameComboBox;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox ForestSeedTextBox;
		public System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;

	}
}
