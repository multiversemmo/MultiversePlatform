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
using Axiom.Core;
using Axiom.Input;
using Axiom.MathLib;
using Axiom.ParticleSystems;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    public class SkyDome : DemoBase {
        #region Fields
        private float curvature = 1;
        private float tiling = 15;
        private float timeDelay = 0;
        private Entity ogre;

        #endregion Fields

        protected override void OnFrameStarted(Object source, FrameEventArgs e) {
            base.OnFrameStarted(source, e);

            bool updateSky = false;

            if(input.IsKeyPressed(KeyCodes.H) && timeDelay <= 0) {
                curvature += 1;
                timeDelay = 0.1f;
                updateSky = true;
            }

            if(input.IsKeyPressed(KeyCodes.G) && timeDelay <= 0) {
                curvature -= 1;
                timeDelay = 0.1f;
                updateSky = true;
            }

            if(input.IsKeyPressed(KeyCodes.U) && timeDelay <= 0) {
                tiling += 1;
                timeDelay = 0.1f;
                updateSky = true;
            }

            if(input.IsKeyPressed(KeyCodes.Y) && timeDelay <= 0) {
                tiling -= 1;
                timeDelay = 0.1f;
                updateSky = true;
            }

            if(timeDelay > 0) {
                timeDelay -= e.TimeSinceLastFrame;
            }

            if(updateSky) {
                scene.SetSkyDome(true, "Examples/CloudySky", curvature, tiling);
            }
        }

        #region Methods

        protected override void CreateScene() {
            // set ambient light
            scene.AmbientLight = new ColorEx(1.0f, 0.5f, 0.5f, 0.5f);

            // create a skydome
            scene.SetSkyDome(true, "Examples/CloudySky", 5, 8);

            // create a light
            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);

            // add a floor plane
            Plane p = new Plane();
            p.Normal = Vector3.UnitY;
            p.D = 200;
            MeshManager.Instance.CreatePlane("FloorPlane", p, 2000, 2000, 1, 1, true, 1, 5, 5, Vector3.UnitZ);

            // add the floor entity
            Entity floor = scene.CreateEntity("Floor", "FloorPlane");
            floor.MaterialName = "Examples/RustySteel";
            scene.RootSceneNode.CreateChildSceneNode().AttachObject(floor);

            ogre = scene.CreateEntity("Ogre", "ogrehead.mesh");
            scene.RootSceneNode.CreateChildSceneNode().AttachObject(ogre);
        }

        #endregion
    }
}
