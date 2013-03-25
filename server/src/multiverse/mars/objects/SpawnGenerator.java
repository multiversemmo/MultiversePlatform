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

import multiverse.msgsys.*;

import multiverse.server.objects.*;
import multiverse.server.engine.*;
import multiverse.server.math.*;
import multiverse.server.util.*;
import multiverse.server.messages.PropertyMessage;
import multiverse.server.plugins.ObjectManagerClient;

import java.util.*;
import java.util.concurrent.*;

// spawn generators are objects that can be placed - these special objects
// spawn mobs and also keep track of when the mob dies so it can respawn them

public class SpawnGenerator
    implements MessageCallback, MessageDispatch, Runnable
{
    public SpawnGenerator() {
    }

    public SpawnGenerator(String name) {
        setName(name);
    }

    public SpawnGenerator(SpawnData data) {
        initialize(data);
    }

    public void initialize(SpawnData data) {
        setSpawnData(data);
	setName(data.getName());
        setInstanceOid(data.getInstanceOid());
	setLoc(data.getLoc());
	setOrientation(data.getOrientation());
	setSpawnRadius(data.getSpawnRadius());
	setNumSpawns(data.getNumSpawns());
	setRespawnTime(data.getRespawnTime());
        if (data.getCorpseDespawnTime() != null)
	    setCorpseDespawnTime(data.getCorpseDespawnTime());
	String value = (String)data.getProperty("corpseDespawnTime");
	if (value != null) {
	    setCorpseDespawnTime(Integer.valueOf(value));
	}
    }

    public void activate() {
        try {
            spawns = new ArrayList<ObjectStub>(numSpawns);
            for (int i = 0; i < numSpawns; i++) {
                spawnObject();
            }
        }
        catch (Exception e) {
            throw new MVRuntimeException("activate failed", e);
        }
    }

    public void deactivate()
    {
        if (spawns == null)
            return;

        List<ObjectStub> cleanupSpawns = spawns;
        spawns = null;
        
        for (ObjectStub obj : cleanupSpawns) {
            try {
                obj.unload();
                removeDeathWatch(obj.getOid());
            }
            catch (Exception e) {
                Log.exception("SpawnGenerator.deactivate()", e);
            }
        }
    }

    public void dispatchMessage(Message message, int flags,
        MessageCallback callback)
    {
        Engine.defaultDispatchMessage(message, flags, callback);
    }

    public void handleMessage(Message msg, int flags) {
        if (msg instanceof PropertyMessage) {
            PropertyMessage propMsg = (PropertyMessage) msg;
            Long oid = propMsg.getSubject();
            Boolean dead = (Boolean)propMsg.getProperty(CombatInfo.COMBAT_PROP_DEADSTATE);
            if (dead != null && dead) {
                removeDeathWatch(oid);
                ObjectStub obj = (ObjectStub) EntityManager.getEntityByNamespace(oid, Namespace.MOB);
                if ((obj != null) && (corpseDespawnTime != -1)) {
                    Engine.getExecutor().schedule(new CorpseDespawner(obj), corpseDespawnTime,
                            TimeUnit.MILLISECONDS);
                }
		Engine.getExecutor().schedule(this, respawnTime, TimeUnit.MILLISECONDS);
	    }
        }
    }

    protected void spawnObject() {
        if (spawns == null)
            return;
        Point loc = Points.findNearby(getLoc(), spawnRadius);
        ObjectStub obj = null;
        obj = factory.makeObject(spawnData, instanceOid, loc);
        if (obj == null) {
            Log.error("SpawnGenerator: Factory.makeObject failed, returned null, factory="+factory);
            return;
        }
        if (Log.loggingDebug)
            Log.debug("SpawnGenerator.spawnObject: name=" + getName() +
                ", created object " + obj + " at loc=" + loc);
        addDeathWatch(obj.getOid());
        obj.spawn();
        spawns.add(obj);
        if (Log.loggingDebug)
            Log.debug("SpawnGenerator.spawnObject: name=" + getName() +
                ", spawned obj " + obj);
    }

    protected void spawnObject(int millis) {
        if (spawns == null)
            return;
        Log.debug("SpawnGenerator: adding spawn timer");
        Engine.getExecutor().schedule(this, millis, TimeUnit.MILLISECONDS);
    }

    // Called by scheduled executor
    public void run() {
        try {
            spawnObject();
        }
        catch (MVRuntimeException e) {
            Log.exception("SpawnGenerator.run caught exception: ", e);
        }
    }

    protected void addDeathWatch(Long oid) {
        if (Log.loggingDebug)
            Log.debug("SpawnGenerator.addDeathWatch: oid=" + oid);
        SubjectFilter filter = new SubjectFilter(oid);
        filter.addType(PropertyMessage.MSG_TYPE_PROPERTY);
        Long sub = Engine.getAgent().createSubscription(filter, this);
        deathWatchMap.put(oid, sub);
    }
    protected void removeDeathWatch(Long oid) {
        Long sub = deathWatchMap.remove(oid);
        if (sub != null) {
            if (Log.loggingDebug)
                Log.debug("SpawnGenerator.removeDeathWatch: oid=" + oid);
            Engine.getAgent().removeSubscription(sub);
        }
    }

    public long getInstanceOid()
    {
        return instanceOid;
    }

    public void setInstanceOid(long oid)
    {
        if (instanceOid == -1) {
            instanceOid = oid;
            addInstanceContent(this);
        }
        else
            throw new MVRuntimeException("Cannot change SpawnGenerator instanceOid, from="+instanceOid + " to="+oid);
    }

    public void setName(String name) { this.name = name; }
    public String getName() { return name; }

    public void setLoc(Point p) { loc = p; }
    public Point getLoc() { return loc; }

    public void setOrientation(Quaternion o) { orient = o; }
    public Quaternion getOrientation() { return orient; }

    public int getSpawnRadius() { return spawnRadius; }
    public void setSpawnRadius(int radius) { spawnRadius = radius; }

    // how long after death does it take to respawn
    public int getRespawnTime() { return respawnTime; }
    public void setRespawnTime(int milliseconds) { respawnTime = milliseconds; }

    public int getNumSpawns() { return numSpawns; }
    public void setNumSpawns(int num) { numSpawns = num; }

    public int getCorpseDespawnTime() { return corpseDespawnTime; }
    public void setCorpseDespawnTime(int time) { corpseDespawnTime = time; }

    public ObjectFactory getObjectFactory() { return factory; }
    public void setObjectFactory(ObjectFactory factory) { this.factory = factory; }
    

    public SpawnData getSpawnData() { return spawnData; }

    public void setSpawnData(SpawnData spawnData) { this.spawnData = spawnData; }

    protected class CorpseDespawner implements Runnable {
	public CorpseDespawner(ObjectStub obj) {
	    this.obj = obj;
	}
	protected ObjectStub obj;

	public void run() {
            if (spawns == null)
                return;
            spawns.remove(obj);
	    try {
                // ObjectStub.despawn() does a local unload then a
                // WM despawn.  The mob does not have a mob sub-object,
                // so the object manager unload won't affect the mob
                // manager local data structures.
		obj.despawn();
                ObjectManagerClient.unloadObject(obj.getOid());
	    }
	    catch (MVRuntimeException e) {
		Log.exception("SpawnGenerator.CorpseDespawner: exception: ", e);
	    }
	}
    }

    private static void addInstanceContent(SpawnGenerator spawnGen)
    {
        synchronized (instanceContent) {
            List<SpawnGenerator> spawnGenList =
                instanceContent.get(spawnGen.getInstanceOid());
            if (spawnGenList == null) {
                spawnGenList = new LinkedList<SpawnGenerator>();
                instanceContent.put(spawnGen.getInstanceOid(), spawnGenList);
            }
            spawnGenList.add(spawnGen);
        }
    }

    public static void cleanupInstance(long instanceOid)
    {
        List<SpawnGenerator> spawnGenList;
        synchronized (instanceContent) {
            spawnGenList = instanceContent.remove(instanceOid);
        }
        if (spawnGenList != null) {
            for (SpawnGenerator spawnGen : spawnGenList) {
                spawnGen.deactivate();
            }
        }
    }

    protected long instanceOid = -1;
    protected String name = null;
    protected Point loc = null;
    protected Quaternion orient = null;
    protected int spawnRadius = 0;
    protected int respawnTime = 0;
    protected int numSpawns = 3;
    protected int corpseDespawnTime = -1;
    protected SpawnData spawnData = null;
    protected ObjectFactory factory = null;
    protected Map<Long, Long> deathWatchMap = new HashMap<Long, Long>();
    protected List<ObjectStub> spawns;

    private static Map<Long,List<SpawnGenerator>> instanceContent =
        new HashMap<Long,List<SpawnGenerator>>();

    private static final long serialVersionUID = 1L;
}
