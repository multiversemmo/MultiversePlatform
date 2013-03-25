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

package multiverse.simpleclient;

import multiverse.server.engine.*;
import multiverse.server.network.rdp.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.server.events.*;
import multiverse.server.math.*;
import multiverse.server.worldmgr.LoginPlugin;
import multiverse.server.messages.PropertyMessage;

import java.util.*;
import java.net.*;
import java.io.*;
import java.util.concurrent.locks.*;
import gnu.getopt.Getopt;
import gnu.getopt.LongOpt;

public class SimpleClient implements ClientConnection.MessageCallback {
    public SimpleClient(Integer accountID) {
        this.accountID = accountID;
    }

    public static void main(String args[]) {
        InitLogAndPid.initLogAndPid(args);
        
        Engine.setOIDManager(new OIDManager());

        Log.info("Running SimpleClient.main");
        
        for (int i=0; i<args.length; i++) {
            String arg = args[i];
            Log.info("   arg[" + i + "] = " + arg);
        }

        scProps = new SimpleClientProps();
        List<String> deleteChars = new LinkedList<String>();

        LongOpt[] longopts = new LongOpt[11];
	longopts[0] = new LongOpt("login-cycle", LongOpt.REQUIRED_ARGUMENT,
		null, 2); 
	longopts[1] = new LongOpt("login", LongOpt.REQUIRED_ARGUMENT,
		null, 3); 
	longopts[2] = new LongOpt("count", LongOpt.REQUIRED_ARGUMENT,
		null, 4); 
	longopts[3] = new LongOpt("seconds-between", LongOpt.REQUIRED_ARGUMENT,
		null, 5); 
	longopts[4] = new LongOpt("log-counters", LongOpt.NO_ARGUMENT,
		null, 6); 
	longopts[5] = new LongOpt("tcp", LongOpt.NO_ARGUMENT,
		null, 7); 
	longopts[6] = new LongOpt("ms-tcp", LongOpt.NO_ARGUMENT,
		null, 8); 
	longopts[7] = new LongOpt("delete", LongOpt.REQUIRED_ARGUMENT,
		null, 9); 
	longopts[8] = new LongOpt("exit-after-login", LongOpt.NO_ARGUMENT,
		null, 10); 
	longopts[9] = new LongOpt("proxy", LongOpt.REQUIRED_ARGUMENT,
		null, 11); 
	longopts[10] = new LongOpt("proxy-map", LongOpt.REQUIRED_ARGUMENT,
		null, 12); 
        Getopt g = new Getopt("SimpleClient", args, "e:s:a:f:Ln:P:t:X:", longopts);

        int c;
        String arg;
        while ((c = g.getopt()) != -1) {
            switch(c) {
            // props file
            case 'e':
                arg = g.getOptarg();
                try {
                    Properties props = new Properties();
                    InputStream file = new FileInputStream(arg);
                    props.load(file);
                    Log.info("props has " + props.size() + " elements");
                    Log.debug("Properties are:");
                    String sKey;
                    Enumeration en = props.propertyNames();
                    while (en.hasMoreElements()) {
                        sKey = (String)en.nextElement();
                            Log.debug("    " + sKey + " = " + props.getProperty(sKey) );
                    }
                    file.close();
                    initConfig(props);
                }
                catch (IOException e) {
                    throw new RuntimeException("simpleclient.main", e);
                }
                break;

                // script files (can be many)
            case 's':
                arg = g.getOptarg();
                Log.info("You picked " + (char)c + 
                                 " with an argument of " +
                                 ((arg != null) ? arg : "null") + "\n");
                scProps.scriptFiles.add(arg);
                break;

                // Set the account id
            case 'a':    
                arg = g.getOptarg();
                scProps.initialAccountID = Integer.parseInt(arg);
                Log.info("Default account id set to " + scProps.initialAccountID);
                break;
                
            case 'f':
                // Set the sentinelFile parameter.  If this is
                // supplied, then when the file no longer exists, the
                // interpolator will notice and exit the process.
                scProps.sentinelFile = g.getOptarg();
                break;
            
            case 'n':
                arg = g.getOptarg();
                scProps.defaultCharName = arg;
                Log.info("Character name set to " + scProps.defaultCharName);
                break;

            // -P key=value
            // -P key=value1,value2,value3
	    case 'P':
		arg = g.getOptarg();
		addCharProperty(arg, SimpleClientProps.defaultCharProperties);
		break;

            case 'X':
                scProps.extraArgs = g.getOptarg();
                break;
                
            // InitLogAndPid has already taken care of this argument
            case 't':
                arg = g.getOptarg();
                break;
                
            // --login-cycle
	    case 2:
		scProps.loginCycleInterval =
                        (int) (60F * Float.parseFloat(g.getOptarg()));
		break;

            // --login
	    case 3:
		arg = g.getOptarg();
		int colon = arg.indexOf(':');
		if (colon == -1) {
		    scProps.loginHostnameOverride = arg;
		    scProps.loginPortOverride = 5040;
		}
		else {
		    scProps.loginHostnameOverride = arg.substring(0,colon);
		    scProps.loginPortOverride =
			Integer.parseInt(arg.substring(colon+1));
		}
		break;

            // --count
            case 4:
                arg = g.getOptarg();
                scProps.clientCount = Integer.parseInt(arg);
                break;

            // --seconds-between
            case 5:
                arg = g.getOptarg();
                scProps.secondsBetween = Float.parseFloat(arg);
                break;

            // --log-counters
            case 6:
                SimpleClient.logCounters = true;
                break;

            // --tcp
            case 7:
                SimpleClient.useTCP = true;
                break;

            // --ms-tcp
            case 8:
                SimpleClient.msUseTCP = true;
                break;

            // --delete
            case 9:
                deleteChars.add(g.getOptarg());
                break;

            // --exit-after-login
            case 10:
                LoginResponseHandler.EXIT_ON_MSG = true;
                break;

            // --proxy
	    case 11:
		arg = g.getOptarg();
		colon = arg.indexOf(':');
		if (colon == -1) {
		    scProps.proxyHostnameOverride = arg;
		    scProps.proxyRdpPortOverride = 5040;
		}
		else {
		    scProps.proxyHostnameOverride = arg.substring(0,colon);
		    scProps.proxyRdpPortOverride =
			Integer.parseInt(arg.substring(colon+1));
		}
		break;

            // --proxy-map
	    case 12:
		arg = g.getOptarg();
                String[] parts = arg.split(",", 2);
                
		colon = parts[1].indexOf(':');
                PortAddress portAddress;
		if (colon == -1) {
                    portAddress = new PortAddress(parts[1], 5040);
		}
		else {
		    portAddress = new PortAddress(parts[1].substring(0,colon),
                        Integer.parseInt(parts[1].substring(colon+1)) );
		}
                scProps.proxyMap.put(parts[0], portAddress);
		break;

            case '?':
                break; // getopt() already printed an error
                //
            default:
                System.out.print("getopt() returned " + c + "\n");
            }
        }
        
        if ((useTCP || msUseTCP) && clientTCPMessageIO == null) {
            clientTCPMessageIO = ClientTCPMessageIO.setup();
            clientTCPMessageIO.start();
        }
        else {
            RDPServer.startRDPServer();
        }

        if (logCounters) {
            Thread simpleClientStatsLogger = new Thread(new SimpleClientStatsLogger());
            simpleClientStatsLogger.start();
        }

        for (String deleteChar : deleteChars) {
            SimpleClient sc = new SimpleClient(scProps.initialAccountID);
            sc.charName = deleteChar;
            sc.deleteChar = deleteChar;
            sc.login();
        }
        if (deleteChars.size() > 0) {
            System.exit(0);
        }

        Integer accountID = scProps.initialAccountID;
        Integer countRemaining = scProps.clientCount;
        while (true) {
            mainLoopIterations++;
            boolean exitPending = (scProps.sentinelFile != null) && (!new File(scProps.sentinelFile).exists());
            if (exitPending)
                scExitPending = true;
            if (exitPending && scProps.openConnections == 0) {
                Log.info("SimpleClient.main: Exiting process");
                System.exit(0);
            }
            if (countRemaining > 0) {
                SimpleClient sc = new SimpleClient(accountID);
                if (scProps.defaultCharName != null)
                    sc.charName = scProps.defaultCharName + "-" + accountID;
                if (SimpleClientProps.defaultCharProperties != null && SimpleClientProps.defaultCharProperties.size() > 0)
                    sc.charProperties = pickCharProperties();
                Thread cr = new Thread(new ClientRunner(sc));
                cr.start();
                countRemaining--;
            }
        
            if (countRemaining > 0) {
                try {
                    Thread.sleep((int)(scProps.secondsBetween * 1000));
                }
                catch (Exception e) {
                    Log.exception("SimpleClient.main starting accountID " + accountID, e);
                }
                accountID++;
            }
            else {
                try {
                    Thread.sleep(1000);
                }
                catch (Exception e) {
                    Log.exception("SimpleClient.main idle thread interrupted", e);
                }
            }
        }
    }
    
