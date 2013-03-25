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
using Axiom.Core;

namespace Axiom.Graphics {
    /// <summary>
    ///     Abstract class defining common features of hardware buffers.
    /// </summary>
    /// <remarks>
    ///     A 'hardware buffer' is any area of memory held outside of core system ram,
    ///     and in our case refers mostly to video ram, although in theory this class
    ///     could be used with other memory areas such as sound card memory, custom
    ///     coprocessor memory etc.
    ///     <p/>
    ///     This reflects the fact that memory held outside of main system RAM must 
    ///     be interacted with in a more formal fashion in order to promote
    ///     cooperative and optimal usage of the buffers between the various 
    ///     processing units which manipulate them.
    ///     <p/>
    ///     This abstract class defines the core interface which is common to all
    ///     buffers, whether it be vertex buffers, index buffers, texture memory
    ///     or framebuffer memory etc.
    ///     <p/>
    ///     Buffers have the ability to be 'shadowed' in system memory, this is because
    ///     the kinds of access allowed on hardware buffers is not always as flexible as
    ///     that allowed for areas of system memory - for example it is often either 
    ///     impossible, or extremely undesirable from a performance standpoint to read from
    ///     a hardware buffer; when writing to hardware buffers, you should also write every
    ///     byte and do it sequentially. In situations where this is too restrictive, 
    ///     it is possible to create a hardware, write-only buffer (the most efficient kind) 
    ///     and to back it with a system memory 'shadow' copy which can be read and updated arbitrarily.
    ///     Axiom handles synchronizing this buffer with the real hardware buffer (which should still be
    ///     created with the <see cref="BufferUsage.Dynamic"/> flag if you intend to update it very frequently). 
    ///     Whilst this approach does have it's own costs, such as increased memory overhead, these costs can 
    ///     often be outweighed by the performance benefits of using a more hardware efficient buffer.
    ///     You should look for the 'useShadowBuffer' parameter on the creation methods used to create
    ///     the buffer of the type you require (see <see cref="HardwareBufferManager"/>) to enable this feature.
    ///     <seealso cref="HardwareBufferManager"/>
    /// </remarks>
    public abstract class HardwareBuffer : IDisposable {
        #region Fields
		
        /// <summary>
        ///     Total size (in bytes) of the buffer.
        /// </summary>
        protected int sizeInBytes;
        /// <summary>
        ///     Usage type for this buffer.
        /// </summary>
        protected BufferUsage usage;
        /// <summary>
        ///     Is this buffer currently locked?
        /// </summary>
        protected bool isLocked;
        /// <summary>
        ///     Byte offset into the buffer where the current lock is held.
        /// </summary>
        protected int lockStart;
        /// <summary>
        ///     Total size (int bytes) of locked buffer data.
        /// </summary>
        protected int lockSize;
        /// <summary>
        ///     
        /// </summary>
        protected bool useSystemMemory;
        /// <summary>
        ///     Does this buffer have a shadow buffer?
        /// </summary>
        protected bool useShadowBuffer;
        /// <summary>
        ///     Reference to the sys memory shadow buffer tied to this hardware buffer.
        /// </summary>
        protected HardwareBuffer shadowBuffer;
        /// <summary>
        ///     Flag indicating whether the shadow buffer (if it exists) has been updated.
        /// </summary>
        protected bool shadowUpdated;
        /// <summary>
        ///     Flag indicating whether hardware updates from shadow buffer should be supressed.
        /// </summary>
        protected bool suppressHardwareUpdate;
		/// <summary>
		///		Unique id for this buffer.
		/// </summary>
		public int ID;
		protected static int nextID;
		
        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="usage">Usage type.</param>
        /// <param name="useSystemMemory"></param>
        /// <param name="useShadowBuffer">Use a software shadow buffer?</param>
        internal HardwareBuffer(BufferUsage usage, bool useSystemMemory, bool useShadowBuffer) {
            this.usage = usage;
            this.useSystemMemory = useSystemMemory;
            this.useShadowBuffer = useShadowBuffer;
			this.shadowBuffer = null;
			this.shadowUpdated = false;
			this.suppressHardwareUpdate = false;
			ID = nextID++;
			if (useShadowBuffer && usage == BufferUsage.Dynamic)
				usage = BufferUsage.DynamicWriteOnly;
			else if (useShadowBuffer && usage == BufferUsage.Static)
				usage = BufferUsage.StaticWriteOnly;
		}

        #endregion
			
        #region Methods

        public BufferStream LockStream(BufferLocking locking) {
            return new BufferStream(this, Lock(locking));
        }

        /// <summary>
        ///		Convenient overload to allow locking the entire buffer with only having
        ///		to supply the locking type.
        /// </summary>
        /// <param name="locking">Locking options.</param>
        /// <returns>IntPtr to the beginning of the locked region of buffer memory.</returns>
        public IntPtr Lock(BufferLocking locking) {
            return Lock(0, sizeInBytes, locking);
        }

