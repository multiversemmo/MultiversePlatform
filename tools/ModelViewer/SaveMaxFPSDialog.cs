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

namespace Multiverse.Tools.ModelViewer {
    public partial class SaveMaxFPSDialog : Form {
        public SaveMaxFPSDialog() {
            InitializeComponent();
        }

        public TextBox NumberBox {
            get {
                return numberBox;
            }
        }

        private void okButton_Click(object sender, EventArgs e) {
            int result = 0;
            if (int.TryParse(numberBox.Text, out result) && result >= 0)
                DialogResult = DialogResult.OK;
            else
                MessageBox.Show("'" + numberBox.Text + "' is not a legal non-negative integer - - try again");
        }

        private void SaveMaxFPSDialog_Activated(object sender, EventArgs e) {
            int count = Root.Instance.MaxFramesPerSecond;
            if (count == 0)
                statusLabel.Text = "There is no maximum FPS rate";
            else
                statusLabel.Text = "The current max FPS is " + count;
        }
    }
}
