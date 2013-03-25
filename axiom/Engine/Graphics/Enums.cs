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
using System.Reflection;
using Axiom.Scripting;

namespace Axiom.Graphics {

	/// <summary>
	///    Parameters that are available via the engine and automatically caclulated for use in GPU programs.
	/// </summary>
	public enum AutoConstants {

		/// <summary>
		///    Global ambient light color.
		/// </summary>
		[ScriptEnum("ambient_light_colour")]
		AmbientLightColor,

        /// <summary>
        /// Provides a parametric animation value [0..1], only available
        /// where the renderable specifically implements it.
		/// </summary>
        [ScriptEnum("animation_parametric")]
		AnimationParametric,

        /// <summary>
        ///    The current camera's position in world space.
        /// </summary>
        [ScriptEnum("camera_position")]
        CameraPosition,

        /// <summary>
        ///    The current camera's position in object space.
        /// </summary>
        [ScriptEnum("camera_position_object_space")]
        CameraPositionObjectSpace,

		/// <summary>
		///    Cosine of "Time0_1". Equivalent to RenderMonkey's "CosTime0_1".
		/// </summary>
		[ScriptEnum("costime_0_1")]
		CosTime_0_1,

		/// <summary>
		///    Cosine of "Time0_2PI". Equivalent to RenderMonkey's "CosTime0_2PI".
		/// </summary>
		[ScriptEnum("costime_0_2pi")]
		CosTime_0_2PI,

		/// <summary>
		///    Cosine of "Time0_X". Equivalent to RenderMonkey's "CosTime0_X".
		/// </summary>
		[ScriptEnum("costime_0_x")]
		CosTime_0_X,

		/// <summary>
		///    A custom parameter which will come from the renderable, using 'data' as the identifier.
		/// </summary>
		[ScriptEnum("custom")]
		Custom,

		/// <summary>
		///    The derived ambient light colour, with 'r', 'g', 'b' components filled with
		///    product of surface ambient colour and ambient light colour, respectively,
		///    and 'a' component filled with surface ambient alpha component.
		/// </summary>
		[ScriptEnum("derived_ambient_light_colour")]
        DerivedAmbientLightColor,

		/// <summary>
		///    The derived light diffuse colour (index determined by setAutoConstant call),
		///    with 'r', 'g' and 'b' components filled with product of surface diffuse colour
		///    and light diffuse colour, respectively, and 'a' component filled with surface
        ///    diffuse alpha component.
		/// </summary>
		[ScriptEnum("derived_light_diffuse_colour")]
        DerivedLightDiffuseColor,

		/// <summary>
		///    Array of derived light diffuse colours (count set by extra param)
		/// </summary>
		[ScriptEnum("derived_light_diffuse_colour_array")]
        DerivedLightDiffuseColorArray,

		/// <summary>
		///    The derived light specular colour (index determined by setAutoConstant call),
		///    with 'r', 'g' and 'b' components filled with product of surface specular colour
		///    and light specular colour, respectively, and 'a' component filled with surface
        ///    specular alpha component.
		/// </summary>
		[ScriptEnum("derived_light_specular_colour")]
        DerivedLightSpecularColor,

		/// <summary>
		///    Array of derived light specular colours (count set by extra param)
		/// </summary>
		[ScriptEnum("derived_light_specular_colour_array")]
        DerivedLightSpecularColorArray,

		/// <summary>
		///    The derived scene colour, with 'r', 'g' and 'b' components filled with sum
		///    of derived ambient light colour and surface emissive colour, respectively,
		///    and 'a' component filled with surface diffuse alpha component.
		/// </summary>
		[ScriptEnum("derived_scene_colour")]
        DerivedSceneColor,

        /// <summary>
        /// Distance from camera to far clip plane
        /// </summary>
        [ScriptEnum("far_clip_distance")]
        FarClipDistance,

		/// <summary>
		///    Fog color
		/// </summary>
		[ScriptEnum("fog_colour")]
        FogColor,

        /// <summary>
        ///   The params needed for the vertex program to be able to compute the fog weight.
        ///   Includes fogStart, fogEnd, and fogScale.
        /// </summary>
        [ScriptEnum("fog_params")]
        FogParams,

        /// <summary>
        ///   This variable provides the field of view as a floating point value.
        ///   Equivalent to RenderMonkey's "FOV".
        /// </summary>
        [ScriptEnum("fov")]
        FOV,

        /// <summary>
        ///   provides the calculated frames per second, returned as a floating point value.
        /// </summary>
        [ScriptEnum("fps")]
        FPS,

        /// <summary>
        ///   provides the scaled frame time, returned as a floating point value.
        /// </summary>
        [ScriptEnum("frame_time")]
        FrameTime,

        /// <summary>
        ///   Provides inverse of projection matrix.  Equivalent to RenderMonkey's "ProjectionInverse".
        /// </summary>
        [ScriptEnum("inverse_projection_matrix")]
        InverseProjectionMatrix,

        /// <summary>
        ///   Provides inverse texture size of the texture unit (index determined by setAutoConstant
        ///   call). Packed as float4(1 / width, 1 / height, 1 / depth, 1)
        /// </summary>
        [ScriptEnum("inverse_texture_size")]
        InverseTextureSize,

        /// <summary>
        ///   Provides inverse transpose of projection matrix.
        ///   Equivalent to RenderMonkey's "ProjectionInverseTranspose".
        /// </summary>
        [ScriptEnum("inverse_transpose_projection_matrix")]
        InverseTransposeProjectionMatrix,

        /// <summary>
        ///   Provides inverse transpose of concatenated view and projection matrices.
        ///   Equivalent to RenderMonkey's "ViewProjectionInverseTranspose".
        /// </summary>
        [ScriptEnum("inverse_transpose_viewproj_matrix")]
        InverseTransposeViewProjMatrix,

        /// <summary>
        ///   Provides inverse transpose of view matrix.
		///   Equivalent to RenderMonkey's "ViewInverseTranspose".
        /// </summary>
        [ScriptEnum("inverse_transpose_view_matrix")]
        InverseTransposeViewMatrix,

        /// <summary>
        ///   Provides inverse transpose of concatenated world, view and projection
        ///   matrices. Equivalent to RenderMonkey's "WorldViewProjectionInverseTranspose".
        /// </summary>
        [ScriptEnum("inverse_transpose_worldviewproj_matrix")]
        InverseTransposeWorldViewProjMatrix,

        /// <summary>
        ///   The current world & view matrices concatenated, then inverted & tranposed
        /// </summary>
        [ScriptEnum("inverse_transpose_worldview_matrix")]
        InverseTransposeWorldViewMatrix,
			
        /// <summary>
        ///   The current world & view matrices concatenated, then inverted & transposed
        /// </summary>
        [ScriptEnum("inverse_transpose_world_matrix")]
        InverseTransposeWorldMatrix,

        /// <summary>
        ///   This variable represents 1.0/ViewportHeight. 
        ///   Equivalent to RenderMonkey's "ViewportHeightInverse".
        /// </summary>
        [ScriptEnum("inverse_viewport_height")]
        InverseViewportHeight,

        /// <summary>
        ///   This variable represents 1.0/ViewportWidth. 
        ///   Equivalent to RenderMonkey's "ViewportWidthInverse".
        /// </summary>
        [ScriptEnum("inverse_viewport_width")]
        InverseViewportWidth,

        /// <summary>
        ///   Provides inverse of concatenated view and projection matrices.
        ///   Equivalent to RenderMonkey's "ViewProjectionInverse".
        /// </summary>
        [ScriptEnum("inverse_viewproj_matrix")]
        InverseViewProjMatrix,

        /// <summary>
        ///    Current view matrix, inverted.
        /// </summary>
        [ScriptEnum("inverse_view_matrix")]
		InverseViewMatrix,

