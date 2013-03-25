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

import java.util.Collection;
import java.util.HashSet;

import multiverse.mars.objects.MarsGroup;
import multiverse.mars.objects.MarsGroupMember;
import multiverse.msgsys.MessageAgent;
import multiverse.msgsys.MessageType;
import multiverse.server.engine.Engine;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedComMessage;
import multiverse.server.util.Log;

public class GroupClient {
    //properties
    public static final String EXTMSG_GROUP_UPDATE = "mv.GROUP_UPDATE";
    public static final String EXTMSG_GROUP_PROPERTY_UPDATE = "mv.GROUP_PROPERTY_UPDATE";
    public static final String EXTMSG_GROUP_INVITE_REQUEST = "mv.GROUP_INVITE_REQUEST";
    public static final String EXTMSG_GROUP_INVITE_DECLINED = "mv.GROUP_INVITE_DECLINED";
    public static final MessageType MSG_TYPE_GROUP_INVITE = MessageType.intern("mv.GROUP_INVITE");
    public static final MessageType MSG_TYPE_GROUP_INVITE_RESPONSE = MessageType.intern("mv.GROUP_INVITE_RESPONSE");
    public static final MessageType MSG_TYPE_GROUP_REMOVE_MEMBER = MessageType.intern("mv.GROUP_REMOVE_MEMBER");
    public static final MessageType MSG_TYPE_GROUP_CHAT = MessageType.intern("mv.GROUP_CHAT");
    public static final MessageType MSG_TYPE_REQUEST_GROUP_INFO = MessageType.intern("mv.REQUEST_GROUP_INFO");
    public static final MessageType MSG_TYPE_GROUP_INFO_RESPONSE = MessageType.intern("mv.GROUP_INFO_RESPONSE");
    public static final MessageType MSG_TYPE_GROUP_SET_ALLOWED_SPEAKER = MessageType.intern("mv.GROUP_SET_ALLOWED_SPEAKER");
    public static final MessageType MSG_TYPE_GROUP_MUTE_VOICE_CHAT = MessageType.intern("mv.GROUP_MUTE_VOICE_CHAT");
    public static final MessageType MSG_TYPE_GROUP_VOICE_CHAT_STATUS = MessageType.intern("mv.GROUP_VOICE_CHAT_STATUS");
    //constructors
    public GroupClient(){}

    /*
     * Methods
     */

    /**
     * SendGroupEventMessage is used to send messages to each group member about specific group releated events
     * @param eventType Type of event to send to the group members
     * @param group Group object for which the event pertains
     * @param subjectOid Oid of the player/object that the message is about
     */
    public static void SendGroupEventMessage(GroupEventType eventType, MarsGroup group, long subjectOid){
        //Set Message to send to each group member
        MarsGroupMember subject = group.GetGroupMember(subjectOid);
        if(subject != null){
            String message = subject.GetGroupMemberName();
            switch(eventType){
            case JOINED:
                message += " has joined the group.";
                break;
            case LEFT:
                message += " has left the group.";
                break;
            case DISBANDED:
                message += " has disbanded the group";
                break;
            case LEADERCHANGED:
                message += " is now the group leader.";
                break;
            case MUTED:                
                message += " has muted the group.";
                break;
            case UNMUTED:
                message += " has un-muted the group.";
                break;
            }

            MessageAgent agent = Engine.getAgent();
            TargetedComMessage groupEventMessage = new TargetedComMessage();
            groupEventMessage.setString(message);
            groupEventMessage.setChannel(4); //Group channel
            Collection<MarsGroupMember> groupMembers = group.GetGroupMembers().values();
            for(MarsGroupMember groupMember : groupMembers){
                if(groupMember.GetGroupMemberOid() != subjectOid){                
                    groupEventMessage.setSubject(groupMember.GetGroupMemberOid());
                    groupEventMessage.setTarget(groupMember.GetGroupMemberOid());
                    agent.sendBroadcast(groupEventMessage);
                }
            }
        } else {
            Log.error("GroupClient.SendGroupEventMessage - MarsGroup.GetGroupMember(" + subjectOid +
                      ") returned null object");
        }
    }

    /**
     * Sends an RPC message to the GroupPlugin and returns a list of group member OIDs.
     * @param subject Oid of the player/object assoicated with the group you want info about
     * 
     */
    public static HashSet<Long> GetGroupMemberOIDs(Long subject){
        ExtensionMessage groupInfoRequest = new ExtensionMessage(GroupClient.MSG_TYPE_REQUEST_GROUP_INFO,
                                                                 "mv.REQUEST_GROUP_INFO", subject);

        Object groupMembers = Engine.getAgent().sendRPCReturnObject(groupInfoRequest);
        if(Log.loggingDebug)
            Log.debug("GroupClient.GetGroupMemberOIDs - Received group info - " + groupMembers.toString());
        return (HashSet<Long>)groupMembers;
    }

    /*
     * Enumerations
     */

    public enum GroupEventType {
        JOINED, LEFT, DISBANDED, LEADERCHANGED, MUTED, UNMUTED
    }
}
