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

package multiverse.mars.events;

import multiverse.server.engine.*;
import multiverse.server.network.*;
import multiverse.mars.objects.*;
import java.util.*;

public class QuestStateInfo extends Event {
    public QuestStateInfo() {
	super();
    }

    public QuestStateInfo(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public QuestStateInfo(MarsMob marsMob, QuestState questState) {
	super();
        setPlayerOid(marsMob.getOid());
        // FIXME:
        //setQuestId(questState.getQuestId());
        setObjectiveStatus(questState.getObjectiveStatus());
    }

    public String getName() {
	return "QuestStateInfo";
    }

    // i use a long here so i dont have to lock around it
    void setPlayerOid(Long id) {
        this.playerId = id;
    }
    void setQuestId(Long id) {
        this.questId = id;
    }
    void setObjectiveStatus(List<String> objStatus) {
        this.objStatus = objStatus;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(500);
	buf.putLong(playerId); 
	buf.putInt(msgId);
	
	buf.putLong(questId);
        buf.putInt(objStatus.size());
        Iterator<String> iter = objStatus.iterator();
        while (iter.hasNext()) {
            buf.putString(iter.next());
        }
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setPlayerOid(buf.getLong());
	/* int msgId = */ buf.getInt();
        setQuestId(buf.getLong());

        // read in the obj status list
        List<String> l = new LinkedList<String>();
        int len = buf.getInt();
        while (len>0) {
            l.add(buf.getString());
            len--;
        }
        setObjectiveStatus(l);
    }

    Long playerId = null;
    Long questId = null;
    List<String> objStatus = null;
}