        /// <summary>
        ///   Provides inverse of concatenated world, view and projection matrices.
        ///   Equivalent to RenderMonkey's "WorldViewProjectionInverse".
        /// </summary>
        [ScriptEnum("inverse_worldviewproj_matrix")]
        InverseWorldViewProjMatrix,

		/// <summary>
		///    Current world and view matrices concatenated, then inverted.
		/// </summary>
		[ScriptEnum("inverse_worldview_matrix")]
		InverseWorldViewMatrix,
			
		/// <summary>
		///    Current world matrix, inverted.
		/// </summary>
		[ScriptEnum("inverse_world_matrix")]
		InverseWorldMatrix,

		/// <summary>
		///    Light attenuation.  Vector4(range, constant, linear, quadratic).
		/// </summary>
		[ScriptEnum("light_attenuation")]
		LightAttenuation,

		/// <summary>
		///    Array of light attenuation parameters, Vector4(range, constant, linear, quadric) (count set by extra param)
		/// </summary>
		[ScriptEnum("light_attenuation_array")]
		LightAttenuationArray,

		/// <summary>
		///    Light diffuse color.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_diffuse_colour")]     
		LightDiffuseColor,

		/// <summary>
		///    Array of light diffuse colours (count set by extra param)
		/// </summary>
		[ScriptEnum("light_diffuse_colour_array")]     
		LightDiffuseColorArray,

		/// <summary>
		///    A light direction in world space.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_direction")]
		LightDirection,

		/// <summary>
		///    Array of light directions in world space (count set by extra param)
		/// </summary>
		[ScriptEnum("light_direction_array")]
		LightDirectionArray,

		/// <summary>
		///    A light direction in object space.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_direction_object_space")]
		LightDirectionObjectSpace,

		/// <summary>
		///    Array of light directions in object space (count set by extra param)
		/// </summary>
		[ScriptEnum("light_direction_object_space_array")]
		LightDirectionObjectSpaceArray,

		/// <summary>
		///    A light direction in view space.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_direction_view_space")]
		LightDirectionViewSpace,

		/// <summary>
		///    Array of light directions in view space (count set by extra param)
		/// </summary>
		[ScriptEnum("light_direction_view_space_array")]
		LightDirectionViewSpaceArray,

		/// <summary>
		///    The distance of the light from the center of the object a useful approximation as an 
		///    alternative to per-vertex distance calculations.
		/// </summary>
		[ScriptEnum("light_distance_object_space")]
		LightDistanceObjectSpace,

		/// <summary>
		///    Array of distances of the lights from the center of the object
		///    a useful approximation as an alternative to per-vertex distance
		///    calculations. (count set by extra param)
		/// </summary>
		[ScriptEnum("light_distance_object_space_array")]
		LightDistanceObjectSpaceArray,

		/// <summary>
		///    A light position in world space.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_position")]
		LightPosition,

		/// <summary>
		///    Array of light positions in world space (count set by extra param)
		/// </summary>
		[ScriptEnum("light_position_array")]
		LightPositionArray,

		/// <summary>
		///    A light position in object space.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_position_object_space")]
		LightPositionObjectSpace,

		/// <summary>
		///    Array of light positions in object space (count set by extra param)
		/// </summary>
		[ScriptEnum("light_position_object_space_array")]
		LightPositionObjectSpaceArray,

		/// <summary>
		///    A light position in view space.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_position_view_space")]
		LightPositionViewSpace,

		/// <summary>
		///    Array of light positions in view space (count set by extra param)
		/// </summary>
		[ScriptEnum("light_position_view_space_array")]
		LightPositionViewSpaceArray,

		/// <summary>
		///    Light power level, a single scalar as set in Light::setPowerScale  (index determined by setAutoConstant call)
		/// </summary>
		[ScriptEnum("light_power")]
		LightPower,

		/// <summary>
		///    Array of light power levels, a single scalar as set in Light::setPowerScale 
        ///    (count set by extra param)
		/// </summary>
		[ScriptEnum("light_power_array")]
		LightPowerArray,

		/// <summary>
		///    Light specular color.  Index determined when setting up auto constants.
		/// </summary>
		[ScriptEnum("light_specular_colour")]  
		LightSpecularColor,

		/// <summary>
		///    Array of light specular colours (count set by extra param)
		/// </summary>
		[ScriptEnum("light_specular_colour_array")]  
		LightSpecularColorArray,

        /// <summary>
        /// Multiverse specific shadow technique in use
        /// </summary>
        [ScriptEnum("mv_shadow_technique")]
        MVShadowTechnique,

        /// <summary>
        /// Distance from camera to near clip plane
        /// </summary>
        [ScriptEnum("near_clip_distance")]
        NearClipDistance,

        /// <summary>
        /// provides the current iteration number of the pass. The iteration
        /// number is the number of times the current render operation has
        /// been drawn for the acitve pass.
        /// </summary>
        [ScriptEnum("pass_iteration_number")]
        PassIterationNumber,

        /// <summary>
        /// Technique pass number
        /// </summary>
        [ScriptEnum("pass_number")]
        PassNumber,

		/// <summary>
		///    Current projection matrix.
		/// </summary>
		[ScriptEnum("projection_matrix")]
		ProjectionMatrix,

        /// <summary>
        ///     Allows you to adjust the position to match with the 'requires texture flipping' 
        ///     flag on render targets when bypassing the standard projection transform.
        /// </summary>
        [ScriptEnum("render_target_flipping")]
        RenderTargetFlipping,

        /// <summary>
        ///     Provides information about the depth range of the scene as viewed
        ///     from the current camera. 
        ///     Passed as float4(minDepth, maxDepth, depthRange, 1 / depthRange)
        /// </summary>
        [ScriptEnum("scene_depth_range")]
        SceneDepthRange,

		/// <summary>
		///    The distance a shadow volume should be extruded when using finite extrusion programs.
		/// </summary>
		[ScriptEnum("shadow_extrusion_distance")]
		ShadowExtrusionDistance,

        /// <summary>
        /// X element is shadow fade near distance
        /// Y element is shadow fade far distance
        /// </summary>
        [ScriptEnum("shadow_fade_params")]
        ShadowFadeParams,

        /// <summary>
        ///     Provides information about the depth range of the scene as viewed
        ///     from a given shadow camera. Requires an index parameter which maps
        ///     to a light index relative to the current light list.
        ///     Passed as float4(minDepth, maxDepth, depthRange, 1 / depthRange)
        /// </summary>
        [ScriptEnum("shadow_scene_depth_range")]
        ShadowSceneDepthRange,

		/// <summary>
		///    Sine of "Time0_1". Equivalent to RenderMonkey's "SinTime0_1".
		/// </summary>
		[ScriptEnum("sintime_0_1")]
		SinTime_0_1,

		/// <summary>
		///    Sine of "Time0_2PI". Equivalent to RenderMonkey's "SinTime0_2PI".
		/// </summary>
		[ScriptEnum("sintime_0_2pi")]
		SinTime_0_2PI,

        /// <summary>
        ///		Specifies that the sin of the time elapsed since last frame 
        ///     will be passed along to the program.
        /// </summary>
        [ScriptEnum("sintime_0_x")]
        SinTime_0_X,

        /// <summary>
        ///     Spotlight parameters, Vector4(innerFactor, outerFactor, falloff, isSpot)
        ///     innerFactor and outerFactor are cos(angle/2)
        ///     The isSpot parameter is 0.0f for non-spotlights, 1.0f for spotlights.
        ///     Also for non-spotlights the inner and outer factors are 1 and nearly 1 respectively
        /// </summary>
        [ScriptEnum("spotlight_params")]
        SpotlightParams,

