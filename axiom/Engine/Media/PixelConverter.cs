using System;
using System.IO;
using System.Diagnostics;
using Axiom.Core;

namespace Axiom.Media {

    ///<summary>
    ///    A class to convert/copy pixels of the same or different formats
    ///</summary>
    public class PixelFormatDescription {

        #region Fields

        // Name of the format, as in the enum
        public string name;
        // The pixel format
        public PixelFormat format;
        // Number of bytes one element (color value) takes.
        public byte elemBytes;
        // Pixel format flags, see enum PixelFormatFlags for the bit field
        // definitions 
        public PixelFormatFlags flags;
        // Component type 
        public PixelComponentType componentType;
        // Component count
        public byte componentCount;
        // Number of bits for red(or luminance), green, blue, alpha
        public byte rbits, gbits, bbits, abits; /*, ibits, dbits, ... */
        // Masks and shifts as used by packers/unpackers */
        public uint rmask, gmask, bmask, amask;
        public byte rshift, gshift, bshift, ashift;

        #endregion Fields

        #region Constructor

        public PixelFormatDescription(string name,
                                      PixelFormat format,
                                      byte elemBytes,
                                      PixelFormatFlags flags,
                                      PixelComponentType componentType,
                                      byte componentCount,
                                      byte rbits,
                                      byte gbits,
                                      byte bbits,
                                      byte abits,
                                      uint rmask,
                                      uint gmask,
                                      uint bmask,
                                      uint amask,
                                      byte rshift,
                                      byte gshift,
                                      byte bshift,
                                      byte ashift) {
            this.name = name;
            this.format = format;
            this.elemBytes = elemBytes;
            this.flags = flags;
            this.componentType = componentType;
            this.componentCount = componentCount;
            this.rbits = rbits;
            this.gbits = gbits;
            this.bbits = bbits;
            this.abits = abits;
            this.rmask = rmask;
            this.gmask = gmask;
            this.bmask = bmask;
            this.amask = amask;
            this.rshift = rshift;
            this.gshift = gshift;
            this.bshift = bshift;
        }
        #endregion Constructor
    }


