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

package multiverse.server.engine;

import multiverse.server.util.*;
import multiverse.server.objects.*;
import multiverse.msgsys.*;

import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;
import java.util.concurrent.*;
import gnu.getopt.*;
import java.lang.management.*; 
import javax.management.*;
import java.text.*;
import java.lang.reflect.Method;
import java.lang.reflect.InvocationTargetException;
import org.python.core.PyException;


// fix loading persistent objects, i call mars.itemtemplatemanager.register()
// from the engine here - but engine should not have mars reference

// checked for locks

/**
 * Base Engine class that maintains static properties for all key
 * server properties, connects to database, handles client connections,
 * and performs other key tasks.
 */
public class Engine
{
    
    /**
     * Constructor gets the properties; creates the management agent
     * if required; and starts the CPU time thread if required.
     */
    public Engine() {
        engineHostName = determineHostName();
        log.info("My local host name is '" + engineHostName + "'");
    	if (isManagementEnabled()) {
            Log.debug("Enabling JMX management agent");
	    createManagementAgent();
    	}

        mxbean = ManagementFactory.getOperatingSystemMXBean();
        cpuMethod = getCPUMethod();
        processorCount = Runtime.getRuntime().availableProcessors();

        // Check to see if the properties file specified cputime logging
        String cpuIntervalString = properties.getProperty("multiverse.cputime_logging_interval");
        int cpuInterval = 0;
        // By default, we log at the debug level
        int logLevel = 1;
        if (cpuIntervalString != null) {
            int p = cpuIntervalString.indexOf(',');
            if (p > 0) {
                String logLevelString = cpuIntervalString.substring(p + 1);
                cpuIntervalString = cpuIntervalString.substring(0, p);
                try {
                    int maybeLogLevel = Integer.parseInt(logLevelString.trim());
                    if (maybeLogLevel >= 0 && maybeLogLevel <= 4)
                        logLevel = maybeLogLevel;
                }
                catch (Exception e) {
                }
            }
            try {
                cpuInterval = Integer.parseInt(cpuIntervalString.trim());
            }
            catch (Exception e) {
            }
        }
        
        if (cpuInterval > 0) {
            runCPUTimeThread = true;
            cpuTimeSamplingInterval = cpuInterval;
            log.debug("multiverse.cputime_logging_interval set to " + cpuInterval + " ms");
        }
        else {
            runCPUTimeThread = false;
            cpuTimeSamplingInterval = cpuInterval;
            log.debug("multiverse.cputime_logging_interval disabled");
        }

        if (runCPUTimeThread) {
            cpuTimeThread = new Thread(new CPUTimeThread(logLevel),"CPUTime");
            cpuTimeThread.start();
        }
        
    }
    
    private static String determineHostName()
    {
        String hostName = System.getProperty("multiverse.hostname");
        if (hostName == null)
            hostName = reverseLocalHostLookup();
        if (hostName == null) {
            log.warn("Could not determine host name from reverse lookup or multiverse.hostname, using 'localhost'");
            hostName = "localhost";
        }
        return hostName;
    }

    private static String reverseLocalHostLookup()
    {
        java.net.InetAddress localMachine = null;
        try {
            localMachine = java.net.InetAddress.getLocalHost();	
            return localMachine.getHostName();
        }
        catch(java.net.UnknownHostException e) {
            log.warn("Could not get host name from local IP address " +
                localMachine);
        }
        return null;
    }

    /**
     * Gets the Engine instance; for now, there are no callers
     * @return Engine instance
     */
    public static Engine getInstance() {
        return instance;
    }

    /*
     * Windows non-cygwin only - Get the process ID and save to file
     * Use the "server name" (multiverse.loggername) Then can use
     * tasklist /fi to get status And tskill processid to stop each
     * server.
     * @param svnName The name of the server process.
     * @param runDir The directory into which the pids will be written.
     */    
    private static void saveProcessID(String svrName, String runDir) {
    	RuntimeMXBean rt = ManagementFactory.getRuntimeMXBean();
    	String pid = rt.getName();
        if (Log.loggingDebug) {
            log.info("PROCESS ID IS " + pid);
            log.info("server name is " + svrName);
        }
    	
	try {
	    if (runDir != null) {
		File outFile = new File(runDir + "\\" + svrName + ".bat");
		PrintWriter out = new PrintWriter( new FileWriter(outFile) );
		out.println( "set pid=" + pid.substring(0, pid.indexOf("@")) );
		out.close();
	    }
	} catch (IOException e){
	    Log.exception("Engine.saveProcessID caught exception", e);
	}
    }

    /**
     * Utility method to dump all the thread stacks of the current process to the log
     */
    public static void dumpAllThreadStacks() {
        Map<Thread, StackTraceElement[]> traces = Thread.getAllStackTraces();
        StringBuilder traceStr = new StringBuilder(1000);
        traceStr.append("Dumping the thread stack for every thread in the process");
        for (Map.Entry<Thread, StackTraceElement[]> entry : traces.entrySet()) {
            Thread thread = entry.getKey();
            StackTraceElement [] elements = entry.getValue();
            traceStr.append("\n\nDumping stack for thread " + thread.getName());
            for (StackTraceElement elem : elements) {
                traceStr.append("\n       at ");
                traceStr.append(elem.toString());
            }
        }
        Log.error(traceStr.toString());
    }

    /**
     * Get the EventServer object, which provides a queue of events.
     * @return EventServer The EventServer instance.
     */
    public static EventServer getEventServer() {
        if (eventServer == null) {
            Log.warn("Engine.getEventServer: creating eventserver (was null)");
            eventServer = new EventServer();
        }
        return eventServer;
    }

    /**
     * Return the thread pool that can run operations some time in the future
     * @return ScheduledThreadPoolExecutor  The ScheduledThreadPoolExecutor instance.
     */
     public static ScheduledThreadPoolExecutor getExecutor() {
        if (executor == null) {
            executor = new ScheduledThreadPoolExecutor(
                Engine.ExecutorThreadPoolSize,
                new NamedThreadFactory("Scheduled"));
        }
        return executor;
    }

