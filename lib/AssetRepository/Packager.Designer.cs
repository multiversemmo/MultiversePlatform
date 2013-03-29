namespace Multiverse.AssetRepository
{
    partial class assetPackagerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(assetPackagerForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.repositoryLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.openWorldAssetsDialog = new System.Windows.Forms.OpenFileDialog();
            this.openAssetListFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.newMediaTreeDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.generateBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.topPanel = new System.Windows.Forms.Panel();
            this.worldAssetsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.worldNamePanel = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.worldNameTextBox = new System.Windows.Forms.TextBox();
            this.openWorldAssetsButton = new System.Windows.Forms.Button();
            this.aboutButton = new System.Windows.Forms.Button();
            this.designateSourceMediaTreeButton = new System.Windows.Forms.Button();
            this.worldAssetsFilesLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.bottomLeftButtonPanel = new System.Windows.Forms.Panel();
            this.createEmptyRepositoryButton = new System.Windows.Forms.Button();
            this.bottomRightButtonPanel = new System.Windows.Forms.Panel();
            this.copyMediaButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.logListBox = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.copyAssetDefinitionsCheckBox = new System.Windows.Forms.CheckBox();
            this.newMediaTreeOpenButton = new System.Windows.Forms.Button();
            this.newTreeTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.assetListCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.assetListFileOpenButton = new System.Windows.Forms.Button();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.helpMenuToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchOnlineHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submitFeedbackOrBugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.releaseNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutAssetPackagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.worldNamePanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.bottomLeftButtonPanel.SuspendLayout();
            this.bottomRightButtonPanel.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.repositoryLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 542);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(602, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // repositoryLabel
            // 
            this.repositoryLabel.Name = "repositoryLabel";
            this.repositoryLabel.Size = new System.Drawing.Size(144, 17);
            this.repositoryLabel.Text = "No Repository Designated";
            // 
            // openWorldAssetsDialog
            // 
            this.openWorldAssetsDialog.DefaultExt = "worldassets";
            this.openWorldAssetsDialog.Filter = "World Editor Asset List Files (*.worldassets)|*.worldassets|All Files (*.*)|*.*";
            this.openWorldAssetsDialog.Multiselect = true;
            this.openWorldAssetsDialog.Title = "Open World Editor Assets File";
            // 
            // openAssetListFileDialog
            // 
            this.openAssetListFileDialog.DefaultExt = "assetlist";
            this.openAssetListFileDialog.Filter = "Asset Name List Files (*.assetlist)|*.assetlist|All Files (*.*)|*.*";
            this.openAssetListFileDialog.Title = "Open Asset Names Asset List File";
            // 
            // generateBackgroundWorker
            // 
            this.generateBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.generateBackgroundWorker_DoWork);
            this.generateBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.generateBackgroundWorker_RunWorkerCompleted);
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.worldAssetsCheckedListBox);
            this.topPanel.Controls.Add(this.worldNamePanel);
            this.topPanel.Controls.Add(this.openWorldAssetsButton);
            this.topPanel.Controls.Add(this.aboutButton);
            this.topPanel.Controls.Add(this.designateSourceMediaTreeButton);
            this.topPanel.Controls.Add(this.worldAssetsFilesLabel);
            this.topPanel.Location = new System.Drawing.Point(2, 29);
            this.topPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(599, 135);
            this.topPanel.TabIndex = 22;
            // 
            // worldAssetsCheckedListBox
            // 
            this.worldAssetsCheckedListBox.CheckOnClick = true;
            this.worldAssetsCheckedListBox.FormattingEnabled = true;
            this.worldAssetsCheckedListBox.Location = new System.Drawing.Point(117, 51);
            this.worldAssetsCheckedListBox.Name = "worldAssetsCheckedListBox";
            this.worldAssetsCheckedListBox.Size = new System.Drawing.Size(391, 64);
            this.worldAssetsCheckedListBox.TabIndex = 39;
            // 
            // worldNamePanel
            // 
            this.worldNamePanel.Controls.Add(this.label5);
            this.worldNamePanel.Controls.Add(this.worldNameTextBox);
            this.worldNamePanel.Location = new System.Drawing.Point(340, 11);
            this.worldNamePanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.worldNamePanel.Name = "worldNamePanel";
            this.worldNamePanel.Size = new System.Drawing.Size(167, 25);
            this.worldNamePanel.TabIndex = 38;
            this.worldNamePanel.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 5);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 39;
            this.label5.Text = "World Name";
            // 
            // worldNameTextBox
            // 
            this.worldNameTextBox.Location = new System.Drawing.Point(74, 3);
            this.worldNameTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.worldNameTextBox.Name = "worldNameTextBox";
            this.worldNameTextBox.Size = new System.Drawing.Size(94, 20);
            this.worldNameTextBox.TabIndex = 38;
            // 
            // openWorldAssetsButton
            // 
            this.openWorldAssetsButton.Location = new System.Drawing.Point(519, 51);
            this.openWorldAssetsButton.Name = "openWorldAssetsButton";
            this.openWorldAssetsButton.Size = new System.Drawing.Size(60, 23);
            this.openWorldAssetsButton.TabIndex = 35;
            this.openWorldAssetsButton.Text = "Browse...";
            this.openWorldAssetsButton.UseVisualStyleBackColor = true;
            this.openWorldAssetsButton.Click += new System.EventHandler(this.openWorldAssetsButton_Click);
            // 
            // aboutButton
            // 
            this.aboutButton.Location = new System.Drawing.Point(519, 11);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(60, 23);
            this.aboutButton.TabIndex = 25;
            this.aboutButton.Text = "About";
            this.aboutButton.UseVisualStyleBackColor = true;
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // designateSourceMediaTreeButton
            // 
            this.designateSourceMediaTreeButton.Location = new System.Drawing.Point(117, 11);
            this.designateSourceMediaTreeButton.Name = "designateSourceMediaTreeButton";
            this.designateSourceMediaTreeButton.Size = new System.Drawing.Size(241, 23);
            this.designateSourceMediaTreeButton.TabIndex = 24;
            this.designateSourceMediaTreeButton.Text = "Designate Source Asset Repository";
            this.designateSourceMediaTreeButton.UseVisualStyleBackColor = true;
            this.designateSourceMediaTreeButton.Click += new System.EventHandler(this.designateSourceMediaTreeButton_Click);
            // 
            // worldAssetsFilesLabel
            // 
            this.worldAssetsFilesLabel.Location = new System.Drawing.Point(11, 51);
            this.worldAssetsFilesLabel.Name = "worldAssetsFilesLabel";
            this.worldAssetsFilesLabel.Size = new System.Drawing.Size(100, 72);
            this.worldAssetsFilesLabel.TabIndex = 23;
            this.worldAssetsFilesLabel.Text = "World Assets Files";
            this.worldAssetsFilesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.bottomLeftButtonPanel);
            this.bottomPanel.Controls.Add(this.bottomRightButtonPanel);
            this.bottomPanel.Controls.Add(this.logListBox);
            this.bottomPanel.Controls.Add(this.label4);
            this.bottomPanel.Controls.Add(this.copyAssetDefinitionsCheckBox);
            this.bottomPanel.Controls.Add(this.newMediaTreeOpenButton);
            this.bottomPanel.Controls.Add(this.newTreeTextBox);
            this.bottomPanel.Controls.Add(this.label3);
            this.bottomPanel.Controls.Add(this.assetListCheckedListBox);
            this.bottomPanel.Controls.Add(this.label2);
            this.bottomPanel.Controls.Add(this.assetListFileOpenButton);
            this.bottomPanel.Location = new System.Drawing.Point(2, 167);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(599, 376);
            this.bottomPanel.TabIndex = 24;
            // 
            // bottomLeftButtonPanel
            // 
            this.bottomLeftButtonPanel.Controls.Add(this.createEmptyRepositoryButton);
            this.bottomLeftButtonPanel.Location = new System.Drawing.Point(18, 325);
            this.bottomLeftButtonPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bottomLeftButtonPanel.Name = "bottomLeftButtonPanel";
            this.bottomLeftButtonPanel.Size = new System.Drawing.Size(170, 48);
            this.bottomLeftButtonPanel.TabIndex = 48;
            // 
            // createEmptyRepositoryButton
            // 
            this.createEmptyRepositoryButton.Location = new System.Drawing.Point(0, 16);
            this.createEmptyRepositoryButton.Name = "createEmptyRepositoryButton";
            this.createEmptyRepositoryButton.Size = new System.Drawing.Size(160, 21);
            this.createEmptyRepositoryButton.TabIndex = 44;
            this.createEmptyRepositoryButton.Text = "Create Empty Asset Repository";
            this.createEmptyRepositoryButton.UseVisualStyleBackColor = true;
            this.createEmptyRepositoryButton.Click += new System.EventHandler(this.createEmptyRepositoryButton_Click);
            // 
            // bottomRightButtonPanel
            // 
            this.bottomRightButtonPanel.Controls.Add(this.copyMediaButton);
            this.bottomRightButtonPanel.Controls.Add(this.cancelButton);
            this.bottomRightButtonPanel.Location = new System.Drawing.Point(321, 326);
            this.bottomRightButtonPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bottomRightButtonPanel.Name = "bottomRightButtonPanel";
            this.bottomRightButtonPanel.Size = new System.Drawing.Size(260, 50);
            this.bottomRightButtonPanel.TabIndex = 47;
            // 
            // copyMediaButton
            // 
            this.copyMediaButton.Location = new System.Drawing.Point(5, 17);
            this.copyMediaButton.Name = "copyMediaButton";
            this.copyMediaButton.Size = new System.Drawing.Size(182, 21);
            this.copyMediaButton.TabIndex = 48;
            this.copyMediaButton.Text = "Copy Assets To Asset Repository";
            this.copyMediaButton.UseVisualStyleBackColor = true;
            this.copyMediaButton.Click += new System.EventHandler(this.copyMediaButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(190, 17);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(70, 21);
            this.cancelButton.TabIndex = 47;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // logListBox
            // 
            this.logListBox.FormattingEnabled = true;
            this.logListBox.Location = new System.Drawing.Point(18, 205);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(564, 121);
            this.logListBox.TabIndex = 45;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 188);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 44;
            this.label4.Text = "Packager Log";
            // 
            // copyAssetDefinitionsCheckBox
            // 
            this.copyAssetDefinitionsCheckBox.AutoSize = true;
            this.copyAssetDefinitionsCheckBox.Checked = true;
            this.copyAssetDefinitionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.copyAssetDefinitionsCheckBox.Location = new System.Drawing.Point(120, 126);
            this.copyAssetDefinitionsCheckBox.Name = "copyAssetDefinitionsCheckBox";
            this.copyAssetDefinitionsCheckBox.Size = new System.Drawing.Size(275, 17);
            this.copyAssetDefinitionsCheckBox.TabIndex = 42;
            this.copyAssetDefinitionsCheckBox.Text = "Copy Asset Definition Files (.asset and .assetlist files) ";
            this.copyAssetDefinitionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // newMediaTreeOpenButton
            // 
            this.newMediaTreeOpenButton.Location = new System.Drawing.Point(521, 154);
            this.newMediaTreeOpenButton.Name = "newMediaTreeOpenButton";
            this.newMediaTreeOpenButton.Size = new System.Drawing.Size(60, 23);
            this.newMediaTreeOpenButton.TabIndex = 40;
            this.newMediaTreeOpenButton.Text = "Browse...";
            this.newMediaTreeOpenButton.UseVisualStyleBackColor = true;
            this.newMediaTreeOpenButton.Click += new System.EventHandler(this.newMediaTreeOpenButton_Click);
            // 
            // newTreeTextBox
            // 
            this.newTreeTextBox.Location = new System.Drawing.Point(120, 156);
            this.newTreeTextBox.Name = "newTreeTextBox";
            this.newTreeTextBox.Size = new System.Drawing.Size(391, 20);
            this.newTreeTextBox.TabIndex = 39;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 31);
            this.label3.TabIndex = 38;
            this.label3.Text = "Target Asset Repository";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // assetListCheckedListBox
            // 
            this.assetListCheckedListBox.CheckOnClick = true;
            this.assetListCheckedListBox.FormattingEnabled = true;
            this.assetListCheckedListBox.Location = new System.Drawing.Point(117, 15);
            this.assetListCheckedListBox.Name = "assetListCheckedListBox";
            this.assetListCheckedListBox.Size = new System.Drawing.Size(391, 94);
            this.assetListCheckedListBox.TabIndex = 37;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(15, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 100);
            this.label2.TabIndex = 36;
            this.label2.Text = "Asset List Files";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // assetListFileOpenButton
            // 
            this.assetListFileOpenButton.Location = new System.Drawing.Point(518, 15);
            this.assetListFileOpenButton.Name = "assetListFileOpenButton";
            this.assetListFileOpenButton.Size = new System.Drawing.Size(60, 23);
            this.assetListFileOpenButton.TabIndex = 35;
            this.assetListFileOpenButton.Text = "Browse...";
            this.assetListFileOpenButton.UseVisualStyleBackColor = true;
            this.assetListFileOpenButton.Click += new System.EventHandler(this.assetListFileOpenButton_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpMenuToolStripItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(602, 24);
            this.menuStrip.TabIndex = 25;
            this.menuStrip.Text = "Help";
            // 
            // helpMenuToolStripItem
            // 
            this.helpMenuToolStripItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchOnlineHelpToolStripMenuItem,
            this.submitFeedbackOrBugToolStripMenuItem,
            this.releaseNotesToolStripMenuItem,
            this.aboutAssetPackagerToolStripMenuItem});
            this.helpMenuToolStripItem.Name = "helpMenuToolStripItem";
            this.helpMenuToolStripItem.Size = new System.Drawing.Size(44, 20);
            this.helpMenuToolStripItem.Text = "&Help";
            // 
            // launchOnlineHelpToolStripMenuItem
            // 
            this.launchOnlineHelpToolStripMenuItem.Name = "launchOnlineHelpToolStripMenuItem";
            this.launchOnlineHelpToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.launchOnlineHelpToolStripMenuItem.Text = "Launch Online Help";
            this.launchOnlineHelpToolStripMenuItem.Click += new System.EventHandler(this.launchOnlineHelpToolStripMenuItem_Clicked);
            // 
            // submitFeedbackOrBugToolStripMenuItem
            // 
            this.submitFeedbackOrBugToolStripMenuItem.Name = "submitFeedbackOrBugToolStripMenuItem";
            this.submitFeedbackOrBugToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.submitFeedbackOrBugToolStripMenuItem.Text = "Submit Feedback or a Bug";
            this.submitFeedbackOrBugToolStripMenuItem.Click += new System.EventHandler(this.submitFeedbackOrBugToolStripMenuItem_Clicked);
            // 
            // releaseNotesToolStripMenuItem
            // 
            this.releaseNotesToolStripMenuItem.Name = "releaseNotesToolStripMenuItem";
            this.releaseNotesToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.releaseNotesToolStripMenuItem.Text = "Release Notes";
            this.releaseNotesToolStripMenuItem.Click += new System.EventHandler(this.releaseNotesToolStripMenuItem_clicked);
            // 
            // aboutAssetPackagerToolStripMenuItem
            // 
            this.aboutAssetPackagerToolStripMenuItem.Name = "aboutAssetPackagerToolStripMenuItem";
            this.aboutAssetPackagerToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.aboutAssetPackagerToolStripMenuItem.Text = "About Asset Packager";
            this.aboutAssetPackagerToolStripMenuItem.Click += new System.EventHandler(this.aboutAssetPackagerToolStripMenuItem_Clicked);
            // 
            // assetPackagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 564);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "assetPackagerForm";
            this.Text = "Asset Packager";
            this.Shown += new System.EventHandler(this.assetPackagerForm_Shown);
            this.Resize += new System.EventHandler(this.assetPackagerForm_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.topPanel.ResumeLayout(false);
            this.worldNamePanel.ResumeLayout(false);
            this.worldNamePanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.bottomLeftButtonPanel.ResumeLayout(false);
            this.bottomRightButtonPanel.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel repositoryLabel;
        private System.Windows.Forms.OpenFileDialog openWorldAssetsDialog;
        private System.Windows.Forms.OpenFileDialog openAssetListFileDialog;
        private System.Windows.Forms.FolderBrowserDialog newMediaTreeDialog;
        private System.ComponentModel.BackgroundWorker generateBackgroundWorker;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button aboutButton;
        private System.Windows.Forms.Button designateSourceMediaTreeButton;
        private System.Windows.Forms.Label worldAssetsFilesLabel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.ListBox logListBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox copyAssetDefinitionsCheckBox;
        private System.Windows.Forms.Button newMediaTreeOpenButton;
        private System.Windows.Forms.TextBox newTreeTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox assetListCheckedListBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button assetListFileOpenButton;
        private System.Windows.Forms.Button openWorldAssetsButton;
        private System.Windows.Forms.Panel bottomRightButtonPanel;
        private System.Windows.Forms.Button copyMediaButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel bottomLeftButtonPanel;
        private System.Windows.Forms.Button createEmptyRepositoryButton;
        private System.Windows.Forms.Panel worldNamePanel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox worldNameTextBox;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem helpMenuToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem launchOnlineHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem submitFeedbackOrBugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem releaseNotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutAssetPackagerToolStripMenuItem;
        private System.Windows.Forms.CheckedListBox worldAssetsCheckedListBox;
    }
}

