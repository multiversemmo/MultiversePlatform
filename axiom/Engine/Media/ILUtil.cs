using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Tao.DevIl;
using Axiom.Core;

namespace Axiom.Media {
    public class ILFormat {
        public ILFormat(int channels, int format, int type) {
            this.channels = channels;
            this.format = format;
            this.type = type;
        }
        public ILFormat(int channels, int format) {
            this.channels = channels;
            this.format = format;
            this.type = Il.IL_TYPE_UNKNOWN;
        }
        public ILFormat() {
            this.channels = 0;
            this.format = Il.IL_FORMAT_NOT_SUPPORTED;
            this.type = Il.IL_TYPE_UNKNOWN;
        }
        public int channels = 0;
        public int format = Il.IL_FORMAT_NOT_SUPPORTED;
        public int type = Il.IL_TYPE_UNKNOWN;
    }

    public class ILUtil {
#if NOT
    //-----------------------------------------------------------------------
	/// Helper functions for DevIL to Ogre conversion
	inline void packI(uint8 r, uint8 g, uint8 b, uint8 a, PixelFormat pf,  void* dest)
	{
		PixelUtil::packColour(r, g, b, a, pf, dest);
	}
	inline void packI(uint16 r, uint16 g, uint16 b, uint16 a, PixelFormat pf,  void* dest)
	{
		PixelUtil::packColour((float)r/65535.0f, (float)g/65535.0f, 
			(float)b/65535.0f, (float)a/65535.0f, pf, dest);
	}
	inline void packI(float r, float g, float b, float a, PixelFormat pf,  void* dest)
	{
		PixelUtil::packColour(r, g, b, a, pf, dest);
	}
    template <typename T> void ilToOgreInternal(uint8 *tar, PixelFormat ogrefmt, 
        T r, T g, T b, T a)
    {
        const int ilfmt = ilGetInteger( IL_IMAGE_FORMAT );
        T *src = (T*)ilGetData();
        T *srcend = (T*)((uint8*)ilGetData() + ilGetInteger( IL_IMAGE_SIZE_OF_DATA ));
        const size_t elemSize = PixelUtil::getNumElemBytes(ogrefmt);
        while(src < srcend) {
            switch(ilfmt) {
			case IL_RGB:
				r = src[0];	g = src[1];	b = src[2];
				src += 3;
				break;
			case IL_BGR:
				b = src[0];	g = src[1];	r = src[2];
				src += 3;
				break;
			case IL_LUMINANCE:
				r = src[0];	g = src[0];	b = src[0];
				src += 1;
				break;
			case IL_LUMINANCE_ALPHA:
				r = src[0];	g = src[0];	b = src[0];	a = src[1];
				src += 2;
				break;
			case IL_RGBA:
				r = src[0];	g = src[1];	b = src[2];	a = src[3];
				src += 4;
				break;
			case IL_BGRA:
				b = src[0];	g = src[1];	r = src[2];	a = src[3];
				src += 4;
				break;
			default:
				return;
            }
            packI(r, g, b, a, ogrefmt, tar);
            tar += elemSize;
        }

    }
#endif


