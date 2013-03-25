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
using System.IO;
using log4net;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Media;
using Axiom.Utility;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
    /// <summary>
    /// Summary description for D3DTexture.
    /// </summary>
    /// <remarks>When loading a cubic texture, the image with the texture base name plus the "_rt", "_lf", "_up", "_dn", "_fr", "_bk" suffixes will automaticaly be loaded to construct it.</remarks>
    public class D3DTexture : Axiom.Core.Texture {
        #region Fields
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(D3DRenderWindow));

        private static TimingMeter textureLoadMeter = MeterManager.GetMeter("Texture Load", "D3DTexture");

        /// <summary>
        ///     Direct3D device reference.
        /// </summary>
        private D3D.Device device;
        /// <summary>
        ///     Actual texture reference.
        /// </summary>
        private D3D.BaseTexture texture;
        /// <summary>
        ///     1D/2D normal texture.
        /// </summary>
        private D3D.Texture normTexture;
        /// <summary>
        ///     Cubic texture reference.
        /// </summary>
        private D3D.CubeTexture cubeTexture;
        /// <summary>
        ///     Temporary cubic texture reference.
        /// </summary>
        private D3D.CubeTexture tempCubeTexture = null;
        /// <summary>
        ///     3D volume texture.
        /// </summary>
        private D3D.VolumeTexture volumeTexture;
        /// <summary>
        ///     Back buffer pixel format.
        /// </summary>
        private D3D.Format bbPixelFormat;
        /// <summary>
        ///     The memory pool being used
        /// </summary>
        private D3D.Pool d3dPool = D3D.Pool.Managed;
        /// <summary>
        ///     Direct3D device creation parameters.
        /// </summary>
        private D3D.DeviceCreationParameters devParms;
        /// <summary>
        ///     Direct3D device capability structure.
        /// </summary>
        private D3D.Caps devCaps;
        /// <summary>
        ///     Array to hold texture names used for loading cube textures.
        /// </summary>
        private string[] cubeFaceNames = new string[6];
        /// <summary>
        ///     Dynamic textures?
        /// </summary>
        private bool dynamicTextures = false;
        /// <summary>
        ///     List of subsurfaces
        /// </summary>
        ///
        // private List<D3DHardwarePixelBuffer> surfaceList = new List<D3DHardwarePixelBuffer>();
        internal List<D3DHardwarePixelBuffer> surfaceList = new List<D3DHardwarePixelBuffer>();

        private List<IDisposable> managedObjects = new List<IDisposable>();

        /// <summary>
        ///   Each call to D3DTexture.GetSurfaceLevel returns a new object.
        ///   When I call Surface.GetGraphics, I need to call ReleaseGraphics 
        ///   on the same object, so cache it.
        /// </summary>
        private Surface graphicsSurface = null;

        #endregion Fields

        //public D3DTexture(string name, D3D.Device device, TextureUsage usage, TextureType type)
        //    : this(name, device, type, 0, 0, 0, PixelFormat.Unknown, usage) {}

        public D3DTexture(string name, bool isManual, D3D.Device device) {
            Debug.Assert(device != null, "Cannot create a texture without a valid D3D Device.");
            this.name = name;
            this.device = device;
            this.isManual = isManual;

            InitDevice();
        }

        //    // set the name of the cubemap faces
        //    if(this.TextureType == TextureType.CubeMap) {
        //        ConstructCubeFaceNames(name);
        //    }

        //    // get device caps
        //    devCaps = device.DeviceCaps;

        //    // save off the params used to create the Direct3D device
        //    this.device = device;
        //    devParms = device.CreationParameters;

        //    // get the pixel format of the back buffer
        //    using(D3D.Surface back = device.GetBackBuffer(0, 0, D3D.BackBufferType.Mono)) {
        //        bbPixelFormat = back.Description.Format;
        //    }

        //    SetSrcAttributes(width, height, 1, format);

        //    // if render target, create the texture up front
        //    if(usage == TextureUsage.RenderTarget) {
        //        // for render texture, use the format we actually asked for
        //        bbPixelFormat = ConvertFormat(format);
        //        CreateTexture();
        //        isLoaded = true;
        //    }
        //}

        #region Properties

        /// <summary>
        ///		Gets the D3D Texture that is contained withing this Texture.
        /// </summary>
        public D3D.BaseTexture DXTexture {
            get {
                return texture;
            }
        }

        public D3D.Texture NormalTexture {
            get {
                return normTexture;
            }
        }

        public D3D.CubeTexture CubeTexture {
            get {
                return cubeTexture;
            }
        }

        public D3D.VolumeTexture VolumeTexture {
            get {
                return volumeTexture;
            }
        }

        //public D3D.Surface DepthStencil {
        //    get {
        //        return depthBuffer;
        //    }
        //}

        #endregion

        #region Methods

        protected override void FreeInternalResourcesImpl() {
            if (texture != null) {
                texture.Dispose();
                texture = null;
            }
            if (normTexture != null) {
                normTexture.Dispose();
                normTexture = null;
            }
            if (cubeTexture != null) {
                cubeTexture.Dispose();
                cubeTexture = null;
            }
            if (volumeTexture != null) {
                volumeTexture.Dispose();
                volumeTexture = null;
            }

            foreach (IDisposable buf in managedObjects)
                buf.Dispose();
            managedObjects.Clear();
        }

        public override void Preload() {
            throw new NotImplementedException();
        }

        protected override void LoadImpl() {
            if ((usage & TextureUsage.RenderTarget) != 0) {
                CreateInternalResources();
                isLoaded = true;
                return;
            }

            if (!internalResourcesCreated) {
                // NB: Need to initialize pool to some value other than D3D.Pool.Managed,
                // otherwise, if the texture loading failed, it might re-create as empty
                // texture when device lost/restore. The actual pool will determine later.
                d3dPool = D3D.Pool.Managed;
            }

            // create a regular texture
            switch (this.TextureType) {
                case TextureType.OneD:
                case TextureType.TwoD:
                    LoadNormalTexture();
                    break;

                case TextureType.ThreeD:
                    LoadVolumeTexture();
                    break;

                case TextureType.CubeMap:
                    LoadCubeTexture();
                    break;

                default:
                    throw new AxiomException("Unknown texture type in D3DTexture.LoadImpl");
            }
        }

        protected void InitDevice() {
            Debug.Assert(device != null);
            // get device caps
            devCaps = device.DeviceCaps;

            // get our device creation parameters
            devParms = device.CreationParameters;

            // get our back buffer pixel format
            using (D3D.Surface back = device.GetBackBuffer(0, 0, D3D.BackBufferType.Mono)) {
                bbPixelFormat = back.Description.Format;
            }
        }

        public override void LoadImage(Image image) {
            List<Image> images = new List<Image>();
            images.Add(image);
            LoadImages(images);
        }



        /// <summary>
        /// 
        /// </summary>
        public override void Dispose() {
            if (isLoaded)
                Unload();
            else
                FreeInternalResources();
            ClearSurfaceList();
            foreach (IDisposable buf in managedObjects)
                buf.Dispose();
            base.Dispose();
        }

        /// <summary>
        ///    
        /// </summary>
        private void ConstructCubeFaceNames(string name) {
            string baseName, ext;
            string[] postfixes = { "_rt", "_lf", "_up", "_dn", "_fr", "_bk" };

            int pos = name.LastIndexOf(".");

            baseName = name.Substring(0, pos);
            ext = name.Substring(pos + 1);

            for (int i = 0; i < 6; i++) {
                cubeFaceNames[i] = baseName + postfixes[i] + "." + ext;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadCubeTexture() {
            Debug.Assert(this.TextureType == TextureType.CubeMap);
            using (AutoTimer auto = new AutoTimer(textureLoadMeter)) {
                // DDS load?
                if (name.EndsWith(".dds")) {
                    Stream stream = TextureManager.Instance.FindResourceData(name);

                    // use D3DX to load the image directly from the stream
                    int numMips = numRequestedMipmaps + 1;
                    // check if mip map volume textures are supported
                    if (!(devCaps.TextureCaps.SupportsMipCubeMap)) {
                        // no mip map support for this kind of textures :(
                        numMipmaps = 0;
                        numMips = 1;
                    }
                    // Determine D3D pool to use
                    D3D.Pool pool;
                    if ((usage & TextureUsage.Dynamic) != 0)
                        pool = D3D.Pool.Default;
                    else
                        pool = D3D.Pool.Managed;
                    Debug.Assert(cubeTexture == null);
                    Debug.Assert(texture == null);

                    // load the cube texture from the image data stream directly
                    cubeTexture =
                        TextureLoader.FromCubeStream(device, stream, (int)stream.Length, 0, numMips,
                                                 D3D.Usage.None,
                                                 D3D.Format.Unknown,
                                                 pool,
                                                 Filter.Triangle | Filter.Dither,
                                                 Filter.Box, 0);

                    // store off a base reference
                    texture = cubeTexture;

                    // set the image data attributes
                    SurfaceDescription desc = cubeTexture.GetLevelDescription(0);
                    d3dPool = desc.Pool;
                    // set src and dest attributes to the same, we can't know
                    SetSrcAttributes(desc.Width, desc.Height, 1, D3DHelper.ConvertEnum(desc.Format));
                    SetFinalAttributes(desc.Width, desc.Height, 1, D3DHelper.ConvertEnum(desc.Format));

                    isLoaded = true;
                    internalResourcesCreated = true;
                    
                    stream.Dispose();
                } else {
                    // Load from 6 separate files
                    // Let Axiom use its own codecs
                    ConstructCubeFaceNames(name);

                    List<Image> images = new List<Image>();

                    int pos = name.LastIndexOf('.');
                    string ext = name.Substring(pos + 1);
                    for (int i = 0; i < 6; ++i) {
                        // find & load resource data into stream
                        Stream stream = TextureManager.Instance.FindResourceData(cubeFaceNames[i]);
                        images.Add(Image.FromStream(stream, ext));
                        stream.Dispose();
                    }
                    LoadImages(images);
                }
            } // using
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadVolumeTexture() {
            Debug.Assert(this.TextureType == TextureType.ThreeD);
            using (AutoTimer auto = new AutoTimer(textureLoadMeter)) {
                // DDS load?
                if (name.EndsWith(".dds")) {
                    Stream stream = TextureManager.Instance.FindResourceData(name);

                    // use D3DX to load the image directly from the stream
                    int numMips = numRequestedMipmaps + 1;
                    // check if mip map volume textures are supported
                    if (!(devCaps.TextureCaps.SupportsMipVolumeMap)) {
                        // no mip map support for this kind of textures :(
                        numMipmaps = 0;
                        numMips = 1;
                    }
                    // Determine D3D pool to use
                    D3D.Pool pool;
                    if ((usage & TextureUsage.Dynamic) != 0)
                        pool = D3D.Pool.Default;
                    else
                        pool = D3D.Pool.Managed;
                    Debug.Assert(volumeTexture == null);
                    Debug.Assert(texture == null);

                    // load the cube texture from the image data stream directly
                    volumeTexture =
                        TextureLoader.FromVolumeStream(device, stream, (int)stream.Length, 0, 0, 0, numMips,
                                                       D3D.Usage.None,
                                                       D3D.Format.Unknown,
                                                       pool,
                                                       Filter.Triangle | Filter.Dither,
                                                       Filter.Box, 0);

                    // store off a base reference
                    texture = volumeTexture;

                    // set the image data attributes
                    VolumeDescription desc = volumeTexture.GetLevelDescription(0);
                    d3dPool = desc.Pool;
                    // set src and dest attributes to the same, we can't know
                    SetSrcAttributes(desc.Width, desc.Height, desc.Depth, D3DHelper.ConvertEnum(desc.Format));
                    SetFinalAttributes(desc.Width, desc.Height, desc.Depth, D3DHelper.ConvertEnum(desc.Format));

                    isLoaded = true;
                    internalResourcesCreated = true;

                    stream.Dispose();
                } else {
                    // find & load resource data into stream
                    Stream stream = TextureManager.Instance.FindResourceData(name);
                    int pos = name.LastIndexOf('.');
                    string ext = name.Substring(pos + 1);
                    Image img = Image.FromStream(stream, ext);
                    LoadImage(img);
                    stream.Dispose();
                }
            } // using
        }

        /// <summary>
        ///    
        /// </summary>
        private void LoadNormalTexture() {
            Debug.Assert(textureType == TextureType.OneD || textureType == TextureType.TwoD);
            using (AutoTimer auto = new AutoTimer(textureLoadMeter)) {
                // DDS load?
                if (name.EndsWith(".dds")) {
                    Stream stream = TextureManager.Instance.FindResourceData(name);

                    // use D3DX to load the image directly from the stream
                    int numMips = numRequestedMipmaps + 1;
                    // check if mip map volume textures are supported
                    if (!(devCaps.TextureCaps.SupportsMipMap)) {
                        // no mip map support for this kind of textures :(
                        numMipmaps = 0;
                        numMips = 1;
                    }
                    // Determine D3D pool to use
                    D3D.Pool pool;
                    if ((usage & TextureUsage.Dynamic) != 0)
                        pool = D3D.Pool.Default;
                    else
                        pool = D3D.Pool.Managed;
                    Debug.Assert(normTexture == null);
                    Debug.Assert(texture == null);
                    // Trace.TraceInformation("Loaded normal texture {0}", this.Name);
                    normTexture =
                        TextureLoader.FromStream(device, stream, 0, 0, numMips,
                                                 D3D.Usage.None,
                                                 D3D.Format.Unknown,
                                                 pool,
                                                 Filter.Triangle | Filter.Dither,
                                                 Filter.Box, 0);

                    // store a ref for the base texture interface
                    texture = normTexture;

                    // set the image data attributes
                    SurfaceDescription desc = normTexture.GetLevelDescription(0);
                    d3dPool = desc.Pool;
                    // set src and dest attributes to the same, we can't know
                    SetSrcAttributes(desc.Width, desc.Height, 1, D3DHelper.ConvertEnum(desc.Format));
                    SetFinalAttributes(desc.Width, desc.Height, 1, D3DHelper.ConvertEnum(desc.Format));

                    isLoaded = true;
                    internalResourcesCreated = true;
                } else {
                    // find & load resource data into stream
                    Stream stream = TextureManager.Instance.FindResourceData(name);
                    int pos = name.LastIndexOf('.');
                    string ext = name.Substring(pos + 1);
                    Image img = Image.FromStream(stream, ext);
                    LoadImage(img);
                    stream.Dispose();
                }
            } // using
        }


        /// <summary>
        /// 
        /// </summary>
        //private void CreateDepthStencil() {
        //    // Get the format of the depth stencil surface of our main render target.
        //    D3D.Surface surface = device.DepthStencilSurface;
        //    D3D.SurfaceDescription desc = surface.Description;

        //    // Create a depth buffer for our render target, it must be of
        //    // the same format as other targets !!!
        //    depthBuffer = device.CreateDepthStencilSurface(
        //        srcWidth,
        //        srcHeight,
        //        // TODO: Verify this goes through, this is ridiculous
        //        (D3D.DepthFormat)desc.Format,
        //        desc.MultiSampleType,
        //        desc.MultiSampleQuality,
        //        false);
        //}

        private void CreateNormalTexture() {
            // we must have those defined here
            Debug.Assert(srcWidth > 0 && srcHeight > 0);

            // determine which D3D9 pixel format we'll use
            D3D.Format d3dPixelFormat = ChooseD3DFormat();
            
            // at this point, Ogre checks to see if this texture format works,
            // but we go on and figure out the rest of our info first.

            // set the appropriate usage based on the usage of this texture
            D3D.Usage d3dUsage =
                ((usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget) ? D3D.Usage.RenderTarget : D3D.Usage.None;

            // how many mips to use?
            int numMips = numRequestedMipmaps + 1;

            // Check dynamic textures
            if ((usage & TextureUsage.Dynamic) != 0) {
                if (CanUseDynamicTextures(d3dUsage, ResourceType.Textures, d3dPixelFormat)) {
                    d3dUsage |= D3D.Usage.Dynamic;
                    dynamicTextures = true;
                } else {
                    dynamicTextures = false;
                }
            }
            // check if mip maps are supported on hardware
            mipmapsHardwareGenerated = false;
            if (devCaps.TextureCaps.SupportsMipMap) {
                if (((usage & TextureUsage.AutoMipMap) == TextureUsage.AutoMipMap)
                    && numRequestedMipmaps > 0)
                {
                    // use auto.gen. if available
                    mipmapsHardwareGenerated = this.CanAutoGenMipMaps(d3dUsage, ResourceType.Textures, d3dPixelFormat);
                    if (mipmapsHardwareGenerated) {
                        d3dUsage |= D3D.Usage.AutoGenerateMipMap;
                        numMips = 0;
                    }
                }
            } else {
                // no mip map support for this kind of texture
                numMipmaps = 0;
                numMips = 1;
            }

            // check texture requirements
            D3D.TextureRequirements texRequire = new D3D.TextureRequirements();
            texRequire.Width = srcWidth;
            texRequire.Height = srcHeight;            
            texRequire.NumberMipLevels = numMips;
            texRequire.Format = d3dPixelFormat;
            // NOTE: Although texRequire is an out parameter, it actually does 
            //       use the data passed in with that object.
            TextureLoader.CheckTextureRequirements(device, d3dUsage, Pool.Default, out texRequire);
            numMips = texRequire.NumberMipLevels;
            d3dPixelFormat = texRequire.Format;
            Debug.Assert(normTexture == null);
            Debug.Assert(texture == null);
            log.InfoFormat("Created normal texture {0}", this.Name);
            // create the texture
            normTexture = new D3D.Texture(
                    device,
                    srcWidth,
                    srcHeight,
                    numMips,
                    d3dUsage,
                    d3dPixelFormat,
                    d3dPool);

            // store base reference to the texture
            texture = normTexture;

            // set the final texture attributes
            D3D.SurfaceDescription desc = normTexture.GetLevelDescription(0);
            SetFinalAttributes(desc.Width, desc.Height, 1, D3DHelper.ConvertEnum(desc.Format));

            if (mipmapsHardwareGenerated)
                texture.AutoGenerateFilterType = GetBestFilterMethod();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateCubeTexture() {
            // we must have those defined here
            Debug.Assert(srcWidth > 0 && srcHeight > 0);

            // determine which D3D9 pixel format we'll use
            D3D.Format d3dPixelFormat = ChooseD3DFormat();

            // at this point, Ogre checks to see if this texture format works,
            // but we go on and figure out the rest of our info first.

            // set the appropriate usage based on the usage of this texture
            D3D.Usage d3dUsage =
                ((usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget) ? D3D.Usage.RenderTarget : D3D.Usage.None;

            // how many mips to use?
            int numMips = numRequestedMipmaps + 1;

            // Check dynamic textures
            if ((usage & TextureUsage.Dynamic) != 0) {
                if (CanUseDynamicTextures(d3dUsage, ResourceType.CubeTexture, d3dPixelFormat)) {
                    d3dUsage |= D3D.Usage.Dynamic;
                    dynamicTextures = true;
                } else {
                    dynamicTextures = false;
                }
            }
            // check if mip maps are supported on hardware
            mipmapsHardwareGenerated = false;
            if (devCaps.TextureCaps.SupportsMipCubeMap) {
                if (((usage & TextureUsage.AutoMipMap) == TextureUsage.AutoMipMap)
                    && numRequestedMipmaps > 0) 
                {
                    // use auto.gen. if available
                    mipmapsHardwareGenerated = this.CanAutoGenMipMaps(d3dUsage, ResourceType.CubeTexture, d3dPixelFormat);
                    if (mipmapsHardwareGenerated) {
                        d3dUsage |= D3D.Usage.AutoGenerateMipMap;
                        numMips = 0;
                    }
                }
            } else {
                // no mip map support for this kind of texture
                numMipmaps = 0;
                numMips = 1;
            }

            // check texture requirements
            D3D.TextureRequirements texRequire = new D3D.TextureRequirements();
            texRequire.Width = srcWidth;
            texRequire.Height = srcHeight;
            texRequire.NumberMipLevels = numMips;
            texRequire.Format = d3dPixelFormat;
            // NOTE: Although texRequire is an out parameter, it actually does 
            //       use the data passed in with that object.
            TextureLoader.CheckTextureRequirements(device, d3dUsage, Pool.Default, out texRequire);
            numMips = texRequire.NumberMipLevels;
            d3dPixelFormat = texRequire.Format;
            Debug.Assert(cubeTexture == null);
            Debug.Assert(texture == null);
            log.InfoFormat("Created normal texture {0}", this.Name);
            // create the texture
            cubeTexture = new D3D.CubeTexture(
                    device,
                    srcWidth,
                    numMips,
                    d3dUsage,
                    d3dPixelFormat,
                    d3dPool);

            // store base reference to the texture
            texture = cubeTexture;

            // set the final texture attributes
            D3D.SurfaceDescription desc = cubeTexture.GetLevelDescription(0);
            SetFinalAttributes(desc.Width, desc.Height, 1, D3DHelper.ConvertEnum(desc.Format));

            if (mipmapsHardwareGenerated)
                texture.AutoGenerateFilterType = GetBestFilterMethod();
        }

        private void CreateVolumeTexture() {
            // we must have those defined here
            Debug.Assert(srcWidth > 0 && srcHeight > 0);

            // determine which D3D9 pixel format we'll use
            D3D.Format d3dPixelFormat = ChooseD3DFormat();

            // at this point, Ogre checks to see if this texture format works,
            // but we go on and figure out the rest of our info first.

            // set the appropriate usage based on the usage of this texture
            D3D.Usage d3dUsage =
                ((usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget) ? D3D.Usage.RenderTarget : D3D.Usage.None;

            // how many mips to use?
            int numMips = numRequestedMipmaps + 1;

            // Check dynamic textures
            if ((usage & TextureUsage.Dynamic) != 0) {
                if (CanUseDynamicTextures(d3dUsage, ResourceType.VolumeTexture, d3dPixelFormat)) {
                    d3dUsage |= D3D.Usage.Dynamic;
                    dynamicTextures = true;
                } else {
                    dynamicTextures = false;
                }
            }
            // check if mip maps are supported on hardware
            mipmapsHardwareGenerated = false;
            if (devCaps.TextureCaps.SupportsMipVolumeMap) {
                if (((usage & TextureUsage.AutoMipMap) == TextureUsage.AutoMipMap)
                    && numRequestedMipmaps > 0)
                {
                    // use auto.gen. if available
                    mipmapsHardwareGenerated = this.CanAutoGenMipMaps(d3dUsage, ResourceType.VolumeTexture, d3dPixelFormat);
                    if (mipmapsHardwareGenerated) {
                        d3dUsage |= D3D.Usage.AutoGenerateMipMap;
                        numMips = 0;
                    }
                }
            } else {
                // no mip map support for this kind of texture
                numMipmaps = 0;
                numMips = 1;
            }

            // check texture requirements
            D3D.TextureRequirements texRequire = new D3D.TextureRequirements();
            texRequire.Width = srcWidth;
            texRequire.Height = srcHeight;
            texRequire.NumberMipLevels = numMips;
            texRequire.Format = d3dPixelFormat;
            // NOTE: Although texRequire is an out parameter, it actually does 
            //       use the data passed in with that object.
            TextureLoader.CheckTextureRequirements(device, d3dUsage, Pool.Default, out texRequire);
            numMips = texRequire.NumberMipLevels;
            d3dPixelFormat = texRequire.Format;
            Debug.Assert(volumeTexture == null);
            Debug.Assert(texture == null);
            log.InfoFormat("Created normal texture {0}", this.Name);
            // create the texture
            volumeTexture = new D3D.VolumeTexture(
                    device,
                    srcWidth,
                    srcHeight,
                    srcDepth,
                    numMips,
                    d3dUsage,
                    d3dPixelFormat,
                    d3dPool);

            // store base reference to the texture
            texture = volumeTexture;

            // set the final texture attributes
            VolumeDescription desc = volumeTexture.GetLevelDescription(0);
            SetFinalAttributes(desc.Width, desc.Height, desc.Depth, D3DHelper.ConvertEnum(desc.Format));

            if (mipmapsHardwareGenerated)
                texture.AutoGenerateFilterType = GetBestFilterMethod();
        }

        protected void CreateSurfaceList() {
            Surface surface;
            Volume volume;
            D3DHardwarePixelBuffer buffer;
            Debug.Assert(texture != null);
            // Make sure number of mips is right
            numMipmaps = texture.LevelCount - 1;
            // Need to know static / dynamic
            BufferUsage bufusage;
            if (((usage & TextureUsage.Dynamic) != 0) && dynamicTextures)
                bufusage = BufferUsage.Dynamic;
            else
                bufusage = BufferUsage.Static;
            if ((usage & TextureUsage.RenderTarget) != 0)
                bufusage = (BufferUsage)((int)bufusage | (int)TextureUsage.RenderTarget);

            // If we already have the right number of surfaces, just update the old list
            bool updateOldList = (surfaceList.Count == (this.NumFaces * (numMipmaps + 1)));
            if (!updateOldList) {
                // Create new list of surfaces
                ClearSurfaceList();
                for (int face = 0; face < this.NumFaces; ++face) {
                    for (int mip = 0; mip <= numMipmaps; ++mip) {
                        buffer = new D3DHardwarePixelBuffer(bufusage);
                        surfaceList.Add(buffer);
                    }
                }
            }

            switch (textureType) {
                case TextureType.OneD:
                case TextureType.TwoD:
                    Debug.Assert(normTexture != null);
                    // For all mipmaps, store surfaces as HardwarePixelBuffer
                    for (int mip = 0; mip <= numMipmaps; ++mip) {
                        surface = normTexture.GetSurfaceLevel(mip);
                        // decrement reference count, the GetSurfaceLevel call increments this
                        // this is safe because the texture keeps a reference as well
                        // surface->Release();
                        GetSurfaceAtLevel(0, mip).Bind(device, surface, updateOldList);
                        managedObjects.Add(surface);
                    }
                    break;
                case TextureType.CubeMap:
                    Debug.Assert(cubeTexture != null);
                    // For all faces and mipmaps, store surfaces as HardwarePixelBufferSharedPtr
                    for (int face = 0; face < 6; ++face) {
                        for (int mip = 0; mip <= numMipmaps; ++mip) {
                            surface = cubeTexture.GetCubeMapSurface((CubeMapFace)face, mip);
                            // decrement reference count, the GetSurfaceLevel call increments this
                            // this is safe because the texture keeps a reference as well
                            // surface->Release();
                            GetSurfaceAtLevel(face, mip).Bind(device, surface, updateOldList);
                            managedObjects.Add(surface);
                        }
                    }
                    break;
                case TextureType.ThreeD:
                    Debug.Assert(volumeTexture != null);
                    // For all mipmaps, store surfaces as HardwarePixelBuffer
                    for (int mip = 0; mip <= numMipmaps; ++mip) {
                        volume = volumeTexture.GetVolumeLevel(mip);
                        // decrement reference count, the GetSurfaceLevel call increments this
                        // this is safe because the texture keeps a reference as well
                        // volume->Release();
                        GetSurfaceAtLevel(0, mip).Bind(device, volume, updateOldList);
                        managedObjects.Add(volume);
                    }
                    break;
            }
            // Set autogeneration of mipmaps for each face of the texture, if it is enabled
            if ((numRequestedMipmaps != 0) && ((usage & TextureUsage.AutoMipMap) != 0)) {
                for (int face = 0; face < this.NumFaces; ++face)
                    GetSurfaceAtLevel(face, 0).SetMipmapping(true, mipmapsHardwareGenerated, texture);
            }
        }

        private void ClearSurfaceList() {
            foreach (D3DHardwarePixelBuffer buf in surfaceList)
                buf.Dispose();
            surfaceList.Clear();
        }

        private D3DHardwarePixelBuffer GetSurfaceAtLevel(int face, int mip) {
            if (face > NumFaces)
                throw new Exception("Face index out of range");
            if (mip > NumMipMaps)
                throw new Exception("Mipmap index out of range");
            return surfaceList[face * (numMipmaps + 1) + mip];
        }

        /// <summary>
        ///   This method is pretty inefficient.  It calls stream.WriteByte.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="surface"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bpp"></param>
        /// <param name="alpha"></param>
        private static void CopyMemoryToSurface(byte[] buffer, D3D.Surface surface, int width, int height, int bpp, bool alpha) {
            // Copy the image from the buffer to the temporary surface.
            // We have to do our own colour conversion here since we don't 
            // have a DC to do it for us
            // NOTE - only non-palettised surfaces supported for now
            D3D.SurfaceDescription desc;
            int pBuf8, pitch;
            uint data32, out32;
            int iRow, iCol;

            // NOTE - dimensions of surface may differ from buffer
            // dimensions (e.g. power of 2 or square adjustments)
            // Lock surface
            desc = surface.Description;
            uint aMask, rMask, gMask, bMask, rgbBitCount;

            GetColorMasks(desc.Format, out rMask, out gMask, out bMask, out aMask, out rgbBitCount);

            // lock our surface to acces raw memory
            GraphicsStream stream = surface.LockRectangle(D3D.LockFlags.NoSystemLock, out pitch);

            // loop through data and do conv.
            pBuf8 = 0;
            for (iRow = 0; iRow < height; iRow++) {
                stream.Position = iRow * pitch;
                for (iCol = 0; iCol < width; iCol++) {
                    // Read RGBA values from buffer
                    data32 = 0;
                    if (bpp >= 24) {
                        // Data in buffer is in RGB(A) format
                        // Read into a 32-bit structure
                        // Uses bytes for 24-bit compatibility
                        // NOTE: buffer is big-endian
                        data32 |= (uint)buffer[pBuf8++] << 24;
                        data32 |= (uint)buffer[pBuf8++] << 16;
                        data32 |= (uint)buffer[pBuf8++] << 8;
                    } else if (bpp == 8 && !alpha) { // Greyscale, not palettised (palettised NOT supported)
                        // Duplicate same greyscale value across R,G,B
                        data32 |= (uint)buffer[pBuf8] << 24;
                        data32 |= (uint)buffer[pBuf8] << 16;
                        data32 |= (uint)buffer[pBuf8++] << 8;
                    }
                    // check for alpha
                    if (alpha) {
                        data32 |= buffer[pBuf8++];
                    } else {
                        data32 |= 0xFF;	// Set opaque
                    }

                    // Write RGBA values to surface
                    // Data in surface can be in varying formats
                    // Use bit concersion function
                    // NOTE: we use a 32-bit value to manipulate
                    // Will be reduced to size later

                    // Red
                    out32 = ConvertBitPattern(data32, 0xFF000000, rMask);
                    // Green
                    out32 |= ConvertBitPattern(data32, 0x00FF0000, gMask);
                    // Blue
                    out32 |= ConvertBitPattern(data32, 0x0000FF00, bMask);

                    // Alpha
                    if (aMask > 0) {
                        out32 |= ConvertBitPattern(data32, 0x000000FF, aMask);
                    }

                    // Assign results to surface pixel
                    // Write up to 4 bytes
                    // Surfaces are little-endian (low byte first)
                    if (rgbBitCount >= 8) {
                        stream.WriteByte((byte)out32);
                    }
                    if (rgbBitCount >= 16) {
                        stream.WriteByte((byte)(out32 >> 8));
                    }
                    if (rgbBitCount >= 24) {
                        stream.WriteByte((byte)(out32 >> 16));
                    }
                    if (rgbBitCount >= 32) {
                        stream.WriteByte((byte)(out32 >> 24));
                    }
                } // for( iCol...
            } // for( iRow...
            // unlock the surface
            surface.UnlockRectangle();
        }

        private void CopyMemoryToSurface(byte[] buffer, D3D.Surface surface) {
            CopyMemoryToSurface(buffer, surface, srcWidth, srcHeight, srcBpp, hasAlpha);
        }

        private static uint ConvertBitPattern(uint srcValue, uint srcBitMask, uint destBitMask) {
            // Mask off irrelevant source value bits (if any)
            srcValue = srcValue & srcBitMask;

            // Shift source down to bottom of DWORD
            int srcBitShift = GetBitShift(srcBitMask);
            srcValue >>= srcBitShift;

            // Get max value possible in source from srcMask
            uint srcMax = srcBitMask >> srcBitShift;

            // Get max avaiable in dest
            int destBitShift = GetBitShift(destBitMask);
            uint destMax = destBitMask >> destBitShift;

            // Scale source value into destination, and shift back
            uint destValue = (srcValue * destMax) / srcMax;
            return (destValue << destBitShift);
        }

        private static int GetBitShift(uint mask) {
            if (mask == 0)
                return 0;

            int result = 0;
            while ((mask & 1) == 0) {
                ++result;
                mask >>= 1;
            }
            return result;
        }

        private static void GetColorMasks(D3D.Format format, out uint red, out uint green, out uint blue, out uint alpha, out uint rgbBitCount) {
            // we choose the format of the D3D texture so check only for our pf types...
            switch (format) {
                case D3D.Format.X8R8G8B8:
                    red = 0x00FF0000; green = 0x0000FF00; blue = 0x000000FF; alpha = 0x00000000;
                    rgbBitCount = 32;
                    break;
                case D3D.Format.R8G8B8:
                    red = 0x00FF0000; green = 0x0000FF00; blue = 0x000000FF; alpha = 0x00000000;
                    rgbBitCount = 24;
                    break;
                case D3D.Format.A8R8G8B8:
                    red = 0x00FF0000; green = 0x0000FF00; blue = 0x000000FF; alpha = 0xFF000000;
                    rgbBitCount = 32;
                    break;
                case D3D.Format.X1R5G5B5:
                    red = 0x00007C00; green = 0x000003E0; blue = 0x0000001F; alpha = 0x00000000;
                    rgbBitCount = 16;
                    break;
                case D3D.Format.R5G6B5:
                    red = 0x0000F800; green = 0x000007E0; blue = 0x0000001F; alpha = 0x00000000;
                    rgbBitCount = 16;
                    break;
                case D3D.Format.A4R4G4B4:
                    red = 0x00000F00; green = 0x000000F0; blue = 0x0000000F; alpha = 0x0000F000;
                    rgbBitCount = 16;
                    break;
                default:
                    throw new AxiomException("Unknown D3D pixel format, this should not happen !!!");
            }
        }

        private D3D.TextureFilter GetBestFilterMethod() {
            // those MUST be initialized !!!
            Debug.Assert(device != null);
            Debug.Assert(texture != null);

            FilterCaps filterCaps;
            // Minification filter is used for mipmap generation
            // Pick the best one supported for this tex type
            switch (this.TextureType) {
                case TextureType.OneD: // Same as 2D
                case TextureType.TwoD:
                    filterCaps = devCaps.TextureFilterCaps;
                    break;
                case TextureType.ThreeD:
                    filterCaps = devCaps.VertexTextureFilterCaps;
                    break;
                case TextureType.CubeMap:
                    filterCaps = devCaps.CubeTextureFilterCaps;
                    break;
                default:
                    return TextureFilter.Point;
            }
            if (filterCaps.SupportsMinifyGaussianQuad)
                return TextureFilter.GaussianQuad;
            if (filterCaps.SupportsMinifyPyramidalQuad)
                return TextureFilter.PyramidalQuad;
            if (filterCaps.SupportsMinifyAnisotropic)
                return TextureFilter.Anisotropic;
            if (filterCaps.SupportsMinifyLinear)
                return TextureFilter.Linear;
            if (filterCaps.SupportsMinifyPoint)
                return TextureFilter.Point;
            return TextureFilter.Point;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        private void BlitImagesToCubeTex() {
            for (int i = 0; i < 6; i++) {
                // get a reference to the current cube surface for this iteration
                D3D.Surface dstSurface;

                // Now we need to copy the source surface (where our image is) to 
                // either the the temp. texture level 0 surface (for s/w mipmaps)
                // or the final texture (for h/w mipmaps)
                if (tempCubeTexture != null) {
                    dstSurface = tempCubeTexture.GetCubeMapSurface((CubeMapFace)i, 0);
                } else {
                    dstSurface = cubeTexture.GetCubeMapSurface((CubeMapFace)i, 0);
                }

                // copy the image data to a memory stream
                Stream stream = TextureManager.Instance.FindResourceData(cubeFaceNames[i]);

                // load the stream into the cubemap surface
                SurfaceLoader.FromStream(dstSurface, stream, Filter.Point, 0);

                dstSurface.Dispose();
                stream.Dispose();
            }

            // After doing all the faces, we generate mipmaps
            // For s/w mipmaps this involves an extra copying step
            // TODO: Find best filtering method for this hardware, currently hardcoded to Point
            if (tempCubeTexture != null) {
                TextureLoader.FilterTexture(tempCubeTexture, 0, Filter.Point);
                device.UpdateTexture(tempCubeTexture, cubeTexture);

                tempCubeTexture.Dispose();
            } else {
                cubeTexture.AutoGenerateFilterType = TextureFilter.Point;
                cubeTexture.GenerateMipSubLevels();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcUsage"></param>
        /// <param name="srcType"></param>
        /// <param name="srcFormat"></param>
        /// <returns></returns>
        private bool CanUseDynamicTextures(D3D.Usage srcUsage, D3D.ResourceType srcType, D3D.Format srcFormat) {
            Debug.Assert(device != null);

            // Check for dynamic texture support
            return D3D.Manager.CheckDeviceFormat(
                devParms.AdapterOrdinal,
                devParms.DeviceType,
                bbPixelFormat,
                srcUsage | D3D.Usage.Dynamic,
                srcType,
                srcFormat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcUsage"></param>
        /// <param name="srcType"></param>
        /// <param name="srcFormat"></param>
        /// <returns></returns>
        private bool CanAutoGenMipMaps(D3D.Usage srcUsage, D3D.ResourceType srcType, D3D.Format srcFormat) {
            Debug.Assert(device != null);

            // Hacky override - many (all?) cards seem to not be able to autogen on 
            // textures which are not a power of two
            // Can we even mipmap on 3D textures? Well
            if (((width & width - 1) != 0) || ((height & height - 1) != 0) || ((depth & depth - 1) != 0))
                return false;

            if (device.DeviceCaps.DriverCaps.CanAutoGenerateMipMap) {
                // make sure we can do it!
                return D3D.Manager.CheckDeviceFormat(
                    devParms.AdapterOrdinal,
                    devParms.DeviceType,
                    bbPixelFormat,
                    srcUsage | D3D.Usage.AutoGenerateMipMap,
                    srcType,
                    srcFormat);
            }

            return false;
        }

        public void CopyToTexture(Axiom.Core.Texture target) {
            // TODO: Check usage and format, need Usage property on Texture
            if (target.Usage != this.Usage ||
                target.TextureType != this.TextureType)
                throw new Exception("Source and destination textures must have the same usage and texture type");

            D3DTexture other = (D3DTexture)target;
            System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(0, 0, this.Width, this.Height);
            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, target.Width, target.Height);

            if (target.TextureType == TextureType.TwoD) {
                using (D3D.Surface srcSurface = normTexture.GetSurfaceLevel(0),
                                  dstSurface = other.NormalTexture.GetSurfaceLevel(0)) {
                    // copy this texture surface to the target
                    device.StretchRectangle(
                        srcSurface,
                        srcRect,
                        dstSurface,
                        destRect,
                        TextureFilter.None);
                }
            } else if (target.TextureType == TextureType.CubeMap) {
                // blit to 6 cube faces
                for (int face = 0; face < 6; face++) {
                    using (D3D.Surface srcSurface = cubeTexture.GetCubeMapSurface((CubeMapFace)face, 0),
                                       dstSurface = other.CubeTexture.GetCubeMapSurface((CubeMapFace)face, 0)) {
                        // copy this texture surface to the target
                        device.StretchRectangle(
                            srcSurface,
                            srcRect,
                            dstSurface,
                            destRect,
                            TextureFilter.None);
                    }
                }
            } else {
                // FIXME: Cube render targets
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void CreateInternalResourcesImpl() {
            // If srcWidth and srcHeight are zero, the requested extents have probably been set
            // through a method which set width and height. Take those values.
            if (srcWidth == 0 || srcHeight == 0) {
                srcWidth = width;
                srcHeight = height;
            }

            // Determine D3D pool to use
            // Use managed unless we're a render target or user has asked for 
            // a dynamic texture
            if ((usage & TextureUsage.RenderTarget) != 0 ||
                (usage & TextureUsage.Dynamic) != 0) {
                d3dPool = D3D.Pool.Default;
            } else {
                d3dPool = D3D.Pool.Managed;
            }

            switch (this.TextureType) {
                case TextureType.OneD:
                case TextureType.TwoD:
                    CreateNormalTexture();
                    break;
                case TextureType.CubeMap:
                    CreateCubeTexture();
                    break;
                case TextureType.ThreeD:
                    CreateVolumeTexture();
                    break;
                default:
                    FreeInternalResources();
                    throw new Exception("Unknown texture type!");
            }
        }

        public override System.Drawing.Graphics GetGraphics()
        {
            log.DebugFormat("Called GetGraphics");
            Debug.Assert(graphicsSurface == null);
            graphicsSurface = normTexture.GetSurfaceLevel(0);
            return graphicsSurface.GetGraphics();
        }
        public override void ReleaseGraphics()
        {
            Debug.Assert(graphicsSurface != null);
            graphicsSurface.ReleaseGraphics();
            graphicsSurface = null;
            log.DebugFormat("Called ReleaseGraphics");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        protected D3D.Format ChooseD3DFormat() {
            if (format == PixelFormat.Unknown)
                return bbPixelFormat;
            return D3DHelper.ConvertEnum(D3DHelper.GetClosestSupported(format));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="format"></param>
        private void SetSrcAttributes(int width, int height, int depth, PixelFormat format) {
            srcWidth = width;
            srcHeight = height;
            srcBpp = PixelUtil.GetNumElemBits(format);
            hasAlpha = PixelUtil.HasAlpha(format);
            // say to the world what we are doing
            switch (this.TextureType) {
                case TextureType.OneD:
                    if ((usage & TextureUsage.RenderTarget) != 0)
                        log.InfoFormat("D3D9 : Creating 1D RenderTarget, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    else
                        log.InfoFormat("D3D9 : Loading 1D Texture, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    break;
                case TextureType.TwoD:
                    if ((usage & TextureUsage.RenderTarget) != 0)
                        log.InfoFormat("D3D9 : Creating 2D RenderTarget, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    else
                        log.InfoFormat("D3D9 : Loading 2D Texture, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    break;
                case TextureType.ThreeD:
                    if ((usage & TextureUsage.RenderTarget) != 0)
                        log.InfoFormat("D3D9 : Creating 3D RenderTarget, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    else
                        log.InfoFormat("D3D9 : Loading 3D Texture, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    break;
                case TextureType.CubeMap:
                    if ((usage & TextureUsage.RenderTarget) != 0)
                        log.InfoFormat("D3D9 : Creating Cube map RenderTarget, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    else
                        log.InfoFormat("D3D9 : Loading Cube map Texture, image name : '{0}' with {1} mip map levels", name, numMipmaps);
                    break;
                default:
                    FreeInternalResources();
                    throw new AxiomException("Unknown texture type in D3DTexture::SetSrcAttributes");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="format"></param>
        private void SetFinalAttributes(int width, int height, int depth, PixelFormat format) {
            // set target texture attributes
            this.height = height;
            this.width = width;
            this.depth = depth;
            this.format = format;

            // Update size (the final size, not including temp space)
            // this is needed in Resource class
            int bytesPerPixel = finalBpp >> 3;
            if (!hasAlpha && finalBpp == 32) {
                bytesPerPixel--;
            }

            size = width * height * depth * bytesPerPixel * ((textureType == TextureType.CubeMap) ? 6 : 1);

            // say to the world what we are doing
            if (width != srcWidth ||
                height != srcHeight) {
                log.Info("D3D9 : ***** Dimensions altered by the render system");
                log.InfoFormat("D3D9 : ***** Source image dimensions : {0}x{1}", srcWidth, srcHeight);
                log.InfoFormat("D3D9 : ***** Texture dimensions : {0}x{1}", width, height);
            }

            // Create list of subsurfaces for getBuffer()
            CreateSurfaceList();
        }

        public override HardwarePixelBuffer GetBuffer(int face, int mipmap) {
            return GetSurfaceAtLevel(face, mipmap);
        }

        public bool ReleaseIfDefaultPool() {
            if (d3dPool == D3D.Pool.Default) {
                log.InfoFormat("Releasing D3D9 default pool texture: {0}", Name);
                // Just free any internal resources, don't call unload() here
                // because we want the un-touched resource to keep its unloaded status
                // after device reset.
                FreeInternalResources();
                log.InfoFormat("Released D3D9 default pool texture: {0}", Name);
                return true;
            }
            return false;
        }

        /****************************************************************************************/
        public bool RecreateIfDefaultPool(Device device) {
            bool ret = false;
            if (d3dPool == D3D.Pool.Default) {
                ret = true;
                object loader = null;
                log.InfoFormat("Recreating D3D9 default pool texture: {0}", Name);
                // We just want to create the texture resources if:
                // 1. This is a render texture, or
                // 2. This is a manual texture with no loader, or
                // 3. This was an unloaded regular texture (preserve unloaded state)
                // FIXME: Ogre supports associating a resource loader with a resource.
                //        This resource loader may be able to be used to recreate the resource
			    if ((isManual && (loader == null)) || 
                    ((usage & TextureUsage.RenderTarget) == TextureUsage.RenderTarget) ||
                    !isLoaded)
			    {
				    // just recreate any internal resources
				    CreateInternalResources();
			    }
			    // Otherwise, this is a regular loaded texture, or a manual texture with a loader
			    else
			    {
				    // The internal resources already freed, need unload/load here:
				    // 1. Make sure resource memory usage statistic correction.
				    // 2. Don't call unload() in releaseIfDefaultPool() because we want
				    //    the un-touched resource keep unload status after device reset.
				    Unload();
				    // if manual, we need to recreate internal resources since load() won't do that
				    if (isManual)
					    CreateInternalResources();
				    Load();
			    }
                log.InfoFormat("Recreated D3D9 default pool texture: {0}", Name);
            }

            return ret;

        }

        #endregion

    }
}
