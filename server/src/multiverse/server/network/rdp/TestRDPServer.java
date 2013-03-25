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

import multiverse.server.network.*;
import multiverse.server.util.*;

public class TestRDPServer implements ClientConnection.AcceptCallback, ClientConnection.MessageCallback {

    public void connectionReset(ClientConnection con) {
    }

    public void acceptConnection(ClientConnection con) {
//         Log.debug("TestRDPServer.acceptRDPCon: Got new Connection: " + con);
	con.registerMessageCallback(testServer);
    }

    public void processPacket(ClientConnection con, MVByteBuffer buf) {
	try {
// 	    Log.info("GOT MESSAGE: " + buf.getString());
	    
	    // send back the message
	    buf.rewind();
// 	    Log.debug("ECHOING MESSAGE BACK");
	    con.send(buf);
	}
	catch(Exception e) {
	    Log.error("got error: " + e);
	}
    }

    public static void main(String args[]) {
	if (args.length != 2) {
	    System.err.println("usage: java TestRDPServer localPort loglevel");
	    System.exit(1);
	}
	
	int port = Integer.valueOf(args[0]).intValue();
        int logLevel = Integer.valueOf(args[1]).intValue();
        Log.setLogLevel(logLevel);

	try {
	    System.out.println("starting server socket");
	    RDPServerSocket serverSocket = new RDPServerSocket();
            serverSocket.registerAcceptCallback(testServer);
	    serverSocket.bind(port);
	    
	    // register ourselves as the packet callback
// 	    TestRDPServer ts = new TestRDPServer();
// 	    con.registerCallback(ts);

// 	    Log.debug("accepting 2nd connection");
// 	    con = serverSocket.accept();
// 	    Log.debug("-------------acceptED 2nd connection");
// 	    con.registerCallback(ts);

// 	    ByteBuffer buf = new ByteBuffer(1000);

// 	    buf.putString("HELO " + 
// 			  con.getRemoteAddr() + ":" + 
// 			  con.getRemotePort());
// 	    Log.debug("----SENDING HELLO MSG");
// 	    con.send(buf);
	    while (true) {
		Thread.sleep(5000);
	    }
	}
	catch(Exception e) {
	    System.err.println("exception: " + e);
	    e.printStackTrace();
	    System.exit(1);
	}
    }
    public static TestRDPServer testServer = new TestRDPServer();
}
