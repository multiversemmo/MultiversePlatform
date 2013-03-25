using System;
using System.IO;
using Axiom.Core;
/*
Axiom Game Engine Library
Copyright (C) 2006  Multiverse Corporation

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

/// This file implements all the Ogre pixel format conversion loops.  
/// However, Axiom doesn't currently support all the Ogre pixel formats.
/// So the unsupported ones are not called; the switch statement at the
/// bottom of the file has the unsupported calls commented out.

namespace Axiom.Media {

    /** Type for R8G8B8/B8G8R8 */
    public struct Col3b {
        public byte x, y, z;
        public Col3b(uint a, uint b, uint c) {
            x = (byte)a;
            y = (byte)b;
            z = (byte)c;
        }
    }

    /** Type for FLOAT32_RGB */
    public struct Col3f {
        public float r, g, b;
        public Col3f(float r, float g, float b) {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    /** Type for FLOAT32_RGBA */
    public struct Col4f {
        public float r, g, b, a;
        public Col4f(float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    ///<summary>
    ///    A class to convert/copy pixels of the same or different formats
    ///</summary>
    public class PixelConversionLoops {

        unsafe private static void A8R8G8B8toA8B8G8R8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 16) | (inp & 0xFF00FF00) | ((inp & 0x00FF0000) >> 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8R8G8B8toB8G8R8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 24) | ((inp & 0x0000FF00) << 8) | ((inp & 0x00FF0000) >> 8) | ((inp & 0xFF000000) >> 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8R8G8B8toR8G8B8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x00FFFFFF) << 8) | ((inp & 0xFF000000) >> 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8B8G8R8toA8R8G8B8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 16) | (inp & 0xFF00FF00) | ((inp & 0x00FF0000) >> 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8B8G8R8toB8G8R8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x00FFFFFF) << 8) | ((inp & 0xFF000000) >> 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8B8G8R8toR8G8B8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 24) | ((inp & 0x0000FF00) << 8) | ((inp & 0x00FF0000) >> 8) | ((inp & 0xFF000000) >> 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void B8G8R8A8toA8R8G8B8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 24) | ((inp & 0x0000FF00) << 8) | ((inp & 0x00FF0000) >> 8) | ((inp & 0xFF000000) >> 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void B8G8R8A8toA8B8G8R8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 24) | ((inp & 0xFFFFFF00) >> 8);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void B8G8R8A8toR8G8B8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x0000FF00) << 16) | (inp & 0x00FF00FF) | ((inp & 0xFF000000) >> 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void R8G8B8A8toA8B8G8R8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x000000FF) << 24) | ((inp & 0x0000FF00) << 8) | ((inp & 0x00FF0000) >> 8) | ((inp & 0xFF000000) >> 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void R8G8B8A8toB8G8R8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp & 0x0000FF00) << 16) | (inp & 0x00FF00FF) | ((inp & 0xFF000000) >> 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8B8G8R8toL8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            byte* dstptr = (byte*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = (byte)(inp & 0x000000FF);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void L8toA8B8G8R8(PixelBox src, PixelBox dst) {
            byte* srcptr = (byte*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        byte inp = srcptr[x];
                        dstptr[x] = 0xFF000000 | (((uint)inp) << 0) | (((uint)inp) << 8) | (((uint)inp) << 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8R8G8B8toL8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            byte* dstptr = (byte*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = (byte)((inp & 0x00FF0000) >> 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void L8toA8R8G8B8(PixelBox src, PixelBox dst) {
            byte* srcptr = (byte*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        byte inp = srcptr[x];
                        dstptr[x] = 0xFF000000 | (((uint)inp) << 0) | (((uint)inp) << 8) | (((uint)inp) << 16);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void B8G8R8A8toL8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            byte* dstptr = (byte*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = (byte)((inp & 0x0000FF00) >> 8);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void L8toB8G8R8A8(PixelBox src, PixelBox dst) {
            byte* srcptr = (byte*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        byte inp = srcptr[x];
                        dstptr[x] = 0x000000FF | (((uint)inp) << 8) | (((uint)inp) << 16) | (((uint)inp) << 24);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void L8toL16(PixelBox src, PixelBox dst) {
            byte* srcptr = (byte*)(src.Data.ToPointer());
            ushort* dstptr = (ushort*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        byte inp = srcptr[x];
                        dstptr[x] = (ushort)((((uint)inp) << 8) | (((uint)inp)));
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void L16toL8(PixelBox src, PixelBox dst) {
            ushort* srcptr = (ushort*)(src.Data.ToPointer());
            byte* dstptr = (byte*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        ushort inp = srcptr[x];
                        dstptr[x] = (byte)(inp >> 8);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void R8G8B8toB8G8R8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            Col3b* dstptr = (Col3b*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
                        dstptr[x].x = inp.z;
						dstptr[x].y = inp.y;
						dstptr[x].z = inp.x;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void B8G8R8toR8G8B8(PixelBox src, PixelBox dst) {
            Col3b *srcptr = (Col3b *)(src.Data.ToPointer());
            Col3b *dstptr = (Col3b *)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
                        dstptr[x].x = inp.z;
						dstptr[x].y = inp.y;
						dstptr[x].z = inp.x;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8R8G8B8toR8G8B8(PixelBox src, PixelBox dst) {
            uint *srcptr = (uint *)(src.Data.ToPointer());
            Col3b *dstptr = (Col3b *)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x].x = (byte)((inp>>16)&0xFF);
						dstptr[x].y = (byte)((inp>>8)&0xFF);
						dstptr[x].z = (byte)((inp>>0)&0xFF);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void A8R8G8B8toB8G8R8(PixelBox src, PixelBox dst) {
            uint *srcptr = (uint *)(src.Data.ToPointer());
            Col3b *dstptr = (Col3b *)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x].x = (byte)((inp>>0)&0xFF);
						dstptr[x].y = (byte)((inp>>8)&0xFF);
						dstptr[x].z = (byte)((inp>>16)&0xFF);
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8R8G8B8toA8R8G8B8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = inp | 0xFF000000;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8R8G8B8toA8B8G8R8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp&0x0000FF)<<16)|((inp&0xFF0000)>>16)|(inp&0x00FF00)|0xFF000000;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8R8G8B8toB8G8R8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp&0x0000FF)<<24)|((inp&0xFF0000)>>8)|((inp&0x00FF00)<<8)|0x000000FF;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8R8G8B8toR8G8B8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp&0xFFFFFF)<<8)|0x000000FF;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8B8G8R8toA8R8G8B8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp&0x0000FF)<<16)|((inp&0xFF0000)>>16)|(inp&0x00FF00)|0xFF000000;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8B8G8R8toA8B8G8R8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = inp | 0xFF000000;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8B8G8R8toB8G8R8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp&0xFFFFFF)<<8)|0x000000FF;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

        unsafe private static void X8B8G8R8toR8G8B8A8(PixelBox src, PixelBox dst) {
            uint* srcptr = (uint*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        uint inp = srcptr[x];
                        dstptr[x] = ((inp&0x0000FF)<<24)|((inp&0xFF0000)>>8)|((inp&0x00FF00)<<8)|0x000000FF;
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }
		
        unsafe private static void R8G8B8toA8R8G8B8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int xshift = 16;
			int yshift = 8;
			int zshift = 0;
			int ashift = 24;
			int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
#if BIG_ENDIAN
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<xshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<zshift);
#else
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<zshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<xshift);
#endif
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }
		
        unsafe private static void B8G8R8toA8R8G8B8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int xshift = 0;
			int yshift = 8;
			int zshift = 16;
			int ashift = 24;
			int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
#if BIG_ENDIAN
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<xshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<zshift);
#else
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<zshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<xshift);
#endif
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }
		
        unsafe private static void R8G8B8toA8B8G8R8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int xshift = 0;
			int yshift = 8;
			int zshift = 16;
			int ashift = 24;
			int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
#if BIG_ENDIAN
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<xshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<zshift);
#else
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<zshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<xshift);
#endif
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }
		
        unsafe private static void B8G8R8toA8B8G8R8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int xshift = 8;
			int yshift = 16;
			int zshift = 24;
			int ashift = 0;
			int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
