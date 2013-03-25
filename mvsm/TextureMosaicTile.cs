/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.Drawing;
using System.IO;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Media;
using Axiom.RenderSystems.DirectX9;
using Image = Axiom.Media.Image;
using D3D = Microsoft.DirectX.Direct3D;
using DX = Microsoft.DirectX;

namespace Axiom.SceneManagers.Multiverse
{
    public class TextureMosaicTile : DataMosaicTile
    {
        protected string textureName;
        const PixelFormat DEFAULT_IMAGE_FORMAT = PixelFormat.A8B8G8R8;
        const PixelFormat INTERNAL_DATA_FORMAT = PixelFormat.A8B8G8R8;
        const PixelFormat EXTERNAL_DATA_FORMAT = PixelFormat.R8G8B8A8;
        const TextureUsage DYNAMIC_TEXTURE_USAGE = TextureUsage.DynamicWriteOnly;
        protected bool dirtyImage;
        protected Rectangle dirtyArea;
        protected D3D.Surface dynamicSurface;

        int m_loadTileZ;
        int m_tileLocationX;

        public TextureMosaicTile(Mosaic parent, int tileSizeSamples, float metersPerSample, int tileLocationX, int tileLocationZ, Vector3 worldLocMM)
            : base(parent, tileSizeSamples, metersPerSample, tileLocationX, tileLocationZ, worldLocMM)
        {
            m_loadTileZ = parent.MosaicDesc.SizeZTiles - 1 - tileLocationZ;
            m_tileLocationX = tileLocationX;

            if (available)
            {
                textureName = string.Format("{0}_x{1}y{2}.{3}", parent.BaseName, tileLocationX, m_loadTileZ, parent.MosaicDesc.FileExt);
            }
            else
            {
                textureName = "zero.png";
            }
            //LogManager.Instance.Write("[{0},{1}] : ({2},{3}) : {4}", tileX, tileZ, worldLocMM.x, worldLocMM.z, textureName);
        }

        public override void Load()
        {
            Load(false);
        }

        public void Load(bool forceDynamic)
        {
            if (available && (!loaded || forceDynamic))
            {
                if( forceDynamic && textureName.Equals( "zero.png" ) )
                {
                    textureName = string.Format( "{0}_x{1}y{2}.{3}", parent.BaseName, m_tileLocationX, m_loadTileZ,
                                                 parent.MosaicDesc.FileExt );
                }

                // Attempt to load the texture.  If the texture
                // is already loaded, then this will just return the
                // already loaded texture, so no extra work will be done.

                Texture texture = TextureManager.Instance.GetByName(textureName);
                if (texture != null)
                {
                    if (forceDynamic)
                    {
                        if (texture.Usage == DYNAMIC_TEXTURE_USAGE)
                        {
                            loaded = true;
                            return;
                        }
                    }
                    texture.Unload();
                    texture = null;
                }

                if (dynamicSurface != null)
                {
                    dynamicSurface.Dispose();
                    dynamicSurface = null;
                }

                if (!forceDynamic)
                {
                    try
                    {
                        texture = TextureManager.Instance.Load(textureName);
                    } // ReSharper disable EmptyGeneralCatchClause
                    catch // ReSharper restore EmptyGeneralCatchClause
                    {
                        // Ignore
                    }
                }

                // If we failed to load the texture, manually create it
                if (texture == null) {
                    texture = TextureManager.Instance.CreateManual(textureName, TextureType.TwoD, parent.MosaicDesc.TileSizeSamples,
                        parent.MosaicDesc.TileSizeSamples, 0, DEFAULT_IMAGE_FORMAT, DYNAMIC_TEXTURE_USAGE);
                    texture.Load();
                    if (texture is D3DTexture)
                    {
                        D3D.Texture t = (texture as D3DTexture).DXTexture as D3D.Texture;
                        if (t != null)
                        {
                            dynamicSurface = t.Device.CreateOffscreenPlainSurface(texture.Width, texture.Height,
                                D3DHelper.ConvertEnum(texture.Format), D3D.Pool.Default);
                        }
                    }
                }

                loaded = true;
            }
        }

        private void EnsureTileDataCreated()
        {
            if (tileData != null)
            {
                return;
            }
            CreateTileData();
        }

