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
using Multiverse.Tools.WorldEditor;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
	public partial class TreeDialog : Form
	{
		public TreeDialog(List<AssetDesc> treeFiles, string selectedString, string titleString)
		{
			InitializeComponent();
            
			TreeFileNameComboBox.BeginUpdate();

			foreach (AssetDesc asset in treeFiles)
			{
				TreeFileNameComboBox.Items.Add(asset.Name);
			}
			TreeFileNameComboBox.EndUpdate();
			if (selectedString != "")
			{
				for (int i = 0; i < TreeFileNameComboBox.Items.Count; i++)
				{
					if (String.Equals(TreeFileNameComboBox.Items[i].ToString(), selectedString))
					{
						TreeFileNameComboBox.SelectedIndex = i;
						break;
					}
				}
			}
			else
			{
				if (TreeFileNameComboBox.Items.Count > 0)
				{
					TreeFileNameComboBox.SelectedIndex = 0;
				}
				else
				{
					TreeFileNameComboBox.SelectedIndex = -1;
				}
			}
			this.Text = titleString;

            this.addTreeInstancesTextBox.Text = WorldEditor.Instance.Config.DefaultTreeInstances.ToString();
		}

		public string treeFileName
		{
			get
			{
				return TreeFileNameComboBox.SelectedItem.ToString();
			}
		}

		public float scale
		{
			get
			{
				return float.Parse(addTreeScaleTextBox.Text);
			}
			set
			{
				addTreeScaleTextBox.Text = (value.ToString());
			}
		}

		public float scaleVariance
		{
			get
			{
				return float.Parse(addTreeScaleVarianceTextBox.Text);
			}
			set
			{
				addTreeScaleVarianceTextBox.Text = value.ToString();
			}
		}

		public uint instances
		{
			get
			{
				return uint.Parse(addTreeInstancesTextBox.Text);
			}
			set
			{
				addTreeInstancesTextBox.Text = value.ToString();
			}
		}

		private void uIntVerifyevent(object sender, CancelEventArgs e)
		{
			TextBox textbox = (TextBox)sender;

			if (!ValidityHelperClass.isUint(textbox.Text))
			{
				Color textColor = Color.Red;
				textbox.ForeColor = textColor;
			}
			else
			{
				Color textColor = Color.Black;
				textbox.ForeColor = textColor;
			}
		}

		private void floatVerifyevent(object sender, CancelEventArgs e)
		{
			TextBox textbox = (TextBox)sender;

			if (!ValidityHelperClass.isFloat(textbox.Text))
			{
				Color textColor = Color.Red;
				textbox.ForeColor = textColor;
			}
			else
			{
				Color textColor = Color.Black;
				textbox.ForeColor = textColor;
			}
		}

		public bool okButton_validating()
		{

			if (!ValidityHelperClass.isFloat(addTreeScaleTextBox.Text))
			{

                MessageBox.Show("Scale can not be parsed to a floating point number", "Incorrect scale", MessageBoxButtons.OK);
				return false;
			}
			else
			{
				if (!ValidityHelperClass.isFloat(addTreeScaleVarianceTextBox.Text))
				{
                    MessageBox.Show("Scale variance can not be parsed to a floating point number", "Incorrect scale variance", MessageBoxButtons.OK);
					return false;
				}
				else
				{
					if (!ValidityHelperClass.isUint(addTreeInstancesTextBox.Text))
					{
                        MessageBox.Show("Instances can not be parsed to a unsigned integer number", "Incorrect instances", MessageBoxButtons.OK);
						return false;
					}
				}
			}
			return true;
		}

		private void helpButton_clicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
			WorldEditor.LaunchDoc(but.Tag as string);
		}
	}
}
