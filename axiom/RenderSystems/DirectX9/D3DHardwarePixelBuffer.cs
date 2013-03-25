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
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;
using Axiom.Graphics;
using Axiom.Media;
using VertexDeclaration = Axiom.Graphics.VertexDeclaration;
using Root = Axiom.Core.Root;
using System.Runtime.InteropServices;
using Axiom.Utility;

namespace Axiom.RenderSystems.DirectX9 {
    /// <summary>
    /// 	DirectX implementation of HardwarePixelBuffer
    /// </summary>
    public class D3DHardwarePixelBuffer : HardwarePixelBuffer {
		
		#region Fields

        protected static TimingMeter timingMeter = MeterManager.GetMeter("BlitFromMemory", "D3DHardwarePixelBuffer");

		///<summary>
		///    D3DDevice pointer
		///</summary>
        protected D3D.Device device;
		///<summary>
		///    Surface abstracted by this buffer
		///</summary>
		protected D3D.Surface surface;
		///<summary>
		///    Volume abstracted by this buffer
		///</summary>
		protected D3D.Volume volume;
		///<summary>
		///    Temporary surface in main memory if direct locking of mSurface is not possible
		///</summary>
		protected D3D.Surface tempSurface;
		///<summary>
		///    Temporary volume in main memory if direct locking of mVolume is not possible
		///</summary>
		protected D3D.Volume tempVolume;
		///<summary>
		///    Doing Mipmapping?
		///</summary>
		protected bool doMipmapGen;
		///<summary>
		///    Hardware Mipmaps?
		///</summary>
		protected bool HWMipmaps;
		///<summary>
		///    The Mipmap texture?
		///</summary>
		protected D3D.BaseTexture mipTex;
		///<summary>
		///    Render targets
		///</summary>
        protected List<RenderTexture> sliceTRT;

		#endregion Fields

		#region Constructors

		public D3DHardwarePixelBuffer(BufferUsage usage) :
			base(0, 0, 0, Axiom.Media.PixelFormat.Unknown, usage, false, false) {
			device = null;
			surface = null;
			volume = null;
			tempSurface = null;
			tempVolume = null;
			doMipmapGen = false;
			HWMipmaps = false;
			mipTex = null;
            sliceTRT = new List<RenderTexture>();
		}
			
		#endregion Constructors

		#region Properties

		///<summary>
		///    Accessor for surface
		///</summary>
		public D3D.Surface Surface {
			get { return surface; }
		}

		#endregion Properties

		#region Methods

		///<summary>
		///    Call this to associate a D3D surface with this pixel buffer
		///</summary>
		public void Bind(D3D.Device device, D3D.Surface surface, bool update) {
			this.device = device;
            this.surface = surface;

			D3D.SurfaceDescription desc = surface.Description;
			width = desc.Width;
			height = desc.Height;
			depth = 1;
            format = D3DHelper.ConvertEnum(desc.Format);
			// Default
			rowPitch = width;
			slicePitch = height * width;
            sizeInBytes = PixelUtil.GetMemorySize(width, height, depth, format);

			if (((int)usage & (int)TextureUsage.RenderTarget) != 0)
				CreateRenderTextures(update);
		}
		
		///<summary>
		///    Call this to associate a D3D volume with this pixel buffer
		///</summary>
		public void Bind(D3D.Device device, D3D.Volume volume, bool update) {
			this.device = device;
			this.volume = volume;

			D3D.VolumeDescription desc = volume.Description;
			width = desc.Width;
			height = desc.Height;
			depth = desc.Depth;
            format = D3DHelper.ConvertEnum(desc.Format);
			// Default
			rowPitch = width;
			slicePitch = height * width;
            sizeInBytes = PixelUtil.GetMemorySize(width, height, depth, format);

            if (((int)usage & (int)TextureUsage.RenderTarget) != 0)
                CreateRenderTextures(update);
		}

