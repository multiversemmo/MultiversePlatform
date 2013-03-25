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
import java.net.*;
import java.util.*;
import multiverse.server.util.*;

public class ClientUDPConnection extends ClientConnection {
    
    public ClientUDPConnection(DatagramChannel datagramChannel) {
        initializeFromDatagramChannel(datagramChannel);
    }
    
    public ClientUDPConnection(DatagramChannel dc, ClientConnection.MessageCallback messageCallback) {
        this.messageCallback = messageCallback;
        initializeFromDatagramChannel(dc);
    }
    
    protected void initializeFromDatagramChannel(DatagramChannel datagramChannel) {
        socket = datagramChannel;
    }
    
    // Return a string containing the IP and port
    public String IPAndPort() {
        return "UDP(" + remoteAddr + ":" + remotePort + ")";
    }

    public void registerMessageCallback(ClientConnection.MessageCallback messageCallback) {
        this.messageCallback = messageCallback;
    }
    
    public void send(MVByteBuffer buf) {
        lock.lock();
        try {
            if (PacketAggregator.usePacketAggregators) {
                if (!packetAggregator.addMessage(buf)) {
                    if (isOpen())
                        Log.error("ClientUDPConnection.send: for con " + this +
                                ", PacketAggregator.addMessage returned false!");
                }
            }
            else {
                unaggregatedSends++;
                PacketAggregator.allUnaggregatedSends++;
                sendInternal(buf);
            }
        }
        finally {
            lock.unlock();
        }
    }
    
    public boolean sendInternal(MVByteBuffer buf) {
        DatagramChannel dc = channelMap.get(remotePort);
        if (dc != null) {
            Log.error("ClientUDPConnection.sendInternal: Could not find DatagramChannel for remote port " + remotePort);
        }
        try {
            int bytes = dc.send(buf.getNioBuf(), new InetSocketAddress(remoteAddr,
                        remotePort));

            if (Log.loggingNet)
                Log.net("ClientUDPConnection.sendPacket: remoteAddr=" + remoteAddr + ", remotePort=" + remotePort + ", numbytes sent=" + bytes);
        } catch (java.io.IOException e) {
            Log.exception("ClientUDPConnection.sendPacket: remoteAddr=" + remoteAddr + ", remotePort=" + remotePort + ", got exception", e);
            throw new MVRuntimeException("ClientUDPConnection.sendPacket", e);
        }
        return true;
    }
    
    public boolean sendIfPossible(MVByteBuffer buf) {
        send(buf);
        return true;
    }
    
    public int sendMultibuf(List<MVByteBuffer> subMessages, int currentSize) {
        int byteCount = 1;
        for (MVByteBuffer buf : subMessages) {
            int bufSize = buf.limit();
            if (bufSize > 255) {
                // Illegal case - - the length must fit in a byte
                Log.error("ClientUDPConnection.sendMultibuf: Buf size is " + bufSize);
            }
            else
                byteCount += 1 + bufSize;
        }
        MVByteBuffer multiBuf = new MVByteBuffer(byteCount);
        multiBuf.putByte(opcodeAggregatedVoicePacket);
        for (MVByteBuffer buf : subMessages) {
            int bufSize = buf.limit();
            if (bufSize <= 255) {
                multiBuf.putByte((byte)bufSize);
                multiBuf.putBytes(buf.array(), 0, bufSize);
            }
        }
        subMessages.clear();
        multiBuf.rewind();
        aggregatedSends++;
        PacketAggregator.allAggregatedSends++;
        sentMessagesAggregated += byteCount;
        PacketAggregator.allSentMessagesAggregated += byteCount;
        if (Log.loggingNet)
            Log.net("ClientUDPConnection.sendMultiBuf: multiBuf size is " + multiBuf.limit());
        return 0;
    }
    
    public void open(String hostname, int remotePort) {
        Log.error("ClientUDPConnection: open(" + hostname + ":" + remotePort + 
            " called; should never happen");
    }
    
    public void connectionReset() {
        if (messageCallback != null) {
            messageCallback.connectionReset(this);
            socket = null;
        }
    }

    public void close() {
        if (socket != null) {
            try {
                socket.close();
                socket = null;
            } catch (java.io.IOException ex) { }
        }
    }
    
    public boolean isOpen() {
        return socket != null;
    }

    public boolean canSend() {
        return isOpen();
    }
    
    public boolean canSendInternal() {
        return true;
    }    

    public int connectionKind() {
        return ClientConnection.connectionTypeUDP;
    }

    public static byte opcodeAggregatedVoicePacket = 7;
    
    protected ClientConnection.MessageCallback messageCallback = null;
    protected static Map<Integer, DatagramChannel> channelMap = new HashMap<Integer, DatagramChannel>();
    protected String remoteAddr;
    protected Integer remotePort;
    protected DatagramChannel socket = null;
}
