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
            this.FilesListBox.ItemHeight = 16;
            this.FilesListBox.Location = new System.Drawing.Point(11, 93);
            this.FilesListBox.Margin = new System.Windows.Forms.Padding(4);
            this.FilesListBox.Name = "FilesListBox";
            this.FilesListBox.Size = new System.Drawing.Size(437, 244);
            this.FilesListBox.TabIndex = 20;
            this.FilesListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilesListBox_DragDrop);
            this.FilesListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseUp);
            this.FilesListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseMove);
            this.FilesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilesListBox_MouseDown);
            this.FilesListBox.SelectedValueChanged += new System.EventHandler(this.FilesListBox_SelectedValueChanged);
            this.FilesListBox.DragOver += new System.Windows.Forms.DragEventHandler(this.FilesListBox_DragOver);
            // 
            // filesLabel
            // 
            this.filesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.filesLabel.Location = new System.Drawing.Point(156, 9);
            this.filesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.filesLabel.Name = "filesLabel";
            this.filesLabel.Size = new System.Drawing.Size(126, 17);
            this.filesLabel.TabIndex = 19;
            this.filesLabel.Text = "Asset Repositories";
            // 
            // BrowseAndAddButton
            // 
            this.BrowseAndAddButton.Location = new System.Drawing.Point(11, 364);
            this.BrowseAndAddButton.Name = "BrowseAndAddButton";
            this.BrowseAndAddButton.Size = new System.Drawing.Size(224, 33);
            this.BrowseAndAddButton.TabIndex = 21;
            this.BrowseAndAddButton.Text = "Browse And Add Directory";
            this.BrowseAndAddButton.UseVisualStyleBackColor = true;
            this.BrowseAndAddButton.Click += new System.EventHandler(this.BrowseAndAddButton_Click);
            // 
            // RemoveDirectory
            // 
            this.RemoveDirectory.Location = new System.Drawing.Point(241, 364);
            this.RemoveDirectory.Name = "RemoveDirectory";
            this.RemoveDirectory.Size = new System.Drawing.Size(207, 33);
            this.RemoveDirectory.TabIndex = 22;
            this.RemoveDirectory.Text = "Remove Directory";
            this.RemoveDirectory.UseVisualStyleBackColor = true;
            this.RemoveDirectory.Click += new System.EventHandler(this.RemoveDirectory_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(367, 439);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(81, 33);
            this.CancelButton.TabIndex = 24;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(264, 439);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(97, 33);
            this.OKButton.TabIndex = 23;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // blahBlahLabel
            // 
            this.blahBlahLabel.AutoSize = true;
            this.blahBlahLabel.Location = new System.Drawing.Point(67, 63);
            this.blahBlahLabel.Name = "blahBlahLabel";
            this.blahBlahLabel.Size = new System.Drawing.Size(313, 17);
            this.blahBlahLabel.TabIndex = 25;
            this.blahBlahLabel.Text = "(Repositories are searched in first-to-last order.)";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(88, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(245, 17);
            this.label1.TabIndex = 26;
            this.label1.Text = "Drag and drop to reorder repositories";
            // 
            // DesignateRepositoriesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 483);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.blahBlahLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.RemoveDirectory);
            this.Controls.Add(this.BrowseAndAddButton);
            this.Controls.Add(this.FilesListBox);
            this.Controls.Add(this.filesLabel);
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