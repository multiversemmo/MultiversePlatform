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
import java.util.*;
import java.util.concurrent.locks.*;

// Update the list of abilities that this object knows

public class AbilityUpdateEvent extends Event {
    public AbilityUpdateEvent() {
	super();
    }

    public AbilityUpdateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public AbilityUpdateEvent(MarsObject obj) {
	super(obj);
	setObjOid(obj.getOid());
	for(MarsAbility.Entry entry : obj.getAbilityMap().values()) {
	    addAbilityEntry(entry);
	}
    }

    public String getName() {
	return "AbilityUpdateEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(500);

        lock.lock();
        try {
	    buf.putLong(objOid);
	    buf.putInt(msgId);
	
	    int size = abilityEntrySet.size();
	    buf.putInt(size);
	    for(MarsAbility.Entry entry : abilityEntrySet) {
		buf.putString(entry.getAbilityName());
		buf.putString(entry.getIcon());
		buf.putString(entry.getCategory());
	    }
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

            int size = buf.getInt();
	    abilityEntrySet = new HashSet<MarsAbility.Entry>(size);
	    while (size-- > 0) {
		String name = buf.getString();
		String icon = buf.getString();
		String category = buf.getString();
		addAbilityEntry(new MarsAbility.Entry(name, icon, category));
	    }
        }
        finally {
            lock.unlock();
        }
    }

    public long getObjOid() { return objOid; }
    public void setObjOid(long oid) { objOid = oid; }
    protected long objOid;

    public void addAbilityEntry(MarsAbility.Entry entry) {
	lock.lock();
	try {
	    abilityEntrySet.add(entry);
	}
	finally {
	    lock.unlock();
	}
    }
    public Set<MarsAbility.Entry> getAbilityEntrySet() {
	lock.lock();
	try {
	    return new HashSet<MarsAbility.Entry>(abilityEntrySet);
	}
	finally {
	    lock.unlock();
	}
    }
    public void setAbilityEntrySet(Set<MarsAbility.Entry> set) {
	lock.lock();
	try {
	    abilityEntrySet = new HashSet<MarsAbility.Entry>(set);
	}
	finally {
	    lock.unlock();
	}
    }
    protected Set<MarsAbility.Entry> abilityEntrySet = null;

    transient Lock lock = LockFactory.makeLock("AbilityInfoEvent");
}
