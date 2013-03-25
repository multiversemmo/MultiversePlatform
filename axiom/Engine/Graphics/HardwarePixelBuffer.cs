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
using Axiom.Media;

namespace Axiom.Graphics {
    /// <summary>
    ///     Specialisation of HardwareBuffer for a pixel buffer. The
    ///     HardwarePixelbuffer abstracts an 1D, 2D or 3D quantity of pixels
    ///     stored by the rendering API. The buffer can be located on the card
    ///     or in main memory depending on its usage. One mipmap level of a
    ///     texture is an example of a HardwarePixelBuffer.
    /// </summary>
	public abstract class HardwarePixelBuffer : HardwareBuffer {

        #region Fields
		
		///<summary>
		///    Extents
        ///</summary>
		protected int width;
		protected int height;
		protected int depth;

        // 
		///<summary>
		///    Pitches (offsets between rows and slices)
        ///</summary>
        protected int rowPitch;
		protected int slicePitch;

		///<summary>
		///    Internal format
        ///</summary>
        protected Axiom.Media.PixelFormat format;

		///<summary>
		///    Currently locked region
        ///</summary>
        protected PixelBox currentLock;
		
		#endregion Fields

		#region Constructors

		///<summary>
		///    Should be called by HardwareBufferManager
        ///</summary>
        public HardwarePixelBuffer(int width, int height, int depth,
								   Axiom.Media.PixelFormat format, BufferUsage usage, 
								   bool useSystemMemory, bool useShadowBuffer) : 
			base(usage, useSystemMemory, useShadowBuffer) {
			this.width = width;
			this.height = height;
			this.depth = depth;
			this.format = format;
			// Default
			rowPitch = width;
			slicePitch = height * width;
            sizeInBytes = height * width * depth * PixelUtil.GetNumElemBytes(format);
		}
		
		#endregion Constructors

		#region Properties

		public int Width {
			get { return width; }
		}
		
		public int Height {
			get { return height; }
		}
			
		public int Depth {
			get { return depth; }
		}
		
		public int RowPitch {
			get { return rowPitch; }
		}
		
		public int SlicePitch {
			get { return slicePitch; }
		}
			
		public Axiom.Media.PixelFormat Format {
			get { return format; }
		}

		///<summary>
		///    Get the current locked region. This is the same value as returned
		///    by Lock(BasicBox, BufferLocking)
        ///<returns>PixelBox containing the locked region</returns>
		public PixelBox CurrentLock {
			get { 
				Debug.Assert(IsLocked, "Cannot get current lock: buffer not locked");
				return currentLock;
			}
		}
		
		#endregion Properties

		#region Abstract Methods

		///<summary>
		///    Internal implementation of lock(), must be overridden in subclasses
        ///</summary>
        public abstract PixelBox LockImpl(BasicBox lockBox,  BufferLocking options);

		///<summary>
		///    Copies a region from normal memory to a region of this pixelbuffer. The source
		///    image can be in any pixel format supported by Axiom, and in any size. 
        ///</summary>
        ///<param name="src">PixelBox containing the source pixels and format in memory</param>
        ///<param name="dstBox">Image.BasicBox describing the destination region in this buffer</param>
        ///<remarks>
		///    The source and destination regions dimensions don't have to match, in which
		///    case scaling is done. This scaling is generally done using a bilinear filter in hardware,
		///    but it is faster to pass the source image in the right dimensions.
		///    Only call this function when both  buffers are unlocked. 
		///</remarks>
		public abstract void BlitFromMemory(PixelBox src,  BasicBox dstBox);
		
		///<summary>
		///    Copies a region of this pixelbuffer to normal memory.
        ///</summary>
        ///<param name="srcBox">BasicBox describing the source region of this buffer</param>
        ///<param name="dst">PixelBox describing the destination pixels and format in memory</param>
        ///<remarks>
		///    The source and destination regions don't have to match, in which
		///    case scaling is done.
		///    Only call this function when the buffer is unlocked. 
		///</remarks>
		public abstract void BlitToMemory(BasicBox srcBox, PixelBox dst);

		#endregion Abstract Methods

		#region Methods

		///<summary>
		///    Lock the buffer for (potentially) reading / writing.
        ///</summary>
        ///<param name="lockBox">Region of the buffer to lock</param>
        ///<param name="options">Locking options</param>
        ///<returns>
		///    PixelBox containing the locked region, the pitches and
		///    the pixel format
		///</returns>
		public virtual PixelBox Lock(BasicBox lockBox, BufferLocking options) {
			if (useShadowBuffer) {
				if (options != BufferLocking.ReadOnly)
					// we have to assume a read / write lock so we use the shadow buffer
					// and tag for sync on unlock()
					shadowUpdated = true;
				currentLock = ((HardwarePixelBuffer)shadowBuffer).Lock(lockBox, options);
			}
			else {
                Debug.Assert(!isLocked);
				// Lock the real buffer if there is no shadow buffer 
				currentLock = LockImpl(lockBox, options);
				isLocked = true;
			}
			return currentLock;
		}
			
		///<summary>
		///    @copydoc HardwareBuffer.Lock
        ///</summary>
        //public virtual IntPtr Lock(int offset, int length, BasicBox lockBox, BufferLocking options) {
        //    Debug.Assert(!IsLocked, "Cannot lock this buffer, it is already locked!");
        //    Debug.Assert(offset == 0 && length == sizeInBytes, "Cannot lock memory region, must lock box or entire buffer");

