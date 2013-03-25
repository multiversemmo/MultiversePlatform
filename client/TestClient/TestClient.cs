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

#define USE_PERFORMANCE_COUNTERS

#region Using directives

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Threading;

using Axiom.MathLib;
using Axiom.Utility;

using Multiverse.Config;
using Multiverse.Network;

using Vector3 = Axiom.MathLib.Vector3;

#endregion

namespace Multiverse.Test
{

	public class FrameEventArgs : System.EventArgs {
		/// <summary>
		///    Time elapsed (in milliseconds) since the last frame event.
		/// </summary>
		public float TimeSinceLastEvent;

		/// <summary>
		///    Time elapsed (in milliseconds) since the last frame.
		/// </summary>
		public float TimeSinceLastFrame;
	}

    public class ConfigParser {
        Client client;
        string filename;
        public ConfigParser(Client client, string filename) {
            this.client = client;
            this.filename = filename;
        }
                    
        public bool Load() {
            if (!File.Exists(filename))
                return false;
            Stream stream = File.Open(filename, FileMode.Open);
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            foreach (XmlNode child in document.ChildNodes) {
                switch (child.Name) {
                    case "client_config":
                        ReadClientConfig(child);
                        break;
                }
            }
            stream.Close();
            return true;
        }

        public void ReadClientConfig(XmlNode node) {
            foreach (XmlNode child in node.ChildNodes) {
                switch (child.Name) {
                    case "loopback_world_response":
                        ReadLoopbackWorldResponse(child);
                        break;
                    case "login_url":
                        if (child.Attributes["href"] != null)
                            client.LoginUrl = child.Attributes["href"].Value;
                        else
                            Trace.TraceWarning("login_url element missing href attribute");
                        break;
                }
            }
        }

