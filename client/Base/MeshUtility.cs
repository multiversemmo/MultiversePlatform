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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Axiom.Animating;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Axiom.Input;
using Axiom.Configuration;

#endregion

namespace Multiverse.Base
{
	public class MeshUtility
	{

		public static void GetSubmeshVertexData(out Vector3[] points, VertexData vertexData) {
//				if (subMesh.operationType != RenderMode.TriangleList)
//					continue;
			points = null;
			for (ushort bindIdx = 0; bindIdx < vertexData.vertexDeclaration.ElementCount; ++bindIdx) {
				VertexElement element = vertexData.vertexDeclaration.GetElement(bindIdx);
				HardwareVertexBuffer vBuffer = vertexData.vertexBufferBinding.GetBuffer(bindIdx);
				if (element.Semantic != VertexElementSemantic.Position)
					continue;
				points = new Vector3[vertexData.vertexCount];
				ReadBuffer(vBuffer, vertexData.vertexCount, element.Size, ref points);
				return;
			}
			Debug.Assert(points != null, "Unable to retrieve position vertex data");
		}

		public static void GetSubmeshIndexData(out int[,] indices, IndexData indexData) {
			HardwareIndexBuffer idxBuffer = indexData.indexBuffer;
			IndexType indexType = IndexType.Size16;
			if ((idxBuffer.Size / indexData.indexCount) != 2) {
				Debug.Assert(false, "Unexpected index buffer size");
				indexType = IndexType.Size32;
			}
			Debug.Assert(indexData.indexCount % 3 == 0);
			indices = new int[indexData.indexCount / 3, 3];
			ReadBuffer(idxBuffer, indexData.indexCount, indexType, ref indices);
		}

		private static void ReadBuffer(HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize,
								ref Vector3[] data) {
			IntPtr bufData = vBuffer.Lock(BufferLocking.ReadOnly);

			unsafe {
				float* pFloats = (float*)bufData.ToPointer();
				for (int i = 0; i < vertexCount; ++i)
					for (int j = 0; j < 3; ++j) {
						Debug.Assert(sizeof(float) * (i * 3 + j) < vertexCount * vertexSize,
							"Read off end of vertex buffer");
						data[i][j] = pFloats[i * 3 + j];
					}
			}

			// unlock the buffer
			vBuffer.Unlock();
		}

		private static void ReadBuffer(HardwareIndexBuffer idxBuffer, int maxIndex, IndexType indexType,
						ref int[,] data) {
			IntPtr indices = idxBuffer.Lock(BufferLocking.ReadOnly);

			int faceCount = data.GetLength(0);

			if (indexType == IndexType.Size32) {
				// read the ints from the buffer data
				unsafe {
					int* pInts = (int*)indices.ToPointer();
					for (int i = 0; i < faceCount; ++i)
						for (int j = 0; j < 3; ++j) {
							Debug.Assert(i * 3 + j < maxIndex, "Read off end of index buffer");
							data[i, j] = pInts[i * 3 + j];
						}
				}
			} else {
				// read the shorts from the buffer data
				unsafe {
					short* pShorts = (short*)indices.ToPointer();
					for (int i = 0; i < faceCount; ++i)
						for (int j = 0; j < 3; ++j) {
							Debug.Assert(i * 3 + j < maxIndex, "Read off end of index buffer");
							data[i, j] = pShorts[i * 3 + j];
						}
				}
			}

			// unlock the buffer
			idxBuffer.Unlock();
		}

	}
}
