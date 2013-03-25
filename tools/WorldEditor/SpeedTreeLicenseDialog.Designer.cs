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
    partial class SpeedTreeLicenseDialog
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
            this.speedTreeLicenseLinkLabel = new System.Windows.Forms.LinkLabel();
            this.dontShowAgainCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.firstLineText = new System.Windows.Forms.Label();
            this.secondLineText = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // speedTreeLicenseLinkLabel
            // 
            this.speedTreeLicenseLinkLabel.AutoSize = true;
            this.speedTreeLicenseLinkLabel.Location = new System.Drawing.Point(31, 43);
            this.speedTreeLicenseLinkLabel.Name = "speedTreeLicenseLinkLabel";
            this.speedTreeLicenseLinkLabel.Size = new System.Drawing.Size(108, 13);
            this.speedTreeLicenseLinkLabel.TabIndex = 1;
            this.speedTreeLicenseLinkLabel.TabStop = true;
            this.speedTreeLicenseLinkLabel.Text = "SpeedTree Licensing";
            this.speedTreeLicenseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.speedTreeLicenseLinkLabel_Clicked);
            // 
            // dontShowAgainCheckBox
            // 
            this.dontShowAgainCheckBox.AutoSize = true;
            this.dontShowAgainCheckBox.Location = new System.Drawing.Point(11, 94);
            this.dontShowAgainCheckBox.Name = "dontShowAgainCheckBox";
            this.dontShowAgainCheckBox.Size = new System.Drawing.Size(177, 17);
            this.dontShowAgainCheckBox.TabIndex = 2;
            this.dontShowAgainCheckBox.Text = "Do not show this warning again.";
            this.dontShowAgainCheckBox.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(12, 117);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "&Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(371, 117);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // firstLineText
            // 
            this.firstLineText.Location = new System.Drawing.Point(12, 13);
            this.firstLineText.Name = "firstLineText";
            this.firstLineText.Size = new System.Drawing.Size(434, 15);
            this.firstLineText.TabIndex = 5;
            this.firstLineText.Text = "The built in tree feature of the Multiverse Platform uses licensed SpeedTree(tm) " +
                "";
            // 
            // secondLineText
            // 
            this.secondLineText.AutoSize = true;
            this.secondLineText.Location = new System.Drawing.Point(12, 28);
            this.secondLineText.Name = "secondLineText";
            this.secondLineText.Size = new System.Drawing.Size(412, 13);
            this.secondLineText.TabIndex = 6;
            this.secondLineText.Text = "technology.  Your use of SpeedTree(tm) may require an additional license.  Please" +
                " see";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 22);
            this.label1.TabIndex = 7;
            this.label1.Text = "the ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(136, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "page for more details.";
            // 
            // SpeedTreeLicenseDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(461, 152);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.firstLineText);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.dontShowAgainCheckBox);
            this.Controls.Add(this.speedTreeLicenseLinkLabel);
            this.Controls.Add(this.secondLineText);
            this.Name = "SpeedTreeLicenseDialog";
            this.ShowInTaskbar = false;
            this.Text = "Speed Tree License Dialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel speedTreeLicenseLinkLabel;
        private System.Windows.Forms.CheckBox dontShowAgainCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label firstLineText;
        private System.Windows.Forms.Label secondLineText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

    }
}
