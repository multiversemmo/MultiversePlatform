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
using Axiom.Core;
using Axiom.Configuration;
using Axiom.MathLib;
using Axiom.Controllers;

namespace Axiom.Graphics {
    /// <summary>
    /// 	This utility class is used to hold the information used to generate the matrices
    /// 	and other information required to automatically populate GpuProgramParameters.
	/// </summary>
	/// <remarks>
	///    This class exercises a lazy-update scheme in order to avoid having to update all
    /// 	the information a GpuProgramParameters class could possibly want all the time. 
    /// 	It relies on the SceneManager to update it when the base data has changed, and
    /// 	will calculate concatenated matrices etc only when required, passing back precalculated
    /// 	matrices when they are requested more than once when the underlying information has
    /// 	not altered.
	/// </remarks>
	public class AutoParamDataSource {
		#region Fields

        /// <summary>
        ///    Current target renderable.
        /// </summary>
        protected IRenderable renderable;
        /// <summary>
        ///    Current camera being used for rendering.
        /// </summary>
        protected Camera camera;
		/// <summary>
		///		Current frustum used for texture projection for each
		///     simultaneous light
		/// </summary>
		protected Frustum [] currentTextureProjector;
		/// <summary>
		///		Current texture view projection matrix for each
		///     simultaneous light
		/// </summary>
		protected Matrix4 [] textureViewProjMatrix;
		protected bool [] textureViewProjMatrixDirty;
        /// <summary>
		///		Current active render target.
		/// </summary>
		protected RenderTarget currentRenderTarget;
        /// <summary>
        ///     The current viewport.  We don't really do anything with this,
        ///     but Ogre uses it to determine the width and height.
        /// </summary>
        protected Viewport currentViewport;
        /// <summary>
        ///    Current view matrix;
        /// </summary>
        protected Matrix4 viewMatrix;
        protected bool viewMatrixDirty;
        /// <summary>
        ///    Current projection matrix.
        /// </summary>
        protected Matrix4 projectionMatrix;
        protected bool projMatrixDirty;
        /// <summary>
        ///    Inverse of current projection matrix.
        /// </summary>
        protected Matrix4 inverseProjectionMatrix;
        protected bool inverseProjectionMatrixDirty;
		/// <summary>
		///    Current view and projection matrices concatenated.
		/// </summary>
		protected Matrix4 viewProjMatrix;
		protected bool viewProjMatrixDirty;
		/// <summary>
		///    Inverse of current view and projection matrices concatenated.
		/// </summary>
		protected Matrix4 inverseViewProjMatrix;
		protected bool inverseViewProjMatrixDirty;
		/// <summary>
		///    Array of world matrices for the current renderable.
		/// </summary>
        protected Matrix4[] worldMatrix = new Matrix4[256];
		/// <summary>
		///    A reference to an array of world matrices
        protected Matrix4[] worldMatrixArray;
		/// </summary>
        protected bool worldMatrixDirty;
		/// <summary>
		///		Current count of matrices in the world matrix array.
		/// </summary>
		protected int worldMatrixCount;
        /// <summary>
        ///    Current concatenated world and view matrices.
        /// </summary>
        protected Matrix4 worldViewMatrix;
        protected bool worldViewMatrixDirty;
        /// <summary>
        ///    Current concatenated world, view, and projection matrices.
        /// </summary>
        protected Matrix4 worldViewProjMatrix;
        protected bool worldViewProjMatrixDirty;
        /// <summary>
        ///    Inverse of current worldViewProj matrix.
        /// </summary>
        protected Matrix4 inverseWorldViewProjMatrix;
        protected bool inverseWorldViewProjMatrixDirty;
        /// <summary>
        ///    Inverse of current world matrix.
        /// </summary>
        protected Matrix4 inverseWorldMatrix;
        protected bool inverseWorldMatrixDirty;
        /// <summary>
        ///    Inverse of current concatenated world and view matrices.
        /// </summary>
        protected Matrix4 inverseWorldViewMatrix;
        protected bool inverseWorldViewMatrixDirty;
        /// <summary>
        ///    Inverse of the current view matrix.
        /// </summary>
        protected Matrix4 inverseViewMatrix;
        protected bool inverseViewMatrixDirty;
		/// <summary>
		///		Scene depth range
		/// </summary>
		protected Vector4 sceneDepthRange;
		protected bool sceneDepthRangeDirty;
		/// <summary>
		///		Shadow cam depth ranges
		/// </summary>
		protected List<Vector4> shadowCamDepthRanges;
		protected bool shadowCamDepthRangesDirty;
		/// <summary>
		///		Distance to extrude shadow volume vertices.
		/// </summary>
		protected float dirLightExtrusionDistance;
        /// <summary>
        ///    Position of the current camera in object space relative to the current renderable.
        /// </summary>
        protected Vector4 cameraPositionObjectSpace;
        /// <summary>
        ///    Current global ambient light color.
        /// </summary>
        protected ColorEx ambientLight;
        /// <summary>
        ///    Parameters for GPU fog.  fogStart, fogEnd, and fogScale
        /// </summary>
        protected Vector4 fogParams;
        /// <summary>
        ///    Color of fog
        /// </summary>
        protected ColorEx fogColor;
        /// <summary>
        ///   current time
        /// </summary>
        protected float time;
        /// <summary>
        ///    List of lights that are in the scene and near the current renderable.
        /// </summary>
        protected List<Light> currentLightList = new List<Light>();
        /// <summary>
        ///    Blank light to use when a higher index light is requested than is available.
        /// </summary>
        protected Light blankLight = new Light();
        /// <summary>
        ///    
        /// </summary>
        protected VisibleObjectsBoundsInfo mainCamBoundsInfo;

