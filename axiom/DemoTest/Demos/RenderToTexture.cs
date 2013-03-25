using System;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for RenderToTexture.
	/// </summary>
	public class RenderToTexture : DemoBase {
        
        Camera reflectCam;
        SceneNode planeNode;
		MovablePlane plane;
		Entity planeEntity;

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            base.OnFrameStarted (source, e);

			// make sure reflection camera is updated too
            reflectCam.Orientation = camera.Orientation;
			reflectCam.Position = camera.Position;

			// rotate plane
			planeNode.Yaw(30 * e.TimeSinceLastFrame, TransformSpace.Parent);
        }

        protected override void CreateScene() {
			// set ambient light
			scene.AmbientLight = new ColorEx(0.2f, 0.2f, 0.2f);

			scene.SetSkyBox(true, "Skybox/Morning", 5000);

            // create a default point light
            Light light = scene.CreateLight("MainLight");
			light.Type = LightType.Directional;
			Vector3 dir = new Vector3(0.5f, -1, 0);
			dir.Normalize();
            light.Direction = dir;
			light.Diffuse = new ColorEx(1.0f, 1.0f, 0.8f);
			light.Specular = ColorEx.White;

			// create a plane
			plane = new MovablePlane("ReflectPlane");
			plane.D = 0;
			plane.Normal = Vector3.UnitY;

			// create another plane to create the mesh.  Ogre's MovablePlane uses multiple inheritance, bah!
			Plane tmpPlane = new Plane();
			tmpPlane.D = 0;
			tmpPlane.Normal = Vector3.UnitY;

			MeshManager.Instance.CreatePlane("ReflectionPlane", tmpPlane, 2000, 2000, 1, 1, true, 1, 1, 1, Vector3.UnitZ);
            planeEntity = scene.CreateEntity("Plane", "ReflectionPlane");

			// create an entity from a model
            Entity knot = scene.CreateEntity("Knot", "knot.mesh");
			knot.MaterialName = "TextureFX/Knot";

			// create an entity from a model
            Entity head = scene.CreateEntity("Head", "ogrehead.mesh");

			// attach the render to texture entity to the root of the scene
            SceneNode rootNode = scene.RootSceneNode;
			planeNode = rootNode.CreateChildSceneNode();

			planeNode.AttachObject(planeEntity);
			planeNode.AttachObject(plane);
			planeNode.Translate(new Vector3(0, -10, 0));

			// tilt it a little to make it interesting
			planeNode.Roll(5);

			rootNode.CreateChildSceneNode("Head").AttachObject(head);

			// create a render texture
			RenderTexture rttTex = Root.Instance.RenderSystem.CreateRenderTexture("RttTex", 512, 512);
			reflectCam = scene.CreateCamera("ReflectCam");
			reflectCam.Near = camera.Near;
			reflectCam.Far = camera.Far;
			reflectCam.AspectRatio = (float)window.GetViewport(0).ActualWidth / (float)window.GetViewport(0).ActualHeight;

			Viewport viewport = rttTex.AddViewport(reflectCam);
			viewport.ClearEveryFrame = true;
			viewport.OverlaysEnabled = false;
			viewport.BackgroundColor = ColorEx.Black;
         
            Material mat = scene.CreateMaterial("RttMat");
			TextureUnitState t = mat.GetTechnique(0).GetPass(0).CreateTextureUnitState("RustedMetal.jpg");
            t = mat.GetTechnique(0).GetPass(0).CreateTextureUnitState("RttTex");

			// blend with base texture
			t.SetColorOperationEx(LayerBlendOperationEx.BlendManual, LayerBlendSource.Texture, LayerBlendSource.Current, 
				ColorEx.White, ColorEx.White, 0.25f);

			t.SetProjectiveTexturing(true, reflectCam);

			// register events for viewport before/after update
			rttTex.AfterUpdate += new RenderTargetUpdateEventHandler(rttTex_AfterUpdate);
			rttTex.BeforeUpdate += new RenderTargetUpdateEventHandler(rttTex_BeforeUpdate);

			// set up linked reflection
			reflectCam.EnableReflection(plane);

			// also clip
			reflectCam.EnableCustomNearClipPlane(plane);

            planeEntity.MaterialName = "RttMat";

            Entity clone = null;

            for(int i = 0; i < 10; i++) {
                // create a new node under the root
                SceneNode node = scene.CreateSceneNode();

                // calculate a random position
                Vector3 nodePosition = new Vector3();
                nodePosition.x = MathUtil.SymmetricRandom() * 750.0f;
                nodePosition.y = MathUtil.SymmetricRandom() * 100.0f + 25;
                nodePosition.z = MathUtil.SymmetricRandom() * 750.0f;

                // set the new position
                node.Position = nodePosition;

                // attach this node to the root node
                rootNode.AddChild(node);

                // clone the knot
                string cloneName = string.Format("Knot{0}", i);
                clone = knot.Clone(cloneName);

                // add the cloned knot to the scene
                node.AttachObject(clone);
            }

			camera.Position = new Vector3(-50, 100, 500);
			camera.LookAt(new Vector3(0, 0, 0));
		}

		/// <summary>
		///		Hides the render target plane prior to the update.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void rttTex_BeforeUpdate(object sender, RenderTargetUpdateEventArgs e) {
			planeEntity.IsVisible = false;
		}

		/// <summary>
		///		Shows the render target plane again after the update.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void rttTex_AfterUpdate(object sender, RenderTargetUpdateEventArgs e) {
			planeEntity.IsVisible = true;
		}
	}
}
