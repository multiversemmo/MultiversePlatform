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
using Axiom.Configuration;
using Axiom.Core;

namespace Axiom.Graphics {
	/// <summary>
	/// 	Class defining a single pass of a Technique (of a Material), ie
	///    a single rendering call. 
	/// </summary>
	/// <remarks>
	///    Rendering can be repeated with many passes for more complex effects.
	///    Each pass is either a fixed-function pass (meaning it does not use
	///    a vertex or fragment program) or a programmable pass (meaning it does
	///    use either a vertex or a fragment program, or both). 
	///    <p/>
	///    Programmable passes are complex to define, because they require custom
	///    programs and you have to set all constant inputs to the programs (like
	///    the position of lights, any base material colors you wish to use etc), but
	///    they do give you much total flexibility over the algorithms used to render your
	///    pass, and you can create some effects which are impossible with a fixed-function pass.
	///    On the other hand, you can define a fixed-function pass in very little time, and
	///    you can use a range of fixed-function effects like environment mapping very
	///    easily, plus your pass will be more likely to be compatible with older hardware.
	///    There are pros and cons to both, just remember that if you use a programmable
	///    pass to create some great effects, allow more time for definition and testing.
	/// </remarks>
	public class Pass {
		#region Fields

		/// <summary>
		///    A reference to the technique that owns this Pass.
		/// </summary>
		protected Technique parent;
		/// <summary>
		///    Index of this rendering pass.
		/// </summary>
		protected int index;
        /// <summary>
        ///     Name of this pass (or the index if it isn't set)
        /// </summary>
        protected string name;
		/// <summary>
		///    Pass hash, used for sorting passes.
		/// </summary>
		protected int hashCode;
		/// <summary>
		///    Ambient color in fixed function passes.
		/// </summary>
		protected ColorEx ambient;
		/// <summary>
		///    Diffuse color in fixed function passes.
		/// </summary>
		protected ColorEx diffuse;
		/// <summary>
		///    Specular color in fixed function passes.
		/// </summary>
		protected ColorEx specular;
		/// <summary>
		///    Emissive color in fixed function passes.
		/// </summary>
		protected ColorEx emissive;
		/// <summary>
		///    Shininess of the object's surface in fixed function passes.
		/// </summary>
		protected float shininess;
        /// <summary>
        ///    Color parameters that should track the vertex color for fixed function passes.
        /// </summary>
        protected TrackVertexColor tracking = TrackVertexColor.None;
		/// <summary>
		///    Source blend factor.
		/// </summary>
		protected SceneBlendFactor sourceBlendFactor;
		/// <summary>
		///    Destination blend factor.
		/// </summary>
		protected SceneBlendFactor destBlendFactor;
		/// <summary>
		///    Depth buffer checking setting for this pass.
		/// </summary>
		protected bool depthCheck;
		/// <summary>
		///    Depth write setting for this pass.
		/// </summary>
		protected bool depthWrite;
		/// <summary>
		///    Depth comparison function for this pass.
		/// </summary>
		protected CompareFunction depthFunc;
		/// <summary>
		///    Depth constant bias for this pass.
		/// </summary>
		protected float depthBiasConstant;
		/// <summary>
		///    Depth slope bias for this pass.
		/// </summary>
		protected float depthBiasSlopeScale;
		/// <summary>
		///    Color write setting for this pass.
		/// </summary>
		protected bool colorWrite;
		/// <summary>
		///    Alpha reject function
		/// </summary>
		CompareFunction alphaRejectFunction;
		/// <summary>
		///    Alpha reject value
		/// </summary>
		int alphaRejectValue;
		/// <summary>
		///    Hardware culling mode for this pass.
		/// </summary>
		protected CullingMode cullMode;
		/// <summary>
		///    Software culling mode for this pass.
		/// </summary>
		protected ManualCullingMode manualCullMode;
		/// <summary>
		///    Is lighting enabled for this pass?
		/// </summary>
		protected bool lightingEnabled;
		/// <summary>
		///    Max number of simultaneous lights that can be used for this pass.
		/// </summary>
		protected int maxLights;
		/// <summary>
		///    Starting light index
		/// </summary>
		protected int startLight;
		/// <summary>
		///    Run this pass once per light? 
		/// </summary>
		protected bool runOncePerLight; 
		/// <summary>
		///    Iterate per how many lights?
		/// </summary>
		protected int lightsPerIteration;
		/// <summary>
		///     Should it only be run for a certain light type? 
		/// </summary>
		protected bool runOnlyForOneLightType; 
		/// <summary>
		///    Type of light for a programmable pass that supports only one particular type of light.
		/// </summary>
		protected LightType onlyLightType; 
		/// <summary>
		///    Shading options for this pass.
		/// </summary>
		protected Shading shadeOptions;
		/// <summary>
		///    Texture anisotropy level.
		/// </summary>
		protected int maxAniso;
		/// <summary>
		///    Does this pass override global fog settings?
		/// </summary>
		protected bool fogOverride;
		/// <summary>
		///    Fog mode to use for this pass (if overriding).
		/// </summary>
		protected FogMode fogMode;
		/// <summary>
		///    Color of the fog used for this pass (if overriding).
		/// </summary>
		protected ColorEx fogColor;
		/// <summary>
		///    Starting point of the fog for this pass (if overriding).
		/// </summary>
		protected float fogStart;
		/// <summary>
		///    Ending point of the fog for this pass (if overriding).
		/// </summary>
		protected float fogEnd;
		/// <summary>
		///    Density of the fog for this pass (if overriding).
		/// </summary>
		protected float fogDensity;
		/// <summary>
		///    List of fixed function texture unit states for this pass.
		/// </summary>
		protected TextureUnitStateList textureUnitStates = new TextureUnitStateList();
		/// <summary>
		///    Details on the vertex program to be used for this pass.
		/// </summary>
		protected GpuProgramUsage vertexProgramUsage;
		/// <summary>
		///    Details on the fragment program to be used for this pass.
		/// </summary>
		protected GpuProgramUsage fragmentProgramUsage;
		/// <summary>
		///    Details on the shadow caster vertex program to be used for this pass.
		/// </summary>
		protected GpuProgramUsage shadowCasterVertexProgramUsage;
        /// <summary>
        ///    Details on the shadow caster fragment program to be used for this pass.
        /// </summary>
        protected GpuProgramUsage shadowCasterFragmentProgramUsage;
		/// <summary>
		///    Details on the shadow receiver vertex program to be used for this pass.
		/// </summary>
		protected GpuProgramUsage shadowReceiverVertexProgramUsage;
		/// <summary>
		///    Details on the shadow receiver fragment program to be used for this pass.
		/// </summary>
		protected GpuProgramUsage shadowReceiverFragmentProgramUsage;
		/// <summary>
		///		Is this pass queued for deletion?
		/// </summary>
		protected bool queuedForDeletion;
		/// <summary>
		///		Solid, Points or WireFrame
		/// </summary>
		protected SceneDetailLevel sceneDetail;
		/// <summary>
		///		Number of pass iterations to perform
		/// </summary>
		protected int passIterationCount;
		/// <summary>
		///		Point size, etc.  Applies when not using per-vertex point size
		/// </summary>
		protected float pointSize;
		protected float pointMinSize;
		protected float pointMaxSize;
		protected bool pointSpritesEnabled;
		protected bool pointAttenuationEnabled;
		// constant, linear, quadratic coeffs
		protected float pointAttenuationConstant;
        protected float pointAttenuationLinear;
        protected float pointAttenuationQuadratic;
		/// <summary>
		///		The cache of content types
		/// </summary>
        protected bool contentTypeLookupBuilt;
        protected List<int> shadowContentTypeLookup;

        /// <summary>
		///		List of passes with dirty hashes.
		/// </summary>
		protected static PassList dirtyHashList = new PassList();
		/// <summary>
		///		List of passes queued for deletion.
		/// </summary>
		protected static PassList graveyardList = new PassList();

        protected static int nextPassId = 0;
        protected static Object passLock = new Object();

        public int passId;

		#endregion
		
		#region Constructors
		
		/// <summary>
		///    Default constructor.
		/// </summary>
		/// <param name="parent">Technique that owns this Pass.</param>
		/// <param name="index">Index of this pass.</param>
		public Pass(Technique parent, int index) {
			this.parent = parent;
			this.index = index;
            
            lock (passLock) {
                this.passId = nextPassId++;
            }

			// color defaults
			ambient = ColorEx.White;
			diffuse = ColorEx.White;
			specular = ColorEx.Black;
			emissive = ColorEx.Black;

			// by default, don't override the scene's fog settings
			fogOverride = false;
			fogMode = FogMode.None;
			fogColor = ColorEx.White;
			fogStart = 0;
			fogEnd = 1;
			fogDensity = 0.001f;

			// default blending (overwrite)
			sourceBlendFactor = SceneBlendFactor.One;
			destBlendFactor = SceneBlendFactor.Zero;



			// depth buffer settings
			depthCheck = true;
			depthWrite = true;
			colorWrite = true;
            alphaRejectFunction = CompareFunction.AlwaysPass;
            alphaRejectValue = 0;
			depthFunc = CompareFunction.LessEqual;

            depthBiasConstant = 0f;
            depthBiasSlopeScale = 0f;
            
            // cull settings
			cullMode = CullingMode.Clockwise;
			manualCullMode = ManualCullingMode.Back;

			// light settings
			lightingEnabled = true;
			runOnlyForOneLightType = true;
			lightsPerIteration = 1;
            runOncePerLight = false;
            onlyLightType = LightType.Point;
			shadeOptions = Shading.Gouraud;

			// Default max lights to the global max
			maxLights = Config.MaxSimultaneousLights;

            // Starting light index
            startLight = 0;

            // Default to solid
            sceneDetail = SceneDetailLevel.Solid;

            // Iteration count of 1
            passIterationCount = 1;
            
            // pointSize, etc.
            pointSize = 1.0f;
            pointMinSize = 0f;
            pointMaxSize = 0f;
            pointSpritesEnabled = false;
            pointAttenuationEnabled = false;
            pointAttenuationConstant = 1.0f;
            pointAttenuationLinear = 0f;
            pointAttenuationQuadratic = 0f;

            contentTypeLookupBuilt = false;
            
            name = index.ToString();

			DirtyHash();
		}
		
