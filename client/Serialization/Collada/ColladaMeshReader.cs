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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

using Multiverse.Serialization.Collada;

namespace Multiverse.Serialization
{
    /// <summary>
    /// 	Summary description for ColladaMeshReader.
    /// </summary>
    public class ColladaMeshReader
    {
        // Create a logger for use in this class
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof( ColladaMeshReader ) );

        // Name of the target param for a transform matrix
        // const string DefaultTargetParam = "matrix";
        // const string DefaultTargetParam = "";

        #region Member variables

        protected Stream m_Stream;
        protected string m_BaseFile;

        public string MaterialScript
        {
            get
            {
                if( null != m_MaterialBuilder )
                {
                    return m_MaterialBuilder.GetMaterialScript();
                }
                return null;
            }
        }

        internal MaterialScriptBuilder MaterialBuilder
        {
            get { return m_MaterialBuilder; }
        }

        public static bool NoRiggingCulling
        {
            get { return m_NoRiggingCulling; }
            set { m_NoRiggingCulling = value; }
        }

        MaterialScriptBuilder m_MaterialBuilder;

        protected XmlElement m_ColladaRootNode;
        protected XmlDocument m_Document;

        static bool m_NoRiggingCulling = false;

        #endregion

        #region Constructors

        internal ColladaMeshReader()
            : this( null, null )
        {
        }

        public ColladaMeshReader( Stream data, string baseFile )
        {
            m_Stream = data;
            m_BaseFile = baseFile;
        }

        #endregion

        #region Methods

        public void DebugMessage( XmlNode node )
        {
            if( node.NodeType == XmlNodeType.Comment )
                return;
            log.InfoFormat( "Unhandled node type: {0} with parent of {1}", node.Name, node.ParentNode.Name );
        }

        public void DebugMessage( string message, XmlNode node )
        {
            if( node.NodeType == XmlNodeType.Comment )
                return;
            log.InfoFormat( "{0}{1} with parent of {2}", message, node.Name, node.ParentNode.Name );
        }

        public void DebugMessage( XmlNode node, XmlAttribute attr )
        {
            log.InfoFormat( "Unhandled node attribute: {0} with parent node of {1}", attr.Name, node.Name );
        }

        public void Import( Mesh mesh )
        {
            Import( Matrix4.Identity, mesh, null, "idle", null );
        }

        /// <summary>
        ///   Import into the mesh, using the skeleton provided, and 
        ///   assigning the animation data to a new animation.
        /// </summary>
        /// <param name="transform">the world transform to apply to this object</param>
        /// <param name="mesh">the mesh we will populate</param>
        /// <param name="skeleton">the skeleton to which we will add animations (or null if we are creating one)</param>
        /// <param name="animationName">the name that will be used for the animation</param>
        /// <param name="materialNamespace">namespace used for generation of material names</param>
        public void Import( Matrix4 transform, Mesh mesh, Skeleton skeleton, string animationName, string materialNamespace )
        {
            ColladaMeshReader reader = null;

            XmlDocument document = new XmlDocument();
            
            document.Load( m_Stream );

            XmlElement rootElement = document.DocumentElement;

            // This is slightly weird. The client calls this method on this object, 
            // but then we determine which version of collada we're actually looking
            // at, and create a new instances of a derived collada reader.  As an 
            // outcome, we have to copy fields from the factory-created instance 
            // back to this instance.
            // TODO: Need a static factory method on the base class to create the
            // collada reader instance, then call that instance from the client;
            // that way we'll only have one instance in the first place.
            reader = GetColladaParser( rootElement );

            reader.m_ColladaRootNode = rootElement;
            reader.m_Document = document;

            reader.m_MaterialBuilder = new MaterialScriptBuilder( materialNamespace );

            ColladaMeshInfo meshInfo = new ColladaMeshInfo( mesh );

            reader.ReadCollada( rootElement, meshInfo );

            meshInfo.NoRiggingCulling = NoRiggingCulling;

            meshInfo.Process( transform, skeleton, m_BaseFile, animationName );

            this.m_MaterialBuilder = reader.MaterialBuilder;
        }

        public virtual void ReadCollada( XmlNode node, ColladaMeshInfo meshInfo )
        {
            throw new NotImplementedException();
        }

        private ColladaMeshReader GetColladaParser( XmlNode node )
        {
            string ver = node.Attributes[ "version" ].Value;
            int major_version = 0;
            int minor_version = 0;
            int patch_version = 0;
            char[] tokens = new char[ 1 ];
            tokens[ 0 ] = '.';
            string[] ver_numbers = ver.Split( tokens );
            if( ver_numbers.Length > 0 )
                major_version = int.Parse( ver_numbers[ 0 ] );
            if( ver_numbers.Length > 1 )
                minor_version = int.Parse( ver_numbers[ 1 ] );
            if( ver_numbers.Length > 2 )
                patch_version = int.Parse( ver_numbers[ 2 ] );

            switch( major_version )
            {
            case 1:
                switch( minor_version )
                {
                case 3:
                    return new ColladaMeshReader_13();
                case 4:
                    return new ColladaMeshReader_14();
                default:
                    Debug.Assert( false, "Invalid collada version: " + ver );
                    break;
                }
                break;
            default:
                Debug.Assert( false, "Invalid collada version: " + ver );
                break;
            }
            return null;
        }

        #endregion Methods

    }
}
