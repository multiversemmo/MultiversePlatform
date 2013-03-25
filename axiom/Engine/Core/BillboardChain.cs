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
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;

namespace Axiom.Core {
    /// <summary>
    ///		Allows the rendering of a chain of connected billboards.
    /// </summary>
    /// <remarks>
    ///    A billboard chain operates much like a traditional billboard, ie its
    ///    segments always face the camera; the difference being that instead of
    ///    a set of disconnected quads, the elements in this class are connected
    ///    together in a chain which must always stay in a continuous strip. This
    ///    kind of effect is useful for creating effects such as trails, beams,
    ///    lightning effects, etc.
    ///    <p/>
    ///    A single instance of this class can actually render multiple separate
    ///    chain segments in a single render operation, provided they all use the
    ///    same material. To clarify the terminology: a 'segment' is a separate 
    ///    sub-part of the chain with its own start and end (called the 'head'
    ///    and the 'tail'. An 'element' is a single position / color / texcoord
    ///    entry in a segment. You can add items to the head of a chain, and 
    ///    remove them from the tail, very efficiently. Each segment has a max
    ///    size, and if adding an element to the segment would exceed this size, 
    ///    the tail element is automatically removed and re-used as the new item
    ///    on the head.
    ///    <p/>
    ///    This class has no auto-updating features to do things like alter the
    ///    color of the elements or to automatically add / remove elements over
    ///    time - you have to do all this yourself as a user of the class. 
    ///    Subclasses can however be used to provide this kind of behaviour 
    ///    automatically. @see RibbonTrail
    /// </remarks>
    public class BillboardChain : MovableObject, IRenderable {
        #region Member variables

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BillboardChain));

		protected List<Element> elementList;

		/// <summary>
        ///    Maximum length of each chain
		/// </summary>
        protected int maxElementsPerChain = 20;

		/// <summary>
        ///    Number of chains
        /// </summary>
		protected int numberOfChains = 1;

		/// <summary>
        ///    Use texture coords?
        /// </summary>
		protected bool useTextureCoords = true;

		/// <summary>
        ///    Use vertex color?
        /// </summary>
		protected bool useVertexColors = true;

		/// <summary>
        ///    Dynamic use?
        /// </summary>
		protected bool dynamic = true;

		/// <summary>
        ///    Vertex data
        /// </summary>
		protected VertexData vertexData;

		/// <summary>
        ///    Index data (to allow multiple unconnected chains)
        /// </summary>
		protected IndexData indexData;

		/// <summary>
        ///    Is the vertex declaration dirty?
        /// </summary>
		protected bool vertexDeclDirty;

		/// <summary>
        ///    Do the buffers need recreating?
        /// </summary>
		protected bool buffersNeedRecreating;

		/// <summary>
        ///    Do the bounds need redefining?
        /// </summary>
		protected bool boundsDirty;

		/// <summary>
        ///    Is the index buffer dirty?
        /// </summary>
		protected bool indexContentDirty;

		/// <summary>
           /// AABB
        /// </summary>
		protected AxisAlignedBox abb = new AxisAlignedBox();

		/// <summary>
        ///    Bounding radius
        /// </summary>
		protected float boundingRadius;

		/// <summary>
        ///    Material 
        /// </summary>
		string materialName;

		/// <summary>
        protected Material material;

        /// </summary>
		///    Texture coord direction
		/// <summary>
        TextureCoordDirection texCoordDirection;

        /// </summary>
		///    Other texture coord range
		/// <summary>
        protected float[] otherTexCoordRange;
        /// </summary>

        /// </summary>
		///    The list holding the chain elements
		/// <summary>
		protected List<Element> chainElementList;

        /// </summary>
		///    A list of chain segments
		/// <summary>
		protected List<ChainSegment> chainSegmentList;

        protected Dictionary<int, object> customParams = new Dictionary<int, object>();

        #endregion Member variables

		/// <summary>
        ///    Simple struct defining a chain segment by referencing a subset of
		///    the preallocated buffer (which will be maxElementsPerChain * numberOfChains
        ///    long), by it's chain index, and a head and tail value which describe
        ///    the current chain. The buffer subset wraps at mMaxElementsPerChain
        ///    so that head and tail can move freely. head and tail are inclusive,
        ///    when the chain is empty head and tail are filled with high-values.
		/// </summary>
		public class ChainSegment {
			/// The start of this chains subset of the buffer
			public int start;
			/// The 'head' of the chain, relative to start
			public int head;
			/// The 'tail' of the chain, relative to start
			public int tail;
		}

        public const int SEGMENT_EMPTY = -1;

        public class Element {
            
			public Element()
            {}

			public Element(Vector3 position, float width, float textureCoord, ColorEx color) {
                this.position = position;
                this.width = width;
                this.textureCoord = textureCoord;
                this.color = color;
            }
            
			public Vector3 position;
			public float width;
			/// U or V texture coord depending on options
			public float textureCoord;
			public ColorEx color;
		}

		/// <summary>
        ///    Constructor
		/// </summary>
		/// <param name="name"> The name to give this object</param>
		/// <param name="maxElements"> The maximum number of elements per chain</param>
		/// <param name="numberOfChains"> The number of separate chain segments contained in this object</param>
		/// <param name="useTextureCoords"> If true, use texture coordinates from the chain elements</param>
		/// <param name="useVertexColors"> If true, use vertex colors from the chain elements</param>
		/// <param name="dynamic"> If true, buffers are created with the intention of being updated</param>
		public BillboardChain(string name, int maxElementsPerChain, int numberOfChains, 
			bool useTextureCoords, bool useVertexColors, bool dynamic) : base(name) {
            this.maxElementsPerChain = maxElementsPerChain;
            this.numberOfChains = numberOfChains;
            this.useTextureCoords = useTextureCoords;
            this.useVertexColors = useVertexColors;
            this.dynamic = dynamic;
            this.texCoordDirection = TextureCoordDirection.U;
            Initialize();
        }

		/// <summary>
        ///    Constructor
		/// <param name="name"> The name to give this object</param>
		/// <param name="parameters"> A varargs array of key/value pairs</param>
		/// </summary>
        public BillboardChain(string name, params Object[] parameters) : base(name) {
            for (int i=0; i<parameters.Length; i+=2) {
                string key = (string) parameters[i];
                Object value = parameters[i + 1];
                if (key == "maxElementsPerChain")
                    maxElementsPerChain = (int)value;
                else if (key == "numberOfChains")
                    numberOfChains = (int)value;
                else if (key == "useTextureCoords")
                    useTextureCoords = (bool)value;
                else if (key == "useVertexColors")
                    useVertexColors = (bool)value;
                else if (key == "dynamic")
                    dynamic = (bool)value;
                else
                    log.Error("BillboardChain constructor: unrecognized parameter '" + key + "'");
            }
            Initialize();
		}

        protected void Initialize() {
            this.vertexDeclDirty = true;
            this.buffersNeedRecreating = true;
            this.boundsDirty = true;
            this.indexContentDirty = true;
            this.boundingRadius = 0.0f;

            vertexData = new VertexData();
            indexData = new IndexData();

            otherTexCoordRange = new float[2];
            otherTexCoordRange[0] = 0.0f;
            otherTexCoordRange[1] = 1.0f;

            SetupChainContainers();

            vertexData.vertexStart = 0;
            // index data set up later
            // set basic white material
            this.materialName = "BaseWhiteNoLighting";
        }

        // Currently unreferenced        
        public void DisposeBuffers() {
            if (vertexData != null)
                vertexData = null;
            if (indexData != null)
                indexData = null;
        }

        protected void SetupChainContainers() {
            // Allocate enough space for everything
            int elements = numberOfChains * maxElementsPerChain;
            chainElementList = new List<Element>();
            for (int i=0; i<elements; i++)
                chainElementList.Add(new Element());
            vertexData.vertexCount = elements * 2;

            // Configure chains
            chainSegmentList = new List<ChainSegment>();
            for (int i=0; i<numberOfChains; i++) {
                ChainSegment seg = new ChainSegment();
                seg.start = i * maxElementsPerChain;
                seg.tail = seg.head = SEGMENT_EMPTY;
                chainSegmentList.Add(seg);
            }
        }

        protected void SetupVertexDeclaration() {
            if (vertexDeclDirty) {
                VertexDeclaration decl = vertexData.vertexDeclaration;

                int offset = 0;
                // Add a description for the buffer of the positions of the vertices
                decl.AddElement(0, offset, VertexElementType.Float3, VertexElementSemantic.Position);
                offset += VertexElement.GetTypeSize(VertexElementType.Float3);

                if (useVertexColors) {
                    decl.AddElement(0, offset, VertexElementType.Color, VertexElementSemantic.Diffuse);
                    offset += VertexElement.GetTypeSize(VertexElementType.Color);
                }

                if (useTextureCoords) {
                    decl.AddElement(0, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords);
                    offset += VertexElement.GetTypeSize(VertexElementType.Float2);
                }

                if (!useTextureCoords && !useVertexColors) {
                    log.Error(
                        "Error - BillboardChain '" + name + "' is using neither " +
                        "texture coordinates or vertex colors; it will not be " +
                        "visible on some rendering APIs so you should change this " +
                        "so you use one or the other.");
                }
                vertexDeclDirty = false;
            }
        }

        protected void SetupBuffers() {
            SetupVertexDeclaration();
            if (buffersNeedRecreating) {
                // Create the vertex buffer (always dynamic due to the camera adjust)
                HardwareVertexBuffer pBuffer =
                    HardwareBufferManager.Instance.CreateVertexBuffer(
                    vertexData.vertexDeclaration.GetVertexSize(0),
                    vertexData.vertexCount,
                    BufferUsage.DynamicWriteOnlyDiscardable);

                // (re)Bind the buffer
                // Any existing buffer will lose its reference count and be destroyed
                vertexData.vertexBufferBinding.SetBinding(0, pBuffer);

                indexData.indexBuffer =
                    HardwareBufferManager.Instance.CreateIndexBuffer(
                        IndexType.Size16,
                        numberOfChains * maxElementsPerChain * 6, // max we can use
                        dynamic? BufferUsage.DynamicWriteOnly : BufferUsage.StaticWriteOnly);
                // NB we don't set the indexCount on IndexData here since we will
                // probably use less than the maximum number of indices

                buffersNeedRecreating = false;
            }
        }

		/// <summary>
        ///    Gets or sets the maximum number of chain elements per chain 
		/// </summary>
		public virtual int MaxChainElements {
            get {
                return maxElementsPerChain;
            }
            set {
                maxElementsPerChain = value;
                SetupChainContainers();
                buffersNeedRecreating = true;
                indexContentDirty = true;
            }
        }

		/// <summary>
        ///    Get the number of chain segments (this class can render
        ///    multiple chains at once using the same material). 
		/// </summary>
		public virtual int NumberOfChains {
            get {
                return numberOfChains;
            }
            set {
                numberOfChains = value;
                SetupChainContainers();
                buffersNeedRecreating = true;
                indexContentDirty = true;
            }
        }
        
		/// <summary>
        ///    Gets or sets whether texture coordinate information should be
		///    included in the final buffers generated.
		/// </summary>
        /// <note>
        ///    You must use either texture coordinates or vertex color since the
        ///    vertices have no normals and without one of these there is no source of
        ///    color for the vertices.
        /// </note>
		public virtual bool UseTextureCoords {
            get {
                return useTextureCoords;
            }
            set {
                useTextureCoords = value;
                vertexDeclDirty = true;
                buffersNeedRecreating = true;
                indexContentDirty = true;
            }
        }


        /// <summary>
		///    Gets or sets the direction in which texture coords specified on each element
		///    are deemed to run along the length of the chain.
        /// </summary>
		/// <param name="dir"> The direction, default is TextureCoordDirection.U.</param>
		public virtual TextureCoordDirection TexCoordDirection {
            get {
                return texCoordDirection;
            }
            set {
                texCoordDirection = value;
            }
        }
        
        /// <summary>
		///    Sets the range of the texture coordinates generated across the width of
		///    the chain elements.
        /// </summary>
		/// <param name="start"> Start coordinate, default 0.0.</param>
		/// <param name="end"> End coordinate, default 1.0.</param>
		public virtual void SetOtherTextureCoordRange(float start, float end) {
            otherTexCoordRange[0] = start;
            otherTexCoordRange[1] = end;
        }

        /// <summary>
        ///    Gets or sets whether vertex color information should be included in the
		///    final buffers generated.
        /// </summary>
		/// <note>
        ///    You must use either texture coordinates or vertex color since the
        ///    vertices have no normals and without one of these there is no source of
        ///    color for the vertices.
        /// </note>
        public bool UseVertexColors {
            get {
                return useVertexColors;
            }
            set {
                useVertexColors = value;
                vertexDeclDirty = true;
                buffersNeedRecreating = true;
                indexContentDirty = true;
            }
        }
        
        /// <summary>
        ///    Sets whether or not the buffers created for this object are suitable
		///    for dynamic alteration.
        /// </summary>
        public bool Dynamic {
            get {
                return dynamic;
            }
            set {
                dynamic = value;
                buffersNeedRecreating = true;
                indexContentDirty = true;
            }
        }

		/// <summary>
        ///    Gets or sets the material name in use
        /// </summary>
		public string MaterialName {
            get {
                return materialName;
            }
            set {
                materialName = value;
                material = MaterialManager.Instance.GetByName(materialName);

                if (material == null) {
                    log.Warn("BillboardChain.MaterialName setter: Can't assign material " + materialName +
                        " to BillboardChain " + name + " because this " +
                        "Material does not exist. Have you forgotten to define it in a " +
                        ".material script?");
                    material = MaterialManager.Instance.GetByName("BaseWhiteNoLighting");
                    if (material == null) {
                        log.Error("BillboardChain.MaterialName setter: Can't assign default material " +
                            "to BillboardChain of " + name + ". Did " +
                            "you forget to call MaterialManager.Initialise()?");
                    }
                }
                // Ensure new material loaded (will not load again if already loaded)
                material.Load();
            }
        }

		/// <summary>
        ///    Add an element to the 'head' of a chain.
        /// </summary>
		/// <remarks>
        ///    If this causes the number of elements to exceed the maximum elements
        ///    per chain, the last element in the chain (the 'tail') will be removed
        ///    to allow the additional element to be added.
        /// </remarks>
		/// <param name="chainIndex"> The index of the chain</param>
		/// <param name="billboardChainElement"> The details to add</param>
        protected void AddChainElement(int chainIndex, Element dtls) {
            if (chainIndex >= numberOfChains) {
                log.Error("BillboardChain.AddChainElement: chainIndex " + chainIndex + " out of bounds");
                return;
            }
            ChainSegment seg = chainSegmentList[chainIndex];
            if (seg.head == SEGMENT_EMPTY) {
                // Tail starts at end, head grows backwards
                seg.tail = maxElementsPerChain - 1;
                seg.head = seg.tail;
                indexContentDirty = true;
            }
            else
            {
                if (seg.head == 0)
                    // Wrap backwards
                    seg.head = maxElementsPerChain - 1;
                else
                    // Just step backward
                    --seg.head;
                // Run out of elements?
                if (seg.head == seg.tail) {
                    // Move tail backwards too, losing the end of the segment and re-using
                    // it in the head
                    if (seg.tail == 0)
                        seg.tail = maxElementsPerChain - 1;
                    else
                        --seg.tail;
                }
            }

            // Set the details
            chainElementList[seg.start + seg.head] = dtls;

            indexContentDirty = true;
            boundsDirty = true;
            // tell parent node to update bounds
            if (parentNode != null)
                parentNode.NeedUpdate();
        }

		/// <summary>
        ///    Remove an element from the 'tail' of a chain.
		/// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        protected void RemoveChainElement(int chainIndex) {
            if (chainIndex >= numberOfChains) {
                log.Error("BillboardChain.RemoveChainElement: chainIndex " + chainIndex + " out of bounds");
                return;
            }
            ChainSegment seg = chainSegmentList[chainIndex];
            if (seg.head == SEGMENT_EMPTY)
                return; // do nothing, nothing to remove


            if (seg.tail == seg.head)
                // last item
                seg.head = seg.tail = SEGMENT_EMPTY;
            else if (seg.tail == 0)
                seg.tail = maxElementsPerChain - 1;
            else
                --seg.tail;

            // we removed an entry so indexes need updating
            indexContentDirty = true;
            boundsDirty = true;
            // tell parent node to update bounds
            if (parentNode != null)
                parentNode.NeedUpdate();
        }

		/// <summary>
        ///    Remove all elements of a given chain (but leave the chain intact).
        /// </summary>
		public virtual void ClearChain(int chainIndex) {
            if (chainIndex >= numberOfChains) {
                log.Error("BillboardChain.ClearChain: chainIndex " + chainIndex + " out of bounds");
                return;
            }
            ChainSegment seg = chainSegmentList[chainIndex];

            // Just reset head & tail
            seg.tail = seg.head = SEGMENT_EMPTY;
            
            // we removed an entry so indexes need updating
            indexContentDirty = true;
            boundsDirty = true;
            // tell parent node to update bounds
            if (parentNode != null)
                parentNode.NeedUpdate();
        }

		/// <summary>
        ///    Remove all elements from all chains (but leave the chains themselves intact).
        /// </summary>
        public void ClearAllChains() {
            for (int i=0; i<numberOfChains; i++)
                ClearChain(i);
        }

		/// <summary>
        ///    Update the details of an existing chain element.
        /// </summary>
		/// <param name="chainIndex> The index of the chain</param>
		/// <param name="elementIndex"> The element index within the chain, measured from the 'head' of the chain</param>
		/// <param name="billboardChainElement"> The details to set</param>
        protected void UpdateChainElement(int chainIndex, int elementIndex, Element dtls) {
            if (chainIndex >= numberOfChains) {
                log.Error("BillboardChain.UpdateChainElement: chainIndex " + chainIndex + " out of bounds");
                return;
            }
            ChainSegment seg = chainSegmentList[chainIndex];
            if (seg.head == SEGMENT_EMPTY) {
                log.Error("BillboardChain.UpdateChainElement: Chain segment is empty " + chainIndex + " is empty");
                return;
            }

            int idx = seg.head + elementIndex;
            // adjust for the edge and start
            idx = (idx % maxElementsPerChain) + seg.start;

            chainElementList[idx] = dtls;

            boundsDirty = true;
            // tell parent node to update bounds
            if (parentNode != null)
                parentNode.NeedUpdate();
        }

		/// <summary>
        ///    Get the detail of a chain element.
        /// </summary>
		/// <param name="chainIndex> The index of the chain</param>
		/// <param name="elementIndex"> The element index within the chain, measured from the 'head' of the chain</param>
        protected Element GetChainElement(int chainIndex, int elementIndex) {
            if (chainIndex >= numberOfChains) {
                log.Error("BillboardChain.GetChainElement: chainIndex " + chainIndex + " out of bounds");
                return null;
            }
            ChainSegment seg = chainSegmentList[chainIndex];
            int idx = seg.head + elementIndex;
            // adjust for the edge and start
            idx = (idx % maxElementsPerChain) + seg.start;

            return chainElementList[idx];
        }

        #region IRenderable members

		public bool CastsShadows {
			get {
				return false;
			}
		}

        public Technique Technique {
            get {
                return material.GetBestTechnique();
            }
        }

        public Material Material {
            get { 
                return material; 
            }
        }

        public virtual void GetWorldTransforms(Matrix4[] matrices) {
            matrices[0] = parentNode.FullTransform;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ushort NumWorldTransforms {
            get { 
                return 1;	
            }
        }

        /// 
        /// </summary>
        public bool UseIdentityProjection {
            get { 
                return false; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseIdentityView {
            get { 
                return false; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SceneDetailLevel RenderDetail {
            get { 
                return SceneDetailLevel.Solid; 
            }
        }

        public float GetSquaredViewDepth(Camera camera) {
            Vector3 min = abb.Minimum;
            Vector3 max = abb.Maximum;
            Vector3 mid = ((max - min) * 0.5f) + min;
            Vector3 dist = camera.DerivedPosition - mid;
            return dist.LengthSquared;
        }

        /// <summary>
        /// 
        /// </summary>
        public Quaternion WorldOrientation {
            get {
                return parentNode.DerivedOrientation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 WorldPosition {
            get {
                return parentNode.DerivedPosition;
            }
        }

        public List<Light> Lights {
            get {
                return parentNode.Lights;
            }
        }

        public void GetRenderOperation(RenderOperation op) {
            op.indexData = indexData;
            op.operationType = OperationType.TriangleList;
            op.useIndices = true;
            op.vertexData = vertexData;
        }

        public override void NotifyCurrentCamera(Camera camera) {
            UpdateVertexBuffer(camera);
        }

        /// <summary>
        ///    Local bounding radius of this billboard set.
        /// </summary>
        public override float BoundingRadius {
            get {
                return boundingRadius;
            }
        }

        public override AxisAlignedBox BoundingBox {
            // cloning to prevent direct modification
            get { 
                UpdateBounds();
                return (AxisAlignedBox)abb.Clone(); 
            }
        }

        public override void UpdateRenderQueue(RenderQueue queue) {
            UpdateIndexBuffer();
            if (indexData.indexCount > 0)
                queue.AddRenderable(this, RenderQueue.DEFAULT_PRIORITY, renderQueueID);
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

        public bool NormalizeNormals {
            get {
                return false;
            }
        }
	
        #endregion IRenderable members

		// Overridden members follow

        protected void UpdateVertexBuffer(Camera camera) {
            SetupBuffers();
            HardwareVertexBuffer pBuffer = vertexData.vertexBufferBinding.GetBuffer(0);
            IntPtr lockPtr = pBuffer.Lock(BufferLocking.Discard);

            Vector3 camPos = camera.DerivedPosition;
            Vector3 eyePos = parentNode.DerivedOrientation.Inverse() *
                ((camPos - parentNode.DerivedPosition) / parentNode.DerivedScale);

            Vector3 chainTangent = Vector3.Zero;
            unsafe {
                foreach (ChainSegment seg in chainSegmentList) {
                    // Skip 0 or 1 element segment counts
                    if (seg.head != SEGMENT_EMPTY && seg.head != seg.tail) {
                        int laste = seg.head;
                        for (int e = seg.head; ; ++e) // until break
                        {
                            // Wrap forwards
                            if (e == maxElementsPerChain)
                                e = 0;

                            Element elem = chainElementList[e + seg.start];
                            Debug.Assert (((e + seg.start) * 2) < 65536, "Too many elements!");
                            short baseIdx = (short)((e + seg.start) * 2);

                            // Determine base pointer to vertex #1
                            byte* pBase = (byte *)lockPtr.ToPointer();
                            pBase += pBuffer.VertexSize * baseIdx;

                            // Get index of next item
                            int nexte = e + 1;
                            if (nexte == maxElementsPerChain)
                                nexte = 0;

                            if (e == seg.head)
                                // No laste, use next item
                                chainTangent = chainElementList[nexte + seg.start].position - elem.position;
                            else if (e == seg.tail)
                                // No nexte, use only last item
                                chainTangent = elem.position - chainElementList[laste + seg.start].position;
                            else
                                // A mid position, use tangent across both prev and next
                                chainTangent = chainElementList[nexte + seg.start].position - chainElementList[laste + seg.start].position;

                            Vector3 vP1ToEye = eyePos - elem.position;
                            Vector3 vPerpendicular = chainTangent.Cross(vP1ToEye);
                            vPerpendicular.Normalize();
                            vPerpendicular *= elem.width * 0.5f;

                            Vector3 pos0 = elem.position - vPerpendicular;
                            Vector3 pos1 = elem.position + vPerpendicular;

                            float* pFloat = (float *)pBase;
                            // pos1
                            *pFloat++ = pos0.x;
                            *pFloat++ = pos0.y;
                            *pFloat++ = pos0.z;

                            pBase = (byte *)pFloat;

                            if (useVertexColors) {
                                uint* pCol = (uint *)pBase;
                                *pCol++ = Root.Instance.ConvertColor(elem.color);
                                pBase = (byte *)pCol;
                            }

                            if (useTextureCoords) {
                                pFloat = (float *)pBase;
                                if (texCoordDirection == TextureCoordDirection.U) {
                                    *pFloat++ = elem.textureCoord;
                                    *pFloat++ = otherTexCoordRange[0];
                                }
                                else {
                                    *pFloat++ = otherTexCoordRange[0];
                                    *pFloat++ = elem.textureCoord;
                                }
                                pBase = (byte *)pFloat;
                            }

                            // pos2
                            pFloat = (float *)pBase;
                            *pFloat++ = pos1.x;
                            *pFloat++ = pos1.y;
                            *pFloat++ = pos1.z;
                            pBase = (byte *)pFloat;

                            if (useVertexColors) {
                                uint* pCol = (uint *)pBase;
                                *pCol++ = Root.Instance.ConvertColor(elem.color);
                                pBase = (byte *)pCol;
                            }

                            if (useTextureCoords) {
                                pFloat = (float *)pBase;
                                if (texCoordDirection == TextureCoordDirection.U) {
                                    *pFloat++ = elem.textureCoord;
                                    *pFloat++ = otherTexCoordRange[1];
                                }
                                else {
                                    *pFloat++ = otherTexCoordRange[1];
                                    *pFloat++ = elem.textureCoord;
                                }
                                pBase = (byte *)pFloat;
                            }

                            if (e == seg.tail)
                                break; // last one

                            laste = e;

                        } // element
                    } // segment valid?

                } // each segment
            } // unsafe

            pBuffer.Unlock();
        }

        protected void UpdateIndexBuffer() {
            SetupBuffers();
            if (indexContentDirty) {
                unsafe {
                    IntPtr idxPtr = indexData.indexBuffer.Lock(BufferLocking.Discard);

                    ushort* pShort = (ushort *)idxPtr.ToPointer();
                    indexData.indexCount = 0;
                    // indexes
                    foreach (ChainSegment seg in chainSegmentList) {
                        // Skip 0 or 1 element segment counts
                        if (seg.head != SEGMENT_EMPTY && seg.head != seg.tail) {
                            // Start from head + 1 since it's only useful in pairs
                            int laste = seg.head;
                            while(true) // until break
                            {
                                int e = laste + 1;
                                // Wrap forwards
                                if (e == maxElementsPerChain)
                                    e = 0;
                                // indexes of this element are (e * 2) and (e * 2) + 1
                                // indexes of the last element are the same, -2
                                Debug.Assert (((e + seg.start) * 2) < 65536, "Too many elements!");
                                ushort baseIdx = (ushort)((e + seg.start) * 2);
                                ushort lastBaseIdx = (ushort)((laste + seg.start) * 2);
                                *pShort++ = lastBaseIdx;
                                *pShort++ = (ushort)(lastBaseIdx + 1);
                                *pShort++ = baseIdx;
                                *pShort++ = (ushort)(lastBaseIdx + 1);
                                *pShort++ = (ushort)(baseIdx + 1);
                                *pShort++ = baseIdx;

                                indexData.indexCount += 6;


                                if (e == seg.tail)
                                    break; // last one

                                laste = e;
                            }
                        }
                    }
                    indexData.indexBuffer.Unlock();
                }
                indexContentDirty = false;
            }
        }

        public virtual void UpdateBounds() {
            if (boundsDirty) {
                abb.IsNull = true;
                Vector3 widthVector = Vector3.Zero;
                foreach (ChainSegment seg in chainSegmentList) {
                    if (seg.head != SEGMENT_EMPTY) {
                        for(int e = seg.head; ; ++e) {
                            // Wrap forwards
                            if (e == maxElementsPerChain)
                                e = 0;
                            Element elem = chainElementList[seg.start + e];
                            widthVector.x = widthVector.y = widthVector.z = elem.width;
                            abb = AxisAlignedBox.FromDimensions(elem.position, widthVector * 2.0f);

                            if (e == seg.tail)
                                break;
                        }
                    }
                }
                // Set the current radius
                if (abb.IsNull)
                    boundingRadius = 0.0f;
                else
                    boundingRadius = (float)Math.Sqrt(Math.Max(abb.Minimum.LengthSquared, abb.Maximum.LengthSquared));
                boundsDirty = false;
            }
        }
	}

}


