using System;
using System.Collections;
using System.Drawing;
using Axiom.Animating;
using Axiom.Controllers;
using Axiom.Controllers.Canned;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Input;
using Axiom.MathLib;
using Axiom.Media;
using Axiom.ParticleSystems;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for Shadows.
	/// </summary>
	public class Shadows : DemoBase {
		Entity athene;
		AnimationState animState;
		Light light;
		Light sunLight;
		SceneNode lightNode;
		Entity lightSphere;
		SceneNode lightSphereNode;
        AnimationState animSphereState;
		ColorEx minLightColor = new ColorEx(0.3f, 0f, 0f);
		ColorEx maxLightColor = new ColorEx( 0.5f, 0.3f, 0.1f );
		float minFlareSize = 40;
		float maxFlareSize = 80;

		string[] atheneMaterials = new string[] { 
			"Examples/Athene/NormalMapped",
			"Examples/Athene/Basic" 
		};

		string[] shadowTechniqueDescriptions = new string[] { 
			"Stencil Shadows (Additive)",
			"Stencil Shadows (Modulative)",
			"Texture Shadows (Additive)",
			"Texture Shadows (Modulative)",
			"Texture Shadows (Modulative, Soft Shadows)",
			"None"
		};

		string[] shadowTechniqueShortDescriptions = new string[] { 
			"Sten/Add",
			"Sten/Mod",
			"Tex/Add",
			"Tex/Mod",
			"Tex/ModSoft",
			"None"
		};

		string[] lightTypeDescriptions = new string[] {
			"Point",
			"Dir",
			"Spot"
		};
		
        LightType[] lightTypes = new LightType[] {
			LightType.Point,
			LightType.Directional,
			LightType.Spotlight,
		};
		
		bool[] shadowTechSoft = new bool[] {
			false, 
			false, 
			false,
			false, 
			true, 
			false

		};

		bool showSunlight = true;
		
		bool softShadowsSupported = false;

		ShadowTechnique[] shadowTechniques = new ShadowTechnique[] { 
			ShadowTechnique.StencilAdditive,
			ShadowTechnique.StencilModulative,
			ShadowTechnique.TextureAdditive,
			ShadowTechnique.TextureModulative,
			ShadowTechnique.TextureModulative,
			ShadowTechnique.None
		};

		int currentShadowTechnique = 2;

		bool lightMoving = false;

		int currentLightType = 2;

        protected void AddKey(AnimationTrack track, int time, Vector3 translate) {
            TransformKeyFrame key = (TransformKeyFrame)track.CreateKeyFrame(time);
			key.Translate = translate;
        }

		protected override void CreateScene() {
			// set ambient light off
//			scene.AmbientLight = new ColorEx(.5f, .5f, .5f);
            scene.AmbientLight = ColorEx.Black;

			// TODO: Check based on caps
			int currentAtheneMaterial = 0;

			// fixed light, dim
			if (showSunlight) {
				sunLight = scene.CreateLight("SunLight");
				sunLight.Type = LightType.Directional;
				sunLight.Position = new Vector3(1000, 1250, 500);
				sunLight.SetSpotlightRange(30, 50);
				Vector3 dir = -sunLight.Position;
				dir.Normalize();
				sunLight.Direction = dir;
				sunLight.Diffuse = new ColorEx(0.65f, 0.65f, 0.68f);
				sunLight.Specular = new ColorEx(0.9f, 0.9f, 1);
			}
			
			// point light, movable, reddish
			light = scene.CreateLight("Light2");
			light.Diffuse = minLightColor;
			light.Specular = ColorEx.White;
			light.SetAttenuation(8000, 1, .0005f, 0);

			// create light node
			lightNode = scene.RootSceneNode.CreateChildSceneNode("MovingLightNode");
			lightNode.AttachObject(light);

			// create billboard set
			BillboardSet bbs = scene.CreateBillboardSet("LightBBS", 1);
			bbs.MaterialName = "Examples/Flare";
			Billboard bb = bbs.CreateBillboard(Vector3.Zero, minLightColor);
			// attach to the scene
			lightNode.AttachObject(bbs);

			// create controller, after this is will get updated on its own
			WaveformControllerFunction func = 
				new WaveformControllerFunction(WaveformType.Sine, 0.75f, 0.5f);

			LightWibbler val = new LightWibbler(light, bb, minLightColor, maxLightColor, minFlareSize, maxFlareSize);
			ControllerManager.Instance.CreateController(val, func);

			lightNode.Position = new Vector3(300, 250, -300);

			// create a track for the light
			Animation anim = scene.CreateAnimation("LightTrack", 20);
			// spline it for nice curves
			anim.InterpolationMode = InterpolationMode.Spline;
			// create a track to animate the camera's node
			AnimationTrack track = anim.CreateNodeTrack(0, lightNode);
			// setup keyframes
			AddKey(track, 0, new Vector3(300, 250, -300));
			AddKey(track, 2, new Vector3(150,300,-250));
			AddKey(track, 4, new Vector3(-150,350,-100));
			AddKey(track, 6, new Vector3(-400,200,-200));
            AddKey(track, 8, new Vector3(-200,200,-400));
            AddKey(track, 10, new Vector3(-100,150,-200));
            AddKey(track, 12, new Vector3(-100,75,180));
			AddKey(track, 14, new Vector3(0,250,300));
			AddKey(track, 16, new Vector3(100,350,100));
			AddKey(track, 18, new Vector3(250,300,0));
            AddKey(track, 20, new Vector3(300,250,-300));

			// create a new animation state to track this
			animState = scene.CreateAnimationState("LightTrack");
			animState.IsEnabled = true;

			// Make light node look at origin, this is for when we
			// change the moving light to a spotlight
			lightNode.SetAutoTracking(true, scene.RootSceneNode);

//             lightSphereNode = scene.RootSceneNode.CreateChildSceneNode();
//             lightSphere = scene.CreateEntity("LightSphere", "tiny_cube.mesh");
//             lightSphereNode.AttachObject(lightSphere);
//             lightSphereNode.Position = new Vector3(300, 260, -300);
            
//             // create a track for the light sphere
// 			Animation animSphere = scene.CreateAnimation("LightSphereTrack", 20);
// 			// spline it for nice curves
// 			animSphere.InterpolationMode = InterpolationMode.Spline;
// 			// create a track to animate the camera's node
// 			AnimationTrack trackSphere = animSphere.CreateTrack(0, lightSphereNode);
// 			// setup keyframes
//             for (int i = 0; i <= 10; i++) {
//                 Vector3 v = track.KeyFrames[i].Translate;
//                 v.y += 10;
//                 key = trackSphere.CreateKeyFrame(i * 2);
//                 key.Translate = v;
//             }

// 			// create a new animation state to track this
// 			animSphereState = scene.CreateAnimationState("LightSphereTrack");
// 			animSphereState.IsEnabled = true;

// 			// Make light node look at origin, this is for when we
// 			// change the moving light to a spotlight
// 			lightSphereNode.SetAutoTracking(true, scene.RootSceneNode);

			Mesh mesh = MeshManager.Instance.Load("athene.mesh");

            short srcIdx, destIdx;

            // the athene mesh requires tangent vectors
            if(!mesh.SuggestTangentVectorBuildParams(out srcIdx, out destIdx)) {
                mesh.BuildTangentVectors(srcIdx, destIdx);
            }

			SceneNode node; //= scene.RootSceneNode.CreateChildSceneNode();
//			athene = scene.CreateEntity("Athene", "athene.mesh");
//			athene.MaterialName = atheneMaterials[currentAtheneMaterial];
//			node.AttachObject(athene);
//			node.Translate(new Vector3(0, -20, 0));
//			node.Yaw(90);
			
 			Entity ent = null;

			node = scene.RootSceneNode.CreateChildSceneNode();
			ent = scene.CreateEntity("Column1", "column.mesh");
			ent.MaterialName = "Examples/Rockwall";
			node.AttachObject(ent);
			node.Translate(new Vector3(200, 0, -200));

			node = scene.RootSceneNode.CreateChildSceneNode();
			ent = scene.CreateEntity("Column2", "column.mesh");
			ent.MaterialName = "Examples/Rockwall";
			node.AttachObject(ent);
			node.Translate(new Vector3(200, 0, 200));

			node = scene.RootSceneNode.CreateChildSceneNode();
			ent = scene.CreateEntity("Column3", "column.mesh");
			ent.MaterialName = "Examples/Rockwall";
			node.AttachObject(ent);
			node.Translate(new Vector3(-200, 0, -200));

			node = scene.RootSceneNode.CreateChildSceneNode();
			ent = scene.CreateEntity("Column4", "column.mesh");
			ent.MaterialName = "Examples/Rockwall";
			node.AttachObject(ent);
			node.Translate(new Vector3(-200, 0, 200));

  			scene.SetSkyBox(true, "Skybox/Stormy", 3000);

			Plane plane = new Plane(Vector3.UnitY, -100);
			MeshManager.Instance.CreatePlane(
				"MyPlane", plane, 1500, 1500, 20, 20, true, 1, 5, 5, Vector3.UnitZ);
			
			Entity planeEnt = scene.CreateEntity("Plane", "MyPlane");
			planeEnt.MaterialName = "Examples/Rockwall";
			planeEnt.CastShadows = false;
			scene.RootSceneNode.CreateChildSceneNode().AttachObject(planeEnt);

			if(Root.Instance.RenderSystem.Name.StartsWith("Axiom Direct")) {
				// In D3D, use a 1024x1024 shadow texture
				scene.SetShadowTextureSettings(1024, 2, PixelFormat.L16);
			}
			else {
				// Use 512x512 texture in GL since we can't go higher than the window res
                scene.SetShadowTextureSettings(512, 2, PixelFormat.L16);
			}

// 			scene.ShadowColor = new ColorEx(0.5f, 0.5f, 0.5f);
 			scene.ShadowColor = ColorEx.Black;

            scene.ShadowFarDistance = 1000f;
			
			// in case infinite far distance is not supported
			camera.Far = 100000;
		
            debugTextDelay = int.MaxValue;
            scene.ShadowTechnique = shadowTechniques[currentShadowTechnique];
			ApplyShadowTechnique();
		}

		protected long lastKeyPress = 0;
		
		protected override void OnFrameStarted(object source, FrameEventArgs e) {
			long t = System.Environment.TickCount;
			if((t - lastKeyPress) > 500) {
			    lastKeyPress = t;
				if (input.IsKeyPressed(KeyCodes.O))
					ChangeShadowTechnique();
				else if (input.IsKeyPressed(KeyCodes.N))
					Root.Instance.ToggleMetering(1);
				else if (input.IsKeyPressed(KeyCodes.L)) {
					currentLightType = ++currentLightType % lightTypes.Length;
					light.Type = lightTypes[currentLightType];
				}
				else if (input.IsKeyPressed(KeyCodes.M))
					lightMoving = !lightMoving;
			}
			base.OnFrameStarted (source, e);
 			if (lightMoving)
				animState.AddTime(e.TimeSinceLastFrame);
//          animSphereState.AddTime(e.TimeSinceLastFrame);
			window.DebugText =  string.Format("{0}, {1}", 
											  shadowTechniqueShortDescriptions[currentShadowTechnique],
											  lightTypeDescriptions[currentLightType]);
		}

		/// <summary>
		///		Method used to cycle through the shadow techniques.
		/// </summary>
		protected void ChangeShadowTechnique() {
			currentShadowTechnique = ++currentShadowTechnique % shadowTechniques.Length;
			if (!softShadowsSupported && shadowTechSoft[currentShadowTechnique])
				// Skip soft shadows if not supported
				currentShadowTechnique = ++currentShadowTechnique % shadowTechniques.Length;

//			if (shadowTechSoft[previousShadowTechnique] && !shadowTechSoft[currentShadowTechnique]) {
//				// Clean up compositors
// 				shadowCompositor->removeListener(&gaussianListener);
// 				CompositorManager::getSingleton().setCompositorEnabled(mShadowVp, 
// 					SHADOW_COMPOSITOR_NAME, false);
// 				// Remove entire compositor chain
// 				CompositorManager::getSingleton().removeCompositorChain(shadowVp);
// 				shadowVp = 0;
// 				shadowCompositor = 0;
//			}
			
			scene.ShadowTechnique = shadowTechniques[currentShadowTechnique];

			ApplyShadowTechnique();
		}
		
		/// <summary>
		///		Method to set the shadow parameters consistent with the current shadow technique.
		/// </summary>
		protected void ApplyShadowTechnique() {

			Vector3 direction = new Vector3();

			switch(shadowTechniques[currentShadowTechnique]) {
				case ShadowTechnique.StencilAdditive:
					// fixed light, dim
					if (showSunlight)
						sunLight.CastShadows = true;

					light.Type = LightType.Point;
					light.CastShadows = true;
					light.Diffuse = minLightColor;
					light.Specular = ColorEx.White;
					light.SetAttenuation(8000, 1, 0.0005f, 0);

					break;

				case ShadowTechnique.StencilModulative:
					// Multiple lights cause obvious silhouette edges in modulative mode
					// So turn off shadows on the direct light
					// Fixed light, dim

					if (showSunlight)
						sunLight.CastShadows = false;

					// point light, movable, reddish
					light.Type = LightType.Point;
					light.CastShadows = true;
					light.Diffuse = minLightColor;
					light.Specular = ColorEx.White;
					light.SetAttenuation(8000, 1, 0.0005f, 0);

					break;

				case ShadowTechnique.TextureAdditive:
				case ShadowTechnique.TextureModulative:
					// Change fixed point light to spotlight
					// Fixed light, dim
					if (showSunlight)
						sunLight.CastShadows = true;
                    light.Type = LightType.Directional;
 					light.Direction = (Vector3.NegativeUnitZ);
					light.Diffuse = minLightColor;
					light.Specular = ColorEx.White;
					light.SetAttenuation(80000, 1, 0.0005f, 0);
					lightNode.Position = new Vector3(300, 750, -300);
					light.SetSpotlightRange(80, 90);

					break;
			}
		}
	}

	/// <summary>
	///		This class 'wibbles' the light and billboard.
	/// </summary>
	public class LightWibbler : IControllerValue<float> {
		#region Fields

		protected Light light;
		protected Billboard billboard;
		protected ColorEx colorRange = new ColorEx();
		protected ColorEx minColor;
		protected float minSize;
		protected float sizeRange;
		protected float intensity;

		#endregion Fields

		#region Constructor

		public LightWibbler(Light light, Billboard billboard, ColorEx minColor,
			ColorEx maxColor, float minSize, float maxSize) {

			this.light = light;
			this.billboard = billboard;
			this.minColor = minColor;
			colorRange.r = maxColor.r - minColor.r;
			colorRange.g = maxColor.g - minColor.g;
			colorRange.b = maxColor.b - minColor.b;
			this.minSize = minSize;
			sizeRange = maxSize - minSize;
		}

		#endregion Constructor

		#region IControllerValue Members

		public float Value {
			get {
				return intensity;
			}
			set {
				intensity = value;

				ColorEx newColor = new ColorEx();

				// Attenuate the brightness of the light
				newColor.r = minColor.r + (colorRange.r * intensity);
				newColor.g = minColor.g + (colorRange.g * intensity);
				newColor.b = minColor.b + (colorRange.b * intensity);

				light.Diffuse = newColor;
				billboard.Color = newColor;

				// set billboard size
				float newSize = minSize + (intensity * sizeRange);
				billboard.SetDimensions(newSize, newSize);
			}
		}

		#endregion
	}
}

