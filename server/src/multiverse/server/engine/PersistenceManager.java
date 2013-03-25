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
import multiverse.server.engine.EnginePlugin.SaveHook;
import multiverse.server.objects.*;
import multiverse.server.plugins.*;

import java.util.*;
import java.util.concurrent.locks.*;

/**
 * For use by EnginePlugins, handles periodically saving objects using 
 * the object persistence plugin.  You must call start() for this
 * to begin its save loop, this is usually done via the EnginePlugin
 * startPersistenceManager() method.
 * You add dirty entities to the PersistenceManager, and it will
 * save it to the database when it goes through its list.
 * You can add dirty entities more than once, it will ignore 
 * subsequent calls.
 * <p>
 * The PersistenceManager gets the database object from Engine.getDatabase()
 *
 * @see EnginePlugin
 */
public class PersistenceManager extends Thread {

    public PersistenceManager() {
        super("PersistenceManager");
    }

    /** Register a save hook.
     * @param saveHook the hook to be used when your sub object is marked dirty and
     * needs to be saved.  it should handle deep copy if needed.  this will be called
     * by the persistenceManager when it wants to save the object.
     * if saveHook is null, it will use the default handler which does not do a deep copy
     */
     public void registerSaveHook(Namespace namespace, SaveHook saveHook) {
         saveHookMap.put(namespace, saveHook);
     }
     Map<Namespace, SaveHook> saveHookMap = Collections.synchronizedMap(new HashMap<Namespace, SaveHook>());

    /**
     * Add entity to the dirty list.  The entity will be saved during
     * the next "save object" cycle.  setDirty() will start the persistence
     * manager on first use.
     */
    public void setDirty(Entity entity) {
        lock.lock();
        try {
            if (!started) {
                log.debug("setDirty: manager not started, starting..");
                start();
            }
            if (Log.loggingDebug)
                log.debug("setDirty: setting dirty, entity=" + entity +
                        " PersistenceFlag="+entity.getPersistenceFlag());
            if (entity.getPersistenceFlag()) {
                dirtySet.add(entity);
            }
        } finally {
            lock.unlock();
        }
    }

    /** Remove entity from the dirty list.
    */
    public void clearDirty(Entity entity) {
        lock.lock();
        try {
            if (Log.loggingDebug)
                log.debug("clearDirty: clearing dirty, entity=" + entity);
            dirtySet.remove(entity);
        } finally {
            lock.unlock();
        }
    }

    /** True if the entity is on the dirty list.
    */
    public boolean isDirty(Entity entity) {
        lock.lock();
        try {
            return dirtySet.contains(entity);
        } finally {
            lock.unlock();
        }
    }

    public void start() {
        lock.lock();
        try {
            if (!started) {
            	log.debug("starting");
                started = true;
                super.start();
            }
        } finally {
            lock.unlock();
        }
    }

    // how many MS in between saves
    int intervalMS = 10000;

    public void run() {
        while (true) {
            try {
                persistEntities();
                Thread.sleep(intervalMS);
            }
            catch (InterruptedException e) {
                Log.exception("PersistenceManager", e);
                throw new RuntimeException("PersistenceManager", e);
            }
            catch (Exception e) {
                Log.exception("PersistenceManager", e);
            }
        }
    }

    /**
     * Goes through all the dirty entities and saves them
     */
    void persistEntities() {
        // copy the dirtyset so that we dont hold the lock while
        // processing, and allow others to add dirty items
        // while we process
        Set<Entity> dirtyCopy;
        lock.lock();
        try {
            log.debug("persistEntities: persisting " + dirtySet.size()
                    + " entities");
            dirtyCopy = new HashSet<Entity>(dirtySet);
            dirtySet.clear();
        } finally {
            lock.unlock();
        }

        for (Entity entity : dirtyCopy) {
            persistEntity(entity);
        }
        log.debug("persistEntities: done persisting");
    }

    public void persistEntity(String persistenceKey, Entity e)
    {
        if (e.isDeleted())
            return;

        clearDirty(e);

        callSaveHooks(e);

        if (!ObjectManagerClient.saveObjectData(persistenceKey, e,
                e.getNamespace())) {
            log.error("could not persist object: " + e);
            setDirty(e);
        } else {
            log.debug("persistEntity: saved entity: " + e);
        }
    }
    public void persistEntity(Entity e) {
        persistEntity(null, e);
    }

    public void callSaveHooks(Entity e) {
        Namespace namespace = e.getNamespace();
        if (namespace != null) {
            // call the save hook for this object
            SaveHook cb = saveHookMap.get(namespace);
            if (cb != null) {
                // boolean rv;
                try {
                    cb.onSave(e, namespace);
                } catch (Exception ex) {
                    throw new RuntimeException("onSave", ex);
                }
            }
        }
    }

    Lock lock = LockFactory.makeLock("PersistenceManagerLock");
    Set<Entity> dirtySet = new HashSet<Entity>();

    static final Logger log = new Logger("PersistenceManager");
    
    // flag on whether the thread has started.  this way to can check on double start.
    boolean started = false;
}

