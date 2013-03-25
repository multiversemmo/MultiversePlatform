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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Configuration;
using Multiverse.AssetRepository;

namespace Multiverse.Tools.TerrainGenerator
{
    public partial class TerrainGenerator : Form
    {
        public enum MouseMode
        {
            MoveCamera,
            AdjustHeight
        }

        protected readonly float oneMeter = 1000.0f;

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
        protected bool spinCamera = false;

        protected Vector3 camVelocity = Vector3.Zero;
        protected Vector3 camAccel = Vector3.Zero;
        protected float camSpeed = 2.5f;
        protected float cameraScale;
        protected bool humanSpeed = false;

        protected Multiverse.Generator.FractalTerrainGenerator terrainGenerator;
        protected LODSpecPrev lodSpecPrev;

        // mouse motion related variables
        protected bool leftMouseDown = false;
        protected bool rightMouseDown = false;
        protected bool leftMouseClick = false;
        protected bool rightMouseClick = false;
        protected bool leftMouseRelease = false;
        protected bool rightMouseRelease = false;

        protected int lastMouseX = 0;
        protected int lastMouseY = 0;

        protected int newMouseX = 0;
        protected int newMouseY = 0;
        protected int mouseWheelDelta = 0;

        protected float mouseWheelMultiplier = 3f;
        protected float cameraFwdBackMultiplier = 500f;

        protected float mouseRotationScale = 50.0f;

        protected bool f1Pressed = false;
        protected bool f2Pressed = false;
        protected bool pageDownPressed = false;
        protected bool pageUpPressed = false;
        protected bool moveForward = false;
        protected bool moveBack = false;
        protected bool moveLeft = false;
        protected bool moveRight = false;

        protected int maxHeightScale = 1000;
        protected int maxHeightFloor = 500;
        protected float minHeightOffset = -1.0f;
        protected float maxHeightOffset = 1.0f;
        protected float maxLacunarity = 10.0f;
        protected float maxH = 10.0f;
        protected float minFractalOffset = -1.0f;
        protected float maxFractalOffset = 1.0f;

        protected bool fractalParamChanged = false;

        protected bool seedMapLoaded = false;
        protected bool useSeedMap = false;

        protected EditableHeightField editableHeightField;

        protected MouseMode hedMouseMode = MouseMode.MoveCamera;

        protected int heightFieldX = 0;
        protected int heightFieldZ = 0;
        protected int heightAdjustSpeed = 100;
        protected bool raisingHeight = false;
        protected bool loweringHeight = false;
        protected float heightAdjustTimer = 0;

        protected string mapFilename;
        protected string baseHelpURL = "http://update.multiverse.net/wiki/index.php/Using_Terrain_Generator_Version_1.5";
        protected string baseReleaseNoteURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.5_Release_Notes";
        protected string baseBugReportURL = "http://update.multiverse.net/custportal/login.php";

        Form newHeightmapDialog;

        public TerrainGenerator()
        {
            InitializeComponent();

            terrainGenerator = new Multiverse.Generator.FractalTerrainGenerator();

            InitControlValues();

            lodSpecPrev = new LODSpecPrev(1024, 4);

            newHeightmapDialog = new NewHeightmapDialog(this);
        }

        private void InitControlValues()
        {
            initHeightScale((int)Math.Round(terrainGenerator.HeightScale));
            initFeatureSpacing((int)Math.Round(terrainGenerator.MetersPerPerlinUnit));
            initHeightFloor((int)Math.Round(terrainGenerator.HeightFloor));
            initHeightOffset(terrainGenerator.HeightOffset);
            initSeedX(terrainGenerator.XOff);
            initSeedY(terrainGenerator.YOff);
            initSeedZ(terrainGenerator.ZOff);
            initH(terrainGenerator.H);
            initLacunarity(terrainGenerator.Lacunarity);
            initIterations((int)terrainGenerator.Octaves);
            initFractalOffset(terrainGenerator.FractalOffset);
            initHeightOutside(terrainGenerator.OutsideMapSeedHeight);
            initSeedMPS(terrainGenerator.SeedMapMetersPerSample);
            initMapOriginX(terrainGenerator.SeedMapOriginX);
            initMapOriginZ(terrainGenerator.SeedMapOriginZ);
            initHeightAdjustSpeed(heightAdjustSpeed);
        }

