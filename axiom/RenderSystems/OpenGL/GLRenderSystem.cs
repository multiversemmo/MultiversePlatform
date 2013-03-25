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
using System.Diagnostics;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.Media;
using Tao.OpenGl;

// TODO: Cache property values and implement property getters

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	/// Summary description for OpenGLRenderer.
	/// </summary>
	public class GLRenderSystem : RenderSystem {
		#region Fields

		/// <summary>
		///		GLSupport class providing platform specific implementation.
		/// </summary>
		private BaseGLSupport glSupport;

		/// <summary>
		///		Flag that remembers if GL has been initialized yet.
		/// </summary>
		private bool isGLInitialized;

		/// <summary>Internal view matrix.</summary>
		protected Matrix4 viewMatrix;
		/// <summary>Internal world matrix.</summary>
		protected Matrix4 worldMatrix;
		/// <summary>Internal texture matrix.</summary>
		protected Matrix4 textureMatrix;

		// used for manual texture matrix calculations, for things like env mapping
		protected bool useAutoTextureMatrix;
		protected float[] autoTextureMatrix = new float[16];
		protected int[] texCoordIndex = new int[Config.MaxTextureLayers];

		// keeps track of type for each stage (2d, 3d, cube, etc)
		protected int[] textureTypes = new int[Config.MaxTextureLayers];

		// retained stencil buffer params vals, since we allow setting invidual params but GL
		// only lets you set them all at once, keep old values around to allow this to work
		protected int stencilFail, stencilZFail, stencilPass, stencilFunc, stencilRef, stencilMask;

		protected bool zTrickEven;      

		/// <summary>
		///    Last min filtering option.
		/// </summary>
		protected FilterOptions minFilter;
		/// <summary>
		///    Last mip filtering option.
		/// </summary>
		protected FilterOptions mipFilter;

		// render state redundency reduction settings
		protected SceneDetailLevel lastRasterizationMode;
		protected ColorEx lastDiffuse, lastAmbient, lastSpecular, lastEmissive;
		protected float lastShininess;
		protected TexCoordCalcMethod[] lastTexCalMethods = new TexCoordCalcMethod[Config.MaxTextureLayers];
		protected bool fogEnabled;
		protected bool lightingEnabled;
		protected SceneBlendFactor lastBlendSrc, lastBlendDest;
		protected LayerBlendOperationEx[] lastColorOp = new LayerBlendOperationEx[Config.MaxTextureLayers];
		protected LayerBlendOperationEx[] lastAlphaOp = new LayerBlendOperationEx[Config.MaxTextureLayers];
		protected LayerBlendType lastBlendType;
		protected TextureAddressing[] lastAddressingMode = new TextureAddressing[Config.MaxTextureLayers];
		protected float lastDepthBias;
		protected bool lastDepthCheck, lastDepthWrite;
		protected CompareFunction lastDepthFunc;

		const int MAX_LIGHTS = 8;
		protected Light[] lights = new Light[MAX_LIGHTS];
        
		// temp arrays to reduce runtime allocations
		protected float[] tempMatrix = new float[16];
		protected float[] tempColorVals = new float[4];
		protected float[] tempLightVals = new float[4];
		protected float[] tempProgramFloats = new float[4];
		protected int[] colorWrite = new int[4];

		protected GLGpuProgramManager gpuProgramMgr;
		protected GLGpuProgram currentVertexProgram;
		protected GLGpuProgram currentFragmentProgram;

		// constants for gl vertex attributes
		const int BLEND_INDICES = 7;
		const int BLEND_WEIGHTS = 1;

		#endregion Fields

		#region Constructors

		/// <summary>
		///		Default constructor.
		/// </summary>
		public GLRenderSystem() {
			viewMatrix = Matrix4.Identity;
			worldMatrix = Matrix4.Identity;
			textureMatrix = Matrix4.Identity;

			// init the stored stencil buffer params
			stencilFail = stencilZFail = stencilPass = Gl.GL_KEEP;
			stencilFunc = Gl.GL_ALWAYS;
			stencilRef = 0;
			stencilMask = unchecked((int)0xffffffff);

			colorWrite[0] = colorWrite[1] = colorWrite[2] = colorWrite[3] = 1;

			minFilter = FilterOptions.Linear;
			mipFilter = FilterOptions.Point;

			// create 
			glSupport = new GLSupport();

			InitConfigOptions();
		}

		#endregion Constructors

		#region Implementation of RenderSystem

		public override EngineConfig ConfigOptions {
			get {
				return glSupport.ConfigOptions;
			}
		}

		public override void ClearFrameBuffer(FrameBuffer buffers, ColorEx color, float depth, int stencil) {
			int flags = 0;

			if ((buffers & FrameBuffer.Color) > 0) {
				flags |= Gl.GL_COLOR_BUFFER_BIT;
			}
			if ((buffers & FrameBuffer.Depth) > 0) {
				flags |= Gl.GL_DEPTH_BUFFER_BIT;
			}
			if ((buffers & FrameBuffer.Stencil) > 0) {
				flags |= Gl.GL_STENCIL_BUFFER_BIT;
			}

			// Enable depth & color buffer for writing if it isn't

			if (!depthWrite) {
				Gl.glDepthMask(Gl.GL_TRUE);
			}

			bool colorMask = 
				colorWrite[0] == 0 
				|| colorWrite[1] == 0 
				|| colorWrite[2] == 0 
				|| colorWrite[3] == 0; 

			if (colorMask) {
				Gl.glColorMask(Gl.GL_TRUE, Gl.GL_TRUE, Gl.GL_TRUE, Gl.GL_TRUE);
			}

			// Set values
			Gl.glClearColor(color.r, color.g, color.b, color.a);
			Gl.glClearDepth(depth);
			Gl.glClearStencil(stencil);

			// Clear buffers
			Gl.glClear(flags);

			// Reset depth write state if appropriate
			// Enable depth buffer for writing if it isn't
			if (!depthWrite) {
				Gl.glDepthMask(Gl.GL_FALSE);
			}

			if (colorMask) {
				Gl.glColorMask(colorWrite[0], colorWrite[1], colorWrite[2], colorWrite[3]);
			}
		}


		public override RenderTexture CreateRenderTexture(string name, int width, int height, PixelFormat format) {
			GLRenderTexture renderTexture = new GLRenderTexture(name, width, height, format);
			AttachRenderTarget(renderTexture);
			return renderTexture;
		}

		/// <summary>
		///		Returns an OpenGL implementation of a hardware occlusion query.
		/// </summary>
		/// <returns></returns>
		public override IHardwareOcclusionQuery CreateHardwareOcclusionQuery() {
			return new GLHardwareOcclusionQuery();
		}

		public override RenderWindow CreateRenderWindow(string name, int width, int height, int colorDepth,
			bool isFullscreen, int left, int top, bool depthBuffer, bool vsync, object target) {

			// TODO: Check for dupe windows

			RenderWindow window = glSupport.NewWindow(name, width, height, colorDepth, isFullscreen, left, top, depthBuffer, vsync, target);

			if(!isGLInitialized) {
				InitGL();
			}

			// add the new render target
			AttachRenderTarget(window);

			return window;
		}

		protected void InitGL() {
			// intialize GL extensions and check capabilites
			glSupport.InitializeExtensions();

			// log hardware info
            LogManager.Instance.Write("Vendor: {0}", glSupport.Vendor);
            LogManager.Instance.Write("Video Board: {0}", glSupport.VideoCard);
            LogManager.Instance.Write("Version: {0}", glSupport.Version);

            LogManager.Instance.Write("Extensions supported:");

            foreach(string ext in glSupport.Extensions) {
                LogManager.Instance.Write(ext);
            }
			
			// create our special program manager
			gpuProgramMgr = new GLGpuProgramManager();

			// query hardware capabilites
			CheckCaps();

			// create a specialized instance, which registers itself as the singleton instance of HardwareBufferManager
			// use software buffers as a fallback, which operate as regular vertex arrays
			if(caps.CheckCap(Capabilities.VertexBuffer)) {
				hardwareBufferManager = new GLHardwareBufferManager();
			}
			else {
				hardwareBufferManager = new GLSoftwareBufferManager();
			}

			// by creating our texture manager, singleton TextureManager will hold our implementation
			textureMgr = new GLTextureManager();

			isGLInitialized = true;
		}

		public override ColorEx AmbientLight {
			get {
				throw new NotImplementedException();
			}
			set {
				// create a float[4]  to contain the RGBA data
				value.ToArrayRGBA(tempColorVals);
				tempColorVals[3] = 1.0f;

				// set the ambient color
				Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, tempColorVals);
			}
		}

		/// <summary>
		///		Gets/Sets the global lighting setting.
		/// </summary>
		public override bool LightingEnabled {
			get {
				throw new NotImplementedException();
			}
			set {
				if(lightingEnabled == value)
					return;

				if(value)
					Gl.glEnable(Gl.GL_LIGHTING);
				else
					Gl.glDisable(Gl.GL_LIGHTING);

				lightingEnabled = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool NormalizeNormals {
			get {
				throw new NotImplementedException();
			}
			set {
				if(value) {
					Gl.glEnable(Gl.GL_NORMALIZE);
				}
				else {
					Gl.glDisable(Gl.GL_NORMALIZE);
				}
			}
		}

		/// <summary>
		///		Sets the mode to use for rendering
		/// </summary>
		public override SceneDetailLevel RasterizationMode {
			get {
				throw new NotImplementedException();
			}
			set {
				if(value == lastRasterizationMode) {
					return;
				}

				// default to fill to make compiler happy
				int mode = Gl.GL_FILL;

				switch(value) {
					case SceneDetailLevel.Solid:
						mode = Gl.GL_FILL;
						break;
					case SceneDetailLevel.Points:
						mode = Gl.GL_POINT;
						break;
					case SceneDetailLevel.Wireframe:
						mode = Gl.GL_LINE;
						break;
					default:
						// if all else fails, just use fill
						mode = Gl.GL_FILL;

						// deactivate viewport clipping
						Gl.glDisable(Gl.GL_SCISSOR_TEST);

						break;
				}

				// set the specified polygon mode
				Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, mode);

				lastRasterizationMode = value;
			}
		}

		public override Shading ShadingMode {
			get {
				throw new NotImplementedException();
			}
			// OpenGL supports Flat and Smooth shaded primitives
			set {
				switch(value) {
					case Shading.Flat:
						Gl.glShadeModel(Gl.GL_FLAT);
						break;
					default:
						Gl.glShadeModel(Gl.GL_SMOOTH);
						break;
				}
			}
		}

		/// <summary>
		///		Specifies whether stencil check should be enabled or not.
		/// </summary>
		public override bool StencilCheckEnabled {
			get {
				throw new NotImplementedException();
			}
			set {
				if(value) {
					Gl.glEnable(Gl.GL_STENCIL_TEST);
				}
				else {
					Gl.glDisable(Gl.GL_STENCIL_TEST);
				}
			}
		}

		public override Matrix4 MakeOrthoMatrix(float fov, float aspectRatio, float near, float far, bool forGpuPrograms) {
			float thetaY = MathUtil.DegreesToRadians(fov / 2.0f);
			float tanThetaY = MathUtil.Tan(thetaY);
			float tanThetaX = tanThetaY * aspectRatio;

			float halfW = tanThetaX * near;
			float halfH = tanThetaY * near;

			float w = 1.0f / halfW;
			float h = 1.0f / halfH;
			float q = 0;

			if(far != 0) {
				q = 2.0f / (far - near);
			}
		
			Matrix4 dest = Matrix4.Zero;
			dest.m00 = w;
			dest.m11 = h;
			dest.m22 = -q;
			dest.m23 = -(far + near) / (far - near);
			dest.m33 = 1.0f;

			return dest;
		}


		/// <summary>
		///		Creates a projection matrix specific to OpenGL based on the given params.
		///		Note: forGpuProgram is ignored because GL uses the same handed projection matrix
		///		normally and for GPU programs.
		/// </summary>
		/// <param name="fov"></param>
		/// <param name="aspectRatio"></param>
		/// <param name="near"></param>
		/// <param name="far"></param>
		/// <returns></returns>
		public override Axiom.MathLib.Matrix4 MakeProjectionMatrix(float fov, float aspectRatio, float near, float far, bool forGpuProgram) {
			Matrix4 matrix = new Matrix4();

			float thetaY = MathUtil.DegreesToRadians(fov * 0.5f);
			float tanThetaY = MathUtil.Tan(thetaY);

			float w = (1.0f / tanThetaY) / aspectRatio;
			float h = 1.0f / tanThetaY;
			float q = 0;
			float qn = 0;

			if(far == 0) {
				q = Frustum.InfiniteFarPlaneAdjust - 1;
				qn = near * (Frustum.InfiniteFarPlaneAdjust - 2);
			}
			else {
				q = -(far + near) / (far - near);
				qn = -2 * (far * near) / (far - near);
			}

			// NB This creates Z in range [-1,1]
			//
			// [ w   0   0   0  ]
			// [ 0   h   0   0  ]
			// [ 0   0   q   qn ]
			// [ 0   0   -1  0  ]

			matrix.m00 = w;
			matrix.m11 = h;
			matrix.m22 = q;
			matrix.m23 = qn;
			matrix.m32 = -1.0f;

			return matrix;
		}

		public override void ApplyObliqueDepthProjection(ref Axiom.MathLib.Matrix4 projMatrix, Axiom.MathLib.Plane plane, bool forGpuProgram) {
			// Thanks to Eric Lenyel for posting this calculation at www.terathon.com

			// Calculate the clip-space corner point opposite the clipping plane
			// as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
			// transform it into camera space by multiplying it
			// by the inverse of the projection matrix

			Vector4 q = new Vector4();
			q.x = (Math.Sign(plane.Normal.x) + projMatrix.m02) / projMatrix.m00;
			q.y = (Math.Sign(plane.Normal.y) + projMatrix.m12) / projMatrix.m11;
			q.z = -1.0f;
			q.w = (1.0f + projMatrix.m22) / projMatrix.m23;

			// Calculate the scaled plane vector
			Vector4 clipPlane4d = new Vector4(plane.Normal.x, plane.Normal.y, plane.Normal.z, plane.D);
			Vector4 c = clipPlane4d * (2.0f / (clipPlane4d.Dot(q)));

			// Replace the third row of the projection matrix
			projMatrix.m20 = c.x;
			projMatrix.m21 = c.y;
			projMatrix.m22 = c.z + 1.0f;
			projMatrix.m23 = c.w;
		}

		/// <summary>
		///		Executes right before each frame is rendered.
		/// </summary>
		public override void BeginFrame() {
			Debug.Assert(activeViewport != null, "BeingFrame cannot run without an active viewport.");

			// clear the viewport if required
			if(activeViewport.ClearEveryFrame) {
				// active viewport clipping
				Gl.glEnable(Gl.GL_SCISSOR_TEST);

				ClearFrameBuffer(FrameBuffer.Color | FrameBuffer.Depth, activeViewport.BackgroundColor);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void EndFrame() {
			// clear stored blend modes, to ensure they gets set properly in multi texturing scenarios
			// overall this will still reduce the number of blend mode changes
			for(int i = 1; i < Config.MaxTextureLayers; i++) {
				lastAlphaOp[i] = 0;
				lastColorOp[i] = 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="viewport"></param>
		public override void SetViewport(Viewport viewport) {
			if(activeViewport != viewport || viewport.IsUpdated) {
				// store this viewport and it's target
				activeViewport = viewport;
				activeRenderTarget = viewport.Target;

				int x, y, width, height;

				// set viewport dimensions
				width = viewport.ActualWidth;
				height = viewport.ActualHeight;
				x = viewport.ActualLeft;
				// make up for the fact that GL's origin starts at the bottom left corner
				y = activeRenderTarget.Height - viewport.ActualTop - height;

				// enable scissor testing (for viewports)
				Gl.glEnable(Gl.GL_SCISSOR_TEST);

				// set the current GL viewport
				Gl.glViewport(x, y, width, height);

				// set the scissor area for the viewport
				Gl.glScissor(x, y, width, height);

				// clear the updated flag
				viewport.IsUpdated = false;
			}
		}

		public override void SetStencilBufferParams(CompareFunction function, int refValue, int mask, StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp, bool twoSidedOperation) {
			if (twoSidedOperation) {
				if(!caps.CheckCap(Capabilities.TwoSidedStencil)) {
					throw new AxiomException("2-sided stencils are not supported on this hardware!");
				}
				
				Gl.glActiveStencilFaceEXT(Gl.GL_FRONT);
			}
        
			Gl.glStencilMask(mask);
			Gl.glStencilFunc(GLHelper.ConvertEnum(function), refValue, mask);
			Gl.glStencilOp(GLHelper.ConvertEnum(stencilFailOp), GLHelper.ConvertEnum(depthFailOp), 
				GLHelper.ConvertEnum(passOp));

			if (twoSidedOperation) {
				// set everything again, inverted
				Gl.glActiveStencilFaceEXT(Gl.GL_BACK);
				Gl.glStencilMask(mask);
				Gl.glStencilFunc(GLHelper.ConvertEnum(function), refValue, mask);
				Gl.glStencilOp(
					GLHelper.ConvertEnum(stencilFailOp, true), 
					GLHelper.ConvertEnum(depthFailOp, true), 
					GLHelper.ConvertEnum(passOp, true));

				// reset
				Gl.glActiveStencilFaceEXT(Gl.GL_FRONT);
				Gl.glEnable(Gl.GL_STENCIL_TEST_TWO_SIDE_EXT);
			}
			else {
				Gl.glDisable(Gl.GL_STENCIL_TEST_TWO_SIDE_EXT);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ambient"></param>
		/// <param name="diffuse"></param>
		/// <param name="specular"></param>
		/// <param name="emissive"></param>
		/// <param name="shininess"></param>
		public override void SetSurfaceParams(ColorEx ambient, ColorEx diffuse, ColorEx specular, ColorEx emissive, float shininess) {
			float[] vals = tempColorVals;
            
			// ambient
			//if(lastAmbient == null || lastAmbient.CompareTo(ambient) != 0) {
			ambient.ToArrayRGBA(vals);
			Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT, vals);
                
			lastAmbient = ambient;
			//}

			// diffuse
			//if(lastDiffuse == null || lastDiffuse.CompareTo(diffuse) != 0) {
			vals[0] = diffuse.r; vals[1] = diffuse.g; vals[2] = diffuse.b; vals[3] = diffuse.a;
			Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_DIFFUSE, vals);

			lastDiffuse = diffuse;
			//}

			// specular
			//if(lastSpecular == null || lastSpecular.CompareTo(specular) != 0) {
			vals[0] = specular.r; vals[1] = specular.g; vals[2] = specular.b; vals[3] = specular.a;
			Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_SPECULAR, vals);

			lastSpecular = specular;
			//}

			// emissive
			//if(lastEmissive == null || lastEmissive.CompareTo(emissive) != 0) {
			vals[0] = emissive.r; vals[1] = emissive.g; vals[2] = emissive.b; vals[3] = emissive.a;
			Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, vals);

			lastEmissive = emissive;
			//}

			// shininess
			//if(lastShininess != shininess) {
			Gl.glMaterialf(Gl.GL_FRONT_AND_BACK, Gl.GL_SHININESS, shininess);

			lastShininess = shininess;
			//}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="texAddressingMode"></param>
		public override void SetTextureAddressingMode(int stage, TextureAddressing texAddressingMode) {
			if(lastAddressingMode[stage] == texAddressingMode) {
				//return;
			}

			lastAddressingMode[stage] = texAddressingMode;

			int type = 0;

			// find out the GL equivalent of out TextureAddressing enum
			switch(texAddressingMode) {
				case TextureAddressing.Wrap:
					type = Gl.GL_REPEAT;
					break;

				case TextureAddressing.Mirror:
					type = Gl.GL_MIRRORED_REPEAT;
					break;

				case TextureAddressing.Clamp:
					type = Gl.GL_CLAMP_TO_EDGE;
					break;
			} // end switch

			// set the GL texture wrap params for the specified unit
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0 + stage);
			Gl.glTexParameteri(textureTypes[stage], Gl.GL_TEXTURE_WRAP_S, type);
			Gl.glTexParameteri(textureTypes[stage], Gl.GL_TEXTURE_WRAP_T, type);
			Gl.glTexParameteri(textureTypes[stage], Gl.GL_TEXTURE_WRAP_R, type);
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="maxAnisotropy"></param>
		public override void SetTextureLayerAnisotropy(int stage, int maxAnisotropy) {
			if(!caps.CheckCap(Capabilities.AnisotropicFiltering)) {
				return;
			}

			// get current setting to compare
			float currentAnisotropy = 1;
			float maxSupportedAnisotropy = 0;

			// TODO: Add getCurrentAnistoropy
			Gl.glGetTexParameterfv(textureTypes[stage], Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, out currentAnisotropy);
			Gl.glGetFloatv(Gl.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out maxSupportedAnisotropy);

			if(maxAnisotropy > maxSupportedAnisotropy) {
				maxAnisotropy = 
					(int)maxSupportedAnisotropy > 0 ? (int)maxSupportedAnisotropy : 1;
			}

			if(currentAnisotropy != maxAnisotropy) {
				Gl.glTexParameterf(textureTypes[stage], Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, (float)maxAnisotropy);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="blendMode"></param>
		public override void SetTextureBlendMode(int stage, LayerBlendModeEx blendMode) {
			if(!caps.CheckCap(Capabilities.TextureBlending)) {
				return;
			}

			LayerBlendOperationEx lastOp;

			if(blendMode.blendType == LayerBlendType.Alpha) {
				lastOp = lastAlphaOp[stage];
			}
			else {
				lastOp = lastColorOp[stage];
			}
            
			// ignore the new blend mode only if the last one for the current texture stage
			// is the same, and if no special texture coord calcs are required
			if( lastOp == blendMode.operation && 
				lastTexCalMethods[stage] == TexCoordCalcMethod.None)  {
				//return;
			}

			// remember last setting
			if(blendMode.blendType == LayerBlendType.Alpha) {
				lastAlphaOp[stage] = blendMode.operation;
			}
			else {
				lastColorOp[stage] = blendMode.operation;
			}

			int src1op, src2op, cmd;

			src1op = src2op = cmd = 0;

			switch(blendMode.source1) {
				case LayerBlendSource.Current:
					src1op = Gl.GL_PREVIOUS;
					break;

				case LayerBlendSource.Texture:
					src1op = Gl.GL_TEXTURE;
					break;
			
				case LayerBlendSource.Manual:
					src1op = Gl.GL_CONSTANT;
					break;

				case LayerBlendSource.Diffuse:
					src1op = Gl.GL_PRIMARY_COLOR;
					break;
			
					// no diffuse or specular equivalent right now
				default:
					src1op = 0;
					break;
			}

			switch(blendMode.source2) {
				case LayerBlendSource.Current:
					src2op = Gl.GL_PREVIOUS;
					break;

				case LayerBlendSource.Texture:
					src2op = Gl.GL_TEXTURE;
					break;
			
				case LayerBlendSource.Manual:
					src2op = Gl.GL_CONSTANT;
					break;

				case LayerBlendSource.Diffuse:
					src2op = Gl.GL_PRIMARY_COLOR;
					break;
			
					// no diffuse or specular equivalent right now
				default:
					src2op = 0;
					break;
			}

			switch (blendMode.operation) {
				case LayerBlendOperationEx.Source1:
					cmd = Gl.GL_REPLACE;
					break;

				case LayerBlendOperationEx.Source2:
					cmd = Gl.GL_REPLACE;
					break;

				case LayerBlendOperationEx.Modulate:
					cmd = Gl.GL_MODULATE;
					break;

				case LayerBlendOperationEx.ModulateX2:
					cmd = Gl.GL_MODULATE;
					break;

				case LayerBlendOperationEx.ModulateX4:
					cmd = Gl.GL_MODULATE;
					break;

				case LayerBlendOperationEx.Add:
					cmd = Gl.GL_ADD;
					break;

				case LayerBlendOperationEx.AddSigned:
					cmd = Gl.GL_ADD_SIGNED;
					break;

				case LayerBlendOperationEx.Subtract:
					cmd = Gl.GL_SUBTRACT;
					break;

				case LayerBlendOperationEx.BlendDiffuseAlpha:
					cmd = Gl.GL_INTERPOLATE;
					break;

				case LayerBlendOperationEx.BlendTextureAlpha:
					cmd = Gl.GL_INTERPOLATE;
					break;

				case LayerBlendOperationEx.BlendCurrentAlpha:
					cmd = Gl.GL_INTERPOLATE;
					break;

				case LayerBlendOperationEx.BlendManual:
					cmd = Gl.GL_INTERPOLATE;
					break;

				case LayerBlendOperationEx.DotProduct:
					// Check for Dot3 support
					cmd = caps.CheckCap(Capabilities.Dot3) ? Gl.GL_DOT3_RGB : Gl.GL_MODULATE;
					break;

				default:
					cmd = 0;
					break;
			} // end switch

			Gl.glActiveTextureARB(Gl.GL_TEXTURE0 + stage);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_COMBINE);

			if (blendMode.blendType == LayerBlendType.Color) {
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB, cmd);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE0_RGB, src1op);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE1_RGB, src2op);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_RGB, Gl.GL_CONSTANT);
			}
			else {
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA, cmd);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE0_ALPHA, src1op);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE1_ALPHA, src2op);
				Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_ALPHA, Gl.GL_CONSTANT);
			}

			// handle blend types first
			switch (blendMode.operation) {
				case LayerBlendOperationEx.BlendDiffuseAlpha:
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_RGB, Gl.GL_PRIMARY_COLOR);
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_ALPHA, Gl.GL_PRIMARY_COLOR);
					break;

				case LayerBlendOperationEx.BlendTextureAlpha:
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_RGB, Gl.GL_TEXTURE);
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_ALPHA, Gl.GL_TEXTURE);
					break;

				case LayerBlendOperationEx.BlendCurrentAlpha:
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_RGB, Gl.GL_PREVIOUS);
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE2_ALPHA, Gl.GL_PREVIOUS);
					break;

				case LayerBlendOperationEx.BlendManual:
					tempColorVals[0] = 0; tempColorVals[1] = 0; 
					tempColorVals[2] = 0; tempColorVals[3] = blendMode.blendFactor;
					Gl.glTexEnvfv(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_COLOR, tempColorVals);
					break;

				default:
					break;
			}

			// set alpha scale to 1 by default unless specifically requested to be higher
			// otherwise, textures that get switch from ModulateX2 or ModulateX4 down to Source1
			// for example, the alpha scale would still be high and overbrighten the texture
			switch (blendMode.operation) {
				case LayerBlendOperationEx.ModulateX2:
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, blendMode.blendType == LayerBlendType.Color ? 
						Gl.GL_RGB_SCALE : Gl.GL_ALPHA_SCALE, 2);
					break;

				case LayerBlendOperationEx.ModulateX4:
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, blendMode.blendType == LayerBlendType.Color ? 
						Gl.GL_RGB_SCALE : Gl.GL_ALPHA_SCALE, 4);
					break;

				default:
					Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, blendMode.blendType == LayerBlendType.Color ? 
						Gl.GL_RGB_SCALE : Gl.GL_ALPHA_SCALE, 1);
					break;
			}

			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND0_RGB, Gl.GL_SRC_COLOR);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND1_RGB, Gl.GL_SRC_COLOR);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND2_RGB, Gl.GL_SRC_ALPHA);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND0_ALPHA, Gl.GL_SRC_ALPHA);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND1_ALPHA, Gl.GL_SRC_ALPHA);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND2_ALPHA, Gl.GL_SRC_ALPHA);

			// check source1 and set colors values appropriately
			if (blendMode.source1 == LayerBlendSource.Manual) {
				if(blendMode.blendType == LayerBlendType.Color) {
					// color value 1
					blendMode.colorArg1.ToArrayRGBA(tempColorVals);
				}
				else {
					// alpha value 1
					tempColorVals[0] = 0.0f; tempColorVals[1] = 0.0f; tempColorVals[2] = 0.0f; tempColorVals[3] = blendMode.alphaArg1;
				}

				Gl.glTexEnvfv(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_COLOR, tempColorVals);
			}

			// check source2 and set colors values appropriately
			if (blendMode.source2 == LayerBlendSource.Manual) {
				if(blendMode.blendType == LayerBlendType.Color) {
					// color value 2
					blendMode.colorArg2.ToArrayRGBA(tempColorVals);
				}
				else {
					// alpha value 2
					tempColorVals[0] = 0.0f; tempColorVals[1] = 0.0f; tempColorVals[2] = 0.0f; tempColorVals[3] = blendMode.alphaArg2;
				}

				Gl.glTexEnvfv(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_COLOR, tempColorVals);
			}
        
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="type"></param>
		/// <param name="filter"></param>
		public override void SetTextureUnitFiltering(int unit, FilterType type, FilterOptions filter) {
			// set the current texture unit
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0 + unit);

			switch(type) {
				case FilterType.Min:
					minFilter = filter;

					// combine with exiting mip filter
					Gl.glTexParameteri(
						textureTypes[unit], 
						Gl.GL_TEXTURE_MIN_FILTER, 
						GetCombinedMinMipFilter());
					break;

				case FilterType.Mag:
				switch(filter) {
					case FilterOptions.Anisotropic:
					case FilterOptions.Linear:
						Gl.glTexParameteri(
							textureTypes[unit], 
							Gl.GL_TEXTURE_MAG_FILTER, 
							Gl.GL_LINEAR);
						break;
					case FilterOptions.Point:
					case FilterOptions.None:
						Gl.glTexParameteri(
							textureTypes[unit], 
							Gl.GL_TEXTURE_MAG_FILTER, 
							Gl.GL_NEAREST);
						break;
				}
					break;

				case FilterType.Mip:
					mipFilter = filter;

					// combine with exiting mip filter
					Gl.glTexParameteri(
						textureTypes[unit], 
						Gl.GL_TEXTURE_MIN_FILTER, 
						GetCombinedMinMipFilter());
					break;
			}

			// reset to the first texture unit
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="index"></param>
		public override void SetTextureCoordSet(int stage, int index) {
			texCoordIndex[stage] = index;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="method"></param>
		public override void SetTextureCoordCalculation(int stage, TexCoordCalcMethod method, Frustum frustum) {
			// Default to no extra auto texture matrix
			useAutoTextureMatrix = false;

			if(method == TexCoordCalcMethod.None && 
				lastTexCalMethods[stage] == method) {
				return;
			}

			// store for next checking next time around
			lastTexCalMethods[stage] = method;

			float[] eyePlaneS = {1.0f, 0.0f, 0.0f, 0.0f};
			float[] eyePlaneT = {0.0f, 1.0f, 0.0f, 0.0f};
			float[] eyePlaneR = {0.0f, 0.0f, 1.0f, 0.0f};
			float[] eyePlaneQ = {0.0f, 0.0f, 0.0f, 1.0f};

			Gl.glActiveTextureARB(Gl.GL_TEXTURE0 + stage );

			switch(method) {
				case TexCoordCalcMethod.None:
					Gl.glDisable( Gl.GL_TEXTURE_GEN_S );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_T );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_R );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_Q );
					break;

				case TexCoordCalcMethod.EnvironmentMap:
					Gl.glTexGeni( Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP );
					Gl.glTexGeni( Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP );

					Gl.glEnable( Gl.GL_TEXTURE_GEN_S );
					Gl.glEnable( Gl.GL_TEXTURE_GEN_T );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_R );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_Q );

					// Need to use a texture matrix to flip the spheremap
					useAutoTextureMatrix = true;
					Array.Clear(autoTextureMatrix, 0, 16);
					autoTextureMatrix[0] = autoTextureMatrix[10] = autoTextureMatrix[15] = 1.0f;
					autoTextureMatrix[5] = -1.0f;

					break;

				case TexCoordCalcMethod.EnvironmentMapPlanar:            
					// XXX This doesn't seem right?!
					if(glSupport.CheckMinVersion("1.3")) {
						Gl.glTexGeni( Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_REFLECTION_MAP );
						Gl.glTexGeni( Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_REFLECTION_MAP );
						Gl.glTexGeni( Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_REFLECTION_MAP );

						Gl.glEnable( Gl.GL_TEXTURE_GEN_S );
						Gl.glEnable( Gl.GL_TEXTURE_GEN_T );
						Gl.glEnable( Gl.GL_TEXTURE_GEN_R );
						Gl.glDisable( Gl.GL_TEXTURE_GEN_Q );
					}
					else {
						Gl.glTexGeni( Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP );
						Gl.glTexGeni( Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP );

						Gl.glEnable( Gl.GL_TEXTURE_GEN_S );
						Gl.glEnable( Gl.GL_TEXTURE_GEN_T );
						Gl.glDisable( Gl.GL_TEXTURE_GEN_R );
						Gl.glDisable( Gl.GL_TEXTURE_GEN_Q );
					}
					break;

				case TexCoordCalcMethod.EnvironmentMapReflection:
        
					Gl.glTexGeni( Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_REFLECTION_MAP );
					Gl.glTexGeni( Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_REFLECTION_MAP );
					Gl.glTexGeni( Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_REFLECTION_MAP );

					Gl.glEnable( Gl.GL_TEXTURE_GEN_S );
					Gl.glEnable( Gl.GL_TEXTURE_GEN_T );
					Gl.glEnable( Gl.GL_TEXTURE_GEN_R );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_Q );

					// We need an extra texture matrix here
					// This sets the texture matrix to be the inverse of the modelview matrix
					useAutoTextureMatrix = true;

					Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, tempMatrix);

					// Transpose 3x3 in order to invert matrix (rotation)
					// Note that we need to invert the Z _before_ the rotation
					// No idea why we have to invert the Z at all, but reflection is wrong without it
					autoTextureMatrix[0] = tempMatrix[0]; autoTextureMatrix[1] = tempMatrix[4]; autoTextureMatrix[2] = -tempMatrix[8];
					autoTextureMatrix[4] = tempMatrix[1]; autoTextureMatrix[5] = tempMatrix[5]; autoTextureMatrix[6] = -tempMatrix[9];
					autoTextureMatrix[8] = tempMatrix[2]; autoTextureMatrix[9] = tempMatrix[6]; autoTextureMatrix[10] = -tempMatrix[10];
					autoTextureMatrix[3] = autoTextureMatrix[7] = autoTextureMatrix[11] = 0.0f;
					autoTextureMatrix[12] = autoTextureMatrix[13] = autoTextureMatrix[14] = 0.0f;
					autoTextureMatrix[15] = 1.0f;

					break;

				case TexCoordCalcMethod.EnvironmentMapNormal:
					Gl.glTexGeni( Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_NORMAL_MAP );
					Gl.glTexGeni( Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_NORMAL_MAP );
					Gl.glTexGeni( Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_NORMAL_MAP );

					Gl.glEnable( Gl.GL_TEXTURE_GEN_S );
					Gl.glEnable( Gl.GL_TEXTURE_GEN_T );
					Gl.glEnable( Gl.GL_TEXTURE_GEN_R );
					Gl.glDisable( Gl.GL_TEXTURE_GEN_Q );
					break;

				case TexCoordCalcMethod.ProjectiveTexture:
					Gl.glTexGeni( Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
					Gl.glTexGeni( Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
					Gl.glTexGeni( Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
					Gl.glTexGeni( Gl.GL_Q, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
					Gl.glTexGenfv( Gl.GL_S, Gl.GL_EYE_PLANE, eyePlaneS);
					Gl.glTexGenfv( Gl.GL_T, Gl.GL_EYE_PLANE, eyePlaneT);
					Gl.glTexGenfv( Gl.GL_R, Gl.GL_EYE_PLANE, eyePlaneR);
					Gl.glTexGenfv( Gl.GL_Q, Gl.GL_EYE_PLANE, eyePlaneQ);
					Gl.glEnable( Gl.GL_TEXTURE_GEN_S);
					Gl.glEnable( Gl.GL_TEXTURE_GEN_T);
					Gl.glEnable( Gl.GL_TEXTURE_GEN_R);
					Gl.glEnable( Gl.GL_TEXTURE_GEN_Q);

					useAutoTextureMatrix = true;

					// Set scale and translation matrix for projective textures
					Matrix4 projectionBias = Matrix4.Zero;
					projectionBias.m00 = 0.5f; projectionBias.m11 = -0.5f; 
					projectionBias.m22 = 1.0f; projectionBias.m03 = 0.5f; 
					projectionBias.m13 = 0.5f; projectionBias.m33 = 1.0f;

					projectionBias = projectionBias * frustum.ProjectionMatrix;
					projectionBias = projectionBias * frustum.ViewMatrix;
					projectionBias = projectionBias * worldMatrix;

					MakeGLMatrix( ref projectionBias, autoTextureMatrix );
					break;
				
				default:
					break;
			}

			Gl.glActiveTextureARB(Gl.GL_TEXTURE0);		
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="xform"></param>
		public override void SetTextureMatrix(int stage, Matrix4 xform) {
            
			MakeGLMatrix(ref xform, tempMatrix);

			tempMatrix[12] = tempMatrix[8];
			tempMatrix[13] = tempMatrix[9];

			Gl.glActiveTextureARB(Gl.GL_TEXTURE0 + stage);
			Gl.glMatrixMode(Gl.GL_TEXTURE);

			// if texture matrix was precalced, use that
			if(useAutoTextureMatrix) {
				Gl.glLoadMatrixf(autoTextureMatrix);
				Gl.glMultMatrixf(tempMatrix);
			}
			else {
				Gl.glLoadMatrixf(tempMatrix);
			}

			// reset to mesh view matrix and to tex unit 0
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="autoCreateWindow"></param>
		/// <param name="windowTitle">Title of the window to create.</param>
		/// <returns></returns>
		public override RenderWindow Initialize(bool autoCreateWindow, string windowTitle) {
			RenderWindow autoWindow = glSupport.CreateWindow(autoCreateWindow, this, windowTitle);

			this.CullingMode = this.cullingMode;

			return autoWindow;
		}

		/// <summary>
		///		Shutdown the render system.
		/// </summary>
		public override void Shutdown() {
			// call base Shutdown implementation
			base.Shutdown();

            if (gpuProgramMgr != null) {
                gpuProgramMgr.Dispose();
            }
            if (hardwareBufferManager != null) {
                hardwareBufferManager.Dispose();
            }
            if (textureMgr != null) {
                textureMgr.Dispose();
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="enabled"></param>
		/// <param name="textureName"></param>
		public override void SetTexture(int stage, bool enabled, string textureName) {
			// load the texture
			GLTexture texture = (GLTexture)TextureManager.Instance.GetByName(textureName);

			int lastTextureType = textureTypes[stage];

			// set the active texture
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0 + stage);

			// enable and bind the texture if necessary
			if(enabled) {
				if(texture != null) {
					textureTypes[stage] = texture.GLTextureType;
				}
				else {
					// assume 2D
					textureTypes[stage] = Gl.GL_TEXTURE_2D;
				}

				if(lastTextureType != textureTypes[stage] && lastTextureType != 0) {
					Gl.glDisable(lastTextureType);
				}

				Gl.glEnable(textureTypes[stage]);

				if(texture != null) {
					Gl.glBindTexture(textureTypes[stage], texture.TextureID);
				}
			}
			else {
				if(textureTypes[stage] != 0) {
					Gl.glDisable(textureTypes[stage]);
				}

				Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
			}

			// reset active texture to unit 0
			Gl.glActiveTextureARB(Gl.GL_TEXTURE0);
		}

		public override void SetAlphaRejectSettings(int stage, CompareFunction func, byte val) {
			Gl.glEnable(Gl.GL_ALPHA_TEST);
			Gl.glAlphaFunc(GLHelper.ConvertEnum(func), val / 255.0f);
		}

		public override void SetColorBufferWriteEnabled(bool red, bool green, bool blue, bool alpha) {
			// record this for later
			colorWrite[0] = red ? 1 : 0;
			colorWrite[1] = green ? 1 : 0;
			colorWrite[2] = blue ? 1 : 0;
			colorWrite[3] = alpha ? 1 : 0;

			Gl.glColorMask(colorWrite[0], colorWrite[1], colorWrite[2], colorWrite[3]);
		}

		public override void SetDepthBufferParams(bool depthTest, bool depthWrite, CompareFunction depthFunction) {
			this.DepthCheck = depthTest;
			this.DepthWrite = depthWrite;
			this.DepthFunction = depthFunction;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="color"></param>
		/// <param name="density"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public override void SetFog(FogMode mode, ColorEx color, float density, float start, float end) {
			int fogMode;

			switch(mode) {
				case FogMode.Exp:
					fogMode = Gl.GL_EXP;
					break;
				case FogMode.Exp2:
					fogMode = Gl.GL_EXP2;
					break;
				case FogMode.Linear:
					fogMode = Gl.GL_LINEAR;
					break;
				default:
					if(fogEnabled) {
						Gl.glDisable(Gl.GL_FOG);
						fogEnabled = false;
					}
					return;
			} // switch

			Gl.glEnable(Gl.GL_FOG);
			Gl.glFogi(Gl.GL_FOG_MODE, fogMode);
			// fog color values
			color.ToArrayRGBA(tempColorVals);
			Gl.glFogfv(Gl.GL_FOG_COLOR, tempColorVals);
			Gl.glFogf(Gl.GL_FOG_DENSITY, density);
			Gl.glFogf(Gl.GL_FOG_START, start);
			Gl.glFogf(Gl.GL_FOG_END, end);
			fogEnabled = true;

			// TODO: Fog hints maybe?
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="op"></param>
		public override void Render(RenderOperation op) {

			// don't even bother if there are no vertices to render, causes problems on some cards (FireGL 8800)
			if(op.vertexData.vertexCount == 0) {
				return;
			}

			// call base class method first
			base.Render(op);

			// will be used to alia either the buffer offset (VBO's) or array data if VBO's are
			// not available
			IntPtr bufferData = IntPtr.Zero;

			VertexDeclaration decl = op.vertexData.vertexDeclaration;
	
			// loop through and handle each element
			for(int i = 0; i < decl.ElementCount; i++) {
				// get a reference to the current object in the collection
				VertexElement element = decl.GetElement(i);

				// get the current vertex buffer
				HardwareVertexBuffer vertexBuffer = op.vertexData.vertexBufferBinding.GetBuffer(element.Source);

				if(caps.CheckCap(Capabilities.VertexBuffer)) {
					// get the buffer id
					int bufferId = ((GLHardwareVertexBuffer)vertexBuffer).GLBufferID;

					// bind the current vertex buffer
					Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, bufferId);
					bufferData = BUFFER_OFFSET(element.Offset);
				}
				else {
					// get a direct pointer to the software buffer data for using standard vertex arrays
					bufferData = ((SoftwareVertexBuffer)vertexBuffer).GetDataPointer(element.Offset);
				}

				// get the type of this buffer
				int type = GLHelper.ConvertEnum(element.Type);

				// set pointer usage based on the use of this buffer
				switch(element.Semantic) {
					case VertexElementSemantic.Position:
						// set the pointer data
						Gl.glVertexPointer(
							VertexElement.GetTypeCount(element.Type),
							type,
							vertexBuffer.VertexSize,
							bufferData);

						// enable the vertex array client state
						Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);

						break;
				
					case VertexElementSemantic.Normal:
						// set the pointer data
						Gl.glNormalPointer(
							type, 
							vertexBuffer.VertexSize,
							bufferData);

						// enable the normal array client state
						Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);

						break;
				
					case VertexElementSemantic.Diffuse:
						// set the color pointer data
						Gl.glColorPointer(
							4,
							type, 
							vertexBuffer.VertexSize,
							bufferData);

						// enable the color array client state
						Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);

						break;
				
					case VertexElementSemantic.Specular:
						// set the secondary color pointer data
						Gl.glSecondaryColorPointerEXT(
							4,
							type, 
							vertexBuffer.VertexSize,
							bufferData);

						// enable the secondary color array client state
						Gl.glEnableClientState(Gl.GL_SECONDARY_COLOR_ARRAY);

						break;

					case VertexElementSemantic.TexCoords:
						// this ignores vertex element index and sets tex array for each available texture unit
						// this allows for multitexturing on entities whose mesh only has a single set of tex coords
						for(int j = 0; j < caps.TextureUnitCount; j++) {
							// only set if this textures index if it is supposed to
							if(texCoordIndex[j] == element.Index) {
								// set the current active texture unit
								Gl.glClientActiveTextureARB(Gl.GL_TEXTURE0 + j); 

								// set the tex coord pointer
								Gl.glTexCoordPointer(
									VertexElement.GetTypeCount(element.Type),
									type,
									vertexBuffer.VertexSize,
									bufferData);

								// enable texture coord state
								Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
							}
						}
						break;

					case VertexElementSemantic.BlendIndices:
						Debug.Assert(caps.CheckCap(Capabilities.VertexPrograms));

						Gl.glVertexAttribPointerARB(
							BLEND_INDICES, // matrix indices are vertex attribute 7
							VertexElement.GetTypeCount(element.Type), 
							GLHelper.ConvertEnum(element.Type),
							Gl.GL_FALSE, // normalisation disabled
							vertexBuffer.VertexSize,
							bufferData);

						Gl.glEnableVertexAttribArrayARB(BLEND_INDICES);
						break;

					case VertexElementSemantic.BlendWeights:
						Debug.Assert(caps.CheckCap(Capabilities.VertexPrograms));

						Gl.glVertexAttribPointerARB(
							BLEND_WEIGHTS, // weights are vertex attribute 1
							VertexElement.GetTypeCount(element.Type), 
							GLHelper.ConvertEnum(element.Type),
							Gl.GL_FALSE, // normalisation disabled
							vertexBuffer.VertexSize,
							bufferData);

						Gl.glEnableVertexAttribArrayARB(BLEND_WEIGHTS);
						break;

					default:
						break;
				} // switch
			} // for

			// reset to texture unit 0
			Gl.glClientActiveTextureARB(Gl.GL_TEXTURE0); 

			int primType = 0;

			// which type of render operation is this?
			switch(op.operationType) {
				case OperationType.PointList:
					primType = Gl.GL_POINTS;
					break;
				case OperationType.LineList:
					primType = Gl.GL_LINES;
					break;
				case OperationType.LineStrip:
					primType = Gl.GL_LINE_STRIP;
					break;
				case OperationType.TriangleList:
					primType = Gl.GL_TRIANGLES;
					break;
				case OperationType.TriangleStrip:
					primType = Gl.GL_TRIANGLE_STRIP;
					break;
				case OperationType.TriangleFan:
					primType = Gl.GL_TRIANGLE_FAN;
					break;
			}

			if(op.useIndices) {
				// setup a pointer to the index data
				IntPtr indexPtr = IntPtr.Zero;

				// if hardware is supported, expect it is a hardware buffer.  else, fallback to software
				if(caps.CheckCap(Capabilities.VertexBuffer)) {
					// get the index buffer id
					int idxBufferID = ((GLHardwareIndexBuffer)op.indexData.indexBuffer).GLBufferID;

					// bind the current index buffer
					Gl.glBindBufferARB(Gl.GL_ELEMENT_ARRAY_BUFFER_ARB, idxBufferID);

					// get the offset pointer to the data in the vbo
					indexPtr = BUFFER_OFFSET(op.indexData.indexStart * op.indexData.indexBuffer.IndexSize);
				}
				else {
					// get the index data as a direct pointer to the software buffer data
					indexPtr = ((SoftwareIndexBuffer)op.indexData.indexBuffer).GetDataPointer(op.indexData.indexStart * op.indexData.indexBuffer.IndexSize);
				}

				// find what type of index buffer elements we are using
				int indexType = (op.indexData.indexBuffer.Type == IndexType.Size16) 
					? Gl.GL_UNSIGNED_SHORT : Gl.GL_UNSIGNED_INT;

				// draw the indexed vertex data
//				Gl.glDrawRangeElementsEXT(
//					primType,
//					op.indexData.indexStart,
//					op.indexData.indexStart + op.indexData.indexCount - 1,
//					op.indexData.indexCount,
//					indexType, indexPtr);
                Gl.glDrawElements(primType, op.indexData.indexCount, indexType, indexPtr);
            }
			else {
				Gl.glDrawArrays(primType, op.vertexData.vertexStart, op.vertexData.vertexCount);
			}

			// disable all client states
			Gl.glDisableClientState( Gl.GL_VERTEX_ARRAY );

            // disable all texture units
            for (int i = 0; i < caps.TextureUnitCount; i++) {
                Gl.glClientActiveTextureARB(Gl.GL_TEXTURE0 + i);
                Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            }

            Gl.glClientActiveTextureARB(Gl.GL_TEXTURE0);
            Gl.glDisableClientState( Gl.GL_NORMAL_ARRAY );
			Gl.glDisableClientState( Gl.GL_COLOR_ARRAY );
			Gl.glDisableClientState( Gl.GL_SECONDARY_COLOR_ARRAY );

			if (caps.CheckCap(Capabilities.VertexPrograms)) {
				Gl.glDisableVertexAttribArrayARB(BLEND_INDICES); // disable indices
				Gl.glDisableVertexAttribArrayARB(BLEND_WEIGHTS); // disable weights
			}

			Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
		}

		/// <summary>
		///		
		/// </summary>
		public override Matrix4 ProjectionMatrix {
			get {
				throw new NotImplementedException();
			}
			set {
				// create a float[16] from our Matrix4
				MakeGLMatrix(ref value, tempMatrix);

				// invert the Y if need be
				if(activeRenderTarget.RequiresTextureFlipping) {
					tempMatrix[5] = -tempMatrix[5];
				}
			
				// set the matrix mode to Projection
				Gl.glMatrixMode(Gl.GL_PROJECTION);

				// load the float array into the projection matrix
				Gl.glLoadMatrixf(tempMatrix);

				// set the matrix mode back to ModelView
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
			}
		}

		/// <summary>
		///		
		/// </summary>
		public override Matrix4 ViewMatrix {
			get {
				throw new NotImplementedException();
			}
			set {
				viewMatrix = value;

				// create a float[16] from our Matrix4
				MakeGLMatrix(ref viewMatrix, tempMatrix);
			
				// set the matrix mode to ModelView
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
			
				// load the float array into the ModelView matrix
				Gl.glLoadMatrixf(tempMatrix);

				// convert the internal world matrix
				MakeGLMatrix(ref worldMatrix, tempMatrix);

				// multply the world matrix by the current ModelView matrix
				Gl.glMultMatrixf(tempMatrix);
			}
		}

		/// <summary>
		/// </summary>
		public override Matrix4 WorldMatrix {
			get {
				throw new NotImplementedException();
			}
			set {
				//store the new world matrix locally
				worldMatrix = value;

				// multiply the view and world matrices, and convert it to GL format
				Matrix4 multMatrix = viewMatrix * worldMatrix;
				MakeGLMatrix(ref multMatrix, tempMatrix);

				// change the matrix mode to ModelView
				Gl.glMatrixMode(Gl.GL_MODELVIEW);

				// load the converted GL matrix
				Gl.glLoadMatrixf(tempMatrix);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lightList"></param>
		/// <param name="limit"></param>
		public override void UseLights(LightList lightList, int limit) {
			// save previous modelview matrix
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glPushMatrix();
			// load the view matrix
			MakeGLMatrix(ref viewMatrix, tempMatrix);
			Gl.glLoadMatrixf(tempMatrix);

			int i = 0;

			for( ; i < limit && i < lightList.Count; i++) {
				SetGLLight(i, lightList[i]);
				lights[i] = lightList[i];
			}

			for( ; i < numCurrentLights; i++) {
				SetGLLight(i, null);
				lights[i] = null;
			}

			numCurrentLights = (int)MathUtil.Min(limit, lightList.Count);

			SetLights();

			// restore the previous matrix
			Gl.glPopMatrix();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public override int ConvertColor(ColorEx color) {
			return color.ToABGR();
		}



		/// <summary>
		///   Convert the RenderSystem's encoding of color to an explicit portable one.
		/// </summary>
		/// <param name="color">The color as an integer</param>
		/// <returns>ColorEx version of the RenderSystem specific int storage of color</returns>
		public override ColorEx ConvertColor(int color) {
            ColorEx colorEx = new ColorEx(); 
            colorEx.a = (float)((color >> 24) % 256) / 255;
            colorEx.b = (float)((color >> 16) % 256) / 255;
            colorEx.g = (float)((color >> 8 ) % 256) / 255;
            colorEx.r = (float)((color      ) % 256) / 255;
			return colorEx;
		}
		
		public override CullingMode CullingMode {
			get {
				return cullingMode;
			}
			set {
				// ignore dupe render state
				if(value == cullingMode) {
					//return;
				}

				cullingMode = value;

				int cullMode = Gl.GL_CW;

				switch(value) {
					case CullingMode.None:
						Gl.glDisable(Gl.GL_CULL_FACE);
						return;
					case CullingMode.Clockwise:
						if(activeRenderTarget != null
							&& (activeRenderTarget.RequiresTextureFlipping ^ invertVertexWinding)) {

							cullMode = Gl.GL_CW;
						}
						else {
							cullMode = Gl.GL_CCW;
						}
						break;
					case CullingMode.CounterClockwise:
						if(activeRenderTarget != null
							&& (activeRenderTarget.RequiresTextureFlipping ^ invertVertexWinding)) {

							cullMode = Gl.GL_CCW;
						}
						else {
							cullMode = Gl.GL_CW;
						}
						break;
				}

				Gl.glEnable(Gl.GL_CULL_FACE);
				Gl.glFrontFace(cullMode);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dest"></param>
		public override void SetSceneBlending(SceneBlendFactor src, SceneBlendFactor dest) {
			if(src == lastBlendSrc && dest == lastBlendDest) {
				return;
			}

			int srcFactor = ConvertBlendFactor(src);
			int destFactor = ConvertBlendFactor(dest);

			// enable blending and set the blend function
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glBlendFunc(srcFactor, destFactor);

			lastBlendSrc = src;
			lastBlendDest = dest;
		}

		/// <summary>
        ///   Set the bias on the z-values for polygons.
        ///   For a 24 bit z buffer, something like 0.00002 should work
        /// </summary>
		public override float DepthBias {
			get {
				throw new NotImplementedException();
			}
			set {
				// reduce dupe state changes
				if(lastDepthBias == value) {
					return;
				}

				lastDepthBias = value;

				if (value > 0) {
					Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
					Gl.glEnable(Gl.GL_POLYGON_OFFSET_POINT);
					Gl.glEnable(Gl.GL_POLYGON_OFFSET_LINE);
					// Bias is in {0, 16}, scale the unit addition appropriately
					Gl.glPolygonOffset(1.0f, value);
				}
				else {
					Gl.glDisable(Gl.GL_POLYGON_OFFSET_FILL);
					Gl.glDisable(Gl.GL_POLYGON_OFFSET_POINT);
					Gl.glDisable(Gl.GL_POLYGON_OFFSET_LINE);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool DepthCheck {
			get {
				throw new NotImplementedException();
			}
			set {
				// reduce dupe state changes
				if(lastDepthCheck == value) {
					return;
				}

				lastDepthCheck = value;

				if(value) {
					// clear the buffer and enable
					Gl.glClearDepth(1.0f);
					Gl.glEnable(Gl.GL_DEPTH_TEST);
				}
				else
					Gl.glDisable(Gl.GL_DEPTH_TEST);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override CompareFunction DepthFunction {
			get {
				throw new NotImplementedException();
			}
			set {
				// reduce dupe state changes
				if(lastDepthFunc == value) {
					return;
				}
				lastDepthFunc = value;

				Gl.glDepthFunc(GLHelper.ConvertEnum(value));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool DepthWrite {
			get {
				throw new NotImplementedException();
			}
			set {
				// reduce dupe state changes
				if(lastDepthWrite == value) {
					return;
				}
				lastDepthWrite = value;

				int flag = value ? Gl.GL_TRUE : Gl.GL_FALSE;
				Gl.glDepthMask( flag );  

				// Store for reference in BeginFrame
				depthWrite = value;
			}
		}

		/// <summary>
		///		
		/// </summary>
		public override float HorizontalTexelOffset {
			get {
				// No offset in GL
				return 0.0f;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override float VerticalTexelOffset {
			get {
				// No offset in GL
				return 0.0f;
			}
		}

		/// <summary>
		///    Binds the specified GpuProgram to the future rendering operations.
		/// </summary>
		/// <param name="program"></param>
		public override void BindGpuProgram(GpuProgram program) {
			GLGpuProgram glProgram = (GLGpuProgram)program;

			glProgram.Bind();

			// store the current program in use for eas unbinding later
			if(glProgram.Type == GpuProgramType.Vertex) {
				currentVertexProgram = glProgram;
			}
			else {
				currentFragmentProgram = glProgram;
			}
		}

		/// <summary>
		///    Binds the supplied parameters to programs of the specified type for future rendering operations.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parms"></param>
		public override void BindGpuProgramParameters(GpuProgramType type, GpuProgramParameters parms) {
			// store the current program in use for eas unbinding later
			if(type == GpuProgramType.Vertex) {
				currentVertexProgram.BindParameters(parms);
			}
			else {
				currentFragmentProgram.BindParameters(parms);
			}
		}

		/// <summary>
		///    Unbinds programs of the specified type.
		/// </summary>
		/// <param name="type"></param>
		public override void UnbindGpuProgram(GpuProgramType type) {
			// store the current program in use for eas unbinding later
			if(type == GpuProgramType.Vertex && currentVertexProgram != null) {
				currentVertexProgram.Unbind();
				currentVertexProgram = null;
			}
			else if(type == GpuProgramType.Fragment && currentFragmentProgram != null) {
				currentFragmentProgram.Unbind();
				currentFragmentProgram = null;
			}
		}

		public override void SetScissorTest(bool enable, int left, int top, int right, int bottom) {
			if(enable) {
				Gl.glEnable(Gl.GL_SCISSOR_TEST);
				// GL uses width / height rather than right / bottom
				Gl.glScissor(left, top, right - left, bottom - top);
			}
			else {
				Gl.glDisable(Gl.GL_SCISSOR_TEST);
			}
		}

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="borderColor"></param>
        public override void SetTextureBorderColor(int stage, ColorEx borderColor)
        {
            throw new Exception("The method or operation is not implemented.");
        }

		#endregion Implementation of RenderSystem

		#region Private methods

		/// <summary>
		///		Private method to convert our blend factors to that of Open GL
		/// </summary>
		/// <param name="factor"></param>
		/// <returns></returns>
		private int ConvertBlendFactor(SceneBlendFactor factor) {
			int glFactor = 0;

			switch(factor) {
				case SceneBlendFactor.One:
					glFactor =  Gl.GL_ONE;
					break;
				case SceneBlendFactor.Zero:
					glFactor =  Gl.GL_ZERO;
					break;
				case SceneBlendFactor.DestColor:
					glFactor =  Gl.GL_DST_COLOR;
					break;
				case SceneBlendFactor.SourceColor:
					glFactor =  Gl.GL_SRC_COLOR;
					break;
				case SceneBlendFactor.OneMinusDestColor:
					glFactor =  Gl.GL_ONE_MINUS_DST_COLOR;
					break;
				case SceneBlendFactor.OneMinusSourceColor:
					glFactor =  Gl.GL_ONE_MINUS_SRC_COLOR;
					break;
				case SceneBlendFactor.DestAlpha:
					glFactor =  Gl.GL_DST_ALPHA;
					break;
				case SceneBlendFactor.SourceAlpha:
					glFactor =  Gl.GL_SRC_ALPHA;
					break;
				case SceneBlendFactor.OneMinusDestAlpha:
					glFactor =  Gl.GL_ONE_MINUS_DST_ALPHA;
					break;
				case SceneBlendFactor.OneMinusSourceAlpha:
					glFactor =  Gl.GL_ONE_MINUS_SRC_ALPHA;
					break;
			}

			// return the GL equivalent
			return glFactor;
		}

		/// <summary>
		///		Converts a Matrix4 object to a float[16] that contains the matrix
		///		in top to bottom, left to right order.
		///		i.e.	glMatrix[0] = matrix[0,0]
		///				glMatrix[1] = matrix[1,0]
		///				etc...
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		private void MakeGLMatrix(ref Matrix4 matrix, float[] floats) {
			Matrix4 mat = matrix.Transpose();

			mat.MakeFloatArray(floats);
		}

		/// <summary>
		///		Helper method for setting all the options for a single light.
		/// </summary>
		/// <param name="index">Light index.</param>
		/// <param name="light">Light object.</param>
		private void SetGLLight(int index, Light light) {
			int lightIndex = Gl.GL_LIGHT0 + index;

			if(light == null) {
				// disable the light if it is not visible
				Gl.glDisable(lightIndex);
			}
			else {
				// set spotlight cutoff
				switch(light.Type) {
					case LightType.Spotlight:
						Gl.glLightf(lightIndex, Gl.GL_SPOT_CUTOFF, light.SpotlightOuterAngle);
						break;
					default:
						Gl.glLightf(lightIndex, Gl.GL_SPOT_CUTOFF, 180.0f);
						break;
				}

				// light color
				light.Diffuse.ToArrayRGBA(tempColorVals);
				Gl.glLightfv(lightIndex, Gl.GL_DIFFUSE, tempColorVals);

				// specular color
				light.Specular.ToArrayRGBA(tempColorVals);
				Gl.glLightfv(lightIndex, Gl.GL_SPECULAR, tempColorVals);

				// disable ambient light for objects
				// BUG: Why does this return GL ERROR 1280?
				Gl.glLighti(lightIndex, Gl.GL_AMBIENT, 0);

				SetGLLightPositionDirection(light, index);

				// light attenuation
				Gl.glLightf(lightIndex, Gl.GL_CONSTANT_ATTENUATION, light.AttenuationConstant);
				Gl.glLightf(lightIndex, Gl.GL_LINEAR_ATTENUATION, light.AttenuationLinear);
				Gl.glLightf(lightIndex, Gl.GL_QUADRATIC_ATTENUATION, light.AttenuationQuadratic);

				// enable the light
				Gl.glEnable(lightIndex);
			}
		}

		/// <summary>
		///		Helper method for resetting the position and direction of a light.
		/// </summary>
		/// <param name="light">Light to use.</param>
		/// <param name="index">Index of the light.</param>
		private void SetGLLightPositionDirection(Light light, int index) {
			// Use general 4D vector which is the same as GL's approach
			Vector4 vec4 = light.GetAs4DVector();

			tempLightVals[0] = vec4.x; tempLightVals[1] = vec4.y; 
			tempLightVals[2] = vec4.z; tempLightVals[3] = vec4.w;

			Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_POSITION, tempLightVals);

			// set spotlight direction
			if(light.Type == LightType.Spotlight) {
				Vector3 vec3 = light.DerivedDirection;
				tempLightVals[0] = vec3.x; tempLightVals[1] = vec3.y; 
				tempLightVals[2] = vec3.z; tempLightVals[3] = 0.0f;

				Gl.glLightfv(Gl.GL_LIGHT0 + index, Gl.GL_SPOT_DIRECTION, tempLightVals);
			}	
		}

		/// <summary>
		///		Private helper method for setting all lights.
		/// </summary>
		private void SetLights() {
			for(int i = 0; i < lights.Length; i++) {
				if(lights[i] != null) {
					SetGLLightPositionDirection(lights[i], i);
				}
			}
		}

		/// <summary>
		///		Called in constructor to init configuration.
		/// </summary>
		private void InitConfigOptions() {
			glSupport.AddConfig();
		}

		/// <summary>
		///		Helper method to go through and interrogate hardware capabilities.
		/// </summary>
		private void CheckCaps() {
			// find out how many lights we have to play with, then create a light array to keep locally
			int maxLights;
			Gl.glGetIntegerv(Gl.GL_MAX_LIGHTS, out maxLights);
			caps.MaxLights = maxLights;

			// check the number of texture units available
			int numTextureUnits = 0;
			Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_UNITS, out numTextureUnits);
			caps.TextureUnitCount = numTextureUnits;

			// check multitexturing
			if(glSupport.CheckExtension("GL_ARB_multitexture")) {
				caps.SetCap(Capabilities.MultiTexturing);
			}

			// check texture blending
			if(glSupport.CheckExtension("GL_EXT_texture_env_combine") || 
				glSupport.CheckExtension("GL_ARB_texture_env_combine")) {
				caps.SetCap(Capabilities.TextureBlending);
			}

			// anisotropic filtering
			if(glSupport.CheckExtension("GL_EXT_texture_filter_anisotropic")) {
				caps.SetCap(Capabilities.AnisotropicFiltering);
			}

			// check dot3 support
			if(glSupport.CheckExtension("GL_ARB_texture_env_dot3")) {
				caps.SetCap(Capabilities.Dot3);
			}

			// check support for vertex buffers in hardware
			if(glSupport.CheckExtension("GL_ARB_vertex_buffer_object")) {
				caps.SetCap(Capabilities.VertexBuffer);
			}

			if(glSupport.CheckExtension("GL_ARB_texture_cube_map")
				|| glSupport.CheckExtension("GL_EXT_texture_cube_map")) {
				caps.SetCap(Capabilities.CubeMapping);
			}

			// check support for hardware vertex blending
			// TODO: Dont check this cap yet, wait for vertex shader support so that software blending is always used
			//if(GLHelper.CheckExtension("GL_ARB_vertex_blend"))
			//    caps.SetCap(Capabilities.VertexBlending);

			// check if the hardware supports anisotropic filtering
			if(glSupport.CheckExtension("GL_EXT_texture_filter_anisotropic")) {
				caps.SetCap(Capabilities.AnisotropicFiltering);
			}

			// check hardware mip mapping
			if(glSupport.CheckExtension("GL_SGIS_generate_mipmap")) {
				caps.SetCap(Capabilities.HardwareMipMaps);
			}

			// Texture Compression
			if(glSupport.CheckExtension("GL_ARB_texture_compression")) {
				caps.SetCap(Capabilities.TextureCompression);

				// DXT compression
				if(glSupport.CheckExtension("GL_EXT_texture_compression_s3tc")) {
					caps.SetCap(Capabilities.TextureCompressionDXT);
				}

				// VTC compression
				if(glSupport.CheckExtension("GL_NV_texture_compression_vtc")) {
					caps.SetCap(Capabilities.TextureCompressionVTC);
				}
			}

			// check stencil buffer depth availability
			int stencilBits;
			Gl.glGetIntegerv(Gl.GL_STENCIL_BITS, out stencilBits);
			caps.StencilBufferBits = stencilBits;

			// if stencil bits are available, enable stencil buffering
			if(stencilBits > 0) {
				caps.SetCap(Capabilities.StencilBuffer);
			}

			// 2 sided stencil
			if(glSupport.CheckExtension("GL_EXT_stencil_two_side")) {
				caps.SetCap(Capabilities.TwoSidedStencil);
			}

			// stencil wrapping
			if(glSupport.CheckExtension("GL_EXT_stencil_wrap")) {
				caps.SetCap(Capabilities.StencilWrap);
			}

			// Check for hardware occlusion support
			if(glSupport.CheckExtension("GL_NV_occlusion_query")) {
				caps.SetCap(Capabilities.HardwareOcculusion);
			}

			// scissor test is standard in GL 1.2 and above
			caps.SetCap(Capabilities.ScissorTest);

			// UBYTE4 is always supported in GL
			caps.SetCap(Capabilities.VertexFormatUByte4);

			// Infinit far plane always supported
			caps.SetCap(Capabilities.InfiniteFarPlane);

			// Hardware occlusion queries
			if(glSupport.CheckExtension("GL_NV_occlusion_query")) {
				caps.SetCap(Capabilities.HardwareOcculusion);
			}

			// ARB Vertex Programs
			if(glSupport.CheckExtension("GL_ARB_vertex_program")) {
				caps.SetCap(Capabilities.VertexPrograms);
				caps.MaxVertexProgramVersion = "arbvp1";
				caps.VertexProgramConstantIntCount = 0;
				// TODO: Fix constant float count calcs, glGetIntegerv doesn't work
				//int maxFloats;
				//Gl.glGetIntegerv(Gl.GL_MAX_PROGRAM_LOCAL_PARAMETERS_ARB, out maxFloats);
				//caps.VertexProgramConstantFloatCount = maxFloats;

				// register support for arbvp1
				gpuProgramMgr.PushSyntaxCode("arbvp1");
				gpuProgramMgr.RegisterProgramFactory("arbvp1", new ARB.ARBGpuProgramFactory());
			}

			// ARB Fragment Programs
			if(glSupport.CheckExtension("GL_ARB_fragment_program")) {
				caps.SetCap(Capabilities.FragmentPrograms);
				caps.MaxFragmentProgramVersion = "arbfp1";
				caps.FragmentProgramConstantIntCount = 0;
				// TODO: Fix constant float count calcs, glGetIntegerv doesn't work
				//int maxFloats;
				//Gl.glGetIntegerv(Gl.GL_MAX_PROGRAM_LOCAL_PARAMETERS_ARB, out maxFloats);
				//caps.FragmentProgramConstantFloatCount = maxFloats;

				// register support for arbfp1
				gpuProgramMgr.PushSyntaxCode("arbfp1");
				gpuProgramMgr.RegisterProgramFactory("arbfp1", new ARB.ARBGpuProgramFactory());
			}

			// ATI Fragment Programs (supported via conversion from DX ps1.1 - ps1.4 shaders)
			if(glSupport.CheckExtension("GL_ATI_fragment_shader")) {
				caps.SetCap(Capabilities.FragmentPrograms);
				caps.MaxFragmentProgramVersion = "ps_1_4";
				caps.FragmentProgramConstantIntCount = 0;
				// TODO: Fix constant float count calcs, glGetIntegerv doesn't work
				//int maxFloats;
				//Gl.glGetIntegerv(Gl.GL_MAX_PROGRAM_LOCAL_PARAMETERS_ARB, out maxFloats);
				//caps.FragmentProgramConstantFloatCount = maxFloats;

				// register support for ps1.1 - ps1.4
				gpuProgramMgr.PushSyntaxCode("ps_1_1");
				gpuProgramMgr.PushSyntaxCode("ps_1_2");
				gpuProgramMgr.PushSyntaxCode("ps_1_3");
				gpuProgramMgr.PushSyntaxCode("ps_1_4");
				gpuProgramMgr.RegisterProgramFactory("ps_1_1", new ATI.ATIFragmentShaderFactory());
				gpuProgramMgr.RegisterProgramFactory("ps_1_2", new ATI.ATIFragmentShaderFactory());
				gpuProgramMgr.RegisterProgramFactory("ps_1_3", new ATI.ATIFragmentShaderFactory());
				gpuProgramMgr.RegisterProgramFactory("ps_1_4", new ATI.ATIFragmentShaderFactory());
			}

			// GeForce3/4 Register Combiners/Texture Shaders
			if(glSupport.CheckExtension("GL_NV_register_combiners2") &&
				glSupport.CheckExtension("GL_NV_texture_shader")) {

				caps.SetCap(Capabilities.FragmentPrograms);
				caps.MaxFragmentProgramVersion = "fp20";

				gpuProgramMgr.PushSyntaxCode("fp20");
				gpuProgramMgr.RegisterProgramFactory("fp20", new Nvidia.NvparseProgramFactory());
			}

			// GeForceFX vp30 Vertex Programs
			if(glSupport.CheckExtension("GL_NV_vertex_program2")) {
				caps.SetCap(Capabilities.VertexPrograms);
				caps.MaxVertexProgramVersion = "vp30";

				gpuProgramMgr.PushSyntaxCode("vp30");
				gpuProgramMgr.RegisterProgramFactory("vp30", new Nvidia.NV3xGpuProgramFactory());
			}

			// GeForceFX fp30 Fragment Programs
			if(glSupport.CheckExtension("GL_NV_fragment_program")) {
				caps.SetCap(Capabilities.FragmentPrograms);
				caps.MaxFragmentProgramVersion = "fp30";

				gpuProgramMgr.PushSyntaxCode("fp30");
				gpuProgramMgr.RegisterProgramFactory("fp30", new Nvidia.NV3xGpuProgramFactory());
			}

			// GLSL support
			if(	glSupport.CheckExtension("GL_ARB_shading_language_100") &&
				glSupport.CheckExtension("GL_ARB_shader_objects") &&
				glSupport.CheckExtension("GL_ARB_fragment_shader") &&
				glSupport.CheckExtension("GL_ARB_vertex_shader")) {

				gpuProgramMgr.PushSyntaxCode("glsl");
			}

			// write info to logs
			caps.Log();
		}

		/// <summary>
		///    
		/// </summary>
		/// <returns></returns>
		private int GetCombinedMinMipFilter() {
			switch(minFilter) {
				case FilterOptions.Anisotropic:
				case FilterOptions.Linear:
				switch(mipFilter) {
					case FilterOptions.Anisotropic:
					case FilterOptions.Linear:
						// linear min, linear map
						return Gl.GL_LINEAR_MIPMAP_LINEAR;
					case FilterOptions.Point:
						// linear min, point mip
						return Gl.GL_LINEAR_MIPMAP_NEAREST;
					case FilterOptions.None:
						// linear, no mip
						return Gl.GL_LINEAR;
				}
					break;

				case FilterOptions.Point:
				case FilterOptions.None:
				switch(mipFilter) {
					case FilterOptions.Anisotropic:
					case FilterOptions.Linear:
						// nearest min, linear mip
						return Gl.GL_NEAREST_MIPMAP_LINEAR;
					case FilterOptions.Point:
						// nearest min, point mip
						return Gl.GL_NEAREST_MIPMAP_NEAREST;
					case FilterOptions.None:
						// nearest min, no mip
						return Gl.GL_NEAREST;
				}
					break;
			}

			// should never get here, but make the compiler happy
			return 0;
		}

		/// <summary>
		///		Convenience method for VBOs
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private IntPtr BUFFER_OFFSET(int i) {
			return new IntPtr(i);
		}

		#endregion Private methods

    }
}
