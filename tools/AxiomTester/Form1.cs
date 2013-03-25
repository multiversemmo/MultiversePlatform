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

using Axiom.Animating;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Configuration;
using Axiom.Utility;
using Axiom.SceneManagers.Multiverse;
using Multiverse.Lib.LogUtil;
using Multiverse.AssetRepository;


namespace Multiverse.Tools.AxiomTester
{
    public partial class Form1 : Form
    {
        protected readonly float oneMeter = 1000.0f;

        protected Root engine;
        protected Camera camera;
        protected Viewport viewport;
        protected Axiom.Core.SceneManager scene;
        protected RenderWindow window;

        protected int inhibit = 0;
        protected bool quit = false;

        protected float time = 0;
        protected bool displayWireFrame = false;
        protected bool displayTerrain = false;
        protected bool displayOcean = false;
        protected bool spinCamera = false;

        protected int objectCount = 0;
        protected int totalVertexCount = 0;
        protected int numObjectsSharingMaterial = 0;

        protected bool randomSizes = false;
        protected bool randomScales = false;
        protected bool randomOrientations = false;
        
        protected Mesh unitBox;
        protected Mesh unitSphere;
        protected Mesh unitCylinder;
        protected Mesh zombieModel;
        protected Mesh humanFemaleModel;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Form1));
        private static string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string ClientAppDataFolder = Path.Combine(MyDocumentsFolder, "AxiomTester");
        private static string ConfigFolder = Path.Combine(ClientAppDataFolder, "Config");
        private static string LogFolder = Path.Combine(ClientAppDataFolder, "Logs");
        private static string FallbackLogfile = Path.Combine(LogFolder, "AxiomTester.log");
        private static string MeterEventsFile = Path.Combine(LogFolder, "MeterEvents.log");
        private static string MeterLogFile = Path.Combine(LogFolder, "MeterLog.log");

        protected enum WhichObjectsEnum {
            woBoxes = 0,
            woEllipsoids = 1,
            woCylinders = 2,
            woPlane = 3,
            woRandom = 4,
            woZombie = 5,
            woHumanFemale = 6
        }
        
        protected Mesh[] meshes;
        protected string[] animationNames;
        protected string[][] visibleSubMeshes;
        protected WhichObjectsEnum whichObjects = WhichObjectsEnum.woBoxes;
        protected bool animatedObjects;

        protected Random rand = new Random();
        
        protected List<SceneNode> sceneNodeList = new List<SceneNode>();
        protected List<Entity> entityList = new List<Entity>();
        protected List<Material> materialList = new List<Material>();
        protected List<Texture> textureList = new List<Texture>();
        protected Material prototypeMaterial;
        protected bool useTextures = false;
        protected bool uniqueTextures = false;

        protected int lastFrameRenderCalls = 0;
        protected long lastTotalRenderCalls = 0;

        protected int lastFrameSetPassCalls = 0;
        protected long lastTotalSetPassCalls = 0;
        protected long lastFrameTime = 0;
        protected float animationSpeed = 1.0f;
        protected float animationTime;
        protected float currentAnimationTime;
        protected float currentAnimationLength;
        protected bool animationInitialized = false;
        protected long timerFreq = Stopwatch.Frequency;
        
        Multiverse.Generator.FractalTerrainGenerator terrainGenerator;

        private List<AnimationState> animStateList;

        public Form1()
        {
            InitializeComponent();

            // Set up log configuration folders
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
                // Note that the DisplaySettings.xml should also show up in this folder.

            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            bool interactive = System.Windows.Forms.SystemInformation.UserInteractive;
            LogUtil.InitializeLogging(Path.Combine(ConfigFolder, "LogConfig.xml"), "DefaultLogConfig.xml", FallbackLogfile, interactive);

            whichObjectsComboBox.SelectedIndex = 0;

            runDemosRadioButton_Click(null, null);
            
            terrainGenerator = new Multiverse.Generator.FractalTerrainGenerator();

            axiomPictureBox.Height = this.ClientSize.Height - 60;

            MeterManager.MeterLogFile = MeterLogFile;
            MeterManager.MeterEventsFile = MeterEventsFile;
        }

        #region Axiom Initialization Code

        protected bool Setup()
        {
            // get a reference to the engine singleton
            engine = new Root("EngineConfig.xml", null);

            // add event handlers for frame events
            engine.FrameStarted += new FrameEvent(OnFrameStarted);
            engine.FrameEnded += new FrameEvent(OnFrameEnded);

            // allow for setting up resource gathering
            SetupResources();

            //show the config dialog and collect options
            if (!ConfigureAxiom())
            {
                // shutting right back down
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

        protected bool SetupResources()
		{
            RepositoryClass.Instance.InitializeRepositoryPath();
            if (!RepositoryClass.Instance.RepositoryDirectoryListSet())
            {
                return false;
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
            DisplayTerrain = false;
        }

        protected void CreateCamera()
        {
            camera = scene.CreateCamera("PlayerCam");

            float scale = 100f;
            camera.Position = new Vector3(64 * oneMeter, 0 * oneMeter, 64 * oneMeter);
            camera.LookAt(new Vector3(0, 0, 0));
            camera.Near = 1 * oneMeter;
            camera.Far = 10000 * oneMeter;
        }

        protected virtual void CreateViewports()
        {
            Debug.Assert(window != null, "Attempting to use a null RenderWindow.");

            // create a new viewport and set it's background color
            viewport = window.AddViewport(camera, 0, 0, 1.0f, 1.0f, 100);
            viewport.BackgroundColor = ColorEx.Black;
            viewport.OverlaysEnabled = false;
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

            ((Axiom.SceneManagers.Multiverse.SceneManager)scene).SetWorldParams(terrainGenerator, new DefaultLODSpec());
            scene.LoadWorldGeometry("");

            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = displayOcean;

            scene.AmbientLight = new ColorEx(0.8f, 0.8f, 0.8f);

            Light light = scene.CreateLight("MainLight");
            light.Type = LightType.Directional;
            Vector3 lightDir = new Vector3(-80 * oneMeter, -70 * oneMeter, -80 * oneMeter);
            lightDir.Normalize();
            light.Direction = lightDir;
            light.Position = -lightDir;
            light.Diffuse = ColorEx.White;
            light.SetAttenuation(1000 * oneMeter, 1, 0, 0);

            return;
        }

        protected void CreateRibbons() {
            viewport.BackgroundColor = ColorEx.Black;
            float scale = 100f;
            scene.AmbientLight = new ColorEx(0.5f, 0.5f, 0.5f);
            //scene.SetSkyBox(true, "Examples/SpaceSkyBox", 20f * oneMeter);
            Vector3 dir = new Vector3(-1f, -1f, 0.5f);
            dir.Normalize();
            Light light1 = scene.CreateLight("light1");
            light1.Type = LightType.Directional;
            light1.Direction = dir;
    
            // Create a barrel for the ribbons to fly through
            Entity barrel = scene.CreateEntity("barrel", "barrel.mesh");
            SceneNode barrelNode = scene.RootSceneNode.CreateChildSceneNode();
            barrelNode.ScaleFactor = 5f * Vector3.UnitScale;
            barrelNode.AttachObject(barrel);

            RibbonTrail trail = new RibbonTrail("DemoTrail", "numberOfChains", 2, "maxElementsPerChain", 80);
            trail.MaterialName = "Examples/LightRibbonTrail";
            trail.TrailLength = scale * 400f;
            scene.RootSceneNode.CreateChildSceneNode().AttachObject(trail);

            // Create 3 nodes for trail to follow
            SceneNode animNode = scene.RootSceneNode.CreateChildSceneNode();
            animNode.Position = scale * new Vector3(50f, 30f, 0);
            Animation anim = scene.CreateAnimation("an1", 14);
            anim.InterpolationMode = InterpolationMode.Spline;
            NodeAnimationTrack track = anim.CreateNodeTrack(1, animNode);
            TransformKeyFrame kf = track.CreateNodeKeyFrame(0);
            kf.Translate = scale * new Vector3(50f,30f,0f);
            kf = track.CreateNodeKeyFrame(2);
            kf.Translate = scale * new Vector3(100f, -30f, 0f);
            kf = track.CreateNodeKeyFrame(4);
            kf.Translate = scale * new Vector3(120f, -100f, 150f);
            kf = track.CreateNodeKeyFrame(6);
            kf.Translate = scale * new Vector3(30f, -100f, 50f);
            kf = track.CreateNodeKeyFrame(8);
            kf.Translate = scale * new Vector3(-50f, 30f, -50f);
            kf = track.CreateNodeKeyFrame(10);
            kf.Translate = scale * new Vector3(-150f, -20f, -100f);
            kf = track.CreateNodeKeyFrame(12);
            kf.Translate = scale * new Vector3(-50f, -30f, 0f);
            kf = track.CreateNodeKeyFrame(14);
            kf.Translate = scale * new Vector3(50f, 30f, 0f);

            AnimationState animState = scene.CreateAnimationState("an1");
            //animState.Enabled = true;
            animStateList = new List<AnimationState>();
            animStateList.Add(animState);

            trail.SetInitialColor(0, 1.0f, 0.8f, 0f, 1.0f);
            trail.SetColorChange(0, 0.5f, 0.5f, 0.5f, 0.5f);
            trail.SetInitialWidth(0, scale * 5f);
            trail.AddNode(animNode);

            // Add light
            Light light2 = scene.CreateLight("light2");
            light2.Diffuse = trail.GetInitialColor(0);
            animNode.AttachObject(light2);

            // Add billboard
            BillboardSet bbs = scene.CreateBillboardSet("bb", 1);
            bbs.CreateBillboard(Vector3.Zero, trail.GetInitialColor(0));
            bbs.MaterialName = "flare";
            animNode.AttachObject(bbs);

            animNode = scene.RootSceneNode.CreateChildSceneNode();
            animNode.Position = scale * new Vector3(-50f, 100f, 0f);
            anim = scene.CreateAnimation("an2", 10);
            anim.InterpolationMode = InterpolationMode.Spline;
            track = anim.CreateNodeTrack(1, animNode);
            kf = track.CreateNodeKeyFrame(0);
            kf.Translate = scale * new Vector3(-50f,100f,0f);
            kf = track.CreateNodeKeyFrame(2);
            kf.Translate = scale * new Vector3(-100f, 150f, -30f);
            kf = track.CreateNodeKeyFrame(4);
            kf.Translate = scale * new Vector3(-200f, 0f, 40f);
            kf = track.CreateNodeKeyFrame(6);
            kf.Translate = scale * new Vector3(0f, -150f, 70f);
            kf = track.CreateNodeKeyFrame(8);
            kf.Translate = scale * new Vector3(50f, 0f, 30f);
            kf = track.CreateNodeKeyFrame(10);
            kf.Translate = scale * new Vector3(-50f,100f,0f);

            animState = scene.CreateAnimationState("an2");
            //animState.setEnabled(true);
            animStateList.Add(animState);

            trail.SetInitialColor(1, 0.0f, 1.0f, 0.4f, 1.0f);
            trail.SetColorChange(1, 0.5f, 0.5f, 0.5f, 0.5f);
            trail.SetInitialWidth(1, scale * 5f);
            trail.AddNode(animNode);


            // Add light
            Light light3 = scene.CreateLight("l3");
            light3.Diffuse = trail.GetInitialColor(1);
            animNode.AttachObject(light3);

            // Add billboard
            bbs = scene.CreateBillboardSet("bb2", 1);
            bbs.CreateBillboard(Vector3.Zero, trail.GetInitialColor(1));
            bbs.MaterialName = "flare";
            animNode.AttachObject(bbs);
        }

        protected void CreateScene()
        {
            viewport.BackgroundColor = ColorEx.White;
            viewport.OverlaysEnabled = false;
            GenerateScene();

            scene.SetFog(FogMode.Linear, ColorEx.White, .008f, 0, 1000 * oneMeter);

            unitBox = MeshManager.Instance.Load("unit_box.mesh");
            unitSphere = MeshManager.Instance.Load("unit_sphere.mesh");
            unitCylinder = MeshManager.Instance.Load("unit_cylinder.mesh");
            zombieModel = MeshManager.Instance.Load("zombie.mesh");
            humanFemaleModel = MeshManager.Instance.Load("hmn_f_01_base.mesh");

            meshes = new Mesh[7] { unitBox, unitSphere, unitCylinder, null, null, zombieModel, humanFemaleModel };
            string[] zombieSubMeshes = new string[] { "Zombie_Body2-obj.0", "Zombie_Clothes2-obj.0" };
            string[] hmSubMeshes = new string[] { "feet_heels_tokyopop_a_01-mesh.0", "legs_ntrl_tokyopop_a_01-mesh.0", "torso_ntrl_tokyopop_a_01-mesh.0", "face_asia_01-mesh.0", "hair_bob_01-mesh.0" };
            visibleSubMeshes = new string[][] { zombieSubMeshes, hmSubMeshes };
            animationNames = new string[2] { "run", "ntrl_walk_loop"}; 
            return;
        }

        protected void ClearCreatedObjects() 
        {
            for (int i=0; i<sceneNodeList.Count; i++) {
                SceneNode node = sceneNodeList[i];
                Entity entity = entityList[i];
                node.DetachObject(entity);
                scene.RemoveEntity(entity);
                node.RemoveFromParent();
            }
            sceneNodeList.Clear();
            entityList.Clear();
            foreach (Material material in materialList) {
                material.Dispose();
                MaterialManager.Instance.Unload(material);
            }
            materialList.Clear();
            foreach (Texture texture in textureList) {
                texture.Dispose();
                TextureManager.Instance.Unload(texture);
            }
            textureList.Clear();
            totalVertexCount = 0;
        }
        
        protected string GetAnimationName() {
            return animationNames[(int)whichObjects - (int)WhichObjectsEnum.woZombie];
        }

        protected void EnsureObjectsCreated()
        {
            ClearCreatedObjects();
            Texture prototypeTexture = null;
            if (useTextures)
            {
                prototypeMaterial = MaterialManager.Instance.Load("barrel.barrel");
                if (uniqueTextures)
                    prototypeTexture = TextureManager.Instance.Load("blank.dds");
            }
            else
                prototypeMaterial = MaterialManager.Instance.Load("unit_box.unit_box");
            prototypeMaterial.Compile();
            if (objectCount == 0)
                return;
            int materialCount = (animatedObjects ? 0 :
                                 (numObjectsSharingMaterial == 0 ? objectCount :
                                  (numObjectsSharingMaterial >= objectCount ? 1 :
                                   (objectCount + numObjectsSharingMaterial - 1) / numObjectsSharingMaterial)));
            materialCountLabel.Text = "Material Count: " + materialCount;
            if (whichObjects == WhichObjectsEnum.woPlane || whichObjects == WhichObjectsEnum.woRandom) {
                Mesh plane = meshes[(int)WhichObjectsEnum.woPlane];
                if (plane != null)
                    plane.Unload();
                // Create the plane
                float planeSide = 1000f;
                int planeUnits = Int32.Parse(planeUnitsTextBox.Text);
                plane = MeshManager.Instance.CreatePlane("testerPlane", new Plane(Vector3.UnitZ, Vector3.Zero), planeSide, planeSide, planeUnits, planeUnits, true, 
                    1, planeSide / planeUnits, planeSide / planeUnits, Vector3.UnitY);
                meshes[(int)WhichObjectsEnum.woPlane] = plane;
            }
            // Create the new materials
            for (int i = 0; i < materialCount; i++)
            {
                Material mat = prototypeMaterial.Clone("mat" + i);
                Pass p = mat.GetTechnique(0).GetPass(0);
                if (!animatedObjects && uniqueTextures) {
                    Texture t = prototypeTexture;
                    Texture texture = TextureManager.Instance.CreateManual("texture" + i, t.TextureType, t.Width, t.Height, t.NumMipMaps, t.Format, t.Usage);
                    textureList.Add(texture);
                    p.CreateTextureUnitState(texture.Name);
                }
                // Make the materials lovely shades of blue
                p.Ambient = new ColorEx(1f, .2f, .2f, (1f / materialCount) * i);
                p.Diffuse = new ColorEx(p.Ambient);
                p.Specular = new ColorEx(1f, 0f, 0f, 0f);
                materialList.Add(mat);
            }
            
            // Create the entities and scene nodes
            for (int i=0; i<objectCount; i++) {
                Mesh mesh = selectMesh();
                Material mat = null;
                if (materialCount > 0)
                    mat = materialList[i % materialCount];
                Entity entity = scene.CreateEntity("entity" + i, mesh);
                if (animatedObjects) {
                    string[] visibleSubs = visibleSubMeshes[(int)whichObjects - (int) WhichObjectsEnum.woZombie];
                    for (int j = 0; j < entity.SubEntityCount; ++j) {
                        SubEntity sub = entity.GetSubEntity(j);
                        bool visible = false;
                        foreach (string s in visibleSubs) {
                            if (s == sub.SubMesh.Name) {
                                visible = true;
                                break;
                            }
                        }
                        sub.IsVisible = visible;
                        if (visible)
                            totalVertexCount += sub.SubMesh.VertexData.vertexCount;
                    }
                }
                else {
                    if (mesh.SharedVertexData != null)
                        totalVertexCount += mesh.SharedVertexData.vertexCount;
                    else {
                        for (int j=0; j<mesh.SubMeshCount; j++) {
                            SubMesh subMesh = mesh.GetSubMesh(j);
                            totalVertexCount += subMesh.VertexData.vertexCount;
                        }
                    }
                }
                if (animatedObjects && animateCheckBox.Checked) {
                    AnimationState currentAnimation = entity.GetAnimationState(GetAnimationName());
                    currentAnimation.IsEnabled = true;
                    if (!animationInitialized)
                    {  
                        currentAnimationLength = entity.GetAnimationState(GetAnimationName()).Length;
                        animationInitialized = true;
                    }
                }
                if (mat != null)
                    entity.MaterialName = mat.Name;
                entityList.Add(entity);
                SceneNode node = scene.RootSceneNode.CreateChildSceneNode();
                sceneNodeList.Add(node);
                node.AttachObject(entity);
                node.Position = new Vector3(randomCoord(), randomCoord(), randomCoord());
                if (randomSizes)
                    node.ScaleFactor = Vector3.UnitScale * randomScale();
                else if (randomScales)
                    node.ScaleFactor = new Vector3(randomScale(), randomScale(), randomScale());
                else
                    node.ScaleFactor = Vector3.UnitScale * 1f;
                if (randomOrientations)
                {
                    Vector3 axis = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                    node.Orientation = Vector3.UnitY.GetRotationTo(axis.ToNormalized());
                }
                else
                    node.Orientation = Quaternion.Identity;
            }
        }

        protected Mesh selectMesh()
        {
            if (whichObjects == WhichObjectsEnum.woRandom) {
                int r = (int)(rand.NextDouble() * 4.0);
                if (r >= 4)
                    r = 3;
                return meshes[r];
            }
            else
                return meshes[(int)whichObjects];
        }

        protected float randomCoord() 
        {
            return (float)((rand.NextDouble() - 0.5) * 50 * oneMeter);
        }
        
        protected float randomScale()
        {
            return (float)(.25 + (2.0 * (rand.NextDouble() - .15)));;
        }
        
        #endregion Scene Setup


        #region Axiom Frame Event Handlers

		public void meterOneFrame()
		{
			if (Root.Instance != null)
				Root.Instance.ToggleMetering(1);
		}

        protected void OnFrameEnded(object source, FrameEventArgs e)
        {
            return;
        }

        protected void runAnimations()
        {
            if (!animationInitialized)
                return;

            long curTimer = Stopwatch.GetTimestamp();

            float frameTime = (float)(curTimer - lastFrameTime) / (float)timerFreq;
            lastFrameTime = curTimer;

            float advanceTime = frameTime * animationSpeed;

            float time = currentAnimationTime + advanceTime;
            // Loop the animation
            while (time >= currentAnimationLength)
                time -= currentAnimationLength;
            foreach (Entity entity in entityList) {
                AnimationState currentAnimation = entity.GetAnimationState(GetAnimationName());
                currentAnimation.Time = time;
            }
            currentAnimationTime = time;
        }

        protected void OnFrameStarted(object source, FrameEventArgs e)
        {

            if (quit)
            {
                Root.Instance.QueueEndRendering();

                return;
            }

            // Calculate the number of render calls since the last frame
            lastFrameRenderCalls = (int)(RenderSystem.TotalRenderCalls - lastTotalRenderCalls);
            // Remember the current count for next frame
            lastTotalRenderCalls = RenderSystem.TotalRenderCalls;
            int fps = Root.Instance.CurrentFPS;
            FPSLabel.Text = "FPS: " + fps.ToString();

            rendersPerFrameLabel.Text = "Renders Per Frame: " + lastFrameRenderCalls;
            renderCallsLabel.Text = "Renders Per Second: " + fps * lastFrameRenderCalls;

            // Calculate the number of set pass calls since the last frame
            lastFrameSetPassCalls = (int)(scene.TotalSetPassCalls - lastTotalSetPassCalls);
            // Remember the current count for next frame
            lastTotalSetPassCalls = scene.TotalSetPassCalls;
            setPassCallsLabel.Text = "Set Pass Calls Per Frame: " + lastFrameSetPassCalls;

            // Display the number of vertices
            vertexCountLabel.Text = "Verts/Obj: " + (objectCount == 0 ? 0 : totalVertexCount / objectCount) + " Total Verts: " + totalVertexCount;
            
            float scaleMove = 100 * e.TimeSinceLastFrame * oneMeter;

            time += e.TimeSinceLastFrame;
            //Axiom.SceneManagers.Multiverse.WorldManager.Instance.Time = time;

            if (spinCamera)
            {
                float rot = 20 * e.TimeSinceLastFrame;

                camera.Yaw(rot);
            }

            if (animatedObjects && animateCheckBox.Checked)
            {
                runAnimations();
            }
            
            //// reset acceleration zero
            //camAccel = Vector3.Zero;

            //// set the scaling of camera motion
            //cameraScale = 100 * e.TimeSinceLastFrame;

            //if (moveForward)
            //{
            //    camAccel.z = -1.0f;
            //}
            //if (moveBack)
            //{
            //    camAccel.z = 1.0f;
            //}
            //if (moveLeft)
            //{
            //    camAccel.x = -0.5f;
            //}
            //if (moveRight)
            //{
            //    camAccel.x = 0.5f;
            //}

            //// handle rotation of the camera with the mouse
            //if (mouseRotate)
            //{
            //    float deltaX = mouseX - lastMouseX;
            //    float deltaY = mouseY - lastMouseY;

            //    camera.Yaw(-deltaX * mouseRotationScale);
            //    camera.Pitch(-deltaY * mouseRotationScale);
            //}

            //// update last mouse position
            //lastMouseX = mouseX;
            //lastMouseY = mouseY;

            //if (humanSpeed)
            //{
            //    camVelocity = camAccel * 7.0f * oneMeter;
            //    camera.MoveRelative(camVelocity * e.TimeSinceLastFrame);
            //}
            //else
            //{
            //    camVelocity += (camAccel * scaleMove * camSpeed);

            //    // move the camera based on the accumulated movement vector
            //    camera.MoveRelative(camVelocity * e.TimeSinceLastFrame);

            //    // Now dampen the Velocity - only if user is not accelerating
            //    if (camAccel == Vector3.Zero)
            //    {
            //        float decel = 1 - (6 * e.TimeSinceLastFrame);

            //        if (decel < 0)
            //        {
            //            decel = 0;
            //        }
            //        camVelocity *= decel;
            //    }
            //}

            //if (followTerrain || (result.worldFragment.SingleIntersection.y + (2.0f * oneMeter)) > camera.Position.y)
            //{
            //    // adjust new camera position to be a fixed distance above the ground

            //    camera.Position = new Vector3(camera.Position.x, result.worldFragment.SingleIntersection.y + (2.0f * oneMeter), camera.Position.z);
            //}
        }

        #endregion Axiom Frame Event Handlers

        private bool DisplayTerrain {
            get {
                return displayTerrain;
            }
            set {
                displayTerrain = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.DrawTerrain = displayTerrain;
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
            displayTerrainToolStripMenuItem.Checked = DisplayTerrain;
            spinCameraToolStripMenuItem.Checked = SpinCamera;
        }

        private void displayOceanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayOcean = !DisplayOcean;
        }

        private void displayTerrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayTerrain = !DisplayTerrain;
        }

        private void spinCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit = true;
            e.Cancel = true;
        }

        private void numObjectsTrackBar_ValueChanged(object sender, EventArgs e)
        {
            objectCount = numObjectsTrackBar.Value;
            numObjectsLabel.Text = "Number of Objects: " + objectCount;
        }

        private void numSharingMaterialTrackBar_ValueChanged(object sender, EventArgs e)
        {
            numObjectsSharingMaterial = numSharingMaterialTrackBar.Value;
            numSharingMaterialLabel.Text = "Number of Object Sharing Each Material: " + numObjectsSharingMaterial; 
        }

        private void generateObjectsButton_Click(object sender, EventArgs e)
        {
            EnsureObjectsCreated();
        }

        private void randomOrientationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            randomOrientations = randomOrientationsCheckBox.Checked;
        }

        private void randomSizesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            randomSizes = randomSizesRadioButton.Checked;
        }

        private void randomScalesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            randomScales = randomScalesRadioButton.Checked;
        }

        private void unitSizeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            randomSizes = false;
            randomScales = false;
        }

        private void whichObjectsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            whichObjects = (WhichObjectsEnum)whichObjectsComboBox.SelectedIndex;
            animatedObjects = whichObjects == WhichObjectsEnum.woZombie || whichObjects == WhichObjectsEnum.woHumanFemale;
            animateCheckBox.Visible = animatedObjects;
            animationInitialized = false;
            planeUnitsPanel.Visible = whichObjects == WhichObjectsEnum.woPlane || whichObjects == WhichObjectsEnum.woRandom;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.N && e.Alt && e.Shift && e.Control)
            {
                commentLabel.Text = "Metering one frame";
                meterOneFrame();
            }
        }

        private void useTextureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            useTextures = useTextureCheckBox.Checked;
            uniqueTexturesCheckBox.Enabled = useTextures;
            if (!useTextures)
                uniqueTexturesCheckBox.Checked = false;
        }

        private void uniqueTexturesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            uniqueTextures = uniqueTexturesCheckBox.Checked;
        }

        private void meterButton_Click(object sender, EventArgs e)
        {
			if (engine != null)
				engine.ToggleMetering(1);
        }

        private void panelSelectionChanged(bool runDemos)
        {
            inhibit++;
            renderObjectsRadioButton.Checked = !runDemos;
            runDemosRadioButton.Checked = runDemos;
            inhibit--;
            renderObjectsPanel.Visible = !runDemos;
            runDemosPanel.Visible = runDemos;
        }
        
        private void renderObjectsRadioButton_Click(object sender, EventArgs e)
        {
            if (inhibit == 0)
                panelSelectionChanged(!renderObjectsRadioButton.Checked);
        }

        private void runDemosRadioButton_Click(object sender, EventArgs e)
        {
            if (inhibit == 0)
                panelSelectionChanged(runDemosRadioButton.Checked);
        }

        private void OnRibbonFrameStarted(object source, FrameEventArgs e)
        {
            // For RibbbonTrail display
            foreach (AnimationState animi in animStateList)
                animi.AddTime(e.TimeSinceLastFrame);
        }

        private void runRibbonsDemoButton_Click(object sender, EventArgs e)
        {
            CreateRibbons();
            engine.FrameStarted += new FrameEvent(OnRibbonFrameStarted);
        }
    }
}
