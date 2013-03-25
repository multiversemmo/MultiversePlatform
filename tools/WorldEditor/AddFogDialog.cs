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
using System.Web;
using Axiom.Core;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
	public partial class AddFogDialog : Form
	{
		ColorEx cx = new ColorEx();
		ColorDialog colorDialog1 = null;

		public AddFogDialog(string title, WorldEditor App)
		{
			InitializeComponent();
			this.Text = title;
		}


		private Color ColorExToColor(ColorEx cx)
		{
			return Color.FromArgb((int)cx.ToARGB());
		}

		private ColorEx ColorToColorEx(Color c)
		{
			return new ColorEx(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f);

		}


		public ColorEx Cx
		{
			get
			{
				return cx;
			}
			set
			{
				cx = value;
				colorSelectButton.BackColor = ColorExToColor(value);
			}
		}

		public string FogFarTextBoxText
		{
			get
			{
				return FogFarTextBox.Text;
			}
			set
			{
				FogFarTextBoxText = value;
			}
		}

		public string FogNearTextBoxText
		{
			get
			{
				return FogNearTextBox.Text;
			}
			set
			{
				FogNearTextBox.Text = value;
			}
		}


		public float FarFog
		{
			get
			{
				return float.Parse(FogFarTextBox.Text);
			}
			set
			{
				FogFarTextBox.Text = value.ToString();
			}
		}

		public float NearFog
		{
			get
			{
				return float.Parse(FogNearTextBox.Text);
			}
			set
			{
				FogNearTextBox.Text = value.ToString();
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



		public bool okButton_validating()
		{
			if (!ValidityHelperClass.isFloat(FogNearTextBox.Text))
			{
                MessageBox.Show("Start fog distance can not be parsed to a floating point number", "Incorrect start fog distance", MessageBoxButtons.OK);
				return false;
			}
			else
			{
				if (!ValidityHelperClass.isFloat(FogFarTextBox.Text))
				{
                    MessageBox.Show("Maximum fog distance can not be parsed to a floating point number", "Maximum fog distance", MessageBoxButtons.OK);
					return false;
				}
			}
			return true;
		}

		private void onHelpButtonClicked(object sender, EventArgs e)
		{
			Button but = sender as Button;
			WorldEditor.LaunchDoc(but.Tag as string);
		}

		private void colorSelectButton_Click(object sender, EventArgs e)
		{
			using (colorDialog1 = new ColorDialog())
			{
				colorDialog1.SolidColorOnly = false;
				colorDialog1.AllowFullOpen = true;
				colorDialog1.AnyColor = true;
                int colorabgr = (int)cx.ToABGR();
				colorabgr &= 0x00ffffff;
				int[] colorsabgr = new int[1];
				colorsabgr[0] = colorabgr;
				colorDialog1.FullOpen = true;
				colorDialog1.ShowHelp = true;
				colorDialog1.Color = ColorExToColor(cx);
				colorDialog1.CustomColors = colorsabgr;
				DialogResult result = colorDialog1.ShowDialog();
				if (result == DialogResult.OK)
				{
					colorSelectButton.BackColor = colorDialog1.Color;
					cx = ColorToColorEx(colorDialog1.Color);
				}
			}
		}

	}
}
