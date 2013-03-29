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

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Input;
using Axiom.MathLib;
using Axiom.Utility;
using Multiverse.AssetRepository;
using Multiverse.CollisionLib;
using Multiverse.Gui;
using Multiverse.Input;
using Multiverse.Interface;
using Multiverse.Lib.LogUtil;
using Multiverse.Lib.TextureFetcher;
using Multiverse.Network;
using Multiverse.Patcher;
using Multiverse.Serialization;
using Multiverse.Utility;
using SystemInformation = System.Windows.Forms.SystemInformation;
using TimeTool = Multiverse.Utility.TimeTool;

#endregion Using directives

namespace Multiverse.Base
{
    public class ClientException : Exception
    {
        public ClientException(string msg)
            : base(msg)
        {
        }

        public ClientException(string msg, Exception inner)
            : base(msg, inner)
        {
        }
    }

    public class PrettyClientException : ClientException
    {
        string htmlPage;

        public PrettyClientException(string page, string msg)
            : base(msg)
        {
            htmlPage = page;
        }

        public PrettyClientException(string page, string msg, Exception inner)
            : base(msg, inner)
        {
            htmlPage = page;
        }

        public string HtmlPage
        {
            get
            {
                return htmlPage;
            }
        }
    }

    public delegate void CommandHandler(string args);
    public delegate void GameWorldCommandHandler(Client client, ObjectNode target, string args);
    public delegate void TickEvent(object source, int time);

