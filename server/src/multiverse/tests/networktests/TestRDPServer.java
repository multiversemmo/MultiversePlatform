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

package multiverse.tests.networktests;

import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.util.*;
import java.util.concurrent.locks.*;

public class TestRDPServer implements ClientConnection.AcceptCallback, ClientConnection.MessageCallback, Runnable {

    public void connectionReset(ClientConnection con) {
    }

    public void acceptConnection(ClientConnection con) {
//         Log.debug("TestRDPServer.acceptRDPCon: Got new Connection: " + con);
	con.registerMessageCallback(testServer);
    }

    public void run() {
	try {
            while (true)
                Thread.sleep(1000);
        }
	catch(Exception e) {
	    System.err.println("exception: " + e);
	    e.printStackTrace();
	    System.exit(1);
	}
    }
    
    public void processPacket(ClientConnection con, MVByteBuffer buf) {
        lock.lock();
        try {
            long currentTime = System.currentTimeMillis();
            long interval = currentTime - lastReceiveCounterResetTime;
            if (interval > 1000) {
                Log.net("WTF: " + interval);
                lastReceiveCounterResetTime = currentTime;
                System.out.println("Received " + receiveCount + " messages in the last " + interval + "ms");
                receiveCount = 0;
            }
            receiveCount++;
        } finally {
            lock.unlock();
        }
    }

    public static void main(String args[]) {
	if (args.length != 2) {
	    System.err.println("usage: java TestRDPServer localPort");
	    System.exit(1);
	}
	
	int port = Integer.valueOf(args[0]).intValue();
        // Log.setLogFilename("TestRDPServer.txt");
        Log.setLogLevel(4);

	try {
	    System.out.println("Setting up server socket with port " + port);
	    RDPServerSocket serverSocket = new RDPServerSocket();
            serverSocket.registerAcceptCallback(testServer);
	    serverSocket.bind(port);
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
    public static int receiveCount = 0;
    public static long lastReceiveCounterResetTime = System.currentTimeMillis();
    static Lock lock = LockFactory.makeLock("TestRDPServerLock");
}
