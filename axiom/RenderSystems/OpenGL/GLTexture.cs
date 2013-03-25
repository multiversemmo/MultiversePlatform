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
using System.IO;
using Axiom.Collections;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Media;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL {
    /// <summary>
    ///    OpenGL specialization of texture handling.
    /// </summary>
    public class GLTexture : Texture {
        #region Member variable

        /// <summary>
        ///    OpenGL texture ID.
        /// </summary>
        private int glTextureID;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor, called from GLTextureManager.
        /// </summary>
        /// <param name="name"></param>
        public GLTexture(string name, TextureType type) {
            this.name = name;
            this.textureType = type;
            Enable32Bit(false);
        }

        /// <summary>
        ///    Constructor used when creating a manual texture.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="numMipMaps"></param>
        /// <param name="format"></param>
        /// <param name="usage"></param>
        public GLTexture(string name, TextureType type, int width, int height, int numMipMaps, PixelFormat format, TextureUsage usage) {
            this.name = name;
            this.textureType = type;
            this.srcWidth = width;
            this.srcHeight = height;
            this.width = srcWidth;
            this.height = srcHeight;
            this.depth = 1;
            this.numMipMaps = numMipMaps;
            this.usage = usage;
            this.format = format;
            this.srcBpp = Image.GetNumElemBits(format);

            Enable32Bit(false);
        }

        #endregion

        #region Properties

        /// <summary>
        ///		OpenGL texture ID.
        /// </summary>
        public int TextureID {
            get { 
                return glTextureID; 
            }
        }

        /// <summary>
        ///    OpenGL texture format enum value.
        /// </summary>
        public int GLFormat {
            get {
                switch(format) {
                    case PixelFormat.L8:
                        return Gl.GL_LUMINANCE;
                    case PixelFormat.R8G8B8:
                        return Gl.GL_RGB;
                    case PixelFormat.B8G8R8:
                        return Gl.GL_BGR;
                    case PixelFormat.B8G8R8A8:
                        return Gl.GL_BGRA;
                    case PixelFormat.A8R8G8B8:
                        return Gl.GL_RGBA;
                    case PixelFormat.DXT1:
                        return Gl.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT;
                    case PixelFormat.DXT3:
                        return Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
                    case PixelFormat.DXT5:
                        return Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
                }

                // make the compiler happy
                return 0;
            }
        }
        
        /// <summary>
        ///     Type of texture this represents, i.e. 2d, cube, etc.
        /// </summary>
        public int GLTextureType {
            get {
                switch(textureType) {
                    case TextureType.OneD:
                        return Gl.GL_TEXTURE_1D;
                    case TextureType.TwoD:
                        return Gl.GL_TEXTURE_2D;
                    case TextureType.ThreeD:
                        return Gl.GL_TEXTURE_3D;
                    case TextureType.CubeMap:
                        return Gl.GL_TEXTURE_CUBE_MAP;
                }

                return 0;
            }
        }
		
        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public override void LoadImage(Image image) {
            // create a list with one texture to pass it in to the common loading method
            ImageList images = new ImageList();
            images.Add(image);
            
            // load this image
            LoadImages(images);

            // clear the temp list of images
            images.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="images"></param>
        public void LoadImages(ImageList images) {
            bool useSoftwareMipMaps = true;

            if(isLoaded) {
                LogManager.Instance.Write("Unloading image '{0}'...", name);
                Unload();
            }

            // generate the texture
            Gl.glGenTextures(1, out glTextureID);

            // bind the texture
            Gl.glBindTexture(this.GLTextureType, glTextureID);

            // log a quick message
            LogManager.Instance.Write("GLTexture: Loading {0} with {1} mipmaps from an Image.", name, numMipMaps);

            if(numMipMaps > 0 && Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.HardwareMipMaps)) {
                Gl.glTexParameteri(this.GLTextureType, Gl.GL_GENERATE_MIPMAP, Gl.GL_TRUE);
                useSoftwareMipMaps = false;
            }

            for(int i = 0; i < images.Count; i++) {
                Image image = (Image)images[i];

                // get the images pixel format
                format = image.Format;
				
                srcBpp = Image.GetNumElemBits(format);
                hasAlpha = image.HasAlpha;

                // get dimensions
                srcWidth = image.Width;
                srcHeight = image.Height;

                // same destination dimensions for GL
                width = srcWidth;
                height = srcHeight;
                depth = image.Depth;

                // only oiverride global mipmap setting if the image itself has at least 1 already
                if(image.NumMipMaps > 0) {
                    numMipMaps = image.NumMipMaps;
                }

                // set the max number of mipmap levels
                Gl.glTexParameteri(this.GLTextureType, Gl.GL_TEXTURE_MAX_LEVEL, numMipMaps);

				// Rescale to Power of 2 (also applies gamma correction)
				byte[] data = RescaleNPower2(image);

                GenerateMipMaps(data, useSoftwareMipMaps, image.HasFlag(ImageFlags.Compressed), i);
            }

            // update the size
            int bytesPerPixel = finalBpp >> 3;
			
            if(!hasAlpha && finalBpp == 32)
                bytesPerPixel--;

            size = (long)(width * height * bytesPerPixel);

            isLoaded = true;
        }

        public override void Preload() {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Load() {
            if(isLoaded)
                return;

            if(usage == TextureUsage.RenderTarget) {
                CreateRenderTexture();
                isLoaded = true;
            }
            else {
                if(textureType == TextureType.TwoD 
                    || textureType == TextureType.OneD 
                    || textureType == TextureType.ThreeD) {

                    Image image = Image.FromFile(name);
                    
                    if(name.EndsWith(".dds") && image.HasFlag(ImageFlags.CubeMap)) {
                        ImageList images = new ImageList();

                        // all 6 images are in a single data buffer, so we will pull out all 6 pieces
                        int imageSize = image.Size / 6;

                        textureType = TextureType.CubeMap;

                        for(int i = 0, offset = 0; i < 6; i++, offset += imageSize) {
                            byte[] tempBuffer = new byte[imageSize];
                            Array.Copy(image.Data, offset, tempBuffer, 0, imageSize);

                            // load the raw data for this portion of the image data
                            Image cubeImage = Image.FromRawStream(
                                new MemoryStream(tempBuffer),
                                image.Width,
                                image.Height,
                                image.Format);

                            // add to the list of images to load
                            images.Add(cubeImage);
                        } // for

                        LoadImages(images);
                    }
                    else {
                        // if this is a dds volumetric texture, set the flag accordingly
                        if(name.EndsWith(".dds") && image.Depth > 1) {
                            textureType = TextureType.ThreeD;
                        }

                        // just load the 1 texture
                        LoadImage(image);
                    }
                }
                else if(textureType == TextureType.CubeMap) {
                    string baseName, ext;
                    ImageList images = new ImageList();
                    string[] postfixes = {"_rt", "_lf", "_up", "_dn", "_fr", "_bk"};

                    int pos = name.LastIndexOf(".");

                    baseName = name.Substring(0, pos);
                    ext = name.Substring(pos);

                    for(int i = 0; i < 6; i++) {
                        string fullName = baseName + postfixes[i] + ext;

                        // load the image
                        Image image = Image.FromFile(fullName);

                        images.Add(image);
                    } // for

                    // load all 6 images
                    LoadImages(images);
                } // else
                else {
                    throw new NotImplementedException("Unknown texture type.");
                }
            } // if
        }

        /// <summary>
        ///    Deletes the texture memory.
        /// </summary>
        public override void Unload() {
            if(isLoaded) {
                Gl.glDeleteTextures(1, ref glTextureID);
                isLoaded = false;
            }
        }

        protected void GenerateMipMaps(byte[] data, bool useSoftware, bool isCompressed, int faceNum) {
            // use regular type, unless cubemap, then specify which face of the cubemap we
            // are dealing with here
            int type = (textureType == TextureType.CubeMap) ? Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X + faceNum : this.GLTextureType;

            if(useSoftware && numMipMaps > 0) {
                if(textureType == TextureType.OneD) {
                    Glu.gluBuild1DMipmaps(
                        type, 
                        hasAlpha ? Gl.GL_RGBA8 : Gl.GL_RGB8, 
                        width, 
                        this.GLFormat, 
                        Gl.GL_UNSIGNED_BYTE, 
                        data);
                }
                else if(textureType == TextureType.ThreeD) {
                    // TODO: Tao needs glTexImage3D
                    Gl.glTexImage3DEXT(
                        type, 
                        0,
                        hasAlpha ? Gl.GL_RGBA8 : Gl.GL_RGB8,
                        srcWidth, srcHeight, depth, 0, this.GLFormat,
                        Gl.GL_UNSIGNED_BYTE,
                        data);
                }
                else {
                    // build the mipmaps
                    Glu.gluBuild2DMipmaps(
                        type,
                        hasAlpha ? Gl.GL_RGBA8 : Gl.GL_RGB8, 
                        width, height,
                        this.GLFormat, 
                        Gl.GL_UNSIGNED_BYTE, 
                        data);
                }
            }
            else {
                if(textureType == TextureType.OneD) {
                    Gl.glTexImage1D(
                        type, 
                        0, 
                        hasAlpha ? Gl.GL_RGBA8 : Gl.GL_RGB8, 
                        width, 
                        0, 
                        this.GLFormat, 
                        Gl.GL_UNSIGNED_BYTE, 
                        data);
                }
                else if(textureType == TextureType.ThreeD) {
                    // TODO: Tao needs glTexImage3D
                    Gl.glTexImage3DEXT(
                        type, 
                        0,
                        hasAlpha ? Gl.GL_RGBA8 : Gl.GL_RGB8,
                        srcWidth, srcHeight, depth, 0, this.GLFormat,
                        Gl.GL_UNSIGNED_BYTE,
                        data);
                }
                else {
                    if(isCompressed && Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.TextureCompressionDXT)) {
                        int blockSize = (format == PixelFormat.DXT1) ? 8 : 16;
                        int size = ((width + 3) / 4)*((height + 3) / 4) * blockSize;

                        // load compressed image data
                        Gl.glCompressedTexImage2DARB(
                            type,
                            0,
                            this.GLFormat,
                            srcWidth,
                            srcHeight,
                            0,
                            size,
                            data);
                    }
                    else {
                        Gl.glTexImage2D(
                            type, 
                            0, 
                            hasAlpha ? Gl.GL_RGBA8 : Gl.GL_RGB8, 
                            width, height, 0, 
                            this.GLFormat, Gl.GL_UNSIGNED_BYTE, 
                            data);
                    }
                }
            }
        }

        /// <summary>
        ///    Used to generate a texture capable of serving as a rendering target.
        /// </summary>
        private void CreateRenderTexture() {
            if(this.TextureType != TextureType.TwoD) {
                throw new NotImplementedException("Can only create render textures for 2D textures.");
            }

            // create and bind the texture
            Gl.glGenTextures(1, out glTextureID);
            Gl.glBindTexture(this.GLTextureType, glTextureID);

            // generate an image without data by default to use for rendering to
            // Note: null is casted to byte[] in order to remove compiler confusion over ambiguous overloads
            Gl.glTexImage2D(
                this.GLTextureType, 
                0, 
                this.GLFormat, 
                width, 
                height, 
                0, 
                this.GLFormat, 
                Gl.GL_UNSIGNED_BYTE, 
                (byte[])null);

            // This needs to be set otherwise the texture doesn't get rendered
            Gl.glTexParameteri(this.GLTextureType, Gl.GL_TEXTURE_MAX_LEVEL, numMipMaps);
        }

		private byte[] RescaleNPower2(Image src) {
			// Scale image to n^2 dimensions
			int newWidth = (1 << MostSignificantBitSet(srcWidth));

			if (newWidth != srcWidth) {
				newWidth <<= 1;
			}

			int newHeight = (1 << MostSignificantBitSet(srcHeight));
			if (newHeight != srcHeight) {
				newHeight <<= 1;
			}

			byte[] tempData;

			if(newWidth != srcWidth || newHeight != srcHeight) {
				int newImageSize = newWidth * newHeight * (hasAlpha ? 4 : 3);

				tempData = new byte[newImageSize];

				if(Glu.gluScaleImage(this.GLFormat, srcWidth, srcHeight,
					Gl.GL_UNSIGNED_BYTE, src.Data, newWidth, newHeight, 
					Gl.GL_UNSIGNED_BYTE, tempData) != 0) {

					throw new AxiomException("Error while rescaling image!");
				}

				Image.ApplyGamma(tempData, gamma, newImageSize, srcBpp);

				srcWidth = width = newWidth; 
				srcHeight = height = newHeight;
			}
			else {
				tempData = new byte[src.Size];
				Array.Copy(src.Data, tempData, src.Size);
				Image.ApplyGamma(tempData, gamma, src.Size, srcBpp);
			}

			return tempData;
		}

		/// <summary>
		///		Helper method for getting the next highest power of 2 value from the specified value.
		/// </summary>
		/// <remarks>Example: Input: 3 Result: 4, Input: 96 Output: 128</remarks>
		/// <param name="val">Integer value.</param>
		/// <returns></returns>
		private int MostSignificantBitSet(int val) {
			int result = 0;

			while(val != 0) {
				result++;
				val >>= 1;
			}

			return result - 1;
		}

        #endregion
    }
}