    public static class SimpleClientStatsLogger implements Runnable {
        public void run() {
            while (!scExitPending) {
                try {
                    Thread.sleep(1000);
                    Log.warn("SimpleClient iterations last second/total: " + 
                        "main " + (mainLoopIterations - lastMainLoopIterations) + "/" + mainLoopIterations + 
                        ", ClientRunner " + (clientRunnerIterations - lastClientRunnerIterations) + "/" + clientRunnerIterations + 
                        ", Interpolator " + (interpolatorIterations - lastInterpolatorIterations) + "/" + interpolatorIterations);
                    lastMainLoopIterations = mainLoopIterations;
                    lastClientRunnerIterations = clientRunnerIterations;
                    lastInterpolatorIterations = interpolatorIterations;
                    Log.warn("SimpleClient messages last second/total: sent " + (sendCount - lastSendCount) + "/" + sendCount + 
                        ", received " + (receiveCount - lastReceiveCount) + "/" + receiveCount);
                    lastSendCount = sendCount;
                    lastReceiveCount = receiveCount;
                }
                catch (Exception e) {
                    Log.exception("SimpleClient.SimpleClientStatsLogger.run thread interrupted", e);
                }
            }
        }
    }

    private static class PortAddress {
        public PortAddress(String host, int port)
        {
            this.host = host;
            this.port = port;
        }
        public String host;
        public int port;
    }

    public static boolean scExitPending = false;
    public static boolean logCounters = false;
    public static boolean useTCP = false;
    public static boolean msUseTCP = false;
    protected static long mainLoopIterations = 0;
    protected static long clientRunnerIterations = 0;
    protected static long interpolatorIterations = 0;
    protected static long lastMainLoopIterations = 0;
    protected static long lastClientRunnerIterations = 0;
    protected static long lastInterpolatorIterations = 0;
    protected static int sendCount = 0;
    protected static int receiveCount = 0;
    protected static int lastSendCount = 0;
    protected static int lastReceiveCount = 0;

    protected static ClientTCPMessageIO clientTCPMessageIO = null;

    private static HashMap<String,Serializable> pickCharProperties() {
        HashMap<String,Serializable> map = new HashMap<String,Serializable>();
        for (Map.Entry<String, LinkedList<String>> entry : SimpleClientProps.defaultCharProperties.entrySet()) {
            LinkedList<String> propList = entry.getValue();
            int index = SimpleClientProps.rand.nextInt(propList.size());
            String alt = propList.get(index);
            Log.warn("pickCharProperties: for prop " + entry.getKey() + ", " + propList.size() + " alternatives, picked " + alt);
            map.put(entry.getKey(), propList.get(index));
        }
        return map;
    }
    