        /// <summary>
        ///     Spotlight parameter array of Vector4(innerFactor, outerFactor, falloff, isSpot)
        ///     innerFactor and outerFactor are cos(angle/2)
        ///     The isSpot parameter is 0.0f for non-spotlights, 1.0f for spotlights.
        ///     Also for non-spotlights the inner and outer factors are 1 and nearly 1 respectively
        ///     (count set by extra param)
        /// </summary>
        [ScriptEnum("spotlight_params_array")]
        SpotlightParamsArray,

		/// <summary>
		///    Surface ambient colour, as set in Pass::setAmbient
		/// </summary>
		[ScriptEnum("surface_ambient_colour")]
		SurfaceAmbientColor,

		/// <summary>
		///    Surface diffuse colour, as set in Pass::setDiffuse
		/// </summary>
		[ScriptEnum("surface_diffuse_colour")]
		SurfaceDiffuseColor,

		/// <summary>
		///    Surface emissive colour, as set in Pass::setEmissive
		/// </summary>
		[ScriptEnum("surface_emissive_colour")]
		SurfaceEmissiveColor,

		/// <summary>
		///    Surface shininess, as set in Pass::setShininess
		/// </summary>
		[ScriptEnum("surface_shininess")]
		SurfaceShininess,

		/// <summary>
		///    Surface shininess colour, as set in Pass::setShininess
		/// </summary>
		[ScriptEnum("surface_shininess_colour")]
		SurfaceShininessColor,

		/// <summary>
		///    Surface specular colour, as set in Pass::setSpecular
		/// </summary>
		[ScriptEnum("surface_specular_colour")]
		SurfaceSpecularColor,

		/// <summary>
		///    Tangent of "Time0_1". Equivalent to RenderMonkey's "TanTime0_1".
		/// </summary>
		[ScriptEnum("tantime_0_1")]
		TanTime_0_1,

		/// <summary>
		///    Tangent of "Time0_2PI". Equivalent to RenderMonkey's "TanTime0_2PI".
		/// </summary>
		[ScriptEnum("tantime_0_2pi")]
		TanTime_0_2PI,

        /// <summary>
        ///		Tangent of "Time0_X". Equivalent to RenderMonkey's "TanTime0_X".
        /// </summary>
        [ScriptEnum("tantime_0_x")]
        TanTime_0_X,

        /// <summary>
        ///		Provides the texel offsets required by this rendersystem to map
        ///		texels to pixels. Packed as 
        ///		float4(absoluteHorizontalOffset, absoluteVerticalOffset, 
        ///		       horizontalOffset / viewportWidth, verticalOffset / viewportHeight)
        /// </summary>
        [ScriptEnum("texel_offsets")]
        TexelOffsets,

        /// <summary>
        ///		Provides texture size of the texture unit (index determined by setAutoConstant
        ///		call). Packed as float4(width, height, depth, 1)
        /// </summary>
        [ScriptEnum("texture_size")]
        TextureSize,

		/// <summary>
		///    The distance a shadow volume should be extruded when using finite extrusion programs.
		/// </summary>
		[ScriptEnum("texture_viewproj_matrix")]
		TextureViewProjMatrix,

        /// <summary>
        ///		Specifies that the time elapsed since last frame will be passed along to the program.
        /// </summary>
        [ScriptEnum("time")]
        Time,

        /// <summary>
        ///		Specifies that the time elapsed since last frame modulo 1
        ///     will be passed along to the program.
        /// </summary>
        [ScriptEnum("time_0_1")]
        Time_0_1,

        /// <summary>
        ///		Vector of "Time0_1", "SinTime0_1", "CosTime0_1",
        ///     "TanTime0_1". Equivalent to RenderMonkey's "Time0_1_Packed".
        /// </summary>
        [ScriptEnum("time_0_1_packed")]
        Time_0_1_Packed,

        /// <summary>
        ///		Single float value, which represents scaled time value [0..2*Pi],
        ///     which repeats itself based on given as parameter "cycle time".
        ///     Equivalent to RenderMonkey's "Time0_2PI".
        /// </summary>
        [ScriptEnum("time_0_2pi")]
        Time_0_2PI,

        /// <summary>
        ///		Vector of "Time0_2PI", "SinTime0_2PI", "CosTime0_2PI",
        ///     "TanTime0_2PI". Equivalent to RenderMonkey's "Time0_2PI_Packed".
        /// </summary>
        [ScriptEnum("time_0_2pi_packed")]
        Time_0_2PI_Packed,

        /// <summary>
        ///		Specifies that the time elapsed since last frame modulo X 
        ///     will be passed along to the program.
        /// </summary>
        [ScriptEnum("time_0_x")]
        Time_0_X,

        /// <summary>
        ///		Vector of "Time0_X", "SinTime0_X", "CosTime0_X",
        ///     "TanTime0_X". Equivalent to RenderMonkey's "Time0_X_Packed".
        /// </summary>
        [ScriptEnum("time_0_x_packed")]
        Time_0_X_Packed,

        /// <summary>
        ///   Provides transpose of projection matrix.  Equivalent to RenderMonkey's "ProjectionTranspose".
        /// </summary>
        [ScriptEnum("transpose_projection_matrix")]
        TransposeProjectionMatrix,

        /// <summary>
        ///   Provides transpose of concatenated view and projection matrices.
        ///   Equivalent to RenderMonkey's "ViewProjectionTranspose".
        /// </summary>
        [ScriptEnum("transpose_viewproj_matrix")]
        TransposeViewProjMatrix,

        /// <summary>
        ///   Provides transpose of the view matrices.
        ///   Equivalent to RenderMonkey's "ViewTranspose".
        /// </summary>
        [ScriptEnum("transpose_view_matrix")]
        TransposeViewMatrix,

        /// <summary>
        ///   Provides transpose of concatenated world, view and projection matrices.
        ///   Equivalent to RenderMonkey's "WorldViewProjectionTranspose".
        /// </summary>
        [ScriptEnum("transpose_worldviewproj_matrix")]
        TransposeWorldViewProjMatrix,

        /// <summary>
        ///   Provides transpose of concatenated world and view matrices.
        ///   Equivalent to RenderMonkey's "WorldViewTranspose".
        /// </summary>
        [ScriptEnum("transpose_worldview_matrix")]
        TransposeWorldViewMatrix,

        /// <summary>
        ///   Provides transpose of world matrix.
        ///   Equivalent to RenderMonkey's "WorldTranspose".
        /// </summary>
        [ScriptEnum("transpose_world_matrix")]
        TransposeWorldMatrix,

        /// <summary>
        ///   Current viewport height (in pixels) as floating point value.
        ///   Equivalent to RenderMonkey's "ViewportHeight".
        /// </summary>
        [ScriptEnum("viewport_height")]
        ViewportHeight,

        /// <summary>
        ///   Packed of "ViewportWidth", "ViewportHeight", "ViewportWidthInverse",
        ///   "ViewportHeightInverse".
        /// </summary>
        [ScriptEnum("viewport_size")]
        ViewportSize,

        /// <summary>
        ///   Current viewport width (in pixels) as floating point value.
        ///   Equivalent to RenderMonkey's "ViewportWidth".
        /// </summary>
        [ScriptEnum("viewport_width")]
        ViewportWidth,

        /// <summary>
        ///   The current view & projection matrices concatenated
        /// </summary>
        [ScriptEnum("viewproj_matrix")]
        ViewProjMatrix,

        /// <summary>
        /// Direction of the camera
        /// </summary>
        [ScriptEnum("view_direction")]
        ViewDirection,

		/// <summary>
		///    Current view matrix.
		/// </summary>
		[ScriptEnum("view_matrix")]
		ViewMatrix,

        /// <summary>
        /// View local X axis
        /// </summary>
        [ScriptEnum("view_side_vector")]
        ViewSideVector,

