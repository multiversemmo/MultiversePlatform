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
	public partial class AddMobDialog : Form, IDisposable
	{
		private bool showRadius;

		public AddMobDialog(string title, bool enableRadius)
		{
			InitializeComponent();
			this.showRadius = enableRadius;
			if (enableRadius)
			{
				spawnRadiusLabel.Visible = true;
				spawnRadiusTextbox.Visible = true;
			}
			else
			{
				spawnRadiusLabel.Visible = false;
				spawnRadiusTextbox.Visible = false;
			}
		}

		public string TemplateNameTextBoxText
		{
			get
			{
				return templateNameTextbox.Text;
			}
			set
			{
				templateNameTextbox.Text = value;
			}
		}

		public string RespawnTimeTextboxText
		{
			get
			{
				return respawnTimeTextbox.Text;
			}
			set
			{
				respawnTimeTextbox.Text = value;
			}
		}

		public string NumberOfSpawnsTextboxText
		{
			get
			{
				return numberOfSpawnsTextbox.Text;
			}
			set
			{
				numberOfSpawnsTextbox.Text = value;
			}
		}

		public string SpawnRadiusTextboxText
		{
			get
			{
				if (showRadius)
				{
					return spawnRadiusTextbox.Text;
				}
				else
				{
					return "";
				}
			}
			set
			{
				if (showRadius)
				{
					spawnRadiusTextbox.Text = value;
				}
				else
				{
					spawnRadiusTextbox.Text = "";
				}
			}
		}

		public float SpawnRadius
		{
			get
			{
				if (showRadius)
				{
					return float.Parse(spawnRadiusTextbox.Text);
				}
				return 0;
			}
			set
			{
				if (showRadius)
				{
					spawnRadiusTextbox.Text = value.ToString();
				}
				else
				{
					spawnRadiusTextbox.Text = "0";
				}
			}
		}

		public int RespawnTime
		{
			get
			{
				return int.Parse(respawnTimeTextbox.Text);
			}
			set
			{
				respawnTimeTextbox.Text = value.ToString();
			}
		}

		public uint NumberOfSpawns
		{
			get
			{
				return uint.Parse(numberOfSpawnsTextbox.Text);
			}
			set
			{
				numberOfSpawnsTextbox.Text = value.ToString();
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

		private void intVerifyevent(object sender, CancelEventArgs e)
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

		private void uintVerifyevent(object sender, CancelEventArgs e)
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
			if (!ValidityHelperClass.isInt(respawnTimeTextbox.Text))
			{
                MessageBox.Show("Respawn time can not be parsed to a integer number", "Incorrect respawn time", MessageBoxButtons.OK);
				return false;
			}
			else
			{
				if (!ValidityHelperClass.isUint(numberOfSpawnsTextbox.Text))
				{
                    MessageBox.Show("Number of spanws can not be parsed to a unsigned integer number", "Incorrect number of spawns", MessageBoxButtons.OK);
					return false;
				}
				else
				{
					if ((!ValidityHelperClass.isInt(spawnRadiusTextbox.Text)) && (showRadius))
					{
                        MessageBox.Show("Spawn radius can not be parsed to a integer number", "Incorrect spawn radius", MessageBoxButtons.OK);
						return false;
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
