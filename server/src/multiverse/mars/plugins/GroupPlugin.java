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

package multiverse.mars.plugins;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeSet;
import java.util.concurrent.locks.Lock;

import multiverse.mars.objects.CombatInfo;
import multiverse.mars.objects.MarsGroup;
import multiverse.mars.objects.MarsGroupMember;
import multiverse.msgsys.Message;
import multiverse.msgsys.MessageAgent;
import multiverse.msgsys.MessageTypeFilter;
import multiverse.msgsys.ResponseMessage;
import multiverse.server.engine.Engine;
import multiverse.server.engine.EnginePlugin;
import multiverse.server.engine.Hook;
import multiverse.server.messages.LogoutMessage;
import multiverse.server.messages.PropertyMessage;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedComMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.plugins.VoiceClient;
import multiverse.server.util.Log;
import multiverse.server.util.Logger;

public class GroupPlugin extends EnginePlugin {

    /*
     * Properties
     */
    
    protected static final Logger _log = new Logger("GroupPlugin");
    protected static List<String> _registeredStats = new ArrayList<String>();
    protected static Map<Long, MarsGroup> _currentGroups = new Hashtable<Long, MarsGroup>();
    protected static int _maxGroupSize = 8; // Default max group size to 8

    /*
     * Constructors
     */ 
    public GroupPlugin() {
        super("Group");
        setPluginType("Group");
    }

    public String GetName() {
        return "GroupPlugin";
    }

    /*
     * Events
     */ 
    
    public void onActivate() {
        super.onActivate();

        // register message hooks
        RegisterHooks();

        // setup message filters
        MessageTypeFilter filter = new MessageTypeFilter();
        filter.addType(PropertyMessage.MSG_TYPE_PROPERTY);
        filter.addType(GroupClient.MSG_TYPE_GROUP_INVITE);
        filter.addType(GroupClient.MSG_TYPE_GROUP_REMOVE_MEMBER);
        filter.addType(GroupClient.MSG_TYPE_GROUP_CHAT);
        filter.addType(GroupClient.MSG_TYPE_GROUP_INVITE_RESPONSE);
        filter.addType(GroupClient.MSG_TYPE_GROUP_SET_ALLOWED_SPEAKER);
        filter.addType(GroupClient.MSG_TYPE_GROUP_MUTE_VOICE_CHAT);
        filter.addType(GroupClient.MSG_TYPE_GROUP_VOICE_CHAT_STATUS);
        filter.addType(VoiceClient.MSG_TYPE_VOICE_MEMBER_ADDED);
        Engine.getAgent().createSubscription(filter, this);
        
        //setup responder message filters
        MessageTypeFilter responderFilter = new MessageTypeFilter();
        responderFilter.addType(LogoutMessage.MSG_TYPE_LOGOUT);
        responderFilter.addType(GroupClient.MSG_TYPE_REQUEST_GROUP_INFO); 
        Engine.getAgent().createSubscription(responderFilter, this, MessageAgent.RESPONDER);

        if (Log.loggingDebug)
            _log.debug("GroupPlugin activated.");
    }

    /*
     * Methods
     */ 
    
    public void RegisterHooks() {
        getHookManager().addHook(PropertyMessage.MSG_TYPE_PROPERTY,
                                 new PropertyHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_INVITE,
                                 new GroupInviteHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_INVITE_RESPONSE,
                                 new GroupInviteResponseHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_REMOVE_MEMBER,
                                 new GroupRemoveMemberHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_CHAT,
                                 new GroupChatHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_REQUEST_GROUP_INFO,
                                 new RequestGroupInfoHook());
        getHookManager().addHook(LogoutMessage.MSG_TYPE_LOGOUT,
                                 new LogOutHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_SET_ALLOWED_SPEAKER,
                                 new SetAllowedSpeakerHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_MUTE_VOICE_CHAT,
                                 new MuteGroupHook());
        getHookManager().addHook(GroupClient.MSG_TYPE_GROUP_VOICE_CHAT_STATUS,
                                 new VoiceStatusHook());
        getHookManager().addHook(VoiceClient.MSG_TYPE_VOICE_MEMBER_ADDED,
                                 new VoiceMemberAddedHook());
    }

