using System;
using Tao.DevIl;

namespace Axiom.Media
{
	/// <summary>
	///    PNG image file codec.
	/// </summary>
	public class PNGCodec : ILImageCodec {
		public PNGCodec() {
		}

        #region ILImageCodec Implementation

        /// <summary>
        ///    Passthrough implementation, no special code needed.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object Decode(System.IO.Stream input, System.IO.Stream output, params object[] args) {
            // nothing special needed, just pass through
            return base.Decode(input, output, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="args"></param>
        public override void Encode(System.IO.Stream source, System.IO.Stream dest, params object[] args) {
            throw new NotImplementedException("PNG encoding is not yet implemented.");
        }

        /// <summary>
        ///    Returns the PNG file extension.
        /// </summary>
        public override String Type {
            get {
                return "png";
            }
        }


        /// <summary>
        ///    Returns PNG enum.
        /// </summary>
        public override int ILType {
            get {
                return Il.IL_PNG;
            }
        }

        #endregion ILImageCodec Implementation
	}
}
