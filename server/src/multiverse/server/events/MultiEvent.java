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
import multiverse.server.util.*;
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * an object is attemping to acquire another object
 */
public class MultiEvent extends Event {
    public MultiEvent() {
	super();
    }

    public MultiEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public String getName() {
	return "MultiEvent";
    }

    public void add(Event event) {
        lock.lock();
        try {
            events.add(event);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * sets the list of events to be sent over, makes a copy of the list
     * shallow copy
     */
    public void setEvents(List<Event> events) {
        lock.lock();
        try {
            this.events = new LinkedList<Event>(events);
        }
        finally {
            lock.unlock();
        }
    }
    /**
     * returns a shallow copy of the events
     */
    public List<Event> getEvents() {
        lock.lock();
        try {
            return new LinkedList<Event>(events);
        }
        finally {
            lock.unlock();
        }
    }

    public void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	
	// standard stuff
	/* long dummyId = */ buf.getLong();
	/* int msgId = */ buf.getInt();

	// data
	/* long objId = */ buf.getLong();
        int size = buf.getInt();
        List<Event> events = new LinkedList<Event>();
        while(size>0) {
            /* MVByteBuffer subBuf = */ buf.getByteBuffer();
            // parse into an event now
            size--;
        }
        setEvents(events);
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
        List<MVByteBuffer> bufList = new LinkedList<MVByteBuffer>();
        int payloadSize = 2000;
        lock.lock();
        try {
            // turn all the events into buffers
            for (Event event : events) {
                MVByteBuffer buf = event.toBytes();
                bufList.add(buf);
                payloadSize += buf.limit();
            }

            // add all the buffers to one large buffer
            if (Log.loggingDebug)
                log.debug("tobytes: making new buffer size " + payloadSize);
            MVByteBuffer multiBuf = new MVByteBuffer(payloadSize);
            multiBuf.putLong(-1); 
            multiBuf.putInt(msgId);
            multiBuf.putInt(bufList.size());
            for (MVByteBuffer buf : bufList) {
                multiBuf.putByteBuffer(buf);
            }
            multiBuf.flip();
            return multiBuf;
        }
        finally {
            lock.unlock();
        }
    }

    private List<Event> events = new LinkedList<Event>();
    transient private Lock lock = LockFactory.makeLock("MultiEventLock");
    protected static final Logger log = new Logger("MultiEvent");
}
