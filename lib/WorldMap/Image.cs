using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Tao.DevIl;

namespace Multiverse.Lib.WorldMap
{
    public unsafe class Image
    {
        protected int imageID;
        protected string filename;
        protected int format;
        protected int bytesPerPixel;
        protected int width;
        protected int height;
        protected int depth;
        protected int stride;

        protected byte *pBuffer;

        static Image()
        {
            Il.ilInit();
            Ilu.iluInit();

            Il.ilEnable(Il.IL_FILE_OVERWRITE);
        }

        public Image(string filename)
        {

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);

            // load the data into DevIL
            Il.ilLoadImage(filename);

            // check for an error
            int ilError = Il.ilGetError();

            if (ilError != Il.IL_NO_ERROR)
            {
                throw new Exception(string.Format("Error while decoding image data: '{0}'", Ilu.iluErrorString(ilError)));
            }

            format = Il.ilGetInteger(Il.IL_IMAGE_FORMAT);
            bytesPerPixel = Math.Max(Il.ilGetInteger(Il.IL_IMAGE_BPC),
                                     Il.ilGetInteger(Il.IL_IMAGE_BYTES_PER_PIXEL));

            width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);
            stride = bytesPerPixel * width;

            // get the decoded data
            IntPtr ptr = Il.ilGetData();