        private void CreateTileData()
        {
            if( textureName.Equals( "zero.png" ) )
            {
                textureName = string.Format( "{0}_x{1}y{2}.{3}", parent.BaseName, m_tileLocationX, m_loadTileZ,
                                             parent.MosaicDesc.FileExt );

            }

            Image textureImage;
            Texture texture = TextureManager.Instance.GetByName(textureName);
            if( ResourceManager.HasCommonResourceData( textureName ) )
            {
                Stream s = ResourceManager.FindCommonResourceData(textureName);
                textureImage = Image.FromStream(s, parent.MosaicDesc.FileExt);
                s.Close();
            }
            else
            {
                // Create a new image
                int bpp = PixelUtil.GetNumElemBytes(DEFAULT_IMAGE_FORMAT);
                byte[] buffer = new byte[tileSizeSamples*tileSizeSamples*bpp];
                textureImage = Image.FromDynamicImage(buffer, tileSizeSamples, tileSizeSamples, DEFAULT_IMAGE_FORMAT);
                Modified = true;
            }

            // Cause the texture image to get refreshed
            dirtyImage = true;
            dirtyArea.X = 0;
            dirtyArea.Y = 0;
            dirtyArea.Width = textureImage.Width;
            dirtyArea.Height = textureImage.Height;

            // Popupate the tileData from the image
            switch (textureImage.Format)
            {
                case PixelFormat.A8:
                case PixelFormat.L8:
                    tileData = new TileData8(textureImage);
                    break;
                case PixelFormat.L16:
                    tileData = new TileData16(textureImage);
                    break;
                case PixelFormat.R8G8B8:
                case PixelFormat.B8G8R8:
                    tileData = new TileData24(textureImage);
                    break;
                case PixelFormat.A8B8G8R8:
                case PixelFormat.A8R8G8B8:
                case PixelFormat.B8G8R8A8:
                case PixelFormat.R8G8B8A8:
                case PixelFormat.X8R8G8B8:
                case PixelFormat.X8B8G8R8:
                    tileData = new TileData32(textureImage);
                    break;
                default:
                    throw new InvalidDataException("Unexpected pixel format: " + textureImage.Format);
            }
        }

        /// <summary>
        /// Recreate the texture if it's not dynamic.  Returns true if it
        /// was reloaded.
        /// </summary>
        /// <returns></returns>
        private bool EnsureTextureIsDynamic()
        {
            Texture texture = TextureManager.Instance.GetByName(textureName);
            if (texture == null || texture.Usage != DYNAMIC_TEXTURE_USAGE)
            {
                Load(true);
                EnsureTileDataCreated();
                return true;
            }

            EnsureTileDataCreated();
            return false;
        }

        public override void Save(bool force)
        {
            if (tileData == null)
            {
                if (force)
                {
                    // Create tile data solely for the purpose of saving it
                    EnsureTileDataCreated();
                }
                else
                {
                    // No tile data means nothing has changed, so nothing to save
                    return;
                }
            }

            base.Save(force);
        }

        public string TextureName
        {
            get
            {
                return textureName;
            }
        }

        public byte[] GetTextureMap(int sampleX, int sampleZ)
        {
            Load();
            EnsureTileDataCreated();

            uint data = tileData.GetData(sampleX, sampleZ);
            byte[] byteData = ConvertPixelToBytes(data, INTERNAL_DATA_FORMAT, EXTERNAL_DATA_FORMAT);
            return byteData;
        }

        public void SetTextureMap(int sampleX, int sampleZ, byte[] textureMap)
        {
            Load(true);
            EnsureTileDataCreated();

            // Update the tile data so that we can save the modification
            uint data = ConvertPixelToUint(textureMap, EXTERNAL_DATA_FORMAT, INTERNAL_DATA_FORMAT);
            tileData.SetData(sampleX, sampleZ, data);

            if (dirtyArea.IsEmpty)
            {
                dirtyArea.X = sampleX;
                dirtyArea.Y = sampleZ;
                dirtyArea.Width = 1;
                dirtyArea.Height = 1;
            }
            else
            {
                if (!dirtyArea.Contains(sampleX, sampleZ))
                {
                    // Expand the dirty area to contain the new pixel
                    // Keep in mind that the width and height are based on 1, not 0
                    int left = dirtyArea.X;
                    int right = dirtyArea.X + dirtyArea.Width - 1;
                    int top = dirtyArea.Y;
                    int bottom = dirtyArea.Y + dirtyArea.Height - 1;
                    if (sampleX < left)
                    {
                        left = sampleX;
                    }
                    if (sampleZ < top)
                    {
                        top = sampleZ;
                    }
                    if (sampleX > right)
                    {
                        right = sampleX;
                    }
                    if (sampleZ > bottom)
                    {
                        bottom = sampleZ;
                    }
                    dirtyArea.X = left;
                    dirtyArea.Y = top;
                    dirtyArea.Width = (right - left) + 1;
                    dirtyArea.Height = (bottom - top) + 1;
                }
            }
            dirtyImage = true;
            Modified = true;
            FireTileChanged(sampleX, sampleZ, 1, 1);
        }

