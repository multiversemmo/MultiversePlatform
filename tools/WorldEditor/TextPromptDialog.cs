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

namespace Multiverse.Tools.WorldEditor
{
    public partial class TextPromptDialog : Form
    {
		protected string anchor;

        public TextPromptDialog(String title, String labelText, string anchor)
        {
            InitializeComponent();

            this.Text = title;
            this.label.Text = labelText;
			this.anchor = anchor;
        }

		public TextPromptDialog(String title, String labelText)
		{
			InitializeComponent();

			this.Text = title;
			this.label.Text = labelText;
		}

        public TextPromptDialog(String title, String labelText, string anchor, string buttonText)
        {
            InitializeComponent();
            this.Text = title;
            this.label.Text = labelText;
            this.anchor = anchor;
            this.okButton.Text = buttonText;
        }


        public String UserText
        {
            get
            {
                return this.textBox.Text;
            }
            set
            {
                this.textBox.Text = value;
            }
        }

		public int UserInt
		{
			get
			{
				return int.Parse(this.textBox.Text);
			}
		}

		public float UserFloat
		{
			get
			{
				return float.Parse(this.textBox.Text);
			}
		}

		public string TextBoxText
		{
			get
			{
				return textBox.Text;
			}
			set
			{
				textBox.Text = value;
			}

		}

		public string TitleString
		{
			get
			{
				return this.Text;
			}
			set
			{
				this.Text = value;
			}
		}

		private void helpButton_clicked(object sender, EventArgs e)
		{
			WorldEditor.LaunchDoc(this.anchor);
		}

    }
}
