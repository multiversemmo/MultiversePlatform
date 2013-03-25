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
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Vector3 = Axiom.MathLib.Vector3;
using Matrix3 = Axiom.MathLib.Matrix3;
using Quaternion = Axiom.MathLib.Quaternion;

namespace NormalBump
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
//             normalMapTextBox.Text = "C:\\Downloads\\foundationWallsNormal_map.bmp";
//             bumpMapTextBox.Text = "C:\\Downloads\\foundationWallsBumpMap.bmp";
//             outputMapTextBox.Text = "C:\\Downloads\\foundationsWallsCombined.bmp";
		}

        private void normalMapOpenButton_Click(object sender, EventArgs e)
        {
            if (normalMapOpenDialog.ShowDialog() == DialogResult.OK)
                normalMapTextBox.Text = normalMapOpenDialog.FileName;
        }

        private void bumpMapOpenButton_Click(object sender, EventArgs e)
        {
            if (bumpMapOpenDialog.ShowDialog() == DialogResult.OK)
                bumpMapTextBox.Text = bumpMapOpenDialog.FileName;
        }

        private void outputMapSaveButton_Click(object sender, EventArgs e)
        {
            if (outputMapSaveDialog.ShowDialog() == DialogResult.OK)
                outputMapTextBox.Text = outputMapSaveDialog.FileName;
        }

		private void ShowError(string format, params Object[] list)
        {
            string s = string.Format(format, list);
			MessageBox.Show (s, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

        private int bumpLevel(Bitmap bumpMap, int x, int y) 
		{
			Color bumpColor = bumpMap.GetPixel(x, y);
			return bumpColor.R;
		}
		
		private float colorToFloat(int c) 
		{
			return (float)(c - 128);
		}
		
		private int floatToColor(float c) 
		{
			return Math.Max(0, Math.Min(255, (int)((c + 0.5f) * 128)));
		}
		
        public static StreamWriter logStream;
        
		private void Log(string format, params Object[] list)
        {
            string s = string.Format(format, list);
			logStream.Write(s);
        }

		private void generateButton_Click(object sender, EventArgs e)
        {
            Cursor previousCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			doneLabel.Visible = false;
			//outputPictureBox.Visible = false;

			if (generateLogFile.Checked) {
				string p = "NormalBump.log";
				FileStream f = new FileStream(p, FileMode.Create, FileAccess.Write);
				logStream = new StreamWriter(f);
				logStream.Write(string.Format("{0} Started writing to {1}\n",
											  DateTime.Now.ToString("hh:mm:ss"), p));
			}
			
			// run the algorithm
			Bitmap normalMap = new Bitmap(normalMapTextBox.Text);
			Bitmap bumpMap = new Bitmap(bumpMapTextBox.Text);

			if (normalMap.Width != bumpMap.Width) {
				ShowError("Normal Map width {0} is not the same as Bump Map width {1}",
						  normalMap.Width, bumpMap.Width);
				return;
			}
			
			if (normalMap.Height != bumpMap.Height) {
				ShowError("Normal Map height {0} is not the same as Bump Map height {1}",
						  normalMap.Height, bumpMap.Height);
				return;
			}

            Bitmap outputMap = (Bitmap)normalMap.Clone();
			
			PixelFormat normalFormat = normalMap.PixelFormat;
			PixelFormat bumpFormat = bumpMap.PixelFormat;

			// This will be set by the slider
			float scaleFactor = (float)trackBar.Value / 100f;

			if (reverseBumpDirection.Checked)
				scaleFactor = - scaleFactor;
			
			Vector3 unitZ = new Vector3(0f, 0f, 1f);
			float epsilon = 0.0000001f;
			
			// Loop through the bump map pixels, computing the normals
			// into the output map
			int w = normalMap.Width;
			int h = normalMap.Height;
			for(int x=0; x < w; x++) {
				for(int y=0; y < h; y++) {
					// Fetch the normal map normal vector
					Color c = normalMap.GetPixel(x, y);
					Vector3 normal = new Vector3(colorToFloat(c.R), 
												 colorToFloat(c.G),
												 colorToFloat(c.B)).ToNormalized();
					Vector3 result = normal;

					// If we're at the edge, use the normal vector
					if (x < w - 1 && y < h - 1) {
						// Compute the bump normal vector
						int xyLevel = bumpLevel(bumpMap, x, y);
						float dx = scaleFactor * (bumpLevel(bumpMap, x+1, y) - xyLevel);
						float dy = scaleFactor * (bumpLevel(bumpMap, x, y+1) - xyLevel);
						float dz = 255f;
						Vector3 bumpNormal = new Vector3(dx, dy, dz).ToNormalized();
						if (generateLogFile.Checked)
							Log("X {0}, Y {1}, normal {2}, bumpNormal {3}\n",
								x, y, normal, bumpNormal);
						Vector3 axis = unitZ.Cross(normal);
						if (axis.Length > epsilon) {
							float cosAngle = unitZ.Dot(normal);
							float angle = (float)Math.Acos(cosAngle);
							Quaternion q = Quaternion.FromAngleAxis(angle, axis);
							Matrix3 rot = q.ToRotationMatrix();
							result = rot * bumpNormal;
							if (generateLogFile.Checked)
								Log("   Angle {0}, Quaternion {1}, Result {2}\n", angle, q, result);
						}
					}
                    Color resultColor = Color.FromArgb(floatToColor(result.x),
													   floatToColor(result.y),
													   floatToColor(result.z));
					outputMap.SetPixel(x, y, resultColor);
				}
			}

			if (generateLogFile.Checked)
				logStream.Close();

            outputMap.Save(outputMapTextBox.Text);

			outputPictureBox.Image = outputMap;
			outputPictureBox.Visible = true;
			
			Cursor.Current = previousCursor;
			doneLabel.Visible = true;
		}

        private void maybeEnableGenerate() 
		{
			generateButton.Enabled = (normalMapTextBox.Text != "" &&
									  bumpMapTextBox.Text != "" &&
									  outputMapTextBox.Text != "");
		}
		
		private void trackBar_Scroll(object sender, EventArgs e)
        {
            this.scaleFactorLabel.Text = "Bump Scale Factor: " + trackBar.Value + "%";
        }

        private void normalMapTextBox_TextChanged(object sender, EventArgs e)
        {
            maybeEnableGenerate();
        }

        private void bumpMapTextBox_TextChanged(object sender, EventArgs e)
        {
            maybeEnableGenerate();
        }

        private void outputMapTextBox_TextChanged(object sender, EventArgs e)
        {
            maybeEnableGenerate();
        }

    }
}