		#endregion
		
		#region Methods

		/// <summary>
		///    Adds the passed in TextureUnitState, to the existing Pass.
		/// </summary>
		/// <param name="state">TextureUnitState to add to this pass.</param>
		public void AddTextureUnitState(TextureUnitState state) {
			Debug.Assert(state != null, "state is null in Pass.AddTextureUnitState()");
            if (state != null) {
                // only attach TUS to pass if TUS does not belong to another pass
                if ((state.Parent == null) || (state.Parent == this)) {
                    textureUnitStates.Add(state);
                    // Notify state
                    state.Parent = this;
                    // if texture unit state name is empty then give it a default name based on its index
                    if (state.Name == null || state.Name == "") {
                        // its the last entry in the container so its index is size - 1
                        int idx = textureUnitStates.Count - 1;
                        state.Name = idx.ToString();
                        // since the name was never set and a default one has been made, clear the alias name
                        // so that when the texture unit name is set by the user, the alias name will be set to
                        // that name
                        state.TextureNameAlias = null;
                    }
                    // needs recompilation
                    parent.NotifyNeedsRecompile();
                    DirtyHash();
                }
                else
                    throw new AxiomException("TextureUnitState already attached to another pass" + 
                        "  In Pass.AddTextureUnitState");
                contentTypeLookupBuilt = false;
            }
        }

		/// <summary>
		///    Method for cloning a Pass object.
		/// </summary>
		/// <param name="parent">Parent technique that will own this cloned Pass.</param>
		/// <returns></returns>
		public Pass Clone(Technique parent, int index) {
			Pass newPass = new Pass(parent, index);

			CopyTo(newPass);

			// dirty the hash on the new pass
			newPass.DirtyHash();

			return newPass;
		}

		/// <summary>
		///		Copy the details of this pass to the target pass.
		/// </summary>
		/// <param name="target">Destination pass to copy this pass's attributes to.</param>
		public void CopyTo(Pass target) {
            target.name = name;
            target.hashCode = hashCode;
			
            // surface
			target.ambient = ambient.Clone();
			target.diffuse = diffuse.Clone();
			target.specular = specular.Clone();
			target.emissive = emissive.Clone();
			target.shininess = shininess;
            target.tracking = tracking;

			// fog
			target.fogOverride = fogOverride;
			target.fogMode = fogMode;
			target.fogColor = fogColor.Clone();
			target.fogStart = fogStart;
			target.fogEnd = fogEnd;
			target.fogDensity = fogDensity;

			// default blending
			target.sourceBlendFactor = sourceBlendFactor;
			target.destBlendFactor = destBlendFactor;

			target.depthCheck = depthCheck;
			target.depthWrite = depthWrite;
            target.alphaRejectFunction = alphaRejectFunction;
            target.alphaRejectValue = alphaRejectValue;
            target.colorWrite = colorWrite;
			target.depthFunc = depthFunc;
			target.depthBiasConstant = depthBiasConstant;
			target.depthBiasSlopeScale = depthBiasSlopeScale;
			target.cullMode = cullMode;
			target.manualCullMode = manualCullMode;
			target.lightingEnabled = lightingEnabled;
			target.maxLights = maxLights;
			target.startLight = startLight;
            target.runOncePerLight = runOncePerLight;
			target.lightsPerIteration = lightsPerIteration;
            target.runOnlyForOneLightType = runOnlyForOneLightType;
			target.onlyLightType = onlyLightType;
			target.shadeOptions = shadeOptions;
            target.sceneDetail = sceneDetail;
            target.passIterationCount = passIterationCount;
            target.pointSize = pointSize;
            target.pointMinSize = pointMinSize;
            target.pointMaxSize = pointMaxSize;
            target.pointSpritesEnabled = pointSpritesEnabled;
            target.pointAttenuationEnabled = pointAttenuationEnabled;
            target.pointAttenuationConstant = pointAttenuationConstant;
            target.pointAttenuationLinear = pointAttenuationLinear;
            target.pointAttenuationQuadratic = pointAttenuationQuadratic;
            target.contentTypeLookupBuilt = contentTypeLookupBuilt;

			// vertex program
			if(vertexProgramUsage != null) {
                target.vertexProgramUsage = vertexProgramUsage.Clone();
            }
			else {
                target.vertexProgramUsage = null;
            }

            // shadow caster vertex program
            if (shadowCasterVertexProgramUsage != null) {
                target.shadowCasterVertexProgramUsage = shadowCasterVertexProgramUsage.Clone();
            } else {
                target.shadowCasterVertexProgramUsage = null;
            }
            
            // shadow receiver vertex program
            if (shadowReceiverVertexProgramUsage != null) {
                target.shadowReceiverVertexProgramUsage = shadowReceiverVertexProgramUsage.Clone();
            } else {
                target.shadowReceiverVertexProgramUsage = null;
            }
			
            // fragment program
			if(fragmentProgramUsage != null) {
                target.fragmentProgramUsage = fragmentProgramUsage.Clone();
            }
			else {
                target.fragmentProgramUsage = null;
            }

            // shadow caster fragment program
            if (shadowCasterFragmentProgramUsage != null) {
                target.shadowCasterFragmentProgramUsage = shadowCasterFragmentProgramUsage.Clone();
            } else {
                target.shadowCasterFragmentProgramUsage = null;
            }

			// shadow receiver fragment program
			if(shadowReceiverFragmentProgramUsage != null) {
                target.shadowReceiverFragmentProgramUsage = shadowReceiverFragmentProgramUsage.Clone();
            }
			else {
                target.shadowReceiverFragmentProgramUsage = null;
            }

			// texture units
			target.RemoveAllTextureUnitStates();

			for(int i = 0; i < textureUnitStates.Count; i++) {
				TextureUnitState newState = new TextureUnitState(target);
				TextureUnitState src = (TextureUnitState)textureUnitStates[i];
				src.CopyTo(newState);

				target.textureUnitStates.Add(newState);
			}

			target.DirtyHash();
		}

		/// <summary>
		///    Overloaded method.
		/// </summary>
		/// <param name="textureName">The basic name of the texture (i.e. brickwall.jpg)</param>
		/// <returns></returns>
		public TextureUnitState CreateTextureUnitState() {
			TextureUnitState state = new TextureUnitState(this);
			AddTextureUnitState(state);
            contentTypeLookupBuilt = false;
			return state;
		}	

		/// <summary>
		///    Overloaded method.
		/// </summary>
		/// <param name="textureName">The basic name of the texture (i.e. brickwall.jpg)</param>
		/// <returns></returns>
		public TextureUnitState CreateTextureUnitState(string textureName) {
			return CreateTextureUnitState(textureName, 0);
		}

		/// <summary>
		///    Inserts a new TextureUnitState object into the Pass.
		/// </summary>
		/// <remarks>
		///    This unit is is added on top of all previous texture units.
		///    <p/>
		///    Applies to both fixed-function and programmable passes.
		/// </remarks>
		/// <param name="textureName">The basic name of the texture (i.e. brickwall.jpg)</param>
		/// <param name="texCoordSet">The index of the texture coordinate set to use.</param>
		/// <returns></returns>
		public TextureUnitState CreateTextureUnitState(string textureName, int texCoordSet) {
			TextureUnitState state = new TextureUnitState(this);
			state.SetTextureName(textureName);
			state.TextureCoordSet = texCoordSet;
			AddTextureUnitState(state);
            contentTypeLookupBuilt = false;
			return state;
		}

		/// <summary>
		///    Retrieves the Texture Unit State matching name.
		///    Returns 0 if name match is not found.
		/// </summary>
        public TextureUnitState GetTextureUnitState(string name) {
            for (int i=0; i<textureUnitStates.Count; i++) {
                TextureUnitState t = (TextureUnitState)textureUnitStates[i];
                if (t.Name == name)
                    return t;
            }
            return null;
        }

		/// <summary>
		///    Gets a reference to the TextureUnitState for this pass at the specified indx.
		/// </summary>
		/// <param name="index">Index of the state to retreive.</param>
		/// <returns>TextureUnitState at the specified index.</returns>
		public TextureUnitState GetTextureUnitState(int index) {
			Debug.Assert(index < textureUnitStates.Count, "index < textureUnitStates.Count");

			return (TextureUnitState)textureUnitStates[index];
		}

