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

package multiverse.simpleclient;

import java.util.concurrent.locks.*;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.events.*;

import java.util.*;

public class NewObjectHandler implements EventHandler {
    public NewObjectHandler(SimpleClient sc) {
        this.sc = sc;
    }
    
    protected SimpleClient sc = null;

    public String getName() {
	return "simpleclient.NewObjectHandler";
    }

    public boolean handleEvent(Event event) {
        // dont call super on this one since we set the obj map

        NewObjectEvent newObjEvent = (NewObjectEvent) event;
	long objOid = event.getObjectOid();
        sc.userLock.lock();
        try {
            log.info("event=" + event.getName() +
                     ", notifyObjOid=" + objOid +
                     ", objOid=" + newObjEvent.objOid +
                     ", name=" + newObjEvent.objName +
                     ", loc=" + newObjEvent.objLoc +
                     ", orientation=" + newObjEvent.objOrient +
                     ", scale=" + newObjEvent.objScale +
                     ", objType=" + NewObjectEvent.objectTypeToName(newObjEvent.objType) +
                     ", followTerrain=" + newObjEvent.objFollowsTerrain);
            ObjectMap.addObject(newObjEvent.objOid, newObjEvent.objName);
            
            // also set up our loc
            if (newObjEvent.objOid == sc.charOid) {
                Log.info("NewObjectHandler: got new object message for myself");
                sc.loc = newObjEvent.objLoc;
            }
            sc.lastUpdated = System.currentTimeMillis();
        }
        finally {
            sc.userLock.unlock();
        }
	return true;
    }

    static final Logger log = new Logger("NewObjectHandler");

    public static class ObjectMap {
        public static void addObject(Long oid, String name) {
            lock.lock();
            try {
                map.put(oid, name);
            }
            finally {
                lock.unlock();
            }
        }

        public static String getObject(Long oid) {
            lock.lock();
            try {
                return map.get(oid);
            }
            finally {
                lock.unlock();
            }
        }

        private static Map<Long, String> map = new HashMap<Long,String>();

        private static Lock lock = LockFactory.makeLock("objmaplock");
    }
}
