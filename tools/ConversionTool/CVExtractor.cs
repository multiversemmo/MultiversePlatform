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
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Utility;
using Axiom.Graphics;
using Axiom.Collections;
using Axiom.Serialization;
using Axiom.Configuration;
using Axiom.Animating;

using Multiverse.Serialization;
using Multiverse.Serialization.Collada;
using Multiverse.CollisionLib;

namespace Multiverse.ConversionTool
{
    public class CVExtractor
    {
        const string ExtractionLogFile = "../ExtractLog.txt";

        public static bool DoLog = false;

        public static StreamWriter writer;

        public static void InitLog( bool b )
        {
            DoLog = b;
            string p = ExtractionLogFile;
            if( DoLog )
            {
                FileStream f = new FileStream( p,
                                              (File.Exists( p ) ? FileMode.Append : FileMode.Create),
                                              FileAccess.Write );
                writer = new StreamWriter( f );
                writer.Write( string.Format( "\n\n\n\n{0} Started writing to {1}\r\n",
                                           DateTime.Now.ToString( "hh:mm:ss" ), p ) );
            }
        }

        public static void CloseLog()
        {
            if( writer != null )
                writer.Close();
        }

        public static void Log( string what )
        {
            writer.Write( string.Format( "{0} {1}\r\n",
                                       DateTime.Now.ToString( "hh:mm:ss" ), what ) );
            writer.Flush();
        }

        private class MeshTriangle
        {
            public Vector3[] vertPos;
            public MeshTriangle( Vector3 p0, Vector3 p1, Vector3 p2 )
            {
                vertPos = new Vector3[ 3 ];
                vertPos[ 0 ] = p0;
                vertPos[ 1 ] = p1;
                vertPos[ 2 ] = p2;
            }
            public Vector3 Center()
            {
                // Not a true centroid, just an average, because it's
                // cheaper, but good enough for our purposes
                return (vertPos[ 0 ] + vertPos[ 1 ] + vertPos[ 2 ]) / 3.0f;
            }
            public override string ToString()
            {
                return string.Format( "triangle({0},{1},{2})",
                                     vertPos[ 0 ].ToString(), vertPos[ 1 ].ToString(), vertPos[ 2 ].ToString() );
            }
        }

        private static MeshTriangle[] ExtractSubmeshTriangles( SubMesh subMesh )
        {
            int[] vertIdx = new int[ 3 ];
            Vector3[] vertPos = new Vector3[ 3 ];
            VertexElement posElem = subMesh.vertexData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.Position );
            HardwareVertexBuffer posBuffer = posBuffer = subMesh.vertexData.vertexBufferBinding.GetBuffer( posElem.Source );
            IntPtr indexPtr = subMesh.indexData.indexBuffer.Lock( BufferLocking.ReadOnly );
            IntPtr posPtr = posBuffer.Lock( BufferLocking.ReadOnly );
            int posOffset = posElem.Offset / sizeof( float );
            int posStride = posBuffer.VertexSize / sizeof( float );
            int numFaces = subMesh.indexData.indexCount / 3;
            MeshTriangle[] triangles = new MeshTriangle[ numFaces ];
            unsafe
            {
                int* pIdxInt32 = null;
                short* pIdxShort = null;
                float* pVPos = (float*) posPtr.ToPointer();
                if( subMesh.indexData.indexBuffer.Type == IndexType.Size32 )
                    pIdxInt32 = (int*) indexPtr.ToPointer();
                else
                    pIdxShort = (short*) indexPtr.ToPointer();

                // loop through all faces to calculate the tangents
                for( int n = 0; n < numFaces; n++ )
                {
                    for( int i = 0; i < 3; i++ )
                    {
                        // get indices of vertices that form a polygon in the position buffer
                        if( subMesh.indexData.indexBuffer.Type == IndexType.Size32 )
                            vertIdx[ i ] = pIdxInt32[ 3 * n + i ];
                        else
                            vertIdx[ i ] = pIdxShort[ 3 * n + i ];
                        vertPos[ i ].x = pVPos[ vertIdx[ i ] * posStride + posOffset ];
                        vertPos[ i ].y = pVPos[ vertIdx[ i ] * posStride + posOffset + 1 ];
                        vertPos[ i ].z = pVPos[ vertIdx[ i ] * posStride + posOffset + 2 ];
                    }
                    triangles[ n ] = new MeshTriangle( vertPos[ 0 ], vertPos[ 1 ], vertPos[ 2 ] );
                }
            }
            posBuffer.Unlock();
            subMesh.indexData.indexBuffer.Unlock();
            if( DoLog )
            {
                int count = triangles.Length;
                Log( string.Format( " extracted {0} triangles", count ) );
                for( int i = 0; i < count; i++ )
                    Log( string.Format( "  {0}", triangles[ i ].ToString() ) );
            }
            return triangles;
        }

