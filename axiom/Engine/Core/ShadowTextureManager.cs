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
using Axiom.Graphics;
using Axiom.Media;

namespace Axiom.Core {

    /// <summary>
    ///     Structure containing the configuration for one shadow texture.
    /// </summary>
    public class ShadowTextureConfig {
        public int width;
        public int height;
        public PixelFormat format;

        public ShadowTextureConfig() {
            width = 512;
            height = 512;
            format = PixelFormat.A8R8G8B8;
        }
        
        public bool Equal(ShadowTextureConfig other) {
            return width == other.width && height == other.height && format == other.format;
        }
        
    }

    /// <summary>
    ///     Class to manage the available shadow textures which may be shared between
    ///     many SceneManager instances if formats agree.
    /// </summary>
    /// <remarks>
    ///     The management of the list of shadow textures has been separated out into
    ///     a dedicated class to enable the clean management of shadow textures
    ///     across many scene manager instances. Where multiple scene managers are
    ///     used with shadow textures, the configuration of those shadows may or may
    ///     not be consistent - if it is, it is good to centrally manage the textures
    ///     so that creation and destruction responsibility is clear.
    /// </remarks>
    public class ShadowTextureManager {
        #region Fields

        /// <summary>
        ///     A list of textures available for shadow use.</summary>
        /// </summary>
        protected List<Texture> textureList = new List<Texture>();
        protected List<Texture> nullTextureList = new List<Texture>();
        protected int count;
        protected static ShadowTextureManager instance = null;
        
        #endregion Fields

        #region Constructor
        
        public ShadowTextureManager() {
            count = 0;
        }

        #endregion Constructor
        
        #region Public Methods

        /// <summary>
        ///     Populate an incoming list with shadow texture references as requested
        ///     in the configuration list.
        /// </summary>
        public void GetShadowTextures(List<ShadowTextureConfig> configList, List<Texture> listToPopulate) {
            listToPopulate.Clear();

            List<Texture> usedTextures = new List<Texture>();

            foreach (ShadowTextureConfig config in configList) {
                bool found = false;
                foreach (Texture tex in textureList) {
                    // Skip if already used this one
                    if (usedTextures.Contains(tex))
                        continue;

                    if (config.width == tex.Width && config.height == tex.Height && config.format == tex.Format) {
                        // Ok, a match
                        listToPopulate.Add(tex);
                        usedTextures.Add(tex);
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // Create a new texture
                    string baseName = "Axiom/ShadowTexture";
                    string targName = baseName + count++;
                    Texture shadowTex = TextureManager.Instance.CreateManual(
                        targName, 
                        TextureType.TwoD, config.width, config.height, 1, 0, config.format, 
                        TextureUsage.RenderTarget);
                    // Ensure texture loaded
                    shadowTex.Load();
                    listToPopulate.Add(shadowTex);
                    usedTextures.Add(shadowTex);
                    textureList.Add(shadowTex);
                }
            }
            
        }

        /// <summary>
        ///     Get an appropriately defined 'null' texture, ie one which will always
        ///     result in no shadows.
        /// </summary>
        public Texture GetNullShadowTexture(PixelFormat format) {
            foreach (Texture tex in nullTextureList) {
                if (format == tex.Format)
                    // Ok, a match
                    return tex;
            }

            // not found, create a new one
            // A 1x1 texture of the correct format, not a render target
            string baseName = "Axiom/ShadowTextureNull";
            string targName = baseName + count++;
            Texture shadowTex = TextureManager.Instance.CreateManual(
                targName, 
                TextureType.TwoD, 1, 1, 1, 0, format, TextureUsage.Default);
            nullTextureList.Add(shadowTex);

            // Populate the texture based on format
            int byteCount = PixelUtil.GetNumElemBytes(format);
            unsafe {
                byte [] bytes = new byte[byteCount];
                for (int i=0; i<byteCount; i++)
                    bytes[i] = 0xff;
                fixed (byte* fixedBytes = &bytes[0]) {
                    PixelBox bytesPixelBox = new PixelBox(1, 1, 1, format, new IntPtr(fixedBytes));
                    shadowTex.GetBuffer().BlitFromMemory(bytesPixelBox);
                }
            }
            return shadowTex;
        }
        
        /// <summary>
        ///     Remove any shadow textures that are no longer being referenced.
        /// </summary>
        /// <remarks>
        ///     This should be called fairly regularly since references may take a 
        ///     little while to disappear in some cases (if referenced by materials)
        /// </remarks>
        public void ClearUnused() {
//         Code commented out til we figure out when and if we want to
//         deal with reference counting textures.
//             for (ShadowTextureList::iterator i = mTextureList.begin(); i != mTextureList.end(); )
//             {
//                 // Unreferenced if only this reference and the resource system
//                 // Any cached shadow textures should be re-bound each frame dropping
//                 // any old references
//                 if ((*i).useCount() == ResourceGroupManager::RESOURCE_SYSTEM_NUM_REFERENCE_COUNTS + 1)
//                 {
//                     TextureManager::getSingleton().remove((*i)->getHandle());
//                     i = mTextureList.erase(i);
//                 }
//                 else
//                 {
//                     ++i;
//                 }
//             }
//             for (ShadowTextureList::iterator i = mNullTextureList.begin(); i != mNullTextureList.end(); )
//             {
//                 // Unreferenced if only this reference and the resource system
//                 // Any cached shadow textures should be re-bound each frame dropping
//                 // any old references
//                 if ((*i).useCount() == ResourceGroupManager::RESOURCE_SYSTEM_NUM_REFERENCE_COUNTS + 1)
//                 {
//                     TextureManager::getSingleton().remove((*i)->getHandle());
//                     i = mNullTextureList.erase(i);
//                 }
//                 else
//                 {
//                     ++i;
//                 }
//             }
        }

        /// <summary>
        ///     Dereference all the shadow textures kept in this class and remove them
        ///     from TextureManager; note that it is up to the SceneManagers to clear 
        ///     their local references.
        /// </summary>
        public void ClearAll() {
            foreach (Texture t in textureList)
            {
                TextureManager.Instance.Remove(t.Name);
            }
            textureList.Clear();
            count = 0;
        }

        /// <summary>
        ///     Retrieve the instance, creating it if necessary
        /// </summary>
        public static ShadowTextureManager Instance {
            get {
                if (instance == null)
                    instance = new ShadowTextureManager();
                return instance;
            }
        }

        #endregion Public Methods

    }

}