    /**
     * Return this engine's interpolator object
     * @return The Interpolator instance.
     */
     public static Interpolator<?> getInterpolator() {
        return interpolator;
    }

     /**
      * Set this engine's interpolator object
      * @param interpolator The interpolator object to associate with
      * this Engine.
      */
     public static void setInterpolator(Interpolator<?> interpolator) {
        Engine.interpolator = interpolator;
    }

     /**
      * Set the time interval between periodic interpolations
      * @param interval The time interval in milliseconds between
      * interpolations.
      */
     public static void setBasicInterpolatorInterval(Integer interval) {
        Engine.interpolator = new BasicInterpolator(interval);
    }

    /**
     * this should only be called by the engine thread
     */
    public static ScriptManager getScriptManager() {
        return scriptManager;
    }

    /**
     * Get the engine's Database object
     * @return The Database instance associated with this Engine.
     */
    public static Database getDatabase() {
        if (db == null) {
            Log.warn("Engine.getDatabase: returning null database object");
        }
        return db;
    }

    /**
     * Set the engine's Database object
     * @param db The Database instance to associate with the Engine.
     */
    public static void setDatabase(Database db) {
        Engine.db = db;
    }

    /**
     * Get the engine's OIDManager object, which hands out oids, and
     * records the ranges used in the database.
     * @return The OIDManager instance.
     */
    public static OIDManager getOIDManager() {
        oidManagerLock.lock();
        try {
            if (oidManager == null) {
                oidManager = new OIDManager();
            }
            return oidManager;
        } finally {
            oidManagerLock.unlock();
        }
    }

    /**
     * Set the engine's OIDManager object.
     * @param o The OIDManager object to associate with this Engine.
     */
    public static void setOIDManager(OIDManager o) {
        oidManagerLock.lock();
        try {
            if (oidManager != null) {
                throw new RuntimeException(
                        "Engine.setOIDManager: oid manager is not null");
            }
            Engine.oidManager = o;
        } finally {
            oidManagerLock.unlock();
        }
    }

    /**
     * Returns the persistence manager, used to save objects into the database
     * at regular intervals.
     * @return The PersistenceManager instance associated with this Engine.
     */
    public static PersistenceManager getPersistenceManager() {
        return persistenceMgr;
    }

    /**
     * The engine main program.  Parses the args to determin the scripts to run;
     * connects to the database and reads the namespace mappings; creates a MessageAgent
     * for the process and connects to the domain server; processes startup scripts;
     * and finally loops forever waiting for shutdown or a request to dump the thread
     * stacks.
     * @param args The command-line arguments.
     */
    public static void main(String args[]) {
        if (args.length < 1) {
            System.err.println("java Engine [-i pre-script ...] [post-script ...]");
            System.exit(1);
        }
        String worldName = System.getProperty("multiverse.worldname");
        String hostName = reverseLocalHostLookup();
        properties = InitLogAndPid.initLogAndPid(args, worldName, hostName);
        
        // Create the Engine instance
        instance = new Engine();

        // Windows non-Cygwin only - save process ID for status script
    	String svrName = System.getProperty("multiverse.loggername");
    	String agentType = System.getProperty("multiverse.agenttype",svrName);
	String runDir = System.getProperty("multiverse.rundir");
        if (System.getProperty("os.name").contains("Windows") && svrName!=null && runDir!=null ) {
	    saveProcessID(svrName, runDir);
        }

        // Process the prescripts indicated by the arguments
        List<String> postScripts = processPreScripts(args);
        
        //
        // connect to the database
        //
        try {
            db = new Database(Engine.getDBDriver());
            if (Log.loggingDebug)
                log.debug("connecting to " + getDBHostname() + "user = " + getDBUser() 
		          + " passwd=" + getDBPassword());
            db.connect(getDBUrl(), getDBUser(), getDBPassword());

        } catch (MVRuntimeException e) {
            Log.exception("Engine.main: error connecting to the database", e);
            System.exit(1);
        }
        Log.info("connected to database");

        Namespace.encacheNamespaceMapping();
        Log.info("encached the mapping of namespace strings to ints");
        
        // Initialize MessageAgent and connect to domain
        agent = new MessageAgent(svrName);
        agent.setDefaultSubscriptionFlags(MessageAgent.BLOCKING);
        agent.setAdvertisementFileName(agentType+"-ads.txt");
        List<MessageType> types = readAdvertisements(agentType);
        types.add(EnginePlugin.MSG_TYPE_PLUGIN_STATE);
        agent.addAdvertisements(types);
        agent.addNoProducersExpected(MessageType.intern("mv.SEARCH"));
        agent.addNoProducersExpected(MessageType.intern("mv.GET_PLUGIN_STATUS"));
        String message_agent_stats =
            properties.getProperty("multiverse.message_agent_stats");
        if (message_agent_stats != null && message_agent_stats.equals("true"))
            agent.startStatsThread();

        try {
            agent.openListener();
            agent.connectToDomain(Engine.getMessageServerHostname(),
                    Engine.getMessageServerPort());
            agent.waitForRemoteAgents();
        }
        catch (Exception ex) {
            Log.exception("Engine.main: domain server "+
                Engine.getMessageServerHostname()+":"+
                (Engine.getMessageServerPort()) + " failed", ex);
            System.exit(1);
        }

        // start up the executor
        executor = new ScheduledThreadPoolExecutor(
            Engine.ExecutorThreadPoolSize,
            new NamedThreadFactory("Scheduled"));

        if (World.getGeometry() == null) {
            Log.warn("engine: world geometry is not set");
        }

        // create an oid manager
        setOIDManager(new OIDManager(db));
        
        processPostScripts(postScripts);

        //
        // go into main loop of handling new client connections
        //
        try {
            while (true) {
                // RDPConnection con = serverSocket.accept();
                // log.debug("engine: accepted new connection " + con);
                if (dumpStacksAndExitIfFileExists.length() > 0) {
                    File dumpFile = new File(dumpStacksAndExitIfFileExists);
                    if (dumpFile.exists()) {
                        Engine.dumpAllThreadStacks();
                        System.exit(0);
                    }
                    Thread.sleep(1000);
                }
                else
                    Thread.sleep(10000);
            }
        } catch (Exception e) {
            Log.exception("Engine.main: error in Thread.sleep", e);
        }
    }

