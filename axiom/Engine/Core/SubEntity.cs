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
using Axiom.Collections;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Animating;

namespace Axiom.Core {
    /// <summary>
    ///		Utility class which defines the sub-parts of an Entity.
    /// </summary>
    /// <remarks>
    ///		<para>
    ///		Just as models are split into meshes, an Entity is made up of
    ///		potentially multiple SubEntities. These are mainly here to provide the
    ///		link between the Material which the SubEntity uses (which may be the
    ///		default Material for the SubMesh or may have been changed for this
    ///		object) and the SubMesh data.
    ///		</para>
    ///		<para>
    ///		SubEntity instances are never created manually. They are created at
    ///		the same time as their parent Entity by the SceneManager method
    ///		CreateEntity.
    ///		</para>
    /// </remarks>
    public class SubEntity : IRenderable {
        #region Fields

        /// <summary>
        ///    Reference to the parent Entity.
        /// </summary>
        protected Entity parent;
        /// <summary>
        ///    Name of the material being used.
        /// </summary>
        protected string materialName;
        /// <summary>
        ///    Reference to the material being used by this SubEntity.
        /// </summary>
        protected Material material;
        /// <summary>
        ///    Reference to the subMesh that represents the geometry for this SubEntity.
        /// </summary>
        protected SubMesh subMesh;
        /// <summary>
        ///    The world AABB computed from the subMesh bounding box.
        /// </summary>
        protected AxisAlignedBox worldAABB = new AxisAlignedBox();
        /// <summary>
        ///    Detail to be used for rendering this sub entity.
        /// </summary>
        protected SceneDetailLevel renderDetail;
		/// <summary>
		///		Current LOD index to use.
		/// </summary>
		internal int materialLodIndex;
		/// <summary>
		///		Flag indicating whether this sub entity should be rendered or not.
		/// </summary>
		protected bool isVisible;
		/// <summary>
		///		Blend buffer details for dedicated geometry.
		/// </summary>
		protected internal VertexData skelAnimVertexData;
		/// <summary>
		///		Temp buffer details for software skeletal anim geometry
		/// </summary>
		protected internal TempBlendedBufferInfo tempSkelAnimInfo = new TempBlendedBufferInfo();
		/// <summary>
		///		Temp buffer details for software Vertex anim geometry
		/// </summary>
		protected TempBlendedBufferInfo tempVertexAnimInfo = new TempBlendedBufferInfo();
        /// <summary>
        ///		Vertex data details for software Vertex anim of shared geometry
        /// </summary>
				/// Temp buffer details for software Vertex anim geometry
		protected VertexData softwareVertexAnimVertexData;
        /// <summary>
		///     Vertex data details for hardware Vertex anim of shared geometry
		///     - separate since we need to s/w anim for shadows whilst still altering
		///       the vertex data for hardware morphing (pos2 binding)
        /// </summary>
		protected VertexData hardwareVertexAnimVertexData;
        /// <summary>
        ///		Have we applied any vertex animation to geometry?
        /// </summary>
		protected bool vertexAnimationAppliedThisFrame;
        /// <summary>
        ///		Number of hardware blended poses supported by material
        /// </summary>
		protected ushort hardwarePoseCount;
        /// <summary>
        ///		Flag indicating whether hardware skinning is supported by this subentity's materials.
        /// </summary>
        protected bool hardwareSkinningEnabled;
        /// <summary>
        ///		Flag indicating whether vertex programs are used by this subentity's materials.
        /// </summary>
        protected bool useVertexProgram;

		protected Hashtable customParams = new Hashtable();

        #endregion Fields

        #region Constructor

        /// <summary>
        ///		Internal constructor, only allows creation of SubEntities within the engine core.
        /// </summary>
        internal SubEntity() {
            material = MaterialManager.Instance.GetByName("BaseWhite");
            renderDetail = SceneDetailLevel.Solid;

			isVisible = true;
        }

        #endregion

        #region Properties

		/// <summary>
		///		Gets a flag indicating whether or not this sub entity should be rendered or not.
		/// </summary>
		public bool IsVisible {
			get {
				return isVisible;
			}
			set {
				isVisible = value;
			}
		}

