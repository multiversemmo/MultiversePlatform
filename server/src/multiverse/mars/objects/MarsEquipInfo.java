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

package multiverse.mars.objects;

import multiverse.server.util.*;
import java.util.*;
import java.util.concurrent.locks.*;
import java.io.*;

/**
 * stores information about how to handle equipping.
 * says what equipslots are valid.
 * says what socket the equipslot maps to.
 * marsmobs all refer to an object like this
 */
public class MarsEquipInfo implements Cloneable, Serializable {
    public MarsEquipInfo() {
	setupTransient();
    }

    public MarsEquipInfo(String name) {
	setupTransient();
        setName(name);
    }

    public String toString() {
        localLock.lock();
        try {
            String s = "[MarsEquipInfo: name=" + name;
            for (MarsEquipSlot slot : equipSlots) {
                s += ", slot=" + slot;
            }
            return s + "]";
        }
        finally {
            localLock.unlock();
        }
    }

    public String getName() {
	return name;
    }
    public void setName(String name) {
	staticMapLock.lock();
	try {
            this.name = name;
	    equipInfoMap.put(name, this);
	}
	finally {
	    staticMapLock.unlock();
	}
    }
    private String name;


    public void addEquipSlot(MarsEquipSlot slot) {
	localLock.lock();
	try {
	    equipSlots.add(slot);
	}
	finally {
	    localLock.unlock();
	}
    }
    public List<MarsEquipSlot> getEquippableSlots() {
	localLock.lock();
	try {
	    return new ArrayList<MarsEquipSlot>(equipSlots);
	}
	finally {
	    localLock.unlock();
	}
    }
    public void setEquippableSlots(List<MarsEquipSlot> slots) {
        localLock.lock();
        try {
            equipSlots = new ArrayList<MarsEquipSlot>(slots);
        }
        finally {
            localLock.unlock();
        }
    }
    List<MarsEquipSlot> equipSlots = new ArrayList<MarsEquipSlot>();
    
    public static MarsEquipInfo getEquipInfo(String name) {
	staticMapLock.lock();
	try {
	    return equipInfoMap.get(name);
	}
	finally {
	    staticMapLock.unlock();
	}
    }

    private static Map<String, MarsEquipInfo> equipInfoMap =
	new HashMap<String, MarsEquipInfo>();


    private static Lock staticMapLock = LockFactory.makeLock("StaticMarsEquipInfo");
    transient private Lock localLock = null;

    void setupTransient() {
	localLock = LockFactory.makeLock("MarsEquipInfo");
    }
    private void readObject(ObjectInputStream in) 
	throws IOException, ClassNotFoundException {
	in.defaultReadObject();
	setupTransient();
    }

    // define the standard mob equippable slots
    public static MarsEquipInfo DefaultEquipInfo =
	new MarsEquipInfo("MarsDefaultEquipInfo");
    static {
	DefaultEquipInfo.addEquipSlot(MarsEquipSlot.PRIMARYWEAPON);
	DefaultEquipInfo.addEquipSlot(MarsEquipSlot.CHEST);
	DefaultEquipInfo.addEquipSlot(MarsEquipSlot.LEGS);
	DefaultEquipInfo.addEquipSlot(MarsEquipSlot.HEAD);
	DefaultEquipInfo.addEquipSlot(MarsEquipSlot.FEET);
	DefaultEquipInfo.addEquipSlot(MarsEquipSlot.HANDS);
    }

    private static final long serialVersionUID = 1L;
}
