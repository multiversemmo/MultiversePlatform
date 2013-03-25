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

package multiverse.msgsys;

import java.io.*;
import java.util.*;
import java.lang.management.ManagementFactory;
import java.lang.management.RuntimeMXBean;
import java.nio.channels.*;
import java.nio.ByteBuffer;
import java.util.concurrent.*;
import java.net.InetAddress;
import gnu.getopt.Getopt;
import gnu.getopt.LongOpt;

import multiverse.msgsys.MessageTypes;

import multiverse.server.util.Log;
import multiverse.server.util.FileUtil;
import multiverse.server.util.Base64;
import multiverse.server.util.ServerVersion;
import multiverse.server.util.SecureTokenUtil;
import multiverse.server.util.InitLogAndPid;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.network.TcpServer;
import multiverse.server.network.TcpAcceptCallback;
import multiverse.server.network.ChannelUtil;
import multiverse.server.marshalling.MarshallingRuntime;


/** Message system domain server.  A message domain is a set of
communicating {@link MessageAgent MessageAgents} using the same
DomainServer.  The DomainServer maintains a registry of all member
agents and keeps the agents informed of membership changes.  No
subscription or message traffic passes through the domain server.
<p>
The domain server port defaults to 20374.  Over-ride with property
multiverse.msgsvr_port or command-line option <code>-p</code>.
<p>
Command-line parameters:
<li>-a &lt;agent-name&gt;<br>
Specify the names of all agents in the domain.  Repeat the option
for each agent.
<li>-p &lt;port&gt;<br>
Domain server port number.

*/
public class DomainServer implements TcpAcceptCallback, MessageIO.Callback
{
    public static final int DEFAULT_PORT = 20374;

    public static void main(String args[])
    {
        String worldName = System.getProperty("multiverse.worldname");
        Properties properties = InitLogAndPid.initLogAndPid(args, worldName, null);

        System.err.println("Multiverse server version "+
                ServerVersion.getVersionString());

        List<String> agentNames = new LinkedList<String>();

        LongOpt[] longopts = new LongOpt[2];
        longopts[0] = new LongOpt("pid", LongOpt.REQUIRED_ARGUMENT, null, 2);
        longopts[1] = new LongOpt("port", LongOpt.REQUIRED_ARGUMENT, null, 3);
        Getopt opt = new Getopt("DomainServer", args, "a:m:t:p:P:", longopts);
        int c;
        int port = DEFAULT_PORT;

        String portStr = properties.getProperty("multiverse.msgsvr_port");
        if (portStr != null)
            port = Integer.parseInt(portStr);

        PluginStartGroup pluginStartGroup = new PluginStartGroup();

        while ((c = opt.getopt()) != -1) {
            switch (c) {
            case 'a':
                agentNames.add(opt.getOptarg());
                break;
            case 't':
            case 'm':
                // ignore RuntimeMarshalling flags
                opt.getOptarg();
                break;
            case 'p':
                String pluginSpec = opt.getOptarg();
                String[] pluginDef = pluginSpec.split(",",2);
                if (pluginDef.length != 2) {
                    System.err.println("Invalid plugin spec format: "+pluginSpec);
                    Log.error("Invalid plugin spec format: "+pluginSpec);
                    System.exit(1);
                }
                int expected = Integer.parseInt(pluginDef[1]);
                pluginStartGroup.add(pluginDef[0], expected);
                break;
            case '?':
                System.exit(1);
                break;
            case 'P':
                break;
            case 2:
                // ignore --pid
                opt.getOptarg();
                break;
                // port
            case 3:
                String arg = opt.getOptarg();
                port = Integer.parseInt(arg);
                break;
            default:
                break;
            }
        }

    	String svrName = System.getProperty("multiverse.loggername");
    	String runDir = System.getProperty("multiverse.rundir");

        // Windows non-Cygwin only - save process ID for status script
    	if (System.getProperty("os.name").contains("Windows") &&
                svrName != null && runDir != null ) {
            saveProcessID(svrName, runDir);
    	}

        // parse command-line options

        domainServer = new DomainServer(port);
        domainServer.setAgentNames(agentNames);
        domainServer.setWorldName(worldName);
        domainServer.start();

        pluginStartGroup.prepareDependencies(properties, worldName);
        domainServer.addPluginStartGroup(pluginStartGroup);
        pluginStartGroup.pluginAvailable("Domain","Domain");

        String timeoutStr = properties.getProperty("multiverse.startup_timeout");
        int timeout = 120;
        if (timeoutStr != null) {
            timeout = Integer.parseInt(timeoutStr);
        }

        ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);
        ScheduledFuture<?> timeoutHandler =
            scheduler.schedule(new TimeoutRunnable(timeout),
                timeout, TimeUnit.SECONDS);