        public void ReadLoopbackWorldResponse(XmlNode node) {
            // WorldResponse response;
            WorldServerEntry entry;
            string world_id = null;
            string update_url = null;
            string patcher_url = null;
            string hostname = null;
            int port = 0;
            if (node.Attributes["world_id"] == null)
                Trace.TraceWarning("loopback_world_response element missing multiverse_id attribute");
            else
                world_id = node.Attributes["world_id"].Value;
            foreach (XmlNode child in node.ChildNodes) {
                switch (child.Name) {
                    case "account":
						// Set our user id to be derived from our pid, so they
						// don't collide when we run multiple test clients
						Process process = Process.GetCurrentProcess();
						client.LoopbackIdToken = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(process.Id));
						Logger.Log(2, "Setting client.LoopbackIdToken to {0}", process.Id);

//                         if (child.Attributes["id_token"] != null) {
//                             string token = child.Attributes["id_token"].Value;
//                             client.LoopbackIdToken = Convert.FromBase64String(token);
//                         } else if (child.Attributes["id_number"] != null) {
//                             string number = child.Attributes["id_number"].Value;
//                             int id = int.Parse(number);
//                             client.LoopbackIdToken = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(id));
//                         } else {
//                             Trace.TraceWarning("account element missing multiverse_id attribute");
//                         }
                        break;
                    case "update_url":
                        if (child.Attributes["href"] != null)
                            update_url = child.Attributes["href"].Value;
                        else
                            Trace.TraceWarning("update_url element missing href attribute");
                        break;
                    case "patcher_url":
                        if (child.Attributes["href"] != null)
                            patcher_url = child.Attributes["href"].Value;
                        else
                            Trace.TraceWarning("patcher_url element missing href attribute");
                        break;
                    case "server":
                        if (child.Attributes["hostname"] != null)
                            hostname = child.Attributes["hostname"].Value;
                        else
                            Trace.TraceWarning("server element missing hostname attribute");
                        if (child.Attributes["port"] != null) {
                            if (!int.TryParse(child.Attributes["port"].Value, out port))
                                Trace.TraceWarning("server element has invalid port attribute");
                        } else
                            Trace.TraceWarning("server element missing port attribute");
                        break;
                }
            }
            entry = new WorldServerEntry(world_id, hostname, port, patcher_url, update_url);
            client.LoopbackWorldServerEntry = entry;
        }
    }

    public class TimestampedTextWriterTraceListener : TextWriterTraceListener {
        protected bool lastWriteLine = true;
		public TimestampedTextWriterTraceListener(Stream stream)
            : base(stream) {
        }
		protected void OutTimeStamp() {
            DateTime time = DateTime.Now.ToUniversalTime();
            string newMsg = "";
            try {
                newMsg = string.Format("[{0}] ", time.ToString("MM/dd/yy HH:mm:ss.fff"));
            } catch (Exception e) {
                Debug.Assert(false, e.ToString());
            }
			base.Write(newMsg);
		}
		public override void WriteLine(string message) {
			if (lastWriteLine) {
				OutTimeStamp();
				lastWriteLine = false;
			}
			base.WriteLine(message);
			lastWriteLine = true;
		}
		public override void Write(string message) {
			if (lastWriteLine) {
				OutTimeStamp();
				lastWriteLine = false;
			}
            base.Write(message);
        }
	}

	public class ClientException : Exception {
		public ClientException(string msg)
			: base(msg) {
		}
	}

	public delegate void CommandHandler(Client client, TestObject target, string args);
    public delegate void TickEvent(object source, int time);

    /// <summary>
	/// This is the main class of my Direct3D application
	/// </summary>
	public class Client
	{
        #region Fields

#if USE_PERFORMANCE_COUNTERS
		public static System.Diagnostics.PerformanceCounter framesCounter;
		public static System.Diagnostics.PerformanceCounter packetsSentCounter;
		public static System.Diagnostics.PerformanceCounter packetsReceivedCounter;
		public static System.Diagnostics.PerformanceCounter bytesSentCounter;
		public static System.Diagnostics.PerformanceCounter bytesReceivedCounter;
		public static System.Diagnostics.PerformanceCounter worldCounterTimer;
		public static System.Diagnostics.PerformanceCounter networkCounterTimer;
		public static System.Diagnostics.PerformanceCounter inputCounterTimer;
		public static System.Diagnostics.PerformanceCounter interFrameCounterTimer;
		public static System.Diagnostics.PerformanceCounter renderQueueCounterTimer;
        long frameCount = 0;
        int lastPerformanceTick = 0;
#endif
        
        Stopwatch worldTimer = new Stopwatch();
		Stopwatch networkTimer = new Stopwatch();
		Stopwatch inputTimer = new Stopwatch();
		Stopwatch interFrameTimer = new Stopwatch();
		Stopwatch renderQueueTimer = new Stopwatch();

        WorldManager worldManager;

		protected Dictionary<string, CommandHandler> commandHandlers = 
			new Dictionary<string, CommandHandler>();

        // To set of a tick event every 1/5 of a second
        int timerTick = 0;
        float TickInterval = 1.0f / 200.0f;

        /// <summary>
        ///   If this is set, we are debugging, and don't use a server
        /// </summary>
        public bool standalone = false;

		/// <summary>
		///   If this is set, we are debugging, so do verbose output
		/// </summary>
		public bool doTraceConsole;

		/// <summary>
		///   If this is set, show the rendering configuration
		/// </summary>
		public bool doDisplayConfig;

        public bool patchMedia = false;

		/// <summary>
		///   If this is set, use the simple terrain instead of the 
		///   paging landscape scene manager.
		/// </summary>
		public bool simpleTerrain = false;

		/// <summary>
		///   If this is set, extensive logging of to collision algorithm will be
		///   appended to the file ../CollisionLog.txt
		/// </summary>
		public bool logCollisions = false;

		/// <summary>
		/// If this is non-zero, then every framesBetweenSleeps frames
		/// we sleep for 20 ms
		/// </summary>
		protected int framesBetweenSleeps = 0;

		/// <summary>
		/// If this is a counter of frames, only used if
		/// framesBetweenSleeps is non-zero
		/// </summary>
		protected int frameSleepCount = 0;
		
		protected float time = 0.0f;

		bool exited = false;
		
		bool consoleMode;

        bool loginFailed;
        string loginMessage;
        string worldPatcherUrl;
        string worldUpdateUrl;
        bool fullScan = false;

        /// <summary>
        ///   WorldServerEntry that we will use if we are not connecting to a master server
        /// </summary>
        WorldServerEntry loopbackWorldServerEntry;
        /// <summary>
        ///   Id token to submit
        /// </summary>
        byte[] loopbackIdToken;

		// public List<string> uiModules = new List<string>();

		NetworkHelper networkHelper;
		// The name manager object

		System.Timers.Timer traceFlushTimer;
		TestObject target = null;

		LoginSettings loginSettings = new LoginSettings();

		BehaviorParms behaviorParms = null;
		
		#region Constants

		public const int OneMeter = 1000;

		public const int HorizonDistance = 1000 * OneMeter;

        const int MaxMessagesPerFrame = 10;

        const string DefaultLoginUrl        = "http://update.multiverse.net/login/login.html";
        const string DefaultMasterServer    = "master.kothuria.com";
        const string DefaultWorldId         = "3";

//        const string MediaPatchUrl          = "http://update.multiverse.net/mvupdate.media/";
        // For now, simply disable media patches if we don't provide a patch url
//        const string MediaPatchHtml         = "http://update.multiverse.net/mvsecret/media_patcher.html";

        /// <summary>
        /// Fired a number of times each second
        /// </summary>
        public event TickEvent Tick;

		#endregion

		IDictionary keyboardKeysDown = new Hashtable();
        private bool requestShutdown = false;

		private bool verifyServer = false;

        #endregion Fields

		public Client(bool verifyServer, BehaviorParms behaviorParms) {
            this.verifyServer = verifyServer;
			this.MasterServer = DefaultMasterServer;
            this.LoginUrl = DefaultLoginUrl;
            this.WorldId = string.Empty;
			this.CharacterId = 0;
			this.behaviorParms = behaviorParms;
			SetupDebug();
		}

        #region Methods

		public void Dispose() {
            Logger.Log(3, "Started call to Client.Dispose()");
            try {
                if (networkHelper != null)
                    networkHelper.Dispose();
                if (worldManager != null) {
                    worldManager.ClearWorld();
                    worldManager.Dispose();
                }
            } catch (Exception e) {
                Logger.Log(4, "Client.Dispose caught exception: " + e);
                throw e;
            }
            Logger.Log(3, "Finished call to Client.Dispose()");
        }

        public void SourceConfig(string configFile) {
            ConfigParser configParser = new ConfigParser(this, configFile);
            configParser.Load();
        }

        public void Write(string message) {
			Trace.TraceInformation(message);
        }

		public void PrintModules() {
			StringBuilder sb = new StringBuilder();
			foreach (ProcessModule pm in Process.GetCurrentProcess().Modules)
					sb.AppendLine(pm.FileName);
			Write(sb.ToString());
			Trace.TraceInformation(sb.ToString());
		}

		protected void Flush(object sender, System.Timers.ElapsedEventArgs e) {
			Trace.Flush();
		}

		protected void SetupDebug() {
            // Create a file for output named TracePID.txt.
			Process process = Process.GetCurrentProcess();
			string f = "../trace" + process.Id + ".txt";
			File.Delete(f);
			Stream myFile = File.Open(f, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

			/* Create a new text writer using the output stream, and add it to
             * the trace listeners. */
            Trace.Listeners.Add(new TimestampedTextWriterTraceListener(myFile));

			traceFlushTimer = new System.Timers.Timer();
			traceFlushTimer.Enabled = true;
			traceFlushTimer.Interval = 100; // .1 seconds
			traceFlushTimer.Elapsed +=
			   new System.Timers.ElapsedEventHandler(this.Flush);
        }

		private bool WaitForPlayerStubData() {
			int waitCount = 0;
			while (!worldManager.PlayerStubInitialized) {
                // handle one message at a time, until we get the player
				MessageDispatcher.Instance.HandleMessageQueue(1);
                if (!worldManager.PlayerStubInitialized) {
					Thread.Sleep(100); // sleep for 100ms.
					waitCount++;
				}
                // Wait for 10 seconds (or 100 messages)
				if (waitCount > 100)
					return false;
			}
			return true;
		}

        // Only wait 5 seconds for the login response
        private bool WaitForLoginResponse() {
            int waitCount = 0;
            while (loginMessage == null) {
                // handle one message at a time, until we get the login response
                MessageDispatcher.Instance.HandleMessageQueue(1);
                if (loginMessage == null) {
                    Thread.Sleep(100); // sleep for 100ms.
                    waitCount++;
                }
                // Wait for 5 seconds (or 50 messages)
                if (waitCount > 50)
                    return false;
            }
            return true;
        }

		private bool WaitForTerrainData() {
			int waitCount = 0;
			while (!worldManager.TerrainInitialized) {
                // handle one message at a time, until we get the terrain
				MessageDispatcher.Instance.HandleMessageQueue(1);
				if (!worldManager.TerrainInitialized) {
					Thread.Sleep(100); // sleep for 100ms.
					waitCount++;
				}
                // Wait for 50 seconds (or 500 messages)
				if (waitCount > 500)
					return false;
			}
			return true;
		}

		protected CounterCreationDataCollection PrepareCounterCollection() {
			CounterCreationDataCollection ccdc = new CounterCreationDataCollection();
			CounterCreationData counter;

			// Add the counter for frames rendered
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "frames rendered";
			ccdc.Add(counter);

			// Add the counter for packets sent
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "packets sent";
			ccdc.Add(counter);

			// Add the counter for packets received
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "packets received";
			ccdc.Add(counter);

			// Add the counter for bytes sent
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "bytes sent";
			ccdc.Add(counter);

			// Add the counter for bytes received
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "bytes received";
			ccdc.Add(counter);

			// Add the counter for world manager counter timer
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "world manager";
			ccdc.Add(counter);

			// Add the counter for bytes received
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "network manager";
			ccdc.Add(counter);

			// Add the counter for bytes received
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "input handler";
			ccdc.Add(counter);

			// Add the counter for bytes received
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "inter frame";
			ccdc.Add(counter);

			// Add the counter for main render queue
			counter = new CounterCreationData();
			counter.CounterType = PerformanceCounterType.RateOfCountsPerSecond64;
			counter.CounterName = "render main queue";
			ccdc.Add(counter);

			return ccdc;
		}
#if USE_PERFORMANCE_COUNTERS
		protected void SetupPerformanceCategories() {
			PerformanceCounterCategory category = null;

			PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();
			for (int i = 0; i < categories.Length; ++i) {
				if (categories[i].CategoryName == "Multiverse Performance") {
					category = categories[i];
					break;
				}
			}
			CounterCreationDataCollection ccdc = PrepareCounterCollection();
			// Make sure we have all the appropriate counters
			if (category != null) {
				foreach (CounterCreationData ccd in ccdc) {
					if (!category.CounterExists(ccd.CounterName)) {
						PerformanceCounterCategory.Delete(category.CategoryName);
						category = null;
						break;
					}
				}
			}
			if (category == null) {
				// Create the category.
				PerformanceCounterCategory.Create("Multiverse Performance",
					"Monitors the performance of the Multiverse Client.",
					PerformanceCounterCategoryType.SingleInstance,
					ccdc);
			}
		}

		protected PerformanceCounter GetCounter(string counterName) {
			return new PerformanceCounter("Multiverse Performance", counterName, false);
		}
		protected void CreateCounters() {
			framesCounter = GetCounter("frames rendered");
			packetsSentCounter = GetCounter("packets sent");
			packetsReceivedCounter = GetCounter("packets received");
			bytesSentCounter = GetCounter("bytes sent");
			bytesReceivedCounter = GetCounter("bytes received");
			worldCounterTimer = GetCounter("world manager");
			networkCounterTimer = GetCounter("network manager");
			inputCounterTimer = GetCounter("input handler");
			interFrameCounterTimer = GetCounter("inter frame");
			renderQueueCounterTimer = GetCounter("render main queue");
		}
#endif

		public void Start() {
			if (Setup()) {
				while (!exited)
					Thread.Sleep(20);
			}
		}

        protected CharacterEntry SelectCharacter(List<CharacterEntry> entries, int characterIndex) {
            // int characterIndex = NetworkHelper.CharacterIndex;
            if (entries.Count <= 0) {
                Trace.TraceError("No characters available");
                return null;
            }
            if (characterIndex < 0 || entries.Count <= characterIndex) {
                // CharacerIndex of -1 just means to pick the last character in the list
                if (characterIndex != -1)
                    Logger.Log(4, "Invalid character selection: {0}/{1}", characterIndex, entries.Count);
                characterIndex = entries.Count - 1;
                Logger.Log(4, "Using alternate character index: " + characterIndex);
            }
            return entries[characterIndex];
        }

		/// <summary>
        ///		Overridden to switch to event based keyboard input.
        /// </summary>
        /// <returns></returns>
        protected bool Setup() {

#if USE_PERFORMANCE_COUNTERS
			SetupPerformanceCategories();
			CreateCounters();
#endif
            this.Tick += new TickEvent(OnTick);

			worldManager = new WorldManager(verifyServer, behaviorParms);
			NetworkHelper helper = new NetworkHelper(worldManager);
			if (this.LoopbackWorldServerEntry != null) {
				string worldId = this.LoopbackWorldServerEntry.WorldName;
				// Bypass the login and connection to master server
				loginSettings.worldId = worldId;
				helper.SetWorldEntry(worldId, this.LoopbackWorldServerEntry);
				helper.AuthToken = this.LoopbackIdToken;
			}
			networkHelper = helper;

            // Sets up the various things attached to the world manager, 
			// as well as registering the various message handlers.
			// This also initializes the networkHelper.
			worldManager.Init(this);

            // Register our handlers.  We must do this before we call
            // MessageDispatcher.Instance.HandleMessageQueue, so that we will
            // get the callbacks for the incoming messages.

            
#if NOT
            // NOTE: Test client isn't advanced enough to handle these.

            // Register our handler for the Portal messages, so that we
            // can drop our connection to the world server, and establish a new 
            // connection to the new world.
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Portal,
                                                       new MessageHandler(this.HandlePortal));
            // Register our handler for the UiTheme messages, so that we
            // can swap out the user interface.
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.UiTheme,
                                                       new MessageHandler(this.HandleUiTheme));
#endif
            // Register our handler for the LoginResponse messages, so that we
            // can throw up a dialog if needed.
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.LoginResponse,
                                                       new WorldMessageHandler(this.HandleLoginResponse));

            if (!networkHelper.HasWorldEntry(loginSettings.worldId))
                networkHelper.ResolveWorld(loginSettings);
            WorldServerEntry entry = networkHelper.GetWorldEntry(loginSettings.worldId);
            NetworkHelperStatus status = networkHelper.ConnectToLogin(loginSettings.worldId);

            // We need to hook our message filter, whether or not we are 
            // standalone, so instead of doing it later (right before 
            // RdpWorldConnect), do it here.
            RequireLoginFilter checkAndHandleLogin = new RequireLoginFilter(worldManager);
            MessageDispatcher.Instance.SetWorldMessageFilter(checkAndHandleLogin.ShouldQueue);

            if (status != NetworkHelperStatus.Success &&
                status != NetworkHelperStatus.Standalone) {
                Trace.TraceInformation("World Connect Status: " + status);
                return false;
            }

            networkHelper.DisconnectFromLogin();
            CharacterEntry charEntry = SelectCharacter(networkHelper.CharacterEntries, -1);
            status = networkHelper.ConnectToWorld(charEntry.CharacterId,
                                                  charEntry.Hostname,
                                                  charEntry.Port, this.Version);
            if (status != NetworkHelperStatus.Success) {
                Trace.TraceInformation("World Connect Status: " + status);
                return false;
            }

			// At this point, the network helper can start handling messages.
            if (!WaitForStartupMessages()) {
                if (loginFailed && loginMessage != null)
                    // The server rejected our login
                    throw new ClientException(loginMessage);
                else if (loginMessage != null)
                    // our login went ok (and we got something back), but something else (terrain/player) failed
                    throw new ClientException("Unable to communicate with server");
                else
                    throw new ClientException("Unable to connect to server");
            }

			// At this point, I can have a camera

            // networkHelper.WorldManager = worldManager;

			// inputHandler.InitViewpoint(worldManager.Player);

			Logger.Log(4, "Client setup complete: " + DateTime.Now);
			// At this point, you can create timer events.
			return true;
        }

		public void HandleCommand(string message) {
			Logger.Log(3, "HandleCommand: " + message);

			if (!message.StartsWith("/"))
				message = "/say " + message;

            // Handle some client side commands
            string[] tokens = message.Split('\t', '\n', ' ');
            if (tokens.Length <= 0)
                return;

			string args = string.Empty;
            if (message.Length > tokens[0].Length)
				args = message.Substring(tokens[0].Length + 1);

			TestObject tmpObj = this.Target;
			if (tmpObj == null)
				tmpObj = this.Player;

			string command = tokens[0].Substring(1);
            if (commandHandlers.ContainsKey(command)) {
                try {
                    commandHandlers[command](this, tmpObj, args);
                } catch (Exception e) {
                    Trace.TraceWarning("Failed to run command handler {0} for command line: '{1}'", command, message);
                    Trace.TraceWarning("Exception: " + e.Message);
                }
            } else
                networkHelper.SendTargettedCommand(tmpObj.Oid, message);
		}

        public void RegisterCommandHandler(string command, CommandHandler handler)
        {
            commandHandlers[command] = handler;
        }

		#region Message Handlers

