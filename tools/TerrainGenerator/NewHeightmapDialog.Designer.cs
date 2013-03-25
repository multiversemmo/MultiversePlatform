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

namespace Multiverse.Tools.TerrainGenerator
{
    partial class NewHeightmapDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.heightmapDefaultHeightUpDown = new System.Windows.Forms.NumericUpDown();
            this.heightmapWidthUpDown = new System.Windows.Forms.NumericUpDown();
            this.heightmapHeightUpDown = new System.Windows.Forms.NumericUpDown();
            this.heightmapCancelButton = new System.Windows.Forms.Button();
            this.heightmapCreateButton = new System.Windows.Forms.Button();
            this.centerHeightmapCheckbox = new System.Windows.Forms.CheckBox();
            this.heightmapMetersPerSampleLabel = new System.Windows.Forms.Label();
            this.heightmapMetersPerSampleUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapDefaultHeightUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapWidthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapHeightUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapMetersPerSampleUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Heightmap X Size (East/West)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Heightmap Z Size (North/South)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Default Height Value";
            // 
            // heightmapDefaultHeightUpDown
            // 
            this.heightmapDefaultHeightUpDown.DecimalPlaces = 2;
            this.heightmapDefaultHeightUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.heightmapDefaultHeightUpDown.Location = new System.Drawing.Point(218, 97);
            this.heightmapDefaultHeightUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.heightmapDefaultHeightUpDown.Name = "heightmapDefaultHeightUpDown";
            this.heightmapDefaultHeightUpDown.Size = new System.Drawing.Size(120, 20);
            this.heightmapDefaultHeightUpDown.TabIndex = 5;
            // 
            // heightmapWidthUpDown
            // 
            this.heightmapWidthUpDown.Location = new System.Drawing.Point(218, 27);
            this.heightmapWidthUpDown.Name = "heightmapWidthUpDown";
            this.heightmapWidthUpDown.Size = new System.Drawing.Size(120, 20);
            this.heightmapWidthUpDown.TabIndex = 6;
            this.heightmapWidthUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // heightmapHeightUpDown
            // 
            this.heightmapHeightUpDown.Location = new System.Drawing.Point(218, 63);
            this.heightmapHeightUpDown.Name = "heightmapHeightUpDown";
            this.heightmapHeightUpDown.Size = new System.Drawing.Size(120, 20);
            this.heightmapHeightUpDown.TabIndex = 7;
            this.heightmapHeightUpDown.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // heightmapCancelButton
            // 
            this.heightmapCancelButton.Location = new System.Drawing.Point(263, 205);
            this.heightmapCancelButton.Name = "heightmapCancelButton";
            this.heightmapCancelButton.Size = new System.Drawing.Size(75, 23);
            this.heightmapCancelButton.TabIndex = 8;
            this.heightmapCancelButton.Text = "Cancel";
            this.heightmapCancelButton.UseVisualStyleBackColor = true;
            this.heightmapCancelButton.Click += new System.EventHandler(this.heightmapCancelButton_Click);
            // 
            // heightmapCreateButton
            // 
            this.heightmapCreateButton.Location = new System.Drawing.Point(158, 205);
            this.heightmapCreateButton.Name = "heightmapCreateButton";
            this.heightmapCreateButton.Size = new System.Drawing.Size(75, 23);
            this.heightmapCreateButton.TabIndex = 9;
            this.heightmapCreateButton.Text = "Create";
            this.heightmapCreateButton.UseVisualStyleBackColor = true;
            this.heightmapCreateButton.Click += new System.EventHandler(this.heightmapCreateButton_Click);
            // 
            // centerHeightmapCheckbox
            // 
            this.centerHeightmapCheckbox.AutoSize = true;
            this.centerHeightmapCheckbox.Checked = true;
            this.centerHeightmapCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.centerHeightmapCheckbox.Location = new System.Drawing.Point(32, 169);
            this.centerHeightmapCheckbox.Name = "centerHeightmapCheckbox";
            this.centerHeightmapCheckbox.Size = new System.Drawing.Size(156, 17);
            this.centerHeightmapCheckbox.TabIndex = 11;
            this.centerHeightmapCheckbox.Text = "Center Heightmap on Origin";
            this.centerHeightmapCheckbox.UseVisualStyleBackColor = true;
            // 
            // heightmapMetersPerSampleLabel
            // 
            this.heightmapMetersPerSampleLabel.AutoSize = true;
            this.heightmapMetersPerSampleLabel.Location = new System.Drawing.Point(29, 134);
            this.heightmapMetersPerSampleLabel.Name = "heightmapMetersPerSampleLabel";
            this.heightmapMetersPerSampleLabel.Size = new System.Drawing.Size(150, 13);
            this.heightmapMetersPerSampleLabel.TabIndex = 12;
            this.heightmapMetersPerSampleLabel.Text = "Heightmap Meters Per Sample";
            // 
            // heightmapMetersPerSampleUpDown
            // 
            this.heightmapMetersPerSampleUpDown.Location = new System.Drawing.Point(218, 132);
            this.heightmapMetersPerSampleUpDown.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.heightmapMetersPerSampleUpDown.Name = "heightmapMetersPerSampleUpDown";
            this.heightmapMetersPerSampleUpDown.Size = new System.Drawing.Size(120, 20);
            this.heightmapMetersPerSampleUpDown.TabIndex = 13;
            this.heightmapMetersPerSampleUpDown.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // NewHeightmapDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 254);
            this.Controls.Add(this.heightmapMetersPerSampleUpDown);
            this.Controls.Add(this.heightmapMetersPerSampleLabel);
            this.Controls.Add(this.centerHeightmapCheckbox);
            this.Controls.Add(this.heightmapCreateButton);
            this.Controls.Add(this.heightmapCancelButton);
            this.Controls.Add(this.heightmapHeightUpDown);
            this.Controls.Add(this.heightmapWidthUpDown);
            this.Controls.Add(this.heightmapDefaultHeightUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "NewHeightmapDialog";
            this.ShowInTaskbar = false;
            this.Text = "Create a new Seed Heightmap";
            ((System.ComponentModel.ISupportInitialize)(this.heightmapDefaultHeightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapHeightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightmapMetersPerSampleUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown heightmapDefaultHeightUpDown;
        private System.Windows.Forms.NumericUpDown heightmapWidthUpDown;
        private System.Windows.Forms.NumericUpDown heightmapHeightUpDown;
        private System.Windows.Forms.Button heightmapCancelButton;
        private System.Windows.Forms.Button heightmapCreateButton;
        private System.Windows.Forms.CheckBox centerHeightmapCheckbox;
        private System.Windows.Forms.Label heightmapMetersPerSampleLabel;
        private System.Windows.Forms.NumericUpDown heightmapMetersPerSampleUpDown;
    }
}
