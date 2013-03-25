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

package multiverse.server.network.rdp;

import java.nio.channels.*;
import multiverse.server.network.*;

/**
 * the RDPServerSocket class represents a socket listening for incoming
 * rdp connections on a given port
 */
public class RDPServerSocket {
    public RDPServerSocket() {
    }

    public void bind() throws java.net.BindException, java.io.IOException {
        this.port = -1;
        this.bind(null);
    }

    /**
     * listens for new connections on this local port.
     * throws bindexception if the port is unavailable.
     */
    public void bind(int port) throws java.net.BindException, java.io.IOException {
        this.bind(new Integer(port), defaultReceiveBufferSize);
    }

    public void bind(Integer port) throws java.net.BindException, java.io.IOException {
        this.bind(port, defaultReceiveBufferSize);
    }

    public void bind(Integer port, int receiveBufferSize) throws java.net.BindException, java.io.IOException {
        if (port < 0) {
            throw new java.net.BindException("RDPServerSocket: port is < 0");
        }
        DatagramChannel dc = RDPServer.bind(port, receiveBufferSize);
        this.dc = dc;
        this.port = dc.socket().getLocalPort();
        RDPServer.registerSocket(this, dc);
    }

    public void registerAcceptCallback(ClientConnection.AcceptCallback cb) {
	acceptCallback = cb;
    }

    ClientConnection.AcceptCallback getAcceptCallback() {
	return acceptCallback;
    }

    /**
     * returns the port number for this socket.
     * returns -1 if the port is not set
     */
    public int getPort() {
        return port;
    }

    public DatagramChannel getDatagramChannel() {
        return dc;
    }
    void setDatagramChannel(DatagramChannel dc) {
        this.dc = dc;
    }
    
    protected int port = -1;
    ClientConnection.AcceptCallback acceptCallback = null;
    DatagramChannel dc = null;
    protected static int defaultReceiveBufferSize = 64 * 1024;
}








// 	// create a datagram channel to be used in the new rdpconnection
// 	DatagramChannel dc;
// 	try {
// 	    dc = DatagramChannel.open();
// 	    dc.configureBlocking(false);
// 	    if (port == null) {
// 		dc.socket().bind(null);
// 	    }
// 	    else {
// 		dc.socket().bind(new InetSocketAddress(port));
// 	    }
// 	}
// 	catch(Exception e) {
// 	    throw new java.net.BindException(e.toString());
// 	}

// 	// make the RDPConnection - this connection is used to listen
// 	// for new connections
// 	port = dc.socket().getLocalPort();

// 	serverCon = new RDPConnection();
// 	serverCon.initConnection(); // sets init seqnum, next seq #, unackd
// 	serverCon.setDatagramChannel(dc);
// 	serverCon.setLocalPort(port);
// 	if (this.getIsListening()) {
// 	    // we are in open passive state
// 	    serverCon.setState(RDPConnection.LISTEN); 
// 	}
// 	else {
// 	    serverCon.setState(RDPConnection.CLOSED);
// 	}
// 	try {
// 	    RDPServer.putServerSocket(port, this, serverCon);
// 	}
// 	catch(Exception e) {
// 	    throw new java.net.BindException(e.toString());
// 	}
// 	this.localPort = port;
// 	}
// 	finally {
// 	    lock.unlock();
// 	}
//     }
    
//     public RDPConnection accept() throws InterruptedException {
// 	try {
// 	    lock.lock();
// 	    while (acceptQueue.isEmpty()) {
// 		Log.net("serversocket waiting on accept()");
// 		acceptQueueNotEmpty.await();
// 	    }
// 	    RDPConnection con = (RDPConnection) acceptQueue.removeFirst();
// 	    if (con == null) {
// 		throw new MVRuntimeException("accept(): connection is null");
// 	    }
// 	    return con;
// 	}
// 	finally {
// 	    lock.unlock();
// 	}
//     }
    

//     public void registerAcceptCallback(RDPAcceptCallback cb) {
// 	acceptCallback = cb;
//     }

//      RDPAcceptCallback getAcceptCallback() {
// 	return acceptCallback;
//     }

//     // called by rdpserver when there is a new connection established
//     void addNewClient(RDPConnection con) {
// 	lock.lock();
// 	try {
// 	    acceptQueue.addLast(con);
// 	    acceptQueueNotEmpty.signal();
// 	}
// 	finally {
// 	    lock.unlock();
// 	}
//     }

//     public void close() {
// 	throw new MVRuntimeException("not implemented");
//     }

//     public int getPort() {
// 	return localPort;
//     }

//     DatagramChannel getDatagramChannel() {
// 	return serverCon.getDatagramChannel();
//     }

// //     UDPReader getUDPReader() {
// // 	return serverCon.getUDPReader();
// //     }

//     void setIsListening(boolean val) {
// 	isListening = val;
//     }
//     boolean getIsListening() {
// 	return isListening;
//     }

//     // 
//     // Variables / Fields
//     //
//     private int localPort = -1;

//     // not all server sockets are accepting new connections
//     // some server connections are made for outgoing client connections
//     // they are used for listening to incoming packets from the
//     // established connection
//     private boolean isListening = false;

//     // the listening server connection, this is not in the server map
//     RDPConnection serverCon = null;

//     // list of new client connections
//     LinkedList acceptQueue = new LinkedList();

//     RDPAcceptCallback acceptCallback = null;

//     MVLock lock = new MVLock("RDPServerSocketLock");

//     /**
//      * waiting on this condition means you are waiting for
//      * the acceptQueue to have something in it
//      */
//     Condition acceptQueueNotEmpty = lock.newCondition();
// }