    public static void addToOpenConnections(Integer addend) {
        synchronized(scProps) {
            scProps.openConnections += addend;
        }
        Log.info("addToOpenConnections(" + addend + "), scProps.openConnections " + scProps.openConnections);
    }
    
    public static class ClientRunner implements Runnable {
        public ClientRunner(SimpleClient sc) {
            this.sc = sc;
        }
        
        public SimpleClient sc = null;
        
        public void run() {
            Thread.currentThread().setName("Runner-" + sc.accountID);
            // start the interpolator thread
            Log.info("SimpleClient: starting interpolation thread");
            Thread interpolator = new Thread(new Interpolator(sc));
            interpolator.start();

            // load up the javascript file
            ScriptManager scriptManager = new ScriptManager();
            try {
                scriptManager.init();

                Iterator<String> iter = scProps.scriptFiles.iterator();
                while (iter.hasNext()) {
                    if (scExitPending)
                        return;
                    String scriptFile = iter.next();
                    System.out.println("For test client " + sc.accountID + ", reading in script: " + scriptFile);
                    File f = new File(scriptFile);
                    if (f.exists()) {
                        System.out.println("For test client " + sc.accountID + ", found script file, running");
                        instantiatingSimpleClient.set(sc);
                        scriptManager.runFile(scriptFile);
                        // Comment out the line below that sets
                        // instantiatingSimpleClient to null, because
                        // Steve has seen examples in which it got
                        // nulled before PlayerClient had captured it.
//                         instantiatingSimpleClient = null;
                        Log.debug("For test client " + sc.accountID + ", script completed");
                    }
                    else {
                        Log.warn("For test client " + sc.accountID + ", didnt find script file " +
                            scriptFile);
                    }
                }
            }
            catch(Exception e) {
                Log.exception("For test client " + sc.accountID + ", error running script", e);
                e.printStackTrace();
                System.exit(1);
            }

            // main thread idles
            while (true) {
                clientRunnerIterations++;
                if (scExitPending)
                    return;
                try {
                    synchronized (sc) {
                        while (sc.loginTime != 0)
                            sc.wait();
                    }
                    if (! sc.login()) {
                        // Login failed, sleep and try again
                        if (LoginResponseHandler.EXIT_ON_MSG) {
                            // --exit-after-login
                            System.exit(1);
                        }
                        Thread.sleep(5000);
                        continue;
                    }
                }
                catch(InterruptedException e) {
                    throw new RuntimeException("SimpleClient test client " + sc.accountID, e);
                }
            }
        }
    }

    /**
     * This class holds all the state that is shared among all
     * SimpleClient instances; most of this state is gotten from the
     * the property file
     */
    public static class SimpleClientProps {

        public Integer clientCount = 1;
        public Float secondsBetween = 5.0F;
        public Integer openConnections = 0;
        
        public String username = null;
        public String password = null;
        public Integer initialAccountID = null;
        public String defaultCharName = null;

        public Boolean useMasterServer = new Boolean(false);
        public String masterServer = "www.multiverse.net";
        public int msTcpPort = -1;
        public int msRdpPort = -1;
        public String worldId = null;

        String loginHostnameOverride = null;
        Integer loginPortOverride = null;
        String proxyHostnameOverride = null;
        int loginCycleInterval = 0;             // seconds
        Integer proxyRdpPortOverride = null;
        String sentinelFile = null;
        String extraArgs;
        Map<String,PortAddress> proxyMap = new HashMap<String,PortAddress>();

        // javascript filenames - for events and stuff
        List<String> scriptFiles = new LinkedList<String>();

        static HashMap<String,LinkedList<String>> defaultCharProperties =
            new HashMap<String,LinkedList<String>>();
        static Random rand = new Random();
    }
    
    public static SimpleClientProps scProps = null;

    public String getExtraArgs()
    {
        return scProps.extraArgs;
    }

    /**
     * Provide a (kludgey) way for PlayerClients started via a script
     * to pick up the SimpleClient instance
     */
    protected static ThreadLocal<SimpleClient> instantiatingSimpleClient =
        new ThreadLocal<SimpleClient>();

    public static SimpleClient getInstantiatingSimpleClient() {
        return instantiatingSimpleClient.get();
    }

    private static void addCharProperty(String propertyArg, HashMap<String,LinkedList<String>> charProps)
    {
	int equal = propertyArg.indexOf('=');
	if (equal == -1)  {
	    System.err.println("Missing '=' in property: "+propertyArg);
	    return;
	}

	String propertyName = propertyArg.substring(0,equal);
	String propertyValue = propertyArg.substring(equal+1, propertyArg.length());

	System.out.println("char prop "+propertyName+"="+propertyValue);
        String[] values = propertyValue.split(",");
        LinkedList<String> list = new LinkedList<String>();
	for (String s : values)
            list.add(s.trim());
//         Log.warn("Adding " + list.size() + " alternatives for char prop " + propertyName);
        charProps.put(propertyName,list);
    }

    public boolean login()
    {
	// indicate login in progress
	loginTime = -1;

	Log.debug("Attempting login ...");

        // log into multiverse
        int userToken = -1;
        if (scProps.useMasterServer) {
            Log.debug("Using master server");
            try {
                userToken = connectMaster();
                if (Log.loggingDebug)
                    Log.debug("SimpleClient: got userToken " + userToken);
            }
            catch(Exception e) {
                Log.exception("exception performing connectMaster()", e);
                return false;
            }

            // resolve the world id
            // loginHostname and loginPort should be filled out after this call 
            try {
                resolveWorld();
            }
            catch(Exception e) {
                Log.exception("exception performing SimpleClient.resolveWorld()", e);
                return false;
            }
        }
        else {
            userToken = ~accountID;
        }

        // connect to the login server - get the characters
        try {
	    getCharacters(userToken);
        }
        catch(Exception e) {
            Log.exception("exception performing SimpleClient.getCharacters(userToken)", e);
            return false;
        }

        if (deleteChar != null)
            return true;

        // connect to the proxy server
        try {
            connectProxy();
        }
        catch(Exception e) {
            Log.exception("exception performing SimpleClient.connectProxy()", e);
            return false;
        }

	loginTime = System.currentTimeMillis();
	interpReady = true;

        synchronized (this) {
            this.notify();
        }

	return true;
    }

