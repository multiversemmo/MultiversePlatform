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

import java.util.concurrent.locks.*;
import java.util.*;

/**
 * a map of oids to a lock.  useful for locking an object when you dont have an entity.
 * for example, in the proxyplugin, we want to make sure there are no other threads
 * processing a request for a given oid, to avoid out of order processing.
 * you must make sure you have only one objectlockmanager.
 * 
 * @author cedeno
 *
 */
public class ObjectLockManager {
    public ObjectLockManager() {
    }
    
    public Lock getLock(Long oid) {
        lock.lock();
        try {
            Lock objLock = lockMap.get(oid);
            if (objLock == null) {
                // make a lock
                objLock = LockFactory.makeLock("ObjectLockManager.ObjLock:" + oid);
                lockMap.put(oid, objLock);
            }
            return objLock;
        }
        finally {
            lock.unlock();
        }
    }
    
    private Lock lock = LockFactory.makeLock("ObjectLockManager");
    private Map<Long, Lock> lockMap = new HashMap<Long, Lock>();

}
