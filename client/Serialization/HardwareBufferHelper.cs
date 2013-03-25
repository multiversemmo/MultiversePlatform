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

using Multiverse.Serialization.Collada;

namespace Multiverse.Serialization
{
    /// <summary>
    /// This is a collection of methods that help with accessing Axiom
    /// hardware buffers.
    /// </summary>
    public class HardwareBufferHelper
    {
        Mesh m_AxiomMesh;

        readonly log4net.ILog m_Log;

        internal HardwareBufferHelper( Mesh axiomMesh, log4net.ILog log )
        {
            m_AxiomMesh = axiomMesh;
            m_Log = log;
        }

        internal VertexDataEntry ExtractData( VertexElementSemantic semantic,
                                            int vertexCount, InputSource source,
                                            List<PointComponents> indexSets, int accessIndex )
        {
            float[ , ] fdata = null;
            uint[ , ] idata = null;
            switch( semantic )
            {
            case VertexElementSemantic.Diffuse:
                idata = new uint[ vertexCount, 1 ];
                for( int i = 0; i < vertexCount; ++i )
                {
                    PointComponents components = indexSets[ i ];
                    int inputIndex = components[ accessIndex ];
                    ColorEx color = ColorEx.Black;
                    color.r = (float) source.Accessor.GetParam( "R", inputIndex );
                    color.g = (float) source.Accessor.GetParam( "G", inputIndex );
                    color.b = (float) source.Accessor.GetParam( "B", inputIndex );
                    if( source.Accessor.ContainsParam( "A" ) )
                        color.a = (float) source.Accessor.GetParam( "A", inputIndex );
                    idata[ i, 0 ] = Root.Instance.RenderSystem.ConvertColor( color );
                }
                break;
            case VertexElementSemantic.TexCoords:
                fdata = new float[ vertexCount, 2 ];
                for( int i = 0; i < vertexCount; ++i )
                {
                    PointComponents components = indexSets[ i ];
                    int inputIndex = components[ accessIndex ];
                    // S,T or U,V ?
                    if( source.Accessor.ContainsParam( "S" ) )
                    {
                        fdata[ i, 0 ] = (float) source.Accessor.GetParam( "S", inputIndex );
                        fdata[ i, 1 ] = 1.0f - (float) source.Accessor.GetParam( "T", inputIndex );
                    }
                    else if( source.Accessor.ContainsParam( "U" ) )
                    {
                        fdata[ i, 0 ] = (float) source.Accessor.GetParam( "U", inputIndex );
                        fdata[ i, 1 ] = 1.0f - (float) source.Accessor.GetParam( "V", inputIndex );
                    }
                    else
                    {
                        Debug.Assert( false, "Invalid param names" );
                    }
                }
                break;
            case VertexElementSemantic.Position:
            case VertexElementSemantic.Normal:
            case VertexElementSemantic.Tangent:
            case VertexElementSemantic.Binormal:
                fdata = new float[ vertexCount, 3 ];
                for( int i = 0; i < vertexCount; ++i )
                {
                    PointComponents components = indexSets[ i ];
                    int inputIndex = components[ accessIndex ];
                    Vector3 tmp = Vector3.Zero;
                    tmp.x = (float) source.Accessor.GetParam( "X", inputIndex );
                    tmp.y = (float) source.Accessor.GetParam( "Y", inputIndex );
                    tmp.z = (float) source.Accessor.GetParam( "Z", inputIndex );
                    for( int j = 0; j < 3; ++j )
                        fdata[ i, j ] = tmp[ j ];
                }
                break;
            default:
                m_Log.InfoFormat( "Unknown semantic: {0}", semantic );
                return null;
            }
            if( fdata == null && idata == null )
                return null;

            VertexDataEntry vde = new VertexDataEntry();
            vde.semantic = semantic;
            vde.fdata = fdata;
            vde.idata = idata;
            return vde;
        }

        #region Allocation methods

        /// <summary>
        ///   Adds a vertex element to the vertexData and allocates the vertex 
        ///   buffer and populates it with the information in the data array.
        ///   This variant uses the data in the mesh to determine appropriate
        ///   settings for the vertexBufferUsage and useVertexShadowBuffer
        ///   parameters.
        /// </summary>
        /// <param name="vertexData">
        ///   the vertex data object whose vertex declaration and buffer 
        ///   bindings must be modified to include the reference to the new 
        ///   buffer
        /// </param>
        /// <param name="bindIdx">the index that will be used for this buffer</param>
        /// <param name="dataList">the list containing information about each elements being added</param>
        internal void AllocateBuffer( VertexData vertexData, ushort bindIdx, List<VertexDataEntry> dataList )
        {
            AllocateBuffer( vertexData, bindIdx, dataList,
                            m_AxiomMesh.VertexBufferUsage,
                            m_AxiomMesh.UseVertexShadowBuffer );
        }