    void logout()
    {
	loggedIn = false;
        Log.debug("Logout ...");

	userLock.lock();
	// This should stop the interpolator
	loc = null;
	locDirty = false;
	dir = new MVVector();
	orientation = new Quaternion();
	charOid = -1;
	interpReady = false;
	userLock.unlock();

	conLock.lock();
	try {
	    if (proxyCon != null) {
		proxyCon.close();
                addToOpenConnections(-1);
            }
	}
	catch (MVRuntimeException e) {
	    // This is stupid: RDPConnection wraps all exceptions in
	    // an MVRuntimeException, so we can't distinguish between an
	    // IOException and anything else.
	    Log.exception("logout ignoring exception: ",e);
	}
	finally {
	    proxyCon = null;
	    proxyHostname = null;
	    proxyRdpPort = 5050;
	    conLock.unlock();
	}

	lastUpdated = null;

	gotResolveResponse = false;

	// wait 5 seconds for the server to sanitize itself
	try {
	    Thread.sleep(5000);
	}
	catch(InterruptedException e) {
	    throw new RuntimeException(e);
	}

	synchronized (this) {
	    loginTime = 0;
	    this.notifyAll();
	}
    }

    /**
     * this class knows how to handle incoming packet.
     */
    public class Dispatcher {
        
        public void registerHandler(int msgType, MessageHandler handler) {
            lock.lock();
            try {
                map.put(msgType, handler);
            }
            finally {
                lock.unlock();
            }
        }
        
        public void dispatch(ClientConnection con, MVByteBuffer buf) {
            // read the int from the buffer
            /* long playerOid = */ buf.getLong();
            int msgType = buf.getInt();
            buf.rewind();

	    if (msgType == 81) {
		// AuthorizedLoginResponse
		try {
		    conLock.lock();
		    /* long oid = */ buf.getLong();
		    /* int msgId = */ buf.getInt();
		    /* long time = */ buf.getLong();
		    loginStatus = buf.getInt();
		    loginMessage = buf.getString();
		    buf.rewind();
		    gotLoginResponse = true;
		    newPacketCondition.signalAll();
		}
		finally {
		    conLock.unlock();
		}
	    }

            // look up in map
            MessageHandler handler = map.get(msgType);
            if (handler == null) {
                if (Log.loggingDebug)
                    Log.debug("Dispatcher.dispatch: unhandled msg type: '" +
                        MVMsgNames.msgName(msgType) + "'; msgType " + msgType);
                return;
            }
            
            // call callback
            if (Log.loggingDebug)
                Log.debug("Dispatcher.dispatch: found matching handler for msg type: '" + MVMsgNames.msgName(msgType) + "'; msgType " + msgType);
            handler.handleMessage(con, buf);
        }
        

        Map<Integer, MessageHandler> map = new HashMap<Integer, MessageHandler>(); 
        Lock lock = LockFactory.makeLock("DispatcherLock");
    }
    
    public Dispatcher getDispatcher() {
        return dispatcher;
    }
    
    public void processPacket(ClientConnection con, MVByteBuffer buf) {
        if (Log.loggingDebug)
            Log.debug("process packet");
        try {
            if (con == msCon) {
                processMSPacket(con, buf);
                return;
            }
            if (con == proxyCon) {
                processProxyPacket(con, buf);
                return;
            }
        }
        catch(MVRuntimeException e) {
            throw new RuntimeException("simpleclient.processPacket", e);
        }

        if (Log.loggingDebug)
            Log.debug("SimpleClient.processPacket: unknown con");
    }
    
    private void processMSPacket(ClientConnection con, MVByteBuffer buf) {
        conLock.lock();
        try {
            int msgType = buf.getInt();
            if (msgType != 2) {
                Log.warn("SimpleClient.processPacket: not a resolve response msg, got msg type " + msgType);
                return;
            }
            if (Log.loggingDebug)
                Log.debug("SimpleClient.processPacket: msgType=" + msgType);

            String worldID = buf.getString();
            if (Log.loggingDebug)
                Log.debug("SimpleClient.processPacket: worldID=" + worldID);

            int status = buf.getInt();
            if (status == 0) {
                throw new RuntimeException("SimpleClient.processPacket: resolve failed");
            }
            if (Log.loggingDebug)
                Log.debug("SimpleClient.processPacket: status=" + status);

            this.loginHostname = buf.getString();
            this.loginPort = buf.getInt();
            Log.info("Resolved world id " + worldID + 
                     " to hostname=" + 
                     this.loginHostname +
                     ", port=" + this.loginPort);
            
            gotResolveResponse = true;
            newPacketCondition.signalAll();
//        }
//        catch(MVRuntimeException e) {
//            throw new RuntimeException("SimpleClient.processPacket", e);
        }
        finally {
            conLock.unlock();
        }
    }

    private void processProxyPacket(ClientConnection con, MVByteBuffer buf) {
        receiveCount++;
        dispatcher.dispatch(con, buf);
    }

    public void connectionReset(ClientConnection con) {
        Log.warn("SimpleClient.connectionReset");
    }

