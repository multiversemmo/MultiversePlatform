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
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;

namespace Multiverse.Update {
	class ManifestEntry {
		public string name;
		public string digest;
		public string length;
	}

	class FileFetcher {
		public static void FetchFile(string filename, string srcUrl, string baseDir) {
			string dirname = "./";
			string[] parts = filename.Split('/');
			for (int i = 0; i < parts.Length - 1; ++i) {
				dirname = dirname + "/" + parts[i];
				if (!Directory.Exists(dirname))
					Directory.CreateDirectory(dirname);
			}
			WebClient client = new WebClient();
			client.DownloadFile(srcUrl + filename, baseDir + filename);
		}
	}

	public delegate void UpdateEventHandler(object sender, UpdaterStatus e);
	public delegate void UpdateFileEventHandler(object sender, UpdateFileStatus e);
	public delegate void UpdateStateEventHandler(object sender, int state);

	public class UpdaterStatus : EventArgs {
		public long bytes;
		public int files;
	}
	public class UpdateFileStatus : EventArgs {
		public string file;
		public long length;
	}
	enum UpdateState {
		UpdateStarted = 1,
		UpdateEnded = 2,
		FetchingManifest = 3,
		ScanningFiles = 4,
		UpdatingFiles = 5,
		Unknown = 6
	}
	
	class Updater {
		Dictionary<string, ManifestEntry> targetManifestDict;
		Dictionary<string, ManifestEntry> currentManifestDict;
		string sourceUrl;
		bool full_scan;

		public event UpdateEventHandler UpdateStarted;
		public event UpdateEventHandler UpdateCompleted;
		public event UpdateFileEventHandler FileFetchStarted;
		public event UpdateFileEventHandler FileFetchEnded;
		public event UpdateFileEventHandler FileRemoved;
		public event UpdateStateEventHandler StateChanged;

		protected void OnUpdateStarted(UpdaterStatus status) {
			if (UpdateStarted != null)
				UpdateStarted(this, status);
		}

		protected void OnUpdateCompleted(UpdaterStatus status) {
			if (UpdateCompleted != null)
				UpdateCompleted(this, status);
		}

		protected void OnFileFetchStarted(UpdateFileStatus status) {
			if (FileFetchStarted != null)
				FileFetchStarted(this, status);
		}

		protected void OnFileFetchEnded(UpdateFileStatus status) {
			if (FileFetchEnded != null)
				FileFetchEnded(this, status);
		}

		protected void OnFileRemoved(UpdateFileStatus status) {
			if (FileRemoved != null)
				FileRemoved(this, status);
		}

		protected void OnStateChanged(int state) {
			if (StateChanged != null)
				StateChanged(this, state);
		}

		protected void OnStateChanged(UpdateState state) {
			if (StateChanged != null)
				StateChanged(this, (int)state);
		}

		public Updater(string sourceUrl) {
			this.sourceUrl = sourceUrl;

			targetManifestDict = 
				new Dictionary<string, ManifestEntry>();
			currentManifestDict =
				new Dictionary<string, ManifestEntry>();
		}

		public static void WriteManifest(string manifestFile, 
										 Dictionary<string, ManifestEntry> dict) {
			Stream stream = File.Open(manifestFile, FileMode.Create);
			StreamWriter writer = new StreamWriter(stream);
			try {
				foreach (ManifestEntry entry in dict.Values) {
					writer.WriteLine("Name: " + entry.name);
					writer.WriteLine("SHA-Digest: " + entry.digest);
					writer.WriteLine("Content-Length: " + entry.length);
					writer.WriteLine();
				}
			} finally {
				writer.Close();
				stream.Close();
			}
		}

		public void BuildManifest(string baseDir, string prefix) {
			string[] dirs = Directory.GetDirectories(baseDir, prefix);
			foreach (string dir in dirs)
				ProcessDir(baseDir, dir);
		}

		public void BuildManifest(string manifestFile, string baseDir, string prefix) {
			BuildManifest(baseDir, prefix);
			WriteManifest(manifestFile, currentManifestDict);
		}

		public void LoadManifest(string manifestFile) {
			ReadManifest(currentManifestDict, manifestFile);
		}

		public void ProcessFile(string name, string file) {
			ManifestEntry entry = new ManifestEntry();
			entry.name = name;
			FileStream data = File.Open(file, FileMode.Open);
			long fileLength = data.Length;
			entry.length = fileLength.ToString();
			HashAlgorithm digester = new SHA1CryptoServiceProvider();
			byte[] digest = digester.ComputeHash(data);
			data.Close();
			StringBuilder digestStr = new StringBuilder();
			foreach (byte b in digest)
				digestStr.Append(b.ToString("x2"));
			entry.digest = digestStr.ToString();
			currentManifestDict[name] = entry;
		}

		public void ProcessDir(string baseDir, string dir) {
			string[] dirs = Directory.GetDirectories(dir);
			string[] files = Directory.GetFiles(dir);
			foreach (string file in files)
				if (file.StartsWith(baseDir)) {
					string name = file.Substring(baseDir.Length);
					name = name.Replace('\\', '/');
					ProcessFile(name, file);
				}
			foreach (string subdir in dirs)
				ProcessDir(baseDir, subdir);
		}