#if BIG_ENDIAN
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<xshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<zshift);
#else
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<zshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<xshift);
#endif
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }
		
        unsafe private static void R8G8B8toB8G8R8A8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int xshift = 8;
			int yshift = 16;
			int zshift = 24;
			int ashift = 0;
			int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
#if BIG_ENDIAN
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<xshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<zshift);
#else
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<zshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<xshift);
#endif
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }
		
        unsafe private static void B8G8R8toB8G8R8A8(PixelBox src, PixelBox dst) {
            Col3b* srcptr = (Col3b*)(src.Data.ToPointer());
            uint* dstptr = (uint*)(dst.Data.ToPointer());
            int xshift = 24;
			int yshift = 16;
			int zshift = 8;
			int ashift = 0;
			int srcSliceSkip = src.SliceSkip;
            int dstSliceSkip = dst.SliceSkip;
            int k = src.Right - src.Left;
            for (int z = src.Front; z < src.Back; z++) {
                for (int y = src.Top; y < src.Bottom; y++) {
                    for (int x = 0; x < k; x++) {
                        Col3b inp = srcptr[x];
#if BIG_ENDIAN
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<xshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<zshift);
#else
                        dstptr[x] = ((uint)(0xFF<<ashift)) | (((uint)inp.x)<<zshift) | (((uint)inp.y)<<yshift) | (((uint)inp.z)<<xshift);
#endif
                    }
                    srcptr += src.RowPitch;
                    dstptr += dst.RowPitch;
                }
                srcptr += srcSliceSkip;
                dstptr += dstSliceSkip;
            }
        }

		public static bool DoOptimizedConversion(PixelBox src, PixelBox dst) {
			switch (((int)src.Format << 8) + (int)dst.Format) {
                case ((int)PixelFormat.A8R8G8B8 << 8) + (int)PixelFormat.A8B8G8R8:
                    A8R8G8B8toA8B8G8R8(src, dst);
                    break;
			    case ((int)PixelFormat.A8R8G8B8 << 8) + (int)PixelFormat.B8G8R8A8:
				    A8R8G8B8toB8G8R8A8(src, dst);
				    break;
                case ((int)PixelFormat.A8R8G8B8 << 8) + (int)PixelFormat.R8G8B8A8:
                    A8R8G8B8toR8G8B8A8(src, dst);
                    break;
                case ((int)PixelFormat.A8B8G8R8 << 8) + (int)PixelFormat.A8R8G8B8:
                    A8B8G8R8toA8R8G8B8(src, dst);
                    break;
                case ((int)PixelFormat.A8B8G8R8 << 8) + (int)PixelFormat.B8G8R8A8:
                    A8B8G8R8toB8G8R8A8(src, dst);
                    break;
                case ((int)PixelFormat.A8B8G8R8 << 8) + (int)PixelFormat.R8G8B8A8:
                    A8B8G8R8toR8G8B8A8(src, dst);
                    break;
			    case ((int)PixelFormat.B8G8R8A8 << 8) + (int)PixelFormat.A8R8G8B8:
				    B8G8R8A8toA8R8G8B8(src, dst);
				    break;
                case ((int)PixelFormat.B8G8R8A8 << 8) + (int)PixelFormat.A8B8G8R8:
                    B8G8R8A8toA8B8G8R8(src, dst);
                    break;
                case ((int)PixelFormat.B8G8R8A8 << 8) + (int)PixelFormat.R8G8B8A8:
                    B8G8R8A8toR8G8B8A8(src, dst);
                    break;
                //case ((int)PixelFormat.R8G8B8A8 << 8) + (int)PixelFormat.A8R8G8B8:
                //    R8G8B8A8toA8R8G8B8(src, dst);
                //    break;
                case ((int)PixelFormat.R8G8B8A8 << 8) + (int)PixelFormat.A8B8G8R8:
                    R8G8B8A8toA8B8G8R8(src, dst);
                    break;
                case ((int)PixelFormat.R8G8B8A8 << 8) + (int)PixelFormat.B8G8R8A8:
                    R8G8B8A8toB8G8R8A8(src, dst);
                    break;
                case ((int)PixelFormat.A8B8G8R8 << 8) + (int)PixelFormat.L8:
                    A8B8G8R8toL8(src, dst);
                    break;
                case ((int)PixelFormat.L8 << 8) + (int)PixelFormat.A8B8G8R8:
                    L8toA8B8G8R8(src, dst);
                    break;
			    case ((int)PixelFormat.A8R8G8B8 << 8) + (int)PixelFormat.L8:
				    A8R8G8B8toL8(src, dst);
				    break;
			    case ((int)PixelFormat.L8 << 8) + (int)PixelFormat.A8R8G8B8:
				    L8toA8R8G8B8(src, dst);
				    break;
			    case ((int)PixelFormat.B8G8R8A8 << 8) + (int)PixelFormat.L8:
				    B8G8R8A8toL8(src, dst);
				    break;
			    case ((int)PixelFormat.L8 << 8) + (int)PixelFormat.B8G8R8A8:
				    L8toB8G8R8A8(src, dst);
				    break;
			    case ((int)PixelFormat.L8 << 8) + (int)PixelFormat.L16:
				    L8toL16(src, dst);
				    break;
			    case ((int)PixelFormat.L16 << 8) + (int)PixelFormat.L8:
				    L16toL8(src, dst);
				    break;
			    case ((int)PixelFormat.B8G8R8 << 8) + (int)PixelFormat.R8G8B8:
				    B8G8R8toR8G8B8(src, dst);
				    break;
			    case ((int)PixelFormat.R8G8B8 << 8) + (int)PixelFormat.B8G8R8:
				    R8G8B8toB8G8R8(src, dst);
				    break;
			    case ((int)PixelFormat.R8G8B8 << 8) + (int)PixelFormat.A8R8G8B8:
				    R8G8B8toA8R8G8B8(src, dst);
				    break;
			    case ((int)PixelFormat.B8G8R8 << 8) + (int)PixelFormat.A8R8G8B8:
				    B8G8R8toA8R8G8B8(src, dst);
				    break;
                case ((int)PixelFormat.R8G8B8 << 8) + (int)PixelFormat.A8B8G8R8:
                    R8G8B8toA8B8G8R8(src, dst);
                    break;
                case ((int)PixelFormat.B8G8R8 << 8) + (int)PixelFormat.A8B8G8R8:
                    B8G8R8toA8B8G8R8(src, dst);
                    break;
			    case ((int)PixelFormat.R8G8B8 << 8) + (int)PixelFormat.B8G8R8A8:
				    R8G8B8toB8G8R8A8(src, dst);
				    break;
                case ((int)PixelFormat.B8G8R8 << 8) + (int)PixelFormat.B8G8R8A8:
				    B8G8R8toB8G8R8A8(src, dst);
				    break;
                case ((int)PixelFormat.A8R8G8B8 << 8) + (int)PixelFormat.R8G8B8:
                    A8R8G8B8toR8G8B8(src, dst);
                    break;
                case ((int)PixelFormat.A8R8G8B8 << 8) + (int)PixelFormat.B8G8R8:
				    A8R8G8B8toB8G8R8(src, dst);
				    break;
                case ((int)PixelFormat.X8R8G8B8 << 8) + (int)PixelFormat.A8R8G8B8:
                    X8R8G8B8toA8R8G8B8(src, dst);
                    break;
                case ((int)PixelFormat.X8R8G8B8 << 8) + (int)PixelFormat.A8B8G8R8:
                    X8R8G8B8toA8B8G8R8(src, dst);
                    break;
                case ((int)PixelFormat.X8R8G8B8 << 8) + (int)PixelFormat.B8G8R8A8:
                    X8R8G8B8toB8G8R8A8(src, dst);
                    break;
                case ((int)PixelFormat.X8R8G8B8 << 8) + (int)PixelFormat.R8G8B8A8:
                    X8R8G8B8toR8G8B8A8(src, dst);
                    break;
                case ((int)PixelFormat.X8B8G8R8 << 8) + (int)PixelFormat.A8R8G8B8:
                    X8B8G8R8toA8R8G8B8(src, dst);
                    break;
                case ((int)PixelFormat.X8B8G8R8 << 8) + (int)PixelFormat.A8B8G8R8:
                    X8B8G8R8toA8B8G8R8(src, dst);
                    break;
                case ((int)PixelFormat.X8B8G8R8 << 8) + (int)PixelFormat.B8G8R8A8:
                    X8B8G8R8toB8G8R8A8(src, dst);
                    break;
                case ((int)PixelFormat.X8B8G8R8 << 8) + (int)PixelFormat.R8G8B8A8:
                    X8B8G8R8toR8G8B8A8(src, dst);
                    break;
			    default:
				    return false;
			}
			return true;
		}
	}
}


