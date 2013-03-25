using System;
using Axiom.Core;

namespace Axiom.Graphics {
	/// <summary>
	///		Static class containing source for vertex programs for extruding shadow volumes.
	/// </summary>
	/// <remarks>
	///		This exists so we don't have to be dependent on an external media files.
	///		 is used so we don't have to rely on particular plugins.
	///		 assembler contents of this file were generated from the following Cg:
	///		 
	///		 <code>
	///		 // Point light shadow volume extrude
	///        void shadowVolumeExtrudePointLight_vp (
	///            float4 position			: POSITION,
	///            float  wcoord			: TEXCOORD0,
	///
	///            out float4 oPosition	: POSITION,
	///
	///            uniform float4x4 worldViewProjMatrix,
	///            uniform float4   lightPos // homogenous, object space
	///            )
	///        {
	///            // extrusion in object space
	///            // vertex unmodified if w==1, extruded if w==0
	///            float4 newpos = 
	///                (wcoord.xxxx * lightPos) + 
	///                float4(position.xyz - lightPos.xyz, 0);
	///
	///            oPosition = mul(worldViewProjMatrix, newpos);
	///
	///        }
	///
	///       // Directional light extrude
	///        void shadowVolumeExtrudeDirLight_vp (
	///           float4 position			: POSITION,
	///           float  wcoord			: TEXCOORD0,
	///
	///          out float4 oPosition	: POSITION,
	///
	///           uniform float4x4 worldViewProjMatrix,
	///         uniform float4   lightPos // homogenous, object space
	///           )
	///       {
	///           // extrusion in object space
	///           // vertex unmodified if w==1, extruded if w==0
	///            float4 newpos = 
	///               (wcoord.xxxx * (position + lightPos)) - lightPos;
	///
	///          oPosition = mul(worldViewProjMatrix, newpos);
	///
	///       }
	///        // Point light shadow volume extrude - FINITE
	///        void shadowVolumeExtrudePointLightFinite_vp (
	///            float4 position			: POSITION,
	///           float  wcoord			: TEXCOORD0,
	///
	///          out float4 oPosition	: POSITION,
	///
	///           uniform float4x4 worldViewProjMatrix,
	///          uniform float4   lightPos, // homogenous, object space
	///			uniform float    extrusionDistance // how far to extrude
	///          )
	///       {
	///            // extrusion in object space
	///           // vertex unmodified if w==1, extruded if w==0
	///			float3 extrusionDir = position.xyz - lightPos.xyz;
	///			extrusionDir = normalize(extrusionDir);
	///			
	///           float4 newpos = float4(position.xyz +  
	///                ((1 - wcoord.x) * extrusionDistance * extrusionDir), 1);
	///
	///           oPosition = mul(worldViewProjMatrix, newpos);
	///
	///        }
	///
	///      // Directional light extrude - FINITE
	///       void shadowVolumeExtrudeDirLightFinite_vp (
	///            float4 position			: POSITION,
	///            float  wcoord			: TEXCOORD0,
	///
	///            out float4 oPosition	: POSITION,
	///
	///            uniform float4x4 worldViewProjMatrix,
	///            uniform float4   lightPos, // homogenous, object space
	///			uniform float    extrusionDistance // how far to extrude
	///            )
	///       {
	///            // extrusion in object space
	///            // vertex unmodified if w==1, extruded if w==0
	///			// -ve lightPos is direction
	///            float4 newpos = float4(position.xyz - 
	///                (wcoord.x * extrusionDistance * lightPos.xyz), 1);
	///
	///            oPosition = mul(worldViewProjMatrix, newpos);
	///
	///        }		
	///		 </code>
	/// </remarks>
	public sealed class ShadowVolumeExtrudeProgram {
		#region Constructor

		/// <summary>
		///		This is a static class; don't allow instantiation.
		/// </summary>
		private ShadowVolumeExtrudeProgram() {}

		#endregion Constructor

		#region Program Definitions

		// c4 is the light position/direction in these
		private const string pointArbvp1 = 
			"!!ARBvp1.0\n" +
			"PARAM c5 = { 0, 0, 0, 0 };\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c4 = program.local[4];\n" +
			"ADD R0.xyz, v16.xyzx, -c4.xyzx;\n" +
			"MOV R0.w, c5.x;\n" +
			"MAD R0, v24.x, c4, R0;\n" +
			"DP4 result.position.x, c0[0], R0;\n" +
			"DP4 result.position.y, c0[1], R0;\n" +
			"DP4 result.position.z, c0[2], R0;\n" +
			"DP4 result.position.w, c0[3], R0;\n" +
			"END\n";

