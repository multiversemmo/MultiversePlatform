using System;
using System.Collections;
using System.Collections.Generic;
using Axiom.Animating;
using Axiom.Collections;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
	/// <summary>
	/// Summary description for Fresnel.
	/// </summary>
	public class Fresnel : DemoBase {
        #region Fields

        Camera theCam;
        Entity planeEnt;
        List<Entity> aboveWaterEnts = new List<Entity>();
        List<Entity> belowWaterEnts = new List<Entity>();

        const int NUM_FISH = 30;
        const int NUM_FISH_WAYPOINTS = 10;
        const int FISH_PATH_LENGTH = 200;
        AnimationState[] fishAnimations = new AnimationState[NUM_FISH];
        PositionalSpline[] fishSplines = new PositionalSpline[NUM_FISH];
        Vector3[] fishLastPosition = new Vector3[NUM_FISH];
        SceneNode[] fishNodes = new SceneNode[NUM_FISH];
        float animTime;
        Plane reflectionPlane = new Plane();

        #endregion Fields

        #region Constructors

        public Fresnel() {
            for(int i = 0; i < NUM_FISH; i++) {
                fishSplines[i] = new PositionalSpline();
            }
        }

        #endregion Constructors
        
        #region Methods

        protected override void CreateScene() {
            // Check gpu caps
            if( !GpuProgramManager.Instance.IsSyntaxSupported("ps_2_0") &&
                !GpuProgramManager.Instance.IsSyntaxSupported("ps_1_4") &&
                !GpuProgramManager.Instance.IsSyntaxSupported("arbfp1")) {

                throw new Exception("Your hardware does not support advanced pixel shaders, so you cannot run this demo.  Time to go to Best Buy ;)");
            }

            Animation.DefaultInterpolationMode = InterpolationMode.Linear;

            theCam = camera;
            theCam.Position = new Vector3(-100, 20, 700);

            // set the ambient scene light
            scene.AmbientLight = new ColorEx(0.5f, 0.5f, 0.5f);

            Light light = scene.CreateLight("MainLight");
            light.Type = LightType.Directional;
            light.Direction = -Vector3.UnitY;

            Material mat = MaterialManager.Instance.GetByName("Examples/FresnelReflectionRefraction");

            // Refraction texture
            RenderTexture rttTex = Root.Instance.RenderSystem.CreateRenderTexture("Refraction", 512, 512); 
            {
                Viewport vp = rttTex.AddViewport(camera, 0, 0, 1.0f, 1.0f, 0);
                vp.OverlaysEnabled = false;
                mat.GetTechnique(0).GetPass(0).GetTextureUnitState(2).SetTextureName("Refraction");
                rttTex.BeforeUpdate += new RenderTargetUpdateEventHandler(Refraction_BeforeUpdate);
                rttTex.AfterUpdate += new RenderTargetUpdateEventHandler(Refraction_AfterUpdate);                                                                                            
            }

            // Reflection texture
            rttTex = Root.Instance.RenderSystem.CreateRenderTexture("Reflection", 512, 512); 
            {
                Viewport vp = rttTex.AddViewport(camera, 0, 0, 1.0f, 1.0f, 0);
                vp.OverlaysEnabled = false;
                mat.GetTechnique(0).GetPass(0).GetTextureUnitState(1).SetTextureName("Reflection");
                rttTex.BeforeUpdate += new RenderTargetUpdateEventHandler(Reflection_BeforeUpdate);
                rttTex.AfterUpdate += new RenderTargetUpdateEventHandler(Reflection_AfterUpdate);                                                                                                     
            }

            reflectionPlane.Normal = Vector3.UnitY;
            reflectionPlane.D = 0;
            MeshManager.Instance.CreatePlane(
                "ReflectionPlane", reflectionPlane, 1500, 1500, 10, 10, true, 1, 5, 5, Vector3.UnitZ);

            planeEnt = scene.CreateEntity("Plane", "ReflectionPlane");
            planeEnt.MaterialName = "Examples/FresnelReflectionRefraction";
            scene.RootSceneNode.CreateChildSceneNode().AttachObject(planeEnt);

            scene.SetSkyBox(true, "Examples/CloudyNoonSkyBox", 2000);

            SceneNode myRootNode = scene.RootSceneNode.CreateChildSceneNode();

            Entity ent;

            // Above water entities - NB all meshes are static
            ent = scene.CreateEntity( "head1", "head1.mesh" );
            myRootNode.AttachObject(ent);
            aboveWaterEnts.Add(ent);
            ent = scene.CreateEntity( "Pillar1", "Pillar1.mesh" );
            myRootNode.AttachObject(ent);
            aboveWaterEnts.Add(ent);
            ent = scene.CreateEntity( "Pillar2", "Pillar2.mesh" );
            myRootNode.AttachObject(ent);
            aboveWaterEnts.Add(ent);
            ent = scene.CreateEntity( "Pillar3", "Pillar3.mesh" );
            myRootNode.AttachObject(ent);
            aboveWaterEnts.Add(ent);
            ent = scene.CreateEntity( "Pillar4", "Pillar4.mesh" );
            myRootNode.AttachObject(ent);
            aboveWaterEnts.Add(ent);
            ent = scene.CreateEntity( "UpperSurround", "UpperSurround.mesh" );
            myRootNode.AttachObject(ent);
            aboveWaterEnts.Add(ent);

            // Now the below water ents
            ent = scene.CreateEntity( "LowerSurround", "LowerSurround.mesh" );
            myRootNode.AttachObject(ent);
            belowWaterEnts.Add(ent);
            ent = scene.CreateEntity( "PoolFloor", "PoolFloor.mesh" );
            myRootNode.AttachObject(ent);
            belowWaterEnts.Add(ent);

            for (int fishNo = 0; fishNo < NUM_FISH; fishNo++) {
                ent = scene.CreateEntity(string.Format("fish{0}", fishNo), "fish.mesh");
                fishNodes[fishNo] = myRootNode.CreateChildSceneNode();
                fishAnimations[fishNo] = ent.GetAnimationState("swim");
                fishAnimations[fishNo].IsEnabled = true;
                fishNodes[fishNo].AttachObject(ent);
                belowWaterEnts.Add(ent);

                // Generate a random selection of points for the fish to swim to
                fishSplines[fishNo].AutoCalculate = false;
                
                Vector3 lastPos = Vector3.Zero;

                for (int waypoint = 0; waypoint < NUM_FISH_WAYPOINTS; waypoint++){
                    Vector3 pos = new Vector3(
                        MathUtil.SymmetricRandom() * 700, -10, MathUtil.SymmetricRandom() * 700);

                    if (waypoint > 0)
                    {
                        // check this waypoint isn't too far, we don't want turbo-fish ;)
                        // since the waypoints are achieved every 5 seconds, half the length
                        // of the pond is ok
                        while ((lastPos - pos).Length > 750)
                        {
                            pos = new Vector3(
                                MathUtil.SymmetricRandom() * 700, -10, MathUtil.SymmetricRandom() * 700);
                        }
                    }

                    fishSplines[fishNo].AddPoint(pos);
                    lastPos = pos;
                }

                // close the spline
                fishSplines[fishNo].AddPoint(fishSplines[fishNo].GetPoint(0));
                // recalc
                fishSplines[fishNo].RecalculateTangents();
            }
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            animTime += e.TimeSinceLastFrame;

            while(animTime > FISH_PATH_LENGTH) {
                animTime -= FISH_PATH_LENGTH;
            }

            for(int i = 0; i < NUM_FISH; i++) {
                // animate the fish
                fishAnimations[i].AddTime(e.TimeSinceLastFrame);

                // move the fish
                Vector3 newPos = fishSplines[i].Interpolate(animTime / FISH_PATH_LENGTH);
                fishNodes[i].Position = newPos;

                // work out the moving direction
                Vector3 direction = fishLastPosition[i] - newPos;
                direction.Normalize();
                Quaternion orientation = -Vector3.UnitX.GetRotationTo(direction);
                fishNodes[i].Orientation = orientation;
                fishLastPosition[i] = newPos;
            }

            base.OnFrameStarted(source, e);
        }


        #endregion Methods

        #region Event Handlers

        private void Reflection_BeforeUpdate(object sender, RenderTargetUpdateEventArgs e) {
            planeEnt.IsVisible = false;

            for(int i = 0; i < belowWaterEnts.Count; i++) {
                ((Entity)belowWaterEnts[i]).IsVisible = false;
            }

            theCam.EnableReflection(reflectionPlane);  
        }

        private void Reflection_AfterUpdate(object sender, RenderTargetUpdateEventArgs e) {
            planeEnt.IsVisible = true;

            for(int i = 0; i < belowWaterEnts.Count; i++) {
                ((Entity)belowWaterEnts[i]).IsVisible = true;
            }

            theCam.DisableReflection();  
        }

        private void Refraction_BeforeUpdate(object sender, RenderTargetUpdateEventArgs e) {
            planeEnt.IsVisible = false;

            for(int i = 0; i < aboveWaterEnts.Count; i++) {
                ((Entity)aboveWaterEnts[i]).IsVisible = false;
            } 
        }

        private void Refraction_AfterUpdate(object sender, RenderTargetUpdateEventArgs e) {
            planeEnt.IsVisible = true;

            for(int i = 0; i < aboveWaterEnts.Count; i++) {
                ((Entity)aboveWaterEnts[i]).IsVisible = true;
            }
        }

        #endregion Event Handlers
    }
}
