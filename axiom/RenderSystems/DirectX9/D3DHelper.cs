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
using Axiom.Core;
using Axiom.Graphics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
    /// <summary>
    ///		Helper class for Direct3D that includes conversion functions and things that are
    ///		specific to D3D.
    /// </summary>
    public class D3DHelper {
        public D3DHelper() {
        }

        /// <summary>
        ///		Enumerates driver information and their supported display modes.
        /// </summary>
        public static Driver GetDriverInfo() {
            ArrayList driverList = new ArrayList();

            // get the information for the default adapter (not checking secondaries)
            AdapterInformation adapterInfo = D3D.Manager.Adapters[0];

            Driver driver = new Driver(adapterInfo);

            int lastWidth = 0, lastHeight = 0;
            D3D.Format lastFormat = 0;

            foreach(DisplayMode mode in adapterInfo.SupportedDisplayModes) {
                // filter out lower resolutions, and make sure this isnt a dupe (ignore variations on refresh rate)
                if((mode.Width >= 640 && mode.Height >= 480) &&
                    ((mode.Width != lastWidth) || mode.Height != lastHeight || mode.Format != lastFormat)) {
                    // add the video mode to the list
                    driver.VideoModes.Add(new VideoMode(mode));	

                    // save current mode settings for comparison on the next iteraion
                    lastWidth = mode.Width;
                    lastHeight = mode.Height;
                    lastFormat = mode.Format;
                }
            }

            return driver;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <param name="caps"></param>
        /// <param name="texType"></param>
        /// <returns></returns>
        public static D3D.TextureFilter ConvertEnum(FilterType type, FilterOptions options, D3D.Caps devCaps, D3DTexType texType) {
            // setting a default val here to keep compiler from complaining about using unassigned value types
            D3D.FilterCaps filterCaps = devCaps.TextureFilterCaps;

            switch(texType) {
                case D3DTexType.Normal:
                    filterCaps = devCaps.TextureFilterCaps;
                    break;
                case D3DTexType.Cube:
                    filterCaps = devCaps.CubeTextureFilterCaps;
                    break;
                case D3DTexType.Volume:
                    filterCaps = devCaps.VolumeTextureFilterCaps;
                    break;
            }

            switch(type) {
                case FilterType.Min: {
                    switch(options) {
                        case FilterOptions.Anisotropic:
                            if(filterCaps.SupportsMinifyAnisotropic) {
                                return D3D.TextureFilter.Anisotropic;
                            }
                            else {
                                return D3D.TextureFilter.Linear;
                            }

                        case FilterOptions.Linear:
                            if(filterCaps.SupportsMinifyLinear) {
                                return D3D.TextureFilter.Linear;
                            }
                            else {
                                return D3D.TextureFilter.Point;
                            }

                        case FilterOptions.Point:
                        case FilterOptions.None:
                            return D3D.TextureFilter.Point;
                    }
                    break;
                }
                case FilterType.Mag: {
                    switch(options) {
                        case FilterOptions.Anisotropic:
                            if(filterCaps.SupportsMagnifyAnisotropic) {
                                return D3D.TextureFilter.Anisotropic;
                            }
                            else {
                                return D3D.TextureFilter.Linear;
                            }

                        case FilterOptions.Linear:
                            if(filterCaps.SupportsMagnifyLinear) {
                                return D3D.TextureFilter.Linear;
                            }
                            else {
                                return D3D.TextureFilter.Point;
                            }

                        case FilterOptions.Point:
                        case FilterOptions.None:
                            return D3D.TextureFilter.Point;
                    }
                    break;
                }
                case FilterType.Mip: {
                    switch(options) {
                        case FilterOptions.Anisotropic:
                        case FilterOptions.Linear:
                            if(filterCaps.SupportsMipMapLinear) {
                                return D3D.TextureFilter.Linear;
                            }
                            else {
                                return D3D.TextureFilter.Point;
                            }

                        case FilterOptions.Point:
                            if(filterCaps.SupportsMipMapPoint) {
                                return D3D.TextureFilter.Point;
                            }
                            else {
                                return D3D.TextureFilter.None;
                            }

                        case FilterOptions.None:
                            return D3D.TextureFilter.None;
                    }
                    break;
                }
            }

            // should never get here
            return 0;
        }

        /// <summary>
        ///		Static method for converting LayerBlendOperationEx enum values to the Direct3D 
        ///		TextureOperation enum.
        /// </summary>
        /// <param name="blendop"></param>
        /// <returns></returns>
        public static D3D.TextureOperation ConvertEnum(LayerBlendOperationEx blendop, D3D.Caps devCaps) {
            D3D.TextureOperation d3dTexOp = 0;

            // figure out what is what
            switch(blendop) {
                case LayerBlendOperationEx.Source1:
                    d3dTexOp = D3D.TextureOperation.SelectArg1;
                    break;

                case LayerBlendOperationEx.Source2:
                    d3dTexOp = D3D.TextureOperation.SelectArg2;
                    break;

                case LayerBlendOperationEx.Modulate:
                    d3dTexOp = D3D.TextureOperation.Modulate;
                    break;

                case LayerBlendOperationEx.ModulateX2:
                    d3dTexOp = D3D.TextureOperation.Modulate2X;
                    break;

                case LayerBlendOperationEx.ModulateX4:
                    d3dTexOp = D3D.TextureOperation.Modulate4X;
                    break;

                case LayerBlendOperationEx.Add:
                    d3dTexOp = D3D.TextureOperation.Add;
                    break;

                case LayerBlendOperationEx.AddSigned:
                    d3dTexOp = D3D.TextureOperation.AddSigned;
                    break;

                case LayerBlendOperationEx.AddSmooth:
                    d3dTexOp = D3D.TextureOperation.AddSmooth;
                    break;

                case LayerBlendOperationEx.Subtract:
                    d3dTexOp = D3D.TextureOperation.Subtract;
                    break;

                case LayerBlendOperationEx.BlendDiffuseAlpha:
                    d3dTexOp = D3D.TextureOperation.BlendDiffuseAlpha;
                    break;

                case LayerBlendOperationEx.BlendTextureAlpha:
                    d3dTexOp = D3D.TextureOperation.BlendTextureAlpha;
                    break;

                case LayerBlendOperationEx.BlendCurrentAlpha:
                    d3dTexOp = D3D.TextureOperation.BlendCurrentAlpha;
                    break;

                case LayerBlendOperationEx.BlendManual:
                    d3dTexOp = D3D.TextureOperation.BlendFactorAlpha;
                    break;

                case LayerBlendOperationEx.DotProduct:
					if (Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.Dot3)) {
						d3dTexOp = D3D.TextureOperation.DotProduct3;
					}
					else {
						d3dTexOp = D3D.TextureOperation.Modulate;
					}
                    break;

                case LayerBlendOperationEx.BlendDiffuseColor:
                    if (devCaps.TextureOperationCaps.SupportsLerp)
                        d3dTexOp = D3D.TextureOperation.Lerp;
                    else
                        d3dTexOp = D3D.TextureOperation.Modulate;
                    break;
                    
            } // end switch

            return d3dTexOp;
        }

        public static D3D.TextureArgument ConvertEnum(LayerBlendSource blendSource) {
            D3D.TextureArgument d3dTexArg = 0;

            switch( blendSource ) {
                case LayerBlendSource.Current:
                    d3dTexArg = D3D.TextureArgument.Current;
                    break;

                case LayerBlendSource.Texture:
                    d3dTexArg = D3D.TextureArgument.TextureColor;
                    break;

                case LayerBlendSource.Diffuse:
                    d3dTexArg = D3D.TextureArgument.Diffuse;
                    break;

                case LayerBlendSource.Specular:
                    d3dTexArg = D3D.TextureArgument.Specular;
                    break;

                case LayerBlendSource.Manual:
                    d3dTexArg = D3D.TextureArgument.TFactor;
                    break;
            } // end switch

            return d3dTexArg;
        }

        /// <summary>
        ///		Helper method to convert Axiom scene blend factors to D3D
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static D3D.Blend ConvertEnum(SceneBlendFactor factor) {
            D3D.Blend d3dBlend = 0;

            switch(factor) {
                case SceneBlendFactor.One:
                    d3dBlend = Blend.One;
                    break;
                case SceneBlendFactor.Zero:
                    d3dBlend =  Blend.Zero;
                    break;
                case SceneBlendFactor.DestColor:
                    d3dBlend =  Blend.DestinationColor;
                    break;
                case SceneBlendFactor.SourceColor:
                    d3dBlend =  Blend.SourceColor;
                    break;
                case SceneBlendFactor.OneMinusDestColor:
                    d3dBlend =  Blend.InvDestinationColor;
                    break;
                case SceneBlendFactor.OneMinusSourceColor:
                    d3dBlend =  Blend.InvSourceColor;
                    break;
                case SceneBlendFactor.DestAlpha:
                    d3dBlend =  Blend.DestinationAlpha;
                    break;
                case SceneBlendFactor.SourceAlpha:
                    d3dBlend =  Blend.SourceAlpha;
                    break;
                case SceneBlendFactor.OneMinusDestAlpha:
                    d3dBlend =  Blend.InvDestinationAlpha;
                    break;
                case SceneBlendFactor.OneMinusSourceAlpha:
                    d3dBlend =  Blend.InvSourceAlpha;
                    break;
            }

            return d3dBlend;
        }

        public static D3D.DeclarationType ConvertEnum(VertexElementType type) {
            // we only need to worry about a few types with D3D
            switch(type) {
                case VertexElementType.Color:
                    return D3D.DeclarationType.Color;

                case VertexElementType.Float1:
                    return D3D.DeclarationType.Float1;

                case VertexElementType.Float2:
                    return D3D.DeclarationType.Float2;

                case VertexElementType.Float3:
                    return D3D.DeclarationType.Float3;

				case VertexElementType.Float4:
					return D3D.DeclarationType.Float4;

				case VertexElementType.Short2:
					return D3D.DeclarationType.Short2;

				case VertexElementType.Short4:
					return D3D.DeclarationType.Short4;

				case VertexElementType.UByte4:
					return D3D.DeclarationType.Ubyte4;

            } // switch

            // keep the compiler happy
            return D3D.DeclarationType.Float3;
        }

        public static D3D.DeclarationUsage ConvertEnum(VertexElementSemantic semantic) {
            switch(semantic) {
                case VertexElementSemantic.BlendIndices:
                    return D3D.DeclarationUsage.BlendIndices;

                case VertexElementSemantic.BlendWeights:
                    return D3D.DeclarationUsage.BlendWeight;

                case VertexElementSemantic.Diffuse:
                    // index makes the difference (diffuse - 0)
                    return D3D.DeclarationUsage.Color;  

                case VertexElementSemantic.Specular:
                    // index makes the difference (specular - 1)
                    return D3D.DeclarationUsage.Color;

                case VertexElementSemantic.Normal:
                    return D3D.DeclarationUsage.Normal;

                case VertexElementSemantic.Position:
                    return D3D.DeclarationUsage.Position;

                case VertexElementSemantic.TexCoords:
                    return D3D.DeclarationUsage.TextureCoordinate;

				case VertexElementSemantic.Binormal:
					return D3D.DeclarationUsage.BiNormal;

				case VertexElementSemantic.Tangent:
					return D3D.DeclarationUsage.Tangent;
            } // switch

            // keep the compiler happy
            return D3D.DeclarationUsage.Position;
        }

        public static D3D.Usage ConvertEnum(BufferUsage usage) {
#if ORIG
            D3D.Usage d3dUsage = 0;

            if(usage == BufferUsage.Dynamic || 
                usage == BufferUsage.DynamicWriteOnly)
                d3dUsage |= D3D.Usage.Dynamic;
            if(usage == BufferUsage.WriteOnly || 
                usage == BufferUsage.StaticWriteOnly || 
                usage == BufferUsage.DynamicWriteOnly)

                d3dUsage |= D3D.Usage.WriteOnly;

            return d3dUsage;
#else
            D3D.Usage ret = 0;

            if ((usage & BufferUsage.Dynamic) != 0) {
#if !NO_OGRE_D3D_MANAGE_BUFFERS
                // Only add the dynamic flag for the default pool, and
                // we use default pool when buffer is discardable
                if ((usage & BufferUsage.Discardable) != 0)
                    ret |= D3D.Usage.Dynamic;
#else
                ret |= D3D.Usage.Dynamic;
#endif
            }
            if ((usage & BufferUsage.WriteOnly) != 0) {
                ret |= D3D.Usage.WriteOnly;
            }
            return ret;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static D3D.FogMode ConvertEnum(Axiom.Graphics.FogMode mode) {
            // convert the fog mode value
            switch(mode) {
                case Axiom.Graphics.FogMode.Exp:
                   return D3D.FogMode.Exp;

                case Axiom.Graphics.FogMode.Exp2:
                    return D3D.FogMode.Exp2;

                case Axiom.Graphics.FogMode.Linear:
                    return D3D.FogMode.Linear;
            } // switch

            return 0;
        }

#if ORIG
        public static D3D.LockFlags ConvertEnum(BufferLocking locking) {
            D3D.LockFlags d3dLockFlags = 0;

            if (locking == BufferLocking.Discard)
                d3dLockFlags |= D3D.LockFlags.Discard;
            if (locking == BufferLocking.ReadOnly)
                d3dLockFlags |= D3D.LockFlags.ReadOnly;
            if (locking == BufferLocking.NoOverwrite)
                d3dLockFlags |= D3D.LockFlags.NoOverwrite;

            return d3dLockFlags;
        }

        public static D3D.LockFlags ConvertEnum(BufferLocking locking, BufferUsage usage) {
            D3D.LockFlags d3dLockFlags = 0;
            if (locking == BufferLocking.Discard) {
                // D3D doesn't like discard or no_overwrite on non-dynamic buffers
                if ((usage & BufferUsage.Dynamic) != 0)
                    d3dLockFlags |= D3D.LockFlags.Discard;
            } else if (locking == BufferLocking.ReadOnly) {
                // D3D debug runtime doesn't like you locking managed buffers readonly
                // when they were created with write-only (even though you CAN read
                // from the software backed version)
                if ((usage & BufferUsage.WriteOnly) == 0)
                    d3dLockFlags |= D3D.LockFlags.ReadOnly;
            } else if (locking == BufferLocking.NoOverwrite) {
                // D3D doesn't like discard or no_overwrite on non-dynamic buffers
                if ((usage & BufferUsage.Dynamic) != 0)
                    d3dLockFlags |= D3D.LockFlags.NoOverwrite;
            }
            return d3dLockFlags;
        }
#else
        public static D3D.LockFlags ConvertEnum(BufferLocking locking, BufferUsage usage) {
            D3D.LockFlags ret = 0;
            if (locking == BufferLocking.Discard)
            {
#if !NO_OGRE_D3D_MANAGE_BUFFERS
                // Only add the discard flag for dynamic usgae and default pool
                if ((usage & BufferUsage.Dynamic) != 0 &&
                    (usage & BufferUsage.Discardable) != 0)
                    ret |= D3D.LockFlags.Discard;
#else
                // D3D doesn't like discard or no_overwrite on non-dynamic buffers
                if ((usage & BufferUsage.Dynamic) != 0)
                    ret |= D3D.LockFlags.Discard;
#endif
            }
            if (locking == BufferLocking.ReadOnly)
            {
	    		// D3D debug runtime doesn't like you locking managed buffers readonly
		    	// when they were created with write-only (even though you CAN read
			    // from the software backed version)
    			if ((usage & BufferUsage.WriteOnly) == 0)
				    ret |= D3D.LockFlags.ReadOnly;

            }
            if (locking == BufferLocking.NoOverwrite)
            {
#if !NO_OGRE_D3D_MANAGE_BUFFERS
                // Only add the nooverwrite flag for dynamic usgae and default pool
                if ((usage & BufferUsage.Dynamic) != 0 &&
                    (usage & BufferUsage.Discardable) != 0)
                    ret |= D3D.LockFlags.NoOverwrite;
#else
                // D3D doesn't like discard or no_overwrite on non-dynamic buffers
                if ((usage & BufferUsage.Dynamic) != 0)
                    ret |= D3D.LockFlags.NoOverwrite;
#endif 
            }

            return ret;
        }
#endif

        public static int ConvertEnum(TexCoordCalcMethod method, D3D.Caps caps) {
            switch(method) {
                case TexCoordCalcMethod.None:
                    return (int)D3D.TextureCoordinateIndex.PassThru;

                case TexCoordCalcMethod.EnvironmentMapReflection:
                    return (int)D3D.TextureCoordinateIndex.CameraSpaceReflectionVector;

                case TexCoordCalcMethod.EnvironmentMapPlanar:
                    //return (int)D3D.TextureCoordinateIndex.CameraSpacePosition;
                    if(caps.VertexProcessingCaps.SupportsTextureGenerationSphereMap) {
                        // use sphere map if available
                        return (int)D3D.TextureCoordinateIndex.SphereMap;
                    }
                    else {
                        // If not, fall back on camera space reflection vector which isn't as good
                        return (int)D3D.TextureCoordinateIndex.CameraSpaceReflectionVector;
                    }

                case TexCoordCalcMethod.EnvironmentMapNormal:
                    return (int)D3D.TextureCoordinateIndex.CameraSpaceNormal;

                case TexCoordCalcMethod.EnvironmentMap:
					if(caps.VertexProcessingCaps.SupportsTextureGenerationSphereMap) {
						return (int)D3D.TextureCoordinateIndex.SphereMap;
					}
					else {
						// fall back on camera space normal if sphere map isnt supported
						return (int)D3D.TextureCoordinateIndex.CameraSpaceNormal;
					}

				case TexCoordCalcMethod.ProjectiveTexture:
					return (int)D3D.TextureCoordinateIndex.CameraSpacePosition;
			} // switch

            return 0;
        }

        public static D3DTexType ConvertEnum(TextureType type) {
            switch(type) {
                case TextureType.OneD:
                case TextureType.TwoD:
                    return D3DTexType.Normal;
                case TextureType.CubeMap:
                    return D3DTexType.Cube;
                case TextureType.ThreeD:
                    return D3DTexType.Volume;
            }

            return D3DTexType.None;
        }

		public static D3D.TextureAddress ConvertEnum(TextureAddressing type) {
			// convert from ours to D3D
			switch(type) {
				case TextureAddressing.Wrap:
					return D3D.TextureAddress.Wrap;

				case TextureAddressing.Mirror:
					return D3D.TextureAddress.Mirror;

				case TextureAddressing.Clamp:
					return D3D.TextureAddress.Clamp;

                case TextureAddressing.Border:
                    return D3D.TextureAddress.Border;
			} // end switch

			return 0;
		}

        /// <summary>
        ///    Converts our CompareFunction enum to the D3D.Compare equivalent.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static D3D.Compare ConvertEnum(CompareFunction func) {
            switch(func) {
                case CompareFunction.AlwaysFail:
                    return D3D.Compare.Never;

                case CompareFunction.AlwaysPass:
                    return D3D.Compare.Always;

                case CompareFunction.Equal:
                    return D3D.Compare.Equal;

                case CompareFunction.Greater:
                    return D3D.Compare.Greater;

                case CompareFunction.GreaterEqual:
                    return D3D.Compare.GreaterEqual;

                case CompareFunction.Less:
                    return D3D.Compare.Less;

                case CompareFunction.LessEqual:
                    return D3D.Compare.LessEqual;

                case CompareFunction.NotEqual:
                    return D3D.Compare.NotEqual;
            }

            return 0;
        }

        /// <summary>
        ///    Converts our Shading enum to the D3D ShadingMode equivalent.
        /// </summary>
        /// <param name="shading"></param>
        /// <returns></returns>
        public static D3D.ShadeMode ConvertEnum(Shading shading) {
            switch(shading) {
                case Shading.Flat:
                    return D3D.ShadeMode.Flat;
                case Shading.Gouraud:
                    return D3D.ShadeMode.Gouraud;
                case Shading.Phong:
                    return D3D.ShadeMode.Phong;
            }

            return 0;
        }
		/// <summary>
		///    Converts our Shading enum to the D3D ShadingMode equivalent.
		/// </summary>
		/// <param name="shading"></param>
		/// <returns></returns>
		public static Shading ConvertEnum(D3D.ShadeMode shading) 
		{
			switch(shading) 
			{
				case ShadeMode.Flat:
					return Shading.Flat;
				case ShadeMode.Gouraud:
					return Shading.Gouraud;
				case ShadeMode.Phong:
					return Shading.Phong;
			}

			return 0;
		}

		public static D3D.StencilOperation ConvertEnum(Axiom.Graphics.StencilOperation op) {
			return ConvertEnum(op, false);
		}

        /// <summary>
        ///    Converts our StencilOperation enum to the D3D StencilOperation equivalent.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static D3D.StencilOperation ConvertEnum(Axiom.Graphics.StencilOperation op, bool invert) {
            switch(op) {
				case Axiom.Graphics.StencilOperation.Keep:
					return D3D.StencilOperation.Keep;

                case Axiom.Graphics.StencilOperation.Zero:
                    return D3D.StencilOperation.Zero;

                case Axiom.Graphics.StencilOperation.Replace:
                    return D3D.StencilOperation.Replace;

                case Axiom.Graphics.StencilOperation.Increment:
                    return invert ? 
						D3D.StencilOperation.DecrementSaturation : D3D.StencilOperation.IncrementSaturation;

                case Axiom.Graphics.StencilOperation.Decrement:
					return invert ? 
						D3D.StencilOperation.IncrementSaturation : D3D.StencilOperation.DecrementSaturation;

				case Axiom.Graphics.StencilOperation.IncrementWrap:
					return invert ? 
						D3D.StencilOperation.Decrement : D3D.StencilOperation.Increment;

				case Axiom.Graphics.StencilOperation.DecrementWrap:
					return invert ? 
						D3D.StencilOperation.Increment : D3D.StencilOperation.Decrement;

				case Axiom.Graphics.StencilOperation.Invert:
					return D3D.StencilOperation.Invert;
            }

            return 0;
        }

        public static D3D.Cull ConvertEnum(Axiom.Graphics.CullingMode mode, bool flip) {
            switch(mode) {
                case CullingMode.None:
                    return D3D.Cull.None;

                case CullingMode.Clockwise:
                    return flip ? D3D.Cull.CounterClockwise : D3D.Cull.Clockwise;

                case CullingMode.CounterClockwise:
                    return flip ? D3D.Cull.Clockwise : D3D.Cull.CounterClockwise;
            }

            return 0;
        }

        public static D3D.Format ConvertEnum(Axiom.Media.PixelFormat format) {
            switch (format) {
                case Axiom.Media.PixelFormat.A8:
                    return D3D.Format.A8;
                case Axiom.Media.PixelFormat.L8:
                    return D3D.Format.L8;
                case Axiom.Media.PixelFormat.L16:
                    return D3D.Format.L16;
                case Axiom.Media.PixelFormat.A4L4:
                    return D3D.Format.A4L4;
                case Axiom.Media.PixelFormat.BYTE_LA:
                    return D3D.Format.A8L8;	// Assume little endian here
                case Axiom.Media.PixelFormat.R3G3B2:
                    return D3D.Format.R3G3B2;
                case Axiom.Media.PixelFormat.A1R5G5B5:
                    return D3D.Format.A1R5G5B5;
                case Axiom.Media.PixelFormat.A4R4G4B4:
                    return D3D.Format.A4R4G4B4;
                case Axiom.Media.PixelFormat.R5G6B5:
                    return D3D.Format.R5G6B5;
                case Axiom.Media.PixelFormat.R8G8B8:
                    return D3D.Format.R8G8B8;
                case Axiom.Media.PixelFormat.X8R8G8B8:
                    return D3D.Format.X8R8G8B8;
                case Axiom.Media.PixelFormat.A8R8G8B8:
                    return D3D.Format.A8R8G8B8;
                case Axiom.Media.PixelFormat.X8B8G8R8:
                    return D3D.Format.X8B8G8R8;
                case Axiom.Media.PixelFormat.A8B8G8R8:
                    return D3D.Format.A8B8G8R8;
                case Axiom.Media.PixelFormat.A2R10G10B10:
                    return D3D.Format.A2R10G10B10;
                case Axiom.Media.PixelFormat.A2B10G10R10:
                    return D3D.Format.A2B10G10R10;
                case Axiom.Media.PixelFormat.FLOAT16_R:
                    return D3D.Format.R16F;
                case Axiom.Media.PixelFormat.FLOAT16_RGBA:
                    return D3D.Format.A16B16G16R16F;
                case Axiom.Media.PixelFormat.FLOAT32_R:
                    return D3D.Format.R32F;
                case Axiom.Media.PixelFormat.FLOAT32_RGBA:
                    return D3D.Format.A32B32G32R32F;
                case Axiom.Media.PixelFormat.SHORT_RGBA:
                    return D3D.Format.A16B16G16R16;
                case Axiom.Media.PixelFormat.DXT1:
                    return D3D.Format.Dxt1;
                case Axiom.Media.PixelFormat.DXT2:
                    return D3D.Format.Dxt2;
                case Axiom.Media.PixelFormat.DXT3:
                    return D3D.Format.Dxt3;
                case Axiom.Media.PixelFormat.DXT4:
                    return D3D.Format.Dxt4;
                case Axiom.Media.PixelFormat.DXT5:
                    return D3D.Format.Dxt5;
                default:
                    return D3D.Format.Unknown;
            }
        }

        public static Axiom.Media.PixelFormat ConvertEnum(D3D.Format format) {
            switch (format) {
                case D3D.Format.A8:
                    return Axiom.Media.PixelFormat.A8;
                case D3D.Format.L8:
                    return Axiom.Media.PixelFormat.L8;
                case D3D.Format.L16:
                    return Axiom.Media.PixelFormat.L16;
                case D3D.Format.A4L4:
                    return Axiom.Media.PixelFormat.A4L4;
                case D3D.Format.A8L8:	// Assume little endian here
                    return Axiom.Media.PixelFormat.BYTE_LA;
                case D3D.Format.R3G3B2:
                    return Axiom.Media.PixelFormat.R3G3B2;
                case D3D.Format.A1R5G5B5:
                    return Axiom.Media.PixelFormat.A1R5G5B5;
                case D3D.Format.A4R4G4B4:
                    return Axiom.Media.PixelFormat.A4R4G4B4;
                case D3D.Format.R5G6B5:
                    return Axiom.Media.PixelFormat.R5G6B5;
                case D3D.Format.R8G8B8:
                    return Axiom.Media.PixelFormat.R8G8B8;
                case D3D.Format.X8R8G8B8:
                    return Axiom.Media.PixelFormat.X8R8G8B8;
                case D3D.Format.A8R8G8B8:
                    return Axiom.Media.PixelFormat.A8R8G8B8;
                case D3D.Format.X8B8G8R8:
                    return Axiom.Media.PixelFormat.X8B8G8R8;
                case D3D.Format.A8B8G8R8:
                    return Axiom.Media.PixelFormat.A8B8G8R8;
                case D3D.Format.A2R10G10B10:
                    return Axiom.Media.PixelFormat.A2R10G10B10;
                case D3D.Format.A2B10G10R10:
                    return Axiom.Media.PixelFormat.A2B10G10R10;
                case D3D.Format.R16F:
                    return Axiom.Media.PixelFormat.FLOAT16_R;
                case D3D.Format.A16B16G16R16F:
                    return Axiom.Media.PixelFormat.FLOAT16_RGBA;
                case D3D.Format.R32F:
                    return Axiom.Media.PixelFormat.FLOAT32_R;
                case D3D.Format.A32B32G32R32F:
                    return Axiom.Media.PixelFormat.FLOAT32_RGBA;
                case D3D.Format.A16B16G16R16:
                    return Axiom.Media.PixelFormat.SHORT_RGBA;
                case D3D.Format.Dxt1:
                    return Axiom.Media.PixelFormat.DXT1;
                case D3D.Format.Dxt2:
                    return Axiom.Media.PixelFormat.DXT2;
                case D3D.Format.Dxt3:
                    return Axiom.Media.PixelFormat.DXT3;
                case D3D.Format.Dxt4:
                    return Axiom.Media.PixelFormat.DXT4;
                case D3D.Format.Dxt5:
                    return Axiom.Media.PixelFormat.DXT5;
                default:
                    return Axiom.Media.PixelFormat.Unknown;
            }
        }

        /// <summary>
        ///    Checks D3D matrix to see if it an identity matrix.
        /// </summary>
        /// <remarks>
        ///    For whatever reason, the equality operator overloads for the D3D Matrix
        ///    struct are extremely slow....
        /// </remarks>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static bool IsIdentity(ref Microsoft.DirectX.Matrix matrix) {
            if(matrix.M11 == 1.0f &&
                matrix.M12 == 0.0f &&
                matrix.M13 == 0.0f &&
                matrix.M14 == 0.0f &&
                matrix.M21 == 0.0f &&
                matrix.M22 == 1.0f &&
                matrix.M23 == 0.0f &&
                matrix.M24 == 0.0f &&
                matrix.M31 == 0.0f &&
                matrix.M32 == 0.0f &&
                matrix.M33 == 1.0f &&
                matrix.M34 == 0.0f &&
                matrix.M41 == 0.0f &&
                matrix.M42 == 0.0f &&
                matrix.M43 == 0.0f &&
                matrix.M44 == 1.0f) {

                return true;
            }
            else {
                return false;
            }
        }

        public static Axiom.Media.PixelFormat GetClosestSupported(Axiom.Media.PixelFormat format) {
            if (ConvertEnum(format) != D3D.Format.Unknown)
                return format;
            switch (format) {
                case Axiom.Media.PixelFormat.B5G6R5:
                    return Axiom.Media.PixelFormat.R5G6B5;
                case Axiom.Media.PixelFormat.B8G8R8:
                    return Axiom.Media.PixelFormat.R8G8B8;
                case Axiom.Media.PixelFormat.B8G8R8A8:
                    return Axiom.Media.PixelFormat.A8R8G8B8;
                case Axiom.Media.PixelFormat.FLOAT16_RGB:
                    return Axiom.Media.PixelFormat.FLOAT16_RGBA;
                case Axiom.Media.PixelFormat.FLOAT32_RGB:
                    return Axiom.Media.PixelFormat.FLOAT32_RGBA;
                case Axiom.Media.PixelFormat.Unknown:
                default:
                    return Axiom.Media.PixelFormat.A8R8G8B8;
            }
        }
    }
}