		private const string pointVs_1_1 = 
			"vs_1_1\n" +
			"def c5, 0, 0, 0, 0\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"add r0.xyz, v0.xyz, -c4.xyz\n" +
			"mov r0.w, c5.x\n" +
			"mad r0, v7.x, c4, r0\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n";

		private const string dirArbvp1 = 
			"!!ARBvp1.0\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c4 = program.local[4];\n" +
			"ADD R0, v16, c4;\n" +
			"MAD R0, v24.x, R0, -c4;\n" +
			"DP4 result.position.x, c0[0], R0;\n" +
			"DP4 result.position.y, c0[1], R0;\n" +
			"DP4 result.position.z, c0[2], R0;\n" +
			"DP4 result.position.w, c0[3], R0;\n" +
			"END\n";

		private const string dirVs_1_1 = 
			"vs_1_1\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"add r0, v0, c4\n" +
			"mad r0, v7.x, r0, -c4\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n";

		private const string pointArbvp1Debug = 
			"!!ARBvp1.0\n" +
			"PARAM c5 = { 0, 0, 0, 0 };\n" +
			"PARAM c6 = { 1, 1, 1, 1 };\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c4 = program.local[4];\n" +
			"ADD R0.xyz, v16.xyzx, -c4.xyzx;\n" +
			"MOV R0.w, c5.x;\n" +
			"MAD R0, v24.x, c4, R0;\n" +
			"DP4 result.position.x, c0[0], R0;\n" +
			"DP4 result.position.y, c0[1], R0;\n" +
			"DP4 result.position.z, c0[2], R0;\n" +
			"DP4 result.position.w, c0[3], R0;\n" +
			"MOV result.color.front.primary, c6.x;\n" +
			"END\n";

		private const string pointVs_1_1Debug = 
			"vs_1_1\n" +
			"def c5, 0, 0, 0, 0\n" +
			"def c6, 1, 1, 1, 1\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"add r0.xyz, v0.xyz, -c4.xyz\n" +
			"mov r0.w, c5.x\n" +
			"mad r0, v7.x, c4, r0\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n" +
			"mov oD0, c6.x\n";

		private const string dirArbvp1Debug = 
			"!!ARBvp1.0\n" +
			"PARAM c5 = { 1, 1, 1, 1};\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c4 = program.local[4];\n" +
			"ADD R0, v16, c4;\n" +
			"MAD R0, v24.x, R0, -c4;\n" +
			"DP4 result.position.x, c0[0], R0;\n" +
			"DP4 result.position.y, c0[1], R0;\n" +
			"DP4 result.position.z, c0[2], R0;\n" +
			"DP4 result.position.w, c0[3], R0;\n" +
			"MOV result.color.front.primary, c5.x;" +
			"END\n";

		private const string dirVs_1_1Debug = 
			"vs_1_1\n" +
			"def c5, 1, 1, 1, 1\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"add r0, v0, c4\n" +
			"mad r0, v7.x, r0, -c4\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n" +
			"mov oD0, c5.x\n";


		// c4 is the light position/direction in these
		// c5 is extrusion distance
		private const string pointArbvp1Finite = 
			"!!ARBvp1.0\n"  +
			"PARAM c6 = { 1, 0, 0, 0 };\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c5 = program.local[5];\n" +
			"PARAM c4 = program.local[4];\n" +
			"ADD R0.x, c6.x, -v24.x;\n" +
			"MUL R0.w, R0.x, c5.x;\n" +
			"ADD R0.xyz, v16.xyzx, -c4.xyzx;\n" +
			"MAD R0.xyz, R0.w, R0.xyzx, v16.xyzx;\n" +
			"DPH result.position.x, R0.xyzz, c0[0];\n" +
			"DPH result.position.y, R0.xyzz, c0[1];\n" +
			"DPH result.position.z, R0.xyzz, c0[2];\n" +
			"DPH result.position.w, R0.xyzz, c0[3];\n" +
			"END\n";

