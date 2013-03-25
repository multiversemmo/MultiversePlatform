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

package multiverse.mars.behaviors;

import java.util.*;

import multiverse.mars.objects.MarsQuest;
import multiverse.mars.objects.MarsStates;
import multiverse.mars.objects.QuestState;
import multiverse.mars.plugins.QuestClient;
import multiverse.mars.plugins.QuestClient.*;
import multiverse.mars.plugins.QuestPlugin;
import multiverse.msgsys.*;
import multiverse.server.plugins.InventoryClient;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.plugins.WorldManagerClient.TargetedPropertyMessage;
import multiverse.server.objects.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;

public class QuestBehavior extends Behavior implements MessageCallback {

    public void initialize() {

        Long mobOid = this.getObjectStub().getOid();
        if (Log.loggingDebug)
            log.debug("QuestBehavior.initialize: my moboid=" + mobOid);

        SubjectFilter filter = new SubjectFilter(mobOid);
        filter.addType(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT);
        filter.addType(QuestClient.MSG_TYPE_REQ_QUEST_INFO);
        filter.addType(QuestClient.MSG_TYPE_QUEST_RESP);
        filter.addType(QuestClient.MSG_TYPE_REQ_CONCLUDE_QUEST);
        eventSub = Engine.getAgent().createSubscription(filter, this);
        Log.debug("QuestBehavior: created subject filter for oid=" + mobOid);

        // Subscribe to all state status change messages. This is inefficient, but it works.
        MessageTypeFilter statusFilter = new MessageTypeFilter(QuestClient.MSG_TYPE_QUEST_STATE_STATUS_CHANGE);
        statusSub = Engine.getAgent().createSubscription(statusFilter, this);
    }

    public void activate() {
    }

    public void deactivate() {
        lock.lock();
        try {
            if (eventSub != null) {
                Engine.getAgent().removeSubscription(eventSub);
                eventSub = null;
            }
            if (statusSub != null) {
                Engine.getAgent().removeSubscription(statusSub);
                statusSub = null;
            }
        } finally {
            lock.unlock();
        }
    }

    public void handleMessage(Message msg, int flags) {
        if (msg instanceof WorldManagerClient.UpdateMessage) {
            WorldManagerClient.UpdateMessage updateMsg = (WorldManagerClient.UpdateMessage) msg;
                processUpdateMsg(updateMsg);
        } else if (msg instanceof QuestClient.RequestQuestInfoMessage) {
            QuestClient.RequestQuestInfoMessage reqMsg = (QuestClient.RequestQuestInfoMessage) msg;
            processReqQuestInfoMsg(reqMsg);
        } else if (msg instanceof QuestClient.QuestResponseMessage) {
            QuestClient.QuestResponseMessage respMsg = (QuestClient.QuestResponseMessage) msg;
            processQuestRespMsg(respMsg);
        } else if (msg instanceof QuestClient.RequestConcludeMessage) {
            QuestClient.RequestConcludeMessage reqMsg = (QuestClient.RequestConcludeMessage) msg;
            processReqConcludeMsg(reqMsg);
        } else if (msg instanceof QuestClient.StateStatusChangeMessage) {
            QuestClient.StateStatusChangeMessage nMsg = (QuestClient.StateStatusChangeMessage) msg;
            processStateStatusChangeMsg(nMsg);
        } else {
            log.error("onMessage: got unknown msg: " + msg);
            return; //return false;
        }
        //return true;
    }

    private void processStateStatusChangeMsg(QuestClient.StateStatusChangeMessage msg) {
        Long playerOid = msg.getSubject();
        String questRef = msg.getQuestRef();
        if (Log.loggingDebug)
            log.debug("processStateStatusChangeMsg: myOid=" + getObjectStub().getOid()
                      + " playerOid=" + playerOid + " questRef=" + questRef);
        
        Map<String,Byte> questStatusMap = QuestClient.getQuestStatus(playerOid, getAllQuestRefs());
        handleQuestState(playerOid, questStatusMap);
    }
    
