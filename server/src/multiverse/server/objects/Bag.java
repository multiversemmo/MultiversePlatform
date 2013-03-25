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

package multiverse.server.objects;

import java.util.*;

import multiverse.server.engine.*;

/**
 * a bag used to hold objects, either items or other bags
 */
public class Bag extends Entity {
    public Bag() {
        super();
        setNamespace(Namespace.BAG);
        this.setName("Bag");
        this.setNumSlots(0);
    }
    
    public Bag(Long oid) {
        super(oid);
        setNamespace(Namespace.BAG);
    }

    public Bag(int numSlots) {
        super();
        setNamespace(Namespace.BAG);
        this.setName("Bag");
        setNumSlots(numSlots);
    }

    public ObjectType getType() {
        return ObjectTypes.bag;
    }

    public int getNumSlots() {
        return numSlots;
    }

    /**
     * should only be set once
     */
    public void setNumSlots(int numSlots) {
        items = new ArrayList<Long>();
        for (int i=0; i<numSlots; i++)
            items.add(null);
        this.numSlots = numSlots;
    }

    /**
     * places item into specified slot. slotNum starts with 0 returns false if
     * there already is an item
     */
    public boolean putItem(int slotNum, Long itemOid) {
        lock.lock();
        try {
            // make sure the slot is within range
            if (slotNum >= numSlots) {
                return false;
            }

            // make sure slot is empty
            if (items.get(slotNum) != null) {
                return false;
            }

            // add item into slot
            items.set(slotNum, itemOid);
            return true;
        } finally {
            lock.unlock();
        }
    }

    public Long getItem(int slotNum) {
        lock.lock();
        try {
            if (slotNum >= numSlots) {
                return null;
            }
            return items.get(slotNum);
        } finally {
            lock.unlock();
        }
    }

    /**
     * add item to next available slot
     */
    public boolean addItem(Long oid) {
        lock.lock();
        try {
            for (int i = 0; i < numSlots; i++) {
                if (getItem(i) == null) {
                    putItem(i, oid);
                    return true;
                }
            }
            return false;
        } finally {
            lock.unlock();
        }
    }

    public boolean removeItem(Long oid) {
        lock.lock();
        try {
            Integer slotNum = findItem(oid);
            if (slotNum == null) {
                return false;
            }
            items.set(slotNum, null);
            return true;
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * java beans paradigm for saving into the databse
     */
    public void setItemsList(Long[] items) {
        lock.lock();
        try {
            this.items = new ArrayList<Long>();
            for (Long longVal : items)
                this.items.add(longVal);
            numSlots = items.length;
        } finally {
            lock.unlock();
        }
    }

    public Long[] getItemsList() {
        lock.lock();
        try {
            Long[] copy = new Long[numSlots];
            for (int i=0; i<numSlots; i++)
                copy[i] = items.get(i);
            return copy;
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the slotnumber where the item is located in this bag, or null if not
     * found
     * @param itemOid oid for the item you are looking for
     * @return slotnumber or null if not found
     */
    public Integer findItem(Long itemOid) {
        lock.lock();
        try {
            for (int i=0; i<getNumSlots(); i++) {
                if (itemOid.equals(items.get(i))) {
                    return i;
                }
            }
            return null;
        }
        finally {
            lock.unlock();
        }
    }
    
    private int numSlots;

    // contains the item's oid
    private ArrayList<Long> items = new ArrayList<Long>();

    private static final long serialVersionUID = 1L;
}
