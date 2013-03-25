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
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.mars.objects.*;
import multiverse.mars.core.*;
import java.util.concurrent.locks.*;

// Activate an ability

public class AbilityActivateEvent extends Event {
    public AbilityActivateEvent() {
	super();
    }

    public AbilityActivateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public AbilityActivateEvent(MarsMob obj, MarsAbility ability, MarsObject target, MarsItem item) {
	super();
	setObjOid(obj.getOid());
	setAbilityName(ability.getName());
	if (target != null) {
	    setTargetOid(target.getOid());
	}
	if (item != null) {
	    setItemOid(item.getOid());
	}
    }

    public String getName() {
	return "AbilityActivateEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(200);

        lock.lock();
        try {
	    buf.putLong(objOid);
	    buf.putInt(msgId);
	    buf.putString(abilityName);
	    buf.putLong(targetOid);
	    buf.putLong(itemOid);
        }
        finally {
            lock.unlock();
        }

	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
        lock.lock();
        try {
	    buf.rewind();

	    setObjOid(buf.getLong());
	    /* int msgId = */ buf.getInt();
	    setAbilityName(buf.getString());
	    setTargetOid(buf.getLong());
	    setItemOid(buf.getLong());
        }
        finally {
            lock.unlock();
        }
    }

    public long getObjOid() { return objOid; }
    public void setObjOid(long oid) { objOid = oid; }
    protected long objOid;

    public long getTargetOid() { return targetOid; }
    public void setTargetOid(long oid) { targetOid = oid; }
    protected long targetOid = -1;

    public String getAbilityName() { return abilityName; }
    public void setAbilityName(String name) { abilityName = name; }
    protected String abilityName;

    public long getItemOid() { return itemOid; }
    public void setItemOid(long oid) { itemOid = oid; }
    protected long itemOid = -1;

    transient Lock lock = LockFactory.makeLock("AbilityInfoEvent");
}