		private const string pointVs_1_1Finite = 
			"vs_1_1\n" +
			"def c6, 1, 0, 0, 0\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"add r0.x, c6.x, -v7.x\n" +
			"mul r1.x, r0.x, c5.x\n" +
			"add r0.yzw, v0.xxyz, -c4.xxyz\n" +
			"dp3 r0.x, r0.yzw, r0.yzw\n" +
			"rsq r0.x, r0.x\n" +
			"mul r0.xyz, r0.x, r0.yzw\n" +
			"mad r0.xyz, r1.x, r0.xyz, v0.xyz\n" +
			"mov r0.w, c6.x\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n";

		private const string dirArbvp1Finite = 
			"!!ARBvp1.0\n" +
			"PARAM c6 = { 1, 0, 0, 0 };\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c4 = program.local[4];\n" +
			"PARAM c5 = program.local[5];\n" +
			"ADD R0.x, c6.x, -v24.x;\n" +
			"MUL R0.x, R0.x, c5.x;\n" +
			"MAD R0.xyz, -R0.x, c4.xyzx, v16.xyzx;\n" +
			"DPH result.position.x, R0.xyzz, c0[0];\n" +
			"DPH result.position.y, R0.xyzz, c0[1];\n" +
			"DPH result.position.z, R0.xyzz, c0[2];\n" +
			"DPH result.position.w, R0.xyzz, c0[3];\n" +
			"END\n";

		private const string dirVs_1_1Finite = 
			"vs_1_1\n" +
			"def c6, 1, 0, 0, 0\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"add r0.x, c6.x, -v7.x\n" +
			"mul r0.x, r0.x, c5.x\n" +
			"mad r0.xyz, -r0.x, c4.xyz, v0.xyz\n" +
			"mov r0.w, c6.x\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n";

		private const string pointArbvp1FiniteDebug = 
			"!!ARBvp1.0\n" +
			"PARAM c6 = { 1, 0, 0, 0 };\n" +
			"TEMP R0, R1;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c5 = program.local[5];\n" +
			"PARAM c4 = program.local[4];\n" +
			"MOV result.color.front.primary, c6.x;\n" +
			"ADD R0.x, c6.x, -v24.x;\n" +
			"MUL R1.x, R0.x, c5.x;\n" +
			"ADD R0.yzw, v16.xxyz, -c4.xxyz;\n" +
			"DP3 R0.x, R0.yzwy, R0.yzwy;\n" +
			"RSQ R0.x, R0.x;\n" +
			"MUL R0.xyz, R0.x, R0.yzwy;\n" +
			"MAD R0.xyz, R1.x, R0.xyzx, v16.xyzx;\n" +
			"DPH result.position.x, R0.xyzz, c0[0];\n" +
			"DPH result.position.y, R0.xyzz, c0[1];\n" +
			"DPH result.position.z, R0.xyzz, c0[2];\n" +
			"DPH result.position.w, R0.xyzz, c0[3];\n" +
			"END\n";

		private const string pointVs_1_1FiniteDebug = 
			"vs_1_1\n" +
			"def c6, 1, 0, 0, 0\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"mov oD0, c6.x\n" +
			"add r0.x, c6.x, -v7.x\n" +
			"mul r1.x, r0.x, c5.x\n" +
			"add r0.yzw, v0.xxyz, -c4.xxyz\n" +
			"dp3 r0.x, r0.yzw, r0.yzw\n" +
			"rsq r0.x, r0.x\n" +
			"mul r0.xyz, r0.x, r0.yzw\n" +
			"mad r0.xyz, r1.x, r0.xyz, v0.xyz\n" +
			"mov r0.w, c6.x\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n";

		private const string dirArbvp1FiniteDebug = 
			"!!ARBvp1.0\n" +
			"PARAM c6 = { 1, 0, 0, 0 };\n" +
			"TEMP R0;\n" +
			"ATTRIB v24 = vertex.texcoord[0];\n" +
			"ATTRIB v16 = vertex.position;\n" +
			"PARAM c0[4] = { program.local[0..3] };\n" +
			"PARAM c4 = program.local[4];\n" +
			"PARAM c5 = program.local[5];\n" +
			"MOV result.color.front.primary, c6.x;\n" +
			"ADD R0.x, c6.x, -v24.x;\n" +
			"MUL R0.x, R0.x, c5.x;\n" +
			"MAD R0.xyz, -R0.x, c4.xyzx, v16.xyzx;\n" +
			"DPH result.position.x, R0.xyzz, c0[0];\n" +
			"DPH result.position.y, R0.xyzz, c0[1];\n" +
			"DPH result.position.z, R0.xyzz, c0[2];\n" +
			"DPH result.position.w, R0.xyzz, c0[3];\n" +
			"END\n";

