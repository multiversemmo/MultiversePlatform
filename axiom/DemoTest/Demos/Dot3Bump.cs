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
using System.Diagnostics;
using System.Drawing;
using Axiom.Core;
using Axiom.Overlays;
using Axiom.Input;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    /// <summary>
    ///     Demonstrates dotproduct blending operation and normalization cube map
    ///     usage for achieving bump mapping effect.
    /// </summary>
    public class Dot3Bump : DemoBase {
        #region Fields

        const int NUM_LIGHTS = 3;

        float timeDelay = 0.0f;
            
        Entity[] entities = new Entity[NUM_LIGHTS];
        string[] entityMeshes = new string[] { "knot.mesh", "ogrehead.mesh" };
        Light[] lights = new Light[NUM_LIGHTS];
        BillboardSet[] lightFlareSets = new BillboardSet[NUM_LIGHTS];
        Billboard[] lightFlares = new Billboard[NUM_LIGHTS];
        Vector3[] lightPositions = new Vector3[] {
                                                     new Vector3(300, 0, 0),
                                                     new Vector3(-200, 50, 0),
                                                     new Vector3(0, -300, -100)
                                                 };

        float[] lightRotationAngles = new float[] { 0, 30, 75 };

        Vector3[] lightRotationAxes = new Vector3[] {
                                                        Vector3.UnitX,
                                                        Vector3.UnitZ,
                                                        Vector3.UnitY
                                                    };

        float[] lightSpeeds = new float[] { 30, 10, 50 };

        ColorEx[] diffuseLightColors = new ColorEx[] {
                                                         new ColorEx(1, 1, 1, 1),
                                                         new ColorEx(1, 1, 0, 0),
                                                         new ColorEx(1, 1, 1, 0.5f)
                                                     };

        ColorEx[] specularLightColors = new ColorEx[] {
                                                          new ColorEx(1, 1, 1, 1),
                                                          new ColorEx(1, 0, 0.8f, 0.8f),
                                                          new ColorEx(1, 1, 1, 0.8f)
                                                      };

        bool[] lightState = new bool[] { true, true, false };

        string[] materialNames = new string[] {
                                                  "Examples/BumpMapping/MultiLight",
                                                  "Examples/BumpMapping/SingleLight",
                                                  "Examples/BumpMapping/MultiLightSpecular"
                                              };

        int currentMaterial = 0;
        int currentEntity = 0;

        SceneNode mainNode;
        SceneNode[] lightNodes = new SceneNode[NUM_LIGHTS];
        SceneNode[] lightPivots = new SceneNode[NUM_LIGHTS];

        #endregion Fields

        protected override void CreateScene() {
            scene.AmbientLight = ColorEx.Black;

            // create scene node
            mainNode = scene.RootSceneNode.CreateChildSceneNode();

            // Load the meshes with non-default HBU options
            for(int mn = 0; mn < entityMeshes.Length; mn++) {
                Mesh mesh = MeshManager.Instance.Load(entityMeshes[mn],
                    BufferUsage.DynamicWriteOnly,
                    BufferUsage.StaticWriteOnly,
                    true, true, 1); //so we can still read it

                // Build tangent vectors, all our meshes use only 1 texture coordset
                short srcIdx, destIdx;

                if (!mesh.SuggestTangentVectorBuildParams(out srcIdx, out destIdx)) {
                    mesh.BuildTangentVectors(srcIdx, destIdx);
                }

                // Create entity
                entities[mn] = scene.CreateEntity("Ent" + mn.ToString(), entityMeshes[mn]);

                // Attach to child of root node
                mainNode.AttachObject(entities[mn]);

                // Make invisible, except for index 0
                if (mn == 0) {
                    entities[mn].MaterialName = materialNames[currentMaterial];
                }
                else {
                    entities[mn].IsVisible = false;
                }
            }

            for (int i = 0; i < NUM_LIGHTS; i++) {
                lightPivots[i] = scene.RootSceneNode.CreateChildSceneNode();
                lightPivots[i].Rotate(lightRotationAxes[i], lightRotationAngles[i]);

                // Create a light, use default parameters
                lights[i] = scene.CreateLight("Light" + i.ToString());
                lights[i].Position = lightPositions[i];
                lights[i].Diffuse = diffuseLightColors[i];
                lights[i].Specular = specularLightColors[i];
                lights[i].IsVisible = lightState[i];

                // Attach light
                lightPivots[i].AttachObject(lights[i]);

                // Create billboard for light
                lightFlareSets[i] = scene.CreateBillboardSet("Flare" + i.ToString());
                lightFlareSets[i].MaterialName = "Particles/Flare";
                lightPivots[i].AttachObject(lightFlareSets[i]);
                lightFlares[i] = lightFlareSets[i].CreateBillboard(lightPositions[i]);
                lightFlares[i].Color = diffuseLightColors[i];
                lightFlareSets[i].IsVisible = lightState[i];
            }
            // move the camera a bit right and make it look at the knot
            camera.MoveRelative(new Vector3(50, 0, 20));
            camera.LookAt(new Vector3(0, 0, 0));
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            base.OnFrameStarted(source, e);

            if(timeDelay > 0.0f) {
                timeDelay -= e.TimeSinceLastFrame;
            }
            else {
                if(input.IsKeyPressed(KeyCodes.O)) {
                    entities[currentEntity].IsVisible = false;
                    currentEntity = (++currentEntity) % entityMeshes.Length;
                    entities[currentEntity].IsVisible = true;
                    entities[currentEntity].MaterialName = materialNames[currentMaterial];
                }

                if(input.IsKeyPressed(KeyCodes.M)) {
                    currentMaterial = (++currentMaterial) % materialNames.Length;
                    entities[currentEntity].MaterialName = materialNames[currentMaterial];
                }

                if(input.IsKeyPressed(KeyCodes.D1)) {
                    FlipLightState(0);
                }

                if(input.IsKeyPressed(KeyCodes.D2)) {
                    FlipLightState(1);
                }

                if(input.IsKeyPressed(KeyCodes.D3)) {
                    FlipLightState(2);
                }

                timeDelay = 0.1f;
            }

            // animate the lights
            for(int i = 0; i < NUM_LIGHTS; i++) {
                lightPivots[i].Rotate(Vector3.UnitZ, lightSpeeds[i] * e.TimeSinceLastFrame);
            }
        }


        /// <summary>
        ///    Flips the light states for the light at the specified index.
        /// </summary>
        /// <param name="index"></param>
        void FlipLightState(int index) {
            lightState[index] = !lightState[index];
            lights[index].IsVisible = lightState[index];
            lightFlareSets[index].IsVisible = lightState[index];
        }
    }
}
