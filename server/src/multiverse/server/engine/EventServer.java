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

package multiverse.server.engine;

import java.util.*;
import java.util.concurrent.locks.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

// checked for locks

public class EventServer {
    public EventServer() {
    }

    /**
     * finds the correct event based on the id, parses the message
     * and returns an event
     */
    public Event parseBytes(MVByteBuffer buf,
			    ClientConnection con) {
	long playerId = buf.getLong();
	int eventID = buf.getInt();
        buf.rewind();

	Class eventClass = null;

	lock.lock();
	try {
	    eventClass = eventIdMapping.get(eventID);
            if (Log.loggingDebug)
                Log.debug("EventServer.parsebytes: id=" + eventID +
                          ((eventClass != null) ? 
                           (", found event class: " + eventClass.getName()) : ""));
	}
	finally {
	    lock.unlock();
	}

	if (eventClass == null) {
	    Log.error("found no event class for oid " + playerId + ", id " + eventID);
	    Log.dumpStack("Event.parseBytes");
	    return null;
	}
	
	try {
	    Object obj = eventClass.newInstance();
	    if (obj == null) {
		throw new MVRuntimeException("EventServer: constructor.newisntance returned null");
	    }
	    if (! (obj instanceof Event)) {
		throw new MVRuntimeException("EventServer: new instance is not an event");
	    }
	    Event event = (Event) obj;
	    event.setConnection(con);
	    event.setBuffer(buf);
	    event.parseBytes(buf);
	    return event;
	}
	catch(Exception e) {
	    throw new MVRuntimeException("EventServer.parseBytes: could not get constructor", e);
	}
    }

    /** 
     * registers an event id number with the event class
     * so that when the server gets an event over the network
     * it knows which event class to dispatch it to for 
     * de-serialization
     */
    public void registerEventId(int id, String className) {
	lock.lock();
	try {
	    Class eventClass = Class.forName(className);
            if (Log.loggingDebug)
                Log.debug("loaded event, event id#" +
                          id + " maps to '" + className + "'");

	    eventIdMapping.put(id, eventClass);
	    eventClassMapping.put(eventClass, id);
	}
	catch(Exception e) {
	    throw new MVRuntimeException("EventServer: could not find/instantiate class '" + className + "': " + e);
	}
	finally {
	    lock.unlock();
	}
    }

    public Class getEventClass(int id) {
        lock.lock();
        try {
            return eventIdMapping.get(id);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * returns the id that was registered for the passed in class
     */
    public int getEventID(Class eventClass) {
	lock.lock();
	try {
	    Integer id = eventClassMapping.get(eventClass);
	    if (id == null) {
		throw new MVRuntimeException("EventServer.getEventId: id is null");
	    }
	    return id.intValue();
	}
	finally {
	    lock.unlock();
	}
    }

    public int getEventID(String className) {
	try {
	    Class eventClass = Class.forName(className);
	    return getEventID(eventClass);
	}
	catch(Exception e) {
	    throw new MVRuntimeException("EventServer.getEventID", e);
	}
    }

    // maps from an event id to the event class
    // used when servers gets a msg and needs to look up what event to
    // deserialize it with
    private Map<Integer, Class> eventIdMapping = new HashMap<Integer,Class>();

    // maps the reverse - from the event class to the id, so that
    // the event.toBytes() method knows what id it should use when
    // making the byte buffer
    private Map<Class, Integer> eventClassMapping = 
	new HashMap<Class,Integer>();

    transient Lock lock = LockFactory.makeLock("EventServerLock");

}
