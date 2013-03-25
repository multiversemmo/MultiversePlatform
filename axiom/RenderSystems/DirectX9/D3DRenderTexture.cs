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
using Axiom.Graphics;
using Axiom.Media;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
	/// <summary>
	///     Summary description for D3DRenderTexture.
	/// </summary>
    public class D3DRenderTexture : RenderTexture
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(D3DRenderTexture));

        public D3DRenderTexture(string name, HardwarePixelBuffer buffer)
            : base(buffer, 0)
        {
            this.name = name;
        }

        //public D3DRenderTexture(string name, int width, int height, PixelFormat format)
        //    :this(name, width, height, TextureType.TwoD, format) {}

        //public D3DRenderTexture(string name, int width, int height, TextureType type, PixelFormat format)
        //    :base(name, width, height, format) {

        //    privateTex = (D3DTexture)TextureManager.Instance.CreateManual(name + "_PRIVATE##", type, width, height, 0, format, TextureUsage.RenderTarget);

        //}

        public void Rebind(D3DHardwarePixelBuffer buffer)
        {
            pixelBuffer = buffer;
            width = pixelBuffer.Width;
            height = pixelBuffer.Height;
            colorDepth = PixelUtil.GetNumElemBits(buffer.Format);
        }

        public override void Update()
        {
            D3D9RenderSystem rs = (D3D9RenderSystem)Root.Instance.RenderSystem;
            if (rs.DeviceLost)
                return;

            base.Update();
        }

        //protected override void CopyToTexture() {
        //    privateTex.CopyToTexture(texture);
        //}

        public override object GetCustomAttribute(string attribute)
        {
            switch (attribute)
            {
                case "DDBACKBUFFER":
                    return ((D3DHardwarePixelBuffer)pixelBuffer).Surface;
                case "HWND":
                    return null;
                case "BUFFER":
                    return pixelBuffer;
            }
            return null;
            // return new NotSupportedException("There is no D3D RenderWindow custom attribute named " + attribute);
        }

        public override bool RequiresTextureFlipping
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///   Save our texture to a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="requestedFormat"></param>
        public override void Save(System.IO.Stream stream, PixelFormat requestedFormat)
        {
            // First try to see if we can use the fast method to save this image.
            // TODO: I should be able to make the common path closer to this in performance (robin@multiverse.net)
            if (pixelBuffer.Format == PixelFormat.A8R8G8B8) {
                if (requestedFormat == PixelFormat.A8B8G8R8 || requestedFormat == PixelFormat.B8G8R8) {
                    SimpleSave(stream, requestedFormat);
                    return;
                }
            }
            int bufSize = PixelUtil.GetMemorySize(pixelBuffer.Width, pixelBuffer.Height, pixelBuffer.Depth, requestedFormat);
            byte[] buffer = new byte[bufSize];
            // Pin down the byte array
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned); 
            IntPtr address = handle.AddrOfPinnedObject();
            PixelBox box = 
                new PixelBox(pixelBuffer.Width, pixelBuffer.Height, pixelBuffer.Depth, requestedFormat, address);
            pixelBuffer.BlitToMemory(box);
            handle.Free();

            // I need to flip this image, since it was upside down in the 
            // internal buffers.  I could do this in the save pass, which
            // would make this more efficient, but I don't currently think
            // that efficiency is that important.
            Image image = Image.FromDynamicImage(buffer, pixelBuffer.Width, pixelBuffer.Height,
                                                 pixelBuffer.Depth, requestedFormat);
            image.FlipAroundX();
            // Ok, now the data in buffer has been flipped.
            // Go ahead and discard the image
            image.Dispose();

            // write the data to the stream provided
            stream.Write(buffer, 0, bufSize);
        }

        /// <summary>
        ///   This is a much faster variant of the save.  We can use it if our source format
        ///   and our destination format are typical.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="requestedFormat"></param>
        protected void SimpleSave(System.IO.Stream stream, PixelFormat requestedFormat)
        {
            D3DHardwarePixelBuffer hwBuffer = pixelBuffer as D3DHardwarePixelBuffer;
            Debug.Assert(hwBuffer.Format == PixelFormat.A8R8G8B8);
            Surface sourceSurface = hwBuffer.Surface;
            int width = hwBuffer.Width;
            int height = hwBuffer.Height;
            Device device = sourceSurface.Device;

            // create surface in system memory to copy render target into
            Surface destSurface = device.CreateOffscreenPlainSurface(
                width, height, D3D.Format.A8R8G8B8, Pool.SystemMemory);

            // copy render target to system memory surface
            device.GetRenderTargetData(sourceSurface, destSurface);

            int pitch;
            GraphicsStream graphStream = destSurface.LockRectangle(LockFlags.ReadOnly | LockFlags.NoSystemLock, out pitch);

            int bytesPerPixel = 3;
            if (requestedFormat == PixelFormat.BYTE_RGBA)
            {
                bytesPerPixel = 4;
            }
            byte[] buffer = new byte[width * height * bytesPerPixel];

            int offset = 0, line = 0, count = 0;

            // gotta copy that data manually since it is in another format (sheesh!)
            unsafe
            {
                byte* data = (byte*)graphStream.InternalData;

                for (int y = height - 1; y >= 0; y--)
                {
                    line = y * pitch;

                    for (int x = 0; x < width; x++)
                    {
                        offset = x * 4;

                        int pixel = line + offset;

                        // Actual format is BRGA for some reason
                        buffer[count++] = data[pixel + 2];
                        buffer[count++] = data[pixel + 1];
                        buffer[count++] = data[pixel + 0];
                        if (bytesPerPixel == 4)
                        {
                            buffer[count++] = data[pixel + 3];
                        }
                    }
                }
            }

            destSurface.UnlockRectangle();

            // dispose of the temporary surface
            destSurface.Dispose();

            // write the data to the stream provided
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
