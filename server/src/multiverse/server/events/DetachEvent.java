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

public class DetachEvent extends Event {

    public DetachEvent() {
	super();
    }

    public DetachEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public DetachEvent(Long detacherOid, Long detachObjOid, String socketName) {
	super.setObjectOid(detacherOid);
	setObjToDetach(detachObjOid);
	setSocketName(socketName);
    }

    public String getName() {
	return "DetachEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getDetacher());
	buf.putInt(msgId);

	buf.putLong(getObjToDetach());
	buf.putString(getSocketName());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setDetacher(buf.getLong());
	/* int msgId = */ buf.getInt();
	setObjToDetach(buf.getLong());
	setSocketName(buf.getString());
    }

    public void setDetacher(Long detacher) {
	setObjectOid(detacher);
    }
    public Long getDetacher() {
	return getObjectOid();
    }

    public void setObjToDetach(Long oid) {
	objToDetach = oid;
    }
    public Long getObjToDetach() {
	return objToDetach;
    }

    public void setSocketName(String socketName) {
	this.socketName = socketName;
    }
    public String getSocketName() {
	return socketName;
    }

    private Long objToDetach = null;
    private String socketName = null;
}
