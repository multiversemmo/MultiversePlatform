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
using System.Linq;
using System.Text;
using Multiverse.Lib;

namespace TilePad
{
    class TilePad
    {
        static void DumpPixel(DDSFile dds, int x, int y)
        {
            Console.WriteLine("Pixel({0}, {1}) : 0x{2:x})", x, y, dds.GetPixel(x, y));
        }

        static void Main(string[] args)
        {
            DDSFile dds = DDSFile.LoadFile(args[0]);

            int w = dds.Width;
            int h = dds.Height;
            int halfW = w / 2;
            int halfH = h / 2;

            if (dds.BitsPerPixel != 32)
            {
                Console.WriteLine("Error: input file must be 32-bits per pixel");
            }
            DDSFile destDDS = new DDSFile32(w * 2, h * 2);

            
            // copy the various parts of the source to pad all edges
            destDDS.Copy(dds, halfW, halfH, halfW, halfH, 0, 0);
            destDDS.Copy(dds, 0, halfH, w, halfH, halfW, 0);
            destDDS.Copy(dds, 0, halfH, halfW, halfH, w + halfW, 0);

            destDDS.Copy(dds, halfW, 0, halfW, h, 0, halfH);
            destDDS.Copy(dds, 0, 0, w, h, halfW, halfH);
            destDDS.Copy(dds, 0, 0, halfW, h, w + halfW, halfH);

            destDDS.Copy(dds, halfW, 0, halfW, halfH, 0, h + halfH);
            destDDS.Copy(dds, 0, 0, w, halfH, halfW, h + halfH);
            destDDS.Copy(dds, 0, 0, halfW, halfH, w + halfW, h + halfH);

            // save the padded tile
            destDDS.Save(args[1]);

            DDSFile preview = new DDSFile32(halfW, halfH);
            for (int y = 0; y < halfH; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    uint p0 = dds.GetPixel(x * 2, y * 2);
                    uint p1 = dds.GetPixel(x * 2 + 1, y * 2);
                    uint p2 = dds.GetPixel(x * 2, y * 2 + 1);
                    uint p3 = dds.GetPixel(x * 2 + 1, y * 2 + 1);

                    uint r = ((p0 >> 16) & 0xff) + ((p1 >> 16) & 0xff) + ((p2 >> 16) & 0xff) + ((p3 >> 16) & 0xff);
                    uint g = ((p0 >> 8) & 0xff) + ((p1 >> 8) & 0xff) + ((p2 >> 8) & 0xff) + ((p3 >> 8) & 0xff);
                    uint b = ((p0 >> 0) & 0xff) + ((p1 >> 0) & 0xff) + ((p2 >> 0) & 0xff) + ((p3 >> 0) & 0xff);
                    uint a = ((p0 >> 24) & 0xff) + ((p1 >> 24) & 0xff) + ((p2 >> 24) & 0xff) + ((p3 >> 24) & 0xff);

                    uint high = r;
                    uint highCol = 0xffffffff;
                    if (g > high)
                    {
                        high = g;
                        highCol = 0xff000000;
                    }
                    if (b > high)
                    {
                        high = b;
                        highCol = 0xffaaaaaa;
                    }
                    if (a > high)
                    {
                        high = g;
                        highCol = 0xff555555;
                    }
                    preview.SetPixel(x, y, highCol);
                }
            }
            preview.Save(args[2]);
        }
    }
}
