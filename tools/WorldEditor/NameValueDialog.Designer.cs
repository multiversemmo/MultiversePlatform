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
	public partial class NameValueDialog
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
			this.nameListBox = new System.Windows.Forms.ListBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.templatesListBox = new System.Windows.Forms.ListBox();
			this.nameValuePairsLable = new System.Windows.Forms.Label();
			this.templatesLabel = new System.Windows.Forms.Label();
			this.addNVPButton = new System.Windows.Forms.Button();
			this.editNVPButton = new System.Windows.Forms.Button();
			this.deleteNVPButton = new System.Windows.Forms.Button();
			this.addTemplateButton = new System.Windows.Forms.Button();
			this.helpButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// nameListBox
			// 
			this.nameListBox.FormattingEnabled = true;
			this.nameListBox.Location = new System.Drawing.Point(1, 27);
			this.nameListBox.Name = "nameListBox";
			this.nameListBox.Size = new System.Drawing.Size(324, 251);
			this.nameListBox.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(601, 323);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(1, 323);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "&Ok";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// templatesListBox
			// 
			this.templatesListBox.FormattingEnabled = true;
			this.templatesListBox.Location = new System.Drawing.Point(331, 27);
			this.templatesListBox.Name = "templatesListBox";
			this.templatesListBox.Size = new System.Drawing.Size(345, 251);
			this.templatesListBox.TabIndex = 5;
			// 
			// nameValuePairsLable
			// 
			this.nameValuePairsLable.AutoSize = true;
			this.nameValuePairsLable.Location = new System.Drawing.Point(-2, 11);
			this.nameValuePairsLable.Name = "nameValuePairsLable";
			this.nameValuePairsLable.Size = new System.Drawing.Size(96, 13);
			this.nameValuePairsLable.TabIndex = 12;
			this.nameValuePairsLable.Text = "Name/Value Pairs:";
			// 
			// templatesLabel
			// 
			this.templatesLabel.AutoSize = true;
			this.templatesLabel.Location = new System.Drawing.Point(331, 10);
			this.templatesLabel.Name = "templatesLabel";
			this.templatesLabel.Size = new System.Drawing.Size(56, 13);
			this.templatesLabel.TabIndex = 13;
			this.templatesLabel.Text = "Templates";
			// 
			// addNVPButton
			// 
			this.addNVPButton.Location = new System.Drawing.Point(16, 284);
			this.addNVPButton.Name = "addNVPButton";
			this.addNVPButton.Size = new System.Drawing.Size(75, 23);
			this.addNVPButton.TabIndex = 1;
			this.addNVPButton.Text = "Add";
			this.addNVPButton.UseVisualStyleBackColor = true;
			this.addNVPButton.Click += new System.EventHandler(this.addNVPButton_click);
			// 
			// editNVPButton
			// 
			this.editNVPButton.Location = new System.Drawing.Point(126, 284);
			this.editNVPButton.Name = "editNVPButton";
			this.editNVPButton.Size = new System.Drawing.Size(75, 23);
			this.editNVPButton.TabIndex = 2;
			this.editNVPButton.Text = "Edit";
			this.editNVPButton.UseVisualStyleBackColor = true;
			this.editNVPButton.Click += new System.EventHandler(this.editNVPButton_click);
			// 
			// deleteNVPButton
			// 
			this.deleteNVPButton.Location = new System.Drawing.Point(236, 284);
			this.deleteNVPButton.Name = "deleteNVPButton";
			this.deleteNVPButton.Size = new System.Drawing.Size(75, 23);
			this.deleteNVPButton.TabIndex = 3;
			this.deleteNVPButton.Text = "Delete";
			this.deleteNVPButton.UseVisualStyleBackColor = true;
			this.deleteNVPButton.Click += new System.EventHandler(this.deleteNVPButton_click);
			// 
			// addTemplateButton
			// 
			this.addTemplateButton.Location = new System.Drawing.Point(461, 284);
			this.addTemplateButton.Name = "addTemplateButton";
			this.addTemplateButton.Size = new System.Drawing.Size(84, 23);
			this.addTemplateButton.TabIndex = 6;
			this.addTemplateButton.Text = "Add Template";
			this.addTemplateButton.UseVisualStyleBackColor = true;
			this.addTemplateButton.Click += new System.EventHandler(this.addTemplateButton_clicked);
			// 
			// helpButton
			// 
			this.helpButton.Location = new System.Drawing.Point(520, 323);
			this.helpButton.Name = "helpButton";
			this.helpButton.Size = new System.Drawing.Size(75, 23);
			this.helpButton.TabIndex = 14;
			this.helpButton.Tag = "Name_Value_Pairs";
			this.helpButton.Text = "Help";
			this.helpButton.UseVisualStyleBackColor = true;
			this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
			// 
			// NameValueDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(678, 349);
			this.Controls.Add(this.helpButton);
			this.Controls.Add(this.addTemplateButton);
			this.Controls.Add(this.deleteNVPButton);
			this.Controls.Add(this.editNVPButton);
			this.Controls.Add(this.addNVPButton);
			this.Controls.Add(this.templatesLabel);
			this.Controls.Add(this.nameValuePairsLable);
			this.Controls.Add(this.templatesListBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.nameListBox);
			this.Name = "NameValueDialog";
			this.Text = "Name Value Dialog";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox nameListBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ListBox templatesListBox;
		private System.Windows.Forms.Label nameValuePairsLable;
		private System.Windows.Forms.Label templatesLabel;
		private System.Windows.Forms.Button addNVPButton;
		private System.Windows.Forms.Button editNVPButton;
		private System.Windows.Forms.Button deleteNVPButton;
		private System.Windows.Forms.Button addTemplateButton;
		private System.Windows.Forms.Button helpButton;
	}
}