    /// <summary>
    /// This is the main class of my Direct3D application
    /// </summary>
    public class Client : IDisposable, Multiverse.Interface.ILogger
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Client));
        private static string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string ClientAppDataFolder = Path.Combine(MyDocumentsFolder, "Multiverse World Browser");
        private static string ConfigFolder = Path.Combine(ClientAppDataFolder, "Config");
        private static string LogFolder = Path.Combine(ClientAppDataFolder, "Logs");
        private static string ScreenshotsFolder = Path.Combine(ClientAppDataFolder, "Screenshots");
        private static string PatchLogFile = Path.Combine(LogFolder, "media_patcher.log");
        private static string MeterEventsFile = Path.Combine(LogFolder, "MeterEvents.log");
        private static string MeterLogFile = Path.Combine(LogFolder, "MeterLog.log");
        private static string LocalConfigFile = Path.Combine(ConfigFolder, "LocalConfig.xml");
        private static string MultiverseImagesetFile = Path.Combine("..", "MultiverseImageset.xml");
        private static string MultiverseLoadScreen = Path.Combine("..", "mvloadscreen.bmp");

        public static Client instance;

        #region Fields

        WorldManager worldManager;

        protected Dictionary<string, CommandHandler> commandHandlers =
            new Dictionary<string, CommandHandler>();

        protected Dictionary<string, GameWorldCommandHandler> gameWorldCommandHandlers =
            new Dictionary<string, GameWorldCommandHandler>();

        protected Dictionary<string, string> parameters =
            new Dictionary<string, string>();

        protected Window loadWindow;  // for the loading window that covers the screen
        protected Window guiWindow;   // for multiverse chrome - going away
        protected Window xmlUiWindow; // for the xml based interface

        protected float[] maxElapsed = new float[100];

        /// <summary>
        /// Count how many frames have been rendered
        /// </summary>
        protected int frameCounter = 1;

        /// <summary>
        ///   If this is set, we are debugging, and don't use a server
        /// </summary>
        public bool standalone;

        /// <summary>
        ///   If this is set, we are debugging, so do verbose output
        /// </summary>
        public bool doTraceConsole;

        /// <summary>
        ///   If this is set, show the rendering configuration
        /// </summary>
        public bool doDisplayConfig;

        /// <summary>
        /// If this string is set, then use it to configure the display window.
        /// Using this option will bypass both the config dialog and loading
        ///   of the DisplayConfig.xml file.
        /// Format is "widthxheightxdepth".  For example "1024x768x32".
        /// </summary>
        public string manualDisplayConfigString = null;

        /// <summary>
        /// If true, user has requested fullscreen display config on the command
        /// line.  Only valid if manualDisplayConfigString is also set.
        /// </summary>
        public bool manualFullscreen = false;

        /// <summary>
        ///    Set by ClientInit.py to the minimum screen width
        ///    allowed by the world.
        /// </summary>
        protected int minScreenWidth = 800;

        /// <summary>
        ///    Set by ClientInit.py to the minimum screen width
        ///    allowed by the world.
        /// </summary>
        protected int minScreenHeight = 600;

        /// <summary>
        ///    Set by ClientInit.py to false if the world does not
        ///    allow full-screen mode.
        /// </summary>
        protected bool allowFullScreen = true;

        /// <summary>
        ///    Can be changed using the --allow_resize command-line arg
        /// </summary>
        protected bool allowResize = true;

        /// <summary>
        ///    Set to true if the last resize left the form maximized.
        ///    This is the only way we can tell that the restore
        ///    button was clicked.
        /// </summary>
        protected bool lastMaximized = false;

        public bool patchMedia = true;

        public bool exitAfterMediaPatch = false;

        public bool useLocalWorld = false;

        /// <summary>
        ///   Allow the client to override the asset config file from the command line
        /// </summary>
        // public string assetConfig;

        /// <summary>
        ///	  If this is set, and we're talking to a real server, we get the assets
        ///   from the directory cached in the registry.
        /// </summary>
        public bool useRepository = false;

        /// <summary>
        ///	  If this is set, get the repository paths from here instead of from
        ///   the directory or directories cached in the registry.
        /// </summary>
        public List<string> repositoryPaths = new List<string>();

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

        protected bool useCooperativeInput = true;

        protected float time = 0.0f;

        /// <summary>
        ///   Don't let the client run any faster than this
        /// </summary>
        protected int maxFPS = 0;

        /// <summary>
        ///   Setting this value to non-zero will cause the client to ignore the actual time
        ///     between frames and instead compute time between frames based on the specified
        ///     FPS.  This allows a non-realtime rendering mode that can be used for creating
        ///     videos.
        /// </summary>
        private int fixedFPS = 0;

        bool loginFailed;
        string loginMessage;
        string worldServerVersion;
        string worldPatcherUrl;
        string worldUpdateUrl;
        bool fullScan = false;
        static bool serverRelease1_5 = false;

        // Used to set an error message if we are going to exit
        string errorMessage = null;
        // Used to set an error page if we are going to exit
        string errorPage = null;

        /// <summary>
        ///   WorldServerEntry that we will use if we are not connecting to a master server
        /// </summary>
        WorldServerEntry loopbackWorldServerEntry;
        /// <summary>
        ///   Id token to submit
        /// </summary>
        byte[] loopbackIdToken;
        /// <summary>
        ///   Old style token to submit for old servers
        /// </summary>
        byte[] loopbackOldToken;

        /// <summary>
        ///   Is the client fully initialized?  Set to false
        ///   initially, and to true by the first LoadingStateMessage
        ///   with loadingState = true
        /// </summary>
        protected bool fullyInitialized = false;

        UiSystem xmlUi = new Multiverse.Interface.UiSystem();

        public List<string> uiModules = new List<string>();
        string keyBindingsFile = null;

        NetworkHelper networkHelper;
        // The name manager object

        Window rootWindow;

        LoginSettings loginSettings = new LoginSettings();
        IInputHandler inputHandler;

        static TimingMeter renderQueueTimer = MeterManager.GetMeter("Render Queue Timer", "Client");

        #region Constants

        public const int OneMeter = 1000;

        public const int HorizonDistance = 1000 * OneMeter;

        protected int lastFrameRenderCalls = 0;
        protected long lastTotalRenderCalls = 0;
        protected float maxRecentFrameTime = 0.0f;

        const long MaxMSForMessagesPerFrame = 100;

        // make this confgurable for the developer
        const string DefaultLoginUrl        = "http://login.multiversemmo.com/login.jsp";
        const string DefaultMasterServer    = "master.multiversemmo.com";
        const string DefaultWorldId         = "sampleworld";
        // This comma-separated list of capabilities supported by the client are passed 
        // to the server in the LoginMessage as part of the version string
        const string ClientCapabilities = "DirLocOrient";

        // For now, simply disable media patches if we don't provide a patch url
        // const string MediaPatchHtml         = "http://update.multiversemmo.com/update/media_patcher.html";

        const SceneType EngineSceneType = SceneType.ExteriorFar;

        #endregion Constants

        public IGameWorld gameWorld;

        // Moved these from TechDemo
        protected Root engine;
        protected Camera camera;
        protected Viewport viewport;
        protected SceneManager scene;
        protected RenderWindow window;
        protected TextureFiltering filtering = TextureFiltering.Bilinear;

        IDictionary keyboardKeysDown = new Hashtable();
        private bool requestShutdown = false;
        private bool needConnect = false;
        private string overrideVersion = null;

        // for cegui
        //        protected Size windowMinSize = new Size(.2f, .2f);
        //        protected ProgressBar progressBar;
        //        protected LayeredStaticText progressLabel;
        //        protected StaticText textArea;
        //        protected Checkbox wordWrapCheckbox;

        // If the player is stuck in a collision volume for this many
        // milliseconds, then PrimeMover sends the server the /goto
        // stuck command
        public int MillisecondsStuckBeforeGotoStuck = 5000;

        // When we login to the master server or proxy, if this is
        // true, we'll use TCP rather than RDP.
        protected bool useTCP = false;

        protected Renderer guiRenderer = null;

        /// <summary>
        ///    If true, CastRay will determine pick based on the
        ///    distance from the ray origin to the closest
        ///    CollisionShape in the object, rather than using the
        ///    bounding box.  The result of CastRay is what determines
        ///    the InputHandler's mouseoverObject.
        /// </summary>
        protected bool targetBasedOnCollisionVolumes = false;

        #endregion Fields

        public Client()
        {
            this.MasterServer = DefaultMasterServer;
            this.LoginUrl = DefaultLoginUrl;
            this.WorldId = string.Empty;
            this.CharacterId = 0;
            log.InfoFormat("Starting up client; client version number is {0}", this.Version);
            // SetupDebug();

            if (instance == null)
                instance = this;
            ConfigManager.Initialize(LocalConfigFile);
        }

        #region Methods

        public virtual void Dispose()
        {
            log.Info("Started call to Client.Dispose()");
            try
            {
                if (engine != null)
                {
                    // remove the event handlers for frame events
                    engine.FrameStarted -= new FrameEvent(OnFrameStarted);
                    engine.FrameEnded -= new FrameEvent(OnFrameEnded);
                }
                if (SoundManager.Instance != null)
                    SoundManager.Instance.Dispose();
                if (MultiverseMeshManager.Instance != null)
                    MultiverseMeshManager.Instance.Dispose();
                if (MultiverseSkeletonManager.Instance != null)
                    MultiverseMeshManager.Instance.Dispose();
                if (networkHelper != null)
                    networkHelper.Dispose();
                if (worldManager != null)
                {
                    worldManager.ClearWorld();
                    worldManager.Dispose();
                }
                if (engine != null)
                    engine.Dispose();
            }
            catch (Exception e)
            {
                LogUtil.ExceptionLog.Warn("Client.Dispose caught exception: {0}", e);
                throw e;
            }
            log.Info("Finished call to Client.Dispose()");
        }

        public bool UpdateWorldAssets(string worldRepository, string patchUrl, string updateUrl, bool fullScan)
        {
            Updater updater = new Updater();
            log.InfoFormat("Base Directory: {0}", worldRepository);

            UpdateForm dialog = new UpdateForm();
            dialog.Updater.FullScan = fullScan;
            dialog.Updater.BaseDirectory = worldRepository;
            dialog.Updater.UpdateUrl = updateUrl;
            dialog.Updater.SetupLog(PatchLogFile);
            dialog.Initialize(patchUrl, true);
            // dialog.StartUpdate();

            bool needUpdate = dialog.Updater.CheckVersion();
            if (!needUpdate)
            {
                dialog.Updater.CloseLog();
                return true;
            }

            System.Windows.Forms.DialogResult rv = System.Windows.Forms.DialogResult.Abort;
            rv = dialog.ShowDialog();
            dialog.Updater.CloseLog();
            if (rv == System.Windows.Forms.DialogResult.Abort || rv == System.Windows.Forms.DialogResult.Cancel)
            {
                if (dialog.Updater.Error != null)
                    // We encountered an error patching
                    log.Error(dialog.Updater.Error);
                return false;
            }
            if (dialog.Updater.Error != null)
            {
                // We encounterd an error patching
                log.Error(dialog.Updater.Error);
                return false;
            }

            // Ok, we finished patching - write the version and launch the client
            // WriteVersionFile(VersionFile);
            return true;
        }

        public void SourceConfig(string configFile)
        {
            ConfigParser configParser = new ConfigParser(configFile);
            configParser.Load();
            // See if I have an entry that matches what they passed as the world id.
            ConfigParser.LoopbackWorldResponse entry = configParser.GetWorldServerEntry(loginSettings.worldId);
            if (entry != null)
            {
                this.LoopbackWorldServerEntry = entry.worldServerEntry;
                this.LoopbackIdToken = entry.idToken;
                this.LoopbackOldToken = entry.oldToken;
            }
            else
            {
                // They didn't give us a world server entry, but they may have the login_url in there
                if (configParser.LoginUrl != null)
                    this.LoginUrl = configParser.LoginUrl;
            }
        }

        protected void ChooseSceneManager()
        {
            //if (simpleTerrain) {
            //    scene = engine.SceneManagers.GetSceneManager(SceneType.Generic);
            //    Axiom.SceneManagers.Octree.OctreeSceneManager osm =
            //        (Axiom.SceneManagers.Octree.OctreeSceneManager)scene;
            //    osm.SetOption("CullCamera", false);
            //    Vector3 min = new Vector3(-500 * OneMeter, -500 * OneMeter, -500 * OneMeter);
            //    Vector3 max = new Vector3(500 * OneMeter, 500 * OneMeter, 500 * OneMeter);
            //    AxisAlignedBox box = new AxisAlignedBox(min, max);
            //    osm.SetOption("Size", box);
            //}  else
            scene = engine.SceneManagers.GetSceneManager(SceneType.ExteriorClose);

            // No shadows until we have manifold objects
            // scene.ShadowTechnique = ShadowTechnique.StencilModulative;
            // The additive method looks nicer, but hasn't been implemented
            // scene.ShadowTechnique = ShadowTechnique.StencilAdditive;

            // The texture mode is the only one appropriate for use with vertex shaders
            // scene.ShadowTechnique = ShadowTechnique.TextureModulative;
            // Use Decal mode if you wanna keep things cheap
            scene.ShadowTechnique = ShadowTechnique.None;

            engine.SceneManager = scene;
        }

        protected void CreateCamera()
        {
            camera = scene.CreateCamera("camera.player");
            camera.Near = OneMeter * 0.23f;
            camera.Far = 2.0f * HorizonDistance; // > sqrt(3)
            camera.AspectRatio = (float)window.Width / window.Height;
        }

        protected void CreateViewports()
        {
            Debug.Assert(window != null, "Attempting to use a null RenderWindow.");

            // create a new viewport and set it's background color
            viewport = window.AddViewport(camera, 0, 0, 1.0f, 1.0f, 100);
            viewport.BackgroundColor = ColorEx.Black;
        }

        public void Write(string message)
        {
            if (gameWorld != null)
                gameWorld.Write(message);
            else
                log.WarnFormat("Write w/o gameworld: {0}", message);
        }

        public void PrintModules()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ProcessModule pm in Process.GetCurrentProcess().Modules)
                sb.AppendLine(pm.FileName);
            Write(sb.ToString());
            log.Info(sb.ToString());
        }

        protected void SetupGui()
        {
            guiRenderer = new Renderer(window, RenderQueueGroupID.Overlay, false);
            // set the scene manager
            guiRenderer.SetSceneManager(scene);

            // init the subsystem singleton
            new GuiSystem(guiRenderer);

            // configure the default mouse cursor
            GuiSystem.Instance.DefaultCursor = null;
            GuiSystem.Instance.CurrentCursor = null;

            // max window size, based on the size of the Axiom window
            SizeF maxSize = new SizeF(window.Width, window.Height);

            rootWindow = new Window("Window/RootWindow");
            // rootWindow.MetricsMode = MetricsMode.Absolute;
            rootWindow.MaximumSize = maxSize;
            rootWindow.Size = maxSize;
            rootWindow.Visible = true;

            // set the main window as the primary GUI sheet
            GuiSystem.Instance.SetRootWindow(rootWindow);

            // Load the default imageset
            try
            {
                AtlasManager.Instance.CreateAtlas(MultiverseImagesetFile);
            }
            catch (AxiomException e)
            {
                throw new PrettyClientException("bad_media.htm", "Invalid media repository", e);
            }

            xmlUiWindow = new Window("XmlUiWindow");
            // xmlUiWindow.MetricsMode = MetricsMode.Absolute;
            xmlUiWindow.MaximumSize = rootWindow.MaximumSize;
            xmlUiWindow.Size = rootWindow.Size;
            xmlUiWindow.Visible = false;

            rootWindow.AddChild(xmlUiWindow);

            // Set up the gui elements
            SetupGuiElements();

            FontManager.SetupFonts();

            rootWindow.Activate();
            xmlUiWindow.Activate();
        }

        public void SetUiVisibility(bool val)
        {
            xmlUiWindow.Visible = val;
        }

        public void ToggleUiVisibility()
        {
            bool val = xmlUiWindow.Visible;
            SetUiVisibility(!val);
        }

        public void ReloadUiElements()
        {
            if (xmlUiWindow != null)
            {
                xmlUiWindow.Visible = false;
                while (xmlUiWindow.ChildCount > 0)
                    xmlUiWindow.RemoveChild(0);
            }
            xmlUi.Cleanup();

            // Debugging
            foreach (string key in Multiverse.Interface.UiSystem.FrameMap.Keys)
                log.WarnFormat("Frame map still contains key: {0}", key);
            foreach (string key in Multiverse.Interface.UiSystem.VirtualFrameMap.Keys)
                log.InfoFormat("Virtual frame map still contains key: {0}", key);

            xmlUi.SetupInterpreter();
            UiSystem.RegisterRegionFactories();

            foreach (string tocFile in uiModules)
            {
                log.InfoFormat("Loading interface module from {0}", tocFile);
                Stream tocStream = AssetManager.Instance.FindResourceData("Interface", tocFile);
                StreamReader reader = new StreamReader(tocStream);
                for (; ; )
                {
                    string file = reader.ReadLine();
                    if (file == null)
                        break;
                    else if (file.StartsWith("#") || file.Trim() == string.Empty)
                        continue;
                    xmlUi.LoadInterfaceFile(file);
                }
                tocStream.Close();
            }

            // Load the dictionary of bindings from actioncode to script
            xmlUi.LoadBindingsFile("Bindings.xml");
            if (keyBindingsFile != null)
                // Load the mapping from key combination to actioncode
                xmlUi.LoadKeyBindings(keyBindingsFile);
            else
                // backwards compatability
                xmlUi.LoadKeyBindings("bindings.txt");

            log.InfoFormat("Begin Xml Ui Prepare: {0}", TimeTool.CurrentTime);
            xmlUi.Prepare(xmlUiWindow);
            log.InfoFormat("End Xml Ui Prepare: {0}", TimeTool.CurrentTime);

            log.InfoFormat("Begin CompileCode: {0}", TimeTool.CurrentTime);
            xmlUi.CompileCode();
            log.InfoFormat("End CompileCode: {0}", TimeTool.CurrentTime);

            GuiSystem.Instance.KeyDownHandler = new KeyboardEventHandler(xmlUi.HandleKeyDown);
            GuiSystem.Instance.KeyUpHandler = new KeyboardEventHandler(xmlUi.HandleKeyUp);
            GuiSystem.Instance.MouseDownHandler = new MouseEventHandler(xmlUi.HandleMouseDown);
            GuiSystem.Instance.MouseUpHandler = new MouseEventHandler(xmlUi.HandleMouseUp);
            if (xmlUiWindow != null)
                xmlUiWindow.Visible = true;

            xmlUi.Setup();
        }

        private void SetupGuiElements()
        {
            LoadingScreen loadScreen = new LoadingScreen(rootWindow.Size, rootWindow.MaximumSize);
            loadWindow = loadScreen.loadWindow;

            rootWindow.AddChild(loadScreen.loadWindow);

            // loadWindow.MoveToFront();
        }

        /// <summary>
        ///		Overridden to specify how our scene should be created.
        /// </summary>
        protected void CreateScene()
        {
            viewport.BackgroundColor = ColorEx.White;

            // scene.LoadWorldGeometry(TERRAIN_FILE);
            // scene.SetFog(FogMode.Linear, ColorEx.White, 1.0f, 100 * OneMeter, HorizonDistance * 2);

            // set some ambient light
            scene.TargetRenderSystem.LightingEnabled = true;
            // scene.AmbientLight = ColorEx.Beige;
        }

