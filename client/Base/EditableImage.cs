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

using Axiom.Media;
using Axiom.Core;
using System.Diagnostics;

namespace Multiverse.Base
{
    public class EditableImage
    {
        protected Image image;
        protected int bytesPerPixel;
        protected int stride;

        protected int width;
        protected int height;
        protected PixelFormat format;

        protected string textureName;
        protected int numMipMaps;
        protected bool isAlpha;

        byte[] pixelData;

        public EditableImage(string name, int width, int height, PixelFormat format, ColorEx color, int numMipMaps, bool isAlpha)
        {
            this.width = width;
            this.height = height;
            this.format = format;
            this.textureName = name;
            this.numMipMaps = numMipMaps;
            this.isAlpha = isAlpha;

            bytesPerPixel = PixelUtil.GetNumElemBytes(format);
            stride = bytesPerPixel * width;

            pixelData = new byte[width * height * bytesPerPixel];

            // this will pin the buffer
            image = Image.FromDynamicImage(pixelData, width, height, format);

            Fill(0, 0, width, height, color);
        }

        protected int ComputeOffset(int x, int y)
        {
            Debug.Assert(((y * width + x) * bytesPerPixel) < pixelData.Length);
            return (y * width + x) * bytesPerPixel;
        }

        public unsafe void SetPixel(int x, int y, ColorEx color)
        {
            fixed (byte* bytebuf = pixelData)
            {
                PixelUtil.PackColor(color.r, color.g, color.b, color.a, format, bytebuf + ComputeOffset(x, y));
            }
        }

        public unsafe ColorEx GetPixel(int x, int y)
        {
            float r, g, b, a;
            fixed (byte* bytebuf = pixelData)
            {
                PixelUtil.UnpackColor(out r, out g, out b, out a, format, bytebuf + ComputeOffset(x, y));
            }

            return new ColorEx(a, r, g, b);
        }

        protected unsafe void Fill(int xfill, int yfill, int w, int h, ColorEx color)
        {
            fixed (byte* bytebuf = pixelData)
            {
                for (int y = yfill; y < (yfill + h); y++)
                {
                    for (int x = xfill; x < (xfill + w); x++)
                    {
                        PixelUtil.PackColor(color.r, color.g, color.b, color.a, format, bytebuf + ComputeOffset(x, y));
                    }
                }
            }
        }

        public Texture LoadTexture()
        {
            // if the texture exists, destroy it
            if (TextureManager.Instance.HasResource(textureName))
            {
                Texture tex = TextureManager.Instance.GetByName(textureName);

                TextureManager.Instance.Remove(textureName);
                tex.Dispose();
            }

            // determine texture type based on height
            Axiom.Graphics.TextureType texType;
            if (height == 1)
            {
                texType = Axiom.Graphics.TextureType.OneD;
            }
            else
            {
                texType = Axiom.Graphics.TextureType.TwoD;
            }

            // create the texture
            return TextureManager.Instance.LoadImage(textureName, image, texType, numMipMaps, 1.0f, isAlpha);
        }
    }
}
