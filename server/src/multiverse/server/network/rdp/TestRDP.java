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

import java.net.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

public class TestRDP implements ClientConnection.MessageCallback, Runnable {
    public void connectionReset(ClientConnection con) {
	// do nothing
    }

    public void processPacket(ClientConnection con, MVByteBuffer buf) {
	try {
	    Log.info("TestRDP.processPacket: GOT MESSAGE '" + 
                     buf.getString() + "'");
	}
	catch(Exception e) {
	    Log.error("got error: " + e);
	}
    }

    public void run() {
	InetAddress addr = null;
	try {
            RDPConnection con = new RDPConnection();
            con.registerMessageCallback(this);

	    addr = InetAddress.getByName(remoteHostname);
            if (addr == null) {
                Log.error("TestRDP: addr is null - exiting");
                return;
            }
	    con.open(addr, remotePort, this.localPort, true);
            int i = 0;
            while (true) {
		MVByteBuffer buf = new MVByteBuffer(200);
		buf.putString("Hello World from CLIENT! - MSG " + i++);
                buf.flip();
		con.send(buf);
                con.close();
                Thread.sleep(100000);
	    }
	}
	catch(Exception e) {
	    System.err.println("exception: " + e);
	    e.printStackTrace();
	    System.exit(1);
	}
    }

    public static void main(String args[]) {
	if (args.length != 4) {
	    System.err.println("usage: java TestRDP hostname remotePort localPort loglevel");
	    System.exit(1);
	}

        remoteHostname = args[0];
	remotePort = Integer.valueOf(args[1]).intValue();
	sLocalPort = Integer.valueOf(args[2]).intValue();
	int logLevel = Integer.valueOf(args[3]).intValue();
        Log.setLogLevel(logLevel);

        for (int i=0; i<1; i++) {
            TestRDP trdp = new TestRDP();
            trdp.localPort = sLocalPort+i;
            Thread thread = new Thread(trdp);
            thread.start();
        }
    }

    public int localPort = -1;
    
    public static TestRDP trdp = new TestRDP();
    public static String remoteHostname = null;
    public static int remotePort = -1;
    public static int sLocalPort = -1;
}
