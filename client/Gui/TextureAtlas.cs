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
using System.Xml;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Diagnostics;

using Axiom.MathLib;
using Axiom.Core;

namespace Multiverse.Gui
{
    /// <summary>
    ///   Composite class that holds multiple images that reference a single 
    ///   texture
    /// </summary>
    public class TextureAtlas
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TextureAtlas));

        public string name;
        public Texture texture;
        Dictionary<string, TextureInfo> textures = new Dictionary<string, TextureInfo>();

        public TextureAtlas(string name, Texture texture) {
            this.name = name;
            this.texture = texture;
        }

        public TextureAtlas(string xmlFile) {
            XmlDocument document = new XmlDocument();
            Stream stream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read);
            document.Load(stream);
            foreach (XmlNode childNode in document.ChildNodes) {
                switch (childNode.Name) {
                    case "xml":
                        break;
                    case "Imageset":
                        ReadImageset(childNode);
                        break;
                    default:
                        if (childNode.NodeType != XmlNodeType.Comment)
                            log.InfoFormat("Unknown tag: {0}", childNode.Name);
                        break;
                }
            }
            stream.Dispose();
        }

        private void ReadImageset(XmlNode node) {
            this.name = node.Attributes["Name"].Value;
            string imagefile = node.Attributes["Imagefile"].Value;
            this.texture = TextureManager.Instance.Load(imagefile);
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "Image":
                        ReadImage(childNode);
                        break;
                    default:
                        if (childNode.NodeType != XmlNodeType.Comment)
                            log.InfoFormat("Unknown tag: {0}", childNode.Name);
                        break;
                }
            }
        }

        private void ReadImage(XmlNode node) {
            XmlAttribute attr;
            attr = node.Attributes["Name"];
            string name = attr.Value;
            float xPos = float.Parse(node.Attributes["XPos"].Value);
            float yPos = float.Parse(node.Attributes["YPos"].Value);
            float width = float.Parse(node.Attributes["Width"].Value);
            float height = float.Parse(node.Attributes["Height"].Value);
            TextureInfo texInfo = new TextureInfo(this, name, new PointF(xPos, yPos), new SizeF(width, height));
            textures[name] = texInfo;
        }

        public TextureInfo GetTextureInfo(string textureName) {
            return textures[textureName];
        }

        public bool ContainsKey(string textureName) {
            return textures.ContainsKey(textureName);
        }

        public TextureInfo DefineImage(string textureName, PointF point, SizeF size) {
            TextureInfo textureInfo = new TextureInfo(this, textureName, point, size);
            textures[textureName] = textureInfo;
            return textureInfo;
        }
        public TextureInfo DefineImage(string textureName, Rect rect)
        {
            PointF point = new PointF(rect.Left, rect.Top);
            SizeF size = new SizeF(rect.Width, rect.Height);
            TextureInfo textureInfo = new TextureInfo(this, name, point, size);
            //if (textures.ContainsKey(textureName))
            //    throw new Exception("duplicate image definition");
            textures[textureName] = textureInfo;
            return textureInfo;
        }
        public TextureInfo DefineImage(string textureName, RectangleF rect)
        {
            TextureInfo textureInfo = new TextureInfo(this, name, rect);
            //if (textures.ContainsKey(textureName))
            //    throw new Exception("duplicate image definition");
            textures[textureName] = textureInfo;
            return textureInfo;
        }
        /// <summary>
        ///   Build a TextureInfo object based on the pixel offsets passed in.
        /// </summary>
        /// <param name="textureName"></param>
        /// <param name="points">These are the points, and are in pixel count space.</param>
        /// <returns></returns>
        public TextureInfo DefineImage(string textureName, PointF[] points) {
            PointF[] uvArray = new PointF[4];
#if TEXEL_OFFSET
            uvArray[0] = new Point((points[0].x + .5f) / texture.Width, (points[0].y + .5f) / texture.Height);
            uvArray[1] = new Point((points[1].x - .5f) / texture.Width, (points[1].y + .5f) / texture.Height);
            uvArray[2] = new Point((points[2].x + .5f) / texture.Width, (points[2].y - .5f) / texture.Height);
            uvArray[3] = new Point((points[3].x - .5f) / texture.Width, (points[3].y - .5f) / texture.Height);
#else
            uvArray[0] = new PointF(points[0].X / texture.Width, points[0].Y / texture.Height);
            uvArray[1] = new PointF(points[1].X / texture.Width, points[1].Y / texture.Height);
            uvArray[2] = new PointF(points[2].X / texture.Width, points[2].Y / texture.Height);
            uvArray[3] = new PointF(points[3].X / texture.Width, points[3].Y / texture.Height);
#endif
            TextureInfo textureInfo = new TextureInfo(this, name, uvArray);
            //if (textures.ContainsKey(textureName))
            //    throw new Exception("duplicate image definition");
            textures[textureName] = textureInfo;
            return textureInfo;
        }
        /// <summary>
        ///   Clear out all the entries that have been created by calls to 
        ///   DefineImage or xml elements.
        /// </summary>
        public void Clear()
        {
            textures.Clear();
        }

        public string Name {
            get {
                return name;
            }
        }
    }

    public class AtlasManager {
        static AtlasManager instance = null;

        Dictionary<string, TextureAtlas> atlases = new Dictionary<string, TextureAtlas>();

        static public AtlasManager Instance {
            // TODO: Lock
            get {
                if (instance == null)
                    instance = new AtlasManager();
                return instance;
            }
        }

        public TextureAtlas GetTextureAtlas(string atlasName) {
            return atlases[atlasName];
        }

        public TextureAtlas CreateAtlas(string atlasName, Texture texture) {
            TextureAtlas rv = new TextureAtlas(atlasName, texture);
            atlases[rv.Name] = rv;
            return rv;
        }
        public TextureAtlas CreateAtlas(string atlasName, string textureFile) {
            Texture texture = TextureManager.Instance.Load(textureFile);
            return CreateAtlas(atlasName, texture);
        }
        // FIXME - actually load an atlas file
        public TextureAtlas CreateAtlas(string atlasFile) {
            TextureAtlas rv = new TextureAtlas(atlasFile);
            atlases[rv.Name] = rv;
            return rv;
        }
        public void DestroyAtlas(TextureAtlas atlas) {
            foreach (KeyValuePair<string, TextureAtlas> kvp in atlases) {
                if (kvp.Value == atlas) {
                    // invalidates my iteration
                    atlases.Remove(kvp.Key);
                    return;
                }
            }
        }

        public bool ContainsKey(string atlasName) {
            return atlases.ContainsKey(atlasName);
        }
    }


    /// <summary>
    ///   Class containing the uv coordinates of the four corners of our 
    ///   rectangular texture chunk
    /// </summary>
    public class TextureInfo
    {
        TextureAtlas atlas;
        SizeF size;
        // topleft, topright, bottomleft, bottomright
        PointF[] uvArray = new PointF[4];
        string name; // mostly just for debugging

        public TextureInfo(TextureAtlas atlas, string name, RectangleF rect)
        {
            this.atlas = atlas;
            this.size = new SizeF(rect.Width, rect.Height);
            this.name = name;
            uvArray[0] = new PointF(rect.Left / atlas.texture.Width, rect.Top / atlas.texture.Height);
            uvArray[1] = new PointF(rect.Right / atlas.texture.Width, rect.Top / atlas.texture.Height);
            uvArray[2] = new PointF(rect.Left / atlas.texture.Width, rect.Bottom / atlas.texture.Height);
            uvArray[3] = new PointF(rect.Right / atlas.texture.Width, rect.Bottom / atlas.texture.Height);
        }
        public TextureInfo(TextureAtlas atlas, string name, PointF point, SizeF size) {
            this.atlas = atlas;
            this.size = size;
            this.name = name;
#if TEXEL_OFFSET
            uvArray[0] = new Point((point.x + .5f) / atlas.texture.Width, (point.y + .5f) / atlas.texture.Height);
            uvArray[1] = new Point((point.x + size.width - .5f) / atlas.texture.Width, (point.y + .5f) / atlas.texture.Height);
            uvArray[2] = new Point((point.x + .5f) / atlas.texture.Width, (point.y + size.height -.5f) / atlas.texture.Height);
            uvArray[3] = new Point((point.x + size.width -.5f) / atlas.texture.Width, (point.y + size.height -.5f) / atlas.texture.Height);
#else
            uvArray[0] = new PointF(point.X / atlas.texture.Width, point.Y / atlas.texture.Height);
            uvArray[1] = new PointF((point.X + size.Width) / atlas.texture.Width, point.Y / atlas.texture.Height);
            uvArray[2] = new PointF(point.X / atlas.texture.Width, (point.Y + size.Height) / atlas.texture.Height);
            uvArray[3] = new PointF((point.X + size.Width) / atlas.texture.Width, (point.Y + size.Height) / atlas.texture.Height);
#endif
        }

        /// <summary>
        ///   Construct a TextureInfo object where we are passed the uv array.
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="uvArray"></param>
        public TextureInfo(TextureAtlas atlas, string name, PointF[] uvArray) {
            this.atlas = atlas;
            this.size = new SizeF(0, 0); // meaningless in this case
            this.name = name;
            this.uvArray = uvArray;
        }

        //public void Draw() {
        //    Point[] destPoints = new Point[4];
        //    destPoints[0] = new Point(target.Left, target.Top);
        //    destPoints[1] = new Point(target.Left + target.Width, target.Top);
        //    destPoints[2] = new Point(target.Left, target.Top + target.Height);
        //    destPoints[3] = new Point(target.Left + target.Width, target.Top + target.Height);

        //    Renderer.Instance.AddQuad(destPoints, 0, null, this.Texture, this.uvArray, null);
        //}

        public void Draw(Rect target, float z, Rect clip, ColorRect colors) {
            PointF[] destPoints = new PointF[4];
            destPoints[0] = new PointF(target.Left, target.Top);
            destPoints[1] = new PointF(target.Left + target.Width, target.Top);
            destPoints[2] = new PointF(target.Left, target.Top + target.Height);
            destPoints[3] = new PointF(target.Left + target.Width, target.Top + target.Height);
            if (colors == null)
                colors = new ColorRect(ColorEx.White);
            Renderer.Instance.AddQuad(destPoints, z, clip, this.Texture, this.uvArray, colors);
        }

        public void Draw(Vector3 pos, SizeF target, Rect clip, ColorRect colors) {
            PointF[] destPoints = new PointF[4];
            destPoints[0] = new PointF(pos.x, pos.y);
            destPoints[1] = new PointF(pos.x + target.Width, pos.y);
            destPoints[2] = new PointF(pos.x, pos.y + target.Height);
            destPoints[3] = new PointF(pos.x + target.Width, pos.y + target.Height);
            if (colors == null)
                colors = new ColorRect(ColorEx.White);
            Renderer.Instance.AddQuad(destPoints, pos.z, clip, this.Texture, this.uvArray, colors);
        }

        public void Draw(Vector3 pos, Rect clip, ColorRect colors) {
            this.Draw(pos, this.size, clip, colors);
        }

        public void Draw(Vector3 pos, Rect clip) {
            this.Draw(pos, this.size, clip, null);
        }

        public void Draw(Vector3 pos, SizeF target, ColorRect colors) {
            this.Draw(pos, target, null, colors);
        }

        /// <summary>
        ///   Allow this library to get at the texture atlas we came from
        /// </summary>
        internal TextureAtlas Atlas {
            get {
                return atlas;
            }
        }

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        public Texture Texture {
            get {
                return atlas.texture;
            }
        }

        /// <summary>
        ///   This is only accurate for rectangular texture uv coordinates
        /// </summary>
        public float Width {
            get {
                float deltaU = uvArray[1].X - uvArray[0].X;
                return deltaU * this.Texture.Width;
            }
        }

        /// <summary>
        ///   This is only accurate for rectangular texture uv coordinates
        /// </summary>
        public float Height {
            get {
                float deltaV = uvArray[2].Y - uvArray[0].Y;
                return deltaV * this.Texture.Height;
            }
        }

        /// <summary>
        ///   This is only accurate for rectangular texture uv coordinates
        /// </summary>
        public float Top {
            get {
                return uvArray[0].Y * atlas.texture.Height;
            }
        }
        /// <summary>
        ///   This is only accurate for rectangular texture uv coordinates
        /// </summary>
        public float Left {
            get {
                return uvArray[0].X * atlas.texture.Width;
            }
        }
        /// <summary>
        ///   This is only accurate for rectangular texture uv coordinates
        /// </summary>
        public float Bottom {
            get {
                return uvArray[3].Y * atlas.texture.Height;
            }
        }
        /// <summary>
        ///   This is only accurate for rectangular texture uv coordinates
        /// </summary>
        public float Right {
            get {
                return uvArray[3].X * atlas.texture.Width;
            }
        }

        public Rectangle Rectangle {
            get {
                return new Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
            }
        }
    }
}