        private void InitHeightFieldEditorControlValues()
        {

            if (editableHeightField != null)
            {
                adjustHeightRadioButton.Enabled = true;
                brushWidthUpDown.Enabled = true;
                brushTaperUpDown.Enabled = true;
                heightAdjustSpeedUpDown.Enabled = true;
                brushShapeComboBox.Enabled = true;

                initBrushWidth(editableHeightField.BrushWidth);
                initBrushTaper(editableHeightField.BrushTaper);
                initBrushShape(EditableHeightField.BrushStyleToString(editableHeightField.BrushShape));
            }
            else
            {
                adjustHeightRadioButton.Enabled = false;
                brushWidthUpDown.Enabled = false;
                brushTaperUpDown.Enabled = false;
                heightAdjustSpeedUpDown.Enabled = false;
                brushShapeComboBox.Enabled = false;
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
            RepositoryClass.Instance.InitializeRepositoryPath();

            // get a reference to the engine singleton
            engine = new Root("", "trace.txt");

            // add event handlers for frame events
            engine.FrameStarted += new FrameEvent(OnFrameStarted);
            engine.FrameEnded += new FrameEvent(OnFrameEnded);

            // allow for setting up resource gathering
            if (!SetupResources())
				return false;

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

            return true;
        }

//         protected void SetupResources()
//         {
//             EngineConfig config = new EngineConfig();

//             // load the config file
//             // relative from the location of debug and releases executables
//             config.ReadXml("EngineConfig.xml");

//             // interrogate the available resource paths
//             foreach (EngineConfig.FilePathRow row in config.FilePath)
//             {
//                 string fullPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + row.src;

//                 ResourceManager.AddCommonArchive(fullPath, row.type);
//             }
//         }

        protected bool SetupResources()
		{
            if (!RepositoryClass.Instance.RepositoryDirectoryListSet())
            {
                using (SetAssetRepositoryDialog dlg = new SetAssetRepositoryDialog(this))
                {
                    DialogResult result;

                    result = dlg.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return false;
                    }
                }
            }
            foreach (string s in RepositoryClass.AxiomDirectories)
            {
                List<string> repositoryDirectoryList = RepositoryClass.Instance.RepositoryDirectoryList;
                List<string> l = new List<string>();
                foreach (string repository in repositoryDirectoryList)
                    l.Add(Path.Combine(repository, s));
                ResourceManager.AddCommonArchive(l, "Folder");
            }
			return true;
		}

        public bool DesignateRepository()
        {
            return DesignateRepository(false);
        }

