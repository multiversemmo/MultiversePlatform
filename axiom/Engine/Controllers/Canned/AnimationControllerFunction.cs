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
using Axiom.Controllers;

namespace Axiom.Controllers.Canned {
	/// <summary>
	///     Predefined controller function for dealing with animation.
	/// </summary>
	public class AnimationControllerFunction : IControllerFunction<float> {
        #region Fields
        
        /// <summary>
        ///     The amount of time in seconds it takes to loop through the whole animation sequence.
        /// </summary>
        protected float sequenceTime;

        /// <summary>
        ///     The offset in seconds at which to start (default is start at 0).
        /// </summary>
        protected float time;

        #endregion Fields

        #region Constructor

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="sequenceTime">The amount of time in seconds it takes to loop through the whole animation sequence.</param>
        public AnimationControllerFunction(float sequenceTime) : this(sequenceTime, 0.0f) {}

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="sequenceTime">The amount of time in seconds it takes to loop through the whole animation sequence.</param>
        /// <param name="timeOffset">The offset in seconds at which to start.</param>
		public AnimationControllerFunction(float sequenceTime, float timeOffset) {
            this.sequenceTime = sequenceTime;
            this.time = timeOffset;
		}

        #endregion

        #region ControllerFunction Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        public float Execute(float sourceValue) {
            // assuming source if the time since the last update
            time += sourceValue;

            // wrap
            while(time >= sequenceTime) {
                time -= sequenceTime;
            }

            // return parametric
            return time / sequenceTime;
        }

        #endregion ControllerFunction Members
	}
}