		/// <summary>
		///    Retrieve the index of the Texture Unit State in the pass.
		/// </summary>
        public int GetTextureUnitStateIndex(TextureUnitState state) {
            Debug.Assert(state != null, "state is 0 in Pass.AddTextureUnitState()");
            // only find index for state attached to this pass
            if (state.Parent == this) {
                // iterate through TUS container and find matching pointer to state
                for (int i=0; i<textureUnitStates.Count; i++) {
                    TextureUnitState t = (TextureUnitState)textureUnitStates[i];
                    if (t == state)
                        return i;
                }
            }
            else
                throw new AxiomException("TextureUnitState is not attached to this pass" + 
                    "  In Pass.GetTextureUnitStateIndex");
            return 0;
        }

		/// <summary>
		///    Internal method for loading this pass.
		/// </summary>
		internal void Load() {
			// it is assumed this is only being called when the Material is being loaded

			// load each texture unit state
			for(int i = 0; i < textureUnitStates.Count; i++) {
				((TextureUnitState)textureUnitStates[i]).Load();
			}

			// load programs
			if(this.HasVertexProgram) {
				// load vertex program
				vertexProgramUsage.Load();
			}

			if(this.HasFragmentProgram) {
				// load vertex program
				fragmentProgramUsage.Load();
			}

			if(this.HasShadowCasterVertexProgram) {
				// load shadow caster vertex program
				shadowCasterVertexProgramUsage.Load();
			}

            if (this.HasShadowCasterFragmentProgram)
            {
                // load shadow caster fragment program
                shadowCasterFragmentProgramUsage.Load();
            }

			if(this.HasShadowReceiverVertexProgram) {
				// load shadow receiver vertex program
				shadowReceiverVertexProgramUsage.Load();
			}

			if(this.HasShadowReceiverFragmentProgram) {
				// load shadow receiver fragment program
				shadowReceiverFragmentProgramUsage.Load();
			}

			// recalculate hash code
			DirtyHash();
		}

		/// <summary>
		///    Tells the pass that it needs recompilation.
		/// </summary>
		internal void NotifyNeedsRecompile() {
			parent.NotifyNeedsRecompile();
		}

		/// <summary>
		///    Internal method for recalculating the hash code used for sorting passes.
		/// </summary>
		internal void RecalculateHash() {
			/* Hash format is 32-bit, divided as follows (high to low bits)
			   bits   purpose
				4     Pass index (i.e. max 16 passes!)
			   14     Hashed texture name from unit 0
			   14     Hashed texture name from unit 1

			   Note that at the moment we don't sort on the 3rd texture unit plus
			   on the assumption that these are less frequently used; sorting on 
			   the first 2 gives us the most benefit for now.
		   */

			hashCode = (index << 28);
			int count = NumTextureUnitStages;

			if(count > 0 && !((TextureUnitState)textureUnitStates[0]).IsBlank) {
				hashCode += (((TextureUnitState)textureUnitStates[0]).TextureName.GetHashCode() & ( ( 1 << 14 ) - 1)) << 14;
			}
			if(count > 1 && !((TextureUnitState)textureUnitStates[1]).IsBlank) {
				hashCode += (((TextureUnitState)textureUnitStates[1]).TextureName.GetHashCode() & ( ( 1 << 14) - 1));
			}
		}

		/// <summary>
		///    Removes all texture unit settings from this pass.
		/// </summary>
		public void RemoveAllTextureUnitStates() {
			textureUnitStates.Clear();

			if(!queuedForDeletion) {
				// needs recompilation
				parent.NotifyNeedsRecompile();
			}

			DirtyHash();
            contentTypeLookupBuilt = false;
		}

		/// <summary>
		///    Removes the specified TextureUnitState from this pass.
		/// </summary>
		/// <param name="state">A reference to the TextureUnitState to remove from this pass.</param>
		public void RemoveTextureUnitState(TextureUnitState state) {
			textureUnitStates.Remove(state);

			if(!queuedForDeletion) {
				// needs recompilation
				parent.NotifyNeedsRecompile();
			}

			DirtyHash();
            contentTypeLookupBuilt = false;
		}

		/// <summary>
		///    Removes the specified TextureUnitState from this pass.
		/// </summary>
		/// <param name="state">Index of the TextureUnitState to remove from this pass.</param>
		public void RemoveTextureUnitState(int index) {
			TextureUnitState state = (TextureUnitState) textureUnitStates[index];

			if (state != null)
				RemoveTextureUnitState(state);
		}

		/// <summary>
		///    Sets the fogging mode applied to this pass.
		/// </summary>
		/// <remarks>
		///    Fogging is an effect that is applied as polys are rendered. Sometimes, you want
		///    fog to be applied to an entire scene. Other times, you want it to be applied to a few
		///    polygons only. This pass-level specification of fog parameters lets you easily manage
		///    both.
		///    <p/>
		///    The SceneManager class also has a SetFog method which applies scene-level fog. This method
		///    lets you change the fog behavior for this pass compared to the standard scene-level fog.
		/// </remarks>
		/// <param name="overrideScene">
		///    If true, you authorise this pass to override the scene's fog params with it's own settings.
		///    If you specify false, so other parameters are necessary, and this is the default behaviour for passs.
		/// </param>
		/// <param name="mode">
		///    Only applicable if <paramref cref="overrideScene"/> is true. You can disable fog which is turned on for the
		///    rest of the scene by specifying FogMode.None. Otherwise, set a pass-specific fog mode as
		///    defined in the enum FogMode.
		/// </param>
		/// <param name="color">
		///    The color of the fog. Either set this to the same as your viewport background color,
		///    or to blend in with a skydome or skybox.
		/// </param>
		/// <param name="density">
		///    The density of the fog in FogMode.Exp or FogMode.Exp2 mode, as a value between 0 and 1. 
		///    The default is 0.001.
		/// </param>
		/// <param name="start">
		///    Distance in world units at which linear fog starts to encroach. 
		///    Only applicable if mode is FogMode.Linear.
		/// </param>
		/// <param name="end">
		///    Distance in world units at which linear fog becomes completely opaque.
		///    Only applicable if mode is FogMode.Linear.
		/// </param>
		public void SetFog(bool overrideScene, FogMode mode, ColorEx color, float density, float start, float end) {
			fogOverride = overrideScene;

			// set individual params if overriding scene level fog
			if(overrideScene) {
				fogMode = mode;
				fogColor = color;
				fogDensity = density;
				fogStart = start;
				fogEnd = end;
			}
		}

		/// <summary>
		///    Overloaded method.
		/// </summary>
		/// <param name="overrideScene">
		///    If true, you authorise this pass to override the scene's fog params with it's own settings.
		///    If you specify false, so other parameters are necessary, and this is the default behaviour for passs.
		/// </param>
		public void SetFog(bool overrideScene) {
			SetFog(overrideScene, FogMode.None, ColorEx.White, 0.001f, 0.0f, 1.0f);
		}

		/// <summary>
		///    Overloaded method.
		/// </summary>
		/// <param name="overrideScene">
		///    If true, you authorise this pass to override the scene's fog params with it's own settings.
		///    If you specify false, so other parameters are necessary, and this is the default behaviour for passs.
		/// </param>
		/// <param name="mode">
		///    Only applicable if <paramref cref="overrideScene"/> is true. You can disable fog which is turned on for the
		///    rest of the scene by specifying FogMode.None. Otherwise, set a pass-specific fog mode as
		///    defined in the enum FogMode.
		/// </param>
		public void SetFog(bool overrideScene, FogMode mode) {
			SetFog(overrideScene, mode, ColorEx.White, 0.001f, 0.0f, 1.0f);
		}

		/// <summary>
		///    Overloaded method.
		/// </summary>
		/// <param name="overrideScene">
		///    If true, you authorise this pass to override the scene's fog params with it's own settings.
		///    If you specify false, so other parameters are necessary, and this is the default behaviour for passs.
		/// </param>
		/// <param name="mode">
		///    Only applicable if <paramref cref="overrideScene"/> is true. You can disable fog which is turned on for the
		///    rest of the scene by specifying FogMode.None. Otherwise, set a pass-specific fog mode as
		///    defined in the enum FogMode.
		/// </param>
		/// <param name="color">
		///    The color of the fog. Either set this to the same as your viewport background color,
		///    or to blend in with a skydome or skybox.
		/// </param>
		public void SetFog(bool overrideScene, FogMode mode, ColorEx color) {
			SetFog(overrideScene, mode, color, 0.001f, 0.0f, 1.0f);
		}

