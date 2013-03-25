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

package multiverse.server.network;

import java.nio.channels.*;
import java.nio.ByteBuffer;
import multiverse.server.util.Log;


public class ChannelUtil
{
    public static final int TIMEOUT = 30 * 1000;

    public static int fillBuffer(ByteBuffer buffer, SocketChannel socket)
	throws java.io.IOException
    {
        Selector selector = null;
        try {
            selector = Selector.open();
            socket.register(selector, SelectionKey.OP_READ);
            while (buffer.remaining() > 0) {
                int nReady = selector.select(TIMEOUT);
                if (nReady == 1) {
                    selector.selectedKeys().clear();
                    int nBytes = socket.read(buffer);
                    if (nBytes == -1)
                        break;
                }
                else {
                    Log.debug("Connection timeout while reading");
                    break;
                }
            }
        }
        finally  {
            selector.close();
        }
	buffer.flip();
	return buffer.limit();
    }

    public static boolean writeBuffer(MVByteBuffer buffer, SocketChannel socket)
        throws java.io.IOException
    {
        Selector selector = null;
        try {
            selector = Selector.open();
            socket.register(selector, SelectionKey.OP_WRITE);
            while (buffer.hasRemaining()) {
                int nReady = selector.select(TIMEOUT);
                if (nReady == 1) {
                    selector.selectedKeys().clear();
                    if (socket.write(buffer.getNioBuf()) == 0)
                        break;
                }
                else {
                    Log.debug("Connection timeout while writing");
                    break;
                }
            }
        }
        finally  {
            selector.close();
        }
        return ! buffer.hasRemaining();
    }

    public static void patchLengthAndFlip(MVByteBuffer messageBuf)
    {
        int len = messageBuf.position();
        messageBuf.getNioBuf().rewind();
        messageBuf.putInt(len-4);
        messageBuf.position(len);
        messageBuf.getNioBuf().flip();
    }


}

