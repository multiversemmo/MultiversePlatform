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
using System.Drawing.Text;
using System.Drawing;
using System.Text;

namespace Multiverse.Gui
{
    public class FontManager
    {
        static FontManager instance = null;

        protected static PrivateFontCollection pfc = new PrivateFontCollection();
        protected static Dictionary<string, FontFamily> fontDict =
            new Dictionary<string, FontFamily>();

        Dictionary<string, SimpleFont> fonts = new Dictionary<string, SimpleFont>();

        static public FontManager Instance
        {
            // TODO: Lock
            get
            {
                if (instance == null)
                    instance = new FontManager();
                return instance;
            }
        }

        /// <summary>
        ///   Fonts may be in an archive file, but windows requires that they 
        ///   be available as files in the file system.
        /// </summary>
        /// <param name="fontFile"></param>
        /// <returns></returns>
        private static void AddFontFile(string fontFile)
        {
            string fullFontFile =
                AssetManager.Instance.ResolveResourceData("Font", fontFile);
            pfc.AddFontFile(fullFontFile);
            int lastFont = pfc.Families.Length - 1;
            if (lastFont >= 0)
                fontDict[fontFile.ToUpper()] = pfc.Families[lastFont];
        }

        /// <summary>
        ///   Try to get the font family - first by the font file, and if 
        ///   that fails, try based on the font name
        /// </summary>
        /// <param name="fontFile"></param>
        /// <returns></returns>
        public static FontFamily GetFontFamily(string fontFile)
        {
            if (fontDict.ContainsKey(fontFile.ToUpper()))
                return fontDict[fontFile.ToUpper()];
            foreach (FontFamily family in pfc.Families)
            {
                if (family.Name == fontFile)
                    return family;
            }
            return null;
        }

        public static void SetupFonts()
        {
            MVResourceManager resourceManager =
                AssetManager.Instance.GetResourceManager("Font");
            if (resourceManager == null)
                return;
            List<Axiom.FileSystem.Archive> archives =
                resourceManager.GetArchives();

            foreach (Axiom.FileSystem.Archive archive in archives)
            {
                string[] files = archive.GetFileNamesLike("", ".TTF");
                foreach (string file in files)
                    FontManager.AddFontFile(file);
            }
        }

        public bool ContainsKey(string name)
        {
            return fonts.ContainsKey(name);
        }

        public SimpleFont CreateFont(string name, FontFamily family, int height)
        {
            return CreateFont(name, family, height, FontStyle.Regular);
        }

        public SimpleFont CreateFont(string name, FontFamily family, int height, FontStyle style)
        {
            return CreateFont(name, family, height, style, null); ;
        }
        public SimpleFont CreateFont(string name, string fontFace, int height, FontStyle style, string characterSet)
        {
            return CreateFont(name, FontManager.GetFontFamily(fontFace), height, style, characterSet);
        }
        public SimpleFont CreateFont(string name, FontFamily family, int height, FontStyle style, string characterSet)
        {
            SimpleFont rv = null;
            if (characterSet == null)
                rv = new SimpleFont(name, family, height, style, (char)32, (char)127);
            else
                rv = new SimpleFont(name, family, height, style, characterSet);
            // rv.Initialize(name, family, height, style);
            fonts[name] = rv;
            return rv;
        }
        public SimpleFont CreateFont(string name, string fontName, int height, FontStyle style)
        {
            System.Drawing.FontFamily fontFamily = null;
            foreach (System.Drawing.FontFamily family in System.Drawing.FontFamily.Families)
            {
                if (family.Name == fontName)
                {
                    fontFamily = family;
                    break;
                }
            }
            return CreateFont(name, fontFamily, height, style);
        }

        public SimpleFont GetFont(string name)
        {
            return fonts[name];
        }
    }
}
