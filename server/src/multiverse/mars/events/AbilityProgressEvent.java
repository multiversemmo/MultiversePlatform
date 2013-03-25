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
import multiverse.mars.core.*;
import java.util.concurrent.locks.*;

// Update progress for the activation of an ability

public class AbilityProgressEvent extends Event {
    public AbilityProgressEvent() {
	super();
    }

    public AbilityProgressEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public AbilityProgressEvent(MarsAbility.State state) {
	super();
	setObjOid(state.getObject().getOid());
	setAbilityName(state.getAbility().getName());
	setState(state.getState().toString());
	setDuration(state.getDuration());
	setEndTime(calculateEndTime(state));
    }

    protected long calculateEndTime(MarsAbility.State state) {
	MarsAbility ability = state.getAbility();

	switch (state.getState()) {
	case ACTIVATING:
	    return state.getNextWakeupTime();
	case CHANNELLING:
	    int pulsesRemaining = ability.getChannelPulses() - state.getNextPulse() - 1;
	    long endTime = state.getNextWakeupTime() + (pulsesRemaining * ability.getChannelPulseTime());
	    return endTime;
	default:
	    return 0;
	}
    }

    public String getName() {
	return "AbilityProgressEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(400);

        lock.lock();
        try {
	    buf.putLong(objOid);
	    buf.putInt(msgId);
	    buf.putString(abilityName);
	    buf.putString(state);
	    buf.putLong(duration);
	    buf.putLong(endTime);
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
	    setState(buf.getString());
	    setDuration(buf.getLong());
	    setEndTime(buf.getLong());
        }
        finally {
            lock.unlock();
        }
    }

    public long getObjOid() { return objOid; }
    public void setObjOid(long oid) { objOid = oid; }
    protected long objOid;

    public String getAbilityName() { return abilityName; }
    public void setAbilityName(String name) { abilityName = name; }
    protected String abilityName;

    public String getState() { return state; }
    public void setState(String state) { this.state = state; }
    protected String state;

    public long getDuration() { return duration; }
    public void setDuration(long duration) { this.duration = duration; }
    protected long duration;

    public long getEndTime() { return endTime; }
    public void setEndTime(long time) { endTime = time; }
    protected long endTime;

    transient Lock lock = LockFactory.makeLock("AbilityInfoEvent");
}
