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
using Multiverse.AssetRepository;

namespace AssetImporter
{
    public partial class AssetPropertyCollectionEditor : Form
    {
        protected List<AssetProperty> assetPropertyList;
        protected bool somethingChanged = false;
        public AssetPropertyCollectionEditor(List<AssetProperty> workingAssetPropertyList)
        {
            assetPropertyList = workingAssetPropertyList;
            InitializeComponent();
            foreach (AssetProperty property in assetPropertyList)
            {
                propertyListBox.Items.Add(String.Format("{0}={1}", property.Name, property.Value) as object);
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AssetProperty newProperty = new AssetProperty(propertyTextBox.Text, valueTextBox.Text);
            assetPropertyList.Add(newProperty);
            propertyListBox.Items.Add(String.Format("{0}={1}", newProperty.Name, newProperty.Value) as object);
            somethingChanged = true;
            
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            int index = propertyListBox.SelectedIndex;
            assetPropertyList[index].Name = propertyTextBox.Text;
            assetPropertyList[index].Value = valueTextBox.Text;
            propertyListBox.Items.RemoveAt(index);
            propertyListBox.Items.Insert(index, String.Format("{0}={1}", propertyTextBox.Text, valueTextBox.Text) as object);
            propertyListBox.SelectedIndex = index;
            somethingChanged = true;
        }

        

        private void deleteButton_Click(object sender, EventArgs e)
        {

            assetPropertyList.RemoveAt(propertyListBox.SelectedIndex);
            propertyListBox.Items.RemoveAt(propertyListBox.SelectedIndex);
            somethingChanged = true;
        }

        public List<AssetProperty> Properties
        {
            get
            {
                return assetPropertyList;
            }
        }

        private void propertyListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (propertyListBox.SelectedItem != null)
            {

                propertyTextBox.Text = assetPropertyList[propertyListBox.SelectedIndex].Name;
                valueTextBox.Text = assetPropertyList[propertyListBox.SelectedIndex].Value;
                editButton.Enabled = true;
                deleteButton.Enabled = true;
            }
            else
            {
                editButton.Enabled = false;
                deleteButton.Enabled = false;
            }
        }

        public bool SomethingChanged
        {
            get
            {
                return somethingChanged;
            }
        }

    }
}
