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
import multiverse.server.network.rdp.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.events.*;
import multiverse.server.objects.*;
import multiverse.testclient.*;
import org.mozilla.javascript.*;
import java.net.*;
import java.io.*;

public class TestClient {
    public TestClient() {
    }

    public static void setManagerHost(String name) {
	managerHost = name;
    }

    public static void setManagerPort(int port) {
	managerPort = port;
    }

    public static void setUserFocus(TestUser user) {
	userFocus = user;
    }
    public static TestUser getUserFocus() {
	return userFocus;
    }

    public static void setTarget(MVObject user) {
	targetFocus = user;
    }
    public static MVObject getTarget() {
	return targetFocus;
    }

    public static TestUser loginUser(int clientNumber, int clientPort) {
	TestUser user = null;

	try {
	    // Create TCP socket on world manager
	    Socket socket = new Socket(managerHost, managerPort);
	    InputStream sin = socket.getInputStream();
	    OutputStream sout = socket.getOutputStream();

	    DataInputStream in = new DataInputStream(sin);
	    DataOutputStream out = new DataOutputStream(sout);

	    // write login token
	    out.writeInt(4);
	    out.writeInt(~clientNumber);

	    // read success (1) failure (0)
	    int success = in.readInt();
	    Log.info("login success: " + success);

	    // read world token
	    int len = in.readInt();
	    byte[] worldTokenBuf = new byte[len];
	    in.readFully(worldTokenBuf);
	    String worldToken = new String(worldTokenBuf);
	    Log.info("world token: " + worldToken);

	    // read number of characters
	    int numCharacters = in.readInt();
	    Log.info("number of characters: " + numCharacters);

	    // ignore all but first character (no loop here)
	    if(numCharacters > 0) {
		// read user id
		long clientID = in.readLong();
		Log.info("user id: " + clientID);

		// read character name
		len = in.readInt();
		byte[] characterNameBuf = new byte[len];
		in.readFully(characterNameBuf);
		String characterName = new String(characterNameBuf);
		Log.info("character name: " + characterName);

		// proxy server name
		len = in.readInt();
		byte[] proxyNameBuf = new byte[len];
		in.readFully(proxyNameBuf);
		String proxyName = new String(proxyNameBuf);
		Log.info("proxy server name: " + proxyName);
	    
		// proxy port number
		int proxyPort = in.readInt();
		Log.info("proxy server port: " + proxyPort);

		if(success > 0) {
		    Log.info("Opening reader port " + clientPort);

		    // Create RDP connection for fake client
		    RDPConnection con = new RDPConnection();
		    ClientServer callbackHandler = new ClientServer();
		    con.registerCallback(callbackHandler);
		    con.open(proxyName, proxyPort, clientPort, true);

		    // create and initialize user object
		    user = new TestUser(clientID);
		    user.setName(characterName);
		    user.setRemoteObjectConnection(con);
		    user.setWorldServerConnection(con);
		    MVObject.addObject(user);

		    // Store in list of TestUsers
		    ObjectManager.mapPut(user, con);

		    // Loc and Oid will be set in LoginResp and NewMob handlers
                    if (Log.loggingDebug)
                        Log.debug("Created user " + user);

		    // make a Login Message and send it
		    user.sendEvent(new LoginEvent(user));
		    System.out.println(characterName + " logged in with uid " + clientID);

		    // wait for NewMob messages to get returned and processed
		    Thread.sleep(1000);
		}
	    }
	    else {
		System.out.println(clientNumber + " does not have a character");
	    }

	    // done with world manager
	    socket.close();
	}
	catch(SocketException se) {
	    Log.exception("TestClient.loginUser caught SocketException", e)
	    System.exit(1);
	}
	catch(Exception e) {
	    Log.exception("TestClient.loginUser caught exception", e)
	    System.exit(1);
	}

	return user;
    }

    public static void main(String args[]) {
	if (args.length < 1) {
	    System.err.println("usage: <setup script> [event_mapping_script] [behavior script]");
	    System.exit(1);
	}
	
	//
	// setup the engine
	//
	Engine engine = new Engine();

	//
	// load in local javascript config file
	//
	scriptManager = new ScriptManager();
	String localScriptFile = args[0];
	System.out.println("Engine: reading in script: " +
			   localScriptFile);
	try {
	    scriptManager.init();

	    File f = new File(localScriptFile);
	    if (f.exists()) {
		System.out.println("Running script: " + args[0]);
		Object obj = scriptManager.runFile(localScriptFile);
                if (Log.loggingDebug)
                    Log.debug("script result: " + scriptManager.getResultString(obj));
	    }
	    else {
		System.out.println("couldnt find local script file: " +
				   localScriptFile);
		System.exit(1);
	    }
	}
	catch(Exception e) {
	    Log.exception("TestClient.main caught exception running local javascript: " + localScriptFile, e)
	    System.exit(1);
	}

	// 
	// load the event mapping file
	//
	try {
	    if (args.length > 1) {
		System.out.println("Running script: " + args[1]);
		Object obj = scriptManager.runFile(args[1]);
                if (Log.loggingDebug)
                    Log.debug("event mapping script result: " + scriptManager.getResultString(obj));
	    }
	}
	catch(Exception e) {
	    Log.exception("TestClient.main caught exception loading the event mapping file " + args[1], e)
	    Log.error("Engine: " + e);
	    e.printStackTrace();
	    System.exit(1);
	}

	//
	// start a network server
	//
	Thread messageServerThread = new Thread(Engine.getMessageServer());
	messageServerThread.start();

	//
	// start up the event server
	//
//	Thread eventServerThread = new Thread(Engine.getEventServer());
//	eventServerThread.start();

	MessageServer msgServer = Engine.getMessageServer();

	Log.info("World Manager host: " + managerHost);
	Log.info("World Manager port: " + managerPort);

	// 
	// load the behavior file(s)
	//
	try {
	    for (int i=2; i<args.length; i++) {
		String scriptFilename = args[i];
		System.out.println("Running script: " + scriptFilename);
		Object obj = scriptManager.runFile(scriptFilename);
                if (Log.loggingDebug)
                    Log.debug("script result: " + scriptManager.getResultString(obj));
	    }
	}
	catch(Exception e) {
	    Log.exception("TestClient.main caught exception running script " + scriptFilename, e)
	    System.exit(1);
	}

	// start up the timer events
	TimerEvent event = new TimerEvent(null);
	Engine.getEventServer().addFixed(event, 1000);

	// the updater thread is in charge of keeping all mob locations
	// up to date
	Thread updaterThread = new Thread(new Updater());
	updaterThread.start();

	// read and execute commands (in this thread)
	processCommands();
    }
 