    public static void RegisterStat(String stat) {
        _registeredStats.add(stat);
    }

    protected MarsGroup GetGroup(long groupOid) {
        return _currentGroups.get(groupOid);
    }

    public static List<String> GetRegisteredStats() {
        return _registeredStats;
    }

    /**
     * Gets information about the group and its members and sends it to each
     * group member
     */
    protected void SendGroupUpdate(MarsGroup group) {
        MessageAgent agent = Engine.getAgent();
        TargetedExtensionMessage groupUpdateMsg = new TargetedExtensionMessage();
        groupUpdateMsg.setExtensionType(GroupClient.EXTMSG_GROUP_UPDATE);
        groupUpdateMsg.setProperty("maxGroupSize", String.valueOf(_maxGroupSize));
        // groupOid is the unique key for the group and the voice group
        groupUpdateMsg.setProperty("groupOid", group.getOid());
        // Set each group members info
        int counter = 1; // Counter is used to supply the client with an ordered key for accessing group members
        Hashtable<Long, MarsGroupMember> groupMembers = group.GetGroupMembers();
        // We must make sure keys are ordered properly by order the player joined the group
        Set<Long> groupMemberKeys = new TreeSet<Long>(group.GetGroupMembers().keySet());

        for (Long groupMemberKey : groupMemberKeys) {
            HashMap<String, Serializable> groupMemberInfo = new HashMap<String, Serializable>();
            MarsGroupMember groupMember = groupMembers.get(groupMemberKey);
            groupMemberInfo.put("memberOid", groupMember.GetGroupMemberOid());
            groupMemberInfo.put("name", groupMember.GetGroupMemberName());
            groupMemberInfo.put("voiceEnabled", groupMember.GetVoiceEnabled());
            groupMemberInfo.put("allowedSpeaker", groupMember.GetAllowedSpeaker());
            groupMemberInfo.put("groupMuted", group.GetGroupMuted());
            for (String stat : _registeredStats) { // Add any registered stats to info
                groupMemberInfo.put(stat, groupMember.GetGroupMemberStat(stat));
            }
            // Store counter as our key so we can sort the list on the client
            groupUpdateMsg.setProperty(String.valueOf(counter), groupMemberInfo);
            //if group does not have a leader set, then the first group member in order is now the leader
            if(group.GetGroupLeaderOid() == 0 && counter == 1)
                group.SetGroupLeaderOid(groupMember.GetGroupMemberOid());
            counter += 1;
        }

        // Send message to each group member
        for (MarsGroupMember groupMember : group.GetGroupMembers().values()) {
            groupUpdateMsg.setTarget(groupMember.GetGroupMemberOid());
            agent.sendBroadcast(groupUpdateMsg);
        }
    }

