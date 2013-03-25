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

package multiverse.server.plugins;

import java.util.*;
import java.io.Serializable;
import java.util.concurrent.*;
import java.util.concurrent.locks.*;

import multiverse.msgsys.*;
import multiverse.management.Management;
import multiverse.server.objects.*;
import multiverse.server.plugins.WorldManagerClient.*;
import multiverse.server.engine.*;
import multiverse.server.events.*;
import multiverse.server.messages.PropertyMessage;
import multiverse.server.messages.LoginMessage;
import multiverse.server.messages.LogoutMessage;
import multiverse.server.util.*;
import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.math.*;
import multiverse.mars.events.ConcludeQuest;
import multiverse.mars.events.QuestResponse;
import multiverse.mars.events.RequestQuestInfo;
import multiverse.mars.plugins.*;
import multiverse.server.messages.PerceptionMessage;
import multiverse.server.messages.PerceptionFilter;
import multiverse.server.messages.PerceptionTrigger;
import multiverse.server.plugins.InstanceClient.InstanceEntryReqMessage;
import multiverse.server.plugins.InstanceClient.InstanceInfo;


/**
 * The ProxyPlugin is the sole plugin charged to communicate with
 * clients.  The onActivate method opens both TCP and RDP listeners,
 * both on the same port, allowing clients to connect over TCP are
 * RDP.
 * <p>
 * Messages from other plugins are delvered to the
 * PluginMessageCallback's handleMessage() method by the MessageAgent,
 * and are queued in a SquareQueue instance based on the player oid
 * for which they are intended.
 * <p>
 * Similarly, events stream in from clients via calls to the
 * processPacket() method, where the events are
 * parsed and and queued in another SquareQueue reserved for events.
 * Ultimately the SquareQueue calls a callback to process the event.
 * @see PluginMessageCallback#handleMessage
 * @see multiverse.server.util.SquareQueue
 * @see EventCallback#doWork
 * @see #processPacket
 */