		///<summary>
		///    Util functions to convert a D3D locked rectangle to a pixel box
		///</summary>
		protected static void FromD3DLock(PixelBox rval, int pitch, GraphicsStream stream) {
            rval.RowPitch = pitch / PixelUtil.GetNumElemBytes(rval.Format);
			rval.SlicePitch = rval.RowPitch * rval.Height;
            Debug.Assert((pitch % PixelUtil.GetNumElemBytes(rval.Format)) == 0);
            rval.Data = stream.InternalData;
		}

		///<summary>
		///    Util functions to convert a D3D LockedBox to a pixel box
		///</summary>
		protected static void FromD3DLock(PixelBox rval, D3D.LockedBox lbox, GraphicsStream stream)
		{
            rval.RowPitch = lbox.RowPitch / PixelUtil.GetNumElemBytes(rval.Format);
            rval.SlicePitch = lbox.SlicePitch / PixelUtil.GetNumElemBytes(rval.Format);
            Debug.Assert((lbox.RowPitch % PixelUtil.GetNumElemBytes(rval.Format)) == 0);
            Debug.Assert((lbox.SlicePitch % PixelUtil.GetNumElemBytes(rval.Format)) == 0);
			rval.Data = stream.InternalData;
		}

		///<summary>
		///    Convert Ogre integer Box to D3D rectangle
		///</summary>
		protected static Rectangle ToD3DRectangle(BasicBox lockBox) {
			Debug.Assert(lockBox.Depth == 1);
			Rectangle r = new Rectangle();
			r.X = lockBox.Left;
			r.Width = lockBox.Width;
			r.Y = lockBox.Top;
            r.Height = lockBox.Height;
			return r;
		}

		///<summary>
		///    Convert Axiom Box to D3D box
		///</summary>
		protected static D3D.Box ToD3DBox(BasicBox lockBox) {
			D3D.Box pbox = new D3D.Box();
			pbox.Left = lockBox.Left;
			pbox.Right = lockBox.Right;
			pbox.Top = lockBox.Top;
			pbox.Bottom = lockBox.Bottom;
			pbox.Front = lockBox.Front;
			pbox.Back = lockBox.Back;
			return pbox;
		}

		///<summary>
		///    Convert Axiom PixelBox extent to D3D rectangle
		///</summary>
		protected static Rectangle ToD3DRectangleExtent(PixelBox lockBox) {
			Debug.Assert(lockBox.Depth == 1);
			Rectangle r = new Rectangle();
			r.X = 0;
			r.Width = lockBox.Width;
			r.X = 0;
			r.Height = lockBox.Height;
			return r;
		}

		///<summary>
		///    Convert Axiom PixelBox extent to D3D box
		///</summary>
		protected static D3D.Box ToD3DBoxExtent(PixelBox lockBox) {
			D3D.Box pbox = new D3D.Box();
			pbox.Left = 0;
			pbox.Right = lockBox.Width;
			pbox.Top = 0;
			pbox.Bottom = lockBox.Height;
			pbox.Front = 0;
			pbox.Back = lockBox.Depth;
			return pbox;
		}

		///<summary>
		///    Lock a box
		///</summary>
		public override PixelBox LockImpl(BasicBox lockBox,  BufferLocking options) {
			// Check for misuse
            if (((int)usage & (int)TextureUsage.RenderTarget) != 0)
				throw new Exception("DirectX does not allow locking of or directly writing to RenderTargets. Use BlitFromMemory if you need the contents; " +
									"in D3D9HardwarePixelBuffer.LockImpl");	
			// Set extents and format
			PixelBox rval = new PixelBox(lockBox, format);
			// Set locking flags according to options
			D3D.LockFlags flags = D3D.LockFlags.None;
			switch(options) {
			case BufferLocking.Discard:
				// D3D only likes D3D.LockFlags.Discard if you created the texture with D3DUSAGE_DYNAMIC
				// debug runtime flags this up, could cause problems on some drivers
				if ((usage & BufferUsage.Dynamic) != 0)
					flags |= D3D.LockFlags.Discard;
				break;
			case BufferLocking.ReadOnly:
				flags |= D3D.LockFlags.ReadOnly;
				break;
			default: 
				break;
			}

			if (surface != null) {
				// Surface
                GraphicsStream data = null;
                int pitch;
        		if (lockBox.Left == 0 && lockBox.Top == 0  &&
		        	lockBox.Right == width && lockBox.Bottom == height) {
        			// Lock whole surface
                    data = surface.LockRectangle(flags, out pitch);
		        } else {
			        Rectangle prect = ToD3DRectangle(lockBox); // specify range to lock
                    data = surface.LockRectangle(prect, flags, out pitch);
		        }
                if (data == null)		
					throw new Exception("Surface locking failed; in D3D9HardwarePixelBuffer.LockImpl");
                FromD3DLock(rval, pitch, data);
			} else {
				// Volume
				D3D.Box pbox = ToD3DBox(lockBox); // specify range to lock
				D3D.LockedBox lbox; // Filled in by D3D

				GraphicsStream data = volume.LockBox(pbox, flags, out lbox);
				FromD3DLock(rval, lbox, data);
			}
			return rval;
		}