        /// <summary>
        /// View local Y axis
        /// </summary>
        [ScriptEnum("view_up_vector")]
        ViewUpVector,

		/// <summary>
		///    Current world, view, and projection matrics concatenated.
		/// </summary>
		[ScriptEnum("worldviewproj_matrix")]
		WorldViewProjMatrix,

		/// <summary>
		///    Current world and view matrices concatenated.
		/// </summary>
		[ScriptEnum("worldview_matrix")]
		WorldViewMatrix,
                
		/// <summary>
		///    Current world matrix.
		/// </summary>
		[ScriptEnum("world_matrix")]
		WorldMatrix,

		/// <summary>
		///    The current array of world matrices, used for blending
		/// </summary>
		[ScriptEnum("world_matrix_array")]
		WorldMatrixArray,

		/// <summary>
		///    The current array of world matrices, as a 3x4 matrix, used for blending.
		/// </summary>
		[ScriptEnum("world_matrix_array_3x4")]
		WorldMatrixArray3x4
	}

	/// <summary>
	///     Describes types of hardware buffer licenses.
	/// </summary>
	public enum BufferLicenseRelease {
		/// <summary>
		///     Licensee will only release buffer when it says so.
		/// </summary>
		Manual,
		/// <summary>
		///     Licensee can have license revoked.
		/// </summary>
		Automatic
	}

	/// <summary>
	///		Describes how a vertex buffer should act when it is locked.
	/// </summary>
	public enum BufferLocking {
		/// <summary>
		/// 
		/// </summary>
		Normal,
		/// <summary>
		///		Discards the <em>entire</em> buffer while locking; this allows optimisation to be 
		///		performed because synchronisation issues are relaxed. Only allowed on buffers 
		///		created with the Dynamic flag. 
		/// </summary>
		Discard,
		/// <summary>
		///		Lock the buffer for reading only. Not allowed in buffers which are created with WriteOnly. 
		///		Mandatory on static buffers, ie those created without the Dynamic flag.
		/// </summary>
		ReadOnly,
		/// <summary>
		///    Potential optimization for some API's.
		/// </summary>
		NoOverwrite
	}

	/// <summary>
	///		Describes how a vertex buffer is to be used, and affects how it is created.
	/// </summary>
	[Flags]
	public enum BufferUsage {
		/// <summary>
		/// 
		/// </summary>
		Static = 1,
		/// <summary>
		///		Indicates the application would like to modify this buffer with the CPU
		///		sometimes. Absence of this flag means the application will never modify. 
		///		Buffers created with this flag will typically end up in AGP memory rather 
		///		than video memory.
		/// </summary>
		Dynamic = 2,
		/// <summary>
		///		Indicates the application will never read the contents of the buffer back, 
		///		it will only ever write data. Locking a buffer with this flag will ALWAYS 
		///		return a pointer to new, blank memory rather than the memory associated 
		///		with the contents of the buffer; this avoids DMA stalls because you can 
		///		write to a new memory area while the previous one is being used
		/// </summary>
		WriteOnly = 4,
        /// <summary>
        ///     Indicates that the application will be refilling the contents
        ///     of the buffer regularly (not just updating, but generating the
        ///     contents from scratch), and therefore does not mind if the contents 
        ///     of the buffer are lost somehow and need to be recreated. This
        ///     allows and additional level of optimisation on the buffer.
        ///     This option only really makes sense when combined with 
        ///     DynamicWriteOnly.
        /// </summary>
        Discardable = 8,
		/// <summary>
		///    Combination of Static and WriteOnly
		/// </summary>
		StaticWriteOnly = 5,
		/// <summary>
		///    Combination of Dynamic and WriteOnly. If you use 
        ///    this, strongly consider using DynamicWriteOnlyDiscardable
        ///    instead if you update the entire contents of the buffer very 
        ///    regularly. 
		/// </summary>
		DynamicWriteOnly = 6,
        DynamicWriteOnlyDiscardable = 14
	}

	/// <summary>
	///		Various types of capabilities supported by hardware that must be checked.
	/// </summary>
	[Flags]
	public enum Capabilities {
		/// <summary>
		///		Supports hardware stencil buffer.
		/// </summary>
		StencilBuffer                   = 0x00000001,
		TextureBlending                 = 0x00000002,
		VertexBlending                  = 0x00000004,
		/// <summary>
		///		Supports anisotropic texture filtering.
		/// </summary>
		AnisotropicFiltering            = 0x00000008,
		/// <summary>
		///		Supports fixed-function DOT3 texture blend.
		/// </summary>
		Dot3	                        = 0x00000010,
		/// <summary>
		///		Supports hardware vertex and index buffers.
		/// </summary>
		VertexBuffer                    = 0x00000020,
		MultiTexturing                  = 0x00000040,
		/// <summary>
		///		Supports generating mipmaps in hardware.
		/// </summary>
		HardwareMipMaps                 = 0x00000080,
		/// <summary>
		///		Supports cube mapping.
		/// </summary>
		CubeMapping                     = 0x00000100,
		/// <summary>
		///		Supports vertex programs (vertex shaders).
		/// </summary>
		VertexPrograms                  = 0x00000200,
		/// <summary>
		///		Supports fragment programs (pixel shaders).
		/// </summary>
		FragmentPrograms                = 0x00000400,
		/// <summary>
		///		Supports compressed textures.
		/// </summary>
		TextureCompression              = 0x00000800,
		/// <summary>
		///		Supports compressed textures in the DXT/ST3C formats.
		/// </summary>
		TextureCompressionDXT           = 0x00001000,
		/// <summary>
		///		Supports compressed textures in the VTC format.
		/// </summary>
		TextureCompressionVTC           = 0x00002000,
		/// <summary>
		///		Supports performing a scissor test to exclude areas of the screen.
		/// </summary>
		ScissorTest                     = 0x00004000,
		/// <summary>
		///		Supports separate stencil updates for both front and back faces.
		/// </summary>
		TwoSidedStencil					= 0x00008000,
		/// <summary>
		///		Supports wrapping the stencil value at the range extremeties.
		/// </summary>
		StencilWrap						= 0x00010000,
		/// <summary>
		///		Hardware occlusion queries.
		/// </summary>
		HardwareOcculusion				= 0x00020000,
		/// <summary>
		///		User clipping planes.
		/// </summary>
		UserClipPlanes					= 0x00040000,
		/// <summary>
		///		4 byte wide vertex elemet format.
		/// </summary>
		VertexFormatUByte4				= 0x00080000,
		/// <summary>
		///		Infinite far plane projection
		/// </summary>
		InfiniteFarPlane				= 0x00100000,
		/// <summary>
        ///     Supports hardware render-to-texture (bigger than framebuffer)
        /// </summary>
        HardwareRenderToTexture         = 0x00200000,
        /// <summary>
        ///     Supports float textures and render targets
        /// </summary>
        TextureFloat                    = 0x00400000,
        /// <summary>
        ///     Supports non-power of two textures
        /// </summary>
        NonPowerOf2Textures             = 0x00800000,
        /// <summary>
        ///     Supports 3d (volume) textures
        /// </summary>
        Texture3D                       = 0x01000000,
        /// <summary>
        ///     Supports basic point sprite rendering
        /// </summary>
        PointSprites                    = 0x02000000,
        /// <summary>
        ///     Supports extra point parameters (minsize, maxsize, attenuation)
        /// </summary>
        PointExtendedParameters         = 0x04000000,
        /// <summary>
        ///     Supports vertex texture fetch
        /// </summary>
		VertexTextureFetch              = 0x08000000, 
        /// <summary>
        ///     Supports mipmap LOD biasing
        /// </summary>
		MipmapLodBias                   = 0x10000000 
	}

