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

import java.util.concurrent.locks.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.mars.plugins.*;
import multiverse.mars.plugins.QuestClient.StateStatusChangeMessage;
import multiverse.msgsys.*;

import java.util.*;

public abstract class QuestState
        implements MessageCallback, MessageDispatch
{
    public QuestState() {
        setupTransient();
    }

    public QuestState(MarsQuest quest, 
                      Long playerOid) {
        // we store the playeroid instead because we are probably
        // loading ourselves while loading up the player object,
        // in which case the player ref isnt available yet
        setupTransient();
	this.playerOid = playerOid;
        setQuestRef(quest.getName());
	setQuestOid(quest.getOid());
	setQuestTitle(quest.getName());
	setQuestDesc(quest.getDesc());
	setQuestObjective(quest.getObjective());
        for (String reward : quest.getRewards()) {
            addReward(reward);
        }
    }
    
    // called from constructor and readObject
    protected void setupTransient() {
        lock = LockFactory.makeLock("QuestStateLock");
    }

    public String toString() {
        return "[AbstractQuestStateObject]";
    }

    /**
     * gets activated by QuestPlugin.  the quest is typically created by the QuestBehavior and
     * sent over to the QuestPlugin, which then adds it to the players quest states and also
     * calls activate on the quest state object
     */
    abstract public void activate();
    abstract public void deactivate();

    public String getName() {
        return "QuestStateFor:" + getQuestRef();
    }

    public Long getPlayerOid() {
        return playerOid;
    }
    public void setPlayerOid(Long oid) {
	this.playerOid = oid;
    }

    /**
     * called after the queststate is initialized and set by the world
     * server to the player
     */
    public void handleInit() {
    }
    
    /**
     * called when a mob is killed that the player is getting credit for
     */
    public void handleDeath(MarsMob mobKilled) {
    }

    /**
     * called when the player's inv changes
     */
    public void handleInvUpdate() {
    }

    /**
     * called when the player is concluding (turning in) the quest
     * returns false if the quest state cannot conclude the quest
     */
    public boolean handleConclude() {
	setConcluded(true);
        return true;
    }

    /**
     * sends QuestLogInfo message for this quest
     */
    public void updateQuestLog() {
        if (concludedFlag) {
            QuestPlugin.sendRemoveQuestResp(playerOid, questOid);
        }
        else {
            QuestPlugin.sendQuestLogInfo(playerOid, questOid, questTitle, questDesc, questObjective, itemRewards);
        }
    }

    public void updateQuestObjectives() {
        if (Log.loggingDebug)
            Log.debug("QuestState.updateQuestObjectives: this " + this + ", playerOid " + getPlayerOid() + ", questOid " + getQuestOid());
        QuestPlugin.sendQuestStateInfo(getPlayerOid(), getQuestOid(), getObjectiveStatus());
    }

    /**
     * send a StateStatusChangeMessage to notify that this quest has been updated
     */
    public void sendStateStatusChange() {
        StateStatusChangeMessage statusMsg = new StateStatusChangeMessage(playerOid, getQuestRef());
        Engine.getAgent().sendBroadcast(statusMsg);
        if (Log.loggingDebug)
            Log.debug("sendStateStatusChange: playerOid=" + playerOid + ", questRef=" + getQuestRef());
    }

    public String getQuestRef() {
        return questRef;
    }

    public void setQuestRef(String quest) {
        this.questRef = quest;
    }

    public void setCompleted(boolean flag) {
        completedFlag = flag;
    }
    public boolean getCompleted() {
        return completedFlag;
    }

    public void setConcluded(boolean flag) {
	concludedFlag = flag;
    }
    public boolean getConcluded() {
	return concludedFlag;
    }

    /**
     * returns a string representation of the current objectives for display
     * on the client.
     * should return a copy that wont be changed
     * eg: entry1: 0/1 orc scalps
     *     entry2: 4/10 orc hides
     */
    abstract public List<String> getObjectiveStatus();


    public Long getQuestOid() {
	return questOid;
    }
    public void setQuestOid(Long oid) {
	this.questOid = oid;
    }

    public String getQuestTitle() {
	return questTitle;
    }
    public void setQuestTitle(String title) {
	questTitle = title;
    }

    public String getQuestDesc() {
	return questDesc;
    }
    public void setQuestDesc(String desc) {
	questDesc = desc;
    }

    public String getQuestObjective() {
	return questObjective;
    }
    public void setQuestObjective(String objective) {
	questObjective = objective;
    }

    /**
     * returns a list item template names
     */
    public List<String> getRewards() {
        return itemRewards;
    }
    public void setRewards(List<String> rewards) {
        itemRewards = rewards;
    }
    public void addReward(String reward) {
        lock.lock();
        try {
            itemRewards.add(reward);
        }
        finally {
            lock.unlock();
        }
    }
    List<String> itemRewards = new LinkedList<String>();

    public abstract void handleMessage(Message msg, int flags);

    public void dispatchMessage(Message message, int flags,
        MessageCallback callback)
    {
        Engine.defaultDispatchMessage(message, flags, callback);
    }

    transient protected Lock lock = null;
    
    String questRef = null;
    Long playerOid = null;
    Long questOid = null;
    boolean completedFlag = false;
    boolean concludedFlag = false;
    String questTitle = null;
    String questDesc = null;
    String questObjective = null;
}
