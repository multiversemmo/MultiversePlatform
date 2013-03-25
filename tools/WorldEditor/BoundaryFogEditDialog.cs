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

namespace Multiverse.MapTool
{
	public partial class BoundaryFogEditDialog : Form
	{
		public BoundaryFogEditDialog(int red, int green, int blue, float far, float near)
		{
			InitializeComponent();
			this.RedTextBox.Text = red.ToString();
			this.GreenTextBox.Text = green.ToString();
			this.BlueTextBox.Text = blue.ToString();
			this.FarTextBox.Text = far.ToString();
			this.NearTextBox.Text = near.ToString();
		}

		public int red
		{
			get
			{
				return int.Parse(RedTextBox.Text);
			}
			set
			{
				RedTextBox.Text = value.ToString();
			}
		}

		public int green
		{
			get
			{
				return int.Parse(GreenTextBox.Text);
			}
			set
			{
				GreenTextBox.Text = value.ToString();
			}
		}

		public int blue
		{
			get
			{
				return int.Parse(BlueTextBox.Text);
			}
			set
			{
				BlueTextBox.Text = value.ToString();
			}
		}

		public float near
		{
			get
			{
				return float.Parse(NearTextBox.Text);
			}
			set
			{
				NearTextBox.Text = value.ToString();
			}
		}

		public float far
		{
			get
			{
				return float.Parse(FarTextBox.Text);
			}
			set
			{
				FarTextBox.Text = value.ToString();
			}
		}
	}

}