	/// <summary>
	///  Comparison functions, for things such as stencil buffer or depth comparisons.
	/// </summary>
	public enum CompareFunction {
        [ScriptEnum("always_fail")]
		AlwaysFail,
        [ScriptEnum("always_pass")]
		AlwaysPass,
        [ScriptEnum("less")]
		Less,
        [ScriptEnum("less_equal")]
		LessEqual,
        [ScriptEnum("equal")]
		Equal,
        [ScriptEnum("not_equal")]
		NotEqual,
        [ScriptEnum("greater_equal")]
		GreaterEqual,
        [ScriptEnum("greater")]
		Greater
	};

	/// <summary>
	/// Options for deciding what geometry gets excluded from the rendering process.
	/// </summary>
	public enum CullingMode {
		/// <summary>
		///		Draw everything (2 sided geometry).
		///	 </summary>
		[ScriptEnum("none")]
		None,
		/// <summary>
		///		Only draw geomtry where vertices were specified in clockwise order.
		///	 </summary>
		[ScriptEnum("clockwise")]
		Clockwise,
		/// <summary>
		///		Only draw geomtry where vertices were specified in counterclockwise order.
		///	 </summary>
		[ScriptEnum("anticlockwise")]
		CounterClockwise
	}

	/// <summary>
	///		Specifes the type of environment mapping to use.
	/// </summary>
	/// <remarks>
	///    Note that these have no effect when using the programmable pipeline, since their
	///    effect is overridden by the vertex / fragment programs.
	/// </remarks>
	public enum EnvironmentMap {
		/// <summary>
		///		Envmap based on vector from camera to vertex position, good for planar geometry.
		///	 </summary>
		[ScriptEnum("spherical")]
		Curved,
		/// <summary>
		///		Envmap based on dot of vector from camera to vertex and vertex normal, good for curves.
		///	 </summary>
		[ScriptEnum("planar")]
		Planar,
		/// <summary>
		///		Envmap entended to supply reflection vectors for cube mapping.
		/// </summary>
		[ScriptEnum("cubic_reflection")]
		Reflection,
		/// <summary>
		///		Envmap entended to supply normal vectors for cube mapping
		/// </summary>
		[ScriptEnum("cubic_normal")]
		Normal
	}

	/// <summary>
	///     A type of face group, i.e. face list of procedural etc
	/// </summary>
	public enum FaceGroup {
		FaceList,
		Patch,
		Unknown
	}

	/// <summary>
	///    Filtering options for textures / mipmaps.
	/// </summary>
	public enum FilterOptions {
		/// <summary>
		///    No filtering, used for FilterType.Mip to turn off mipmapping.
		/// </summary>
		[ScriptEnum("none")]
		None,
		/// <summary>
		///    Use the closest pixel.
		/// </summary>
		[ScriptEnum("point")]
		Point,
		/// <summary>
		///    Average of a 2x2 pixel area, denotes bilinear for Min and Mag, trilinear for Mip.
		/// </summary>
		[ScriptEnum("linear")]
		Linear,
		/// <summary>
		///    Similar to Linear, but compensates for the angle of the texture plane.
		/// </summary>
		[ScriptEnum("anisotropic")]
		Anisotropic
	}

	/// <summary>
	///    Stages of texture rendering to which filters can be applied.
	/// </summary>
	public enum FilterType {
		/// <summary>
		///    The filter used when shrinking a texture.
		/// </summary>
		Min,
		/// <summary>
		///    The filter used when magnifiying a texture.
		/// </summary>
		Mag,
		/// <summary>
		///    The filter used when determining the mipmap.
		/// </summary>
		Mip
	}

	/// <summary>
	/// Type of fog to use in the scene.
	/// </summary>
	public enum FogMode {
		/// <summary>
		///		No fog.
		///	 </summary>
		[ScriptEnum("none")]
		None,
		/// <summary>
		///		Fog density increases exponentially from the camera (fog = 1/e^(distance * density)).
		///	 </summary>
		[ScriptEnum("exp")]
		Exp,
		/// <summary>
		///		Fog density increases at the square of FOG_EXP, i.e. even quicker (fog = 1/e^(distance * density)^2).
		///	 </summary>
		[ScriptEnum("exp2")]
		Exp2,
		/// <summary>
		///		Fog density increases linearly between the start and end distances.
		///	 </summary>
		[ScriptEnum("linear")]
		Linear
	}

	/// <summary>
	///    Enumerates the types of programs which can run on the GPU.
	/// </summary>
	public enum GpuProgramType {
		/// <summary>
		///    Executes for each vertex passed through the pipeline while this program is active.
		/// </summary>
		Vertex,
		/// <summary>
		///    Executes for each fragment (or pixel) for primitives that are passed through the pipeline
		///    while this program is active..
		/// </summary>
		Fragment
	}

	/// <summary>
	///    Enumerates the types of parameters that can be specified for shaders
	/// </summary>
	public enum GpuProgramParameterType {
		/// <summary>
		///    Parameter is passed in by index. Used for ASM shaders.
		/// </summary>
		Indexed,

		/// <summary>
		///    Parameter is managed by Axiom and passed in by index. Used for ASM shaders.
		/// </summary>

		IndexedAuto,

		/// <summary>
		///    Parameter is passed in by name. Used for high-level shaders.
		/// </summary>
		Named,

		/// <summary>
		///    Parameter is managed by Axiom and passed in by name. Used for HL shaders.
		/// </summary>
		NamedAuto
	}


	/// <summary>
	///		Defines the frame buffers which can be cleared.
	/// </summary>
	[Flags]
	public enum FrameBuffer {
		Color	= 0x1,
		Depth	= 0x2,
		Stencil = 0x4
	}

	/// <summary>
	///		Describes the stage of rendering when performing complex illumination.
	/// </summary>
	public enum IlluminationRenderStage {
		/// <summary>
		///		No special illumination stage.
		/// </summary>
		None,
		/// <summary>
		///		Ambient stage, when background light is added.
		/// </summary>
		Ambient,
		/// <summary>
		///		Diffuse / specular stage, when individual light contributions are added.
		/// </summary>
		PerLight,
		/// <summary>
		///		Decal stage, when texture detail is added to the lit base.
		/// </summary>
		Decal,
		/// <summary>
		///		Render to texture stage, used for texture based shadows.
		/// </summary>
		RenderToTexture,
		/// <summary>
		///		Modulative render from shadow texture stage.
		/// </summary>
		RenderModulativePass
	}

    // illumination pass state type
    public enum IlluminationPassesState {
        CompileDisabled = -1,
        NotCompiled = 0,
        Compiled = 1
    }

	/// <summary>
	///		Possible stages of illumination during the rendering process.
	/// </summary>
	public enum IlluminationStage {
		/// <summary>
		///		Part of the rendering which occurs without any kind of direct lighting.
		/// </summary>
		Ambient,
		/// <summary>
		///		Part of the rendering which occurs per light.
		/// </summary>
		PerLight,
		/// <summary>
		///		Post-lighting rendering.
		/// </summary>
		Decal
	}

	/// <summary>
	///		Type of index buffer to use.
	/// </summary>
	/// <remarks>
	///		No declarations can begin with a number, so Size prefix is used.
	/// </remarks>
	public enum IndexType {
		Size16,
		Size32
	}

	/// <summary>
	///		Lists the texture layer operations that are available on both multipass and multitexture
	///		hardware.
	/// </summary>
	public enum LayerBlendOperation {
		/// <summary>
		///		Replace all color with texture and no adjustment.
		/// </summary>
		[ScriptEnum("replace")]
		Replace,
		/// <summary>
		///		Add color components together.
		/// </summary>
		[ScriptEnum("add")]
		Add,
		/// <summary>
		///		Multiply the color components together.
		/// </summary>
		[ScriptEnum("modulate")]
		Modulate,
		/// <summary>
		///		Blend based on texture alpha.
		/// </summary>
		[ScriptEnum("alpha_blend")]
		AlphaBlend
	}

