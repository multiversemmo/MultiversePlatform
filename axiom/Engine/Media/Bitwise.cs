using System;
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

namespace Axiom.Media {


    ///<summary>
	///    Class for manipulating bit patterns.
    ///</summary>
    public class Bitwise {

		///<summary>
		///    Returns the most significant bit set in a value.
		///</summary>
        public static uint MostSignificantBitSet(uint value) {
            uint result = 0;
            while (value != 0) {
                ++result;
                value >>= 1;
            }
            return result-1;
        }

		///<summary>
		///    Returns the closest power-of-two number greater or equal to value.
		///</summary>
		///<remarks>
		///   0 and 1 are powers of two, so firstPO2From(0)==0 and firstPO2From(1)==1.
		///</remarks>
        public static uint FirstPO2From(uint n) {
            --n;            
            n |= n >> 16;
            n |= n >> 8;
            n |= n >> 4;
            n |= n >> 2;
            n |= n >> 1;
            ++n;
            return n;
        }

		///<summary>
		///    Convert N bit colour channel value to P bits. It fills P bits with the
		///    bit pattern repeated. (this is /((1<<n)-1) in fixed point)
		///</summary>
        public static uint FixedToFixed(uint value, int n, int p) {
            if(n > p) 
                // Less bits required than available; this is easy
                value >>= n - p;
            else if(n < p) {
                // More bits required than are there, do the fill
                // Use old fashioned division, probably better than a loop
                if(value == 0)
                        value = 0;
                else if(value == ((uint)(1)<<n) - 1)
                        value = (1u<<p)-1;
                else    value = value * (1u<<p) / ((1u<<n) - 1u);
            }
            return value;    
        }

		///<summary>
		///    Convert N bit colour channel value to 8 bits, and return as a byte. It 
		///    fills P bits with thebit pattern repeated. (this is /((1<<n)-1) in fixed point)
		///</summary>
		public static byte FixedToByteFixed(uint value, int p) {
			return (byte)FixedToFixed(value, 8, p);
		}
		
		///<summary>
		///    Convert floating point colour channel value between 0.0 and 1.0 (otherwise clamped) 
		///    to integer of a certain number of bits. Works for any value of bits between 0 and 31.
		///</summary>
        public static uint FloatToFixed(float value, int bits) {
            if(value <= 0.0f) return 0;
            else if (value >= 1.0f) return (1u<<bits)-1;
            else return (uint)(value * (1u<<bits));     
        }

		///<summary>
		///    Convert floating point colour channel value between 0.0 and 1.0 (otherwise clamped) 
		///    to an 8-bit integer, and return as a byte.
		///</summary>
		public static byte FloatToByteFixed(float value) {
			return (byte)FloatToFixed(value, 8);
		}
		
		///<summary>
		///    Fixed point to float
		///</summary>
        public static float FixedToFloat(uint value, int bits) {
            return (float)value/(float)((1u<<bits)-1);
        }

        /**
         * Write a n*8 bits integer value to memory in native endian.
         */
        unsafe public static void IntWrite(byte *dest, int n, uint value)
        {
			switch(n) {
                case 1:
                    ((byte*)dest)[0] = (byte)value;
                    break;
                case 2:
                    ((ushort*)dest)[0] = (ushort)value;
                    break;
                case 3:
#if BIG_ENDIAN
                    ((byte*)dest)[0] = (byte)((value >> 16) & 0xFF);
                    ((byte*)dest)[1] = (byte)((value >> 8) & 0xFF);
                    ((byte*)dest)[2] = (byte)(value & 0xFF);
#else
                    ((byte*)dest)[2] = (byte)((value >> 16) & 0xFF);
                    ((byte*)dest)[1] = (byte)((value >> 8) & 0xFF);
                    ((byte*)dest)[0] = (byte)(value & 0xFF);
#endif
                    break;
                case 4:
                    ((uint*)dest)[0] = (uint)value;                
                    break;                
            }        
        }

