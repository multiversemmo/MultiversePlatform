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
import multiverse.mars.objects.*;

// mob is unequiping obj
public class MarsUnequipEvent extends Event {
    public MarsUnequipEvent() {
	super();
    }

    public MarsUnequipEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public MarsUnequipEvent(MarsMob unequipper, 
			    MarsItem objToUnequip, 
			    String slotName) {
	super(unequipper);
	setObjToUnequip(objToUnequip);
	setSlotName(slotName);
    }

    public String getName() {
	return "UnequipEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getObjectOid()); 
	buf.putInt(msgId);
	buf.putLong(getObjToUnequip().getOid());
	buf.putString(getSlotName());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setUnequipper(MarsMob.convert(MVObject.getObject(buf.getLong())));
	/* int msgId = */ buf.getInt();
	setObjToUnequip(MarsItem.convert(MVObject.getObject(buf.getLong())));
	setSlotName(buf.getString());
    }

    public void setUnequipper(MarsMob mob) {
	setObject(mob);
    }

    public void setObjToUnequip(MarsItem obj) {
	objToUnequip = obj;
    }
    public MarsItem getObjToUnequip() {
	return objToUnequip;
    }

    public void setSlotName(String slotName) {
	this.slotName = slotName;
    }
    public String getSlotName() {
	return slotName;
    }

    private MarsItem objToUnequip = null;
    private String slotName = null;
}
