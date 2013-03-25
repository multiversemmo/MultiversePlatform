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

package multiverse.testclient;

import multiverse.server.math.*;
import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import java.net.*;
import java.io.*;

public class WorldIDTest implements RDPPacketCallback {
    public WorldIDTest() {
    }

    // from interface RDPPacketCallback
    public void connectionReset(RDPConnection con) {
    }

    // from interface RDPPacketCallback
    public void processPacket(RDPConnection con, RDPPacket packet, MVByteBuffer) {
	try {
	    int msgType = buf.getInt();
	    if(msgType==2) {
		String worldId = buf.getString();
		
		Log.debug("got a name resolve response msg");
		int status = buf.getInt();
		if (status == 0) {
		    Log.debug("name lookup failed");
		    return;
		}
		String hostname = buf.getString();
		int port = buf.getInt();
		Log.info("worldid=" + worldId + ", hostname=" + hostname + ", port=" + port);
	    }
	}
	catch(Exception e) {
	    Log.error("exception while processing packet: " + e);
	}
    }

    public static void main(String args[]) {
	if (args.length != 4) {
	    System.err.println("usage: <localport> <masterserver> <masterserver_port> <worldID>");
	    System.exit(1);
	}

	// get the command line args
	int localPort = Integer.valueOf(args[0]).intValue();
	String loginServer = args[1];
	int loginPort = Integer.valueOf(args[2]).intValue();
	String worldID = args[3];
	System.out.println("listening on port: " + localPort);
	System.out.println("master server host: " + loginServer);
	System.out.println("master server port: " + loginPort);
	System.out.println("world id: " + worldID);
// 	System.out.println("config file: " + configFile);

	try {
// 	    // start the engine - this reads the message handler mappings, etc
// 	    Engine engine = new Engine();
// 	    Configuration config = new Configuration(configFile);
// 	    engine.readConfig(config);

	    // make an RDP connection & register a callback handler
	    Log.info("MAKING RDP CONNECTION");
	    RDPConnection con = new RDPConnection();
	    WorldIDTest callbackHandler = new WorldIDTest();
	    con.registerCallback(callbackHandler);
	    Log.info("REGISTERED CALLBACK");

	    // make the connection
            if (Log.loggingDebug)
                Log.debug("opening rdp connection to " + loginServer + ":" + loginPort);
	    con.open(loginServer, loginPort, localPort, true);
	    
	    // make a name lookup message and send it
	    MVByteBuffer buf = new MVByteBuffer(200);
	    buf.putInt(0); // name resolution type message
	    buf.putString(worldID); // worldID
	    buf.flip();
	    con.send(buf.copyBytes());
	    Log.debug("sent name lookup message to server");
	    
// 	    // wait for receive message
// 	    do {
// 		msg = new Message(60000);
// 		System.out.println("waiting for loginresponse");
// 		msgServer.read(reader, msg);
// 		System.out.println("received a response - msg id=" +
// 				   msg.getMessageType());
// 	    } while (msg.getMessageType() != 4);

// 	    System.out.println("Got Login Response - sending com msg");
// 	    msg = MessageHelper.mkComMessage(uid, "Hello World");
// 	    msgServer.write(writer, msg.getByteBuffer());
// 	    System.out.println("sent com mesg");

// 	    // send a movement message
// 	    Point point = new Point(10, 10, 10);
// 	    msg = MessageHelper.mkLocMessage(uid, point);
// 	    msgServer.write(writer, msg.getByteBuffer());
// 	    System.out.println("sent loc mesg");

// 	    // main loop
// 	    while (true) {
// 		msg = new Message(60000);
// 		System.out.println("waiting for message");
// 		msgServer.read(reader, msg);
// 		System.out.println("received a message - msg id=" +
// 				   msg.getMessageType());
// 	    }
// 	}
// 	catch(SocketException se) {
// 	    Log.exception("WorldIDTest.main caught SocketException", se);
// 	    se.printStackTrace();
// 	    System.exit(1);
// 	}
	}
	catch(Exception e) {
	    Log.exception("WorldIDTest.main caught exception", e);
	    System.exit(1);
	}
    }

//     public static void setWorldServer(String worldServer) {
// 	WorldIDTest.worldServer = worldServer;
//     }
//     public static String getWorldServer() {
// 	return worldServer;
//     }

//     public static void setWorldServerPort(int port) {
// 	WorldIDTest.worldServerPort = port;
//     }
//     public static int getWorldServerPort() {
// 	return worldServerPort;
//     }

    // the permanent user id in the database
    public static void setUserId(int id) {
	WorldIDTest.uid = id;
    }
    public static int getUserId() {
	return WorldIDTest.uid;
    }

    public static void setOid(int id) {
	WorldIDTest.oid = id;
    }
    public static int getOid() {
	return WorldIDTest.oid;
    }

    public synchronized void setState(int state) {
	localState = state;
    }
    public synchronized int getState() {
	return localState;
    }

    private static String worldServer = null;
    private static int worldServerPort = -1;
    private static int uid = -1;
    private static int oid = -1;

    private static int NEW = 0;
    private static int LOGGED_IN = 1;

    // local state info for the client connection
    private int localState = NEW;
}
