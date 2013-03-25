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
using System.Diagnostics;
using System.IO;
using Tao.DevIl;

namespace Axiom.SceneManagers.Multiverse
{
    public class TaoImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] RawData
        {
            get
            {
                return buffer;
            }
        }

        protected int format;
        protected int bytesPerPixel;
        protected int depth;

        protected byte[] buffer;

        static TaoImage()
        {
            Il.ilInit();
            Ilu.iluInit();

            Il.ilEnable(Il.IL_FILE_OVERWRITE);
        }

        public TaoImage(string filename)
        {
            FileStream s = new FileStream(filename, FileMode.Open);

            int imageID;

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);

            // create a temp buffer and write the stream into it
            byte[] tmpBuffer = new byte[s.Length];
            s.Read(tmpBuffer, 0, tmpBuffer.Length);

            // load the data into DevIL
            Il.ilLoadL(Il.IL_PNG, tmpBuffer, tmpBuffer.Length);

            // check for an error
            int ilError = Il.ilGetError();

            if (ilError != Il.IL_NO_ERROR)
            {
                throw new Exception(string.Format("Error while decoding image data: '{0}'", Ilu.iluErrorString(ilError)));
            }

            // flip the image so that the mosaics produced match what L3DT produces
            Ilu.iluFlipImage();

            format = Il.ilGetInteger(Il.IL_IMAGE_FORMAT);
            bytesPerPixel = Math.Max(Il.ilGetInteger(Il.IL_IMAGE_BPC),
                                     Il.ilGetInteger(Il.IL_IMAGE_BYTES_PER_PIXEL));

            Width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            Height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);

            // get the decoded data
            buffer = new byte[Width * Height * bytesPerPixel];
            IntPtr ptr = Il.ilGetData();

            // copy the data into the byte array
            unsafe
            {
                byte* pBuffer = (byte*)ptr;
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = pBuffer[i];
                }
            }

            // we won't be needing this anymore
            Il.ilDeleteImages(1, ref imageID);
        }

        /// <summary>
        /// Create an image from an area of another image
        /// </summary>
        /// <param name="src">source image</param>
        /// <param name="sx">x offset in source image to copy from</param>
        /// <param name="sy">y offset in source image to copy from</param>
        /// <param name="w">width of dest image and area to copy</param>
        /// <param name="h">width of dest image and area to copy</param>
        public TaoImage(TaoImage src, int sx, int sy, int w, int h)
        {
            format = src.format;
            bytesPerPixel = src.bytesPerPixel;
            depth = src.depth;
            Width = w;
            Height = h;

            buffer = new byte[w * h * bytesPerPixel];

            int copyw = w;
            int copyh = h;

            if ((sx + w) > src.Width)
            {
                copyw = src.Width - sx;
            }
            if ((sy + h) > src.Height)
            {
                copyh = src.Height - sy;
            }

            Copy(src, sx, sy, 0, 0, copyw, copyh);
        }

        /// <summary>
        /// Create an image with a given width & height
        /// </summary>
        /// <param name="w">width of dest image and area to copy</param>
        /// <param name="h">width of dest image and area to copy</param>
        public TaoImage(int w, int h, int bytesPerPixel, int format)
        {
            this.format = format;
            this.bytesPerPixel = bytesPerPixel;
            depth = 1;
            Width = w;
            Height = h;

            buffer = new byte[w * h * bytesPerPixel];
        }

        public TaoImage(int w, int h, byte[] source)
        {
            Width = w;
            Height = h;

            format = Il.IL_LUMINANCE;
            bytesPerPixel = 2;

            buffer = new byte[source.Length];
            Array.Copy(source, buffer, buffer.Length);
        }

        public void SetPixel(int x, int y, uint value)
        {
            int index = (y * Width + x) * bytesPerPixel;

            for (int b=0; b < bytesPerPixel; b++)
            {
                // Write out the bytes in little-endian order
                byte bits = (byte) (value & 0xff);
                value >>= 8;
                buffer[index + b] = bits;
            }
        }

        public uint GetPixel(int x, int y)
        {
            int index = (y * Width + x) * bytesPerPixel;
            uint value = 0;

            // Bytes are organized in little-endian order
            for (int b = bytesPerPixel - 1; b >= 0; b--)
            {
                value <<= 8;
                value |= buffer[index + b];
            }
            
            return value;
        }

        public void Copy(TaoImage src, int sx, int sy, int dx, int dy, int w, int h)
        {
            Debug.Assert(src.bytesPerPixel == bytesPerPixel);
            Debug.Assert((sx + w) <= src.Width);
            Debug.Assert((sy + h) <= src.Height);
            Debug.Assert((dx + w) <= Width);
            Debug.Assert((dy + h) <= Height);

            for (int y = 0; y < h; y++)
            {
                int destoff = ((dy + y) * Width + dx) * bytesPerPixel;
                int srcoff = ((sy + y) * src.Width + sx) * bytesPerPixel;
                for (int x = 0; x < (w * bytesPerPixel); x++)
                {
                    buffer[destoff + x] = src.buffer[srcoff + x];
                }
            }
        }

        public void Save(string savename)
        {
            int imageID;

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);
            Il.ilSetString(Il.IL_PNG_AUTHNAME_STRING, "Multiverse Network");

            // stuff the data into the image
            if (bytesPerPixel == 2 && format == Il.IL_LUMINANCE)
            {
                // special case for 16 bit greyscale
                Il.ilTexImage(Width, Height, depth, 1, format, Il.IL_UNSIGNED_SHORT, buffer);
            }
            else
            {
                Il.ilTexImage(Width, Height, depth, (byte)bytesPerPixel, format, Il.IL_UNSIGNED_BYTE, buffer);
            }

            // flip the image so that the mosaics produced match what L3DT produces
            Ilu.iluFlipImage();

            // save the image to file
            Il.ilEnable(Il.IL_FILE_OVERWRITE);
            Il.ilSaveImage(savename);
            
            // delete the image
            Il.ilDeleteImages(1, ref imageID);
        }

        public void DumpErrors(string title)
        {
            int error = Il.ilGetError();
            while (error != 0)
            {
                Console.WriteLine(title + ": Error #" + error + " Message: " + Ilu.iluErrorString(error));
                error = Il.ilGetError();
            }
        }


        /// <summary>
        ///    Converts a DevIL format enum to a PixelFormat enum.
        /// </summary>
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
    }
}
