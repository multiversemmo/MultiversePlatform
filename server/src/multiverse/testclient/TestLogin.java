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

import multiverse.server.network.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import java.net.*;
import java.io.*;

public class TestLogin {
    public TestLogin() {
    }

    public static void main(String args[]) {
	if (args.length != 5) {
	    System.err.println("usage: <server hostname> <server port> <username> <password> <check_login_or_logout (1=login - 2=logout)>");
	    System.exit(1);
	}
	
	// get the command line args
	String serverHostname = args[0];
	int serverPort = Integer.valueOf(args[1]).intValue();
	String usernameString = args[2];
	String passwordString = args[3];
	int login = Integer.valueOf(args[4]).intValue();

	//
	// setup the engine
	//
	Engine engine = new Engine();
// 	try {
	    Configuration config = new Configuration("config.xml");
	    // FIXME_TESTCLIENT
// 	    engine.readConfig(config);
// 	}
// 	catch(MVRuntimeException e) {
// 	    System.err.println("could not read config file: " + e);
// 	    System.exit(1);
// 	}

	Socket socket = null;
	try {
	    socket = new Socket(serverHostname, serverPort);
	    InputStream sin = socket.getInputStream();
	    OutputStream sout = socket.getOutputStream();

	    DataInputStream in = new DataInputStream(sin);
	    DataOutputStream out = new DataOutputStream(sout);
	    
	    // write loginAction (1)
	    out.writeInt(login);

	    byte[] username = usernameString.getBytes();
	    out.writeInt(username.length);
	    out.write(username);

	    byte[] password = passwordString.getBytes();
	    out.writeInt(password.length);
	    out.write(password);

	    if (login == 1) {
		// read success (1) failure (0)
		Log.info("login success: " + in.readInt());
		
		// read user id
		Log.info("user id: " + in.readInt());
		
		// server
		int len = in.readInt();
		byte[] handoffHostname = new byte[len];
		in.readFully(handoffHostname);
		Log.info("handoff server: " + new String(handoffHostname));
		
		// port
		Log.info("handoff server port: " + in.readInt());
	    }

	}
	catch(SocketException se) {
	    Log.exception("TestLogin.main caught SocketException", e)
	    System.exit(1);
	}
	catch(Exception e) {
	    Log.exception("TestLogin.main caught exception", e)
	    System.exit(1);
	}
	finally {
	    if (socket != null) {
		try {
		    socket.close();
		}
		catch(Exception e) {
		    socket = null;
		}
	    }
	}
    }
}
