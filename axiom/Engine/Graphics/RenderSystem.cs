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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Axiom.Core;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.MathLib;
using Axiom.Media;

namespace Axiom.Graphics {

    public class ConfigOption {
        public string name;
        public string currentValue;
        public List<string> possibleValues = new List<string>();
    }

	/// <summary>
	///    Defines the functionality of a 3D API
	/// </summary>
	///	<remarks>
	///		The RenderSystem class provides a base class
	///		which abstracts the general functionality of the 3D API
	///		e.g. Direct3D or OpenGL. Whilst a few of the general
	///		methods have implementations, most of this class is
	///		abstract, requiring a subclass based on a specific API
	///		to be constructed to provide the full functionality.
	///		<p/>
	///		Note there are 2 levels to the interface - one which
	///		will be used often by the caller of the engine library,
	///		and one which is at a lower level and will be used by the
	///		other classes provided by the engine. These lower level
	///		methods are marked as internal, and are not accessible outside
	///		of the Core library.
	///	</remarks>
	public abstract class RenderSystem : IDisposable {
		#region Constants

		/// <summary>
		///		Default window title if one is not specified upon a call to <see cref="Initialize"/>.
		/// </summary>
		const string DefaultWindowTitle = "Axiom Window";

		#endregion Constants

		#region Fields

		/// <summary>
		///		List of current render targets (i.e. a <see cref="RenderWindow"/>, or a<see cref="RenderTexture"/>)
		/// </summary>
        protected List<RenderTarget> renderTargets = new List<RenderTarget>();
		/// <summary>
		///		A reference to the texture management class specific to this implementation.
		/// </summary>
		protected TextureManager textureManager;
		/// <summary>
		///		A reference to the hardware vertex/index buffer manager specific to this API.
		/// </summary>
		protected HardwareBufferManager hardwareBufferManager;
		/// <summary>
		///		Current hardware culling mode.
		/// </summary>
		protected CullingMode cullingMode;
		/// <summary>
		///		Are we syncing frames with the refresh rate of the screen?
		/// </summary>
		protected bool isVSync;
		/// <summary>
		///		Current depth write setting.
		/// </summary>
		protected bool depthWrite;
		/// <summary>
		///		Number of current active lights.
		/// </summary>
		protected int numCurrentLights;
		/// <summary>
		///		Reference to the config options for the graphics engine.
		/// </summary>
		protected DisplayConfig displayConfig = new DisplayConfig();
		/// <summary>
		///		Active viewport (dest for future rendering operations) and target.
		/// </summary>
		protected Viewport activeViewport;
		/// <summary>
		///		Active render target.
		/// </summary>
		protected RenderTarget activeRenderTarget;
		/// <summary>
		///		Number of faces currently rendered this frame.
		/// </summary>
		protected int numFaces;
		/// <summary>
		///		Number of faces currently rendered this frame.
		/// </summary>
		protected int numVertices;
		/// <summary>
		///		Capabilites of the current hardware (populated at startup).
		/// </summary>
		protected HardwareCaps caps = new HardwareCaps();
		/// <summary>
		///		Saved set of world matrices.
		/// </summary>
		protected Matrix4[] worldMatrices = new Matrix4[256];
		/// <summary>
		///     Flag for whether vertex winding needs to be inverted, useful for reflections.
		/// </summary>
		protected bool invertVertexWinding;

        protected bool vertexProgramBound = false;
        protected bool fragmentProgramBound = false;

		protected static long totalRenderCalls = 0;

        protected bool allowResize = false;
        
        #endregion Fields

		#region Constructor

		/// <summary>
		///		Base constructor.
		/// </summary>
		public RenderSystem() {		
			// default to true
			isVSync = true;

			// default to true
			depthWrite = true;

			// This means CULL clockwise vertices, i.e. front of poly is counter-clockwise
			// This makes it the same as OpenGL and other right-handed systems
			cullingMode = Axiom.Graphics.CullingMode.Clockwise; 
		}

		#endregion

		#region Virtual Members

		#region Properties

        /// <summary>
        ///		Gets the currently-active viewport
        /// </summary>
        public Viewport ActiveViewport {
            get {
                return activeViewport;
            }
        }

		/// <summary>
		///		Gets a set of hardware capabilities queryed by the current render system.
		/// </summary>
		public virtual HardwareCaps Caps {
			get {
				return caps;
			}
		}

		/// <summary>
		/// Gets a dataset with the options set for the rendering system.
		/// </summary>
		public virtual DisplayConfig ConfigOptions {
			get {
				return displayConfig;
			}
		}

		/// <summary>
		///		Number of faces rendered during the current frame so far.
		/// </summary>
		public int FacesRendered {
			get {
				return numFaces;
			}
		}

		/// <summary>
		///     Sets whether or not vertex windings set should be inverted; this can be important
		///     for rendering reflections.
		/// </summary>
		public virtual bool InvertVertexWinding {
			get {
				return invertVertexWinding;
			}
			set {
				invertVertexWinding = value;
			}
		}

		/// <summary>
		/// Gets/Sets a value that determines whether or not to wait for the screen to finish refreshing
		/// before drawing the next frame.
		/// </summary>
		public virtual bool IsVSync {
			get {
				return isVSync;
			}
			set {
				isVSync = value;
			}
		}

		/// <summary>
		///    Set by the client to indicate that the maximize button
		///    and resize border should be added to the window
		/// </summary>
		public virtual bool AllowResize {
			get {
				return allowResize;
			}
			set {
				allowResize = value;
			}
		}

		/// <summary>
		/// Gets the name of this RenderSystem based on it's assembly attribute Title.
		/// </summary>
		public virtual string Name {
			get {
				AssemblyTitleAttribute attribute =
					(AssemblyTitleAttribute) Attribute.GetCustomAttribute(this.GetType().Assembly, typeof(AssemblyTitleAttribute), false);

				if (attribute != null)
					return attribute.Title;
				else
					return "Not Found";
			}
		}

        public int RenderTargetCount {
            get {
                return renderTargets.Count;
            }
        }

        public static long TotalRenderCalls {
            get {
                return totalRenderCalls;
            }
        }

		#endregion Properties

		#region Methods

		/// <summary>
		///    Attaches a render target to this render system.
		/// </summary>
		/// <param name="target">Reference to the render target to attach to this render system.</param>
		public virtual void AttachRenderTarget(RenderTarget target) {
			if (target.Priority == RenderTargetPriority.High) {
				// insert at the front of the list
				renderTargets.Insert(0, target);
			} else {
				// add to the end
				renderTargets.Add(target);
			}
            // FIXME: Investigate
            // Ogre maintains a renderTargets and a prioritizedRenderTargets list.
		}

