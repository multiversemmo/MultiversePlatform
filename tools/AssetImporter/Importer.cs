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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Multiverse.AssetRepository;

namespace AssetImporter
{
    public partial class Importer : Form
    {
        private int genOffset = 0;
        private string baseHelpURL = "http://update.multiverse.net/wiki/index.php/Using_Asset_Importer_Version_1.5";
        private string baseReleaseNoteURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.5_Release_Notes";
        private string feedbackURL = "http://update.multiverse.net/custportal/login.php";
        
        public Importer()
        {
            inhibit = true;
            InitializeComponent();
			for (AssetTypeEnum i=AssetTypeEnum.Mesh; i<=AssetTypeEnum.Other; i++)
				assetTypeComboBox.Items.Add(AssetTypeDesc.AssetTypeEnumName(i));
			List<string> log = RepositoryClass.Instance.InitializeRepository();
			CheckLogAndMaybeExit(log);
			updateRepositoryPath();
            somethingChanged = false;
            cameFromFile = false;
			setEnables();
			categoryPanel.Visible = false;
            genOffset = filesListBox.Left + filesListBox.Width;
            inhibit = false;
		}


        private void LaunchProcess(string URL)
        {
            System.Diagnostics.Process.Start(URL);
        }

		protected void CheckLogAndMaybeExit(List<string> log)
		{
            if (log.Count > 0) {
                string lines = "";
                foreach (string s in log)
                    lines += s + "\n";
                if (MessageBox.Show("Error(s) initializing asset repository:\n\n" + lines,
                                    "Errors Initializing Asset Repository.  Click Cancel To Exit",
                                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    Environment.Exit(-1);
            }
		}
		
        private void updateRepositoryPath()
		{
			string path = RepositoryClass.Instance.RepositoryDirectoryListString;
			repositoryLabel.Text = (path.Length > 0 ? "Repositories: " + path : 
                                    "No Repository Designated");
		}
		
		private void setAssetType(AssetTypeEnum assetTypeEnum)
        {
			createControlsForAssetType(assetTypeEnum);
			setupCategoryComboBox(assetTypeEnum);
		}
		
		private void setupCategoryComboBox(AssetTypeEnum assetTypeEnum)
		{
			categoryComboBox.Items.Clear();
			string []categories = RepositoryClass.GetCategoriesForType(assetTypeEnum);
			if (categories != null) {
				foreach (string category in categories)
					categoryComboBox.Items.Add(category);
			}
			categoryPanel.Visible = categories != null;
		}

		private void setCategory(string categoryString)
		{
			if (categoryPanel.Visible) {
				if (categoryString.Length == 0)
					categoryComboBox.SelectedItem = null;
				else
					categoryComboBox.Text = categoryString;
			}
		}
		
        private void setAssetFileName(string path)
		{
			assetFileName = path;
			Text = "Asset Importer: " + path;
		}
		
		internal class FileDescControls
		{
			internal AssetTypeFileDesc desc;
			internal int index;
			internal int yCoord;
			internal int tabIndex;
			internal int height;
			internal bool useTextBox;
			internal string labelText;
			internal Label label;
			internal Button button;
			internal TextBox textBox;
			internal ListBox listBox;
			internal void RemoveControls(Form form) 
			{
                form.Controls.Remove(label);
                form.Controls.Remove(button);
				form.Controls.Remove(useTextBox ? (Control)textBox : (Control)listBox);
			}
		}
		
        private void RemoveDynamicControls()
        {
            if (allControls != null)
            {
                foreach (FileDescControls controls in allControls)
                    controls.RemoveControls(this);
                allControls = null;
            }
        }

        private void ClearControls()
        {
            assetNameTextBox.Text = "";
            descriptionTextBox.Text = "";
            inhibit = true;
            assetTypeComboBox.SelectedItem = null;
            inhibit = false;
            RemoveDynamicControls();
            somethingChanged = false;
			categoryPanel.Visible = false;
            setEnables();
        }
        
		private void newAsset()
		{
            if (checkNeedToSave("create a new asset")) {
                savedProperties = new List<AssetProperty>();
				ClearControls();
				assetFileName = "";
				cameFromFile = false;
				setEnables();
			}
        }

        private void createControlsForAssetType(AssetTypeEnum typeEnum)
		{
			SuspendLayout();
            RemoveDynamicControls();
			allControls = new List<FileDescControls>();
			AssetTypeDesc type = AssetTypeDesc.FindAssetTypeDesc(typeEnum);
			int index = 0;
			int y = 180;
			foreach (AssetTypeFileDesc desc in type.FileTypes) {
                FileDescControls c;
				y = createAssetTypeFileControls(index, y, desc, out c);
                allControls.Add(c);
				index++;
			}

            ResumeLayout(false);
            PerformLayout();
		}
		
		private int createAssetTypeFileControls(int index, int y, AssetTypeFileDesc desc, out FileDescControls c)
		{
			Control parentControl = this;
			c = new FileDescControls();
			c.desc = desc;
			c.tabIndex = assetTypeComboBox.TabIndex + 1 + index * 2;
			c.index = index;
            c.yCoord = y;
			int labelHeight = 36;
			if (desc.MaxCount == 1) {
				c.labelText = (desc.MinCount == 0 ? "Optional " : "") + 
					(desc.AdditionalText != "" ? desc.AdditionalText + " " : "") +
					AssetFile.AssetFileEnumName(desc.FileTypeEnum);
				c.height = 40;
				c.useTextBox = true;
			}
			else {
                c.labelText = "Optional " + (desc.AdditionalText != "" ? desc.AdditionalText + " " : "") +
							   AssetFile.AssetFileEnumName(desc.FileTypeEnum) + "s";
				labelHeight = 72;
				c.height = 100;
				c.useTextBox = false;
			}
            int boxWidth = this.Width - genOffset - 94 - 67 - 18;
			int buttonLeft = genOffset + 101 + boxWidth;
            c.labelText += ":";
			Label label = new System.Windows.Forms.Label();
			label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleRight;
			label.Location = new System.Drawing.Point(genOffset + 5, y - 6);
            label.Name = "AddLabel" + index;
            label.Size = new System.Drawing.Size(85, labelHeight);
            label.TabStop = false;
            label.Text = c.labelText;
			label.Parent = parentControl;
			c.label = label;
			
			Button button = new System.Windows.Forms.Button();
            button.Location = new System.Drawing.Point(buttonLeft, y);
            button.Name = "AddButton" + index;
            button.Size = new System.Drawing.Size(63, 24);
            button.TabIndex = c.tabIndex;
            button.Text = "Browse...";
            button.UseVisualStyleBackColor = true;
			button.Tag = c;
			button.Click += buttonSelect_Click;
			button.Parent = parentControl;
			c.button = button;

            if (c.useTextBox) {
				TextBox textBox = new System.Windows.Forms.TextBox();
				textBox.Location = new System.Drawing.Point(genOffset + 94, y);
				textBox.Name = "textBox1";
				textBox.Size = new System.Drawing.Size(boxWidth, 20);
                textBox.TabIndex = c.tabIndex + 1;
				textBox.Tag = c;
				textBox.Parent = parentControl;
				textBox.AllowDrop = true;
				textBox.DragDrop += new System.Windows.Forms.DragEventHandler(anyTextBox_DragDrop);
				textBox.DragOver += new System.Windows.Forms.DragEventHandler(anyTextBox_DragOver);
                textBox.TextChanged += new System.EventHandler(someTextBoxChanged);
				c.textBox = textBox;
			}
			else {
				ListBox listBox = new System.Windows.Forms.ListBox();
				listBox.FormattingEnabled = true;
				listBox.Location = new System.Drawing.Point(genOffset + 94, y);
				listBox.Name = "AddListBox" + index;
				listBox.Size = new System.Drawing.Size(boxWidth, 82);
                listBox.TabIndex = c.tabIndex + 1;
				listBox.Tag = c;
				listBox.Parent = parentControl;
				listBox.AllowDrop = true;
				listBox.DragDrop += new System.Windows.Forms.DragEventHandler(anyListBox_DragDrop);
				listBox.DragOver += new System.Windows.Forms.DragEventHandler(anyListBox_DragOver);
				listBox.MouseUp += new System.Windows.Forms.MouseEventHandler(anyListBox_MouseUp);
				listBox.MouseMove += new System.Windows.Forms.MouseEventHandler(anyListBox_MouseMove);
				listBox.MouseDown += new System.Windows.Forms.MouseEventHandler(anyListBox_MouseDown);

				c.listBox = listBox;
			}
			return y + c.height;
		}

        private void anyTextBox_DragOver(object sender, DragEventArgs e) 
        {
			TextBox targetTextBox = (TextBox)sender;
			if (!e.Data.GetDataPresent(typeof(System.String))) {
					e.Effect = DragDropEffects.None;
					return;
			}
			string value = (string)e.Data.GetData(DataFormats.StringFormat);
			Object tag = targetTextBox.Tag;
			if (targetTextBox.Text == value ||
				(tag != null && !AssetFile.AllExtensionsForEnum(((FileDescControls)tag).desc.FileTypeEnum).
				                   Contains(Path.GetExtension(value).ToLower()))) {
				e.Effect = DragDropEffects.None;
				return;
            }
			e.Effect = DragDropEffects.Copy;
        }

        private void anyTextBox_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) 
        {
			TextBox targetTextBox = (TextBox)sender;
			FileDescControls c = (FileDescControls)targetTextBox.Tag;
            if (e.Data.GetDataPresent(typeof(System.String))) {
                string value = (string)e.Data.GetData(typeof(System.String));
                // Perform drag-and-drop, depending upon the effect.
                if (e.Effect == DragDropEffects.Copy ||
                    e.Effect == DragDropEffects.Move) {
					value = RepositoryClass.MakeFullFilePath(lastSourceDirectory, value);
					value = adjustForRepositoryPath(value);
					targetTextBox.Text = value;
                    if (c.index == 0 && assetNameTextBox.Text.Length == 0) {
                        string fileName = Path.GetFileNameWithoutExtension(value);
                        assetNameTextBox.Text = Path.GetFileNameWithoutExtension(fileName) + "_" +
                            AssetTypeDesc.AssetTypeEnumFileName((AssetTypeEnum)assetTypeComboBox.SelectedIndex + 1);
                    }
                    setSomethingChanged();
				}
            }
        }

        private void trashPictureBox_DragDrop(object sender, DragEventArgs e)
        {
			if (e.Data.GetDataPresent(typeof(System.String))) {
                string value = (string)e.Data.GetData(typeof(System.String));
                if (sourceListBox != null &&
					e.Effect == DragDropEffects.Copy ||
                    e.Effect == DragDropEffects.Move) {
					// Remove the selected item from the source list box
					sourceListBox.Items.RemoveAt(sourceListBox.SelectedIndex);
					setSomethingChanged();
				}
			}
        }

        private void trashPictureBox_DragOver(object sender, DragEventArgs e)
        {
			if (sourceListBox == null ||
				!e.Data.GetDataPresent(typeof(System.String))) {
					e.Effect = DragDropEffects.None;
					return;
			}
			e.Effect = DragDropEffects.Copy;
        }

        private void anyListBox_DragOver(object sender, DragEventArgs e) 
        {
			ListBox targetListBox = (ListBox)sender;
			if (!e.Data.GetDataPresent(typeof(System.String))) {
					e.Effect = DragDropEffects.None;
					return;
			}
			string value = (string)e.Data.GetData(DataFormats.StringFormat);
			Object tag = targetListBox.Tag;
			if (targetListBox.Items.Contains(value) ||
				(tag != null && !AssetFile.AllExtensionsForEnum(((FileDescControls)tag).desc.FileTypeEnum).
 				                       Contains(Path.GetExtension(value).ToLower()))) {
				e.Effect = DragDropEffects.None;
				return;
            }
			e.Effect = DragDropEffects.Copy;
			indexOfTargetItem = targetListBox.IndexFromPoint(targetListBox.PointToClient(new Point(e.X, e.Y)));
        }

        private void anyListBox_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) 
        {
			ListBox targetListBox = (ListBox)sender;
            if (e.Data.GetDataPresent(typeof(System.String))) {
                string s = (string)e.Data.GetData(typeof(System.String));
                if (targetListBox != filesListBox)
					s = RepositoryClass.MakeFullFilePath(lastSourceDirectory, s);
				s = adjustForRepositoryPath(s);
				Object item = (Object)s;
				// Perform drag-and-drop, depending upon the effect.
                if (e.Effect == DragDropEffects.Copy ||
                    e.Effect == DragDropEffects.Move) {
                    // Insert the item.
                    deleteMatchingListBoxItem(targetListBox, s);
					if (indexOfTargetItem != ListBox.NoMatches)
                        targetListBox.Items.Insert(indexOfTargetItem, item);
                    else
                        targetListBox.Items.Add(item);
					setSomethingChanged();
                }
            }
        }

