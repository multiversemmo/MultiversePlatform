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
using System.Runtime.InteropServices;

namespace Axiom.Graphics {
	/// <summary>
	///		Describes the graphics API independent functionality required by a hardware
	///		index buffer.  
    /// </summary>
    public abstract class HardwareIndexBuffer : HardwareBuffer 
	{
        #region Fields

		/// <summary>
		///		Type of index (16 or 32 bit).
		/// </summary>
        protected IndexType type;
		/// <summary>
		///		Number of indices in this buffer.
		/// </summary>
        protected int numIndices;
        /// <summary>
        ///     Size of each index.
        /// </summary>
        protected int indexSize;

        #endregion

        #region Constructors

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="type">Type of index (16 or 32 bit).</param>
		/// <param name="numIndices">Number of indices to create in this buffer.</param>
		/// <param name="usage">Buffer usage.</param>
		/// <param name="useSystemMemory">Create in system memory?</param>
		/// <param name="useShadowBuffer">Use a shadow buffer for reading/writing?</param>
        public HardwareIndexBuffer(IndexType type, int numIndices, BufferUsage usage, bool useSystemMemory, bool useShadowBuffer) 
            : base(usage, useSystemMemory, useShadowBuffer) {
            this.type = type;
            this.numIndices = numIndices;

            // calc the index buffer size
            sizeInBytes = numIndices;

            if (type == IndexType.Size32) {
                indexSize = Marshal.SizeOf(typeof(int));
            }
            else {
                indexSize = Marshal.SizeOf(typeof(short));
            }

            sizeInBytes *= indexSize;

            // create a shadow buffer if required
            if(useShadowBuffer) {
                shadowBuffer = new SoftwareIndexBuffer(type, numIndices, BufferUsage.Dynamic);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Gets an enum specifying whether this index buffer is 16 or 32 bit elements.
        /// </summary>
        public IndexType Type {
            get { 
				return type; 
			}
        }

		/// <summary>
		///		Gets the number of indices in this buffer.
		/// </summary>
		public int IndexCount {
			get {
				return numIndices;
			}
		}

        /// <summary>
        ///     Gets the size (in bytes) of each index element.
        /// </summary>
        /// <value></value>
        public int IndexSize {
            get {
                return indexSize;
            }
        }

        #endregion
    }
}
