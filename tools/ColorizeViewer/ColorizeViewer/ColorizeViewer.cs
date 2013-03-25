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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Multiverse.Lib;
using System.Xml;
using System.IO;

namespace ColorizeViewer
{
    public enum RefColorMode
    {
        Average = 0,
        Color0 = 1,
        Color1 = 2,
        Color2 = 3,
        Color3 = 4
    }

    public delegate Color ColorTransform(Color src);

    public partial class ColorizeViewer : Form
    {
        protected DDSFile dds = null;
        protected HSVColor[] baseColors;
        protected HSVColor[] fileColors;
        protected Button[] colorButtons;

        protected ColorTransform colorizePixel;
        protected string currentTileName = null;
        protected Bitmap tileBitmap = null;
        protected float hAdjust = 0f;
        protected float sMult = 1f;
        protected float vMult = 1f;
        protected ContextMenuStrip refColorMenu;

        protected List<HSVColor> refColors;
        protected RefColorMode refColorMode = RefColorMode.Average;

        protected ColorLibrary library;
 
        public ColorizeViewer()
        {
            InitializeComponent();
            colorizePixel = new ColorTransform(ColorizePixelExpandMult);
            colorizeModeCombobox.SelectedIndex = 1;

            refColorModeComboBox.SelectedIndex = 0;

            InitRefColorMenu();

            library = new ColorLibrary();

            // initialize color array
            baseColors = new HSVColor[4];
            baseColors[0] = HSVColor.FromRGB(255, 0, 0);
            baseColors[1] = HSVColor.FromRGB(0, 255, 0);
            baseColors[2] = HSVColor.FromRGB(0, 0, 255);
            baseColors[3] = HSVColor.FromRGB(255, 255, 255);

            fileColors = new HSVColor[4];

            // save buttons in an array for easy lookup
            colorButtons = new Button[4];
            colorButtons[0] = color0Button;
            colorButtons[1] = color1Button;
            colorButtons[2] = color2Button;
            colorButtons[3] = color3Button;

            // init button background colors
            UpdateButtons();

            
        }

        protected void AddColorItem(int red, int green, int blue)
        {
            Color c = Color.FromArgb(red, green, blue);
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.BackColor = c;
            item.Text = String.Format("{0}, {1}, {2}", red, green, blue);
            item.Click += new EventHandler(refColorMenuItem_Click);
            refColorMenu.Items.Add(item);

            refColors.Add(HSVColor.FromRGB(red, green, blue));
        }

        void refColorMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            refColorButton.BackColor = item.BackColor;
        }

        protected void InitRefColorMenu()
        {
            refColorMenu = new ContextMenuStrip();
            refColors = new List<HSVColor>();
            
            AddColorItem(251, 174, 174);
            AddColorItem(253, 248, 175);
            AddColorItem(196, 206, 255);
            AddColorItem(154, 208, 253);
            AddColorItem(147, 254, 234);
            AddColorItem(123, 255, 145);

            AddColorItem(190, 125, 215);
            AddColorItem(253, 244, 99);
            AddColorItem(192, 165, 136);
            AddColorItem(108, 157, 224);
            AddColorItem(25, 167, 197);
            AddColorItem(64, 181, 77);

            AddColorItem(249, 64, 249);
            AddColorItem(248, 223, 17);
            AddColorItem(255, 162, 72);
            AddColorItem(56, 130, 255);
            AddColorItem(82, 155, 138);
            AddColorItem(75, 113, 62);

            AddColorItem(116, 46, 163);
            AddColorItem(255, 87, 48);
            AddColorItem(170, 116, 42);
            AddColorItem(57, 67, 40);
            AddColorItem(41, 122, 87);
            AddColorItem(49, 97, 13);

            AddColorItem(64, 49, 82);
            AddColorItem(237, 16, 16);
            AddColorItem(126, 15, 56);
            AddColorItem(54, 25, 19);
            AddColorItem(24, 86, 99);
            AddColorItem(12, 29, 49);
        }

