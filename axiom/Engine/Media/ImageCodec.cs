using System;
using System.IO;

namespace Axiom.Media {
	/// <summary>
	/// Summary description for ImageCodec.
	/// </summary>
	public abstract class ImageCodec : ICodec {
		public ImageCodec() {
        }
        
        #region ICodec Members

        // Note: Redefining as abstract to force subclasses to implement, since interface methods must still be included
        // in abstract base classes
        public abstract object Decode(Stream input, Stream output, params object[] args);
        public abstract void Encode(Stream input, Stream output, params object[] args);
        public abstract void EncodeToFile(Stream input, string fileName, object codecData);

        public abstract string Type {
            get;
        }

        #endregion

        public class ImageData {
            public int width;
            public int height;
            public int depth;
            public int bpp;
            public int size;
            public ImageFlags flags;
            public int numMipMaps;
            public PixelFormat format;
            public bool flip;
        }
    }
}