    static private List<MessageType> readAdvertisements(String agentName) {
        List<MessageType> result = new LinkedList<MessageType>();

        String home = System.getenv("MV_HOME");
        String commonFileName = home+"/config/common/"+agentName+"-ads.txt";
        File commonFile = new File(commonFileName);
        String worldFileName = home+"/config/"+getWorldName()+"/"+agentName+"-ads.txt";
        File worldFile = new File(worldFileName);
        if (! commonFile.exists() && ! worldFile.exists()) {
        	Log.warn("Missing advertisements file for agent "+agentName+
        			" for world "+getWorldName() + " in either "+commonFileName+" or "+worldFileName);
        	return result;
        }

        if (commonFile.exists())
            addAdvertisements(commonFileName, result);
        if (worldFile.exists())
            addAdvertisements(worldFileName, result);
        return result;
    }
    
    static void addAdvertisements(String fileName, List<MessageType> result) {
        try {
        	File file = new File(fileName);
            BufferedReader in = new BufferedReader(new FileReader(file));
            String originalLine = null;
            int count = 0;
            while ((originalLine = in.readLine()) != null) {
                count++;
                String line = originalLine.trim();
                int pos = line.indexOf("#");
                if (pos >= 0)
                    line = line.substring(0, pos).trim();
                if (line.length() == 0)
                    continue;
                if (line.indexOf(" ") > 0 || line.indexOf(",") > 0) {
                    Log.error("File '" + fileName + "', line "+count+": unexpected character"); 
                    continue;
                }
                MessageType type = MessageCatalog.getMessageType(line);
                if (type == null) {
                    Log.error("File '" + fileName + "', line "+count+
                        ": unknown message type "+line);
                }
                else {
                    if (!result.contains(type))
                        result.add(type);
                }
            }
            in.close();
        }
        catch (IOException ex) {
            Log.exception(fileName,ex);
        }
    }

    /**
     * Process a set of startup scripts whose file names are given by array arg.
     * These scripts are called 'preScripts' because they must run before the Engine
     * object itself is initialized
     * @param args The command-line args passed in from the main program.
     * @return A list of the "post" scripts to be run after the Engine
     * instance is initialized.
     */
    public static List<String> processPreScripts(String[] args) {
        // list of scripts we load before the network and database starts up
        List<String> preScripts = new LinkedList<String>();

        // list of scripts we load after
        List<String> postScripts = new LinkedList<String>();

        // use getopt to popular the lists
        populateScriptList(args, preScripts, postScripts);

        // load in local javascript config file
        scriptManager = new ScriptManager();
        String scriptName = null;
        try {
            scriptManager.init();
            for (String initScriptFile : preScripts) {
                scriptName = initScriptFile;
                if (Log.loggingDebug)
                    log.debug("Engine: reading in script: " + initScriptFile);

                File f = new File(initScriptFile);
                if (f.exists()) {
                    if (Log.loggingDebug)
                        log.debug("Executing init script file: " + initScriptFile);
                    scriptManager.runFile(initScriptFile);
                    log.debug("script completed");
                } else {
                    Log.warn("didnt find local script file, skipping: " + initScriptFile);
                }
            }
        } catch (Exception e) {
            Log.exception("Engine.processPreScripts: got exception running script '" + scriptName + "'", e);
            System.exit(1);
        }
        return postScripts;
    }
    
    /**
     * Process a set of startup scripts whose file names are given by
     * array arg.  These scripts are called 'postScripts' because they
     * must run after the Engine object itself is initialized
     * @param postScripts A list of the scripts to be run after the
     * Engine object is initialized.
     */
    public static void processPostScripts(List<String> postScripts) {
        //
        // run the rest of the scripts
        //
        scriptManager = new ScriptManager();
        String scriptName = null;
        try {
            scriptManager.init();
            for (String scriptFilename : postScripts) {
                scriptName = scriptFilename;
                if (Log.loggingDebug)
                    log.debug("Executing script file: " + scriptFilename);
                scriptManager.runFile(scriptFilename);
                log.debug("script completed");
            }
        } catch (Exception e) {
            Log.exception("Engine.processPostScripts: got exception running script '" + scriptName + "'", e);
            System.exit(1);
        }
    }
    
    /**
     * Parse the command-line arguments into "pre" scripts, to be run
     * during Engine initialization, and "post" scripts, to be run
     * after the Engine is initialized.
     * @param args The main program's command-line args
     * @param preScripts A list of the file names of scripts to be run
     * during Engine initialization.
     * @param postScripts A list of the file names of scripts to be run
     * after Engine initialization.
     */
    static void populateScriptList(String[] args, List<String> preScripts,
            List<String> postScripts)
    {
        LongOpt[] longopts = new LongOpt[1];
        longopts[0] = new LongOpt("pid", LongOpt.REQUIRED_ARGUMENT, null, 2);
        Getopt g = new Getopt("Engine", args, "i:w:m:t:rgP:", longopts);

        int c;
        String arg;
        while ((c = g.getopt()) != -1) {
            switch (c) {

            // pre-script
            case 'i':
                arg = g.getOptarg();
                if (Log.loggingDebug)
                    log.debug("populateScriptList: option i: " + arg);
                preScripts.add(arg);
                break;

            case '?':
                break; // getopt() already printed an error

            // MarshallingRuntime options - - ignored
            case 'm':
            case 't':
            case 2:
                arg = g.getOptarg();
                break;
                
            case 'r':
            case 'g':
            case 'P':
                break;
                
            default:
                System.out.print("getopt() returned " + c + "\n");
            }
        }
        for (int i = g.getOptind(); i < args.length; i++) {
            if (Log.loggingDebug)
                log.debug("populateScriptList: nonoption args element: " + args[i]);
            postScripts.add(args[i]);
        }
    }
    
    /**
     * Get the interval between saves of dirty objects.
     * @return Milliseconds between saves in the PersistenceManager.
     */
    public long getPersistentObjectSaveIntervalMS() {
    	return PersistentObjectSaveIntervalMS;
    }
    
