using System;
using Axiom.Animating;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Axiom.Input;
using Axiom.Collections;
using System.Collections;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for BspDemo.
	/// </summary>
	public class Bsp : DemoBase {
		protected override void ChooseSceneManager() {
			scene = SceneManagerEnumerator.Instance.GetSceneManager(SceneType.Interior);
		}

		protected override void CreateScene() {
			// Load world geometry
			scene.LoadWorldGeometry("maps/chiropteradm.bsp");

			// modify camera for close work
			camera.Near = 4;
			camera.Far = 4000;

			// Also change position, and set Quake-type orientation
			// Get random player start point
			ViewPoint vp = scene.GetSuggestedViewpoint(true);
			camera.Position = vp.position;
			camera.Pitch(90); // Quake uses X/Y horizon, Z up
			camera.Rotate(vp.orientation);
			// Don't yaw along variable axis, causes leaning
			camera.FixedYawAxis = Vector3.UnitZ;
		}
	}
}