		/// <summary>
		///		The RenderSystem will keep a count of tris rendered, this resets the count.
		/// </summary>
		public virtual void BeginGeometryCount() {
			numFaces = 0;
		}

		/// <summary>
		///		Detaches the render target with the specified name from this render system.
		/// </summary>
		/// <param name="name">Name of the render target to detach.</param>
        /// <returns>the render target that was detached</returns>
		public RenderTarget DetachRenderTarget(string name) {
			RenderTarget target = null;

			for(int i = 0; i < renderTargets.Count; i++) {
				RenderTarget tmp = renderTargets[i];

				if(tmp.Name == name) {
					target = tmp;
					break;
				}
			}

			if(target != null) {
				DetachRenderTarget(target);
			}

            /// If detached render target is the active render target, 
            /// reset active render target
            if (target == activeRenderTarget)
                activeRenderTarget = null;

            return target;
		}

		/// <summary>
		///		Detaches the render target from this render system.
		/// </summary>
		/// <param name="target">Reference to the render target to detach.</param>
        /// <returns>the render target that was detached</returns>
        public virtual RenderTarget DetachRenderTarget(RenderTarget target) {
			// TODO: Remove prioritized render targets?
			renderTargets.Remove(target);
            return target;
		}

		/// <summary>
		///		Turns off a texture unit if not needed.
		/// </summary>
		/// <param name="stage"></param>
		public virtual void DisableTextureUnit(int stage) {
			SetTexture(stage, false, "");
			SetTextureMatrix(stage, Matrix4.Identity);
		}

		/// <summary>
		///		Disables all texture units from the given unit upwards */
		/// </summary>
        public virtual void DisableTextureUnitsFrom(int texUnit)
        {
            for (int i = texUnit; i < caps.TextureUnitCount; ++i)
            {
                DisableTextureUnit(i);
            }
        }

        /// <summary>
        ///     Utility method for initializing all render targets attached to this rendering system.
        /// </summary>
        public virtual void InitRenderTargets() {
            // init stats for each render target
            foreach (RenderTarget target in renderTargets) {
                target.ResetStatistics();
            }
        }

        /// <summary>
		///		Utility method to notify all render targets that a camera has been removed, 
		///		incase they were referring to it as their viewer. 
		/// </summary>
		/// <param name="camera">Camera being removed.</param>
		internal virtual void NotifyCameraRemoved(Camera camera) {
			for (int i = 0; i < renderTargets.Count; i++) {
				RenderTarget target = renderTargets[i];
				target.NotifyCameraRemoved(camera);
			}
		}

		/// <summary>
		///		Render something to the active viewport.
		/// </summary>
		/// <remarks>
		///		Low-level rendering interface to perform rendering
		///		operations. Unlikely to be used directly by client
		///		applications, since the <see cref="SceneManager"/> and various support
		///		classes will be responsible for calling this method.
		///		Can only be called between <see cref="BeginScene"/> and <see cref="EndScene"/>
		/// </remarks>
		/// <param name="op">
		///		A rendering operation instance, which contains details of the operation to be performed.
		///	</param>
		public virtual void Render(RenderOperation op) {
			int val;

            if (op.useIndices) {
				val = op.indexData.indexCount;
			}
			else {
				val = op.vertexData.vertexCount;
			}

			// calculate faces
			switch (op.operationType) {
				case OperationType.TriangleList:
					numFaces += val / 3;
					break;
				case OperationType.TriangleStrip:
				case OperationType.TriangleFan:
					numFaces += val - 2;
					break;
				case OperationType.PointList:
				case OperationType.LineList:
				case OperationType.LineStrip:
					break;
			}

			// increment running vertex count
			numVertices += op.vertexData.vertexCount;
		}

		/// <summary>
		///		Utility function for setting all the properties of a texture unit at once.
		///		This method is also worth using over the individual texture unit settings because it
		///		only sets those settings which are different from the current settings for this
		///		unit, thus minimising render state changes.
		/// </summary>
		/// <param name="textureUnit">Index of the texture unit to configure</param>
		/// <param name="layer">Reference to a TextureLayer object which defines all the settings.</param>
		public virtual void SetTextureUnit(int unit, TextureUnitState unitState, bool fixedFunction) {

            // This method is only ever called to set a texture unit to valid details
            // The method _disableTextureUnit is called to turn a unit off

            // Vertex texture binding?
			if (caps.CheckCap(Capabilities.VertexTextureFetch) &&
                !caps.CheckCap(Capabilities.VertexTextureFetch)) {
                if (unitState.BindingType == GpuProgramType.Vertex) {
                    // Bind vertex texture
                    SetVertexTexture(unit, unitState);
                    // bind nothing to fragment unit (hardware isn't shared but fragment
                    // unit can't be using the same index
                    SetTexture(unit, true, string.Empty);
                }
                else {
                    // vice versa
                    SetVertexTexture(unit, unitState);
                    SetTexture(unit, true, unitState.TextureName);
                }
            }
            else {
                // Shared vertex / fragment textures or no vertex texture support
                // Bind texture (may be blank)
                SetTexture(unit, true, unitState.TextureName);
            }

            // Set texture coordinate set
            // SetTextureCoordSet(unit, unitState.TextureCoordSet);
            if (!fixedFunction)
                // From Direct3D9 error log:
                // Texture coordinate index in the stage must be equal to the stage index when programmable vertex pipeline is used
                SetTextureCoordSet(unit, unit);

            // Texture layer filtering
            SetTextureUnitFiltering(
                unit,
                unitState.GetTextureFiltering(FilterType.Min),
                unitState.GetTextureFiltering(FilterType.Mag),
                unitState.GetTextureFiltering(FilterType.Mip));

            // Texture layer anistropy
            SetTextureLayerAnisotropy(unit, unitState.TextureAnisotropy);

            // Set mipmap biasing
            SetTextureMipmapBias(unit, unitState.MipmapBias);

            // Texture addressing mode
            UVWAddressingMode uvw = unitState.GetTextureAddressingMode();
            SetTextureAddressingMode(unit, uvw);
            // Set texture border colour only if required
            if (uvw.u == TextureAddressing.Border ||
                uvw.v == TextureAddressing.Border ||
                uvw.w == TextureAddressing.Border)
                SetTextureBorderColor(unit, unitState.TextureBorderColor);

            // This stuff only gets done for the fixed function pipeline.  It is not needed
            // if we are using a pixel shader.
            if (fixedFunction)
            {
                // Tex Coord Set
                SetTextureCoordSet(unit, unitState.TextureCoordSet);

                // set the texture blending mode
                SetTextureBlendMode(unit, unitState.ColorBlendMode);

                // set the texture blending mode
                SetTextureBlendMode(unit, unitState.AlphaBlendMode);

                bool anyCalcs = false;

                for (int i = 0; i < unitState.NumEffects; i++)
                {
                    TextureEffect effect = unitState.GetEffect(i);

                    switch (effect.type)
                    {
                        case TextureEffectType.EnvironmentMap:
                            if ((EnvironmentMap)effect.subtype == EnvironmentMap.Curved)
                            {
                                SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMap);
                                anyCalcs = true;
                            }
                            else if ((EnvironmentMap)effect.subtype == EnvironmentMap.Planar)
                            {
                                SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMapPlanar);
                                anyCalcs = true;
                            }
                            else if ((EnvironmentMap)effect.subtype == EnvironmentMap.Reflection)
                            {
                                SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMapReflection);
                                anyCalcs = true;
                            }
                            else if ((EnvironmentMap)effect.subtype == EnvironmentMap.Normal)
                            {
                                SetTextureCoordCalculation(unit, TexCoordCalcMethod.EnvironmentMapNormal);
                                anyCalcs = true;
                            }
                            break;

                        case TextureEffectType.Scroll:
                        case TextureEffectType.Rotate:
                        case TextureEffectType.Transform:
                            break;

                        case TextureEffectType.ProjectiveTexture:
                            SetTextureCoordCalculation(unit, TexCoordCalcMethod.ProjectiveTexture, effect.frustum);
                            anyCalcs = true;
                            break;
                    } // switch
                } // for

