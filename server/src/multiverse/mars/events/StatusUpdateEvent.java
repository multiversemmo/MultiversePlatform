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
import multiverse.mars.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

/**
 * we actually copy the attributes into the event (instead of just
 * storing the obj ref) because when the
 * event goes over the wire, the other server/client wont have the
 * correct values unless its part of the event
 */
public class StatusUpdateEvent extends MarsEvent {
    public StatusUpdateEvent() {
	super();
    }

    public StatusUpdateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public StatusUpdateEvent(MarsObject obj) {
	super(obj);
	setBody(obj.getBody());
	setCurrentBody(obj.getCurrentBody());
	setStun(obj.getStun());
	setCurrentStun(obj.getCurrentStun());
	if (obj instanceof MarsMob) {
	    MarsMob mob = (MarsMob) obj;
	    setEndurance(mob.getEndurance());
	    setCurrentEndurance(mob.getCurrentEndurance());
	    setPD(obj.getPD());
	}
	else {
	    setEndurance(0);
	    setCurrentEndurance(0);
	    setPD(0);
	}
    }

    public String getName() {
	return "StatusUpdateEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(4000);
	buf.putLong(getObjectOid()); 
	buf.putInt(msgId);
	
	// send the # of attributes we are sending over
	// for now, stun & body
	buf.putInt(7);

	buf.putString("stun");
	buf.putInt(this.getStun());

	buf.putString("stun_cur");
	buf.putInt(this.getCurrentStun());

	buf.putString("body");
	buf.putInt(this.getBody());

	buf.putString("body_cur");
	buf.putInt(this.getCurrentBody());

	buf.putString("end");
	buf.putInt(this.getEndurance());

	buf.putString("end_cur");
	buf.putInt(this.getCurrentEndurance());

	buf.putString("pd");
	buf.putInt(this.getPD());

	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setObjectOid(buf.getLong());
	/* int msgId = */ buf.getInt();

	int numAttr = buf.getInt();
	while(numAttr > 0) {
	    processAttribute(buf);
	    numAttr--;
	}
    }

    // helper method
    private void processAttribute(MVByteBuffer buf) {
	String attr = buf.getString();
	if (attr.equals("stun")) {
	    setStun(buf.getInt());
	}
	else if (attr.equals("stun_cur")) {
	    setCurrentStun(buf.getInt());
	}
	else if (attr.equals("body")) {
	    setBody(buf.getInt());
	}
	else if (attr.equals("body_cur")) {
	    setCurrentBody(buf.getInt());
	}
	else if (attr.equals("end")) {
	    setEndurance(buf.getInt());
	}
	else if (attr.equals("end_cur")) {
	    setCurrentEndurance(buf.getInt());
	}
	else if (attr.equals("pd")) {
	    setPD(buf.getInt());
	}
	else {
	    int val = buf.getInt();
	    log.warn("unknown attr: " + attr + ", val=" + val);
	}
    }

    public void setStun(int stun) {
	this.stun = stun;
    }
    public int getStun() {
	return stun;
    }
    public void setCurrentStun(int stun) {
	this.current_stun = stun;
    }
    public int getCurrentStun() {
	return current_stun;
    }
    
    public void setBody(int body) {
	this.body = body;
    }
    public int getBody() {
	return body;
    }
    public void setCurrentBody(int body) {
	this.current_body = body;
    }
    public int getCurrentBody() {
	return current_body;
    }

    public void setEndurance(int end) {
	this.end = end;
    }
    public int getEndurance() {
	return end;
    }
    public void setCurrentEndurance(int end) {
	this.current_end = end;
    }
    public int getCurrentEndurance() {
	return current_end;
    }

    public void setPD(int pd) {
	this.pd = pd;
    }
    public int getPD() {
	return pd;
    }

    private int stun = 0;
    private int body = 0;
    private int end = 0;
    private int pd = 0;
    private int current_stun = 0;
    private int current_body = 0;
    private int current_end = 0;

    static Logger log = new Logger("StatusUpdateEvent");
}
