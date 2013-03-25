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

import multiverse.msgsys.*;
import multiverse.server.objects.*;
import multiverse.server.engine.*;
import multiverse.server.pathing.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.plugins.MobManagerClient.CreateSpawnGeneratorMessage;

import multiverse.mars.objects.*;

/**
 * handles perceiver updates, interpolation
 */
public class MobManagerPlugin extends multiverse.server.engine.EnginePlugin {
    public MobManagerPlugin() {
	super("MobManager");
        setPluginType("MobManager");
    }
    public void onActivate() {
	try {
	    log.debug("onActivate()");
            // register for msgtype->hooks
            registerHooks();
            registerUnloadHook(Namespace.MOB, new MobUnloadHook());
            registerDeleteHook(Namespace.MOB, new MobDeleteHook());

            MessageTypeFilter filter = new MessageTypeFilter();
            filter.addType(MobManagerClient.MSG_TYPE_CREATE_SPAWN_GEN);
            filter.addType(InstanceClient.MSG_TYPE_INSTANCE_DELETED);
            filter.addType(InstanceClient.MSG_TYPE_INSTANCE_UNLOADED);
            Engine.getAgent().createSubscription(filter,this,
                MessageAgent.RESPONDER);
            ObjectFactory.register("WEObjFactory", new WEObjFactory());
	}
	catch(Exception e) {
	    throw new MVRuntimeException("activate failed", e);
	}
    }

    // how to process incoming messages
    protected void registerHooks()
    {
        getHookManager().addHook(MobManagerClient.MSG_TYPE_CREATE_SPAWN_GEN,
                new CreateSpawnGenHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_INSTANCE_DELETED,
                new InstanceUnloadedHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_INSTANCE_UNLOADED,
                new InstanceUnloadedHook());
    }

    public static ObjectStub createObject(String templateName,
        long instanceOid, Point loc, Quaternion orient)
    {
        return createObject(templateName, instanceOid, loc, orient, true);
    }

