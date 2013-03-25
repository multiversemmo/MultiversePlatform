using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Lib.WorldMap
{
    public abstract class MapBuffer
    {
        protected int metersPerSample;
        protected int numSamples;
        protected bool dirty;
        protected WorldMap map;

        public abstract uint GetValue(int x, int z);
        public abstract void SetValue(int x, int z, uint value);

        public abstract void Copy(int destX, int destZ, MapBuffer src);
        public abstract void Fill(uint value);
        public abstract void Fill(uint value, int x, int z, int size);
        protected abstract void InitBuffer();
        protected abstract void LoadRaw(string filename);
        protected abstract void SaveRaw(string filename);
        protected abstract System.Drawing.Color AverageColor(int x, int z, int size);
        protected abstract uint AverageValue(int x, int z, int size);
        protected abstract MapBuffer NewBuffer(WorldMap map, int numSamples, int metersPerSample);

        /// <summary>
        /// This constructor builds an empty buffer
        /// </summary>
        /// <param name="size"></param>
        /// <param name="metersPerSample"></param>
        public MapBuffer(WorldMap map, int numSamples, int metersPerSample)
        {
            this.metersPerSample = metersPerSample;
            this.numSamples = numSamples;
            this.map = map;

            InitBuffer();
            dirty = false;
        }

        public MapBuffer(WorldMap map, string filename)
        {
            LoadRaw(filename);

            metersPerSample = WorldMap.metersPerTile / numSamples;
            this.map = map;

            dirty = false;
        }

        /// <summary>
        /// Copy pixels in from a source image, rescaling the values to map into a new value range.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sx"></param>
        /// <param name="sz"></param>
        /// <param name="srcMinHeight"></param>
        /// <param name="srcMaxHeight"></param>
        /// <param name="size"></param>
        /// <param name="destMinHeight"></param>
        /// <param name="destMaxHeight"></param>
        protected virtual void CopyFromImageRescale(Image src, int sx, int sz, float srcMinHeight, float srcMaxHeight, int size, float destMinHeight, float destMaxHeight)
        {
            // copy source sub-image to destination
            double sourceHeightRange = srcMaxHeight - srcMinHeight;
            double destHeightRange = destMaxHeight - destMinHeight;

            // Convert height range to inverse so that we can use multiplication rather than
            // division in the inner loop.
            double invHeightRange = 1.0 / destHeightRange;

            // loop over all pixels, converting to the world's height range while
            // copying.
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    double raw = src.GetNormPixel(x + sx, z + sz);

                    double scaledSourceValue = raw * sourceHeightRange + srcMinHeight;
                    double destNorm = (scaledSourceValue - destMinHeight) * invHeightRange;

                    if (destNorm > 1.0)
                    {
                        destNorm = 1.0;
                    }
                    if (destNorm < 0.0)
                    {
                        destNorm = 0.0;
                    }
                    SetValue(x, z, (uint)(destNorm * MaxSampleValue));
                }
            }

            dirty = true;
        }

        /// <summary>
        /// Copy pixels in from a source image.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sx"></param>
        /// <param name="sz"></param>
        /// <param name="size"></param>
        protected virtual void CopyFromImage(Image src, int sx, int sz, int size)
        {
            // loop over all pixels, converting to the world's height range while
            // copying.
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    uint raw = src.GetRawPixel(x + sx, z + sz);
                    SetValue(x, z, raw);
                }
            }

            dirty = true;
        }

        /// <summary>
        /// Build a buffer from a region of an image, scaling the pixels to the right value range
        /// </summary>
        /// <param name="src"></param>
        /// <param name="metersPerSample"></param>
        /// <param name="sx"></param>
        /// <param name="sz"></param>
        /// <param name="size"></param>
        public MapBuffer(WorldMap map, Image src, int metersPerSample, int sx, int sz, float srcMinHeight, float srcMaxHeight, int size, float destMinHeight, float destMaxHeight)
        {
            Debug.Assert(src.Width >= (sx + size));
            Debug.Assert(src.Height >= (sz + size));
            Debug.Assert(src.BytesPerPixel == BytesPerSample);

            this.metersPerSample = metersPerSample;
            this.numSamples = size;
            this.map = map;

            InitBuffer();

            CopyFromImageRescale(src, sx, sz, srcMinHeight, srcMaxHeight, size, destMinHeight, destMaxHeight);
        }

        /// <summary>
        /// Build a buffer from a region of an image
        /// </summary>
        /// <param name="src"></param>
        /// <param name="metersPerSample"></param>
        /// <param name="sx"></param>
        /// <param name="sz"></param>
        /// <param name="size"></param>
        public MapBuffer(WorldMap map, Image src, int metersPerSample, int sx, int sz, int size)
        {
            Debug.Assert(src.Width >= (sx + size));
            Debug.Assert(src.Height >= (sz + size));
            Debug.Assert(src.BytesPerPixel == BytesPerSample);

            this.metersPerSample = metersPerSample;
            this.numSamples = size;
            this.map = map;

            InitBuffer();

            CopyFromImage(src, sx, sz, size);
        }

        public void Save(string filename)
        {
            if (dirty)
            {
                SaveRaw(filename);
                dirty = false;
            }
        }

        /// <summary>
        /// Scale a subset of the image (must be power of 2 size) and create a windows
        /// Bitmap from it.
        /// </summary>
        /// <param name="srcX"></param>
        /// <param name="srcZ"></param>
        /// <param name="size"></param>
        /// <param name="destsize"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap CreateThumbnail(int srcX, int srcZ, int size, int destsize)
        {
            Debug.Assert((srcX + size) <= numSamples);
            Debug.Assert((srcZ + size) <= numSamples);
            Debug.Assert(destsize < size);

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(destsize, destsize);
            int scalefactor = size / destsize;

            for (int dz = 0; dz < destsize; dz++)
            {
                for (int dx = 0; dx < destsize; dx++)
                {

                    System.Drawing.Color color = AverageColor(dx * scalefactor + srcX, dz * scalefactor + srcZ, scalefactor);

                    bitmap.SetPixel(dx, dz, color);
                }
            }
            return bitmap;
        }

        public virtual MapBuffer Scale(int destNumSamples)
        {
            Debug.Assert(destNumSamples != numSamples);
            if (destNumSamples < numSamples)
            {
                return ScaleDown(numSamples / destNumSamples);
            }
            else
            {
                return ScaleUp(destNumSamples / numSamples);
            }
        }


        /// <summary>
        /// Internal method to scale up an image.
        /// 
        /// XXX - should probably use some better algorithm here.
        /// </summary>
        /// <param name="scaleFactor"></param>
        /// <returns></returns>
        protected MapBuffer ScaleUp(int scaleFactor)
        {
            int destSize = scaleFactor * numSamples;
            MapBuffer dest = NewBuffer(map, destSize, metersPerSample / scaleFactor);

            for (int z = 0; z < numSamples; z++)
            {
                for (int x = 0; x < numSamples; x++)
                {
                    uint destValue = GetValue(x, z);
                    dest.Fill(destValue, x * scaleFactor, z * scaleFactor, scaleFactor);
                }
            }
            return dest;
        }

        protected MapBuffer ScaleDown(int scaleFactor)
        {
            int destSize = numSamples / scaleFactor;
            MapBuffer dest = NewBuffer(map, destSize, metersPerSample * scaleFactor);

            for (int z = 0; z < destSize; z++)
            {
                for (int x = 0; x < destSize; x++)
                {
                    uint destValue = AverageValue(x * scaleFactor, z * scaleFactor, scaleFactor);
                    dest.SetValue(x, z, destValue);
                }
            }
            return dest;
        }

        public int NumSamples
        {
            get
            {
                return numSamples;
            }
        }

        public int MetersPerSample
        {
            get
            {
                return metersPerSample;
            }
        }

        public bool Dirty
        {
            get
            {
                return dirty;
            }
            set
            {
                dirty = value;
            }
        }

        public abstract int BytesPerSample
        {
            get;
        }

        public abstract uint MaxSampleValue
        {
            get;
        }
    }

    public class MapBuffer16 : MapBuffer
    {
        protected ushort[] buffer;

        public MapBuffer16(WorldMap map, int numSamples, int metersPerSample)
            : base(map, numSamples, metersPerSample)
        {
        }

        public MapBuffer16(WorldMap map, string filename)
            : base(map, filename)
        {
        }

        public MapBuffer16(WorldMap map, Image src, int metersPerSample, int sx, int sz, 
            float srcMinHeight, float srcMaxHeight, int size, float destMinHeight, float destMaxHeight)
            : base(map, src, metersPerSample, sx, sz, srcMinHeight, srcMaxHeight, size, destMinHeight, destMaxHeight)
        {
        }

        protected override void InitBuffer()
        {
            buffer = new ushort[numSamples * numSamples];
        }

        protected override void LoadRaw(string filename)
        {
            int w, h;

            buffer = Image.LoadRawL16(filename, out w, out h);

            Debug.Assert(w == h);

            numSamples = w;
        }

        protected override void SaveRaw(string filename)
        {

            Image.SaveRawL16(filename, numSamples, numSamples, buffer);
        }

        protected override MapBuffer NewBuffer(WorldMap map, int numSamples, int metersPerSample)
        {
            return new MapBuffer16(map, numSamples, metersPerSample);
        }

        public override uint GetValue(int x, int z)
        {
            return buffer[x + z * numSamples];
        }

        public override void SetValue(int x, int z, uint value)
        {
            buffer[x + z * numSamples] = (ushort)value;
            dirty = true;
        }

        /// <summary>
        /// Copies one buffer into an area of another.
        /// Source buffer must fit within bounds of dest buffer when placed at dest offset.
        /// Source and dest buffers must be of the same type (MapBuffer16).
        /// Source and dest buffers must have the same meters per sample.
        /// </summary>
        /// <param name="destX">X offset within dest to place src</param>
        /// <param name="destZ">Z offset within dest to place src</param>
        /// <param name="src">Source map</param>
        public override void Copy(int destX, int destZ, MapBuffer src)
        {
            Debug.Assert(src is MapBuffer16);
            Debug.Assert(src.MetersPerSample == metersPerSample);
            Debug.Assert((destX + src.NumSamples) <= numSamples);
            Debug.Assert((destZ + src.NumSamples) <= numSamples);

            MapBuffer16 src16 = src as MapBuffer16;

            for (int z = 0; z < src.NumSamples; z++)
            {
                int srcoff = z * src.NumSamples;
                int destoff = ((destZ + z) * numSamples) + destX;
                for (int x = 0; x < src.NumSamples; x++)
                {
                    buffer[destoff] = src16.buffer[srcoff];
                    srcoff++;
                    destoff++;
                }
            }

            dirty = true;
        }

        public override void Fill(uint value)
        {
            for (int i = 0; i < numSamples * numSamples; i++)
            {
                buffer[i] = (ushort)value;
            }
            dirty = true;
        }

        public override void Fill(uint value, int x, int z, int size)
        {
            for (int dz = z; dz < (z + size); dz++)
            {
                int rowoff = dz * numSamples;
                for (int dx = x; dx < (x + size); dx++)
                {
                    buffer[rowoff + dx] = (ushort)value;
                }
            }
        }

        protected override uint AverageValue(int x, int z, int size)
        {
            uint tmpValue = 0;
            for (int sz = z; sz < (z + size); sz++)
            {
                int rowoff = sz * numSamples;
                for (int sx = x; sx < (x + size); sx++)
                {
                    tmpValue += buffer[rowoff + sx];
                }
            }

            return tmpValue / (uint)(size * size);
        }

        protected override System.Drawing.Color AverageColor(int x, int z, int size)
        {
            uint tmpValue = 0;
            for (int sz = z; sz < (z + size); sz++)
            {
                int rowoff = sz * numSamples;
                for (int sx = x; sx < (x + size); sx++)
                {
                    tmpValue += buffer[rowoff + sx];
                }
            }

            // take the top byte of the 16 bit result color
            tmpValue = ( tmpValue / (uint)(size * size) ) >> 8;

            Debug.Assert(tmpValue < 256);

            return System.Drawing.Color.FromArgb(255, (int)tmpValue, (int)tmpValue, (int)tmpValue);
        }

        public override int BytesPerSample
        {
            get
            {
                return 2;
            }
        }

        public override uint MaxSampleValue
        {
            get
            {
                return ushort.MaxValue;
            }
        }
    }

    public class MapBuffer32 : MapBuffer
    {
        protected uint[] buffer;

        public MapBuffer32(WorldMap map, int numSamples, int metersPerSample)
            : base(map, numSamples, metersPerSample)
        {
        }

        public MapBuffer32(WorldMap map, string filename)
            : base(map, filename)
        {
        }

        public MapBuffer32(WorldMap map, Image src, int metersPerSample, int sx, int sz, 
            float srcMinHeight, float srcMaxHeight, int size, float destMinHeight, float destMaxHeight)
            : base(map, src, metersPerSample, sx, sz, srcMinHeight, srcMaxHeight, size, destMinHeight, destMaxHeight)
        {
        }

        public MapBuffer32(WorldMap map, Image src, int metersPerSample, int sx, int sz, int size)
            : base(map, src, metersPerSample, sx, sz, size)
        {
        }

        protected override void InitBuffer()
        {
            buffer = new uint[numSamples * numSamples];
        }

        protected override void LoadRaw(string filename)
        {
            int w, h;

            buffer = Image.LoadRawL32(filename, out w, out h);

            Debug.Assert(w == h);

            numSamples = w;
        }

        protected override void SaveRaw(string filename)
        {

            Image.SaveRawL32(filename, numSamples, numSamples, buffer);
        }

        protected override MapBuffer NewBuffer(WorldMap map, int numSamples, int metersPerSample)
        {
            return new MapBuffer32(map, numSamples, metersPerSample);
        }

        public override uint GetValue(int x, int z)
        {
            return buffer[x + z * numSamples];
        }

        public override void SetValue(int x, int z, uint value)
        {
            buffer[x + z * numSamples] = value;
            dirty = true;
        }

        /// <summary>
        /// Copies one buffer into an area of another.
        /// Source buffer must fit within bounds of dest buffer when placed at dest offset.
        /// Source and dest buffers must be of the same type (MapBuffer16).
        /// Source and dest buffers must have the same meters per sample.
        /// </summary>
        /// <param name="destX">X offset within dest to place src</param>
        /// <param name="destZ">Z offset within dest to place src</param>
        /// <param name="src">Source map</param>
        public override void Copy(int destX, int destZ, MapBuffer src)
        {
            Debug.Assert(src is MapBuffer32);
            Debug.Assert(src.MetersPerSample == metersPerSample);
            Debug.Assert((destX + src.NumSamples) <= numSamples);
            Debug.Assert((destZ + src.NumSamples) <= numSamples);

            MapBuffer32 src32 = src as MapBuffer32;

            for (int z = 0; z < src.NumSamples; z++)
            {
                int srcoff = z * src.NumSamples;
                int destoff = ((destZ + z) * numSamples) + destX;
                for (int x = 0; x < src.NumSamples; x++)
                {
                    buffer[destoff] = src32.buffer[srcoff];
                    srcoff++;
                    destoff++;
                }
            }

            dirty = true;
        }

        public override void Fill(uint value)
        {
            for (int i = 0; i < numSamples * numSamples; i++)
            {
                buffer[i] = value;
            }
            dirty = true;
        }

        public override void Fill(uint value, int x, int z, int size)
        {
            for (int dz = z; dz < (z + size); dz++)
            {
                int rowoff = dz * numSamples;
                for (int dx = x; dx < (x + size); dx++)
                {
                    buffer[rowoff + dx] = value;
                }
            }
        }

        protected override uint AverageValue(int x, int z, int size)
        {
            ulong tmpValue = 0;
            for (int sz = z; sz < (z + size); sz++)
            {
                int rowoff = sz * numSamples;
                for (int sx = x; sx < (x + size); sx++)
                {
                    tmpValue += buffer[rowoff + sx];
                }
            }

            return (uint) (tmpValue / (ulong)(size * size));
        }

        protected override System.Drawing.Color AverageColor(int x, int z, int size)
        {
            ulong tmpValue = 0;
            for (int sz = z; sz < (z + size); sz++)
            {
                int rowoff = sz * numSamples;
                for (int sx = x; sx < (x + size); sx++)
                {
                    tmpValue += buffer[rowoff + sx];
                }
            }

            // take the top byte of the 16 bit result color
            uint retValue = (uint)((tmpValue / (ulong)(size * size)) >> 24);

            Debug.Assert(retValue < 256);

            return System.Drawing.Color.FromArgb(255, (int)retValue, (int)retValue, (int)retValue);
        }

        public override int BytesPerSample
        {
            get
            {
                return 4;
            }
        }

        public override uint MaxSampleValue
        {
            get
            {
                return uint.MaxValue;
            }
        }
    }


    public class MapBufferARGB : MapBuffer32
    {
        public MapBufferARGB(WorldMap map, int numSamples, int metersPerSample)
            : base(map, numSamples, metersPerSample)
        {
        }

        public MapBufferARGB(WorldMap map, string filename)
            : base(map, filename)
        {
        }

        public MapBufferARGB(WorldMap map, Image src, int metersPerSample, int sx, int sz, int size)
            : base(map, src, metersPerSample, sx, sz, size)
        {
        }

        protected override void LoadRaw(string filename)
        {
            int w, h;

            buffer = Image.LoadRawARGB(filename, out w, out h);

            Debug.Assert(w == h);

            numSamples = w;
        }

        protected override void SaveRaw(string filename)
        {

            Image.SaveRawARGB(filename, numSamples, numSamples, buffer);
        }

        protected override MapBuffer NewBuffer(WorldMap map, int numSamples, int metersPerSample)
        {
            return new MapBufferARGB(map, numSamples, metersPerSample);
        }

        protected override System.Drawing.Color AverageColor(int x, int z, int size)
        {
            uint alphaAccum = 0;
            uint redAccum = 0;
            uint greenAccum = 0;
            uint blueAccum = 0;

            for (int sz = z; sz < (z + size); sz++)
            {
                int rowoff = sz * numSamples;
                for (int sx = x; sx < (x + size); sx++)
                {
                    uint pixel = buffer[rowoff + sx];
                    uint alpha = pixel >> 24;
                    uint red = (pixel >> 16) & 0xff;
                    uint green = (pixel >> 8) & 0xff;
                    uint blue = (pixel & 0xff);

                    alphaAccum += alpha;
                    redAccum += red;
                    greenAccum += green;
                    blueAccum += blue;
                }
            }

            uint samples = (uint)(size * size);

            // take the top byte of the 16 bit result color
            uint alphaValue = alphaAccum / samples;
            uint redValue = redAccum / samples;
            uint greenValue = greenAccum / samples;
            uint blueValue = blueAccum / samples;

            Debug.Assert(alphaValue < 256);
            Debug.Assert(redValue < 256);
            Debug.Assert(greenValue < 256);
            Debug.Assert(blueValue < 256);

            if (alphaValue > 0)
            {
                return System.Drawing.Color.FromArgb(255, (int)alphaValue, (int)alphaValue, (int)alphaValue);
            }
            else
            {
                return System.Drawing.Color.FromArgb(255, (int)redValue, (int)greenValue, (int)blueValue);
            }
        }
    }
}