public class ProxyPlugin extends EnginePlugin
    implements MessageCallback, ClientConnection.AcceptCallback,
        ClientConnection.MessageCallback
{
    /**
     * The ProxyPlugin constructor tells the underlying engine that it
     * is, in fact, the proxy plugin, and creates a series of message
     * counters for debugging purposes.  All the real startup action
     * happens in the onActivate method.
     * @see #onActivate
     */
    public ProxyPlugin()
    {
        super();
        setPluginType("Proxy");
        String proxyPluginName;
        try {
            proxyPluginName = Engine.getAgent().getDomainClient().allocName("PLUGIN",getPluginType()+"#");
        }
        catch (java.io.IOException e) {
            throw new MVRuntimeException("Could not allocate proxy plugin name",e);
        }

        setName(proxyPluginName);

        serverVersion = ServerVersion.ServerMajorVersion + " " +
            ServerVersion.getBuildNumber();

        // We have our own message handler, so get rid of the default
        // EnginePlugin handler
        setMessageHandler(null);
        countMsgPerception = countLogger.addCounter("mv.PERCEPTION_INFO");
        countMsgPerceptionGain = countLogger.addCounter("Perception.gain");
        countMsgPerceptionLost = countLogger.addCounter("Perception.lost");
        countMsgUpdateWNodeIn = countLogger.addCounter("mv.UPDATEWNODE.in");
        countMsgUpdateWNodeOut = countLogger.addCounter("mv.UPDATEWNODE.out");
        countMsgPropertyIn = countLogger.addCounter("mv.PROPERTY.in");
        countMsgPropertyOut = countLogger.addCounter("mv.PROPERTY.out");
        countMsgTargetedProperty = countLogger.addCounter("mv.TARGETED_PROPERTY");
        countMsgWNodeCorrectIn = countLogger.addCounter("mv.WNODECORRECT.in");
        countMsgWNodeCorrectOut = countLogger.addCounter("mv.WNODECORRECT.out");
        countMsgMobPathIn = countLogger.addCounter("mv.MOB_PATH.in");
        countMsgMobPathOut = countLogger.addCounter("mv.MOB_PATH.out");

        (new Thread(new PlayerTimeout(), "PlayerTimeout")).start();
        // The periodic GC is handy when evaluating memory leaks
	//periodicGC = new Thread(new PeriodicGC());
	//periodicGC.start();
    }

    Thread periodicGC;
    CountLogger countLogger = new CountLogger("ProxyMsg", 5000, 2);
    CountLogger.Counter countMsgPerception;
    CountLogger.Counter countMsgPerceptionGain;
    CountLogger.Counter countMsgPerceptionLost;
    CountLogger.Counter countMsgUpdateWNodeIn;
    CountLogger.Counter countMsgUpdateWNodeOut;
    CountLogger.Counter countMsgPropertyIn;
    CountLogger.Counter countMsgPropertyOut;
    CountLogger.Counter countMsgTargetedProperty;
    CountLogger.Counter countMsgWNodeCorrectIn;
    CountLogger.Counter countMsgWNodeCorrectOut;
    CountLogger.Counter countMsgMobPathIn;
    CountLogger.Counter countMsgMobPathOut;

    public List<MessageType> getExtraPlayerMessageTypes()
    {
        return extraPlayerMessageTypes;
    }

    /** Set message types to add to proxy's player subscription filter.
        Must be called before the plugin is registered/activated.
        Useful to
        make the proxy subscribe to additional ExtensionMessage message types.
        You still need to add a Hook to handle the additional message types.
    */
    public void setExtraPlayerMessageTypes(
            List<MessageType> extraPlayerMessageTypes)
    {
        this.extraPlayerMessageTypes = extraPlayerMessageTypes;
    }

    /** Additional message type to add to proxy's player subscription filter.
        Must be called before the plugin is registered/activated.
        Useful to
        make the proxy subscribe to additional ExtensionMessage message types.
        You still need to add a Hook to handle the message type.
    */
    public void addExtraPlayerMessageType(MessageType messageType)
    {
        if (extraPlayerMessageTypes == null)
            extraPlayerMessageTypes = new LinkedList<MessageType>();
        extraPlayerMessageTypes.add(messageType);
    }

    /** Additional extension message type to add to proxy's player subscription filter.
        Must be called before the plugin is registered/activated.
        Message type will be handled with by the ProxyPlugin default
        ExtensionMessage handler.
    */
    public void addExtraPlayerExtensionMessageType(MessageType messageType)
    {
        addExtraPlayerMessageType(messageType);
        getHookManager().addHook(messageType, new ExtensionHook());
    }

    /** Call hook when client sends extension message sub-type.  Multiple hooks
        can be registered for the same extension message sub-type.  The
        order of invocation is undefined.
        @param subType Extension message sub-type.
        @param hook Extension message handler.
    */
    public void addProxyExtensionHook(String subType, ProxyExtensionHook hook)
    {
        synchronized (extensionHooks) {
            List<ProxyExtensionHook> hookList = extensionHooks.get(subType);
            if (hookList == null) {
                hookList = new ArrayList<ProxyExtensionHook>();
                extensionHooks.put(subType, hookList);
            }
            hookList.add(hook);
        }
    }

    public Map<String,List<ProxyExtensionHook>> getProxyExtensionHooks(
        String subType)
    {
        return extensionHooks;
    }

    /**
     * onActivate() is the real startup method.  It:
     * <li>Initializes the PacketAggregator, which groups together messages to the client if they are created in a short interval, defaulting to 25ms.</li>
     * <li>Initilizes display of message processing time histograms.</li>
     * <li>Enumerates the list of properties in PropertyMessages that don't need to be sent to the client,
     * because the client doesn't pay attention to them.  This is not yet customizable, but should be.</li>
     * <li>Calls registerHooks to enumerate the proxy's message processing hooks.</li>
     * <li>Establishes the PerceptionFilter for the many
     * message types sent by other plugins, to which players and other objects will 
     * be added as the are created, and creates the master subscription using the
     * PerceptionFilter.  This is not yet customizable, but should be.
     * <li>Creates and opens the RDP listener using the proxy listener port.</li>
     * <li>Creates and opens the TCP listener using the proxy listern port.</li>
     * @see #registerHooks
     * @see EventCallback#doWork
     */
    public void onActivate() {
        try {
            // Initialize network aggregation based on the properties
            PacketAggregator.initializeAggregation(Engine.getProperties());
            
            String logProxyHistograms = Engine.properties.getProperty("multiverse.log_proxy_histograms");
            if (logProxyHistograms != null &&
                    logProxyHistograms.equals("true")) {
                int interval = 5000;
                String intervalString = Engine.properties.getProperty("multiverse.log_proxy_histograms_interval");
                if (intervalString != null) {
                    int newInterval = Integer.parseInt(intervalString);
                    if (newInterval > 0)
                        interval = newInterval;
                }
                proxyQueueHistogram = new TimeHistogram("TimeInQ", interval);
                proxyCallbackHistogram = new TimeHistogram("TimeInCallback", interval);
                countLogger.start();
            }

            // create the set of properties we don't send to the client
            filteredProps = new HashSet<String>();
            playerSpecificProps = new HashSet<String>();
            cachedPlayerSpecificFilterProps = new HashSet<String>();

            addFilteredProperty("inv.bag");
            addFilteredProperty(":loc");
            addFilteredProperty("masterOid");
            addFilteredProperty("marsobj.basedc");
            addFilteredProperty("mvobj.dc");
            addFilteredProperty("mvobj.followsterrainflag");
            addFilteredProperty("mvobj.perceiver");
            addFilteredProperty("mvobj.scale");
            addFilteredProperty("mvobj.mobflag");
            addFilteredProperty("mvobj.structflag");
            addFilteredProperty("mvobj.userflag");
            addFilteredProperty("mvobj.itemflag");
            addFilteredProperty("mvobj.lightflag");
            addFilteredProperty("namespace");
	    addFilteredProperty("regenEffectMap");
            addFilteredProperty(WorldManagerClient.MOB_PATH_PROPERTY);
            addFilteredProperty(WorldManagerClient.TEMPL_SOUND_DATA_LIST);
            addFilteredProperty(WorldManagerClient.TEMPL_TERRAIN_DECAL_DATA);
            addFilteredProperty(ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK);
            addFilteredProperty(ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME);
            addFilteredProperty("ignored_oids");
            
            // register hooks
            registerHooks();

            // subscribe for system chat
            // these messages handled by PluginMessageCallback
            PluginMessageCallback pluginMessageCallback =
                new PluginMessageCallback();
            MessageTypeFilter filter = new MessageTypeFilter();
	    filter.addType(WorldManagerClient.MSG_TYPE_SYS_CHAT);
            Engine.getAgent().createSubscription(filter,
                pluginMessageCallback);

            // Set up the perception subscription
            // these messages handled by PlayerMessageCallback
            perceptionFilter = new PerceptionFilter();
            LinkedList<MessageType> types = new LinkedList<MessageType>();
            types.add(WorldManagerClient.MSG_TYPE_PERCEPTION_INFO);
            types.add(WorldManagerClient.MSG_TYPE_ANIMATION);
            types.add(WorldManagerClient.MSG_TYPE_DISPLAY_CONTEXT); // S
            types.add(WorldManagerClient.MSG_TYPE_DETACH); // S
            types.add(PropertyMessage.MSG_TYPE_PROPERTY); // S
            types.add(WorldManagerClient.MSG_TYPE_COM); // S
            types.add(CombatClient.MSG_TYPE_DAMAGE);
            types.add(WorldManagerClient.MSG_TYPE_UPDATEWNODE); // S
            types.add(WorldManagerClient.MSG_TYPE_MOB_PATH); // S
            types.add(WorldManagerClient.MSG_TYPE_WNODECORRECT); // S
            types.add(WorldManagerClient.MSG_TYPE_ORIENT);
            types.add(WorldManagerClient.MSG_TYPE_SOUND); // S
            types.add(AnimationClient.MSG_TYPE_INVOKE_EFFECT);
            types.add(WorldManagerClient.MSG_TYPE_EXTENSION);
            types.add(WorldManagerClient.MSG_TYPE_P2P_EXTENSION);
            
            // Subject messages for player
            types.add(InventoryClient.MSG_TYPE_INV_UPDATE); // S
            // Target messages for player
            types.add(CombatClient.MSG_TYPE_ABILITY_UPDATE); // T
            types.add(WorldManagerClient.MSG_TYPE_FOG); // T
            types.add(WorldManagerClient.MSG_TYPE_ROAD); // T
            types.add(WorldManagerClient.MSG_TYPE_NEW_DIRLIGHT); // T
            types.add(WorldManagerClient.MSG_TYPE_SET_AMBIENT); // T
            types.add(WorldManagerClient.MSG_TYPE_TARGETED_PROPERTY);  // T
            types.add(WorldManagerClient.MSG_TYPE_FREE_OBJECT);  // T
            types.add(MSG_TYPE_VOICE_PARMS);
            types.add(MSG_TYPE_UPDATE_PLAYER_IGNORE_LIST);
            types.add(MSG_TYPE_GET_MATCHING_PLAYERS);
            if (extraPlayerMessageTypes != null)
                types.addAll(extraPlayerMessageTypes);
            perceptionFilter.setTypes(types);

            // On the proxy side, we want the filter to match all subjects
            // (the PlayerManager tracks subject perception).  This is
            // a transient filter setting, so the remote side of the filter
            // is not affected.
            perceptionFilter.setMatchAllSubjects(true);

            PerceptionTrigger perceptionTrigger = new PerceptionTrigger();
            perceptionSubId = Engine.getAgent().createSubscription(
                perceptionFilter, playerMessageCallback,
                MessageAgent.NO_FLAGS, perceptionTrigger);

            responderFilter = new PerceptionFilter();
            types.clear();
            types.add(InstanceClient.MSG_TYPE_INSTANCE_ENTRY_REQ);  // T
            types.add(MSG_TYPE_PLAYER_IGNORE_LIST_REQ);
            types.add(MSG_TYPE_GET_PLAYER_LOGIN_STATUS); // T
            responderFilter.setTypes(types);
            responderSubId = Engine.getAgent().createSubscription(
                responderFilter, playerMessageCallback,
                MessageAgent.RESPONDER);

            types.clear();
            types.add(Management.MSG_TYPE_GET_PLUGIN_STATUS);
            Engine.getAgent().createSubscription(
                new MessageTypeFilter(types), pluginMessageCallback,
                MessageAgent.RESPONDER);

            // set up an rdp server
            // do this last so we know the msging system is already spun up
            // for incoming packets
            serverSocket = new RDPServerSocket();
            String log_rdp_counters =
                Engine.getProperty("multiverse.log_rdp_counters");
            if (log_rdp_counters == null || log_rdp_counters.equals("false"))
                RDPServer.setCounterLogging(false);
            RDPServer.startRDPServer();
            serverSocket.registerAcceptCallback(this);

            initializeVoiceServerInformation();
            registerExtensionSubtype("voice_parms", MSG_TYPE_VOICE_PARMS);
            registerExtensionSubtype("player_path_req", MSG_TYPE_PLAYER_PATH_REQ);
            registerExtensionSubtype("player_path_req", MSG_TYPE_PLAYER_PATH_REQ);
            registerExtensionSubtype("mv.UPDATE_PLAYER_IGNORE_LIST", MSG_TYPE_UPDATE_PLAYER_IGNORE_LIST);
            registerExtensionSubtype("mv.GET_MATCHING_PLAYERS", MSG_TYPE_GET_MATCHING_PLAYERS);
            registerExtensionSubtype("mv.PLAYER_IGNORE_LIST_REQ", MSG_TYPE_PLAYER_IGNORE_LIST_REQ);

            clientPort = Integer.parseInt(Engine.getProperty("multiverse.proxyport").trim());
            if (Log.loggingDebug)
                Log.debug("Proxy: binding for client rdp packets on port " +
                    clientPort);
            serverSocket.bind(clientPort, serverSocketReceiveBufferSize);
            clientTCPMessageIO = ClientTCPMessageIO.setup(clientPort, this, this);
            clientTCPMessageIO.start("ClientIO");
            // Test the escaping
            // setPluginInfo("fooo'asdf\nasdf\tasdf\\asdf\u0000asdf\u001Aasdf\rasdf\basdf");
            setPluginInfo("port="+clientPort);
            Engine.registerStatusReportingPlugin(this);
            Log.debug("Proxy: activation done");
        } catch (Exception e) {
            throw new MVRuntimeException("activate failed", e);
        }
    }

    public Map<String, String> getStatusMap()
    {
        Map<String,String> status =
            new HashMap<String,String>();
        status.put("players",Integer.toString(playerManager.getPlayerCount()));
        return status;
    }
    
    class ReceivedMessage implements Runnable
    {
        ReceivedMessage(Message message, int flags)
        {
            this.message = message;
            this.flags = flags;
        }

        public void run()
        {
            callEngineOnMessage(message,flags);
        }

        Message message;
        int flags;
    }

    /**
     * Handler for non-player-specific messages
     */
    public class PluginMessageCallback implements multiverse.msgsys.MessageCallback
    {
        public void handleMessage(Message message, int flags)
        {
            executor.execute(new ReceivedMessage(message,flags));
        }

        ExecutorService executor = Executors.newSingleThreadExecutor();
    }

    /**
     * The Handler for player-specific messages coming in from other
     * plugins.
     */
    public class PlayerMessageCallback implements multiverse.msgsys.MessageCallback
    {
        /**
         * If the message is a TargetMessage, insert it in the message
         * SquareQueue by player oid for processing in order and
         * return.
         *
         * If the message is a SubjectMessage, look up the client
         * perceivers of the subject oid, and send them the message,
         * and increment the common event message counter associated
         * with this type of message and return.
         *
         * If the message is a PerceptionMessage, increment the
         * perceptionGained and perceptionLost counters, and insert it
         * in the message SquareQueue by player oid for processing in
         * order and return.
         * 
         * If the message hasn't yet been handled, insert it in the
         * message SquareQueue by player oid for processing in order
         * and return.
         */
        public void handleMessage(Message message, int flags)
        {
            List<Player> perceivers;
            if (message instanceof TargetMessage) {
                if (message.getMsgType() == WorldManagerClient.MSG_TYPE_TARGETED_PROPERTY)
                    countMsgTargetedProperty.add();

                long playerOid = ((TargetMessage)message).getTarget();
                Player player = playerManager.getPlayer(playerOid);
                if (player == null) {
                    Log.debug("TargetMessage: player "+playerOid+
                        " not found");
                }
                else {
                    message.setEnqueueTime();
                    messageQQ.insert(player, message);
                }
                return;
            }
            else if (message instanceof SubjectMessage) {
                perceivers = playerManager.getPerceivers(
                    ((SubjectMessage)message).getSubject());
                if (perceivers == null)
                    return;
                if (message instanceof WorldManagerClient.UpdateWorldNodeMessage) {
                    WorldManagerClient.UpdateWorldNodeMessage wMsg =
                        (WorldManagerClient.UpdateWorldNodeMessage) message;
                    DirLocOrientEvent dloEvent =
                        new DirLocOrientEvent(wMsg.getSubject(), wMsg.getWorldNode());
                    wMsg.setEventBuf(dloEvent.toBytes());
                }

                // Increment stats counters
                if (message.getMsgType() == WorldManagerClient.MSG_TYPE_UPDATEWNODE) {
                    countMsgUpdateWNodeIn.add();
                    countMsgUpdateWNodeOut.add(perceivers.size());
                }
                else if (message.getMsgType() == PropertyMessage.MSG_TYPE_PROPERTY) {
                    countMsgPropertyIn.add();
                    countMsgPropertyOut.add(perceivers.size());
                }
                else if (message.getMsgType() == WorldManagerClient.MSG_TYPE_WNODECORRECT) {
                    countMsgWNodeCorrectIn.add();
                    countMsgWNodeCorrectOut.add(perceivers.size());
                }
                else if (message.getMsgType() == WorldManagerClient.MSG_TYPE_MOB_PATH) {
                    countMsgMobPathIn.add();
                    countMsgMobPathOut.add(perceivers.size());
                }
            }
            else if (message instanceof PerceptionMessage) {
                PerceptionMessage pMsg = ((PerceptionMessage)message);
                countMsgPerception.add();
                countMsgPerceptionGain.add(pMsg.getGainObjectCount());
                countMsgPerceptionLost.add(pMsg.getLostObjectCount());

                long playerOid = pMsg.getTarget();
                Player player = playerManager.getPlayer(playerOid);
                if (player == null) {
                    Log.debug("PerceptionMessage: player "+playerOid+
                        " not found");
                }
                else {
                    message.setEnqueueTime();
                    messageQQ.insert(player, message);
                }
                return;
            }
            else {
                Log.error("PlayerMessageCallback unknown type=" +
                        message.getMsgType());
                return;
            }

            if (perceivers == null) {
                Log.warn("No perceivers for "+message);
                return;
            }

            message.setEnqueueTime();
            messageQQ.insert(perceivers, message);

        }
    }

    /**
     * Associates a CommandParser with the appropriate "slash" command.
     * @param command The command to associate with the CommandParse,
     * eg: "/attack"
     * @param parser Command handler.
     */
    public void registerCommand(String command, CommandParser parser) {
        commandMapLock.lock();
        try {
            commandMap.put(command, parser);
        } finally {
            commandMapLock.unlock();
        }
    }

    /** Interface for handling client command events.  Register instances
        with {@link #registerCommand}.
     */
    public interface CommandParser {
        /** Handle client command event.  Command events are generally
            associated with "/" (slash) commands.
         */
        public void parse(CommandEvent event);
    }

    protected Lock commandMapLock = LockFactory.makeLock("CommandMapLock");

    Map<String, CommandParser> commandMap = new HashMap<String, CommandParser>();

    /**
     * Registers hooks for incoming messages from other plugins.  This
     * is not yet customizable, but should be.
     */
    void registerHooks() {
        log.debug("registering hooks");

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_DISPLAY_CONTEXT,
                new DisplayContextHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_DETACH, 
                new DetachHook());
        
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_ANIMATION,
                new AnimationHook());
 
        getHookManager().addHook(AnimationClient.MSG_TYPE_INVOKE_EFFECT,
                new InvokeEffectHook());

	getHookManager().addHook(PropertyMessage.MSG_TYPE_PROPERTY,
                new PropertyHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_EXTENSION,
                new ExtensionHook());
        
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_TARGETED_PROPERTY,
                new TargetedPropertyHook());
        
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_PERCEPTION_INFO,
                new PerceptionHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_COM,
                new ComHook());

        getHookManager().addHook(CombatClient.MSG_TYPE_DAMAGE,
                new DamageHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_SYS_CHAT,
                new SysChatHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATEWNODE,
                new UpdateWNodeHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_MOB_PATH,
                new UpdateMobPathHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_WNODECORRECT,
                new WNodeCorrectHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_ORIENT,
                new OrientHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_SOUND,
                new SoundHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_FOG, new FogHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_ROAD,
                new RoadHook());

        getHookManager().addHook(InventoryClient.MSG_TYPE_INV_UPDATE,
                new InvUpdateHook());
        
        getHookManager().addHook(CombatClient.MSG_TYPE_ABILITY_UPDATE,
                new AbilityUpdateHook());
        
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_NEW_DIRLIGHT,
                new NewDirLightHook());
        
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_FREE_OBJECT,
                new FreeObjectHook());
        
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_SET_AMBIENT,
                new SetAmbientHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_P2P_EXTENSION,
                new P2PExtensionHook());
        getHookManager().addHook(MSG_TYPE_VOICE_PARMS,
                new VoiceParmsHook());
        getHookManager().addHook(MSG_TYPE_PLAYER_PATH_REQ,
                new PlayerPathReqHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_INSTANCE_ENTRY_REQ,
                new InstanceEntryReqHook());
        getHookManager().addHook(Management.MSG_TYPE_GET_PLUGIN_STATUS,
                new GetPluginStatusHook());

        getHookManager().addHook(MSG_TYPE_UPDATE_PLAYER_IGNORE_LIST,
                new UpdatePlayerIgnoreListHook());
        getHookManager().addHook(MSG_TYPE_GET_MATCHING_PLAYERS,
                new GetMatchingPlayersHook());
        getHookManager().addHook(MSG_TYPE_PLAYER_IGNORE_LIST_REQ,
                new PlayerIgnoreListReqHook());
        getHookManager().addHook(MSG_TYPE_GET_PLAYER_LOGIN_STATUS,
                new GetPlayerLoginStatusHook());
    }

    class MatchedMessage {
	MatchedMessage(Long sub, Message message) {
	    this.sub = sub;
	    this.message = message;
	    enqueueTime = System.currentTimeMillis();
	}

	public String toString() {
            return "MatchedMessage[subId=" + sub + ", enqueueTime=" + enqueueTime + ",msg=" + message;
        }
        
        final Long sub;
	final Message message;
	final long enqueueTime;
    }
 
    private static final Player loginSerializer = new Player(-1,null);

    void callEngineOnMessage(Message message, int flags) {
	super.handleMessage(message, flags);
    }

    protected void initializeVoiceServerInformation() {
        // Initialize the voice server contact info.  For now,
        // there is only one voice process.
        voiceServerHost = Engine.properties.getProperty("multiverse.voiceserver");
        String s = Engine.properties.getProperty("multiverse.voiceport");
        if (s != null)
            voiceServerPort = Integer.parseInt(s);
        if (Log.loggingDebug)
            log.debug("initializeVoiceServerInformation: voiceServerHost " + voiceServerHost + ", voiceServerPort " + voiceServerPort);
    }

    /**
     * This is the SquareQueue callback for the proxy; when the
     * SquareQueue has a message that should be processed, because all
     * messages for the same player have already been processed, it
     * calls the doWork method.
     */
    class MessageCallback implements SQCallback {
	public MessageCallback(ProxyPlugin proxyPlugin) {
	    this.proxyPlugin = proxyPlugin;
	}
	/**
         * doWork "executes" the message, which originated in some
         * other plugin, on behalf the player by running any message
         * hooks for the message's MessageType.  It also maintains the
         * time histograms of message time-in-queue, and the time
         * running the hooks on the message.
         * @param value Actually a subtype of Message; represents the
         * recently-dequeued message.
         * @param key A Player instance.
         */
        public void doWork(Object value, Object key) {
	    Message message = (Message)value;
	    Player player = (Player)key;
            if (message == null) {
                Log.dumpStack("DOMESSAGE: Message for oid="+player.getOid()+" is not a Message: " + value);
                return;
            }

            if (message instanceof ConnectionResetMessage) {
                if (player == loginSerializer)
                    processConnectionResetInternal((ConnectionResetMessage)message);
                else
                    messageQQ.insert(loginSerializer,message);
                return;
            }

            // Ignore messages for unknown players or players in
            // the midst of logging out.
            int status = player.getStatus();
            if (status == Player.STATUS_LOGOUT ||
                    status == Player.STATUS_UNKNOWN) {
                Log.debug("Ignoring message; player status="+
                    status + " oid="+player.getOid() +
                    ": id="+message.getMsgId()+" type="+
                    message.getMsgType());
                return;
            }

	    try {
                long inQueue = 0;
                if (Log.loggingDebug) {
                    inQueue = System.nanoTime() - message.getEnqueueTime();
                    Log.debug("DOINGSVRMESSAGE: Message for oid="+player.getOid()+
                        ",msgId="+ message.getMsgId()+
                        ",in-queue=" + (inQueue / 1000L) + " usec: " +
                        message.getMsgType());
                }
		if (Log.loggingInfo && proxyQueueHistogram != null)
                    proxyQueueHistogram.addTime(inQueue);

                List<Hook> hooks = getHookManager().getHooks(message.getMsgType());
                long callbackStart = System.nanoTime();
                // Second param to processMessage is flags
		for (Hook hook : hooks) {
                    ((ProxyHook)hook).processMessage(message,0,player);
                }
                long callbackTime = 0;
                if (Log.loggingDebug || Log.loggingInfo)
                    callbackTime = System.nanoTime() - callbackStart;
                if (Log.loggingDebug) {
                    Log.debug("DONESVRMESSAGE: Message for oid="+player.getOid()+
                        ",msgId="+ message.getMsgId()+
                        ",in-queue=" + (inQueue / 1000L) + " usec: " +
                        ",execute=" + (callbackTime / 1000L) + " usec: " +
                        message.getMsgType());
                }
		if (Log.loggingInfo && proxyCallbackHistogram != null)
                    proxyCallbackHistogram.addTime(callbackTime);
	    }
	    catch (Exception ex) {
                Log.exception("SQ MessageCallback", ex);
	    }
	}

	protected ProxyPlugin proxyPlugin;
    }

    /**
     * The SquareQueue instance that queues messages from other
     * plugins.
     */
    SquareQueue<Player,Message> messageQQ =
		new SquareQueue<Player,Message>("Message");
    SQThreadPool messageThreadPool = new SQThreadPool(messageQQ,
		new MessageCallback(this));

    /**
     * Registers the proxy plugin instance as the message handler for
     * the client connection.
     * @param con The new client connection.
     */
    public void acceptConnection(ClientConnection con) {
        Log.info("ProxyPlugin: CONNECTION remote=" + con);
        con.registerMessageCallback(this);
    }

    /**
     * The proxy method called to process a message from a client.
     * @param con The ClientConnection object for the player's client.
     * @param buf The byte buffer containing message from the client.
     */
    public void processPacket(ClientConnection con, MVByteBuffer buf) {
        try {
            if (Log.loggingNet) {
                if (ClientConnection.getLogMessageContents())
                    Log.net("ProxyPlugin.processPacket: con " + con + ", length " + buf.limit() + ", packet " + 
                        DebugUtils.byteArrayToHexString(buf));
                else
                    Log.net("ProxyPlugin.processPacket: con " + con + ", buf " + buf);
            }
            // turn packet into an event
            Event event = Engine.getEventServer().parseBytes(buf, con);
            if (event == null) {
                Log.error("Engine: could not parse packet data, remote=" + con);
                return;
            }

            Player player = (Player)con.getAssociation();
            if (player == null) {
                player = loginSerializer;
                if (event instanceof AuthorizedLoginEvent) {
                    Log.info("ProxyPlugin: LOGIN_RECV remote=" + con +
                        " playerOid=" + ((AuthorizedLoginEvent)event).getOid());
                }
            }
	    playerManager.processEvent(player, event, eventQQ);

        } catch (MVRuntimeException e) {
            Log.exception("ProxyPlugin.processPacket caught exception", e);
        }
    }

    /**
     * The callback called by the SquareQueue holding messages from clients.
     */
    public class EventCallback implements SQCallback {
        /*
         * doWork "executes" a message from a client.  After some
         * special-case code used only during login or connection
         * reset, it tests the Event to see if it is one of the dozen
         * or so events it knows how to process, and calls the
         * processing function.  The events and processing functions
         * are not yet customizable, but should be.
         *
         * @param value Actually a subtype of Event; represents the
         * recently-dequeued message from the client.
         * @param key A Player instance.
         */
        public void doWork(Object value, Object key) {
	    Event event = (Event)value;
            if (event == null) {
                Log.dumpStack("EventCallback.doWork: event object is null, for key " + key);
                return;
            }
	    ClientConnection con = event.getConnection();
	    Player player = (Player)key;

            try {
                // process event
                long startTime = System.currentTimeMillis();
		long inQueue = startTime - event.getEnqueueTime();
                
		if (player == loginSerializer &&
				    event instanceof AuthorizedLoginEvent) {
		    AuthorizedLoginEvent loginEvent =
                        (AuthorizedLoginEvent) event;
		    Long playerOid = loginEvent.getOid();

                    Log.info("ProxyPlugin: LOGIN_BEGIN remote=" + con +
                        " playerOid=" + playerOid +
                        " in-queue=" + inQueue + " ms");

                    boolean loginOK = processLogin(con, loginEvent);

                    Player newPlayer = playerManager.getPlayer(playerOid);
                    String playerName = null;
                    if (newPlayer != null)
                        playerName = newPlayer.getName();
                    Log.info("ProxyPlugin: LOGIN_END remote=" + con +
                        ((loginOK) ? " SUCCESS " : " FAILURE ") +
                        " playerOid=" + playerOid +
                        " name=" + playerName +
                        " in-queue=" + inQueue + " ms" +
                        " processing=" + 
                        (System.currentTimeMillis() - startTime) + " ms" +
                        " nPlayers=" + playerManager.getPlayerCount());
		    return;
		}

		if (player == loginSerializer) {
                    Log.error("ClientEvent: Illegal event for loginSerializer: " +
			event.getClass().getName() + ", con=" + con);
                    return;
		}

                if (Log.loggingDebug)
                    Log.debug("ClientEvent: player=" + player +
			", in-queue=" + inQueue +
			" ms: "+event.getName());
		if (Log.loggingInfo && inQueue > 2000) {
		    Log.info("LONG IN-QUEUE: "+inQueue+" ms: player=" + player +
			" " + event.getName());
		}

                Lock objLock = getObjectLockManager().getLock(player.getOid());
                objLock.lock();
                try {
                    if (Log.loggingDebug)
                        Log.debug("ClientEvent: event detail: " + event);
                    if (event instanceof ComEvent) {
                        processCom(con, (ComEvent) event);
                    } else if (event instanceof DirLocOrientEvent) {
                        processDirLocOrient(con, (DirLocOrientEvent) event);
                    } else if (event instanceof CommandEvent) {
                        processCommand(con, (CommandEvent) event);
                    } else if (event instanceof AutoAttackEvent) {
                        processAutoAttack(con, (AutoAttackEvent) event);
                    } else if (event instanceof ExtensionMessageEvent) {
                        processExtensionMessageEvent(con, (ExtensionMessageEvent)event);
                    } else if (event instanceof RequestQuestInfo) {
                        processRequestQuestInfo(con, (RequestQuestInfo) event);
                    } else if (event instanceof QuestResponse) {
                        processQuestResponse(con, (QuestResponse) event);
                    } else if (event instanceof ConcludeQuest) {
                        processReqConcludeQuest(con, (ConcludeQuest) event);
		    } else if (event instanceof ActivateItemEvent) {
			processActivateItem(con, (ActivateItemEvent) event);
                    } else {
                        throw new RuntimeException("Unknown event: " + event);
                    }
		    long elapsed = (System.currentTimeMillis() - startTime);
                    if (Log.loggingDebug) {
                        Log.debug("ClientEvent: processed event " + event +
				", player=" + player +
				", processing=" + elapsed + " ms");
                        clientMsgMeter.add(elapsed);
                    }
		    if (elapsed > 2000) {
			Log.info("LONG PROCESS: "+elapsed+" ms: player=" +
                            player + " " + event.getName());
		    }
                } finally {
                    objLock.unlock();
                }
            } catch (Exception e) {
                throw new RuntimeException("ProxyPlugin.EventCallback", e);
            }
	}
    }

    /**
     * The SquareQueue instance for incoming client messages.
     */
    SquareQueue<Player,Event> eventQQ =
		new SquareQueue<Player,Event>("Event");
    SQThreadPool eventThreadPool= new SQThreadPool(eventQQ,
		new EventCallback());

    /**
     * Used by the /dirlight command to get the set of player oids.
     * @return The set of oids for logged-in players.
     */
    public Set<Long> getPlayerOids() {
	List<Player> players = new ArrayList<Player>(playerManager.getPlayerCount());
        playerManager.getPlayers(players);
        Set<Long> result = new HashSet<Long>(players.size());
        for (Player player : players) {
            result.add(player.getOid());
        }
        return result;
    }

    /**
     * Used by the /who command to get the set of player names.
     * @return The list of names for logged-in players.
     */
    public List<String> getPlayerNames() {
	List<Player> players = new ArrayList<Player>(playerManager.getPlayerCount());
        Log.debug("ProxyPlugin.getPlayerNames: count is " + playerManager.getPlayerCount());
        playerManager.getPlayers(players);
        List<String> result = new ArrayList<String>(players.size());
        for (Player player : players) {
            result.add(player.getName());
        }
        return result;
    }

    /** Get the players using this proxy.  The Player object is local to
        the proxy and only tracks the player's login status.
    */
    public List<Player> getPlayers()
    {
	List<Player> players = new ArrayList<Player>(playerManager.getPlayerCount());
        playerManager.getPlayers(players);
        return players;
    }

    /** Get player.  The Player object is local to the proxy and only
        tracks the player's login status.
        @return Player on success, null on failure.
    */
    public Player getPlayer(long oid)
    {
        return playerManager.getPlayer(oid);
    }

    /** Add message directly to player's message queue.
    */
    public void addPlayerMessage(Message message, Player player)
    {
        message.setEnqueueTime();
        messageQQ.insert(player, message);
    }

    /**
     * An entrypoint that allows Python code to add a filtered
     * property, which is a property that is _not_ sent to any client.
     * @param filteredProperty The string property name.
     */
    public void addFilteredProperty(String filteredProperty) {
        filteredProps.add(filteredProperty);
        cachedPlayerSpecificFilterProps.add(filteredProperty);
    }
    
    /**
     * An entrypoint that allows Python code to add a player-specific
     * property, which is a property that is _not_ sent to any client
     * except the client running the player with the property.
     * @param filteredProperty The string property name.
     */
    public void addPlayerSpecificProperty(String filteredProperty) {
        playerSpecificProps.add(filteredProperty);
        cachedPlayerSpecificFilterProps.add(filteredProperty);
    }
    
    protected void recreatePlayerSpecificCache() {
        cachedPlayerSpecificFilterProps = new HashSet<String>();
        cachedPlayerSpecificFilterProps.addAll(filteredProps);
        cachedPlayerSpecificFilterProps.addAll(playerSpecificProps);
    }

    MVMeter clientMsgMeter = new MVMeter("ClientEventProcessorMeter");

    /**
     * Process login message from the client.  The heavy lifting is
     * done in the processLoginHelper method, and then we ask the
     * playerManager to make a Player object for the connection.
     * @param con The connection to the client.
     * @param loginEvent The client message that asks to log the
     * client in.
     */
    protected boolean processLogin(ClientConnection con, AuthorizedLoginEvent loginEvent) {

        // create a stub object to track login state
        Long playerOid = loginEvent.getOid();

        String version = loginEvent.getVersion();
	int nPlayers = playerManager.getPlayerCount();

        String[] clientVersionCapabilities = null;
        if (version != null && version.length() > 0)
            clientVersionCapabilities = version.split(",");
        LinkedList<String> clientCapabilities = new LinkedList<String>();
        String clientVersion = "";
        if (clientVersionCapabilities != null && clientVersionCapabilities.length > 0) {
            clientVersion = clientVersionCapabilities[0];
            for (int i = 1; i < clientVersionCapabilities.length; i++)
                clientCapabilities.add(clientVersionCapabilities[i].trim());
        }

        int versionCompare = ServerVersion.compareVersionStrings(
                clientVersion, ServerVersion.ServerMajorVersion);
        if (versionCompare != ServerVersion.VERSION_GREATER &&
                versionCompare != ServerVersion.VERSION_EQUAL) {
            Log.warn("processLogin: unsupported version "+clientVersion+
                " from player: " + playerOid);
	    AuthorizedLoginResponseEvent loginResponse =
		    new AuthorizedLoginResponseEvent(playerOid, false, 
		    "Login Failed: Unsupported client version",
		    serverVersion);
	    con.send(loginResponse.toBytes());
	    return false;
        }

        // check if we have too many players online
        if (!isAdmin(playerOid)) {
            Log.debug("processLogin: player is not admin");
            if (nPlayers >= ProxyPlugin.MaxConcurrentUsers) {
                Log.warn("processLogin: too many users, failed for player: " + playerOid);
                Event loginResponse = new AuthorizedLoginResponseEvent(playerOid, false, 
                        capacityError, serverVersion);
                con.send(loginResponse.toBytes());
                return false;
            }
        } else {
            Log.debug("processLogin: player is admin, bypassing max check");
        }

        // check token
        // character_oid must match character oid from login message
        // proxy_server must match message agent name for this process
        SecureToken token = SecureTokenManager.getInstance().importToken(loginEvent.getWorldToken());
        if (!token.getValid() || !token.getProperty("character_oid").equals(playerOid) ||
            !token.getProperty("proxy_server").equals(Engine.getAgent().getName())) {
            Log.debug("processLogin: invalid proxy token");
            Event loginResponse = new AuthorizedLoginResponseEvent(playerOid, false, 
                                                                   tokenError, serverVersion);
            con.send(loginResponse.toBytes());
            return false;
        }

        try {
            TargetMessage getPlayerLoginStatus =
                new TargetMessage(MSG_TYPE_GET_PLAYER_LOGIN_STATUS, playerOid);
            PlayerLoginStatus loginStatus;
            loginStatus =
                (PlayerLoginStatus)Engine.getAgent().sendRPCReturnObject(
                    getPlayerLoginStatus);
	    Log.info("processLogin: LOGIN_DUPLICATE remote=" + con +
                " playerOid=" + playerOid +
		" name=" + loginStatus.name +
		" existingCon=" + loginStatus.clientCon +
		" existingProxy=" + loginStatus.proxyPluginName);
	    AuthorizedLoginResponseEvent loginResponse =
		    new AuthorizedLoginResponseEvent(playerOid, false, 
		    "Login Failed: Already connected",
		    serverVersion);
	    con.send(loginResponse.toBytes());
	    return false;
        }
        catch (NoRecipientsException e) {
            // OK, player is not logged in
        }

/*
        ObjectManagerClient.ObjectStatus playerObjectStatus =
            ObjectManagerClient.getObjectStatus(playerOid);
        if (playerObjectStatus.namespaces != null) {
	    Log.info("processLogin: LOGIN_DUPLICATE remote=" + con +
                " playerOid=" + playerOid +
		" name=" + playerObjectStatus.name);
	    AuthorizedLoginResponseEvent loginResponse =
		    new AuthorizedLoginResponseEvent(playerOid, false, 
		    "Login Failed: Already connected",
		    serverVersion);
	    con.send(loginResponse.toBytes());
	    return false;
        }
*/

        Player player = new Player(playerOid, con);
        player.setStatus(Player.STATUS_LOGIN_PENDING);
        if (! playerManager.addPlayer(player)) {
        //## race, addPlayer needs to return existing player
            player = playerManager.getPlayer(playerOid);
	    Log.info("processLogin: LOGIN_DUPLICATE remote=" + con +
                " playerOid=" + playerOid +
		" existing=" + player.getConnection());
	    AuthorizedLoginResponseEvent loginResponse =
		    new AuthorizedLoginResponseEvent(playerOid, false, 
		    "Login Failed: Already connected",
		    serverVersion);
	    con.send(loginResponse.toBytes());
	    return false;
        }
        con.setAssociation(player);

        player.setVersion(clientVersion);
        player.setCapabilities(clientCapabilities);

        String errorMessage = proxyLoginCallback.preLoad(player, con);
        if (errorMessage != null) {
            playerManager.removePlayer(playerOid);
            Event loginResponse = new AuthorizedLoginResponseEvent(playerOid,
                false, errorMessage, serverVersion);
            con.send(loginResponse.toBytes());
            return false;
        }

        //
        // load the object
        //
        if (Log.loggingDebug)
            Log.debug("processLogin: loading object: " + playerOid +
                ", con=" + con);
        if (! loadPlayerObject(player)) {
            Log.error("processLogin: could not load object " + playerOid);
	    playerManager.removePlayer(playerOid);
            Event loginResponse = new AuthorizedLoginResponseEvent(playerOid,
                false, "Login Failed", serverVersion);
            con.send(loginResponse.toBytes());
            return false;
        }
        if (Log.loggingDebug)
            Log.debug("processLogin: loaded player object: " + playerOid);

        errorMessage = proxyLoginCallback.postLoad(player, con);
        if (errorMessage != null) {
            playerManager.removePlayer(playerOid);
            Event loginResponse = new AuthorizedLoginResponseEvent(playerOid,
                false, errorMessage, serverVersion);
            con.send(loginResponse.toBytes());
            return false;
        }

        //
        // make a login response
        //
        AuthorizedLoginResponseEvent loginResponse =
		new AuthorizedLoginResponseEvent(playerOid, true, 
                "Login Succeeded",
		serverVersion + ", " + serverCapabilitiesSentToClient);
        con.send(loginResponse.toBytes());
	if (Log.loggingDebug)
	    Log.debug("Login response sent for playerOid="+playerOid+
		" the authorized login response message is : " +
		loginResponse.getMessage());

        // Perceive self so we get any targeted messages prior to the
        // WM's PerceptionMessage
        playerManager.addPerception(playerOid,playerOid);

        boolean loginOK = processLoginHelper(con, player);
	if (! loginOK) {
            // We already told the client the login succeeded, so it will
            // ignore subsequent LoginResponse
            //loginResponse = new AuthorizedLoginResponseEvent(playerOid,
            //    false, "Internal error", serverVersion);
            //con.send(loginResponse.toBytes());
            con.setAssociation(null);
            if (perceptionFilter.removeTarget(playerOid)) {
                FilterUpdate filterUpdate = new FilterUpdate(1);
                filterUpdate.removeFieldValue(PerceptionFilter.FIELD_TARGETS,
                        new Long(playerOid));
                Engine.getAgent().applyFilterUpdate(perceptionSubId,filterUpdate);
                responderFilter.removeTarget(playerOid);
                Engine.getAgent().applyFilterUpdate(responderSubId,filterUpdate);
            }
	    playerManager.removePlayer(playerOid);
            playerManager.removePerception(playerOid,playerOid);
            con.close();
	}
	else {
            proxyLoginCallback.postSpawn(player, con);

	    playerManager.loginComplete(player, eventQQ);

            //
            // Get the ignored oids and set in player object, then get the
            // corresponding names and send to the client as a property
            // message.
            //
            processPlayerIgnoreList(player);
	}
	return loginOK;
    }

    protected boolean loadPlayerObject(Player player)
    {
        InstanceRestorePoint restorePoint = null;
        LinkedList restoreStack = null;
        boolean first = true;
        long playerOid = player.getOid();

        // Load the player's object manager sub-object
        List<Namespace> namespaces = new ArrayList<Namespace>();
        Long oidResult = ObjectManagerClient.loadSubObject(playerOid,namespaces);
        if (oidResult == null)
            return false;

        Point location = new Point();
        Long instanceOid = Engine.getDatabase().getLocation(playerOid,
            WorldManagerClient.NAMESPACE, location);

        do {
            Long result = null;
            if (instanceEntryAllowed(playerOid, instanceOid, location))
                result = ObjectManagerClient.loadObject(playerOid);

            // Save the "popped" stack if needed
            if (restoreStack != null && ! restorePoint.getFallbackFlag())
                EnginePlugin.setObjectProperty(
                    playerOid, Namespace.OBJECT_MANAGER,
                    ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK,
                    restoreStack);

            // Load success, return
            if (result != null) {
                if (restorePoint != null) {
                    EnginePlugin.setObjectProperty(
                        playerOid, Namespace.OBJECT_MANAGER,
                        ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME,
                        restorePoint.getInstanceName());
                }
                break;
            }

            if (first) {
                // Get the player's current instance when they logged out
                String instanceName = (String) EnginePlugin.getObjectProperty(
                    playerOid, Namespace.OBJECT_MANAGER,
                    ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME);
                Log.debug("Failed initial load, retrying with current instanceName="+instanceName);
                if (instanceName != null) {
                    instanceOid = instanceEntryCallback.selectInstance(player,
                        instanceName);
                    if (instanceOid != null) {
                        Log.debug("Failed initial load, retrying with instanceOid="+instanceOid);
                        BasicWorldNode wnode = new BasicWorldNode();
                        wnode.setInstanceOid(instanceOid);
                        ObjectManagerClient.fixWorldNode(playerOid,wnode);
                        first = false;
                        continue;
                    }
                }
            }

            // Load failed, previous restore point was the fallback,
            // so give up.
            if (restorePoint != null && restorePoint.getFallbackFlag())
                return false;

            // Get the instance restore stack
            restoreStack = (LinkedList) EnginePlugin.getObjectProperty(
                playerOid, Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK);

            if (restoreStack == null || restoreStack.size() == 0)
                return false;

            // Get the top of the stack, and set player location,
            // loop around to try loading again
            int size = restoreStack.size();
            restorePoint = (InstanceRestorePoint) restoreStack.get(size-1);

            instanceOid = restorePoint.getInstanceOid();
            if (restorePoint.getInstanceName() != null)
                instanceOid = instanceEntryCallback.selectInstance(player,
                    restorePoint.getInstanceName());

            if (instanceOid != null) {
                BasicWorldNode wnode = new BasicWorldNode();
                wnode.setInstanceOid(instanceOid);
                if (! first) {
                    wnode.setLoc(restorePoint.getLoc());
                    wnode.setOrientation(restorePoint.getOrientation());
                    wnode.setDir(new MVVector(0,0,0));
                }
                boolean rc = ObjectManagerClient.fixWorldNode(playerOid,wnode);
                if (! rc)
                    return false;
                location = restorePoint.getLoc();
            }
            first = false;

            if (! restorePoint.getFallbackFlag())
                restoreStack.remove(size-1);

        } while (true);

        return true;
    }

    /**
     * This is a helper method that implements the part of login.
     * Mostly, it sends the client additional messages here, and set up
     * the subscriptions that we will need.
     * @param con The ClientConnection to the client
     * @param player The Player instance of the player that just logged in
     */
    protected boolean processLoginHelper(ClientConnection con, Player player)
    {
        Long playerOid = player.getOid();

        // object info
        WorldManagerClient.ObjectInfo objInfo =
            WorldManagerClient.getObjectInfo(playerOid);
        if (objInfo == null) {
            Log.error("processLogin: Could not get player ObjectInfo oid="+playerOid);
            return false;
        }

        if (World.FollowsTerrainOverride != null) {
            Log.debug("using follows terrain override");
            objInfo.followsTerrain = World.FollowsTerrainOverride;
        }
        if (Log.loggingDebug)
            Log.debug("processLogin: got object info: " + objInfo);
        // Save the name for logging
        player.setName(objInfo.name);

        InstanceInfo instanceInfo = InstanceClient.getInstanceInfo(
            objInfo.instanceOid,
            InstanceClient.FLAG_ALL_INFO);

        if (instanceInfo == null) {
            Log.error("processLogin: unknown instanceOid="+objInfo.instanceOid);
            return false;
        }

        // Broadcast the new login.  An RPC is used, so we'll block until
        // all subscribers respond.
        // ## potential perf issue as there's only one login processed
        // ## at a time
        LoginMessage loginMessage = new LoginMessage(playerOid, objInfo.name);
        loginMessage.setInstanceOid(objInfo.instanceOid);
        AsyncRPCCallback asyncRPCCallback = new AsyncRPCCallback(player,
                "processLogin: got LoginMessage response");
        int expectedResponses = Engine.getAgent().sendBroadcastRPC(
            loginMessage, asyncRPCCallback);
        asyncRPCCallback.waitForResponses(expectedResponses);

        // Send the terrain config
        if (Log.loggingDebug)
            Log.debug("processLogin: sending terrain config: " +
                instanceInfo.terrainConfig);
        if (instanceInfo.terrainConfig != null)
            con.send(instanceInfo.terrainConfig.toBuffer());

        con.send(objInfo.toBuffer(playerOid));

        // Get display context and send
        DisplayContext dc = WorldManagerClient.getDisplayContext(playerOid);
        ModelInfoEvent modelInfoEvent = new ModelInfoEvent(playerOid);
        modelInfoEvent.setDisplayContext(dc);
        if (Log.loggingDebug)
            Log.debug("processLogin: got dc: " + dc);
        con.send(modelInfoEvent.toBytes());

	// Send attachment message(s)
	Map<String, DisplayContext> childMap = dc.getChildDCMap();
	if ((childMap != null) && (!childMap.isEmpty())) {
	    for (String slot : childMap.keySet()) {
		DisplayContext attachDC = childMap.get(slot);
		if (attachDC == null) {
		    throw new MVRuntimeException("attach DC is null for obj: " + playerOid);
		}
		Long attacheeOID = attachDC.getObjRef();
		if (attacheeOID == null) {
		    throw new MVRuntimeException("attachee oid is null for obj: " + playerOid);
		}
		// send over an attach event for each attachment
		if (Log.loggingDebug)
		    Log.debug("processLogin: sending attach message to "
			      + playerOid + " attaching to obj " + playerOid
			      + ", object being attached=" + attacheeOID +
			      " to slot " + slot + ", attachmentDC=" + attachDC);
		
		AttachEvent event = new AttachEvent(playerOid, attacheeOID,
						    slot, attachDC);
		con.send(event.toBytes());
	    }
	    Log.debug("processLogin: done with processing attachments");
	}

        // Add player as target in our perception filter, and update
        // the remote filter.
        if (perceptionFilter.addTarget(playerOid)) {
            FilterUpdate filterUpdate = new FilterUpdate(1);
            filterUpdate.addFieldValue(PerceptionFilter.FIELD_TARGETS,
                new Long(playerOid));
            Engine.getAgent().applyFilterUpdate(perceptionSubId,filterUpdate);
            responderFilter.addTarget(playerOid);
            Engine.getAgent().applyFilterUpdate(responderSubId,filterUpdate);
        }

        // last thing the client needs before it renders is the
        // model info for the player object
        WorldManagerClient.updateObject(playerOid, playerOid);

        // Send UI theme
        List<String> uiThemes = World.getThemes();
        if (Log.loggingDebug)
            Log.debug("processLogin: setting themes: " + uiThemes);
        Event uiThemeEvent = new UITheme(uiThemes);
        con.send(uiThemeEvent.toBytes());

        // Send skybox
        if (Log.loggingDebug)
            Log.debug("processLogin: using skybox: " + instanceInfo.skybox);
        if (instanceInfo.skybox != null) {
            Event skyboxEvent = new SkyboxEvent(instanceInfo.skybox);
            con.send(skyboxEvent.toBytes());
        }
        
        // Send ocean data
        if (instanceInfo.ocean != null) {
            OceanData oceanData = instanceInfo.ocean;
            TargetedExtensionMessage oceanMsg = new ClientParameter.ClientParameterMessage(playerOid);
            oceanMsg.setProperty("Ocean.DisplayOcean", oceanData.displayOcean
                    .toString());
            if (oceanData.useParams != null) {
                oceanMsg.setProperty("Ocean.UseParams", oceanData.useParams.toString());
            }
            if (oceanData.waveHeight != null) {
                oceanMsg.setProperty("Ocean.WaveHeight", oceanData.waveHeight.toString());
            }
            if (oceanData.seaLevel != null) {
                oceanMsg.setProperty("Ocean.SeaLevel", oceanData.seaLevel.toString());
            }
            if (oceanData.bumpScale != null) {
                oceanMsg.setProperty("Ocean.BumpScale", oceanData.bumpScale.toString());
            }
            if (oceanData.bumpSpeedX != null) {
                oceanMsg.setProperty("Ocean.BumpSpeedX", oceanData.bumpSpeedX.toString());
            }
            if (oceanData.bumpSpeedZ != null) {
                oceanMsg.setProperty("Ocean.BumpSpeedZ", oceanData.bumpSpeedZ.toString());
            }
            if (oceanData.textureScaleX != null) {
                oceanMsg.setProperty("Ocean.TextureScaleX", oceanData.textureScaleX.toString());
            }
            if (oceanData.textureScaleZ != null) {
                oceanMsg.setProperty("Ocean.TextureScaleZ", oceanData.textureScaleZ.toString());
            }
            if (oceanData.deepColor != null) {
                oceanMsg.setProperty("Ocean.DeepColor", oceanData.deepColor.toString());
            }
            if (oceanData.shallowColor != null) {
                oceanMsg.setProperty("Ocean.ShallowColor", oceanData.shallowColor.toString());
            }
            con.send(oceanMsg.toBuffer(player.getVersion()));
        }
        
        // Send fog
        Log.debug("processLogin: got fog: " + instanceInfo.fog);
        if (instanceInfo.fog != null) {
            WorldManagerClient.FogMessage fogMessage =
                new WorldManagerClient.FogMessage(0L, instanceInfo.fog);
            con.send(fogMessage.toBuffer());
        }
        
        // Send regions
        for (String config : instanceInfo.regionConfig) {
            Event regionEvent = new RegionConfiguration(config);
            con.send(regionEvent.toBytes());
        }

        // Spawn the player
        //
        TargetedExtensionMessage spawnBegin = new TargetedExtensionMessage(
                playerOid, playerOid);
        spawnBegin.setExtensionType("mv.SCENE_BEGIN");
        spawnBegin.setProperty("action","login");
        spawnBegin.setProperty(InstanceClient.TEMPL_INSTANCE_NAME,
            instanceInfo.name);
        spawnBegin.setProperty(
            InstanceClient.TEMPL_INSTANCE_TEMPLATE_NAME,
            instanceInfo.templateName);

        TargetedExtensionMessage spawnEnd = new TargetedExtensionMessage(
                playerOid, playerOid);
        spawnEnd.setExtensionType("mv.SCENE_END");
        spawnEnd.setProperty("action","login");

        Integer perceptionCount;
        //## This while loop should be used if instance restore is popped
        do {
            perceptionCount = WorldManagerClient.spawn(playerOid,
                spawnBegin,spawnEnd);
            if (perceptionCount < 0) {
                Log.error("processLogin: spawn failed error="+
                        perceptionCount+" playerOid=" + playerOid);
                if (perceptionCount == -2) {
                    // Instance does not exist.
                    //## Should pop the instance restore stack and try again
                    return false;
                }
                else
                    return false;
            }
            else
                break;
        } while (false);

        // If the new object count for the player as perceiver is
        // zero, there won't be a NewsAndFrees message, so set the
        // loading state to false now rather than waiting for
        // newsAndFrees processing to do it, because we won't get any
        // news and frees at startup.
        if (perceptionCount == 0 && player.getVersion().startsWith("1.1")) {
            player.setLoadingState(Player.LOAD_COMPLETE);
            con.send(new LoadingStateEvent(false).toBytes());
        }

        Log.debug("processLogin: During login, perceptionCount is " +
            perceptionCount);
                
        if (Log.loggingDebug)
            Log.debug("processLogin: spawned player, master playerOid=" +
                playerOid);
	return true;
    }

    /** Get the login callback.
    */
    public ProxyLoginCallback getProxyLoginCallback()
    {
        return proxyLoginCallback;
    }

    /** Set the login callback.  Methods on this object are
        called during player login.
    */
    public void setProxyLoginCallback(ProxyLoginCallback callback)
    {
        proxyLoginCallback = callback;
    }

    private static class DefaultProxyLoginCallback implements ProxyLoginCallback
    {
        public String preLoad(Player player, ClientConnection con)
        {
            return null;
        }

        public String postLoad(Player player, ClientConnection con)
        {
            return null;
        }

        public void postSpawn(Player player, ClientConnection con)
        {
            return;
        }
    }

    private boolean instanceEntryAllowed(long playerOid, Long instanceOid,
        Point location)
    {
        if (instanceOid == null)
            return false;

        return instanceEntryCallback.instanceEntryAllowed(playerOid,
                instanceOid, location);
    }

    /** Get the instance entry callback.
    */
    public InstanceEntryCallback getInstanceEntryCallback()
    {
        return instanceEntryCallback;
    }

    /** Set the instance entry callback.  Methods on this object are
        called during player instancing.
    */
    public void setInstanceEntryCallback(InstanceEntryCallback callback)
    {
        instanceEntryCallback = callback;
    }

    private static class DefaultInstanceEntryCallback
        implements InstanceEntryCallback
    {
        public boolean instanceEntryAllowed(long playerOid, Long instanceOid,
            Point location)
        {
            return true;
        }

        public Long selectInstance(Player player, String instanceName)
        {
            return InstanceClient.getInstanceOid(instanceName);
        }
    }

    /**
     * Player is asking for info about a quest
     */
    public void processRequestQuestInfo(ClientConnection con,
        RequestQuestInfo event)
    {
        Long npcOid = event.getQuestNpcOid();
        Player player = verifyPlayer("processRequestQuestInfo", event, con);
        if (Log.loggingDebug)
            Log.debug("processRequestQuestInfo: player=" + player +
                ", npc=" + npcOid);
        QuestClient.requestQuestInfo(npcOid, player.getOid());
    }

    /**
     * Player is accepting or declining a quest
     */
    public void processQuestResponse(ClientConnection con, QuestResponse event) {
        Player player = verifyPlayer("processQuestResponse", event, con);
        boolean acceptStatus = event.getResponse();
        Long npcOid = event.getQuestNpcOid();
        
        if (Log.loggingDebug)
            Log.debug("processQuestResponse: player=" + player + ", npcOid=" +
                npcOid + ", acceptStatus=" + acceptStatus);

        // send response message to QuestBehavior
        QuestClient.QuestResponseMessage msg =
            new QuestClient.QuestResponseMessage(npcOid, player.getOid(),
                acceptStatus);
        Engine.getAgent().sendBroadcast(msg);
    }
    
    /**
     * Player is attempting to conclude a quest with a mob
     */
    public void processReqConcludeQuest(ClientConnection con, ConcludeQuest event)
    {
        Player player = verifyPlayer("processReqConcludeQuest", event, con);
        Long mobOid = event.getQuestNpcOid();
        if (Log.loggingDebug)
            Log.debug("processReqConclude: player=" + player + ", mobOid=" + mobOid);
        QuestClient.requestConclude(mobOid, player.getOid());
    }

    /**
     * Method to log a client out, by asking the PlayerManager to set
     * its status to logged out, and enqueuing a ConnectionResetMessage
     * in the player's event SquareQueue.
     * @param con The connection to the client.
     */
    public void connectionReset(ClientConnection con)
    {
        Player player = (Player) con.getAssociation();

        if (player == null) {
            Log.info("ProxyPlugin: DISCONNECT remote=" + con);
            return;
        }
        Log.info("ProxyPlugin: DISCONNECT remote=" + con +
                " playerOid=" + player.getOid() + " name=" + player.getName());

	if (playerManager.logout(player)) {
	    ProxyPlugin.ConnectionResetMessage message =
		    new ProxyPlugin.ConnectionResetMessage(con, player);
	    message.setEnqueueTime(System.currentTimeMillis());
	    messageQQ.insert(player,message);
	}
    }

    class ConnectionResetMessage extends Message {
	ConnectionResetMessage(ClientConnection con, Player player) {
	    this.con = con;
            this.player = player;
	}
        public Player getPlayer() {
            return player;
        }
        public ClientConnection getConnection() {
            return con;
        }

        ClientConnection con;
        Player player;
        
        public static final long serialVersionUID = 1L;
    }

    /**
     * The method called by the event SquareQueue to reset the
     * connection for a player.  The player oid is in the event
     * object.
     */
    protected void processConnectionResetInternal(ConnectionResetMessage message)
    {
        long startTime = System.currentTimeMillis();
        ClientConnection con = message.getConnection();
        Player player = message.getPlayer();
        long playerOid = player.getOid();

        Log.info("ProxyPlugin: LOGOUT_BEGIN remote=" + con +
            " playerOid=" + player.getOid() + " name=" + player.getName());

        // sanity check
        if (player.getStatus() != Player.STATUS_LOGOUT) {
            log.error("processConnectionReset: player status is " +
                    Player.statusToString(player.getStatus()) +
                    " should be " +
                    Player.statusToString(Player.STATUS_LOGOUT));
        }
        
        // despawn the player
        if (! WorldManagerClient.despawn(playerOid)) {
            log.warn("processConnectionReset: despawn player failed for "
                    + playerOid);
        }
        
        if (perceptionFilter.removeTarget(playerOid)) {
            FilterUpdate filterUpdate = new FilterUpdate(1);
            filterUpdate.removeFieldValue(PerceptionFilter.FIELD_TARGETS,
                    new Long(playerOid));
            Engine.getAgent().applyFilterUpdate(perceptionSubId,filterUpdate);
            responderFilter.removeTarget(playerOid);
            Engine.getAgent().applyFilterUpdate(responderSubId,filterUpdate);
        }

        // Broadcast the logout.  An RPC is used, so we'll block until
        // all subscribers respond.
        // ## potential perf issue as there's only one logout processed
        // ## at a time
        LogoutMessage logoutMessage = new LogoutMessage(playerOid,
            player.getName());
        AsyncRPCCallback asyncRPCCallback = new AsyncRPCCallback(player,
            "processLogout: got LogoutMessage response");
        int expectedResponses = Engine.getAgent().sendBroadcastRPC(
            logoutMessage, asyncRPCCallback);
        asyncRPCCallback.waitForResponses(expectedResponses);

        // unload player to release memory
        if (! ObjectManagerClient.unloadObject(playerOid)) {
            log.warn("processConnectionReset: unloadObject failed oid="+
                playerOid);
        }

        // scrub the queues again because we've done two RPCs
        messageQQ.removeKey(player);
        eventQQ.removeKey(player);
        
        playerManager.removePlayer(playerOid);
        playerManager.removePerception(playerOid,playerOid);
        
        Log.info("ProxyPlugin: LOGOUT_END remote=" + con +
                " playerOid=" + player.getOid() + " name=" + player.getName() +
                " in-queue=" + (startTime - message.getEnqueueTime()) +
                " processing=" + (System.currentTimeMillis() - startTime) +
                " nPlayers=" + playerManager.getPlayerCount());
    }

    /**
     * Process a DirLocOrientEvent from the client; calls the
     * WorldManagerClient.updateWorldNode to get the work done.
     */
    protected void processDirLocOrient(ClientConnection con,
        DirLocOrientEvent event) {
        if (Log.loggingDebug)
            Log.debug("processDirLocOrient: got dir loc orient event: " + event);
        Player player = verifyPlayer("processDirLoc", event, con);

        // send an update to the world server
        BasicWorldNode wnode = new BasicWorldNode();
        wnode.setDir(event.getDir());
        wnode.setLoc(event.getLoc());
        wnode.setOrientation(event.getQuaternion());

        WorldManagerClient.updateWorldNode(player.getOid(), wnode);
    }

    /**
     * Process a ComEvent from the client, represent a chat event;
     * calls the WorldManagerClient.sendaChatMsg to get the work done.
     */
    protected void processCom(ClientConnection con, ComEvent event)
    {
        Player player = verifyPlayer("processCom", event, con);

        Log.info("ProxyPlugin: CHAT_SENT player=" + player +
            " channel=" + event.getChannelId() +
            " msg=[" + event.getMessage() + "]");

        incrementChatCount();
        WorldManagerClient.sendChatMsg(player.getOid(),
                event.getChannelId(), event.getMessage());
    }

    /**
     * Process a CommandEvent, representing a /foo command typed by a
     * client; looks up the command in the commandMap, and runs the
     * parser found there against the event, creating a Message
     * instance, and broadcasts the Message.
     */
    protected void processCommand(ClientConnection con, CommandEvent event) {
        /* Player player = */ verifyPlayer("processCommand", event, con);

        String cmd = event.getCommand().split(" ")[0];
        if (Log.loggingDebug)
            log.debug("processCommand: cmd=" + cmd + ", fullCmd="
                      + event.getCommand());

        // get the parser for the command
        CommandParser parser;
        commandMapLock.lock();
        try {
            parser = commandMap.get(cmd);
        } finally {
            commandMapLock.unlock();
        }
        if (parser == null) {
            Log.warn("processCommand: no parser for command: "
                    + event.getCommand());
            parser = commandMap.get("/unknowncmd");
            if (parser == null)
                parser.parse(event);
        }

        Engine.setCurrentPlugin(this);
        // Run the command
        parser.parse(event);
        Engine.setCurrentPlugin(null);
    }

    protected void processAutoAttack(ClientConnection con, AutoAttackEvent event) {
        // CombatClient.startAbility("attack ability",
        // event.getAttackerOid(),
        // event.getTargetOid(), OIDManager.invalidOid);
        CombatClient.autoAttack(event.getAttackerOid(), event.getTargetOid(),
				event.getAttackStatus());
    }

    protected void processActivateItem(ClientConnection con, ActivateItemEvent event) {
	InventoryClient.activateObject(event.getItemOid(), event.getObjectOid(), event.getTargetOid());
    }

    protected void processExtensionMessageEvent(ClientConnection con, ExtensionMessageEvent event) {
        String key = (String)event.getPropertyMap().get("ext_msg_subtype");
        Long target = event.getTargetOid();
        Player player = (Player)con.getAssociation();

        if (Log.loggingDebug)
            Log.debug("processExtensionMessageEvent: "+player+" subType="+key+
                " target="+target);

        List<ProxyExtensionHook> proxyHookList = extensionHooks.get(key);
        if (proxyHookList != null) {
            for (ProxyExtensionHook hook : proxyHookList) {
                hook.processExtensionEvent(event, player, this);
            }
            return;
        }

        if (target != null) {
            WorldManagerClient.TargetedExtensionMessage msg = 
                new WorldManagerClient.TargetedExtensionMessage(WorldManagerClient.MSG_TYPE_EXTENSION, target,
                        player.getOid(), 
                        event.getClientTargeted(), event.getPropertyMap());
            if (event.getClientTargeted()) {
                msg.setMsgType(WorldManagerClient.MSG_TYPE_P2P_EXTENSION);
                if (allowClientToClientMessage(player, target, msg))
                    Engine.getAgent().sendBroadcast(msg);
            }
            else {
                MessageType msgType = getExtensionMessageType(key);
                if (msgType == null) {
                    Log.error("processExtensionMessageEvent: key '" + key + "' has no corresponding MessageType");
                    return;
                }
                msg.setMsgType(msgType);
                Engine.getAgent().sendBroadcast(msg);
            }
        }
        else {
            MessageType msgType = getExtensionMessageType(key);
            if (msgType == null) {
                Log.error("processExtensionMessageEvent: key '" + key + "' has no corresponding MessageType");
                return;
            }
            ExtensionMessage msg = new ExtensionMessage(msgType,
                player.getOid(), event.getPropertyMap());
            Engine.getAgent().sendBroadcast(msg);
        }
    }
    
    /** Register mapping from extension sub-type to server message type.
        Client-originated extension message with {@code subtype} will be
        assigned server message type {@code type}.
    */
    public void registerExtensionSubtype(String subtype, MessageType type) {
        extensionMessageRegistry.put(subtype, type);
    }

    /** Unregister extension sub-type mapping.
    */
    public MessageType unregisterExtensionSubtype(String subtype) {
        return extensionMessageRegistry.remove(subtype);
    }

    /** Get server message type for an extension message sub-type.
    */
    public MessageType getExtensionMessageType(String subtype) {
        return extensionMessageRegistry.get(subtype);
    }

    /** Return true if the message is allowed from sending to target player.
        By default, returns the value of {@link #defaultAllowClientToClientMessage}.
        <p>
        Over-ride to implement security and filtering logic.  The
        {@code message} can be modified by this method; the modified
        message is sent to the target client.
        @param sender Sending player.
        @param targetOid Target player OID.
        @param message The extension message.
        @return False if the message should not be sent to target player.
    */
    public boolean allowClientToClientMessage(Player sender, Long targetOid,
            WorldManagerClient.TargetedExtensionMessage message)
    {
        return defaultAllowClientToClientMessage;
    }

    /** Default value of {@link #allowClientToClientMessage allowClientToClientMessage()} when there is no method over-ride.
    */
    public boolean defaultAllowClientToClientMessage = false;

    protected HashMap<String, MessageType> extensionMessageRegistry = new HashMap<String, MessageType>();

    /**
     * A base class for the normal kind of proxy hook that uses the
     * default implementation of processMessage(Message msg, int flags).
     */
    abstract class BasicProxyHook implements ProxyHook {
        public boolean processMessage(Message msg, int flags) {
            return true;
        }
        
        abstract public void processMessage(Message msg, int flags, Player player);
    }
    
    /**
     * Got a display context for one of the mobs we care about, which
     * must repackaged as an Event and sent to the client.
     */
    class DisplayContextHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.DisplayContextMessage dcMsg =
                (WorldManagerClient.DisplayContextMessage) msg;

            Long dcObjOid = dcMsg.getSubject();
            DisplayContext dc = dcMsg.getDisplayContext();
            if (Log.loggingDebug)
                log.debug("handleDC: oid=" + dcObjOid + " dc=" + dc);
            
            ClientConnection con = player.getConnection();

            // create the modelinfo event
            if (dc != null) {
                ModelInfoEvent event = new ModelInfoEvent(dcObjOid);
                event.setDisplayContext(dc);
                con.send(event.toBytes());
            }
            
            // send over attachment mesg
            Map<String, DisplayContext> childMap = dc.getChildDCMap();
            if ((childMap != null) && (!childMap.isEmpty())) {
                for (String slot : childMap.keySet()) {
                    DisplayContext attachDC = childMap.get(slot);
                    if (attachDC == null) {
                        throw new MVRuntimeException("attach DC is null for obj: "
                                + dcObjOid);
                    }
                    Long attacheeOID = attachDC.getObjRef();
                    if (attacheeOID == null) {
                        throw new MVRuntimeException("attachee oid is null for obj: "
                                + dcObjOid);
                    }
                    // send over an attach event for each attachment
                    if (Log.loggingDebug)
                        log.debug("DisplayContextHook: sending attach message to "
                                  + player.getOid() + " attaching to obj " + dcObjOid
                                  + ", object being attached=" + attacheeOID +
                                  " to slot " + slot + ", attachmentDC=" + attachDC);

                    AttachEvent event = new AttachEvent(dcObjOid, attacheeOID,
                            slot, attachDC);
                    con.send(event.toBytes());
                }
                log.debug("DisplayContextHook: done with processing attachments");
            }
        }
    }

    /**
     * The world manager plugin is telling a player that he has a new
     * directional light.
     */
    class NewDirLightHook extends BasicProxyHook {
        public void processMessage(Message m, int flags, Player player)
        {
            WorldManagerClient.NewDirLightMessage msg =
                (WorldManagerClient.NewDirLightMessage) m;
            Long playerOid = msg.getTarget();
            Long lightOid = msg.getSubject();
            LightData lightData = msg.getLightData();

            if (playerOid != player.getOid()) {
                Log.error("Message target and perceiver mismatch");
            }

            ClientConnection con = player.getConnection();

            if (Log.loggingDebug)
                log.debug("NewDirLightHook: notifyOid=" + playerOid +
                        ", lightOid=" + lightOid + ", light=" + lightData);
            NewLightEvent lightEvent =
                new NewLightEvent(playerOid, lightOid, lightData);
            con.send(lightEvent.toBytes());
        }
    }
      
    // MSG_TYPE_FREE_OBJECT is only used for directional light regions
    class FreeObjectHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.FreeObjectMessage message =
                (WorldManagerClient.FreeObjectMessage) msg;

            player.getConnection().send(message.toBuffer());
        }
    }

    /**
     * The world manager plugin is telling a player that he has a new
     * ambient light.
     */
    class SetAmbientHook extends BasicProxyHook {
        public void processMessage(Message m, int flags, Player player)
        {
            WorldManagerClient.SetAmbientLightMessage msg =
                (WorldManagerClient.SetAmbientLightMessage)m;
            Color ambientLight = msg.getColor();
            Long playerOid = msg.getTarget();

            if (playerOid != player.getOid()) {
                Log.error("Message target and perceiver mismatch");
            }

            ClientConnection con = player.getConnection();

            if (Log.loggingDebug)
                log.debug("SetAmbientHook: targetOid=" + playerOid +
                        ", ambient=" + ambientLight);

            Event ambientLightEvent = new AmbientLightEvent(ambientLight);
            con.send(ambientLightEvent.toBytes());
        }
    }

    /**
     * The world manager is telling us to remove an attachment from a
     * socket.
     */
    class DetachHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.DetachMessage dMsg =
                (WorldManagerClient.DetachMessage) msg;

            Long dcObjOid = dMsg.getSubject();
            Long objBeingDetached = dMsg.getObjBeingDetached();
            String socket = dMsg.getSocketName();
            if (Log.loggingDebug)
                log.debug("DetachHook: dcObjOid=" + dcObjOid +
                        ", objBeingDetached=" + objBeingDetached +
                        ", socket=" + socket);
            
            // get the user's connection
            ClientConnection con = player.getConnection();

            DetachEvent detachEvent =
                new DetachEvent(dcObjOid, objBeingDetached, socket);
            con.send(detachEvent.toBytes());
        }
    }

    /**
     * We got an animation msg from the world server relay to client.
     */
    class AnimationHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {

            WorldManagerClient.AnimationMessage animMsg =
                (WorldManagerClient.AnimationMessage) msg;

            // get the user's connection
            Long playerOid = player.getOid();
            ClientConnection con = player.getConnection();

            Long objOid = animMsg.getSubject();
            List<AnimationCommand> animList = animMsg.getAnimationList();

            // make the event
            NotifyPlayAnimationEvent animEvent = new NotifyPlayAnimationEvent(
                    objOid);
            animEvent.setAnimList(animList);

            // send over a detach event
            con.send(animEvent.toBytes());
            if (Log.loggingDebug)
                log.debug("AnimationHook: send anim msg for playerOid " +
                        playerOid + ", objId=" +
                        objOid + ", animEvent=" + animEvent);
        }
    }
    
    /**
     * We got an invoke effect msg -- send it to the client.
     */
    class InvokeEffectHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            AnimationClient.InvokeEffectMessage effectMsg = 
                (AnimationClient.InvokeEffectMessage) msg;

            Long objOid = effectMsg.getSubject();

            if (Log.loggingDebug)
                log.debug("InvokeEffectHook: got msg=" + effectMsg.toString());

            // get the user's connection
            ClientConnection con = player.getConnection();

            // send it over
            MVByteBuffer buf = effectMsg.toBuffer(player.getVersion());
            if (buf != null) {
                con.send(buf);
                if (Log.loggingDebug)
                    log.debug("InvokeEffectHook: sent ext msg for notifyOid " + objOid);
            }
        }
    }

    /**
     * We got an extension msg -- send it to the client.
     */
    class ExtensionHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            MVByteBuffer buf = null;
            // get the user's connection
            ClientConnection con = player.getConnection();
            Long subject = null;
            Long target = null;
            String subType = null;

            if (msg instanceof WorldManagerClient.TargetedExtensionMessage) {
                WorldManagerClient.TargetedExtensionMessage extMsg = 
                    (WorldManagerClient.TargetedExtensionMessage) msg;

		subject = extMsg.getSubject();
		target = extMsg.getTarget();
                subType = extMsg.getExtensionType();

                if (Log.loggingDebug) {
                    Set<String> keySet = extMsg.keySet();
                    for (String key : keySet) {
                        log.debug("ExtensionHook: playerOid=" + player.getOid() +
                            ", oid="
                            + subject + ", key " + key + ", value="
                            + extMsg.getProperty(key));
                    }
                }

                // Create the buffer
                buf = extMsg.toBuffer(player.getVersion());

            }
            else {
                WorldManagerClient.ExtensionMessage extMsg = 
                    (WorldManagerClient.ExtensionMessage) msg;

		subject = extMsg.getSubject();
                subType = extMsg.getExtensionType();

                if (Log.loggingDebug) {
                    Set<String> keySet = extMsg.keySet();
                    for (String key : keySet) {
                        log.debug("ExtensionHook: playerOid=" + player.getOid() +
                            ", oid=" + subject +
			    ", key " + key + ", value="
                            + extMsg.getProperty(key));
                    }
                }

                // Create the buffer
                buf = extMsg.toBuffer(player.getVersion());
            }
            
            if (buf != null) {
                con.send(buf);
                if (Log.loggingDebug)
                    log.debug("ExtensionHook: sent subType " +
			subType + " for playerOid=" +
			player.getOid() +
			", target=" + target + ", subject=" + subject);
            }
        }
    }

    /**
     * We got an extension msg -- send it to the client.
     */
    class P2PExtensionHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.TargetedExtensionMessage extMsg = 
                (WorldManagerClient.TargetedExtensionMessage) msg;

            Long objOid = extMsg.getSubject();

            Set<String> keySet = extMsg.keySet();
            for (String key : keySet) {
                if (Log.loggingDebug)
                    log.debug("P2PExtensionHook: playerOid=" + player.getOid() +
                                ", oid = "
                                + objOid + ", got key " + key + ", value="
                                + extMsg.getProperty(key));
            }

            // get the user's connection
            ClientConnection con = player.getConnection();

            // send it over
            MVByteBuffer buf = extMsg.toBuffer(player.getVersion());
            if (buf != null) {
                con.send(buf);
                if (Log.loggingDebug)
                    log.debug("P2PExtensionHook: sent ext msg for notifyOid " + objOid);
            }
        }
    }

    /**
     * We got a property msg from the world server.  Properties are
     * usually health, mana, int, str, etc relay to client.
     */
    class PropertyHook extends BasicProxyHook {
        public boolean processMessage(Message msg, int flags) {
            return true;
        }
        public void processMessage(Message msg, int flags, Player player)
        {
            PropertyMessage propMsg = (PropertyMessage) msg;

            long subjectOid = propMsg.getSubject();

	    if (Log.loggingDebug) {
		Set<String> keySet = propMsg.keySet();
		for (String key : keySet) {
		    log.debug("handlePropertyMsg: player=" + player + ", oid="
			      + subjectOid + ", got key " + key + ", value="
			      + propMsg.getProperty(key));
		}
	    }

            ClientConnection con = player.getConnection();

            MVByteBuffer buf = null;
            if (playerSpecificProps.size() > 0 && subjectOid != player.getOid())
                buf = propMsg.toBuffer(player.getVersion(), cachedPlayerSpecificFilterProps);
            else
                buf = propMsg.toBuffer(player.getVersion(), filteredProps);
            // send it over
	    if (buf != null) {
		con.send(buf);
                if (Log.loggingDebug)
                    log.debug("sent prop msg for player " + player +
                        ", subjectId="+ subjectOid);
	    }
            else if (Log.loggingDebug)
                log.debug("filtered out prop msg for player " +
                        player + ", subjectId=" + subjectOid + 
                        " because all props were filtered");
        }
    }

    /**
     * We got a property msg from the world server.  Properties are
     * usually health, mana, int, str, etc relay to client
     */
    class TargetedPropertyHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.TargetedPropertyMessage propMsg =
                (WorldManagerClient.TargetedPropertyMessage) msg;

            Long targetOid = propMsg.getTarget();
            Long subjectOid = propMsg.getSubject();

            if (Log.loggingDebug) {
                Set<String> keySet = propMsg.keySet();
                for (String key : keySet) {
                    log.debug("handleTargetedPropertyMsg: playerOid=" +
                        player.getOid() + ", targetOid=" + targetOid + ", oid = "
                        + subjectOid + ", got key " + key + ", value="
                        + propMsg.getProperty(key));
                }
            }

            // get the user's connection
            ClientConnection con = player.getConnection();

            // send it over
            MVByteBuffer buf = null;
            if (playerSpecificProps.size() > 0 && subjectOid != player.getOid())
                buf = propMsg.toBuffer(player.getVersion(), cachedPlayerSpecificFilterProps);
            else
                buf = propMsg.toBuffer(player.getVersion(), filteredProps);
	    if (buf != null) {
		con.send(buf);
                if (Log.loggingDebug)
                    log.debug("sent targeted prop msg for targetOid " +
                        targetOid + ", subjectOid=" + subjectOid);
	    }
            else if (Log.loggingDebug)
                log.debug("filtered out targeted prop msg for targetOid " +
                        targetOid + ", subjectOid=" + subjectOid + 
                        " because all props were filtered");
        }
    }

    /**
     * Check to see if making this new object is one of the special
     * cases of object creation: making a light, or a terrain decal,
     * or a point sound.  If so, do the processing here.  If not, get
     * the object info for the object, and send it to the client.
     * @param objectNote Describes the object to be created.
     * @param player The player object to which the message should be
     * sent.
     * @return True if special-case handling took place, false
     * otherwise.
     */
    protected boolean specialCaseNewProcessing(
        PerceptionMessage.ObjectNote objectNote, Player player)
    {

        long start = System.currentTimeMillis();

        ClientConnection con = player.getConnection();

        Long objOid = objectNote.getSubject();
        ObjectType objType = (ObjectType) objectNote.getObjectType();

        // special case lights, since lights need to send
        // over a LightMessage, not the normal ObjectInfo
        if (objType == ObjectTypes.light) {
            Log.debug("specialCaseNewProcessing: got a light object");
            LightData lightData = (LightData) EnginePlugin.getObjectProperty(objOid, Namespace.WORLD_MANAGER, Light.LightDataPropertyKey);
            if (Log.loggingDebug)
                Log.debug("specialCaseNewProcessing: light data=" + lightData);
            NewLightEvent lightEvent = new NewLightEvent(player.getOid(),
                objOid, lightData);
            con.send(lightEvent.toBytes());
            return true;
        }

        if (objType.equals(WorldManagerClient.TEMPL_OBJECT_TYPE_TERRAIN_DECAL)) {
            Log.debug("specialCaseNewProcessing: got a terrain decal object");
            TerrainDecalData decalData =
                (TerrainDecalData) EnginePlugin.getObjectProperty(objOid, Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_TERRAIN_DECAL_DATA);
            if (Log.loggingDebug)
                Log.debug("specialCaseNewProcessing: terrain decal data=" + decalData);
            NewTerrainDecalEvent decalEvent= new NewTerrainDecalEvent(
                objOid, decalData);
            con.send(decalEvent.toBytes());
            return true;
        }
            
        if (objType.equals(WorldManagerClient.TEMPL_OBJECT_TYPE_POINT_SOUND)) {
            Log.debug("specialCaseNewProcessing: got a point sound object");
            List<SoundData> soundData =
                (List<SoundData>) EnginePlugin.getObjectProperty(objOid, Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_SOUND_DATA_LIST);
            if (Log.loggingDebug)
                Log.debug("specialCaseNewProcessing: sound data=" + soundData);
            WorldManagerClient.SoundMessage soundMsg;
            soundMsg = new WorldManagerClient.SoundMessage(objOid);
            soundMsg.setSoundData(soundData);
            con.send(soundMsg.toBuffer());
            return true;
        }

        WorldManagerClient.PerceptionInfo perceptionInfo =
            (WorldManagerClient.PerceptionInfo) objectNote.getObjectInfo();

        // send the client a new object msg
	if (perceptionInfo.objectInfo == null) {
	    // No object info, it must have been deleted/despawned
	    return true;
	}
        // unfotunately, the objectInfo message must come _before_ the
        // model info, but the mob path message must come _after_ the
        // model info, so extract the mob path message, if any.
        WorldManagerClient.MobPathMessage pathMsg = 
            (WorldManagerClient.MobPathMessage)perceptionInfo.objectInfo.getProperty(WorldManagerClient.MOB_PATH_PROPERTY);
        con.send(perceptionInfo.objectInfo.toBuffer(player.getOid()));

        if (perceptionInfo.displayContext != null) {
            ModelInfoEvent modelInfoEvent = new ModelInfoEvent(objOid);
            modelInfoEvent.setDisplayContext(perceptionInfo.displayContext);
            con.send(modelInfoEvent.toBytes());
        }
        else {
            if (Log.loggingDebug)
                Log.debug("No display context for "+objOid);
        }

        // If it's a mob, send on the mob path message, if any
        if (pathMsg != null) {
            // If it is expired, remove it from the map
            if (pathMsg.pathExpired()) {
                if (Log.loggingDebug)
                    Log.debug("specialCaseNewProcessing: for mob " + objOid + ", last mob path expired " + pathMsg.toString());
            }
            // Otherwise send it to the client
            else {
                if (Log.loggingDebug)
                    Log.debug("specialCaseNewProcessing: for mob " + objOid + ", sending last mob path " + pathMsg.toString());
                MVByteBuffer pathBuf = pathMsg.toBuffer();
                con.send(pathBuf);
            }
        }

        // send over attachment mesg
        Map<String, DisplayContext> childMap = null;
        if (perceptionInfo.displayContext != null)
            childMap = perceptionInfo.displayContext.getChildDCMap();
        if ((childMap != null) && (!childMap.isEmpty())) {
            for (String slot : childMap.keySet()) {
                DisplayContext attachDC = childMap.get(slot);
                if (attachDC == null) {
                    throw new MVRuntimeException("attach DC is null for obj: "
                        + objOid);
                }
                Long attacheeOID = attachDC.getObjRef();
                if (attacheeOID == null) {
                    throw new MVRuntimeException("attachee oid is null for obj: "
                        + objOid);
                }
                // send over an attach event for each attachment
                if (Log.loggingDebug)
                    Log.debug("specialCaseNewProcessing: sending attach message to "
                        + player.getOid() + " attaching to obj " + objOid
                        + ", object being attached=" + attacheeOID +
                        " to slot " + slot + ", attachmentDC=" + attachDC);

                AttachEvent event = new AttachEvent(objOid, attacheeOID,
                    slot, attachDC);
                con.send(event.toBytes());
            }
            if (Log.loggingDebug)
                Log.debug("specialCaseNewProcessing: done with processing attachments");
        }

//## Experimental: PerceptionMessage contains WM properties
/*
        WorldManagerClient.TargetedPropertyMessage wmProps =
            (WorldManagerClient.TargetedPropertyMessage) perceptionInfo.objectInfo.getProperty("wmProps");
        if (wmProps != null) {
	    MVByteBuffer buf = wmProps.toBuffer(player.getVersion(),
                filteredProps);
	    if (buf != null) {
		con.send(buf);
                if (Log.loggingDebug)
                    log.debug("sent targeted prop msg for oid " + objOid);
	    }
            else if (Log.loggingDebug)
                log.debug("filtered out targeted prop msg for oid " +
                        objOid + " because all props were filtered");
        }
*/

        long finish = System.currentTimeMillis();
        if (Log.loggingDebug)
            Log.debug("specialCaseNewProcessing: finished.  playerOid=" +
                player.getOid() + ", oid=" + objOid +
                " in " + (finish - start) + " ms");
        return false;
    }
    
    /**
     * Check to see if freeing of this object is one of the special
     * cases of object freeing: freeing a road, or a terrain decal.
     * If so, do the processing here.
     * @param objectNote Describes the object to be created.
     * @param player The player object to which the message should be
     * sent.
     * @return True if special-case handling took place, false
     * otherwise.
     */
    protected boolean specialCaseFreeProcessing(
        PerceptionMessage.ObjectNote objectNote, Player player)
    {
        if (player.getOid() == objectNote.getSubject()) {
            Log.debug("ignoring free object message to self");
            return true;
        }
        
        ClientConnection con = player.getConnection();
        if (!con.isOpen())
            con = null;

        Long objOid = objectNote.getSubject();

        if (objectNote.getObjectType() == ObjectTypes.road) {
            if (Log.loggingDebug)
                Log.debug("specialCaseFreeProcessing: playerOid=" +
                    player.getOid() +
                    ", roadSegmentOid=" + objOid);
            handleFreeRoad(con, objOid);
            return true;
        }
        if (objectNote.getObjectType().equals(
                WorldManagerClient.TEMPL_OBJECT_TYPE_TERRAIN_DECAL)) {
            if (Log.loggingDebug)
                Log.debug("specialCaseFreeProcessing: playerOid=" +
                    player.getOid() + ", decalOid=" + objOid);
            FreeTerrainDecalEvent decalEvent= new FreeTerrainDecalEvent(
                objOid);
            if (con != null)
                con.send(decalEvent.toBytes());
            return true;
        }

        // send the client a free object msg
        if (Log.loggingDebug)
            Log.debug("specialCaseFreeProcessing: playerOid=" +
                player.getOid() + ", objOid=" + objOid);

        NotifyFreeObjectEvent freeEvent =
                new NotifyFreeObjectEvent(player.getOid(), objOid);

        if (con != null)
            con.send(freeEvent.toBytes());

        return false;
    }
    
    /**
     * Tell the client to free the road represented by the objOid.
     */
    protected void handleFreeRoad(ClientConnection con, Long objOid) {
        WorldManagerClient.FreeRoadMessage freeRoadMsg = new WorldManagerClient.FreeRoadMessage(
            objOid);
        MVByteBuffer buf = freeRoadMsg.toBuffer();
        if (con != null)
            con.send(buf);
    }

    class PerceptionHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            PerceptionMessage perceptionMessage = (PerceptionMessage) msg;

            List<PerceptionMessage.ObjectNote> gain =
                perceptionMessage.getGainObjects();
            List<PerceptionMessage.ObjectNote> lost =
                perceptionMessage.getLostObjects();

            if (Log.loggingDebug)
                Log.debug("PerceptionHook.processMessage: start " +
                    ((gain==null)?0:gain.size()) + " gain and " +
                    ((lost==null)?0:lost.size()) + " lost");

