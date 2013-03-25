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
using System.Collections;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.Controllers.Canned;
using Axiom.Graphics;

namespace Axiom.Controllers {
    /// <summary>
    /// Summary description for ControllerManager.
    /// </summary>
    public sealed class ControllerManager : IDisposable {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static ControllerManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal ControllerManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static ControllerManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        #region Member variables

        /// <summary>
        ///		List of references to controllers in a scene.
        /// </summary>
        private List<Controller<float>> controllers = new List<Controller<float>>();

        /// <summary>
        ///		Local instance of a FrameTimeControllerValue to be used for time based controllers.
        /// </summary>
        private IControllerValue<float> frameTimeController = new FrameTimeControllerValue();

        private IControllerFunction<float> passthroughFunction = new PassthroughControllerFunction();
        private ulong lastFrameNumber = 0;


        #endregion

        #region Methods

        /// <summary>
        ///		Overloaded method.  Creates a new controller, using a reference to a FrameTimeControllerValue as
        ///		the source.
        /// </summary>
        /// <param name="destination">Controller value to use as the destination.</param>
        /// <param name="function">Controller funcion that will use the source value to set the destination.</param>
        /// <returns>A newly created controller object that will be updated during the main render loop.</returns>
        public Controller<float> CreateController(IControllerValue<float> destination, IControllerFunction<float> function) {
            // call the overloaded method passing in our precreated frame time controller value as the source
            return CreateController(frameTimeController, destination, function);
        }

        /// <summary>
        ///		Factory method for creating an instance of a controller based on the input provided.
        /// </summary>
        /// <param name="source">Controller value to use as the source.</param>
        /// <param name="destination">Controller value to use as the destination.</param>
        /// <param name="function">Controller funcion that will use the source value to set the destination.</param>
        /// <returns>A newly created controller object that will be updated during the main render loop.</returns>
        public Controller<float> CreateController(IControllerValue<float> source, IControllerValue<float> destination, IControllerFunction<float> function) {
            // create a new controller object
            Controller<float> controller = new Controller<float>(source, destination, function);

            // add the new controller to our list
            controllers.Add(controller);

            return controller;
        }

        public void DestroyController(Controller<float> controller)
        {
            controllers.Remove(controller);
        }

        public Controller<float> CreateFrameTimePassthroughController(IControllerValue<float> dest)
        {
            return CreateController(frameTimeController, dest, passthroughFunction);
        }

        public float GetElapsedTime() {
            return ((FrameTimeControllerValue)frameTimeController).ElapsedTime;
        }


        /// <summary>
        ///     Creates a texture layer animator controller.
        /// </summary>
        /// <remarks>
        ///     This helper method creates the Controller, IControllerValue and IControllerFunction classes required
        ///     to animate a texture.
        /// </remarks>
        /// <param name="texUnit">The texture unit to animate.</param>
        /// <param name="sequenceTime">Length of the animation (in seconds).</param>
        /// <returns>A newly created controller object that will be updated during the main render loop.</returns>
        public Controller<float> CreateTextureAnimator(TextureUnitState texUnit, float sequenceTime) {
            IControllerValue<float> val = new TextureFrameControllerValue(texUnit);
            IControllerFunction<float> func = new AnimationControllerFunction(sequenceTime);

            return CreateController(val, func);
        }

        /// <summary>
        ///     Creates a basic time-based texture coordinate modifier designed for creating rotating textures.
        /// </summary>
        /// <remarks>
        ///     This simple method allows you to easily create constant-speed rotating textures. If you want more
        ///     control, look up the ControllerManager.CreateTextureWaveTransformer for more complex wave-based
        ///     scrollers / stretchers / rotaters.
        /// </remarks>
        /// <param name="layer">The texture unit to animate.</param>
        /// <param name="speed">Speed of the rotation, in counter-clockwise revolutions per second.</param>
        /// <returns>A newly created controller object that will be updated during the main render loop.</returns>
        public Controller<float> CreateTextureRotator(TextureUnitState layer, float speed) {
            IControllerValue<float> val = new TexCoordModifierControllerValue(layer, false, false, false, false, true);
            IControllerFunction<float> func = new MultipyControllerFunction(-speed, true);

            return CreateController(val, func);
        }

        /// <summary>
        ///     Predefined controller value for setting a single floating-
        ///     point value in a constant paramter of a vertex or fragment program.
        /// </summary>
        /// <remarks>
        ///     Any value is accepted, it is propagated into the 'x'
        ///     component of the constant register identified by the index. If you
        ///     need to use named parameters, retrieve the index from the param
        ///     object before setting this controller up.
        /// </remarks>
        /// <param name="parms"></param>
        /// <param name="index"></param>
        /// <param name="timeFactor"></param>
        /// <returns></returns>
        public Controller<float> CreateGpuProgramTimerParam(GpuProgramParameters parms, int index, float timeFactor) {
            IControllerValue<float> val = new FloatGpuParamControllerValue(parms, index);
            IControllerFunction<float> func = new MultipyControllerFunction(timeFactor, true);

            return CreateController(val, func);
        }