#if NOT
        private void HandlePortal(BaseWorldMessage message) {
			PortalMessage portalMessage = (PortalMessage)message;
			Write("Transporting via portal to alternate world.");

            loginSettings.worldId = portalMessage.WorldId;
            portalMessage.AbortHandling = true;

            needConnect = true;
        }
		private void HandleUiTheme(BaseWorldMessage message) {
			UiThemeMessage uiTheme = (UiThemeMessage)message;
			uiModules = new List<string>(uiTheme.UiModules);
			if (uiModules.Count == 0)
				uiModules.Add("basic.toc");
		}
#endif

        private void HandleLoginResponse(BaseWorldMessage message) {
            LoginResponseMessage loginResponse = (LoginResponseMessage)message;
            loginFailed = !loginResponse.Success;
            loginMessage = loginResponse.Message;
        }

        private bool WaitForStartupMessages() {
            if (!WaitForLoginResponse()) {
                Trace.TraceError("Unable to retrieve login response");
                return false;
            }
            if (!WaitForTerrainData()) {
                Trace.TraceError("Unable to retrieve terrain data");
                return false;
            }
            if (!WaitForPlayerStubData()) {
                Trace.TraceError("Unable to retrieve player data");
                return false;
            }
            return true;
        }

        //private void HandlePortal(BaseWorldMessage message) {
        //    PortalMessage portalMessage = (PortalMessage)message;
        //    Write("Transported via portal to alternate world.");

        //    Monitor.Enter(worldManager);
        //    try {
        //        worldManager.ClearWorld();
        //        networkHelper.ConnectToWorld(portalMessage.WorldId, -1, "0.9.1");
        //    } finally {
        //        Monitor.Exit(worldManager);
        //    }

        //    if (!WaitForStartupMessages()) {
        //        return;
        //    }
        //    this.Target = null;
        //    Logger.Log(2, "Got player data");
        //}

		#endregion

        protected virtual void OnTick(object source, int time) {
        }

		protected static TimingMeter frameMeter = MeterManager.GetMeter("Client Frame", "Client");
		protected static TimingMeter messageMeter = MeterManager.GetMeter("Client Message Handling", "Client");
		protected static TimingMeter worldManagerMeter = MeterManager.GetMeter("Client WorldMgr", "Client");
		protected static TimingMeter inputMeter = MeterManager.GetMeter("Client Input Handling", "Client");

		// Login settings
		// Default settings for login settings
		public string MasterServer {
			set {
				loginSettings.rdpServer = value;
				loginSettings.tcpServer = value;
			}
		}
		public string WorldId {
			set {
				loginSettings.worldId = value;
			}
		}
		public long CharacterId {
			set {
				loginSettings.characterId = value;
			}
		}
        public string LoginUrl {
            set {
                loginSettings.loginUrl = value;
            }
        }
        /// <summary>
        ///   Override for the world specific patcher url
        /// </summary>
        public string WorldPatcherUrl {
            set {
                worldPatcherUrl = value;
            }
            get {
                return worldPatcherUrl;
            }
        }
        /// <summary>
        ///   Override for the world specific asset url
        /// </summary>
        public string WorldUpdateUrl {
            set {
                worldUpdateUrl = value;
            }
            get {
                return worldUpdateUrl;
            }
        }
		public NetworkHelper NetworkHelper {
			get { return networkHelper; }
		}

		public WorldManager WorldManager {
			get { return worldManager; }
		}

        public TestObject Player {
            get {
                return worldManager.Player;
            }
        }
        public long PlayerId {
            get {
                return worldManager.PlayerId;
            }
        }

		public TestObject Target {
			get {
				return target;
			}
			set {
				target = value;
			}
		}

        public WorldServerEntry LoopbackWorldServerEntry {
            get {
                return loopbackWorldServerEntry;
            }
            set {
                loopbackWorldServerEntry = value;
            }
        }

        public byte[] LoopbackIdToken {
            get {
                return loopbackIdToken;
            }
            set {
                loopbackIdToken = value;
            }
        }

        public string Version {
            get {
                return "0.9.1";
            }
        }
	}
}
#endregion Methods