            // copy the data into the byte array
            pBuffer = (byte*)ptr;
        }

        public uint GetRawPixel(int x, int y)
        {
            Debug.Assert((x < width) && (y < height));

            int shift = 0;
            uint value = 0;
            int offset = (x + (y * width)) * bytesPerPixel;
            for (int i = 0; i < bytesPerPixel; i++)
            {
                value |= ( (uint)pBuffer[offset + i] << shift );
                shift += 8;
            }

            return value;
        }

        public double GetNormPixel(int x, int y)
        {
            Debug.Assert((x < width) && (y < height));

            int offset = (x + (y * width)) * bytesPerPixel;
            uint rawValue = 0;
            int shift = 24;

            for (int i = bytesPerPixel - 1; i >= 0; i--)
            {
                uint tmp = pBuffer[offset + i];
                rawValue |= (tmp << shift);
                shift -= 8;
            }

            return ((double)rawValue) / ((double)uint.MaxValue);
        }

        public void Dispose()
        {
            // we won't be needing this anymore
            Il.ilDeleteImages(1, ref imageID);
        }

        //public void Save(string savename)
        //{
        //    int imageID;

        //    // create and bind a new image
        //    Il.ilGenImages(1, out imageID);
        //    Il.ilBindImage(imageID);

        //    // stuff the data into the image
        //    if (bytesPerPixel == 2 && format == Il.IL_LUMINANCE)
        //    {
        //        // special case for 16 bit greyscale
        //        Il.ilTexImage(width, height, depth, (byte)1, format, Il.IL_UNSIGNED_SHORT, buffer);
        //    }
        //    else
        //    {
        //        Il.ilTexImage(width, height, depth, (byte)bytesPerPixel, format, Il.IL_UNSIGNED_BYTE, buffer);
        //    }

        //    // save the image to file
        //    Il.ilSaveImage(savename);

        //    // delete the image
        //    Il.ilDeleteImages(1, ref imageID);
        //}

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

        public int BytesPerPixel
        {
            get
            {
                return bytesPerPixel;
            }
        }

        /// <summary>
        ///    Converts a DevIL format enum to a PixelFormat enum.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="bytesPerPixel"></param>
        /// <returns></returns>
        public string PixelFormatName
        {
            get
            {
                string ret = "Unknown";

                switch (bytesPerPixel)
                {
                    case 1:
                        ret = "8-bit Greyscale";
                        break;
                    case 2:
                        switch (format)
                        {
                            case Il.IL_BGR:
                                ret = "16-bit RGB (B5G6R5)";
                                break;
                            case Il.IL_RGB:
                                ret = "16-bit RGB (R5G6B5)";
                                break;
                            case Il.IL_BGRA:
                                ret = "16-bit BGRA (B4G4R4A4)";
                                break;
                            case Il.IL_RGBA:
                                ret = "16-bit RGBA (A4R4G4B4)";
                                break;
                            case Il.IL_LUMINANCE:
                                ret = "16-bit Greyscale";
                                break;
                        }
                        break;
                    case 3:
                        switch (format)
                        {
                            case Il.IL_BGR:
                                ret = "24-bit BGR";
                                break;
                            case Il.IL_RGB:
                                ret = "24-bit RGB";
                                break;
                            case Il.IL_LUMINANCE:
                                ret = "24-bit Greyscale";
                                break;
                        }
                        break;

                    case 4:
                        switch (format)
                        {
                            case Il.IL_BGRA:
                                ret = "32-bit BGRA";
                                break;
                            case Il.IL_RGBA:
                                ret = "32-bit RGBA";
                                break;
                            case Il.IL_DXT1:
                                ret = "32-bit DXT1";
                                break;
                            case Il.IL_DXT2:
                                ret = "32-bit DXT2";
                                break;
                            case Il.IL_DXT3:
                                ret = "32-bit DXT3";
                                break;
                            case Il.IL_DXT4:
                                ret = "32-bit DXT4";
                                break;
                            case Il.IL_DXT5:
                                ret = "32-bit DXT5";
                                break;
                        } break;
                }

                return ret;
            }
        }

        public static void SaveRawL16(string savename, int width, int height, ushort[] buffer)
        {
            int imageID;

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);

            fixed (ushort * pbuf = buffer)
            {
                // special case for 16 bit greyscale
                Il.ilTexImage(width, height, 1, (byte)1, Il.IL_LUMINANCE, Il.IL_UNSIGNED_SHORT, (IntPtr)pbuf);
            }

            // flip image since direct buffer copy gets it in the wrong y order
            Ilu.iluFlipImage();

            // save the image to file
            Il.ilSaveImage(savename);

            // delete the image
            Il.ilDeleteImages(1, ref imageID);
        }

        public static ushort[] LoadRawL16(string filename, out int width, out int height)
        {
            Image tmpimg = new Image(filename);

            Debug.Assert(tmpimg.BytesPerPixel == 2);
            
            width = tmpimg.Width;
            height = tmpimg.Height;

            ushort[] buffer = new ushort[width * height];

            int offset = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[offset] = (ushort)tmpimg.GetRawPixel(x, y);
                    offset++;
                }
            }
            tmpimg.Dispose();

            return buffer;
        }

        public static uint[] LoadRawL32(string filename, out int width, out int height)
        {
            Image tmpimg = new Image(filename);

            Debug.Assert(tmpimg.BytesPerPixel == 4);

            width = tmpimg.Width;
            height = tmpimg.Height;

            uint[] buffer = new uint[width * height];

            int offset = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[offset] = (uint)tmpimg.GetRawPixel(x, y);
                    offset++;
                }
            }
            tmpimg.Dispose();

            return buffer;
        }

        public static uint[] LoadRawARGB(string filename, out int width, out int height)
        {
            return LoadRawL32(filename, out width, out height);
        }

        public static void SaveRawL32(string savename, int width, int height, uint[] buffer)
        {
            int imageID;

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);

            fixed (uint* pbuf = buffer)
            {
                Il.ilTexImage(width, height, 1, (byte)4, Il.IL_RGBA, Il.IL_UNSIGNED_BYTE, (IntPtr)pbuf);
            }

            // flip image since direct buffer copy gets it in the wrong y order
            Ilu.iluFlipImage();

            // save the image to file
            Il.ilSaveImage(savename);

            // delete the image
            Il.ilDeleteImages(1, ref imageID);
        }

        public static void SaveRawARGB(string savename, int width, int height, uint[] buffer)
        {
            SaveRawL32(savename, width, height, buffer);
        }
    }
}
