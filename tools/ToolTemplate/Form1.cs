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

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Configuration;

namespace Multiverse.Tools.ToolTemplate
{
    public partial class Form1 : Form
    {
        protected readonly float oneMeter = 1000.0f;

        protected Root engine;
        protected Camera camera;
        protected Viewport viewport;
        protected SceneManager scene;
        protected RenderWindow window;

        protected bool quit = false;

        protected float time = 0;
        protected bool displayWireFrame = false;
        protected bool displayTerrainTiles = true;
        protected bool displayTerrainStitches = true;
        protected bool displayOcean = true;
        protected bool spinCamera = false;

        Multiverse.Generator.Generator terrainGenerator;

        public Form1()
        {
            InitializeComponent();

            terrainGenerator = new Multiverse.Generator.Generator();

        }

        #region Axiom Initialization Code

        protected bool Setup()
        {
            // get a reference to the engine singleton
            engine = new Root("EngineConfig.xml", "AxiomEngine.log");

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

        protected void SetupResources()
        {
            EngineConfig config = new EngineConfig();

            // load the config file
            // relative from the location of debug and releases executables
            config.ReadXml("EngineConfig.xml");

            // interrogate the available resource paths
            foreach (EngineConfig.FilePathRow row in config.FilePath)
            {
                string fullPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + row.src;

                ResourceManager.AddCommonArchive(fullPath, row.type);
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

        protected void CreateCamera()
        {
            camera = scene.CreateCamera("PlayerCam");

            camera.Position = new Vector3(128 * oneMeter, 200 * oneMeter, 128 * oneMeter);
            camera.LookAt(new Vector3(0, 0, -300 * oneMeter));
            camera.Near = 1 * oneMeter;
            camera.Far = 10000 * oneMeter;
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

            ((Axiom.SceneManagers.Multiverse.SceneManager)scene).SetWorldParams(terrainGenerator, null, 256, 4);
            scene.LoadWorldGeometry("");

            Axiom.SceneManagers.Multiverse.WorldManager.Instance.ShowOcean = displayOcean;
            Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawTiles = displayTerrainTiles;
            Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawStitches = displayTerrainStitches;

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

        private void Regenerate()
        {
            GenerateScene();
        }

        protected void CreateScene()
        {

            viewport.BackgroundColor = ColorEx.White;
            viewport.OverlaysEnabled = false;

            GenerateScene();

            scene.SetFog(FogMode.Linear, ColorEx.White, .008f, 0, 1000 * oneMeter);

            return;
        }

        #endregion Scene Setup


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

            float scaleMove = 100 * e.TimeSinceLastFrame * oneMeter;

            time += e.TimeSinceLastFrame;
            Axiom.SceneManagers.Multiverse.WorldManager.Instance.Time = time;

            if (spinCamera)
            {
                float rot = 20 * e.TimeSinceLastFrame;

                camera.Yaw(rot);
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
                Axiom.SceneManagers.Multiverse.WorldManager.Instance.ShowOcean = displayOcean;
            }
        }

        private bool DisplayTerrainTiles
        {
            get
            {
                return displayTerrainTiles;
            }
            set
            {
                displayTerrainTiles = value;
                Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawTiles = displayTerrainTiles;
            }
        }

        private bool DisplayTerrainStitches
        {
            get
            {
                return displayTerrainStitches;
            }
            set
            {
                displayTerrainStitches = value;
                Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawStitches = displayTerrainStitches;
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
            displayTerrainStitchesToolStripMenuItem.Checked = DisplayTerrainStitches;
            displayTerrainTilesToolStripMenuItem.Checked = DisplayTerrainTiles;
            spinCameraToolStripMenuItem.Checked = SpinCamera;
        }

        private void displayOceanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayOcean = !DisplayOcean;
        }

        private void displayTerrainTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayTerrainTiles = !DisplayTerrainTiles;
        }

        private void displayTerrainStitchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayTerrainStitches = !DisplayTerrainStitches;
        }

        private void spinCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SpinCamera = !SpinCamera;
        }
    }
}
