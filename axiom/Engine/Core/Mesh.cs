// #define OLD_WAY

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Axiom.Animating;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.MathLib;
using Axiom.Serialization;
using Axiom.Graphics;
using Axiom.Utility;

namespace Axiom.Core {
    /// <summary>
    ///    Resource holding data about a 3D mesh.
    /// </summary>
    /// <remarks>
    ///    This class holds the data used to represent a discrete
    ///    3-dimensional object. Mesh data usually contains more
    ///    than just vertices and triangle information; it also
    ///    includes references to materials (and the faces which use them),
    ///    level-of-detail reduction information, convex hull definition,
    ///    skeleton/bones information, keyframe animation etc.
    ///    However, it is important to note the emphasis on the word
    ///    'discrete' here. This class does not cover the large-scale
    ///    sprawling geometry found in level / landscape data.
    ///    <p/>
    ///    Multiple world objects can (indeed should) be created from a
    ///    single mesh object - see the Entity class for more info.
    ///    The mesh object will have its own default
    ///    material properties, but potentially each world instance may
    ///    wish to customize the materials from the original. When the object
    ///    is instantiated into a scene node, the mesh material properties
    ///    will be taken by default but may be changed. These properties
    ///    are actually held at the SubMesh level since a single mesh may
    ///    have parts with different materials.
    ///    <p/>
    ///    As described above, because the mesh may have sections of differing
    ///    material properties, a mesh is inherently a compound contruct,
    ///    consisting of one or more SubMesh objects.
    ///    However, it strongly 'owns' its SubMeshes such that they
    ///    are loaded / unloaded at the same time. This is contrary to
    ///    the approach taken to hierarchically related (but loosely owned)
    ///    scene nodes, where data is loaded / unloaded separately. Note
    ///    also that mesh sub-sections (when used in an instantiated object)
    ///    share the same scene node as the parent.
    /// </remarks>
    /// TODO: Add Clone method
    public class Mesh : Resource {
        #region Fields

        // protected static TimingMeter meshLoadMeter = MeterManager.GetMeter("Mesh Load", "Mesh");

        /// <summary>
        ///		Shared vertex data between multiple meshes.
        ///	</summary>
        protected VertexData sharedVertexData;
        /// <summary>
        ///     Shared index map for translating blend index to bone index.
        /// </summary>
        /// <remarks>
        ///     This index map can be shared among multiple submeshes. SubMeshes might not have
        ///     their own IndexMap, they might share this one.
        ///
        ///     We collect actually used bones of all bone assignments, and build the
        ///     blend index in 'packed' form, then the range of the blend index in vertex
        ///     data BlendIndices element is continuous, with no gaps. Thus, by
        ///     minimising the world matrix array constants passing to GPU, we can support
        ///     more bones for a mesh when hardware skinning is used. The hardware skinning
        ///     support limit is applied to each set of vertex data in the mesh, in other words, the
        ///     hardware skinning support limit is applied only to the actually used bones of each
        ///     SubMeshes, not all bones across the entire Mesh.
        ///
        ///     Because the blend index is different to the bone index, therefore, we use
        ///     the index map to translate the blend index to bone index.
        ///
        ///     The use of shared or non-shared index map is determined when
        ///     model data is converted to the OGRE .mesh format.
        /// </remarks>
        protected List<ushort> sharedBlendIndexToBoneIndexMap = new List<ushort>();
        /// <summary>
        ///		Collection of sub meshes for this mesh.
        ///	</summary>
        protected List<SubMesh> subMeshList = new List<SubMesh>();
		/// <summary>
		///		Local bounding box of this mesh.
		/// </summary>
        protected AxisAlignedBox boundingBox = AxisAlignedBox.Null;
		/// <summary>
		///		Radius of this mesh's bounding sphere.
		/// </summary>
        protected float boundingSphereRadius;

        /// <summary>Name of the skeleton bound to this mesh.</summary>
        protected string skeletonName;
         /// <summary>Reference to the skeleton bound to this mesh.</summary>
        protected Skeleton skeleton;
        /// <summary>List of bone assignment for this mesh.</summary>
        protected Dictionary<int, List<VertexBoneAssignment>> boneAssignmentList = 
            new Dictionary<int, List<VertexBoneAssignment>>();
        /// <summary>Flag indicating that bone assignments need to be recompiled.</summary>
        protected bool boneAssignmentsOutOfDate;
        /// <summary>Number of blend weights that are assigned to each vertex.</summary>
        protected short numBlendWeightsPerVertex;
        /// <summary>Option whether to use software or hardware blending, there are tradeoffs to both.</summary>
        protected internal bool useSoftwareBlending;

		/// <summary>
		///		Flag indicating the use of manually created LOD meshes.
		/// </summary>
        protected internal bool isLodManual;
		/// <summary>
		///		Number of LOD meshes available.
		/// </summary>
        protected internal int numLods;
		/// <summary>
		///		List of data structures describing LOD usage.
		/// </summary>
        protected internal List<MeshLodUsage> lodUsageList = new List<MeshLodUsage>();

		/// <summary>
		///		Usage type for the vertex buffer.
		/// </summary>
        protected BufferUsage vertexBufferUsage;
		/// <summary>
		///		Usage type for the index buffer.
		/// </summary>
        protected BufferUsage indexBufferUsage;
		/// <summary>
		///		Use a shadow buffer for the vertex data?
		/// </summary>
        protected bool useVertexShadowBuffer;
		/// <summary>
		///		Use a shadow buffer for the index data?
		/// </summary>
        protected bool useIndexShadowBuffer;

		/// <summary>
		///		Flag indicating whether precalculation steps to support shadows have been taken.
		/// </summary>
		protected bool isPreparedForShadowVolumes;
		/// <summary>
		///		Should edge lists be automatically built for this mesh?
		/// </summary>
		protected bool autoBuildEdgeLists;
        /// <summary>
        ///     Have the edge lists been built for this mesh yet?
        /// </summary>
        protected internal bool edgeListsBuilt;

        /// <summary>Internal list of named transforms attached to this mesh.</summary>
        protected List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();

        /// <summary>
        ///     Storage of morph animations, lookup by name
        /// </summary>
		protected Dictionary<string, Animation> animationsList = new Dictionary<string, Animation>();
        /// <summary>
        ///     The vertex animation type associated with the shared vertex data
        /// </summary>
		protected VertexAnimationType sharedVertexDataAnimationType;
        /// <summary>
        ///     Do we need to scan animations for animation types?
        /// </summary>
		protected bool animationTypesDirty;
        /// <summary>
        ///     List of available poses for shared and dedicated geometryPoseList
        /// </summary>
		protected List<Pose> poseList = new List<Pose>();
        /// <summary>
        ///     A list of triangles, plus machinery to determine the closest intersection point
        /// </summary>
		protected TriangleIntersector triangleIntersector = null;

        #endregion

        public class MeshSkinningContext {
            public Matrix4[] boneMatrices;
            public List<ushort> boneIndexMap;
            public Matrix4[] inverseTransposeBoneMatrices;
            public Dictionary<int, List<VertexBoneAssignment>> vertexWeights = 
                new Dictionary<int, List<VertexBoneAssignment>>();
        }

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public Mesh(string name) {
            this.name = name;

            // default to static write only for speed
            vertexBufferUsage = BufferUsage.StaticWriteOnly;
            indexBufferUsage = BufferUsage.StaticWriteOnly;

			// default to having shadow buffers
			useVertexShadowBuffer = true;
			useIndexShadowBuffer = true;

            numLods = 1;
            MeshLodUsage lod = new MeshLodUsage();
            lod.fromSquaredDepth = 0.0f;
            lodUsageList.Add(lod);

            // always use software blending for now
            useSoftwareBlending = true;

            this.SkeletonName = "";
        }

        #endregion

        #region Properties

        public List<AttachmentPoint> AttachmentPoints { get { return attachmentPoints; } }

		/// <summary>
		///		Gets/Sets whether or not this Mesh should automatically build edge lists
		///		when asked for them, or whether it should never build them if
		///		they are not already provided.
		/// </summary>
		public bool AutoBuildEdgeLists {
			get {
				return autoBuildEdgeLists;
			}
			set {
				autoBuildEdgeLists = value;
			}
		}

		/// <summary>
		///		Gets/Sets the shared VertexData for this mesh.
		/// </summary>
		public VertexData SharedVertexData {
			get { 
				return sharedVertexData; 
			}
			set { 
				sharedVertexData = value; 
			}
		}

        public List<ushort> SharedBlendIndexToBoneIndexMap {
            get {
                return sharedBlendIndexToBoneIndexMap;
            }
        }

		/// <summary>
		///    Gets the number of submeshes belonging to this mesh.
		/// </summary>
		public int SubMeshCount {
			get {
				return subMeshList.Count;
			}
		}

		/// <summary>
		///		Gets/Sets the bounding box for this mesh.
		/// </summary>
		/// <remarks>
		///		Setting this property is required when building manual meshes now, because Axiom can no longer 
		///		update the bounds for you, because it cannot necessarily read vertex data back from 
		///		the vertex buffers which this mesh uses (they very well might be write-only, and even
		///		if they are not, reading data from a hardware buffer is a bottleneck).
		/// </remarks>
		public AxisAlignedBox BoundingBox {
			get {
				// OPTIMIZE: Cloning to prevent direct modification
				return (AxisAlignedBox)boundingBox.Clone();
			}
			set {
				boundingBox = value;

				float sqLen1 = boundingBox.Minimum.LengthSquared;
				float sqLen2 = boundingBox.Maximum.LengthSquared;

				// update the bounding sphere radius as well
				boundingSphereRadius = MathUtil.Sqrt(MathUtil.Max(sqLen1, sqLen2));
			}
		}

        /// <summary>
        ///    Bounding spehere radius from this mesh in local coordinates.
        /// </summary>
        public float BoundingSphereRadius {
            get { 
				return boundingSphereRadius; 
			}
            set { 
				boundingSphereRadius = value; 
			}
        }

        /// <summary>
        ///   The number of vertex animations in the mesh
        /// </summary>
        public int AnimationCount {
            get {
                return animationsList.Count;
            }
        }

        /// <summary>
        ///		Gets the edge list for this mesh, building it if required. 
        /// </summary>
        /// <returns>The edge list for mesh LOD 0.</returns>
        public EdgeData GetEdgeList() {
            return GetEdgeList(0);
        }

        /// <summary>
		///		Gets the edge list for this mesh, building it if required. 
		/// </summary>
		/// <remarks>
		///		You must ensure that the Mesh as been prepared for shadow volume 
		///		rendering if you intend to use this information for that purpose.
		/// </remarks>
		public EdgeData GetEdgeList(int lodIndex) {
			if(!edgeListsBuilt) {
				BuildEdgeList();
			}

			return GetLodLevel(lodIndex).edgeData;
		}

        /// <summary>
        ///    Determins whether or not this mesh has a skeleton associated with it.
        /// </summary>
        public bool HasSkeleton {
            get {
                return (skeletonName.Length != 0);
            }
        }

        /// <summary>
        ///    Gets the usage setting for this meshes index buffers.
        /// </summary>
        public BufferUsage IndexBufferUsage {
            get {
                return indexBufferUsage;
            }
        }

        /// <summary>
        ///    Gets whether or not this meshes index buffers are shadowed.
        /// </summary>
        public bool UseIndexShadowBuffer {
            get {
                return useIndexShadowBuffer;
            }
        }

        /// <summary>
        ///     Returns whether this mesh has an attached edge list.
        /// </summary>
        public bool IsEdgeListBuilt {
            get {
                return edgeListsBuilt;
            }
        }

        /// <summary>
        ///     Returns true if this mesh is using manual LOD.
        /// </summary>
        /// <remarks>
        ///     A mesh can either use automatically generated LOD, or it can use alternative
        ///     meshes as provided by an artist. A mesh can only use either all manual LODs 
        ///     or all generated LODs, not a mixture of both.
        /// </remarks>
        public bool IsLodManual {
            get {
                return isLodManual;
            }
        }

        /// <summary>
        ///		Defines whether this mesh is to be loaded from a resource, or created manually at runtime.
        /// </summary>
        public bool IsManuallyDefined {
            get { 
				return isManual; 
			}
            set {
                isManual = value; 
			}
        }

		/// <summary>
		///		Gets whether this mesh has already had its geometry prepared for use in 
		///		rendering shadow volumes.
		/// </summary>
		public bool IsPreparedForShadowVolumes {
			get {
				return isPreparedForShadowVolumes;
			}
		}

        /// <summary>
        ///		Gets the current number of Lod levels associated with this mesh.
        /// </summary>
        public int LodLevelCount {
            get { 
				return lodUsageList.Count; 
			}
        }

        /// <summary>
        ///    Gets the skeleton currently bound to this mesh.
        /// </summary>
        public Skeleton Skeleton {
            get {
                return skeleton;
            }
        }