    /**
     * Set the interval between saves of dirty objects.
     * @param interval Milliseconds between saves in the PersistenceManager.
     */
    public void setPersistentObjectSaveIntervalMS(long interval) {
    	PersistentObjectSaveIntervalMS = interval;
    }
    
    /**
     * Creates the new plugin instance, and registers the plugin with
     * the engine, and calls the plugin's activate method.
     * @param className The class name of the plugin.
     * @return The newly-created plugin.
     */
    public static EnginePlugin registerPlugin(String className) {
        try {
            if (Log.loggingDebug)
                log.debug("Engine.registerPlugin: loading class " + className);
            Class enginePluginClass = Class.forName(className);
            EnginePlugin enginePlugin = (EnginePlugin) enginePluginClass.newInstance();
            registerPlugin(enginePlugin);
            return enginePlugin;
        } catch (Exception e) {
            throw new RuntimeException("could not load and/or activate class", e);
        }
    }

    /**
     * Registers a plugin instance with the engine, and call the
     * plugin's activate method.
     * @param plugin The engine plugin instance to be registered
     */
    public static void registerPlugin(EnginePlugin plugin) {
        if (Log.loggingDebug)
            log.debug("Engine.registerPlugin: registering " + plugin.getName());
        setCurrentPlugin(plugin);
        plugin.activate();
        setCurrentPlugin(null);
        pluginMapLock.lock();
        try {
            pluginMap.put(plugin.getName(), plugin);
        } finally {
            pluginMapLock.unlock();
        }
    }
    
    /**
     * Get the named plugin, or return null if it's not part of this server.
     * @param name The name of the plugin to return
     * @return The plugin with the given name, or null if it can't be found.
     */
    public static EnginePlugin getPlugin(String name) {
        pluginMapLock.lock();
        try {
            return pluginMap.get(name);
        } finally {
            pluginMapLock.unlock();
        }
    }

    /** Get the plugin active on the current thread.
        @return Plugin or null if no active plugin has been set.
    */
    public static EnginePlugin getCurrentPlugin()
    {
        return currentPlugin.get();
    }

    /** Set the plugin active on the current thread.
    */
    public static void setCurrentPlugin(EnginePlugin plugin)
    {
        currentPlugin.set(plugin);
    }

    private static ThreadLocal<EnginePlugin> currentPlugin =
        new ThreadLocal<EnginePlugin>();

    /*
     * Engine properties: Properties file takes precedence over system
     * properties.
     */

    /**
     * Get the engine host name
     * @return The host name of this engine instance.
     */
    public static String getEngineHostName() {
        return engineHostName;
    }

    /**
     * Gets the JDBC driver classname string - default is MySQL driver.
     * @return The class name of the JDBC driver.
     */
    public static String getDBDriver() {
	String driver = properties.getProperty("multiverse.db_driver");
	if (driver == null)
	    driver = "com.mysql.jdbc.Driver";
	return driver;
    }

    /**
     * Sets the JDBC driver classname string.
     * @param driver The JDBC driver classname string.
     */
    public static void setDBDriver(String driver) {
	properties.setProperty("multiverse.db_driver", driver);
    }
        
    /**
     * Gets the database type - default is "mysql".
     * @return The database type.
     */
    public static String getDBType() {
	String dbtype = properties.getProperty("multiverse.db_type");
	if (dbtype == null)
	    return "mysql";
	else
	    return dbtype;
    }

    /**
     * Sets the database type
     * @param dbtype The database type.
     */
    public static void setDBType(String dbtype) {
	properties.setProperty("multiverse.db_type", dbtype);
    }
        
    /**
     * Sets the JDBC connection string (URL).
     * @param url The JDBC connection string.
     */
    public static void setDBUrl(String url) {
	properties.setProperty("multiverse.db_url", url);
    }
        
    /**
     * Gets the JDBC connection string (URL).
     * @return The JDBC connection string.
     */
    public static String getDBUrl() {
	String url = properties.getProperty("multiverse.db_url");
	if (url == null)
	    url = "jdbc:" + getDBType() + "://" + getDBHostname() + "/" + getDBName(); 
	return url;
    }

    /**
     * Gets the database user name.
     * @return The database user name.
     */
    public static String getDBUser() {
	return properties.getProperty("multiverse.db_user");
    }

    /**
     * Sets the database user name.
     * @param username The database user name.
     */
    public static void setDBUser(String username) {
	properties.setProperty("multiverse.db_user", username);
    }

    /**
     * Gets the database password.
     * @return The database password.
     */
    public static String getDBPassword() {
	return properties.getProperty("multiverse.db_password");
    }

    /**
     * Sets the database password.
     * @param password The database password.
     */
    public static void setDBPassword(String password) {
	properties.setProperty("multiverse.db_password", password);
    }

    /**
     * Gets The database host name.
     * @return The database host name.
     */
    public static String getDBHostname() {
	return properties.getProperty("multiverse.db_hostname");
    }

    /**
     * Sets the database host name.
     * @param hostname The database host name.
     */
    public static void setDBHostname(String hostname) {
	properties.setProperty("multiverse.db_hostname", hostname);
    }

    /**
     * Gets the database name - default is "multiverse".
     * @return The database name.
     */
    public static String getDBName() {
	String dbname = properties.getProperty("multiverse.db_name");
	if (dbname == null) {
	    return "multiverse";
	} else {
	    return dbname;
	}
    }

    /**
     * Sets the database name.
     * @param name The database name.
     */
    public static void setDBName(String name) {
	properties.setProperty("multiverse.db_name", name);
    }
        
    /**
     * Gets the name of the msgsys domain server.
     * @return The name of the msgsys domain server.
     */
    public static String getMessageServerHostname() {  
	String msgSvrHostname = defaultMsgSvrHostname;
	msgSvrHostname = properties.getProperty("multiverse.msgsvr_hostname");
	if (msgSvrHostname == null) {
	    msgSvrHostname = defaultMsgSvrHostname; 
	}
	return msgSvrHostname;
    }
        
