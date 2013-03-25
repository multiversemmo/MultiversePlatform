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
using System.Collections;
using Axiom.MathLib;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
	public partial class ForestDialog : Form
	{
		public ForestDialog(List<AssetDesc> speedWind, string selectedString, string titleString)
		{
			InitializeComponent();
			ForestDialogSpeedWindFilenameComboBox.BeginUpdate();
			foreach (AssetDesc asset in speedWind)
			{
				ForestDialogSpeedWindFilenameComboBox.Items.Add(asset.Name);
			}
			ForestDialogSpeedWindFilenameComboBox.EndUpdate();
            //if (selectedString != "")
            //{

            //    for (int i = 0; i < ForestDialogSpeedWindFilenameComboBox.Items.Count; i++)
            //    {
            //        if (String.Equals(ForestDialogSpeedWindFilenameComboBox.Items[i].ToString(), selectedString))
            //        {
            //            ForestDialogSpeedWindFilenameComboBox.SelectedIndex = i;
            //            break;
            //        }
            //    }
            //}
            //else
            //{
                if (ForestDialogSpeedWindFilenameComboBox.Items.Count > 0)
				{
					ForestDialogSpeedWindFilenameComboBox.SelectedIndex = 0;
				}
				else
				{
					ForestDialogSpeedWindFilenameComboBox.SelectedIndex = -1;
				}
            //}
			this.Text = titleString;
		}

		public string SpeedWindFile
		{
			get
			{
				return ForestDialogSpeedWindFilenameComboBox.SelectedItem.ToString();
			}

		}

		public float WindSpeed
		{
			get
			{
				return float.Parse(forestAddDialogWindSpeedTextBox.Text);
			}
			set
			{
				(forestAddDialogWindSpeedTextBox.Text) = value.ToString();
			}
		}

		public int Seed
		{
			get
			{
				return int.Parse(ForestSeedTextBox.Text);
			}
			set
			{
				ForestSeedTextBox.Text = value.ToString();
			}
		}

		public Vector3 WindDirection
		{
			get
			{
				return new Vector3(float.Parse(forestAddDialogWindDirectionXTextBox.Text), float.Parse(forestAddDialogWindDirectionYTextBox.Text), float.Parse(forestAddDialogWindDirectionZTextBox.Text));
			}
			set
			{
				forestAddDialogWindDirectionXTextBox.Text = value.x.ToString();
				forestAddDialogWindDirectionYTextBox.Text = value.y.ToString();
				forestAddDialogWindDirectionXTextBox.Text = value.z.ToString();
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

		public bool okButton_validating()
		{
            if (String.Equals(ForestDialogSpeedWindFilenameComboBox.Text, ""))
            {
                return false;
            }
            else
            {
                if (!ValidityHelperClass.isInt(ForestSeedTextBox.Text))
                {
                    MessageBox.Show("Random number seed can not be parsed to a integer number", "Incorrect random number seed", MessageBoxButtons.OK);
                    return false;
                }
                else
                {
                    if (!ValidityHelperClass.isFloat(forestAddDialogWindSpeedTextBox.Text))
                    {
                        MessageBox.Show("Wind speed can not be parsed to a floating point number", "Incorrect wind speed", MessageBoxButtons.OK);
                        return false;
                    }
                    else
                    {
                        if (!ValidityHelperClass.isFloat(forestAddDialogWindDirectionXTextBox.Text))
                        {
                            MessageBox.Show("Wind direction X can not be parsed to a floating point number", "Incorrect wind direction X", MessageBoxButtons.OK);
                            return false;
                        }
                        else
                        {
                            if (!ValidityHelperClass.isFloat(forestAddDialogWindDirectionYTextBox.Text))
                            {
                                MessageBox.Show("Wind direction Y can not be parsed to a floating point number", "Incorrect wind direction Y", MessageBoxButtons.OK);
                                return false;
                            }
                            else
                            {
                                if (!ValidityHelperClass.isFloat(forestAddDialogWindDirectionZTextBox.Text))
                                {
                                    MessageBox.Show("Wind direction Z can not be parsed to a floating point number", "Incorrect wind direction Z", MessageBoxButtons.OK);
                                    return false;
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