        private const float geometryEpsilon = .1f;

        private static bool Orthogonal( Vector3 v1, Vector3 v2 )
        {
            return v1.Cross( v2 ).Length < geometryEpsilon;
        }

        private static bool Parallel( Plane p1, Plane p2 )
        {
            return p1.Normal.Cross( p2.Normal ).Length < geometryEpsilon;
        }

        private static bool Orthogonal( Plane p1, Plane p2 )
        {
            return p1.Normal.Cross( p2.Normal ).Length > geometryEpsilon;
        }

        private static bool SamePlane( Plane p1, Plane p2 )
        {
            // The planes are only the same if the Normals point in
            // the same direction, not opposite directions
            return (Math.Abs( p1.Normal.Dot( p2.Normal ) - 1 ) < geometryEpsilon &&
                    Math.Abs( p1.D - p2.D ) < geometryEpsilon);
        }

        private static void NormalizePlane( ref Plane p )
        {
            int negativeCount = 0;
            for( int i = 0; i < 3; i++ )
            {
                // Just return if any component is positive
                if( p.Normal[ i ] > 0 )
                    return;
                if( p.Normal[ i ] < 0 )
                    negativeCount++;
            }
            if( negativeCount > 0 )
            {
                p.Normal = -p.Normal;
                p.D = -p.D;
            }
        }

