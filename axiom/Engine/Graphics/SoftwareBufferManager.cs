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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Axiom.Graphics {
    /// <summary>
    /// 	Summary description for SoftwareBufferManager.
    /// </summary>
    // TODO: Switch go using GCHandle for array pointer after resolving stack overflow in TerrainSceneManager.
    public class SoftwareBufferManager : HardwareBufferManager {	
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numIndices"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        /// DOC
        public override HardwareIndexBuffer CreateIndexBuffer(IndexType type, int numIndices, BufferUsage usage) {
            return new SoftwareIndexBuffer(type, numIndices, usage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numIndices"></param>
        /// <param name="usage"></param>
        /// <param name="useShadowBuffer"></param>
        /// <returns></returns>
        /// DOC
        public override HardwareIndexBuffer CreateIndexBuffer(IndexType type, int numIndices, BufferUsage usage, bool useShadowBuffer) {
            return new SoftwareIndexBuffer(type, numIndices, usage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexSize"></param>
        /// <param name="numVerts"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        /// DOC
        public override HardwareVertexBuffer CreateVertexBuffer(int vertexSize, int numVerts, BufferUsage usage) {
            return new SoftwareVertexBuffer(vertexSize, numVerts, usage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexSize"></param>
        /// <param name="numVerts"></param>
        /// <param name="usage"></param>
        /// <param name="useShadowBuffer"></param>
        /// <returns></returns>
        /// DOC
        public override HardwareVertexBuffer CreateVertexBuffer(int vertexSize, int numVerts, BufferUsage usage, bool useShadowBuffer) {
            return new SoftwareVertexBuffer(vertexSize, numVerts, usage);
        }
		
        #endregion
		
        #region Properties
		
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class SoftwareVertexBuffer : HardwareVertexBuffer {
        #region Fields
		
        /// <summary>
        ///     Holds the buffer data.
        /// </summary>
        protected byte[] data;
        protected GCHandle handle;
		
        #endregion Fields

        #region Constructors
		
        /// <summary>
        ///		
        /// </summary>
        /// <remarks>
        ///		This is already in system memory, so no need to use a shadow buffer.
        /// </remarks>
        /// <param name="vertexSize"></param>
        /// <param name="numVertices"></param>
        /// <param name="usage"></param>
        /// DOC
        public SoftwareVertexBuffer(int vertexSize, int numVertices, BufferUsage usage)
            : base(vertexSize, numVertices, usage, true, false) {
            data = new byte[sizeInBytes];
        }

        #endregion

        #region Methods

        public override IntPtr Lock(int offset, int length, BufferLocking locking) {
            isLocked = true;

            // return the offset into the array as a pointer
            // return Marshal.UnsafeAddrOfPinnedArrayElement(data, offset);

            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return handle.AddrOfPinnedObject();
        }

        protected override IntPtr LockImpl(int offset, int length, BufferLocking locking) {
            isLocked = true;

            // return the offset into the array as a pointer
            // return Marshal.UnsafeAddrOfPinnedArrayElement(data, offset);
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return handle.AddrOfPinnedObject();
        }

        public override void ReadData(int offset, int length, IntPtr dest) {
            Debug.Assert((offset + length) <= sizeInBytes, "Buffer overrun while trying to read a software buffer.");
	
            unsafe {
                // get a pointer to the destination intptr
                byte* pDest = (byte*)dest.ToPointer();

                // copy the src data to the destination buffer
                for (int i = 0; i < length; i++) {
                    pDest[offset + i] = data[offset + i];
                }
            }
        }

        public override void Unlock() {
            isLocked = false;

            handle.Free();
        }

        public override void UnlockImpl() {
            isLocked = false;

            handle.Free();
        }

        public override void WriteData(int offset, int length, IntPtr src, bool discardWholeBuffer) {
            Debug.Assert((offset + length) <= sizeInBytes, "Buffer overrun while trying to write to a software buffer.");

            unsafe {
                // get a pointer to the destination intptr
                byte* pSrc = (byte*)src.ToPointer();

                // copy the src data to the destination buffer
                for (int i = 0; i < length; i++) {
                    data[offset + i] = pSrc[offset + i];
                }
            }
        }

        /// <summary>
        ///		Allows direct access to the software buffer data in cases when it is known that the underlying
        ///		buffer is software and not hardware.
        /// </summary>
        public IntPtr GetDataPointer(int offset) {
            return Marshal.UnsafeAddrOfPinnedArrayElement(data, offset);
			//return handle.AddrOfPinnedObject();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class SoftwareIndexBuffer : HardwareIndexBuffer {
        #region Member variables

        /// <summary>
        ///     Holds the buffer data.
        /// </summary>
        protected byte[] data;
		
        #endregion

        #region Constructors

        /// <summary>
        ///		
        /// </summary>
        /// <remarks>
        ///		This is already in system memory, so no need to use a shadow buffer.
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="numIndices"></param>
        /// <param name="usage"></param>
        /// DOC
        public SoftwareIndexBuffer(IndexType type, int numIndices, BufferUsage usage)
            : base(type, numIndices, usage, true, false) {
            data = new byte[sizeInBytes];
        }

        #endregion

        #region Methods

        public override IntPtr Lock(int offset, int length, BufferLocking locking) {
            isLocked = true;

            // return the offset into the array as a pointer
            return Marshal.UnsafeAddrOfPinnedArrayElement(data, offset);
        }


        protected override IntPtr LockImpl(int offset, int length, BufferLocking locking) {
            isLocked = true;

            // return the offset into the array as a pointer
            return Marshal.UnsafeAddrOfPinnedArrayElement(data, offset);
        }

        public override void ReadData(int offset, int length, IntPtr dest) {
            Debug.Assert((offset + length) <= sizeInBytes, "Buffer overrun while trying to read a software buffer.");

            unsafe {
                // get a pointer to the destination intptr
                byte* pDest = (byte*)dest.ToPointer();

                // copy the src data to the destination buffer
                for (int i = 0; i < length; i++) {
                    pDest[offset + i] = data[offset + i];
                }
            }
        }

        public override void Unlock() {
            isLocked = false;
        }

        public override void UnlockImpl() {
            isLocked = false;
        }

        public override void WriteData(int offset, int length, IntPtr src, bool discardWholeBuffer) {
            Debug.Assert((offset + length) <= sizeInBytes, "Buffer overrun while trying to write to a software buffer.");

            unsafe {
                // get a pointer to the destination intptr
                byte* pSrc = (byte*)src.ToPointer();

                // copy the src data to the destination buffer
                for (int i = 0; i < length; i++) {
                    data[offset + i] = pSrc[offset + i];
                }
            }
        }

        /// <summary>
        ///		Allows direct access to the software buffer data in cases when it is known that the underlying
        ///		buffer is software and not hardware.
        /// </summary>
        public IntPtr GetDataPointer(int offset) {
            return Marshal.UnsafeAddrOfPinnedArrayElement(data, offset);
        }

        #endregion
    }
}