        /// <summary>
        ///    Get/Sets the name of the skeleton which will be bound to this mesh.
        /// </summary>
        public string SkeletonName {
            get {
                return skeletonName;
            }
            set {
                skeletonName = value;

				if (skeletonName == null || skeletonName.Length == 0) {
					skeleton = null;
				} else {
					// load the skeleton
					skeleton = SkeletonManager.Instance.Load(skeletonName);
				}
            }
        }

        /// <summary>
        ///    Gets the usage setting for this meshes vertex buffers.
        /// </summary>
        public BufferUsage VertexBufferUsage {
            get {
                return vertexBufferUsage;
            }
        }

        /// <summary>
        ///    Gets whether or not this meshes vertex buffers are shadowed.
        /// </summary>
        public bool UseVertexShadowBuffer {
            get {
                return useVertexShadowBuffer;
            }
        }

		/// <summary>
		///		Gets bone assigment list
		/// </summary>
		public Dictionary<int, List<VertexBoneAssignment>> BoneAssignmentList {
			get { return boneAssignmentList; }
		}

		/// <summary>
		///		Gets bone assigment list
		/// </summary>
		public List<Pose> PoseList {
			get { return poseList; }
		}

		/// <summary>
		///		Gets bone assigment list
		/// </summary>
		public VertexAnimationType SharedVertexDataAnimationType {
			get {
				if (animationTypesDirty)
					DetermineAnimationTypes();
				return sharedVertexDataAnimationType;
			}
		}

		/// <summary>Returns whether or not this mesh has some kind of vertex animation.</summary>
		public bool HasVertexAnimation {
			get {
				return animationsList.Count > 0;
			}
		}
		
		/// <summary>Are the derived animation types out of date?</summary>
		public bool AnimationTypesDirty {
			get {
				return animationTypesDirty;
			}
		}
		
		/// <summary>A list of triangles, plus machinery to determine the closest intersection point</summary>
		public TriangleIntersector TriangleIntersector {
			get {
				return triangleIntersector;
			}
			set {
				triangleIntersector = value;
			}
		}
		
		#endregion Properties

        #region Methods

        /// <summary>
        ///    Assigns a vertex to a bone with a given weight, for skeletal animation. 
        /// </summary>
        /// <remarks>
        ///    This method is only valid after setting SkeletonName.
        ///    You should not need to modify bone assignments during rendering (only the positions of bones) 
        ///    and the engine reserves the right to do some internal data reformatting of this information, 
        ///    depending on render system requirements.
        /// </remarks>
        /// <param name="boneAssignment">Bone assignment to add.</param>
        public void AddBoneAssignment(VertexBoneAssignment boneAssignment) {
            if (!boneAssignmentList.ContainsKey(boneAssignment.vertexIndex))
                boneAssignmentList[boneAssignment.vertexIndex] = new List<VertexBoneAssignment>();
            boneAssignmentList[boneAssignment.vertexIndex].Add(boneAssignment);
            boneAssignmentsOutOfDate = true;
        }

        /// <summary>
        ///    Adds the vertex and index sets necessary for a builder instance
		///    to iterate over the triangles in a mesh
		/// </summary>
		public void AddVertexAndIndexSets(AnyBuilder builder, int lodIndex) {
			int vertexSetCount = 0;

			if (sharedVertexData != null) {
				builder.AddVertexData(sharedVertexData);
				vertexSetCount++;
			}

			// Prepare the builder using the submesh information
			for (int i = 0; i < subMeshList.Count; i++) {
				SubMesh sm = subMeshList[i];

				if (sm.useSharedVertices) {
					// Use shared vertex data, index as set 0
					if (lodIndex == 0) {
						// Use shared vertex data, index as set 0
						builder.AddIndexData(sm.indexData, 0, sm.operationType);
					}
					else {
						builder.AddIndexData(sm.lodFaceList[lodIndex - 1], 0, sm.operationType);
					}
				}
				else {
					// own vertex data, add it and reference it directly
					builder.AddVertexData(sm.vertexData);

					if (lodIndex == 0) {
						// base index data
						builder.AddIndexData(sm.indexData, vertexSetCount++, sm.operationType);
					}
					else {
						// LOD index data
						builder.AddIndexData(sm.lodFaceList[lodIndex - 1], vertexSetCount++, sm.operationType);
					}
				}
			}
		}

		/// <summary>
		///		Builds an edge list for this mesh, which can be used for generating a shadow volume
		///		among other things.
		/// </summary>
		public void BuildEdgeList() {
            if (edgeListsBuilt) {
                return;
            }

			// loop over LODs
            for (int lodIndex = 0; lodIndex < lodUsageList.Count; lodIndex++) {
                // use getLodLevel to enforce loading of manual mesh lods
                MeshLodUsage usage = GetLodLevel(lodIndex);

                if (isLodManual && lodIndex != 0) {
                    // Delegate edge building to manual mesh
                    // It should have already built its own edge list while loading
                    usage.edgeData = usage.manualMesh.GetEdgeList(0);
                }
                else {
                    EdgeListBuilder builder = new EdgeListBuilder();

					// Add this mesh's vertex and index data structures
					AddVertexAndIndexSets(builder, lodIndex);

                    // build the edge data from all accumulate vertex/index buffers
                    usage.edgeData = builder.Build();
                }
			}
			
            edgeListsBuilt = true;
        }

        public void FreeEdgeList() {
            if (!edgeListsBuilt)
                return;

            for (int i = 0; i < lodUsageList.Count; ++i) {
                MeshLodUsage usage = lodUsageList[i];
                usage.edgeData = null;
            }

            edgeListsBuilt = false;
        }

        /// <summary>
        ///     Create the list of triangles used to query mouse hits
        /// </summary>
		public void CreateTriangleIntersector() {
			// Create the TriangleListBuilder instance that will create the list of triangles for this mesh
			TriangleListBuilder builder = new TriangleListBuilder();
			// Add this mesh's vertex and index data structures for lod 0
			AddVertexAndIndexSets(builder, 0);
			// Create the list of triangles
			triangleIntersector = new TriangleIntersector(builder.Build());
		}

        /// <summary>
        ///     Builds tangent space vector required for accurate bump mapping.
        /// </summary>
        /// <remarks>
        ///    Adapted from bump mapping tutorials at:
        ///    http://www.paulsprojects.net/tutorials/simplebump/simplebump.html
        ///    author : paul.baker@univ.ox.ac.uk
        ///    <p/>
        ///    Note: Only the tangent vector is calculated, it is assumed the binormal
        ///    will be calculated in a vertex program.
        /// </remarks>
        /// <param name="sourceTexCoordSet">Source texcoord set that holds the current UV texcoords.</param>
        /// <param name="destTexCoordSet">Destination texcoord set to hold the tangent vectors.</param>
        public void BuildTangentVectors(short sourceTexCoordSet, short destTexCoordSet) {
            if (destTexCoordSet == 0) {
                throw new AxiomException("Destination texture coordinate set must be greater than 0.");
            }

            // temp data buffers
            ushort[] vertIdx = new ushort[3];
            Vector3[] vertPos = new Vector3[3];
            float[] u = new float[3];
            float[] v = new float[3];

            // setup a new 3D texture coord-set buffer for every sub mesh
            int numSubMeshes = this.SubMeshCount;

            bool sharedGeometryDone = false;

            unsafe {
                // setup a new 3D tex coord buffer for every submesh
                for(int sm = 0; sm < numSubMeshes; sm++) {
                    // the face indices buffer, read only
                    ushort* pIdx = null;
                    // pointer to 2D tex.coords, read only
                    float* p2DTC = null;
                    // pointer to 3D tex.coords, write/read (discard)
                    float* p3DTC = null;
                    // vertex position buffer, read only
                    float* pVPos = null;

                    SubMesh subMesh = GetSubMesh(sm);

                    // get index buffer pointer
                    IndexData idxData = subMesh.indexData;
                    HardwareIndexBuffer buffIdx = idxData.indexBuffer;
                    IntPtr indices = buffIdx.Lock(BufferLocking.ReadOnly);
                    pIdx = (ushort*)indices.ToPointer();

                    // get vertex pointer
                    VertexData usedVertexData;

                    if(subMesh.useSharedVertices) {
                        // don't do shared geometry more than once
                        if (sharedGeometryDone) {
                            continue;
                        }

                        usedVertexData = sharedVertexData;
                        sharedGeometryDone = true;
                    }
                    else {
                        usedVertexData = subMesh.vertexData;
                    }

                    VertexDeclaration decl = usedVertexData.vertexDeclaration;
                    VertexBufferBinding binding = usedVertexData.vertexBufferBinding;

                    // make sure we have a 3D coord to place data in
                    OrganizeTangentsBuffer(usedVertexData, destTexCoordSet);

                    // get the target element
                    VertexElement destElem = decl.FindElementBySemantic(VertexElementSemantic.TexCoords, destTexCoordSet);
                    // get the source element
                    VertexElement srcElem = decl.FindElementBySemantic(VertexElementSemantic.TexCoords, sourceTexCoordSet);

                    if (srcElem == null || srcElem.Type != VertexElementType.Float2) {
                        // TODO: SubMesh names
                        throw new AxiomException("SubMesh '{0}' of Mesh '{1}' has no 2D texture coordinates at the selected set, therefore we cannot calculate tangents.", "<TODO: SubMesh name>", name);
                    }

                    HardwareVertexBuffer srcBuffer = null, destBuffer = null, posBuffer = null;

                    IntPtr srcPtr, destPtr, posPtr;
                    int srcInc, destInc, posInc;

                    srcBuffer = binding.GetBuffer(srcElem.Source);

                    // Is the source and destination buffer the same?
                    if (srcElem.Source == destElem.Source) {
                        // lock source for read and write
                        srcPtr = srcBuffer.Lock(BufferLocking.Normal);

                        srcInc = srcBuffer.VertexSize;
                        destPtr = srcPtr;
                        destInc = srcInc;
                    }
                    else {
                        srcPtr = srcBuffer.Lock(BufferLocking.ReadOnly);
                        srcInc = srcBuffer.VertexSize;
                        destBuffer = binding.GetBuffer(destElem.Source);
                        destInc = destBuffer.VertexSize;
                        destPtr = destBuffer.Lock(BufferLocking.Normal);
                    }

                    VertexElement elemPos = decl.FindElementBySemantic(VertexElementSemantic.Position);

                    if (elemPos.Source == srcElem.Source) {
                        posPtr = srcPtr;
                        posInc = srcInc;
                    }
                    else if (elemPos.Source == destElem.Source) {
                        posPtr = destPtr;
                        posInc = destInc;
                    }
                    else {
                        // a different buffer
                        posBuffer = binding.GetBuffer(elemPos.Source);
                        posPtr = posBuffer.Lock(BufferLocking.ReadOnly);
                        posInc = posBuffer.VertexSize;
                    }

                    // loop through all faces to calculate the tangents and normals
                    int numFaces = idxData.indexCount / 3;
                    int vCount = 0;

                    // loop through all faces to calculate the tangents
                    for(int n = 0; n < numFaces; n++) {
                        int i = 0;

                        for(i = 0; i < 3; i++) {
                            // get indices of vertices that form a polygon in the position buffer
                            vertIdx[i] = pIdx[vCount++];

                            IntPtr tmpPtr = new IntPtr(posPtr.ToInt32() + elemPos.Offset + (posInc * vertIdx[i]));

                            pVPos = (float*)tmpPtr.ToPointer();

                            // get the vertex positions from the position buffer
                            vertPos[i].x = pVPos[0];
                            vertPos[i].y = pVPos[1];
                            vertPos[i].z = pVPos[2];

                            // get the vertex tex coords from the 2D tex coord buffer
                            tmpPtr = new IntPtr(srcPtr.ToInt32() + srcElem.Offset + (srcInc * vertIdx[i]));
                            p2DTC = (float*)tmpPtr.ToPointer();

                            u[i] = p2DTC[0];
                            v[i] = p2DTC[1];
                        } // for v = 1 to 3

                        // calculate the tangent space vector
                        Vector3 tangent = 
                            MathUtil.CalculateTangentSpaceVector(
                                vertPos[0], vertPos[1], vertPos[2],
                                u[0], v[0], u[1], v[1], u[2], v[2]);

                        // write new tex.coords 
                        // note we only write the tangent, not the binormal since we can calculate
                        // the binormal in the vertex program
                        byte* vBase = (byte*)destPtr.ToPointer();

                        for(i = 0; i < 3; i++) {
                            // write values (they must be 0 and we must add them so we can average
                            // all the contributions from all the faces
                            IntPtr tmpPtr = new IntPtr(destPtr.ToInt32() + destElem.Offset + (destInc * vertIdx[i]));

                            p3DTC = (float*)tmpPtr.ToPointer();

                            p3DTC[0] += tangent.x;
                            p3DTC[1] += tangent.y;
                            p3DTC[2] += tangent.z;
                        } // for v = 1 to 3
                    } // for each face

                    int numVerts = usedVertexData.vertexCount;

                    int offset = 0;

                    byte* qBase = (byte*)destPtr.ToPointer();

                    // loop through and normalize all 3d tex coords
                    for(int n = 0; n < numVerts; n++) {
                        IntPtr tmpPtr = new IntPtr(destPtr.ToInt32() + destElem.Offset + offset);

                        p3DTC = (float*)tmpPtr.ToPointer();

                        // read the 3d tex coord
                        Vector3 temp = new Vector3(p3DTC[0], p3DTC[1], p3DTC[2]);

                        // normalize the tex coord
                        temp.Normalize();

                        // write it back to the buffer
                        p3DTC[0] = temp.x;
                        p3DTC[1] = temp.y;
                        p3DTC[2] = temp.z;

                        offset += destInc;
                    }

                    // unlock all used buffers
                    srcBuffer.Unlock();

                    if (destBuffer != null) {
                        destBuffer.Unlock();
                    }

                    if (posBuffer != null) {
                        posBuffer.Unlock();
                    }

                    buffIdx.Unlock();
                } // for each subMesh
            } // unsafe
        }