		/// <summary>
		///    Overloaded method.
		/// </summary>
		/// <param name="overrideScene">
		///    If true, you authorise this pass to override the scene's fog params with it's own settings.
		///    If you specify false, so other parameters are necessary, and this is the default behaviour for passs.
		/// </param>
		/// <param name="mode">
		///    Only applicable if <paramref cref="overrideScene"/> is true. You can disable fog which is turned on for the
		///    rest of the scene by specifying FogMode.None. Otherwise, set a pass-specific fog mode as
		///    defined in the enum FogMode.
		/// </param>
		/// <param name="color">
		///    The color of the fog. Either set this to the same as your viewport background color,
		///    or to blend in with a skydome or skybox.
		/// </param>
		/// <param name="density">
		///    The density of the fog in FogMode.Exp or FogMode.Exp2 mode, as a value between 0 and 1. 
		///    The default is 0.001.
		/// </param>
		public void SetFog(bool overrideScene, FogMode mode, ColorEx color, float density) {
			SetFog(overrideScene, mode, color, density, 0.0f, 1.0f);
		}

		/// <summary>
		///    Sets whether or not this pass should be run n times per light which 
		///    can affect the object being rendered.
		/// </summary>
		/// <remarks>
		///    The default behavior for a pass (when this option is 'false'), is 
		///    for a pass to be rendered only once, with all the lights which could 
		///    affect this object set at the same time (up to the maximum lights 
		///    allowed in the render system, which is typically 8). 
		///    <p/>
		///    Setting this option to 'true' changes this behavior, such that 
		///    instead of trying to issue render this pass once per object, it 
		///    is run once <b>per light</b> which can affect this object. In 
		///    this case, only light index 0 is ever used, and is a different light 
		///    every time the pass is issued, up to the total number of lights 
		///    which is affecting this object. This has 2 advantages: 
		///    <ul><li>There is no limit on the number of lights which can be 
		///    supported</li> 
		///    <li>It's easier to write vertex / fragment programs for this because 
		///    a single program can be used for any number of lights</li> 
		///    </ul> 
		///    However, this technique is a lot more expensive, and typically you 
		///    will want an additional ambient pass, because if no lights are 
		///    affecting the object it will not be rendered at all, which will look 
		///    odd even if ambient light is zero (imagine if there are lit objects 
		///    behind it - the objects silhouette would not show up). Therefore, 
		///    use this option with care, and you would be well advised to provide 
		///    a less expensive fallback technique for use in the distance. 
		///    <p/>
		///    Note: The number of times this pass runs is still limited by the maximum 
		///    number of lights allowed as set in MaxLights, so 
		///    you will never get more passes than this. 
		/// </remarks>
		/// <param name="enabled">Whether this feature is enabled.</param>
		/// <param name="iterationCount">The number of iterations.</param>
		/// <param name="lightCount">The number of lights per iteration.</param>
        /// <param name="onlyForOneLightType">
		///    If true, the pass will only be run for a single type of light, other light types will be ignored. 
		/// </param>
		/// <param name="lightType">The single light type which will be considered for this pass.</param>
		public void SetRunNTimesPerLight(bool enabled, int iterationCount, int lightCount, 
                                         bool onlyForOneLightType, LightType lightType) {
			runOncePerLight = enabled;
			passIterationCount = iterationCount;
            lightsPerIteration = lightCount;
            runOnlyForOneLightType = onlyForOneLightType;
			onlyLightType = lightType;
		}

		public void SetRunOncePerLight(bool enabled, bool onlyForOneLightType) {
			SetRunNTimesPerLight(enabled, 1, 1, onlyForOneLightType, LightType.Point);
		}

		public void SetRunOncePerLight(bool enabled) {
			SetRunOncePerLight(enabled, true);
		}

		/// <summary>
		///    Sets the kind of blending this pass has with the existing contents of the scene.
		/// </summary>
		/// <remarks>
		///    Whereas the texture blending operations seen in the TextureUnitState class are concerned with
		///    blending between texture layers, this blending is about combining the output of the Pass
		///    as a whole with the existing contents of the rendering target. This blending therefore allows
		///    object transparency and other special effects. If all passes in a technique have a scene
		///    blend, then the whole technique is considered to be transparent.
		///    <p/>
		///    This method allows you to select one of a number of predefined blending types. If you require more
		///    control than this, use the alternative version of this method which allows you to specify source and
		///    destination blend factors.
		///    <p/>
		///    This method is applicable for both the fixed-function and programmable pipelines.
		/// </remarks>
		/// <param name="type">One of the predefined SceneBlendType blending types.</param>
		public void SetSceneBlending(SceneBlendType type) {
			// convert canned blending types into blending factors
			switch(type) {
				case SceneBlendType.Add:
					SetSceneBlending(SceneBlendFactor.One, SceneBlendFactor.One);
					break;
				case SceneBlendType.TransparentAlpha:
					SetSceneBlending(SceneBlendFactor.SourceAlpha, SceneBlendFactor.OneMinusSourceAlpha);
					break;
				case SceneBlendType.TransparentColor:
					SetSceneBlending(SceneBlendFactor.SourceColor, SceneBlendFactor.OneMinusSourceColor);
					break;
                case SceneBlendType.Modulate:
                    SetSceneBlending(SceneBlendFactor.DestColor, SceneBlendFactor.Zero);
                    break;
                case SceneBlendType.Replace:
                    SetSceneBlending(SceneBlendFactor.One, SceneBlendFactor.Zero);
                    break;
			}
		}

		/// <summary>
		///    Allows very fine control of blending this Pass with the existing contents of the scene.
		/// </summary>
		/// <remarks>
		///    Wheras the texture blending operations seen in the TextureUnitState class are concerned with
		///    blending between texture layers, this blending is about combining the output of the material
		///    as a whole with the existing contents of the rendering target. This blending therefore allows
		///    object transparency and other special effects.
		///    <p/>
		///    This version of the method allows complete control over the blending operation, by specifying the
		///    source and destination blending factors. The result of the blending operation is:
		///    <span align="center">
		///    final = (texture * sourceFactor) + (pixel * destFactor)
		///    </span>
		///    <p/>
		///    Each of the factors is specified as one of a number of options, as specified in the SceneBlendFactor
		///    enumerated type.
		///    <p/>
		///    This method is applicable for both the fixed-function and programmable pipelines.
		/// </remarks>
		/// <param name="src">The source factor in the above calculation, i.e. multiplied by the texture color components.</param>
		/// <param name="dest">The destination factor in the above calculation, i.e. multiplied by the pixel color components.</param>
		public void SetSceneBlending(SceneBlendFactor src, SceneBlendFactor dest) {
			// copy settings
			sourceBlendFactor = src;
			destBlendFactor = dest;
		}

        public void SetAlphaRejectSettings(CompareFunction func, int value) {
            alphaRejectFunction = func;
            alphaRejectValue = value;
        }

