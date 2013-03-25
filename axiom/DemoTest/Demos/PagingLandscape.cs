using System;
using System.IO;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Axiom.Input;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for Terrain.
	/// </summary>
	public class PagingLandscape : DemoBase, IRaySceneQueryListener {

        SceneNode waterNode;
        float flowAmount;
        bool flowUp = true;
        const float FLOW_HEIGHT = 0.8f;
        const float FLOW_SPEED = 0.2f;
		RaySceneQuery raySceneQuery = null;

		// move the camera like a human at 3m/sec
		bool humanSpeed = false;

		// keep camera 2m above the ground
		bool followTerrain = false;

        protected override void ChooseSceneManager() {
            scene = SceneManagerEnumerator.Instance.GetSceneManager(SceneType.ExteriorFar);
        }

        protected override void CreateCamera() {
            camera = scene.CreateCamera("PlayerCam");

//            camera.Position = new Vector3(128, 25, 128);
//            camera.LookAt(new Vector3(0, 0, -300));
//            camera.Near = 1;
//            camera.Far = 384;

			camera.Position = new Vector3(128, 400, 128);
			camera.LookAt(new Vector3(0, 0, -300));
			camera.Near = 1;
			camera.Far = 100000;
        }

        protected override void CreateScene() {
            viewport.BackgroundColor = ColorEx.White;

            scene.AmbientLight = new ColorEx(0.5f, 0.5f, 0.5f);

            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);
            light.Diffuse = ColorEx.Blue;

            scene.LoadWorldGeometry("landscape.xml");
            
            // scene.SetFog(FogMode.Exp2, ColorEx.White, .008f, 0, 250);

            // water plane setup
            Plane waterPlane = new Plane(Vector3.UnitY, 1.5f);

            MeshManager.Instance.CreatePlane(
                "WaterPlane",
                waterPlane,
                2800, 2800,
                20, 20,
                true, 1,
                10, 10,
                Vector3.UnitZ);

            Entity waterEntity  = scene.CreateEntity("Water", "WaterPlane");
            waterEntity.MaterialName = "Terrain/WaterPlane";

            waterNode = scene.RootSceneNode.CreateChildSceneNode("WaterNode");
            waterNode.AttachObject(waterEntity);
            waterNode.Translate(new Vector3(1000, 0, 1000));

			raySceneQuery = scene.CreateRayQuery( new Ray(camera.Position, Vector3.NegativeUnitY));
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
        	float waterFlow;

            waterFlow = FLOW_SPEED * e.TimeSinceLastFrame;

            if(waterNode != null) {
                if(flowUp) {
                    flowAmount += waterFlow;
                }
                else {
                    flowAmount -= waterFlow;
                }

                if(flowAmount >= FLOW_HEIGHT) {
                    flowUp = false;
                }
                else if(flowAmount <= 0.0f) {
                    flowUp = true;
                }

                waterNode.Translate(new Vector3(0, flowUp ? waterFlow : -waterFlow, 0));
            }

			float scaleMove = 200 * e.TimeSinceLastFrame;

			// reset acceleration zero
			camAccel = Vector3.Zero;

			// set the scaling of camera motion
			cameraScale = 100 * e.TimeSinceLastFrame;

			// TODO: Move this into an event queueing mechanism that is processed every frame
			input.Capture();

			if(input.IsKeyPressed(KeyCodes.Escape)) 
			{
				Root.Instance.QueueEndRendering();
				return;
			}

			if(input.IsKeyPressed(KeyCodes.H) && toggleDelay < 0)
			{
				humanSpeed = !humanSpeed;
				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.G) && toggleDelay < 0)
			{
				followTerrain = !followTerrain;
				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.A)) 
			{
				camAccel.x = -0.5f;
			}

			if(input.IsKeyPressed(KeyCodes.D)) 
			{
				camAccel.x = 0.5f;
			}

			if(input.IsKeyPressed(KeyCodes.W)) 
			{
				camAccel.z = -1.0f;
			}

			if(input.IsKeyPressed(KeyCodes.S)) 
			{
				camAccel.z = 1.0f;
			}

			camAccel.y += input.RelativeMouseZ * 0.1f;

			if(input.IsKeyPressed(KeyCodes.Left)) 
			{
				camera.Yaw(cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Right)) 
			{
				camera.Yaw(-cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Up)) 
			{
				camera.Pitch(cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Down)) 
			{
				camera.Pitch(-cameraScale);
			}

			// subtract the time since last frame to delay specific key presses
			toggleDelay -= e.TimeSinceLastFrame;

			// toggle rendering mode
			if(input.IsKeyPressed(KeyCodes.R) && toggleDelay < 0) 
			{
				if(camera.SceneDetail == SceneDetailLevel.Points) 
				{
					camera.SceneDetail = SceneDetailLevel.Solid;
				}
				else if(camera.SceneDetail == SceneDetailLevel.Solid) 
				{
					camera.SceneDetail = SceneDetailLevel.Wireframe;
				}
				else 
				{
					camera.SceneDetail = SceneDetailLevel.Points;
				}

				Console.WriteLine("Rendering mode changed to '{0}'.", camera.SceneDetail);

				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.T) && toggleDelay < 0) 
			{
				// toggle the texture settings
				switch(filtering) 
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

				Console.WriteLine("Texture Filtering changed to '{0}'.", filtering);

				// set the new default
				MaterialManager.Instance.SetDefaultTextureFiltering(filtering);
				MaterialManager.Instance.DefaultAnisotropy = aniso;
                
				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.P)) 
			{
				string[] temp = Directory.GetFiles(Environment.CurrentDirectory, "screenshot*.jpg");
				string fileName = string.Format("screenshot{0}.jpg", temp.Length + 1);
                
				// show briefly on the screen
				window.DebugText = string.Format("Wrote screenshot '{0}'.", fileName);

				TakeScreenshot(fileName);

				// show for 2 seconds
				debugTextDelay = 2.0f;
			}

			if(input.IsKeyPressed(KeyCodes.B)) 
			{
				scene.ShowBoundingBoxes = !scene.ShowBoundingBoxes;
			}

			if(input.IsKeyPressed(KeyCodes.F)) 
			{
				// hide all overlays, includes ones besides the debug overlay
				viewport.OverlaysEnabled = !viewport.OverlaysEnabled;
			}

			if(!input.IsMousePressed(MouseButtons.Left)) 
			{
				float cameraYaw = -input.RelativeMouseX * .13f;
				float cameraPitch = -input.RelativeMouseY * .13f;
                
				camera.Yaw(cameraYaw);
				camera.Pitch(cameraPitch);
			} 
			else 
			{
				cameraVector.x += input.RelativeMouseX * 0.13f;
			}

			if ( humanSpeed ) 
			{
				camVelocity = camAccel * 7.0f;
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
					camVelocity *= (1 - (6 * e.TimeSinceLastFrame)); 
				}
			}

			// update performance stats once per second
			if(statDelay < 0.0f && showDebugOverlay) 
			{
				UpdateStats();
				statDelay = 1.0f;
			}
			else 
			{
				statDelay -= e.TimeSinceLastFrame;
			}

			// turn off debug text when delay ends
			if(debugTextDelay < 0.0f) 
			{
				debugTextDelay = 0.0f;
				window.DebugText = "";
			}
			else if(debugTextDelay > 0.0f) 
			{
				debugTextDelay -= e.TimeSinceLastFrame;
			}

			if ( followTerrain ) 
			{
				// adjust new camera position to be a fixed distance above the ground
				raySceneQuery.Ray = new Ray(camera.Position, Vector3.NegativeUnitY);
				raySceneQuery.Execute(this);
			}
        }

		public bool OnQueryResult(SceneQuery.WorldFragment fragment, float distance)
		{
			camera.Position = new Vector3(camera.Position.x, fragment.SingleIntersection.y + 2.0f, camera.Position.z);
			return false;
		}
		
		public bool OnQueryResult(MovableObject sceneObject, float distance) 
		{
			return true;
		}
	}
}
