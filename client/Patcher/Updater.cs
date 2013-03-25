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
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Net;
using System.Net.Cache;
using System.Xml;
using System.Security.Cryptography;

namespace Multiverse.Patcher {
    public class RepositoryInfo {
        public string version;
        public string url;
        public List<IgnoreEntry> excludes = new List<IgnoreEntry>();
    }

    public class IgnoreEntry {
        public IgnoreEntry(RepositoryInfo rep, string pattern) {
            this.repository = rep;
            this.pattern = pattern;
            this.regex = new Regex(pattern);
        }
        RepositoryInfo repository;
        Regex regex;
        string pattern;

        public RepositoryInfo Repository {
            get {
                return repository;
            }
        }
        public Regex Regex {
            get {
                return regex;
            }
        }
        public string Pattern {
            get {
                return pattern;
            }
        }
    }
    
    public class ManifestEntry {
        public RepositoryInfo repository;
        public string name;
		public string type;
		public byte[] digest;
		public long length;

        public string DigestString {
            get {
                StringBuilder digestStr = new StringBuilder();
                if (digest == null)
                    return string.Empty;
                foreach (byte b in digest)
                    digestStr.Append(b.ToString("x2"));
                return digestStr.ToString();
            }
            set {
                string digestString = value;
                digest = new byte[digestString.Length / 2];
                for (int i = 0; i < digest.Length; ++i)
                    digest[i] = byte.Parse(digestString.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber);
            }
        }
	}

	public class FileFetcher {
        private long bytesFetched = 0;
        private long bytesTransferred = 0;

        public event ProgressEventHandler ProgressUpdate;

        public bool FetchFile(string filename, string srcUrl, string baseDir, byte[] digest) {
            long dummy = 0;
            return FetchFile(filename, srcUrl, baseDir, digest, out dummy, out dummy);
        }

        // Fetch a file named filename from the source url and put it in baseDir
        public bool FetchFile(string filename, string srcUrl, string baseDir, byte[] digest,
                              out long length, out long compressedLength)
        {
            string targetFile = Path.GetFullPath(Path.Combine(baseDir, filename));
            if (!targetFile.StartsWith(Path.GetFullPath(baseDir)))
                throw new Exception(string.Format("Illegal filename: {0}", filename));
            // Right now, filename is in the unix format.  Convert it to the system format,
            // and also create any directories we need.
            string dirname = "";
            string[] parts = filename.Split('/');
            for (int i = 0; i < parts.Length - 1; ++i) {
                dirname = Path.Combine(dirname, parts[i]);
                if (!Directory.Exists(Path.Combine(baseDir, dirname)))
                    Directory.CreateDirectory(Path.Combine(baseDir, dirname));
            }
            return DownloadFile(srcUrl + filename, Path.Combine(baseDir, filename), digest, out length, out compressedLength);
        }

        public bool FetchFileAsync(WebClient client, string filename, string srcUrl, string baseDir, byte[] digest,
                                   out long length, out long compressedLength)
        {
            string targetFile = Path.GetFullPath(Path.Combine(baseDir, filename));
            if (!targetFile.StartsWith(Path.GetFullPath(baseDir)))
                throw new Exception(string.Format("Illegal filename: {0}", filename));
            // Right now, filename is in the unix format.  Convert it to the system format,
            // and also create any directories we need.
            string dirname = "";
            string[] parts = filename.Split('/');
            for (int i = 0; i < parts.Length - 1; ++i)
            {
                dirname = Path.Combine(dirname, parts[i]);
                if (!Directory.Exists(Path.Combine(baseDir, dirname)))
                    Directory.CreateDirectory(Path.Combine(baseDir, dirname));
            }
            return DownloadFile(srcUrl + filename, Path.Combine(baseDir, filename), digest, out length, out compressedLength);
        }

        /// <summary>
        ///   Download a stream into a memory stream.
        ///   In order to determine how much we are downloading from the
        ///   network, we need to download it into a temporary stream, 
        ///   then use the GzipStream or the like to decode this stream.
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        protected Stream DownloadStream(Stream inStream)
        {
            long bytes = 0; // reset the bytes downloaded
            Stream outStream = new MemoryStream();
            byte[] buffer = new byte[8192];
            int len = 0;
            do {
                len = inStream.Read(buffer, 0, buffer.Length);
                outStream.Write(buffer, 0, len);
                bytes += len;
                if (ProgressUpdate != null)
                    ProgressUpdate(this, bytes);
            } while (len > 0);
            // Rewind the stream to the start
            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }

        protected bool DownloadFile(string sourceUrl, string destPath, byte[] digest, 
                                    out long length, out long compressedLength) {
            WebRequest request = WebRequest.Create(sourceUrl);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            if (request is HttpWebRequest)
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            // Stream requestStream = request.GetRequestStream();
            // requestStream.Close();
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            compressedLength = 0;
            length = 0;
            // compressedLength = responseStream.Length;
            if (response is HttpWebResponse) {
                HttpWebResponse httpResponse = response as HttpWebResponse;
                if (httpResponse.ContentEncoding.ToLowerInvariant().Contains("gzip")) {
                    Stream tmpStream = DownloadStream(responseStream);
                    compressedLength = tmpStream.Length;
                    responseStream = new GZipStream(tmpStream, CompressionMode.Decompress);
                } else if (httpResponse.ContentEncoding.ToLowerInvariant().Contains("deflate")) {
                    Stream tmpStream = DownloadStream(responseStream);
                    compressedLength = tmpStream.Length;
                    responseStream = new DeflateStream(tmpStream, CompressionMode.Decompress);
                }
            }
            // length = responseStream.Length;
            long bytes = 0;
            Stream outStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            HashAlgorithm hash = SHA1.Create();
            byte[] buffer = new byte[8192];
            int len = 0;
            do {
                len = responseStream.Read(buffer, 0, buffer.Length);
                outStream.Write(buffer, 0, len);
                hash.TransformBlock(buffer, 0, len, buffer, 0);
                bytes += len;
                if (ProgressUpdate != null)
                    ProgressUpdate(this, bytes);
            } while (len > 0);
            hash.TransformFinalBlock(buffer, 0, 0);
            length += outStream.Length;
            if (compressedLength == 0)
                compressedLength = length;
            bytesTransferred += compressedLength;
            outStream.Close();
            responseStream.Close();
            if (digest != null) {
                byte[] hashCode = hash.Hash;
                Debug.Assert(digest.Length == hashCode.Length);
                for (int i = 0; i < hashCode.Length; ++i)
                    if (digest[i] != hashCode[i])
                        return false;
            }
            bytesFetched += length;
            return true;
        }

        #region Properties

        /// <summary>
        ///   This returns how many bytes we have downloaded, and put on local 
        ///   disk.  This may be less than the number of bytes we transferred 
        ///   if we got some invalid downloads, or more if the transfer was 
        ///   compressed.
        /// </summary>
        public long BytesFetched {
            get {
                return bytesFetched;
            }
        }

        /// <summary>
        ///   This returns our best estimate of how many bytes we transferred.  
        ///   This doesn't account for any of the overhead from HTTP or TCP.
        /// </summary>
        public long BytesTransferred {
            get {
                return bytesTransferred;
            }
        }

        #endregion
	}

	public delegate void UpdateEventHandler(object sender, UpdaterStatus e);
	public delegate void UpdateFileEventHandler(object sender, UpdateFileStatus e);
    public delegate void UpdateStateEventHandler(object sender, int state);
    public delegate void ProgressEventHandler(object sender, long bytes);

	public class UpdaterStatus : EventArgs 
    {
		public long bytes;    
		public int files;
        public string message;

        public int filesFetched;
        public long bytesFetched;
        public long bytesTransferred;
    }
	public class UpdateFileStatus : EventArgs 
    {
		public string file;
		public long length;
        public long compressedLength;
	}
	public enum UpdateState {
		UpdateStarted = 1,
		UpdateEnded = 2,
		FetchingManifest = 3,
		ScanningFiles = 4,
		UpdatingFiles = 5,
		Unknown = 6
	}
	
    /// <summary>
    ///   Class that updates either media or client binaries.
    ///   Note that this class cannot link with any libraries
    ///   other than the .NET core, since we need to be able
    ///   to run the patcher without depending on any other files.
    /// 
    ///   One side effect of this is that we do not use the log4net
    ///   system here, relying on trace and file writing instead.
    /// </summary>
	public class Updater {
        public static string CurrentManifest = "mv.patch.cur";
        public static string NewManifest = "mv.patch";
        public static string Patcher = "patcher.exe";

		Dictionary<string, ManifestEntry> targetManifestDict;
		Dictionary<string, ManifestEntry> currentManifestDict;
        List<IgnoreEntry> targetIgnorePatterns;
        List<IgnoreEntry> targetExcludePatterns;
        List<IgnoreEntry> currentIgnorePatterns;
        List<IgnoreEntry> currentExcludePatterns;

        bool fullScan;

        string sourceUrl;
        string baseDirectory = string.Empty;
        string prefixDirectory = null;

        long bytesUpdated = 0;
        long bytesNeeded = 0;
        int filesUpdated = 0;
        int filesNeeded = 0;

        string error = null;

        public event UpdateEventHandler UpdateAborted;
		public event UpdateEventHandler UpdateStarted;
		public event UpdateEventHandler UpdateCompleted;
		public event UpdateFileEventHandler FileFetchStarted;
        public event UpdateFileEventHandler FileFetchEnded;
        public event UpdateFileEventHandler FileAdded;
        public event UpdateFileEventHandler FileModified;
        public event UpdateFileEventHandler FileRemoved;
        public event UpdateFileEventHandler FileIgnored;
        public event UpdateStateEventHandler StateChanged;
        public event UpdateEventHandler UpdateProgress;

        public StreamWriter log;