        public bool AdjustActive
        {
            get
            {
                return (hAdjust != 0f) || (sMult != 1f) || (vMult != 1f);
            }
        }

        public void ResetAdjust()
        {
            hAdjust = 0f;
            sMult = 1f;
            vMult = 1f;

            hueAdjustTrackBar.Value = 0;

            saturationTrackBar.Value = 50;

            valueTrackBar.Value = 50;
        }

        /// <summary>
        /// Set the base colors to the currently adjusted colors, and then reset the
        /// adjustments.
        /// </summary>
        public void BakeAdjust()
        {
            for (int i = 0; i < 4; i++)
            {
                baseColors[i] = AdjustedColor(i);
            }
            ResetAdjust();
        }

        public HSVColor AdjustedColor(int index)
        {
            HSVColor baseColor = baseColors[index];
            float h = baseColor.H + hAdjust;
            float s = baseColor.S * sMult;
            float v = baseColor.V * vMult;

            // keep h in [0..360)
            while (h < 0f)
            {
                h = h + 360f;
            }
            while (h >= 360f)
            {
                h = h - 360f;
            }

            // clamp s to 1.0
            if (s > 1f)
            {
                s = 1f;
            }

            if (v > 1f)
            {
                v = 1f;
            }

            return new HSVColor(h, s, v);
        }

        public void UpdateButtons()
        {
            // init button background colors
            color0Button.BackColor = AdjustedColor(0).Color;
            color1Button.BackColor = AdjustedColor(1).Color;
            color2Button.BackColor = AdjustedColor(2).Color;
            color3Button.BackColor = AdjustedColor(3).Color;
        }

        /// <summary>
        /// Colorize a single pixel using a simple multiply
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        public Color ColorizePixelMult(Color pixel)
        {
            float c0mult = pixel.R / 255.0f;
            float c1mult = pixel.G / 255.0f;
            float c2mult = pixel.B / 255.0f;
            float c3mult = pixel.A / 255.0f;

            Color c0 = AdjustedColor(0).Color;
            Color c1 = AdjustedColor(1).Color;
            Color c2 = AdjustedColor(2).Color;
            Color c3 = AdjustedColor(3).Color;

            float r = c0.R * c0mult + c1.R * c1mult + c2.R * c2mult + c3.R * c3mult;
            float g = c0.G * c0mult + c1.G * c1mult + c2.G * c2mult + c3.G * c3mult;
            float b = c0.B * c0mult + c1.B * c1mult + c2.B * c2mult + c3.B * c3mult;
            float a = c0.A * c0mult + c1.A * c1mult + c2.A * c2mult + c3.A * c3mult;

            int resr = (int)r;
            int resg = (int)g;
            int resb = (int)b;
            int resa = (int)a;

            if (resr > 255)
            {
                resr = 255;
            }
            if (resg > 255)
            {
                resg = 255;
            }
            if (resb > 255)
            {
                resb = 255;
            }
            if (resa > 255)
            {
                resa = 255;
            }

            return Color.FromArgb(resa, resr, resg, resb);
        }

