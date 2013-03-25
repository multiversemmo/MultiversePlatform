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

namespace Multiverse.Tools.ModelViewer
{
    partial class ModelViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelViewer));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.designateRepositoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.wireFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayTerrainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGroundPlaneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCollisionVolumesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBoundingBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spinCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spinLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayBoneInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setMaximumFramesPerSecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bgColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.materialSchemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripHelpItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchReleaseNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submitABugReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.fpsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fpsStatusValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.modelHeightLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.modelHeightValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.spinCameraToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.mouseModeToolStripButton = new System.Windows.Forms.ToolStripSplitButton();
            this.moveCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDirectionalLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.axiomPictureBox = new System.Windows.Forms.PictureBox();
            this.bonesPage = new System.Windows.Forms.TabPage();
            this.showBonesGroupBox = new System.Windows.Forms.GroupBox();
            this.boneAxisSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.showBonesCheckBox = new System.Windows.Forms.CheckBox();
            this.boneAxisSizeLabel = new System.Windows.Forms.Label();
            this.bonesLinkLabel = new System.Windows.Forms.LinkLabel();
            this.bonesTreeView = new System.Windows.Forms.TreeView();
            this.cameraPage = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cameraControlsButton = new System.Windows.Forms.Button();
            this.cameraFocusHeightGroupBox = new System.Windows.Forms.GroupBox();
            this.cameraControlsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.cameraFocusHeightTrackbar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.cameraNearDistanceGroupBox = new System.Windows.Forms.GroupBox();
            this.nearDistanceValueLabel = new System.Windows.Forms.Label();
            this.whatsNearDistanceLabel = new System.Windows.Forms.LinkLabel();
            this.cameraNearDistanceTrackBar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.showNormalsGroupBox = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.showNormalsCheckBox = new System.Windows.Forms.CheckBox();
            this.normalsAxisSizeLabel = new System.Windows.Forms.Label();
            this.normalsAxisSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.lightingPage = new System.Windows.Forms.TabPage();
            this.lightingFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ambientLightButton = new System.Windows.Forms.Button();
            this.ambientLightGroupBox = new System.Windows.Forms.GroupBox();
            this.ambientLightLinkLabel = new System.Windows.Forms.LinkLabel();
            this.ambientLightColorLabel = new System.Windows.Forms.Label();
            this.ambientLightColorButton = new System.Windows.Forms.Button();
            this.directionalLightButton = new System.Windows.Forms.Button();
            this.directionalLightGroupBox = new System.Windows.Forms.GroupBox();
            this.directionalLightLinkLabel = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.lightZenithTrackBar = new System.Windows.Forms.TrackBar();
            this.lightAzimuthTrackBar = new System.Windows.Forms.TrackBar();
            this.specularColorButton = new System.Windows.Forms.Button();
            this.diffuseColorButton = new System.Windows.Forms.Button();
            this.specularColorLabel = new System.Windows.Forms.Label();
            this.diffuseColorLabel = new System.Windows.Forms.Label();
            this.socketsPage = new System.Windows.Forms.TabPage();
            this.showSocketGroupBox = new System.Windows.Forms.GroupBox();
            this.socketAxisSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.socketAxisSizeLabel = new System.Windows.Forms.Label();
            this.showSocketsCheckBox = new System.Windows.Forms.CheckBox();
            this.socketsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.parentBoneValueLabel = new System.Windows.Forms.Label();
            this.parentBoneLabel = new System.Windows.Forms.Label();
            this.socketListBox = new System.Windows.Forms.CheckedListBox();
            this.animPage = new System.Windows.Forms.TabPage();
            this.timeCurrentLabel = new System.Windows.Forms.Label();
            this.timeEndLabel = new System.Windows.Forms.Label();
            this.timeStartLabel = new System.Windows.Forms.Label();
            this.animationTrackBar = new System.Windows.Forms.TrackBar();
            this.animLinkLabel = new System.Windows.Forms.LinkLabel();
            this.animationSpeedTextBox = new System.Windows.Forms.TextBox();
            this.animationSpeedLabel = new System.Windows.Forms.Label();
            this.loopingCheckBox = new System.Windows.Forms.CheckBox();
            this.playStopButton = new System.Windows.Forms.Button();
            this.animationListBox = new System.Windows.Forms.ListBox();
            this.subMeshPage = new System.Windows.Forms.TabPage();
            this.subMeshTreeView = new System.Windows.Forms.TreeView();
            this.subMeshLinkLabel = new System.Windows.Forms.LinkLabel();
            this.materialLabel = new System.Windows.Forms.Label();
            this.materialTextBox = new System.Windows.Forms.TextBox();
            this.hideButton = new System.Windows.Forms.Button();
            this.showButton = new System.Windows.Forms.Button();
            this.subMeshTextBox = new System.Windows.Forms.TextBox();
            this.hideShowLabel = new System.Windows.Forms.Label();
            this.hideAllButton = new System.Windows.Forms.Button();
            this.showAllButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axiomPictureBox)).BeginInit();
            this.bonesPage.SuspendLayout();
            this.showBonesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boneAxisSizeTrackBar)).BeginInit();
            this.cameraPage.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.cameraFocusHeightGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraFocusHeightTrackbar)).BeginInit();
            this.cameraNearDistanceGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraNearDistanceTrackBar)).BeginInit();
            this.showNormalsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalsAxisSizeTrackBar)).BeginInit();
            this.lightingPage.SuspendLayout();
            this.lightingFlowLayoutPanel.SuspendLayout();
            this.ambientLightGroupBox.SuspendLayout();
            this.directionalLightGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightZenithTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightAzimuthTrackBar)).BeginInit();
            this.socketsPage.SuspendLayout();
            this.showSocketGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.socketAxisSizeTrackBar)).BeginInit();
            this.animPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationTrackBar)).BeginInit();
            this.subMeshPage.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1016, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadModelToolStripMenuItem,
            this.designateRepositoryMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadModelToolStripMenuItem
            // 
            this.loadModelToolStripMenuItem.Name = "loadModelToolStripMenuItem";
            this.loadModelToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.loadModelToolStripMenuItem.Text = "Load &Model...";
            this.loadModelToolStripMenuItem.Click += new System.EventHandler(this.loadModelToolStripMenuItem_Click);
            // 
            // designateRepositoryMenuItem
            // 
            this.designateRepositoryMenuItem.Name = "designateRepositoryMenuItem";
            this.designateRepositoryMenuItem.Size = new System.Drawing.Size(219, 22);
            this.designateRepositoryMenuItem.Text = "Designate &Asset Repository...";
            this.designateRepositoryMenuItem.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem1
            // 
            this.viewToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireFrameToolStripMenuItem,
            this.displayTerrainToolStripMenuItem,
            this.showGroundPlaneToolStripMenuItem,
            this.showCollisionVolumesToolStripMenuItem,
            this.showBoundingBoxToolStripMenuItem,
            this.spinCameraToolStripMenuItem,
            this.spinLightToolStripMenuItem,
            this.displayBoneInformationToolStripMenuItem,
            this.setMaximumFramesPerSecondToolStripMenuItem,
            this.bgColorToolStripMenuItem,
            this.materialSchemeToolStripMenuItem});
            this.viewToolStripMenuItem1.Name = "viewToolStripMenuItem1";
            this.viewToolStripMenuItem1.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem1.Text = "&View";
            this.viewToolStripMenuItem1.DropDownOpening += new System.EventHandler(this.viewToolStripMenuItem1_DropDownOpening);
            // 
            // wireFrameToolStripMenuItem
            // 
            this.wireFrameToolStripMenuItem.Name = "wireFrameToolStripMenuItem";
            this.wireFrameToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.wireFrameToolStripMenuItem.Text = "Wire Frame";
            this.wireFrameToolStripMenuItem.Click += new System.EventHandler(this.wireFrameToolStripMenuItem_Click);
            // 
            // displayTerrainToolStripMenuItem
            // 
            this.displayTerrainToolStripMenuItem.Name = "displayTerrainToolStripMenuItem";
            this.displayTerrainToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.displayTerrainToolStripMenuItem.Text = "Display Terrain";
            this.displayTerrainToolStripMenuItem.Click += new System.EventHandler(this.displayTerrainToolStripMenuItem_Click);
            // 
            // showGroundPlaneToolStripMenuItem
            // 
            this.showGroundPlaneToolStripMenuItem.Name = "showGroundPlaneToolStripMenuItem";
            this.showGroundPlaneToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.showGroundPlaneToolStripMenuItem.Text = "Show Ground Plane";
            this.showGroundPlaneToolStripMenuItem.Click += new System.EventHandler(this.showGroundPlaneToolStripMenuItem_Click);
            // 
            // showCollisionVolumesToolStripMenuItem
            // 
            this.showCollisionVolumesToolStripMenuItem.CheckOnClick = true;
            this.showCollisionVolumesToolStripMenuItem.Name = "showCollisionVolumesToolStripMenuItem";
            this.showCollisionVolumesToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.showCollisionVolumesToolStripMenuItem.Text = "Show Collision Volumes";
            this.showCollisionVolumesToolStripMenuItem.Click += new System.EventHandler(this.showCollisionVolumesToolStripMenuItem_Click);
            // 
            // showBoundingBoxToolStripMenuItem
            // 
            this.showBoundingBoxToolStripMenuItem.Name = "showBoundingBoxToolStripMenuItem";
            this.showBoundingBoxToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.showBoundingBoxToolStripMenuItem.Text = "Show Bounding Box";
            this.showBoundingBoxToolStripMenuItem.Click += new System.EventHandler(this.showBoundingBoxToolStripMenuItem_Click);
            // 
            // spinCameraToolStripMenuItem
            // 
            this.spinCameraToolStripMenuItem.Name = "spinCameraToolStripMenuItem";
            this.spinCameraToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.spinCameraToolStripMenuItem.Text = "Spin Camera";
            this.spinCameraToolStripMenuItem.Click += new System.EventHandler(this.spinCameraToolStripMenuItem_Click);
            // 
            // spinLightToolStripMenuItem
            // 
            this.spinLightToolStripMenuItem.Name = "spinLightToolStripMenuItem";
            this.spinLightToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.spinLightToolStripMenuItem.Text = "Spin Light";
            this.spinLightToolStripMenuItem.Click += new System.EventHandler(this.spinLightToolStripMenuItem_Click);
            // 
            // displayBoneInformationToolStripMenuItem
            // 
            this.displayBoneInformationToolStripMenuItem.Name = "displayBoneInformationToolStripMenuItem";
            this.displayBoneInformationToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.displayBoneInformationToolStripMenuItem.Text = "Display Bone Information";
            this.displayBoneInformationToolStripMenuItem.Click += new System.EventHandler(this.displayBoneInformationToolStripMenuItem_Click);
            // 
            // setMaximumFramesPerSecondToolStripMenuItem
            // 
            this.setMaximumFramesPerSecondToolStripMenuItem.Name = "setMaximumFramesPerSecondToolStripMenuItem";
            this.setMaximumFramesPerSecondToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.setMaximumFramesPerSecondToolStripMenuItem.Text = "Set Maximum Frames Per Second";
            this.setMaximumFramesPerSecondToolStripMenuItem.Click += new System.EventHandler(this.setMaximumFramesPerSecondToolStripMenuItem_Click);
            // 
            // bgColorToolStripMenuItem
            // 
            this.bgColorToolStripMenuItem.Name = "bgColorToolStripMenuItem";
            this.bgColorToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.bgColorToolStripMenuItem.Text = "Set Background Color";
            this.bgColorToolStripMenuItem.Click += new System.EventHandler(this.bgColorToolStripMenuItem_Click);
            // 
            // materialSchemeToolStripMenuItem
            // 
            this.materialSchemeToolStripMenuItem.Enabled = false;
            this.materialSchemeToolStripMenuItem.Name = "materialSchemeToolStripMenuItem";
            this.materialSchemeToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.materialSchemeToolStripMenuItem.Text = "Material Scheme";
            this.materialSchemeToolStripMenuItem.Click += new System.EventHandler(this.materialSchemeToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripHelpItem,
            this.launchReleaseNotesToolStripMenuItem,
            this.submitABugReportToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // helpToolStripHelpItem
            // 
            this.helpToolStripHelpItem.Name = "helpToolStripHelpItem";
            this.helpToolStripHelpItem.Size = new System.Drawing.Size(198, 22);
            this.helpToolStripHelpItem.Text = "Launch Online Help";
            this.helpToolStripHelpItem.Click += new System.EventHandler(this.helpToolStripHelpItem_Click);
            // 
            // launchReleaseNotesToolStripMenuItem
            // 
            this.launchReleaseNotesToolStripMenuItem.Name = "launchReleaseNotesToolStripMenuItem";
            this.launchReleaseNotesToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.launchReleaseNotesToolStripMenuItem.Text = "Launch Release Notes";
            this.launchReleaseNotesToolStripMenuItem.Click += new System.EventHandler(this.launchReleaseNotesToolStripMenuItem_Click);
            // 
            // submitABugReportToolStripMenuItem
            // 
            this.submitABugReportToolStripMenuItem.Name = "submitABugReportToolStripMenuItem";
            this.submitABugReportToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.submitABugReportToolStripMenuItem.Text = "Submit Feedback or a Bug";
            this.submitABugReportToolStripMenuItem.Click += new System.EventHandler(this.submitABugReportToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.aboutToolStripMenuItem.Text = "About ModelViewer";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fpsStatusLabel,
            this.fpsStatusValueLabel,
            this.modelHeightLabel,
            this.modelHeightValueLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 719);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1016, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // fpsStatusLabel
            // 
            this.fpsStatusLabel.Name = "fpsStatusLabel";
            this.fpsStatusLabel.Size = new System.Drawing.Size(29, 17);
            this.fpsStatusLabel.Text = "FPS:";
            // 
            // fpsStatusValueLabel
            // 
            this.fpsStatusValueLabel.Name = "fpsStatusValueLabel";
            this.fpsStatusValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // modelHeightLabel
            // 
            this.modelHeightLabel.Name = "modelHeightLabel";
            this.modelHeightLabel.Size = new System.Drawing.Size(73, 17);
            this.modelHeightLabel.Text = "Model Height:";
            // 
            // modelHeightValueLabel
            // 
            this.modelHeightValueLabel.Name = "modelHeightValueLabel";
            this.modelHeightValueLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.spinCameraToolStripButton,
            this.mouseModeToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1016, 39);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // spinCameraToolStripButton
            // 
            this.spinCameraToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.spinCameraToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("spinCameraToolStripButton.Image")));
            this.spinCameraToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.spinCameraToolStripButton.Name = "spinCameraToolStripButton";
            this.spinCameraToolStripButton.Size = new System.Drawing.Size(36, 36);
            this.spinCameraToolStripButton.Text = "Spin Camera";
            this.spinCameraToolStripButton.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // mouseModeToolStripButton
            // 
            this.mouseModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mouseModeToolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveCameraToolStripMenuItem,
            this.moveDirectionalLightToolStripMenuItem});
            this.mouseModeToolStripButton.Image = global::ModelViewer.Properties.Resources.camera;
            this.mouseModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mouseModeToolStripButton.Name = "mouseModeToolStripButton";
            this.mouseModeToolStripButton.Size = new System.Drawing.Size(48, 36);
            this.mouseModeToolStripButton.Text = "Mouse Mode";
            // 
            // moveCameraToolStripMenuItem
            // 
            this.moveCameraToolStripMenuItem.Image = global::ModelViewer.Properties.Resources.camera;
            this.moveCameraToolStripMenuItem.Name = "moveCameraToolStripMenuItem";
            this.moveCameraToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.moveCameraToolStripMenuItem.Text = "Move Camera";
            this.moveCameraToolStripMenuItem.Click += new System.EventHandler(this.moveCameraToolStripMenuItem_Click);
            // 
            // moveDirectionalLightToolStripMenuItem
            // 
            this.moveDirectionalLightToolStripMenuItem.Image = global::ModelViewer.Properties.Resources.lightbulb;
            this.moveDirectionalLightToolStripMenuItem.Name = "moveDirectionalLightToolStripMenuItem";
            this.moveDirectionalLightToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.moveDirectionalLightToolStripMenuItem.Text = "Move Directional Light";
            this.moveDirectionalLightToolStripMenuItem.Click += new System.EventHandler(this.moveDirectionalLightToolStripMenuItem_Click);
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            this.colorDialog1.FullOpen = true;
            // 
            // axiomPictureBox
            // 
            this.axiomPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.axiomPictureBox.Location = new System.Drawing.Point(251, 66);
            this.axiomPictureBox.Name = "axiomPictureBox";
            this.axiomPictureBox.Size = new System.Drawing.Size(765, 650);
            this.axiomPictureBox.TabIndex = 2;
            this.axiomPictureBox.TabStop = false;
            // 
            // bonesPage
            // 
            this.bonesPage.Controls.Add(this.showBonesGroupBox);
            this.bonesPage.Controls.Add(this.bonesLinkLabel);
            this.bonesPage.Controls.Add(this.bonesTreeView);
            this.bonesPage.Location = new System.Drawing.Point(4, 40);
            this.bonesPage.Name = "bonesPage";
            this.bonesPage.Padding = new System.Windows.Forms.Padding(3);
            this.bonesPage.Size = new System.Drawing.Size(237, 606);
            this.bonesPage.TabIndex = 5;
            this.bonesPage.Text = "Bones";
            this.bonesPage.UseVisualStyleBackColor = true;
            // 
            // showBonesGroupBox
            // 
            this.showBonesGroupBox.Controls.Add(this.boneAxisSizeTrackBar);
            this.showBonesGroupBox.Controls.Add(this.showBonesCheckBox);
            this.showBonesGroupBox.Controls.Add(this.boneAxisSizeLabel);
            this.showBonesGroupBox.Location = new System.Drawing.Point(6, 340);
            this.showBonesGroupBox.Name = "showBonesGroupBox";
            this.showBonesGroupBox.Size = new System.Drawing.Size(225, 118);
            this.showBonesGroupBox.TabIndex = 11;
            this.showBonesGroupBox.TabStop = false;
            // 
            // boneAxisSizeTrackBar
            // 
            this.boneAxisSizeTrackBar.BackColor = System.Drawing.SystemColors.Control;
            this.boneAxisSizeTrackBar.Location = new System.Drawing.Point(6, 67);
            this.boneAxisSizeTrackBar.Maximum = 20;
            this.boneAxisSizeTrackBar.Name = "boneAxisSizeTrackBar";
            this.boneAxisSizeTrackBar.Size = new System.Drawing.Size(211, 45);
            this.boneAxisSizeTrackBar.TabIndex = 9;
            this.boneAxisSizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.boneAxisSizeTrackBar.Scroll += new System.EventHandler(this.boneAxisSizeTrackBar_Scroll);
            // 
            // showBonesCheckBox
            // 
            this.showBonesCheckBox.AutoSize = true;
            this.showBonesCheckBox.Location = new System.Drawing.Point(6, 19);
            this.showBonesCheckBox.Name = "showBonesCheckBox";
            this.showBonesCheckBox.Size = new System.Drawing.Size(86, 17);
            this.showBonesCheckBox.TabIndex = 8;
            this.showBonesCheckBox.Text = "Show Bones";
            this.showBonesCheckBox.UseVisualStyleBackColor = true;
            this.showBonesCheckBox.CheckedChanged += new System.EventHandler(this.showBonesCheckBox_CheckedChanged);
            // 
            // boneAxisSizeLabel
            // 
            this.boneAxisSizeLabel.AutoSize = true;
            this.boneAxisSizeLabel.Location = new System.Drawing.Point(3, 51);
            this.boneAxisSizeLabel.Name = "boneAxisSizeLabel";
            this.boneAxisSizeLabel.Size = new System.Drawing.Size(77, 13);
            this.boneAxisSizeLabel.TabIndex = 10;
            this.boneAxisSizeLabel.Text = "Bone Axis Size";
            // 
            // bonesLinkLabel
            // 
            this.bonesLinkLabel.AutoSize = true;
            this.bonesLinkLabel.Location = new System.Drawing.Point(154, 575);
            this.bonesLinkLabel.Name = "bonesLinkLabel";
            this.bonesLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.bonesLinkLabel.TabIndex = 7;
            this.bonesLinkLabel.TabStop = true;
            this.bonesLinkLabel.Text = "What\'s This?";
            this.bonesLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.bonesLinkLabel_LinkClicked);
            // 
            // bonesTreeView
            // 
            this.bonesTreeView.Location = new System.Drawing.Point(0, 0);
            this.bonesTreeView.Name = "bonesTreeView";
            this.bonesTreeView.Size = new System.Drawing.Size(237, 334);
            this.bonesTreeView.TabIndex = 0;
            // 
            // cameraPage
            // 
            this.cameraPage.Controls.Add(this.flowLayoutPanel1);
            this.cameraPage.Location = new System.Drawing.Point(4, 40);
            this.cameraPage.Name = "cameraPage";
            this.cameraPage.Padding = new System.Windows.Forms.Padding(3);
            this.cameraPage.Size = new System.Drawing.Size(237, 606);
            this.cameraPage.TabIndex = 3;
            this.cameraPage.Text = "Camera";
            this.cameraPage.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cameraControlsButton);
            this.flowLayoutPanel1.Controls.Add(this.cameraFocusHeightGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.cameraNearDistanceGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.showNormalsGroupBox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 6);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(234, 433);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // cameraControlsButton
            // 
            this.cameraControlsButton.Image = ((System.Drawing.Image)(resources.GetObject("cameraControlsButton.Image")));
            this.cameraControlsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cameraControlsButton.Location = new System.Drawing.Point(3, 3);
            this.cameraControlsButton.Name = "cameraControlsButton";
            this.cameraControlsButton.Size = new System.Drawing.Size(228, 26);
            this.cameraControlsButton.TabIndex = 0;
            this.cameraControlsButton.Text = "Camera Controls";
            this.cameraControlsButton.UseVisualStyleBackColor = true;
            this.cameraControlsButton.Click += new System.EventHandler(this.cameraControlsButton_Click);
            // 
            // cameraFocusHeightGroupBox
            // 
            this.cameraFocusHeightGroupBox.Controls.Add(this.cameraControlsLinkLabel);
            this.cameraFocusHeightGroupBox.Controls.Add(this.cameraFocusHeightTrackbar);
            this.cameraFocusHeightGroupBox.Controls.Add(this.label2);
            this.cameraFocusHeightGroupBox.Location = new System.Drawing.Point(3, 35);
            this.cameraFocusHeightGroupBox.Name = "cameraFocusHeightGroupBox";
            this.cameraFocusHeightGroupBox.Size = new System.Drawing.Size(228, 125);
            this.cameraFocusHeightGroupBox.TabIndex = 1;
            this.cameraFocusHeightGroupBox.TabStop = false;
            // 
            // cameraControlsLinkLabel
            // 
            this.cameraControlsLinkLabel.AutoSize = true;
            this.cameraControlsLinkLabel.Location = new System.Drawing.Point(40, 63);
            this.cameraControlsLinkLabel.Name = "cameraControlsLinkLabel";
            this.cameraControlsLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.cameraControlsLinkLabel.TabIndex = 2;
            this.cameraControlsLinkLabel.TabStop = true;
            this.cameraControlsLinkLabel.Text = "What\'s This?";
            this.cameraControlsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cameraControlsLinkLabel_LinkClicked);
            // 
            // cameraFocusHeightTrackbar
            // 
            this.cameraFocusHeightTrackbar.LargeChange = 10;
            this.cameraFocusHeightTrackbar.Location = new System.Drawing.Point(167, 19);
            this.cameraFocusHeightTrackbar.Maximum = 100;
            this.cameraFocusHeightTrackbar.Name = "cameraFocusHeightTrackbar";
            this.cameraFocusHeightTrackbar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.cameraFocusHeightTrackbar.Size = new System.Drawing.Size(45, 96);
            this.cameraFocusHeightTrackbar.TabIndex = 1;
            this.cameraFocusHeightTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.cameraFocusHeightTrackbar.Value = 50;
            this.cameraFocusHeightTrackbar.Scroll += new System.EventHandler(this.cameraFocusHeightTrackbar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Camera Focus Height:";
            // 
            // cameraNearDistanceGroupBox
            // 
            this.cameraNearDistanceGroupBox.Controls.Add(this.nearDistanceValueLabel);
            this.cameraNearDistanceGroupBox.Controls.Add(this.whatsNearDistanceLabel);
            this.cameraNearDistanceGroupBox.Controls.Add(this.cameraNearDistanceTrackBar);
            this.cameraNearDistanceGroupBox.Controls.Add(this.label3);
            this.cameraNearDistanceGroupBox.Location = new System.Drawing.Point(3, 166);
            this.cameraNearDistanceGroupBox.Name = "cameraNearDistanceGroupBox";
            this.cameraNearDistanceGroupBox.Size = new System.Drawing.Size(228, 125);
            this.cameraNearDistanceGroupBox.TabIndex = 2;
            this.cameraNearDistanceGroupBox.TabStop = false;
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
            this.whatsNearDistanceLabel.Location = new System.Drawing.Point(40, 63);
            this.whatsNearDistanceLabel.Name = "whatsNearDistanceLabel";
            this.whatsNearDistanceLabel.Size = new System.Drawing.Size(69, 13);
            this.whatsNearDistanceLabel.TabIndex = 2;
            this.whatsNearDistanceLabel.TabStop = true;
            this.whatsNearDistanceLabel.Text = "What\'s This?";
            this.whatsNearDistanceLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.whatsNearDistanceLabel_LinkClicked);
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
            // showNormalsGroupBox
            // 
            this.showNormalsGroupBox.Controls.Add(this.linkLabel1);
            this.showNormalsGroupBox.Controls.Add(this.showNormalsCheckBox);
            this.showNormalsGroupBox.Controls.Add(this.normalsAxisSizeLabel);
            this.showNormalsGroupBox.Controls.Add(this.normalsAxisSizeTrackBar);
            this.showNormalsGroupBox.Location = new System.Drawing.Point(3, 297);
            this.showNormalsGroupBox.Name = "showNormalsGroupBox";
            this.showNormalsGroupBox.Size = new System.Drawing.Size(228, 124);
            this.showNormalsGroupBox.TabIndex = 14;
            this.showNormalsGroupBox.TabStop = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(146, 20);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(69, 13);
            this.linkLabel1.TabIndex = 14;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "What\'s This?";
            // 
            // showNormalsCheckBox
            // 
            this.showNormalsCheckBox.AutoSize = true;
            this.showNormalsCheckBox.Location = new System.Drawing.Point(6, 19);
            this.showNormalsCheckBox.Name = "showNormalsCheckBox";
            this.showNormalsCheckBox.Size = new System.Drawing.Size(94, 17);
            this.showNormalsCheckBox.TabIndex = 12;
            this.showNormalsCheckBox.Text = "Show Normals";
            this.showNormalsCheckBox.UseVisualStyleBackColor = true;
            this.showNormalsCheckBox.CheckedChanged += new System.EventHandler(this.showNormalsCheckBox_CheckedChanged);
            // 
            // normalsAxisSizeLabel
            // 
            this.normalsAxisSizeLabel.AutoSize = true;
            this.normalsAxisSizeLabel.Location = new System.Drawing.Point(3, 51);
            this.normalsAxisSizeLabel.Name = "normalsAxisSizeLabel";
            this.normalsAxisSizeLabel.Size = new System.Drawing.Size(90, 13);
            this.normalsAxisSizeLabel.TabIndex = 13;
            this.normalsAxisSizeLabel.Text = "Normals Axis Size";
            // 
            // normalsAxisSizeTrackBar
            // 
            this.normalsAxisSizeTrackBar.BackColor = System.Drawing.SystemColors.Control;
            this.normalsAxisSizeTrackBar.Location = new System.Drawing.Point(6, 67);
            this.normalsAxisSizeTrackBar.Maximum = 20;
            this.normalsAxisSizeTrackBar.Name = "normalsAxisSizeTrackBar";
            this.normalsAxisSizeTrackBar.Size = new System.Drawing.Size(211, 45);
            this.normalsAxisSizeTrackBar.TabIndex = 10;
            this.normalsAxisSizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.normalsAxisSizeTrackBar.Scroll += new System.EventHandler(this.normalsAxisSizeTrackBar_Scroll);
            // 
            // lightingPage
            // 
            this.lightingPage.Controls.Add(this.lightingFlowLayoutPanel);
            this.lightingPage.Location = new System.Drawing.Point(4, 40);
            this.lightingPage.Name = "lightingPage";
            this.lightingPage.Size = new System.Drawing.Size(237, 606);
            this.lightingPage.TabIndex = 2;
            this.lightingPage.Text = "Lighting";
            this.lightingPage.UseVisualStyleBackColor = true;
            // 
            // lightingFlowLayoutPanel
            // 
            this.lightingFlowLayoutPanel.Controls.Add(this.ambientLightButton);
            this.lightingFlowLayoutPanel.Controls.Add(this.ambientLightGroupBox);
            this.lightingFlowLayoutPanel.Controls.Add(this.directionalLightButton);
            this.lightingFlowLayoutPanel.Controls.Add(this.directionalLightGroupBox);
            this.lightingFlowLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.lightingFlowLayoutPanel.Name = "lightingFlowLayoutPanel";
            this.lightingFlowLayoutPanel.Size = new System.Drawing.Size(231, 469);
            this.lightingFlowLayoutPanel.TabIndex = 2;
            // 
            // ambientLightButton
            // 
            this.ambientLightButton.Image = ((System.Drawing.Image)(resources.GetObject("ambientLightButton.Image")));
            this.ambientLightButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ambientLightButton.Location = new System.Drawing.Point(3, 3);
            this.ambientLightButton.Name = "ambientLightButton";
            this.ambientLightButton.Size = new System.Drawing.Size(228, 29);
            this.ambientLightButton.TabIndex = 1;
            this.ambientLightButton.Text = "Ambient Light";
            this.ambientLightButton.UseVisualStyleBackColor = true;
            this.ambientLightButton.Click += new System.EventHandler(this.ambientLightButton_Click);
            // 
            // ambientLightGroupBox
            // 
            this.ambientLightGroupBox.Controls.Add(this.ambientLightLinkLabel);
            this.ambientLightGroupBox.Controls.Add(this.ambientLightColorLabel);
            this.ambientLightGroupBox.Controls.Add(this.ambientLightColorButton);
            this.ambientLightGroupBox.Location = new System.Drawing.Point(3, 38);
            this.ambientLightGroupBox.Name = "ambientLightGroupBox";
            this.ambientLightGroupBox.Size = new System.Drawing.Size(228, 86);
            this.ambientLightGroupBox.TabIndex = 2;
            this.ambientLightGroupBox.TabStop = false;
            // 
            // ambientLightLinkLabel
            // 
            this.ambientLightLinkLabel.AutoSize = true;
            this.ambientLightLinkLabel.Location = new System.Drawing.Point(133, 60);
            this.ambientLightLinkLabel.Name = "ambientLightLinkLabel";
            this.ambientLightLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.ambientLightLinkLabel.TabIndex = 2;
            this.ambientLightLinkLabel.TabStop = true;
            this.ambientLightLinkLabel.Text = "What\'s This?";
            this.ambientLightLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ambientLightLinkLabel_LinkClicked);
            // 
            // ambientLightColorLabel
            // 
            this.ambientLightColorLabel.AutoSize = true;
            this.ambientLightColorLabel.Location = new System.Drawing.Point(24, 26);
            this.ambientLightColorLabel.Name = "ambientLightColorLabel";
            this.ambientLightColorLabel.Size = new System.Drawing.Size(101, 13);
            this.ambientLightColorLabel.TabIndex = 1;
            this.ambientLightColorLabel.Text = "Ambient Light Color:";
            // 
            // ambientLightColorButton
            // 
            this.ambientLightColorButton.Location = new System.Drawing.Point(158, 19);
            this.ambientLightColorButton.Name = "ambientLightColorButton";
            this.ambientLightColorButton.Size = new System.Drawing.Size(44, 26);
            this.ambientLightColorButton.TabIndex = 0;
            this.ambientLightColorButton.UseVisualStyleBackColor = true;
            this.ambientLightColorButton.Click += new System.EventHandler(this.ambientLightColorButton_Click);
            // 
            // directionalLightButton
            // 
            this.directionalLightButton.Image = ((System.Drawing.Image)(resources.GetObject("directionalLightButton.Image")));
            this.directionalLightButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.directionalLightButton.Location = new System.Drawing.Point(3, 130);
            this.directionalLightButton.Name = "directionalLightButton";
            this.directionalLightButton.Size = new System.Drawing.Size(228, 29);
            this.directionalLightButton.TabIndex = 3;
            this.directionalLightButton.Text = "Directional Light";
            this.directionalLightButton.UseVisualStyleBackColor = true;
            this.directionalLightButton.Click += new System.EventHandler(this.directionalLightButton_Click);
            // 
            // directionalLightGroupBox
            // 
            this.directionalLightGroupBox.Controls.Add(this.directionalLightLinkLabel);
            this.directionalLightGroupBox.Controls.Add(this.label1);
            this.directionalLightGroupBox.Controls.Add(this.lightZenithTrackBar);
            this.directionalLightGroupBox.Controls.Add(this.lightAzimuthTrackBar);
            this.directionalLightGroupBox.Controls.Add(this.specularColorButton);
            this.directionalLightGroupBox.Controls.Add(this.diffuseColorButton);
            this.directionalLightGroupBox.Controls.Add(this.specularColorLabel);
            this.directionalLightGroupBox.Controls.Add(this.diffuseColorLabel);
            this.directionalLightGroupBox.Location = new System.Drawing.Point(3, 165);
            this.directionalLightGroupBox.Name = "directionalLightGroupBox";
            this.directionalLightGroupBox.Size = new System.Drawing.Size(228, 287);
            this.directionalLightGroupBox.TabIndex = 4;
            this.directionalLightGroupBox.TabStop = false;
            // 
            // directionalLightLinkLabel
            // 
            this.directionalLightLinkLabel.AutoSize = true;
            this.directionalLightLinkLabel.Location = new System.Drawing.Point(133, 253);
            this.directionalLightLinkLabel.Name = "directionalLightLinkLabel";
            this.directionalLightLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.directionalLightLinkLabel.TabIndex = 7;
            this.directionalLightLinkLabel.TabStop = true;
            this.directionalLightLinkLabel.Text = "What\'s This?";
            this.directionalLightLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.directionalLightLinkLabel_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Light Direction:";
            // 
            // lightZenithTrackBar
            // 
            this.lightZenithTrackBar.Location = new System.Drawing.Point(162, 163);
            this.lightZenithTrackBar.Maximum = 90;
            this.lightZenithTrackBar.Minimum = -90;
            this.lightZenithTrackBar.Name = "lightZenithTrackBar";
            this.lightZenithTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.lightZenithTrackBar.Size = new System.Drawing.Size(45, 87);
            this.lightZenithTrackBar.TabIndex = 5;
            this.lightZenithTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.lightZenithTrackBar.Scroll += new System.EventHandler(this.lightZenithTrackBar_Scroll);
            // 
            // lightAzimuthTrackBar
            // 
            this.lightAzimuthTrackBar.Location = new System.Drawing.Point(27, 134);
            this.lightAzimuthTrackBar.Maximum = 180;
            this.lightAzimuthTrackBar.Minimum = -180;
            this.lightAzimuthTrackBar.Name = "lightAzimuthTrackBar";
            this.lightAzimuthTrackBar.Size = new System.Drawing.Size(129, 45);
            this.lightAzimuthTrackBar.TabIndex = 4;
            this.lightAzimuthTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.lightAzimuthTrackBar.Scroll += new System.EventHandler(this.lightAzimuthTrackBar_Scroll);
            // 
            // specularColorButton
            // 
            this.specularColorButton.Location = new System.Drawing.Point(158, 61);
            this.specularColorButton.Name = "specularColorButton";
            this.specularColorButton.Size = new System.Drawing.Size(44, 26);
            this.specularColorButton.TabIndex = 3;
            this.specularColorButton.UseVisualStyleBackColor = true;
            this.specularColorButton.Click += new System.EventHandler(this.specularColorButton_Click);
            // 
            // diffuseColorButton
            // 
            this.diffuseColorButton.Location = new System.Drawing.Point(158, 19);
            this.diffuseColorButton.Name = "diffuseColorButton";
            this.diffuseColorButton.Size = new System.Drawing.Size(44, 26);
            this.diffuseColorButton.TabIndex = 2;
            this.diffuseColorButton.UseVisualStyleBackColor = true;
            this.diffuseColorButton.Click += new System.EventHandler(this.diffuseColorButton_Click);
            // 
            // specularColorLabel
            // 
            this.specularColorLabel.AutoSize = true;
            this.specularColorLabel.Location = new System.Drawing.Point(24, 68);
            this.specularColorLabel.Name = "specularColorLabel";
            this.specularColorLabel.Size = new System.Drawing.Size(76, 13);
            this.specularColorLabel.TabIndex = 1;
            this.specularColorLabel.Text = "Specular Color";
            // 
            // diffuseColorLabel
            // 
            this.diffuseColorLabel.AutoSize = true;
            this.diffuseColorLabel.Location = new System.Drawing.Point(24, 26);
            this.diffuseColorLabel.Name = "diffuseColorLabel";
            this.diffuseColorLabel.Size = new System.Drawing.Size(70, 13);
            this.diffuseColorLabel.TabIndex = 0;
            this.diffuseColorLabel.Text = "Diffuse Color:";
            // 
            // socketsPage
            // 
            this.socketsPage.Controls.Add(this.showSocketGroupBox);
            this.socketsPage.Controls.Add(this.socketsLinkLabel);
            this.socketsPage.Controls.Add(this.parentBoneValueLabel);
            this.socketsPage.Controls.Add(this.parentBoneLabel);
            this.socketsPage.Controls.Add(this.socketListBox);
            this.socketsPage.Location = new System.Drawing.Point(4, 40);
            this.socketsPage.Name = "socketsPage";
            this.socketsPage.Padding = new System.Windows.Forms.Padding(3);
            this.socketsPage.Size = new System.Drawing.Size(237, 606);
            this.socketsPage.TabIndex = 4;
            this.socketsPage.Text = "Sockets";
            this.socketsPage.UseVisualStyleBackColor = true;
            // 
            // showSocketGroupBox
            // 
            this.showSocketGroupBox.Controls.Add(this.socketAxisSizeTrackBar);
            this.showSocketGroupBox.Controls.Add(this.socketAxisSizeLabel);
            this.showSocketGroupBox.Controls.Add(this.showSocketsCheckBox);
            this.showSocketGroupBox.Location = new System.Drawing.Point(6, 340);
            this.showSocketGroupBox.Name = "showSocketGroupBox";
            this.showSocketGroupBox.Size = new System.Drawing.Size(225, 118);
            this.showSocketGroupBox.TabIndex = 12;
            this.showSocketGroupBox.TabStop = false;
            // 
            // socketAxisSizeTrackBar
            // 
            this.socketAxisSizeTrackBar.Location = new System.Drawing.Point(6, 67);
            this.socketAxisSizeTrackBar.Maximum = 20;
            this.socketAxisSizeTrackBar.Name = "socketAxisSizeTrackBar";
            this.socketAxisSizeTrackBar.Size = new System.Drawing.Size(211, 45);
            this.socketAxisSizeTrackBar.TabIndex = 11;
            this.socketAxisSizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.socketAxisSizeTrackBar.Scroll += new System.EventHandler(this.socketAxisSizeTrackBar_Scroll);
            // 
            // socketAxisSizeLabel
            // 
            this.socketAxisSizeLabel.AutoSize = true;
            this.socketAxisSizeLabel.Location = new System.Drawing.Point(3, 51);
            this.socketAxisSizeLabel.Name = "socketAxisSizeLabel";
            this.socketAxisSizeLabel.Size = new System.Drawing.Size(86, 13);
            this.socketAxisSizeLabel.TabIndex = 10;
            this.socketAxisSizeLabel.Text = "Socket Axis Size";
            // 
            // showSocketsCheckBox
            // 
            this.showSocketsCheckBox.AutoSize = true;
            this.showSocketsCheckBox.Location = new System.Drawing.Point(6, 19);
            this.showSocketsCheckBox.Name = "showSocketsCheckBox";
            this.showSocketsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.showSocketsCheckBox.TabIndex = 9;
            this.showSocketsCheckBox.Text = "Show Sockets";
            this.showSocketsCheckBox.UseVisualStyleBackColor = true;
            this.showSocketsCheckBox.CheckedChanged += new System.EventHandler(this.showSocketsCheckBox_CheckedChanged);
            // 
            // socketsLinkLabel
            // 
            this.socketsLinkLabel.AutoSize = true;
            this.socketsLinkLabel.Location = new System.Drawing.Point(154, 578);
            this.socketsLinkLabel.Name = "socketsLinkLabel";
            this.socketsLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.socketsLinkLabel.TabIndex = 8;
            this.socketsLinkLabel.TabStop = true;
            this.socketsLinkLabel.Text = "What\'s This?";
            this.socketsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.socketsLinkLabel_LinkClicked);
            // 
            // parentBoneValueLabel
            // 
            this.parentBoneValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.parentBoneValueLabel.AutoSize = true;
            this.parentBoneValueLabel.Location = new System.Drawing.Point(81, 481);
            this.parentBoneValueLabel.Name = "parentBoneValueLabel";
            this.parentBoneValueLabel.Size = new System.Drawing.Size(33, 13);
            this.parentBoneValueLabel.TabIndex = 6;
            this.parentBoneValueLabel.Text = "None";
            // 
            // parentBoneLabel
            // 
            this.parentBoneLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.parentBoneLabel.AutoSize = true;
            this.parentBoneLabel.Location = new System.Drawing.Point(6, 481);
            this.parentBoneLabel.Name = "parentBoneLabel";
            this.parentBoneLabel.Size = new System.Drawing.Size(69, 13);
            this.parentBoneLabel.TabIndex = 5;
            this.parentBoneLabel.Text = "Parent Bone:";
            // 
            // socketListBox
            // 
            this.socketListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.socketListBox.FormattingEnabled = true;
            this.socketListBox.Location = new System.Drawing.Point(0, 0);
            this.socketListBox.Name = "socketListBox";
            this.socketListBox.Size = new System.Drawing.Size(237, 334);
            this.socketListBox.TabIndex = 0;
            // 
            // animPage
            // 
            this.animPage.Controls.Add(this.timeCurrentLabel);
            this.animPage.Controls.Add(this.timeEndLabel);
            this.animPage.Controls.Add(this.timeStartLabel);
            this.animPage.Controls.Add(this.animationTrackBar);
            this.animPage.Controls.Add(this.animLinkLabel);
            this.animPage.Controls.Add(this.animationSpeedTextBox);
            this.animPage.Controls.Add(this.animationSpeedLabel);
            this.animPage.Controls.Add(this.loopingCheckBox);
            this.animPage.Controls.Add(this.playStopButton);
            this.animPage.Controls.Add(this.animationListBox);
            this.animPage.Location = new System.Drawing.Point(4, 40);
            this.animPage.Name = "animPage";
            this.animPage.Padding = new System.Windows.Forms.Padding(3);
            this.animPage.Size = new System.Drawing.Size(237, 606);
            this.animPage.TabIndex = 1;
            this.animPage.Text = "Animations";
            this.animPage.UseVisualStyleBackColor = true;
            // 
            // timeCurrentLabel
            // 
            this.timeCurrentLabel.AutoSize = true;
            this.timeCurrentLabel.Location = new System.Drawing.Point(100, 471);
            this.timeCurrentLabel.Name = "timeCurrentLabel";
            this.timeCurrentLabel.Size = new System.Drawing.Size(22, 13);
            this.timeCurrentLabel.TabIndex = 10;
            this.timeCurrentLabel.Text = "0.5";
            // 
            // timeEndLabel
            // 
            this.timeEndLabel.AutoSize = true;
            this.timeEndLabel.Location = new System.Drawing.Point(186, 471);
            this.timeEndLabel.Name = "timeEndLabel";
            this.timeEndLabel.Size = new System.Drawing.Size(22, 13);
            this.timeEndLabel.TabIndex = 9;
            this.timeEndLabel.Text = "1.0";
            // 
            // timeStartLabel
            // 
            this.timeStartLabel.AutoSize = true;
            this.timeStartLabel.Location = new System.Drawing.Point(15, 471);
            this.timeStartLabel.Name = "timeStartLabel";
            this.timeStartLabel.Size = new System.Drawing.Size(22, 13);
            this.timeStartLabel.TabIndex = 8;
            this.timeStartLabel.Text = "0.0";
            // 
            // animationTrackBar
            // 
            this.animationTrackBar.Location = new System.Drawing.Point(18, 488);
            this.animationTrackBar.Name = "animationTrackBar";
            this.animationTrackBar.Size = new System.Drawing.Size(203, 45);
            this.animationTrackBar.TabIndex = 7;
            // 
            // animLinkLabel
            // 
            this.animLinkLabel.AutoSize = true;
            this.animLinkLabel.Location = new System.Drawing.Point(152, 544);
            this.animLinkLabel.Name = "animLinkLabel";
            this.animLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.animLinkLabel.TabIndex = 6;
            this.animLinkLabel.TabStop = true;
            this.animLinkLabel.Text = "What\'s This?";
            this.animLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.animLinkLabel_LinkClicked);
            // 
            // animationSpeedTextBox
            // 
            this.animationSpeedTextBox.Location = new System.Drawing.Point(121, 393);
            this.animationSpeedTextBox.Name = "animationSpeedTextBox";
            this.animationSpeedTextBox.Size = new System.Drawing.Size(100, 20);
            this.animationSpeedTextBox.TabIndex = 4;
            this.animationSpeedTextBox.Text = "1.0";
            // 
            // animationSpeedLabel
            // 
            this.animationSpeedLabel.AutoSize = true;
            this.animationSpeedLabel.Location = new System.Drawing.Point(15, 396);
            this.animationSpeedLabel.Name = "animationSpeedLabel";
            this.animationSpeedLabel.Size = new System.Drawing.Size(90, 13);
            this.animationSpeedLabel.TabIndex = 3;
            this.animationSpeedLabel.Text = "Animation Speed:";
            // 
            // loopingCheckBox
            // 
            this.loopingCheckBox.AutoSize = true;
            this.loopingCheckBox.Location = new System.Drawing.Point(18, 364);
            this.loopingCheckBox.Name = "loopingCheckBox";
            this.loopingCheckBox.Size = new System.Drawing.Size(99, 17);
            this.loopingCheckBox.TabIndex = 2;
            this.loopingCheckBox.Text = "Loop Animation";
            this.loopingCheckBox.UseVisualStyleBackColor = true;
            // 
            // playStopButton
            // 
            this.playStopButton.Location = new System.Drawing.Point(18, 431);
            this.playStopButton.Name = "playStopButton";
            this.playStopButton.Size = new System.Drawing.Size(75, 23);
            this.playStopButton.TabIndex = 1;
            this.playStopButton.Text = "Play";
            this.playStopButton.UseVisualStyleBackColor = true;
            this.playStopButton.Click += new System.EventHandler(this.playPauseButton_Click);
            // 
            // animationListBox
            // 
            this.animationListBox.FormattingEnabled = true;
            this.animationListBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.animationListBox.Location = new System.Drawing.Point(0, 0);
            this.animationListBox.Name = "animationListBox";
            this.animationListBox.Size = new System.Drawing.Size(237, 329);
            this.animationListBox.TabIndex = 0;
            this.animationListBox.SelectedIndexChanged += new System.EventHandler(this.animationListBox_SelectedIndexChanged);
            // 
            // subMeshPage
            // 
            this.subMeshPage.Controls.Add(this.subMeshTreeView);
            this.subMeshPage.Controls.Add(this.subMeshLinkLabel);
            this.subMeshPage.Controls.Add(this.materialLabel);
            this.subMeshPage.Controls.Add(this.materialTextBox);
            this.subMeshPage.Controls.Add(this.hideButton);
            this.subMeshPage.Controls.Add(this.showButton);
            this.subMeshPage.Controls.Add(this.subMeshTextBox);
            this.subMeshPage.Controls.Add(this.hideShowLabel);
            this.subMeshPage.Controls.Add(this.hideAllButton);
            this.subMeshPage.Controls.Add(this.showAllButton);
            this.subMeshPage.Location = new System.Drawing.Point(4, 40);
            this.subMeshPage.Name = "subMeshPage";
            this.subMeshPage.Padding = new System.Windows.Forms.Padding(3);
            this.subMeshPage.Size = new System.Drawing.Size(237, 606);
            this.subMeshPage.TabIndex = 0;
            this.subMeshPage.Text = "Sub Meshes";
            this.subMeshPage.UseVisualStyleBackColor = true;
            // 
            // subMeshTreeView
            // 
            this.subMeshTreeView.CheckBoxes = true;
            this.subMeshTreeView.Location = new System.Drawing.Point(0, 0);
            this.subMeshTreeView.Name = "subMeshTreeView";
            this.subMeshTreeView.Size = new System.Drawing.Size(237, 329);
            this.subMeshTreeView.TabIndex = 10;
            this.subMeshTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.subMeshTreeView_AfterSelect);
            // 
            // subMeshLinkLabel
            // 
            this.subMeshLinkLabel.AutoSize = true;
            this.subMeshLinkLabel.Location = new System.Drawing.Point(145, 566);
            this.subMeshLinkLabel.Name = "subMeshLinkLabel";
            this.subMeshLinkLabel.Size = new System.Drawing.Size(69, 13);
            this.subMeshLinkLabel.TabIndex = 9;
            this.subMeshLinkLabel.TabStop = true;
            this.subMeshLinkLabel.Text = "What\'s This?";
            this.subMeshLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.subMeshLinkLabel_LinkClicked);
            // 
            // materialLabel
            // 
            this.materialLabel.AutoSize = true;
            this.materialLabel.Location = new System.Drawing.Point(21, 504);
            this.materialLabel.Name = "materialLabel";
            this.materialLabel.Size = new System.Drawing.Size(44, 13);
            this.materialLabel.TabIndex = 8;
            this.materialLabel.Text = "Material";
            // 
            // materialTextBox
            // 
            this.materialTextBox.Location = new System.Drawing.Point(24, 530);
            this.materialTextBox.Name = "materialTextBox";
            this.materialTextBox.Size = new System.Drawing.Size(188, 20);
            this.materialTextBox.TabIndex = 7;
            this.materialTextBox.TextChanged += new System.EventHandler(this.materialTextBox_TextChanged);
            // 
            // hideButton
            // 
            this.hideButton.Enabled = false;
            this.hideButton.Location = new System.Drawing.Point(134, 464);
            this.hideButton.Name = "hideButton";
            this.hideButton.Size = new System.Drawing.Size(80, 30);
            this.hideButton.TabIndex = 6;
            this.hideButton.Text = "Hide";
            this.hideButton.UseVisualStyleBackColor = true;
            this.hideButton.Click += new System.EventHandler(this.hideButton_Click);
            // 
            // showButton
            // 
            this.showButton.Enabled = false;
            this.showButton.Location = new System.Drawing.Point(25, 462);
            this.showButton.Name = "showButton";
            this.showButton.Size = new System.Drawing.Size(80, 30);
            this.showButton.TabIndex = 5;
            this.showButton.Text = "Show";
            this.showButton.UseVisualStyleBackColor = true;
            this.showButton.Click += new System.EventHandler(this.showButton_Click);
            // 
            // subMeshTextBox
            // 
            this.subMeshTextBox.Location = new System.Drawing.Point(24, 424);
            this.subMeshTextBox.Name = "subMeshTextBox";
            this.subMeshTextBox.Size = new System.Drawing.Size(188, 20);
            this.subMeshTextBox.TabIndex = 4;
            this.subMeshTextBox.TextChanged += new System.EventHandler(this.subMeshTextBox_TextChanged);
            // 
            // hideShowLabel
            // 
            this.hideShowLabel.AutoSize = true;
            this.hideShowLabel.Location = new System.Drawing.Point(23, 392);
            this.hideShowLabel.Name = "hideShowLabel";
            this.hideShowLabel.Size = new System.Drawing.Size(179, 13);
            this.hideShowLabel.TabIndex = 3;
            this.hideShowLabel.Text = "Hide/Show Sub Meshes Containing:";
            // 
            // hideAllButton
            // 
            this.hideAllButton.Location = new System.Drawing.Point(132, 347);
            this.hideAllButton.Name = "hideAllButton";
            this.hideAllButton.Size = new System.Drawing.Size(80, 30);
            this.hideAllButton.TabIndex = 2;
            this.hideAllButton.Text = "Hide All";
            this.hideAllButton.UseVisualStyleBackColor = true;
            this.hideAllButton.Click += new System.EventHandler(this.hideAllButton_Click);
            // 
            // showAllButton
            // 
            this.showAllButton.Location = new System.Drawing.Point(24, 347);
            this.showAllButton.Name = "showAllButton";
            this.showAllButton.Size = new System.Drawing.Size(80, 30);
            this.showAllButton.TabIndex = 1;
            this.showAllButton.Text = "Show All";
            this.showAllButton.UseVisualStyleBackColor = true;
            this.showAllButton.Click += new System.EventHandler(this.showAllButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.subMeshPage);
            this.tabControl1.Controls.Add(this.animPage);
            this.tabControl1.Controls.Add(this.bonesPage);
            this.tabControl1.Controls.Add(this.socketsPage);
            this.tabControl1.Controls.Add(this.lightingPage);
            this.tabControl1.Controls.Add(this.cameraPage);
            this.tabControl1.Location = new System.Drawing.Point(0, 66);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(245, 650);
            this.tabControl1.TabIndex = 4;
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.axiomPictureBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ModelViewer";
            this.Text = "ModelViewer";
            this.Resize += new System.EventHandler(this.ModelViewer_Resize);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModelViewer_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axiomPictureBox)).EndInit();
            this.bonesPage.ResumeLayout(false);
            this.bonesPage.PerformLayout();
            this.showBonesGroupBox.ResumeLayout(false);
            this.showBonesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boneAxisSizeTrackBar)).EndInit();
            this.cameraPage.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.cameraFocusHeightGroupBox.ResumeLayout(false);
            this.cameraFocusHeightGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraFocusHeightTrackbar)).EndInit();
            this.cameraNearDistanceGroupBox.ResumeLayout(false);
            this.cameraNearDistanceGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraNearDistanceTrackBar)).EndInit();
            this.showNormalsGroupBox.ResumeLayout(false);
            this.showNormalsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalsAxisSizeTrackBar)).EndInit();
            this.lightingPage.ResumeLayout(false);
            this.lightingFlowLayoutPanel.ResumeLayout(false);
            this.ambientLightGroupBox.ResumeLayout(false);
            this.ambientLightGroupBox.PerformLayout();
            this.directionalLightGroupBox.ResumeLayout(false);
            this.directionalLightGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightZenithTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightAzimuthTrackBar)).EndInit();
            this.socketsPage.ResumeLayout(false);
            this.socketsPage.PerformLayout();
            this.showSocketGroupBox.ResumeLayout(false);
            this.showSocketGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.socketAxisSizeTrackBar)).EndInit();
            this.animPage.ResumeLayout(false);
            this.animPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationTrackBar)).EndInit();
            this.subMeshPage.ResumeLayout(false);
            this.subMeshPage.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.PictureBox axiomPictureBox;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayTerrainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spinCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton spinCameraToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem loadModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton mouseModeToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem moveCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDirectionalLightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spinLightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBoundingBoxToolStripMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripHelpItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel fpsStatusValueLabel;
        private System.Windows.Forms.ToolStripMenuItem submitABugReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel fpsStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel modelHeightLabel;
        private System.Windows.Forms.ToolStripStatusLabel modelHeightValueLabel;
        private System.Windows.Forms.ToolStripMenuItem designateRepositoryMenuItem;

        private System.Windows.Forms.ToolStripMenuItem launchReleaseNotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCollisionVolumesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayBoneInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setMaximumFramesPerSecondToolStripMenuItem;
        private System.Windows.Forms.TabPage bonesPage;
        private System.Windows.Forms.LinkLabel bonesLinkLabel;
        private System.Windows.Forms.TreeView bonesTreeView;
        private System.Windows.Forms.TabPage cameraPage;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cameraControlsButton;
        private System.Windows.Forms.GroupBox cameraFocusHeightGroupBox;
        private System.Windows.Forms.LinkLabel cameraControlsLinkLabel;
        private System.Windows.Forms.TrackBar cameraFocusHeightTrackbar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox cameraNearDistanceGroupBox;
        private System.Windows.Forms.Label nearDistanceValueLabel;
        private System.Windows.Forms.LinkLabel whatsNearDistanceLabel;
        private System.Windows.Forms.TrackBar cameraNearDistanceTrackBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage lightingPage;
        private System.Windows.Forms.FlowLayoutPanel lightingFlowLayoutPanel;
        private System.Windows.Forms.Button ambientLightButton;
        private System.Windows.Forms.GroupBox ambientLightGroupBox;
        private System.Windows.Forms.LinkLabel ambientLightLinkLabel;
        private System.Windows.Forms.Label ambientLightColorLabel;
        private System.Windows.Forms.Button ambientLightColorButton;
        private System.Windows.Forms.Button directionalLightButton;
        private System.Windows.Forms.GroupBox directionalLightGroupBox;
        private System.Windows.Forms.LinkLabel directionalLightLinkLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar lightZenithTrackBar;
        private System.Windows.Forms.TrackBar lightAzimuthTrackBar;
        private System.Windows.Forms.Button specularColorButton;
        private System.Windows.Forms.Button diffuseColorButton;
        private System.Windows.Forms.Label specularColorLabel;
        private System.Windows.Forms.Label diffuseColorLabel;
        private System.Windows.Forms.TabPage socketsPage;
        private System.Windows.Forms.LinkLabel socketsLinkLabel;
        private System.Windows.Forms.Label parentBoneValueLabel;
        private System.Windows.Forms.Label parentBoneLabel;
        private System.Windows.Forms.CheckedListBox socketListBox;
        private System.Windows.Forms.TabPage animPage;
        private System.Windows.Forms.Label timeCurrentLabel;
        private System.Windows.Forms.Label timeEndLabel;
        private System.Windows.Forms.Label timeStartLabel;
        private System.Windows.Forms.TrackBar animationTrackBar;
        private System.Windows.Forms.LinkLabel animLinkLabel;
        private System.Windows.Forms.TextBox animationSpeedTextBox;
        private System.Windows.Forms.Label animationSpeedLabel;
        private System.Windows.Forms.CheckBox loopingCheckBox;
        private System.Windows.Forms.Button playStopButton;
        private System.Windows.Forms.ListBox animationListBox;
        private System.Windows.Forms.TabPage subMeshPage;
        private System.Windows.Forms.TreeView subMeshTreeView;
        private System.Windows.Forms.LinkLabel subMeshLinkLabel;
        private System.Windows.Forms.Label materialLabel;
        private System.Windows.Forms.TextBox materialTextBox;
        private System.Windows.Forms.Button hideButton;
        private System.Windows.Forms.Button showButton;
        private System.Windows.Forms.TextBox subMeshTextBox;
        private System.Windows.Forms.Label hideShowLabel;
        private System.Windows.Forms.Button hideAllButton;
        private System.Windows.Forms.Button showAllButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolStripMenuItem bgColorToolStripMenuItem;
        private System.Windows.Forms.CheckBox showBonesCheckBox;
        private System.Windows.Forms.CheckBox showSocketsCheckBox;
        private System.Windows.Forms.Label boneAxisSizeLabel;
        private System.Windows.Forms.TrackBar boneAxisSizeTrackBar;
        private System.Windows.Forms.TrackBar socketAxisSizeTrackBar;
        private System.Windows.Forms.Label socketAxisSizeLabel;
        private System.Windows.Forms.GroupBox showSocketGroupBox;
        private System.Windows.Forms.GroupBox showBonesGroupBox;
        private System.Windows.Forms.ToolStripMenuItem showGroundPlaneToolStripMenuItem;
        private System.Windows.Forms.TrackBar normalsAxisSizeTrackBar;
        private System.Windows.Forms.ToolStripMenuItem materialSchemeToolStripMenuItem;
        private System.Windows.Forms.CheckBox showNormalsCheckBox;
        private System.Windows.Forms.Label normalsAxisSizeLabel;
        private System.Windows.Forms.GroupBox showNormalsGroupBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

