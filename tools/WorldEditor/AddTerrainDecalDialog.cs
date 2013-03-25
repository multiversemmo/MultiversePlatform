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
    public partial class AddTerrainDecalDialog : Form
    {
        protected WorldEditor app = WorldEditor.Instance;

        public AddTerrainDecalDialog()
        {
            InitializeComponent();
            this.SizeX = app.Config.TerrainDecalDefaultSize;
            this.SizeZ = app.Config.TerrainDecalDefaultSize;
            this.Priority = app.Config.TerrainDecalDefaultPriority;
        }

        public string ObjectName
        {
            get
            {
                return nameTextBox.Text;
            }
            set
            {
                nameTextBox.Text = value;
            }
        }

        public string Filename
        {
            get
            {
                return filenameTextBox.Text;
            }
            set
            {
                filenameTextBox.Text = value;
            }
        }

        public float SizeX
        {
            get
            {
                if(ValidityHelperClass.isFloat(sizeXTextBox.Text))
                {
                    return float.Parse(sizeXTextBox.Text);
                }
                return 0f;
            }
            set
            {
                sizeXTextBox.Text = value.ToString();
            }
        }

        public float SizeZ
        {
            get
            {
                if(ValidityHelperClass.isFloat(sizeZTextBox.Text))
                {
                    return float.Parse(sizeZTextBox.Text);
                }
                return 0f;
            }
            set
            {
                sizeZTextBox.Text = value.ToString();
            }
        }

        public int Priority
        {
            get
            {
                if(ValidityHelperClass.isInt(priorityTextBox.Text))
                {
                       return int.Parse(priorityTextBox.Text);
                }
                return 0;
            }
            set
            {
                priorityTextBox.Text = value.ToString();
            }
        }

        private void floatVerifyevent(object sender, EventArgs e)
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

        private void intVerifyevent(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)sender;

            if (!ValidityHelperClass.isInt(textbox.Text))
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

        private void onHelpButtonClicked(object sender, EventArgs e)
        {
            Button but = sender as Button;
            WorldEditor.LaunchDoc(but.Tag as string);
        }

        private void verifyAssetExits(object sender, EventArgs e)
        {
            if (!app.CheckAssetFileExists((sender as TextBox).Text))
            {
                Color textColor = Color.Red;
                (sender as TextBox).ForeColor = textColor;
            }
            else
            {
                Color textColor = Color.Black;
                (sender as TextBox).ForeColor = textColor;
            }
        }


        public bool okButton_validating()
        {
            if (!ValidityHelperClass.assetExists(filenameTextBox.Text))
            {
                MessageBox.Show("The filename selected does not exist in the asset repository", "Invalid Filename", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                if (!ValidityHelperClass.isFloat(sizeXTextBox.Text))
                {
                    MessageBox.Show("The SizeX entered can not be parsed to a floating point number", "Invalid size", MessageBoxButtons.OK);
                    return false;
                }
                else
                {
                    if (!ValidityHelperClass.isFloat(sizeZTextBox.Text))
                    {
                        MessageBox.Show("The SizeZ entered can not be parsed to a floating point number", "Invalid size", MessageBoxButtons.OK);
                        return false;
                    }
                    else
                    {
                        if (!ValidityHelperClass.isInt(priorityTextBox.Text))
                        {
                            MessageBox.Show("The priority entered can not be parsed to an integer number", "Invalid Priority", MessageBoxButtons.OK);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void pickDecalImageButton_Click(object sender, EventArgs e)
        {
            TexturePickerDialog dlg = new TexturePickerDialog(filenameTextBox.Text, "TerrainDecal");
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filenameTextBox.Text = dlg.TextureComboBoxString;
            }
        }
    }
}
