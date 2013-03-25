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

using Axiom.Core;
using Axiom.Graphics;
using Axiom.Animating;

/// If a vertex is not referenced by any of the index buffers in the 
/// submeshes (including those from other lods), it can be excluded.
/// SubMesh index type (16 or 32 bit) needs to be recomputed
/// SubMesh index buffer needs the vertex ids rewritten
/// SubMesh bone assignments need their vertex ids rewritten
/// Mesh or SubMesh vertex count needs to be recomputed
/// VertexBufferData needs the vertices rewritten in the new order
/// Mesh bone assignments need their vertex ids rewritten

/// For now, build LOD entries after doing this conversion
/// Edge lists should simply be rebuilt.
/// Shadow volumes should simply be rebuilt
/// For now, don't bother with animation morph keyframes or poses
/// Pose vertex entries may go away, or will need the vertex id rewritten
/// Animation morph keyframes are not supported in this tool - assert
namespace Multiverse.Serialization {
    /// <summary>
    ///   This class is designed to cleanup a mesh.  The main thing I want to 
    ///   do here is to remove any redundant vertices.
    ///   At some later point, I might try to strip out unused bones as well.
    /// </summary>
    public class MeshUtility {
        public class SubMeshData {
            /// <summary>
            ///   Mapping from old vertex id to new vertex id
            /// </summary>
            Dictionary<uint, uint> vertexIdMap = new Dictionary<uint, uint>();
            /// <summary>
            ///   The original vertex data
            /// </summary>
            VertexData vertexData;
            /// <summary>
            ///   The submeshes that use this vertex data.
            /// </summary>
            List<SubMesh> subMeshes = new List<SubMesh>();

            public SubMeshData(VertexData vData) {
                this.vertexData = vData;
            }

            public void AddSubmesh(SubMesh subMesh) {
                subMeshes.Add(subMesh);
                AddSubmeshIndexData(subMesh.IndexData);
                foreach (IndexData indexData in subMesh.LodFaceList)
                    AddSubmeshIndexData(indexData);
            }

            protected void AddSubmeshIndexData(IndexData indexData) {
                HardwareIndexBuffer indexBuffer = indexData.indexBuffer;
                IntPtr indices = indexBuffer.Lock(BufferLocking.ReadOnly);
                unsafe {
                    if (indexBuffer.IndexSize == sizeof(ushort)) {
                        ushort *pIdx = (ushort*)indices.ToPointer();
                        for (int i = 0; i < indexData.indexCount; ++i) {
                            uint index = pIdx[indexData.indexStart + i];
                            if (!vertexIdMap.ContainsKey(index)) {
                                uint nextId = (uint)vertexIdMap.Count;
                                vertexIdMap[index] = nextId;
                            }
                        }
                    } else if (indexBuffer.IndexSize == sizeof(uint)) {
                        uint *pIdx = (uint*)indices.ToPointer();
                        for (int i = 0; i < indexData.indexCount; ++i) {
                            uint index = pIdx[indexData.indexStart + i];
                            if (!vertexIdMap.ContainsKey(index)) {
                                uint nextId = (uint)vertexIdMap.Count;
                                vertexIdMap[index] = nextId;
                            }
                        }
                    } else {
                        Debug.Assert(false, "Invalid index buffer index size");
                    }
                }
                indexBuffer.Unlock();
            }

            public Dictionary<uint, uint> VertexIdMap {
                get {
                    return vertexIdMap;
                }
            }
        }

        SubMeshData sharedSubMeshData = null;
        Dictionary<string, SubMeshData> subMeshDataMap = new Dictionary<string, SubMeshData>();

        public void AddSubmeshData(SubMesh subMesh) {
            SubMeshData smd = null;
            if (!subMesh.useSharedVertices)
                smd = new SubMeshData(subMesh.VertexData);
            else {
                if (sharedSubMeshData == null)
                    sharedSubMeshData = new SubMeshData(subMesh.VertexData);
                smd = sharedSubMeshData;
            }
            smd.AddSubmesh(subMesh);
            subMeshDataMap[subMesh.Name] = smd;
        }

