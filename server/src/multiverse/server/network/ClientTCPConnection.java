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
import multiverse.msgsys.*;
import multiverse.server.util.*;

public class ClientTCPConnection extends ClientConnection {
    
    public ClientTCPConnection(ClientTCPMessageIO clientTCPMessageIO) {
        this.clientTCPMessageIO = clientTCPMessageIO;
        agentInfo = new AgentInfo();
        agentInfo.association = this;
    }
    
    public ClientTCPConnection(SocketChannel socketChannel) {
        agentInfo = new AgentInfo();
        agentInfo.association = this;
        initializeFromSocketChannel(socketChannel);
    }
    
    public ClientTCPConnection(ClientTCPMessageIO clientTCPMessageIO, SocketChannel socketChannel, ClientConnection.MessageCallback messageCallback) {
        this.clientTCPMessageIO = clientTCPMessageIO;
        agentInfo = new AgentInfo();
        agentInfo.association = this;
        this.messageCallback = messageCallback;
        initializeFromSocketChannel(socketChannel);
    }
    
    protected void initializeFromSocketChannel(SocketChannel socketChannel) {
        agentInfo.socket = socketChannel;
        agentInfo.agentId = -1;
        agentInfo.agentName = null;
        agentInfo.agentIP = null;
        agentInfo.agentPort = -1;
        agentInfo.outputBuf = new MVByteBuffer(8192);
        agentInfo.inputBuf = new MVByteBuffer(8192);
    }
    
    // Return a string containing the IP and port
    public String IPAndPort() {
        if (agentInfo.socket != null)
            return "TCP(" + agentInfo.socket.socket().getRemoteSocketAddress() + ")";
        else
            return "TCP(null)";
    }

    public void registerMessageCallback(ClientConnection.MessageCallback messageCallback) {
        this.messageCallback = messageCallback;
    }
    
    public ClientConnection.MessageCallback getMessageCallback()
    {
        return messageCallback;
    }

    public void send(MVByteBuffer buf) {
        if (logMessageContents && Log.loggingNet)
            Log.net("ClientTCPConnection.send: length " + buf.limit() + ", packet " + 
                DebugUtils.byteArrayToHexString(buf));
        lock.lock();
        try {
            if (PacketAggregator.usePacketAggregators) {
                if (!packetAggregator.addMessage(buf)) {
                    if (isOpen())
                        Log.error("ClientTCPConnection.send: for con " + this +
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
        clientTCPMessageIO.addToOutputWithLength(buf, agentInfo);
        return true;
    }
    
    public boolean sendIfPossible(MVByteBuffer buf) {
        send(buf);
        return true;
    }
    
    public int sendMultibuf(List<MVByteBuffer> subMessages, int currentSize) {
        MVByteBuffer multiBuf = new MVByteBuffer(currentSize);
        int size = subMessages.size();
        for (MVByteBuffer buf : subMessages)
            multiBuf.putByteBuffer(buf);
        subMessages.clear();
        multiBuf.rewind();
        clientTCPMessageIO.addToOutput(multiBuf, agentInfo);
        aggregatedSends++;
        PacketAggregator.allAggregatedSends++;
        sentMessagesAggregated += size;
        PacketAggregator.allSentMessagesAggregated += size;
        if (Log.loggingNet)
            Log.net("ClientTCPConnection.sendMultiBuf: multiBuf size is " + multiBuf.limit());
        return 0;
    }
    
    public void open(String hostname, int remotePort) {
        try {
            SocketChannel socket = SocketChannel.open(new InetSocketAddress(hostname, remotePort));
            socket.configureBlocking(false);
            socket.socket().setTcpNoDelay(true);
            initializeFromSocketChannel(socket);
        }
        catch (Exception ex) {
            Log.info("Could not connect to host "+
                    hostname + ":" + remotePort + " " + ex);
        }
    }
    
    public void connectionReset() {
        boolean call = false;
        synchronized (this) {
            if (! connectionResetCalled) {
                call = true;
                connectionResetCalled = true;
            }
        }
        if (call && messageCallback != null)
            messageCallback.connectionReset(this);
    }

    public void close() {
        if (agentInfo.socket != null) {
            try {
                agentInfo.socket.close();
                clientTCPMessageIO.outputReady();
                connectionReset();
            } catch (java.io.IOException ex) { }
        }
    }
    
    public boolean isOpen() {
        return agentInfo.socket != null;
    }

    public boolean canSend() {
        return isOpen();
    }
    
    public boolean canSendInternal() {
        return true;
    }    
    public int connectionKind() {
        return ClientConnection.connectionTypeTCP;
    }

    public AgentInfo getAgentInfo() {
        return agentInfo;
    }

    private ClientConnection.MessageCallback messageCallback = null;
    private AgentInfo agentInfo;
    private ClientTCPMessageIO clientTCPMessageIO = null;
    private boolean connectionResetCalled = false;
}
