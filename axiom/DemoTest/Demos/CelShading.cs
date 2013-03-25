using System;
using System.Collections;
using Axiom.Animating;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for CelShading.
	/// </summary>
	public class CelShading : DemoBase {
		#region Constants

		const int CustomShininess = 1;
		const int CustomDiffuse = 2;
		const int CustomSpecular = 3;

		#endregion Constants

        #region Fields

        SceneNode rotNode;

        #endregion Fields

        protected override void CreateScene() {
            if( !Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.VertexPrograms) ||
                !Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.FragmentPrograms)) {

                throw new Exception("Your hardware does not support vertex and fragment programs, so you cannot run this demo.");
            }

            // create a simple default point light
            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);

            rotNode = scene.RootSceneNode.CreateChildSceneNode();
            rotNode.CreateChildSceneNode(new Vector3(20, 40, 50), Quaternion.Identity).AttachObject(light);

            Entity entity = scene.CreateEntity("Head", "ogrehead.mesh");

            camera.Position = new Vector3(20, 0, 100);
            camera.LookAt(Vector3.Zero);
			
			// eyes
			SubEntity subEnt = entity.GetSubEntity(0);
			subEnt.MaterialName = "Examples/CelShading";
			subEnt.SetCustomParameter(CustomShininess, new Vector4(35.0f, 0.0f, 0.0f, 0.0f));
			subEnt.SetCustomParameter(CustomDiffuse, new Vector4(1.0f, 0.3f, 0.3f, 1.0f));
			subEnt.SetCustomParameter(CustomSpecular, new Vector4(1.0f, 0.6f, 0.6f, 1.0f));

			// skin
			subEnt = entity.GetSubEntity(1);
			subEnt.MaterialName = "Examples/CelShading";
			subEnt.SetCustomParameter(CustomShininess, new Vector4(10.0f, 0.0f, 0.0f, 0.0f));
			subEnt.SetCustomParameter(CustomDiffuse, new Vector4(0.0f, 0.5f, 0.0f, 1.0f));
			subEnt.SetCustomParameter(CustomSpecular, new Vector4(0.3f, 0.5f, 0.3f, 1.0f));

			// earring
			subEnt = entity.GetSubEntity(2);
			subEnt.MaterialName = "Examples/CelShading";
			subEnt.SetCustomParameter(CustomShininess, new Vector4(25.0f, 0.0f, 0.0f, 0.0f));
			subEnt.SetCustomParameter(CustomDiffuse, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
			subEnt.SetCustomParameter(CustomSpecular, new Vector4(1.0f, 1.0f, 0.7f, 1.0f));

			// teeth
			subEnt = entity.GetSubEntity(3);
			subEnt.MaterialName = "Examples/CelShading";
			subEnt.SetCustomParameter(CustomShininess, new Vector4(20.0f, 0.0f, 0.0f, 0.0f));
			subEnt.SetCustomParameter(CustomDiffuse, new Vector4(1.0f, 1.0f, 0.7f, 1.0f));
			subEnt.SetCustomParameter(CustomSpecular, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

			// add entity to the root scene node
            scene.RootSceneNode.CreateChildSceneNode().AttachObject(entity);

            window.GetViewport(0).BackgroundColor = ColorEx.White;
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            rotNode.Yaw(e.TimeSinceLastFrame * 30);

            base.OnFrameStarted (source, e);
        }
	}
}
