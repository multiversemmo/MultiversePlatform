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

// package multiverse.server.testclient;

// import multiverse.server.network.*;
// import multiverse.server.util.*;
// import multiverse.server.msghandlers.*;
// import java.util.*;
// import java.net.InetAddress;

// public class TestClient2 {
//     public TestClient2() {
//     }

//     public static void readConfig(Configuration config) {

// 	org.w3c.dom.Node serverNode = config.getRoot().getFirstChild();

// 	//
// 	// get the port
// 	//
// 	org.w3c.dom.Node portNode = 
// 	    Configuration.findChild(serverNode, "port");

// 	if (portNode == null) {
// 	    throw new MVRuntimeException("could not find port node");
// 	}
// 	String nodeValue = Configuration.getNodeValue(portNode);
// 	sPort = Integer.valueOf(nodeValue).intValue();
// 	System.out.println("listening on port: " + getPort());

// 	//
// 	// get all the msgHandlers
// 	//
// 	org.w3c.dom.Node msgHandlers = 
// 	    Configuration.findChild(serverNode, "messagehandlers");
// 	if (msgHandlers == null) {
// 	    throw new MVRuntimeException("could not find any messagehandlers");
// 	}
// 	// read each message handler
// 	org.w3c.dom.NodeList handlerList = msgHandlers.getChildNodes();
// 	for (int i=0; i<handlerList.getLength(); i++) {
// 	    org.w3c.dom.Node curNode = handlerList.item(i);
// 	    if (curNode.getNodeName().equals("handler")) {
// 		registerMessageHandler(curNode);
// 	    }
// 	}

// 	//
// 	// get all the events & eventHandlers
// 	//
// 	org.w3c.dom.Node eventHandlers =
// 	    Configuration.findChild(serverNode, "eventhandlers");
// 	if (eventHandlers == null) {
// 	    throw new MVRuntimeException("could not find any event handlers");
// 	}
// 	// read each event handler
// 	handlerList = eventHandlers.getChildNodes();
// 	for (int i=0; i<handlerList.getLength(); i++) {
// 	    org.w3c.dom.Node curNode = handlerList.item(i);
// 	    if (curNode.getNodeName().equals("handler")) {
// 		registerEventHandler(curNode);
// 	    }
// 	}
//     }

//     private static void registerMessageHandler(org.w3c.dom.Node handlerNode) {

// 	// get the id
// 	org.w3c.dom.Node idNode = Configuration.findChild(handlerNode, "id");
// 	if (idNode == null) {
// 	    throw new MVRuntimeException("could not find id node");
// 	}
// 	String nodeValue = Configuration.getNodeValue(idNode);
// 	if (nodeValue == null) {
// 	    throw new MVRuntimeException("could not get value for id node");
// 	}
// 	int id = Integer.valueOf(nodeValue).intValue();
	
// 	// get the class
// 	org.w3c.dom.Node classNode = Configuration.findChild(handlerNode,
// 							     "msgclass");
// 	if (classNode == null) {
// 	    throw new MVRuntimeException("could not find class node for msgclass");
// 	}
// 	String className = Configuration.getNodeValue(classNode);

// 	// load that class
// 	try {
// 	    Class msgHandlerClass = Class.forName(className);
// 	    MessageHandler msgHandler = 
// 		(MessageHandler) msgHandlerClass.newInstance();
// 	    System.out.println("loaded msghandler, msg id#" +
// 			       id +
// 			       " maps to '" + 
// 			       className + "'");
// 	    msgServer.registerHandler(id, msgHandler);
// 	}
// 	catch(Exception e) {
// 	    throw new MVRuntimeException("could not find/instant class: " + e);
// 	}
//     }

//     private static void registerEventHandler(org.w3c.dom.Node handlerNode) {

// 	// get the handler's class
// 	org.w3c.dom.Node handlerClassNode = 
// 	    Configuration.findChild(handlerNode, 
// 				    "handlerclass");

// 	if (handlerClassNode == null) {
// 	    throw new MVRuntimeException("could not find handlerclass");
// 	}
// 	String handlerClassString = 
// 	    Configuration.getNodeValue(handlerClassNode);
// 	if (handlerClassString == null) {
// 	    throw new MVRuntimeException("could not get value for handlerclass node");
// 	}
// 	Class handlerClass = null;
// 	EventHandler eventHandler = null;
// 	try {
// 	    handlerClass = Class.forName(handlerClassString);
	    
