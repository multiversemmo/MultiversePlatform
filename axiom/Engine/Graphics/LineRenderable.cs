using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Animating;

namespace Axiom.Graphics {
    public class LineRenderable : SimpleRenderable {
        float boundingSphereRadius = 0;
        Skeleton skeleton = null;

        public void SetPoints(List<Vector3> points) {
            SetPoints(points, null, null);
        }

        public void SetPoints(List<Vector3> points, List<ColorEx> colors) {
            SetPoints(points, colors, null);
        }

        public void SetPoints(List<Vector3> points, List<ColorEx> colors, List<VertexBoneAssignment> boneHandles) {
            // Preprocess the list of bone assignments so we get a list of 
            // bone assignments for each vertex.
            List<List<VertexBoneAssignment>> boneAssignments = null;
            if (boneHandles != null) {
                Dictionary<int, List<VertexBoneAssignment>> dict = new Dictionary<int, List<VertexBoneAssignment>>();
                foreach (VertexBoneAssignment vba in boneHandles) {
                    List<VertexBoneAssignment> vbaList;
                    if (!dict.TryGetValue(vba.vertexIndex, out vbaList)) {
                        vbaList = new List<VertexBoneAssignment>();
                        dict[vba.vertexIndex] = vbaList;
                    }
                    vbaList.Add(vba);
                }
                // Construct the list of bone assignments for each vertex
                boneAssignments = new List<List<VertexBoneAssignment>>();
                for (int i = 0; i < points.Count; ++i) {
                    List<VertexBoneAssignment> vbaList;
                    if (!dict.TryGetValue(i, out vbaList))
                        vbaList = new List<VertexBoneAssignment>();
                    if (vbaList.Count > 4) {
                        // only allowed to use 4 bone influences - trim the less important ones
                        vbaList.Sort(new VertexBoneAssignmentWeightComparer());
                        vbaList.RemoveRange(4, vbaList.Count - 4);
                    } else {
                        while (vbaList.Count < 4) {
                            // Pad it out to 4 influences
                            VertexBoneAssignment vba = new VertexBoneAssignment();
                            vba.vertexIndex = i;
                            vba.boneIndex = 0;
                            vba.weight = 0;
                            vbaList.Add(vba);
                        }
                    }
                    boneAssignments.Add(vbaList);
                }
            }
            SetPointsImpl(points, colors, boneAssignments);
        }

        protected void SetPointsImpl(List<Vector3> points, List<ColorEx> colors, List<List<VertexBoneAssignment>> boneAssignments) {
            if (colors != null && points.Count != colors.Count)
                throw new Exception("Invalid parameters to SetPoints.  Point list length does not match colors list length");
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            // set up vertex data
            vertexData = new VertexData();

            // set up vertex declaration
            VertexDeclaration vertexDeclaration = vertexData.vertexDeclaration;
            int currentOffset = 0;

            // always need positions
            vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            currentOffset += VertexElement.GetTypeSize(VertexElementType.Float3);
            int colorOffset = currentOffset / sizeof(float);
            if (colors != null) {
                vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Color, VertexElementSemantic.Diffuse);
                currentOffset += VertexElement.GetTypeSize(VertexElementType.Color);
            }
            int boneIndexOffset = currentOffset / sizeof(float);
            if (boneAssignments != null) {
                vertexDeclaration.AddElement(0, currentOffset, VertexElementType.UByte4, VertexElementSemantic.BlendIndices);
                currentOffset += VertexElement.GetTypeSize(VertexElementType.UByte4);
            }
            int boneWeightOffset = currentOffset / sizeof(float);
            if (boneAssignments != null) {
                vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float4, VertexElementSemantic.BlendWeights);
                currentOffset += VertexElement.GetTypeSize(VertexElementType.Float4);
            }
            int stride = currentOffset / sizeof(float);
            vertexData.vertexCount = points.Count;

            // allocate vertex buffer
            HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(vertexDeclaration.GetVertexSize(0), vertexData.vertexCount, BufferUsage.StaticWriteOnly);