        /// <summary>
        ///   Adds a vertex element to the vertexData and allocates the vertex 
        ///   buffer and populates it with the information in the data array.
        /// </summary>
        /// <param name="vertexData">
        ///   the vertex data object whose vertex declaration and buffer 
        ///   bindings must be modified to include the reference to the new 
        ///   buffer
        /// </param>
        /// <param name="bindIdx">the index that will be used for this buffer</param>
        /// <param name="dataList">the list of raw data that will be used to populate the buffer</param>
        /// <param name="vertexBufferUsage"></param>
        /// <param name="useVertexShadowBuffer"></param>
        internal static void AllocateBuffer( VertexData vertexData,
                                          ushort bindIdx,
                                          List<VertexDataEntry> dataList,
                                          BufferUsage vertexBufferUsage,
                                          bool useVertexShadowBuffer )
        {
            // vertex buffers
            int offset = 0;
            List<int> offsets = new List<int>();
            foreach( VertexDataEntry vde in dataList )
            {
                VertexElementType type = vde.GetVertexElementType();
                vertexData.vertexDeclaration.AddElement( bindIdx, offset, type, vde.semantic, vde.textureIndex );
                offsets.Add( offset );
                offset += VertexElement.GetTypeSize( type );
            }
            int vertexSize = vertexData.vertexDeclaration.GetVertexSize( bindIdx );
            int vertexCount = vertexData.vertexCount;
            HardwareVertexBuffer vBuffer =
                HardwareBufferManager.Instance.CreateVertexBuffer( vertexSize,
                    vertexCount, vertexBufferUsage, useVertexShadowBuffer );

            for( int i = 0; i < dataList.Count; ++i )
            {
                int vertexOffset = offsets[ i ];
                if( dataList[ i ].fdata != null )
                    FillBuffer( vBuffer, vertexCount, vertexSize, vertexOffset, vertexSize, dataList[ i ].fdata );
                else if( dataList[ i ].idata != null )
                    FillBuffer( vBuffer, vertexCount, vertexSize, vertexOffset, vertexSize, dataList[ i ].idata );
                else
                    throw new Exception( "Invalid data element with no data" );
            }
            // bind the data
            vertexData.vertexBufferBinding.SetBinding( bindIdx, vBuffer );
        }

        /// <summary>
        ///   Adds a vertex element to the vertexData and allocates the vertex 
        ///   buffer and populates it with the information in the data array.
        ///   This variant uses the data in the mesh to determine appropriate
        ///   settings for the vertexBufferUsage and useVertexShadowBuffer
        ///   parameters.
        /// </summary>
        /// <param name="vertexData">
        ///   the vertex data object whose vertex declaration and buffer 
        ///   bindings must be modified to include the reference to the new 
        ///   buffer
        /// </param>
        /// <param name="type">the type of vertex element being added</param>
        /// <param name="semantic">the semantic of the element being added</param>
        /// <param name="bindIdx">the index that will be used for this buffer</param>
        /// <param name="index">the texture index to which this buffer will apply (or 0)</param>
        /// <param name="data">the raw data that will be used to populate the buffer</param>
        internal void AllocateBuffer( VertexData vertexData, VertexElementType type,
                                    VertexElementSemantic semantic,
                                    ushort bindIdx, int index, int[] data )
        {
            AllocateBuffer( vertexData, type, semantic,
                            bindIdx, index, data,
                            m_AxiomMesh.VertexBufferUsage,
                            m_AxiomMesh.UseVertexShadowBuffer );
        }

