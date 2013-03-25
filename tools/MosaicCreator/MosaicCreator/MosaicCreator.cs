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
using System.IO;
using System.Windows.Forms;
using Tao.DevIl;

namespace Multiverse.Tools.MosaicCreator
{

    public partial class MosaicCreator : Form
    {
        protected Image src;

        public MosaicCreator()
        {
            InitializeComponent();

            if (tileSizeComboBox.SelectedItem == null)
            {
                tileSizeComboBox.SelectedItem = "512";
            }

            if (mpsComboBox.SelectedItem == null)
            {
                mpsComboBox.SelectedItem = "4";
            }
        }

        protected void WriteMMF(string mosaicName, int tileSize, int tileW, int tileH)
        {
            int baseStart = mosaicName.LastIndexOf('\\');
            int extOff = mosaicName.LastIndexOf('.');
            string basename = mosaicName.Substring(baseStart + 1, extOff - baseStart - 1);

            StreamWriter w = new StreamWriter(mosaicName);
            bool isHeightmap = heightMapCheckbox.Checked;

            w.WriteLine("L3DT Mosaic master file");
            w.WriteLine("#MosaicName:\t{0}", basename);
            w.WriteLine("#MosaicType:\t{0}", "XXX");
            w.WriteLine("#FileExt:\t{0}", "png");
            w.WriteLine("#nPxlsX:\t{0}", tileW * tileSize);
            w.WriteLine("#nPxlsY:\t{0}", tileH * tileSize);
            w.WriteLine("#nMapsX:\t{0}", tileW);
            w.WriteLine("#nMapsY:\t{0}", tileH);
            w.WriteLine("#SubMapSize:\t{0}", tileSize);
            w.WriteLine("#HorizScale:\t{0}", mpsComboBox.Text);
            w.WriteLine("#WrapFlag:\tFALSE");

            if (isHeightmap)
            {
                w.WriteLine("#UnifiedScale:\tTRUE");
                w.WriteLine("#GlobalMinAlt:\t{0}", minAltTextBox.Text);
                w.WriteLine("#GlobalMaxAlt:\t{0}", maxAltTextBox.Text);
            }

            for (int i = 0; i < tileW * tileH; i++)
            {
                w.WriteLine("#TileState:\t{0}\tOK", i);
            }
            w.WriteLine("#EOF");

            w.Close();
        }

        protected void CreateMosaic(string mosaicName)
        {
            
            //src.Save("temp.png");
            //return;

            if (src != null)
            {
                string basename = mosaicName.Substring(0, mosaicName.LastIndexOf('.'));

                int tileSize = int.Parse(tileSizeComboBox.Text);
                int tileW = src.Width / tileSize;
                int tileH = src.Height / tileSize;

                if ((tileW * tileSize) < src.Width)
                {
                    tileW++;
                }
                if ((tileH * tileSize) < src.Height)
                {
                    tileH++;
                }

                for (int tileY = 0; tileY < tileH; tileY++)
                {
                    for (int tileX = 0; tileX < tileW; tileX++)
                    {
                        Image dest = new Image(src, tileX * tileSize, tileY * tileSize, tileSize, tileSize);

                        dest.Save(string.Format("{0}_x{1}y{2}.png", basename, tileX, tileY));
                    }
                }

                WriteMMF(mosaicName, tileSize, tileW, tileH);
            }

        }

        private void heightMapCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            altPanel.Enabled = heightMapCheckbox.Checked;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Source Image";
                dlg.DefaultExt = "png";
                dlg.Filter = "PNG Image File (*.png)|*.png";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    sourceImageFilenameLabel.Text = dlg.FileName;

                    src = new Image(dlg.FileName);

                    sourceImageInfoLabel.Text = string.Format("Pixel Format: {0}\nWidth: {1}\nHeight: {2}", src.PixelFormatName, src.Width, src.Height);
                }
            }
        }

        private void launchOnlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string target = "http://update.multiverse.net/wiki/index.php/Using_Mosaic_Creator_Version_1.0";
            System.Diagnostics.Process.Start(target);
        }

        private void launchReleaseNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string releaseNotesURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.0_Release_Notes";
            System.Diagnostics.Process.Start(releaseNotesURL); 
        }

        private void submitFeedbackOrABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string feedbackURL = "http://www.multiverse.net/developer/feedback.jsp?Tool=MosaicCreator";
            System.Diagnostics.Process.Start(feedbackURL);
        }

        private void aboutMosaicCreatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string msg = string.Format("Multiverse Mosaic Creator\n\nVersion: {0}\n\nCopyright 2006-2007 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse Mosaic Creator", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        }

        private void saveMosaicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Mosaic";
                dlg.DefaultExt = "mmf";
                dlg.Filter = "Image Mosaic (*.mmf)|*.mmf";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    CreateMosaic(dlg.FileName);
                }
            }
        }
    }
}
