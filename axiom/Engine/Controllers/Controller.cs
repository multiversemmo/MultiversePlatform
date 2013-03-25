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
    ///		Instances of this class 'control' the value of another object in the system.
    ///	</summary>
    ///	 <remarks>
    ///		Controller classes are used to manage the values of object automatically based
    ///		on the value of some input. For example, a Controller could animate a texture
    ///		by controlling the current frame of the texture based on time, or a different Controller
    ///		could change the color of a material used for a spaceship shield mesh based on the remaining
    ///		shield power level of the ship.
    ///		<p/>
    ///		The Controller is an intentionally abstract concept - it can generate values
    ///		based on input and a function, which can either be one of the standard ones
    ///		supplied, or a function can be 'plugged in' for custom behavior - see the <see cref="ControllerFunction"/> class for details.
    ///		Both the input and output values are via <see cref="ControllerValue"/> objects, meaning that any value can be both
    ///		input and output of the controller.
    ///		<p/>
    ///		While this is very flexible, it can be a little bit confusing so to make it simpler the most often used
    ///		controller setups are available by calling methods on the ControllerManager object.
    /// </remarks>
    public class Controller<T> {
        #region Member variables
		
        /// <summary>
        /// 
        /// </summary>
        protected IControllerValue<T> source;

        /// <summary>
        /// 
        /// </summary>
        protected IControllerValue<T> destination;

        /// <summary>
        ///		Local reference to the function to be used for this controller.
        /// </summary>
        protected IControllerFunction<T> function;

        /// <summary>
        ///		States whether or not this controller is enabled.
        /// </summary>
        protected bool isEnabled;
		
        #endregion

        #region Constructors

        /// <summary>
        ///		Main constructor.  Should not be used directly, rather a controller should be created using the
        ///		ControllerManager so it can keep track of them.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="function"></param>
        internal Controller(IControllerValue<T> source, IControllerValue<T> destination, IControllerFunction<T> function) {
            this.source = source;
            this.destination = destination;
            this.function = function;

            // enabled by default, of course
            isEnabled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        ///		Called to update the destination value for this controller.  Will be called during
        ///		the render loop by ControllerManager.
        /// </summary>
        public void Update() {
            // if we are enabled, set the destination value based on the return value of the
            // controller function ran using the source value
            if(isEnabled)
                destination.Value = function.Execute(source.Value);
        }

        #endregion

        #region Properties

        /// <summary>
        ///		The value that returns the source data for this controller.
        /// </summary>
        public IControllerValue<T> Source {
            get { return source; }
            set { source = value; }
        }

        /// <summary>
        ///		The object the sets the destination objects value.
        /// </summary>
        public IControllerValue<T> Destination {
            get { return destination; }
            set { destination = value; }
        }

        /// <summary>
        ///		Gets/Sets the eference to the function to be used for this controller.
        /// </summary>
        public IControllerFunction<T> Function {
            get { return function; }
            set { function = value; }
        }

        /// <summary>
        ///		Gets/Sets whether this controller is active or not.
        /// </summary>
        public bool IsEnabled {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        #endregion
    }
}