#if OLD_LOGGING
		protected void Flush(object sender, System.Timers.ElapsedEventArgs e) {
			Trace.Flush();
		}

		protected void SetupDebug() {
            // Create any directories we might need for config and
            // logging.  Also create a timestamped trace file for output
            // in the Logs directory named trace.txt (from Client.TraceFile).
            if (!Directory.Exists("../Logs"))
                Directory.CreateDirectory("../Logs");
            if (!Directory.Exists("../Config"))
                Directory.CreateDirectory("../Config");
            if (File.Exists(TraceFile))
                File.Delete(TraceFile);
			Stream logFile = File.Open(TraceFile, FileMode.Create,
									   FileAccess.ReadWrite, FileShare.Read);

			/* Create a new text writer using the output stream, and add it to
             * the trace listeners. */
            Trace.Listeners.Add(new TimestampedTextWriterTraceListener(logFile));

            // Set up a timer event that will flush the log every 5 seconds
			traceFlushTimer = new System.Timers.Timer();
			traceFlushTimer.Enabled = true;
			traceFlushTimer.Interval = 5000; // 5 seconds
			traceFlushTimer.Elapsed +=
			   new System.Timers.ElapsedEventHandler(this.Flush);

            // Write our first log message (with the client version).
			Trace.WriteLine("Starting up client; client version number is " + this.Version);
        }
