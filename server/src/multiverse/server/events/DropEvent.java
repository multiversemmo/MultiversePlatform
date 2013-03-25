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

// obj is dropping something from its container

public class DropEvent extends Event {

    public DropEvent() {
	super();
    }

    public DropEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public DropEvent(MVObject dropper, MVObject obj) {
	super(dropper);
	setObjToDrop(obj);
    }

    public String getName() {
	return "DropEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getObjectOid()); 
	buf.putInt(msgId);
	buf.putLong(getObjToDrop().getOid());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	
	// standard stuff
	long playerId = buf.getLong();
	setDropper(MVObject.getObject(playerId));

	/* int msgId = */ buf.getInt();

	setObjToDrop(MVObject.getObject(buf.getLong()));
    }

    public void setDropper(MVObject dropper) {
	setObject(dropper);
    }

    public void setObjToDrop(MVObject obj) {
	objToDrop = obj;
    }
    public MVObject getObjToDrop() {
	return objToDrop;
    }

    private MVObject objToDrop = null;
}
