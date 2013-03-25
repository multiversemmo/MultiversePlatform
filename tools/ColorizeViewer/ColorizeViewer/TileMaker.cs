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
using System.Drawing;

namespace ColorizeViewer
{
    public class ColorInfo : IComparable
    {
        protected int kmIndex;
        protected float minV;
        protected float maxV;
        protected float midV;
        protected float Vmult;
        protected HSVColor hsv;

        public ColorInfo(int kmIndex, float minV, float maxV, HSVColor hsv)
        {
            this.kmIndex = kmIndex;
            this.minV = minV;
            this.maxV = maxV;
            midV = (maxV + minV) / 2.0f;
            if (midV == 0.0f)
            {
                Vmult = 0.0f;
            }
            else
            {
                Vmult = 0.5f / midV;
            }
            this.hsv = new HSVColor(hsv.H, hsv.S, midV);
        }

        public int KMIndex
        {
            get
            {
                return kmIndex;
            }
        }

        public float MidV
        {
            get
            {
                return midV;
            }
        }

        public float VMult
        {
            get
            {
                return Vmult;
            }
        }

        public HSVColor HSVColor
        {
            get
            {
                return hsv;
            }
        }

        public Color Color
        {
            get
            {
                return hsv.Color;
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            ColorInfo comp = obj as ColorInfo;
            if (midV == comp.midV)
            {
                return 0;
            }
            else if (midV < comp.midV)
            {
                return -1;
            }
            else
            {
                return 1;
            }

        }

        #endregion
    }

    public class TileMaker
    {
        protected Kmeans km;
        protected DDSFile dds;
        List<ColorInfo> colors = new List<ColorInfo>();
        int [] kmColorMap;

        public TileMaker(string filename, int numColors)
        {
            dds = DDSFile.LoadFile(filename);

            int numPixels = dds.Width * dds.Height;

            HSVColor[] pixels = new HSVColor[numPixels];

            // create the array of colors in this image
            int offset = 0;
            for (int y = 0; y < dds.Height; y++)
            {
                for (int x = 0; x < dds.Width; x++)
                {
                    pixels[offset] = HSVColor.FromRGB(dds.GetPixel(x, y));
                    offset++;
                }
            }

            // compute the clustering
            km = new Kmeans(pixels, numColors);

            float[] minv = new float[numColors];
            float[] maxv = new float[numColors];

            for (int i = 0; i < numColors; i++)
            {
                minv[i] = float.MaxValue;
                maxv[i] = float.MinValue;
            }

            // compute min and max v for each color cluster
            for (int y = 0; y < dds.Height; y++)
            {
                for (int x = 0; x < dds.Width; x++)
                {
                    HSVColor hsv = HSVColor.FromRGB(dds.GetPixel(x, y));
                    int index = km.ClosestIndex(hsv);

                    // record min and max v for each channel
                    float v = hsv.V;
                    if (v < minv[index])
                    {
                        minv[index] = v;
                    }
                    if (v > maxv[index])
                    {
                        maxv[index] = v;
                    }
                }
            }

            for (int i = 0; i < numColors; i++)
            {
                if (minv[i] == float.MaxValue)
                {
                    minv[i] = 0.0f;
                }
                if (maxv[i] == float.MinValue)
                {
                    maxv[i] = 0.0f;
                }

                ColorInfo ci = new ColorInfo(i, minv[i], maxv[i], km.CenterColors[i]);
                colors.Add(ci);
            }

            colors.Sort();

            // create the mapping from the kmeans returned colors to the sorted colors
            kmColorMap = new int[numColors];
            for (int i = 0; i < numColors; i++)
            {
                kmColorMap[colors[i].KMIndex] = i;
            }
        }

        public int Width
        {
            get
            {
                return dds.Width;
            }
        }

        public int Height
        {
            get
            {
                return dds.Height;
            }
        }

        public void FillTile(DDSFile ddsDest)
        {
            for (int y = 0; y < dds.Height; y++)
            {
                for (int x = 0; x < dds.Width; x++)
                {
                    HSVColor hsv = HSVColor.FromRGB(dds.GetPixel(x, y));

                    float[] distances = km.DistanceToCenters(hsv);
                    int nearest, second;
                    float nearestWeight;
                    if (distances[0] < distances[1])
                    {
                        nearest = 0;
                        second = 1;
                    }
                    else
                    {
                        nearest = 1;
                        second = 0;
                    }
                    if (distances[2] < distances[nearest])
                    {
                        second = nearest;
                        nearest = 2;
                    }
                    else if (distances[2] < distances[second])
                    {
                        second = 2;
                    }
                    if (distances[3] < distances[nearest])
                    {
                        second = nearest;
                        nearest = 3;
                    }
                    else if (distances[3] < distances[second])
                    {
                        second = 3;
                    }
                    // only weight between the two colors if the current point is nearer to each of them
                    // than they are to each other.  otherwise just weight to neartest.
                    if (km.CenterDistances[nearest, second] < distances[second])
                    {
                        nearestWeight = 1.0f;
                    }
                    else
                    {
                        nearestWeight = distances[second] / (distances[nearest] + distances[second]);
                    }

                    // init color components
                    int[] components = new int[4];
                    components[0] = 0;
                    components[1] = 0;
                    components[2] = 0;
                    components[3] = 0;

                    nearest = kmColorMap[nearest];
                    second = kmColorMap[second];

                    components[nearest] = (int)(hsv.V * colors[nearest].VMult * 255 * nearestWeight);
                    components[second] = (int)(hsv.V * colors[second].VMult * 255 * (1.0f - nearestWeight));
                    Color c = Color.FromArgb(components[3], components[0], components[1], components[2]);

                    int index = kmColorMap[km.ClosestIndex(hsv)];

                    ddsDest.SetColor(x, y, c);
                }
            }

        }

        public DDSFile TileXXX
        {
            get
            {
                DDSFile32 ddsDest = new DDSFile32(dds.Width, dds.Height);
                for (int y = 0; y < dds.Height; y++)
                {
                    for (int x = 0; x < dds.Width; x++)
                    {
                        HSVColor hsv = HSVColor.FromRGB(dds.GetPixel(x, y));
                        int index = kmColorMap[km.ClosestIndex(hsv)];

                        ColorInfo ci = colors[index];
                        Color c;
                        int value = (int)(hsv.V * ci.VMult * 255);
                        switch (index)
                        {
                            case 0:
                                c = Color.FromArgb(0, value, 0, 0);
                                break;
                            case 1:
                                c = Color.FromArgb(0, 0, value, 0);
                                break;
                            case 2:
                                c = Color.FromArgb(0, 0, 0, value);
                                break;
                            case 3:
                                c = Color.FromArgb(value, 0, 0, 0);
                                break;
                            default:
                                c = Color.FromArgb(0, 0, 0, 0);
                                break;
                        }

                        ddsDest.SetColor(x, y, c);
                    }
                }

                return ddsDest;
            }
        }
        public List<ColorInfo> Colors
        {
            get
            {
                return colors;
            }
        }
    }
}
