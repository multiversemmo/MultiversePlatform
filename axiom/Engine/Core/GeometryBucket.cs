using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using log4net;

using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Collections;

namespace Axiom.Core
{
    ///<summary>
	///    A GeometryBucket is a the lowest level bucket where geometry with 
	///    the same vertex & index format is stored. It also acts as the 
	///    renderable.
    ///</summary>
    public class GeometryBucket : IRenderable, IDisposable
	{
		#region Fields and Properties
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GeometryBucket));

        // Geometry which has been queued up pre-build (not for deallocation)
        protected List<QueuedGeometry> queuedGeometry;
        // Pointer to parent bucket
		protected MaterialBucket parent;
        // String identifying the vertex / index format
		protected string formatString;
        // Vertex information, includes current number of vertices
        // committed to be a part of this bucket
		protected VertexData vertexData;
        // Index information, includes index type which limits the max
        // number of vertices which are allowed in one bucket
		protected IndexData indexData;
        // Size of indexes
		protected IndexType indexType;
        // Maximum vertex indexable
		protected int maxVertexIndex;

		protected Hashtable customParams = new Hashtable();

        public MaterialBucket Parent {
            get { return parent; }
			}
        
        // Get the vertex data for this geometry 
        public VertexData VertexData {
            get { return vertexData; }
		}

        // Get the index data for this geometry 
        public IndexData IndexData {
            get { return indexData; }
			}

        // @copydoc Renderable::getMaterial
        public Material Material {
            get { return parent.Material; }
		}

        public Technique Technique {
            get { return parent.CurrentTechnique; }
			}

        public Quaternion WorldOrientation {
            get { return Quaternion.Identity; }
		}

        public Vector3 WorldPosition {
            get { return parent.Parent.Parent.Center; }
		}

        public List<Light> Lights {
            get { return parent.Parent.Parent.Lights; }
		}

        public bool CastsShadows {
            get { return parent.Parent.Parent.CastShadows; }
		}

		#endregion Fields and Properties

        #region Constructors

        public GeometryBucket(MaterialBucket parent, string formatString, 
                              VertexData vData, IndexData iData) {
            // Clone the structure from the example
            this.parent = parent;
            this.formatString = formatString;
            vertexData = vData.Clone(false);
            indexData = iData.Clone(false);
            vertexData.vertexCount = 0;
            vertexData.vertexStart = 0;
            indexData.indexCount = 0;
            indexData.indexStart = 0;
            indexType = indexData.indexBuffer.Type;
            queuedGeometry = new List<QueuedGeometry>();
            // Derive the max vertices
            if (indexType == IndexType.Size32)
                maxVertexIndex = int.MaxValue;
            else
                maxVertexIndex = ushort.MaxValue;

            // Check to see if we have blend indices / blend weights
            // remove them if so, they can try to blend non-existent bones!
            VertexElement blendIndices =
                vertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.BlendIndices);
            VertexElement blendWeights =
                vertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.BlendWeights);
            if (blendIndices != null && blendWeights != null) {
                Debug.Assert(blendIndices.Source == blendWeights.Source,
                             "Blend indices and weights should be in the same buffer");
                // Get the source
                ushort source = blendIndices.Source;
                Debug.Assert(blendIndices.Size + blendWeights.Size ==
                    vertexData.vertexBufferBinding.GetBuffer(source).VertexSize,
                    "Blend indices and blend buffers should have buffer to themselves!");
                // Unset the buffer
                vertexData.vertexBufferBinding.UnsetBinding(source);
                // Remove the elements
                vertexData.vertexDeclaration.RemoveElement(VertexElementSemantic.BlendIndices);
                vertexData.vertexDeclaration.RemoveElement(VertexElementSemantic.BlendWeights);
			}
		}

        #endregion Constructors

		#region Public Methods

        public float GetSquaredViewDepth(Camera cam) {
            return parent.Parent.SquaredDistance;
		}

        public void GetRenderOperation(RenderOperation op) {
            op.indexData = indexData;
            op.operationType = OperationType.TriangleList;
            //op.srcRenderable = this;
            op.useIndices = true;
            op.vertexData = vertexData;
		}

        public void GetWorldTransforms(Matrix4[] xform) {
            // Should be the identity transform, but lets allow transformation of the
            // nodes the regions are attached to for kicks
            xform[0] = parent.Parent.Parent.ParentNodeFullTransform;
		}


		public bool Assign(QueuedGeometry qgeom) {
			// do we have enough space
			if(vertexData.vertexCount + qgeom.geometry.vertexData.vertexCount > maxVertexIndex)
				return false;

			queuedGeometry.Add(qgeom);
			vertexData.vertexCount += qgeom.geometry.vertexData.vertexCount;
			indexData.indexCount += qgeom.geometry.indexData.indexCount;

			return true;
		}
		
		public void Build(bool stencilShadows, bool logDetails) {
			// Ok, here's where we transfer the vertices and indexes to the shared buffers
			VertexDeclaration dcl = vertexData.vertexDeclaration;
			VertexBufferBinding binds = vertexData.vertexBufferBinding;

			// create index buffer, and lock
            if (logDetails)
                log.InfoFormat("GeometryBucket.Build: Creating index buffer indexType {0} indexData.indexCount {1}",
                    indexType, indexData.indexCount);
			indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(indexType, indexData.indexCount, BufferUsage.StaticWriteOnly);
			IntPtr indexBufferIntPtr = indexData.indexBuffer.Lock(BufferLocking.Discard);

			// create all vertex buffers, and lock
			ushort b;
			ushort posBufferIdx = dcl.FindElementBySemantic(VertexElementSemantic.Position).Source;

            List<List<VertexElement>> bufferElements = new List<List<VertexElement>>();
            unsafe {
                byte *[] destBufferPtrs = new byte*[binds.BindingCount];
                for (b = 0; b < binds.BindingCount; ++b) {
                    int vertexCount = vertexData.vertexCount;
                    if (logDetails)
                        log.InfoFormat("GeometryBucket.Build b {0}, binds.BindingCount {1}, vertexCount {2}, dcl.GetVertexSize(b) {3}",
                            b, binds.BindingCount, vertexCount, dcl.GetVertexSize(b));
                    // Need to double the vertex count for the position buffer
                    // if we're doing stencil shadows
                    if(stencilShadows && b == posBufferIdx) {
                        vertexCount = vertexCount * 2;
                        if(vertexCount > maxVertexIndex) 
                            throw new Exception("Index range exceeded when using stencil shadows, consider " +
                                "reducing your region size or reducing poly count");
                    }
                    HardwareVertexBuffer vbuf = HardwareBufferManager.Instance.CreateVertexBuffer(dcl.GetVertexSize(b), vertexCount, BufferUsage.StaticWriteOnly);
                    binds.SetBinding(b, vbuf);
                    IntPtr pLock = vbuf.Lock(BufferLocking.Discard);
                    destBufferPtrs[b] = (byte *)pLock.ToPointer();
                    // Pre-cache vertex elements per buffer
                    bufferElements.Add(dcl.FindElementBySource(b));
                }

                // iterate over the geometry items
                int indexOffset = 0;
                IEnumerator iter = queuedGeometry.GetEnumerator();
                Vector3 regionCenter = parent.Parent.Parent.Center;
                int *pDestInt = (int *)indexBufferIntPtr.ToPointer();
                ushort *pDestUShort = (ushort *)indexBufferIntPtr.ToPointer();
                foreach (QueuedGeometry geom in queuedGeometry) {
                    // copy indexes across with offset
                    IndexData srcIdxData = geom.geometry.indexData;
                    IntPtr srcIntPtr = srcIdxData.indexBuffer.Lock(BufferLocking.ReadOnly);
                    if(indexType == IndexType.Size32) {
                        int *pSrcInt = (int *)srcIntPtr.ToPointer();
                        for (int i=0; i<srcIdxData.indexCount; i++)
                            *pDestInt++ = (*pSrcInt++) + indexOffset;
                    }
                    else {
                        ushort *pSrcUShort = (ushort *)(srcIntPtr.ToPointer());
                        for (int i=0; i<srcIdxData.indexCount; i++)
                            *pDestUShort++ = (ushort)((*pSrcUShort++) + indexOffset);
                    }
					srcIdxData.indexBuffer.Unlock();

                    // Now deal with vertex buffers
                    // we can rely on buffer counts / formats being the same
                    VertexData srcVData = geom.geometry.vertexData;
                    VertexBufferBinding srcBinds = srcVData.vertexBufferBinding;
                    for(b = 0; b < binds.BindingCount; ++b)
                        // Iterate over vertices
                        destBufferPtrs[b] = CopyVertices(srcBinds.GetBuffer(b), destBufferPtrs[b], bufferElements[b], geom, regionCenter);
                    indexOffset += geom.geometry.vertexData.vertexCount;
				}
			}

			// unlock everything
			indexData.indexBuffer.Unlock();
			for(b = 0; b < binds.BindingCount; ++b)
				binds.GetBuffer(b).Unlock();

			// If we're dealing with stencil shadows, copy the position data from
			// the early half of the buffer to the latter part
			if(stencilShadows) {
				unsafe 
				{
					HardwareVertexBuffer buf = binds.GetBuffer(posBufferIdx);
					IntPtr src = buf.Lock(BufferLocking.Normal);
					byte * pSrc = (byte *)src.ToPointer();
					// Point dest at second half (remember vertexcount is original count)
					byte * pDst = pSrc + buf.VertexSize * vertexData.vertexCount;

					int count = buf.VertexSize * buf.VertexCount;
					while(count-- > 0)
						*pDst++ = *pSrc++;
					buf.Unlock();

					// Also set up hardware W buffer if appropriate
					RenderSystem rend = Root.Instance.RenderSystem;
					if(null != rend && rend.Caps.CheckCap(Capabilities.VertexPrograms))
					{
						buf = HardwareBufferManager.Instance.CreateVertexBuffer(sizeof(float), vertexData.vertexCount * 2, BufferUsage.StaticWriteOnly, false);
						// Fill the first half with 1.0, second half with 0.0
						float * pW = (float *)buf.Lock(BufferLocking.Discard).ToPointer();
						for(int v = 0; v < vertexData.vertexCount; ++v)
							*pW++ = 1.0f;
						for(int v = 0; v < vertexData.vertexCount; ++v)
							*pW++ = 0.0f;
						buf.Unlock();
						vertexData.hardwareShadowVolWBuffer = buf;
					}
				}
			}
		}

		public void Dump()
		{
			log.DebugFormat("---------------");
			log.DebugFormat("Geometry Bucket");
			log.DebugFormat("Format string: {0}", formatString);
			log.DebugFormat("Geometry items: {0}", queuedGeometry.Count);
			log.DebugFormat("Vertex count: {0}", vertexData.vertexCount);
			log.DebugFormat("Index count: {0}", indexData.indexCount);
		}

		#endregion

		#region Protected Methods

		protected unsafe byte *CopyVertices(HardwareVertexBuffer srcBuf, byte *pDst, List<VertexElement> elems, QueuedGeometry geom, Vector3 regionCenter)
		{
			// lock source
			IntPtr src = srcBuf.Lock(BufferLocking.ReadOnly);
			int bufInc = srcBuf.VertexSize;

			byte * pSrc = (byte *)src.ToPointer();
			float * pSrcReal;
			float * pDstReal;
			Vector3 temp = Vector3.Zero;

			// Calculate elem sizes outside the loop
            int [] elemSizes = new int[elems.Count];
            for (int i=0; i<elems.Count; i++)
                elemSizes[i] = VertexElement.GetTypeSize(elems[i].Type);            
            
            // Move the position offset calculation outside the loop
            Vector3 positionDelta = geom.position - regionCenter;

			for(int v = 0; v < geom.geometry.vertexData.vertexCount; ++v)
			{
				// iterate over vertex elements
				for (int i=0; i<elems.Count; i++) {
					VertexElement elem = elems[i];
					pSrcReal = (float *)(pSrc + elem.Offset);
					pDstReal = (float *)(pDst + elem.Offset);

					switch(elem.Semantic)
					{
						case VertexElementSemantic.Position:
							temp.x = *pSrcReal++;
							temp.y = *pSrcReal++;
							temp.z = *pSrcReal++;
							// transform
							temp = (geom.orientation * (temp * geom.scale));
							*pDstReal++ = temp.x + positionDelta.x;
							*pDstReal++ = temp.y + positionDelta.y;
							*pDstReal++ = temp.z + positionDelta.z;
							break;
						case VertexElementSemantic.Normal:
                        case VertexElementSemantic.Tangent:
                        case VertexElementSemantic.Binormal:
							temp.x = *pSrcReal++;
							temp.y = *pSrcReal++;
							temp.z = *pSrcReal++;
							// rotation only
							temp = geom.orientation * temp;
							*pDstReal++ = temp.x;
							*pDstReal++ = temp.y;
							*pDstReal++ = temp.z;
							break;
						default:
							// just raw copy
							int size = elemSizes[i];
							// Optimize the loop for the case that
							// these things are in units of 4
                            if ((size & 0x3) == 0x0) {
                                int cnt = size / 4;
                                while (cnt-- > 0)
                                    *pDstReal++ = *pSrcReal++;
                            }
                            else {
                                // Fall back to the byte-by-byte copy
                                byte * pbSrc = (byte*)pSrcReal;
                                byte * pbDst = (byte*)pDstReal;
                                while(size-- > 0)
                                    *pbDst++ = *pbSrc++;
							}
							break;
					}
				}

				// Increment both pointers
				pDst += bufInc;
				pSrc += bufInc;
			}

			srcBuf.Unlock();
			return pDst;
		}

		#endregion

		#region IRenderable members

		/// TODO: Talk to Jeff about these parameters
        
        public bool NormalizeNormals {
			get { return false; }
		}

		public ushort NumWorldTransforms {
			get { return parent.Parent.Parent.NumWorldTransforms; }
		}

		public bool UseIdentityProjection {
			get { return false; }
		}

		public bool UseIdentityView {
			get { return false; }
		}

		public SceneDetailLevel RenderDetail {
			get { return SceneDetailLevel.Solid; }
		}

		public Vector4 GetCustomParameter(int index) {
			if(customParams[index] == null) {
				throw new Exception("A parameter was not found at the given index");
			}
			else {
				return (Vector4)customParams[index];
			}
		}

		public void SetCustomParameter(int index, Vector4 val) {
			customParams[index] = val;
		}

		public void UpdateCustomGpuParameter(GpuProgramParameters.AutoConstantEntry entry, GpuProgramParameters gpuParams) {
			if(customParams[entry.data] != null) {
				gpuParams.SetConstant(entry.index, (Vector4)customParams[entry.data]);
			}
		}

        /// <summary>
        ///     Dispose the hardware index and vertex buffers
        /// </summary>
        public virtual void Dispose() {
            indexData.indexBuffer.Dispose();
            VertexBufferBinding bindings = vertexData.vertexBufferBinding;
            for(ushort b = 0; b < bindings.BindingCount; ++b)
                bindings.GetBuffer(b).Dispose();
		}

		#endregion

	}

}