        //protected bool cameraPositionDirty;
        protected bool cameraPositionObjectSpaceDirty;

		protected Matrix4 ProjectionClipSpace2DToImageSpacePerspective = new Matrix4(
            //from ogre
            //0.5f,    0,  0, 0.5f, 
            //0, -0.5f,  0, 0.5f, 
            //0,    0,  0.5f,   0.5f,
            //0,    0,  0,   1);
            //original from axiom
            //0.5f, 0, 0, -0.5f,
            //0, -0.5f,  0, -0.5f, 
            //0,    0,  0,   1,
			//0,    0,  0,   1);
            0.5f, 0, 0, 0.5f,
            0, -0.5f, 0, 0.5f,
            0, 0, 1, 0,
            0, 0, 0, 1);

		protected int passNumber;

        protected int passIterationNumber;
        
        protected Pass currentPass;
        
        protected Vector4 mvShadowTechnique;
		
		protected SceneManager currentSceneManager;

        /// <summary>
        /// X element is shadow fade near distance
        /// Y element is shadow fade far distance
        /// </summary>
        protected Vector4 shadowFadeParams;
        
        #endregion Fields
		
		#region Constructors
		
        /// <summary>
        ///    Default constructor.
        /// </summary>
		public AutoParamDataSource() {
            //cameraPositionDirty = true;
            cameraPositionObjectSpaceDirty = true;
            inverseProjectionMatrixDirty = true;
            inverseViewMatrixDirty = true;
            inverseViewProjMatrixDirty = true;
            inverseWorldMatrixDirty = true;
            inverseWorldViewMatrixDirty = true;
            inverseWorldViewProjMatrixDirty = true;
            projMatrixDirty = true;
            sceneDepthRangeDirty = true;
            shadowCamDepthRangesDirty = true;
            viewMatrixDirty = true;
            viewProjMatrixDirty = true;
            worldMatrixDirty = true;
            worldViewMatrixDirty = true;
            worldViewProjMatrixDirty = true;

            // inverseTransposeWorldMatrixDirty = true;
            // inverseTransposeWorldViewMatrixDirty = true;
            currentTextureProjector = new Frustum[Config.MaxSimultaneousLights];
            textureViewProjMatrix = new Matrix4[Config.MaxSimultaneousLights];
			textureViewProjMatrixDirty = new bool[Config.MaxSimultaneousLights];
            for(int i=0; i<Config.MaxSimultaneousLights; ++i) {
                textureViewProjMatrixDirty[i] = true;
                currentTextureProjector[i] = null;
            }

            // defaults for the blank light
            blankLight.Diffuse = ColorEx.Black;
            blankLight.Specular = ColorEx.Black;
            blankLight.SetAttenuation(0, 1, 0, 0);
		}
		
