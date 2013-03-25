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
    class ColladaMeshReader_13 : ColladaMeshReader
    {
        // Mapping from source name to source object
        protected Dictionary<string, Source> Sources
        {
            get { return m_Sources; }
            set { m_Sources = value; }
        }
        private Dictionary<string, Source> m_Sources;


        // Mapping from data source name (typically an array) to the data source
        protected Dictionary<string, DataSource> DataSources
        {
            get { return m_DataSources; }
            set { m_DataSources = value; }
        }
        private Dictionary<string, DataSource> m_DataSources;


        // Mapping from source name (typically an accessor) to accessor
        protected Dictionary<string, Accessor> Accessors
        {
            get { return m_Accessors; }
            set { m_Accessors = value; }
        }
        private Dictionary<string, Accessor> m_Accessors;

        // Combiners
        public Dictionary<string, List<CombinerComponents>> Combiners
        {
            get { return m_Combiners; }
            set { m_Combiners = value; }
        }
        private Dictionary<string, List<CombinerComponents>> m_Combiners;

        public ColladaMeshReader_13()
        {
            m_Sources = new Dictionary<string, Source>();
            m_DataSources = new Dictionary<string, DataSource>();
            m_Accessors = new Dictionary<string, Accessor>();
            m_Combiners = new Dictionary<string, List<CombinerComponents>>();
        }

        #region Xml Parsing Methods

        public virtual void ReadLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string type = node.Attributes[ "type" ].Value;
            switch( type )
            {
            case "IMAGE":
                ReadImageLibrary( node, meshInfo );
                break;
            case "TEXTURE":
                ReadTextureLibrary( node, meshInfo );
                break;
            case "MATERIAL":
                ReadMaterialLibrary( node, meshInfo );
                break;
            case "GEOMETRY":
                ReadGeometryLibrary( node, meshInfo );
                break;
            case "CONTROLLER":
                ReadControllerLibrary( node, meshInfo );
                break;
            case "ANIMATION":
                ReadAnimationLibrary( node, meshInfo );
                break;
            default:
                log.InfoFormat( "Ignoring unhandled library type: {0}", type );
                break;
            }
        }

        /// <summary>
        ///    Reads bone information from the file.
        /// </summary>
        public override void ReadCollada( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "library":
                    ReadLibrary( childNode, meshInfo );
                    break;
                case "asset":
                    ReadAsset( childNode, meshInfo );
                    break;
                case "scene":
                    ReadScene( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public virtual void ReadAsset( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "unit":
                    ReadUnit( childNode, meshInfo );
                    break;
                case "up_axis":
                    ReadUpAxis( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadUnit(XmlNode node, ColladaMeshInfo meshInfo)
        {
            string str = node.Attributes["meter"].Value;
            meshInfo.SourceUnits = float.Parse(str);
        }

        public void ReadUpAxis(XmlNode node, ColladaMeshInfo meshInfo)
        {
            string str = node.InnerText.Trim();
            meshInfo.UpAxis = str;
        }

        public void ReadTextureLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "texture":
                    ReadTexture( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadMaterialLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "material":
                    ReadMaterial( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public virtual void ReadImageLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "image":
                    ReadImage( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadGeometryLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "geometry":
                    ReadGeometry( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadControllerLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
#if UNORDERED
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "controller":
                        ReadController(childNode, meshInfo);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
#else
            XmlElement libElement = node as XmlElement;

            List<XmlNode> orderedControllerNodes = new List<XmlNode>();

            foreach( XmlNode childNode in node.ChildNodes )
            {
                if( IsMorphController( childNode ) )
                {
                    orderedControllerNodes.Add( childNode );
                }
            }
            foreach( XmlNode childNode in node.ChildNodes )
            {
                if( IsSkinController( childNode ) )
                {
                    orderedControllerNodes.Add( childNode );
                }
            }
            foreach( XmlNode childNode in orderedControllerNodes )
            {
                ReadController( childNode, meshInfo );
            }
#endif
        }

        bool IsMorphController( XmlNode node )
        {
            return node.FirstChild.Name.Equals( "morph" );
        }

        bool IsSkinController( XmlNode node )
        {
            return node.FirstChild.Name.Equals( "skin" );
        }

        public void ReadAnimationLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "animation":
                    ReadAnimation( null, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public virtual void ReadMaterial( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            List<MaterialTechnique_13> materialTechniques = new List<MaterialTechnique_13>();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "shader":
                    ReadShader( materialTechniques, childNode );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

            MaterialBuilder.AddTechniques( id, materialTechniques );
        }

        public void ReadShader( List<MaterialTechnique_13> materialTechniques, XmlNode node )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "technique":
                    {
                        MaterialTechnique_13 mt = ReadMaterialTechnique( childNode );
                        if( mt != null )
                            materialTechniques.Add( mt );
                    }
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public MaterialTechnique_13 ReadMaterialTechnique( XmlNode node )
        {
            string profile = node.Attributes[ "profile" ].Value;
            if( !profile.Equals( "COMMON" ) )
            {
                log.WarnFormat( "Skipping material technique: profile is not 'COMMON'" );
                return null;
            }
            MaterialTechnique_13 mt = new MaterialTechnique_13();
            mt.passes = new List<TechniquePass>();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "pass":
                    {
                        TechniquePass tp = ReadTechniquePass( childNode );
                        if( tp != null )
                            mt.passes.Add( tp );
                        else
                            Debug.Assert( false, "Invalid technique pass" );
                    }
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            return mt;
        }

        public TechniquePass ReadTechniquePass( XmlNode node )
        {
            TechniquePass tp = new TechniquePass();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadTechniquePassInput( tp, childNode );
                    break;
                case "program":
                    ReadTechniquePassProgram( tp, childNode );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            return tp;
        }

        public void ReadTechniquePassInput( TechniquePass tp, XmlNode node )
        {
            string semantic = node.Attributes[ "semantic" ].Value;
            if( !semantic.Equals( "TEXTURE" ) )
                return;
            string source = node.Attributes[ "source" ].Value;
            // strip off the leading '#'
            source = source.Substring( 1 );
            tp.textureId = source;
        }

        public void ReadTechniquePassProgram( TechniquePass tp, XmlNode node )
        {
            string url = node.Attributes[ "url" ].Value;
            switch( url )
            {
            case "PHONG":
            case "LAMBERT":
                tp.shader = "phong";
                break;
            case "CONSTANT":
            default:
                DebugMessage( node );
                break;
            }
        }

        public virtual void ReadImage( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            string source = node.Attributes[ "source" ].Value;
            int slashIndex = source.LastIndexOf( '/' );
            if( slashIndex >= 0 )
                source = source.Substring( slashIndex + 1 );
            MaterialBuilder.AddImage( id, source );
        }

        public void ReadTexture( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            List<TextureTechnique> techniques = new List<TextureTechnique>();

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "technique":
                    {
                        TextureTechnique technique = ReadTextureTechnique( childNode );
                        if( technique != null )
                            techniques.Add( technique );
                    }
                    break;
                case "param":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            MaterialBuilder.AddTextureTechniques( id, techniques );
        }

        public TextureTechnique ReadTextureTechnique( XmlNode node )
        {
            string profile = node.Attributes[ "profile" ].Value;
            if( !profile.Equals( "COMMON" ) )
                return null;
            TextureTechnique technique = new TextureTechnique();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadTechniqueInput( technique, childNode );
                    break;
                case "param":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            return technique;
        }

        public void ReadTechniqueInput( TextureTechnique technique, XmlNode node )
        {
            string semantic = node.Attributes[ "semantic" ].Value;
            if( semantic != "IMAGE" )
                return;
            string source = node.Attributes[ "source" ].Value;
            // strip off the leading '#'
            source = source.Substring( 1 );
            technique.imageId = source;
        }

        public void ReadGeometry( XmlNode node, ColladaMeshInfo meshInfo )
        {
            // Set up the current geometry name
            string geometryName = node.Attributes[ "id" ].Value;

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "mesh":
                    ReadMesh( geometryName, childNode, meshInfo );
                    break;
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public virtual void ReadController( XmlNode node, ColladaMeshInfo meshInfo )
        {
            // Set up the current geometry name
            string controllerName = node.Attributes[ "id" ].Value;
            string target = node.Attributes[ "target" ].Value;

            SkinController controller = new SkinController( controllerName );
            controller.Target = meshInfo.Geometries[ target ];

            meshInfo.Controllers[ controllerName ] = controller;

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "skin":
                    ReadSkin( controller, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

        }

        public virtual void ReadAnimation( string parentAnimationName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string animationName = null;
            if( node.Attributes[ "id" ] != null )
                animationName = node.Attributes[ "id" ].Value;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                    ReadSource( null, childNode, meshInfo );
                    break;
                case "sampler":
                    ReadSampler( childNode, meshInfo );
                    break;
                case "channel":
                    ReadChannel( parentAnimationName, animationName, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public virtual void ReadMesh( string geometryName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            Dictionary<int, List<InputSourceCollection>> inputSources =
                new Dictionary<int, List<InputSourceCollection>>();
            Dictionary<string, VertexSet> vertexSets = new Dictionary<string, VertexSet>();

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                    ReadSource( null, childNode, meshInfo );
                    break;
                case "vertices":
                    // Read in inputs and vertex sets that should apply to all
                    // the sub-portions of this geometry.
                    ReadVertices( vertexSets, childNode, meshInfo );
                    break;
                case "lines":
                case "linestrips":
                case "triangles":
                case "trifans":
                case "tristrips":
                case "polygons":
                    // Handle this in the second pass
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

            int submeshIndex = 0;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                case "vertices":
                    // these were handled in the first pass
                    break;
                case "polygons":
                case "triangles":
                    {
                        string name = string.Format( "{0}.{1}", geometryName, submeshIndex++ );
                        if( !meshInfo.Geometries.ContainsKey( geometryName ) )
                            meshInfo.Geometries[ geometryName ] = new GeometrySet( geometryName );
                        GeometrySet geoSet = meshInfo.Geometries[ geometryName ];
                        MeshGeometry geometry = new MeshGeometry( name, geoSet );
                        foreach( VertexSet vertexSet in vertexSets.Values )
                            geometry.AddVertexSet( vertexSet );
                        foreach( int inputIndex in inputSources.Keys )
                            geometry.AddInputs( inputIndex, inputSources[ inputIndex ] );
                        geoSet.Add( geometry );
                        ReadPolygons( geometry, vertexSets, childNode, meshInfo );
                        break;
                    }
                case "lines":
                case "linestrips":
                case "trifans":
                case "tristrips":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public virtual void ReadSkin( SkinController controller, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                    ReadSource( controller, childNode, meshInfo );
                    break;
                case "vertices":
                    ReadSkinVertices( controller, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadSampler( XmlNode node, ColladaMeshInfo meshInfo )
        {
            // the spec shows the id attribute as optional, 
            // but a sampler without an id is pointless 
            // (cannot be referenced by a channel)
            string samplerId = node.Attributes[ "id" ].Value;
            Sampler sampler = new Sampler( samplerId );
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadSamplerInput( sampler, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            // TODO: Put the sampler in a dictionary
            if( sampler.Input == null || sampler.Output == null )
            {
                log.InfoFormat( "Ignoring incomplete sampler: {0}", samplerId );
                return;
            }
            meshInfo.Samplers[ sampler.SamplerId ] = sampler;
        }


        public void ReadSamplerInput( Sampler sampler, XmlNode node, ColladaMeshInfo meshInfo )
        {
            InputSourceCollection tmp = ReadInput( node, meshInfo );
            Debug.Assert( tmp.GetSources().Count == 1 );
            InputSource entry = tmp.GetSources()[ 0 ];
            switch( entry.Semantic )
            {
            case "INPUT":
                sampler.Input = entry;
                break;
            case "OUTPUT":
                sampler.Output = entry;
                break;
            case "INTERPOLATION":
                sampler.Interpolation = entry;
                break;
            default:
                log.WarnFormat( "Unhandled sampler semantic: {0}", entry.Semantic );
                break;
            }
        }

        public virtual void ReadChannel( string parentAnimationName, string animationName,
                                        XmlNode node, ColladaMeshInfo meshInfo )
        {
            string samplerId = node.Attributes[ "source" ].Value;
            string targetId = node.Attributes[ "target" ].Value;
            // Strip off the leading '#'
            samplerId = samplerId.Substring( 1 );
            Sampler sampler = meshInfo.Samplers[ samplerId ];
            Channel channel = new Channel( sampler, targetId, "matrix" );
            channel.AnimationName = animationName;
            channel.ParentAnimationName = parentAnimationName;
            meshInfo.Channels.Add( channel );
        }


        /// <summary>
        ///   Read in a data source, including the technique, accessors, 
        ///   joints, and combiners.  Sometimes this is in the context of 
        ///   a controller, in which case controller is not null.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="node"></param>
        public virtual void ReadSource( Controller controller, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string sourceId = node.Attributes[ "id" ].Value;
            Source source = null;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "array":
                    {
                        string arrayId = childNode.Attributes[ "id" ].Value;
                        DataSource dataSource = new DataSource( arrayId );
                        ReadWeaklyTypedArray( dataSource, childNode, meshInfo );
                        source = new Source( sourceId, dataSource );
                    }
                    break;
                case "bool_array":
                    {
                        string arrayId = childNode.Attributes[ "id" ].Value;
                        DataSource dataSource = new DataSource( arrayId );
                        ReadBoolArray( dataSource, childNode, meshInfo );
                        source = new Source( sourceId, dataSource );
                    }
                    break;
                case "float_array":
                    {
                        string arrayId = childNode.Attributes[ "id" ].Value;
                        DataSource dataSource = new DataSource( arrayId );
                        ReadFloatArray( dataSource, childNode, meshInfo );
                        source = new Source( sourceId, dataSource );
                    }
                    break;
                case "int_array":
                    {
                        string arrayId = childNode.Attributes[ "id" ].Value;
                        DataSource dataSource = new DataSource( arrayId );
                        ReadIntArray( dataSource, childNode, meshInfo );
                        source = new Source( sourceId, dataSource );
                    }
                    break;
                case "Name_array":
                    {
                        string arrayId = childNode.Attributes[ "id" ].Value;
                        DataSource dataSource = new DataSource( arrayId );
                        ReadStringArray( dataSource, childNode, meshInfo );
                        source = new Source( sourceId, dataSource );
                    }
                    break;
                case "technique":
                    break; // Skip these in the first pass
                }
            }
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "array":
                case "bool_array":
                case "float_array":
                case "int_array":
                case "Name_array":
                    break; // Skip these in the second pass
                case "technique":
                    ReadTechnique( controller, source, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            Sources[ sourceId ] = source;
        }

        public void ReadSkinVertices( SkinController controller,
                                     XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadSkinVerticesInput( controller, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadSkinVerticesInput( SkinController controller,
                                          XmlNode node, ColladaMeshInfo meshInfo )
        {
            string semantic = node.Attributes[ "semantic" ].Value;
            switch( semantic )
            {
            case "BIND_SHAPE_POSITION":
            case "BIND_SHAPE_NORMAL":
                ReadInput( controller.InputSources, null, node, meshInfo );
                return;
            case "JOINTS_AND_WEIGHTS":
                {
                    string source = node.Attributes[ "source" ].Value;
                    // string off leading '#'
                    source = source.Substring( 1 );
                    controller.Target.Combiner = Combiners[ source ];
                }
                break;
            default:
                log.InfoFormat( "Not currently handling semantic {0} for skin vertices input", semantic );
                break;
            }
        }

        public void ReadVertices( Dictionary<string, VertexSet> vertexSets,
                                    XmlNode node, ColladaMeshInfo meshInfo )
        {
            string verticesId = string.Empty;
            if( node.Attributes[ "id" ] != null )
                verticesId = node.Attributes[ "id" ].Value;
            else
            {
                DebugMessage( node );
                // vertices inside of a controller don't have the id
                // Since I don't handle bone weights (the data in these),
                // just skip this for now.  TODO: Fix
                return;
            }
            VertexSet vertexSet = new VertexSet();
            vertexSet.id = verticesId;
            vertexSets[ vertexSet.id ] = vertexSet;

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    {
                        InputSourceCollection entry = ReadInput( childNode, meshInfo );
                        vertexSet.vertexEntries.Add( entry );
                        break;
                    }
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        /// <summary>
        ///   ReadPolygons basically corresponds to reading in a SubMesh.
        ///   The grandparent of this node is probably the geometry node
        ///   with the submesh name.
        /// </summary>
        /// <param name="node">The node corresponding to the 'polygons' element</param>
        public void ReadPolygons( MeshGeometry geometry,
                                    Dictionary<string, VertexSet> vertexSets,
                                    XmlNode node, ColladaMeshInfo meshInfo )
        {
            bool doubleSided = false;

            Dictionary<int, List<InputSourceCollection>> inputSources =
                new Dictionary<int, List<InputSourceCollection>>();
            // First pass to get params and inputs
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "param":
                    if( childNode.Attributes[ "name" ].Value == "DOUBLE_SIDED" )
                        doubleSided = bool.Parse( childNode.InnerText );
                    break;
                case "input":
                    ReadInput( inputSources, vertexSets, childNode, meshInfo );
                    break;
                case "p":
                    // ignore on this pass
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            foreach( int inputIndex in inputSources.Keys )
                geometry.AddInputs( inputIndex, inputSources[ inputIndex ] );

            // TODO: If we are double sided, we probably need to build a
            // negated version of the normals.

            // second pass to handle the 'p' entries
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "param":
                case "input":
                    // ignore on this pass
                    break;
                case "p":
                    ReadPolygon( geometry, doubleSided, childNode );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            string materialId = null;
            string submeshName = geometry.Id;
            if( node.Attributes[ "material" ] != null &&
                node.Attributes[ "material" ].Value != null )
            {
                materialId = node.Attributes[ "material" ].Value;
                // strip off the leading '#'
                if( materialId.StartsWith( "#" ) )
                {
                    log.InfoFormat( "Material {0} starts with '#'", materialId );
                    materialId = materialId.Substring( 1 );
                }
                // I used to append the material name to the submesh name,
                // but now I want to leave it alone.
                //submeshName = submeshName + "/" + materialId;
            }
            SubMeshInfo smInfo = new SubMeshInfo();
            smInfo.name = submeshName;
            if( materialId != null )
            {
                if( MaterialScriptBuilder.MaterialNamespace != null )
                    smInfo.material = MaterialScriptBuilder.MaterialNamespace + "." + materialId;
                else
                    smInfo.material = materialId;
            }
            else
            {
                smInfo.material = "BaseWhite";
            }
            geometry.AddSubMesh( smInfo );
        }


        #region Array handling

        public void ReadWeaklyTypedArray( DataSource source, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string type = "string";

            if( node.Attributes[ "type" ] != null )
                type = node.Attributes[ "type" ].Value;

            switch( type )
            {
            case "float":
                ReadFloatArray( source, node, meshInfo );
                break;
            case "string":
            case "Name":
                ReadStringArray( source, node, meshInfo );
                break;
            default:
                log.WarnFormat( "Unhandled array type: {0}", type );
                break;
            }
        }

        public void ReadBoolArray( DataSource source, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            int count = int.Parse( node.Attributes[ "count" ].Value );
            DataSources[ id ] = source;
            FillArray( source.BoolData, node.InnerText );
            Debug.Assert( source.BoolData.Count == count );
        }

        public void ReadFloatArray( DataSource source, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            int count = int.Parse( node.Attributes[ "count" ].Value );
            DataSources[ id ] = source;
            FillArray( source.FloatData, node.InnerText );
            Debug.Assert( source.FloatData.Count == count );
        }

        public void ReadIntArray( DataSource source, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            int count = int.Parse( node.Attributes[ "count" ].Value );
            DataSources[ id ] = source;
            FillArray( source.IntData, node.InnerText );
            Debug.Assert( source.IntData.Count == count );
        }

        public void ReadStringArray( DataSource source, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            int count = int.Parse( node.Attributes[ "count" ].Value );
            DataSources[ id ] = source;
            FillArray( source.StringData, node.InnerText );
            Debug.Assert( source.StringData.Count == count );
        }

        void FillArray( List<bool> values, string data )
        {
            string[] data_entries = data.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < data_entries.Length; ++i )
                values.Add( bool.Parse( data_entries[ i ] ) );
        }
        void FillArray( List<float> values, string data )
        {
            string[] data_entries = data.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < data_entries.Length; ++i )
                values.Add( float.Parse( data_entries[ i ] ) );
        }
        void FillArray( List<int> values, string data )
        {
            string[] data_entries = data.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < data_entries.Length; ++i )
                values.Add( int.Parse( data_entries[ i ] ) );
        }
        void FillArray( List<string> values, string data )
        {
            string[] data_entries = data.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            for( int i = 0; i < data_entries.Length; ++i )
                values.Add( data_entries[ i ] );
        }

        #endregion Array handling

        public void ReadTechnique( Controller controller,
                                  Source source, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "accessor":
                    {
                        // default the accessor id to the source id
                        string accessorId = source.SourceId;
                        if( childNode.Attributes[ "id" ] != null )
                            accessorId = childNode.Attributes[ "id" ].Value;
                        string arrayId = childNode.Attributes[ "source" ].Value;
                        // Get the part after the '#'
                        if( arrayId.StartsWith( "#" ) )
                            arrayId = arrayId.Substring( 1 );
                        else
                            log.InfoFormat( "Array Id {0} does not start with '#'", arrayId );
                        Debug.Assert( DataSources.ContainsKey( arrayId ) );
                        DataSource dataSource = DataSources[ arrayId ];
                        // TODO: Check stride and offset
                        Accessor accessor = new Accessor( dataSource, accessorId );
                        int stride = -1;
                        if( childNode.Attributes[ "stride" ] != null )
                            stride = int.Parse( childNode.Attributes[ "stride" ].Value );
                        accessor.Stride = stride;
                        ReadAccessor( accessor, childNode );
                        // Link this accessor in both as the source, and the id, which
                        // allows us to reference it by either name
                        Accessors[ accessorId ] = accessor;
                        Accessors[ source.SourceId ] = accessor;
                        source.AddAccessor( accessor );
                    }
                    break;
                case "combiner":
                    ReadCombiner( source.SourceId, childNode, meshInfo );
                    break;
                case "joints":
                    {
                        SkinController skinController = controller as SkinController;
                        Debug.Assert( skinController != null );
                        ReadJoints( skinController, childNode, meshInfo );
                    }
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadCombiner( string currentSource, XmlNode node, ColladaMeshInfo meshInfo )
        {
            Dictionary<int, List<InputSourceCollection>> inputSources =
                new Dictionary<int, List<InputSourceCollection>>();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadInput( inputSources, null, childNode, meshInfo );
                    break;
                case "v":
                    // Ignore in this pass
                    break;
                default:
                    break;
                }
            }
            List<CombinerComponents> combComps = new List<CombinerComponents>();
            int vertexIndex = 0;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    // Ignore in this pass
                    break;
                case "v":
                    {
                        CombinerComponents cc = ReadCombinerV( vertexIndex++, inputSources, childNode );
                        if( cc != null )
                            combComps.Add( cc );
                    }
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            Combiners[ currentSource ] = combComps;
        }

        public void ReadJoints( SkinController controller,
                               XmlNode node, ColladaMeshInfo meshInfo )
        {
            Dictionary<int, List<InputSourceCollection>> inputSources =
                new Dictionary<int, List<InputSourceCollection>>();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadInput( inputSources, null, childNode, meshInfo );
                    break;
                default:
                    break;
                }
            }
            // process joints here
            InputSource jointInput = null;
            InputSource invBindMatrixInput = null;
            foreach( List<InputSourceCollection> inputs in inputSources.Values )
            {
                foreach( InputSourceCollection tmp in inputs )
                {
                    Debug.Assert( tmp.GetSources().Count == 1 );
                    InputSource source = tmp.GetSources()[ 0 ];
                    switch( source.Semantic )
                    {
                    case "JOINT":
                        jointInput = source;
                        break;
                    case "INV_BIND_MATRIX":
                        invBindMatrixInput = source;
                        break;
                    default:
                        log.InfoFormat( "Unhandled joint input semantic: {0}", source.Semantic );
                        break;
                    }
                }
            }
            Debug.Assert( jointInput != null && invBindMatrixInput != null );
            string jointParam = "";
            if( jointInput.Accessor.ContainsParam( "JOINT" ) )
                jointParam = "JOINT";
            string invBindParam = "";
            if( invBindMatrixInput.Accessor.ContainsParam( "INV_BIND_MATRIX" ) )
                invBindParam = "INV_BIND_MATRIX";
            else if( invBindMatrixInput.Accessor.ContainsParam( "TRANSFORM" ) )
                invBindParam = "TRANSFORM";
            for( int ptIndex = 0; ptIndex < jointInput.Count; ++ptIndex )
            {
                string jointName = (String) jointInput.Accessor.GetParam( jointParam, ptIndex );
                Matrix4 invBindMatrix = (Matrix4) invBindMatrixInput.Accessor.GetParam( invBindParam, ptIndex );
                if( controller.InverseBindMatrices.ContainsKey( jointName ) )
                {
                    string msg =
                        string.Format( "Duplicate inverse bind matrix found in controller '{0}' for joint '{1}'; Aborting export.",
                                      controller.Name, jointName );

                    log.Error( msg );
                    throw new Exception( msg );
                }
                // Debug.Assert(controller.InvBindTransforms[jointName] == invBindMatrix);
                controller.InverseBindMatrices[ jointName ] = invBindMatrix;
            }
        }

        public CombinerComponents ReadCombinerV( int vertexIndex,
                                                Dictionary<int, List<InputSourceCollection>> inputSources,
                                                XmlNode node )
        {
            CombinerComponents cc = new CombinerComponents( vertexIndex, inputSources );

            string[] indices = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            int inputCount = inputSources.Count; // technically, i want the maximum key
            int influenceCount = indices.Length / inputCount;
            for( int influence = 0; influence < influenceCount; ++influence )
            {
                CombinerEntry entry = new CombinerEntry();
                cc.Add( entry );
                // Build the entry to be the right size
                while( entry.Count < inputSources.Count )
                    entry.Add( -1 );
                foreach( int inputIndex in inputSources.Keys )
                    entry[ inputIndex ] = int.Parse( indices[ influence * inputCount + inputIndex ] );
            }
            return cc;
        }

        public virtual void ReadPolygon( MeshGeometry geometry, bool doubleSided, XmlNode node )
        {
            string[] indices = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            int inputCount = geometry.InputCount;
            // How many vertices are in this polygon
            int vertexCount = indices.Length / inputCount;
            // Indices (into currentPoint) of the points in a polygon.
            // This is temporary, and is reset for each polygon
            List<int> cwPolyPoints = geometry.BeginPolygon();
            List<int> ccwPolyPoints = geometry.BeginPolygon();
            for( int vertex = 0; vertex < vertexCount; ++vertex )
            {
                PointComponents currentPoint = geometry.BeginPoint();
                foreach( int inputIndex in geometry.InputSources.Keys )
                {
                    List<InputSourceCollection> inputs = geometry.InputSources[ inputIndex ];
                    int ptIndex = int.Parse( indices[ vertex * inputCount + inputIndex ] );
                    foreach( InputSourceCollection isc in inputs )
                    {
                        foreach( InputSource source in isc.GetSources() )
                            geometry.AddPointComponent( currentPoint, ptIndex );
                        // Set the vertex index of this point.  This is used by 
                        // combiners, which reference vertices by their position 
                        // in the vertex array.
                        if( isc is VertexInputSource )
                            currentPoint.VertexIndex = ptIndex;
                    }
                }
                geometry.EndPoint( currentPoint, cwPolyPoints, ccwPolyPoints );
            }
            geometry.EndPolygon( cwPolyPoints );
            if( doubleSided )
            {
                ccwPolyPoints.Reverse();
                geometry.EndPolygon( ccwPolyPoints );
            }
        }

        public InputSourceCollection ReadInput( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string sourceId = node.Attributes[ "source" ].Value;
            // Get the part after the '#'
            sourceId = sourceId.Substring( 1 );
            string semantic = node.Attributes[ "semantic" ].Value;
            // offset source semantic set

            Accessor accessor = null;

            if( !Accessors.ContainsKey( sourceId ) )
            {
                if( Sources.ContainsKey( sourceId ) )
                {
                    Source source = Sources[ sourceId ];
                    Debug.Assert( source.Accessors.Count == 1 );
                    accessor = Accessors[ source.Accessors[ 0 ].AccessorId ];
                    return new InputSource( sourceId, semantic, accessor );
                }
                // TODO: Check combiners as well.
                Debug.Assert( false, "Missing accessor for source: " + sourceId );
                return null;
            }

            accessor = Accessors[ sourceId ];
            return new InputSource( sourceId, semantic, accessor );
        }

        public virtual void ReadInput( Dictionary<int, List<InputSourceCollection>> inputSources,
                                      Dictionary<string, VertexSet> vertexSets,
                                      XmlNode node, ColladaMeshInfo meshInfo )
        {
            InputSourceCollection input = null;
            string source = node.Attributes[ "source" ].Value;
            // Get the part after the '#'
            source = source.Substring( 1 );
            string semantic = node.Attributes[ "semantic" ].Value;
            int inputIndex = inputSources.Count;
            if( node.Attributes[ "idx" ] != null )
                inputIndex = int.Parse( node.Attributes[ "idx" ].Value );
            if( !inputSources.ContainsKey( inputIndex ) )
                inputSources[ inputIndex ] = new List<InputSourceCollection>();
            if( semantic == "VERTEX" )
            {
                VertexSet vertexSet = vertexSets[ source ];
                // Dereference the vertex input and add that instead.
                if( inputSources != null )
                {
                    VertexInputSource vertexInput = new VertexInputSource();
                    foreach( InputSourceCollection tmp in vertexSet.vertexEntries )
                        vertexInput.AddSource( tmp );
                    inputSources[ inputIndex ].Add( vertexInput );
                }
            }
            else
            {
                if( !Accessors.ContainsKey( source ) )
                {
                    Debug.Assert( false, "Missing accessor for source: " + source );
                    return;
                }
                Accessor accessor = Accessors[ source ];
                if( inputSources != null )
                {
                    input = new InputSource( source, semantic, accessor );
                    inputSources[ inputIndex ].Add( input );
                }
            }
        }

        public void ReadAccessor( Accessor accessor, XmlNode node )
        {
            bool stride_set = accessor.Stride >= 0;
            int inferred_stride = 0;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "param":
                    {
                        string paramName = "";
                        if( childNode.Attributes[ "name" ] != null )
                            paramName = childNode.Attributes[ "name" ].Value;
                        string paramType = childNode.Attributes[ "type" ].Value;
                        accessor.AddParam( paramName, paramType );
                        if( !stride_set )
                            inferred_stride += accessor.GetAccessorParam( paramName ).DefaultSize;
                    }
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            if( !stride_set )
                accessor.Stride = inferred_stride;
        }

        public virtual void ReadScene( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "node":
                    ReadNode( new List<NamedTransform>(), null, childNode, meshInfo );
                    break;
                case "extra":
                    ReadExtra( childNode, meshInfo );
                    break;
                case "lookat":
                case "matrix":
                case "perspective":
                case "rotate":
                case "scale":
                case "skew":
                case "translate":
                case "boundingbox":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

        }

        public string GetSid( XmlNode node )
        {
            XmlAttribute attr = node.Attributes[ "sid" ];
            if( attr == null )
                return string.Empty;
            return attr.Value;
        }

        public virtual void ReadNode( List<NamedTransform> parentTransforms, string parentBone, XmlNode node, ColladaMeshInfo meshInfo )
        {
            List<NamedTransform> transformChain = new List<NamedTransform>();
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "rotate":
                    {
                        float angle = 0;
                        Vector3 axis = Vector3.UnitY;
                        ReadRotate( ref angle, ref axis, childNode );
                        transformChain.Add( new NamedRotateTransform( GetSid( childNode ), angle, axis ) );
                    }
                    break;
                case "scale":
                    {
                        Vector3 scale = Vector3.UnitScale;
                        ReadVector( ref scale, childNode );
                        transformChain.Add( new NamedScaleTransform( GetSid( childNode ), scale ) );
                    }
                    break;
                case "translate":
                    {
                        Vector3 translation = Vector3.Zero;
                        ReadVector( ref translation, childNode );
                        transformChain.Add( new NamedTranslateTransform( GetSid( childNode ), translation ) );
                    }
                    break;
                case "skew":
                    {
                        // FIXME: For now, just handle skew with a matrix
                        Matrix4 matrix = Matrix4.Identity;
                        ReadSkewMatrix( ref matrix, childNode );
                        transformChain.Add( new NamedMatrixTransform( GetSid( childNode ), matrix ) );
                    }
                    break;
                case "matrix":
                    {
                        Matrix4 matrix = Matrix4.Identity;
                        ReadMatrix( ref matrix, childNode );
                        transformChain.Add( new NamedMatrixTransform( GetSid( childNode ), matrix ) );
                    }
                    break;
                case "instance":
                case "node":
                case "lookat":
                case "perspective":
                case "boundingbox":
                case "extras":
                default:
                    break;
                }
            }

            if( node.Attributes[ "type" ] != null &&
                node.Attributes[ "id" ] != null )
            {
                string nodeId = node.Attributes[ "id" ].Value;
                switch( node.Attributes[ "type" ].Value )
                {
                case "JOINT":
                    {
                        meshInfo.JointTransformChains[ nodeId ] = transformChain;
                        meshInfo.BoneParents[ nodeId ] = parentBone;
                        parentBone = nodeId;
                    }
                    break;
                case "NODE":
                    // the default
                    break;
                default:
                    DebugMessage( node );
                    break;
                }
            }

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "instance":
                    {
                        Matrix4 transform = Matrix4.Identity;
                        foreach( NamedTransform t in transformChain )
                        {
                            transform = t.Transform * transform;
                            Debug.Assert( transform != Matrix4.Zero );
                        }
                        Matrix4 localTransform = transform;
                        foreach( NamedTransform t in parentTransforms )
                        {
                            transform = t.Transform * transform;
                            Debug.Assert( transform != Matrix4.Zero );
                        }

                        if( IsInstanceGeometry( childNode, meshInfo ) )
                        {
                            ReadInstanceGeometry( null, parentBone, localTransform, transform, childNode, meshInfo );
                        }
                        else
                        {
                            ReadInstanceController( null, parentBone, localTransform, transform, childNode, meshInfo );
                        }
                        break;
                    }
                case "node":
                    ReadNode( transformChain, parentBone, childNode, meshInfo );
                    break;
                case "rotate":
                case "scale":
                case "translate":
                case "skew":
                case "matrix":
                    break;
                case "lookat":
                case "perspective":
                case "boundingbox":
                case "extras":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadExtra( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "technique":
                    ReadExtraTechnique( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadExtraTechnique( XmlNode node, ColladaMeshInfo meshInfo )
        {
            XmlAttribute attr = node.Attributes[ "profile" ];
            if( attr != null && attr.Value == "MAYA" )
            {
                log.Info( "Handling maya technique" );
                foreach( XmlNode childNode in node.ChildNodes )
                {
                    switch( childNode.Name )
                    {
                    case "param":
                        ReadExtraTechniqueParam( childNode, meshInfo );
                        break;
                    default:
                        DebugMessage( childNode );
                        break;
                    }
                }
            }
            else
            {
                log.InfoFormat( "Skipping extra technique with unknown profile: {0}", attr );
            }
        }

        /// <summary>
        ///   Right now, I use this to set the layer of objects to avoid drawing controls
        /// </summary>
        /// <param name="node"></param>
        public void ReadExtraTechniqueParam( XmlNode node, ColladaMeshInfo meshInfo )
        {
            XmlAttribute attr;
            attr = node.Attributes[ "type" ];
            if( attr == null || attr.Value != "layer" )
            {
                log.InfoFormat( "Skipping extra technique parameter with unknown type: {0}", attr );
                return;
            }
            attr = node.Attributes[ "name" ];
            if( attr == null || attr.Value == "" )
            {
                log.Info( "Skipping unnamed layer in extra technique parameter" );
                return;
            }
            string layer = attr.Value;
            // Ok, looks like these are the names of nodes that should not be displayed
            string[] values = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            foreach( string val in values )
                meshInfo.Layers[ val ] = layer;
        }

        public void ReadRotate( ref float angle, ref Vector3 axis, XmlNode node )
        {
            string[] values = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            Debug.Assert( values.Length == 4 );
            axis = Vector3.Zero;
            for( int i = 0; i < 3; ++i )
                axis[ i ] = float.Parse( values[ i ] );
            angle = MathUtil.DegreesToRadians( float.Parse( values[ 3 ] ) );
        }

        public void ReadVector( ref Vector3 vec, XmlNode node )
        {
            string[] values = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            Debug.Assert( values.Length == 3 );
            for( int i = 0; i < 3; ++i )
                vec[ i ] = float.Parse( values[ i ] );
        }

        public void ReadMatrix( ref Matrix4 matrix, XmlNode node )
        {
            string[] values = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            Debug.Assert( values.Length == 16 );
            for( int i = 0; i < 16; ++i )
                matrix[ i ] = float.Parse( values[ i ] );
        }
        public void ReadSkewMatrix( ref Matrix4 matrix, XmlNode node )
        {
            string[] values = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            Debug.Assert( values.Length == 7 );
            float angle = float.Parse( values[ 0 ] );
            if( angle == 0 )
                return;
            angle = MathUtil.RadiansToDegrees( angle );

            Vector3 axis = new Vector3();
            axis.x = float.Parse( values[ 0 ] );
            axis.y = float.Parse( values[ 1 ] );
            axis.z = float.Parse( values[ 2 ] );

            Vector3 along = new Vector3();
            along.x = float.Parse( values[ 3 ] );
            along.y = float.Parse( values[ 4 ] );
            along.z = float.Parse( values[ 5 ] );

            Matrix4 shear = Matrix4.Identity;
            shear[ 0, 1 ] = (float) Math.Tan( angle );
            Debug.Assert( axis.Dot( along ) < 0.001f,
                         "Vectors for skew must be perpendicular" );

            // FIXME: Handle these skews
            DebugMessage( node );
        }

        public void ReadInstanceGeometry( string nodeName, string parentBone, Matrix4 localTransform, Matrix4 transform, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string instance = UrlFragementName( node );

            if( nodeName == null )
            {
                nodeName = instance;
            }

            // It's possible that we failed to build geometries referenced by the 
            // a geometry instance (e.g. if the geometry only contains 'line' 
            // primitives), so only create the instance if the geometry is present.
            if( meshInfo.Geometries.ContainsKey( instance ) )
            {
                GeometryInstance geoInstance = CreateGeometryInstance( nodeName, parentBone, ref localTransform, ref transform, node, meshInfo );

                geoInstance.controller = null;
                geoInstance.geoSet = meshInfo.Geometries[ instance ];

                meshInfo.GeoInstances.Add( geoInstance );
            }
        }

        public void ReadInstanceController( string nodeName, string parentBone, Matrix4 localTransform, Matrix4 transform, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string instance = UrlFragementName( node );

            if( nodeName == null )
            {
                nodeName = instance;
            }

            if( !meshInfo.Controllers.ContainsKey( instance ) )
            {
                log.WarnFormat( "Cannot find controller instance named '{0}'", instance );
            }
            else
            {
                GeometryInstance geoInstance = CreateGeometryInstance( nodeName, parentBone, ref localTransform, ref transform, node, meshInfo );

                geoInstance.controller = meshInfo.Controllers[ instance ];
                geoInstance.geoSet = meshInfo.Controllers[ instance ].Target;

                meshInfo.GeoInstances.Add( geoInstance );
            }
        }

        private static GeometryInstance CreateGeometryInstance( string nodeName, string parentBone, ref Matrix4 localTransform, ref Matrix4 transform, XmlNode node, ColladaMeshInfo meshInfo )
        {
            GeometryInstance geoInstance = new GeometryInstance( nodeName, parentBone, localTransform, transform );

#if WORKS_AS_EXPECTED
            XmlNode bindNode = node.SelectSingleNode( "bind_material" );

            if( null != bindNode )
            {
                geoInstance.bindMaterial = new BindMaterial( bindNode, meshInfo.materialNamespace );
            }
#else
            foreach( XmlNode child in node.ChildNodes )
            {
                if( "bind_material" == child.Name )
                {
                    geoInstance.bindMaterial = new BindMaterial( child, MaterialScriptBuilder.MaterialNamespace );
                }
            }
#endif
            return geoInstance;
        }

        #region Helpers for parsing

        private static string UrlFragementName( XmlNode node )
        {
            XmlElement element = node as XmlElement;

            if( null != element )
            {
                if( String.Empty != element.GetAttribute( "url" ) )
                {
                    // The attribute is of the form "#fragmentName"; get it
                    // stripped of the leading "#"
                    return element.GetAttribute( "url" ).Substring( 1 );
                }
            }

            return String.Empty;
        }

        private bool IsInstanceGeometry( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string instanceName = UrlFragementName( node );

            if( String.Empty != instanceName )
            {
                return meshInfo.Geometries.ContainsKey( instanceName );
            }

            return false;
        }
        #endregion Helpers for parsing
        #endregion

    }
}