        public static Mesh CopyMesh(Mesh mesh) {
            Mesh newMesh = new Mesh(mesh.Name);
            if (mesh.Skeleton != null)
                newMesh.NotifySkeleton(mesh.Skeleton);
            newMesh.SetVertexBufferPolicy(mesh.VertexBufferUsage, mesh.UseVertexShadowBuffer);
            newMesh.SetIndexBufferPolicy(mesh.IndexBufferUsage, mesh.UseIndexShadowBuffer);
            
            // this sets bounding radius as well
            newMesh.BoundingBox = mesh.BoundingBox;

            MeshUtility meshUtility = new MeshUtility();
            for (int i = 0; i < mesh.SubMeshCount; ++i)
                meshUtility.AddSubmeshData(mesh.GetSubMesh(i));

            // This should be done after we finish with the lod stuff
            newMesh.AutoBuildEdgeLists = true;
            newMesh.BuildEdgeList();

            foreach (AttachmentPoint ap in mesh.AttachmentPoints)
                newMesh.AttachmentPoints.Add(new AttachmentPoint(ap));
            
            for (int i = 0; i < mesh.SubMeshCount; ++i) {
                SubMesh srcSubMesh = mesh.GetSubMesh(i);
                SubMesh dstSubMesh = newMesh.CreateSubMesh(srcSubMesh.Name);
                CopySubMesh(dstSubMesh, srcSubMesh, meshUtility.subMeshDataMap[srcSubMesh.Name]);
            }

            if (mesh.SharedVertexData != null) {
                newMesh.SharedVertexData = new VertexData();
                CopyVertexData(newMesh.SharedVertexData, mesh.SharedVertexData, meshUtility.sharedSubMeshData.VertexIdMap);
                CopyBoneAssignments(newMesh, mesh, meshUtility.sharedSubMeshData.VertexIdMap);
            }
            // newMesh.CompileBoneAssignments();

            return newMesh;
        }

        public static void CopySubMesh(SubMesh dstSubMesh, SubMesh srcSubMesh, SubMeshData subMeshData) {
            dstSubMesh.OperationType = srcSubMesh.OperationType;
            dstSubMesh.useSharedVertices = srcSubMesh.useSharedVertices;
            dstSubMesh.MaterialName = srcSubMesh.MaterialName;
            CopyIndexData(dstSubMesh.IndexData, srcSubMesh.IndexData, subMeshData.VertexIdMap);
            if (!srcSubMesh.useSharedVertices) {
                dstSubMesh.useSharedVertices = false;
                dstSubMesh.vertexData = new VertexData();
                CopyVertexData(dstSubMesh.VertexData, srcSubMesh.VertexData, subMeshData.VertexIdMap);
                CopyBoneAssignments(dstSubMesh, srcSubMesh, subMeshData.VertexIdMap);
            }
            Debug.Assert(srcSubMesh.VertexAnimationType == VertexAnimationType.None);
        }

        public static void CopyIndexData(IndexData dst, IndexData src, Dictionary<uint, uint> vertexIdMap) {
            dst.indexStart = src.indexStart;
            dst.indexCount = src.indexCount;
            IndexType iType = IndexType.Size16;
            if (vertexIdMap.Count > ushort.MaxValue)
                iType = IndexType.Size32;
            HardwareIndexBuffer srcBuf = src.indexBuffer;
            HardwareIndexBuffer dstBuf =
                HardwareBufferManager.Instance.CreateIndexBuffer(iType, dst.indexCount, srcBuf.Usage);
            // TODO: copy the data
            IntPtr srcData = srcBuf.Lock(BufferLocking.ReadOnly);
            IntPtr dstData = dstBuf.Lock(BufferLocking.Discard);
            unsafe {
                if (srcBuf.IndexSize == 2 && dstBuf.IndexSize == 2) {
                    ushort* srcPtr = (ushort *)srcData.ToPointer();
                    ushort* dstPtr = (ushort *)dstData.ToPointer();
                    for (int i = 0; i < srcBuf.IndexCount; ++i)
                        dstPtr[i] = (ushort)vertexIdMap[srcPtr[i]];
                } else if (srcBuf.IndexSize == 4 && dstBuf.IndexSize == 2) {
                    uint* srcPtr = (uint *)srcData.ToPointer();
                    ushort* dstPtr = (ushort *)dstData.ToPointer();
                    for (int i = 0; i < srcBuf.IndexCount; ++i)
                        dstPtr[i] = (ushort)vertexIdMap[srcPtr[i]];
                } else if (srcBuf.IndexSize == 4 && dstBuf.IndexSize == 4) {
                    uint* srcPtr = (uint *)srcData.ToPointer();
                    uint* dstPtr = (uint *)dstData.ToPointer();
                    for (int i = 0; i < srcBuf.IndexCount; ++i)
                        dstPtr[i] = vertexIdMap[srcPtr[i]];
                } else {
                    throw new NotImplementedException();
                }
            }
            dstBuf.Unlock();
            srcBuf.Unlock();
            dst.indexBuffer = dstBuf;
        }