            // set up the binding, one source only
            VertexBufferBinding binding = vertexData.vertexBufferBinding;
            binding.SetBinding(0, vertexBuffer);

            // Generate vertex data
            unsafe {
                // lock the vertex buffer
                IntPtr data = vertexBuffer.Lock(BufferLocking.Discard);

                byte* pData = (byte*)data.ToPointer();
                float* pFloat = (float*)pData;
                uint* pInt = (uint*)pData;
                for (int i = 0; i < points.Count; ++i) {
                    Vector3 vec = points[i];
                    // assign to geometry
                    pFloat[stride * i] = vec.x;
                    pFloat[stride * i + 1] = vec.y;
                    pFloat[stride * i + 2] = vec.z;
                    if (colors != null) {
                        // assign to diffuse
                        pInt[stride * i + colorOffset] = Root.Instance.RenderSystem.ConvertColor(colors[i]);
                    }
                    if (boneAssignments != null) {
                        for (int j = 0; j < 4; ++j) {
                            pData[4 * (stride * i + boneIndexOffset) + j] = (byte)(boneAssignments[i][j].boneIndex);
                            pFloat[stride * i + boneWeightOffset + j] = boneAssignments[i][j].weight;
                        }
                    }
                }
                // unlock the buffer
                vertexBuffer.Unlock();
            } // unsafe

            for (int i = 0; i < points.Count; ++i) {
                    Vector3 vec = points[i];
                    // Also update the bounding sphere radius
                    float len = vec.Length;
                    if (len > boundingSphereRadius)
                        boundingSphereRadius = len;
                    // Also update the bounding box
                    if (vec.x < min.x)
                        min.x = vec.x;
                    if (vec.y < min.y)
                        min.y = vec.y;
                    if (vec.z < min.z)
                        min.z = vec.z;
                    if (vec.x > max.x)
                        max.x = vec.x;
                    if (vec.y > max.y)
                        max.y = vec.y;
                    if (vec.z > max.z)
                        max.z = vec.z;
            }

            // Set the SimpleRenderable bounding box
            box = new AxisAlignedBox(min, max);
        }

        public Skeleton Skeleton {
            get {
                return skeleton;
            }
            set {
                skeleton = value;
            }
        }

        #region IRenderable methods
        public override void GetRenderOperation(RenderOperation op) {
            // LineLists never use indices
            op.useIndices = false;
            op.indexData = null;

            // set the operation type
            op.operationType = OperationType.LineList;

            // set the vertex data correctly
            op.vertexData = vertexData;
        }

        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public override float GetSquaredViewDepth(Camera camera) {
            // get the parent entitie's parent node
            Node node = this.ParentNode;

            Debug.Assert(node != null);

            return node.GetSquaredViewDepth(camera);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrices"></param>
        public override void GetWorldTransforms(Matrix4[] matrices) {
            if (skeleton == null || skeleton.BoneCount == 0) {
                matrices[0] = worldTransform * parentNode.FullTransform;
                return;
            }
            skeleton.GetBoneMatrices(matrices);
            Matrix4 worldXform = this.ParentNodeFullTransform;
            // Apply our world transform to the points
            for (int i = 0; i < skeleton.BoneCount; ++i)
                matrices[i] = worldXform * matrices[i];
        }

        /// <summary>
        /// 
        /// </summary>
        public override ushort NumWorldTransforms {
            get {
                if (skeleton == null || skeleton.BoneCount == 0)
                    return 1;
                return (ushort)skeleton.BoneCount;
            }
        }
        #endregion

        #region MovableObject properties
        /// <summary>
        ///    Local bounding radius of this entity.
        /// </summary>
        public override float BoundingRadius {
            get {
                float radius = boundingSphereRadius;

                // scale by the largest scale factor
                if (parentNode != null) {
                    Vector3 s = parentNode.DerivedScale;
                    radius *= MathUtil.Max(s.x, MathUtil.Max(s.y, s.z));
                }

                return radius;
            }
        }
        #endregion
    }

}
