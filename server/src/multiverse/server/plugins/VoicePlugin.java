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
import java.io.*;
import java.util.concurrent.locks.*;

import multiverse.server.engine.*;
import multiverse.server.messages.*;
import multiverse.server.objects.*;
import multiverse.server.math.Point;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.server.voice.*;
import multiverse.msgsys.*;
import multiverse.management.Management;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;

/**
 * Plugin to handle voice channels
 */
public class VoicePlugin extends EnginePlugin implements 
           VoiceSender,
           ClientConnection.AcceptCallback,
           ClientConnection.MessageCallback {

    /**
     * The VoicePlugin accepts connections from game clients who have
     * been supplied the IP and port number to contact by the initial
     * game system login.
     *
     * The client initiates the connection, and must immediately
     * thereafter send it's codec parameters.  At that point, the
     * client can send voice frames from the microphone, and the voice
     * plugin forwards those frames to listeners.
     *
     * The process by which a set of listeners to any voice is
     * established is game-dependent.  But the sequence of events is fixed:
     *   - The listener allocates a "voice channel" to receive frames
     *   - The listener initializes the codec parameters for the new 
     *     voice channel to be those associated with the channel.
     *   - As frames are received from clients, they are forwarded 
     *     to listeners.
     *
     * At any point, for each client, there is a (possibly null) set
     * of listeners; this is maintained in the voice group to which
     * the player belongs.
     *
     */

    public VoicePlugin() {
	super("Voice");
        setPluginType("Voice");
        serverVersion = ServerVersion.ServerMajorVersion + " " +
            ServerVersion.getBuildNumber();
        Log.info("VoicePlugin (server version " + serverVersion + ") starting up");
        loginStatusEventReceivers = new HashSet<VoiceConnection>();
        countLogger = new CountLogger("VoiceMsg", 5000, 2, true);
        String log_voice_counters =
                Engine.getProperty("multiverse.log_voice_counters");
        if (log_voice_counters == null || log_voice_counters.equals("false"))
            countLogger.setLogging(false);


        countPacketsReceived = countLogger.addCounter("pkts received");
        countDataFramesReceived = countLogger.addCounter("data frames recvd");
        countAllocateVoiceReceived = countLogger.addCounter("alloc voice recvd");
        countDeallocateVoiceReceived = countLogger.addCounter("dealloc voice recvd");
        countSeqNumGaps = countLogger.addCounter("seqnum gaps");
        countPacketsIgnored = countLogger.addCounter("pkts ignored");

        countPacketsSent = countLogger.addCounter("pkts sent");
        countDataFramesSent = countLogger.addCounter("data frames sent");
        countAllocateVoiceSent = countLogger.addCounter("alloc voice sent");
        countDeallocateVoiceSent = countLogger.addCounter("dealloc voice sent");

        countSendLoginStatus = countLogger.addCounter("login status");

        handleVoiceProperties();

        if (runHistograms) {
            processPacketHistogram = new TimeHistogram("Process Packet");
            dataSendHistogram = new TimeHistogram("Process Data Frames");
            voiceAllocHistogram = new TimeHistogram("Process Allocate");
            voiceDeallocHistogram = new TimeHistogram("Process Deallocate");
        }

        instance = this;
        countLogger.start();
        updater = new Updater();
        Thread updaterThread = new Thread(updater);
        updaterThread.start();
    }
    
    /**
     * A method to encache the voice-specific members of
     * multiverse.properties.
     */
    private void handleVoiceProperties() {
        Long precreatedPositionalOid = parseLongOrNull(Engine.getProperty("multiverse.precreated_positional_voice_group"));
        if (precreatedPositionalOid != null)
            conMgr.addGroup(precreatedPositionalOid, new PositionalVoiceGroup(precreatedPositionalOid, null, this, maxVoiceChannels, audibleRadius, hystericalMargin));
        Long precreatedNonpositionalOid = parseLongOrNull(Engine.getProperty("multiverse.precreated_nonpositional_voice_group"));
        if (precreatedNonpositionalOid != null)
            conMgr.addGroup(precreatedNonpositionalOid, new NonpositionalVoiceGroup(precreatedNonpositionalOid, null, this, maxVoiceChannels));
        Boolean autoCreateGroups = Boolean.valueOf(Engine.getProperty("multiverse.autocreate_referenced_voice_groups"));
        if (autoCreateGroups != null && autoCreateGroups)
            createGroupWhenReferenced = true;
        Boolean voiceBots = Boolean.valueOf(Engine.getProperty("multiverse.voice_bots"));
        if (voiceBots != null && voiceBots)
            allowVoiceBots = true;
        Boolean histograms = Boolean.valueOf(Engine.getProperty("multiverse.voice_packet_histograms"));
        if (histograms != null && histograms)
            runHistograms = true;
        Boolean checkAuthTokenValue = Boolean.valueOf(Engine.getProperty("multiverse.check_auth_token"));
        if (checkAuthTokenValue != null)
            checkAuthToken = checkAuthTokenValue;
    }
    
    private Long parseLongOrNull(String s) {
        try {
            return Long.parseLong(s);
        }
        catch (Exception e) {
            return null;
        }
    }

    /**
     * @return the VoicePlugin instance.
     */
    public static VoicePlugin getInstance() {
        return instance;
    }

    /**
     * Register the hooks and create the subscriptions for the
     * VoicePlugin.
     */
    public void onActivate() {
        voicePort = Integer.parseInt(Engine.getProperty("multiverse.voiceport"));
        String s = Engine.getProperty("multiverse.record_voices");
        if (s != null && s.length() > 3)
            recordVoices = Boolean.parseBoolean(s);
        if (allowVoiceBots)
            getHookManager().addHook(LoginMessage.MSG_TYPE_LOGIN,
                new LoginHook());
        getHookManager().addHook(VoiceClient.MSG_TYPE_VOICECLIENT,
                new VoiceClientMessageHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_INSTANCE_DELETED,
                new InstanceUnloadedHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_INSTANCE_UNLOADED,
                new InstanceUnloadedHook());
        getHookManager().addHook(Management.MSG_TYPE_GET_PLUGIN_STATUS,
                new GetPluginStatusHook());

        getHookManager().addHook(WorldManagerClient.MSG_TYPE_PERCEPTION,
                new PerceptionHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATEWNODE,
                new UpdateWorldNodeHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_SPAWNED,
                new SpawnedHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_DESPAWNED,
                new DespawnedHook());
        getHookManager().addHook(ProxyPlugin.MSG_TYPE_RELAY_UPDATE_PLAYER_IGNORE_LIST,
                new RelayUpdatePlayerIgnoreListHook());

        MessageTypeFilter filter = new MessageTypeFilter();
        if (allowVoiceBots)
            filter.addType(LoginMessage.MSG_TYPE_LOGIN);
        filter.addType(VoiceClient.MSG_TYPE_VOICECLIENT);
        filter.addType(InstanceClient.MSG_TYPE_INSTANCE_DELETED);
        filter.addType(InstanceClient.MSG_TYPE_INSTANCE_UNLOADED);
        filter.addType(Management.MSG_TYPE_GET_PLUGIN_STATUS);
        Engine.getAgent().createSubscription(filter, this,
            MessageAgent.RESPONDER);

        // Set up ClientTCPMessageIO instance to expect 2 bytes of packet length
        clientTCPMessageIO = ClientTCPMessageIO.setup(2, voicePort, this, this);
        clientTCPMessageIO.start("VoiceIO");

        Engine.registerStatusReportingPlugin(this);

        // The perception filter will _not_ handle spawned and
        // despawned messages; they are handled by the popuplation
        // filter
        perceptionFilter = new PerceptionFilter();
        perceptionFilter.addType(WorldManagerClient.MSG_TYPE_PERCEPTION);
        perceptionFilter.addType(WorldManagerClient.MSG_TYPE_UPDATEWNODE);
        perceptionFilter.addType(ProxyPlugin.MSG_TYPE_RELAY_UPDATE_PLAYER_IGNORE_LIST);
        perceptionFilter.setMatchAllSubjects(false);
        List<ObjectType> subjectTypes = new ArrayList<ObjectType>(1);
        subjectTypes.add(ObjectTypes.player);
        perceptionFilter.setSubjectObjectTypes(subjectTypes);
        PerceptionTrigger perceptionTrigger = new PerceptionTrigger();
        perceptionSubId = Engine.getAgent().createSubscription(
            perceptionFilter, this, MessageAgent.NO_FLAGS, perceptionTrigger);

        // Provide a way to get the spawned and despawned messages
        PopulationFilter populationFilter =
                new PopulationFilter(ObjectTypes.player);
        Engine.getAgent().createSubscription(populationFilter, this);
        if (Engine.getInterpolator() == null)
            Engine.setInterpolator(new BasicInterpolator());
    }
    
    /**
     * The callback from ClientTCPIO invoked when a connection is established.
     * @param con The ClientConnection object for the new connection.
     */
    public void acceptConnection(ClientConnection con) {
         con.setAssociation(new VoiceConnection(con));
         Log.info("VoicePlugin: CONNECT remote=" + con.IPAndPort());
    }

    /**
     * This is used exclusively to support voice bots for large-scale
     * tests.
     */
    class LoginHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            LoginMessage message = (LoginMessage) msg;
            Long playerOid = message.getSubject();
            Long instanceOid = message.getInstanceOid();
            Log.debug("LoginHook: playerOid=" + playerOid + " instanceOid=" + instanceOid);
            Engine.getAgent().sendResponse(new ResponseMessage(message));
            sendLoginStatusToReceivers(playerOid, true);
            return true;
        }
    }

    /**
     * Used for both instance unloaded and instance deleted.
     */
    class InstanceUnloadedHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            SubjectMessage message = (SubjectMessage) msg;
            long instanceOid = message.getSubject();
            Set<PositionalVoiceGroup> groups = conMgr.removeInstance(instanceOid);
            if (groups != null) {
                for (PositionalVoiceGroup group : groups)
                    group.unloadInstance(instanceOid);
            }
            Engine.getAgent().sendResponse(new ResponseMessage(message));
            return true;
        }
    }

    /**
     * A hook to process ExtensionMessages whose msgType is
     * VoiceClient.MSG_TYPE_VOICECLIENT.
     */
    class VoiceClientMessageHook implements Hook {
        public boolean processMessage(Message amsg, int flags) {
            WorldManagerClient.ExtensionMessage msg = (WorldManagerClient.ExtensionMessage) amsg;
            String opcode = (String)msg.getProperty("opcode");
            if (Log.loggingDebug)
                Log.debug("VoiceClientMessageHook.processMessage: Received VoiceClient msg for opcode " + opcode);
            // Most but not all messages have a memberOid
            int returnCode = VoiceClient.SUCCESS;
            if (opcode.equals("getPlayerGroup")) {
                Long groupOid = null;
                Long memberOid = (Long)msg.getProperty("memberOid");
                if (memberOid != null) {
                    VoiceGroup group = conMgr.getPlayerGroup(memberOid);
                    if (group != null)
                        groupOid = group.getGroupOid();
                }
                Engine.getAgent().sendResponse(new LongResponseMessage(amsg, groupOid));
                return true;
            }
            Long groupOid = (Long)msg.getProperty("groupOid");
            if (opcode.equals("addVoiceGroup")) {
                if (conMgr.getGroup(groupOid) != null)
                    returnCode = VoiceClient.ERROR_GROUP_ALREADY_EXISTS;
                else {
                    Integer maxVoices = (Integer)msg.getProperty("maxVoices");
                    Boolean positional = (Boolean)msg.getProperty("positional");
                    if (maxVoices == null)
                        returnCode = VoiceClient.ERROR_MISSING_MAX_VOICES;
                    else if (positional == null)
                        returnCode = VoiceClient.ERROR_MISSING_POSITIONAL;
                    else {
                        VoiceGroup group = (positional ?
                            new PositionalVoiceGroup(groupOid, null, VoicePlugin.this, maxVoices, audibleRadius, hystericalMargin) :
                            new NonpositionalVoiceGroup(groupOid, null, VoicePlugin.this, maxVoices));
                        addGroup(groupOid, group);
                    }
                }
            }
            else {
                // All the rest of the opcodes require that the group exists
                VoiceGroup group = conMgr.getGroup(groupOid);
                if (group == null)
                    returnCode = VoiceClient.ERROR_NO_SUCH_GROUP;
                else {
                    if (opcode.equals("removeVoiceGroup"))
                        removeGroup(groupOid);
                    else if (opcode.equals("isPositional"))
                        returnCode = successTrueOrFalse(group.isPositional());
                    else if (opcode.equals("setAllowedMembers"))
                        group.setAllowedMembers((Set<Long>)msg.getProperty("allowedMembers"));
                    else if (opcode.equals("getAllowedMembers")) {
                        Engine.getAgent().sendResponse(new GenericResponseMessage(amsg, group.getAllowedMembers()));
                        return true;
                    }
                    else {
                        // All the remaining opcodes require that memberOid is non-null
                        Long memberOid = (Long)msg.getProperty("memberOid");
                        if (memberOid == null)
                            returnCode = VoiceClient.ERROR_NO_SUCH_MEMBER;
                        else if (opcode.equals("addMember")) {
                            VoiceConnection con = conMgr.getPlayerCon(memberOid);
                            if (con == null)
                                returnCode = VoiceClient.ERROR_PLAYER_NOT_CONNECTED;
                            else
                                group.addMember(memberOid, con, (Integer)msg.getProperty("priority"), (Boolean)msg.getProperty("allowedSpeaker"));
                        }
                        else if (opcode.equals("isMember"))
                            returnCode = successTrueOrFalse(group.isMember(memberOid) != null);
                        else if (opcode.equals("addMemberAllowed")) {
                            returnCode = successTrueOrFalse(group.addMemberAllowed(memberOid));
                        }
                        else {
                            // All other opcodes require that the memberOid is a member of the group
                            if (group.isMember(memberOid) == null)
                                returnCode = VoiceClient.ERROR_NO_SUCH_MEMBER;
                            else if (opcode.equals("removeMember"))
                                group.removeMember(memberOid);
                            else if (opcode.equals("isMemberSpeaking"))
                                returnCode = successTrueOrFalse(group.isMemberSpeaking(memberOid));
                            else if (opcode.equals("isListener"))
                                returnCode = successTrueOrFalse(group.isListener(memberOid));
                            else {
                                // All other opcodes require a boolean "add" property
                                Boolean add = (Boolean)msg.getProperty("add");
                                if (add == null)
                                    returnCode = VoiceClient.ERROR_MISSING_ADD_PROPERTY;
                                else {
                                    if (opcode.equals("setAllowedSpeaker"))
                                        group.setAllowedSpeaker(memberOid, add);
                                    else if (opcode.equals("setMemberSpeaking"))
                                        group.setMemberSpeaking(memberOid, add);
                                    else if (opcode.equals("setListener"))
                                        group.setListener(memberOid, add);
                                    else
                                        returnCode = VoiceClient.ERROR_NO_SUCH_OPCODE;
                                }
                            }
                        }
                    }
                }
            }
            if (Log.loggingDebug)
                Log.debug("VoiceClientMessageHook.processMessage: Response to VoiceClient msg for opcode " + 
                    opcode + " is returnCode " + returnCode);
            Engine.getAgent().sendResponse(new IntegerResponseMessage(amsg, returnCode));
            return true;
        }

        protected int successTrueOrFalse(boolean which) {
            return which ? VoiceClient.SUCCESS_TRUE : VoiceClient.SUCCESS_FALSE;
        }
    }

    /**
     * PerceptionHook processes PerceptionMessages, which incorporate lists
     * of perceived objects gained and lost by the target object.
     */
    class PerceptionHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            PerceptionMessage perceptionMessage = (PerceptionMessage)msg;
            long perceiverOid = perceptionMessage.getTarget();
            PositionalGroupMember perceiverMember = conMgr.getPositionalMember(perceiverOid);
            if (perceiverMember != null) {
                List<PerceptionMessage.ObjectNote> gain = perceptionMessage.getGainObjects();
                List<PerceptionMessage.ObjectNote> lost = perceptionMessage.getLostObjects();
                if (Log.loggingDebug)
                    Log.debug("PerceptionHook.processMessage: perceiverOid " + perceiverOid + ", instanceOid=" + perceiverMember.getInstanceOid() + " " +
                        ((gain==null)?0:gain.size()) + " gain and " +
                        ((lost==null)?0:lost.size()) + " lost");

                if (gain != null) {
                    for (PerceptionMessage.ObjectNote note : gain)
                        processNote(perceiverOid, perceiverMember, note, true);
                }
                if (lost != null) {
                    for (PerceptionMessage.ObjectNote note : lost)
                        processNote(perceiverOid, perceiverMember, note, false);
                }
            }
            else if (Log.loggingDebug)
                Log.debug("PerceptionHook.processMessage: Could not find PositionalGroupMember for player " + perceiverOid);
            return true;
        }
        
        protected void processNote(long perceiverOid, PositionalGroupMember perceiverMember, PerceptionMessage.ObjectNote note, boolean add) {
            long perceivedOid = note.getSubject();
            PositionalVoiceGroup group = (PositionalVoiceGroup)conMgr.getPlayerGroup(perceiverOid);
            ObjectType objType = (ObjectType) note.getObjectType();
            if (objType.isPlayer()) {
                group.maybeChangePerceivedObject(perceiverMember, perceivedOid, add);
            }
        }
    }

    /**
     * UpdateWorldNodeHook asks the conneciton manager if the message
     * subject oid is a positional group member, and if it is, and it
     * has an InterpolatedWorldNode, it calls the group's
     * updateWorldNode method.
     */
    class UpdateWorldNodeHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.UpdateWorldNodeMessage wnodeMsg = (WorldManagerClient.UpdateWorldNodeMessage)msg;
	    long playerOid = wnodeMsg.getSubject();
            PositionalGroupMember perceiverMember = conMgr.getPositionalMember(playerOid);
            if (perceiverMember != null && perceiverMember.wnode != null) {
                BasicWorldNode bwnode = wnodeMsg.getWorldNode();
                if (Log.loggingDebug)
                    Log.debug("VoicePlugin.handleMessage: UpdateWnode for " + playerOid + ", loc " + bwnode.getLoc() + ", dir " + bwnode.getDir());
                PositionalVoiceGroup group = (PositionalVoiceGroup)perceiverMember.getGroup();
                group.updateWorldNode(perceiverMember, bwnode);
            }
            return true;
        }
    }

    /**
     * SpawnedHook asks the conneciton manager if the message subject
     * oid is a positional group member, and if it is, it calls
     * trackNewPerceiver to add it to the set of members whose
     * perceived oids are tracked.
     */
    class SpawnedHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.SpawnedMessage spawnedMsg = (WorldManagerClient.SpawnedMessage)msg;
            long playerOid = spawnedMsg.getSubject();
            long instanceOid = spawnedMsg.getInstanceOid();
            PositionalGroupMember member = conMgr.getPositionalMember(playerOid);
            if (member != null && member.wnode == null) {
                if (Log.loggingDebug)
                    Log.debug("SpawnedHook.processMessage: playerOid " + playerOid + " spawned, instanceOid " + instanceOid);
                trackNewPerceiver(member);
            }
            else
                if (Log.loggingDebug)
                    Log.debug("SpawnedHook.processMessage: playerOid " + playerOid + " spawn ignored, instanceOid " + instanceOid);
            return true;
        }
    }
    
    /**
     * DespawnedHook asks the conneciton manager if the message
     * subject oid is a positional group member, and if it is, and it
     * has an InterpolatedWorldNode, it calls the group's
     * removeTrackedPerceiver method, and also removes the subject oid
     * from the perception filter.
     */
    class DespawnedHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.DespawnedMessage despawnedMsg = (WorldManagerClient.DespawnedMessage)msg;
            Long playerOid = despawnedMsg.getSubject();
            PositionalGroupMember member = conMgr.getPositionalMember(playerOid);
            if (member != null && member.wnode != null) {
                if (Log.loggingDebug)
                    Log.debug("DespawnedHook.processMessage: Despawn for " + playerOid + ", instanceOid " + member.getInstanceOid());
                PositionalVoiceGroup group = (PositionalVoiceGroup)member.getGroup();
                group.removeTrackedPerceiver(member);
                removeFromPerceptionFilter(playerOid);
            }
            else if (Log.loggingDebug)
                Log.debug("DespawnedHook.processMessage: Ignored despawn for player " +  playerOid + 
                    " because " + (member != null ? "member was null" : "member.wnode wasn't null"));
            return true;
        }
    }

    class GetPluginStatusHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            LinkedHashMap<String,Serializable> status =
                new LinkedHashMap<String,Serializable>();
            status.put("plugin", getName());
            status.put("voice_user", conMgr.getPlayerCount());
            status.put("voice_alloc", countAllocateVoiceReceived.getCount());
            status.put("voice_frame", countDataFramesReceived.getCount());
            Engine.getAgent().sendObjectResponse(msg,status);
            return true;
        }
    }

    /**
     * Used only when running voice bots.
     */
    protected void sendLoginStatusToReceivers(long playerOid, boolean login) {
        for (VoiceConnection playerCon : loginStatusEventReceivers)
            sendLoginStatus(playerCon, playerOid, login);
    }

    /**
     * Get the GroupMember associated with the playerOid, or null if
     * there is none.
     * @param playerOid The oid of the member returned.
     * @return The member with the given playerOid.
     */
    protected GroupMember getPlayerMember(long playerOid) {
        VoiceConnection con = conMgr.getPlayerCon(playerOid);
        if (con == null || con.group == null) {
            Log.error("VoicePlugin.getPlayerMember: For " + playerOid + ", con " + con + " group field is null!");
            return null;
        }
        else
            return con.groupMember;
    }
    
    /**
     * Process a packet received from a player.
     * @param con The VoiceConnection object of the player sending the
     * packet.
     * @param buf The buffer containing the packet.  The buf limit is
     * the size of the packet.
     */
    public void processPacket(ClientConnection con, MVByteBuffer buf) {
        byte opcode = (byte)0;
        long startTime = System.nanoTime();
        lock.lock();
        try {
            countPacketsReceived.add();
            short seqNum = buf.getShort();
            opcode = buf.getByte();
            byte micVoiceNumber = buf.getByte();
            VoiceConnection speaker = getConnectionData(con);
            if (speaker == null) {
                Log.error("VoicePlugin.processPacket: Could not find VoiceConnection for con " + con +
                    ", micVoiceNumber " + micVoiceNumber + ", opcode " + opcodeString(opcode));
                return;
            }
            if (incSeqNum(speaker.seqNum) != seqNum)
                countSeqNumGaps.add();
            speaker.seqNum = seqNum;
            // Ignore all packets until we set the authentication token
            if (speaker.authToken == null) {
                if (opcode != opcodeAuthenticate) {
                    countPacketsIgnored.add();
                    Log.error("VoicePlugin.processPacket: Have not yet received authentication token, but packet opcode is " + opcodeString(opcode));
                }
                else
                    processAuthenticationPacket(speaker, buf);
            }
            else {
                speaker.micVoiceNumber = micVoiceNumber;
                long oid = speaker.playerOid;
                if (Log.loggingNet)
                    Log.net("VoicePlugin.processPacket: micVoiceNumber " + micVoiceNumber + ", opcode " + opcodeString(opcode) + ", seqNum " + seqNum + 
                        ", oid " + oid);
                else if (Log.loggingDebug && (opcode != opcodeData && opcode != opcodeAggregatedData))
                    Log.debug("VoicePlugin.processPacket: micVoiceNumber " + micVoiceNumber + ", opcode " + opcodeString(opcode) + ", seqNum " + seqNum + 
                        ", oid " + oid);
                VoiceGroup group = speaker.group;
                if (group == null) {
                    Log.error("VoicePlugin.processPacket: For speaker " + oid + ", connection " + con + ", group is null");
                    return;
                }
                int dataSize;
                switch (opcode) {
                case opcodeAuthenticate:
                    processAuthenticationPacket(speaker, buf);
                    break;
                case opcodeAllocateCodec:
                    if (recordVoices && speaker.recordSpeexStream == null)
                        speaker.recordSpeexStream = openSpeexVoiceFile(oid);
                    countAllocateVoiceReceived.add();
                    group.setMemberSpeaking(oid, true);
                    break;
                case opcodeAllocatePositionalCodec:
                    Log.error("VoicePlugin.processPacket: shouldn't get AllocatePositionalCodec message!");
                    break;
                case opcodeReconfirmCodec:
                    group.setMemberSpeaking(oid, true);
                    break;
                case opcodeDeallocate:
                    if (recordVoices && speaker.recordSpeexStream == null) {
                        try {
                            speaker.recordSpeexStream.close();
                        }
                        catch (IOException e) {
                            Log.exception("Error closing Speex stream for voice " + speaker.playerOid, e);
                        }
                        speaker.recordSpeexStream = null;
                    }
                    countDeallocateVoiceReceived.add();
                    group.setMemberSpeaking(oid, false);
                    break;
                case opcodeData:
                    dataSize = buf.limit();
                    if (speaker.recordSpeexStream != null)
                        writeSpeexData(speaker.recordSpeexStream, buf.array(), voicePacketHeaderSize, dataSize - voicePacketHeaderSize);
                    countDataFramesReceived.add();
                    if (group.isMemberSpeaking(speaker.playerOid))
                    group.sendVoiceFrameToListeners(oid, buf, opcodeData, dataSize);
                    else
                        Log.error("VoicePlugin.processPacket: got data pkt for speaker " + speaker.playerOid + ", but speaker.currentSpeaker false.  micVoiceNumber " +
                            micVoiceNumber + ", opcode " + opcodeString(opcode) + ", seqNum " + seqNum + DebugUtils.byteArrayToHexString(buf));
                    break;
                case opcodeAggregatedData:
                    dataSize = buf.limit();
                    byte dataFrameCount = buf.getByte();
                    if (speaker.recordSpeexStream != null) {
                        byte[] array = buf.array();
                        int currentIndex = voicePacketHeaderSize + 1;
                        for (int i=0; i<dataFrameCount; i++) {
                            byte frameLength = array[currentIndex];
                            currentIndex += 1;
                            writeSpeexData(speaker.recordSpeexStream, array, currentIndex, frameLength);
                            currentIndex += frameLength;
                        }
                        if (currentIndex != dataSize)
                            Log.error("VoicePlugin.processPacket: While recording agg data packet: currentIndex " + 
                                    currentIndex + " != dataSize " + dataSize);
                    }
    //                 if (Log.loggingDebug)
    //                     Log.debug("VoicePlugin.processPacket: processing agg data message with " + 
    //                         dataFrameCount + " voice frames: " + DebugUtils.byteArrayToHexString(buf) + ", dataSize " + dataSize);
                    if (group.isMemberSpeaking(speaker.playerOid))
                        group.sendVoiceFrameToListeners(oid, buf, opcodeAggregatedData, dataSize);
                    else
                        Log.error("VoicePlugin.processPacket: got data pkt for speaker " + speaker.playerOid + ", but speaker.currentSpeaker false.  micVoiceNumber " +
                            micVoiceNumber + ", opcode " + opcodeString(opcode) + ", seqNum " + seqNum + DebugUtils.byteArrayToHexString(buf));
                    countDataFramesReceived.add(dataFrameCount);
                    speaker.seqNum = incSeqNum(speaker.seqNum, dataFrameCount - 1);
                    break;
                case opcodeChangeIgnoredStatus:
                    processIgnoreListChangeMessage(speaker, buf);
                    break;
                }
            }
        }
        catch (Exception e) {
            Log.exception("VoicePlugin.processPacket: For packet " + DebugUtils.byteArrayToHexString(buf), e);
        }
        finally {
            lock.unlock();
        }
        if (runHistograms) {
            long packetTime = System.nanoTime() - startTime;
            switch (opcode) {
            case opcodeAllocateCodec:
                voiceAllocHistogram.addTime(packetTime);
                break;
            case opcodeDeallocate:
                voiceDeallocHistogram.addTime(packetTime);
                break;
            case opcodeAggregatedData:
            case opcodeData:
                dataSendHistogram.addTime(packetTime);
                break;
            }
            processPacketHistogram.addTime(packetTime);
        }
    }

    /**
     * Write a single Speex frame to the stream, from the byte array
     * @param recordSpeexStream The stream to which the voice frame(s) should be written.
     * @param buf The buffer containing the voice frame(s).
     * @param startIndex The index in the buf of the first byte to be written.
     * @param byteCount The number of bytes of frame data to be written.
     */
    protected void writeSpeexData(BufferedOutputStream recordSpeexStream, byte[] buf, int startIndex, int byteCount) {
        try {
            // Write just the speex frame portion of the packet
            recordSpeexStream.write(buf, startIndex, byteCount);
        }
        catch (IOException e) {
            Log.exception("VoicePlugin.writeVoiceData: Exception writing voice data", e);
        }
    }
    

    /**
     * Check to see of the auth packet contains the proper
     * credentials, and if so, create a group member in the specified
     * group.
     * @param playerCon The VoiceConnection object for the player that
     * sent the auth packet.
     * @param buf The buffer containing the auth packet.
     */
    public void processAuthenticationPacket(VoiceConnection playerCon, MVByteBuffer buf) {
        long playerOid = buf.getLong();
        long groupOid = buf.getLong();
        String encodedToken = buf.getString();
        byte listenToYourselfByte = buf.getByte();
        boolean listenToYourself = listenToYourselfByte != 0;
        if (Log.loggingDebug)
            Log.debug("VoicePlugin.processAuthenticationPacket: Received auth packet; playerOid " + playerOid + 
                      ", groupOid " + groupOid + ", authToken " + encodedToken +
                      ", listenToYourself " + listenToYourself);

        // If there is an existing connection different from this one,
        // that means the previous one didn't get closed properly, so
        // close it now.
        VoiceConnection previousPlayerCon = conMgr.getPlayerCon(playerOid);
        if (previousPlayerCon != null && previousPlayerCon != playerCon)
            expungeVoiceClient(playerOid, previousPlayerCon);

        // If the player is currently associated with a group, null
        // that out.
        if (playerCon.group != null)
            removePlayerFromGroup(playerCon);

        SecureToken authToken = SecureTokenManager.getInstance().importToken(Base64.decode(encodedToken));
        if (checkAuthToken && ((authToken.getValid() != true) || ((Long)authToken.getProperty("player_oid") != playerOid))) {
            Log.error("VoicePlugin.processAuthenticationPacket: token rejected for playerOid=" +
                      playerOid + " token=" + authToken);
            playerCon.con.close();
            return;
        }
        // This is the special playerOid/groupOid pair, only enabled
        // when voice bots are allowed, that causes the this
        // connection to be recorded as one to which we'll send login
        // and logout events.
        if (allowVoiceBots && playerOid == -1L && groupOid == 0)
            loginStatusEventReceivers.add(playerCon);
        else {
            // The normal case
            VoiceGroup group = findVoiceGroup(groupOid);
            if (group != null) {
                if (group.addMemberAllowed(playerOid)) {
                    playerCon.groupOid = groupOid;
                    playerCon.group = group;
                    playerCon.authToken = authToken;
                    playerCon.playerOid = playerOid;
                    playerCon.listenToYourself = listenToYourself;
                    conMgr.setPlayerCon(playerOid, playerCon);
                    playerCon.groupMember = group.addMember(playerOid, playerCon);
                    initializeIgnoreList(playerCon);
                    conMgr.setPlayerGroup(playerOid, group);
                    group.setListener(playerOid, true);
                    if (group.isPositional()) {
                        PositionalGroupMember member = (PositionalGroupMember)playerCon.groupMember;
                        if (member.wnode == null)
                            trackNewPerceiver(member);
                    }
                    Log.info("VoicePlugin: VOICE_AUTH remote=" +
                        playerCon.con.IPAndPort() + " playerOid=" + playerOid +
                        " groupOid=" + groupOid);
                }
                else
                    Log.error("VoicePlugin.processAuthenticationPacket: Player " + playerOid + " with authToken '" + encodedToken + "' was denied access to group " + groupOid + " - - auth packet ignored!");
            }
            else
                Log.error("VoicePlugin.processAuthenticationPacket: Could not find group " + groupOid + " for playerOid " + playerOid + 
                    " - - auth packet ignored!"); 
        }
    }

    protected void initializeIgnoreList(VoiceConnection playerCon) {
        long playerOid = playerCon.playerOid;
        TargetMessage msg = new TargetMessage(ProxyPlugin.MSG_TYPE_PLAYER_IGNORE_LIST_REQ, playerOid, playerOid);
        try {
            List<Long> ignoreList = (List<Long>)Engine.getAgent().sendRPCReturnObject(msg);
            playerCon.groupMember.initializeIgnoredSpeakers(ignoreList);
        }
        catch (Exception NoRecipientsException) {
            Log.error("VoicePlugin.initializeIgnoreList: Could not retrieve ignore list for player " + playerCon.playerOid);
            playerCon.groupMember.initializeIgnoredSpeakers(new LinkedList<Long>());
        }
    }

    /**
     * Internal method to add the positional group member to the set
     * of member whose perceived players will be tracked.
     * @param member The positional group member to be tracked.
     */
    protected void trackNewPerceiver(PositionalGroupMember member) {
        long memberOid = member.getMemberOid();
        WorldManagerClient.ObjectInfo info = WorldManagerClient.getObjectInfo(memberOid);
        member.wnode = new InterpolatedWorldNode(info);
        PositionalVoiceGroup pGroup = (PositionalVoiceGroup)member.getGroup();
        pGroup.addTrackedPerceiver(member, info.instanceOid);
        addToPerceptionFilter(memberOid);
    }
    
    /**
     * A hook to handle update ignore list message contents
     * originating with the client, and passed on by the proxy.
     */
    class RelayUpdatePlayerIgnoreListHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage extMsg = (ExtensionMessage)msg;
            Long playerOid = extMsg.getSubject();
            VoiceConnection playerCon = conMgr.getPlayerCon(playerOid);
            if (playerCon == null)
                return true;
            GroupMember member = playerCon.groupMember;
            member.applyIgnoreUpdateMessage(extMsg);
            return true;
        }
    }
    
    /**
     * Change the ignored status of a set of potential speakers.  Note
     * that this is only called when processing an ignore request from
     * a voice client, and that mechanism is now deprecated.
     * @param playerCon The VoiceConnection object for the player that
     * sent the blacklist packet.
     * @param buf The buffer containing the blacklist message.
     */
    protected void processIgnoreListChangeMessage(VoiceConnection playerCon, MVByteBuffer buf) {
        GroupMember member = playerCon.group.isMember(playerCon.playerOid);
        if (member == null) {
            Log.error("VoicePlugin.processBlacklistChangeMessage: player " + playerCon.playerOid + " is not associated with a group member!");
            return;
        }
        int count = buf.getShort();
        List<Long> addToIgnored = new LinkedList<Long>();
        List<Long> removeFromIgnored = new LinkedList<Long>();
        for (int i=0; i<count; i++) {
            byte which = buf.getByte();
            long oid = buf.getLong();
            if (which != 0)
                addToIgnored.add(oid);
            else
                removeFromIgnored.add(oid);
        }
        member.addIgnoredSpeakerOids(addToIgnored);
        member.removeIgnoredSpeakerOids(removeFromIgnored);
    }
    
    /**
     * Internal method that adds the playerOid to the list of oids for
     * which the perception filter will send perception messages.
     * @param playerOid The oid of the player to be added to the
     * perception filter.
     */
    protected void addToPerceptionFilter(long playerOid) {
        if (Log.loggingDebug)
            Log.debug("VoicePlugin.addToPerceptionFilter: Adding playerOid " + playerOid);
        if (perceptionFilter.addTarget(playerOid)) {
            FilterUpdate filterUpdate = new FilterUpdate(1);
            filterUpdate.addFieldValue(PerceptionFilter.FIELD_TARGETS,
                new Long(playerOid));
            Engine.getAgent().applyFilterUpdate(perceptionSubId,
                filterUpdate);
        }
        else if (Log.loggingDebug)
            Log.debug("VoicePlugin.addToPerceptionFilter: PlayerOid " + playerOid + " was already in the filter");

    }        

    /**
     * Internal method that removes the playerOid from the list of oids for
     * which the perception filter will send perception messages.
     * @param playerOid The oid of the player to be removed to the
     * perception filter.
     */
    protected void removeFromPerceptionFilter(long playerOid) {
        if (perceptionFilter.hasTarget(playerOid)) {
            perceptionFilter.removeTarget(playerOid);
            FilterUpdate filterUpdate = new FilterUpdate(1);
            filterUpdate.removeFieldValue(PerceptionFilter.FIELD_TARGETS,
                new Long(playerOid));
            Engine.getAgent().applyFilterUpdate(perceptionSubId,
                filterUpdate);
        }
        else if (Log.loggingDebug)
            Log.debug("VoicePlugin.removeFromPerceptionFilter: PlayerOid " + playerOid + " was not in the filter");
    }
    
    /**
     * Remove the player identified by the argument from the group it belongs to.
     * @param playerCon The VoiceConnection object corresponding to the player.
     */
    public void removePlayerFromGroup(VoiceConnection playerCon) {
        if (playerCon.group == null)
            Log.error("VoicePlugin.removePlayerFromGroup: playerCon " + playerCon + " group is null!");
        else {
            playerCon.group.removeMember(playerCon.playerOid);
            playerCon.group = null;
            playerCon.groupOid = 0;
            playerCon.authToken = null;
        }
    }

    /** 
     * Return the group for groupOid, having checked to see that the
     * group exists and contains the player.
     * @param groupOid The oid of the group to be returned.
     */
    protected VoiceGroup findVoiceGroup(long groupOid) { 
        VoiceGroup group = conMgr.getGroup(groupOid);
        if (group == null) {
            if (createGroupWhenReferenced) {
                group = new NonpositionalVoiceGroup(groupOid, null, this, maxVoiceChannels);
                conMgr.addGroup(groupOid, group);
            }
            else {
                return null;
            }
        }
        return group;
    }

    /**
     * Inform the VoicePlugin about the existance of a voice group.
     * @param groupOid The oid of the voice group to be added.
     * @param group The voice group to be added.
     */
    public void addGroup(long groupOid, VoiceGroup group) {
        conMgr.addGroup(groupOid, group);
    }

    /**
     * Remove a group.
     * @param groupOid The oid of the voice group to be removed.
     */
    public void removeGroup(long groupOid) {
        List<Long> playerOids = conMgr.groupPlayers(groupOid);
        for (long playerOid : playerOids)
            expungeVoiceClient(playerOid);
    }

    /**
     * Send a packet, whose contents is in the buf, to the listener.
     * @param listener The VoiceConnection object for the listener.
     * @param buf The buffer containing the packet to be sent.
     */
    private void sendPacketToListener(VoiceConnection listener, MVByteBuffer buf) {
        countPacketsSent.add();
        listener.con.send(buf);
    }

    /**
     * Send an allocate voice packet to the listener, representing a
     * voice channel from the speaker to the listener.  This method is
     * a required member of the VoiceSender interface.
     * @param speaker The VoiceConnection object for the speaker.
     * @param listener The VoiceConnection object for the listener.
     * @param voiceNumber The listener-specific number of the voice
     * channel that is created by this allocation.
     * @param positional If true, this voice channel is positional; if
     * false, non-positional.
     */
    public void sendAllocateVoice(VoiceConnection speaker, VoiceConnection listener, byte voiceNumber, boolean positional) {
        sendAllocateVoice(speaker, listener, voiceNumber, (positional ? opcodeAllocatePositionalCodec : opcodeAllocateCodec));
    }

    /**
     * Send an allocate voice packet to the listener, representing a
     * voice channel from the speaker to the listener.  This method is
     * a required member of the VoiceSender interface.
     * @param speaker The VoiceConnection object for the speaker.
     * @param listener The VoiceConnection object for the listener.
     * @param voiceNumber The listener-specific number of the voice
     * channel that is created by this allocation.
     * @param opcode The allocate opcode of the packet to be sent;
     * there are three kinds of allocate packets, each with different
     * opcodes, but at this time, only opcodeAllocateCodec is ever
     * sent.
     */
    public void sendAllocateVoice(VoiceConnection speaker, VoiceConnection listener, byte voiceNumber, byte opcode) {
        short msgSize = (short)voiceMsgSize[opcode];
        MVByteBuffer buf = new MVByteBuffer(msgSize);
        buf.putShort(speaker.seqNum);
        buf.putByte(opcode);
        buf.putByte(voiceNumber);
        buf.putLong(speaker.playerOid);
        if (Log.loggingDebug)
            Log.debug("VoicePlugin.sendAllocateVoice: speaker " + speaker + ", listener " + listener + 
                ", voiceNumber " + voiceNumber + ", opcode " + opcodeString(opcode) + ", seqNum " + 
                speaker.seqNum + ", oid " + speaker.playerOid + " " + DebugUtils.byteArrayToHexString(buf));
        countAllocateVoiceSent.add();
        sendPacketToListener(listener, buf);
    }
    
    /**
     * Send a message to the connection deallocating the voice with
     * the given number.
     * @param speaker The VoiceConnection object for the speaker.
     * @param listener The VoiceConnection object for the listener.
     * @param voiceNumber The listener-specific number of the voice
     * channel that is removed by this deallocation.
     */
    public void sendDeallocateVoice(VoiceConnection speaker, VoiceConnection listener, byte voiceNumber) {
        MVByteBuffer buf = new MVByteBuffer(voicePacketHeaderSize);
        buf.putShort(speaker.seqNum);
        buf.putByte(opcodeDeallocate);
        buf.putByte(voiceNumber);
        if (Log.loggingDebug)
            Log.debug("VoicePlugin.sendDeallocateVoice: speaker " + speaker + ", listener " + listener + 
                ", voiceNumber " + voiceNumber + ", seqNum " + speaker.seqNum + ", oid " + speaker.playerOid + 
                " " + DebugUtils.byteArrayToHexString(buf));
        countDeallocateVoiceSent.add();
        sendPacketToListener(listener, buf);
    }
    
    /**
     * Send a message containing voice frame(s) to the listener.
     * @param speaker The VoiceConnection object for the speaker.
     * @param listener The VoiceConnection object for the listener.
     * @param opcode The opcode of the voice frame(s) packet.  There
     * are two possible opcodes: opcodeData, which sends a single
     * frame, and opcodeAggregatedData, which sends multiple voice
     * frames.
     * @param voiceNumber The listener-specific number of the voice
     * channel that is removed by this deallocation.
     * @param sourceBuf The buffer containing the voice frame(s).
     * @param pktLength The number of bytes in the buffer.
     */
    public void sendVoiceFrame(VoiceConnection speaker, VoiceConnection listener, byte opcode, byte voiceNumber, MVByteBuffer sourceBuf, short pktLength) {
        if (Log.loggingNet)
            Log.net("VoicePlugin.sendVoiceFrame: length " + pktLength + ", speaker " + speaker + 
                ", listener " + listener + ", packet " + DebugUtils.byteArrayToHexString(sourceBuf));
        MVByteBuffer buf = new MVByteBuffer(pktLength);
        buf.putShort(speaker.seqNum);
        buf.putByte(opcode);
        buf.putByte(voiceNumber);
        buf.putBytes(sourceBuf.array(), voicePacketHeaderSize, pktLength - voicePacketHeaderSize);
        countDataFramesSent.add();
        sendPacketToListener(listener, buf);
    }

    /**
     * Send a login status message, telling the receiver that a player
     * has logged in.  This is used only for voice bots.
     * @param receiver The VoiceConnection object for the voice bot
     * manager to receive the packet.
     * @param playerOid The oid of the player whose login status
     * changed.
     * @param login If true, the playerOid logged in; if false, the
     * playerOid logged out.
     */
    public void sendLoginStatus(VoiceConnection receiver, long playerOid, boolean login) {
        countSendLoginStatus.add();
        short msgSize = (short)voiceMsgSize[opcodeLoginStatus];
        MVByteBuffer buf = new MVByteBuffer(msgSize);
        loginStatusSeqNum = incSeqNum(loginStatusSeqNum);
        buf.putShort(loginStatusSeqNum);
        buf.putByte(opcodeLoginStatus);
        buf.putByte((byte)(login ? 1 : 0));
        buf.putLong(playerOid);
        if (Log.loggingDebug)
            Log.debug("VoicePlugin.sendLoginStatus: receiver " + receiver + ", opcode " + opcodeString(opcodeLoginStatus) + 
                ", seqNum " + loginStatusSeqNum + " " + DebugUtils.byteArrayToHexString(buf));
        receiver.con.send(buf);
    }

    /**
     * Send an ExtensionMessage.
     * @param msg The ExtensionMessage to be sent.
     */
    public void sendExtensionMessage(WorldManagerClient.ExtensionMessage msg) {
        Engine.getAgent().sendBroadcast(msg);
    }
    
    /**
     * Internal method used when writing Speex voice files.  This
     * is used only for testing.
     * @param oid The oid used to look up the voice file.
     * @return The stream object representing the opened and writable
     * file.
     */
    protected BufferedOutputStream openSpeexVoiceFile(long oid) {
        try {
        FileOutputStream fileOutputStream = new FileOutputStream("Voice-" + oid + ".speex");
        BufferedOutputStream bufferedStream = new BufferedOutputStream(fileOutputStream);
        return bufferedStream;
        }
        catch (Exception e) {
            Log.exception("VoicePlugin.openSpeexVoiceFile: Exception opening file for oid " + oid, e);
            return null;
        }
    }

    /**
     * Get the VoiceConnection object associated with the ClientConnection.
     * con The ClientConnection object.
     * @param con The ClientConnection object whose VoiceConnection should be returned.
     * @return The VoiceConnection object associated with the ClientConnection.
     */
    public VoiceConnection getConnectionData(ClientConnection con) {
        VoiceConnection data = (VoiceConnection)con.getAssociation();
        if (data == null) {
            String s = "getConnectionData: Could not find connection " + formatCon(con);
            Log.dumpStack(s);
            return null;
        }
        else
            return data;
    }
    
    /**
     * Produce a string description of the ClientConnection object.
     * @param con The ClientConnection object whose description is to be returned.
     * @return The string description of the ClientConnection object.
     */
    public String formatCon(ClientConnection con) {
        return con.toString();
    }
    
    /**
     * In response to a ClientConnection being closed, deallocate all
     * voices in use by the connection, and remove the listeners, and
     * close record stream if it's open.
     * @param con The ClientConnection object that was closed.
     */
    public void connectionReset(ClientConnection con) {
        VoiceConnection data = getConnectionData(con);
        if (data == null)
            Log.error("VoicePlugin.connectionReset: Could not find connection " + con);
        else {
            long playerOid = data.playerOid;
            Log.info("VoicePlugin: DISCONNECT remote=" + data.con.IPAndPort() + " playerOid=" + playerOid);
            if (data.recordSpeexStream != null) {
                try {
                    data.recordSpeexStream.close();
                }
                catch (IOException e) {
                    Log.exception("VoicePlugin.connectionReset: Exception closing record stream", e);
                }
                data.recordSpeexStream = null;
            }
            expungeVoiceClient(playerOid, data);
        }
    }

    /**
     * Remove the voice client associated with the playerOid from the
     * VoicePlugin data structures.
     * @param playerOid The oid of the player being expunged.
     */
    protected void expungeVoiceClient(long playerOid) {
        expungeVoiceClient(playerOid, conMgr.getPlayerCon(playerOid));
    }

    /**
     * Remove the voice client associated with the playerOid from the
     * VoicePlugin data structures.
     * @param playerOid The oid of the player being expunged.
     * @param con The VoiceConnection object for the player.
     */
    protected void expungeVoiceClient(long playerOid, VoiceConnection con) {
        // If this member is already expunged, just return
        if (con.groupMember != null && con.groupMember.getExpunged())
            return;
        VoiceGroup group = con.group;
        if (group != null) {
            con.group = null;
            if (group.isPositional()) {
                PositionalVoiceGroup pGroup = (PositionalVoiceGroup)group;
                pGroup.removeTrackedPerceiver(playerOid);
                if (con.groupMember != null)
                    removeFromPerceptionFilter(playerOid);
                else if (Log.loggingDebug)
                	Log.debug("VoicePlugin.expungeVoiceClient: For playerOid " + playerOid + ", con.groupMember is null");
            }
            GroupMember member = con.groupMember;
            if (member == null)
                Log.info("VoicePlugin.expungeVoiceClient: For playerOid " + playerOid + ", could not find member in group " + group);
            else {
                member.setExpunged();
                group.removeMember(playerOid);
            }
        }
        con.groupMember = null;
        conMgr.removePlayer(playerOid);
        if (Log.loggingDebug)
            Log.debug("VoicePlugin.expungeVoiceClient: PlayerOid " + playerOid + " expunged");
    }

    /**
     * Increment the short seqNum so that it wraps around properly.
     * @param original The seqNum before being incremented.
     * @return The incremented seqNum.
     */
    public static short incSeqNum(short original) {
        if (original == Short.MAX_VALUE)
            return Short.MIN_VALUE;
        else
            return (short)(original + 1);
    }
    
    /**
     * Increment the short seqNum so that it wraps around properly.
     * @param original The seqNum before being incremented.
     * @param byWhat The amount to increment the seqNum by.
     * @return The incremented seqNum.
     */
    public static short incSeqNum(short original, int byWhat) {
        int sum = byWhat + (int)original;
        if (sum >= Short.MAX_VALUE)
            return (short)(Short.MIN_VALUE + (sum - Short.MAX_VALUE));
        else
            return (short)(original + byWhat);
    }
    
    /**
     * Return the total frame size for narrow or wide-band mode given
     * @param mode The Speex mode number for the frame.
     * @param wideBand If true, the frame is wide-band; if false,
     * narrow-band.
     * @return The frame size.
     */
    public static int encodedFrameSizeForMode(int mode, boolean wideBand) {
        if (wideBand) {
            if (mode < 0 || mode > 4) {
                Log.error("VoicePlugin.encodedFrameSizeForMode: wide-band mode " + 
                    mode + " is outside the range of 0-4");
                mode = 3;
            }
            return speexNarrowBandFrameSize[mode] + speexWideBandFrameSize[mode];
        }
        else {
            if (mode < 0 || mode > 8) {
                Log.error("VoicePlugin.encodedFrameSizeForMode: narrow-band mode " +
                    mode + " is outside the range of 0-8");
                mode = 5;
            }
            return speexWideBandFrameSize[mode];
        }
    }

    /**
     * Return the frame size for the band/mode specified by the first
     * byte of the Speex frame.
     * @param b The first byte of the Speex frame.
     * @return The frame size.
     */
    public static int encodedFrameSizeFromFirstByte(byte b) {
        if ((b & 0x80) != 0)
            return encodedFrameSizeForMode((b & 0x70) >> 4, true);
        else
            return encodedFrameSizeForMode((b & 0x78) >> 3, true);
    }

    /**
     * Return a string representation of the opcode number; used for logging.
     * @param opcode The voice packet opcode.
     * @return The string representing the opcode.
     */
    public static String opcodeString(byte opcode) {
        String name;
        if (opcode >= 0 && opcode <= opcodeHighest)
            name = opcodeNames[opcode];
        else {
            Log.error("VoicePlugin.opcodeString: opcode " + opcode + " is out of range!");
            name = "????????";
        }
        return name + "(" + opcode + ")";
    }
    
    /**
     * A class with locking to manage the Maps and Sets of connections.
     * The data structure that holds the maps:
     * </p>
     * playerOid to VoiceConnection
     * </p>
     * playerOid to VoiceGroup
     * </p>
     * groupOid to VoiceGroup
     * </p>
     * instanceOid to set of groups with members in that instance
     */
    public static class VoiceConManager {

        public VoiceConManager() {
        }
        
        /**
         * Add a mapping from groupOid to group
         * @param groupOid The oid of the group.
         * @param group The group for which the mapping should be
         * added.
         */
        public void addGroup(long groupOid, VoiceGroup group) {
            lock.lock();
            try {
                groupOidToGroupMap.put(groupOid, group);
            }
            finally {
                lock.unlock();
            }
        }
        
        /**
         * Get a list of the players in a group.
         * @param groupOid The oid of the group.
         * @return A list of oids of the players in that group.
         */
        public List<Long> groupPlayers(long groupOid) {
            List<Long> playerOids = new LinkedList<Long>();
            VoiceGroup group = getGroup(groupOid);
            if (group == null)
                Log.error("VoicePlugin.VoiceConManager.groupPlayers: There is no group associated with groupOid " + groupOid);
            else {
                lock.lock();
                try {
                    for (Map.Entry<Long, VoiceGroup> entry : playerOidToGroupMap.entrySet()) {
                        if (entry.getValue() == group)
                            playerOids.add(entry.getKey());
                    }
                }
                finally {
                    lock.unlock();
                }
            }
            return playerOids;
        }
        
        /**
         * Get the group for the given groupOid.
         * @param groupOid The oid of the group to be returned.
         * @return The group with the given groupOid.
         */
        public VoiceGroup getGroup(long groupOid) {
            lock.lock();
            try {
                return groupOidToGroupMap.get(groupOid);
            }
            finally {
                lock.unlock();
            }
        }
        
        /**
         * Remove a player from all ConnecitonManager maps.
         * @param playerOid The oid of the player to be removed.
         */
        public void removePlayer(long playerOid) {
            lock.lock();
            try {
                playerOidToVoiceConnectionMap.remove(playerOid);
                playerOidToGroupMap.remove(playerOid);
            }
            finally {
                lock.unlock();
            }
        }

        /**
         * Get the VoiceConnection object for the player with the
         * given oid.
         * @param playerOid The oid of the player whose
         * VoiceConnection should be returned.
         * @return The VoiceConnection object corresponding to
         * playerOid.
         */
        public VoiceConnection getPlayerCon(long playerOid) {
            lock.lock();
            try {
                return playerOidToVoiceConnectionMap.get(playerOid);
            }
            finally {
                lock.unlock();
            }
        }

        /**
         * Create the mapping from playerOid to its VoiceConnection
         * object.
         * @param playerOid The oid of the player for which the
         * mapping should be created.
         * @param con The VoiceConnection to be associated with the
         * playerOid.
         */
        public void setPlayerCon(long playerOid, VoiceConnection con) {
            lock.lock();
            try {
                playerOidToVoiceConnectionMap.put(playerOid, con);
            }
            finally {
                lock.unlock();
            }
        }

        /**
         * Create the mapping from the playerOid to the VoiceGroup
         * that contains the player.
         * @param playerOid The oid of the player contained in the
         * VoiceGroup.
         * @param group The group to be associated with the playerOid.
         */
        public void setPlayerGroup(long playerOid, VoiceGroup group) {
            lock.lock();
            try {
                playerOidToGroupMap.put(playerOid, group);
            }
            finally {
                lock.unlock();
            }
        }

        /**
         * Get the group associated with the playerOid.
         * @param playerOid The oid of the player whose VoiceGroup
         * should be returned.
         * @return The VoiceGroup associated with the playerOid.
         */
        public VoiceGroup getPlayerGroup(long playerOid) {
            lock.lock();
            try {
                return playerOidToGroupMap.get(playerOid);
            }
            finally {
                lock.unlock();
            }
        }
        
        /**
         * If the player with the given oid is in a positional group,
         * return its PositionalGroupMember; else return null.
         * @param playerOid The oid of the player whose member should
         * be returned.
         * @return The PositionalGroupMember associated with the
         * playerOid, or null if there is none.
         */
        public PositionalGroupMember getPositionalMember(long playerOid) {
            lock.lock();
            try {
                VoiceGroup group = playerOidToGroupMap.get(playerOid);
                if (group != null && group.isPositional())
                    return (PositionalGroupMember)group.isMember(playerOid);
                else
                    return null;
            }
            finally {
                lock.unlock();
            }
        }
            
        /**
         * Get a list of all GroupMembers that are members of some
         * PositionalVoiceGroup.  Called by the update thread to get
         * the positional group members, so it can iterate over them.
         * @return The list of all GroupMembers of all
         * PositionalVoiceGroups.
         */
        public List<GroupMember> getAllPositionalGroupMembers() {
            List<GroupMember> pGroupMembers = new LinkedList<GroupMember>();
            lock.lock();
            try {
                for (VoiceGroup group : groupOidToGroupMap.values()) {
                    if (group.isPositional())
                        group.getAllMembers(pGroupMembers);
                }
                return pGroupMembers;
            }
            finally {
                lock.unlock();
            }
        }
        
        /**
         * If the group is not yet in the set of groups associated
         * with the instanceOid, add it.
         * @param instanceOid The oid of the instance to which the
         * group should be added.
         * @param group The group to be added to the list of groups
         * associated with this instanceOid.
         */
        public boolean maybeAddToGroupInstances(long instanceOid, PositionalVoiceGroup group) {
            lock.lock();
            try {
                Set<PositionalVoiceGroup> groups = groupsInInstance.get(instanceOid);
                if (groups == null) {
                    groups = new HashSet<PositionalVoiceGroup>();
                    groupsInInstance.put(instanceOid, groups);
                }
                return groups.add(group);
            }
            finally {
                lock.unlock();
            }
        }
        
        /**
         * Remove the mapping from the instanceOid to the set of
         * PositionalVoiceGroups, and return that set.
         * @param instanceOid The oid of the instance whose mapping
         * should be removed.
         * @return the set of PositionalVoiceGroups formerly
         * associated with the instanceOid.
         */
        public Set<PositionalVoiceGroup> removeInstance(long instanceOid) {
            lock.lock();
            try {
                return groupsInInstance.remove(instanceOid);
            }
            finally {
                lock.unlock();
            }
        }

        public int getPlayerCount()
        {
            return playerOidToVoiceConnectionMap.size();
        }

        /**
         * Maps speaker oid to his connection
         */
        private Map<Long, VoiceConnection> playerOidToVoiceConnectionMap = new HashMap<Long, VoiceConnection>();

        /**
         * A map that identifies what group corresponds to a particular
         * group oid
         */
        private Map<Long, VoiceGroup> groupOidToGroupMap = new HashMap<Long, VoiceGroup>();

        /**
         * A map that identifies the group that currently contains the
         * player with the given player oid
         */
        private Map<Long, VoiceGroup> playerOidToGroupMap = new HashMap<Long, VoiceGroup>();

        /**
         * A map from instanceOid to the set of positional groups with
         * members in that instance.
         */
        private Map<Long, Set<PositionalVoiceGroup>> groupsInInstance = new HashMap<Long, Set<PositionalVoiceGroup>>();

        protected transient Lock lock = LockFactory.makeLock("VoiceConManager");
    }
    
    /**
     * A class containing a thread that interpolates the locations of
     * all PositionalVoiceGroup members once every second.
     */
    class Updater implements Runnable {
        
        /**
         * Loop while running, calling update() and then sleeping for 1 second.
         */
        public void run() {
            while (running) {
                try {
                    update();
                } catch (MVRuntimeException e) {
                    Log.exception("ProximityTracker.Updater.run caught MVRuntimeException", e);
                } catch (Exception e) {
                    Log.exception("ProximityTracker.Updater.run caught exception", e);
                }

                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    Log.warn("Updater: " + e);
                    e.printStackTrace();
                }
            }
        }

        /**
         * Call the ConnectionManager to get the list of all
         * PositionalGroupMembers; interpolate the locations of all
         * those members; and then iterate over them and for each
         * element of their perceivedOids, if the perceived member can
         * be found, call the group's testProximity() method to update
         * their membersInRadius.
         */
        protected void update() {
            List<GroupMember> pMembers = conMgr.getAllPositionalGroupMembers();
//             Log.debug("Updater.update: in update, " + pMembers.size() + " positional group members");
            // We loop over the copied positional group members
            // causing interpolation to happen, and capturing the
            // location in the PerceiverData, so we can later do
            // comparisons cheaply.  Note that underlying map can
            // change while we're doing so, so we don't raise errors
            // if it happens.
            for (GroupMember member : pMembers) {
                PositionalGroupMember pMember = (PositionalGroupMember)member;
                if (pMember.wnode == null)
                    continue;
                pMember.previousLoc = pMember.lastLoc;
//              long lastInterp = pMember.wnode.getLastInterp();
                pMember.lastLoc = pMember.wnode.getLoc();
//              if (Log.loggingDebug)
//              Log.debug("Updater.update: perceiverOid " + pMember.getMemberOid() + ", previousLoc " + pMember.previousLoc + 
//                        ", lastLoc " + pMember.lastLoc + ", time since interp " + (System.currentTimeMillis() - lastInterp));
            }
            // Now actually do the double loop to check if inRange has
            // changed
            for (GroupMember member : pMembers) {
                PositionalGroupMember pMember = (PositionalGroupMember)member;
                if (pMember.wnode == null)
                    continue;
                // If the perceiver hasn't moved much, no need to
                // iterate over it's perceived entities
                if (Log.loggingDebug)
                    Log.debug("Updater.update: perceiverOid " + pMember.getMemberOid() + " previousLoc " + pMember.previousLoc + ", lastLoc " + pMember.lastLoc);
                if (pMember.previousLoc != null &&
                    Point.distanceToSquared(pMember.previousLoc, pMember.lastLoc) < 100f)
                    continue;
                ArrayList<Long> perceivedOids = new ArrayList<Long>(pMember.perceivedOids);
                if (Log.loggingDebug)
                    Log.debug("Updater.update: perceiverOid " + pMember.getMemberOid() + " has " + perceivedOids.size() + " perceivedOids");
                for (long perceivedOid : perceivedOids) {
                    VoiceConnection con = conMgr.getPlayerCon(perceivedOid);
                    if (con == null)
                        continue;
                    PositionalGroupMember perceivedMember = (PositionalGroupMember)con.groupMember;
                    if (perceivedMember == null || perceivedMember.wnode == null)
                        continue;
                    // Invoke the testProximity method but tell it not
                    // to interpolate, but instead get its location
                    // from the PerceptionData.lastLoc members
                    PositionalVoiceGroup pGroup = (PositionalVoiceGroup)pMember.getGroup();
                    pGroup.testProximity(pMember, perceivedMember, false, false);
                }
            }
        }
    }

    /**
     * All voice packets start with 4 bytes:
     *    o 16-bit sequence number, increased by one for each
     *      successive transmission for this voice.
     *    o 8-bit opcode byte
     *    o 8-bit voice number
     */
    public static final byte voicePacketHeaderSize = 4;

    /*
     * Three of the message types, AllocateCodec, ReallocateCodec
     * and ReconfirmCodec, allocate a voice channel for sounds
     * from a game object.  They contain the 8-byte OID of the
     * game object producing the sound, for a total size of 12
     * bytes.
     *
     * There are really only two state transitions for listener
     * voices: from Unallocated to Allocated, and back again.
     *
     * When a voice is unallocated data packets are ignored until
     * an AllocateCodec, ReallocateCodec, or ReallocateCodec is
     * seen.  All three result in allocation of the channel, which
     * moves it to the Allocated state.
     *
     * When in the Allocated state, opcodeDeallocate returns the
     * listener voice to the unallocated state.
     *
     * The only remaining message is opcodeData, whose representation
     * is the standard 4-byte header followed by the Speex data frame.
     *
     * The client uses the same format opcodeAllocateCodec message to
     * inform the server about a new microphone, and opcodeData to
     * send frames of Speex data, and opcodeDeallocate message to
     * disassociate the microphone.  We may need some additional
     * opcodes to handle voice activity changes, but for now the
     * assumption is that the microphone will either deallocate the
     * channel when the user isn't speaking, or just not send data
     * packets.
     *
     * In all cases, the microphone number is stuck in the voice
     * number field of the packet.
     *
     * All allocate and deallocate packets are sent by themselves, but
     * data frames are aggregated into 
     */

    /**
     * All voices start out unallocated
     */
    public static final byte opcodeVoiceUnallocated = 0;

    /**
     * The authenticate packet must be the first one received by the
     * voice plugin on any new connection from a client.  The voice
     * plugin maintains a map of the IP/port number to the oid of the
     * client, used to validate traffic.  The packet contains the
     * string authentication token, the oid of the player; the oid of
     * the group the player is signing up to; and a bool saying
     * whether voice packets from this connection should be sent back
     * to this connection.  Total size is 2 + 1 + 1 + 8 + 8 + 1 + 4 =
     * 25 bytes plus number of bytes in the authToken string.
     */
    public static final byte opcodeAuthenticate = 1;

    /**
     * Allocate a voice.  Apart from the header, the payload
     * is the 8-byte OID of the object emitting the sound.
     * Size is 12 bytes.
     */
    public static final byte opcodeAllocateCodec = 2;

    /**
     * This has exactly the same payload as AllocateCodec, but
     * says that the voice should be positional.
    */
    public static final byte opcodeAllocatePositionalCodec = 3;

    /**
     * This has exactly the same payload as AllocateCodec, but
     * is with lossy transports like UDP to send the opcode
     * parameters every second or so.  Size is 12 bytes.
     */
    public static final byte opcodeReconfirmCodec = 4;

    /**
     * Deallocate the voice number.  A voice number must be
     * deallocated before it can be reused.  There is no
     * additional data.  This is used both when a client signs
     * off, and when the microphone goes quiet.  A total of 4
     * bytes.
     */
    public static final byte opcodeDeallocate = 5;

    /**
     * A data packet, consisting of a 4-byte header followed by the
     * bytes of the data frame.  All data messages _from_ the client
     * supply the microphone number as the voice number.  Since for
     * the time being we support exactly one microphone, the voice
     * numbers in messages from the client are always zero.  Size is 4
     * bytes plus the codec frame playload, typically 28 bytes for
     * 11000bps.
     */
    public static final byte opcodeData = 6;

    /**
     * An aggregated data packet contains a number of data packets.
     * It starts with a standard header, and has an additional byte
     * arg which is the number of data packets contained therein.  The
     * first contained data frame has a seqnum equal to the seqnum of
     * the ggregated data packet, and are numbered sequentially
     * thereafter.  So the seqnum of the next packet after an
     * aggregated data packet is larger by the number of data frames
     * in the packet.  Each data packet inside an aggregated data
     * packet starts with a 1-byte length.
     */
    public static final byte opcodeAggregatedData = 7;

    /**
     * An opcode sent exclusively from the server to the client, and
     * used only to support synchronization between voice bots and
     * test clients.  It contains the standard header plus the oid of
     * the player whose login status has changed.  The voiceNumber is
     * 1 if it's a login, and 0 if it's a logout.
     */
    public static final byte opcodeLoginStatus = 8;

    /**
     * An opcode sent exclusively from the client to the server that
     * marks a collection of oids as ignored or not ignored.
     * The format is a short count of speakers whose ignore status
     * should change, and for each speaker, a byte which is non-zero
     * if it should be ignored or zero if it should no longer be
     * ignored.
     * This opcode is obsolete and unused; ignoredSpeakerOids are 
     * maintained via messaging with the proxy.
     */
    public static final byte opcodeChangeIgnoredStatus = 9;

    public static final int opcodeHighest = opcodeChangeIgnoredStatus;

    /**
     * Used in logging messages
     */
    public static String[] opcodeNames = new String[] { "Unallocd", "Auth    ", "Alloc   ", "AllocPos", "Confirm ", "Dealloc ", "Data    ", "AggrData", "LgnStatus" };
    
    /**
     * This gives the number of bytes in the message excluding the
     * length, except for the data case, where it gives the number of
     * bytes in the header, but not including the data itself
     */
    public static int[] voiceMsgSize = new int[] { 0, 25, 12, 12, 12, 4, 4, 5, 12, 6 };

    /**
     * There are two bytes in the length prefixed to a TCP message
     */
    public static int lengthBytes = 2;

    /**
     * This array is indexed by Speex narrow-band mode, and gives
     * the narrow-band frame size for that mode.
     */
    public static int[] speexNarrowBandFrameSize = new int[] { 1, 6, 15, 20, 28, 38, 46, 62, 10 };

    /*
     * When sending wide-band data, for each frame Speex first
     * sends the narrow-band encoded frame, and then sends the
     * wide-band "supplement", whose size for each wide-band mode
     * is given below.
     * ??? TBD: When transmitting wide-band frames, is the mode in
     * both the wide and narrow chunks of the frame the same?
     * That would mean that the narrow-band mode is constrained to
     * be 0 - 4
     */
    public static int[] speexWideBandFrameSize = new int[] { 1, 5, 14, 24, 44 };

    /**
     * The server is supposed to run things so no client ever has
     * more than this number of voice channels.
     */
    public static int maxVoiceChannels = 4;

    /**
     * Counters that get logged every second
     */
    private CountLogger countLogger;

    private static CountLogger.Counter countPacketsIgnored;
    private static CountLogger.Counter countSeqNumGaps;

    public static CountLogger.Counter countSendLoginStatus;

    private static CountLogger.Counter countPacketsReceived;
    private static CountLogger.Counter countDataFramesReceived;
    public static CountLogger.Counter countAllocateVoiceReceived;
    public static CountLogger.Counter countDeallocateVoiceReceived;

    private static CountLogger.Counter countPacketsSent;
    private static CountLogger.Counter countDataFramesSent;
    public static CountLogger.Counter countAllocateVoiceSent;
    public static CountLogger.Counter countDeallocateVoiceSent;

    public static boolean runHistograms = false;
    
    public static TimeHistogram processPacketHistogram;    
    public static TimeHistogram dataSendHistogram;
    public static TimeHistogram voiceAllocHistogram;
    public static TimeHistogram voiceDeallocHistogram;
    
    public static boolean checkAuthToken = true;

    private VoiceConManager conMgr = new VoiceConManager();
    
    /**
     * The port number we're listening on
     */
    private Integer voicePort;

    /**
     * Should we record all voices as we run?
     *
     * ??? TBD: We'll need to make this more selective in the
     * production version, via an API call
     */
    private boolean recordVoices = false;

    /**
     * A map of connections that will receive login/logout events - -
     * only used by the voice bot system.
     */
    private Set<VoiceConnection> loginStatusEventReceivers = null;
    
    /**
     * The next sequence number for a login status message
     */
    private short loginStatusSeqNum = 0;

    /**
     * The engine that handles TCP message traffic to all the clients.
     */
    private ClientTCPMessageIO clientTCPMessageIO = null;
    
    /**
     * If this is true, we'll synthesize the group and/or group member
     * when an auth packet comes in.  Used only for testing.
     */
    protected static boolean createGroupWhenReferenced = false;

    /**
     * If this is true, we subscribe to login messages and pass on
     * login notifications.
     */
    protected static boolean allowVoiceBots = false;

    /**
     * Can hear positional sounds up to 20 meters away
     */
    private static float audibleRadius = 20 * 1000f;

    /**
     * The hysteresis constant: don't change whether a pair of
     * positonal group members are in-radius of each other if the
     * distance is within this distance of the audibleRadius.
     */
    protected static float hystericalMargin = 3000f;

    /**
     * singleton
     */
    protected static VoicePlugin instance = null;

    /**
     * The server version to send to clients
     */
    private String serverVersion = null;

    protected PerceptionFilter perceptionFilter;
    protected long perceptionSubId;

    protected Updater updater = null;
    
    protected Thread updaterThread = null;
    
    protected boolean running = true;

    protected transient Lock lock = LockFactory.makeLock("VoicePlugin");
}


