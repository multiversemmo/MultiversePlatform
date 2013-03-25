using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Multiverse.AssetRepository
{
    // <summary>
    // This class defines a dialog that permits designation of an
    // ordered list of asset repository directories.  The dialog
    // itself handles all error messages and so on.  You can only exit
    // the dialog by providing a valid set of repositories, or by
    // cancelling.
    // 
    // The class below has two constructors: if you use the no-arg
    // constructor, then clicking OK on the dialog will set the
    // repository directory list in the registry.  If you pass the
    // repository list in, then clicking OK only modified that list.
    // The caller can pick up the modified list by getting the
    // RepositoryDirectoryList property.
    // </summary>
    public partial class DesignateRepositoriesDialog : Form
    {
        private List<string> errorLog = null;

        private List<string> repositoryDirectoryList = null;
        
        private int indexOfTargetItem = -1;
        
		private Rectangle dragBoxFromMouseDown;

        public DesignateRepositoriesDialog()
        {
            this.repositoryDirectoryList = null;
            InitializeComponent();
            InitRest();
        }

        public DesignateRepositoriesDialog(List<string> repositoryDirectoryList)
        {
            this.repositoryDirectoryList = repositoryDirectoryList;
            InitializeComponent();
            InitRest();
        }

        private void InitRest() 
        {
            FilesListBox.Items.Clear();
            List<string> directories = (repositoryDirectoryList != null ? repositoryDirectoryList : 
                                        new List<string>(RepositoryClass.Instance.RepositoryDirectoryList));
            foreach (string dir in directories)
                FilesListBox.Items.Add(dir);
            if (directories.Count > 0)
                FilesListBox.SelectedIndex = 0;
        }
        
        private void BrowseAndAddButton_Click(object sender, EventArgs e)
        {
            if (RepositoryFolderBrowserDialog.ShowDialog() == DialogResult.OK) {
                FilesListBox.Items.Add(RepositoryFolderBrowserDialog.SelectedPath);
                FilesListBox.SelectedItem = RepositoryFolderBrowserDialog.SelectedPath;
            }
        }

        private void RemoveDirectory_Click(object sender, EventArgs e)
        {
            int index = FilesListBox.SelectedIndex;
            FilesListBox.Items.Remove(FilesListBox.SelectedItem);
            int count = FilesListBox.Items.Count;
            if (count > 0) {
                index = Math.Max(count - 1, Math.Min(0, index));
                FilesListBox.SelectedIndex = index;
            }
        }

        private void FilesListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            RemoveDirectory.Enabled = FilesListBox.SelectedIndices.Count > 0;
        }

        // This only returns when you have a valid repository.
        // Otherwise, the user is forced to hit Cancel.
        private void OKButton_Click(object sender, EventArgs e)
        {
            List<string> directories = new List<string>();
            foreach (string item in FilesListBox.Items) {
                if (item != "")
                    directories.Add(item.Trim());
            }
            if (directories.Count > 0) {
                List<string> errorLog = RepositoryClass.Instance.CheckForValidRepository(directories);

                if (errorLog.Count > 0)
                {
                    ErrorLogPopup(errorLog, "The folder you selected does not contain a valid respository.  The following errors were generated:\n\n",
                        "Invalid Repository", MessageBoxButtons.OK);
                    this.errorLog = errorLog;
                }
                else
                {
                    if (repositoryDirectoryList != null)
                        repositoryDirectoryList = new List<string>(directories);
                    else
                        RepositoryClass.Instance.SetRepositoryDirectoriesInRegistry(directories);
                    this.DialogResult = DialogResult.OK;
                }
            }
            else {
                ErrorLogPopup(null, "There are no directories designated.  Without an asset repository, the game engine won't be able to find your models, meshes, and textures.", "No Repository Directories Designated", MessageBoxButtons.OK);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
#if false // XXXMLM - not an error to cancel your changes
            string s = RepositoryClass.Instance.RepositoryDirectoryListSet() ? 
                "No changes were mades in asset repositories\nCurrent repositories: " + RepositoryClass.Instance.RepositoryDirectoryListString :
                "No asset repositories are designated";
            ErrorLogPopup(null, s, "Cancelled Setting Repositories", MessageBoxButtons.OK);
#endif
            this.DialogResult = DialogResult.Cancel;
        }
        
        public List<string> ErrorLog {
            get
            {
                return ErrorLog;
            }
        }

        protected DialogResult ErrorLogPopup(List<string> log, string message, string title, MessageBoxButtons buttons)
        {
            string lines = message;
            if (log != null && log.Count > 0)
            {
                foreach (string s in log)
                {
                    lines += s + "\n";
                }
            }
            return MessageBox.Show(lines, title, buttons, MessageBoxIcon.Error);

            // return DialogResult.OK;
        }

        public List<string> RepositoryDirectoryList 
        {
            get {
                return repositoryDirectoryList;
            }
        }

        private void FilesListBox_DragOver(object sender, DragEventArgs e)
        {
			ListBox targetListBox = (ListBox)sender;
			if (!e.Data.GetDataPresent(typeof(System.String))) {
					e.Effect = DragDropEffects.None;
					return;
			}
			string value = (string)e.Data.GetData(DataFormats.StringFormat);
			e.Effect = DragDropEffects.Move;
			indexOfTargetItem = targetListBox.IndexFromPoint(targetListBox.PointToClient(new Point(e.X, e.Y)));
        }

        private void FilesListBox_DragDrop(object sender, DragEventArgs e)
        {
			ListBox targetListBox = (ListBox)sender;
            if (e.Data.GetDataPresent(typeof(System.String))) {
                string s = (string)e.Data.GetData(typeof(System.String));
				Object item = (Object)s;
				// Perform drag-and-drop, depending upon the effect.
                if (e.Effect == DragDropEffects.Copy ||
                    e.Effect == DragDropEffects.Move) {
                    // Insert the item.
                    targetListBox.Items.Remove((Object)s);
					if (indexOfTargetItem != ListBox.NoMatches)
                        targetListBox.Items.Insert(indexOfTargetItem, item);
                    else
                        targetListBox.Items.Add(item);
                }
            }
        }

        private void FilesListBox_MouseMove(object sender, MouseEventArgs e)
		{
			ListBox listBox = (ListBox)sender;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {

                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && 
                    !dragBoxFromMouseDown.Contains(e.X, e.Y)) {
                    indexOfTargetItem = Math.Max(0, indexOfTargetItem);
                    string s = (string)listBox.Items[indexOfTargetItem];
					DragDropEffects dropEffect = listBox.DoDragDrop(s, DragDropEffects.All);
				}
            }
		}
		
        private void FilesListBox_MouseDown(object sender, MouseEventArgs e)
        {
			ListBox listBox = (ListBox)sender;
			
			// Get the index of the item the mouse is below.
            indexOfTargetItem = listBox.IndexFromPoint(e.X, e.Y);
			
			if (indexOfTargetItem != ListBox.NoMatches) {
                Size dragSize = SystemInformation.DragSize;
				dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width /2),
                                                               e.Y - (dragSize.Height /2)), dragSize);
            } else
                dragBoxFromMouseDown = Rectangle.Empty;

        }

        private void FilesListBox_MouseUp(object sender, MouseEventArgs e) {
            // Reset the drag rectangle when the mouse button is raised.
            dragBoxFromMouseDown = Rectangle.Empty;
		}

    }
}