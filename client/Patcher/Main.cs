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

namespace Multiverse.StandalonePatcher
{
	public class MVUpdate
    {
        public static string VersionFile = "patch_version.txt";
        // Default patch base url
        public static string UpdateUrl = "http://update.multiverse.net/mvupdate.client/";
        // Default patch page
        public static string PatchHtml = "http://update.multiverse.net/patcher/patcher.html";
        // Log file for patcher
        public static string PatchLogFile = "patcher.log";

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

        private static void WaitForExit(int parentId) {
            Process parentProcess = null;
            try {
                parentProcess = Process.GetProcessById(parentId);
            } catch (ArgumentException) {
                // Looks like that process has already exited
            }
            if (parentProcess != null)
                if (!parentProcess.WaitForExit(5000))
                    throw new Exception("Timed out waiting for parent process to exit");
        }

        private static string Version {
            get {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                return version.ToString();
            }
        }

        private static void WriteVersionFile(string versionFile) {
            FileStream stream = File.Open(versionFile, FileMode.OpenOrCreate);
            try {
                StreamWriter writer = new StreamWriter(stream);
                try {
                    writer.WriteLine(MVUpdate.Version);
                } finally {
                    writer.Close();
                }
            } finally {
                stream.Close();
            }
        }

		[STAThread]
		private static void Main(string[] args) {
            // Changes the CurrentCulture of the current thread to the invariant
            // culture.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			try {
				bool full_scan = true;
				bool text_mode = false;
                int parent_id = -1;
                // Source URL is where we will get the media from
                string sourceUrl = UpdateUrl;
                // Patch URL is the html page we will use for the patcher
                string patchUrl = PatchHtml;
                string masterServer = null;
                bool exit_after_patch = false;
				string commandArgs = "";
                for (int i = 0; i < args.Length; ++i) {
					switch (args[i]) {
                        case "--version":
                            Console.WriteLine(MVUpdate.Version);
                            return;
                        case "--parent_id":
                            Debug.Assert(i + 1 < args.Length);
                            parent_id = int.Parse(args[++i]);
                            break;
						case "--partial":
							full_scan = false;
							break;
						case "--text_mode":
							text_mode = true;
							break;
                        case "--update_url":
                            Debug.Assert(i + 1 < args.Length);
                            sourceUrl = args[++i];
                            break;
                        case "--patch_url":
                        case "--patcher_url":
                            Debug.Assert(i + 1 < args.Length);
                            patchUrl = args[++i];
                            break;
                        case "--master":
                            Debug.Assert(i + 1 < args.Length);
                            masterServer = args[++i];
                            break;
                        case "--exit":
                            exit_after_patch = true;
                            break;
					    default:
                            // An argument that we don't understand.. just pass it through
                            // If there is a second part to the option pass that here too
							commandArgs += args[i] + " ";
							if (i+1 < args.Length && args[i+1].Length > 2 && !args[i+1].StartsWith("--"))
								commandArgs += "\"" + args[++i] + "\" ";
							break;
					}
				}
                if (parent_id > 0)
                    WaitForExit(parent_id);

				if (text_mode) {
                    Updater updater = new Updater();
                    updater.FullScan = full_scan;
                    updater.BaseDirectory = "./";
                    updater.UpdateUrl = sourceUrl;
                    updater.SetupLog(PatchLogFile);

					updater.UpdateStarted += MVUpdate.HandleUpdateStarted;
					updater.UpdateCompleted += MVUpdate.HandleUpdateCompleted;
					updater.FileFetchStarted += MVUpdate.HandleFileFetchStarted;
					updater.FileFetchEnded += MVUpdate.HandleFileFetchEnded;
					updater.FileRemoved += MVUpdate.HandleFileRemoved;
                    // Start the thread
                    Thread updaterThread = new Thread(new ThreadStart(updater.Update));
                    updaterThread.Name = "Resource Loader";
                    updaterThread.Start();
                    updaterThread.Join();
                } else {
                    UpdateForm dialog = new UpdateForm();
                    Updater updater = dialog.Updater;
                    updater.FullScan = full_scan;
                    updater.BaseDirectory = "./";
                    updater.UpdateUrl = sourceUrl;
                    updater.SetupLog(PatchLogFile);
                    // this will start the update, since the html calls the OnLoaded, which calls StartUpdate
                    dialog.Initialize(patchUrl, true);
                    // dialog.StartUpdate();

                    // Patcher is run from the MultiverseClient\bin directory
                    DialogResult rv = dialog.ShowDialog();
                    if (rv == DialogResult.Abort || rv == DialogResult.Cancel)
                        return;
				}
                // Ok, we finished patching - write the version and launch the client
                WriteVersionFile(VersionFile);
                // Check to see if we should just exit now.
                if (exit_after_patch)
                    // In this case, we don't want to launch the client.  They can launch it
                    // manually if they need to, but since we don't want to screw up the
                    // extended command options, we should just exit.               
                    return;
                string currentDir = Directory.GetCurrentDirectory();
                string firstCommandArgs = "--update_url " + sourceUrl;
                if (masterServer != null)
                    firstCommandArgs += " --master " + masterServer;
                // Prepend the firstCommandArgs to the commandArgs
				commandArgs = firstCommandArgs + " " + commandArgs;
                ProcessStartInfo psi =
                    new ProcessStartInfo("MultiverseClient.exe", commandArgs);
                psi.WorkingDirectory = currentDir + "\\bin";
                Process.Start(psi);

            } catch (Exception ex) {
				// Check the latest manifest file
				// call the existing global exception handler
				Console.WriteLine(ex.ToString());
                Debug.Assert(false, "Caught exception while updating client");
			} finally {
				GC.Collect();
				// Kernel.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1); 
				// this.WindowState = FormWindowState.Normal;
				// this.Show();
			}
		}
	}
}
