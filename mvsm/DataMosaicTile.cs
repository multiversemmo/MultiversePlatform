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
using System.IO;
using System.Diagnostics;
using Axiom.MathLib;
using Axiom.Media;
using Axiom.Core;
using Tao.DevIl;

namespace Axiom.SceneManagers.Multiverse
{
    public class DataMosaicTile : MosaicTile
    {
        protected TileData tileData;

        public DataMosaicTile(Mosaic parent, int tileSizeSamples, float metersPerSample, int tileLocationX, int tileLocationZ, Vector3 worldLocMM)
            : base(parent, tileSizeSamples, metersPerSample, tileLocationX, tileLocationZ, worldLocMM)
        {
        }

        public override void Load()
        {
            if (available && !loaded)
            {
                // swap the z coord of the tiles to deal with our -Z = north coordinate system
                int loadTileZ = parent.MosaicDesc.SizeZTiles - 1 - tileZ;
                string filename =
                    string.Format("{0}_x{1}y{2}.{3}", parent.BaseName, tileX, loadTileZ, parent.MosaicDesc.FileExt);

                Image img;
                if (ResourceManager.HasCommonResourceData(filename))
                {
                    Stream s = ResourceManager.FindCommonResourceData(filename);
                    img = Image.FromStream(s, parent.MosaicDesc.FileExt);
                    s.Close();
                } 
                else
                {
                    // Create a new image
                    byte[] buffer = new byte[tileSizeSamples*tileSizeSamples*2];
                    img = Image.FromDynamicImage(buffer, tileSizeSamples, tileSizeSamples, PixelFormat.L16);
                    Modified = true;
                }

                Debug.Assert(tileSizeSamples == img.Width);
                Debug.Assert(tileSizeSamples == img.Height);

                switch (img.Format)
                {
                    case PixelFormat.A8:
                    case PixelFormat.L8:
                        tileData = new TileData8(img);
                        break;
                    case PixelFormat.L16:
                        tileData = new TileData16(img);
                        break;
                    case PixelFormat.R8G8B8:
                    case PixelFormat.B8G8R8:
                        tileData = new TileData24(img);
                        break;
                    case PixelFormat.A8R8G8B8:
                    case PixelFormat.A8B8G8R8:
                    case PixelFormat.B8G8R8A8:
                    case PixelFormat.R8G8B8A8:
                        tileData = new TileData32(img);
                        break;
                    default:
                        throw new ArgumentException("Pixel format " + img.Format + " is not currently supported.");
                }

                if (tileData != null)
                {
                    loaded = true;
                }
            }
        }

        public override void Save(bool force)
        {
            if (!force && !Modified)
            {
                return;
            }

            string ext = parent.MosaicDesc.FileExt;

            // swap the z coord of the tiles to deal with our -Z = north coordinate system
            int loadTileZ = parent.MosaicDesc.SizeZTiles - 1 - tileZ;
            string tileName = string.Format("{0}_x{1}y{2}.{3}", parent.BaseName, tileX, loadTileZ, ext);

            string fileName;
            if (ResourceManager.HasCommonResourceData(tileName))
            {
                fileName = ResourceManager.GetCommonResourceDataFilePath(tileName);
            }
            else
            {
                string saveDir = parent.MosaicDesc.DefaultTerrainSaveDirectory;
                fileName = Path.Combine(saveDir, tileName);
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                ResourceManager.AddCommonSearchPath(saveDir);
            }

            TaoImage image = new TaoImage(tileSizeSamples, tileSizeSamples, 
                tileData.BytesPerSample, tileData.IlFormat);

            for (int z = 0; z < tileSizeSamples; z++)
            {
                for (int x = 0; x < tileSizeSamples; x++)
                {
                    uint heightData = tileData.GetData(x, z);
                    image.SetPixel(x, z, heightData);
                }
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            image.Save(fileName);

            if (Modified)
            {
                Modified = false;
            }
        }

        /// <summary>
        /// Get the raw data from the data map
        /// </summary>
        /// <param name="sampleX">x coord in samples within this tile</param>
        /// <param name="sampleZ">z coord in samples within this tile</param>
        /// <returns></returns>
        public uint GetData(int sampleX, int sampleZ)
        {
            if (!loaded)
            {
                Load();
            }
            return tileData.GetData(sampleX, sampleZ);
        }

        /// <summary>
        /// Get the value from the data map normalized to be between 0 and 1
        /// </summary>
        /// <param name="sampleX"></param>
        /// <param name="sampleZ"></param>
        /// <returns></returns>
        public float GetDataNormalized(int sampleX, int sampleZ)
        {
            if (!loaded)
            {
                Load();
            }
            return tileData.GetData(sampleX, sampleZ) / (float) tileData.MaxValue;
        }

        /// <summary>
        /// Set the value from the data map normalized to be between 0 and 1
        /// </summary>
        /// <param name="sampleX"></param>
        /// <param name="sampleZ"></param>
        /// <param name="normalizedHeight"></param>
        /// <returns></returns>
        public void SetDataNormalized(int sampleX, int sampleZ, float normalizedHeight)
        {
            if (!loaded)
            {
                Load();
            }
            tileData.SetData(sampleX, sampleZ, (uint) ((double)normalizedHeight * (double)tileData.MaxValue));
            Modified = true;
            FireTileChanged(sampleX, sampleZ, 1, 1);
        }
    }