        /// <summary>
        ///   Adds a vertex element to the vertexData and allocates the vertex 
        ///   buffer and populates it with the information in the data array.
        /// </summary>
        /// <param name="vertexData">
        ///   the vertex data object whose vertex declaration and buffer 
        ///   bindings must be modified to include the reference to the new 
        ///   buffer
        /// </param>
        /// <param name="type">the type of vertex element being added</param>
        /// <param name="semantic">the semantic of the element being added</param>
        /// <param name="bindIdx">the index that will be used for this buffer</param>
        /// <param name="index">the texture index to which this buffer will apply (or 0)</param>
        /// <param name="data">the raw data that will be used to populate the buffer</param>
        /// <param name="vertexBufferUsage"></param>
        /// <param name="useVertexShadowBuffer"></param>
        internal static void AllocateBuffer( VertexData vertexData, VertexElementType type,
                                          VertexElementSemantic semantic,
                                          ushort bindIdx, int index, int[] data,
                                          BufferUsage vertexBufferUsage,
                                          bool useVertexShadowBuffer )
        {
            // vertex buffers
            vertexData.vertexDeclaration.AddElement( bindIdx, 0, type, semantic, index );
            int vertexSize = vertexData.vertexDeclaration.GetVertexSize( bindIdx );
            int vertexCount = vertexData.vertexCount;
            HardwareVertexBuffer vBuffer =
                HardwareBufferManager.Instance.CreateVertexBuffer( vertexSize,
                    vertexCount, vertexBufferUsage, useVertexShadowBuffer );

            FillBuffer( vBuffer, vertexCount, vertexSize, data );

            // bind the data
            vertexData.vertexBufferBinding.SetBinding( bindIdx, vBuffer );
        }
        #endregion Allocation methods

        #region Fill Buffer methods
        /// <summary>
        ///   Fill a vertex buffer with the contents of a two dimensional 
        ///   float array
        /// </summary>
        /// <param name="vBuffer">HardwareVertexBuffer to populate</param>
        /// <param name="vertexCount">the number of vertices</param>
        /// <param name="vertexSize">the size of each vertex</param>
        /// <param name="data">the array of data to put in the buffer</param>
        internal static void FillBuffer( HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize, float[ , ] data )
        {
            FillBuffer( vBuffer, vertexCount, vertexSize, 0, sizeof( float ) * data.GetLength( 1 ), data );
        }

