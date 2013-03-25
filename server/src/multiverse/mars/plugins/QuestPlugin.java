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

import java.util.HashMap;
import java.util.List;
import java.util.LinkedList;
import java.util.Map;
import java.util.concurrent.locks.Lock;
import java.io.Serializable;

import multiverse.mars.objects.PlayerQuestStates;
import multiverse.mars.objects.QuestState;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.server.plugins.InventoryClient;
import multiverse.mars.plugins.QuestClient.*;
import multiverse.msgsys.*;
import multiverse.server.engine.*;
import multiverse.server.objects.Template;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.util.Log;
import multiverse.server.util.Logger;

/**
 * handles requests for quest state information related to a player. manages all
 * quest states for players.
 * 
 * @author cedeno
 * 
 */
public class QuestPlugin extends EnginePlugin {
    public QuestPlugin() {
        super("Quest");
        setPluginType("Quest");
    }

    public void onActivate() {
        registerHooks();
        
        MessageTypeFilter filter = new MessageTypeFilter();
        filter.addType(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT);
        filter.addType(QuestClient.MSG_TYPE_REQ_RESET_QUESTS);
        /* Long sub = */ Engine.getAgent().createSubscription(filter, this);

        filter = new MessageTypeFilter();
        filter.addType(QuestClient.MSG_TYPE_NEW_QUESTSTATE);
        filter.addType(QuestClient.MSG_TYPE_GET_QUEST_STATUS);
        /* Long sub = */ Engine.getAgent().createSubscription(filter, this,
            MessageAgent.RESPONDER);

        if (Log.loggingDebug)
            log.debug("QuestPlugin activated");
    }

    // how to process incoming messages
    protected void registerHooks() {
        getHookManager().addHook(QuestClient.MSG_TYPE_GET_QUEST_STATUS, new GetQuestStatusHook());
        getHookManager().addHook(QuestClient.MSG_TYPE_NEW_QUESTSTATE, new NewQuestStateHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT, new UpdateObjHook());
	getHookManager().addHook(QuestClient.MSG_TYPE_REQ_RESET_QUESTS, new ResetQuestsHook());
    }

    protected PlayerQuestStates getPlayerQuestStates(Long playerOid) {
        lock.lock();
        try {
            PlayerQuestStates pQS = playerQSMap.get(playerOid);
            if (pQS == null) {
                pQS = new PlayerQuestStates();
                playerQSMap.put(playerOid, pQS);
            }
            return pQS;
        }
        finally {
            lock.unlock();
        }
    }
    private Map<Long, PlayerQuestStates> playerQSMap = new HashMap<Long, PlayerQuestStates>();
    
    public class NewQuestStateHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            QuestClient.NewQuestStateMessage msg = (QuestClient.NewQuestStateMessage) m;
            Long playerOid = msg.getSubject();
            QuestState qs = msg.getQuestState();
            if (Log.loggingDebug)
                log.debug("NewQuestStateHook: playerOid=" + playerOid + ", qs=" + qs);
            