    public static void initConfig(Properties props) {
	Integer logLevel = Integer.parseInt(props.getProperty("log_level"));
	if (logLevel != null)
	    Log.setLogLevel(logLevel);

	scProps.username = props.getProperty("username");
	scProps.password = props.getProperty("password");

	String val = null;

	val = props.getProperty("use_master_server");
	if (val != null) {
	    scProps.useMasterServer = Boolean.parseBoolean(val);
	}
	val = props.getProperty("account_id");
	if (val != null) {
	    scProps.initialAccountID = Integer.parseInt(val);
	}

	val = props.getProperty("ms_tcp_port");
	if (val != null) {
	    scProps.msTcpPort = Integer.parseInt(val);
	}
	val = props.getProperty("ms_rdp_port");
	if (val != null) {
	    scProps.msRdpPort = Integer.parseInt(val);
	}
	val = props.getProperty("ms_hostname");
	if (val != null) {
	    scProps.masterServer = val;
        }
        
	scProps.worldId = props.getProperty("world_id");
	scProps.defaultCharName = props.getProperty("character_name");

	scProps.loginHostnameOverride = props.getProperty("login_hostname_override");
	val = props.getProperty("login_port_override");
	if (val != null) {
	    scProps.loginPortOverride = Integer.parseInt(val);
	}
	scProps.proxyHostnameOverride = props.getProperty("proxy_hostname_override");
	val = props.getProperty("proxy_port_override");
	if (val != null) {
	    scProps.proxyRdpPortOverride = Integer.parseInt(val);
	}

        // Initialize RDP aggregation based on the properties
        PacketAggregator.initializeAggregation(props);
    }


    // returns the uid for the user or -1 on failure
    public int connectMaster() throws IOException {

        // create socket connection
        Socket socket = new Socket(scProps.masterServer, scProps.msTcpPort);
        
	DataInputStream in = 
	    new DataInputStream(socket.getInputStream());
	DataOutputStream out = 
	    new DataOutputStream(socket.getOutputStream());

	// write the username
        int len = scProps.username.length();
        out.writeInt(len);
        if (Log.loggingDebug)
            Log.debug("SimpleClient: sending username " + scProps.username +
                      ", len=" + len);
	out.write(scProps.username.getBytes());
	
	// sending password
        Log.debug("SimpleClient: sending password");
        out.writeInt(scProps.password.length());
        out.write(scProps.password.getBytes());

	// read success or failure (0==fail, 1==success)
	int status = in.readInt();
        
	// read userid
	/* int length = */ in.readInt();
	int uid = in.readInt();

        if (status == 0) {
            Log.error("Login failed");
        }
        else {
            Log.info("MasterServer login succeeded");
        }
        if (status == 0)
            return -1;

        return uid;
    }

    // with the master server
    // resolves the world id
    // sets the login server info in the object
    public void resolveWorld() 
        throws java.net.UnknownHostException,
               java.net.BindException,
               java.lang.InterruptedException, 
               MVRuntimeException {
        if (Log.loggingDebug)
            Log.debug("SimpleClient.resolveWorld: connecting to port " +
                      scProps.msRdpPort +
                      ", worldId=" + scProps.worldId);
        this.msCon = (msUseTCP ? new ClientTCPConnection(clientTCPMessageIO) : new RDPConnection());
        msCon.registerMessageCallback(this);
        msCon.open(scProps.masterServer, scProps.msRdpPort);
        if (msCon instanceof ClientTCPConnection)
            clientTCPMessageIO.addAgent(((ClientTCPConnection)msCon).getAgentInfo());
        
        if (Log.loggingDebug)
            Log.debug("SimpleClient.resolveWorld: connection open: " + msCon);
        
        MVByteBuffer buf = new MVByteBuffer(200);
        buf.putInt(0); // says its a resolve request
        buf.putString(scProps.worldId); // send over the world id
        buf.flip();

        conLock.lock();
        try {
            Log.debug("SimpleClient.resolveWorld: sending request packet");
            sendCount++;
            msCon.send(buf);

            // now we wait until we get back the response
            // throught the packet callback
            Log.debug("SimpleClient.resolveWorld: waiting for world resolution response");
            while(! gotResolveResponse) {
                newPacketCondition.await();
            }
            Log.debug("SimpleClient.resolveWorld: got response");
        }
        finally {
            conLock.unlock();
        }
    }

    String readString(DataInputStream in)
	throws IOException
    {
	int len = in.readInt();
	if (len == 0)
	    return "";
	byte[] stringBytes = new byte[len];
	in.readFully(stringBytes);
	return new String(stringBytes, "UTF8");
    }

