using System;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for Terrain.
	/// </summary>
	public class Terrain : DemoBase {

        SceneNode waterNode;
        float flowAmount;
        bool flowUp = true;
        const float FLOW_HEIGHT = 0.8f;
        const float FLOW_SPEED = 0.2f;

        protected override void ChooseSceneManager() {
            scene = SceneManagerEnumerator.Instance.GetSceneManager(SceneType.ExteriorClose);
        }

        protected override void CreateCamera() {
            camera = scene.CreateCamera("PlayerCam");

            camera.Position = new Vector3(128, 25, 128);
            camera.LookAt(new Vector3(0, 0, -300));
            camera.Near = 1;
            camera.Far = 384;
        }

        protected override void CreateScene() {
            viewport.BackgroundColor = ColorEx.White;

            scene.AmbientLight = new ColorEx(0.5f, 0.5f, 0.5f);

            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);
            light.Diffuse = ColorEx.Blue;

            scene.LoadWorldGeometry("Terrain.xml");
            
            scene.SetFog(FogMode.Exp2, ColorEx.White, .008f, 0, 250);

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
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            float moveScale;
            float waterFlow;

            moveScale = 10 * e.TimeSinceLastFrame;
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

            base.OnFrameStarted (source, e);
        }

	}
}
