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
using System.Collections;
using Multiverse.Tools.WorldEditor;

namespace Multiverse.Tools.WorldEditor
{
	public partial class comboBoxPrompt : Form
	{
		protected string anchor;
		public comboBoxPrompt(List<AssetDesc> assets, string selectedString, string titleString, string anchor)
		{
			this.anchor = anchor;
			InitializeComponent();
			promptComboBox.BeginUpdate();

			foreach (AssetDesc asset in assets)
			{
				promptComboBox.Items.Add(asset.Name);
			}
			promptComboBox.EndUpdate();
			if (selectedString != "")
			{
				for (int i = 0; i < promptComboBox.Items.Count; i++)
				{
					if (String.Equals(promptComboBox.Items[i].ToString(), selectedString))
					{
						promptComboBox.SelectedIndex = i;
						break;
					}
				}
			}
			else
			{
				if (promptComboBox.Items.Count > 0)
				{
					promptComboBox.SelectedIndex = 0;
				}
				else
				{
					promptComboBox.SelectedIndex = -1;
				}
			}
			this.Text = titleString;
		}

        public comboBoxPrompt(List<AssetDesc> assets, string selectedString, string titleString, string anchor, string buttonText) : 
            this (assets, selectedString, titleString, anchor)
        {
            okButton.Text = buttonText;
        }


		public string ComboBoxSelectedItemAsString
		{
			get
			{
				return promptComboBox.SelectedItem.ToString();
			}

		}

		public comboBoxPrompt(List<string> strings, string selectedString, string titleString)
		{
			InitializeComponent();
			promptComboBox.BeginUpdate();

			foreach (string s in strings)
			{
				promptComboBox.Items.Add(s);
			}
			promptComboBox.EndUpdate();
			if (selectedString != "")
			{
				for (int i = 0; i < promptComboBox.Items.Count; i++)
				{
					if (String.Equals(promptComboBox.Items[i].ToString(), selectedString))
					{
						promptComboBox.SelectedIndex = i;
						break;
					}
				}
			}
			else
			{
				if (promptComboBox.Items.Count > 0)
				{
					promptComboBox.SelectedIndex = 0;
				}
				else
				{
					promptComboBox.SelectedIndex = -1;
				}
			}
			this.Text = titleString;
		}

		public string selectedItem
		{
			get
			{
				return promptComboBox.SelectedItem.ToString();
			}
		}

		public string ComboBoxLabelText
		{
			get
			{
				return comboBoxLabel.Text;
			}
			set
			{
				comboBoxLabel.Text = value;
			}
		}

		public string TitleString
		{
			get
			{
				return this.Text;
			}
			set
			{
				this.Text = value;
			}
		}


		private void helpButton_clicked(object sender, EventArgs e)
		{
			WorldEditor.LaunchDoc(this.anchor);
		}
	}
}
