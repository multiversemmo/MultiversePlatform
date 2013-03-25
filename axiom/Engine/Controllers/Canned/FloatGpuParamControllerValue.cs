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
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.Controllers.Canned {
    /// <summary>
    ///     Predefined controller value for setting a single floating-
    ///     point value in a constant paramter of a vertex or fragment program.
    /// </summary>
    /// <remarks>
    ///     Any value is accepted, it is propagated into the 'x'
    ///     component of the constant register identified by the index. If you
    ///     need to use named parameters, retrieve the index from the param
    ///     object before setting this controller up.
    ///     <p/>
    ///     Note: Retrieving a value from the program parameters is not currently 
    ///     supported, therefore do not use this controller value as a source,
    ///     only as a target.
    /// </remarks>
	public class FloatGpuParamControllerValue : IControllerValue<float> {
        #region Fields

        /// <summary>
        ///     Gpu parameters to access.
        /// </summary>
        protected GpuProgramParameters parms;
        /// <summary>
        ///     The constant register index of the parameter to set.
        /// </summary>
        protected int index;
        /// <summary>
        ///     Member level Vector to use for returning.
        /// </summary>
        protected Vector4 vec4 = new Vector4(0, 0, 0, 0);

        #endregion Fields

        #region Constructor

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parms">Params to set.</param>
        /// <param name="index">Index of the parameter to set.</param>
		public FloatGpuParamControllerValue(GpuProgramParameters parms, int index) {
            this.parms = parms;
            this.index = index;
		}

        #endregion Constructor
	
        #region IControllerValue Members

        /// <summary>
        /// Gets or Sets the value of the GPU parameter
        /// </summary>
        public float Value {
            get {
                return parms.floatConstantsArray[index * 4];
            }
            set {
                parms.SetConstant(index, value);
            }
        }

        #endregion
    }
}
