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
using System.Windows.Forms;

namespace Multiverse.Tools.WorldEditor
{
	public partial class SubMeshDialog : Form
	{
        SubMeshCollection subMeshCollection;
 
		public SubMeshDialog(SubMeshCollection subMeshCollectionIn)
		{
            // Make a copy for us to muck with.  We don't touch the original.
            this.subMeshCollection = new SubMeshCollection(subMeshCollectionIn);

			InitializeComponent();
			foreach (SubMeshInfo info in subMeshCollection)
			{
				subMeshListBox.Items.Add(info.Name, info.Show);
			}
            subMeshListBox.SelectedIndex = 0;

			if (subMeshListBox.SelectedIndex >= 0)
			{
				materialTextBox.Text = subMeshCollection[subMeshListBox.SelectedIndex].MaterialName;
			}

		}

        public SubMeshCollection SubMeshCollection
        {
            get
            {
                return subMeshCollection;
            }
        }

		private void subMeshListBox_ItemCheck(object obj, ItemCheckEventArgs e)
		{
			subMeshCollection[e.Index].Show = (e.NewValue == CheckState.Checked);
		}

		private void materialTextBox_TextChanged(object sender, EventArgs e)
		{
            if (subMeshListBox.Items.Count > 0 && subMeshListBox.SelectedIndex >= 0)
            {
                if (String.Equals(subMeshCollection[subMeshListBox.SelectedIndex].MaterialName, materialTextBox.Text))
                {
                    return;
                }
                else
                {
                    subMeshCollection[subMeshListBox.SelectedIndex].MaterialName = materialTextBox.Text;
                }
            }
		}

		private void showAllButton_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < subMeshListBox.Items.Count; i++)
			{
				subMeshListBox.SetItemChecked(i, true);
			}
		}

		private void hideAllButton_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < subMeshListBox.Items.Count; i++)
			{
				subMeshListBox.SetItemChecked(i, false);
			}
		}

		private void showButton_Click(object sender, EventArgs e)
		{
			string matchText = subMeshTextBox.Text;
			for (int i = 0; i < subMeshListBox.Items.Count; i++)
			{
				if (subMeshListBox.Items[i].ToString().Contains(matchText))
				{
					subMeshListBox.SetItemChecked(i, true);
				}
			}
		}

		private void hideButton_Click(object sender, EventArgs e)
		{
			string matchText = subMeshTextBox.Text;
			for (int i = 0; i < subMeshListBox.Items.Count; i++)
			{
				if (subMeshListBox.Items[i].ToString().Contains(matchText))
				{
					subMeshListBox.SetItemChecked(i, false);
				}
			}
		}



		private void subMeshTextBox_TextChanged(object sender, EventArgs e)
		{
			if (subMeshTextBox.Text.Length == 0)
			{
				showButton.Enabled = false;
				hideButton.Enabled = false;
			}
			else
			{
				showButton.Enabled = true;
				hideButton.Enabled = true;
			}

		}

		private void subMeshListBox_ItemCheck(object sender, ItemCheckedEventArgs e)
		{
			int index = subMeshListBox.Items.IndexOf(e.Item);
			SubMeshInfo info = subMeshCollection.FindSubMeshInfo((subMeshListBox.Items[index]).ToString());
			if (e.Item.Checked)
			{
				subMeshCollection.ShowSubMesh(info.Name, true);
			}
			else
			{
				subMeshCollection.ShowSubMesh(info.Name, false);
			}
		}

		private void subMeshListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			int index = subMeshListBox.SelectedIndex;
			materialTextBox.Text = subMeshCollection.FindSubMeshInfo(subMeshListBox.Items[index].ToString()).MaterialName;
		}

		private void helpButton_clicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
			WorldEditor.LaunchDoc(but.Tag as string);
		}

	}
}