        private void LogFileFetchStarted(object sender, UpdateFileStatus msg) {
            if (log != null)
                log.WriteLine("Downloading File: " + msg.file + " ...");
        }
        private void LogFileFetchEnded(object sender, UpdateFileStatus msg) {
            if (log != null)
                log.WriteLine("Downloaded File: {0} ({1}/{2} bytes)", msg.file, msg.compressedLength, msg.length);
        }
        private void LogFileAdded(object sender, UpdateFileStatus msg) {
            if (log != null)
                log.WriteLine("Added file: " + msg.file);
        }
        private void LogFileRemoved(object sender, UpdateFileStatus msg) {
            if (log != null)
                log.WriteLine("Removed file: " + msg.file);
        }
        private void LogFileModified(object sender, UpdateFileStatus msg) {
            if (log != null)
                log.WriteLine("Modified file: " + msg.file);
        }
        private void LogFileIgnored(object sender, UpdateFileStatus msg) {
            if (log != null)
                log.WriteLine("Ignored file: " + msg.file);
        }
        private void LogStateChanged(object sender, int state) {
            UpdateState updateState = UpdateState.Unknown;
            updateState = (UpdateState)state;
            if (log != null)
                log.WriteLine("State Changed: " + updateState);
        }
        private void LogUpdateAborted(object sender, UpdaterStatus msg) {
            if (log != null)
                log.WriteLine("Update Aborted");
        }
        private void LogUpdateStarted(object sender, UpdaterStatus msg) {
            if (log != null)
                log.WriteLine("Updating Media ...");
        }
        private void LogUpdateCompleted(object sender, UpdaterStatus msg) {
            if (log != null)
                log.WriteLine("Update Succeeded: {0}/{1}", msg.bytesTransferred, msg.bytesFetched);
        }
        private void LogUpdateProgress(object sender, UpdaterStatus msg) {
            if (log != null)
                log.WriteLine("Bytes Fetched: {0}/{1}", msg.bytesFetched, msg.bytes);
        }
        /// <summary>
        ///    Sets up the log stream.
        ///    Sets up callbacks to write to the log file at the 
        ///    various stages of our udpate.
        /// </summary>
        public void SetupLog(string logFile) {
            if (log == null) {
                // This is the first time we have called SetupLog -- set up the hooks
                this.UpdateAborted += this.LogUpdateAborted;
                this.UpdateStarted += this.LogUpdateStarted;
                this.UpdateCompleted += this.LogUpdateCompleted;
                //this.UpdateProgress += this.LogUpdateProgress;
                this.FileFetchStarted += this.LogFileFetchStarted;
                this.FileFetchEnded += this.LogFileFetchEnded;
                this.FileAdded += this.LogFileAdded;
                this.FileRemoved += this.LogFileRemoved;
                this.FileModified += this.LogFileModified;
                this.FileIgnored += this.LogFileIgnored;
                this.StateChanged += this.LogStateChanged;
            } else {
                // We already had a log - close it and reopen
                log.Close();
            }
            log = new StreamWriter(logFile);
        }

        public void CloseLog()
        {
            if (log != null)
            {
                log.Close();
                log = null;
            }
        }

        protected void OnUpdateAborted(UpdaterStatus status) {
            if (UpdateAborted != null)
                UpdateAborted(this, status);
        }

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

        protected void OnUpdateProgress(UpdaterStatus status)
        {
            if (UpdateProgress != null)
                UpdateProgress(this, status);
        }

        protected void OnFileFetchEnded(UpdateFileStatus status)
        {
            if (FileFetchEnded != null)
                FileFetchEnded(this, status);
        }

        protected void OnFileAdded(UpdateFileStatus status) {
            if (FileAdded != null)
                FileAdded(this, status);
        }

        protected void OnFileRemoved(UpdateFileStatus status) {
            if (FileRemoved != null)
                FileRemoved(this, status);
        }

        protected void OnFileModified(UpdateFileStatus status) {
            if (FileModified != null)
                FileModified(this, status);
        }

        protected void OnFileIgnored(UpdateFileStatus status) {
            if (FileIgnored != null)
                FileIgnored(this, status);
        }

        protected void OnStateChanged(int state) {
			if (StateChanged != null)
				StateChanged(this, state);
		}

		protected void OnStateChanged(UpdateState state) {
			if (StateChanged != null)
				StateChanged(this, (int)state);
		}

		public Updater() {
            targetManifestDict = new Dictionary<string, ManifestEntry>();
            currentManifestDict = new Dictionary<string, ManifestEntry>();
            targetIgnorePatterns = new List<IgnoreEntry>();
            targetExcludePatterns = new List<IgnoreEntry>();
            currentIgnorePatterns = new List<IgnoreEntry>();
            currentExcludePatterns = new List<IgnoreEntry>();
        }