        public bool DesignateRepository(bool restart)
        {
            DesignateRepositoriesDialog designateRepositoriesDialog = new DesignateRepositoriesDialog();
            DialogResult result = designateRepositoriesDialog.ShowDialog();
            if (result == DialogResult.Cancel) {
                return false;
            }
            else if (restart) {
                MessageBox.Show("The Asset Repository has been successfully set.  You should restart the tool to get the new settings.", 
                    "Repository Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool ConfigureAxiom()
        {
            // HACK: Temporary
            RenderSystem renderSystem = Root.Instance.RenderSystems[0];
            Root.Instance.RenderSystem = renderSystem;

            Root.Instance.Initialize(false);

            window = Root.Instance.CreateRenderWindow("Main Window", axiomPictureBox.Width, axiomPictureBox.Height, false,
                                                      "externalWindow", axiomPictureBox, "useNVPerfHUD", true);
            Root.Instance.Initialize(false);

            return true;
        }

        protected void ChooseSceneManager()
        {
            scene = Root.Instance.SceneManagers.GetSceneManager(SceneType.ExteriorClose);
        }

        protected void ResetCameraPosition()
        {
            camera.Position = new Vector3(128 * oneMeter, 3000 * oneMeter, 128 * oneMeter);
            camera.LookAt(new Vector3(0, 0, 2000 * oneMeter));
        }

        protected void CreateCamera()
        {
            camera = scene.CreateCamera("PlayerCam");

            ResetCameraPosition();

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

        public bool Start()
        {
            if (!Setup())
            {
                return false;
            }

            // start the engines rendering loop
            engine.StartRendering();

            return true;
        }

        #endregion Application Startup Code

        #region Scene Setup

        protected void GenerateScene()
        {

            ((Axiom.SceneManagers.Multiverse.SceneManager)scene).SetWorldParams(terrainGenerator, lodSpecPrev);
            scene.LoadWorldGeometry("");

            scene.AmbientLight = new ColorEx(0.8f, 0.8f, 0.8f);

            Light light;
            try
            {
                light = scene.GetLight("MainLight");
            } 
            catch (Exception)
            {
                light = scene.CreateLight("MainLight");
            }
            light.Type = LightType.Directional;
            Vector3 lightDir = new Vector3(-80 * oneMeter, -70 * oneMeter, -80 * oneMeter);
            lightDir.Normalize();
            light.Direction = lightDir;
            light.Position = -lightDir;
            light.Diffuse = ColorEx.White;
            light.SetAttenuation(1000 * oneMeter, 1, 0, 0);

            DisplayOcean = false;
            DisplayTerrain = DisplayTerrain;

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

            scene.SetFog(FogMode.None, ColorEx.White, .008f, 0, 1000 * oneMeter);

            return;
        }

        #endregion Scene Setup

        private void LaunchDoc(string anchor)
        {
            string target = String.Format("{0}#{1}", baseHelpURL, anchor);
            System.Diagnostics.Process.Start(target);
        }

        // load the map (terrain parameters) from the given file
        public void LoadTerrain(string filename)
        {
            if (filename != null)
            {
                // remember the filename
                mapFilename = filename;

                ReadSavedMap(filename);

                Text = "TerrainGenerator: " + filename;
				
				if (terrainGenerator.SeedMap != null)
                {
					editableHeightField = new EditableHeightField(terrainGenerator.SeedMap, 
																  terrainGenerator.SeedMapWidth,
																  terrainGenerator.SeedMapHeight);
					editableHeightField.YScale = terrainGenerator.HeightScale * oneMeter;
                    editableHeightField.XZScale = SeedMapMetersPerSample * oneMeter;
                    editableHeightField.OffsetX = SeedMapOriginX * oneMeter;
                    editableHeightField.OffsetZ = SeedMapOriginZ * oneMeter;

                    SeedMapLoaded = true;
                    UseSeedMap = AlgorithmUsesSeedMap();
                }
                else
                {
                    editableHeightField = null;
                }

                InitControlValues();
                InitHeightFieldEditorControlValues();

                fractalParamChanged = true;
            }
        }

        // read a saved map from the given filename
        protected void ReadSavedMap(string filename)
        {
            TextReader t = new StreamReader(filename);
            XmlTextReader r = new XmlTextReader(t);

            ReadSavedMap(r);
            t.Close();

            return;
        }

        // read a saved map from the given XML stream
        protected void ReadSavedMap(XmlTextReader r)
        {
            // read until we find the start of the world description
            while (r.Read())
            {
                // look for the start of the terrain description
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "Terrain")
                    {
                        terrainGenerator.FromXML(r);
                        break;
                    }
                }
            }
        }

        // save the map(terrain parameters) in the given file
        public void SaveTerrain(string filename)
        {
            if (filename != null)
            {
                // remember the filename
                mapFilename = filename;

                TextWriter s = new StreamWriter(filename);
                XmlTextWriter w = new XmlTextWriter(s);
                w.WriteStartDocument();
                terrainGenerator.ToXml(w);
                w.WriteEndDocument();
                s.Close();
				Text = "TerrainGenerator: " + filename;
			}
        }

        public void NewSeedMap(int w, int h, float defHeight, int mps, bool centered)
        {
            SeedMapMetersPerSample = mps;
            if (centered)
            {
                SeedMapOriginX = -w * mps / 2;
                SeedMapOriginZ = -h * mps / 2;
            }

            editableHeightField = new EditableHeightField(w, h, defHeight);
            editableHeightField.YScale = terrainGenerator.HeightScale * oneMeter;
            editableHeightField.XZScale = SeedMapMetersPerSample * oneMeter;
            editableHeightField.OffsetX = SeedMapOriginX * oneMeter;
            editableHeightField.OffsetZ = SeedMapOriginZ * oneMeter;

			terrainGenerator.BitsPerSample = 16;
            terrainGenerator.SetSeedMap(editableHeightField.Map, editableHeightField.Width, editableHeightField.Height);
            SeedMapLoaded = true;
            UseSeedMap = true;

            InitHeightFieldEditorControlValues();

        }

        // this method returns true if the currently selected terrain generation algorithm
        // uses a seed heightmap
        private bool AlgorithmUsesSeedMap()
        {
            if (terrainGenerator.Algorithm == Multiverse.Generator.GeneratorAlgorithm.HybridMultifractalWithSeedMap)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Axiom Frame Event Handlers

        protected void OnFrameEnded(object source, FrameEventArgs e)
        {
            return;
        }


        protected void OnFrameStarted(object source, FrameEventArgs e)
        {

            if (quit)
            {
                Root.Instance.QueueEndRendering();

                return;
            }

            if (fractalParamChanged)
            {
                Regenerate();
                fractalParamChanged = false;
            }

            float scaleMove = 100 * e.TimeSinceLastFrame * oneMeter;

            time += e.TimeSinceLastFrame;
            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.Time = time;

            if (spinCamera)
            {
                float rot = 20 * e.TimeSinceLastFrame;

                camera.Yaw(rot);
            }

            // reset acceleration zero
            camAccel = Vector3.Zero;

            // set the scaling of camera motion
            cameraScale = 100 * e.TimeSinceLastFrame;

            if (moveForward)
            {
                camAccel.z = -1.0f;
            }
            if (moveBack)
            {
                camAccel.z = 1.0f;
            }
            if (moveLeft)
            {
                camAccel.x = -0.5f;
            }
            if (moveRight)
            {
                camAccel.x = 0.5f;
            }

            int deltaX = lastMouseX - newMouseX;
            int deltaY = lastMouseY - newMouseY;

            lastMouseX = newMouseX;
            lastMouseY = newMouseY;

            MouseMode mouseMode;
            if (tabControl1.SelectedTab == heightMapEditor)
            {
                mouseMode = hedMouseMode;
            }
            else
            {
                mouseMode = MouseMode.MoveCamera;
            }

            switch (mouseMode)
            {
                case MouseMode.MoveCamera:
                    if (leftMouseDown)
                    {
                        if (deltaX != 0)
                        {
                            camera.Yaw(-deltaX * mouseRotationScale * e.TimeSinceLastFrame);
                        }
                        if (deltaY != 0)
                        {
                            camera.Pitch(-deltaY * mouseRotationScale * e.TimeSinceLastFrame);
                        }
                    }
                    if (rightMouseDown)
                    {
                        if (deltaX != 0)
                        {
                            camera.Yaw(deltaX * mouseRotationScale * e.TimeSinceLastFrame);
                        }
                        if (deltaY != 0)
                        {
                            camera.Pitch(deltaY * mouseRotationScale * e.TimeSinceLastFrame);
                        }
                    }
                    break;
                case MouseMode.AdjustHeight:
                    if ((tabControl1.SelectedTab == heightMapEditor) && (editableHeightField != null))
                    { // handle mouse events in the height map editor

                        // move cursor based on mouse motion
                        if ((deltaX != 0) || (deltaY != 0))
                        {
                            Ray ray = camera.GetCameraToViewportRay((float)newMouseX / (float)window.Width, (float)newMouseY / (float)window.Height);
                            int x;
                            int z;
                            bool hit = editableHeightField.RayIntersection(ray, out x, out z);
                            if (hit)
                            {
                                heightFieldX = x;
                                heightFieldZ = z;
                                toolStripStatusLabel1.Text = string.Format("{0}, {1}", heightFieldX, heightFieldZ);
                                toolStripStatusLabel2.Text = string.Format("{0}", editableHeightField.GetHeight(heightFieldX, heightFieldZ));
                                editableHeightField.SetCursorLoc(heightFieldX, heightFieldZ);
                            }
                        }

                        // handle click/release that start/stop raising/lowering
                        if (leftMouseClick)
                        {
                            raisingHeight = true;
                        }
                        if (rightMouseClick)
                        {
                            loweringHeight = true;
                        }
                        if (leftMouseRelease)
                        {
                            raisingHeight = false;
                        }
                        if (rightMouseRelease)
                        {
                            loweringHeight = false;
                        }

                        if (raisingHeight)
                        {
                            editableHeightField.AdjustPointWithBrush(heightFieldX, heightFieldZ, e.TimeSinceLastFrame * heightAdjustSpeed / 100);
                        }
                        if (loweringHeight)
                        {
                            editableHeightField.AdjustPointWithBrush(heightFieldX, heightFieldZ, -e.TimeSinceLastFrame * heightAdjustSpeed / 100);
                        }
                    }
                    break;
            }

            if (mouseWheelDelta != 0)
            {
                camera.MoveRelative(new Vector3(0, 0, -oneMeter * mouseWheelMultiplier * mouseWheelDelta));

                mouseWheelDelta = 0;
            }

            // reset single fire click and release events
            leftMouseClick = false;
            rightMouseClick = false;
            leftMouseRelease = false;
            rightMouseRelease = false;

            if (f1Pressed)
            {
                f1Pressed = false;
                WarpCamera();
            }

            if (f2Pressed)
            {
                f2Pressed = false;
                camera.LookAt(new Vector3(camera.Position.x, 0, camera.Position.z + oneMeter * 100f));
            }

            if (pageUpPressed)
            {
                camera.MoveRelative(new Vector3(0, 0, -oneMeter * cameraFwdBackMultiplier * e.TimeSinceLastFrame));
            }

            if (pageDownPressed)
            {
                camera.MoveRelative(new Vector3(0, 0, oneMeter * cameraFwdBackMultiplier * e.TimeSinceLastFrame));
            }

            if (humanSpeed)
            {
                camVelocity = camAccel * 7.0f * oneMeter;
                camera.MoveRelative(camVelocity * e.TimeSinceLastFrame);
            }
            else
            {
                camVelocity += (camAccel * scaleMove * camSpeed);

                // move the camera based on the accumulated movement vector
                camera.MoveRelative(camVelocity * e.TimeSinceLastFrame);

                // Now dampen the Velocity - only if user is not accelerating
                if (camAccel == Vector3.Zero)
                {
                    float decel = 1 - (6 * e.TimeSinceLastFrame);

                    if (decel < 0)
                    {
                        decel = 0;
                    }
                    camVelocity *= decel;
                }
            }

            //if (followTerrain || (result.worldFragment.SingleIntersection.y + (2.0f * oneMeter)) > camera.Position.y)
            //{
            //    // adjust new camera position to be a fixed distance above the ground

            //    camera.Position = new Vector3(camera.Position.x, result.worldFragment.SingleIntersection.y + (2.0f * oneMeter), camera.Position.z);
            //}
        }

        protected void WarpCamera()
        {
            Ray ray = camera.GetCameraToViewportRay((float)newMouseX / (float)window.Width, (float)newMouseY / (float)window.Height);
            Vector3 newLoc;

            if ((tabControl1.SelectedTab == heightMapEditor) && (editableHeightField != null))
            { // heightmap editor
                int hitX;
                int hitZ;

                bool hit = editableHeightField.RayIntersection(ray, out hitX, out hitZ);
                if (hit)
                {
                    newLoc = editableHeightField.HeightfieldCoordToWorldCoord(hitX, hitZ);

                    camera.Position = new Vector3(newLoc.x, camera.Position.y, newLoc.z);
                }
            }
            else
            { // regular terrain
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
        }

        #endregion Axiom Frame Event Handlers

        #region Properties

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

        private bool DisplayOcean
        {
            get
            {
                return displayOcean;
            }
            set
            {
                displayOcean = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = displayOcean;
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

        private bool SpinCamera
        {
            get
            {
                return spinCamera;
            }
            set
            {
                spinCamera = value;
            }
        }

        private bool SeedMapLoaded
        {
            get
            {
                return seedMapLoaded;
            }
            set
            {
                seedMapLoaded = value;
                useSeedMapCheckBox.Enabled = value;
                heightOutsideTextBox.Enabled = value;
                mpsTextBox.Enabled = value;
                mapOriginXTextBox.Enabled = value;
                mapOriginZTextBox.Enabled = value;

                if (!value)
                {
                    useSeedMapCheckBox.Checked = false;
                }
                fractalParamChanged = true;
            }
        }

        private bool UseSeedMap
        {
            get
            {
                return useSeedMap;
            }
            set
            {
                useSeedMap = value;
                if (useSeedMap)
                {
                    terrainGenerator.Algorithm = Multiverse.Generator.GeneratorAlgorithm.HybridMultifractalWithSeedMap;
                }
                else
                {
                    terrainGenerator.Algorithm = Multiverse.Generator.GeneratorAlgorithm.HybridMultifractalWithFloor;
                }
                fractalParamChanged = true;

                useSeedMapCheckBox.Checked = useSeedMap;
            }
        }

        private float SeedMapOriginX
        {
            get
            {
                return terrainGenerator.SeedMapOriginX;
            }
            set
            {
                terrainGenerator.SeedMapOriginX = value;
                mapOriginXTextBox.Text = value.ToString();
            }
        }

        private float SeedMapOriginZ
        {
            get
            {
                return terrainGenerator.SeedMapOriginZ;
            }
            set
            {
                terrainGenerator.SeedMapOriginZ = value;
                mapOriginZTextBox.Text = value.ToString();
            }
        }

        private float SeedMapMetersPerSample
        {
            get
            {
                return terrainGenerator.SeedMapMetersPerSample;
            }
            set
            {
                terrainGenerator.SeedMapMetersPerSample = value;
                mpsTextBox.Text = value.ToString();
            }
        }

        #endregion Properties

        #region Methods to initialize control value

        private void initHeightScale(int height)
        {
            heightScaleTrackBar.Value = invertValueNSquared(height, heightScaleTrackBar.Maximum, maxHeightScale);
            heightScaleValueLabel.Text = scaleValueNSquared(heightScaleTrackBar.Value, heightScaleTrackBar.Maximum, maxHeightScale).ToString();
        }

        private void initHeightFloor(int heightFloor)
        {
            heightFloorTrackBar.Value = invertValueNSquared(heightFloor, heightFloorTrackBar.Maximum, maxHeightFloor);
            heightFloorValueLabel.Text = scaleValueNSquared(heightFloorTrackBar.Value, heightFloorTrackBar.Maximum, maxHeightFloor).ToString();

        }

        private void initHeightOffset(float heightOffset)
        {
            heightOffsetTrackBar.Value = invertValueLinear(heightOffset, heightOffsetTrackBar.Maximum, minHeightOffset, maxHeightOffset);
            heightOffsetValueLabel.Text = heightOffset.ToString();
        }

        private void initFeatureSpacing(int value)
        {
            featureSpacingTrackBar.Value = value;
            featureSpacingValueLabel.Text = featureSpacingTrackBar.Value.ToString();
        }

        private void initSeedX(float value)
        {
            seedXUpDown.Value = (decimal)value;
        }

        private void initSeedY(float value)
        {
            seedYUpDown.Value = (decimal)value;
        }

        private void initSeedZ(float value)
        {
            seedZUpDown.Value = (decimal)value;
        }

        private void initH(float value)
        {
            hTrackBar.Value = invertValueLinear(value, hTrackBar.Maximum, 0, maxH);
            hValueLabel.Text = value.ToString();
        }

        private void initLacunarity(float value)
        {
            lacunarityTrackBar.Value = invertValueLinear(value, lacunarityTrackBar.Maximum, 0, maxLacunarity);
            lacunarityValueLabel.Text = value.ToString();
        }

        private void initIterations(int value)
        {
            iterationsUpDown.Value = value;
        }

        private void initFractalOffset(float value)
        {
            fractalOffsetTrackBar.Value = invertValueLinear(value, fractalOffsetTrackBar.Maximum, minFractalOffset, maxFractalOffset);
            fractalOffsetValueLabel.Text = value.ToString();
        }

        private void initHeightOutside(float value)
        {
            heightOutsideTextBox.Text = value.ToString();
        }

        private void initSeedMPS(float value)
        {
            mpsTextBox.Text = value.ToString();
        }

        private void initMapOriginX(float value)
        {
            mapOriginXTextBox.Text = value.ToString();
        }

        private void initMapOriginZ(float value)
        {
            mapOriginZTextBox.Text = value.ToString();
        }

        private void initHeightAdjustSpeed(int value)
        {
            heightAdjustSpeedUpDown.Value = value;
        }

        private void initBrushWidth(int value)
        {
            brushWidthUpDown.Value = value;
        }

        private void initBrushTaper(int value)
        {
            brushTaperUpDown.Value = value;
        }

        private void initBrushShape(string value)
        {
            brushShapeComboBox.SelectedItem = value;
        }

        #endregion Methods to initialize control value


        #region Methods to scale control values

        private int scaleValueNSquared(int value, int maxInput, int maxOutput)
        {
            double tmp = value * Math.Sqrt((double)maxOutput) / maxInput;
            double squared = tmp * tmp;

            return (int)Math.Round(squared);
        }

        private int invertValueNSquared(int value, int maxInput, int maxOutput)
        {
            double tmp = Math.Sqrt((double)value);
            double ret = tmp / Math.Sqrt((double)maxOutput) * maxInput;

            return (int)Math.Round(ret);
        }

        private float scaleValueLinear(int value, int maxInput, float minOutput, float maxOutput)
        {
            float outRange = maxOutput - minOutput;

            return (outRange * value / maxInput) + minOutput;
        }

        private int invertValueLinear(float value, int maxInput, float minOutput, float maxOutput)
        {
            float outRange = maxOutput - minOutput;

            return (int)Math.Round((value - minOutput) / outRange * maxInput);
        }

        #endregion Methods to scale control values

        #region Form Control Event Handlers

        private void axiomPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftMouseDown = true;
                leftMouseClick = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                rightMouseDown = true;
                rightMouseClick = true;
            }

            newMouseX = e.X;
            newMouseY = e.Y;
        }

        private void axiomPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftMouseDown = false;
                leftMouseRelease = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                rightMouseDown = false;
                rightMouseRelease = true;
            }

            newMouseX = e.X;
            newMouseY = e.Y;
        }

        private void axiomPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            newMouseX = e.X;
            newMouseY = e.Y;
        }

        private void axiomPictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            newMouseX = e.X;
            newMouseY = e.Y;

            mouseWheelDelta += e.Delta;
        }

