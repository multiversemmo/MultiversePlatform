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

import java.util.List;
import java.util.concurrent.locks.*;
import multiverse.server.util.*;

public abstract class ClientConnection {
    
    public ClientConnection() {
        if (PacketAggregator.usePacketAggregators)
            packetAggregator = new PacketAggregator(this);
    }

    abstract public void registerMessageCallback(MessageCallback pcallback);
    
    abstract public void connectionReset();

    // This version of send may do aggregation
    abstract public void send(MVByteBuffer buf);

    // This version of send puts the buf on the wire immediately
    abstract public boolean sendInternal(MVByteBuffer buf);

    abstract public boolean sendIfPossible(MVByteBuffer buf);
    
    // This method sends multiple messages.
    abstract public int sendMultibuf(List<MVByteBuffer> subMessages, int currentSize);
    
    abstract public void open(String hostname, int remotePort);
    abstract public void close();

    abstract public int connectionKind();
    
    abstract public boolean isOpen();
    // This version of canSend() asserts the connection lock if it
    // needs to do anything non-trivial
    abstract public boolean canSend();
    // This version of canSend() assumes that the caller has asserted
    // the connection lock
    abstract public boolean canSendInternal();

    // Return a string containing the IP and port
    abstract public String IPAndPort();
    
    public Object getAssociation()
    {
        return association;
    }

    public void setAssociation(Object object)
    {
        association = object;
    }

    public Lock getLock() {
        return lock;
    }
    
    public PacketAggregator getAggregator() {
        return packetAggregator;
    }

    public interface MessageCallback {
        
        public void processPacket(ClientConnection con, MVByteBuffer buf);
        public void connectionReset(ClientConnection con);
    }

    public interface AcceptCallback {
        
        public void acceptConnection(ClientConnection con);
        
    }

    public String toString() {
        return IPAndPort();
    }
    
    public static boolean getLogMessageContents() {
        return logMessageContents;
    }
    
    public static void setLogMessageContents(boolean logMessageContents) {
        ClientConnection.logMessageContents = logMessageContents;
    }

    private Object association;
    protected PacketAggregator packetAggregator = null;

    /**
     * Some per-connection statistics
     */
    public long aggregatedSends = 0;
    public long sentMessagesAggregated = 0;
    public long unaggregatedSends = 0;

    public long aggregatedReceives = 0;
    public long receivedMessagesAggregated = 0;
    public long unaggregatedReceives = 0;
    
    public static final int connectionTypeRDP = 1;
    public static final int connectionTypeTCP = 2;
    public static final int connectionTypeUDP = 2;
    
    /**
     * Set this to true to log message contents
     */
    protected static boolean logMessageContents = false;

    protected transient Lock lock = LockFactory.makeLock("BasicConnectionLock");
}
