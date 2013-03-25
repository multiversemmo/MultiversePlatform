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
using System.Drawing;

namespace ColorizeViewer
{
    public struct HSVColor
    {
        private float h;
        private float s;
        private float v;

        /// <summary>
        /// create an hsv color from h, s, and v components
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        public HSVColor(float h, float s, float v)
        {
            this.h = h;
            this.s = s;
            this.v = v;
        }

        /// <summary>
        /// create an HSV color from a cartesian coordinate in the HS plane, and
        ///  use 1.0 for V
        /// </summary>
        /// <param name="p"></param>
        public HSVColor(Point p) : this(p, 1.0f)
        {
        }

        /// <summary>
        /// Create an HSV color from a cartesian coordinate in the HS plane, plus
        ///   the provided V
        /// </summary>
        /// <param name="p"></param>
        /// <param name="vin"></param>
        public HSVColor(Point p, float vin)
        {
            double theta;
            double r;

            r = Math.Sqrt(p.Y * p.Y + p.X * p.X);

            if (p.X > 0.0f)
            {
                if (p.Y >= 0.0f)
                {
                    theta = Math.Atan2(p.Y, p.X);
                }
                else
                {
                    theta = Math.Atan2(p.Y, p.X) + 2 * Math.PI;
                }
            }
            else if (p.X < 0.0f)
            {
                theta = Math.Atan2(p.Y, p.X) + Math.PI;
            }
            else // p.X == 0
            {
                if (p.Y > 0)
                {
                    theta = Math.PI / 2.0;
                }
                else
                {
                    theta = Math.PI * 3.0 / 2.0;
                }
            }

            h = (float)(theta * 180.0 / Math.PI);
            s = (float)r;
            v = vin;
        }

        /// <summary>
        /// Create an HSV color from a packed RGB value
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static HSVColor FromRGB(uint rgb)
        {
            int r = (int)(rgb >> 16) & 0xff;
            int g = (int)(rgb >> 8) & 0xff;
            int b = (int)(rgb & 0xff);

            return FromRGB(r, g, b);
        }

        public static HSVColor FromColor(Color c)
        {
            return FromRGB(c.R, c.G, c.B);
        }

        /// <summary>
        /// Create an HSV color from R, G and B values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static HSVColor FromRGB(int r, int g, int b)
        {
            int max;
            int min;
            float h, s, v;

            if (r >= g)
            {
                max = r;
            }
            else
            {
                max = g;
            }

            if (b > max)
            {
                max = b;
            }

            if (r <= g)
            {
                min = r;
            }
            else
            {
                min = g;
            }

            if (b < min)
            {
                min = b;
            }

            float mmdif = max - min;

            if (max == min)
            {
                h = 0.0f;
            }
            else if (max == r)
            {
                h = 60.0f * (float)(g - b) / mmdif;
            }
            else if (max == g)
            {
                h = 60.0f * (float)(b - r) / mmdif + 120.0f;
            }
            else
            {
                h = 60.0f * (float)(r - g) / mmdif + 240.0f;
            }

            if (h < 0.0f)
            {
                h = h + 360.0f;
            }
            if (h > 360.0f)
            {
                h = h - 360.0f;
            }

            if (max == 0.0f)
            {
                s = 0.0f;
            }
            else
            {
                s = mmdif / max;
            }

            v = max / 255.0f;

            return new HSVColor(h, s, v);
        }

        /// <summary>
        /// Return the cartesian coordinates in the HS plane of the color
        /// </summary>
        public Point HSCartesian
        {
            get
            {
                float hrad = h * (float)Math.PI / 180.0f;

                float x = s * (float)Math.Cos(hrad);
                float y = s * (float)Math.Sin(hrad);

                return new Point(x, y);
            }
        }

        /// <summary>
        /// computes the distance in cartesian space between the HS components of
        ///   two colors.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float Distance(HSVColor other)
        {
            float hrad = h * (float)Math.PI / 180.0f;

            float x1 = s * (float)Math.Cos(hrad);
            float y1 = s * (float)Math.Sin(hrad);

            hrad = other.h * (float)Math.PI / 180.0f;

            float x2 = other.s * (float)Math.Cos(hrad);
            float y2 = other.s * (float)Math.Sin(hrad);

            float xdelta = x2 - x1;
            float ydelta = y2 - y1;

            return (float)Math.Sqrt(xdelta * xdelta + ydelta * ydelta);
        }

        public float H
        {
            get
            {
                return h;
            }
        }

        public float S
        {
            get
            {
                return s;
            }
        }

        public float V
        {
            get
            {
                return v;
            }
        }

        public Color Color
        {
            get
            {
                float htmp = h;
                if (htmp >= 360.0)
                {
                    htmp = htmp - 360.0f;
                }

                htmp = htmp / 60.0f;

                int hi = (int)Math.Floor(htmp);
                float f = htmp - hi;
                float p = v * (1.0f - s);
                float q = v * (1.0f - f * s);
                float t = v * (1.0f - (1.0f - f) * s);

                float r, g, b;
                switch (hi)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                    default:
                        throw new Exception("HSVColor RGB conversion error");

                }

                return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
            }
        }

        public override string ToString()
        {
            return string.Format("H: {0}, S: {1}, V: {2}", h, s, v);
        }
    }
}