		/// <summary>
		///     Builds tangent space vector required for accurate bump mapping.
		/// </summary>
		/// <remarks>
		///    Adapted from bump mapping tutorials at:
		///    http://www.paulsprojects.net/tutorials/simplebump/simplebump.html
		///    author : paul.baker@univ.ox.ac.uk
		///    <p/>
		///    Note: Only the tangent vector is calculated, it is assumed the binormal
		///    will be calculated in a vertex program.
		/// </remarks>
        public void BuildTangentVectors() {
            // default using the first tex coord set and stuffing the tangent vectors in the 
            BuildTangentVectors(0, 1);
        }

        /// <summary>
        ///    Removes all bone assignments for this mesh. 
        /// </summary>
        /// <remarks>
        ///    This method is for modifying weights to the shared geometry of the Mesh. To assign
        ///    weights to the per-SubMesh geometry, see the equivalent methods on SubMesh.
        /// </remarks>
        public void ClearBoneAssignments() {
            boneAssignmentList.Clear();
            boneAssignmentsOutOfDate = true;
        }

        protected internal void BuildIndexMap(Dictionary<int, List<VertexBoneAssignment>> boneAssignments, 
                                              List<ushort> boneIndexToBlendIndexMap, 
                                              List<ushort> blendIndexToBoneIndexMap)
        {
            if (boneAssignments.Count == 0)
            {
                // Just in case
                boneIndexToBlendIndexMap.Clear();
                blendIndexToBoneIndexMap.Clear();
                return;
            }

            Dictionary<ushort, ushort> usedBoneIndicesSet = new Dictionary<ushort,ushort>();

            // Collect actually used bones
            foreach (List<VertexBoneAssignment> vbaList in boneAssignments.Values) {
                foreach (VertexBoneAssignment vba in vbaList) {
                    usedBoneIndicesSet[vba.boneIndex] = vba.boneIndex;
                }
            }

            List<ushort> usedBoneIndices = new List<ushort>(usedBoneIndicesSet.Keys);
            usedBoneIndices.Sort();

            // Allocate space for index map
            ResizeList(blendIndexToBoneIndexMap, usedBoneIndices.Count, ushort.MaxValue);
            ResizeList(boneIndexToBlendIndexMap, usedBoneIndices[usedBoneIndices.Count - 1] + 1, ushort.MaxValue);

            // Make index map between bone index and blend index
            ushort blendIndex = 0;
            foreach (ushort boneIndex in usedBoneIndices) {
                boneIndexToBlendIndexMap[boneIndex] = blendIndex;
                blendIndexToBoneIndexMap[blendIndex] = boneIndex;
                blendIndex++;
            }
        }

        protected internal void ResizeList<T>(List<T> l, int count, T defaultValue) {
            if (l.Count > count)
                l.RemoveRange(count, count - l.Count);
            else
                for (int i = l.Count; i < count; ++i)
                    l.Add(defaultValue);
        }

        /// <summary>
        ///    Compile bone assignments into blend index and weight buffers.
        /// </summary>
        protected internal void CompileBoneAssignments() {
            int maxBones = RationalizeBoneAssignments(sharedVertexData.vertexCount, boneAssignmentList);

            // check for no bone assignments
            if (maxBones != 0)
            {
                CompileBoneAssignments(boneAssignmentList, maxBones, sharedBlendIndexToBoneIndexMap, sharedVertexData);
            }

            boneAssignmentsOutOfDate = false;
        }

        /// <summary>
        ///    Software blending oriented bone assignment compilation.
        /// </summary>
        protected internal void CompileBoneAssignments(Dictionary<int, List<VertexBoneAssignment>> boneAssignments, 
													   int numBlendWeightsPerVertex, List<ushort> blendIndexToBoneIndexMap,
                                                       VertexData targetVertexData) {
			// Create or reuse blend weight / indexes buffer
			// Indices are always a UBYTE4 no matter how many weights per vertex
			// Weights are more specific though since they are Reals
			VertexDeclaration decl = targetVertexData.vertexDeclaration;
			VertexBufferBinding bind = targetVertexData.vertexBufferBinding;
			ushort bindIndex;

            List<ushort> boneIndexToBlendIndexMap = new List<ushort>();
            BuildIndexMap(boneAssignments, boneIndexToBlendIndexMap, blendIndexToBoneIndexMap);

			VertexElement testElem = decl.FindElementBySemantic(VertexElementSemantic.BlendIndices);

			if (testElem != null) {
				// Already have a buffer, unset it & delete elements
				bindIndex = testElem.Source;

				// unset will cause deletion of buffer
				bind.UnsetBinding(bindIndex);
				decl.RemoveElement(VertexElementSemantic.BlendIndices);
				decl.RemoveElement(VertexElementSemantic.BlendWeights);
			}
			else {
				// Get new binding
				bindIndex = bind.NextIndex;
			}

			int bufferSize = Marshal.SizeOf(typeof(byte)) * 4;
			bufferSize += Marshal.SizeOf(typeof(float)) * numBlendWeightsPerVertex; 

			HardwareVertexBuffer vbuf = 
				HardwareBufferManager.Instance.CreateVertexBuffer(
					bufferSize,
					targetVertexData.vertexCount, 
					BufferUsage.StaticWriteOnly,
					true); // use shadow buffer
	                
			// bind new buffer
			bind.SetBinding(bindIndex, vbuf);
	        
			VertexElement idxElem, weightElem;

			VertexElement firstElem = decl.GetElement(0);

			// add new vertex elements
			// Note, insert directly after position to abide by pre-Dx9 format restrictions
			if(firstElem.Semantic == VertexElementSemantic.Position) {
				int insertPoint = 1;
				
				while(insertPoint < decl.ElementCount && 
					decl.GetElement(insertPoint).Source == firstElem.Source) {

					insertPoint++;
				}

				idxElem = decl.InsertElement(insertPoint, bindIndex, 0, VertexElementType.UByte4, 
					VertexElementSemantic.BlendIndices);

				weightElem = decl.InsertElement(insertPoint + 1, bindIndex, Marshal.SizeOf(typeof(byte)) * 4, 
					VertexElement.MultiplyTypeCount(VertexElementType.Float1, numBlendWeightsPerVertex),
					VertexElementSemantic.BlendWeights);
			}
			else
			{
				// Position is not the first semantic, therefore this declaration is
				// not pre-Dx9 compatible anyway, so just tack it on the end
				idxElem = decl.AddElement(bindIndex, 0, VertexElementType.UByte4, VertexElementSemantic.BlendIndices);
				weightElem = decl.AddElement(bindIndex, Marshal.SizeOf(typeof(byte)) * 4, 
					VertexElement.MultiplyTypeCount(VertexElementType.Float1, numBlendWeightsPerVertex),
					VertexElementSemantic.BlendWeights);
			}


			// Assign data
			IntPtr ptr = vbuf.Lock(BufferLocking.Discard);

			unsafe {
				byte* pBase = (byte*)ptr.ToPointer();

				// Iterate by vertex
				float* pWeight;
				byte* pIndex;

				for (int v = 0; v < targetVertexData.vertexCount; v++) {
					/// Convert to specific pointers
					pWeight = (float*)((byte*)pBase + weightElem.Offset);
					pIndex = pBase + idxElem.Offset;

                    // get the bone assignment enumerator and move to the first one in the list
                    List<VertexBoneAssignment> vbaList = boneAssignments[v];

                    for (int bone = 0; bone < numBlendWeightsPerVertex; bone++) {
						// Do we still have data for this vertex?
						if (bone < vbaList.Count) {
                            VertexBoneAssignment ba = vbaList[bone];
							// If so, write weight
							*pWeight++ = ba.weight;
							*pIndex++ = (byte)boneIndexToBlendIndexMap[ba.boneIndex];
						}
						else {
							// Ran out of assignments for this vertex, use weight 0 to indicate empty
							*pWeight++ = 0.0f;
							*pIndex++ = 0;
						}
					}

					pBase += vbuf.VertexSize;
				}
			}

			vbuf.Unlock();
        }

        protected internal void BuildIndexMap() {
        }

        /// <summary>
        ///    Retrieves the level of detail index for the given depth value.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public int GetLodIndex(float depth) {
            return GetLodIndexSquaredDepth(depth * depth);
        }

        /// <summary>
        ///    Gets the mesh lod level at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MeshLodUsage GetLodLevel(int index) {
            Debug.Assert(index < lodUsageList.Count, "index < lodUsageList.Count");

            MeshLodUsage usage = lodUsageList[index];

            // load the manual lod mesh for this level if not done already
            if (isLodManual && index > 0 && usage.manualMesh == null) {
                usage.manualMesh = MeshManager.Instance.Load(usage.manualName);

                // get the edge data, if required
                if (!autoBuildEdgeLists) {
                    usage.edgeData = usage.manualMesh.GetEdgeList(0);
                }
            }

