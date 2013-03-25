/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole
{
    /// <summary>
    /// This class implements a Stream on top of a text buffer.
    /// </summary>
    internal class TextBufferStream : Stream
    {
        // The text buffer used to write.
        private IVsTextLines textLines;
        // The text marker used to mark the read-only region of the buffer.
        private IVsTextLineMarker lineMarker;
        private byte[] byteBuffer;
        int usedBuffer;

        private const int bufferSize = 1024;

        /// <summary>
        /// Creates a new TextBufferStream on top of a text buffer.
        /// The optional text marker can be used to let the stream set read only the
        /// text that it writes on the buffer.
        /// </summary>
        public TextBufferStream(IVsTextLines lines)
        {
            if (null == lines)
            {
                throw new ArgumentNullException("lines");
            }
            textLines = lines;
            byteBuffer = new byte[bufferSize];
        }

        /// <summary>
        /// Gets the read status of the stream.
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the seek status of the stream.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the write status of the stream.
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Flushes the pending data.
        /// </summary>
        public override void Flush()
        {
            // If there is no data in the buffer, then there is nothing to do.
            if (0 == usedBuffer)
            {
                // Make sure that the read-only region is correct and exit.
                ExtendReadOnlyRegion();
                return;
            }

            string text = null;
            // We have to use a StreamReader in order to work around problems with the
            // encoding of the data sent in, but in order to build the reader we need
            // a memory stream to read the data in the buffer.
            using (MemoryStream s = new MemoryStream(byteBuffer, 0, usedBuffer))
            {
                // Now we can build the reader from the memory stream.
                using (StreamReader reader = new StreamReader(s))
                {
                    // At the end we can get the text.
                    text = reader.ReadToEnd();
                }
            }
            // Now the buffer is empty.
            usedBuffer = 0;

            // The text is always added at the end of the buffer.
            int lastLine;
            int lastColumn;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.GetLastLineIndex(out lastLine, out lastColumn));

            // Lock the buffer before changing its content.
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.LockBuffer());
            try
            {
                GCHandle handle = GCHandle.Alloc(text, GCHandleType.Pinned);
                try
                {
                    TextSpan[] span = new TextSpan[1];
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textLines.ReplaceLines(lastLine, lastColumn, lastLine, lastColumn, handle.AddrOfPinnedObject(), text.Length, span));
                }
                finally
                {
                    handle.Free();
                }
                // Extend the read-only region of the buffer to include this text.
                ExtendReadOnlyRegion();
            }
            finally
            {
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.UnlockBuffer());
            }
        }

        /// <summary>
        /// Gets the size of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                int length;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetSize(out length));
                return length;
            }
        }

        /// <summary>
        /// Gets the current position inside the stream. The set function is not implemented.
        /// </summary>
        public override long Position
        {
            get
            {
                // The position is always at the end of the buffer.
                int lastLine;
                int lastColumn;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetLastLineIndex(out lastLine, out lastColumn));
                int pos;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetPositionOfLineIndex(lastLine, lastColumn, out pos));
                return (long)pos;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Reads data from the stream. This function is not implemented.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seeks for a specific position inside the stream. This function is
        /// not implemented.
        /// </summary>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the length of the stream. This function is not implemented.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes some data in the stream.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (null == buffer)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (offset + count > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            int totalCopied = 0;
            while (totalCopied < count)
            {
                int copySize = Math.Min(byteBuffer.Length - usedBuffer, count - totalCopied);
                if (copySize > 0)
                    System.Array.Copy(buffer, offset, byteBuffer, usedBuffer, copySize);
                usedBuffer += copySize;
                if (usedBuffer >= byteBuffer.Length)
                {
                    Flush();
                }
                totalCopied += copySize;
            }
        }

        public IVsTextLineMarker ReadOnlyMarker
        {
            get { return lineMarker; }
        }

        /// <summary>
        /// Expands the read only region to the end of the current buffer
        /// </summary>
        private void ExtendReadOnlyRegion()
        {
            // Get the position of the last element in the buffer
            int lastLine;
            int lastColumn;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.GetLastLineIndex(out lastLine, out lastColumn));
            // Check if the text marker for the read-only region was created.
            if (null == lineMarker)
            {
                // No text marker, so we have to create it.
                IVsTextLineMarker[] markers = new IVsTextLineMarker[1];
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.CreateLineMarker(
                            (int)MARKERTYPE.MARKER_READONLY,  // Type of marker.
                            0, 0,                             // Position of the beginning of the marker.
                            lastLine, lastColumn,             // Position of the last char in the marker.
                            null,                             // Object implementing the text marker client.
                            markers));                        // Output marker.
                lineMarker = markers[0];
                // Get the behavior of the marker
                uint behavior;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    lineMarker.GetBehavior(out behavior));
                // Add the track left behavior.
                behavior |= (uint)MARKERBEHAVIORFLAGS.MB_LEFTEDGE_LEFTTRACK;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    lineMarker.SetBehavior(behavior));
            }
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                lineMarker.ResetSpan(0, 0, lastLine, lastColumn));
        }
    }
}
