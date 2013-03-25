#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.Input;
using Axiom.Overlays;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using MouseButtons = Axiom.Input.MouseButtons;

namespace Axiom.Utility {
	/// <summary>
	///     Base class for Axiom examples.
	/// </summary>
	public abstract class TechDemo : IDisposable {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TechDemo));

		#region Protected Fields

		protected Root engine;
		protected Camera camera;
		protected Viewport viewport;
		protected SceneManager scene; 
		protected RenderWindow window;
		protected InputReader input;
		protected Vector3 cameraVector = Vector3.Zero;
		protected float cameraScale;
		protected bool showDebugOverlay = true;
		protected float statDelay = 0.0f;
		protected float debugTextDelay = 0.0f;
		protected float toggleDelay = 0.0f;
		protected Vector3 camVelocity = Vector3.Zero;
		protected Vector3 camAccel = Vector3.Zero;
		protected float camSpeed = 2.5f;
		protected int aniso = 1;
		protected TextureFiltering filtering = TextureFiltering.Bilinear;

		#endregion Protected Fields

		#region Protected Methods

		protected bool Configure() {
			// HACK: Temporary
			RenderSystem renderSystem = Root.Instance.RenderSystems[0];
			Root.Instance.RenderSystem = renderSystem;
			DisplayMode mode = renderSystem.ConfigOptions.FullscreenModes[0];
			//mode.FullScreen = true;
            renderSystem.ConfigOptions.SelectMode(mode.Width, mode.Height, mode.Depth, mode.Fullscreen);

			window = Root.Instance.Initialize(true, "Axiom Engine Window");

			ShowDebugOverlay(showDebugOverlay);
			
			return true;
		}

		protected virtual void CreateCamera() {
			// create a camera and initialize its position
			camera = scene.CreateCamera("MainCamera");
			camera.Position = new Vector3(0, 0, 500);
			camera.LookAt(new Vector3(0, 0, -300));

			// set the near clipping plane to be very close
			camera.Near = 5;
		}

		/// <summary>
		///    Shows the debug overlay, which displays performance statistics.
		/// </summary>
		protected void ShowDebugOverlay(bool show) {
			// gets a reference to the default overlay
			Overlay o = OverlayManager.Instance.GetByName("Core/DebugOverlay");

			if(o == null) {
				throw new Exception(string.Format("Could not find overlay named '{0}'.", "Core/DebugOverlay"));
			}

			if(show) {
				o.Show();
			}
			else {
				o.Hide();
			}
		}

		protected void TakeScreenshot(string fileName) {
			window.Save(fileName);
		}

		#endregion Protected Methods

		#region Protected Virtual Methods

		protected virtual void ChooseSceneManager() {
			// Get the SceneManager, a generic one by default
			scene = engine.SceneManagers.GetSceneManager(SceneType.Generic);
		}

		protected virtual void CreateViewports() {
			Debug.Assert(window != null, "Attempting to use a null RenderWindow.");

			// create a new viewport and set it's background color
			viewport = window.AddViewport(camera, 0, 0, 1.0f, 1.0f, 100);
			viewport.BackgroundColor = ColorEx.Black;
		}

		protected virtual bool Setup() {
            // instantiate the Root singleton
            engine = new Root("EngineConfig.xml", "AxiomEngine.log");

            // add event handlers for frame events
            engine.FrameStarted += new FrameEvent(OnFrameStarted);
            engine.FrameEnded += new FrameEvent(OnFrameEnded);

            // allow for setting up resource gathering
			SetupResources();

			//show the config dialog and collect options
			if(!Configure()) {
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

			// retreive and initialize the input system
			input = PlatformManager.Instance.CreateInputReader();
            input.Initialize(window, true, true, false, false, false);

			return true;
		}

		/// <summary>
		///		Loads default resource configuration if one exists.
		/// </summary>
		protected virtual void SetupResources() {
		}

		#endregion Protected Virtual Methods

		#region Protected Abstract Methods

		/// <summary>
		/// 
		/// </summary>
		protected abstract void CreateScene();

		#endregion Protected Abstract Methods

		#region Public Methods

		public virtual void Start() {
            try {
                if (Setup()) {
                    // start the engines rendering loop
                    engine.StartRendering();
                }
            }
            catch (Exception ex) {
                // try logging the error here first, before Root is disposed of
                if (log != null)
                    log.Error(ex.Message);
				throw;
            }
		}

		public virtual void Dispose() {
			if(engine != null) {
				// remove event handlers
				engine.FrameStarted -= new FrameEvent(OnFrameStarted);
				engine.FrameEnded -= new FrameEvent(OnFrameEnded);

				engine.Dispose();
			}
		}

		#endregion Public Methods

		#region Event Handlers

		protected virtual void OnFrameEnded(Object source, FrameEventArgs e) {
		}

		private static TimingMeter onFrameStartedMeter = MeterManager.GetMeter("TechDemo OnFrameStarted", "TechDemo");
		
		protected virtual void OnFrameStarted(Object source, FrameEventArgs e) {
			onFrameStartedMeter.Enter();
			float scaleMove = 200 * e.TimeSinceLastFrame;

			// reset acceleration zero
			camAccel = Vector3.Zero;

			// set the scaling of camera motion
			cameraScale = 100 * e.TimeSinceLastFrame;

			// TODO: Move this into an event queueing mechanism that is processed every frame
			input.Capture();

			if(input.IsKeyPressed(KeyCodes.Escape)) {
                Root.Instance.QueueEndRendering();

				onFrameStartedMeter.Exit();
                return;
			}

			if(input.IsKeyPressed(KeyCodes.A)) {
				camAccel.x = -0.5f;
			}

			if(input.IsKeyPressed(KeyCodes.D)) {
				camAccel.x = 0.5f;
			}

			if(input.IsKeyPressed(KeyCodes.W)) {
				camAccel.z = -1.0f;
			}

			if(input.IsKeyPressed(KeyCodes.S)) {
				camAccel.z = 1.0f;
			}

			camAccel.y += (float)(input.RelativeMouseZ * 0.1f);

			if(input.IsKeyPressed(KeyCodes.Left)) {
				camera.Yaw(cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Right)) {
				camera.Yaw(-cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Up)) {
				camera.Pitch(cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Down)) {
				camera.Pitch(-cameraScale);
			}

			// subtract the time since last frame to delay specific key presses
			toggleDelay -= e.TimeSinceLastFrame;

			// toggle rendering mode
			if(input.IsKeyPressed(KeyCodes.R) && toggleDelay < 0) {
				if(camera.SceneDetail == SceneDetailLevel.Points) {
					camera.SceneDetail = SceneDetailLevel.Solid;
				}
				else if(camera.SceneDetail == SceneDetailLevel.Solid) {
					camera.SceneDetail = SceneDetailLevel.Wireframe;
				}
				else {
					camera.SceneDetail = SceneDetailLevel.Points;
				}

                log.InfoFormat("Rendering mode changed to '{0}'.", camera.SceneDetail);

				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.T) && toggleDelay < 0) {
				// toggle the texture settings
				switch(filtering) {
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
                
				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.P)) {
				string[] temp = Directory.GetFiles(Environment.CurrentDirectory, "screenshot*.jpg");
				string fileName = string.Format("screenshot{0}.jpg", temp.Length + 1);
                
				TakeScreenshot(fileName);

				// show briefly on the screen
				window.DebugText = string.Format("Wrote screenshot '{0}'.", fileName);

				// show for 2 seconds
				debugTextDelay = 2.0f;
			}

			if(input.IsKeyPressed(KeyCodes.B)) {
				scene.ShowBoundingBoxes = !scene.ShowBoundingBoxes;
			}

			if(input.IsKeyPressed(KeyCodes.F)) {
				// hide all overlays, includes ones besides the debug overlay
				viewport.OverlaysEnabled = !viewport.OverlaysEnabled;
			}

			if(!input.IsMousePressed(MouseButtons.Left)) {
				float cameraYaw = -input.RelativeMouseX * .13f;
				float cameraPitch = -input.RelativeMouseY * .13f;
                
				camera.Yaw(cameraYaw);
				camera.Pitch(cameraPitch);
			} 
			else {
				cameraVector.x += input.RelativeMouseX * 0.13f;
			}

			camVelocity += (camAccel * scaleMove * camSpeed);

			// move the camera based on the accumulated movement vector
			camera.MoveRelative(camVelocity * e.TimeSinceLastFrame);

			// Now dampen the Velocity - only if user is not accelerating
			if (camAccel == Vector3.Zero) { 
				camVelocity *= (1 - (6 * e.TimeSinceLastFrame)); 
			}

			// update performance stats once per second
			if(statDelay < 0.0f && showDebugOverlay) {
				UpdateStats();
				statDelay = 1.0f;
			}
			else {
				statDelay -= e.TimeSinceLastFrame;
			}

			// turn off debug text when delay ends
			if(debugTextDelay < 0.0f) {
				debugTextDelay = 0.0f;
				window.DebugText = "";
			}
			else if(debugTextDelay > 0.0f) {
				debugTextDelay -= e.TimeSinceLastFrame;
			}

            OverlayElement element = OverlayElementManager.Instance.GetElement("Core/DebugText");
            element.Text = window.DebugText;

			onFrameStartedMeter.Exit();
        }

		protected void UpdateStats() {
			// TODO: Replace with CEGUI
			OverlayElement element = OverlayElementManager.Instance.GetElement("Core/CurrFps");
			element.Text = string.Format("Current FPS: {0}", Root.Instance.CurrentFPS);

			element = OverlayElementManager.Instance.GetElement("Core/BestFps");
			element.Text = string.Format("Best FPS: {0}", Root.Instance.BestFPS);

			element = OverlayElementManager.Instance.GetElement("Core/WorstFps");
			element.Text = string.Format("Worst FPS: {0}", Root.Instance.WorstFPS);

			element = OverlayElementManager.Instance.GetElement("Core/AverageFps");
			element.Text = string.Format("Average FPS: {0}", Root.Instance.AverageFPS);

			element = OverlayElementManager.Instance.GetElement("Core/NumTris");
			element.Text = string.Format("Triangle Count: {0}", scene.TargetRenderSystem.FacesRendered);
		}

		#endregion Event Handlers
	}
}