    private static Point parsePoint(String args) {
	int x = 0;
	int y = 0;
	int z = 0;

	int comma1 = args.indexOf(',', 0);
	int comma2 = args.indexOf(',', comma1+1);
	int end = args.length();

//	System.out.println("Parsing string: " + args);
//	System.out.println("Commas and end at: " + comma1 + comma2 + end);

	if( comma1 > 0 && comma2 > comma1 ) {
	    String xStr = args.substring(0, comma1);
	    String yStr = args.substring(comma1+1, comma2);
	    String zStr = args.substring(comma2+1, end);
//	    System.out.println("Numbers are: " + xStr + "," + yStr + "," + zStr);

	    try {
		x = Integer.parseInt(xStr);
		y = Integer.parseInt(yStr);
		z = Integer.parseInt(zStr);
	    }
	    catch (Exception e) {
		return null;
	    }

//	    System.out.println("Converted to: " + x + y + z);
	    return new Point(x, y, z);
	}
	
	return null;
    }

    private static void processCommands() {
	InputStreamReader converter = new InputStreamReader(System.in);
	BufferedReader in = new BufferedReader(converter);
	String curLine = "";

	System.out.println("Type ? or /help for help");
	try {
	    while(true) {
		TestUser user = getUserFocus();

		while(user == null) {
		    // wait for 1st user to login
		    Thread.sleep(1000);
		    user = getUserFocus();
		}

		// print prompt as user_name:
		System.out.print(user.getName() + "> ");

		// get command line
		curLine = in.readLine();

		if(curLine.equalsIgnoreCase("/quit")) {
		    System.exit(0);
		}
		else if(curLine.equalsIgnoreCase("/help") ||
			curLine.equalsIgnoreCase("?")) {
		    System.out.println("Commands are:");
		    System.out.println("/quit");
		    System.out.println("/connect <testuser>");
		    System.out.println("/goto <x,y,z>");
		    System.out.println("/target <mob>");
		    System.out.println("/attack");
		    System.out.println("/<server command>");
		    System.out.println("<say command>");
		}
		else if(curLine.startsWith("/connect ")) {
		    String name = curLine.substring(9);

		    TestUser next = ObjectManager.findTestUser(name);
		    if(next != null) {
			setUserFocus(next);
		    }
		    else {
			System.out.println("Unknown character name:" + name);
		    }
		}
		else if(curLine.startsWith("/target ")) {
		    String name = curLine.substring(8);

		    MVObject target = ObjectManager.findObject(name);
		    if(target != null) {
			setTarget(target);
			System.out.println("Target set to:" + name);
		    }
		    else {
			System.out.println("Unknown object name:" + name);
		    }
		}
		else if(curLine.startsWith("/goto ")) {
		    String args = curLine.substring(6);
		    Point dest = parsePoint(args);

		    if( dest != null ) {
			BaseBehavior behave = (BaseBehavior) user.getBehavior();
			behave.gotoLoc(dest, 3000);
			System.out.println("Going to:" + dest);
		    }
		    else {
			System.out.println("Invalid location:" + args);			
		    }
		}
		else if(curLine.equalsIgnoreCase("/attack")) {
		    MVObject target = getTarget();
		    if(target != null) {
			System.out.println("Attacking:" + target.getName());
			Log.info("TestClient: " + user.getName() + " attacking " + target.getName());
			user.sendEvent(new AutoAttackEvent(user, (MVObject)target, "strike", true));
		    }
		    else {
			System.out.println("Can't attack without target");
		    }
		}
		else if(curLine.startsWith("/")) {
		    System.out.println("Command:" + curLine);

		    // Create command event containing user command
		    Log.info("TestClient: " + user.getName() + " sending command: " + curLine);
		    user.sendEvent(new CommandEvent(user, user, curLine));
		}
		else {
		    System.out.println("Saying:" + curLine);

		    // Create com event containing user text
		    Log.info("TestClient: " + user.getName() + " saying: " + curLine);
		    user.sendEvent(new ComEvent(user, ComEvent.SAY, curLine));
		}
	    }
	}
	catch(Exception e) {
	    Log.exception("TestClient.processCommands caught exception", e)
	    System.exit(1);
	}
    }

    public static synchronized void setTimeDifference(long deltaTime) {
	if (deltaTime < timeDifference) {
	    timeDifference = deltaTime;
	    Log.warn("Testclient: Adjusting time difference: "
		     + timeDifference);
	}
    }
    public static long getTimeDifference() {
	return timeDifference;
    }

    private static String managerHost = "Master-DXP";  // Get name from config file
    private static int managerPort = 5001;             // Get real value from config file
    private static ScriptManager scriptManager = null;
    private static TestUser userFocus = null;
    private static MVObject targetFocus = null;

    private static long timeDifference = Long.MAX_VALUE;
}
