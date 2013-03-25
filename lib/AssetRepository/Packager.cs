using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Multiverse.AssetRepository;

namespace Multiverse.AssetRepository
{
    public partial class assetPackagerForm : Form
    {
		private string lastDirectoryReferenced = "";
        private bool inhibit = false;
        private List<string> worldAssetLines = new List<string>();
        private string baseHelpURL = "http://update.multiverse.net/wiki/index.php/Using_Asset_Packager_Version_1.5";
        private string baseReleaseNoteURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.5_Release_Notes";
        private string feedbackURL = "http://update.multiverse.net/custportal/login.php";
        

        public assetPackagerForm(string worldName)
        {
            inhibit = true;
            InitializeComponent();
			if (worldName != "") {
                worldNamePanel.Visible = true;
                worldNameTextBox.Text = worldName;
            }
            RepositoryClass.Instance.InitializeRepositoryPath();
            worldAssetsFilesLabel.Text = "World Assets Files";
            worldAssetLines = new List<string>();
            updateRepositoryPath();
			setEnables();
            inhibit = false;
        }

        // This is the entrypoint used by the World Editor
        public assetPackagerForm(List<string> worldAssetLines) 
        {
            this.worldAssetLines = worldAssetLines;
            inhibit = true;
            InitializeComponent();
            worldAssetsFilesLabel.Text = "Additional World Assets Files";
//             topPanel.Visible = false;
//             bottomPanel.Top = 0;
//             this.Height = this.Height - topPanel.Height;
//             bottomLeftButtonPanel.Visible = false;
			updateRepositoryPath();            
            setEnables();
            inhibit = false;
        }

        private void updateRepositoryPath()
		{
			if (RepositoryClass.Instance.RepositoryDirectoryListSet()) {
                // string s = "";
                List<string> allPaths = new List<string>(RepositoryClass.Instance.RepositoryDirectoryList);
                // Apply heuristic: look 1 directory up from the last
                // directory in the list, and if it exists, add it to
                // the end.  This gets us /Media/foo.assetlist
                if (allPaths.Count > 0) {
                    DirectoryInfo dir = Directory.GetParent(RepositoryClass.Instance.LastRepositoryPath);
                    if (dir.Exists)
                        allPaths.Add(dir.FullName);
                }
                foreach (string path in allPaths) {
                    assetListCheckedListBox.Items.Clear();
                    DirectoryInfo info = new DirectoryInfo(path);
                    FileInfo[] files = info.GetFiles("*.assetlist");
                    foreach (FileInfo file in files)
                        assetListCheckedListBox.Items.Add(file.FullName);
                }
                repositoryLabel.Text = "Repositories: " + RepositoryClass.Instance.RepositoryDirectoryListString;
            }
            else
                repositoryLabel.Text = "No Repository Designated";
		}

        private void designateSourceMediaTreeButton_Click(object sender, EventArgs e)
        {
            DesignateRepositoriesDialog designateRepositoriesDialog = new DesignateRepositoriesDialog();
            DialogResult result = designateRepositoriesDialog.ShowDialog();
            if (result == DialogResult.OK)
				updateRepositoryPath();
        }

		protected void CheckLogAndMaybeExit(List<string> log)
		{
            if (log.Count > 0) {
                string lines = "";
                foreach (string s in log)
                    lines += s + "\n";
                if (MessageBox.Show("Error(s) initializing asset repository:\n\n" + lines,
                                    "Errors Initializing Asset Repository.  Click Cancel To Exit",
                                    MessageBoxButtons.OK) == DialogResult.OK)
                    Environment.Exit(-1);
            }
		}
        
        private void setEnables()
		{
			copyMediaButton.Enabled = ((topPanel.Visible == false ||
                                        worldAssetsCheckedListBox.CheckedItems.Count > 0 || 
                                        assetListCheckedListBox.CheckedItems.Count != 0) &&
									   newTreeTextBox.Text != "");
			createEmptyRepositoryButton.Enabled = newTreeTextBox.Text != "";
		}
		
		private void openWorldAssetsButton_Click(object sender, EventArgs e)
        {
			if (openWorldAssetsDialog.ShowDialog() == DialogResult.OK) {
                foreach (string file in openWorldAssetsDialog.FileNames)
                    worldAssetsCheckedListBox.Items.Add(file, true);
                lastDirectoryReferenced = Path.GetDirectoryName(openWorldAssetsDialog.FileName);
				setEnables();
			}
        }

        private void assetListFileOpenButton_Click(object sender, EventArgs e)
        {
            if (openAssetListFileDialog.ShowDialog() == DialogResult.OK) {
                assetListCheckedListBox.Items.Add(openAssetListFileDialog.FileName);
                assetListCheckedListBox.SetItemChecked(assetListCheckedListBox.Items.Count - 1, true);
                lastDirectoryReferenced = Path.GetDirectoryName(openAssetListFileDialog.FileName);
                setEnables();
            }
        }

        private void newMediaTreeOpenButton_Click(object sender, EventArgs e)
        {
            string s = RepositoryClass.Instance.LastRepositoryPath;
            if (s != "")
                newMediaTreeDialog.SelectedPath = s;
			if (newMediaTreeDialog.ShowDialog() == DialogResult.OK) {
                newTreeTextBox.Text = newMediaTreeDialog.SelectedPath;
                lastDirectoryReferenced = newMediaTreeDialog.SelectedPath;
				setEnables();
			}
        }