        private static CollisionShape ExtractBox_Old( SubMesh subMesh )
        {
            if( DoLog )
            {
                Log( "" );
                Log( string.Format( "Extracting box for submesh {0}", subMesh.Name ) );
            }
            MeshTriangle[] triangles = ExtractSubmeshTriangles( subMesh );
            int count = triangles.Length;
            Plane[] planesUnsorted = new Plane[ 6 ];
            int planeCount = 0;
            // Iterate through the triangles.  For each triangle,
            // determine the plane it belongs to, and find the plane
            // in the array of planes, or if it's not found, add it.
            for( int i = 0; i < count; i++ )
            {
                MeshTriangle t = triangles[ i ];
                bool found = false;
                Plane p = new Plane( t.vertPos[ 0 ], t.vertPos[ 1 ], t.vertPos[ 2 ] );
                NormalizePlane( ref p );
                if( DoLog )
                    Log( string.Format( " {0} => plane {1}", t.ToString(), p.ToString() ) );
                for( int j = 0; j < planeCount; j++ )
                {
                    Plane pj = planesUnsorted[ j ];
                    if( SamePlane( pj, p ) )
                    {
                        if( DoLog )
                            Log( string.Format( " plane {0} same as plane {1}", p.ToString(), pj.ToString() ) );
                        found = true;
                        break;
                    }
                }
                if( !found )
                {
                    if( planeCount < 6 )
                    {
                        if( DoLog )
                            Log( string.Format( " plane[{0}] = plane {1}", planeCount, p.ToString() ) );
                        planesUnsorted[ planeCount++ ] = p;
                    }
                    else
                        Debug.Assert( false,
                                     string.Format( "In the definition of box {0}, more than 6 planes were found",
                                                   subMesh.Name ) );
                }
            }
            Debug.Assert( planeCount == 6,
                         string.Format( "In the definition of box {0}, fewer than 6 planes were found",
                                       subMesh.Name ) );
            // Now recreate the planes array, making sure that
            // opposite faces are 3 planes apart
            Plane[] planes = new Plane[ 6 ];
            bool[] planeUsed = new bool[ 6 ];
            for( int i = 0; i < 6; i++ )
                planeUsed[ i ] = false;
            int planePair = 0;
            for( int i = 0; i < 6; i++ )
            {
                if( !planeUsed[ i ] )
                {
                    Plane p1 = planesUnsorted[ i ];
                    planes[ planePair ] = p1;
                    planeUsed[ i ] = true;
                    for( int j = 0; j < 6; j++ )
                    {
                        Plane p2 = planesUnsorted[ j ];
                        if( !planeUsed[ j ] && !Orthogonal( p2, p1 ) )
                        {
                            planes[ 3 + planePair++ ] = p2;
                            planeUsed[ j ] = true;
                            break;
                        }
                    }
                }
            }
            Debug.Assert( planePair == 3, "Didn't find 3 pairs of parallel planes" );
            // Make sure that the sequence of planes follows the
            // right-hand rule
            if( planes[ 0 ].Normal.Cross( planes[ 1 ].Normal ).Dot( planes[ 3 ].Normal ) < 0 )
            {
                // Swap the first two plane pairs
                Plane p = planes[ 0 ];
                planes[ 0 ] = planes[ 1 ];
                planes[ 1 ] = p;
                p = planes[ 0 + 3 ];
                planes[ 0 + 3 ] = planes[ 1 + 3 ];
                planes[ 1 + 3 ] = p;
                Debug.Assert( planes[ 0 ].Normal.Cross( planes[ 1 ].Normal ).Dot( planes[ 3 ].Normal ) > 0,
                             "Even after swapping, planes don't obey the right-hand rule" );
            }
            // Now we have our 6 planes, sorted so that opposite
            // planes are 3 planes apart, and so they obey the
            // right-hand rule.  This guarantees that corners
            // correspond.  Find the 8 intersections that define the
            // corners.
            Vector3[] corners = new Vector3[ 8 ];
            int cornerCount = 0;
            for( int i = 0; i <= 3; i += 3 )
            {
                Plane p1 = planes[ i ];
                for( int j = 1; j <= 4; j += 3 )
                {
                    Plane p2 = planes[ j ];
                    for( int k = 2; k <= 5; k += 3 )
                    {
                        Plane p3 = planes[ k ];
                        Vector3 corner = -1 * ((p1.D * (p2.Normal.Cross( p3.Normal )) +
                                                p2.D * (p3.Normal.Cross( p1.Normal )) +
                                                p3.D * (p1.Normal.Cross( p2.Normal ))) /
                                               p1.Normal.Dot( p2.Normal.Cross( p3.Normal ) ));
                        Debug.Assert( cornerCount < 8,
                                     string.Format( "In the definition of box {0}, more than 8 corners were found",
                                                   subMesh.Name ) );
                        if( DoLog )
                            Log( string.Format( "  corner#{0}: {1}", cornerCount, corner.ToString() ) );
                        corners[ cornerCount++ ] = corner;
                    }
                }
            }
            Debug.Assert( cornerCount == 8,
                          string.Format( "In the definition of box {0}, fewer than 8 corners were found",
                                        subMesh.Name ) );
            // We know that corners correspond.  Now find the center
            Vector3 center = (corners[ 0 ] + corners[ 7 ]) / 2;
            Debug.Assert( (center - (corners[ 1 ] + corners[ 5 ]) / 2.0f).Length > geometryEpsilon ||
                          (center - (corners[ 2 ] + corners[ 6 ]) / 2.0f).Length > geometryEpsilon ||
                          (center - (corners[ 3 ] + corners[ 7 ]) / 2.0f).Length > geometryEpsilon,
                          string.Format( "In the definition of box {0}, center definition {0} is not consistent",
                                        subMesh.Name, center.ToString() ) );
            // Find the extents
            Vector3 extents = new Vector3( Math.Abs( (corners[ 1 ] - corners[ 0 ]).Length / 2.0f ),
                                          Math.Abs( (corners[ 3 ] - corners[ 1 ]).Length / 2.0f ),
                                          Math.Abs( (corners[ 4 ] - corners[ 0 ]).Length / 2.0f ) );
            if( DoLog )
                Log( string.Format( " extents {0}", extents.ToString() ) );
            // Find the axes
            Vector3[] axes = new Vector3[ 3 ] { (corners[1] - corners[0]).ToNormalized(),
											   (corners[3] - corners[1]).ToNormalized(),
											   (corners[4] - corners[0]).ToNormalized() };
            if( DoLog )
            {
                for( int i = 0; i < 3; i++ )
                {
                    Log( string.Format( " axis[{0}] {1}", i, axes[ i ] ) );
                }
            }
            // Now, is it an obb or an aabb?  Figure out if the axes
            // point in the same direction as the basis vectors, and
            // if so, the order of the axes
            int[] mapping = new int[ 3 ] { -1, -1, -1 };
            int foundMapping = 0;
            for( int i = 0; i < 3; i++ )
            {
                for( int j = 0; j < 3; j++ )
                {
                    if( axes[ i ].Cross( Primitives.UnitBasisVectors[ j ] ).Length < geometryEpsilon )
                    {
                        mapping[ i ] = j;
                        foundMapping++;
                        break;
                    }
                }
            }
            CollisionShape shape;
            if( foundMapping == 3 )
            {
                // It's an AABB, so build the min and max vectors, in
                // the order that matches the unit basis vector order
                Vector3 min = Vector3.Zero;
                Vector3 max = Vector3.Zero;
                for( int i = 0; i < 3; i++ )
                {
                    float e = extents[ i ];
                    int j = mapping[ i ];
                    min[ j ] = center[ j ] - e;
                    max[ j ] = center[ j ] + e;
                }
                shape = new CollisionAABB( min, max );
            }
            else
            {
                Vector3 ruleTest = axes[ 0 ].Cross( axes[ 1 ] );
                if( axes[ 2 ].Dot( ruleTest ) < 0 )
                    axes[ 2 ] = -1 * axes[ 2 ];
                // Return the OBB
                shape = new CollisionOBB( center, axes, extents );
            }
            if( DoLog )
                Log( string.Format( "Extraction result: {0}", shape ) );
            return shape;
        }

