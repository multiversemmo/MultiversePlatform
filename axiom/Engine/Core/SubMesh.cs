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
using Axiom.Animating;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.MathLib;

using Axiom.Graphics;

namespace Axiom.Core {
    /// <summary>
    ///		Defines a part of a complete 3D mesh.
    /// </summary>
    /// <remarks>
    ///		Models which make up the definition of a discrete 3D object
    ///		are made up of potentially multiple parts. This is because
    ///		different parts of the mesh may use different materials or
    ///		use different vertex formats, such that a rendering state
    ///		change is required between them.
    ///		<p/>
    ///		Like the Mesh class, instatiations of 3D objects in the scene
    ///		share the SubMesh instances, and have the option of overriding
    ///		their material differences on a per-object basis if required.
    ///		See the SubEntity class for more information.
    /// </remarks>
    public class SubMesh {
        #region Member variables

        /// <summary>The parent mesh that this subMesh belongs to.</summary>
        protected Mesh parent;
        /// <summary>Name of the material assigned to this subMesh.</summary>
        protected string materialName;
        /// <summary>Name of this SubMesh.</summary>
        internal string name;
        /// <summary></summary>
        protected bool isMaterialInitialized;
		
        /// <summary>List of bone assignment for this mesh.</summary>
        protected Dictionary<int, List<VertexBoneAssignment>> boneAssignmentList =
            new Dictionary<int, List<VertexBoneAssignment>>();
        /// <summary>Flag indicating that bone assignments need to be recompiled.</summary>
        protected internal bool boneAssignmentsOutOfDate;

        /// <summary>Mode used for rendering this submesh.</summary>
        protected internal Axiom.Graphics.OperationType operationType;
        public VertexData vertexData;
        public IndexData indexData = new IndexData();
        /// <summary>Indicates if this submesh shares vertex data with other meshes or whether it has it's own vertices.</summary>
        public bool useSharedVertices;
        
		/// <summary>
		///		Local bounding box of this submesh.
		/// </summary>
        protected AxisAlignedBox boundingBox = AxisAlignedBox.Null;

		/// <summary>
		///		Radius of this submesh's bounding sphere.
		/// </summary>
        protected float boundingSphereRadius;

        /// <summary>
        ///     Dedicated index map for translate blend index to bone index (only valid if useSharedVertices = false).
        /// </summary>
        /// <remarks>
        ///     This data is completely owned by this submesh.
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
        protected internal List<ushort> blendIndexToBoneIndexMap = new List<ushort>();
        protected internal List<IndexData> lodFaceList = new List<IndexData>();

        /// <summary>Type of vertex animation for dedicated vertex data (populated by Mesh)</summary>
		protected VertexAnimationType vertexAnimationType = VertexAnimationType.None;

		#endregion
		
        #region Constructor

        /// <summary>
        ///		Basic contructor.
        /// </summary>
        /// <param name="name"></param>
        public SubMesh(string name) {
            this.name = name;

            useSharedVertices = true;

            operationType = OperationType.TriangleList;
        }

        /// <summary>The texture aliases for this submesh.</summary>
        protected Dictionary<string, string> textureAliases = new Dictionary<string, string>();
        
        #endregion

        #region Methods