        public static void CopyVertexData(VertexData dst, VertexData src, Dictionary<uint, uint> vertexIdMap) {
            dst.vertexStart = 0;
            dst.vertexCount = vertexIdMap.Count;

            // Copy vertex buffers in turn
            Dictionary<ushort, HardwareVertexBuffer> bindings = src.vertexBufferBinding.Bindings;

            foreach (ushort source in bindings.Keys) {
                HardwareVertexBuffer srcBuf = bindings[source];
                // Create our new, more limited, buffer
                HardwareVertexBuffer dstBuf =
                    HardwareBufferManager.Instance.CreateVertexBuffer(
                                srcBuf.VertexSize, dst.vertexCount, srcBuf.Usage,
                                srcBuf.HasShadowBuffer);
                // Copy elements
                for (int i = 0; i < src.vertexDeclaration.ElementCount; i++) {
                    VertexElement element = src.vertexDeclaration.GetElement(i);
                    dst.vertexDeclaration.AddElement(
                        element.Source,
                        element.Offset,
                        element.Type,
                        element.Semantic,
                        element.Index);
                }

                // write the data to this buffer
                IntPtr srcData = srcBuf.Lock(BufferLocking.ReadOnly);
                IntPtr dstData = dstBuf.Lock(BufferLocking.Discard);
                unsafe {
                    byte* srcPtr = (byte*)srcData.ToPointer();
                    byte* dstPtr = (byte*)dstData.ToPointer();
                    foreach (uint srcVertexId in vertexIdMap.Keys) {
                        uint dstVertexId = vertexIdMap[srcVertexId];
                        int srcOffset = (int)(srcBuf.VertexSize * srcVertexId);
                        int dstOffset = (int)(dstBuf.VertexSize * dstVertexId);
                        for (int i = 0; i < srcBuf.VertexSize; ++i)
                            dstPtr[dstOffset + i] = srcPtr[srcOffset + i];
                    }
                }
                dstBuf.Unlock();
                srcBuf.Unlock();

                dst.vertexBufferBinding.SetBinding(source, dstBuf);
            }
        }

        public static void CopyBoneAssignments(SubMesh dst, SubMesh src, Dictionary<uint, uint> vertexIdMap) {
            foreach (KeyValuePair<uint, uint> vertexMapping in vertexIdMap) {
                if (!src.BoneAssignmentList.ContainsKey((int)vertexMapping.Key))
                    continue;
                List<VertexBoneAssignment> srcVbaList = src.BoneAssignmentList[(int)vertexMapping.Key];
                foreach (VertexBoneAssignment srcVba in srcVbaList) {
                    Debug.Assert(srcVba.vertexIndex == (int)vertexMapping.Key);
                    VertexBoneAssignment dstVba = new VertexBoneAssignment();
                    dstVba.boneIndex = srcVba.boneIndex;
                    dstVba.vertexIndex = (int)vertexMapping.Value;
                    dstVba.weight = srcVba.weight;
                    dst.AddBoneAssignment(dstVba);
                }
            }
        }

        public static void CopyBoneAssignments(Mesh dst, Mesh src, Dictionary<uint, uint> vertexIdMap) {
            foreach (KeyValuePair<uint, uint> vertexMapping in vertexIdMap) {
                List<VertexBoneAssignment> srcVbaList = src.BoneAssignmentList[(int)vertexMapping.Key];
                foreach (VertexBoneAssignment srcVba in srcVbaList) {
                    Debug.Assert(srcVba.vertexIndex == (int)vertexMapping.Key);
                    VertexBoneAssignment dstVba = new VertexBoneAssignment();
                    dstVba.boneIndex = srcVba.boneIndex;
                    dstVba.vertexIndex = (int)vertexMapping.Value;
                    dstVba.weight = srcVba.weight;
                    dst.AddBoneAssignment(dstVba);
                }
            }
        }
    }



}