        // Take a different approach, based on an idea of Robin's:
        // Find the farthest point pair, and use that to identify the
        // triangles with one of the points as a vertex.  Then take
        // the normals of the triangles, adjust to object the
        // right-hand rule, and they are the axes.  Compute the center
        // from the average of the farthest points, and extents by
        // dotting farthest - center with the axes.
        private static CollisionShape ExtractBox( SubMesh subMesh )
        {
            if( DoLog )
            {
                Log( "" );
                Log( string.Format( "Extracting box for submesh {0}", subMesh.Name ) );
            }
            MeshTriangle[] triangles = ExtractSubmeshTriangles( subMesh );
            int count = triangles.Length;
            // Find the two farthest vertices across all triangle
            Vector3[] farthestPoints = new Vector3[ 2 ] { Vector3.Zero, Vector3.Zero };
            float farthestDistanceSquared = 0.0f;
            for( int i = 0; i < count; i++ )
            {
                MeshTriangle t1 = triangles[ i ];
                for( int j = 0; j < 3; j++ )
                {
                    Vector3 p1 = t1.vertPos[ j ];
                    for( int r = i; r < count; r++ )
                    {
                        MeshTriangle t2 = triangles[ r ];
                        for( int s = 0; s < 3; s++ )
                        {
                            Vector3 p2 = t2.vertPos[ s ];
                            Vector3 diff = (p1 - p2);
                            float d = diff.LengthSquared;
                            // 							if (DoLog)
                            // 								Log(string.Format(" TriVert {0} {1} {2} / {3} {4} {5} dist {6}", i, j, p1, r, s, p2, d));
                            if( d > farthestDistanceSquared )
                            {
                                if( DoLog )
                                    Log( string.Format( " Largest! TriVert {0} {1} {2} / {3} {4} {5} dist {6}",
                                                      i, j, p1, r, s, p2, d ) );
                                farthestDistanceSquared = d;
                                farthestPoints[ 0 ] = p1;
                                farthestPoints[ 1 ] = p2;
                            }
                        }
                    }
                }
            }
            // The center is the average of the farthest points
            Vector3 center = (farthestPoints[ 0 ] + farthestPoints[ 1 ]) * 0.5f;
            if( DoLog )
            {
                Log( string.Format( "The farthest points are {0} and {1}",
                                  farthestPoints[ 0 ], farthestPoints[ 1 ] ) );
                Log( string.Format( "The center is {0}", center ) );
            }
            // Now find the three triangles that have the
            // farthestPoints[0] as a vertex
            Vector3[] axes = new Vector3[] { Vector3.Zero, Vector3.Zero, Vector3.Zero };
            int foundCount = 0;
            for( int i = 0; i < count; i++ )
            {
                MeshTriangle t = triangles[ i ];
                for( int j = 0; j < 3; j++ )
                {
                    Vector3 p = t.vertPos[ j ];
                    if( (p - farthestPoints[ 0 ]).LengthSquared < geometryEpsilon )
                    {
                        Vector3 side1 = t.vertPos[ 1 ] - t.vertPos[ 0 ];
                        Vector3 side2 = t.vertPos[ 2 ] - t.vertPos[ 1 ];
                        Vector3 axis = side1.Cross( side2 ).ToNormalized();
                        // Ignore this triangle if his normal matches one we already have
                        bool ignore = false;
                        for( int k = 0; k < foundCount; k++ )
                        {
                            if( Math.Abs( axis.Cross( axes[ k ] ).LengthSquared ) < geometryEpsilon )
                            {
                                ignore = true;
                                break;
                            }
                        }
                        if( !ignore )
                        {
                            Debug.Assert( foundCount < 3, "Found more than three triangles with distinct normals and vertex = farthest point" );
                            axes[ foundCount ] = axis;
                            foundCount++;
                        }
                    }
                }
            }
            // Put the axes in coordinate order
            for( int i = 0; i < 3; i++ )
            {
                float largest = float.MinValue;
                int largestIndex = i;
                for( int j = 0; j < 3; j++ )
                {
                    float v = Math.Abs( axes[ j ][ i ] );
                    if( v > largest )
                    {
                        largestIndex = j;
                        largest = v;
                    }
                }
                if( largestIndex != i )
                {
                    Vector3 t = axes[ i ];
                    axes[ i ] = axes[ largestIndex ];
                    axes[ largestIndex ] = t;
                }
                if( axes[ i ][ i ] < 0 )
                    axes[ i ] = -axes[ i ];
            }

            // Put the axes in right-hand-rule order
            if( axes[ 0 ].Cross( axes[ 1 ] ).Dot( axes[ 2 ] ) < 0 )
            {
                axes[ 2 ] = -axes[ 2 ];
            }
            Debug.Assert( axes[ 0 ].Cross( axes[ 1 ] ).Dot( axes[ 2 ] ) > 0,
                         "After swapping axes, still don't obey right-hand rule" );
            // The extents are just the abs of the dot products of
            // farthest point minus the center with the axes
            Vector3 f = farthestPoints[ 0 ] - center;
            Vector3 extents = new Vector3( Math.Abs( f.Dot( axes[ 0 ] ) ),
                                          Math.Abs( f.Dot( axes[ 1 ] ) ),
                                          Math.Abs( f.Dot( axes[ 2 ] ) ) );
            if( DoLog )
            {
                for( int i = 0; i < 3; i++ )
                {
                    Log( string.Format( " axis[{0}] {1}, extent[{2}] {3}", i, axes[ i ], i, extents[ i ] ) );
                }
                int[] sign = new int[ 3 ] { 0, 0, 0 };
                for( int i = -1; i < 2; i += 2 )
                {
                    sign[ 0 ] = i;
                    for( int j = -1; j < 2; j += 2 )
                    {
                        sign[ 1 ] = j;
                        for( int k = -1; k < 2; k += 2 )
                        {
                            sign[ 2 ] = k;
                            Vector3 corner = center;
                            for( int a = 0; a < 3; a++ )
                                corner += axes[ a ] * extents[ a ] * sign[ a ];
                            Log( string.Format( " corner[{0},{1},{2}] = {3}", i, j, k, corner ) );
                        }
                    }
                }
            }
            // Now, is it an obb or an aabb?  Figure out if the axes
            // point in the same direction as the basis vectors, and
            // if so, the order of the axes
            int[] mapping = new int[ 3 ] { -1, -1, -1 };
            int foundMapping = 0;
            for( int i = 0; i < 3; i++ )
            {
                for( int j = 0; j < 3; j++ )
                {
                    if( Math.Abs( axes[ i ].Dot( Primitives.UnitBasisVectors[ j ] ) - 1.0f ) < .0001f )
                    {
                        if( DoLog )
                            Log( string.Format( " foundMapping[{0}], basis vector {1}", i, Primitives.UnitBasisVectors[ j ] ) );
                        mapping[ i ] = j;
                        foundMapping++;
                        break;
                    }
                }
            }
            CollisionShape shape;
            if( foundMapping == 3 )
            {
                // It's an AABB, so build the min and max vectors, in
                // the order that matches the unit basis vector order
                Vector3 min = Vector3.Zero;
                Vector3 max = Vector3.Zero;
                for( int i = 0; i < 3; i++ )
                {
                    float e = extents[ i ];
                    int j = mapping[ i ];
                    min[ j ] = center[ j ] - e;
                    max[ j ] = center[ j ] + e;
                }
                shape = new CollisionAABB( min, max );
            }
            else
                // Return the OBB
                shape = new CollisionOBB( center, axes, extents );
            if( DoLog )
                Log( string.Format( "Extraction result: {0}", shape ) );
            return shape;
        }

