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
	public partial class AddParticleFXDialog : Form
	{
		public AddParticleFXDialog(List<AssetDesc> list)
		{
            InitializeComponent();
            foreach (AssetDesc asset in list)
            {
				particleEffectComboBox.Items.Add(asset.Name);
			}
			if (particleEffectComboBox.Items.Count > 0)
			{
				particleEffectComboBox.SelectedIndex = 0;
			}
		}

        public AddParticleFXDialog(List<AssetDesc> list, List<string> attachmentPoints) : this(list)
        {
            foreach (string s in attachmentPoints)
            {
                attachmentPointComboBox.Items.Add(s);
            }

            if (attachmentPointComboBox.Items.Count > 0)
            {
                attachmentPointComboBox.Show();
                attachmentPointLabel.Show();
                attachmentPointComboBox.SelectedIndex = 0;
            }

        }

		public string ParticleEffectSelectedItem
		{
			get
			{
				return particleEffectComboBox.SelectedItem.ToString();
			}
		}

        public string AttachmentPointName
        {
            get
            {
                return attachmentPointComboBox.SelectedItem.ToString();
            }
        }

		public string VelocityScaleTextBoxText
		{
			get
			{
				return velocityScaleTextBox.Text;
			}
			set
			{
				velocityScaleTextBox.Text = value;
			}
		}

		public string PositionScaleTextBoxText
		{
			get
			{
				return positionScaleTextBox.Text;
			}
			set
			{
				positionScaleTextBox.Text = value;
			}
		}

		public float VelocityScale
		{
			get
			{
				return float.Parse(velocityScaleTextBox.Text);
			}
			set
			{
				velocityScaleTextBox.Text = value.ToString();
			}
		}

		public float PositionScale
		{
			get
			{
				return float.Parse(positionScaleTextBox.Text);
			}
			set
			{
				positionScaleTextBox.Text = value.ToString();
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

			if (!ValidityHelperClass.isFloat(positionScaleTextBox.Text))
			{
                MessageBox.Show("Position scale can not be parsed to a floating point number", "Incorrect position scale", MessageBoxButtons.OK);
				return false;
			}
			else
			{
				if (!ValidityHelperClass.isFloat(velocityScaleTextBox.Text))
				{
                    MessageBox.Show("Velocity scale can not be parsed to a floating point number", "Incorrect position scale", MessageBoxButtons.OK);
					return false;
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