        javax.crypto.SecretKey domainKey = SecureTokenUtil.generateDomainKey();
        // XXX Use a random keyID for now. Ideally, this would be semi-unique.
        long keyId = new Random().nextLong();
        encodedDomainKey = Base64.encodeBytes(SecureTokenUtil.encodeDomainKey(keyId, domainKey));
        Log.debug("generated domain key: " + encodedDomainKey);

        try {
            pluginStartGroup.awaitDependency("Domain");
            timeoutHandler.cancel(false);
            String availableMessage =
                properties.getProperty("multiverse.world_available_message");
            String availableFile =
                properties.getProperty("multiverse.world_available_file");
            if (availableFile != null)
                touchFile(FileUtil.expandFileName(availableFile));
            if (availableMessage != null)
                System.err.println("\n"+availableMessage);
            while (true) {
                Thread.sleep(10000000);
            }
        }
        catch (Exception ex) {
            Log.exception("DomainServer.main", ex);
        }
    }

    static DomainServer domainServer;

    public DomainServer(int port)
    {
        MessageTypes.initializeCatalog();
        listenPort = port;
        messageIO = new MessageIO(this);
    }

    public void setAgentNames(List<String> names)
    {
        agentNames = new LinkedList<String>(names);
    }

    public List<String> getAgentNames()
    {
        return agentNames;
    }

    public String getWorldName()
    {
        return worldName;
    }

    public void setWorldName(String worldName)
    {
        this.worldName = worldName;
    }

    public void start()
    {
        try {
            domainStartTime = System.currentTimeMillis();
            messageIO.start();
            listener = new TcpServer(listenPort);
            listener.registerAcceptCallback(this);
            listener.start();
        } catch (Exception e) {
            Log.exception("DomainServer listener", e);
            System.exit(1);
        }
    }

    public void onTcpAccept(SocketChannel agentSocket) {
        Log.debug("Got connection: "+agentSocket);
        try {
            threadPool.execute(new AgentHandler(agentSocket));
        } catch (IOException ex) {
            Log.exception("DomainServer listener", ex);
        }
    }

    // Internal methods

    private class AgentHandler implements Runnable {
        public AgentHandler(SocketChannel socket)
                throws IOException
        {
            agentSocket = socket;
        }

        public void run()
        {
            try {
                while (handleMessage()) {
                }
            } catch (InterruptedIOException ex) {
                Log.info("DomainServer: closed connection due to timeout " +
                    agentSocket);
            } catch (IOException ex) {
                Log.info("DomainServer: agent closed connection " +
                    agentSocket);
            } catch (Exception ex) {
                Log.exception("DomainServer.SocketHandler: ", ex);
            }

            try {
                if (agentSocket != null)
                    agentSocket.close();
            }
            catch (IOException ex) { /* ignore */ }
        }

        public boolean handleMessage()
            throws java.io.IOException
        {
                ByteBuffer buf = ByteBuffer.allocate(4);
                int nBytes = ChannelUtil.fillBuffer(buf,agentSocket);
                if (nBytes == 0)  {
                    Log.info("DomainServer: agent closed connection " +
                        agentSocket);
                    return false;
                }
                if (nBytes < 4)  {
                    Log.error("DomainServer: invalid message "+nBytes);
                    return false;
                }

                int msgLen = buf.getInt();
                if (msgLen < 0) {
                    return false;
                }

                MVByteBuffer buffer = new MVByteBuffer(msgLen);
                nBytes = ChannelUtil.fillBuffer(buffer.getNioBuf(),agentSocket);
                if (nBytes == 0)  {
                    Log.info("DomainServer: agent closed connection " +
                        agentSocket);
                    return false;
                }
                if (nBytes < msgLen)  {
                    Log.error("DomainServer: invalid message, expecting "+
                        msgLen + " got "+nBytes + " from " + agentSocket);
                    return false;
                }

                Message message =
                    (Message) MarshallingRuntime.unmarshalObject(buffer);

                if (message instanceof AgentHelloMessage) {
                    // Successfully added agent, clear our socket var
                    // so we don't close it.
                    if (handleAgentHello((AgentHelloMessage) message))
                        agentSocket = null;
                    return false;
                }
                else if (message instanceof AllocNameMessage)
                    handleAllocName((AllocNameMessage) message, agentSocket);

                return true;
        }

        boolean handleAgentHello(AgentHelloMessage agentHello)
            throws java.io.IOException
        {
            if (agentHello.getMsgType() != MessageTypes.MSG_TYPE_AGENT_HELLO) {
                Log.error("DomainServer: invalid agent hello, got message type "
                    + agentHello.getMsgType() + " from " + agentSocket);
                return false;
            }

            int agentId;
            synchronized (domainServer) {
                agentId = getNextAgentId();
                if (! agentNames.contains(agentHello.getAgentName()))
                    agentNames.add(agentHello.getAgentName());
            }

            MVByteBuffer buffer = new MVByteBuffer(1024);
            HelloResponseMessage helloResponse =
                new HelloResponseMessage(agentId, domainStartTime, agentNames, encodedDomainKey);
            Message.toBytes(helloResponse, buffer);
            buffer.flip();

            if (! ChannelUtil.writeBuffer(buffer, agentSocket)) {
                Log.error("could not write to new agent, "+agentSocket);
                return false;
            }

            addNewAgent(agentId, agentSocket,
                    agentHello.getAgentName(), agentHello.getAgentIP(),
                    agentHello.getAgentPort(), agentHello.getFlags());

            return true;
        }

        SocketChannel agentSocket;
    }

    void handleAllocName(AllocNameMessage allocName, SocketChannel agentSocket)
        throws java.io.IOException
    {
        if (allocName.getMsgType() != MessageTypes.MSG_TYPE_ALLOC_NAME) {
            Log.error("DomainServer: invalid alloc name message");
            return;
        }

        String agentName =
            allocName(allocName.getType(), allocName.getAgentName());

        MVByteBuffer buffer = new MVByteBuffer(1024);
        AllocNameResponseMessage allocNameResponse =
                new AllocNameResponseMessage(allocName, agentName);
        Message.toBytes(allocNameResponse, buffer);
        buffer.flip();

        if (! ChannelUtil.writeBuffer(buffer, agentSocket)) {
            throw new RuntimeException("could not write alloc name response");
        }

    }

    void handleAwaitPluginDependents(AwaitPluginDependentsMessage await,
        SocketChannel agentSocket)
        throws java.io.IOException
    {
        if (await.getMsgType() != MessageTypes.MSG_TYPE_AWAIT_PLUGIN_DEPENDENTS) {
            Log.error("DomainServer: invalid await message");
            return;
        }

        if (pluginStartGroup == null) {
            Log.error("DomainServer: no start group defined for plugin type="+
                await.getPluginType() + " name=" + await.getPluginName());
//## return error
            return;
        }

        
        new Thread(new PluginDependencyWatcher(await, agentSocket)).start();

    }

    void handlePluginAvailable(PluginAvailableMessage available,
        SocketChannel agentSocket)
        throws java.io.IOException
    {
        if (available.getMsgType() != MessageTypes.MSG_TYPE_PLUGIN_AVAILABLE) {
            Log.error("DomainServer: invalid available message");
            return;
        }

        if (pluginStartGroup == null) {
            Log.error("DomainServer: no start group defined for plugin type="+
                available.getPluginType() + " name=" + available.getPluginName());
//## return error
            return;
        }

        pluginStartGroup.pluginAvailable(available.getPluginType(),
            available.getPluginName());

    }


    private int getNextAgentId()
    {
        return nextAgentId++;
    }

    private synchronized void addNewAgent(int agentId, SocketChannel socket,
                String agentName, String agentIP, int agentPort, int flags)
    {
        if (agentIP.equals(":same")) {
            InetAddress agentAddress = socket.socket().getInetAddress();
            agentIP = agentAddress.getHostAddress();
        }

        Log.info("New agent id="+agentId+" name="+agentName+" address="+
                agentIP+":"+agentPort + " flags="+flags);
        AgentInfo agentInfo = new AgentInfo();
        agentInfo.agentId = agentId;
        agentInfo.flags = flags;
        agentInfo.socket = socket;
        agentInfo.agentName = agentName;
        agentInfo.agentIP = agentIP;
        agentInfo.agentPort = agentPort;
        agentInfo.outputBuf = new MVByteBuffer(1024);
        agentInfo.inputBuf = new MVByteBuffer(1024);
        agents.put(socket,agentInfo);

        NewAgentMessage newAgentMessage = new NewAgentMessage(agentId,
                agentName, agentIP, agentPort, flags);
        for (Map.Entry<SocketChannel,AgentInfo> entry : agents.entrySet()) {
            if (entry.getKey() == socket)
                continue;

            // Tell other agents about the new one
            synchronized (entry.getValue().outputBuf) {
                Message.toBytes(newAgentMessage, entry.getValue().outputBuf);
            }

            // Tell new agent about other agents
            NewAgentMessage otherAgentMessage =
                new NewAgentMessage(entry.getValue().agentId,
                        entry.getValue().agentName, entry.getValue().agentIP,
                        entry.getValue().agentPort, entry.getValue().flags);
            synchronized (agentInfo.outputBuf) {
                Message.toBytes(otherAgentMessage, agentInfo.outputBuf);
            }
        }

        messageIO.addAgent(agentInfo);
        messageIO.outputReady();
    }

    public void handleMessageData(int length, MVByteBuffer messageData,
                AgentInfo agentInfo)
    {
        if (length == -1 || messageData == null) {
            if ((agentInfo.flags & MessageAgent.DOMAIN_FLAG_TRANSIENT) != 0) {
                Log.info("Lost connection to '"+agentInfo.agentName+"' (transient)");
                agents.remove(agentInfo.socket);
                agentNames.remove(agentInfo.agentName);
                messageIO.removeAgent(agentInfo);
            }
            else
                Log.info("Lost connection to '"+agentInfo.agentName+"'");

            try {
                agentInfo.socket.close();
            } catch (java.io.IOException ex) { Log.exception("close",ex); }
            agentInfo.socket = null;
            //## clear buffers
            //## keep agentInfo?  to preserve agentId?
            return;
        }
 
        Message message = (Message)MarshallingRuntime.unmarshalObject(messageData);

        MessageType msgType = message.getMsgType();
        if (Log.loggingDebug)
            Log.debug("handleMessageData from "+agentInfo.agentName+","+
                    message.getMsgId()+" type="+msgType.getMsgTypeString()+
                    " len="+length +
                    " class="+message.getClass().getName());

        try {
            if (message instanceof AllocNameMessage)
                handleAllocName((AllocNameMessage) message, agentInfo.socket);
            else if (message instanceof AwaitPluginDependentsMessage)
                handleAwaitPluginDependents(
                    (AwaitPluginDependentsMessage) message, agentInfo.socket);
            else if (message instanceof PluginAvailableMessage)
                handlePluginAvailable(
                    (PluginAvailableMessage) message, agentInfo.socket);
            else
                Log.error("Unsupported message from "+agentInfo.agentName+","+
                        message.getMsgId()+" type="+msgType.getMsgTypeString()+
                        " len="+length +
                        " class="+message.getClass().getName());
        }
        catch (java.io.IOException e) {
            Log.error("IO error on message from "+agentInfo.agentName+","+
                        message.getMsgId()+" type="+msgType.getMsgTypeString()+
                        " len="+length +
                        " class="+message.getClass().getName());
        }
    }

    private synchronized String allocName(String type, String namePattern)
    {
        Map<String,Integer> patterns = nameTypes.get(type);
        if (patterns == null) {
            patterns = new HashMap<String,Integer>();
            nameTypes.put(type,patterns);
        }

        Integer id = patterns.get(namePattern);
        if (id == null)
            id = 0;

        id++;
        patterns.put(namePattern,id);
        String agentName = namePattern.replaceFirst("#", id.toString());
        if (agentName.equals(namePattern))
            Log.warn("AllocName: missing '#' in name pattern '" +
                namePattern + "'");
        else
            Log.debug("AllocName: for type="+type+" assigned '" + agentName +
                "' from pattern '" + namePattern + "'");
        return agentName;
    }

    private static void saveProcessID(String svrName, String runDir)
    {
    	Log.info("Server Name is " + svrName + " Run Dir is " + runDir);
    	RuntimeMXBean rt = ManagementFactory.getRuntimeMXBean();
    	String pid = rt.getName();
        if (Log.loggingDebug) {
            Log.info("PROCESS ID IS " + pid);
            Log.info("server name is " + svrName);
        }
    	
        try {
            if (runDir != null) {
                File outFile = new File(runDir + "\\" + svrName + ".bat");
                PrintWriter out = new PrintWriter( new FileWriter(outFile) );
                out.println( "set pid=" + pid.substring(0, pid.indexOf("@")) );
                out.close();
            }
        } catch (IOException e){
            Log.exception("saveProcessID caught exception", e);
        }
    }

    private static void touchFile(String fileName)
    {
        try {
            FileWriter writer = new FileWriter(fileName);
            writer.close();
        }
        catch (java.io.IOException e) {
            Log.exception("touchFile "+fileName, e);
        }
    }

    static class PluginSpec
    {
        public PluginSpec(String pluginType, int expected) {
            this.pluginType = pluginType;
            this.expected = expected;
        }
        public String pluginType;
        public int expected;
        public int running;
    }

    static class PluginStartGroup
    {
        public void add(String pluginType, int expected) {
            plugins.put(pluginType, new PluginSpec(pluginType,expected));
        }

        public void prepareDependencies(Properties properties, String worldName)
        {
            for (PluginSpec plugin : plugins.values()) {
                String depString;
                depString = properties.getProperty(
                    "multiverse.plugin_dep."+worldName+"."+plugin.pluginType);
                if (depString == null) {
                    depString = properties.getProperty(
                        "multiverse.plugin_dep."+plugin.pluginType);
                }
                if (depString == null)
                    continue;
                depString = depString.trim();
                String[] deps = null;
                if (! depString.equals(""))
                    deps = depString.split(",");
                dependencies.put(plugin.pluginType, deps);
                if (Log.loggingDebug)
                    Log.debug("plugin type " + plugin.pluginType +
                        " depends on plugin types: " +
                        ((deps == null)?"*none*":depString));
            }
        }

        public synchronized void pluginAvailable(String pluginType,
            String pluginName)
        {
            if (Log.loggingDebug)
                Log.debug("Plugin available type=" + pluginType +
                    " name=" + pluginName);

            PluginSpec pluginSpec = plugins.get(pluginType);
            if (pluginSpec == null) {
                Log.error("DomainServer: unexpected plugin type=" + pluginType +
                    " name=" + pluginName);
                return;
            }

            pluginSpec.running ++;
            if (pluginSpec.running > pluginSpec.expected) {
                Log.warn("DomainServer: more plugins than expected, type=" +
                    pluginType + " name=" + pluginName +
                    " expected=" + pluginSpec.expected +
                    " available=" + pluginSpec.running);
            }

            if (pluginSpec.running >= pluginSpec.expected) {
                this.notifyAll();
            }
        }

        public synchronized boolean hasDependencies(String pluginType,
            String pluginName)
        {
            String[] deps = dependencies.get(pluginType);
            if (deps == null)
                return false;
            for (String dependentType : deps) {
                PluginSpec pluginSpec = plugins.get(dependentType);
                if (pluginSpec == null) {
                    Log.warn("No information for dependent type=" +
                        dependentType);
                    continue;
                }
                if (pluginSpec.running < pluginSpec.expected) {
                    if (Log.loggingDebug)
                        Log.debug("Incomplete dependency for type="+
                            pluginType +
                            " name=" + pluginName +
                            " dependentType=" + dependentType);
                    return true;
                }
            }
            return false;
        }

        public synchronized void awaitDependency(String pluginType)
        {
            while (hasDependencies(pluginType,pluginType)) {
                try {
                    this.wait();
                } catch (InterruptedException e) { /* ignore */ }
            }
        }

        public synchronized void awaitAllAvailable()
        {
             while (! allAvailable()) {
                try {
                    this.wait();
                } catch (InterruptedException e) { /* ignore */ }
             }
        }

        boolean allAvailable()
        {
            for (Map.Entry<String,PluginSpec> plugin : plugins.entrySet()) {
                PluginSpec pluginSpec = plugin.getValue();
                if (pluginSpec.running < pluginSpec.expected) {
System.err.println("STILL waiting for "+pluginSpec.pluginType +
" expected "+pluginSpec.expected+" running "+pluginSpec.running);
                    return false;
}
            }
            return true;
        }

        Map<String,PluginSpec> plugins = new HashMap<String,PluginSpec>();
        Map<String,String[]> dependencies = new HashMap<String,String[]>();
    }

    class PluginDependencyWatcher implements Runnable
    {
        public PluginDependencyWatcher(AwaitPluginDependentsMessage await,
            SocketChannel agentSocket)
        {
           this.await = await;
           this.agentSocket = agentSocket;
        }

        public void run()
        {
            synchronized (pluginStartGroup) {
                waitForDependencies();
            }

            if (Log.loggingDebug)
                Log.debug("Dependency satisfied for type="+
                    await.getPluginType() + " name=" + await.getPluginName());

            MVByteBuffer buffer = new MVByteBuffer(1024);
            ResponseMessage response = new ResponseMessage(await);
            Message.toBytes(response, buffer);
            buffer.flip();

            try {
                if (! ChannelUtil.writeBuffer(buffer, agentSocket)) {
                    Log.error("could not write await dependencies response");
                }
            }
            catch (java.io.IOException e) {
                Log.exception("could not write await dependencies response", e);
            }

        }

        void waitForDependencies()
        {
            while (pluginStartGroup.hasDependencies(await.getPluginType(),
                    await.getPluginName())) {
                try {
                    pluginStartGroup.wait();
                } catch (InterruptedException e) { /* ignore */ }
            }
        }

        AwaitPluginDependentsMessage await;
        SocketChannel agentSocket;
    }

    public void addPluginStartGroup(PluginStartGroup startGroup)
    {
        pluginStartGroup = startGroup;
    }

    class DomainThreadFactory implements ThreadFactory
    {
        public Thread newThread(Runnable runnable)
        {
            return new Thread(runnable, "Domain-"+threadCount++);
        }
        int threadCount = 1;
    }

    static class TimeoutRunnable implements Runnable {
        public TimeoutRunnable(int timeout) {
            this.timeout = timeout;
        }
        public void run() {
            System.err.println("\nSTARTUP FAILED -- didnt complete after " + timeout + " seconds.\nPlease stop server.");
        }
        int timeout;
    }

    private int listenPort;
    private TcpServer listener;
    private List<String> agentNames = new LinkedList<String>();
    private ExecutorService threadPool = Executors.newCachedThreadPool(
        new DomainThreadFactory());
    private PluginStartGroup pluginStartGroup;
    private String worldName;
    private long domainStartTime;

    private static String encodedDomainKey;

    private int nextAgentId = 1;
    // ## linked list might be good enough
    private Map<SocketChannel,AgentInfo> agents =
        new HashMap<SocketChannel,AgentInfo>();

    private Map<String,Map<String,Integer>> nameTypes =
        new HashMap<String,Map<String,Integer>>();

    private MessageIO messageIO;
}

