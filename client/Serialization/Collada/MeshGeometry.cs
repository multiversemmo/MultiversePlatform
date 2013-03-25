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
using System.Diagnostics;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Multiverse.Serialization.Collada
{
    /// <summary>
    ///   This object essentially corresponds to a polygons or
    ///   triangles clause in the collada file.
    /// </summary>
    public class MeshGeometry
    {
        /// <summary>
        ///  The id of this geometry object.
        /// </summary>
        public string Id
        {
            get { return id; }
        }
        string id;

        /// <summary>
        /// 
        /// </summary>
        public int InputCount
        {
            get { return inputSources.Count; }
        }

        /// <summary>
        /// The list of faces -- indices of entries into pointSets
        /// </summary>
        public List<int[]> Faces
        {
            get { return faces; }
        }
        List<int[]> faces;

        /// <summary>
        /// 
        /// </summary>
        public List<SubMeshInfo> SubMeshes
        {
            get { return subMeshes; }
        }
        List<SubMeshInfo> subMeshes;

        ///<summary>
        ///  This is fundamentally the list of all the vertices.  Each
        ///  vertex is represented here as a list indexes into inputs.
        ///</summary>
        //
        // This will be used for the vertex buffers.  Each vertex will end
        // up being composed of one entry for each item in the point set.
        // A point set is something like the normal, position, texcoord 
        // and color of a vertex.
        // The PointComponents object is basically just a list of the
        // indices for the inputs.
        public List<PointComponents> IndexSets
        {
            get { return pointSets; }
        }
        List<PointComponents> pointSets;

        /// <summary>
        ///   This is the set of input source collections associated with 
        ///   something like a triangles clause.  Some of these input 
        ///   sources are compound (e.g. a vertex), so there is not a
        ///   one to one mapping from InputSource to entries in the
        ///   PointComponents object.  Use GetAllInputSources if you want
        ///   the list that maps to PointComponents entries.
        /// </summary>
        public Dictionary<int, List<InputSourceCollection>> InputSources
        {
            get { return inputSources; }
        }

        /// <summary>
        ///   This is temporary storage for the data that will go in our 
        ///   vertex buffers.  I leave in here so I can run transforms on it
        ///   as we go through this code.
        /// </summary>
        public List<VertexDataEntry> VertexDataEntries
        {
            get { return vertexDataEntries; }
        }
        List<VertexDataEntry> vertexDataEntries;


        /// <summary>
        ///   Our vertex data.  This is also stored in the mesh or submesh, 
        ///   but it's convenient to have a handle here as well.
        /// </summary>
        public VertexData VertexData
        {
            get { return vertexData; }
            set { vertexData = value; }
        }
        VertexData vertexData = new VertexData();

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, List<VertexBoneAssignment>> BoneAssignmentList
        {
            get { return boneAssignmentList; }
            set { boneAssignmentList = value; }
        }
        Dictionary<int, List<VertexBoneAssignment>> boneAssignmentList = null;


        public bool IsSkinned
        {
            get { return null != BoneAssignmentList; }
        }

        /// <summary>
        /// 
        /// </summary>
        public GeometrySet GeometrySet
        {
            get { return parentGeometrySet; }
        }
        GeometrySet parentGeometrySet;


        // Create a logger for use in this class
        protected static readonly log4net.ILog m_Log = log4net.LogManager.GetLogger( typeof( MeshGeometry ) );


        // The input sources (e.g. normal vector data or texture coordinates)
        // This is a mapping from input index to input source.  The index is
        // in the context of the parent 'triangles' node, where we are the
        // nested input node, and have an 'idx' attribute.  The values are
        // the lists of input sources associated with that input index.  
        // These input sources may be simple input sources, like color, 
        // or complex input source, like the vertex.
        Dictionary<int, List<InputSourceCollection>> inputSources;

        // Mapping between the vertex id within the parent Geometry object,
        // to the vertex indices within this submeshes of that vertex.
        Dictionary<int, List<int>> vertexIds;

        // The list of vertex entries.  Each of these entries corresponds to 
        // a 'vertices' clause in the xml
        Dictionary<string, VertexSet> vertexDict;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id of this mesh</param>
        /// <param name="geoSet">the Geometry set this mesh belongs to</param>
        public MeshGeometry( string id, GeometrySet geoSet )
        {
            this.id = id;
            parentGeometrySet = geoSet;

            inputSources = new Dictionary<int, List<InputSourceCollection>>();
            pointSets    = new List<PointComponents>();
            faces        = new List<int[]>();
            vertexDict   = new Dictionary<string, VertexSet>();
            vertexIds    = new Dictionary<int, List<int>>();
            subMeshes    = new List<SubMeshInfo>();
            vertexDataEntries = new List<VertexDataEntry>();
        }

        public MeshGeometry Clone( string id, GeometrySet geoSet )
        {
            MeshGeometry clone = new MeshGeometry( id, geoSet );

            foreach( int ii in this.inputSources.Keys )
            {
                clone.AddInputs( ii, this.inputSources[ ii ] );
            }
            foreach( PointComponents pc in this.pointSets )
            {
                clone.pointSets.Add( pc );
            }
            foreach( int[] f in this.faces )
            {
                clone.faces.Add( f );
            }
            foreach( string vs in this.vertexDict.Keys )
            {
                clone.vertexDict[ vs ] = this.vertexDict[ vs ];
            }
            foreach( int vi in this.vertexIds.Keys )
            {
                clone.vertexIds[ vi ] = this.vertexIds[ vi ];
            }
            foreach( VertexDataEntry vde in this.vertexDataEntries )
            {
                clone.vertexDataEntries.Add( vde.Clone() );
            }

            clone.VertexData = this.VertexData.Clone();

            if( this.boneAssignmentList != null )
            {
                foreach( int vba in this.boneAssignmentList.Keys )
                {
                    clone.boneAssignmentList[ vba ] = this.boneAssignmentList[ vba ];
                }
            }

            int smi_count = 0;
            foreach( SubMeshInfo smi in this.subMeshes )
            {
                SubMeshInfo new_smi = new SubMeshInfo();
                new_smi.name = this.subMeshes.Count > 1 ? id + "." + (smi_count++).ToString() : id;
                new_smi.material = smi.material;
                clone.subMeshes.Add( new_smi );
            }
            return clone;
        }


        public void AddSubMesh( SubMeshInfo subMesh )
        {
            subMeshes.Add( subMesh );
        }

        public void AddInputs( int inputIndex, List<InputSourceCollection> input )
        {
            if( inputSources.ContainsKey( inputIndex ) )
            {
                m_Log.InfoFormat( "Duplicate input index: {0}", inputIndex );
            }

            inputSources[ inputIndex ] = input;
        }

        public void AddVertexSet( VertexSet vertexSet )
        {
            vertexDict[ vertexSet.id ] = vertexSet;
        }

        private VertexSet GetVertexSet( string name )
        {
            return vertexDict[ name ];
        }

        public List<InputSource> GetAllInputSources()
        {
            List<InputSource> sources = new List<InputSource>();

            foreach( List<InputSourceCollection> tmpList in inputSources.Values )
            {
                foreach( InputSourceCollection tmp in tmpList )
                {
                    sources.AddRange( tmp.GetSources() );
                }
            }
            return sources;
        }

        public PointComponents BeginPoint()
        {
            return new PointComponents();
        }

        /// <summary>
        ///   Add a point component.
        /// </summary>
        /// <param name="index">Index within the given input source</param>
        public void AddPointComponent( PointComponents currentPoint, int index )
        {
            currentPoint.Add( index );
        }

        public void EndPoint( PointComponents currentPoint, List<int> cwPolyPoints, List<int> ccwPolyPoints )
        {
            AddPointComponents( cwPolyPoints, currentPoint );

            // Build a copy of currentPointComponents with normals inverted
            if( ccwPolyPoints == null )
                return;

            PointComponents tmpPointComponents = new PointComponents( currentPoint );
            
            List<InputSource> sources = GetAllInputSources();
            
            for( int accessIndex = 0; accessIndex < sources.Count; ++accessIndex )
            {
                // For normals, since we need to invert them in the copy,
                // we use a negating accessor, and tamper with the point
                // component to change the index to the portion that is negated.
                if( sources[ accessIndex ].IsNormal )
                {
                    Debug.Assert( sources[ accessIndex ].Accessor is NegatingFloatAccessor );

                    tmpPointComponents[ accessIndex ] = -1 * tmpPointComponents[ accessIndex ];
                }
            }

            AddPointComponents( ccwPolyPoints, tmpPointComponents );
        }

        /// <summary>
        ///   This adds the composite point currentPoint to the set of 
        ///   points in this object.  If there is already a point that
        ///   matches, we will use that instead.
        /// </summary>
        /// <param name="polyPoints"></param>
        /// <param name="currentPoint"></param>
        private void AddPointComponents( List<int> polyPoints, PointComponents currentPoint )
        {
            if( polyPoints == null )
                return;

            int compositeIndex = pointSets.IndexOf( currentPoint );

            if( compositeIndex < 0 )
            {
                // we don't already have an entry for this point
                int vertexIndex = currentPoint.VertexIndex;
                if( vertexIndex < 0 )
                {
                    m_Log.ErrorFormat( "No vertex id for point with vertex index: {0}", vertexIndex );
                }

                pointSets.Add( currentPoint );
                
                compositeIndex = pointSets.Count - 1;

                if( !vertexIds.ContainsKey( vertexIndex ) )
                {
                    vertexIds[ vertexIndex ] = new List<int>();
                }
                
                vertexIds[ vertexIndex ].Add( compositeIndex );
            }

            polyPoints.Add( compositeIndex );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<int> BeginPolygon()
        {
            return new List<int>();
        }

        /// <summary>
        ///   Need to build a triangulation of the polygon
        /// </summary>
        /// <param name="polyPoints"></param>
        /// <param name="front"></param>
        /// <param name="back"></param>
        public void EndPolygon( List<int> polyPoints )
        {
            Debug.Assert( polyPoints.Count >= 3 );

            // TODO: Check that all the points are coplanar
            // TODO: Handle holes and maybe self-intersecting polygons
            //       For now, I just skip self-intersecting polygons
            // TODO: Perhaps a Delaunay triangulation (edge-flip)
            //       should be done here.
            List<Vector3> contour = new List<Vector3>();
            List<int> contourToComposite = new List<int>();
            List<InputSource> sources = GetAllInputSources();

            // Find the input with position semantic
            for( int accessIndex = 0; accessIndex < sources.Count; ++accessIndex )
            {
                if( sources[ accessIndex ].IsPosition )
                {
                    // We found the position source. Use this to build a contour
                    // and triangluate the polygon.
                    // This code is not really used anymore, since we can now do
                    // the triangulation in maya instead.
                    InputSource source = sources[ accessIndex ];
                    
                    foreach( int compositeIndex in polyPoints )
                    {
                        int inputIndex = pointSets[ compositeIndex ][ accessIndex ];
                        Vector3 pt;
                        pt.x = (float) source.Accessor.GetParam( "X", inputIndex );
                        pt.y = (float) source.Accessor.GetParam( "Y", inputIndex );
                        pt.z = (float) source.Accessor.GetParam( "Z", inputIndex );

                        contourToComposite.Add( compositeIndex );
                        
                        contour.Add( pt );
                    }
                    break;  // done with position
                }
            }

            List<int[]> result = new List<int[]>();

            bool status = Multiverse.MathLib.TriangleTessellation.Process( contour, result );

            if( !status )
            {
                m_Log.Warn( "Skipping self-intersecting polygon - this may simply be a sliver polygon" );
            }
            else
            {
                foreach( int[] tri in result )
                {
                    int[] faceIndices;
                    faceIndices = new int[ 3 ];
                    faceIndices[ 0 ] = contourToComposite[ tri[ 0 ] ];
                    faceIndices[ 1 ] = contourToComposite[ tri[ 1 ] ];
                    faceIndices[ 2 ] = contourToComposite[ tri[ 2 ] ];
                    faces.Add( faceIndices );
                }
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int[ , ] GetFaceData()
        {
            int[ , ] faceData = new int[ faces.Count, 3 ];
            for( int faceIndex = 0; faceIndex < faces.Count; ++faceIndex )
            {
                int[] face = faces[ faceIndex ];

                faceData[ faceIndex, 0 ] = face[ 0 ];
                faceData[ faceIndex, 1 ] = face[ 1 ];
                faceData[ faceIndex, 2 ] = face[ 2 ];
            }
            return faceData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        public List<int> GetVertexIds( int vertexId )
        {
            if( vertexIds.ContainsKey( vertexId ) )
            {
                return vertexIds[ vertexId ];
            }
            return null;
        }

        internal void RigidBindToBone( SubMesh subMesh, Bone bone )
        {
            m_Log.InfoFormat( "Creating rigid binding from {0} to {1}", Id, bone.Name );

            for( int i = 0; i < subMesh.vertexData.vertexCount; ++i )
            {
                VertexBoneAssignment vba = new VertexBoneAssignment();

                vba.boneIndex = bone.Handle;

                vba.vertexIndex = i;

                vba.weight = 1.0f;

                subMesh.AddBoneAssignment( vba );
            }

            BoneAssignmentList = subMesh.BoneAssignmentList;
        }

        internal void WeightedBindToBones( SubMesh subMesh, List<VertexBoneAssignment> vbaList )
        {
            // Some of these vertices needed to be split into multiple vertices.
            // Rebuild the vertex bone assignment list, to have entries for our
            // new vertex ids instead.
            foreach( VertexBoneAssignment item in vbaList )
            {
                List<int> subMeshVertexIds = GetVertexIds( item.vertexIndex );

                if( null != subMeshVertexIds )
                {
                    foreach( int subVertexId in subMeshVertexIds )
                    {
                        VertexBoneAssignment vba = new VertexBoneAssignment( item );

                        vba.vertexIndex = subVertexId;

                        subMesh.AddBoneAssignment( vba );
                    }
                }
            }

            BoneAssignmentList = subMesh.BoneAssignmentList;
        }

        /// <summary>
        /// Reskin this geometry to the skeleton's position as the bind position.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="invBindMatrices">Dictionary mapping bone id to inverse bind matrix for that bone</param>
        internal void Reskin( Skeleton skeleton, Dictionary<string, Matrix4> invBindMatrices )
        {
            if( 0 < VertexDataEntries.Count )
            {
                m_Log.InfoFormat( "Generating per-vertex transforms for {0}", Id );

                Debug.Assert( null != VertexDataEntries[ 0 ].fdata,
                    String.Format( "Invalid VertexDataEntry on geometry '{0}'", Id ) );


                int vertexCount = VertexDataEntries[ 0 ].fdata.GetLength( 0 );
                
                for( int vIdx = 0; vIdx < vertexCount; ++vIdx )
                {
                    if( ! BoneAssignmentList.ContainsKey( vIdx ) )
                    {
                        // TODO: When does this condition actually occur? --jrl
                        // Is there a better way to bail out?  Do we need notification?
                        m_Log.InfoFormat(
                            "No bone assignment for vertex[ {0} ] on geometry '{1}'",
                            vIdx, Id );
                        return;
                    }

                    Matrix4 vertexTransform = Matrix4.Zero;

                    foreach( VertexBoneAssignment vba in BoneAssignmentList[ vIdx ] )
                    {
                        Bone bone = skeleton.GetBone( vba.boneIndex );

                        Matrix4 mtx = bone.FullTransform * invBindMatrices[ bone.Name ];
                        
                        vertexTransform += mtx * vba.weight;
                    }
                    
                    Transform( vertexTransform, vIdx );
                }
            }
        }

        /// <summary>
        ///   Transform the vertices
        /// </summary>
        /// <remarks>
        /// TODO: This is conceivably an inefficient implementation. I could 
        /// restructure the loop to have a similar switch() statement as in
        /// Transform( Matrix4, int ), and have Transform*Float3() methods 
        /// that iterate over the entire array in the entry.  I've run some
        /// casual performance checks on that type of implementation, and found
        /// no improvement in performance.  For now, I opt for simpler code.
        /// </remarks>
        /// <param name="transform"></param>
        /// <param name="geometry"></param>
        internal void Transform( Matrix4 transform )
        {
            if( 0 < VertexDataEntries.Count )
            {
                int vertexCount = VertexDataEntries[ 0 ].fdata.GetLength( 0 );

                for( int index = 0; index < vertexCount; index++ )
                {
                    Transform( transform, index );
                }
            }
        }


        /// <summary>
        ///   Transform a single vertex
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="geometry"></param>
        /// <param name="index"></param>
        private void Transform( Matrix4 transform, int index )
        {
            foreach( VertexDataEntry entry in VertexDataEntries )
            {
                switch( entry.semantic )
                {
                case VertexElementSemantic.Position:
                    {
                        TransformFloat3( transform, entry.fdata, index );
                        break;
                    }

                case VertexElementSemantic.Normal:
                case VertexElementSemantic.Tangent:
                case VertexElementSemantic.Binormal:
                    { 
                        TransformAndNormalizeFloat3( transform, entry.fdata, index );
                        break;
                    }
                }
            }
        }


        private static void TransformFloat3( Matrix4 transform, float[ , ] data, int index )
        {
            Vector3 tmp = new Vector3( data[ index, 0 ], data[ index, 1 ], data[ index, 2 ] );

            tmp = transform * tmp;

            data[ index, 0 ] = tmp.x;
            data[ index, 1 ] = tmp.y;
            data[ index, 2 ] = tmp.z;
        }

        private static void TransformAndNormalizeFloat3( Matrix4 transform, float[ , ] data, int index )
        {
            Matrix4 matrix = transform;

            matrix.Translation = Vector3.Zero;

            Vector3 tmp = matrix * new Vector3( data[ index, 0 ], data[ index, 1 ], data[ index, 2 ] ); ;

            tmp.Normalize();

            data[ index, 0 ] = tmp.x;
            data[ index, 1 ] = tmp.y;
            data[ index, 2 ] = tmp.z;
        }
    }
}