		#endregion
		
		#region Methods
		
        /// <summary>
        ///    Get the light which is 'index'th closest to the current object 
        /// </summary>
        /// <param name="index">Ordinal value signifying the light to retreive, with 0 being closest, 1 being next closest, etc.</param>
        /// <returns>A light located near the current renderable.</returns>
        public Light GetLight(int index) {
            if(currentLightList.Count <= index) {
                return blankLight;
            }
            else {
                return currentLightList[index];
            }
        }

        /// <summary>
        ///    
        /// </summary>
        public void SetCurrentLightList(List<Light> lightList) {
            currentLightList = lightList;
            shadowCamDepthRangesDirty = true;
        }

		/// <summary>
		///		Sets the constant extrusion distance for directional lights.
		/// </summary>
		/// <param name="distance"></param>
		public void SetShadowDirLightExtrusionDistance(float distance) {
			dirLightExtrusionDistance = distance;
		}

		/// <summary>
		///		Gets the current texture * view * projection matrix.
		/// </summary>
		public Matrix4 GetTextureViewProjectionMatrix(int index) {
            if(textureViewProjMatrixDirty[index] && currentTextureProjector[index] != null) {
                textureViewProjMatrix[index] =
                        ProjectionClipSpace2DToImageSpacePerspective *
                        currentTextureProjector[index].ProjectionMatrixWithRSDepth *
                        currentTextureProjector[index].ViewMatrix;

                textureViewProjMatrixDirty[index] = false;
			}
            return textureViewProjMatrix[index];
		}

		/// <summary>
		///		Sets the current texture projector for a index
		/// </summary>
        public void SetTextureProjector(Frustum frust, int index) {
            currentTextureProjector[index] = frust;
            textureViewProjMatrixDirty[index] = true;
        }

        public void SetFog(FogMode fogMode, ColorEx fogColor, float fogStart, float fogEnd, float fogScale) {
            this.fogColor = fogColor;
            fogParams = new Vector4(fogStart, fogEnd, fogScale, 0);
        }
            
        public float GetTime_0_X(float data) {
            return time % data;
        }
        
        public float GetTime_0_1(float data) {
            return (time % data) / data;
        }
        
        public float GetTime_0_2PI(float data) {
            return (float)((time % data) / (data * data * 2 * Math.PI));
        }

        public Vector4 GetTextureSize(int index) {
            Vector4 size = new Vector4(1f, 1f, 1f, 1f);
            if (index < currentPass.NumTextureUnitStages) {
                Texture tex = currentPass.GetTextureUnitState(index).GetTexturePtr(0);
                if (tex != null) {
                    size.x = tex.Width;
                    size.y = tex.Height;
                    size.z = tex.Depth;
                }
            }
            return size;
        }

        protected static Vector4 dummyDepthRange = new Vector4(0f, 100000f, 100000f, 1f/100000f);
        
