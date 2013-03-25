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

namespace Multiverse.Tools.AxiomTester
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
            this.displayTerrainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spinCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.FPSLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.rendersPerFrameLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.renderCallsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.commentLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.setPassCallsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.vertexCountLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.axiomPictureBox = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.renderObjectsPanel = new System.Windows.Forms.Panel();
            this.planeUnitsPanel = new System.Windows.Forms.Panel();
            this.planeUnitsTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.animateCheckBox = new System.Windows.Forms.CheckBox();
            this.uniqueTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.useTextureCheckBox = new System.Windows.Forms.CheckBox();
            this.materialCountLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.whichObjectsComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.unitSizeRadioButton = new System.Windows.Forms.RadioButton();
            this.randomScalesRadioButton = new System.Windows.Forms.RadioButton();
            this.randomSizesRadioButton = new System.Windows.Forms.RadioButton();
            this.randomOrientationsCheckBox = new System.Windows.Forms.CheckBox();
            this.generateObjectsButton = new System.Windows.Forms.Button();
            this.numSharingMaterialLabel = new System.Windows.Forms.Label();
            this.numSharingMaterialTrackBar = new System.Windows.Forms.TrackBar();
            this.numObjectsLabel = new System.Windows.Forms.Label();
            this.numObjectsTrackBar = new System.Windows.Forms.TrackBar();
            this.runDemosPanel = new System.Windows.Forms.Panel();
            this.runRibbonsDemoButton = new System.Windows.Forms.Button();
            this.renderObjectsRadioButton = new System.Windows.Forms.RadioButton();
            this.runDemosRadioButton = new System.Windows.Forms.RadioButton();
            this.meterButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axiomPictureBox)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.renderObjectsPanel.SuspendLayout();
            this.planeUnitsPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSharingMaterialTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numObjectsTrackBar)).BeginInit();
            this.runDemosPanel.SuspendLayout();
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
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1355, 26);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(43, 22);
            this.viewToolStripMenuItem.Text = "Edit";
            // 
            // viewToolStripMenuItem1
            // 
            this.viewToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireFrameToolStripMenuItem,
            this.displayOceanToolStripMenuItem,
            this.displayTerrainToolStripMenuItem,
            this.spinCameraToolStripMenuItem});
            this.viewToolStripMenuItem1.Name = "viewToolStripMenuItem1";
            this.viewToolStripMenuItem1.Size = new System.Drawing.Size(49, 22);
            this.viewToolStripMenuItem1.Text = "View";
            this.viewToolStripMenuItem1.DropDownOpening += new System.EventHandler(this.viewToolStripMenuItem1_DropDownOpening);
            // 
            // wireFrameToolStripMenuItem
            // 
            this.wireFrameToolStripMenuItem.Name = "wireFrameToolStripMenuItem";
            this.wireFrameToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.wireFrameToolStripMenuItem.Text = "Wire Frame";
            this.wireFrameToolStripMenuItem.Click += new System.EventHandler(this.wireFrameToolStripMenuItem_Click);
            // 
            // displayOceanToolStripMenuItem
            // 
            this.displayOceanToolStripMenuItem.Name = "displayOceanToolStripMenuItem";
            this.displayOceanToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.displayOceanToolStripMenuItem.Text = "Display Ocean";
            this.displayOceanToolStripMenuItem.Click += new System.EventHandler(this.displayOceanToolStripMenuItem_Click);
            // 
            // displayTerrainToolStripMenuItem
            // 
            this.displayTerrainToolStripMenuItem.Name = "displayTerrainToolStripMenuItem";
            this.displayTerrainToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.displayTerrainToolStripMenuItem.Text = "Display Terrain";
            this.displayTerrainToolStripMenuItem.Click += new System.EventHandler(this.displayTerrainToolStripMenuItem_Click);
            // 
            // spinCameraToolStripMenuItem
            // 
            this.spinCameraToolStripMenuItem.Name = "spinCameraToolStripMenuItem";
            this.spinCameraToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.spinCameraToolStripMenuItem.Text = "Spin Camera";
            this.spinCameraToolStripMenuItem.Click += new System.EventHandler(this.spinCameraToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FPSLabel,
            this.rendersPerFrameLabel,
            this.renderCallsLabel,
            this.commentLabel,
            this.setPassCallsLabel,
            this.vertexCountLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 889);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1355, 23);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // FPSLabel
            // 
            this.FPSLabel.AutoSize = false;
            this.FPSLabel.Name = "FPSLabel";
            this.FPSLabel.Size = new System.Drawing.Size(100, 18);
            this.FPSLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rendersPerFrameLabel
            // 
            this.rendersPerFrameLabel.AutoSize = false;
            this.rendersPerFrameLabel.Name = "rendersPerFrameLabel";
            this.rendersPerFrameLabel.Size = new System.Drawing.Size(200, 18);
            this.rendersPerFrameLabel.Text = "toolStripStatusLabel1";
            // 
            // renderCallsLabel
            // 
            this.renderCallsLabel.AutoSize = false;
            this.renderCallsLabel.Name = "renderCallsLabel";
            this.renderCallsLabel.Size = new System.Drawing.Size(200, 18);
            this.renderCallsLabel.Text = "toolStripStatusLabel1";
            this.renderCallsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commentLabel
            // 
            this.commentLabel.Name = "commentLabel";
            this.commentLabel.Size = new System.Drawing.Size(0, 18);
            // 
            // setPassCallsLabel
            // 
            this.setPassCallsLabel.AutoSize = false;
            this.setPassCallsLabel.Name = "setPassCallsLabel";
            this.setPassCallsLabel.Size = new System.Drawing.Size(240, 18);
            this.setPassCallsLabel.Text = "toolStripStatusLabel1";
            // 
            // vertexCountLabel
            // 
            this.vertexCountLabel.Name = "vertexCountLabel";
            this.vertexCountLabel.Size = new System.Drawing.Size(141, 18);
            this.vertexCountLabel.Text = "toolStripStatusLabel1";
            // 
            // axiomPictureBox
            // 
            this.axiomPictureBox.Location = new System.Drawing.Point(335, 30);
            this.axiomPictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.axiomPictureBox.Name = "axiomPictureBox";
            this.axiomPictureBox.Size = new System.Drawing.Size(1020, 800);
            this.axiomPictureBox.TabIndex = 2;
            this.axiomPictureBox.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 26);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1355, 39);
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
            // renderObjectsPanel
            // 
            this.renderObjectsPanel.Controls.Add(this.planeUnitsPanel);
            this.renderObjectsPanel.Controls.Add(this.animateCheckBox);
            this.renderObjectsPanel.Controls.Add(this.uniqueTexturesCheckBox);
            this.renderObjectsPanel.Controls.Add(this.useTextureCheckBox);
            this.renderObjectsPanel.Controls.Add(this.materialCountLabel);
            this.renderObjectsPanel.Controls.Add(this.label1);
            this.renderObjectsPanel.Controls.Add(this.whichObjectsComboBox);
            this.renderObjectsPanel.Controls.Add(this.groupBox1);
            this.renderObjectsPanel.Controls.Add(this.randomOrientationsCheckBox);
            this.renderObjectsPanel.Controls.Add(this.generateObjectsButton);
            this.renderObjectsPanel.Controls.Add(this.numSharingMaterialLabel);
            this.renderObjectsPanel.Controls.Add(this.numSharingMaterialTrackBar);
            this.renderObjectsPanel.Controls.Add(this.numObjectsLabel);
            this.renderObjectsPanel.Controls.Add(this.numObjectsTrackBar);
            this.renderObjectsPanel.Location = new System.Drawing.Point(12, 126);
            this.renderObjectsPanel.Name = "renderObjectsPanel";
            this.renderObjectsPanel.Size = new System.Drawing.Size(312, 608);
            this.renderObjectsPanel.TabIndex = 4;
            // 
            // planeUnitsPanel
            // 
            this.planeUnitsPanel.Controls.Add(this.planeUnitsTextBox);
            this.planeUnitsPanel.Controls.Add(this.label2);
            this.planeUnitsPanel.Location = new System.Drawing.Point(33, 423);
            this.planeUnitsPanel.Name = "planeUnitsPanel";
            this.planeUnitsPanel.Size = new System.Drawing.Size(225, 44);
            this.planeUnitsPanel.TabIndex = 18;
            this.planeUnitsPanel.Visible = false;
            // 
            // planeUnitsTextBox
            // 
            this.planeUnitsTextBox.Location = new System.Drawing.Point(107, 8);
            this.planeUnitsTextBox.Name = "planeUnitsTextBox";
            this.planeUnitsTextBox.Size = new System.Drawing.Size(100, 22);
            this.planeUnitsTextBox.TabIndex = 19;
            this.planeUnitsTextBox.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 18;
            this.label2.Text = "Plane Units:";
            // 
            // animateCheckBox
            // 
            this.animateCheckBox.AutoSize = true;
            this.animateCheckBox.Checked = true;
            this.animateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.animateCheckBox.Location = new System.Drawing.Point(109, 528);
            this.animateCheckBox.Name = "animateCheckBox";
            this.animateCheckBox.Size = new System.Drawing.Size(78, 21);
            this.animateCheckBox.TabIndex = 15;
            this.animateCheckBox.Text = "Animate";
            this.animateCheckBox.UseVisualStyleBackColor = true;
            this.animateCheckBox.Visible = false;
            // 
            // uniqueTexturesCheckBox
            // 
            this.uniqueTexturesCheckBox.AutoSize = true;
            this.uniqueTexturesCheckBox.Enabled = false;
            this.uniqueTexturesCheckBox.Location = new System.Drawing.Point(48, 222);
            this.uniqueTexturesCheckBox.Name = "uniqueTexturesCheckBox";
            this.uniqueTexturesCheckBox.Size = new System.Drawing.Size(195, 21);
            this.uniqueTexturesCheckBox.TabIndex = 14;
            this.uniqueTexturesCheckBox.Text = "Generate Unique Textures";
            this.uniqueTexturesCheckBox.UseVisualStyleBackColor = true;
            this.uniqueTexturesCheckBox.CheckedChanged += new System.EventHandler(this.uniqueTexturesCheckBox_CheckedChanged);
            // 
            // useTextureCheckBox
            // 
            this.useTextureCheckBox.AutoSize = true;
            this.useTextureCheckBox.Location = new System.Drawing.Point(48, 195);
            this.useTextureCheckBox.Name = "useTextureCheckBox";
            this.useTextureCheckBox.Size = new System.Drawing.Size(210, 21);
            this.useTextureCheckBox.TabIndex = 13;
            this.useTextureCheckBox.Text = "Use Materials With A Texture";
            this.useTextureCheckBox.UseVisualStyleBackColor = true;
            this.useTextureCheckBox.CheckedChanged += new System.EventHandler(this.useTextureCheckBox_CheckedChanged);
            // 
            // materialCountLabel
            // 
            this.materialCountLabel.AutoSize = true;
            this.materialCountLabel.Location = new System.Drawing.Point(88, 165);
            this.materialCountLabel.Name = "materialCountLabel";
            this.materialCountLabel.Size = new System.Drawing.Size(115, 17);
            this.materialCountLabel.TabIndex = 12;
            this.materialCountLabel.Text = "Material Count: 0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(88, 469);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 17);
            this.label1.TabIndex = 11;
            this.label1.Text = "Which Objects?";
            // 
            // whichObjectsComboBox
            // 
            this.whichObjectsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.whichObjectsComboBox.FormattingEnabled = true;
            this.whichObjectsComboBox.Items.AddRange(new object[] {
            "Boxes",
            "Ellipsoids",
            "Cylinders",
            "Planes",
            "Random",
            "Zombie",
            "Human Female"});
            this.whichObjectsComboBox.Location = new System.Drawing.Point(48, 498);
            this.whichObjectsComboBox.Name = "whichObjectsComboBox";
            this.whichObjectsComboBox.Size = new System.Drawing.Size(198, 24);
            this.whichObjectsComboBox.TabIndex = 10;
            this.whichObjectsComboBox.SelectedIndexChanged += new System.EventHandler(this.whichObjectsComboBox_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.unitSizeRadioButton);
            this.groupBox1.Controls.Add(this.randomScalesRadioButton);
            this.groupBox1.Controls.Add(this.randomSizesRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(21, 295);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 122);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Size/Scale";
            // 
            // unitSizeRadioButton
            // 
            this.unitSizeRadioButton.AutoSize = true;
            this.unitSizeRadioButton.Checked = true;
            this.unitSizeRadioButton.Location = new System.Drawing.Point(49, 33);
            this.unitSizeRadioButton.Name = "unitSizeRadioButton";
            this.unitSizeRadioButton.Size = new System.Drawing.Size(82, 21);
            this.unitSizeRadioButton.TabIndex = 2;
            this.unitSizeRadioButton.TabStop = true;
            this.unitSizeRadioButton.Text = "Unit Size";
            this.unitSizeRadioButton.UseVisualStyleBackColor = true;
            this.unitSizeRadioButton.CheckedChanged += new System.EventHandler(this.unitSizeRadioButton_CheckedChanged);
            // 
            // randomScalesRadioButton
            // 
            this.randomScalesRadioButton.AutoSize = true;
            this.randomScalesRadioButton.Location = new System.Drawing.Point(49, 87);
            this.randomScalesRadioButton.Name = "randomScalesRadioButton";
            this.randomScalesRadioButton.Size = new System.Drawing.Size(164, 21);
            this.randomScalesRadioButton.TabIndex = 1;
            this.randomScalesRadioButton.Text = "Random X/Y/Z Scales";
            this.randomScalesRadioButton.UseVisualStyleBackColor = true;
            this.randomScalesRadioButton.CheckedChanged += new System.EventHandler(this.randomScalesRadioButton_CheckedChanged);
            // 
            // randomSizesRadioButton
            // 
            this.randomSizesRadioButton.AutoSize = true;
            this.randomSizesRadioButton.Location = new System.Drawing.Point(49, 60);
            this.randomSizesRadioButton.Name = "randomSizesRadioButton";
            this.randomSizesRadioButton.Size = new System.Drawing.Size(117, 21);
            this.randomSizesRadioButton.TabIndex = 0;
            this.randomSizesRadioButton.Text = "Random Sizes";
            this.randomSizesRadioButton.UseVisualStyleBackColor = true;
            this.randomSizesRadioButton.CheckedChanged += new System.EventHandler(this.randomSizesRadioButton_CheckedChanged);
            // 
            // randomOrientationsCheckBox
            // 
            this.randomOrientationsCheckBox.AutoSize = true;
            this.randomOrientationsCheckBox.Location = new System.Drawing.Point(48, 258);
            this.randomOrientationsCheckBox.Name = "randomOrientationsCheckBox";
            this.randomOrientationsCheckBox.Size = new System.Drawing.Size(161, 21);
            this.randomOrientationsCheckBox.TabIndex = 5;
            this.randomOrientationsCheckBox.Text = "Random Orientations";
            this.randomOrientationsCheckBox.UseVisualStyleBackColor = true;
            this.randomOrientationsCheckBox.CheckedChanged += new System.EventHandler(this.randomOrientationsCheckBox_CheckedChanged);
            // 
            // generateObjectsButton
            // 
            this.generateObjectsButton.Location = new System.Drawing.Point(70, 555);
            this.generateObjectsButton.Name = "generateObjectsButton";
            this.generateObjectsButton.Size = new System.Drawing.Size(155, 35);
            this.generateObjectsButton.TabIndex = 4;
            this.generateObjectsButton.Text = "Generate Objects";
            this.generateObjectsButton.UseVisualStyleBackColor = true;
            this.generateObjectsButton.Click += new System.EventHandler(this.generateObjectsButton_Click);
            // 
            // numSharingMaterialLabel
            // 
            this.numSharingMaterialLabel.AutoSize = true;
            this.numSharingMaterialLabel.Location = new System.Drawing.Point(18, 99);
            this.numSharingMaterialLabel.Name = "numSharingMaterialLabel";
            this.numSharingMaterialLabel.Size = new System.Drawing.Size(269, 17);
            this.numSharingMaterialLabel.TabIndex = 3;
            this.numSharingMaterialLabel.Text = "Number Objects Sharing Each Material: 0";
            // 
            // numSharingMaterialTrackBar
            // 
            this.numSharingMaterialTrackBar.LargeChange = 200;
            this.numSharingMaterialTrackBar.Location = new System.Drawing.Point(21, 119);
            this.numSharingMaterialTrackBar.Maximum = 2000;
            this.numSharingMaterialTrackBar.Name = "numSharingMaterialTrackBar";
            this.numSharingMaterialTrackBar.Size = new System.Drawing.Size(275, 53);
            this.numSharingMaterialTrackBar.TabIndex = 2;
            this.numSharingMaterialTrackBar.TickFrequency = 25;
            this.numSharingMaterialTrackBar.ValueChanged += new System.EventHandler(this.numSharingMaterialTrackBar_ValueChanged);
            // 
            // numObjectsLabel
            // 
            this.numObjectsLabel.AutoSize = true;
            this.numObjectsLabel.Location = new System.Drawing.Point(79, 23);
            this.numObjectsLabel.Name = "numObjectsLabel";
            this.numObjectsLabel.Size = new System.Drawing.Size(142, 17);
            this.numObjectsLabel.TabIndex = 1;
            this.numObjectsLabel.Text = "Number of Objects: 0";
            // 
            // numObjectsTrackBar
            // 
            this.numObjectsTrackBar.LargeChange = 200;
            this.numObjectsTrackBar.Location = new System.Drawing.Point(21, 43);
            this.numObjectsTrackBar.Maximum = 2000;
            this.numObjectsTrackBar.Name = "numObjectsTrackBar";
            this.numObjectsTrackBar.Size = new System.Drawing.Size(275, 53);
            this.numObjectsTrackBar.TabIndex = 0;
            this.numObjectsTrackBar.TickFrequency = 25;
            this.numObjectsTrackBar.ValueChanged += new System.EventHandler(this.numObjectsTrackBar_ValueChanged);
            // 
            // runDemosPanel
            // 
            this.runDemosPanel.Controls.Add(this.runRibbonsDemoButton);
            this.runDemosPanel.Location = new System.Drawing.Point(12, 126);
            this.runDemosPanel.Name = "runDemosPanel";
            this.runDemosPanel.Size = new System.Drawing.Size(312, 436);
            this.runDemosPanel.TabIndex = 21;
            this.runDemosPanel.TabStop = true;
            // 
            // runRibbonsDemoButton
            // 
            this.runRibbonsDemoButton.Location = new System.Drawing.Point(57, 43);
            this.runRibbonsDemoButton.Name = "runRibbonsDemoButton";
            this.runRibbonsDemoButton.Size = new System.Drawing.Size(189, 30);
            this.runRibbonsDemoButton.TabIndex = 0;
            this.runRibbonsDemoButton.Text = "Run RibbonTrail Demo";
            this.runRibbonsDemoButton.UseVisualStyleBackColor = true;
            this.runRibbonsDemoButton.Click += new System.EventHandler(this.runRibbonsDemoButton_Click);
            // 
            // renderObjectsRadioButton
            // 
            this.renderObjectsRadioButton.AutoSize = true;
            this.renderObjectsRadioButton.Checked = true;
            this.renderObjectsRadioButton.Location = new System.Drawing.Point(32, 80);
            this.renderObjectsRadioButton.Name = "renderObjectsRadioButton";
            this.renderObjectsRadioButton.Size = new System.Drawing.Size(125, 21);
            this.renderObjectsRadioButton.TabIndex = 5;
            this.renderObjectsRadioButton.TabStop = true;
            this.renderObjectsRadioButton.Text = "Render Objects";
            this.renderObjectsRadioButton.UseVisualStyleBackColor = true;
            this.renderObjectsRadioButton.Click += new System.EventHandler(this.renderObjectsRadioButton_Click);
            // 
            // runDemosRadioButton
            // 
            this.runDemosRadioButton.AutoSize = true;
            this.runDemosRadioButton.Location = new System.Drawing.Point(170, 80);
            this.runDemosRadioButton.Name = "runDemosRadioButton";
            this.runDemosRadioButton.Size = new System.Drawing.Size(100, 21);
            this.runDemosRadioButton.TabIndex = 6;
            this.runDemosRadioButton.Text = "Run Demos";
            this.runDemosRadioButton.UseVisualStyleBackColor = true;
            this.runDemosRadioButton.Click += new System.EventHandler(this.runDemosRadioButton_Click);
            // 
            // meterButton
            // 
            this.meterButton.Location = new System.Drawing.Point(82, 768);
            this.meterButton.Name = "meterButton";
            this.meterButton.Size = new System.Drawing.Size(155, 35);
            this.meterButton.TabIndex = 20;
            this.meterButton.Text = "Meter One Frame";
            this.meterButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1355, 912);
            this.Controls.Add(this.meterButton);
            this.Controls.Add(this.runDemosRadioButton);
            this.Controls.Add(this.renderObjectsRadioButton);
            this.Controls.Add(this.renderObjectsPanel);
            this.Controls.Add(this.runDemosPanel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.axiomPictureBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Axiom Tester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axiomPictureBox)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.renderObjectsPanel.ResumeLayout(false);
            this.renderObjectsPanel.PerformLayout();
            this.planeUnitsPanel.ResumeLayout(false);
            this.planeUnitsPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSharingMaterialTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numObjectsTrackBar)).EndInit();
            this.runDemosPanel.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem displayTerrainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spinCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.Panel renderObjectsPanel;
        private System.Windows.Forms.Label numSharingMaterialLabel;
        private System.Windows.Forms.TrackBar numSharingMaterialTrackBar;
        private System.Windows.Forms.Label numObjectsLabel;
        private System.Windows.Forms.TrackBar numObjectsTrackBar;
        private System.Windows.Forms.Button generateObjectsButton;
        private System.Windows.Forms.ToolStripStatusLabel FPSLabel;
        private System.Windows.Forms.ToolStripStatusLabel renderCallsLabel;
        private System.Windows.Forms.CheckBox randomOrientationsCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton randomScalesRadioButton;
        private System.Windows.Forms.RadioButton randomSizesRadioButton;
        private System.Windows.Forms.RadioButton unitSizeRadioButton;
        private System.Windows.Forms.ToolStripStatusLabel rendersPerFrameLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox whichObjectsComboBox;
        private System.Windows.Forms.ToolStripStatusLabel commentLabel;
        private System.Windows.Forms.Label materialCountLabel;
        private System.Windows.Forms.CheckBox useTextureCheckBox;
        private System.Windows.Forms.ToolStripStatusLabel setPassCallsLabel;
        private System.Windows.Forms.CheckBox uniqueTexturesCheckBox;
        private System.Windows.Forms.ToolStripStatusLabel vertexCountLabel;
        private System.Windows.Forms.CheckBox animateCheckBox;
        private System.Windows.Forms.Panel planeUnitsPanel;
        private System.Windows.Forms.TextBox planeUnitsTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton renderObjectsRadioButton;
        private System.Windows.Forms.RadioButton runDemosRadioButton;
        private System.Windows.Forms.Button meterButton;
        private System.Windows.Forms.Panel runDemosPanel;
        private System.Windows.Forms.Button runRibbonsDemoButton;
    }
}