                // Ensure any previous texcoord calc settings are reset if there are now none
                if (!anyCalcs)
                {
                    SetTextureCoordCalculation(unit, TexCoordCalcMethod.None);
                }

                // set the texture matrix to that of the current layer for any transformations
                SetTextureMatrix(unit, unitState.TextureMatrix);
            }
		}

		/// <summary>
		///    Binds a texture to a vertex sampler.
		/// </summary>
		/// <remarks>
		///     Not all rendersystems support separate vertex samplers. For those that
		///     do, you can set a texture for them, separate to the regular texture
		///     samplers, using this method. For those that don't, you should use the
		///     regular texture samplers which are shared between the vertex and
		///     fragment units; calling this method will throw an exception.
		///     @see RenderSystemCapabilites::getVertexTextureUnitsShared
        /// </remarks>
        public void SetVertexTexture(int unit, TextureUnitState unitState)
        {
            throw new AxiomException( 
                "This rendersystem does not support separate vertex texture samplers, " +
                "you should use the regular texture samplers which are shared between " +
                "the vertex and fragment units." +
                "In RenderSystem.SetVertexTexture");
        }

		/// <summary>
		///    Sets the filtering options for a given texture unit.
		/// </summary>
		/// <param name="unit">The texture unit to set the filtering options for.</param>
		/// <param name="minFilter">The filter used when a texture is reduced in size.</param>
		/// <param name="magFilter">The filter used when a texture is magnified.</param>
		/// <param name="mipFilter">
		///		The filter used between mipmap levels, <see cref="FilterOptions.None"/> disables mipmapping.
		/// </param>
		public void SetTextureUnitFiltering(int unit, FilterOptions minFilter, FilterOptions magFilter, FilterOptions mipFilter) {
			SetTextureUnitFiltering(unit, FilterType.Min, minFilter);
			SetTextureUnitFiltering(unit, FilterType.Mag, magFilter);
			SetTextureUnitFiltering(unit, FilterType.Mip, mipFilter);
		}

		/// <summary>
		///	
		/// </summary>
		/// <param name="matrices"></param>
		/// <param name="count"></param>
		public virtual void SetWorldMatrices(Matrix4[] matrices, ushort count) {
			if (!caps.CheckCap(Capabilities.VertexBlending)) {
				// save these for later during software vertex blending
				for (int i = 0; i < count; i++) {
					worldMatrices[i] = matrices[i];
				}

				// reset the hardware world matrix to identity
				WorldMatrix = Matrix4.Identity;
			}
		}

        public virtual void RemoveRenderTargets() 
		{
			// destroy each render window
            while (renderTargets.Count > 0) {
                RenderTarget target = renderTargets[0];
                DetachRenderTarget(target);
                target.Dispose();
            }
    	}

		/// <summary>
		///		Shuts down the RenderSystem.
		/// </summary>
		public virtual void Shutdown() {
            RemoveRenderTargets();

            // dispose of the render system
			this.Dispose();
		}

		/// <summary>
		///    Internal method for updating all render targets attached to this rendering system.
		/// </summary>
		public virtual void UpdateAllRenderTargets() {
			// Update all in order of priority
			// This ensures render-to-texture targets get updated before render windows
			for (int i = 0; i < renderTargets.Count; i++) {
				RenderTarget target = renderTargets[i];

				// update whether or not it is active
				if (target.IsAutoUpdated) {
					target.Update();
				}
			}
		}

		#endregion Methods

		#endregion Virtual Members

		#region Abstract Members

		/// <summary>
		///		Sets the depth bias, NB you should use the Material version of this. 
		/// </summary>
		/// <remarks>
        ///     When polygons are coplanar, you can get problems with 'depth fighting' where
        ///     the pixels from the two polys compete for the same screen pixel. This is particularly
        ///     a problem for decals (polys attached to another surface to represent details such as
        ///     bulletholes etc.).
        ///     
        ///     A way to combat this problem is to use a depth bias to adjust the depth buffer value
        ///     used for the decal such that it is slightly higher than the true value, ensuring that
        ///     the decal appears on top.
        ///     
        ///     The final bias value is a combination of a constant bias and a bias proportional
        ///     to the maximum depth slope of the polygon being rendered. The final bias
        ///     is constantBias + slopeScaleBias * maxslope. Slope scale biasing is
        ///     generally preferable but is not available on older hardware.
        /// <param name="constantBias">
        ///     The constant bias value, expressed as a value in homogenous depth coordinates.
		/// </param>
        /// <param name="slopeScaleBias">
        ///     The bias value which is factored by the maximum slope
        ///     of the polygon, see the 
        ///     description above. This is not supported by all cards.
        /// </param>
		public abstract void SetDepthBias(float constantBias, float slopScaleBias);

		#region Properties

		/// <summary>
		///		Sets the color & strength of the ambient (global directionless) light in the world.
		/// </summary>
		public abstract ColorEx AmbientLight { get; set; }

		/// <summary>
		///    Gets/Sets the culling mode for the render system based on the 'vertex winding'.
		/// </summary>
		/// <remarks>
		///		A typical way for the rendering engine to cull triangles is based on the
		///		'vertex winding' of triangles. Vertex winding refers to the direction in
		///		which the vertices are passed or indexed to in the rendering operation as viewed
		///		from the camera, and will wither be clockwise or counterclockwise.  The default is <see cref="CullingMode.Clockwise"/>  
		///		i.e. that only triangles whose vertices are passed/indexed in counterclockwise order are rendered - this 
		///		is a common approach and is used in 3D studio models for example. You can alter this culling mode 
		///		if you wish but it is not advised unless you know what you are doing. You may wish to use the 
		///		<see cref="CullingMode.None"/> option for mesh data that you cull yourself where the vertex winding is uncertain.
		/// </remarks>
		public abstract CullingMode CullingMode { get; set; }

		/// <summary>
		///		Gets/Sets whether or not the depth buffer is updated after a pixel write.
		/// </summary>
		/// <value>
		///		If true, the depth buffer is updated with the depth of the new pixel if the depth test succeeds.
		///		If false, the depth buffer is left unchanged even if a new pixel is written.
		/// </value>
		public abstract bool DepthWrite { get; set; }

		/// <summary>
		///		Gets/Sets whether or not the depth buffer check is performed before a pixel write.
		/// </summary>
		/// <value>
		///		If true, the depth buffer is tested for each pixel and the frame buffer is only updated
		///		if the depth function test succeeds. If false, no test is performed and pixels are always written.
		/// </value>
		public abstract bool DepthCheck { get; set; }

		/// <summary>
		///		Gets/Sets the comparison function for the depth buffer check.
		/// </summary>
		/// <remarks>
		///		Advanced use only - allows you to choose the function applied to compare the depth values of
		///		new and existing pixels in the depth buffer. Only an issue if the depth buffer check is enabled.
		/// <seealso cref="DepthCheck"/>
		/// </remarks>
		/// <value>
		///		The comparison between the new depth and the existing depth which must return true
		///		for the new pixel to be written.
		/// </value>
		public abstract CompareFunction DepthFunction { get; set; }

		/// <summary>
		///		Returns the horizontal texel offset value required for mapping 
		///		texel origins to pixel origins in this rendersystem.
		/// </summary>
		/// <remarks>
		///		Since rendersystems sometimes disagree on the origin of a texel, 
		///		mapping from texels to pixels can sometimes be problematic to 
		///		implement generically. This method allows you to retrieve the offset
		///		required to map the origin of a texel to the origin of a pixel in
		///		the horizontal direction.
		/// </remarks>
		public abstract float HorizontalTexelOffset { get; }

		/// <summary>
		///		Gets/Sets whether or not dynamic lighting is enabled.
		///		<p/>
		///		If true, dynamic lighting is performed on geometry with normals supplied, geometry without
		///		normals will not be displayed. If false, no lighting is applied and all geometry will be full brightness.
		/// </summary>
		public abstract bool LightingEnabled { get; set; }

		/// <summary>
		///    Get/Sets whether or not normals are to be automatically normalized.
		/// </summary>
		/// <remarks>
		///    This is useful when, for example, you are scaling SceneNodes such that
		///    normals may not be unit-length anymore. Note though that this has an
		///    overhead so should not be turn on unless you really need it.
		///    <p/>
		///    You should not normally call this direct unless you are rendering
		///    world geometry; set it on the Renderable because otherwise it will be
		///    overridden by material settings. 
		/// </remarks>
		public abstract bool NormalizeNormals { get; set; }

		/// <summary>
		///		Gets/Sets the current projection matrix.
		///	</summary>
		public abstract Matrix4 ProjectionMatrix { get; set; }

		/// <summary>
		///		Gets/Sets how to rasterise triangles, as points, wireframe or solid polys.
		/// </summary>
		public abstract SceneDetailLevel RasterizationMode { get; set; }

		/// <summary>
		///		Gets/Sets the type of light shading required (default = Gouraud).
		/// </summary>
		public abstract Shading ShadingMode { get; set; }

		/// <summary>
		///		Turns stencil buffer checking on or off. 
		/// </summary>
		///	<remarks>
		///		Stencilling (masking off areas of the rendering target based on the stencil 
		///		buffer) can be turned on or off using this method. By default, stencilling is
		///		disabled.
		///	</remarks>
		public abstract bool StencilCheckEnabled { get; set; }

		/// <summary>
		///		Returns the vertical texel offset value required for mapping 
		///		texel origins to pixel origins in this rendersystem.
		/// </summary>
		/// <remarks>
		///		Since rendersystems sometimes disagree on the origin of a texel, 
		///		mapping from texels to pixels can sometimes be problematic to 
		///		implement generically. This method allows you to retrieve the offset
		///		required to map the origin of a texel to the origin of a pixel in
		///		the vertical direction.
		/// </remarks>
		public abstract float VerticalTexelOffset { get; }

		/// <summary>
		///		Gets/Sets the current view matrix.
		///	</summary>
		public abstract Matrix4 ViewMatrix { get; set; }

		/// <summary>
		///		Gets/Sets the current world matrix.
		/// </summary>
		public abstract Matrix4 WorldMatrix { get; set; }

		/// <summary>
        ///     Sets whether or not rendering points using OT_POINT_LIST will 
		///     render point sprites (textured quads) or plain points.
		/// </summary>
		public abstract bool PointSpritesEnabled { set; }
        
		#endregion Properties

		#region Methods

		/// <summary>
		///		Update a perspective projection matrix to use 'oblique depth projection'.
		/// </summary>
		/// <remarks>
		///		This method can be used to change the nature of a perspective 
		///		transform in order to make the near plane not perpendicular to the 
		///		camera view direction, but to be at some different orientation. 
		///		This can be useful for performing arbitrary clipping (e.g. to a 
		///		reflection plane) which could otherwise only be done using user
		///		clip planes, which are more expensive, and not necessarily supported
		///		on all cards.
		/// </remarks>
		/// <param name="projMatrix">
		///		The existing projection matrix. Note that this must be a
		///		perspective transform (not orthographic), and must not have already
		///		been altered by this method. The matrix will be altered in-place.
		/// </param>
		/// <param name="plane">
		///		The plane which is to be used as the clipping plane. This
		///		plane must be in CAMERA (view) space.
		///	</param>
		/// <param name="forGpuProgram">Is this for use with a Gpu program or fixed-function transforms?</param>
		public abstract void ApplyObliqueDepthProjection(ref Matrix4 projMatrix, Plane plane, bool forGpuProgram);

		/// <summary>
		///		Signifies the beginning of a frame, ie the start of rendering on a single viewport. Will occur
		///		several times per complete frame if multiple viewports exist.
		/// </summary>
		public abstract void BeginFrame();

		/// <summary>
		///    Binds a given GpuProgram (but not the parameters). 
		/// </summary>
		/// <remarks>
		///    Only one GpuProgram of each type can be bound at once, binding another
		///    one will simply replace the existing one.
		/// </remarks>
		/// <param name="program"></param>
        public virtual void BindGpuProgram(GpuProgram program) {
            switch (program.Type) {
                case GpuProgramType.Vertex:
                    vertexProgramBound = true;
                    break;
                case GpuProgramType.Fragment:
                    fragmentProgramBound = true;
                    break;
            }
        }

		/// <summary>
		///    Bind Gpu program parameters.
		/// </summary>
		/// <param name="parms"></param>
        public abstract void BindGpuProgramParameters(GpuProgramType type, GpuProgramParameters parms);

		/// <summary>
		///		Clears one or more frame buffers on the active render target.
		/// </summary>
		/// <param name="buffers">
		///		Combination of one or more elements of <see cref="FrameBuffer"/>
		///		denoting which buffers are to be cleared.
		/// </param>
		/// <param name="color">The color to clear the color buffer with, if enabled.</param>
		/// <param name="depth">The value to initialize the depth buffer with, if enabled.</param>
		/// <param name="stencil">The value to initialize the stencil buffer with, if enabled.</param>
		public abstract void ClearFrameBuffer(FrameBuffer buffers, ColorEx color, float depth, int stencil);

		/// <summary>
		///		Converts the Axiom.Core.ColorEx value to a int.  Each API may need the 
		///		bytes of the packed color data in different orders. i.e. OpenGL - ABGR, D3D - ARGB
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public abstract uint ConvertColor(ColorEx color);

		/// <summary>
		///		Converts the int value to an Axiom.Core.ColorEx object.  Each API may have the 
		///		bytes of the packed color data in different orders. i.e. OpenGL - ABGR, D3D - ARGB
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public abstract ColorEx ConvertColor(uint color);

		/// <summary>
		///    Creates and registers a render texture object.
		/// </summary>
		/// <param name="name">The name for the new render texture. Note that names must be unique.</param>
		/// <param name="width">Requested width for the render texture.</param>
		/// <param name="height">Requested height for the render texture.</param>
		/// <param name="format">Requested pixel format for the render texture.</param>
		/// <returns>
		///    On success, a reference to a new API-dependent, RenderTexture-derived
		///    class is returned. On failure, null is returned.
		/// </returns>
		/// <remarks>
		///    Because a render texture is basically a wrapper around a texture object,
		///    the width and height parameters of this method just hint the preferred
		///    size for the texture. Depending on the hardware driver or the underlying
		///    API, these values might change when the texture is created.
		/// </remarks>
        //public abstract RenderTexture CreateRenderTexture(string name, int width, int height, PixelFormat format);

        //public virtual RenderTexture CreateRenderTexture(string name, int width, int height, 
        //                                         TextureType ttype, PixelFormat format,
        //                                         Dictionary<string, string> miscParams) {
        //    /// Create a new 2D texture, and return surface to render to
        //    Texture texture = 
        //        TextureManager.Instance.CreateManual(name, ttype, width, height, 
        //                                             0, format, TextureUsage.RenderTarget);
        //    // Ensure texture loaded and internal resources created
        //    texture.Load();

        //    return texture.GetBuffer().GetRenderTarget();
        //}

        //public RenderTexture CreateRenderTexture(string name, int width, int height) {
        //    return CreateRenderTexture(name, width, height, PixelFormat.R8G8B8);
        //}		

		/// <summary>
		///		Creates a new render window.
		/// </summary>
		/// <remarks>
		///		This method creates a new rendering window as specified
		///		by the paramteters. The rendering system could be
		///		responible for only a single window (e.g. in the case
		///		of a game), or could be in charge of multiple ones (in the
		///		case of a level editor). The option to create the window
		///		as a child of another is therefore given.
		///		This method will create an appropriate subclass of
		///		RenderWindow depending on the API and platform implementation.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="isFullscreen"></param>
        /// <param name="miscParams">
		///		An array of attribute name followed by value (contains things like colorDepth).
		///	</param>
		/// <returns></returns>
		public abstract RenderWindow CreateRenderWindow(string name, int width, int height, bool isFullscreen, 
                                                        params object[] miscParams);

		/// <summary>
		///		Requests an API implementation of a hardware occlusion query used to test for the number
		///		of fragments rendered between calls to <see cref="IHardwareOcclusionQuery.Begin"/> and 
		///		<see cref="IHardwareOcclusionQuery.End"/> that pass the depth buffer test.
		/// </summary>
		/// <returns>An API specific implementation of an occlusion query.</returns>
		public abstract IHardwareOcclusionQuery CreateHardwareOcclusionQuery();

		/// <summary>
		///		Ends rendering of a frame to the current viewport.
		/// </summary>
		public abstract void EndFrame();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="autoCreateWindow"></param>
        public virtual RenderWindow Initialize(bool autoCreateWindow, string windowTitle) {
            vertexProgramBound = false;
            fragmentProgramBound = false;
            return null;
        }

        public virtual RenderWindow Initialize(bool autoCreateWindow, string windowTitle, string initialLoadBitmap) {
            return Initialize(autoCreateWindow, windowTitle);
        }

		/// <summary>
		///		Builds an orthographic projection matrix suitable for this render system.
		/// </summary>
		/// <remarks>
		///		Because different APIs have different requirements (some incompatible) for the
		///		projection matrix, this method allows each to implement their own correctly and pass
		///		back a generic Matrix4 for storage in the engine.
		///	 </remarks>
		/// <param name="fov">Field of view angle.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <param name="near">Near clipping plane distance.</param>
		/// <param name="far">Far clipping plane distance.</param>
		/// <param name="forGpuProgram"></param>
		/// <returns></returns>
		public abstract Matrix4 MakeOrthoMatrix(float fov, float aspectRatio, float near, float far, bool forGpuPrograms);

        /// <summary>
        /// 	Converts a uniform projection matrix to one suitable for this render system.
        /// </summary>
        /// <remarks>
        ///		Because different APIs have different requirements (some incompatible) for the
        ///		projection matrix, this method allows each to implement their own correctly and pass
        ///		back a generic Matrix4 for storage in the engine.
        ///	 </remarks>
        /// <param name="matrix"></param>
        /// <param name="forGpuProgram"></param>
        /// <returns></returns>
        public abstract Matrix4 ConvertProjectionMatrix(Matrix4 matrix, bool forGpuProgram);

		/// <summary>
		///		Builds a perspective projection matrix suitable for this render system.
		/// </summary>
		/// <remarks>
		///		Because different APIs have different requirements (some incompatible) for the
		///		projection matrix, this method allows each to implement their own correctly and pass
		///		back a generic Matrix4 for storage in the engine.
		///	 </remarks>
		/// <param name="fov">Field of view angle.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <param name="near">Near clipping plane distance.</param>
		/// <param name="far">Far clipping plane distance.</param>
		/// <param name="forGpuProgram"></param>
		/// <returns></returns>
		public abstract Matrix4 MakeProjectionMatrix(float fov, float aspectRatio, float near, float far, bool forGpuProgram);

        public abstract Dictionary<string, ConfigOption> GetConfigOptions();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="func"></param>
		/// <param name="val"></param>
		public abstract void SetAlphaRejectSettings(CompareFunction func, byte val);

        /// <summary>
        ///   Used to confirm the settings (normally chosen by the user) in
        ///   order to make the renderer able to inialize with the settings as required.
        ///   This make be video mode, D3D driver, full screen / windowed etc.
        ///   Called automatically by the default configuration
        ///   dialog, and by the restoration of saved settings.
        ///   These settings are stored and only activeated when 
        ///   RenderSystem::Initalize or RenderSystem::Reinitialize are called
        /// </summary>
        /// <param name="name">the name of the option to alter</param>
        /// <param name="value">the value to set the option to</param>
        public abstract void SetConfigOption(string name, string value);

		/// <summary>
		///    Sets whether or not color buffer writing is enabled, and for which channels. 
		/// </summary>
		/// <remarks>
		///    For some advanced effects, you may wish to turn off the writing of certain color
		///    channels, or even all of the color channels so that only the depth buffer is updated
		///    in a rendering pass. However, the chances are that you really want to use this option
		///    through the Material class.
		/// </remarks>
		/// <param name="red">Writing enabled for red channel.</param>
		/// <param name="green">Writing enabled for green channel.</param>
		/// <param name="blue">Writing enabled for blue channel.</param>
		/// <param name="alpha">Writing enabled for alpha channel.</param>
		public abstract void SetColorBufferWriteEnabled(bool red, bool green, bool blue, bool alpha);

		/// <summary>
		///		Sets the mode of operation for depth buffer tests from this point onwards.
		/// </summary>
		/// <remarks>
		///		Sometimes you may wish to alter the behavior of the depth buffer to achieve
		///		special effects. Because it's unlikely that you'll set these options for an entire frame,
		///		but rather use them to tweak settings between rendering objects, this is intended for internal
		///		uses, which will be used by a <see cref="SceneManager"/> implementation rather than directly from 
		///		the client application.
		/// </remarks>
		/// <param name="depthTest">
		///		If true, the depth buffer is tested for each pixel and the frame buffer is only updated
		///		if the depth function test succeeds. If false, no test is performed and pixels are always written.
		/// </param>
		/// <param name="depthWrite">
		///		If true, the depth buffer is updated with the depth of the new pixel if the depth test succeeds.
		///		If false, the depth buffer is left unchanged even if a new pixel is written.
		/// </param>
		/// <param name="depthFunction">Sets the function required for the depth test.</param>
		public abstract void SetDepthBufferParams(bool depthTest, bool depthWrite, CompareFunction depthFunction);

		/// <summary>
		///		Sets the fog with the given params.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="color"></param>
		/// <param name="density"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public abstract void SetFog(FogMode mode, ColorEx color, float density, float start, float end);

		/// <summary>
		///		Sets the global blending factors for combining subsequent renders with the existing frame contents.
		///		The result of the blending operation is:</p>
		///		<p align="center">final = (texture * src) + (pixel * dest)</p>
		///		Each of the factors is specified as one of a number of options, as specified in the SceneBlendFactor
		///		enumerated type.
		/// </summary>
		/// <param name="src">The source factor in the above calculation, i.e. multiplied by the texture color components.</param>
		/// <param name="dest">The destination factor in the above calculation, i.e. multiplied by the pixel color components.</param>
		public abstract void SetSceneBlending(SceneBlendFactor src, SceneBlendFactor dest);

        /// <summary>
        ///   Set up a texture and region that will be used for the hardware cursor.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="section"></param>
        public void SetCursor(Texture texture, System.Drawing.Rectangle section) {
            SetCursor(texture, section, new System.Drawing.Point(0, 0));
        }

        /// <summary>
        ///   Set up a texture and region that will be used for the hardware cursor.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="section"></param>
        /// <param name="hotSpot"></param>
        public abstract void SetCursor(Texture texture, System.Drawing.Rectangle section, System.Drawing.Point hotSpot);
        
        /// <summary>
        ///   Method to update our cursor position when we use hardware cursors
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void SetCursorPosition(int x, int y);
        
        /// <summary>
        ///     Clear the current cursor, so that we will not draw a cursor.
        /// </summary>
        public abstract void ClearCursor();

        /// <summary>
        ///     Restore the cursor.  This is used so that after another window 
        ///     took control of the cursor, we can set it back to the surface
        ///     we specified in the SetCursor call.
        /// </summary>
        public abstract void RestoreCursor();

		/// <summary>
		///     Sets the 'scissor region' ie the region of the target in which rendering can take place.
		/// </summary>
		/// <remarks>
		///     This method allows you to 'mask off' rendering in all but a given rectangular area
		///     as identified by the parameters to this method.
		///     <p/>
		///     Not all systems support this method. Check the <see cref="Axiom.Graphics.Capabilites"/> enum for the
		///     ScissorTest capability to see if it is supported.
		/// </remarks>
		/// <param name="enabled">True to enable the scissor test, false to disable it.</param>
		/// <param name="left">Left corner (in pixels).</param>
		/// <param name="top">Top corner (in pixels).</param>
		/// <param name="right">Right corner (in pixels).</param>
		/// <param name="bottom">Bottom corner (in pixels).</param>
		public abstract void SetScissorTest(bool enable, int left, int top, int right, int bottom);

		public void SetScissorTest(bool enable) {
			SetScissorTest(enable, 0, 0, 800, 600);
		}

		/// <summary>
		///		This method allows you to set all the stencil buffer parameters in one call.
		/// </summary>
		/// <remarks>
		///		<para>
		///		The stencil buffer is used to mask out pixels in the render target, allowing
		///		you to do effects like mirrors, cut-outs, stencil shadows and more. Each of
		///		your batches of rendering is likely to ignore the stencil buffer, 
		///		update it with new values, or apply it to mask the output of the render.
		///		The stencil test is:<PRE>
		///		(Reference Value & Mask) CompareFunction (Stencil Buffer Value & Mask)</PRE>
		///		The result of this will cause one of 3 actions depending on whether the test fails,
		///		succeeds but with the depth buffer check still failing, or succeeds with the
		///		depth buffer check passing too.</para>
		///		<para>
		///		Unlike other render states, stencilling is left for the application to turn
		///		on and off when it requires. This is because you are likely to want to change
		///		parameters between batches of arbitrary objects and control the ordering yourself.
		///		In order to batch things this way, you'll want to use OGRE's separate render queue
		///		groups (see RenderQueue) and register a RenderQueueListener to get notifications
		///		between batches.</para>
		///		<para>
		///		There are individual state change methods for each of the parameters set using 
		///		this method. 
		///		Note that the default values in this method represent the defaults at system 
		///		start up too.</para>
		/// </remarks>
		/// <param name="function">The comparison function applied.</param>
		/// <param name="refValue">The reference value used in the comparison.</param>
		/// <param name="mask">
		///		The bitmask applied to both the stencil value and the reference value 
		///		before comparison.
		/// </param>
		/// <param name="stencilFailOp">The action to perform when the stencil check fails.</param>
		/// <param name="depthFailOp">
		///		The action to perform when the stencil check passes, but the depth buffer check still fails.
		/// </param>
		/// <param name="passOp">The action to take when both the stencil and depth check pass.</param>
		/// <param name="twoSidedOperation">
		///		If set to true, then if you render both back and front faces 
		///		(you'll have to turn off culling) then these parameters will apply for front faces, 
		///		and the inverse of them will happen for back faces (keep remains the same).
		/// </param>
		public abstract void SetStencilBufferParams(CompareFunction function, int refValue, int mask,
			StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp, bool twoSidedOperation);

		/// <summary>
		///		Sets the surface parameters to be used during rendering an object.
		/// </summary>
		/// <param name="ambient"></param>
		/// <param name="diffuse"></param>
		/// <param name="specular"></param>
		/// <param name="emissive"></param>
		/// <param name="shininess"></param>
        /// <param name="tracking"></param>
		public abstract void SetSurfaceParams(ColorEx ambient, ColorEx diffuse, ColorEx specular, ColorEx emissive, float shininess, TrackVertexColor tracking);

		/// <summary>
		///		Sets the details of a texture stage, to be used for all primitives
		///		rendered afterwards. User processes would
		///		not normally call this direct unless rendering
		///		primitives themselves - the SubEntity class
		///		is designed to manage materials for objects.
		///		Note that this method is called by SetMaterial.
		/// </summary>
		/// <param name="stage">The index of the texture unit to modify. Multitexturing hardware
		//		can support multiple units (see TextureUnitCount)</param>
		/// <param name="enabled">Boolean to turn the unit on/off</param>
		/// <param name="textureName">The name of the texture to use - this should have
		///		already been loaded with TextureManager.Load.</param>
		public abstract void SetTexture(int stage, bool enabled, string textureName);

		/// <summary>
		///		Tells the hardware how to treat texture coordinates.
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="texAddressingMode"></param>
		public abstract void SetTextureAddressingMode(int stage, TextureAddressing texAddressingMode);

        /// <summary>
        ///		Tells the hardware how to treat texture coordinates.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="texAddressingMode"></param>
        public abstract void SetTextureAddressingMode(int stage, UVWAddressingMode uvwAddressingMode);

        /// <summary>
        ///    Tells the hardware what border color to use when texture addressing mode is set to Border
        /// </summary>
        /// <param name="state"></param>
        /// <param name="borderColor"></param>
        public abstract void SetTextureBorderColor(int stage, ColorEx borderColor);

		/// <summary>
		///		Sets the texture blend modes from a TextureLayer record.
		///		Meant for use internally only - apps should use the Material
		///		and TextureLayer classes.
		/// </summary>
		/// <param name="stage">Texture unit.</param>
		/// <param name="blendMode">Details of the blending modes.</param>
		public abstract void SetTextureBlendMode(int stage, LayerBlendModeEx blendMode);

		/// <summary>
		///		Sets a method for automatically calculating texture coordinates for a stage.
		/// </summary>
		/// <param name="stage">Texture stage to modify.</param>
		/// <param name="method">Calculation method to use</param>
		/// <param name="frustum">Frustum, only used for projective effects</param>
		public abstract void SetTextureCoordCalculation(int stage, TexCoordCalcMethod method, Frustum frustum);

		/// <summary>
		///		Sets the index into the set of tex coords that will be currently used by the render system.
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="index"></param>
		public abstract void SetTextureCoordSet(int stage, int index);

		/// <summary>
		///		Sets the maximal anisotropy for the specified texture unit.
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="index">maxAnisotropy</param>
		public abstract void SetTextureLayerAnisotropy(int stage, int maxAnisotropy);

		/// <summary>
		///		Sets the texture matrix for the specified stage.  Used to apply rotations, translations,
		///		and scaling to textures.
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="xform"></param>
		public abstract void SetTextureMatrix(int stage, Matrix4 xform);

		/// <summary>
		///    Sets a single filter for a given texture unit.
		/// </summary>
		/// <param name="stage">The texture unit to set the filtering options for.</param>
		/// <param name="type">The filter type.</param>
		/// <param name="filter">The filter to be used.</param>
		public abstract void SetTextureUnitFiltering(int stage, FilterType type, FilterOptions filter);

        /// <summary>
        ///		Sets the mipmap bias for the texture unit.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="bias"></param>
        public abstract void SetTextureMipmapBias(int stage, float bias);

		/// <summary>
		///		Sets the current viewport that will be rendered to.
		/// </summary>
		/// <param name="viewport"></param>
		public abstract void SetViewport(Viewport viewport);

		/// <summary>
		///    Unbinds the current GpuProgram of a given GpuProgramType.
		/// </summary>
		/// <param name="type"></param>
        public virtual void UnbindGpuProgram(GpuProgramType type) {
            switch (type) {
                case GpuProgramType.Vertex:
                    vertexProgramBound = false;
                    break;
                case GpuProgramType.Fragment:
                    fragmentProgramBound = false;
                    break;
            }
        }

        /// <summary>
        ///    Gets the bound status of a given GpuProgramType.
        /// </summary>
        /// <param name="type"></param>
        public bool IsGpuProgramBound(GpuProgramType type) {
            switch (type) {
                case GpuProgramType.Vertex:
                    return vertexProgramBound;
                case GpuProgramType.Fragment:
                    return fragmentProgramBound;
            }
            return false;
        }

		/// <summary>
		///    Tells the rendersystem to use the attached set of lights (and no others) 
		///    up to the number specified (this allows the same list to be used with different
		///    count limits).
		/// </summary>
		/// <param name="lightList">List of lights.</param>
		/// <param name="limit">Max number of lights that can be used from the list currently.</param>
		public abstract void UseLights(List<Light> lightList, int limit);


		/// <summary>
		///		Sets the size of points and how they are attenuated with distance.
		/// </summary>
		/// <remarks>
		///		When performing point rendering or point sprite rendering,
		///		point size can be attenuated with distance. The equation for
		///		
		///		For example, to disable distance attenuation (constant screensize) 
		///		you would set constant to 1, and linear and quadratic to 0. A
		///		standard perspective attenuation would be 0, 1, 0 respectively.
		/// </remarks>
		public abstract void SetPointParameters(float size, bool attenuationEnabled, 
			float constant, float linear, float quadratic, float minSize, float maxSize);

		#endregion Methods

		#endregion Abstract Members

        /// <summary>
        ///   Destroys a render target of any sort
        /// </summary>
        /// <param name="name"></param>
        public virtual void DestroyRenderTarget(string name) {
            RenderTarget rt = DetachRenderTarget(name);
            rt.Dispose();
        }
        
        /// <summary>
        ///   Destroys a render window
        /// </summary>
        /// <param name="name"></param>
        public virtual void DestroyRenderWindow(string name) {
            DestroyRenderTarget(name);
        }
        
        /// <summary>
        ///   Destroys a render texture
        /// </summary>
        /// <param name="name"></param>
        public virtual void DestroyRenderTexture(string name) {
            DestroyRenderTarget(name);
        }

        /// <summary>
        /// This is used to insert events into the PIX trace for DirectX debugging and profiling.
        /// </summary>
        /// <param name="color">Color to display the event</param>
        /// <param name="message">Message to display</param>
        /// <returns>nesting level of events</returns>
        public abstract int BeginProfileEvent(ColorEx color, string message);

        /// <summary>
        /// This is used to end an event in the PIX trace on DirectX
        /// </summary>
        /// <returns>nesting level of events</returns>
        public abstract int EndProfileEvent();

        /// <summary>
        /// Set an instantaneous marker in the profiling event log.  (PIX for DirectX)
        /// </summary>
        /// <param name="color"></param>
        /// <param name="message"></param>
        public abstract void SetProfileMarker(ColorEx color, string message);

		#region Overloaded Methods

        /// <summary>
        ///		Converts a uniform projection matrix to one suitable for this render system.
        /// </summary>
        /// <remarks>
        ///		Because different APIs have different requirements (some incompatible) for the
        ///		projection matrix, this method allows each to implement their own correctly and pass
        ///		back a generic Matrix4 for storage in the engine.
        ///	 </remarks>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public Matrix4 ConvertProjectionMatrix(Matrix4 matrix) {
            // create without consideration for Gpu programs by default
            return ConvertProjectionMatrix(matrix, false);
        }

        /// <summary>
        ///		Builds a perspective projection matrix suitable for this render system.
        /// </summary>
        /// <remarks>
        ///		Because different APIs have different requirements (some incompatible) for the
        ///		projection matrix, this method allows each to implement their own correctly and pass
        ///		back a generic Matrix4 for storage in the engine.
        ///	 </remarks>
        /// <param name="fov">Field of view angle.</param>
        /// <param name="aspectRatio">Aspect ratio.</param>
        /// <param name="near">Near clipping plane distance.</param>
        /// <param name="far">Far clipping plane distance.</param>
        /// <returns></returns>
        public Matrix4 MakeProjectionMatrix(float fov, float aspectRatio, float near, float far) {
            // create without consideration for Gpu programs by default
            return MakeProjectionMatrix(fov, aspectRatio, near, far, false);
        }

		/// <summary>
		///		Builds a orthographic projection matrix suitable for this render system.
		/// </summary>
		/// <remarks>
		///		Because different APIs have different requirements (some incompatible) for the
		///		projection matrix, this method allows each to implement their own correctly and pass
		///		back a generic Matrix4 for storage in the engine.
		///	 </remarks>
		/// <param name="fov">Field of view angle.</param>
		/// <param name="aspectRatio">Aspect ratio.</param>
		/// <param name="near">Near clipping plane distance.</param>
		/// <param name="far">Far clipping plane distance.</param>
		/// <returns></returns>
		public Matrix4 MakeOrthoMatrix(float fov, float aspectRatio, float near, float far) {
			return MakeOrthoMatrix(fov, aspectRatio, near, far, false);
		}

		/// <summary>
		///		Initialize the rendering engine.
		/// </summary>
		/// <param name="autoCreateWindow">If true, a default window is created to serve as a rendering target.</param>
		/// <returns>A RenderWindow implementation specific to this RenderSystem.</returns>
		public RenderWindow Initialize(bool autoCreateWindow) {
			return Initialize(autoCreateWindow, DefaultWindowTitle);
		}

		/// <summary>
		///		Sets a method for automatically calculating texture coordinates for a stage.
		/// </summary>
		/// <param name="stage">Texture stage to modify.</param>
		/// <param name="method">Calculation method to use</param>
		public void SetTextureCoordCalculation(int stage, TexCoordCalcMethod method) {
			SetTextureCoordCalculation(stage, method, null);
		}

		#region SetDepthBufferParams()

		public void SetDepthBufferParams() {
			SetDepthBufferParams(true, true, CompareFunction.LessEqual);
		}

		public void SetDepthBufferParams(bool depthTest) {
			SetDepthBufferParams(depthTest, true, CompareFunction.LessEqual);
		}

		public void SetDepthBufferParams(bool depthTest, bool depthWrite) {
			SetDepthBufferParams(depthTest, depthWrite, CompareFunction.LessEqual);
		}

		#endregion SetDepthBufferParams()

		#region SetStencilBufferParams()

		public void SetStencilBufferParams() {
			SetStencilBufferParams(CompareFunction.AlwaysPass, 0, unchecked((int)0xffffffff), 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}
		
		public void SetStencilBufferParams(CompareFunction function) {
			SetStencilBufferParams(function, 0, unchecked((int)0xffffffff), 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue) {
			SetStencilBufferParams(function, refValue, unchecked((int)0xffffffff), 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask) {
			SetStencilBufferParams(function, refValue, mask, 
				StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp) {

			SetStencilBufferParams(function, refValue, mask, 
				stencilFailOp, StencilOperation.Keep, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp, StencilOperation depthFailOp) {

			SetStencilBufferParams(function, refValue, mask, 
				stencilFailOp, depthFailOp, StencilOperation.Keep, false);
		}

		public void SetStencilBufferParams(CompareFunction function, int refValue, int mask, 
			StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp) {

			SetStencilBufferParams(function, refValue, mask, 
				stencilFailOp, depthFailOp, passOp, false);
		}

		#endregion SetStencilBufferParams() 

		#region ClearFrameBuffer()

		public void ClearFrameBuffer(FrameBuffer buffers, ColorEx color, float depth) {
			ClearFrameBuffer(buffers, color, depth, 0);
		}

		public void ClearFrameBuffer(FrameBuffer buffers, ColorEx color) {
			ClearFrameBuffer(buffers, color, 1.0f, 0);
		}

		public void ClearFrameBuffer(FrameBuffer buffers) {
			ClearFrameBuffer(buffers, ColorEx.Black, 1.0f, 0);
		}

		#endregion ClearFrameBuffer()

		#endregion Overloaded Methods

		#region Object overrides

		/// <summary>
		/// Returns the name of this RenderSystem.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return this.Name;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		///		Override to dispose of resources on shutdown if needed.
		/// </summary>
		public virtual void Dispose() {
			// no default implementation
        }

		#endregion

    }

}