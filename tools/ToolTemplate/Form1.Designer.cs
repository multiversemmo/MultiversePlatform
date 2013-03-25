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

namespace Multiverse.Tools.ToolTemplate
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.wireFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayOceanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayTerrainTilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayTerrainStitchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spinCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.axiomPictureBox = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axiomPictureBox)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.viewToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1016, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.viewToolStripMenuItem.Text = "Edit";
            // 
            // viewToolStripMenuItem1
            // 
            this.viewToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireFrameToolStripMenuItem,
            this.displayOceanToolStripMenuItem,
            this.displayTerrainTilesToolStripMenuItem,
            this.displayTerrainStitchesToolStripMenuItem,
            this.spinCameraToolStripMenuItem});
            this.viewToolStripMenuItem1.Name = "viewToolStripMenuItem1";
            this.viewToolStripMenuItem1.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem1.Text = "View";
            this.viewToolStripMenuItem1.DropDownOpening += new System.EventHandler(this.viewToolStripMenuItem1_DropDownOpening);
            // 
            // wireFrameToolStripMenuItem
            // 
            this.wireFrameToolStripMenuItem.Name = "wireFrameToolStripMenuItem";
            this.wireFrameToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.wireFrameToolStripMenuItem.Text = "Wire Frame";
            this.wireFrameToolStripMenuItem.Click += new System.EventHandler(this.wireFrameToolStripMenuItem_Click);
            // 
            // displayOceanToolStripMenuItem
            // 
            this.displayOceanToolStripMenuItem.Name = "displayOceanToolStripMenuItem";
            this.displayOceanToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.displayOceanToolStripMenuItem.Text = "Display Ocean";
            this.displayOceanToolStripMenuItem.Click += new System.EventHandler(this.displayOceanToolStripMenuItem_Click);
            // 
            // displayTerrainTilesToolStripMenuItem
            // 
            this.displayTerrainTilesToolStripMenuItem.Name = "displayTerrainTilesToolStripMenuItem";
            this.displayTerrainTilesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.displayTerrainTilesToolStripMenuItem.Text = "Display Terrain Tiles";
            this.displayTerrainTilesToolStripMenuItem.Click += new System.EventHandler(this.displayTerrainTilesToolStripMenuItem_Click);
            // 
            // displayTerrainStitchesToolStripMenuItem
            // 
            this.displayTerrainStitchesToolStripMenuItem.Name = "displayTerrainStitchesToolStripMenuItem";
            this.displayTerrainStitchesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.displayTerrainStitchesToolStripMenuItem.Text = "Display Terrain Stitches";
            this.displayTerrainStitchesToolStripMenuItem.Click += new System.EventHandler(this.displayTerrainStitchesToolStripMenuItem_Click);
            // 
            // spinCameraToolStripMenuItem
            // 
            this.spinCameraToolStripMenuItem.Name = "spinCameraToolStripMenuItem";
            this.spinCameraToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.spinCameraToolStripMenuItem.Text = "Spin Camera";
            this.spinCameraToolStripMenuItem.Click += new System.EventHandler(this.spinCameraToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 719);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1016, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // axiomPictureBox
            // 
            this.axiomPictureBox.Location = new System.Drawing.Point(251, 66);
            this.axiomPictureBox.Name = "axiomPictureBox";
            this.axiomPictureBox.Size = new System.Drawing.Size(765, 650);
            this.axiomPictureBox.TabIndex = 2;
            this.axiomPictureBox.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1016, 39);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.axiomPictureBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "ToolTemplate";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axiomPictureBox)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.PictureBox axiomPictureBox;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayOceanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayTerrainTilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayTerrainStitchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spinCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }
}

