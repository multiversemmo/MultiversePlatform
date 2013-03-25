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

namespace MVImportTool
{
    partial class MVImportToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( MVImportToolForm ) );
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchOnlineHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.releaseNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.submitFeedbackOrABugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandFlagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutCOLLADAImportToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ColladaTabPage = new System.Windows.Forms.TabPage();
            this.sourceFileSelector1 = new MVImportTool.SourceFileSelector();
            this.RepositoriesTabPage = new System.Windows.Forms.TabPage();
            this.repositoryControl1 = new MVImportTool.RepositoryControl();
            this.ExecutionTabControl = new System.Windows.Forms.TabControl();
            this.CommandTabPage = new System.Windows.Forms.TabPage();
            this.commandControl1 = new MVImportTool.CommandControl();
            this.LogTabPage = new System.Windows.Forms.TabPage();
            this.logPanel1 = new MVImportTool.LogPanel();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.ColladaTabPage.SuspendLayout();
            this.RepositoriesTabPage.SuspendLayout();
            this.ExecutionTabControl.SuspendLayout();
            this.CommandTabPage.SuspendLayout();
            this.LogTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartButton.Location = new System.Drawing.Point( 575, 335 );
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size( 75, 23 );
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler( this.StartButton_Click );
            // 
            // StopButton
            // 
            this.StopButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StopButton.Location = new System.Drawing.Point( 656, 335 );
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size( 75, 23 );
            this.StopButton.TabIndex = 2;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler( this.StopButton_Click );
            // 
            // ExitButton
            // 
            this.ExitButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExitButton.Location = new System.Drawing.Point( 737, 335 );
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size( 75, 23 );
            this.ExitButton.TabIndex = 3;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler( this.ExitButton_Click );
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem} );
            this.menuStrip1.Location = new System.Drawing.Point( 0, 0 );
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size( 814, 24 );
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem} );
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size( 84, 20 );
            this.toolStripMenuItem1.Text = "Configuration";
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size( 132, 22 );
            this.configurationToolStripMenuItem.Text = "Configure";
            this.configurationToolStripMenuItem.Click += new System.EventHandler( this.configurationToolStripMenuItem_Click );
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.launchOnlineHelpToolStripMenuItem,
            this.releaseNotesToolStripMenuItem,
            this.submitFeedbackOrABugToolStripMenuItem,
            this.commandFlagsToolStripMenuItem,
            this.aboutCOLLADAImportToolToolStripMenuItem} );
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size( 40, 20 );
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // launchOnlineHelpToolStripMenuItem
            // 
            this.launchOnlineHelpToolStripMenuItem.Name = "launchOnlineHelpToolStripMenuItem";
            this.launchOnlineHelpToolStripMenuItem.Size = new System.Drawing.Size( 221, 22 );
            this.launchOnlineHelpToolStripMenuItem.Text = "Launch Online Help";
            this.launchOnlineHelpToolStripMenuItem.Click += new System.EventHandler( this.launchOnlineHelpToolStripMenuItem_Click );
            // 
            // releaseNotesToolStripMenuItem
            // 
            this.releaseNotesToolStripMenuItem.Name = "releaseNotesToolStripMenuItem";
            this.releaseNotesToolStripMenuItem.Size = new System.Drawing.Size( 221, 22 );
            this.releaseNotesToolStripMenuItem.Text = "Release Notes";
            this.releaseNotesToolStripMenuItem.Click += new System.EventHandler( this.releaseNotesToolStripMenuItem_Click );
            // 
            // submitFeedbackOrABugToolStripMenuItem
            // 
            this.submitFeedbackOrABugToolStripMenuItem.Name = "submitFeedbackOrABugToolStripMenuItem";
            this.submitFeedbackOrABugToolStripMenuItem.Size = new System.Drawing.Size( 221, 22 );
            this.submitFeedbackOrABugToolStripMenuItem.Text = "Submit Feedback or a Bug";
            this.submitFeedbackOrABugToolStripMenuItem.Click += new System.EventHandler( this.submitFeedbackOrABugToolStripMenuItem_Click );
            // 
            // commandFlagsToolStripMenuItem
            // 
            this.commandFlagsToolStripMenuItem.Name = "commandFlagsToolStripMenuItem";
            this.commandFlagsToolStripMenuItem.Size = new System.Drawing.Size( 221, 22 );
            this.commandFlagsToolStripMenuItem.Text = "Command Flags";
            this.commandFlagsToolStripMenuItem.Click += new System.EventHandler( this.commandFlagsToolStripMenuItem_Click );
            // 
            // aboutCOLLADAImportToolToolStripMenuItem
            // 
            this.aboutCOLLADAImportToolToolStripMenuItem.Name = "aboutCOLLADAImportToolToolStripMenuItem";
            this.aboutCOLLADAImportToolToolStripMenuItem.Size = new System.Drawing.Size( 221, 22 );
            this.aboutCOLLADAImportToolToolStripMenuItem.Text = "About COLLADA Import Tool";
            this.aboutCOLLADAImportToolToolStripMenuItem.Click += new System.EventHandler( this.aboutCOLLADAImportToolToolStripMenuItem_Click );
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point( 0, 23 );
            this.splitContainer1.MinimumSize = new System.Drawing.Size( 650, 310 );
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add( this.tabControl1 );
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add( this.ExecutionTabControl );
            this.splitContainer1.Panel2MinSize = 415;
            this.splitContainer1.Size = new System.Drawing.Size( 814, 310 );
            this.splitContainer1.SplitterDistance = 262;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add( this.ColladaTabPage );
            this.tabControl1.Controls.Add( this.RepositoriesTabPage );
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point( 0, 0 );
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.ShowToolTips = true;
            this.tabControl1.Size = new System.Drawing.Size( 258, 306 );
            this.tabControl1.TabIndex = 0;
            // 
            // ColladaTabPage
            // 
            this.ColladaTabPage.Controls.Add( this.sourceFileSelector1 );
            this.ColladaTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.ColladaTabPage.Name = "ColladaTabPage";
            this.ColladaTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.ColladaTabPage.Size = new System.Drawing.Size( 250, 280 );
            this.ColladaTabPage.TabIndex = 0;
            this.ColladaTabPage.Text = "COLLADA Files";
            this.ColladaTabPage.ToolTipText = "Set up which COLLADA files to import";
            this.ColladaTabPage.UseVisualStyleBackColor = true;
            // 
            // sourceFileSelector1
            // 
            this.sourceFileSelector1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceFileSelector1.FileFilter = "*.dae";
            this.sourceFileSelector1.Folder = "";
            this.sourceFileSelector1.Location = new System.Drawing.Point( 3, 3 );
            this.sourceFileSelector1.Name = "sourceFileSelector1";
            this.sourceFileSelector1.Size = new System.Drawing.Size( 244, 274 );
            this.sourceFileSelector1.TabIndex = 0;
            // 
            // RepositoriesTabPage
            // 
            this.RepositoriesTabPage.Controls.Add( this.repositoryControl1 );
            this.RepositoriesTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.RepositoriesTabPage.Name = "RepositoriesTabPage";
            this.RepositoriesTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.RepositoriesTabPage.Size = new System.Drawing.Size( 250, 280 );
            this.RepositoriesTabPage.TabIndex = 1;
            this.RepositoriesTabPage.Text = "Repositories";
            this.RepositoriesTabPage.ToolTipText = "Select the repositories into which you are importing files";
            this.RepositoriesTabPage.UseVisualStyleBackColor = true;
            // 
            // repositoryControl1
            // 
            this.repositoryControl1.CheckedRespositoryPaths = ((System.Collections.Generic.List<string>) (resources.GetObject( "repositoryControl1.CheckedRespositoryPaths" )));
            this.repositoryControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.repositoryControl1.Location = new System.Drawing.Point( 3, 3 );
            this.repositoryControl1.Name = "repositoryControl1";
            this.repositoryControl1.Size = new System.Drawing.Size( 244, 274 );
            this.repositoryControl1.TabIndex = 0;
            // 
            // ExecutionTabControl
            // 
            this.ExecutionTabControl.Controls.Add( this.CommandTabPage );
            this.ExecutionTabControl.Controls.Add( this.LogTabPage );
            this.ExecutionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExecutionTabControl.Location = new System.Drawing.Point( 0, 0 );
            this.ExecutionTabControl.Name = "ExecutionTabControl";
            this.ExecutionTabControl.SelectedIndex = 0;
            this.ExecutionTabControl.ShowToolTips = true;
            this.ExecutionTabControl.Size = new System.Drawing.Size( 544, 306 );
            this.ExecutionTabControl.TabIndex = 0;
            // 
            // CommandTabPage
            // 
            this.CommandTabPage.Controls.Add( this.commandControl1 );
            this.CommandTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.CommandTabPage.Name = "CommandTabPage";
            this.CommandTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.CommandTabPage.Size = new System.Drawing.Size( 536, 280 );
            this.CommandTabPage.TabIndex = 0;
            this.CommandTabPage.Text = "Command Controls";
            this.CommandTabPage.ToolTipText = "Controls for Conversion and Copying";
            this.CommandTabPage.UseVisualStyleBackColor = true;
            // 
            // commandControl1
            // 
            this.commandControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandControl1.Location = new System.Drawing.Point( 3, 3 );
            this.commandControl1.MinimumSize = new System.Drawing.Size( 210, 250 );
            this.commandControl1.Name = "commandControl1";
            this.commandControl1.Size = new System.Drawing.Size( 530, 274 );
            this.commandControl1.TabIndex = 0;
            // 
            // LogTabPage
            // 
            this.LogTabPage.Controls.Add( this.logPanel1 );
            this.LogTabPage.Location = new System.Drawing.Point( 4, 22 );
            this.LogTabPage.Name = "LogTabPage";
            this.LogTabPage.Padding = new System.Windows.Forms.Padding( 3 );
            this.LogTabPage.Size = new System.Drawing.Size( 536, 280 );
            this.LogTabPage.TabIndex = 1;
            this.LogTabPage.Text = "Execution Log";
            this.LogTabPage.ToolTipText = "Displays output from the Import commands";
            this.LogTabPage.UseVisualStyleBackColor = true;
            // 
            // logPanel1
            // 
            this.logPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logPanel1.Location = new System.Drawing.Point( 3, 3 );
            this.logPanel1.Name = "logPanel1";
            this.logPanel1.Size = new System.Drawing.Size( 530, 274 );
            this.logPanel1.TabIndex = 0;
            // 
            // MVImportToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 814, 361 );
            this.Controls.Add( this.ExitButton );
            this.Controls.Add( this.StopButton );
            this.Controls.Add( this.StartButton );
            this.Controls.Add( this.splitContainer1 );
            this.Controls.Add( this.menuStrip1 );
            this.Icon = ((System.Drawing.Icon) (resources.GetObject( "$this.Icon" )));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size( 690, 395 );
            this.Name = "MVImportToolForm";
            this.Text = "Multiverse COLLADA Import Tool";
            this.Load += new System.EventHandler( this.MVImportToolForm_Load );
            this.menuStrip1.ResumeLayout( false );
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout( false );
            this.splitContainer1.Panel2.ResumeLayout( false );
            this.splitContainer1.ResumeLayout( false );
            this.tabControl1.ResumeLayout( false );
            this.ColladaTabPage.ResumeLayout( false );
            this.RepositoriesTabPage.ResumeLayout( false );
            this.ExecutionTabControl.ResumeLayout( false );
            this.CommandTabPage.ResumeLayout( false );
            this.LogTabPage.ResumeLayout( false );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private CommandControl commandControl1;
        //        private ImporterConfiguration importerConfiguration1;
        private System.Windows.Forms.TabControl ExecutionTabControl;
        private System.Windows.Forms.TabPage CommandTabPage;
        private System.Windows.Forms.TabPage LogTabPage;
        private LogPanel logPanel1;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem launchOnlineHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem releaseNotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem submitFeedbackOrABugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commandFlagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutCOLLADAImportToolToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ColladaTabPage;
        private SourceFileSelector sourceFileSelector1;
        private System.Windows.Forms.TabPage RepositoriesTabPage;
        private RepositoryControl repositoryControl1;

    }
}

