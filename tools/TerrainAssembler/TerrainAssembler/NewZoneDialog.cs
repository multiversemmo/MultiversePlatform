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
    public partial class NewZoneDialog : Form
    {
        protected Image heightmap;
        protected int tilesWidth;
        protected int tilesHeight;

        public NewZoneDialog()
        {
            InitializeComponent();

            mpsComboBox.SelectedIndex = 0;
            heightmapSizeValueLabel.Text = "";
            zoneSizeValueLabel.Text = "";
        }

        protected bool ValidateFilename(string name)
        {
            bool exists = System.IO.File.Exists(name);

            return exists;
        }

        protected void LoadHeightmap(string name)
        {
            heightmap = new Image(name);

            UpdateHeightmapInfo();
        }

        protected void UpdateHeightmapInfo()
        {
            if (heightmap != null)
            {
                tilesWidth = ((heightmap.Width * MetersPerSample * WorldMap.oneMeter) + WorldMap.tileSize - 1 ) / WorldMap.tileSize;
                tilesHeight = ((heightmap.Height * MetersPerSample * WorldMap.oneMeter) + WorldMap.tileSize - 1 ) / WorldMap.tileSize;

                heightmapSizeValueLabel.Text = String.Format("{0}x{1}", heightmap.Width, heightmap.Height);
                zoneSizeValueLabel.Text = String.Format("{0}x{1}", tilesWidth, tilesHeight);
            }
        }

        #region Properties

        public Image Heightmap
        {
            get
            {
                return heightmap;
            }
        }

        public string ZoneName
        {
            get
            {
                return zoneNameTextBox.Text;
            }
        }

        public string HeightmapName
        {
            get
            {
                return heightmapImageTextBox.Text;
            }
        }

        public int MetersPerSample
        {
            get
            {
                return int.Parse(mpsComboBox.Text);
            }
        }

        public float MinTerrainHeight
        {
            get
            {
                return float.Parse(minHeightTextBox.Text) * 1000;
            }
        }

        public float MaxTerrainHeight
        {
            get
            {
                return float.Parse(maxHeightTextBox.Text) * 1000;
            }
        }

        public NewZoneData NewZoneData
        {
            get
            {
                return new NewZoneData(ZoneName, HeightmapName, Heightmap, MetersPerSample, tilesWidth, tilesHeight, MinTerrainHeight, MaxTerrainHeight);
            }
        }

        #endregion Properties

        private void zoneNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if ((zoneNameTextBox.Text == null) || (zoneNameTextBox.Text == ""))
            {
                e.Cancel = true;
                validationErrorProvider.SetError(zoneNameTextBox, "Zone name required");
            }
        }

        private void zoneNameTextBox_Validated(object sender, EventArgs e)
        {
            validationErrorProvider.SetError(zoneNameTextBox, "");
        }

        private void browseHeightmapButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select heightmap image";
                dlg.DefaultExt = "png";
                dlg.Filter = "PNG Image File (*.png)|*.png";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    heightmapImageTextBox.Text = dlg.FileName;

                    if (ValidateFilename(dlg.FileName))
                    {
                        LoadHeightmap(dlg.FileName);
                        validationErrorProvider.SetError(heightmapImageTextBox, "");
                        createButton.Enabled = true;
                    }
                    else
                    {
                        validationErrorProvider.SetError(heightmapImageTextBox, "File does not exist");
                        createButton.Enabled = false;
                    }
                }
            }
        }

        private void mpsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHeightmapInfo();
        }

        private void heightmapImageTextBox_Validating(object sender, CancelEventArgs e)
        {
            bool valid = ValidateFilename(heightmapImageTextBox.Text);

            if (!valid)
            {
                e.Cancel = true;
                validationErrorProvider.SetError(heightmapImageTextBox, "File does not exist");
                createButton.Enabled = false;
            }
        }

        private void heightmapImageTextBox_Validated(object sender, EventArgs e)
        {
            LoadHeightmap(heightmapImageTextBox.Text);
            validationErrorProvider.SetError(heightmapImageTextBox, "");
            createButton.Enabled = true;
        }

        private void minHeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            float result;
            bool valid = float.TryParse(minHeightTextBox.Text, out result);
            if (!valid)
            {
                e.Cancel = true;
                validationErrorProvider.SetError(minHeightTextBox, "Invalid floating point value");
            }
        }

        private void minHeightTextBox_Validated(object sender, EventArgs e)
        {
            validationErrorProvider.SetError(minHeightTextBox, "");
        }

        private void maxHeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            float result;
            bool valid = float.TryParse(maxHeightTextBox.Text, out result);
            if (!valid)
            {
                e.Cancel = true;
                validationErrorProvider.SetError(maxHeightTextBox, "Invalid floating point value");
            }
        }

        private void maxHeightTextBox_Validated(object sender, EventArgs e)
        {
            validationErrorProvider.SetError(maxHeightTextBox, "");
        }
    }

    public struct NewZoneData
    {
        public string ZoneName;
        public string HeightmapFilename;
        public Image Heightmap;
        public int MetersPerSample;
        public int TilesWidth;
        public int TilesHeight;
        public float MinHeight;
        public float MaxHeight;

        public NewZoneData(string zoneName, string heightmapFilename, Image heightmap, int metersPerSample, 
            int tilesWidth, int tilesHeight, float minHeight, float maxHeight)
        {
            this.ZoneName = zoneName;
            this.HeightmapFilename = heightmapFilename;
            this.Heightmap = heightmap;
            this.MetersPerSample = metersPerSample;
            this.TilesWidth = tilesWidth;
            this.TilesHeight = tilesHeight;
            this.MinHeight = minHeight;
            this.MaxHeight = maxHeight;
        }
    }
}
