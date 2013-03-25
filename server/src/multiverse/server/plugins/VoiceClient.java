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
import multiverse.server.engine.*;
import multiverse.msgsys.MessageType;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;

/**
 * Client to communicate with the voice plugin
 */
public class VoiceClient {

    /**
     * Add a positional voice group.
     * @param groupOid The oid of the voice group to add.
     * @param positional True if the new group is a positional group;
     * false if it's a non-positional group.
     * @param maxVoices The maximum number of voice channels that may 
     * ever be simultaneously in use by any client.
     * @return The return code for the operation: SUCCESS or an error
     * return code.
     */
    public static int addVoiceGroup(long groupOid, boolean positional, int maxVoices) {
        return sendNewGroupMessage("addVoiceGroup", groupOid, positional, maxVoices);
    }

    /**
     * Remove a voice group.
     * @param groupOid The oid of the voice group to remove.
     * @return The return code for the operation: SUCCESS or an error
     * return code.
     */
    public static int removeVoiceGroup(long groupOid) {
        return sendVoicePluginRPC("removeVoiceGroup", groupOid);
    }

    /**
     * Return SUCCESS_TRUE if the group is a positional group; that
     * is, if membership in the group is determined by being near the
     * listener, and the priority of speakers is determined by how
     * near any speaker is.
     * @param groupOid The oid of the voice group.
     * @return The return code for the operation: SUCCESS_TRUE or
     * SUCCESS_FALSE, or an error return code.
     */
    public static int isPositional(long groupOid) {
        return sendVoicePluginRPC("isPositional", groupOid);
    }

    /**
     * Determine if the oid is that of a member of the group.
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of possible group member.
     * @param authToken The authentication token for voice connection
     * for this member.
     * @return The return code for the operation: SUCCESS_TRUE if
     * adding the member is allowed; SUCCESS_FALSE otherwise
     */
    public int addMemberAllowed(long groupOid, long memberOid, String authToken) {
        ExtensionMessage msg = makeVoicePluginMessage("addMemberAllowed", groupOid, memberOid);
        msg.setProperty("authToken", authToken);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }
    
    /**
     * Supply a list of oids of players allowed to be in the group
     * @param groupOid The oid of the voice group.
     * @param allowedMembers The set of oids of possible group members.
     * @return SUCCESS if the operation completed; otherwise an error
     * return code.
     */
    public int setAllowedMembers(long groupOid, Set<Long> allowedMembers) {
        ExtensionMessage msg = makeVoicePluginMessage("addMemberAllowed", groupOid);
        msg.setProperty("allowedMembers", (Serializable)allowedMembers);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }

    /**
     * Return the allowed members
     * @param groupOid The oid of the voice group.
     * @return A Set of allowed members, or an error code.
     */
    public Set<Long> getAllowedMembers(long groupOid) {
        ExtensionMessage msg = makeVoicePluginMessage("getAllowedMembers", groupOid);
        return (Set<Long>)Engine.getAgent().sendRPCReturnObject(msg);
    }
    
    /**
     * Add a member to a group
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of the newly-created member
     * @param priority The speaking priority that the member should be
     * assigned; speakers with higher priorities will be heard over
     * speakers with lower priorities.
     * @param allowedSpeaker True if the new member should be allowed
     * to speak; false otherwise.
     * @return The return code for the operation: SUCCESS or an error
     * return code.
     */
    public static int addMember(long groupOid, long memberOid, int priority, boolean allowedSpeaker) {
        return sendAddMemberMessage(groupOid, memberOid, priority, allowedSpeaker);
    }

    /** 
     * Find a member in the group by oid.
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of the member to find
     * @return The return code for the operation: SUCCESS_TRUE or
     * SUCCESS_FALSE, or an error return code.
     */
    public static int isMember(long groupOid, long memberOid) {
        return sendVoicePluginRPC("isMember", groupOid, memberOid);
    }

    /**
     * Remove the member of the group identified by the given oid, and
     * return true if that member was found in the group.
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of the member to remove.
     * @return The return code for the operation: SUCESS or an error
     * return code.
     */
    public static int removeMember(long groupOid, long memberOid) {
        return sendVoicePluginRPC("isPositional", groupOid, memberOid);
    }
    
    /** 
     * Is the member speaking?
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of the member who speaking status should be returned.
     * @return The return code for the operation: SUCCESS_TRUE or
     * SUCCESS_FALSE, or an error return code.
     */
    public static int isMemberSpeaking(long groupOid, long memberOid) {
        return sendVoicePluginRPC("isMemberSpeaking", groupOid, memberOid);
    }
        