		///<summary>
		///    Unlock a box
		///</summary>
		public override void UnlockImpl() {
			if (surface != null) 
				// Surface
				surface.UnlockRectangle();
			else 
				// Volume
				volume.UnlockBox();

			if (doMipmapGen)
				GenMipmaps();
		}
			
		///<summary>
		///    Create (or update) render textures for slices
		///</summary>
        ///<param name="update">are we updating an existing texture</param>
		protected void CreateRenderTextures(bool update) {
			if (update) {
				Debug.Assert(sliceTRT.Count == depth);
				foreach (D3DRenderTexture trt in sliceTRT)
					trt.Rebind(this);
				return;
			}

			DestroyRenderTextures();
			if(surface == null)
				throw new Exception("Rendering to 3D slices not supported yet for Direct3D; in " +
									"D3D9HardwarePixelBuffer.CreateRenderTexture");
			// Create render target for each slice
			sliceTRT.Clear();
			Debug.Assert(depth==1);
			for(int zoffset=0; zoffset<depth; ++zoffset) {
                string name = "rtt/" + this.ID;
				RenderTexture trt = new D3DRenderTexture(name, this);
				sliceTRT.Add(trt);
				Root.Instance.RenderSystem.AttachRenderTarget(trt);
			}
		}

		///<summary>
		///    Destroy render textures for slices
		///</summary>
		protected void DestroyRenderTextures() {
			if(sliceTRT.Count == 0)
				return;
			// Delete all render targets that are not yet deleted via _clearSliceRTT
            for (int i = 0; i < sliceTRT.Count; ++i) {
                RenderTexture trt = sliceTRT[i];
                if (trt != null)
                    Root.Instance.RenderSystem.DestroyRenderTarget(trt.Name);
            }
			// sliceTRT.Clear();
		}

		///<summary>
		///    @copydoc HardwarePixelBuffer.Blit
		///</summary>
		public override void Blit(HardwarePixelBuffer _src, BasicBox srcBox, BasicBox dstBox) {
            D3DHardwarePixelBuffer src = (D3DHardwarePixelBuffer)_src;
			if (surface != null && src.surface != null) {
				// Surface-to-surface
				Rectangle dsrcRect = ToD3DRectangle(srcBox);
				Rectangle ddestRect = ToD3DRectangle(dstBox);
				// D3DXLoadSurfaceFromSurface
				SurfaceLoader.FromSurface(surface, ddestRect, src.surface, dsrcRect, Filter.None, 0);
			} else if (volume != null && src.volume != null) {
				// Volume-to-volume
                Box dsrcBox = ToD3DBox(srcBox);
				Box ddestBox = ToD3DBox(dstBox);
				// D3DXLoadVolumeFromVolume
                VolumeLoader.FromVolume(volume, ddestBox, src.volume, dsrcBox, Filter.None, 0);
			} else
				// Software fallback   
				base.Blit(_src, srcBox, dstBox);
		}

