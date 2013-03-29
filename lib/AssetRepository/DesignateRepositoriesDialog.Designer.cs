namespace Multiverse.AssetRepository
{
    partial class DesignateRepositoriesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DesignateRepositoriesDialog));
            this.FilesListBox = new System.Windows.Forms.ListBox();
            this.filesLabel = new System.Windows.Forms.Label();
            this.BrowseAndAddButton = new System.Windows.Forms.Button();
            this.RemoveDirectory = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.RepositoryFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.blahBlahLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FilesListBox
            // 
            this.FilesListBox.AllowDrop = true;
            this.FilesListBox.FormattingEnabled = true;
            this.FilesListBox.Location = new System.Drawing.Point(8, 76);
            this.FilesListBox.Name = "FilesListBox";
            this.FilesListBox.Size = new System.Drawing.Size(329, 199);
            this.FilesListBox.TabIndex = 20;
            this.FilesListBox.SelectedValueChanged += new System.EventHandler(this.FilesListBox_SelectedValueChanged);
            this.FilesListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilesListBox_DragDrop);
            this.FilesListBox.DragOver += new System.Windows.Forms.DragEventHandler(this.FilesListBox_DragOver);
            this.FilesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseDown);
            this.FilesListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseMove);
            this.FilesListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseUp);
            // 
            // filesLabel
            // 
            this.filesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.filesLabel.Location = new System.Drawing.Point(114, 7);
            this.filesLabel.Name = "filesLabel";
            this.filesLabel.Size = new System.Drawing.Size(100, 14);
            this.filesLabel.TabIndex = 19;
            this.filesLabel.Text = "Asset Repositories";
            // 
            // BrowseAndAddButton
            // 
            this.BrowseAndAddButton.Location = new System.Drawing.Point(8, 296);
            this.BrowseAndAddButton.Margin = new System.Windows.Forms.Padding(2);
            this.BrowseAndAddButton.Name = "BrowseAndAddButton";
            this.BrowseAndAddButton.Size = new System.Drawing.Size(168, 27);
            this.BrowseAndAddButton.TabIndex = 21;
            this.BrowseAndAddButton.Text = "Browse and Add Directory";
            this.BrowseAndAddButton.UseVisualStyleBackColor = true;
            this.BrowseAndAddButton.Click += new System.EventHandler(this.BrowseAndAddButton_Click);
            // 
            // RemoveDirectory
            // 
            this.RemoveDirectory.Location = new System.Drawing.Point(181, 296);
            this.RemoveDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.RemoveDirectory.Name = "RemoveDirectory";
            this.RemoveDirectory.Size = new System.Drawing.Size(155, 27);
            this.RemoveDirectory.TabIndex = 22;
            this.RemoveDirectory.Text = "Remove Directory";
            this.RemoveDirectory.UseVisualStyleBackColor = true;
            this.RemoveDirectory.Click += new System.EventHandler(this.RemoveDirectory_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(275, 357);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(61, 27);
            this.CancelButton.TabIndex = 24;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(198, 357);
            this.OKButton.Margin = new System.Windows.Forms.Padding(2);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(73, 27);
            this.OKButton.TabIndex = 23;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // blahBlahLabel
            // 
            this.blahBlahLabel.AutoSize = true;
            this.blahBlahLabel.Location = new System.Drawing.Point(50, 51);
            this.blahBlahLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.blahBlahLabel.Name = "blahBlahLabel";
            this.blahBlahLabel.Size = new System.Drawing.Size(227, 13);
            this.blahBlahLabel.TabIndex = 25;
            this.blahBlahLabel.Text = "(Repositories are searched in first-to-last order.)";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(72, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(182, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Drag and drop to reorder repositories.";
            // 
            // DesignateRepositoriesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 392);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.blahBlahLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.RemoveDirectory);
            this.Controls.Add(this.BrowseAndAddButton);
            this.Controls.Add(this.FilesListBox);
            this.Controls.Add(this.filesLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DesignateRepositoriesDialog";
            this.ShowInTaskbar = false;
            this.Text = "Designate Asset Repositories";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox FilesListBox;
        private System.Windows.Forms.Label filesLabel;
        private System.Windows.Forms.Button BrowseAndAddButton;
        private System.Windows.Forms.Button RemoveDirectory;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.FolderBrowserDialog RepositoryFolderBrowserDialog;
        private System.Windows.Forms.Label blahBlahLabel;
        private System.Windows.Forms.Label label1;
    }
}