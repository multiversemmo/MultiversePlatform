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

namespace Multiverse.Tools.TerrainGenerator
{
    public partial class NewHeightmapDialog : Form
    {
        private TerrainGenerator genApp;

        public NewHeightmapDialog(TerrainGenerator gen)
        {
            InitializeComponent();
            genApp = gen;
        }

        private void heightmapCancelButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void heightmapCreateButton_Click(object sender, EventArgs e)
        {
            int sizeX = (int)heightmapWidthUpDown.Value;
            int sizeZ = (int)heightmapHeightUpDown.Value;
            float height = (float)heightmapDefaultHeightUpDown.Value;
            int metersPerSample = (int)heightmapMetersPerSampleUpDown.Value;
            bool center = centerHeightmapCheckbox.Checked;

            genApp.NewSeedMap(sizeX, sizeZ, height, metersPerSample, center);

            this.Hide();
        }
    }
}
