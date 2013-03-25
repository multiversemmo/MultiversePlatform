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
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import multiverse.mars.objects.QuestState;
import multiverse.server.engine.*;
import multiverse.msgsys.*;

public class QuestClient {
 
    /**
     * makes a request to the questplugin to get the quest completion status for the passed
     * in quest ids for a given player
     * @param playerOid player
     * @param questRefs name of quests we want the status for
     */
    public static Map<String,Byte> getQuestStatus(Long playerOid, Collection<String> questRefs) {
        GetQuestStatusMessage msg = new GetQuestStatusMessage(playerOid, questRefs);
        Map<String, Byte> questStatusMap = (Map<String,Byte>) Engine.getAgent().sendRPCReturnObject(msg);
        return questStatusMap;
    }
    
    public static void requestQuestInfo(Long mobOid, Long playerOid) {
        RequestQuestInfoMessage msg = new RequestQuestInfoMessage(mobOid, playerOid);
        Engine.getAgent().sendBroadcast(msg);
    }
    
    public static void requestConclude(Long mobOid, Long playerOid) {
        RequestConcludeMessage msg = new RequestConcludeMessage(mobOid, playerOid);
        Engine.getAgent().sendBroadcast(msg);
    }

    public static void resetQuests(Long playerOid) {
	SubjectMessage msg = new SubjectMessage(MSG_TYPE_REQ_RESET_QUESTS, playerOid);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * the quest plugin (usually via quest state object) has updated its state,
     * and is alerting others (usually quest behavior) so that they can let the player know
     * if their available actions have changed (such as ability to turn in a quest)
     * @author cedeno
     */
    public static class StateStatusChangeMessage extends SubjectMessage {

        public StateStatusChangeMessage() {
            super(MSG_TYPE_QUEST_STATE_STATUS_CHANGE);
        }

        public StateStatusChangeMessage(Long playerOid, String questRef) {
            super(MSG_TYPE_QUEST_STATE_STATUS_CHANGE, playerOid);
            setQuestRef(questRef);
        }
        
        public String getQuestRef() {
            return questRef;
        }

        public void setQuestRef(String questRef) {
            this.questRef = questRef;
        }
        String questRef;
        
        private static final long serialVersionUID = 1L;
    }


    public static class RequestConcludeMessage extends SubjectMessage {

        public RequestConcludeMessage() {
            super(MSG_TYPE_REQ_CONCLUDE_QUEST);
        }

        public RequestConcludeMessage(Long mobOid, Long playerOid) {
            super(MSG_TYPE_REQ_CONCLUDE_QUEST, mobOid);
            setPlayerOid(playerOid);
        }

        public Long getPlayerOid() {
            return playerOid;
        }

        public void setPlayerOid(Long playerOid) {
            this.playerOid = playerOid;
        }
        Long playerOid;

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * mob (quest behavior) is telling us (usually quest state obj) that the quest has been concluded
     * player is subject because it is going to the player's quest state
     * @author cedeno
     *
     */
    public static class ConcludeMessage extends SubjectMessage {

        public ConcludeMessage() {
            super(MSG_TYPE_CONCLUDE_QUEST);
        }

        public ConcludeMessage(Long playerOid, Long mobOid, Long questOid) {
            super(MSG_TYPE_CONCLUDE_QUEST, playerOid);
            setMobOid(mobOid);
            setQuestOid(questOid);
        }

        public Long getMobOid() {
            return mobOid;
        }

        public void setMobOid(Long mobOid) {
            this.mobOid = mobOid;
        }
        Long mobOid;

        public Long getQuestOid() {
            return questOid;
        }

        public void setQuestOid(Long questOid) {
            this.questOid = questOid;
        }
        Long questOid;

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * (usually quest behavior) asking quest plugin for the status of various quest states
     * @author cedeno
     *
     */
    public static class GetQuestStatusMessage extends SubjectMessage {

        public GetQuestStatusMessage() {
            super(MSG_TYPE_GET_QUEST_STATUS);
        }
        
        public GetQuestStatusMessage(Long playerOid, Collection<String> questRefs) {
            super(MSG_TYPE_GET_QUEST_STATUS, playerOid);
            setQuestRefs(questRefs);
        }

        List<String> questRefs = new LinkedList<String>();

        public List<String> getQuestRefs() {
            return questRefs;
        }

        public void setQuestRefs(Collection<String> questRefs) {
            this.questRefs = new LinkedList<String>(questRefs);
        }

        private static final long serialVersionUID = 1L;
    }

    /**
     * (usually proxy plugin) asking for quest info
     * @author cedeno
     *
     */
    public static class RequestQuestInfoMessage extends SubjectMessage {

        public RequestQuestInfoMessage() {
            super(MSG_TYPE_REQ_QUEST_INFO);
        }

        RequestQuestInfoMessage(Long npcOid, Long playerOid) {
            super(MSG_TYPE_REQ_QUEST_INFO, npcOid);
            setPlayerOid(playerOid);
        }
        
        Long playerOid = null;

        public Long getPlayerOid() {
            return playerOid;
        }

        public void setPlayerOid(Long playerOid) {
            this.playerOid = playerOid;
        }

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * client is responding to server, accepting or declining quest
     * @author cedeno
     *
     */
    public static class QuestResponseMessage extends SubjectMessage {

        public QuestResponseMessage() {
            super(MSG_TYPE_QUEST_RESP);
        }

        public QuestResponseMessage(Long npcOid, Long playerOid, boolean acceptStatus) {
            super(MSG_TYPE_QUEST_RESP, npcOid);
            setPlayerOid(playerOid);
            setAcceptStatus(acceptStatus);
        }
        
        public Boolean getAcceptStatus() {
            return acceptStatus;
        }
        public void setAcceptStatus(Boolean acceptStatus) {
            this.acceptStatus = acceptStatus;
        }
        public Long getPlayerOid() {
            return playerOid;
        }
        public void setPlayerOid(Long playerOid) {
            this.playerOid = playerOid;
        }
        
        private Boolean acceptStatus;
        private Long playerOid;

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * client accepted a quest, so the quest behavior has created a quest state object
     * and is now alerting the quest plugin about it so it can keep track of it
     * @author cedeno
     *
     */
    public static class NewQuestStateMessage extends SubjectMessage {

        public NewQuestStateMessage() {
            super(MSG_TYPE_NEW_QUESTSTATE);
        }
        
        public NewQuestStateMessage(Long playerOid, QuestState questState) {
            super(MSG_TYPE_NEW_QUESTSTATE, playerOid);
            setQuestState(questState);
        }

        public QuestState getQuestState() {
            return questState;
        }

        public void setQuestState(QuestState questState) {
            this.questState = questState;
        }
        
        private QuestState questState;

        private static final long serialVersionUID = 1L;
    }

    // Enumerated values of QuestStatus
    public static final byte QuestStatusDNE = 1;
    public static final byte QuestStatusInProgress = 2;
    public static final byte QuestStatusCompleted = 3;
    public static final byte QuestStatusConcluded = 4;
    
    public static final MessageType MSG_TYPE_REQ_QUEST_INFO = MessageType.intern("mv.REQ_QUEST_INFO");
    public static final MessageType MSG_TYPE_REQ_CONCLUDE_QUEST = MessageType.intern("mv.REQ_CONCLUDE_QUEST");
    public static final MessageType MSG_TYPE_QUEST_INFO = MessageType.intern("mv.QUEST_INFO");
    public static final MessageType MSG_TYPE_GET_QUEST_STATUS = MessageType.intern("mv.GET_QUEST_STATUS");
    public static final MessageType MSG_TYPE_QUEST_RESP = MessageType.intern("mv.QUEST_RESP");
    public static final MessageType MSG_TYPE_NEW_QUESTSTATE = MessageType.intern("mv.NEW_QUESTSTATE");
    public static final MessageType MSG_TYPE_CONCLUDE_QUEST = MessageType.intern("mv.CONCLUDE_QUEST");
    public static final MessageType MSG_TYPE_QUEST_STATE_STATUS_CHANGE = MessageType.intern("mv.QUEST_STATE_STATUS_CHANGE");
    public static final MessageType MSG_TYPE_QUEST_LOG_INFO = MessageType.intern("mv.QUEST_LOG_INFO");
    public static final MessageType MSG_TYPE_QUEST_STATE_INFO = MessageType.intern("mv.QUEST_STATE_INFO");
    public static final MessageType MSG_TYPE_REMOVE_QUEST_RESP = MessageType.intern("mv.REMOVE_QUEST_RESP");
    public static final MessageType MSG_TYPE_REQ_RESET_QUESTS = MessageType.intern("mv.REQ_RESET_QUESTS");
}
