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

package multiverse.server.util;

import java.util.Collection;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.Map;
import java.util.concurrent.locks.Lock;

/**
 * Basically a thread safe Map of Map
 * @author cedeno
 *
 */
public class Table<A, B, C> {
    public Table() {    
    }
    
    public boolean isEmpty() {
        lock.lock();
        try {
        for (Map<B,C> subMap : map.values()) {
            if (! subMap.isEmpty()) {
                return false;
            }
        }
        return true;
        }
        finally {
            lock.unlock();
        }
    }
    
    public Collection<A> getKeys() {
        lock.lock();
        try {
            Collection<A> l = new LinkedList<A>(map.keySet());
            return l;
        }
        finally {
            lock.unlock();
        }
    }
    
    public void put(A a, B b, C c) {
        lock.lock();
        try {
            Map<B,C> subMap = map.get(a);
            if (subMap == null) {
                subMap = new HashMap<B,C>();
                map.put(a, subMap);
            }
            subMap.put(b, c);
        }
        finally {
            lock.unlock();
        }
    }
    
//    public void put(A a, B b, Collection<C> clist) {
//        lock.lock();
//        try {
//            for (C c : clist) {
//                put(a, b, c);
//            }
//        }
//        finally {
//            lock.unlock();
//        }
//    }
    
    public C get(A a, B b) {
        lock.lock();
        try {
            Map<B,C> subMap = map.get(a);
            if (subMap == null) {
                return null;
            }
            return subMap.get(b);
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * creates the submap entry if it doesnt exist already
     * and returns it
     */
    public C getWithAddSubMap(A a, B b) {
        lock.lock();
        try {
            Map<B,C> subMap = map.get(a);
            if (subMap == null) {
                subMap = new HashMap<B,C>();
                map.put(a, subMap);
            }
            return subMap.get(b);
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * returns a copy of the submap
     */
    public Map<B,C> getSubMap(A a) {
        lock.lock();
        try {
            Map<B,C> subMap = map.get(a);
            if (subMap == null) {
                subMap = new HashMap<B,C>();
                map.put(a, subMap);
                return subMap;
            }
            return new HashMap<B,C>(subMap);
        }
        finally {
            lock.unlock();
        }
    }
    
    Lock lock = LockFactory.makeLock("TableLock");
    Map<A, Map<B, C>> map = new HashMap<A, Map<B, C>>();
    
}