        public void RefreshTexture()
        {
            if (tileData != null && dirtyImage)
            {
                EnsureTextureIsDynamic(); // Unload texture & recreate if needed

                Texture texture = TextureManager.Instance.GetByName(textureName);
                if (texture == null)
                {
                    // We couldn't find the texture.  It might be that we're in 
                    // the midst of swapping them at the DirectX level (this is
                    // only speculation)
                    return;
                }

                if (texture is D3DTexture)
                {
                    // Turns out that to get performance, not only did I have to go 
                    // straight to DirectX, unsafe code is much faster as well.  What
                    // we're doing here is keeping a temporary surface around that we
                    // can lock and draw to at our leisure, then copying the surface
                    // over to the correct texture data when we're done.  To do this,
                    // the easiest way is to lock the desired rectangle on the temp
                    // surface, and get a graphics stream object back from it.  You
                    // might think you could then use byte arrays, or even ask for 
                    // an Array of bytes back from LockRectangle, but not only was 
                    // that slower, it also produced unpredictable results, possibly
                    // due to storing the array in row order vs. column order in the
                    // native vs. managed areas.
                    //
                    // The temporary surface is necessary because, well, I could 
                    // never seem to acquire the lock on the real surface.  However,
                    // an offscreen plain surface (as used above) seems to lock fine.
                    //
                    // Next caveat: The pointer on the graphics stream points to the
                    // start of the row of the locked rectangle.  You'd be surprised
                    // how long it took me to figure that one out.  Further, it's 
                    // important to use the pitch returned by LockRectangle to adjust
                    // your row array position, as the pitch may or may not be your
                    // surface width in bytes.  (Some drivers store extra data on the
                    // ends of the rows, it seems.)
                    Rectangle lockRect = new Rectangle();
                    lockRect.X = dirtyArea.X;
                    lockRect.Y = dirtyArea.Y;
                    lockRect.Width = dirtyArea.Width;
                    lockRect.Height = dirtyArea.Height;

                    D3D.Texture t = (texture as D3DTexture).DXTexture as D3D.Texture;
                    int pitch;
                    int bpp = PixelUtil.GetNumElemBytes(texture.Format);
                    D3D.Surface dst = t.GetSurfaceLevel(0);
                    DX.GraphicsStream g = dynamicSurface.LockRectangle(lockRect, D3D.LockFlags.NoSystemLock, out pitch);
                    unsafe
                    {
                        uint *dstArray = (uint *) g.InternalDataPointer;
                        pitch /= sizeof(uint);
                        for (int z = 0; z < lockRect.Height; z++)
                        {
                            for (int x = 0; x < lockRect.Width; x++)
                            {
                                uint data = GetData(x + lockRect.X, z + lockRect.Y);
                                uint converted = ConvertPixel(data, INTERNAL_DATA_FORMAT, texture.Format);
                                dstArray[z * pitch + x] = converted;
                            }
                        }
                    }
                    dynamicSurface.UnlockRectangle();
                    D3D.SurfaceLoader.FromSurface(dst, dynamicSurface, D3D.Filter.None, 0);
                }
                else
                {
#if false
                    // following code is for blitting only the dirty rectangle
                    BasicBox destBox = new BasicBox(dirtyArea.X, dirtyArea.Y, dirtyArea.X + dirtyArea.Width,
                                                    dirtyArea.Y + dirtyArea.Height);
                    PixelBox srcPixel = textureImage.GetPixelBox(0, 0);
                    BasicBox srcBox = new BasicBox(0, 0, dirtyArea.Width, dirtyArea.Height);
                    PixelBox trimmedSrcPixel = srcPixel.GetSubVolume(srcBox);
                    buffer.BlitFromMemory(trimmedSrcPixel, destBox);
#endif
                }

                // Clean up dirty bit
                dirtyImage = false;
                dirtyArea.X = 0;
                dirtyArea.Y = 0;
                dirtyArea.Width = 0;
                dirtyArea.Height = 0;
            }
        }

