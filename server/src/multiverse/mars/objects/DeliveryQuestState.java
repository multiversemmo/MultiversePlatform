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

import multiverse.msgsys.*;

import java.util.*;

public class DeliveryQuestState extends QuestState {
    public DeliveryQuestState() {
    }

    public DeliveryQuestState(MarsQuest quest,
                              Long playerOid) {
        super(quest, playerOid);
    }

    public List<String> getObjectiveStatus() {
        return new LinkedList<String>();
    }

    /**
     * called when a mob is killed that the player is getting credit for
     */
    public void handleDeath(MarsMob mobKilled) {
//         if (completed()) {
//             return;
//         }
    }

    String mobName = null;
    int count = 0;
    int currentCount = 0;

    public void activate() {
        // TODO Auto-generated method stub
        
    }

    public void deactivate() {
        // TODO Auto-generated method stub
        
    }

    public void handleMessage(Message msg, int flags) {
        // TODO Auto-generated method stub
        //return false;
    }

    private static final long serialVersionUID = 1L;
}
