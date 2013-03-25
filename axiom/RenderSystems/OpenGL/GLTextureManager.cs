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
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.RenderSystems.OpenGL {
    /// <summary>
    /// Summary description for GLTextureManager.
    /// </summary>
    public class GLTextureManager : TextureManager {
        public GLTextureManager() {
			is32Bit = true;
        }
	
        public override Texture Create(string name, TextureType type) {
            GLTexture texture = new GLTexture(name, type);
            
            texture.Enable32Bit(is32Bit);

            return texture;
        }

        /// <summary>
        ///    Creates a GL texture manually.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="numMipMaps"></param>
        /// <param name="format"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public override Texture CreateManual(string name, TextureType type, int width, int height, int numMipMaps, Axiom.Media.PixelFormat format, TextureUsage usage) {
            GLTexture texture = new GLTexture(name, type, width, height, numMipMaps, format, usage);
            texture.Enable32Bit(is32Bit);
            return texture;
        }

        // TODO: Finish
        public override void UnloadAndDestroyAll() {
            base.UnloadAndDestroyAll ();
        }
    }
}
