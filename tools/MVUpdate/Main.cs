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
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using Multiverse.Patcher;

namespace Multiverse.Update
{
	public class MVUpdate
    {
		private static void HandleFileFetchStarted(object sender, UpdateFileStatus msg) {
			Console.WriteLine("Downloading File: " + msg.file + " ...");
		}
		private static void HandleFileFetchEnded(object sender, UpdateFileStatus msg) {
			Console.WriteLine("Downloaded File: " + msg.file);
		}
		private static void HandleFileRemoved(object sender, UpdateFileStatus msg) {
			Console.WriteLine("Removed file: " + msg.file);
		}
		private static void HandleUpdateStarted(object sender, UpdaterStatus msg) {
			Console.WriteLine("Updating Media ...");
		}
		private static void HandleUpdateCompleted(object sender, UpdaterStatus msg) {
			Console.WriteLine("Update Succeeded");
		}
		[STAThread]
		private static void Main(string[] args) {
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("");
			try {
				bool full_scan = true;
				bool text_mode = false;
                string sourceUrl = "http://update.multiverse.net/mvupdate/";
				for (int i = 0; i < args.Length; ++i) {
					switch (args[i]) {
						case "--partial":
							full_scan = false;
							break;
						case "--text_mode":
							text_mode = true;
							break;
						case "--source":
							sourceUrl = args[++i];
							break;
					}
				}

				Updater updater = new Updater(sourceUrl);
				updater.FullScan = full_scan;
				UpdateForm dialog = null;

				if (text_mode) {
					updater.UpdateStarted += MVUpdate.HandleUpdateStarted;
					updater.UpdateCompleted += MVUpdate.HandleUpdateCompleted;
					updater.FileFetchStarted += MVUpdate.HandleFileFetchStarted;
					updater.FileFetchEnded += MVUpdate.HandleFileFetchEnded;
					updater.FileRemoved += MVUpdate.HandleFileRemoved;
				} else {
					dialog = new UpdateForm();
					updater.UpdateStarted += dialog.HandleUpdateStarted;
                    updater.UpdateCompleted += dialog.HandleUpdateCompleted;
                    updater.FileFetchStarted += dialog.HandleFileFetchStarted;
					updater.FileFetchEnded += dialog.HandleFileFetchEnded;
					updater.FileRemoved += dialog.HandleFileRemoved;
					updater.StateChanged += dialog.HandleStateChanged;
                    // Patcher is run from the MultiverseClient\bin directory
                    updater.BaseDirectory = "..\\";
				}
				Thread updaterThread = new Thread(new ThreadStart(updater.Update));
				updaterThread.Name = "Resource Loader";
				updaterThread.Start();
                DialogResult rv = DialogResult.Abort;
				if (dialog != null)
					rv = dialog.ShowDialog();
                if (rv == DialogResult.Abort || rv == DialogResult.Cancel) {
                    updaterThread.Abort();
                }
				updaterThread.Join();
			} catch (Exception ex) {
				// Check the latest manifest file
				// call the existing global exception handler
				Console.WriteLine(ex.ToString());
			} finally {
				GC.Collect();
				// Kernel.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1); 
				// this.WindowState = FormWindowState.Normal;
				// this.Show();
			}
		}
	}
}
