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
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Timers;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Configuration;
using Multiverse.AssetRepository;
using Multiverse.Serialization;
using Multiverse.CollisionLib;
using Microsoft.Win32;
using Multiverse.ToolBox;
using log4net;

namespace Multiverse.Tools.WorldEditor
{
    public partial class WorldEditor : Form
    {
        protected readonly float oneMeter = 1000.0f;

        protected static WorldEditor instance = null;

        protected WorldEditorConfig config;

        protected Root engine;
        protected Camera camera;
        protected Viewport viewport;
        protected SceneManager scene;
        protected RenderWindow window;

        protected bool quit = false;

        protected float time = 0;
        protected bool displayWireFrame = false;
        protected bool displayTerrain = true;
        protected bool displayOcean = true;
        protected bool renderLeaves = true;
        protected bool displayBoundaryMarkers = true;
        protected bool displayRoadMarkers = true;
        protected bool displayMarkerPoints = true;
        protected bool disableAllMarkers = false;
        protected bool displayParticleEffects = true;
        protected bool displayTerrainDecals = true;
        protected bool displayPointLightMarker = true;
        protected bool displayShadows = false;
        protected bool displayPointLightCircles = false;
        protected bool disableVideoPlayback = false;
        protected bool lockCameraToObject = false;
        protected float cameraNearDistance = 1f;

        Axiom.SceneManagers.Multiverse.ITerrainGenerator terrainGenerator;

        protected WorldRoot worldRoot = null;
        protected String mapFilename;

        // camera movement related variables
        protected float camVelocity = 0f;
        protected float camAccel;
        protected float defaultCamAccelSpeed;
        protected Vector3 camDirection = Vector3.Zero;
        protected float camSpeed;
        protected bool cameraTerrainFollow = false;
        protected bool cameraAboveTerrain = true;
        protected SetCameraNearDialog setCameraNearDialog = null;
        protected float cameraAzimuth = 0f;
        protected float cameraZenith = 10f;
        protected float cameraRadius;
        protected float scaleMoveMultiplier = 1000f;
        protected float cameraAccelerateIncrement;
        protected bool orientationUpdate = false;


        // mouse motion related variables
        protected bool leftMouseClick = false;
        protected bool rightMouseClick = false;
        protected bool middleMouseClick = false;
        protected bool leftMouseRelease = false;
        protected bool rightMouseRelease = false;
        protected bool middleMouseRelease = false;
        protected System.Windows.Forms.Timer mouseDownTimer;
        protected System.Windows.Forms.Timer positionPanelButtonMouseDownTimer;
        protected Vector2 mouseDownPosition;
        protected bool mouseDragEvent = false;
        protected List<IWorldObject> mouseDragObject = null;
        protected Vector3 mouseDownObjectPosition;
        protected IWorldObject hitObject = null;
        protected bool mouseSelectObject = false;
        protected bool mouseSelectMultipleObject = false;
        protected bool mouseTurnCamera = false;
        protected MouseButtons mouseSelectButton;
        protected Keys mouseSelectModifier;

        protected int lastMouseX = 0;
        protected int lastMouseY = 0;

        protected int newMouseX = 0;
        protected int newMouseY = 0;
        protected int mouseWheelDelta = 0;

        protected float mouseWheelMultiplier;
        protected float presetMWM1;
        protected float presetMWM2;
        protected float presetMWM3;
        protected float presetMWM4;
        protected float cameraUpDownMultiplier = 100f;

        protected float mouseRotationScale = 1.0f;

        protected bool warpCamera = false;
        protected bool cameraLookDownSouth = false;
        protected bool takeScreenShot = false;
        protected bool moveCameraUp = false;
        protected bool moveCameraDown = false;
        protected bool moveForward = false;
        protected bool moveBack = false;
        protected bool moveLeft = false;
        protected bool moveRight = false;
        protected bool moveOnPlane = false;
        protected bool turnCameraLeft = false;
        protected bool turnCameraRight = false;
        protected bool turnCameraUp = false;
        protected bool turnCameraDown = false;
        protected bool accelerateCamera;
        protected float cameraTurnIncrement;
        protected float camSpeedIncrement;
        protected float camAccelIncrement;
        protected float presetCameraSpeed1;
        protected float presetCameraSpeed2;
        protected float presetCameraSpeed3;
        protected float presetCameraSpeed4;
        protected float presetCameraAccel1;
        protected float presetCameraAccel2;
        protected float presetCameraAccel3;
        protected float presetCameraAccel4;
        protected System.Windows.Forms.Timer incrementSpeedTimer = new System.Windows.Forms.Timer();
        protected System.Windows.Forms.Timer incrementAccelTimer = new System.Windows.Forms.Timer();
        protected int activeFps;

        protected UserCommandMapping commandMapping = null;

        /// <summary>
        /// interal variables used for grabbing mouse events in the 3d window
        /// </summary>
        protected bool mouseIntercepted = false;
        protected MouseMoveIntercepter mouseMoveIntercepter = null;
        protected MouseButtonIntercepter mouseUpIntercepter = null;
        protected MouseButtonIntercepter mouseDownIntercepter = null;
        protected MouseCaptureLost mouseCaptureLost = null;

        protected AssetCollection assets;

        protected NameValueTemplateCollection templates;

        protected Random random;

        protected XmlWriterSettings xmlWriterSettings;
        protected XmlReaderSettings xmlReaderSettings;

        protected UndoRedo undoRedo;

        protected float movementScale = 100f;
        protected float scalePercentage = 1.1f;
        protected static string helpURL;
        protected static string feedbackURL;
        protected static string releaseNoteURL;

        protected AxiomControl axiomControl;

        protected GlobalFog globalFog;
        protected bool displayFog = true;
        protected GlobalAmbientLight globalAmbientLight;
        protected GlobalDirectionalLight globalDirectionalLight;
        protected bool displayLights = true;
        protected String activeDirectionalLightName = "";
        protected CollisionAPI collisionManager = new CollisionAPI(false);

        protected long timerFreq;
        protected long lastFrameTime;

        protected ShowPathDialog showPathDialog;

        protected bool logPathGeneration = false;

        protected List<Boundary> boundaryList = new List<Boundary>();

        protected List<string> messageList = new List<string>();

        protected List<string> missingAssetList = new List<string>();

        protected Dictionary<string, List<CollisionShape>> meshCollisionShapes =
            new Dictionary<string, List<CollisionShape>>();

        protected uint autoSaveTime = 1800000;
        protected bool autoSaveEnabled = true;

        protected System.Windows.Forms.Timer saveTimer = new System.Windows.Forms.Timer();

        protected object autoSaveHead = null;

        protected ClipboardObject clipboard = new ClipboardObject();
        protected List<string> args;
        protected ToolStripStatusLabel fpsStatusValueLabel = new ToolStripStatusLabel("FPS:");
        protected ToolStripStatusLabel mouseGroundCoorPanel = new ToolStripStatusLabel();
        protected ToolStripStatusLabel activeBoundaryListToolStripStatusLabel = new ToolStripStatusLabel();
        protected ToolStripStatusLabel cameraSpeedStatusLabel = new ToolStripStatusLabel("Camera Speed:");
        protected ToolStripStatusLabel cameraStatusLabel = new ToolStripStatusLabel("Camera Status:");
        protected ToolStripStatusLabel cameraAccelRateLabel = new ToolStripStatusLabel("Camera Acceratation Rate:");
        protected DesignateRepositoriesDialog designateRepositoriesDialog = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("InitializationError");

        protected void InitAxiomControl()
        {
            axiomControl = new AxiomControl();

            // 
            // axiomControl
            // 
            this.axiomControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axiomControl.BackColor = Color.Gray;
            this.axiomControl.Location = new System.Drawing.Point(0, 0);
            this.axiomControl.Name = "axiomControl";
            this.axiomControl.Size = new System.Drawing.Size(734, 656);
            this.axiomControl.TabIndex = 2;
            this.axiomControl.TabStop = true;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.axiomControl);


        }

        protected Axiom.SceneManagers.Multiverse.ITerrainGenerator DefaultTerrainGenerator()
        {
            Multiverse.Generator.FractalTerrainGenerator gen = new Multiverse.Generator.FractalTerrainGenerator();
            gen.HeightFloor = 20;
            gen.HeightScale = 0;

            return gen;
        }

        protected DialogResult ErrorLogPopup(List<string> log, string message, string title, MessageBoxButtons buttons)
        {
            if (log.Count > 0)
            {
                string lines = message;
                foreach (string s in log)
                {
                    lines += s + "\n";
                }
                return MessageBox.Show(lines, title, buttons, MessageBoxIcon.Error);
            }

            return DialogResult.OK;
        }