        private void anyListBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) 
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
		
		private void anyListBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) 
        {
			ListBox listBox = (ListBox)sender;
			
			// Get the index of the item the mouse is below.
            indexOfTargetItem = listBox.IndexFromPoint(e.X, e.Y);
			
			if (indexOfTargetItem != ListBox.NoMatches) {
                Size dragSize = SystemInformation.DragSize;
                if (listBox != filesListBox)
					sourceListBox = listBox;
				else
					sourceListBox = null;
				dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width /2),
                                                               e.Y - (dragSize.Height /2)), dragSize);
            } else
                dragBoxFromMouseDown = Rectangle.Empty;

        }

        private void anyListBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            // Reset the drag rectangle when the mouse button is raised.
            dragBoxFromMouseDown = Rectangle.Empty;
		}

        private void someTextBoxChanged(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            setSomethingChanged();
        }

        private List<string> getSelectedItems(ListBox listBox)
        {
            List<string> list = new List<string>();
            foreach (int index in listBox.SelectedIndices)
                list.Add((string)listBox.Items[index]);
            return list;
        }

        private bool assetFilesFilledIn()
        {
            if (allControls == null)
                return false;
            foreach (FileDescControls desc in allControls)
            {
                if (desc.desc.MinCount >= 1 &&
                    (desc.useTextBox ? desc.textBox.Text.Length == 0 :
                     desc.listBox.Items.Count == 0))
                    return false;
            }
            return true;
        }

        private int countAssetFilesFilledIn()
        {
            if (allControls == null)
                return 0;
            int cnt = 0;
			foreach (FileDescControls desc in allControls)
				cnt += (desc.useTextBox ? (desc.textBox.Text.Length == 0 ? 0 : 1) :
						desc.listBox.Items.Count);
			return cnt;
        }

        private void setSomethingChanged()
		{
			somethingChanged = true;
			setEnables();
		}
		
		private void setEnables()
        {
            bool haveRepository = RepositoryClass.Instance.RepositoryDirectoryList.Count > 0;
			assetTypeComboBox.Enabled = haveRepository;
			newAssetToolStripMenuItem.Enabled = haveRepository;
			newAssetToolStripButton.Enabled = haveRepository;
			openAssetToolStripMenuItem.Enabled = haveRepository;
			openAssetToolStripButton.Enabled = haveRepository;
			assetNameTextBox.Enabled = haveRepository;
			bool enableSave = false;
            if (haveRepository && 
				assetNameTextBox.Text.Length > 0 &&
                assetTypeComboBox.Text.Length > 0 &&
                countAssetFilesFilledIn() > 0 &&
                somethingChanged)
                enableSave = true;
			saveAssetToolStripButton.Enabled = enableSave;
            saveAssetToolStripMenuItem.Enabled = enableSave;
			saveAssetAsToolStripMenuItem.Enabled = assetNameTextBox.Text.Length > 0;
        }

		bool areYouSure(string introducer, string message, string caption)
        {
            return MessageBox.Show(introducer + "  Are you sure you wish to " + message + "?", 
                                   caption,
								   MessageBoxButtons.OKCancel) == DialogResult.OK;
        }
        
 
		private bool checkNeedToSave(string whatToDo)
        {
            return (!somethingChanged ||
                    areYouSure("You have unsaved changes.", "discard them and " + whatToDo,
                               "Discard Unsaved Changes?"));
        }           
        
		private void openAsset()
		{
            if (checkNeedToSave("open an asset definition file")) {
                string dir = RepositoryClass.Instance.AssetDefinitionDirectory;
                if (dir != "" && Directory.Exists(dir))
                    openAssetDialog.InitialDirectory = dir;
				if (openAssetDialog.ShowDialog() == DialogResult.OK) {
                    List<string> log = new List<string>();
					assetDefinition = AssetDefinition.ReadXmlFile(openAssetDialog.FileName, log);
					if (log.Count > 0) {
						string lines = "";
						foreach (string s in log)
							lines += s + "\n";
						MessageBox.Show("Error(s) reading asset definition - - reading cancelled\n\n" + lines,
										"Errors Reading Asset Definition File",
										MessageBoxButtons.OK);
						return;
					}
					ClearControls();
					CheckLogAndMaybeExit(log);
					savedProperties = assetDefinition.Properties;
					displayAssetDefinition();
					setAssetFileName(openAssetDialog.FileName);
					somethingChanged = false;
					cameFromFile = true;
					setEnables();
				}
			}
		}
		
        // Returns true if the file should be copied
		private bool copyFromSource(string path)
		{
			string s = Path.GetPathRoot(path);
			return s != "";
		}
		
        private string MakeDestinationFile(AssetTypeEnum assetType, string s, AssetFileEnum fileType, int fileNumber) 
        {
            bool unrootedOther = (assetType == AssetTypeEnum.Other || fileType == AssetFileEnum.Other) && 
                                 !Path.IsPathRooted(s);
            string directory = unrootedOther ? Path.GetDirectoryName(s) :
                AssetFile.DirectoryForFileEnum(assetType, fileNumber == 0, fileType);
            string targetFile = directory + "\\" + Path.GetFileName(s);
            return targetFile;
        }

		private void encacheAssetDefinition(out List<string> filesToCopy, out List<string> fileDestinations)
        {
            assetDefinition = new AssetDefinition();
            assetDefinition.Name = assetNameTextBox.Text;
            assetDefinition.Description = descriptionTextBox.Text;
			assetDefinition.TypeEnum = AssetTypeDesc.AssetTypeEnumFromName(assetTypeComboBox.Text);
            assetDefinition.Category = categoryComboBox.Text;
			assetDefinition.Properties = savedProperties;
			int fileNumber = 0;
			filesToCopy = new List<string>();
			fileDestinations = new List<string>();
			foreach (FileDescControls desc in allControls)
            {
                AssetTypeFileDesc d = desc.desc;
                if (desc.useTextBox) {
                    if (desc.textBox.Text.Length > 0)
                    {
                        string s = desc.textBox.Text.ToString();
                        string targetFile = MakeDestinationFile(assetDefinition.TypeEnum, s, d.FileTypeEnum, fileNumber);
                        AssetFile file = new AssetFile(targetFile, d.FileTypeEnum);
                        if (copyFromSource(s)) {
							filesToCopy.Add(s);
							fileDestinations.Add(file.TargetFile);
						}
                        assetDefinition.Files.Add(file);
						fileNumber++;
                    }
                }
                else
                {
                    for (int i = 0; i < desc.listBox.Items.Count; i++)
                    {
                        AssetFile file = new AssetFile();
                        file.FileTypeEnum = d.FileTypeEnum;
                        string s = desc.listBox.Items[i].ToString();
                        string targetFile = MakeDestinationFile(assetDefinition.TypeEnum, s, d.FileTypeEnum, fileNumber);
                        file.TargetFile = targetFile;
                        if (copyFromSource(s)) {
							filesToCopy.Add(s);
							fileDestinations.Add(file.TargetFile);
						}
                        assetDefinition.Files.Add(file);
						fileNumber++;
                    }
                }
            }
			assetDefinition.ComputeStatus();
        }

        private void displayAssetDefinition()
        {
            assetNameTextBox.Text = assetDefinition.Name;
            descriptionTextBox.Text = assetDefinition.Description;
			assetTypeComboBox.Text = AssetTypeDesc.AssetTypeEnumName(assetDefinition.TypeEnum);
			setupCategoryComboBox(assetDefinition.TypeEnum);
			setCategory(assetDefinition.Category);
			createControlsForAssetType(assetDefinition.TypeEnum);
			int fileIndex = 0;
			foreach (FileDescControls desc in allControls)
            {
                if (fileIndex >= assetDefinition.Files.Count)
                    break;
                AssetTypeFileDesc d = desc.desc;
				AssetFile file = assetDefinition.Files[fileIndex];
                if (desc.useTextBox) {
                    if (d.FileTypeEnum == file.FileTypeEnum) {
						desc.textBox.Text = file.TargetFile;
						fileIndex++;
					}
                }
                else
                {
                    while (fileIndex < assetDefinition.Files.Count) {
						file = assetDefinition.Files[fileIndex];
						if (d.FileTypeEnum != file.FileTypeEnum)
							break;
                        desc.listBox.Items.Add(file.TargetFile);
						fileIndex++;
                    }
                }
            }
        }

        private bool saveOrSaveAs(bool saveExisting, bool saveButton)
        {
            if (saveButton)
				saveExisting = assetFileName != "" && 
					Path.GetFileNameWithoutExtension(assetFileName) == assetNameTextBox.Text;
			if (!saveExisting) {
                string dir = RepositoryClass.Instance.AssetDefinitionDirectory;
                if (dir != "" && Directory.Exists(dir))
                    saveAssetDialog.InitialDirectory = dir;
				saveAssetDialog.FileName = assetNameTextBox.Text + ".asset";
				if (saveAssetDialog.ShowDialog() == DialogResult.OK)
					assetFileName = saveAssetDialog.FileName;
				else
					return false;
			}
 			List<string> filesToCopy;
			List<string> fileDestinations;
			encacheAssetDefinition(out filesToCopy, out fileDestinations);
			assetDefinition.WriteXmlFile(assetFileName);
			RepositoryClass.Instance.CopyAssetFiles(filesToCopy, fileDestinations);
			setAssetFileName(assetFileName);
			somethingChanged = false;
            cameFromFile = true;
			setEnables();
			return true;
        }

		private void designateRepository()
		{
            DesignateRepositoriesDialog designateRepositoriesDialog = new DesignateRepositoriesDialog();
            DialogResult result = designateRepositoriesDialog.ShowDialog();
            if (result == DialogResult.OK) {
				updateRepositoryPath();
                setEnables();
            }
        }

		private void maybeUpdateSourceDirectory(string fileName)
		{
			string dir = Path.GetDirectoryName(fileName);
			filesLabel.Text = "Files in " + dir;
            filesLabel.Left = filesListBox.Left;
			if (lastSourceDirectory != dir) {
				filesListBox.Items.Clear();
				lastSourceDirectory = dir;
				DirectoryInfo info = new DirectoryInfo(dir);
				FileInfo[] files = info.GetFiles();
				foreach (FileInfo file in files)
					filesListBox.Items.Add(file.Name);
			}
		}

        private bool somethingChanged = false;
        private bool cameFromFile = false;
		private bool inhibit = false;
		private List<FileDescControls> allControls = null;
        private AssetDefinition assetDefinition = null;
		private List<AssetProperty> savedProperties = new List<AssetProperty>();
		private string assetFileName = "";
		private Rectangle dragBoxFromMouseDown;
		private int indexOfTargetItem;
		private string lastSourceDirectory = "";
		private ListBox sourceListBox = null;

        private void assetNameTextBox_TextChanged(object sender, EventArgs e)
        {
            setSomethingChanged();
        }

		private void buttonSelect_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			FileDescControls c = (FileDescControls)button.Tag;
			AssetTypeFileDesc desc = c.desc;
			// Invoke the file open dialog
			openSourceFileDialog.FileName = "";
			List<string> extensions = AssetFile.AllExtensionsForEnum(desc.FileTypeEnum);
			string extList = "";
			string extListHidden = "";
			foreach (string ext in extensions) {
                if (extList.Length > 0)
                    extList += ";";
				extList += "*" + ext;
				if (extListHidden.Length > 0)
					extListHidden += ";";
				extListHidden += "*" + ext + ";" + "*" + ext.ToUpper();
			}
			string filter = string.Format("All {0} Files ({1})|{2}|All Files (*.*)|*.*",
										  AssetFile.AssetFileEnumName(desc.FileTypeEnum),
										  extList, extListHidden);
			openSourceFileDialog.Filter = filter;
			openSourceFileDialog.Multiselect = c.listBox != null;
			if (openSourceFileDialog.ShowDialog() == DialogResult.OK) {
				foreach (string fileName in openSourceFileDialog.FileNames) {
					string extension = Path.GetExtension(fileName);
					if (c.index == 0 && assetNameTextBox.Text.Length == 0)
						assetNameTextBox.Text = Path.GetFileNameWithoutExtension(fileName) + "_" +
							AssetTypeDesc.AssetTypeEnumFileName((AssetTypeEnum)assetTypeComboBox.SelectedIndex + 1);
					// 				if (extension.ToLower() == ".mesh") {
					// 					string materialName;
					// 					string skeletonName;
					// 					RepositoryClass.Instance.ReadMeshMaterialAndSkeleton(fileName, out materialName, out skeletonName);
					// 				}
					maybeUpdateSourceDirectory(fileName);
					string adjustedFileName = adjustForRepositoryPath(fileName);
					if (c.useTextBox)
						c.textBox.Text = adjustedFileName;
					else {
						deleteMatchingListBoxItem(c.listBox, adjustedFileName);
						c.listBox.Items.Add(adjustedFileName);
					}
				}
                setSomethingChanged();
			}
		}

		private string adjustForRepositoryPath(string fileName)
		{
			List<string> dirs = RepositoryClass.Instance.RepositoryDirectoryList;
            if (dirs != null) {
                foreach (string dir in dirs) {
                    if (fileName.StartsWith(dir))
                        return fileName.Substring(dir.Length + 1);
                }
            }
            return fileName;
		}
		
		private void deleteMatchingListBoxItem(ListBox box, string fileName)
		{
            box.Items.Remove((Object)fileName);
		}
		
		private void assetTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!inhibit)
                setAssetType(AssetTypeDesc.AssetTypeEnumFromName(assetTypeComboBox.Text));
        }

       private void Importer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (somethingChanged && !checkNeedToSave("close the application"))
                e.Cancel = true;
                    
        }

        private void categoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!inhibit)
				setSomethingChanged();
        }

        private void newAssetToolStripMenuItem_Click(object sender, EventArgs e)
        {
			newAsset();
        }

        private void openAssetToolStripMenuItem_Click(object sender, EventArgs e)
        {
			openAsset();
        }

        private void saveAssetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveOrSaveAs(cameFromFile, false);
        }

        private void saveAssetAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
			saveOrSaveAs(false, false);
        }

        private void designateRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
			designateRepository();
        }

        private void newAssetToolStripButton_Click(object sender, EventArgs e)
        {
			newAsset();
        }

        private void openAssetToolStripButton_Click(object sender, EventArgs e)
        {
			openAsset();
        }

        private void saveAssetToolStripButton_Click(object sender, EventArgs e)
        {
			saveOrSaveAs(false, true);
        }

        private void designateRepositoryToolStripButton_Click(object sender, EventArgs e)
        {
			designateRepository();
        }

        private void descriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            setSomethingChanged();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) 
		{
            if (somethingChanged && !checkNeedToSave("exit the application"))
				return;
			Environment.Exit(0);
        }

        private void aboutAssetImporterToolStripMenuItem_Click(object sender, EventArgs e) {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string msg = string.Format("Multiverse Asset Importer\n\nVersion: {0}\n\nCopyright 2006-2007 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse Asset Importer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Importer_Resize(object sender, EventArgs e) {
            if (inhibit)
                return;
			// int offset = 223;
            int boxWidth = this.Width - genOffset - 94 - 67 - 18;
			int buttonLeft = genOffset + 101 + boxWidth;
            descriptionTextBox.Width = boxWidth - 35;
            categoryPanel.Left = this.Width - categoryPanel.Width - 130;
            trashPictureBox.Left = this.Width - trashPictureBox.Width - 20;
            filesListBox.Height = this.Height - filesListBox.Top - 60;
            descriptionLabel.Top = this.Height - 90;
            descriptionTextBox.Top = this.Height - 90;
            if (allControls != null)
            {
                foreach (FileDescControls controls in allControls) {
                    controls.button.Left = buttonLeft;
                    if (controls.textBox != null)
                        controls.textBox.Width = boxWidth;
                    if (controls.listBox != null)
                        controls.listBox.Width = boxWidth;
                }
            }
        }

        private void releaseNotesMenuItem_Clicked(object sender, EventArgs e)
        {
            LaunchProcess(baseReleaseNoteURL);
        }

        private void submitFeedbackMenuItem_Clicked(object sender, EventArgs e)
        {
            LaunchProcess(feedbackURL);
        }

        private void launchOnlineHelpMenuItem_Clicked(object sender, EventArgs e)
        {
            LaunchProcess(baseHelpURL);
        }

        

        private void propertyEditorButton_Clicked(object sender, EventArgs e)
        {
            List<AssetProperty> properties = new List<AssetProperty>();
            if (savedProperties != null)
            {
                foreach (AssetProperty property in savedProperties)
                {
                    properties.Add(new AssetProperty(property));
                }
                using (AssetPropertyCollectionEditor dlg = new AssetPropertyCollectionEditor(properties))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        savedProperties = dlg.Properties;
                    }
                    if (dlg.SomethingChanged && !somethingChanged)
                    {
                        setSomethingChanged();
                    }
                }
            }

        }

     }

}
