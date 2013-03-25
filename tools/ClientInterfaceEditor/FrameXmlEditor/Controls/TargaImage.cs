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
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	public class TargaImage
	{
        /// <summary>
        /// Loads the file from the specified fileName.
        /// It performs a basic lookup for the file:
        ///  - checks for missing extension
        ///  - considers fileName relative to project path
        ///  - checks the file in the root of project directory
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>an Image object or null</returns>
		public static Image LookupFile(BaseControl baseControl, string fileName)
		{
			if (String.IsNullOrEmpty(fileName))
				return null;

			if (!fileName.EndsWith(".tga"))
				fileName += ".tga";

			if (File.Exists(fileName))
				return LoadFromFile(fileName);

			string projectDirectory = Path.GetDirectoryName(baseControl.DesignerLoader.DocumentMoniker);

			fileName = Path.Combine(projectDirectory, fileName);
			if (File.Exists(fileName))
				return LoadFromFile(fileName);

			fileName = Path.GetFileName(fileName);
			fileName = Path.Combine(projectDirectory, fileName);
			if (File.Exists(fileName))
				return LoadFromFile(fileName);

			return null;
		}

        /// <summary>
        /// Loads the file from the specified fileName.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Image or null</returns>
		public static Image LoadFromFile(string fileName)
		{
			if (!File.Exists(fileName))
				return null;

			byte[] header = new byte[18];
			using (FileStream stream = new FileStream(fileName, FileMode.Open))
			{
				stream.Read(header, 0, header.Length);
				int width = header[12] + (header[13] << 8);
				int height = header[14] + (header[15] << 8);
				byte colorDepth = header[16];
				byte imageDescriptor = header[17];

                if (colorDepth != 32)
                {
                    Trace.WriteLine("Color depth should be 32");
                    return null;
                }

				// handling color map
                if (header[1] != 0)
                {
                    Trace.WriteLine("Color maps are not supported");
                    return null;
                }

				// image type
                if (header[2] > 3)
                {
                    Trace.WriteLine("Encoded or compressed images are not supported");
                    return null;
                }

				// bypass image ID
				byte[] imageID = new byte[header[0]];
				stream.Read(imageID, 0, imageID.Length);

				bool origoTop = ((imageDescriptor & 0x20) != 0);
				bool origoLeft = ((imageDescriptor & 0x10) == 0);

				// we don't support color maps so there is nothing to bypass

				Bitmap bitmap = new Bitmap(width, height);
				byte[] argb = new byte[4];
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						stream.Read(argb, 0, argb.Length);
						Color color = Color.FromArgb(argb[3], argb[2], argb[1], argb[0]);
                        bitmap.SetPixel(
                            origoLeft ? x : width - 1 - x,
                            origoTop ? y : height - 1 - y,
                            color);
					}
				}
				return bitmap;
			}
		}
	}
}