        /// <summary>
        ///   Check our version to see if we need to do the update.
        ///   This grabs the first 1kb of the file and expects to get
        ///   a version string in that chunk.
        /// </summary>
        /// <returns>true if we should run the updater</returns>
        public bool CheckVersion() {
            if (fullScan)
                return true;

            string local_version = null;
            string remote_version = null;

            // Get the local version
            try {
                Stream data = File.Open(Path.Combine(baseDirectory, CurrentManifest), FileMode.Open);
                try {
                    XmlReader xmlReader = XmlReader.Create(data);
                    if (xmlReader.ReadToFollowing("patch_data"))
                        local_version = xmlReader.GetAttribute("revision");
                } finally {
                    data.Close();
                }
            } catch (Exception e) {
                // Failed to fetch or parse the local manifest file
                Trace.TraceError("Failed to handle local version: " + e.Message);
                // We probably need to patch
                return true;
            }

            try {
                WebClient client = new WebClient();
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                Stream data;
                // It is extremely common for people to not have the right update url.
                // Assume they left off the trailing slash and try again if we do not
                // find the file.
                try
                {
                    data = client.OpenRead(this.UpdateUrl + NewManifest);
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse &&
                        ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        data = client.OpenRead(this.UpdateUrl + "/" + NewManifest);
                        log.Write("Incorrect Update URL Specification: Update URL should have a trailing slash.");
                        this.UpdateUrl = this.UpdateUrl + "/";
                    }
                    else
                    {
                        throw;
                    }
                }
                try {
                    // Read in the first 1kb of data from the remote stream
                    byte[] buffer = new byte[1024];
                    int offset = 0;
                    while (offset < buffer.Length) {
                        int count = data.Read(buffer, offset, buffer.Length - offset);
                        if (count == 0) // end of stream
                            break;
                        offset += count;
                    }

                    MemoryStream memStream = new MemoryStream(buffer);
                    XmlReader xmlReader = XmlReader.Create(memStream);
                    if (xmlReader.ReadToFollowing("patch_data"))
                        remote_version = xmlReader.GetAttribute("revision");
                } finally {
                    // Make sure we close the stream
                    data.Close();
                }
            } catch (Exception e) {
                // Failed to fetch or parse the remote manifest file
                Trace.TraceError("Failed to handle remote version: " + e.Message);
                // We probably need to patch
                return true;
            }


            if (local_version == null || remote_version == null)
                return true;
            
            if (local_version != remote_version) {
                Trace.TraceInformation("Update of world assets is required. Local: {0}; Remote: {1}", local_version, remote_version);
                return true;
            }

            return false;
        }

        public static void WriteManifest(string manifestFile,
                                         Dictionary<string, ManifestEntry> entries,
                                         List<IgnoreEntry> ignores, 
                                         RepositoryInfo repInfo) {
            XmlDocument doc = new XmlDocument();
            XmlNode pdNode = doc.CreateElement("patch_data");
            XmlAttribute revAttr = doc.CreateAttribute("revision");
            revAttr.Value = repInfo.version;
            pdNode.Attributes.Append(revAttr);
            XmlAttribute urlAttr = doc.CreateAttribute("url");
            urlAttr.Value = repInfo.url;
            pdNode.Attributes.Append(urlAttr);
            foreach (IgnoreEntry entry in ignores) {
                if (entry.Repository != repInfo)
                    // not an ignore for this repository - don't write it to this file
                    continue;
                XmlNode node = doc.CreateElement("ignore");
                XmlAttribute attr = doc.CreateAttribute("pattern");
                attr.Value = entry.Pattern;
                node.Attributes.Append(attr);
                pdNode.AppendChild(node);
            }
            foreach (IgnoreEntry entry in repInfo.excludes) {
                if (entry.Repository != repInfo)
                    // not an exclude for this repository - don't write it to this file
                    continue;
                XmlNode node = doc.CreateElement("exclude");
                XmlAttribute attr = doc.CreateAttribute("pattern");
                attr.Value = entry.Pattern;
                node.Attributes.Append(attr);
                pdNode.AppendChild(node);
            }
            foreach (ManifestEntry entry in entries.Values) {
                if (entry.repository != repInfo)
                    // not an entry for this repository - don't write it to this file
                    continue;
                XmlNode node = doc.CreateElement("entry");
                XmlAttribute nameAttr = doc.CreateAttribute("name");
                nameAttr.Value = entry.name;
                node.Attributes.Append(nameAttr);
                XmlAttribute kindAttr = doc.CreateAttribute("kind");
                kindAttr.Value = entry.type;
                node.Attributes.Append(kindAttr);
                if (entry.type == "file") {
                    XmlAttribute digestAttr = doc.CreateAttribute("sha1_digest");
                    digestAttr.Value = entry.DigestString;
                    node.Attributes.Append(digestAttr);
                    XmlAttribute sizeAttr = doc.CreateAttribute("size");
                    sizeAttr.Value = entry.length.ToString();
                    node.Attributes.Append(sizeAttr);
                }
                pdNode.AppendChild(node);
            }
            doc.AppendChild(pdNode);
            // Write the document to the current manifest file
            Stream stream = File.Open(manifestFile, FileMode.Create);
            try {
                StreamWriter writer = new StreamWriter(stream);
                try {
                    XmlWriter xmlWriter = XmlWriter.Create(writer);
                    doc.WriteTo(xmlWriter);
                    xmlWriter.Close();
                } finally {
                    writer.Close();
                }
            } finally {
                stream.Close();
            }
        }