        //    BasicBox myBox = new BasicBox(0, 0, 0, width, height, depth);
        //    PixelBox rv = Lock(myBox, options);
        //    return rv.Data;
        //}
				
		///<summary>
		///    Internal implementation of lock(), do not override or call this
		///    for HardwarePixelBuffer implementations, but override the previous method
        ///</summary>
        protected override IntPtr LockImpl(int offset, int length, BufferLocking options) {
            throw new NotImplementedException("HardwarePixelBuffer does not support this variant of LockImpl");
        }

		///<summary>
		///    Copies a box from another PixelBuffer to a region of the 
		///    this PixelBuffer. 
        ///</summary>
        ///<param name="src">Source/dest pixel buffer</param>
        ///<param name="srcBox">Image.BasicBox describing the source region in this buffer</param>
        ///<param name="dstBox">Image.BasicBox describing the destination region in this buffer</param>
        ///<remarks>
		///    The source and destination regions dimensions don't have to match, in which
		///    case scaling is done. This scaling is generally done using a bilinear filter in hardware,
		///    but it is faster to pass the source image in the right dimensions.
		///    Only call this function when both  buffers are unlocked. 
		///</remarks>
		public virtual void Blit(HardwarePixelBuffer src, BasicBox srcBox, BasicBox dstBox) {
			if(IsLocked || src.IsLocked)
				throw new Exception("Source and destination buffer may not be locked!  In HardwarePixelBuffer.Blit");
			if(src == this)
				throw new Exception("Source must not be the same object, in HardwarePixelBuffer.Blit");

			PixelBox srclock = src.Lock(srcBox, BufferLocking.ReadOnly);

			BufferLocking method = BufferLocking.Normal;
			if(dstBox.Left == 0 && dstBox.Top == 0 && dstBox.Front == 0 &&
			   dstBox.Right == width && dstBox.Bottom == height &&
			   dstBox.Back == depth)
				// Entire buffer -- we can discard the previous contents
				method = BufferLocking.Discard;

			PixelBox dstlock = Lock(dstBox, method);
			if(dstlock.Width != srclock.Width || dstlock.Height != srclock.Height || dstlock.Depth != srclock.Depth)
				// Scaling desired
				throw new Exception("Image scaling not yet implemented; in HardwarePixelBuffer.Blit");
				// Image.Scale(srclock, dstlock);
			else
				// No scaling needed
				PixelUtil.BulkPixelConversion(srclock, dstlock);

			Unlock();
			src.Unlock();
		}

		///<summary>
		///    Notify TextureBuffer of destruction of render target.
		///    Called by RenderTexture when destroyed.
        ///</summary>
		public virtual void ClearSliceRTT(int zoffset) {
			// Do nothing; derived classes may override
		}

		///<summary>
		///    @copydoc HardwareBuffer::readData
        ///</summary>
		public override void ReadData(int offset, int length, IntPtr dest) {
			throw new Exception("Reading a byte range is not implemented. Use blitToMemory; in " +
								"HardwarePixelBuffer.ReadData");
		}
			
		///<summary>
		///    @copydoc HardwareBuffer::writeData
        ///</summary>
		public override void WriteData(int offset, int length, IntPtr source,
									  bool discardWholeBuffer) {
			throw new Exception("Writing a byte range is not implemented. Use blitToMemory; in " +
								"HardwarePixelBuffer.WriteData");
		}

		///<summary>
		///    Convience function that blits the entire source pixel buffer to this buffer. 
		///    If source and destination dimensions don't match, scaling is done.
        ///</summary>
        ///<param name="src">PixelBox containing the source pixels and format in memory</param>
        ///<remarks>
		///    Only call this function when the buffer is unlocked. 
		///</remarks>
		public void Blit(HardwarePixelBuffer src) {
			Blit(src, 
				 new BasicBox(0, 0, 0, src.Width, src.Height, src.Depth), 
				 new BasicBox(0, 0, 0, width, height, depth));
		}
		
		///<summary>
		///    Convenience function that blits a pixelbox from memory to the entire 
		///    buffer. The source image is scaled as needed.
        ///</summary>
        ///<param name="src">PixelBox containing the source pixels and format in memory</param>
        ///<remarks>
		///    Only call this function when the buffer is unlocked. 
		///</remarks>
		public void BlitFromMemory(PixelBox src) {
			BlitFromMemory(src, new BasicBox(0, 0, 0, width, height, depth));
		}
		

		///<summary>
		///    Convenience function that blits this entire buffer to a pixelbox.
		///    The image is scaled as needed.
        ///</summary>
        ///<param name="src">PixelBox containing the source pixels and format in memory</param>
        ///<remarks>
		///    Only call this function when the buffer is unlocked. 
		///</remarks>
		public void BlitToMemory(PixelBox dst) {
			BlitToMemory(new BasicBox(0, 0, 0, width, height, depth), dst);
		}
        
		///<summary>
		///    Get a render target for this PixelBuffer, or a slice of it. The texture this
		///    was acquired from must have TextureUsage.RenderTarget set, otherwise it is possible to
		///    render to it and this method will throw an exception.
        ///</summary>
        ///<param name="slice">Which slice</param>
        ///<returns>
		///    A pointer to the render target. This pointer has the lifespan of this PixelBuffer.
		///</returns>
		public virtual RenderTexture GetRenderTarget(int slice) {
			throw new Exception("Not yet implemented for this rendersystem; in " +
								"HardwarePixelBuffer.GetRenderTarget");
		}

		public virtual RenderTexture GetRenderTarget() {
			return GetRenderTarget(0);
		}
		
		#endregion Methods
		
	}
}
