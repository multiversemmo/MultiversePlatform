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
    partial class SetCameraNearDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nearDistanceValueLabel = new System.Windows.Forms.Label();
            this.whatsNearDistanceLabel = new System.Windows.Forms.LinkLabel();
            this.cameraNearDistanceTrackBar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraNearDistanceTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nearDistanceValueLabel);
            this.groupBox1.Controls.Add(this.whatsNearDistanceLabel);
            this.groupBox1.Controls.Add(this.cameraNearDistanceTrackBar);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 125);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // nearDistanceValueLabel
            // 
            this.nearDistanceValueLabel.AutoSize = true;
            this.nearDistanceValueLabel.Location = new System.Drawing.Point(44, 37);
            this.nearDistanceValueLabel.Name = "nearDistanceValueLabel";
            this.nearDistanceValueLabel.Size = new System.Drawing.Size(0, 13);
            this.nearDistanceValueLabel.TabIndex = 3;
            // 
            // whatsNearDistanceLabel
            // 
            this.whatsNearDistanceLabel.AutoSize = true;
            this.whatsNearDistanceLabel.Location = new System.Drawing.Point(40, 65);
            this.whatsNearDistanceLabel.Name = "whatsNearDistanceLabel";
            this.whatsNearDistanceLabel.Size = new System.Drawing.Size(69, 13);
            this.whatsNearDistanceLabel.TabIndex = 2;
            this.whatsNearDistanceLabel.TabStop = true;
            this.whatsNearDistanceLabel.Text = "What\'s This?";
            // 
            // cameraNearDistanceTrackBar
            // 
            this.cameraNearDistanceTrackBar.LargeChange = 10;
            this.cameraNearDistanceTrackBar.Location = new System.Drawing.Point(167, 19);
            this.cameraNearDistanceTrackBar.Maximum = 100;
            this.cameraNearDistanceTrackBar.Name = "cameraNearDistanceTrackBar";
            this.cameraNearDistanceTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.cameraNearDistanceTrackBar.Size = new System.Drawing.Size(45, 96);
            this.cameraNearDistanceTrackBar.TabIndex = 1;
            this.cameraNearDistanceTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.cameraNearDistanceTrackBar.Value = 50;
            this.cameraNearDistanceTrackBar.Scroll += new System.EventHandler(this.cameraNearDistanceTrackBar_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Camera Near Distance:";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(165, 150);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // SetCameraNearDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 185);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.groupBox1);
            this.Name = "SetCameraNearDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Set Camera Near Distance";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetCameraNearDialog_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraNearDistanceTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label nearDistanceValueLabel;
        private System.Windows.Forms.LinkLabel whatsNearDistanceLabel;
        private System.Windows.Forms.TrackBar cameraNearDistanceTrackBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button okButton;
    }
}
