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

using System.IO;
using System.Diagnostics;

using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Configuration;
using Axiom.Collections;
using Axiom.Animating;
using Axiom.ParticleSystems;
using Axiom.Serialization;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
	public partial class AddPlantTypeDialog : Form
	{
		private Color ColorExToColor(ColorEx cx)
		{
			return Color.FromArgb((int)cx.ToARGB());
		}

		private ColorEx ColorToColorEx(Color c)
		{
			return new ColorEx(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f);
		}

		public AddPlantTypeDialog(string title, List<AssetDesc> list, WorldEditor app)
		{
			InitializeComponent();
			ColorEx color = new ColorEx();
			color = app.Config.PlantColorDefault;
			colorDialog1.Color = ColorExToColor(color);
			colorSelectButton.BackColor = colorDialog1.Color;
			int colorabgr = (int)color.ToABGR();
			int[] colorsabgr = new int[1];
			colorabgr &= 0x00ffffff;
			colorsabgr[0] = colorabgr;
			colorDialog1.CustomColors = colorsabgr;

			foreach (AssetDesc asset in list)
			{
				imageNameComboBox.Items.Add(asset.Name);
			}
			imageNameComboBox.SelectedIndex = 0;
		}

		private void colorSelectButton_Click(object sender, EventArgs e)
		{
			colorDialog1.ShowDialog();
			colorSelectButton.BackColor = colorDialog1.Color;
		}

		public string ComboBoxSelected
		{
			get
			{
				return imageNameComboBox.SelectedItem.ToString();
			}
		}


		public ColorEx ColorRGB
		{
			get
			{
				return ColorToColorEx(colorDialog1.Color);
			}
			set
			{
				colorDialog1.Color = ColorExToColor(value);
			}
		}

		public string MinimumWidthScaleTextBoxText
		{
			get
			{
				return minimumWidthScaleTextBox.Text;
			}
			set
			{
				minimumWidthScaleTextBox.Text = value;
			}
		}

		public string MaximumWidthScaleTextBoxText
		{
			get
			{
				return maximumWidthScaleTextBox.Text;
			}
			set
			{
				maximumWidthScaleTextBox.Text = value;
			}
		}

		public string MinimumHeightScaleTextBoxText
		{
			get
			{
				return minimumHeightScaleTextBox.Text;
			}
			set
			{
				minimumHeightScaleTextBox.Text = value;
			}
		}

		public string MaximumHeightScaleTextBoxText
		{
			get
			{
				return maximumHeightScaleTextBox.Text;
			}
			set
			{
				maximumHeightScaleTextBox.Text = value;
			}
		}

		public string ColorMultLowTextBoxText
		{
			get
			{
				return colorMultLowTextBox.Text;
			}
			set
			{
				colorMultLowTextBox.Text = value;
			}
		}

		public string ColorMultHiTextBoxText
		{
			get
			{
				return colorMultHiTextBox.Text;
			}
			set
			{
				colorMultHiTextBox.Text = value;
			}
		}

		public string InstancesTextBoxText
		{
			get
			{
				return instancesTextBox.Text;
			}
			set
			{
				instancesTextBox.Text = value;
			}
		}

        public string WindMagnitudeTextBoxText
		{
			get
			{
				return windMagnitudeTextBox.Text;
			}
			set
			{
				windMagnitudeTextBox.Text = value;
			}
		}






		public float MinimumWidthScale
		{
			get
			{
				return float.Parse(minimumWidthScaleTextBox.Text);
			}
			set
			{
				minimumWidthScaleTextBox.Text = value.ToString();
			}
		}

		public float MaximumWidthScale
		{
			get
			{
				return float.Parse(maximumWidthScaleTextBox.Text);
			}
			set
			{
				maximumWidthScaleTextBox.Text = value.ToString();
			}
		}

		public float MinimumHeightScale
		{
			get
			{
				return float.Parse(minimumHeightScaleTextBox.Text);
			}
			set
			{
				minimumHeightScaleTextBox.Text = value.ToString();
			}
		}

		public float MaximumHeightScale
		{
			get
			{
				return float.Parse(maximumHeightScaleTextBox.Text);
			}
			set
			{
				maximumHeightScaleTextBox.Text = value.ToString();
			}
		}

		public float ColorMultLow
		{
			get
			{
				return float.Parse(colorMultLowTextBox.Text);
			}
			set
			{
				colorMultLowTextBox.Text = value.ToString();
			}
		}

		public float ColorMultHi
		{
			get
			{
				return float.Parse(colorMultHiTextBox.Text);
			}
			set
			{
				colorMultHiTextBox.Text = value.ToString();
			}
		}

		public uint Instances
		{
			get
			{
				return uint.Parse(instancesTextBox.Text);
			}
			set
			{
				instancesTextBox.Text = value.ToString();
			}
		}

        public float WindMagnitude
		{
			get
			{
				return float.Parse(windMagnitudeTextBox.Text);
			}
			set
			{
				windMagnitudeTextBox.Text = value.ToString();
			}
		}

		private void floatVerifyevent(object sender, CancelEventArgs e)
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

		private void uIntVerifyevent(object sender, CancelEventArgs e)
		{
			TextBox textbox = (TextBox)sender;
			if (!ValidityHelperClass.isUint(textbox.Text))
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

		public bool okButton_validating()
		{

			if (!ValidityHelperClass.isFloat(minimumWidthScaleTextBox.Text))
			{

                MessageBox.Show("Minimum width scale can not be parsed to a floating point number", "Incorrect minimum width scale", MessageBoxButtons.OK);
				return false;
			}
			else
			{
				if (!ValidityHelperClass.isFloat(maximumWidthScaleTextBox.Text))
				{
                    MessageBox.Show("Maximum width scale can not be parsed to a floating point number", "Maximum width scale", MessageBoxButtons.OK);
					return false;
				}
				else
				{
					if (!ValidityHelperClass.isFloat(minimumHeightScaleTextBox.Text))
                    {
                        MessageBox.Show("Minimum height scale can not be parsed to a floating point number", "Incorrect minimum height scale", MessageBoxButtons.OK);
						return false;
					}
					else
					{
						if (!ValidityHelperClass.isFloat(maximumHeightScaleTextBox.Text))
						{

                            MessageBox.Show("Maximum height scale can not be parsed to a floating point number", "Incorrect maximum height scale", MessageBoxButtons.OK);
							return false;
						}
						else
						{
							if (!ValidityHelperClass.isFloat(colorMultLowTextBox.Text))
							{

                                MessageBox.Show("Color muliplier low can not be parsed to a floating point number", "Incorrect color multiplier low", MessageBoxButtons.OK);
								return false;
							}
							else
							{
								if (!ValidityHelperClass.isFloat(colorMultHiTextBox.Text))
                                {
                                    MessageBox.Show("Color multiplier high can not be parsed to a floating point number", "Incorrect color multiplier high", MessageBoxButtons.OK);
									return false;
								}
								else
								{
									if (!ValidityHelperClass.isUint(instancesTextBox.Text))
									{
                                        MessageBox.Show("Instances can not be parsed to a unsigned integer number", "Incorrect instances", MessageBoxButtons.OK);
										return false;
									}
									else
									{
                                        if (!ValidityHelperClass.isFloat(windMagnitudeTextBox.Text))
										{
                                            MessageBox.Show("Wind magnitude can not be parsed to a floating point number", "Incorrect wind magnitude", MessageBoxButtons.OK);
											return false;
										}
									}
								}
							}
						}
					}
				}
			}
			return true;
		}

		private void helpButton_clicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
			WorldEditor.LaunchDoc(but.Tag as string);
		}
	}
}