    /**
     * Sets the name of the msgsys domain server.
     * @param host The name of the msgsys domain server.
     */
    public static void setMessageServerHostname(String host) {
	properties.setProperty("multiverse.msgsvr_port", host);
    }

    /**
     * Gets the msgsys domain server listener port.
     * @return The msgsys domain server listener port.
     */
    public static Integer getMessageServerPort() {
	int msgSvrPort;
	String sMsgSvrPort = properties.getProperty("multiverse.msgsvr_port");
	if (sMsgSvrPort == null) {
	    msgSvrPort = defaultMsgSvrPort;
	} else  {
	    msgSvrPort = Integer.parseInt(sMsgSvrPort.trim());
	}        
	return msgSvrPort;
    }
    
    /**
     * Set the msgsys domain server listener port.
     * @param port The msgsys domain server listener port.
     */
    public static void setMessageServerPort(Integer port) {
        properties.setProperty("multiverse.msgsvr_port", Integer.toString(port));
    }
        
    /**
     * Sets the port number of the world manager.
     * @param port The port number of the world manager.
     */
    public static void setWorldMgrPort(Integer port) {
	properties.setProperty("multiverse.worldmgrport", Integer.toString(port));
    }
        
    /**
     * Get the port number of the world manager.
     * @return The port number of the world manager.
     */
    public static Integer getWorldMgrPort() {
	int port;
	String sWorldMgrPort = properties.getProperty("multiverse.worldmgrport");
	if (sWorldMgrPort == null) {
	    port = defaultWorldmgrport;
	} else  {
	    port = Integer.parseInt(sWorldMgrPort.trim());
	}        
	return port;
    } 
    
    /**
     * Getter for the status reporting interval
     * @return The number of milliseconds between iterations of the
     * status reporting thread.
     */
    public static int getStatusReportingInterval() {
        return statusReportingInterval;
    }

    /**
     * Setter for the status reporting interval
     * @param statusReportingInterval The number of milliseconds between iterations of the
     * status reporting thread.
     */
    public static void setStatusReportingInterval(int statusReportingInterval) {
        Engine.statusReportingInterval = statusReportingInterval;
    }

    public static Properties getProperties() {
        return properties;
    }

    /**
     * Get the string property value for the given property name,
     * either from the System property of that name, or from the
     * properties read from the propery file.
     * @param propName The name of the property whose value should be
     * returned.
     * @return The value associated with propName, or null if there is
     * none.
     */
    public static String getProperty(String propName) {
        return properties.getProperty(propName);
    }

    /**
     * Set the string property value for the given property name, both in
     * the System property as well as the encached Properties map.  
     * @param propName The name of the property whose value should be
     * set.
     * @param propValue The value to associate with the propName.
     */
    public static void setProperty(String propName, String propValue) {
        properties.setProperty(propName, propValue);
    }
    
    /**
     * Get the named property, coercing it to an int.
     * @param propName The property name.
     * @return The integer property whose name is propName.
     * @see #getProperty
     */
    public static Integer getIntProperty(String propName) {
        String intString = properties.getProperty(propName);
        if (intString == null)
            return null;
	try {
	    return Integer.valueOf(intString.trim());
	}
	catch (NumberFormatException e) {
	    Log.error("Property '"+propName+"' value '"+intString.trim()+"' is not an integer.");
	    return null;
	}
    }
    
    /**
     * The engine should know the world name. it uses it when it creates
     * characters in the database.  This is different from the server id.
     * @return The value associated with property "multiverse.worldname".
     */
    public static String getWorldName() {
	return System.getProperty("multiverse.worldname");
    }

    /**
     * Set the world name to be used by the engine.
     * @param worldName The value to associate with property
     * "multiverse.worldname".
     */
    public static void setWorldName(String worldName) {
    	properties.setProperty("multiverse.worldname", worldName);
    }

    /**
     * Get the log level from the properties file or sys. prop, or
     * default to 1 (debug).  Plugins are free to subsequently set the
     * log level to a different value.
     * @return The current log level value.
     */ 
    public static String getLogLevel() {
        String logLevelString = getProperty("multiverse.log_level");
        if (logLevelString == null) {
        	logLevelString = "1";
        }
        return logLevelString;
    }

    /**
     * Set the log level.
     * @param level The new log level value.
     */
    public static void setLogLevel(String level) {
    	properties.setProperty("multiverse.log_level", level);
    }

    /**
     * Get whether management is enabled
     * @return True if management is enabled.
     */
    public static boolean isManagementEnabled()
    {
	String mgmt = properties.getProperty("com.sun.management.jmxremote");
	if (mgmt==null) {
	    return false;
	} else {
	    return true;
	}        			
    }

    /**
     * Get the MessageAgent instance.
     * @return The MessageAgent instance.
     */
    public static MessageAgent getAgent()
    {
        return agent;
    }

    /**
     * Make a map from the comma-separated name=value pairs
     * @param str The string containing the comma-separated name=value pairs
     * @return The map generated from the pairs
     */
    public static Map<String, String> makeMapOfString(String str) {
        Map<String, String> propMap = new HashMap<String, String>();
        String[] keysAndValues = str.split(",");
        for (String keyAndValue : keysAndValues) {
            String[] ss = keyAndValue.split("=");
            if (ss.length == 2)
                propMap.put(ss[0], ss[1]);
            else
                Log.error("Engine.makeMapOfString: Could not parse name/value string '" + str + "' at '"+keyAndValue+"'");
        }
        return propMap;
    }
    
    /**
     * Make a string from the map of name/value pairs
     * @param propMap The map of name/value pairs
     * @return The string synthesized from the map.
     */
    public static String makeStringFromMap(Map<String, String> propMap) {
        String s = "";
        if (propMap == null)
            return s;
        for (Map.Entry<String, String> pair : propMap.entrySet()) {
            if (s != "")
                s += ",";
            s += pair.getKey() + "=" + pair.getValue();
        }
        return s;
    }

    /**
     * A Runnable utility class used to handle callbacks that require
     * a thread pool because it takes a long time or blocks.
     */
    static class QueuedMessage implements Runnable
    {
        /**
         * Builds a QueuedMessage instance.
         * @param message The message to be queued for execution.
         * @param flags The message flags.
         * @param callback The MessageCallback instance to run when
         * the message is dequeued.
         */
        QueuedMessage(Message message, int flags, MessageCallback callback)
        {
            this.message = message;
            this.flags = flags;
            this.callback = callback;
        }