    protected void RemoveGroupMember(CombatInfo info) {
        MarsGroup group = GetGroup(info.getGroupOid());
        //Check to ensure group is valid
        if (group == null){
            if (Log.loggingDebug) {
                _log.error("GroupPlugin.RemoveGroupMember : group is null");
            }
            info.setGroupOid(0);
            info.setGroupMemberOid(0);
            return;
        }
        // Dis-associate group with member and member with group
        group.RemoveGroupMember(info);

        //If the group leader left, trigger system to set new group leader
        if(group.GetGroupLeaderOid() == info.getOwnerOid()){
            group.SetGroupLeaderOid(0); //Clear group leader Oid so it is reset in SendGroupUpdate method
        }

        // Send update to group
        if (group.GetNumGroupMembers() > 1) {
            SendGroupUpdate(group);            
        } else {
            // Update remaining member that they no longer are in group either
            // since one person can't be a group
            CombatInfo groupLeader = CombatPlugin.getCombatInfo(group.GetGroupLeaderOid());
            if(groupLeader != null){
                group.RemoveGroupMember(groupLeader); //Removing the last group member triggers removal of voice group
                TargetedExtensionMessage groupUpdateMsg = new TargetedExtensionMessage(groupLeader.getOwnerOid());
                groupUpdateMsg.setExtensionType(GroupClient.EXTMSG_GROUP_UPDATE);
                Engine.getAgent().sendBroadcast(groupUpdateMsg);
            } else {
                _log.error("GroupPlugin.RemoveGroupMember - Group leader is null");
            }
            _currentGroups.remove(group);
            group = null;
        }

        // Send group update message to player being removed in order to clear their group info
        TargetedExtensionMessage groupUpdateMsg = new TargetedExtensionMessage(info.getOwnerOid());
        groupUpdateMsg.setExtensionType(GroupClient.EXTMSG_GROUP_UPDATE);
        Engine.getAgent().sendBroadcast(groupUpdateMsg);
    }

    /**
     * Sets the maximum number of players that can be in a single group - Default is 8
     */
    public static void SetMaxGroupSize(int size) {
        _maxGroupSize = size;
    }

    /**
     * Sends update to group members about the group and its members
     */
    protected boolean UpdateGroupMemberProps(PropertyMessage propMsg) {
        CombatInfo subject = CombatPlugin.getCombatInfo(propMsg.getSubject());
        if (subject == null)
            return false;
        // If the subject is grouped, continue
        if (subject.isGrouped()) {
            Set<String> props = propMsg.keySet();
            Map<String, Serializable> statsToUpdate = new HashMap<String, Serializable>();
            // Check our registered properties to see if any are in the
            // update list
            for (String stat : _registeredStats) {
                if (props.contains(stat)) {
                    statsToUpdate.put(stat, propMsg.getProperty(stat));
                }
            }
            // If any properties that the group cares about was updated,
            // send out a message to all the group members
            if (statsToUpdate.size() > 0) {
                MarsGroup group = GetGroup(subject.getGroupOid());
                if(group == null){
                    _log.error("GroupPlugin.UpdateGroupMemberProps - group is null");
                    subject.setGroupMemberOid(0);
                    subject.setGroupOid(0);
                    return false;
                }

                SendGroupPropertyUpdate(subject.getOwnerOid(), group, statsToUpdate);
            }
        }
        return true;
    }

    /**
     * SendGroupPropertyUpdate - Sends a mv.GROUP_PROPERTY_UPDATE message to each client in the group client
     * @param subjectOid - Player whos property changed
     * @param group - Group in which the subject belongs to
     * @param statsToUpdate - Map<String, Serializable> of properties that were updated
     */
    protected void SendGroupPropertyUpdate(Long subjectOid, MarsGroup group, Map<String, Serializable> statsToUpdate){
        Collection<MarsGroupMember> groupMembers = group.GetGroupMembers().values();
        for (MarsGroupMember groupEntry : groupMembers) {
            TargetedExtensionMessage updateMessage =
                new TargetedExtensionMessage(groupEntry.GetGroupMemberOid());

            updateMessage.setExtensionType(GroupClient.EXTMSG_GROUP_PROPERTY_UPDATE);
            updateMessage.setProperty("memberOid", subjectOid); // member being updated
            for (String stat : statsToUpdate.keySet()) {
                updateMessage.setProperty(stat, statsToUpdate.get(stat));
            }
            Engine.getAgent().sendBroadcast(updateMessage);
        }
    }

