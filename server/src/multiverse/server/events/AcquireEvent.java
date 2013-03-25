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
 * an object is attemping to acquire another object
 */
public class AcquireEvent extends Event {
    public AcquireEvent() {
	super();
    }

    public AcquireEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public AcquireEvent(MVObject targetOwner, 
			MVObject objToAcquire) {
	setTargetOwner(targetOwner);
	setTargetObject(objToAcquire);
    }

    public String getName() {
	return "AcquireEvent";
    }


    public void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	
	// standard stuff
	long playerId = buf.getLong();
	setTargetOwner(MVObject.getObject(playerId));
	/* int msgId = */ buf.getInt();

	// data
	long objId = buf.getLong();
	setTargetObject(MVObject.getObject(objId));
        if (getTargetObject() == null) {
            if (Log.loggingDebug)
                Log.debug("AcquireEvent.parseBytes: targetobject is null, oid=" +
                          objId);
        }
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getTargetOwner().getOid()); 
	buf.putInt(msgId);
	buf.putLong(getTargetObject().getOid());
	buf.flip();
	return buf;
    }

    //
    // the object attempting to acquire the object
    //
    public void setTargetOwner(MVObject targetOwner) {
	this.targetOwner = targetOwner;
    }

    public MVObject getTargetOwner() {
	return targetOwner;
    }
    
    public void setTargetObject(MVObject targetObject) {
	this.targetObject = targetObject;
    }
    public MVObject getTargetObject() {
	return targetObject;
    }

    private MVObject targetOwner = null;
    private MVObject targetObject = null;
}
