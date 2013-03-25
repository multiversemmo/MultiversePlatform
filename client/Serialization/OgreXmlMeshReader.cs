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
	public class XmlVertexData
	{
		public float[,] positionData;
		public float[,] normalData;
		public uint[] diffuseData;
		public uint[] specularData;

		public List<float[,]> multiTexData;

		public ushort bindIdx;

		/// <summary>
		///   Offset into the list of texture buffers for this vertex buffer chunk
		/// </summary>
		public int textureOffset;

		public int vertexCount;

		public XmlVertexData(int vertexCount) {
			this.vertexCount = vertexCount;

			multiTexData = new List<float[,]>();
			
			bindIdx = 0;
			textureOffset = 0;
		}

		public int AddTexture(int dim) {
			multiTexData.Add(new float[vertexCount, dim]);
			return multiTexData.Count - 1;
		}

		public float[,] GetTextureData(int textureIndex) {
			return multiTexData[textureOffset + textureIndex];
		}
	}

	/// <summary>
	/// 	Summary description for OgreMeshReader.
	///		TODO: Add support for level of detail and operation type (other than triangle list)
	/// </summary>
	public class OgreXmlMeshReader
	{
		#region Member variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OgreXmlMeshReader));

		protected Mesh mesh;
		protected bool isSkeletallyAnimated;
		protected int subMeshAutoNumber = 0;
		protected short bindIdx = 0;

		protected Dictionary<int, string> submeshNames;
		protected Dictionary<SubMesh, XmlVertexData> xmlVertexDataDict;

		protected Stream stream;

		#endregion

		#region Constructors

		public OgreXmlMeshReader(Stream data) {
			stream = data;
		}

		#endregion

		#region Methods

		public void Import(Mesh mesh) {
			// store a local reference to the mesh for modification
			this.mesh = mesh;
			this.submeshNames = new Dictionary<int, string>();
			this.xmlVertexDataDict = new Dictionary<SubMesh, XmlVertexData>();

			XmlDocument document = new XmlDocument();
			document.Load(stream);
			foreach (XmlNode childNode in document.ChildNodes) {
				switch (childNode.Name) {
					case "mesh":
						ReadMesh(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadMesh(XmlNode node) {
			// First try to read submesh names
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "submeshnames":
						ReadSubmeshNames(childNode);
						break;
					default:
						break;
				}
			}

			bool hasBoundsInfo = false;
			// Next read submeshes and skeleton link
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "submeshes":
						ReadSubmeshes(childNode);
						break;
					case "skeletonlink":
						ReadSkeletonLink(childNode);
						break;
					case "submeshnames":
						break;
					// Multiverse extensions
					case "boundsinfo":
						hasBoundsInfo = true;
						ReadBoundsInfo(childNode);
						break;
//					case "attachmentpoints":
//						ReadAttachmentPoints(childNode);
//						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}

			if (!hasBoundsInfo)
				ComputeBoundsInfo();
		}

		protected void ReadSubmeshes(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "submesh":
						ReadSubmesh(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadSkeletonLink(XmlNode node) {
			foreach (XmlAttribute attr in node.Attributes) {
				switch (attr.Name) {
					case "name":
						mesh.SkeletonName = attr.Value;
						break;
					default:
						DebugMessage(node, attr);
						break;
				}
			}
		}

		protected void ReadSubmeshNames(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "submeshname":
						ReadSubmeshName(childNode);
						break;
					default:
						DebugMessage(node);
						break;
				}
			}
		}

		protected void ReadSubmesh(XmlNode node) {
			if (!submeshNames.ContainsKey(subMeshAutoNumber))
				submeshNames[subMeshAutoNumber] = "SubMesh" + subMeshAutoNumber;
			string submeshName = submeshNames[subMeshAutoNumber];
			subMeshAutoNumber++;
			ReadSubmesh(node, submeshName);
		}

		protected void ReadSubmesh(XmlNode node, string subMeshName) {
			SubMesh subMesh = mesh.CreateSubMesh(subMeshName);

			// does this use 32 bit index buffer
			IndexType indexType = IndexType.Size16;
			subMesh.useSharedVertices = false;

			foreach (XmlAttribute attr in node.Attributes) {
				switch (attr.Name) {
					case "material":
						subMesh.MaterialName = attr.Value;
						break;
					case "usesharedvertices":
						// use shared vertices?
						if (attr.Value == "true")
							subMesh.useSharedVertices = true;
						break;
					case "use32bitindexes":
						if (attr.Value == "true")
							indexType = IndexType.Size32;
						break;
					case "operationtype":
						if (attr.Value != "triangle_list")
							throw new Exception("Unsupported operation type: " + attr.Value);
						break;
					default:
						DebugMessage(node, attr);
						break;
				}
			}
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "faces":
						ReadFaces(childNode, subMesh, indexType);
						break;
					case "geometry":
						ReadGeometry(childNode, subMesh);
						break;
					case "boneassignments":
						ReadBoneAssignments(childNode, subMesh);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadSubmeshName(XmlNode node) {
			int index = int.Parse(node.Attributes["index"].Value);
			submeshNames[index] = node.Attributes["name"].Value;
		}

		protected void ReadFaces(XmlNode node, SubMesh subMesh, IndexType indexType) {
            uint faceCount = uint.Parse(node.Attributes["count"].Value);

			int faceIndex = 0;
			int[,] data = new int[faceCount, 3];
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "face":
						ReadFace(childNode, data, faceIndex++);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}

			int count = data.GetLength(1);
			subMesh.indexData.indexStart = 0;
			subMesh.indexData.indexCount = data.GetLength(0) * data.GetLength(1);

			HardwareIndexBuffer idxBuffer = null;

			// create the index buffer
			idxBuffer =
				HardwareBufferManager.Instance.
				CreateIndexBuffer(
				indexType,
				subMesh.indexData.indexCount,
				mesh.IndexBufferUsage,
				mesh.UseIndexShadowBuffer);

			IntPtr indices = idxBuffer.Lock(BufferLocking.Discard);

			if (indexType == IndexType.Size32) {
				// read the ints into the buffer data
				unsafe {
					int* pInts = (int*)indices.ToPointer();
					for (int i = 0; i < faceCount; ++i)
						for (int j = 0; j < count; ++j) {
							Debug.Assert(i * count + j < subMesh.indexData.indexCount, "Wrote off end of index buffer");
							pInts[i * count + j] = data[i, j];
						}
				}
			} else {
				// read the shorts into the buffer data
				unsafe {
					short* pShorts = (short*)indices.ToPointer();
					for (int i = 0; i < faceCount; ++i)
						for (int j = 0; j < count; ++j) {
							Debug.Assert(i * count + j < subMesh.indexData.indexCount, "Wrote off end of index buffer");
							pShorts[i * count + j] = (short)data[i, j];
						}
				}
			}
			// unlock the buffer to commit
			idxBuffer.Unlock();

			// save the index buffer
			subMesh.indexData.indexBuffer = idxBuffer;
        }

		protected void ReadGeometry(XmlNode node, SubMesh subMesh) {
			if (subMesh.useSharedVertices)
				throw new Exception("I don't support shared vertices");
			
			VertexData vertexData = new VertexData();
			subMesh.vertexData = vertexData;

			vertexData.vertexStart = 0;
			vertexData.vertexCount = int.Parse(node.Attributes["vertexcount"].Value);

			XmlVertexData xmlVertexData = new XmlVertexData(vertexData.vertexCount);

			// Read in the various vertex buffers for this geometry, and 
			// consolidate them into one vertex buffer.
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "vertexbuffer":
						ReadVertexBuffer(childNode, subMesh.vertexData, xmlVertexData);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}

			xmlVertexDataDict[subMesh] = xmlVertexData;
		}

		protected void ReadBoneAssignments(XmlNode node, SubMesh subMesh) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "vertexboneassignment":
						ReadVertexBoneAssigment(childNode, subMesh);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadFace(XmlNode node, int[,] buffer, int faceIndex) {
			buffer[faceIndex, 0] = int.Parse(node.Attributes["v1"].Value);
			buffer[faceIndex, 1] = int.Parse(node.Attributes["v2"].Value);
			buffer[faceIndex, 2] = int.Parse(node.Attributes["v3"].Value);
		}

		protected void ReadVertexBuffer(XmlNode node, VertexData vertexData, XmlVertexData xmlVertexData) {
			bool positions = false;
			bool normals = false;
			bool colours_diffuse = false;
			bool colours_specular = false;
			int texture_coords = 0;

			foreach (XmlAttribute attr in node.Attributes) {
				switch (attr.Name) {
					case "positions":
						if (attr.Value == "true") {
							positions = true;
							xmlVertexData.positionData = new float[xmlVertexData.vertexCount, 3];
						}
						break;
					case "normals":
						if (attr.Value == "true") {
							normals = true;
							xmlVertexData.normalData = new float[xmlVertexData.vertexCount, 3];
						}
						break;
					case "colours_diffuse":
						if (attr.Value == "true") {
							colours_diffuse = true;
							xmlVertexData.diffuseData = new uint[xmlVertexData.vertexCount];
						}
						break;
					case "colours_specular":
						if (attr.Value == "true") {
							colours_specular = true;
							xmlVertexData.specularData = new uint[xmlVertexData.vertexCount];
						}
						break;
					case "texture_coords":
						texture_coords = int.Parse(attr.Value);
						break;
					case "texture_coord_dimensions_0":
					case "texture_coord_dimensions_1":
					case "texture_coord_dimensions_2":
					case "texture_coord_dimensions_3":
					case "texture_coord_dimensions_4":
					case "texture_coord_dimensions_5":
					case "texture_coord_dimensions_6":
					case "texture_coord_dimensions_7":
						break;
					default:
						DebugMessage(node, attr);
						break;
				}
			}

			for (int i = 0; i < texture_coords; ++i) {
				string key = string.Format("texture_coord_dimensions_{0}", i);
				XmlNode attrNode = node.Attributes.GetNamedItem(key);
				if (attrNode != null)
					xmlVertexData.AddTexture(int.Parse(attrNode.Value));
				else
					xmlVertexData.AddTexture(2);
			}

			int vertexIndex = 0;
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "vertex":
						ReadVertex(childNode, xmlVertexData, vertexIndex++);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}

			if (positions)
				AllocateBuffer(vertexData, VertexElementType.Float3,
							   VertexElementSemantic.Position, xmlVertexData.bindIdx++,
							   0, xmlVertexData.positionData);
			if (normals)
				AllocateBuffer(vertexData, VertexElementType.Float3,
							   VertexElementSemantic.Normal, xmlVertexData.bindIdx++,
							   0, xmlVertexData.normalData);
			if (colours_diffuse)
				AllocateBuffer(vertexData, VertexElementType.Color,
							   VertexElementSemantic.Diffuse, xmlVertexData.bindIdx++,
							   0, xmlVertexData.diffuseData);
			if (colours_specular)
				AllocateBuffer(vertexData, VertexElementType.Color,
							   VertexElementSemantic.Specular, xmlVertexData.bindIdx++,
							   0, xmlVertexData.specularData);
			for (int i = 0; i < texture_coords; ++i) {
				int dim = xmlVertexData.GetTextureData(i).GetLength(1);
				AllocateBuffer(vertexData,
							   VertexElement.MultiplyTypeCount(VertexElementType.Float1, dim),
							   VertexElementSemantic.TexCoords, xmlVertexData.bindIdx++,
							   i, xmlVertexData.GetTextureData(i));
			}

			// We have read the textures for this vertex buffer node.
			xmlVertexData.textureOffset += texture_coords;
		}

		private void AllocateBuffer(VertexData vertexData, VertexElementType type,
									VertexElementSemantic semantic,
									ushort bindIdx, int index, float[,] data) {
			// vertex buffers
			vertexData.vertexDeclaration.AddElement(bindIdx, 0, type, semantic, index);
			int vertexSize = vertexData.vertexDeclaration.GetVertexSize(bindIdx);
			int vertexCount = vertexData.vertexCount;
			HardwareVertexBuffer vBuffer = 
				HardwareBufferManager.Instance.CreateVertexBuffer(vertexSize,
					vertexCount, mesh.VertexBufferUsage, mesh.UseVertexShadowBuffer);

			FillBuffer(vBuffer, vertexCount, vertexSize, data);

			// bind the position data
			vertexData.vertexBufferBinding.SetBinding(bindIdx, vBuffer);
		}

		private void FillBuffer(HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize, float[,] data) {
			int count = data.GetLength(1);
			IntPtr bufData = vBuffer.Lock(BufferLocking.Discard);

			unsafe {
				float* pFloats = (float*)bufData.ToPointer();
				for (int i = 0; i <  vertexCount; ++i)
					for (int j = 0; j < count; ++j) {
						Debug.Assert(sizeof(float) * (i * count + j) < vertexCount * vertexSize,
							"Wrote off end of index buffer");
						pFloats[i * count + j] = data[i, j];
					}
			}

			// unlock the buffer
			vBuffer.Unlock();
		}

		private void AllocateBuffer(VertexData vertexData, VertexElementType type,
					VertexElementSemantic semantic,
					ushort bindIdx, int index, uint[] data) {
			// vertex buffers
			vertexData.vertexDeclaration.AddElement(bindIdx, 0, type, semantic, index);
			int vertexSize = vertexData.vertexDeclaration.GetVertexSize(bindIdx);
			int vertexCount = vertexData.vertexCount;
			HardwareVertexBuffer vBuffer =
				HardwareBufferManager.Instance.CreateVertexBuffer(vertexSize,
					vertexCount, mesh.VertexBufferUsage, mesh.UseVertexShadowBuffer);

			FillBuffer(vBuffer, vertexCount, vertexSize, data);

			// bind the position data
			vertexData.vertexBufferBinding.SetBinding(bindIdx, vBuffer);
		}

		private void FillBuffer(HardwareVertexBuffer vBuffer, int vertexCount, int vertexSize, uint[] data) {
			IntPtr bufData = vBuffer.Lock(BufferLocking.Discard);

			unsafe {
				uint* pInts = (uint*)bufData.ToPointer();
				for (int i = 0; i < vertexCount; ++i) {
					Debug.Assert(sizeof(int) * i < vertexCount * vertexSize,
						"Wrote off end of index buffer");
					pInts[i] = data[i];
				}
			}

			// unlock the buffer
			vBuffer.Unlock();
		}


		protected void ReadVertexBoneAssigment(XmlNode node, SubMesh subMesh) {
			VertexBoneAssignment assignment = new VertexBoneAssignment();

			// read the data from the file
			assignment.vertexIndex = int.Parse(node.Attributes["vertexindex"].Value);
			assignment.boneIndex = ushort.Parse(node.Attributes["boneindex"].Value); ;
			assignment.weight = float.Parse(node.Attributes["weight"].Value); ;

			// add the assignment to the mesh
			subMesh.AddBoneAssignment(assignment);
		}

		protected void ReadVertex(XmlNode node, XmlVertexData vertexData, int vertexIndex) {
			int textureIndex = 0;
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "position":
						ReadVector(childNode, vertexData.positionData, vertexIndex);
						break;
					case "normal":
						ReadVector(childNode, vertexData.normalData, vertexIndex);
						break;
					case "colour_diffuse":
						ReadColour(childNode, vertexData.diffuseData, vertexIndex);
						break;
					case "colour_specular":
						ReadColour(childNode, vertexData.specularData, vertexIndex);
						break;
					case "texcoord":
						ReadTexCoord(childNode, vertexData.GetTextureData(textureIndex), vertexIndex);
						textureIndex++;
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadVector(XmlNode node, float[,] buffer, int vertexIndex) {
			buffer[vertexIndex, 0] = float.Parse(node.Attributes["x"].Value);
			buffer[vertexIndex, 1] = float.Parse(node.Attributes["y"].Value);
			buffer[vertexIndex, 2] = float.Parse(node.Attributes["z"].Value);
		}

		protected void ReadColour(XmlNode node, uint[] buffer, int vertexIndex) {
			string value = node.Attributes["value"].Value;
			string[] vals = value.Split(' ');
			Debug.Assert(vals.Length == 4, "Invalid colour value");
			ColorEx color = new ColorEx();
			color.r = float.Parse(vals[0]);
			color.g = float.Parse(vals[1]);
			color.b = float.Parse(vals[2]);
			color.a = float.Parse(vals[3]);
			buffer[vertexIndex] = Root.Instance.RenderSystem.ConvertColor(color);
		}

		protected void ReadTexCoord(XmlNode node, float[,] buffer, int vertexIndex) {
			buffer[vertexIndex, 0] = float.Parse(node.Attributes["u"].Value);
			if (buffer.GetLength(1) > 1)
				buffer[vertexIndex, 1] = float.Parse(node.Attributes["v"].Value);
			if (buffer.GetLength(1) > 2)
				buffer[vertexIndex, 2] = float.Parse(node.Attributes["w"].Value);
		}

		protected void ReadBoundsInfo(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "boundingbox":
						ReadBoundingBox(childNode);
						break;
					case "boundingsphere":
						mesh.BoundingSphereRadius = float.Parse(childNode.Attributes["radius"].Value);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadBoundingBox(XmlNode node) {
			float[,] box = new float[2, 3];
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "min":
						ReadVector(childNode, box, 0);
						break;
					case "max":
						ReadVector(childNode, box, 1);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
			Vector3 min = new Vector3(box[0, 0], box[0, 1], box[0, 2]);
			Vector3 max = new Vector3(box[1, 0], box[1, 1], box[1, 2]);
			mesh.BoundingBox = new AxisAlignedBox(min, max);
		}

		private void ComputeBoundsInfo() {
			AxisAlignedBox boundingBox = new AxisAlignedBox();
			float boundingRadius = 0.0f;
			foreach (XmlVertexData vertData in xmlVertexDataDict.Values)
				ComputeBoundsInfo(boundingBox, ref boundingRadius,
								  vertData.positionData);
			mesh.BoundingBox = boundingBox;
			mesh.BoundingSphereRadius = boundingRadius;
		}

		/// <summary>
		///   Compute the bounds information for the portion of a mesh specified by the points parameter.
		/// </summary>
		/// <param name="boundingBox">This is the bounding box that will 
		///							  probably be expanded to include all 
		///							  of the points.</param>
		/// <param name="boundingRadius">This is the bounding sphere radius
		///								 that will probably be expanded to 
		///								 include all of the points.</param>
		/// <param name="points"></param>
		private void ComputeBoundsInfo(AxisAlignedBox boundingBox, ref float boundingRadius,
									   float[,] points) {
			float boundingRadiusSquared = boundingRadius * boundingRadius;
			Vector3 min = boundingBox.Minimum;
			Vector3 max = boundingBox.Maximum;
			for (int i = 0; i < points.GetLength(0); ++i) {
				min.x = Math.Min(min.x, points[i, 0]);
				min.y = Math.Min(min.y, points[i, 1]);
				min.z = Math.Min(min.z, points[i, 2]);
				max.x = Math.Max(max.x, points[i, 0]);
				max.y = Math.Max(max.y, points[i, 1]);
				max.z = Math.Max(max.z, points[i, 2]);
				float lenSquared = points[i, 0] * points[i, 0] +
								   points[i, 1] * points[i, 1] +
								   points[i, 2] * points[i, 2];
				boundingRadiusSquared = Math.Max(boundingRadiusSquared, lenSquared);
			}
			boundingBox.Minimum = min;
			boundingBox.Maximum = max;
			if (boundingRadiusSquared > boundingRadius * boundingRadius)
				boundingRadius = (float)Math.Sqrt(boundingRadiusSquared);
		}
		

		protected void DebugMessage(XmlNode node) {
            if (node.NodeType == XmlNodeType.Comment)
                return;
            log.InfoFormat("Unhandled node type: {0} with parent of {1}", node.Name, node.ParentNode.Name);
		}

		protected void DebugMessage(XmlNode node, XmlAttribute attr) {
            log.InfoFormat("Unhandled node attribute: {0} with parent node of {1}", attr.Name, node.Name);
        }

		#endregion

		#region Properties

		#endregion
	}
}