        private class copyArgs {
            internal string worldName;
            internal List<string> worldAssetsFileLines;
            internal List<string> assetListFiles;
            internal string newTreeFile;
            internal bool copyDefinitions;
            internal copyArgs (string worldName, List<string> worldAssetsFileLines, List<string> assetListFiles,
							   string newTreeFile, bool copyDefinitions)
			{
				this.worldName = worldName;
                this.worldAssetsFileLines = worldAssetsFileLines;
				this.assetListFiles = assetListFiles;
				this.newTreeFile = newTreeFile;
				this.copyDefinitions = copyDefinitions;
			}
		}

        private void createEmptyRepositoryButton_Click(object sender, EventArgs e) {
            logListBox.Items.Clear();
			RepositoryClass.Instance.BuildMediaDirectoryStructure(newTreeTextBox.Text);
            logListBox.Items.Add("Created empty asset repository '" + newTreeTextBox.Text + "'");
        }

        private void copyMediaButton_Click(object sender, EventArgs e)
        {
			this.Cursor = Cursors.WaitCursor;
            List<string> repositoryDirectoryList = RepositoryClass.Instance.RepositoryDirectoryList;
            string worldName = worldNameTextBox.Text.Trim();
            if (worldName != "") {
                if (repositoryDirectoryList.Count == 0)
                    throw new Exception("Specified world name without a repository");
                repositoryDirectoryList.Insert(0, Path.Combine(repositoryDirectoryList[repositoryDirectoryList.Count - 1], worldName));
                // We should probably warn them if we have more than one repository
            }
            List<string> log =
                RepositoryClass.Instance.InitializeRepository(repositoryDirectoryList);
			CheckLogAndMaybeExit(log);
            List<string> assetListFiles = new List<string>();
            logListBox.Items.Clear();
            logListBox.Items.Add("Starting copy to repository " + newTreeTextBox.Text + "...");
			List<string> allWorldAssetsLines = new List<string>(worldAssetLines);
            if (worldAssetsCheckedListBox.CheckedItems.Count > 0) {
                foreach (Object item in worldAssetsCheckedListBox.CheckedItems)
                    allWorldAssetsLines.AddRange(RepositoryClass.ReadFileLines(((string)item).Trim()));
            }
            foreach (Object obj in assetListCheckedListBox.CheckedItems)
                assetListFiles.Add((string)obj);
            copyMediaButton.Enabled = false;
            createEmptyRepositoryButton.Enabled = false;
            cancelButton.Enabled = false;
            generateBackgroundWorker.RunWorkerAsync(new copyArgs(worldNameTextBox.Text, allWorldAssetsLines, assetListFiles,
																 newTreeTextBox.Text,
																 copyAssetDefinitionsCheckBox.Checked));
        }


        private void generateBackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            copyArgs args = (copyArgs)e.Argument;
			RepositoryClass.Instance.GenerateAndCopyMediaTree(args.worldAssetsFileLines, args.assetListFiles,
                                                              args.newTreeFile, args.copyDefinitions);
        }

        private void generateBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            List<string> log = RepositoryClass.Instance.ErrorLog;
            foreach (string s in log) {
                logListBox.Items.Add(s);
            }
            this.Cursor = Cursors.Default;
            copyMediaButton.Enabled = true;
            createEmptyRepositoryButton.Enabled = true;
            cancelButton.Enabled = true;
        }

        private void aboutButton_Click(object sender, EventArgs e) {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string msg = string.Format("Multiverse Asset Packager\n\nVersion: {0}\n\nCopyright 2006 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse Asset Packager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void assetPackagerForm_Resize(object sender, EventArgs e) {
            if (inhibit)
                return;
            topPanel.Width = this.Width;
            bottomPanel.Width = this.Width;
            bottomPanel.Height = this.Height - bottomPanel.Top - 60;
            logListBox.Width = this.Width - 60;
            logListBox.Height = this.Height - logListBox.Top - bottomPanel.Top - 70 - 45;
            int buttonWidth = openWorldAssetsButton.Width;
            int newWidth = this.Width - worldAssetsCheckedListBox.Left - buttonWidth - 47;
            worldAssetsCheckedListBox.Width = newWidth;
            newTreeTextBox.Width = newWidth;
            assetListCheckedListBox.Width = newWidth;
            int buttonLeft = this.Width - buttonWidth - 32;
            openWorldAssetsButton.Left = buttonLeft;
            assetListFileOpenButton.Left = buttonLeft;
            newMediaTreeOpenButton.Left = buttonLeft;
            int doitButtonTop = logListBox.Top + logListBox.Height + 1;
            bottomLeftButtonPanel.Top = doitButtonTop;
            bottomRightButtonPanel.Top = doitButtonTop;
            bottomRightButtonPanel.Left = logListBox.Right - bottomRightButtonPanel.Width;
            aboutButton.Left = buttonLeft;
        }

        private void LaunchProcess(string URL)
        {
            System.Diagnostics.Process.Start(URL);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void launchOnlineHelpToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            LaunchProcess(baseHelpURL);
        }

        private void submitFeedbackOrBugToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            LaunchProcess(feedbackURL);
        }

        private void releaseNotesToolStripMenuItem_clicked(object sender, EventArgs e)
        {
            LaunchProcess(baseReleaseNoteURL);
        }

        private void aboutAssetPackagerToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string msg = string.Format("Multiverse Asset Packager\n\nVersion: {0}\n\nCopyright 2006-2007 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse Asset Packager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void assetPackagerForm_Shown(object sender, EventArgs e)
        {
            assetPackagerForm_Resize(sender, e);
        }

    }
}