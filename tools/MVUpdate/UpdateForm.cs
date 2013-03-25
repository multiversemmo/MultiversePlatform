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
using System.Threading;
using System.Windows.Forms;

namespace Multiverse.Update {
	public partial class UpdateForm : Form {
		string statusText;
		long bytesFetched;
		long bytesNeeded;
		bool loaded = false;
		UpdateState updateState;

		public delegate void UpdateHandler(object sender);

		public UpdateForm() {
			statusText = "Preparing to Update";
			InitializeComponent();
		}

		private void SetStatusText(string txt) {
			// Console.WriteLine("Set Status Text to " + txt);
			lock (this) {
				this.statusText = txt;
			}
		}
		// Called in the form's thread
		private void HandleUpdate(object sender) {
			lock (this) {
				label1.Text = statusText;
				progressBar2.Maximum = 100;
				if (bytesNeeded > 0)
					progressBar2.Value = (int)((100 * bytesFetched) / bytesNeeded);
				else
					progressBar2.Value = 0;
				if (updateState == UpdateState.UpdateEnded)
					button1.Enabled = true;
			}
		}
		// Called from the worker thread
		private void UpdateProgress() {
			bool doInvoke = false;
			lock (this) {
				doInvoke = loaded;
			}
			if (doInvoke)
				this.BeginInvoke(new UpdateHandler(this.HandleUpdate), this);
		}
		// Called from the worker thread
		public void HandleFileFetchStarted(object sender, UpdateFileStatus status) {
			SetStatusText("Downloading File: " + status.file + " ...");
			UpdateProgress();
		}
		// Called from the worker thread
		public void HandleFileFetchEnded(object sender, UpdateFileStatus status) {
			SetStatusText("Downloaded File: " + status.file);
			lock (this) {
				bytesFetched += status.length;
			}
			UpdateProgress();
		}
		// Called from the worker thread
		public void HandleFileRemoved(object sender, UpdateFileStatus status) {
			SetStatusText("Removed file: " + status.file);
			UpdateProgress();
		}
		// Called from the worker thread
		public void HandleStateChanged(object sender, int state) {
			updateState = (UpdateState)state;
			switch (updateState) {
				case UpdateState.UpdateStarted:
					SetStatusText("Updating Media ...");
					break;
				case UpdateState.UpdateEnded:
					SetStatusText("Update Completed ...");
					break;
				case UpdateState.FetchingManifest:
					SetStatusText("Retrieving Manifest");
					break;
				case UpdateState.ScanningFiles:
					SetStatusText("Scanning Files");
					break;
				case UpdateState.UpdatingFiles:
					SetStatusText("Updating Files");
					break;
				default:
					SetStatusText("");
					Console.WriteLine("Invalid state: " + updateState);
					break;
			}
			UpdateProgress();
		}
		// Called from the worker thread
		public void HandleUpdateStarted(object sender, UpdaterStatus status) {
			SetStatusText("Updating Media ...");
			lock (this) {
				bytesNeeded = status.bytes;
			}
			UpdateProgress();
		}
		private void Form1_Load(object sender, EventArgs e) {
			// SetStatusText("Preparing to Update");
			lock (this) {
				loaded = true;
			}
			button1.DialogResult = DialogResult.OK;
			button1.Enabled = false;
			button2.DialogResult = DialogResult.Cancel;
			progressBar2.Minimum = 0;
			UpdateProgress();
		}
	}
}