        /// <summary>
        ///    Assigns a vertex to a bone with a given weight, for skeletal animation. 
        /// </summary>
        /// <remarks>
        ///    This method is only valid after setting the SkeletonName property.
        ///    You should not need to modify bone assignments during rendering (only the positions of bones) 
        ///    and the engine reserves the right to do some internal data reformatting of this information, 
        ///    depending on render system requirements.
        /// </remarks>
        /// <param name="boneAssignment"></param>
        public void AddBoneAssignment(VertexBoneAssignment boneAssignment) {
            if (!boneAssignmentList.ContainsKey(boneAssignment.vertexIndex))
                boneAssignmentList[boneAssignment.vertexIndex] = new List<VertexBoneAssignment>();
            boneAssignmentList[boneAssignment.vertexIndex].Add(boneAssignment);
            boneAssignmentsOutOfDate = true;
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

        /// <summary>
        ///    Must be called once to compile bone assignments into geometry buffer.
        /// </summary>
        protected internal void CompileBoneAssignments() {
            int maxBones = parent.RationalizeBoneAssignments(vertexData.vertexCount, boneAssignmentList);

            // return if no bone assigments
            if(maxBones != 0) {
				// FIXME: For now, to support hardware skinning with a single shader,
				// we always want to have 4 bones. (robin@multiverse.net)
				maxBones = 4;
				parent.CompileBoneAssignments(boneAssignmentList, maxBones, blendIndexToBoneIndexMap, vertexData);
			}
            boneAssignmentsOutOfDate = false;
        }

        public void RemoveLodLevels() {
            lodFaceList.Clear();
        }

        /// <summary>
        ///    Adds the alias or replaces an existing one and associates the texture name to it.
        /// </summary>
        /// <remarks>
        ///    The submesh uses the texture alias to replace textures used in the material applied
        ///    to the submesh.
        /// </remarks>
        /// <param name="aliasName">The name of the alias.</param>
        /// <param name="textureName">The name of the texture to be associated with the alias.</param>
        public void AddTextureAlias(string aliasName, string textureName) {
            textureAliases[aliasName] = textureName;
        }
        
        /// <summary>
        ///    Remove a specific texture alias name from the sub mesh
        /// </summary>
        /// <param name="aliasName">The name of the alias.  If it is not found
        ///    then it is ignored.
        /// </param>
        public void RemoveTextureAlias(string aliasName) {
            textureAliases.Remove(aliasName);
        }
        
        /// <summary>
        ///    Removes all texture aliases from the sub mesh
        /// </summary>
        public void RemoveAllTextureAliases() {
            textureAliases.Clear();
        }
        
        /// <summary>
        ///    The current material used by the submesh is copied into a new material
        ///    and the submesh's texture aliases are applied if the current texture alias
        ///    names match those found in the original material.
        /// </summary>
        /// <remarks>
        ///    The submesh's texture aliases must be setup prior to calling this method.
        ///    If a new material has to be created, the subMesh autogenerates the new name.
        ///    The new name is the old name + "_" + number.
        /// </remarks>
        /// <returns>True if texture aliases were applied and a new material was created.</returns>
        public bool UpdateMaterialUsingTextureAliases() {
            bool newMaterialCreated = false;
            // if submesh has texture aliases
            // ask the material manager if the current summesh material exists
            if (HasTextureAliases && MaterialManager.Instance.HasResource(materialName)) {
                // get the current submesh material
                Material material = MaterialManager.Instance.GetByName( materialName );
                // get test result for if change will occur when the texture aliases are applied
                if (material.ApplyTextureAliases(textureAliases, false)) {
                    // material textures will be changed so copy material,
                    // new material name is old material name + index
                    // check with material manager and find a unique name
                    int index = 0;
                    string newMaterialName = materialName + "_" + index;
                    while (MaterialManager.Instance.HasResource(newMaterialName))
                        // increment index for next name
                        newMaterialName = materialName + "_" + ++index;
                    Material newMaterial = (Material)MaterialManager.Instance.Create(newMaterialName, false);
                    // copy parent material details to new material
                    material.CopyTo(newMaterial);
                    // apply texture aliases to new material
                    newMaterial.ApplyTextureAliases(textureAliases);
                    // place new material name in submesh
                    materialName = newMaterialName;
                    newMaterialCreated = true;
                }
            }
            return newMaterialCreated;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        ///		Gets/Sets the name of this SubMesh.
        /// </summary>
        public string Name {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        ///		Gets/Sets the name of the material this SubMesh will be using.
        /// </summary>
        public string MaterialName {
            get { return materialName; }
            set { materialName = value; isMaterialInitialized = true; }
        }

        /// <summary>
        ///		Gets/Sets the parent mode of this SubMesh.
        /// </summary>
        public Mesh Parent {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        ///		Overloaded method.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public void GetRenderOperation(RenderOperation op) {
            // call overloaded method with lod index of 0 by default
            GetRenderOperation(op, 0);
        }

        /// <summary>
        ///    Fills a RenderOperation structure required to render this mesh.
        /// </summary>
        /// <param name="op">Reference to a RenderOperation structure to populate.</param>
        /// <param name="lodIndex">The index of the LOD to use.</param>
        public void GetRenderOperation(RenderOperation op, int lodIndex) {
            // SubMeshes always use indices
            op.useIndices = true;

            // use lod face list if requested, else pass the normal face list
            if(lodIndex > 0 && (lodIndex - 1) < lodFaceList.Count) {
                // Use the set of indices defined for this LOD level
                op.indexData = lodFaceList[lodIndex - 1];
            }
            else
                op.indexData = indexData;
			
            // set the operation type
            op.operationType = operationType;

            // set the vertex data correctly
            op.vertexData = useSharedVertices ? parent.SharedVertexData : vertexData;
        }

        /// <summary>
        ///		Gets whether or not a material has been set for this subMesh.
        /// </summary>
        public bool IsMaterialInitialized {
            get { return isMaterialInitialized; }
        }

        /// <summary>
        ///		Gets bone assigment list
        /// </summary>
        public Dictionary<int, List<VertexBoneAssignment>> BoneAssignmentList {
            get { return boneAssignmentList; }
        }

        public List<ushort> BlendIndexToBoneIndexMap {
            get {
                return blendIndexToBoneIndexMap;
            }
        }

        public int NumFaces {
            get {
                int numFaces = 0;
                if (indexData == null)
                    return 0;
                if (operationType == OperationType.TriangleList)
                    numFaces = indexData.indexCount / 3;
                else
                    numFaces = indexData.indexCount - 2;
                return numFaces;
            }
        }

        public OperationType OperationType {
            get {
                return operationType;
            }
            set {
                operationType = value;
            }
        }

        public VertexAnimationType VertexAnimationType {
			get { 
				if (parent.AnimationTypesDirty) 
					parent.DetermineAnimationTypes();
				return vertexAnimationType;
			}
			set { vertexAnimationType = value; }
		}
		
        public VertexAnimationType CurrentVertexAnimationType {
			get { 
				return vertexAnimationType;
			}
		}
		
		public List<IndexData> LodFaceList {
			get { 
                return lodFaceList;
            }
        }
        
		public VertexData VertexData {
			get { 
                return vertexData;
            }
        }
        
		public IndexData IndexData {
			get { 
                return indexData;
            }
        }
        
        /// <summary>
        ///    Returns true if the sub mesh has texture aliases
        /// </summary>
        public bool HasTextureAliases {
            get { return textureAliases.Count != 0; }
        }

        /// <summary>
        ///    Gets the texture aliases assigned to the sub mesh.
        /// </summary>
        public Dictionary<string, string> TextureAliases 
        {
            get { return textureAliases; }
        }

        /// <summary>
        ///    Gets the number of texture aliases assigned to the sub mesh.
        /// </summary>
        public int TextureAliasCount 
        {
            get { return textureAliases.Count; }
        }

		/// <summary>
		///		Gets/Sets the bounding box for this submesh.
		/// </summary>
		/// <remarks>
		///		Setting this property is required when building manual
		///		submeshes now, because Axiom can no longer update the
		///		bounds for you, because it cannot necessarily read
		///		vertex data back from the vertex buffers which this
		///		mesh uses (they very well might be write-only, and
		///		even if they are not, reading data from a hardware
		///		buffer is a bottleneck).
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
        ///    Bounding spehere radius from this submesh in local coordinates.
        /// </summary>
        public float BoundingSphereRadius {
            get { 
				return boundingSphereRadius; 
			}
            set { 
				boundingSphereRadius = value; 
			}
        }

		#endregion
    }
}
