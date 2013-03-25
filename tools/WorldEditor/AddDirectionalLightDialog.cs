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
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
    public partial class AddDirectionalLightDialog : Form
    {
        ColorEx specular = new ColorEx();
        ColorEx diffuse = new ColorEx();
 
        public AddDirectionalLightDialog(ColorEx specular, ColorEx diffuse)
        {
            this.specular = specular;
            this.diffuse = diffuse;
            InitializeComponent();
            specularSelectButton.BackColor = ColorExToColor(specular);
            diffuseSelectButton.BackColor = ColorExToColor(diffuse);
        }

                
        private Color ColorExToColor(ColorEx cx)
        {
            return Color.FromArgb((int)cx.ToARGB());
        }

        private ColorEx ColorToColorEx(Color c)
        {
            return new ColorEx(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f);

        }
        public ColorEx Specular
        {
            get
            {
                return specular;
            }
            set
            {
                specular = value;
                specularSelectButton.BackColor = ColorExToColor(value);
            }
        }

        public ColorEx Diffuse
        {
            get
            {
                return diffuse;
            }
            set
            {
                diffuse = value;
                diffuseSelectButton.BackColor = ColorExToColor(value);
            }
        }

        private void specularSelectButton_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1;
            using (colorDialog1 = new ColorDialog())
            {
                colorDialog1.SolidColorOnly = false;
                colorDialog1.AllowFullOpen = true;
                colorDialog1.AnyColor = true;
                int colorabgr = (int)specular.ToABGR();
                colorabgr &= 0x00ffffff;
                int[] colorsabgr = new int[1];
                colorsabgr[0] = colorabgr;
                colorDialog1.FullOpen = true;
                colorDialog1.ShowHelp = true;
                colorDialog1.Color = ColorExToColor(specular);
                colorDialog1.CustomColors = colorsabgr;
                DialogResult result = colorDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    specularSelectButton.BackColor = colorDialog1.Color;
                    specular = ColorToColorEx(colorDialog1.Color);
                }
            }
        }

        private void diffuseSelectButton_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1;
            using (colorDialog1 = new ColorDialog())
            {
                colorDialog1.SolidColorOnly = false;
                colorDialog1.AllowFullOpen = true;
                colorDialog1.AnyColor = true;
                int colorabgr = (int)diffuse.ToABGR();
                colorabgr &= 0x00ffffff;
                int[] colorsabgr = new int[1];
                colorsabgr[0] = colorabgr;
                colorDialog1.FullOpen = true;
                colorDialog1.ShowHelp = true;
                colorDialog1.Color = ColorExToColor(diffuse);
                colorDialog1.CustomColors = colorsabgr;
                DialogResult result = colorDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    diffuseSelectButton.BackColor = colorDialog1.Color;
                    diffuse = ColorToColorEx(colorDialog1.Color);
                }
            }
        }


        private void onHelpButtonClicked(object sender, EventArgs e)
        {
            Button but = sender as Button;
            WorldEditor.LaunchDoc(but.Tag as string);
        }

    }
}