    /**
     * Handles logic for an invite request response - either accepted or declined
     * Creates a new group if the inviter is not currently grouped
     */
    protected boolean HandleInviteResponse(ExtensionMessage inviteMsg) {
        CombatInfo invitee = CombatPlugin.getCombatInfo(inviteMsg.getSubject());
        CombatInfo inviter = CombatPlugin.getCombatInfo((Long) inviteMsg
                                                        .getProperty("groupLeaderOid"));
        if (inviter == null || invitee == null)
            return false;

        String response = inviteMsg.getProperty("response").toString();
        if (response.equals("accept")) {
            MarsGroup group = null;
            Boolean voiceEnabled = (Boolean)inviteMsg.getProperty("groupVoiceEnabled");
            if (inviter.isGrouped()) { // Add invitee to group
                group = GetGroup(inviter.getGroupOid());
                if(group == null){
                    _log.error("GroupPlugin.HandleInviteResponse - group is null");
                    inviter.setGroupMemberOid(0);
                    inviter.setGroupOid(0);
                    return false;
                }
                MarsGroupMember groupMember = group.AddGroupMember(invitee);  
                groupMember.SetVoiceEnabled(voiceEnabled);
            } else { // Create a new group
                group = new MarsGroup();
                MarsGroupMember groupLeader = group.AddGroupMember(inviter);
                groupLeader.SetVoiceEnabled(true); //TODO: Figure out how to get leader's setting
                group.SetGroupLeaderOid(inviter.getOwnerOid());
                MarsGroupMember groupMember = group.AddGroupMember(invitee);  
                groupMember.SetVoiceEnabled(voiceEnabled);
                _currentGroups.put(group.GetGroupOid(), group); // Add to our list to track
            }      
            // Send clients info about the group and group members
            SendGroupUpdate(group);
        } else {
            String inviteeName = WorldManagerClient.getObjectInfo(invitee.getOwnerOid()).name;
            SendTargetedGroupMessage(inviter.getOwnerOid(), inviteeName + " has declined your group invite.");
        }
        // Clear pending group invite flag
        invitee.setPendingGroupInvite(false);
        return true;
    }

    /**
     * Logic to handle group specific chat
     */
    protected void HandleGroupChat(ExtensionMessage groupChatMsg) {
        String message = groupChatMsg.getProperty("message").toString();
        Long senderOid = (Long) groupChatMsg.getProperty("senderOid");

        CombatInfo sender = CombatPlugin.getCombatInfo(senderOid);
        if (sender.isGrouped()) {
            String senderName =  WorldManagerClient.getObjectInfo(sender.getOwnerOid()).name;
            MarsGroup group = GetGroup(sender.getGroupOid());
            if(group == null){
                _log.error("GroupPlugin.HandleGroupChat - group is null");
                sender.setGroupMemberOid(0);
                sender.setGroupOid(0);
                return;
            }
            Collection<MarsGroupMember> groupMembers = group.GetGroupMembers().values();            
            //Send chat message to each group member
            for (MarsGroupMember groupMember : groupMembers) {
                SendTargetedGroupMessage(groupMember.GetGroupMemberOid(), "[" + senderName + "]: " + message);
            }
        } else {
            SendTargetedGroupMessage(sender.getOwnerOid(), "You are not grouped!");
        }
    }