    // connects to the login server and gets the character listing
    // pass in the user token
    private void getCharacters(int userToken)
    {
        try {
            String hostname = (scProps.loginHostnameOverride == null) ?
                loginHostname : scProps.loginHostnameOverride;
	    int port = (scProps.loginPortOverride == null) ?
                loginPort : scProps.loginPortOverride;
            
            if (Log.loggingDebug)
                Log.debug("SimpleClient.getCharacters: logging into login server " + hostname + ":" + port);

            // create socket connection
            Socket socket = new Socket(hostname, port);

            DataInputStream in = 
                new DataInputStream(socket.getInputStream());
            DataOutputStream out = 
                new DataOutputStream(socket.getOutputStream());

            // send token id - string
	    MVByteBuffer buffer = new MVByteBuffer(32);
	    buffer.putInt(0);		// message length
	    buffer.putInt(LoginPlugin.MSGCODE_CHARACTER_REQUEST);
	    buffer.putString("1.5");	// client version
            buffer.putInt(4);		// auth code; this is a string in the
            buffer.putInt(userToken);   // protocol, so this is a hack

	    // patch the message length
	    int len = buffer.position();
	    buffer.getNioBuf().rewind();
	    buffer.putInt(len-4);
	    buffer.position(len);
	    out.write(buffer.getNioBuf().array(), 0, len);

            int msgLength = in.readInt();
	    byte[] msgBytes = new byte[msgLength];
	    in.readFully(msgBytes);
	    MVByteBuffer message = new MVByteBuffer(msgBytes) ;

            int msgCode = message.getInt();
            if (msgCode != LoginPlugin.MSGCODE_CHARACTER_RESPONSE) {
                throw new RuntimeException("SimpleClient.getCharacters: unexpected message code"+msgCode);
            }

	    String serverVersion = message.getString();
	    /* String worldToken = */ message.getString();  // obsolete
	    String errorMessage = message.getString();

            if (Log.loggingDebug)
                Log.debug("SimpleClient.getCharacters: serverVersion="+
			serverVersion);

            if (! errorMessage.equals("")) {
                Log.error("getCharacters failed for "+userToken+": "+
                     errorMessage);
                System.err.println("getCharacters failed for "+userToken+": "+
                     errorMessage);
                socket.close();
                return;
            }

            // read num characters
            int nChars = message.getInt();
            if (Log.loggingDebug)
                Log.debug("SimpleClient.getCharacters: nChars=" + nChars);

	    if (nChars == 0) {
                if (deleteChar == null) {
                    message = createCharacter(in,out);
                    nChars = 1;
                }
                else {
                    System.out.println("Account "+accountID+" has no characters to delete");
                    socket.close();
                    return;
                }
                    
	    }

            this.charOid = -1;

	    boolean firstChar = true;
            while(nChars > 0) {
		Map<String,Serializable> props =
			PropertyMessage.unmarshallProperyMap(message);
		Long oid = (Long) props.get("characterId");
		String name = (String) props.get("characterName");
		//String hostName = (String) props.get("hostname");
		//Integer proxyPort = (Integer) props.get("port");

		Boolean status = (Boolean) props.get("status");
		errorMessage = (String) props.get("errorMessage");

                Log.info("SimpleClient.getCharacters: oid=" + oid +
                          " name=" + name );

		if (status != null && ! status) {
		    Log.error("SimpleClient: character creation failed for name '"+charName+"': "+errorMessage);
		    break;
	 	}

		if (this.charName == null) {
		    if (firstChar) {
			this.charOid = oid;
			Log.info("SimpleClient.getCharacters: using first character oid="+oid);
                        break;
		    }
		}
		else {
		    if (name.equals(this.charName)) {
			this.charOid = oid;
			Log.info("SimpleClient.getCharacters: matched charname="+this.charName+" oid="+oid);
                        break;
		    }
                    else {
                        Log.error("SimpleClient.getCharacters: found no match for charname="+this.charName);
                    }
		}

		nChars--;
		firstChar = false;
	    }

            if (deleteChar != null) {
                if (this.charOid != -1)
                    deleteCharacter(in,out);
                else
                    System.out.println("Character \""+deleteChar+"\" not found.");
            }

            // Select the character and get proxy auth token
            if (this.charOid != -1 && deleteChar == null) {
                // send token id - string
                buffer = new MVByteBuffer(64);
                buffer.putInt(0);		// message length
                buffer.putInt(LoginPlugin.MSGCODE_CHARACTER_SELECT_REQUEST);
                Map<String,Serializable> props =
                    new HashMap<String,Serializable>();
                props.put("characterId",this.charOid);
                List<String> propStrings = new ArrayList<String>();
                int nProps = PropertyMessage.createPropertyString(propStrings,
                        props, "");
                buffer.putInt(nProps);
                for (String s : propStrings) {
                    buffer.putString(s);
                }

                // patch the message length
                len = buffer.position();
                buffer.getNioBuf().rewind();
                buffer.putInt(len-4);
                buffer.position(len);
                out.write(buffer.getNioBuf().array(), 0, len);

                // read the response
                msgLength = in.readInt();
                msgBytes = new byte[msgLength];
                in.readFully(msgBytes);
                message = new MVByteBuffer(msgBytes) ;

                msgCode = message.getInt();
                if (msgCode != LoginPlugin.MSGCODE_CHARACTER_SELECT_RESPONSE) {
                    throw new RuntimeException("SimpleClient.getCharacters: unexpected message code"+msgCode);
                }

                props = message.getPropertyMap();
                this.proxyToken = (byte[]) props.get("token");
                this.proxyHostname = (String) props.get("proxyHostname");
                this.proxyRdpPort = (Integer) props.get("proxyPort");

                if (this.proxyHostname != null &&
                            this.proxyHostname.equals(":same")) {
                    this.proxyHostname = hostname;
                }
            }

	    socket.close();
        }
        catch(Exception e) {
            throw new RuntimeException("SimpleClient.getCharacters", e);
        }

        if (this.charOid == -1)
            throw new RuntimeException("found no match for charname="+this.charName);
    }

    private MVByteBuffer createCharacter(DataInputStream in,
		DataOutputStream out)
	throws IOException
    {
	MVByteBuffer message = new MVByteBuffer(128);
	message.putInt(0);	// message length
	message.putInt(LoginPlugin.MSGCODE_CHARACTER_CREATE);

	HashMap<String,Serializable> charProps = new HashMap<String,Serializable>(charProperties);
	if (charName != null)
	    charProps.put("characterName", charName);
        charProps.put("strength", "10");
        charProps.put("dexterity", "10");
        charProps.put("wisdom", "10");
        charProps.put("intelligence", "10");
        charProps.put("class", "10");

	List<String> propStrings = new ArrayList<String>();
	int nProps = PropertyMessage.createPropertyString(propStrings,
                charProps, "");
        Log.debug("SimpleClient.createCharacter: nProps " + nProps);
        message.putInt(nProps);
	for (String s : propStrings) {
	    message.putString(s);
	}

	// patch the message length
	int len = message.position();
	message.getNioBuf().rewind();
	message.putInt(len-4);
	message.position(len);

	out.write(message.getNioBuf().array(),0,len);

	int msgLength = in.readInt();
	byte[] msgBytes = new byte[msgLength];
	in.readFully(msgBytes);
	message = new MVByteBuffer(msgBytes) ;

	int msgCode = message.getInt();
	if (msgCode != LoginPlugin.MSGCODE_CHARACTER_CREATE_RESPONSE) {
	    throw new RuntimeException("SimpleClient.createCharacter: unexpected message code"+msgCode);
	}

	return message;
    }