            return usage;
        }

        /// <summary>
        ///    Internal method for making the space for a 3D texture coord buffer to hold tangents.
        /// </summary>
        /// <param name="vertexData">Target vertex data.</param>
        /// <param name="destCoordSet">Destination texture coordinate set.</param>
        protected void OrganizeTangentsBuffer(VertexData vertexData, short destCoordSet) {
            bool needsToBeCreated = false;

            // grab refs to the declarations and bindings
            VertexDeclaration decl = vertexData.vertexDeclaration;
            VertexBufferBinding binding = vertexData.vertexBufferBinding;

            // see if we already have a 3D tex coord buffer
            VertexElement tex3d = decl.FindElementBySemantic(VertexElementSemantic.TexCoords, destCoordSet);

            if(tex3d == null) {
                needsToBeCreated = true;
            }
            else if(tex3d.Type != VertexElementType.Float3) {
                // tex coord buffer exists, but is not 3d.
                throw new AxiomException("Texture coordinate set {0} already exists but is not 3D, therefore cannot contain tangents. Pick an alternative destination coordinate set.", destCoordSet);
            }
                
            if(needsToBeCreated) {
                // What we need to do, to be most efficient with our vertex streams, 
                // is to tack the new 3D coordinate set onto the same buffer as the 
                // previous texture coord set
                VertexElement prevTexCoordElem = 
                    vertexData.vertexDeclaration.FindElementBySemantic(
                        VertexElementSemantic.TexCoords, (short)(destCoordSet - 1));

                if (prevTexCoordElem == null) {
                    throw new AxiomException("Cannot locate the texture coordinate element preceding the destination texture coordinate set to which to append the new tangents.");
                }

                // find the buffer associated with this element
                HardwareVertexBuffer origBuffer = vertexData.vertexBufferBinding.GetBuffer(prevTexCoordElem.Source);

                // Now create a new buffer, which includes the previous contents
                // plus extra space for the 3D coords
                HardwareVertexBuffer newBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                    origBuffer.VertexSize + (3 * Marshal.SizeOf(typeof(float))),
                    vertexData.vertexCount,
                    origBuffer.Usage,
                    origBuffer.HasShadowBuffer);

                // add the new element
                decl.AddElement(
                    prevTexCoordElem.Source, 
                    origBuffer.VertexSize, 
                    VertexElementType.Float3, 
                    VertexElementSemantic.TexCoords, 
                    destCoordSet);

                // now copy the original data across
                IntPtr srcPtr = origBuffer.Lock(BufferLocking.ReadOnly);
                IntPtr destPtr = newBuffer.Lock(BufferLocking.Discard);

                int vertSize = origBuffer.VertexSize;

                // size of the element to skip
                int elemSize = Marshal.SizeOf(typeof(float)) * 3;

                for (int i = 0, srcOffset = 0, dstOffset = 0; i < vertexData.vertexCount; i++) {
                    // copy original vertex data
                    Memory.Copy(srcPtr, destPtr, srcOffset, dstOffset, vertSize);

                    srcOffset += vertSize;
                    dstOffset += vertSize;

                    // Set the new part to 0 since we'll accumulate in this
                    Memory.Set(destPtr, dstOffset, elemSize);
                    dstOffset += elemSize;
                }

                // unlock those buffers!
                origBuffer.Unlock();
                newBuffer.Unlock();

                // rebind the new buffer
                binding.SetBinding(prevTexCoordElem.Source, newBuffer);
            }
        }

        /// <summary>
        ///     Ask the mesh to suggest parameters to a future <see cref="BuildTangentVectors"/> call.
        /// </summary>
        /// <remarks>
        ///     This helper method will suggest source and destination texture coordinate sets
        ///     for a call to <see cref="BuildTangentVectors"/>. It will detect when there are inappropriate
        ///     conditions (such as multiple geometry sets which don't agree). 
        ///     Moreover, it will return 'true' if it detects that there are aleady 3D 
        ///     coordinates in the mesh, and therefore tangents may have been prepared already.
        /// </remarks>
        /// <param name="sourceCoordSet">A source texture coordinate set which will be populated.</param>
        /// <param name="destCoordSet">A destination texture coordinate set which will be populated.</param>
        public bool SuggestTangentVectorBuildParams(out short sourceCoordSet, out short destCoordSet) {
            // initialize out params
            sourceCoordSet = 0;
            destCoordSet = 0;

            // Go through all the vertex data and locate source and dest (must agree)
            bool sharedGeometryDone = false;
            bool foundExisting = false;
            bool firstOne = true;

            for (int i = 0; i < subMeshList.Count; i++) {
                SubMesh sm = subMeshList[i];

                VertexData vertexData;

                if (sm.useSharedVertices) {
                    if (sharedGeometryDone) {
                        continue;
                    }

                    vertexData = sharedVertexData;
                    sharedGeometryDone = true;
                }
                else {
                    vertexData = sm.vertexData;
                }

                VertexElement sourceElem = null;

                short t = 0;

                for ( ; t < Config.MaxTextureCoordSets; t++) {
                    VertexElement testElem = 
                        vertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.TexCoords, t);

                    if (testElem == null) {
                        // finish if we've run out, t will be the target
                        break;
                    }

                    if (sourceElem == null) {
                        // We're still looking for the source texture coords
                        if (testElem.Type == VertexElementType.Float2) {
                            // ok, we found it!
                            sourceElem = testElem;
                        }
                    }
                    else {
                        // We're looking for the destination
                        // Check to see if we've found a possible
                        if (testElem.Type == VertexElementType.Float3) {
                            // This is a 3D set, might be tangents
                            foundExisting = true;
                        }
                    }
                } // for t

                // After iterating, we should have a source and a possible destination (t)
                if (sourceElem == null) {
                    throw new AxiomException("Cannot locate an appropriate 2D texture coordinate set for all the vertex data in this mesh to create tangents from.");
                }

                // Check that we agree with previous decisions, if this is not the first one
                if (!firstOne) {
                    if (sourceElem.Index != sourceCoordSet) {
                        throw new AxiomException("Multiple sets of vertex data in this mesh disagree on the appropriate index to use for the source texture coordinates. This ambiguity must be rectified before tangents can be generated.");
                    }
                    if (t != destCoordSet) {
                        throw new AxiomException("Multiple sets of vertex data in this mesh disagree on the appropriate index to use for the target texture coordinates. This ambiguity must be rectified before tangents can be generated.");
                    }
                }

                // Otherwise, save this result
                sourceCoordSet = (short)sourceElem.Index;
                destCoordSet = t;

                firstOne = false;
            } // for i

            return foundExisting;
        }

        public void GenerateLodLevels(List<float> lodDistances, 
                                      ProgressiveMesh.VertexReductionQuota reductionMethod,
                                      float reductionValue) {
            RemoveLodLevels();

            foreach (SubMesh subMesh in subMeshList) {
                // Set up data for reduction
                VertexData vertexData = subMesh.useSharedVertices ? sharedVertexData : subMesh.vertexData;

                ProgressiveMesh pm = new ProgressiveMesh(vertexData, subMesh.indexData);
                pm.Build((ushort)lodDistances.Count, subMesh.lodFaceList, reductionMethod, reductionValue);
            }

            // Iterate over the lods and record usage
            foreach (float dist in lodDistances) {
                // Record usage
                MeshLodUsage lod = new MeshLodUsage();
                lod.fromSquaredDepth = dist * dist;
                lod.edgeData = null;
                lod.manualMesh = null;
                lodUsageList.Add(lod);
            }
            numLods = (ushort)lodDistances.Count + 1;
        }

        public void RemoveLodLevels() {
            if (!this.IsLodManual) {
                foreach (SubMesh subMesh in this.subMeshList)
                    subMesh.RemoveLodLevels();
            }

            FreeEdgeList();
            this.lodUsageList.Clear();
            this.numLods = 1;
            MeshLodUsage lod = new MeshLodUsage();
            lod.fromSquaredDepth = 0.0f;
            lod.edgeData = null;
            lod.manualMesh = null;
            this.lodUsageList.Add(lod);
            this.isLodManual = false;
        }

        /// <summary>
        ///    Retrieves the level of detail index for the given squared depth value.
        /// </summary>
        /// <remarks>
        ///    Internally the lods are stored at squared depths to avoid having to perform
        ///    square roots when determining the lod. This method allows you to provide a
        ///    squared length depth value to avoid having to do your own square roots.
        /// </remarks>
        /// <param name="squaredDepth"></param>
        /// <returns></returns>
        public int GetLodIndexSquaredDepth(float squaredDepth) {
            for(int i = 0; i < lodUsageList.Count; i++) {
                if(lodUsageList[i].fromSquaredDepth > squaredDepth) {
                    return i - 1;
                }
            }

            // if we fall all the wat through, use the higher value
            return lodUsageList.Count - 1;
        }

        /// <summary>
        ///    Gets the sub mesh at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SubMesh GetSubMesh(int index) {
            Debug.Assert(index < subMeshList.Count, "index < subMeshList.Count");

            return subMeshList[index];
        }
        
        /// <summary>
        ///   Gets the animation track handle for a named submesh.
        /// </summary>
        /// <param name="name">The name of the submesh</param>
        /// <returns>The track handle to use for animation tracks associated with the give submesh</returns>
        public int GetTrackHandle(string name)
        {
            for (int i = 0; i < subMeshList.Count; i++)
            {
                if (subMeshList[i].name == name)
                {
                    return i + 1;
                }
            }

            // not found
            throw new AxiomException("A SubMesh with the name '{0}' does not exist in mesh '{1}'", name, this.name);
        }

		/// <summary>
        ///     Gets the sub mesh with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SubMesh GetSubMesh(string name) {
            for (int i = 0; i < subMeshList.Count; i++) {
                SubMesh sub = subMeshList[i];

                if (sub.name == name) {
                    return sub;
                }
            }

            // not found
            throw new AxiomException("A SubMesh with the name '{0}' does not exist in mesh '{1}'", name, this.name);
        }

        /// <summary>
        ///    Remove the sub mesh with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void RemoveSubMesh(string name) {
            for (int i = 0; i < subMeshList.Count; i++) {
                SubMesh sub = subMeshList[i];

                if (sub.name == name) {
                    subMeshList.RemoveAt(i);
					return;
				}
            }

            // not found
            throw new AxiomException("A SubMesh with the name '{0}' does not exist in mesh '{1}'", name, this.name);
        }

        
        /// <summary>
        ///    Initialise an animation set suitable for use with this mesh.
        /// </summary>
        /// <remarks>
        ///    Only recommended for use inside the engine, not by applications.
        /// </remarks>
        /// <param name="animSet"></param>
        public void InitAnimationState(AnimationStateSet animSet) {
//             Debug.Assert(skeleton != null, "Skeleton not present.");

            if (HasSkeleton) {
				// delegate the animation set to the skeleton
				skeleton.InitAnimationState(animSet);

				// Take the opportunity to update the compiled bone assignments
				if(boneAssignmentsOutOfDate) {
					CompileBoneAssignments();
				}

				// compile bone assignments for each sub mesh
				for(int i = 0; i < subMeshList.Count; i++) {
					SubMesh subMesh = subMeshList[i];

					if(subMesh.boneAssignmentsOutOfDate) {
						subMesh.CompileBoneAssignments();
					}
				} // for
			}

			// Animation states for vertex animation
			foreach (Animation animation in animationsList.Values) {
				// Only create a new animation state if it doesn't exist
				// We can have the same named animation in both skeletal and vertex
				// with a shared animation state affecting both, for combined effects
				// The animations should be the same length if this feature is used!
				if (!HasAnimationState(animSet, animation.Name)) {
					animSet.CreateAnimationState(animation.Name, 0.0f, animation.Length);
				}
			}
        }

		/// <summary>Returns whether or not this mesh has some kind of vertex animation.</summary>
		public bool HasAnimationState(AnimationStateSet animSet, string name) {
			return animSet.HasAnimationState(name);
		}

		/// <summary>Returns whether or not this mesh has the named vertex animation.</summary>
        public bool ContainsAnimation(string name) {
			return animationsList.ContainsKey(name);
		}
		
		/// <summary>
        ///    Internal notification, used to tell the Mesh which Skeleton to use without loading it. 
        /// </summary>
        /// <remarks>
        ///    This is only here for unusual situation where you want to manually set up a
        ///    Skeleton. Best to let the engine deal with this, don't call it yourself unless you
        ///    really know what you're doing.
        /// </remarks>
        /// <param name="skeleton"></param>
        public void NotifySkeleton(Skeleton skeleton) {
            this.skeleton = skeleton;
            skeletonName = skeleton.Name;
        }

		/// <summary>
		///		This method prepares the mesh for generating a renderable shadow volume.
		/// </summary>
		/// <remarks>
		///		Preparing a mesh to generate a shadow volume involves firstly ensuring that the 
		///		vertex buffer containing the positions for the mesh is a standalone vertex buffer,
		///		with no other components in it. This method will therefore break apart any existing
		///		vertex buffers this mesh holds if position is sharing a vertex buffer. 
		///		Secondly, it will double the size of this vertex buffer so that there are 2 copies of 
		///		the position data for the mesh. The first half is used for the original, and the second 
		///		half is used for the 'extruded' version of the mesh. The vertex count of the main 
		///		<see cref="VertexData"/> used to render the mesh will remain the same though, so as not to add any 
		///		overhead to regular rendering of the object.
		///		Both copies of the position are required in one buffer because shadow volumes stretch 
		///		from the original mesh to the extruded version.
		///		<p/>
		///		Because shadow volumes are rendered in turn, no additional
		///		index buffer space is allocated by this method, a shared index buffer allocated by the
		///		shadow rendering algorithm is used for addressing this extended vertex buffer.
		/// </remarks>
		public void PrepareForShadowVolume() {
            if (isPreparedForShadowVolumes) {
                return;
            }

            if(sharedVertexData != null) {
				sharedVertexData.PrepareForShadowVolume();
			}

			for(int i = 0; i < subMeshList.Count; i++) {
				SubMesh sm = subMeshList[i];

				if(!sm.useSharedVertices) {
					sm.vertexData.PrepareForShadowVolume();
				}
			}

			isPreparedForShadowVolumes = true;
		}

        /// <summary>
        ///     Rationalizes the passed in bone assignment list.
        /// </summary>
        /// <remarks>
        ///     We support up to 4 bone assignments per vertex. The reason for this limit
        ///     is that this is the maximum number of assignments that can be passed into
        ///     a hardware-assisted blending algorithm. This method identifies where there are
        ///     more than 4 bone assignments for a given vertex, and eliminates the bone
        ///     assignments with the lowest weights to reduce to this limit. The remaining
        ///     weights are then re-balanced to ensure that they sum to 1.0.
        /// </remarks>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="assignments">
        ///     The bone assignment list to rationalize. This list will be modified and
        ///     entries will be removed where the limits are exceeded.
        /// </param>
        /// <returns>The maximum number of bone assignments per vertex found, clamped to [1-4]</returns>
        internal int RationalizeBoneAssignments(int vertexCount, Dictionary<int, List<VertexBoneAssignment>> assignments) {
            int maxBones = 0;
            int currentBones = 0;

            for(int i = 0; i < vertexCount; i++) {
                // gets the numbers of assignments for the current vertex
                currentBones = assignments[i].Count;

                // Deal with max bones update 
                // (note this will record maxBones even if they exceed limit)
                if(maxBones < currentBones) {
                    maxBones = currentBones;
                }

                // does the number of bone assignments exceed limit?
                if(currentBones > Config.MaxBlendWeights) {
                    List<VertexBoneAssignment> sortedList = assignments[i];
                    IComparer<VertexBoneAssignment> comp = new VertexBoneAssignmentWeightComparer();
					sortedList.Sort(comp);
                    sortedList.RemoveRange(0, currentBones - Config.MaxBlendWeights);
                }

                float totalWeight = 0.0f;

                // Make sure the weights are normalised
                // Do this irrespective of whether we had to remove assignments or not
                //   since it gives us a guarantee that weights are normalised
                //  We assume this, so it's a good idea since some modellers may not
                List<VertexBoneAssignment> vbaList = assignments[i];

                foreach (VertexBoneAssignment vba in vbaList) {
                    totalWeight += vba.weight;
                }

                // Now normalise if total weight is outside tolerance
                float delta = 1.0f / (1 << 24);
                if(!MathUtil.FloatEqual(totalWeight, 1.0f, delta)) {
                    foreach (VertexBoneAssignment vba in vbaList) {
                        vba.weight /= totalWeight;
                    }
                }
            }

            // Warn that we've reduced bone assignments
            if(maxBones > Config.MaxBlendWeights) {
                LogManager.Instance.Write("WARNING: Mesh '{0}' includes vertices with more than {1} bone assignments.  The lowest weighted assignments beyond this limit have been removed.", name, Config.MaxBlendWeights);

                maxBones = Config.MaxBlendWeights;
            }

            return maxBones;
        }

		/// <summary>
		///		Creates a new <see cref="SubMesh"/> and gives it a name.
		/// </summary>
		/// <param name="name">Name of the new <see cref="SubMesh"/>.</param>
		/// <returns>A new <see cref="SubMesh"/> with this Mesh as its parent.</returns>
		public SubMesh CreateSubMesh(string name) {
			SubMesh subMesh = new SubMesh(name);

			// set the parent of the subMesh to us
			subMesh.Parent = this;

			// add to the list of child meshes
			subMeshList.Add(subMesh);

			return subMesh;
		}

		/// <summary>
		///		Creates a new <see cref="SubMesh"/>.
		/// </summary>
		/// <remarks>
		///		Method for manually creating geometry for the mesh.
		///		Note - use with extreme caution - you must be sure that
		///		you have set up the geometry properly.
		/// </remarks>
		/// <returns>A new SubMesh with this Mesh as its parent.</returns>
		public SubMesh CreateSubMesh() {
			string name = string.Format("{0}_SubMesh{1}", this.name, subMeshList.Count);

			SubMesh subMesh = new SubMesh(name);

			// set the parent of the subMesh to us
			subMesh.Parent = this;

			// add to the list of child meshes
			subMeshList.Add(subMesh);

			return subMesh;
		}

		/// <summary>
		///		Sets the policy for the vertex buffers to be used when loading this Mesh.
		/// </summary>
		/// <remarks>
		///		By default, when loading the Mesh, static, write-only vertex and index buffers 
		///		will be used where possible in order to improve rendering performance. 
		///		However, such buffers
		///		cannot be manipulated on the fly by CPU code (although shader code can). If you
		///		wish to use the CPU to modify these buffers, you should call this method. Note,
		///		however, that it only takes effect after the Mesh has been reloaded. Note that you
		///		still have the option of manually repacing the buffers in this mesh with your
		///		own if you see fit too, in which case you don't need to call this method since it
		///		only affects buffers created by the mesh itself.
		///		<p/>
		///		You can define the approach to a Mesh by changing the default parameters to 
		///		<see cref="MeshManager.Load"/> if you wish; this means the Mesh is loaded with those options
		///		the first time instead of you having to reload the mesh after changing these options.
		/// </remarks>
		/// <param name="usage">The usage flags, which by default are <see cref="BufferUsage.StaticWriteOnly"/></param>
		/// <param name="useShadowBuffer">
		///		If set to true, the vertex buffers will be created with a
		///		system memory shadow buffer. You should set this if you want to be able to
		///		read from the buffer, because reading from a hardware buffer is a no-no.
		/// </param>
		public void SetVertexBufferPolicy(BufferUsage usage, bool useShadowBuffer) {
			vertexBufferUsage = usage;
			useVertexShadowBuffer = useShadowBuffer;
		}

		/// <summary>
		///		Sets the policy for the index buffers to be used when loading this Mesh.
		/// </summary>
		/// <remarks>
		///		By default, when loading the Mesh, static, write-only vertex and index buffers 
		///		will be used where possible in order to improve rendering performance. 
		///		However, such buffers
		///		cannot be manipulated on the fly by CPU code (although shader code can). If you
		///		wish to use the CPU to modify these buffers, you should call this method. Note,
		///		however, that it only takes effect after the Mesh has been reloaded. Note that you
		///		still have the option of manually repacing the buffers in this mesh with your
		///		own if you see fit too, in which case you don't need to call this method since it
		///		only affects buffers created by the mesh itself.
		///		<p/>
		///		You can define the approach to a Mesh by changing the default parameters to 
		///		<see cref="MeshManager.Load"/> if you wish; this means the Mesh is loaded with those options
		///		the first time instead of you having to reload the mesh after changing these options.
		/// </remarks>
		/// <param name="usage">The usage flags, which by default are <see cref="BufferUsage.StaticWriteOnly"/></param>
		/// <param name="useShadowBuffer">
		///		If set to true, the index buffers will be created with a
		///		system memory shadow buffer. You should set this if you want to be able to
		///		read from the buffer, because reading from a hardware buffer is a no-no.
		/// </param>
		public void SetIndexBufferPolicy(BufferUsage usage, bool useShadowBuffer) {
			indexBufferUsage = usage;
			useIndexShadowBuffer = useShadowBuffer;
		}

        /// <summary>
        ///   This method is fairly internal, and is used to add new manual lod info
        /// </summary>
        /// <param name="manualLodEntries"></param>
        public void AddManualLodEntries(List<MeshLodUsage> manualLodEntries) {
            Debug.Assert(lodUsageList.Count == 1);
            isLodManual = true;
            foreach (MeshLodUsage usage in manualLodEntries)
                lodUsageList.Add(usage);
        }

        /// <summary>
        ///   TODO: should this replace an existing attachment point with the same name?
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public virtual AttachmentPoint CreateAttachmentPoint(string name, Quaternion rotation, Vector3 translation) {
            AttachmentPoint ap = new AttachmentPoint(name, null, rotation, translation);
            attachmentPoints.Add(ap);
            return ap;
        }


		/// <summary>
		///	    Internal method which, if animation types have not been determined,
		///	    scans any vertex animations and determines the type for each set of
		///	    vertex data (cannot have 2 different types).
		/// </summary>
		public void DetermineAnimationTypes() {
			// Don't check flag here; since detail checks on track changes are not
			// done, allow caller to force if they need to

			// Initialise all types to nothing
			sharedVertexDataAnimationType = VertexAnimationType.None;
			for(int sm = 0; sm < this.SubMeshCount; sm++) {
				SubMesh subMesh = GetSubMesh(sm);
				subMesh.VertexAnimationType = VertexAnimationType.None;
			}
			
			// Scan all animations and determine the type of animation tracks
			// relating to each vertex data
			foreach(Animation anim in animationsList.Values) {
				foreach(VertexAnimationTrack track in anim.VertexTracks.Values) {
					ushort handle = track.Handle;
					if (handle == 0)
					{
						// shared data
						if (sharedVertexDataAnimationType != VertexAnimationType.None &&
							sharedVertexDataAnimationType != track.AnimationType) {
							// Mixing of morph and pose animation on same data is not allowed
							throw new Exception("Animation tracks for shared vertex data on mesh "
												+ name + " try to mix vertex animation types, which is " +
												"not allowed, in Mesh.DetermineAnimationTypes");
						}
						sharedVertexDataAnimationType = track.AnimationType;
					}
					else {
						// submesh index (-1)
						SubMesh sm = GetSubMesh(handle-1);
						if (sm.CurrentVertexAnimationType != VertexAnimationType.None &&
							sm.CurrentVertexAnimationType != track.AnimationType) {
							// Mixing of morph and pose animation on same data is not allowed
							throw new Exception(string.Format("Animation tracks for dedicated vertex data {0}  on mesh {1}",
                                                              handle-1, name) +
												" try to mix vertex animation types, which is " +
												"not allowed, in Mesh.DetermineAnimationTypes");
						}
						sm.VertexAnimationType = track.AnimationType;
					}
				}
			}

			animationTypesDirty = false;
		}

        /// <summary>
        ///     Creates a new Animation object for vertex animating this mesh. 
        /// </summary>
        /// <param name="name">The name of this animation</param>
        /// <param name="length">The length of the animation in seconds</param>
		public Animation CreateAnimation(string name, float length) {
			// Check name not used
			if (animationsList.ContainsKey(name)) {
				throw new Exception("An animation with the name " + name + " already exists" +
									", in Mesh.CreateAnimation");
			}
			Animation ret = new Animation(name, length);
			// Add to list
			animationsList[name] = ret;
			// Mark animation types dirty
			animationTypesDirty = true;
			return ret;
		}

        /// <summary>
        ///     Returns the named vertex Animation object. 
        /// </summary>
        /// <param name="name">The name of the animation</param>
		public Animation GetAnimation(string name) {
			Animation ret;
			if (!animationsList.TryGetValue(name, out ret))
                return null;
			return ret;
		}

		/// <summary>Gets a single morph animation by index.</summary>
        // ??? Not sure this is right - - it's depending on the order of 
        // ??? insertion, which seems really wrong for a dictionary
		public Animation GetAnimation(ushort index) {
			// If you hit this assert, then the index is out of bounds.
			Debug.Assert(index < animationsList.Count);
			// ??? The only way I can figure out to do this is with
			// ??? a loop over the elements.
			ushort i = 0;
			foreach(Animation animation in animationsList.Values) {
				if (i == index)
					return animation;
				i++;
			}
			// Make compiler happy
			return null;
		}

		/// <summary>Returns whether this mesh contains the named vertex animation.</summary>
		public bool HasAnimation(string name) {
			return animationsList.ContainsKey(name);
		}

		/// <summary>Removes vertex Animation from this mesh.</summary>
		public void RemoveAnimation(string name) {
			if (!HasAnimation(name)) {
				throw new Exception("No animation entry found named " + name +
									", in Mesh.RemoveAnimation");
			}
			animationsList.Remove(name);
			animationTypesDirty = true;
		}

		/// <summary>Removes all morph Animations from this mesh.</summary>
		public void RemoveAllAnimations() {
			animationsList.Clear();
			animationTypesDirty = true;
		}

		/// <summary>
		///     Gets a pointer to a vertex data element based on a morph animation 
		///    	track handle.
		/// </summary>
		/// <remarks>
		///	    0 means the shared vertex data, 1+ means a submesh vertex data (index+1)
		/// </remarks>
		public VertexData GetVertexDataByTrackHandle(ushort handle) {
			if (handle == 0)
				return sharedVertexData;
			else
				return GetSubMesh(handle-1).vertexData;
		}

		/// <summary>
		///     Create a new Pose for this mesh or one of its submeshes.
		/// </summary>
		/// <param name="target">
		///     The target geometry index; 0 is the shared Mesh geometry, 1+ is the
		///    	dedicated SubMesh geometry belonging to submesh index + 1.
		/// </param>	
		/// <param name="name">Name to give the pose, which is optional</param>
		/// <returns>A new Pose ready for population</returns>
		public Pose CreatePose(ushort target, string name) {
			Pose retPose = new Pose(target, name);
			PoseList.Add(retPose);
			return retPose;
		}

		/// <summary>Retrieve an existing Pose by index.</summary>
		public Pose GetPose(ushort index) {
			if (index >= PoseList.Count)
				throw new Exception("Index out of bounds, in Mesh.GetPose");
			return poseList[index];
		}

		/// <summary>Retrieve an existing Pose by name.</summary>
		public Pose GetPose(string name) {
			foreach (Pose pose in PoseList) {
				if (pose.Name == name)
					return pose;
			}
			throw new Exception("No pose called " + name + " found in Mesh " + name +
								", in Mesh.GetPose");
		}
         /// <summary>Retrieve an existing Pose index by name.</summary>
         public ushort GetPoseIndex(string name)
         {
             for (ushort i = 0; i < PoseList.Count; i++)
             {
                 if (PoseList[i].Name == name)
                     return i;
             }
             throw new Exception("No pose called " + name + " found in Mesh " + this.name +
                                 ", in Mesh.GetPoseIndex");
         }
 
		/// <summary>Destroy a pose by index.</summary>
		/// <remarks>This will invalidate any animation tracks referring to this pose or those after it.</remarks>
		public void RemovePose(ushort index) {
			if (index >= poseList.Count) {
				throw new Exception("Index out of bounds, in Mesh.RemovePose");
			}
			PoseList.RemoveAt(index);
		}

		/// <summary>Destroy a pose by name.</summary>
		/// <remarks>This will invalidate any animation tracks referring to this pose or those after it.</remarks>
		public void RemovePose(string name) {
			for (int i=0; i<poseList.Count; i++) {
				Pose pose = PoseList[i];
				if (pose.Name == name) {
					PoseList.RemoveAt(i);
					return;
				}
			}
			throw new Exception("No pose called " + name + " found in Mesh " + name +
								"Mesh.RemovePose");
		}

		/// <summary>Destroy all poses.</summary>
		public void RemoveAllPoses() {
			poseList.Clear();
		}

        #endregion Methods

		#region Static Methods

        /// <summary>
        ///   TODO: Alignment issues with floats
        /// </summary>
        /// <param name="sourceElements"></param>
        /// <param name="sourceBuffer"></param>
        /// <param name="targetElements"></param>
        /// <param name="targetBuffer"></param>
        /// <param name="skinContext"></param>
        protected static void CopyVertexBuffer(List<VertexElement> sourceElements, HardwareVertexBuffer sourceBuffer,
                                               List<VertexElement> targetElements, HardwareVertexBuffer targetBuffer,
                                               MeshSkinningContext skinContext) 
        {
            IntPtr sourcePtr = sourceBuffer.Lock(BufferLocking.ReadOnly);
            IntPtr targetPtr = targetBuffer.Lock(BufferLocking.Discard);
            for (int i = 0; i < sourceElements.Count; ++i) {
                VertexElement sourceElement = sourceElements[i];
                VertexElement targetElement = targetElements[i];
                unsafe {
                    byte* pSrcBase = (byte*)sourcePtr.ToPointer();
                    byte* pDstBase = (byte*)targetPtr.ToPointer();
                    switch (sourceElement.Semantic) {
                        case VertexElementSemantic.Position: {
                                float* pfSrcBase = (float*)pSrcBase;
                                float* pfDstBase = (float*)pDstBase;
                                for (int vertexIndex = 0; vertexIndex < sourceBuffer.VertexCount; ++vertexIndex) {
                                    int srcVertOffset = vertexIndex * sourceBuffer.VertexSize + sourceElement.Offset;
                                    int dstVertOffset = vertexIndex * targetBuffer.VertexSize + targetElement.Offset;
                                    int srcOffsetFloat = srcVertOffset / sizeof(float);
                                    int dstOffsetFloat = dstVertOffset / sizeof(float);
                                    Vector3 srcVec = new Vector3(pfSrcBase[srcOffsetFloat], pfSrcBase[srcOffsetFloat + 1], pfSrcBase[srcOffsetFloat + 2]);
                                    Vector3 dstVec = Vector3.Zero;
                                    foreach (VertexBoneAssignment vba in skinContext.vertexWeights[vertexIndex])
                                        BlendPosVector(ref dstVec, ref skinContext.boneMatrices[skinContext.boneIndexMap[vba.boneIndex]], ref srcVec, vba.weight);
                                    pfDstBase[dstOffsetFloat] = dstVec.x;
                                    pfDstBase[dstOffsetFloat + 1] = dstVec.y;
                                    pfDstBase[dstOffsetFloat + 2] = dstVec.z;
                                }
                            }
                            break;
                        case VertexElementSemantic.Normal: {
                                float* pfSrcBase = (float*)pSrcBase;
                                float* pfDstBase = (float*)pDstBase;
                                for (int vertexIndex = 0; vertexIndex < sourceBuffer.VertexCount; ++vertexIndex) {
                                    int srcVertOffset = vertexIndex * sourceBuffer.VertexSize + sourceElement.Offset;
                                    int dstVertOffset = vertexIndex * targetBuffer.VertexSize + targetElement.Offset;
                                    int srcOffsetFloat = srcVertOffset / sizeof(float);
                                    int dstOffsetFloat = dstVertOffset / sizeof(float);
                                    Vector3 srcVec = new Vector3(pfSrcBase[srcOffsetFloat], pfSrcBase[srcOffsetFloat + 1], pfSrcBase[srcOffsetFloat + 2]);
                                    Vector3 dstVec = Vector3.Zero;
                                    foreach (VertexBoneAssignment vba in skinContext.vertexWeights[vertexIndex])
                                        BlendDirVector(ref dstVec, ref skinContext.inverseTransposeBoneMatrices[skinContext.boneIndexMap[vba.boneIndex]], ref srcVec, vba.weight);
                                    pfDstBase[dstOffsetFloat] = dstVec.x;
                                    pfDstBase[dstOffsetFloat + 1] = dstVec.y;
                                    pfDstBase[dstOffsetFloat + 2] = dstVec.z;
                                }
                            }
                            break;
                        case VertexElementSemantic.Tangent:
                        case VertexElementSemantic.Binormal: {
                                float* pfSrcBase = (float*)pSrcBase;
                                float* pfDstBase = (float*)pDstBase;
                                for (int vertexIndex = 0; vertexIndex < sourceBuffer.VertexCount; ++vertexIndex) {
                                    int srcVertOffset = vertexIndex * sourceBuffer.VertexSize + sourceElement.Offset;
                                    int dstVertOffset = vertexIndex * targetBuffer.VertexSize + targetElement.Offset;
                                    int srcOffsetFloat = srcVertOffset / sizeof(float);
                                    int dstOffsetFloat = dstVertOffset / sizeof(float);
                                    Vector3 srcVec = new Vector3(pfSrcBase[srcOffsetFloat], pfSrcBase[srcOffsetFloat + 1], pfSrcBase[srcOffsetFloat + 2]);
                                    Vector3 dstVec = Vector3.Zero;
                                    foreach (VertexBoneAssignment vba in skinContext.vertexWeights[vertexIndex])
                                        BlendDirVector(ref dstVec, ref skinContext.boneMatrices[skinContext.boneIndexMap[vba.boneIndex]], ref srcVec, vba.weight);
                                    pfDstBase[dstOffsetFloat] = dstVec.x;
                                    pfDstBase[dstOffsetFloat + 1] = dstVec.y;
                                    pfDstBase[dstOffsetFloat + 2] = dstVec.z;
                                }
                            }
                            break;
                        default:
                            for (int vertexIndex = 0; vertexIndex < sourceBuffer.VertexCount; ++vertexIndex) {
                                int srcVertOffset = vertexIndex * sourceBuffer.VertexSize + sourceElement.Offset;
                                int dstVertOffset = vertexIndex * targetBuffer.VertexSize + targetElement.Offset;
                                for (int offset = 0; offset < sourceElement.Size; ++offset)
                                    pDstBase[dstVertOffset + offset] = pSrcBase[srcVertOffset + offset];
                            }
                            break;
                    }
                }
            }
            targetBuffer.Unlock();
            sourceBuffer.Unlock();
        }

		/// <summary>
		///		Performs a software indexed vertex blend, of the kind used for
		///		skeletal animation although it can be used for other purposes. 
		/// </summary>
		/// <remarks>
		///		This function is supplied to update vertex data with blends 
		///		done in software, either because no hardware support is available, 
		///		or that you need the results of the blend for some other CPU operations.
		/// </remarks>
		/// <param name="sourceVertexData">
		///		<see cref="VertexData"/> class containing positions, normals, blend indices and blend weights.
		///	</param>
		/// <param name="targetVertexData">
		///		<see cref="VertexData"/> class containing target position
		///		and normal buffers which will be updated with the blended versions.
		///		Note that the layout of the source and target position / normal 
		///		buffers must be identical, ie they must use the same buffer indexes.
		/// </param>
		/// <param name="matrices">An array of matrices to be used to blend.</param>
		/// <param name="blendNormals">If true, normals, binormals and tangents are blended as well as positions.</param>
