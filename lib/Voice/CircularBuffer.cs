#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using log4net;
using Multiverse.Lib.LogUtil;

#endregion


namespace Multiverse.Voice
{

    ///<summary>
    ///    A circular queue of shorts that knows how to copy ranges
    ///</summary>
    public class CircularBuffer {
        private short[] buffer;
        protected int readPos;
        protected int writePos;
        protected int used;
        protected int size;

        public CircularBuffer(int size)
        {
            this.size = size;
            buffer = new short[size];
            readPos = 0;
            writePos = 0;
            used = 0;
        }

        ///<summary>
        ///    Add a single short value to the playback queue
        ///</summary>
        public void Put(short value)
        {
            if (IsFull())
                ThrowError("CircularBuffer.Put: Buffer is Full!");
            buffer[writePos] = value;
            writePos = (writePos + 1) % size;
            used++;
        }

        ///<summary>
        ///    Add elements of array samples, starting at array
        ///    index sampleIndex, to the circular buffer.  Returns
        ///    the number of elements actually added.
        ///</summary>
        public int PutSamples(short[] source, int sourceOffset, int wantCount) {
            lock(this) {
                if (IsFull())
                    return 0;
                // writePos is the first element.  What's the count?
                int count = Math.Min(size - used, wantCount);
                for (int i=0; i<count; i++) {
                    buffer[writePos] = source[sourceOffset + i];
                    writePos = (writePos + 1) % size;
                }
                used += count;
                return count;
            }
        }

        ///<summary>
        ///    Get a single short value from the playback queue
        ///</summary>
        public short Get() {
            lock(this) {
                if (IsEmpty())
                    ThrowError("CircularBuffer.Get: Buffer is Empty.");
                short value = buffer[readPos];
                readPos = (readPos + 1) % size;
                used--;
                return value;
            }
        }

        ///<summary>
        ///    Get min(available, wantCount) short samples and add
        ///    them to dest.  Return the number of shorts copied.
        ///</summary>
        public int GetSamples(IntPtr dest, int wantCount) {
            lock(this) {
                if (used < wantCount)
                    return 0;
                int count = Math.Min(used, wantCount);
                unsafe {
                    short* shorts = (short*)dest;
                    for (int i = 0; i < count; i++) {
                        *shorts++ = buffer[readPos];
                        readPos = (readPos + 1) % size;
                    }
                }
                used -= count;
                return count;
            }
        }

        ///<summary>
        ///    Get min(available, wantCount) short samples and
        ///    assign them to short array dest, starting at array
        ///    element offset destOffset.  Return the number of
        ///    shorts copied. 
        ///</summary>
        public int GetSamples(short[] dest, int destOffset, int wantCount) {
            lock(this) {
                if (IsEmpty())
                    return 0;
                int actualCount = Math.Min(used, wantCount);
                for (int counter=0; counter < actualCount; counter++) {
                    dest[destOffset + counter] = buffer[readPos];
                    readPos = (readPos + 1) % size;
                }
                used -= actualCount;
                return actualCount;
            }
        }

        ///<summary>
        ///    Return true if all slots have been used; false otherwise
        ///</summary>
        public bool IsFull() {
            return (used == size);
        }

        ///<summary>
        ///    Return true if no slots have been used; false otherwise
        ///</summary>
        public bool IsEmpty() {
            return (used == 0);
        }

        ///<summary>
        ///    Return the count of short samples in the buffer
        ///</summary>
        public int Used {
            get { return used; }
        }

        ///<summary>
        ///    Return the number of free slots in the buffer
        ///</summary>
        public int Free {
            get { return size - used; }
        }

        ///<summary>
        ///    Return the array offset of the next sample written,
        ///    used only for logging.
        ///</summary>
        public int WritePos {
            get { return writePos; }
        }

        ///<summary>
        ///    Return the array offset of the next sample to be read,
        ///    used only for logging.
        ///</summary>
        public int ReadPos {
            get { return readPos; }
        }

        ///<summary>
        ///    This records the message and the stack trace, and then
        ///    throws the exception.
        ///</summary>
        public static void ThrowError(string msg) {
            log.Error(msg + "; stack trace:\n" + new StackTrace(true).ToString());
            throw new Exception(msg);
        }
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CircularBuffer));
    }

}