    private void processReqConcludeMsg(QuestClient.RequestConcludeMessage msg) {
        Long myOid = getObjectStub().getOid();
        Long playerOid = msg.getPlayerOid();
        
        if (Log.loggingDebug)
            log.debug("processReqConcludeMsg: mob=" + myOid + ", player=" + playerOid);
        
        // find a completed quest
        MarsQuest completedQuest = null;
        Map<String,Byte> questStatusMap = QuestClient.getQuestStatus(playerOid, getAllQuestRefs());
        for (String questRef : questStatusMap.keySet()) {
            byte status = questStatusMap.get(questRef);
            if (Log.loggingDebug)
                log.debug("processReqConcludedMsg: checking status for quest " + questRef + ", status=" + status);
            if (status == QuestClient.QuestStatusCompleted) {
                // found the quest
                completedQuest = getEndQuest(questRef);
                if (completedQuest != null) {
                    if (Log.loggingDebug)
                        log.debug("processReqConcludeMsg: found a completed quest: " + questRef);
                    break;
                }
                else {
                    log.warn("processReqConcludeMsg: quest is completed, but not in end quests");
                }
            }
        }
        if (completedQuest == null) {
            log.warn("processReqConcludedMsg: did not find completed quest");
            return;
        }

        QuestClient.ConcludeMessage concludeMsg = new QuestClient.ConcludeMessage(playerOid, myOid, completedQuest.getOid());
        Engine.getAgent().sendBroadcast(concludeMsg);
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "You have concluded my quest, many thanks");