#endif

        public PointF GetScreenPosition(SceneNode node, Vector3 offset, PointF pixelOffset)
        {
            Vector3 widgetPosition = node.DerivedPosition + offset;
            float screenX, screenY, screenZ;
            if (!GetScreenPosition(widgetPosition, out screenX, out screenY, out screenZ))
            {
                throw new Exception("widget is offscreen");
            }
            screenX = screenX * rootWindow.Width + pixelOffset.X;
            screenY = screenY * rootWindow.Height + pixelOffset.Y;
            if ((screenX > rootWindow.Width) || (screenX < 0.0f) ||
                (screenY > rootWindow.Height) || (screenY < 0.0f))
            {
                throw new Exception("widget is offscreen");
            }
            return new PointF(screenX, screenY);
        }

        protected bool Configurate()
        {
            DisplaySettings settings = new DisplaySettings(minScreenWidth, minScreenHeight, allowFullScreen);

            bool configureFailed = false;

            /// <summary>
            /// If this string is set, then use it to configure the display window.
            /// Using this option will bypass both the config dialog and loading
            ///   of the DisplayConfig.xml file.
            /// Format is "widthxheightxdepth".  For example "1024x768x32".
            /// </summary>
            if (manualDisplayConfigString != null)
            {
                string[] vals = manualDisplayConfigString.Split(new Char[] { 'x' });
                if (vals.Length == 3)
                {
                    int width;
                    int height;
                    int depth;
                    bool fail = false;

                    if (!int.TryParse(vals[0], out width))
                    {
                        fail = true;
                    }

                    if (!int.TryParse(vals[1], out height))
                    {
                        fail = true;
                    }

                    if (!int.TryParse(vals[2], out depth))
                    {
                        fail = true;
                    }
                    if (!fail)
                    {
                        settings.displayMode = new DisplayMode(width, height, depth, allowFullScreen && manualFullscreen);
                        settings.renderSystem = engine.RenderSystems[0];
                    }
                }
            }
            else
            {
                // attempt to load display config file first, unless the user has explicitly
                // asked for the config dialog
                settings = DisplaySettings.LoadConfig(minScreenWidth, minScreenHeight, allowFullScreen);

                if (settings.renderSystem == null || settings.displayMode == null || doDisplayConfig)
                {
                    configureFailed = true;
                    if (settings.renderSystem == null)
                    {
                        settings.renderSystem = engine.RenderSystems[0];
                        settings.renderSystemName = settings.renderSystem.Name;
                    }
                    if (settings.displayMode == null)
                        settings.displayMode = new DisplayMode(800, 600, 32, false);
                    ConfigDialog dialog = new ConfigDialog(settings);
                    System.Windows.Forms.DialogResult rv = dialog.ShowDialog();
                    if (rv == System.Windows.Forms.DialogResult.OK)
                    {
                        configureFailed = false;
                        // get render system and display mode values from the dialog
                        dialog.settings.CopyTo(settings);
                    }

                    // save the display config to a file if we got a successful configuration
                    //  from the dialog
                    if (!configureFailed)
                        DisplaySettings.SaveDisplaySettings(settings);
                }
            }

            // At this point, allowFullScreen and world specific size
            // limitations have been applied to the display modes.
            if (configureFailed || settings.renderSystem == null || settings.displayMode == null)
                // failed to configure
                return false;
            else
            {
                RenderSystem renderSystem = settings.renderSystem;
                DisplayMode displayMode = settings.displayMode;
                // set the renderSystem for axiom
                Root.Instance.RenderSystem = renderSystem;

                // set the vsync option
                renderSystem.IsVSync = settings.vSync;
                renderSystem.AllowResize = allowResize;
                renderSystem.SetConfigOption("VSync", (settings.vSync ? "Yes" : "No"));
                renderSystem.SetConfigOption("Anti Aliasing", settings.antiAliasing);

                // set whether NVPerfHUD is supported
                renderSystem.SetConfigOption("Allow NVPerfHUD", (settings.allowNVPerfHUD ? "Yes" : "No"));

                // disgusting evil mlm video stuff
                if (File.Exists("Multiverse.Movie.dll"))
                    renderSystem.SetConfigOption("Multi-Threaded", "Yes");

                int width, height;
                if (displayMode.Fullscreen)
                {
                    width = displayMode.Width;
                    height = displayMode.Height;
                }
                else
                    AdjustToFitInWorkingArea(displayMode.Width, displayMode.Height, out width, out height);

                // set the display mode for axiom to use when it creates its window
                engine.RenderSystem.ConfigOptions.SelectMode(width, height, displayMode.Depth, displayMode.Fullscreen);

                return true;
            }
        }

        /// <summary>
        ///    If either the finalWidth and finalHeight would exceed
        ///    the size of the available space for a form, adjust them
        ///    based on whether the width or height is the limiting
        ///    factor, preserving the aspect ratio.
        /// </summary>
        protected void AdjustToFitInWorkingArea(int inputWidth, int inputHeight, out int finalWidth, out int finalHeight)
        {
            finalWidth = inputWidth;
            finalHeight = inputHeight;
            int maxWidth = SystemInformation.WorkingArea.Width;
            int maxHeight = SystemInformation.WorkingArea.Height;
            int borderPixels = 2 * (SystemInformation.Border3DSize.Height + SystemInformation.BorderSize.Height);
            maxWidth -= borderPixels;
            maxHeight -= SystemInformation.CaptionHeight + borderPixels;
            if (inputWidth > maxWidth)
                finalWidth = maxWidth;
            if (inputHeight > maxHeight)
                finalHeight = maxHeight;
            if (finalHeight != inputHeight || finalWidth != inputWidth)
                log.InfoFormat("Size required adjustment: {0}x{1} => {2}x{3}", inputWidth, inputHeight, finalWidth, finalHeight);
        }

        public bool ShowLoginDialog(NetworkHelper helper)
        {
            LoginForm dialog = new LoginForm(loginSettings, helper, PatchLogFile, PatchMedia);

            System.Windows.Forms.DialogResult rv = dialog.ShowDialog();
            if (rv != System.Windows.Forms.DialogResult.OK)
                return false;
            fullScan = dialog.FullScan;
            return true;
        }

        private bool WaitForPlayerStubData()
        {
            int waitCount = 0;
            while (!worldManager.PlayerStubInitialized)
            {
                // handle one message at a time, until we get the player
                MessageDispatcher.Instance.HandleMessageQueue(1);
                if (!worldManager.PlayerStubInitialized)
                {
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
        private bool WaitForLoginResponse()
        {
            int waitCount = 0;
            while (loginMessage == null)
            {
                // handle one message at a time, until we get the login response
                MessageDispatcher.Instance.HandleMessageQueue(1);
                if (loginMessage == null)
                {
                    Thread.Sleep(100); // sleep for 100ms.
                    waitCount++;
                }
                // Wait for 10 seconds (or 100 messages)
                if (waitCount > 100)
                {
                    log.Info("When waiting for login response, got more than 100 messages or more than 10 seconds of delay");
                    return false;
                }
            }
            return true;
        }

        private bool WaitForTerrainData()
        {
            int waitCount = 0;

            while (!worldManager.TerrainInitialized)
            {
                log.Debug("checking for terrain message");
                // handle one message at a time, until we get the terrain
                MessageDispatcher.Instance.HandleMessageQueue(1);
                if (!worldManager.TerrainInitialized)
                {
                    Thread.Sleep(100); // sleep for 100ms.
                    waitCount++;
                }
                // Wait for 50 seconds (or 500 messages)
                if (waitCount > 500)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///   Sets up the asset repository based on the repository path.
        ///   If we are using a custom repository, this will be passed in.
        ///   If we are using a world specific repository, it will also be
        ///   passed in here.  If we are using the default (from the registry),
        ///   the repositoryPath parameter will be null.
        /// </summary>
        /// <param name="repositoryPath">Path to the media directory</param>
        protected static void SetupResources(List<string> directories)
        {
            if (directories != null && directories.Count > 0)
                RepositoryClass.Instance.InitializeRepositoryPath(directories);
            else
                RepositoryClass.Instance.InitializeRepositoryPath();
            foreach (string s in RepositoryClass.AxiomDirectories)
            {
                if (s != "AssetDefinitions")
                {
                    List<string> l = new List<string>();
                    foreach (string directory in RepositoryClass.Instance.RepositoryDirectoryList)
                        l.Add(Path.Combine(directory, s));
                    AssetManager.Instance.AddArchive("Common", l, "Folder");
                }
            }
            string[] resources = RepositoryClass.ClientResources;
            for (int i = 0; i < resources.Length; i += 2)
            {
                List<string> l = new List<string>();
                foreach (string directory in RepositoryClass.Instance.RepositoryDirectoryList)
                    l.Add(Path.Combine(directory, resources[i + 1]));
                AssetManager.Instance.AddArchive(resources[i], l, "Folder");
            }
        }

        public void Start()
        {
            try
            {
                try
                {
                    if (Setup())
                    {
                        // start the engines rendering loop
                        engine.StartRendering();
                    }
                }
                catch (Exception ex)
                {
                    // try logging the error here first, before Root is disposed of
                    if (LogUtil.ExceptionLog != null)
                        LogUtil.ExceptionLog.Error(ex.Message);
                    throw;
                }
            }
            catch (ClientException ex)
            {
                if (ex is PrettyClientException)
                    errorPage = ((PrettyClientException)ex).HtmlPage;
                errorMessage = ex.Message;
                try
                {
                    Root.Instance.QueueEndRendering();
                }
                catch (Exception)
                {
                }
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void OnClosing(object source, System.ComponentModel.CancelEventArgs e)
        {
            this.RequestShutdown();
        }

        protected void OnResize(object source, EventArgs args)
        {
            System.Windows.Forms.Form mainForm = (System.Windows.Forms.Form)source;
            if (mainForm.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                AdjustUiToResize(mainForm);
                lastMaximized = true;
            }
            else if (lastMaximized)
            {
                AdjustUiToResize(mainForm);
                lastMaximized = false;
            }
        }

        protected void OnResizeEnd(object source, EventArgs args)
        {
            System.Windows.Forms.Form mainForm = (System.Windows.Forms.Form)source;
            AdjustUiToResize(mainForm);
        }

        protected void AdjustUiToResize(System.Windows.Forms.Form mainForm)
        {
            int clientWidth = mainForm.Width;
            int clientHeight = mainForm.Height;
            if (!window.IsFullScreen)
            {
                int borderPixels = 2 * (SystemInformation.Border3DSize.Height + SystemInformation.BorderSize.Height);
                clientWidth -= borderPixels;
                clientHeight -= SystemInformation.CaptionHeight + borderPixels;
            }
            float aspectRatio = (float)clientWidth / (float)clientHeight;
            camera.AspectRatio = aspectRatio;
            SizeF size = new SizeF(clientWidth, clientHeight);
            rootWindow.Size = size;
            loadWindow.Size = size;
            xmlUiWindow.Size = size;
            xmlUi.OnResize();
        }

        /// <summary>
        ///		Overridden to switch to event based keyboard input.
        /// </summary>
        /// <returns></returns>
        protected bool Setup()
        {
            // get a reference to the engine singleton
            engine = new Root(null, null);

            engine.FixedFPS = fixedFPS;

            // If the user supplied the command-line arg, limit the
            // FPS.  In any case, limit the frame rate to 100 fps,
            // since this prevents the render loop spinning when
            // rendering isn't happening
            if (maxFPS > 0)
                engine.MaxFramesPerSecond = maxFPS;
            else
                engine.MaxFramesPerSecond = 100;

            // add event handlers for frame events
            engine.FrameStarted += new FrameEvent(OnFrameStarted);
            engine.FrameEnded += new FrameEvent(OnFrameEnded);

            // Set up our game specific logic
            // gameWorld = new BetaWorld(this);

            worldManager = new WorldManager();
            gameWorld.WorldManager = worldManager;

            //if (standalone) {
            //	networkHelper = new LoopbackNetworkHelper(worldManager);
            //} else
            networkHelper = new NetworkHelper(worldManager);
            if (standalone)
                this.LoopbackWorldServerEntry = networkHelper.GetWorldEntry("standalone");
            if (this.LoopbackWorldServerEntry != null)
            {
                string worldId = this.LoopbackWorldServerEntry.WorldName;
                // Bypass the login and connection to master server
                loginSettings.worldId = worldId;
                networkHelper.SetWorldEntry(worldId, this.LoopbackWorldServerEntry);
                networkHelper.MasterToken = this.LoopbackIdToken;
                networkHelper.OldToken = this.LoopbackOldToken;
            }
            else
            {
                // Show the login dialog.  If we successfully return from this
                // we have initialized the helper's world entry map with the
                // resolveresponse data.
                if (!ShowLoginDialog(networkHelper))
                    return false;
            }

            // Update our media tree (base this on the world to which we connect)
            WorldServerEntry entry = networkHelper.GetWorldEntry(loginSettings.worldId);
            // If they specify an alternate location for the media patcher,
            // use that.
            if (this.WorldPatcherUrl != null)
                entry.PatcherUrl = this.WorldPatcherUrl;
            if (this.WorldUpdateUrl != null)
                entry.UpdateUrl = this.WorldUpdateUrl;
            if (this.PatchMedia &&
                entry.PatcherUrl != null && entry.PatcherUrl != string.Empty &&
                entry.UpdateUrl != null && entry.UpdateUrl != string.Empty)
            {
                // Fetch the appropriate media (full scan)
                if (!UpdateWorldAssets(entry.WorldRepository, entry.PatcherUrl, entry.UpdateUrl, fullScan))
                    return false;
            }

            // exit the client after patching media if flag is set
            if (this.ExitAfterMediaPatch)
            {
                return false;
            }
            // If we aren't overriding the repository, and we could have
            // updated, use the world directory.  If we don't have an
            // update url for some reason, use the default repository.
            if (!useRepository && entry.UpdateUrl != null && entry.UpdateUrl != string.Empty)
                this.RepositoryPaths = entry.WorldRepositoryDirectories;

            // Tell the engine where to find media
            SetupResources(this.RepositoryPaths);

            // We need to load the Movie plugin here.
            PluginManager.Instance.LoadPlugins(".", "Multiverse.Movie.dll");

            // Set up the scripting system
            log.Debug("Client.Startup: Loading UiScripting");
            UiScripting.SetupInterpreter();
            UiScripting.LoadCallingAssembly();
            log.Debug("Client.Startup: Finished loading UiScripting");

            log.Debug("Client.Startup: Loading Assemblies");
            ArrayList assemblies = PluginManager.Instance.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                UiScripting.LoadAssembly(assembly);
            }
            log.Debug("Client.Startup: Finished loading Assemblies");

            // Run script that contains world-specific initializations
            // that must be performed before the display device is
            // created.  But for upward compatibility, if we can't
            // find the file, don't throw an error.
            UiScripting.RunFile("ClientInit.py");

            // Wait til now to set the networkHelper.UseTCP flag,
            // because the ClientInit.py may have overriden it.
            networkHelper.UseTCP = this.UseTCP;

            // Register our handlers.  We must do this before we call
            // MessageDispatcher.Instance.HandleMessageQueue, so that we will
            // get the callbacks for the incoming messages.
            SetupMessageHandlers();

            string initialLoadBitmap = "";
            if (File.Exists(MultiverseLoadScreen))
                initialLoadBitmap = MultiverseLoadScreen;

            // show the config dialog and collect options
            if (!Configurate())
            {
                log.Warn("Failed to configure system");
                // shutting right back down
                engine.Shutdown();
                throw new ClientException("Failed to configure system");
                // return false;
            }

            // setup the engine
            log.Debug("Client.Startup: Calling engine.Initialize()");
            window = engine.Initialize(true, "Multiverse World Browser", initialLoadBitmap);
            log.Debug("Client.Startup: Finished engine.Initialize()");
            try
            {
                System.Windows.Forms.Control control = window.GetCustomAttribute("HWND") as System.Windows.Forms.Control;
                System.Windows.Forms.Form f = control.FindForm();
                if (allowResize)
                {
                    if (!window.IsFullScreen)
                        f.MaximizeBox = true;
                    f.Resize += this.OnResize;
                    f.ResizeEnd += this.OnResizeEnd;
                }
                f.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
            }
            catch (Exception)
            {
                log.Warn("Unable to register closing event handler");
            }
            try
            {
                System.Windows.Forms.Control control = window.GetCustomAttribute("HWND") as System.Windows.Forms.Control;
                System.Windows.Forms.Form f = control.FindForm();
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
                Stream iconStream = assembly.GetManifestResourceStream("MultiverseClient.MultiverseIcon.ico");
                control.FindForm().Icon = new System.Drawing.Icon(iconStream);
            }
            catch (Exception e)
            {
                LogUtil.ExceptionLog.Warn("Unable to load or apply MultiverseIcon.  Using default instead");
                LogUtil.ExceptionLog.InfoFormat("Exception Detail: {0}", e);
            }

            // Tell the MeterManager where it should write logs
            MeterManager.MeterEventsFile = MeterEventsFile;
            MeterManager.MeterLogFile = MeterLogFile;

            // This seems to need the window object to have been initialized
            TextureManager.Instance.DefaultNumMipMaps = 5;

            ChooseSceneManager();

            // Set up my timers so that I can see how much time is spent in
            // the various render queues.
            scene.QueueStarted += new RenderQueueEvent(scene_OnQueueStarted);
            scene.QueueEnded += new RenderQueueEvent(scene_OnQueueEnded);

            // XXXMLM - force a reference to the assembly for script reflection
            log.Debug("Client.Startup: Calling Multiverse.Web.Browser.RegisterForScripting()");
            Multiverse.Web.Browser.RegisterForScripting();
            log.Debug("Client.Startup: Finished Multiverse.Web.Browser.RegisterForScripting()");

            log.Debug("Client.Startup: Calling SetupGui()");
            SetupGui();

            // Tell Windows that the widget containing the DirectX
            // screen should now be visible.  This may not be the
            // right place to do this.
            window.PictureBoxVisible = true;

            // Set the scene manager
            worldManager.SceneManager = scene;

            // Set up a default ambient light for the scene manager
            scene.AmbientLight = ColorEx.Black;

            // Sets up the various things attached to the world manager,
            // as well as registering the various message handlers.
            // This also initializes the networkHelper.
            worldManager.Init(rootWindow, this);

            // At this point, I can have a camera
            CreateCamera();
            // #if !PERFHUD_BUILD
            inputHandler = new DefaultInputHandler(this);
            // inputHandler.InitViewpoint(worldManager.Player);
            // #endif
            CreateViewports();

            // call the overridden CreateScene method
            CreateScene();

            // Set up our game specific stuff (right now, just betaworld)
            gameWorld.Initialize();
            gameWorld.SetupMessageHandlers();

            needConnect = true;

            // retrieve and initialize the input system
            // input = PlatformManager.Instance.CreateInputReader();
            // input.Initialize(window, true, true, false, false);

            // Initialize the client API and load world specific scripts
            ClientAPI.InitAPI(gameWorld);

            Monitor.Enter(scene);

            log.InfoFormat("Client setup complete at {0}", DateTime.Now);
            // At this point, you can create timer events.
            return true;
        }

        /// <summary>
        ///   Set up our hooks for handling messages.
        /// </summary>
        protected void SetupMessageHandlers()
        {
            // Register our handler for the Portal messages, so that we
            // can drop our connection to the world server, and establish a new
            // connection to the new world.
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Portal,
                                                       new WorldMessageHandler(this.HandlePortal));
            // Register our handler for the UiTheme messages, so that we
            // can swap out the user interface.
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.UiTheme,
                                                       new WorldMessageHandler(this.HandleUiTheme));
            // Register our handler for the LoginResponse messages, so that we
            // can throw up a dialog if needed.
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.LoginResponse,
                                                       new WorldMessageHandler(this.HandleLoginResponse));

            // Register our handler for the AuthorizedLoginResponse messages, so that we
            // can throw up a dialog if needed.  This version allows us to get the server
            // version information as well (and possibly capabilities).
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AuthorizedLoginResponse,
                                                       new WorldMessageHandler(this.HandleAuthorizedLoginResponse));
        }

        public void ToggleRenderMode()
        {
            if (camera.SceneDetail == SceneDetailLevel.Points)
                camera.SceneDetail = SceneDetailLevel.Solid;
            else if (camera.SceneDetail == SceneDetailLevel.Solid)
                camera.SceneDetail = SceneDetailLevel.Wireframe;
            else
                camera.SceneDetail = SceneDetailLevel.Points;

            log.InfoFormat("Rendering mode changed to '{0}'.", camera.SceneDetail);
        }

        public void ToggleRenderCollisionVolumes()
        {
            worldManager.ToggleRenderCollisionVolumes();
        }

        public void ToggleMetering()
        {
            if (engine != null)
                engine.ToggleMetering(int.MaxValue);
        }

        public void MeterOneFrame()
        {
            if (engine != null)
                engine.ToggleMetering(1);
        }

        public bool ToggleMeteringLongestFrame()
        {
            if (engine != null)
                return engine.ToggleMeteringLongestFrame();
            else
                return false;
        }

        public void HandleCommand(string message)
        {
            log.ErrorFormat("Running deprecated version of HandleCommand: {0}", message);
        }

        public void RegisterNewStyleCommandHandler(string command, CommandHandler handler)
        {
            commandHandlers[command] = handler;
        }

        public void SetParameter(string paramName, string paramValue)
        {
            parameters[paramName] = paramValue;
        }

        public bool HasParameter(string paramName)
        {
            return parameters.ContainsKey(paramName);
        }

        public string GetParameter(string paramName)
        {
            return parameters[paramName];
        }

        #region Message Handlers

        private void HandleUiTheme(BaseWorldMessage message)
        {
            UiThemeMessage uiTheme = (UiThemeMessage)message;
            uiModules = new List<string>(uiTheme.UiModules);
            if (uiModules.Count == 0)
                uiModules.Add("basic.toc");
            keyBindingsFile = uiTheme.KeyBindingsFile;
            ReloadUiElements();
        }

        private void HandleLoginResponse(BaseWorldMessage message)
        {
            LoginResponseMessage loginResponse = (LoginResponseMessage)message;
            loginFailed = !loginResponse.Success;
            loginMessage = loginResponse.Message;
        }

        private void HandleAuthorizedLoginResponse(BaseWorldMessage message)
        {
            AuthorizedLoginResponseMessage loginResponse = (AuthorizedLoginResponseMessage)message;
            loginFailed = !loginResponse.Success;
            loginMessage = loginResponse.Message;
            worldServerVersion = loginResponse.Version;
            // If the server version starts with "2007-07-20" or with
            // "1.0", it's a pre-1.1 version, so don't expect
            // LoadingStateMessages.
            log.InfoFormat("In HandleAuthorizedLoginResponse, testing server version '{0}'", worldServerVersion);
            if ((worldServerVersion.Length >= 10 && worldServerVersion.Substring(0, 10) == "2007-07-20") ||
                (worldServerVersion.Length >= 3 && worldServerVersion.Substring(0, 3) == "1.0"))
                fullyInitialized = true;
            if (worldServerVersion.Length >= 3 && worldServerVersion.Substring(0, 3) == "1.5")
                serverRelease1_5 = true;
            log.InfoFormat("World Server Version: {0}", worldServerVersion);
        }

        private bool WaitForStartupMessages()
        {
            if (!WaitForLoginResponse())
            {
                log.Error("Unable to retrieve login response");
                return false;
            }
            else if (loginFailed)
            {
                // Ok, we got the login response, but if we fail, we
                // should skip waiting for player and terrain data
                return false;
            }
            log.Debug("waiting for terrain data");
            if (!WaitForTerrainData())
            {
                log.Error("Unable to retrieve terrain data");
                return false;
            }
            log.Debug("got tdata response");
            if (!WaitForPlayerStubData())
            {
                log.Error("Unable to retrieve player data");
                return false;
            }
            log.Debug("got pdata response");
            return true;
        }

        private void HandlePortal(BaseWorldMessage message)
        {
            PortalMessage portalMessage = (PortalMessage)message;
            Write("Transporting via portal to alternate world.");

            loginSettings.worldId = portalMessage.WorldId;
            loginSettings.characterId = portalMessage.CharacterId;
            portalMessage.AbortHandling = true;

            needConnect = true;
        }

        protected CharacterEntry SelectAnyCharacter(List<CharacterEntry> entries)
        {
            // int characterIndex = NetworkHelper.CharacterIndex;
            if (entries.Count <= 0)
            {
                log.Error("No characters available");
                return null;
            }
            return entries[0];
        }

        protected CharacterEntry SelectCharacter(List<CharacterEntry> entries, long characterId)
        {
            foreach (CharacterEntry entry in entries)
            {
                if (entry.CharacterId == characterId)
                    return entry;
            }
            log.ErrorFormat("No characters matching characterId {0}", characterId);
            return null;
        }

        protected bool DoWorldConnect()
        {
            Monitor.Enter(scene);
            try
            {
                // Set up the mask window, so that they don't see the loading
                loadWindow.Visible = true;
                xmlUiWindow.Visible = false;
                MessageDispatcher.Instance.ClearQueue();
                Monitor.Enter(worldManager);
                string clientVersion = this.Version;
                try
                {
                    string worldId = loginSettings.worldId;
                    WorldServerEntry entry;
                    Boolean reconnect = (worldId != networkHelper.connectedWorldId);
                    NetworkHelperStatus status = NetworkHelperStatus.Success;
                    worldManager.ClearWorld();
                    if (reconnect)
                    {
                        networkHelper.Disconnect();

                        //if (useLocalWorld)
                        //    worldId = "local_world";
                        if (!networkHelper.HasWorldEntry(loginSettings.worldId))
                            networkHelper.ResolveWorld(loginSettings);
                        entry = networkHelper.GetWorldEntry(loginSettings.worldId);
                        status = networkHelper.ConnectToLogin(loginSettings.worldId, clientVersion);
                        if (status == NetworkHelperStatus.UnsupportedClientVersion)
                        {
                            // Temporary workaround to handle version with the 1.1 server.
                            // Unfortunately, the 1.1 server does not handle newer client numbers.
                            log.InfoFormat("Attempting to connect as a 1.1 client");
                            clientVersion = "1.1";
                            networkHelper.Disconnect();
                            status = networkHelper.ConnectToLogin(loginSettings.worldId, clientVersion);
                        }
                        if (status == NetworkHelperStatus.WorldTcpConnectFailure)
                        {
                            errorMessage = "Unable to connect to tcp world server";
                            errorPage = "unable_connect_tcp_world.htm";
                            return false;
                        }
                        else if (status == NetworkHelperStatus.UnsupportedClientVersion)
                        {
                            errorMessage = "Server does not support this version";
                            errorPage = "unable_connect_tcp_world.htm";
                            return false;
                        }
                    }
                    else
                    {
                        entry = networkHelper.GetWorldEntry(loginSettings.worldId);
                    }
                    foreach (Dictionary<string, object> c in networkHelper.CharacterEntries)
                    {
                        log.Info("Character: ");
                        foreach (string attr in c.Keys)
                            log.InfoFormat("  '{0}' => '{1}'", attr, c[attr]);
                    }
                    // We need to hook our message filter, whether or not we are
                    // standalone, so instead of doing it later (right before
                    // RdpWorldConnect), do it here.
                    RequireLoginFilter checkAndHandleLogin = new RequireLoginFilter(worldManager);
                    MessageDispatcher.Instance.SetWorldMessageFilter(checkAndHandleLogin.ShouldQueue);

                    if (entry.StartupScript != null)
                        UiScripting.RunFile(entry.StartupScript, "Standalone");

                    if (status != NetworkHelperStatus.Success &&
                        status != NetworkHelperStatus.Standalone)
                    {
                        log.InfoFormat("World Connect Status: {0}", status);
                        errorMessage = "Unable to connect to server";
                        return false;
                    }
                    if (status != NetworkHelperStatus.Standalone && !useLocalWorld)
                    {
                        CharacterEntry charEntry;
                        if (loginSettings.characterId > 0)
                        {
                            log.InfoFormat("Selecting character with id: {0}", loginSettings.characterId);
                            charEntry = SelectCharacter(networkHelper.CharacterEntries, loginSettings.characterId);
                        }
                        else
                        {
                            log.Warn("Selecting first character in list of characters (deprecated)");
                            // just grab the first
                            charEntry = SelectAnyCharacter(networkHelper.CharacterEntries);
                        }

                        string proxyHostname = charEntry.Hostname;
                        int proxyPort = charEntry.Port;
                        if (proxyHostname == null)
                        {
                            NetworkHelperStatus tokenStatus = networkHelper.GetProxyToken(charEntry.CharacterId);
                            proxyHostname = networkHelper.ProxyPluginHost;
                            proxyPort = networkHelper.ProxyPluginPort;
                        }
                        networkHelper.DisconnectFromLogin();

                        string actualHost = proxyHostname;
                        if (actualHost == ":same")
                            actualHost = NetworkHelper.Instance.LoginPluginHost;

                        status = networkHelper.ConnectToWorld(charEntry.CharacterId,
                                                              actualHost,
                                                              proxyPort,
                                                              clientVersion + ", " + ClientCapabilities);
                        if (status != NetworkHelperStatus.Success)
                        {
                            log.InfoFormat("World Connect Status: {0}", status);
                            return false;
                        }
                    }
                    ClientAPI.OnWorldConnect();
                }
                finally
                {
                    Monitor.Exit(worldManager);
                }
                // We use the null setting to indicate that we haven't logged in, so
                // we need to reset it back to null before the connect.
                loginMessage = null;
                // At this point, the network helper can start handling messages.
                if (!WaitForStartupMessages())
                {
                    if (loginFailed && loginMessage != null)
                        // The server rejected our login
                        errorMessage = loginMessage;
                    else if (loginMessage != null)
                        // our login went ok (and we got something back), but something else (terrain/player) failed
                        errorMessage = "Unable to communicate with server";
                    else
                        errorMessage = "Unable to connect to server";
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogUtil.ExceptionLog.WarnFormat("Got an exception in world connect: {0}", ex);
            }
            finally
            {
                Monitor.Exit(scene);
            }
            return true;
        }

        #endregion Message Handlers

        public void ToggleTexture()
        {
            int aniso = 1;
            // toggle the texture settings
            switch (filtering)
            {
                case TextureFiltering.Bilinear:
                    filtering = TextureFiltering.Trilinear;
                    aniso = 1;
                    break;
                case TextureFiltering.Trilinear:
                    filtering = TextureFiltering.Anisotropic;
                    aniso = 8;
                    break;
                case TextureFiltering.Anisotropic:
                    filtering = TextureFiltering.Bilinear;
                    aniso = 1;
                    break;
            }

            log.InfoFormat("Texture Filtering changed to '{0}'.", filtering);

            // set the new default
            MaterialManager.Instance.SetDefaultTextureFiltering(filtering);
            MaterialManager.Instance.DefaultAnisotropy = aniso;
        }

        /// <summary>
        ///	   Handle the text changes in the edit box.
        ///    Reflect them in the new ui widget.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnTextChanged(object sender, WindowEventArgs e) {
        //    if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
        //        return;
        //    Multiverse.Interface.EditBox editFrame =
        //        (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
        //    CrayzEdsGui.Base.Widgets.EditBox editBox =
        //        sender as CrayzEdsGui.Base.Widgets.EditBox;
        //    // update our text to reflect that of the cegui element
        //    editFrame.SetText(editBox.Text);
        //    // update out caret to reflect that of the cegui element
        //    OnCaretMoved(sender, e);
        //}
        //private void OnCharacter(object sender, KeyEventArgs e) {
        //    if (e.Character == '\0')
        //        return;
        //    if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
        //        return;
        //    Multiverse.Interface.EditBox editFrame =
        //        (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
        //    CrayzEdsGui.Base.Widgets.EditBox editBox =
        //        sender as CrayzEdsGui.Base.Widgets.EditBox;
        //    // update our text to reflect that of the cegui element
        //    editFrame.SetText(editBox.Text);
        //    // update out caret to reflect that of the cegui element
        //    OnCaretMoved(sender, new WindowEventArgs(editBox));
        //}
        //private void OnCaretMoved(object sender, WindowEventArgs e) {
        //    if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
        //        return;
        //    Multiverse.Interface.EditBox editFrame =
        //        (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
        //    CrayzEdsGui.Base.Widgets.EditBox editBox =
        //        sender as CrayzEdsGui.Base.Widgets.EditBox;
        //    editFrame.CaretIndex = editBox.CaratIndex;
        //}
        //private void OnActivated(object sender, WindowEventArgs e) {
        //    if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
        //        return;
        //    Multiverse.Interface.EditBox editFrame =
        //        (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
        //    editFrame.ActivateWindow();
        //    editFrame.Show();
        //}
        //private void OnDeactivated(object sender, WindowEventArgs e) {
        //    if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
        //        return;
        //    Multiverse.Interface.EditBox editFrame =
        //        (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
        //    editFrame.Hide();
        //}
        //private void OnTextSelectionChanged(object sender, WindowEventArgs e) {
        //    if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
        //        return;
        //    Multiverse.Interface.EditBox editFrame =
        //        (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
        //    CrayzEdsGui.Base.Widgets.EditBox editBox =
        //        sender as CrayzEdsGui.Base.Widgets.EditBox;
        //    if (editBox.SelectionStartIndex != editBox.SelectionEndIndex)
        //        editFrame.Select(editBox.SelectionStartIndex, editBox.SelectionEndIndex);
        //    else
        //        // Looks like some strangeness when there is a cursor in the editbox.
        //        editFrame.Select(0, 0);
        //}

        public void ToggleBoundingBoxes()
        {
            scene.ShowBoundingBoxes = !scene.ShowBoundingBoxes;
        }

        private bool scene_OnQueueStarted(RenderQueueGroupID x)
        {
            //if (x == RenderQueueGroupID.Overlay)
            //    renderOverlayQueueTimer.Start();
            //else
            // renderQueueTimer.Enter();
            MeterManager.AddInfoEvent(string.Format("Client Render Queue Timer OnQueueStarted({0})", x));
            return false;
        }

        private bool scene_OnQueueEnded(RenderQueueGroupID x)
        {
            //if (x == RenderQueueGroupID.Overlay)
            //    renderOverlayQueueTimer.Stop();
            //else
            // renderQueueTimer.Exit();
            MeterManager.AddInfoEvent(string.Format("Client Render Queue Timer OnQueueEnded({0})", x));
            return false;
        }

        public void TakeScreenshot()
        {
            string fileName = GetNextScreenshotFilename();

            TakeScreenshot(fileName);

            // show briefly on the screen
            log.InfoFormat("Wrote screenshot '{0}'.", fileName);
            Write("Screenshot saved as '" + fileName + "'");
        }

        public string GetNextScreenshotFilename()
        {
            return GetNextScreenshotFilename(ScreenshotsFolder, "png");
        }

        public string GetNextScreenshotFilename(string screenshotFolder)
        {
            return GetNextScreenshotFilename(screenshotFolder, "png");
        }

        /// <summary>
        ///   Get the filename that should be used for the next screenshot.
        ///   To get this filename, we look at all the files in the screenshot
        ///   folder that match our naming convention, parse out the number,
        ///   and choose a number that is greater than any of the existing
        ///   screenshot numbers.
        /// </summary>
        /// <param name="extension">the extension to use for the file</param>
        /// <returns>
        ///   the full path to the file that should be used as a screenshot
        /// </returns>
        public static string GetNextScreenshotFilename(string screenshotFolder, string extension)
        {
            string[] temp = Directory.GetFiles(screenshotFolder, "screenshot*.*");
            int maxScreenshot = 0;
            Regex rx = new Regex(@"screenshot([0-9]+)\..*", RegexOptions.IgnoreCase);
            foreach (string name in temp)
            {
                Match m = rx.Match(name);
                if (m.Success)
                {
                    int tmp = int.Parse(m.Groups[1].ToString());
                    if (tmp > maxScreenshot)
                        maxScreenshot = tmp;
                }
            }
            return Path.Combine(screenshotFolder, string.Format("screenshot{0:00000}.{1}", maxScreenshot + 1, extension));
        }

        protected void TakeScreenshot(string fileName)
        {
            window.Save(fileName);
        }

        protected static TimingMeter frameMeter = MeterManager.GetMeter("Client Frame", "Client");
        protected static TimingMeter visibilityMeter = MeterManager.GetMeter("Client Visibility", "Client");
        protected static TimingMeter messageMeter = MeterManager.GetMeter("Client Message Handling", "Client");
        protected static TimingMeter worldManagerMeter = MeterManager.GetMeter("Client WorldMgr", "Client");
        protected static TimingMeter inputMeter = MeterManager.GetMeter("Client Input Handling", "Client");

        protected static TimingMeter clientAPIFrameStartedMeter = MeterManager.GetMeter("OnFrameStarted", "ClientAPI");
        protected static TimingMeter clientAPISceneAnimMeter = MeterManager.GetMeter("ProcessSceneAnimations", "ClientAPI");
        protected static TimingMeter clientAPIProcessYieldEffectsMeter = MeterManager.GetMeter("ProcessYieldEffects", "ClientAPI");

        protected void OnFrameEnded(object source, FrameEventArgs e)
        {
            frameMeter.Enter();
            // Special case for the character selection screen
            if (loadWindow.Visible && camera != null && worldManager.Player != null &&
                (useLocalWorld || (!serverRelease1_5 && fullyInitialized)))
            {
                loadWindow.Visible = false;
                xmlUiWindow.Visible = true;
            }

            // interFrameTimer.Exit();

            messageMeter.Enter();
            MessageDispatcher.Instance.HandleMessageQueue(MaxMSForMessagesPerFrame);
            messageMeter.Exit();

            // trigger client scripting event handlers
            ClientAPI.OnFrameEnded(e.TimeSinceLastFrame);

            frameMeter.Exit();

            if (requestShutdown)
            {
                Root.Instance.QueueEndRendering();
                return;
            }
        }

        protected void OnFrameStarted(object source, FrameEventArgs e)
        {
            if (needConnect)
            {
                if (!DoWorldConnect())
                    Root.Instance.QueueEndRendering();
                needConnect = false;
            }

            scene.FindVisibleObjectsBool = !loadWindow.Visible;

            //             log.DebugFormat("OnFrameStarted: frameCounter {0}", frameCounter);

            // Calculate the number of render calls since the last frame
            lastFrameRenderCalls = (int)(RenderSystem.TotalRenderCalls - lastTotalRenderCalls);
            // Remember the current count for next frame
            lastTotalRenderCalls = RenderSystem.TotalRenderCalls;

            Monitor.Exit(scene);

            //
            // If we are using a fixed framerate (non-realtime rendering) then calculate the
            // current time based on that framerate and the current frame counter.
            //
            if (fixedFPS > 0)
            {
                Multiverse.Utility.TimeTool.TimeOverride = (long)frameCounter * 1000L / fixedFPS;
            }

            //if (MeterManager.Collecting == false)
            //    this.ToggleMetering();

            // If the command-line argument made framesBetweenSleeps
            // non-zero, then test to see if this is a frame in which
            // we should sleep
            if (framesBetweenSleeps > 0)
            {
                frameSleepCount--;
                if (frameSleepCount <= 0)
                {
                    Thread.Sleep(20);
                    frameSleepCount = framesBetweenSleeps;
                }
            }
            if (e.TimeSinceLastFrame > 0.2f)
                log.InfoFormat("Frame time of {0} is greater than 200ms.  Average FPS is {1}.",
                               e.TimeSinceLastFrame, Root.Instance.AverageFPS);
            // Track the longest frame we have had in the recent past
            maxElapsed[frameCounter++ % maxElapsed.Length] = e.TimeSinceLastFrame;
            if (guiRenderer != null)
                guiRenderer.FrameCounter = frameCounter;
            maxRecentFrameTime = 0.0f;
            for (int i = 0; i < maxElapsed.Length; ++i)
                if (maxElapsed[i] > maxRecentFrameTime)
                    maxRecentFrameTime = maxElapsed[i];

            time += e.TimeSinceLastFrame;
            if (scene is Axiom.SceneManagers.Multiverse.SceneManager)
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.Time = time;

            long now = WorldManager.CurrentTime;

            worldManagerMeter.Enter();
            worldManager.OnFrameStarted(e.TimeSinceLastFrame, now);
            worldManagerMeter.Exit();

            inputMeter.Enter();
            if (worldManager.Player != null)
                inputHandler.OnFrameStarted(source, e, now);
            inputMeter.Exit();

            UiSystem.OnUpdate(e.TimeSinceLastFrame);

            // trigger client scripting event handlers
            clientAPIFrameStartedMeter.Enter();
            ClientAPI.OnFrameStarted(e.TimeSinceLastFrame);
            clientAPIFrameStartedMeter.Exit();

            // process any pending animations
            clientAPIProcessYieldEffectsMeter.Enter();
            if (!loadWindow.Visible)
                ClientAPI.ProcessSceneAnimations(e.TimeSinceLastFrame);
            clientAPIProcessYieldEffectsMeter.Exit();

            // process coordinated effects
            clientAPISceneAnimMeter.Enter();
            ClientAPI.ProcessYieldEffectQueue();
            clientAPISceneAnimMeter.Exit();

            // do per-frame sound processing
            SoundManager.Instance.Update();

            // let the web texture fetcher do work
            TextureFetcher.Instance.Process();

            // interFrameTimer.Enter();
            Monitor.Enter(scene);
        }

        public bool GetScreenPosition(Vector3 pos, out float screenX, out float screenY)
        {
            float screenZ;
            return GetScreenPosition(pos, out screenX, out screenY, out screenZ);
        }

        /// <summary>
        ///   Get the screen position for an object.  All coordinates are in the range [0, 1].
        /// </summary>
        /// <param name="pos">Position of the object in the world</param>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="screenZ"></param>
        /// <returns></returns>
        public bool GetScreenPosition(Vector3 pos, out float screenX, out float screenY, out float screenZ)
        {
            screenX = screenY = screenZ = 0.0f;
            float normalizedSlope = MathUtil.Tan(MathUtil.DegreesToRadians(camera.FOVy * 0.5f));
            float viewportYToWorldY = normalizedSlope * camera.Near * 2;
            float viewportXToWorldX = viewportYToWorldY * camera.AspectRatio;

            Vector3 deltaPos = camera.ViewMatrix * pos;
            float zDist = -deltaPos.z;
            if (zDist > camera.Far || zDist < camera.Near)
                return false;
            float scale = -camera.Near / deltaPos.z;
            screenX = scale * deltaPos.x / viewportXToWorldX + 0.5f;
            screenY = 0.5f - scale * deltaPos.y / viewportYToWorldY;
            screenZ = (zDist - camera.Near) / (camera.Far - camera.Near);
            if (screenX >= 0 && screenX <= 1 && screenY >= 0 && screenY <= 1)
                return true;

            return false;
        }

        public Vector3 GetScreenPosition(Vector3 pos)
        {
            Vector3 rv = new Vector3();
            if (!GetScreenPosition(pos, out rv.x, out rv.y, out rv.z))
                return Vector3.Zero;
            return rv;
        }

        /// <summary>
        ///   mouseX and mouseY are in terms of Overlay coordinates
        ///   (0 to 1 starting from the upper left corner of the screen).
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public ObjectNode CastRay(float mouseX, float mouseY)
        {
            return CastRay(mouseX, mouseY, 1 << 3);
        }

        /// <summary>
        ///   This overloading permits the user to pass in a query mask
        ///   mouseX and mouseY are in terms of Overlay coordinates
        ///   (0 to 1 starting from the upper left corner of the screen).
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <param name="queryMas"></param>
        /// <returns></returns>
        public ObjectNode CastRay(float mouseX, float mouseY, ulong queryMask)
        {
            Ray ray = camera.GetCameraToViewportRay(mouseX, mouseY);
#if NOT
			Vector3 origin, direction;
			float winWidth = (float)(2 * camera.Near *
							  Math.Tan(MathUtil.DegreesToRadians(camera.FOV / 2)));
            direction.x = (mouseX - .5f) * winWidth;
            direction.y = (.5f - mouseY) * winWidth / camera.AspectRatio;
            direction.z = -camera.Near;
            origin = camera.Position;
            direction = camera.Orientation * direction;
            Ray ray = new Ray(origin, direction);
#endif
            RaySceneQuery query = scene.CreateRayQuery(ray, queryMask);
            query.SortByDistance = true;
            List<RaySceneQueryResultEntry> results = query.Execute();
            if (results.Count == 0)
                return null;

            MovableObject closestObj = null;
            float closestMatch = float.MaxValue;

            // Check that the picked object is not terrain, not a prop, and
            // that it is the closest object.
            foreach (RaySceneQueryResultEntry result in results)
            {
                MovableObject sceneObj = result.SceneObject;
                if (sceneObj == null || sceneObj.UserData == null || !(sceneObj.UserData is ObjectNode))
                    continue;

                // Skip player and buildings (props)
                ObjectNode objNode = (ObjectNode)sceneObj.UserData;
                float distanceToObject = result.Distance;
                bool hitCollisionVolume = false;
                if (targetBasedOnCollisionVolumes)
                {
                    // If there are are no collision shapes to collide
                    // with, fall back to the distance to the bounding
                    // box.
                    if ((objNode.Collider != null && objNode.Collider.parts.Count > 0) ||
                        (objNode.CollisionShapes != null && objNode.CollisionShapes.Count > 0))
                    {
                        float previousDistance = distanceToObject;
                        distanceToObject = CheckCollisionWithCollisionShapes(objNode, ray);
                        hitCollisionVolume = true;
                        //                         log.DebugFormat("Client.CastRay: objNode {0}, distanceToObject {1}, previousDistance {2}",
                        //                             objNode.Name, distanceToObject, previousDistance);
                    }
                }
                float dist;
#if PER_POLY_PICKING
                if (!CheckCollision(ray, sceneObj, out dist))
					continue;
#else
                // If we aren't doing the per-poly stuff, we would use this instead.
                dist = distanceToObject;
#endif
                if (dist < closestMatch)
                {
                    // If the closest thing is a collision volume, and
                    // it's not targetable and not the player, set
                    // closestObj to null.
                    if (!objNode.Targetable && objNode != worldManager.Player && hitCollisionVolume)
                    {
                        closestMatch = dist;
                        closestObj = null;
                    }
                    else if (objNode.Targetable)
                    {
                        closestMatch = dist;
                        closestObj = sceneObj;
                    }
                }
            }
            if (closestObj == null)
                return null;
            else
                return closestObj.UserData as ObjectNode;
        }

        // By the time this method is called, we know for sure that
        // the object has collision volumes.
        protected float CheckCollisionWithCollisionShapes(ObjectNode objNode, Ray ray)
        {
            float distance = float.MaxValue;
            if (objNode.Collider != null)
            {
                MovingObject mo = objNode.Collider;
                foreach (MovingPart part in mo.parts)
                {
                    CollisionShape partShape = mo.parts[0].shape;
                    // Check for intersection of a line 1000 meters long from the ray origin toward the ray direction
                    float distanceToShape = partShape.RayIntersectionDistance(ray.Origin, ray.Origin + ray.Direction * (1000f * OneMeter));
                    if (distanceToShape == float.MaxValue)
                        continue;
                    else if (distanceToShape < distance)
                        distance = distanceToShape;
                }
            }
            else if (objNode.CollisionShapes != null)
            {
                foreach (CollisionShape shape in objNode.CollisionShapes)
                {
                    // Check for intersection of a line 1000 meters long from the ray origin toward the ray direction
                    float distanceToShape = shape.RayIntersectionDistance(ray.Origin, ray.Origin + ray.Direction * (1000f * OneMeter));
                    if (distanceToShape == float.MaxValue)
                        continue;
                    else if (distanceToShape < distance)
                    {
                        //                         log.DebugFormat("Client.CheckCollisionWithCollisionShapes: objNode {0}, distanceToShape {1}, shape {2}",
                        //                             objNode.Name,distanceToShape, shape);
                        distance = distanceToShape;
                    }
                }
            }
            return distance;
        }

        public void RequestShutdown()
        {
            requestShutdown = true;
        }

        public void RequestShutdown(string message)
        {
            errorMessage = message;
            requestShutdown = true;
        }

        public bool UseLocalWorld
        {
            get
            {
                return useLocalWorld;
            }
            set
            {
                useLocalWorld = value;
            }
        }

        #endregion Methods

#if PER_POLY_PICKING
		protected bool CheckCollision(Ray ray, SceneObject sceneObj, out float t) {
			t = float.MaxValue;
			if (!(sceneObj is Entity))
				return false;
			bool intersect = false;
			Entity entity = (Entity)sceneObj;
			VertexData vData;
			if (entity.Mesh.HasSkeleton) {
				// The blended vertex buffers have already been transformed
				for (int i = 0; i < entity.SubEntityCount; ++i) {
					SubEntity subEntity = entity.GetSubEntity(i);
					if (!subEntity.IsVisible)
						continue;
					if (subEntity.SubMesh.useSharedVertices)
						vData = entity.SharedBlendedVertexData;
					else
						vData = subEntity.BlendedVertexData;
					IndexData iData = subEntity.SubMesh.indexData;
					if (CheckCollision(vData, iData, ray.Origin, ray.Direction, ref t))
						intersect = true;
				}
			} else {
				// Transform the ray based on the inverse of the sceneObj transform, so
				// we don't need to transform each point in the submesh.
				Matrix4 inverseTransform = sceneObj.ParentNodeFullTransform.Inverse();
				Vector3 origin = inverseTransform * ray.Origin;
				Vector3 direction = (inverseTransform * (ray.Origin + ray.Direction)) - origin;
				for (int i = 0; i < entity.SubEntityCount; ++i) {
					SubEntity subEntity = entity.GetSubEntity(i);
					if (!subEntity.IsVisible)
						continue;
					if (subEntity.SubMesh.useSharedVertices)
						vData = entity.Mesh.SharedVertexData;
					else
						vData = subEntity.SubMesh.vertexData;
					IndexData iData = subEntity.SubMesh.indexData;
					if (CheckCollision(vData, iData, origin, direction, ref t))
						intersect = true;
				}
			}
			return intersect;
		}

		private bool CheckCollision(VertexData vData, IndexData iData,
									Vector3 origin, Vector3 direction,
									ref float t)
		{
			bool intersect = false;
			Vector3[] points;
			MeshUtility.GetSubmeshVertexData(out points, vData);
			int[,] indices;
			MeshUtility.GetSubmeshIndexData(out indices, iData);
			for (int i = 0; i < indices.GetLength(0); ++i) {
				float t2, u, v;
				if (MVMathUtil.RayIntersectTriangle(origin, direction,
													points[indices[i, 0]],
													points[indices[i, 1]],
													points[indices[i, 2]],
													out t2, out u, out v)) {
					t = Math.Min(t, t2);
					intersect = true;
				}
			}
			return intersect;
		}
#endif

        // Login settings
        // Default settings for login settings
        public string MasterServer
        {
            set
            {
                loginSettings.rdpServer = value;
                loginSettings.tcpServer = value;
            }
        }

        public string WorldId
        {
            get
            {
                return loginSettings.worldId;
            }
            set
            {
                loginSettings.worldId = value;
            }
        }

        public long CharacterId
        {
            get
            {
                return loginSettings.characterId;
            }
            set
            {
                loginSettings.characterId = value;
            }
        }

        public string LoginUrl
        {
            set
            {
                loginSettings.loginUrl = value;
            }
        }

        /// <summary>
        ///   Override for the world specific patcher url
        /// </summary>
        public string WorldPatcherUrl
        {
            set
            {
                worldPatcherUrl = value;
            }
            get
            {
                return worldPatcherUrl;
            }
        }

        /// <summary>
        ///   Override for the world specific asset url
        /// </summary>
        public string WorldUpdateUrl
        {
            set
            {
                worldUpdateUrl = value;
            }
            get
            {
                return worldUpdateUrl;
            }
        }

        public NetworkHelper NetworkHelper
        {
            get { return networkHelper; }
        }

        public WorldManager WorldManager
        {
            get { return worldManager; }
        }

        public Window XmlUiWindow
        {
            get { return xmlUiWindow; }
        }

        public Camera Camera
        {
            get
            {
                return camera;
            }
        }

        public Player Player
        {
            get
            {
                return worldManager.Player;
            }
        }

        public long PlayerId
        {
            get
            {
                return worldManager.PlayerId;
            }
        }

        public RenderWindow Window
        {
            get
            {
                return window;
            }
        }

        public Viewport Viewport
        {
            get
            {
                return viewport;
            }
        }

        public string Version
        {
            get
            {
                if (overrideVersion != null)
                    return overrideVersion;
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                return version.ToString();
            }
        }

        public IGameWorld GameWorld
        {
            set
            {
                gameWorld = value;
                gameWorld.WorldManager = worldManager;
            }
            get
            {
                return gameWorld;
            }
        }

        public IInputHandler InputHandler
        {
            get
            {
                return inputHandler;
            }
            set
            {
                if (inputHandler != null && inputHandler != value)
                    inputHandler.Detach();
                inputHandler = value;
            }
        }

        public bool GuiHasFocus
        {
            get
            {
                Window mouseWindow = GuiSystem.Instance.WindowContainingMouse;
                return (mouseWindow != rootWindow) &&
                       (mouseWindow != xmlUiWindow) &&
                       (mouseWindow != guiWindow);
            }
        }

        public bool PatchMedia
        {
            get
            {
                return patchMedia;
            }
            set
            {
                patchMedia = value;
            }
        }

        public bool ExitAfterMediaPatch
        {
            get
            {
                return exitAfterMediaPatch;
            }
            set
            {
                exitAfterMediaPatch = value;
            }
        }

        //public string AssetConfig {
        //    get {
        //        return assetConfig;
        //    }
        //    set {
        //        assetConfig = value;
        //    }
        //}

        /// <summary>
        ///   This property determines whether we use a different repository
        ///   than we normally would for the world.  If it is set, we are
        ///   either using the repository from the registry, or we have passed
        ///   in a repository path on the command line
        /// </summary>
        public bool UseRepository
        {
            get
            {
                return useRepository;
            }
            set
            {
                useRepository = value;
            }
        }

        public string RepositoryPath
        {
            get
            {
                if (repositoryPaths.Count > 0)
                    return repositoryPaths[0];
                return null;
            }
            set
            {
                repositoryPaths = new List<string>();
                repositoryPaths.Add(value);
            }
        }

        public List<string> RepositoryPaths
        {
            get { return repositoryPaths; }
            set
            {
                repositoryPaths = value;
            }
        }

        public int FramesBetweenSleeps
        {
            get
            {
                return framesBetweenSleeps;
            }
            set
            {
                framesBetweenSleeps = value;
            }
        }

        public WorldServerEntry LoopbackWorldServerEntry
        {
            get
            {
                return loopbackWorldServerEntry;
            }
            set
            {
                loopbackWorldServerEntry = value;
            }
        }

        public byte[] LoopbackIdToken
        {
            get
            {
                return loopbackIdToken;
            }
            set
            {
                loopbackIdToken = value;
            }
        }

        public byte[] LoopbackOldToken
        {
            get
            {
                return loopbackOldToken;
            }
            set
            {
                loopbackOldToken = value;
            }
        }

        public Window RootWindow
        {
            get
            {
                return rootWindow;
            }
        }

        public int MaxFPS
        {
            get
            {
                return maxFPS;
            }
            set
            {
                maxFPS = value;
            }
        }

        public List<CharacterEntry> CharacterEntries
        {
            get
            {
                return networkHelper.CharacterEntries;
            }
        }

        protected bool logTerrainConfig = false;

        public bool LogTerrainConfig
        {
            get
            {
                return logTerrainConfig;
            }
            set
            {
                logTerrainConfig = value;
            }
        }

        public int LastFrameRenderCalls
        {
            get
            {
                return lastFrameRenderCalls;
            }
        }

        public float MaxRecentFrameTime
        {
            get
            {
                return maxRecentFrameTime;
            }
        }

        public bool LoadingState
        {
            get
            {
                return loadWindow.Visible;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
        }

        public string ErrorPage
        {
            get
            {
                return errorPage;
            }
        }

        public UiSystem UiSystem
        {
            get
            {
                return xmlUi;
            }
        }

        public bool UseCooperativeInput
        {
            get
            {
                return useCooperativeInput;
            }
            set
            {
                useCooperativeInput = value;
            }
        }

        public bool UseTCP
        {
            get
            {
                return useTCP;
            }
            set
            {
                useTCP = value;
            }
        }

        /// <summary>
        ///   Setting this value to non-zero will cause the client to ignore the actual time
        ///     between frames and instead compute time between frames based on the specified
        ///     FPS.  This allows a non-realtime rendering mode that can be used for creating
        ///     videos.
        /// </summary>
        public int FixedFPS
        {
            get
            {
                return fixedFPS;
            }
            set
            {
                fixedFPS = value;
                if (engine != null)
                {
                    engine.FixedFPS = value;
                }
                Multiverse.Utility.TimeTool.TimeOverride = (long)frameCounter * 1000L / fixedFPS;
            }
        }

        public int FrameCounter
        {
            get
            {
                return frameCounter;
            }
        }

        public long TickCount
        {
            get
            {
                long tickCount;

                if (fixedFPS == 0)
                {
                    tickCount = TimeTool.CurrentTime;
                }
                else
                {
                    // make a synthetic tick count based on the fixed frame rate
                    tickCount = frameCounter * 1000 / fixedFPS;
                }

                return tickCount;
            }
        }

        public static Client Instance
        {
            get
            {
                return instance;
            }
        }

        public static string ScreenshotPath
        {
            get
            {
                return ScreenshotsFolder;
            }
        }

        public static int NumExistingScreenshots
        {
            get
            {
                return Directory.GetFiles(ScreenshotsFolder, "screenshot*.*").Length;
            }
        }

        ///<summary>
        ///    Set to true in order to display the loading window,
        ///    set to false to stop displaying the loading window.
        ///</summary>
        public bool LoadWindowVisible
        {
            get
            {
                return loadWindow.Visible;
            }
            set
            {
                log.DebugFormat("Client.LoadWindowVisible: Setting loadWindow.Visible to {0}", value);
                loadWindow.Visible = value;
            }
        }

        /// <summary>
        ///    Set by ClientInit.py to the minimum screen width
        ///    allowed by the world.
        /// </summary>
        public int MinScreenWidth
        {
            get
            {
                return minScreenWidth;
            }
            set
            {
                minScreenWidth = value;
            }
        }

        /// <summary>
        ///    Set by ClientInit.py to the minimum screen width
        ///    allowed by the world.
        /// </summary>
        public int MinScreenHeight
        {
            get
            {
                return minScreenHeight;
            }
            set
            {
                minScreenHeight = value;
            }
        }

        /// <summary>
        ///    Set by ClientInit.py to false if the world does not
        ///    allow full-screen mode.
        /// </summary>
        public bool AllowFullScreen
        {
            get
            {
                return allowFullScreen;
            }
            set
            {
                allowFullScreen = value;
            }
        }

        /// <summary>
        ///    Set to true by command-line arg --allow_resize
        /// </summary>
        public bool AllowResize
        {
            get
            {
                return allowResize;
            }
            set
            {
                allowResize = value;
            }
        }

        ///<summary>
        ///    Set to true, the default value, when adjusting the list
        ///    of renderables in response to new and free messages;
        ///    set to false by an OnLoadingStateChange handler.
        ///</summary>
        public bool UpdateRenderTargets
        {
            get
            {
                return !engine.DontUpdateRenderTargets;
            }
            set
            {
                log.DebugFormat("Client.UpdateRenderTargets: Setting engine.DontUpdateRenderTargets to {0}", !value);
                engine.DontUpdateRenderTargets = !value;
            }
        }

        public static bool ServerRelease1_5
        {
            get
            {
                return serverRelease1_5;
            }
        }

        ///<summary>
        ///    Getter/Setter for data member that determines if collision
        ///    volumes instead of bounding boxes are used to determine distances
        ///    to objects.
        ///</summary>
        public bool TargetBasedOnCollisionVolumes
        {
            get
            {
                return targetBasedOnCollisionVolumes;
            }
            set
            {
                targetBasedOnCollisionVolumes = value;
            }
        }
    }
}
