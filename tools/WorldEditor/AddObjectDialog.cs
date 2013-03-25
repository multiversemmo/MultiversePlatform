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
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public partial class AddObjectDialog : Form
    {
        protected AssetCollection assets;
        protected static string lastTypeSelected = "All";

        public AddObjectDialog(AssetCollection assets)
        {
            InitializeComponent();

            this.assets = assets;

            PopulateTypeComboBox();
            if(!String.Equals(lastTypeSelected,""))
            {
                objectTypeComboBox.SelectedItem = lastTypeSelected;
            }
            if (objectComboBox.Items.Count > 0)
            {
                objectComboBox.SelectedIndex = 0;
            }
        }

        private void PopulateObjectComboBox(string type)
        {
            List<AssetDesc> models;
            if (type == "All")
            {
                models = assets.Select("Model");
            }
            else
            {
                models = assets.Select("Model", type);
            }

            objectComboBox.BeginUpdate();
            objectComboBox.Items.Clear();

            foreach (AssetDesc asset in models)
            {
                objectComboBox.Items.Add(asset.Name);
            }
            objectComboBox.EndUpdate();

            return;
        }

        private void PopulateTypeComboBox()
        {
            List<String> modelTypes = assets.SubTypes("Model");
            modelTypes.Add("All");

            objectTypeComboBox.DataSource = modelTypes;
        }

        public String ObjectName
        {
            get
            {
                return objectNameTextBox.Text;
            }
        }

        public String ObjectMeshName
        {
            get
            {
                return assets[objectComboBox.Text].AssetName;
            }
        }

        public bool RandomRotation
        {
            get
            {
                return randomRotationCheckBox.Checked;
            }
            set
            {
                randomRotationCheckBox.Checked = value;
            }
        }

        public bool RandomScale
        {
            get
            {
                return randomScaleCheckBox.Checked;
            }
            set
            {
                randomScaleCheckBox.Checked = value;
            }
        }

        public float MinScale
        {
            get
            {
                return float.Parse(minScaleTextBox.Text);
            }
            set
            {
                minScaleTextBox.Text = value.ToString();
            }
        }


        public float MaxScale
        {
            get
            {
                return float.Parse(maxScaleTextBox.Text);
            }
            set
            {
                maxScaleTextBox.Text = value.ToString();
            }
        }

        public bool MultiPlacement
        {
            get
            {
                return placeMultipleCheckBox.Checked;
            }
        }

        private void objectTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateObjectComboBox(objectTypeComboBox.Text);
            if (objectComboBox.Items.Count > 0)
            {
                objectComboBox.SelectedIndex = 0;
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
            if (string.Equals(objectComboBox.Text, ""))
            {
                return false;
            }
            else
            {

                if (!ValidityHelperClass.isFloat(minScaleTextBox.Text))
                {
                    MessageBox.Show("Minimum scale can not be parsed to a floating point number", "Incorrect minimum scale", MessageBoxButtons.OK);
                    return false;
                }
                else
                {
                    if (!ValidityHelperClass.isFloat(maxScaleTextBox.Text))
                    {
                        MessageBox.Show("Maximum scale can not be parsed to a floating point number", "Incorrect maximum scale", MessageBoxButtons.OK);
                        return false;
                    }
                }
                return true;
            }
		}

		private void onHelpButton_clicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
			WorldEditor.LaunchDoc(but.Tag as string);
		}

        private void createButton_Click(object sender, EventArgs e)
        {
            lastTypeSelected = objectTypeComboBox.SelectedItem as string;
        }

        private void randomScaleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (randomScaleCheckBox.Checked)
            {
                minScaleTextBox.Enabled = true;
                maxScaleTextBox.Enabled = true;
            }
            else
            {
                minScaleTextBox.Enabled = false;
                maxScaleTextBox.Enabled = false;
            }
        }
    }
}