        /// <summary>
        ///   Build a dictionary with entries for the various files and directories.
        ///   This method assumes that all the files covered by this prefix are in one
        ///   repository, though that may not be the case.  Better support for multiple
        ///   repositories in these methods will be added later.
        /// </summary>
        /// <param name="baseDir">Base directory to patch (e.g. C:\Foo\Bar)</param>
        /// <param name="prefix">Prefix to limit the scan</param>
        /// <param name="repInfo">Info about the repository this will be based on</param>
        protected void BuildManifestDict(string baseDir, string prefix, RepositoryInfo repInfo) {
            string[] dirs;
            if (prefix == null) {
                dirs = new string[1];
                dirs[0] = baseDir;
            } else {
                dirs = Directory.GetDirectories(baseDir, prefix);
            }
            List<IgnoreEntry> ignores = new List<IgnoreEntry>();
            // Regardless of what is in the patch data, ignore these three files
            ignores.Add(new IgnoreEntry(repInfo, CurrentManifest));
            ignores.Add(new IgnoreEntry(repInfo, NewManifest));
            ignores.Add(new IgnoreEntry(repInfo, Patcher));
            ignores.Add(new IgnoreEntry(repInfo, "")); // Ignore the existing name
            foreach (string dir in dirs)
                ProcessDir(baseDir, dir, ignores, repInfo);
		}

        /// <summary>
        ///   Build a dictionary with entries for the various files and directories.
        ///   Then write that information out to a file.
        /// </summary>
        /// <param name="currentManifestFile">file to which we will write the current manifest data</param>
        /// <param name="baseDir">full directory to start in (e.g. C:\Foo\Bar)</param>
        /// <param name="prefix">directory corresponding to baseDir in the manifest</param>
        /// <param name="repInfo">info about the version and source of data</param>
		private void BuildManifestFile(string currentManifestFile, string baseDir, string prefix, 
                                       RepositoryInfo repInfo) {
            BuildManifestDict(baseDir, prefix, repInfo);
            
            WriteManifest(currentManifestFile, currentManifestDict, currentIgnorePatterns, repInfo);
		}

        /// <summary>
        ///   Build up information about the existing files
        /// </summary>
        /// <param name="prefix">prefix to limit the search to</param>
        /// <param name="fullScan">true to scan all files or false to use the exising manifest file</param>
        private void BuildManifest(string prefix, bool fullScan, List<IgnoreEntry> excludes) {
            string baseDir = baseDirectory;
            string currentManifestFile = Path.Combine(baseDir, CurrentManifest);

            OnStateChanged(UpdateState.ScanningFiles);
            if (!fullScan) {
                try {
                    LoadManifest(currentManifestFile);
                } catch (FileNotFoundException) {
                    fullScan = true;
                }
            }
            if (fullScan) {
                RepositoryInfo nullRep = new RepositoryInfo();
                nullRep.url = "http://localhost/";
                nullRep.version = "0";
                foreach (IgnoreEntry entry in excludes)
                    nullRep.excludes.Add(new IgnoreEntry(nullRep, entry.Pattern));

                BuildManifestFile(currentManifestFile, baseDir, prefix, nullRep);
            }
        }
        
        public void LoadManifest(string manifestFile) {
			ReadManifest(currentManifestDict, currentIgnorePatterns, currentExcludePatterns, manifestFile);
		}

        /// <summary>
        ///   Process the given file and add an entry for it in the 
        ///   currentManifestDict map
        /// </summary>
        /// <param name="name">name of the file (relative to base dir)</param>
        /// <param name="file">full path of the file</param>
        /// <param name="repInfo">info about the repository</param>
		protected void ProcessFile(string name, string file, RepositoryInfo repInfo) {
			ManifestEntry entry = new ManifestEntry();
            entry.repository = repInfo;
			entry.name = name;
            entry.type = "file";
            try {
                Stream data = File.Open(file, FileMode.Open);
                try {
                    long fileLength = data.Length;
                    entry.length = fileLength;
                    HashAlgorithm digester = new SHA1CryptoServiceProvider();
                    byte[] digest = digester.ComputeHash(data);
                    entry.digest = digest;
                } finally {
                    data.Close();
                }
            } catch (IOException e) {
                Trace.TraceError("Failed to access file: {0} - {1}", file, e.Message);
                entry.length = 0;
                entry.digest = null;
            }
			currentManifestDict[entry.name] = entry;
		}

