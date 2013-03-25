// #define OGRE_ENDIAN_BIG
using System;

namespace Axiom.Media {
    /// <summary>
    ///    Various flags that give details on a particular image.
    /// </summary>
    [Flags]
    public enum ImageFlags {
        Compressed = 0x00000001,
        CubeMap = 0x00000002,
        Volume = 0x00000004
    }

    /// <summary>
    ///    The pixel format used for images.
    /// </summary>
    public enum PixelFormat {
        /// <summary>
        ///    Unknown pixel format.
        /// </summary>
        Unknown = 0,
        /// <summary>
        ///    8-bit pixel format, all bits luminance.
        /// </summary>
        L8 = 1,
        BYTE_L = L8,
        /// <summary>
        ///    16-bit pixel format, all bits luminance.
        /// </summary>
        L16,
        SHORT_L = L16,
        /// <summary>
        ///    8-bit pixel format, all bits alpha.
        /// </summary>
        A8 = 3,
        BYTE_A = A8,
        /// <summary>
        ///    8-bit pixel format, 4 bits alpha, 4 bits luminance.
        /// </summary>
        A4L4 = 4,
        /// <summary>
        ///    16-bit pixel format, 8 bits for luminance, 8 bits for alpha.
        /// </summary>
        BYTE_LA = 5,
        /// <summary>
        ///    16-bit pixel format, 5 bits red, 6 bits green, 5 bits blue.
        /// </summary>
        R5G6B5 = 6,
        /// <summary>
        ///    16-bit pixel format, 5 bits blue, 6 bits green, 5 bits red.
        /// </summary>
        B5G6R5 = 7,
        /// <summary>
        ///   8-bit pixel format, 3 bits red, 3 bits green, 2 bits blue.
        /// </summary>
        R3G3B2 = 31,
        /// <summary>
        ///    16-bit pixel format, 4 bits for alpha, red, green and blue.
        /// </summary>
        A4R4G4B4 = 8,
        /// <summary>
        ///    16-bit pixel format, 1 bit for alpha, 5 bits for blue, green and red.
        /// </summary>
        A1R5G5B5 = 9,
        /// <summary>
        ///    24-bit pixel format, 8 bits for red, green and blue.
        /// </summary>
        R8G8B8 = 10,
        /// <summary>
        ///    24-bit pixel format, 8 bits for blue, green and red.
        /// </summary>
        B8G8R8 = 11,
        /// <summary>
        ///    32-bit pixel format, 8 bits for alpha, red, green and blue.
        /// </summary>
        A8R8G8B8 = 12,
        /// <summary>
        ///    32-bit pixel format, 8 bits for alpha, blue, green and red`.
        /// </summary>
        A8B8G8R8 = 13,
        /// <summary>
        ///    32-bit pixel format, 8 bits for blue, green, red and alpha.
        /// </summary>
        B8G8R8A8 = 14,
        /// <summary>
        ///    32-bit pixel format, 8 bits for red, green, blue and alpha.
        /// </summary>
        R8G8B8A8 = 28,
        /// <summary>
        ///    32-bit pixel format, 8 bits for red, green and blue.
        /// </summary>
        X8R8G8B8 = 26,
        /// <summary>
        ///    32-bit pixel format, 8 bits for blue, green and red.
        /// </summary>
        X8B8G8R8 = 27,
#if OGRE_ENDIAN_BIG
        /// 3 byte pixel format, 1 byte for red, 1 byte for green, 1 byte for blue
        BYTE_RGB = R8G8B8,
        /// 3 byte pixel format, 1 byte for blue, 1 byte for green, 1 byte for red
        BYTE_BGR = B8G8R8,
        /// 4 byte pixel format, 1 byte for blue, 1 byte for green, 1 byte for red and one byte for alpha
        BYTE_BGRA = B8G8R8A8,
        /// 4 byte pixel format, 1 byte for red, 1 byte for green, 1 byte for blue, and one byte for alpha
        BYTE_RGBA = R8G8B8A8,
#else
		/// 3 byte pixel format, 1 byte for red, 1 byte for green, 1 byte for blue
		BYTE_RGB = B8G8R8,
		/// 3 byte pixel format, 1 byte for blue, 1 byte for green, 1 byte for red
		BYTE_BGR = R8G8B8,
		/// 4 byte pixel format, 1 byte for blue, 1 byte for green, 1 byte for red and one byte for alpha
		BYTE_BGRA = A8R8G8B8,
		/// 4 byte pixel format, 1 byte for red, 1 byte for green, 1 byte for blue, and one byte for alpha
		BYTE_RGBA = A8B8G8R8,
#endif
        /// <summary>
        ///    32-bit pixel format, 2 bits for alpha, 10 bits for red, green and blue.
        /// </summary>
        A2R10G10B10 = 15,
        /// <summary>
        ///    32-bit pixel format, 10 bits for blue, green and red, 2 bits for alpha.
        /// </summary>
        A2B10G10R10 = 16,
        /// <summary>
        ///    DDS (DirectDraw Surface) DXT1 format.
        /// </summary>
        DXT1 = 17,
        /// <summary>
        ///    DDS (DirectDraw Surface) DXT2 format.
        /// </summary>
        DXT2 = 18,
        /// <summary>
        ///    DDS (DirectDraw Surface) DXT3 format.
        /// </summary>
        DXT3 = 19,
        /// <summary>
        ///    DDS (DirectDraw Surface) DXT4 format.
        /// </summary>
        DXT4 = 20,
        /// <summary>
        ///    DDS (DirectDraw Surface) DXT5 format.
        /// </summary>
        DXT5 = 21, 
        /// <summary>
        ///   64 bit pixel format, 16 bits for red, 16 bits for green, 16 bits for blue, 16 bits for alpha
        /// </summary>
        SHORT_RGBA = 30,
        /// <summary>
        ///    16 bit floating point with a single channel (red)
        /// </summary>
        FLOAT16_R = 32,
        /// <summary>
        ///    48-bit pixel format, 16 bits (float) for red, 16 bits (float) for green, 16 bits (float) for blue
        /// </summary>
        FLOAT16_RGB = 22,
        /// <summary>
        ///    64-bit pixel format, 16 bits (float) for red, 16 bits (float) for green, 16 bits (float) for blue, 16 bits (float) for alpha
        /// </summary>
        FLOAT16_RGBA = 23,
        /// <summary>
        ///    32 bit floating point with a single channel (red)
        /// </summary>
        FLOAT32_R = 33,
        /// <summary>
        ///    96-bit pixel format, 32 bits (float) for red, 32 bits (float) for green, 32 bits (float) for blue
        /// </summary>
        FLOAT32_RGB = 24,
        /// <summary>
        ///    128-bit pixel format, 32 bits (float) for red, 32 bits (float) for green, 32 bits (float) for blue, 32 bits (float) for alpha
        /// </summary>
        FLOAT32_RGBA = 25,
        /// <summary>
        ///    8-bit pixel format, 4 bits luminance, 4 bits alpha.
        /// </summary>
        // L4A4,
        /// <summary>
        ///    16-bit pixel format, 8 bits alpha, 8 bits luminance.
        /// </summary>
        // A8L8,
        /// <summary>
        ///    16-bit pixel format, 4 bits for blue, green, red and alpha.
        /// </summary>
        // B4G4R4A4,
        /// <summary>
        ///    24-bit pixel format, all bits luminance.
        /// </summary>
        // L24,
        /// <summary>
        ///    The last one, used to size arrays of PixelFormat.  Don't add anything after this one!
        /// </summary>
		Count = 34
	}

