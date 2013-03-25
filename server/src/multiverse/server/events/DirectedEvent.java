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
import multiverse.server.network.*;
import multiverse.server.objects.*;
import multiverse.server.util.*;
import java.util.*;

/**
 * use this event when you want to send a particular event to multiple
 * recipients on a single connection.  this is used when the world
 * server is sending a message to the entity manager.  there is only
 * one shared connection but the message is supposed to go to a set of
 * entities
 */
public class DirectedEvent extends Event {

    public DirectedEvent() {
	super();
    }

    public DirectedEvent(Collection<MVObject> recipients,
			 Event event) {
	setRecipients(recipients);
	setContainedEvent(event);
    }

    public String getName() {
	return "DirectedEvent";
    }

    public void setContainedEvent(Event e) {
	containedEvent = e;
    }
    public Event getContainedEvent() {
	return containedEvent;
    }
    private Event containedEvent = null;

    public void setRecipients(Collection<MVObject> c) {
	recipientCol = c;
    }
    public Collection<MVObject> getRecipients() {
	return recipientCol;
    }
    private Collection<MVObject> recipientCol = null;
    
    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(1000);
	buf.putLong(-1); // dummy id
	buf.putInt(msgId);

	// write out the recipient list
	if (recipientCol == null) {
	    throw new MVRuntimeException("DirectedEvent: recipient list size is 0");
	}
	buf.putInt(recipientCol.size());
// 	Log.debug("DirectedEvent: recipient list size=" + recipientCol.size());
	Iterator<MVObject> iter = recipientCol.iterator();
	while(iter.hasNext()) {
	    MVObject e = iter.next();
	    buf.putLong(e.getOid());
// 	    Log.debug("DirectedEvent: recipient oid=" + e.getOid());
	}

	// write out the event itself
	MVByteBuffer subEventBuf = getContainedEvent().toBytes();
	buf.putByteBuffer(subEventBuf);
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();

	// read in the message id
	/* long playerId = */ buf.getLong();
	/* int msgId = */ buf.getInt();

// 	Log.debug("DirectedEvent.parseBytes: msgid=" + msgId);

	// read in the recipient list
	Collection<MVObject> col = new HashSet<MVObject>();
	int len = buf.getInt();
// 	Log.debug("DirectedEvent.parseBytes: recipient list size=" + len);
	for (int i=0; i<len; i++) {
	    Long oid = buf.getLong();
// 	    Log.debug("DirectedEvent.parseBytes: recipient oid: " + oid);
	    MVObject e = MVObject.getObject(oid);
	    if (e == null) {
		log.warn("could not find entity with oid " + oid);
		continue;
	    }
// 	    Log.debug("DirectedEvent.parseBytes: recipient: " + e);
	    col.add(e);
	}
	setRecipients(col);

	// parse the event
// 	Log.debug("DirectedEvent.parseBytes: parsing subevent");
	MVByteBuffer subBuf = buf.getByteBuffer();
	Event subEvent = Engine.getEventServer().parseBytes(subBuf, 
							    getConnection());
// 	Log.debug("DirectedEvent.parseBytes: subevent=" + subEvent.getName());
	setContainedEvent(subEvent);
    }

    public void setMessage(String msg) {
	mMessage = msg;
    }
    public String getMessage() {
	return mMessage;
    }

    private String mMessage = null;

    static final Logger log = new Logger("ComEvent");
}