        public Vector4 GetShadowSceneDepthRange(int lightIndex) {

            if (currentSceneManager.IsShadowTechniqueTextureBased)
                return dummyDepthRange;

            if (shadowCamDepthRangesDirty)
            {
                shadowCamDepthRanges.Clear();
                foreach (Light light in currentLightList) {
                    // stop as soon as we run out of shadow casting lights, they are
                    // all grouped at the beginning
                    if (light.CastShadows)
                        break;

                    VisibleObjectsBoundsInfo info = currentSceneManager.GetShadowCasterBoundsInfo(light);

                    sceneDepthRange.x = mainCamBoundsInfo.minDistance;
                    sceneDepthRange.y = mainCamBoundsInfo.maxDistance;
                    sceneDepthRange.z = mainCamBoundsInfo.maxDistance - mainCamBoundsInfo.minDistance;
                    sceneDepthRange.w = 1.0f / sceneDepthRange.z;

                    shadowCamDepthRanges.Add(new Vector4(info.minDistance, 
                                                         info.maxDistance, 
                                                         info.maxDistance - info.minDistance,
                                                         1.0f / (info.maxDistance - info.minDistance)));
                }
                shadowCamDepthRangesDirty = false;
            }

            if (lightIndex >= shadowCamDepthRanges.Count)
                return dummyDepthRange;
            else
                return shadowCamDepthRanges[lightIndex];

        }
        // TODO: Ask Jeff about this, because the setting in ogre
        // is below, which is different    
//     void AutoParamDataSource::setFog(FogMode mode, const ColourValue& colour,
//         Real expDensity, Real linearStart, Real linearEnd)
//     {
//         (void)mode; // ignored
//         mFogColour = colour;
//         mFogParams.x = expDensity;
//         mFogParams.y = linearStart;
//         mFogParams.z = linearEnd;
//         mFogParams.w = linearEnd != linearStart ? 1 / (linearEnd - linearStart) : 0;
//     }


        
        #endregion
		
		#region Properties

        /// <summary>
        ///    Gets/Sets the current renderable object.
        /// </summary>
		public IRenderable Renderable {
		    get {
		        return renderable;
		    }
		    set {
                renderable = value;

                // set the dirty flags to force updates
                cameraPositionObjectSpaceDirty = true;
                inverseProjectionMatrixDirty = true;
                inverseViewMatrixDirty = true;
                inverseViewProjMatrixDirty = true;
                inverseWorldMatrixDirty = true;
                inverseWorldViewMatrixDirty = true;
                inverseWorldViewProjMatrixDirty = true;
                projMatrixDirty = true;
                viewMatrixDirty = true;
                viewProjMatrixDirty = true;
                worldMatrixDirty = true;
                worldViewMatrixDirty = true;
                worldViewProjMatrixDirty = true;
                // inverseTransposeWorldMatrixDirty = true;
                // inverseTransposeWorldViewMatrixDirty = true;
            }
		}

        /// <summary>
        ///    Gets/Sets the current camera being used for rendering.
        /// </summary>
		public Camera Camera {
		    get {
		        return camera;
		    }
		    set {
                camera = value;
            
                // set the dirty flags to force updates
                //cameraPositionDirty = true;
                cameraPositionObjectSpaceDirty = true;
                inverseProjectionMatrixDirty = true;
                inverseViewMatrixDirty = true;
                inverseViewProjMatrixDirty = true;
                inverseWorldViewMatrixDirty = true;
                inverseWorldViewProjMatrixDirty = true;
                projMatrixDirty = true;
                viewMatrixDirty = true;
                viewProjMatrixDirty = true;
                worldViewMatrixDirty = true;
                worldViewProjMatrixDirty = true;
                // inverseTransposeWorldViewMatrixDirty = true;
		    }
		}

		/// <summary>
		///		Get/Set the current frustum used for texture projection.
		/// </summary>
		public Frustum TextureProjector {
			get {
				return currentTextureProjector[0];
			}
			set {
				SetTextureProjector(value, 0);
			}
		}

        /// <summary>
        ///		Get/Set the current active render target in use.
        /// </summary>
        public RenderTarget RenderTarget {
            get {
                return currentRenderTarget;
            }
            set {
                currentRenderTarget = value;
            }
        }

        /// <summary>
        ///		Get/Set the current active viewport in use.
        /// </summary>
        public Viewport Viewport {
            get {
                return currentViewport;
            }
            set {
                currentViewport = value;
            }
        }