        private static CollisionShape ExtractSphere( SubMesh subMesh )
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            Vector3 minVertex = Vector3.Zero;
            Vector3 maxVertex = Vector3.Zero;
            MeshTriangle[] triangles = ExtractSubmeshTriangles( subMesh );
            int count = triangles.Length;
            for( int i = 0; i < count; i++ )
            {
                MeshTriangle t = triangles[ i ];
                for( int j = 0; j < 3; j++ )
                {
                    if( t.vertPos[ j ].x < minX )
                    {
                        minX = t.vertPos[ j ].x;
                        minVertex = t.vertPos[ j ];
                    }
                    if( t.vertPos[ j ].x > maxX )
                    {
                        maxX = t.vertPos[ j ].x;
                        maxVertex = t.vertPos[ j ];
                    }
                }
            }
            return new CollisionSphere( (minVertex + maxVertex) / 2.0f,
                                       (maxVertex - minVertex).Length );
        }

        private static CollisionShape ExtractCapsule( SubMesh subMesh )
        {
            // Find the two triangles that are farthest apart.  The
            // distance between them is the distance betwen
            // bottomCenter and topCenter, plus twice the capRadius.
            // The centers of these two triangles define a line which
            // contains the capsule segment.  Finally, find the
            // triangle whose center is furthest from the centers of
            // those two triangles, and determine the distance from
            // the center of that triangle to the line.  That distance
            // is the capRadius.  Move from the original two triangles
            // toward each other capRadius distance and that
            // determines the bottomCenter and topCenter
            int[] farthest = new int[ 2 ] { -1, -1 };
            float farthestDistanceSquared = 0.0f;
            MeshTriangle[] triangles = ExtractSubmeshTriangles( subMesh );
            int count = triangles.Length;
            for( int i = 0; i < count; i++ )
            {
                MeshTriangle t = triangles[ i ];
                Vector3 c = t.Center();
                for( int j = 0; j < count; j++ )
                {
                    float d = (c - triangles[ j ].Center()).LengthSquared;
                    if( farthestDistanceSquared < d )
                    {
                        farthest[ 0 ] = i;
                        farthest[ 1 ] = j;
                        farthestDistanceSquared = d;
                    }
                }
            }
            Vector3 bottom = triangles[ farthest[ 0 ] ].Center();
            Vector3 top = triangles[ farthest[ 1 ] ].Center();
            float bottomTopDistance = (float) Math.Sqrt( farthestDistanceSquared );
            float capRadiusSquared = 0f;
            for( int i = 0; i < count; i++ )
            {
                MeshTriangle t = triangles[ i ];
                Vector3 c = t.Center();
                float d = Primitives.SqDistPointSegment( bottom, top, c );
                if( capRadiusSquared < d )
                    capRadiusSquared = d;
            }
            float capRadius = (float) Math.Sqrt( capRadiusSquared );
            Vector3 unitBottomTop = (top - bottom) / bottomTopDistance;
            return new CollisionCapsule( bottom + (unitBottomTop * capRadius),
                                        top - (unitBottomTop * capRadius),
                                        capRadius );
        }

        
        // Find the name of a submesh in the Axiom mesh that matches a name
        // encoded within the collisionSubmesh string.  Different dialects of
        // COLLADA seem to encode names a little differently. In particular,
        // 3dsMax sticks some bits in that were not in the name the user created
        // in the source file.  
        //
        // We approach this heuristically, where first we see if we can get a
        // match assuming a Max-ish name; if that fails, the we attempt again
        // using a more regular style; if that still fails, we give up.
        //
        // Returns a target submesh name on success; else returns the raw
        // collisionSubmesh name.
        public static string GetTargetSubmesh( Mesh mesh, string collisionSubmesh )
        {
            string targetMesh = GetTargetSubmeshMaxStyle( mesh, collisionSubmesh );

            if( String.IsNullOrEmpty( targetMesh ) )
            {
                targetMesh = GetTargetSubmeshGeneralStyle( mesh, collisionSubmesh );
            }

            if( String.IsNullOrEmpty( targetMesh ) )
            {
                return collisionSubmesh;
            }
            else
            {
                return targetMesh;
            }
        }

        // Look for a target submesh assuming that the COLLADA exporter did not
        // do any special name mangling.  I've tested this against files emitted
        // from Maya; I expect it will work for DAE files from other sources, too,
        // such as Blender or XSI.  Time will tell...
        private static string GetTargetSubmeshGeneralStyle( Mesh mesh, string collisionSubmesh )
        {
            string targetName = String.Empty;

            CvNameParser parser =  new CvNameParser();

            parser.AnalyzeName( collisionSubmesh );

            if( parser.IsValid )
            {
                // Form the target name as the first submesh name and see if 
                // there is actually matching submesh.
                string candidate = parser.Target + ".0";

                for( int i = 0; i < mesh.SubMeshCount; i++ )
                {
                    if( mesh.GetSubMesh( i ).Name.Equals( candidate ) )
                    {
                        targetName = candidate;
                        break;
                    }
                }
            }

            return targetName;
        }

        // This class parses a submesh name that might be a CV mesh name. We are 
        // looking for names of a special form:
        //
        //      mvcv_[prefix]_<targetName>_[cvIndex].[submeshIndex]
        // 
        // Where [prefix] is one of the CV strings (e.g. "aabb"), [cvIndex] is a
        // user tag of a numeric pair (e.g. "01", or "13"), and [submeshIndex]
        // is the submesh index the conversion applied to the CV source (typically
        // "0"). The <targetName> is what we are trying to get to.
        //
        // BTW, the [cvIndex], by convention a pair of numeric chars, is insignificant.
        // Its purpose is to give you a way to have more that one CV mesh that has the
        // same target name. As far as the parser is concerned, you could just as well
        // use "Fred" and "Joe" for the values.
        class CvNameParser
        {
            public string Name { get { return m_Name; } }
            string m_Name;

            Dictionary<string, string> m_CvPrefixTypes = new Dictionary<string, string>();

            public string Prefix { get; protected set; }
            public string Target { get; protected set; }

            public bool IsCvName
            {
                get { return m_Name.StartsWith( "mvcv_" ); }
            }

            public bool IsValid
            {
                get
                {
                    return IsCvName && !String.IsNullOrEmpty( Prefix );
                }
            }

            public CvNameParser()
            {
                m_CvPrefixTypes.Add( "aabb",    "Axis-Aligned Bounding Box" );
                m_CvPrefixTypes.Add( "obb",     "Oriented Bounding Box" );
                m_CvPrefixTypes.Add( "sphere",  "Sphere" );
                m_CvPrefixTypes.Add( "capsule", "Capsule" );
            }

            public virtual void AnalyzeName( string name )
            {
                m_Name = name;

                string partial = m_Name.Substring( "mvcv_".Length );

                foreach( string prefix in m_CvPrefixTypes.Keys )
                {
                    if( partial.StartsWith( prefix ) )
                    {
                        partial = partial.Substring( prefix.Length + "_".Length );
                        Prefix  = prefix;
                        break;
                    }
                }

                // Trim the submesh index off
                int submeshIndex = partial.LastIndexOf( '.' );

                if( 0 < submeshIndex )
                {
                    partial = partial.Substring( 0, submeshIndex );
                }

                // Trim the cvIndex off
                int cvIndex = partial.LastIndexOf( '_' );

                if( 0 < cvIndex )
                {
                    partial = partial.Substring( 0, cvIndex );
                }

                // What's left should be the target name
                Target = partial;
            }
        }

        // This attempts to parse a CV name and get the name of a matching submesh
        // defined in the Axiom mesh.  This is tailored to ideosyncrasities in how
        // the 3dsMax COLLADA exporter mangles names.  I'm not sure exactly what it 
        // does, but it has something to do with inserting a suffix like '-lib' at
        // or near the end of the CV mesh name.
        // 
        // Return a target submesh name, or String.Empty if either we cannot parse
        // the name satisfactorily, or the target is not found.
        private static string GetTargetSubmeshMaxStyle( Mesh mesh, string collisionSubmesh )
        {
            const string mesh_pattern = "(.*)-(lib|obj|mesh)\\.([0-9]+)";
            const string cv_prefix = "mvcv_(obb|aabb|sphere|capsule)_";

            Regex mvcv_regex = new Regex( cv_prefix + mesh_pattern );
            Match mvcvMatch = mvcv_regex.Match( collisionSubmesh );

            if( !mvcvMatch.Success || mvcvMatch.Groups.Count < 3 )
            {
                if( DoLog )
                {
                    Log( string.Format( "Unexpected collision volume name: {0}", collisionSubmesh ) );
                }
                return String.Empty;
            }

            string mvcv_target = mvcvMatch.Groups[ 2 ].Value;

            for( int i = 0; i < mesh.SubMeshCount; ++i )
            {
                string submeshName = mesh.GetSubMesh( i ).Name;

                // strip off the -obj.0 part
                Regex submesh_regex = new Regex( mesh_pattern );
                Match submeshMatch = submesh_regex.Match( submeshName );

                if( !submeshMatch.Success || submeshMatch.Groups.Count < 2 )
                {
                    continue;
                }

                string shortName = submeshMatch.Groups[ 1 ].Value;
                if( mvcv_target.StartsWith( shortName, StringComparison.CurrentCultureIgnoreCase ) )
                {
                    return submeshName;
                }
            }

            if( DoLog )
            {
                Log( string.Format( "Failed to find target submesh for {0}", collisionSubmesh ) );
            }

            return String.Empty;
        }

        public static void ExtractCollisionShapes( Mesh mesh, string path )
        {
            PhysicsData physicsData = null;
            List<string> deleteEm = new List<string>();
            int count = mesh.SubMeshCount;

            for( int i = 0; i < count; i++ )
            {
                SubMesh subMesh = mesh.GetSubMesh( i );
                CollisionShape shape = null;
                string targetName = null;
                bool cv = String.Compare( subMesh.Name.Substring( 0, 5 ), "mvcv_", false ) == 0;
                bool rg = String.Compare( subMesh.Name.Substring( 0, 5 ), "mvrg_", false ) == 0;
                int firstIndex = 0;
                if( cv )
                    firstIndex = 5;
                else if( rg )
                {
                    string rest = subMesh.Name.Substring( 5 );
                    firstIndex = rest.IndexOf( "_" ) + 1 + 5;
                }
                if( cv || rg )
                {
                    // It's probably a collision volume - - check the
                    // shape type to make sure
                    if( String.Compare( subMesh.Name.Substring( firstIndex, 4 ), "obb_", false ) == 0 )
                    {
                        shape = ExtractBox( subMesh );
                    }
                    else if( String.Compare( subMesh.Name.Substring( firstIndex, 5 ), "aabb_", false ) == 0 )
                    {
                        shape = ExtractBox( subMesh );
                    }
                    else if( String.Compare( subMesh.Name.Substring( firstIndex, 7 ), "sphere_", false ) == 0 )
                    {
                        shape = ExtractSphere( subMesh );
                    }
                    else if( String.Compare( subMesh.Name.Substring( firstIndex, 8 ), "capsule_", false ) == 0 )
                    {
                        shape = ExtractCapsule( subMesh );
                    }
                    if( shape != null )
                        targetName = GetTargetSubmesh( mesh, subMesh.Name );
                }
                if( shape != null )
                {
                    deleteEm.Add( subMesh.Name );
                    if( physicsData == null )
                        physicsData = new PhysicsData();
                    physicsData.AddCollisionShape( targetName, shape );
                }
            }
            for( int i = 0; i < deleteEm.Count; i++ )
            {
                mesh.RemoveSubMesh( deleteEm[ i ] );
            }
            if( physicsData != null )
            {
                PhysicsSerializer serializer = new PhysicsSerializer();
                serializer.ExportPhysics( physicsData, path + ".physics" );
            }

            if( DoLog )
                CloseLog();
        }
    }
}
