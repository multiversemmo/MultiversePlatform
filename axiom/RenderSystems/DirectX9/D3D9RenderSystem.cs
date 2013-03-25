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
using System.Windows.Forms;
using System.Runtime.InteropServices;
using log4net;
using Axiom.Collections;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.Media;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Diagnostics;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using FogMode = Axiom.Graphics.FogMode;
using LightType = Axiom.Graphics.LightType;
using StencilOperation = Axiom.Graphics.StencilOperation;
using TextureFiltering = Axiom.Graphics.TextureFiltering;

namespace Axiom.RenderSystems.DirectX9 {
	/// <summary>
	/// DirectX9 Render System implementation.
	/// </summary>
	public class D3D9RenderSystem : RenderSystem {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(D3D9RenderSystem));

		public static readonly Matrix4 ProjectionClipSpace2DToImageSpacePerspective = new Matrix4(
			0.5f,    0,  0, -0.5f, 
			0, -0.5f,  0, -0.5f, 
			0,    0,  0,   1f,
			0,    0,  0,   1f);

		public static readonly Matrix4 ProjectionClipSpace2DToImageSpaceOrtho = new Matrix4(
			-0.5f,    0,  0, -0.5f, 
			0, 0.5f,  0, -0.5f, 
			0,    0,  0,   1f,
			0,    0,  0,   1f);
        
		/// <summary>
		///    Reference to the Direct3D device.
		/// </summary>
		protected D3D.Device device;
		/// <summary>
		///    Direct3D capability structure.
		/// </summary>
		protected D3D.Caps d3dCaps;
		/// <summary>
		///    Signifies whether the basic states are initialized.
		/// </summary>
        protected bool basicStatesInitialized = false;
		protected bool isFirstWindow = true;

		/// <summary>
		///		Should we use the W buffer? (16 bit color only).
		/// </summary>
		public bool useWBuffer;

		/// <summary>
		///    Number of streams used last frame, used to unbind any buffers not used during the current operation.
		/// </summary>
		protected int lastVertexSourceCount;

		// stores texture stage info locally for convenience
        // note that we need to create this before we create the cache, so we 
        // can handle the disable pass, but we also clobber it when we call
        // CreateAndApplyCache
		internal D3DTextureStageDesc[] texStageDesc = new D3DTextureStageDesc[Config.MaxTextureLayers];

		protected int primCount;
		protected int renderCount = 0;

		// temp fields for tracking render states
		protected bool lightingEnabled;

		const int MAX_LIGHTS = 8;
		protected Axiom.Core.Light[] lights = new Axiom.Core.Light[MAX_LIGHTS];

		protected D3DGpuProgramManager gpuProgramMgr;

		/// Saved last view matrix
		protected Matrix4 viewMatrix = Matrix4.Identity;

        protected bool deviceLost = false;
        protected Surface cursorSurface = null;
        protected System.Drawing.Point cursorHotSpot;
        public D3DRenderWindow primaryWindow = null;
        protected List<RenderWindow> secondaryWindows = 
            new List<RenderWindow>();
        public D3DRenderWindow PrimaryWindow { get { return primaryWindow; } }

        Driver activeD3DDriver = null;
        protected Dictionary<Format, DepthFormat> depthStencilCache = 
            new Dictionary<Format, DepthFormat>();

        Dictionary<string, ConfigOption> options = 
            new Dictionary<string, ConfigOption>();

        bool vsync = true;
        bool useNVPerfHUD = true;
        MultiSampleType fsaa = MultiSampleType.None;
        int fsaaQuality = 0;
        bool multiThreaded = false;

        CursorProperties cursorProperties;
        /// <summary>
        ///   This class keeps track of the cursor properties, so that after 
        ///   restoring the device, we can restore the cursor as well.
        /// </summary>
        protected struct CursorProperties
        {
            public Axiom.Core.Texture texture;
            public System.Drawing.Rectangle section;
            public System.Drawing.Point hotSpot;
        }

        ///
        /// Cached values to minimize the number of DirectX calls
        ///

        public class CachedLight {
            public bool enabled = false;
            public LightType type = LightType.Point;
			public ColorEx diffuse = ColorEx.White;
			public ColorEx specular = ColorEx.Black;
			public float range = 100000;
			public float attenuationConst = 1.0f;
			public float attenuationLinear = 0.0f;
			public float attenuationQuad = 0.0f;
			public float spotInner = 30.0f;
			public float spotOuter = 40.0f;
			public float spotFalloff = 1.0f;
            public Axiom.MathLib.Vector3 position = Axiom.MathLib.Vector3.Zero;
			public Axiom.MathLib.Vector3 direction = Axiom.MathLib.Vector3.UnitZ;
        }
        
        public class DirectXCache {
            
            public SceneDetailLevel rasterizationMode = SceneDetailLevel.Solid;

            public bool writeEnabledRed = true;
            public bool writeEnabledGreen = true;
            public bool writeEnabledBlue = true;
            public bool writeEnabledAlpha = true;

            public bool alphaTestEnable = false;
            public CompareFunction alphaFunction = CompareFunction.AlwaysPass;
            public byte referenceAlpha = (byte)0;

            public bool colorVertex = true;

            public Shading shadingMode = Shading.Flat;

            public CullingMode cullingMode = CullingMode.CounterClockwise;
            public bool cullingFlip = true;

            public float constantBias = 1.0f;
            public float slopeScaleBias = 1.0f;

            public bool pointSpriteEnable = true;

            public bool pointScaleEnable = false;
            public float pointSize = 0f;
            public float pointSizeMin = 0f;
            public float pointSizeMax = 0f;

            public CompareFunction zBufferFunction = CompareFunction.AlwaysFail;
            public bool zBufferWriteEnable = false;

            public bool depthCheck = true;
        
            public bool alphaBlendEnable = false;
            public SceneBlendFactor sourceBlend = SceneBlendFactor.OneMinusSourceColor;
            public SceneBlendFactor destinationBlend = SceneBlendFactor.OneMinusSourceColor;

            // Sampler state caches - - there may be a different
            // number of these than texture units

            public readonly int maxSamplerState = 16;
            public bool[] samplerStateInitialized;
            public int[] textureLayerAnisotropy;
            public D3D.TextureFilter[,] textureUnitFilteringFilter;

            public TextureAddressing[] textureAddressingModeU;
            public TextureAddressing[] textureAddressingModeV;
            public TextureAddressing[] textureAddressingModeW;

            public ColorEx[] textureBorderColor;
            
            public float[] mipMapLODBias;
            
            // Texture state caches
            
            public int[] textureCoordinateIndex;
            public bool[] usingTextureMatrix;
            public Matrix4[] textureMatrix;
        
            public D3D.TextureOperation[] colorOperation;
            public D3D.TextureOperation[] alphaOperation;
            public D3D.TextureArgument[] colorArgument1;
            public D3D.TextureArgument[] colorArgument2;
            public D3D.TextureArgument[] alphaArgument1;
            public D3D.TextureArgument[] alphaArgument2;
            public D3D.TextureTransform[] textureTransformOp;

            public int textureFactor = -1;

            public CachedLight[] cachedLights;

            public Axiom.MathLib.Matrix4 worldMatrix = Matrix4.Identity;
            public Axiom.MathLib.Matrix4 viewMatrix = Matrix4.Identity;
            public Axiom.MathLib.Matrix4 projectionMatrix = Matrix4.Identity;


            public ColorEx ambient = ColorEx.Black;

            public ColorEx materialAmbient = ColorEx.Black;
            public ColorEx materialDiffuse = ColorEx.Black;
            public ColorEx materialSpecular = ColorEx.Black;
            public ColorEx materialEmissive = ColorEx.Black;
            public float materialShininess = 0.0f;
            public TrackVertexColor materialTracking = TrackVertexColor.None;

            public VertexShader vertexShader = null;
            public PixelShader pixelShader = null;

            public D3DHardwareVertexBuffer[] vertexBuffer;
            public bool[] streamNull;

            public IndexBuffer indexBuffer = null;
        
            public Axiom.Graphics.VertexDeclaration vertexDeclaration = null;
        
            public bool useConstantCache = true;
            public bool useSkipRange = false;
        
            public readonly int skipRange = 2;

            public Object[] intShaderConstants;
            public float[] floatShaderConstants;
            public Object[] intPixelConstants;
            public float[] floatPixelConstants;

            public bool fogEnable = true;
            public FogMode fogMode = FogMode.None;
            public ColorEx fogColor = ColorEx.Black;
            public float fogDensity = 0.0f;
            public float fogStart = 0.0f;
            public float fogEnd = 0.0f;
            public D3D.FogMode fogVertexMode = D3D.FogMode.None;
            public D3D.FogMode fogTableMode = D3D.FogMode.None;

            public DirectXCache(D3D.Caps d3dCaps) {

                samplerStateInitialized = new bool[maxSamplerState];
                textureLayerAnisotropy = new int[maxSamplerState];
                textureAddressingModeU = new TextureAddressing[maxSamplerState];
                textureAddressingModeV = new TextureAddressing[maxSamplerState];
                textureAddressingModeW = new TextureAddressing[maxSamplerState];
                textureUnitFilteringFilter = new D3D.TextureFilter[maxSamplerState, 3];
                textureBorderColor = new ColorEx[maxSamplerState];
                mipMapLODBias = new float[maxSamplerState];

                for (int i=0; i<maxSamplerState; i++) {
                    textureLayerAnisotropy[i] = 1;
                    textureAddressingModeU[i] = TextureAddressing.Wrap;
                    textureAddressingModeV[i] = TextureAddressing.Wrap;
                    textureAddressingModeW[i] = TextureAddressing.Wrap;
                    for (int j=0; j<3; j++)
                        textureUnitFilteringFilter[i, j] = D3D.TextureFilter.Linear;
                    textureBorderColor[i] = ColorEx.Black;
                }
                
                textureCoordinateIndex = new int[d3dCaps.MaxSimultaneousTextures];
                usingTextureMatrix = new bool[d3dCaps.MaxSimultaneousTextures];
                textureMatrix = new Matrix4[d3dCaps.MaxSimultaneousTextures];

                colorOperation = new D3D.TextureOperation[d3dCaps.MaxSimultaneousTextures];
                alphaOperation = new D3D.TextureOperation[d3dCaps.MaxSimultaneousTextures];
                colorArgument1 = new D3D.TextureArgument[d3dCaps.MaxSimultaneousTextures];
                colorArgument2 = new D3D.TextureArgument[d3dCaps.MaxSimultaneousTextures];
                alphaArgument1 = new D3D.TextureArgument[d3dCaps.MaxSimultaneousTextures];
                alphaArgument2 = new D3D.TextureArgument[d3dCaps.MaxSimultaneousTextures];
                textureTransformOp = new D3D.TextureTransform[d3dCaps.MaxSimultaneousTextures];

                for (int i=0; i<d3dCaps.MaxSimultaneousTextures; i++) {
                    textureMatrix[i] = Matrix4.Identity;
                    colorOperation[i] = D3D.TextureOperation.Disable;
                    alphaOperation[i] = D3D.TextureOperation.Disable;
                    colorArgument1[i] = D3D.TextureArgument.Complement;
                    colorArgument2[i] = D3D.TextureArgument.Complement;
                    alphaArgument1[i] = D3D.TextureArgument.Complement;
                    alphaArgument2[i] = D3D.TextureArgument.Complement;
                }

                vertexBuffer = new D3DHardwareVertexBuffer[d3dCaps.MaxStreams];
                streamNull = new bool[d3dCaps.MaxStreams];
                int numLights = d3dCaps.MaxActiveLights;
                if (numLights <= 0)
                {
                    numLights = 8;
                }
                cachedLights = new CachedLight[numLights];
                for (int i=0; i<numLights; i++)
                    cachedLights[i] = new CachedLight();
            }

            public void ApplyCache(D3D9RenderSystem renderSystem, D3D.Device device, D3D.Caps d3dCaps) {

                log.Info("DirectXCache.ApplyCache: Starting to apply cache settings to device");

                renderSystem.SetSceneDetailLevel(rasterizationMode);

                renderSystem.SetColorBufferWriteEnabledInternal(writeEnabledRed, writeEnabledGreen, writeEnabledBlue, writeEnabledAlpha);

                device.RenderState.AlphaTestEnable = alphaTestEnable;
                device.RenderState.AlphaFunction = D3DHelper.ConvertEnum(alphaFunction);
                
                device.RenderState.ReferenceAlpha = referenceAlpha;

                renderSystem.SetRenderState(RenderStates.ColorVertex, colorVertex);

                device.RenderState.ShadeMode = D3DHelper.ConvertEnum(shadingMode);

                device.RenderState.CullMode = D3DHelper.ConvertEnum(cullingMode, cullingFlip);

                if (d3dCaps.RasterCaps.SupportsDepthBias)
                    renderSystem.SetRenderState(RenderStates.DepthBias, -constantBias / 250000.0f);

                if (d3dCaps.RasterCaps.SupportsSlopeScaleDepthBias)
                    renderSystem.SetRenderState(RenderStates.SlopeScaleDepthBias, -slopeScaleBias);

                renderSystem.SetRenderState(RenderStates.PointSpriteEnable, pointSpriteEnable);
                renderSystem.SetRenderState(RenderStates.PointScaleEnable, pointScaleEnable);
                
                renderSystem.SetRenderState(RenderStates.PointSize, pointSize);
                renderSystem.SetRenderState(RenderStates.PointSizeMin, pointSizeMin);
                renderSystem.SetRenderState(RenderStates.PointSizeMax, pointSizeMax);

                device.RenderState.ZBufferFunction = D3DHelper.ConvertEnum(zBufferFunction);

                if(depthCheck) {
                    // use w-buffer if available
                    if(renderSystem.useWBuffer && d3dCaps.RasterCaps.SupportsWBuffer)
                        device.RenderState.UseWBuffer = true;
                    else
                        device.RenderState.ZBufferEnable = true;
                        
                }
                else
                    device.RenderState.ZBufferEnable = false;
                
                zBufferWriteEnable = device.RenderState.ZBufferWriteEnable;

                device.RenderState.AlphaBlendEnable = alphaBlendEnable;
                device.RenderState.SourceBlend = D3DHelper.ConvertEnum(sourceBlend);
                device.RenderState.DestinationBlend = D3DHelper.ConvertEnum(destinationBlend);

                for (int i=0; i<d3dCaps.MaxSimultaneousTextures; i++) {
                    device.TextureState[i].TextureCoordinateIndex = D3DHelper.ConvertEnum(renderSystem.texStageDesc[i].autoTexCoordType, d3dCaps) | textureCoordinateIndex[i];

                    renderSystem.texStageDesc[i].autoTexCoordType = TexCoordCalcMethod.None;

                    renderSystem.SetTextureMatrixInternal(i, textureMatrix[i]);

                    device.TextureState[i].ColorOperation = colorOperation[i];
                    device.TextureState[i].AlphaOperation = alphaOperation[i];
                    
                    device.TextureState[i].ColorArgument1 = colorArgument1[i];
                    device.TextureState[i].ColorArgument2 = colorArgument2[i];
                    device.TextureState[i].AlphaArgument1 = alphaArgument1[i];
                    device.TextureState[i].AlphaArgument2 = alphaArgument2[i];
                    device.TextureState[i].TextureTransform = textureTransformOp[i];
                    for (int j=0; j<3; j++)
                        renderSystem.SetTextureUnitFilteringInternal(i, (FilterType)j, textureUnitFilteringFilter[i,j]);
                }
                device.RenderState.TextureFactor = textureFactor;;
                
                device.Transform.World = renderSystem.MakeD3DMatrix(worldMatrix);
                renderSystem.SetViewMatrixInternal(viewMatrix);
                renderSystem.SetProjectionMatrixInternal(projectionMatrix, false);

                device.RenderState.Ambient = ambient.ToColor();

                // create a new material based on the supplied params
                D3D.Material mat = new D3D.Material();
                mat.Diffuse = materialDiffuse.ToColor();
                mat.Ambient = materialAmbient.ToColor();
                mat.Specular = materialSpecular.ToColor();
                mat.Emissive = materialEmissive.ToColor();
                mat.SpecularSharpness = materialShininess;

                // set the current material
                device.Material = mat;

                device.VertexShader = vertexShader;
                device.PixelShader = pixelShader;

                device.Indices = indexBuffer;

                for (int i=0; i<d3dCaps.MaxStreams; i++) {
                    streamNull[i] = true;
                    device.SetStreamSource(i, null, 0, 0);
                }

                device.VertexDeclaration = null;

                device.RenderState.FogEnable = fogEnable;
                device.RenderState.FogColor = fogColor.ToColor();
                device.RenderState.FogDensity = fogDensity;
                device.RenderState.FogStart = fogStart;
                device.RenderState.FogEnd = fogEnd;
                device.RenderState.FogVertexMode = D3DHelper.ConvertEnum(fogMode);
                device.RenderState.FogTableMode = D3D.FogMode.None; 

                intShaderConstants = new Object[d3dCaps.MaxVertexShaderConst];
                floatShaderConstants = new float[d3dCaps.MaxVertexShaderConst * 4];
                intPixelConstants = new Object[d3dCaps.MaxVertexShaderConst];
                floatPixelConstants = new float[d3dCaps.MaxVertexShaderConst * 4];

                for (int i=0; i<cachedLights.Length; i++) {
                    CachedLight cachedLight = cachedLights[i];
                    device.Lights[i].Enabled = cachedLight.enabled;
                    switch(cachedLight.type) {
					case LightType.Point:
						device.Lights[i].Type = D3D.LightType.Point;
						break;
					case LightType.Directional:
						device.Lights[i].Type = D3D.LightType.Directional;
						break;
					case LightType.Spotlight:
						device.Lights[i].Type = D3D.LightType.Spot;
                        break;
                    }
                    device.Lights[i].Falloff = cachedLight.spotFalloff;
                    device.Lights[i].InnerConeAngle = MathUtil.DegreesToRadians(cachedLight.spotInner);
                    device.Lights[i].OuterConeAngle = MathUtil.DegreesToRadians(cachedLight.spotOuter);
                    device.Lights[i].Diffuse = cachedLight.diffuse.ToColor();
                    device.Lights[i].Specular = cachedLight.specular.ToColor();
                    Axiom.MathLib.Vector3 vec = cachedLight.position;
                    device.Lights[i].Position = new DX.Vector3(vec.x, vec.y, vec.z);
                    vec = cachedLight.direction;
                    device.Lights[i].Direction = new DX.Vector3(vec.x, vec.y, vec.z);
                    device.Lights[i].Range = cachedLight.range;
                    device.Lights[i].Attenuation0 = cachedLight.attenuationConst;
                    device.Lights[i].Attenuation1 = cachedLight.attenuationLinear;
                    device.Lights[i].Attenuation2 = cachedLight.attenuationQuad;

                    device.RenderState.Lighting = renderSystem.lightingEnabled;
                }

                log.Info("DirectXCache.ApplyCache: Finished applying cache settings to device");
            }