		/// <summary>
		///		
		/// </summary>
		/// <param name="name"></param>
		public void SetFragmentProgram(string name) {
			SetFragmentProgram(name, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resetParams"></param>
		public void SetFragmentProgram(string name, bool resetParams) {
			// turn off fragment programs when the name is set to null
			if(name.Length == 0) {
				fragmentProgramUsage = null;
			}
			else {
				// create a new usage object
				if(!this.HasFragmentProgram) {
					fragmentProgramUsage = new GpuProgramUsage(GpuProgramType.Fragment);
				}

				fragmentProgramUsage.ProgramName = name;
			}

			// needs recompilation
			parent.NotifyNeedsRecompile();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resetParams"></param>
        public void SetShadowCasterFragmentProgram(string name)
        {
            // turn off fragment programs when the name is set to null
            if (name.Length == 0)
            {
                shadowCasterFragmentProgramUsage = null;
            }
            else
            {
                // create a new usage object
                if (!this.HasShadowCasterFragmentProgram)
                {
                    shadowCasterFragmentProgramUsage = new GpuProgramUsage(GpuProgramType.Fragment);
                }

                shadowCasterFragmentProgramUsage.ProgramName = name;
            }

            // needs recompilation
            parent.NotifyNeedsRecompile();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resetParams"></param>
		public void SetShadowReceiverFragmentProgram(string name) {
			// turn off fragment programs when the name is set to null
			if(name.Length == 0) {
				shadowReceiverFragmentProgramUsage = null;
			}
			else {
				// create a new usage object
				if(!this.HasShadowReceiverFragmentProgram) {
					shadowReceiverFragmentProgramUsage = new GpuProgramUsage(GpuProgramType.Fragment);
				}

				shadowReceiverFragmentProgramUsage.ProgramName = name;
			}

			// needs recompilation
			parent.NotifyNeedsRecompile();
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="name"></param>
		public void SetVertexProgram(string name) {
			SetVertexProgram(name, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resetParams"></param>
		public void SetVertexProgram(string name, bool resetParams) {
			// turn off vertex programs when the name is set to null
			if(name.Length == 0) {
				vertexProgramUsage = null;
			}
			else {
				// create a new usage object
				if(!this.HasVertexProgram) {
					vertexProgramUsage = new GpuProgramUsage(GpuProgramType.Vertex);
				}

				vertexProgramUsage.ProgramName = name;
			}

			// needs recompilation
			parent.NotifyNeedsRecompile();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resetParams"></param>
		public void SetShadowCasterVertexProgram(string name) {
			// turn off vertex programs when the name is set to null
			if(name.Length == 0) {
				shadowCasterVertexProgramUsage = null;
			}
			else {
				// create a new usage object
				if(!this.HasShadowCasterVertexProgram) {
					shadowCasterVertexProgramUsage = new GpuProgramUsage(GpuProgramType.Vertex);
				}

				shadowCasterVertexProgramUsage.ProgramName = name;
			}

			// needs recompilation
			parent.NotifyNeedsRecompile();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resetParams"></param>
		public void SetShadowReceiverVertexProgram(string name) {
			// turn off vertex programs when the name is set to null
			if(name.Length == 0) {
				shadowReceiverVertexProgramUsage = null;
			}
			else {
				// create a new usage object
				if(!this.HasShadowReceiverVertexProgram) {
					shadowReceiverVertexProgramUsage = new GpuProgramUsage(GpuProgramType.Vertex);
				}

				shadowReceiverVertexProgramUsage.ProgramName = name;
			}

			// needs recompilation
			parent.NotifyNeedsRecompile();
		}

		/// <summary>
		///    Splits this Pass to one which can be handled in the number of
		///    texture units specified.
		/// </summary>
		/// <param name="numUnits">
		///    The target number of texture units.
		/// </param>
		/// <returns>
		///    A new Pass which contains the remaining units, and a scene_blend
		///    setting appropriate to approximate the multitexture. This Pass will be 
		///    attached to the parent Technique of this Pass.
		/// </returns>
		public Pass Split(int numUnits) {
			// can't split programmable passes
			if(vertexProgramUsage != null || fragmentProgramUsage != null) {
				throw new Exception("Passes with vertex or fragment programs cannot be automatically split.  Define a fallback technique instead");
			}

			if(textureUnitStates.Count > numUnits) {
				int start = textureUnitStates.Count - numUnits;

				Pass newPass = parent.CreatePass();

				// get a reference ot the texture unit state at the split position
				TextureUnitState state = (TextureUnitState)textureUnitStates[start];

				// set the new pass to fallback using scene blending
				newPass.SetSceneBlending(state.ColorBlendFallbackSource, state.ColorBlendFallbackDest);

                // Fixup the texture unit 0   of new pass   blending method   to replace
                // all colour and alpha   with texture without adjustment, because we
                // assume it's detail texture.
                state.SetColorOperationEx(LayerBlendOperationEx.Source1, LayerBlendSource.Texture, LayerBlendSource.Current);
                state.SetAlphaOperation(LayerBlendOperationEx.Source1, LayerBlendSource.Texture, LayerBlendSource.Current);

				// add the rest of the texture units to the new pass
				for(int i = start; i < textureUnitStates.Count; i++) {
					state = (TextureUnitState)textureUnitStates[i];
					state.Parent = null;
                    newPass.AddTextureUnitState(state);
				}

                // Now remove texture units from this Pass, we don't need to delete since they've
                // been transferred
				textureUnitStates.RemoveRange(start, textureUnitStates.Count - start);
                DirtyHash();
				contentTypeLookupBuilt = false;
                return newPass;
			}

			return null;
		}

        public void SetPointAttenuation(bool enabled, float constant, float linear, float quadratic)
        {
            pointAttenuationEnabled = enabled;
            pointAttenuationConstant = constant;
            pointAttenuationLinear = linear;
            pointAttenuationQuadratic = quadratic;
        }

		/// <summary>
		///    Internal method for unloaded this pass.
		/// </summary>
		internal void Unload() {
			// load each texture unit state
			for(int i = 0; i < textureUnitStates.Count; i++) {
				((TextureUnitState)textureUnitStates[i]).Unload();
			}
		}

		/// <summary>
		///    Update any automatic light parameters on this pass.
		/// </summary>
		/// <param name="renderable">Current object being rendered.</param>
		/// <param name="camera">Current being being used for rendering.</param>
		public void UpdateAutoParamsLightsOnly(AutoParamDataSource source) {
			// auto update vertex program parameters
			if(this.HasVertexProgram) {
				vertexProgramUsage.Params.UpdateAutoParamsLightsOnly(source);
			}

			// auto update fragment program parameters
			if(this.HasFragmentProgram) {
				fragmentProgramUsage.Params.UpdateAutoParamsLightsOnly(source);
			}
		}

		/// <summary>
		///    Update any automatic parameters (except lights) on this pass.
		/// </summary>
		/// <param name="renderable">Current object being rendered.</param>
		/// <param name="camera">Current being being used for rendering.</param>
		public void UpdateAutoParamsNoLights(AutoParamDataSource source) {
			// auto update vertex program parameters
			if(this.HasVertexProgram) {
				vertexProgramUsage.Params.UpdateAutoParamsNoLights(source);
			}

			// auto update fragment program parameters
			if(this.HasFragmentProgram) {
				fragmentProgramUsage.Params.UpdateAutoParamsNoLights(source);
			}
		}

		/// <summary>
		///		Mark the hash for this pass as dirty.	
		/// </summary>
		public void DirtyHash() {
            lock (passLock)
            {
                dirtyHashList.Add(this);
            }
		}

		/// <summary>
		///		Queue this pass for deletion when appropriate.
		/// </summary>
		public void QueueForDeletion() {
			queuedForDeletion = true;

			RemoveAllTextureUnitStates();

            lock (passLock)
            {
                // remove from the dirty list
                dirtyHashList.Remove(this);

                graveyardList.Add(this);
            }
		}

		/// <summary>
		///		Process all dirty and pending deletion passes.
		/// </summary>
		public static void ProcessPendingUpdates() {
            lock (passLock)
            {
                // clear the graveyard
                graveyardList.Clear();

                // recalc the hashcode for each pass
                for (int i = 0; i < dirtyHashList.Count; i++)
                {
                    Pass pass = (Pass)dirtyHashList[i];
                    pass.RecalculateHash();
                }

                // clear out the dirty list
                dirtyHashList.Clear();
            }
		}
		
		/// <summary>
		///		Applies texture names to Texture Unit State with matching texture name aliases.
		///		All techniques, passes, and Texture Unit States within the material are checked.
		///		If matching texture aliases are found then true is returned.
		/// </summary>
		/// <param name="aliasList">A map container of texture alias, texture name pairs.</param>
		/// <param name="apply">Set to true to apply the texture aliases else just test to see if texture alias matches are found.</param>
		/// <returns>True if matching texture aliases were found in the material.</returns>
        public bool ApplyTextureAliases(Dictionary<string, string> aliasList, bool apply) {
            bool testResult = false;
            // iterate through passes and apply texture alias
			for(int i = 0; i < textureUnitStates.Count; i++) {
				TextureUnitState t = (TextureUnitState)textureUnitStates[i];
                if (t.ApplyTextureAliases(aliasList, apply))
                    testResult = true;
            }
            return testResult;
        }

		/// <summary>
		///		Overloading that defaults apply to true
		/// </summary>
        public bool ApplyTextureAliases(Dictionary<string, string> aliasList) {
            return ApplyTextureAliases(aliasList, true);
        }
        
		/// <summary>
		///		Gets the 'nth' texture which references the given content type.
		/// </summary>
		/// <remarks>
		///     If the 'nth' texture unit which references the content type doesn't
		///     exist, then this method returns an arbitrary high-value outside the
        ///     valid range to index texture units.
        /// </remarks>
        public int GetTextureUnitWithContentTypeIndex(TextureContentType contentType, int index) {
            if (!contentTypeLookupBuilt)
            {
                shadowContentTypeLookup = new List<int>();
                for (int i=0; i<textureUnitStates.Count; ++i) {
                    if (((TextureUnitState)textureUnitStates[i]).ContentType == TextureContentType.Shadow)
                        shadowContentTypeLookup.Add(i);
                }
                contentTypeLookupBuilt = true;
            }

            switch(contentType)
            {
            case TextureContentType.Shadow:
                if (index < shadowContentTypeLookup.Count)
                    return shadowContentTypeLookup[index];
                break;
            default:
                // Simple iteration
                for (int i=0; i<textureUnitStates.Count; ++i) {
                    if (((TextureUnitState)textureUnitStates[i]).ContentType == TextureContentType.Shadow) {
                        if (index == 0)
                            return i;
                        else
                            --index;
                    }
                }
                break;
            }

            // not found - return out of range
            return textureUnitStates.Count + 1;
        }

		public void SetDepthBias(float constantBias, float slopeScaleBias) {
            depthBiasConstant = constantBias;
            depthBiasSlopeScale = slopeScaleBias;
        }
        
        #endregion

		#region Object overrides

		/// <summary>
		///    Gets the 'hash' of this pass, ie a precomputed number to use for sorting.
		/// </summary>
		/// <remarks>
		///    This hash is used to sort passes, and for this reason the pass is hashed
		///    using firstly its index (so that all passes are rendered in order), then
		///    by the textures which it's TextureUnitState instances are using.
		/// </remarks>
		/// <returns></returns>
		public override int GetHashCode() {
			return hashCode;
		}

		#endregion Object overrides
		
		#region Properties

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

		/// <summary>
		///    Sets the ambient color reflectance properties of this pass.
		/// </summary>
		/// <remarks>
		///    The base color of a pass is determined by how much red, green and blue light is reflects
		///    (provided texture layer #0 has a blend mode other than LayerBlendOperation.Replace). 
		///    This property determines how much ambient light (directionless global light) is reflected. 
		///    The default is full white, meaning objects are completely globally illuminated. Reduce this 
		///    if you want to see diffuse or specular light effects, or change the blend of colors to make 
		///    the object have a base color other than white.
		///    <p/>
		///    This setting has no effect if dynamic lighting is disabled (see <see cref="Pass.LightingEnabled"/>),
		///    or if this is a programmable pass.
		/// </remarks>
		public ColorEx Ambient {
			get {
				return ambient;
			}
			set {
				ambient = value;
			}
		}

		/// <summary>
		///    Sets whether or not color buffer writing is enabled for this Pass.
		/// </summary>
		/// <remarks>
		///    For some effects, you might wish to turn off the color write operation
		///    when rendering geometry; this means that only the depth buffer will be
		///    updated (provided you have depth buffer writing enabled, which you 
		///    probably will do, although you may wish to only update the stencil
		///    buffer for example - stencil buffer state is managed at the RenderSystem
		///    level only, not the Material since you are likely to want to manage it 
		///    at a higher level).
		/// </remarks>
		public bool ColorWrite {
			get {
				return colorWrite;
			}
			set {
				colorWrite = value;
			}
		}

		/// <summary>
		///    Alpha reject function
		/// </summary>
		public CompareFunction AlphaRejectFunction {
			get {
				return alphaRejectFunction;
			}
			set {
				alphaRejectFunction = value;
			}
		}

		/// <summary>
		///    Alpha reject value
		/// </summary>
		public int AlphaRejectValue {
			get {
				return alphaRejectValue;
			}
			set {
				alphaRejectValue = value;
			}
		}

		/// <summary>
		///    Sets the culling mode for this pass based on the 'vertex winding'.
		/// </summary>
		/// <remarks>
		///    A typical way for the rendering engine to cull triangles is based on the 'vertex winding' of
		///    triangles. Vertex winding refers to the direction in which the vertices are passed or indexed
		///    to in the rendering operation as viewed from the camera, and will wither be clockwise or
		///    counterclockwise. The default is Clockwise i.e. that only triangles whose vertices are passed/indexed in 
		///    counter-clockwise order are rendered - this is a common approach and is used in 3D studio models for example. 
		///    You can alter this culling mode if you wish but it is not advised unless you know what you are doing.
		///    <p/>
		///    You may wish to use the CullingMode.None option for mesh data that you cull yourself where the vertex
		///    winding is uncertain.
		/// </remarks>
		public CullingMode CullMode {
			get {
				return cullMode;
			}
			set {
				cullMode = value;
			}
		}
        
		/// <summary>
		///    Provide a compatibility interface for setting depth bias
		/// </summary>
		public int DepthBias {
			get {
				return (int)depthBiasConstant;
			}
			set {
				Debug.Assert(value <= 16, "Depth bias must be between 0 and 16.");
				depthBiasConstant = value;
			}
		}

		/// <summary>
		///    Sets the depth bias to be used for this Pass.
		/// </summary>
		/// <remarks>
		///    When polygons are coplanar, you can get problems with 'depth fighting' (or 'z fighting') where
		///    the pixels from the two polys compete for the same screen pixel. This is particularly
		///    a problem for decals (polys attached to another surface to represent details such as
		///    bulletholes etc.).
		///    <p/>
		///    A way to combat this problem is to use a depth bias to adjust the depth buffer value
		///    used for the decal such that it is slightly higher than the true value, ensuring that
        ///    the decal appears on top. There are two aspects to the biasing, a constant
		///    bias value and a slope-relative biasing value, which varies according to the
		///    maximum depth slope relative to the camera, ie:
		///      finalBias = maxSlope * slopeScaleBias + constantBias
		///    Note that slope scale bias, whilst more accurate, may be ignored by old hardware.
		/// </remarks>
		public float DepthBiasConstant {
			get {
				return depthBiasConstant;
			}
			set {
				depthBiasConstant = value;
			}
		}

		public float DepthBiasSlopeScale {
			get {
				return depthBiasSlopeScale;
			}
			set {
				depthBiasSlopeScale = value;
			}
		}

		/// <summary>
		///    Gets/Sets whether or not this pass renders with depth-buffer checking on or not.
		/// </summary>
		/// <remarks>
		///    If depth-buffer checking is on, whenever a pixel is about to be written to the frame buffer
		///    the depth buffer is checked to see if the pixel is in front of all other pixels written at that
		///    point. If not, the pixel is not written.
		///    <p/>
		///    If depth checking is off, pixels are written no matter what has been rendered before.
		///    Also see <see cref="DepthFunction"/> for more advanced depth check configuration.
		/// </remarks>
		public bool DepthCheck {
			get {
				return depthCheck;
			}
			set {
				depthCheck = value;
			}
		}

		/// <summary>
		///    Gets/Sets the function used to compare depth values when depth checking is on.
		/// </summary>
		/// <remarks>
		///    If depth checking is enabled (see <see cref="DepthCheck"/>) a comparison occurs between the depth
		///    value of the pixel to be written and the current contents of the buffer. This comparison is
		///    normally CompareFunction.LessEqual, i.e. the pixel is written if it is closer (or at the same distance)
		///    than the current contents. If you wish, you can change this comparison using this method.
		/// </remarks>
		public CompareFunction DepthFunction {
			get {
				return depthFunc;
			}
			set {
				depthFunc = value;
			}
		}
        
		/// <summary>
		///    Gets/Sets whether or not this pass renders with depth-buffer writing on or not.
		/// </summary>
		/// <remarks>
		///    If depth-buffer writing is on, whenever a pixel is written to the frame buffer
		///    the depth buffer is updated with the depth value of that new pixel, thus affecting future
		///    rendering operations if future pixels are behind this one.
		///    <p/>
		///    If depth writing is off, pixels are written without updating the depth buffer. Depth writing should
		///    normally be on but can be turned off when rendering static backgrounds or when rendering a collection
		///    of transparent objects at the end of a scene so that they overlap each other correctly.
		/// </remarks>
		public bool DepthWrite {
			get {
				return depthWrite;
			}
			set {
				depthWrite = value;
			}
		}

		/// <summary>
		///    Retrieves the destination blending factor for the material (as set using SetSceneBlending).
		/// </summary>
        public SceneBlendFactor DestBlendFactor
        {
            get
            {
                return destBlendFactor;
            }
            set
            {
                destBlendFactor = value;
            }
        }

		/// <summary>
		///    Sets the diffuse color reflectance properties of this pass.
		/// </summary>
		/// <remarks>
		///    The base color of a pass is determined by how much red, green and blue light is reflects
		///    (provided texture layer #0 has a blend mode other than LayerBlendOperation.Replace). This property determines how
		///    much diffuse light (light from instances of the Light class in the scene) is reflected. The default
		///    is full white, meaning objects reflect the maximum white light they can from Light objects.
		///    <p/>
		///    This setting has no effect if dynamic lighting is disabled (see <see cref="Pass.LightingEnabled"/>),
		///    or if this is a programmable pass.
		/// </remarks>
		public ColorEx Diffuse {
			get {
				return diffuse;
			}
			set {
				diffuse = value;
			}
		}
        
		/// <summary>
		/// 
		/// </summary>
		public ColorEx Emissive {
			get {
				return emissive;
			}
			set {
				emissive = value;
			}
		}
        
		/// <summary>
		///    Returns the fog color for the scene.
		/// </summary>
		/// <remarks>
		///    Only valid if FogOverride is true.
		/// </remarks>
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
        
		/// <summary>
		///    Returns the fog density for this pass.
		/// </summary>
		/// <remarks>
		///    Only valid if FogOverride is true.
		/// </remarks>
        public float FogDensity
        {
            get
            {
                return fogDensity;
            }
            set
            {
                fogDensity = value;
            }
        }

		/// <summary>
		///    Returns the fog end distance for this pass.
		/// </summary>
		/// <remarks>
		///    Only valid if FogOverride is true.
		/// </remarks>
        public float FogEnd
        {
            get
            {
                return fogEnd;
            }
            set
            {
                fogEnd = value;
            }
        }
        
		/// <summary>
		///    Returns the fog mode for this pass.
		/// </summary>
		/// <remarks>
		///    Only valid if FogOverride is true.
		/// </remarks>
        public FogMode FogMode
        {
            get
            {
                return fogMode;
            }
            set
            {
                fogMode = value;
            }
        }

		/// <summary>
		///    Returns true if this pass is to override the scene fog settings.
		/// </summary>
		public bool FogOverride {
			get {
				return fogOverride;
			}
            set
            {
                fogOverride = value;
            }
		}

		/// <summary>
		///    Returns the fog start distance for this pass.
		/// </summary>
		/// <remarks>
		///    Only valid if FogOverride is true.
		/// </remarks>
        public float FogStart
        {
            get
            {
                return fogStart;
            }
            set
            {
                fogStart = value;
            }
        }

		/// <summary>
		///    Gets the vertex program used by this pass.
		/// </summary>
		/// <remarks>
		///    Only available after Load() has been called.
		/// </remarks>
		public GpuProgram FragmentProgram {
			get {
				Debug.Assert(this.HasFragmentProgram, "This pass does not contain a fragment program!");
				return fragmentProgramUsage.Program;
			}
		}

		/// <summary>
		///    Gets/Sets the name of the fragment program to use.
		/// </summary>
		/// <remarks>
		///    Only applicable to programmable passes, and this particular call is
		///    designed for low-level programs; use the named parameter methods
		///    for setting high-level programs.
		///    <p/>
		///    This must have been created using GpuProgramManager by the time that 
		///    this Pass is loaded.
		/// </remarks>
		public string FragmentProgramName {
			get {
				// return blank if there is no fragment program in this pass
				if(this.HasFragmentProgram) {
					return fragmentProgramUsage.ProgramName;
				}
				else {
					return String.Empty;
				}
			}
		}

		/// <summary>
		///    Gets/Sets the fragment program parameters used by this pass.
		/// </summary>
		/// <remarks>
		///    Only applicable to programmable passes, and this particular call is
		///    designed for low-level programs; use the named parameter methods
		///    for setting high-level program parameters.
		/// </remarks>
		public GpuProgramParameters FragmentProgramParameters {
			get {
				Debug.Assert(this.HasFragmentProgram, "This pass does not contain a fragment program!");
				return fragmentProgramUsage.Params;
			}
			set {
				Debug.Assert(this.HasFragmentProgram, "This pass does not contain a fragment program!");
				fragmentProgramUsage.Params = value;
			}
		}

        public string ShadowCasterFragmentProgramName
        {
            get
            {
                if (this.HasShadowCasterFragmentProgram)
                {
                    return shadowCasterFragmentProgramUsage.ProgramName;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

		public string ShadowReceiverFragmentProgramName {
			get {
				if(this.HasShadowReceiverFragmentProgram) {
					return shadowReceiverFragmentProgramUsage.ProgramName;
				}
				else {
					return String.Empty;
				}
			}
		}

        public GpuProgramParameters ShadowCasterFragmentProgramParameters
        {
            get
            {
                Debug.Assert(this.HasShadowCasterFragmentProgram, "This pass does not contain a shadow caster fragment program!");
                return shadowCasterFragmentProgramUsage.Params;
            }
            set
            {
                Debug.Assert(this.HasShadowCasterFragmentProgram, "This pass does not contain a shadow caster fragment program!");
                shadowCasterFragmentProgramUsage.Params = value;
            }
        }

		public GpuProgramParameters ShadowReceiverFragmentProgramParameters {
			get {
				Debug.Assert(this.HasShadowReceiverFragmentProgram, "This pass does not contain a shadow receiver fragment program!");
				return shadowReceiverFragmentProgramUsage.Params;
			}
			set {
				Debug.Assert(this.HasShadowReceiverFragmentProgram, "This pass does not contain a shadow receiver fragment program!");
				shadowReceiverFragmentProgramUsage.Params = value;
			}
		}

		/// <summary>
		///    Returns true if this Pass uses the programmable fragment pipeline.
		/// </summary>
		public bool HasFragmentProgram {
			get {
				return fragmentProgramUsage != null;
			}
		}

		/// <summary>
		///    Returns true if this Pass uses the programmable vertex pipeline.
		/// </summary>
		public bool HasVertexProgram {
			get {
				return vertexProgramUsage != null;
			}
		}

		/// <summary>
		///    Returns true if this Pass uses the programmable shadow caster vertex pipeline.
		/// </summary>
		public bool HasShadowCasterVertexProgram {
			get {
				return shadowCasterVertexProgramUsage != null;
			}
		}

        /// <summary>
        ///    Returns true if this Pass uses the programmable shadow caster fragment pipeline.
        /// </summary>
        public bool HasShadowCasterFragmentProgram
        {
            get
            {
                return shadowCasterFragmentProgramUsage != null;
            }
        }

		/// <summary>
		///    Returns true if this Pass uses the programmable shadow receiver vertex pipeline.
		/// </summary>
		public bool HasShadowReceiverVertexProgram {
			get {
				return shadowReceiverVertexProgramUsage != null;
			}
		}

		/// <summary>
		///    Returns true if this Pass uses the programmable shadow receiver fragment pipeline.
		/// </summary>
		public bool HasShadowReceiverFragmentProgram {
			get {
				return shadowReceiverFragmentProgramUsage != null;
			}
		}

		/// <summary>
		///    Gets the index of this Pass in the parent Technique.
		/// </summary>
		public int Index {
			get {
				return index;
			}
            set {
                index = value;
            }
		}

		/// <summary>
		///		Gets a flag indicating whether this pass is ambient only.
		/// </summary>
		public bool IsAmbientOnly {
			get {
				// treat as ambient if lighting is off, or color write is off, 
				// or all non-ambient (& emissive) colors are black
				// NB a vertex program could override this, but passes using vertex
				// programs are expected to indicate they are ambient only by 
				// setting the state so it matches one of the conditions above, even 
				// though this state is not used in rendering.
				return (!lightingEnabled || !colorWrite ||
					(diffuse == ColorEx.Black && specular == ColorEx.Black));
			}
		}

		/// <summary>
		///    Returns true if this pass is loaded.
		/// </summary>
		public bool IsLoaded {
			get {
				return parent.IsLoaded;
			}
		}

		/// <summary>
		///    Returns true if this pass is programmable ie includes either a vertex or fragment program.
		/// </summary>
		public bool IsProgrammable {
			get {
				return vertexProgramUsage != null || fragmentProgramUsage != null;
			}
		}

		/// <summary>
		///    Returns true if this pass has some element of transparency.
		/// </summary>
		public bool IsTransparent {
			get {
				// Transparent if any of the destination color is taken into account
				if (destBlendFactor == SceneBlendFactor.Zero &&
                    sourceBlendFactor != SceneBlendFactor.DestColor &&
                    sourceBlendFactor != SceneBlendFactor.OneMinusDestColor &&
                    sourceBlendFactor != SceneBlendFactor.DestAlpha &&
                    sourceBlendFactor != SceneBlendFactor.OneMinusDestAlpha)
                    return false;
                else
                    return true;
			}
		}

		/// <summary>
		///    Sets whether or not dynamic lighting is enabled.
		/// </summary>
		/// <remarks>
		///    If true, dynamic lighting is performed on geometry with normals supplied, geometry without
		///    normals will not be displayed.
		///    If false, no lighting is applied and all geometry will be full brightness.
		/// </remarks>
		public bool LightingEnabled {
			get {
				return lightingEnabled;
			}
			set {
				lightingEnabled = value;
			}
		}

		/// <summary>
		///    Sets the manual culling mode, performed by CPU rather than hardware.
		/// </summary>
		/// <remarks>
		///    In some situations you want to use manual culling of triangles rather than sending the
		///    triangles to the hardware and letting it cull them. This setting only takes effect on SceneManager's
		///    that use it (since it is best used on large groups of planar world geometry rather than on movable
		///    geometry since this would be expensive), but if used can cull geometry before it is sent to the
		///    hardware.
		/// </remarks>
		/// <value>
		///    The default for this setting is ManualCullingMode.Back.
		/// </value>
		public ManualCullingMode ManualCullMode {
			get {
				return manualCullMode;
			}
			set {
				manualCullMode = value;
			}
		}

		/// <summary>
		///    Sets the maximum number of lights to be used by this pass. 
		/// </summary>
		/// <remarks>
		///    During rendering, if lighting is enabled (or if the pass uses an automatic
		///    program parameter based on a light) the engine will request the nearest lights 
		///    to the object being rendered in order to work out which ones to use. This
		///    parameter sets the limit on the number of lights which should apply to objects 
		///    rendered with this pass. 
		/// </remarks>
		public int MaxLights {
			get {
				return maxLights;
			}
			set {
				maxLights = value;
			}
		}

		/// <summary>
		///    Sets the light index that this pass will start at in the light list.
		/// </summary>
		/// <remarks>
		///    Normally the lights passed to a pass will start from the beginning
		///    of the light list for this object. This option allows you to make this
		///    pass start from a higher light index, for example if one of your earlier
		///    passes could deal with lights 0-3, and this pass dealt with lights 4+. 
		///    This option also has an interaction with pass iteration, in that
		///    if you choose to iterate this pass per light too, the iteration will
		///    only begin from light 4.
		/// </remarks>
		public int StartLight {
			get {
				return startLight;
			}
			set {
				startLight = value;
			}
		}

		/// <summary>
		///    Gets the number of fixed function texture unit states for this Pass.
		/// </summary>
		public int NumTextureUnitStages {
			get {
				return textureUnitStates.Count;
			}
		}

		/// <summary>
		///     Gets the single light type this pass runs for if RunOncePerLight and 
		///     RunOnlyForOneLightType are both true. 
		/// </summary>
		public LightType OnlyLightType {
			get {
				return onlyLightType;
			}
		}

		/// <summary>
		///    Gets a reference to the Technique that owns this pass.
		/// </summary>
		public Technique Parent {
			get {
				return parent;
			}
		}

		/// <summary>
		///    Does this pass run once for every light in range?
		/// </summary>
		public bool RunOncePerLight {
			get {
				return runOncePerLight;
			}
		}

		/// <summary>
		///    Does this pass run only for a single light type (if RunOncePerLight is true). 
		/// </summary>
		public bool RunOnlyForOneLightType {
			get {
				return runOnlyForOneLightType;
			}
		}
        
		/// <summary>
		///    Iterate per how many lights?
		/// </summary>
		public int LightsPerIteration {
			get {
				return lightsPerIteration;
			}
			set {
				lightsPerIteration = value;
			}
		}

		/// <summary>
		///    Sets the type of light shading required.
		/// </summary>
		/// <value>
		///    The default shading method is Gouraud shading.
		/// </value>
		public Shading ShadingMode {
			get {
				return shadeOptions;
			}
			set {
				shadeOptions = value;
			}
		}
        
		/// <summary>
		///    Sets the shininess of the pass, affecting the size of specular highlights.
		/// </summary>
		/// <remarks>
		///    This setting has no effect if dynamic lighting is disabled (see Pass::setLightingEnabled),
		///    or if this is a programmable pass.
		/// </remarks>
		public float Shininess {
			get {
				return shininess;
			}
			set {
				shininess = value;
			}
		}

        public TrackVertexColor VertexColorTracking {
            get {
                return tracking;
            }
            set {
                tracking = value;
            }
        }

		/// <summary>
		///    Retrieves the source blending factor for the material (as set using SetSceneBlending).
		/// </summary>
        public SceneBlendFactor SourceBlendFactor
        {
            get
            {
                return sourceBlendFactor;
            }
            set
            {
                sourceBlendFactor = value;
            }
        }

		/// <summary>
		///    Sets the specular color reflectance properties of this pass.
		/// </summary>
		/// <remarks>
		///    The base color of a pass is determined by how much red, green and blue light is reflects
		///    (provided texture layer #0 has a blend mode other than LBO_REPLACE). This property determines how
		///    much specular light (highlights from instances of the Light class in the scene) is reflected.
		///    The default is to reflect no specular light.
		///    <p/>
		///    The size of the specular highlights is determined by the separate Shininess property.
		///    <p/>
		///    This setting has no effect if dynamic lighting is disabled (see <see cref="Pass.LightingEnabled"/>),
		///    or if this is a programmable pass.
		/// </remarks>
		public ColorEx Specular {
			get {
				return specular;
			}
			set {
				specular = value;
			}
		}

		/// <summary>
		///    Sets the anisotropy level to be used for all textures.
		/// </summary>
		/// <remarks>
		///    This property has been moved to the TextureUnitState class, which is accessible via the 
		///    Technique and Pass. For simplicity, this method allows you to set these properties for 
		///    every current TeextureUnitState, If you need more precision, retrieve the Technique, 
		///    Pass and TextureUnitState instances and set the property there.
		/// </remarks>
		public int TextureAnisotropy {
			set {
				for(int i = 0; i < textureUnitStates.Count; i++) {
					((TextureUnitState)textureUnitStates[i]).TextureAnisotropy = value;
				}
			}
		}

		/// <summary>
		///    Set texture filtering for every texture unit.
		/// </summary>
		/// <remarks>
		///    This property actually exists on the TextureUnitState class
		///    For simplicity, this method allows you to set these properties for 
		///    every current TeextureUnitState, If you need more precision, retrieve the  
		///    TextureUnitState instance and set the property there.
		/// </remarks>
		public TextureFiltering TextureFiltering {
			set {
				for(int i = 0; i < textureUnitStates.Count; i++) {
					((TextureUnitState)textureUnitStates[i]).SetTextureFiltering(value);
				}
			}
		}

		/// <summary>
		///    Gets the vertex program used by this pass.
		/// </summary>
		/// <remarks>
		///    Only available after Load() has been called.
		/// </remarks>
		public GpuProgram VertexProgram {
			get {
				Debug.Assert(this.HasVertexProgram, "This pass does not contain a vertex program!");
				return vertexProgramUsage.Program;
			}
		}

		/// <summary>
		///    Gets/Sets the name of the vertex program to use.
		/// </summary>
		/// <remarks>
		///    Only applicable to programmable passes, and this particular call is
		///    designed for low-level programs; use the named parameter methods
		///    for setting high-level programs.
		///    <p/>
		///    This must have been created using GpuProgramManager by the time that 
		///    this Pass is loaded.
		/// </remarks>
		public string VertexProgramName {
			get {
				if(this.HasVertexProgram) {
					return vertexProgramUsage.ProgramName;
				}
				else {
					return String.Empty;
				}
			}
		}

		/// <summary>
		///    Gets/Sets the vertex program parameters used by this pass.
		/// </summary>
		/// <remarks>
		///    Only applicable to programmable passes, and this particular call is
		///    designed for low-level programs; use the named parameter methods
		///    for setting high-level program parameters.
		/// </remarks>
		public GpuProgramParameters VertexProgramParameters {
			get {
				Debug.Assert(this.HasVertexProgram, "This pass does not contain a vertex program!");
				return vertexProgramUsage.Params;
			}
			set {
				Debug.Assert(this.HasVertexProgram, "This pass does not contain a vertex program!");
				vertexProgramUsage.Params = value;
			}
		}

		public string ShadowCasterVertexProgramName {
			get {
				if(this.HasShadowCasterVertexProgram) {
					return shadowCasterVertexProgramUsage.ProgramName;
				}
				else {
					return String.Empty;
				}
			}
		}

		public GpuProgramParameters ShadowCasterVertexProgramParameters {
			get {
				Debug.Assert(this.HasShadowCasterVertexProgram, "This pass does not contain a shadow caster vertex program!");
				return shadowCasterVertexProgramUsage.Params;
			}
			set {
				Debug.Assert(this.HasShadowCasterVertexProgram, "This pass does not contain a shadow caster vertex program!");
				shadowCasterVertexProgramUsage.Params = value;
			}
		}

		public string ShadowReceiverVertexProgramName {
			get {
				if(this.HasShadowReceiverVertexProgram) {
					return shadowReceiverVertexProgramUsage.ProgramName;
				}
				else {
					return String.Empty;
				}
			}
		}

		public int PassIterationCount {
			get {
				return passIterationCount;
			}
			set {
				passIterationCount = value;
			}
		}

		public SceneDetailLevel SceneDetail {
			get {
				return sceneDetail;
			}
			set {
				sceneDetail = value;
			}
		}

		public GpuProgramParameters ShadowReceiverVertexProgramParameters {
			get {
				Debug.Assert(this.HasShadowReceiverVertexProgram, "This pass does not contain a shadow receiver vertex program!");
				return shadowReceiverVertexProgramUsage.Params;
			}
			set {
				Debug.Assert(this.HasShadowReceiverVertexProgram, "This pass does not contain a shadow receiver vertex program!");
				shadowReceiverVertexProgramUsage.Params = value;
			}
		}

		public float PointSize {
            get {
                return pointSize;
            }
            set {
                pointSize = value;
            }
        }
        
		public float PointMinSize {
            get {
                return pointMinSize;
            }
            set {
                pointMinSize = value;
            }
        }
        
		public float PointMaxSize {
            get {
                return pointMaxSize;
            }
            set {
                pointMaxSize = value;
            }
        }
        
		public bool PointSpritesEnabled {
            get {
                return pointSpritesEnabled;
            }
            set {
                pointSpritesEnabled = value;
            }
        }
        
		public bool PointAttenuationEnabled {
            get {
                return pointAttenuationEnabled;
            }
            set { 
               pointAttenuationEnabled = value;
            }
        }
        
		public float PointAttenuationConstant {
            get {
                return pointAttenuationConstant;
            }
            set {
                pointAttenuationConstant = value;
            }
        }
        
        public float PointAttenuationLinear {
            get {
                return pointAttenuationLinear;
            }
            set {
                pointAttenuationLinear = value;
            }
        }
        
        public float PointAttenuationQuadratic {
            get {
                return pointAttenuationQuadratic;
            }
            set {
                pointAttenuationQuadratic = value;
            }
        }
        

		/// <summary>
		///		Gets a list of dirty passes.
		/// </summary>
		internal static PassList DirtyList {
			get {
				return dirtyHashList;
			}
		}
        
		/// <summary>
		///		Gets a list of passes queued for deletion.
		/// </summary>
		internal static PassList GraveyardList {
			get {
				return graveyardList;
			}
		}

        /// <summary>
        /// Lock the graveyard and dirty hash lists for outside of thread use.
        /// </summary>
        internal static object PassLock
        {
            get
            {
                return passLock;
            }
        }
		#endregion
	}

	/// <summary>
	///		Struct recording a pass which can be used for a specific illumination stage.
	/// </summary>
	/// <remarks>
	///		This structure is used to record categorized passes which fit into a 
	///		number of distinct illumination phases - ambient, diffuse / specular 
	///		(per-light) and decal (post-lighting texturing).
	///		An original pass may fit into one of these categories already, or it
	///		may require splitting into its component parts in order to be categorized 
	///		properly.
	/// </remarks>
	public struct IlluminationPass {
		/// <summary>
		///		The stage at which this pass is relevant.
		/// </summary>
		public IlluminationStage Stage;
		/// <summary>
		///		The pass to use in this stage.
		/// </summary>
		public Pass Pass;
		/// <summary>
		///		Whether this pass is one which should be deleted itself.
		/// </summary>
		public bool DestroyOnShutdown;
		/// <summary>
		///		The original pass which spawned this one.
		/// </summary>
		public Pass OriginalPass;
	}

}
