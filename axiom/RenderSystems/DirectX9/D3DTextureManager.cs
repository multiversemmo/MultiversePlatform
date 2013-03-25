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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
    /// <summary>
    ///     Summary description for D3DTextureManager.
    /// </summary>
    public class D3DTextureManager : TextureManager {
        /// <summary>Reference to the D3D device.</summary>
        private D3D.Device device;
        public D3D.Device Device { get { return device; } }

        public D3DTextureManager(D3D.Device device) {
            this.device = device;

			is32Bit = true;
        }

        public override Axiom.Core.Resource Create(string name, bool isManual) {
            Axiom.Core.Resource rv = new D3DTexture(name, isManual, device);
            Add(rv);
            return rv;
        }

        //public override Axiom.Core.Texture Create(string name, TextureType type) {
        //    D3DTexture texture = new D3DTexture(name, device, TextureUsage.Default, type);

        //    // Handle 32-bit texture settings
        //    texture.Enable32Bit(is32Bit);

        //    return texture;
        //}

        /// <summary>
        ///    Used to create a blank D3D texture.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="numMipMaps"></param>
        /// <param name="format"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        //public override Axiom.Core.Texture CreateManual(string name, TextureType type, int width, int height, int numMipMaps, Axiom.Media.PixelFormat format, TextureUsage usage) {
        //    D3DTexture texture = new D3DTexture(name, device, type, width, height, numMipMaps, format, usage);
        //    texture.Enable32Bit(is32Bit);
        //    return texture;
        //}
        
        // This ends up just discarding the format passed in; the C# methods don't let you supply
        // a "recommended" format.  Ah well.
        public override Axiom.Media.PixelFormat GetNativeFormat(TextureType ttype, Axiom.Media.PixelFormat format, TextureUsage usage) {
            // Basic filtering
            D3D.Format d3dPF = D3DHelper.ConvertEnum(D3DHelper.GetClosestSupported(format));

            // Calculate usage
            D3D.Usage d3dusage = 0;
            D3D.Pool pool = D3D.Pool.Managed;
            if ((usage & TextureUsage.RenderTarget) != 0) {
                d3dusage |= D3D.Usage.RenderTarget;
                pool = D3D.Pool.Default;
            }
            if ((usage & TextureUsage.Dynamic) != 0) {
                d3dusage |= D3D.Usage.Dynamic;
                pool = D3D.Pool.Default;
            }

            // Use D3DX to adjust pixel format
            switch (ttype) {
                case TextureType.OneD:
                case TextureType.TwoD:
                    TextureRequirements tReqs;
                    TextureLoader.CheckTextureRequirements(device, d3dusage, pool, out tReqs);
                    d3dPF = tReqs.Format;
                    break;
                case TextureType.ThreeD:
                    VolumeTextureRequirements volReqs;
                    TextureLoader.CheckVolumeTextureRequirements(device, pool, out volReqs);
                    d3dPF = volReqs.Format;
                    break;
                case TextureType.CubeMap:
                    CubeTextureRequirements cubeReqs;
                    TextureLoader.CheckCubeTextureRequirements(device, d3dusage, pool, out cubeReqs);
                    d3dPF = cubeReqs.Format;
                    break;
            };
            return D3DHelper.ConvertEnum(d3dPF);
        }


        public void ReleaseDefaultPoolResources()
	    {
		    int count = 0;
            foreach (D3DTexture tex in resourceList.Values) {
    			if (tex.ReleaseIfDefaultPool())
				    count++;
            }
            LogManager.Instance.Write("D3DTextureManager released: {0} unmanaged textures", count);
	    }

	    public void RecreateDefaultPoolResources()
	    {
		    int count = 0;
            foreach (D3DTexture tex in resourceList.Values) {
    			if (tex.RecreateIfDefaultPool(device))
				    count++;
            }
            LogManager.Instance.Write("D3DTextureManager recreated: {0} unmanaged textures", count);
    	}


		public override int AvailableTextureMemory {
			get {
				return device.AvailableTextureMemory;
			}
		}
    }
}
