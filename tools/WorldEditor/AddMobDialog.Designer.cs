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
	partial class AddMobDialog 
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
            this.spawnRadiusLabel = new System.Windows.Forms.Label();
            this.spawnRadiusTextbox = new System.Windows.Forms.TextBox();
            this.templateName = new System.Windows.Forms.Label();
            this.templateNameTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numSpawnsLabel = new System.Windows.Forms.Label();
            this.respawnTimeTextbox = new System.Windows.Forms.TextBox();
            this.numberOfSpawnsTextbox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // spawnRadiusLabel
            // 
            this.spawnRadiusLabel.AutoSize = true;
            this.spawnRadiusLabel.Location = new System.Drawing.Point(13, 93);
            this.spawnRadiusLabel.Name = "spawnRadiusLabel";
            this.spawnRadiusLabel.Size = new System.Drawing.Size(74, 13);
            this.spawnRadiusLabel.TabIndex = 0;
            this.spawnRadiusLabel.Text = "Spawn radius:";
            this.spawnRadiusLabel.Visible = false;
            // 
            // spawnRadiusTextbox
            // 
            this.spawnRadiusTextbox.Location = new System.Drawing.Point(114, 90);
            this.spawnRadiusTextbox.Name = "spawnRadiusTextbox";
            this.spawnRadiusTextbox.Size = new System.Drawing.Size(166, 20);
            this.spawnRadiusTextbox.TabIndex = 3;
            this.spawnRadiusTextbox.Visible = false;
            this.spawnRadiusTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // templateName
            // 
            this.templateName.AutoSize = true;
            this.templateName.Location = new System.Drawing.Point(13, 12);
            this.templateName.Name = "templateName";
            this.templateName.Size = new System.Drawing.Size(83, 13);
            this.templateName.TabIndex = 2;
            this.templateName.Text = "Template name:";
            // 
            // templateNameTextbox
            // 
            this.templateNameTextbox.Location = new System.Drawing.Point(114, 9);
            this.templateNameTextbox.Name = "templateNameTextbox";
            this.templateNameTextbox.Size = new System.Drawing.Size(166, 20);
            this.templateNameTextbox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Respawn time:";
            // 
            // numSpawnsLabel
            // 
            this.numSpawnsLabel.AutoSize = true;
            this.numSpawnsLabel.Location = new System.Drawing.Point(13, 65);
            this.numSpawnsLabel.Name = "numSpawnsLabel";
            this.numSpawnsLabel.Size = new System.Drawing.Size(98, 13);
            this.numSpawnsLabel.TabIndex = 5;
            this.numSpawnsLabel.Text = "Number of spawns:";
            // 
            // respawnTimeTextbox
            // 
            this.respawnTimeTextbox.Location = new System.Drawing.Point(114, 37);
            this.respawnTimeTextbox.Name = "respawnTimeTextbox";
            this.respawnTimeTextbox.Size = new System.Drawing.Size(166, 20);
            this.respawnTimeTextbox.TabIndex = 1;
            // 
            // numberOfSpawnsTextbox
            // 
            this.numberOfSpawnsTextbox.Location = new System.Drawing.Point(114, 62);
            this.numberOfSpawnsTextbox.Name = "numberOfSpawnsTextbox";
            this.numberOfSpawnsTextbox.Size = new System.Drawing.Size(166, 20);
            this.numberOfSpawnsTextbox.TabIndex = 2;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(16, 116);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "Add";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(205, 116);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(124, 116);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 5;
            this.helpButton.Tag = "Spawn_Generator";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // AddMobDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 149);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.numberOfSpawnsTextbox);
            this.Controls.Add(this.respawnTimeTextbox);
            this.Controls.Add(this.numSpawnsLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.templateNameTextbox);
            this.Controls.Add(this.templateName);
            this.Controls.Add(this.spawnRadiusTextbox);
            this.Controls.Add(this.spawnRadiusLabel);
            this.Name = "AddMobDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Spawn Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label spawnRadiusLabel;
		private System.Windows.Forms.TextBox spawnRadiusTextbox;
		private System.Windows.Forms.Label templateName;
		private System.Windows.Forms.TextBox templateNameTextbox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label numSpawnsLabel;
		private System.Windows.Forms.TextBox respawnTimeTextbox;
		private System.Windows.Forms.TextBox numberOfSpawnsTextbox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button helpButton;
	}
}
