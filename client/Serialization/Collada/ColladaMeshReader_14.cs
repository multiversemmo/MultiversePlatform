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
    class ColladaMeshReader_14 : ColladaMeshReader_13
    {
        public Dictionary<string, LibraryNode> LibraryNodes
        {
            get { return m_LibraryNodes; }
            set { m_LibraryNodes = value; }
        }
        private Dictionary<string, LibraryNode> m_LibraryNodes;

        public ColladaMeshReader_14()
        {
            m_LibraryNodes = new Dictionary<string, LibraryNode>();
        }

        public override void ReadCollada( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "asset":
                    ReadAsset( childNode, meshInfo );
                    break;
                case "library_geometries":
                case "library_controllers":
                case "library_visual_scenes":
                case "library_animations":
                case "library_materials":
                case "library_images":
                case "library_effects":
                case "library_nodes":
                case "scene":
                    // these will be handled later
                    break;
                case "library_animation_clips":
                case "library_cameras":
                case "library_force_fields":
                case "library_lights":
                case "library_physics_materials":
                case "library_physics_models":
                case "library_physics_scenes":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

            // These elements can occur in any order, but we
            // care about the order that we process them
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_geometries" )
                    ReadGeometryLibrary( childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_controllers" )
                    ReadControllerLibrary( childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_nodes" )
                    ReadNodeLibrary( childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_visual_scenes" )
                    ReadVisualScenes( childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_animations" )
                    ReadAnimation( null, childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_materials" )
                    ReadMaterialLibrary( childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_effects" )
                    ReadEffectLibrary( childNode, meshInfo );
            foreach( XmlNode childNode in node.ChildNodes )
                if( childNode.Name == "library_images" )
                    ReadImageLibrary( childNode, meshInfo );

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "scene":
                    ReadScene( childNode, meshInfo );
                    break;
                }
            }
        }


        public override void ReadAsset( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "contributor":
                    ReadContributor( childNode, meshInfo );
                    break;
                default:
                    base.ReadAsset( node, meshInfo );
                    break;
                }
            }
        }

        public void ReadContributor( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "authoring_tool":
                    ReadAuthoringTool( childNode, meshInfo );
                    break;
                default:
                    // Just ignore this.. Contributor data isn't very important
                    break;
                }
            }
        }

        public void ReadAuthoringTool( XmlNode node, ColladaMeshInfo meshInfo )
        {
            if( node.InnerText == "Feeling ColladaMax v1.06 with FCollada v1.14." )
            {
                meshInfo.SourceUnitsAreBroken = true;
                log.ErrorFormat( "INFO: This version of collada corrupts the units.  Using centimeters instead" );
            }
        }

        /// <summary>
        ///   Read in a data source, including the technique, accessors, 
        ///   joints, and combiners.  Sometimes this is in the context of 
        ///   a controller, in which case controller is not null.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="node"></param>
        public override void ReadSource( Controller controller, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string sourceId = node.Attributes[ "id" ].Value;
            Source source = null;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "IDREF_array":
                    {
                        string arrayId = childNode.Attributes[ "id" ].Value;
                        DataSource dataSource = new DataSource( arrayId );
                        ReadStringArray( dataSource, childNode, meshInfo );
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
                case "technique_common":
                    break; // Skip these in the first pass
                }
            }
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "IDREF_array":
                case "bool_array":
                case "float_array":
                case "int_array":
                case "Name_array":
                    break; // Skip these in the second pass
                case "technique":
                    ReadTechnique( controller, source, childNode, meshInfo );
                    break;
                case "technique_common":
                    ReadTechniqueCommon( controller, source, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            Sources[ sourceId ] = source;
        }

        public void ReadTechniqueCommon( Controller controller,
                                               Source source, XmlNode node,
                                               ColladaMeshInfo meshInfo )
        {
            ReadTechnique( controller, source, node, meshInfo );
        }

        public override void ReadMesh( string geometryName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            Dictionary<int, List<InputSourceCollection>> inputSources =
                new Dictionary<int, List<InputSourceCollection>>();
            Dictionary<string, VertexSet> vertexSets = new Dictionary<string, VertexSet>();
            int submeshIndex = 0;

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
                case "polygons":
                case "polylist":
                case "triangles":
                    {
                        string name = string.Format( "{0}.{1}", geometryName, submeshIndex++ );
                        if( !meshInfo.Geometries.ContainsKey( geometryName ) )
                            meshInfo.Geometries[ geometryName ] = new GeometrySet( geometryName );
                        GeometrySet geoSet = meshInfo.Geometries[ geometryName ];
                        MeshGeometry geometry = new MeshGeometry( name, geoSet );
                        foreach( VertexSet vertexSet in vertexSets.Values )
                            geometry.AddVertexSet( vertexSet );
                        geoSet.Add( geometry );
                        if( childNode.Name == "triangles" )
                            ReadTriangles( geometry, vertexSets, childNode, meshInfo );
                        else if( childNode.Name == "polygons" )
                            ReadPolygons( geometry, vertexSets, childNode, meshInfo );
                        else if( childNode.Name == "polylist" )
                            log.Error( "TODO: Add support for polylist" );
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

        public override void ReadAnimation( string parentAnimationName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string animationName = null;
            if( node.Attributes[ "id" ] != null )
                animationName = node.Attributes[ "id" ].Value;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "animation":
                    ReadAnimation( animationName, childNode, meshInfo );
                    break;
                case "source":
                    ReadSource( null, childNode, meshInfo );
                    break;
                case "sampler":
                    ReadSampler( childNode, meshInfo );
                    break;
                case "channel":
                    ReadChannel( parentAnimationName, animationName, childNode, meshInfo );
                    break;
                case "asset":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

        }

        public override void ReadChannel( string parentAnimationName, string animationName,
                                         XmlNode node, ColladaMeshInfo meshInfo )
        {
            string samplerId = node.Attributes[ "source" ].Value;
            string targetId = node.Attributes[ "target" ].Value;
            // Strip off the leading '#'
            samplerId = samplerId.Substring( 1 );
            Sampler sampler = meshInfo.Samplers[ samplerId ];
            Channel channel = new Channel( sampler, targetId, "TRANSFORM" );
            channel.AnimationName = animationName;
            channel.ParentAnimationName = parentAnimationName;
            meshInfo.Channels.Add( channel );
        }

        /// <summary>
        ///   ReadTriangles basically corresponds to reading in a SubMesh.
        ///   The grandparent of this node is probably the geometry node
        ///   with the submesh name.
        /// </summary>
        /// <param name="node">The node corresponding to the 'polygons' element</param>
        public void ReadTriangles( MeshGeometry geometry,
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
                    ReadTriangleVertices( geometry, doubleSided, childNode );
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

        public void ReadTriangleVertices( MeshGeometry geometry, bool doubleSided, XmlNode node )
        {
            string[] indices = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            int inputCount = geometry.InputCount;
            // How many vertices are in this list
            int vertexCount = indices.Length / inputCount;
            // Indices (into currentPoint) of the points in a polygon.
            // This is temporary, and is reset for each polygon
            int vertex = 0;
            while( vertex < vertexCount )
            {
                List<int> cwPolyPoints = geometry.BeginPolygon();
                List<int> ccwPolyPoints = geometry.BeginPolygon();
                for( int poly_vertex = 0; poly_vertex < 3; ++poly_vertex, ++vertex )
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
        }

        public void ReadVisualScenes( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "visual_scene":
                    base.ReadScene( childNode, meshInfo );
                    break;
                case "extra":
                    ReadExtra( childNode, meshInfo );
                    break;
                case "asset":
                    ReadAsset( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public override void ReadNode( List<NamedTransform> parentTransforms, string parentBone, XmlNode node, ColladaMeshInfo meshInfo )
        {
            // The order in the transformChain is the order that these 
            // transforms are applied, so later nodes actually go at the 
            // front of the list.
            List<NamedTransform> transformChain = BuildNodeTransformChain( node );

            string nodeId = null;
            if( node.Attributes[ "id" ] != null )
                nodeId = node.Attributes[ "id" ].Value;

            string nodeSid = null;
            if( node.Attributes[ "sid" ] != null )
                nodeSid = node.Attributes[ "sid" ].Value;

            // default node type
            string nodeType = "NODE";
            if( node.Attributes[ "type" ] != null )
                nodeType = node.Attributes[ "type" ].Value;
            if( node.Attributes[ "id" ] != null )
            {
                switch( nodeType )
                {
                case "JOINT":
                    {
                        meshInfo.JointTransformChains[ nodeId ] = transformChain;
                        meshInfo.BoneParents[ nodeId ] = parentBone;
                        if( nodeSid != null )
                            meshInfo.NodeSidMap[ nodeSid ] = nodeId;
                        parentBone = nodeId;
                    }
                    break;
                case "NODE":
                    // the default -- this might be a tag point object, so handle that
                    // Technically, our tagpoints could be children of another node instead of a 
                    // bone, but for now, just assume they are the child of a bone (JOINT).
                    if( nodeId != null && nodeId.StartsWith( "mvsock_", StringComparison.CurrentCultureIgnoreCase ) )
                    {
                        string attachPointName = nodeId.Substring( "mvsock_".Length );
                        // Honor the node name, if it is present
                        if( node.Attributes[ "name" ] != null )
                        {
                            string name = node.Attributes[ "name" ].Value;
                            if( name.StartsWith( "mvsock_", StringComparison.CurrentCultureIgnoreCase ) )
                                attachPointName = name.Substring( "mvsock_".Length );
                        }
                        Matrix4 localTransform = ComposeTransforms( transformChain );
                        Matrix4 transform = ComposeTransforms( parentTransforms ) * localTransform;
                        Matrix4 transformRelativeToParentBone;
                        if( parentBone != null )
                        {
                            Matrix4 parentTransform = meshInfo.GetBonePoseTransform( parentBone );
                            // log.DebugFormat("Parent Transform for {0}:\n{1}", nodeId, parentTransform);
                            transformRelativeToParentBone = parentTransform.Inverse() * transform;
                        }
                        else
                        {
                            transformRelativeToParentBone = transform;
                        }
                        // log.DebugFormat("Local Transform for {0}:\n{1}", nodeId, localTransform);
                        meshInfo.AttachmentPoints.Add( new AttachmentPointNode( attachPointName, parentBone, transformRelativeToParentBone ) );
                    }
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
                case "instance_controller":
                    {
                        Matrix4 localTransform = ComposeTransforms( transformChain );
                        Matrix4 transform = ComposeTransforms( parentTransforms ) * localTransform;
                        ReadInstanceController( nodeId, parentBone, localTransform, transform, childNode, meshInfo );
                    }
                    break;
                case "instance_geometry":
                    {
                        Matrix4 localTransform = ComposeTransforms( transformChain );
                        Matrix4 transform = ComposeTransforms( parentTransforms ) * localTransform;
                        ReadInstanceGeometry( nodeId, parentBone, localTransform, transform, childNode, meshInfo );
                    }
                    break;
                case "instance_node":
                    {
                        // do what ReadInstance does, expanding all references to geoInstances within the ref'd library node
                        Matrix4 localTransform = ComposeTransforms( transformChain );
                        Matrix4 transform = ComposeTransforms( parentTransforms ) * localTransform;
                        if( ! ReadInstanceNode( nodeId, localTransform, transform, childNode, meshInfo ) )
                        {
                            // The instance is not in a library, so find the reference and
                            // expand it here.
                            XmlNode unresolvedInstance = DiscoverInstanceReference( childNode );
                            if( null != unresolvedInstance )
                            {
                                ReadNode( parentTransforms, parentBone, unresolvedInstance, meshInfo );
                            }
                        }
                    }
                    break;
                case "node":
                    // this list of transforms is applied in the order of the list,
                    // so counter to intuition, the total is: Mn...M2xM1
                    List<NamedTransform> fullChain = new List<NamedTransform>();
                    fullChain.AddRange( transformChain );
                    fullChain.AddRange( parentTransforms );
                    ReadNode( fullChain, parentBone, childNode, meshInfo );
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

        private static Matrix4 ComposeTransforms( List<NamedTransform> transformChain )
        {
            Matrix4 composedTransform = ColladaMeshInfo.GetBakedTransform( transformChain );

            if( composedTransform == Matrix4.Zero )
            {
                throw new Exception( "Transform chain composes to a zero matrix." );
            }

            return composedTransform;
        }

        // The transforms on the node are represented as a chain of discrete
        // components. The order in the transformChain is the order that these 
        // transforms are applied, so later nodes actually go at the front of 
        // the list.        
        private List<NamedTransform> BuildNodeTransformChain( XmlNode node )
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
                        transformChain.Insert( 0, new NamedRotateTransform( GetSid( childNode ), angle, axis ) );
                    }
                    break;
                case "scale":
                    {
                        Vector3 scale = Vector3.UnitScale;
                        ReadVector( ref scale, childNode );
                        transformChain.Insert( 0, new NamedScaleTransform( GetSid( childNode ), scale ) );
                    }
                    break;
                case "translate":
                    {
                        Vector3 translation = Vector3.Zero;
                        ReadVector( ref translation, childNode );
                        transformChain.Insert( 0, new NamedTranslateTransform( GetSid( childNode ), translation ) );
                    }
                    break;
                case "skew":
                    {
                        // FIXME: For now, just handle skew with a matrix
                        Matrix4 matrix = Matrix4.Identity;
                        ReadSkewMatrix( ref matrix, childNode );
                        transformChain.Insert( 0, new NamedMatrixTransform( GetSid( childNode ), matrix ) );
                    }
                    break;
                case "matrix":
                    {
                        Matrix4 matrix = Matrix4.Identity;
                        ReadMatrix( ref matrix, childNode );
                        transformChain.Insert( 0, new NamedMatrixTransform( GetSid( childNode ), matrix ) );
                    }
                    break;
                case "instance_controller":
                case "instance_geometry":
                case "node":
                case "lookat":
                case "perspective":
                case "boundingbox":
                case "extras":
                default:
                    break;
                }
            }

            return transformChain;
        }

        // This succeeds if the instance_node references a node in a library,
        // but an instance_node is allowed to reference an arbitrary node.
        // If the referenced node is in a library, expand the reference into
        // the current location; else return false.
        public bool ReadInstanceNode( string referrerId, Matrix4 localTransform, Matrix4 transform, XmlNode node, ColladaMeshInfo meshInfo )
        {
            bool found = false;
            
            // expands an <instance_node> within a <node>, emitting all geoInstances 
            // from the ref'd library-node, with the referring node's transform in place
            string lnodeId = node.Attributes[ "url" ].Value.Substring( 1 );

            if( LibraryNodes.ContainsKey( lnodeId ) )
            {
                found = true;
                LibraryNode lnode = LibraryNodes[ lnodeId ];
                ExpandLibraryNode( lnode, referrerId, localTransform, transform, meshInfo );
            }
            return found;
        }

        // Assume instanceNode is in fact an instance_node; get the node it references.
        // returns null if the reference cannot be found.
        XmlNode DiscoverInstanceReference( XmlNode instanceNode )
        {

            XmlNamespaceManager nsm = new XmlNamespaceManager( m_Document.NameTable );

            nsm.AddNamespace( "mv", instanceNode.NamespaceURI );

            string instanceUrl = (instanceNode as XmlElement).GetAttribute( "url" ).Substring( 1 );
            
            string xpath = String.Format( "//mv:node[@id='{0}']", instanceUrl );

            XmlNodeList nodes = m_ColladaRootNode.SelectNodes( xpath, nsm );

            if( 0 < nodes.Count )
            {
                return nodes[ 0 ];
            }

            return null;
        }

        public void ExpandLibraryNode( LibraryNode lnode, string referrerId, Matrix4 localTransform, Matrix4 transform, ColladaMeshInfo meshInfo )
        {
            // recurse through any child nodes
            int childNo = 0;
            foreach( LibraryNode child in lnode.children )
                ExpandLibraryNode( child, referrerId + "." + (childNo++).ToString(), localTransform, transform, meshInfo );
            // expand geometry_instance references
            int instNo = 0;
            foreach( string geoInstanceId in lnode.geoInstanceIds )
            {
                GeometryInstance geoInstance = new GeometryInstance( referrerId + "." + (instNo++).ToString(), null, localTransform, transform );
                if( meshInfo.Geometries.ContainsKey( geoInstanceId ) )
                {
                    // this was an instance_geometry instead of an instance_controller in 1.4 terms
                    geoInstance.controller = null;
                    geoInstance.geoSet = meshInfo.Geometries[ geoInstanceId ].Clone( geoInstance.name );
                    meshInfo.GeoInstances.Add( geoInstance );
                }
            }
        }

        // In Collada 1.4, skins have targets (source attribute), but 
        // controllers do not
        public override void ReadController( XmlNode node, ColladaMeshInfo meshInfo )
        {
            // Set up the current geometry name
            string controllerName = node.Attributes[ "id" ].Value;
            // string target = node.Attributes["target"].Value;

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "skin":
                    SkinController skinController = new SkinController( controllerName );
                    // skinController.Target = meshInfo.geometries[target];
                    meshInfo.Controllers[ controllerName ] = skinController;
                    ReadSkin( skinController, childNode, meshInfo );
                    break;

                case "morph":
                    string method = "NORMALIZED";
                    if( childNode.Attributes[ "method" ] != null )
                        method = childNode.Attributes[ "method" ].Value;
                    MorphController morphController = new MorphController( controllerName, method );
                    // morphController.Target = meshInfo.geometries[target];
                    meshInfo.Controllers[ controllerName ] = morphController;
                    ReadMorph( morphController, childNode, meshInfo );
                    break;

                default:
                    DebugMessage( childNode );
                    break;
                }
            }

        }

        public override void ReadSkin( SkinController controller, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string skinSource = node.Attributes[ "source" ].Value;
            if( skinSource.StartsWith( "#" ) )
                // strip off the leading '#'
                skinSource = skinSource.Substring( 1 );
            else
                log.InfoFormat( "Skin source {0} does not start with '#'", skinSource );
            
            if( controller.Target != null )
            {
                string msg = string.Format( "Duplicate skin for controller: {0}", controller.Name );
                throw new Exception( msg );
            }

            // The skin source may actually be a morph controller.
            if( meshInfo.Controllers.ContainsKey( skinSource ) )
            {
                controller.Target = meshInfo.Controllers[ skinSource ].Target;
                controller.Morph = meshInfo.Controllers[ skinSource ] as MorphController;
            }
            else if( meshInfo.Geometries.ContainsKey( skinSource ) )
            {
                controller.Target = meshInfo.Geometries[ skinSource ];
            }


            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                    ReadSource( controller, childNode, meshInfo );
                    break;
                case "joints":
                    ReadJoints( controller, childNode, meshInfo );
                    break;
                case "vertex_weights":
                    controller.Target.Combiner =
                        ReadVertexWeights( childNode, meshInfo );
                    break;
                case "bind_shape_matrix":
                    {
                        Matrix4 bindShapeMatrix = Matrix4.Identity;
                        ReadMatrix( ref bindShapeMatrix, childNode );
                        controller.BindShapeMatrix = bindShapeMatrix;
                    }
                    break;
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadMorph( MorphController controller, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string morphSource = node.Attributes[ "source" ].Value;
            if( morphSource.StartsWith( "#" ) )
                // strip off the leading '#'
                morphSource = morphSource.Substring( 1 );
            else
                log.InfoFormat( "Morph source {0} does not start with '#'", morphSource );
            if( controller.Target != null )
            {
                string msg = string.Format( "Duplicate morph for controller {0}", controller.Name );
                throw new Exception( msg );
            }
            controller.Target = meshInfo.Geometries[ morphSource ];
#if SET_BASE_GEOMETRY
            controller.Target.BaseGeometrySet = meshInfo.geometries[ morphSource ];
#endif
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                    ReadSource( controller, childNode, meshInfo );
                    break;
                case "targets":
                    ReadTargets( controller, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadTargets( MorphController morphController, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadInput( morphController.InputSources, null, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public override void ReadScene( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "extra":
                    ReadExtra( childNode, meshInfo );
                    break;
                case "instance_visual_scene":
                //ReadNode(new List<NamedTransform>(), null, childNode, meshInfo);
                //break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public List<CombinerComponents> ReadVertexWeights( XmlNode node, ColladaMeshInfo meshInfo )
        {
            Dictionary<int, List<InputSourceCollection>> inputSources =
                new Dictionary<int, List<InputSourceCollection>>();
            List<CombinerComponents> combComps = null;
            List<int> vcounts = null;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "input":
                    ReadInput( inputSources, null, childNode, meshInfo );
                    break;
                case "vcount":
                    vcounts = ReadVCount( childNode );
                    // Ignore in this pass
                    break;
                case "v":
                    combComps = ReadV( inputSources, vcounts, childNode );
                    break;
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            return combComps;
        }

        public List<int> ReadVCount( XmlNode node )
        {
            List<int> vcount_list = new List<int>();
            string[] vcount_array = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            foreach( string str in vcount_array )
            {
                vcount_list.Add( int.Parse( str ) );
            }
            return vcount_list;
        }

        public List<CombinerComponents> ReadV( Dictionary<int, List<InputSourceCollection>> inputSources,
                                              List<int> vertexCounts,
                                              XmlNode node )
        {
            List<CombinerComponents> combComps = new List<CombinerComponents>();
            string[] indices = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            int offset = 0;
            for( int vertexIndex = 0; vertexIndex < vertexCounts.Count; ++vertexIndex )
            {
                CombinerComponents cc = new CombinerComponents( vertexIndex, inputSources );
                combComps.Add( cc );
                for( int influence = 0; influence < vertexCounts[ vertexIndex ]; ++influence )
                {
                    CombinerEntry entry = new CombinerEntry();
                    cc.Add( entry );
                    // Build the entry to be the right size
                    while( entry.Count < inputSources.Count )
                        entry.Add( -1 );
                    foreach( int inputIndex in inputSources.Keys )
                        entry[ inputIndex ] = int.Parse( indices[ offset + influence * inputSources.Count + inputIndex ] );
                }
                offset += vertexCounts[ vertexIndex ] * inputSources.Count;
            }
            return combComps;
        }

        public override void ReadMaterial( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            string name = node.Attributes[ "name" ] != null ? node.Attributes[ "name" ].Value : null;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "instance_effect":
                    ReadInstanceEffect( id, name, childNode, meshInfo );
                    break;
                case "asset":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadInstanceEffect( string materialId, string materialName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string url = node.Attributes[ "url" ].Value;

            MaterialBuilder.AddEffect( materialId, url.Substring( 1 ) );

            // for now, create effect references with both mtlId and mtlName 
            // (if they differ), as the <triangles> material attribute may use 
            // either. At some point, we should handle <bind_material> elements 
            // in <library_visual_scenes> which use IDs deterministically.
            // TODO: We do deal with bind_material elements now, so maybe this
            // is moot.  I don't know yet how to test this, so I'll leave it 
            // in for now.
            if( null != materialName && materialId.Equals( materialName ) )
            {
                MaterialBuilder.AddEffect( materialName, url.Substring( 1 ) );
            }

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "technique_hint":
                case "setparam":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public override void ReadInput( Dictionary<int, List<InputSourceCollection>> inputSources,
                                       Dictionary<string, VertexSet> vertexSets,
                                       XmlNode node, ColladaMeshInfo meshInfo )
        {
            InputSourceCollection input = null;
            string source = node.Attributes[ "source" ].Value;
            // Get the part after the '#'
            source = source.Substring( 1 );
            string semantic = node.Attributes[ "semantic" ].Value;
            int inputIndex = inputSources.Count;
            if( node.Attributes[ "offset" ] != null )
                inputIndex = int.Parse( node.Attributes[ "offset" ].Value );
            int inputSet = -1;
            if( node.Attributes[ "set" ] != null )
                inputSet = int.Parse( node.Attributes[ "set" ].Value );

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
                    string msg = string.Format( "Missing accessor for source: {0}", source );
                    throw new Exception( msg );
                }
                Accessor accessor = Accessors[ source ];
                if( inputSources != null )
                {
                    input = new InputSource( source, semantic, accessor, inputSet );
                    inputSources[ inputIndex ].Add( input );
                }
            }
        }

        public override void ReadImageLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "image":
                    ReadImage( childNode, meshInfo );
                    break;
                case "asset":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public override void ReadImage( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string id = node.Attributes[ "id" ].Value;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "init_from":
                    MaterialBuilder.AddImage( id, childNode.InnerText );
                    break;
                case "asset":
                case "data":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadNodeLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "node":
                    ReadLibraryNode( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public LibraryNode ReadLibraryNode( XmlNode node, ColladaMeshInfo meshInfo )
        {
            // Add a lib-node entry into meshInfo's node-library, expanded later by instance_node references in visual_scene nodes
            string lnodeId = node.Attributes[ "id" ].Value;
            LibraryNode lnode = new LibraryNode();
            LibraryNodes.Add( lnodeId, lnode );

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "node":
                    lnode.children.Add( ReadLibraryNode( childNode, meshInfo ) );
                    break;
                case "instance_geometry":
                    string gId = childNode.Attributes[ "url" ].Value.Substring( 1 );
                    lnode.geoInstanceIds.Add( gId );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            return lnode;
        }

        public void ReadEffectLibrary( XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "effect":
                    ReadEffect( childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadEffect( XmlNode node, ColladaMeshInfo meshInfo )
        {
            // Add an entry to the meshInfo's effect information
            string effectId = node.Attributes[ "id" ].Value;
            string effectName = effectId;
            if( node.Attributes[ "name" ] != null )
                effectName = node.Attributes[ "name" ].Value;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "profile_COMMON":
                    ReadEffectCommon( effectId, effectName, childNode, meshInfo );
                    break;
                case "asset":
                case "annotate":
                case "image":
                case "newparam":
                case "profile_CG":
                case "profile_GLSL":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

        }

        public void ReadEffectCommon( string effectId, string effectName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "technique":
                    ReadTechniqueCommon( effectId, effectName, childNode, meshInfo );
                    break;
                case "newparam":
                    ReadNewParam( childNode, meshInfo );
                    break;
                case "image":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        private void ReadNewParam( XmlNode node, ColladaMeshInfo meshInfo )
        {
            string param_sid = node.Attributes[ "sid" ].Value;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "sampler2D":
                    ReadSampler2D( param_sid, childNode, meshInfo );
                    break;
                case "surface":
                    ReadSurface( param_sid, childNode, meshInfo );
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        private void ReadSurface( string param_sid, XmlNode node, ColladaMeshInfo meshInfo )
        {
            Surface surface = new Surface();
            string surface_type = node.Attributes[ "type" ].Value;
            switch( surface_type )
            {
            case "2D":
                surface.texType = TextureType.TwoD;
                break;
            default:
                break;
            }

            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "init_from":
                    surface.image = childNode.InnerText;
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

            MaterialBuilder.AddSurface( param_sid, surface );
        }

        private void ReadSampler2D( string param_sid, XmlNode node, ColladaMeshInfo meshInfo )
        {
            SurfaceSampler sampler = new SurfaceSampler();
            sampler.samplerName = param_sid;
            sampler.texType = TextureType.TwoD;
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "source":
                    sampler.surfaceName = childNode.InnerText;
                    break;
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

            MaterialBuilder.AddSurfaceSampler( param_sid, sampler );
        }

        public void ReadTechniqueCommon( string effectId, string effectName, XmlNode node, ColladaMeshInfo meshInfo )
        {
            string techniqueId = string.Empty;
            if( node.Attributes[ "id" ] != null )
                techniqueId = node.Attributes[ "id" ].Value;
            MaterialTechnique_14 technique = new MaterialTechnique_14( techniqueId );
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "constant":
                    ReadConstant( technique, childNode, meshInfo );
                    break;
                case "lambert":
                case "phong":
                case "blinn":
                    // looks like the modeling tools output phong to collada even if the
                    // shader is specified as blinn in the tool
                    // Note: phong uses the reflection vector instead of the half angle vector
                    ReadPhong( technique, childNode, meshInfo );
                    break;
                case "asset":
                case "image":
                case "extra":
                default:
                    DebugMessage( childNode );
                    break;
                }
            }

            MaterialBuilder.AddTechnique( effectId, technique );
        }

        public void ReadConstant( MaterialTechnique_14 technique, XmlNode node, ColladaMeshInfo meshInfo )
        {
            // should really be lit based solely on emissive, 
            // with ambient, diffuse and specular set to 0
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "emission":
                    technique.Emissive = ReadColorProperty( childNode );
                    break;
                case "reflective":
                case "reflectivity":
                case "transparent":
                case "transparency":
                case "index_of_refraction":
                // ignored
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public void ReadPhong( MaterialTechnique_14 technique, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "diffuse":
                    ReadDiffuse( technique, childNode, meshInfo );
                    break;
                case "emission":
                    ReadEmission( technique, childNode, meshInfo );
                    break;
                case "ambient":
                    ReadAmbient( technique, childNode, meshInfo );
                    break;
                case "specular":
                    ReadSpecular( technique, childNode, meshInfo );
                    break;
                case "shininess":
                case "reflective":
                case "reflectivity":
                case "transparent":
                case "transparency":
                case "index_of_refraction":
                // ignored
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
        }

        public ColorEx ReadColorProperty( XmlNode node )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "color":
                    return ReadColor( node );
                default:
                    DebugMessage( childNode );
                    break;
                }
            }
            // no color data
            return ColorEx.White;
        }

        public ColorEx ReadColor( XmlNode node )
        {
            string[] components = node.InnerText.Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
            if( components.Length < 4 )
            {
                log.WarnFormat( "Invalid color specification: '{0}'", node.InnerText );
                return ColorEx.White;
            }
            float r, g, b, a;
            r = float.Parse( components[ 0 ] );
            g = float.Parse( components[ 1 ] );
            b = float.Parse( components[ 2 ] );
            a = float.Parse( components[ 3 ] );
            return new ColorEx( a, r, g, b );
        }

        public void ReadDiffuse( MaterialTechnique_14 technique, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "color":
                    technique.Diffuse = ReadColor( node );
                    break;
                case "texture":
                    technique.DiffuseTexture = ReadTexture( childNode );
                    break;
                default:
                    DebugMessage( node );
                    break;
                }
            }
        }
        public void ReadAmbient( MaterialTechnique_14 technique, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "color":
                    technique.Ambient = ReadColor( node );
                    break;
                case "texture":
                    log.Warn( "Ambient texture is unsupported" );
                    technique.AmbientTexture = ReadTexture( childNode );
                    break;
                default:
                    DebugMessage( node );
                    break;
                }
            }
        }
        public void ReadEmission( MaterialTechnique_14 technique, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "color":
                    technique.Emissive = ReadColor( node );
                    break;
                case "texture":
                    log.Warn( "Emissive texture is unsupported" );
                    technique.EmissiveTexture = ReadTexture( childNode );
                    break;
                default:
                    DebugMessage( node );
                    break;
                }
            }
        }
        public void ReadSpecular( MaterialTechnique_14 technique, XmlNode node, ColladaMeshInfo meshInfo )
        {
            foreach( XmlNode childNode in node.ChildNodes )
            {
                switch( childNode.Name )
                {
                case "color":
                    technique.Specular = ReadColor( node );
                    break;
                case "texture":
                    log.Warn( "Specular texture is unsupported" );
                    technique.SpecularTexture = ReadTexture( childNode );
                    break;
                default:
                    DebugMessage( node );
                    break;
                }
            }
        }

        public MaterialTechnique_14.TextureInfo ReadTexture( XmlNode node )
        {
            if( node.Attributes[ "texture" ] == null )
            {
                log.Warn( "Invalid texture without texture attribute" );
                return null;
            }
            MaterialTechnique_14.TextureInfo textureInfo = new MaterialTechnique_14.TextureInfo();
            textureInfo.imageId = node.Attributes[ "texture" ].Value;
            textureInfo.channelId = node.Attributes[ "texcoord" ].Value;
            // NOTE: texcoord attribute is ignored here
            return textureInfo;
        }
    }
}
