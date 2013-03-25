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

namespace Multiverse.Tools.MosaicCreator
{
    partial class MosaicCreator
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
            this.sourceLabel = new System.Windows.Forms.Label();
            this.tileSizeLabel = new System.Windows.Forms.Label();
            this.tileSizeComboBox = new System.Windows.Forms.ComboBox();
            this.mpsLabel = new System.Windows.Forms.Label();
            this.mpsComboBox = new System.Windows.Forms.ComboBox();
            this.sourceGroupBox = new System.Windows.Forms.GroupBox();
            this.sourceImageInfoLabel = new System.Windows.Forms.Label();
            this.heightMapCheckbox = new System.Windows.Forms.CheckBox();
            this.minAltLabel = new System.Windows.Forms.Label();
            this.minAltTextBox = new System.Windows.Forms.TextBox();
            this.altPanel = new System.Windows.Forms.Panel();
            this.maxAltTextBox = new System.Windows.Forms.TextBox();
            this.maxAltLabel = new System.Windows.Forms.Label();
            this.sourceImageFilenameLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMosaicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchOnlineHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchReleaseNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submitFeedbackOrABugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMosaicCreatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourceGroupBox.SuspendLayout();
            this.altPanel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sourceLabel
            // 
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Location = new System.Drawing.Point(27, 43);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(76, 13);
            this.sourceLabel.TabIndex = 0;
            this.sourceLabel.Text = "Source Image:";
            // 
            // tileSizeLabel
            // 
            this.tileSizeLabel.AutoSize = true;
            this.tileSizeLabel.Location = new System.Drawing.Point(27, 81);
            this.tileSizeLabel.Name = "tileSizeLabel";
            this.tileSizeLabel.Size = new System.Drawing.Size(50, 13);
            this.tileSizeLabel.TabIndex = 2;
            this.tileSizeLabel.Text = "Tile Size:";
            // 
            // tileSizeComboBox
            // 
            this.tileSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tileSizeComboBox.FormattingEnabled = true;
            this.tileSizeComboBox.Items.AddRange(new object[] {
            "128",
            "256",
            "512",
            "1024",
            "2048"});
            this.tileSizeComboBox.Location = new System.Drawing.Point(144, 78);
            this.tileSizeComboBox.Name = "tileSizeComboBox";
            this.tileSizeComboBox.Size = new System.Drawing.Size(121, 21);
            this.tileSizeComboBox.TabIndex = 3;
            // 
            // mpsLabel
            // 
            this.mpsLabel.AutoSize = true;
            this.mpsLabel.Location = new System.Drawing.Point(27, 120);
            this.mpsLabel.Name = "mpsLabel";
            this.mpsLabel.Size = new System.Drawing.Size(86, 13);
            this.mpsLabel.TabIndex = 6;
            this.mpsLabel.Text = "Meters Per Pixel:";
            // 
            // mpsComboBox
            // 
            this.mpsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mpsComboBox.FormattingEnabled = true;
            this.mpsComboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128",
            "256"});
            this.mpsComboBox.Location = new System.Drawing.Point(144, 117);
            this.mpsComboBox.Name = "mpsComboBox";
            this.mpsComboBox.Size = new System.Drawing.Size(121, 21);
            this.mpsComboBox.TabIndex = 7;
            // 
            // sourceGroupBox
            // 
            this.sourceGroupBox.Controls.Add(this.sourceImageInfoLabel);
            this.sourceGroupBox.Location = new System.Drawing.Point(308, 68);
            this.sourceGroupBox.Name = "sourceGroupBox";
            this.sourceGroupBox.Size = new System.Drawing.Size(242, 88);
            this.sourceGroupBox.TabIndex = 4;
            this.sourceGroupBox.TabStop = false;
            this.sourceGroupBox.Text = "Source Image Info";
            // 
            // sourceImageInfoLabel
            // 
            this.sourceImageInfoLabel.AutoSize = true;
            this.sourceImageInfoLabel.Location = new System.Drawing.Point(18, 28);
            this.sourceImageInfoLabel.Name = "sourceImageInfoLabel";
            this.sourceImageInfoLabel.Size = new System.Drawing.Size(0, 13);
            this.sourceImageInfoLabel.TabIndex = 5;
            // 
            // heightMapCheckbox
            // 
            this.heightMapCheckbox.AutoSize = true;
            this.heightMapCheckbox.Location = new System.Drawing.Point(30, 205);
            this.heightMapCheckbox.Name = "heightMapCheckbox";
            this.heightMapCheckbox.Size = new System.Drawing.Size(81, 17);
            this.heightMapCheckbox.TabIndex = 8;
            this.heightMapCheckbox.Text = "Height Map";
            this.heightMapCheckbox.UseVisualStyleBackColor = true;
            this.heightMapCheckbox.CheckedChanged += new System.EventHandler(this.heightMapCheckbox_CheckedChanged);
            // 
            // minAltLabel
            // 
            this.minAltLabel.AutoSize = true;
            this.minAltLabel.Location = new System.Drawing.Point(7, 16);
            this.minAltLabel.Name = "minAltLabel";
            this.minAltLabel.Size = new System.Drawing.Size(89, 13);
            this.minAltLabel.TabIndex = 10;
            this.minAltLabel.Text = "Minimum Altitude:";
            // 
            // minAltTextBox
            // 
            this.minAltTextBox.Location = new System.Drawing.Point(102, 13);
            this.minAltTextBox.Name = "minAltTextBox";
            this.minAltTextBox.Size = new System.Drawing.Size(100, 20);
            this.minAltTextBox.TabIndex = 11;
            this.minAltTextBox.Text = "0";
            // 
            // altPanel
            // 
            this.altPanel.Controls.Add(this.maxAltTextBox);
            this.altPanel.Controls.Add(this.maxAltLabel);
            this.altPanel.Controls.Add(this.minAltLabel);
            this.altPanel.Controls.Add(this.minAltTextBox);
            this.altPanel.Enabled = false;
            this.altPanel.Location = new System.Drawing.Point(134, 190);
            this.altPanel.Name = "altPanel";
            this.altPanel.Size = new System.Drawing.Size(459, 50);
            this.altPanel.TabIndex = 9;
            // 
            // maxAltTextBox
            // 
            this.maxAltTextBox.Location = new System.Drawing.Point(337, 13);
            this.maxAltTextBox.Name = "maxAltTextBox";
            this.maxAltTextBox.Size = new System.Drawing.Size(100, 20);
            this.maxAltTextBox.TabIndex = 13;
            this.maxAltTextBox.Text = "1000";
            // 
            // maxAltLabel
            // 
            this.maxAltLabel.AutoSize = true;
            this.maxAltLabel.Location = new System.Drawing.Point(239, 16);
            this.maxAltLabel.Name = "maxAltLabel";
            this.maxAltLabel.Size = new System.Drawing.Size(92, 13);
            this.maxAltLabel.TabIndex = 12;
            this.maxAltLabel.Text = "Maximum Altitude:";
            // 
            // sourceImageFilenameLabel
            // 
            this.sourceImageFilenameLabel.AutoSize = true;
            this.sourceImageFilenameLabel.Location = new System.Drawing.Point(131, 43);
            this.sourceImageFilenameLabel.Name = "sourceImageFilenameLabel";
            this.sourceImageFilenameLabel.Size = new System.Drawing.Size(0, 13);
            this.sourceImageFilenameLabel.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(603, 24);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadImageToolStripMenuItem,
            this.saveMosaicToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadImageToolStripMenuItem
            // 
            this.loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            this.loadImageToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.loadImageToolStripMenuItem.Text = "Load Image...";
            this.loadImageToolStripMenuItem.Click += new System.EventHandler(this.loadImageToolStripMenuItem_Click);
            // 
            // saveMosaicToolStripMenuItem
            // 
            this.saveMosaicToolStripMenuItem.Name = "saveMosaicToolStripMenuItem";
            this.saveMosaicToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.saveMosaicToolStripMenuItem.Text = "Save Mosaic...";
            this.saveMosaicToolStripMenuItem.Click += new System.EventHandler(this.saveMosaicToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchOnlineHelpToolStripMenuItem,
            this.launchReleaseNotesToolStripMenuItem,
            this.submitFeedbackOrABugToolStripMenuItem,
            this.aboutMosaicCreatorToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // launchOnlineHelpToolStripMenuItem
            // 
            this.launchOnlineHelpToolStripMenuItem.Name = "launchOnlineHelpToolStripMenuItem";
            this.launchOnlineHelpToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.launchOnlineHelpToolStripMenuItem.Text = "Launch Online Help";
            this.launchOnlineHelpToolStripMenuItem.Click += new System.EventHandler(this.launchOnlineHelpToolStripMenuItem_Click);
            // 
            // launchReleaseNotesToolStripMenuItem
            // 
            this.launchReleaseNotesToolStripMenuItem.Name = "launchReleaseNotesToolStripMenuItem";
            this.launchReleaseNotesToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.launchReleaseNotesToolStripMenuItem.Text = "Launch Release Notes";
            this.launchReleaseNotesToolStripMenuItem.Click += new System.EventHandler(this.launchReleaseNotesToolStripMenuItem_Click);
            // 
            // submitFeedbackOrABugToolStripMenuItem
            // 
            this.submitFeedbackOrABugToolStripMenuItem.Name = "submitFeedbackOrABugToolStripMenuItem";
            this.submitFeedbackOrABugToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.submitFeedbackOrABugToolStripMenuItem.Text = "Submit Feedback or a Bug";
            this.submitFeedbackOrABugToolStripMenuItem.Click += new System.EventHandler(this.submitFeedbackOrABugToolStripMenuItem_Click);
            // 
            // aboutMosaicCreatorToolStripMenuItem
            // 
            this.aboutMosaicCreatorToolStripMenuItem.Name = "aboutMosaicCreatorToolStripMenuItem";
            this.aboutMosaicCreatorToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.aboutMosaicCreatorToolStripMenuItem.Text = "About Mosaic Creator";
            this.aboutMosaicCreatorToolStripMenuItem.Click += new System.EventHandler(this.aboutMosaicCreatorToolStripMenuItem_Click);
            // 
            // MosaicCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 251);
            this.Controls.Add(this.sourceImageFilenameLabel);
            this.Controls.Add(this.altPanel);
            this.Controls.Add(this.heightMapCheckbox);
            this.Controls.Add(this.sourceGroupBox);
            this.Controls.Add(this.mpsComboBox);
            this.Controls.Add(this.mpsLabel);
            this.Controls.Add(this.tileSizeComboBox);
            this.Controls.Add(this.tileSizeLabel);
            this.Controls.Add(this.sourceLabel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MosaicCreator";
            this.Text = "Mosaic Creator";
            this.sourceGroupBox.ResumeLayout(false);
            this.sourceGroupBox.PerformLayout();
            this.altPanel.ResumeLayout(false);
            this.altPanel.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.Label tileSizeLabel;
        private System.Windows.Forms.ComboBox tileSizeComboBox;
        private System.Windows.Forms.Label mpsLabel;
        private System.Windows.Forms.ComboBox mpsComboBox;
        private System.Windows.Forms.GroupBox sourceGroupBox;
        private System.Windows.Forms.Label sourceImageInfoLabel;
        private System.Windows.Forms.CheckBox heightMapCheckbox;
        private System.Windows.Forms.Label minAltLabel;
        private System.Windows.Forms.TextBox minAltTextBox;
        private System.Windows.Forms.Panel altPanel;
        private System.Windows.Forms.TextBox maxAltTextBox;
        private System.Windows.Forms.Label maxAltLabel;
        private System.Windows.Forms.Label sourceImageFilenameLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMosaicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem launchOnlineHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem launchReleaseNotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem submitFeedbackOrABugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMosaicCreatorToolStripMenuItem;
    }
}

