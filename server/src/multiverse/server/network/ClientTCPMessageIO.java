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

import java.io.*;
import java.nio.channels.*;
import multiverse.msgsys.*;
import multiverse.server.util.*;

public class ClientTCPMessageIO extends MessageIO implements TcpAcceptCallback, MessageIO.Callback {

    // Used for clients that do not accept new connections
    protected ClientTCPMessageIO() {
        super();
        initialize(this);        
    }
    
    // Provide a constructor that permits setting the number of bytes in a message length
    protected ClientTCPMessageIO(int messageLengthByteCount) {
        super(messageLengthByteCount);
        initialize(this);        
    } 
    
    // Used for clients that can accept new connections
    protected ClientTCPMessageIO(Integer port, ClientConnection.MessageCallback messageCallback, ClientConnection.AcceptCallback acceptCallback) {
        super();
        this.messageCallback = messageCallback;
        this.acceptCallback = acceptCallback;
        initialize(this);
        if (port != null)
            startListener(port);
    }
    
    // Used for clients that can accept new connections.  Provide a constructor 
    // that permits setting the number of bytes in a message length
    protected ClientTCPMessageIO(int messageLengthByteCount, Integer port, ClientConnection.MessageCallback messageCallback, ClientConnection.AcceptCallback acceptCallback) {
        super(messageLengthByteCount);
        this.messageCallback = messageCallback;
        this.acceptCallback = acceptCallback;
        initialize(this);
        if (port != null)
            startListener(port);
    }
   
    public static ClientTCPMessageIO setup() {
        return new ClientTCPMessageIO();
    }
    
    public static ClientTCPMessageIO setup(Integer port, ClientConnection.MessageCallback messageCallback) {
        return new ClientTCPMessageIO(port, messageCallback, null);
    }
    
    public static ClientTCPMessageIO setup(Integer port,
        ClientConnection.MessageCallback messageCallback,
        ClientConnection.AcceptCallback acceptCallback)
    {
        return new ClientTCPMessageIO(port, messageCallback, acceptCallback);
    }
    
    public static ClientTCPMessageIO setup(int messageLengthByteCount,
        Integer port,
        ClientConnection.MessageCallback messageCallback, 
        ClientConnection.AcceptCallback acceptCallback)
    {
        return new ClientTCPMessageIO(messageLengthByteCount, port,
            messageCallback, acceptCallback);
    }
    
    public void handleMessageData(int length, MVByteBuffer buf, AgentInfo agentInfo) {
        ClientTCPConnection con = (ClientTCPConnection)(agentInfo.association);
        if (length == -1 || buf == null) {
            con.connectionReset();
            return;
        }
        
        // Get the buf without the initial length
        MVByteBuffer packet = buf.cloneAtOffset(0, length);
        
        // Now hand it off to be processed
        if (con.getMessageCallback() != null) {
            con.getMessageCallback().processPacket(con, packet);
        }
    }

    protected void startListener(int port) {
        try {
            openListener(port);
            listener.start();
        }
        catch (Exception e) {
            Log.exception("Could not bind ClientTCPMessageIO to port " + port, e);
        }
    }
    
    public int getListenerPort() {
        return listener.getPort();
    }

    public void openListener(int port) throws IOException {
        if (listener != null)
            return;

        listener = new TcpServer();
        listener.bind(port);
        listener.registerAcceptCallback(this);
        // Don't put into selector (kernel will accept packets,
        // but we won't see them until we select/accept)
    }

    // TcpServer callback
    public void onTcpAccept(SocketChannel agentSocket) {
        try {
            agentSocket.socket().setTcpNoDelay(true);
            agentSocket.configureBlocking(false);
            ClientTCPConnection con = new ClientTCPConnection(this, agentSocket, messageCallback);
            if (acceptCallback != null)
                acceptCallback.acceptConnection(con);
            addAgent(con.getAgentInfo());
        } catch (IOException ex) {
            Log.exception("Agent listener", ex);
        }
    }

    /**
     * The default implmentation of ClientConnection.AcceptCallback
     */
    public void acceptConnection(ClientConnection con) {
    }
    
    private ClientConnection.MessageCallback messageCallback;
    private ClientConnection.AcceptCallback acceptCallback;
    private TcpServer listener = null;
}

    
