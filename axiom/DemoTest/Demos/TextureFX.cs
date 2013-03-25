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
using System.Drawing;
using Axiom.Controllers;
using Axiom.Controllers.Canned;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    /// <summary>
    /// 	Summary description for TextureBlending.
    /// </summary>
    public class TextureFX : DemoBase {
        protected override void CreateScene() {
            // since whole screen is being redrawn every frame, dont bother clearing
            // option works for GL right now, uncomment to test it out.  huge fps increase
            // also, depth_write in the skybox material must be set to on
            //mainViewport.ClearEveryFrame = false;

            // set some ambient light
            scene.TargetRenderSystem.LightingEnabled = true;
            scene.AmbientLight = ColorEx.Gray;

            // create a point light (default)
            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);

            CreateScalingPlane();
            CreateScrollingKnot();
            CreateWateryPlane();

            // set up a material for the skydome
            Material skyMaterial = scene.CreateMaterial("SkyMat");
            skyMaterial.Lighting = false;
            // use a cloudy sky
            Pass pass = skyMaterial.GetTechnique(0).GetPass(0);
            TextureUnitState textureLayer = pass.CreateTextureUnitState("clouds.jpg");
            // scroll the clouds
            textureLayer.SetScrollAnimation(0.15f, 0);

            // create the skydome
            scene.SetSkyDome(true, "SkyMat", -5, 2);
        }

        private void CreateScalingPlane() {
            // create a prefab plane
            Entity plane = scene.CreateEntity("Plane", PrefabEntity.Plane);
            // give the plane a texture
            plane.MaterialName = "TextureFX/BumpyMetal";
            // add entity to the root scene node
            SceneNode node = scene.RootSceneNode.CreateChildSceneNode(new Vector3(-250, -40, -100), Quaternion.Identity);
            node.AttachObject(plane);
        }

        private void CreateScrollingKnot() {
            Entity knot = scene.CreateEntity("knot", "knot.mesh");
            knot.MaterialName = "TextureFX/Knot";
            // add entity to the root scene node
            SceneNode node = scene.RootSceneNode.CreateChildSceneNode(new Vector3(200, 50, 150), Quaternion.Identity);
            node.AttachObject(knot);
        }

        private void CreateWateryPlane() {
            // create a prefab plane
            Entity plane = scene.CreateEntity("WaterPlane", PrefabEntity.Plane);
            // give the plane a texture
            plane.MaterialName = "TextureFX/Water";
            // add entity to the root scene node
            scene.RootSceneNode.AttachObject(plane);
        }
    }
}