    /**
     * Handles invite request by sending invite request message to the invitee
     */
    protected boolean HandleGroupInvite(ExtensionMessage inviteMsg) {
        CombatInfo inviter = CombatPlugin.getCombatInfo(inviteMsg.getSubject());
        CombatInfo invitee = CombatPlugin.getCombatInfo((Long)inviteMsg.getProperty("target"));
        if (inviter == null || invitee == null) {
            return false;
        }
        if (Log.loggingDebug) {
            _log.debug("GroupPlugin.GroupInviteHook: Received group invite message inviter:"
                       + inviter.getOwnerOid()
                       + " invitee:"
                       + invitee.getOwnerOid());
        }    

        // A player should not be able to invite themselves to a group
        if(inviter.getOwnerOid().equals(invitee.getOwnerOid())){
            return true;
        }
        
        if(inviter.isGrouped()){
            MarsGroup group = GetGroup(inviter.getGroupOid());
            if(group == null){
                _log.error("GroupPlugin.HandleGroupInvite - Inviter's group is null");
                inviter.setGroupMemberOid(0);
                inviter.setGroupOid(0);
                return false;
            }
            if (group.GetGroupMembers().size() >= _maxGroupSize){
                SendTargetedGroupMessage(inviter.getOwnerOid(), "Your group is full.");
                return true;
            }
        }
        
        String inviteeName = WorldManagerClient.getObjectInfo(invitee.getOwnerOid()).name;
        //Send message to inviter
        SendTargetedGroupMessage(inviter.getOwnerOid(), "You have invited " + inviteeName + " to your grouped.");

        if (invitee.isGrouped()) {            
            SendTargetedGroupMessage(inviter.getOwnerOid(), inviteeName + " is already grouped.");
        } else if (invitee.isPendingGroupInvite()) {
            SendTargetedGroupMessage(inviter.getOwnerOid(), inviteeName + " is already considering a group invite.");
        } else if (!invitee.isGrouped()) {            
            // Set pending group invite flag
            invitee.setPendingGroupInvite(true);

            TargetedExtensionMessage inviteRequestMsg = new TargetedExtensionMessage(invitee.getOwnerOid());
            inviteRequestMsg.setExtensionType(GroupClient.EXTMSG_GROUP_INVITE_REQUEST);
            inviteRequestMsg.setProperty("groupLeaderOid", inviter.getOwnerOid());
            String inviterName = WorldManagerClient.getObjectInfo(inviter.getOwnerOid()).name;
            inviteRequestMsg.setProperty("groupLeaderName", inviterName);

            if (Log.loggingDebug) {
                _log.debug("GroupPlugin.GroupInviteHook: Sending group invite request inviter:"
                           + inviter.getOwnerOid()
                           + " invitee:"
                           + invitee.getOwnerOid());
            }

            Engine.getAgent().sendBroadcast(inviteRequestMsg);
        }
        return true;
    }

    /**
     * HandleGroupInfoRequest handles a request for information about a group.
     * Returns the groupOid, groupleaderOid and each member's Oid in a response message.
     */
    protected HashSet<Long> HandleGroupInfoRequest(CombatInfo subject){
        HashSet<Long> memberOids = new HashSet<Long>();
        if(subject.isGrouped()){
            MarsGroup group = GetGroup(subject.getGroupOid());
            if(group == null){
                _log.error("GroupPlugin.HandleGroupInfoRequest - group is null");
                subject.setGroupMemberOid(0);
                subject.setGroupOid(0);
                return memberOids;
            }

            Collection<MarsGroupMember> groupMembers = group.GetGroupMembers().values();
            for(MarsGroupMember groupMember : groupMembers){
                memberOids.add(groupMember.GetGroupMemberOid());
            }
        }
        return memberOids;
    }

    /**
     * SendTargetedGroupMessage - Handles sending messages to the group com channel
     */
    protected void SendTargetedGroupMessage(long target, String message){
        TargetedComMessage comMessage = new TargetedComMessage();
        comMessage.setString(message);
        comMessage.setChannel(4); //Group channel
        comMessage.setTarget(target);
        Engine.getAgent().sendBroadcast(comMessage);
    }

    protected static MarsGroupMember GetGroupMember(Long subjectOid){
        Collection<MarsGroup> groups = _currentGroups.values();
        for(MarsGroup group : groups){
            MarsGroupMember subject = group.GetGroupMember(subjectOid);
            if(subject != null)
                return subject;
        }
        return null;
    }

