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

using log4net;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Configuration;
using Axiom.Collections;
using Axiom.Animating;
using Axiom.ParticleSystems;
using Axiom.Serialization;

using Multiverse.Serialization;
using Multiverse.AssetRepository;
using Multiverse.CollisionLib;
using Microsoft.Win32;

namespace Multiverse.Tools.ModelViewer
{
    public partial class ModelViewer : Form
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ModelViewer));

        protected static readonly float OneMeter = 1000.0f;

        protected Root engine;
        protected Camera camera;
        protected Viewport viewport;
        protected SceneManager sceneManager;
        protected RenderWindow window;

        protected bool quit = false;

        protected float normalsAxisLength = .0f * OneMeter;
        protected float boneAxisLength = .05f * OneMeter;
        protected float socketAxisLength = .05f * OneMeter;
        protected float selectBoxSize = .025f * OneMeter;
        protected float trackSizeFactor = .01f * OneMeter;
        protected float time = 0;
        protected bool displayWireFrame = false;
        protected bool displayTerrain = false;
        protected bool spinCamera = false;
        protected bool spinLight = false;
        protected bool showBoundingBox = false;
        protected bool showBones = false;
        protected bool bonesShown = false;
        protected bool showSockets = false;
        protected bool socketsShown = false;
        protected bool showGroundPlane = true;
        protected bool groundPlaneShown = false;
        protected bool showNormals = true; // set to true for now, so i can see these
        protected bool normalsShown = false;

        protected Entity loadedModel = null;
        protected Mesh loadedMesh = null;
        protected SceneNode modelNode = null;
        protected SceneNode helperNode = null;
        protected SceneNode particleNode = null;

        protected Vector3 modelBase = new Vector3(32 * OneMeter, 50 * OneMeter, 32 * OneMeter);
        protected float modelHeight = 10 * OneMeter;
        protected float cameraFocusHeight = 0.5f;

        protected float cameraRadius = 10 * OneMeter;
        protected float cameraAzimuth = 0;
        protected float cameraZenith = 0;

        protected float lightAzimuth = 0;
        protected float lightZenith = 45;
        protected Light directionalLight;

        protected bool updateLight = false;
        protected bool updateCamera = false;

        protected Multiverse.Generator.FractalTerrainGenerator terrainGenerator;

        protected bool leftMouseDown = false;
		protected bool leftMouseClick = false;
        protected bool rightMouseDown = false;

        protected int lastMouseX = 0;
        protected int lastMouseY = 0;

        protected int newMouseX = 0;
        protected int newMouseY = 0;
        protected int newMouseDelta = 0;

        protected float mouseScale = 0.5f;

        protected MouseMode mouseMode = MouseMode.MoveCamera;

        protected bool animationPlaying = false;
        protected AnimationState currentAnimation;
        protected bool animationLooping = false;
        protected float animationSpeed = 1.0f;

        protected long timerFreq;
        protected long lastFrameTime;
        protected bool first = true;
        protected string lastDir = null;

        protected string initial_model = null;

        public BoneDisplay boneDisplay = null;
        protected List<Node> attachedNodes = new List<Node>();

        // The collision detector, only used to render collision objects
        protected CollisionAPI collisionManager = null;
        
        protected List<string> materialSchemeNames = null;
        
        protected List<string> repositoryDirectoryList = new List<string>();

        protected bool advancedOptions = false;     

        private void PositionCamera()
        {
            Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(CameraAzimuth), Vector3.UnitY);
            Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(CameraZenith), Vector3.UnitX);
            Matrix3 camMatrix = (azimuthRotation * zenithRotation).ToRotationMatrix();

            // Put the camera at the correct radius from the model
            Vector3 relativeCameraPos = camMatrix * (CameraRadius * Vector3.UnitZ);

            // Look at a point that is 1.8m above the player's base - this should be
            // around the character's head.
            camera.Position = CameraFocus + relativeCameraPos;
            camera.LookAt(CameraFocus);

            updateCamera = false;
        }

        private void PositionLight()
        {
            Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(LightAzimuth), Vector3.UnitY);
            Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-LightZenith), Vector3.UnitX);
            Matrix3 lightMatrix = (azimuthRotation * zenithRotation).ToRotationMatrix();

            // Compute "position" of light (actually just reverse direction)
            Vector3 relativeLightPos = lightMatrix * Vector3.UnitZ;

            relativeLightPos.Normalize();

            directionalLight.Direction = -relativeLightPos;
            directionalLight.Position = relativeLightPos;

            updateLight = false;
        }

        public ModelViewer()
        {
            InitializeComponent();

            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.axiomPictureBox_MouseWheel);

            animationTrackBar.Scroll += new EventHandler(this.animationTrackBar_Scroll);
            animationTrackBar.Value = 0;
            animationTrackBar.Minimum = 0;
            animationTrackBar.Maximum = 10;

            subMeshTreeView.AfterCheck +=
                new TreeViewEventHandler(this.subMeshTreeView_ItemCheck);
            subMeshTreeView.AfterSelect +=
                new TreeViewEventHandler(this.subMeshTreeView_SelectedIndexChanged);
            socketListBox.ItemCheck +=
                new ItemCheckEventHandler(this.socketListBox_ItemCheck);
            socketListBox.SelectedIndexChanged +=
                new EventHandler(this.socketListBox_SelectedIndexChanged);
            bonesTreeView.AfterSelect +=
                new TreeViewEventHandler(this.bonesTreeView_SelectedIndexChanged);
            animationListBox.SelectedIndexChanged +=
                new EventHandler(this.animationListBox_ItemCheck);

            socketAxisSizeTrackBar.Value = (int)(socketAxisLength / trackSizeFactor);
            boneAxisSizeTrackBar.Value = (int)(boneAxisLength / trackSizeFactor);

            terrainGenerator = new Multiverse.Generator.FractalTerrainGenerator();
            terrainGenerator.HeightFloor = 50;
            terrainGenerator.HeightScale = 0;

            timerFreq = Stopwatch.Frequency;

            lastFrameTime = Stopwatch.GetTimestamp();
        
            // For now, disable these two controls until they are ready
            // for prime time
            displayBoneInformationToolStripMenuItem.Visible = false;
            showNormalsGroupBox.Visible = false;
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
            if (repositoryDirectoryList.Count > 0)
                RepositoryClass.Instance.InitializeRepository(repositoryDirectoryList);
            else
                RepositoryClass.Instance.InitializeRepositoryPath();
            
            // get a reference to the engine singleton
            engine = new Root(null, null);
            // retrieve the max FPS, if it exists
            getMaxFPSFromRegistry();
            
            // add event handlers for frame events
            engine.FrameStarted += new FrameEvent(OnFrameStarted);
            engine.FrameEnded += new FrameEvent(OnFrameEnded);

            // make the collisionAPI object
            collisionManager = new CollisionAPI(false);
            
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

            materialSchemeNames = MaterialManager.Instance.SchemeNames;
            if (materialSchemeNames.Count > 1)
                materialSchemeToolStripMenuItem.Enabled = true;
            
            // call the overridden CreateScene method
            CreateScene();

            InitializeAxiomControlCallbacks();

            return true;
        }


        protected void InitializeAxiomControlCallbacks() {
            this.axiomPictureBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.axiomPictureBox_MouseWheel);
            this.axiomPictureBox.Click += new System.EventHandler(this.axiomPictureBox_Click);
            this.axiomPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axiomPictureBox_MouseDown);
            this.axiomPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.axiomPictureBox_MouseMove);
            this.axiomPictureBox.Resize += new System.EventHandler(this.axiomPictureBox_Resize);
            this.axiomPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axiomPictureBox_MouseUp);
        }

        protected bool SetupResources()
		{
            RepositoryClass.Instance.InitializeRepositoryPath();
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
        
//      protected void SetupResources()
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
            sceneManager = Root.Instance.SceneManagers.GetSceneManager(SceneType.ExteriorClose);
        }

        protected void CreateCamera()
        {
            camera = sceneManager.CreateCamera("PlayerCam");

            //camera.Position = new Vector3(0 * oneMeter, 55 * oneMeter, 10 * oneMeter);
            //camera.LookAt(new Vector3(0, 50 * oneMeter, 0));
            camera.Near = 0.1f * OneMeter;
			camera.Far = 1000 * OneMeter;
            camera.AspectRatio = (float)window.Width / window.Height;
			setTrackBarFromCameraNear();
            PositionCamera();
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
            engine.FrameStarted += new FrameEvent(this.CheckInitialModel);
            // start the engines rendering loop
            engine.StartRendering();

            return true;
        }

        #endregion Application Startup Code

        #region Scene Setup

        protected void GenerateScene()
        {

            ((Axiom.SceneManagers.Multiverse.SceneManager)sceneManager).SetWorldParams(terrainGenerator, new LODSpec());
            sceneManager.LoadWorldGeometry("");

            AmbientLightColor = new ColorEx(0.5f, 0.5f, 0.5f);

            // set up directional light
            directionalLight = sceneManager.CreateLight("MainLight");
            directionalLight.Type = LightType.Directional;
            directionalLight.SetAttenuation(1000 * OneMeter, 1, 0, 0);

            DirectionalDiffuseColor = ColorEx.White;
            DirectionalSpecularColor = ColorEx.White;

            PositionLight();

            // create and position the scene node used to display the loaded model
            modelNode = sceneManager.RootSceneNode.CreateChildSceneNode();
            modelNode.Position = modelBase;

            helperNode = sceneManager.RootSceneNode.CreateChildSceneNode();
            helperNode.Position = modelBase;

            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = false;
            // Set our DisplayTerrain property to the current value.  
            // This will set the desired SceneManager properties.
            this.DisplayTerrain = displayTerrain;
            //particleNode = scene.RootSceneNode.CreateChildSceneNode();
            //particleNode.Position = new Vector3(0 * oneMeter, 50 * oneMeter, 0 * oneMeter);

            //ParticleSystem ps = ParticleSystemManager.Instance.CreateSystem("foo", "PEExamples/ringOfFire");
            //particleNode.AttachObject(ps);
            //particleNode.ScaleFactor = new Vector3(1000f, 1000f, 1000f);
            //ps.ShowBoundingBox = true;

            return;
        }

        /// <summary>
        ///   Create a ground plane mesh 
        /// </summary>
        protected float GetGroundPlaneSize() {
            int minPlaneSize = 5 * (int)OneMeter;
            if (loadedModel != null) {
                float maxDim = 0;
                if (loadedModel.BoundingBox.Maximum.x > maxDim)
                    maxDim = loadedModel.BoundingBox.Maximum.x;
                if ((-1 * loadedModel.BoundingBox.Minimum.x) > maxDim)
                    maxDim = -1 * loadedModel.BoundingBox.Minimum.x;
                if (loadedModel.BoundingBox.Maximum.z > maxDim)
                    maxDim = loadedModel.BoundingBox.Maximum.z;
                if ((-1 * loadedModel.BoundingBox.Minimum.z) > maxDim)
                    maxDim = -1 * loadedModel.BoundingBox.Minimum.z;
                if (maxDim == 0)
                    maxDim = .001f * OneMeter;
                minPlaneSize = (int)(maxDim * 2);
            }

            // This does some trippy logic so we have a set of tiles, each 
            // of which are some power of 10 meters.
            double mag = Math.Log(minPlaneSize, 10);
            double fraction = mag - Math.Floor(mag);
            // factor is a number from [0, 10)
            double factor = Math.Pow(10, fraction);
            int tiles = (factor == 0) ? 2 : 2 * (int)Math.Ceiling(factor / 2);
            float planeSize = (float)Math.Pow(10, Math.Floor(mag)) * tiles;
            return planeSize;
        }

        private void CheckInitialModel(object sender, FrameEventArgs args) {
            if (initial_model != null) {
                LoadMesh(initial_model);
                initial_model = null;
            }
        }

        private void Regenerate()
        {
            GenerateScene();
        }

        protected void CreateScene()
        {
            viewport.BackgroundColor = ColorEx.DimGray;
            viewport.OverlaysEnabled = false;

            GenerateScene();

            sceneManager.SetFog(FogMode.None, ColorEx.White, .008f, 0, 1000 * OneMeter);

            return;
        }

        #endregion Scene Setup


        #region Axiom Frame Event Handlers

        protected void OnFrameEnded(object source, FrameEventArgs e)
        {
            return;
        }

        protected AnimationState GetAnimationState(string animName) {
            return loadedModel.GetAnimationState(animName);
        }

        protected void StartAnimation(string animName, bool looping, float animSpeed)
        {
            animationPlaying = true;
            animationLooping = looping;
            animationSpeed = animSpeed;
            playStopButton.Text = "Stop";
        }

        protected void StopAnimation() {
            animationPlaying = false;
            // SetAnimationTime(0);
        }

        protected void PauseAnimation() {
            animationPlaying = false;
            playStopButton.Text = "Play";
        }

        protected void SetAnimation(AnimationState state) {
            // If we are replacing an existing animation, stop
            // that animation.
            if (currentAnimation != null && currentAnimation != state)
               currentAnimation.IsEnabled = false;
            // If they are the same animation, leave the time alone
            float time = 0;
            if (currentAnimation != null && state != null && 
                state.Name == currentAnimation.Name)
                time = currentAnimation.Time;
            // Set our current animation, as well as the labels and the time
            currentAnimation = state;
            timeStartLabel.Text = string.Format("{0:f}", 0);
            timeEndLabel.Text = string.Format("{0:f}", 0);
            if (currentAnimation != null) {
                // currentAnimation.IsEnabled = true;
                timeEndLabel.Text = string.Format("{0:f}", currentAnimation.Length);
                animationTrackBar.Minimum = 0;
                animationTrackBar.Maximum = (int)Math.Round(currentAnimation.Length * 30);
                // Set up tickFreq so there are between 10 and 100 ticks
                int tickFreq = 1;
                while (tickFreq * 100 < animationTrackBar.Maximum)
                    tickFreq *= 10;
                animationTrackBar.TickFrequency = tickFreq;
            }
            SetAnimationTime(time);
        }

        public void SetAnimationTime(float time) {
            if (currentAnimation != null) {
                currentAnimation.Time = time;
                timeCurrentLabel.Text = string.Format("{0:f}", currentAnimation.Time);
                if (currentAnimation.Length > 0)
                    animationTrackBar.Value = (int)(currentAnimation.Time / currentAnimation.Length * animationTrackBar.Maximum);
                else
                    animationTrackBar.Value = animationTrackBar.Maximum;
            } else {
                animationTrackBar.Value = animationTrackBar.Minimum;
                timeCurrentLabel.Text = string.Format("{0:f}", 0);
            }
            if (boneDisplay != null)
                boneDisplay.UpdateFields();
        }

        protected void RunAnimation(float advanceTime)
        {
            advanceTime = advanceTime * animationSpeed;

            float time = currentAnimation.Time + advanceTime;
            if (animationLooping) {
                while (time >= currentAnimation.Length)
                    time -= currentAnimation.Length;
            } else if (time >= currentAnimation.Length) {
                time = currentAnimation.Length;
                PauseAnimation();
            }
            SetAnimationTime(time);
        }

        protected void OnFrameStarted(object source, FrameEventArgs e)
        {
            long curTimer = Stopwatch.GetTimestamp();

            float frameTime = (float)(curTimer - lastFrameTime) / (float)timerFreq;
            lastFrameTime = curTimer;

            int maxFPS = Root.Instance.MaxFramesPerSecond;
            fpsStatusValueLabel.Text = Root.Instance.CurrentFPS.ToString() + (maxFPS > 0 ? " [Limited]" : "");

            if (quit)
            {
                Root.Instance.QueueEndRendering();

                return;
            }

            float scaleMove = 100 * frameTime * OneMeter;

            time += frameTime;
            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.Time = time;

            if (leftMouseClick) {
// 				if (loadedMesh.TriangleIntersector != null) {
// 					Vector3 intersection;
// 					string loc;
// 					Ray ray = camera.GetCameraToViewportRay((float)newMouseX/axiomPictureBox.Size.Width,
// 															(float)newMouseY/axiomPictureBox.Size.Height);
// 					if (loadedMesh.TriangleIntersector.ClosestRayIntersection(ray, modelBase, camera.Near, out intersection))
// 						loc = "Model Click Coords: " + intersection.ToString();
// 					else 
// 						loc = "Did Not Click On Model";
// 					lastClickCoords.Text = loc;
// 				}
				leftMouseClick = false;
			}
			
			if (leftMouseDown || rightMouseDown || newMouseDelta != 0)
            {
                float deltaX = ( lastMouseX - newMouseX ) * mouseScale;
                float deltaY = ( lastMouseY - newMouseY ) * mouseScale;

                // 120 is the mouse wheel amount suggested for scrolling one line
                int deltaZ = -1 * newMouseDelta / 120;
                newMouseDelta = 0;

                if (deltaX != 0)
                {

                    if (leftMouseDown)
                    {
                        switch (mouseMode)
                        {
                            case MouseMode.MoveCamera:

                                CameraAzimuth += deltaX;

                                break;
                            case MouseMode.MoveDirectionalLight:
                                LightAzimuth += deltaX;
                                break;
                        }
                    }
                    lastMouseX = newMouseX;
                }
                if (deltaY != 0)
                {
                    if (leftMouseDown)
                    {
                        switch (mouseMode)
                        {
                            case MouseMode.MoveCamera:
                                CameraZenith += deltaY;
                                break;
                            case MouseMode.MoveDirectionalLight:
                                LightZenith -= deltaY;
                                break;
                        }
                    }
                    else if (rightMouseDown)
                    {
                        if (mouseMode == MouseMode.MoveCamera)
                        {
                            CameraRadius -= (deltaY * OneMeter / 10f);
                        }
                    }

                    lastMouseY = newMouseY;
                }
                if (deltaZ != 0) {
                    cameraRadius *= (float)Math.Pow(1.1, deltaZ);
                    updateCamera = true;
                }
                if (updateCamera)
                {
                    PositionCamera();
                }
            }

            if (spinCamera)
            {
                float rot = frameTime * MathUtil.DEGREES_PER_RADIAN;

                CameraAzimuth += rot;
            }

            if (spinLight)
            {
                float rot = frameTime;
                LightAzimuth += rot;
            }

            if (updateLight)
            {
                PositionLight();
            }

            if (updateCamera)
            {
                PositionCamera();
            }

            if (animationPlaying)
            {
                RunAnimation(frameTime);
            }

            if (showBones && !bonesShown) {
                SetupSkeletonRenderable();
            } else if (bonesShown && !showBones) {
                CleanupSkeletonRenderable();
            }

            if (showSockets && !socketsShown) {
                SetupSocketRenderables();
            } else if (socketsShown && !showSockets) {
                CleanupSocketRenderables();
            }

            if (showGroundPlane && !groundPlaneShown) {
                SetupGroundPlaneRenderable();
            } else if (groundPlaneShown && !showGroundPlane) {
                CleanupGroundPlaneRenderable();
            }

            if (showNormals && !normalsShown) {
                SetupNormalsRenderable();
            } else if (normalsShown && !showNormals) {
                CleanupNormalsRenderable();
            }

            SceneNode attachedSceneNode = sceneManager.GetSceneNode("_skeletonSceneNode_");
            if (attachedSceneNode != null)
                attachedSceneNode.Visible = showBones;
        }

        #endregion Axiom Frame Event Handlers

        private void LaunchDoc(string anchor)
        {
            string WebDocsString = "http://update.multiverse.net/wiki/index.php/Using_Model_Viewer_Version_1.5";

            string target = String.Format("{0}#{1}", WebDocsString, anchor); 
            System.Diagnostics.Process.Start(target); 
        }

        private static Mesh ReadMesh(Matrix4 transform, string srcDir, string meshFile) {
            Stream meshData = new FileStream(srcDir + meshFile, FileMode.Open);
            Mesh mesh = new Mesh(meshFile);
            if (meshFile.EndsWith(".mesh", StringComparison.CurrentCultureIgnoreCase)) {
                MeshSerializer meshReader = new MeshSerializer();
                meshReader.ImportMesh(meshData, mesh);
            } else if (meshFile.EndsWith(".mesh.xml", StringComparison.CurrentCultureIgnoreCase)) {
                OgreXmlMeshReader meshReader = new OgreXmlMeshReader(meshData);
                meshReader.Import(mesh);
            } else if (meshFile.EndsWith(".dae", StringComparison.CurrentCultureIgnoreCase)) {
                string extension = Path.GetExtension(meshFile);
                string baseFile = Path.GetFileNameWithoutExtension(meshFile);
                string basename = meshFile.Substring(0, meshFile.Length - extension.Length);
                ColladaMeshReader meshReader = new ColladaMeshReader(meshData, baseFile);
                // import the .dae file
                meshReader.Import(transform, mesh, null, "idle", basename);
                // materialScript = meshReader.MaterialScript;
            } else {
                meshData.Close();
                string extension = Path.GetExtension(meshFile);
                throw new AxiomException("Unsupported mesh format '{0}'", extension);
            }
            meshData.Close();
            return mesh;
        }

        /// <summary>
        ///   Add the collision data for an object.  This involves looking 
        ///   for a physics file that matches the mesh file's name, and 
        ///   loading the information from that file to build collision 
        ///   volumes.
        /// </summary>
        /// <param name="objNode">the object for which we are adding the collision data</param>
        private void AddCollisionObject(Entity modelEntity, SceneNode modelSceneNode,
                                        long oid, string physicsName) {
            // Create a set of collision shapes for the object
            List<CollisionShape> shapes = new List<CollisionShape>();
            PhysicsData pd = new PhysicsData();
            PhysicsSerializer ps = new PhysicsSerializer();

            try {
                Stream stream = ResourceManager.FindCommonResourceData(physicsName);
                ps.ImportPhysics(pd, stream);
                for (int i=0; i<modelEntity.SubEntityCount; i++) {
                    SubEntity subEntity = modelEntity.GetSubEntity(i);
                    if (subEntity.IsVisible) {
                        string submeshName = subEntity.SubMesh.Name;
                        List<CollisionShape> subEntityShapes = pd.GetCollisionShapes(submeshName);
                        foreach (CollisionShape subShape in subEntityShapes) {
                            // static objects will be transformed here, but movable objects
                            // are transformed on the fly
                            subShape.Transform(modelSceneNode.DerivedScale,
                                               modelSceneNode.DerivedOrientation,
                                               modelSceneNode.DerivedPosition);
                            shapes.Add(subShape);
                            log.InfoFormat("Added collision shape: {0} for {1}",
                                           subShape, submeshName);
                        }
                    }
                }
            } catch (Exception e) {
                // Unable to load physics data -- use a sphere or no collision data?
                log.WarnFormat("Unable to load physics data: {0}", e);
            }
            foreach (CollisionShape shape in shapes)
                collisionManager.AddCollisionShape(shape, oid);
        }


        /// <summary>
        ///   Remove the associated collision data from an object.
        /// </summary>
        /// <param name="objNode">the object for which we are removing the collision data</param>
        private void RemoveCollisionObjects(long oid) {
            collisionManager.RemoveCollisionShapesWithHandle(oid);
        }

        private LineRenderable CreateBoneLines(Skeleton skeleton) {
            List<Vector3> points = new List<Vector3>();
            List<ColorEx> colors = new List<ColorEx>();
            List<VertexBoneAssignment> vbas = new List<VertexBoneAssignment>();
            int vertexIndex = 0;
            VertexBoneAssignment vba;
            foreach (Bone bone in skeleton.Bones) {
                Matrix4 bindTransform = bone.BindDerivedInverseTransform.Inverse();
                Vector3 bonePosition = bindTransform.Translation;
                Bone parentBone = null;
                if (bone.Parent != null)
                    parentBone = bone.Parent as Bone;
                // If we have a parent bone, draw a line to the parent bone
                if (parentBone != null) {
                    points.Add(parentBone.BindDerivedInverseTransform.Inverse().Translation);
                    points.Add(bonePosition);
                    colors.Add(ColorEx.Cyan);
                    colors.Add(ColorEx.Cyan);
                    // Set up the vba for the bone base
                    vba = new VertexBoneAssignment();
                    vba.vertexIndex = vertexIndex++;
                    vba.weight = 1.0f;
                    vba.boneIndex = parentBone.Handle;
                    vbas.Add(vba);
                    // Set up the vba for the bone end
                    vba = new VertexBoneAssignment();
                    vba.vertexIndex = vertexIndex++;
                    vba.weight = 1.0f;
                    vba.boneIndex = bone.Handle;
                    vbas.Add(vba);
                }
                // Set up axis lines for this entry
                // X axis line
                points.Add(bonePosition);
                points.Add(bindTransform * (Vector3.UnitX * boneAxisLength));
                colors.Add(ColorEx.Red);
                colors.Add(ColorEx.Red);
                vba = new VertexBoneAssignment();
                vba.vertexIndex = vertexIndex++;
                vba.weight = 1.0f;
                vba.boneIndex = bone.Handle;
                vbas.Add(vba);
                vba = new VertexBoneAssignment();
                vba.vertexIndex = vertexIndex++;
                vba.weight = 1.0f;
                vba.boneIndex = bone.Handle;
                vbas.Add(vba);
                // Y axis line
                points.Add(bonePosition);
                points.Add(bindTransform * (Vector3.UnitY * boneAxisLength));
                colors.Add(ColorEx.Blue);
                colors.Add(ColorEx.Blue);
                vba = new VertexBoneAssignment();
                vba.vertexIndex = vertexIndex++;
                vba.weight = 1.0f;
                vba.boneIndex = bone.Handle;
                vbas.Add(vba);
                vba = new VertexBoneAssignment();
                vba.vertexIndex = vertexIndex++;
                vba.weight = 1.0f;
                vba.boneIndex = bone.Handle;
                vbas.Add(vba);
                // Z axis line
                points.Add(bonePosition);
                points.Add(bindTransform * (Vector3.UnitZ * boneAxisLength));
                colors.Add(ColorEx.Lime);
                colors.Add(ColorEx.Lime);
                vba = new VertexBoneAssignment();
                vba.vertexIndex = vertexIndex++;
                vba.weight = 1.0f;
                vba.boneIndex = bone.Handle;
                vbas.Add(vba);
                vba = new VertexBoneAssignment();
                vba.vertexIndex = vertexIndex++;
                vba.weight = 1.0f;
                vba.boneIndex = bone.Handle;
                vbas.Add(vba);
            }
            
            LineRenderable lines = new LineRenderable();
            lines.SetPoints(points, colors, vbas);
            lines.MaterialName = "MVSkinnedLines";
            lines.Skeleton = skeleton;
            return lines;
        }

        private static void BuildAxisLines(List<Vector3> points, List<ColorEx> colors, Vector3 origin,
                                           Vector3 xVector, Vector3 yVector, Vector3 zVector) {
            // The three axis lines
            if (!xVector.IsZero) {
                points.Add(origin);
                points.Add(origin + xVector);
                colors.Add(ColorEx.Red);
                colors.Add(ColorEx.Red);
            }
            if (!yVector.IsZero) {
                points.Add(origin);
                points.Add(origin + yVector);
                colors.Add(ColorEx.Blue);
                colors.Add(ColorEx.Blue);
            }
            if (!zVector.IsZero) {
                points.Add(origin);
                points.Add(origin + zVector);
                colors.Add(ColorEx.Lime);
                colors.Add(ColorEx.Lime);
            }
        }

        private static void BuildCubeLines(List<Vector3> points, List<ColorEx> colors, float size) {
            // The 12 cube lines
            // three lines from 1,1,1
            int[,] signs = new int[4, 3];
            signs[0, 0] = 1;
            signs[0, 1] = 1;
            signs[0, 2] = 1;
            signs[1, 0] = 1;
            signs[1, 1] = -1;
            signs[1, 2] = -1;
            signs[2, 0] = -1;
            signs[2, 1] = 1;
            signs[2, 2] = -1;
            signs[3, 0] = -1;
            signs[3, 1] = -1;
            signs[3, 2] = 1;
            // For each of four points, draw a line down three axes.
            // This gives us our cube.
            for (int i = 0; i < 4; ++i) {
                for (int j = 0; j < 3; ++j) {
                    Vector3 src = Vector3.Zero;
                    Vector3 dst = Vector3.Zero;
                    for (int k = 0; k < 3; ++k)
                        src[k] = signs[i, k];
                    dst = src;
                    dst[j] = -1 * dst[j];
                    points.Add(.5f * size * src);
                    points.Add(.5f * size * dst);
                    colors.Add(ColorEx.Orange);
                    colors.Add(ColorEx.Orange);
                }
            }
        }

        private LineRenderable CreateAxisLines(bool selected) {
            List<Vector3> points = new List<Vector3>();
            List<ColorEx> colors = new List<ColorEx>();

            BuildAxisLines(points, colors, Vector3.Zero,
                           Vector3.UnitX * socketAxisLength, 
                           Vector3.UnitY * socketAxisLength, 
                           Vector3.UnitZ * socketAxisLength);

            if (selected)
                BuildCubeLines(points, colors, selectBoxSize);

            LineRenderable lines = new LineRenderable();
            lines.SetPoints(points, colors);
            lines.MaterialName = "MVLines";
            return lines;
        }

        private LineRenderable CreateGridLines(float size, int majorDivisions, int minorDivisions)
        {
            List<Vector3> points = new List<Vector3>();
            List<ColorEx> colors = new List<ColorEx>();

            ColorEx axisColor = ColorEx.Black;
            ColorEx majorColor = ColorEx.DarkGray;
            ColorEx minorColor = ColorEx.Gray;
            float majorIncrement = size / majorDivisions;
            float minorIncrement = size / majorDivisions / minorDivisions;
            float x_max = size / 2;
            float x_min = -1 * size / 2;
            float z_max = size / 2;
            float z_min = -1 * size / 2;
            for (int i = 0; i < majorDivisions + 1; ++i)
            {
                for (int j = 0; j < minorDivisions; j++)
                {
                    float x = x_min + i * majorIncrement + j * minorIncrement;
                    points.Add(new Vector3(x, 0, z_min));
                    points.Add(new Vector3(x, 0, z_max));
                    if (j == 0 && x == 0)
                    {
                        colors.Add(axisColor);
                        colors.Add(axisColor);
                    }
                    else if (j == 0)
                    {
                        colors.Add(majorColor);
                        colors.Add(majorColor);
                    }
                    else
                    {
                        colors.Add(minorColor);
                        colors.Add(minorColor);
                    }
                    float z = z_min + i * majorIncrement + j * minorIncrement;
                    points.Add(new Vector3(x_min, 0, z));
                    points.Add(new Vector3(x_max, 0, z));
                    if (j == 0 && z == 0)
                    {
                        colors.Add(axisColor);
                        colors.Add(axisColor);
                    }
                    else if (j == 0)
                    {
                        colors.Add(majorColor);
                        colors.Add(majorColor);
                    }
                    else
                    {
                        colors.Add(minorColor);
                        colors.Add(minorColor);
                    }
                    // On the last set, don't generate more minor lines
                    if (i == majorDivisions)
                        break;
                }
            }

            LineRenderable lines = new LineRenderable();
            lines.SetPoints(points, colors);
            lines.MaterialName = "MVGridLines";
            return lines;
        }
        private static Vector3[] ReadBuffer(HardwareVertexBuffer vBuffer, VertexElement elem, int vertexCount)
        {
            Vector3[] rv = new Vector3[vertexCount];
            if (vBuffer == null) {
                for (int i = 0; i < vertexCount; ++i)
                    rv[i] = Vector3.Zero;
                return rv;
            }
            float[,] data = new float[vertexCount, 3];
            HardwareBufferHelper.ReadBuffer( data, vBuffer, elem );
            for (int i = 0; i < vertexCount; ++i)
                for (int j = 0; j < 3; ++j)
                    rv[i][j] = data[i, j];
            return rv;
        }

        private LineRenderable CreateNormalLines() {
            List<Vector3> points = new List<Vector3>();
            List<ColorEx> colors = new List<ColorEx>();

            foreach (SubEntity subEntity in loadedModel.SubEntities) {
                if (subEntity.IsVisible) {
                    VertexData vData = subEntity.SubMesh.VertexData;
                    VertexElement[] elems = new VertexElement[4];
                    elems[0] = vData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
                    elems[1] = vData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Normal);
                    elems[2] = vData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Tangent);
                    elems[3] = vData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Binormal);
                    HardwareVertexBuffer[] vBuffers = new HardwareVertexBuffer[4];
                    for (int i = 0; i < 4; ++i) {
                        if (elems[i] == null)
                            vBuffers[i] = null;
                        else
                            vBuffers[i] = vData.vertexBufferBinding.Bindings[elems[i].Source];
                    }
                    Vector3[] positionVectors = ReadBuffer(vBuffers[0], elems[0], vData.vertexCount);
                    Vector3[] normalVectors   = ReadBuffer(vBuffers[1], elems[1], vData.vertexCount);
                    Vector3[] tangentVectors  = ReadBuffer(vBuffers[2], elems[2], vData.vertexCount);
                    Vector3[] binormalVectors = ReadBuffer(vBuffers[3], elems[3], vData.vertexCount);
                    for (int i = 0; i < vData.vertexCount; ++i)
                        BuildAxisLines(points, colors, positionVectors[i],
                                       normalsAxisLength * normalVectors[i], 
                                       normalsAxisLength * tangentVectors[i], 
                                       normalsAxisLength * binormalVectors[i]);
                }
            }

            if (points.Count == 0)
                return null;
            LineRenderable lines = new LineRenderable();
            lines.SetPoints(points, colors);
            lines.MaterialName = "MVLines";
            
            return lines;
        }

        private void LoadMesh(string filename)
        {
            int lastBackslash = filename.LastIndexOf('\\');
            string clippedFilename = filename.Substring(lastBackslash + 1);
            string srcDir = filename.Substring(0, lastBackslash + 1);
            SetAnimation(null);
            UpdateAnimationListBox();

            if (boneDisplay != null) {
                boneDisplay.Close();
                boneDisplay = null;
            }

            if (loadedModel != null)
            {
                // clean up the last model if we are loading a new one
                RemoveCollisionObjects(0);
                CleanupSockets();
                CleanupBones();
                CleanupNormalsRenderable();
                Debug.Assert(attachedNodes.Count == 0);
                modelNode.DetachObject(loadedModel);
                sceneManager.RemoveEntity(loadedModel);
            }
            CleanupGroundPlaneRenderable();

            Mesh mesh = ReadMesh(Matrix4.Identity, srcDir, clippedFilename);
            SetupPoseAnimation(mesh);
//             // Create the list of triangles used to query mouse hits
// 			mesh.CreateTriangleIntersector();
			loadedMesh = mesh;

            loadedModel = sceneManager.CreateEntity("model", mesh);
			Text = "ModelViewer: " + filename;

            // move the camera focus to the middle of the new model
            ModelHeight = loadedModel.BoundingBox.Maximum.y - loadedModel.BoundingBox.Minimum.y;

            if (CameraRadius < loadedModel.BoundingRadius)
            {
                CameraRadius = loadedModel.BoundingRadius;
            }

            PositionCamera();

            modelNode.AttachObject(loadedModel);

            AddCollisionObject(loadedModel, modelNode, 0, 
                               Path.GetFileNameWithoutExtension(clippedFilename) + ".physics");

            //modelNode.DetachObject(loadedModel);
            loadedModel.ShowBoundingBox = showBoundingBox;

            if (loadedModel.Mesh != null && loadedModel.Mesh.ContainsAnimation("manual")) {
                AnimationState manualAnimState = loadedModel.GetAnimationState("manual");
                manualAnimState.Time = 0;
                manualAnimState.IsEnabled = true;
            }

            PopulateSubMeshListBox();
            PopulateAnimationListBox();
            PopulateSocketListBox();
            PopulateBonesListBox();

            // disable the fields
            parentBoneLabel.Visible = false;
            parentBoneValueLabel.Text = "";
            parentBoneValueLabel.Visible = false;
        }

        public AttachmentPoint GetAttachmentPoint(string socketName) {
            if (loadedMesh == null)
                return null;
            List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
            attachmentPoints.AddRange(loadedMesh.AttachmentPoints);
            if (loadedMesh.Skeleton != null)
                attachmentPoints.AddRange(loadedMesh.Skeleton.AttachmentPoints);
            foreach (AttachmentPoint attachPoint in attachmentPoints) {
                if (attachPoint.Name == socketName)
                    return attachPoint;
            }
            return null;
        }

        /// <summary>
        ///   If we are displaying sockets, this method will remove the 
        ///   entities that have been created for display of the attachment 
        ///   points from the scene.
        /// </summary>
        public void CleanupSocketRenderables() {
            if (loadedModel == null) {
                Debug.Assert(attachedNodes.Count == 0);
                return;
            }
            List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
            attachmentPoints.AddRange(loadedMesh.AttachmentPoints);
            if (loadedMesh.Skeleton != null)
                attachmentPoints.AddRange(loadedMesh.Skeleton.AttachmentPoints);
            foreach (AttachmentPoint attachPoint in attachmentPoints)
                CleanupSocket(attachPoint);
            socketsShown = false;
        }

        public void CleanupSockets() {
            CleanupSocketRenderables();
            // Debug.Assert(attachedNodes.Count == 0);
            socketListBox.Items.Clear();
        }

        public void CleanupSocket(string socketName) {
            CleanupSocket(GetAttachmentPoint(socketName));
        }

        protected void CleanupSocket(AttachmentPoint attachPoint) {
            if (attachPoint == null)
                return;
            string sceneObjName = "_attachMesh_" + attachPoint.Name;
            CleanupAttachedObject(sceneObjName);
        }

        protected void CleanupAttachedObject(string name) {
            this.sceneManager.RemoveMovableObject(name, "SimpleRenderable");
            // Clear our attached nodes
            foreach (Node node in attachedNodes) {
                if (node is TagPoint) {
                    TagPoint tagPoint = node as TagPoint;
                    if (tagPoint.ChildObject.Name != name)
                        continue;
                    loadedModel.DetachObjectFromBone(tagPoint.Parent.Name, tagPoint);
                } else if (node is SceneNode) {
                    SceneNode sceneNode = node as SceneNode;
                    if (sceneNode.ObjectCount != 1)
                        continue;
                    // Run through the objects that are under this node.
                    // If the object is the one we were looking for, then
                    // remove this scene node.
                    foreach (MovableObject sceneObj in sceneNode.Objects) {
                        if (sceneObj.Name == name) {
                            sceneManager.DestroySceneNode(node.Name);
                            break;
                        }
                    }
                } else {
                    continue;
                }
                attachedNodes.Remove(node);
                return;
            }
        }

        /// <summary>
        ///   If we are displaying bones, this method will remove the 
        ///   movable object that has been created for display of the bones 
        ///   from the scene.
        /// </summary>
        public void CleanupSkeletonRenderable() {
            if (loadedModel == null)
                return;
            sceneManager.RemoveMovableObject("_skeleton_", "SimpleRenderable");
            helperNode.RemoveChild("_skeletonSceneNode_");
            if (sceneManager.GetSceneNode("_skeletonSceneNode_") != null)
                sceneManager.DestroySceneNode("_skeletonSceneNode_");
            bonesShown = false;
        }

        protected void CleanupBones() {
            bonesTreeView.Nodes.Clear();
            CleanupSkeletonRenderable();
        }

        protected void CleanupGroundPlaneRenderable() {
            // Remove the ground plane from the scene if it exists
            sceneManager.RemoveMovableObject("_groundPlane_", "SimpleRenderable");
            helperNode.RemoveChild("_groundPlaneSceneNode_");
            if (sceneManager.GetSceneNode("_groundPlaneSceneNode_") != null)
                sceneManager.DestroySceneNode("_groundPlaneSceneNode_");
            groundPlaneShown = false;
        }

        public void SetupSocket(string socketName, bool selected) {
            SetupSocket(GetAttachmentPoint(socketName), selected);
        }

        protected void SetupSocket(AttachmentPoint attachPoint, bool selected) {
            if (attachPoint == null)
                return;
            string sceneObjName = "_attachMesh_" + attachPoint.Name;
            MovableObject sceneObj = CreateAxisLines(selected);
            sceneObj.Name = sceneObjName;
            sceneManager.AddMovableObject(sceneObj);
            Node attachNode = null;
            if (attachPoint.ParentBone != null) {
                attachNode = loadedModel.AttachObjectToBone(attachPoint.ParentBone, sceneObj, attachPoint.Orientation, attachPoint.Position);
            } else {
                string attachNodeName = string.Format("_attachmentAxis.{0}", attachPoint.Name);
                SceneNode sceneNode = sceneManager.CreateSceneNode(attachNodeName);
                sceneNode.Orientation = attachPoint.Orientation;
                sceneNode.Position = attachPoint.Position;
                helperNode.AddChild(sceneNode);
                sceneNode.AttachObject(sceneObj);
                attachNode = sceneNode;
            }
            attachedNodes.Add(attachNode);
        }


        /// <summary>
        ///   If we are displaying sockets this method will create entities 
        ///   for display of attachment points and attach these entities to
        ///   the mesh.
        /// </summary>
        protected void SetupSocketRenderables() {
            if (loadedModel == null)
                return;
            Debug.Assert(attachedNodes.Count == 0);
            List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
            attachmentPoints.AddRange(loadedMesh.AttachmentPoints);
            if (loadedMesh.Skeleton != null)
                attachmentPoints.AddRange(loadedMesh.Skeleton.AttachmentPoints);
            foreach (AttachmentPoint attachPoint in attachmentPoints) {
                bool selected = false;
                for (int i = 0; i < socketListBox.Items.Count; ++i) {
                    if ((string)socketListBox.Items[i] == attachPoint.Name) {
                        selected = socketListBox.GetItemChecked(i);
                        break;
                    }
                }
                SetupSocket(attachPoint, selected);
            }
            socketsShown = true;
        }

        protected void SetupSkeletonRenderable() {
            if (loadedModel == null || loadedModel.Skeleton == null)
                return;
            MovableObject sceneObj = CreateBoneLines(loadedModel.Skeleton);
            sceneObj.Name = "_skeleton_";
            sceneManager.AddMovableObject(sceneObj);
            SceneNode sceneNode = sceneManager.CreateSceneNode("_skeletonSceneNode_");
            helperNode.AddChild(sceneNode);
            sceneNode.AttachObject(sceneObj);
            bonesShown = true;
        }

        protected void SetupGroundPlaneRenderable() {
            string sceneObjName = "_groundPlane_";
            float planeSize = GetGroundPlaneSize();
            MovableObject sceneObj = CreateGridLines(planeSize, 10, 5);
            sceneObj.Name = sceneObjName;
            sceneManager.AddMovableObject(sceneObj);
            SceneNode sceneNode = sceneManager.CreateSceneNode("_groundPlaneSceneNode_");
            helperNode.AddChild(sceneNode);
            sceneNode.AttachObject(sceneObj);
            groundPlaneShown = true;
        }

        /// <summary>
        ///   If we are displaying normals this method will create entities 
        ///   for display of the axis lines.
        /// </summary>
        protected void SetupNormalsRenderable() {
            if (loadedModel == null)
                return;
            if (normalsAxisLength == 0)
                return;
            MovableObject sceneObj = CreateNormalLines();
            if (sceneObj == null) {
                // this object has no normals.. just set the normalsAxisLength to 0
                normalsAxisLength = 0;
                return;
            }
            sceneObj.Name = "_normalLines_";
            sceneManager.AddMovableObject(sceneObj);
            SceneNode sceneNode = sceneManager.CreateSceneNode("_normalLinesSceneNode_");
            helperNode.AddChild(sceneNode);
            sceneNode.AttachObject(sceneObj);
            normalsShown = true;
        }

        /// <summary>
        ///   If we are displaying normals, this method will remove the 
        ///   movable object that has been created for display of the normals 
        ///   from the scene.
        /// </summary>
        public void CleanupNormalsRenderable() {
            if (loadedModel == null)
                return;
            sceneManager.RemoveMovableObject("_normalLines_", "SimpleRenderable");
            helperNode.RemoveChild("_normalLinesSceneNode_");
            if (sceneManager.GetSceneNode("_normalLinesSceneNode_") != null)
                sceneManager.DestroySceneNode("_normalLinesSceneNode_");
            normalsShown = false;
        }

        public string InitialModel {
            get {
                return initial_model;
            }
            set {
                initial_model = value;
            }
        }

        private void SetupPoseAnimation(Mesh mesh) {
            if (mesh != null && mesh.PoseList.Count > 0) {
                Animation anim = mesh.CreateAnimation("manual", 0);
                List<ushort> poseTargets = new List<ushort>();
                foreach (Pose pose in mesh.PoseList)
                    if (!poseTargets.Contains(pose.Target))
                        poseTargets.Add(pose.Target);
                foreach (ushort poseTarget in poseTargets) {
                    VertexAnimationTrack track = anim.CreateVertexTrack(poseTarget, VertexAnimationType.Pose);
                    VertexPoseKeyFrame manualKeyFrame = track.CreateVertexPoseKeyFrame(0);
                    // create pose references, initially zero
                    for (ushort poseIndex = 0; poseIndex < mesh.PoseList.Count; ++poseIndex) {
                        Pose pose = mesh.PoseList[poseIndex];
                        if (pose.Target == poseTarget)
                            manualKeyFrame.AddPoseReference(poseIndex, 0);
                    }
                }
            }

        }

        private void UpdatePoseAnimation(string poseName, string subMeshName, bool enabled) {
            if (!loadedModel.Mesh.ContainsAnimation("manual"))
                return;
            Animation anim = loadedModel.Mesh.GetAnimation("manual");
            if (enabled) {
                // Any time we check any pose, we want to enable the manual animation
                AnimationState manualAnimState = loadedModel.GetAnimationState("manual");
                manualAnimState.Time = 0;
                manualAnimState.IsEnabled = true;
            }
            foreach (VertexAnimationTrack track in anim.VertexTracks.Values) {
                for (ushort poseIndex = 0; poseIndex < loadedModel.Mesh.PoseList.Count; ++poseIndex) {
                    Pose pose = loadedModel.Mesh.PoseList[poseIndex];
                    if (pose.Target == track.Handle) {
                        VertexPoseKeyFrame keyFrame = track.KeyFrames[0] as VertexPoseKeyFrame;
                        // Is this the pose we are modifying
                        if (pose.Name == poseName)
                            keyFrame.UpdatePoseReference(poseIndex, enabled ? 1 : 0);
//                        else
//                            keyFrame.UpdatePoseReference(poseIndex, 0);
                    }
                }
            }
        }

        private void PopulateSubMeshListBox()
        {
            subMeshTreeView.Nodes.Clear();
            List<SubEntity> subEntities = new List<SubEntity>((List<SubEntity>)loadedModel.SubEntities);
            subEntities.Sort(delegate(SubEntity s1, SubEntity s2) { return s1.SubMesh.Name.CompareTo(s2.SubMesh.Name); });
            foreach (SubEntity subEntity in subEntities) {
                TreeNode node = new TreeNode(subEntity.SubMesh.Name);
                node.Name = subEntity.SubMesh.Name;
                node.Checked = subEntity.IsVisible;
                subMeshTreeView.Nodes.Add(node);
                List<Pose> poses = new List<Pose>(loadedModel.Mesh.PoseList);
                poses.Sort(delegate(Pose p1, Pose p2) { return p1.Name.CompareTo(p2.Name); });
                foreach (Pose pose in poses)
                {
                    bool include_pose = false;
                    if (pose.Target == 0 && subEntity.SubMesh.useSharedVertices)
                        // the pose is for the shared submesh
                        include_pose = true;
                    else if (pose.Target != 0) {
                        SubMesh subMesh = loadedModel.Mesh.GetSubMesh(pose.Target - 1);
                        if (subMesh == subEntity.SubMesh)
                            include_pose = true;
                    }
                    if (include_pose) {
                        TreeNode poseNode = new TreeNode(pose.Name);
                        poseNode.Name = pose.Name;
                        poseNode.Checked = false;
                        node.Nodes.Add(poseNode);
                    }
                }
            }
        }

        /// <summary>
        ///   Clear and repopulate the animation list box based on the
        ///   information in the model.
        /// </summary>
        private void PopulateAnimationListBox() {
            animationListBox.Items.Clear();
            if (loadedModel == null)
                return;
            AnimationStateSet animations = loadedModel.GetAllAnimationStates();
            foreach (AnimationState state in animations.Values) {
                animationListBox.Items.Add(state.Name);
                if (state.IsEnabled)
                    animationListBox.SelectedItem = state.Name;
            }
        }


        /// <summary>
        ///   Update our animation list box to reflect the animation states
        ///   of the model.  This method doesn't need to repopulate the 
        ///   control, but also doesn't handle changing the skeleton.
        /// </summary>
        private void UpdateAnimationListBox() {
            if (loadedModel == null)
                return;
            AnimationStateSet animations = loadedModel.GetAllAnimationStates();
            foreach (AnimationState state in animations.Values) {
                if (state.IsEnabled)
                    animationListBox.SelectedItem = state.Name;
            }
        }

        private void PopulateSocketListBox() {
            socketListBox.Items.Clear();
            if (loadedModel == null)
                return;
            List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
            attachmentPoints.AddRange(loadedMesh.AttachmentPoints);
            if (loadedMesh.Skeleton != null)
                attachmentPoints.AddRange(loadedMesh.Skeleton.AttachmentPoints);
            foreach (AttachmentPoint attachPoint in attachmentPoints) {
                socketListBox.Items.Add(attachPoint.Name);
                socketListBox.SetItemChecked(socketListBox.Items.Count - 1, false);
            }
            socketListBox.SelectedIndex = -1;
        }

        /// <summary>
        ///   Add an entry for bone, and recurse for the children of bone
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="parentNode">the node under which to add this bone</param>
        private void PopulateBonesListBoxHelper(Bone bone, TreeNode parentNode) {
            TreeNode node = new TreeNode(bone.Name);
            node.Checked = false;
            foreach (Bone childBone in bone.Children)
                PopulateBonesListBoxHelper(childBone, node);
            parentNode.Nodes.Add(node);
        }

        private void PopulateBonesListBox() {
            bonesTreeView.Nodes.Clear();
            if (loadedModel.Skeleton == null)
                return;
            for (int i = 0; i < loadedModel.Skeleton.RootBoneCount; ++i) {
                Bone bone = loadedModel.Skeleton.GetRootBone(i);
                TreeNode node = new TreeNode(bone.Name);
                node.Checked = false;
                foreach (Bone childBone in bone.Children)
                    PopulateBonesListBoxHelper(childBone, node);
                bonesTreeView.Nodes.Add(node);
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

        private bool DisplayTerrain {
            get {
                return displayTerrain;
            }
            set {
                displayTerrain = value;
                if (displayTerrain)
                    showGroundPlane = false;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.DrawTerrain = displayTerrain;
            }
        }


        private bool ShowGroundPlane {
            get {
                return showGroundPlane;
            }
            set {
                showGroundPlane = value;
                if (showGroundPlane)
                    this.DisplayTerrain = false;
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

        private bool SpinLight
        {
            get
            {
                return spinLight;
            }
            set
            {
                spinLight = value;
            }
        }

        private bool ShowBoundingBox
        {
            get
            {
                return showBoundingBox;
            }
            set
            {
                showBoundingBox = value;
                loadedModel.ShowBoundingBox = value;
            }
        }

        private bool ShowBones {
            get {
                return showBones;
            }
            set {
                showBones = value;
                showBonesCheckBox.Checked = showBones;
            }
        }

        private bool ShowSockets {
            get {
                return showSockets;
            }
            set {
                showSockets = value;
                showSocketsCheckBox.Checked = showSockets;
            }
        }

        private ColorEx AmbientLightColor
        {
            get
            {
                return sceneManager.AmbientLight;
            }
            set
            {
                sceneManager.AmbientLight = value;

                ambientLightColorButton.BackColor = value.ToColor();
            }
        }

        private ColorEx DirectionalDiffuseColor
        {
            get
            {
                return directionalLight.Diffuse;
            }
            set
            {
                directionalLight.Diffuse = value;
                diffuseColorButton.BackColor = value.ToColor();
            }
        }

        private ColorEx DirectionalSpecularColor
        {
            get
            {
                return directionalLight.Specular;
            }
            set
            {
                directionalLight.Specular = value;
                specularColorButton.BackColor = value.ToColor();
            }
        }

        private float LightAzimuth
        {
            get
            {
                return lightAzimuth;
            }
            set
            {
                lightAzimuth = value;
                while (lightAzimuth > 180)
                {
                    lightAzimuth -= 360;
                }
                while (lightAzimuth < -180)
                {
                    lightAzimuth += 360;
                }
                lightAzimuthTrackBar.Value = (int)Math.Round(lightAzimuth);
                updateLight = true;
            }
        }

        private float LightZenith
        {
            get
            {
                return lightZenith;
            }
            set
            {
                lightZenith = value;
                if (lightZenith > 90)
                {
                    lightZenith = 90;
                }
                if (lightZenith < -90)
                {
                    lightZenith = -90;
                }
                lightZenithTrackBar.Value = (int)Math.Round(lightZenith);
                updateLight = true;
            }
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
                updateCamera = true;
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
                updateCamera = true;
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
                updateCamera = true;
            }
        }

        private float CameraFocusHeight
        {
            get
            {
                return cameraFocusHeight;
            }
            set
            {
                cameraFocusHeight = value;
                cameraFocusHeightTrackbar.Value = (int)Math.Round(value * 100);
                updateCamera = true;
            }
        }

        private Vector3 CameraFocus
        {
            get
            {
                return new Vector3(modelBase.x, modelBase.y + modelHeight * CameraFocusHeight, modelBase.z);
            }
        }

        private float ModelHeight
        {
            get
            {
                return modelHeight;
            }
            set
            {
                modelHeight = value;
                modelHeightValueLabel.Text = String.Format("{0} Meters", modelHeight / 1000f);
            }
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
            displayTerrainToolStripMenuItem.Checked = DisplayTerrain;
            spinCameraToolStripMenuItem.Checked = SpinCamera;
            spinLightToolStripMenuItem.Checked = SpinLight;
            showBoundingBoxToolStripMenuItem.Checked = ShowBoundingBox;
            showGroundPlaneToolStripMenuItem.Checked = ShowGroundPlane;
        }

        private void displayTerrainToolStripMenuItem_Click(object sender, EventArgs e) {
            DisplayTerrain = !DisplayTerrain;
        }

        private void showGroundPlaneToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowGroundPlane = !ShowGroundPlane;
        }

        private void spinCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void showCollisionVolumesToolStripMenuItem_Click(object sender, EventArgs e) {
            if (loadedModel != null)
                collisionManager.ToggleRenderCollisionVolumes(sceneManager, false);
        }
        
        private void loadModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Load Model";
                dlg.Filter = "Multiverse Mesh files (*.mesh)|*.mesh|Ogre XML Files (*.mesh.xml)|*.mesh.xml|Collada DAE Files (*.dae)|*.dae|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                dlg.RestoreDirectory = true;
                if (Directory.Exists(lastDir)) {
                    dlg.InitialDirectory = lastDir;
                } else {
                    string startDir = string.Format("{0}\\Meshes", RepositoryClass.Instance.FirstRepositoryPath);
                    if (Directory.Exists(startDir) && first)
                        dlg.InitialDirectory = startDir;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadMesh(dlg.FileName);
                    lastDir = Path.GetDirectoryName(dlg.FileName);
                    first = false;
                }
            }
        }

        private void axiomPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftMouseDown = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                rightMouseDown = true;
            }
            lastMouseX = newMouseX = e.X;
            lastMouseY = newMouseY = e.Y;

        }

        private void axiomPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftMouseDown = false;
				leftMouseClick = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                rightMouseDown = false;
            }
        }

        private void axiomPictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (leftMouseDown || rightMouseDown) {
                newMouseX = e.X;
                newMouseY = e.Y;
            }
        }

        //private void axiomPictureBox_MouseEnter(object sender, EventArgs e) {
        //    axiomPictureBox.Focus();
        //}
        
        private void axiomPictureBox_MouseWheel(object sender, MouseEventArgs e) {
            newMouseDelta += e.Delta;
        }

        private void showAllButton_Click(object sender, EventArgs e)
        {
            // just check all the checkboxes, and the state change events will cause
            // the submeshes to all be turned visible.
            for (int i = 0; i < subMeshTreeView.Nodes.Count; i++)
            {
                subMeshTreeView.Nodes[i].Checked = true;
            }
        }

        private void hideAllButton_Click(object sender, EventArgs e)
        {
            // just un-check all the checkboxes, and the state change events will cause
            // the submeshes to all be turned invisible.
            for (int i = 0; i < subMeshTreeView.Nodes.Count; i++)
            {
                subMeshTreeView.Nodes[i].Checked = false;
            }
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            string matchText = subMeshTextBox.Text;
            for (int i = 0; i < subMeshTreeView.Nodes.Count; i++)
            {
                if (subMeshTreeView.Nodes[i].Name.Contains(matchText))
                {
                    subMeshTreeView.Nodes[i].Checked = true;
                }
            }
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            string matchText = subMeshTextBox.Text;
            for (int i = 0; i < subMeshTreeView.Nodes.Count; i++)
            {
                if (subMeshTreeView.Nodes[i].Name.Contains(matchText))
                {
                    subMeshTreeView.Nodes[i].Checked = false;
                }
            }
        }

        private void subMeshTextBox_TextChanged(object sender, EventArgs e)
        {
            if (subMeshTextBox.Text.Length == 0)
            {
                showButton.Enabled = false;
                hideButton.Enabled = false;
            }
            else
            {
                showButton.Enabled = true;
                hideButton.Enabled = true;
            }

        }

        private void playPauseButton_Click(object sender, EventArgs e)
        {
            if (animationListBox.SelectedItem == null)
                return;
            string animName = animationListBox.SelectedItem.ToString();
            
            if (animationPlaying)
            {
                PauseAnimation();
            }
            else
            {
                if (animName != null)
                {
                    bool looping = loopingCheckBox.Checked;
                    StartAnimation(animName, looping, float.Parse(animationSpeedTextBox.Text));
                    // Start the animation over from the beginning
                    if (!looping && currentAnimation != null &&
                        currentAnimation.Time == currentAnimation.Length)
                        SetAnimationTime(0);
                }
            }
        }

        private void moveCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.mouseModeToolStripButton.Image = global::ModelViewer.Properties.Resources.camera;
            mouseMode = MouseMode.MoveCamera;
        }

        private void moveDirectionalLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.mouseModeToolStripButton.Image = global::ModelViewer.Properties.Resources.lightbulb;
            mouseMode = MouseMode.MoveDirectionalLight;
        }

        private void spinLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpinLight = !SpinLight;
        }

        private void materialTextBox_TextChanged(object sender, EventArgs e) {
            if (subMeshTreeView.SelectedNode.Index >= 0) {
                Material mat = MaterialManager.Instance.GetByName(materialTextBox.Text);
                if (mat == null)
                    materialTextBox.ForeColor = Color.Red;
                else {
                    loadedModel.GetSubEntity(subMeshTreeView.SelectedNode.Name).MaterialName = mat.Name;
                    materialTextBox.ForeColor = Color.Black;
                }
            }
        }

        private void subMeshTreeView_ItemCheck(object sender, TreeViewEventArgs e) {
            TreeNode node = e.Node;
            if (node.Parent != null) {
                // this node is a pose
                // If we checked, uncheck all our peers
                if (node.Checked) {
                    foreach (TreeNode peerNode in node.Parent.Nodes)
                        if (peerNode != node) {
                            peerNode.Checked = false;
                            UpdatePoseAnimation(peerNode.Text, peerNode.Parent.Text, peerNode.Checked);
                        }
                }
                // We should generate/update a pose animation that causes
                // us to draw this pose.
                UpdatePoseAnimation(node.Text, node.Parent.Text, node.Checked);
                return;
            }
            SubEntity entity = loadedModel.GetSubEntity(node.Text);
            entity.IsVisible = node.Checked;
        }

        private void subMeshTreeView_SelectedIndexChanged(object sender, TreeViewEventArgs e) {
            if (subMeshTreeView.SelectedNode != null &&
                subMeshTreeView.SelectedNode.Index >= 0 &&
                subMeshTreeView.SelectedNode.Parent == null) {
                materialTextBox.Enabled = true;
                materialTextBox.Text = loadedModel.GetSubEntity(subMeshTreeView.SelectedNode.Name).MaterialName;
            } else
                materialTextBox.Enabled = false;
        }

        private void socketListBox_ItemCheck(object sender, ItemCheckEventArgs e) {
            if (e.CurrentValue == e.NewValue)
                return;
            // Clean up the sockets.. on the next frame, we will rebuild
            CleanupSocketRenderables();
        }

        private void socketListBox_SelectedIndexChanged(object sender, EventArgs e) {
            string parentBone = null;
            if (socketListBox.SelectedItem != null &&
                socketListBox.SelectedIndex >= 0) {
                // TODO: show the translate/rotate/scale/parentbone
                AttachmentPoint attachPoint = GetAttachmentPoint((string)socketListBox.SelectedItem);
                if (attachPoint != null)
                    parentBone = attachPoint.ParentBone;
            }
            if (parentBone == null) {
                // disable the fields
                parentBoneLabel.Visible = false;
                parentBoneValueLabel.Text = "";
                parentBoneValueLabel.Visible = false;
            } else {
                parentBoneLabel.Visible = true;
                parentBoneValueLabel.Text = parentBone;
                parentBoneValueLabel.Visible = true;
            }
        }

        private void bonesTreeView_SelectedIndexChanged(object sender, TreeViewEventArgs e) {
            // Later, I would like to display the translate/rotate/scale fields.
        }
        
        private void animationListBox_ItemCheck(object sender, EventArgs e) {
            if (loadedModel == null)
                return;
            AnimationStateSet animations = loadedModel.GetAllAnimationStates();
            foreach (AnimationState state in animations.Values) {
                if (state.Name == (string)animationListBox.SelectedItem)
                    state.IsEnabled = true;
                // else
                   // state.IsEnabled = false;
            }
        }

        private void animationListBox_SelectedIndexChanged(object sender, EventArgs e) {
            string animName = animationListBox.SelectedItem.ToString();
            AnimationState state = GetAnimationState(animName);
            StopAnimation();
            SetAnimation(state);
            playStopButton.Text = "Play";
        }

        private void showBoundingBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowBoundingBox = !ShowBoundingBox;
        }

        private void ambientLightButton_Click(object sender, EventArgs e)
        {
            ambientLightGroupBox.Visible = !ambientLightGroupBox.Visible;
        }

        private void ambientLightColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = AmbientLightColor.ToColor();
            colorDialog1.ShowDialog();

            AmbientLightColor = ColorEx.FromColor(colorDialog1.Color);
        }

        private void directionalLightButton_Click(object sender, EventArgs e)
        {
            directionalLightGroupBox.Visible = !directionalLightGroupBox.Visible;
        }

        private void diffuseColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = DirectionalDiffuseColor.ToColor();
            colorDialog1.ShowDialog();

            DirectionalDiffuseColor = ColorEx.FromColor(colorDialog1.Color);
        }

        private void specularColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = DirectionalSpecularColor.ToColor();
            colorDialog1.ShowDialog();

            DirectionalSpecularColor = ColorEx.FromColor(colorDialog1.Color);
        }

        private void lightAzimuthTrackBar_Scroll(object sender, EventArgs e)
        {
            LightAzimuth = lightAzimuthTrackBar.Value;
        }

        private void lightZenithTrackBar_Scroll(object sender, EventArgs e)
        {
            LightZenith = lightZenithTrackBar.Value;
        }

        private void ModelViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit = true;
        }

        private void helpToolStripHelpItem_Click(object sender, EventArgs e)
        {
            LaunchDoc("");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string msg = string.Format("Multiverse ModelViewer\n\nVersion: {0}\n\nCopyright 2006-2007 The Multiverse Network, Inc.\n\nPortions of this software are covered by additional copyrights and license agreements which can be found in the Licenses folder in this program's install folder.\n\nPortions of this software utilize SpeedTree technology.  Copyright 2001-2006 Interactive Data Visualization, Inc.  All rights reserved.", assemblyVersion);
            DialogResult result = MessageBox.Show(this, msg, "About Multiverse ModelViewer", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        }

        private void subMeshLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("submeshes");
        }

        private void ambientLightLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("lighting");
        }

        private void directionalLightLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("lighting");
        }

        private void animLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("anim");
        }

        private void socketsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            LaunchDoc("sockets");
        }

        private void cameraControlsButton_Click(object sender, EventArgs e)
        {
            cameraFocusHeightGroupBox.Visible = !cameraFocusHeightGroupBox.Visible;
            cameraNearDistanceGroupBox.Visible = !cameraNearDistanceGroupBox.Visible;
        }

        private void cameraFocusHeightTrackbar_Scroll(object sender, EventArgs e)
        {
            CameraFocusHeight = ( (float)cameraFocusHeightTrackbar.Value ) / 100f;
        }

        private void cameraControlsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchDoc("camera");
        }

        private void cameraNearDistanceTrackBar_Scroll(object sender, EventArgs e) {
            setCameraNearFromTrackBar();
        }

		protected void setTrackBarFromCameraNear()
		{
            if (camera.Near > 10f * OneMeter)
                cameraNearDistanceTrackBar.Value = 20 * (int)(camera.Near / (10f * OneMeter));
            else
                cameraNearDistanceTrackBar.Value = Math.Max(0, (int)((Math.Log10((double)camera.Near) - 2.0) * 20.0));
			setNearLabel();
		}

        protected void setCameraNearFromTrackBar()
		{
			// It's a logarithmic scale .01 meters to 10 meters, and then linear after that
			if (cameraNearDistanceTrackBar.Value > 20)
				camera.Near = OneMeter * 10f * ((float)(cameraNearDistanceTrackBar.Value - 20) / 20f);
			else
				camera.Near = OneMeter / 10f * (float)Math.Pow(10.0, (double)cameraNearDistanceTrackBar.Value / 20.0);
			setNearLabel();
		}
		
		protected void setNearLabel()
		{
            nearDistanceValueLabel.Text = string.Format("{0:f4} meters", camera.Near / OneMeter);
		}
		
        private void whatsNearDistanceLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            LaunchDoc("camera");
        }

        private void submitABugReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string BugReportURL = "http://www.multiverse.net/developer/feedback.jsp?Tool=ModelViewer";

            System.Diagnostics.Process.Start(BugReportURL); 
        }

        private void ModelViewer_Resize(object sender, EventArgs e)
        {
            if (Root.Instance == null)
                return;
            Root.Instance.SuspendRendering = this.WindowState == FormWindowState.Minimized;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DesignateRepository(true);
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
            }
            return true;
        }

        public bool AdvancedOptions {
            get {
                return advancedOptions;
            }
            set {
                advancedOptions = value;
                displayBoneInformationToolStripMenuItem.Visible = advancedOptions;
                showNormalsGroupBox.Visible = advancedOptions;
            }
        }
        
        protected void setMaximumFramesPerSecondToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveMaxFPSDialog saveMaxFPSDialog = new SaveMaxFPSDialog();
            if (saveMaxFPSDialog.ShowDialog() == DialogResult.OK)
            {
                int maxFPS = int.Parse(saveMaxFPSDialog.NumberBox.Text);
                Root.Instance.MaxFramesPerSecond = maxFPS;
                setMaxFPSInRegistry(maxFPS);
            }
        }

		protected static string modelViewerKey = Registry.CurrentUser + "\\Software\\Multiverse\\ModelViewer\\";

		public void setMaxFPSInRegistry(int maxFPS)
		{
			Registry.SetValue(modelViewerKey, "MaxFPS", maxFPS);
		}
		
		protected void getMaxFPSFromRegistry()
		{
            Object maxFPS = Registry.GetValue(modelViewerKey, "MaxFPS", null);
            if (maxFPS != null)
                Root.Instance.MaxFramesPerSecond = (int)maxFPS;
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
            string releaseNotesURL = "http://update.multiverse.net/wiki/index.php/Tools_Version_1.5_Release_Notes";

            System.Diagnostics.Process.Start(releaseNotesURL);
        }

        private void animationTrackBar_Scroll(object sender, System.EventArgs e) {
            PauseAnimation();
            if (currentAnimation == null)
                return;
            SetAnimationTime((animationTrackBar.Value * currentAnimation.Length) / animationTrackBar.Maximum);
        }

        #region Debugging Code (for slipping feet)
        public static Quaternion GetRotation(Matrix4 transform) {
            Matrix3 tmp =
                new Matrix3(transform.m00, transform.m01, transform.m02,
                            transform.m10, transform.m11, transform.m12,
                            transform.m20, transform.m21, transform.m22);
            float det = tmp.Determinant;
            float scale = (float)(1 / Math.Pow(det, 1.0 / 3.0));
            tmp = tmp * scale;
            Quaternion rv = Quaternion.Identity;
            rv.FromRotationMatrix(tmp);
            Debug.Assert(Math.Abs(1.0 - rv.Norm) < .001f, "Possible non-uniform scale factor on rotation matrix");
            // rv.Normalize();
            return rv;
        }

        private void DumpBones(StringBuilder message, Bone b, int indent) {
            string indentStr = new string(' ', indent);
            Quaternion q = GetRotation(b.FullTransform);
            message.Append(string.Format("{0}Bone: {1}\n", indentStr, b.Name));
            message.Append(string.Format("{0} pos: {1}\n", indentStr, b.FullTransform.Translation));
            message.Append(string.Format("{0} rot: {1}\n", indentStr, q));
            message.Append(string.Format("{0}rpos: {1}\n", indentStr, b.Position));
            message.Append(string.Format("{0}rrot: {1}\n", indentStr, b.Orientation));
            if (b.Parent != null) {
                Matrix4 relativeTransform = b.Parent.FullTransform.Inverse() * b.FullTransform;
                Quaternion q2 = GetRotation(relativeTransform);
                message.Append(string.Format("{0}ipos: {1}\n", indentStr, relativeTransform.Translation));
                message.Append(string.Format("{0}irot: {1}\n", indentStr, q2));
            }
            foreach (Bone child in b.Children) {
                if (child == null)
                    continue;
                DumpBones(message, child, indent + 1);
            }
        }

        private void displayBoneInformationToolStripMenuItem_Click(object sender, EventArgs e) {
            //if (currentAnimation != null) {
            //    StringBuilder message = new StringBuilder();
            //    message.Append(string.Format("Animation at {0:g}: {1}\n", currentAnimation.Time, currentAnimation.Name));
            //    Bone b = loadedModel.Skeleton.RootBone;
            //    DumpBones(message, b, 0);
            //    Trace.TraceInformation(message.ToString());
            //    // currentAnimation.
            //    // MessageBox.Show(message.ToString());
            //}   
            if (loadedModel != null && loadedModel.Skeleton != null) {
                if (boneDisplay == null)
                    boneDisplay = new BoneDisplay(this);
                boneDisplay.SetSkeleton(loadedModel.Skeleton);
                boneDisplay.Show();
                boneDisplay.UpdateFields();
            } else {
                MessageBox.Show("A model with a skeleton must be loaded in order to display bones");
            }
        }
        #endregion

        #region Properties
        public AnimationState CurrentAnimation {
            get {
                return currentAnimation;
            }
        }

        public List<string> RepositoryDirectoryList {
            get {
                return repositoryDirectoryList;
            }
            set {
                repositoryDirectoryList = value;
            }
        }

        #endregion

        private void axiomPictureBox_Click(object sender, EventArgs e) {
            this.axiomPictureBox.Focus();
        }

        private void showBonesToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowBones = !ShowBones;
        }

        private void showSocketsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSockets = !ShowSockets;
        }

        private void bonesLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            LaunchDoc("bones");
        }

        private void bgColorToolStripMenuItem_Click(object sender, EventArgs e) {
            colorDialog1.Color = viewport.BackgroundColor.ToColor();
            colorDialog1.ShowDialog();

            viewport.BackgroundColor = ColorEx.FromColor(colorDialog1.Color);
        }

        private void showBonesCheckBox_CheckedChanged(object sender, EventArgs e) {
            showBones = showBonesCheckBox.Checked;
        }

        private void showSocketsCheckBox_CheckedChanged(object sender, EventArgs e) {
            showSockets = showSocketsCheckBox.Checked;
        }

        private void socketAxisSizeTrackBar_Scroll(object sender, EventArgs e) {
            socketAxisLength = trackSizeFactor * socketAxisSizeTrackBar.Value;
            selectBoxSize = .5f * socketAxisLength;
            CleanupSocketRenderables(); // need to refresh these
        }

        private void boneAxisSizeTrackBar_Scroll(object sender, EventArgs e) {
            boneAxisLength = trackSizeFactor * boneAxisSizeTrackBar.Value;
            CleanupSkeletonRenderable(); // need to refresh these
        }

        private void normalsAxisSizeTrackBar_Scroll(object sender, EventArgs e) {
            normalsAxisLength = trackSizeFactor * normalsAxisSizeTrackBar.Value;
            CleanupNormalsRenderable(); // need to refresh these
        }

        private void subMeshTreeView_AfterSelect(object sender, TreeViewEventArgs e) {

        }

        private void materialSchemeToolStripMenuItem_Click(object sender, EventArgs e) {
            MaterialSchemeDialog schemeDialog = new MaterialSchemeDialog(materialSchemeNames, viewport.MaterialScheme);
            if (schemeDialog.ShowDialog() == DialogResult.OK)
                viewport.MaterialScheme = schemeDialog.getScheme();

        }

        private void showNormalsCheckBox_CheckedChanged(object sender, EventArgs e) {
            showNormals = showNormalsCheckBox.Checked;
        }
    }

    public enum MouseMode
    {
        MoveCamera,
        MoveDirectionalLight
    }

}
