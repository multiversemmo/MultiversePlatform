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

import multiverse.server.util.*;
import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;

/**
 * A base class for classes the require a name and a set of
 * properties, but which is not persistable
 */
public class NamedPropertyClass implements Serializable {

    public NamedPropertyClass() {
        setupTransient();
    }
    
    public NamedPropertyClass(String name) {
        setupTransient();
        setName(name);
    }
    
    // called from constructor and readObject
    protected void setupTransient() {
        lock = LockFactory.makeLock("NamedPropertyLock");
    }

    /**
     * Returns the name of this entity.
     * @return name for this entity.
     */
    public String getName() {
        return name;
    }

    /**
     * Sets the name for this entity.
     * @param name name for this entity.
     */
    public void setName(String name) {
        this.name = name;
    }

    /**
     * Sets a property (name/value pair) to this entity. 
     * This is useful for attributes like strength,
     * intelligence, state information.  The key parameter must be a string and the value parameter must
     * be serializable.  When saving the entity to the database or
     * serializing the object over the network, all of its properties 
     * will also be serialized. 
     * <p>
     * If you have an
     * accessor method to a property like getDisplayContext() and
     * setDisplayContext() make sure to set it up as transient with the
     * Introspector's BeanInfo object.  See MVObject static initialization block
     * for an example.
     * 
     * @return old value for given key or null if none ever set.
     */
    public Serializable setProperty(String key, Serializable value) {
        lock.lock();
        try {
            return propertyMap.put(key, value);
        } finally {
            lock.unlock();
        }
    }

    /**
     * Returns the property named by key.
     * @param key serializable key object.
     * @return value of the property, null if property does not exist.
     * @see #setProperty(String, Serializable)
     */
    public Serializable getProperty(String key) {
        lock.lock();
        try {
            return propertyMap.get(key);
        } finally {
            lock.unlock();
        }
    }

    public String getStringProperty(String key) {
        return (String) getProperty(key);
    }

    /**
     * if null, returns false
     */
    public boolean getBooleanProperty(String key) {
        Boolean val = (Boolean) getProperty(key);
        if (val == null) {
            return false;
        }
        return val;
    }

    public Integer getIntProperty(String key) {
        return (Integer) getProperty(key);
    }

    /**
     * Helper method for entity properties.  Atomically modified an integer
     * value property by delta.  newval = oldval + delta.  delta can be negative.
     * @param key key for property
     * @param delta amount changed, can be negative.
     * @return new value.
     */
    public Integer modifyIntProperty(String key, int delta) {
        lock.lock();
        try {
            Integer val = (Integer) propertyMap.get(key);
            val = new Integer(delta + val.intValue());
            return (Integer) propertyMap.put(key, val);
        } finally {
            lock.unlock();
        }
    }

    /**
     * For java beans xml serialization, not for general consumption.
     */
    public Map<String, Serializable> getPropertyMap() {
        lock.lock();
        try {
            HashMap<String, Serializable> newMap = new HashMap<String, Serializable>(propertyMap);
            return newMap;
        } finally {
            lock.unlock();
        }
    }

    /**
     * For java beans xml serialization, not for general consumption.
     */
    public void setPropertyMap(Map<String, Serializable> propMap) {
        lock.lock();
        try {
            if (this.propertyMap == null) {
                throw new RuntimeException("NamedPropertyClass prop map is null: "
                        + this.getName());
            }
            this.propertyMap = new HashMap<String, Serializable>(propMap);
        } finally {
            lock.unlock();
        }
    }

    public Map<String, Serializable> getPropertyMapRef() {
        return propertyMap;
    }

    public void lock()
    {
        lock.lock();
    }

    public void unlock()
    {
        lock.unlock();
    }

    /**
     * Lock to protect the property map
     */
    transient protected Lock lock = null;
    
    protected String name = null;

    private Map<String, Serializable> propertyMap = new HashMap<String, Serializable>();
    
    private static final long serialVersionUID = 1L;
}

