using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Axiom.Core;
using Tao.DevIl;

namespace Axiom.Media {
	/// <summary>
	///    Base DevIL (OpenIL) implementation for loading images.
	/// </summary>
	public class ILImageCodec : ImageCodec {
        #region Fields

        /// <summary>
        ///    Flag used to ensure DevIL gets initialized once.
        /// </summary>
        protected static bool isInitialized = false;
        protected static bool isILUInitialized = false;

        protected string type;
        protected int ilType;

        #endregion

        #region Constructor

        public ILImageCodec(string type, int ilType) {
            this.type = type;
            this.ilType = ilType;
            InitializeIL();
        }

        #endregion Constructor

        #region ImageCodec Implementation

        public override void Encode(Stream input, Stream output, params object[] args) {
            throw new NotImplementedException("Encode to memory not implemented");
        }

        // TODO: Fix this to be like Ogre
        public override void EncodeToFile(Stream input, string fileName, object codecData) {
            int imageID;

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);

            byte[] buffer = new byte[input.Length];
            input.Read(buffer, 0, buffer.Length);

            ImageData data = (ImageData)codecData;
            ILFormat ilfmt = ILUtil.ConvertToILFormat(data.format);

            // stuff the data into the image
            Il.ilTexImage(data.width, data.height, 1, (byte)ilfmt.channels, ilfmt.format, ilfmt.type, buffer);

            if (data.flip) {
                // flip the image
                Ilu.iluFlipImage();
            }

            // save the image to file
            Il.ilSaveImage(fileName);

            // delete the image
            Il.ilDeleteImages(1, ref imageID);
        }

        public override object Decode(Stream input, Stream output, params object[] args) {
            ImageData data = new ImageData();

            int imageID;
            int format, bytesPerPixel, imageType;

            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);

            // Put it right side up
            Il.ilEnable(Il.IL_ORIGIN_SET);
            Il.ilSetInteger(Il.IL_ORIGIN_MODE, Il.IL_ORIGIN_UPPER_LEFT);

            // Keep DXTC(compressed) data if present
            Il.ilSetInteger(Il.IL_KEEP_DXTC_DATA, Il.IL_TRUE);

            // create a temp buffer and write the stream into it
            byte[] buffer = new byte[input.Length];
			input.Read(buffer, 0, buffer.Length);

            // load the data into DevIL
            Il.ilLoadL(this.ILType, buffer, buffer.Length);

            // check for an error
            int ilError = Il.ilGetError();

            if(ilError != Il.IL_NO_ERROR) {
                throw new AxiomException("Error while decoding image data: '{0}'", Ilu.iluErrorString(ilError));
            }

            format = Il.ilGetInteger(Il.IL_IMAGE_FORMAT);
            imageType = Il.ilGetInteger(Il.IL_IMAGE_TYPE);
            //bytesPerPixel = Math.Max(Il.ilGetInteger(Il.IL_IMAGE_BPC), 
            //                         Il.ilGetInteger(Il.IL_IMAGE_BYTES_PER_PIXEL));

