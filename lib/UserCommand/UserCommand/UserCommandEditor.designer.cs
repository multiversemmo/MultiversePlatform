namespace Multiverse.ToolBox
{
    partial class UserCommandEditor
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
            this.contextComboBox = new System.Windows.Forms.ComboBox();
            this.contextComboBoxLabel = new System.Windows.Forms.Label();
            this.keyComboBox = new System.Windows.Forms.ComboBox();
            this.keysComboBoxLabel = new System.Windows.Forms.Label();
            this.modifierComboBox = new System.Windows.Forms.ComboBox();
            this.modifierComboBoxLabel = new System.Windows.Forms.Label();
            this.activityComboBox = new System.Windows.Forms.ComboBox();
            this.activityComboBoxLabel = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.deleteCommandButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.eventsTreeView = new System.Windows.Forms.TreeView();
            this.treeViewLabel = new System.Windows.Forms.Label();
            this.eventsTreeViewLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // contextComboBox
            // 
            this.contextComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.contextComboBox.FormattingEnabled = true;
            this.contextComboBox.Location = new System.Drawing.Point(385, 39);
            this.contextComboBox.Name = "contextComboBox";
            this.contextComboBox.Size = new System.Drawing.Size(406, 21);
            this.contextComboBox.Sorted = true;
            this.contextComboBox.TabIndex = 1;
            this.contextComboBox.SelectedIndexChanged += new System.EventHandler(this.contextComboBox_selectedIndexChanged);
            // 
            // contextComboBoxLabel
            // 
            this.contextComboBoxLabel.AutoSize = true;
            this.contextComboBoxLabel.Location = new System.Drawing.Point(337, 42);
            this.contextComboBoxLabel.Name = "contextComboBoxLabel";
            this.contextComboBoxLabel.Size = new System.Drawing.Size(46, 13);
            this.contextComboBoxLabel.TabIndex = 3;
            this.contextComboBoxLabel.Text = "Context:";
            // 
            // keyComboBox
            // 
            this.keyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.keyComboBox.FormattingEnabled = true;
            this.keyComboBox.Location = new System.Drawing.Point(385, 122);
            this.keyComboBox.Name = "keyComboBox";
            this.keyComboBox.Size = new System.Drawing.Size(406, 21);
            this.keyComboBox.Sorted = true;
            this.keyComboBox.TabIndex = 3;
            this.keyComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_selectedIndexChanged);
            // 
            // keysComboBoxLabel
            // 
            this.keysComboBoxLabel.AutoSize = true;
            this.keysComboBoxLabel.Location = new System.Drawing.Point(336, 125);
            this.keysComboBoxLabel.Name = "keysComboBoxLabel";
            this.keysComboBoxLabel.Size = new System.Drawing.Size(28, 13);
            this.keysComboBoxLabel.TabIndex = 7;
            this.keysComboBoxLabel.Text = "Key:";
            // 
            // modifierComboBox
            // 
            this.modifierComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modifierComboBox.FormattingEnabled = true;
            this.modifierComboBox.Location = new System.Drawing.Point(385, 93);
            this.modifierComboBox.Name = "modifierComboBox";
            this.modifierComboBox.Size = new System.Drawing.Size(406, 21);
            this.modifierComboBox.Sorted = true;
            this.modifierComboBox.TabIndex = 2;
            this.modifierComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_selectedIndexChanged);
            // 
            // modifierComboBoxLabel
            // 
            this.modifierComboBoxLabel.AutoSize = true;
            this.modifierComboBoxLabel.Location = new System.Drawing.Point(336, 96);
            this.modifierComboBoxLabel.Name = "modifierComboBoxLabel";
            this.modifierComboBoxLabel.Size = new System.Drawing.Size(47, 13);
            this.modifierComboBoxLabel.TabIndex = 9;
            this.modifierComboBoxLabel.Text = "Modifier:";
            // 
            // activityComboBox
            // 
            this.activityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.activityComboBox.FormattingEnabled = true;
            this.activityComboBox.Location = new System.Drawing.Point(385, 149);
            this.activityComboBox.Name = "activityComboBox";
            this.activityComboBox.Size = new System.Drawing.Size(406, 21);
            this.activityComboBox.Sorted = true;
            this.activityComboBox.TabIndex = 4;
            this.activityComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_selectedIndexChanged);
            // 
            // activityComboBoxLabel
            // 
            this.activityComboBoxLabel.AutoSize = true;
            this.activityComboBoxLabel.Location = new System.Drawing.Point(336, 152);
            this.activityComboBoxLabel.Name = "activityComboBoxLabel";
            this.activityComboBoxLabel.Size = new System.Drawing.Size(44, 13);
            this.activityComboBoxLabel.TabIndex = 11;
            this.activityComboBoxLabel.Text = "Activity:";
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(339, 191);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 5;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_clicked);
            // 
            // deleteCommandButton
            // 
            this.deleteCommandButton.Location = new System.Drawing.Point(716, 191);
            this.deleteCommandButton.Name = "deleteCommandButton";
            this.deleteCommandButton.Size = new System.Drawing.Size(75, 23);
            this.deleteCommandButton.TabIndex = 7;
            this.deleteCommandButton.Text = "Delete";
            this.deleteCommandButton.UseVisualStyleBackColor = true;
            this.deleteCommandButton.Click += new System.EventHandler(this.deleteButton_clicked);
            // 
            // editButton
            // 
            this.editButton.Location = new System.Drawing.Point(420, 191);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(75, 23);
            this.editButton.TabIndex = 6;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.editButton_clicked);
            // 
            // eventsTreeView
            // 
            this.eventsTreeView.FullRowSelect = true;
            this.eventsTreeView.HideSelection = false;
            this.eventsTreeView.Location = new System.Drawing.Point(16, 39);
            this.eventsTreeView.Name = "eventsTreeView";
            this.eventsTreeView.Size = new System.Drawing.Size(296, 435);
            this.eventsTreeView.TabIndex = 0;
            this.eventsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.eventsTreeView_AfterSelect);
            // 
            // treeViewLabel
            // 
            this.treeViewLabel.AutoSize = true;
            this.treeViewLabel.Location = new System.Drawing.Point(16, 13);
            this.treeViewLabel.Name = "treeViewLabel";
            this.treeViewLabel.Size = new System.Drawing.Size(0, 13);
            this.treeViewLabel.TabIndex = 16;
            // 
            // eventsTreeViewLabel
            // 
            this.eventsTreeViewLabel.AutoSize = true;
            this.eventsTreeViewLabel.Location = new System.Drawing.Point(16, 13);
            this.eventsTreeViewLabel.Name = "eventsTreeViewLabel";
            this.eventsTreeViewLabel.Size = new System.Drawing.Size(43, 13);
            this.eventsTreeViewLabel.TabIndex = 17;
            this.eventsTreeViewLabel.Text = "Events:";
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(12, 489);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "&Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(736, 489);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // UserCommandEditor
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(823, 524);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.eventsTreeViewLabel);
            this.Controls.Add(this.treeViewLabel);
            this.Controls.Add(this.eventsTreeView);
            this.Controls.Add(this.editButton);
            this.Controls.Add(this.deleteCommandButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.activityComboBoxLabel);
            this.Controls.Add(this.activityComboBox);
            this.Controls.Add(this.modifierComboBoxLabel);
            this.Controls.Add(this.modifierComboBox);
            this.Controls.Add(this.keysComboBoxLabel);
            this.Controls.Add(this.keyComboBox);
            this.Controls.Add(this.contextComboBoxLabel);
            this.Controls.Add(this.contextComboBox);
            this.Name = "UserCommandEditor";
            this.Text = "UserCommandEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox contextComboBox;
        private System.Windows.Forms.Label contextComboBoxLabel;
        private System.Windows.Forms.ComboBox keyComboBox;
        private System.Windows.Forms.Label keysComboBoxLabel;
        private System.Windows.Forms.ComboBox modifierComboBox;
        private System.Windows.Forms.Label modifierComboBoxLabel;
        private System.Windows.Forms.ComboBox activityComboBox;
        private System.Windows.Forms.Label activityComboBoxLabel;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button deleteCommandButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.TreeView eventsTreeView;
        private System.Windows.Forms.Label treeViewLabel;
        private System.Windows.Forms.Label eventsTreeViewLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}