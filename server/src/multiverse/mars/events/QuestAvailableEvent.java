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
import multiverse.server.network.*;

public class QuestAvailableEvent extends Event {
    public QuestAvailableEvent() {
	super();
    }

    public QuestAvailableEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public QuestAvailableEvent(MarsMob user,
                               MarsMob questGiver,
                               boolean isAvail,
                               boolean isConclude) {
	super(user);
	setQuestGiverOid(questGiver.getOid());
        isAvailable(isAvail);
        isConcludable(isConclude);
    }

    public String getName() {
	return "QuestAvailableEvent";
    }

    public void setQuestGiverOid(Long oid) {
	this.questGiverOid = oid;
    }
    public Long getQuestGiverOid() {
	return questGiverOid;
    }

    public void isAvailable(boolean flag) {
        this.isAvailableFlag = flag;
    }
    public boolean isAvailable() {
        return isAvailableFlag;
    }

    public void isConcludable(boolean flag) {
        this.isConcludableFlag = flag;
    }
    public boolean isConcludable() {
        return isConcludableFlag;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(32);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);
	
	buf.putLong(getQuestGiverOid());
	buf.putBoolean(isAvailable());
        buf.putBoolean(isConcludable());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();

	long userId = buf.getLong();
	setObjectOid(userId);
	/* int msgId = */ buf.getInt();

	Long questGiverOid = buf.getLong();
	setQuestGiverOid(questGiverOid);

        isAvailable(buf.getBoolean());
        isConcludable(buf.getBoolean());
    }

    private Long questGiverOid = null;
    private boolean isAvailableFlag = false;
    private boolean isConcludableFlag = false;
}
