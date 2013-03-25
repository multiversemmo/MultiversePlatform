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
import multiverse.server.objects.*;
import multiverse.server.network.*;

/**
 * object is dropping the a diff obj from its inventory
 */
public class DropResponseEvent extends Event {
    public DropResponseEvent() {
	super();
    }

    public DropResponseEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public DropResponseEvent(MVObject dropper, 
			     MVObject obj, 
			     String slot, 
			     boolean status) {
	super(obj);
	setDropper(dropper);
	setSlotName(slot);
	setStatus(status);
    }

    public String getName() {
	return "DropEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getDropper().getOid()); 
	buf.putInt(msgId);
	
	buf.putLong(getObjectOid());
	buf.putString(getSlotName());
	buf.putInt(getStatus()?1:0);
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();

	long playerId = buf.getLong();
	setDropper(MVObject.getObject(playerId));

	/* int msgId = */ buf.getInt();

	long objId = buf.getLong();
	setObjectOid(objId);

	setSlotName(buf.getString());
	setStatus(buf.getInt() == 1);
    }

    public void setDropper(MVObject dropper) {
	this.dropper = dropper;
    }
    public MVObject getDropper() {
	return dropper;
    }

    public void setSlotName(String slotName) {
	this.slotName = slotName;
    }
    public String getSlotName() {
	return slotName;
    }

    public void setStatus(boolean status) {
	this.status = status;
    }
    public boolean getStatus() {
	return status;
    }

    private MVObject dropper = null;
    private String slotName = null;
    private boolean status;
}