// 	    System.out.println("loaded eventhandler class: " +
// 			       handlerClassString);
// 	    eventHandler = (EventHandler) handlerClass.newInstance();
// 	}
// 	catch(Exception e) {
// 	    throw new MVRuntimeException("could not load class: " + e);
// 	}

// 	// get all events to register for this eventHandler
// 	List childList = Configuration.getMatchingChildren(handlerNode,
// 							   "event");
// 	if (childList == null) {
// 	    throw new MVRuntimeException("eventhandler '" +
// 				  handlerClassString +
// 				  "' has no events associated");
// 	}
	
// 	Iterator it = childList.iterator();
// 	while (it.hasNext()) {
// 	    org.w3c.dom.Node eventNode = (org.w3c.dom.Node) it.next();
// 	    String eventClassString = Configuration.getNodeValue(eventNode);
// 	    if (eventClassString == null) {
// 		throw new MVRuntimeException("event has no name");
// 	    }
// 	    // load that class
// 	    try {
// 		Class eventClass = Class.forName(eventClassString);
// 		System.out.println("loaded event class: " + 
// 				   eventClassString);
		
// 		// register it
// 		eventServer.registerEvent(eventClass, eventHandler);
// 	    }
// 	    catch(Exception e) {
// 		throw new MVRuntimeException("could not find/instant class: " + e);
// 	    }
// 	}
//     }

//     public static UserManager getUserManager() {
// 	return userManager;
//     }

//     public static MessageServer getMessageServer() {
// 	return msgServer;
//     }

//     public static EventServer getEventServer() {
// 	return eventServer;
//     }

//     public static void main(String args[]) {
// 	//
// 	// read config
// 	//
// 	if (args.length != 1) {
// 	    System.err.println("specify config file");
// 	    System.exit(1);
// 	}
// 	Configuration config = new Configuration(args[0]);
// 	try {
// 	    readConfig(config);
// 	}
// 	catch(MVRuntimeException e) {
// 	    System.err.println("could not read config file: " + e);
// 	    System.exit(1);
// 	}

// 	//
// 	// start a network server
// 	//
// 	try {
// 	    UDPReader reader = new UDPReader(getPort());
// 	    Message msg = new Message(60000);
	    
// 	    System.out.println("Listening on port " + getPort() +
// 			       " for incoming messages");
	    
// 	    // start up the event server
// 	    Thread eventServerThread = new Thread(eventServer);
// 	    eventServerThread.start();

// 	    // 
// 	    // login to the server
// 	    //
// 	    UDPWriter writer = new UDPWriter(serverHostname, 
// 					     serverPort);

// 	    ByteBuffer buf = new ByteBuffer(60000);
// 	    buf.putInt(uid); // player id
// 	    buf.putInt(3); // message type -- login
// 	    buf.putString("cedeno"); // username
// 	    msgServer.write(writer, buf);

// 	    while (true) {
// 		// read a message
// 		msgServer.receive(reader, msg);
// 		ByteBuffer buf = msg.getByteBuffer();

// 		// find out what message it is
// 		buf.rewind();
// 		InetAddress addr = msg.getAddress();
// 		int playerId = buf.getInt();
// 		int msgID = buf.getInt();
// 		System.out.println("Engine: from player id " +
// 				   playerId +
// 				   ", got message id: " + msgID);
		
// 		// find a message handler
// 		MessageHandler msgHandler = msgServer.getHandler(msgID);
// 		if (msgHandler == null) {
// 		    System.err.println("could not find msgHandler");
// 		}
// 		System.out.println("Engine: passing message to handler: " +
// 				   msgHandler.getName());
		
// 		// retrieve the event
// 		Event event = msgHandler.handleMessage(msg);
// 		System.out.println("engine: event: name='" +
// 				   event.getName() + "'");

// 		// now that we have an event, pass it to the eventserver
// 		eventServer.add(event);
// 	    }
// 	}
// 	catch(java.net.SocketException e) {
// 	    System.err.println("exception: " + e);
// 	    System.exit(1);
// 	}
// 	catch(MVRuntimeException e) {
// 	    System.err.println("MVRuntimeException: " + e);
// 	    System.exit(1);
// 	}
//     }

//     public static int getPort() {
// 	return sPort;
//     }

//     private static int sPort = 0;
//     private static MessageServer msgServer = new MessageServer();
//     private static EventServer eventServer = new EventServer();
//     private static UserManager userManager = new UserManager();
// }