        /// <summary>
        ///		Gets/Sets the name of the material used for this SubEntity.
        /// </summary>
        public string MaterialName {
            get { 
                return materialName; 
            }
            set {
                if (value == null)
                    throw new AxiomException("Cannot set the subentity material to be null");
                materialName = value; 

                // load the material from the material manager (it should already exist
                material = MaterialManager.Instance.GetByName(materialName);

                if(material == null) {
                    LogManager.Instance.Write(
                        "Cannot assign material '{0}' to SubEntity '{1}' because the material doesn't exist.", materialName, parent.Name);

                    // give it base white so we can continue
                    material = MaterialManager.Instance.GetByName("BaseWhite");
                }

                // ensure the material is loaded.  It will skip it if it already is
                material.Load();

				// since the material has changed, re-evaulate its support of skeletal animation
                    parent.ReevaluateVertexProcessing();
                }
            }

        /// <summary>
        ///		Gets/Sets the subMesh to be used for rendering this SubEntity.
        /// </summary>
        public SubMesh SubMesh {
            get { 
                return subMesh; 
            }
            set { 
                subMesh = value; 
            }
        }

        /// <summary>
        ///    The world AABB computed from the subMesh bounding box.
        /// </summary>
        public AxisAlignedBox WorldAABB {
            get {
                return worldAABB;
            }
            set {
                worldAABB = value;
            }
        }

        /// <summary>
        ///		Gets/Sets the parent entity of this SubEntity.
        /// </summary>
        public Entity Parent {
            get { 
                return parent; 
            }
            set { 
                parent = value; 
            }
        }

		public VertexData SkelAnimVertexData {
			get {
				return skelAnimVertexData;
			}
		}

		public TempBlendedBufferInfo TempSkelAnimInfo {
			get {
				return tempSkelAnimInfo;
			}
		}
	
		public TempBlendedBufferInfo TempVertexAnimInfo {
			get {
				return tempVertexAnimInfo;
			}
		}
	
		public VertexData SoftwareVertexAnimVertexData {
			get {
				return softwareVertexAnimVertexData;
			}
		}
	
		public VertexData HardwareVertexAnimVertexData {
			get {
				return hardwareVertexAnimVertexData;
			}
		}

		public ushort HardwarePoseCount {
			get {
				return hardwarePoseCount;
			}
			set {
				hardwarePoseCount = value;
			}
		}

		/// <summary>
		///		Are buffers already marked as vertex animated?
		/// </summary>
		public bool BuffersMarkedForAnimation {
			get {
				return vertexAnimationAppliedThisFrame;
			}
		}

        #endregion

		#region Methods

		/// <summary>
		///		Internal method for preparing this sub entity for use in animation.
		/// </summary>
		protected internal void PrepareTempBlendBuffers() {
			// Handle the case where we have no submesh vertex data (probably shared)
			if (subMesh.useSharedVertices)
				return;
			if (skelAnimVertexData != null) 
				skelAnimVertexData = null;
			if (softwareVertexAnimVertexData != null) 
				softwareVertexAnimVertexData = null;
			if (hardwareVertexAnimVertexData != null) 
				hardwareVertexAnimVertexData = null;

			if (!subMesh.useSharedVertices) {
				if (subMesh.VertexAnimationType != VertexAnimationType.None) {
					// Create temporary vertex blend info
					// Prepare temp vertex data if needed
					// Clone without copying data, don't remove any blending info
					// (since if we skeletally animate too, we need it)
					softwareVertexAnimVertexData = subMesh.vertexData.Clone(false);
					parent.ExtractTempBufferInfo(softwareVertexAnimVertexData, tempVertexAnimInfo);

					// Also clone for hardware usage, don't remove blend info since we'll
					// need it if we also hardware skeletally animate
					hardwareVertexAnimVertexData = subMesh.vertexData.Clone(false);
				}
			
				if (parent.HasSkeleton) {
					// Create temporary vertex blend info
					// Prepare temp vertex data if needed
					// Clone without copying data, remove blending info
					// (since blend is performed in software)
					skelAnimVertexData = parent.CloneVertexDataRemoveBlendInfo(subMesh.vertexData);
					parent.ExtractTempBufferInfo(skelAnimVertexData, tempSkelAnimInfo);
				}
			}
		}

