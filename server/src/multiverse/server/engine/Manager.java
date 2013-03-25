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

package multiverse.server.engine;

import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.Serializable;
import java.util.*;

import multiverse.server.util.*;

import java.util.concurrent.locks.*;

public class Manager<E> implements Serializable {
    public Manager() {
        setupTransient();
    }
    
    public Manager(String name) {
        this.name = name;
        setupTransient();
    }

    // called from constructor and readObject
    private void setupTransient() {
        lock = LockFactory.makeLock("ManagerLock:" + name);
    }
    
    /**
     * private method to recreate the lock when deserializing
     */
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }
    
    /**
     * Not for public consumption - for java beans xml compatiblity
     * @return The value map
     */
    public Map<String, E> getMap() {
        return map;
    }

    /**
     * Not for public consumption - for java beans xml compatiblity
     * @param map The value map.
     */
    public void setMap(Map<String, E> map) {
        this.map = map;
    }
    
    public boolean set(String name, E e) {
        return register(name, e);
    }
    
    public boolean register(String name, E e) {
        lock.lock();
        try {
            if (map.put(name, e) != null) {
		Log.warn("Manager: obj with same name already in manager: " + name);
	    }
	    return true;
        }
        finally {
            lock.unlock();
        }
    }

    public E get(String name) {
        lock.lock();
        try {
            return map.get(name);
        }
        finally {
            lock.unlock();
        }
    }

    public Collection<String> keySet() {
        lock.lock();
        try {
            return new HashSet<String>(map.keySet());
        }
        finally {
            lock.unlock();
        }
    }

    public List<String> keyList() {
        lock.lock();
        try {
            return new ArrayList<String>(map.keySet());
        }
        finally {
            lock.unlock();
        }
    }
    
    private Map<String, E> map = new HashMap<String, E>();
    private String name = null;
    transient private Lock lock = null;
    private static final long serialVersionUID = 1L;
}
