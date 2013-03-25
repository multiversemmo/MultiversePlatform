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
using System.Diagnostics;
using Axiom.MathLib;

namespace Axiom.Graphics {
    /// <summary>
    ///		Records the state of all the vertex buffer bindings required to provide a vertex declaration
    ///		with the input data it needs for the vertex elements.
    ///	 </summary>
    ///	 <remarks>
    ///		Why do we have this binding list rather than just have VertexElement referring to the
    ///		vertex buffers direct? Well, in the underlying APIs, binding the vertex buffers to an
    ///		index (or 'stream') is the way that vertex data is linked, so this structure better
    ///		reflects the realities of that. In addition, by separating the vertex declaration from
    ///		the list of vertex buffer bindings, it becomes possible to reuse bindings between declarations
    ///		and vice versa, giving opportunities to reduce the state changes required to perform rendering.
    /// </remarks>
    public class VertexBufferBinding {
        #region Fields

		/// <summary>
		///		Defines the vertex buffer bindings used as source for vertex declarations.
		/// </summary>
        protected Dictionary<ushort, HardwareVertexBuffer> bindingMap = 
            new Dictionary<ushort, HardwareVertexBuffer>();
		/// <summary>
		///		The highest index in use for this binding.
		/// </summary>
        protected ushort highIndex;
		
        #endregion Fields

        #region Methods
		
        /// <summary>
        ///		Set a binding, associating a vertex buffer with a given index.
        /// </summary>
        /// <remarks>
        ///		If the index is already associated with a vertex buffer, 
        ///		the association will be replaced. This may cause the old buffer
        ///		to be destroyed if nothing else is referring to it.
        ///		You should assign bindings from 0 and not leave gaps, although you can
        ///		bind them in any order.
        /// </remarks>
        /// <param name="index">Index at which to bind the buffer.</param>
        /// <param name="buffer">Vertex buffer to bind.</param>
        public virtual void SetBinding(ushort index, HardwareVertexBuffer buffer) {
            bindingMap[index] = buffer;
            highIndex = (ushort)MathUtil.Max(highIndex, index + 1);
        }

        /// <summary>
        ///		Removes an existing binding.
        /// </summary>
        /// <param name="index">Index of the buffer binding to remove.</param>
        public virtual void UnsetBinding(ushort index) {
            Debug.Assert(bindingMap.ContainsKey(index), "Cannot find buffer for index" + index);

            bindingMap.Remove(index);
        }

        /// <summary>
        ///		Removes all current buffer bindings.
        /// </summary>
        public virtual void UnsetAllBindings() {
            bindingMap.Clear();
        }

        /// <summary>
        ///		Gets the buffer bound to the given source index.
        /// </summary>
        /// <param name="index">Index of the binding to retreive the buffer for.</param>
        /// <returns>Buffer at the specified index.</returns>
        public virtual HardwareVertexBuffer GetBuffer(ushort index) {
            Debug.Assert(bindingMap.ContainsKey(index), "No buffer is bound to index " + index);

			HardwareVertexBuffer buf = bindingMap[index];

            return buf;
        }

        #endregion
		
        #region Properties
		
        /// <summary>
        ///		Gets an enumerator to iterate through the buffer bindings.
        /// </summary>
        /// TODO: Change this to strongly typed later on
        public virtual Dictionary<ushort, HardwareVertexBuffer> Bindings {
            get { 
                return bindingMap; 
            }
        }

		/// <summary>
		///		Gets the number of bindings.
		/// </summary>
		public int BindingCount {
			get {
				return bindingMap.Count;
			}
		}

        /// <summary>
        ///		Gets the highest index which has already been set, plus 1.
        /// </summary>
        /// <remarks>
		///		This is to assist in binding the vertex buffers such that there are
		///		not gaps in the list.
        /// </remarks>
        public virtual ushort NextIndex {
            get { 
                return highIndex++; 
            }
        }

        #endregion Properties
    }
}
