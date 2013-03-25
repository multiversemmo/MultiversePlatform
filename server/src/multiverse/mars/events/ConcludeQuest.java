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

import multiverse.mars.objects.*;
import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

/**
 * the client is turning in a quest
 */
public class ConcludeQuest extends Event {
    public ConcludeQuest() {
	super();
    }

    public ConcludeQuest(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public ConcludeQuest(MarsMob player, 
                         MarsMob questNpc) {
	super(player);
	setQuestNpcOid(questNpc.getOid());
    }

    public String getName() {
	return "ConcludeQuest";
    }

    public MarsMob getQuestNpc() {
	try {
	    return MarsMob.convert(MVObject.getObject(questNpcOid));
	}
	catch(MVRuntimeException e) {
	    throw new RuntimeException("concludequest", e);
	}
    }
    public Long getQuestNpcOid() {
	return this.questNpcOid;
    }
    public void setQuestNpcOid(Long questNpcOid) {
	this.questNpcOid = questNpcOid;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getObjectOid()); 
	buf.putInt(msgId);
	buf.putLong(getQuestNpc().getOid());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	long playerId = buf.getLong();
	setObjectOid(playerId);
	/* int msgId = */ buf.getInt();
	long questNpcId = buf.getLong();
	setQuestNpcOid(questNpcId);
    }

    protected Long questNpcOid = null;
}