            public void EnsureSamplerStateInitialized(int i, D3D.Device device, D3D.Caps d3dCaps) {
                samplerStateInitialized[i] = true;
                if (textureLayerAnisotropy[i] > d3dCaps.MaxAnisotropy)
                    textureLayerAnisotropy[i] = d3dCaps.MaxAnisotropy;
                device.SamplerState[i].MaxAnisotropy = textureLayerAnisotropy[i];

                device.SamplerState[i].AddressU = D3DHelper.ConvertEnum(textureAddressingModeU[i]);
                device.SamplerState[i].AddressV = D3DHelper.ConvertEnum(textureAddressingModeV[i]);
                device.SamplerState[i].AddressW = D3DHelper.ConvertEnum(textureAddressingModeW[i]);
                device.SamplerState[i].MipMapLevelOfDetailBias = mipMapLODBias[i];
                device.SamplerState[i].MinFilter = textureUnitFilteringFilter[i, (int)FilterType.Min];
                device.SamplerState[i].MagFilter = textureUnitFilteringFilter[i, (int)FilterType.Mag];
                device.SamplerState[i].MipFilter = textureUnitFilteringFilter[i, (int)FilterType.Mip];
                device.SamplerState[i].BorderColor = textureBorderColor[i].ToColor();
            }
        }
        
        protected DirectXCache cache = null;

        public void CreateAndApplyCache() {
            texStageDesc = new D3DTextureStageDesc[Config.MaxTextureLayers];
            cache = new DirectXCache(d3dCaps);
            if (cache != null)
                cache.ApplyCache(this, device, d3dCaps);
        }
        
        public struct ZBufferFormat {
            public ZBufferFormat(DepthFormat f, MultiSampleType m) {
                this.format = f;
                this.multisample = m;
            }
            public DepthFormat format;
            public MultiSampleType multisample;
        }
        protected Dictionary<ZBufferFormat, Surface> zBufferCache =
            new Dictionary<ZBufferFormat, Surface>();

		/// <summary>
		///		Temp D3D vector to avoid constant allocations.
		/// </summary>
		private Microsoft.DirectX.Vector4 tempVec = new Microsoft.DirectX.Vector4();

		public D3D9RenderSystem() {
			InitConfigOptions();

            // init the texture stage descriptions
			for(int i = 0; i < Config.MaxTextureLayers; i++) {
				texStageDesc[i].autoTexCoordType = TexCoordCalcMethod.None;
				texStageDesc[i].coordIndex = 0;
				texStageDesc[i].texType = D3DTexType.Normal;
				texStageDesc[i].tex = null;
			}
		}

		#region Implementation of RenderSystem

		public override ColorEx AmbientLight {
			get {
				return cache.ambient;
			}
			set {
				if (cache.ambient.CompareTo(value) != 0) {
                    cache.ambient = value.Clone();
                    device.RenderState.Ambient = value.ToColor();
                }
			}
		}
	