if (player.getOid() == World.DEBUG_OID)
    Log.info("PerceptionHook: oid="+World.DEBUG_OID + " start " +
        ((gain==null)?0:gain.size()) + " gain and " +
        ((lost==null)?0:lost.size()) + " lost");

            ClientConnection con = player.getConnection();

            synchronized (playerManager) {
                LinkedList<Long> newSubjects = new LinkedList<Long>();
                LinkedList<Long> deleteSubjects = new LinkedList<Long>();
                if (lost != null)
                    playerManager.removePerception(player,lost,deleteSubjects);
                if (gain != null)
                    playerManager.addPerception(player,gain,newSubjects);
                if (deleteSubjects.size() > 0 || newSubjects.size() > 0) {
                    FilterUpdate perceptionUpdate = new FilterUpdate(
                        deleteSubjects.size() + newSubjects.size());
                    for (Long oid : deleteSubjects) {
                        perceptionUpdate.removeFieldValue(
                            PerceptionFilter.FIELD_SUBJECTS, oid);
                    }
                    for (Long oid : newSubjects) {
                        perceptionUpdate.addFieldValue(
                            PerceptionFilter.FIELD_SUBJECTS, oid);
                    }

                    if (player.getOid() == World.DEBUG_OID)
                        Log.info("subject changes: "+
                            newSubjects.size() + " gained " +
                            deleteSubjects.size() + " lost");

                    // Send FilterUpdate to every producer except the
                    // one that sent us the PerceptionMessage (indicated
                    // by last 'perceptionMessage' parameter).
                    Engine.getAgent().applyFilterUpdate(perceptionSubId,
                        perceptionUpdate, MessageAgent.NO_FLAGS,
                        perceptionMessage);
                }
            }

            boolean loadingState = false;
            if (player.getVersion().startsWith("1.1") &&
                    (player.getLoadingState() == Player.LOAD_PENDING ||
                    (gain != null && gain.size() > 3))) {
                // First perceivable objects, so tell client to defer
                // rendering and display loading screen.
                con.send(new LoadingStateEvent(true).toBytes());
                loadingState = true;
            }

            if (lost != null) {
                for (PerceptionMessage.ObjectNote objectNote : lost) {
                    specialCaseFreeProcessing(objectNote, player);
                }
            }

            if (gain != null) {
                for (PerceptionMessage.ObjectNote objectNote : gain) {
                    try {
                        specialCaseNewProcessing(objectNote, player);
                        WorldManagerClient.updateObject(
                            player.getOid(), objectNote.getSubject());
                    }
                    catch (Exception e) {
                        Log.exception("specialCaseNewProcessing: player="+player+
                                " oid="+objectNote.getSubject(), e);
                    }
                }
            }

            if (loadingState) {
                player.setLoadingState(Player.LOAD_COMPLETE);
                con.send(new LoadingStateEvent(false).toBytes());
            }
        }
    }
            
    class VoiceParmsHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            // Send the voice server host and port back to the client
            WorldManagerClient.TargetedExtensionMessage extMsg = new WorldManagerClient.TargetedExtensionMessage("voice_parms_response", player.getOid());
            extMsg.setProperty("host", voiceServerHost);
            extMsg.setProperty("port", voiceServerPort);
            
            SecureTokenSpec tokenSpec = new SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_DOMAIN,
                                                            Engine.getAgent().getName(),
                                                            System.currentTimeMillis() + 30000);
            tokenSpec.setProperty("player_oid", player.getOid());
            byte[] authToken = SecureTokenManager.getInstance().generateToken(tokenSpec);
            extMsg.setProperty("auth_token", Base64.encodeBytes(authToken));

            // get the user's connection
            ClientConnection con = player.getConnection();

            // send it over
            MVByteBuffer buf = extMsg.toBuffer(player.getVersion());
            if (buf != null) {
                con.send(buf);
                if (Log.loggingDebug)
                    log.debug("VoiceParmsHook: sent voice_parm_response ext msg for player " + player.getOid());
            }

        }
    }
    
    /**
     * Get the ignored oids property and if it is null, create it and
     * set the player's property, and update the player.  message.
     * @param player The player object
     */
    protected void processPlayerIgnoreList(Player player) {
        Set<Long> ignoreSet = (HashSet<Long>)getObjectProperty(player.getOid(), Namespace.WORLD_MANAGER, "ignored_oids");
        player.initializeIgnoredOids(ignoreSet);
        if (ignoreSet == null)
            player.setIgnoredOidsProperty();
        sendPlayerIgnoreList(player);
    }
    
    /**
     * Get the ignored oids from the player object, then get the
     * corresponding names and send both to the client in a property
     * message.
     * @param player The player object
     */
    protected void sendPlayerIgnoreList(Player player) {        
        String missing = " Missing ";
        List<Long> ignoreList = player.getIgnoredOids();
        List<String> ignoredNames = Engine.getDatabase().getObjectNames(
            ignoreList, WorldManagerClient.NAMESPACE, missing);
        boolean updatedList = false;
        int index = 0;
        while ((index = ignoredNames.indexOf(missing)) >= 0) {
            ignoredNames.remove(index);
            ignoreList.remove(index);
            updatedList = true;
        }
        if (updatedList)
            player.setIgnoredOids(ignoreList);
        PropertyMessage msg = new PropertyMessage(player.getOid(), player.getOid());
        msg.setProperty("ignored_oids", (Serializable)ignoreList);
        msg.setProperty("ignored_player_names", (Serializable)ignoredNames);
        if (Log.loggingDebug)
            Log.debug("processPlayerIgnoreList: Sent player " + player.getOid() + 
                " property message with " + ignoreList.size() + " oids: " + Database.makeOidCollectionString(ignoreList) + 
                " and " + ignoredNames.size() + " names: " + Database.makeNameCollectionString(ignoredNames));
        player.getConnection().send(msg.toBuffer(player.getVersion()));
    }

    class UpdatePlayerIgnoreListHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            TargetedExtensionMessage extMsg = (TargetedExtensionMessage)msg;
            LinkedList<Long> nowIgnored = (LinkedList<Long>)extMsg.getProperty("now_ignored");
            LinkedList<Long> noLongerIgnored = (LinkedList<Long>)extMsg.getProperty("no_longer_ignored");
            updateIgnoredOids(player, nowIgnored, noLongerIgnored);
        }
    }

    public void updateIgnoredOids(Player player, List<Long> nowIgnored, List<Long> noLongerIgnored) {
        player.updateIgnoredOids(nowIgnored, noLongerIgnored);
        // Send the ignore list to the player
        sendPlayerIgnoreList(player);
        if (Log.loggingDebug)
            log.debug("ProxyPlugin.UpdatePlayerIgnoreListHook: For player " + player.getOid() + 
                ", added " + (nowIgnored == null ? "0" : ((Integer)nowIgnored.size()).toString() + ": " + Database.makeOidCollectionString(nowIgnored)) +
                ", removed " + (noLongerIgnored == null ? "0" : ((Integer)noLongerIgnored.size()).toString() + Database.makeOidCollectionString(noLongerIgnored)) +
                ", current oid count " + player.ignoredOidCount() + ": " + Database.makeOidCollectionString(player.getIgnoredOids()));
        // We need to relay the contents of this message to
        // subscribers.  It is done in this way so that the
        // message doesn't get sent to every client who perceives
        // this player.
        ExtensionMessage relayMsg = new ExtensionMessage();
        relayMsg.setMsgType(MSG_TYPE_RELAY_UPDATE_PLAYER_IGNORE_LIST);
        relayMsg.setSubject(player.getOid());
        relayMsg.setProperty("now_ignored", (Serializable)nowIgnored);
        relayMsg.setProperty("no_longer_ignored", (Serializable)noLongerIgnored);
        Engine.getAgent().sendBroadcast(relayMsg);
    }

    class GetMatchingPlayersHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            TargetedExtensionMessage extMsg = (TargetedExtensionMessage)msg;
            String playerName = (String)extMsg.getProperty("player_name");
            Boolean exactMatch = (Boolean)extMsg.getProperty("exact_match");
            boolean match = exactMatch == null ? true : (boolean)exactMatch;
            List<Object> matchLists = Engine.getDatabase().getOidsAndNamesMatchingName(playerName, match);
            TargetedExtensionMessage response = new TargetedExtensionMessage("player_ignore_list", player.getOid());
            response.setSubject(player.getOid());
            List<Long> oids = (List<Long>)matchLists.get(0);
            List<String> names = (List<String>)matchLists.get(1);
            response.setProperty("ignored_oids", (Serializable)oids);
            response.setProperty("ignored_player_names", (Serializable)names);
            if (Log.loggingDebug)
                log.debug("ProxyPlugin.GetMatchingPlayersHook: For player " + player.getOid() + 
                    ", found " + (oids == null ? 0 : oids.size()) + " players: " + Database.makeOidCollectionString(oids) + 
                    " " + (match ? "exactly matching" : "starting with") + 
                    " name '" + playerName + "':" + Database.makeNameCollectionString(names));
            player.getConnection().send(response.toBuffer(player.getVersion()));
        }
    }

    // The hook above won't work for us, as there's no RPC to the client.
    // The client requesting a list of oids to ignore, then processing an
    // ignore request for each oid, isn't feasible.  So we'll only process
    // ignores for multiple "John" characters from the server, via the
    // /ignore command.  This function handles processing of that request.
    //
    public List<Object> matchingPlayers(Player player, String playerName, Boolean exactMatch) {
        boolean match = exactMatch == null ? true : (boolean)exactMatch;
        List<Object> matchLists = Engine.getDatabase().getOidsAndNamesMatchingName(playerName, match);
        List<Long> oids = (List<Long>)matchLists.get(0);
        List<String> names = (List<String>)matchLists.get(1);
        if (Log.loggingDebug)
            log.debug("ProxyPlugin.matchingPlayers: For player " + player.getOid() + 
                ", found " + (oids == null ? 0 : oids.size()) + " players: " + Database.makeOidCollectionString(oids) + 
                " " + (match ? "exactly matching" : "starting with") + 
                " name '" + playerName + "':" + Database.makeNameCollectionString(names));
        return matchLists;
    }

    class PlayerIgnoreListReqHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            List<Long> oids = player.getIgnoredOids();
            if (Log.loggingDebug)
                log.debug("ProxyPlugin.PlayerIgnoreListReqHook: For player " + player.getOid() + ", responding with " + oids.size() + " oids");
            Engine.getAgent().sendResponse(new GenericResponseMessage(msg, player.getIgnoredOids()));
        }
    }

    // Parse the fields in the player path request from the client;
    // encache path metadata if the message contains such; generate
    // the player path using the pathing A* algorithm; and send all
    // perceivers of the player a MobPathMessage containing the
    // player's path.
    class PlayerPathReqHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            long playerOid = player.getOid();
            BasicWorldNode wnode = WorldManagerClient.getWorldNode(playerOid);
            WorldManagerClient.ExtensionMessage extMsg = (WorldManagerClient.ExtensionMessage)msg;
            WorldManagerClient.PlayerPathWMReqMessage reqMsg = new WorldManagerClient.PlayerPathWMReqMessage(
                playerOid,
                wnode.getInstanceOid(),
                (String)extMsg.getProperty("room_id"),
                (MVVector)extMsg.getProperty("start"),
                (Float)extMsg.getProperty("speed"),
                (Quaternion)extMsg.getProperty("start_orient"),
                (MVVector)extMsg.getProperty("dest"),
                (Quaternion)extMsg.getProperty("dest_orient"),
                (List<MVVector>)extMsg.getProperty("boundary"),
                (List<List<MVVector>>)extMsg.getProperty("obstacles"),
                (Float)extMsg.getProperty("avatar_width"));
            Engine.getAgent().sendBroadcast(reqMsg);
        }
    }

    class ComHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            MVByteBuffer buf = null;
            if (msg instanceof WorldManagerClient.ComMessage) {
                WorldManagerClient.ComMessage comMsg =
                    (WorldManagerClient.ComMessage) msg;
                long oid = comMsg.getSubject();
                if (player.oidIgnored(oid)) {
                	if (Log.loggingDebug)
                		Log.debug("ComHook.processMessage: Ignoring chat from player " + oid + " to player " + player.getOid() +
                				" because originator is in the player's ignored list");
                	return;		
                }
                buf = comMsg.toBuffer();
                Log.info("ProxyPlugin: CHAT_RECV player=" + player +
                    " from=" + comMsg.getSubject() +
                    " private=false" +
                    " msg=[" + comMsg.getString() + "]");
            }
            else if (msg instanceof WorldManagerClient.TargetedComMessage) {
                WorldManagerClient.TargetedComMessage comMsg =
                    (WorldManagerClient.TargetedComMessage) msg;
                long oid = comMsg.getSubject();
                if (player.oidIgnored(oid)) {
                	if (Log.loggingDebug)
                		Log.debug("ComHook.processMessage: Ignoring chat from player " + oid + " to player " + player.getOid() +
                				" because originator is in the player's ignored list");
                	return;		
                }
                buf = comMsg.toBuffer();
                Log.info("ProxyPlugin: CHAT_RECV player=" + player +
                    " from=" + comMsg.getSubject() +
                    " private=true" +
                    " msg=[" + comMsg.getString() + "]");
            }
            else
                return;

            ClientConnection con = player.getConnection();
            con.send(buf);
        }
    }

    class DamageHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            CombatClient.DamageMessage dmgMsg =
                (CombatClient.DamageMessage) msg;

            Long attackerOid = dmgMsg.getAttackerOid();
            Long targetOid = dmgMsg.getTargetOid();
            
            // send the client a new object msg
            MVByteBuffer buf = dmgMsg.toBuffer();

            ClientConnection con = player.getConnection();
            if (Log.loggingDebug)
                log.debug("DamageHook: attackerOid= " + attackerOid +
                        ", attacks targetOid=" + targetOid  + " for " +
		        dmgMsg.getDmg() + " damage");
            con.send(buf);
        }
    }

    class SysChatHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            WorldManagerClient.SysChatMessage sysMsg =
                (WorldManagerClient.SysChatMessage) msg;

            MVByteBuffer buf = sysMsg.toBuffer();

            if (Log.loggingDebug)
                log.debug("syschathook:  " + sysMsg.getString());
	    Collection<Player> players =
                new ArrayList<Player>(playerManager.getPlayerCount());
	    playerManager.getPlayers(players);
	    for (Player pp : players) {
		pp.getConnection().send(buf);
	    }
            return true;
        }
    }

    class UpdateWNodeHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.UpdateWorldNodeMessage wMsg =
                (WorldManagerClient.UpdateWorldNodeMessage) msg;
            long subjectOid = wMsg.getSubject();
            if (Log.loggingDebug)
                Log.debug("UpdateWNodeHook.processMessage: subjectOid=" + subjectOid
                          + ", msg=" + msg);

            long playerOid = player.getOid();
            if (playerOid == subjectOid) {
                // dont send an update to the originator
                if (Log.loggingDebug)
                    Log.debug("UpdateWNodeHook.processMessage: subjectOid=" + subjectOid
                              + ", ignoring msg since playerOid matchines subjectOid");
                return;
            }

            player.getConnection().send(wMsg.getEventBuf());
        }
    }

    class UpdateMobPathHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.MobPathMessage pathMsg =
                (WorldManagerClient.MobPathMessage) msg;
            Long subjectOid = pathMsg.getSubject();
            if (Log.loggingDebug)
                log.debug("UpdateMobPathHook.processMessage: subjectOid=" +
                        subjectOid + ", msg=" + msg);
            MVByteBuffer buf = pathMsg.toBuffer();
            ClientConnection con = player.getConnection();
            con.send(buf);
        }
    }
    
    class WNodeCorrectHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.WorldNodeCorrectMessage wMsg =
                (WorldManagerClient.WorldNodeCorrectMessage) msg;

            long oid = wMsg.getSubject();

            if (Log.loggingDebug)
                log.debug("WNodeCorrectHook.processMessage: oid=" + oid +
                    ", msg=" + msg);

            //
            // send the client a dirloc msg
            //
            MVByteBuffer buf = wMsg.toBuffer();
            ClientConnection con = player.getConnection();
            con.send(buf);
        }
    }

    class OrientHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.OrientMessage oMsg =
                (WorldManagerClient.OrientMessage) msg;

            // send the client a new object msg
            MVByteBuffer buf = oMsg.toBuffer();

            ClientConnection con = player.getConnection();
            con.send(buf);
        }
    }

    /**
     * obj is playing a sound or stopped playing a sound
     */
    class SoundHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            WorldManagerClient.SoundMessage sMsg =
                (WorldManagerClient.SoundMessage) msg;

            // Ignore sounds not targeted at player
            long target = sMsg.getTarget();
            if (target != -1 && target != player.getOid())
                return;

            ClientConnection con = player.getConnection();
            con.send(sMsg.toBuffer());
        }
    }

    class InvUpdateHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            InventoryClient.InvUpdateMessage uMsg =
                (InventoryClient.InvUpdateMessage) msg;

            // We only want InvUpdate messages for the player itself
            if (player.getOid() != uMsg.getSubject())
                return;
            ClientConnection con = player.getConnection();
            if (Log.loggingDebug)
                log.debug("InvUpdateHook: sending update to player " +
                        player.getOid() + " msgOid=" + uMsg.getSubject());
            
            con.send(uMsg.toBuffer());
        }
    }

    class FogHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            WorldManagerClient.FogMessage fogMsg =
                (WorldManagerClient.FogMessage) msg;
            FogRegionConfig fogConfig = fogMsg.getFogConfig();
            Long targetOid = fogMsg.getTarget();
            ClientConnection con = player.getConnection();

            WorldManagerClient.FogMessage fogMessage = new WorldManagerClient.FogMessage(0L, fogConfig);
            con.send(fogMessage.toBuffer());
            if (Log.loggingDebug)
                log.debug("FogHook: sending new fog to targetOid " + targetOid + fogConfig);
        }
    }

    /**
     * Handles message from world manager plugin saying that there are new Road
     * objects the player can see.
     * 
     * @author cedeno
     * 
     */
    class RoadHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            WorldManagerClient.RoadMessage roadMsg =
                (WorldManagerClient.RoadMessage) msg;
            Set<Road> roads = roadMsg.getRoads();
            if (Log.loggingDebug)
                log.debug("RoadHook: got " + roads.size() + " roads");
            Long targetOid = roadMsg.getTarget();
            ClientConnection con = player.getConnection();
            List<MVByteBuffer> bufList = roadMsg.toBuffer();
            for (MVByteBuffer buf : bufList) {
                con.send(buf);
            }
            if (Log.loggingDebug)
                log.debug("RoadHook: sent new roads to targetOid " + targetOid);
        }
    }

    class AbilityUpdateHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            CombatClient.AbilityUpdateMessage pMsg =
                (CombatClient.AbilityUpdateMessage) msg;
            if (Log.loggingDebug)
                log.debug("AbilityUpdateHook: got AbilityUpdate message: " + msg);

	    ClientConnection con = player.getConnection();
            con.send(pMsg.toBuffer());
	}
    }

    class GetPluginStatusHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            LinkedHashMap<String,Serializable> status =
                new LinkedHashMap<String,Serializable>();
            status.put("plugin", getName());
            status.put("user", playerManager.getPlayerCount());
            status.put("login", playerManager.getLoginCount());
            status.put("login_sec", playerManager.getLoginSeconds());
            status.put("instance_entry", instanceEntryCount);
            status.put("chat", chatSentCount);
            status.put("private_chat", privateChatSentCount);
            Engine.getAgent().sendObjectResponse(msg,status);
            return true;
        }
    }

    public static class PlayerLoginStatus {
        public PlayerLoginStatus()
        {
        }
        public long oid;
        public int status;
        public String name;
        public String clientCon;
        public String proxyPluginName;

        private static final long serialVersionUID = 1L;
    }

    class GetPlayerLoginStatusHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player) {
            PlayerLoginStatus loginStatus = new PlayerLoginStatus();
            loginStatus.oid = player.getOid();
            loginStatus.status = player.getStatus();
            loginStatus.name = player.getName();
            loginStatus.clientCon = player.getConnection().toString();
            loginStatus.proxyPluginName = getName();
            Engine.getAgent().sendObjectResponse(msg,loginStatus);
        }
    }

    static class InstanceEntryState {
        int step = 1;
        InstanceInfo instanceInfo;
        LinkedList restoreStack;
        BasicWorldNode previousLoc;
    }

    class InstanceEntryReqHook extends BasicProxyHook {
        public void processMessage(Message msg, int flags, Player player)
        {
            InstanceEntryReqMessage entryMessage =
                (InstanceEntryReqMessage) msg;
            
            InstanceEntryState state =
                (InstanceEntryState) entryMessage.getProcessingState();
            if (state == null) {
                state = new InstanceEntryState();
                entryMessage.setProcessingState(state);
            }

            // Instance entry is split into two steps;
            // 1. validation and despawn
            // 2. set new location, send global world settings, and spawn
            // Between steps the entry message is re-enqueued.  This is
            // done so the PerceptionMessage due to despawn can be
            // processed before sending info about the new instance.

            if (state.step == 1) {
                entryStep1(entryMessage, state, player);
            }
            else if (state.step == 2) {
                entryStep2(entryMessage, state, player);
            }
        }

        protected void entryStep1(InstanceEntryReqMessage entryMessage,
            InstanceEntryState state, Player player)
        {
            BasicWorldNode destination = entryMessage.getWorldNode();
            int entryFlags = entryMessage.getFlags();

            String flagStr = "";
            if ((entryFlags & InstanceEntryReqMessage.FLAG_PUSH) != 0)
                flagStr += "push,";
            if ((entryFlags & InstanceEntryReqMessage.FLAG_POP) != 0)
                flagStr += "pop,";

            Log.info("ProxyPlugin: INSTANCE_BEGIN player=" + player +
                " destination=" + destination + " flags=" + flagStr);

            if ((entryFlags & InstanceEntryReqMessage.FLAG_PUSH) != 0 &&
                    (entryFlags & InstanceEntryReqMessage.FLAG_POP) != 0) {
                Log.error("InstanceEntryReqHook: push and pop flags cannot be combined oid="+player.getOid());
                Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                return ;
            }

            if ((entryFlags & InstanceEntryReqMessage.FLAG_PUSH) != 0) {
                if (destination == null) {
                    Log.error("InstanceEntryReqHook: push without destination oid="+player.getOid());
                    Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                    return ;
                }
            }
            if ((entryFlags & InstanceEntryReqMessage.FLAG_POP) != 0) {
                if (destination != null) {
                    Log.error("InstanceEntryReqHook: pop with destination oid="+player.getOid());
                    Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                    return ;
                }
            }

            if (player.getStatus() != Player.STATUS_LOGIN_OK) {
                Log.error("InstanceEntryReqHook: invalid player status "+
                    player);
                Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                return ;
            }

            if ((entryFlags & InstanceEntryReqMessage.FLAG_POP) != 0) {
                // Get the instance restore stack
                LinkedList restoreStack = (LinkedList) EnginePlugin.getObjectProperty(
                    player.getOid(), Namespace.OBJECT_MANAGER,
                    ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK);
                if (restoreStack == null || restoreStack.size() == 0) {
                    Log.error("InstanceEntryReqHook: player has no stack to pop "+player);
                    Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                    return ;
                }
                state.restoreStack = restoreStack;
                InstanceRestorePoint restorePoint = (InstanceRestorePoint)
                    restoreStack.get(restoreStack.size()-1);
                if (restoreStack.size() == 1) {
                    if (restorePoint.getFallbackFlag())
                        Log.warn("InstanceEntryReqHook: popping to fallback restore point "+player);
                    else
                        Log.warn("InstanceEntryReqHook: popping last instance restore point "+player);
                }
                destination = new BasicWorldNode();
                Long instanceOid = restorePoint.getInstanceOid();
                if (restorePoint.getInstanceName() != null)
                    instanceOid = instanceEntryCallback.selectInstance(player,
                        restorePoint.getInstanceName());

                if (instanceOid != null) {
                    destination.setInstanceOid(instanceOid);
                    destination.setLoc(restorePoint.getLoc());
                    destination.setOrientation(restorePoint.getOrientation());
                    destination.setDir(new MVVector(0,0,0));
                }
                entryMessage.setWorldNode(destination);
            }

            if (! instanceEntryAllowed(player.getOid(),
                    destination.getInstanceOid(), destination.getLoc())) {
                Log.info("ProxyPlugin: INSTANCE_REJECT player=" + player +
                    " current=" + state.previousLoc +
                    " destination=" + destination);
                Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                return ;
            }

            state.instanceInfo = InstanceClient.getInstanceInfo(
                destination.getInstanceOid(),
                InstanceClient.FLAG_ALL_INFO);

            if (state.instanceInfo.oid == null) {
                Log.error("InstanceEntryReqHook: unknown instanceOid="+
                    destination.getInstanceOid());
                Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                return ;
            }

            if (Log.loggingDebug)
                Log.debug("InstanceEntryReqHook: instance terrain config: " +
                    state.instanceInfo.terrainConfig);

            TargetedExtensionMessage instanceBegin =
                new TargetedExtensionMessage(player.getOid(), player.getOid());
            instanceBegin.setExtensionType("mv.SCENE_BEGIN");
            instanceBegin.setProperty("action","instance");
            instanceBegin.setProperty(InstanceClient.TEMPL_INSTANCE_NAME,
                state.instanceInfo.name);
            instanceBegin.setProperty(
                InstanceClient.TEMPL_INSTANCE_TEMPLATE_NAME,
                state.instanceInfo.templateName);

            boolean rc;
            rc = WorldManagerClient.despawn(player.getOid(),
                instanceBegin,null);
            if (! rc) {
                Log.error("InstanceEntryReqHook: despawn failed "+
                    player);
                Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                return ;
            }

            // Get player's last location
            state.previousLoc =
                WorldManagerClient.getWorldNode(player.getOid());

            Log.info("ProxyPlugin: INSTANCE_STEP1 player=" + player +
                " current=" + state.previousLoc +
                " destination=" + destination +
                " destName=" + state.instanceInfo.name);

            // Unload player's world manager sub-object.  Required in case
            // the destination instance is in a different world manager.
            ArrayList<Namespace> unloadWM = new ArrayList<Namespace>(1);
            unloadWM.add(WorldManagerClient.NAMESPACE);
            rc = ObjectManagerClient.unloadSubObject(player.getOid(), unloadWM);
            if (! rc) {
                Log.error("InstanceEntryReqHook: unload wm sub-object failed "+
                    player);
                Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                return ;
            }
            state.step = 2;
            messageQQ.insert(player, entryMessage);
        }

        protected void entryStep2(InstanceEntryReqMessage entryMessage,
            InstanceEntryState state, Player player)
        {
            boolean rc;
            int entryFlags = entryMessage.getFlags();
            ClientConnection con = player.getConnection();
            BasicWorldNode destination = entryMessage.getWorldNode();
            BasicWorldNode previousLoc = state.previousLoc;

            BasicWorldNode restoreLoc = null;
            if ((entryFlags & InstanceEntryReqMessage.FLAG_PUSH) != 0) {
                restoreLoc = entryMessage.getRestoreNode();
                if (restoreLoc == null)
                    restoreLoc = previousLoc;
            }

            while (true) {
                rc = ObjectManagerClient.fixWorldNode(player.getOid(),
                    destination);
                if (! rc) {
                    Log.error("InstanceEntryReqHook: fixWorldNode failed "+
                        player + " node="+destination);
                    Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                    return ;
                }

                InstanceInfo instanceInfo = InstanceClient.getInstanceInfo(
                    destination.getInstanceOid(),
                    InstanceClient.FLAG_NAME);

                EnginePlugin.setObjectProperty(
                    player.getOid(), Namespace.OBJECT_MANAGER,
                    ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME,
                    instanceInfo.name);

                // Tell the client about their new location
                WorldManagerClient.WorldNodeCorrectMessage correctMsg;
                correctMsg = new WorldManagerClient.WorldNodeCorrectMessage(
                    player.getOid(), destination);
                con.send(correctMsg.toBuffer());

                if ((entryFlags & InstanceEntryReqMessage.FLAG_PUSH) != 0)
                    pushInstanceRestorePoint(player, restoreLoc);

                con.send(state.instanceInfo.terrainConfig.toBuffer());
                Event skyboxEvent = new SkyboxEvent(state.instanceInfo.skybox);
                con.send(skyboxEvent.toBytes());
                sendOceanData(state.instanceInfo.ocean, player);
                WorldManagerClient.FogMessage fogMessage =
                    new WorldManagerClient.FogMessage(0L, state.instanceInfo.fog);
                con.send(fogMessage.toBuffer());
                List<String> regions = state.instanceInfo.regionConfig;
                for (String region : regions) {
                    Event regionEvent = new RegionConfiguration(region);
                    con.send(regionEvent.toBytes());
                }

                TargetedExtensionMessage instanceEnd =
                    new TargetedExtensionMessage(player.getOid(), player.getOid());
                instanceEnd.setExtensionType("mv.SCENE_END");
                instanceEnd.setProperty("action","instance");

                ArrayList<Namespace> loadWM = new ArrayList<Namespace>(1);
                loadWM.add(WorldManagerClient.NAMESPACE);
                Long oid = ObjectManagerClient.loadSubObject(player.getOid(),
                    loadWM);
                if (oid == null) {
                    Log.error("InstanceEntryReqHook: load wm sub-object failed "+ player);
                    if (previousLoc != null &&
                                destination != previousLoc) {
                        Log.error("InstanceEntryReqHook: attempting to restore previous location "+ player + " previous="+previousLoc);
                        destination = previousLoc;
                        entryFlags &= (~InstanceEntryReqMessage.FLAG_POP);
                        continue;
                    }
                }

                Integer result = WorldManagerClient.spawn(player.getOid(),
                    null, instanceEnd);
                if (result < 0) {
                    Log.error("InstanceEntryReqHook: spawn failed "+ player);
                    if (result == -2 && previousLoc != null &&
                                destination != previousLoc) {
                        Log.error("InstanceEntryReqHook: attempting to restore previous location "+ player + " previous="+previousLoc);
                        destination = previousLoc;
                        entryFlags &= (~InstanceEntryReqMessage.FLAG_POP);
                        continue;
                    }
                    else {
                        Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.FALSE);
                        return ;
                    }
                    //## if there was a push, then pop back to pushed loc
                    //## if there was no push, then?  disconnect? save the
                    //## previous location and return there?
                }

                // Send another correction after loading structures so
                // player can drop on collision volumes.
                WorldManagerClient.correctWorldNode(player.getOid(),
                    destination);

                break;
            }

            Log.info("ProxyPlugin: INSTANCE_END player=" + player +
                " destination=" + destination);

            // "pop" flag: remove top of stack on successful instance entry
            if ((entryFlags & InstanceEntryReqMessage.FLAG_POP) != 0) {
                LinkedList restoreStack = state.restoreStack;
                InstanceRestorePoint top =
                    (InstanceRestorePoint) restoreStack.get(
                        restoreStack.size()-1);
                if (! top.getFallbackFlag()) {
                    restoreStack.remove(restoreStack.size()-1);
                    EnginePlugin.setObjectProperty(
                        player.getOid(), Namespace.OBJECT_MANAGER,
                        ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK,
                        restoreStack);
                }
            }

            instanceEntryCount++;
            Engine.getAgent().sendBooleanResponse(entryMessage, Boolean.TRUE);

        }
    }

    protected void pushInstanceRestorePoint(Player player, BasicWorldNode loc)
    {
        long playerOid = player.getOid();
        InstanceRestorePoint restorePoint = new InstanceRestorePoint();
        restorePoint.setInstanceOid(loc.getInstanceOid());
        restorePoint.setLoc(loc.getLoc());
        restorePoint.setOrientation(loc.getOrientation());

        InstanceInfo instanceInfo = InstanceClient.getInstanceInfo(loc.getInstanceOid(), InstanceClient.FLAG_NAME);
        restorePoint.setInstanceName(instanceInfo.name);

        // Get the instance restore stack
        LinkedList<Object> restoreStack = (LinkedList<Object>) EnginePlugin.getObjectProperty(
            playerOid, Namespace.OBJECT_MANAGER,
            ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK);

        if (restoreStack == null) {
            restoreStack = new LinkedList<Object>();
        }

        restoreStack.add(restorePoint);

        EnginePlugin.setObjectProperty(
            playerOid, Namespace.OBJECT_MANAGER,
            ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK,
            restoreStack);
    }

    protected void sendOceanData(OceanData oceanData, Player player)
    {
        TargetedExtensionMessage oceanMsg = new ClientParameter.ClientParameterMessage(player.getOid());
        oceanMsg.setProperty("Ocean.DisplayOcean", oceanData.displayOcean.toString());
        if (oceanData.useParams != null) {
            oceanMsg.setProperty("Ocean.UseParams", oceanData.useParams.toString());
        }
        if (oceanData.waveHeight != null) {
            oceanMsg.setProperty("Ocean.WaveHeight", oceanData.waveHeight.toString());
        }
        if (oceanData.seaLevel != null) {
            oceanMsg.setProperty("Ocean.SeaLevel", oceanData.seaLevel.toString());
        }
        if (oceanData.bumpScale != null) {
            oceanMsg.setProperty("Ocean.BumpScale", oceanData.bumpScale.toString());
        }
        if (oceanData.bumpSpeedX != null) {
            oceanMsg.setProperty("Ocean.BumpSpeedX", oceanData.bumpSpeedX.toString());
        }
        if (oceanData.bumpSpeedZ != null) {
            oceanMsg.setProperty("Ocean.BumpSpeedZ", oceanData.bumpSpeedZ.toString());
        }
        if (oceanData.textureScaleX != null) {
            oceanMsg.setProperty("Ocean.TextureScaleX", oceanData.textureScaleX.toString());
        }
        if (oceanData.textureScaleZ != null) {
            oceanMsg.setProperty("Ocean.TextureScaleZ", oceanData.textureScaleZ.toString());
        }
        if (oceanData.deepColor != null) {
            oceanMsg.setProperty("Ocean.DeepColor", oceanData.deepColor.toString());
        }
        if (oceanData.shallowColor != null) {
            oceanMsg.setProperty("Ocean.ShallowColor", oceanData.shallowColor.toString());
        }
        player.getConnection().send(oceanMsg.toBuffer(player.getVersion()));
    }

    /**
     * If the ClientConnection associated with the playerOid passed in
     * does not match the ClientConnection argument, throw an error.
     */
    protected Player verifyPlayer(String context, Event event, ClientConnection con) {
        Player player = (Player)con.getAssociation();
        if (player.getOid() != event.getObjectOid()) {
            throw new MVRuntimeException(context +
                ": con doesn't match player " + player);
        }
        return player;
    }

    private class PlayerTimeout implements Runnable
    {
        public void run()
        {
            while (true) {
                try {
                    timeoutPlayers();
                }
                catch (Exception e) {
                    Log.exception("PlayerTimeout", e);
                }
                try {
                    Thread.sleep(10 * 1000);
                }
                catch (InterruptedException e) { /* ignore */ }
            }
        }

        private void timeoutPlayers()
        {
            List<Player> timedoutPlayers =
                playerManager.getTimedoutPlayers(idleTimeout * 1000,
                    silenceTimeout * 1000);
            for (Player player : timedoutPlayers) {
                Log.info("ProxyPlugin: IDLE_TIMEOUT remote=" +
                    player.getConnection() + " player=" + player);
                player.getConnection().close();
            }
        }
    }

    static class AsyncRPCCallback implements ResponseCallback
    {
        AsyncRPCCallback(Player player, String debugPrefix)
        {
            this.player = player;
            this.debugPrefix = debugPrefix;
        }

        public synchronized void handleResponse(ResponseMessage response)
        {
            responders --;
            Log.debug(debugPrefix + ", fromAgent="+
                response.getSenderName() + " playerOid=" + player.getOid());
            if (responders == 0)
                this.notify();
        }

        public synchronized void waitForResponses(int expectedResponses)
        {
            responders += expectedResponses;
            while (responders != 0) {
                try {
                    this.wait();
                } catch (InterruptedException e) {
                }
            }
        }

        Player player;
        String debugPrefix;
        int responders = 0;
    }

    public void incrementChatCount()
    {
        chatSentCount++;
    }

    public void incrementPrivateChatCount()
    {
        privateChatSentCount++;
    }

    protected PerceptionFilter perceptionFilter;
    protected long perceptionSubId;

    protected PerceptionFilter responderFilter;
    protected long responderSubId;

    protected RDPServerSocket serverSocket = null;
    protected int clientPort;

    protected static final Logger log = new Logger("ProxyPlugin");

    PlayerMessageCallback playerMessageCallback = new PlayerMessageCallback();

    protected PlayerManager playerManager = new PlayerManager();

    protected TimeHistogram proxyQueueHistogram = null;
    protected TimeHistogram proxyCallbackHistogram = null;

    protected List<MessageType> extraPlayerMessageTypes = null;

    private ProxyLoginCallback proxyLoginCallback =
        new DefaultProxyLoginCallback();
    private InstanceEntryCallback instanceEntryCallback =
        new DefaultInstanceEntryCallback();

    private int instanceEntryCount = 0;
    private int chatSentCount = 0;
    private int privateChatSentCount = 0;

    public static final MessageType MSG_TYPE_VOICE_PARMS = MessageType.intern("mv.VOICE_PARMS");
    public static final MessageType MSG_TYPE_PLAYER_PATH_REQ = MessageType.intern("mv.PLAYER_PATH_REQ");
    public static final MessageType MSG_TYPE_UPDATE_PLAYER_IGNORE_LIST = MessageType.intern("mv.UPDATE_PLAYER_IGNORE_LIST");
    public static final MessageType MSG_TYPE_GET_MATCHING_PLAYERS = MessageType.intern("mv.GET_MATCHING_PLAYERS");
    public static final MessageType MSG_TYPE_PLAYER_IGNORE_LIST = MessageType.intern("mv.PLAYER_IGNORE_LIST");
    public static final MessageType MSG_TYPE_PLAYER_IGNORE_LIST_REQ = MessageType.intern("mv.PLAYER_IGNORE_LIST_REQ");
    public static final MessageType MSG_TYPE_RELAY_UPDATE_PLAYER_IGNORE_LIST = MessageType.intern("mv.RELAY_UPDATE_PLAYER_IGNORE_LIST");

    public static final MessageType MSG_TYPE_GET_PLAYER_LOGIN_STATUS = MessageType.intern("mv.GET_PLAYER_LOGIN_STATUS");

    
    /**
     * For now, just one voice plugin.  In the future, the proxy will
     * maintain load information for each voice plugin process, and
     * use that load information to decide which process to direct the
     * user to.
     */
    protected static String voiceServerHost = "";
    protected static Integer voiceServerPort = null;

    /**
     * This is a comma-separated list of capabilities that will be
     * sent to the client in the LoginResponseEvent, intended to tell
     * the client what the server can do.  This complements the client
     * capabilities sent to the server in the version field of the
     * initial LoginEvent.
    */
    public String serverCapabilitiesSentToClient = "DirLocOrient";
    
    /**
     * The size of the receive buffer for our server socket
     */
    static int serverSocketReceiveBufferSize = 128 * 1024;
    
    /**
     * The number of concurrent users allowed in the game.  login will
     * fail if there are this many users online.
     */
    public static int MaxConcurrentUsers = 1000;

    /**
     * Player idle timeout in seconds.
     */
    public static int idleTimeout = 60 * 60;

    /**
     * Player idle timeout in seconds.
     */
    public static int silenceTimeout = 1 * 60;

    /**
     * If the client connection has more than this number of messages
     * queued, reset the client connection
     */
    public static int maxMessagesBeforeConnectionReset = 15000;

    /**
     * If the client connection has more than this number of message
     * bytes queued, reset the client connection
     */
    public static int maxByteCountBeforeConnectionReset = 2000000;

    /**
     * The error message issued if too many clients try to log in.
     */
    public String capacityError = "Login Failed: Servers at capacity, please try again later.";
    
    /**
     * The error message issued if there's a secure token error.
     */
    public String tokenError = "Login Failed: Secure token invalid.";

    /**
     * The TCP message I/O instance
     */
    private ClientTCPMessageIO clientTCPMessageIO = null;
    
    /**
     * Adds an oid to the list of players which are allowed to log in
     * even when the server reaches MaxConcurrentUsers
     */
    public void addAdmin(Long oid) {
            if (Log.loggingDebug)
                    log.debug("ProxyPlugin.addAdmin: adding oid " + oid);
            lock.lock();
            try {
                    adminSet.add(oid);
            }
            finally {
                    lock.unlock();
            }
    }
    
    /**
     * Returns a set of admin oids
     */
    public Set<Long> getAdmins() {
            lock.lock();
            try {
                    return new HashSet<Long>(adminSet);
            }
            finally {
                    lock.unlock();
            }
    }
    
    /**
     * Returns true if the oid is in the set of admins; false
     * otherwise.
     */
    public boolean isAdmin(Long oid) {
            lock.lock();
            try {
                    if (oid == null) {
                            return false;
                    }
                    return adminSet.contains(oid);
            }
            finally {
                    lock.unlock();
            }
    }
    
    // oids for admins
    Set<Long> adminSet = new HashSet<Long>();

    // A list of properties that we won't send to the client
    Set<String> filteredProps = null;
    
    // A list of properties that are private to the player that owns
    // them, and should not be sent to any other player
    Set<String> playerSpecificProps = null;

    // A list of properties that are private to the player that owns
    // them, merged with props that filtered for all clients.
    Set<String> cachedPlayerSpecificFilterProps = null;

    // The server version to send to clients
    String serverVersion = null;

    protected Map<String,List<ProxyExtensionHook>> extensionHooks =
        new HashMap<String,List<ProxyExtensionHook>>();

    // JMX stuff

    /** Return JMX MBean instance object.  Over-ride to provide your
        own MBean implementation.
    */
    protected Object createMBeanInstance() {
        return new ProxyJMX();
    }

    public interface ProxyJMXMBean {
        public int getMaxConcurrentUsers();
        public void setMaxConcurrentUsers(int users);
        public int getIdleTimeout();
        public void setIdleTimeout(int timeout);
        public int getSilenceTimeout();
        public void setSilenceTimeout(int timeout);
        public int getCurrentUsers();
        public int getPeakUsers();
        public int getLoginCount();
        public int getLogoutCount();

        public int getClientPort();

        public int getMaxMessagesBeforeConnectionReset();
        public void setMaxMessagesBeforeConnectionReset(int count);
        public int getMaxByteCountBeforeConnectionReset();
        public void setMaxByteCountBeforeConnectionReset(int bytes);

        public String getCapacityErrorMessage();
        public void setCapacityErrorMessage(String errorMessage);
    }

    protected class ProxyJMX implements ProxyJMXMBean {
        protected ProxyJMX() { }

        public int getMaxConcurrentUsers()
        {
            return MaxConcurrentUsers;
        }

        public void setMaxConcurrentUsers(int users)
        {
            if (users >= 0)
                MaxConcurrentUsers = users;
        }

        public int getIdleTimeout()
        {
            return idleTimeout;
        }

        public void setIdleTimeout(int timeout)
        {
            if (timeout > 0)
                idleTimeout = timeout;
        }

        public int getSilenceTimeout()
        {
            return silenceTimeout;
        }

        public void setSilenceTimeout(int timeout)
        {
            if (timeout > 0)
                silenceTimeout = timeout;
        }

        public int getCurrentUsers()
        {
            return playerManager.getPlayerCount();
        }

        public int getPeakUsers()
        {
            return playerManager.getPeakPlayerCount();
        }

        public int getLoginCount()
        {
            return playerManager.getLoginCount();
        }

        public int getLogoutCount()
        {
            return playerManager.getLogoutCount();
        }

        public int getClientPort()
        {
            return clientPort;
        }

        public int getMaxMessagesBeforeConnectionReset()
        {
            return maxMessagesBeforeConnectionReset;
        }

        public void setMaxMessagesBeforeConnectionReset(int count)
        {
            if (count > 0)
                maxMessagesBeforeConnectionReset = count;
        }

        public int getMaxByteCountBeforeConnectionReset()
        {
            return maxByteCountBeforeConnectionReset;
        }

        public void setMaxByteCountBeforeConnectionReset(int bytes)
        {
            if (bytes > 0)
                maxByteCountBeforeConnectionReset = bytes;
        }

        public String getCapacityErrorMessage()
        {
            return capacityError;
        }

        public void setCapacityErrorMessage(String errorMessage)
        {
            if (errorMessage != null)
                capacityError = errorMessage;
        }

    }

    // Periodic forced GC
    class PeriodicGC implements Runnable {
	public void run() {
	    int count= 1;
	    while (true) {
		try {
		    Thread.sleep(60 * 1000);
		} catch (InterruptedException ex) {}
		System.out.println("Proxy running GC "+count);
		System.gc();
		count++;
	    }
	}
    }
}