#if !OLD_WAY
        public static void SoftwareVertexBlend(VertexData sourceVertexData, VertexData targetVertexData, 
                                               Matrix4[] matrices, List<ushort> indexMap, bool blendNormals) {
            MeshSkinningContext skinContext = new MeshSkinningContext();
            skinContext.boneMatrices = matrices;
            skinContext.boneIndexMap = indexMap;
            GetBoneSkinningData(skinContext, sourceVertexData);
            bool needInverseTransposeBindMatrices = false;
            List<VertexElement> sourceElements = new List<VertexElement>();
            List<VertexElement> targetElements = new List<VertexElement>();
            // Build a dictionary with the vertex elements we intend to copy for each vertex buffer
            Dictionary<ushort, List<VertexElement>> sourceElementMap = new Dictionary<ushort, List<VertexElement>>();
            Dictionary<ushort, List<VertexElement>> targetElementMap = new Dictionary<ushort, List<VertexElement>>();
            for (int elementIndex = 0; elementIndex < sourceVertexData.vertexDeclaration.ElementCount; ++elementIndex) {
                VertexElement sourceElement = 
                    sourceVertexData.vertexDeclaration.GetElement(elementIndex);
                if (!blendNormals &&
                    (sourceElement.Semantic == VertexElementSemantic.Normal ||
                     sourceElement.Semantic == VertexElementSemantic.Tangent ||
                     sourceElement.Semantic == VertexElementSemantic.Binormal))
                    continue;
                else if (sourceElement.Semantic == VertexElementSemantic.BlendIndices ||
                         sourceElement.Semantic == VertexElementSemantic.BlendWeights)
                    continue;
                VertexElement targetElement = 
                    targetVertexData.vertexDeclaration.FindElementBySemantic(sourceElement.Semantic, (short)sourceElement.Index);
                if (!sourceElementMap.ContainsKey(sourceElement.Source))
                    sourceElementMap[sourceElement.Source] = new List<VertexElement>();
                if (!targetElementMap.ContainsKey(targetElement.Source))
                    targetElementMap[targetElement.Source] = new List<VertexElement>();
                sourceElementMap[sourceElement.Source].Add(sourceElement);
                targetElementMap[targetElement.Source].Add(targetElement);
                if (sourceElement.Semantic == VertexElementSemantic.Normal)
                    needInverseTransposeBindMatrices = true;
            }
            if (needInverseTransposeBindMatrices) {
                // Build the inverse transpose skin matrices
                skinContext.inverseTransposeBoneMatrices = new Matrix4[skinContext.boneMatrices.Length];
                for (int boneId = 0; boneId < skinContext.boneMatrices.Length; ++boneId)
                    skinContext.inverseTransposeBoneMatrices[boneId] = skinContext.boneMatrices[boneId].Inverse().Transpose();
            }
            foreach (ushort key in sourceElementMap.Keys) {
                HardwareVertexBuffer sourceBuffer = sourceVertexData.vertexBufferBinding.GetBuffer(key);
                HardwareVertexBuffer targetBuffer = targetVertexData.vertexBufferBinding.GetBuffer(key);
                // Sometimes the setup will have set the source buffer and the target buffer to be the same.
                // If that is the case, we assume we don't need to do anything to the target buffer.
                if (sourceBuffer != targetBuffer)
                    CopyVertexBuffer(sourceElementMap[key], sourceBuffer, targetElementMap[key], targetBuffer, skinContext);
            }
        }

        protected static void GetBoneSkinningData(MeshSkinningContext skinContext, VertexData sourceVertexData) {
            VertexElement srcElemBlendIndices = 
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.BlendIndices);
            VertexElement srcElemBlendWeights = 
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.BlendWeights);

            if( null == srcElemBlendIndices ) {
                // TODO: This will need to be reconsidered if we support parenting 
                // geometry to a skeleton.
                throw new AxiomException(
                    "Failed to find skinning data. One thing that can cause this\n" +
                    "condition is parenting a mesh to a bone in a skinned skeleton.\n" +
                    "Parenting is currently unsupported functionality." );
            }

            // Indices must be 4 bytes
            Debug.Assert(srcElemBlendIndices.Type == VertexElementType.UByte4,
                "Blend indices must be VET_UBYTE4");

            HardwareVertexBuffer srcIdxBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemBlendIndices.Source);
            HardwareVertexBuffer srcWeightBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemBlendWeights.Source);

            unsafe {
                IntPtr ptr;
                byte* pBlendIdx;
                float* pBlendWeight;

                if (srcWeightBuf == srcIdxBuf) {
                    ptr = srcIdxBuf.Lock(BufferLocking.ReadOnly);
                    pBlendIdx = (byte*)ptr.ToPointer();
                    pBlendWeight = (float*)ptr;
                } else {
                    // Lock buffer
                    ptr = srcIdxBuf.Lock(BufferLocking.ReadOnly);
                    pBlendIdx = (byte*)ptr.ToPointer();
                    ptr = srcWeightBuf.Lock(BufferLocking.ReadOnly);
                    pBlendWeight = (float*)ptr.ToPointer();
                }

                int numWeightsPerVertex = VertexElement.GetTypeCount(srcElemBlendWeights.Type);
                for (int vertexIndex = 0; vertexIndex < sourceVertexData.vertexCount; ++vertexIndex) {
                    int blendIdxOffset = vertexIndex * srcIdxBuf.VertexSize + srcElemBlendIndices.Offset;
                    int blendWeightOffset = vertexIndex * srcWeightBuf.VertexSize + srcElemBlendWeights.Offset;
                    List<VertexBoneAssignment> vbaList = new List<VertexBoneAssignment>();
                    for (int influenceIndex = 0; influenceIndex < numWeightsPerVertex; ++influenceIndex) {
                        VertexBoneAssignment vba = new VertexBoneAssignment();
                        vba.vertexIndex = vertexIndex;
                        vba.boneIndex = pBlendIdx[blendIdxOffset + influenceIndex];
                        vba.weight = pBlendWeight[blendWeightOffset / sizeof(float) + influenceIndex];
                        vbaList.Add(vba);
                    }
                    skinContext.vertexWeights[vertexIndex] = vbaList;
                }

                if (srcWeightBuf == srcIdxBuf) {
                    srcIdxBuf.Unlock();
                } else {
                    srcIdxBuf.Unlock();
                    srcWeightBuf.Unlock();
                }
            }
        }