    private MVByteBuffer deleteCharacter(DataInputStream in,
		DataOutputStream out)
	throws IOException
    {
        if (charOid == -1) {
            return null;
        }

	MVByteBuffer message = new MVByteBuffer(128);
	message.putInt(0);	// message length
	message.putInt(LoginPlugin.MSGCODE_CHARACTER_DELETE);

	HashMap<String,Serializable> charProps = new HashMap<String,Serializable>();
        charProps.put("characterId", charOid);

	List<String> propStrings = new ArrayList<String>();
	int nProps = PropertyMessage.createPropertyString(propStrings,
                charProps, "");
	message.putInt(nProps);
	for (String s : propStrings) {
	    message.putString(s);
	}

	// patch the message length
	int len = message.position();
	message.getNioBuf().rewind();
	message.putInt(len-4);
	message.position(len);

	out.write(message.getNioBuf().array(),0,len);

	int msgLength = in.readInt();
	byte[] msgBytes = new byte[msgLength];
	in.readFully(msgBytes);
	message = new MVByteBuffer(msgBytes) ;

	int msgCode = message.getInt();
	if (msgCode != LoginPlugin.MSGCODE_CHARACTER_DELETE_RESPONSE) {
	    throw new RuntimeException("SimpleClient.deleteCharacter: unexpected message code"+msgCode);
	}

        Map<String,Serializable> props =
                PropertyMessage.unmarshallProperyMap(message);
        Boolean status = (Boolean) props.get("status");
        String errorMessage = (String) props.get("errorMessage");
        if (status == true) {
            System.out.println("Deleted character \""+deleteChar+"\", oid="+charOid);
        }
        else {
            System.out.println("Delete character failed: "+errorMessage);
        }

	return message;
    }

    // rdp - connects to proxy
    public void connectProxy() 
        throws java.net.UnknownHostException,
               java.net.BindException,
               java.lang.InterruptedException {
        Log.info("SimpleClient.connectProxy: proxyRdpPortOverride " + scProps.proxyRdpPortOverride + 
            ", proxyRdpPort " + proxyRdpPort);

        String hostname;
        int port;

        if (scProps.proxyHostnameOverride != null)
            hostname = scProps.proxyHostnameOverride;
        else {
            PortAddress proxyAddress = scProps.proxyMap.get(
                proxyHostname+":"+proxyRdpPort);
            if (proxyAddress != null) {
                hostname = proxyAddress.host;
                port = proxyAddress.port;
            }
            else {
                hostname = proxyHostname;
                port = proxyRdpPort;
            }
        }
        if (scProps.proxyRdpPortOverride != null)
            port = scProps.proxyRdpPortOverride;
        else
            port = proxyRdpPort;

        Log.info("SimpleClient.connectProxy: connecting to " +
                 hostname + ":" + port);
        proxyCon = (useTCP ? new ClientTCPConnection(clientTCPMessageIO) : new RDPConnection());
        proxyCon.registerMessageCallback(this);
        proxyCon.open(hostname, port);
        addToOpenConnections(1);
        if (proxyCon instanceof ClientTCPConnection)
            clientTCPMessageIO.addAgent(((ClientTCPConnection)proxyCon).getAgentInfo());
	gotLoginResponse = false;
	loginStatus = 0;
	loginMessage = null;

        // send a login event
        AuthorizedLoginEvent loginEvent = new AuthorizedLoginEvent();
        loginEvent.setOid(this.charOid);
        loginEvent.setVersion("1.5" + "," + "DirLocOrient");
	if (proxyToken == null)
	    loginEvent.setWorldToken(new MVByteBuffer(0));
	else
	    loginEvent.setWorldToken(new MVByteBuffer(proxyToken));
        Log.debug("sending login event with version " + loginEvent.getVersion()+
                " tokenByteCount="+proxyToken.length);
        sendCount++;
        if (! proxyCon.sendIfPossible(loginEvent.toBytes())) {
	    Log.error("Failed sending login event to proxy");
	    throw new MVRuntimeException("Failed sending login event to proxy");
	}

	Log.debug("SimpleClient.login: waiting for login response");
        conLock.lock();
        try {
            // Wait for the response to come through the packet callback
            while(! gotLoginResponse) {
                newPacketCondition.await();
            }
	    if (loginStatus == 0) {
		Log.error("Login failed: "+loginMessage);
		logout();
		return;
	    }
        }
        finally {
            loggedIn = true;
            conLock.unlock();
        }
	Log.debug("SimpleClient.login: got response");
    }

    Dispatcher dispatcher = new Dispatcher();
    

    // client's dir and loc
    public Point loc = null;
    public MVVector dir = new MVVector();
    public Quaternion orientation = new Quaternion();
    public Long lastUpdated = null;
    public boolean locDirty = false;
    public boolean interpReady = false;
    public Lock userLock = LockFactory.makeLock("UserLock");

    long loginTime = 0;
    HashMap<String,Serializable> charProperties = new HashMap<String,Serializable>();

    // as returned by the master server upon login
    int userToken = -1;

    public Integer accountID = null;

    // the character you want to log in as (must be already created)
    public String charName = null;
    public String deleteChar = null;

    // this will be set by the getCharacters() method when it matches the
    // character name passed in
    public long charOid = -1;

    // master server rdp connection used for world id resolution
    ClientConnection msCon = null;

    public ClientConnection proxyCon = null;

    String loginHostname = null;
    int loginPort = -1;

    String proxyHostname = null;
    int proxyRdpPort = 5050;
    byte[] proxyToken = null;

    boolean gotResolveResponse = false;
    boolean gotLoginResponse = false;
    int loginStatus = 0;
    String loginMessage = null;
    boolean loggedIn = false;
    Lock conLock = LockFactory.makeLock("RDPConnectionLock");

