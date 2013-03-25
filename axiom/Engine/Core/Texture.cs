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
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Axiom.Graphics;
using Axiom.Media;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Axiom.Core {
    /// <summary>
    ///		Abstract class representing a Texture resource.
    /// </summary>
    /// <remarks>
    ///		The actual concrete subclass which will exist for a texture
    ///		is dependent on the rendering system in use (Direct3D, OpenGL etc).
    ///		This class represents the commonalities, and is the one 'used'
    ///		by programmers even though the real implementation could be
    ///		different in reality. Texture objects are created through
    ///		the 'Create' method of the TextureManager concrete subclass.
    /// </remarks>
    public abstract class Texture : Resource {
        #region Member variables
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Texture));

        /// <summary>Width of this texture.</summary>
        protected int width;
        /// <summary>Height of this texture.</summary>
        protected int height;
        /// <summary>Depth of this texture.</summary>
        protected int depth;
        /// <summary>Bits per pixel in this texture.</summary>
        protected int finalBpp;
        /// <summary>Original source width if this texture had been modified.</summary>
        protected int srcWidth;
        /// <summary>Original source height if this texture had been modified.</summary>
        protected int srcHeight;
        /// <summary>Original source depth if this texture had been modified.</summary>
        protected int srcDepth;
        /// <summary>Original source bits per pixel if this texture had been modified.</summary>
        protected int srcBpp;
        /// <summary>Does this texture have an alpha component?</summary>
        protected bool hasAlpha;
        /// <summary>Pixel format of this texture.</summary>
        protected PixelFormat format;
        /// <summary>Specifies how this texture will be used.</summary>
        protected TextureUsage usage = TextureUsage.Default;
        /// <summary>Type of texture, i.e. 1D, 2D, Cube, Volume.</summary>
        protected TextureType textureType;
        /// <summary>Number of mipmaps present in this texture.</summary>
        protected int numMipmaps;
        /// <summary>Number of mipmaps requested for this texture.</summary>
        protected int numRequestedMipmaps;
        /// <summary>Are the mipmaps generated in hardware?</summary>
        protected bool mipmapsHardwareGenerated = false;
        /// <summary>Gamma setting for this texture.</summary>
        protected float gamma;
        /// <summary>Have the internal resources been created?</summary>
        protected bool internalResourcesCreated = false;

        #endregion

        #region Constructors

        #endregion

        #region Methods

        /// <summary>
        ///    Specifies whether this texture should use 32 bit color or not.
        /// </summary>
        /// <param name="enable">true if this should be treated as 32-bit, false if it should be 16-bit.</param>
        public void Enable32Bit(bool enable) {
            finalBpp = (enable == true) ? 32 : 16;
        }


        /// <summary>
        ///    Loads data from an Image directly into this texture.
        /// </summary>
        /// <param name="image"></param>
        public abstract void LoadImage(Image image);

        /// <summary>
        ///    Loads raw image data from the stream into this texture.
        /// </summary>
        /// <param name="data">The raw, decoded image data.</param>
        /// <param name="width">Width of the texture data.</param>
        /// <param name="height">Height of the texture data.</param>
        /// <param name="format">Format of the supplied image data.</param>
        public void LoadRawData(Stream data, int width, int height, PixelFormat format) {
            // load the raw data
            Image image = Image.FromRawStream(data, width, height, format);

            // call the polymorphic LoadImage implementation
            LoadImage(image);
        }

        public void CreateInternalResources() {
            if (!internalResourcesCreated) {
                CreateInternalResourcesImpl();
                internalResourcesCreated = true;
            }
        }

        public void FreeInternalResources() {
            if (internalResourcesCreated) {
                FreeInternalResourcesImpl();
                internalResourcesCreated = false;
            }
        }

        protected override void UnloadImpl() {
            FreeInternalResources();
        }

        protected abstract void CreateInternalResourcesImpl();
        protected abstract void FreeInternalResourcesImpl();
        public abstract System.Drawing.Graphics GetGraphics();
        public abstract void ReleaseGraphics();

        protected virtual void LoadImages(List<Image> images) {
            Debug.Assert(images.Count >= 1);
            if (isLoaded) {
                log.InfoFormat("Unloading image: {0}", name);
                Unload();
            }
            srcWidth = width = images[0].Width;
            srcHeight = height = images[0].Height;
            srcDepth = depth = images[0].Depth;
            if (hasAlpha && images[0].Format == PixelFormat.L8) {
                format = PixelFormat.A8;
                srcBpp = 8;
            } else {
                this.Format = images[0].Format;
            }
            if (finalBpp == 16) {
                switch (format) {
                    case PixelFormat.R8G8B8:
                    case PixelFormat.X8R8G8B8:
                        format = PixelFormat.R5G6B5;
                        break;
                    case PixelFormat.B8G8R8:
                    case PixelFormat.X8B8G8R8:
                        format = PixelFormat.B5G6R5;
                        break;
                    case PixelFormat.A8R8G8B8:
                    case PixelFormat.R8G8B8A8:
                    case PixelFormat.A8B8G8R8:
                    case PixelFormat.B8G8R8A8:
                        format = PixelFormat.A4R4G4B4;
                        break;
                    default:
                        // use the original format
                        break;
                }
            }

            // The custom mipmaps in the image have priority over everything
            int imageMips = images[0].NumMipMaps;
            if (imageMips > 0) {
                numMipmaps = imageMips;
                usage &= ~TextureUsage.AutoMipMap;
            }

            // Create the texture
            CreateInternalResources();

            // Check if we're loading one image with multiple faces
            // or a vector of images representing the faces
            int faces;
		    bool multiImage; // Load from multiple images?
		    if (images.Count > 1) {
		    	faces = images.Count;
			    multiImage = true;
		    } else {
		    	faces = images[0].NumFaces;
			    multiImage = false;
            }
		
    		// Check wether number of faces in images exceeds number of faces
	    	// in this texture. If so, clamp it.
		    if (faces > this.NumFaces)
			    faces = this.NumFaces;
		
		    // Say what we're doing
            log.InfoFormat("Texture: {0}: Loading {1} faces({2},{3}x{4}x{5})",
                name, faces, PixelUtil.GetFormatName(images[0].Format), 
                images[0].Width, images[0].Height, images[0].Depth);
#if NOT // crazy ogre logging
		if (!(mMipmapsHardwareGenerated && mNumMipmaps == 0))
			str << mNumMipmaps;
		if(mUsage & TU_AUTOMIPMAP)
		{
			if (mMipmapsHardwareGenerated)
				str << " hardware";

			str << " generated mipmaps";
		}
		else
		{
			str << " custom mipmaps";
		}
 		if(multiImage)
			str << " from multiple Images.";
		else
			str << " from Image.";
		// Scoped
		{
			// Print data about first destination surface
			HardwarePixelBufferSharedPtr buf = getBuffer(0, 0); 
			str << " Internal format is " << PixelUtil::getFormatName(buf->getFormat()) << 
			"," << buf->getWidth() << "x" << buf->getHeight() << "x" << buf->getDepth() << ".";
		}
		LogManager::getSingleton().logMessage( 
				LML_NORMAL, str.str());
#endif
		    // Main loading loop
            // imageMips == 0 if the image has no custom mipmaps, otherwise contains the number of custom mips
            for (int mip = 0; mip <= imageMips; ++mip) {
                for (int i = 0; i < faces; ++i) {
                    PixelBox src;
                    if (multiImage) {
                        // Load from multiple images
                        src = images[i].GetPixelBox(0, mip);
                    } else {
                        // Load from faces of images[0]
                        src = images[0].GetPixelBox(i, mip);

                        if (hasAlpha && src.Format == PixelFormat.L8)
                            src.Format = PixelFormat.A8;
                    }
    
                    if (gamma != 1.0f) {
                        // Apply gamma correction
                        // Do not overwrite original image but do gamma correction in temporary buffer
                        IntPtr buffer = Marshal.AllocHGlobal(PixelUtil.GetMemorySize(src.Width, src.Height, src.Depth, src.Format));
                        try {
                            PixelBox corrected = new PixelBox(src.Width, src.Height, src.Depth, src.Format, buffer);
                            PixelUtil.BulkPixelConversion(src, corrected);

                            Image.ApplyGamma(corrected.Data, gamma, corrected.ConsecutiveSize, PixelUtil.GetNumElemBits(src.Format));

                            // Destination: entire texture. BlitFromMemory does the scaling to
                            // a power of two for us when needed
                            GetBuffer(i, mip).BlitFromMemory(corrected);
                        } finally {
                            Marshal.FreeHGlobal(buffer);
                        }
                    } else {
                        // Destination: entire texture. BlitFromMemory does the scaling to
                        // a power of two for us when needed
                        GetBuffer(i, mip).BlitFromMemory(src);
                    }
                }
            }
            // Update size (the final size, not including temp space)
            size = this.NumFaces * PixelUtil.GetMemorySize(width, height, depth, format);

            isLoaded = true;
        }

        /// <summary>
        ///    Return hardware pixel buffer for a surface. This buffer can then
        ///    be used to copy data from and to a particular level of the texture.
        /// </summary>
        /// <param name="face">
        ///    Face number, in case of a cubemap texture. Must be 0
        ///    for other types of textures.
        ///    For cubemaps, this is one of 
        ///    +X (0), -X (1), +Y (2), -Y (3), +Z (4), -Z (5)
        /// </param>
        /// <param name="mipmap">
        ///    Mipmap level. This goes from 0 for the first, largest
        ///    mipmap level to getNumMipmaps()-1 for the smallest.
        /// </param>
        /// <remarks>
        ///    The buffer is invalidated when the resource is unloaded or destroyed.
        ///    Do not use it after the lifetime of the containing texture.
        /// </remarks>
        /// <returns>A shared pointer to a hardware pixel buffer</returns>
        public abstract HardwarePixelBuffer GetBuffer(int face, int mipmap);

        public HardwarePixelBuffer GetBuffer(int face) {
            return GetBuffer(face, 0);
        }
        public HardwarePixelBuffer GetBuffer() {
            return GetBuffer(0, 0);
        }

        #endregion

        #region Properties

        /// <summary>
        ///    Gets the width (in pixels) of this texture.
        /// </summary>
        public int SrcWidth {
            get {
                return srcWidth;
            }
        }

        public int SrcHeight {
            get {
                return srcHeight;
            }
        }

        public int SrcDepth {
            get {
                return srcDepth;
            }
        }

        public int SrcBpp {
            get {
                return srcBpp;
            }
        }
       
        /// <summary>
        ///    Gets the width (in pixels) of this texture.
        /// </summary>
        public int Width {
            get {
                return width;
            }
            set {
                width = srcWidth = value;
            }
        }

        /// <summary>
        ///    Gets the height (in pixels) of this texture.
        /// </summary>
        public int Height {
            get {
                return height;
            }
            set {
                height = srcHeight = value;
            }
        }

        /// <summary>
        ///    Gets the depth of this texture (for volume textures).
        /// </summary>
        public int Depth {
            get {
                return depth;
            }
            set {
                depth = srcDepth = value;
            }
        }

        /// <summary>
        ///    Gets the bits per pixel found within this texture data.
        /// </summary>
        public int Bpp {
            get {
                return finalBpp;
            }
        }

        /// <summary>
        ///    Gets whether or not the PixelFormat of this texture contains an alpha component.
        /// </summary>
        public bool HasAlpha {
            get {
                return hasAlpha;
            }
        }

        /// <summary>
        ///    Gets/Sets the gamma adjustment factor for this texture.
        /// </summary>
        /// <remarks>
        ///    Must be called before any variation of Load.
        /// </remarks>
        public float Gamma {
            get {
                return gamma;
            }
            set {
                gamma = value;
            }
        }

        /// <summary>
        ///    Gets the PixelFormat of this texture.
        /// </summary>
        public PixelFormat Format {
            get {
                return format;
            }
            set {
                // This can only be called before Load()
                format = value;
                srcBpp = PixelUtil.GetNumElemBits(format);
                hasAlpha = PixelUtil.HasAlpha(format);
            }
        }

        /// <summary>
        ///    Number of mipmaps present in this texture.
        /// </summary>
        public int NumMipMaps {
            get { 
                return numMipmaps; 
            }
            set { 
                numMipmaps = value;
                numRequestedMipmaps = value;
            }
        }

        /// <summary>
        ///   Number of faces in this texture
        /// </summary>
        public int NumFaces {
            get {
                return (textureType == TextureType.CubeMap) ? 6 : 1;
            }
        }

        /// <summary>
        ///    Type of texture, i.e. 2d, 3d, cubemap.
        /// </summary>
        public TextureType TextureType {
            get {
                return textureType;
            }
            set {
                // This should not be called after Load()
                textureType = value;
            }
        }

        /// <summary>
        ///     Gets the intended usage of this texture, whether for standard usage
        ///     or as a render target.
        /// </summary>
        public TextureUsage Usage {
            get {
                return usage;
            }
            set {
                usage = value;
            }
        }

        #endregion Properties

        #region Implementation of Resource

        /// <summary>
        ///		Implementation of IDisposable to determine how resources are disposed of.
        /// </summary>
        public override void Dispose() {
            // call polymorphic Unload method
            Unload();
        }

        #endregion
    }
}
