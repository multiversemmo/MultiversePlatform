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

import java.io.*;
import java.util.*;

public class MarsCollectionQuest extends MarsQuest {
    public MarsCollectionQuest() {
        super();
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
    public List<String> getDeliveryItems() {
        lock.lock();
        try {
            return new LinkedList<String>(deliveryItems);
        }
        finally {
            lock.unlock();
        }
    }
    public void addDeliveryItem(String templateName) {
        lock.lock();
        try {
            deliveryItems.add(templateName);
        }
        finally {
            lock.unlock();
        }
    }

    public void setCollectionGoals(List<CollectionGoal> goals) {
        lock.lock();
        try {
            this.goals = new LinkedList<CollectionGoal>(goals);
        }
        finally {
            lock.unlock();
        }
    }
    public List<CollectionGoal> getCollectionGoals() {
        lock.lock();
        try {
            return new LinkedList<CollectionGoal>(goals);
        }
        finally {
            lock.unlock();
        }
    }
    public void addCollectionGoal(CollectionGoal goal) {
        lock.lock();
        try {
            goals.add(goal);
        }
        finally {
            lock.unlock();
        }
    }

    public QuestState generate(Long playerOid) {
        lock.lock();
        try {
            List<CollectionQuestState.CollectionGoalStatus> goalsStatus = 
                new LinkedList<CollectionQuestState.CollectionGoalStatus>();

            // go through all the collection goals and make a list of
            // CollectionGoalStatus objects
            CollectionQuestState qs = new CollectionQuestState(this, playerOid);
            Iterator<CollectionGoal> iter = goals.iterator();
            while(iter.hasNext()) {
                CollectionGoal goal = iter.next();
                CollectionQuestState.CollectionGoalStatus status = 
                    new CollectionQuestState.CollectionGoalStatus(goal);
                goalsStatus.add(status);
            }
            qs.setGoalsStatus(goalsStatus);

            // set the delivery item list
            qs.setDeliveryItems(deliveryItems);
            return qs;
        }
        finally {
            lock.unlock();
        }
    }

    public static class CollectionGoal implements Serializable {
        public CollectionGoal() {
        }

        public CollectionGoal(String templateName, int num) {
            setTemplateName(templateName);
            setNum(num);
        }

        public void setTemplateName(String templateName) {
            this.templateName = templateName;
        }
        public String getTemplateName() {
            return templateName;
        }

        public void setNum(int num) {
            this.num = num;
        }
        public int getNum() {
            return num;
        }

        public String templateName = null;
        public int num = 0;

        private static final long serialVersionUID = 1L;
    }

    List<CollectionGoal> goals = new LinkedList<CollectionGoal>();
    List<String> deliveryItems = new LinkedList<String>();
    private static final long serialVersionUID = 1L;
}