		///<summary>
		///    Read a n*8 bits integer value to memory in native endian.
		///</summary>
		unsafe public static uint IntRead(byte *src, int n) {
			switch(n) {
                case 1:
                    return ((byte*)src)[0];
                case 2:
                    return ((ushort*)src)[0];
                case 3:
#if BIG_ENDIAN
                    return ((uint)((byte*)src)[0]<<16)|
                            ((uint)((byte*)src)[1]<<8)|
                            ((uint)((byte*)src)[2]);
#else
                    return ((uint)((byte*)src)[0])|
                            ((uint)((byte*)src)[1]<<8)|
                            ((uint)((byte*)src)[2]<<16);
#endif
                case 4:
                    return ((uint*)src)[0];
            } 
            return 0; // ?
        }

		private static float [] floatConversionBuffer = new float[] { 0f };
        private static uint[] uintConversionBuffer = new uint[] { 0 };

		///<summary>
		///    Convert a float32 to a float16 (NV_half_float)
		///    Courtesy of OpenEXR
		///</summary>
        public static ushort FloatToHalf(float f) {
			uint i;
			floatConversionBuffer[0] = f;
			unsafe {
				fixed (float* pFloat = floatConversionBuffer) {
					i = *((uint *)pFloat);
				}
			}
            return FloatToHalfI(i);
        }

		///<summary>
		///    Converts float in uint format to a a half in ushort format
		///</summary>
        public static ushort FloatToHalfI(uint i) {
            int s = (int)(i >> 16) & 0x00008000;
            int e = (int)((i >> 23) & 0x000000ff) - (127 - 15);
            int m = (int)i        & 0x007fffff;
        
            if (e <= 0)
            {
                if (e < -10)
                {
                    return 0;
                }
                m = (m | 0x00800000) >> (1 - e);
        
                return (ushort)(s | (m >> 13));
            }
            else if (e == 0xff - (127 - 15))
            {
                if (m == 0) // Inf
                {
                    return (ushort)(s | 0x7c00);
                } 
                else    // NAN
                {
                    m >>= 13;
                    return (ushort)(s | 0x7c00 | m | (m == 0 ? 0x0001 : 0x0000));
                }
            }
            else
            {
                if (e > 30) // Overflow
                {
                    return (ushort)(s | 0x7c00);
                }
        
                return (ushort)(s | (e << 10) | (m >> 13));
            }
        }
        
		///<summary>
		///    Convert a float16 (NV_half_float) to a float32
		///    Courtesy of OpenEXR
		///</summary>
        public static float HalfToFloat(ushort y) {
			uintConversionBuffer[0] = HalfToFloatI(y);
			unsafe {
				fixed (uint* pUint = uintConversionBuffer) {
					return *((float *)pUint);
				}
			}
        }

		///<summary>
		///    Converts a half in ushort format to a float
		///    in uint format
		///</summary>
        public static uint HalfToFloatI(ushort y) {
            uint yuint = (uint)y;
			uint s = (yuint >> 15) & 0x00000001;
            uint e = (yuint >> 10) & 0x0000001f;
            uint m =  yuint        & 0x000003ff;
        
            if (e == 0) {
                if (m == 0) // Plus or minus zero
                    return (s << 31);
                else { // Denormalized number -- renormalize it
                    while ((m & 0x00000400) == 0) {
                        m <<= 1;
                        e -=  1;
                    }
                    e += 1;
                    m &= 0xFFFFFBFF; // ~0x00000400;
                }
            }
            else if (e == 31) {
                if (m == 0) // Inf
                    return (s << 31) | 0x7f800000;
                else // NaN
                    return (s << 31) | 0x7f800000 | (m << 13);
            }

            e = e + (127 - 15);
            m = m << 13;

            return (s << 31) | (e << 23) | m;
        }

    }
}