    /**
     * HandleSetAllowedSpeaker - Used to mark the target as an allowed speaker or not of the group's voice chat.
     *                              If the target is currently an allowed speaker they will in effect become muted
     * @param targetOid - Player to mute or un-mute
     * @param setterOid - Requesting Player
     * @param groupOid - Identifier for the group the target and setter belong to
     */
    protected boolean HandleSetAllowedSpeaker(long targetOid, long setterOid, long groupOid){
        MarsGroup group = GetGroup(groupOid);
        MarsGroupMember target = group.GetGroupMember(targetOid);

        if(group == null){
            Log.error("GroupPlugin.HandleSetAllowedSpeaker - Group is null.");
            return false;
        }

        if(target == null){
            Log.error("GroupPlugin.HandleSetAllowedSpeaker - Target is null.");
            return false;
        }

        if(target.GetVoiceEnabled()){
            Map<String, Serializable> statToUpdate = new HashMap<String, Serializable>();
            // If group is muted, then cannot change status unless the setter is the gorup leader
            if(!group.GetGroupMuted() || setterOid == group.GetGroupLeaderOid()){
                target.SetAllowedSpeaker(!target.GetAllowedSpeaker());  
                // Update voice server                
                int result = VoiceClient.setAllowedSpeaker(groupOid, targetOid, target.GetAllowedSpeaker());
                if(result != VoiceClient.SUCCESS)
                    Log.error("GroupPlugin.HandleSetAllowedSpeaker : Create Voice Group Response - " + VoiceClient.errorString(result));
            }
            // Send voice status to all group members
            statToUpdate.put("allowedSpeaker", target.GetAllowedSpeaker());
            SendGroupPropertyUpdate(targetOid, group, statToUpdate);

        }
        return true;
    }
    
    /**
     * HandleMuteGroup - Allows group leader to mute or un-mute the group's voice chat
     * @param setterOid
     * @param groupOid
     */
    protected boolean HandleMuteGroup(Long setterOid, Long groupOid){
        MarsGroup group = GetGroup(groupOid);
        
        if(group == null){
            Log.error("GroupPlugin.HandleMuteGroup - Group is null.");
            return false;
        }

        // Only the group leader should be able to mute the group
        if(setterOid == group.GetGroupLeaderOid()){
            group.SetGroupMuted(!group.GetGroupMuted());

            Collection<MarsGroupMember> groupMembers = group.GetGroupMembers().values();

            // Mute each player in the group except for group leader
            for(MarsGroupMember groupMember : groupMembers){
                if(groupMember.GetVoiceEnabled() && groupMember.GetGroupMemberOid() != group.GetGroupLeaderOid()){
                    groupMember.SetAllowedSpeaker(!group.GetGroupMuted());
                    // Call voice server to mute player
                    VoiceClient.setAllowedSpeaker(groupOid, groupMember.GetGroupMemberOid(), !group.GetGroupMuted());
                    // Update group members about player voice status
                    Map<String, Serializable> statToUpdate = new HashMap<String, Serializable>();
                    statToUpdate.put("allowedSpeaker", !group.GetGroupMuted());
                    statToUpdate.put("groupMuted", group.GetGroupMuted());
                    SendGroupPropertyUpdate(groupMember.GetGroupMemberOid(), group, statToUpdate);
                }
            }
            GroupClient.GroupEventType eventType = GroupClient.GroupEventType.MUTED;
            if (!group.GetGroupMuted())
                eventType = GroupClient.GroupEventType.UNMUTED;
            GroupClient.SendGroupEventMessage(eventType, group, setterOid);
        }

        return true;
    }