        /**
         * The run method just invokes the callback on the message and
         * flags.
         */
        public void run() {
            try {
                callback.handleMessage(message,flags);
            }
            catch (Exception ex) {
                Log.exception("Engine message handler: "+message.getMsgType(), ex);
            }
        }

        Message message;
        int flags;
        MessageCallback callback;
    }

    /**
     * The default message dispatcher.
     * @param message The message to be dispatched.
     * @param flags The message flags.
     * @param callback The MessageCallback instance to run when
     * the message is dequeued.
     */
    public static void defaultDispatchMessage(Message message, int flags,
        MessageCallback callback)
    {
        if (Log.loggingDebug)
            Log.debug("defaultDispatchMessage "+message.getSenderName()+","+
                message.getMsgId() + " " + message.getMsgType());
        if (defaultMessageExecutor == null)
            defaultMessageExecutor = Executors.newFixedThreadPool(10,
                new NamedThreadFactory("EngineDispatch"));
        defaultMessageExecutor.execute(
            new QueuedMessage(message,flags,callback));
    }

    /**
     * Register the plugin passed in with Engine for status reporting
     * with the database.
     * @param plugin An EnginPlugin instance.
     */
    public static void registerStatusReportingPlugin(EnginePlugin plugin) {
        statusReportingLock.lock();
        try {
            if (Log.loggingDebug)
                Log.debug("Engine.registerStatusReportingPlugin: Registering plugin " + plugin.getName() + 
                    " of type " + plugin.getPluginType());
            if (statusReportingPlugins == null) {
                // We need to make the HashSet and start the thread
                statusReportingPlugins = new HashSet<EnginePlugin>();
                // If we haven't already set up the mxbean, do it now
                if (mxbean == null)
                    mxbean = ManagementFactory.getOperatingSystemMXBean();
                statusReportingThread = new Thread(new StatusReportingThread(),"StatusReporting");
                statusReportingThread.start();
            }
            if (!Engine.getDatabase().registerStatusReportingPlugin(plugin,getAgent().getDomainStartTime()))
                log.error("Engine.registerStatusReportingPlugin: Registration of plugin '" + plugin.getName() + "' failed!");
            else
                statusReportingPlugins.add(plugin);
        }
        finally {
            statusReportingLock.unlock();
        }
    }
    
    /**
     * Get method capable of returning process CPU usage.
     * @return A Method object
     */
    protected static Method getCPUMethod() {
        try {
            Class operatingSystemMXBean = null;
            operatingSystemMXBean = getParentInterface(mxbean.getClass(),
                "com.sun.management.OperatingSystemMXBean");
            if (operatingSystemMXBean == null) {
                throw new ClassNotFoundException("OperatingSystemMXBean is not a super-class of the management bean");
            }
            return operatingSystemMXBean.getMethod("getProcessCpuTime");
        }
        catch (NoSuchMethodException ex) {
            Log.exception("CPU time will not be reported", ex);
        }
        catch (ClassNotFoundException ex) {
            Log.exception("CPU time will not be reported", ex);
        }
        return null;
    }

    /**
     * Common method to get the process CPU time.
     * @param cpuMethod A Method object that returns the process CPU time.
     * @param mxbean The JavaBean used to get the CPU time.
     * @return The cumulative CPU time for the Engine process.
     */
    protected static long getProcessCpuTime(Method cpuMethod, Object mxbean)
    {
        if (cpuMethod == null)
            return 0;
        try {
            return (Long) cpuMethod.invoke(mxbean);
        }
        catch (IllegalAccessException ex) {
            Log.exception("Failed getting CPU time", ex);
        }
        catch (InvocationTargetException ex) {
            Log.exception("Failed getting CPU time", ex);
        }
        return 0;
    }

    /**
     * Common method to get the parent interface
     * @param cl The class whose parent interface is requested.
     * @param name The name of the interface.
     * @return The Class object representing the parent interface.
     */
    protected static Class getParentInterface(Class cl, String name)
    {
        if (cl.getName().equals(name)) {
            return cl;
        }
        Class[] interfaces = cl.getInterfaces();
        for (int ii = 0; ii < interfaces.length; ii++) {
            Class match = getParentInterface(interfaces[ii],name);
            if (match != null)
                return match;
        }
        return null;
    }

    /**
     * The Executor that runs the QueuedMessages.
     */
    private static Executor defaultMessageExecutor;

    /**
     * Boolean saying whether the CPU time thread should be run.
     */
    private static boolean runCPUTimeThread = false;

    /**
     * The interval in milliseconds between runs of the CPU time thread.
     */
    private static int cpuTimeSamplingInterval = 250;
    
    /**
     *  The JavaBean used to get process CPU time
     */
    private static Object mxbean = null;

    /**
     * The Thread instance that runs the CPU time loop.
     */
    private static Thread cpuTimeThread = null;

    /**
     * The Runnable class that runs the CPU time loop.
     */
    static class CPUTimeThread implements Runnable {
        
        public CPUTimeThread(int logLevel) {
            this.logLevel = logLevel;
        }
        
        /**
         * Loops, sleeping for cpuTimeSamplingInterval milliseconds,
         * then waking up and displaying the total process CPU time,
         * and the time since the last loop iteration.
         */
        public void run() {

            float lastCPUTime = ((float)(getProcessCpuTime(cpuMethod, mxbean) / 1000000L)) / 1000.0f;
            long lastTime = System.currentTimeMillis();
            DecimalFormat timeFormatter = new DecimalFormat("####.000");
            while (true) {
                try {
                    Thread.sleep(cpuTimeSamplingInterval);
                }
                catch (Exception e) {
                    log.exception("CPUTimeThread.run exception", e);
                }
                long currentTime = System.currentTimeMillis();
                float currentCPUTime = ((float)(getProcessCpuTime(cpuMethod, mxbean) / 1000000L)) / 1000.0f;
                float diff = currentCPUTime - lastCPUTime;
                long msDiff = (currentTime - lastTime) * processorCount;
                float secsDiff = ((float)msDiff) / 1000.0f;
                int percentDiff = (int)(diff * 100.0f / secsDiff);
                if (Log.getLogLevel() <= logLevel)
                    Log.logAtLevel(logLevel, "Process CPU time: " + timeFormatter.format(currentCPUTime)+
                        ", CPU time since last " + timeFormatter.format(diff) + 
                        ", " + percentDiff + "% CPU");
                lastTime = currentTime;
                lastCPUTime = currentCPUTime;
            }
        }