        /// <summary>
        ///   Process a directory (including subdirectories) to generate 
        ///   information about the files and directories that are present.
        ///   This will be used in a later pass to determine what actions
        ///   must be taken to bring this in line with the patch info.
        ///   An entry for the directory will be added to the 
        ///   currentManifestDict map.
        /// </summary>
        /// <param name="baseDir">Base directory to patch (e.g. C:\Foo\Bar)</param>
        /// <param name="dir">directory we are scanning (e.g. C:\Foo\Bar\Interface\FrameXML)</param>
        /// <param name="ignores">list of entries that should be ignored</param>
        /// <param name="repInfo">information about the repository</param>
        protected void ProcessDir(string baseDir, string dir, List<IgnoreEntry> ignores, RepositoryInfo repInfo) {
			string[] dirs = Directory.GetDirectories(dir);
			string[] files = Directory.GetFiles(dir);

            // Add an entry for our directory
            string shortDir = dir.Substring(baseDir.Length);
            shortDir = shortDir.Replace('\\', '/');
            if (shortDir.StartsWith("/"))
                shortDir = shortDir.Substring(1);
            // If we are excluded from the manifest, just return
            // Do not process subdirectories or files
            if (IsExcluded(repInfo.excludes, shortDir))
                return;

            if (!IsIgnored(ignores, shortDir)) {
                ManifestEntry entry = new ManifestEntry();
                entry.repository = repInfo;
                entry.name = shortDir;
                entry.type = "dir";
                entry.length = 0;
                entry.digest = null;
                currentManifestDict[entry.name] = entry;
            }

            // Add entries for the files in this directory
            foreach (string file in files) {
                if (file.StartsWith(baseDir)) {
                    string name = file.Substring(baseDir.Length);
                    name = name.Replace('\\', '/');
                    if (name.StartsWith("/"))
                        name = name.Substring(1);
                    if (IsIgnored(ignores, name))
                        continue;
                    ProcessFile(name, file, repInfo);
                }
            }

            // Process our subdirectories
            foreach (string subdir in dirs)
                ProcessDir(baseDir, subdir, ignores, repInfo);
		}

        public static bool IsIgnored(List<IgnoreEntry> ignores, string name) {
            foreach (IgnoreEntry entry in ignores) {
                Match m = entry.Regex.Match(name);
                if (m.Success && m.Length == name.Length)
                    // Full match
                    return true;
            }
            return false;
        }
        
        public static bool IsExcluded(List<IgnoreEntry> excludes, string name) {
            foreach (IgnoreEntry entry in excludes) {
                Match m = entry.Regex.Match(name);
                if (m.Success && m.Length == name.Length)
                    // Full match
                    return true;
            }
            return false;
        }

        protected void fetcher_ProgressUpdate(object sender, long bytes)
        {
            UpdaterStatus updateStatus = this.UpdaterStatus;
            updateStatus.bytesFetched += bytes;
            OnUpdateProgress(updateStatus);
        }

