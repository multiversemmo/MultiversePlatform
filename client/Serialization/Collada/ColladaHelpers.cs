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
using System.Diagnostics;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

///
/// This file is an intermediate step in refactoring the Collada
/// serialiazation code.  It contains a hodge-podge of classes 
/// (mostly functioning like structures with no behavior) used 
/// for converting Collada files. As the refactor continues, I
/// expect this file to get pared away bit by bit, and ultimately
/// go away.
///
namespace Multiverse.Serialization.Collada
{
    public class Triple<T, U, V>
    {
        public T first;
        public U second;
        public V third;

        public Triple( T first, U second, V third )
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
    }

    public class KeyFrameInfo
    {
        public float time;
        public TransformKeyFrame keyFrame;
        public Dictionary<Channel, Triple<int, int, float>> channelIndices;
    }

    public class TextureTechnique
    {
        public string imageId;
    }
    public class MaterialTechnique_13
    {
        public List<TechniquePass> passes;
    }
    public class TechniquePass
    {
        public string textureId;
        public string shader = "gouraud";
    }

    public abstract class AccessorParam
    {
        protected string name;
        protected int offset = 0;
        protected int stride = 1;

        public AccessorParam( string name, int offset, int stride )
        {
            this.name = name;
            this.offset = offset;
            this.stride = stride;
        }

        public abstract object GetEntry( DataSource source, int index );

        public int Stride
        {
            get { return stride; }
            set { stride = value; }
        }

        public abstract int DefaultSize
        {
            get;
        }
    }

    // Class to encapsulate an accessor param of type float
    public class ParamFloat : AccessorParam
    {
        public ParamFloat( string name, int offset, int stride )
            : base( name, offset, stride )
        {
        }

        public override object GetEntry( DataSource source, int index )
        {
            List<float> sourceData = source.FloatData;
            return sourceData[ stride * index + offset ];
        }

        public override int DefaultSize
        {
            get { return 1; }
        }
    }


    // Class to encapsulate an accessor param of type double
    public class ParamDouble : AccessorParam
    {
        public ParamDouble( string name, int offset, int stride )
            : base( name, offset, stride )
        {
        }

        public override object GetEntry( DataSource source, int index )
        {
            List<float> sourceData = source.FloatData;
            return sourceData[ stride * index + offset ];
        }

        public override int DefaultSize
        {
            get { return 1; }
        }
    }

    // Class to encapsulate an accessor param of type float
    public class ParamMatrix4 : AccessorParam
    {
        public ParamMatrix4( string name, int offset, int stride )
            : base( name, offset, stride )
        {
        }

        public override object GetEntry( DataSource source, int index )
        {
            List<float> sourceData = source.FloatData;
            Matrix4 rv = new Matrix4();
            for( int i = 0; i < 4; ++i )
                for( int j = 0; j < 4; ++j )
                    rv[ 4 * i + j ] = sourceData[ 4 * i + j + stride * index + offset ];
            return rv;
        }

        public override int DefaultSize
        {
            get { return 16; }
        }
    }

    // Class to encapsulate an accessor param of type float3
    public class ParamVector3 : AccessorParam
    {
        public ParamVector3( string name, int offset, int stride )
            : base( name, offset, stride )
        {
        }

        public override object GetEntry( DataSource source, int index )
        {
            List<float> sourceData = source.FloatData;
            Vector3 rv = new Vector3();
            for( int i = 0; i < 3; ++i )
                rv[ i ] = sourceData[ i + stride * index + offset ];
            return rv;
        }

        public override int DefaultSize
        {
            get { return 3; }
        }
    }

    // Class to encapsulate an accessor param of type string
    public class ParamString : AccessorParam
    {
        public ParamString( string name, int offset, int stride )
            : base( name, offset, stride )
        {
        }

        public override object GetEntry( DataSource source, int index )
        {
            List<string> sourceData = source.StringData;
            return sourceData[ stride * index + offset ];
        }

        public override int DefaultSize
        {
            get { return 1; }
        }
    }

