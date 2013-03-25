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

public class MarsKillQuest extends MarsQuest {
    public MarsKillQuest() {
        super();
    }

    public void setKillGoal(String mobName, int count) {
        setKillGoal(new KillGoal(mobName, count));
    }

    public void setKillGoal(KillGoal goal) {
        this.goal = goal;
    }
    
    public KillGoal getKillGoal() {
        return goal;
    }

    public QuestState generate(Long playerOid) {
	KillQuestState qs = new KillQuestState(this, playerOid);
	qs.setKillGoal(getKillGoal());
	return qs;
    }

    public static class KillGoal implements Serializable {
        public KillGoal() {
        }

        public KillGoal(String name, int count) {
            setName(name);
            setCount(count);
        }

        public String toString() {
            return "[KillGoal: mobName=" + getName() +
                ", targetCount=" + getCount() + "]";
        }
        public void setName(String name) {
            this.name = name;
        }
        public String getName() {
            return name;
        }

        public void setCount(int count) {
            this.count = count;
        }
        public int getCount() {
            return count;
        }

        private String name = null;
        private int count = -1;
        private static final long serialVersionUID = 1L;
    }

    private KillGoal goal = null;

    private static final long serialVersionUID = 1L;
}
