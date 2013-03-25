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

import java.net.*;
import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.util.*;
import java.util.concurrent.locks.*;

public class TestRDPClient implements ClientConnection.MessageCallback, Runnable {
    public void connectionReset(ClientConnection con) {
	// do nothing
    }
    
    public void processPacket(ClientConnection con, MVByteBuffer buf) {
        lock.lock();
        try {
            long currentTime = System.currentTimeMillis();
            long interval = currentTime - lastReceiveCounterResetTime;
            if (interval > 1000) {
                lastReceiveCounterResetTime = currentTime;
                receiveCount = 0;
                System.out.println("Received " + receiveCount + " messages" + " in the last " + interval + "ms");
            }                    
            receiveCount++;
            try {
                Log.info("TestRDPClient.processPacket: GOT MESSAGE '" + 
                    buf.getString() + "'");
            }
            catch(Exception e) {
                Log.error("got error: " + e);
            }
        } finally {
            lock.unlock();
        }
    }

    public void run() {
	InetAddress addr = null;
	try {
            RDPConnection con = new RDPConnection();
            con.registerMessageCallback(this);

	    addr = InetAddress.getByName(remoteHostname);
            if (addr == null) {
                Log.error("TestRDPClient: addr is null - exiting");
                return;
            }
	    System.out.println("Connecting to sockaddr " + addr + ", remotePort " + remotePort + ", localPort " + localPort);
            con.open(addr, remotePort, this.localPort, !unsequenced);
            System.out.println("Connection successful - - max unacked packets is " + con.getMaxSendUnacks());
            int runningCounter = 0;
            int sentCount = 0;
            long lastTime = System.currentTimeMillis();
            for (int i=0; i<messageCount; i++) {
                long currentTime = System.currentTimeMillis();
                long interval = currentTime - lastTime;
                if (interval > 1000) {
                    lastTime = currentTime;
                    System.out.println("Sent " + sentCount + " msgs" + " in " + interval + "ms");
                    sentCount = 0;
                }
                MVByteBuffer buf = new MVByteBuffer(200);
                buf.putString("Hello World from CLIENT! - MSG " + runningCounter++);
                buf.flip();
                // If the 
                while (!con.sendIfPossible(buf)) {
                    Log.net("Too many unacked - sleep(10)");
                    Thread.sleep(10);
                }
                sentCount++;
            }
            con.close();
	}
	catch(Exception e) {
	    System.err.println("exception: " + e);
	    e.printStackTrace();
	    System.exit(1);
	}
    }

    public static void main(String args[]) {
	if (args.length < 3 && args.length > 4) {
	    System.err.println("usage: java TestRDPClient hostname remotePort localPort <messageCount> <unsequenced>");
	    System.exit(1);
	}

        remoteHostname = args[0];
	remotePort = Integer.valueOf(args[1]).intValue();
	sLocalPort = Integer.valueOf(args[2]).intValue();
        if (args.length >= 4)
            messageCount = Integer.valueOf(args[3]).intValue();
        if (args.length == 5)
            unsequenced = true;
        // Log.setLogFilename("TestRDPClient.txt");

        if (unsequenced)
            System.out.println("The connection will be opened non-sequentially");
        
        for (int i=0; i<1; i++) {
            TestRDPClient trdp = new TestRDPClient();
            trdp.localPort = sLocalPort+i;
            Thread thread = new Thread(trdp);
            thread.start();
        }
    }

    public int localPort = -1;
    
    public static TestRDPClient trdp = new TestRDPClient();
    public static String remoteHostname = null;
    public static int remotePort = -1;
    public static int sLocalPort = -1;
    public static int messageCount = 1;
    public static int receiveCount = 0;
    public static boolean unsequenced = false;
    public static long lastReceiveCounterResetTime = System.currentTimeMillis();
    static Lock lock = LockFactory.makeLock("TestRDPClientLock");
}