    /// <summary>
    ///   This class maps to the 'accessor' xml node
    /// </summary>
    public class Accessor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof( Accessor ) );

        // this is the id of the accessor (or the containing source)
        protected string accessorId;

        protected DataSource source;
        //protected int count;

        protected int offset = 0;
        protected int stride = 1;

        protected Dictionary<string, AccessorParam> paramMap;

        public Accessor( DataSource source, string accessorId )
        {
            this.accessorId = accessorId;
            this.source = source;
            paramMap = new Dictionary<string, AccessorParam>();
        }

        protected Accessor( Accessor other )
        {
            this.accessorId = other.accessorId;
            this.source = other.source;
            //this.count = other.count;
            this.offset = other.offset;
            this.stride = other.stride;
            paramMap = new Dictionary<string, AccessorParam>( other.paramMap );
        }

        public void AddParam( string name, string type )
        {
            switch( type )
            {
            case "float":
                paramMap[ name ] = new ParamFloat( name, offset, stride );
                offset += 1;
                break;
            case "double":
                paramMap[ name ] = new ParamDouble( name, offset, stride );
                offset += 1;
                break;
            case "float4x4":
                paramMap[ name ] = new ParamMatrix4( name, offset, stride );
                offset += 16;
                break;
            case "name":
            case "Name":
            case "IDREF":
                paramMap[ name ] = new ParamString( name, offset, stride );
                offset += 1;
                break;
            case "float3":
                paramMap[ name ] = new ParamVector3( name, offset, stride );
                offset += 3;
                break;
            default:
                log.InfoFormat( "Unhandled accessor param type: {0}", type );
                break;
            }
        }

        public virtual object GetParam( string name, int index )
        {
            AccessorParam param = paramMap[ name ];
            return param.GetEntry( source, index );
        }

        public AccessorParam GetAccessorParam( string name )
        {
            return paramMap[ name ];
        }

        public bool ContainsParam( string name )
        {
            return paramMap.ContainsKey( name );
        }

        public string AccessorId
        {
            get { return accessorId; }
        }
        /// <summary>
        ///   The number of entries associated with this source
        /// </summary>
        public int Count
        {
            get { return source.Count / stride; }
        }
        /// <summary>
        ///   The number of data values associated with an entry
        /// </summary>
        public int Stride
        {
            get { return stride; }
            set
            {
                stride = value;
                foreach( string name in paramMap.Keys )
                {
                    paramMap[ name ].Stride = stride;
                }
            }
        }
        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }
    }

    public class NegatingFloatAccessor : Accessor
    {
        public NegatingFloatAccessor( Accessor other )
            : base( other )
        {
        }

        public override object GetParam( string name, int index )
        {
            AccessorParam param = paramMap[ name ];
            bool negate = false;
            if( index < 0 )
            {
                negate = true;
                index = -1 * index;
            }
            float f = (float) param.GetEntry( source, index );
            return negate ? -f : f;
        }
    }

    /// <summary>
    ///   This class maps essentially maps to the 'source' xml node
    /// </summary>
    public class Source
    {
        protected string sourceId;
        protected DataSource dataSource;
        protected List<Accessor> accessors;

        public Source( string sourceId, DataSource dataSource )
        {
            this.sourceId = sourceId;
            this.dataSource = dataSource;
            accessors = new List<Accessor>();
        }

        public void AddAccessor( Accessor accessor )
        {
            accessors.Add( accessor );
        }

        public string SourceId
        {
            get { return sourceId; }
        }

        public DataSource DataSource
        {
            get { return dataSource; }
        }

        public List<Accessor> Accessors
        {
            get { return accessors; }
        }


    }

    /// <summary>
    ///   This class maps essentially maps to the 'array' xml node
    /// </summary>
    public class DataSource
    {
        protected string arrayId;
        protected List<bool> boolData;
        protected List<float> floatData;
        protected List<int> intData;
        protected List<string> stringData;

        public DataSource( string arrayId )
        {
            this.arrayId = arrayId;
            boolData = new List<bool>();
            floatData = new List<float>();
            intData = new List<int>();
            stringData = new List<string>();
        }

        public List<bool> BoolData
        {
            get { return boolData; }
        }

        public List<float> FloatData
        {
            get { return floatData; }
        }

        public List<int> IntData
        {
            get { return intData; }
        }

        public List<string> StringData
        {
            get { return stringData; }
        }

        public int Count
        {
            get
            {
                if( boolData.Count > 0 )
                    return boolData.Count;
                else if( floatData.Count > 0 )
                    return floatData.Count;
                else if( intData.Count > 0 )
                    return intData.Count;
                else if( stringData.Count > 0 )
                    return stringData.Count;
                return 0;
            }
        }

    }


    /// <summary>
    ///   This class is intended to model an input source, which may be a simple
    ///   one like texture coordinates, or a more complex one like a vertex which
    ///   contains positions and normal sources.
    /// </summary>
    public abstract class InputSourceCollection
    {
        public abstract List<InputSource> GetSources();
    }

    /// <summary>
    ///   This class is used for the input entries to hold the semantic 
    ///   and accessor information.  This is close to the same thing as
    ///   the 'input' xml node.
    /// </summary>
    public class InputSource : InputSourceCollection
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof( InputSource ) );

        /// <summary>
        ///    id of the source -- may be an accessor within a technique, 
        ///    or the source id itself.
        /// </summary>
        string source;
        // Semantic of the source
        string semantic;
        // input set (to group inputs -- -1 means it has no set specified)
        int inputSet;
        // Accessor that is associated with the input
        protected Accessor accessor;

        bool isPosition;
        bool isNormal;

        public InputSource( string source, string semantic, Accessor accessor )
            : this( source, semantic, accessor, -1 )
        {
        }
        public InputSource( string source, string semantic, Accessor accessor, int inputSet )
        {
            this.accessor = accessor;
            this.source = source;
            this.semantic = semantic;
            this.inputSet = inputSet;
            switch( semantic )
            {
            case "POSITION":
            case "BIND_SHAPE_POSITION":
                isPosition = true;
                isNormal = false;
                break;
            case "VERTEX":
                Debug.Assert( false, "Should not get here" );
                isPosition = false;
                isNormal = false;
                break;
            case "NORMAL":
            case "BIND_SHAPE_NORMAL":
                isPosition = false;
                isNormal = true;
                this.accessor = new NegatingFloatAccessor( accessor );
                break;
            case "COLOR":
            case "UV":
            case "TEXCOORD":
            case "TANGENT":
            case "TEXTANGENT":
            case "BINORMAL":
            case "TEXBINORMAL":
            case "JOINT":
            case "WEIGHT":
            case "INPUT":
            case "OUTPUT":
            case "INV_BIND_MATRIX":
                isPosition = false;
                isNormal = false;
                break;
            default:
                log.InfoFormat( "Unhandled input semantic: {0}", semantic );
                break;
            }
        }

        public override List<InputSource> GetSources()
        {
            List<InputSource> rv = new List<InputSource>();
            rv.Add( this );
            return rv;
        }

        public string Source
        {
            get { return source; }
        }

        public string Semantic
        {
            get { return semantic; }
        }

        public bool IsPosition
        {
            get { return isPosition; }
        }

        public bool IsNormal
        {
            get { return isNormal; }
        }

        public int Set
        {
            get { return inputSet; }
        }

        public int Count
        {
            get { return accessor.Count; }
        }

        public Accessor Accessor
        {
            get { return accessor; }
        }

    }

    public class VertexInputSource : InputSourceCollection
    {
        List<InputSourceCollection> sources = new List<InputSourceCollection>();
        public VertexInputSource()
        {
        }

        public void AddSource( InputSourceCollection source )
        {
            sources.Add( source );
        }

        public override List<InputSource> GetSources()
        {
            List<InputSource> rv = new List<InputSource>();
            foreach( InputSourceCollection source in sources )
                rv.AddRange( source.GetSources() );
            return rv;
        }
    }

    /// <summary>
    ///   This is a collection of sources and semantics for vertices.	
    /// </summary>
    public class VertexSet
    {
        public string id;
        public List<InputSourceCollection> vertexEntries = new List<InputSourceCollection>();
    }

    /// <summary>
    ///   This is a list of input indices with one entry for each input
    ///   source.  For example, this might be the list containing a joint 
    ///   index and a weight index.
    /// </summary>
    public class CombinerEntry : List<int>
    {
    }

    /// <summary>
    ///   This is a list of combiner entries.  Bone weights might be 
    ///   modeled using a combiner entry for each weighted bone 
    ///   assignment of a vertex.
    /// </summary>
    public class CombinerComponents : List<CombinerEntry>
    {
        // Index in the position array (used by the bone assignment table)
        int vertexIndex = -1;
        // Mapping of input index to the set of input sources.  These input 
        // sources may be composite inputs (such as vertex)
        Dictionary<int, List<InputSourceCollection>> inputSources;

        public CombinerComponents( int vertexIndex, Dictionary<int, List<InputSourceCollection>> inputSources )
        {
            this.vertexIndex = vertexIndex;
            this.inputSources = inputSources;
        }

        public int VertexIndex
        {
            get { return vertexIndex; }
        }

        public Dictionary<int, List<InputSourceCollection>> InputSources
        {
            get { return inputSources; }
        }
    }

    /// <summary>
    ///   This class has the indexes into the various inputs for each vertex.
    ///   The order of the entries in the list matches the order of the inputs 
    ///   in the list of inputs.
    /// </summary>
    public class PointComponents : List<int>
    {
        // Index into the parent geometry's vertices array (used by the bone assignment table)
        int vertexIndex = -1;

        public PointComponents()
        {
        }

        public PointComponents( PointComponents other )
            : base( other )
        {
            this.vertexIndex = other.vertexIndex;
            //foreach (int key in other.boneAssignments.Keys) {
            //    boneAssignments[key] = other.boneAssignments[key];
            //}
        }

        public override bool Equals( object obj )
        {
            if( obj == null || !(obj is PointComponents) )
                return false;
            PointComponents other = (PointComponents) obj;
            if( this.Count != other.Count )
                return false;
            for( int i = 0; i < this.Count; ++i )
                if( this[ i ] != other[ i ] )
                    return false;
            if( vertexIndex != other.vertexIndex )
                return false;
            //if (boneAssignments.Count != other.boneAssignments.Count)
            //    return false;
            //foreach (int key in boneAssignments.Keys) {
            //    if (!other.boneAssignments.ContainsKey(key))
            //        return false;
            //    if (boneAssignments[key] != other.boneAssignments[key])
            //        return false;
            //}
            return true;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public int VertexIndex
        {
            get { return vertexIndex; }
            set { vertexIndex = value; }
        }
    }

    public class SubMeshInfo
    {
        public string name;
        public string material;
    }

    public class VertexDataEntry
    {
        public VertexElementSemantic semantic;
        public int textureIndex;
        public float[ , ] fdata;
        public uint[ , ] idata;

        public VertexDataEntry Clone()
        {
            VertexDataEntry clone = new VertexDataEntry();
            clone.semantic = this.semantic;
            clone.textureIndex = this.textureIndex;
            if( fdata != null )
                clone.fdata = (float[ , ]) this.fdata.Clone();
            if( idata != null )
                clone.idata = (uint[ , ]) this.idata.Clone();
            return clone;
        }

        public VertexElementType GetVertexElementType()
        {
            VertexElementType type;
            switch( semantic )
            {
            case VertexElementSemantic.Diffuse:
            case VertexElementSemantic.Specular:
                type = VertexElementType.Color;
                break;
            case VertexElementSemantic.TexCoords:
                type = VertexElementType.Float2;
                break;
            case VertexElementSemantic.Position:
            case VertexElementSemantic.Normal:
            case VertexElementSemantic.Tangent:
            case VertexElementSemantic.Binormal:
                type = VertexElementType.Float3;
                break;
            default:
                throw new Exception( "Invalid type" );
            }
            return type;
        }
    }
    
    /// <summary>
    /// Logically speaking, this is a two-dimensional jagged array of PoseInfo.
    /// The first dimension is the base geometry name, and the second is the
    /// target geometry name.  Thus, you can find all the targets for any base 
    /// geometry.
    /// </summary>
    public class PoseInfoCatalog
    {
        Dictionary< string, Dictionary< string, PoseInfo> >  m_BaseMeshToTargets =
            new Dictionary<string,Dictionary<string,PoseInfo>>();

        public PoseInfoCatalog()
        {
        }

        public void Add( GeometrySet baseGeometry, GeometrySet targetGeometry )
        {
            if( m_BaseMeshToTargets.ContainsKey( baseGeometry.Name ) )
            {
                m_BaseMeshToTargets[ baseGeometry.Name ][ targetGeometry.Name ] =
                    new PoseInfo( baseGeometry, targetGeometry );
            }
            else
            {
                Dictionary< string, PoseInfo > targetInfo = new Dictionary<string,PoseInfo>();

                targetInfo.Add( targetGeometry.Name, new PoseInfo( baseGeometry, targetGeometry ) );

                m_BaseMeshToTargets.Add( baseGeometry.Name, targetInfo );
            }
        }

        public IEnumerable<PoseInfo> AllInfos()
        {
            foreach( Dictionary<string, PoseInfo> infoList in m_BaseMeshToTargets.Values )
            {
                foreach( PoseInfo info in infoList.Values )
                {
                    yield return info;
                }
            }
        }

        public IEnumerable<PoseInfo> TargetInfos( string baseGeometryName )
        {
            if( m_BaseMeshToTargets.ContainsKey( baseGeometryName ) )
            {
                foreach( PoseInfo info in m_BaseMeshToTargets[ baseGeometryName ].Values )
                {
                    yield return info;
                }
            }
        }

        // Return the PoseInfo for the given base and given target; returns null
        // if no matching info is found.
        public PoseInfo FindInfo( GeometrySet baseGeometry, GeometrySet targetGeometry )
        {
            if( m_BaseMeshToTargets.ContainsKey( baseGeometry.Name ) )
            {
                if( m_BaseMeshToTargets[ baseGeometry.Name ].ContainsKey( targetGeometry.Name ) )
                {
                    return m_BaseMeshToTargets[ baseGeometry.Name ][ targetGeometry.Name ];
                }
            }

            return null;
        }
    }

    public class GeometryInstance
    {
        public string name;
        public string parentBone;
        public Matrix4 localTransform;
        public Matrix4 transform;
        public GeometrySet geoSet;
        public Controller controller;
        public BindMaterial bindMaterial;

        // TODO: This is a hack that will get resolved when I can rationalize
        // the treatment of transforms throughout the system.
        // The purpose of this is to apply an arbitrary transform that comes from
        // the ConversionTool command-line.  Some day this will get applied at a
        // much higher level, and then this property shall trouble us no more.
        public Matrix4 GlobalTransform
        {
            get { return m_GlobalTransform; }
            set { m_GlobalTransform = value; }
        }
        Matrix4 m_GlobalTransform = new Matrix4();

        public GeometryInstance( string name, string parentName, Matrix4 localTransform, Matrix4 transform )
        {
            this.name = name;
            this.parentBone = parentName;
            this.localTransform = localTransform;
            this.transform = transform;
        }

        public Matrix4 BindShapeMatrix
        {
            get
            {
                if( controller != null )
                    return controller.BindShapeMatrix;
                return Matrix4.Identity;
            }
        }

        public void ApplyMaterialBindings()
        {
            if( null != bindMaterial )
            {
                foreach( MeshGeometry mesh in geoSet )
                {
                    foreach( SubMeshInfo subMesh in mesh.SubMeshes )
                    {
                        if( bindMaterial.Bindings.ContainsKey( subMesh.material ) )
                        {
                            subMesh.material = bindMaterial.Bindings[ subMesh.material ];
                        }
                    }
                }
            }
        }

        public void RemoveEmptyGeometrySets()
        {
            List<int> indices = new List<int>();

            // Generate indices in reverse order so we can remove items from
            // the geoSet starting at the end-most and not affect earlier indices.
            for( int index = geoSet.Count - 1; index >= 0; index-- )
            {
                if( 0 == geoSet[ index ].Faces.Count )
                {
                    indices.Add( index );
                }
            }

            foreach( int index in indices )
            {
                geoSet.RemoveAt( index );
            }
        }

        // TODO: The reason we need the skinController here has something to do
        // with how morph targets are associated with skinning.  I don't fully
        // understand it yet, but this should be handled differently.
        public void SkinGeometry( Mesh mesh, Skeleton skeleton, Dictionary<string, string> nodeSidMap, SkinController skinController )
        {
            Bone rootBone = GetRootBone( mesh, skeleton );
            
            List<VertexBoneAssignment> vbaList = 
                geoSet.GetBoneAssignments( nodeSidMap, mesh.Skeleton );

            foreach( MeshGeometry geometry in geoSet )
            {
                string subMeshName = geometry.SubMeshes[ 0 ].name;
                SubMesh subMesh = mesh.GetSubMesh( subMeshName );

                if( 0 == vbaList.Count )
                {
                    geometry.RigidBindToBone( subMesh, rootBone );
                }
                else
                {
                    geometry.WeightedBindToBones( subMesh, vbaList );
                }
            }

            ReskinGeometry( geoSet, mesh, skeleton, nodeSidMap, skinController );
        }

        public void SkinMorphTarget( GeometrySet targetGeometry, Mesh mesh, Skeleton skeleton, Dictionary<string, string> nodeSidMap, SkinController skinController )
        {
            Bone rootBone = GetRootBone( mesh, skeleton );

            // Note that we get the bone assignments of the base 
            // geometry (i.e. the geometry owned by this instance)
            // because we are skinning the morphTarget to the base
            // geometry deformer.
            List<VertexBoneAssignment> vbaList = 
                geoSet.GetBoneAssignments( nodeSidMap, mesh.Skeleton );

            // Associate the base geometry bone assignments with the target.
            for( int i = 0; i < targetGeometry.Count; i++ )
            {
                targetGeometry[ i ].BoneAssignmentList = geoSet[ i ].BoneAssignmentList;
            }

            ReskinGeometry( targetGeometry, mesh, skeleton, nodeSidMap, skinController );
        }

        private Bone GetRootBone( Mesh mesh, Skeleton skeleton )
        {
            if( parentBone != null )
            {
                // What bone was this instance of the geometry under?
                return skeleton.GetBone( parentBone );
            }
            else
            {
                // Get the first root bone -- we will attach these to that bone
                return mesh.Skeleton.RootBone;
            }
        }

        // Note that we parametrize the GeometrySet to allow this to work with
        // both SkinGeometry() and SkinMorphTarget().  
        // TODO: This is not ideal, but the best compromise until I can manage until
        // I get around to a bunch more refactoring that makes morph targets part
        // of the instance itself.
        private void ReskinGeometry( GeometrySet geometrySet, Mesh mesh, Skeleton skeleton, Dictionary<string, string> nodeSidMap, SkinController skinController )
        {
            Quaternion worldRotate;
            Vector3 worldTranslate, worldScale;
            Matrix4.DecomposeMatrix( ref m_GlobalTransform, out worldTranslate, out worldRotate, out worldScale );

            foreach( MeshGeometry geometry in geometrySet )
            {
                // Update the geometry to effectively move the skeleton 
                // from the position that it was in for binding to the 
                // skeleton's idea of the bind pose.
                // This is likely to screw up the BIND_SHAPE_NORMAL or BIND_SHAPE_POSITION
                // systems that used to be used in Collada 1.3.
                Dictionary<string, Matrix4> invBindMatrices = new Dictionary<string, Matrix4>();

                // these matrices have not been transformed by the transform
                // argument from the command line or the unit conversion
                foreach( KeyValuePair<string, Matrix4> kvp in skinController.InverseBindMatrices )
                {
                    Matrix4 bindMatrix = kvp.Value.Inverse();

                    Quaternion bindRotate;
                    Vector3 bindTranslate, bindScale;
                    Matrix4.DecomposeMatrix( ref bindMatrix, out bindTranslate, out bindRotate, out bindScale );

                    Matrix4 boneTransform = (worldRotate * bindRotate).ToRotationMatrix();

                    boneTransform.Translation = MathHelpers.ScaleVector( worldScale, worldRotate * bindTranslate );

                    string boneName = kvp.Key;

                    if( nodeSidMap.ContainsKey( boneName ) )
                    {
                        boneName = nodeSidMap[ boneName ];
                    }

                    invBindMatrices[ boneName ] = boneTransform.Inverse();
                }

                geometry.Reskin( skeleton, invBindMatrices );
            }
        }
    }

    public class GeometrySet : List<MeshGeometry>
    {
        public string Name
        {
            get { return m_Name; }
        }
        string m_Name;

        public List<CombinerComponents> Combiner
        {
            get { return m_Combiner; }
            set { m_Combiner = value; }
        }
        List<CombinerComponents> m_Combiner = new List<CombinerComponents>();

        /// <summary>
        ///   This is the base geometry set (if we are a morph target), or 
        ///   null if we are not.
        /// </summary>
        public GeometrySet BaseGeometrySet
        {
            get { return m_BaseGeometrySet; }
            set { m_BaseGeometrySet = value; }
        }
        GeometrySet m_BaseGeometrySet = null;

        public bool IsPoseTarget
        {
            get { return m_BaseGeometrySet != null; }
        }

        public bool IsSkinned
        {
            get
            {
                if( 0 < this.Count )
                {
                    // If one submesh is skinned, they all are.
                    return this[ 0 ].IsSkinned;
                }
                return false;
            }
        }

        public GeometrySet( string name )
        {
            this.m_Name = name;
        }

        public GeometrySet Clone( string name )
        {
            GeometrySet clone = new GeometrySet( name );

            clone.m_BaseGeometrySet = this.m_BaseGeometrySet;

            foreach( MeshGeometry geometry in this )
            {
                clone.Add( geometry.Clone( name + "." + geometry.Id, clone ) );
            }
            
            return clone;
        }

        public List<VertexBoneAssignment> GetBoneAssignments( 
            Dictionary<string, string> nodeSidMap, Skeleton skeleton )
        {
            List<VertexBoneAssignment> vbas = new List<VertexBoneAssignment>();

            foreach( CombinerComponents components in Combiner )
            {
                foreach( CombinerEntry entry in components )
                {
                    VertexBoneAssignment vba = new VertexBoneAssignment();
                    vba.vertexIndex = components.VertexIndex;

                    for( int inputIndex = 0; inputIndex < entry.Count; ++inputIndex )
                    {
                        int ptIndex = entry[ inputIndex ];

                        List<InputSourceCollection> inputs = components.InputSources[ inputIndex ];
                        
                        foreach( InputSourceCollection isc in inputs )
                        {
                            Debug.Assert( isc is InputSource );
                        
                            InputSource source = (InputSource) isc;
                            
                            switch( source.Semantic )
                            {
                            case "JOINT":
                                {
                                    string jointParam = "";
                                    if( source.Accessor.ContainsParam( "JOINT" ) )
                                    {
                                        jointParam = "JOINT";
                                    }
                                    
                                    string jointName = (string) source.Accessor.GetParam( jointParam, ptIndex );
                                    
                                    if( nodeSidMap.ContainsKey( jointName ) )
                                    {
                                        jointName = nodeSidMap[ jointName ];
                                    }

                                    Bone bone = skeleton.GetBone( jointName );

                                    vba.boneIndex = bone.Handle;
                                }
                                break;

                            case "WEIGHT":
                                {
                                    string weightParam = "";
                                    if( source.Accessor.ContainsParam( "WEIGHT" ) )
                                    {
                                        weightParam = "WEIGHT";
                                    }
                                    vba.weight = (float) source.Accessor.GetParam( weightParam, ptIndex );
                                }
                                break;

                            default:
                                // TODO: This class currently does not have a log; add one?
                               // m_Log.InfoFormat( "Unhandled entry semantic: {0}", source.Semantic );
                                break;

                            }
                        }
                    }

                    vbas.Add( vba );
                }
            }

            return vbas;
        }
    }


    /// <summary>
    /// Abstraction of a bind_material element.  The element can occur, e.g.,
    /// in an instance_controller, and serves to bind concrete materials to
    /// the symbolic materials in a source geometry.
    /// 
    /// TODO: Currently this only maps abstract material names to the concrete
    /// material names. However, the bind_material element also defines a semantic
    /// specifying a UVSET for a given target.  I don't exactly understand how that
    /// works, so I have not yet implemented it.
    /// </summary>
    public sealed class BindMaterial
    {
        public Dictionary<string, string> Bindings
        {
            get { return m_Bindings; }
        }
        Dictionary<string, string> m_Bindings = new Dictionary<string, string>();


        public string MaterialNamespace
        {
            get { return m_MaterialNamespace; }
        }
        string m_MaterialNamespace;


        public BindMaterial( XmlNode bindMaterialNode )
            : this( bindMaterialNode, null )
        {
        }

        public BindMaterial( XmlNode bindMaterialNode, string materialNamespace )
        {
            m_MaterialNamespace = materialNamespace;

            XmlElement bindElement = bindMaterialNode as XmlElement;

            if( null != bindElement )
            {
#if WORKS_AS_EXPECTED
                XmlNodeList instances = bindElement.SelectNodes( "//instance_material" );
#else
                XmlNodeList instances = bindElement.SelectNodes( "./*/*" );
#endif

                foreach( XmlNode instance in instances )
                {
                    string symbol = (instance as XmlElement).GetAttribute( "symbol" );
                    string target = (instance as XmlElement).GetAttribute( "target" );

                    // The target is written as a url fragment name (e.g. "#frootBat" );
                    // we want to resolve it to the raw name.
                    target = target.Substring( 1 );

                    if( null != materialNamespace )
                    {
                        symbol = materialNamespace + "." + symbol;
                        target = materialNamespace + "." + target;
                    }

                    // We've observed DAE files that redundantly define <instance_material>
                    // elements.  I'm not sure if that's even kosher, but we can tolerate
                    // the condition nonetheless with no harm.
                    Bindings[ symbol ] = target;
                }
            }
        }
    }

    public class Sampler
    {
        string samplerId;
        InputSource input;
        InputSource output;
        InputSource interpolation;

        public Sampler( string samplerId )
        {
            this.samplerId = samplerId;
        }

        public string SamplerId
        {
            get { return samplerId; }
        }
        public InputSource Input
        {
            get { return input; }
            set { input = value; }
        }
        public InputSource Output
        {
            get { return output; }
            set { output = value; }
        }
        public InputSource Interpolation
        {
            get { return interpolation; }
            set { interpolation = value; }
        }
    }

    /// <summary>
    ///   Channel objects represent the binding between output values and the 
    ///   parameters that will be animated.
    /// </summary>
    public class Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof( Channel ) );

        // public const string DefaultTargetParam = "matrix";

        // string channelId;

        // If we are animating a bone or other node, we will have a
        // target node
        string targetNode;
        // If we are animating a controller source (for a morph), we will 
        // have a target source
        string targetSource;
        // The component only applies to animation of nodes, and is so that
        // you can target the rotate, translate, or overall transform of a node
        string component;
        // The param field identifies which parameter is being modified.  
        // This may be something like 'X' or by default "TRANSFORM"
        string param;
        // Which member of the attribute are we interested in.
        // This will typically be null, but may be something like "(0)"
        string targetMember;
        // This is the default name we will use for animations
        string animationName;
        // This is the topmost animation name
        string parentAnimationName;
        Sampler sampler;

        public Channel( Sampler sampler, string target, string defaultTargetParam )
        {
            // this.channelId = channelId;
            this.sampler = sampler;
            this.targetMember = null;
            int index = target.IndexOf( '/' );
            if( index == -1 )
            {
                int memberIndex = target.IndexOf( '(' );
                if( memberIndex != -1 )
                {
                    targetSource = target.Substring( 0, memberIndex );
                    targetMember = target.Substring( memberIndex );
                    target = target.Substring( 0, memberIndex );
                    log.Warn( "Only controller channel targets of the form 'component' are currently supported" );
                    component = target;
                    param = string.Empty;
                }
                else
                {
                    targetSource = target;
                    component = string.Empty;
                    param = string.Empty;
                }
            }
            else
            {
                targetNode = target.Substring( 0, index );
                target = target.Substring( index + 1 );
                index = target.IndexOf( '.' );
                if( index == -1 )
                {
                    int memberIndex = target.IndexOf( '(' );
                    if( memberIndex != -1 )
                    {
                        targetMember = target.Substring( memberIndex );
                        target = target.Substring( 0, memberIndex );
                        log.Warn( "Only node channel targets of the form 'node/component.param' are currently supported" );
                        component = target;
                        param = string.Empty;
                    }
                    else
                    {
                        component = target;
                        param = defaultTargetParam;
                    }
                }
                else
                {
                    component = target.Substring( 0, index );
                    param = target.Substring( index + 1 );
                }
            }
        }

        //public string ChannelId {
        //    get { return channelId; }
        //}
        public string TargetNode
        {
            get { return targetNode; }
        }
        public string TargetSource
        {
            get { return targetSource; }
        }
        public string TargetComponent
        {
            get { return component; }
        }
        public string TargetParam
        {
            get { return param; }
        }
        public string TargetMember
        {
            get { return targetMember; }
        }
        public string AnimationName
        {
            get { return animationName; }
            set { animationName = value; }
        }
        public string ParentAnimationName
        {
            get { return parentAnimationName; }
            set { parentAnimationName = value; }
        }
        public Sampler Sampler
        {
            get { return sampler; }
        }
    }

    public class MaterialTechnique_14
    {
        public class TextureInfo
        {
            public string imageId;
            public string channelId;
        }

        string techniqueId;
        ColorEx ambient;
        ColorEx emissive;
        ColorEx diffuse;
        ColorEx specular;
        TextureInfo ambientTexture = null;
        TextureInfo emissiveTexture = null;
        TextureInfo diffuseTexture = null;
        TextureInfo specularTexture = null;
        float shininess;

        public MaterialTechnique_14( string techniqueId )
        {
            this.techniqueId = techniqueId;
            ambient = ColorEx.White;
            diffuse = ColorEx.White;
            specular = ColorEx.Black;
            emissive = ColorEx.Black;
            shininess = 1;
        }


        public ColorEx Ambient
        {
            get { return ambient; }
            set { ambient = value; }
        }
        public ColorEx Emissive
        {
            get { return emissive; }
            set { emissive = value; }
        }
        public ColorEx Diffuse
        {
            get { return diffuse; }
            set { diffuse = value; }
        }
        public ColorEx Specular
        {
            get { return specular; }
            set { specular = value; }
        }
        public TextureInfo AmbientTexture
        {
            get { return ambientTexture; }
            set { ambientTexture = value; }
        }
        public TextureInfo EmissiveTexture
        {
            get { return emissiveTexture; }
            set { emissiveTexture = value; }
        }
        public TextureInfo DiffuseTexture
        {
            get { return diffuseTexture; }
            set { diffuseTexture = value; }
        }
        public TextureInfo SpecularTexture
        {
            get { return specularTexture; }
            set { specularTexture = value; }
        }
        public float Shininess
        {
            get { return shininess; }
            set { shininess = value; }
        }
    }

    /// <summary>
    ///   Attachment point object - these are attached to a bone with a local transform
    /// </summary>
    public class AttachmentPointNode
    {
        string name;
        string parentBone;
        Matrix4 transform;

        public AttachmentPointNode( string name, string parentBone, Matrix4 localTransform )
        {
            this.name = name;
            this.parentBone = parentBone;
            this.transform = localTransform;
        }

        public string Name
        {
            get { return name; }
        }
        public string ParentBone
        {
            get { return parentBone; }
        }
        public Matrix4 Transform
        {
            get { return transform; }
        }
    }

    public class PoseInfo
    {
        internal PoseInfo( GeometrySet baseGeometry, GeometrySet targetGeometry )
        {
            m_BaseGeometry = baseGeometry;
            m_TargetGeometry = targetGeometry;
        }

        // This is the geometry set that is the basis of the pose
        public GeometrySet BaseGeometry
        {
            get { return m_BaseGeometry; }
        }
        GeometrySet m_BaseGeometry;
        
        // This is the geometry set that represents the extreme
        // of the pose.
        public GeometrySet TargetGeometry
        {
            get { return m_TargetGeometry; }
        }
        GeometrySet m_TargetGeometry;
        
        // The index of the pose in the mesh
        public List<int> poseIndices = new List<int>();

        internal bool HasThisGeometry( GeometrySet baseGeometry, GeometrySet targetGeometry )
        {
            return baseGeometry.Equals( m_BaseGeometry ) && 
                   targetGeometry.Equals( m_TargetGeometry );
        }

        internal void Build( Mesh axiomMesh )
        {
            // We need the meshes to be split the same way.  This will 
            // not guarrantee that, but will catch some cases where it
            // is broken.
            Debug.Assert( m_BaseGeometry.Count == m_TargetGeometry.Count );

            for( int geoIndex = 0; geoIndex < m_BaseGeometry.Count; ++geoIndex )
            {
                int targetSubmeshIndex = GetSubmeshIndex( axiomMesh, geoIndex );

                VertexDataEntry baseVertexDataEntry   = FindPositionVertexDataEntry( m_BaseGeometry[ geoIndex ] );
                VertexDataEntry targetVertexDataEntry = FindPositionVertexDataEntry( m_TargetGeometry[ geoIndex ] );

                Debug.Assert( (null != baseVertexDataEntry), "Missing base position data for morph" );
                Debug.Assert( (null != targetVertexDataEntry), "Missing target position data for morph" );

                // The ColladaMeshInfo's poseInfoList has a list of PoseInfo objects 
                // (including poseInfo).  Each of the PoseInfo objects has a reference
                // to the two geometry sets involved, as well as the list of indices 
                // of the pose entries in the mesh.  There are often multiple pose
                // objects for a single base and target geometry pair, since there 
                // may be multiple submeshes as part of the geometry.
                poseIndices.Add( axiomMesh.PoseList.Count );

                // Create the pose, and add all the vertices that are not the same
                string poseName = m_TargetGeometry[ geoIndex ].Id;

                Pose pose = axiomMesh.CreatePose( (ushort) targetSubmeshIndex, poseName );

                if( ! BuildPoseDeltas( pose, baseVertexDataEntry, targetVertexDataEntry ) )
                {
#if THIS_IS_BROKEN
// "Exactly what is broken?" you ask. If I remove a pose here I get an
// assert in the engine, specifically in Axiom.Core.Mesh.SoftwareVertexPoseBlend(),
// that leads to a crash. The problem seems to be something about other
// pose artifacts lying around, so the blender still attempts to build
// something into a bogus buffer.
//
// By disabling this 'remove' we get correct behavior, but at a performance
// penalty. A better way to eliminate 'identity' morphs is to avoid adding
// the pose in the first place.  This will not be simple to do because the
// way poses are handled is still not very well encapsulated, and there are 
// a fair few dependencies and couplings lingering in ColladaMeshInfo.
//
// TODO: Further refactor until we can eliminate 'identity' morphs.

                    axiomMesh.RemovePose( poseName );
#endif
                }

            }
        }

        private static bool BuildPoseDeltas(Pose pose , VertexDataEntry baseVertexDataEntry, VertexDataEntry targetVertexDataEntry)
        {
            bool doBaseAndTargetDiffer = false;

            for( int vertexId = 0; vertexId < baseVertexDataEntry.fdata.GetLength( 0 ); ++vertexId )
            {
                Vector3 startPos, endPos;

                startPos.x = baseVertexDataEntry.fdata[ vertexId, 0 ];
                startPos.y = baseVertexDataEntry.fdata[ vertexId, 1 ];
                startPos.z = baseVertexDataEntry.fdata[ vertexId, 2 ];

                endPos.x = targetVertexDataEntry.fdata[ vertexId, 0 ];
                endPos.y = targetVertexDataEntry.fdata[ vertexId, 1 ];
                endPos.z = targetVertexDataEntry.fdata[ vertexId, 2 ];

                if( startPos != endPos )
                {
                    pose.AddVertex( vertexId, endPos - startPos );

                    doBaseAndTargetDiffer = true;
                }
            }

            return doBaseAndTargetDiffer;
        }

        private int GetSubmeshIndex( Mesh axiomMesh, int geoIndex )
        {
            SubMesh baseSubmesh = axiomMesh.GetSubMesh( m_BaseGeometry[ geoIndex ].Id );
            int targetSubmeshIndex = -1;
            for( ushort j = 0; j < axiomMesh.SubMeshCount; ++j )
            {
                if( axiomMesh.GetSubMesh( j ) == baseSubmesh )
                {
                    if( baseSubmesh.useSharedVertices )
                        targetSubmeshIndex = 0;
                    else
                        targetSubmeshIndex = j + 1;
                    break;
                }
            }
            Debug.Assert( targetSubmeshIndex >= 0 );
            return targetSubmeshIndex;
        }

        // Search the vertex data entries of the geometry for one that has
        // the "Position" semantic.  Returns null if not found.
        private static VertexDataEntry FindPositionVertexDataEntry( MeshGeometry geometry )
        {
            VertexDataEntry positionDataEntry = null;

            for( int vdeIndex = 0; vdeIndex < geometry.VertexDataEntries.Count; ++vdeIndex )
            {
                if( geometry.VertexDataEntries[ vdeIndex ].semantic.Equals( VertexElementSemantic.Position ) )
                {
                    positionDataEntry = geometry.VertexDataEntries[ vdeIndex ];
                    break;
                }
            }
            
            return positionDataEntry;
        }
    }

    public class SurfaceSampler
    {
        public TextureType texType;
        public string samplerName;
        public string surfaceName;
    }

    public class Surface
    {
        /// <summary>
        ///   The type of image we are referencing
        /// </summary>
        public TextureType texType;
        /// <summary>
        ///   The name of the image from the image library
        /// </summary>
        public string image;
    }

    // holds a <node> in a <library_nodes> module
    public class LibraryNode
    {
        public Matrix4 transform = Matrix4.Identity; // the node's relative transform
        public List<LibraryNode> children = new List<LibraryNode>();  // any child nodes
        public List<string> geoInstanceIds = new List<string>();  // and geometry instance references at this level
    }

}
