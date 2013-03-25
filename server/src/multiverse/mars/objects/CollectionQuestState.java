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

package multiverse.mars.objects;

import multiverse.server.objects.*;
import multiverse.server.plugins.*;
import multiverse.server.util.*;
import multiverse.server.engine.*;
import multiverse.mars.plugins.QuestClient;
import multiverse.msgsys.*;

import java.io.*;
import java.util.*;

public class CollectionQuestState extends QuestState
{
    public CollectionQuestState() {
        setupTransient();
    }

    public CollectionQuestState(MarsQuest quest,
                                Long playerOid) {
        super(quest, playerOid);
        setupTransient();
    }
    
    /**
     * private method to recreate the lock when deserializing
     */
    private void readObject(ObjectInputStream in) throws IOException, ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }
    
    /**
     * returns the current state of this quest, ie, how many mobs to kill,etc
     */
    public String toString() {
        String status = "Quest=" + getName() + "\n";
        Iterator<String> iter = getObjectiveStatus().iterator();
        while(iter.hasNext()) {
            String s = iter.next();
            status = status + "   " + s + "\n";
        }
        return status;
    }

    public void activate() {
        if (Log.loggingDebug)
            log.debug("in activate: this " + this);
        // subscribe for some messages
        SubjectFilter filter = new SubjectFilter(getPlayerOid());
        filter.addType(InventoryClient.MSG_TYPE_INV_UPDATE);
        filter.addType(QuestClient.MSG_TYPE_CONCLUDE_QUEST);
        sub = Engine.getAgent().createSubscription(filter, this);
	makeDeliveryItems();
	updateQuestLog();
	updateObjectiveStatus();
	// updateQuestObjectives();
        log.debug("QuestPlugin activated");
    }

    public void deactivate() {
        if (Log.loggingDebug)
            log.debug("CollectionQuestState.deactivate: playerOid=" + getPlayerOid() + " questRef=" + getQuestRef());
        if (sub != null) {
            Engine.getAgent().removeSubscription(sub);
	}
    }
    
    /**
     * process network messages
     */
    public void handleMessage(Message msg, int flags) {
        if (msg instanceof InventoryClient.InvUpdateMessage) {
            processInvUpdate((InventoryClient.InvUpdateMessage) msg);
        }
        else if (msg instanceof QuestClient.ConcludeMessage) {
            processConcludeQuest((QuestClient.ConcludeMessage) msg);
        }
        else {
            log.error("unknown msg: " + msg);
        }
        //return true;
    }
    
    protected boolean processInvUpdate(InventoryClient.InvUpdateMessage msg) {
        if (Log.loggingDebug)
            log.debug("processInvUpdate: player=" + getPlayerOid() + ", invUpdate=" + msg);
        
        updateObjectiveStatus();
        return true;
    }
    
    protected boolean processConcludeQuest(QuestClient.ConcludeMessage msg) {
        Long mobOid = msg.getMobOid();
        if (!questOid.equals(msg.getQuestOid()))
            return true;
        if (Log.loggingDebug)
            log.debug("processConcludeQuest: player=" + getPlayerOid() + ", mob=" + mobOid);
	ArrayList<String> templateList = new ArrayList<String>();
	for (CollectionGoalStatus goalStatus : goalsStatus) {
	    for (int i=0; i < goalStatus.getTargetCount(); i++) {
		templateList.add(goalStatus.getTemplateName());
	    }
	}

        boolean conclude = false;
        if (templateList.isEmpty()) {
            conclude = true;
        } else {
            List<Long> removeResult = InventoryClient.removeItems(getPlayerOid(), templateList);
            if (removeResult != null) {
                conclude = true;
                for (Long itemOid : removeResult) {
                    ObjectManagerClient.deleteObject(itemOid);
                }
            }
        }
        if (conclude) {
            setConcluded(true);
            deactivate();
            updateQuestLog();
            sendStateStatusChange();
        }
        return true;
    }

    public void updateObjectiveStatus() {
	for (CollectionGoalStatus goalStatus : goalsStatus) {
	    ArrayList<String> templateList = new ArrayList<String>();
	    for (int i=0; i < goalStatus.getTargetCount(); i++) {
		templateList.add(goalStatus.getTemplateName());
	    }
            if (templateList.size() > 0) {
                List<Long> findResult =
                    InventoryClient.findItems(getPlayerOid(), templateList);
                goalStatus.currentCount = 0;
                for (Long itemOid : findResult) {
                    if (itemOid != null) {
                        goalStatus.currentCount++;
                    }
                }
            }
	}

        updateQuestObjectives();

	// update quest completed flag
	for (CollectionGoalStatus goalStatus : goalsStatus) {
	    if (goalStatus.currentCount < goalStatus.targetCount) {
		log.debug("updateObjectiveStatus: quest not completed");
		boolean wasComplete = getCompleted();
                setCompleted(false);
                if (wasComplete) {
                    // we were complete, but no longer, so update
                    sendStateStatusChange();
                }
		return;
	    }
	}
	if (getCompleted()) {
	    // already completed, ignore
	    return;
	}
	log.debug("updateObjectiveStatus: quest is completed");
	setCompleted(true);
        sendStateStatusChange();
	WorldManagerClient.sendObjChatMsg(playerOid, 0, "You have completed quest " + getName());
    }

    /**
     * generate delivery items and give them to the player
     */
    protected void makeDeliveryItems() {
        Long playerOid = getPlayerOid();
        Long bagOid = playerOid;
        if (Log.loggingDebug)
            log.debug("makeDeliveryItems: playerOid " + playerOid + ", bagOid + " + bagOid);

        // Normally the persistence flag is inherited from the enclosing
        // object, but all we have are OIDs.  Assume this is only used
        // for players and players are always persistent.
        Template overrideTemplate = new Template();
        overrideTemplate.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, true);

        for (String templateName : deliveryItems) {
            Long itemOid = ObjectManagerClient.generateObject(templateName, overrideTemplate);
            InventoryClient.addItem(bagOid, playerOid, bagOid, itemOid);
        }
    }
    
    /**
     * for client display: current state
     */
    public List<String> getObjectiveStatus() {
        lock.lock();
        try {
            List<String> l = new LinkedList<String>();
            
            Iterator<CollectionGoalStatus> iter = goalsStatus.iterator();
            while (iter.hasNext()) {
                CollectionGoalStatus status = iter.next();
                String itemName = status.getTemplateName();
                int numNeeded = status.targetCount;
                int cur = Math.min(status.currentCount, numNeeded);
                
                String objective = itemName + ": " + cur + "/" + numNeeded;
                l.add(objective);
            }
            return l;
        }
        finally {
            lock.unlock();
        }
    }

