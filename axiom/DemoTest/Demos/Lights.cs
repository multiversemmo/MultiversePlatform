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
using Axiom.Animating;
using Axiom.Controllers;
using Axiom.Controllers.Canned;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Demos;

namespace Axiom.Demos {
    /// <summary>
    /// 	Summary description for Controllers.
    /// </summary>
    public class Lights : DemoBase {
        #region Member variables
		
        private SceneNode redYellowLightsNode;
        private SceneNode greenBlueLightsNode;
        private BillboardSet redYellowLights;
        private BillboardSet greenBlueLights;
        private Billboard redLightBoard;
        private Billboard greenLightBoard;
        private Billboard yellowLightBoard;
        private Billboard blueLightBoard;
        private Light redLight;
        private Light yellowLight;
        private Light greenLight;
        private Light blueLight;
        private IControllerFunction<float> redLightFunc;
        private IControllerFunction<float> yellowLightFunc;
        private IControllerFunction<float> greenLightFunc;
        private IControllerFunction<float> blueLightFunc;
        private IControllerValue<float> redLightFlasher;
        private IControllerValue<float> yellowLightFlasher;
        private IControllerValue<float> greenLightFlasher;
        private IControllerValue<float> blueLightFlasher;

        #endregion

        #region Methods

        protected override void CreateScene() {
            // set some ambient light
            scene.AmbientLight = new ColorEx(1, 0.1f, 0.1f, 0.1f);

            // set a basic skybox
            scene.SetSkyBox(true, "Skybox/Space", 5000.0f);

            // create the ogre head
            Entity ogre = scene.CreateEntity("OgreHead", "ogrehead.mesh");

            // attach the ogre to the scene
            scene.RootSceneNode.AttachObject(ogre);

            // create nodes for the billboard sets
            redYellowLightsNode = scene.RootSceneNode.CreateChildSceneNode();
            greenBlueLightsNode = scene.RootSceneNode.CreateChildSceneNode();

            // create a billboard set for creating billboards
            redYellowLights = scene.CreateBillboardSet("RedYellowLights", 20);
            redYellowLights.MaterialName = "Particles/Flare";
            redYellowLightsNode.AttachObject(redYellowLights);

            greenBlueLights = scene.CreateBillboardSet("GreenBlueLights", 20);
            greenBlueLights.MaterialName = "Particles/Flare";
            greenBlueLightsNode.AttachObject(greenBlueLights);

            // red light billboard in off set
            Vector3 redLightPos = new Vector3(78, -8, -70);
            redLightBoard = redYellowLights.CreateBillboard(redLightPos, ColorEx.Black);

            // yellow light billboard in off set
            Vector3 yellowLightPos = new Vector3(-4.5f, 30, -80);
            yellowLightBoard = redYellowLights.CreateBillboard(yellowLightPos, ColorEx.Black);

            // blue light billboard in off set
            Vector3 blueLightPos = new Vector3(-90, -8, -70);
            blueLightBoard = greenBlueLights.CreateBillboard(blueLightPos, ColorEx.Black);

            // green light billboard in off set
            Vector3 greenLightPos = new Vector3(50, 70, 80);
            greenLightBoard = greenBlueLights.CreateBillboard(greenLightPos, ColorEx.Black);

            // red light in off state
            redLight = scene.CreateLight("RedLight");
            redLight.Position = redLightPos;
            redLight.Type = LightType.Point;
            redLight.Diffuse = ColorEx.Black;
            redYellowLightsNode.AttachObject(redLight);

            // yellow light in off state
            yellowLight = scene.CreateLight("YellowLight");
            yellowLight.Type = LightType.Point;
            yellowLight.Position = yellowLightPos;
            yellowLight.Diffuse = ColorEx.Black;
            redYellowLightsNode.AttachObject(yellowLight);

            // green light in off state
            greenLight = scene.CreateLight("GreenLight");
            greenLight.Type = LightType.Point;
            greenLight.Position = greenLightPos;
            greenLight.Diffuse = ColorEx.Black;
            greenBlueLightsNode.AttachObject(greenLight);

            // blue light in off state
            blueLight = scene.CreateLight("BlueLight");
            blueLight.Type = LightType.Point;
            blueLight.Position = blueLightPos;
            blueLight.Diffuse = ColorEx.Black;
            greenBlueLightsNode.AttachObject(blueLight);

            // create controller function
            redLightFlasher = 
                new LightFlasherControllerValue(redLight, redLightBoard, ColorEx.Red);
            yellowLightFlasher = 
                new LightFlasherControllerValue(yellowLight, yellowLightBoard, ColorEx.Yellow);
            greenLightFlasher = 
                new LightFlasherControllerValue(greenLight, greenLightBoard, ColorEx.Green);
            blueLightFlasher = 
                new LightFlasherControllerValue(blueLight, blueLightBoard, ColorEx.Blue);

            // set up the controller value and function for flashing
            redLightFunc = new WaveformControllerFunction(WaveformType.Sine, 0, 0.5f, 0, 1);
            yellowLightFunc = new WaveformControllerFunction(WaveformType.Triangle, 0, 0.25f, 0, 1);
            greenLightFunc = new WaveformControllerFunction(WaveformType.Sine, 0, 0.25f, 0.5f, 1);
            blueLightFunc = new WaveformControllerFunction(WaveformType.Sine, 0, 0.75f, 0.5f, 1);

            // set up the controllers
            ControllerManager.Instance.CreateController(redLightFlasher, redLightFunc);
            ControllerManager.Instance.CreateController(yellowLightFlasher, yellowLightFunc);
            ControllerManager.Instance.CreateController(greenLightFlasher, greenLightFunc);
            ControllerManager.Instance.CreateController(blueLightFlasher, blueLightFunc);
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {
            base.OnFrameStarted (source, e);

            // move the billboards around a bit
            redYellowLightsNode.Yaw(10 * e.TimeSinceLastFrame);
            greenBlueLightsNode.Pitch(20 * e.TimeSinceLastFrame);
        }

        #endregion	
    }

    public class LightFlasherControllerValue : IControllerValue<float> {
        private Billboard billboard;
        private Light light;
        private float intensity;
        private ColorEx maxColor;

        public LightFlasherControllerValue(Light light, Billboard billboard, ColorEx maxColor) {
            this.billboard = billboard;
            this.light = light;
            this.maxColor = maxColor;
        }

        #region IControllerValue Members

        public float Value {
            get { return intensity; }
            set {
                intensity = value;

                ColorEx newColor = new ColorEx();

                newColor.r = maxColor.r * intensity;
                newColor.g = maxColor.g * intensity;
                newColor.b = maxColor.b * intensity;

                billboard.Color = newColor;
                light.Diffuse = newColor;
            }
        }

        #endregion

    }

}
