using System;
using System.IO;

namespace Axiom.Media {
	/// <summary>
	///    Interface describing an object that can handle a form of media, be it
	///    a image, sound, video, etc.
	/// </summary>
	public interface ICodec {

        /// <summary>
        ///    Codes the data from the input chunk into the output chunk.
        /// </summary>
        /// <param name="input">Input stream (encoded data).</param>
        /// <param name="output">Output stream (decoded data).</param>
        /// <param name="args">Variable number of extra arguments.</param>
        /// <returns>
        ///    An object that holds data specific to the media format which this codec deal with.
        ///    For example, an image coded might return a structure that has image related details,
        ///    such as height, width, etc.
        /// </returns>
        object Decode(Stream input, Stream output, params object[] args);

        /// <summary>
        ///    Encodes the data in the input stream and saves the result in the output stream.
        /// </summary>
        /// <param name="input">Input stream (decoded data).</param>
        /// <param name="output">Output stream (encoded data).</param>
        /// <param name="args">Variable number of extra arguments.</param>
        void Encode(Stream input, Stream output, params object[] args);

        /// <summary>
        ///     Encodes data to a file.
        /// </summary>
        /// <param name="input">Stream containing data to write.</param>
        /// <param name="fileName">Filename to output to.</param>
        /// <param name="codecData">Extra data to use in order to describe the codec data.</param>
        void EncodeToFile(Stream input, string fileName, object codecData);

        /// <summary>
        ///    Gets the type of data that this codec is meant to handle, typically a file extension.
        /// </summary>
        String Type {
            get;
        }
	}
}
