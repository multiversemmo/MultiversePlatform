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
import java.nio.channels.*;
import java.net.*;

public class TestDatagramClient {

    // tests this class - writes to a hostname/port
    public static void main(String args[]) {
	if (args.length < 3 || args.length > 4) {
	    System.err.println("TestDatagramClient remoteport localport replymode <targetCount>");
	    System.exit(1);
	}
	
        int remotePort = Integer.parseInt(args[0]);
	int localPort = Integer.parseInt(args[1]);
	boolean replyMode = Integer.parseInt(args[2]) != 0;
        int targetCount = 0;
        if (args.length == 4)
            targetCount = Integer.parseInt(args[3]);
        String message = "Now is the time for all good men to come to the aid of their party.  " + 
                         "Now is the time for all good men to come to the aid of their party.";
        DatagramChannel dc = null;
        try {
            dc = DatagramChannel.open();
            dc.configureBlocking(replyMode);
            dc.socket().setReceiveBufferSize(256 * 1024);
            dc.socket().bind(new InetSocketAddress(localPort));
        }
        catch (Exception e) {
            
        }
        InetSocketAddress remoteAddress = new InetSocketAddress("127.0.0.1", remotePort);
        try {
            System.out.println("Socket receive buffer size is " + dc.socket().getReceiveBufferSize());
        }
        catch (Exception e) {
            System.out.println("Exception getting receive buffer size: " + e.getMessage());
        }
        long lastTime = System.currentTimeMillis();
        int badCount = 0;
        int count = 0;
        int lastMsgNumber = 0;
        while (true) {
            try {
                MVByteBuffer sendBuf = new MVByteBuffer(500);
                String m = "" + lastMsgNumber + "         " + message;
                sendBuf.putString(m);
                sendBuf.rewind();
                dc.send(sendBuf.getNioBuf(), remoteAddress);
                if (replyMode) {
                    MVByteBuffer receiveBuf = new MVByteBuffer(4000);
                    InetSocketAddress addr;
                    try {
                        addr = (InetSocketAddress) dc.receive(receiveBuf.getNioBuf());
                    } 
                    catch (Exception e) {
                        System.out.println("Exception calling dc.receive(byteBuffer): " + e.getMessage());
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
                    if (msgNumber != lastMsgNumber) {
                        badCount++;
                        System.out.println("Last msg # was " + lastMsgNumber + " but reply # is " + msgNumber);
                    }
                }
                lastMsgNumber++;
                count++;
            }
            catch(Exception e) {
                System.out.println("Got exception: " + e);
                System.exit(1);
            }

            long now = System.currentTimeMillis();
            if (now - lastTime >= 1000) {
                System.out.println("Received in the last " + (now - lastTime) + "ms: " + count + " messages; mismatch count " + badCount);
                lastTime = now;
                count = 0;
            }
            else if (targetCount > 0 && count >= targetCount) {
                try {
                    Thread.sleep(lastTime + 1000 - now);
                }
                catch (Exception e) {
                    System.out.println("Exception during Thread.sleep(): " + e.getMessage());
                }
            }
        }
    }
                
}