        private static uint EndianSwapPixel(uint pixel)
        {
            uint ans = 0;
            ans |= ((pixel & 0x000000ff) << 24);
            ans |= ((pixel & 0x0000ff00) << 8);
            ans |= ((pixel & 0x00ff0000) >> 8);
            ans |= ((pixel & 0xff000000) >> 24);
            return ans;
        }

        private static void EndianSwapPixel(ref byte[] pixel)
        {
            Array.Reverse(pixel);
        }

        private static uint ByteArrayToUint(byte[] src)
        {
            uint ans = 0;
            ans |= (uint)(src[0] << 24);
            ans |= (uint)(src[1] << 16);
            ans |= (uint)(src[2] << 8);
            ans |= (uint)(src[3] << 0);
            return ans;
        }

        private static byte[] UintToByteArray(uint src)
        {
            byte[] ans = new byte[4];
            ans[0] |= (byte)((src & 0xff000000) >> 24);
            ans[1] |= (byte)((src & 0x00ff0000) >> 16);
            ans[2] |= (byte)((src & 0x0000ff00) >> 8);
            ans[3] |= (byte)((src & 0x000000ff) >> 0);
            return ans;
        }

        private static uint UintToRGBA(uint value, PixelFormat pf)
        {
            uint ans = 0;
            switch (pf)
            {
                case PixelFormat.R8G8B8A8:
                    return value;
                case PixelFormat.A8B8G8R8:
                case PixelFormat.X8B8G8R8:
                    ans |= ((value & 0xff000000) >> 24);
                    ans |= ((value & 0x00ff0000) >> 8);
                    ans |= ((value & 0x0000ff00) << 8);
                    ans |= ((value & 0x000000ff) << 24);
                    break;
                case PixelFormat.A8R8G8B8:
                case PixelFormat.X8R8G8B8:
                    ans |= ((value & 0xff000000) >> 24);
                    ans |= ((value & 0x00ff0000) << 8);
                    ans |= ((value & 0x0000ff00) << 8);
                    ans |= ((value & 0x000000ff) << 8);
                    break;
                default:
                    throw new Exception("Pixel format " + pf + " is not supported for alpha map.");
            }
            return ans;
        }

        private static uint RGBAToFormat(uint value, PixelFormat pf)
        {
            uint ans = 0;
            switch (pf)
            {
                case PixelFormat.R8G8B8A8:
                    return value;
                case PixelFormat.A8B8G8R8:
                case PixelFormat.X8B8G8R8:
                    ans |= ((value & 0xff000000) >> 24);
                    ans |= ((value & 0x00ff0000) >> 8);
                    ans |= ((value & 0x0000ff00) << 8);
                    ans |= ((value & 0x000000ff) << 24);
                    break;
                case PixelFormat.A8R8G8B8:
                case PixelFormat.X8R8G8B8:
                    ans |= ((value & 0xff000000) >> 8);
                    ans |= ((value & 0x00ff0000) >> 8);
                    ans |= ((value & 0x0000ff00) >> 8);
                    ans |= ((value & 0x000000ff) << 24);
                    break;
                default:
                    throw new Exception("Pixel format " + pf + " is not supported for alpha map.");
            }
            return ans;
        }

        private static uint ConvertPixel(uint src, PixelFormat inFormat, PixelFormat outFormat)
        {
            uint rgba = UintToRGBA(src, inFormat);
            uint value = RGBAToFormat(rgba, outFormat);
            return value;
        }

        private static byte[] ConvertPixelToBytes(uint src, PixelFormat inFormat, PixelFormat outFormat)
        {
            uint value = ConvertPixel(src, inFormat, outFormat);
            return UintToByteArray(value);
        }

        private static uint ConvertPixelToUint(byte[] bytes, PixelFormat inFormat, PixelFormat outFormat)
        {
            uint value = 0;
            if (bytes != null)
            {
                value = ConvertPixel(ByteArrayToUint(bytes), inFormat, outFormat);
            }
            return value;
        }
    }
}
