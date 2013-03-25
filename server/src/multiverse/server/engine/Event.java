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

import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

// checked for locks

abstract public class Event {

    public Event() {
    }

    // construct this event from a byte buffer
    public Event(MVByteBuffer buf, ClientConnection con) {
	try {
	    parseBytes(buf);
	    this.con = con;
	    this.buffer = buf;
	}
	catch(MVRuntimeException e) {
	    Log.error("Event constructor: failed to parse bytes");
	}
    }

    public Event(Entity obj) {
	setEntity(obj);
    }
    
    public Event(Long oid) {
	eventObjOid = oid;
    }

//     public Event(MVObject obj) {
// 	// set the connection so that the TestClient look up works
// 	if( !Engine.isEntityManager() && obj != null ) {
// 	    this.con = obj.getRemoteObjectConnection();
// 	}
//     }

    public String toString() {
	return "[Event: " + getName() + "]";
    }
    abstract public String getName();

    abstract public MVByteBuffer toBytes();

    /**
     * load this event from the passed in buffer
     * called by the constructor
     */
    abstract protected void parseBytes(MVByteBuffer buf);

    // is this event for/about the object
//     public boolean isForObject(MVObject obj) {
// 	return Entity.equals(getObject(), obj);
//     }

    // usually the subject of the event (if comevent - then mob saying it)
    // you want to set this object based on who should 'know' about it
    // for example, if its a damage messge, it should be sent to everyone
    // who knows about the target, so you would make the target the 'object'
    public void setEntity(Entity obj) {
        if (obj != null) {
            eventObjOid = obj.getOid();
        }
    }
    public void setObject(MVObject obj) {
        if (obj != null) {
            eventObjOid = obj.getOid();
        }
    }
    public void setObjectOid(Long objOid) {
        eventObjOid = objOid;
    }
    public void setObjectOid(long objOid) {
        eventObjOid = new Long(objOid);
    }
    
    public Long getObjectOid() {
        return eventObjOid;
    }

    public void setConnection(ClientConnection con) {
	this.con = con;
    }

    public ClientConnection getConnection() {
	return con;
    }

    public void setBuffer(MVByteBuffer buf) {
	buffer = buf;
    }

    public void setEnqueueTime(long time) {
	enqueueTime = time;
    }   
    public long getEnqueueTime() {
	return enqueueTime;
    }   

    /**
     * you should rewind this before using it
     */
    public MVByteBuffer getBuffer() {
	return buffer;
    }
    private Long eventObjOid = null;
    private ClientConnection con = null;
    private MVByteBuffer buffer = null;
    private long enqueueTime = 0;
}
