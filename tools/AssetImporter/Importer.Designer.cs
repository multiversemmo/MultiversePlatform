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

namespace AssetImporter
{
    partial class Importer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Importer));
            this.assetNameTextBox = new System.Windows.Forms.TextBox();
            this.assetTypeComboBox = new System.Windows.Forms.ComboBox();
            this.openSourceFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.repositoryLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.openAssetDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveAssetDialog = new System.Windows.Forms.SaveFileDialog();
            this.categoryPanel = new System.Windows.Forms.Panel();
            this.categoryComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newAssetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openAssetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAssetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAssetAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.designateRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchOnlineHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submitFeedbackMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.releaseNotesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutAssetImporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.newAssetToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openAssetToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveAssetToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.designateRepositoryToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.filesLabel = new System.Windows.Forms.Label();
            this.filesListBox = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.trashPictureBox = new System.Windows.Forms.PictureBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.propertyEditorButton = new System.Windows.Forms.Button();
            this.statusStrip.SuspendLayout();
            this.categoryPanel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trashPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // assetNameTextBox
            // 
            this.assetNameTextBox.Location = new System.Drawing.Point(111, 133);
            this.assetNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.assetNameTextBox.Name = "assetNameTextBox";
            this.assetNameTextBox.Size = new System.Drawing.Size(409, 22);
            this.assetNameTextBox.TabIndex = 6;
            this.assetNameTextBox.TextChanged += new System.EventHandler(this.assetNameTextBox_TextChanged);
            // 
            // assetTypeComboBox
            // 
            this.assetTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assetTypeComboBox.FormattingEnabled = true;
            this.assetTypeComboBox.Location = new System.Drawing.Point(111, 97);
            this.assetTypeComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.assetTypeComboBox.Name = "assetTypeComboBox";
            this.assetTypeComboBox.Size = new System.Drawing.Size(199, 24);
            this.assetTypeComboBox.TabIndex = 8;
            this.assetTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.assetTypeComboBox_SelectedIndexChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.repositoryLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 675);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip.Size = new System.Drawing.Size(1084, 22);
            this.statusStrip.TabIndex = 9;
            // 
            // repositoryLabel
            // 
            this.repositoryLabel.Name = "repositoryLabel";
            this.repositoryLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // openAssetDialog
            // 
            this.openAssetDialog.DefaultExt = "asset";
            this.openAssetDialog.Filter = "Asset Files (*.asset)|*.asset|All Files (*.*)|*.*";
            // 
            // saveAssetDialog
            // 
            this.saveAssetDialog.Filter = "Asset Files (*.asset)|*.asset|All Files (*.*)|*.*";
            // 
            // categoryPanel
            // 
            this.categoryPanel.Controls.Add(this.categoryComboBox);
            this.categoryPanel.Controls.Add(this.label3);
            this.categoryPanel.Location = new System.Drawing.Point(708, 73);
            this.categoryPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.categoryPanel.Name = "categoryPanel";
            this.categoryPanel.Size = new System.Drawing.Size(273, 48);
            this.categoryPanel.TabIndex = 14;
            // 
            // categoryComboBox
            // 
            this.categoryComboBox.FormattingEnabled = true;
            this.categoryComboBox.Location = new System.Drawing.Point(76, 11);
            this.categoryComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.categoryComboBox.Name = "categoryComboBox";
            this.categoryComboBox.Size = new System.Drawing.Size(188, 24);
            this.categoryComboBox.TabIndex = 1;
            this.categoryComboBox.SelectedIndexChanged += new System.EventHandler(this.categoryComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Category:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1084, 26);
            this.menuStrip1.TabIndex = 15;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newAssetToolStripMenuItem,
            this.openAssetToolStripMenuItem,
            this.saveAssetToolStripMenuItem,
            this.saveAssetAsToolStripMenuItem,
            this.designateRepositoryToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newAssetToolStripMenuItem
            // 
            this.newAssetToolStripMenuItem.Name = "newAssetToolStripMenuItem";
            this.newAssetToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.newAssetToolStripMenuItem.Text = "&New Asset";
            this.newAssetToolStripMenuItem.Click += new System.EventHandler(this.newAssetToolStripMenuItem_Click);
            // 
            // openAssetToolStripMenuItem
            // 
            this.openAssetToolStripMenuItem.Name = "openAssetToolStripMenuItem";
            this.openAssetToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.openAssetToolStripMenuItem.Text = "&Open Asset...";
            this.openAssetToolStripMenuItem.Click += new System.EventHandler(this.openAssetToolStripMenuItem_Click);
            // 
            // saveAssetToolStripMenuItem
            // 
            this.saveAssetToolStripMenuItem.Name = "saveAssetToolStripMenuItem";
            this.saveAssetToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.saveAssetToolStripMenuItem.Text = "&Save Asset";
            this.saveAssetToolStripMenuItem.Click += new System.EventHandler(this.saveAssetToolStripMenuItem_Click);
            // 
            // saveAssetAsToolStripMenuItem
            // 
            this.saveAssetAsToolStripMenuItem.Name = "saveAssetAsToolStripMenuItem";
            this.saveAssetAsToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.saveAssetAsToolStripMenuItem.Text = "Save Asset &As...";
            this.saveAssetAsToolStripMenuItem.Click += new System.EventHandler(this.saveAssetAsToolStripMenuItem_Click);
            // 
            // designateRepositoryToolStripMenuItem
            // 
            this.designateRepositoryToolStripMenuItem.Name = "designateRepositoryToolStripMenuItem";
            this.designateRepositoryToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.designateRepositoryToolStripMenuItem.Text = "&Designate Repository...";
            this.designateRepositoryToolStripMenuItem.Click += new System.EventHandler(this.designateRepositoryToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchOnlineHelpMenuItem,
            this.submitFeedbackMenuItem,
            this.releaseNotesMenuItem,
            this.aboutAssetImporterToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(48, 22);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // launchOnlineHelpMenuItem
            // 
            this.launchOnlineHelpMenuItem.Name = "launchOnlineHelpMenuItem";
            this.launchOnlineHelpMenuItem.Size = new System.Drawing.Size(263, 22);
            this.launchOnlineHelpMenuItem.Text = "Launch Online Help";
            this.launchOnlineHelpMenuItem.Click += new System.EventHandler(this.launchOnlineHelpMenuItem_Clicked);
            // 
            // submitFeedbackMenuItem
            // 
            this.submitFeedbackMenuItem.Name = "submitFeedbackMenuItem";
            this.submitFeedbackMenuItem.Size = new System.Drawing.Size(263, 22);
            this.submitFeedbackMenuItem.Text = "Submit Feedback or a Bug";
            this.submitFeedbackMenuItem.Click += new System.EventHandler(this.submitFeedbackMenuItem_Clicked);
            // 
            // releaseNotesMenuItem
            // 
            this.releaseNotesMenuItem.Name = "releaseNotesMenuItem";
            this.releaseNotesMenuItem.Size = new System.Drawing.Size(263, 22);
            this.releaseNotesMenuItem.Text = "Release Notes";
            this.releaseNotesMenuItem.Click += new System.EventHandler(this.releaseNotesMenuItem_Clicked);
            // 
            // aboutAssetImporterToolStripMenuItem
            // 
            this.aboutAssetImporterToolStripMenuItem.Name = "aboutAssetImporterToolStripMenuItem";
            this.aboutAssetImporterToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
            this.aboutAssetImporterToolStripMenuItem.Text = "&About Asset Importer...";
            this.aboutAssetImporterToolStripMenuItem.Click += new System.EventHandler(this.aboutAssetImporterToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(36, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newAssetToolStripButton,
            this.openAssetToolStripButton,
            this.saveAssetToolStripButton,
            this.designateRepositoryToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 26);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1084, 39);
            this.toolStrip1.TabIndex = 16;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // newAssetToolStripButton
            // 
            this.newAssetToolStripButton.AutoSize = false;
            this.newAssetToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newAssetToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newAssetToolStripButton.Image")));
            this.newAssetToolStripButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.newAssetToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.newAssetToolStripButton.ImageTransparentColor = System.Drawing.SystemColors.Control;
            this.newAssetToolStripButton.Name = "newAssetToolStripButton";
            this.newAssetToolStripButton.Size = new System.Drawing.Size(38, 35);
            this.newAssetToolStripButton.Text = "New";
            this.newAssetToolStripButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.newAssetToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.newAssetToolStripButton.ToolTipText = "Create New Asset";
            this.newAssetToolStripButton.Click += new System.EventHandler(this.newAssetToolStripButton_Click);
            // 
            // openAssetToolStripButton
            // 
            this.openAssetToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openAssetToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openAssetToolStripButton.Image")));
            this.openAssetToolStripButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.openAssetToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.openAssetToolStripButton.ImageTransparentColor = System.Drawing.SystemColors.ControlLight;
            this.openAssetToolStripButton.Name = "openAssetToolStripButton";
            this.openAssetToolStripButton.Size = new System.Drawing.Size(38, 36);
            this.openAssetToolStripButton.Text = "Open";
            this.openAssetToolStripButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.openAssetToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.openAssetToolStripButton.ToolTipText = "Open Asset";
            this.openAssetToolStripButton.Click += new System.EventHandler(this.openAssetToolStripButton_Click);
            // 
            // saveAssetToolStripButton
            // 
            this.saveAssetToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAssetToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveAssetToolStripButton.Image")));
            this.saveAssetToolStripButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.saveAssetToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.saveAssetToolStripButton.ImageTransparentColor = System.Drawing.SystemColors.ControlLight;
            this.saveAssetToolStripButton.Name = "saveAssetToolStripButton";
            this.saveAssetToolStripButton.Size = new System.Drawing.Size(36, 36);
            this.saveAssetToolStripButton.Text = "Save";
            this.saveAssetToolStripButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.saveAssetToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.saveAssetToolStripButton.ToolTipText = "Save Asset";
            this.saveAssetToolStripButton.Click += new System.EventHandler(this.saveAssetToolStripButton_Click);
            // 
            // designateRepositoryToolStripButton
            // 
            this.designateRepositoryToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.designateRepositoryToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("designateRepositoryToolStripButton.Image")));
            this.designateRepositoryToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.designateRepositoryToolStripButton.Name = "designateRepositoryToolStripButton";
            this.designateRepositoryToolStripButton.Size = new System.Drawing.Size(40, 36);
            this.designateRepositoryToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.designateRepositoryToolStripButton.ToolTipText = "Designate Asset Repository Directory";
            this.designateRepositoryToolStripButton.Visible = false;
            this.designateRepositoryToolStripButton.Click += new System.EventHandler(this.designateRepositoryToolStripButton_Click);
            // 
            // filesLabel
            // 
            this.filesLabel.Location = new System.Drawing.Point(17, 190);
            this.filesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.filesLabel.Name = "filesLabel";
            this.filesLabel.Size = new System.Drawing.Size(290, 35);
            this.filesLabel.TabIndex = 17;
            this.filesLabel.Text = "Files";
            // 
            // filesListBox
            // 
            this.filesListBox.FormattingEnabled = true;
            this.filesListBox.ItemHeight = 16;
            this.filesListBox.Location = new System.Drawing.Point(20, 255);
            this.filesListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.filesListBox.Name = "filesListBox";
            this.filesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.filesListBox.Size = new System.Drawing.Size(287, 404);
            this.filesListBox.TabIndex = 18;
            this.filesListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.anyListBox_MouseUp);
            this.filesListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.anyListBox_MouseMove);
            this.filesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.anyListBox_MouseDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 228);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(295, 17);
            this.label4.TabIndex = 19;
            this.label4.Text = "(Drag and drop onto listboxes and text boxes)";
            // 
            // trashPictureBox
            // 
            this.trashPictureBox.AllowDrop = true;
            this.trashPictureBox.Image = global::AssetImporter.Properties.Resources.image4;
            this.trashPictureBox.Location = new System.Drawing.Point(1007, 69);
            this.trashPictureBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trashPictureBox.Name = "trashPictureBox";
            this.trashPictureBox.Size = new System.Drawing.Size(53, 84);
            this.trashPictureBox.TabIndex = 20;
            this.trashPictureBox.TabStop = false;
            this.trashPictureBox.DragOver += new System.Windows.Forms.DragEventHandler(this.trashPictureBox_DragOver);
            this.trashPictureBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.trashPictureBox_DragDrop);
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(337, 639);
            this.descriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(150, 17);
            this.descriptionLabel.TabIndex = 21;
            this.descriptionLabel.Text = "Description (Optional):";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Location = new System.Drawing.Point(491, 635);
            this.descriptionTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(568, 22);
            this.descriptionTextBox.TabIndex = 22;
            this.descriptionTextBox.TextChanged += new System.EventHandler(this.descriptionTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 138);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 17);
            this.label6.TabIndex = 5;
            this.label6.Text = "Asset Name:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 102);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 17);
            this.label7.TabIndex = 7;
            this.label7.Text = "Asset Type:";
            // 
            // propertyEditorButton
            // 
            this.propertyEditorButton.Location = new System.Drawing.Point(708, 132);
            this.propertyEditorButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.propertyEditorButton.Name = "propertyEditorButton";
            this.propertyEditorButton.Size = new System.Drawing.Size(115, 28);
            this.propertyEditorButton.TabIndex = 23;
            this.propertyEditorButton.Text = "Edit Properties";
            this.propertyEditorButton.UseVisualStyleBackColor = true;
            this.propertyEditorButton.Click += new System.EventHandler(this.propertyEditorButton_Clicked);
            // 
            // Importer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 697);
            this.Controls.Add(this.propertyEditorButton);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.trashPictureBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.filesListBox);
            this.Controls.Add(this.filesLabel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.categoryPanel);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.assetTypeComboBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.assetNameTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Importer";
            this.Text = "Asset Importer";
            this.Resize += new System.EventHandler(this.Importer_Resize);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Importer_FormClosing);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.categoryPanel.ResumeLayout(false);
            this.categoryPanel.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trashPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox assetNameTextBox;
        private System.Windows.Forms.ComboBox assetTypeComboBox;
        private System.Windows.Forms.OpenFileDialog openSourceFileDialog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel repositoryLabel;
        private System.Windows.Forms.OpenFileDialog openAssetDialog;
        private System.Windows.Forms.SaveFileDialog saveAssetDialog;
        private System.Windows.Forms.Panel categoryPanel;
        private System.Windows.Forms.ComboBox categoryComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newAssetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openAssetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAssetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAssetAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem designateRepositoryToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton newAssetToolStripButton;
        private System.Windows.Forms.ToolStripButton openAssetToolStripButton;
        private System.Windows.Forms.ToolStripButton saveAssetToolStripButton;
        private System.Windows.Forms.ToolStripButton designateRepositoryToolStripButton;
        private System.Windows.Forms.Label filesLabel;
        private System.Windows.Forms.ListBox filesListBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox trashPictureBox;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutAssetImporterToolStripMenuItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStripMenuItem launchOnlineHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem submitFeedbackMenuItem;
        private System.Windows.Forms.ToolStripMenuItem releaseNotesMenuItem;
        private System.Windows.Forms.Button propertyEditorButton;


    }
}