        /// <summary>
        /// Colorize a single pixel using the expanded multiplication method
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        public Color ColorizePixelExpandMult(Color pixel)
        {
            float c0mult = pixel.R / 255.0f * 2.0f;
            float c1mult = pixel.G / 255.0f * 2.0f;
            float c2mult = pixel.B / 255.0f * 2.0f;
            float c3mult = pixel.A / 255.0f * 2.0f;

            Color c0 = AdjustedColor(0).Color;
            Color c1 = AdjustedColor(1).Color;
            Color c2 = AdjustedColor(2).Color;
            Color c3 = AdjustedColor(3).Color;

            float r = c0.R * c0mult + c1.R * c1mult + c2.R * c2mult + c3.R * c3mult;
            float g = c0.G * c0mult + c1.G * c1mult + c2.G * c2mult + c3.G * c3mult;
            float b = c0.B * c0mult + c1.B * c1mult + c2.B * c2mult + c3.B * c3mult;
            float a = c0.A * c0mult + c1.A * c1mult + c2.A * c2mult + c3.A * c3mult;

            int resr = (int)r;
            int resg = (int)g;
            int resb = (int)b;
            int resa = (int)a;

            if (resr > 255)
            {
                resr = 255;
            }
            if (resg > 255)
            {
                resg = 255;
            }
            if (resb > 255)
            {
                resb = 255;
            }
            if (resa > 255)
            {
                resa = 255;
            }

            return Color.FromArgb(resa, resr, resg, resb);
        }

        /// <summary>
        /// Colorize the current tile with the current colors and colorization method.
        /// </summary>
        public void ColorizeTile()
        {
            if (dds != null)
            {
                tileBitmap = new Bitmap(dds.Width, dds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                for (int y = 0; y < dds.Height; y++)
                {
                    for (int x = 0; x < dds.Width; x++)
                    {
                        tileBitmap.SetPixel(x, y, colorizePixel(dds.GetColor(x, y)));
                    }
                }
                pictureBox1.Image = tileBitmap;
            }
        }

        /// <summary>
        /// Swap two of the current colors, and recolorize the current tile.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        private void SwapColors(int src, int dest)
        {
            HSVColor tmpColor;

            // swap colors
            tmpColor = baseColors[dest];
            baseColors[dest] = baseColors[src];
            baseColors[src] = tmpColor;

            UpdateButtons();

            ColorizeTile();
        }

        protected void writeColorMetadata(XmlWriter w, int colornum, Color c)
        {
            w.WriteStartElement("Color");
            w.WriteAttributeString("ColorNumber", colornum.ToString());
            w.WriteAttributeString("R", (c.R / 255.0f).ToString());
            w.WriteAttributeString("G", (c.G / 255.0f).ToString());
            w.WriteAttributeString("B", (c.B / 255.0f).ToString());
            w.WriteAttributeString("A", (c.A / 255.0f).ToString());
            w.WriteEndElement();
        }

        protected void saveMetadata(string filename)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;

            XmlWriter w = XmlWriter.Create(filename, xmlWriterSettings);
            w.WriteStartElement("TileColor");
            w.WriteAttributeString("TileName", Path.GetFileNameWithoutExtension(currentTileName));
            writeColorMetadata(w, 0, AdjustedColor(0).Color);
            writeColorMetadata(w, 1, AdjustedColor(1).Color);
            writeColorMetadata(w, 2, AdjustedColor(2).Color);
            writeColorMetadata(w, 3, AdjustedColor(3).Color);
            w.WriteEndElement();
            w.Close();
        }

        protected HSVColor AverageColor()
        {

            // rotate the colors around so that color0 is at h=180 before averaging
            float normDiff = 180f - baseColors[0].H;
            HSVColor[] normColors = new HSVColor[4];
            for (int i = 0; i < 4; i++)
            {
                float nh = baseColors[i].H + normDiff;
                if (nh < 0f)
                {
                    nh = nh + 360f;
                }
                if (nh >= 360f)
                {
                    nh = nh - 360f;
                }
                normColors[i] = new HSVColor(nh, baseColors[i].S, baseColors[i].V);
            }

            // compute average color
            float avH = 0f;
            float avS = 0f;
            float avV = 0f;
            for (int i = 0; i < 4; i++)
            {
                avH = avH + normColors[i].H;
                avS = avS + normColors[i].S;
                avV = avV + normColors[i].V;
            }

            avH = avH / 4f;
            avS = avS / 4f;
            avV = avV / 4f;

            // shift H back to original location
            avH = avH - normDiff;
            if (avH < 0f)
            {
                avH = avH + 360f;
            }
            if (avH >= 360f)
            {
                avH = avH - 360f;
            }

            return new HSVColor(avH, avS, avV);
        }

