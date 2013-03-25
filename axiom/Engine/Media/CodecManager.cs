using System;
using System.Collections;
using Axiom.Core;
using Tao.DevIl;

namespace Axiom.Media {
	/// <summary>
	///    Manages registering/fulfilling requests for codecs that handle various types of media.
	/// </summary>
	public sealed class CodecManager : IDisposable {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static CodecManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal CodecManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static CodecManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

        public void Dispose() {
            if (instance == this) {
                instance = null;
            }
        }

        #region Fields

        /// <summary>
        ///    List of registered media codecs.
        /// </summary>
        private Hashtable codecs = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

        #endregion Fields

        /// <summary>
        ///     Register all default IL image codecs.
        /// </summary>
        public void RegisterCodecs() {
            // register codecs
            RegisterCodec(new ILImageCodec("jpg", Il.IL_JPG));
            RegisterCodec(new ILImageCodec("bmp", Il.IL_BMP));
            RegisterCodec(new ILImageCodec("png", Il.IL_PNG));
            RegisterCodec(new ILImageCodec("dds", Il.IL_DDS));
            RegisterCodec(new ILImageCodec("tga", Il.IL_TGA));
        }

        /// <summary>
        ///    Registers a new codec that can handle a particular type of media files.
        /// </summary>
        /// <param name="codec"></param>
        public void RegisterCodec(ICodec codec) {
            codecs[codec.Type] = codec;
        }

        /// <summary>
        ///    Gets the codec registered for the passed in file extension.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ICodec GetCodec(string extension) {
            if(!codecs.ContainsKey(extension)) {
                throw new AxiomException("No codec available for media with extension .{0}", extension);
            }

            return (ICodec)codecs[extension];
        }
	}
}
