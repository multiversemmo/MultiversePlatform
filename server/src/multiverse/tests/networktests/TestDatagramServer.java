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
import multiverse.server.util.*;
import java.util.*;
import java.nio.channels.*;
import java.net.*;

public class TestDatagramServer {

    // tests this class - writes to a hostname/port
    public static void main(String args[]) {
	if (args.length < 1 || args.length > 3) {
	    System.err.println("TestDatagramServer localport <remoteport>");
	    System.exit(1);
	}
	
	int localPort = Integer.parseInt(args[0]);
        int remotePort = 0;
        // InetSocketAddress remoteAddress = null;
	if (args.length > 1) {
            remotePort = Integer.parseInt(args[1]);
            // remoteAddress = new InetSocketAddress("127.0.0.1", remotePort);
        }
        DatagramChannel dc = null;
        try {
            dc = DatagramChannel.open();
            dc.configureBlocking(false);
            dc.socket().setReceiveBufferSize(256 * 1024);
            dc.socket().bind(new InetSocketAddress(localPort));
        }
        catch (Exception e) {
            System.out.println("Exception creating DatagramChannel: " + e.getMessage());
        }
        try {
            System.out.println("Socket receive buffer size is " + dc.socket().getReceiveBufferSize());
        }
        catch (Exception e) {
            System.out.println("Exception getting receive buffer size: " + e.getMessage());
        }
        Selector selector = null;
        try {
            selector = Selector.open();
        }
        catch (Exception e) {
            System.out.println("Exception creating Selector: " + e.getMessage());
        }
        try {
            dc.register(selector, SelectionKey.OP_READ);
        }
        catch (Exception e) {
            System.out.println("Exception registering dc with Selector: " + e + ", " + e.getMessage());
        }
        // int numReady;
        Set<SelectionKey> readyKeys = null;

        long lastTime = System.currentTimeMillis();
        int count = 0;
        int badCount = 0;
        int lastMsgNumber = -1;
        while (true) {
            do {
                try {
                    /* numReady = */ selector.select(); // this is a blocking call - thread safe
                    readyKeys = selector.selectedKeys();
                }
                catch (Exception e) {
                    System.out.println("Exception calling selector.select() or selector.selectedKeys(): " + e.getMessage());
                }
            } while (readyKeys == null || readyKeys.isEmpty());
            // get a datagramchannel that is ready
            Set<DatagramChannel> activeChannels = new HashSet<DatagramChannel>();

            Iterator<SelectionKey> iter = readyKeys.iterator();
            while (iter.hasNext()) {
                SelectionKey key = iter.next();
                Log.net("RDPServer.getActiveChannels: matched selectionkey: " + key +
                            ", isAcceptable=" + key.isAcceptable() +
                            ", isReadable=" + key.isReadable() +
                            ", isValid=" + key.isValid() +
                            ", isWritable=" + key.isWritable());
                iter.remove(); // remove from the selected key list

                if (!key.isReadable() || !key.isValid()) {
                    System.out.println("getActiveChannels: key not readable or invalid");
                }
                else {
                    DatagramChannel keydc = (DatagramChannel) key.channel();
                    activeChannels.add(keydc);
                }
            }
            if (Log.loggingNet)
                Log.net("getActiveChannels: returning " + activeChannels.size() + " active channels");
            Iterator<DatagramChannel> activeiter = activeChannels.iterator();
            while (activeiter.hasNext()) {
                DatagramChannel activedc = activeiter.next();
                MVByteBuffer receiveBuf = new MVByteBuffer(4000);
                InetSocketAddress addr;
                try {
                    addr = (InetSocketAddress) activedc.receive(receiveBuf.getNioBuf());
                } 
                catch (Exception e) {
                    System.out.println("Exception calling activedc.receive(byteBuffer): " + e.getMessage());
                    break;
                }
                if (addr == null) {
                    System.out.println("Receive socket address was null!");
                    continue;
                }
                receiveBuf.rewind();
                String s;
                try {
                    s = receiveBuf.getString();
                } 
                catch (Exception e) {
                    System.out.println("Exception calling buf.getString(): " + e.getMessage());
                    break;
                }
                //             System.out.println("s.length() is " + s.length() + ", s is: " + s);
                int msgNumber = Integer.parseInt(s.substring(0, 9).trim());
                if (lastMsgNumber != -1 && msgNumber != lastMsgNumber + 1) {
                    badCount++;
                    System.out.println("Last msg # was " + lastMsgNumber + " but current is " + msgNumber);
                }
                lastMsgNumber = msgNumber;
                int socketLocalPort = activedc.socket().getLocalPort();
                if (socketLocalPort != localPort)
                    System.out.println("socketLocalPort is " + socketLocalPort + " but localPort is " + localPort);
                if (remotePort != 0) {
                    try {
                        MVByteBuffer sendBuf = new MVByteBuffer(500);
                        String m = "" + msgNumber + "          Reply";
                        sendBuf.putString(m);
                        sendBuf.rewind();
                        activedc.send(sendBuf.getNioBuf(), addr);
                    }
                    catch(Exception e) {
                        System.out.println("When sending, got exception: " + e);
                    }
                }
            }
            count++;
            long now = System.currentTimeMillis();
            if (now - lastTime > 1000) {
                System.out.println("Received in the last " + (now - lastTime) + "ms: " + count + " messages; mismatch count " + badCount);
                lastTime = now;
                count = 0;
                badCount = 0;
            }
        }
    }
                
}
