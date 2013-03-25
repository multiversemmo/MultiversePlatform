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

namespace Multiverse.Tools.WorldEditor
{
    public partial class TexturePickerDialog : Form
    {
        WorldEditor app;

        public TexturePickerDialog(string texture, string subType)
        {
            app = WorldEditor.Instance;
            InitializeComponent();
            bool found = false;
            foreach (AssetDesc desc in app.Assets.Select("Texture"))
            {
                if (String.Equals(desc.Type, "Texture") && String.Equals(desc.SubType, subType))
                {
                    textureComboBox.Items.Add(desc.AssetName);
                }
            }
            if (!String.Equals(texture, ""))
            {
                foreach (string tex in textureComboBox.Items)
                {
                    if (String.Equals(tex, texture))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    textureComboBox.Items.Add(texture);
                }
                textureComboBox.SelectedIndex = textureComboBox.Items.IndexOf(texture);
            }
            else
            {
                if (textureComboBox.Items.Count > 0)
                {
                    textureComboBox.SelectedIndex = 0;
                }
            }
        }

        public string TextureComboBoxString
        {
            get
            {
                return (string)(textureComboBox.SelectedItem);
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            string[] extensions = { "dds", "tga", "jpg", "png", "bmp" };
            OpenFileDialog dlg = new OpenFileDialog();
            string filter = "Image Files(";
            foreach(string ext in extensions)
            {
                filter += String.Format("*.{0}", ext);
            }
            filter += ")|";
            dlg.Filter = filter;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.InitialDirectory = String.Format("{0}\\Textures", RepositoryClass.Instance.FirstRepositoryPath);
            if (dlg.ShowDialog() == DialogResult.OK && WorldEditor.Instance.CheckAssetFileExists(dlg.FileName.Substring(dlg.FileName.LastIndexOf('\\') + 1)))
            {
                textureComboBox.Items.Add(dlg.FileName.Substring(dlg.FileName.LastIndexOf('\\') + 1));
            }
        }
    }
}
