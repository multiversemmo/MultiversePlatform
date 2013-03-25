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
using System.Text;
using System.Windows.Forms;
using Multiverse.Lib.WorldMap;

namespace Multiverse.Tools.TerrainAssembler
{
    public partial class LoadLayerDialog : Form
    {
        protected Image layerMap;

        public LoadLayerDialog()
        {
            InitializeComponent();
        }

        protected bool ValidateFilename(string name)
        {
            bool exists = System.IO.File.Exists(name);

            return exists;
        }

        protected void LoadImage(string name)
        {
            layerMap = new Image(name);

            UpdateLayerMapInfo();
        }

        protected void UpdateLayerMapInfo()
        {
            if (layerMap != null)
            {
            }
        }

        public List<string> LayerNames
        {
            set
            {
                layerComboBox.Items.Clear();
                foreach (string s in value)
                {
                    layerComboBox.Items.Add(s);
                }
            }
        }

        public string LayerImageFilename
        {
            get
            {
                return imageNameTextBox.Text;
            }
        }

        public int MetersPerPixel
        {
            get
            {
                return int.Parse(metersPerPixelComboBox.Text);
            }
        }

        public string LayerName
        {
            get
            {
                return layerComboBox.Text;
            }
        }

        public Image LayerMapImage
        {
            get
            {
                return layerMap;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select layer map image";
                dlg.DefaultExt = "png";
                dlg.Filter = "PNG Image File (*.png)|*.png";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imageNameTextBox.Text = dlg.FileName;

                    if (ValidateFilename(dlg.FileName))
                    {
                        LoadImage(dlg.FileName);
                        validationErrorProvider.SetError(imageNameTextBox, "");
                        loadButton.Enabled = true;
                    }
                    else
                    {
                        validationErrorProvider.SetError(imageNameTextBox, "File does not exist");
                        loadButton.Enabled = false;
                    }
                }
            }
        }

        private void metersPerPixelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLayerMapInfo();
        }

        private void imageNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            bool valid = ValidateFilename(imageNameTextBox.Text);

            if (!valid)
            {
                e.Cancel = true;
                validationErrorProvider.SetError(imageNameTextBox, "File does not exist");
                loadButton.Enabled = false;
            }
        }

        private void imageNameTextBox_Validated(object sender, EventArgs e)
        {
            LoadImage(imageNameTextBox.Text);
            validationErrorProvider.SetError(imageNameTextBox, "");
            loadButton.Enabled = true;
        }

    }
}