    /** 
     * Is the member listening?
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of the member who listening status should be returned.
     * @return The return code for the operation: SUCCESS_TRUE or
     * SUCCESS_FALSE, or an error return code.
     */
    public static int isListener(long groupOid, long memberOid) {
        return sendVoicePluginRPC("isListener", groupOid, memberOid);
    }
    
    /**
     * Change the member with the given oid to be an allowed speaker
     * if add is true, or not an allowed speaker if add is false.
     * @param groupOid The oid of the voice group.
     * @param memberOid The member whose allowed speaker status should
     * change.
     * @param add If true, make the member an allowed speaker; if
     * false, stop allowing the member to be a speaker.
     * @return The return code for the operation: SUCCESS or an error
     * return code.
     */
    public static int setAllowedSpeaker(long groupOid, long memberOid, boolean add) {
        return sendVoicePluginRPC("setAllowedSpeaker", groupOid, memberOid, add);
    }

    /**
     * Change the member with the given oid, who must be an allowed
     * speaker, to speak if add is true, or to stop speaking, if add
     * is false.  Reflect any changes in who is and isn't speaking by
     * sending out the appropriate voice deallocations and voice
     * allocations to users.  Calls to this method are what generates
     * voice allocation and deallocation traffic to the clients in
     * response to speakers starting and stopping speaking.
     * @param groupOid The oid of the voice group.
     * @param memberOid The member whose speaking status should
     * change.
     * @param add If true, make the member a speaker in the group,
     * if add is True, else stop the member speaking.
     * @return The return code for the operation: SUCCESS or an error
     * return code.
     */
    public static int setMemberSpeaking(long groupOid, long memberOid, boolean add) {
        return sendVoicePluginRPC("setMemberSpeaking", groupOid, memberOid, add);
    }

    /**
     * Change the member with the given oid to be a listener if add is
     * true, or not a listener if add is false.
     * @param groupOid The oid of the voice group.
     * @param memberOid The member whose whose status should
     * change.
     * @param add If true, make the member a listener in the group,
     * if add is True, else stop the member listening.
     * @return The return code for the operation: SUCCESS or an error
     * return code.
     */
    public static int setListener(long groupOid, long memberOid, boolean add) {
        return sendVoicePluginRPC("setListener", groupOid, memberOid, add);
    }
        
    /*
     * These are unreferenced right now.  Should they be eliminated,
     * since their value can change immediately afterwards anyway?
     */

    /** 
     * Is the member allowed to speak?
     * @param groupOid The oid of the voice group.
     * @param memberOid The oid of the member who allowed speaking status should be returned.
     * @return The return code for the operation: SUCCESS_TRUE or
     * SUCCESS_FALSE, or an error return code.
     */
    public static int isAllowedSpeaker(long groupOid, long memberOid) {
        return sendVoicePluginRPC("isAllowedSpeaker", groupOid, memberOid);
    }

    /** 
     * What group does the member belong to?
     * @param memberOid The oid of the member whose groupOid should be returned.
     * @return The groupOid of the player, or null if the player is
     * not currently associated with a group.
     */
    public static Long getPlayerGroup(long memberOid) {
        ExtensionMessage msg = makeVoicePluginMessage("getPlayerGroup", memberOid);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }     
    
    /**
     * Translate an error code into a string
     */
    public static String errorString(int errorCode) {
        switch (errorCode) {
        case ERROR_NO_SUCH_GROUP:
            return "There is no group with the supplied groupOid";
        case ERROR_GROUP_ALREADY_EXISTS:
            return "The group with the supplied groupOid already exists";
        case ERROR_NO_SUCH_MEMBER:
            return "There is no member in the group identified by the supplied groupOid with the supplied memberOid";
        case ERROR_MEMBER_ALREADY_EXISTS:
            return "There is already a member with the supplied memberOid in the group identified by the groupOid";
        case ERROR_NO_SUCH_OPCODE:
            return "The VoicePlugin doesn't recognize the supplied voice message opcode";
        case ERROR_PLAYER_NOT_CONNECTED:
            return "The player identified by the memberOid is not currently connected to the VoicePlugin";
        case ERROR_MISSING_ADD_PROPERTY:
            return "There is no 'add' property in the message";
        case ERROR_MISSING_MAX_VOICES:
            return "There is no 'maxVoices' property in the message";
        case ERROR_MISSING_POSITIONAL:
            return "There is no 'positional' property in the message";
        default:
            return "No error corresponding to the supplied error code";
        }
    }

    /*
     * The low-level plumbing that sends messages and returns return
     * codes.
     */