            // Convert image if ImageType is incompatible with us (double or long)
            if (imageType != Il.IL_BYTE && imageType != Il.IL_UNSIGNED_BYTE &&
                imageType != Il.IL_FLOAT &&
                imageType != Il.IL_UNSIGNED_SHORT && imageType != Il.IL_SHORT) {
                Il.ilConvertImage(format, Il.IL_FLOAT);
                imageType = Il.IL_FLOAT;
            }
            // Converted paletted images
            if (format == Il.IL_COLOUR_INDEX) {
                Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);
                format = Il.IL_BGRA;
                imageType = Il.IL_UNSIGNED_BYTE;
            }

            bytesPerPixel = Il.ilGetInteger(Il.IL_IMAGE_BYTES_PER_PIXEL);

            // populate the image data
            data.format = ILUtil.ConvertFromILFormat(format, imageType);
            data.width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            data.height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            data.depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);
            data.numMipMaps = Il.ilGetInteger(Il.IL_NUM_MIPMAPS);
            data.flags = 0;

            if (data.format == PixelFormat.Unknown) {
		    	Il.ilDeleteImages(1, ref imageID);
			    throw new AxiomException("Unsupported DevIL format: ImageFormat = {0:x} ImageType = {1:x}", format, imageType);
    		}

            // Check for cubemap
            // int cubeflags = Il.ilGetInteger(Il.IL_IMAGE_CUBEFLAGS);
            int numFaces = Il.ilGetInteger(Il.IL_NUM_IMAGES) + 1;
            if (numFaces == 6)
                data.flags |= ImageFlags.CubeMap;
            else
                numFaces = 1; // Support only 1 or 6 face images for now

            // Keep DXT data (if present at all and the GPU supports it)
            int dxtFormat = Il.ilGetInteger(Il.IL_DXTC_DATA_FORMAT);
            if (dxtFormat != Il.IL_DXT_NO_COMP && 
                Root.Instance.RenderSystem.Caps.CheckCap(Axiom.Graphics.Capabilities.TextureCompressionDXT))
            {
                data.format = ILUtil.ConvertFromILFormat(dxtFormat, imageType);
                data.flags |= ImageFlags.Compressed;
            
                // Validate that this devil version saves DXT mipmaps
                if (data.numMipMaps > 0)
                {
                    Il.ilBindImage(imageID);
                    Il.ilActiveMipmap(1);
                    if (Il.ilGetInteger(Il.IL_DXTC_DATA_FORMAT) != dxtFormat)
                    {
                        data.numMipMaps = 0;
                        LogManager.Instance.Write("Warning: Custom mipmaps for compressed image were ignored because they are not loaded by this DevIL version");
                    }
                }
            }
        
            // Calculate total size from number of mipmaps, faces and size
            data.size = Image.CalculateSize(data.numMipMaps, numFaces,
                                            data.width, data.height,
                                            data.depth, data.format);

            // set up buffer for the decoded data
            buffer = new byte[data.size];
            // Pin the buffer, so we can use our PixelBox methods on it
            GCHandle bufGCHandle = new GCHandle();
            bufGCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufPtr = bufGCHandle.AddrOfPinnedObject();

            int offset = 0;

            // Dimensions of current mipmap
            int width = data.width;
            int height = data.height;
            int depth = data.depth;

            // Transfer data
            for (int mip=0; mip <= data.numMipMaps; ++mip)
            {   
                for (int i = 0; i < numFaces; ++i)
                {
                    Il.ilBindImage(imageID);
                    if (numFaces > 1)
                        Il.ilActiveImage(i);
                    if (data.numMipMaps > 0)
                        Il.ilActiveMipmap(mip);
                    /// Size of this face
                    int imageSize = PixelUtil.GetMemorySize(width, height, depth, data.format);
                    if ((data.flags & ImageFlags.Compressed) != 0)
                    {
                        // Compare DXT size returned by DevIL with our idea of the compressed size
                        if (imageSize == Il.ilGetDXTCData(IntPtr.Zero, 0, dxtFormat))
                        {
                            // Retrieve data from DevIL
                            byte[] tmpBuffer = new byte[imageSize];
                            Il.ilGetDXTCData(tmpBuffer, imageSize, dxtFormat);
                            // Copy the data into our output buffer
                            Array.Copy(tmpBuffer, 0, buffer, offset, tmpBuffer.Length);
                        } else {
                            LogManager.Instance.Write("Warning: compressed image size mismatch, devilsize={0} oursize={1}", 
                                                      Il.ilGetDXTCData(IntPtr.Zero, 0, dxtFormat), imageSize);
                        }
                    }
                    else
                    {
                        /// Retrieve data from DevIL
                        PixelBox dst = new PixelBox(width, height, depth, data.format, bufPtr);
                        dst.Offset = offset;
                        ILUtil.ToAxiom(dst);
                    }
                    offset += imageSize;
                }

                /// Next mip
                if (width != 1) width /= 2;
                if (height != 1) height /= 2;
                if (depth != 1) depth /= 2;
            }

            // Restore IL state
            Il.ilDisable(Il.IL_ORIGIN_SET);
            Il.ilDisable(Il.IL_FORMAT_SET);

            // we won't be needing this anymore
            Il.ilDeleteImages(1, ref imageID);

            output.Write(buffer, 0, buffer.Length);


            // Free the buffer we allocated for the conversion.
            // I used bufPtr to store my data while I converted it.
            // I need to free it here.  This invalidates bufPtr.
            // My data has already been copied to output.
            if (bufGCHandle.IsAllocated)
                bufGCHandle.Free();

            return data;
        }

        #endregion ImageCodec Implementation

        #region Methods

        /// <summary>
        ///    One time DevIL initialization.
        /// </summary>
        public static void InitializeIL() {
            if(!isInitialized) {
                // fire it up!
                Il.ilInit();

                // enable automatic file overwriting
                Il.ilEnable(Il.IL_FILE_OVERWRITE);

                isInitialized = true;
            }
        }

        public static void InitializeILU() {
            if(!isILUInitialized) {
                // init Il utils
                Ilu.iluInit();
                isILUInitialized = true;
            }
        }

        public static bool ReshapeToPowersOf2AndSave(Stream input, int size, string outputFileName)
        {
            InitializeIL();
            InitializeILU();

            // load the image 
            ImageData data = new ImageData();
            int imageID;
            // create and bind a new image
            Il.ilGenImages(1, out imageID);
            Il.ilBindImage(imageID);
            // create a temp buffer and copy the stream into it
            byte[] buffer = new byte[size];
            int bytesRead = 0, byteOffset = 0, bytes = buffer.Length;
            do {
                bytesRead = input.Read(buffer, byteOffset, bytes);
                bytes -= bytesRead;
                byteOffset += bytesRead;
            } while (bytes > 0 && bytesRead > 0);

            Il.ilLoadL(Il.IL_TYPE_UNKNOWN, buffer, buffer.Length);
            // check errors
            int ilError = Il.ilGetError();
            if (ilError != Il.IL_NO_ERROR) {
                throw new AxiomException("Error while loading image data: '{0}'", Ilu.iluErrorString(ilError));
            }
            // determine dimensions & compute powers-of-2 reshape if needed
            int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            int newWidth = (int)Math.Pow(2, Math.Floor(Math.Log(width, 2)));
            int newHeight = (int)Math.Pow(2, Math.Floor(Math.Log(height, 2)));
            if (width != newWidth || height != newHeight) {
                // reshape
                // set the scale function filter & scale
                Ilu.iluImageParameter(Ilu.ILU_FILTER, Ilu.ILU_BILINEAR); // .ILU_SCALE_BSPLINE);
                Ilu.iluScale(newWidth, newHeight, 1);
            }
            // save
            Il.ilSetInteger(Il.IL_JPG_QUALITY, 50);
            Il.ilSaveImage(outputFileName);
            // drop image
            Il.ilDeleteImages(1, ref imageID);
            return true;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        ///    Implemented by subclasses to return the IL type enum value for this
        ///    images file type.
        /// </summary>
        public int ILType {
            get {
                return ilType;
            }
        }

        public override string Type {
            get {
                return type;
            }
        }

        #endregion Properties
	}
}
