using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace Multiverse.Lib
{
    public abstract class DDSFile
    {
        // values from dds main header
        protected uint magic;
        protected uint headerSize;
        protected uint headerFlags;
        protected uint height;
        protected uint width;
        protected uint linearSize;
        protected uint depth;
        protected uint mipMapCount;
        protected uint caps;
        protected uint caps2;
        protected uint caps3;
        protected uint caps4;

        protected uint pfSize;
        protected uint pfFlags;
        protected uint pfFourCC;
        protected uint pfRGBBitCount;
        protected uint pfRBitMask;
        protected uint pfGBitMask;
        protected uint pfBBitMask;
        protected uint pfABitMask;

        protected uint pixelBytes;
        protected uint pixelBits;
        protected uint AMask;
        protected int AShift;
        protected uint RMask;
        protected int RShift;
        protected uint GMask;
        protected int GShift;
        protected uint BMask;
        protected int BShift;

        [Flags]
        public enum DDSHeaderFlags
        {
            None = 0x0,
            Caps = 0x1,
            Height = 0x2,
            Width = 0x4,
            Pitch = 0x10,
            PixelFormat = 0x1000,
            MipMapCount = 0x20000,
            LinearSize = 0x80000,
            Depth = 0x800000
        }

        [Flags]
        public enum PFFlags
        {
            None = 0x0,
            AlphaPixel = 0x1,
            FourCC = 0x4,
            RGB = 0x40,
            Luminance = 0x20000
        }

        [Flags]
        public enum DDSCaps
        {
            None = 0x0,
            Complex = 0x8,
            Texture = 0x1000,
            MipMap = 0x400000
        }

        protected abstract void SizeInit();

        public DDSFile(string filename)
        {
            SizeInit();

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            ReadHeader(br);

            CheckHeader();

            ReadBody(br);

            DumpHeader();

            br.Close();
        }

        public DDSFile(int w, int h)
        {
            SizeInit();

            magic = FourCC("DDS ");
            headerSize = 124;
            headerFlags = (uint)(DDSHeaderFlags.Caps | DDSHeaderFlags.Height | DDSHeaderFlags.Width | DDSHeaderFlags.PixelFormat);
            height = (uint)h;
            width = (uint)w;
            linearSize = width * pixelBytes;
            depth = 0;
            mipMapCount = 1;
            caps = (uint)DDSCaps.Texture;
            caps2 = 0;
            caps3 = 0;
            caps4 = 0;

            pfSize = 32;
            pfFlags = (uint)(PFFlags.AlphaPixel | PFFlags.RGB);
            pfFourCC = FourCC("    ");
            pfRGBBitCount = pixelBits;
            pfRBitMask = RMask;
            pfGBitMask = GMask;
            pfBBitMask = BMask;
            pfABitMask = AMask;

            // subclasses must allocate the image data
        }

        public void Save(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            WriteHeader(bw);
            WriteBody(bw);

            bw.Close();
        }

        protected uint FourCC(string str)
        {
            Debug.Assert(str.Length == 4);
            return (uint)(str[0] | str[1] << 8 | str[2] << 16 | str[3] << 24);
        }

        protected string FourCCToStr(uint fourCC)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((char)(fourCC & 0xff));
            sb.Append((char)((fourCC >> 8) & 0xff));
            sb.Append((char)((fourCC >> 16) & 0xff));
            sb.Append((char)((fourCC >> 24) & 0xff));

            return sb.ToString();
        }

        protected bool CheckHeader()
        {
            bool ok = true;

            if (magic != FourCC("DDS "))
            {
                Console.WriteLine("Error: File not DDS format");
                ok = false;
            }
            if (headerSize != 124)
            {
                Console.WriteLine("Error: Header length wrong");
                ok = false;
            }
            if (depth != 0)
            {
                Console.WriteLine("Error: Depth not 0");
                ok = false;
            }
            if (mipMapCount != 1)
            {
                Console.WriteLine("Error: MipMapCount not 1");
                ok = false;
            }
            if (headerSize != 124)
            {
                Console.WriteLine("Error: Header length wrong");
                ok = false;
            }
            if (FourCCToStr(pfFourCC).StartsWith("DXT"))
            {
                Console.WriteLine("Error: DXT compression not supported!");
                ok = false;
            }
            if (pfRGBBitCount != pixelBits)
            {
                Console.WriteLine("Error: non-{0} bit pixel size", pixelBits);
                ok = false;
            }

            return ok;
        }

        protected void DumpHeader()
        {
            Console.WriteLine("Magic: {0}", FourCCToStr(magic));
            Console.WriteLine("HeaderSize: {0}", headerSize);
            Console.WriteLine("HeaderFlags: 0x{0:x}", headerFlags);
            Console.WriteLine("Height: {0}", height);
            Console.WriteLine("Width: {0}", width);
            Console.WriteLine("LinearSize: {0}", linearSize);
            Console.WriteLine("Depth: {0}", depth);
            Console.WriteLine("MipMapCount: {0}", mipMapCount);

            Console.WriteLine("pfSize: {0}", pfSize);
            Console.WriteLine("pfFlags: 0x{0:x}", pfFlags);
            Console.WriteLine("pfFourCC: {0}", FourCCToStr(pfFourCC));
            Console.WriteLine("pfRGBBitCount: {0}", pfRGBBitCount);
            Console.WriteLine("pfRBitMask: 0x{0:x}", pfRBitMask);
            Console.WriteLine("pfGBitMask: 0x{0:x}", pfGBitMask);
            Console.WriteLine("pfBBitMask: 0x{0:x}", pfBBitMask);
            Console.WriteLine("pfABitMask: 0x{0:x}", pfABitMask);
            Console.WriteLine("caps: 0x{0:x}", caps);
            Console.WriteLine("caps2: 0x{0:x}", caps2);
            Console.WriteLine("caps3: 0x{0:x}", caps3);
            Console.WriteLine("caps4: 0x{0:x}", caps4);

        }

        protected void ReadHeader(BinaryReader br)
        {
            uint junk;

            magic = br.ReadUInt32();
            headerSize = br.ReadUInt32();
            headerFlags = br.ReadUInt32();
            height = br.ReadUInt32();
            width = br.ReadUInt32();
            linearSize = br.ReadUInt32();
            depth = br.ReadUInt32();
            mipMapCount = br.ReadUInt32();

            // read 11 reserved DWORDS
            for (int i = 0; i < 11; i++)
            {
                junk = br.ReadUInt32();
            }

            pfSize = br.ReadUInt32();
            pfFlags = br.ReadUInt32();
            pfFourCC = br.ReadUInt32();
            pfRGBBitCount = br.ReadUInt32();
            pfRBitMask = br.ReadUInt32();
            pfGBitMask = br.ReadUInt32();
            pfBBitMask = br.ReadUInt32();
            pfABitMask = br.ReadUInt32();
            caps = br.ReadUInt32();
            caps2 = br.ReadUInt32();
            caps3 = br.ReadUInt32();
            caps4 = br.ReadUInt32();

            // read final reserved DWORD
            junk = br.ReadUInt32();
        }


        protected void WriteHeader(BinaryWriter bw)
        {
            uint zero = 0;

            bw.Write(magic);
            bw.Write(headerSize);
            bw.Write(headerFlags);
            bw.Write(height);
            bw.Write(width);
            bw.Write(linearSize);
            bw.Write(depth);
            bw.Write(mipMapCount);

            // read 11 reserved DWORDS
            for (int i = 0; i < 11; i++)
            {
                bw.Write(zero);
            }

            bw.Write(pfSize);
            bw.Write(pfFlags);
            bw.Write(pfFourCC);
            bw.Write(pfRGBBitCount);
            bw.Write(pfRBitMask);
            bw.Write(pfGBitMask);
            bw.Write(pfBBitMask);
            bw.Write(pfABitMask);
            bw.Write(caps);
            bw.Write(caps2);
            bw.Write(caps3);
            bw.Write(caps4);

            // read final reserved DWORD
            bw.Write(zero);
        }

        protected abstract void WriteBody(BinaryWriter bw);
        protected abstract void ReadBody(BinaryReader br);

        public abstract uint GetPixel(int x, int y);
        public abstract void SetPixel(int x, int y, uint val);
        protected abstract Color PixelToColor(uint pixel);
        protected abstract uint ColorToPixel(Color c);

        public Color GetColor(int x, int y)
        {
            uint pixel = GetPixel(x, y);
            return PixelToColor(pixel);
        }

        public void SetColor(int x, int y, Color c)
        {
            uint pixel = ColorToPixel(c);
            SetPixel(x, y, pixel);
        }

        public int Width
        {
            get
            {
                return (int)width;
            }
        }

        public int Height
        {
            get
            {
                return (int)height;
            }
        }

        public int BitsPerPixel
        {
            get
            {
                return (int)pfRGBBitCount;
            }
        }

        public static DDSFile LoadFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            for (int i = 0; i < 22; i++)
            {
                uint junk = br.ReadUInt32();
            }
            uint bitsize = br.ReadUInt32();

            br.Close();

            if (bitsize == 16)
            {
                return new DDSFile16(filename);
            }
            else if (bitsize == 32)
            {
                return new DDSFile32(filename);
            }
            else
            {
                throw new Exception("DDSFile.LoadFile(): loading image with unsupported bits per pixel");
            }
        }

        public abstract void Copy(DDSFile src, int srcX, int srcY, int w, int h, int destX, int destY);
    }

    public abstract class DDSFile<T> : DDSFile
    {
        protected T[] imageData;
        

        public DDSFile(string filename) : base(filename)
        {
        }

        public DDSFile(int w, int h) :base(w, h)
        {
            imageData = new T[w * h];
        }


        protected abstract void WritePixelImpl(BinaryWriter bw, T val);

        // write the body of the image
        protected override void WriteBody(BinaryWriter bw)
        {
            uint imageLen = width * height;
            for (uint i = 0; i < imageLen; i++)
            {
                WritePixelImpl(bw, imageData[i]);
            }
        }

        protected abstract T ReadPixelImpl(BinaryReader br);

        protected override void ReadBody(BinaryReader br)
        {
            uint imageLen = width * height;
            imageData = new T[imageLen];
            for (uint i = 0; i < imageLen; i++)
            {
                imageData[i] = ReadPixelImpl(br);
            }
        }

        protected abstract uint CastFromPixelImpl(T val);

        public override uint GetPixel(int x, int y)
        {
            int offset = (int)(y * width + x);
            return CastFromPixelImpl(imageData[offset]);
        }

        protected abstract T CastToPixelImpl(uint val);

        public override void SetPixel(int x, int y, uint val)
        {
            int offset = (int)(y * width + x);
            imageData[offset] = CastToPixelImpl(val);
        }

        public override void Copy(DDSFile inSrc, int srcX, int srcY, int w, int h, int destX, int destY)
        {
            DDSFile<T> src = inSrc as DDSFile<T>;
            if (src == null)
            {
                throw new Exception("DDSFile: attempt to Copy() between different depths");
            }
            if (((srcX + w) > src.width) || ((srcY+h) > src.height) || ((destX + w) > width) || ((destY + h) > height))
            {
                throw new Exception("attempt to Copy() outside bounds");
            }
            for (int y = 0; y < h; y++)
            {
                int srcOff = (srcY + y) * (int)src.width + srcX;
                int destOff = (destY + y) * (int)width + destX;

                for (int x = 0; x < w; x++)
                {
                    imageData[destOff + x] = src.imageData[srcOff + x];
                }
            }
        }


    }

    public class DDSFile32 : DDSFile<uint>
    {
        public DDSFile32(string filename)
            : base(filename)
        {
        }

        public DDSFile32(int w, int h)
            : base(w, h)
        {
        }

        protected override void SizeInit()
        {
            pixelBytes = 4;
            pixelBits = pixelBytes * 8;
            AMask = 0xff000000;
            AShift = 24;
            RMask = 0xff0000;
            RShift = 16;
            GMask = 0xff00;
            GShift = 8;
            BMask = 0xff;
            BShift = 0;
        }

        protected override uint ReadPixelImpl(BinaryReader br)
        {
            return br.ReadUInt32();
        }

        protected override void WritePixelImpl(BinaryWriter bw, uint val)
        {
            bw.Write(val);
        }

        protected override uint CastToPixelImpl(uint val)
        {
            return val;
        }

        protected override uint CastFromPixelImpl(uint val)
        {
            return val;
        }

        protected override Color PixelToColor(uint pixel)
        {
            int alpha = (int)((pixel & AMask) >> AShift);
            int red = (int)((pixel & RMask) >> RShift);
            int green = (int)((pixel & GMask) >> GShift);
            int blue = (int)((pixel & BMask) >> BShift);

            return Color.FromArgb(alpha, red, green, blue);
        }

        protected override uint ColorToPixel(Color c)
        {
            uint pixel = (uint)((((uint)c.A << AShift) & AMask) | (((uint)c.R << RShift) & RMask) | (((uint)c.G << GShift) & GMask) | (((uint)c.B << BShift) & BMask)); 

            return pixel;
        }
    }

    public class DDSFile16 : DDSFile<ushort>
    {
        public DDSFile16(string filename)
            : base(filename)
        {
        }

        public DDSFile16(int w, int h)
            : base(w, h)
        {
        }

        protected override void SizeInit()
        {
            pixelBytes = 2;
            pixelBits = pixelBytes * 8;
            AMask = 0xf000;
            AShift = 12;
            RMask = 0xf00;
            RShift = 8;
            GMask = 0xf0;
            GShift = 4;
            BMask = 0xf;
            BShift = 0;
        }

        protected override ushort ReadPixelImpl(BinaryReader br)
        {
            return br.ReadUInt16();
        }

        protected override void WritePixelImpl(BinaryWriter bw, ushort val)
        {
            bw.Write(val);
        }

        protected override ushort CastToPixelImpl(uint val)
        {
            return (ushort)val;
        }

        protected override uint CastFromPixelImpl(ushort val)
        {
            return (uint)val;
        }

        protected override Color PixelToColor(uint pixel)
        {
            int alpha = (int)((pixel & AMask) >> AShift);
            int red = (int)((pixel & RMask) >> RShift);
            int green = (int)((pixel & GMask) >> GShift);
            int blue = (int)((pixel & BMask) >> BShift);

            return Color.FromArgb(alpha << 4, red << 4, green << 4, blue << 4);
        }

        protected override uint ColorToPixel(Color c)
        {
            uint a = (uint)c.A >> 4;
            uint r = (uint)c.R >> 4;
            uint g = (uint)c.G >> 4;
            uint b = (uint)c.B >> 4;
            uint pixel = (uint)(((a << AShift) & AMask) | ((r << RShift) & RMask) | ((g << GShift) & GMask) | ((b << BShift) & BMask));

            return pixel;
        }
    }
}
