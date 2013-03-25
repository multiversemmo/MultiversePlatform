using System;
using Axiom.Core;
using Axiom.ParticleSystems;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for Smoke.
	/// </summary>
	public class Smoke : DemoBase {
		protected override void CreateScene() {
			scene.AmbientLight = new ColorEx(0.5f, 0.5f, 0.5f);

			scene.SetSkyDome(true, "Examples/CloudySky", 5, 8);

			ParticleSystem smokeSystem = 
				ParticleSystemManager.Instance.CreateSystem("SmokeSystem", "Examples/Smoke");

			scene.RootSceneNode.CreateChildSceneNode().AttachObject(smokeSystem);
		}
	}
}
