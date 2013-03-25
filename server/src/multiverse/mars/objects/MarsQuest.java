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
import multiverse.server.engine.*;
import java.util.*;

abstract public class MarsQuest extends Entity {
    public MarsQuest() {
        super();
        // For now, put establish the quest oid in this constructor.
        // When quests are persisted, however, the quest oid will come
        // from the one-arg constructor
        setOid(Engine.getOIDManager().getNextOid());
        setNamespace(Namespace.QUEST);
    }

    public void setDesc(String desc) {
        this.desc = desc;
    }
    public String getDesc() {
        return desc;
    }
    String desc = null;

    public void setObjective(String s) {
	this.objective = s;
    }
    public String getObjective() {
	return objective;
    }
    String objective = null;

    public void setCashReward(int reward) {
	cashReward = reward;
    }
    public int getCashReward() {
	return cashReward;
    }
    int cashReward = 0;

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

    public List<String> getQuestPrereqs() {
	return questPrereqs;
    }
    public void setQuestPrereqs(List<String> prereqs) {
	questPrereqs = prereqs;
    }
    public void addQuestPrereq(String questRef) {
	questPrereqs.add(questRef);
    }
    List<String> questPrereqs = new LinkedList<String>();

    // quest that is immediately offered when this quest is concluded
    public MarsQuest getChainQuest() {
        return chainQuest;
    }
    public void setChainQuest(MarsQuest chainQuest) {
        this.chainQuest = chainQuest;
    }
    MarsQuest chainQuest = null;

    public abstract QuestState generate(Long playerOid);
}