    public static ObjectStub createObject(String templateName,
        long instanceOid, Point loc, Quaternion orient, boolean followsTerrain)
    {
        if (Log.loggingDebug)
            log.debug("createObject: template=" + templateName
                      + ", point=" + loc + ", calling into objectmanager to generate");
	Template override = new Template();
	override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_INSTANCE, instanceOid);
	override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_LOC, loc);
        if (orient != null)
            override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_ORIENT, orient);
	override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_FOLLOWS_TERRAIN,
		     new Boolean(followsTerrain));
	return createObject(templateName, override, null);
    }

    public static ObjectStub createObject(String templateName,
        Template override, Long instanceOid)
    {
        if (Log.loggingDebug)
            log.debug("createObject: template=" + templateName
                      + ", override=" + override +
                      ", instanceOid="+instanceOid +
                      " calling into objectmanager to generate");

        if (instanceOid != null) {
            override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_INSTANCE, instanceOid);
        }
        Long objId = ObjectManagerClient.generateObject(templateName, override);

        if (Log.loggingDebug)
            log.debug("generated object oid=" + objId);

        if (objId == null) {
            Log.warn("MobManagerPlugin: oid is null, skipping");
            return null;
        }

        BasicWorldNode bwNode = WorldManagerClient.getWorldNode(objId);
        InterpolatedWorldNode iwNode = new InterpolatedWorldNode(bwNode);
        ObjectStub obj = new ObjectStub(objId, iwNode, templateName);
        EntityManager.registerEntityByNamespace(obj, Namespace.MOB);
        if (Log.loggingDebug)
            log.debug("createObject: obj=" + obj);
        return obj;
    }

    class MobUnloadHook implements UnloadHook
    {
        public void onUnload(Entity entity)
        {
            if (entity instanceof ObjectStub) {
                ((ObjectStub)entity).unload();
            }
        }
    }

    class MobDeleteHook implements DeleteHook
    {
        public void onDelete(Entity entity)
        {
            if (entity instanceof ObjectStub) {
                ((ObjectStub)entity).unload();
            }
        }
        public void onDelete(Long oid, Namespace namespace)
        {
            // Mobs currently not persistent.  If they are, then might
            // need to delete from database here.
        }
    }

    class CreateSpawnGenHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            CreateSpawnGeneratorMessage message =
                (CreateSpawnGeneratorMessage) msg;

            SpawnData spawnData = message.getSpawnData();
            ObjectFactory factory =
                ObjectFactory.getFactory(spawnData.getFactoryName());
            if (factory == null) {
                Engine.getAgent().sendBooleanResponse(message, false);
                if (Log.loggingDebug)
                    Log.debug("CreateSpawnGenHook: unknown factory=" +
                        spawnData.getFactoryName());
                return true;
            }

            SpawnGenerator spawnGen = null;
            String spawnGenClassName = (String) spawnData.getProperty("className");
            if (spawnGenClassName == null)
                spawnGenClassName = spawnData.getClassName();

            if (spawnGenClassName == null ) {
                spawnGen = new SpawnGenerator(spawnData);
            }
            else {
                try {
                    Class spawnGenClass =
                        spawnGeneratorClasses.get(spawnGenClassName);
                    if (spawnGenClass == null) {
                        throw new MVRuntimeException("spawn generator class not registered");
                    }
                    spawnGen = (SpawnGenerator) spawnGenClass.newInstance();
                    spawnGen.initialize(spawnData);
                }
                catch (Exception ex) {
                    Log.exception("CreateSpawnGenHook: failed instantiating class "+spawnGenClassName, ex);
                    Engine.getAgent().sendBooleanResponse(message, false);
                    return true;
                }
            }
            spawnGen.setObjectFactory(factory);
            spawnGen.activate();
            Engine.getAgent().sendBooleanResponse(message, true);
            return true;
        }
    }

    // Used for both instance unloaded and instance deleted
    class InstanceUnloadedHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            SubjectMessage message = (SubjectMessage) msg;
            long instanceOid = message.getSubject();

            SpawnGenerator.cleanupInstance(instanceOid);
            removeTracker(instanceOid);

            Engine.getAgent().sendResponse(new ResponseMessage(message));
            return true;
        }
    }


    public PathInfo getPathInfo() {
        return pathInfo;
    }
    
    public void setPathInfo(PathInfo pathInfo) {
        this.pathInfo = pathInfo;
        PathSearcher.createPathSearcher(pathInfo, World.getGeometry());
    }

    /** Set the object types to track.  By default, all object types are
        tracked.  This should be set before an instance is created
        or loaded.  Changing the setting has no effect on existing
        instances.
    */
    public static void setTrackedObjectTypes(Collection<ObjectType> objectTypes) {
        if (objectTypes != null)
            trackedObjectTypes = new ArrayList<ObjectType>(objectTypes);
        else
            trackedObjectTypes = null;
    }

    /** Get the object types to track.
    */
    public static List<ObjectType> getTrackedObjectTypes() {
        return new ArrayList<ObjectType>(trackedObjectTypes);
    }

    public static ObjectTracker getTracker(long instanceOid)
    {
        synchronized (trackers) {
            ObjectTracker tracker = trackers.get(instanceOid);
            if (tracker == null) {
                if (Log.loggingDebug)
                    log.debug("Creating ObjectTracker for instanceOid="+instanceOid);
                tracker = new ObjectTracker(Namespace.MOB, instanceOid,
                        new ObjectStubFactory(), trackedObjectTypes);
                trackers.put(instanceOid, tracker);
            }
            return tracker;
        }
    }

    public static void removeTracker(long instanceOid)
    {
        synchronized (trackers) {
            trackers.remove(instanceOid);
        }
    }

    /** Register a spawn generator class.
        @param name Spawn generator class registered name.
        @param spawnGenClass Spawn generator class, must be a
            {@link multiverse.mars.objects.SpawnGenerator} sub-class.
    */
    public static void registerSpawnGeneratorClass(String name,
        Class spawnGenClass)
    {
        synchronized (spawnGeneratorClasses) {
            spawnGeneratorClasses.put(name,spawnGenClass);
        }
    }

    /** Get a registered spawn generator class.
    */
    public static Class getSpawnGeneratorClass(String name)
    {
        return spawnGeneratorClasses.get(name);
    }

    private static Map<String,Class> spawnGeneratorClasses =
        new HashMap<String,Class>();

    private static Map<Long,ObjectTracker> trackers =
        new HashMap<Long,ObjectTracker>();

    private static Collection<ObjectType> trackedObjectTypes;

    protected static final Logger log = new Logger("MobManagerPlugin");

//## need per instance path info
    protected PathInfo pathInfo = null;
    protected boolean askedForPathInfo = false;

}
