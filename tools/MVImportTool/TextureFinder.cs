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
using System.IO;
using System.Xml;

namespace MVImportTool
{
    /// <summary>
    /// Helper class to discover files referenced in a DAE file.
    /// TODO: It would be nice if this were a service available from the
    /// ColladaMeshReader class.
    /// </summary>
    internal class TextureFinder
    {
        /// <summary>
        /// Textures with validated paths, i.e. the files exist; 
        /// indexed by the "id" attribute on the source image.
        /// </summary>
        public Dictionary<string, FileInfo> Textures
        {
            get { return m_Textures; }
            set { m_Textures = value; }
        }
        Dictionary<string, FileInfo> m_Textures = new Dictionary<string, FileInfo>();

        /// <summary>
        /// Textures that could not be validated, i.e. the files cannot be found;
        /// indexed by the "id" attribute on the source image.
        /// </summary>
        public Dictionary<string, FileInfo> MissingTextures
        {
            get { return m_MissingTextures; }
            set { m_MissingTextures = value; }
        }
        Dictionary<string, FileInfo> m_MissingTextures = new Dictionary<string, FileInfo>();


        internal TextureFinder( string daeFile )
        {
            XmlDocument doc = new XmlDocument();

            m_DaeInfo = GetDaeFileInfo( daeFile );

            doc.Load( m_DaeInfo.FullName );

#if XPATH_WORKS_THE_WAY_I_THINK
            XmlNodeList imageNodes = doc.SelectNodes( "//library_images/image" );

            foreach( XmlElement imageNode in imageNodes )
            {
                // ...evaluate the imageNode...
            }
#else
            XmlNodeList libraryNodes = doc.SelectNodes( "*/*" );

            foreach( XmlElement libNode in libraryNodes )
            {
                if( libNode.Name.Equals( "library_images" ) )
                {
                    ParseNewStyleImageLibrary( libNode );
                }
                else if( libNode.Name.Equals( "library" ) &&
                         libNode.GetAttribute( "type" ).Equals( "IMAGE" ) )
                {
                    ParseOldStyleImageLibrary( libNode );
                }
            }
#endif
        }

        #region Fromage for KMZ (SketchUp) support
        // I call this fromage because we should have been able to contain
        // all knowledge of KMZ in the conversion phase; however, ConversionTool
        // is too broken to make that work, so this kludge is the workaround.
        // TODO: Fix this when ConversionTool gets fixed.
        private FileInfo m_DaeInfo;

        private FileInfo GetDaeFileInfo( string daeFile )
        {
            FileInfo daeInfo = new FileInfo( daeFile );

            if( daeInfo.Extension.EndsWith( ".kmz", StringComparison.CurrentCultureIgnoreCase ) )
            {
                daeInfo = new FileInfo( DaeFileFromKmz( daeInfo ) );
            }

            return daeInfo;
        }

        private string DaeFileFromKmz( FileInfo kmzInfo )
        {
            string baseName = kmzInfo.Name.Substring( 
                0, kmzInfo.Name.LastIndexOf( ".kmz", StringComparison.CurrentCultureIgnoreCase ) );

            string daeName = baseName + ".dae";

            string daeDir = Path.Combine( kmzInfo.DirectoryName, "models" );

            return Path.Combine( daeDir, daeName );
        }
        #endregion Fromage for KMZ (SketchUp) support

        // Use the Collada pre-1.4 schema
        private void ParseOldStyleImageLibrary( XmlElement libNode )
        {
            foreach( XmlNode imageNode in libNode.ChildNodes )
            {
                if( imageNode.Name.Equals( "image" ) )
                {
                    string imageId = (imageNode as XmlElement).GetAttribute( "id" );
                    string imageSource = (imageNode as XmlElement).GetAttribute( "source" );

                    AddImageReference( imageId, imageSource );
                }
            }
        }

        // Use the Collada 1.4 schema
        private void ParseNewStyleImageLibrary( XmlElement libNode )
        {
            foreach( XmlNode imageNode in libNode.ChildNodes )
            {
                if( imageNode.Name.Equals( "image" ) )
                {
                    XmlElement init_fromNode = imageNode.FirstChild as XmlElement;

                    if( null != init_fromNode )
                    {
                        string imageId = (imageNode as XmlElement).GetAttribute( "id" );

                        string imageSource = init_fromNode.InnerText;

                        AddImageReference( imageId, imageSource );
                    }
                }
            }
        }

        // Add a reference to an image source, i.e. a texture file. This checks
        // if the file exists; if it does, the reference is added to the 
        // Textures list, else it is added to the MissingTextures list.
        private void AddImageReference( string imageId, string imageSource )
        {
            FileInfo sourceInfo;

            try
            {
                if( imageSource.StartsWith( "." ) )
                {
                    // Let's infer that this is a path relative to the .dae location
                    sourceInfo = new FileInfo( Path.Combine( m_DaeInfo.DirectoryName, imageSource ) );
                }
                else
                {
                    Uri imageUri = new Uri( imageSource );

                    sourceInfo = new FileInfo( imageUri.LocalPath );
                }
            }
            catch
            {
                // It seems the source is not a well-formed URI, so let's 
                // just take it raw, and hope it is meaningful as a path.
                sourceInfo = new FileInfo( imageSource );
            }

            try
            {
                if( sourceInfo.Exists )
                {
                    if( ! Textures.ContainsKey( imageId ) )
                    {
                        Textures.Add( imageId, sourceInfo );
                    }
                }
                else
                {
                    if( ! MissingTextures.ContainsKey( imageId ) )
                    {
                        MissingTextures.Add( imageId, sourceInfo );
                    }
                }                
            }
            catch( Exception ex )
            {
                Console.Error.WriteLine( 
                    "Caught exception adding texture: id = '{0}'; source = '{1}'", 
                    imageId, imageSource );

                Console.Error.WriteLine( ex.Message );
            }
        }
    }
}
