using System;
using System.IO;
using Axiom.Core;
using Axiom.MathLib;
using System.Runtime.InteropServices;

namespace Axiom.Graphics {
	/// <summary>
	///     This class is intended to allow a clean stream interface for writing to hardware buffers.
	///     An instance of this would be returned by one of the HardwareBuffer.Lock methods, which would
	///     allow easily and safely writing to a hardware buffer without having to use unsafe code.
	/// </summary>
	public class BufferStream {
		#region Fields
		
		/// <summary>
		///		Current position (as a byte offset) into the stream.
		/// </summary>
		protected long position;
        /// <summary>
        ///     Pointer to the raw data we will be writing to.
        /// </summary>
        protected IntPtr data;
        /// <summary>
        ///     Reference to the hardware buffer who owns this stream.
        /// </summary>
        protected HardwareBuffer owner;

        /// <summary>
        ///     Temp array.
        /// </summary>
        protected ValueType[] tmp = new ValueType[1];

		#endregion Fields

        #region Constructor

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="owner">Reference to the hardware buffer who owns this stream.</param>
        /// <param name="data">Pointer to the raw data we will be writing to.</param>
        internal BufferStream(HardwareBuffer owner, IntPtr data) {
            this.data = data;
            this.owner = owner;
        }

        #endregion Constructor

		#region Methods
	
		/// <summary>
        ///     Length (in bytes) of this stream.
        /// </summary>
		public long Length {
			get {
				return owner.Size;
			}
		}

		/// <summary>
		///     Current position of the stream.
		/// </summary>
		public long Position {
			get {
				return position;
			}
			set {
				if(value > this.Length) {
					throw new ArgumentException("Position of the buffer may not exceed the length.");
				}

				position = value;
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int Read(byte[] buffer, int offset, int count) {
			// TODO:  Add BufferStream.Read implementation
			return 0;
		}

		public void Write(Vector3 vec, int offset) {
            tmp[0] = vec;

            Write(tmp, offset);
        }

        public void Write(Vector4 vec, int offset) {
            tmp[0] = vec;

            Write(tmp, offset);
        }

        public void Write(float val, int offset) {
            tmp[0] = val;

            Write(tmp, offset);
        }

		public void Write(short val, int offset) {
            tmp[0] = val;

            Write(tmp, offset);
        }

		public void Write(byte val, int offset) {
            tmp[0] = val;

            Write(tmp, offset);
        }

        public void Write(System.Array val) {
            Write(val, 0);
        }

        public void Write(System.Array val, int offset) {
            int count = Marshal.SizeOf(val.GetType().GetElementType()) * val.Length;

            Write(val, offset, count);
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="val"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
		public void Write(System.Array val, int offset, int count) {
            // can't write to unlocked buffers
            if (!owner.IsLocked) {
                throw new AxiomException("Cannot write to a buffer stream when the buffer is not locked.");
            }

            long newOffset = position + offset;

            // ensure we won't go past the end of the stream
            if (newOffset + count > this.Length) {
                throw new AxiomException("Unable to write data past the end of a BufferStream");
            }

            // pin the array so we can get a pointer to it
            GCHandle handle = GCHandle.Alloc(val, GCHandleType.Pinned);

            unsafe {
                // get byte pointers for the source and target
                byte* b = (byte*)handle.AddrOfPinnedObject().ToPointer();
                byte* dataPtr = (byte*)data.ToPointer();

                // copy the data from the source to the target
                for (int i = 0; i < count; i++) {
                    dataPtr[i + newOffset] = b[i];
                }
            }

            handle.Free();
        }

        /// <summary>
        ///     Moves the "cursor" position within the buffer.
        /// </summary>
        /// <param name="offset">Offset (in bytes) to move from the current position.</param>
        /// <returns></returns>
        public long Seek(long offset) {
            return Seek(offset, SeekOrigin.Current);
        }

        /// <summary>
		///     Moves the "cursor" position within the buffer.
		/// </summary>
		/// <param name="offset">Number of bytes to move.</param>
		/// <param name="origin">How to treat the offset amount.</param>
		/// <returns></returns>
		public long Seek(long offset, SeekOrigin origin) {
			switch(origin) {
                // seeks from the beginning of the stream
				case SeekOrigin.Begin:
					position = offset;
					break;

                // offset is from the current stream position
				case SeekOrigin.Current:
					if(position + offset > this.Length) {
						throw new ArgumentException("Cannot seek past the end of the stream.");
					}

					position = position + offset;
					break;

                // seeks backwards from the end of the stream
				case SeekOrigin.End:
					if(this.Length - offset < 0) {
						throw new ArgumentException("Cannot seek past the beginning of the stream.");
					}

                    position = this.Length - offset;
                    break;
			}

			return position;
		}

		#endregion Methods
	}
}		