            Lock lock = getObjectLockManager().getLock(playerOid);
            lock.lock();
            try {
                // add this quest state to the player's quest states object
                PlayerQuestStates pQS = getPlayerQuestStates(playerOid);
                pQS.addQuestState(qs);
                log.debug("NewQuestStateHook: added qs to player's quest state object");
                qs.activate();
                
                // send response msg
                Engine.getAgent().sendBooleanResponse(msg, Boolean.TRUE);
            }
            finally {
                lock.unlock();
            }
            return false;
        }
    }
    
    public class GetQuestStatusHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            QuestClient.GetQuestStatusMessage pMsg = (QuestClient.GetQuestStatusMessage) msg;

            Long oid = pMsg.getSubject();
            List<String> questRefs = pMsg.getQuestRefs();

            if (Log.loggingDebug)
                log.debug("GetQuestStatusHook: player=" + oid + ", questRefs="
                          + questRefs);

            Lock lock = getObjectLockManager().getLock(oid);
            lock.lock();
            try {
                // get the response
                HashMap<String, Byte> respMap = new HashMap<String, Byte>();
		PlayerQuestStates pQS = getPlayerQuestStates(oid);
		for (String questRef : questRefs) {
                    QuestState qs = pQS.getQuestState(questRef);
                    Byte status;
                    if (qs == null) {
                        status = QuestClient.QuestStatusDNE;
                    } else if (qs.getConcluded()) {
                        status = QuestClient.QuestStatusConcluded;
                    } else if (qs.getCompleted()) {
                        status = QuestClient.QuestStatusCompleted;
                    } else {
                        status = QuestClient.QuestStatusInProgress;
                    }
                    Log.debug("QuestStatus: oid="+oid+" quest="+questRef+
                        " status="+status);
                    respMap.put(questRef, status);
                }

                Engine.getAgent().sendObjectResponse(pMsg, respMap);
                log.debug("GetQuestStatusHook: sent response");
                return true;
            } finally {
                lock.unlock();
            }
        }
    }
    
    class UpdateObjHook implements Hook {
        public boolean processMessage(Message msg, int flags) {

            WorldManagerClient.UpdateMessage cMsg = (WorldManagerClient.UpdateMessage) msg;
            Long oid = cMsg.getSubject();

	    // only send quest log data if object is asking about itself
	    if (!oid.equals(cMsg.getTarget())) {
		return true;
	    }

            if (Log.loggingDebug)
                log.debug("QuestPlugin.UpdateObjHook: updating obj " + oid + " with quest info");
	    PlayerQuestStates pQS = getPlayerQuestStates(oid);
	    for (QuestState qs : pQS.getQuestStateMap().values()) {
		qs.updateQuestLog();
	    }
	    return true;
        }
    }

    class ResetQuestsHook implements Hook {
	public boolean processMessage(Message msg, int flags) {
	    SubjectMessage qMsg = (SubjectMessage) msg;
	    Long oid = qMsg.getSubject();
            if (Log.loggingDebug)
                log.debug("ResetQuestsHook: resetting quests for oid=" + oid);
	    PlayerQuestStates pQS = playerQSMap.get(oid);
	    if (pQS == null) {
		return true;
	    }
	    playerQSMap.put(oid, null);
	    for (QuestState qs : pQS.getQuestStateMap().values()) {
                if (Log.loggingDebug)
                    log.debug("ResetQuestsHook: resetting quest=" + qs.getQuestRef() + " for oid=" + oid);
		qs.deactivate();
                TargetedExtensionMessage rMsg = new TargetedExtensionMessage(QuestClient.MSG_TYPE_REMOVE_QUEST_RESP,
                                                                             "mv.REMOVE_QUEST_RESP",
                                                                             qs.getPlayerOid(), qs.getQuestOid());
		Engine.getAgent().sendBroadcast(rMsg);
		StateStatusChangeMessage cMsg = new StateStatusChangeMessage(oid, qs.getQuestRef());
		Engine.getAgent().sendBroadcast(cMsg);
	    }
	    return true;
	}
    }

    protected static String getItemTemplateIcon(String templateName) {
        Template template = ObjectManagerClient.getTemplate(templateName);
        return (String)template.get(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON);
    }

    public static void sendRemoveQuestResp(Long playerOid, Long questOid) {
        TargetedExtensionMessage msg = new TargetedExtensionMessage(QuestClient.MSG_TYPE_REMOVE_QUEST_RESP,
                                                                    "mv.REMOVE_QUEST_RESP", playerOid, questOid);
        if (Log.loggingDebug)
            Log.debug("QuestState.sendRemoveQuestResp: removing questOid=" + questOid
                      + " from player=" + playerOid);
        Engine.getAgent().sendBroadcast(msg);
    }

    public static void sendQuestLogInfo(Long playerOid, Long questOid, String questTitle, String questDesc,
                                        String questObjective, List<String> itemRewards) {
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.QUEST_LOG_INFO");
        props.put("title", questTitle);
        props.put("description", questDesc);
        props.put("objective", questObjective);
        LinkedList<LinkedList> rewardList = new LinkedList<LinkedList>();
        for (String rewardName : itemRewards) {
            LinkedList<Object> reward = new LinkedList<Object>();
            reward.add(rewardName);
            reward.add(getItemTemplateIcon(rewardName));
            reward.add(1);
            rewardList.add(reward);
        }
        props.put("rewards", rewardList);
        TargetedExtensionMessage msg = new TargetedExtensionMessage(QuestClient.MSG_TYPE_QUEST_LOG_INFO,
                                                                    playerOid, questOid, false, props);
        if (Log.loggingDebug)
            Log.debug("QuestState.sendQuestLogInfo: updating player=" + playerOid + " with quest="
                      + questTitle);
        Engine.getAgent().sendBroadcast(msg);
    }

    public static void sendQuestInfo(Long playerOid, Long npcOid, Long questOid, String questTitle,
                                     String questDesc, String questObjective, List<String> itemRewards)
    {
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.QUEST_INFO");
        props.put("title", questTitle);
        props.put("description", questDesc);
        props.put("objective", questObjective);
        props.put("questOid", questOid);
        LinkedList<LinkedList> rewardList = new LinkedList<LinkedList>();
        for (String rewardName : itemRewards) {
            LinkedList<Object> reward = new LinkedList<Object>();
            reward.add(rewardName);
            reward.add(getItemTemplateIcon(rewardName));
            reward.add(1);
            rewardList.add(reward);
        }
        props.put("rewards", rewardList);
        TargetedExtensionMessage msg = new TargetedExtensionMessage(QuestClient.MSG_TYPE_QUEST_INFO,
                                                                    playerOid, npcOid, false, props);
        Engine.getAgent().sendBroadcast(msg);
    }

    public static void sendQuestStateInfo(Long playerOid, Long questOid, List<String> objectives) {
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.QUEST_STATE_INFO");
        LinkedList<String> objectivesList = new LinkedList<String>(objectives);
        props.put("objectives", objectivesList);
        TargetedExtensionMessage msg = new TargetedExtensionMessage(QuestClient.MSG_TYPE_QUEST_INFO,
                                                                    playerOid, questOid, false, props);
        Engine.getAgent().sendBroadcast(msg);
    }

    private static final Logger log = new Logger("QuestPlugin");
}
