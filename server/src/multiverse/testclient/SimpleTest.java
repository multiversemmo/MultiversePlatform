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

public class SimpleTest  {
    public SimpleTest() {
    }

    // returns success
    static boolean tcpLogin(String loginServer, int port,
			    String username, String password) {
	Socket socket = null;
	try {
	    socket = new Socket(loginServer, port);
	    InputStream sin = socket.getInputStream();
	    OutputStream sout = socket.getOutputStream();
	    
	    DataInputStream in = new DataInputStream(sin);
	    DataOutputStream out = new DataOutputStream(sout);
	    
	    // Send name and password to login server
	    out.writeInt(username.length());
	    out.write(username.getBytes());
	    
	    out.writeInt(password.length());
	    out.write(password.getBytes());
	    
	    // read success (1) failure (0)
	    int success = in.readInt();
	    Log.info("login success: " + success);
	    if (success != 1) {
		Log.error("login failed");
		return false;
	    }
	    
	    // read in token length
	    int tokenLen = in.readInt();

	    // read user id
	    int authToken = in.readInt();
	    int userId = ~authToken;
	    setUserId(userId);
	    Log.info("user id: " + getUserId());
	    Log.info("auth token: " + authToken);
	}
	catch(Exception e) {
	    Log.error("failed tcp login");
	    return false;
	}
	finally {
	    try {
		if (socket != null) {
		    socket.close();
		}
	    }
	    catch(Exception e) {
		Log.error("cannot close socket: " + e);
	    }
	}
	Log.debug("tcplogin succeeded");
	return true;
    }

    public static void main(String args[]) {
	if (args.length != 5) {
	    System.err.println("usage: <localport> <masterserver> <masterserver_port> <username> <password>");
	    System.exit(1);
	}

	// get the command line args
	int localPort = Integer.valueOf(args[0]).intValue();
	String loginServer = args[1];
	int loginPort = Integer.valueOf(args[2]).intValue();
	String username = args[3];
	String password = args[4];

	System.out.println("listening on port: " + localPort);
	System.out.println("master server host: " + loginServer);
	System.out.println("master server port: " + loginPort);
	System.out.println("username: " + username);
	System.out.println("password: " + password);

	try {
	    // log into the tcp masterserver first
	    Log.info("LOGGING INTO TCP MASTER SERVER");
	    if (! tcpLogin(loginServer, loginPort, username, password)) {
		Log.error("login failed");
		System.exit(1);
	    }
	}
	catch(Exception e) {
	    Log.exception("SimpleTest.main caught exception", e)
	    System.exit(1);
	}
    }

    public static void setWorldServer(String worldServer) {
	SimpleTest.worldServer = worldServer;
    }
    public static String getWorldServer() {
	return worldServer;
    }

    public static void setWorldServerPort(int port) {
	SimpleTest.worldServerPort = port;
    }
    public static int getWorldServerPort() {
	return worldServerPort;
    }

    // the permanent user id in the database
    public static void setUserId(int id) {
	SimpleTest.uid = id;
    }
    public static int getUserId() {
	return SimpleTest.uid;
    }

    public static void setOid(int id) {
	SimpleTest.oid = id;
    }
    public static int getOid() {
	return SimpleTest.oid;
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