    /**
     * HandledVoiceStatus - Logic to handle mv.VOICE_CHAT_STATUS message.
     *                          Updates group member's voiceEnabled property and
     *                          broadcasts update to the other group members
     * @param playerOid - Player being updated
     * @param groupOid - Group being referenced
     * @param voiceEnabled - Value to determine if the player's voice is enabled on their client (Voice enabled and join Party enabled)
     */
    protected boolean HandledVoiceStatus(Long playerOid, Long groupOid, Boolean voiceEnabled){
        MarsGroup group = GetGroup(groupOid);        

        if(group == null){
            Log.error("GroupPlugin.HandledVoiceStatus - Group is null.");
            return false;
        }

        MarsGroupMember player = group.GetGroupMember(playerOid);

        if(player == null){
            Log.error("GroupPlugin.HandledVoiceStatus - Player is null.");
            return false; 
        }

        player.SetVoiceEnabled(voiceEnabled);
        Map<String, Serializable> statToUpdate = new HashMap<String, Serializable>();
        statToUpdate.put("voiceEnabled", voiceEnabled);
        SendGroupPropertyUpdate(playerOid, group, statToUpdate);

        return true;
    }

    /**
     * HandleVoiceMemberAdded - Handles logic for processing the VoiceClient.MSG_TYPE_VOICE_MEMBER_ADDED
     *  message type. Update any group or group member information related to a player joining the voice group
     *  that is associated with a corresponding MarsGroup object.
     * @param memberOid
     * @param groupOid
     */
    protected boolean HandleVoiceMemberAdded(Long memberOid, Long groupOid){
        _log.debug("GroupPlugin.HandleVoiceMemberAdded - Got member added message");
        // We only want to process groupOids that matches an OID in our current groups list
        if(_currentGroups.containsKey(groupOid)){
            _log.debug("GroupPlugin.HandleVoiceMemberAdded - Got member match");
            MarsGroup group = _currentGroups.get(groupOid);
            MarsGroupMember groupMember = group.GetGroupMember(memberOid);
            if(groupMember != null){
                Map<String, Serializable> statsToUpdate = new HashMap<String, Serializable>();
                //If the group is currently in a muted state, then we need to mute the new member
                if(group.GetGroupMuted()){
                    groupMember.SetAllowedSpeaker(Boolean.FALSE);                
                    statsToUpdate.put("allowedSpeaker", Boolean.FALSE);                
                }

                //If the group member is flagged to indicate their voice is disabled, then enable it
                if(!groupMember.GetVoiceEnabled()){
                    groupMember.SetVoiceEnabled(Boolean.TRUE);
                    statsToUpdate.put("voiceEnabled", Boolean.FALSE);
                }

                if(statsToUpdate.size() > 0)
                    SendGroupPropertyUpdate(memberOid, group, statsToUpdate);
            }
            else
                _log.error("GroupPlugin.HandleVoiceMemberAdded - Player with OID " + memberOid.toString() + 
                           " is not a member of the group with OID " + groupOid.toString());
        }
        return true;
    }
    
    /*
     * Hooks
     */     