		public override bool LightingEnabled {
			get {
				return lightingEnabled;
			}
			set {
				if(lightingEnabled != value) 
				{
					lightingEnabled = value;
                    device.RenderState.Lighting = value;
				}

			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		public override bool NormalizeNormals {
			get {
				return device.RenderState.NormalizeNormals;
			}
			set {
				device.RenderState.NormalizeNormals = value;
			}
		}

		public override Shading ShadingMode {
			get {
				return cache.shadingMode;
			}
			set {
				if (cache.shadingMode != value) {
                    cache.shadingMode = value;
                    device.RenderState.ShadeMode = D3DHelper.ConvertEnum(value);
                }
			}
		}
	
		public override bool StencilCheckEnabled {
			get {
				return device.RenderState.StencilEnable;
			}
			set {
				device.RenderState.StencilEnable = value;
			}
		}

        public bool DeviceLost {
            get {
                return deviceLost;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		protected void SetVertexBufferBinding(VertexBufferBinding binding) {
			Dictionary<ushort, HardwareVertexBuffer> bindings = binding.Bindings;

			// TODO: Optimize to remove enumeration if possible, although with so few iterations it may never make a difference
            int c = 0;
            foreach (ushort stream in bindings.Keys) {
                D3DHardwareVertexBuffer buffer = (D3DHardwareVertexBuffer)bindings[stream];
                if (cache.streamNull[c] || cache.vertexBuffer[c] != buffer) {
                    cache.vertexBuffer[c] = buffer;
                    cache.streamNull[c] = false;
                    device.SetStreamSource(stream, buffer.D3DVertexBuffer, 0, buffer.VertexSize);
                }
                c++;
                lastVertexSourceCount++;
			}

			// Unbind any unused sources
			for(int i = bindings.Count; i < lastVertexSourceCount; i++) {
                if (!cache.streamNull[i]) {
                    cache.streamNull[i] = true;
                    device.SetStreamSource(i, null, 0, 0);
                }
			}
            
			lastVertexSourceCount = bindings.Count;
		}

		/// <summary>
		///		Helper method for setting the current vertex declaration.
		/// </summary>
		protected void SetVertexDeclaration(Axiom.Graphics.VertexDeclaration decl) {

            if (!Object.ReferenceEquals(cache.vertexDeclaration, decl)) {
                cache.vertexDeclaration = decl;
                D3DVertexDeclaration d3dVertDecl = (D3DVertexDeclaration)decl;
                device.VertexDeclaration = d3dVertDecl.D3DVertexDecl;
            }
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="buffers"></param>
		/// <param name="color"></param>
		/// <param name="depth"></param>
		/// <param name="stencil"></param>
		public override void ClearFrameBuffer(FrameBuffer buffers, ColorEx color, float depth, int stencil) {
			D3D.ClearFlags flags = 0;

			if((buffers & FrameBuffer.Color) > 0) {
				flags |= D3D.ClearFlags.Target;
			}	
			if((buffers & FrameBuffer.Depth) > 0) {
				flags |= D3D.ClearFlags.ZBuffer;
			}	
			// Only try to clear the stencil buffer if supported
			if((buffers & FrameBuffer.Stencil) > 0 
				&& caps.CheckCap(Capabilities.StencilBuffer)) {

				flags |= D3D.ClearFlags.Stencil;
			}	

			// clear the device using the specified params
            device.Clear(flags, (int)color.ToARGB(), depth, stencil);
        }
	
		/// <summary>
		///     Create a D3D specific render texture.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
        //public override RenderTexture CreateRenderTexture(string name, int width, int height, 
        //                                                  TextureType ttype, PixelFormat format, 
        //                                                  Dictionary<string, string> miscParams) {
        //    D3DRenderTexture renderTexture = new D3DRenderTexture(name, width, height, TextureType.TwoD, format);
        //    AttachRenderTarget(renderTexture);
        //    return renderTexture;
        //}

		/// <summary>
		///		Returns a Direct3D implementation of a hardware occlusion query.
		/// </summary>
		/// <returns></returns>
		public override IHardwareOcclusionQuery CreateHardwareOcclusionQuery() {
			return new D3DHardwareOcclusionQuery(device);
		}

		public override RenderWindow CreateRenderWindow(string name, int width, int height, bool isFullscreen, params object[] miscParams) {
    		// Check we're not creating a secondary window when the primary
	    	// was fullscreen
		    if (primaryWindow != null && primaryWindow.IsFullScreen) {
    		    throw new Exception("Cannot create secondary windows when the primary is full screen");
	    	} else if (primaryWindow != null && isFullscreen) {
                throw new Exception("Cannot create full screen secondary windows");
		    }

        	// Make sure we don't already have a render target of the 
		    // same name as the one supplied
            foreach (RenderTarget x in renderTargets) {
                if (x.Name == name)
                    throw new Exception("A render target of the same name '" + name +
                                        "' already exists.  You cannot create a new window with this name.");
            }

            RenderWindow win = new D3DRenderWindow(activeD3DDriver, primaryWindow != null);
            // create the window
            win.Create(name, width, height, isFullscreen, miscParams);
			// add the new render target
			AttachRenderTarget(win);

            // If this is the first window, get the D3D device and create the texture manager
		    if (primaryWindow == null) {
		        primaryWindow = (D3DRenderWindow)win;
                device = (Device)win.GetCustomAttribute("D3DDEVICE");

                // by creating our texture manager, singleton TextureManager will hold our implementation
                textureManager = new D3DTextureManager(device);

                // by creating our Gpu program manager, singleton GpuProgramManager will hold our implementation
                gpuProgramMgr = new D3DGpuProgramManager(device);

                // intializes the HardwareBufferManager singleton
                hardwareBufferManager = new D3DHardwareBufferManager(device);

                // Initialise the capabilities structures
                InitCapabilities();
                		
                CreateAndApplyCache();
            } else {
                secondaryWindows.Add(win);
            }
			return win;
		}
		
#if NOT // This is the older Axiom code where we did the RenderWindow stuff in the RenderSystem instead
		private D3D.Device InitDevice(bool isFullscreen, bool depthBuffer, int width, int height, int colorDepth, Control target) {
			if(device != null) {
				return device;
			}

			// we don't care about event handlers
			Device.IsUsingEventHandlers = false;

			D3D.Device newDevice;

			PresentParameters presentParams = CreatePresentationParams(isFullscreen, depthBuffer, width, height, colorDepth);

		    // create the D3D Device, trying for the best vertex support first, and settling for less if necessary
		    try {
			    // hardware vertex processing
                int adapterNum = 0;
                DeviceType type = DeviceType.Hardware;
#if DEBUG
                for ( int i = 0; i < Manager.Adapters.Count; i++ ) {
                    if (Manager.Adapters[i].Information.Description == "NVIDIA NVPerfHUD")
                    {
                        adapterNum = i;
                        type = DeviceType.Reference;
                    }
                }
#endif
				// use this with NVPerfHUD
				newDevice = new D3D.Device(adapterNum, type, target, CreateFlags.HardwareVertexProcessing, presentParams);
    			// newDevice = new D3D.Device(0, DeviceType.Hardware, target, CreateFlags.HardwareVertexProcessing, presentParams);
			}
			catch(Exception) {
				try {
					// doh, how bout mixed vertex processing
					newDevice = new D3D.Device(0, DeviceType.Hardware, target, CreateFlags.MixedVertexProcessing, presentParams);
				}
				catch(Exception) {
					// what the...ok, how bout software vertex procssing.  if this fails, then I don't even know how they are seeing
					// anything at all since they obviously don't have a video card installed
					newDevice = new D3D.Device(0, DeviceType.Hardware, target, CreateFlags.SoftwareVertexProcessing, presentParams);
				}
			}
		
			// CMH - end
		
		
			// save the device capabilites
			d3dCaps = newDevice.DeviceCaps;
		
            // by creating our texture manager, singleton TextureManager will hold our implementation
			textureMgr = new D3DTextureManager(newDevice);
		
			// by creating our Gpu program manager, singleton GpuProgramManager will hold our implementation
			gpuProgramMgr = new D3DGpuProgramManager(newDevice);
		
			// intializes the HardwareBufferManager singleton
			hardwareBufferManager = new D3DHardwareBufferManager(newDevice);
            
            return newDevice;
		}

        private static PresentParameters CreatePresentationParams(bool isFullscreen, bool depthBuffer, int width, int height, int colorDepth)
		{
			// if this is the first window, get the device and do other initialization
			// CMH - 4/24/2004 start
			/// get the Direct3D.Device params
			PresentParameters presentParams = new PresentParameters();
			presentParams.Windowed = !isFullscreen;
			presentParams.BackBufferCount = 0;
			presentParams.EnableAutoDepthStencil = depthBuffer;

			if (isFullscreen)
			{
				presentParams.BackBufferWidth = width;
				presentParams.BackBufferHeight = height;
			}
			else
			{ // Save us some bytes.
				presentParams.BackBufferWidth = 16;
				presentParams.BackBufferHeight = 16;
			}

			presentParams.MultiSample = MultiSampleType.None;
			presentParams.SwapEffect = SwapEffect.Discard;
			// TODO: Check vsync setting
			presentParams.PresentationInterval = PresentInterval.Immediate;

			// supports 16 and 32 bit color
			if (colorDepth == 16)
			{
				presentParams.BackBufferFormat = Format.R5G6B5;
			}
			else
			{
				presentParams.BackBufferFormat = Format.X8R8G8B8;
			}

			if (colorDepth > 16)
			{
				// check for 24 bit Z buffer with 8 bit stencil (optimal choice)
				if (!Manager.CheckDeviceFormat(0, DeviceType.Hardware, presentParams.BackBufferFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D24S8))
				{
					// doh, check for 32 bit Z buffer then
					if (!Manager.CheckDeviceFormat(0, DeviceType.Hardware, presentParams.BackBufferFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D32))
					{
						// float doh, just use 16 bit Z buffer
						presentParams.AutoDepthStencilFormat = DepthFormat.D16;
					}
					else
					{
						// use 32 bit Z buffer
						presentParams.AutoDepthStencilFormat = DepthFormat.D32;
					}
				}
				else
				{
					if (Manager.CheckDepthStencilMatch(0, DeviceType.Hardware, presentParams.BackBufferFormat, presentParams.BackBufferFormat, DepthFormat.D24S8))
					{
						presentParams.AutoDepthStencilFormat = DepthFormat.D24S8;
					}
					else
					{
						presentParams.AutoDepthStencilFormat = DepthFormat.D24X8;
					}
				}
			}
			else
			{
				// use 16 bit Z buffer if they arent using true color
				presentParams.AutoDepthStencilFormat = DepthFormat.D16;
			}
			return presentParams;
		}
#endif


        public override void DestroyRenderTarget(string name) {
		    // Check in specialised lists
		    if (primaryWindow.Name == name) {
			    // We're destroying the primary window, so reset device and window
			    primaryWindow = null;
		    } else {
			    // Check secondary windows
			    foreach (RenderWindow window in secondaryWindows) {
				    if (window.Name == name) {
    				    secondaryWindows.Remove(window);
	    				break;
		    		}
			    }
            }

		    // Do the real removal
		    base.DestroyRenderTarget(name);

		    // Did we destroy the primary?
		    if (primaryWindow == null) {
    			// device is no longer valid, so free it all up
	    		FreeDevice();
		    }
    	}

        protected void FreeDevice() {
            if (device != null) {
                if (cursorSurface != null) {
                    cursorSurface.Dispose();
                    cursorSurface = null;
                }
                // FIXME: Need to do cleanup here
#if OGRE_CODE
			// Set all texture units to nothing to release texture surfaces
			_disableTextureUnitsFrom(0);
			// Unbind any vertex streams to avoid memory leaks
			for (unsigned int i = 0; i < mLastVertexSourceCount; ++i)
			{
				HRESULT hr = mpD3DDevice->SetStreamSource(i, NULL, 0, 0);
			}
			// Clean up depth stencil surfaces
			_cleanupDepthStencils();
#endif
                // Unbind any vertex streams to avoid memory leaks
                for (int i = 0; i < lastVertexSourceCount; ++i)
                    device.SetStreamSource(i, null, 0, 0);
                activeD3DDriver.Device = null;
                device.Dispose();
                device = null;
            }
        }

        public void NotifyDeviceLost() {
            log.Info("!!! Direct3D Device Lost!");
            deviceLost = true;
            // will have lost basic states
            basicStatesInitialized = false;
        }

		public override void Shutdown() {
			base.Shutdown();

            activeD3DDriver = null;
            // dispose of the device
            if (device != null) {
				device.Dispose();
			}

            if (gpuProgramMgr != null) {
                gpuProgramMgr.Dispose();
            }
            if (hardwareBufferManager != null) {
                hardwareBufferManager.Dispose();
            }
            if (textureManager != null) {
                textureManager.Dispose();
            }
        }

		/// <summary>
		///		Sets the rasterization mode to use during rendering.
		/// </summary>
		public override SceneDetailLevel RasterizationMode {
			get {
                return cache.rasterizationMode;
			}
			set {
				if (value == cache.rasterizationMode)
                    return;
                cache.rasterizationMode = value;
                SetSceneDetailLevel(value);
            }
        }
        
        public void SetSceneDetailLevel(SceneDetailLevel value) {
            switch(value) {
            case SceneDetailLevel.Points:
                device.RenderState.FillMode = FillMode.Point;
                break;
            case SceneDetailLevel.Wireframe:
                device.RenderState.FillMode = FillMode.WireFrame;
                break;
            case SceneDetailLevel.Solid:
                device.RenderState.FillMode = FillMode.Solid;
                break;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="func"></param>
		/// <param name="val"></param>
		public override void SetAlphaRejectSettings(CompareFunction func, byte val) {
			bool enabled = (func != CompareFunction.AlwaysPass);
            if (cache.alphaTestEnable != enabled) {
                cache.alphaTestEnable = enabled;
                device.RenderState.AlphaTestEnable = (func != CompareFunction.AlwaysPass);
            }
            if (cache.alphaFunction != func) {
                cache.alphaFunction = func;
                device.RenderState.AlphaFunction = D3DHelper.ConvertEnum(func);
            }
            if (cache.referenceAlpha != val) {
                cache.referenceAlpha = val;
                device.RenderState.ReferenceAlpha = val;
            }
		}

        public override Dictionary<string, ConfigOption> GetConfigOptions() {
            Dictionary<string, ConfigOption> rv = new Dictionary<string, ConfigOption>();
            foreach (KeyValuePair<string, ConfigOption> kvp in options)
                rv[kvp.Key] = kvp.Value;
            return rv;
        }

        public override void SetConfigOption(string name, string value)
        {
            if (options.ContainsKey(name))
                options[name].currentValue = value;

		    switch (name) {
                case "Anti Aliasing":
                    fsaaQuality = 0;
                    if (value == "None")
                    {
                        fsaa = MultiSampleType.None;
                    }
                    else
                    {
                        if (value.StartsWith("Quality"))
                        {
                            fsaa = MultiSampleType.NonMaskable;
                            string[] args = value.Split();
                            fsaaQuality = int.Parse(args[1]) - 1;
                        }
                        // this is support for maskable multisampling types.  I'm leaving it here for now
                        // even though it shouldn't get exercised.
                        else if (value.StartsWith("Level"))
                        {
                            string[] args = value.Split();
                            int tmp = int.Parse(args[1]);
                            fsaa = (MultiSampleType)tmp;
                        }
                    }
                    break;
                case "VSync":
                    vsync = (value == "Yes") ? true : false;
                    break;
                case "Allow NVPerfHUD":
                    useNVPerfHUD = (value == "Yes") ? true : false;
                    break;
                case "Multi-Threaded":
                    multiThreaded = (value == "Yes") ? true : false;
                    break;
                case "Full Screen":
                case "Video Mode":
                case "Rendering Device":
                default:
                    break;
			}
		}

        public string GetConfigOption(string name) {
            switch (name) {
                case "Anti aliasing":
                    if (fsaa == MultiSampleType.None)
                        return "None";
                    else if (fsaa == MultiSampleType.NonMaskable)
                        return string.Format("NonMaskable {0}", fsaaQuality + 1);
                    else
                        return string.Format("Level {0}", (int)fsaa);
                case "VSync":
                    return vsync ? "Yes" : "No";
                default:
                    return null;
            }
        }
       
        public override void SetDepthBias(float constantBias, float slopeScaleBias) {
            if (cache.constantBias != constantBias) {
                cache.constantBias = constantBias;
                if (d3dCaps.RasterCaps.SupportsDepthBias) {
                    // Negate bias since D3D is backward
                    // D3D also expresses the constant bias as an absolute value, rather than 
                    // relative to minimum depth unit, so scale to fit
                    constantBias = -constantBias / 250000.0f;
                    SetRenderState(RenderStates.DepthBias, constantBias);
                }
            }
            
            if (cache.slopeScaleBias != slopeScaleBias) {
                cache.slopeScaleBias = slopeScaleBias;
                if (d3dCaps.RasterCaps.SupportsSlopeScaleDepthBias) {
                    // Negate bias since D3D is backward
                    slopeScaleBias = -slopeScaleBias;
                    SetRenderState(RenderStates.SlopeScaleDepthBias, slopeScaleBias);
                }
            }
        }

        public override void SetColorBufferWriteEnabled(bool red, bool green, bool blue, bool alpha) {
            if (cache.writeEnabledRed == red &&
                cache.writeEnabledGreen == green &&
                cache.writeEnabledBlue == blue &&
                cache.writeEnabledAlpha == alpha)
                return;

            cache.writeEnabledRed = red;
            cache.writeEnabledGreen = green;
            cache.writeEnabledBlue = blue;
            cache.writeEnabledAlpha = alpha;

            SetColorBufferWriteEnabledInternal(red, green, blue, alpha);
        }

        public void SetColorBufferWriteEnabledInternal(bool red, bool green, bool blue, bool alpha) {
            D3D.ColorWriteEnable val = 0;
			if(red) {
				val |= ColorWriteEnable.Red;
			}
			if(green) {
				val |= ColorWriteEnable.Green;
			}
			if(blue) {
				val |= ColorWriteEnable.Blue;
			}
			if(alpha) {
				val |= ColorWriteEnable.Alpha;
			}

			device.RenderState.ColorWriteEnable = val;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="color"></param>
		/// <param name="density"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public override void SetFog(Axiom.Graphics.FogMode mode, ColorEx color, float density, float start, float end) {
			//log.InfoFormat("D3D9RenderSystem.SetFog: mode {0}, color {1}, density {2}, start {3}, end {4}",
            //    mode, color, density, start, end);
            // disable fog if set to none
			if(mode == FogMode.None) {
				if (cache.fogTableMode != D3D.FogMode.None) {
                    cache.fogTableMode = D3D.FogMode.None;
                    device.RenderState.FogTableMode = D3D.FogMode.None;
                }
				if (cache.fogVertexMode != D3D.FogMode.None) {
                    cache.fogVertexMode = D3D.FogMode.None;
                    device.RenderState.FogVertexMode = D3D.FogMode.None;
                }
				if (cache.fogEnable) {
                    cache.fogEnable = false;
                    device.RenderState.FogEnable = false;
                }
			}
			else {
				// enable fog
                D3D.FogMode d3dFogMode = D3DHelper.ConvertEnum(mode);
				if (!cache.fogEnable) {
                    cache.fogEnable = true;
                    device.RenderState.FogEnable = true;
                }
				if (cache.fogVertexMode != d3dFogMode) {
                    cache.fogVertexMode = d3dFogMode;
                    device.RenderState.FogVertexMode = d3dFogMode; 
                }
				if (cache.fogTableMode != D3D.FogMode.None) {
                    cache.fogTableMode = D3D.FogMode.None;
                    device.RenderState.FogTableMode = D3D.FogMode.None;
                }
				if (cache.fogColor.CompareTo(color) != 0) {
                    cache.fogColor = color.Clone();
                    device.RenderState.FogColor = color.ToColor();
                }
                if (cache.fogStart != start) {
                    cache.fogStart = start;
                    device.RenderState.FogStart = start;
                }
				if (cache.fogEnd != end) {
                    cache.fogEnd = end;
                    device.RenderState.FogEnd = end;
                }
                if (cache.fogDensity != density) {
                    cache.fogDensity = density;
                    device.RenderState.FogDensity = density;
                }
			}
		}

        public override RenderWindow Initialize(bool autoCreateWindow, string windowTitle) {
            return Initialize(autoCreateWindow, windowTitle, "");
        }
        
        public override RenderWindow Initialize(bool autoCreateWindow, string windowTitle, string initialLoadBitmap) {
			RenderWindow renderWindow = null;
            activeD3DDriver = D3DHelper.GetDriverInfo();
			if (autoCreateWindow) {
                Axiom.Configuration.DisplayMode mode = displayConfig.SelectedMode;

                if (mode == null)
                {
                    throw new Exception("No video mode is selected");
                }

				// create a default form window
				// DefaultForm newWindow = CreateDefaultForm(windowTitle, 0, 0, mode.Width, mode.Height, mode.FullScreen);

				// create the render window
				renderWindow = CreateRenderWindow("Main Window", (int)mode.Width, (int)mode.Height, mode.Fullscreen,
                                                  "colorDepth", mode.Depth, 
                                                  "FSAA", fsaa,
                                                  "FSAAQuality", fsaaQuality,
                                                  "vsync", vsync,
                                                  "title", windowTitle,
                                                  "useNVPerfHUD", useNVPerfHUD,
                                                  "multiThreaded", multiThreaded,
                                                  "initialLoadBitmap", initialLoadBitmap);
				
				// use W buffer when in 16 bit color mode
				useWBuffer = (renderWindow.ColorDepth == 16);

			}

            base.Initialize(autoCreateWindow, windowTitle);

			return renderWindow;
		}

#if NOT
		/// <summary>
		///		Creates a default form to use for a rendering target.
		/// </summary>
		/// <remarks>
		///		This is used internally whenever <see cref="Initialize"/> is called and autoCreateWindow is set to true.
		/// </remarks>
		/// <param name="windowTitle">Title of the window.</param>
		/// <param name="top">Top position of the window.</param>
		/// <param name="left">Left position of the window.</param>
		/// <param name="width">Width of the window.</param>
		/// <param name="height">Height of the window</param>
		/// <param name="fullScreen">Prepare the form for fullscreen mode?</param>
		/// <returns>A form suitable for using as a rendering target.</returns>
		private DefaultForm CreateDefaultForm(string windowTitle, int top, int left, int width, int height, bool fullScreen) {
			DefaultForm form = new DefaultForm();

			form.ClientSize = new System.Drawing.Size(width,height);
			form.MaximizeBox = true;
			form.MinimizeBox = true;
			form.StartPosition = FormStartPosition.CenterScreen;
			form.BringToFront();

			if(fullScreen) {
				form.Top = 0;
				form.Left = 0;
				form.FormBorderStyle = FormBorderStyle.None;
				form.WindowState = FormWindowState.Maximized;
				form.TopMost = true;
				form.TopLevel = true;
			}
			else {
				form.Top = top;
				form.Left = left;
				form.FormBorderStyle = FormBorderStyle.FixedSingle;
				form.WindowState = FormWindowState.Normal;
				form.Text = windowTitle;
			}

			return form;
		}
#endif
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fov"></param>
		/// <param name="aspectRatio"></param>
		/// <param name="near"></param>
		/// <param name="far"></param>
		/// <param name="forGpuPrograms"></param>
		/// <returns></returns>
		public override Matrix4 MakeOrthoMatrix(float fov, float aspectRatio, float near, float far, bool forGpuPrograms) {
			float thetaY = MathUtil.DegreesToRadians(fov / 2.0f);
			float tanThetaY = MathUtil.Tan(thetaY);
			float tanThetaX = tanThetaY * aspectRatio;

			float halfW = tanThetaX * near;
			float halfH = tanThetaY * near;

			float w = 1.0f / (halfW);
			float h = 1.0f / (halfH);
			float q = 0;
			
			if(far != 0) {
				q = 1.0f / (far - near);
			}

			Matrix4 dest = Matrix4.Zero;
			dest.m00 = w;
			dest.m11 = h;
			dest.m22 = q;
			dest.m23 = -near / (far - near);
			dest.m33 = 1;

			if(forGpuPrograms) {
				dest.m22 = - dest.m22;
			}

			return dest;
		}

        public override Axiom.MathLib.Matrix4 ConvertProjectionMatrix(Matrix4 mat, bool forGpuProgram) {
            Matrix4 dest = new Matrix4(mat.m00, mat.m01, mat.m02, mat.m03, 
                                       mat.m10, mat.m11, mat.m12, mat.m13, 
                                       mat.m20, mat.m21, mat.m22, mat.m23, 
                                       mat.m30, mat.m31, mat.m32, mat.m33);

            // Convert depth range from [-1,+1] to [0,1]
            dest.m20 = (dest.m20 + dest.m30) / 2;
            dest.m21 = (dest.m21 + dest.m31) / 2;
            dest.m22 = (dest.m22 + dest.m32) / 2;
            dest.m23 = (dest.m23 + dest.m33) / 2;

            if (!forGpuProgram) {
                // Convert right-handed to left-handed
                dest.m02 = -dest.m02;
                dest.m12 = -dest.m12;
                dest.m22 = -dest.m22;
                dest.m32 = -dest.m32;
            }

            return dest;
        }

		/// <summary>
		///		
		/// </summary>
		/// <param name="fov"></param>
		/// <param name="aspectRatio"></param>
		/// <param name="near"></param>
		/// <param name="far"></param>
		/// <param name="forGpuProgram"></param>
		/// <returns></returns>
		public override Axiom.MathLib.Matrix4 MakeProjectionMatrix(float fov, float aspectRatio, float near, float far, bool forGpuProgram) {
			float theta = MathUtil.DegreesToRadians(fov * 0.5f);
			float h = 1 / MathUtil.Tan(theta);
			float w = h / aspectRatio;
			float q = 0;
			float qn = 0;

			if(far == 0) {
				q = 1 - Frustum.InfiniteFarPlaneAdjust;
				qn = near * (Frustum.InfiniteFarPlaneAdjust - 1);
			}
			else {
				q = far / (far - near);
				qn = -q * near;
			}

			Matrix4 dest = Matrix4.Zero;

			dest.m00 = w;
			dest.m11 = h;

			if(forGpuProgram) {
				dest.m22 = -q;
				dest.m32 = -1.0f;
			}
			else {
				dest.m22 = q;
				dest.m32 = 1.0f;
			}

			dest.m23 = qn;

			return dest;
		}

		public override void ApplyObliqueDepthProjection(ref Axiom.MathLib.Matrix4 projMatrix, Axiom.MathLib.Plane plane, bool forGpuProgram) {
			// Thanks to Eric Lenyel for posting this calculation at www.terathon.com

			// Calculate the clip-space corner point opposite the clipping plane
			// as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
			// transform it into camera space by multiplying it
			// by the inverse of the projection matrix

			/* generalised version
			Vector4 q = matrix.inverse() * 
				Vector4(Math::Sign(plane.normal.x), Math::Sign(plane.normal.y), 1.0f, 1.0f);
			*/
			Axiom.MathLib.Vector4 q = new Axiom.MathLib.Vector4();
			q.x = Math.Sign(plane.Normal.x) / projMatrix.m00;
			q.y = Math.Sign(plane.Normal.y) / projMatrix.m11;
			q.z = 1.0f;
 
			// flip the next bit from Lengyel since we're right-handed
			if (forGpuProgram) {
				q.w = (1.0f - projMatrix.m22) / projMatrix.m23;
			}
			else {
				q.w = (1.0f + projMatrix.m22) / projMatrix.m23;
			}

			// Calculate the scaled plane vector
			Axiom.MathLib.Vector4 clipPlane4d = 
				new Axiom.MathLib.Vector4(plane.Normal.x, plane.Normal.y, plane.Normal.z, plane.D);

			Axiom.MathLib.Vector4 c = clipPlane4d * (1.0f / (clipPlane4d.Dot(q)));

			// Replace the third row of the projection matrix
			projMatrix.m20 = c.x;
			projMatrix.m21 = c.y;

			// flip the next bit from Lengyel since we're right-handed
			if (forGpuProgram) {
				projMatrix.m22 = c.z; 
			}
			else {
				projMatrix.m22 = -c.z; 
			}

			projMatrix.m23 = c.w;   
		}

		/// <summary>
		/// 
		/// </summary>
		public override void BeginFrame() {
			Debug.Assert(activeViewport != null, "BeingFrame cannot run without an active viewport.");

			// clear the device if need be
			if(activeViewport.ClearEveryFrame) {
				ClearFrameBuffer(FrameBuffer.Color | FrameBuffer.Depth, activeViewport.BackgroundColor);
			}

			// begin the D3D scene for the current viewport
			device.BeginScene();

			// set initial render states if this is the first frame. we only want to do 
			//	this once since renderstate changes are expensive
			if (!basicStatesInitialized) {
				// enable alpha blending and specular materials
				// device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SpecularEnable = true;
				//device.RenderState.ZBufferEnable = true;
                basicStatesInitialized = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void EndFrame() {
			// end the D3D scene
			device.EndScene();
		}

        // TODO: Added this for debugging
        public void SurfaceDisposed(object sender, EventArgs e) {
            log.Info("Disposing surface");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="viewport"></param>
		public override void SetViewport(Axiom.Core.Viewport viewport) {
			if (activeViewport != viewport || viewport.IsUpdated) {

                // FIXME
                // If we had a render target of texture, and are switching, grab the surface
                //try {
                //    if (activeViewport != null && activeViewport.Target is D3DRenderTexture) {
                //        D3D.Surface back1 = (D3D.Surface)activeViewport.Target.GetCustomAttribute("DDBACKBUFFER");
                //        SurfaceLoader.Save("../RenderTarget_surface1.jpg", ImageFileFormat.Jpg, back1);
                //        D3D.Surface back2 = device.GetBackBuffer(0, 0, BackBufferType.Mono);
                //        SurfaceLoader.Save("../RenderTarget_surface2.jpg", ImageFileFormat.Jpg, back2);
                //        D3D.Surface back3 = device.GetRenderTarget(0);
                //        SurfaceLoader.Save("../RenderTarget_surface3.jpg", ImageFileFormat.Jpg, back3);
                //    }
                //} catch (Exception e) {
                //    Trace.TraceWarning("Couldn't save texture: RenderTarget_surface.jpg");
                //}


                
                // store this viewport and it's target
				activeViewport = viewport;
				activeRenderTarget = viewport.Target;

                RenderTarget target = viewport.Target;
                // FIXME: Looks like these methods should be able to return multiple buffers
				// get the back buffer surface for this viewport
                D3D.Surface back = (D3D.Surface)activeRenderTarget.GetCustomAttribute("DDBACKBUFFER");
                if (back == null)
                    return;

                // This is useful for debugging, but it breaks the exit (since we try to write to the trace)
                // back.Disposing += this.SurfaceDisposed;

                // we cannot dipose of the back buffer in fullscreen mode, since we have a direct reference to
                // the main back buffer.  all other surfaces are safe to dispose
                // FIXME: Do I need this?  Not in Ogre, but in Axiom
                // bool disposeBackBuffer = true;
                //if (activeRenderTarget is D3DRenderWindow) {
                //    D3DRenderWindow window = activeRenderTarget as D3DRenderWindow;
                //    if (window.IsFullScreen) {
                //        disposeBackBuffer = false;
                //    }
                //}
                // be sure to destroy the surface we had
                //if (disposeBackBuffer) {
                //    back.Dispose();
                //}

                D3D.Surface depth = (D3D.Surface)activeRenderTarget.GetCustomAttribute("D3DZBUFFER");
                if (depth == null) {
       				/// No depth buffer provided, use our own
    				/// Request a depth stencil that is compatible with the format, multisample type and
	    			/// dimensions of the render target.
                    SurfaceDescription srfDesc = back.Description;
                    depth = GetDepthStencilFor(srfDesc.Format, srfDesc.MultiSampleType, srfDesc.Width, srfDesc.Height);
                }

                // Bind render targets
                device.SetRenderTarget(0, back);
                // FIXME: Support multiple render targets
                //uint count = caps.NumMultiRenderTargets;
                //for (int i = 0; i < count; ++i) {
                //    device.SetRenderTarget(i, back[i]);
                //}

				// set the render target and depth stencil for the surfaces beloning to the viewport
				device.DepthStencilSurface = depth;

				// set the culling mode, to make adjustments required for viewports
				// that may need inverted vertex winding or texture flipping
				this.CullingMode = cullingMode;

				D3D.Viewport d3dvp = new D3D.Viewport();

				// set viewport dimensions
				d3dvp.X = viewport.ActualLeft;
				d3dvp.Y = viewport.ActualTop;
				d3dvp.Width = viewport.ActualWidth;
				d3dvp.Height = viewport.ActualHeight;

                if (target.RequiresTextureFlipping) {
                    // Convert "top-left" to "bottom-left"
                    d3dvp.Y = target.Height - d3dvp.Height - d3dvp.Y;
                }

				// Z-values from 0.0 to 1.0 (TODO: standardize with OpenGL)
				d3dvp.MinZ = 0.0f;
				d3dvp.MaxZ = 1.0f;

				// set the current D3D viewport
				device.Viewport = d3dvp;

				// clear the updated flag
				viewport.IsUpdated = false;
			}
		}

		/// <summary>
		///		Renders the current render operation in D3D's own special way.
		/// </summary>
		/// <param name="op"></param>
		public override void Render(RenderOperation op) {

            // Increment the static count of render calls
            totalRenderCalls++;

            // don't even bother if there are no vertices to render, causes problems on some cards (FireGL 8800)
			if(op.vertexData.vertexCount == 0) {
				return;
			}

			// Don't call the class base implementation first, since
			// we can compute the equivalent faster without calling it
			// base.Render(op);

			// set the vertex declaration and buffer binding
			SetVertexDeclaration(op.vertexData.vertexDeclaration);
			SetVertexBufferBinding(op.vertexData.vertexBufferBinding);

			PrimitiveType primType = 0;
            int vertexCount = op.vertexData.vertexCount;
            int cnt = op.useIndices ? op.indexData.indexCount : op.vertexData.vertexCount;

			switch(op.operationType) {
				case OperationType.TriangleList:
					primType = PrimitiveType.TriangleList;
					primCount = cnt / 3;
					numFaces += primCount;
                    break;
				case OperationType.TriangleStrip:
					primType = PrimitiveType.TriangleStrip;
					primCount = cnt - 2;
					numFaces += primCount;
					break;
				case OperationType.TriangleFan:
					primType = PrimitiveType.TriangleFan;
					primCount = cnt - 2;
					numFaces += primCount;
					break;
				case OperationType.PointList:
					primType = PrimitiveType.PointList;
					primCount = cnt;
					break;
				case OperationType.LineList:
					primType = PrimitiveType.LineList;
					primCount = cnt / 2;
					break;
				case OperationType.LineStrip:
					primType = PrimitiveType.LineStrip;
					primCount = cnt - 1;
					break;
			} // switch(primType)

			numVertices += vertexCount;
            
            if (MeterManager.Collecting)
                MeterManager.AddInfoEvent("D3D9 Render vertices {0}, primitives {1}, indices {2}", 
                    vertexCount, primCount, (op.useIndices ? op.indexData.indexCount : 0));
            
			// are we gonna use indices?
			if(op.useIndices) {
				D3DHardwareIndexBuffer idxBuffer = 
					(D3DHardwareIndexBuffer)op.indexData.indexBuffer;

				// set the index buffer on the device
				if (cache.indexBuffer != idxBuffer.D3DIndexBuffer) {
                    cache.indexBuffer = idxBuffer.D3DIndexBuffer;
                    device.Indices = idxBuffer.D3DIndexBuffer;
                }

				// draw the indexed primitives
				device.DrawIndexedPrimitives(
					primType, op.vertexData.vertexStart, 0, vertexCount, 
					op.indexData.indexStart, primCount);
			}
			else {
				// draw vertices without indices
				device.DrawPrimitives(primType, op.vertexData.vertexStart, primCount);
			}
		}

        public void RestoreLostDevice() {
            // Release all non-managed resources

            // Cleanup depth stencils
	    	CleanupDepthStencils();

            // Set all texture units to nothing
            this.DisableTextureUnitsFrom(0);

		    // Unbind any vertex streams
            for (int i = 0; i < lastVertexSourceCount; i++) {
                device.SetStreamSource(i, null, 0, 0);
//                 cache.streamNull[i] = true;
            }
            lastVertexSourceCount = 0;

            // Release all automatic temporary buffers and free unused
            // temporary buffers, so we doesn't need to recreate them,
            // and they will reallocate on demand. This save a lot of
            // release/recreate of non-managed vertex buffers which
            // wasn't need at all.
            hardwareBufferManager.ReleaseBufferCopies(true);

            // We have to deal with non-managed textures and vertex buffers
            // GPU programs don't have to be restored
            ((D3DTextureManager)textureManager).ReleaseDefaultPoolResources();
            ((D3DHardwareBufferManager)hardwareBufferManager).ReleaseDefaultPoolResources();

            // release additional swap chains (secondary windows)
            foreach (D3DRenderWindow sw in secondaryWindows) {
                sw.DestroyD3DResources();
            }

            // Reset the device, using the primary window presentation params
            try {
                log.Warn("Calling Device.Reset");
                device.Reset(primaryWindow.PresentationParameters);
                CreateAndApplyCache();
            }
            catch (InvalidCallException e)
            {
                Axiom.Core.LogManager.Instance.WriteException("InvalidCallException in device.Reset: {0}", e);
                // Don't continue
                return;
            } catch (D3D.DeviceLostException e) {
                Axiom.Core.LogManager.Instance.WriteException("DeviceLostException in device.Reset: {0}", e);
                // Don't continue
                return;
            }

		    // will have lost basic states
            basicStatesInitialized = false;
            vertexProgramBound = false;
            fragmentProgramBound = false;

    		// recreate additional swap chains
    		foreach (D3DRenderWindow sw in secondaryWindows) {
			    sw.CreateD3DResources();
            }

            // Recreate all non-managed resources
            ((D3DTextureManager)textureManager).RecreateDefaultPoolResources();
            ((D3DHardwareBufferManager)hardwareBufferManager).RecreateDefaultPoolResources();

            if (cursorProperties.texture != null)
		SetCursor(cursorProperties.texture, cursorProperties.section, cursorProperties.hotSpot);

            log.Info("!!! Direct3D Device successfully restored.");

		    deviceLost = false;

            // fireEvent("DeviceRestored");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="enabled"></param>
		/// <param name="textureName"></param>
		public override void SetTexture(int stage, bool enabled, string textureName) {
			D3DTexture texture = (D3DTexture)TextureManager.Instance.GetByName(textureName);

            // FIXME: Debugging
            //if (textureName.StartsWith("CompositorInstanceTexture")) {
            //    try {
            //        TextureLoader.Save("../" + textureName + "_dxtexture.jpg", ImageFileFormat.Jpg, texture.DXTexture);
            //        SurfaceLoader.Save("../" + textureName + "_surface.jpg", ImageFileFormat.Jpg, texture.surfaceList[0].Surface);
            //    } catch (Exception e) {
            //        Trace.TraceWarning("Couldn't save texture: " + textureName);
            //    }
            //}

            if(enabled && texture != null) {
                // note used
                texture.Touch();

                if (texStageDesc[stage].tex != texture.DXTexture)
                {
                    device.SetTexture(stage, texture.DXTexture);

                    // set stage description
                    texStageDesc[stage].tex = texture.DXTexture;
                    texStageDesc[stage].texType = D3DHelper.ConvertEnum(texture.TextureType);
                }
			}
			else {
                if (texStageDesc[stage].tex != null) {
					device.SetTexture(stage, null);
				}

                if (stage < caps.TextureUnitCount) {
                    if (cache.colorOperation[stage] != D3D.TextureOperation.Disable) {
                        cache.colorOperation[stage] = D3D.TextureOperation.Disable;
                        device.TextureState[stage].ColorOperation = D3D.TextureOperation.Disable;
                    }
                }
                
                // set stage description to defaults
				texStageDesc[stage].tex = null;
                if (texStageDesc[stage].autoTexCoordType != TexCoordCalcMethod.None) {
                    texStageDesc[stage].autoTexCoordType = TexCoordCalcMethod.None;
                    cache.usingTextureMatrix[stage] = false;
                }
				texStageDesc[stage].coordIndex = 0;
				texStageDesc[stage].texType = D3DTexType.Normal;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="maxAnisotropy"></param>
		public override void SetTextureLayerAnisotropy(int stage, int maxAnisotropy) {
			if (!cache.samplerStateInitialized[stage])
                cache.EnsureSamplerStateInitialized(stage, device, d3dCaps);
            if(maxAnisotropy > d3dCaps.MaxAnisotropy) {
				maxAnisotropy = d3dCaps.MaxAnisotropy;
			}

			if (cache.textureLayerAnisotropy[stage] != maxAnisotropy) {
                cache.textureLayerAnisotropy[stage] = maxAnisotropy;
				device.SamplerState[stage].MaxAnisotropy = maxAnisotropy;
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="method"></param>
		public override void SetTextureCoordCalculation(int stage, TexCoordCalcMethod method, Frustum frustum) {
			// save this for texture matrix calcs later
            if (texStageDesc[stage].autoTexCoordType != method || texStageDesc[stage].frustum != frustum) {
                // We will need ot recalculate the texture matrix
                cache.usingTextureMatrix[stage] = false;
                texStageDesc[stage].autoTexCoordType = method;
                texStageDesc[stage].frustum = frustum;
            }
            SetTextureCoordSetInternal(stage, texStageDesc[stage].coordIndex);
		}

		public override void BindGpuProgram(GpuProgram program) {
			switch(program.Type) {
				case GpuProgramType.Vertex:
                    VertexShader vertexShader = ((D3DVertexProgram)program).VertexShader;

                    if (cache.vertexShader != vertexShader) {
                        cache.vertexShader = vertexShader;
                        device.VertexShader = vertexShader;
                    }
					break;

				case GpuProgramType.Fragment:
                    PixelShader pixelShader = ((D3DFragmentProgram)program).PixelShader;

                    if (cache.pixelShader != pixelShader) {
                        cache.pixelShader = pixelShader;
                        device.PixelShader = pixelShader;
                    }
					break;
			}

            base.BindGpuProgram(program);
		}

        protected bool FloatConstantDifferentFromCache(GpuProgramParameters parms, float[] cachedConstants, int index) {
            if (index >= parms.float4VecConstantsCount || !parms.floatIsSet[index])
                return false;
            int i = index * 4;
            float [] s = parms.floatConstantsArray;
            return !(cachedConstants[i] == s[i] && cachedConstants[i+1] == s[i+1] && cachedConstants[i+2] == s[i+2] && cachedConstants[i+3] == s[i+3]);
        }

        protected bool SomeFloatConstantInSkipRangeNeedsUpdating(GpuProgramParameters parms, float[] cachedConstants, int startIndex) {
            for (int i=0; i<cache.skipRange; i++) {
                int index = startIndex + i;
                if (index >= parms.float4VecConstantsCount) {
                    return false;
                }
                if (index >= parms.float4VecConstantsCount || !parms.floatIsSet[index])
                    continue;
                float [] s = parms.floatConstantsArray;
                if (!(cachedConstants[i] == s[index] && cachedConstants[index+1] == s[index+1] && cachedConstants[index+2] == s[index+2] && cachedConstants[index+3] == s[index+3]))
                    return true;
            }
            return false;
        }

		public struct FloatArrayAndOffset {
            public float[] array;
            public int offset;
            
            public FloatArrayAndOffset(float[] array, int offset) {
                this.array = array;
                this.offset = offset;
            }
        }
        
        /// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public List<FloatArrayAndOffset> GetFloatConstantArrays(GpuProgramParameters parms, float[] cachedConstants) {
            int firstIndex = -1;
            int count = parms.maxSetCount;
            List<FloatArrayAndOffset> arrays = new List<FloatArrayAndOffset>();
            for (int i = 0; i < count; i++) {
				bool thisNeedsUpdating = FloatConstantDifferentFromCache(parms, cachedConstants, i);
                if (firstIndex == -1) {
                    if (thisNeedsUpdating)
                        firstIndex = i;
                }
                else if (!thisNeedsUpdating && !SomeFloatConstantInSkipRangeNeedsUpdating(parms, cachedConstants, i + 1)) {
                    arrays.Add(new FloatArrayAndOffset(GetFloatConstantArrayStartingWith(parms, cachedConstants, firstIndex, i - firstIndex), firstIndex));
                    firstIndex = -1;
                }
            }
            if (firstIndex != -1)
                arrays.Add(new FloatArrayAndOffset(GetFloatConstantArrayStartingWith(parms, cachedConstants, firstIndex, count - firstIndex), firstIndex));
            return arrays;
        }
        
        protected float[] GetFloatConstantArrayStartingWith(GpuProgramParameters parms, float[] cachedConstants, int firstIndex, int count) {
            float[] array = new float[count * 4];
            float[] source = parms.floatConstantsArray;
            for (int i = 0; i < count; i++) {
                int index = firstIndex + i;
                int b = index * 4;
                int ba = i * 4;
                if (!parms.floatIsSet[index]) {
                    for (int j=0; j<4; j++) {
                        int s = b + j;
                        array[ba + j] = 0.0f;
                        cachedConstants[s] = 0.0f;
                    }
                }
                else {
                    for (int j=0; j<4; j++) {
                        int s = b + j;
                        float v = source[s];
                        array[ba + j] = v;
                        cachedConstants[s] = v;
                    }
                }
            }
            return array;
        }

        protected bool IntConstantDifferentFromCache(GpuProgramParameters parms, Object[] intConstants, int index) {
            GpuProgramParameters.IntConstantEntry e = parms.GetIntConstant(index);
            if (!e.isSet)
                return false;
            int[] c = (int[])intConstants[index];
            return c == null || !(c[0] == e.val[0] && c[1] == e.val[1] && c[2] == e.val[2] && c[3] == e.val[3]);
        }

        protected bool SomeIntConstantInSkipRangeNeedsUpdating(GpuProgramParameters parms, Object[] intConstants, int index) {
            for (int i=0; i<cache.skipRange; i++) {
                if (index + i >= parms.IntConstantCount) {
                    return false;
                }
                if (IntConstantDifferentFromCache(parms, intConstants, index + i))
                    return true;
            }
            return false;
        }

		public struct IntArrayAndOffset {
            public int[] array;
            public int offset;
            
            public IntArrayAndOffset(int[] array, int offset) {
                this.array = array;
                this.offset = offset;
            }
        }
        
        /// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public List<IntArrayAndOffset> GetIntConstantArrays(GpuProgramParameters parms, Object[] intConstants) {
            int firstIndex = -1;
            int count = parms.IntConstantCount;
            List<IntArrayAndOffset> arrays = new List<IntArrayAndOffset>();
            for (int i = 0; i < count; i++) {
				bool thisNeedsUpdating = IntConstantDifferentFromCache(parms, intConstants, i);
                if (firstIndex == -1) {
                    if (thisNeedsUpdating)
                        firstIndex = i;
                }
                else if (!thisNeedsUpdating && !SomeIntConstantInSkipRangeNeedsUpdating(parms, intConstants, i + 1)) {
                    arrays.Add(new IntArrayAndOffset(GetIntConstantArrayStartingWith(parms, intConstants, firstIndex, i - firstIndex), firstIndex));
                    firstIndex = -1;
                }
            }
            if (firstIndex != -1)
                arrays.Add(new IntArrayAndOffset(GetIntConstantArrayStartingWith(parms, intConstants, firstIndex, count - firstIndex), firstIndex));
            return arrays;
        }
        
        protected int[] GetIntConstantArrayStartingWith(GpuProgramParameters parms, Object[] intConstants, int firstIndex, int count) {
            int[] array = new int[count * 4];
            for (int i = 0; i < count; i++) {
				GpuProgramParameters.IntConstantEntry e = parms.GetIntConstant(firstIndex + i);
                if (!e.isSet) {
                    for (int j=0; j<4; j++)
                        array[i * 4 + j] = 0;
                }
                else {
                    for (int j=0; j<4; j++)
                        array[i * 4 + j] = e.val[j];
                }
                int[] cacheConstant = (int[])intConstants[firstIndex + i];
                if (cacheConstant == null) {
                    cacheConstant = new int[4];
                    intConstants[firstIndex + i] = cacheConstant;
                }
                for (int j=0; j<4; j++)
                    cacheConstant[j] = array[i * 4 + j];
            }
            return array;
        }

		public override void BindGpuProgramParameters(GpuProgramType type, GpuProgramParameters parms) {
			switch(type) {
            case GpuProgramType.Vertex:
                if(parms.HasIntConstants) {
                    if (cache.useConstantCache) {
                        foreach (IntArrayAndOffset arrayAndOffset in GetIntConstantArrays(parms, cache.intShaderConstants))
                            device.SetVertexShaderConstant(arrayAndOffset.offset, arrayAndOffset.array);
                    }
                    else {
                        for(int index = 0; index < parms.IntConstantCount; index++) {
                            GpuProgramParameters.IntConstantEntry entry = parms.GetIntConstant(index);
                            if(entry.isSet)
                                device.SetVertexShaderConstant(index, entry.val);
                        }
                    }
                }

                if(parms.HasFloatConstants) {
                    if (cache.useConstantCache) {
                        foreach (FloatArrayAndOffset arrayAndOffset in GetFloatConstantArrays(parms, cache.floatShaderConstants))
                            device.SetVertexShaderConstant(arrayAndOffset.offset, arrayAndOffset.array);
                    }
                    else
                        device.SetVertexShaderConstant(0, parms.floatConstantsArray);
                }
                break;
                
            case GpuProgramType.Fragment:
                if(parms.HasIntConstants) {
                    if (cache.useConstantCache) {
                        foreach (IntArrayAndOffset arrayAndOffset in GetIntConstantArrays(parms, cache.intPixelConstants))
                            device.SetPixelShaderConstant(arrayAndOffset.offset, arrayAndOffset.array);
                    }
                    else {
                        for(int index = 0; index < parms.IntConstantCount; index++) {
							GpuProgramParameters.IntConstantEntry entry = parms.GetIntConstant(index);
							if(entry.isSet)
								device.SetPixelShaderConstant(index, entry.val);
                        }
                    }
                }
                if(parms.HasFloatConstants) {
                    if (cache.useConstantCache) {
                        foreach (FloatArrayAndOffset arrayAndOffset in GetFloatConstantArrays(parms, cache.floatPixelConstants))
                            device.SetPixelShaderConstant(arrayAndOffset.offset, arrayAndOffset.array);
                    }
                    else
                        device.SetPixelShaderConstant(0, parms.floatConstantsArray);
                }
                break;
            }
		}

		public override void UnbindGpuProgram(GpuProgramType type) {
			switch(type) {
				case GpuProgramType.Vertex:
                    if (cache.vertexShader != null) {
                        cache.vertexShader = null;
                        device.VertexShader = null;
                    }
					break;

				case GpuProgramType.Fragment:
                    if (cache.pixelShader != null) {
                        cache.pixelShader = null;
                        device.PixelShader = null;
                    }
					break;
			}

            base.UnbindGpuProgram(type);
		}

		#endregion

		public override Axiom.MathLib.Matrix4 WorldMatrix {
			get {
				throw new NotImplementedException();
			}
			set {
                if (cache.worldMatrix != value) {
                    cache.worldMatrix = value;
                    device.Transform.World = MakeD3DMatrix(value);
                }
			}
		}

		public override Axiom.MathLib.Matrix4 ViewMatrix {
			get {
				throw new NotImplementedException();
			}
			set {
                if (cache.viewMatrix != value) {
                    cache.viewMatrix = value;

                    SetViewMatrixInternal(value);
                }
            }
        }
            
        public void SetViewMatrixInternal(Axiom.MathLib.Matrix4 value) {
                
            // flip the transform portion of the matrix for DX and its left-handed coord system
            // save latest view matrix
            viewMatrix = value;
            viewMatrix.m20 = -viewMatrix.m20;
            viewMatrix.m21 = -viewMatrix.m21;
            viewMatrix.m22 = -viewMatrix.m22;
            viewMatrix.m23 = -viewMatrix.m23;

            DX.Matrix dxView = MakeD3DMatrix(viewMatrix);
            device.Transform.View = dxView;
        }

		public override Axiom.MathLib.Matrix4 ProjectionMatrix {
			get {
				throw new NotImplementedException();
			}
			set {
                if (cache.projectionMatrix != value) {
                    cache.projectionMatrix = value;

                    SetProjectionMatrixInternal(value, true);
                }
            }
        }
            
        public void SetProjectionMatrixInternal(Axiom.MathLib.Matrix4 value, bool haveActiveTarget) {
                
            Matrix mat = MakeD3DMatrix(value);

            if(haveActiveTarget && activeRenderTarget.RequiresTextureFlipping) {
                mat.M12 = -mat.M12;
                mat.M22 = -mat.M22;
                mat.M32 = -mat.M32;
                mat.M42 = -mat.M42;
            }

            device.Transform.Projection = mat;
        }
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lightList"></param>
		/// <param name="limit"></param>
		public override void UseLights(List<Axiom.Core.Light> lightList, int limit) {
			int i = 0;

			//log.DebugFormat("D3D9RenderSystem.UseLights: lightList.Count {0}, limit {1}", lightList.Count, limit);

            for( ; i < limit && i < lightList.Count; i++)
                SetD3DLight(i, lightList[i]);

			for( ; i < numCurrentLights; i++)
                SetD3DLight(i, null);

			numCurrentLights = (int)MathUtil.Min(limit, lightList.Count);
		}

		public override uint ConvertColor(ColorEx color) {
			return color.ToARGB();
		}

		/// <summary>
		///   Convert the RenderSystem's encoding of color to an explicit portable one.
		/// </summary>
		/// <param name="color">The color as an integer</param>
		/// <returns>ColorEx version of the RenderSystem specific int storage of color</returns>
		public override ColorEx ConvertColor(uint color) {
			ColorEx colorEx = new ColorEx();
			colorEx.a = (float)((color >> 24) % 256) / 255;
			colorEx.r = (float)((color >> 16) % 256) / 255;
			colorEx.g = (float)((color >> 8 ) % 256) / 255;
			colorEx.b = (float)((color      ) % 256) / 255;
			return colorEx;
		}


		public override void SetSceneBlending(SceneBlendFactor src, SceneBlendFactor dest) {
			// set the render states after converting the incoming values to D3D.Blend
            if (src == SceneBlendFactor.One && dest == SceneBlendFactor.Zero)
                SetRenderState(ref cache.alphaBlendEnable, RenderStates.AlphaBlendEnable, false);
            else {
                
                SetRenderState(ref cache.alphaBlendEnable, RenderStates.AlphaBlendEnable, true);

                if (cache.sourceBlend != src) {
                    cache.sourceBlend = src;
                    device.RenderState.SourceBlend = D3DHelper.ConvertEnum(src);
                }

                if (cache.destinationBlend != dest) {
                    cache.destinationBlend = dest;
                    device.RenderState.DestinationBlend = D3DHelper.ConvertEnum(dest);
                }
            }
		}

        public override void SetCursor(Axiom.Core.Texture texture, System.Drawing.Rectangle section, System.Drawing.Point hotSpot) {
            // Kludge to keep track of the cursor properties, so that when we
            // reset the device, we can restore the cursor.
            cursorProperties.texture = texture;
            cursorProperties.section = section;
            cursorProperties.hotSpot = hotSpot;
            ClearCursor();
            //Cursor.Hide();
            D3DTexture d3dTexture = texture as D3DTexture;
            int width = section.Width;
            int height = section.Height;
            int tmp = width - 1;
            if ((width & (width - 1)) != 0)
                width = (int)Math.Pow(2, Math.Round(Math.Log(width, 2)));
            if ((height & (height - 1)) != 0)
                height = (int)Math.Pow(2, Math.Round(Math.Log(height, 2)));
            if (width > 32)
                width = 32;
            if (height > 32)
                height = 32;
            if (width != section.Width || height != section.Height)
                log.InfoFormat("Adjusted cursor dimensions: ({0}, {1})", width, height);
            cursorSurface = 
                device.CreateOffscreenPlainSurface(width, height, D3D.Format.A8R8G8B8, Pool.Scratch);
            using (Surface surf = d3dTexture.NormalTexture.GetSurfaceLevel(0)) {
                SurfaceLoader.FromSurface(cursorSurface, surf, section, Filter.Box, 0);
            }
            cursorHotSpot = hotSpot;
            // device.SetCursorPosition(100, 100, true);
            device.SetCursorProperties(cursorHotSpot.X, cursorHotSpot.Y, cursorSurface);
            device.ShowCursor(true);
            //Cursor.Show();
        }

        public override void SetCursorPosition(int x, int y) {
            device.SetCursorPosition(x, y, true);
        }

        public override void RestoreCursor() {
            try {
                if (cursorSurface != null) {
                    //device.ShowCursor(false);
                    device.SetCursorProperties(cursorHotSpot.X, cursorHotSpot.Y, cursorSurface);
                    //device.ShowCursor(true);
                }
            }
            catch (Exception e) {
                log.WarnFormat("D3D9RenderSystem.RestoreCursor: Exception setting cursor: {0} Stack trace {1}",
                    e.Message, e.StackTrace);
            }
        }

        public override void ClearCursor() {
            if (cursorSurface != null) {
                device.ShowCursor(false);
                cursorSurface.Dispose();
                cursorSurface = null;
            }
        }



		/// <summary>
		/// 
		/// </summary>
		public override CullingMode CullingMode {
			get {
				return cullingMode;
			}
			set {
				cullingMode = value;

				bool flip = activeRenderTarget.RequiresTextureFlipping ^ invertVertexWinding;

				if (cache.cullingMode != cullingMode || cache.cullingFlip != flip) {
                    cache.cullingMode = cullingMode;
                    cache.cullingFlip = flip;
                    device.RenderState.CullMode = D3DHelper.ConvertEnum(value, flip);
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
				if (cache.depthCheck != value) {
                    cache.depthCheck = value;
                    if(value) {
                        // use w-buffer if available
                        if(useWBuffer && d3dCaps.RasterCaps.SupportsWBuffer) {
                            device.RenderState.UseWBuffer = true;
                        }
                        else {
                            device.RenderState.ZBufferEnable = true;
                        }
                    }
                    else {
                        device.RenderState.ZBufferEnable = false;
                    }
                }
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
				if (cache.zBufferFunction != value) {
                    cache.zBufferFunction = value;
                    device.RenderState.ZBufferFunction = D3DHelper.ConvertEnum(value);
                }
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool DepthWrite {
			get {
				return cache.zBufferWriteEnable;
			}
			set {
				if (cache.zBufferWriteEnable != value) {
                    cache.zBufferWriteEnable = value;
                    device.RenderState.ZBufferWriteEnable = value;
                }
			}
		}

		/// <summary>
		///		
		/// </summary>
		public override float HorizontalTexelOffset {
			get {
				// D3D considers the origin to be in the center of a pixel
				return -0.5f;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override float VerticalTexelOffset {
			get {
				// D3D considers the origin to be in the center of a pixel
				return -0.5f;
			}
		}

		/// <summary>
		/// 
		/// </summary>
        public override bool PointSpritesEnabled {
            set {
                SetRenderState(ref cache.pointSpriteEnable, RenderStates.PointSpriteEnable, value);
            }
        }
        
		#region Private methods

		/// <summary>
		///		Sets up a light in D3D.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="light"></param>
		private void SetD3DLight(int index, Axiom.Core.Light light) {
			CachedLight cachedLight = cache.cachedLights[index];
            float v;
            if(light == null) {
				if (cachedLight.enabled != false) {
                    cachedLight.enabled = false;
                    device.Lights[index].Enabled = false;
                }
			}
			else {
				if (cachedLight.type != light.Type) {
                    cachedLight.type = light.Type;
                    switch(light.Type) {
					case LightType.Point:
						device.Lights[index].Type = D3D.LightType.Point;
						break;
					case LightType.Directional:
						device.Lights[index].Type = D3D.LightType.Directional;
						break;
					case LightType.Spotlight:
						device.Lights[index].Type = D3D.LightType.Spot;
                        break;
                    }
                }
                if (light.Type == LightType.Spotlight) {
                    v = light.SpotlightFalloff;
                    if (cachedLight.spotFalloff != v) {
                        cachedLight.spotFalloff = v;
                        device.Lights[index].Falloff = v;
                    }
                    v = light.SpotlightInnerAngle;
                    if (cachedLight.spotInner != v) {
                        cachedLight.spotInner = v;
                        device.Lights[index].InnerConeAngle = MathUtil.DegreesToRadians(v);
                    }
                    v = light.SpotlightOuterAngle;
                    if (cachedLight.spotOuter != v) {
                        cachedLight.spotOuter = v;
                        device.Lights[index].OuterConeAngle = MathUtil.DegreesToRadians(v);
                    }
				} // switch

				// light colors
				if (cachedLight.diffuse.CompareTo(light.Diffuse) != 0) {
                    cachedLight.diffuse = light.Diffuse;
                    device.Lights[index].Diffuse = light.Diffuse.ToColor();
                }
				if (cachedLight.specular.CompareTo(light.Specular) != 0) {
                    cachedLight.specular = light.Specular;
                    device.Lights[index].Specular = light.Specular.ToColor();
                }

                Axiom.MathLib.Vector3 vec;
				if(light.Type != LightType.Directional) {
                    vec = light.DerivedPosition;
                    if (cachedLight.position != vec) {
                        cachedLight.position = vec;
                        device.Lights[index].Position = new DX.Vector3(vec.x, vec.y, vec.z);
                    }
				}

				if(light.Type != LightType.Point) {
                    vec = light.DerivedDirection;
					if (cachedLight.direction != vec) {
                        cachedLight.direction = vec;
                        device.Lights[index].Direction = new DX.Vector3(vec.x, vec.y, vec.z);
                    }
				}

				v = light.AttenuationRange;
                if (cachedLight.range != v) {
                    cachedLight.range = v;
                    device.Lights[index].Range = v;
                }
				v = light.AttenuationConstant;
                if (cachedLight.attenuationConst != v) {
                    cachedLight.attenuationConst = v;
                    device.Lights[index].Attenuation0 = v;
                }
				v = light.AttenuationLinear;
                if (cachedLight.attenuationLinear != v) {
                    cachedLight.attenuationLinear = v;
                    device.Lights[index].Attenuation1 = v;
                }
				v = light.AttenuationQuadratic;
                if (cachedLight.attenuationQuad != v) {
                    cachedLight.attenuationQuad = v;
                    device.Lights[index].Attenuation2 = v;
                }

				device.Lights[index].Update();
				if (!cachedLight.enabled) {
                    cachedLight.enabled = true;
                    device.Lights[index].Enabled = true;
                }
			} // if
		}

		/// <summary>
		///		Called in constructor to init configuration.
		/// </summary>
		private void InitConfigOptions() {
			Driver driver = D3DHelper.GetDriverInfo();

            foreach (VideoMode mode in driver.VideoModes)
            {
                // add a new row to the display settings table
                displayConfig.FullscreenModes.Add(new Axiom.Configuration.DisplayMode(mode.Width, mode.Height, mode.ColorDepth, true));
            }

            ConfigOption optDevice = new ConfigOption();
            ConfigOption optVideoMode = new ConfigOption();
            ConfigOption optFullScreen = new ConfigOption();
            ConfigOption optVSync = new ConfigOption();
            ConfigOption optAA = new ConfigOption();
            ConfigOption optNVPerfHUD = new ConfigOption();

            optDevice.name = "Rendering Device";

            optVideoMode.name = "Video Mode";
            optVideoMode.currentValue = "800 x 600 @ 32-bit color";
            foreach (VideoMode mode in driver.VideoModes)
                optVideoMode.possibleValues.Add(mode.ToString());

            optFullScreen.name = "Full Screen";
            optFullScreen.possibleValues.Add("Yes");
            optFullScreen.possibleValues.Add("No");
            optFullScreen.currentValue = "Yes";

            optVSync.name = "VSync";
            optVSync.possibleValues.Add("Yes");
            optVSync.possibleValues.Add("No");
            optVSync.currentValue = "Yes";

            optAA.name = "Anti Aliasing";
            optAA.possibleValues.Add("None");
            optAA.currentValue = "None";
            //
            // Query DirectX to get the list of valid multisample quality levels.
            // I am currently limiting us to the Non-maskable types, since we don't
            // have any support at the moment for an app to enable multisample masking,
            // and so providing both masking and non-masking types would just provide
            // redundant, confusing, and possibly slower options to the user.
            //
            // Really, the world creator should decide whether they need maskable
            // multisampling for their visual effects, so the choice of maskable
            // or non-maskable should eventually be available through scripting
            // rather than at the request of the user.
            //
            AdapterDetails details = D3D.Manager.Adapters[driver.AdapterNumber].Information;

            int result;
            int maxAAQuality;
            bool ret;
            // Configuring anti-aliasing before the render target has been created is problematic, since we don't know
            // for sure the surface format or whether we are windowed or not.  We will make guesses and hope for the best...
            ret = D3D.Manager.CheckDeviceMultiSampleType(driver.AdapterNumber, DeviceType.Hardware, Format.X8R8G8B8, false, MultiSampleType.NonMaskable, out result, out maxAAQuality);
            if (ret && (result == (int)ResultCode.Success))
            {
                for (int i = 0; i < maxAAQuality; i++)
                {
                    optAA.possibleValues.Add("Quality " + (i + 1).ToString());
                }
            }

            optNVPerfHUD.name = "Allow NVPerfHUD";
            optNVPerfHUD.possibleValues.Add("Yes");
            optNVPerfHUD.possibleValues.Add("No");
            optNVPerfHUD.currentValue = "No";

            // options[optDevice.name] = optDevice.currentValue;
            options[optVideoMode.name] = optVideoMode;
            options[optFullScreen.name] = optFullScreen;
            options[optVSync.name] = optVSync;
            options[optAA.name] = optAA;
            options[optNVPerfHUD.name] = optNVPerfHUD;
		}

		private DX.Matrix MakeD3DMatrix(Axiom.MathLib.Matrix4 matrix) {
			DX.Matrix dxMat = new DX.Matrix();

			// set it to a transposed matrix since DX uses row vectors
			dxMat.M11 = matrix.m00;
			dxMat.M12 = matrix.m10;
			dxMat.M13 = matrix.m20;
			dxMat.M14 = matrix.m30;
			dxMat.M21 = matrix.m01;
			dxMat.M22 = matrix.m11;
			dxMat.M23 = matrix.m21;
			dxMat.M24 = matrix.m31;
			dxMat.M31 = matrix.m02;
			dxMat.M32 = matrix.m12;
			dxMat.M33 = matrix.m22;
			dxMat.M34 = matrix.m32;
			dxMat.M41 = matrix.m03;
			dxMat.M42 = matrix.m13;
			dxMat.M43 = matrix.m23;
			dxMat.M44 = matrix.m33;

			return dxMat;
		}

		/// <summary>
		///		Helper method to compare 2 vertex element arrays for equality.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool CompareVertexDecls(D3D.VertexElement[] a, D3D.VertexElement[] b) {
			// if b is null, return false
			if(b == null)
				return false;

			// compare lengths of the arrays
			if(a.Length != b.Length)
				return false;

			// continuing on, compare each property of each element.  if any differ, return false
			for(int i = 0; i < a.Length; i++) {
				if( a[i].DeclarationMethod != b[i].DeclarationMethod ||
					a[i].Offset != b[i].Offset ||
					a[i].Stream != b[i].Stream ||
					a[i].DeclarationType != b[i].DeclarationType ||
					a[i].DeclarationUsage != b[i].DeclarationUsage ||
					a[i].UsageIndex != b[i].UsageIndex
					)
					return false;
			}

			// if we made it this far, they matched up
			return true;
		}

		#endregion

		public override void SetDepthBufferParams(bool depthTest, bool depthWrite, CompareFunction depthFunction) {
			this.DepthCheck = depthTest;
			this.DepthWrite = depthWrite;
			this.DepthFunction = depthFunction;
		}

		public override void SetStencilBufferParams(CompareFunction function, int refValue, int mask, StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp, bool twoSidedOperation) {
			// 2 sided operation?
			if(twoSidedOperation) {
				if(!caps.CheckCap(Capabilities.TwoSidedStencil)) {
					throw new AxiomException("2-sided stencils are not supported on this hardware!");
				}

				device.RenderState.TwoSidedStencilMode = true;

				// use CCW version of the operations
				device.RenderState.CounterClockwiseStencilFail = D3DHelper.ConvertEnum(stencilFailOp, true);
				device.RenderState.CounterClockwiseStencilZBufferFail = D3DHelper.ConvertEnum(depthFailOp, true);
				device.RenderState.CounterClockwiseStencilPass = D3DHelper.ConvertEnum(passOp, true);
			}
			else {
				device.RenderState.TwoSidedStencilMode = false;
			}

			// configure standard version of the stencil operations
			device.RenderState.StencilFunction = D3DHelper.ConvertEnum(function);
			device.RenderState.ReferenceStencil = refValue;
			device.RenderState.StencilMask = mask;
			device.RenderState.StencilFail = D3DHelper.ConvertEnum(stencilFailOp);
			device.RenderState.StencilZBufferFail = D3DHelper.ConvertEnum(depthFailOp);
			device.RenderState.StencilPass = D3DHelper.ConvertEnum(passOp);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ambient"></param>
		/// <param name="diffuse"></param>
		/// <param name="specular"></param>
		/// <param name="emissive"></param>
		/// <param name="shininess"></param>
		public override void SetSurfaceParams(ColorEx ambient, ColorEx diffuse, ColorEx specular, ColorEx emissive, float shininess, TrackVertexColor tracking) {
			// TODO: Cache color values to prune unneccessary setting

            if (cache.materialAmbient.CompareTo(ambient) == 0 &&
                cache.materialDiffuse.CompareTo(diffuse) == 0 &&
                cache.materialSpecular.CompareTo(specular) == 0 &&
                cache.materialEmissive.CompareTo(emissive) == 0 &&
                cache.materialShininess == shininess &&
                cache.materialTracking == tracking)
                return;
            
            cache.materialAmbient = ambient.Clone();
            cache.materialDiffuse = diffuse.Clone();
            cache.materialSpecular = specular.Clone();
            cache.materialEmissive = emissive.Clone();
            cache.materialShininess = shininess;
            cache.materialTracking = tracking;
            
            // create a new material based on the supplied params
			D3D.Material mat = new D3D.Material();
			mat.Diffuse = diffuse.ToColor();
            mat.Ambient = ambient.ToColor();
            mat.Specular = specular.ToColor();
            mat.Emissive = emissive.ToColor();
			mat.SpecularSharpness = shininess;

			// set the current material
			device.Material = mat;

            if (tracking != TrackVertexColor.None) {
                SetRenderState(ref cache.colorVertex, RenderStates.ColorVertex, true);
                device.SetRenderState(RenderStates.AmbientMaterialSource, (int)(((tracking & TrackVertexColor.Ambient) != 0) ? D3D.ColorSource.Color1 : D3D.ColorSource.Material));
                device.SetRenderState(RenderStates.DiffuseMaterialSource, (int)(((tracking & TrackVertexColor.Diffuse) != 0) ? D3D.ColorSource.Color1 : D3D.ColorSource.Material));
                device.SetRenderState(RenderStates.SpecularMaterialSource, (int)(((tracking & TrackVertexColor.Specular) != 0) ? D3D.ColorSource.Color1 : D3D.ColorSource.Material));
                device.SetRenderState(RenderStates.EmissiveMaterialSource, (int)(((tracking & TrackVertexColor.Emissive) != 0) ? D3D.ColorSource.Color1 : D3D.ColorSource.Material));
            } else
                SetRenderState(ref cache.colorVertex, RenderStates.ColorVertex, false);
		}

        public void SetRenderState(RenderStates state, bool val) {
            device.SetRenderState(state, val);
        }

        public void SetRenderState(RenderStates state, float val) {
            device.SetRenderState(state, val);
        }

        public void SetRenderState(RenderStates state, int val) {
            device.SetRenderState(state, val);
        }

        public void SetRenderState(ref bool cachedValue, RenderStates state, bool val) {
            if (cachedValue != val) {
                cachedValue = val;
                device.SetRenderState(state, val);
            }
        }

        public void SetRenderState(ref float cachedValue, RenderStates state, float val) {
            if (cachedValue != val) {
                cachedValue = val;
                device.SetRenderState(state, val);
            }
        }

        public void SetRenderState(ref int cachedValue, RenderStates state, int val) {
            if (cachedValue != val) {
                cachedValue = val;
                device.SetRenderState(state, val);
            }
        }

	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="texAddressingMode"></param>
		public override void SetTextureAddressingMode(int stage, TextureAddressing texAddressingMode) {
			if (!cache.samplerStateInitialized[stage])
                cache.EnsureSamplerStateInitialized(stage, device, d3dCaps);
            D3D.TextureAddress d3dMode = D3DHelper.ConvertEnum(texAddressingMode);

			// set the device sampler states accordingly
			if (cache.textureAddressingModeU[stage] != texAddressingMode) {
                cache.textureAddressingModeU[stage] = texAddressingMode;
                device.SamplerState[stage].AddressU = d3dMode;
            }
            if (cache.textureAddressingModeV[stage] != texAddressingMode) {
                cache.textureAddressingModeV[stage] = texAddressingMode;
                device.SamplerState[stage].AddressV = d3dMode;
            }
            if (cache.textureAddressingModeW[stage] != texAddressingMode) {
                cache.textureAddressingModeW[stage] = texAddressingMode;
                device.SamplerState[stage].AddressW = d3dMode;
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="texAddressingMode"></param>
        public override void SetTextureAddressingMode(int stage, UVWAddressingMode uvwAddressingMode) {
			if (!cache.samplerStateInitialized[stage])
                cache.EnsureSamplerStateInitialized(stage, device, d3dCaps);
            // set the device sampler states accordingly
            TextureAddressing mode = uvwAddressingMode.u;
			if (cache.textureAddressingModeU[stage] != mode) {
                cache.textureAddressingModeU[stage] = mode;
                device.SamplerState[stage].AddressU = D3DHelper.ConvertEnum(mode);
            }
            mode = uvwAddressingMode.v;
            if (cache.textureAddressingModeV[stage] != mode) {
                cache.textureAddressingModeV[stage] = mode;
                device.SamplerState[stage].AddressV = D3DHelper.ConvertEnum(mode);
            }
            mode = uvwAddressingMode.w;
            if (cache.textureAddressingModeW[stage] != mode) {
                cache.textureAddressingModeW[stage] = mode;
                device.SamplerState[stage].AddressW = D3DHelper.ConvertEnum(mode);
            }
        }

        public override void SetTextureBorderColor(int stage, ColorEx borderColor)
        {
            if (cache.textureBorderColor[stage].CompareTo(borderColor) != 0) {
                cache.textureBorderColor[stage] = borderColor;
                device.SamplerState[stage].BorderColor = borderColor.ToColor();
            }
        }
	
		public override void SetTextureBlendMode(int stage, LayerBlendModeEx blendMode) {
			D3D.TextureOperation d3dTexOp = D3DHelper.ConvertEnum(blendMode.operation, d3dCaps);

			// TODO: Verify byte ordering
			if(blendMode.operation == LayerBlendOperationEx.BlendManual)
				SetTextureFactor((int)(new ColorEx(blendMode.blendFactor, 0, 0, 0)).ToARGB());

			if( blendMode.blendType == LayerBlendType.Color ) {

                // Make call to set operation
				if (cache.colorOperation[stage] != d3dTexOp) {
                    cache.colorOperation[stage] = d3dTexOp;
                    device.TextureState[stage].ColorOperation = d3dTexOp;
                }
			}
			else if( blendMode.blendType == LayerBlendType.Alpha ) {
				// Make call to set operation
				if (cache.alphaOperation[stage] != d3dTexOp) {
                    cache.alphaOperation[stage] = d3dTexOp;
                    device.TextureState[stage].AlphaOperation = d3dTexOp;
                }
			}

			// Now set up sources
			System.Drawing.Color factor = System.Drawing.Color.FromArgb(cache.textureFactor);

            // First do source1

			ColorEx manualD3D = ColorEx.FromColor(factor);
			if (blendMode.blendType == LayerBlendType.Color)
				manualD3D = new ColorEx(manualD3D.a, blendMode.colorArg1.r, blendMode.colorArg1.g, blendMode.colorArg1.b);
			else if (blendMode.blendType == LayerBlendType.Alpha)
				manualD3D = new ColorEx(blendMode.alphaArg1, manualD3D.r, manualD3D.g, manualD3D.b);

            LayerBlendSource blendSource = blendMode.source1;
            D3D.TextureArgument d3dTexArg = D3DHelper.ConvertEnum(blendSource);

            // set the texture blend factor if this is manual blending
            if(blendSource == LayerBlendSource.Manual)
                SetTextureFactor((int)manualD3D.ToARGB());

            if( blendMode.blendType == LayerBlendType.Color) {
                if (cache.colorArgument1[stage] != d3dTexArg) {
                    cache.colorArgument1[stage] = d3dTexArg;
                    device.TextureState[stage].ColorArgument1 = d3dTexArg;
                }
            }
            else if (blendMode.blendType == LayerBlendType.Alpha) {
                if (cache.alphaArgument1[stage] != d3dTexArg) {
                    cache.alphaArgument1[stage] = d3dTexArg;
                    device.TextureState[stage].AlphaArgument1 = d3dTexArg;
                }
            }

            // Now do source2

            if( blendMode.blendType == LayerBlendType.Color)
                manualD3D = new ColorEx(manualD3D.a, blendMode.colorArg2.r, blendMode.colorArg2.g, blendMode.colorArg2.b);
            else if( blendMode.blendType == LayerBlendType.Alpha)
                manualD3D = new ColorEx(blendMode.alphaArg2, manualD3D.r, manualD3D.g, manualD3D.b);

            blendSource = blendMode.source2;
            d3dTexArg = D3DHelper.ConvertEnum(blendSource);

            if(blendSource == LayerBlendSource.Manual)
                SetTextureFactor((int)manualD3D.ToARGB());

            if( blendMode.blendType == LayerBlendType.Color) {
                if (cache.colorArgument2[stage] != d3dTexArg) {
                    cache.colorArgument2[stage] = d3dTexArg;
                    device.TextureState[stage].ColorArgument2 = d3dTexArg;
                }
            }
            else if (blendMode.blendType == LayerBlendType.Alpha) {
                if (cache.alphaArgument2[stage] != d3dTexArg) {
                    cache.alphaArgument2[stage] = d3dTexArg;
                    device.TextureState[stage].AlphaArgument2 = d3dTexArg;
                }
            }
		}
	
        protected void SetTextureFactor(int factor) { 
            if (cache.textureFactor != factor) {
                cache.textureFactor = factor;
                device.RenderState.TextureFactor = factor;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="index"></param>
		public override void SetTextureCoordSet(int stage, int index) {
			// store
			texStageDesc[stage].coordIndex = index;
            if (stage < 8)
                SetTextureCoordSetInternal(stage, index);
		}

		protected void SetTextureCoordSetInternal(int stage, int index) {
            if (cache.textureCoordinateIndex[stage] != index) {
                cache.textureCoordinateIndex[stage] = index;
                device.TextureState[stage].TextureCoordinateIndex = D3DHelper.ConvertEnum(texStageDesc[stage].autoTexCoordType, d3dCaps) | index;
            }
        }
        
        /// <summary>
		/// 
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="type"></param>
		/// <param name="filter"></param>
		public override void SetTextureUnitFiltering(int stage, FilterType type, FilterOptions filter) {
            D3DTexType texType = texStageDesc[stage].texType;
            D3D.TextureFilter texFilter = D3DHelper.ConvertEnum(type, filter, d3dCaps, texType);
			if (!cache.samplerStateInitialized[stage])
                cache.EnsureSamplerStateInitialized(stage, device, d3dCaps);
            if (cache.textureUnitFilteringFilter[stage, (int)type] != texFilter) {
                cache.textureUnitFilteringFilter[stage, (int)type] = texFilter;
                SetTextureUnitFilteringInternal(stage, type, texFilter);
            }
        }
            
        public void SetTextureUnitFilteringInternal(int stage, FilterType type, D3D.TextureFilter texFilter) {
            switch(type) {
            case FilterType.Min:
                device.SamplerState[stage].MinFilter = texFilter;
                break;

            case FilterType.Mag:
                device.SamplerState[stage].MagFilter = texFilter;
                break;

            case FilterType.Mip:
                device.SamplerState[stage].MipFilter = texFilter;
                break;
            }
        }

        public override void SetTextureMipmapBias(int stage, float bias) {
			if (!cache.samplerStateInitialized[stage])
                cache.EnsureSamplerStateInitialized(stage, device, d3dCaps);
            if (cache.mipMapLODBias[stage] != bias) {
                cache.mipMapLODBias[stage] = bias;
                device.SamplerState[stage].MipMapLevelOfDetailBias = bias;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="xform"></param>
		public override void SetTextureMatrix(int stage, Matrix4 xform) {
			if (cache.usingTextureMatrix[stage] && cache.textureMatrix[stage] == xform)
                return;
            cache.textureMatrix[stage] = xform;
            cache.usingTextureMatrix[stage] = true;
            SetTextureMatrixInternal(stage, xform);
        }
            
        public void SetTextureMatrixInternal(int stage, Matrix4 xform) {
            DX.Matrix d3dMat = DX.Matrix.Identity;
			Matrix4 newMat = xform;

			/* If envmap is applied, but device doesn't support spheremap,
			then we have to use texture transform to make the camera space normal
			reference the envmap properly. This isn't exactly the same as spheremap
			(it looks nasty on flat areas because the camera space normals are the same)
			but it's the best approximation we have in the absence of a proper spheremap */
			if(texStageDesc[stage].autoTexCoordType == TexCoordCalcMethod.EnvironmentMap) {
				if(d3dCaps.VertexProcessingCaps.SupportsTextureGenerationSphereMap) {
					// inverts the texture for a spheremap
					Matrix4 matEnvMap = Matrix4.Identity;
					matEnvMap.m11 = -1.0f;

					// concatenate 
					newMat = newMat * matEnvMap;
				}
				else {
					/* If envmap is applied, but device doesn't support spheremap,
					then we have to use texture transform to make the camera space normal
					reference the envmap properly. This isn't exactly the same as spheremap
					(it looks nasty on flat areas because the camera space normals are the same)
					but it's the best approximation we have in the absence of a proper spheremap */

					// concatenate with the xForm
					newMat = newMat * Matrix4.ClipSpace2DToImageSpace;
				}
			}

			// If this is a cubic reflection, we need to modify using the view matrix
			if(texStageDesc[stage].autoTexCoordType == TexCoordCalcMethod.EnvironmentMapReflection) {
				// get the current view matrix
				DX.Matrix viewMatrix = device.Transform.View;

				// Get transposed 3x3, ie since D3D is transposed just copy
				// We want to transpose since that will invert an orthonormal matrix ie rotation
				Matrix4 viewTransposed = Matrix4.Identity;
				viewTransposed.m00 = viewMatrix.M11;
				viewTransposed.m01 = viewMatrix.M12;
				viewTransposed.m02 = viewMatrix.M13;
				viewTransposed.m03 = 0.0f;

				viewTransposed.m10 = viewMatrix.M21;
				viewTransposed.m11 = viewMatrix.M22;
				viewTransposed.m12 = viewMatrix.M23;
				viewTransposed.m13 = 0.0f;

				viewTransposed.m20 = viewMatrix.M31;
				viewTransposed.m21 = viewMatrix.M32;
				viewTransposed.m22 = viewMatrix.M33;
				viewTransposed.m23 = 0.0f;

				viewTransposed.m30 = viewMatrix.M41;
				viewTransposed.m31 = viewMatrix.M42;
				viewTransposed.m32 = viewMatrix.M43;
				viewTransposed.m33 = 1.0f;

				// concatenate
				newMat = newMat * viewTransposed;
			}

			if (texStageDesc[stage].autoTexCoordType == TexCoordCalcMethod.ProjectiveTexture) {
				// Derive camera space to projector space transform
				// To do this, we need to undo the camera view matrix, then 
				// apply the projector view & projection matrices
				newMat = viewMatrix.Inverse() * newMat;
				newMat = texStageDesc[stage].frustum.ViewMatrix * newMat;
				newMat = texStageDesc[stage].frustum.ProjectionMatrix * newMat;

				if (texStageDesc[stage].frustum.ProjectionType == Projection.Perspective) {
					newMat = ProjectionClipSpace2DToImageSpacePerspective * newMat;

                    //LogManager.Instance.Write("Texture Matrix: {0}", newMat.ToString());
				}
				else {
					newMat = ProjectionClipSpace2DToImageSpaceOrtho * newMat;

                    //LogManager.Instance.Write("Texture Matrix: {0}", newMat.ToString());
				}

			}

			// convert to D3D format
			d3dMat = MakeD3DMatrix(newMat);

			// need this if texture is a cube map, to invert D3D's z coord
			if(texStageDesc[stage].autoTexCoordType != TexCoordCalcMethod.None) {
				d3dMat.M13 = -d3dMat.M13;
				d3dMat.M23 = -d3dMat.M23;
				d3dMat.M33 = -d3dMat.M33;
				d3dMat.M43 = -d3dMat.M43;
			}

			D3D.TransformType d3dTransType = (D3D.TransformType)((int)(D3D.TransformType.Texture0) + stage);

			// set the matrix if it is not the identity
			if(!D3DHelper.IsIdentity(ref d3dMat)) { 
				// tell D3D the dimension of tex. coord
				int texCoordDim = 0;
				if (texStageDesc[stage].autoTexCoordType == TexCoordCalcMethod.ProjectiveTexture) {
					texCoordDim = (int)D3D.TextureTransform.Projected | (int)D3D.TextureTransform.Count3;
				}
				else {
					switch(texStageDesc[stage].texType) {
						case D3DTexType.Normal:
							texCoordDim = (int)D3D.TextureTransform.Count2;
							break;
						case D3DTexType.Cube:
						case D3DTexType.Volume:
							texCoordDim = (int)D3D.TextureTransform.Count3;
							break;
					}
				}

				// note: int values of D3D.TextureTransform correspond directly with tex dimension, so direct conversion is possible
				// i.e. Count1 = 1, Count2 = 2, etc
                SetTextureTransformOp(stage, (D3D.TextureTransform)texCoordDim);

				// set the manually calculated texture matrix
				device.SetTransform(d3dTransType, d3dMat);
			}
			else {
				// disable texture transformation
                SetTextureTransformOp(stage, D3D.TextureTransform.Disable);

				// set as the identity matrix
                device.SetTransform(d3dTransType, DX.Matrix.Identity);
			}
		}

		protected void SetTextureTransformOp(int stage, D3D.TextureTransform t) {
            if (cache.textureTransformOp[stage] != t) {
                cache.textureTransformOp[stage] = t;
                device.TextureState[stage].TextureTransform = t;
            }
        }
        
        public override void SetScissorTest(bool enable, int left, int top, int right, int bottom) {
			if(enable) {
				device.ScissorRectangle = new System.Drawing.Rectangle(left, top, right - left, bottom - top);
				device.RenderState.ScissorTestEnable = true;
			}
			else {
				device.RenderState.ScissorTestEnable = false;
			}
		}
	
        public override void SetPointParameters(float size, bool attenuationEnabled, float constant,
                                                float linear, float quadratic, float minSize, float maxSize) {
            if (attenuationEnabled) {
                // scaling required
                SetRenderState(ref cache.pointScaleEnable, RenderStates.PointScaleEnable, true);
                SetRenderState(RenderStates.PointScaleA, constant);
                SetRenderState(RenderStates.PointScaleB, linear);
                SetRenderState(RenderStates.PointScaleC, quadratic);
            }
            else
                // no scaling required
                SetRenderState(ref cache.pointScaleEnable, RenderStates.PointScaleEnable, false);
            
            SetRenderState(ref cache.pointSize, RenderStates.PointSize, size);

            SetRenderState(ref cache.pointSizeMin, RenderStates.PointSizeMin, minSize);

            if (maxSize == 0.0f)
                maxSize = caps.MaxPointSize;
            SetRenderState(ref cache.pointSizeMax, RenderStates.PointSizeMax, maxSize);
        }

		/// <summary>
		///		Helper method to go through and interrogate hardware capabilities.
		/// </summary>
		private void InitCapabilities() {
            // get caps
            d3dCaps = device.DeviceCaps;
			
			// max active lights
			caps.MaxLights = d3dCaps.MaxActiveLights;

			D3D.Surface surface = device.DepthStencilSurface;
			D3D.SurfaceDescription surfaceDesc = surface.Description;
			surface.Dispose();
     
			if (surfaceDesc.Format == D3D.Format.D24S8 || surfaceDesc.Format == D3D.Format.D24X8) {
				caps.SetCap(Capabilities.StencilBuffer);
                // Actually, it's always 8-bit
				caps.StencilBufferBits = 8;
			}

            // Set number of texture units
            caps.TextureUnitCount = d3dCaps.MaxSimultaneousTextures;

			// some cards, oddly enough, do not support this
			if(d3dCaps.DeclTypes.SupportsUByte4) {
				caps.SetCap(Capabilities.VertexFormatUByte4);
			}

			// Anisotropy?
			if(d3dCaps.MaxAnisotropy > 1) {
				caps.SetCap(Capabilities.AnisotropicFiltering);
			}

			// Hardware mipmapping?
			if(d3dCaps.DriverCaps.CanAutoGenerateMipMap) {
				caps.SetCap(Capabilities.HardwareMipMaps);
			}

			// blending between stages is definately supported
			caps.SetCap(Capabilities.TextureBlending);
			caps.SetCap(Capabilities.MultiTexturing);

			// Dot3 bump mapping?
			if(d3dCaps.TextureOperationCaps.SupportsDotProduct3) {
				caps.SetCap(Capabilities.Dot3);
			}

			// Cube mapping?
			if(d3dCaps.TextureCaps.SupportsCubeMap) {
				caps.SetCap(Capabilities.CubeMapping);
			}

			// Texture Compression
			// We always support compression, D3DX will decompress if device does not support
			caps.SetCap(Capabilities.TextureCompression);
			caps.SetCap(Capabilities.TextureCompressionDXT);

			// D3D uses vertex buffers for everything
			caps.SetCap(Capabilities.VertexBuffer);

			// Scissor test
			if(d3dCaps.RasterCaps.SupportsScissorTest) {
				caps.SetCap(Capabilities.ScissorTest);
			}

			// 2 sided stencil
			if(d3dCaps.StencilCaps.SupportsTwoSided) {
				caps.SetCap(Capabilities.TwoSidedStencil);
			}

			// stencil wrap
			if(d3dCaps.StencilCaps.SupportsIncrement && d3dCaps.StencilCaps.SupportsDecrement) {
				caps.SetCap(Capabilities.StencilWrap);
			}

			// Hardware Occlusion
			try {
				D3D.Query test = new D3D.Query(device, QueryType.Occlusion);

				// if we made it this far, it is supported
				caps.SetCap(Capabilities.HardwareOcculusion);

				test.Dispose();
			}
			catch {
				// eat it, this is not supported
				// TODO: Isn't there a better way to check for D3D occlusion query support?
			}

			if(d3dCaps.MaxUserClipPlanes > 0) {
				caps.SetCap(Capabilities.UserClipPlanes);
			}

			CheckVertexProgramCaps();

			CheckFragmentProgramCaps();

            Driver driver = D3DHelper.GetDriverInfo();

            AdapterDetails details = D3D.Manager.Adapters[driver.AdapterNumber].Information;

            caps.DeviceName = details.Description;
            caps.DriverVersion = details.DriverVersion.ToString();

            try {
                //
                // use the dxdiag interface to get the actual size of video memory
                //
                Container container = new Container(false);
                container = container.GetContainer("DxDiag_DisplayDevices").Container;
                container = container.GetContainer(0).Container;

                Microsoft.DirectX.Diagnostics.PropertyData prop = container.GetProperty("szDisplayMemoryEnglish");
                string s = prop.Data as string;

                int numStartOffset = -1;
                for (int i = 0; i < s.Length; i++) {
                    char c = s[i];
                    if (char.IsDigit(s[i])) {
                        numStartOffset = i;
                        break;
                    }
                }

                if (numStartOffset >= 0) {
                    int numEndOffset;
                    for (numEndOffset = numStartOffset; numEndOffset < s.Length; numEndOffset++) {
                        if (!char.IsDigit(s[numEndOffset])) {
                            break;
                        }
                    }

                    string numString = s.Substring(numStartOffset, numEndOffset - numStartOffset);
                    //LogManager.Instance.Write("DXDiag memory size string returned: {0}", s);
                    //LogManager.Instance.Write("parsing memory size string: {0}", numString);

                    caps.VideoMemorySize = int.Parse(numString);
                    //LogManager.Instance.Write("Parsed memory size value is: {0}", caps.VideoMemorySize);

                } else {
                    // couldn't determine size
                    caps.VideoMemorySize = 0;
                }
            } catch (Exception) {
#if FALSE_WARNING_IS_FIXED
                log.Warn("DXDiag unable to determine memory size.");
#endif
                // couldn't determine size
                caps.VideoMemorySize = 0;
            }

			// Infinite projection?
			// We have no capability for this, so we have to base this on our
			// experience and reports from users
			// Non-vertex program capable hardware does not appear to support it
			if(caps.CheckCap(Capabilities.VertexPrograms)) {
				// GeForce4 Ti (and presumably GeForce3) does not
				// render infinite projection properly, even though it does in GL
				// So exclude all cards prior to the FX range from doing infinite

				// not nVidia or GeForceFX and above
				if(details.VendorId != 0x10DE || details.DeviceId >= 0x0301) {
					caps.SetCap(Capabilities.InfiniteFarPlane);
				}
			}

            // TODO: Add the rest of the caps.
            caps.NumMultiRenderTargets = d3dCaps.NumberSimultaneousRts;

			// write hardware capabilities to registered log listeners
			caps.Log();
		}

		private void CheckFragmentProgramCaps()
		{
			int fpMajor = d3dCaps.PixelShaderVersion.Major;
			int fpMinor = d3dCaps.PixelShaderVersion.Minor;

			switch(fpMajor) 
			{
				case 1:
					caps.MaxFragmentProgramVersion = string.Format("ps_1_{0}", fpMinor);

					caps.FragmentProgramConstantIntCount = 0;
					// 8 4d float values, entered as floats but stored as fixed
					caps.FragmentProgramConstantFloatCount = 8;
					break;

				case 2:
					if(fpMinor > 0) {
						caps.MaxFragmentProgramVersion = "ps_2_x";
						//16 integer params allowed
						caps.FragmentProgramConstantIntCount = 16 * 4;
						// 4d float params
						caps.FragmentProgramConstantFloatCount = 224;
					}
					else {
						caps.MaxFragmentProgramVersion = "ps_2_0";
						// no integer params allowed
						caps.FragmentProgramConstantIntCount = 0;
						// 4d float params
						caps.FragmentProgramConstantFloatCount = 32;
					}

					break;

				case 3:
					if(fpMinor > 0) {
						caps.MaxFragmentProgramVersion = "ps_3_x";
					}
					else {
						caps.MaxFragmentProgramVersion = "ps_3_0";
					}

					// 16 integer params allowed
					caps.FragmentProgramConstantIntCount = 16;
					caps.FragmentProgramConstantFloatCount = 224;
					break;

				default:
					// doh, SOL
					caps.MaxFragmentProgramVersion = "";
					break;
			}

			// Fragment Program syntax code checks
			if(fpMajor >= 1) {
				caps.SetCap(Capabilities.FragmentPrograms);
				gpuProgramMgr.PushSyntaxCode("ps_1_1");

				if(fpMajor > 1 || fpMinor >= 2) {
					gpuProgramMgr.PushSyntaxCode("ps_1_2");
				}
				if(fpMajor > 1 || fpMinor >= 3) {
					gpuProgramMgr.PushSyntaxCode("ps_1_3");
				}
				if(fpMajor > 1 || fpMinor >= 4) {
					gpuProgramMgr.PushSyntaxCode("ps_1_4");
				}
			}

			if(fpMajor >= 2) {
				gpuProgramMgr.PushSyntaxCode("ps_2_0");

                if (d3dCaps.PixelShaderCaps.SupportsGradientInstructions)
                {
                    gpuProgramMgr.PushSyntaxCode("ps_2_a");
                }
				if(fpMajor > 2 || fpMinor > 0) {
					gpuProgramMgr.PushSyntaxCode("ps_2_x");
				}
			}

			if(fpMajor >= 3) {
				gpuProgramMgr.PushSyntaxCode("ps_3_0");

				if(fpMinor > 0) {
					gpuProgramMgr.PushSyntaxCode("ps_3_x");
				}
			}
		}

		private void CheckVertexProgramCaps()
		{
			int vpMajor = d3dCaps.VertexShaderVersion.Major;
			int vpMinor = d3dCaps.VertexShaderVersion.Minor;

			// check vertex program caps
			switch(vpMajor) {
				case 1:
					caps.MaxVertexProgramVersion = "vs_1_1";
					// 4d float vectors
					caps.VertexProgramConstantFloatCount = d3dCaps.MaxVertexShaderConst;
					// no int params supports
					caps.VertexProgramConstantIntCount = 0;
					break;
				case 2:
					if(vpMinor > 0) {
						caps.MaxVertexProgramVersion = "vs_2_x";
					}
					else {
						caps.MaxVertexProgramVersion = "vs_2_0";
					}

					// 16 ints
					caps.VertexProgramConstantIntCount = 16 * 4;
					// 4d float vectors
					caps.VertexProgramConstantFloatCount = d3dCaps.MaxVertexShaderConst;

					break;
				case 3:
					caps.MaxVertexProgramVersion = "vs_3_0";

					// 16 ints
					caps.VertexProgramConstantIntCount = 16 * 4;
					// 4d float vectors
					caps.VertexProgramConstantFloatCount = d3dCaps.MaxVertexShaderConst;

					break;
				default:
					// not gonna happen
					caps.MaxVertexProgramVersion = "";
					break;
			}

			// check for supported vertex program syntax codes
			if(vpMajor >= 1) {
				caps.SetCap(Capabilities.VertexPrograms);
				gpuProgramMgr.PushSyntaxCode("vs_1_1");
			}
			if(vpMajor >= 2) {
				if(vpMajor > 2 || vpMinor > 0) {
					gpuProgramMgr.PushSyntaxCode("vs_2_x");
				}
				gpuProgramMgr.PushSyntaxCode("vs_2_0");
			}
			if(vpMajor >= 3) {
				gpuProgramMgr.PushSyntaxCode("vs_3_0");
			}
		}

        private static D3D.DepthFormat[] PreferredStencilFormats = {
            DepthFormat.D24SingleS8,
            DepthFormat.D24S8,
            DepthFormat.D24X4S4,
            DepthFormat.D24X8,
            DepthFormat.D15S1,
            DepthFormat.D16,
            DepthFormat.D32
        };

        private D3D.DepthFormat GetDepthStencilFormatFor(D3D.Format fmt) {
            D3D.DepthFormat dsfmt;
            /// Check if result is cached
            if (depthStencilCache.TryGetValue(fmt, out dsfmt))
                return dsfmt;
            /// If not, probe with CheckDepthStencilMatch
            dsfmt = D3D.DepthFormat.Unknown;
            /// Get description of primary render target
            Surface surface = primaryWindow.RenderSurface;
            SurfaceDescription srfDesc = surface.Description;

            /// Probe all depth stencil formats
			/// Break on first one that matches
            foreach (DepthFormat x in PreferredStencilFormats) {
                // Verify that the depth format exists
                if (!D3D.Manager.CheckDeviceFormat(activeD3DDriver.AdapterNumber, DeviceType.Hardware,
                                                   srfDesc.Format, Usage.DepthStencil, ResourceType.Surface, x))
                    continue;
                // Verify that the depth format is compatible
                if (D3D.Manager.CheckDepthStencilMatch(activeD3DDriver.AdapterNumber, DeviceType.Hardware,
                                                       srfDesc.Format, fmt, x)) {
                    dsfmt = x;
                    break;
                }
            }
    		/// Cache result
	    	depthStencilCache[fmt] = dsfmt;
            return dsfmt;
        }

        private Surface GetDepthStencilFor(D3D.Format fmt, D3D.MultiSampleType multisample, 
                                           int width, int height)
        {
            D3D.DepthFormat dsfmt = GetDepthStencilFormatFor(fmt);
            if (dsfmt == D3D.DepthFormat.Unknown)
                return null;
            Surface surface = null;
            /// Check if result is cached
            ZBufferFormat zbfmt = new ZBufferFormat(dsfmt, multisample);
            Surface cachedSurface;
            if (zBufferCache.TryGetValue(zbfmt, out cachedSurface)) {
                /// Check if size is larger or equal
                if (cachedSurface.Description.Width >= width && 
                    cachedSurface.Description.Height >= height) {
                    surface = cachedSurface;
                } else {
                    zBufferCache.Remove(zbfmt);
                    cachedSurface.Dispose();
                }
            }
            if (surface == null) {
                /// If not, create the depthstencil surface
                surface = device.CreateDepthStencilSurface(width, height, dsfmt, multisample, 0, true);
                zBufferCache[zbfmt] = surface;
            }
            return surface;
        }

        protected void CleanupDepthStencils() {
            foreach (Surface surface in zBufferCache.Values) {
                /// Release buffer
                surface.Dispose();
            }
            zBufferCache.Clear();
        }

        public override void Dispose() {
            FreeDevice();
        }

		/// <summary>
		///		Helper method that converts a DX Matrix to our Matrix4.
		/// </summary>
		/// <param name="d3dMat"></param>
		/// <returns></returns>
		private Matrix4 ConvertD3DMatrix(ref DX.Matrix d3dMat) {
			Matrix4 mat = Matrix4.Zero;

			mat.m00 = d3dMat.M11;
			mat.m10 = d3dMat.M12;
			mat.m20 = d3dMat.M13;
			mat.m30 = d3dMat.M14;

			mat.m01 = d3dMat.M21;
			mat.m11 = d3dMat.M22;
			mat.m21 = d3dMat.M23;
			mat.m31 = d3dMat.M24;

			mat.m02 = d3dMat.M31;
			mat.m12 = d3dMat.M32;
			mat.m22 = d3dMat.M33;
			mat.m32 = d3dMat.M34;

			mat.m03 = d3dMat.M41;
			mat.m13 = d3dMat.M42;
			mat.m23 = d3dMat.M43;
			mat.m33 = d3dMat.M44;

			return mat;
		}

        [DllImport("d3d9.dll", CharSet = CharSet.Auto)]
        private static extern int D3DPERF_BeginEvent(uint color, [MarshalAs(UnmanagedType.LPWStr)]string message);

        [DllImport("d3d9.dll", CharSet = CharSet.Auto)]
        private static extern int D3DPERF_EndEvent();

        [DllImport("d3d9.dll", CharSet = CharSet.Auto)]
        private static extern void D3DPERF_SetMarker(uint color, [MarshalAs(UnmanagedType.LPWStr)]string message);

        /// <summary>
        /// This is used to insert events into the PIX trace for DirectX debugging and profiling.
        /// </summary>
        /// <param name="color">Color to display the event</param>
        /// <param name="message">Message to display</param>
        /// <returns>nesting level of events</returns>
        public override int BeginProfileEvent(ColorEx color, string message)
        {
            return D3DPERF_BeginEvent(color.ToARGB(), message);
        }

        /// <summary>
        /// This is used to end an event in the PIX trace on DirectX
        /// </summary>
        /// <returns>nesting level of events</returns>
        public override int EndProfileEvent()
        {
            return D3DPERF_EndEvent();
        }

        /// <summary>
        /// Set an instantaneous marker in the profiling event log.  (PIX for DirectX)
        /// </summary>
        /// <param name="color"></param>
        /// <param name="message"></param>
        public override void SetProfileMarker(ColorEx color, string message)
        {
            D3DPERF_SetMarker(color.ToARGB(), message);
        }
	}

	/// <summary>
	///		Structure holding texture unit settings for every stage
	/// </summary>
	internal struct D3DTextureStageDesc {
		/// the type of the texture
		public D3DTexType texType;
		/// wich texCoordIndex to use
		public int coordIndex;
		/// type of auto tex. calc. used
		public TexCoordCalcMethod autoTexCoordType;
		/// Frustum, used if the above is projection
		public Frustum frustum;
		/// texture 
		public D3D.BaseTexture tex;
	}

	/// <summary>
	///		D3D texture types
	/// </summary>
	public enum D3DTexType {
		Normal,
		Cube,
		Volume,
		None
	}
}