		///<summary>
		///    @copydoc HardwarePixelBuffer.BlitFromMemory
		///</summary>
		public override void BlitFromMemory(PixelBox src, BasicBox dstBox) {
            using (AutoTimer timer = new AutoTimer(timingMeter)) {
                BlitFromMemoryImpl(src, dstBox);
            }
        }

        protected void BlitFromMemoryImpl(PixelBox src, BasicBox dstBox) {
            // TODO: This currently does way too many copies.  We copy
            // from src to a converted buffer (if needed), then from 
            // converted to a byte array, then into the temporary surface,
            // and finally from the temporary surface to the real surface.
			PixelBox converted = src;
            IntPtr bufPtr = IntPtr.Zero;
            GCHandle bufGCHandle = new GCHandle();
			// convert to pixelbuffer's native format if necessary
            if (D3DHelper.ConvertEnum(src.Format) == D3D.Format.Unknown) {
                int bufSize = PixelUtil.GetMemorySize(src.Width, src.Height, src.Depth, format);
                byte[] newBuffer = new byte[bufSize];
                bufGCHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);
                bufPtr = bufGCHandle.AddrOfPinnedObject();
				converted = new PixelBox(src.Width, src.Height, src.Depth, format, bufPtr);
				PixelUtil.BulkPixelConversion(src, converted);
			}

            // int formatBytes = PixelUtil.GetNumElemBytes(converted.Format);
            Surface tmpSurface = device.CreateOffscreenPlainSurface(converted.Width, converted.Height, D3DHelper.ConvertEnum(converted.Format), Pool.Scratch);
            int pitch;
            // Ideally I would be using the Array mechanism here, but that doesn't seem to work
            GraphicsStream buf = tmpSurface.LockRectangle(LockFlags.NoSystemLock, out pitch);
            buf.Position = 0;
            unsafe {
                int bufSize = PixelUtil.GetMemorySize(converted.Width, converted.Height, converted.Depth, converted.Format);
                byte* srcPtr = (byte*)converted.Data.ToPointer();
                byte[] ugh = new byte[bufSize];
                for (int i = 0; i < bufSize; ++i)
                    ugh[i] = srcPtr[i];
                buf.Write(ugh);
            }
            tmpSurface.UnlockRectangle();
            buf.Dispose();

            //ImageInformation imageInfo = new ImageInformation();
            //imageInfo.Format = D3DHelper.ConvertEnum(converted.Format);
            //imageInfo.Width = converted.Width;
            //imageInfo.Height = converted.Height;
            //imageInfo.Depth = converted.Depth;
            if (surface != null) {
                // I'm trying to write to surface using the data in converted
                Rectangle srcRect = ToD3DRectangleExtent(converted);
				Rectangle destRect = ToD3DRectangle(dstBox);
                SurfaceLoader.FromSurface(surface, destRect, tmpSurface, srcRect, Filter.None, 0);
            } else {
				D3D.Box srcBox = ToD3DBoxExtent(converted);
				D3D.Box destBox = ToD3DBox(dstBox);
                Debug.Assert(false, "Volume textures not yet supported");
                // VolumeLoader.FromStream(volume, destBox, converted.Data, converted.RowPitch * converted.SlicePitch * formatBytes, srcBox, Filter.None, 0);
                VolumeLoader.FromStream(volume, destBox, buf, srcBox, Filter.None, 0);
            }

            tmpSurface.Dispose();

            // If we allocated a buffer for the temporary conversion, free it here
            // If I used bufPtr to store my temporary data while I converted 
            // it, I need to free it here.  This invalidates converted.
            // My data has already been copied to tmpSurface and then to the 
            // real surface.
            if (bufGCHandle.IsAllocated)
                bufGCHandle.Free();

            if (doMipmapGen)
				GenMipmaps();
        }

