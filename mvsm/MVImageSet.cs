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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using Vector2 = Axiom.MathLib.Vector2;
using Axiom.Core;

namespace Axiom.SceneManagers.Multiverse
{

	public struct ImageRect
	{
		public string name;
		public int left;
		public int right;
		public int top;
		public int bottom;

		public ImageRect(string name, int left, int right, int top, int bottom)
		{
			this.name = name;
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}
		
		public int Width
		{
			get {
				return right - left;
			}
		}
		
		public int Height
		{
			get {
				return bottom - top;
			}
		}
			
		// Returns coordinates of the start of the image in the
		// floating point range 0..1, and the size as a fraction of
		// the width and height.
		public void UnitCoordStartAndSize(int totalWidth, int totalHeight, 
										  out Vector2 start, out Vector2 size)
		{
			float onePixelWide = 1.0f / (float)totalWidth;
			float onePixelHigh = 1.0f / (float)totalHeight;
			start = new Vector2(((float)left + 0.5f) * onePixelWide,
								((float)top + 0.5f) * onePixelHigh);
			size = new Vector2(((float)(Width - 1) + 0.5f) * onePixelWide,
							   ((float)(Height - 1) + 0.5f) * onePixelHigh);
		}

	}
	
	public class MVImageSet
	{
		// Width and Height in pixels
		public int width;
		public int height;
		// A list of regions of the image
		public List<ImageRect> imageRects;
		
		public MVImageSet()
		{
			width = 0;
			height = 0;
			imageRects = new List<ImageRect>();
		}
		
		// Returns coordinates of the start of the image in the
		// floating point range 0..1, and the size as a fraction of
		// the width and height.  The return value is true if the
		// named image was found; false otherwise.
		public bool FindImageStartAndSize (string imageName, 
										   out Vector2 start, out Vector2 size)
		{
			foreach(ImageRect rect in imageRects)
			{
				if (rect.name == imageName)
				{
					rect.UnitCoordStartAndSize(width, height, out start, out size);
					return true;
				}
			}
			start = new Vector2(0f, 0f);
			size = new Vector2(0f, 0f);
			return false;
		}

		public void AddImageRect(ImageRect rect)
		{
			imageRects.Add(rect);
		}
		
        public static MVImageSet FindMVImageSet(string fileName)
		{
			Stream stream  = null;
            try {
                stream = ResourceManager.FindCommonResourceData(fileName);
            } catch (AxiomException) {
                return null;
            }
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreWhitespace = true;
			XmlReader r = XmlReader.Create(stream, settings);
			r.Read();
			if (r.Name == "MVImageSet")
			{
				MVImageSet imageSet = new MVImageSet();
				imageSet.FromXML(r);
				return imageSet;
			}
			return null;
		}
		
		public void ToXML(XmlTextWriter w)
        {
            w.WriteStartElement("MVImageSet");
            w.WriteElementString("width", width.ToString());
            w.WriteElementString("height", height.ToString());
            foreach (ImageRect r in imageRects)
            {
                w.WriteStartElement("ImageRect");
                w.WriteAttributeString("name", r.name);
				w.WriteAttributeString("left", r.left.ToString());
				w.WriteAttributeString("right", r.right.ToString());
				w.WriteAttributeString("top", r.top.ToString());
				w.WriteAttributeString("bottom", r.bottom.ToString());
				w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        private void FromXML(XmlReader r)
        {
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    ParseElement(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the terrain description
                    return;
                }
            }
        }

        protected void ParseElement(XmlReader r)
        {
            bool readEnd = true;
            // set the field in this object based on the element we just read
            switch (r.Name)
			{
			case "width":
				// read the value
				r.Read();
				if (r.NodeType != XmlNodeType.Text)
					return;
				width = int.Parse(r.Value);
				break;

			case "height":
				// read the value
				r.Read();
				if (r.NodeType != XmlNodeType.Text)
					return;
				height = int.Parse(r.Value);
				break;

			case "ImageRect":
				string name = "";
				int left = 0;
				int right = 0;
				int top = 0;
				int bottom = height;
				for (int i = 0; i < r.AttributeCount; i++)
				{
					r.MoveToAttribute(i);

					// set the field in this object based on the element we just read
					switch (r.Name)
					{
					case "name":
						name = r.Value;
						break;
					case "left":
						left = int.Parse(r.Value);
						break;
					case "right":
						right = int.Parse(r.Value);
						break;
					case "top":
						top = int.Parse(r.Value);
						break;
					case "bottom":
						bottom = int.Parse(r.Value);
						break;
					}
				}
				r.MoveToElement(); //Moves the reader back to the element node.
				AddImageRect(new ImageRect(name, left, right, top, bottom));
				readEnd = false;
				break;
			}
            if (readEnd)
            {
                // error out if we dont see an end element here
                r.Read();
                if (r.NodeType != XmlNodeType.EndElement)
                    return;
            }
		}

	}
}
