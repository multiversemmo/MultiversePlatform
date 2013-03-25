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

namespace ColorizeViewer
{
    partial class ColorizeViewer
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveColorizedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importImageAsTile8bitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importColorsFromImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTileAndColorMetadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadColorLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveColorLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportColorLibraryAsPythonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.color0Button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.color1Button = new System.Windows.Forms.Button();
            this.color2Button = new System.Windows.Forms.Button();
            this.color3Button = new System.Windows.Forms.Button();
            this.colorizeModeCombobox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.hueAdjustTrackBar = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.saturationTrackBar = new System.Windows.Forms.TrackBar();
            this.clearAdjustButton = new System.Windows.Forms.Button();
            this.bakeColorButton = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.label8 = new System.Windows.Forms.Label();
            this.refColorButton = new System.Windows.Forms.Button();
            this.refColorModeComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.applyRefButton = new System.Windows.Forms.Button();
            this.libraryTreeView = new System.Windows.Forms.TreeView();
            this.resetFileColors = new System.Windows.Forms.Button();
            this.tileRefColorsToLibraryButton = new System.Windows.Forms.Button();
            this.colorToLibraryButton = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.valueTrackBar = new System.Windows.Forms.TrackBar();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueAdjustTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationTrackBar)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(977, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadTileToolStripMenuItem,
            this.saveTileToolStripMenuItem,
            this.saveColorizedToolStripMenuItem,
            this.importImageAsTile8bitToolStripMenuItem,
            this.importColorsFromImageToolStripMenuItem,
            this.saveTileAndColorMetadataToolStripMenuItem,
            this.toolStripSeparator1,
            this.loadColorLibraryToolStripMenuItem,
            this.saveColorLibraryToolStripMenuItem,
            this.exportColorLibraryAsPythonToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadTileToolStripMenuItem
            // 
            this.loadTileToolStripMenuItem.Name = "loadTileToolStripMenuItem";
            this.loadTileToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.loadTileToolStripMenuItem.Text = "Load Tile...";
            this.loadTileToolStripMenuItem.Click += new System.EventHandler(this.loadTileToolStripMenuItem_Click);
            // 
            // saveTileToolStripMenuItem
            // 
            this.saveTileToolStripMenuItem.Name = "saveTileToolStripMenuItem";
            this.saveTileToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.saveTileToolStripMenuItem.Text = "Save Tile...";
            this.saveTileToolStripMenuItem.Click += new System.EventHandler(this.saveTileToolStripMenuItem_Click);
            // 
            // saveColorizedToolStripMenuItem
            // 
            this.saveColorizedToolStripMenuItem.Name = "saveColorizedToolStripMenuItem";
            this.saveColorizedToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.saveColorizedToolStripMenuItem.Text = "Save Colorized...";
            this.saveColorizedToolStripMenuItem.Click += new System.EventHandler(this.saveColorizedToolStripMenuItem_Click);
            // 
            // importImageAsTile8bitToolStripMenuItem
            // 
            this.importImageAsTile8bitToolStripMenuItem.Name = "importImageAsTile8bitToolStripMenuItem";
            this.importImageAsTile8bitToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.importImageAsTile8bitToolStripMenuItem.Text = "Import Image As Tile...";
            this.importImageAsTile8bitToolStripMenuItem.Click += new System.EventHandler(this.importImageAsTile8bitToolStripMenuItem_Click);
            // 
            // importColorsFromImageToolStripMenuItem
            // 
            this.importColorsFromImageToolStripMenuItem.Name = "importColorsFromImageToolStripMenuItem";
            this.importColorsFromImageToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.importColorsFromImageToolStripMenuItem.Text = "Import Colors From Image...";
            this.importColorsFromImageToolStripMenuItem.Click += new System.EventHandler(this.importColorsFromImageToolStripMenuItem_Click);
            // 
            // saveTileAndColorMetadataToolStripMenuItem
            // 
            this.saveTileAndColorMetadataToolStripMenuItem.Name = "saveTileAndColorMetadataToolStripMenuItem";
            this.saveTileAndColorMetadataToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.saveTileAndColorMetadataToolStripMenuItem.Text = "Save Tile and Color Metadata...";
            this.saveTileAndColorMetadataToolStripMenuItem.Click += new System.EventHandler(this.saveTileAndColorMetadataToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(239, 6);
            // 
            // loadColorLibraryToolStripMenuItem
            // 
            this.loadColorLibraryToolStripMenuItem.Name = "loadColorLibraryToolStripMenuItem";
            this.loadColorLibraryToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.loadColorLibraryToolStripMenuItem.Text = "Load Color Library...";
            this.loadColorLibraryToolStripMenuItem.Click += new System.EventHandler(this.loadColorLibraryToolStripMenuItem_Click);
            // 
            // saveColorLibraryToolStripMenuItem
            // 
            this.saveColorLibraryToolStripMenuItem.Name = "saveColorLibraryToolStripMenuItem";
            this.saveColorLibraryToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.saveColorLibraryToolStripMenuItem.Text = "Save Color Library...";
            this.saveColorLibraryToolStripMenuItem.Click += new System.EventHandler(this.saveColorLibraryToolStripMenuItem_Click);
            // 
            // exportColorLibraryAsPythonToolStripMenuItem
            // 
            this.exportColorLibraryAsPythonToolStripMenuItem.Name = "exportColorLibraryAsPythonToolStripMenuItem";
            this.exportColorLibraryAsPythonToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.exportColorLibraryAsPythonToolStripMenuItem.Text = "Export Color Library as Python...";
            this.exportColorLibraryAsPythonToolStripMenuItem.Click += new System.EventHandler(this.exportColorLibraryAsPythonToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(239, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 27);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(256, 256);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // color0Button
            // 
            this.color0Button.AllowDrop = true;
            this.color0Button.BackColor = System.Drawing.SystemColors.Control;
            this.color0Button.Location = new System.Drawing.Point(407, 45);
            this.color0Button.Name = "color0Button";
            this.color0Button.Size = new System.Drawing.Size(50, 50);
            this.color0Button.TabIndex = 2;
            this.color0Button.Tag = "0";
            this.color0Button.UseVisualStyleBackColor = false;
            this.color0Button.Click += new System.EventHandler(this.color0Button_Click);
            this.color0Button.DragDrop += new System.Windows.Forms.DragEventHandler(this.color0Button_DragDrop);
            this.color0Button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.color0Button_MouseDown);
            this.color0Button.DragEnter += new System.Windows.Forms.DragEventHandler(this.color0Button_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(331, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Color 0:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(331, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Color 1:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(331, 234);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Color 2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(331, 319);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Color 3";
            // 
            // color1Button
            // 
            this.color1Button.AllowDrop = true;
            this.color1Button.BackColor = System.Drawing.SystemColors.Control;
            this.color1Button.Location = new System.Drawing.Point(407, 130);
            this.color1Button.Name = "color1Button";
            this.color1Button.Size = new System.Drawing.Size(50, 50);
            this.color1Button.TabIndex = 7;
            this.color1Button.Tag = "1";
            this.color1Button.UseVisualStyleBackColor = false;
            this.color1Button.Click += new System.EventHandler(this.color1Button_Click);
            this.color1Button.DragDrop += new System.Windows.Forms.DragEventHandler(this.color1Button_DragDrop);
            this.color1Button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.color1Button_MouseDown);
            this.color1Button.DragEnter += new System.Windows.Forms.DragEventHandler(this.color1Button_DragEnter);
            // 
            // color2Button
            // 
            this.color2Button.AllowDrop = true;
            this.color2Button.BackColor = System.Drawing.SystemColors.Control;
            this.color2Button.Location = new System.Drawing.Point(407, 215);
            this.color2Button.Name = "color2Button";
            this.color2Button.Size = new System.Drawing.Size(50, 50);
            this.color2Button.TabIndex = 8;
            this.color2Button.Tag = "2";
            this.color2Button.UseVisualStyleBackColor = false;
            this.color2Button.Click += new System.EventHandler(this.color2Button_Click);
            this.color2Button.DragDrop += new System.Windows.Forms.DragEventHandler(this.color2Button_DragDrop);
            this.color2Button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.color2Button_MouseDown);
            this.color2Button.DragEnter += new System.Windows.Forms.DragEventHandler(this.color2Button_DragEnter);
            // 
            // color3Button
            // 
            this.color3Button.AllowDrop = true;
            this.color3Button.BackColor = System.Drawing.SystemColors.Control;
            this.color3Button.Location = new System.Drawing.Point(407, 300);
            this.color3Button.Name = "color3Button";
            this.color3Button.Size = new System.Drawing.Size(50, 50);
            this.color3Button.TabIndex = 9;
            this.color3Button.Tag = "3";
            this.color3Button.UseVisualStyleBackColor = false;
            this.color3Button.Click += new System.EventHandler(this.color3Button_Click);
            this.color3Button.DragDrop += new System.Windows.Forms.DragEventHandler(this.color3Button_DragDrop);
            this.color3Button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.color3Button_MouseDown);
            this.color3Button.DragEnter += new System.Windows.Forms.DragEventHandler(this.color3Button_DragEnter);
            // 
            // colorizeModeCombobox
            // 
            this.colorizeModeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorizeModeCombobox.FormattingEnabled = true;
            this.colorizeModeCombobox.Items.AddRange(new object[] {
            "Simple Multiply",
            "Expanded Multiply"});
            this.colorizeModeCombobox.Location = new System.Drawing.Point(135, 324);
            this.colorizeModeCombobox.Name = "colorizeModeCombobox";
            this.colorizeModeCombobox.Size = new System.Drawing.Size(121, 21);
            this.colorizeModeCombobox.TabIndex = 10;
            this.colorizeModeCombobox.SelectedIndexChanged += new System.EventHandler(this.colorizeModeCombobox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 327);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Colorization Method:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(322, 378);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Adjust Hues:";
            // 
            // hueAdjustTrackBar
            // 
            this.hueAdjustTrackBar.LargeChange = 30;
            this.hueAdjustTrackBar.Location = new System.Drawing.Point(345, 394);
            this.hueAdjustTrackBar.Maximum = 180;
            this.hueAdjustTrackBar.Minimum = -180;
            this.hueAdjustTrackBar.Name = "hueAdjustTrackBar";
            this.hueAdjustTrackBar.Size = new System.Drawing.Size(253, 45);
            this.hueAdjustTrackBar.SmallChange = 5;
            this.hueAdjustTrackBar.TabIndex = 16;
            this.hueAdjustTrackBar.TickFrequency = 10;
            this.hueAdjustTrackBar.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(322, 442);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(90, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Adjust Saturation:";
            // 
            // saturationTrackBar
            // 
            this.saturationTrackBar.Location = new System.Drawing.Point(345, 458);
            this.saturationTrackBar.Maximum = 100;
            this.saturationTrackBar.Name = "saturationTrackBar";
            this.saturationTrackBar.Size = new System.Drawing.Size(253, 45);
            this.saturationTrackBar.TabIndex = 18;
            this.saturationTrackBar.TickFrequency = 5;
            this.saturationTrackBar.Value = 50;
            this.saturationTrackBar.Scroll += new System.EventHandler(this.saturationTrackBar_Scroll);
            // 
            // clearAdjustButton
            // 
            this.clearAdjustButton.Location = new System.Drawing.Point(320, 595);
            this.clearAdjustButton.Name = "clearAdjustButton";
            this.clearAdjustButton.Size = new System.Drawing.Size(137, 23);
            this.clearAdjustButton.TabIndex = 19;
            this.clearAdjustButton.Text = "Clear Color Adjustments";
            this.clearAdjustButton.UseVisualStyleBackColor = true;
            this.clearAdjustButton.Click += new System.EventHandler(this.clearAdjustButton_Click);
            // 
            // bakeColorButton
            // 
            this.bakeColorButton.Location = new System.Drawing.Point(490, 595);
            this.bakeColorButton.Name = "bakeColorButton";
            this.bakeColorButton.Size = new System.Drawing.Size(135, 23);
            this.bakeColorButton.TabIndex = 20;
            this.bakeColorButton.Text = "Bake Color Adjustments";
            this.bakeColorButton.UseVisualStyleBackColor = true;
            this.bakeColorButton.Click += new System.EventHandler(this.bakeColorButton_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(78, 48);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(77, 22);
            this.toolStripMenuItem2.Text = " ";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.BackColor = System.Drawing.Color.Yellow;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(77, 22);
            this.toolStripMenuItem3.Text = " ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(487, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Reference Color:";
            // 
            // refColorButton
            // 
            this.refColorButton.Location = new System.Drawing.Point(600, 45);
            this.refColorButton.Name = "refColorButton";
            this.refColorButton.Size = new System.Drawing.Size(50, 50);
            this.refColorButton.TabIndex = 22;
            this.refColorButton.UseVisualStyleBackColor = false;
            this.refColorButton.Click += new System.EventHandler(this.refColorButton_Click);
            // 
            // refColorModeComboBox
            // 
            this.refColorModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.refColorModeComboBox.FormattingEnabled = true;
            this.refColorModeComboBox.Items.AddRange(new object[] {
            "Average Color",
            "Color 0",
            "Color 1",
            "Color 2",
            "Color 3"});
            this.refColorModeComboBox.Location = new System.Drawing.Point(529, 159);
            this.refColorModeComboBox.Name = "refColorModeComboBox";
            this.refColorModeComboBox.Size = new System.Drawing.Size(121, 21);
            this.refColorModeComboBox.TabIndex = 23;
            this.refColorModeComboBox.SelectedIndexChanged += new System.EventHandler(this.refColorModeComboBox_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(487, 130);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(140, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Reference Color Applies To:";
            // 
            // applyRefButton
            // 
            this.applyRefButton.Location = new System.Drawing.Point(490, 203);
            this.applyRefButton.Name = "applyRefButton";
            this.applyRefButton.Size = new System.Drawing.Size(137, 23);
            this.applyRefButton.TabIndex = 25;
            this.applyRefButton.Text = "Apply Reference Color";
            this.applyRefButton.UseVisualStyleBackColor = true;
            this.applyRefButton.Click += new System.EventHandler(this.applyRefButton_Click);
            // 
            // libraryTreeView
            // 
            this.libraryTreeView.FullRowSelect = true;
            this.libraryTreeView.HideSelection = false;
            this.libraryTreeView.Location = new System.Drawing.Point(668, 27);
            this.libraryTreeView.Name = "libraryTreeView";
            this.libraryTreeView.Size = new System.Drawing.Size(281, 444);
            this.libraryTreeView.TabIndex = 26;
            this.libraryTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.libraryTreeView_AfterSelect);
            this.libraryTreeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.libraryTreeView_KeyUp);
            // 
            // resetFileColors
            // 
            this.resetFileColors.Location = new System.Drawing.Point(490, 260);
            this.resetFileColors.Name = "resetFileColors";
            this.resetFileColors.Size = new System.Drawing.Size(137, 23);
            this.resetFileColors.TabIndex = 27;
            this.resetFileColors.Text = "Reset Original Colors";
            this.resetFileColors.UseVisualStyleBackColor = true;
            this.resetFileColors.Click += new System.EventHandler(this.resetFileColors_Click);
            // 
            // tileRefColorsToLibraryButton
            // 
            this.tileRefColorsToLibraryButton.Location = new System.Drawing.Point(706, 497);
            this.tileRefColorsToLibraryButton.Name = "tileRefColorsToLibraryButton";
            this.tileRefColorsToLibraryButton.Size = new System.Drawing.Size(195, 23);
            this.tileRefColorsToLibraryButton.TabIndex = 28;
            this.tileRefColorsToLibraryButton.Text = "Tile Reference Colors To Library";
            this.tileRefColorsToLibraryButton.UseVisualStyleBackColor = true;
            this.tileRefColorsToLibraryButton.Click += new System.EventHandler(this.tileRefColorsToLibraryButton_Click);
            // 
            // colorToLibraryButton
            // 
            this.colorToLibraryButton.Location = new System.Drawing.Point(706, 543);
            this.colorToLibraryButton.Name = "colorToLibraryButton";
            this.colorToLibraryButton.Size = new System.Drawing.Size(195, 23);
            this.colorToLibraryButton.TabIndex = 29;
            this.colorToLibraryButton.Text = "Save Current Color To Library";
            this.colorToLibraryButton.UseVisualStyleBackColor = true;
            this.colorToLibraryButton.Click += new System.EventHandler(this.colorToLibraryButton_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(322, 506);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(127, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Adjust Value(Luminance):";
            // 
            // valueTrackBar
            // 
            this.valueTrackBar.Location = new System.Drawing.Point(345, 531);
            this.valueTrackBar.Maximum = 100;
            this.valueTrackBar.Name = "valueTrackBar";
            this.valueTrackBar.Size = new System.Drawing.Size(253, 45);
            this.valueTrackBar.TabIndex = 31;
            this.valueTrackBar.TickFrequency = 5;
            this.valueTrackBar.Value = 50;
            this.valueTrackBar.Scroll += new System.EventHandler(this.valueTrackBar_Scroll);
            // 
            // ColorizeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(977, 644);
            this.Controls.Add(this.valueTrackBar);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.colorToLibraryButton);
            this.Controls.Add(this.tileRefColorsToLibraryButton);
            this.Controls.Add(this.resetFileColors);
            this.Controls.Add(this.libraryTreeView);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.applyRefButton);
            this.Controls.Add(this.colorizeModeCombobox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.refColorModeComboBox);
            this.Controls.Add(this.refColorButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.saturationTrackBar);
            this.Controls.Add(this.bakeColorButton);
            this.Controls.Add(this.clearAdjustButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.hueAdjustTrackBar);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.color2Button);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.color3Button);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.color1Button);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.color0Button);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ColorizeViewer";
            this.Text = "ColorizeViewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColorizeViewer_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueAdjustTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationTrackBar)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.valueTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTileToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button color0Button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button color1Button;
        private System.Windows.Forms.Button color2Button;
        private System.Windows.Forms.Button color3Button;
        private System.Windows.Forms.ComboBox colorizeModeCombobox;
        private System.Windows.Forms.ToolStripMenuItem saveTileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importColorsFromImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveColorizedToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem saveTileAndColorMetadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importImageAsTile8bitToolStripMenuItem;
        private System.Windows.Forms.TrackBar hueAdjustTrackBar;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar saturationTrackBar;
        private System.Windows.Forms.Button clearAdjustButton;
        private System.Windows.Forms.Button bakeColorButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button refColorButton;
        private System.Windows.Forms.ComboBox refColorModeComboBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button applyRefButton;
        private System.Windows.Forms.TreeView libraryTreeView;
        private System.Windows.Forms.Button resetFileColors;
        private System.Windows.Forms.ToolStripMenuItem saveColorLibraryToolStripMenuItem;
        private System.Windows.Forms.Button tileRefColorsToLibraryButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem loadColorLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Button colorToLibraryButton;
        private System.Windows.Forms.ToolStripMenuItem exportColorLibraryAsPythonToolStripMenuItem;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TrackBar valueTrackBar;
    }
}