        /// <summary>
        ///    Converts a PixelFormat enum to a pair with DevIL format enum and bytesPerPixel.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static ILFormat ConvertToILFormat(PixelFormat format) {
            switch (format) {
                case PixelFormat.BYTE_L:
                    return new ILFormat(1, Il.IL_LUMINANCE, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.BYTE_A:
                    return new ILFormat(1, Il.IL_LUMINANCE, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.SHORT_L:
                    return new ILFormat(1, Il.IL_LUMINANCE, Il.IL_UNSIGNED_SHORT);
                case PixelFormat.BYTE_LA:
                    return new ILFormat(2, Il.IL_LUMINANCE_ALPHA, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.BYTE_RGB:
                    return new ILFormat(3, Il.IL_RGB, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.BYTE_RGBA:
                    return new ILFormat(4, Il.IL_RGBA, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.BYTE_BGR:
                    return new ILFormat(3, Il.IL_BGR, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.BYTE_BGRA:
                    return new ILFormat(4, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);
                case PixelFormat.SHORT_RGBA:
                    return new ILFormat(4, Il.IL_RGBA, Il.IL_UNSIGNED_SHORT);
                case PixelFormat.FLOAT32_RGB:
                    return new ILFormat(3, Il.IL_RGB, Il.IL_FLOAT);
                case PixelFormat.FLOAT32_RGBA:
                    return new ILFormat(4, Il.IL_RGBA, Il.IL_FLOAT);
                case PixelFormat.DXT1:
                    return new ILFormat(0, Il.IL_DXT1);
                case PixelFormat.DXT2:
                    return new ILFormat(0, Il.IL_DXT2);
                case PixelFormat.DXT3:
                    return new ILFormat(0, Il.IL_DXT3);
                case PixelFormat.DXT4:
                    return new ILFormat(0, Il.IL_DXT4);
                case PixelFormat.DXT5:
                    return new ILFormat(0, Il.IL_DXT5);
            }

            return new ILFormat();
        }

        /// <summary>
        ///    Converts a DevIL format enum to a PixelFormat enum.
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <param name="imageType"></param>
        /// <returns></returns>
        public static PixelFormat ConvertFromILFormat(int imageFormat, int imageType) {
            PixelFormat fmt = PixelFormat.Unknown;
            switch (imageFormat) {
                /* Compressed formats -- ignore type */
                case Il.IL_DXT1: fmt = PixelFormat.DXT1; break;
                case Il.IL_DXT2: fmt = PixelFormat.DXT2; break;
                case Il.IL_DXT3: fmt = PixelFormat.DXT3; break;
                case Il.IL_DXT4: fmt = PixelFormat.DXT4; break;
                case Il.IL_DXT5: fmt = PixelFormat.DXT5; break;
                /* Normal formats */
                case Il.IL_RGB:
                    switch (imageType) {
                        case Il.IL_FLOAT: fmt = PixelFormat.FLOAT32_RGB; break;
                        case Il.IL_UNSIGNED_SHORT:
                        case Il.IL_SHORT: fmt = PixelFormat.SHORT_RGBA; break;
                        default: fmt = PixelFormat.BYTE_RGB; break;
                    }
                    break;
                case Il.IL_BGR:
                    switch (imageType) {
                        case Il.IL_FLOAT: fmt = PixelFormat.FLOAT32_RGB; break;
                        case Il.IL_UNSIGNED_SHORT:
                        case Il.IL_SHORT: fmt = PixelFormat.SHORT_RGBA; break;
                        default: fmt = PixelFormat.BYTE_BGR; break;
                    }
                    break;
                case Il.IL_RGBA:
                    switch (imageType) {
                        case Il.IL_FLOAT: fmt = PixelFormat.FLOAT32_RGBA; break;
                        case Il.IL_UNSIGNED_SHORT:
                        case Il.IL_SHORT: fmt = PixelFormat.SHORT_RGBA; break;
                        default: fmt = PixelFormat.BYTE_RGBA; break;
                    }
                    break;
                case Il.IL_BGRA:
                    switch (imageType) {
                        case Il.IL_FLOAT: fmt = PixelFormat.FLOAT32_RGBA; break;
                        case Il.IL_UNSIGNED_SHORT:
                        case Il.IL_SHORT: fmt = PixelFormat.SHORT_RGBA; break;
                        default: fmt = PixelFormat.BYTE_BGRA; break;
                    }
                    break;
                case Il.IL_LUMINANCE:
                    switch (imageType) {
                        case Il.IL_BYTE:
                        case Il.IL_UNSIGNED_BYTE:
                            fmt = PixelFormat.L8;
                            break;
                        default:
                            fmt = PixelFormat.L16;
                            break;
                    }
                    break;
                case Il.IL_LUMINANCE_ALPHA:
                    fmt = PixelFormat.BYTE_LA;
                    break;
            }
            return fmt;
        }

        //----------------------------------------------------------------------- 
	    /// Utility function to convert IL data types to UNSIGNED_
	    public static int ILabs(int type) {
		    switch (type) {
		        case Il.IL_INT: return Il.IL_UNSIGNED_INT;
		        case Il.IL_BYTE: return Il.IL_UNSIGNED_BYTE;
		        case Il.IL_SHORT: return Il.IL_UNSIGNED_SHORT;
		        default: return type;
		    }
	    }
    
        //-----------------------------------------------------------------------
        public static void ToAxiom(PixelBox dst) 
        {
		    if (!dst.Consecutive)
                throw new NotImplementedException("Destination must currently be consecutive");
    		if (dst.Width != Il.ilGetInteger(Il.IL_IMAGE_WIDTH) ||
        	    dst.Height != Il.ilGetInteger(Il.IL_IMAGE_HEIGHT) ||
        	    dst.Depth != Il.ilGetInteger(Il.IL_IMAGE_DEPTH))
			    throw new AxiomException("Destination dimensions must equal IL dimension");
        
            int ilfmt = Il.ilGetInteger(Il.IL_IMAGE_FORMAT);
            int iltp = Il.ilGetInteger(Il.IL_IMAGE_TYPE);

		    // Check if in-memory format just matches
		    // If yes, we can just copy it and save conversion
		    ILFormat ifmt = ILUtil.ConvertToILFormat(dst.Format);
            if (ifmt.format == ilfmt && ILabs(ifmt.type) == ILabs(iltp)) {
                int size = Il.ilGetInteger(Il.IL_IMAGE_SIZE_OF_DATA);
                // Copy from the IL structure to our buffer
                PixelUtil.CopyBytes(dst.Data, dst.Offset, Il.ilGetData(), 0, size);
                return;
            }
		    // Try if buffer is in a known OGRE format so we can use OGRE its
		    // conversion routines
		    PixelFormat bufFmt = ILUtil.ConvertFromILFormat(ilfmt, iltp);
		
		    ifmt = ILUtil.ConvertToILFormat(bufFmt);

		    if (ifmt.format == ilfmt && ILabs(ifmt.type) == ILabs(iltp))
		    {
    			// IL format matches another OGRE format
	    		PixelBox src = new PixelBox(dst.Width, dst.Height, dst.Depth, bufFmt, Il.ilGetData());
		    	PixelUtil.BulkPixelConversion(src, dst);
			    return;
            }

#if NOT
            // The extremely slow method
            if (iltp == Il.IL_UNSIGNED_BYTE || iltp == Il.IL_BYTE) 
            {
                ilToOgreInternal(static_cast<uint8*>(dst.data), dst.format, (uint8)0x00,(uint8)0x00,(uint8)0x00,(uint8)0xFF);
            } 
            else if(iltp == IL_FLOAT)
            {
                ilToOgreInternal(static_cast<uint8*>(dst.data), dst.format, 0.0f,0.0f,0.0f,1.0f);          
            }
		    else if(iltp == IL_SHORT || iltp == IL_UNSIGNED_SHORT)
            {
    			ilToOgreInternal(static_cast<uint8*>(dst.data), dst.format, 
	    			(uint16)0x0000,(uint16)0x0000,(uint16)0x0000,(uint16)0xFFFF); 
            }
            else 
            {
                OGRE_EXCEPT( Exception::UNIMPLEMENTED_FEATURE,
                    "Cannot convert this DevIL type",
                    "ILUtil::ilToOgre" ) ;
            }
#else
            throw new NotImplementedException("Cannot convert this DevIL type");
#endif
        }
#if NOT        
        //-----------------------------------------------------------------------
        public void ILUtil.FromAxiom(PixelBox src)
        {
		// ilTexImage http://openil.sourceforge.net/docs/il/f00059.htm
		ILFormat ifmt = OgreFormat2ilFormat( src.format );
		if(src.isConsecutive() && ifmt.isValid()) 
		{
			// The easy case, the buffer is laid out in memory just like 
			// we want it to be and is in a format DevIL can understand directly
			// We could even save the copy if DevIL would let us
			Il.ilTexImage(src.Width, src.Height, src.Depth, ifmt.numberOfChannels,
				          ifmt.format, ifmt.type, src.data);
		} 
		else if(ifmt.isValid()) 
		{
			// The format can be understood directly by DevIL. The only 
			// problem is that ilTexImage expects our image data consecutively 
			// so we cannot use that directly.
			
			// Let DevIL allocate the memory for us, and copy the data consecutively
			// to its memory
			ilTexImage(static_cast<ILuint>(src.getWidth()), 
				static_cast<ILuint>(src.getHeight()), 
				static_cast<ILuint>(src.getDepth()), ifmt.numberOfChannels,
				ifmt.format, ifmt.type, 0);
			PixelBox dst(src.getWidth(), src.getHeight(), src.getDepth(), src.format, ilGetData());
			PixelUtil::bulkPixelConversion(src, dst);
		} 
		else 
		{
			// Here it gets ugly. We're stuck with a pixel format that DevIL
			// can't do anything with. We will do a bulk pixel conversion and
			// then feed it to DevIL anyway. The problem is finding the best
			// format to convert to.
			
			// most general format supported by OGRE and DevIL
			PixelFormat fmt = PixelUtil::hasAlpha(src.format)?PF_FLOAT32_RGBA:PF_FLOAT32_RGB; 

			// Make up a pixel format
			// We don't have to consider luminance formats as they have
			// straight conversions to DevIL, just weird permutations of RGBA an LA
			int depths[4];
			PixelUtil::getBitDepths(src.format, depths);
			
			// Native endian format with all bit depths<8 can safely and quickly be 
			// converted to 24/32 bit
			if(PixelUtil::isNativeEndian(src.format) && 
				depths[0]<=8 && depths[1]<=8 && depths[2]<=8 && depths[3]<=8) {
				if(PixelUtil::hasAlpha(src.format)) {
					fmt = PF_A8R8G8B8;
				} else {
					fmt = PF_R8G8B8;
				}
			}
			
			// Let DevIL allocate the memory for us, then do the conversion ourselves
			ifmt = OgreFormat2ilFormat( fmt );
			ilTexImage(static_cast<ILuint>(src.getWidth()), 
				static_cast<ILuint>(src.getHeight()), 
				static_cast<ILuint>(src.getDepth()), ifmt.numberOfChannels,
				ifmt.format, ifmt.type, 0);
			PixelBox dst(src.getWidth(), src.getHeight(), src.getDepth(), fmt, ilGetData());
			PixelUtil::bulkPixelConversion(src, dst);
		}
    }

#endif

    }
}
