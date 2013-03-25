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

namespace Axiom.Controllers {
    /// <summary>
    ///		Subclasses of this class are responsible for performing a function on an input value for a Controller.
    ///	 </summary>
    ///	 <remarks>
    ///		This abstract class provides the interface that needs to be supported for a custom function which
    ///		can be 'plugged in' to a Controller instance, which controls some object value based on an input value.
    ///		For example, the WaveControllerFunction class provided by Ogre allows you to use various waveforms to
    ///		translate an input value to an output value.
    ///		<p/>
    ///		This base class implements IControllerFunction, but leaves the implementation up to the subclasses.
    /// </remarks>
    public abstract class BaseControllerFunction : IControllerFunction<float> {
        #region Member variables
		
        /// <summary>
        ///		If true, function will add input values together and wrap at 1.0 before evaluating.
        /// </summary>
        protected bool useDeltaInput;

        /// <summary>
        ///		Value to be added during evaluation.
        /// </summary>
        protected float deltaCount;
		
        #endregion

        #region Constructors

        public BaseControllerFunction(bool useDeltaInput) {
            this.useDeltaInput = useDeltaInput;
            deltaCount = 0;
        }

        #endregion

        #region Methods

        /// <summary>
        ///		Adjusts the input value by a delta.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual float AdjustInput(float input) {
			if(useDeltaInput) 
			{
				// wrap the value if it went past 1
                deltaCount = (deltaCount + input) % 1f;

                // return the adjusted input value
                return deltaCount;
            }
            else {
                // return the input value as is
                return input;
            }
        }

        #endregion

        #region IControllerFunction methods

        public abstract float Execute(float sourceValue);

        #endregion
    }
}