	///<summary>
	///    A class to convert/copy pixels of the same or different formats
	///</summary>
	public class PixelConverter {
		///<summary>
		///    Pixel format database
		///</summary>
        protected static PixelFormatDescription[] UnindexedPixelFormats = new PixelFormatDescription[] {
			new PixelFormatDescription(
			    "PF_UNKNOWN", 
				PixelFormat.Unknown,
				/* Bytes per element */ 
				0,  
				/* Flags */
				PixelFormatFlags.None,  
				/* Component type and count */
				PixelComponentType.Byte, 0,
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
  				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_L8",
				PixelFormat.L8,
				/* Bytes per element */ 
				1,  
				/* Flags */
				PixelFormatFlags.Luminance | PixelFormatFlags.NativeEndian,
				/* Component type and count */
				PixelComponentType.Byte, 1,
				/* rbits, gbits, bbits, abits */
				8, 0, 0, 0,
				/* Masks and shifts */
				0xFF, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_L16",
				PixelFormat.L16,
				/* Bytes per element */ 
				2,  
				/* Flags */
				PixelFormatFlags.Luminance | PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Short, 1,
				/* rbits, gbits, bbits, abits */
				16, 0, 0, 0,
				/* Masks and shifts */
				0xFFFF, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_A8",
				PixelFormat.A8,
				/* Bytes per element */ 
				1,  
				/* Flags */
				PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,
				/* Component type and count */
				PixelComponentType.Byte, 1,
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 8,
				/* Masks and shifts */
				0, 0, 0, 0xFF, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_A4L4",
				PixelFormat.A4L4,
				/* Bytes per element */ 
				1,  
				/* Flags */
				PixelFormatFlags.HasAlpha | PixelFormatFlags.Luminance | PixelFormatFlags.NativeEndian,
				/* Component type and count */
				PixelComponentType.Byte, 2,
				/* rbits, gbits, bbits, abits */
				4, 0, 0, 4,
				/* Masks and shifts */
				0x0F, 0, 0, 0xF0, 0, 0, 0, 4
				),
			//-----------------------------------------------------------------------
//  		new PixelFormatDescription(
//             "PF_BYTE_LA",
//             PixelFormat.BYTE_LA,
//  			/* Bytes per element */ 
//  			2,  
//  			/* Flags */
//  			PixelFormatFlags.HasAlpha | PixelFormatFlags.Luminance,  
//  			/* Component type and count */
//  			PixelComponentType.Byte, 2,
//  			/* rbits, gbits, bbits, abits */
//  			8, 0, 0, 8,
//  			/* Masks and shifts */
//  			0,0,0,0,0,0,0,0
//  			),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_R5G6B5",
				PixelFormat.R5G6B5,
				/* Bytes per element */ 
				2,  
				/* Flags */
				PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 3,
				/* rbits, gbits, bbits, abits */
				5, 6, 5, 0,
				/* Masks and shifts */
				0xF800, 0x07E0, 0x001F, 0, 
				11, 5, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_B5G6R5",
				PixelFormat.B5G6R5,
				/* Bytes per element */ 
				2,  
				/* Flags */
				PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 3,
				/* rbits, gbits, bbits, abits */
				5, 6, 5, 0,
				/* Masks and shifts */
				0x001F, 0x07E0, 0xF800, 0, 
				0, 5, 11, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_A4R4G4B4",
				PixelFormat.A4R4G4B4,
				/* Bytes per element */ 
				2,  
				/* Flags */
				PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				4, 4, 4, 4,
				/* Masks and shifts */
				0x0F00, 0x00F0, 0x000F, 0xF000, 
				8, 4, 0, 12 
				),
			//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_A1R5G5B5",
             PixelFormat.A1R5G5B5,
  			/* Bytes per element */ 
  			2,  
  			/* Flags */
  			PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 4,
  			/* rbits, gbits, bbits, abits */
  			5, 5, 5, 1,
  			/* Masks and shifts */
  			0x7C00, 0x03E0, 0x001F, 0x8000, 
  			10, 5, 0, 15
  			),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_R8G8B8",
				PixelFormat.R8G8B8,
				/* Bytes per element */ 
				3,  // 24 bit integer -- special
				/* Flags */
				PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 3,
				/* rbits, gbits, bbits, abits */
				8, 8, 8, 0,
				/* Masks and shifts */
				0xFF0000, 0x00FF00, 0x0000FF, 0, 
				16, 8, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_B8G8R8",
				PixelFormat.B8G8R8,
				/* Bytes per element */ 
				3,  // 24 bit integer -- special
				/* Flags */
				PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 3,
				/* rbits, gbits, bbits, abits */
				8, 8, 8, 0,
				/* Masks and shifts */
				0x0000FF, 0x00FF00, 0xFF0000, 0, 
				0, 8, 16, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_A8R8G8B8",
				PixelFormat.A8R8G8B8,
				/* Bytes per element */ 
				4,  
				/* Flags */
				PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				8, 8, 8, 8,
				/* Masks and shifts */
				0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000,
				16, 8, 0, 24
				),
			//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_A8B8G8R8",
             PixelFormat.A8B8G8R8,
  			/* Bytes per element */ 
  			4,  
  			/* Flags */
  			PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 4,
  			/* rbits, gbits, bbits, abits */
  			8, 8, 8, 8,
  			/* Masks and shifts */
  			0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000,
  			0, 8, 16, 24
  			),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_B8G8R8A8",
				PixelFormat.B8G8R8A8,
				/* Bytes per element */ 
				4,  
				/* Flags */
				PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				8, 8, 8, 8,
				/* Masks and shifts */
				0x0000FF00, 0x00FF0000, 0xFF000000, 0x000000FF,
				8, 16, 24, 0
				),
			//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_A2R10G10B10",
             PixelFormat.A2R10G10B10,
  			/* Bytes per element */ 
  			4,  
  			/* Flags */
  			PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 4,
  			/* rbits, gbits, bbits, abits */
  			10, 10, 10, 2,
  			/* Masks and shifts */
  			0x3FF00000, 0x000FFC00, 0x000003FF, 0xC0000000,
  			20, 10, 0, 30
  			),
 		    //-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_A2B10G10R10",
             PixelFormat.A2B10G10R10,
  			/* Bytes per element */ 
  			4,  
  			/* Flags */
  			PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 4,
  			/* rbits, gbits, bbits, abits */
  			10, 10, 10, 2,
  			/* Masks and shifts */
  			0x000003FF, 0x000FFC00, 0x3FF00000, 0xC0000000,
  			0, 10, 20, 30
  			),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_DXT1",
				PixelFormat.DXT1,
				/* Bytes per element */ 
				0,  
				/* Flags */
				PixelFormatFlags.Compressed | PixelFormatFlags.HasAlpha,  
				/* Component type and count */
				PixelComponentType.Byte, 3, // No alpha
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_DXT2",
				PixelFormat.DXT2,
				/* Bytes per element */ 
				0,  
				/* Flags */
				PixelFormatFlags.Compressed | PixelFormatFlags.HasAlpha,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_DXT3",
				PixelFormat.DXT3,
				/* Bytes per element */ 
				0,  
				/* Flags */
				PixelFormatFlags.Compressed | PixelFormatFlags.HasAlpha,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_DXT4",
				PixelFormat.DXT4,
				/* Bytes per element */ 
				0,  
				/* Flags */
				PixelFormatFlags.Compressed | PixelFormatFlags.HasAlpha,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_DXT5",
				PixelFormat.DXT5,
				/* Bytes per element */ 
				0,  
				/* Flags */
				PixelFormatFlags.Compressed | PixelFormatFlags.HasAlpha,  
				/* Component type and count */
				PixelComponentType.Byte, 4,
				/* rbits, gbits, bbits, abits */
				0, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_FLOAT16_RGB",
				PixelFormat.FLOAT16_RGB,
				/* Bytes per element */ 
				6,  
				/* Flags */
				PixelFormatFlags.Float,  
				/* Component type and count */
				PixelComponentType.Float16, 3,
				/* rbits, gbits, bbits, abits */
				16, 16, 16, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_FLOAT16_RGBA",
				PixelFormat.FLOAT16_RGBA,
				/* Bytes per element */ 
				8,  
				/* Flags */
				PixelFormatFlags.Float,  
				/* Component type and count */
				PixelComponentType.Float16, 4,
				/* rbits, gbits, bbits, abits */
				16, 16, 16, 16,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_FLOAT32_RGB",
				PixelFormat.FLOAT32_RGB,
				/* Bytes per element */ 
				12,  
				/* Flags */
				PixelFormatFlags.Float,  
				/* Component type and count */
				PixelComponentType.Float32, 3,
				/* rbits, gbits, bbits, abits */
				32, 32, 32, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_FLOAT32_RGBA",
				PixelFormat.FLOAT32_RGBA,
				/* Bytes per element */ 
				16,  
				/* Flags */
				PixelFormatFlags.Float,  
				/* Component type and count */
				PixelComponentType.Float32, 4,
				/* rbits, gbits, bbits, abits */
				32, 32, 32, 32,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_X8R8G8B8",
             PixelFormat.X8R8G8B8,
  			/* Bytes per element */ 
  			4,  
  			/* Flags */
  			PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 3,
  			/* rbits, gbits, bbits, abits */
  			8, 8, 8, 0,
  			/* Masks and shifts */
  			0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000,
  			16, 8, 0, 24
  			),
  		//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_X8B8G8R8",
             PixelFormat.X8B8G8R8,
  			/* Bytes per element */ 
  			4,  
  			/* Flags */
  			PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 3,
  			/* rbits, gbits, bbits, abits */
  			8, 8, 8, 0,
  			/* Masks and shifts */
  			0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000,
  			0, 8, 16, 24
  			),
 		//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_R8G8B8A8",
             PixelFormat.R8G8B8A8,
  			/* Bytes per element */ 
  			4,  
  			/* Flags */
  			PixelFormatFlags.HasAlpha | PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 4,
  			/* rbits, gbits, bbits, abits */
  			8, 8, 8, 8,
  			/* Masks and shifts */
  			0xFF000000, 0x00FF0000, 0x0000FF00, 0x000000FF,
  			24, 16, 8, 0
  			),
 		//-----------------------------------------------------------------------
