using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Multiverse.Lib.DXTUtil
{
    /// <summary>
    /// This class holds the data for a single compressed mip level.
    /// This is the abstract base class which the various DXT format
    ///   mip classes derive from.
    /// </summary>
    public abstract class DXTMip
    {
        protected int width;
        protected int height;
        protected byte [] buffer;
        protected readonly int blockSize;

        public DXTMip(int width, int height, int blockSize)
        {
            this.width = width;
            this.height = height;
            this.blockSize = blockSize;

            int blockWidth = width / 4;
            int blockHeight = height / 4;

            int size = blockWidth * blockHeight * blockSize;
            buffer = new byte[size];
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return buffer;
            }
        }
    }

    public class DXT1Mip : DXTMip
    {
        public DXT1Mip(int width, int height)
            : base(width, height, 8)
        {
        }
    }

    public class DXT5Mip : DXTMip
    {
        public DXT5Mip(int width, int height)
            : base(width, height, 16)
        {
        }
    }

    public class Image
    {
        public int Width;
        public int Height;
        public int NumFaces = 1;
        public int NumMipMaps = 1;
        public byte[] Data;

        public Image(int w, int h, byte [] data)
        {
            Width = w;
            Height = h;
            Data = data;
        }
    }

    public class DXTStream : Stream
    {
        protected long position = 0;
        protected List<byte[]> buffers = new List<byte[]>();
        protected long len;
        
        public DXTStream(DXT1Image image, int topMip)
        {
            byte[] header = image.BuildHeader(topMip);
            buffers.Add(header);
            len = header.Length;
            for (int i = topMip; i < image.CompressedMips.Length; i++ )
            {
                byte[] buffer = image.CompressedMips[i].Buffer;
                buffers.Add(buffer);
                len += buffer.Length;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length
        {
            get
            {
                return len;
            }
        }

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                if (position > len)
                {
                    position = len;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int retcount;
            if (len - position < count)
            {
                count = (int)(len - position);
            }

            retcount = count;

            if (count > 0)
            {

                long curBufferOff = position;
                int curBufferNum = 0;

                while (curBufferOff >= buffers[curBufferNum].Length)
                {
                    curBufferOff -= buffers[curBufferNum].Length;
                    curBufferNum++;
                }

                while (count > 0)
                {
                    byte[] srcBuf = buffers[curBufferNum];
                    int leftInbuffer = (int)(srcBuf.Length - curBufferOff);

                    if (leftInbuffer > count)
                    {
                        Buffer.BlockCopy(srcBuf, (int)curBufferOff, buffer, offset, count);
                        offset += count;
                        count = 0;
                    }
                    else
                    {
                        Buffer.BlockCopy(srcBuf, (int)curBufferOff, buffer, offset, leftInbuffer);
                        curBufferOff = 0;
                        curBufferNum++;
                        offset += leftInbuffer;
                        count -= leftInbuffer;
                    }
                }
            }
            position = position + retcount;
            return retcount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.End:
                    position = len + offset;
                    break;
            }
            return position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

    }

    public class DXT1Image
    {
        protected int width;
        protected int height;
        protected DXT1Mip[] compressedMips;

        private static readonly int srcPixelSize = 4;

        [DllImport("FastDXT.dll", CharSet = CharSet.Auto)]
        private static extern void CompressImageDXT1(IntPtr inBuf,  IntPtr outBuf, int width, int height );

        public DXT1Image(Image source)
        {
            // make sure width and height are divisible by 4
            Debug.Assert((source.Width & 0x3) == 0);
            Debug.Assert((source.Height & 0x3) == 0);

            // width and height are powers of 2
            Debug.Assert(isPowerOf2(source.Width));
            Debug.Assert(isPowerOf2(source.Height));

            // only support RGB 24-bit images for now
            //Debug.Assert(source.Format == PixelFormat.R8G8B8);

            // only normal textures
            Debug.Assert(source.NumFaces == 1);
            Debug.Assert(source.NumMipMaps == 1);

            width = source.Width;
            height = source.Height;

            int mipWidth = width;
            int mipHeight = height;
            int mipLevels;

            if (width > height)
            {
                mipLevels = log2(width) - 1;
            }
            else
            {
                mipLevels = log2(height) - 1;
            }

            compressedMips = new DXT1Mip[mipLevels];

            // start by using the source image data for the first iterations.
            // later iterations will use new allocated buffers containing scaled pixels
            byte[] rawBuffer = source.Data;
            for( int mipLevel = 0; mipLevel < mipLevels; mipLevel++)
            {
                compressedMips[mipLevel] = new DXT1Mip(mipWidth, mipHeight);

                //CompressMip(compressedMips[mipLevel], rawBuffer);
                unsafe
                {
                    byte [] outBuffer = compressedMips[mipLevel].Buffer;
                    GCHandle inHandle = GCHandle.Alloc(rawBuffer, GCHandleType.Pinned);
                    GCHandle outHandle = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);
                    IntPtr pIn = inHandle.AddrOfPinnedObject();
                    IntPtr pOut = outHandle.AddrOfPinnedObject();

                    CompressImageDXT1(pIn, pOut, mipWidth, mipHeight);

                    inHandle.Free();
                    outHandle.Free();
                }

                mipWidth = mipWidth / 2;
                mipHeight = mipHeight / 2;

                // create a new uncompressed buffer by scaling the last buffer
                // to the new mip size
                rawBuffer = scaleRawHalfBox(mipWidth, mipHeight, rawBuffer);
            }

        }

        public DXT1Mip[] CompressedMips
        {
            get
            {
                return compressedMips;
            }
        }

        public Stream GetStream(int topMip)
        {
            return new DXTStream(this, topMip);
        }

        public void ToStream(Stream s)
        {
            ToStream(s, 0);
        }

        public void ToStream(Stream s, int topMip)
        {
            byte[] header = BuildHeader(topMip);
            s.Write(header, 0, header.Length);
            for (int i = 0; i < compressedMips.Length; i++)
            {
                s.Write(compressedMips[i].Buffer, 0, compressedMips[i].Buffer.Length);
            }
        }

        protected void SetHeaderDword(byte[] header, int off, uint value)
        {
            int offset = off * 4;
            header[offset] = (byte)(value & 0xff);
            header[offset + 1] = (byte)((value >> 8) & 0xff);
            header[offset + 2] = (byte)((value >> 16) & 0xff);
            header[offset + 3] = (byte)((value >> 24) & 0xff);
        }

        public byte [] BuildHeader(int topMip)
        {
            byte[] header = new byte[128];
            SetHeaderDword(header, 0, (uint)('D' | 'D' << 8 | 'S' << 16 | ' ' << 24));
            SetHeaderDword(header, 1, 124);
            SetHeaderDword(header, 2, 0x1 | 0x2 | 0x4 | 0x1000 | 0x20000 | 0x80000);
            SetHeaderDword(header, 3, (uint)compressedMips[topMip].Height);
            SetHeaderDword(header, 4, (uint)compressedMips[topMip].Width);
            SetHeaderDword(header, 5, (uint)compressedMips[topMip].Buffer.Length);
            SetHeaderDword(header, 6, 0);
            SetHeaderDword(header, 7, (uint)(compressedMips.Length - topMip));
            SetHeaderDword(header, 19, 32);
            SetHeaderDword(header, 20, 0x4);
            SetHeaderDword(header, 21, (uint)('D' | 'X' << 8 | 'T' << 16 | '1' << 24));
            SetHeaderDword(header, 27, 0x8 | 0x1000 | 0x400000);

            return header;
        }

        protected void CompressMip(DXT1Mip mip, byte[] buffer)
        {
            Debug.Assert(buffer.Length == (mip.Width * mip.Height * srcPixelSize));

            byte[] block = new byte[64];

            int blocksWidth = mip.Width / 4;
            int blocksHeight = mip.Height / 4;

            int stride = mip.Width * srcPixelSize;
            int copyLen = 4 * srcPixelSize;

            int dxtOffset = 0;

            for (int y = 0; y < mip.Height; y += 4)
            {
                int lineOffset = y * stride;
                for (int x = 0; x < mip.Width; x += 4)
                {
                    int offset = lineOffset + x * srcPixelSize;
                    int destOffset = 0;

                    // fill the block with data from the source image
                    for (int line = 0; line < 4; line++)
                    {
                        Buffer.BlockCopy(buffer, offset, block, destOffset, copyLen);
                        offset += stride;
                        destOffset += copyLen;
                    }
                    CompressBlockDXT1Opaque(block, mip.Buffer, dxtOffset);
                    dxtOffset += 8;
                }
            }

        }

        protected void CompressBlockDXT1Opaque(byte [] block, byte [] output, int outOffset)
        {
            byte [] minColor = new byte[3];
            byte [] maxColor = new byte[3];
            GetMinMaxColors(block, minColor, maxColor);
            ushort min565 = ColorTo565(minColor[0], minColor[1], minColor[2]);
            ushort max565 = ColorTo565(maxColor[0], maxColor[1], maxColor[2]);

            output[outOffset] = (byte)(max565 & 0xff);
            output[outOffset + 1] = (byte)((max565 >> 8) & 0xff);
            output[outOffset + 2] = (byte)(min565 & 0xff);
            output[outOffset + 3] = (byte)((min565 >> 8) & 0xff);

            uint indices = GetColorIndices(block, minColor, maxColor);

            output[outOffset + 4] = (byte)(indices & 0xff);
            output[outOffset + 5] = (byte)((indices >> 8) & 0xff);
            output[outOffset + 6] = (byte)((indices >> 16) & 0xff);
            output[outOffset + 7] = (byte)((indices >> 24) & 0xff);
        }

        protected void GetMinMaxColors(byte[] colorBlock, byte[] minColor, byte[] maxColor)
        {
            int i;
            int minR = 255;
            int minG = 255;
            int minB = 255;
            int maxR = 0;
            int maxG = 0;
            int maxB = 0;

            for (i = 0; i < 16; i++)
            {
                if (colorBlock[i * srcPixelSize + 0] < minR) { minR = colorBlock[i * srcPixelSize + 0]; }
                if (colorBlock[i * srcPixelSize + 1] < minG) { minG = colorBlock[i * srcPixelSize + 1]; }
                if (colorBlock[i * srcPixelSize + 2] < minB) { minB = colorBlock[i * srcPixelSize + 2]; }
                if (colorBlock[i * srcPixelSize + 0] > maxR) { maxR = colorBlock[i * srcPixelSize + 0]; }
                if (colorBlock[i * srcPixelSize + 1] > maxG) { maxG = colorBlock[i * srcPixelSize + 1]; }
                if (colorBlock[i * srcPixelSize + 2] > maxB) { maxB = colorBlock[i * srcPixelSize + 2]; }
            }
            int insetR = (maxR - minR) >> 4;
            int insetG = (maxG - minG) >> 4;
            int insetB = (maxB - minB) >> 4;
            minR = (minR + insetR <= 255) ? minR + insetR : 255;
            minG = (minG + insetG <= 255) ? minG + insetG : 255;
            minB = (minB + insetB <= 255) ? minB + insetB : 255;
            maxR = (maxR >= insetR) ? maxR - insetR : 0;
            maxG = (maxG >= insetG) ? maxG - insetG : 0;
            maxB = (maxB >= insetB) ? maxB - insetB : 0;

            minColor[0] = (byte)minR;
            minColor[1] = (byte)minG;
            minColor[2] = (byte)minB;
            maxColor[0] = (byte)maxR;
            maxColor[1] = (byte)maxG;
            maxColor[2] = (byte)maxB;
        }

        uint GetColorIndices(byte[] colorBlock, byte[] minColor, byte[] maxColor)
        {
            byte C565_5_MASK = 0xf8;
            byte C565_6_MASK = 0xfc;

            byte[,] colors = new byte[4, 3];
            uint result = 0;
            colors[0, 0] = (byte)((maxColor[0] & C565_5_MASK) | (maxColor[0] >> 5));
            colors[0, 1] = (byte)((maxColor[1] & C565_6_MASK) | (maxColor[1] >> 6));
            colors[0, 2] = (byte)((maxColor[2] & C565_5_MASK) | (maxColor[2] >> 5));
            colors[1, 0] = (byte)((minColor[0] & C565_5_MASK) | (minColor[0] >> 5));
            colors[1, 1] = (byte)((minColor[1] & C565_6_MASK) | (minColor[1] >> 6));
            colors[1, 2] = (byte)((minColor[2] & C565_5_MASK) | (minColor[2] >> 5));
            colors[2, 0] = (byte)((2 * colors[0, 0] + 1 * colors[1, 0]) / 3);
            colors[2, 1] = (byte)((2 * colors[0, 1] + 1 * colors[1, 1]) / 3);
            colors[2, 2] = (byte)((2 * colors[0, 2] + 1 * colors[1, 2]) / 3);
            colors[3, 0] = (byte)((1 * colors[0, 0] + 2 * colors[1, 0]) / 3);
            colors[3, 1] = (byte)((1 * colors[0, 1] + 2 * colors[1, 1]) / 3);
            colors[3, 2] = (byte)((1 * colors[0, 2] + 2 * colors[1, 2]) / 3);
            for (int i = 15; i >= 0; i--)
            {
                int c0 = colorBlock[i * srcPixelSize + 0];
                int c1 = colorBlock[i * srcPixelSize + 1];
                int c2 = colorBlock[i * srcPixelSize + 2];

                uint d0 = (uint)(Math.Abs(colors[0, 0] - c0) + Math.Abs(colors[0, 1] - c1) + Math.Abs(colors[0, 2] - c2));
                uint d1 = (uint)(Math.Abs(colors[1, 0] - c0) + Math.Abs(colors[1, 1] - c1) + Math.Abs(colors[1, 2] - c2));
                uint d2 = (uint)(Math.Abs(colors[2, 0] - c0) + Math.Abs(colors[2, 1] - c1) + Math.Abs(colors[2, 2] - c2));
                uint d3 = (uint)(Math.Abs(colors[3, 0] - c0) + Math.Abs(colors[3, 1] - c1) + Math.Abs(colors[3, 2] - c2));

                uint b0 = (d0 > d3) ? 1u : 0u;
                uint b1 = (d1 > d2) ? 1u : 0u;
                uint b2 = (d0 > d2) ? 1u : 0u;
                uint b3 = (d1 > d3) ? 1u : 0u;
                uint b4 = (d2 > d3) ? 1u : 0u;
                uint x0 = b1 & b2;
                uint x1 = b0 & b3;
                uint x2 = b0 & b4;
                result |= (x2 | ((x0 | x1) << 1)) << (i << 1);
            }
            return(result);
        }

        protected ushort ColorTo565(byte r, byte g, byte b)
        {
            return (ushort)(((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3));
        }
        /// <summary>
        /// Scale a raw uncompressed pixel buffer, which has 3 bytes per pixel, to half size in width and height.
        /// Scaling is done by using a simple box filter, which means that we just take the 2x2 pixel block and
        ///   average the color to get the result.
        /// </summary>
        /// <param name="w">width of the destination (half source width)</param>
        /// <param name="h">height of the destination (half source height)</param>
        /// <param name="src">byte array of the source image</param>
        /// <returns>byte array containing the scaled down image</returns>
        private static byte[] scaleRawHalfBox(int w, int h, byte[] src)
        {
            byte [] dest = new byte[w * h * srcPixelSize];

            int destOff = 0;
            int sourceOff = 0;
            int sourceStride = w * 2 * srcPixelSize;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // scale red
                    int tmp = ((int)src[sourceOff] + (int)src[sourceOff + srcPixelSize] + (int)src[sourceOff + sourceStride] + (int)src[sourceOff + sourceStride + srcPixelSize])/4;
                    dest[destOff] = (byte)tmp;
                    destOff++;
                    sourceOff++;

                    // scale green
                    tmp = ((int)src[sourceOff] + (int)src[sourceOff + srcPixelSize] + (int)src[sourceOff + sourceStride] + (int)src[sourceOff + sourceStride + srcPixelSize]) / 4;
                    dest[destOff] = (byte)tmp;
                    destOff++;
                    sourceOff++;

                    // scale blue
                    tmp = ((int)src[sourceOff] + (int)src[sourceOff + srcPixelSize] + (int)src[sourceOff + sourceStride] + (int)src[sourceOff + sourceStride + srcPixelSize]) / 4;
                    dest[destOff] = (byte)tmp;
                    destOff++;
                    sourceOff++;

                    // scale alpha
                    tmp = ((int)src[sourceOff] + (int)src[sourceOff + srcPixelSize] + (int)src[sourceOff + sourceStride] + (int)src[sourceOff + sourceStride + srcPixelSize]) / 4;
                    dest[destOff] = (byte)tmp;
                    destOff++;
                    sourceOff = sourceOff + 1 + srcPixelSize;
                }
                // skip every other line
                sourceOff += sourceStride;
            }

            return dest;
        }

        private static int log2(int value)
        {
            int ret = 0;

            if ((value & 0xaaaa) != 0)
            {
                ret = 1;
            }
            if ((value & 0xcccc) != 0)
            {
                ret += 2;
            }
            if ((value & 0xf0f0) != 0)
            {
                ret += 4;
            }
            if ((value & 0xff00) != 0)
            {
                ret += 8;
            }

            return ret;
        }

        private static bool isPowerOf2(int value)
        {
            return (value & (~value + 1)) == value;  //~value+1 equals a two's complement -value
        }
    }
}