        /// <summary>
        ///     Creates a basic time-based texture coordinate modifier designed for creating rotating textures.
        /// </summary>
        /// <remarks>
        ///     This simple method allows you to easily create constant-speed scrolling textures. If you want more
        ///     control, look up the ControllerManager.CreateTextureWaveTransformer for more complex wave-based
        ///     scrollers / stretchers / rotaters.
        /// </remarks>
        /// <param name="layer">The texture unit to animate.</param>
        /// <param name="speedU">Horizontal speed, in wraps per second.</param>
        /// <param name="speedV">Vertical speed, in wraps per second.</param>
        /// <returns>A newly created controller object that will be updated during the main render loop.</returns>
        public Controller<float> CreateTextureScroller(TextureUnitState layer, float speedU, float speedV) {
            IControllerValue<float> val = null;
            IControllerFunction<float> func = null;
            Controller<float> controller = null;

            // if both u and v speeds are the same, we can use a single controller for it
            if(speedU != 0 && (speedU == speedV)) {
                // create the value and function
                val = new TexCoordModifierControllerValue(layer, true, true);
                func = new MultipyControllerFunction(-speedU, true);

                // create the controller (uses FrameTime for source by default)
                controller = CreateController(val, func);
            }
            else {
                // create seperate for U
                if(speedU != 0) {
                    // create the value and function
                    val = new TexCoordModifierControllerValue(layer, true, false);
                    func = new MultipyControllerFunction(-speedU, true);

                    // create the controller (uses FrameTime for source by default)
                    controller = CreateController(val, func);
                }

                // create seperate for V
                if(speedV != 0) {
                    // create the value and function
                    val = new TexCoordModifierControllerValue(layer, false, true);
                    func = new MultipyControllerFunction(-speedV, true);

                    // create the controller (uses FrameTime for source by default)
                    controller = CreateController(val, func);
                }
            }

            // TODO: Revisit, since we can't return 2 controllers in the case of non equal U and V speeds
            return controller;
        }

        /// <summary>
        ///	    Creates a very flexible time-based texture transformation which can alter the scale, position or
        ///	    rotation of a texture based on a wave function.	
        /// </summary>
        /// <param name="layer">The texture unit to effect.</param>
        /// <param name="transformType">The type of transform, either translate (scroll), scale (stretch) or rotate (spin).</param>
        /// <param name="waveType">The shape of the wave, see WaveformType enum for details.</param>
        /// <param name="baseVal">The base value of the output.</param>
        /// <param name="frequency">The speed of the wave in cycles per second.</param>
        /// <param name="phase">The offset of the start of the wave, e.g. 0.5 to start half-way through the wave.</param>
        /// <param name="amplitude">Scales the output so that instead of lying within 0..1 it lies within 0..(1 * amplitude) for exaggerated effects</param>
        /// <returns>A newly created controller object that will be updated during the main render loop.</returns>
        public Controller<float> CreateTextureWaveTransformer(TextureUnitState layer, TextureTransform type, WaveformType waveType, 
            float baseVal, float frequency, float phase, float amplitude) {
            IControllerValue<float> val = null;
            IControllerFunction<float> function = null;

            // determine which type of controller value this layer needs
            switch(type) {
                case TextureTransform.TranslateU:
                    val = new TexCoordModifierControllerValue(layer, true, false);
                    break;

                case TextureTransform.TranslateV:
                    val = new TexCoordModifierControllerValue(layer, false, true);
                    break;

                case TextureTransform.ScaleU:
                    val = new TexCoordModifierControllerValue(layer, false, false, true, false, false);
                    break;

                case TextureTransform.ScaleV:
                    val = new TexCoordModifierControllerValue(layer, false, false, false, true, false);
                    break;

                case TextureTransform.Rotate:
                    val = new TexCoordModifierControllerValue(layer, false, false, false, false, true);
                    break;
            } // switch

            // create a new waveform controller function
            function = new WaveformControllerFunction(waveType, baseVal, frequency, phase, amplitude, true);

            // finally, create the controller using frame time as the source value
            return CreateController(frameTimeController, val, function);
        }

        /// <summary>
        ///		Causes all registered controllers to execute.  This will depend on RenderSystem.BeginScene already
        ///		being called so that the time since last frame can be obtained for calculations.
        /// </summary>
        public void UpdateAll() {
            ulong thisFrameNumber = Root.Instance.CurrentFrameCount;
            if (thisFrameNumber != lastFrameNumber) {
                // loop through each controller and tell it to update
                for (int i = 0; i < controllers.Count; i++) {
                    Controller<float> controller = controllers[i];
                    controller.Update();
                }
                lastFrameNumber = thisFrameNumber;
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public void Dispose() {
            controllers.Clear();

            instance = null;
        }

        #endregion IDisposable Implementation
    }
}
