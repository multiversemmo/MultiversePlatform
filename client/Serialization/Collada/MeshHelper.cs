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


namespace Multiverse.Serialization
{
    public class MeshHelper
    {

        public static void FillBuffer( HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize, float[ , ] data )
        {
            int count = data.GetLength( 1 );
            IntPtr bufData = vBuffer.Lock( BufferLocking.Discard );

            unsafe
            {
                float* pFloats = (float*) bufData.ToPointer();
                for( int i = 0; i < vertexCount; ++i )
                    for( int j = 0; j < count; ++j )
                    {
                        Debug.Assert( sizeof( float ) * (i * count + j) < vertexCount * vertexSize,
                            "Wrote off end of index buffer" );
                        pFloats[ i * count + j ] = data[ i, j ];
                    }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }



        /// <summary>
        ///   This will create a vertex buffer that will hold tangent and bitangent data.
        ///   The algorithm used here assumes a fairly simple structure, where the normals
        ///   are face normals, instead of vertex normals, and the bitangents do not handle
        ///   reflected uv maps.  Since the actual handling of those situations is quite a
        ///   bit more complex (some vertices will generally need to be duplicated),
        ///   that is best left to the modeling tool or other tools like NVMeshMender.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexData"></param>
        /// <param name="indexData"></param>
        private static void BuildTangentVectors( Mesh mesh, VertexData vertexData, IndexData indexData )
        {
            // FIXME: Add the tangent/binormal vectors to the first buffer stream if there is only one.
            // This will allow us to run on intel cards.  Probably generally appropriate to add them
            // to the first buffer stream anyhow.

            // vertex buffers
            ushort bindIdx = vertexData.vertexBufferBinding.NextIndex;
            vertexData.vertexDeclaration.AddElement( bindIdx, 0, VertexElementType.Float3, VertexElementSemantic.Tangent );
            int offset = vertexData.vertexDeclaration.GetVertexSize( bindIdx );
            vertexData.vertexDeclaration.AddElement( bindIdx, offset, VertexElementType.Float3, VertexElementSemantic.Binormal );
            int vertexSize = vertexData.vertexDeclaration.GetVertexSize( bindIdx );
            int vertexCount = vertexData.vertexCount;
            HardwareVertexBuffer vBuffer =
                HardwareBufferManager.Instance.CreateVertexBuffer( vertexSize,
                    vertexCount, mesh.VertexBufferUsage, mesh.UseVertexShadowBuffer );

            // This is the array I will use for convenient generation of the tangents
            float[ , ] data = new float[ vertexCount, vertexSize / sizeof( float ) ];

            // temp data buffers
            int[] vertIdx = new int[ 3 ];
            Vector3[] vertPos = new Vector3[ 3 ];
            float[] u = new float[ 3 ];
            float[] v = new float[ 3 ];

            // get the first texture element
            VertexElement texElem = vertexData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.TexCoords, 0 );
            VertexElement posElem = vertexData.vertexDeclaration.FindElementBySemantic( VertexElementSemantic.Position );

            if( texElem == null || texElem.Type != VertexElementType.Float2 )
            {
                // TODO: SubMesh names
                throw new AxiomException( "SubMesh '{0}' of Mesh '{1}' has no 2D texture coordinates at the selected set, therefore we cannot calculate tangents.", "<TODO: SubMesh name>", "name" );
            }

            IntPtr indexPtr = indexData.indexBuffer.Lock( BufferLocking.ReadOnly );

            HardwareVertexBuffer texBuffer = vertexData.vertexBufferBinding.GetBuffer( texElem.Source );
            IntPtr texPtr = texBuffer.Lock( BufferLocking.ReadOnly );

            HardwareVertexBuffer posBuffer = null;
            IntPtr posPtr;
            if( texElem.Source == posElem.Source )
            {
                posBuffer = texBuffer;
                posPtr = texPtr;
            }
            else
            {
                posBuffer = vertexData.vertexBufferBinding.GetBuffer( posElem.Source );
                posPtr = posBuffer.Lock( BufferLocking.ReadOnly );
            }

            // loop through all faces to calculate the tangents and normals
            int numFaces = indexData.indexCount / 3;

            int texOffset = texElem.Offset / sizeof( float );
            int texStride = texBuffer.VertexSize / sizeof( float );

            int posOffset = posElem.Offset / sizeof( float );
            int posStride = posBuffer.VertexSize / sizeof( float );

            unsafe
            {
                uint* pIdxInt32 = null;
                ushort* pIdxShort = null;
                if( indexData.indexBuffer.Type == IndexType.Size32 )
                    pIdxInt32 = (uint*) indexPtr.ToPointer();
                else
                    pIdxShort = (ushort*) indexPtr.ToPointer();

                float* p3DTC = (float*) texPtr.ToPointer();
                float* pVPos = (float*) posPtr.ToPointer();

                // loop through all faces to calculate the tangents
                for( int n = 0; n < numFaces; n++ )
                {
                    for( int i = 0; i < 3; i++ )
                    {
                        // get indices of vertices that form a polygon in the position buffer
                        if( indexData.indexBuffer.Type == IndexType.Size32 )
                            vertIdx[ i ] = (int) pIdxInt32[ 3 * n + i ];
                        else
                            vertIdx[ i ] = (int) pIdxShort[ 3 * n + i ];

                        vertPos[ i ].x = pVPos[ vertIdx[ i ] * posStride + posOffset ];
                        vertPos[ i ].y = pVPos[ vertIdx[ i ] * posStride + posOffset + 1 ];
                        vertPos[ i ].z = pVPos[ vertIdx[ i ] * posStride + posOffset + 2 ];
                        u[ i ] = p3DTC[ vertIdx[ i ] * texStride + texOffset ];
                        v[ i ] = p3DTC[ vertIdx[ i ] * texStride + texOffset + 1 ];
                    }

                    // calculate the tangent space vector
                    Vector3 tangent =
                          MathUtil.CalculateTangentSpaceVector(
                          vertPos[ 0 ], vertPos[ 1 ], vertPos[ 2 ],
                          u[ 0 ], v[ 0 ], u[ 1 ], v[ 1 ], u[ 2 ], v[ 2 ] );

                    Vector3 side0 = vertPos[ 0 ] - vertPos[ 1 ];
                    Vector3 side1 = vertPos[ 2 ] - vertPos[ 0 ];
                    // Calculate face normal
                    Vector3 normal = side1.Cross( side0 );
                    normal.Normalize();

                    Vector3 bitangent = normal.Cross( tangent );
                    bitangent.Normalize();

                    for( int i = 0; i < 3; i++ )
                    {
                        data[ vertIdx[ i ], 0 ] = tangent.x;
                        data[ vertIdx[ i ], 1 ] = tangent.y;
                        data[ vertIdx[ i ], 2 ] = tangent.z;
                    }
                    for( int i = 0; i < 3; i++ )
                    {
                        data[ vertIdx[ i ], 3 ] = bitangent.x;
                        data[ vertIdx[ i ], 4 ] = bitangent.y;
                        data[ vertIdx[ i ], 5 ] = bitangent.z;
                    }
                }
            }
            // unlock all used buffers
            texBuffer.Unlock();

            if( posBuffer != texBuffer )
            {
                posBuffer.Unlock();
            }

            indexData.indexBuffer.Unlock();
            FillBuffer( vBuffer, vertexCount, vertexSize, data );

            vertexData.vertexBufferBinding.SetBinding( bindIdx, vBuffer );
        }

        // Create the vertex buffer that we will use later for the Tangents
        public static void BuildTangentVectors( Mesh mesh )
        {
            // I don't have index data in the mesh itself (that data is in the submesh entries)
            // so I can't do this directly.
            //if (mesh.SharedVertexData != null)
            //    BuildTangentVectors(mesh.SharedVertexData, mesh);
            for( int i = 0; i < mesh.SubMeshCount; ++i )
            {
                SubMesh subMesh = mesh.GetSubMesh( i );
                if( subMesh.useSharedVertices )
                {
                    // Since the index data is in the sub mesh, I may need to make multiple
                    // passes through this data to generate the tangent data.
                    System.Diagnostics.Debug.Assert( false, "This isn't supported yet" );
                    continue;
                }
                BuildTangentVectors( mesh, subMesh.vertexData, subMesh.indexData );
            }
        }
    }
}