        /// <summary>
        ///    Gets/Sets the current global ambient light color.
        /// </summary>
        public ColorEx AmbientLight {
            get {
                return ambientLight;
            }
            set {
                ambientLight = value;
            }
        }

        /// <summary>
        ///    Gets/Sets the derived ambient light color.
        /// </summary>
        public ColorEx DerivedAmbientLight {
            get {
                return ambientLight * currentPass.Ambient;
            }
        }
        
        /// <summary>
        ///    Gets/Sets the current gpu fog parameters.
        /// </summary>
        public Vector4 FogParams
        {
            get
            {
                return fogParams;
            }
            set
            {
                fogParams = value;
            }
        }

        /// <summary>
        ///    Gets/Sets the current gpu fog color.
        /// </summary>
        public ColorEx FogColor
        {
            get
            {
                return fogColor;
            }
            set
            {
                fogColor = value;
            }
        }

        public void SetWorldMatrices(Matrix4 [] array, int count) {
            worldMatrixArray = array;
            worldMatrixCount = count;
            worldMatrixDirty = false;
        }
        
        /// <summary>
        ///    Gets the current world matrix.
        /// </summary>
        public Matrix4 WorldMatrix {
            get {
                if(worldMatrixDirty) {
                    worldMatrixArray = worldMatrix;
                    renderable.GetWorldTransforms(worldMatrix);
					worldMatrixCount = renderable.NumWorldTransforms;
                    worldMatrixDirty = false;
                }
                return worldMatrixArray[0];
            }
        }

		/// <summary>
		///    Gets the number of current world matrices.
		/// </summary>
		public int WorldMatrixCount {
			get {
				if(worldMatrixDirty) {
					worldMatrixArray = worldMatrix;
                    renderable.GetWorldTransforms(worldMatrix);
					worldMatrixCount = renderable.NumWorldTransforms;
					worldMatrixDirty = false;
				}
				return worldMatrixCount;
			}
		}

        /// <summary>
        ///    Gets/Sets the inverse of current world matrix.
        /// </summary>
		public Matrix4 InverseWorldMatrix {
		    get {
                if(inverseWorldMatrixDirty) {
                    inverseWorldMatrix = this.WorldMatrix.Inverse();
                    inverseWorldMatrixDirty = false;
                }
		        return inverseWorldMatrix;
		    }
		}

		/// <summary>
		///		Gets an array with all the current world matrix transforms.
		/// </summary>
		public Matrix4[] WorldMatrixArray {
			get {
				if(worldMatrixDirty) {
					worldMatrixArray = worldMatrix;
                    renderable.GetWorldTransforms(worldMatrix);
					worldMatrixCount = renderable.NumWorldTransforms;
					worldMatrixDirty = false;
				}
				return worldMatrixArray;
			}
		}

        /// <summary>
        ///    Gets/Sets the current concatenated world and view matrices.
        /// </summary>
		public Matrix4 WorldViewMatrix {
		    get {
                if(worldViewMatrixDirty) {
                    worldViewMatrix = this.ViewMatrix.ConcatenateAffine(this.WorldMatrix);
                    worldViewMatrixDirty = false;
                }
		        return worldViewMatrix;
		    }
		}

        /// <summary>
        ///    Gets/Sets the inverse of current concatenated world and view matrices.
        /// </summary>
		public Matrix4 InverseWorldViewMatrix {
		    get {
                if(inverseWorldViewMatrixDirty) {
                    inverseWorldViewMatrix = this.WorldViewMatrix.Inverse();
                    inverseWorldViewMatrixDirty = false;
                }
                return inverseWorldViewMatrix;
		    }
		}

        /// <summary>
        ///    Gets/Sets the current concatenated world, view, and projection matrices.
        /// </summary>
		public Matrix4 WorldViewProjMatrix {
		    get {
                if(worldViewProjMatrixDirty) {
                    worldViewProjMatrix = this.ProjectionMatrix * this.WorldViewMatrix;
                    worldViewProjMatrixDirty = false;
                }
		        return worldViewProjMatrix;
		    }
		}