        /// <summary>
        ///   Examine the local manifest and the remote manifest, and build
        ///   a concept of what files need to be added/removed/modified.
        /// </summary>
        public void ProcessManifest(FileFetcher fetcher) {
            string baseDir = baseDirectory;
            string currentManifestFile = Path.Combine(baseDir, CurrentManifest);
            string newManifestFile = Path.Combine(baseDir, NewManifest);

            ReadManifest(currentManifestDict, currentIgnorePatterns, currentExcludePatterns, currentManifestFile);

            OnStateChanged(UpdateState.FetchingManifest);
            fetcher.FetchFile(NewManifest, sourceUrl, baseDir, null);

            ReadManifest(targetManifestDict, targetIgnorePatterns, targetExcludePatterns, newManifestFile);

            fetcher.ProgressUpdate += fetcher_ProgressUpdate;
			bytesNeeded = 0;
			filesNeeded = 0;

			List<string> newFiles = new List<string>();
			List<string> removedFiles = new List<string>();
			List<string> modifiedFiles = new List<string>();
			foreach (string key in targetManifestDict.Keys) {
                ManifestEntry targetEntry = targetManifestDict[key];
                if (targetEntry.type == "dir") {
                    if (IsIgnored(targetIgnorePatterns, targetEntry.name))
                        Debug.Assert(false, "Ignoring directory: " + targetEntry.name);
                }
                // if the entry is ignored, just skip it
                if (IsIgnored(targetIgnorePatterns, targetEntry.name))
                    continue;
                if (targetEntry.type == "dir") {
                    MakeDirectory(targetEntry.name);
                    if (!currentManifestDict.ContainsKey(key)) {
                        UpdateFileStatus ufs = new UpdateFileStatus();
                        ufs.file = targetEntry.name;
                        ufs.length = 0;
                        OnFileAdded(ufs);
                    }
                    continue;
                }
				if (!currentManifestDict.ContainsKey(key)) {
					newFiles.Add(key);
                    bytesNeeded += targetEntry.length;
					filesNeeded++;
				} else {
                    ManifestEntry currentEntry = currentManifestDict[key];
                    Debug.Assert(currentEntry.digest != null);
                    Debug.Assert(targetEntry.digest != null);
                    Debug.Assert(currentEntry.digest.Length == targetEntry.digest.Length);
                    bool matches = true;
                    for (int i = 0; i < currentEntry.digest.Length; ++i) {
                        if (currentEntry.digest[i] != targetEntry.digest[i]) {
                            matches = false;
                            break;
                        }
                    }
                    if (!matches) {
                        modifiedFiles.Add(key);
                        bytesNeeded += targetEntry.length;
                        filesNeeded++;
                    }
				}
			}
			foreach (string key in currentManifestDict.Keys) {
                if (IsIgnored(targetIgnorePatterns, key)) {
                    UpdateFileStatus ufs = new UpdateFileStatus();
                    ufs.file = key;
                    OnFileIgnored(ufs);
                    continue;
                }
                // TODO: this should be cleaned up to prevent me from
                // removing entries managed by another repository
				if (!targetManifestDict.ContainsKey(key))
					removedFiles.Add(key);
			}

			// At this point, we are about to start fetching new files,
			// so remove our existing manifest.  This will cause us to 
			// do a full scan if our download is interrupted, which will
			// be more efficient than fetching everything again.
            File.Delete(currentManifestFile);

			UpdaterStatus updateStatus = new UpdaterStatus();
			updateStatus.files = filesNeeded;
            updateStatus.filesFetched = 0;
			updateStatus.bytes = bytesNeeded;
            updateStatus.bytesFetched = 0;
            updateStatus.bytesTransferred = 0;
			OnUpdateStarted(updateStatus);
			OnStateChanged(UpdateState.UpdatingFiles);
            
            // Sort and reverse the removed files so that subdirectory entries are listed
            // before their parent directories.
            removedFiles.Sort();
            removedFiles.Reverse();
			foreach (string file in removedFiles) {
                UpdateFileStatus ufs = new UpdateFileStatus();
				ufs.file = file;
                string fname = Path.Combine(baseDir, file);
                if (Directory.Exists(fname))
                    Directory.Delete(fname);
                else if (File.Exists(fname))
                    File.Delete(fname);
                else
                    Debug.Assert(false, "Unexpected removal of entry: " + fname + string.Format(" - '{0}' + '{1}'", baseDir, file));
				OnFileRemoved(ufs);
			}
            // Handle modified files
            foreach (string file in modifiedFiles) {
                long lastBytesUpdated = bytesUpdated;
                UpdateFileStatus ufs = new UpdateFileStatus();
                ufs.file = file;
                ufs.length = targetManifestDict[file].length;
                OnFileFetchStarted(ufs);
                // Trace.TraceInformation("Fetching modified file: {0}", file);
                // Trace.TraceInformation("Old Digest: {0}", currentManifestDict[file].DigestString);
                // Trace.TraceInformation("New Digest: {0}", targetManifestDict[file].DigestString);
                long length, compressedLength;
                if (!fetcher.FetchFile(file, sourceUrl, baseDir, targetManifestDict[file].digest,
                                       out length, out compressedLength))
                    throw new Exception(string.Format("Unable to retrieve valid file: '{0}'", file));
                if (length != ufs.length) {
                    string msg = string.Format("Unable to retrieve valid file: '{0}' - size mismatch (got {1} but expected {2})",
                                               file, length, ufs.length);
                    throw new Exception(msg);
                }
                ufs.compressedLength = compressedLength;
                OnFileFetchEnded(ufs);
                OnFileModified(ufs);
                filesUpdated++;
                bytesUpdated = lastBytesUpdated + ufs.length;
            }
            // Handle new files
            foreach (string file in newFiles) {
                long lastBytesUpdated = bytesUpdated;
                UpdateFileStatus ufs = new UpdateFileStatus();
                ufs.file = file;
                ufs.length = targetManifestDict[file].length;
                OnFileFetchStarted(ufs);
                // Trace.TraceInformation("Fetching new file: {0}", file);
                long length, compressedLength;
                if (!fetcher.FetchFile(file, sourceUrl, baseDir, targetManifestDict[file].digest,
                                       out length, out compressedLength))
                    throw new Exception(string.Format("Unable to retrieve valid file: '{0}'", file));
                if (length != ufs.length) {
                    string msg = string.Format("Unable to retrieve valid file: '{0}' - size mismatch (got {1} but expected {2})",
                                               file, length, ufs.length);
                    throw new Exception(msg);
                }
                ufs.compressedLength = compressedLength;
                OnFileFetchEnded(ufs);
                OnFileAdded(ufs);
                filesUpdated++;
                bytesUpdated = lastBytesUpdated + ufs.length;
            }

            File.Copy(newManifestFile, currentManifestFile);

            updateStatus.filesFetched = filesUpdated;
            updateStatus.bytesFetched = fetcher.BytesFetched;
            updateStatus.bytesTransferred = fetcher.BytesTransferred;

			OnUpdateCompleted(updateStatus);
			OnStateChanged(UpdateState.UpdateEnded);
		}

        protected void MakeDirectory(string dir) {
            if (!Directory.Exists(Path.Combine(baseDirectory, dir)))
                Directory.CreateDirectory(Path.Combine(baseDirectory, dir));
        }
		protected static void RegisterEntry(Dictionary<string, ManifestEntry> dict,
											ManifestEntry entry) {
			dict[entry.name] = entry;
		}

		public void GetManifestEntry(string asset) {
		}