	/// <summary>
	///		Full and complete list of valid texture blending operations.  Fallbacks will be required on older hardware
	///		that does not supports some of these multitexturing techniques.
	/// </summary>
	public enum LayerBlendOperationEx {
		/// <summary>
		///		Use source 1 as is.
		/// </summary>
		[ScriptEnum("source1")]
		Source1,
		/// <summary>
		///		Use source 2 as is.
		/// </summary>
		[ScriptEnum("source2")]
		Source2,
		/// <summary>
		///		Multiply source 1 and source 2 together.
		/// </summary>
		[ScriptEnum("modulate")]
		Modulate,
		/// <summary>
		///		Same as Modulate, but brightens as a result.
		/// </summary>
		[ScriptEnum("modulate_x2")]
		ModulateX2,
		/// <summary>
		///		Same as ModuleX2, but brightens even more as a result.
		/// </summary>
		[ScriptEnum("modulate_x4")]
		ModulateX4,
		/// <summary>
		///		Add source 1 and source 2 together.
		/// </summary>
		[ScriptEnum("add")]
		Add,
		/// <summary>
		///		Same as Add, but subtracts 0.5 from the result.
		/// </summary>
		[ScriptEnum("add_signed")]
		AddSigned,
		/// <summary>
		///		Same as Add, but subtracts the product from the sum.
		/// </summary>
		[ScriptEnum("add_smooth")]
		AddSmooth,
		/// <summary>
		///		Subtract source 2 from source 1.
		/// </summary>
		[ScriptEnum("subtract")]
		Subtract,
		/// <summary>
		///		Use interpolated alpha value from vertices to scale source 1, then add source 2 scaled by 1 - alpha
		/// </summary>
		[ScriptEnum("blend_diffuse_alpha")]
		BlendDiffuseAlpha,
		/// <summary>
		///		Same as BlendDiffuseAlpha, but uses current alpha from the texture.
		/// </summary>
		[ScriptEnum("blend_texture_alpha")]
		BlendTextureAlpha,
		/// <summary>
		///		Same as BlendDiffuseAlpha, but uses current alpha from previous stages.
		/// </summary>
		[ScriptEnum("blend_current_alpha")]
		BlendCurrentAlpha,
		/// <summary>
		///		Sames as BlendDiffuseAlpha, but uses a constant manual blend value from [0.0,1.0]
		/// </summary>
		[ScriptEnum("blend_manual")]
		BlendManual,
		/// <summary>
		///		Takes the dot product of color 1 and color 2.
		/// </summary>
		[ScriptEnum("dotproduct")]
		DotProduct,
		/// <summary>
		///		Use interpolated colour value from vertices to scale source1, then add source2 scaled by (1-colour).
		/// </summary>
		[ScriptEnum("blend_diffuse_color")]
		BlendDiffuseColor
	}

	/// <summary>
	///		Valid sources of values for texture layer blending operations.
	/// </summary>
	public enum LayerBlendSource {
		/// <summary>
		///		The color as built up from previous stages.
		/// </summary>
		[ScriptEnum("src_current")]
		Current,
		/// <summary>
		///		The color derived from the texture assigned to the current layer.
		/// </summary>
		[ScriptEnum("src_texture")]
		Texture,
		/// <summary>
		///		The interpolated diffuse color from the vertices.
		/// </summary>
		[ScriptEnum("src_diffuse")]
		Diffuse,
		/// <summary>
		///		The interpolated specular color from the vertices.
		/// </summary>
		[ScriptEnum("src_specular")]
		Specular,
		/// <summary>
		///		A color supplied manually as a seperate argument.
		/// </summary>
		[ScriptEnum("src_manual")]
		Manual
	}

	/// <summary>
	///		Texture blending mode.
	/// </summary>
	public enum LayerBlendType {
		/// <summary>
		///		Based on diffuse color of the texture.
		/// </summary>
		[ScriptEnum("color")]
		Color,
		/// <summary>
		///		Based on the alpha value of the texture.
		/// </summary>
		[ScriptEnum("alpha")]
		Alpha
	}

	/// <summary>
	///		Texture content type: named or shadow
	/// </summary>
	public enum TextureContentType {
		/// <summary>
		///		Named content
		/// </summary>
		[ScriptEnum("named")]
        Named,
		/// <summary>
		///		Shadow content
		/// </summary>
		[ScriptEnum("shadow")]
		Shadow
    }

    /// <summary>
	///		Defines the types of lights that can be added to a scene.
	/// </summary>
	public enum LightType {
		/// <summary>
		///		Point light sources give off light equally in all directions, so require only position not direction.
		///	 </summary>
		[ScriptEnum("point")]
		Point,
		/// <summary>
		///		Directional lights simulate parallel light beams from a distant source, hence have direction but no position.
		///	 </summary>
		[ScriptEnum("directional")]
		Directional,
		/// <summary>
		///		Spotlights simulate a cone of light from a source so require position and direction, plus extra values for falloff.
		///	 </summary>
		[ScriptEnum("spot")]
		Spotlight
	}

	/// <summary>
	///		Manual culling modes based on vertex normals.
	///		This setting applies to how the software culls triangles before sending them to the 
	///		hardware API. This culling mode is used by scene managers which choose to implement it -
	///		normally those which deal with large amounts of fixed world geometry which is often 
	///		planar (software culling movable variable geometry is expensive).
	/// </summary>
	public enum ManualCullingMode {
		/// <summary>
		///		No culling so everything is sent to the hardware.
		///	 </summary>
		[ScriptEnum("none")]
		None = 1,
		/// <summary>
		///		Cull triangles whose normal is pointing away from the camera (default).
		///	 </summary>
		[ScriptEnum("back")]
		Back = 2,
		/// <summary>
		///		Cull triangles whose normal is pointing towards the camera.
		///	 </summary>
		[ScriptEnum("front")]
		Front = 3
	}

	/// <summary>
	/// Type of projection used by the camera.
	/// </summary>
	public enum Projection {
		/// <summary> Things stay the same size no matter where they are in terms of the camera.  Normally only used in 3D HUD elements. </summary>
		Orthographic,
		/// <summary> Things get smaller when they are furthur away from the camera. </summary>
		Perspective
	}

	/// <summary>
	///		Types for determining which render operation to do for a series of vertices.
	/// </summary>
	public enum OperationType {
		/// <summary>
		///		Render the vertices as individual points.
		/// </summary>
		PointList = 1,
		/// <summary>
		///		Render the vertices as a series of individual lines.
		/// </summary>
		LineList,
		/// <summary>
		///		Render the vertices as a continuous line.
		/// </summary>
		LineStrip,
		/// <summary>
		///		Render the vertices as a series of individual triangles.
		/// </summary>
		TriangleList,
		/// <summary>
		///		Render the vertices as a continous set of triangles in a zigzag type fashion.
		/// </summary>
		TriangleStrip,
		/// <summary>
		///		Render the vertices as a set of trinagles in a fan like formation.
		/// </summary>
		TriangleFan
	}

	/// <summary>
	///    Specifies priorities for processing Render Targets.
	/// </summary>
	public enum RenderTargetPriority {
		/// <summary>
		///    Will be processed last.
		/// </summary>
		Low,
		/// <summary>
		///    Will be processed first (i.e. RenderTextures).
		/// </summary>
		High
	}

