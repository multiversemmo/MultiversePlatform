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

package multiverse.msgsys;

import java.io.*;
import java.util.*;
import java.nio.channels.*;
import java.nio.ByteBuffer;

import multiverse.server.network.MVByteBuffer;
import multiverse.server.util.Log;
import multiverse.server.util.MVRuntimeException;


public class MessageIO implements Runnable
{
    public MessageIO()
    {
    }
    
    // Provide a construct that permits initializing the number of
    // bytes in a message length.
    public MessageIO(int messageLengthByteCount)
    {
        setMessageLengthByteCount(messageLengthByteCount);
    }
    
    public MessageIO(Callback callback)
    {
        initialize(callback);
    }
    
    protected void initialize(Callback callback)
    {
        readBuf = ByteBuffer.allocate(8192);
        this.callback = callback;
        try {
            ioSelector = Selector.open();
        } catch (IOException ex) {
            Log.exception("MessageHandler selector failed", ex);
            System.exit(1);
        }
    }

    public void start()
    {
        start("MessageIO");
    }

    public void start(String threadName)
    {
        if (Log.loggingNet)
            Log.net("MessageIO.start: Starting MessageIO thread");
        Thread thread = new Thread(this,threadName);
        thread.setDaemon(true);
        thread.start();
    }

    public void addAgent(AgentInfo agentInfo)
    {
        synchronized (newAgents) {
            newAgents.add(agentInfo);
        }
        ioSelector.wakeup();
    }

    public void removeAgent(AgentInfo agentInfo)
    {
        agentInfo.socket.keyFor(ioSelector).cancel();
        ioSelector.wakeup();
    }

    public void outputReady()
    {
        scanForWrite = true;
        ioSelector.wakeup();
    }

    public void addToOutputWithLength(MVByteBuffer buf, AgentInfo agentInfo) {
        boolean needNotify = true;
        synchronized (agentInfo.outputBuf) {
            needNotify = agentInfo.outputBuf.position() == 0;
            putMessageLength(buf, agentInfo);
            byte[] data = buf.array();
            agentInfo.outputBuf.putBytes(data, 0, buf.limit());
        }
        if (needNotify)
            outputReady();
    }

    public void addToOutput(MVByteBuffer buf, AgentInfo agentInfo) {
        boolean needNotify = true;
        synchronized (agentInfo.outputBuf) {
            needNotify = agentInfo.outputBuf.position() == 0;
            byte[] data = buf.array();
            agentInfo.outputBuf.putBytes(data, 0, buf.limit());
        }
        if (needNotify)
            outputReady();
    }

    public interface Callback {
        // Called with positive length when data is read from socket.
        // Called with -1 length (and null buf) when socket is closed.
        void handleMessageData(int length, MVByteBuffer buf, AgentInfo agentInfo);
    }

    public void run()
    {
        while (true) {
            try {
                doMessageIO();
            }
            catch (IOException ex) {
                Log.exception("MessageIO thread got", ex);
            }
            catch (Exception ex) {
                Log.exception("MessageIO thread got", ex);
            }
        }
    }

    private void doMessageIO()
        throws IOException
    {
        ioSelector.select();

        synchronized (newAgents) {
            for (AgentInfo agentInfo : newAgents) {
                try {
                    agentInfo.socket.register(ioSelector,
                            SelectionKey.OP_READ | SelectionKey.OP_WRITE,
                            agentInfo);
                }
                catch (ClosedChannelException ex) {
                    Log.exception("addNewAgent",ex);
                    try {
                        //## should not hold newAgents for this callback
                        callback.handleMessageData(-1,null,agentInfo);
                    }
                    catch (MVRuntimeException e) { /* ignore */ }
                }
            }
            newAgents.clear();
        }

        Set<SelectionKey> readyKeys = ioSelector.selectedKeys();
        for (SelectionKey key : readyKeys) {
            SocketChannel socket = (SocketChannel) key.channel();
            AgentInfo agentInfo = (AgentInfo) key.attachment();
            try {
                handleReadyChannel(socket, key, agentInfo);
            }
            catch (CancelledKeyException ex) { 
                Log.debug("Connection closed (cancelled) "+socket);
                try {
                    callback.handleMessageData(-1,null,agentInfo);
                } catch (Exception e) { /* ignore */ }
            }
        }
 
        if (scanForWrite) {
            Set<SelectionKey> allKeys = ioSelector.keys();
            for (SelectionKey key : allKeys) {
                AgentInfo agentInfo = (AgentInfo) key.attachment();
                try {
                    synchronized (agentInfo.outputBuf) {
                        if (agentInfo.outputBuf.position() > 0)
                            key.interestOps(SelectionKey.OP_READ |
                                SelectionKey.OP_WRITE);
                    }
                }
                catch (CancelledKeyException ex) { /* ignore */ }
            }
            scanForWrite = false;
        }
    }

