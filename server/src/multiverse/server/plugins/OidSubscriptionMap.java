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

package multiverse.server.plugins;

import java.util.*;
import java.util.concurrent.locks.*;

import multiverse.server.util.*;


/**
 * keeps track of subscription this world server makes for
 * mobs.  
 */
public class OidSubscriptionMap {
    public OidSubscriptionMap() {
    }

    public void put(Long oid, Long sub) {
	lock.lock();
	try {
	    oidToSubMap.put(oid, sub);
	    subToOidMap.put(sub, oid);
	}
	finally {
	    lock.unlock();
	}
    }

    public Long getSub(Long oid) {
	lock.lock();
	try {
	    return oidToSubMap.get(oid);
	}
	finally {
	    lock.unlock();
	}
    }

    public Long getOid(Long sub) {
	lock.lock();
	try {
	    return subToOidMap.get(sub);
	}
	finally {
	    lock.unlock();
	}
    }

    public Long removeSub(Long oid) {
	lock.lock();
	try {
	    Long sub = oidToSubMap.remove(oid);
	    if (subToOidMap.remove(sub) == null) {
		throw new RuntimeException("remove failed: sub="+sub);
	    }
	    return sub;
	}
	finally {
	    lock.unlock();
	}
    }
    public Long removeOid(Long sub) {
	lock.lock();
	try {
	    Long oid = subToOidMap.remove(sub);
	    if (oidToSubMap.remove(oid) == null) {
		throw new RuntimeException("remove failed");
	    }
	    return oid;
	}
	finally {
	    lock.unlock();
	}
    }

    public Lock getLock() {
	return lock;
    }

    Map<Long, Long> oidToSubMap = new HashMap<Long, Long>();
    Map<Long, Long> subToOidMap = new HashMap<Long,Long>();
    Lock lock = LockFactory.makeLock("OidSubscriptionLock");
}
