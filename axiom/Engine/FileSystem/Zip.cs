#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

The .zip archive functionality uses a dynamically linked version of
SharpZipLib (http://www.icsharpcode.net/OpenSource/SharpZipLib/Default.aspx.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using Axiom.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Axiom.FileSystem {
    /// <summary>
    ///    Implementation of Archive that allows for reading resources from a .zip file.
    /// </summary>
    /// <remarks>
    ///    This would also be suitable for reading other .zip like formats, including
    ///    .pk3.
    /// </remarks>
    public class Zip : Archive {
        public Zip(string archiveName) : base(archiveName) {
        }

        public override void Load() {
            LogManager.Instance.Write("Zip Archive codec for {0} created.", name);
        }
        public override void Unload() {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        ///    Reads a file with the specified name in the .zip file and returns the
        ///    file as a MemoryStream.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override Stream ReadFile(string fileName) {
            // read the open the zip archive
            FileStream fs = File.OpenRead(name);
            fs.Position = 0;

            // get a input stream from the zip file
            ZipInputStream s = new ZipInputStream(fs);
            ZipEntry entry;

            // we will put the decompressed data into a memory stream
            MemoryStream output = new MemoryStream();

            // get the first entry 
            entry = s.GetNextEntry();

            // loop through all the entries until we find the requested one
            while (entry != null) {
                if(entry.Name.ToLower() == fileName.ToLower()) {
                    break;
                }

                // look at the next file in the list
                entry = s.GetNextEntry();
            }

            if(entry == null) {
                return null;
            }

            // write the data to the output stream
            int size = 2048;
            byte[] data = new byte[2048];
            while (true) {
                size = s.Read(data, 0, data.Length);
                if (size > 0) {
                    output.Write(data, 0, size);
                } 
                else {
                    break;
                }
            }

            // reset the position to make sure it is at the beginning of the stream
            output.Position = 0;

            return output;
        }

        /// <summary>
        ///    Returns a list of files matching the specified pattern (usually extension) located
        ///    within this .zip file.
        /// </summary>
        /// <param name="startPath"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public override string[] GetFileNamesLike(string startPath, string pattern) {
            FileStream fs = File.OpenRead(name);
            fs.Position = 0;
            ZipInputStream s = new ZipInputStream(fs);
            ZipEntry entry;
            StringCollection fileList = new StringCollection();

            entry = s.GetNextEntry();

            while (entry != null) {

                // get the full path for the output file
                string file = entry.Name;
				
                if(file.EndsWith(pattern)) {
                    fileList.Add(file);
                }

                entry = s.GetNextEntry();
            }
            s.Close();

            string[] files = new string[fileList.Count];
            fileList.CopyTo(files, 0);

            return files;
        }
    }
}
