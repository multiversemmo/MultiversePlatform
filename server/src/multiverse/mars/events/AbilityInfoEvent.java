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
import java.util.*;
import java.util.concurrent.locks.*;

// Provide information about a specific ability

public class AbilityInfoEvent extends Event {
    public AbilityInfoEvent() {
	super();
    }

    public AbilityInfoEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public AbilityInfoEvent(MarsAbility ability) {
	super();
	setAbilityName(ability.getName());
	setIcon(ability.getIcon());
	setDesc("");
	for (String cooldownID : ability.getCooldownMap().keySet()) {
	    addCooldown(cooldownID);
	}
	setProperty("targetType", ability.getTargetType().toString());
	setProperty("minRange", Integer.toString(ability.getMinRange()));
	setProperty("maxRange", Integer.toString(ability.getMaxRange()));
	setProperty("costProp", ability.getCostProperty());
        setProperty("cost", Integer.toString(ability.getActivationCost()));
    }

    public String getName() {
	return "AbilityInfoEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(400);

        lock.lock();
        try {
	    buf.putInt(-1); // dummy PlayerID
	    buf.putInt(msgId);
	
	    buf.putString(abilityName);
	    buf.putString(icon);
	    buf.putString(desc);

            int size = cooldowns.size();
            buf.putInt(size);
            for(String cooldown : cooldowns) {
                buf.putString(cooldown);
            }
	    size = props.size();
	    buf.putInt(size);
	    for(Map.Entry<String, String> entry : props.entrySet()) {
		buf.putString(entry.getKey());
		buf.putString(entry.getValue());
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

	    buf.getInt(); // dummy playerID
	    /* int msgId = */ buf.getInt();

	    setAbilityName(buf.getString());
	    setIcon(buf.getString());
	    setDesc(buf.getString());

            int size = buf.getInt();
            cooldowns = new HashSet<String>(size);
            while (size-- > 0) {
                String cooldown = buf.getString();
		cooldowns.add(cooldown);
            }
	    size = buf.getInt();
	    props = new HashMap<String, String>(size);
	    while (size-- > 0) {
		String key = buf.getString();
		String value = buf.getString();
		setProperty(key, value);
	    }
        }
        finally {
            lock.unlock();
        }
    }

    public String getAbilityName() { return abilityName; }
    public void setAbilityName(String abilityName) { this.abilityName = abilityName; }
    protected String abilityName;

    public String getIcon() { return icon; }
    public void setIcon(String icon) { this.icon = icon; }
    protected String icon;

    public String getDesc() { return desc; }
    public void setDesc(String desc) { this.desc = desc; }
    protected String desc;

    public void addCooldown(String cooldownID) {
	lock.lock();
	try {
	    if (cooldowns == null) {
		cooldowns = new HashSet<String>();
	    }
	    cooldowns.add(cooldownID);
	}
	finally {
	    lock.unlock();
	}
    }
    public Set<String> getCooldowns() {
	lock.lock();
	try {
	    return new HashSet<String>(cooldowns);
	}
	finally {
	    lock.unlock();
	}
    }
    protected Set<String> cooldowns = null;

    public String getProperty(String key) { return props.get(key); }
    public void setProperty(String key, String value) {
	lock.lock();
	try {
	    if (props == null) {
		props = new HashMap<String, String>();
	    }
	    props.put(key, value);
	}
	finally {
	    lock.unlock();
	}
    }
    protected Map<String, String> props = null;

    transient Lock lock = LockFactory.makeLock("AbilityInfoEvent");
}
