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
    public partial class SetCameraNearDialog : Form {
        public SetCameraNearDialog(WorldEditor worldEditor) {
			InitializeComponent();
			this.worldEditor = worldEditor;
            setTrackBarFromCameraNear();
        }

        private void cameraNearDistanceTrackBar_Scroll(object sender, EventArgs e) {
            setCameraNearFromTrackBar();
        }

        protected WorldEditor worldEditor;
        protected readonly float oneMeter = 1000.0f;

        public float CameraNear {
            get { return worldEditor.CameraNear; }
            set { worldEditor.CameraNear = value; }
        }

        protected void setTrackBarFromCameraNear() {
            if (CameraNear > 10f * oneMeter)
                cameraNearDistanceTrackBar.Value = 20 * (int)(CameraNear / (10f * oneMeter));
            else
                cameraNearDistanceTrackBar.Value = Math.Max(0, (int)((Math.Log10((double)CameraNear) - 2.0) * 20.0));
            setNearLabel();
        }

        protected void setCameraNearFromTrackBar() {
            // It's a logarithmic scale .01 meters to 10 meters, and then linear after that
            if (cameraNearDistanceTrackBar.Value > 20)
                CameraNear = oneMeter * 10f * ((float)(cameraNearDistanceTrackBar.Value - 20) / 20f);
            else
                CameraNear = oneMeter / 10f * (float)Math.Pow(10.0, (double)cameraNearDistanceTrackBar.Value / 20.0);
            setNearLabel();
		}

        protected void setNearLabel() {
            nearDistanceValueLabel.Text = string.Format("{0:f4} meters", CameraNear / oneMeter);
        }

        private void okButton_Click(object sender, EventArgs e) {
            Hide();
        }

        private void SetCameraNearDialog_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
            Hide();
        }
		
    }
}