        /// <summary>
        ///		Used to lock a vertex buffer in hardware memory in order to make modifications.
        /// </summary>
        /// <param name="offset">Starting index in the buffer to lock.</param>
        /// <param name="length">Nunber of bytes to lock after the offset.</param>
        /// <param name="locking">Specifies how to lock the buffer.</param>
        /// <returns>An array of the <code>System.Type</code> associated with this VertexBuffer.</returns>
        public virtual IntPtr Lock(int offset, int length, BufferLocking locking) {
            Debug.Assert(!isLocked, "Cannot lock this buffer because it is already locked.");

            IntPtr data = IntPtr.Zero;

            if(useShadowBuffer) {
                if(locking != BufferLocking.ReadOnly) {
                    // we have to assume a read / write lock so we use the shadow buffer
                    // and tag for sync on Unlock()
                    shadowUpdated = true;
                }

                data = shadowBuffer.Lock(offset, length, locking);
            }
            else {
                // lock the real deal and flag it as locked
                data = this.LockImpl(offset, length, locking);
                isLocked = true;
            }

            lockStart = offset;
            lockSize = length;

            return data;
        }

        /// <summary>
        ///     Internal implementation of Lock, which will be overridden by subclasses to provide
        ///     the core locking functionality.
        /// </summary>
        /// <param name="offset">Offset into the buffer (in bytes) to lock.</param>
        /// <param name="length">Length of the portion of the buffer (int bytes) to lock.</param>
        /// <param name="locking">Locking type.</param>
        /// <returns>IntPtr to the beginning of the locked portion of the buffer.</returns>
        protected abstract IntPtr LockImpl(int offset, int length, BufferLocking locking);

        /// <summary>
        ///		Must be called after a call to <code>Lock</code>.  Unlocks the vertex buffer in the hardware
        ///		memory.
        /// </summary>
        public virtual void Unlock() {
            Debug.Assert(this.IsLocked, "Cannot unlock this buffer if it isn't locked to begin with.");

            if(useShadowBuffer && shadowBuffer.IsLocked) {
                shadowBuffer.Unlock();

                // potentially update the real buffer from the shadow buffer
                UpdateFromShadow();
            }
            else {
                // unlock the real deal
                this.UnlockImpl();

                isLocked = false;
            }
        }

        /// <summary>
        ///     Abstract implementation of <see cref="Unlock"/>.
        /// </summary>
        public abstract void UnlockImpl();

        /// <summary>
        ///     Updates the real buffer from the shadow buffer, if required.
        /// </summary>
        protected void UpdateFromShadow() {
            if(useShadowBuffer && shadowUpdated && !suppressHardwareUpdate) {
                // do this manually to avoid locking problems
                IntPtr src = shadowBuffer.LockImpl(lockStart, lockSize, BufferLocking.ReadOnly);

                // Lock with discard if the whole buffer was locked, otherwise normal
                BufferLocking locking = 
                    (lockStart == 0 && lockSize == sizeInBytes) ? BufferLocking.Discard : BufferLocking.Normal;

                IntPtr dest = this.LockImpl(lockStart, lockSize, locking);

                // copy the data in directly
                Memory.Copy(src, dest, lockSize);

                // unlock both buffers to commit the write
                this.UnlockImpl();
                shadowBuffer.UnlockImpl();

                shadowUpdated = false;
            }
        }

        /// <summary>
        ///     Reads data from the buffer and places it in the memory pointed to by 'dest'.
        /// </summary>
        /// <param name="offset">The byte offset from the start of the buffer to read.</param>
        /// <param name="length">The size of the area to read, in bytes.</param>
        /// <param name="dest">
        ///     The area of memory in which to place the data, must be large enough to 
        ///     accommodate the data!
        /// </param>
        public abstract void ReadData(int offset, int length, IntPtr dest);

        /// <summary>
        ///     Writes data to the buffer from an area of system memory; note that you must
        ///     ensure that your buffer is big enough.
        /// </summary>
        /// <param name="offset">The byte offset from the start of the buffer to start writing.</param>
        /// <param name="length">The size of the data to write to, in bytes.</param>
        /// <param name="src">The source of the data to be written.</param>
        public void WriteData(int offset, int length, IntPtr src) {
            WriteData(offset, length, src, false);
        }

        /// <summary>
        ///     Writes data to the buffer from an area of system memory; note that you must
        ///     ensure that your buffer is big enough.
        /// </summary>
        /// <param name="offset">The byte offset from the start of the buffer to start writing.</param>
        /// <param name="length">The size of the data to write to, in bytes.</param>
        /// <param name="src">The source of the data to be written.</param>
        /// <param name="discardWholeBuffer">
        ///     If true, this allows the driver to discard the entire buffer when writing,
        ///     such that DMA stalls can be avoided; use if you can.
        /// </param>
        public abstract void WriteData(int offset, int length, IntPtr src, bool discardWholeBuffer);

