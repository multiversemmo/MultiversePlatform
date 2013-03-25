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
    public class SkyBox : DemoBase {
        #region Fields
        private float defaultDimension = 25;
        private float defaultVelocity = 50;
        protected ParticleSystem thrusters = null;
        #endregion Fields

        protected override void OnFrameStarted(Object source, FrameEventArgs e) {
            base.OnFrameStarted (source, e);

            if(input.IsKeyPressed(KeyCodes.N)) {
                thrusters.DefaultWidth = defaultDimension + 0.25f;
                thrusters.DefaultHeight = defaultDimension + 0.25f;
                defaultDimension += 0.25f;
            }

            if(input.IsKeyPressed(KeyCodes.M)) {
                thrusters.DefaultWidth = defaultDimension - 0.25f;
                thrusters.DefaultHeight = defaultDimension - 0.25f;
                defaultDimension -= 0.25f;
            }

            if(input.IsKeyPressed(KeyCodes.H)) {
                thrusters.GetEmitter(0).ParticleVelocity = defaultVelocity + 1;
                thrusters.GetEmitter(1).ParticleVelocity = defaultVelocity + 1;
                defaultVelocity += 1;
            }

            if(input.IsKeyPressed(KeyCodes.J) && !(defaultVelocity < 0.0f)) {
                thrusters.GetEmitter(0).ParticleVelocity = defaultVelocity - 1;
                thrusters.GetEmitter(1).ParticleVelocity = defaultVelocity - 1;
                defaultVelocity -= 1;
            }
        }

        #region Methods
        protected override void CreateScene() {
            // since whole screen is being redrawn every frame, dont bother clearing
            // option works for GL right now, uncomment to test it out.  huge fps increase
            // also, depth_write in the skybox material must be set to on
            //mainViewport.ClearEveryFrame = false;

            // set ambient light
            scene.AmbientLight = new ColorEx(1.0f, 0.5f, 0.5f, 0.5f);

            // create a skybox
            scene.SetSkyBox(true, "Skybox/Space", 50);

            // create a light
            Light light = scene.CreateLight("MainLight");
            light.Position = new Vector3(20, 80, 50);

            // add a nice starship
            Entity ship = scene.CreateEntity("razor", "razor.mesh");
            scene.RootSceneNode.AttachObject(ship);

            thrusters = ParticleSystemManager.Instance.CreateSystem("ParticleSystem", 200);
            thrusters.MaterialName = "Particles/Flare";
            thrusters.DefaultWidth = 25;
            thrusters.DefaultHeight = 25;

            ParticleEmitter emitter1 = thrusters.AddEmitter("Point");
            ParticleEmitter emitter2 = thrusters.AddEmitter("Point");

            // thruster 1
            emitter1.Angle = 3;
            emitter1.TimeToLive = 0.2f;
            emitter1.EmissionRate = 70;
            emitter1.ParticleVelocity = 50;
            emitter1.Direction = -Vector3.UnitZ;
            emitter1.ColorRangeStart = ColorEx.White;
            emitter1.ColorRangeEnd = ColorEx.Red;

            // thruster 2
            emitter2.Angle = 3;
            emitter2.TimeToLive = 0.2f;
            emitter2.EmissionRate = 70;
            emitter2.ParticleVelocity = 50;
            emitter2.Direction = -Vector3.UnitZ;
            emitter2.ColorRangeStart = ColorEx.White;
            emitter2.ColorRangeEnd = ColorEx.Red;

            // set the position of the thrusters
            emitter1.Position = new Vector3(5.7f, 0, 0);
            emitter2.Position = new Vector3(-18, 0, 0);

            scene.RootSceneNode.CreateChildSceneNode(new Vector3(0, 6.5f, -67), Quaternion.Identity).AttachObject(thrusters);
        }
        #endregion
    }
}
