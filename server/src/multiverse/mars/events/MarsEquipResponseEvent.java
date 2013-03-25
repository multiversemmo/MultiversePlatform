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
import multiverse.server.util.*;
import multiverse.mars.objects.*;

public class MarsEquipResponseEvent extends Event {

    public MarsEquipResponseEvent() {
	super();
    }

    public MarsEquipResponseEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public MarsEquipResponseEvent(MarsMob equipper, 
				  MarsItem obj, 
				  String slotName,
				  boolean success) {
	super(equipper);
	setObjToEquip(obj);
	setSlotName(slotName);
	setSuccess(success);
    }

    public String getName() {
	return "MarsEquipResponseEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);

	buf.putLong(getObjToEquip().getOid());
	buf.putString(getSlotName());
	buf.putInt(getSuccess()?1:0);
	
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	MVObject obj = MVObject.getObject(buf.getLong());
	if (! (obj.isMob())) {
	    throw new MVRuntimeException("EquipResponseEvent.parseBytes: not a mob");
	}
	setEquipper(MarsMob.convert(obj));

	/* int msgId = */ buf.getInt();
	
	setObjToEquip(MarsItem.convert(MVObject.getObject(buf.getLong())));
	setSlotName(buf.getString());
	setSuccess(buf.getInt() == 1);
    }

    public void setEquipper(MarsMob mob) {
	setObject(mob);
    }

    public void setObjToEquip(MarsItem item) {
	objToEquip = item;
    }
    public MVObject getObjToEquip() {
	return objToEquip;
    }

    public void setSuccess(boolean success) {
	this.success = success;
    }
    public boolean getSuccess() {
	return success;
    }
    public void setSlotName(String slotName) {
	this.slotName = slotName;
    }
    public String getSlotName() {
	return slotName;
    }

    private MarsItem objToEquip = null;
    private boolean success = false;
    private String slotName = null;
}