        int logLevel;
    }
    
    /**
     * The thread that reports status for registered plugins.  We
     * don't create the thread until the first plugin registers
     * itself.
     */
    private static Thread statusReportingThread = null;
    
    /**
     * The set of plugins that have registered themselves for status reporting.
     */
    private static Set<EnginePlugin> statusReportingPlugins = null;
    
    /**
     * The interval between runs of the status reporting thread.
     */
    private static int statusReportingInterval = 5000;
    
    /**
     * The lock protecting the map of plugin names to plugin
     * instances.
     */
    private static Lock statusReportingLock = LockFactory.makeLock("statusReportingLock");

    /**
     * The Runnable class that performs status reporting on behalf of
     * plugins running in this Engine
     */
    static class StatusReportingThread implements Runnable {
        
        public StatusReportingThread() {
        }
        
        /**
         * Loops, sleeping for cpuTimeSamplingInterval milliseconds,
         * then waking up and displaying the total process CPU time,
         * and the time since the last loop iteration.
         */
        public void run() {

            lastCPUTime = ((float)(getProcessCpuTime(cpuMethod, mxbean) / 1000000L)) / 1000.0f;
            lastTime = System.currentTimeMillis();
            while (true) {
                try {
                    Thread.sleep(statusReportingInterval);
                }
                catch (InterruptedException e) {
                    log.exception("StatusReportingThread.run exception", e);
                }
                try {
                    updateStatus();
                }
                catch (Exception e) {
                    log.exception("StatusReportingThread.run", e);
                    /* ignore */
                }
            }
        }

        private void updateStatus()
        {
            if (Log.loggingDebug)
                Log.debug("Engine.StatusReportingThread.run: count of status reporting plugins is " + statusReportingPlugins.size());
            long currentTime = System.currentTimeMillis();
            float currentCPUTime = ((float)(getProcessCpuTime(cpuMethod, mxbean) / 1000000L)) / 1000.0f;
            float diff = currentCPUTime - lastCPUTime;
            long msDiff = (currentTime - lastTime) * processorCount;
            float secsDiff = ((float)msDiff) / 1000.0f;
            int percentDiff = (int)(diff * 100.0f / secsDiff);
            if (Log.loggingDebug)
                Log.debug("Engine.StatusReportingThread: " +
                    statusReportingPlugins.size() + " plugins, " +
                    percentDiff + "% CPU");
            for (EnginePlugin plugin : statusReportingPlugins) {
                plugin.setPercentCPULoad(percentDiff);
                Engine.getDatabase().updatePluginStatus(plugin, System.currentTimeMillis() + statusReportingInterval);
            }
            lastTime = currentTime;
            lastCPUTime = currentCPUTime;
        }

        float lastCPUTime;
        long lastTime;
    }

    // ---Members---
    
    protected static Method cpuMethod;
    protected static int processorCount;

    static final Logger log = new Logger("Engine");
    
    /**
     * The Properties instance, typically read from file
     * $MV_HOME/bin/multiverse.properties
     */
    public static Properties properties = new Properties();

    /**
     * Determines the size of the scheduled thread pool.  To take
     * effect, must be defined by a pre-script, e.g., proxy.py
     */
    public static int ExecutorThreadPoolSize = 10;

    /**
     * The scriptManager instance, used to run Python files.
     */
    private static ScriptManager scriptManager = null;

    /**
     * The maximum size of an RDP or aggregated RDP packet, and the
     * size at which the packet aggregator will send a packet even
     * if the aggregation interval has not expired
     */
    public static int MAX_NETWORK_BUF_SIZE = 1000;

    /**
     * Scripting should set this to the name of a file which, if 
     * the file exists, causes the engine to dump the stacks of all
     * threads and exit the process
     */
    public static String dumpStacksAndExitIfFileExists = "";

    /**
     * The default port used to connect to the world manager.
     */
    private static Integer defaultWorldmgrport = 5040;

    /**
     * The default domain server host name.
     */
    private static String defaultMsgSvrHostname = "localhost";

    /**
     * The fall-back host name for this Engine
     */
    private static String engineHostName = "localhost";

    /** 
     * The default domain server port number.
     */
    private static Integer defaultMsgSvrPort = DomainServer.DEFAULT_PORT;

    /**
     * The Engine instance.
     */
    private static Engine instance = null;
    
    /**
     * The default interval between PersistenceManager saves.
     */
    private static final int DEFAULT_PERSISTENT_OBJ_SAVE_INTERVAL = 600000;
    
    /**
     * The current interval in millisecond between periodic saves of
     * modified persistent objects to the database.
     */
    public long PersistentObjectSaveIntervalMS = DEFAULT_PERSISTENT_OBJ_SAVE_INTERVAL;

    /**
     * The lock protecting the map of plugin names to plugin
     * instances.
     */
    private static Lock pluginMapLock = LockFactory.makeLock("pluginMapLock");

    /**
     * The map of plugin names to plugin instances.
     */
    private static Map<String, EnginePlugin> pluginMap = new HashMap<String, EnginePlugin>();
    
    /**
     * The lock used to protect access to the oidManager.
     */
    private static Lock oidManagerLock = LockFactory.makeLock("oidManagerLock");

    /**
     * The OIDManager instance.
     */
    private static OIDManager oidManager = null;

    /**
     * The EventServer instance.
     */
    private static EventServer eventServer = null;

    /**
     * The Executor for QueuedMessage instances.
     */
    private static ScheduledThreadPoolExecutor executor = null;

    /**
     * The Interpolator instance associated with this Engine.
     */
    private static Interpolator interpolator = null;