#else
        public static void SoftwareVertexBlend(VertexData sourceVertexData, VertexData targetVertexData, Matrix4[] matrices, bool blendNormals) {
            SoftwareVertexBlend(sourceVertexData, targetVertexData, matrices, blendNormals, blendNormals, blendNormals);
        }

        public static void SoftwareVertexBlend(VertexData sourceVertexData, VertexData targetVertexData, Matrix4[] matrices, bool blendNormals, bool blendTangents, bool blendBinorms) {
// Source vectors
			Vector3 sourcePos = Vector3.Zero;
            Vector3 sourceNorm = Vector3.Zero;
            Vector3 sourceTan = Vector3.Zero;
            Vector3 sourceBinorm = Vector3.Zero;
            // Accumulation vectors
			Vector3 accumVecPos = Vector3.Zero;
            Vector3 accumVecNorm = Vector3.Zero;
            Vector3 accumVecTan = Vector3.Zero;
            Vector3 accumVecBinorm = Vector3.Zero;

			HardwareVertexBuffer srcPosBuf = null, srcNormBuf = null, srcTanBuf = null, srcBinormBuf = null;
            HardwareVertexBuffer destPosBuf = null, destNormBuf = null, destTanBuf = null, destBinormBuf = null;
            HardwareVertexBuffer srcIdxBuf = null, srcWeightBuf = null;

			bool weightsIndexesShareBuffer = false;

			IntPtr ptr = IntPtr.Zero;

			// Get elements for source
			VertexElement srcElemPos = 
				sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
            VertexElement srcElemNorm =
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Normal);
            VertexElement srcElemDiffuse =
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Diffuse);
            VertexElement srcElemTex =
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.TexCoords);
            VertexElement srcElemTan =
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Tangent);
            VertexElement srcElemBinorm =
                sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Binormal);
            VertexElement srcElemBlendIndices = 
				sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.BlendIndices);
			VertexElement srcElemBlendWeights = 
				sourceVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.BlendWeights);

			Debug.Assert(srcElemPos != null && srcElemBlendIndices != null && srcElemBlendWeights != null, "You must supply at least positions, blend indices and blend weights");

			// Get elements for target
			VertexElement destElemPos = 
				targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
            VertexElement destElemNorm =
                targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Normal);
            VertexElement destElemDiffuse =
                targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Diffuse);
            VertexElement destElemTex =
                targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.TexCoords);
            VertexElement destElemTan =
                targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Tangent);
            VertexElement destElemBinorm =
                targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Binormal);
        
			// Do we have normals and want to blend them?
            bool includeNormals = blendNormals && (srcElemNorm != null) && (destElemNorm != null);
            bool includeDiffuse = (srcElemDiffuse != null) && (destElemDiffuse != null) &&
                                  (srcElemPos.Source == srcElemDiffuse.Source) &&
                                  (destElemPos.Source == destElemDiffuse.Source);
            bool includeTexCoords = (srcElemTex != null) && (destElemTex != null) &&
                                    (srcElemPos.Source == srcElemTex.Source) &&
                                    (destElemPos.Source == destElemTex.Source);
            bool includeTangents = blendTangents && (srcElemTan != null) && (destElemTan != null);
            bool includeBinormals = blendBinorms && (srcElemBinorm != null) && (destElemBinorm != null);

			// Get buffers for source
			srcPosBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemPos.Source);
			srcIdxBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemBlendIndices.Source);
			srcWeightBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemBlendWeights.Source);

            if (includeNormals)
                srcNormBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemNorm.Source);
            if (includeTangents)
                srcTanBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemTan.Source);
            if (includeBinormals)
                srcBinormBuf = sourceVertexData.vertexBufferBinding.GetBuffer(srcElemBinorm.Source);

			// note: reference comparison
			weightsIndexesShareBuffer = (srcIdxBuf == srcWeightBuf);

			// Get buffers for target
			destPosBuf = targetVertexData.vertexBufferBinding.GetBuffer(destElemPos.Source);
			if (includeNormals)
				destNormBuf = targetVertexData.vertexBufferBinding.GetBuffer(destElemNorm.Source);
            if (includeTangents)
                destTanBuf = targetVertexData.vertexBufferBinding.GetBuffer(destElemTan.Source);
            if (includeBinormals)
                destBinormBuf = targetVertexData.vertexBufferBinding.GetBuffer(destElemBinorm.Source);

			// Lock source buffers for reading
			Debug.Assert(srcElemPos.Offset == 0, "Positions must be first element in dedicated buffer!");

			unsafe {
                float* pSrcPos = null, pSrcNorm = null, pSrcTan = null, pSrcBinorm = null, pSrcTex = null, pSrcDiffuse = null;
                float* pDestPos = null, pDestNorm = null, pDestTan = null, pDestBinorm = null, pDestTex = null, pDestDiffuse = null;
                float* pBlendWeight = null;
				byte* pBlendIdx = null;
			
				ptr = srcPosBuf.Lock(BufferLocking.ReadOnly);
				pSrcPos = (float*)ptr.ToPointer();
                if (includeNormals) {
                    if (srcNormBuf == srcPosBuf)
                        pSrcNorm = pSrcPos;
                    else {
                        ptr = srcNormBuf.Lock(BufferLocking.ReadOnly);
                        pSrcNorm = (float*)ptr.ToPointer();
                    }
                }
                if (includeTangents) {
                    if (srcTanBuf == srcPosBuf)
                        pSrcTan = pSrcPos;
                    else if (srcTanBuf == srcNormBuf)
                        pSrcTan = pSrcNorm;
                    else {
                        ptr = srcTanBuf.Lock(BufferLocking.ReadOnly);
                        pSrcTan = (float*)ptr.ToPointer();
                    }
                }
                if (includeBinormals) {
                    if (srcBinormBuf == srcPosBuf)
                        pSrcBinorm = pSrcPos;
                    else if (srcBinormBuf == srcNormBuf)
                        pSrcBinorm = pSrcNorm;
                    else if (srcBinormBuf == srcTanBuf)
                        pSrcBinorm = pSrcTan;
                    else {
                        ptr = srcBinormBuf.Lock(BufferLocking.ReadOnly);
                        pSrcBinorm = (float*)ptr.ToPointer();
                    }
                }

				// Indices must be 4 bytes
				Debug.Assert(srcElemBlendIndices.Type == VertexElementType.UByte4, 
					"Blend indices must be VET_UBYTE4");

				ptr = srcIdxBuf.Lock(BufferLocking.ReadOnly);
				pBlendIdx = (byte*)ptr.ToPointer();

                if (srcWeightBuf == srcIdxBuf)
					pBlendWeight = (float*)pBlendIdx;
				else {
					// Lock buffer
					ptr = srcWeightBuf.Lock(BufferLocking.ReadOnly);
					pBlendWeight = (float*)ptr.ToPointer();
				}

				int numWeightsPerVertex = VertexElement.GetTypeCount(srcElemBlendWeights.Type);

				// Lock destination buffers for writing
				ptr = destPosBuf.Lock(BufferLocking.Discard);
				pDestPos = (float*)ptr.ToPointer();

				if (includeNormals) {
                    if (destNormBuf == destPosBuf)
                        pDestNorm = pDestPos;
                    else {
					    ptr = destNormBuf.Lock(BufferLocking.Discard);
					    pDestNorm = (float*)ptr.ToPointer();
                    }
				}
				if (includeTangents) {
                    if (destTanBuf == destPosBuf)
                        pDestTan = pDestPos;
                    else if (destTanBuf == destNormBuf)
                        pDestTan = pDestNorm;
                    else {
					    ptr = destTanBuf.Lock(BufferLocking.Discard);
					    pDestTan = (float*)ptr.ToPointer();
                    }
				}
				if (includeBinormals) {
                    if (destBinormBuf == destPosBuf)
                        pDestBinorm = pDestPos;
                    else if (destBinormBuf == destNormBuf)
                        pDestBinorm = pDestNorm;
                    else if (destBinormBuf == destTanBuf)
                        pDestBinorm = pDestTan;
                    else {
					    ptr = destBinormBuf.Lock(BufferLocking.Discard);
					    pDestBinorm = (float*)ptr.ToPointer();
                    }
				}

				// Loop per vertex
				for(int vertIdx = 0; vertIdx < targetVertexData.vertexCount; vertIdx++) {
                    int srcPosOffset = (vertIdx * srcPosBuf.VertexSize + srcElemPos.Offset) / 4;
					// Load source vertex elements
                    sourcePos.x = pSrcPos[srcPosOffset];
                    sourcePos.y = pSrcPos[srcPosOffset + 1];
                    sourcePos.z = pSrcPos[srcPosOffset + 2];

                    if (includeNormals) {
                        int srcNormOffset = (vertIdx * srcNormBuf.VertexSize + srcElemNorm.Offset) / 4;
                        sourceNorm.x = pSrcNorm[srcNormOffset];
                        sourceNorm.y = pSrcNorm[srcNormOffset + 1];
                        sourceNorm.z = pSrcNorm[srcNormOffset + 2];
                    }

                    if (includeTangents) {
                        int srcTanOffset = (vertIdx * srcTanBuf.VertexSize + srcElemTan.Offset) / 4;
                        sourceTan.x = pSrcTan[srcTanOffset];
                        sourceTan.y = pSrcTan[srcTanOffset + 1];
                        sourceTan.z = pSrcTan[srcTanOffset + 2];
                    }

                    if (includeBinormals) {
                        int srcBinormOffset = (vertIdx * srcBinormBuf.VertexSize + srcElemBinorm.Offset) / 4;
                        sourceBinorm.x = pSrcBinorm[srcBinormOffset];
                        sourceBinorm.y = pSrcBinorm[srcBinormOffset + 1];
                        sourceBinorm.z = pSrcBinorm[srcBinormOffset + 2];
                    }

                    // Load accumulators
					accumVecPos = Vector3.Zero;
					accumVecNorm = Vector3.Zero;
                    accumVecTan = Vector3.Zero;
                    accumVecBinorm = Vector3.Zero;

                    int blendWeightOffset = (vertIdx * srcWeightBuf.VertexSize + srcElemBlendWeights.Offset) / 4;
                    int blendMatrixOffset = vertIdx * srcIdxBuf.VertexSize + srcElemBlendIndices.Offset;
                    // Loop per blend weight 
                    for (int blendIdx = 0; blendIdx < numWeightsPerVertex; blendIdx++) {
                        float blendWeight = pBlendWeight[blendWeightOffset + blendIdx];
                        int blendMatrixIdx = pBlendIdx[blendMatrixOffset + blendIdx];
                        // Blend by multiplying source by blend matrix and scaling by weight
                        // Add to accumulator
                        // NB weights must be normalised!!
                        if (blendWeight != 0.0f) {
                            // Blend position, use 3x4 matrix
                            Matrix4 mat = matrices[blendMatrixIdx];
                            BlendPosVector(ref accumVecPos, ref mat, ref sourcePos, blendWeight);

                            if (includeNormals) {
                                // Blend normal
                                // We should blend by inverse transpose here, but because we're assuming the 3x3
                                // aspect of the matrix is orthogonal (no non-uniform scaling), the inverse transpose
                                // is equal to the main 3x3 matrix
                                // Note because it's a normal we just extract the rotational part, saves us renormalising here
                                BlendDirVector(ref accumVecNorm, ref mat, ref sourceNorm, blendWeight);
                            }
                            if (includeTangents) {
                                BlendDirVector(ref accumVecTan, ref mat, ref sourceTan, blendWeight);
                            }
                            if (includeBinormals) {
                                BlendDirVector(ref accumVecBinorm, ref mat, ref sourceBinorm, blendWeight);
                            }

                        }
                    }

					// Stored blended vertex in hardware buffer
                    int dstPosOffset = (vertIdx * destPosBuf.VertexSize + destElemPos.Offset) / 4;
					pDestPos[dstPosOffset] = accumVecPos.x;
                    pDestPos[dstPosOffset + 1] = accumVecPos.y;
                    pDestPos[dstPosOffset + 2] = accumVecPos.z;

                    // Copy the texture coordinates and diffuse from the 
                    // position buffer if they are in there.
                    if (includeTexCoords) {
                        int srcTexOffset = (vertIdx * srcPosBuf.VertexSize + srcElemTex.Offset) / 4;
                        int dstTexOffset = (vertIdx * destPosBuf.VertexSize + destElemTex.Offset) / 4;
                        pDestPos[dstTexOffset] = pSrcPos[srcTexOffset];
                        pDestPos[dstTexOffset + 1] = pSrcPos[srcTexOffset + 1];
                    }
                    if (includeDiffuse) {
                        int srcDiffuseOffset = (vertIdx * srcPosBuf.VertexSize + srcElemDiffuse.Offset) / 4;
                        int dstDiffuseOffset = (vertIdx * destPosBuf.VertexSize + destElemDiffuse.Offset) / 4;
                        pDestPos[dstDiffuseOffset] = pSrcPos[srcDiffuseOffset];
                    }

                    // Stored blended vertex in temp buffer
                    if (includeNormals) {
                        // Normalise
                        accumVecNorm.Normalize();
                        int dstNormOffset = (vertIdx * destNormBuf.VertexSize + destElemNorm.Offset) / 4;
                        pDestNorm[dstNormOffset] = accumVecNorm.x;
                        pDestNorm[dstNormOffset + 1] = accumVecNorm.y;
                        pDestNorm[dstNormOffset + 2] = accumVecNorm.z;
                    }
                    // Stored blended vertex in temp buffer
                    if (includeTangents) {
                        // Normalise
                        accumVecTan.Normalize();
                        int dstTanOffset = (vertIdx * destTanBuf.VertexSize + destElemTan.Offset) / 4;
                        pDestTan[dstTanOffset] = accumVecTan.x;
                        pDestTan[dstTanOffset + 1] = accumVecTan.y;
                        pDestTan[dstTanOffset + 2] = accumVecTan.z;
                    }
                    // Stored blended vertex in temp buffer
                    if (includeBinormals) {
                        // Normalise
                        accumVecBinorm.Normalize();
                        int dstBinormOffset = (vertIdx * destBinormBuf.VertexSize + destElemBinorm.Offset) / 4;
                        pDestBinorm[dstBinormOffset] = accumVecBinorm.x;
                        pDestBinorm[dstBinormOffset + 1] = accumVecBinorm.y;
                        pDestBinorm[dstBinormOffset + 2] = accumVecBinorm.z;
                    }

				}
				// Unlock source buffers
				srcPosBuf.Unlock();
				srcIdxBuf.Unlock();

                if (srcWeightBuf != srcIdxBuf) {
					srcWeightBuf.Unlock();
				}

                if (includeNormals &&
                    srcNormBuf != srcPosBuf) {
                    srcNormBuf.Unlock();
                }
                if (includeTangents &&
                    srcTanBuf != srcPosBuf &&
                    srcTanBuf != srcNormBuf) {
                    srcTanBuf.Unlock();
                }
                if (includeBinormals &&
                    srcBinormBuf != srcPosBuf &&
                    srcBinormBuf != srcNormBuf &&
                    srcBinormBuf != srcTanBuf) {
                    srcBinormBuf.Unlock();
                }
			
				// Unlock destination buffers
				destPosBuf.Unlock();

                if (includeNormals &&
                    destNormBuf != destPosBuf) {
                    destNormBuf.Unlock();
                }
                if (includeTangents &&
                    destTanBuf != destPosBuf &&
                    destTanBuf != destNormBuf) {
                    destTanBuf.Unlock();
                }
                if (includeBinormals &&
                    destBinormBuf != destPosBuf &&
                    destBinormBuf != destNormBuf &&
                    destBinormBuf != destTanBuf) {
                    destBinormBuf.Unlock();
                }

			} // unsafe
		}