    #region TileData Classes
    #region TileData Base
    public abstract class TileData
    {
        protected int size;
        protected PixelFormat format;

        protected TileData(Image img)
        {
            Debug.Assert(img.Width == img.Height);
            size = img.Width;
            format = img.Format;
        }

        public abstract uint GetData(int x, int z);
        public abstract void SetData(int x, int z, uint height);
        public abstract uint MaxValue { get; }
        public abstract int BytesPerSample { get; }
        public abstract int IlFormat { get; }
    }
    #endregion TileData Base

    #region TileData8
    public class TileData8 : TileData
    {
        public override int BytesPerSample { get { return 1; } }
        public override int IlFormat { get { return Il.IL_LUMINANCE; } }

        readonly byte[] data;

        public TileData8(Image img) : base(img)
        {
            Debug.Assert((img.Format == PixelFormat.L8) || (img.Format == PixelFormat.A8));
            data = new byte[size * size];

            LoadData(img);
        }

        protected void LoadData(Image img)
        {
            for (int i = 0; i < size * size; i++)
            {
                data[i] = img.Data[i];
            }
        }

        public override uint GetData(int x, int z)
        {
            return data[x + z * size];
        }

        public override void SetData(int x, int z, uint height)
        {
            data[x + z*size] = (byte) height;
        }

        public override uint MaxValue
        {
            get
            {
                return 0xff;
            }
        }
    }
    #endregion TileData8

    #region TileData16
    public class TileData16 : TileData
    {
        public override int BytesPerSample { get { return 2; } }
        public override int IlFormat { get { return Il.IL_LUMINANCE; } }

        readonly ushort[] data;

        public TileData16(Image img)
            : base(img)
        {
            Debug.Assert(img.Format == PixelFormat.L16);

            data = new ushort[size * size];

            LoadData(img);
        }

        protected void LoadData(Image img)
        {
            for (int i = 0; i < size * size; i++)
            {
                int j = i * 2;
                ushort lo = img.Data[j];
                ushort hi = img.Data[j + 1];

                data[i] = (ushort)(lo + (hi << 8));
            }
        }

        public override uint GetData(int x, int z)
        {
            return data[x + z * size];
        }

        public override void SetData(int x, int z, uint height)
        {
            data[x + z * size] = (ushort) height;
        }

        public override uint MaxValue
        {
            get
            {
                return 0xffff;
            }
        }
    }
    #endregion TileData16

    #region TileData24
    public class TileData24 : TileData
    {
        public override int BytesPerSample { get { return 3; } }
        public override int IlFormat { get { return Il.IL_RGB; } }

        readonly byte[] data;

        public TileData24(Image img)
            : base(img)
        {
            Debug.Assert((img.Format == PixelFormat.R8G8B8) || (img.Format == PixelFormat.B8G8R8));

            data = new byte[size * size * 3];

            LoadData(img);
        }

        protected void LoadData(Image img)
        {
            for (int i = 0; i < size * size * 3; i++)
            {
                data[i] = img.Data[i];
            }
        }

        public override uint GetData(int x, int z)
        {
            int offset = (x + z * size) * 3;
            uint val = (uint) ( data[offset+0] | ( data[offset + 1] << 8 ) | ( data[offset + 2] << 16 ) );
            return val;
        }

        public override void SetData(int x, int z, uint height)
        {
            byte b3 = (byte) height;
            height >>= 8;
            byte b2 = (byte) height;
            height >>= 8;
            byte b1 = (byte) height;

            int offset = (x + z * size) * 3;
            data[offset + 0] = b1;
            data[offset + 1] = b2;
            data[offset + 2] = b3;
        }

        public override uint MaxValue
        {
            get
            {
                return 0xffffff;
            }
        }
    }
    #endregion TileData24

    #region TileData32
    public class TileData32 : TileData
    {
        public override int BytesPerSample { get { return 4; } }
        public override int IlFormat { get { return Il.IL_RGBA; } }

        readonly uint[] data;

        public TileData32(Image img)
            : base(img)
        {
            Debug.Assert((img.Format == PixelFormat.A8B8G8R8) || (img.Format == PixelFormat.A8R8G8B8) ||
                (img.Format == PixelFormat.B8G8R8A8) || img.Format == PixelFormat.R8G8B8A8 || 
                 img.Format == PixelFormat.X8R8G8B8 || img.Format == PixelFormat.X8B8G8R8);

            data = new uint[size * size];

            LoadData(img);
        }

        protected void LoadData(Image img)
        {
            for (int i = 0; i < size * size; i++)
            {
                int j = i * 4;

                uint val = (uint)(img.Data[j] |
                    (img.Data[j + 1] << 8) |
                    (img.Data[j + 2] << 16) |
                    (img.Data[j + 3] << 24));

                data[i] = val;
            }
        }

        public override uint GetData(int x, int z)
        {
            return data[x + z * size];
        }

        public override void SetData(int x, int z, uint height)
        {
            data[x + z * size] = height;
        }

        public override uint MaxValue
        {
            get
            {
                return 0xffffffff;
            }
        }
    }
    #endregion TileData32
    #endregion TileData Classes
}