		public static void ReadManifest(Dictionary<string, ManifestEntry> dict, 
										List<IgnoreEntry> ignorePatterns,
                                        List<IgnoreEntry> excludePatterns,
                                        string manifestFile) {
			Stream stream = File.Open(manifestFile, FileMode.Open);
			try {
                StreamReader reader = new StreamReader(stream);
                try {
                    ParseManifest(dict, ignorePatterns, excludePatterns, reader);
                } finally {
                    reader.Close();
                }
            } finally {
				stream.Close();
			}
		}

        protected static void ParseManifest(Dictionary<string, ManifestEntry> entries,
                                            List<IgnoreEntry> ignores,
                                            List<IgnoreEntry> excludes,
                                            StreamReader reader) {
			XmlDocument document = new XmlDocument();
			document.Load(reader);
            foreach (XmlNode childNode in document.ChildNodes) {
                switch (childNode.Name) {
                    case "xml":
                        // this is the xml header
                        break;
                    case "patch_data":
                        ReadPatchData(entries, ignores, excludes, childNode);
                        break;
                    default:
                        Debug.Assert(false, "Invalid xml");
                        break;
                }
            }

        }
        protected static void ReadPatchData(Dictionary<string, ManifestEntry> entries,
                                            List<IgnoreEntry> ignores,
                                            List<IgnoreEntry> excludes,
                                            XmlNode node) {
            RepositoryInfo repInfo = new RepositoryInfo();
            repInfo.version = node.Attributes["revision"].Value.ToString();
            repInfo.url = node.Attributes["url"].Value.ToString();
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "entry":
                        ReadEntry(entries, repInfo, childNode);
                        break;
                    case "ignore":
                        ReadIgnore(ignores, repInfo, childNode);
                        break;
                    case "exclude":
                        ReadExclude(excludes, repInfo, childNode);
                        break;
                    default:
                        Debug.Assert(false, "Invalid xml");
                        break;
                }
            }
        }
        protected static void ReadEntry(Dictionary<string, ManifestEntry> entries,
                                        RepositoryInfo repInfo, XmlNode node) {
            ManifestEntry entry = new ManifestEntry();
            entry.repository = repInfo;
            entry.name = node.Attributes["name"].Value.ToString();
            entry.type = node.Attributes["kind"].Value.ToString();
            if (entry.type == "file") {
                entry.DigestString = node.Attributes["sha1_digest"].Value.ToString();
                entry.length = long.Parse(node.Attributes["size"].Value);
            }
            entries[entry.name] = entry;
        }
        protected static void ReadIgnore(List<IgnoreEntry> ignores,
                                         RepositoryInfo repInfo, XmlNode node) {
            string pattern = node.Attributes["pattern"].Value.ToString();
            ignores.Add(new IgnoreEntry(repInfo, pattern));
        }
        protected static void ReadExclude(List<IgnoreEntry> excludes,
                                          RepositoryInfo repInfo, XmlNode node) {
            string pattern = node.Attributes["pattern"].Value.ToString();
            excludes.Add(new IgnoreEntry(repInfo, pattern));
        }

        public void Update() {
            FileFetcher fetcher = new FileFetcher();
            try {
                // Begin the update
                OnStateChanged(UpdateState.UpdateStarted);

                // Make sure the base directory exists
                if (!Directory.Exists(baseDirectory))
                    Directory.CreateDirectory(baseDirectory);

                // Grab the manifest
                OnStateChanged(UpdateState.FetchingManifest);
                fetcher.FetchFile(NewManifest, sourceUrl, baseDirectory, null);

                ReadManifest(targetManifestDict, targetIgnorePatterns, targetExcludePatterns, Path.Combine(baseDirectory, NewManifest));

                //List<Regex> clientExcludes = new List<Regex>();
                //clientExcludes.Add(new Regex("Media/.*"));

                BuildManifest(prefixDirectory, fullScan, targetExcludePatterns);
                ProcessManifest(fetcher);
            } catch (Exception e) {
                Trace.TraceError("Error updating media: {0}", e.StackTrace);
                error = string.Format("Error updating media: {0}", e.Message);
                if (log != null)
                    log.Write(error);
                UpdaterStatus status = new UpdaterStatus();
                status.message = error;
                OnUpdateAborted(status);
            }
            if (log != null) {
                log.Close();
                log = null;
            }
		}

        public UpdaterStatus UpdaterStatus {
            get {
                UpdaterStatus updateStatus = new UpdaterStatus();
                updateStatus.bytes = bytesNeeded;
                updateStatus.files = filesNeeded;
                updateStatus.filesFetched = filesUpdated;
                updateStatus.bytesFetched = bytesUpdated;
                return updateStatus;
            }
        }

		public bool FullScan {
			get {
				return fullScan;
			}
			set {
				fullScan = value;
			}
		}

        public string BaseDirectory {
            get {
                return baseDirectory;
            }
            set {
                baseDirectory = value;
            }
        }

        public string PrefixDirectory {
            get {
                return prefixDirectory;
            }
            set {
                prefixDirectory = value;
            }
        }

        public string UpdateUrl {
            get {
                return sourceUrl;
            }
            set {
                sourceUrl = value;
            }
        }

        public string Error {
            get {
                return error;
            }
        }

    }
}