//  		new PixelFormatDescription(
//             "PF_DEPTH",
//             PixelFormat.DEPTH,
//  			/* Bytes per element */ 
//  			4,  
//  			/* Flags */
//  			PixelFormatFlags.Depth, 
//  			/* Component type and count */
//  			PixelComponentType.Float32, 1, // ?
//  			/* rbits, gbits, bbits, abits */
//  			0, 0, 0, 0,
//  			/* Masks and shifts */
//  			0, 0, 0, 0, 0, 0, 0, 0
//  			),
 		//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_SHORT_RGBA",
             PixelFormat.SHORT_RGBA,
  			/* Bytes per element */ 
  			8,  
  			/* Flags */
  			PixelFormatFlags.HasAlpha,  
  			/* Component type and count */
  			PixelComponentType.Short, 4,
  			/* rbits, gbits, bbits, abits */
  			16, 16, 16, 16,
  			/* Masks and shifts */
  			0, 0, 0, 0, 0, 0, 0, 0
  			),
 		//-----------------------------------------------------------------------
  		new PixelFormatDescription(
             "PF_R3G3B2",
             PixelFormat.R3G3B2,
  			/* Bytes per element */ 
  			1,  
  			/* Flags */
  			PixelFormatFlags.NativeEndian,  
  			/* Component type and count */
  			PixelComponentType.Byte, 3,
  			/* rbits, gbits, bbits, abits */
  			3, 3, 2, 0,
  			/* Masks and shifts */
  			0xE0, 0x1C, 0x03, 0, 
  			5, 2, 0, 0 
  			),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_FLOAT16_R",
				PixelFormat.FLOAT16_R,
				/* Bytes per element */ 
				2,  
				/* Flags */
				PixelFormatFlags.Float,  
				/* Component type and count */
				PixelComponentType.Float16, 1,
				/* rbits, gbits, bbits, abits */
				16, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
				),
			//-----------------------------------------------------------------------
			new PixelFormatDescription(
				"PF_FLOAT32_R",
				PixelFormat.FLOAT32_R,
				/* Bytes per element */ 
				4,  
				/* Flags */
				PixelFormatFlags.Float,  
				/* Component type and count */
				PixelComponentType.Float32, 1,
				/* rbits, gbits, bbits, abits */
				32, 0, 0, 0,
				/* Masks and shifts */
				0, 0, 0, 0, 0, 0, 0, 0 
 			    )
 		};

        protected static PixelFormatDescription[] IndexedPixelFormats = null;
		
		public static void Initialize() {
			if (IndexedPixelFormats != null)
				return;
			IndexedPixelFormats = new PixelFormatDescription[(int)PixelFormat.Count];
			foreach (PixelFormatDescription d in UnindexedPixelFormats) {
				IndexedPixelFormats[(int)d.format] = d;
			}
		}
		
		public static PixelFormatDescription GetDescriptionFor(PixelFormat format) {
            lock (UnindexedPixelFormats) {
                Initialize();
            }
			return IndexedPixelFormats[(int)format];
		}


    }

    public class PixelUtil {
        /// <summary>
        ///    Returns the size in bytes of an element of the given pixel format.
        /// </summary>
        /// <param name="format">Pixel format to test.</param>
        /// <returns>Size in bytes.</returns>
        public static int GetNumElemBytes(PixelFormat format) {
            return PixelConverter.GetDescriptionFor(format).elemBytes;
        }

        /// <summary>
        ///    Returns the size in bits of an element of the given pixel format.
        /// </summary>
        /// <param name="format">Pixel format to test.</param>
        /// <returns>Size in bits.</returns>
        public static int GetNumElemBits(PixelFormat format) {
            return GetNumElemBytes(format) * 8;
        }

        ///<summary>
        ///    Returns the size in memory of a region with the given extents and pixel
        ///    format with consecutive memory layout.
        ///</summary>
        ///<param name="width">Width of the area</param>
        ///<param name="height">Height of the area</param>
        ///<param name="depth">Depth of the area</param>
        ///<param name="format">Format of the area</param>
        ///<returns>The size in bytes</returns>
        ///<remarks>
        ///    In case that the format is non-compressed, this simply returns
        ///    width * height * depth * PixelConverter.GetNumElemBytes(format). In the compressed
        ///    case, this does serious magic.
        ///</remarks>
        public static int GetMemorySize(int width, int height, int depth, PixelFormat format) {
            if (IsCompressed(format)) {
                switch (format) {
                    case PixelFormat.DXT1:
                        Debug.Assert(depth == 1);
                        return ((width + 3) / 4) * ((height + 3) / 4) * 8;
                    case PixelFormat.DXT2:
                    case PixelFormat.DXT3:
                    case PixelFormat.DXT4:
                    case PixelFormat.DXT5:
                        Debug.Assert(depth == 1);
                        return ((width + 3) / 4) * ((height + 3) / 4) * 16;
                    default:
                        throw new Exception("Invalid compressed pixel format");
                }
            } else {
                return width * height * depth * GetNumElemBytes(format);
            }
        }

        public static bool IsCompressed(PixelFormat format) {
            return (PixelConverter.GetDescriptionFor(format).flags & PixelFormatFlags.Compressed) > 0;
        }

        public static bool HasAlpha(PixelFormat format) {
            return (PixelConverter.GetDescriptionFor(format).flags & PixelFormatFlags.HasAlpha) > 0;
        }

        public static string GetFormatName(PixelFormat format) {
            return PixelConverter.GetDescriptionFor(format).name;
        }

        ///<summary>
        ///    Convert consecutive pixels from one format to another. No dithering or filtering is being done. 
        ///    Converting from RGB to luminance takes the R channel.  In case the source and destination format match,
        ///    just a copy is done.
        ///</summary>
        ///<param name="srcBytes">Pointer to source region</param>
        ///<param name="srcFormat">Pixel format of source region</param>
        ///<param name="dstBytes">Pointer to destination region</param>
        ///<param name="dstFormat">Pixel format of destination region</param>
        public static void BulkPixelConversion(IntPtr srcBytes, int srcOffset, PixelFormat srcFormat,
                                               IntPtr dstBytes, int dstOffset, PixelFormat dstFormat,
                                               int count) {
            PixelBox src = new PixelBox(count, 1, 1, srcFormat, srcBytes);
            src.Offset = srcOffset;
            PixelBox dst = new PixelBox(count, 1, 1, dstFormat, dstBytes);
            dst.Offset = dstOffset;
            BulkPixelConversion(src, dst);
        }

        ///<summary>
        ///    Convert pixels from one format to another. No dithering or filtering is being done. Converting
        ///    from RGB to luminance takes the R channel. 
        ///</summary>
        ///<param name="src">PixelBox containing the source pixels, pitches and format</param>
        ///<param name="dst">PixelBox containing the destination pixels, pitches and format</param>
        ///<remarks>
        ///    The source and destination boxes must have the same
        ///    dimensions. In case the source and destination format match, a plain copy is done.
        ///</remarks>
        public static void BulkPixelConversion(PixelBox src, PixelBox dst) {
            Debug.Assert(src.Width == dst.Width && src.Height == dst.Height && src.Depth == dst.Depth);

            // Check for compressed formats, we don't support decompression, compression or recoding
            if (PixelBox.Compressed(src.Format) || PixelBox.Compressed(dst.Format)) {
                if (src.Format == dst.Format) {
                    CopyBytes(dst.Data, dst.Offset, src.Data, src.Offset, src.ConsecutiveSize);
                    return;
                } else
                    throw new Exception("This method can not be used to compress or decompress images, in PixelBox.BulkPixelConversion");
            }

            // The easy case
            if (src.Format == dst.Format) {
                // Everything consecutive?
                if (src.Consecutive && dst.Consecutive) {
                    CopyBytes(dst.Data, dst.Offset, src.Data, src.Offset, src.ConsecutiveSize);
                    return;
                }
                unsafe {
                    byte* srcBytes = (byte*)src.Data.ToPointer();
                    byte* dstBytes = (byte*)dst.Data.ToPointer();
                    byte* srcptr = srcBytes + src.Offset;
                    byte* dstptr = dstBytes + dst.Offset;
                    int srcPixelSize = PixelUtil.GetNumElemBytes(src.Format);
                    int dstPixelSize = PixelUtil.GetNumElemBytes(dst.Format);

                    // Calculate pitches+skips in bytes
                    int srcRowPitchBytes = src.RowPitch * srcPixelSize;
                    //int srcRowSkipBytes = src.RowSkip * srcPixelSize;
                    int srcSliceSkipBytes = src.SliceSkip * srcPixelSize;

                    int dstRowPitchBytes = dst.RowPitch * dstPixelSize;
                    //int dstRowSkipBytes = dst.RowSkip * dstPixelSize;
                    int dstSliceSkipBytes = dst.SliceSkip * dstPixelSize;

                    // Otherwise, copy per row
                    int rowSize = src.Width * srcPixelSize;
                    for (int z = src.Front; z < src.Back; z++) {
                        for (int y = src.Top; y < src.Bottom; y++) {
                            byte* s = srcptr;
                            byte* d = dstptr;
                            for (int i = 0; i < rowSize; i++)
                                *d++ = *s++;
                            srcptr += srcRowPitchBytes;
                            dstptr += dstRowPitchBytes;
                        }
                        srcptr += srcSliceSkipBytes;
                        dstptr += dstSliceSkipBytes;
                    }
                }
                return;
            }

            if (PixelConversionLoops.DoOptimizedConversion(src, dst))
                // If so, good
                return;

            unsafe {
                byte* srcBytes = (byte*)src.Data.ToPointer();
                byte* dstBytes = (byte*)dst.Data.ToPointer();
                byte* srcptr = srcBytes + src.Offset;
                byte* dstptr = dstBytes + dst.Offset;
                int srcPixelSize = PixelUtil.GetNumElemBytes(src.Format);
                int dstPixelSize = PixelUtil.GetNumElemBytes(dst.Format);

                // Calculate pitches+skips in bytes
                int srcRowSkipBytes = src.RowSkip * srcPixelSize;
                int srcSliceSkipBytes = src.SliceSkip * srcPixelSize;
                int dstRowSkipBytes = dst.RowSkip * dstPixelSize;
                int dstSliceSkipBytes = dst.SliceSkip * dstPixelSize;

                // The brute force fallback
                float r, g, b, a;
                for (int z = src.Front; z < src.Back; z++) {
                    for (int y = src.Top; y < src.Bottom; y++) {
                        for (int x = src.Left; x < src.Right; x++) {
                            UnpackColor(out r, out g, out b, out a, src.Format, srcptr);
                            PackColor(r, g, b, a, dst.Format, dstptr);
                            srcptr += srcPixelSize;
                            dstptr += dstPixelSize;
                        }
                        srcptr += srcRowSkipBytes;
                        dstptr += dstRowSkipBytes;
                    }
                    srcptr += srcSliceSkipBytes;
                    dstptr += dstSliceSkipBytes;
                }
            }
        }

        #region Static Bulk Conversion Methods

        ///*************************************************************************
        ///   Pixel packing/unpacking utilities
        ///*************************************************************************


        //           ///<summary>
        //           ///    Pack a color value to memory
        //           ///</summary>
        //           ///<param name="color">The color</param>
        //           ///<param name="format">Pixel format in which to write the color</param>
        //           ///<param name="dest">Destination memory location</param>
        //   		public static void PackColor(System.Drawing.Color color, PixelFormat format,  IntPtr dest) {
        //   			PackColor(color.r, color.g, color.b, color.a, format, dest);
        //   		}

        ///<summary>
        ///    Pack a color value to memory
        ///</summary>
        ///<param name="r,g,b,a">The four color components, range 0x00 to 0xFF</param>
        ///<param name="format">Pixelformat in which to write the color</param>
        ///<param name="dest">Destination memory location</param>
        unsafe public void PackColor(uint r, uint g, uint b, uint a, PixelFormat format, byte* dest) {
            PixelFormatDescription des = PixelConverter.GetDescriptionFor(format);
            if ((des.flags & PixelFormatFlags.NativeEndian) != 0) {
                // Shortcut for integer formats packing
                uint value = (((Bitwise.FixedToFixed(r, 8, des.rbits) << des.rshift) & des.rmask) |
                              ((Bitwise.FixedToFixed(g, 8, des.gbits) << des.gshift) & des.gmask) |
                              ((Bitwise.FixedToFixed(b, 8, des.bbits) << des.bshift) & des.bmask) |
                              ((Bitwise.FixedToFixed(a, 8, des.abits) << des.ashift) & des.amask));
                // And write to memory
                Bitwise.IntWrite(dest, des.elemBytes, value);
            } else {
                // Convert to float
                PackColor((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f, format, dest);
            }
        }

        ///<summary>
        ///    Pack a color value to memory
        ///</summary>
        ///<param name="r,g,b,a">
        ///    The four color components, range 0.0f to 1.0f
        ///    (an exception to this case exists for floating point pixel
        ///    formats, which don't clamp to 0.0f..1.0f)
        ///</param>
        ///<param name="format">Pixelformat in which to write the color</param>
        ///<param name="dest">Destination memory location</param>
        unsafe public static void PackColor(float r, float g, float b, float a, PixelFormat format, byte* dest) {
            // Catch-it-all here
            PixelFormatDescription des = PixelConverter.GetDescriptionFor(format);
            if ((des.flags & PixelFormatFlags.NativeEndian) != 0) {
                // Do the packing
                uint value = ((Bitwise.FloatToFixed(r, des.rbits) << des.rshift) & des.rmask) |
                    ((Bitwise.FloatToFixed(g, des.gbits) << des.gshift) & des.gmask) |
                    ((Bitwise.FloatToFixed(b, des.bbits) << des.bshift) & des.bmask) |
                    ((Bitwise.FloatToFixed(a, des.abits) << des.ashift) & des.amask);
                // And write to memory
                Bitwise.IntWrite(dest, des.elemBytes, value);
            } else {
                switch (format) {
                    case PixelFormat.FLOAT32_R:
                        ((float*)dest)[0] = r;
                        break;
                    case PixelFormat.FLOAT32_RGB:
                        ((float*)dest)[0] = r;
                        ((float*)dest)[1] = g;
                        ((float*)dest)[2] = b;
                        break;
                    case PixelFormat.FLOAT32_RGBA:
                        ((float*)dest)[0] = r;
                        ((float*)dest)[1] = g;
                        ((float*)dest)[2] = b;
                        ((float*)dest)[3] = a;
                        break;
                    case PixelFormat.FLOAT16_R:
                        ((ushort*)dest)[0] = Bitwise.FloatToHalf(r);
                        break;
                    case PixelFormat.FLOAT16_RGB:
                        ((ushort*)dest)[0] = Bitwise.FloatToHalf(r);
                        ((ushort*)dest)[1] = Bitwise.FloatToHalf(g);
                        ((ushort*)dest)[2] = Bitwise.FloatToHalf(b);
                        break;
                    case PixelFormat.FLOAT16_RGBA:
                        ((ushort*)dest)[0] = Bitwise.FloatToHalf(r);
                        ((ushort*)dest)[1] = Bitwise.FloatToHalf(g);
                        ((ushort*)dest)[2] = Bitwise.FloatToHalf(b);
                        ((ushort*)dest)[3] = Bitwise.FloatToHalf(a);
                        break;
                    //   				case PixelFormat.SHORT_RGBA:
                    //   					((ushort*)dest)[0] = Bitwise.FloatToFixed(r, 16);
                    //   					((ushort*)dest)[1] = Bitwise.FloatToFixed(g, 16);
                    //   					((ushort*)dest)[2] = Bitwise.FloatToFixed(b, 16);
                    //   					((ushort*)dest)[3] = Bitwise.FloatToFixed(a, 16);
                    //   					break;
                    //   				case PixelFormat.BYTE_LA:
                    //   					((byte*)dest)[0] = Bitwise.FloatToFixed(r, 8);
                    //   					((byte*)dest)[1] = Bitwise.FloatToFixed(a, 8);
                    //   					break;
                    default:
                        // Not yet supported
                        throw new Exception("Pack to " + format + " not implemented, in PixelUtil.PackColor");
                }
            }
        }

        //           /** Unpack a color value from memory
        //           	@param color	The color is returned here
        //           	@param pf		Pixelformat in which to read the color
        //           	@param src		Source memory location
        //           */
        //   		protected static void UnpackColor(ref System.Drawing.Color color, PixelFormat pf,  IntPtr src) {
        //   			UnpackColor(color.r, color.g, color.b, color.a, pf, src);
        //   		}

        /** Unpack a color value from memory
          @param r,g,b,a	The color is returned here (as byte)
          @param pf		Pixelformat in which to read the color
          @param src		Source memory location
          @remarks 	This function returns the color components in 8 bit precision,
              this will lose precision when coming from A2R10G10B10 or floating
              point formats.  
        */
        unsafe public static void UnpackColor(ref byte r, ref byte g, ref byte b, ref byte a,
                                                 PixelFormat pf, byte* src) {
            PixelFormatDescription des = PixelConverter.GetDescriptionFor(pf);
            if ((des.flags & PixelFormatFlags.NativeEndian) != 0) {
                // Shortcut for integer formats unpacking
                uint value = Bitwise.IntRead(src, des.elemBytes);
                if ((des.flags & PixelFormatFlags.Luminance) != 0)
                    // Luminance format -- only rbits used
                    r = g = b = (byte)Bitwise.FixedToFixed((value & des.rmask) >> des.rshift, des.rbits, 8);
                else {
                    r = (byte)Bitwise.FixedToFixed((value & des.rmask) >> des.rshift, des.rbits, 8);
                    g = (byte)Bitwise.FixedToFixed((value & des.gmask) >> des.gshift, des.gbits, 8);
                    b = (byte)Bitwise.FixedToFixed((value & des.bmask) >> des.bshift, des.bbits, 8);
                }
                if ((des.flags & PixelFormatFlags.HasAlpha) != 0) {
                    a = (byte)Bitwise.FixedToFixed((value & des.amask) >> des.ashift, des.abits, 8);
                } else
                    a = 255; // No alpha, default a component to full
            } else {
                // Do the operation with the more generic floating point
                float rr, gg, bb, aa;
                UnpackColor(out rr, out gg, out bb, out aa, pf, src);
                r = Bitwise.FloatToByteFixed(rr);
                g = Bitwise.FloatToByteFixed(gg);
                b = Bitwise.FloatToByteFixed(bb);
                a = Bitwise.FloatToByteFixed(aa);
            }
        }

        unsafe public static void UnpackColor(out float r, out float g, out float b, out float a,
                                                 PixelFormat pf, byte* src) {
            PixelFormatDescription des = PixelConverter.GetDescriptionFor(pf);
            if ((des.flags & PixelFormatFlags.NativeEndian) != 0) {
                // Shortcut for integer formats unpacking
                uint value = Bitwise.IntRead(src, des.elemBytes);
                if ((des.flags & PixelFormatFlags.Luminance) != 0) {
                    // Luminance format -- only rbits used
                    r = g = b = Bitwise.FixedToFloat(
                        (value & des.rmask) >> des.rshift, des.rbits);
                } else {
                    r = Bitwise.FixedToFloat((value & des.rmask) >> des.rshift, des.rbits);
                    g = Bitwise.FixedToFloat((value & des.gmask) >> des.gshift, des.gbits);
                    b = Bitwise.FixedToFloat((value & des.bmask) >> des.bshift, des.bbits);
                }
                if ((des.flags & PixelFormatFlags.HasAlpha) != 0)
                    a = Bitwise.FixedToFloat((value & des.amask) >> des.ashift, des.abits);
                else
                    a = 1.0f; // No alpha, default a component to full
            } else {
                switch (pf) {
                    case PixelFormat.FLOAT32_R:
                        r = g = b = ((float*)src)[0];
                        a = 1.0f;
                        break;
                    case PixelFormat.FLOAT32_RGB:
                        r = ((float*)src)[0];
                        g = ((float*)src)[1];
                        b = ((float*)src)[2];
                        a = 1.0f;
                        break;
                    case PixelFormat.FLOAT32_RGBA:
                        r = ((float*)src)[0];
                        g = ((float*)src)[1];
                        b = ((float*)src)[2];
                        a = ((float*)src)[3];
                        break;
                    case PixelFormat.FLOAT16_R:
                        r = g = b = Bitwise.HalfToFloat(((ushort*)src)[0]);
                        a = 1.0f;
                        break;
                    case PixelFormat.FLOAT16_RGB:
                        r = Bitwise.HalfToFloat(((ushort*)src)[0]);
                        g = Bitwise.HalfToFloat(((ushort*)src)[1]);
                        b = Bitwise.HalfToFloat(((ushort*)src)[2]);
                        a = 1.0f;
                        break;
                    case PixelFormat.FLOAT16_RGBA:
                        r = Bitwise.HalfToFloat(((ushort*)src)[0]);
                        g = Bitwise.HalfToFloat(((ushort*)src)[1]);
                        b = Bitwise.HalfToFloat(((ushort*)src)[2]);
                        a = Bitwise.HalfToFloat(((ushort*)src)[3]);
                        break;
                    //   				case PixelFormat.SHORT_RGBA:
                    //   					r = Bitwise.FixedToFloat(((ushort*)src)[0], 16);
                    //   					g = Bitwise.FixedToFloat(((ushort*)src)[1], 16);
                    //   					b = Bitwise.FixedToFloat(((ushort*)src)[2], 16);
                    //   					a = Bitwise.FixedToFloat(((ushort*)src)[3], 16);
                    //   					break;
                    //   				case PixelFormat.BYTE_LA:
                    //   					r = g = b = Bitwise.FixedToFloat(((byte*)src)[0], 8);
                    //   					a = Bitwise.FixedToFloat(((byte*)src)[1], 8);
                    //   					break;
                    default:
                        // Not yet supported
                        throw new Exception("Unpack from " + pf + " not implemented, in PixelUtil.UnpackColor");
                }
            }
        }

        unsafe public static void CopyBytes(IntPtr dst, int dstOffset, IntPtr src, int srcOffset, int count) {
            byte* srcBytes = (byte*)src.ToPointer();
            byte* dstBytes = (byte*)dst.ToPointer();
            for (int i = 0; i < count; i++)
                dstBytes[dstOffset + i] = srcBytes[srcOffset + i];
        }


        #endregion Static Bulk Conversion Methods
    }

}