		#endregion Methods

        #region IRenderable Members

		public bool CastsShadows {
			get {
				return parent.CastShadows;
			}
		}

        /// <summary>
        ///		Gets/Sets a reference to the material being used by this SubEntity.
        /// </summary>
        /// <remarks>
        ///		By default, the SubEntity will use the material defined by the SubMesh.  However,
        ///		this can be overridden by the SubEntity in the case where several entities use the
        ///		same SubMesh instance, but want to shade it different.
        ///     This should probably call parent.ReevaluateVertexProcessing.
        /// </remarks>
        public Material Material {
            get { 
                return material;
            }
            set { 
                material = value;
                // We may have switched to a material with a vertex shader 
                // or something similar.
                parent.ReevaluateVertexProcessing();
            }
        }

        // ??? In the ogre version, it get the value of these from the parent Entity.
		public bool NormalizeNormals {
            get {
                return false;
            }
        }

        public Technique Technique {
            get {
                return material.GetBestTechnique(materialLodIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        public void GetRenderOperation(RenderOperation op) {
			// use LOD
            subMesh.GetRenderOperation(op, parent.MeshLodIndex);
			// Deal with any vertex data overrides
// 			if (!hardwareSkinningEnabled)
				op.vertexData = GetVertexDataForBinding();
        }

		public VertexData GetVertexDataForBinding()
		{
			if (subMesh.useSharedVertices)
				return parent.GetVertexDataForBinding();
			else {
				VertexDataBindChoice c = 
					parent.ChooseVertexDataForBinding(
						subMesh.VertexAnimationType != VertexAnimationType.None);
				switch(c) {
				case VertexDataBindChoice.Original:
					return subMesh.vertexData;
				case VertexDataBindChoice.HardwareMorph:
					return hardwareVertexAnimVertexData;
				case VertexDataBindChoice.SoftwareMorph:
					return softwareVertexAnimVertexData;
				case VertexDataBindChoice.SoftwareSkeletal:
					return skelAnimVertexData;
				};
				// keep compiler happy
				return subMesh.vertexData;
			}
		}

        Material IRenderable.Material {
            get { 
                return material; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrices"></param>
        public void GetWorldTransforms(Matrix4[] matrices) {
            if(parent.numBoneMatrices == 0 || !parent.IsHardwareAnimationEnabled) {
                matrices[0] = parent.ParentFullTransform;
            }
            else {
                // Hardware skinning, pass all actually used matrices
                List<ushort> indexMap = subMesh.useSharedVertices ? 
                    subMesh.Parent.SharedBlendIndexToBoneIndexMap : subMesh.BlendIndexToBoneIndexMap;
                Debug.Assert(indexMap.Count <= this.Parent.numBoneMatrices);

                if (parent.IsSkeletonAnimated)
                {
                    // Bones, use cached matrices built when Entity::UpdateRenderQueue was called
                    Debug.Assert(parent.boneMatrices != null);

                    for (int i = 0; i < indexMap.Count; i++) {
                        matrices[i] = parent.boneMatrices[indexMap[i]];
                    }
                }
                else
                {
                    // All animations disabled, use parent entity world transform only
                    for (int i = 0; i < indexMap.Count; i++) {
                        matrices[i] = parent.ParentFullTransform;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ushort NumWorldTransforms {
            get {
                if (parent.numBoneMatrices == 0 ||
                    !parent.IsHardwareAnimationEnabled) {
                    // No skeletal animation, or software skinning
                    return 1;
                } else {
                    // Hardware skinning, pass all actually used matrices
                    List<ushort> indexMap = subMesh.useSharedVertices ?
                        subMesh.Parent.SharedBlendIndexToBoneIndexMap : subMesh.BlendIndexToBoneIndexMap;
                    Debug.Assert(indexMap.Count <= this.Parent.numBoneMatrices);

                    return (ushort)indexMap.Count;
                }
            }
        }

        /// <summary>
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
                return renderDetail;	
            }
            set {
                renderDetail = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public float GetSquaredViewDepth(Camera camera) {
            // get the parent entitie's parent node
            Node node = parent.ParentNode;

            Debug.Assert(node != null);

            return node.GetSquaredViewDepth(camera);
        }

        /// <summary>
        /// 
        /// </summary>
        public Quaternion WorldOrientation {
            get {
                // get the parent entitie's parent node
                Node node = parent.ParentNode;

                Debug.Assert(node != null);

                return parent.ParentNode.DerivedOrientation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 WorldPosition {
            get {
                // get the parent entitie's parent node
                Node node = parent.ParentNode;

                Debug.Assert(node != null);

                return parent.ParentNode.DerivedPosition;
            }
        }

        /// <summary>
        ///    
        /// </summary>
        public List<Light> Lights {
            get {
                // get the parent entitie's parent node
                Node node = parent.ParentNode;

                Debug.Assert(node != null);

                return parent.ParentNode.Lights;
            }
        }

        /// <summary>
        ///		Returns whether or not hardware skinning is enabled.
        /// </summary>
        /// <remarks>
        ///		Because fixed-function indexed vertex blending is rarely supported
        ///		by existing graphics cards, hardware skinning can only be done if
        ///		the vertex programs in the materials used to render an entity support
        ///		it. Therefore, this method will only return true if all the materials
        ///		assigned to this entity have vertex programs assigned, and all those
        ///		vertex programs must support 'include_skeletal_animation true'.
        /// </remarks>
        public bool HardwareSkinningEnabled {
            get {
                 return useVertexProgram && hardwareSkinningEnabled;
            }
            set {
                hardwareSkinningEnabled = value;
            }
        }

        public bool VertexProgramInUse {
            get {
                return useVertexProgram;
            }
            set {
                useVertexProgram = value;
            }
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
			if (entry.type == AutoConstants.AnimationParametric) {
				// Set up to 4 values, or up to limit of hardware animation entries
				// Pack into 4-element constants offset based on constant data index
				// If there are more than 4 entries, this will be called more than once
				Vector4 val = Vector4.Zero;

				int animIndex = entry.data * 4;
				for (int i = 0; i < 4 && 
					animIndex < hardwareVertexAnimVertexData.HWAnimationDataList.Count;
					++i, ++animIndex) {
					val[i] = hardwareVertexAnimVertexData.HWAnimationDataList[animIndex].Parametric;
				}
				// set the parametric morph value
				gpuParams.SetConstant(entry.index, val);
			}
			else if (customParams[entry.data] != null) {
				gpuParams.SetConstant(entry.index, (Vector4)customParams[entry.data]);
			}
		}

		public void MarkBuffersUnusedForAnimation() {
			vertexAnimationAppliedThisFrame = false;
		}

		public void MarkBuffersUsedForAnimation() {
			vertexAnimationAppliedThisFrame = true;
		}

		public void RestoreBuffersForUnusedAnimation(bool hardwareAnimation) {
			// Rebind original positions if:
			//  We didn't apply any animation and 
			//    We're morph animated (hardware binds keyframe, software is missing)
			//    or we're pose animated and software (hardware is fine, still bound)
			if (subMesh.VertexAnimationType != VertexAnimationType.None && 
				!subMesh.useSharedVertices && 
				!vertexAnimationAppliedThisFrame &&
				(!hardwareAnimation || subMesh.VertexAnimationType == VertexAnimationType.Morph)) {
				VertexElement srcPosElem = 
					subMesh.vertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
				HardwareVertexBuffer srcBuf = 
					subMesh.vertexData.vertexBufferBinding.GetBuffer(srcPosElem.Source);

				// Bind to software
				VertexElement destPosElem = 
					softwareVertexAnimVertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
				softwareVertexAnimVertexData.vertexBufferBinding.SetBinding(destPosElem.Source, srcBuf);
			}
		}


        #endregion
    }
}