        // Normally the persistence flag is inherited from the enclosing
        // object, but all we have are OIDs.  Assume this is only used
        // for players and players are always persistent.
        Template overrideTemplate = new Template();
        overrideTemplate.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, true);

        // generate the reward item
        List<String> rewards = completedQuest.getRewards();
        for (String rewardTemplate : rewards) {
            if (Log.loggingDebug)
                Log.debug("processReqConcludedMsg: createitem: templ=" + rewardTemplate + ", generating object");
            Long itemOid = ObjectManagerClient.generateObject(rewardTemplate, overrideTemplate);
            // add to inventory
            Long bagOid = playerOid;
            if (Log.loggingDebug)
                Log.debug("processReqConcludedMsg: createitem: oid=" + itemOid + ", bagOid=" + bagOid + ", adding to inventory");
            boolean rv = InventoryClient.addItem(bagOid, playerOid, bagOid, itemOid);
            if (Log.loggingDebug)
                Log.debug("processReqConcludedMsg: createitem: oid=" + itemOid + ", added, rv=" + rv);
        }
        // update the quest status map so we can re-use it in the handleQuestState call
        // this will update the state for the client (you can probably no longer conclude a 
        // quest with this npc)
        questStatusMap.put(completedQuest.getName(), QuestClient.QuestStatusConcluded);
        handleQuestState(playerOid, questStatusMap);

        MarsQuest chainQuest = completedQuest.getChainQuest();
        if (chainQuest != null && isQuestAvailableHelper(chainQuest, questStatusMap)) {
            offerQuestToPlayer(playerOid, chainQuest);
        }
    }
    
    private void processQuestRespMsg(QuestResponseMessage msg) {
        Long myOid = getObjectStub().getOid();
        Long playerOid = msg.getPlayerOid();
        Boolean acceptStatus = msg.getAcceptStatus();

        Log.debug("processQuestResp: player=" + playerOid + " mob=" + myOid + " acceptStatus=" + acceptStatus);
        // find out what quest they are responding to
        MarsQuest quest;
        lock.lock();
        try {
            quest = offeredQuestMap.remove(playerOid);
        }
        finally {
            lock.unlock();
        }
        if (! acceptStatus) {
            if (Log.loggingDebug)
                log.debug("processQuestRespMsg: player " + playerOid + " declined quest for mob " + myOid);
            return;
        }
        if (quest == null) {
            log.error("mob " + myOid + " hasnt offered player " + playerOid + " any quests");
            return;
        }
        
        // create a quest state object
        if (Log.loggingDebug)
            log.debug("processQuestRespMsg: player " + playerOid + " has accepted quest " + quest + ", by mob " + myOid);
        final QuestState qs = quest.generate(playerOid);
        if (Log.loggingDebug)
            log.debug("processQuestRespMsg: sending new quest state msg: " + qs);
        
        QuestClient.NewQuestStateMessage qsMsg = new QuestClient.NewQuestStateMessage(playerOid, qs);
        log.debug("processQuestRespMsg: waiting for response msg");
        // wait for response before updating quest availability
        Engine.getAgent().sendRPC(qsMsg);
        
        // update the players quest availability info
        log.debug("processQuestRespMsg: updating availability");
        Map<String,Byte> questStatusMap = QuestClient.getQuestStatus(playerOid, getAllQuestRefs());
        handleQuestState(playerOid, questStatusMap);
    }
    
     private boolean isQuestAvailableHelper(MarsQuest quest, Map<String, Byte> questStatusMap) {
         Byte status = questStatusMap.get(quest.getName());
         if (status != QuestClient.QuestStatusDNE) {
             return false;
         }
         for (String prereq : quest.getQuestPrereqs()) {
             Byte prereqStatus = questStatusMap.get(prereq);
             if (prereqStatus != QuestClient.QuestStatusConcluded) {
                 return false;
             }
         }
         return true;
     }
 
     protected void offerQuestToPlayer(Long playerOid, MarsQuest quest) {
          Long myOid = getObjectStub().getOid();
          if (Log.loggingDebug)
              log.debug("offerQuestToPlayer: sending quest info for quest: " + quest);
        lock.lock();
        try {
            offeredQuestMap.put(playerOid, quest);
        }
        finally {
            lock.unlock();
        }

        QuestPlugin.sendQuestInfo(playerOid, myOid, quest.getOid(), quest.getName(), quest.getDesc(),
                                  quest.getObjective(), quest.getRewards());
    }

    private void processReqQuestInfoMsg(RequestQuestInfoMessage reqMsg) {
        Long myOid = getObjectStub().getOid();
        Long playerOid = reqMsg.getPlayerOid();
         
        // get quest states for this player
        MarsQuest offerQuest = null; // this will be set to the quest we want to offer
        Map<String,Byte> questStatusMap = QuestClient.getQuestStatus(playerOid, getStartQuestRefs());
        for (MarsQuest q : getStartQuests()) {
            if (isQuestAvailableHelper(q, questStatusMap)) {
                offerQuest = q;
                break;
            }
        }
         
        if (offerQuest == null) {
            if (Log.loggingDebug)
                log.debug("processReqQuestInfoMsg: playerOid=" + playerOid + ", mobOid=" + myOid + ", no quest to offer");
            return;
        }
        offerQuestToPlayer(playerOid, offerQuest);
    }
 
    public void processUpdateMsg(WorldManagerClient.UpdateMessage msg) {
        Long myOid = msg.getSubject();
        Long playerOid = msg.getTarget();

        if (Log.loggingDebug)
            log.debug("processUpdateMsg: myOid=" + myOid + ", playerOid=" + playerOid);

        if (!myOid.equals(this.getObjectStub().getOid())) {
            log.debug("processUpdateMsg: oids dont match!");
        }

        Map<String,Byte> questStatusMap = QuestClient.getQuestStatus(playerOid, getAllQuestRefs());
        handleQuestState(playerOid, questStatusMap);
    }

    protected void handleQuestState(Long playerOid, Map<String,Byte> questStatusMap) {
        Long myOid = getObjectStub().getOid();
        
        // ask the quest plugin if the player has completed the quests we can give out
        Collection<MarsQuest> startQuests = getStartQuests();
        Collection<MarsQuest> endQuests = getEndQuests();
        
        if (startQuests.isEmpty() && endQuests.isEmpty()) {
            // mob has no quests
            if (Log.loggingDebug)
                log.debug("QuestBehavior.handleQuestState: playerOid=" + playerOid + " has no quests, returning");
            return;
        }
        
        if (Log.loggingDebug)
            log.debug("QuestBehavior.handleQuestState: getting quest status for player=" + playerOid + ", starts "
                      + startQuests.size() + " quests, ends " + endQuests.size() + " quests");

        boolean hasAvailableQuest = false;
        boolean hasConcludableQuest = false;

        for (MarsQuest q : startQuests) {
            byte status = questStatusMap.get(q.getName());
            if (status == QuestClient.QuestStatusDNE) {
                boolean available = true;
                for (String prereq : q.getQuestPrereqs()) {
                    byte prereqStatus = questStatusMap.get(prereq);
                    if (prereqStatus != QuestClient.QuestStatusConcluded) {
                        if (Log.loggingDebug)
                            log.debug("QuestBehavior.handleQuestState: playerOid=" + playerOid + " startQuest=" + q
                                      + " missing prereq=" + prereq + " prereqStatus=" + prereqStatus);
                        available = false;
                    }
                }
                if (available) {
                    if (Log.loggingDebug)
                        log.debug("QuestBehavior.handleQuestState: playerOid=" + playerOid + " startQuest=" + q
                                  + " quest is available");
                    hasAvailableQuest = true;
                    WorldManagerClient.sendObjChatMsg(playerOid, 0,
                                WorldManagerClient.getObjectInfo(myOid).name + " starts '" +  q.getName() + "'.");
                }
            }
            else {
                if (Log.loggingDebug)
                    log.debug("QuestBehavior.handleQuestState: playerOid=" + playerOid + " startQuest=" + q
                              + " questStatus=" + status);
            }
        }

        for (MarsQuest q : endQuests) {
            byte status = questStatusMap.get(q.getName());
            if (Log.loggingDebug)
                log.debug("QuestBehavior.handleQuestState: playerOid=" + playerOid + " endQuest=" + q
                          + " status=" + status);
            if (status == QuestClient.QuestStatusCompleted) {
                hasConcludableQuest = true;
                WorldManagerClient.sendObjChatMsg(playerOid, 0,
                    WorldManagerClient.getObjectInfo(myOid).name + " concludes '" + q.getName() + "'.");
            }
        }
        
        TargetedPropertyMessage propMsg = new TargetedPropertyMessage(playerOid, myOid);
        propMsg.setProperty(MarsStates.QuestAvailable.toString(), hasAvailableQuest);
        propMsg.setProperty(MarsStates.QuestConcludable.toString(), hasConcludableQuest);
        Engine.getAgent().sendBroadcast(propMsg);
    }
    
    public void startsQuest(MarsQuest quest) {
        lock.lock();
        try {
            startQuestsMap.put(quest.getName(), quest);
            if (Log.loggingDebug)
                log.debug("startsQuest: added quest " + quest);
        }
        finally {
            lock.unlock();
        }
    }
    public void endsQuest(MarsQuest quest) {
        lock.lock();
        try {
            endQuestsMap.put(quest.getName(), quest);
            if (Log.loggingDebug)
                log.debug("endsQuest: adding quest " + quest);
        }
        finally {
            lock.unlock();
        }
    }
    
   
    public MarsQuest getQuest(String questName) {
        lock.lock();
        try {
            MarsQuest q = startQuestsMap.get(questName);
            if (q != null) {
                return q;
            }
            return endQuestsMap.get(questName);
        }
        finally {
            lock.unlock();
        }
    }
    
    public MarsQuest getStartQuest(String questName) {
        lock.lock();
        try {
            return startQuestsMap.get(questName);
        }
        finally {
            lock.unlock();
        }
    }
    
    public MarsQuest getEndQuest(String questName) {
        lock.lock();
        try {
            return endQuestsMap.get(questName);
        }
        finally {
            lock.unlock();
        }
    }
    
    public Collection<MarsQuest> getStartQuests() {
        lock.lock();
        try {
            return new LinkedList<MarsQuest>(startQuestsMap.values());
        }
        finally {
            lock.unlock();
        }
    }
    public Collection<MarsQuest> getEndQuests() {
        lock.lock();
        try {
            return new LinkedList<MarsQuest>(endQuestsMap.values());
        }
        finally {
            lock.unlock();
        }
    }
    public Collection<MarsQuest> getAllQuests() {
        lock.lock();
        try {
            Set<MarsQuest> l = new HashSet<MarsQuest>();
            l.addAll(getStartQuests());
            l.addAll(getEndQuests());
            return l;
        }
        finally {
            lock.unlock();
        }
    }
    public Collection<String> getAllQuestRefs() {
        lock.lock();
        try {
            Collection<String> set = new HashSet<String>();
            for (MarsQuest q : getStartQuests()) {
                set.add(q.getName());
                set.addAll(q.getQuestPrereqs());
            }
            for (MarsQuest q : getEndQuests()) {
                set.add(q.getName());
            }
            return set;
        }
        finally {
            lock.unlock();
        }
    }
    public Collection<String> getStartQuestRefs() {
        lock.lock();
        try {
            Collection<String> set = new HashSet<String>();
            for (MarsQuest q : getStartQuests()) {
                set.add(q.getName());
                set.addAll(q.getQuestPrereqs());
            }
            return set;
        }
        finally {
            lock.unlock();
        }
    }
    
    private Map<String, MarsQuest> startQuestsMap = new HashMap<String, MarsQuest>();
    private Map<String, MarsQuest> endQuestsMap = new HashMap<String, MarsQuest>();
    
    // players who have asked for quest info, we keep track of what quest we gave them
    private Map<Long, MarsQuest> offeredQuestMap = new HashMap<Long, MarsQuest>();
    
    Long eventSub = null;
    Long statusSub = null;
    static final Logger log = new Logger("QuestBehavior");
    private static final long serialVersionUID = 1L;
}
