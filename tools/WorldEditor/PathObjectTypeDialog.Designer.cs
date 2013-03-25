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

namespace Multiverse.Tools.WorldEditor {
    partial class PathObjectTypeDialog {
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
            this.label1 = new System.Windows.Forms.Label();
            this.pathObjectTypeName = new System.Windows.Forms.TextBox();
            this.pathObjectTypeHeight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pathObjectTypeWidth = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pathObjectTypeSlope = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.pathObjectTypeMaxDisjointDistance = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.pathObjectTypeMinimumFeatureSize = new System.Windows.Forms.TextBox();
            this.pathObjectTypeGridResolution = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(55, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name of path object type";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pathObjectTypeName
            // 
            this.pathObjectTypeName.Location = new System.Drawing.Point(176, 65);
            this.pathObjectTypeName.Name = "pathObjectTypeName";
            this.pathObjectTypeName.Size = new System.Drawing.Size(187, 20);
            this.pathObjectTypeName.TabIndex = 1;
            this.pathObjectTypeName.TextChanged += new System.EventHandler(this.pathObjectTypeName_TextChanged);
            // 
            // pathObjectTypeHeight
            // 
            this.pathObjectTypeHeight.Location = new System.Drawing.Point(176, 100);
            this.pathObjectTypeHeight.Name = "pathObjectTypeHeight";
            this.pathObjectTypeHeight.Size = new System.Drawing.Size(86, 20);
            this.pathObjectTypeHeight.TabIndex = 3;
            this.pathObjectTypeHeight.Text = "1.8";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Minimum path height (meters)";
            // 
            // pathObjectTypeWidth
            // 
            this.pathObjectTypeWidth.Location = new System.Drawing.Point(176, 135);
            this.pathObjectTypeWidth.Name = "pathObjectTypeWidth";
            this.pathObjectTypeWidth.Size = new System.Drawing.Size(86, 20);
            this.pathObjectTypeWidth.TabIndex = 5;
            this.pathObjectTypeWidth.Text = "0.5";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Minimum path width (meters)";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(52, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(319, 43);
            this.label4.TabIndex = 6;
            this.label4.Text = "Define the characteristics of a \"path object type\", used to generate path informa" +
                "tion for mobs of this size, used by the server-side path and collision detection" +
                " system";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Location = new System.Drawing.Point(7, 204);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 4);
            this.panel1.TabIndex = 7;
            // 
            // pathObjectTypeSlope
            // 
            this.pathObjectTypeSlope.Location = new System.Drawing.Point(176, 167);
            this.pathObjectTypeSlope.Name = "pathObjectTypeSlope";
            this.pathObjectTypeSlope.Size = new System.Drawing.Size(86, 20);
            this.pathObjectTypeSlope.TabIndex = 9;
            this.pathObjectTypeSlope.Text = "0.5";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(38, 161);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 32);
            this.label5.TabIndex = 8;
            this.label5.Text = "Maximum slope a mob can climb (fraction)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(52, 221);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(319, 31);
            this.label6.TabIndex = 10;
            this.label6.Text = "Parameters that are exposed but you should not need to change except in unusual c" +
                "ircumstances";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(38, 267);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(132, 41);
            this.label7.TabIndex = 17;
            this.label7.Text = "Fraction of path width to use as the path grid resolution (fraction)";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pathObjectTypeMaxDisjointDistance
            // 
            this.pathObjectTypeMaxDisjointDistance.Location = new System.Drawing.Point(176, 381);
            this.pathObjectTypeMaxDisjointDistance.Name = "pathObjectTypeMaxDisjointDistance";
            this.pathObjectTypeMaxDisjointDistance.Size = new System.Drawing.Size(86, 20);
            this.pathObjectTypeMaxDisjointDistance.TabIndex = 16;
            this.pathObjectTypeMaxDisjointDistance.Text = "0.1";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(34, 370);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(136, 41);
            this.label8.TabIndex = 15;
            this.label8.Text = "Gaps allowed between collision volumes (meters)";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pathObjectTypeMinimumFeatureSize
            // 
            this.pathObjectTypeMinimumFeatureSize.Location = new System.Drawing.Point(176, 332);
            this.pathObjectTypeMinimumFeatureSize.Name = "pathObjectTypeMinimumFeatureSize";
            this.pathObjectTypeMinimumFeatureSize.Size = new System.Drawing.Size(86, 20);
            this.pathObjectTypeMinimumFeatureSize.TabIndex = 14;
            this.pathObjectTypeMinimumFeatureSize.Text = "3";
            // 
            // pathObjectTypeGridResolution
            // 
            this.pathObjectTypeGridResolution.Location = new System.Drawing.Point(176, 279);
            this.pathObjectTypeGridResolution.Name = "pathObjectTypeGridResolution";
            this.pathObjectTypeGridResolution.Size = new System.Drawing.Size(86, 20);
            this.pathObjectTypeGridResolution.TabIndex = 12;
            this.pathObjectTypeGridResolution.Text = "0.25";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(38, 321);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(132, 41);
            this.label10.TabIndex = 19;
            this.label10.Text = "If a feature of the grid is smaller than this number of grid cells, ignore it";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(246, 423);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 20;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(327, 423);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 21;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // PathObjectTypeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 458);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.pathObjectTypeMaxDisjointDistance);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pathObjectTypeMinimumFeatureSize);
            this.Controls.Add(this.pathObjectTypeGridResolution);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.pathObjectTypeSlope);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pathObjectTypeWidth);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pathObjectTypeHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pathObjectTypeName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "PathObjectTypeDialog";
            this.ShowInTaskbar = false;
            this.Text = "Editing Path Object Type";
            this.Shown += new System.EventHandler(this.PathObjectTypeDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox pathObjectTypeName;
        private System.Windows.Forms.TextBox pathObjectTypeHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox pathObjectTypeWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox pathObjectTypeSlope;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox pathObjectTypeMaxDisjointDistance;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox pathObjectTypeMinimumFeatureSize;
        private System.Windows.Forms.TextBox pathObjectTypeGridResolution;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