        protected void CheckLogAndMaybeExit(List<string> log)
        {
            if (ErrorLogPopup(log, "Error(s) initializing asset repository:\n\n",
                "Errors Initializing Asset Repository.  Click Cancel To Exit",
                MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                Environment.Exit(-1);
            }
        }

        public WorldEditor()
        {
            undoRedo = new UndoRedo();
            InitializeComponent();
            config = new WorldEditorConfig();
            helpURL = config.HelpBaseURL;
            feedbackURL = config.FeedbackBaseURL;
            releaseNoteURL = config.ReleaseNotesURL;

            // create the default terrain generator
            terrainGenerator = DefaultTerrainGenerator();
            RepositoryClass.Instance.InitializeRepositoryPath();
            designateRepositoriesDialog = new DesignateRepositoriesDialog();
            List<string> validityLog = RepositoryClass.Instance.CheckForValidRepository();
            if (validityLog.Count != 0)
            {
                using (SetAssetRepositoryDialog dlg = new SetAssetRepositoryDialog(this))
                {
                    DialogResult result;

                    result = dlg.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }
            }


            List<string> log = RepositoryClass.Instance.InitializeRepository();
            CheckLogAndMaybeExit(log);

            InitAxiomControl();
            instance = this;
            DisplayObject.collisionManager = collisionManager;
            assets = new AssetCollection();
            AssetListConverter.assetCollection = assets;
            SpeedWindFileListUITypeEditor.assetCollection = assets;
            TreeDescriptionFilenameUITypeEditor.assetCollection = assets;
            ImageNameUITypeEditor.assetCollection = assets;
            ParticleEffectNameUITypeEditor.assetCollection = assets;

            // Read in and set user preferences

            object ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayFog", (object)displayFog);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayFog = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayLight", (object)displayLights);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayLights = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayRegionMarkers", (object)true);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayBoundaryMarkers = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayRoadMarkers", (object)true);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayRoadMarkers = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayMarkerPoints", (object)true);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayMarkerPoints = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayPointLightMarkers", (object)true);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayPointLightMarker = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "Disable All Markers", (object)false);
            if (ret == null || String.Equals(ret.ToString(), "False"))
            {
                ret = (object)false;
            }
            else
            {
                ret = (object)true;
            }
            disableAllMarkers = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "Display Terrain Decals", (object)true);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            displayTerrainDecals = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "LockCameraToObject", (object)false);
            if (ret == null || String.Equals(ret.ToString(), "False"))
            {
                ret = (object)false;
            }
            else
            {
                ret = (object)true;
            }
            lockCameraToObject = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraFollowsTerrain", (object)false);
            if (ret == null || String.Equals(ret.ToString(), "False"))
            {
                ret = (object)false;
            }
            else
            {
                ret = (object)true;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraStaysAboveTerrain", (object)true);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            cameraAboveTerrain = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayOcean", (object)DisplayOcean);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            DisplayOcean = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayShadows", (object)false);
            if (ret == null || String.Equals(ret.ToString(), "False"))
            {
                ret = (object)false;
            }
            else
            {
                ret = (object)true;
            }
            DisplayShadows = (bool)ret;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraNearDistance", (object)1f);
            if (ret == null)
            {
                ret = (object)1f;
            }
            CameraNearDistance = float.Parse(ret.ToString());
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "AutoSaveEnable", (object)autoSaveEnabled);
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            autoSaveEnabled = (bool)ret;
            if (ret == null || String.Equals(ret.ToString(), "True"))
            {
                ret = (object)true;
            }
            else
            {
                ret = (object)false;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "AutoSaveTime", (object)autoSaveTime);
            if (ret == null)
            {
                autoSaveTime = 30 * 60 * 1000;
            }
            else
            {
                autoSaveTime = uint.Parse(ret.ToString());
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraDefaultSpeed", (object)(Config.DefaultCamSpeed));
            if (ret != null)
            {
                camSpeed = float.Parse(ret.ToString());
            }
            else
            {
                camSpeed = Config.DefaultCamSpeed;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraSpeedIncrement", (object)(Config.DefaultCamSpeedIncrement));
            if (ret != null)
            {
                camSpeedIncrement = float.Parse(ret.ToString());
            }
            else
            {
                camSpeedIncrement = Config.DefaultCamSpeedIncrement;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed1", (object)(Config.DefaultPresetCamSpeed1));
            if (ret != null)
            {
                presetCameraSpeed1 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraSpeed1 = Config.DefaultPresetCamSpeed1;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed2", (object)(Config.DefaultPresetCamSpeed2));
            if (ret != null)
            {
                presetCameraSpeed2 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraSpeed2 = config.DefaultPresetCamSpeed2;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed3", (object)(Config.DefaultPresetCamSpeed3));
            if (ret != null)
            {
                presetCameraSpeed3 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraSpeed3 = Config.DefaultPresetCamSpeed3;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed4", (object)(Config.DefaultPresetCamSpeed4));
            if (ret != null)
            {
                presetCameraSpeed4 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraSpeed4 = Config.DefaultPresetCamSpeed4;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerate", (object)Config.DefaultAccelerateCamera);
            if (ret != null && String.Equals(ret.ToString(), "False"))
            {
                accelerateCamera = false;
            }
            else
            {
                accelerateCamera = true;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerationRate", (object)(Config.DefaultCamAccelRate));
            if (ret != null)
            {
                defaultCamAccelSpeed = float.Parse(ret.ToString());
            }
            else
            {
                defaultCamAccelSpeed = Config.DefaultCamAccelRate;
            }
            camAccel = defaultCamAccelSpeed;
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerationIncrement", (object)(Config.DefaultCamAccelRateIncrement));
            if (ret != null)
            {
                camAccelIncrement = float.Parse(ret.ToString());
            }
            else
            {
                camAccelIncrement = Config.DefaultCamAccelRateIncrement;
            }


            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate1", (object)(Config.DefaultPresetCamAccel1));
            if (ret != null)
            {
                presetCameraAccel1 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraAccel1 = Config.DefaultPresetCamAccel1;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate2", (object)(Config.DefaultPresetCamAccel2));
            if (ret != null)
            {
                presetCameraAccel2 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraAccel2 = Config.DefaultPresetCamAccel2;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate3", (object)(Config.DefaultPresetCamAccel3));
            if (ret != null)
            {
                presetCameraAccel3 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraAccel3 = Config.DefaultPresetCamAccel3;
            }
            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate4", (object)(Config.DefaultPresetCamAccel4));
            if (ret != null)
            {
                presetCameraAccel4 = float.Parse(ret.ToString());
            }
            else
            {
                presetCameraAccel4 = Config.DefaultPresetCamAccel4;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraTurnRate", (object)Config.DefaultCameraTurnRate);
            if (ret != null)
            {
                cameraTurnIncrement = float.Parse(ret.ToString());
            }
            else
            {
                cameraTurnIncrement = Config.DefaultCameraTurnRate;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MouseWheelMultiplier", (object)Config.DefaultMouseWheelMultiplier);
            if (ret != null)
            {
                mouseWheelMultiplier = float.Parse(ret.ToString());
            }
            else
            {
                mouseWheelMultiplier = Config.DefaultMouseWheelMultiplier;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset1", (object)Config.DefaultPresetMWM1);
            if (ret != null)
            {
                presetMWM1 = float.Parse(ret.ToString());
            }
            else
            {
                presetMWM1 = Config.DefaultPresetMWM1;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset2", (object)Config.DefaultPresetMWM2);
            if (ret != null)
            {
                presetMWM2 = float.Parse(ret.ToString());
            }
            else
            {
                presetMWM2 = Config.DefaultPresetMWM2;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset3", (object)Config.DefaultPresetMWM3);
            if (ret != null)
            {
                presetMWM3 = float.Parse(ret.ToString());
            }
            else
            {
                presetMWM3 = Config.DefaultPresetMWM3;
            }

            ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset4", (object)Config.DefaultPresetMWM4);
            if (ret != null)
            {
                presetMWM4 = float.Parse(ret.ToString());
            }
            else
            {
                presetMWM4 = Config.DefaultPresetMWM4;
            }


            setToolStripMWMDropDownMenu();

            setToolStripAccelSpeedDropDownMenu();

            random = new Random();

            templates = new NameValueTemplateCollection("./NameValueTemplates");
            NameValueUITypeEditor.nvt = templates;

            xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;

            xmlReaderSettings = new XmlReaderSettings();



            createKeybindings();

            StatusBarAddItem(fpsStatusValueLabel);
            StatusBarAddItem(mouseGroundCoorPanel);
            StatusBarAddItem(cameraSpeedStatusLabel);
            StatusBarAddItem(cameraStatusLabel);
            activeBoundaryListToolStripStatusLabel.Name = "activeBoundaryList";

            RepositoryClass.Instance.InitializeRepositoryPath();

            timerFreq = Stopwatch.Frequency;

            lastFrameTime = Stopwatch.GetTimestamp();

            showPathDialog = new ShowPathDialog();
        }


        private void setShortCuts()
        {
            string[] shortcutsArray = {"worldEditorCut", "worldEditorCopy", "worldEditorPaste", "worldEditorUndo", "worldEditorRedo",
                "worldEditorNewWorld", "worldEditorLoadWorld", "worldEditorExit", "worldEditorSaveWorld", "worldEditorEditPreferences",
                "worldEditorTreeViewSearch", "worldEditorControlMappingEditor", "axiomDeleteObject"};
            foreach (string evstring in shortcutsArray)
            {
                UserCommand command = commandMapping.GetCommandForEvent(evstring);
                ToolStripMenuItem menuItem = null;
                if (command != null)
                {
                    switch (evstring)
                    {
                        case "worldEditorCut":
                        case "axiomCut":
                            menuItem = cutToolStripMenuItem;
                            break;
                        case "worldEditorCopy":
                        case "axiomCopy":
                            menuItem = copyToolStripMenuItem;
                            break;
                        case "worldEditorPaste":
                        case "axiomPase":
                            menuItem = pasteToolStripMenuItem;
                            break;
                        case "worldEditorUndo":
                            menuItem = undoToolStripMenuItem;
                            break;
                        case "worldEditorRedo":
                            menuItem = redoToolStripMenuItem;
                            break;
                        case "worldEditorNewWorld":
                            menuItem = newWorldToolStripMenuItem;
                            break;
                        case "worldEditorLoadWorld":
                            menuItem = loadWorldToolStripMenuItem;
                            break;
                        case "worldEditorExit":
                            menuItem = exitToolStripMenuItem;
                            break;
                        case "worldEditorSaveWorld":
                            menuItem = saveWorldToolStripMenuItem;
                            break;
                        case "worldEditorEditPreferences":
                            menuItem = preferencesToolStripMenuItem;
                            break;
                        case "worldEditorTreeViewSearch":
                            menuItem = treeViewSearchMenuItem;
                            break;
                        case "worldEditorControlMappingEditor":
                            menuItem = controlMappingEditorToolStripMenuItem;
                            break;
                        case "worldEditorToggleLockCameraToObject":
                            menuItem = lockCameraToSelectedObjectToolStipMenuItem;
                            break;
                        case "axiomDeleteObject":
                            menuItem = deleteToolStripMenuItem;
                            break;

                    }
                    if (String.Equals(command.Modifier, "none"))
                    {
                        menuItem.ShortcutKeyDisplayString = command.Key;
                    }
                    else
                    {
                        menuItem.ShortcutKeyDisplayString = String.Format("{0}+{1}", command.Modifier, command.Key);
                    }
                }

            }
        }

        public void createKeybindings()
        {
            // int i = 0; (unused)
            List<EventObject> evlist = new List<EventObject>();
            if (File.Exists(config.CommandBindingEventsFilePath))
            {
                try
                {

                    XmlReader r = XmlReader.Create(Config.CommandBindingEventsFilePath, xmlReaderSettings);
                    while (r.Read())
                    {
                        if (r.NodeType == XmlNodeType.Whitespace)
                        {
                            continue;
                        }
                        if (r.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            continue;
                        }
                        if (r.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                        switch (r.Name)
                        {
                            case "EventObjects":
                                while (r.Read())
                                {
                                    if (r.NodeType == XmlNodeType.Whitespace)
                                    {
                                        continue;
                                    }
                                    if (r.NodeType == XmlNodeType.EndElement)
                                    {
                                        break;
                                    }
                                    switch (r.Name)
                                    {
                                        case "Event":
                                            evlist.Add(new EventObject(r));
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    string filePath = String.Format("{0}\\{1}", Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")), this.Config.CommandBindingEventsFilePath.Substring(this.Config.CommandBindingEventsFilePath.IndexOf(".") + 2, this.Config.CommandBindingEventsFilePath.Length - this.Config.CommandBindingEventsFilePath.IndexOf(".") - 2));
                    log.ErrorFormat("Exception Reading CommandEvents.xml at {1}: {0}", e, filePath);
                    MessageBox.Show(String.Format("The CommandEvents.xml file could not be parsed at {0}.  The World Editor can not run properly without this file.  The World Editor will exit now.", filePath), "Can not parse Key Bindings file", MessageBoxButtons.OK);
                    exitToolStripMenuItem_Click(this, new EventArgs());
                    log.Error("World Editor Exiting now");
                    return;
                }
                foreach (EventObject obj in evlist)
                {
                    bool mouseEvent = false;
                    EventHandler hand = null;
                    switch (obj.EvString)
                    {
                        case "worldEditorToggleLockCameraToObject":
                            hand = lockCameraToSelectedObjectToolStipMenuItem_Click;
                            break;
                        case "axiomToggleMoveCameraOnPlane":
                            hand = this.axiomToggleMoveCameraOnPlane;
                            break;
                        case "axiomOnCameraOnPlane":
                            hand = this.axiomOnCameraOnPlane;
                            break;
                        case "axiomOffCameraOnPlane":
                            hand = this.axiomOffCameraOnPlane;
                            break;
                        case "axiomCameraMoveSpeedUp":
                            hand = this.axiomCameraMoveSpeedUp;
                            break;
                        case "axiomCameraMoveSpeedDown":
                            hand = this.axiomCameraMoveSpeedDown;
                            break;
                        case "axiomToggleAccelerate":
                            hand = this.axiomToggleAccelerate;
                            break;
                        case "axiomCameraAccelerateUp":
                            hand = this.axiomCameraAccelerateUp;
                            break;
                        case "axiomCameraAccelerateDown":
                            hand = this.axiomCameraAccelerateDown;
                            break;
                        case "worldEditorMoveCameraForward":
                        case "axiomMoveCameraForward":
                            hand = this.axiomMoveCameraForward;
                            break;
                        case "axiomMoveCameraForwardStop":
                        case "worldEditorMoveCameraForwardStop":
                            hand = this.axiomMoveCameraForwardStop;
                            break;
                        case "axiomMoveCameraBack":
                        case "worldEditorMoveCameraBack":
                            hand = this.axiomMoveCameraBack;
                            break;
                        case "axiomMoveCameraBackStop":
                        case "worldEditorMoveCameraBackStop":
                            hand = this.axiomMoveCameraBackStop;
                            break;
                        case "axiomMoveCameraLeft":
                        case "worldEditorMoveCameraLeft":
                            hand = this.axiomMoveCameraLeft;
                            break;
                        case "axiomMoveCameraLeftStop":
                        case "worldEditorMoveCameraLeftStop":
                            hand = this.axiomMoveCameraLeftStop;
                            break;
                        case "axiomMoveCameraRight":
                        case "worldEditorMoveCameraRight":
                            hand = this.axiomMoveCameraRight;
                            break;
                        case "axiomMoveCameraRightStop":
                        case "worldEditorMoveCameraRightStop":
                            hand = this.axiomMoveCameraRightStop;
                            break;
                        case "worldEditorMoveCameraUp":
                            hand = this.worldEditorMoveCameraUp;
                            break;
                        case "worldEditorMoveCameraUpStop":
                            hand = this.worldEditorMoveCameraUpStop;
                            break;
                        case "worldEditorMoveCameraDown":
                            hand = this.worldEditorMoveCameraDown;
                            break;
                        case "worldEditorMoveCameraDownStop":
                            hand = this.worldEditorMoveCameraDownStop;
                            break;
                        case "axiomMoveCameraStop":
                            hand = this.axiomMoveCameraStop;
                            break;
                        case "axiomTurnCameraLeft":
                            hand = this.axiomTurnCameraLeft;
                            break;
                        case "axiomTurnCameraLeftStop":
                            hand = this.axiomTurnCameraLeftStop;
                            break;
                        case "axiomTurnCameraRight":
                            hand = this.axiomTurnCameraRight;
                            break;
                        case "axiomTurnCameraRightStop":
                            hand = this.axiomTurnCameraRightStop;
                            break;
                        case "axiomTurnCameraDown":
                            hand = this.axiomTurnCameraDown;
                            break;
                        case "axiomTurnCameraDownStop":
                            hand = this.axiomTurnCameraDownStop;
                            break;
                        case "axiomTurnCameraUp":
                            hand = this.axiomTurnCameraUp;
                            break;
                        case "axiomTurnCameraUpStop":
                            hand = this.axiomTurnCameraUpStop;
                            break;
                        case "worldEditorCut":
                        case "axiomCut":
                        case "treeViewCut":
                            hand = this.worldEditorCutObjects;
                            break;
                        case "worldEditorCopy":
                        case "axiomCopy":
                        case "treeViewCopy":
                            hand = this.worldEditorCopyObjects;
                            break;
                        case "worldEditorPaste":
                        case "axiomPaste":
                        case "treeViewPaste":
                            hand = this.worldEditorPasteObjects;
                            break;
                        case "axiomDeleteObject":
                        case "treeViewDeleteObject":
                            hand = deleteToolStripMenuItem_Clicked;
                            break;
                        case "axiomMouseSelectObject":
                            hand = axiomMouseSelectObject;
                            mouseEvent = true;
                            break;
                        case "axiomMouseSelectMultipleObject":
                            hand = axiomMouseSelectMultipleObject;
                            mouseEvent = true;
                            break;
                        case "axiomMouseTurnCamera":
                            hand = axiomMouseTurnCamera;
                            mouseEvent = true;
                            break;
                        case "axiomEditorWarpCamera":
                            hand = axiomWarpCamera;
                            break;
                        case "axiomCameraLookDownSouth":
                            hand = axiomCameraLookDownSouth;
                            break;
                        case "axiomMoveCameraToSelectedMarker":
                            hand = axiomMoveCameraToSelectedMarker;
                            break;
                        case "worldEditorUndo":
                            hand = undoToolStripMenuItem_Click;
                            break;
                        case "worldEditorRedo":
                            hand = redoToolStripMenuItem_Click;
                            break;
                        case "worldEditorNewWorld":
                            hand = newWorldToolStripMenuItem_Click;
                            break;
                        case "worldEditorLoadWorld":
                            hand = loadWorldToolStripMenuItem_Click;
                            break;
                        case "worldEditorExit":
                            hand = exitToolStripMenuItem_Click;
                            break;
                        case "worldEditorSaveWorld":
                            hand = saveWorldToolStripMenuItem_Click;
                            break;
                        case "worldEditorEditPreferences":
                            hand = editMenuPreferencesItem_clicked;
                            break;
                        case "worldEditorTreeViewSearch":
                            hand = editMenuTreeViewSearchItem_clicked;
                            break;
                        case "worldEditorControlMappingEditor":
                            hand = editMenuControlMappingEditorItem_clicked;
                            break;
                        case "worldEditorTakeScreenShotOn":
                            hand = worldEditorTakeScreenShotOn;
                            break;
                        case "worldEditorTakeScreenShotOff":
                            hand = worldEditorTakeScreenShotOff;
                            break;
                        case "worldEditorDisplayPointLightCircles":
                            hand = displayPointLightAttenuationCirclesMenuItem_Click;
                            break;
                        case "treeViewMoveToNextNode":
                            hand = treeViewMoveToNextNode;
                            break;
                        case "treeViewMoveToPrevNode":
                            hand = treeViewMoveToPrevNode;
                            break;
                        case "treeViewExpandSelectedNode":
                            hand = treeViewExpandSelectedNode;
                            break;
                        case "treeViewExpandAllSelectedNode":
                            hand = treeViewExpandAllSelectedNode;
                            break;
                        case "treeViewCollapseSelectedNode":
                            hand = treeViewCollapseSelectedNode;
                            break;
                        case "axiomSetCameraSpeedPreset1":
                            hand = axiomSetSpeedPreset1;
                            break;
                        case "axiomSetCameraSpeedPreset2":
                            hand = axiomSetSpeedPreset2;
                            break;
                        case "axiomSetCameraSpeedPreset3":
                            hand = axiomSetSpeedPreset3;
                            break;
                        case "axiomSetCameraSpeedPreset4":
                            hand = axiomSetSpeedPreset4;
                            break;
                        case "axiomSetPresetMWM1":
                            hand = axiomSetPresetMWM1;
                            break;
                        case "axiomSetPresetMWM2":
                            hand = axiomSetPresetMWM2;
                            break;
                        case "axiomSetPresetMWM3":
                            hand = axiomSetPresetMWM3;
                            break;
                        case "axiomSetPresetMWM4":
                            hand = axiomSetPresetMWM4;
                            break;
                        case "axiomSetAccelPreset1":
                            hand = axiomSetAccelPreset1;
                            break;
                        case "axiomSetAccelPreset2":
                            hand = axiomSetAccelPreset2;
                            break;
                        case "axiomSetAccelPreset3":
                            hand = axiomSetAccelPreset3;
                            break;
                        case "axiomSetAccelPreset4":
                            hand = axiomSetAccelPreset4;
                            break;
                        case "axiomCameraMoveSpeedUpStop":
                            hand = axiomCameraMoveSpeedUpStop;
                            break;
                        case "axiomCameraMoveSpeedDownStop":
                            hand = axiomCameraMoveSpeedDownStop;
                            break;
                        case "axiomCameraAccelerateUpStop":
                            hand = axiomCameraAccelerateUpStop;
                            break;
                        case "axiomCameraAccelerateDownStop":
                            hand = axiomCameraAccelerateDownStop;
                            break;
                        default:
                            break;
                    }

                    if (mouseEvent)
                    {
                        obj.MouseButtonEvent = true;
                    }
                    obj.Handler = hand;
                }


                XmlReader xr;
                string keyBindingsFile = this.Config.AltCommandBindingsFilePath;
                if (!File.Exists(keyBindingsFile))
                {
                    keyBindingsFile = this.Config.CommandBindingsFilePath;
                    if (!File.Exists(keyBindingsFile))
                    {
                        string filePath1 = String.Format("{0}\\{1}", Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")), this.Config.AltCommandBindingsFilePath.Substring(this.Config.AltCommandBindingsFilePath.IndexOf(".") + 2, this.Config.AltCommandBindingsFilePath.Length - this.Config.AltCommandBindingsFilePath.IndexOf(".") - 2));
                        string filePath2 = String.Format("{0}\\{1}", Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")), this.Config.CommandBindingsFilePath.Substring(this.Config.CommandBindingsFilePath.IndexOf(".") + 2, this.Config.CommandBindingsFilePath.Length - this.Config.CommandBindingsFilePath.IndexOf(".") - 2));
                        log.ErrorFormat("Files MyBindings.xml and KeyBindings.xml could not be found at {0} and {1}", filePath1, filePath2);
                        MessageBox.Show(String.Format("The MyBindings.xml and  KeyBindings.xml file could not be found at {0} and {1}.  The World Editor can not run properly without at least one of these files.  The World Editor will exit now.", filePath1, filePath2), "Could not find MyBindings.xml and KeyBindings.xml", MessageBoxButtons.OK);
                        exitToolStripMenuItem_Click(this, new EventArgs());
                        log.Error("World Editor Exiting now");
                        return;
                    }
                }

                while (true)
                {
                    try
                    {
                        xr = XmlReader.Create(keyBindingsFile, xmlReaderSettings);

                        commandMapping = new UserCommandMapping(xr, evlist, Config.Context, Config.ExcludedKeys);
                        foreach (UserCommand command in commandMapping.Commands)
                        {
                            if (String.Equals(command.EvString, "axiomMouseSelectObject"))
                            {
                                mouseSelectModifier = command.ModifierCode;
                                switch (command.KeyCode)
                                {
                                    case Keys.LButton:
                                        mouseSelectButton = MouseButtons.Left;
                                        break;
                                    case Keys.RButton:
                                        mouseSelectButton = MouseButtons.Right;
                                        break;
                                    case Keys.MButton:
                                        mouseSelectButton = MouseButtons.Middle;
                                        break;
                                }
                            }
                        }
                        setShortCuts();
                    }
                    catch (Exception e)
                    {
                        if (String.Equals(keyBindingsFile, this.Config.CommandBindingEventsFilePath))
                        {
                            string filePath = String.Format("{0}\\{1}", Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")), this.Config.CommandBindingsFilePath.Substring(this.Config.CommandBindingsFilePath.IndexOf(".") + 2, this.Config.CommandBindingsFilePath.Length - this.Config.CommandBindingsFilePath.IndexOf(".") - 2));
                            log.ErrorFormat("Exception Reading KeyBindings.xml at {1}: {0}", e, filePath);
                            MessageBox.Show(String.Format("The KeyBindings.xml file could not be parsed at {0}.  The World Editor can not run properly without this file.  The World Editor will exit now.", filePath), "Can not parse Key Bindings file", MessageBoxButtons.OK);
                            exitToolStripMenuItem_Click(this, new EventArgs());
                            log.Error("World Editor Exiting now");
                            return;
                        }
                        else
                        {
                            string filePath = String.Format("{0}\\{1}", Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\")), this.Config.AltCommandBindingsFilePath.Substring(this.Config.AltCommandBindingsFilePath.IndexOf(".") + 2, this.Config.AltCommandBindingsFilePath.Length - this.Config.AltCommandBindingsFilePath.IndexOf(".") - 2));
                            log.ErrorFormat("Exception Reading MyBindings.xml at {1}: {0}", e, filePath);
                            MessageBox.Show(String.Format("The MyBindings.xml file could not be parsed at {0}.  The World Editor can not run properly without this file.  We will try to load the default command bindings from KeyBindings.xml file", filePath), "Can not parse Key Bindings file", MessageBoxButtons.OK);
                            keyBindingsFile = this.Config.CommandBindingsFilePath;
                            continue;
                        }
                    }
                    return;
                }
            }
            else
            {
                
                string dir = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                string filename = this.Config.CommandBindingEventsFilePath.Substring(this.Config.CommandBindingEventsFilePath.IndexOf(".") + 2, this.Config.CommandBindingEventsFilePath.Length - this.Config.CommandBindingEventsFilePath.IndexOf(".") - 2);
                string filePath = String.Format("{0}\\{1}", dir , filename);
                log.ErrorFormat("CommandEvents.xml was not found at {0}", filePath);
                MessageBox.Show(String.Format("The CommandEvents.xml file could not be found at {0}.  The World Editor can not run properly without this file.  The World Editor will exit now.", filePath) , "Could not find CommandEvents.xml", MessageBoxButtons.OK);
                exitToolStripMenuItem_Click(this, new EventArgs());
                log.Error("World Editor Exiting now");
                return;
            }
        }



        public NameValueTemplateCollection NameValueTemplates
        {
            get
            {
                return this.templates;
            }
        }

        public void UpdatePropertyGrid()
        {
            nodePropertyGrid.SelectedObject = nodePropertyGrid.SelectedObject;
        }

        public void ExecuteCommand(ICommand cmd)
        {
            if (cmd != null)
            {
                cmd.Execute();
                if (cmd.Undoable())
                {
                    undoRedo.PushCommand(cmd);
                }
            }
        }

        #region Axiom Initialization Code

        protected bool CheckShaderCaps()
        {
            string maxPSVersion = engine.RenderSystem.Caps.MaxFragmentProgramVersion;
            string maxVSVersion = engine.RenderSystem.Caps.MaxVertexProgramVersion;

            bool psOK;
            bool vsOK;

            switch (maxPSVersion)
            {
                case "ps_2_0":
                case "ps_2_x":
                case "ps_3_0":
                case "ps_3_x":
                    psOK = true;
                    break;
                default:
                    psOK = false;
                    break;
            }

            switch (maxVSVersion)
            {
                case "vs_2_0":
                case "vs_2_x":
                case "vs_3_0":
                    vsOK = true;
                    break;
                default:
                    vsOK = false;
                    break;
            }

            return vsOK && psOK;
        }

        protected bool Setup()
        {
            // get a reference to the engine singleton
            engine = new Root("", "trace.txt");
            // retrieve the max FPS, if it exists
            getMaxFPSFromRegistry();
            activeFps = Root.Instance.MaxFramesPerSecond;


            // add event handlers for frame events
            engine.FrameStarted += new FrameEvent(OnFrameStarted);
            engine.FrameEnded += new FrameEvent(OnFrameEnded);

            // allow for setting up resource gathering
            SetupResources();

            object ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "Disable Video Playback", (object)false);
            if (ret == null || String.Equals(ret.ToString(), "False"))
            {
                ret = (object)false;
            }
            else
            {
                ret = (object)true;
            }
            disableVideoPlayback = (bool)ret;

            //show the config dialog and collect options
            if (!ConfigureAxiom())
            {
                // shutting right back down
                engine.Shutdown();

                return false;
            }

            if (!CheckShaderCaps())
            {
                MessageBox.Show("Your graphics card does not support pixel shader 2.0 and vertex shader 2.0, which are required to run this tool.", "Graphics Card Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                engine.Shutdown();
                return false;
            }

            ChooseSceneManager();
            CreateCamera();
            CreateViewports();

            // set default mipmap level
            TextureManager.Instance.DefaultNumMipMaps = 5;

            // call the overridden CreateScene method
            CreateScene();

            // setup save timer
            saveTimer.Interval = (int)autoSaveTime;
            saveTimer.Tick += new EventHandler(saveTimerEvent);
            saveTimer.Start();

            InitializeAxiomControlCallbacks();

            return true;
        }

        protected void InitializeAxiomControlCallbacks()
        {
            this.axiomControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axiomControl_MouseDown);
            this.axiomControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.axiomControl_MouseMove);
            this.axiomControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.axiomControl_MouseWheel);
            this.axiomControl.MouseCaptureChanged += new System.EventHandler(this.axiomControl_MouseCaptureChanged);
            this.axiomControl.Resize += new System.EventHandler(this.axiomControl_Resize);
            this.axiomControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axiomControl_MouseUp);
            this.axiomControl.KeyDown += new KeyEventHandler(this.axiomControl_KeyDown);
            this.axiomControl.KeyUp += new KeyEventHandler(this.axiomControl_KeyUp);
            this.axiomControl.GotFocus += new EventHandler(axiomControl_GotFocus);
            this.axiomControl.LostFocus += new EventHandler(axiomControl_LostFocus);
        }

        protected void saveTimerEvent(object obj, EventArgs e)
        {
            if (undoRedo.AutoSaveDirty)
            {
                if (worldRoot.WorldFilePath != null && !String.Equals(worldRoot.WorldFilePath, ""))
                {
                    SaveWorld(worldRoot.WorldFilePath, true);
                    undoRedo.ResetAutoSaveDirty();
                }
            }
        }


        protected void SetupResources()
        {
            foreach (string s in RepositoryClass.AxiomDirectories)
            {
                List<string> repositoryDirectoryList = RepositoryClass.Instance.RepositoryDirectoryList;
                List<string> l = new List<string>();
                foreach (string repository in repositoryDirectoryList)
                    l.Add(Path.Combine(repository, s));
                ResourceManager.AddCommonArchive(l, "Folder");
            }
        }


        protected bool ConfigureAxiom()
        {
            // HACK: Temporary
            RenderSystem renderSystem = Root.Instance.RenderSystems[0];
            Root.Instance.RenderSystem = renderSystem;

            bool hasMovie = false;

            if (File.Exists(".\\Multiverse.Movie.dll") && !disableVideoPlayback)
            {
                hasMovie = true;
                renderSystem.SetConfigOption("Multi-Threaded", "Yes");
                PluginManager.Instance.LoadPlugins(".", "Multiverse.Movie.dll");
            }

            Root.Instance.Initialize(false);

            window = Root.Instance.CreateRenderWindow("Main Window", axiomControl.Width, axiomControl.Height, false,
                                                      "externalWindow", axiomControl, "useNVPerfHUD", true,
                                                      "multiThreaded", hasMovie);
            Root.Instance.Initialize(false);

            return true;
        }

        protected void ChooseSceneManager()
        {
            scene = Root.Instance.SceneManagers.GetSceneManager(SceneType.ExteriorClose);
        }

        protected void CreateCamera()
        {
            camera = scene.CreateCamera("PlayerCam");

            camera.Position = new Vector3(128 * oneMeter, 200 * oneMeter, 128 * oneMeter);
            camera.LookAt(new Vector3(0, 0, -300 * oneMeter));
            camera.Near = 1 * oneMeter;
            camera.Far = 10000 * oneMeter;

            camera.AspectRatio = (float)window.Width / window.Height;
        }

        protected virtual void CreateViewports()
        {
            Debug.Assert(window != null, "Attempting to use a null RenderWindow.");

            // create a new viewport and set it's background color
            viewport = window.AddViewport(camera, 0, 0, 1.0f, 1.0f, 100);
            viewport.BackgroundColor = ColorEx.Black;
        }
        #endregion Axiom Initialization Code

        #region Application Startup Code

        public bool Start(string[] args)
        {

            if (!Setup())
            {
                return false;
            }
            this.args = new List<string>();
            foreach (string arg in args)
            {
                this.args.Add(arg);
            }

            if (this.args.Count > 0 && File.Exists(args[0]))
            {
                LoadWorld(this.args[0]);
            }
            // start the engines rendering loop
            engine.StartRendering();


            return true;
        }


        #endregion Application Startup Code

        #region Scene Setup

        protected void GenerateScene()
        {
            ((Axiom.SceneManagers.Multiverse.SceneManager)scene).SetWorldParams(terrainGenerator, null);
            scene.LoadWorldGeometry("");

            scene.AmbientLight = Config.DefaultAmbientLightColor;

            if (!String.Equals(activeDirectionalLightName, ""))
            {
                scene.RemoveLight(activeDirectionalLightName);
                activeDirectionalLightName = "";
            }
            Light light = scene.CreateLight("MainLight");
            activeDirectionalLightName = light.Name;
            light.Type = LightType.Directional;
            Vector3 lightDir = Config.DefaultDirectionalLightDirection;
            lightDir.Normalize();
            light.Direction = lightDir;
            light.Diffuse = Config.DefaultGlobalDirectionalLightDiffuse;
            light.Specular = Config.DefaultGlobalDirectionalLightSpecular;
            return;
        }

        private void Regenerate()
        {
            GenerateScene();
        }

        protected void CreateScene()
        {

            viewport.BackgroundColor = ColorEx.White;
            viewport.OverlaysEnabled = false;

            GenerateScene();

            return;
        }

        #endregion Scene Setup

        #region Map Loading
        // load the map (terrain parameters) from the given file
        public void LoadMap(string filename)
        {
            if (filename != null)
            {
                // remember the filename
                mapFilename = filename;
                worldRoot.RemoveFromScene();
                worldRoot.Terrain.LoadTerrainFile(filename);
                Regenerate();
                worldRoot.UpdateScene(UpdateTypes.All, UpdateHint.TerrainUpdate);
                worldRoot.AddToScene();
            }
        }

        // read a saved map from the given filename
        protected void ReadSavedMap(string filename)
        {
            XmlReader r = XmlReader.Create(filename, XMLReaderSettings);

            ReadSavedMap(r);
            r.Close();

            return;
        }

        // read a saved map from the given XML stream
        protected void ReadSavedMap(XmlReader r)
        {
            // read until we find the start of the world description
            while (r.Read())
            {
                // look for the start of the terrain description
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "Terrain")
                    {

                        worldRoot.Terrain.LoadTerrain(r);
                        break;
                    }
                }
            }
        }

        public Axiom.SceneManagers.Multiverse.ITerrainGenerator TerrainGenerator
        {
            get
            {
                return terrainGenerator;
            }
            set
            {
                terrainGenerator = value;
                Regenerate();
            }
        }

        #endregion Map Loading

        #region World Loading

        public XmlReaderSettings XMLReaderSettings
        {
            get
            {
                return xmlReaderSettings;
            }
        }

        protected void ClearWorld()
        {
            SelectedObject = null;

            if (worldRoot != null)
            {
                worldRoot.RemoveFromScene();
                worldRoot.Dispose();
                worldRoot = null;
                worldTreeView.SelectedNodes.Clear();
                worldTreeView.Nodes.Clear();
                meshCollisionShapes.Clear();
                clearToolStrip1ContextIcons();
            }
        }

        private void InitializeResourcesPath(string path)
        {
            List<string> subdirs = new List<string>();
            foreach (string subdir in RepositoryClass.AxiomDirectories)
            {
                subdirs.Add(Path.Combine(path, subdir));
            }
            ResourceManager.AddCommonArchive(subdirs, "Folder");
        }

        protected void LoadWorld(String filename)
        {
            bool autoSaveLoad = false;
            string title;
            missingAssetList.Clear();
            FileInfo fileinfo = new FileInfo(filename);
            FileInfo autoSaveFileInfo = new FileInfo(filename.Insert(filename.LastIndexOf('.'), "~"));
            if (autoSaveFileInfo != null && fileinfo != null)
            {
                if (autoSaveFileInfo.LastWriteTime > fileinfo.LastWriteTime)
                {
                    DialogResult dlgRes = MessageBox.Show(
                        "Your saved file is older than the backup file.  Would you like to restore from the backup?",
                        "Load autosave backup?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    {
                        if (DialogResult.Yes == dlgRes)
                        {
                            autoSaveLoad = true;
                            filename = filename.Insert(filename.LastIndexOf('.'), "~");
                        }
                    }
                }
            }

            // The toolbox now includes the directory containing the world file
            // in the resource path, which it uses for creating new assets on
            // the fly.  Make sure the world editor uses this path as well.
            if (fileinfo.Directory != null)
            {
                InitializeResourcesPath(fileinfo.Directory.FullName);
            }

            XmlReader r = XmlReader.Create(filename, xmlReaderSettings);

            do
            {
                r.Read();
            } while (r.NodeType != XmlNodeType.Element);

            if (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "World")
                { // normal file reading
                    worldRoot = new WorldRoot(r, filename, worldTreeView, this, true);
                    saveWorldButton.Enabled = true;
                    saveWorldToolStripMenuItem.Enabled = true;
                    saveWorldAsMenuItem.Enabled = true;
                    if (!autoSaveLoad)
                    {
                        worldRoot.WorldFilePath = filename;
                    }
                    else
                    {
                        string worldFilename = filename.Remove(filename.LastIndexOf('~'), 1);
                        worldRoot.WorldFilePath = worldFilename;
                    }
                }
                else
                    if (r.Name == "WorldDescription")
                    {
                        worldRoot = LoadOldWorld(filename, r);
                    }
                if (!autoSaveLoad)
                {
                    title = String.Format("World Editor : {0}", filename.Substring(filename.LastIndexOf("\\") + 1));
                }
                else
                {
                    string tempname = filename.Substring(filename.LastIndexOf("\\") + 1);
                    String displayName = tempname.Remove(tempname.LastIndexOf('~'), 1);
                    title = String.Format("World Editor : {0}", displayName);
                }
                this.Text = title;
            }

            r.Close();

            if (worldRoot == null)
            {
                // couldn't load xml
                MessageBox.Show(string.Format("{0} is not a valid world file.", filename), "Invalid World File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                worldRoot.CheckAssets();

                if (missingAssetList.Count != 0)
                {
                    worldRoot = null;


                    string dialogStr = "Loading this world failed because the following assets are missing from the current Asset Repository:\n\n";
                    foreach (string s in missingAssetList)
                    {
                        dialogStr = string.Format("{0}{1}\n", dialogStr, s);
                    }
                    MessageBox.Show(dialogStr, "Missing Assets", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    camera.Position = worldRoot.CameraPosition;
                    camera.Orientation = worldRoot.CameraOrientation;

                    Regenerate();

                    worldRoot.AddToTree(null);
                    worldRoot.AddToScene();

                    DisplayPopupMessages("Warning: Loading World");
                }
            }
            if (autoSaveLoad)
            {
                foreach (WorldObjectCollection objects in worldRoot.WorldObjectCollections)
                {
                    if (objects.Filename != null && !String.Equals(objects.Filename, ""))
                    {
                        if (objects.Filename.LastIndexOf('~') >= 0)
                        {
                            objects.Filename = objects.Filename.Remove(objects.Filename.LastIndexOf('~'));
                        }
                    }
                    else
                    {

                        if (worldRoot.WorldFilePath != null && !String.Equals(worldRoot.WorldFilePath, ""))
                        {

                        }
                    }
                }
            }
            setWorldDefaults();
            setRecentFiles(filename);
        }

        /// <summary>
        /// Load world files from the old world editor
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        protected WorldRoot LoadOldWorld(string filename, XmlReader r)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(r);

            string baseName = filename.Substring(filename.LastIndexOf('\\') + 1);
            string worldName = baseName.Substring(0, baseName.LastIndexOf('.'));

            WorldRoot root = new WorldRoot(worldName, worldTreeView, this);
            saveWorldButton.Enabled = true;
            saveWorldToolStripMenuItem.Enabled = true;
            saveWorldAsMenuItem.Enabled = true;
            WorldObjectCollection collection = new WorldObjectCollection(worldName, root, this);

            root.Add(collection);

            foreach (XmlNode childNode in doc.FirstChild.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Terrain":
                        LoadOldTerrain(childNode, root);
                        break;
                    case "Skybox":
                        LoadOldSkybox(childNode, root);
                        break;
                    case "Objects":
                        LoadOldObjects(childNode, collection);
                        break;
                }
            }

            setWorldDefaults();
            setRecentFiles(filename);
            return root;
        }


        protected void LoadWorldRoot(String filename)
        {
            bool autoSaveLoad = false;
            string title;
            missingAssetList.Clear();
            FileInfo fileinfo = new FileInfo(filename);
            FileInfo autoSaveFileInfo = new FileInfo(filename.Insert(filename.LastIndexOf('.'), "~"));
            if (autoSaveFileInfo != null && fileinfo != null)
            {
                if (autoSaveFileInfo.LastWriteTime > fileinfo.LastWriteTime)
                {
                    DialogResult dlgRes = MessageBox.Show(
                        "Your saved file is older than the backup file.  Would you like to restore from the backup?",
                        "Load autosave backup?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    {
                        if (DialogResult.Yes == dlgRes)
                        {
                            autoSaveLoad = true;
                            filename = filename.Insert(filename.LastIndexOf('.'), "~");
                            filename.Remove(filename.LastIndexOf('~'), 1);
                        }
                    }
                }
            }

            // The toolbox now includes the directory containing the world file
            // in the resource path, which it uses for creating new assets on
            // the fly.  Make sure the world editor uses this path as well.
            if (fileinfo.Directory != null)
            {
                InitializeResourcesPath(fileinfo.Directory.FullName);
            }

            XmlReader r = XmlReader.Create(filename, xmlReaderSettings);

            do
            {
                r.Read();
            } while (r.NodeType != XmlNodeType.Element);

            if (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "World")
                { // normal file reading
                    worldRoot = new WorldRoot(r, filename, worldTreeView, this, false);
                    saveWorldButton.Enabled = true;
                    saveWorldToolStripMenuItem.Enabled = true;
                    saveWorldAsMenuItem.Enabled = true;
                    if (!autoSaveLoad)
                    {
                        worldRoot.WorldFilePath = filename;
                    }
                    else
                    {
                        string worldFilename = filename.Remove(filename.LastIndexOf('~'), 1);
                        worldRoot.WorldFilePath = worldFilename;
                    }
                }
                else
                    if (r.Name == "WorldDescription")
                    {
                        worldRoot = LoadOldWorld(filename, r);
                    }
                if (!autoSaveLoad)
                {
                    title = String.Format("World Editor : {0}", filename.Substring(filename.LastIndexOf("\\") + 1));
                }
                else
                {
                    filename = filename.Substring(filename.LastIndexOf("\\") + 1);
                    String displayName = filename.Remove(filename.LastIndexOf('~'), 1);
                    title = String.Format("World Editor : {0}", displayName);
                }
                this.Text = title;
            }

            r.Close();

            if (worldRoot == null)
            {
                // couldn't load xml
                MessageBox.Show(string.Format("{0} is not a valid world file.", filename), "Invalid World File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                worldRoot.CheckAssets();

                if (missingAssetList.Count != 0)
                {
                    worldRoot = null;


                    string dialogStr = "Loading this world failed because the following assets are missing from the current Asset Repository:\n\n";
                    foreach (string s in missingAssetList)
                    {
                        dialogStr = string.Format("{0}{1}\n", dialogStr, s);
                    }
                    MessageBox.Show(dialogStr, "Missing Assets", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    camera.Position = worldRoot.CameraPosition;
                    camera.Orientation = worldRoot.CameraOrientation;

                    Regenerate();

                    worldRoot.AddToTree(null);
                    worldRoot.AddToScene();

                    DisplayPopupMessages("Warning: Loading World");
                }
            }
            if (autoSaveLoad)
            {
                foreach (WorldObjectCollection objects in worldRoot.WorldObjectCollections)
                {
                    if (objects.Filename != null && !String.Equals(objects.Filename, ""))
                    {
                        if (objects.Filename.LastIndexOf('~') >= 0)
                        {
                            objects.Filename = objects.Filename.Remove(objects.Filename.LastIndexOf('~'));
                        }
                    }
                    else
                    {

                        if (worldRoot.WorldFilePath != null && !String.Equals(worldRoot.WorldFilePath, ""))
                        {
                            int pathlen = worldRoot.WorldFilePath.LastIndexOf("\\");
                            string worldRootFilename = worldRoot.WorldFilePath.Substring(pathlen + 1);
                            worldRootFilename = worldRootFilename.Substring(0, worldRootFilename.LastIndexOf('.'));
                            objects.Filename = String.Format("{0}-{1}.mwc", worldRootFilename, objects.Name);
                        }
                    }
                }
            }
            setWorldDefaults();
            setRecentFiles(filename);
        }


        public void setRecentFiles(string file)
        {
            string filename = file;
            string[] valueNames = { "a", "b", "c", "d", "e" };
            List<string> values = new List<string>();
            object ret;
            if ((filename.Substring(0, filename.LastIndexOf('.'))).EndsWith("~"))
            {
                int index = filename.LastIndexOf('~');
                filename = filename.Remove(filename.LastIndexOf('~'), 1);
            }
            values.Add(filename);
            foreach (string name in valueNames)
            {
                ret = Registry.GetValue(Config.RecentFileListBaseRegistryKey, name, (object)"");
                if (String.Equals(filename, (string)ret))
                {
                    continue;
                }
                if (ret != null)
                {
                    values.Add((string)ret);
                }
            }
            for (int i = 0; i < values.Count && i < 5; i++)
            {
                Registry.SetValue(Config.RecentFileListBaseRegistryKey, valueNames[i], values[i]);
            }

        }

        public void UpdateButtonBar()
        {

            for (int i = toolStrip1.Items.Count - 1; i >= 0; i--)
            {
                ToolStripItem item = toolStrip1.Items[i];
                if (item.Alignment == System.Windows.Forms.ToolStripItemAlignment.Right)
                {
                    toolStrip1.Items.Remove(item);
                }
            }
            if (SelectedObject.Count == 1)
            {
                if (SelectedObject[0].ButtonBar != null)
                {
                    for (int i = SelectedObject[0].ButtonBar.Count - 1; i >= 0; i--)
                    {
                        ToolStripItem button = (ToolStripItem)(SelectedObject[0].ButtonBar[i]);
                        toolStrip1.Items.Add(button);
                    }
                }
            }
            else
            {
                if (SelectedObject.Count > 1)
                {
                    CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                    foreach (ToolStripButton button in menuBuilder.MultiSelectButtonBar())
                    {
                        toolStrip1.Items.Add(button);
                    }
                }
            }
        }



        private void setWorldDefaults()
        {
            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = DisplayOcean && WorldRoot.DisplayOcean;
            if (Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique == Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth && !DisplayShadows)
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.None;
            }
            if (Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique == Axiom.SceneManagers.Multiverse.ShadowTechnique.None && DisplayShadows)
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth;
            }
            if (CameraNearDistance > 20)
            {
                CameraNear = oneMeter * 10f * (CameraNearDistance - 20f) / 20f;
            }
            else
            {
                CameraNear = oneMeter / 10f * (float)Math.Pow(10.0f, (double)CameraNearDistance / 20.0f);
            }
        }


        protected void LoadOldTerrain(XmlNode node, WorldRoot root)
        {
            root.Terrain.LoadTerrain(node.OuterXml);
        }

        protected void LoadOldSkybox(XmlNode node, WorldRoot root)
        {
        }

        protected void LoadOldObjects(XmlNode node, WorldObjectCollection collection)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "StaticObject":
                        LoadOldStaticObject(childNode, collection);
                        break;
                    case "Boundary":
                        LoadOldBoundary(childNode, collection);
                        break;
                    case "Waypoint":
                        LoadOldWaypoint(childNode, collection);
                        break;
                    case "Road":
                        LoadOldRoad(childNode, collection);
                        break;
                    default:
                        string name = childNode.Name;
                        break;
                }
            }
        }

        protected void LoadOldRoad(XmlNode node, WorldObjectCollection collection)
        {
            string name = null;
            int halfWidth = 1;
            XmlNode pointsNode = null;

            // get the name
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Name":
                        name = childNode.InnerText;
                        break;
                    case "Points":
                        pointsNode = childNode;
                        break;
                    case "HalfWidth":
                        halfWidth = int.Parse(childNode.InnerText);
                        break;
                    default:
                        break;
                }
            }

            // if the road has no points, just call it bogus and return
            if (pointsNode == null)
            {
                return;
            }

            // create and add the road to the world
            RoadObject road = new RoadObject(name, collection, this, halfWidth);
            collection.Add(road);

            // set up the points
            foreach (XmlNode pointNode in pointsNode.ChildNodes)
            {
                if (pointNode.Name == "Point")
                {
                    XmlNode locNode = pointNode.SelectSingleNode("Position");
                    Vector3 pointPos = GetVectorAttributes(locNode);

                    int pointnum;
                    road.Points.AddPoint(pointPos, out pointnum);
                }
            }
        }

        public GlobalFog GlobalFog
        {
            get
            {
                return globalFog;
            }
            set
            {
                globalFog = value;
            }
        }

        public GlobalAmbientLight GlobalAmbientLight
        {
            get
            {
                return globalAmbientLight;
            }
            set
            {
                globalAmbientLight = value;
            }
        }

        public GlobalDirectionalLight GlobalDirectionalLight
        {
            get
            {
                return globalDirectionalLight;
            }
            set
            {
                globalDirectionalLight = value;
            }
        }

        public MultiSelectTreeView WorldTreeView
        {
            get
            {
                return worldTreeView;
            }
        }


        protected void LoadOldBoundary(XmlNode node, WorldObjectCollection collection)
        {
            string name = null;
            XmlNode pointsNode = null;
            XmlNode semanticsNode = null;
            // int priority;

            // get the name
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Name":
                        name = childNode.InnerText;
                        break;
                    case "Points":
                        pointsNode = childNode;
                        break;
                    case "Attributes":
                        semanticsNode = childNode;
                        break;
                    default:
                        break;
                }
            }

            // if the boundary has no points, just call it bogus and return
            if (pointsNode == null)
            {
                return;
            }

            // create and add the boundary to the world
            Boundary boundary = new Boundary(collection, this, name, 100);
            collection.Add(boundary);

            // set up the points
            foreach (XmlNode pointNode in pointsNode.ChildNodes)
            {
                if (pointNode.Name == "Point")
                {
                    XmlNode locNode = pointNode.SelectSingleNode("Position");
                    Vector3 pointPos = GetVectorAttributes(locNode);

                    int pointNum;
                    boundary.Points.AddPoint(pointPos, out pointNum);
                }
            }

            if (semanticsNode != null)
            {
                // handle boundary semantics
                foreach (XmlNode semanticNode in semanticsNode.ChildNodes)
                {
                    switch (semanticNode.Name)
                    {
                        case "WaterAttribute":
                            XmlNode heightNode = semanticNode.SelectSingleNode("Height");
                            float height = float.Parse(heightNode.InnerText);
                            Water water = new Water(height, boundary, this);
                            boundary.Add(water);
                            break;
                        case "ForestAttribute":
                            XmlNode seedNode = semanticNode.SelectSingleNode("Seed");
                            int seed = int.Parse(seedNode.InnerText);

                            XmlNode speedWindNode = semanticNode.SelectSingleNode("SpeedWindFile");
                            string speedWindFile = speedWindNode.InnerText;

                            XmlNode windSpeedNode = semanticNode.SelectSingleNode("WindSpeed");
                            float windSpeed = float.Parse(windSpeedNode.InnerText);

                            XmlNode windDirNode = semanticNode.SelectSingleNode("WindDirection");
                            Vector3 windDir = GetVectorAttributes(windDirNode);

                            // Add the forest object
                            Forest forest = new Forest(speedWindFile, windSpeed, windDir, seed, boundary, this);
                            boundary.Add(forest);

                            XmlNode treeTypesNode = semanticNode.SelectSingleNode("TreeTypes");
                            if (treeTypesNode != null)
                            {
                                foreach (XmlNode treeTypeNode in treeTypesNode.ChildNodes)
                                {
                                    XmlNode treeNameNode = treeTypeNode.SelectSingleNode("TreeName");
                                    string treeName = treeNameNode.InnerText;

                                    XmlNode treeFilenameNode = treeTypeNode.SelectSingleNode("TreeDescriptionFilename");
                                    string treeFilename = treeFilenameNode.InnerText;

                                    XmlNode scaleNode = treeTypeNode.SelectSingleNode("Scale");
                                    float scale = float.Parse(scaleNode.InnerText);

                                    XmlNode scaleVarianceNode = treeTypeNode.SelectSingleNode("ScaleVariance");
                                    float scaleVariance = float.Parse(scaleVarianceNode.InnerText);

                                    XmlNode instancesNode = treeTypeNode.SelectSingleNode("Instances");
                                    uint instances = uint.Parse(instancesNode.InnerText);

                                    Tree tree = new Tree(treeName, treeFilename, scale, scaleVariance, instances, forest, this);
                                    forest.Add(tree);
                                }
                            }

                            break;
                        case "Fog":
                            XmlNode redNode = semanticNode.SelectSingleNode("ColorRed");
                            int red = int.Parse(redNode.InnerText);

                            XmlNode greenNode = semanticNode.SelectSingleNode("ColorGreen");
                            int green = int.Parse(greenNode.InnerText);

                            XmlNode blueNode = semanticNode.SelectSingleNode("ColorBlue");
                            int blue = int.Parse(blueNode.InnerText);

                            XmlNode nearNode = semanticNode.SelectSingleNode("Near");
                            float near = float.Parse(nearNode.InnerText);

                            XmlNode farNode = semanticNode.SelectSingleNode("Far");
                            float far = int.Parse(farNode.InnerText);

                            ColorEx fogColor = new ColorEx(((float)red) / 255f, ((float)green) / 255f, ((float)blue) / 255f);
                            Fog fog = new Fog(this, boundary, fogColor, near, far);
                            boundary.Add(fog);

                            break;
                        case "Sound":
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        protected void LoadOldWaypoint(XmlNode node, WorldObjectCollection collection)
        {
            string name = null;
            Vector3 pos = Vector3.Zero;

            // get everything but submeshes first
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Name":
                        name = childNode.InnerText;
                        break;
                    case "Position":
                        pos = GetVectorAttributes(childNode);
                        break;
                    default:
                        break;
                }
            }

            Waypoint waypoint = new Waypoint(name, collection, this, pos, Vector3.Zero);
            collection.Add(waypoint);
        }

        protected void LoadOldStaticObject(XmlNode node, WorldObjectCollection collection)
        {
            string name = null;
            string mesh = null;
            Vector3 pos = Vector3.Zero;
            Vector3 scale = Vector3.Zero;
            Vector3 rot = Vector3.Zero;

            // get everything but submeshes first
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Name":
                        name = childNode.InnerText;
                        break;
                    case "Mesh":
                        mesh = childNode.InnerText;
                        break;
                    case "Position":
                        pos = GetVectorAttributes(childNode);
                        break;
                    case "Scale":
                        scale = GetVectorAttributes(childNode);
                        break;
                    case "Rotation":
                        rot = GetVectorAttributes(childNode);

                        // force rot.y into the range of -180 to 180
                        while (rot.y > 180)
                        {
                            rot.y -= 360;
                        }
                        while (rot.y < -180)
                        {
                            rot.y += 360;
                        }
                        break;
                    default:
                        break;
                }
            }

            // create the object
            StaticObject obj = new StaticObject(name, collection, this, mesh, pos, scale, rot);
            collection.Add(obj);


            // process submeshes
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "SubMesh")
                {
                }
            }
        }

        protected Vector3 GetVectorAttributes(XmlNode node)
        {
            XmlAttribute attr;

            attr = node.Attributes["x"];
            float x = float.Parse(attr.Value);

            attr = node.Attributes["y"];
            float y = float.Parse(attr.Value);

            attr = node.Attributes["z"];
            float z = float.Parse(attr.Value);

            return new Vector3(x, y, z);
        }

        #endregion World Loading

        #region World Saving

        public XmlWriterSettings XMLWriterSettings
        {
            get
            {
                return xmlWriterSettings;
            }
        }

        protected void SaveManifest(string filename)
        {
            StreamWriter w = new StreamWriter(filename);
            worldRoot.ToManifest(w);

            w.Close();
        }

        public void SaveWorld(String filename)
        {
            try
            {
                XmlWriter w = XmlWriter.Create(filename, xmlWriterSettings);

                worldRoot.WorldFilePath = filename;

                worldRoot.CameraPosition = camera.Position;
                worldRoot.CameraOrientation = camera.Orientation;

                worldRoot.ToXml(w);

                w.Close();
            }
            catch (Exception e)
            {
                Axiom.Core.LogManager.Instance.Write(e.ToString());
                MessageBox.Show(String.Format("Unable to open the file for writing.  Use file menu \"Save As\" to save to another location.  Error: {0}", e.Message), "Error Saving File", MessageBoxButtons.OK);
            }

            // always save the manifest file
            string manifestFilename = string.Format("{0}.worldassets", filename.Substring(0, filename.LastIndexOf('.')));
            SaveManifest(manifestFilename);

            saveTimer.Interval = (int)autoSaveTime;
            saveTimer.Start();
            setRecentFiles(filename);
        }


        // this is used to save backup files
        public void SaveWorld(String filename, bool backup)
        {
            if (!backup)
            {
                SaveWorld(filename);
                return;
            }
            string file = filename.Insert(filename.LastIndexOf("."), "~");
            worldRoot.CameraPosition = camera.Position;
            worldRoot.CameraOrientation = camera.Orientation;

            try
            {
                XmlWriter w = XmlWriter.Create(file, xmlWriterSettings);
                worldRoot.ToXml(w, backup);
                w.Close();
            }
            catch (Exception e)
            {
                Axiom.Core.LogManager.Instance.Write(e.ToString());
                MessageBox.Show(String.Format("Unable to open the file for writing.  Use file menu \"Save As\" to save to another location.  Error: {0}", e.Message), "Error Saving File", MessageBoxButtons.OK);
            }

            // don't save the manifest file when doing autosave
            saveTimer.Interval = (int)autoSaveTime;
            saveTimer.Start();

        }


        #endregion World Saving

        #region Skybox
        public void SetSkybox(bool enable, string materialName)
        {
            scene.SetSkyBox(enable, materialName, 1000f * oneMeter);
        }

        #endregion Skybox

        #region Axiom Frame Event Handlers

        protected void OnFrameEnded(object source, FrameEventArgs e)
        {
            return;
        }


        protected void OnFrameStarted(object source, FrameEventArgs e)
        {
            Vector3 moveVector = Vector3.Zero;
            bool positionCamera = false;
            if (quit)
            {
                Root.Instance.QueueEndRendering();

                return;
            }

            long curTimer = Stopwatch.GetTimestamp();

            float frameTime = (float)(curTimer - lastFrameTime) / (float)timerFreq;
            lastFrameTime = curTimer;

            int maxFPS = Root.Instance.MaxFramesPerSecond;
            fpsStatusValueLabel.Text = String.Format("FPS: {0}", (Root.Instance.CurrentFPS.ToString() + (maxFPS > 0 ? " [Limited]" : "")));

            List<Boundary> activeBoundaryList = new List<Boundary>();

            foreach (Boundary bound in boundaryList)
            {
                if (bound.PointIn(camera.Position))
                {
                    activeBoundaryList.Add(bound);

                }
            }



            if (moveOnPlane && !lockCameraToObject)
            {
                cameraStatusLabel.Text = "Camera Status: Lock to plane";
            }
            if (lockCameraToObject && !moveOnPlane)
            {
                cameraStatusLabel.Text = "Camera Status: Lock Object";
            }
            if (lockCameraToObject && moveOnPlane)
            {
                cameraStatusLabel.Text = "Camera Status: Lock to plane & Lock to Object";
            }
            if (!moveOnPlane && !lockCameraToObject)
            {
                if (accelerateCamera)
                {
                    float accelRate = camAccel / 1000f;
                    cameraStatusLabel.Text = String.Format("Camera Status: Accelerate Camera");
                    if (!statusStrip1.Items.Contains(cameraAccelRateLabel))
                    {
                        cameraAccelRateLabel = new ToolStripStatusLabel(String.Format("Camera Acceleration Rate: {0}", accelRate));
                        StatusBarAddItem(cameraAccelRateLabel);
                    }
                    else
                    {
                        cameraAccelRateLabel.Text = String.Format("Camera Acceleration Rate: {0}", accelRate);
                    }
                }
                else
                {
                    if (statusStrip1.Items.Contains(cameraAccelRateLabel))
                    {
                        StatusBarRemoveItem(cameraAccelRateLabel);
                    }
                    cameraStatusLabel.Text = "Camera Status: Camera Maintains Speed";
                }

            }

            checkPointAgainstBoundaryList(activeBoundaryList);
            updateStatusBarBoundaryList(activeBoundaryList);
            time += e.TimeSinceLastFrame;
            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.Time = time;


            // reset acceleration zero
            camDirection = Vector3.Zero;

            if (moveForward)
            {
                camDirection += Vector3.NegativeUnitZ;
            }
            if (moveBack)
            {
                camDirection += Vector3.UnitZ;
            }
            if (moveLeft)
            {
                camDirection += Vector3.NegativeUnitX;
            }
            if (moveRight)
            {
                camDirection += Vector3.UnitX;
            }

            int deltaX = lastMouseX - newMouseX;
            int deltaY = lastMouseY - newMouseY;

            lastMouseX = newMouseX;
            lastMouseY = newMouseY;

            //            mouseGroundCoorPanel.Text = string.Format("[{0}, {1}]", Math.Round(mouseWorldLoc.x / oneMeter), Math.Round(mouseWorldLoc.z / oneMeter));
            mouseGroundCoorPanel.Text = PickTriangleAndTerrain(newMouseX, newMouseY);

            if (mouseIntercepted)
            {
                // We check mouseIntercepted before calling each callback below because
                // one of the callback might release the mouse, which will clear out
                // the callbacks.

                // Generate callbacks for intercepted mouse events
                if ((deltaX != 0) || (deltaY != 0))
                {
                    mouseMoveIntercepter(this, newMouseX, newMouseY);
                }
                if (mouseIntercepted && leftMouseClick)
                {
                    mouseDownIntercepter(this, MouseButtons.Left, newMouseX, newMouseY);
                }
                if (mouseIntercepted && leftMouseRelease)
                {
                    mouseUpIntercepter(this, MouseButtons.Left, newMouseX, newMouseY);
                }
                if (mouseIntercepted && rightMouseClick)
                {
                    mouseDownIntercepter(this, MouseButtons.Right, newMouseX, newMouseY);
                }
                if (mouseIntercepted && rightMouseRelease)
                {
                    mouseUpIntercepter(this, MouseButtons.Right, newMouseX, newMouseY);
                }
                if (mouseIntercepted && middleMouseClick)
                {
                    mouseDownIntercepter(this, MouseButtons.Middle, newMouseX, newMouseY);
                }
                if (mouseIntercepted && middleMouseRelease)
                {
                    mouseUpIntercepter(this, MouseButtons.Middle, newMouseX, newMouseY);
                }
            }
            else
            {
                //              if (leftMouseDown)
                //              {
                //                  if (deltaX != 0)
                //                  {
                //                      camera.Yaw(-deltaX * mouseRotationScale);
                //                  }
                //                  if (deltaY != 0)
                //                  {
                //                      camera.Pitch(-deltaY * mouseRotationScale);
                //                  }
                //              }
                if (mouseTurnCamera)
                {
                    if (lockCameraToObject && SelectedObject != null && SelectedObject.Count == 1 && (SelectedObject[0] is IObjectCameraLockable))
                    {
                        if (deltaY != 0)
                        {
                            CameraZenith += deltaY;
                        }
                        if (deltaX != 0)
                        {
                            CameraAzimuth += deltaX;
                        }
                        positionCamera = true;
                    }
                    else
                    {
                        if (deltaX != 0)
                        {
                            camera.Yaw(deltaX * mouseRotationScale);
                        }
                        if (deltaY != 0)
                        {
                            camera.Pitch(deltaY * mouseRotationScale);
                        }
                    }
                }

                if (!lockCameraToObject)
                {
                    if (turnCameraDown)
                    {
                        camera.Pitch(-cameraTurnIncrement * e.TimeSinceLastFrame);
                    }
                    if (turnCameraUp)
                    {
                        camera.Pitch(cameraTurnIncrement * e.TimeSinceLastFrame);
                    }
                    if (turnCameraLeft)
                    {
                        camera.Yaw(cameraTurnIncrement * e.TimeSinceLastFrame);
                    }
                    if (turnCameraRight)
                    {
                        camera.Yaw(-(cameraTurnIncrement * e.TimeSinceLastFrame));
                    }
                }
                if (mouseWheelDelta != 0)
                {
                    if (lockCameraToObject && SelectedObject != null && SelectedObject.Count == 1 && (SelectedObject[0] is IObjectCameraLockable))
                    {
                        CameraRadius = cameraRadius - (mouseWheelMultiplier * mouseWheelDelta * oneMeter);
                        positionCamera = true;
                    }
                    else
                    {
                        camera.MoveRelative(new Vector3(0, 0, -oneMeter * mouseWheelMultiplier * mouseWheelDelta));
                    }
                    mouseWheelDelta = 0;
                }
            }

            // reset single fire click and release events
            leftMouseClick = false;
            rightMouseClick = false;
            leftMouseRelease = false;
            rightMouseRelease = false;

            if (warpCamera)
            {
                warpCamera = false;
                WarpCamera();
            }

            if (cameraLookDownSouth)
            {
                cameraLookDownSouth = false;
                camera.LookAt(new Vector3(camera.Position.x, 0, camera.Position.z + oneMeter * 100f));
            }

            if (takeScreenShot)
            {
                string[] temp = Directory.GetFiles(Environment.CurrentDirectory, "screenshot*.png");
                string fileName = string.Format("screenshot{0}.png", temp.Length + 1);
                window.Save(fileName);
            }

            if (moveOnPlane)
            {
                camera.MoveRelative(new Vector3(deltaX * 1000f, deltaY * 1000F, 0f));
            }


            if (moveCameraUp && !lockCameraToObject && !moveOnPlane)
            {
                camera.Move(new Vector3(0, oneMeter * cameraUpDownMultiplier * e.TimeSinceLastFrame, 0));
            }

            if (moveCameraDown && !lockCameraToObject && !moveOnPlane)
            {
                camera.Move(new Vector3(0, -oneMeter * cameraUpDownMultiplier * e.TimeSinceLastFrame, 0));
            }



            if (!lockCameraToObject && !moveOnPlane && (camDirection.x != 0f || camDirection.y != 0f || camDirection.z != 0f))
            {
                if (accelerateCamera)
                {
                    camVelocity += camAccel * e.TimeSinceLastFrame;
                }
                else
                {
                    camVelocity = camSpeed;
                }
                camera.MoveRelative(camDirection * camVelocity * e.TimeSinceLastFrame);
                cameraSpeedStatusLabel.Text = String.Format("Camera Speed: {0} m/s", camVelocity / 1000f);
            }
            else
            {
                cameraSpeedStatusLabel.Text = String.Format("Camera Speed: {0} m/s", 0f);
                camVelocity = 0f;
            }

            // Now dampen the Velocity - only if user is not accelerating
            //if (camAccel == Vector3.Zero)
            //{
            //    float decel = 1 - (6 * e.TimeSinceLastFrame);

            //    if (decel < 0)
            //    {
            //        decel = 0;
            //    }
            //    camVelocity *= decel;
            //}


            // keep camera at least 2 meters above ground
            float heightUnderCamera = GetTerrainHeight(camera.Position.x, camera.Position.z);

            if (!lockCameraToObject && (cameraAboveTerrain && (camera.Position.y < (heightUnderCamera + 2000))) || cameraTerrainFollow)
            {
                camera.Position = new Vector3(camera.Position.x, heightUnderCamera + 2000, camera.Position.z);
            }

            if (positionCamera && lockCameraToObject)
            {
                PositionCamera();
            }

            // Set the * at the end of the filename if it is dirty
            if (undoRedo != null && undoRedo.Dirty && !this.Text.EndsWith("*"))
            {
                this.Text = String.Format("{0}{1}", this.Text, "*");
            }
            else if (undoRedo != null && !undoRedo.Dirty && this.Text.EndsWith("*"))
            {
                this.Text = this.Text.Substring(0, this.Text.Length - 1);
            }
        }

        protected void WarpCamera()
        {
            Ray ray = camera.GetCameraToViewportRay((float)newMouseX / (float)window.Width, (float)newMouseY / (float)window.Height);
            Vector3 newLoc;

            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(ray);

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.FirstTerrain;
            List<RaySceneQueryResultEntry> results = q.Execute();

            if (results.Count > 0)
            {
                RaySceneQueryResultEntry result = results[0];

                newLoc = result.worldFragment.SingleIntersection;

                camera.Position = new Vector3(newLoc.x, camera.Position.y, newLoc.z);
            }
        }

        protected void updateStatusBarBoundaryList(List<Boundary> activeBoundaryList)
        {
            string itemText = "Active Regions: ";
            string boundNames = "";
            ToolStripItem item;
            if (activeBoundaryList.Count > 0)
            {
                if (statusStrip1.Items.Contains(activeBoundaryListToolStripStatusLabel as ToolStripItem))
                {
                    int index = statusStrip1.Items.IndexOf(activeBoundaryListToolStripStatusLabel as ToolStripItem);
                    item = statusStrip1.Items[index];
                }
                else
                {
                    item = activeBoundaryListToolStripStatusLabel as ToolStripItem;
                    StatusBarAddItem(item);
                }
                if (activeBoundaryList.Count > 0)
                {
                    foreach (Boundary bound in activeBoundaryList)
                    {
                        if (boundNames != "")
                        {
                            boundNames = String.Format("{1}, {0}", bound.Name, boundNames);
                        }
                        else
                        {
                            boundNames = String.Format("{0}", bound.Name);
                        }
                    }
                    itemText = String.Format("{0}{1}", itemText, boundNames);
                    ((ToolStripLabel)item).Text = itemText;
                }
            }
            else
            {
                if (statusStrip1.Items.Contains(activeBoundaryListToolStripStatusLabel as ToolStripItem))
                {
                    StatusBarRemoveItem(activeBoundaryListToolStripStatusLabel as ToolStripItem);
                }
            }
        }

        protected void CameraToSelectedObject()
        {
            if (SelectedObject != null && SelectedObject.Count == 1)
            {
                Vector3 focusLoc = SelectedObject[0].FocusLocation;
                Vector3 cameraLoc = focusLoc + config.CameraFocusOffset;

                camera.Position = cameraLoc;
                camera.LookAt(focusLoc);
            }
        }

        public void SceneChangedHandler(object sender, EventArgs args)
        {
            bool match = false;
            foreach (Boundary bound in boundaryList)
            {
                match = String.Equals(bound.Name, ((Boundary)sender).Name);
                if (match)
                {
                    boundaryList.Remove((Boundary)sender);
                    break;
                }
            }
            if (match == false)
            {
                boundaryList.Add((Boundary)sender);
            }
        }


        private Boundary findBoundaryListPriority(List<Boundary> boundList)
        {
            int topPriority = 101;
            Boundary top = null;
            foreach (Boundary bound in boundList)
            {
                if (bound.Priority < topPriority)
                {
                    top = bound;
                    topPriority = bound.Priority;
                }
            }
            return top;
        }

        protected void FogOff()
        {
            scene.SetFog(FogMode.None, null, 0);
        }


        public WorldTreeNode MakeTreeNode(IWorldObject obj, String text)
        {
            return new WorldTreeNode(obj, text);
        }

        protected void checkPointAgainstBoundaryList(List<Boundary> activeBoundaryList)
        {
            List<Boundary> fog = new List<Boundary>();
            List<Boundary> ambientLight = new List<Boundary>();
            List<Boundary> directionalLight = new List<Boundary>();
            Boundary region;
            foreach (Boundary bound in activeBoundaryList)
            {
                foreach (IWorldObject obj in bound.Children)
                {
                    switch (obj.ObjectType)
                    {
                        case "Fog":
                            fog.Add(bound);
                            break;
                        case "AmbientLight":
                            ambientLight.Add(bound);
                            break;
                        case "DirectionalLight":
                            directionalLight.Add(bound);
                            break;
                    }
                }

            }

            if (displayFog)
            {
                if (fog.Count == 0)
                {
                    if (globalFog != null)
                    {
                        scene.SetFog(FogMode.Linear, globalFog.Color, 0, globalFog.Near, globalFog.Far);
                    }
                    else
                    {
                        FogOff();
                    }
                }
                else
                {
                    region = findBoundaryListPriority(fog);
                    if (region != null)
                    {
                        foreach (IWorldObject obj in region.Children)
                        {
                            if (String.Equals(obj.ObjectType, "Fog"))
                            {
                                scene.SetFog(FogMode.Linear, ((Fog)obj).Cx, 0, ((Fog)obj).Near, ((Fog)obj).Far);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                FogOff();
            }

            if (displayLights)
            {

                if (ambientLight.Count == 0)
                {
                    if (globalAmbientLight != null)
                    {
                        scene.AmbientLight = globalAmbientLight.Color;
                    }
                    else
                    {
                        scene.AmbientLight = Config.DefaultAmbientLightColor;
                    }
                }
                else
                {
                    region = findBoundaryListPriority(ambientLight);
                    if (region != null)
                    {
                        foreach (IWorldObject obj in region.Children)
                        {
                            if (String.Equals(obj.ObjectType, "AmbientLight"))
                            {
                                scene.AmbientLight = ((AmbientLight)obj).Color;
                                break;
                            }
                        }
                    }
                }

                if (directionalLight.Count == 0)
                {
                    if (globalDirectionalLight != null)
                    {
                        if (!String.Equals(activeDirectionalLightName, ""))
                        {
                            scene.RemoveLight(activeDirectionalLightName);
                            activeDirectionalLightName = "";
                        }
                        Light mainLight = scene.CreateLight("MainLight");
                        activeDirectionalLightName = mainLight.Name;
                        mainLight.Type = LightType.Directional;
                        Vector3 mainLightDir = globalDirectionalLight.LightDirection;
                        mainLightDir.Normalize();
                        mainLight.Direction = mainLightDir;
                        mainLight.Diffuse = globalDirectionalLight.Diffuse;
                        mainLight.Specular = globalDirectionalLight.Specular;
                    }
                }
                else
                {
                    region = findBoundaryListPriority(directionalLight);
                    if (region != null)
                    {
                        foreach (IWorldObject obj in region.Children)
                        {
                            if (String.Equals(obj.ObjectType, "DirectionalLight"))
                            {
                                if (!String.Equals(activeDirectionalLightName, ""))
                                {
                                    scene.RemoveLight(activeDirectionalLightName);
                                    activeDirectionalLightName = "";
                                }
                                Light light = scene.CreateLight(String.Format("{0}-{1}", region.Name, "DirectionalLight"));
                                activeDirectionalLightName = light.Name;
                                light.Type = LightType.Directional;
                                DirectionalLight dirLight = ((DirectionalLight)obj);
                                light.Direction = dirLight.LightDir;
                                light.Direction.Normalize();
                                light.Diffuse = dirLight.Diffuse;
                                light.Specular = dirLight.Specular;

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                scene.AmbientLight = Config.DefaultAmbientLightColor;

                // remove the active directional light
                if (!String.Equals(activeDirectionalLightName, ""))
                {
                    scene.RemoveLight(activeDirectionalLightName);
                    activeDirectionalLightName = "";
                }

                // create a default directional light
                Light mainLight = scene.CreateLight("MainLight");
                activeDirectionalLightName = mainLight.Name;
                mainLight.Type = LightType.Directional;
                mainLight.Direction = config.DefaultDirectionalLightDirection;
                mainLight.Diffuse = config.DefaultGlobalDirectionalLightDiffuse;
                mainLight.Specular = config.DefaultGlobalDirectionalLightSpecular;
            }
        }

        #endregion Axiom Frame Event Handlers

        #region Mouse Interception

        /// <summary>
        /// Grab mouse events for some modal operation in the 3d world
        /// </summary>
        /// <param name="mouseMoveEvent"></param>
        /// <param name="mouseDownEvent"></param>
        /// <param name="mouseUpEvent"></param>
        /// <returns></returns>
        public bool InterceptMouse(MouseMoveIntercepter mouseMove, MouseButtonIntercepter mouseDown, MouseButtonIntercepter mouseUp, MouseCaptureLost lost, bool capture)
        {
            if (mouseIntercepted)
            {
                return true;
            }
            else
            {
                // hook in event handlers
                mouseMoveIntercepter = mouseMove;
                mouseDownIntercepter = mouseDown;
                mouseUpIntercepter = mouseUp;
                mouseCaptureLost = lost;

                if (capture)
                {
                    axiomControl.Capture = true;
                }

                mouseIntercepted = true;
                return false;
            }
        }

        /// <summary>
        /// Release the grabbed mouse.
        /// </summary>
        public void ReleaseMouse()
        {
            // remove hooked in handlers
            mouseMoveIntercepter = null;
            mouseDownIntercepter = null;
            mouseUpIntercepter = null;

            mouseIntercepted = false;

            return;
        }

        public bool MouseIntercepted
        {
            get
            {
                return mouseIntercepted;
            }
        }

        #endregion Mouse Interception

        #region Properties

        public static WorldEditor Instance
        {
            get
            {
                if (instance == null)
                    throw new Exception("WorldEditor.Instance called, but instance is null");
                return instance;
            }
        }

        public SceneManager Scene
        {
            get
            {
                return scene;
            }
        }

        private bool DisplayWireFrame
        {
            get
            {
                return displayWireFrame;
            }
            set
            {
                displayWireFrame = value;
                if (displayWireFrame)
                {
                    camera.SceneDetail = SceneDetailLevel.Wireframe;
                }
                else
                {
                    camera.SceneDetail = SceneDetailLevel.Solid;
                }
            }
        }

        public bool DisplayTerrainDecals
        {
            get
            {
                return displayTerrainDecals;
            }
            set
            {
                displayTerrainDecals = value;
            }
        }



        public bool DisplayParticleEffects
        {
            get
            {
                return displayParticleEffects;
            }
            set
            {
                displayParticleEffects = value;
            }
        }

        private bool DisplayOcean
        {
            get
            {
                return displayOcean;
            }
            set
            {
                displayOcean = value;
                if (worldRoot != null)
                {
                    Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = displayOcean && worldRoot.DisplayOcean;
                }
            }
        }

        private bool DisplayShadows
        {
            get
            {
                return displayShadows;
            }
            set
            {
                displayShadows = value;
                if (worldRoot != null)
                {
                    if (Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique == Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth && !displayShadows)
                    {
                        Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.None;
                    }
                    if (Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique == Axiom.SceneManagers.Multiverse.ShadowTechnique.None && displayShadows)
                    {
                        Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth;
                    }
                }
            }
        }

        private float CameraNearDistance
        {
            get
            {
                return cameraNearDistance;
            }
            set
            {
                cameraNearDistance = value;
                if (worldRoot != null)
                {
                    if (cameraNearDistance > 20)
                    {
                        CameraNear = oneMeter * 10f * (cameraNearDistance - 20f) / 20f;
                    }
                    else
                    {
                        CameraNear = oneMeter / 10f * (float)Math.Pow(10.0f, (double)cameraNearDistance / 20.0f);
                    }
                }
            }
        }

        private bool DisplayTerrain
        {
            get
            {
                return displayTerrain;
            }
            set
            {
                displayTerrain = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.DrawTerrain = displayTerrain;
            }
        }

        public bool DisplayPointLightMarker
        {
            get
            {
                return displayPointLightMarker;
            }
        }

        public bool DisplayLights
        {
            get
            {
                return displayLights;
            }
        }

        private bool RenderLeaves
        {
            get
            {
                return renderLeaves;
            }
            set
            {
                renderLeaves = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.RenderLeaves = renderLeaves;
            }
        }


        public float CameraNear
        {
            get
            {
                return camera.Near;
            }
            set
            {
                camera.Near = value;
            }
        }

        public AssetCollection Assets
        {
            get
            {
                return assets;
            }
        }

        public EventHandler DefaultCommandClickHandler
        {
            get
            {
                return new EventHandler(CommandMenuButton_Click);
            }
        }

        public EventHandler HelpClickHandler
        {
            get
            {
                return new EventHandler(HelpMenuButton_Click);
            }
        }

        public EventHandler RemoveCollectionFromSceneClickHandler
        {
            get
            {
                return new EventHandler(RemoveCollectionFromSceneButton_Click);
            }
        }

        public EventHandler AddCollectionToSceneClickHandler
        {
            get
            {
                return new EventHandler(AddCollectionToSceneButton_Click);
            }
        }

        public EventHandler CameraObjectDirClickHandler
        {
            get
            {
                return new EventHandler(CameraObjectDirMenuButton_Click);
            }
        }

        public EventHandler CameraMarkerDirClickHandler
        {
            get
            {
                return new EventHandler(CameraMarkerDirMenuButton_Click);
            }
        }

        public Random Random
        {
            get
            {
                return random;
            }
        }

        public WorldEditorConfig Config
        {
            get
            {
                return config;
            }
        }

        public bool DisplayBoundaryMarkers
        {
            get
            {
                return displayBoundaryMarkers;
            }
        }

        public bool DisplayRoadMarkers
        {
            get
            {
                return displayRoadMarkers;
            }
        }

        public bool DisplayMarkerPoints
        {
            get
            {
                return displayMarkerPoints;
            }
        }

        public bool DisplayPointLightCircles
        {
            get
            {
                return displayPointLightCircles;
            }
        }


        public bool CameraAboveTerrain
        {
            get
            {
                return cameraAboveTerrain;
            }
            set
            {
                cameraAboveTerrain = value;
            }
        }

        public List<IWorldObject> SelectedObject
        {
            get
            {
                List<IWorldObject> selectedObject = new List<IWorldObject>();
                foreach (MultiSelectTreeNode node in worldTreeView.SelectedNodes)
                {
                    selectedObject.Add((node as WorldTreeNode).WorldObject);
                }
                return selectedObject;
            }
            set
            {
                List<MultiSelectTreeNode> selectedNodes = new List<MultiSelectTreeNode>();

                if (value != null)
                {
                    worldTreeView.ClearSelectedTreeNodes();
                    foreach (IWorldObject obj in value)
                    {
                        obj.Node.Select();
                    }
                }
            }
        }

        // A list of _untranformed_ collision shapes for the mesh.
        // Used by the path generation algorithm
        public Dictionary<string, List<CollisionShape>> MeshCollisionShapes
        {
            get
            {
                return meshCollisionShapes;
            }
        }

        public WorldRoot WorldRoot
        {
            get
            {
                return worldRoot;
            }
        }

        public ClipboardObject Clipboard
        {
            get
            {
                return clipboard;
            }
        }

        public bool MouseDragEvent
        {
            get
            {
                return mouseDragEvent;
            }
            set
            {
                mouseDragEvent = value;
            }
        }

        public bool WorldDirty
        {
            get
            {
                return undoRedo.Dirty;
            }
        }

        public void ResetDirtyWorld()
        {
            undoRedo.ResetDirty();
        }

        public Vector3 CameraPosition
        {
            get
            {
                return camera.Position;
            }
        }

        public Quaternion CameraOrientation
        {
            get
            {
                return camera.Orientation;
            }
        }

        public bool Quit
        {
            get
            {
                return quit;
            }
        }

        #endregion Properties

        #region Position Panel

        protected void SetObjectPositionWithCommand(Vector3 v)
        {
            PositionChangeCommand cmd = new PositionChangeCommand(this, SelectedPositionObject, SelectedPositionObject.Position, v);

            ExecuteCommand(cmd);
        }

        protected void UpdatePositionFromPanel()
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (SelectedObject.Count > 0 && SelectedObject.Count == 1)
            {
                if (positionXTextBox.Text != null && !String.Equals(positionXTextBox.Text, "") && !String.Equals(positionXTextBox.Text, " "))
                {
                    if (!float.TryParse(positionXTextBox.Text, out x))
                    {
                        x = 0;
                    }
                }
                if (positionYTextBox.Text != null && !String.Equals(positionYTextBox.Text, "") && !String.Equals(positionYTextBox.Text, " "))
                {

                    if (!float.TryParse(positionYTextBox.Text, out y))
                    {
                        y = 0;
                    }
                }
                if (positionZTextBox.Text != null && !String.Equals(positionZTextBox.Text, "") && !String.Equals(positionZTextBox.Text, " "))
                {
                    if (!float.TryParse(positionZTextBox.Text, out z))
                    {
                        z = 0;
                    }
                }

                Vector3 v = new Vector3(x, y, z);

                if (v != SelectedPositionObject.Position)
                {
                    SetObjectPositionWithCommand(v);
                }

                // Update the panel in case the object changed the position (can happen when following terrain
                UpdatePositionPanel(SelectedPositionObject);
            }
        }

        public void UpdateDisplayObjectFromPanel()
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (SelectedObject.Count > 0 && SelectedObject.Count == 1)
            {
                if (positionXTextBox.Text != null && !String.Equals(positionXTextBox.Text, "") && !String.Equals(positionXTextBox.Text, " "))
                {
                    if (!float.TryParse(positionXTextBox.Text, out x))
                    {
                        x = 0;
                    }
                }
                if (positionYTextBox.Text != null && !String.Equals(positionYTextBox.Text, "") && !String.Equals(positionYTextBox.Text, " "))
                {

                    if (!float.TryParse(positionYTextBox.Text, out y))
                    {
                        y = 0;
                    }
                }
                if (positionZTextBox.Text != null && !String.Equals(positionZTextBox.Text, "") && !String.Equals(positionZTextBox.Text, " "))
                {
                    if (!float.TryParse(positionZTextBox.Text, out z))
                    {
                        z = 0;
                    }
                }

                Vector3 v = new Vector3(x, y, z);


                if ((SelectedPositionObject is StaticObject || SelectedPositionObject is Waypoint || SelectedPositionObject is PointLight || SelectedPositionObject is MPPoint) && SelectedPositionObject.Display != null && SelectedPositionObject.Display.Position != v)
                {
                    SelectedPositionObject.Display.Position = v;
                }
                else
                {
                    if (SelectedPositionObject is TerrainDecal && (((v.z != (SelectedPositionObject as TerrainDecal).Decal.PosZ)) || ((SelectedPositionObject as TerrainDecal).Decal.PosX != v.x)))
                    {
                        (SelectedPositionObject as TerrainDecal).Decal.PosX = v.x;
                        (SelectedPositionObject as TerrainDecal).Decal.PosZ = v.z;
                    }
                }

                // Update the panel in case the object changed the position (can happen when following terrain
                UpdatePositionPanel(v);
            }
        }

        public void UpdatePositionPanel(IObjectPosition obj)
        {
            if (SelectedPositionObject != null && obj == SelectedPositionObject)
            {
                UpdatePositionPanel(obj.Position);
            }
        }

        protected void UpdatePositionPanel(Vector3 v)
        {
            positionXTextBox.Text = v.x.ToString();
            positionYTextBox.Text = v.y.ToString();
            positionZTextBox.Text = v.z.ToString();
        }

        protected IObjectPosition SelectedPositionObject
        {
            get
            {
                if (SelectedObject != null && SelectedObject.Count == 1 && SelectedObject[0] is IObjectPosition && SelectedObject.Count < 2)
                {
                    return SelectedObject[0] as IObjectPosition;
                }
                return null;
            }
        }

        #endregion Position Panel

        #region Scale Panel

        protected void SetObjectScaleWithCommand(float scale)
        {
            ScaleChangeCommand cmd = new ScaleChangeCommand(this, SelectedScaleObject, SelectedScaleObject.Scale, scale);

            ExecuteCommand(cmd);
        }

        public void UpdateScalePanel(IObjectScale obj)
        {
            if (SelectedScaleObject != null && obj == SelectedScaleObject)
            {
                UpdateScalePanel(obj.Scale);
            }
        }

        protected void UpdateScalePanel(float s)
        {
            scaleTextBox.Text = s.ToString();
        }

        protected void UpdateScaleFromPanel()
        {
            float scale = 1;
            if (SelectedScaleObject != null)
            {
                float s;

                if (scaleTextBox.Text == null || String.Equals(scaleTextBox.Text, "") || (String.Equals(scaleTextBox.Text, " ")))
                {
                    scale = 1;
                }
                if (float.TryParse(scaleTextBox.Text, out s))
                {
                    scale = s;
                }
                else
                {
                    return;
                }
                if (scale != SelectedScaleObject.Scale)
                {
                    SetObjectScaleWithCommand(scale);
                }
            }
        }

        protected IObjectScale SelectedScaleObject
        {
            get
            {
                if (SelectedObject != null && SelectedObject.Count == 1 && SelectedObject[0] is IObjectScale)
                {
                    return SelectedObject[0] as IObjectScale;
                }
                return null;
            }
        }

        #endregion Scale Panel

        #region Rotation Panel

        protected void SetObjectRotationWithCommand(float rot)
        {
            RotationChangeCommand cmd = new RotationChangeCommand(this, SelectedRotationObject, SelectedRotationObject.Rotation, rot);

            ExecuteCommand(cmd);
        }

        protected void SetRotationOnDisplayObject(float rot)
        {
            if (SelectedObject.Count == 1 && (SelectedObject[0] is StaticObject || SelectedObject[0] is TerrainDecal))
            {
                switch (SelectedObject[0].ObjectType)
                {
                    case "Object":
                        (SelectedObject[0] as StaticObject).DisplayObject.SetRotation(rot);
                        break;
                    case "TerrainDecal":
                        (SelectedObject[0] as TerrainDecal).Decal.Rot = rot;
                        (SelectedObject[0] as TerrainDecal).Decal.Update(camera.Position.x, camera.Position.z);
                        break;
                }
            }
        }

        protected void UpdateRotationPanel(float r)
        {
            while (r > 180 || r < -180)
            {
                if (r > 180)
                {
                    r -= 360;
                }
                else
                {
                    if (r < -180)
                    {
                        r += 360;
                    }
                }
            }
            int rot = (int)Math.Round(r);
            rotationTextBox.Text = rot.ToString();
            rotationTrackBar.Value = rot;
        }

        public void UpdateRotationPanel(IObjectRotation obj)
        {
            if (SelectedRotationObject != null && obj == SelectedRotationObject)
            {
                UpdateRotationPanel(obj.Rotation);
            }
        }

        protected void UpdateRotationFromTrackbar()
        {
            if (SelectedRotationObject != null)
            {
                SetRotationOnDisplayObject(rotationTrackBar.Value);
            }
            UpdateRotationPanel(rotationTrackBar.Value);
        }

        protected void SetRotationFromTrackbar()
        {
            if (SelectedRotationObject != null)
            {
                SetObjectRotationWithCommand(float.Parse(rotationTrackBar.Value.ToString()));
            }
        }

        protected void UpdateRotationFromTextbox()
        {
            float rot = 0;
            if (SelectedRotationObject != null)
            {
                if (rotationTextBox.Text != null && !String.Equals(rotationTextBox.Text, "") && !(String.Equals(rotationTextBox.Text, " ")))
                {
                    if (float.TryParse(rotationTextBox.Text, out rot))
                    {
                        if (rot != SelectedRotationObject.Rotation)
                        {
                            SetObjectRotationWithCommand(rot);
                        }
                    }
                    else
                    {
                        UpdateRotationPanel(0f);
                    }
                }
            }

            UpdateRotationPanel(rot);
        }

        protected IObjectRotation SelectedRotationObject
        {
            get
            {
                if (SelectedObject != null && SelectedObject.Count == 1 && SelectedObject[0] is IObjectRotation)
                {
                    return SelectedObject[0] as IObjectRotation;
                }
                return null;
            }
        }

        #endregion //Rotation Panel

        #region Orientation Panel

        protected void SetObjectOrientationWithCommand(float r, float i)
        {
            if (!orientationUpdate)
            {
                OrientationChangeCommand cmd = new OrientationChangeCommand(this, SelectedOrientationObject, r, i, SelectedOrientationObject.Azimuth, SelectedOrientationObject.Zenith);
                ExecuteCommand(cmd);
            }
        }


        //protected void UpdateOrientationFromTrackBars(object sender, EventArgs e)
        //{
        //    if (SelectedOrientationObject != null && (orientationRotationTrackBar.Value != 
        //        (int) Math.Round(SelectedOrientationObject.Azimuth) || inclinationTrackbar.Value != 
        //        (int) Math.Round(SelectedOrientationObject.Zenith)))
        //    {
        //        SetObjectOrientationWithCommand(orientationRotationTrackBar.Value, inclinationTrackbar.Value); 
        //    }
        //}


        protected void inclinationTrackbar_MouseUp(object sender, MouseEventArgs e)
        {
            SetObjectOrientationWithCommand(float.Parse(orientationRotationTrackBar.Value.ToString()),
                float.Parse(inclinationTrackbar.Value.ToString()));
        }

        protected void orientationRotationTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            SetObjectOrientationWithCommand(float.Parse(orientationRotationTrackBar.Value.ToString()),
                float.Parse(inclinationTrackbar.Value.ToString()));
        }

        protected void orientationRotationTrackBar_Scroll(object sender, EventArgs e)
        {
            if (SelectedOrientationObject != null && SelectedOrientationObject is IObjectOrientation && (orientationRotationTrackBar.Value != (int)Math.Round(SelectedOrientationObject.Azimuth)))
            {
                UpdateObjectOrienation(orientationRotationTrackBar.Value, inclinationTrackbar.Value);
            }
        }

        protected void inclincationTrackbar_Scroll(object sender, EventArgs e)
        {
            if (SelectedOrientationObject != null && SelectedOrientationObject is IObjectOrientation && (inclinationTrackbar.Value != (int)Math.Round(SelectedOrientationObject.Zenith)))
            {
                UpdateObjectOrienation(orientationRotationTrackBar.Value, inclinationTrackbar.Value);
            }
        }

        protected void UpdateOrientationFromTextboxes(object sender, EventArgs e)
        {
            float rot = 0;
            float inc = 0;
            if ((SelectedOrientationObject != null) && (((orientationRotationTextBox.Text != null) && (orientationRotationTextBox.Text != "")) || ((inclinationTextBox.Text != "")
                && (inclinationTextBox.Text != ""))))
            {
                if (!float.TryParse(orientationRotationTextBox.Text, out rot))
                {
                    orientationRotationTextBox.Text = "0";
                }
                if (!float.TryParse(inclinationTextBox.Text, out inc))
                {
                    inclinationTextBox.Text = "0";
                }

                while (rot > 180 || rot < -180)
                {
                    if (rot > 180)
                    {
                        rot -= 360;
                    }
                    else
                    {
                        if (rot < -180)
                        {
                            rot += 360;
                        }
                    }
                }

                while (inc > 90 || inc < -90)
                {
                    if (inc > 90)
                    {
                        inc -= 180;
                    }
                    else
                    {
                        if (inc < -90)
                        {
                            inc += 180;
                        }
                    }
                }
                if ((rot != SelectedOrientationObject.Azimuth || inc != SelectedOrientationObject.Zenith))
                {
                    SetObjectOrientationWithCommand(rot, inc);
                }
            }
        }

        public void UpdateOrientationPanel(float r, float i)
        {
            while (r > 180 || r < -180)
            {
                if (r > 180)
                {
                    r -= 360;
                }
                else
                {
                    if (r < -180)
                    {
                        r += 360;
                    }
                }
            }
            while (i > 90 || i < -90)
            {
                if (i > 90)
                {
                    i -= 180;
                }
                else
                {
                    if (i < -90)
                    {
                        i += 180;
                    }
                }
            }
            int rot = (int)Math.Round(r);
            orientationUpdate = true;
            if (float.Parse(orientationRotationTextBox.Text) != rot)
            {
                orientationRotationTextBox.Text = rot.ToString();
            }
            if (orientationRotationTrackBar.Value != rot)
            {
                orientationRotationTrackBar.Value = rot;
            }
            int inc = (int)Math.Round(i);
            if (int.Parse(inclinationTextBox.Text) != inc)
            {
                inclinationTextBox.Text = inc.ToString();
            }
            if (inclinationTrackbar.Value != inc)
            {
                inclinationTrackbar.Value = inc;
            }
            orientationUpdate = false;
        }

        public void UpdateOrientationPanel(IObjectOrientation obj)
        {
            int rot = (int)Math.Round(obj.Azimuth);
            int inc = (int)Math.Round(obj.Zenith);

            if (float.Parse(orientationRotationTextBox.Text) != rot)
            {
                orientationRotationTextBox.Text = rot.ToString();
            }
            if (orientationRotationTrackBar.Value != rot)
            {
                orientationRotationTrackBar.Value = rot;
            }
            if (float.Parse(inclinationTextBox.Text) != inc)
            {
                inclinationTextBox.Text = inc.ToString();
            }
            if (inclinationTrackbar.Value != inc)
            {
                inclinationTrackbar.Value = inc;
            }
        }

        public IObjectOrientation SelectedOrientationObject
        {
            get
            {
                if (SelectedObject != null && SelectedObject.Count == 1)
                {
                    return SelectedObject[0] as IObjectOrientation;
                }
                return null;
            }
        }

        public void UpdateObjectOrienation(float azimuth, float zenith)
        {
            if (SelectedOrientationObject != null && SelectedOrientationObject is IObjectOrientation)
            {
                if (SelectedOrientationObject.Display != null)
                {
                    SelectedOrientationObject.Display.UpdateOrientation(azimuth, zenith);
                }
                else
                {
                    switch ((SelectedOrientationObject as IWorldObject).ObjectType)
                    {
                        case "DirectionalLight":
                            (SelectedOrientationObject as DirectionalLight).SetDirection(azimuth, zenith);
                            break;
                        case "GlobalDirectionalLight":
                            (SelectedOrientationObject as GlobalDirectionalLight).SetDirection(azimuth, zenith);
                            break;
                    }
                }
            }
        }

        #endregion Orientation Panel

        #region Unique Object Names

        private static int uniqueIDCount = 0;

        /// <summary>
        /// The Axiom engine requires that entities and 
        /// GetUniqueName() provides a mechanism for the application to create unique names for
        /// objects that will be placed in the 3d world, while not exposing 
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static string GetUniqueName(string objectType, string objectName)
        {
            string ret = string.Format("{0} - {1} - {2}", objectType, objectName, uniqueIDCount);
            uniqueIDCount++;

            return ret;
        }

        #endregion Unique Object Names

        #region Picking Methods

        public Vector3 PickTerrain(int x, int y)
        {
            Ray pointRay = camera.GetCameraToViewportRay((float)x / (float)window.Width, (float)y / (float)window.Height);
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(camera.GetCameraToViewportRay((float)x / (float)window.Width,
                (float)y / (float)window.Height));

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.FirstTerrain;
            List<RaySceneQueryResultEntry> results = null;
            try
            {
                results = q.Execute();
            }
            catch(Exception e)
            {
                log.DebugFormat("Exception in PickTerrain:\r\n{0)", e.ToString());
            }

            if (results != null && results.Count > 0)
            {
                RaySceneQueryResultEntry result = results[0];
                return (result.worldFragment.SingleIntersection);
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Vector3 PickTerrain(Ray pointRay)
        {
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(pointRay);

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.FirstTerrain;
            List<RaySceneQueryResultEntry> results = q.Execute();

            if (results.Count > 0)
            {
                RaySceneQueryResultEntry result = results[0];
                return (result.worldFragment.SingleIntersection);
            }
            else
            {

                return Vector3.Zero;
            }
        }

        public float GetTerrainHeight(float x, float z)
        {
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(new Ray(new Vector3(x, 0, z), Vector3.NegativeUnitY));
            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.Height;
            List<RaySceneQueryResultEntry> results = q.Execute();

            RaySceneQueryResultEntry result = results[0];

            return result.worldFragment.SingleIntersection.y;
        }

        public IWorldObject PickObject(int x, int y)
        {
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(camera.GetCameraToViewportRay((float)x / (float)window.Width,
                (float)y / (float)window.Height));

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.Entities;

            List<RaySceneQueryResultEntry> results = q.Execute();

            int i = 0;
            foreach (RaySceneQueryResultEntry result in results)
            {
                if (DisplayObject.LookupName(result.SceneObject.Name).WorldViewSelectable)
                {
                    return DisplayObject.LookupName(result.SceneObject.Name);
                }
                i++;
            }
            return null;
        }




        public IWorldObject PickObjectByTriangle(int x, int y)
        {
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(camera.GetCameraToViewportRay((float)x / (float)window.Width,
                (float)y / (float)window.Height));

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.AllEntityTriangles;
            List<RaySceneQueryResultEntry> results = q.Execute();

            foreach (RaySceneQueryResultEntry result in results)
            {
                if (result.SceneObject != null && DisplayObject.LookupName(result.SceneObject.Name) != null && DisplayObject.LookupName(result.SceneObject.Name).WorldViewSelectable)
                {
                    mouseGroundCoorPanel.Text = result.SceneObject.Name;
                    return DisplayObject.LookupName(result.SceneObject.Name);
                }
            }
            mouseGroundCoorPanel.Text = "";
            return null;
        }

        public bool PickEntityTriangle(int x, int y, out Vector3 pos)
        {

            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(camera.GetCameraToViewportRay((float)x / (float)window.Width,
                (float)y / (float)window.Height));

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.FirstEntityTriangle;
            List<RaySceneQueryResultEntry> results = q.Execute();

            if (results.Count > 0)
            {
                RaySceneQueryResultEntry result = results[0];

                pos = result.worldFragment.SingleIntersection;
                return true;
            }
            else
            {
                pos = Vector3.Zero;
                return false;
            }
        }


        public Vector3 ObjectPlacementLocation(Vector3 objPosition)
        {

            Ray pointRay = new Ray(new Vector3(objPosition.x, camera.Position.y, objPosition.z), Vector3.NegativeUnitY);
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(pointRay);
            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.AllEntityTriangles;
            List<RaySceneQueryResultEntry> results = q.Execute();

            if (results.Count > 0)
            {
                foreach (RaySceneQueryResultEntry result in results)
                {
                    IWorldObject obj = DisplayObject.LookupName(result.SceneObject.Name);
                    if (obj is StaticObject && obj.AcceptObjectPlacement)
                    {
                        Vector3 position = pointRay.Origin + (pointRay.Direction * result.Distance);
                        return position;
                    }
                }
            }
            return PickTerrain(pointRay);
        }

        public Vector3 ObjectPlacementLocation(int x, int y)
        {
            Ray pointRay = camera.GetCameraToViewportRay((float)x / (float)window.Width,
                (float)y / (float)window.Height);
            Axiom.Core.RaySceneQuery q = scene.CreateRayQuery(pointRay);

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.AllEntityTriangles;
            List<RaySceneQueryResultEntry> results = q.Execute();

            if (results.Count > 0)
            {
                foreach (RaySceneQueryResultEntry result in results)
                {
                    if (result.SceneObject != null && DisplayObject.LookupName(result.SceneObject.Name) != null && DisplayObject.LookupName(result.SceneObject.Name).AcceptObjectPlacement)
                    {
                        Vector3 position = pointRay.Origin + (pointRay.Direction * result.Distance);
                        return position;
                    }
                }
            }
            return PickTerrain(x, y);
        }

        public string PickTriangleAndTerrain(int x, int y)
        {

            string s = "Terrain: " + PickTerrain(x, y).ToString();
            Vector3 pos;
            if (PickEntityTriangle(x, y, out pos))
                s += " Model: " + pos.ToString();
            return s;
        }



        #endregion Picking Methods

        #region StatusBar Access Methods

        public void StatusBarAddItem(ToolStripItem item)
        {
            statusStrip1.Items.Add(item);
            ToolStripSeparator newSep = new ToolStripSeparator();
            statusStrip1.Items.Add(newSep);
        }

        public void StatusBarRemoveItem(ToolStripItem item)
        {
            int index = statusStrip1.Items.IndexOf(item);
            statusStrip1.Items.RemoveAt(index + 1);
            statusStrip1.Items.Remove(item);
        }

        #endregion // StatusBar Access Methods

        #region Missing Asset List System

        public bool CheckAssetFileExists(string name)
        {
            return ResourceManager.HasCommonResourceData(name);
        }

        public bool CheckMaterialExists(string name)
        {
            if (name == null)
            {
                return true;
            }
            else
            {
                return MaterialManager.Instance.HasResource(name);
            }
        }

        public bool CheckParticleExists(string name)
        {
            return Axiom.ParticleSystems.ParticleSystemManager.Instance.ParticleSystems.ContainsKey(name);
        }

        public void AddMissingAsset(string s)
        {
            if (!missingAssetList.Contains(s))
            {
                missingAssetList.Add(s);
            }
        }

        #endregion Missing Asset List System

        #region Popup Message System

        public void AddPopupMessage(string s)
        {
            messageList.Add(s);
        }

        public void ClearPopupMessages()
        {
            messageList.Clear();
        }

        public void DisplayPopupMessages(string caption)
        {
            foreach (string s in messageList)
            {
                MessageBox.Show(s, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            ClearPopupMessages();
        }

        #endregion Popup Message System

        public static void LaunchDoc(string anchor)
        {
            string target = String.Format("{0}#{1}", helpURL, anchor);
            System.Diagnostics.Process.Start(target);
        }

        public static void LaunchFeedback()
        {
            System.Diagnostics.Process.Start(feedbackURL);
        }

        public static void LaunchReleaseNotes()
        {
            System.Diagnostics.Process.Start(releaseNoteURL);
        }

        public void axiomControl_GotFocus(object sender, EventArgs e)
        {
            mainSplitContainer.Panel1.BackColor = Color.Black;
        }

        public void axiomControl_LostFocus(object sender, EventArgs e)
        {
            mainSplitContainer.Panel1.BackColor = Color.Gray;
        }

        public void HelpMenuButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null)
            {
                String anchorString = button.Tag as string;

                LaunchDoc(anchorString);
            }
        }

        public void CameraObjectDirMenuButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null)
            {
                DirectionAndObject dirObj = button.Tag as DirectionAndObject;

                dirObj.SetCamera(camera);
            }
        }

        public void CameraMarkerDirMenuButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null)
            {
                DirectionAndMarker dirObj = button.Tag as DirectionAndMarker;
                dirObj.SetCamera(camera);
            }
        }

        public void CommandMenuButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null)
            {
                ICommandFactory commandFactory = button.Tag as ICommandFactory;
                if (commandFactory != null)
                {
                    ICommand commandInstance = commandFactory.CreateCommand();
                    if (commandInstance != null)
                    {
                        commandInstance.Execute();
                        if (commandInstance.Undoable())
                        {
                            undoRedo.PushCommand(commandInstance);
                        }
                    }
                }
            }
        }

        public void RemoveCollectionFromSceneButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null && button.Tag != null)
            {
                (button.Tag as WorldObjectCollection).RemoveFromScene();
            }
        }

        public void AddCollectionToSceneButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button != null && button.Tag != null)
            {
                (button.Tag as WorldObjectCollection).AddToScene();
            }
        }

        public void copyToClipboardMenuButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.Clear();
            string copyString = "";

            if (SelectedObject.Count > 0)
            {
                foreach (IWorldObject obj in SelectedObject)
                {
                    copyString += obj.ObjectAsString;
                }
                System.Windows.Forms.Clipboard.SetText(copyString);
            }
        }


        public void CollectionButton_clicked(object sender, EventArgs e)
        {
            List<IObjectChangeCollection> changeObjList = new List<IObjectChangeCollection>();
            IObjectCollectionParent toCollection = ((ToolStripItem)sender).Tag as IObjectCollectionParent;
            foreach (IWorldObject changeObj in SelectedObject)
            {
                changeObjList.Add((changeObj as IObjectChangeCollection));
            }
            ICommandFactory commandFactory = new ChangeCollectionCommandFactory(WorldEditor.Instance, changeObjList, toCollection);
            if (commandFactory != null)
            {
                ICommand commandInstance = commandFactory.CreateCommand();
                if (commandInstance != null)
                {
                    commandInstance.Execute();
                    if (commandInstance.Undoable())
                    {
                        undoRedo.PushCommand(commandInstance);
                    }
                }

            }
        }

        private bool canLoad()
        {
            bool rv = canExit("You have unsaved changes, would you like to save the file before you load new world?");
            if (!rv)
            {
                return rv;
            }
            return true;
        }

        private float CameraAzimuth
        {
            get
            {
                return cameraAzimuth;
            }
            set
            {
                cameraAzimuth = value;
                while (cameraAzimuth > 180)
                {
                    cameraAzimuth -= 360;
                }
                while (cameraAzimuth < -180)
                {
                    cameraAzimuth += 360;
                }
            }
        }


        private float CameraZenith
        {
            get
            {
                return cameraZenith;
            }
            set
            {
                cameraZenith = value;
                if (cameraZenith > 89)
                {
                    cameraZenith = 89;
                }
                if (cameraZenith < -89)
                {
                    cameraZenith = -89;
                }
            }
        }

        private float CameraRadius
        {
            get
            {
                return cameraRadius;
            }
            set
            {
                cameraRadius = value;
                if (cameraRadius < 0)
                {
                    cameraRadius = 0;
                }
            }
        }


        private void PositionCameraToObject(IWorldObject obj)
        {
            if (!(obj is IObjectCameraLockable))
            {
                return;
            }
            IObjectCameraLockable lockObj = obj as IObjectCameraLockable;
            if (lockObj.Radius == 1.0 && lockObj.Center.IsZero)
            {
                return;
            }
            CameraRadius = lockObj.Radius + 5000f;
            CameraAzimuth = 0f;
            CameraZenith = -44f;

            Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(CameraAzimuth), Vector3.UnitY);
            Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(CameraZenith), Vector3.UnitX);
            Matrix3 camMatrix = (azimuthRotation * zenithRotation).ToRotationMatrix();

            // Put the camera at the correct radius from the model
            Vector3 relativeCameraPos = camMatrix * (CameraRadius * Vector3.UnitZ);

            // Look at a point that is 1.8m above the player's base - this should be
            // around the character's head.
            camera.Position = relativeCameraPos + lockObj.Center;
            camera.LookAt(lockObj.Center);
        }

        private void PositionCamera()
        {
            if (SelectedObject[0] is IObjectCameraLockable && SelectedObject.Count == 1)
            {
                Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(CameraAzimuth), Vector3.UnitY);
                Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(CameraZenith), Vector3.UnitX);
                Matrix3 camMatrix = (azimuthRotation * zenithRotation).ToRotationMatrix();

                // Put the camera at the correct radius from the model
                Vector3 relativeCameraPos = camMatrix * (CameraRadius * Vector3.UnitZ);

                // Look at a point that is 1.8m above the player's base - this should be
                // around the character's head.

                Vector3 objLoc = (SelectedObject[0] as IObjectCameraLockable).Center;
                camera.Position = relativeCameraPos + objLoc;
                camera.LookAt(objLoc);
            }
        }


        private bool canExit(String msg)
        {
            if (undoRedo.Dirty)
            {
                DialogResult dlgRes = MessageBox.Show(
                    msg,
                    "Save Changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                switch (dlgRes)
                {
                    case DialogResult.Yes:
                        EventArgs e = new EventArgs();
                        saveWorldToolStripMenuItem_Click(this, e);
                        break;
                    case DialogResult.Cancel:
                        return false;
                    case DialogResult.No:
                        break;

                }
            }
            return true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            quit = canExit("You have unsaved changes, would you like to save the file before you exit?");
        }


        private void recentFilesToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            recentFilesToolStripMenuItem.DropDownItems.Clear();
            char[] letters = new char[5] { 'a', 'b', 'c', 'd', 'e' };
            foreach (char c in letters)
            {
                string ret = Registry.GetValue(config.RecentFileListBaseRegistryKey, c.ToString(), "") as string;
                if (ret != null && !String.Equals(ret, ""))
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(ret.Substring(ret.LastIndexOf('\\') + 1));
                    item.Tag = ret;
                    item.Click += new System.EventHandler(recentFilesMenuItem_Click);
                    recentFilesToolStripMenuItem.DropDownItems.Add(item);
                }
            }
        }

        private void recentFilesMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (canLoad())
            {
                if (worldRoot != null)
                {
                    ClearWorld();
                }
                LoadWorld(item.Tag as String);
            }
        }

        private void wireFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayWireFrame = !DisplayWireFrame;
        }

        private void viewToolStripMenu_DropDownOpening(object sender, EventArgs e)
        {
            wireFrameToolStripMenuItem.Checked = DisplayWireFrame;
            displayOceanToolStripMenuItem.Checked = DisplayOcean;
            displayTerrainToolStripMenuItem.Checked = DisplayTerrain;
            displayFogEffectsToolStripMenuItem.Checked = displayFog;
            displayLightEffectsToolStripMenuItem.Checked = displayLights;
            renderLeavesToolStripMenuItem.Checked = RenderLeaves;
            cameraFollowsTerrainToolStripMenuItem.Checked = cameraTerrainFollow;
            displayParticleEffectsToolStripMenuItem.Checked = displayParticleEffects;
            displayBoundaryMarkersToolStripMenuItem.Checked = displayBoundaryMarkers;
            displayRoadMarkersToolStripMenuItem.Checked = displayRoadMarkers;
            displayMarkerPointsToolStripMenuItem.Checked = displayMarkerPoints;
            disableAllMarkersToolStripMenuItem.Checked = disableAllMarkers;
            cameraAboveTerrainToolStripMenuItem.Checked = cameraAboveTerrain;
            displayTerrainDecalsToolStripMenuItem.Checked = displayTerrainDecals;
            displayPointLightMarkersViewMenuItem.Checked = displayPointLightMarker;
            displayPointLightAttenuationCirclesMenuItem.Checked = displayPointLightCircles;
            lockCameraToSelectedObjectToolStipMenuItem.Checked = lockCameraToObject;
            displayShadowsMenuItem.Checked = DisplayShadows;
        }

        private void displayDisplayTerrainDecalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayTerrainDecals = !displayTerrainDecals;
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.TerrainDecal, UpdateHint.DisplayDecal);
            }
        }

        private void displayOceanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayOcean = !DisplayOcean;
        }

        private void displayTerrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayTerrain = !DisplayTerrain;
        }

        private void displayParticleEffectsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DisplayParticleEffects = !DisplayParticleEffects;
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.ParticleEffect, UpdateHint.Display);
            }
        }

        private void displayBoundaryMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayBoundaryMarkers = !DisplayBoundaryMarkers;
            if (DisplayBoundaryMarkers)
            {
                disableAllMarkers = false;
            }
            else
            {
                if (!DisplayMarkerPoints && !DisplayRoadMarkers && !DisplayPointLightMarker)
                {
                    disableAllMarkers = true;
                }
            }
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.Regions, UpdateHint.DisplayMarker);
            }
        }

        private void displayRoadMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayRoadMarkers = !DisplayRoadMarkers;
            if (DisplayRoadMarkers)
            {
                disableAllMarkers = false;
            }
            else
            {
                if (!DisplayBoundaryMarkers && !DisplayMarkerPoints && !DisplayPointLightMarker)
                {

                    disableAllMarkers = true;
                }
            }
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.Road, UpdateHint.DisplayMarker);
            }
        }

        private void displayMarkerPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayMarkerPoints = !DisplayMarkerPoints;
            if (DisplayMarkerPoints)
            {
                disableAllMarkers = false;
            }
            else
            {
                if (!DisplayBoundaryMarkers && !DisplayRoadMarkers && !DisplayPointLightMarker)
                {
                    disableAllMarkers = true;
                }
            }
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.Markers, UpdateHint.DisplayMarker);
            }
        }

        private void disableAllMarkersToolStripMenuItem_Click(object Sender, EventArgs e)
        {
            disableAllMarkers = !disableAllMarkers;
            if (disableAllMarkers)
            {
                displayRoadMarkers = false;
                displayBoundaryMarkers = false;
                displayMarkerPoints = false;
                displayPointLightMarker = false;
            }
            else
            {
                displayRoadMarkers = true;
                displayBoundaryMarkers = true;
                displayMarkerPoints = true;
                displayPointLightMarker = true;
            }
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.All, UpdateHint.DisplayMarker);
            }
        }

        private void displayPointLightMarkerMenuItem_Click(object sender, EventArgs e)
        {
            displayPointLightMarker = !DisplayPointLightMarker;
            if (DisplayPointLightMarker)
            {
                disableAllMarkers = false;
            }
            else
            {
                if (!DisplayBoundaryMarkers && !DisplayRoadMarkers && !DisplayMarkerPoints)
                {
                    disableAllMarkers = true;
                }
            }
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.PointLight, UpdateHint.DisplayMarker);
            }
        }

        private void displayPointLightAttenuationCirclesMenuItem_Click(object sender, EventArgs e)
        {
            displayPointLightCircles = !displayPointLightCircles;
            if (worldRoot != null)
            {
                worldRoot.UpdateScene(UpdateTypes.PointLight, UpdateHint.DisplayPointLightCircles);
            }
        }

        private void lockCameraToSelectedObjectToolStipMenuItem_Click(object sender, EventArgs e)
        {
            lockCameraToObject = !lockCameraToObject;
            if (lockCameraToObject && SelectedObject != null && SelectedObject.Count == 1 && SelectedObject[0] is IObjectCameraLockable)
            {
                PositionCameraToObject(SelectedObject[0]);
            }
        }

        private void cameraAboveTerrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cameraAboveTerrain = !cameraAboveTerrain;
        }

        private void displayTerrainDecalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayTerrainDecals = !displayTerrainDecals;
        }


        private void newWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool rv = canLoad();
            if (!rv)
            {
                return;
            }
            if (worldRoot != null)
            {
                ClearWorld();
            }
            using (TextPromptDialog dlg = new TextPromptDialog("Create a New World", "Enter the name of your new World:", "World"))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    worldRoot = new WorldRoot(dlg.UserText, worldTreeView, this);
                    saveWorldButton.Enabled = true;
                    saveWorldToolStripMenuItem.Enabled = true;
                    saveWorldAsMenuItem.Enabled = true;
                    worldRoot.AddToTree(null);
                    worldRoot.AddToScene();
                    setWorldDefaults();
                    undoRedo.ClearUndoRedo();
                    worldRoot.Node.Select();
                    worldRoot.Node.Expand();
                }
            }
        }

        private void axiomControl_MouseDown(object sender, MouseEventArgs e)
        {
            bool dragable = true;
            Keys key = Keys.None;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    key = Keys.LButton;
                    break;
                case MouseButtons.Right:
                    key = Keys.RButton;
                    break;
                case MouseButtons.Middle:
                    key = Keys.MButton;
                    break;
            }
            EventObject eo = commandMapping.GetMatch(Control.ModifierKeys, key, "down", "world view");
            if (eo != null)
            {
                eo.Handler(this, new EventArgs());
            }

            newMouseX = e.X;
            newMouseY = e.Y;
            if (e.Button == MouseButtons.Left)
            {
                leftMouseClick = true;
            }
            if (e.Button == MouseButtons.Right)
            {
                rightMouseClick = true;
            }
            if (e.Button == MouseButtons.Middle)
            {
                middleMouseClick = true;
            }

            hitObject = PickObjectByTriangle(e.X, e.Y);
            mouseDragObject = new List<IWorldObject>();
            foreach (IWorldObject obj in SelectedObject)
            {
                mouseDragObject.Add(obj);
            }
            if (mouseSelectObject || mouseSelectMultipleObject)
            {
                if (mouseSelectObject)
                {

                    if (hitObject != null)
                    {
                        if (SelectedObject.Count > 0)
                        {
                            for (int i = SelectedObject.Count - 1; i >= 0; i--)
                            {
                                IWorldObject obj = SelectedObject[i];
                                if (obj.Node != null)
                                {
                                    obj.Node.UnSelect();
                                }
                            }
                        }
                        if (hitObject != null && hitObject.Node != null)
                        {
                            hitObject.Node.Select();
                        }
                        if (!mouseDragEvent)
                        {
                            if (SelectedObject.Count > 0)
                            {
                                foreach (IWorldObject obj in SelectedObject)
                                {
                                    if (!(obj is IObjectDrag))
                                    {
                                        dragable = false;
                                    }
                                    else
                                    {
                                        if (obj is MPPoint && SelectedObject.Count > 1)
                                        {
                                            return;
                                        }
                                    }
                                }
                                if (dragable)
                                {
                                    mouseDownTimer = new System.Windows.Forms.Timer();
                                    mouseDownTimer.Tick += new EventHandler(mouseDownTimerEvent);
                                    mouseDownTimer.Interval = 500;
                                    mouseDownTimer.Enabled = true;
                                    mouseDownPosition = new Vector2(e.X, e.Y);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            hitObject.Node.Select();
                        }
                    }
                }
                if (mouseSelectMultipleObject && hitObject != null)
                {
                    WorldTreeView.AddSelectedNode(hitObject.Node);
                }

            }
        }


        private void mouseDownTimerEvent(object source, EventArgs e)
        {
            ICommandFactory cmdFac;
            ICommand cmd;
            if (!mouseDragEvent)
            {
                if (((MousePosition.X > (mouseDownPosition.x + 10)) ||
                    (MousePosition.X < (mouseDownPosition.x - 10)) ||
                    (MousePosition.Y > (mouseDownPosition.y + 10)) ||
                    (MousePosition.Y < (mouseDownPosition.y - 10))) &&
                    mouseDragObject != null && mouseSelectObject && hitObject != null)
                {
                    mouseDragEvent = true;
                    foreach (IWorldObject obj in mouseDragObject)
                    {
                        if (obj.IsTopLevel || obj is MPPoint)
                        {
                            obj.Node.Select();
                        }
                    }
                    if (mouseDragObject.Count == 1 && mouseDragObject[0] is MPPoint)
                    {
                        cmdFac = new DragMPPointCommandFactory(this, mouseDragObject[0]);
                    }
                    else
                    {
                        foreach (IWorldObject obj in mouseDragObject)
                        {
                            if (!obj.IsTopLevel)
                            {
                                mouseDragEvent = false;
                                return;
                            }
                        }
                        cmdFac = new DragObjectsInWorldCommandFactory(this, mouseDragObject, hitObject);
                    }
                    cmd = cmdFac.CreateCommand();
                    this.ExecuteCommand(cmd);
                }
                else
                {
                    return;
                }
            }
        }


        private void axiomControl_MouseUp(object sender, MouseEventArgs e)
        {
            Keys key = Keys.None;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    key = Keys.LButton;
                    break;
                case MouseButtons.Right:
                    key = Keys.RButton;
                    break;
                case MouseButtons.Middle:
                    key = Keys.MButton;
                    break;
            }

            if (e.Button == MouseButtons.Right)
            {
                rightMouseRelease = true;
            }

            if (e.Button == MouseButtons.Left)
            {
                leftMouseRelease = true;
            }

            if (e.Button == MouseButtons.Middle)
            {
                middleMouseRelease = true;
            }
            EventObject eo = commandMapping.GetMatch(Control.ModifierKeys, key, "up", "world view");
            if (eo != null)
            {
                eo.Handler(this, new EventArgs());
            }

            if (mouseSelectObject)
            {
                if (mouseDownTimer != null)
                {
                    // The timer hasn't expired so this is a click rather than an drag
                    mouseDownTimer.Enabled = false;
                    mouseDownTimer = null;
                }
                else
                {
                    if (mouseDragEvent && mouseDragObject != null && mouseDragObject.Count > 0)
                    {
                        // The user has dropped the object so we update the position and end the drag
                        mouseDragEvent = false;
                    }
                }
            }
            else if (mouseTurnCamera)
            {
                mouseTurnCamera = false;
            }

            newMouseX = e.X;
            newMouseY = e.Y;

            mouseSelectObject = false;
            mouseSelectMultipleObject = false;
        }

        private void axiomControl_MouseMove(object sender, MouseEventArgs e)
        {
            newMouseX = e.X;
            newMouseY = e.Y;
        }

        private void axiomControl_MouseWheel(object sender, MouseEventArgs e)
        {
            newMouseX = e.X;
            newMouseY = e.Y;

            mouseWheelDelta += e.Delta;
        }



        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            EventObject eo = commandMapping.GetMatch(e.Modifiers, e.KeyCode, "down", "treeView");
            if (eo != null)
            {
                eo.Handler(this, new EventArgs());
                e.Handled = true;
            }
        }


        #region WorldEditor control events



        private void worldEditorTakeScreenShotOn(object sender, EventArgs e)
        {
            takeScreenShot = true;
        }

        private void worldEditorTakeScreenShotOff(object sender, EventArgs e)
        {
            takeScreenShot = false;
        }

        private void worldEditorCutObjects(object sender, EventArgs e)
        {
            cutToolStripMenuItemClicked_Clicked(this, new EventArgs());
        }

        private void worldEditorCopyObjects(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Clicked(this, new EventArgs());
        }

        private void worldEditorPasteObjects(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Clicked(this, new EventArgs());
        }

        private void worldEditorMoveCameraUp(object sender, EventArgs e)
        {
            moveCameraUp = true;
        }

        private void worldEditorMoveCameraUpStop(object sender, EventArgs e)
        {
            moveCameraUp = false;
        }

        private void worldEditorMoveCameraDown(object sender, EventArgs e)
        {
            moveCameraDown = true;
        }

        private void worldEditorMoveCameraDownStop(object sender, EventArgs e)
        {
            moveCameraDown = false;
        }

        #endregion WorldEditor control events

        #region Axiom window control events

        private void editMenuControlMappingEditorItem_clicked(object sender, EventArgs e)
        {

            UserCommandMapping workingCopy = commandMapping.Clone();
            using (UserCommandEditor dialog = new UserCommandEditor(workingCopy, Config.MouseCapableContexts))
            {
                DialogResult dlgres = dialog.ShowDialog();
                if (dlgres == DialogResult.OK)
                {
                    commandMapping.Dispose();
                    commandMapping = workingCopy;
                    XmlWriter w = XmlWriter.Create(this.Config.AltCommandBindingsFilePath, xmlWriterSettings);
                    commandMapping.ToXml(w);
                    w.Close();
                }
            }
            createKeybindings();
        }

        private void axiomWarpCamera(object sender, EventArgs e)
        {
            warpCamera = true;
        }

        private void axiomCameraLookDownSouth(object sender, EventArgs e)
        {
            cameraLookDownSouth = true;
        }

        private void axiomOnCameraLockObject(object sender, EventArgs e)
        {
            lockCameraToObject = true;
        }

        private void axiomOffCameraLockObject(object sender, EventArgs e)
        {
            lockCameraToObject = false;
        }

        private void axiomToggleMoveCameraOnPlane(object sender, EventArgs e)
        {
            moveOnPlane = !moveOnPlane;
        }

        private void axiomOnCameraOnPlane(object sender, EventArgs e)
        {
            moveOnPlane = true;
        }

        private void axiomOffCameraOnPlane(object sender, EventArgs e)
        {
            moveOnPlane = false;
        }

        private void axiomCameraMoveSpeedUp(object sender, EventArgs e)
        {

            camSpeed += camSpeedIncrement;
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    (item as ToolStripMenuItem).Checked = false;
                }
            }
            if (!incrementSpeedTimer.Enabled)
            {
                incrementSpeedTimer.Interval = 500;
                incrementSpeedTimer.Tick -= axiomCameraMoveSpeedDown;
                incrementSpeedTimer.Tick += axiomCameraMoveSpeedUp;
                incrementSpeedTimer.Enabled = true;
            }
            incrementSpeedTimer.Start();
        }

        private void axiomCameraMoveSpeedUpStop(object sender, EventArgs e)
        {
            incrementSpeedTimer.Stop();
            incrementSpeedTimer.Enabled = false;
        }

        private void axiomCameraMoveSpeedDown(object sender, EventArgs e)
        {
            camSpeed = camSpeed - camSpeedIncrement;
            if (camSpeed <= 1000f)
            {
                camSpeed = 1000f;
            }
            if (!accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    (item as ToolStripMenuItem).Checked = false;
                }
            }
            if (!incrementSpeedTimer.Enabled)
            {
                incrementSpeedTimer.Interval = 500;
                incrementSpeedTimer.Tick -= axiomCameraMoveSpeedUp;
                incrementSpeedTimer.Tick += axiomCameraMoveSpeedDown;
                incrementSpeedTimer.Enabled = true;
            }
            incrementSpeedTimer.Start();
        }


        private void axiomCameraMoveSpeedDownStop(object sender, EventArgs e)
        {
            incrementSpeedTimer.Stop();
            incrementSpeedTimer.Enabled = false;
        }


        private void axiomToggleAccelerate(object sender, EventArgs e)
        {
            accelerateCamera = !accelerateCamera;
            setToolStripAccelSpeedDropDownMenu();
        }


        private void setToolStripMWMDropDownMenu()
        {
            mWMMenuItemPreset1.Text = presetMWM1.ToString();
            mWMMenuItemPreset2.Text = presetMWM2.ToString();
            mWMMenuItemPreset3.Text = presetMWM3.ToString();
            mWMMenuItemPreset4.Text = presetMWM4.ToString();
        }

        private void setToolStripAccelSpeedDropDownMenu()
        {
            if (accelerateCamera)
            {
                cameraSpeedAccelDropDownButton.ToolTipText = "Preset Camera Acceleration Rate Drop Down";
                cameraSpeedAccelDropDownButton.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.cameraAcceleration_icon;
                accelDropDownPreset1MenuItem.Text = String.Format("{0} m/(s^2)", presetCameraAccel1 / 1000f);
                accelDropDownPreset1MenuItem.Visible = true;
                accelDropDownPreset1MenuItem.Name = "accelDropDownPreset1MenuItem";
                accelDropDownPreset2MenuItem.Text = String.Format("{0} m/(s^2)", presetCameraAccel2 / 1000f);
                accelDropDownPreset2MenuItem.Visible = true;
                accelDropDownPreset2MenuItem.Name = "accelDropDownPreset2MenuItem";
                accelDropDownPreset3MenuItem.Text = String.Format("{0} m/(s^2)", presetCameraAccel3 / 1000f);
                accelDropDownPreset3MenuItem.Name = "accelDropDownPreset3MenuItem";
                accelDropDownPreset3MenuItem.Visible = true;
                accelDropDownPreset4MenuItem.Text = String.Format("{0} m/(s^2)", presetCameraAccel4 / 1000f);
                accelDropDownPreset4MenuItem.Visible = true;
                accelDropDownPreset4MenuItem.Name = "accelDropDownPreset1MenuItem";
                cameraSpeedDropDownPreset1MenuItem.Visible = false;
                cameraSpeedDropDownPreset2MenuItem.Visible = false;
                cameraSpeedDropDownPreset3MenuItem.Visible = false;
                cameraSpeedDropDownPreset4MenuItem.Visible = false;

            }
            else
            {
                cameraSpeedAccelDropDownButton.ToolTipText = "Preset Camera Speed Drop Down";
                cameraSpeedAccelDropDownButton.Image = global::Multiverse.Tools.WorldEditor.Properties.Resources.cameraSpeed_icon;
                cameraSpeedDropDownPreset1MenuItem.Text = String.Format("{0}m/s", presetCameraSpeed1 / 1000f);
                cameraSpeedDropDownPreset1MenuItem.Visible = true;
                cameraSpeedDropDownPreset1MenuItem.Name = "cameraSpeedDropDownPreset1MenuItem";
                cameraSpeedDropDownPreset2MenuItem.Text = String.Format("{0}m/s", presetCameraSpeed2 / 1000f);
                cameraSpeedDropDownPreset2MenuItem.Visible = true;
                cameraSpeedDropDownPreset2MenuItem.Name = "cameraSpeedDropDownPreset2MenuItem";
                cameraSpeedDropDownPreset3MenuItem.Text = String.Format("{0}m/s", presetCameraSpeed3 / 1000f);
                cameraSpeedDropDownPreset3MenuItem.Visible = true;
                cameraSpeedDropDownPreset3MenuItem.Name = "cameraSpeedDropDownPreset3MenuItem";
                cameraSpeedDropDownPreset4MenuItem.Text = String.Format("{0}m/s", presetCameraSpeed4 / 1000f);
                cameraSpeedDropDownPreset4MenuItem.Visible = true;
                cameraSpeedDropDownPreset4MenuItem.Name = "cameraSpeedDropDownPreset4MenuItem";
                accelDropDownPreset1MenuItem.Visible = false;
                accelDropDownPreset2MenuItem.Visible = false;
                accelDropDownPreset3MenuItem.Visible = false;
                accelDropDownPreset4MenuItem.Visible = false;
            }
        }

        private void axiomCameraAccelerateUp(object sender, EventArgs e)
        {
            camAccel += 1000f;
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    (item as ToolStripMenuItem).Checked = false;
                }
            }
            if (!incrementAccelTimer.Enabled)
            {
                incrementAccelTimer.Interval = 500;
                incrementAccelTimer.Tick -= axiomCameraAccelerateDown;
                incrementAccelTimer.Tick += axiomCameraAccelerateUp;
                incrementAccelTimer.Enabled = true;
            }
            incrementAccelTimer.Start();
        }


        private void axiomCameraAccelerateUpStop(object sender, EventArgs e)
        {
            incrementAccelTimer.Stop();
            incrementAccelTimer.Enabled = false;
            incrementAccelTimer.Tick -= axiomCameraAccelerateUp;
        }


        private void axiomCameraAccelerateDown(object sender, EventArgs e)
        {
            camAccel = camAccel - 1000f;
            if (camAccel < 1000f)
            {
                camAccel = 1000f;
            }
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    (item as ToolStripMenuItem).Checked = false;
                }
            }
            if (!incrementAccelTimer.Enabled)
            {
                incrementAccelTimer.Interval = 500;
                incrementAccelTimer.Tick -= axiomCameraAccelerateUp;
                incrementAccelTimer.Tick += axiomCameraAccelerateDown;
                incrementAccelTimer.Enabled = true;
            }
            incrementAccelTimer.Start();
        }

        private void axiomMoveCameraToSelectedMarker(object sender, EventArgs e)
        {
            if (SelectedObject.Count == 1 && SelectedObject[0] is Waypoint)
            {
                camera.Position = (SelectedObject[0] as Waypoint).Position;
                camera.Orientation = (SelectedObject[0] as Waypoint).Orientation;
            }
        }



        private void axiomCameraAccelerateDownStop(object sender, EventArgs e)
        {
            incrementAccelTimer.Stop();
            incrementAccelTimer.Enabled = false;
            incrementAccelTimer.Tick -= axiomCameraAccelerateDown;
        }

        private void axiomMoveCameraForward(object sender, EventArgs e)
        {
            moveForward = true;
        }

        private void axiomMoveCameraForwardStop(object sender, EventArgs e)
        {
            moveForward = false;
        }

        private void axiomMoveCameraBack(object sender, EventArgs e)
        {
            moveBack = true;
        }

        private void axiomMoveCameraBackStop(object sender, EventArgs e)
        {
            moveBack = false;
        }

        private void axiomMoveCameraLeft(object sender, EventArgs e)
        {
            moveLeft = true;
        }

        private void axiomMoveCameraLeftStop(object sender, EventArgs e)
        {
            moveLeft = false;
        }

        private void axiomMoveCameraRight(object sender, EventArgs e)
        {
            moveRight = true;

        }

        private void axiomMoveCameraRightStop(object sender, EventArgs e)
        {
            moveRight = false;
        }

        private void axiomMoveCameraStop(object sender, EventArgs e)
        {
            moveBack = false;
            moveForward = false;
            moveRight = false;
            moveLeft = false;
            moveCameraUp = false;
            moveCameraDown = false;
        }



        private void axiomTurnCameraLeft(object sender, EventArgs e)
        {
            turnCameraLeft = true;
            turnCameraRight = false;
        }

        private void axiomTurnCameraLeftStop(object sender, EventArgs e)
        {
            turnCameraLeft = false;
        }


        private void axiomTurnCameraRight(object sender, EventArgs e)
        {
            turnCameraRight = true;
            turnCameraLeft = false;
        }

        private void axiomTurnCameraRightStop(object sender, EventArgs e)
        {
            turnCameraRight = false;
        }

        private void axiomTurnCameraDown(object sender, EventArgs e)
        {
            turnCameraDown = true;
            turnCameraUp = false;
        }

        private void axiomTurnCameraDownStop(object sender, EventArgs e)
        {
            turnCameraDown = false;
        }


        private void axiomTurnCameraUp(object sender, EventArgs e)
        {
            turnCameraUp = true;
            turnCameraDown = false;
        }

        private void axiomTurnCameraUpStop(object sender, EventArgs e)
        {
            turnCameraUp = false;
        }

        private void axiomMouseSelectObject(object sender, EventArgs e)
        {
            mouseSelectObject = true;
        }

        private void axiomMouseSelectMultipleObject(object sender, EventArgs e)
        {
            mouseSelectMultipleObject = true;
        }

        private void axiomMouseTurnCamera(object sender, EventArgs e)
        {
            mouseTurnCamera = true;
        }

        private void axiomSetAccelPreset1(object sender, EventArgs e)
        {
            defaultCamAccelSpeed = presetCameraAccel1;
            camAccel = presetCameraAccel1;
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    if (!ReferenceEquals(item, accelDropDownPreset1MenuItem))
                    {
                        ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                        thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                    }
                }
                accelDropDownPreset1MenuItem.Font = new Font(accelDropDownPreset1MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetAccelPreset2(object sender, EventArgs e)
        {
            defaultCamAccelSpeed = presetCameraAccel2;
            camAccel = presetCameraAccel2;
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    if (!ReferenceEquals(item, accelDropDownPreset2MenuItem))
                    {
                        ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                        thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                    }
                }
                accelDropDownPreset2MenuItem.Font = new Font(accelDropDownPreset2MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetAccelPreset3(object sender, EventArgs e)
        {
            defaultCamAccelSpeed = presetCameraAccel3;
            camAccel = presetCameraAccel3;
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    if (!ReferenceEquals(item, accelDropDownPreset3MenuItem))
                    {
                        ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                        thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                    }
                }
                accelDropDownPreset3MenuItem.Font = new Font(accelDropDownPreset3MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetAccelPreset4(object sender, EventArgs e)
        {
            defaultCamAccelSpeed = presetCameraAccel4;
            camAccel = presetCameraAccel4;
            if (accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    if (!ReferenceEquals(item, accelDropDownPreset4MenuItem))
                    {
                        ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                        thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                    }
                }
                accelDropDownPreset4MenuItem.Font = new Font(accelDropDownPreset4MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetSpeedPreset1(object sender, EventArgs e)
        {
            camSpeed = presetCameraSpeed1;
            if (!accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                    thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                }
                cameraSpeedDropDownPreset1MenuItem.Font = new Font(cameraSpeedDropDownPreset1MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetSpeedPreset2(object sender, EventArgs e)
        {
            camSpeed = presetCameraSpeed2;
            if (!accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                    thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                }
                cameraSpeedDropDownPreset2MenuItem.Font = new Font(cameraSpeedDropDownPreset2MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetSpeedPreset3(object sender, EventArgs e)
        {
            camSpeed = presetCameraSpeed3;
            if (!accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                    thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                }
                cameraSpeedDropDownPreset3MenuItem.Font = new Font(cameraSpeedDropDownPreset3MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetSpeedPreset4(object sender, EventArgs e)
        {
            camSpeed = presetCameraSpeed4;
            if (!accelerateCamera)
            {
                foreach (ToolStripItem item in cameraSpeedAccelDropDownButton.DropDownItems)
                {
                    ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                    thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
                }
                cameraSpeedDropDownPreset4MenuItem.Font = new Font(cameraSpeedDropDownPreset4MenuItem.Font, FontStyle.Bold);
            }
        }

        private void axiomSetPresetMWM1(object sender, EventArgs e)
        {
            mouseWheelMultiplier = presetMWM1;
            foreach (ToolStripItem item in mouseWheelMultiplierDropDownButton.DropDownItems)
            {
                ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
            }
            mWMMenuItemPreset1.Font = new Font(mWMMenuItemPreset1.Font, FontStyle.Bold);
        }

        private void axiomSetPresetMWM2(object sender, EventArgs e)
        {
            mouseWheelMultiplier = presetMWM2;
            foreach (ToolStripItem item in mouseWheelMultiplierDropDownButton.DropDownItems)
            {
                ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
            }
            mWMMenuItemPreset2.Font = new Font(mWMMenuItemPreset2.Font, FontStyle.Bold);
        }

        private void axiomSetPresetMWM3(object sender, EventArgs e)
        {
            mouseWheelMultiplier = presetMWM3;

            foreach (ToolStripItem item in mouseWheelMultiplierDropDownButton.DropDownItems)
            {
                ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
            }
            mWMMenuItemPreset3.Font = new Font(mWMMenuItemPreset3.Font, FontStyle.Bold);
        }

        private void axiomSetPresetMWM4(object sender, EventArgs e)
        {
            mouseWheelMultiplier = presetMWM4;
            foreach (ToolStripItem item in mouseWheelMultiplierDropDownButton.DropDownItems)
            {
                ToolStripMenuItem thisItem = (item as ToolStripMenuItem);
                thisItem.Font = new Font(thisItem.Font, FontStyle.Regular);
            }
            mWMMenuItemPreset4.Font = new Font(mWMMenuItemPreset4.Font, FontStyle.Bold); ;
        }


        public MouseButtons MouseSelectButton
        {
            get
            {
                return mouseSelectButton;
            }
        }

        #endregion Axiom window control events


        #region Tree View control events

        public void treeViewMoveToNextNode(object sender, EventArgs e)
        {
            if (SelectedNodes.Count >= 1)
            {
                WorldTreeNode node = worldTreeView.FindNextNode(SelectedNodes[0] as MultiSelectTreeNode) as WorldTreeNode;
                if (node != null)
                {
                    foreach (WorldTreeNode tnode in SelectedNodes)
                    {
                        tnode.UnSelect();
                    }
                    node.Select();
                }
            }
            else
            {
                if (worldTreeView.Nodes.Count != 0)
                {
                    ((worldTreeView.Nodes[0]) as WorldTreeNode).Select();
                }
            }
        }

        public void treeViewMoveToPrevNode(object sender, EventArgs e)
        {
            if (SelectedNodes.Count >= 1)
            {
                WorldTreeNode node = worldTreeView.FindPrevNode(SelectedNodes[0] as MultiSelectTreeNode) as WorldTreeNode;
                if (node != null)
                {
                    foreach (WorldTreeNode tnode in SelectedNodes)
                    {
                        tnode.UnSelect();
                    }
                    node.Select();
                }
            }
            else
            {
                {
                    if (worldTreeView.Nodes.Count != 0)
                    {
                        (worldTreeView.Nodes[0] as WorldTreeNode).Select();
                    }
                }
            }
        }

        private void treeViewExpandSelectedNode(object sender, EventArgs e)
        {
            foreach (WorldTreeNode node in SelectedNodes)
            {
                if (!node.IsExpanded && node.Nodes.Count > 0)
                {
                    node.Expand();
                }
            }
        }

        private void treeViewExpandAllSelectedNode(object sender, EventArgs e)
        {
            foreach (WorldTreeNode node in SelectedNodes)
            {
                if (node.Nodes.Count > 0)
                {
                    node.ExpandAll();
                }
            }
        }

        private void treeViewCollapseSelectedNode(object sender, EventArgs e)
        {
            foreach (WorldTreeNode node in SelectedNodes)
            {
                if (node.IsExpanded && node.Nodes.Count > 0)
                {
                    node.Collapse();
                }
            }
        }

        #endregion Tree View control events

        private void axiomControl_KeyDown(object sender, KeyEventArgs e)
        {
            EventObject eo = commandMapping.GetMatch(e.Modifiers, e.KeyCode, "down", "world view");
            if (eo != null && !e.Handled)
            {
                eo.Handler(this, new EventArgs());
                e.Handled = true;
            }
        }

        private void axiomControl_KeyUp(object sender, KeyEventArgs e)
        {
            EventObject eo = commandMapping.GetMatch(e.Modifiers, e.KeyCode, "up", "world view");
            if (eo != null && !e.Handled)
            {
                eo.Handler(this, new EventArgs());
                e.Handled = true;
            }
        }

        private void WorldEditor_KeyDown(object sender, KeyEventArgs e)
        {
            EventObject eo = commandMapping.GetMatch(e.Modifiers, e.KeyCode, "down", "global");
            if (eo != null)
            {
                eo.Handler(this, new EventArgs());
                e.Handled = true;
            }
        }

        private void WorldEditor_KeyUp(object sender, KeyEventArgs e)
        {
            EventObject eo = commandMapping.GetMatch(e.Modifiers, e.KeyCode, "up", "global");
            if (eo != null)
            {
                eo.Handler(this, new EventArgs());
                e.Handled = true;
            }
        }

        private void axiomControl_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (mouseCaptureLost != null)
            {
                mouseCaptureLost(this);
            }
        }

        private void worldTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            for (int i = toolStrip1.Items.Count - 1; i >= 0; i--)
            {
                ToolStripItem item = toolStrip1.Items[i];
                if (item.Alignment == System.Windows.Forms.ToolStripItemAlignment.Right)
                {
                    toolStrip1.Items.Remove(item);
                }
            }
            if (SelectedObject.Count == 1)
            {
                WorldTreeNode node = SelectedNodes[0];

                if (SelectedObject[0].ButtonBar != null)
                {
                    for (int i = SelectedObject[0].ButtonBar.Count - 1; i >= 0; i--)
                    {
                        ToolStripItem button = (ToolStripItem)(SelectedObject[0].ButtonBar[i]);
                        toolStrip1.Items.Add(button);
                    }
                }
                // set the propertyGrid to the selected object
                nodePropertyGrid.SelectedObject = SelectedObject[0];

                // enable and update position panel
                if (SelectedObject[0] is IObjectPosition)
                {
                    positionPanel.Enabled = true;

                    // enable/disable y up/down buttons
                    yUpButton.Enabled = SelectedPositionObject.AllowYChange;
                    yDownButton.Enabled = SelectedPositionObject.AllowYChange;
                    positionYTextBox.Enabled = SelectedPositionObject.AllowYChange;

                    UpdatePositionPanel(SelectedPositionObject.Position);
                }
                else
                {
                    positionPanel.Enabled = false;
                    UpdatePositionPanel(Vector3.Zero);
                }

                // enable and update scale panel
                if (SelectedObject[0] is IObjectScale)
                {
                    scalePanel.Enabled = true;
                    UpdateScalePanel(SelectedScaleObject.Scale);
                }
                else
                {
                    scalePanel.Enabled = false;
                    UpdateScalePanel(1);
                }

                // enable and update rotation panel
                if (SelectedObject[0] is IObjectRotation)
                {
                    rotationPanel.Enabled = true;
                    UpdateRotationPanel(SelectedRotationObject.Rotation);
                }
                else
                {
                    rotationPanel.Enabled = false;
                    UpdateRotationPanel(0);
                }

                // enable and update Orientation panel
                if (SelectedObject[0] is IObjectOrientation)
                {
                    orientationPanel.Enabled = true;
                    UpdateOrientationPanel(SelectedOrientationObject.Azimuth, SelectedOrientationObject.Zenith);
                }
                else
                {
                    orientationPanel.Enabled = false;
                    UpdateOrientationPanel(0, 0);
                }
                if (SelectedObject[0] is IObjectCameraLockable && lockCameraToObject)
                {
                    PositionCameraToObject(SelectedObject[0]);
                }
            }
            else
            {
                if (SelectedObject.Count > 1 || SelectedObject.Count == 0)
                {
                    nodePropertyGrid.SelectedObject = null;
                    orientationPanel.Enabled = false;
                    UpdateOrientationPanel(0, 0);
                    rotationPanel.Enabled = false;
                    UpdateRotationPanel(0);
                    scalePanel.Enabled = false;
                    UpdateScalePanel(1);
                    positionPanel.Enabled = false;
                    UpdatePositionPanel(Vector3.Zero);
                    CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                    foreach (ToolStripButton button in menuBuilder.MultiSelectButtonBar())
                    {
                        toolStrip1.Items.Add(button);
                    }
                }
            }
        }

        private void nodePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            PropertyChangeCommand cmd = new PropertyChangeCommand(this, nodePropertyGrid.SelectedObject, e.ChangedItem.PropertyDescriptor, e.ChangedItem.Value, e.OldValue);

            // we don't execute the command here because the property has already changed the value
            undoRedo.PushCommand(cmd);
        }

        private void saveWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (worldRoot != null && worldRoot.WorldFilePath != null && !String.Equals(worldRoot.WorldFilePath, ""))
            {
                SaveWorld(worldRoot.WorldFilePath);
                undoRedo.ResetDirty();
            }
            else
            {
                saveAsWorldToolStripMenuItem_Click(sender, e);
            }
        }

        private void saveAsWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                string filename = "";

                dlg.Title = "Save World";
                dlg.DefaultExt = "mvw";
                if (worldRoot != null && worldRoot.WorldFilePath != null)
                {
                    filename = worldRoot.WorldFilePath;
                    dlg.FileName = worldRoot.WorldFilePath;
                    foreach (WorldObjectCollection obj in worldRoot.WorldObjectCollections)
                    {
                        obj.Filename = "";
                    }
                }
                else
                {
                    if (worldRoot == null)
                    {
                        return;
                    }
                }
                dlg.Filter = "Multiverse World files (*.mvw)|*.mvw|xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    worldRoot.WorldFilePath = dlg.FileName;
                    string title = String.Format("World Editor : {0}", dlg.FileName.Substring(dlg.FileName.LastIndexOf("\\") + 1));
                    SaveWorld(dlg.FileName);
                    this.Text = title;
                    undoRedo.ResetDirty();
                }
            }
        }

        private void packageWorldAssetsMenuItem_Click(object sender, EventArgs e)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter w = new StreamWriter(memoryStream);
            worldRoot.ToManifest(w);
            w.Flush();
            memoryStream.Seek(0L, SeekOrigin.Begin);
            StreamReader r = new StreamReader(memoryStream);
            List<string> worldAssetLines = RepositoryClass.ReadStreamLines(r);
            assetPackagerForm packagerForm = new assetPackagerForm(worldAssetLines);
            packagerForm.Text = "Package Assets For World " + worldRoot.Name;
            packagerForm.ShowDialog();
        }

        private void worldTreeView_DoubleClick(object sender, EventArgs e)
        {
            CameraToSelectedObject();
        }

        private void loadWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool rv = canLoad();
            if (rv)
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Load World";
                    dlg.DefaultExt = "mvw";
                    dlg.Filter = "Multiverse World files (*.mvw)|*.mvw|xml files (*.xml)|*.xml|All files (*.*)|*.*";
                    dlg.RestoreDirectory = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (worldRoot != null)
                        {
                            ClearWorld();
                        }
                        LoadWorld(dlg.FileName);
                        undoRedo.ClearUndoRedo();
                    }
                }
            }
            return;
        }

        private void loadWorldRootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool rv = canLoad();
            if (rv)
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Load World Root";
                    dlg.DefaultExt = "mvw";
                    dlg.Filter = "Multiverse World files (*.mvw)|*.mvw|xml files (*.xml)|*.xml|All files (*.*)|*.*";
                    dlg.RestoreDirectory = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        ClearWorld();
                        LoadWorldRoot(dlg.FileName);
                        undoRedo.ClearUndoRedo();
                    }
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoRedo.CanUndo)
            {
                undoRedo.Undo();
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoRedo.CanRedo)
            {
                undoRedo.Redo();
            }
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = undoRedo.CanUndo;
            redoToolStripMenuItem.Enabled = undoRedo.CanRedo;
        }

        private void axiomControl_Resize(object sender, EventArgs e)
        {
            if (window != null)
            {
                int height = axiomControl.Height;
                int width = axiomControl.Width;
                window.Resize(width, height);
                viewport.SetDimensions(0, 0, 1.0f, 1.0f);
                camera.AspectRatio = (float)window.Width / window.Height;
            }
        }

        private void oneMMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (oneMMRadioButton.Checked)
            {
                movementScale = 1f;
            }
        }

        private void oneCMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (oneCMRadioButton.Checked)
            {
                movementScale = 10f;
            }
        }

        private void tenCMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (tenCMRadioButton.Checked)
            {
                movementScale = 100f;
            }
        }

        private void oneMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (oneMRadioButton.Checked)
            {
                movementScale = 1000f;
            }
        }


        private void yDownButton_mouseDown(object sender, MouseEventArgs e)
        {
            if (SelectedPositionObject != null)
            {
                Vector3 v = SelectedPositionObject.Display.Position;
                v.y = v.y - movementScale;
                UpdatePositionPanel(v);
                if (positionPanelButtonMouseDownTimer == null)
                {
                    positionPanelButtonMouseDownTimer = new System.Windows.Forms.Timer();
                    positionPanelButtonMouseDownTimer.Interval = 1000;
                    positionPanelButtonMouseDownTimer.Tick += new EventHandler(positionPanelButtonMouseDownTimer_tick);
                    positionPanelButtonMouseDownTimer.Tag = ("y-" as object);
                    positionPanelButtonMouseDownTimer.Enabled = true;
                }
                UpdateDisplayObjectFromPanel();
            }
        }

        private void yUpButton_mouseDown(object sender, MouseEventArgs e)
        {
            if (SelectedPositionObject != null)
            {
                Vector3 v = SelectedPositionObject.Display.Position;
                v.y += movementScale;
                UpdatePositionPanel(v);

                if (positionPanelButtonMouseDownTimer == null)
                {
                    positionPanelButtonMouseDownTimer = new System.Windows.Forms.Timer();
                    positionPanelButtonMouseDownTimer.Interval = 1000;
                    positionPanelButtonMouseDownTimer.Tick += new EventHandler(positionPanelButtonMouseDownTimer_tick);
                    positionPanelButtonMouseDownTimer.Tag = ("y+" as object);
                    positionPanelButtonMouseDownTimer.Enabled = true;
                }
                UpdateDisplayObjectFromPanel();
            }
        }


        private void zDownButton_mouseDown(object sender, MouseEventArgs e)
        {
            if (SelectedPositionObject != null)
            {
                Vector3 v;
                if (!(SelectedPositionObject is TerrainDecal))
                {
                    v = SelectedPositionObject.Display.Position;
                    v.z = v.z - movementScale;
                    v.y = SelectedPositionObject.TerrainOffset + GetTerrainHeight(v.x, v.z);
                }
                else
                {
                    v = new Vector3();
                    (SelectedPositionObject as TerrainDecal).Decal.PosZ = (SelectedPositionObject as TerrainDecal).Decal.PosZ - movementScale;
                    v.x = (SelectedPositionObject as TerrainDecal).Decal.PosX;
                    v.z = (SelectedPositionObject as TerrainDecal).Decal.PosZ;
                    v.y = GetTerrainHeight(v.x, v.z);
                }
                UpdatePositionPanel(v);

                if (positionPanelButtonMouseDownTimer == null)
                {
                    positionPanelButtonMouseDownTimer = new System.Windows.Forms.Timer();
                    positionPanelButtonMouseDownTimer.Interval = 1000;
                    positionPanelButtonMouseDownTimer.Tick += new EventHandler(positionPanelButtonMouseDownTimer_tick);
                    positionPanelButtonMouseDownTimer.Tag = ("z-" as object);
                    positionPanelButtonMouseDownTimer.Enabled = true;
                }
                UpdateDisplayObjectFromPanel();
            }
        }

        private void zUpButton_mouseDown(object sender, MouseEventArgs e)
        {
            if (SelectedPositionObject != null)
            {
                Vector3 v;
                if (!(SelectedPositionObject is TerrainDecal))
                {
                    v = SelectedPositionObject.Display.Position;
                    v.z += movementScale;
                    v.y = SelectedPositionObject.TerrainOffset + GetTerrainHeight(v.x, v.z);
                }
                else
                {
                    v = new Vector3();
                    (SelectedPositionObject as TerrainDecal).Decal.PosZ += movementScale;
                    v.x = (SelectedPositionObject as TerrainDecal).Decal.PosX;
                    v.z = (SelectedPositionObject as TerrainDecal).Decal.PosZ;
                    v.y = GetTerrainHeight(v.x, v.z);
                }
                UpdatePositionPanel(v);

                if (positionPanelButtonMouseDownTimer == null)
                {
                    positionPanelButtonMouseDownTimer = new System.Windows.Forms.Timer();
                    positionPanelButtonMouseDownTimer.Interval = 1000;
                    positionPanelButtonMouseDownTimer.Tick += new EventHandler(positionPanelButtonMouseDownTimer_tick);
                    positionPanelButtonMouseDownTimer.Tag = ("z+" as object);
                    positionPanelButtonMouseDownTimer.Enabled = true;
                }
                UpdateDisplayObjectFromPanel();
            }
        }

        private void xDownButton_mouseDown(object sender, MouseEventArgs e)
        {
            if (SelectedPositionObject != null)
            {
                Vector3 v;
                if (!(SelectedPositionObject is TerrainDecal))
                {
                    v = SelectedPositionObject.Display.Position;
                    v.x = v.x - movementScale;
                    v.y = SelectedPositionObject.TerrainOffset + GetTerrainHeight(v.x, v.z);
                }
                else
                {
                    v = new Vector3();
                    (SelectedPositionObject as TerrainDecal).Decal.PosX -= movementScale;
                    v.x = (SelectedPositionObject as TerrainDecal).Decal.PosX;
                    v.z = (SelectedPositionObject as TerrainDecal).Decal.PosZ;
                    v.y = GetTerrainHeight(v.x, v.z);
                }
                UpdatePositionPanel(v);

                if (positionPanelButtonMouseDownTimer == null)
                {
                    positionPanelButtonMouseDownTimer = new System.Windows.Forms.Timer();
                    positionPanelButtonMouseDownTimer.Interval = 1000;
                    positionPanelButtonMouseDownTimer.Tick += new EventHandler(positionPanelButtonMouseDownTimer_tick);
                    positionPanelButtonMouseDownTimer.Tag = ("x-" as object);
                    positionPanelButtonMouseDownTimer.Enabled = true;
                }
                UpdateDisplayObjectFromPanel();
            }
        }

        private void xUpButton_mouseDown(object sender, MouseEventArgs e)
        {
            Vector3 v;
            if (SelectedPositionObject != null)
            {
                if (!(SelectedPositionObject is TerrainDecal))
                {
                    v = SelectedPositionObject.Display.Position;
                    v.x = v.x + movementScale;
                    v.y = SelectedPositionObject.TerrainOffset + GetTerrainHeight(v.x, v.z);
                }
                else
                {
                    v = new Vector3();
                    (SelectedPositionObject as TerrainDecal).Decal.PosX += movementScale;
                    v.x = (SelectedPositionObject as TerrainDecal).Decal.PosX;
                    v.z = (SelectedPositionObject as TerrainDecal).Decal.PosZ;
                    v.y = GetTerrainHeight(v.x, v.z);
                }
                UpdatePositionPanel(v);
                if (positionPanelButtonMouseDownTimer == null)
                {
                    positionPanelButtonMouseDownTimer = new System.Windows.Forms.Timer();
                    positionPanelButtonMouseDownTimer.Interval = 1000;
                    positionPanelButtonMouseDownTimer.Tick += new EventHandler(positionPanelButtonMouseDownTimer_tick);
                    positionPanelButtonMouseDownTimer.Tag = ("x+" as object);
                    positionPanelButtonMouseDownTimer.Enabled = true;
                }
                UpdateDisplayObjectFromPanel();
            }
        }

        private void positionPanelButton_mouseUp(object sender, MouseEventArgs e)
        {
            positionPanelButtonMouseDownTimer.Dispose();
            positionPanelButtonMouseDownTimer = null;
            UpdatePositionFromPanel();
        }


        private void positionPanelButtonMouseDownTimer_tick(object sender, EventArgs e)
        {
            string axis = positionPanelButtonMouseDownTimer.Tag as string;
            positionPanelButtonMouseDownTimer.Interval = 333;
            MouseEventArgs em = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            switch (axis)
            {
                case "z-":
                    zDownButton_mouseDown(sender, em);
                    break;
                case "z+":
                    zUpButton_mouseDown(sender, em);
                    break;
                case "x-":
                    xDownButton_mouseDown(sender, em);
                    break;
                case "x+":
                    xUpButton_mouseDown(sender, em);
                    break;
                case "y-":
                    yDownButton_mouseDown(sender, em);
                    break;
                case "y+":
                    yUpButton_mouseDown(sender, em);
                    break;
            }
            return;
        }

        private void onePercentScaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (onePercentScaleRadioButton.Checked)
            {
                scalePercentage = 1.01f;
            }
        }

        private void tenPercentScaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (tenPercentScaleRadioButton.Checked)
            {
                scalePercentage = 1.1f;
            }
        }

        private void oneHundredPercentScaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (oneHundredPercentScaleRadioButton.Checked)
            {
                scalePercentage = 2f;
            }
        }

        private void scaleUpButton_Click(object sender, EventArgs e)
        {
            if (SelectedScaleObject != null)
            {
                float s = SelectedScaleObject.Scale;

                s = s * scalePercentage;

                SetObjectScaleWithCommand(s);

                UpdateScalePanel(s);
            }
        }

        private void scaleDownButton_Click(object sender, EventArgs e)
        {
            if (SelectedScaleObject != null)
            {
                float s = SelectedScaleObject.Scale;

                s = s / scalePercentage;

                SetObjectScaleWithCommand(s);

                UpdateScalePanel(s);
            }
        }

        private void scaleTextBox_Leave(object sender, EventArgs e)
        {
            UpdateScaleFromPanel();
        }

        private void scaleTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                UpdateScaleFromPanel();
            }
        }

        private void positionXTextBox_Leave(object sender, EventArgs e)
        {
            UpdatePositionFromPanel();
        }

        private void positionYTextBox_Leave(object sender, EventArgs e)
        {
            UpdatePositionFromPanel();
        }

        private void positionZTextBox_Leave(object sender, EventArgs e)
        {
            UpdatePositionFromPanel();
        }

        private void positionZTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                UpdatePositionFromPanel();
            }
        }

        private void positionYTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                UpdatePositionFromPanel();
            }
        }

        private void positionXTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                UpdatePositionFromPanel();
            }
        }

        private void rotationTextBox_Leave(object sender, EventArgs e)
        {
            UpdateRotationFromTextbox();
        }

        private void rotationTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                UpdateRotationFromTextbox();
            }
        }

        private void rotationTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateRotationFromTrackbar();
        }

        private void renderLeavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenderLeaves = !RenderLeaves;
        }

        private void AboutMenuItem_clicked(object sender, System.EventArgs e)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string msg = string.Format("Multiverse World Editor\n\nVersion: {0}\n\nCopyright 2006-2007 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse World Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void helpToolStripMenuItem_clicked(object sender, EventArgs e)
        {
            LaunchDoc("");
        }

        private void feedbackMenuItem_clicked(object sender, EventArgs e)
        {
            LaunchFeedback();
        }

        private void releaseNotesToolStripMenuItem_click(object sender, EventArgs e)
        {
            LaunchReleaseNotes();
        }

        private void WorldEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit = canExit("You have unsaved changes, would you like to save the file before you exit?");
        }

        private void cameraFollowsTerrainToolStripMenuItem_clicked(object sender, EventArgs e)
        {
            cameraTerrainFollow = !cameraTerrainFollow;
        }


        private void designateAssetRepositoryMenuItem_Click(object sender, EventArgs e)
        {
            DesignateRepository(true);
        }

        public bool DesignateRepository()
        {
            return DesignateRepository(false);
        }

        public bool ValidateAssetRepository(List<string> paths, Preferences_Dialog dlg)
        {
            List<string> validityLog = RepositoryClass.Instance.CheckForValidRepository(paths);

            if (validityLog.Count == 0)
            {
                // repository has been successfully set
                return true;
            }
            else
            {
                ErrorLogPopup(validityLog, "The directories you selected do not make up a valid respository.  The following errors were generated:\n\n", "Invalid Repository", MessageBoxButtons.OK);
                dlg.RepositoryDirectoryList = new List<string>(RepositoryClass.Instance.RepositoryDirectoryList);
                return false;
            }
        }

        public bool DesignateRepository(bool restart)
        {
            DialogResult result = designateRepositoriesDialog.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return false;
            }
            else if (restart)
            {
                MessageBox.Show("The Asset Repository has been successfully set.  You should restart the tool to get the new settings.",
                    "Repository Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return true;
        }

        protected void setMaximumFramesPerSecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveMaxFPSDialog saveMaxFPSDialog = new SaveMaxFPSDialog();
            if (saveMaxFPSDialog.ShowDialog() == DialogResult.OK)
            {
                int maxFPS = int.Parse(saveMaxFPSDialog.NumberBox.Text);
                Root.Instance.MaxFramesPerSecond = maxFPS;
            }
        }

        protected static string worldEditorKey = Registry.CurrentUser + "\\Software\\Multiverse\\WorldEditor";

        public void setMaxFPSInRegistry(int maxFPS)
        {
            Registry.SetValue(worldEditorKey, "MaxFPS", maxFPS);
        }

        protected void getMaxFPSFromRegistry()
        {
            Object maxFPS = Registry.GetValue(worldEditorKey, "MaxFPS", null);
            if (maxFPS != null)
            {
                Root.Instance.MaxFramesPerSecond = int.Parse(maxFPS.ToString());
                activeFps = Root.Instance.MaxFramesPerSecond;
            }

        }

        /// <summary>
        ///   Return the untransformed collision shapes associated
        ///   with a mesh.  If they are already encached, return them,
        ///   else look for a physics file that matches the mesh
        ///   file's name, and  load the information from that file to
        ///   build the collision shapes.
        /// </summary>
        public List<CollisionShape> FindMeshCollisionShapes(string meshName, Entity entity)
        {
            if (collisionManager == null)
            {
                Axiom.Core.LogManager.Instance.Write("DisplayObject.collisionManager is null!");
                return null;
            }
            List<CollisionShape> shapes;
            if (!WorldEditor.Instance.MeshCollisionShapes.TryGetValue(meshName, out shapes))
            {
                string physicsName = Path.GetFileNameWithoutExtension(meshName) + ".physics";
                // Create a set of collision shapes for the object
                shapes = new List<CollisionShape>();
                PhysicsData pd = new PhysicsData();
                PhysicsSerializer ps = new PhysicsSerializer();

                try
                {
                    Stream stream = ResourceManager.FindCommonResourceData(physicsName);
                    ps.ImportPhysics(pd, stream);
                    for (int i = 0; i < entity.SubEntityCount; i++)
                    {
                        SubEntity subEntity = entity.GetSubEntity(i);
                        if (subEntity.IsVisible)
                        {
                            string submeshName = subEntity.SubMesh.Name;
                            List<CollisionShape> subEntityShapes = pd.GetCollisionShapes(submeshName);
                            foreach (CollisionShape subShape in subEntityShapes)
                            {
                                // Clone the shape, and add to the list of
                                // untransformed shapes
                                shapes.Add(subShape);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // Unable to load physics data -- use a sphere or no collision data?
                    Axiom.Core.LogManager.Instance.Write("Unable to load physics data: " + e);
                    shapes = new List<CollisionShape>();
                }
                WorldEditor.Instance.MeshCollisionShapes[meshName] = shapes;
            }
            List<CollisionShape> newShapes = new List<CollisionShape>();
            foreach (CollisionShape shape in shapes)
                newShapes.Add(shape.Clone());
            return newShapes;
        }

        private void UpateOrientationFromTrackBars()
        {
            SelectedOrientationObject.SetDirection(orientationRotationTrackBar.Value, inclinationTrackbar.Value);
        }

        private void UpdateOrientationFromTextboxes()
        {
            SelectedOrientationObject.SetDirection(float.Parse(orientationRotationTextBox.Text), float.Parse(inclinationTextBox.Text));
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (worldRoot == null)
            {
                saveWorldAsMenuItem.Enabled = false;
                saveWorldButton.Enabled = false;
                packageWorldAssetsMenuItem.Enabled = false;
            }
            else
            {
                saveWorldToolStripMenuItem.Enabled = true;
                saveWorldAsMenuItem.Enabled = true;
                packageWorldAssetsMenuItem.Enabled = true;
            }
        }

        private void displayFogEffectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayFog = !displayFog;
        }

        private void displayLightEffectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayLights = !displayLights;
            worldRoot.UpdateScene(UpdateTypes.PointLight, UpdateHint.DisplayLight);
        }

        private void showCollisionVolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            collisionManager.ToggleRenderCollisionVolumes(scene, false);
        }

        public void MaybeChangeObjectCollisionVolumeRendering(IObjectPosition posObj, bool generate)
        {
            if (showCollisionVolToolStripMenuItem.Checked && posObj is StaticObject)
            {
                StaticObject staticObject = (StaticObject)posObj;
                if (generate)
                    staticObject.DisplayObject.AddCollisionObject();
                else
                    staticObject.DisplayObject.RemoveCollisionObject();
            }
        }

        private void WorldEditor_Resize(object sender, EventArgs e)
        {
            if (Root.Instance == null)
                return;
            Root.Instance.SuspendRendering = (this.WindowState == FormWindowState.Minimized);
        }

        private void setCameraNearDistanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (setCameraNearDialog == null)
                setCameraNearDialog = new SetCameraNearDialog(this);
            setCameraNearDialog.Show();
            setCameraNearDialog.BringToFront();
        }

        private void displayPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showPathDialog.Prepare();
            showPathDialog.ShowDialog();
        }

        public bool LogPathGeneration
        {
            get
            {
                return logPathGeneration;
            }
            set
            {
                logPathGeneration = value;
            }
        }

        private void displayShadowsMenuItem_Clicked(object sender, EventArgs e)
        {
            if (displayShadowsMenuItem.Checked)
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth;
            }
            else
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.None;
            }
            DisplayShadows = displayShadowsMenuItem.Checked;
        }

        private void undoButton_Clicked(object sender, EventArgs e)
        {
            undoRedo.Undo();
        }

        private void redoButton_Clicked(object sender, EventArgs e)
        {
            undoRedo.Redo();
        }

        private void editMenuTreeViewSearchItem_clicked(object sender, EventArgs e)
        {
            if (worldRoot != null && worldRoot.Node != null)
            {
                SearchDialog dlg = new SearchDialog(worldRoot.Node, worldTreeView.Nodes, this);
                dlg.Show();
            }
        }

        private void copyToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            bool dontCopy = false;
            foreach (IWorldObject obj in SelectedObject)
            {
                if (!(obj is IObjectCutCopy))
                {
                    dontCopy = true;
                }
            }
            if (!dontCopy)
            {
                ICommandFactory cmdFac = new CopyToClipboardCommandFactory(this, SelectedObject);
                ICommand cmd = cmdFac.CreateCommand();
                ExecuteCommand(cmd);
            }
        }

        private void cutToolStripMenuItemClicked_Clicked(object sender, EventArgs e)
        {

            bool dontCut = false;
            foreach (IWorldObject obj in SelectedObject)
            {
                if (!(obj is IObjectCutCopy))
                {
                    dontCut = true;
                }
            }
            if (!dontCut)
            {
                ICommandFactory cmdFac = new CutToClipboardCommandFactory(this, SelectedObject);
                ICommand cmd = cmdFac.CreateCommand();
                ExecuteCommand(cmd);
            }
        }

        private void pasteToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            if (clipboard.Count > 0)
            {
                ICommandFactory cmdFac = new PasteFromClipboardCommandFactory(this);
                ICommand cmd = cmdFac.CreateCommand();
                ExecuteCommand(cmd);
            }
        }

        private void deleteToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            foreach (IWorldObject obj in SelectedObject)
            {
                if (!(obj is IObjectDelete))
                {
                    return;
                }
                DeleteObjectsCommandFactory factory = new DeleteObjectsCommandFactory(this, SelectedObject);
                ICommand cmd = factory.CreateCommand();
                ExecuteCommand(cmd);
            }
        }


        private void editMenuPreferencesItem_clicked(object sender, EventArgs e)
        {
            object ret;
            using (Preferences_Dialog dlg = new Preferences_Dialog(this))
            {
                //Check Registry for previous settings. If some are missing fill in with defaults
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayOcean", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayOceanCheckbox = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayFog", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayFogEffects = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayLight", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.LightEffectsDisplay = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayShadows", (object)false);
                if (ret == null || String.Equals(ret.ToString(), "False"))
                {
                    ret = (object)false;
                }
                else
                {
                    ret = (object)true;
                }
                dlg.ShadowsDisplay = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayRegionMarkers", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayRegionMarkers = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayRoadMarkers", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayRoadMarkers = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayMarkerPoints", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayMarkerPoints = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "DisplayPointLightMarkers", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayPointLightMarkers = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "Disable All Markers", (object)false);
                if (ret == null || String.Equals(ret.ToString(), "False"))
                {
                    ret = (object)false;
                }
                else
                {
                    ret = (object)true;
                }
                dlg.DisableAllMarkerPoints = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "Display Terrain Decals", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.DisplayTerrainDecals = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraFollowsTerrain", (object)false);
                if (ret == null || String.Equals(ret.ToString(), "False"))
                {
                    ret = (object)false;
                }
                else
                {
                    ret = (object)true;
                }
                dlg.CameraFollowsTerrain = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraStaysAboveTerrain", (object)true);
                if (ret == null || String.Equals(ret.ToString(), "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.CameraStaysAboveTerrain = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraNearDistance", (object)1f);
                if (ret != null)
                {
                    dlg.CameraNearDistanceFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.CameraNearDistanceFloat = 1f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "LockCameraToObject", (object)false);
                if (ret == null || String.Equals(ret.ToString(), "False"))
                {
                    ret = (object)false;
                }
                else
                {
                    ret = (object)true;
                }
                dlg.LockCameraToObject = (bool)ret;

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "Disable Video Playback", (object)false);
                if (ret == null || String.Equals(ret.ToString(), "False"))
                {
                    ret = (object)false;
                }
                else
                {
                    ret = (object)true;
                }
                dlg.DisableVideoPlayback = (bool)ret;

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MaxFPS", (object)0);
                if (ret != null)
                {
                    uint maxfps;
                    if (uint.TryParse(ret.ToString(), out maxfps) && maxfps > 0)
                    {
                        dlg.MaxFramesPerSecondEnabled = true;
                        dlg.MaxFramesPerSesconduInt = maxfps;
                    }
                    else
                    {
                        dlg.MaxFramesPerSecondEnabled = false;
                    }
                }
                else
                {
                    dlg.MaxFramesPerSecondEnabled = false;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "AutoSaveEnable", (object)true);
                if (ret == null || String.Equals(ret, "True"))
                {
                    ret = (object)true;
                }
                else
                {
                    ret = (object)false;
                }
                dlg.AutoSaveEnabled = (bool)ret;
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "AutoSaveTime", (object)(30 * 60000));
                if (ret == null)
                {
                    dlg.AutoSaveTimeuInt = 30;
                }
                else
                {
                    dlg.AutoSaveTimeuInt = (uint.Parse(ret.ToString())) / 60000;
                }
                List<string> dirs = RepositoryClass.Instance.RepositoryDirectoryList;
                dlg.RepositoryDirectoryList = dirs;

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraDefaultSpeed", (object)(Config.DefaultCamSpeed));
                if (ret != null)
                {
                    dlg.CameraDefaultSpeedTextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.CameraDefaultSpeedTextBoxAsFloat = Config.DefaultCamSpeed / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraSpeedIncrement", (object)(Config.DefaultCamSpeedIncrement));
                if (ret != null)
                {
                    dlg.CameraSpeedIncrementTextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.CameraSpeedIncrementTextBoxAsFloat = Config.DefaultCamSpeedIncrement / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed1", (object)(Config.DefaultPresetCamSpeed1));
                if (ret != null)
                {
                    dlg.PresetCameraSpeed1TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraSpeed1TextBoxAsFloat = Config.DefaultPresetCamSpeed1 / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed2", (object)(Config.DefaultPresetCamSpeed2));
                if (ret != null)
                {
                    dlg.PresetCameraSpeed2TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraSpeed2TextBoxAsFloat = config.DefaultPresetCamSpeed2 / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed3", (object)(Config.DefaultPresetCamSpeed3));
                if (ret != null)
                {
                    dlg.PresetCameraSpeed3TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraSpeed3TextBoxAsFloat = Config.DefaultPresetCamSpeed3 / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed4", (object)(Config.DefaultPresetCamSpeed4));
                if (ret != null)
                {
                    dlg.PresetCameraSpeed4TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraSpeed4TextBoxAsFloat = Config.DefaultPresetCamSpeed4 / 1000f;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerate", (object)Config.DefaultAccelerateCamera);
                if (ret != null && String.Equals(ret, "False"))
                {
                    dlg.AccelerateCameraCheckBoxChecked = false;
                }
                else
                {
                    dlg.AccelerateCameraCheckBoxChecked = true;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerationRate", (object)(Config.DefaultCamAccelRate));
                if (ret != null)
                {
                    dlg.CameraAccelerationRateTextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.CameraAccelerationRateTextBoxAsFloat = Config.DefaultCamAccelRate / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerationIncrement", (object)(Config.DefaultCamAccelRateIncrement));
                if (ret != null)
                {
                    dlg.CameraAccelerationIncrementTextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.CameraAccelerationIncrementTextBoxAsFloat = Config.DefaultCamAccelRateIncrement / 1000f;
                }


                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate1", (object)(Config.DefaultPresetCamAccel1));
                if (ret != null)
                {
                    dlg.PresetCameraAcceleration1TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraAcceleration1TextBoxAsFloat = Config.DefaultPresetCamAccel1 / 1000f;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate2", (object)(Config.DefaultPresetCamAccel2));
                if (ret != null)
                {
                    dlg.PresetCameraAcceleration2TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraAcceleration2TextBoxAsFloat = Config.DefaultPresetCamAccel2 / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate3", (object)(Config.DefaultPresetCamAccel3));
                if (ret != null)
                {
                    dlg.PresetCameraAcceleration3TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraAcceleration3TextBoxAsFloat = Config.DefaultPresetCamAccel3 / 1000f;
                }
                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate4", (object)(Config.DefaultPresetCamAccel4));
                if (ret != null)
                {
                    dlg.PresetCameraAcceleration4TextBoxAsFloat = float.Parse(ret.ToString()) / 1000f;
                }
                else
                {
                    dlg.PresetCameraAcceleration4TextBoxAsFloat = Config.DefaultPresetCamAccel4 / 1000f;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "CameraTurnRate", (object)Config.DefaultCameraTurnRate);
                if (ret != null)
                {
                    dlg.CameraTurnRateTextBoxAsFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.CameraTurnRateTextBoxAsFloat = Config.DefaultCameraTurnRate;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MouseWheelMultiplier", (object)Config.DefaultMouseWheelMultiplier);
                if (ret != null)
                {
                    dlg.MouseWheelMultiplierTextBoxAsFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.MouseWheelMultiplierTextBoxAsFloat = Config.DefaultMouseWheelMultiplier;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset1", (object)Config.DefaultPresetMWM1);
                if (ret != null)
                {
                    dlg.Preset1MWMTextBoxAsFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.Preset1MWMTextBoxAsFloat = Config.DefaultPresetMWM1;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset2", (object)Config.DefaultPresetMWM2);
                if (ret != null)
                {
                    dlg.Preset2MWMTextBoxAsFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.Preset2MWMTextBoxAsFloat = Config.DefaultPresetMWM2;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset3", (object)Config.DefaultPresetMWM3);
                if (ret != null)
                {
                    dlg.Preset3MWMTextBoxAsFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.Preset3MWMTextBoxAsFloat = Config.DefaultPresetMWM3;
                }

                ret = Registry.GetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset4", (object)Config.DefaultPresetMWM4);
                if (ret != null)
                {
                    dlg.Preset4MWMTextBoxAsFloat = float.Parse(ret.ToString());
                }
                else
                {
                    dlg.Preset4MWMTextBoxAsFloat = Config.DefaultPresetMWM4;
                }
                // Show the dialog and if the dialog returns with an ok, set the registry settings and 
                // make them the current settings.

                DialogResult result;
                bool showAgain;
                do
                {
                    result = dlg.ShowDialog();
                    showAgain = false;
                    if (result == DialogResult.OK)
                    {
                        // do validation here
                        // if validation fails, set showAgain to true
                        showAgain = ((result == DialogResult.OK) && (!dlg.okButton_validating()));
                    }
                } while (showAgain);
                if (result == DialogResult.OK)
                {
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayOcean", (object)dlg.DisplayOceanCheckbox);
                    DisplayOcean = dlg.DisplayOceanCheckbox;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayFog", (object)dlg.DisplayFogEffects);
                    displayFog = dlg.DisplayFogEffects;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayLight", (object)dlg.LightEffectsDisplay);
                    displayLights = dlg.LightEffectsDisplay;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayShadows", (object)dlg.ShadowsDisplay);
                    if (Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique == Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth && !dlg.ShadowsDisplay)
                    {
                        Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.None;
                    }
                    if (Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique == Axiom.SceneManagers.Multiverse.ShadowTechnique.None && dlg.ShadowsDisplay)
                    {
                        Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.Depth;
                    }
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayRegionMarkers", (object)dlg.DisplayRegionMarkers);
                    displayBoundaryMarkers = dlg.DisplayRegionMarkers;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayRoadMarkers", (object)dlg.DisplayRoadMarkers);
                    displayRoadMarkers = dlg.DisplayRoadMarkers;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayMarkerPoints", (object)dlg.DisplayMarkerPoints);
                    displayMarkerPoints = dlg.DisplayMarkerPoints;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisplayPointLightMarkers", (object)dlg.DisplayPointLightMarkers);
                    displayPointLightMarker = dlg.DisplayPointLightMarkers;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "DisableAllMarkers", (object)dlg.DisableAllMarkerPoints);
                    disableAllMarkers = dlg.DisableAllMarkerPoints;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "Display Terrain Decals", (object)dlg.DisplayTerrainDecals);
                    displayTerrainDecals = dlg.DisplayTerrainDecals;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "Disable Video Playback", (object)dlg.DisableVideoPlayback);
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraFollowsTerrain", (object)dlg.CameraFollowsTerrain);
                    cameraTerrainFollow = dlg.CameraFollowsTerrain;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraStaysAboveTerrain", (object)dlg.CameraStaysAboveTerrain);
                    cameraAboveTerrain = dlg.CameraStaysAboveTerrain;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "LockCameraToObject", (object)dlg.LockCameraToObject);
                    lockCameraToObject = dlg.LockCameraToObject;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraNearDistance", (object)dlg.CameraNearDistanceFloat);
                    float nearDistance = dlg.CameraNearDistanceFloat;
                    if (nearDistance > 20)
                    {
                        CameraNear = oneMeter * 10f * (nearDistance - 20f) / 20f;
                    }
                    else
                    {
                        CameraNear = oneMeter / 10f * (float)Math.Pow(10.0f, (double)nearDistance / 20.0f);
                    }
                    if (dlg.MaxFramesPerSecondEnabled)
                    {
                        Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MaxFPS", (object)dlg.MaxFramesPerSesconduInt);
                    }
                    else
                    {
                        Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MaxFPS", (object)0);
                    }
                    Root.Instance.MaxFramesPerSecond = (int)dlg.MaxFramesPerSesconduInt;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "AutoSaveEnable", (object)dlg.AutoSaveEnabled);
                    autoSaveEnabled = dlg.AutoSaveEnabled;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "AutoSaveTime", ((object)(dlg.AutoSaveTimeuInt * 60000)));
                    autoSaveTime = dlg.AutoSaveTimeuInt * 60000;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraDefaultSpeed", (object)(dlg.CameraDefaultSpeedTextBoxAsFloat * 1000f));
                    camSpeed = dlg.CameraDefaultSpeedTextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraSpeedIncrement", (object)(dlg.CameraSpeedIncrementTextBoxAsFloat * 1000f));
                    camSpeedIncrement = dlg.CameraSpeedIncrementTextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed1", (object)(dlg.PresetCameraSpeed1TextBoxAsFloat * 1000f));
                    presetCameraSpeed1 = dlg.PresetCameraSpeed1TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed2", (object)(dlg.PresetCameraSpeed2TextBoxAsFloat * 1000f));
                    presetCameraSpeed2 = dlg.PresetCameraSpeed2TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed3", (object)(dlg.PresetCameraSpeed3TextBoxAsFloat * 1000f));
                    presetCameraSpeed3 = dlg.PresetCameraSpeed3TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraSpeed4", (object)(dlg.PresetCameraSpeed4TextBoxAsFloat * 1000f));
                    presetCameraSpeed4 = dlg.PresetCameraSpeed4TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerate", (object)dlg.AccelerateCameraCheckBoxChecked);
                    accelerateCamera = dlg.AccelerateCameraCheckBoxChecked;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerationRate", (object)(dlg.CameraAccelerationRateTextBoxAsFloat * 1000f));
                    camAccel = dlg.CameraAccelerationRateTextBoxAsFloat * 1000f;
                    defaultCamAccelSpeed = dlg.CameraAccelerationRateTextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraAccelerationIncrement", (object)(dlg.CameraAccelerationIncrementTextBoxAsFloat * 1000f));
                    camAccelIncrement = dlg.CameraAccelerationIncrementTextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate1", (object)(dlg.PresetCameraAcceleration1TextBoxAsFloat * 1000f));
                    presetCameraAccel1 = dlg.PresetCameraAcceleration1TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate2", (object)(dlg.PresetCameraAcceleration2TextBoxAsFloat * 1000f));
                    presetCameraAccel2 = dlg.PresetCameraAcceleration2TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate3", (object)(dlg.PresetCameraAcceleration3TextBoxAsFloat * 1000f));
                    presetCameraAccel3 = dlg.PresetCameraAcceleration3TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "PresetCameraAccelRate4", (object)(dlg.PresetCameraAcceleration4TextBoxAsFloat * 1000f));
                    presetCameraAccel4 = dlg.PresetCameraAcceleration4TextBoxAsFloat * 1000f;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "CameraTurnRate", (object)dlg.CameraTurnRateTextBoxAsFloat);
                    cameraTurnIncrement = dlg.CameraTurnRateTextBoxAsFloat;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MouseWheelMultiplier", (object)dlg.MouseWheelMultiplierTextBoxAsFloat);
                    mouseWheelMultiplier = dlg.MouseWheelMultiplierTextBoxAsFloat;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset1", (object)dlg.Preset1MWMTextBoxAsFloat);
                    presetMWM1 = dlg.Preset1MWMTextBoxAsFloat;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset2", (object)dlg.Preset2MWMTextBoxAsFloat);
                    presetMWM2 = dlg.Preset2MWMTextBoxAsFloat;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset3", (object)dlg.Preset3MWMTextBoxAsFloat);
                    presetMWM3 = dlg.Preset3MWMTextBoxAsFloat;
                    Registry.SetValue(Config.WorldEditorBaseRegistryKey, "MWMPreset4", (object)dlg.Preset4MWMTextBoxAsFloat);
                    presetMWM4 = dlg.Preset4MWMTextBoxAsFloat;
                    setToolStripMWMDropDownMenu();
                    setToolStripAccelSpeedDropDownMenu();
                    List<string> newDirs = dlg.RepositoryDirectoryList;
                    if (RepositoryClass.Instance.DifferentDirectoryList(newDirs))
                    {
                        RepositoryClass.Instance.SetRepositoryDirectoriesInRegistry(newDirs);
                        MessageBox.Show("The Asset Repository has been successfully set.  The World Editor must shut down.", "Repository Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        EventArgs ea = new EventArgs();
                        exitToolStripMenuItem_Click(this, ea);
                    }
                }
            }
        }

        private void clearToolStrip1ContextIcons()
        {
            for (int i = toolStrip1.Items.Count - 1; i >= 0; i--)
            {
                ToolStripItem item = toolStrip1.Items[i];
                if (item.Alignment == System.Windows.Forms.ToolStripItemAlignment.Right)
                {
                    toolStrip1.Items.Remove(item);
                }
            }
        }


        public List<WorldTreeNode> SelectedNodes
        {
            get
            {
                List<WorldTreeNode> nodes = new List<WorldTreeNode>();
                foreach (MultiSelectTreeNode node in worldTreeView.SelectedNodes)
                {
                    nodes.Add(node as WorldTreeNode);
                }
                return nodes;
            }
            set
            {
                foreach (MultiSelectTreeNode node in worldTreeView.SelectedNodes)
                {
                    (node as WorldTreeNode).UnSelect();
                }
                foreach (WorldTreeNode node in value)
                {
                    (node as WorldTreeNode).Select();
                }
            }
        }

        private void worldEditor_DragEnter(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].EndsWith(".mvw"))
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                {
                    e.Effect = DragDropEffects.All;
                }
            }
        }

        private void worldEditor_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].EndsWith(".mvw"))
            {
                bool rv = canLoad();
                if (rv)
                {
                    if (worldRoot != null)
                    {
                        ClearWorld();
                    }
                    LoadWorld(files[0]);
                }
            }
            return;
        }

        private void rotationTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            SetRotationFromTrackbar();
        }

        public UserCommandMapping CommandMapping
        {
            get
            {
                return commandMapping;
            }
        }

        private void cameraSpeedAccelDropDownButtonEnter(object sender, EventArgs e)
        {
            cameraSpeedAccelDropDownButton.ShowDropDown();
        }

        private void mouseWheelMultiplierMouseEnter(object sender, EventArgs e)
        {
            mouseWheelMultiplierDropDownButton.ShowDropDown();
        }

        private void worldEditor_activated(object sender, EventArgs e)
        {
            if (Root.Instance != null)
            {
                Root.Instance.MaxFramesPerSecond = activeFps;
            }
        }

        private void worldEditor_deactivated(object sender, EventArgs e)
        {
            if (Root.Instance != null)
            {
                activeFps = Root.Instance.MaxFramesPerSecond;
                Root.Instance.MaxFramesPerSecond = 3;
            }
        }
    }

    public delegate void MouseButtonIntercepter(WorldEditor app, MouseButtons button, int x, int y);
    public delegate void MouseMoveIntercepter(WorldEditor app, int x, int y);
    public delegate void MouseCaptureLost(WorldEditor app);
}