        protected HSVColor[] ComputeRefColors(HSVColor refColor)
        {
            HSVColor applyTo;

            switch (refColorMode)
            {
                case RefColorMode.Average:
                default:
                    applyTo = AverageColor();
                    break;
                case RefColorMode.Color0:
                    applyTo = baseColors[0];
                    break;
                case RefColorMode.Color1:
                    applyTo = baseColors[1];
                    break;
                case RefColorMode.Color2:
                    applyTo = baseColors[2];
                    break;
                case RefColorMode.Color3:
                    applyTo = baseColors[3];
                    break;
            }

            float deltaH = refColor.H - applyTo.H;
            float deltaS = refColor.S - applyTo.S;

            HSVColor[] retColors = new HSVColor[4];

            for (int i = 0; i < 4; i++)
            {
                float h = baseColors[i].H + deltaH;
                float s = baseColors[i].S + deltaS;

                // keep h in [0..360)
                while (h < 0f)
                {
                    h = h + 360f;
                }
                while (h >= 360f)
                {
                    h = h - 360f;
                }

                // clamp s to [0..1]
                if (s > 1f)
                {
                    s = 1f;
                }
                if (s < 0f)
                {
                    s = 0f;
                }

                retColors[i] = new HSVColor(h, s, baseColors[i].V);
            }
            return retColors;
        }

        protected void SetAllColors(HSVColor[] colors)
        {
            for (int i = 0; i < 4; i++)
            {
                baseColors[i] = colors[i];
            }

            UpdateButtons();
            ColorizeTile();
        }

        protected void ApplyRefColor()
        {
            // extract reference color from button
            HSVColor refColor = HSVColor.FromColor(refColorButton.BackColor);

            HSVColor[] colors = ComputeRefColors(refColor);
            SetAllColors(colors);
        }

        /// <summary>
        /// Compute all the reference colors for the current tile, and save
        /// them in the library.
        /// </summary>
        protected void TileRefColorsToLibrary(string tilename)
        {
            library.AddEntry(tilename, "original", fileColors);
            for (int i = 0; i < refColors.Count; i++)
            {
                HSVColor[] colors = ComputeRefColors(refColors[i]);

                library.AddEntry(tilename, "ref" + i.ToString(), colors);
            }
        }

        protected void LibraryToTreeview()
        {
            libraryTreeView.Nodes.Clear();
            //TreeNode rootNode = libraryTreeView.Nodes.Add("Colors");

            foreach (string tilename in library.GetTileNames())
            {
                TreeNode tileNode = libraryTreeView.Nodes.Add(tilename);
                foreach (string colorname in library.GetColorNames(tilename))
                {
                    TreeNode colorNode = new TreeNode(colorname);
                    colorNode.Tag = library.GetEntry(tilename, colorname);
                    tileNode.Nodes.Add(colorNode);
                }
            }
        }

        private void loadTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Load Tile";
                dlg.DefaultExt = "dds";
                dlg.Filter = "DDS Image Files (*.dds)|*.dds|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    currentTileName = dlg.FileName;
                    dds = DDSFile.LoadFile(dlg.FileName);

