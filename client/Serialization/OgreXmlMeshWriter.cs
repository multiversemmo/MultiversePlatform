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

#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Multiverse.Serialization
{
	/// <summary>
	/// 	Summary description for OgreMeshReader.
	///		TODO: Add support for level of detail and shared geometry
	/// </summary>
	public class OgreXmlMeshWriter
	{
		#region Member variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OgreXmlMeshWriter));

		protected Mesh mesh;
		protected Dictionary<int, string> submeshNames;
		protected Dictionary<SubMesh, XmlVertexData> xmlVertexDataDict;
		protected XmlVertexData sharedXmlVertexData;

		protected AxisAlignedBox boundingBox;
		protected float boundingRadius;

		protected Stream stream;

		protected XmlDocument document;

		protected Matrix4 exportTransform = Matrix4.Identity;

		#endregion

		#region Constructors

		public OgreXmlMeshWriter(Stream data) {
			stream = data;
		}

		#endregion

		#region Methods

		public void Export(Mesh mesh, Matrix4 exportTransform) {
			this.exportTransform = exportTransform;
			Export(mesh);
		}

		public void Export(Mesh mesh) {
			// store a local reference to the mesh for modification
			this.mesh = mesh;
			this.submeshNames = new Dictionary<int, string>();
			this.xmlVertexDataDict = new Dictionary<SubMesh, XmlVertexData>();
			this.document = new XmlDocument();
			XmlNode meshNode = WriteMesh();
			document.AppendChild(meshNode);
			document.Save(stream);
		}

		protected XmlNode WriteMesh() {
			XmlElement node = document.CreateElement("mesh");

			XmlNode childNode;
			
			// Build the dictionary of vertex data for the submeshes.
			for (int i = 0; i < mesh.SubMeshCount; ++i) {
				SubMesh subMesh = mesh.GetSubMesh(i);
				if (subMesh.vertexData == null)
					continue;
				XmlVertexData vertexData = GetXmlVertexData(subMesh.vertexData);
				xmlVertexDataDict[subMesh] = vertexData;
			}

			// TODO: Write the shared xml vertex data
			if (mesh.SharedVertexData != null)
				sharedXmlVertexData = GetXmlVertexData(mesh.SharedVertexData);

			// Write the submesh components
			childNode = WriteSubmeshes();
			node.AppendChild(childNode);

			// Next write skeletonlink
			if (mesh.HasSkeleton) {
				childNode = WriteSkeletonLink();
				node.AppendChild(childNode);
			}

			if (mesh.SharedVertexData != null) {
				childNode = WriteGeometry(mesh);
				node.AppendChild(childNode);
			}

			if (mesh.BoneAssignmentList.Count > 0) {
				childNode = WriteBoneAssignments(mesh);
				node.AppendChild(childNode);
			}
			// TODO:
			// childNode = WriteLevelOfDetail();
			// node.AppendChild(childNode);

			// Next write submesh names
			childNode = WriteSubmeshNames();
			node.AppendChild(childNode);
			
			// Finally write the bounds info
			childNode = WriteBoundsInfo();
			node.AppendChild(childNode);

			return node;
		}

		protected XmlElement WriteSubmeshes() {
			XmlElement node = document.CreateElement("submeshes");

			for (int i = 0; i < mesh.SubMeshCount; ++i) {
				SubMesh subMesh = mesh.GetSubMesh(i);
				XmlElement childNode = WriteSubmesh(subMesh);
				node.AppendChild(childNode);
			}

			return node;
		}

		protected XmlElement WriteSkeletonLink() {
			XmlElement node = document.CreateElement("skeletonlink");
			XmlAttribute attr = document.CreateAttribute("name");
			attr.Value = mesh.SkeletonName;
			node.Attributes.Append(attr);
			return node;
		}

		protected XmlElement WriteSubmeshNames() {
			XmlElement node = document.CreateElement("submeshnames");

			for (int i = 0; i < mesh.SubMeshCount; ++i) {
				SubMesh subMesh = mesh.GetSubMesh(i);
				XmlNode childNode = WriteSubmeshName(subMesh, i);
				node.AppendChild(childNode);
			}

			return node;
		}

		protected XmlElement WriteSubmesh(SubMesh subMesh) {
			XmlElement node = document.CreateElement("submesh");
			XmlAttribute attr;

			attr = document.CreateAttribute("material");
			attr.Value = subMesh.MaterialName;
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("usesharedvertices");
			attr.Value = (subMesh.useSharedVertices) ? "true" : "false";
			node.Attributes.Append(attr);

			VertexData vertexData =
				(subMesh.useSharedVertices) ? mesh.SharedVertexData : subMesh.vertexData;
			IndexType indexType = IndexType.Size16; 
			if (vertexData.vertexCount > short.MaxValue)
				indexType = IndexType.Size32;

			attr = document.CreateAttribute("use32bitindexes");
			attr.Value = (indexType == IndexType.Size32) ? "true" : "false";
			node.Attributes.Append(attr);

			bool isTriList = true;

			// TODO: Support things other than triangle lists
			attr = document.CreateAttribute("operationtype");
			RenderOperation op = new RenderOperation();
			subMesh.GetRenderOperation(op);
			switch (op.operationType) {
				case OperationType.TriangleList:
					attr.Value = "triangle_list";
					break;
				case OperationType.TriangleStrip:
					attr.Value = "triangle_strip";
					isTriList = false;
					break;
				case OperationType.TriangleFan:
					attr.Value = "triangle_fan";
					isTriList = false;
					break;
				default:
					throw new AxiomException("Export of non triangle lists is not supported");
			}
			node.Attributes.Append(attr);

			XmlElement childNode;

			
			childNode = WriteFaces(subMesh, indexType, isTriList);
			node.AppendChild(childNode);

			if (!subMesh.useSharedVertices) {
				childNode = WriteGeometry(subMesh);
				node.AppendChild(childNode);
			}

			if (subMesh.BoneAssignmentList.Count > 0) {
				childNode = WriteBoneAssignments(subMesh);
				node.AppendChild(childNode);
			}

			return node;
		}

		protected XmlElement WriteSubmeshName(SubMesh subMesh, int index) {
			XmlElement node = document.CreateElement("submeshname");
			XmlAttribute attr;

			attr = document.CreateAttribute("index");
			attr.Value = index.ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("name");
			attr.Value = subMesh.Name;
			node.Attributes.Append(attr);

			return node;
		}

		protected XmlElement WriteFaces(SubMesh subMesh, IndexType indexType, bool isTriList) {
			XmlElement node = document.CreateElement("faces");

			// Extract the hardware vertex buffer data into this array
			int[,] data = new int[subMesh.NumFaces, 3];

            HardwareIndexBuffer idxBuffer = subMesh.indexData.indexBuffer;
			IntPtr indices = idxBuffer.Lock(BufferLocking.ReadOnly);

			if (isTriList)
				GetTriangleListIndices(ref data, indices, indexType, subMesh.indexData.indexCount);
			else
				GetTriangleStripOrFanIndices(ref data, indices, indexType, subMesh.indexData.indexCount);

			// unlock the buffer
			idxBuffer.Unlock();

			int faceCount = data.GetLength(0);

			XmlAttribute attr;
			attr = document.CreateAttribute("count");
			attr.Value = faceCount.ToString();
			node.Attributes.Append(attr);

			if (isTriList) {
				for (int i = 0; i < faceCount; ++i) {
					XmlElement childNode = WriteFace(ref data, i);
					node.AppendChild(childNode);
				}
			} else {
				// triangle strip or fan
				if (faceCount != 0) {
					XmlElement childNode = WriteFace(ref data, 0);
					node.AppendChild(childNode);
				}
				for (int i = 1; i < faceCount; ++i) {
					XmlElement childNode = WriteNextFace(ref data, i);
					node.AppendChild(childNode);
				}
			}

			return node;
		}

		private void GetTriangleListIndices(ref int[,] data, IntPtr indices, IndexType indexType, int maxIndex) {
			int faceCount = data.GetLength(0);
			int count = data.GetLength(1);

			if (indexType == IndexType.Size32) {
				// read the ints from the buffer data
				unsafe {
					int* pInts = (int*)indices.ToPointer();
					for (int i = 0; i < faceCount; ++i)
						for (int j = 0; j < count; ++j) {
							Debug.Assert(i * count + j < maxIndex, "Read off end of index buffer");
							data[i, j] = pInts[i * count + j];
						}
				}
			} else {
				// read the shorts from the buffer data
				unsafe {
					short* pShorts = (short*)indices.ToPointer();
					for (int i = 0; i < faceCount; ++i)
						for (int j = 0; j < count; ++j) {
							Debug.Assert(i * count + j < maxIndex, "Read off end of index buffer");
							data[i, j] = pShorts[i * count + j];
						}
				}
			}
		}

		private void GetTriangleStripOrFanIndices(ref int[,] data, IntPtr indices, IndexType indexType, int maxIndex) {
			int faceCount = data.GetLength(0);
			int count = data.GetLength(1);

			if (faceCount == 0)
				return;

			if (indexType == IndexType.Size32) {
				// read the ints from the buffer data
				unsafe {
					int* pInts = (int*)indices.ToPointer();
					for (int j = 0; j < count; ++j) {
						Debug.Assert(j < maxIndex, "Read off end of index buffer");
						data[0, j] = pInts[j];
					}
					for (int i = 1; i < faceCount; ++i)
						for (int j = 0; j < count; ++j) {
							Debug.Assert(2 + i < maxIndex, "Read off end of index buffer");
							data[i, j] = pInts[2 + i];
						}
				}
			} else {
				// read the shorts from the buffer data
				unsafe {
					short* pShorts = (short*)indices.ToPointer();
					for (int j = 0; j < count; ++j) {
						Debug.Assert(j < maxIndex, "Read off end of index buffer");
						data[0, j] = pShorts[j];
					}
					for (int i = 1; i < faceCount; ++i)
						for (int j = 0; j < count; ++j) {
							Debug.Assert(2 + i < maxIndex, "Read off end of index buffer");
							data[i, j] = pShorts[2 + i];
						}
				}
			}
		}


		protected XmlElement WriteGeometry(SubMesh subMesh) {
			XmlElement node = document.CreateElement("geometry");
			XmlAttribute attr;

			attr = document.CreateAttribute("vertexcount");
			attr.Value = subMesh.vertexData.vertexCount.ToString();
			node.Attributes.Append(attr);
			return WriteGeometry(xmlVertexDataDict[subMesh], node);
		}
		protected XmlElement WriteGeometry(Mesh mesh) {
			XmlElement node = document.CreateElement("sharedgeometry");
			XmlAttribute attr;

			attr = document.CreateAttribute("vertexcount");
			attr.Value = mesh.SharedVertexData.vertexCount.ToString();
			node.Attributes.Append(attr);

			return WriteGeometry(sharedXmlVertexData, node);
		}
		protected XmlElement WriteGeometry(XmlVertexData xmlVertData, XmlElement node) {
			// Break up the vertex data into the portions used for geometry, and the 
			// portions used for color and textures.

            XmlElement childNode;

			if (xmlVertData.positionData != null ||
				xmlVertData.normalData != null)
			{
				XmlVertexData xmlGeomVertData = new XmlVertexData(xmlVertData.vertexCount);
				xmlGeomVertData.positionData = xmlVertData.positionData;
				xmlGeomVertData.normalData = xmlVertData.normalData;
				xmlGeomVertData.diffuseData = null;
				xmlGeomVertData.specularData = null;
				xmlGeomVertData.multiTexData = null;

				childNode = WriteVertexBuffer(xmlGeomVertData);
				node.AppendChild(childNode);
			}

			if (xmlVertData.diffuseData != null ||
				xmlVertData.specularData != null ||
				xmlVertData.multiTexData.Count > 0)
			{
				XmlVertexData xmlMatVertData = new XmlVertexData(xmlVertData.vertexCount);
				xmlMatVertData.positionData = null;
				xmlMatVertData.normalData = null;
				xmlMatVertData.diffuseData = xmlVertData.diffuseData;
				xmlMatVertData.specularData = xmlVertData.specularData;
				xmlMatVertData.multiTexData = xmlVertData.multiTexData;

				childNode = WriteVertexBuffer(xmlMatVertData);
				node.AppendChild(childNode);
			}

			return node;
		}

		public static Matrix4 ScaleMatrix(Matrix4 transform, float scale) {
			Matrix4 rv = transform;
			for (int row = 0; row < 3; ++row) {
				for (int col = 0; col < 3; ++col) {
					rv[row, col] *= scale;
				}
			}
			return rv;
		}

		public static float GetScale(Matrix4 transform) {
			Matrix3 tmp =
				new Matrix3(transform.m00, transform.m01, transform.m02,
							transform.m10, transform.m11, transform.m12,
							transform.m20, transform.m21, transform.m22);
			return (float)Math.Pow(tmp.Determinant, 1 / 3.0f);
		}

		private XmlVertexData GetXmlVertexData(VertexData vertexData) {
			XmlVertexData xmlVertexData = new XmlVertexData(vertexData.vertexCount);
            xmlVertexData.positionData = null;
            xmlVertexData.normalData = null;
            xmlVertexData.diffuseData = null;
            xmlVertexData.specularData = null;

			// Normals are transformed by the transpose of the inverse of the spatial transform.
			// Exclude the scale though, since I don't want to mess that up.
			float scale = GetScale(exportTransform);
			Matrix4 tmpTransform = ScaleMatrix(exportTransform, 1 / scale);
			Matrix4 invTrans = tmpTransform.Inverse().Transpose();
            // I'm going to write all the texture buffers to one vertex 
            // buffer, so I can leave textureOffset at zero.
            int textureOffset = 0; 
            for (short bindIdx = 0; bindIdx < vertexData.vertexDeclaration.ElementCount; ++bindIdx) {
				VertexElement element = vertexData.vertexDeclaration.GetElement(bindIdx);
				HardwareVertexBuffer vBuffer = vertexData.vertexBufferBinding.GetBuffer(element.Source);
				int vertexOffset = element.Offset;
				int vertexStride = vBuffer.VertexSize;
                switch (element.Semantic) {
                    case VertexElementSemantic.Position:
                        xmlVertexData.positionData = new float[xmlVertexData.vertexCount, 3];
						ReadBuffer(vBuffer, vertexOffset, vertexStride, xmlVertexData.vertexCount, element.Size, xmlVertexData.positionData, exportTransform);
                        break;
                    case VertexElementSemantic.Normal:
                        xmlVertexData.normalData = new float[xmlVertexData.vertexCount, 3];
						ReadBuffer(vBuffer, vertexOffset, vertexStride, xmlVertexData.vertexCount, element.Size, xmlVertexData.normalData, invTrans);
                        break;
                    case VertexElementSemantic.Diffuse:
                        xmlVertexData.diffuseData = new uint[xmlVertexData.vertexCount];
						ReadBuffer(vBuffer, vertexOffset, vertexStride, xmlVertexData.vertexCount, element.Size, xmlVertexData.diffuseData);
                        break;
                    case VertexElementSemantic.Specular:
                        xmlVertexData.specularData = new uint[xmlVertexData.vertexCount];
						ReadBuffer(vBuffer, vertexOffset, vertexStride, xmlVertexData.vertexCount, element.Size, xmlVertexData.specularData);
                        break;
                    case VertexElementSemantic.TexCoords: {
                            int dim = VertexElement.GetTypeSize(element.Type) /
                                      VertexElement.GetTypeSize(VertexElementType.Float1);
                            float[,] data = new float[xmlVertexData.vertexCount, dim];
                            ReadBuffer(vBuffer, vertexOffset, vertexStride, xmlVertexData.vertexCount, element.Size, data);
                            // pad out the list
                            while (textureOffset + element.Index >= xmlVertexData.multiTexData.Count)
                                xmlVertexData.multiTexData.Add(null);
                            // set this element
                            xmlVertexData.multiTexData[textureOffset + element.Index] = data;
                            textureOffset++;
                        }
                        break;
                    case VertexElementSemantic.Tangent:
                    case VertexElementSemantic.Binormal: {
                            int dim = VertexElement.GetTypeSize(element.Type) /
                                      VertexElement.GetTypeSize(VertexElementType.Float1);
                            float[,] data = new float[xmlVertexData.vertexCount, dim];
                            ReadBuffer(vBuffer, vertexOffset, vertexStride, xmlVertexData.vertexCount, element.Size, data);
                            // pad out the list
                            while (textureOffset + element.Index >= xmlVertexData.multiTexData.Count)
                                xmlVertexData.multiTexData.Add(null);
                            // set this element
                            xmlVertexData.multiTexData[textureOffset + element.Index] = data;
                            textureOffset++;
                        }
                        break;
                    default:
                        log.WarnFormat("Unknown vertex buffer semantic: {0}", element.Semantic);
                        break;
                }
            }
            return xmlVertexData;
        }

		private void ReadBuffer(HardwareVertexBuffer vBuffer, int vertexOffset, int vertexStride, int vertexCount, int vertexSize, float[,] data) {
			int count = data.GetLength(1);
			IntPtr bufData = vBuffer.Lock(BufferLocking.ReadOnly);
			int floatOffset = vertexOffset / sizeof(float);
			int floatStride = vertexStride / sizeof(float);
			Debug.Assert(vertexOffset % sizeof(float) == 0);

			unsafe {
				float* pFloats = (float*)bufData.ToPointer();
				for (int i = 0; i < vertexCount; ++i)
					for (int j = 0; j < count; ++j) {
						int k = i * floatStride + floatOffset + j;
						Debug.Assert(sizeof(float) * k < vertexCount * vertexStride,
							"Read off end of vertex buffer");
						data[i, j] = pFloats[k];
					}
			}

			// unlock the buffer
			vBuffer.Unlock();
		}

		/// <summary>
		///   Hacked old version to add a transform
		/// </summary>
		/// <param name="vBuffer"></param>
		/// <param name="vertexOffset"></param>
		/// <param name="vertexStride"></param>
		/// <param name="vertexCount"></param>
		/// <param name="vertexSize"></param>
		/// <param name="data"></param>
		/// <param name="transform"></param>
        private void ReadBuffer(HardwareVertexBuffer vBuffer, int vertexOffset, int vertexStride, int vertexCount, int vertexSize, float[,] data, Matrix4 transform) {
            int count = data.GetLength(1);
            IntPtr bufData = vBuffer.Lock(BufferLocking.ReadOnly);
			Debug.Assert(count == 3);
			int floatOffset = vertexOffset / sizeof(float);
			int floatStride = vertexStride / sizeof(float);
			Debug.Assert(vertexOffset % sizeof(float) == 0);
			
			unsafe {
                float* pFloats = (float*)bufData.ToPointer();
				for (int i = 0; i < vertexCount; ++i) {
					Vector3 tmpVec = Vector3.Zero;
					for (int j = 0; j < count; ++j) {
						int k = i * floatStride + floatOffset + j;
						Debug.Assert(sizeof(float) * k < vertexCount * vertexStride,
							"Read off end of vertex buffer");
						tmpVec[j] = pFloats[k];
					}
					tmpVec = transform * tmpVec;
					for (int j = 0; j < count; ++j)
						data[i, j] = tmpVec[j];
				}
            }

            // unlock the buffer
            vBuffer.Unlock();
        }

		private void ReadBuffer(HardwareVertexBuffer vBuffer, int vertexOffset, int vertexStride, int vertexCount, int vertexSize, uint[] data) {
            IntPtr bufData = vBuffer.Lock(BufferLocking.ReadOnly);
			int intOffset = vertexOffset / sizeof(int);
			int intStride = vertexStride / sizeof(int);
			Debug.Assert(vertexOffset % sizeof(int) == 0);

            unsafe {
                uint* pInts = (uint*)bufData.ToPointer();
                for (int i = 0; i < vertexCount; ++i) {
                    Debug.Assert(sizeof(int) * i < vertexCount * vertexStride,
						"Read off end of vertex buffer");
					data[i] = pInts[i * intStride + intOffset];
                }
            }

            // unlock the buffer
            vBuffer.Unlock();
        }
        
        protected XmlElement WriteVertexBuffer(XmlVertexData xmlVertData) {
            XmlElement node = document.CreateElement("vertexbuffer");
			XmlAttribute attr;

			if (xmlVertData.positionData != null) {
				attr = document.CreateAttribute("positions");
				attr.Value = "true";
				node.Attributes.Append(attr);
			}

			if (xmlVertData.normalData != null) {
				attr = document.CreateAttribute("normals");
				attr.Value = "true";
				node.Attributes.Append(attr);
			}
			
			if (xmlVertData.diffuseData != null) {
				attr = document.CreateAttribute("colours_diffuse");
				attr.Value = "true";
				node.Attributes.Append(attr);
			}

			if (xmlVertData.specularData != null) {
				attr = document.CreateAttribute("colours_specular");
				attr.Value = "true";
				node.Attributes.Append(attr);
			}

			if (xmlVertData.multiTexData != null) {
				attr = document.CreateAttribute("texture_coords");
				attr.Value = xmlVertData.multiTexData.Count.ToString();
				node.Attributes.Append(attr);

				for (int i = 0; i < xmlVertData.multiTexData.Count; ++i) {
					attr = document.CreateAttribute("texture_coord_dimensions_" + i);
					attr.Value = xmlVertData.multiTexData[i].GetLength(1).ToString();
					node.Attributes.Append(attr);
				}
			}

			// now write the vertex entries;
			for (int i = 0; i < xmlVertData.vertexCount; ++i) {
				XmlElement childNode = WriteVertex(xmlVertData, i);
				node.AppendChild(childNode);
			}

			return node;
		}

		protected XmlElement WriteVertex(XmlVertexData xmlVertData, int vertIndex) {
			XmlElement node = document.CreateElement("vertex");

			if (xmlVertData.positionData != null) {
				XmlElement childNode = document.CreateElement("position");
				WriteVector(childNode, ref xmlVertData.positionData, vertIndex);
				node.AppendChild(childNode);
			}

			if (xmlVertData.normalData != null) {
				XmlElement childNode = document.CreateElement("normal");
				WriteVector(childNode, ref xmlVertData.normalData, vertIndex);
				node.AppendChild(childNode);
			}

			if (xmlVertData.diffuseData != null) {
				XmlElement childNode = document.CreateElement("colour_diffuse");
				WriteColour(childNode, xmlVertData.diffuseData[vertIndex]);
				node.AppendChild(childNode);
			}

			if (xmlVertData.specularData != null) {
				XmlElement childNode = document.CreateElement("colour_specular");
				WriteColour(childNode, xmlVertData.specularData[vertIndex]);
				node.AppendChild(childNode);
			}

			if (xmlVertData.multiTexData != null) {
				for (int texIndex = 0; texIndex < xmlVertData.multiTexData.Count; ++texIndex) {
					XmlElement childNode = document.CreateElement("texcoord");
					WriteTexCoord(childNode, xmlVertData.multiTexData[texIndex], vertIndex);
					node.AppendChild(childNode);
				}
			}

			return node;
		}


		protected XmlElement WriteBoneAssignments(Mesh mesh) {
			return WriteBoneAssignments(mesh.BoneAssignmentList);
		}
		protected XmlElement WriteBoneAssignments(SubMesh subMesh) {
			return WriteBoneAssignments(subMesh.BoneAssignmentList);
		}
		protected XmlElement WriteBoneAssignments(Dictionary<int, List<VertexBoneAssignment>> boneAssignmentList) {
			XmlElement node = document.CreateElement("boneassignments");

            foreach (int i in boneAssignmentList.Keys) {
                List<VertexBoneAssignment> vbaList = boneAssignmentList[i];
                foreach (VertexBoneAssignment vba in vbaList) {
                    XmlElement childNode = WriteVertexBoneAssignment(vba);
                    node.AppendChild(childNode);
                }
            }
			return node;
		}

		protected XmlElement WriteFace(ref int[,] data, int faceIndex) {
			XmlElement node = document.CreateElement("face");
			XmlAttribute attr;

			attr = document.CreateAttribute("v1");
			attr.Value = data[faceIndex, 0].ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("v2");
			attr.Value = data[faceIndex, 1].ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("v3");
			attr.Value = data[faceIndex, 2].ToString();
			node.Attributes.Append(attr);

			return node;
		}


		/// <summary>
		///   Used to write faces other than the first for the triangle_strip 
		///   and triangle_fan render modes.  This just writes the first vertex.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="faceIndex"></param>
		/// <returns></returns>
		protected XmlElement WriteNextFace(ref int[,] data, int faceIndex) {
			XmlElement node = document.CreateElement("face");
			XmlAttribute attr;

			attr = document.CreateAttribute("v1");
			attr.Value = data[faceIndex, 0].ToString();
			node.Attributes.Append(attr);

			return node;
		}

		protected XmlElement WriteVertexBoneAssignment(VertexBoneAssignment vba) {
			XmlElement node = document.CreateElement("vertexboneassignment");
			XmlAttribute attr;

			attr = document.CreateAttribute("vertexindex");
			attr.Value = vba.vertexIndex.ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("boneindex");
			attr.Value = vba.boneIndex.ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("weight");
			attr.Value = vba.weight.ToString();
			node.Attributes.Append(attr);

			return node;
		}

		protected XmlElement WriteVector(XmlElement node, Vector3 vec) {
			XmlAttribute attr;

			attr = document.CreateAttribute("x");
			attr.Value = vec.x.ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("y");
			attr.Value = vec.y.ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("z");
			attr.Value = vec.z.ToString();
			node.Attributes.Append(attr);

			return node;
		}

		protected XmlElement WriteVector(XmlElement node, ref float[,] buffer, int vertIndex) {
			XmlAttribute attr;

			attr = document.CreateAttribute("x");
			attr.Value = buffer[vertIndex, 0].ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("y");
			attr.Value = buffer[vertIndex, 1].ToString();
			node.Attributes.Append(attr);

			attr = document.CreateAttribute("z");
			attr.Value = buffer[vertIndex, 2].ToString();
			node.Attributes.Append(attr);

			return node;
		}

		protected XmlElement WriteColour(XmlElement node, ColorEx color) {
			XmlAttribute attr;

			attr = document.CreateAttribute("value");
			attr.Value = string.Format("{0} {1} {2} {3}", color.r, color.g, color.b, color.a);

			node.Attributes.Append(attr);
			return node;
		}

		protected XmlElement WriteColour(XmlElement node, uint color) {
			ColorEx colorEx = Root.Instance.RenderSystem.ConvertColor(color);
			return WriteColour(node, colorEx);
		}

		protected XmlElement WriteTexCoord(XmlElement node, float[,] buffer, int vertexIndex) {
			XmlAttribute attr;

			if (buffer.GetLength(1) > 0) {
				attr = document.CreateAttribute("u");
				attr.Value = buffer[vertexIndex, 0].ToString();
				node.Attributes.Append(attr);
			}

			if (buffer.GetLength(1) > 1) {
				attr = document.CreateAttribute("v");
				attr.Value = buffer[vertexIndex, 1].ToString();
				node.Attributes.Append(attr);
			}

			if (buffer.GetLength(1) > 2) {
				attr = document.CreateAttribute("w");
				attr.Value = buffer[vertexIndex, 2].ToString();
				node.Attributes.Append(attr);
			}

			return node;
		}

		protected XmlElement WriteBoundsInfo() {
			XmlElement node = document.CreateElement("boundsinfo");
			XmlElement childNode;

			childNode = WriteBoundingBox();
			node.AppendChild(childNode);

			childNode = WriteBoundingSphere();
			node.AppendChild(childNode);

			return node;
		}

		protected XmlElement WriteBoundingBox() {
			XmlElement node = document.CreateElement("boundingbox");

			XmlElement childNode;

			childNode = document.CreateElement("min");
			WriteVector(childNode, mesh.BoundingBox.Minimum);
			node.AppendChild(childNode);

			childNode = document.CreateElement("max");
			WriteVector(childNode, mesh.BoundingBox.Maximum);
			node.AppendChild(childNode);

			return node;
		}

		protected XmlElement WriteBoundingSphere() {
			XmlElement node = document.CreateElement("boundingsphere");
			XmlAttribute attr = document.CreateAttribute("radius");
			attr.Value = mesh.BoundingSphereRadius.ToString();
			node.Attributes.Append(attr);
			return node;
		}




		#endregion

		#region Properties

		#endregion
	}
}
