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

import multiverse.server.util.*;
import java.util.concurrent.locks.*;

/**
 * handles giving out OIDs
 * it gets unique OIDs from the database in chunks
 * and gives them out
 */
public class OIDManager {
    public OIDManager() {
    }

    public OIDManager(Database db) {
	if (db == null) {
	    throw new RuntimeException("OIDManager: db is null");
	}
	this.db = db;
    }

    /**
     * returns the next free oid
     */
    public long getNextOid() {
	lock.lock();
	try {
	    if (empty()) {
		getNewChunk(defaultChunkSize);
	    }
	    if (empty()) {
		throw new RuntimeException("OIDManager.getNextOid: failed");
	    }
	    return freeOid++;
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * returns if there are no more OIDs and we need to grab more
     */
    public boolean empty() {
	lock.lock();
	try {
	    return (freeOid > lastOid);
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * grabs a new chunk of Oids
     * sets freeOid and lastOid appropriately
     */
    protected void getNewChunk(int chunkSize) {
	lock.lock();
	try {
	    if (db == null) {
// 		Log.debug("OIDManager.getNewChunk: db is null");
                freeOid = 1;
                lastOid = 1000000000;
                return;
	    }
	    Database.OidChunk oidChunk = db.getOidChunk(chunkSize);

	    // good to set the freeOid in case we got non-sequential chunks
	    freeOid = oidChunk.begin;
	    lastOid = oidChunk.end;
            if (Log.loggingDebug)
                Log.debug("OIDManager.getNewChunk: begin=" + oidChunk.begin +
		          ", end=" + oidChunk.end);
	}
	catch(Exception e) {
	    throw new RuntimeException("OIDManager.getNewChunk", e);
	}
	finally {
	    lock.unlock();
	}
    }

    // the last free oid we can give out
    private long lastOid = -2;

    // the next oid to give out
    private long freeOid = 1;

    // this is guaranteed to be an invalid oid
    public final static long invalidOid = -1;

    transient private Lock lock = LockFactory.makeLock("OIDManager");
    private Database db = null;
    public int defaultChunkSize = 100;
}