#endif

        public static void BlendDirVector(ref Vector3 accumVec, ref Matrix4 mat, ref Vector3 srcVec, float blendWeight) {
            accumVec.x +=
                (mat.m00 * srcVec.x +
                 mat.m01 * srcVec.y +
                 mat.m02 * srcVec.z)
                * blendWeight;

            accumVec.y +=
                (mat.m10 * srcVec.x +
                 mat.m11 * srcVec.y +
                 mat.m12 * srcVec.z)
                * blendWeight;

            accumVec.z +=
                (mat.m20 * srcVec.x +
                 mat.m21 * srcVec.y +
                 mat.m22 * srcVec.z)
                * blendWeight;
        }

        public static void BlendPosVector(ref Vector3 accumVec, ref Matrix4 mat, ref Vector3 srcVec, float blendWeight) {
            accumVec.x +=
                (mat.m00 * srcVec.x +
                 mat.m01 * srcVec.y +
                 mat.m02 * srcVec.z +
                 mat.m03)
                * blendWeight;

            accumVec.y +=
                (mat.m10 * srcVec.x +
                 mat.m11 * srcVec.y +
                 mat.m12 * srcVec.z +
                 mat.m13)
                * blendWeight;

            accumVec.z +=
                (mat.m20 * srcVec.x +
                 mat.m21 * srcVec.y +
                 mat.m22 * srcVec.z +
                 mat.m23)
                * blendWeight;
        }

        /// <summary>
        ///     Performs a software vertex morph, of the kind used for
        ///     morph animation although it can be used for other purposes. 
	    /// </summary>
        /// <remarks>
		///   	This function will linearly interpolate positions between two
		/// 	source buffers, into a third buffer.
	    /// </remarks>
        /// <param name="t">Parametric distance between the start and end buffer positions</param>
        /// <param name="b1">Vertex buffer containing VertexElementType.Float3 entries for the start positions</param>
		/// <param name="b2">Vertex buffer containing VertexElementType.Float3 entries for the end positions</param>
		/// <param name="targetVertexData" VertexData destination; assumed to have a separate position
		///	     buffer already bound, and the number of vertices must agree with the
		///   number in start and end
        /// </param>
        public static void SoftwareVertexMorph(float t, HardwareVertexBuffer b1, 
											   HardwareVertexBuffer b2, VertexData targetVertexData) {
			unsafe {
				float* pb1 = (float *)b1.Lock(BufferLocking.ReadOnly);
				float* pb2 = (float *)b2.Lock(BufferLocking.ReadOnly);
				VertexElement posElem =
					targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
				Debug.Assert(posElem != null);
				HardwareVertexBuffer destBuf = targetVertexData.vertexBufferBinding.GetBuffer(posElem.Source);
                //Debug.Assert(posElem.Size == destBuf.VertexSize,
                //             "Positions must be in a buffer on their own for morphing");
                // I can't discard the existing contents, since I am only touching the position data
				float* pdst = (float *)destBuf.Lock(BufferLocking.Normal);
                Debug.Assert(posElem.Offset % sizeof(float) == 0, "Position data must be aligned with a word boundary");
                int posOffset = posElem.Offset / sizeof(float);
                int vertSize = destBuf.Size / sizeof(float);
				for (int i = 0; i < targetVertexData.vertexCount; ++i)
					for (int j = 0; j < 3; ++j)
                        pdst[posOffset + i * vertSize + j] = pb1[3 * i + j] + t * (pb2[3 * i + j] - pb1[3 * i + j]);
                destBuf.Unlock();
                b1.Unlock();
                b2.Unlock();
			}
        }

        /// <summary>
        ///     Performs a software vertex pose blend, of the kind used for
        ///     morph animation although it can be used for other purposes.
        /// </summary>
        /// <remarks>
		///     This function will apply a weighted offset to the positions in the 
		///     incoming vertex data (therefore this is a read/write operation, and 
		///     if you expect to call it more than once with the same data, then
		///     you would be best to suppress hardware uploads of the position buffer
		///     for the duration)
        /// </remarks>
        /// <param name="weight"Parametric weight to scale the offsets by</param>
		/// <param name="vertexOffsetMap" Potentially sparse map of vertex index -> offset</param>
		/// <param name="targetVertexData" VertexData destination; assumed to have a separate position
		///	    buffer already bound, and the number of vertices must agree with the
		///	    number in start and end
	    /// </param>
		public static void SoftwareVertexPoseBlend(float weight, Dictionary<int, Vector3> vertexOffsetMap,
				                                   VertexData targetVertexData) {
			// Do nothing if no weight
			if (weight == 0.0f)
				return;

			VertexElement posElem = targetVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
			Debug.Assert(posElem != null);
            HardwareVertexBuffer destBuf = targetVertexData.vertexBufferBinding.GetBuffer(posElem.Source);
            int posOffset = posElem.Offset / 4;
            int vertexSize = destBuf.VertexSize / 4;
            // Have to lock in normal mode since this is incremental
            unsafe {
			    float* pBase = (float *)destBuf.Lock(BufferLocking.Normal);
				// Iterate over affected vertices
				foreach (KeyValuePair<int, Vector3> pair in vertexOffsetMap) {
                    Debug.Assert(4 * (pair.Key * vertexSize + posOffset) + 12 < destBuf.Size);
					// Adjust pointer
					float *pdst = pBase + pair.Key * vertexSize + posOffset;
					*pdst = *pdst + (pair.Value.x * weight);
					++pdst;
					*pdst = *pdst + (pair.Value.y * weight);
					++pdst;
					*pdst = *pdst + (pair.Value.z * weight);
					++pdst;
				}
                destBuf.Unlock();
			}
		}

		#endregion Static Methods

        #region Implementation of Resource

        public override void Preload() {
            //if (isPreloaded) {
            //    return;
            //}

            // load this bad boy if it is not to be manually defined
            if (!isManual) {
                MeshSerializer serializer = new MeshSerializer();

                // get the resource data from MeshManager
                Stream data = MeshManager.Instance.FindResourceData(name);

                // fetch the .mesh dependency info
                serializer.GetDependencyInfo(data, this);

                // close the stream (we don't need to leave it open here)
                data.Close();
            }

        }

        /// <summary>
        ///		Loads the mesh data.
        /// </summary>
        public override void Load() {
            base.Load();

            // prepare the mesh for a shadow volume?
            if (MeshManager.Instance.PrepareAllMeshesForShadowVolumes) {
                if (edgeListsBuilt || autoBuildEdgeLists) {
                    PrepareForShadowVolume();
                }
                if (!edgeListsBuilt && autoBuildEdgeLists) {
                    BuildEdgeList();
                }
            }   
        }

                    /// <summary>
        ///		Loads the mesh data.
        /// </summary>
        protected override void LoadImpl() {
            // meshLoadMeter.Enter();

            // I should eventually call Preload here, and then use 
            // the preloaded data to make future loads faster, but
            // I haven't finished the Preload stuff yet.
            // Preload();
            
            MeshSerializer serializer = new MeshSerializer();

            // get the resource data from MeshManager
            Stream data = MeshManager.Instance.FindResourceData(name);

            // import the .mesh file
            serializer.ImportMesh(data, this);
			
            // check all submeshes to see if their materials should be
            // updated.  If the submesh has texture aliases that match those
            // found in the current material then a new material is created using
            // the textures from the submesh.
            // TODO: UpdateMaterialForAllSubMeshes();

            // close the stream (we don't need to leave it open here)
            data.Close();
            
            // meshLoadMeter.Exit();
        }

        /// <summary>
        ///		Unloads the mesh data.
        /// </summary>
        protected override void UnloadImpl() {
            sharedVertexData = null;
            // Clear SubMesh lists
            subMeshList.Clear();
            // subMeshNameMap.Clear();
            // Removes all LOD data
            RemoveLodLevels();
            isPreparedForShadowVolumes = false;
            
            // remove all poses & animations
            RemoveAllAnimations();
            RemoveAllPoses();
            
            // Clear bone assignments
            boneAssignmentList.Clear();
            boneAssignmentsOutOfDate = false;

            // Removes reference to skeleton
            SkeletonName = string.Empty;
        }

        #endregion
    }
    
    ///<summary>
    ///     A way of recording the way each LOD is recorded this Mesh.
    /// </summary>
    public class MeshLodUsage {
        ///	<summary>
        ///		Squared Z value from which this LOD will apply.
        ///	</summary>
        public float fromSquaredDepth;
         /// <summary>
         ///	Only relevant if isLodManual is true, the name of the alternative mesh to use.
         /// </summary>
 	    public string manualName;
        ///	<summary>
        ///		Reference to the manual mesh to avoid looking up each time.
        ///	</summary>    	
        public Mesh manualMesh;
		/// <summary>
		///		Edge list for this LOD level (may be derived from manual mesh).	
		/// </summary>
		public EdgeData edgeData;
    }
}