        /// <summary>
        ///   Fill a vertex buffer with the contents of a two dimensional 
        ///   float array
        /// </summary>
        /// <param name="vBuffer">HardwareVertexBuffer to populate</param>
        /// <param name="vertexCount">the number of vertices</param>
        /// <param name="vertexSize">the size of each vertex</param>
        /// <param name="vertexOffset">the offset (in bytes) of this element in the vertex buffer</param>
        /// <param name="vertexStride">the stride (in bytes) between vertices</param>
        /// <param name="data">the array of data to put in the buffer</param>
        internal static void FillBuffer( HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize,
                                       int vertexOffset, int vertexStride, float[ , ] data )
        {
            int count = data.GetLength( 1 );
            IntPtr bufData = vBuffer.Lock( BufferLocking.Discard );

            int floatStride = vertexStride / sizeof( float );
            int floatOffset = vertexOffset / sizeof( float );
            unsafe
            {
                float* pFloats = (float*) bufData.ToPointer();
                for( int i = 0; i < vertexCount; ++i )
                    for( int j = 0; j < count; ++j )
                    {
                        Debug.Assert( sizeof( float ) * (i * floatStride + floatOffset + j) < vertexCount * vertexSize,
                            "Wrote off end of vertex buffer" );
                        pFloats[ i * floatStride + floatOffset + j ] = data[ i, j ];
                    }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }


        /// <summary>
        ///   Fill a vertex buffer with the contents of a two dimensional 
        ///   float array
        /// </summary>
        /// <param name="vBuffer">HardwareVertexBuffer to populate</param>
        /// <param name="vertexCount">the number of vertices</param>
        /// <param name="vertexSize">the size of each vertex</param>
        /// <param name="vertexOffset">the offset (in bytes) of this element in the vertex buffer</param>
        /// <param name="vertexStride">the stride (in bytes) between vertices</param>
        /// <param name="data">the array of data to put in the buffer</param>
        internal static void FillBuffer( HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize,
                                       int vertexOffset, int vertexStride, uint[ , ] data )
        {
            int count = data.GetLength( 1 );
            IntPtr bufData = vBuffer.Lock( BufferLocking.Discard );

            int uintStride = vertexStride / sizeof( uint );
            int uintOffset = vertexOffset / sizeof( uint );
            unsafe
            {
                uint* pUints = (uint*) bufData.ToPointer();
                for( int i = 0; i < vertexCount; ++i )
                    for( int j = 0; j < count; ++j )
                    {
                        Debug.Assert( sizeof( uint ) * (i * uintStride + uintOffset + j) < vertexCount * vertexSize,
                            "Wrote off end of vertex buffer" );
                        pUints[ i * uintStride + uintOffset + j ] = data[ i, j ];
                    }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }


        /// <summary>
        ///   Fill a vertex buffer with the contents of a one dimensional 
        ///   integer array
        /// </summary>
        /// <param name="vBuffer">HardwareVertexBuffer to populate</param>
        /// <param name="vertexCount">the number of vertices</param>
        /// <param name="vertexSize">the size of each vertex</param>
        /// <param name="data">the array of data to put in the buffer</param>
        internal static void FillBuffer( HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize, int[] data )
        {
            IntPtr bufData = vBuffer.Lock( BufferLocking.Discard );

            unsafe
            {
                int* pInts = (int*) bufData.ToPointer();
                for( int i = 0; i < vertexCount; ++i )
                {
                    Debug.Assert( sizeof( int ) * i < vertexCount * vertexSize,
                        "Wrote off end of vertex buffer" );
                    pInts[ i ] = data[ i ];
                }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }

        /// <summary>
        ///   Populate the index buffer with the information in the data array
        /// </summary>
        /// <param name="idxBuffer">HardwareIndexBuffer to populate</param>
        /// <param name="indexCount">the number of indices</param>
        /// <param name="indexType">the type of index (e.g. IndexType.Size16)</param>
        /// <param name="data">the data to fill the buffer</param>
        internal void FillBuffer( HardwareIndexBuffer idxBuffer, int indexCount, IndexType indexType, int[ , ] data )
        {
            int faceCount = data.GetLength( 0 );
            int count = data.GetLength( 1 );

            IntPtr indices = idxBuffer.Lock( BufferLocking.Discard );

            if( indexType == IndexType.Size32 )
            {
                // read the ints into the buffer data
                unsafe
                {
                    int* pInts = (int*) indices.ToPointer();
                    for( int i = 0; i < faceCount; ++i )
                        for( int j = 0; j < count; ++j )
                        {
                            Debug.Assert( i * count + j < indexCount, "Wrote off end of index buffer" );
                            pInts[ i * count + j ] = data[ i, j ];
                        }
                }
            }
            else
            {
                // read the shorts into the buffer data
                unsafe
                {
                    short* pShorts = (short*) indices.ToPointer();
                    for( int i = 0; i < faceCount; ++i )
                        for( int j = 0; j < count; ++j )
                        {
                            Debug.Assert( i * count + j < indexCount, "Wrote off end of index buffer" );
                            pShorts[ i * count + j ] = (short) data[ i, j ];
                        }
                }
            }
            // unlock the buffer to commit
            idxBuffer.Unlock();
        }
        #endregion Fill Buffer methods

        /// <summary>
        ///   Utility method to pull a bunch of floats out of a vertex buffer.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vBuffer"></param>
        /// <param name="elem"></param>
        public static void ReadBuffer( float[ , ] data, HardwareVertexBuffer vBuffer, VertexElement elem )
        {
            int count = data.GetLength( 1 );
            IntPtr bufData = vBuffer.Lock( BufferLocking.ReadOnly );

            Debug.Assert( vBuffer.VertexSize % sizeof( float ) == 0 );
            Debug.Assert( elem.Offset % sizeof( float ) == 0 );
            int vertexCount = vBuffer.VertexCount;
            int vertexSpan = vBuffer.VertexSize / sizeof( float );
            int offset = elem.Offset / sizeof( float );
            unsafe
            {
                float* pFloats = (float*) bufData.ToPointer();
                for( int i = 0; i < vertexCount; ++i )
                {
                    for( int j = 0; j < count; ++j )
                    {
                        Debug.Assert( ((offset + i * vertexSpan + j) * sizeof( float )) < (vertexCount * vBuffer.VertexSize),
                                     "Read off end of vertex buffer" );
                        data[ i, j ] = pFloats[ offset + i * vertexSpan + j ];
                    }
                }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }
#if NOT
        /// <summary>
        ///    Fetch the data from the vBuffer into the data array
        /// </summary>
        /// <remarks>the data field that will be populated by this call should 
        ///          already have the correct dimensions</remarks>
        /// <param name="vBuffer">the vertex buffer with the data</param>
        /// <param name="vertexCount">the number of vertices</param>
        /// <param name="vertexSize">the size of each vertex</param>
        /// <param name="data">the array to fill</param>
        internal void GetBuffer(HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize, float[,] data) {
            int count = data.GetLength(1);
            IntPtr bufData = vBuffer.Lock(BufferLocking.Discard);

            unsafe {
                float* pFloats = (float*)bufData.ToPointer();
                for (int i = 0; i < vertexCount; ++i)
                    for (int j = 0; j < count; ++j) {
                        Debug.Assert(sizeof(float) * (i * count + j) < vertexCount * vertexSize,
                            "Read off end of index buffer");
                        data[i, j] = pFloats[i * count + j];
                    }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }
#endif
    }
}
