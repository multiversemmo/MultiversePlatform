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

import java.util.*;
import java.io.*;
import multiverse.server.util.*;
import java.util.concurrent.locks.*;

public class MarsEquipSlot implements Serializable {
    public MarsEquipSlot() {
    }

    public MarsEquipSlot(String slotName) {
	this.name = slotName;
	mapLock.lock();
	try {
	    slotNameMapping.put(slotName, this);
	}
	finally {
	    mapLock.unlock();
	}
    }

    public void setName(String name) {
        this.name = name;
    }
    public String getName() {
	return name;
    }
    private String name = null;

    public int hashCode() {
        return name.hashCode();
    }
    public boolean equals(Object other) {
        if (other instanceof MarsEquipSlot) {
            MarsEquipSlot otherSlot = (MarsEquipSlot)other;
            return otherSlot.getName().equals(name);
        }
        return false;
    }

    public String toString() {
	return "[MarsEquipSlot name=" + getName() + "]";
    }

    public static MarsEquipSlot getSlotByName(String slotName) {
	mapLock.lock();
	try {
	    return slotNameMapping.get(slotName);
	}
	finally {
	    mapLock.unlock();
	}
    }
    private static Map<String, MarsEquipSlot> slotNameMapping =
	new HashMap<String, MarsEquipSlot>();

    private static Lock mapLock = LockFactory.makeLock("MarsEquipSlot");

    public static MarsEquipSlot PRIMARYWEAPON = 
	new MarsEquipSlot("primaryWeapon");

    public static MarsEquipSlot CHEST = 
	new MarsEquipSlot("chest");

    public static MarsEquipSlot LEGS = 
	new MarsEquipSlot("legs");

    public static MarsEquipSlot HEAD = 
	new MarsEquipSlot("head");

    public static MarsEquipSlot FEET = 
	new MarsEquipSlot("feet");

    public static MarsEquipSlot HANDS = 
	new MarsEquipSlot("hands");

    private static final long serialVersionUID = 1L;
}