    /// <summary>
    ///    Flags defining some on/off properties of pixel formats
    /// </summary>
    public enum PixelFormatFlags {
        // No flags
		None            = 0x00000000,
		// This format has an alpha channel
        HasAlpha        = 0x00000001,      
        // This format is compressed. This invalidates the values in elemBytes,
        // elemBits and the bit counts as these might not be fixed in a compressed format.
        Compressed    = 0x00000002,
        // This is a floating point format
        Float           = 0x00000004,         
        // This is a depth format (for depth textures)
        Depth           = 0x00000008,
        // Format is in native endian. Generally true for the 16, 24 and 32 bits
        // formats which can be represented as machine integers.
        NativeEndian    = 0x00000010,
        // This is an intensity format instead of a RGB one. The luminance
        // replaces R,G and B. (but not A)
        Luminance       = 0x00000020
    }

    /// <summary>
    ///    Pixel component format */
    /// </summary>
    public enum PixelComponentType
    {
        Byte = 0,    /// Byte per component (8 bit fixed 0.0..1.0)
        Short = 1,   /// Short per component (16 bit fixed 0.0..1.0))
        Float16 = 2, /// 16 bit float per component
        Float32 = 3, /// 32 bit float per component
        Count = 4    /// Number of pixel types
    }
}