	/// <summary>
	///		Blending factors for manually blending objects with the scene. If there isn't a predefined
	///		SceneBlendType that you like, then you can specify the blending factors directly to affect the
	///		combination of object and the existing scene. See Material.SceneBlending for more details.
	/// </summary>
	public enum SceneBlendFactor {
		/// <summary></summary>
		[ScriptEnum("one")]
		One,
		/// <summary></summary>
		[ScriptEnum("zero")]
		Zero,
		/// <summary></summary>
		[ScriptEnum("dest_colour")]
		DestColor,
		/// <summary></summary>
		[ScriptEnum("src_colour")]
		SourceColor,
		/// <summary></summary>
		[ScriptEnum("one_minus_dest_colour")]
		OneMinusDestColor,
		/// <summary></summary>
		[ScriptEnum("one_minus_src_colour")]
		OneMinusSourceColor,
		/// <summary></summary>
		[ScriptEnum("dest_alpha")]
		DestAlpha,
		/// <summary></summary>
		[ScriptEnum("src_alpha")]
		SourceAlpha,
		/// <summary></summary>
		[ScriptEnum("one_minus_dest_alpha")]
		OneMinusDestAlpha,
		/// <summary></summary>
		[ScriptEnum("one_minus_src_alpha")]
		OneMinusSourceAlpha
	}

	/// <summary>
	///		Types of blending that you can specify between an object and the existing contents of the scene.
	/// </summary>
	public enum SceneBlendType {
		/// <summary>
		///		Make the object transparent based on the final alpha values in the texture.
		///	 </summary>
		[ScriptEnum("alpha_blend")]
		TransparentAlpha,
		/// <summary>
		///		Make the object transparent based on the color values in the texture (brighter = more opaque).
		///	 </summary>
		[ScriptEnum("colour_blend")]
		TransparentColor,
        /// <summary>
        ///		Make the object transparent based on the color values in the texture (brighter = more opaque).
        ///	 </summary>
        [ScriptEnum("modulate")]
		Modulate,
		/// <summary>
		///		Add the texture values to the existing scene content.
		///	 </summary>
		[ScriptEnum("add")]
		Add,
		/// <summary>
		///		Replace the texture values to the existing scene content.
		///	 </summary>
		[ScriptEnum("replace")]
        Replace
	}

	/// <summary>
	/// The broad type of detail for rendering.
	/// </summary>
	public enum SceneDetailLevel {
		/// <summary>
		///		Render subsequent requests drawing only the vertices in the scene.
		/// </summary>
		Points,
		/// <summary>
		///		Render subsequent requests drawing only objects using wireframe mode.
		/// </summary>
		Wireframe,
		/// <summary>
		///		Render everything in the scene normally (textures, etc).
		/// </summary>
		Solid
	}

	/// <summary>
	/// Types for deciding how to shade geometry primitives.
	/// </summary>
	public enum Shading {
		/// <summary>
		///		Draw with a single color.
		///	 </summary>
		[ScriptEnum("flat")]
		Flat,
		/// <summary>
		///		Interpolate color across primitive vertices.
		///	 </summary>
		[ScriptEnum("gouraud")]
		Gouraud,
		/// <summary>
		///		Draw everything (2 sided geometry).
		///	 </summary>
		[ScriptEnum("phong")]
		Phong
	}

	/// <summary>
	///		A set of flags that can be used to influence <see cref="ShadowRenderable"/> creation.
	/// </summary>
	public enum ShadowRenderableFlags {
		/// <summary>
		///		For shadow volume techniques only, generate a light cap on the volume.
		/// </summary>
		IncludeLightCap = 1,
		/// <summary>
		///		For shadow volume techniques only, generate a dark cap on the volume.
		/// </summary>
		IncludeDarkCap	= 2,
		/// <summary>
		///		For shadow volume techniques only, indicates volume is extruded to infinity
		/// </summary>
		ExtrudeToInfinity = 4
	}

	/// <summary>
	///	An enumeration of broad shadow techniques .
	/// </summary>
	public enum ShadowTechnique {
		/// <summary>
		///		No shadows.
		/// </summary>
		None,
		/// <summary>
		///		Stencil shadow technique which renders all shadow volumes as
		///		a modulation after all the non-transparent areas have been 
		///		rendered. This technique is considerably less fillrate intensive 
		///		than the additive stencil shadow approach when there are multiple
		///		lights, but is not an accurate model. 
		/// </summary>
		StencilModulative,
		///	<summary>		
		///		Stencil shadow technique which renders each light as a separate
		///		additive pass to the scene. This technique can be very fillrate
		///		intensive because it requires at least 2 passes of the entire
		///		scene, more if there are multiple lights. However, it is a more
		///		accurate model than the modulative stencil approach and this is
		///		especially apparant when using coloured lights or bump mapping.
		/// </summary>
		StencilAdditive,
		/// <summary>
		///		Texture-based shadow technique which involves a monochrome render-to-texture
		///		of the shadow caster and a projection of that texture onto the 
		///		shadow receivers as a modulative pass.
		/// </summary>
		TextureModulative,
		/// <summary>
		///		Texture-based shadow technique which involves a render-to-texture
		///		of the shadow caster and a projection of that texture onto the 
		///		shadow receivers, followed by a depth test to detect the closest
		///		fragment to the light.
		/// </summary>
		TextureAdditive
	}

	/// <summary>
	///		Describes the various actions which can be taken on the stencil buffer.
	///	</summary> 
	public enum StencilOperation {
		/// <summary>
		///		Leave the stencil buffer unchanged.
		///	 </summary>
		Keep,
		/// <summary>
		///		Set the stencil value to zero.
		///	 </summary>
		Zero,
		/// <summary>
		///		Set the stencil value to the reference value.
		///	 </summary>
		Replace,
		/// <summary>
		///		Increase the stencil value by 1, clamping at the maximum value.
		///	 </summary>
		Increment,
		/// <summary>
		///		Decrease the stencil value by 1, clamping at 0.
		///	 </summary>
		Decrement,
		/// <summary>
		///		Increase the stencil value by 1, wrapping back to 0 when incrementing the maximum value.
		///	 </summary>
		IncrementWrap,
		/// <summary>
		///		Decrease the stencil value by 1, wrapping when decrementing 0.
		///	 </summary>
		DecrementWrap,
		/// <summary>
		///		Invert the bits of the stencil buffer.
		///	 </summary>
		Invert
	};

	/// <summary>
	/// Texture addressing modes - default is Wrap.
	/// </summary>
	/// <remarks>
	///    These settings are relevant in both the fixed-function and programmable pipeline.
	/// </remarks>
	public enum TextureAddressing {
		/// <summary>
		///		Texture wraps at values over 1.0 
		///	 </summary>
		[ScriptEnum("wrap")] 
		Wrap,
		/// <summary>
		///		Texture mirrors (flips) at joins over 1.0.
		///	 </summary>
		[ScriptEnum("mirror")]
		Mirror,
		/// <summary>
		///		Texture clamps at 1.0.
		///	 </summary>
		[ScriptEnum("clamp")]
		Clamp,
		/// <summary>
        ///		Values outside the range [0.0, 1.0] are set to the border colour
		///	 </summary>
		[ScriptEnum("border")]
		Border
	}

	/// <summary>
	///		Describes the ways to generate texture coordinates.
	/// </summary>
	public enum TexCoordCalcMethod {
		/// <summary>
		///		No calculated texture coordinates.
		///	 </summary>
		None,
		/// <summary>
		///		Environment map based on vertex normals.
		///	 </summary>
		EnvironmentMap,
		/// <summary>
		///		Environment map based on vertex positions.
		///	 </summary>
		EnvironmentMapPlanar,
		EnvironmentMapReflection,
		EnvironmentMapNormal,
		/// <summary>
		///		Projective texture.
		///	 </summary>
		ProjectiveTexture
	}