                    ColorizeTile();
                }
            }

        }

        private void PickColor(int index)
        {
            if (AdjustActive)
            {
                MessageBox.Show("Colors may not be set while adjustment is active");
            }
            else
            {
                using (ColorDialog dlg = new ColorDialog())
                {
                    dlg.AllowFullOpen = true;
                    dlg.FullOpen = true;
                    dlg.AnyColor = true;
                    dlg.Color = baseColors[index].Color;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        baseColors[index] = HSVColor.FromColor(dlg.Color);

                        UpdateButtons();
                        ColorizeTile();
                    }
                }
            }
        }

        private void color0Button_Click(object sender, EventArgs e)
        {
            PickColor(0);
        }

        private void color1Button_Click(object sender, EventArgs e)
        {
            PickColor(1);
        }

        private void color2Button_Click(object sender, EventArgs e)
        {
            PickColor(2);
        }

        private void color3Button_Click(object sender, EventArgs e)
        {
            PickColor(3);
        }

        private void colorizeModeCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (colorizeModeCombobox.SelectedIndex)
            {
                case 0:
                default:
                    colorizePixel = new ColorTransform(ColorizePixelMult);
                    break;
                case 1:
                    colorizePixel = new ColorTransform(ColorizePixelExpandMult);
                    break;

            }
            ColorizeTile();
        }

        private void saveColorizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTileName != null)
            {
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Title = "Save Colorized Image";
                    dlg.DefaultExt = "dds";
                    string outfilename = currentTileName.Replace(".dds", "-colorized.dds");
                    dlg.FileName = outfilename;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        DDSFile32 colorDDS = new DDSFile32(dds.Width, dds.Height);

                        for (int y = 0; y < dds.Height; y++)
                        {
                            for (int x = 0; x < dds.Width; x++)
                            {
                                Color c = tileBitmap.GetPixel(x, y);
                                colorDDS.SetColor(x, y, c);
                            }
                        }

                        colorDDS.Save(dlg.FileName);
                    }
                }
            }
            else
            {
                string msg = "No Tile Loaded";
                DialogResult result = MessageBox.Show(this, msg, "No Tile Loaded", MessageBoxButtons.OK);
            }
        }

        private void color0Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                color0Button.DoDragDrop(color0Button, DragDropEffects.Copy);
            }
        }


        private void color0Button_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Button)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void color0Button_DragDrop(object sender, DragEventArgs e)
        {
            Button srcButton = (Button)e.Data.GetData(typeof(Button));

            SwapColors(int.Parse((string)srcButton.Tag), 0);
        }

        private void color1Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                color1Button.DoDragDrop(color1Button, DragDropEffects.Copy);
            }
        }

        private void color1Button_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Button)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void color1Button_DragDrop(object sender, DragEventArgs e)
        {
            Button srcButton = (Button)e.Data.GetData(typeof(Button));

            SwapColors(int.Parse((string)srcButton.Tag), 1);

        }

        private void color2Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                color2Button.DoDragDrop(color2Button, DragDropEffects.Copy);
            }
        }

        private void color2Button_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Button)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void color2Button_DragDrop(object sender, DragEventArgs e)
        {
            Button srcButton = (Button)e.Data.GetData(typeof(Button));

            SwapColors(int.Parse((string)srcButton.Tag), 2);
        }

        private void color3Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                color3Button.DoDragDrop(color3Button, DragDropEffects.Copy);
            }
        }

        private void color3Button_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Button)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void color3Button_DragDrop(object sender, DragEventArgs e)
        {
            Button srcButton = (Button)e.Data.GetData(typeof(Button));

            SwapColors(int.Parse((string)srcButton.Tag), 3);
        }

        private void importImageAsTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Import Image As Tile";
                dlg.DefaultExt = "dds";
                dlg.Filter = "DDS Image Files (*.dds)|*.dds|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Cursor saveCursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;
                    TileMaker tm = new TileMaker(dlg.FileName, 4);

                    dds = new DDSFile16(tm.Width, tm.Height);
                    tm.FillTile(dds);

                    currentTileName = dlg.FileName;

                    // grab colors from tilemaker

                    ResetAdjust();
                    for (int i = 0; i < 4; i++)
                    {
                        fileColors[i] = baseColors[i] = tm.Colors[i].HSVColor;
                    }

                    UpdateButtons();

                    ColorizeTile();

                    this.Cursor = saveCursor;
                }
            }
        }

        private void importColorsFromImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Import Colors From Image";
                dlg.DefaultExt = "dds";
                dlg.Filter = "DDS Image Files (*.dds)|*.dds|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Cursor saveCursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;
                    TileMaker tm = new TileMaker(dlg.FileName, 4);

                    // grab colors from tilemaker

                    ResetAdjust();
                    for (int i = 0; i < 4; i++)
                    {
                        fileColors[i] = baseColors[i] = tm.Colors[i].HSVColor;
                    }

                    UpdateButtons();
                    ColorizeTile();

                    this.Cursor = saveCursor;
                }
            }
        }

        private void saveTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTileName != null)
            {
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Title = "Save Tile";
                    dlg.DefaultExt = "dds";
                    dlg.FileName = currentTileName;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        dds.Save(dlg.FileName);
                        currentTileName = dlg.FileName;
                    }
                }
            }
            else
            {
                string msg = "No Tile Loaded";
                DialogResult result = MessageBox.Show(this, msg, "No Tile Loaded", MessageBoxButtons.OK);
            }
        }

        private void saveTileAndColorMetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTileName != null)
            {
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Title = "Save Tile And Color Metadata";
                    dlg.DefaultExt = "xml";
                    dlg.FileName = currentTileName.Replace(".dds", ".xml");
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        saveMetadata(dlg.FileName);
                    }
                }
            }
            else
            {
                string msg = "No Tile Loaded";
                DialogResult result = MessageBox.Show(this, msg, "No Tile Loaded", MessageBoxButtons.OK);
            }
        }

        private void importImageAsTile8bitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Import Image As Tile";
                dlg.DefaultExt = "dds";
                dlg.Filter = "DDS Image Files (*.dds)|*.dds|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Cursor saveCursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;
                    TileMaker tm = new TileMaker(dlg.FileName, 4);

                    dds = new DDSFile32(tm.Width, tm.Height);
                    tm.FillTile(dds);

                    currentTileName = dlg.FileName;

                    // grab colors from tilemaker

                    for (int i = 0; i < 4; i++)
                    {
                        fileColors[i] = baseColors[i] = tm.Colors[i].HSVColor;
                    }

                    UpdateButtons();
                    ColorizeTile();

                    this.Cursor = saveCursor;
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            hAdjust = (float)hueAdjustTrackBar.Value;

            UpdateButtons();
            ColorizeTile();
        }

        private void saturationTrackBar_Scroll(object sender, EventArgs e)
        {
            float exponent = (float)saturationTrackBar.Value / 50f - 1f;
            sMult = (float)Math.Pow(10, exponent);

            UpdateButtons();
            ColorizeTile();
        }

        private void clearAdjustButton_Click(object sender, EventArgs e)
        {
            ResetAdjust();
            UpdateButtons();
            ColorizeTile();
        }

        private void bakeColorButton_Click(object sender, EventArgs e)
        {
            BakeAdjust();
            UpdateButtons();
            ColorizeTile();
        }

        private void refColorButton_Click(object sender, EventArgs e)
        {
            refColorMenu.Show(refColorButton, new System.Drawing.Point(0, refColorButton.Height));
        }

        private void refColorModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            refColorMode = (RefColorMode)refColorModeComboBox.SelectedIndex;
        }

        private void applyRefButton_Click(object sender, EventArgs e)
        {
            ResetAdjust();
            ApplyRefColor();
        }

        private void resetFileColors_Click(object sender, EventArgs e)
        {
            SetAllColors(fileColors);
        }

        private bool SaveColorLibrary()
        {
            bool ret = false;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Color Library";
                dlg.DefaultExt = "xml";
                dlg.FileName = "tileColorLibrary.xml";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    library.Save(dlg.FileName);
                }
                else
                {
                    ret = true;
                }
            }
            return ret;
        }

        private void saveColorLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveColorLibrary();
        }

        private void tileRefColorsToLibraryButton_Click(object sender, EventArgs e)
        {
            string tilename = Path.GetFileNameWithoutExtension(currentTileName);
            using (TilePrompt dlg = new TilePrompt())
            {
                dlg.TileName = tilename;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    tilename = dlg.TileName;
                    TileRefColorsToLibrary(tilename);

                    LibraryToTreeview();
                }
            }
        }

        // return true to cancel close
        public bool CheckClose(bool cancelbutton, string additionalMessage)
        {
            bool ret = false;
            if (library.Dirty)
            {
                MessageBoxButtons buttons;
                if (cancelbutton)
                {
                    buttons = MessageBoxButtons.YesNoCancel;
                }
                else
                {
                    buttons = MessageBoxButtons.YesNo;
                }
                DialogResult result = MessageBox.Show(additionalMessage + "Save unsaved changes to color library?", "Save Color library?", buttons);

                if (result == DialogResult.Cancel)
                {
                    ret = true;
                }
                else if (result == DialogResult.Yes)
                {
                    ret = SaveColorLibrary();
                }
            }
            return ret;
        }

        private void ColorizeViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CheckClose(true, "");
        }

        private void libraryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            HSVColor [] colors = libraryTreeView.SelectedNode.Tag as HSVColor[];
            if (colors != null)
            {
                ResetAdjust();
                SetAllColors(colors);
            }
        }

        private void loadColorLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Load Color Library";
                dlg.DefaultExt = "xml";
                dlg.FileName = "tileColorLibrary.xml";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    library.Clear();
                    library.Load(dlg.FileName);
                    LibraryToTreeview();
                }
            }
        }

        private void colorToLibraryButton_Click(object sender, EventArgs e)
        {
            HSVColor [] colors = new HSVColor[4];
            colors[0] = AdjustedColor(0);
            colors[1] = AdjustedColor(1);
            colors[2] = AdjustedColor(2);
            colors[3] = AdjustedColor(3);

            string tilename;
            string colorname;

            TreeNode selectedNode = libraryTreeView.SelectedNode;
            if (selectedNode.Tag != null)
            {
                colorname = selectedNode.Text;
                tilename = selectedNode.Parent.Text;
            }
            else
            {
                tilename = Path.GetFileNameWithoutExtension(currentTileName);
                colorname = "";
            }

            using (TileColorPrompt dlg = new TileColorPrompt())
            {
                dlg.TileName = tilename;
                dlg.ColorName = colorname;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    tilename = dlg.TileName;
                    colorname = dlg.ColorName;

                    library.AddEntry(tilename, colorname, colors);

                    LibraryToTreeview();
                }
            }
        }

        private void exportColorLibraryAsPythonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Export Color Library as Python";
                dlg.DefaultExt = "py";
                dlg.FileName = "TileColorLibrary.py";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    library.Export(dlg.FileName);
                }
            }
        }

        private void libraryTreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && libraryTreeView.SelectedNode != null)
            {
                if (libraryTreeView.SelectedNode.Tag == null)
                { // its a tile
                    DialogResult result = MessageBox.Show("Delete tile?", "Delete tile?", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        string tilename = libraryTreeView.SelectedNode.Text;
                        library.RemoveTile(tilename);
                    }
                }
                else
                { // its a color
                    DialogResult result = MessageBox.Show("Delete color?", "Delete color?", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        string colorname = libraryTreeView.SelectedNode.Text;
                        string tilename = libraryTreeView.SelectedNode.Parent.Text;
                        library.RemoveEntry(tilename, colorname);

                    }
                }
                libraryTreeView.SelectedNode.Remove();
                e.Handled = true;
            }
        }

        private void valueTrackBar_Scroll(object sender, EventArgs e)
        {
            float exponent = (float)valueTrackBar.Value / 50f - 1f;
            vMult = (float)Math.Pow(10, exponent);

            UpdateButtons();
            ColorizeTile();
        }

    }
}
