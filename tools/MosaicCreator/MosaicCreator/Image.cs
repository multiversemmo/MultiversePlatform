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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Tao.DevIl;

namespace Multiverse.Tools.MosaicCreator
{
    public class Image
    {
        protected string filename;
        protected int format;
        protected int bytesPerPixel;
        protected int width;
        protected int height;
        protected int depth;

        protected byte[] buffer;

        static Image()
        {
            Il.ilInit();
            Ilu.iluInit();

            Il.ilEnable(Il.IL_FILE_OVERWRITE);
        }

        public Image(string filename)
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

            width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);

            // get the decoded data
            buffer = new byte[width * height * bytesPerPixel];
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
        /// <param name="x">x offset in source image to copy from</param>
        /// <param name="y">y offset in source image to copy from</param>
        /// <param name="w">width of dest image and area to copy</param>
        /// <param name="h">width of dest image and area to copy</param>
        public Image(Image src, int sx, int sy, int w, int h)
        {
            filename = null;
            format = src.format;
            bytesPerPixel = src.bytesPerPixel;
            depth = src.depth;
            width = w;
            height = h;

            buffer = new byte[w * h * bytesPerPixel];

            int copyw = w;
            int copyh = h;

            if ((sx + w) > src.width)
            {
                copyw = src.width - sx;
            }
            if ((sy + h) > src.height)
            {
                copyh = src.height - sy;
            }

            Copy(src, sx, sy, 0, 0, copyw, copyh);
        }



        public void Copy(Image src, int sx, int sy, int dx, int dy, int w, int h)
        {
            Debug.Assert(src.bytesPerPixel == bytesPerPixel);
            Debug.Assert((sx + w) <= src.width);
            Debug.Assert((sy + h) <= src.height);
            Debug.Assert((dx + w) <= width);
            Debug.Assert((dy + h) <= height);

            for (int y = 0; y < h; y++)
            {
                int destoff = ((dy + y) * width + dx) * bytesPerPixel;
                int srcoff = ((sy + y) * src.width + sx) * bytesPerPixel;
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

            // stuff the data into the image
            if (bytesPerPixel == 2 && format == Il.IL_LUMINANCE)
            {
                // special case for 16 bit greyscale
                Il.ilTexImage(width, height, depth, (byte)1, format, Il.IL_UNSIGNED_SHORT, buffer);
            }
            else
            {
                Il.ilTexImage(width, height, depth, (byte)bytesPerPixel, format, Il.IL_UNSIGNED_BYTE, buffer);
            }

            // save the image to file
            Il.ilSaveImage(savename);

            // delete the image
            Il.ilDeleteImages(1, ref imageID);
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
    }
}