    protected static ExtensionMessage makeVoicePluginMessage(String opcode, long groupOid) {
        ExtensionMessage msg = new ExtensionMessage();
        msg.setMsgType(MSG_TYPE_VOICECLIENT);
        msg.setProperty("opcode", opcode);
        msg.setProperty("groupOid", groupOid);
        return msg;
    }
    
    protected static ExtensionMessage makeVoicePluginMessage(String opcode, long groupOid, long memberOid) {
        ExtensionMessage msg = makeVoicePluginMessage(opcode, groupOid);
        msg.setProperty("memberOid", memberOid);
        return msg;
    }

    protected static ExtensionMessage makeVoicePluginMessage(String opcode, long groupOid, long memberOid, boolean add) {
        ExtensionMessage msg = makeVoicePluginMessage(opcode, groupOid, memberOid);
        msg.setProperty("add", add);
        return msg;
    }

    protected static int sendNewGroupMessage(String opcode, long groupOid, boolean positional, int maxVoices) {
        ExtensionMessage msg = makeVoicePluginMessage(opcode, groupOid);
        msg.setProperty("positional", positional);
        msg.setProperty("maxVoices", maxVoices);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }

    protected static int sendAddMemberMessage(long groupOid, long memberOid, int priority, boolean allowedSpeaker) {
        ExtensionMessage msg = makeVoicePluginMessage("addMember", groupOid, memberOid);
        msg.setProperty("priority", priority);
        msg.setProperty("allowedSpeaker", allowedSpeaker);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }

    protected static int sendVoicePluginRPC(String opcode, long groupOid) {
        ExtensionMessage msg = makeVoicePluginMessage(opcode, groupOid);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }
    
    protected static int sendVoicePluginRPC(String opcode, long groupOid, long memberOid) {
        ExtensionMessage msg = makeVoicePluginMessage(opcode, groupOid, memberOid);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }
    
    protected static int sendVoicePluginRPC(String opcode, long groupOid, long memberOid, boolean add) {
        ExtensionMessage msg = makeVoicePluginMessage(opcode, groupOid, memberOid, add);
        return Engine.getAgent().sendRPCReturnInt(msg);
    }
    
    /**
     * The ExtensionMessages sent by the VoiceClient all have this
     * type, so that the VoicePlugin(s) don't get deluged with
     * ExtensionMessages intended for other plugins.  The only
     * downside is that plugins that use VoiceClient will have to add
     * this message type to the "published" messages of the plugin.
     */
    public static MessageType MSG_TYPE_VOICECLIENT = MessageType.intern("mv.VOICECLIENT");

    /**
     * The MessageType of the message sent by a voice group to notify
     * of addition of a member to the group.
     */
    public static MessageType MSG_TYPE_VOICE_MEMBER_ADDED = MessageType.intern("mv.VOICE_MEMBER_ADDED");

    /**
     * The MessageType of the message sent by a voice group to notify
     * removal of a member from the group.
     */
    public static MessageType MSG_TYPE_VOICE_MEMBER_REMOVED = MessageType.intern("mv.VOICE_MEMBER_REMOVED");

    /**
     * An operation with no return value succeeded
     */
    public static final int SUCCESS = 1;

    /**
     * A boolean operation returned true
     */
    public static final int SUCCESS_TRUE = 2;

    /**
     * A boolean operation returned false
     */
    public static final int SUCCESS_FALSE = 3;

    /**
     * Error: no such group
     */
    public static final int ERROR_NO_SUCH_GROUP = -1;

    /**
     * Error: group already exists
     */
    public static final int ERROR_GROUP_ALREADY_EXISTS = -2;

    /**
     * Error: no such member of the group
     */
    public static final int ERROR_NO_SUCH_MEMBER = -3;
    
    /**
     * Error: member already exists in the group
     */
    public static final int ERROR_MEMBER_ALREADY_EXISTS = -4;
    
    /**
     * Error: the VoicePlugin doesn't understand the message opcode
     */
    public static final int ERROR_NO_SUCH_OPCODE = -5;

    /**
     * Error: we tried to add a member who isn't connected to the VoicePlugin.
     */
    public static final int ERROR_PLAYER_NOT_CONNECTED = -6;
    
    /**
     * Error: we tried perform an operation that requires a boolean
     * "add" property, but didn't find one.
     */
    public static final int ERROR_MISSING_ADD_PROPERTY = -7;
    
    /**
     * Error: we tried to create a new group but there was no "maxVoices" parm
     */
    public static final int ERROR_MISSING_MAX_VOICES = -8;

    /**
     * Error: we tried to create a new group but there was no "positional" parm
     */
    public static final int ERROR_MISSING_POSITIONAL = -9;
    
}

