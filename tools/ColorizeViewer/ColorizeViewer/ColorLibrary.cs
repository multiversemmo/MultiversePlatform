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
using System.IO;
using System.Xml;
using System.Drawing;

namespace ColorizeViewer
{
    public class ColorLibrary
    {
        protected Dictionary<string, Dictionary<string, HSVColor[]>> library;
        protected bool dirty = false;

        public ColorLibrary()
        {
            library = new Dictionary<string, Dictionary<string, HSVColor[]>>();
        }

        public void Clear()
        {
            dirty = false;
            library.Clear();
        }

        public void AddEntry(string tileName, string colorName, HSVColor[] colors)
        {
            Dictionary<string, HSVColor[]> tileMap;
            library.TryGetValue(tileName, out tileMap);
            if (tileMap == null)
            {
                tileMap = new Dictionary<string, HSVColor[]>();
                library[tileName] = tileMap;
            }

            // make sure we have our own copy of the colors
            HSVColor[] newColors = new HSVColor[4];
            tileMap[colorName] = newColors;
            newColors[0] = colors[0];
            newColors[1] = colors[1];
            newColors[2] = colors[2];
            newColors[3] = colors[3];

            dirty = true;
        }

        public void RemoveEntry(string tileName, string colorName)
        {
            Dictionary<string, HSVColor[]> tileMap;
            library.TryGetValue(tileName, out tileMap);
            if (tileMap != null)
            {
                if (tileMap.ContainsKey(colorName))
                {
                    tileMap.Remove(colorName);
                }
                if (tileMap.Count == 0)
                {
                    library.Remove(tileName);
                }
            }
            dirty = true;
        }

        public void RemoveTile(string tileName)
        {
            if (library.ContainsKey(tileName))
            {
                library.Remove(tileName);
            }
        }

        public bool ContainsEntry(string tileName)
        {
            return library.ContainsKey(tileName);
        }

        public bool ContainsEntry(string tileName, string colorName)
        {
            Dictionary<string, HSVColor[]> tileMap;
            library.TryGetValue(tileName, out tileMap);
            if (tileMap != null)
            {
                return tileMap.ContainsKey(colorName);
            }
            else
            {
                return false;
            }
        }

        public HSVColor[] GetEntry(string tileName, string colorName)
        {
            Dictionary<string, HSVColor[]> tileMap;
            library.TryGetValue(tileName, out tileMap);
            HSVColor[] entry = null;
            if (tileMap != null)
            {
                tileMap.TryGetValue(colorName, out entry);
            }

            return entry;
        }

        public List<string> GetTileNames()
        {
            return new List<string>(library.Keys);
        }

        public List<string> GetColorNames(string tileName)
        {
            Dictionary<string, HSVColor[]> tileMap;
            library.TryGetValue(tileName, out tileMap);
            if (tileMap != null)
            {
                return new List<string>(tileMap.Keys);
            }
            else
            {
                return null;
            }
        }

        public void Save(string filename)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;

            XmlWriter w = XmlWriter.Create(filename, xmlWriterSettings);
            w.WriteStartElement("ColorLibrary");

            foreach (string tilename in GetTileNames())
            {
                w.WriteStartElement("Tile");
                w.WriteAttributeString("Name", tilename);
                foreach (string colorname in GetColorNames(tilename))
                {
                    w.WriteStartElement("Colors");
                    w.WriteAttributeString("Name", colorname);
                    HSVColor [] colors = GetEntry(tilename, colorname);
                    for (int i = 0; i < 4; i++)
                    {
                        w.WriteStartElement("HSVColor");
                        w.WriteAttributeString("ColorNum", i.ToString());
                        w.WriteAttributeString("H", colors[i].H.ToString());
                        w.WriteAttributeString("S", colors[i].S.ToString());
                        w.WriteAttributeString("V", colors[i].V.ToString());
                        w.WriteEndElement(); // HSVColor
                    }
                    w.WriteEndElement(); // Colors
                }
                w.WriteEndElement(); // Tile
            }

            w.WriteEndElement(); // ColorLibrary
            w.Close();

            dirty = false;
        }

        public void Load(string filename)
        {
            bool empty = (library.Count == 0);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

            XmlReader r = XmlReader.Create(filename, xmlReaderSettings);

            // read until we find the start of a tile description
            while (r.Read())
            {
                // look for the start of the tile description
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "Tile")
                    {
                        string tileName = r.GetAttribute("Name");

                        // read until we find the start of a color description
                        while (r.Read())
                        {
                            // look for the start of the tile description
                            if (r.NodeType == XmlNodeType.Element)
                            {
                                if (r.Name == "Colors")
                                {
                                    string colorName = r.GetAttribute("Name");
                                    HSVColor[] colors = new HSVColor[4];

                                    while (r.Read())
                                    {
                                        if (r.NodeType == XmlNodeType.Element)
                                        {
                                            if (r.Name == "HSVColor")
                                            {
                                                int colorNum = int.Parse(r.GetAttribute("ColorNum"));
                                                float h = float.Parse(r.GetAttribute("H"));
                                                float s = float.Parse(r.GetAttribute("S"));
                                                float v = float.Parse(r.GetAttribute("V"));
                                                colors[colorNum] = new HSVColor(h, s, v);
                                            }
                                        }
                                        else if (r.NodeType == XmlNodeType.EndElement)
                                        {
                                            break;
                                        }
                                    }
                                    AddEntry(tileName, colorName, colors);
                                }
                            }
                            else if (r.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            dirty = !empty;
        }

        public void Export(string filename)
        {
            StreamWriter w = new StreamWriter(filename);

            w.WriteLine("import ClientAPI");
            w.WriteLine();
            w.WriteLine("TileColorLibrary = {");

            foreach (string tilename in GetTileNames())
            {
                w.WriteLine("  '{0}' : ", tilename);
                w.WriteLine("  {");

                foreach (string colorname in GetColorNames(tilename))
                {
                    w.WriteLine("    '{0}' :", colorname);
                    w.WriteLine("    (");

                    HSVColor[] colors = GetEntry(tilename, colorname);
                    for (int i = 0; i < 4; i++)
                    {
                        Color color = colors[i].Color;
                        float r = color.R / 255f;
                        float g = color.G / 255f;
                        float b = color.B / 255f;
                        float a = 1f;

                        w.WriteLine("      ClientAPI.ColorEx({0}, {1}, {2}, {3}),", a, r, g, b);
                    }
                    w.WriteLine("    ),");
                }
                w.WriteLine("  },");
            }

            w.WriteLine("}");
            w.Close();
        }

        public bool Dirty
        {
            get
            {
                return dirty;
            }
        }
    }
}
