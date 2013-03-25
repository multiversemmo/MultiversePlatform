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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

#endregion

namespace Multiverse.Tools
{
    public class FontMaker
    {
        public const int START_CHAR = 33; // bang
        public const int END_CHAR = 127;  // tilde

        public static void CreateAllFonts() {
            string baseDir = "C:\\GameDevelopment\\MultiverseClient\\bin\\Media\\Fonts";
            for (int size = 6; size <= 10; size += 2) {
                CreateFontFiles(baseDir, "Verdana", size);
                CreateFontFiles(baseDir, "Tahoma", size);
            }
        }

		public static Graphics GetGraphics(Bitmap bitmap) {
            // get a handles to the graphics context of the bitmap
            Graphics g = Graphics.FromImage(bitmap);
            // clear the image to transparent
            g.Clear(Color.Transparent);
            // nice smooth text
            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            return g;
        }

        public static Bitmap GetBitmap(int w, int h) {
            return new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public static bool BuildImage(Bitmap bitmap, StringBuilder sb, 
                                      string fullFontName, Font font, int maxHeight) 
        {
            int HSPACE = 1;
            int VSPACE = 1;
            int x = 0;
            int y = 0;
            
            StringFormat sfmt = StringFormat.GenericTypographic;
            Graphics g = GetGraphics(bitmap);
            PointF zeroPoint = new PointF(0, 0);
            sb.AppendFormat("{0}\n", fullFontName);
            sb.Append("{\n");
            sb.Append("\ttype image\n");
            sb.AppendFormat("\tsource {0}.png\n", fullFontName);
            sb.AppendFormat("\tmax_height {0}\n", maxHeight);
            sb.Append("\n");

            // loop through each character in the glyph string and draw it to the bitmap
            for (int i = START_CHAR; i < END_CHAR; i++) {
                char c = (char)i;

                // measure the width and height of the character

                SizeF metrics = g.MeasureString(c.ToString(), font, zeroPoint, sfmt);

                int pixelWidth = (int)Math.Ceiling(metrics.Width);
                // int pixelHeight = (int)Math.Ceiling(metrics.Height);
                // are we gonna wrap?
                if (x + pixelWidth + HSPACE >= bitmap.Width) {
                    // increment the y coord and reset x to move to the beginning of next line
                    y += maxHeight + VSPACE;
                    x = 0;
                }

                if (y + maxHeight + VSPACE >= bitmap.Height)
                    return false;

                // draw the character
                g.DrawString(c.ToString(), font, Brushes.White, x, y, sfmt);
//                if (c == 'y' && font.SizeInPoints == 8)
//                    Console.WriteLine("glyph y: {0},{1} {2},{3}", x, y, x + pixelWidth, y + pixelHeight);
                // g.DrawString(c.ToString(), font, Brushes.Black, x, y);

                // calculate the texture coords for the character
                float u1 = ((float)x) / ((float)bitmap.Width);
                float v1 = ((float)y) / ((float)bitmap.Height);
                float u2 = ((float)x + pixelWidth) / ((float)bitmap.Width);
                float v2 = ((float)y + maxHeight) / ((float)bitmap.Height);
                // SetCharTexCoords(c, u1, u2, v1, v2);
                
                sb.AppendFormat("\tglyph\t{0}\t{1}\t{2}\t{3}\t{4}\n", c, u1, v1, u2, v2);
                // sb.AppendFormat("//\tdims\t{0}\t{1}\t{2}\n", c, metrics.Width, maxHeight);

                // increment x by the width of the current char, and add some white space
                x += pixelWidth + HSPACE;

            }  // for
            sb.Append("}\n");
            return true;
        }

        public static void CreateFontFiles(string baseDir, string fontName, int fontSize) {
            string fullFontName = string.Format("{0}{1}", fontName, fontSize);
            
            // TODO: Revisit after checking current Imaging support in Mono.
            // get a font object for the specified font
            System.Drawing.Font font = new System.Drawing.Font(fontName, fontSize);

            // used for calculating position in the image for rendering the characters
            int maxHeight = 0;

            int bitmapWidth = 64;
            int bitmapHeight = 64;

            Bitmap bitmap = GetBitmap(bitmapWidth, bitmapHeight);
            PointF zeroPoint = new PointF(0, 0);
            // Compute the maximum height of any character
            StringBuilder allChars = new StringBuilder();
            for (int i = START_CHAR; i < END_CHAR; i++)
                allChars.Append((char)i);
            Graphics g = GetGraphics(bitmap);
            // measure the width and height of the character
            SizeF metrics = g.MeasureString(allChars.ToString(), font, zeroPoint, 
                                            StringFormat.GenericTypographic);
            maxHeight = (int)Math.Ceiling(metrics.Height);
            
            StringBuilder sb = null;
            // Check to see if the bitmap is large enough
            while (true) {
                sb = new StringBuilder();
                if (BuildImage(bitmap, sb, fullFontName, font, maxHeight))
                    break;
                bitmapWidth = bitmapWidth * 2;
                bitmapHeight = bitmapHeight * 2;
                bitmap = GetBitmap(bitmapWidth, bitmapHeight);
            }
            
            FileStream fstream =
                File.Create(string.Format("{0}\\{1}.fontdef", baseDir, fullFontName));
            TextWriter writer = new StreamWriter(fstream);
            writer.Write(sb.ToString());
            writer.Close();

            bitmap.Save(string.Format("{0}\\{1}.png", baseDir, fullFontName));
        }
    }
}