        /// <summary>
        ///    Gets/Sets the inverse of current concatenated world
        ///    view and projection matrices.
        /// </summary>
		public Matrix4 InverseWorldViewProjMatrix {
		    get {
                if(inverseWorldViewProjMatrixDirty) {
                    inverseWorldViewProjMatrix = this.WorldViewProjMatrix.Inverse();
                    inverseWorldViewProjMatrixDirty = false;
                }
                return inverseWorldViewProjMatrix;
		    }
		}

        /// <summary>
        ///    Gets/Sets the position of the current camera in object space relative to the current renderable.
        /// </summary>
		public Vector4 CameraPositionObjectSpace {
		    get {
                if(cameraPositionObjectSpaceDirty) {
                    cameraPositionObjectSpace = (Vector4)(this.InverseWorldMatrix * camera.DerivedPosition);
                    cameraPositionObjectSpaceDirty = false;
                }
		        return cameraPositionObjectSpace;
		    }
		}

        /// <summary>
        ///    Gets the position of the current camera in world space.
        /// </summary>
        public Vector3 CameraPosition {
            get {
                return camera.DerivedPosition;
            }
        }

        /// <summary>
        ///    Gets/Sets the current projection matrix.
        /// </summary>
        public Matrix4 ProjectionMatrix {
            get {
                if (projMatrixDirty) {
                    // NB use API-independent projection matrix since GPU programs
                    // bypass the API-specific handedness and use right-handed coords
                    if (renderable != null && renderable.UseIdentityProjection) {
                        // Use identity projection matrix, still need to take RS depth into account
                        projectionMatrix = 
                            Root.Instance.RenderSystem.ConvertProjectionMatrix(Matrix4.Identity, true);
                    } else {
                        projectionMatrix = camera.ProjectionMatrixWithRSDepth;
                    }
                    if (currentRenderTarget != null && currentRenderTarget.RequiresTextureFlipping) {
                        projectionMatrix.m10 = -projectionMatrix.m10;
                        projectionMatrix.m11 = -projectionMatrix.m11;
                        projectionMatrix.m12 = -projectionMatrix.m12;
                        projectionMatrix.m13 = -projectionMatrix.m12;
                    }
                    projMatrixDirty = false;
				}
                return projectionMatrix;
            }
        }

        /// <summary>
        ///    Gets/Sets the inverse of current concatenated projection matrices.
        /// </summary>
		public Matrix4 InverseProjectionMatrix {
            get {
                if(inverseProjectionMatrixDirty) {
                    inverseProjectionMatrix = this.ProjectionMatrix.Inverse();
                    inverseProjectionMatrixDirty = false;
                }
                return inverseProjectionMatrix;
            }
		}

        /// <summary>
        ///    Gets/Sets the current view matrix.
        /// </summary>
        public Matrix4 ViewMatrix {
            get {
                if (viewMatrixDirty) {
                    if (renderable != null && renderable.UseIdentityView)
                        viewMatrix = Matrix4.Identity;
                    else
                        viewMatrix = camera.ViewMatrix;
                    viewMatrixDirty = false;
                }
                return viewMatrix;
            }
        }

        /// <summary>
        ///    Gets/Sets the inverse of current concatenated view matrices.
        /// </summary>
		public Matrix4 InverseViewMatrix {
            get {
                if(inverseViewMatrixDirty) {
                    inverseViewMatrix = this.ViewMatrix.Inverse();
                    inverseViewMatrixDirty = false;
                }
                return inverseViewMatrix;
            }
		}

		/// <summary>
		///		Gets the projection and view matrices concatenated.
		/// </summary>
		public Matrix4 ViewProjectionMatrix {
			get {
				if (viewProjMatrixDirty) {
					viewProjMatrix = this.ProjectionMatrix * this.ViewMatrix;
					viewProjMatrixDirty = false;
				}

				return viewProjMatrix;
			}
		}

