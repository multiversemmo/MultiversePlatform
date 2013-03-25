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
using Axiom.Graphics;
using Axiom.Media;

namespace Axiom.Core {
    /// <summary>
    ///    Class for loading & managing textures.
    /// </summary>
    /// <remarks>
    ///    Texture manager serves as an abstract singleton for all API specific texture managers.
    ///		When a class inherits from this and is created, a instance of that class (i.e. GLTextureManager)
    ///		is stored in the global singleton instance of the TextureManager.  
    ///		Note: This will not take place until the RenderSystem is initialized and at least one RenderWindow
    ///		has been created.
    /// </remarks>
    public abstract class TextureManager : ResourceManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static TextureManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        /// <remarks>
        ///     Protected internal because this singleton will actually hold the instance of a subclass
        ///     created by a render system plugin.
        /// </remarks>
        protected internal TextureManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static TextureManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        #region Fields

        /// <summary>
        ///    Flag that indicates whether 32-bit texture are being used.
        /// </summary>
        protected bool is32Bit;

        /// <summary>
        ///    Default number of mipmaps to be used for loaded textures.
        /// </summary>
        protected int defaultNumMipMaps = 5;

        #endregion Fields

        #region Properties

        /// <summary>
        ///    Gets/Sets the default number of mipmaps to be used for loaded textures.
        /// </summary>
        public int DefaultNumMipMaps {
            get { 
                return defaultNumMipMaps; 
            }
            set { 
                defaultNumMipMaps = value; 
            }
        }

        public bool Is32Bit {
            get {
                return is32Bit;
            }
        }

        #endregion Properties
        
        #region Methods

        /// <summary>
        ///    Method for creating a new blank texture.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="texType"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="numMipMaps"></param>
        /// <param name="format"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public Texture CreateManual(string name, TextureType texType, int width, int height, int depth, int numMipmaps, PixelFormat format, TextureUsage usage) {
            Texture ret = (Texture)Create(name, true);
            ret.TextureType = texType;
            ret.Width = width;
            ret.Height = height;
            ret.Depth = depth;
            ret.NumMipMaps = (numMipmaps == -1) ? defaultNumMipMaps : numMipmaps;
            ret.Format = format;
            ret.Usage = usage;
            ret.Enable32Bit(is32Bit);
		    ret.CreateInternalResources();
		    return ret;
        }

        public Texture CreateManual(string name, TextureType type, int width, int height, int numMipmaps, PixelFormat format, TextureUsage usage) {
            return CreateManual(name, type, width, height, 1, numMipmaps, format, usage);
        }

        /// <summary>
        ///    Loads a texture with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Texture Load(string name) {
            return Load(name, TextureType.TwoD);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Texture Load(string name, TextureType type) {
            // load the texture by default with -1 mipmaps (uses default), gamma of 1, isAlpha of false
            return Load(name, type, -1, 1.0f, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="numMipMaps"></param>
        /// <param name="gamma"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public Texture Load(string name, TextureType type, int numMipMaps, float gamma, bool isAlpha) {
            // does this texture exist already?
            Texture texture = GetByName(name);

            if (texture == null) {
                // create a new texture
                texture = (Texture)Create(name);
                texture.TextureType = type;
                if (numMipMaps == -1)
                    texture.NumMipMaps = defaultNumMipMaps;
                else
                    texture.NumMipMaps = numMipMaps;
                
                // set bit depth and gamma
                texture.Gamma = gamma;
                if (isAlpha)
                    texture.Format = PixelFormat.A8;
                texture.Enable32Bit(is32Bit);
            }
            // The old code called the base class load method, but now we just call texture.Load()
            // base.Load(texture, 1);
            texture.Load();

            return texture;
        }

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="image"></param>
		/// <returns></returns>
        //public Texture LoadImage(string name, Image image) 
        //{
        //    return LoadImage(name, image, TextureType.TwoD, -1, 1.0f, 1);
        //}

        /// <summary>
        ///		Overloaded method.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="image"></param>
		/// <param name="texType"></param>
		/// <returns></returns>
        //public Texture LoadImage(string name, Image image, TextureType texType) 
        //{
        //    return LoadImage(name, image, texType, -1, 1.0f, 1);
        //}

        public Texture LoadImage(string name, Image image)
        {
            return LoadImage(name, image, TextureType.TwoD);
        }
        public Texture LoadImage(string name, Image image, TextureType texType)
        {
            return LoadImage(name, image, texType, -1, 1.0f, false);
        }

        /// <summary>
        ///		Loads a pre-existing image into the texture.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="image">the image to load, or null to return an empty texture</param>
        /// <param name="numMipMaps"></param>
        /// <param name="gamma"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public Texture LoadImage(string name, Image image, TextureType texType, int numMipMaps, float gamma, bool isAlpha) {
            // create a new texture
            Texture texture = (Texture)Create(name, true);

            texture.TextureType = texType;
            // set the number of mipmaps to use for this texture
            if (numMipMaps == -1)
                texture.NumMipMaps = defaultNumMipMaps;
            else
                texture.NumMipMaps = numMipMaps;

            // set bit depth and gamma
            texture.Gamma = gamma;
            if (isAlpha)
                texture.Format = PixelFormat.A8;
            texture.Enable32Bit(is32Bit);

            if (image != null)
                // load image data
                texture.LoadImage(image);
            
            return texture;
        }

        /// <summary>
        ///    Returns an instance of Texture that has the supplied name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new Texture GetByName(string name) {
            return (Texture)base.GetByName(name);
        }

        /// <summary>
        ///     Called when the engine is shutting down.    
        /// </summary>
        public override void Dispose() {
            base.Dispose();

            if (this == instance) {
                instance = null;
            }
        }

        public virtual PixelFormat GetNativeFormat(TextureType ttype, PixelFormat format,
                                                   TextureUsage usage) {
            // Just throw an error, for non-overriders
            throw new NotImplementedException();
        }

        public bool IsFormatSupported(TextureType ttype, PixelFormat format, TextureUsage usage) {
            return GetNativeFormat(ttype, format, usage) == format;
        }

        public bool IsEquivalentFormatSupported(TextureType ttype, PixelFormat format, TextureUsage usage) {
            PixelFormat supportedFormat = GetNativeFormat(ttype, format, usage);
            // Assume that same or greater number of bits means quality not degraded
            return PixelUtil.GetNumElemBits(supportedFormat) >= PixelUtil.GetNumElemBits(format);
        }

		public virtual int AvailableTextureMemory {
			get {
				throw new NotImplementedException();
			}
		}

        #endregion Methods
    }
}
