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

namespace Multiverse.Base
{
	partial class ConfigDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

//		public void Layout() {
//			int imageWidth = 411;
//			int imageHeight = 206;
//			int pad = 13;
//			logoPicture.Location = new System.Drawing.Point(13, 13);
//			renderOptions.Location = new System.Drawing.Point(9, 254);
//			renderOptions.Size = new System.Drawing.Size(imageWidth, 153);
//
//			attributeListBox.Location = new System.Drawing.Point(7, 20);
//			attributeListBox.Size = new System.Drawing.Size(imageWidth - 12, 102);
//			int attrValWidth = 212;
//			attrValComboBox.Location = new System.Drawing.Point(imageWidth - 4 - attrValWidth, 126);
//			attrValComboBox.Size = new System.Drawing.Size(212, 21);
//		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.renderOptions = new System.Windows.Forms.GroupBox();
            this.aaComboBox = new System.Windows.Forms.ComboBox();
            this.aaLabel = new System.Windows.Forms.Label();
            this.nvPerfHUDGroupBox = new System.Windows.Forms.GroupBox();
            this.nvPerfHUDNoButton = new System.Windows.Forms.RadioButton();
            this.nvPerfHUDYesButton = new System.Windows.Forms.RadioButton();
            this.vsyncGroupBox = new System.Windows.Forms.GroupBox();
            this.vsyncNoButton = new System.Windows.Forms.RadioButton();
            this.vsyncYesButton = new System.Windows.Forms.RadioButton();
            this.fullScreenGroupBox = new System.Windows.Forms.GroupBox();
            this.fullScreenNoButton = new System.Windows.Forms.RadioButton();
            this.fullScreenYesButton = new System.Windows.Forms.RadioButton();
            this.videoModeComboBox = new System.Windows.Forms.ComboBox();
            this.videoModeLabel = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.logoPicture = new System.Windows.Forms.PictureBox();
            this.renderSystemLabel = new System.Windows.Forms.Label();
            this.renderSystemComboBox = new System.Windows.Forms.ComboBox();
            this.renderOptions.SuspendLayout();
            this.nvPerfHUDGroupBox.SuspendLayout();
            this.vsyncGroupBox.SuspendLayout();
            this.fullScreenGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // renderOptions
            // 
            this.renderOptions.Controls.Add(this.aaComboBox);
            this.renderOptions.Controls.Add(this.aaLabel);
            this.renderOptions.Controls.Add(this.nvPerfHUDGroupBox);
            this.renderOptions.Controls.Add(this.vsyncGroupBox);
            this.renderOptions.Controls.Add(this.fullScreenGroupBox);
            this.renderOptions.Controls.Add(this.videoModeComboBox);
            this.renderOptions.Controls.Add(this.videoModeLabel);
            this.renderOptions.Location = new System.Drawing.Point(11, 303);
            this.renderOptions.Name = "renderOptions";
            this.renderOptions.Size = new System.Drawing.Size(470, 196);
            this.renderOptions.TabIndex = 0;
            this.renderOptions.TabStop = false;
            this.renderOptions.Text = "Rendering System Options";
            // 
            // aaComboBox
            // 
            this.aaComboBox.FormattingEnabled = true;
            this.aaComboBox.Location = new System.Drawing.Point(161, 83);
            this.aaComboBox.Name = "aaComboBox";
            this.aaComboBox.Size = new System.Drawing.Size(291, 24);
            this.aaComboBox.TabIndex = 6;
            // 
            // aaLabel
            // 
            this.aaLabel.AutoSize = true;
            this.aaLabel.Location = new System.Drawing.Point(54, 86);
            this.aaLabel.Name = "aaLabel";
            this.aaLabel.Size = new System.Drawing.Size(89, 17);
            this.aaLabel.TabIndex = 5;
            this.aaLabel.Text = "Anti-aliasing:";
            // 
            // nvPerfHUDGroupBox
            // 
            this.nvPerfHUDGroupBox.Controls.Add(this.nvPerfHUDNoButton);
            this.nvPerfHUDGroupBox.Controls.Add(this.nvPerfHUDYesButton);
            this.nvPerfHUDGroupBox.Location = new System.Drawing.Point(337, 126);
            this.nvPerfHUDGroupBox.Name = "nvPerfHUDGroupBox";
            this.nvPerfHUDGroupBox.Size = new System.Drawing.Size(115, 53);
            this.nvPerfHUDGroupBox.TabIndex = 4;
            this.nvPerfHUDGroupBox.TabStop = false;
            this.nvPerfHUDGroupBox.Text = "NVPerfHUD";
            // 
            // nvPerfHUDNoButton
            // 
            this.nvPerfHUDNoButton.AutoSize = true;
            this.nvPerfHUDNoButton.Location = new System.Drawing.Point(66, 21);
            this.nvPerfHUDNoButton.Name = "nvPerfHUDNoButton";
            this.nvPerfHUDNoButton.Size = new System.Drawing.Size(44, 21);
            this.nvPerfHUDNoButton.TabIndex = 1;
            this.nvPerfHUDNoButton.TabStop = true;
            this.nvPerfHUDNoButton.Text = "No";
            this.nvPerfHUDNoButton.UseVisualStyleBackColor = true;
            this.nvPerfHUDNoButton.CheckedChanged += new System.EventHandler(this.nvPerfHUDNoButton_CheckedChanged);
            // 
            // nvPerfHUDYesButton
            // 
            this.nvPerfHUDYesButton.AutoSize = true;
            this.nvPerfHUDYesButton.Location = new System.Drawing.Point(10, 21);
            this.nvPerfHUDYesButton.Name = "nvPerfHUDYesButton";
            this.nvPerfHUDYesButton.Size = new System.Drawing.Size(50, 21);
            this.nvPerfHUDYesButton.TabIndex = 0;
            this.nvPerfHUDYesButton.TabStop = true;
            this.nvPerfHUDYesButton.Text = "Yes";
            this.nvPerfHUDYesButton.UseVisualStyleBackColor = true;
            this.nvPerfHUDYesButton.CheckedChanged += new System.EventHandler(this.nvPerfHUDYesButton_CheckedChanged);
            // 
            // vsyncGroupBox
            // 
            this.vsyncGroupBox.Controls.Add(this.vsyncNoButton);
            this.vsyncGroupBox.Controls.Add(this.vsyncYesButton);
            this.vsyncGroupBox.Location = new System.Drawing.Point(178, 124);
            this.vsyncGroupBox.Name = "vsyncGroupBox";
            this.vsyncGroupBox.Size = new System.Drawing.Size(115, 55);
            this.vsyncGroupBox.TabIndex = 3;
            this.vsyncGroupBox.TabStop = false;
            this.vsyncGroupBox.Text = "Video Sync";
            // 
            // vsyncNoButton
            // 
            this.vsyncNoButton.AutoSize = true;
            this.vsyncNoButton.Location = new System.Drawing.Point(66, 21);
            this.vsyncNoButton.Name = "vsyncNoButton";
            this.vsyncNoButton.Size = new System.Drawing.Size(44, 21);
            this.vsyncNoButton.TabIndex = 1;
            this.vsyncNoButton.TabStop = true;
            this.vsyncNoButton.Text = "No";
            this.vsyncNoButton.UseVisualStyleBackColor = true;
            this.vsyncNoButton.CheckedChanged += new System.EventHandler(this.vsyncNoButton_CheckedChanged);
            // 
            // vsyncYesButton
            // 
            this.vsyncYesButton.AutoSize = true;
            this.vsyncYesButton.Location = new System.Drawing.Point(10, 21);
            this.vsyncYesButton.Name = "vsyncYesButton";
            this.vsyncYesButton.Size = new System.Drawing.Size(50, 21);
            this.vsyncYesButton.TabIndex = 0;
            this.vsyncYesButton.TabStop = true;
            this.vsyncYesButton.Text = "Yes";
            this.vsyncYesButton.UseVisualStyleBackColor = true;
            this.vsyncYesButton.CheckedChanged += new System.EventHandler(this.vsyncYesButton_CheckedChanged);
            // 
            // fullScreenGroupBox
            // 
            this.fullScreenGroupBox.Controls.Add(this.fullScreenNoButton);
            this.fullScreenGroupBox.Controls.Add(this.fullScreenYesButton);
            this.fullScreenGroupBox.Location = new System.Drawing.Point(18, 124);
            this.fullScreenGroupBox.Name = "fullScreenGroupBox";
            this.fullScreenGroupBox.Size = new System.Drawing.Size(115, 55);
            this.fullScreenGroupBox.TabIndex = 2;
            this.fullScreenGroupBox.TabStop = false;
            this.fullScreenGroupBox.Text = "Full Screen";
            // 
            // fullScreenNoButton
            // 
            this.fullScreenNoButton.AutoSize = true;
            this.fullScreenNoButton.Location = new System.Drawing.Point(66, 21);
            this.fullScreenNoButton.Name = "fullScreenNoButton";
            this.fullScreenNoButton.Size = new System.Drawing.Size(44, 21);
            this.fullScreenNoButton.TabIndex = 1;
            this.fullScreenNoButton.TabStop = true;
            this.fullScreenNoButton.Text = "No";
            this.fullScreenNoButton.UseVisualStyleBackColor = true;
            this.fullScreenNoButton.CheckedChanged += new System.EventHandler(this.fullScreenNoButton_CheckedChanged);
            // 
            // fullScreenYesButton
            // 
            this.fullScreenYesButton.AutoSize = true;
            this.fullScreenYesButton.Location = new System.Drawing.Point(10, 21);
            this.fullScreenYesButton.Name = "fullScreenYesButton";
            this.fullScreenYesButton.Size = new System.Drawing.Size(50, 21);
            this.fullScreenYesButton.TabIndex = 0;
            this.fullScreenYesButton.TabStop = true;
            this.fullScreenYesButton.Text = "Yes";
            this.fullScreenYesButton.UseVisualStyleBackColor = true;
            this.fullScreenYesButton.CheckedChanged += new System.EventHandler(this.fullScreenYesButton_CheckedChanged);
            // 
            // videoModeComboBox
            // 
            this.videoModeComboBox.FormattingEnabled = true;
            this.videoModeComboBox.Location = new System.Drawing.Point(161, 38);
            this.videoModeComboBox.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.videoModeComboBox.Name = "videoModeComboBox";
            this.videoModeComboBox.Size = new System.Drawing.Size(291, 24);
            this.videoModeComboBox.TabIndex = 1;
            // 
            // videoModeLabel
            // 
            this.videoModeLabel.AutoSize = true;
            this.videoModeLabel.Location = new System.Drawing.Point(56, 45);
            this.videoModeLabel.Margin = new System.Windows.Forms.Padding(3, 3, 1, 3);
            this.videoModeLabel.Name = "videoModeLabel";
            this.videoModeLabel.Size = new System.Drawing.Size(87, 17);
            this.videoModeLabel.TabIndex = 0;
            this.videoModeLabel.Text = "Video Mode:";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(292, 518);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(90, 26);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(391, 518);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 26);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            // 
            // logoPicture
            // 
            this.logoPicture.Location = new System.Drawing.Point(39, 12);
            this.logoPicture.Name = "logoPicture";
            this.logoPicture.Size = new System.Drawing.Size(411, 228);
            this.logoPicture.TabIndex = 3;
            this.logoPicture.TabStop = false;
            this.logoPicture.Click += new System.EventHandler(this.f);
            // 
            // renderSystemLabel
            // 
            this.renderSystemLabel.AutoSize = true;
            this.renderSystemLabel.Location = new System.Drawing.Point(16, 264);
            this.renderSystemLabel.Name = "renderSystemLabel";
            this.renderSystemLabel.Size = new System.Drawing.Size(147, 17);
            this.renderSystemLabel.TabIndex = 4;
            this.renderSystemLabel.Text = "Rendering Subsystem";
            // 
            // renderSystemComboBox
            // 
            this.renderSystemComboBox.FormattingEnabled = true;
            this.renderSystemComboBox.Location = new System.Drawing.Point(172, 261);
            this.renderSystemComboBox.Name = "renderSystemComboBox";
            this.renderSystemComboBox.Size = new System.Drawing.Size(291, 24);
            this.renderSystemComboBox.TabIndex = 5;
            // 
            // ConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(492, 557);
            this.Controls.Add(this.renderSystemComboBox);
            this.Controls.Add(this.renderSystemLabel);
            this.Controls.Add(this.logoPicture);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.renderOptions);
            this.Name = "ConfigDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Multiverse Graphics Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigDialog_FormClosing);
            this.renderOptions.ResumeLayout(false);
            this.renderOptions.PerformLayout();
            this.nvPerfHUDGroupBox.ResumeLayout(false);
            this.nvPerfHUDGroupBox.PerformLayout();
            this.vsyncGroupBox.ResumeLayout(false);
            this.vsyncGroupBox.PerformLayout();
            this.fullScreenGroupBox.ResumeLayout(false);
            this.fullScreenGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox renderOptions;
		private System.Windows.Forms.Label videoModeLabel;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ComboBox videoModeComboBox;
		private System.Windows.Forms.PictureBox logoPicture;
		private System.Windows.Forms.Label renderSystemLabel;
        private System.Windows.Forms.ComboBox renderSystemComboBox;
        private System.Windows.Forms.GroupBox fullScreenGroupBox;
        private System.Windows.Forms.RadioButton fullScreenNoButton;
        private System.Windows.Forms.RadioButton fullScreenYesButton;
        private System.Windows.Forms.GroupBox nvPerfHUDGroupBox;
        private System.Windows.Forms.RadioButton nvPerfHUDNoButton;
        private System.Windows.Forms.RadioButton nvPerfHUDYesButton;
        private System.Windows.Forms.GroupBox vsyncGroupBox;
        private System.Windows.Forms.RadioButton vsyncNoButton;
        private System.Windows.Forms.RadioButton vsyncYesButton;
        private System.Windows.Forms.ComboBox aaComboBox;
        private System.Windows.Forms.Label aaLabel;
	}
}
