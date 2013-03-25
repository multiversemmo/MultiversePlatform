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
 * sends the client state information
 */
public class StateEvent extends Event {
    public StateEvent() {
	super();
    }

    public StateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

//     public StateEvent(MVObject targetOwner, 
// 		      MVObject objToAcquire) {
// 	super(targetOwner);
// 	setTargetObject(objToAcquire);
//     }

    public String getName() {
	return "StateEvent";
    }

    public void addState(String stateName, int val) {
	lock.lock();
	try {
	    stateMap.put(stateName.intern(), val);
	}
	finally {
	    lock.unlock();
	}
    }

    public Integer getState(String stateName) {
        lock.lock();
        try {
            return stateMap.get(stateName.intern());
        }       
        finally {
            lock.unlock();
        }
    }

    /**
     * returns a copy of the state map
     */
    public Map<String, Integer> getStateMap() {
	lock.lock();
	try {
	    return new HashMap<String, Integer>(stateMap);
	}
	finally {
	    lock.unlock();
	}
    }

    public void parseBytes(MVByteBuffer buf) {
	lock.lock();
	try {
	    buf.rewind();
	    
	    // standard stuff
	    long playerId = buf.getLong();
// 	    MVObject obj = MVObject.getObject(playerId);
// 	    if (obj == null) {
// 		throw new MVRuntimeException("StateEvent: no obj with id " + 
// 				      playerId);
// 	    }
	    setObjectOid(playerId);
	    
	    /* int msgId = */ buf.getInt();
	    
	    // read in # of states
	    int len = buf.getInt();
	    while(len > 0) {
		String stateName = buf.getString();
		int val = buf.getInt();
                if (Log.loggingDebug)
                    Log.debug("StateEvent.parseBytes: got state " +
			      stateName + "=" + val);
		addState(stateName.intern(), val);
		len--;
	    }
	}
	finally {
	    lock.unlock();
	}
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	lock.lock();
	try {
	    MVByteBuffer buf = new MVByteBuffer(400);
	    buf.putLong(getObjectOid()); 
	    buf.putInt(msgId);
	    
	    buf.putInt(stateMap.size());
	    Iterator<Map.Entry<String,Integer> > iter = stateMap.entrySet().iterator();
	    while (iter.hasNext()) {
		Map.Entry<String, Integer> entry = iter.next();
		String state = entry.getKey();
		Integer val = entry.getValue();
		buf.putString(state);
		buf.putInt(val);
                if (Log.loggingDebug)
                    Log.debug("StateEvent.toBytes: state=" + state + 
			      ", val=" + val);
	    }
	    buf.flip();
	    return buf;
	}
	finally {
	    lock.unlock();
	}
    }

    Map<String, Integer> stateMap = new HashMap<String, Integer>();
    transient Lock lock = LockFactory.makeLock("StateEvent");
}