//    public void giveReward() {
////        getPlayer().sendServerInfo("You have completed the quest " +
////                                   getName());
////                                   
//	if (getCashReward() > 0) {
//	    // give the player some cash
//	}
//        throw new RuntimeException("CollectionQuestState: need to give out reward");
//    }

    public void setGoalsStatus(List<CollectionGoalStatus> goalsStatus) {
        this.goalsStatus = new LinkedList<CollectionGoalStatus>(goalsStatus);
    }

    public List<CollectionGoalStatus> getGoalsStatus() {
	lock.lock();
	try {
	    return new LinkedList<CollectionGoalStatus>(goalsStatus);
	}
	finally {
	    lock.unlock();
	}
    }


    /**
     * a list of items that the quest gives to the player
     * when the player accepts the quest
     */
    public void setDeliveryItems(List<String> items) {
        lock.lock();
        try {
            deliveryItems = new LinkedList<String>(items);
        }
        finally {
            lock.unlock();
        }
    }
    public void addDeliveryItem(String item) {
        lock.lock();
        try {
            deliveryItems.add(item);
        }
        finally {
            lock.unlock();
        }
    }
    public List<String> getDeliveryItems() {
        lock.lock();
        try {
            return deliveryItems;
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * called after the queststate is initialized and set by the world
     * server to the player
     */
    public void handleInit() {
	MVObject.transferLock.lock();
	try {
	    handleInitHelper();

	    handleInvUpdate();
	    completeHandler(); 
	}
	finally {
	    MVObject.transferLock.unlock();
	}
    }

    protected void handleInitHelper() {
        // give the delivery items to the player
        lock.lock();
        try {
//            log.debug("handleInit: quest=" + getName());
//            MarsMob player = getPlayer();
//            for (ItemTemplate templ : getDeliveryItems()) {
//                log.debug("handleInit: quest=" + getName() +
//                          ", generating item from template " +
//                          templ.getName());
//                MarsItem item = (MarsItem)templ.generate();
////                 item.spawn();
//                player.invAddToFreeSlot(item);
//                player.sendServerInfo("You are given " + item.getName());
//
//                DisplayContext dc = item.displayContext();
//                if (dc != null) {
//                    log.debug("attachinfo for item " + 
//                              item.getName());
//                    dc.printAttachInfo();
//                }
//            }
//            log.debug("handleInit: done generating delivery items");
//            player.updateMobInventory();
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * called then the player acquires a new item
     */
//     public void handleAcquire(MarsItem item) {
//         Log.debug("CollectionQuestState.handleAcquire: quest=" + 
//                   getName() + 
//                   ", acquiredItem=" + item.getName() +
//                   ", templateIsNull=" + (item.getTemplate() == null));

//         ItemTemplate newItemTempl = item.getTemplate();
//         if (newItemTempl == null) {
//             Log.warn("CollectionQuestState.handleAcquire: template is null");
// 	    return;
//         }        
//         lock.lock();
//         try {
//             if (completed()) {
//                 Log.debug("CollectionQuestState.handleAcquire: quest is completed");
//                 return;
//             }
//             // check if the item is something we needed
//             Log.debug("CollectionQuestState.handleAcquire: numGoals=" +
//                       goalsStatus.size());
//             Iterator<CollectionGoalStatus> iter = goalsStatus.iterator();
//             while(iter.hasNext()) {
//                 CollectionGoalStatus status = iter.next();
//                 Log.debug("CollectionQuestState.handleAcquire: comparing newitem " +
//                           newItemTempl +
//                           " with quest goal of " +
//                           status.template);
//                 if (MVObject.equals(newItemTempl, status.template)) {
//                     Log.debug("CollectionGoalStatus.handleAcquire: matches");
//                     if (status.currentCount < status.targetCount) {
//                         status.currentCount++;
//                         // check if completed and handle if it is
//                         // (sending message to user and also notifying
//                         //  mobs nearby in case one of them is a quest 
//                         //  completer)
//                         completeHandler(); 

//                         getPlayer().sendServerInfo("You make progress on the quest " + getName() + ".  You have " + status.currentCount + "/" + status.targetCount + " " + status.template.getName());
//                     }
//                     return;
//                 }
//                 else {
//                     Log.debug("CollectionGoalStatus.handleAcquire: no match");
//                 }
//             }
//         }
//         finally {
//             lock.unlock();
//         }
//     }
    public void handleInvUpdate() {
        if (Log.loggingDebug)
            if (Log.loggingDebug)
                Log.debug("CollectionQuestState.handleAcquire: quest=" + 
                          getName());

        lock.lock();
        try {
	    if (getConcluded()) {
		return;
	    }
//            MarsMob player = getPlayer();
//            Log.debug("CollectionQuestState.handleInvUpdate: player=" +
//		      player.getName() +
//		      ", questState=" + this.getName() +
//		      ", numGoals=" +
//                      goalsStatus.size());
//            Iterator<CollectionGoalStatus> iter = goalsStatus.iterator();
//            while(iter.hasNext()) {
//                CollectionGoalStatus status = iter.next();
//                int invCount = player.getInvItemCount(status.template);
//		Log.debug("CollectionQuestState.handleInvUpdate: checking itemTemplate=" + status.template + ", invCount=" + invCount);
//                if (getCompleted()) {
//		    Log.debug("CollectionQuestState.handleInvUpdate: completed");
//                    // the quest was completed prior to getting this item
//                    if (invCount > status.currentCount) {
//                        // we must have just picked up a quest item
//                        // but we dont need it
//                        continue;
//                    }
//                    else if (invCount == status.currentCount) {
//                        // quest is still completed - you can ignore
//                        continue;
//                    }
//                    
//                    // invCount < status.currentCount
//                    // we must have just dropped an item, and
//                    // the quest is no longer complete
//                    setCompleted(false);
//                    status.currentCount = invCount;
//
//                    // send a QuestCompleted event to mobs that perceive us
//                    // so that if one of them can accept this quest for 
//                    // completion, it can update its state
//                    QuestCompleted completeEvent = 
//                        new QuestCompleted(player, getQuest());
//                    Engine.getMessageServer().sendToPerceivers(player,
//                                                               completeEvent,
//                                                               RDPMessageServer.NON_USERS);
//                    
//                    // send a log update
//                    Event questStateInfo = new QuestStateInfo(player, this);
//                    player.sendEvent(questStateInfo);
//
//                    // send text message
//                    player.sendServerInfo("Your quest " + getName() + " is no longer completed.  You have " + status.currentCount + "/" + status.targetCount + " " + status.template.getName());
//                    continue;
//                }
//
//                // quest is not completed
//		Log.debug("CollectionQuestState.handleInvUpdate: not completed");
//
//                if (invCount < status.currentCount) {
//		    Log.debug("CollectionQuestState.handleInvUpdate: lost item");
//                    // we lost an item
//                    status.currentCount = invCount;
//
//                    // send a log update
//                    Event questStateInfo = new QuestStateInfo(player, this);
//                    player.sendEvent(questStateInfo);
//
//                    // send text message
//                    player.sendServerInfo("You lost an item for quest " + getName() + ".  You have " + status.currentCount + "/" + status.targetCount + " " + status.template.getName());
//                    continue;
//                }
//                else if (invCount == status.currentCount) {
//                    // we didnt get a new quest item
//		    Log.debug("CollectionQuestState.handleInvUpdate: not quest item");
//                    continue;
//                }
//		if (invCount > status.currentCount) {
//                    // we just got a new needed quest item
//		    Log.debug("CollectionQuestState.handleInvUpdate: got new quest item");
//                    if (invCount > status.targetCount) {
//                        // we have more than we need
//                        status.currentCount = status.targetCount;
//                    }
//                    else {
//                        status.currentCount = invCount;
//                    }
//
//                    // send a log update
//                    Event questStateInfo = new QuestStateInfo(player, this);
//                    player.sendEvent(questStateInfo);
//
//                    // send text message
//                    player.sendServerInfo("You make progress on the quest " + getName() + ".  You have " + status.currentCount + "/" + status.targetCount + " " + status.template.getName());
//
//                    // check if completed and handle if it is
//                    // (sending message to user and also notifying
//                    //  mobs nearby in case one of them is a quest 
//                    //  completer)
//                    completeHandler(); 
//                }
//            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * called when the player is concluding (turning in) the quest
     * returns false if the quest is not able to be concluded
     */
    public boolean handleConclude() {
        if (Log.loggingDebug)
            log.debug("handleConclude: thisQuest=" + this.getName() +
		      ", playerOid=" + getPlayerOid());
//         RemoteMob player = MVObject.getRemoteMob(getPlayerOid());
//         if (player == null) {
//             throw new MVRuntimeException("CollectionQuestState.handleConclude: cannot get remote mob");
//         }
//         try {
//             Log.debug("CollectionQuestState.handleConclude: player=" + 
//                       player.getName() +
//                       ", quest=" + getName());
//         }
//         catch(java.rmi.RemoteException e) {
//             throw new MVRuntimeException("CollectionQuestState", e);
//         }

//         // take the items away
//         List<CollectionQuestState.CollectionGoalStatus> goals = 
//             getGoalsStatus();
        
//         Log.debug("CollectionQuestState.handleConclude: quest=" + 
//                   getName() +
//                   ", goals length=" + 
//                   goals.size());
//         Iterator<CollectionQuestState.CollectionGoalStatus> iter = 
//             goals.iterator();
//         while(iter.hasNext()) {
//             CollectionGoalStatus goal = iter.next();
//             ItemTemplate templ = goal.template;
//             int count = goal.targetCount;
            
//             Log.debug("CollectionQuestState.handleConclude: need to destroy " +
//                       count + 
//                       " items named " + templ.getName());

//             try {
//                 while(count > 0) {
//                     MarsItem item = player.findItem(templ);
//                     if (item == null) {
//                         throw new MVRuntimeException("CollectionQuestState.handleConclude: could not find the item with matching template");
//                     }
//                     if (! player.destroyItem(item)) {
//                         throw new MVRuntimeException("CollectionQuestState.handleConclude: destroyItem failed");
//                     }
//                     count--;
//                     Log.debug("CollectionQuestState.handleConclude: destroyed " +
//                               item.getName() +
//                               " need to destroy " + count + " more");
//                 }
//             }
//             catch(java.rmi.RemoteException e) {
//                 throw new MVRuntimeException("CollectionQuestState.handleConclude: got remote exception", e);
//             }
//         }
//         Log.debug("CollectionQuestState.handleConclude: destroyed all items");
// 	setConcluded(true);

// 	// send a remove quest state event to player
// 	Event removeQuest = new RemoveQuestResponse(this);
// 	this.getPlayer().sendEvent(removeQuest);

        return true;
    }

    /**
     * marks quest as completed if we just completed it
     */
    protected void completeHandler() {
        lock.lock();
        try {
            if (getCompleted()) {
                return;
            }

//            MarsMob player = getPlayer();
//
//            // go through all the goals and see if all are completed
//            Iterator<CollectionGoalStatus> iter = goalsStatus.iterator();
//            while(iter.hasNext()) {
//                CollectionGoalStatus status = iter.next();
//                if (status.currentCount < status.targetCount) {
//                    return;
//                }
//            }
//            setCompleted(true);
//            player.sendServerInfo("You have completed the quest " + getName());
//            
//            // send a QuestCompleted event to mobs that perceive us
//            // so that if one of them can accept this quest for completion,
//            // it can update its state
//            QuestCompleted completeEvent = new QuestCompleted(player, 
//                                                              getQuest());
//
//            Engine.getMessageServer().sendToPerceivers(player,
//                                                       completeEvent,
//                                                       RDPMessageServer.NON_USERS);
        }
        finally {
            lock.unlock();
        }
    }

//    public void readExternal(ObjectInput in)
//	throws IOException, ClassNotFoundException {
//        lock.lock();
//	try {
//	    Log.lock.lock();
//	    try {
//		Log.debug("CollectionQuestState.readExternal: dump stack for debug: ");
//		Log.dumpStack();
//	    }
//	    finally {
//		Log.lock.unlock();
//	    }
//
//            super.readExternal(in);
//            goalsStatus = (List<CollectionGoalStatus>) in.readObject();
//            deliveryItems = (List<ItemTemplate>) in.readObject();
//	}
//	finally {
//	    lock.unlock();
//	}
//    }

//    protected void writeExternalImpl(ObjectOutput out) throws IOException {
//        lock.lock();
//	try {
//            Log.debug("CollectionQuestState.writeExternal");
//            super.writeExternalImpl(out);
//            out.writeObject(goalsStatus);
//            out.writeObject(deliveryItems);
//        }
//        finally {
//            lock.unlock();
//        }
//    }

    public static class CollectionGoalStatus implements Serializable {
        public CollectionGoalStatus() {
        }

        public CollectionGoalStatus(MarsCollectionQuest.CollectionGoal goal) {
            this.templateName = goal.getTemplateName();
            this.targetCount = goal.getNum();
            this.currentCount = 0;
        }

        public void setTemplateName(String templateName) {
            this.templateName = templateName;
        }
        public String getTemplateName() {
            return templateName;
        }
        public String templateName = null;

        public void setTargetCount(int c) {
            this.targetCount = c;
        }
        public int getTargetCount() {
            return this.targetCount;
        }
        public int targetCount = 0;

        public void setCurrentCount(int c) {
            this.currentCount = c;
        }
        public int getCurrentCount() {
            return this.currentCount;
        }
        public int currentCount = 0;

        private static final long serialVersionUID = 1L;
    }

    static final Logger log = new Logger("CollectionQuestState");

    transient Long sub = null;

    List<CollectionGoalStatus> goalsStatus = new LinkedList<CollectionGoalStatus>();
    List<String> deliveryItems = new LinkedList<String>();
    private static final long serialVersionUID = 1L;
}
