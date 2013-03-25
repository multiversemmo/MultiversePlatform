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

namespace Multiverse.Tools.TerrainAssembler
{
    public partial class NewMapDialog : Form
    {
        protected Dictionary<string, int> units;

        public NewMapDialog()
        {
            InitializeComponent();

            units = new Dictionary<string, int>();

            units["Millimeters"] = 1;
            units["Meters"] = 1000;
            units["Kilometers"] = 1000000;

            // set default values for unit selection menus
            widthUnitsComboBox.SelectedIndex = 1;
            heightUnitsComboBox.SelectedIndex = 1;
            minHeightUnitsComboBox.SelectedIndex = 1;
            maxHeightUnitsComboBox.SelectedIndex = 1;
            defaultHeightUnitsComboBox.SelectedIndex = 1;

        }

        #region Properties

        public string MapName
        {
            get
            {
                return mapNameTextBox.Text;
            }
        }

        public int WidthUnits
        {
            get
            {
                return units[widthUnitsComboBox.Text];
            }
        }

        public int HeightUnits
        {
            get
            {
                return units[heightUnitsComboBox.Text];
            }
        }

        public int MapWidth
        {
            get
            {
                float width = float.Parse(mapWidthTextBox.Text);
                return (int)(WidthUnits * width);
            }
        }

        public int MapHeight
        {
            get
            {
                float height = float.Parse(mapHeightTextBox.Text);
                return (int)(HeightUnits * height);
            }
        }

        public float MinTerrainHeight
        {
            get
            {
                return float.Parse(minHeightTextBox.Text) * units[minHeightUnitsComboBox.Text];
            }
        }

        public float MaxTerrainHeight
        {
            get
            {
                return float.Parse(maxHeightTextBox.Text) * units[maxHeightUnitsComboBox.Text];
            }
        }

        public float DefaultTerrainHeight
        {
            get
            {
                return float.Parse(defaultHeightTextBox.Text);
            }
        }
        #endregion Properties

        private void mapWidthTextBox_Validating(object sender, CancelEventArgs e)
        {
            float tmp;
            if (float.TryParse(mapWidthTextBox.Text, out tmp) == false)
            {
                e.Cancel = true;
                errorProvider1.SetError(mapWidthTextBox, "invalid numeric value");
            }
        }

        private void mapWidthTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(mapWidthTextBox, "");
        }

        private void mapHeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            float tmp;
            if (float.TryParse(mapHeightTextBox.Text, out tmp) == false)
            {
                e.Cancel = true;
                errorProvider1.SetError(mapHeightTextBox, "invalid numeric value");
            }
        }

        private void mapHeightTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(mapHeightTextBox, "");
        }

        private void minHeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            float tmp;
            if (float.TryParse(minHeightTextBox.Text, out tmp) == false)
            {
                e.Cancel = true;
                errorProvider1.SetError(minHeightTextBox, "invalid numeric value");
            }
        }

        private void minHeightTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(minHeightTextBox, "");
        }

        private void maxHeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            float tmp;
            if (float.TryParse(maxHeightTextBox.Text, out tmp) == false)
            {
                e.Cancel = true;
                errorProvider1.SetError(maxHeightTextBox, "invalid numeric value");
            }
        }

        private void maxHeightTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(maxHeightTextBox, "");
        }

        private void defaultHeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            float tmp;
            if (float.TryParse(defaultHeightTextBox.Text, out tmp) == false)
            {
                e.Cancel = true;
                errorProvider1.SetError(defaultHeightTextBox, "invalid numeric value");
            }
        }

        private void defaultHeightTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(defaultHeightTextBox, "");
        }

        private void mapNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if ((mapNameTextBox.Text == null) || (mapNameTextBox.Text == ""))
            {
                e.Cancel = true;
                errorProvider1.SetError(mapNameTextBox, "Map name required");
            }
        }

        private void mapNameTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(mapNameTextBox, "");
        }

    }
}