		///<summary>
		///    @copydoc HardwarePixelBuffer.BlitToMemory
		///</summary>
		public override void BlitToMemory(BasicBox srcBox, PixelBox dst) {
			// Decide on pixel format of temp surface
			PixelFormat tmpFormat = format;
            if (D3DHelper.ConvertEnum(dst.Format) == D3D.Format.Unknown)
				tmpFormat = dst.Format;
			if (surface != null) {
				Debug.Assert(srcBox.Depth == 1 && dst.Depth == 1);
				// Create temp texture
                D3D.Texture tmp = 
                    new D3D.Texture(device, dst.Width, dst.Height,
                                    1, // 1 mip level ie topmost, generate no mipmaps
                                    0, D3DHelper.ConvertEnum(tmpFormat), 
                                    Pool.Scratch);
                D3D.Surface subSurface = tmp.GetSurfaceLevel(0);
				// Copy texture to this temp surface
				Rectangle destRect, srcRect;
				srcRect = ToD3DRectangle(srcBox);
				destRect = ToD3DRectangleExtent(dst);

                SurfaceLoader.FromSurface(subSurface, destRect, surface, srcRect, Filter.None, 0);

                // Lock temp surface and copy it to memory
                int pitch; // Filled in by D3D
                GraphicsStream data = subSurface.LockRectangle(D3D.LockFlags.ReadOnly, out pitch);
				// Copy it
				PixelBox locked = new PixelBox(dst.Width, dst.Height, dst.Depth, tmpFormat);
				FromD3DLock(locked, pitch, data);
				PixelUtil.BulkPixelConversion(locked, dst);
                subSurface.UnlockRectangle();
				// Release temporary surface and texture
				subSurface.Dispose();
				tmp.Dispose();
			}
			else {
				// Create temp texture
                D3D.VolumeTexture tmp =
                    new D3D.VolumeTexture(device, dst.Width, dst.Height, dst.Depth,
                                          0, D3D.Usage.None,
                                          D3DHelper.ConvertEnum(tmpFormat), 
                                          Pool.Scratch);
                D3D.Volume subVolume = tmp.GetVolumeLevel(0);
				// Volume
				D3D.Box ddestBox = ToD3DBoxExtent(dst);
				D3D.Box dsrcBox = ToD3DBox(srcBox);

                VolumeLoader.FromVolume(subVolume, ddestBox, volume, dsrcBox, Filter.None, 0);
				// Lock temp surface and copy it to memory
				D3D.LockedBox lbox; // Filled in by D3D
                GraphicsStream data = subVolume.LockBox(LockFlags.ReadOnly, out lbox);
				// Copy it
				PixelBox locked = new PixelBox(dst.Width, dst.Height, dst.Depth, tmpFormat);
				FromD3DLock(locked, lbox, data);
				PixelUtil.BulkPixelConversion(locked, dst);
                subVolume.UnlockBox();
				// Release temporary surface and texture
                subVolume.Dispose();
                tmp.Dispose();
			}
		}

		///<summary>
		///    Internal function to update mipmaps on update of level 0
		///</summary>
		public void GenMipmaps() {
			Debug.Assert(mipTex != null);
			// Mipmapping
			if (HWMipmaps)
				// Hardware mipmaps
				mipTex.GenerateMipSubLevels();
			else {
				// Software mipmaps
                TextureLoader.FilterTexture(mipTex, 0, Filter.Box);
			}
		}

		///<summary>
		///    Function to set mipmap generation
		///</summary>
		public void SetMipmapping(bool doMipmapGen, bool HWMipmaps, D3D.BaseTexture mipTex) {
			this.doMipmapGen = doMipmapGen;
			this.HWMipmaps = HWMipmaps;
			this.mipTex = mipTex;
		}

		///<summary>
		///    Get rendertarget for z slice
		///</summary>
		public override RenderTexture GetRenderTarget(int zoffset) {
			Debug.Assert(((int)usage & (int)TextureUsage.RenderTarget) != 0);
			Debug.Assert(zoffset < depth);
			return sliceTRT[zoffset];
		}

		///<summary>
		///    Notify TextureBuffer of destruction of render target
		///</summary>
        public override void ClearSliceRTT(int zoffset) {
            sliceTRT[zoffset] = null;
        }

        public override void Dispose() {
            DestroyRenderTextures();
            base.Dispose();
        }

        #endregion Methods

    }
}