		public void BuildManifest(string currentManifest, string baseDir, 
								  string prefix, bool fullScan) {
			OnStateChanged(UpdateState.ScanningFiles);
			if (!fullScan) {
				try {
					LoadManifest(currentManifest);
				} catch (FileNotFoundException) {
					fullScan = true;
				}
			}
			if (fullScan)
				BuildManifest(currentManifest, baseDir, prefix);
		}

		public void ProcessManifest(string currentManifest) {
			if (currentManifest != null)
				ReadManifest(currentManifestDict, currentManifest);

			OnStateChanged(UpdateState.FetchingManifest);
			string baseDir = "";
			FileFetcher.FetchFile("MANIFEST", sourceUrl, baseDir);

			ReadManifest(targetManifestDict, "MANIFEST");

			long bytesNeeded = 0;
			int filesNeeded = 0;
			List<string> newFiles = new List<string>();
			List<string> removedFiles = new List<string>();
			List<string> modifiedFiles = new List<string>();
			foreach (string key in targetManifestDict.Keys) {
				long bytes = long.Parse(targetManifestDict[key].length);
				if (!currentManifestDict.ContainsKey(key)) {
					newFiles.Add(key);
					bytesNeeded += bytes;
					filesNeeded++;
				} else if (currentManifestDict[key].digest != targetManifestDict[key].digest) {
					modifiedFiles.Add(key);
					bytesNeeded += bytes;
					filesNeeded++;
				}
			}
			foreach (string key in currentManifestDict.Keys) {
				if (!targetManifestDict.ContainsKey(key))
					removedFiles.Add(key);
			}

			// At this point, we are about to start fetching new files,
			// so remove our existing manifest.  This will cause us to 
			// do a full scan if our download is interrupted, which will
			// be more efficient than fetching everything again.
			File.Delete(currentManifest);

			UpdaterStatus updateStatus = new UpdaterStatus();
			updateStatus.files = filesNeeded;
			updateStatus.bytes = bytesNeeded;
			OnUpdateStarted(updateStatus);
			OnStateChanged(UpdateState.UpdatingFiles);
			foreach (string file in newFiles) {
				UpdateFileStatus ufs = new UpdateFileStatus();
				ufs.file = file;
				ufs.length = long.Parse(targetManifestDict[file].length);
				OnFileFetchStarted(ufs);
				FileFetcher.FetchFile(file, sourceUrl, baseDir);
				OnFileFetchEnded(ufs);
			}
			foreach (string file in modifiedFiles) {
				UpdateFileStatus ufs = new UpdateFileStatus();
				ufs.file = file;
				ufs.length = long.Parse(targetManifestDict[file].length);
				OnFileFetchStarted(ufs);
				FileFetcher.FetchFile(file, sourceUrl, baseDir);
				OnFileFetchEnded(ufs);
			}
			foreach (string file in removedFiles) {
				UpdateFileStatus ufs = new UpdateFileStatus();
				ufs.file = file;
				File.Delete(file);
				OnFileRemoved(ufs);
			}
			File.Copy("MANIFEST", "MANIFEST.cur");

			OnUpdateCompleted(updateStatus);
			OnStateChanged(UpdateState.UpdateEnded);
		}

		protected static void RegisterEntry(Dictionary<string, ManifestEntry> dict,
											ManifestEntry entry) {
			dict[entry.name] = entry;
		}

		public void GetManifestEntry(string asset) {
		}

		public static void ReadManifest(Dictionary<string, ManifestEntry> dict, 
										string manifestFile) {
			Stream stream = File.Open(manifestFile, FileMode.Open);
			StreamReader reader = new StreamReader(stream);
			try {
				ParseManifest(dict, reader);
			} finally {
				reader.Close();
				stream.Close();
			}
		}

		protected static void ParseManifest(Dictionary<string, ManifestEntry> dict,
											StreamReader reader) {
			ManifestEntry entry = null;
			for (; ; ) {
				string line = reader.ReadLine();
				if (line == null) {
					if (entry != null) {
						RegisterEntry(dict, entry);
						entry = null;
					}
					break;
				} else if (line.Trim().Length == 0) {
					if (entry != null) {
						RegisterEntry(dict, entry);
						entry = null;
					}
					continue;
				}

				if (!line.Contains(":")) {
					Trace.TraceWarning("Invalid line: " + line);
					continue;
				}
				char[] delims = { ':' };
				string[] tmp = line.Split(delims, 2);
				string attr = tmp[0];
				if (tmp[1].Length <= 1 || !tmp[1].StartsWith(" ")) {
					Trace.TraceWarning("Invalid value string: " + tmp[1]);
					continue;
				}
				string val = tmp[1].Substring(1); // trim off leading space

				switch (attr) {
					case "Name":
						entry = new ManifestEntry();
						entry.name = val;
						break;
					case "SHA-Digest":
					// case "MD5-Digest":
						if (entry == null)
							Trace.TraceWarning("Got digest without name");
						else
							entry.digest = val;
						break;
					case "Content-Length":
						if (entry == null)
							Trace.TraceWarning("Got length without name");
						else
							entry.length = val;
						break;
					case "Content-Type":
						break;
					default:
						Trace.TraceInformation("Unexpected attribute: " + attr);
						break;
				}
			}
		}
		public void Update() {
			BuildManifest("MANIFEST.cur", ".\\", "Media", full_scan);
			ProcessManifest("MANIFEST.cur");
		}

		public bool FullScan {
			get {
				return full_scan;
			}
			set {
				full_scan = value;
			}
		}
	}
}