        private void axiomPictureBox_MouseEnter(object sender, EventArgs e) {
            axiomPictureBox.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            quit = true;
        }

        private void wireFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayWireFrame = !DisplayWireFrame;
        }

        private void viewToolStripMenuItem1_DropDownOpening(object sender, EventArgs e)
        {
            wireFrameToolStripMenuItem.Checked = DisplayWireFrame;
            displayOceanToolStripMenuItem.Checked = DisplayOcean;
            spinCameraToolStripMenuItem.Checked = SpinCamera;
        }

        private void displayOceanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayOcean = !DisplayOcean;
        }

        private void spinCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void fractalParamsButton_Click(object sender, EventArgs e)
        {
            if (fractalParamsGroupBox.Visible)
            {
                fractalParamsGroupBox.Hide();
                this.fractalParamsButton.Image = global::TerrainGenerator.Properties.Resources.group_down_icon;
            }
            else
            {
                fractalParamsGroupBox.Show();
                this.fractalParamsButton.Image = global::TerrainGenerator.Properties.Resources.group_up_icon;
            }
        }

        private void generalTerrainButton_Click(object sender, EventArgs e)
        {
            if (generalTerrainGroupBox.Visible)
            {
                generalTerrainGroupBox.Hide();
                this.generalTerrainButton.Image = global::TerrainGenerator.Properties.Resources.group_down_icon;
            }
            else
            {
                generalTerrainGroupBox.Show();
                this.generalTerrainButton.Image = global::TerrainGenerator.Properties.Resources.group_up_icon;
            }
        }

        private void heightMapParamsButton_Click(object sender, EventArgs e)
        {
            if (heightMapGroupBox.Visible)
            {
                heightMapGroupBox.Hide();
                this.heightMapParamsButton.Image = global::TerrainGenerator.Properties.Resources.group_down_icon;
            }
            else
            {
                heightMapGroupBox.Show();
                this.heightMapParamsButton.Image = global::TerrainGenerator.Properties.Resources.group_up_icon;
            }
        }

        private void heightScaleTrackBar_Scroll(object sender, EventArgs e)
        {
            int value = scaleValueNSquared(heightScaleTrackBar.Value, heightScaleTrackBar.Maximum, maxHeightScale);
            heightScaleValueLabel.Text = value.ToString();
            terrainGenerator.HeightScale = value;
            if (editableHeightField != null)
            {
                editableHeightField.YScale = value * oneMeter;
            }
            fractalParamChanged = true;
        }

        private void heightFloorTrackBar_Scroll(object sender, EventArgs e)
        {
            int value = scaleValueNSquared(heightFloorTrackBar.Value, heightFloorTrackBar.Maximum, maxHeightFloor);
            heightFloorValueLabel.Text = value.ToString();
            terrainGenerator.HeightFloor = value;
            fractalParamChanged = true;
        }

        private void heightOffsetTrackBar_Scroll(object sender, EventArgs e)
        {
            float value = scaleValueLinear(heightOffsetTrackBar.Value, heightOffsetTrackBar.Maximum, minHeightOffset, maxHeightOffset);
            heightOffsetValueLabel.Text = value.ToString();
            terrainGenerator.HeightOffset = value;
            fractalParamChanged = true;
        }

        private void featureSpacingTrackBar_Scroll(object sender, EventArgs e)
        {
            int value = featureSpacingTrackBar.Value;
            featureSpacingValueLabel.Text = value.ToString();
            terrainGenerator.MetersPerPerlinUnit = value;
            fractalParamChanged = true;
        }

        private void TerrainGenerator_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit = true;
        }

        private void hTrackBar_Scroll(object sender, EventArgs e)
        {
            float value = scaleValueLinear(hTrackBar.Value, hTrackBar.Maximum, 0, maxH);
            hValueLabel.Text = value.ToString();
            terrainGenerator.H = value;
            fractalParamChanged = true;
        }

        private void lacunarityTrackBar_Scroll(object sender, EventArgs e)
        {
            float value = scaleValueLinear(lacunarityTrackBar.Value, lacunarityTrackBar.Maximum, 0, maxLacunarity);
            lacunarityValueLabel.Text = value.ToString();
            terrainGenerator.Lacunarity = value;
            fractalParamChanged = true;
        }

        private void iterationsUpDown_ValueChanged(object sender, EventArgs e)
        {
            terrainGenerator.Octaves = (float)iterationsUpDown.Value;
            fractalParamChanged = true;
        }

        private void fractalOffsetTrackBar_Scroll(object sender, EventArgs e)
        {
            float value = scaleValueLinear(fractalOffsetTrackBar.Value, fractalOffsetTrackBar.Maximum, minFractalOffset, maxFractalOffset);
            fractalOffsetValueLabel.Text = value.ToString();
            terrainGenerator.FractalOffset = value;
            fractalParamChanged = true;
        }

        private void seedXUpDown_ValueChanged(object sender, EventArgs e)
        {
            terrainGenerator.XOff = (float)seedXUpDown.Value;
            fractalParamChanged = true;
        }

        private void seedYUpDown_ValueChanged(object sender, EventArgs e)
        {
            terrainGenerator.YOff = (float)seedYUpDown.Value;
            fractalParamChanged = true;
        }

        private void seedZUpDown_ValueChanged(object sender, EventArgs e)
        {
            terrainGenerator.ZOff = (float)seedZUpDown.Value;
            fractalParamChanged = true;
        }

        private void loadSeedMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open Seed Map File";
            dlg.Filter = "Seed Files (*.csv)|*.csv|PNG files (*.png)|*.png|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filename = dlg.FileName;
                // terrainGenerator.LoadSeedMap(filename);

				int bitsPerSampleRead;
				editableHeightField = new EditableHeightField(filename, out bitsPerSampleRead);
                editableHeightField.YScale = terrainGenerator.HeightScale * oneMeter;
                editableHeightField.XZScale = SeedMapMetersPerSample * oneMeter;
                editableHeightField.OffsetX = SeedMapOriginX * oneMeter;
                editableHeightField.OffsetZ = SeedMapOriginZ * oneMeter;

				terrainGenerator.BitsPerSample = bitsPerSampleRead;
                terrainGenerator.SetSeedMap(editableHeightField.Map, editableHeightField.Width, editableHeightField.Height);
                SeedMapLoaded = true;

                InitHeightFieldEditorControlValues();
            }
            dlg.Dispose();
        }

        private void useSeedMapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UseSeedMap = useSeedMapCheckBox.Checked;
        }

        private void heightOutsideTextBox_TextChanged(object sender, EventArgs e)
        {
            terrainGenerator.OutsideMapSeedHeight = float.Parse(heightOutsideTextBox.Text);
            fractalParamChanged = true;
        }

        private void mpsTextBox_TextChanged(object sender, EventArgs e)
        {
            float mps = float.Parse(mpsTextBox.Text);
            SeedMapMetersPerSample = mps;
            if (editableHeightField != null)
            {
                editableHeightField.XZScale = mps * oneMeter;
            }
            fractalParamChanged = true;
        }

        private void mapOriginXTextBox_TextChanged(object sender, EventArgs e)
        {
            float originX = float.Parse(mapOriginXTextBox.Text);
            SeedMapOriginX = originX;
            if (editableHeightField != null)
            {
                editableHeightField.OffsetX = originX * oneMeter;
            }
            fractalParamChanged = true;
        }

        private void mapOriginZTextBox_TextChanged(object sender, EventArgs e)
        {
            float originZ = float.Parse(mapOriginZTextBox.Text);
            SeedMapOriginZ = originZ;
            if (editableHeightField != null)
            {
                editableHeightField.OffsetZ = originZ * oneMeter;
            }
            fractalParamChanged = true;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (tabControl1.SelectedTab == heightMapEditor)
            {
                if (editableHeightField != null)
                {
                    DisplayTerrain = false;
                    scene.RootSceneNode.AttachObject(editableHeightField);
                }
                else
                {
                    MessageBox.Show("There is currently no Height Map loaded for this terrain.  Before editing a Height Map, you must first load or create one.", "No Height Map Loaded", MessageBoxButtons.OK);
                }
            }
            else
            {
                DisplayTerrain = true;
                if (editableHeightField != null)
                {
                    scene.RootSceneNode.DetachObject(editableHeightField);

                    // when switching back to the main view, force a refresh if the heightmap editor was active
                    fractalParamChanged = true;
                }
            }
        }

        private void moveCameraRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (moveCameraRadioButton.Checked)
            {
                hedMouseMode = MouseMode.MoveCamera;
                editableHeightField.ShowBrush = false;
            }
        }

        private void adjustHeightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (adjustHeightRadioButton.Checked)
            {
                hedMouseMode = MouseMode.AdjustHeight;
                editableHeightField.ShowBrush = true;
            }
        }

        private void heightAdjustSpeedUpDown_ValueChanged(object sender, EventArgs e)
        {
            heightAdjustSpeed = (int)heightAdjustSpeedUpDown.Value;
        }

        private void brushWidthUpDown_ValueChanged(object sender, EventArgs e)
        {
            editableHeightField.BrushWidth = (int)brushWidthUpDown.Value;
        }

        private void loadTerrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {

                dlg.Title = "Load Terrain";
                dlg.Filter = "Multiverse Terrain files (*.mvt)|*.mvt|xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadTerrain(dlg.FileName);
                    tabControl1.SelectedIndex = 0;
                }
            }
        }

        private void saveTerrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Terrain";
                dlg.DefaultExt = "mvt";
                dlg.Filter = "Multiverse World files (*.mvt)|*.mvt|xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SaveTerrain(dlg.FileName);
                }
            }
        }

        private void designateRepositoryMenuItem_Click(object sender, EventArgs e) {
            DesignateRepository(true);
        }

        private void brushShapeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            BrushStyle style = EditableHeightField.StringToBrushStyle(brushShapeComboBox.SelectedItem as string);

            editableHeightField.BrushShape = style;
        }

        private void brushTaperUpDown_ValueChanged(object sender, EventArgs e)
        {
            editableHeightField.BrushTaper = (int)brushTaperUpDown.Value;
        }

        private void resetCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetCameraPosition();
        }

        #endregion Form Control Event Handlers

        private void createSeedHeightmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newHeightmapDialog.ShowDialog();
        }

        private void TerrainGenerator_Load(object sender, EventArgs e)
        {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.axiomPictureBox_MouseWheel);
        }

        private void TerrainGenerator_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad8:
                case Keys.W:
                case Keys.I:
                    e.Handled = true;
                    moveForward = true;
                    break;
                case Keys.NumPad2:
                case Keys.S:
                case Keys.K:
                    e.Handled = true;
                    moveBack = true;
                    break;
                case Keys.NumPad4:
                case Keys.A:
                case Keys.J:
                    e.Handled = true;
                    moveLeft = true;
                    break;
                case Keys.NumPad6:
                case Keys.D:
                case Keys.L:
                    e.Handled = true;
                    moveRight = true;
                    break;
                case Keys.F1:
                    e.Handled = true;
                    f1Pressed = true;
                    break;
                case Keys.F2:
                    e.Handled = true;
                    f2Pressed = true;
                    break;
                case Keys.PageDown:
                    e.Handled = true;
                    pageDownPressed = true;
                    break;
                case Keys.PageUp:
                    e.Handled = true;
                    pageUpPressed = true;
                    break;
            }

        }

        private void TerrainGenerator_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad8:
                case Keys.W:
                case Keys.I:
                    e.Handled = true;
                    moveForward = false;
                    break;
                case Keys.NumPad2:
                case Keys.S:
                case Keys.K:
                    e.Handled = true;
                    moveBack = false;
                    break;
                case Keys.NumPad4:
                case Keys.A:
                case Keys.J:
                    e.Handled = true;
                    moveLeft = false;
                    break;
                case Keys.NumPad6:
                case Keys.D:
                case Keys.L:
                    e.Handled = true;
                    moveRight = false;
                    break;
                case Keys.F1:
                    e.Handled = true;
                    f1Pressed = false;
                    break;
                case Keys.F2:
                    e.Handled = true;
                    f2Pressed = false;
                    break;
                case Keys.PageDown:
                    e.Handled = true;
                    pageDownPressed = false;
                    break;
                case Keys.PageUp:
                    e.Handled = true;
                    pageUpPressed = false;
                    break;
            }
        }

        private void launchOnlineHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchDoc("");
        }

        private void aboutTerrainGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			string msg = string.Format("Multiverse Terrain Generator\n\nVersion: {0}\n\nCopyright 2006-2007 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse TerrainGenerator", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        }

        private void generalTerrainHelpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("generaln");
        }

        private void fractalParamsHelpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("fractal");
        }

        private void heightMapParamsHelpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("heightmap-params");
        }

        private void heightMapEditorHelpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("heightmap-options");
        }

        private void hmeOptionsButton_Click(object sender, EventArgs e)
        {
            if (hmeOptionsGroupBox.Visible)
            {
                hmeOptionsGroupBox.Hide();
                this.hmeOptionsButton.Image = global::TerrainGenerator.Properties.Resources.group_down_icon;
            }
            else
            {
                hmeOptionsGroupBox.Show();
                this.hmeOptionsButton.Image = global::TerrainGenerator.Properties.Resources.group_up_icon;
            }
        }

        private void submitABugReToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(baseBugReportURL); 
        }

        private void TerrainGenerator_Resize(object sender, EventArgs e)
        {
            if (Root.Instance == null)
                return;
            Root.Instance.SuspendRendering = this.WindowState == FormWindowState.Minimized;
        }

        private void axiomPictureBox_Resize(object sender, EventArgs e)
        {
            if (window != null)
            {
                int height = axiomPictureBox.Height;
                int width = axiomPictureBox.Width;
                window.Resize(width, height);
                viewport.SetDimensions(0, 0, 1.0f, 1.0f);
                camera.AspectRatio = (float)window.Width / window.Height;
            }
        }

        private void launchReleaseNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(baseReleaseNoteURL);
        }
    }
}
