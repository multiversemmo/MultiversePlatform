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

import java.util.HashMap;
import java.util.Map;

import multiverse.server.engine.Namespace;
import multiverse.server.objects.Entity;

/**
 * a subobject for a player that keeps track of the players quest states
 * @author cedeno
 *
 */
public class PlayerQuestStates extends Entity {

    public PlayerQuestStates() {
        super();
        setNamespace(Namespace.PLAYERQUESTSTATES);
        // TODO Auto-generated constructor stub
    }

    public PlayerQuestStates(String name) {
        super(name);
        setNamespace(Namespace.PLAYERQUESTSTATES);
        // TODO Auto-generated constructor stub
    }

    public PlayerQuestStates(Long oid) {
        super(oid);
        setNamespace(Namespace.PLAYERQUESTSTATES);
        // TODO Auto-generated constructor stub
    }

    public void addQuestState(QuestState qs) {
        lock.lock();
        try {
            questStateMap.put(qs.getQuestRef(), qs);
        }
        finally {
            lock.unlock();
        }
    }
    
    public QuestState getQuestState(String name) {
        lock.lock();
        try {
            return questStateMap.get(name);
        }
        finally {
            lock.unlock();
        }
    }
    
    public void setQuestStateMap(Map<String, QuestState> map) {
        lock.lock();
        try {
            this.questStateMap = new HashMap<String,QuestState>(map);
        }
        finally {
            lock.unlock();
        }
    }
    
    public Map<String, QuestState> getQuestStateMap() {
        lock.lock();
        try {
            return new HashMap<String,QuestState>(questStateMap);
        }
        finally {
            lock.unlock();
        }
    }
    
    private Map<String,QuestState> questStateMap = new HashMap<String, QuestState>();

    private static final long serialVersionUID = 1L;
}
