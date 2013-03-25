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
	partial class SubMeshDialog
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
            this.subMeshListBox = new System.Windows.Forms.CheckedListBox();
            this.materialLabel = new System.Windows.Forms.Label();
            this.materialTextBox = new System.Windows.Forms.TextBox();
            this.hideButton = new System.Windows.Forms.Button();
            this.showButton = new System.Windows.Forms.Button();
            this.subMeshTextBox = new System.Windows.Forms.TextBox();
            this.hideShowLabel = new System.Windows.Forms.Label();
            this.hideAllButton = new System.Windows.Forms.Button();
            this.showAllButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // subMeshListBox
            // 
            this.subMeshListBox.FormattingEnabled = true;
            this.subMeshListBox.Location = new System.Drawing.Point(2, 3);
            this.subMeshListBox.Name = "subMeshListBox";
            this.subMeshListBox.Size = new System.Drawing.Size(277, 274);
            this.subMeshListBox.TabIndex = 8;
            this.subMeshListBox.SelectedIndexChanged += new System.EventHandler(this.subMeshListBox_SelectedIndexChanged);
            this.subMeshListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.subMeshListBox_ItemCheck);
            // 
            // materialLabel
            // 
            this.materialLabel.AutoSize = true;
            this.materialLabel.Location = new System.Drawing.Point(-1, 413);
            this.materialLabel.Name = "materialLabel";
            this.materialLabel.Size = new System.Drawing.Size(47, 13);
            this.materialLabel.TabIndex = 16;
            this.materialLabel.Text = "Material:";
            // 
            // materialTextBox
            // 
            this.materialTextBox.Location = new System.Drawing.Point(52, 410);
            this.materialTextBox.Name = "materialTextBox";
            this.materialTextBox.Size = new System.Drawing.Size(227, 20);
            this.materialTextBox.TabIndex = 15;
            this.materialTextBox.TextChanged += new System.EventHandler(this.materialTextBox_TextChanged);
            // 
            // hideButton
            // 
            this.hideButton.Enabled = false;
            this.hideButton.Location = new System.Drawing.Point(199, 368);
            this.hideButton.Name = "hideButton";
            this.hideButton.Size = new System.Drawing.Size(80, 30);
            this.hideButton.TabIndex = 14;
            this.hideButton.Text = "Hide";
            this.hideButton.UseVisualStyleBackColor = true;
            this.hideButton.Click += new System.EventHandler(this.hideButton_Click);
            // 
            // showButton
            // 
            this.showButton.Enabled = false;
            this.showButton.Location = new System.Drawing.Point(2, 368);
            this.showButton.Name = "showButton";
            this.showButton.Size = new System.Drawing.Size(80, 30);
            this.showButton.TabIndex = 13;
            this.showButton.Text = "Show";
            this.showButton.UseVisualStyleBackColor = true;
            this.showButton.Click += new System.EventHandler(this.showButton_Click);
            // 
            // subMeshTextBox
            // 
            this.subMeshTextBox.Location = new System.Drawing.Point(2, 342);
            this.subMeshTextBox.Name = "subMeshTextBox";
            this.subMeshTextBox.Size = new System.Drawing.Size(277, 20);
            this.subMeshTextBox.TabIndex = 12;
            this.subMeshTextBox.TextChanged += new System.EventHandler(this.subMeshTextBox_TextChanged);
            // 
            // hideShowLabel
            // 
            this.hideShowLabel.AutoSize = true;
            this.hideShowLabel.Location = new System.Drawing.Point(-1, 326);
            this.hideShowLabel.Name = "hideShowLabel";
            this.hideShowLabel.Size = new System.Drawing.Size(179, 13);
            this.hideShowLabel.TabIndex = 11;
            this.hideShowLabel.Text = "Hide/Show Sub Meshes Containing:";
            // 
            // hideAllButton
            // 
            this.hideAllButton.Location = new System.Drawing.Point(199, 283);
            this.hideAllButton.Name = "hideAllButton";
            this.hideAllButton.Size = new System.Drawing.Size(80, 30);
            this.hideAllButton.TabIndex = 10;
            this.hideAllButton.Text = "Hide All";
            this.hideAllButton.UseVisualStyleBackColor = true;
            this.hideAllButton.Click += new System.EventHandler(this.hideAllButton_Click);
            // 
            // showAllButton
            // 
            this.showAllButton.Location = new System.Drawing.Point(2, 283);
            this.showAllButton.Name = "showAllButton";
            this.showAllButton.Size = new System.Drawing.Size(80, 30);
            this.showAllButton.TabIndex = 9;
            this.showAllButton.Text = "Show All";
            this.showAllButton.UseVisualStyleBackColor = true;
            this.showAllButton.Click += new System.EventHandler(this.showAllButton_Click);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(2, 448);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(80, 30);
            this.okButton.TabIndex = 17;
            this.okButton.Text = "&Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(199, 448);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(80, 30);
            this.helpButton.TabIndex = 18;
            this.helpButton.Tag = "Submeshes";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // SubMeshDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 486);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.materialLabel);
            this.Controls.Add(this.materialTextBox);
            this.Controls.Add(this.hideButton);
            this.Controls.Add(this.showButton);
            this.Controls.Add(this.subMeshTextBox);
            this.Controls.Add(this.hideShowLabel);
            this.Controls.Add(this.hideAllButton);
            this.Controls.Add(this.showAllButton);
            this.Controls.Add(this.subMeshListBox);
            this.Name = "SubMeshDialog";
            this.ShowInTaskbar = false;
            this.Text = "SubMesh Editing Dialog";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckedListBox subMeshListBox;
		private System.Windows.Forms.Label materialLabel;
		private System.Windows.Forms.TextBox materialTextBox;
		private System.Windows.Forms.Button hideButton;
		private System.Windows.Forms.Button showButton;
		private System.Windows.Forms.TextBox subMeshTextBox;
		private System.Windows.Forms.Label hideShowLabel;
		private System.Windows.Forms.Button hideAllButton;
		private System.Windows.Forms.Button showAllButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button helpButton;
	}
}