    /**
     * The PersistenceManager associated with this Engine instance.
     */
    static PersistenceManager persistenceMgr = new PersistenceManager();

    /**
     * The Database instance associated with this Engine instance.
     */
    private static Database db = null;

    /**
     * The MessageAgent associated with this Engine instance.
     */
    private static MessageAgent agent = null;

    private static MBeanServer mbeanServer = null;

    /**
     * Create an MBeanServer for this Engine instance.
     */
    private void createManagementAgent() {
        mbeanServer = ManagementFactory.getPlatformMBeanServer(); 
        try {
	    ObjectName name = new ObjectName("net.multiverse:type=Engine");
	    mbeanServer.registerMBean(createMBeanInstance(), name); 
	    Log.debug("Registered Engine with JMX management agent");
        } catch (javax.management.JMException ex) {
            Log.exception("Engine.createManagementAgent: exception in registerMBean", ex);
        }
    }

    /** Return JMX MBean instance object.  Over-ride to provide your
        own MBean implementation.
    */
    protected Object createMBeanInstance() {
        return new EngineJMX();
    }

    /** Get the JMX top-level agent.
    */
    public static MBeanServer getManagementAgent() {
        return mbeanServer;
    }

    public static interface EngineJMXMBean {
        public String getVersion();
        public String getFullVersion();
        public String getBuildNumber();
        public String getBuildDate();
        public String getBuildString();

        public String getAgentName();
        public String getWorldName();
        public String getPlugins();

        public int getLogLevel();
        public String getLogLevelString();
        public void setLogLevel(int level);

        public long getPersistentObjectSaveIntervalMS();
        public void setPersistentObjectSaveIntervalMS(long interval);

        public boolean getCPUTimeMonitor();
        public int getCPUTimeMonitorIntervalMS();
        public void setCPUTimeMonitorIntervalMS(int milliSeconds);

        public int getEntities();

        public String runPythonScript(String script);
        public String evalPythonScript(String script);
    }

    protected static class EngineJMX implements EngineJMXMBean {
        protected EngineJMX() { }

        public String getVersion()
        {
            return ServerVersion.ServerMajorVersion;
        }

        public String getFullVersion()
        {
            return ServerVersion.getVersionString();
        }

        public String getBuildNumber()
        {
            return ServerVersion.getBuildNumber();
        }

        public String getBuildDate()
        {
            return ServerVersion.getBuildDate();
        }

        public String getBuildString()
        {
            return ServerVersion.getBuildString();
        }

        public String getAgentName() {
            return agent.getName();
        }

        public String getWorldName() {
            return Engine.getWorldName();
        }

        public String getPlugins() {
            String plugins = "";
            for (String name : pluginMap.keySet()) {
                if (! plugins.equals(""))
                    plugins += ",";
                plugins += name;
            }
            return plugins;
        }

        public int getLogLevel()
        {
            return Log.getLogLevel();
        }

        public String getLogLevelString()
        {
            int level = Log.getLogLevel();
            if (level == 0) return "TRACE";
            if (level == 1) return "DEBUG";
            if (level == 2) return "INFO";
            if (level == 3) return "WARN";
            if (level == 4) return "ERROR";
            return "unknown";
        }

        public void setLogLevel(int level)
        {
            Log.setLogLevel(level);
        }

        /**
         * Get the interval between saves of dirty objects.
         * @return Milliseconds between saves in the PersistenceManager.
         */
        public long getPersistentObjectSaveIntervalMS() {
            return Engine.getInstance().PersistentObjectSaveIntervalMS;
        }
        
        /**
         * Set the interval between saves of dirty objects.
         * @param interval Milliseconds between saves in the PersistenceManager.
         */
        public void setPersistentObjectSaveIntervalMS(long interval) {
            Engine.getInstance().PersistentObjectSaveIntervalMS = interval;
        }
    
        /** For EngineMBean */
        public boolean getCPUTimeMonitor()
        {
            return runCPUTimeThread;
        }

        /** For EngineMBean */
        public int getCPUTimeMonitorIntervalMS()
        {
            return cpuTimeSamplingInterval;
        }

        /** For EngineMBean */
        public void setCPUTimeMonitorIntervalMS(int milliSeconds)
        {
            if (milliSeconds > 0)
                cpuTimeSamplingInterval = milliSeconds;
        }

        public int getEntities()
        {
            return EntityManager.getEntityCount();
        }

        protected static String defaultPythonImports=
                "import sys\n"+
                "from multiverse.mars import *\n"+
                "from multiverse.mars.objects import *\n"+
                "from multiverse.mars.core import *\n"+
                "from multiverse.mars.events import *\n"+
                "from multiverse.mars.util import *\n"+
                "from multiverse.mars.plugins import *\n"+
                "from multiverse.msgsys import *\n"+
                "from multiverse.server.plugins import *\n"+
                "from multiverse.server.math import *\n"+
                "from multiverse.server.events import *\n"+
                "from multiverse.server.objects import *\n"+
                "from multiverse.server.worldmgr import *\n"+
                "from multiverse.server.engine import *";

        public String runPythonScript(String script)
        {
            initScriptManager();
            try {
                ScriptManager.ScriptOutput output = mbeanScriptManager.runPYScript(script);
                if (output.stderr == null || output.stderr.equals(""))
                    return output.stdout;
                else
                    return "OUT: "+output.stdout+"\nERR: "+output.stderr;
            }
            catch (PyException e) {
                return e.toString();
            }
        }

        public String evalPythonScript(String script)
        {
            initScriptManager();
            try {
                return mbeanScriptManager.evalPYScriptAsString(script);
            }
            catch (PyException e) {
                return e.toString();
            }
        }

        protected static void initScriptManager()
        {
            if (mbeanScriptManager != null)
                return;

            mbeanScriptManager = new ScriptManager();
            mbeanScriptManager.initLocal();
            try {
                mbeanScriptManager.runPYScript(defaultPythonImports);
            }
            catch (PyException e) {
                Log.exception("EngineJMX.initScriptManager",e);
            }
        }

        protected static ScriptManager mbeanScriptManager;

    }
}