		private const string dirVs_1_1FiniteDebug = 
			"vs_1_1\n" +
			"def c6, 1, 0, 0, 0\n" +
			"dcl_texcoord0 v7\n" +
			"dcl_position v0\n" +
			"mov oD0, c6.x\n" +
			"add r0.x, c6.x, -v7.x\n" +
			"mul r0.x, r0.x, c5.x\n" +
			"mad r0.xyz, -r0.x, c4.xyz, v0.xyz\n" +
			"mov r0.w, c6.x\n" +
			"dp4 oPos.x, c0, r0\n" +
			"dp4 oPos.y, c1, r0\n" +
			"dp4 oPos.z, c2, r0\n" +
			"dp4 oPos.w, c3, r0\n";

		private const int NumShadowExtruderPrograms = 8;

		/// <summary>
		///		Have the hardware extrusion programs been initialized yet?
		/// </summary>
		private static bool isInitialized = false;

		public static string[] programNames = new string[]{
															   "Ogre/ShadowExtrudePointLight",
															   "Ogre/ShadowExtrudePointLightDebug",
															   "Ogre/ShadowExtrudeDirLight",
															   "Ogre/ShadowExtrudeDirLightDebug",
															   "Ogre/ShadowExtrudePointLightFinite",
															   "Ogre/ShadowExtrudePointLightFiniteDebug",
															   "Ogre/ShadowExtrudeDirLightFinite",
															   "Ogre/ShadowExtrudeDirLightFiniteDebug"
														   };

		/// <summary>
		///		Contains the possible hardware extrusion programs.
		/// </summary>
		public enum Programs {
			/// <summary>
			///		Point light extruder, infinite distance.
			/// </summary>
			PointLight = 0,
			/// <summary>
			///		Point light extruder, infinite distance, debug mode.
			/// </summary>
			PointLightDebug = 1,
			/// <summary>
			///		Directional light extruder, infinite distance.
			/// </summary>
			DirectionalLight = 2,
			/// <summary>
			///		Directional light extruder, infinite distance, debug mode.
			/// </summary>
			DirectionalLightDebug = 3,
			/// <summary>
			///		Point light extruder, finite distance.
			/// </summary>
			PointLightFinite = 4,
			/// <summary>
			///		Point light extruder, finite distance, debug mode.
			/// </summary>
			PointLightFiniteDebug = 5,
			/// <summary>
			///		Directional light extruder, finite distance.
			/// </summary>
			DirectionalLightFinite = 6,
			/// <summary>
			///		Directional light extruder, finite distance, debug mode.
			/// </summary>
			DirectionalLightFiniteDebug = 7
		}

		#endregion Program Definitions

		#region Methods

		/// <summary>
		///		General purpose method to get any of the program sources.
		/// </summary>
		/// <param name="lightType">Type of light to get the source for.</param>
		/// <param name="syntax">Syntax code of interest.</param>
		/// <param name="finite">Is this for finite volume extrusion?</param>
		/// <param name="debug">Should the shadow volumes be visible?</param>
		/// <returns>Source of the specified program.</returns>
		public static string GetProgramSource(LightType lightType, string syntax, bool finite, bool debug) {
			if (lightType == LightType.Directional) {
				if (syntax == "arbvp1") {
					if (finite) {
						if (debug) {
							return dirArbvp1FiniteDebug;
						}
						else {
							return dirArbvp1Finite;
						}
					}
					else {
						if (debug) {
							return dirArbvp1Debug;
						}
						else {
							return dirArbvp1;
						}
					}
				}
				else {
					if (finite) {
						if (debug) {
							return dirVs_1_1FiniteDebug;
						}
						else {
							return dirVs_1_1Finite;
						}
					}
					else {
						if (debug) {
							return dirVs_1_1Debug;
						}
						else {
							return dirVs_1_1;
						}
					}
				}
			}
			else {
				if (syntax == "arbvp1") {
					if (finite) {
						if (debug) {
							return pointArbvp1FiniteDebug;
						}
						else {
							return pointArbvp1Finite;
						}
					}
					else {
						if (debug) {
							return pointArbvp1Debug;
						}
						else {
							return pointArbvp1;
						}
					}
				}
				else {
					if (finite) {
						if (debug) {
							return pointVs_1_1FiniteDebug;
						}
						else {
							return pointVs_1_1Finite;
						}
					}
					else {
						if (debug) {
							return pointVs_1_1Debug;
						}
						else {
							return pointVs_1_1;
						}
					}
				}
			}
		}