		/// <summary>
		///		Gets the projection and view matrices concatenated.
		/// </summary>
		public Matrix4 InverseViewProjMatrix {
			get {
				if (inverseViewProjMatrixDirty) {
					inverseViewProjMatrix = this.ViewProjectionMatrix.Inverse();
					inverseViewProjMatrixDirty = false;
				}

				return inverseViewProjMatrix;
			}
		}

		/// <summary>
		///		Get the extrusion distance for shadow volume vertices.
		/// </summary>
		public float ShadowExtrusionDistance {
			get {
				// only ever applies to one light at once
				Light light = GetLight(0);

				if(light.Type == LightType.Directional) {
					// use constant value
					return dirLightExtrusionDistance;
				}
				else {
					// Calculate based on object space light distance
					// compared to light attenuation range
					Vector3 objPos = this.InverseWorldMatrix * light.DerivedPosition;
					return light.AttenuationRange - objPos.Length;
				}
			}
		}

        /// <summary>
        /// Get the derived camera position (which includes any parent sceneNode transforms)
        /// </summary>
        public Vector3 ViewDirection
        {
            get
            {
                return camera.DerivedDirection;
            }
        }

        /// <summary>
        /// Get the derived camera right vector (which includes any parent sceneNode transforms)
        /// </summary>
        public Vector3 ViewSideVector
        {
            get
            {
                return camera.DerivedRight;
            }
        }

        /// <summary>
        /// Get the derived camera up vector (which includes any parent sceneNode transforms)
        /// </summary>
        public Vector3 ViewUpVector
        {
            get
            {
                return camera.DerivedUp;
            }
        }

        public float NearClipDistance
        {
            get
            {
                return camera.Near;
            }
        }

        public float FarClipDistance
        {
            get
            {
                return camera.Far;
            }
        }

        public float Time
        {
            get
            {
                return ControllerManager.Instance.GetElapsedTime();
            }
        }

        public Vector4 MVShadowTechnique
        {
            get
            {
                return mvShadowTechnique;
            }
            set
            {
                mvShadowTechnique = value;
            }
        }
	
        /// <summary>
        /// The technique pass number
        /// </summary>
        public int PassNumber
        {
            get
            {
                return passNumber;
            }
            set
            {
                passNumber = value;
            }
        }
	
        /// <summary>
        /// The technique pass iteration number
        /// </summary>
        public int PassIterationNumber
        {
            get
            {
                return passIterationNumber;
            }
            set
            {
                passIterationNumber = value;
            }
        }
	
        /// <summary>
        /// The technique pass
        /// </summary>
        public Pass CurrentPass
        {
            get
            {
                return currentPass;
            }
            set
            {
                currentPass = value;
            }
        }
	
        public VisibleObjectsBoundsInfo MainCamBoundsInfo
        {
            get {
                return mainCamBoundsInfo;
            }
            set {
                mainCamBoundsInfo = value;
                sceneDepthRangeDirty = true;
            }
        }
	
        public Vector4 SceneDepthRange {
            get {
                if (sceneDepthRangeDirty) {
                    // calculate depth information
                    sceneDepthRange.x = mainCamBoundsInfo.minDistance;
                    sceneDepthRange.y = mainCamBoundsInfo.maxDistance;
                    sceneDepthRange.z = mainCamBoundsInfo.maxDistance - mainCamBoundsInfo.minDistance;
                    sceneDepthRange.w = 1.0f / sceneDepthRange.z;
                    sceneDepthRangeDirty = false;
                }
                return sceneDepthRange;
            }
        }

		public SceneManager CurrentSceneManager {
            set {
                currentSceneManager = value;
            }
        }

        public Vector4 ShadowFadeParams
        {
            get
            {
                return shadowFadeParams;
            }
            set
            {
                shadowFadeParams = value;
            }
        }
        
        #endregion
	}
}