    /**
     * PropertyHook catches any property updates. We only want to process
     * entities that are flagged as grouped
     */
    class PropertyHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            PropertyMessage propMsg = (PropertyMessage) msg;
            return UpdateGroupMemberProps(propMsg);
        }
    }

    /**
     * GroupInviteResponseHook Adds a player to a group, or creates a new group
     * and sends out group info to the clients
     */
    class GroupInviteResponseHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage inviteMsg = (ExtensionMessage) msg;
            return HandleInviteResponse(inviteMsg);
        }
    }

    /**
     * GroupRemoveMemberHook is used to remove the group member in question.
     * 
     */
    class GroupRemoveMemberHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage removeMemberMsg = (ExtensionMessage) msg;
            CombatInfo subject = CombatPlugin.getCombatInfo((Long) removeMemberMsg.getProperty("target"));
            if (subject == null)
                return false;

            RemoveGroupMember(subject);

            return true;
        }
    }

    /**
     * Handles group chat messages from the client
     */
    class GroupChatHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage groupChatMsg = (ExtensionMessage) msg;
            HandleGroupChat(groupChatMsg);
            return true;
        }
    }

    /**
     * LogOutHook is used to remove group members from a group who log out of the game.
     */
    class LogOutHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            LogoutMessage logoutMsg = (LogoutMessage) msg;
            CombatInfo subject = CombatPlugin.getCombatInfo(logoutMsg.getSubject());

            // If the player logging out is grouped, then remove them from their group
            if (subject != null && subject.isGrouped()) {
                RemoveGroupMember(subject);
            }
            Engine.getAgent().sendResponse(new ResponseMessage(logoutMsg));
            return true;
        }
    }

    /*
     * GroupInviteHook handles invitee response
     */
    class GroupInviteHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage inviteMsg = (ExtensionMessage) msg;

            return HandleGroupInvite(inviteMsg);
        }
    }

    /**
     * RequestGroupInfoHook handles group info requests. Returns information about the group and who is in it
     * Message is intend for server to server comm.
     */
    class RequestGroupInfoHook implements Hook{
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage requestGroupInfoMsg = (ExtensionMessage) msg;
            CombatInfo subject = CombatPlugin.getCombatInfo(requestGroupInfoMsg.getSubject());
            if(subject == null)
                return false;

            Long sOid = subject.getOwnerOid();
            Lock lock = getObjectLockManager().getLock(sOid);
            lock.lock();

            try {
                HashSet<Long> groupMembers = HandleGroupInfoRequest(subject);
                Engine.getAgent().sendObjectResponse(msg, groupMembers);
            } finally {
                lock.unlock();
            }
            return true;
        }
    }
    
    /**
     * SetAllowedSpeakerInfoHook - used to set a whether the group member is allowed to talk in voice chat
     * 
     */
    class SetAllowedSpeakerHook implements Hook{
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage setAllowedSpeakerMsg = (ExtensionMessage)msg;

            Long targetOid = (Long)setAllowedSpeakerMsg.getProperty("target");
            Long setterOid = (Long)setAllowedSpeakerMsg.getProperty("setter");
            Long groupOid = (Long)setAllowedSpeakerMsg.getProperty("groupOid");

            return HandleSetAllowedSpeaker(targetOid, setterOid, groupOid);
        }
    }

    /**
     * MuteGrouphook - Used to mute or un-mute the group voice chat
     *                  If muting, only the group leader may talk
     */
    class MuteGroupHook implements Hook{
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage setAllowedSpeakerMsg = (ExtensionMessage)msg;

            Long setterOid = (Long)setAllowedSpeakerMsg.getProperty("setter");
            Long groupOid = (Long)setAllowedSpeakerMsg.getProperty("groupOid");
            return HandleMuteGroup(setterOid, groupOid);
        }
    }

    /**
     * VoiceStatusHook - Handles updates that determine if the player in a group has their voice
     *  configuration set to be enabled and to join group chat.
     *  Updates MarsGroupMember._voiceEnabled and broadcasts that to the rest of the group.
     */
    class VoiceStatusHook implements Hook{
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage voiceStatusMsg = (ExtensionMessage)msg;

            Long playerOid = (Long)voiceStatusMsg.getProperty("playerOid");
            Long groupOid = (Long)voiceStatusMsg.getProperty("groupOid");
            Boolean voiceEnabled = (Boolean)voiceStatusMsg.getProperty("voiceEnabled");

            return HandledVoiceStatus(playerOid, groupOid, voiceEnabled);
        }
    }

    /**
     * VoiceMemberAddedHook - Handles messages from the VoiceServer that 
     *  a new player was added to a voice group.
     */
    class VoiceMemberAddedHook implements Hook{
        public boolean processMessage (Message msg, int flags){
            ExtensionMessage voiceMemberAddedMsg = (ExtensionMessage)msg;
            Long memberOid = (Long)voiceMemberAddedMsg.getProperty("memberOid");
            Long groupOid = (Long)voiceMemberAddedMsg.getProperty("groupOid");
            return HandleVoiceMemberAdded(memberOid, groupOid);
        }
    }
}