        /// <summary>
        ///    Allows passing in a managed array of data to fill the vertex buffer.
        /// </summary>
        /// <param name="offset">The byte offset from the start of the buffer to start writing.</param>
        /// <param name="length">The size of the data to write to, in bytes.</param>
        /// <param name="data">
        ///     Array of data to blast into the buffer.  This can be an array of custom structs, that hold
        ///     position, normal, etc data.  The size of the struct *must* match the vertex size of the buffer,
        ///     so use with care.
        /// </param>
        public void WriteData(int offset, int length, System.Array data) {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataPtr = handle.AddrOfPinnedObject();
            // IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
            WriteData(offset, length, dataPtr);
            handle.Free();
        }

        /// <summary>
        ///    Allows passing in a managed array of data to fill the vertex buffer.
        /// </summary>
        /// <param name="offset">The byte offset from the start of the buffer to start writing.</param>
        /// <param name="length">The size of the data to write to, in bytes.</param>
        /// <param name="data">
        ///     Array of data to blast into the buffer.  This can be an array of custom structs, that hold
        ///     position, normal, etc data.  The size of the struct *must* match the vertex size of the buffer,
        ///     so use with care.
        /// </param>
        /// <param name="discardWholeBuffer">
        ///     If true, this allows the driver to discard the entire buffer when writing,
        ///     such that DMA stalls can be avoided; use if you can.
        /// </param>
        public void WriteData(int offset, int length, System.Array data, bool discardWholeBuffer) {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataPtr = handle.AddrOfPinnedObject();
            // IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
            WriteData(offset, length, dataPtr, discardWholeBuffer);
            handle.Free();
        }

        /// <summary>
        ///     Copy data from another buffer into this one.
        /// </summary>
        /// <param name="srcBuffer">The buffer from which to read the copied data.</param>
        /// <param name="srcOffset">Offset in the source buffer at which to start reading.</param>
        /// <param name="destOffset">Offset in the destination buffer to start writing.</param>
        /// <param name="length">Length of the data to copy, in bytes.</param>
        public virtual void CopyData(HardwareBuffer srcBuffer, int srcOffset, int destOffset, int length) {
            // call the overloaded method
            CopyData(srcBuffer, srcOffset, destOffset, length, false);
        }

        /// <summary>
        ///     Copy data from another buffer into this one.
        /// </summary>
        /// <param name="srcBuffer">The buffer from which to read the copied data.</param>
        /// <param name="srcOffset">Offset in the source buffer at which to start reading.</param>
        /// <param name="destOffset">Offset in the destination buffer to start writing.</param>
        /// <param name="length">Length of the data to copy, in bytes.</param>
        /// <param name="discardWholeBuffer">If true, will discard the entire contents of this buffer before copying.</param>
        public virtual void CopyData(HardwareBuffer srcBuffer, int srcOffset, int destOffset, int length, bool discardWholeBuffer) {
            // lock the source buffer
            IntPtr srcData = srcBuffer.Lock(srcOffset, length, BufferLocking.ReadOnly);

            // write the data to this buffer
            this.WriteData(destOffset, length, srcData, discardWholeBuffer);

            // unlock the source buffer
            srcBuffer.Unlock();
        }

        /// <summary>
        ///     Pass true to suppress hardware upload of shadow buffer changes.
        /// </summary>
        /// <param name="suppress">If true, shadow buffer updates won't be uploaded to hardware.</param>
        public void SuppressHardwareUpdate(bool suppress) {
            suppressHardwareUpdate = suppress;

			// if disabling future shadow updates, then update from what is current in the buffer now
			// this is needed for shadow volumes
			if(!suppress) {
				UpdateFromShadow();
			}
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Gets whether or not this buffer is currently locked.
        /// </summary>
        public bool IsLocked { 
            get { 
                return isLocked || (useShadowBuffer && shadowBuffer.IsLocked); 
            } 
        }

        /// <summary>
        ///		Gets whether this buffer is held in system memory.
        /// </summary>
        public bool IsSystemMemory {
            get { 
                return useSystemMemory; 
            }
        }

        /// <summary>
        ///		Gets the size (in bytes) for this buffer.
        /// </summary>
        public int Size { 
            get { 
                return sizeInBytes; 
            } 
        }

        /// <summary>
        ///		Gets the usage of this buffer.
        /// </summary>
        public BufferUsage Usage { 
            get { 
                return usage; 
            } 
        }

        /// <summary>
        ///     Gets a bool that specifies whether this buffer has a software shadow buffer.
        /// </summary>
        public bool HasShadowBuffer {
            get {
                return useShadowBuffer;
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Called to destroy resources used by this hardware buffer.
        /// </summary>
        public virtual void Dispose() {
			// do nothing by default
        }

        #endregion IDisposable Implementation
    }
}