    Condition newPacketCondition = conLock.newCondition();

    public void interpolateNow() {
        userLock.lock();
        try {
	    if (! interpReady || loc == null)
		return;
            long currentTime = System.currentTimeMillis();
            if (lastUpdated == null)
                lastUpdated = currentTime - 100;  // Initialize, but avoid a divide-by-zero
            else if (currentTime == lastUpdated)
                return;
            long timeDelta = currentTime - lastUpdated;
            float timeDeltaSeconds = (float)timeDelta / 1000;
            MVVector scaledDir = (MVVector)dir.clone();
                    
            if (! scaledDir.isZero()) {
                scaledDir.scale(timeDeltaSeconds);
                Point newLoc = (Point) loc.clone();
                newLoc.add((int)scaledDir.getX(),
                    (int)scaledDir.getY(),
                    (int)scaledDir.getZ());
                loc = newLoc;
                if (Log.loggingDebug)
                    Log.debug("SimpleClient.interpolate: oldLoc=" +
                        loc +
                        ", newLoc=" + newLoc +
                        ", dir=" + dir +
                        ", timeDeltaSeconds=" + timeDeltaSeconds +
                        ", scaledDir=" + scaledDir);
            }
                    
            lastUpdated = currentTime;
        }
        finally {
            userLock.unlock();
        }
    }
    
    public static class Interpolator implements Runnable {
        public Interpolator(SimpleClient sc) {
            this.sc = sc;
        }
        
        public void run() {
            Thread.currentThread().setName("SC-" + sc.accountID);
            int cycleCount = 0;
            while (true) {
		interpolatorIterations++;
                if (scProps.loginCycleInterval > 0 && sc.loginTime > 0)  {
		    long delta = (System.currentTimeMillis() -
			sc.loginTime) / 1000;
		    if (delta > scProps.loginCycleInterval) {
			sc.logout();
			continue;
		    }
		}

                if (sc.loc == null) {
                    try {
                        Thread.sleep(1000);
                    }
                    catch(Exception e) {
                        throw new RuntimeException(e);
                    }
                    continue;
                }

                try {
                    Thread.sleep(100);
                }
                catch(Exception e) {
                    throw new RuntimeException(e);
                }
                sc.interpolateNow();
                try {
                    if (scProps.sentinelFile != null) {
                        if (!new File(scProps.sentinelFile).exists()) {
                            // Close the connections and exit
                            if (sc.proxyCon != null) {
                                sc.conLock.lock();
                                try {
                                    sc.proxyCon.close();
                                    addToOpenConnections(-1);
                                }
                                finally {
                                    sc.conLock.unlock();
                                }
                            }
                            Log.info("Sentinel file '" + scProps.sentinelFile + "' no longer exists, so exiting thread.  openConnection count " + scProps.openConnections);
                            return;
                        }
                    }
                    cycleCount++;
                    if (sc.loggedIn && (sc.locDirty || cycleCount >= 50)) { // 5 seconds
                        sc.locDirty = false;
                        cycleCount = 0;
                        sendDirLocOrientMessage();
                    }
                }
                catch(Exception e) {
                    throw new RuntimeException(e);
                }
            }
        }

        protected void sendDirLocOrientMessage() {
            MVByteBuffer buf = new MVByteBuffer(80);
            buf.putLong(sc.charOid);
            buf.putInt(79);
            buf.putLong(System.currentTimeMillis());
            buf.putMVVector(sc.dir);
            buf.putPoint(sc.loc);
            buf.putQuaternion(sc.orientation);
            buf.flip();
            try {
                sc.conLock.lock();
                try {
                    // Don't try to send to a closed connection
                    if (!sc.proxyCon.isOpen())
                        return;
                    sendCount++;
                    if (sc.proxyCon.sendIfPossible(buf)) {
                        if (Log.loggingDebug)
                            Log.debug("Sent DirLocOrient message: dir = " +
                                      sc.dir + "; loc = " +
                                      sc.loc + "; orient = " +
                                      sc.orientation);
                    }
                }
                finally {
                    sc.conLock.unlock();
                }
            }
            catch (Exception e) {
                throw new RuntimeException(e.getMessage());
            }
        }
        
        private SimpleClient sc = null;

    }

    protected void sendCommandMessage(String commandString, Long target) {
        if (!loggedIn)
            return;
        MVByteBuffer buf = new MVByteBuffer(200);
        buf.putLong(charOid);
        buf.putInt(13);
        buf.putLong(target);
        buf.putString(commandString);
        try {
            conLock.lock();
            try {
                // Don't try to send to a closed connection
                if (!proxyCon.isOpen())
                    return;
                sendCount++;
                if (proxyCon.sendIfPossible(buf.flip()))
		    Log.debug("Sent Command Event: '" + commandString +
			      "', targeting " + target);
            }
            finally {
                conLock.unlock();
            }
        }
        catch (Exception e) {
            throw new RuntimeException(e.getMessage());
        }
    }

    protected void sendExtensionMessage(Long target,
        Map<String,Serializable> properties)
    {
        if (!loggedIn)
            return;
        MVByteBuffer buf = new MVByteBuffer(1024);
        buf.putLong(charOid);
        buf.putInt(83);
        if (target != null) {
            buf.putByte((byte)1);
            buf.putLong(target);
        }
        else
            buf.putByte((byte)0);
        buf.putPropertyMap(properties);
        try {
            conLock.lock();
            try {
                // Don't try to send to a closed connection
                if (!proxyCon.isOpen())
                    return;
                sendCount++;
                if (proxyCon.sendIfPossible(buf.flip()))
		    Log.debug("Sent Extension Event");
            }
            finally {
                conLock.unlock();
            }
        }
        catch (Exception e) {
            throw new RuntimeException(e.getMessage());
        }
    }

}
