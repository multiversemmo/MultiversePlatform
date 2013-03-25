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
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;

using log4net;

using Axiom.Utility;

using Multiverse.Base;
using Multiverse.Config;
using Multiverse.Lib.LogUtil;

namespace Multiverse
{

    /// <summary>
    ///     Demo browser entry point.
    /// </summary>
	public class MultiverseClient
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MultiverseClient));

        // The version file in the directory above the client
        public static string VersionFile = "patch_version.txt";
        public static string WorldSettingsFile = "world_settings.xml";
        public static string Patcher = "patcher.exe";
        /// <summary>
        ///   Default patch base url.  We need this here, because the client
        ///   needs to check that it is up to date, and fetch the patcher if
        ///   required.
        /// </summary>
        public static string UpdateUrl = "http://update.multiverse.net/mvupdate.client/";
        /// <summary>
        ///   The base for login urls.  The --login_page argument is appended 
        ///   to this.
        /// </summary>
        public static string LoginBase = "http://login.multiverse.net/";

        private static string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string ClientAppDataFolder = Path.Combine(MyDocumentsFolder, "Multiverse World Browser");
        private static string ConfigFolder = Path.Combine(ClientAppDataFolder, "Config");
        private static string LogFolder = Path.Combine(ClientAppDataFolder, "Logs");
        private static string ScreenshotsFolder = Path.Combine(ClientAppDataFolder, "Screenshots");
        private static string FallbackLogfile = Path.Combine(LogFolder, "MultiverseClient.log");

        private static void SetupUserFolders()
        {
            if (!Directory.Exists(ClientAppDataFolder))
                Directory.CreateDirectory(ClientAppDataFolder);

            // Set up log configuration folders
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
                // Note that the DisplaySettings.xml should also show up in this folder.

            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            if (!Directory.Exists(ScreenshotsFolder))
                Directory.CreateDirectory(ScreenshotsFolder);

            string worldsFolder = Path.Combine(ClientAppDataFolder, "Worlds");
            if (!Directory.Exists(worldsFolder))
                Directory.CreateDirectory(worldsFolder);
        }

        private static string GetClientVersion(Stream stream) {
            string version = string.Empty;
            try {
                TextReader reader = new StreamReader(stream);
                try {
                    string line = reader.ReadLine();
                    version = line.Trim();
                } finally {
                    reader.Close();
                }
            } catch (Exception e) {
                LogUtil.ExceptionLog.InfoFormat("Failed to parse version: {0}", e);
            }
            return version;
        }

        private static bool Patch(string updateUrl, string patcherUrl, string[] extraArgs, bool forceScan, bool exit_after_patch) {
            string localVersion = string.Empty;
            try {
                localVersion = GetClientVersion(File.Open("../" + VersionFile, FileMode.Open, FileAccess.Read));
            } catch (Exception e) {
                LogUtil.ExceptionLog.ErrorFormat("Unable to determine local version: {0}", e);
            }
            string remoteVersion = string.Empty;
            // Pull the remote version
            WebClient webClient = new WebClient();
            webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            string localUpdateUrl = (updateUrl == null) ? UpdateUrl : updateUrl;
            Stream webStream;
            // It is extremely common for people to not have the right update url.
            // Assume they left off the trailing slash and try again if we do not
            // find the file.
            try
            {
                webStream = webClient.OpenRead(localUpdateUrl + VersionFile);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse &&
                    ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    webStream = webClient.OpenRead(localUpdateUrl + "/" + VersionFile);
                    log.Warn("Incorrect Update URL Specification: Update URL should have a trailing slash.");
                    localUpdateUrl = localUpdateUrl + "/";
                }
                else
                {
                    throw;
                }
            }
            try {
                remoteVersion = GetClientVersion(webStream);
            } catch (Exception e) {
                LogUtil.ExceptionLog.ErrorFormat("Failed to retrieve remote version: {0}", e);
            } finally {
                webStream.Close();
            }
            if (localVersion == remoteVersion)
            {
                log.InfoFormat("Client version up to date: {0}", localVersion);
                // we are up to date.. 
                if (!forceScan)
                    return false;
                log.InfoFormat("Forcing patch from {0} to {1}", localVersion, remoteVersion);
            } else {
                log.InfoFormat("Patching from {0} to {1}", localVersion, remoteVersion);
            }
            // Pull the updated patcher, and spawn it
            log.Info("Downloading patcher...");
            string currentDir = Directory.GetCurrentDirectory();
            string parentDir = Directory.GetParent(currentDir).FullName;
            if ((parentDir + "\\bin") != currentDir) {
                Debug.Assert(false, "Client must be run from the bin folder.\nIf you are running a development build, you should use the --noupdate command line option.  Current folder is: " + currentDir);
                return true;
            }
            try
            {
                webClient.DownloadFile(localUpdateUrl + Patcher, Path.Combine(Path.GetTempPath(), Patcher));
            }
            catch (Exception e)
            {
                log.Error("Failed to download patcher" + e.ToString());
            }
            Process clientProcess = Process.GetCurrentProcess();
            string cmdArgs = "--parent_id " + clientProcess.Id.ToString();
            if (updateUrl != null)
                cmdArgs += " --update_url " + updateUrl;
            if (patcherUrl != null)
                cmdArgs += " --patcher_url " + patcherUrl;
            if (exit_after_patch)
                cmdArgs += " --exit";
            foreach (string extraArg in extraArgs)
                cmdArgs += " \"" + extraArg + "\"";
            log.DebugFormat("Patcher arguments: {0}", cmdArgs);
            ProcessStartInfo psi = 
                new ProcessStartInfo(Path.Combine(Path.GetTempPath(), Patcher), cmdArgs);
            psi.WorkingDirectory = parentDir;
            try
            {
                Process p = Process.Start(psi);
                log.Info("Launched patcher..." + p.ToString());
            }
            catch (Exception)
            {
                // Probably didn't have permission on Vista
                log.Error("Failed to launch patcher...");
            }
            return true;
        }

        /// <summary>
        ///   Make sure that the repository thresholds are at least as high 
        ///   as the level we set here.  This does not allow us to lower the
        ///   logging level.  That would be complex, because even in cases 
        ///   where we don't want heavy logging, we still want to make sure
        ///   we get our status messages.
        /// </summary>
        /// <param name="loglevel"></param>
        /// <param name="force"></param>
        private static void SetLogLevel(string loglevel, bool force) {
            log4net.Core.Level logLevel = log4net.Core.Level.Off;
            switch (loglevel.ToLowerInvariant()) {
                case "0":
                case "debug":
                    logLevel = log4net.Core.Level.Debug;
                    break;
                case "1":
                case "2":
                case "info":
                    logLevel = log4net.Core.Level.Info;
                    break;
                case "3":
                case "warn":
                    logLevel = log4net.Core.Level.Warn;
                    break;
                case "4":
                case "error":
                    logLevel = log4net.Core.Level.Error;
                    break;
                default:
                    log.WarnFormat("Invalid log level: {0}", loglevel);
                    return;
            }
            LogUtil.SetLogLevel(logLevel, force);
        }

        [STAThread]
		private static void Main(string[] args) {
            // Changes the CurrentCulture of the current thread to the invariant
            // culture.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // Setup any per-user folder structer that we need.
            SetupUserFolders();

            List<string> extraArguments = new List<string>();
            try {
                bool patch = true;
#if PERFHUD_BUILD
                patch = false;
#endif
                bool unhandledArguments = false;
                string logFileOverride = null;
                string updateUrl = null;
                string patchUrl = null;
                bool force_scan = false;
                bool exit_after_patch = false;
                for (int i = 0; i < args.Length; ++i) {
                    switch (args[i]) {
                        case "--noupdate":
                        case "--no_client_update":
                            patch = false;
                            break;
                        case "--update_url":
                            Debug.Assert(i + 1 < args.Length);
                            updateUrl = args[++i];
                            break;
                        case "--patch_url":
                        case "--patcher_url":
                            Debug.Assert(i + 1 < args.Length);
                            patchUrl = args[++i];
                            break;
                        case "--force_scan":
                            force_scan = true;
                            break;
                        // Now handle arguments we want to pass through
                        case "--master":
                        case "--world":
                        case "--login_page":
                        case "--world_settings_file":
                        case "--log_level":
                            Debug.Assert(i + 1 < args.Length);
                            extraArguments.Add(args[i]);
                            extraArguments.Add(args[++i]);
                            break;
                        case "--log_config":
                            Debug.Assert(i + 1 < args.Length);
                            logFileOverride = args[++i];
                            break;
                        default:
                            if (!exit_after_patch)
                                unhandledArguments = true;
                            exit_after_patch = true;
                            break;
                    }
                }

                // initialize logging
                bool interactive = System.Windows.Forms.SystemInformation.UserInteractive;
                string logConfigFile = Path.Combine(ConfigFolder, "LogConfig.xml");
                if (logFileOverride != null)
                    logConfigFile = logFileOverride;
                LogUtil.InitializeLogging(logConfigFile, Path.Combine("..", "DefaultLogConfig.xml"), FallbackLogfile, interactive);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (string s in args)
                {
                    sb.AppendFormat("{0} ", s);
                }

                log.InfoFormat("Client command line arguments: {0}", sb.ToString());

                if (unhandledArguments)
                    log.Info("Got extra arguments; Will exit after patch if a client patch is required.");

                if (patch) {
                    log.Info("Checking if restart required");
                    try {
                        if (Patch(updateUrl, patchUrl, extraArguments.ToArray(), force_scan, exit_after_patch)) {
                            log.Info("Restart required");
                            return;
                        }
                    } catch (Exception e) {
                        LogUtil.ExceptionLog.ErrorFormat("Version determination failed: {0}", e);
                        throw;
                    }
                }

                string errorMessage = null;
                string errorPage = null;

				// this.Hide();
				// this.WindowState = FormWindowState.Minimized;
				// create an instance of the client class and start it up 
				// We use the using declaration here so that we dispose of the object
				using (Client client = new Multiverse.Base.Client()) {
#if PERFHUD_BUILD
                    client.standalone = true;
                    client.PatchMedia = false;
                    client.Repository = "../../../Media/";
                    // Debug.Assert(false, "Breakpoint for Debug");
#endif
					for (int i = 0; i < args.Length; ++i) {
						switch (args[i]) {
							case "--standalone":
								client.standalone = true;
                                client.WorldId = "standalone";
								break;
							case "--debug":
								client.doTraceConsole = true;
								break;
							case "--simple_terrain":
								client.simpleTerrain = true;
								break;
							case "--config":
								client.doDisplayConfig = true;
								break;
							case "--tocFile":
								Debug.Assert(i + 1 < args.Length);
								client.uiModules.Add(args[++i]);
								break;
							case "--master":
								Debug.Assert(i + 1 < args.Length);
								client.MasterServer = args[++i];
								break;
							case "--world":
                            case "--world_id":
								Debug.Assert(i + 1 < args.Length);
								client.WorldId = args[++i];
								break;
							case "--character":
								Debug.Assert(i + 1 < args.Length);
								client.CharacterId = long.Parse(args[++i]);
								break;
                            case "--login_url":
                                Debug.Assert(i + 1 < args.Length);
                                client.LoginUrl = args[++i];
                                break;
                            case "--login_page":
                                Debug.Assert(i + 1 < args.Length);
                                client.LoginUrl = LoginBase + args[++i];
                                break;
                            case "--noupdate":
                            case "--no_media_update":
                                client.PatchMedia = false;
                                break;
                            case "--world_patcher_url":
                                Debug.Assert(i + 1 < args.Length);
                                log.Warn("--world_patcher_url argument will not be honored by the login patcher");
                                client.WorldPatcherUrl = args[++i];
                                break;
                            case "--world_update_url":
                                Debug.Assert(i + 1 < args.Length);
                                log.Warn("--world_update_url argument will not be honored by the login patcher");
                                client.WorldUpdateUrl = args[++i];
                                break;
						    case "--world_settings_file":
                                Debug.Assert(i + 1 < args.Length);
								WorldSettingsFile = args[++i];
                                break;
							case "--logcollisions":
                                client.logCollisions = true;
                                break;
						    case "--frames_between_sleeps":
                                Debug.Assert(i + 1 < args.Length);
                                client.FramesBetweenSleeps = int.Parse(args[++i]);
                                break;
						    case "--log_level":
                                Debug.Assert(i + 1 < args.Length);
                                SetLogLevel(args[++i], false);
                                break;
						    case "--development":
                                client.UseRepository = true;
								client.FramesBetweenSleeps = 2;
                                client.PatchMedia = false;
								break;
						    case "--use_default_repository":
								client.UseRepository = true;
                                client.PatchMedia = false;
								break;
						    case "--repository_path":
								client.RepositoryPaths.Add(args[++i]);
                                client.PatchMedia = false;
								client.UseRepository = true;
                                break;
                            case "--maxfps":
                                client.MaxFPS = int.Parse(args[++i]);
                                break;
                            case "--log_terrain":
                                client.LogTerrainConfig = true;
                                break;
                            case "--coop_input":
                                // Used when running from the debugger
                                client.UseCooperativeInput = true;
                                break;
                            case "--tcp":
                                client.UseTCP = true;
                                break;
                            case "--rdp":
                                client.UseTCP = false;
                                break;
                            case "--display_config":
                                client.manualDisplayConfigString = args[++i];
                                break;
                            case "--fullscreen":
                                client.manualFullscreen = true;
                                break;
                            case "--fixed_fps":
                                client.FixedFPS = int.Parse(args[++i]);
                                break;
                            case "--client_patch_only":
                                return;
                            case "--media_patch_only":
                                client.ExitAfterMediaPatch = true;
                                break;
                            case "--allow_resize":
                                Debug.Assert(i + 1 < args.Length);
                                client.AllowResize = bool.Parse(args[++i]);
                                break;
                            case "--log_config":
                                // this is handled up above, but ignore it here
                                Debug.Assert(i + 1 < args.Length);
                                ++i;
                                break;
                            // These are the arguments used by the patcher
                            case "--no_client_update":
                            case "--force_scan":
                                break;
                            case "--update_url":
                            case "--patch_url":
                            case "--patcher_url":
                                ++i;
                                break;
                            default:
                                // Handle -Dproperty=value arguments
                                if (args[i].StartsWith("-D") && args[i].Contains("="))
                                {
                                    char[] delims = {'='};
                                    string[] tmp = args[i].Substring(2).Split(delims, 2);
                                    client.SetParameter(tmp[0], tmp[1]);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid argument " + args[i]);
                                }
								break;
						}
					}
                    client.GameWorld = new Multiverse.Base.MarsWorld(client);

                    client.SourceConfig(Path.Combine(ConfigFolder, WorldSettingsFile));
                    try {
                        client.Start();
                    } catch (Exception ex) {
                        LogUtil.ExceptionLog.ErrorFormat("Exiting client due to exception: {0}", ex);
                        errorMessage = ex.Message;
                    }
                    log.Info("Exiting client");
                    if (errorPage == null)
                        errorPage = client.ErrorPage;
                    if (errorMessage == null)
                        errorMessage = client.ErrorMessage;
                } // using client
                bool errorShown = false;
                if (errorPage != null) {
                    string fullPath = Path.GetFullPath("../Html/" + errorPage);
                    if (File.Exists(fullPath)) {
                        Multiverse.Base.HtmlWindow tmp = new Multiverse.Base.HtmlWindow("file://" + fullPath);
                        tmp.Text = "Multiverse Client Error";
                        tmp.ShowDialog();
                        errorShown = true;
                    }
                }
                if (!errorShown && errorMessage != null) {
                    Dialog tmp = new Dialog();
                    tmp.TopMost = true;
                    tmp.Text = "Client Shutting Down!";
                    tmp.Message = errorMessage;
                    tmp.ShowDialog();
                }
			} catch (Exception ex) {
				// call the existing global exception handler
                LogUtil.ExceptionLog.ErrorFormat("Exited client due to exception: {0}", ex);
                Dialog tmp = new Dialog();
                tmp.TopMost = true;
                tmp.Text = "Alert";
                tmp.Message = ex.Message;
                tmp.ShowDialog();
            } finally {
                log.Info("Cleaning up");
                log.Info("Exiting Client");
                LogManager.Shutdown();
                GC.Collect();
                // Kernel.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1); 
				// this.WindowState = FormWindowState.Normal;
				// this.Show();
                Application.Exit();
			}
		}
	}
}