		/// <summary>
		///		Gets the name of the program for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetProgramName(Programs type) {
			int index = (int)type;

			if(index < programNames.Length) {
				return programNames[index];
			}

			return string.Empty;
		}

		/// <summary>
		///		General purpose method to get any of the program names based on the specified parameters.
		/// </summary>
		/// <param name="lightType">Type of light to get the source for.</param>
		/// <param name="finite">Is this for finite volume extrusion?</param>
		/// <param name="debug">Should the shadow volumes be visible?</param>
		/// <returns>Name of the program matching the specified parameters.</returns>
		public static string GetProgramName(LightType lightType, bool finite, bool debug) {
			if (lightType == LightType.Directional) {
				if (finite) {
					if (debug) {
						return programNames[(int)Programs.DirectionalLightFiniteDebug];
					}
					else {
						return programNames[(int)Programs.DirectionalLightFinite];
					}
				}
				else {
					if (debug) {
						return programNames[(int)Programs.DirectionalLightDebug];
					}
					else {
						return programNames[(int)Programs.DirectionalLight];
					}
				}
			}
			else {
				if (finite) {
					if (debug) {
						return programNames[(int)Programs.PointLightFiniteDebug];
					}
					else {
						return programNames[(int)Programs.PointLightFinite];
					}
				}
				else {
					if (debug) {
						return programNames[(int)Programs.PointLightDebug];
					}
					else {
						return programNames[(int)Programs.PointLight];
					}
				}
			}
		}

		/// <summary>
		///		Initialize the creation of these core vertex programs.
		/// </summary>
		public static void Initialize() {
			// only need to initialize once
			if(!isInitialized) {
				string syntax = "";

				// flags for which of the programs use finite extrusion
				bool[] vertexProgramFinite = 
					new bool[] { false, false, false, false, true, true, true, true };

				// flags for which of the programs use debug rendering
				bool[] vertexProgramDebug = 
					new bool[] { false, true, false, true, false, true, false, true };

				// types of lights that each of the programs target
				LightType[] vertexProgramLightTypes = 
					new LightType[] { 
							LightType.Point, LightType.Point, 
							LightType.Directional, LightType.Directional,
							LightType.Point, LightType.Point,
							LightType.Directional, LightType.Directional
					};

				// load hardware extrusion programs for point & dir lights
				if(GpuProgramManager.Instance.IsSyntaxSupported("arbvp1")) {
					syntax = "arbvp1";
				}
				else if(GpuProgramManager.Instance.IsSyntaxSupported("vs_1_1")) {
					syntax = "vs_1_1";
				}
				else {
					throw new AxiomException("Vertex programs are supposedly supported, but neither arbvp1 nor vs_1_1 syntaxes are supported.");
				}

				// create the programs
				for(int i = 0; i < programNames.Length; i++) {
					// sanity check to make sure it doesn't already exist
					if(GpuProgramManager.Instance.GetByName(programNames[i]) == null) {
						string source = ShadowVolumeExtrudeProgram.GetProgramSource(
							vertexProgramLightTypes[i], syntax, vertexProgramFinite[i], vertexProgramDebug[i]);

						// create the program from the static source
						GpuProgram program = 
							GpuProgramManager.Instance.CreateProgramFromString(
								programNames[i], source, GpuProgramType.Vertex, syntax);

						// load the program
						program.Load();
					}
				}

				isInitialized = true;
			}
		}

        /// <summary>
        ///     Called on engine shutdown; destroys all auto created gpu programs.
        /// </summary>
        public static void Shutdown() {
            if (isInitialized) {
                // destroy shadow volume extruders
                foreach (string programName in programNames) {
                    // TODO: Toast the programs
                    //GpuProgramManager.Instance.Remove(programName);
                }

                isInitialized = false;
            }
        }

		#endregion Methods
	}
}