	/// <summary>
	/// Enum identifying the frame indexes for faces of a cube map (not the composite 3D type.
	/// </summary>
	public enum TextureCubeFace {
		Front,
		Back,
		Left,
		Right,
		Up,
		Down
	}

	/// <summary>
	///    Definition of the broad types of texture effect you can apply to a texture layer.
	/// </summary>
	/// <remarks>
	///    Note that these have no effect when using the programmable pipeline, since their
	///    effect is overridden by the vertex / fragment programs.
	/// </remarks>
	public enum TextureEffectType {
		/// <summary>
		///		Generate all texture coords based on angle between camera and vertex.
		///	</summary>
		EnvironmentMap,
		/// <summary>
		///		Generate texture coords based on a frustum.
		///	</summary>
		ProjectiveTexture,
		/// <summary>
		///		Constant u/v scrolling effect.
		///	</summary>
        Scroll,
		/// <summary>
		///		Constant rotation.
		///	</summary>
		Rotate,
		/// <summary>
		///		More complex transform.
		///	</summary>
		Transform
	}

	/// <summary>
	///    Texture filtering defining the different minification and magnification.
	/// </summary>
	public enum TextureFiltering {
		/// <summary>
		///		Equal to: min=Point, mag=Point, mip=None
		///	 </summary>
		[ScriptEnum("none")]
		None,
		/// <summary>
		///		Equal to: min=Linear, mag=Linear, mip=Point
		///	 </summary>
		[ScriptEnum("bilinear")]
		Bilinear,
		/// <summary>
		///		Equal to: min=Linear, mag=Linear, mip=Linear
		///	 </summary>
		[ScriptEnum("trilinear")]
		Trilinear,
		/// <summary>
		///    Equal to: min=Anisotropic, max=Anisotropic, mip=Linear
		/// </summary>
		[ScriptEnum("anisotropic")]
		Anisotropic
	}

	/// <summary>
	/// Useful enumeration when dealing with procedural transforms.
	/// </summary>
	/// <remarks>
	///    Note that these have no effect when using the programmable pipeline, since their
	///    effect is overridden by the vertex / fragment programs.
	/// </remarks>
	public enum TextureTransform {
		/// <summary>
		/// 
		/// </summary>
		[ScriptEnum("scroll_x")]
		TranslateU,
		/// <summary>
		/// 
		/// </summary>
		[ScriptEnum("scroll_y")]
		TranslateV,
		/// <summary>
		/// 
		/// </summary>
		[ScriptEnum("scale_x")]
		ScaleU,
		/// <summary>
		/// 
		/// </summary>
		[ScriptEnum("scale_y")]
		ScaleV,
		/// <summary>
		/// 
		/// </summary>
		[ScriptEnum("rotate")]
		Rotate
	}

	/// <summary>
	///    Enum identifying the texture type.
	/// </summary>
	public enum TextureType {
		/// <summary>
		///    1D texture, used in combination with 1D texture coordinates.
		/// </summary>
		[ScriptEnum("1d")]
		OneD = 1,
		/// <summary>
		///    2D texture, used in combination with 2D texture coordinates (default).
		/// </summary>
		[ScriptEnum("2d")]
		TwoD = 2,
		/// <summary>
		///    3D volume texture, used in combination with 3D texture coordinates.
		/// </summary>
		[ScriptEnum("3d")]
		ThreeD = 3,
		/// <summary>
		///    3D cube map, used in combination with 3D texture coordinates.
		/// </summary>
		[ScriptEnum("cubic")]
		CubeMap = 4,
		/// <summary>
		///    This means that the texture manager should load the
		///    default number of mipmaps
		/// </summary>
		[ScriptEnum("unlimited")]
		MipUnliminted,
        /// <summary>
		///    Sets whether luminace image should be treat as alpha
		///    format when we load the texture.
		/// </summary>
		[ScriptEnum("alpha")]
        IsAlpha
	}

	/// <summary>
	///		Specifies how a texture is to be used in the engine.
	/// </summary>
    [Flags]
	public enum TextureUsage {
        Static                      = BufferUsage.Static,
		Dynamic                     = BufferUsage.Dynamic,
		WriteOnly                   = BufferUsage.WriteOnly,
        StaticWriteOnly             = BufferUsage.StaticWriteOnly,
        DynamicWriteOnly            = BufferUsage.DynamicWriteOnly,
        DynamicWriteOnlyDiscardable = BufferUsage.DynamicWriteOnlyDiscardable,
        /// <summary>
		///    Mipmaps will be automatically generated for this texture
        /// </summary>
        AutoMipMap = 0x100,
		/// <summary>
		///    This texture will be a render target, ie. used as a target for render to texture
		///    setting this flag will ignore all other texture usages except AutoMipMap
		///	</summary>
        RenderTarget = 0x200,
		/// <summary>
		///    Default to automatic mipmap generation static textures
		///	</summary>
        Default = AutoMipMap | StaticWriteOnly
    }

	/// <summary>
	///		Types for definings what information a vertex will hold.
	/// </summary>
	/// <remarks>
	///		Implemented with the Flags attribute to treat this enum with bitwise addition
	///		and comparisons.
	/// </remarks>
	[Flags]
	public enum VertexFlags {
		/// <summary>
		///		Specifies the 3D coordinates of the vertex.
		///	 </summary>
		Position = 1,
		/// <summary>
		///		When applying 1 or more world matrices to a vertex, the weight values of a vertex dictate how much
		///		of an effect each matrix has in determining its final position.  
		/// </summary>
		BlendWeights = 2,
		/// <summary>
		///		Normal vector, determines the logical direction the vertex is facing for use in
		///		lighting calculations.
		///	 </summary>
		Normals = 4,
		/// <summary>
		///		Texture coordinate for the vertex.
		///	 </summary>
		TextureCoords = 8,
		/// <summary>
		///		The primary color of the vertex.
		/// </summary>
		Diffuse = 16,
		/// <summary>
		///		Specular color for this vertex.
		///	 </summary>
		Specular = 32
	}

	/// <summary>
	///     Vertex element semantics, used to identify the meaning of vertex buffer contents.
	/// </summary>
	public enum VertexElementSemantic {
		/// <summary>
		///     Position, 3 reals per vertex.
		/// </summary>
		Position = 1,
		/// <summary>
		///     Blending weights.
		/// </summary>
		BlendWeights = 2,
		/// <summary>
		///     Blending indices.
		/// </summary>
		BlendIndices = 3,
        /// <summary>
        ///     Normal, 3 reals per vertex.
        /// </summary>
        Normal = 4,
        /// <summary>
		///     Diffuse colors.
		/// </summary>
		Diffuse = 5,
		/// <summary>
		///     Specular colors.
		/// </summary>
		Specular = 6,
		/// <summary>
		///     Texture coordinates.
		/// </summary>
		TexCoords = 7,
		/// <summary>
		///     Binormal (Y axis if normal is Z).
		/// </summary>
		Binormal = 8,
		/// <summary>
		///     Tangent (X axis if normal is Z).
		/// </summary>
		Tangent = 9
	}

	/// <summary>
	///     Vertex element type, used to identify the base types of the vertex contents.
	/// </summary>
	public enum VertexElementType {
		Float1,
		Float2,
		Float3,
		Float4,
		Color,
		Short1,
		Short2,
		Short3,
		Short4,
		UByte4
	}

    /// <summary>
    ///     The types of compositing passes
    /// </summary>
    public enum CompositorPassType {
        Clear,           // Clear target to one colour
        Stencil,		 // Set stencil operation
        RenderScene,     // Render the scene or part of it
        RenderQuad       // Render a full screen quad
    }

    /// <summary>
    ///     Input mode of a TargetPass
    /// </summary>
    public enum CompositorInputMode {
        None,            // No input
        Previous         // Output of previous Composition in chain
    }
}