    private void handleReadyChannel(SocketChannel socket, SelectionKey key,
        AgentInfo agentInfo)
        throws IOException
    {
        if (key.isWritable()) {
            synchronized (agentInfo.outputBuf) {
                if (agentInfo.outputBuf.position() > 0) {
                    agentInfo.outputBuf.flip();
                    try {
                        socket.write(agentInfo.outputBuf.getNioBuf());
                    }
                    catch (IOException ex) {
                        Log.debug("Connection closed (exception on write) "+
                            socket + " exception=" + ex);
                        return;
                    }

                    agentInfo.outputBuf.getNioBuf().compact();
                }
            }
        }
        if (key.isReadable()) {
            int nRead = -1;
            readBuf.clear();
            try {
                nRead = socket.read(readBuf);
            }
            catch (IOException ex) {
                Log.debug("Connection closed (exception) "+socket);
                socket.close();
                try {
                    callback.handleMessageData(-1,null,agentInfo);
                } catch (Exception e) {
                    Log.exception("Exception handling closed connection",e);
                }
                return;
            }
            if (nRead == -1) {
                Log.debug("Connection closed (-1) "+socket);
                socket.close();
                try {
                    callback.handleMessageData(-1,null,agentInfo);
                } catch (Exception e) {
                    Log.exception("Exception handling closed connection",e);
                }
                return;
            }
            readBuf.flip();

            addAgentData(readBuf,agentInfo);
        }
        synchronized (agentInfo.outputBuf) {
            if (agentInfo.outputBuf.position() > 0) {
                key.interestOps(
                    SelectionKey.OP_READ | SelectionKey.OP_WRITE);
            }
            else  {
                key.interestOps(SelectionKey.OP_READ);
                agentInfo.outputBuf.clear();
            }
        }
    }

    private void addAgentData(ByteBuffer buf, AgentInfo agentInfo)
    {
//         if (Log.loggingNet)
//             Log.net("MessageIO.addAgentData: agentInfo.socket " +  agentInfo.socket + ", buf " + buf);
        MVByteBuffer inputBuf = agentInfo.inputBuf;
        if (inputBuf.remaining() < buf.limit()) {
            int additional = inputBuf.capacity();
            while (inputBuf.remaining() + additional < buf.limit()) {
                additional = 2 * additional;
            }
            MVByteBuffer newBuf = new MVByteBuffer(
                inputBuf.capacity() + additional);
            byte[] bytes = inputBuf.array();
            newBuf.putBytes(bytes, 0, bytes.length);
            newBuf.position(inputBuf.position());
            newBuf.limit(inputBuf.limit());
            agentInfo.inputBuf = newBuf;
            inputBuf = newBuf;
        }

        byte[] bytes = buf.array();
        inputBuf.putBytes(bytes,0,buf.limit());

        inputBuf.flip();
        while (inputBuf.remaining() >= 4) {
            int currentPos = inputBuf.position();
            int messageLen = getMessageLength(inputBuf);
            if (inputBuf.remaining() < messageLen)  {
                inputBuf.position(currentPos);
                break;
            }
            try {
                callback.handleMessageData(messageLen,inputBuf,agentInfo);
            }
            catch (Exception ex) {
                Log.exception("handleMessageData", ex);
            }
            // Move the position as if we read the data
            // The callback may or may not have read the data, so this
            // ensures the position is moved.
            inputBuf.position(currentPos + messageLengthByteCount + messageLen);
        }
        inputBuf.getNioBuf().compact();
    }

    // This, together with private variable 
    private int getMessageLength(MVByteBuffer inputBuf) {
        switch (messageLengthByteCount) {
        case 4:
            return inputBuf.getInt();
        case 2:
            return (int)inputBuf.getShort();
        case 1:
            return (int)inputBuf.getByte();
        default:
            throw new MVRuntimeException("MessageIO.getMessageLength: messageLengthByteCount is " + messageLengthByteCount);
        }
    }

    private void putMessageLength(MVByteBuffer buf, AgentInfo agentInfo) {
        int dataLen = buf.limit();
        MVByteBuffer target = agentInfo.outputBuf;
        switch(messageLengthByteCount) {
        case 4:
            target.putInt(dataLen);
            break;
        case 2:
            target.putShort((short)dataLen);
            break;
        case 1:
            target.putByte((byte)dataLen);
            break;
        default:
            Log.error("MessageIO.putBufLength: messageLengthByteCount is " + messageLengthByteCount);
            target.putInt(dataLen);
            break;
        }
    }
    
    public int getMessageLengthByteCount() {
        return messageLengthByteCount;
    }
    
    public void setMessageLengthByteCount(int messageLengthByteCount) {
        this.messageLengthByteCount = messageLengthByteCount;
    }

    private Callback callback;
    private List<AgentInfo> newAgents = new LinkedList<AgentInfo>();
    private Selector ioSelector;
    private ByteBuffer readBuf;
    private boolean scanForWrite = false;
    // The length in bytes of the message length field that preceeds
    // every message.  Most messages use 4-byte message lengths, the
    // default, but Voice uses 2-byte message lengths
    private int messageLengthByteCount = 4;
}
