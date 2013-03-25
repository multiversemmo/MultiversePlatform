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

package multiverse.server.events;

import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

/**
 * this event is saying notifyObj needs to know about a new object
 * the event handler should send it a newobj message
 */
public class NotifyNewObjectEvent extends Event {
    public NotifyNewObjectEvent() {
	super();
    }

    public NotifyNewObjectEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }
    
    // pass in the new mob in the contructor
    public NotifyNewObjectEvent(MVObject notifyObj, MVObject newObj) {
	super(notifyObj);
	setNewObjectOid(newObj.getOid());

        Log.debug("NotifyNewObjectEvent: checking obj to notify");
    }

    // from parent
    public String getName() {
	return "NotifyNewObjectEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getObjectOid()); 
	buf.putInt(msgId);
	buf.putLong(getNewObjectOid());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setObjectOid(buf.getLong());
	/* int msgId = */ buf.getInt();
	setNewObjectOid(buf.getLong());
    }
    
    // the mob we are going to notify that there is a new mob
    // the new mob is set in the constructor
    public void setObjToNotify(MVObject obj) {
	setObjectOid(obj.getOid());
    }

    public void setObjToNotifyOid(Long oid) {
	setObjectOid(oid);
    }

    // the mob we are going to notify that there is a new mob
    public Long getObjToNotifyOid() {
	return getObjectOid();
    }

    public void setNewObjectOid(Long oid) {
	newObjOid = oid;
    }
    public Long getNewObjectOid() {
	return newObjOid;
    }

    // the mob we need to notify that there is a new mob
    private Long newObjOid = null;
}
