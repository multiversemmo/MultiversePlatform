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
import java.io.*;
import java.util.concurrent.locks.*;

import multiverse.msgsys.*;
import multiverse.management.Management;
import multiverse.server.objects.*;
import multiverse.server.plugins.ObjectManagerClient.GenerateSubObjectMessage;
import multiverse.server.plugins.WorldManagerClient.NewRegionMessage;
import multiverse.server.plugins.WorldManagerClient.HostInstanceMessage;
import multiverse.server.engine.*;
import multiverse.server.pathing.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.messages.*;

/**
 * The world manager is in change of telling perceivers what they "see" around
 * them, such as mobs, structures, and terrain. In addition to the existence of
 * objects, the server also tells perceivers about how these things look, such
 * as their model and animations. As part of determining what the perceivers see,
 * the world server also handles movement and interpolation.
 */
public abstract class WorldManagerPlugin
        extends multiverse.server.engine.EnginePlugin
        implements MessageCallback, PerceiverCallback<WMWorldNode>,
            PerceptionUpdateTrigger
{

    public WorldManagerPlugin() {
        super();
        setPluginType("WorldManager");
        String wmAgentName;
        try {
            wmAgentName = Engine.getAgent().getDomainClient().allocName("PLUGIN",getPluginType()+"#");
        }
        catch (java.io.IOException e) {
            throw new MVRuntimeException("Could not allocate world manager plugin name",e);
        }

        setName(wmAgentName);

        propertyExclusions.add(MVObject.wnodeKey);
        propertyExclusions.add(MVObject.perceiverKey);
        propertyExclusions.add(MVObject.dcKey);
        propertyExclusions.add(MVObject.stateMapKey);

        PerceptionFilter.addUpdateTrigger(this);
    }

    public void onActivate() {
        try {
            // register for msgtype->hooks
            registerHooks();

	    Integer maxObjects =
		Engine.getIntProperty("multiverse.quad_tree_node_max_objects");
	    Integer maxDepth =
		Engine.getIntProperty("multiverse.quad_tree_max_depth");
            if (maxObjects != null)
                this.maxObjects = maxObjects;
            if (maxDepth != null)
                this.maxDepth = maxDepth;

            Geometry localGeo = World.getLocalGeometry();
            if (localGeo == null) {
                throw new RuntimeException("null local geometry");
            }

            // register this plugin's namespace
            List<Namespace> namespaces = new LinkedList<Namespace>();
            namespaces.add(Namespace.WORLD_MANAGER);
            namespaces.add(Namespace.WM_INSTANCE);

            WorldManagerFilter selectionFilter = new WorldManagerFilter(getName());
            subObjectFilter = new SubObjectFilter();
            subObjectFilter.setMatchSubjects(true);
            registerPluginNamespaces(namespaces, 
                    new WorldManagerGenerateSubObjectHook(),
                    selectionFilter, subObjectFilter);

            HostInstanceFilter hostInstanceFilter = new HostInstanceFilter(getName());
            hostInstanceFilter.addType(WorldManagerClient.MSG_TYPE_HOST_INSTANCE);
            Engine.getAgent().createSubscription(
                hostInstanceFilter, this, MessageAgent.RESPONDER);

            newRegionFilter = new WorldManagerFilter();
            newRegionFilter.setNamespaces(namespaces);
            newRegionFilter.addType(WorldManagerClient.MSG_TYPE_NEW_REGION);
            newRegionFilter.addType(WorldManagerClient.MSG_TYPE_PLAYER_PATH_WM_REQ);
            newRegionSub = Engine.getAgent().createSubscription(newRegionFilter,
                this);

            registerLoadHook(Namespace.WORLD_MANAGER, new MobLoadHook());
            registerUnloadHook(Namespace.WORLD_MANAGER, new MobUnloadHook());
            registerDeleteHook(Namespace.WORLD_MANAGER, new MobDeleteHook());

            // Instance load/unload/delete hooks
            registerLoadHook(WorldManagerClient.INSTANCE_NAMESPACE,
                new InstanceLoadHook());
            registerUnloadHook(WorldManagerClient.INSTANCE_NAMESPACE,
                new InstanceUnloadHook());
            registerDeleteHook(WorldManagerClient.INSTANCE_NAMESPACE,
                new InstanceDeleteHook());

            // Create subscription for non-structures
            LinkedList<MessageType> types = new LinkedList<MessageType>();
            types.add(WorldManagerClient.MSG_TYPE_REFRESH_WNODE); // S
            types.add(WorldManagerClient.MSG_TYPE_MODIFY_DC); // S
            types.add(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT); // S
            types.add(WorldManagerClient.MSG_TYPE_UPDATEWNODE_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_ORIENT_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_MOB_PATH_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_REPARENT_WNODE_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_COM_REQ); // S
            mobFilter = new PerceptionFilter(types);
            mobFilter.setMatchSubjects(true);
            mobSubId = Engine.getAgent().createSubscription(mobFilter,
                this);

            types.clear();
            types.add(WorldManagerClient.MSG_TYPE_OBJINFO_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_DC_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_SPAWN_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_DESPAWN_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_SETWNODE_REQ); // S
            types.add(WorldManagerClient.MSG_TYPE_GETWNODE_REQ); // S
            mobRPCFilter = new PerceptionFilter(types);
            mobRPCFilter.setMatchSubjects(true);
            mobRPCSubId = Engine.getAgent().createSubscription(
                mobRPCFilter, this, MessageAgent.RESPONDER);

            // subscribe to (fixed) Perceiver Region messages
            // this comes from other world servers starting up
            // to tell us their geometry - we in turn set up a
            // fixed perceiver and notify the remote world server about
            // our mobs near them
            Filter percFilter = new MessageTypeSessionIdFilter(
                WorldManagerClient.MSG_TYPE_PERCEIVER_REGIONS,
                Engine.getAgent().getName(), true);
            Long percSub = Engine.getAgent().createSubscription(percFilter, this);
            if (percSub == null) {
                throw new MVRuntimeException("create perceiver sub failed");
            }
            if (Log.loggingDebug) {
                Log.debug("created perceiver regions subscriptions: " + percSub);
            }

            WorldManagerTransferFilter transferFilter = new WorldManagerTransferFilter();
            transferFilter.addGeometry(localGeo);
            Hook transferHook = new WorldManagerTransferHook();
            registerTransferHook(transferFilter, transferHook);
            
            // subscribe to newRemoteObject messages
            // these messages will come from the other world mgr
            MessageTypeSessionIdFilter remoteMobFilter = new MessageTypeSessionIdFilter(Engine.getAgent().getName());
            remoteMobFilter.matchesNullSessionId(false);
            remoteMobFilter.addType(WorldManagerClient.MSG_TYPE_NEW_REMOTE_OBJ);
            remoteMobFilter.addType(WorldManagerClient.MSG_TYPE_FREE_REMOTE_OBJ);
            /* Long remoteMobSub = */ Engine.getAgent().createSubscription(remoteMobFilter, this);

            types.clear();
            types.add(Management.MSG_TYPE_GET_PLUGIN_STATUS);
            Engine.getAgent().createSubscription(
                new MessageTypeFilter(types), this, MessageAgent.RESPONDER);

/* Disabled until we have real zoning
            // send out a PerceiverRegionsMessage
            // FIXME/TODO: currently sending max geometry
            if (Log.loggingDebug)
                log.debug("activate: sending out perceiver regions msg, geom="
                          + Geometry.maxGeometry());
            //## hardcoded for instance oid 0L
            WorldManagerClient.sendPerceiverRegionsMsg(0L,
                Geometry.maxGeometry(), null);
            log.debug("activate: sent out perceiver regions msg");
*/

            Engine.registerStatusReportingPlugin(this);

            // start an update thread which periodically updates all
            // subscribers on the mobs current wnode.
            // extend this method to override how this works
            startUpdater();
        } catch (Exception e) {
            Log.exception("WorldManagerPlugin.onActivate failed", e);
            throw new MVRuntimeException("activate failed", e);
        }
    }

    public Map<String, String> getStatusMap()
    {
        Map<String,String> status =
            new HashMap<String,String>();
        status.put("instances", Integer.toString(quadtrees.size()));
        status.put("entities", Integer.toString(EntityManager.getEntityCount()));
        return status;
    }

    public Entity getWorldManagerEntity(Long oid) {
        return EntityManager.getEntityByNamespace(oid, Namespace.WORLD_MANAGER);
    }
    
    public Entity getWorldManagerEntityOrError(Long oid) {
        Entity entity = getWorldManagerEntity(oid);
        if (entity == null)
            throw new MVRuntimeException("Could not find wm entity for oid " + oid);
        return entity;
    }

    public void registerWorldManagerEntity(Entity entity) {
        EntityManager.registerEntityByNamespace(entity, Namespace.WORLD_MANAGER);
    }
    
    public boolean removeWorldManagerEntity(Long oid) {
        return EntityManager.removeEntityByNamespace(oid, Namespace.WORLD_MANAGER);
    }
    
    // called on activate
    // start an update thread which periodically updates all
    // subscribers on the mobs current wnode.
    // extend this method to override how this works
    protected void startUpdater() {
        updater = new Updater();
        Thread updateThread = new Thread(updater, "WMUpdater");
        updateThread.start();
    }

    public void sendRegionUpdate(MVObject obj)
    {
        WMWorldNode wnode = (WMWorldNode) obj.worldNode();
        QuadTreeNode<WMWorldNode> quadNode = wnode.getQuadNode();
        if (quadNode != null) {
            Point loc = wnode.getCurrentLoc();
            List<Region> regionList = quadNode.getRegionByLoc(loc);
            if (regionList != null) {
                synchronized (updater) {
                    updater.updateRegion(obj, regionList);
                }
            }
        }
    }

    class Updater implements Runnable {
        public void run() {
            while (true) {
                try {
                    update();
                } catch (MVRuntimeException e) {
                    Log.exception("WorldManagerPluging.Updater.run caught MVRuntimeException", e);
                } catch (Exception e) {
                    Log.exception("WorldManagerPluging.Updater.run caught exception", e);
                }

                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    Log.warn("Updater: " + e);
                    e.printStackTrace();
                }
            }
        }

        protected void update() {
            log.debug("Update.update: in update");

            // go through all mobs and send an update message
            for (Entity e : EntityManager.getAllEntitiesByNamespace(Namespace.WORLD_MANAGER)) {
                // only send out messages for spawned entities
                if (!(e instanceof MVObject)) {
                    continue;
                }
                MVObject obj = (MVObject) e;
                WMWorldNode wnode = (WMWorldNode) obj.worldNode();
                if (wnode == null) {
                    continue;
                }
                if (!wnode.isSpawned()) {
                    continue;
                }
                if (!wnode.isLocal()) {
                    continue;
                }

                Point loc = null;
		if (obj.isMob() || obj.isUser()) {
                    // first check to see if we want to save this object
                    if (obj.getPersistenceFlag()) {
                        Long lastSaved = (Long) obj.getProperty(WorldManagerClient.WMGR_LAST_SAVED_PROP);
                        if (lastSaved == null) {
                            lastSaved = 0L;
                        }
                        Long currentTime = System.currentTimeMillis();
                        Long elapsed = currentTime - lastSaved;
                        if (elapsed > WorldManagerClient.WMGR_SAVE_INTERVAL_MS) {
                            if (Log.loggingDebug) {
                                Log.debug("update: elapsedTime=" + elapsed
                                        + ", marking obj dirty: " + obj);
                                obj.setProperty(WorldManagerClient.WMGR_LAST_SAVED_PROP, currentTime);
                                Engine.getPersistenceManager().setDirty(obj);
                            }
                        }
                    }
                    
                    // zoning check:
		    // Only mobs and users are checked for zoning.
		    // Getting the location causes interpolators to run.
		    loc = wnode.getLoc();
		    if (zoneObject(obj, wnode, loc))
			continue;
		}

                if (obj.isMob()) {
                    // object is spawned, send an update, unless the
                    // node has a path, in which case we don't send
                    // the client the update because it's
                    // interpolating itself to discover the correct
                    // location of the mob
                    if (wnode.getPathInterpolator() != null) {
                        if (Log.loggingDebug)
                            log.debug("Update.update: sending out wnode update for oid " + obj.getOid());
                        sendWNodeMessage(obj.getOid(), obj);
                    }
                }

                // update regions if its a user
                if (obj.isUser()) {
                    if (Log.loggingDebug)
                        log.debug("Update.update: updating regions for oid " + obj.getOid());
                    // first get the ambient sound that should
                    // be playing for this location
		    QuadTreeNode<WMWorldNode> quadNode = wnode.getQuadNode();
		    if (quadNode != null) {
			List<Region> regionList = quadNode.getRegionByLoc(loc);
			if (regionList != null) {
                            synchronized (this) {
                                updateRegion(obj, regionList);
                            }
                        }
		    }
                }
            }
        }

	private boolean zoneObject(MVObject obj, WMWorldNode wnode, Point loc)
	{
	    QuadTreeNode<WMWorldNode> quadNode = wnode.getQuadNode();

	    // If the object isn't in the quad tree, we can't zone it
	    if (quadNode == null)
		return false;

            QuadTree<WMWorldNode> quadtree =
                quadtrees.get(wnode.getInstanceOid());
            if (quadtree == null) {
                log.error("zoneObject: unknown instanceOid=" +
                    wnode.getInstanceOid() + " oid="+obj.getOid());
                return false;
            }

	    // due to sloppy race condition, make sure the loc
	    // we now get is actually in the quadNode
	    if ((quadNode.getNodeType() != QuadTreeNode.NodeType.LOCAL) &&
		(quadNode.containsPointWithHysteresis(loc))) {
		// world node has moved into a non-local node
		if (Log.loggingDebug)
		    log.debug("Update.update: obj moved into non-local node, obj="
			      + obj + ", wnode=" + wnode);

		long oid = obj.getOid();

		// remove node from quad tree
		if (! quadtree.removeElement(wnode)) {
		    throw new MVRuntimeException("Update.update: failed to remove element from quadtree: oid=" + oid);
		}

		// remove object from entity map
		if (!removeWorldManagerEntity(oid)) {
		    Log.warn("Update.update: could not remove entity " + oid);
		}
		else {
		    if (Log.loggingDebug) {
			log.debug("Update.update: removed entity from map " + oid);
		    }
		}
	      
		try {
		    Thread.sleep(2);
		} catch (InterruptedException e1) {
		    // TODO Auto-generated catch block
		    e1.printStackTrace();
		}
	     
		
		// transfer this object to a new world manager plugin
		HashMap<String, Serializable> propMap = new HashMap<String, Serializable>();
		propMap.put(WorldManagerClient.MSG_PROP_LOC, loc);
		if (! transferObject(propMap, obj)) {
		    log.error("Update.update: transfer failed for obj " + obj);
		}
		
		// // remove perceiver (right now serializtion problem)
		// obj.setPerceiver(null);
		// wnode.setPerceiver(null);

//                    // send out a bind request object for the object
//                    Long rv = WorldManagerClient.bind(obj.toBytes());
//                    if ((rv == null) || !(rv.equals(oid))) {
//                        throw new RuntimeException("bind failed, rv=" + rv
//						   + ", oid=" + oid);
//                    }
//
//                    // send out a spawn req
//                    if (!WorldManagerClient.spawn(oid)) {
//                        throw new RuntimeException("spawn failed oid=" + oid);
//                    }
		if (Log.loggingDebug)
		    log.debug("Update.update: done zoning oid " + oid);

		return true;
	    }
	    return false;
	}

        // returns a region config with the highest pri (lower pri #)
        // can return null if none with matching configType found
        RegionConfig getPriorityRegionConfig(List<Region> regionList, String configType) {
            Region highestPriRegion = null;
            for (Region region : regionList) {
                // does this region have a matching region config
                RegionConfig config = region.getConfig(configType);
                if (config == null) {
                    // no matching config
                    continue;
                }
                
                // there is a matching config, check the priority
                if (highestPriRegion == null) {
                    // no existing priority, so make this the highest pri
                    highestPriRegion = region;
                    continue;
                }
                
                // compare priorities
                if (highestPriRegion.getPriority() > region.getPriority()) {
                    // the new region is higher priority
                    highestPriRegion = region;
                }
            }
            return (highestPriRegion == null) ? null : 
                highestPriRegion.getConfig(configType);
        }
        
        List<RegionConfig> getRegionConfigs(List<Region> regionList,
		String configType) {
	    LinkedList<RegionConfig> configs = null;
            for (Region region : regionList) {
                RegionConfig config = region.getConfig(configType);
                if (config != null)  {
		    if (configs == null)
			configs= new LinkedList<RegionConfig>();
		    configs.add(config);
		}
	    }
	    return configs;
	}

        List<Region> getCustomRegionConfigs(List<Region> regionList)
        {
	    List<Region> result = null;
            for (Region region : regionList) {
                if (region.getProperty("onEnter") != null ||
                        region.getProperty("onLeave") != null) {
		    if (result == null)
			result= new LinkedList<Region>();
		    result.add(region);
		}
	    }
	    return result;
	}

        // pass in the region list because there may be overlapping
        // regions - and we need to check if any of them have the specific
        // type we are interested in (for instance, sound) - so that
        // we know when we 'turn off' a regionconfig
        void updateRegion(MVObject obj, List<Region> regionList) {
            // FIXME: dont go through the regions list 'n' times here.
            // find a sound region
            List<RegionConfig> sConfig = getRegionConfigs(regionList, SoundRegionConfig.RegionType);
            updateSoundRegion(obj, sConfig);

            // find a fog region
            FogRegionConfig fConfig = (FogRegionConfig) getPriorityRegionConfig(regionList, FogRegionConfig.RegionType);
            updateFogRegion(obj, fConfig);

            // find a road region
            RoadRegionConfig rConfig = (RoadRegionConfig) getPriorityRegionConfig(regionList, RoadRegionConfig.RegionType);
            updateRoadRegion(obj, rConfig);
            
            // find a dir light region
            RegionConfig dirLightConfig = getPriorityRegionConfig(regionList, LightData.DirLightRegionType);
            updateDirLightRegion(obj, dirLightConfig);
            
            // find ambient light region
            RegionConfig ambientConfig = getPriorityRegionConfig(regionList, LightData.AmbientLightRegionType);
            updateAmbientLightRegion(obj, ambientConfig);

            List<Region> customRegions = getCustomRegionConfigs(regionList);
            updateCustomRegions(obj, customRegions);
        }

        void updateFogRegion(MVObject obj, FogRegionConfig fogConfig) {
            WorldManagerClient.FogMessage fogMsg;
            // get the user's current fog
            FogRegionConfig curFog = (FogRegionConfig) obj.getProperty(FogRegionConfig.RegionType);
            if (fogConfig == null) {
                if (curFog != null) {
                    // there is no more fog
                    obj.setProperty(FogRegionConfig.RegionType, null);
                    Fog fog = getInstanceFog(obj.worldNode().getInstanceOid());
                    fogMsg = new WorldManagerClient.FogMessage(obj.getOid(),
                        fog);
                    Engine.getAgent().sendBroadcast(fogMsg);
                }
            } else {
                // we are in a fog region
                if ((curFog == null) || (!fogConfig.equals(curFog))) {
                    // send the fog region config over
                    if (Log.loggingDebug)
                        log.debug("updateFogRegion: new fog region: oldFog="
                                  + curFog + ", newFog=" + fogConfig);
                    obj.setProperty(FogRegionConfig.RegionType, fogConfig);
                    fogMsg = new WorldManagerClient.FogMessage(obj.getOid(), fogConfig);
                    Engine.getAgent().sendBroadcast(fogMsg);
                }
            }
        }

        void updateCustomRegions(MVObject obj, List<Region> newRegions)
        {
            List<Region> currentRegions =
                (List<Region>) obj.getProperty(REGION_MEMBERSHIP);

            if (currentRegions == null)  {
                if (newRegions == null)
                    return;
                currentRegions = new LinkedList<Region>();
                obj.setProperty(REGION_MEMBERSHIP, (Serializable) currentRegions);
            }

            List<Region> left = new LinkedList<Region>();

            for (ListIterator<Region> iter = currentRegions.listIterator();
                        iter.hasNext(); )  {
                Region currentRegion = iter.next();
                boolean inside = false;
                if (newRegions != null) {
                    for (Region newRegion : newRegions) {
                        if (newRegion == currentRegion)
                            inside = true;
                    }
                }
                if (! inside) {
                    left.add(currentRegion);
                    iter.remove();
                }
            }

            handleLeaveRegion(obj,left);

            List<Region> entered = new LinkedList<Region>();
            if (newRegions != null) {
                for (Region newRegion : newRegions) {
                    boolean existing = false;
                    for (Region currentRegion : currentRegions) {
                        if (currentRegion == newRegion)
                            existing = true;
                    }
                    if (! existing)
                        entered.add(newRegion);
                }
                currentRegions.addAll(entered);
                handleEnterRegion(obj,entered);
            }

        }

        void handleLeaveRegion(MVObject obj, List<Region> left)
        {
            for (Region region : left) {
                String onLeave = (String) region.getProperty("onLeave");
                if (onLeave == null)
                    continue;
                handleRegionChange(obj, region, "leave", onLeave);
            }
        }

        void handleEnterRegion(MVObject obj, List<Region> left)
        {
            for (Region region : left) {
                String onEnter = (String) region.getProperty("onEnter");
                if (onEnter == null)
                    continue;
                handleRegionChange(obj, region, "enter", onEnter);
            }
        }

        void handleRegionChange(MVObject obj, Region region, String action,
            String triggerName)
        {
            if (Log.loggingDebug)
                Log.debug("Custom RegionTrigger: "+obj+
                    " regionName="+region.getName()+
                    " action="+action+" trigger="+triggerName);

            RegionTrigger trigger = regionTriggers.get(triggerName);

            if (trigger == null) {
                Log.error("unknown RegionTrigger name="+triggerName+
                   ", object="+obj+" region="+region+" action="+action);
                return;
            }

            try {
                if (action.equals("enter"))
                    trigger.enter(obj, region);
                else if (action.equals("leave"))
                    trigger.leave(obj, region);
            }
            catch (Exception e) {
                Log.exception("RegionTrigger exception trigger name="+triggerName, e);
            }
        }

        void updateSoundRegion(MVObject obj, List<RegionConfig> soundConfig) {
            // Get the ambient sounds the user is currently playing
            List<SoundData> curSound = (List<SoundData>) obj.getProperty(SoundManager.AMBIENTSOUND);

	    if (curSound == null) {
		if (soundConfig == null)
		    return;
		curSound= new LinkedList<SoundData>();
		obj.setProperty(SoundManager.AMBIENTSOUND, (Serializable)curSound);
	    }

	    // Figure out what should be turned off
	    // Also removes sounds from the user's current ambient sounds
	    List<SoundData> turnOff = new LinkedList<SoundData>();
	    for (ListIterator<SoundData> iter = curSound.listIterator();
			iter.hasNext(); )  {
		SoundData data= iter.next();
		String fileName= data.getFileName();
		boolean on= false;
		if (soundConfig != null) {
		    for (RegionConfig config : soundConfig) {
			SoundRegionConfig sConfig = (SoundRegionConfig)config;
			if (sConfig.containsSound(fileName)) {
			    on= true;
			    break;
			}
		    }
		}
		if (!on) {
		    turnOff.add(data);
		    iter.remove();
		}
	    }

	    // Figure out what should be turned on
	    List<SoundData> turnOn = new LinkedList<SoundData>();
	    if (soundConfig != null) {
		for (RegionConfig config : soundConfig) {
		    SoundRegionConfig sConfig = (SoundRegionConfig)config;
		    for (SoundData data : sConfig.getSoundData()) {
			boolean on= false;
			String fileName= data.getFileName();
			for (SoundData curData : curSound) {
			    if (curData.getFileName().equals(fileName)) {
			       on= true;
			    }
			}
			if (!on)
			    turnOn.add(data);
		    }
		}
	    }

	    // No change
	    if (turnOn.size() == 0 && turnOff.size() == 0) {
		return;
	    }

	    // Build the (ambient) sound message
	    WorldManagerClient.SoundMessage soundMsg;
	    soundMsg = new WorldManagerClient.SoundMessage(obj.getOid());
            soundMsg.setTarget(obj.getOid());
	    soundMsg.setType(WorldManagerClient.SoundMessage.soundTypeAmbient);

	    for (SoundData data : turnOff) {
		soundMsg.removeSound(data.getFileName());
	    }

	    // Also add sounds to the user's current ambient sounds
	    // ## race condition doing the above
	    List<String> turnedOn = new LinkedList<String>();
	    for (SoundData data : turnOn) {
		if (! turnedOn.contains(data.getFileName())) {
		    soundMsg.addSound(data);
		    curSound.add(data);
		}
	    }

	    if (curSound.size() == 0)
		obj.setProperty(SoundManager.AMBIENTSOUND, null);

// 	    if (Log.loggingDebug)
// 		log.debug("updateSoundRegion: sound message " + soundMsg);

	    Engine.getAgent().sendBroadcast(soundMsg);
        }

        void updateRoadRegion(MVObject obj, RoadRegionConfig roadConfig)
        {
            RoadRegionConfig curRoadRegion =
                (RoadRegionConfig) obj.getProperty(RoadRegionConfig.RegionType);

            if (roadConfig == null) {
                if (curRoadRegion != null) {
                    obj.setProperty(RoadRegionConfig.RegionType, null);
                    WorldManagerClient.FreeRoadMessage freeRoadMsg =
                        new WorldManagerClient.FreeRoadMessage(obj.getOid());
                    Engine.getAgent().sendBroadcast(freeRoadMsg);
                }
                return;
            }

            if (curRoadRegion != null) {
                // we've already set the road data, return
                return;
            }

            // send over all road info
            WorldManagerClient.RoadMessage roadMsg =
                new WorldManagerClient.RoadMessage(obj.getOid(),
                roadConfig.getRoads());
            Engine.getAgent().sendBroadcast(roadMsg);
            obj.setProperty(RoadRegionConfig.RegionType, roadConfig);
            log.debug("updateRoadRegion: sent road region");
        }

        /**
         * @param obj
         * @param regionConfig
         */
        void updateDirLightRegion(MVObject obj, RegionConfig regionConfig)
        {
            Long curLightOid = (Long) obj.getProperty(LightData.DirLightRegionType);
            Long masterOid = obj.getMasterOid();
            
            if (regionConfig == null) {
                // there is no directional light in this area
                if (curLightOid != null) {
                    // free the light
                    if (Log.loggingDebug)
                        log.debug("updateDirLightRegion: free light: " + curLightOid);
                    Message freeMsg =
                        new WorldManagerClient.FreeObjectMessage(
                            masterOid, curLightOid);
                    Engine.getAgent().sendBroadcast(freeMsg);
                    
                    // clear player's light
                    obj.setProperty(LightData.DirLightRegionType, null);                    
                }
                return;
            }
            
            Light dirLight = null;
            
            // make sure no one else is making a dir light for this
            // region 
            dirLightLock.lock();
            try {
                // see if there is a spawned light for this regionconfig
                 dirLight = (Light) regionConfig.getProperty("spawnLight");

                if (dirLight == null) {
                    // need to create the light object
                    
                    Quaternion orient = (Quaternion) regionConfig.getProperty("orient");
                    Color specular = (Color) regionConfig.getProperty("specular");
                    Color diffuse = (Color) regionConfig.getProperty("diffuse");
		    String name = (String) regionConfig.getProperty("name");
                    if (Log.loggingDebug)
                        log.debug("updateDirLightRegion: none found, creating spawned light, diffuse="
			          + diffuse + ", specular=" + specular + ", orient=" + orient);

                    // The "_oid" property is set in the NewRegion handler
                    dirLight = new Light();
                    dirLight.setOid((Long)regionConfig.getProperty("_oid"));
                    dirLight.setName("light_" + dirLight.getOid());
                    LightData lightData = new LightData();
                    lightData.setName(name);
                    lightData.setDiffuse(diffuse);
                    lightData.setSpecular(specular);
                    lightData.setAttenuationRange(100000);
                    lightData.setAttenuationConstant(1);
                    lightData.setOrientation(orient);
                    dirLight.setLightData(lightData);
                    regionConfig.setProperty("spawnLight", dirLight);
                    if (Log.loggingDebug)
                        log.debug("updateDirLightRegion: spawned dir light=" + dirLight + ", ld="
			          + dirLight.getLightData());
                }
            } finally {
                dirLightLock.unlock();
            }
            Long regionLightOid = dirLight.getOid();
            if ((curLightOid != null) && (curLightOid.equals(regionLightOid))) {
                return;
            }
            
            if (curLightOid != null) {
                // need to free the existing light first
                if (Log.loggingDebug)
                    log.debug("updateDirLightRegion: need to free existing light: " + curLightOid);
                Message freeMsg =
                    new WorldManagerClient.FreeObjectMessage(
                        masterOid, curLightOid);
                Engine.getAgent().sendBroadcast(freeMsg);
            }

            // send over the new light
            if ((curLightOid == null) || (! curLightOid.equals(regionLightOid))) {
                LightData lightData = dirLight.getLightData();
                if (Log.loggingDebug)
                    log.debug("updateDirLightRegion: sending over new light: " + regionLightOid + ", ld=" + lightData);
                Message newDirLightMsg =
                    new WorldManagerClient.NewDirLightMessage(
                        masterOid, dirLight.getOid(), lightData);
                Engine.getAgent().sendBroadcast(newDirLightMsg);
                
                // update the player's dir light
                if (Log.loggingDebug)
                    log.debug("updateDirLightRegion: setting users new current light to " + regionLightOid);
                obj.setProperty(LightData.DirLightRegionType, regionLightOid);
            }
        }

        void updateAmbientLightRegion(MVObject obj, RegionConfig ambientConfig) {
            // get the user's current ambient light
            Color curAmbient = (Color) obj.getProperty(LightData.AmbientLightRegionType);
    
            // if there is no ambient light in this region
            if (ambientConfig == null) {
                if (curAmbient != null) {
                    // user had an ambient light, but no longer has one.
                    // send a black ambient light
                    log.debug("updateAmbientLightRegion: user had light, but region does not, sending black");
                    obj.setProperty(LightData.AmbientLightRegionType, null);
                    Engine.getAgent().sendBroadcast(new WorldManagerClient.SetAmbientLightMessage(obj.getMasterOid(), new Color(0, 0, 0)));
                    return;
                }
                else {
//                     log.debug("updateAmbientLightRegion: region and user both have no light, returning");
                    // user had no ambient light, and current region still has none
                    return;
                }
            }

            // current region has an ambient light
            Color ambientColor = (Color)ambientConfig.getProperty("color");
            if (ambientColor == null) {
                log.error("updateAmbientLight: ambient color not defined");
                return;
            }
            if (ambientColor.equals(curAmbient)) {
//                 log.debug("updateAmbientLight: colors are the same, returning");
                return;
            }
            
            // color are different, update the player
            log.debug("updateAmbientLightRegion: colors differ, updating client");
            obj.setProperty(LightData.AmbientLightRegionType, ambientColor);
            Engine.getAgent().sendBroadcast(new WorldManagerClient.SetAmbientLightMessage(obj.getMasterOid(), ambientColor));
        }

        Lock dirLightLock = LockFactory.makeLock("DirLightLock");
    }

    Fog getInstanceFog(Long instanceOid)
    {
        WorldManagerInstance instance =
            (WorldManagerInstance) EntityManager.getEntityByNamespace(instanceOid,
            WorldManagerClient.INSTANCE_NAMESPACE);

        return instance.getGlobalFog();
    }

    // how to process incoming messages
    protected void registerHooks() {
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_OBJINFO_REQ,
                new ObjectInfoReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_DC_REQ,
                new DisplayContextReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_SPAWN_REQ,
                new SpawnReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_DESPAWN_REQ,
                new DespawnReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT,
                new UpdateObjectHook());
	getHookManager().addHook(WorldManagerClient.MSG_TYPE_GETWNODE_REQ,
                new GetWNodeReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_COM_REQ,
                new ComReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_ORIENT_REQ,
                new OrientReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_REFRESH_WNODE,
                new RefreshWNodeHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_PERCEIVER_REGIONS,
                new PerceiverRegionsHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_NEW_REMOTE_OBJ,
                new NewRemoteObjHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_FREE_REMOTE_OBJ,
                new FreeRemoteObjHook());
//        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATEWNODE,
//                new UpdateWNodeHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_MOB_PATH_REQ,
                new MobPathReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_NEW_REGION,
                new NewRegionHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_MODIFY_DC,
                new ModifyDisplayContextHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_HOST_INSTANCE,
                new HostInstanceHook());
        getHookManager().addHook(Management.MSG_TYPE_GET_PLUGIN_STATUS,
                new GetPluginStatusHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_PLAYER_PATH_WM_REQ,
                new PlayerPathWMReqHook());
        
    }

    class MobLoadHook implements LoadHook {
        public void onLoad(Entity entity)
        {
            subscribeForMob(entity.getOid());
        }
    }

    class MobUnloadHook implements UnloadHook {
        public void onUnload(Entity entity)
        {
            if (! (entity instanceof MVObject))
                return;
            unsubscribeForMob(entity.getOid());
            despawnObject((MVObject) entity);
        }
    }

    class MobDeleteHook implements DeleteHook {
        public void onDelete(Entity entity)
        {
            if (! (entity instanceof MVObject))
                return;
            unsubscribeForMob(entity.getOid());
            despawnObject((MVObject) entity);
        }
        public void onDelete(Long oid, Namespace namespace)
        {
            // n/a
        }
    }

    protected WorldManagerClient.PerceptionInfo makePerceptionInfo(
        Long oid, MVObject object)
    {
        WorldManagerClient.ObjectInfo objectInfo = makeObjectInfo(oid);

/*
        WorldManagerClient.TargetedPropertyMessage wmProps = new WorldManagerClient.TargetedPropertyMessage(oid,oid);
        for (Object key : object.getPropertyMap().keySet()) {
            if (MVObject.wnodeKey.equals(key) ||
                MVObject.perceiverKey.equals(key) ||
                MVObject.dcKey.equals(key) ||
                MVObject.stateMapKey.equals(key))
                continue;
            wmProps.put((String)key, object.getProperty(key));
        }
        objectInfo.setProperty("wmProps", wmProps);
*/

        WorldManagerClient.PerceptionInfo info =
            new WorldManagerClient.PerceptionInfo(objectInfo);
        ObjectType objectType = object.getType();
        if (!(objectType == ObjectTypes.light) &&
                !(objectType == WorldManagerClient.TEMPL_OBJECT_TYPE_TERRAIN_DECAL) &&
                !(objectType == WorldManagerClient.TEMPL_OBJECT_TYPE_POINT_SOUND)) {
            info.displayContext = getDisplayContext(oid);
        }
        return info;
    }

    protected void sendWMMessage(Message msg) {
        Engine.getAgent().sendBroadcast(msg);
    }
    
    protected Long getPerceiverOid(MobilePerceiver<WMWorldNode> mobileP) {
        WMWorldNode pWnode = mobileP.getElement();
        MVObject pObj = pWnode.getObject();
        return pObj.getOid();
    }
    
    /**
     * We know about a new object via some fixed perceiver. this fixed perceiver
     * is for some remote world manager. We take the new node, and make a
     * newShadowObject message and send it to the remote world manager.
     */
    public void newObjectForFixedPerceiver(Perceiver<WMWorldNode> p, WMWorldNode newWnode) {
        FixedPerceiver<WMWorldNode> fixedP = (FixedPerceiver<WMWorldNode>) p;
        Engine.getAgent().sendBroadcast(
            makeNewObjectForFixedPerceiverMessage(fixedP, newWnode));
    }
    
    protected WorldManagerClient.NewRemoteObjectMessage makeNewObjectForFixedPerceiverMessage(FixedPerceiver<WMWorldNode> fixedP, WMWorldNode newWnode) {
        // find out which world manager this fixed perceiver is associated with
        String remoteSessionId = fixedPerceiverMap.getSessionId(fixedP);
        if (remoteSessionId == null) {
            throw new RuntimeException("unknown remoteSessionId "
                    + remoteSessionId);
        }

        MVObject newObj = newWnode.getObject();
        Long newOid = newObj.getOid();

        if (Log.loggingDebug)
            log.debug("newObjectForFixedPerceiver: objOid=" + newOid + ", newWnode=" + newWnode
                      + ", remoteSessionId=" + remoteSessionId);

        // send a new object message to the perceiver
        WorldManagerClient.NewRemoteObjectMessage newObjMsg = new WorldManagerClient.NewRemoteObjectMessage(
                remoteSessionId, newWnode.getInstanceOid(), newOid, newWnode.getLoc(), 
                newWnode.getOrientation(), newWnode.getPerceptionRadius(),
		newObj.getType());
        return newObjMsg;
    }

    public void freeObjectForFixedPerceiver(Perceiver<WMWorldNode> p, WMWorldNode freeWnode) {
        FixedPerceiver<WMWorldNode> fixedP = (FixedPerceiver<WMWorldNode>) p;
        Engine.getAgent().sendBroadcast(
            makeFreeObjectForFixedPerceiverMessage(fixedP, freeWnode));
    }
    
    protected WorldManagerClient.FreeRemoteObjectMessage makeFreeObjectForFixedPerceiverMessage(FixedPerceiver<WMWorldNode> fixedP, WMWorldNode freeWnode) {
        // find out which world manager this fixed perceiver is associated with
        String remoteSessionId = fixedPerceiverMap.getSessionId(fixedP);
        if (remoteSessionId == null) {
            throw new RuntimeException("unknown remoteSessionId "
                    + remoteSessionId);
        }
        MVObject freeObj = freeWnode.getObject();

        Long freeOid = freeObj.getOid();

        if (Log.loggingDebug)
            log.debug("freeFixedObj: objOid=" + freeOid + ", wnode=" + freeWnode
                      + ", remoteSessionId=" + remoteSessionId);

        // send a free remote object msg
        WorldManagerClient.FreeRemoteObjectMessage msg = new WorldManagerClient.FreeRemoteObjectMessage(
                remoteSessionId, freeWnode.getInstanceOid(), freeOid);
        return msg;
    }

    /**
     * This method creates the NewsAndFreesMessage, and sends it to to
     * subscribers.  The return value is the count of news plus count
     * of frees sent to the perceiver p if its oid is notifyOid, or
     * null if not.
     */
    public Integer processNewsAndFrees(Perceiver<WMWorldNode> p,
                PerceiverNewsAndFrees<WMWorldNode> newsAndFrees,
                Long perceiverOid)
    {
        if (Log.loggingDebug)
            Log.debug("processNewsAndFrees: perceiverOid " +
                perceiverOid +
                " freeCount="+newsAndFrees.getFreedElements().size() +
                " newCount="+newsAndFrees.getNewElements().size());

        if (p instanceof MobilePerceiver) {
            MobilePerceiver<WMWorldNode> mobileP = (MobilePerceiver<WMWorldNode>) p;
            Long pOid = getPerceiverOid(mobileP);
            long instanceOid = mobileP.getElement().getInstanceOid();
            QuadTree<WMWorldNode> quadtree = quadtrees.get(instanceOid);
            if (quadtree == null) {
                Log.error("processNewsAndFrees: unknown instanceOid=" +
                        instanceOid + " oid="+pOid);
                return null;
            }

            if (Log.loggingDebug)
                Log.debug("processNewsAndFrees: perceiverOid " + perceiverOid + ", pOid " + pOid);

            // Perception debugging aid, see QuadTree.java
            if (quadtree.spawningNewsAndFrees != null) {
                if (!quadtree.spawningNewsAndFrees.getMap().values().contains(newsAndFrees)) {
                    System.out.println("perceiverOid " + perceiverOid +
                        ", pOid " + pOid +
                        " freeCount="+newsAndFrees.getFreedElements().size() +
                        " newCount="+newsAndFrees.getNewElements().size());
                    Thread.dumpStack();
                }
            }

            if (perceiverOid == World.DEBUG_OID || pOid == World.DEBUG_OID) {
                Log.info("processNewsAndFrees: oid="+ perceiverOid +
                    " pOid="+pOid+
                    " newCount="+newsAndFrees.getNewElements().size()+
                    " freeCount="+newsAndFrees.getFreedElements().size());
            }

            PerceptionMessage message =
                new PerceptionMessage(
                    WorldManagerClient.MSG_TYPE_PERCEPTION_INFO, pOid);

            int count = 0;
            for (WMWorldNode lostNode : newsAndFrees.getFreedElements()) {
                MVObject lostObj = lostNode.getObject();
                Long lostOid = lostObj.getOid();
                message.lostObject(pOid, lostOid, lostObj.getType());
                count++;
            }

            for (WMWorldNode gainNode : newsAndFrees.getNewElements()) {
                MVObject gainObj = gainNode.getObject();
                Long gainOid = gainObj.getOid();
                PerceptionMessage.ObjectNote note =
                    new PerceptionMessage.ObjectNote(pOid, gainOid,
                        gainObj.getType());
                note.setObjectInfo(
                    makePerceptionInfo(gainOid, gainObj));
                message.gainObject(note);
                count++;
            }

            Engine.getAgent().sendBroadcast(message);

            message.setMsgType(WorldManagerClient.MSG_TYPE_PERCEPTION);
            if (message.getGainObjects() != null) {
                for (PerceptionMessage.ObjectNote gainNote :
                        message.getGainObjects()) {
                    gainNote.setObjectInfo(null);
                }
            }

            Engine.getAgent().sendBroadcast(message);

            if (pOid.equals(perceiverOid))
                return count;
            else
                return null;

        } else if (p instanceof FixedPerceiver) {
            FixedPerceiver<WMWorldNode> fixedP = (FixedPerceiver<WMWorldNode>) p;
            for (WMWorldNode freeNode : newsAndFrees.getFreedElements())
                freeObjectForFixedPerceiver(fixedP, freeNode);
            for (WMWorldNode newNode : newsAndFrees.getNewElements())
                newObjectForFixedPerceiver(fixedP, newNode);
            return null;
        } else {
            throw new RuntimeException("unknown perceiver type");
        }
    }

    // for PerceptionUpdateTrigger
    public void preUpdate(PerceptionFilter filter,
        FilterUpdate.Instruction instruction,
        AgentHandle sender,
        SubscriptionHandle sub)
    {
        if (instruction.opCode == FilterUpdate.OP_ADD &&
                instruction.fieldId == PerceptionFilter.FIELD_TARGETS) {
            MessageType perceptionType = null;
            if (filter.getMessageTypes().contains(WorldManagerClient.MSG_TYPE_PERCEPTION_INFO))
                perceptionType = WorldManagerClient.MSG_TYPE_PERCEPTION_INFO;
            else if (filter.getMessageTypes().contains(WorldManagerClient.MSG_TYPE_PERCEPTION))
                perceptionType = WorldManagerClient.MSG_TYPE_PERCEPTION;
            else
                return;

            Long targetOid = (Long) instruction.value;
            MVObject obj = (MVObject)getWorldManagerEntity(targetOid);
            if (obj == null)
                return;

            WMWorldNode targetNode = (WMWorldNode) obj.worldNode();
            if (targetNode == null)
                return;

            Collection<WMWorldNode> perceivables;
            PerceptionMessage message;

            QuadTree<WMWorldNode> quadtree =
                quadtrees.get(targetNode.getInstanceOid());
            quadtree.getLock().lock();
            try {
                targetNode = (WMWorldNode) obj.worldNode();
                if (targetNode == null)
                    return;
                perceivables = quadtree.getElementPerceivables(targetNode);
                message = new PerceptionMessage(perceptionType, targetOid);
                for (WMWorldNode gainNode : perceivables) {
                    MVObject gainObj = gainNode.getObject();
                    Long gainOid = gainObj.getOid();
                    if (gainOid.equals(targetOid)) {
                        // Targets do not perceive themselves.  This
                        // matches the existing WorldManager behavior.
                        continue;
                    }
                    PerceptionMessage.ObjectNote note =
                        new PerceptionMessage.ObjectNote(targetOid, gainOid,
                            gainObj.getType());
                    if (perceptionType ==
                            WorldManagerClient.MSG_TYPE_PERCEPTION_INFO)
                        note.setObjectInfo(
                            makePerceptionInfo(gainOid, gainObj));
                    message.gainObject(note);
                }
            } finally {
                quadtree.getLock().unlock();
            }
            if (perceivables.size() > 0) {
                if (Log.loggingDebug)
                    Log.debug("PerceptionUpdateTrigger: sending initial perception for oid="+targetOid+ " agent="+sender.getAgentName());
                Engine.getAgent().sendDirect(message, sender, sub);
            }
        }
    }

    // for PerceptionUpdateTrigger
    public void postUpdate(PerceptionFilter filter,
        FilterUpdate.Instruction instruction,
        AgentHandle sender,
        SubscriptionHandle sub)
    {
        // do nothing
    }

    public PathInfo getPathInfo() {
        return pathInfo;
    }
    
    public void setPathInfo(PathInfo pathInfo) {
        this.pathInfo = pathInfo;
    	PathSearcher.createPathSearcher(pathInfo, World.getGeometry());
    }

    // This class only exists to get the latest interpolated location
    // without updating the object's world node or moving it within
    // the quad tree.
    class CaptureInterpWorldNode extends InterpolatedWorldNode {
        CaptureInterpWorldNode(InterpolatedWorldNode node)
        {
            node.lock.lock();
            try {
                instanceOid = node.getInstanceOid();
                interpLoc = node.getInterpLoc();
                lastInterp = node.getLastInterp();
                rawLoc = node.getRawLoc();
                dir = node.getDir();
                pathInterpolator = node.getPathInterpolator();
                spawned = node.isSpawned();
                orient = node.getOrientation();
                lastUpdate = node.getLastUpdate();
                //## objHandle
                //## parent
                //## children
            } finally {
                node.lock.unlock();
            }
        }

        // Interpolator calls this to set the interpolated location
        public void setPathInterpolatorValues(long time, MVVector newDir,
                Point newLoc, Quaternion orientation)
        {
            lastInterp = time;
            interpLoc = (Point)newLoc.clone();
            dir = newDir;
            orient = orientation;
        }
        
        public static final long serialVersionUID = 1L;
    }

    protected WorldManagerClient.ObjectInfo makeObjectInfo(Long oid)
    {
        if (Log.loggingDebug)
            log.debug("makeObjectInfo: oid=" + oid);

        Entity entity = getWorldManagerEntity(oid);
        if (entity == null) {
            return null;
        }
        if (!(entity instanceof MVObject)) {
            throw new MVRuntimeException("entity is not MVObject");
        }
        MVObject obj = (MVObject) entity;

        InterpolatedWorldNode.InterpolatedDirLocOrientTime before = null;
        if (Log.loggingDebug)
            before = obj.getDirLocOrientTime();

        // Get up-to-date interpolated location
        CaptureInterpWorldNode capture = new CaptureInterpWorldNode(
                (InterpolatedWorldNode)obj.getProperty(MVObject.wnodeKey));
        capture.getLoc();   // force interpolator to run
        InterpolatedWorldNode.InterpolatedDirLocOrientTime vals =
            capture.getDirLocOrientTime();

        if (Log.loggingDebug) {
            float distance = Point.distanceTo(vals.interpLoc, before.interpLoc);
            if (distance != 0F)
                Log.debug("DISTANCE "+distance+" TIME "+
                    (vals.lastInterp - before.lastInterp) + " DIR "+
                    vals.dir + " oid="+obj.getOid());
        }

        WorldManagerClient.ObjectInfo objInfo =
            new WorldManagerClient.ObjectInfo();

        objInfo.instanceOid = capture.getInstanceOid();
        objInfo.oid = obj.getOid();
        objInfo.name = obj.getName();
        objInfo.loc = vals.interpLoc;
        objInfo.orient = vals.orient;
        objInfo.scale = obj.scale();
        objInfo.objType = obj.getType();
        objInfo.dir = vals.dir;
        objInfo.lastInterp = vals.lastInterp;

        if (objInfo.objType == ObjectTypes.mob) {
            Object pathMsgObject = obj.getProperty(WorldManagerClient.MOB_PATH_PROPERTY);
            if (pathMsgObject != null) {
                WorldManagerClient.MobPathMessage pathMsg = (WorldManagerClient.MobPathMessage)pathMsgObject;
                objInfo.setProperty(WorldManagerClient.MOB_PATH_PROPERTY, pathMsg);
            }
        }

        WMWorldNode wnode = (WMWorldNode) obj.worldNode();
        Boolean b = wnode.getFollowsTerrain();
        objInfo.followsTerrain = (b == null ? false : (boolean)b);

        return objInfo;
    }
    
    // handles a mob path message request
    class MobPathReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.MobPathReqMessage pathReqMsg = (WorldManagerClient.MobPathReqMessage) msg;
            if (Log.loggingDebug)
                log.debug("Received MobPathReqMessage " + pathReqMsg);
            Long oid = pathReqMsg.getSubject();
            MVObject obj = (MVObject)getWorldManagerEntity(oid);
            if (obj == null) {
                Log.error("MobPathReqHook: unknown oid=" + oid);
                return true;
            }
	    InterpolatedWorldNode wnode = (InterpolatedWorldNode)obj.worldNode();
            float speed = pathReqMsg.getSpeed();
            boolean nomove = obj.getBooleanProperty(WorldManagerClient.WORLD_PROP_NOMOVE);
            if (nomove) {
                WorldManagerClient.MobPathCorrectionMessage correction = new 
                    WorldManagerClient.MobPathCorrectionMessage(oid,  System.currentTimeMillis(), "linear", 0, 
                        "", new LinkedList<Point>());
                Engine.getAgent().sendBroadcast(correction);
            }
            else {
                // if the obj already has an interpolator, call getLoc to
                // make sure that it has the latest location.
                if (wnode.getPathInterpolator() != null) {
                    if (Log.loggingDebug)
                        log.debug("MobPathReqHook: calling getLoc on oid " + oid);
                    wnode.getLoc();
                }
                long startTime = pathReqMsg.getStartTime();
                String interpKind = pathReqMsg.getInterpKind();
                String terrainString = pathReqMsg.getTerrainString();
                List<Point> pathPoints = pathReqMsg.getPathPoints();
                WorldManagerClient.MobPathMessage pathMsg = new WorldManagerClient.MobPathMessage(
                    oid, startTime, interpKind, speed, terrainString, pathPoints);
                // Save the message in the entity
                obj.setProperty(WorldManagerClient.MOB_PATH_PROPERTY, pathMsg);
                Engine.getAgent().sendBroadcast(pathMsg);
                if (Log.loggingDebug)
                    log.debug("Sending MobPathMessage " + pathMsg);
                if (speed != 0) {
                    PathInterpolator pathInterpolator = 
                        (interpKind.equalsIgnoreCase("spline") ?
                            new PathSpline(oid, startTime, speed, terrainString, pathPoints) :
                            new PathLinear(oid, startTime, speed, terrainString, pathPoints));
                    wnode.setPathInterpolator(pathInterpolator);
                }
                else {
                    // remove the pathInterpolator
                    wnode.setPathInterpolator(null);
                }
            }
            return true;
        }
    }

    /**
     * send basic info about the object
     */
    class ObjectInfoReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.ObjInfoReqMessage objInfoMsg =
                (WorldManagerClient.ObjInfoReqMessage) msg;

            Long oid = objInfoMsg.getSubject();

            WorldManagerClient.ObjectInfo objInfo = makeObjectInfo(oid);

            ResponseMessage respMsg =
                new WorldManagerClient.ObjInfoRespMessage(objInfoMsg, 
                    msg.getSenderName(), objInfo);
            Engine.getAgent().sendResponse(respMsg);
            return true;
        }
    }

    class DisplayContextReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.DisplayContextReqMessage rMsg = (WorldManagerClient.DisplayContextReqMessage) msg;

            Long oid = rMsg.getSubject();
            DisplayContext dc = getDisplayContext(oid);
            if (Log.loggingDebug)
                log.debug("DisplayContextHook: oid=" + oid + ", dc=" + dc);
            Engine.getAgent().sendObjectResponse(msg, dc);
            return true;
        }
    }

    class SpawnReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.SpawnReqMessage wrldMsg = (WorldManagerClient.SpawnReqMessage) msg;

            Long oid = wrldMsg.getSubject();

            if (Log.loggingDebug)
                Log.debug("SpawnReqHook: spawning oid=" + oid + ", msg=" + msg);
            
            Entity entity = getWorldManagerEntity(oid);
            if ((entity == null) || (!(entity instanceof MVObject))) {
                log.error("SpawnReqHook: entity null or not found, oid="+oid);
                // -1 means that the spawn failed
                Engine.getAgent().sendObjectResponse(msg, -1);
                return false;
            }

            MVObject obj = (MVObject) entity;
            WorldNode wnode = obj.worldNode();
            if (wnode.isSpawned()) {
                log.error("SpawnReqHook: object already spawned oid="+oid);
                Engine.getAgent().sendObjectResponse(msg, -3);
                return false;
            }

            QuadTree<WMWorldNode> quadtree =
                quadtrees.get(wnode.getInstanceOid());

            if (quadtree == null) {
                log.error("SpawnReqHook: unknown instanceOid="+
                    wnode.getInstanceOid() + " oid="+oid);
                Engine.getAgent().sendObjectResponse(msg, -2);
                return false;
            }

            if (wrldMsg.getPreMessage() != null) {
                Engine.getAgent().sendBroadcast(wrldMsg.getPreMessage());
            }

	    Integer newsAndFreesCount = null;
            try {
		newsAndFreesCount = spawnObject(obj, quadtree);
	    }
	    catch (Exception e) {
		Log.exception("spawnObject failed", e);
                // -1 means that the spawn failed
                Engine.getAgent().sendObjectResponse(msg, -1);
		return false;
	    }

            if (wrldMsg.getPostMessage() != null) {
                Engine.getAgent().sendBroadcast(wrldMsg.getPostMessage());
            }

            // send out a spawned object message
            Message spawnedMsg = new WorldManagerClient.SpawnedMessage(oid,
                wnode.getInstanceOid(), obj.getType());
            Engine.getAgent().sendBroadcast(spawnedMsg);

            // send a response message with the count of news and
            // frees for this object if it is a perceiver, or null if
            // it isn't
            Engine.getAgent().sendObjectResponse(msg, (Integer)newsAndFreesCount);

            if (obj.isUser())
                sendRegionUpdate(obj);

            return true;
        }
    }

    class DespawnReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.DespawnReqMessage wrldMsg = (WorldManagerClient.DespawnReqMessage) msg;

            Long oid = wrldMsg.getSubject();
            
            if (Log.loggingDebug)
                log.debug("DespawnReqHook: oid=" + oid);

            Entity entity = getWorldManagerEntity(oid);

            if ((entity == null) || (!(entity instanceof MVObject))) {
                log.error("DespawnReqHook: entity null or not found, oid="+oid);
                Engine.getAgent().sendBooleanResponse(msg, Boolean.FALSE);
                return false;
            }

            if (wrldMsg.getPreMessage() != null) {
                Engine.getAgent().sendBroadcast(wrldMsg.getPreMessage());
            }

            MVObject obj = (MVObject) entity;
            WorldNode wnode = obj.worldNode();

	    try {
		despawnObject(obj);
	    }
	    catch (Exception e) {
		Log.exception("despawnObject failed", e);
                Engine.getAgent().sendBooleanResponse(msg, Boolean.FALSE);
		return false;
	    }

            if (wrldMsg.getPostMessage() != null) {
                Engine.getAgent().sendBroadcast(wrldMsg.getPostMessage());
            }

            // send out a despawned notification message
            if (wnode != null) {
                Message despawnedMsg =
                    new WorldManagerClient.DespawnedMessage(
                        oid, wnode.getInstanceOid(), obj.getType());
                Engine.getAgent().sendBroadcast(despawnedMsg);
            }

            Engine.getAgent().sendBooleanResponse(msg, Boolean.TRUE);
            return true;
        }
    }

    class WorldManagerGenerateSubObjectHook extends GenerateSubObjectHook {
	public WorldManagerGenerateSubObjectHook() {
	    super(WorldManagerPlugin.this);
	}

        public SubObjData generateSubObject(Template template,
            Namespace namespace, Long masterOid)
        {
            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid=" + masterOid +
                    " namespace=" + namespace +
                    " template=" + template);

            Boolean persistent = (Boolean)template.get(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT);
            if (persistent == null)
                persistent = false;

            if (namespace == WorldManagerClient.INSTANCE_NAMESPACE) {
                return generateInstanceSubObject(masterOid, persistent);
            }
            
            Map<String, Serializable> props = template.getSubMap(Namespace.WORLD_MANAGER);
            if (props == null) {
                Log.warn("GenerateSubObjectHook: no props in ns " + Namespace.WORLD_MANAGER);
                return null;
            }
            
            Long instanceOid = (Long) props.get(WorldManagerClient.TEMPL_INSTANCE);
            if (instanceOid == null) {
                Log.error("GenerateSubObjectHook: missing instanceOid");
                return null;
            }
            if (quadtrees.get(instanceOid) == null) {
                Log.error("GenerateSubObjectHook: unknown instanceOid="+
                    instanceOid);
                return null;
            }

            // get the location for the object
            Point loc = (Point) props.get(WorldManagerClient.TEMPL_LOC);
            if (loc == null) {
                Log.warn("GenerateSubObjectHook: no loc in templ");
                return null;
            }
            // get the name
            String objName = (String) props.get(WorldManagerClient.TEMPL_NAME);
            if (objName == null) {
                objName = template.getName();
                if (objName == null) {
                    objName = "(null)";
                }
            }
            // get the orientation
            Quaternion orient = (Quaternion) props.get(WorldManagerClient.TEMPL_ORIENT);
            if (orient == null) {
                orient = new Quaternion();
            }
            // get the scale
            MVVector scale = (MVVector) props.get(WorldManagerClient.TEMPL_SCALE);
            if (scale == null) {
                scale = new MVVector(1,1,1);
            }
            // get the perception radius
            Integer perceptionRadius = (Integer) props.get(WorldManagerClient.TEMPL_PERCEPTION_RADIUS);
           
            // generate the subobject
            MVObject wObj = generateWorldManagerSubObject(template, masterOid);
           
            wObj.setName(objName);
            wObj.scale(scale);
            
	    // set the base display context for the object
	    DisplayContext dc = (DisplayContext)props.get(WorldManagerClient.TEMPL_DISPLAY_CONTEXT);
	    if (dc != null) {
	        dc = (DisplayContext) dc.clone();
                dc.setObjRef(wObj.getOid());
                wObj.displayContext(dc);
            }
            else {
                Log.debug("GenerateSubObjectHook: object has no display context, oid="+masterOid);
            }
            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: created entity " + wObj + ", loc=" + loc);

            // create a world node for the object
            WMWorldNode wnode;
            if (perceptionRadius != null) {
                wnode = new WMWorldNode(perceptionRadius);
            }
            else {
                wnode = new WMWorldNode();
            }
            wnode.setInstanceOid(instanceOid);
            wnode.setLoc(loc);
            wnode.setOrientation(orient);
	    Boolean followsTerrain = (Boolean)props.get(WorldManagerClient.TEMPL_FOLLOWS_TERRAIN);
	    if (followsTerrain == null)
		followsTerrain = Boolean.TRUE;
	    wnode.setFollowsTerrain(followsTerrain);
            wObj.worldNode(wnode);
            wnode.setObject(wObj);
            wObj.setPersistenceFlag(persistent);

            // register the entity
            EntityManager.registerEntityByNamespace(wObj, Namespace.WORLD_MANAGER);

            if (persistent)
                Engine.getPersistenceManager().persistEntity(wObj);

            // subscribe to messages regarding this object
            // we need this for static objects.
            // problem is when players log in, we try to bind them
            // again, but subscribeForMob checks for double binds.
            subscribeForMob(masterOid);

            return new SubObjData();
        }

        public SubObjData generateInstanceSubObject(Long instanceOid,
            Boolean persistent)
        {
            WorldManagerInstance instance = createInstanceEntity(instanceOid);
            instance.setPersistenceFlag(persistent);

            if (persistent)
                Engine.getPersistenceManager().persistEntity(instance);

            subscribeForObject(instanceOid);
            hostInstance(instanceOid,instance.getQuadTree().getLocalGeometry());

            //##
            //WorldManagerClient.sendPerceiverRegionsMsg(0L,
            //    Geometry.maxGeometry(), null);

            return new SubObjData();
        }
    } // end WorldManagerGenerateSubObjectHook

    WorldManagerInstance createInstanceEntity(long instanceOid)
    {
        WorldManagerInstance instance = new WorldManagerInstance(instanceOid);
        initializeInstance(instance);

        EntityManager.registerEntityByNamespace(instance,
            WorldManagerClient.INSTANCE_NAMESPACE);

        return instance;
    }

    void initializeInstance(WorldManagerInstance instance)
    {
        Geometry localGeo = Geometry.maxGeometry();
        if (localGeo == null) {
            throw new RuntimeException("null local geometry");
        }

        QuadTree<WMWorldNode> quadtree;
        quadtree = new QuadTree<WMWorldNode>(Geometry.maxGeometry(),
            defaultWorldManagerHysteresis);
        quadtrees.put(instance.getOid(), quadtree);

        quadtree.setMaxObjects(WorldManagerPlugin.this.maxObjects);
        quadtree.setMaxDepth(WorldManagerPlugin.this.maxDepth);
        quadtree.setLocalGeometry(localGeo);

        instance.setQuadTree(quadtree);
    }

    void hostInstance(long instanceOid, Geometry localGeo)
    {
        WorldManagerFilter.InstanceGeometry instanceGeo =
            new WorldManagerFilter.InstanceGeometry();
        instanceGeo.instanceOid = instanceOid;
        instanceGeo.geometry = new ArrayList<Geometry>(1);
        instanceGeo.geometry.add(localGeo);

        // Update WM subscriptions with new instance
        FilterUpdate filterUpdate = new FilterUpdate();
        filterUpdate.addFieldValue(WorldManagerFilter.FIELD_INSTANCES,
            instanceGeo);
        ((WorldManagerFilter)selectionFilter).applyFilterUpdate(filterUpdate);
        Engine.getAgent().applyFilterUpdate(selectionSubscription,
            filterUpdate, MessageAgent.BLOCKING);

        filterUpdate = new FilterUpdate();
        filterUpdate.addFieldValue(WorldManagerFilter.FIELD_INSTANCES,
            instanceGeo);
        newRegionFilter.applyFilterUpdate(filterUpdate);
        Engine.getAgent().applyFilterUpdate(newRegionSub, filterUpdate,
            MessageAgent.BLOCKING);
    }

    void unhostInstance(long instanceOid)
    {
        // Update WM subscriptions with removed instance
        FilterUpdate filterUpdate = new FilterUpdate();
        filterUpdate.removeFieldValue(WorldManagerFilter.FIELD_INSTANCES,
            new Long(instanceOid));
        ((WorldManagerFilter)selectionFilter).applyFilterUpdate(filterUpdate);
        Engine.getAgent().applyFilterUpdate(selectionSubscription,
            filterUpdate, MessageAgent.BLOCKING);

        filterUpdate = new FilterUpdate();
        filterUpdate.removeFieldValue(WorldManagerFilter.FIELD_INSTANCES,
            new Long(instanceOid));
        newRegionFilter.applyFilterUpdate(filterUpdate);
        Engine.getAgent().applyFilterUpdate(newRegionSub, filterUpdate,
            MessageAgent.BLOCKING);
    }

    class InstanceLoadHook implements LoadHook
    {
        public void onLoad(Entity entity)
        {
            WorldManagerInstance instance = (WorldManagerInstance) entity;
            initializeInstance(instance);
            subscribeForObject(entity.getOid());
        }
    }

    class InstanceUnloadHook implements UnloadHook {
        public void onUnload(Entity entity)
        {
            quadtrees.remove(entity.getOid());
            unsubscribeForObject(entity.getOid());
            unhostInstance(entity.getOid());
        }
    }

    class InstanceDeleteHook implements DeleteHook {
        public void onDelete(Entity entity)
        {
            quadtrees.remove(entity.getOid());
            unsubscribeForObject(entity.getOid());
            unhostInstance(entity.getOid());
        }
        public void onDelete(Long oid, Namespace namespace)
        {
            // n/a
        }
    }

    public String getInstanceInfoString(long instanceOid)
    {
        String info = "";
        Entity entity = EntityManager.getEntityByNamespace(instanceOid,
            WorldManagerClient.INSTANCE_NAMESPACE);
        if (entity != null) {
            info += "Entity:               " + "yes" + "\n";
            info += "Entity class:         " + entity.getClass().getSimpleName() + "\n";
            info += "Entity name:          " + entity.getName() + "\n";
        }
        else
            info += "Entity:               " + "no" + "\n";

        QuadTree<WMWorldNode> quadtree = quadtrees.get(instanceOid);
        if (quadtree != null)
            info += "Quad tree:            " + "yes" + "\n";
        else
            info += "Quad tree:            " + "no" + "\n";

        boolean rc = subObjectFilter.hasSubject(instanceOid);
        info += "Sub-object filter:    " + rc + "\n";

        List<Geometry> geometry = newRegionFilter.getInstance(instanceOid);
        if (geometry != null) {
            info += "New region filter:    " + "yes" + "\n";
            info += "New region geometry:  " + geometry.get(0) + "\n";
        }
        else
            info += "New region filter:    " + "no" + "\n";

        WorldManagerFilter filter = (WorldManagerFilter)selectionFilter;
        geometry = filter.getInstance(instanceOid);
        if (geometry != null) {
            info += "Selection filter:     " + "yes" + "\n";
            info += "Selection geometry:   " + geometry.get(0) + "\n";
         }
        else
            info += "Selection filter:     " + "no" + "\n";

        return info;
    }

    class WorldManagerTransferHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            EnginePlugin.TransferObjectMessage transferMsg = (EnginePlugin.TransferObjectMessage)msg;
            MVObject obj = (MVObject)transferMsg.getEntity();
            if (Log.loggingDebug) {
                Log.debug("WorldManagerTransferHook: obj=" + obj + ", sessionid=" + msg.getSenderName());
            }
            
            // create the world node
            WMWorldNode wnode = new WMWorldNode();
            wnode.setInstanceOid(obj.worldNode().getInstanceOid());
            wnode.setLoc(obj.getLoc());
            wnode.setOrientation(obj.getOrientation());
            wnode.setFollowsTerrain(Boolean.TRUE);
            obj.worldNode(wnode);
            wnode.setObject(obj);
            
            QuadTree<WMWorldNode> quadtree = quadtrees.get(wnode.getInstanceOid());
            if (quadtree == null) {
                log.error("WorldManagerTransferHook: unknown instanceOid="+
                    wnode.getInstanceOid() + " oid=" + obj.getOid());
                Engine.getAgent().sendBooleanResponse(msg, Boolean.FALSE);
                return false;
            }

            // register the entity
            Long masterOid = obj.getMasterOid();
            registerWorldManagerEntity(obj);
            
            // subscribe to messages regarding this object
            // we need this for static objects.
            // problem is when players log in, we try to bind them
            // again, but subscribeForMob checks for double binds.
            subscribeForMob(masterOid);
            if (Log.loggingDebug) {
                Log.debug("WorldManagerTransferHook: bound obj " + obj);
            }
            
            spawnObject(obj,quadtree);
            if (Log.loggingDebug) {
                Log.debug("WorldManagerTransferHook: complete, spawned obj " + obj);
            }
            Engine.getAgent().sendBooleanResponse(msg, Boolean.TRUE);
            
            return true;
        }
    }

    /**
     * Override this method to change what kind of object is created
     * for the sub object hook.
     * @return MVObject representing the generated sub-object.
     */
    protected MVObject generateWorldManagerSubObject(Template template, Long masterOid) {
        MVObject wObj = new MVObject(masterOid);

	Map<String, Serializable> props = template.getSubMap(Namespace.WORLD_MANAGER);
	if (props == null) {
	    Log.warn("WorldManagerPlugin.generateSubObject: no props in ns "
		     + Namespace.WORLD_MANAGER);
	    return null;
	}

	// copy properties from template to object
	for (Map.Entry<String, Serializable> entry : props.entrySet()) {
	    String key = entry.getKey();
	    Serializable value = entry.getValue();
	    if (!key.startsWith(":")) {
		wObj.setProperty(key, value);
	    }
	}

        ObjectType objType = (ObjectType) template.get(Namespace.WORLD_MANAGER,
           WorldManagerClient.TEMPL_OBJECT_TYPE);
        // set the object type
        if (objType != null)
            wObj.setType(objType);

        return wObj;
    }

    protected void subscribeForMob(Long oid) {
        // subscribe for subsequent messages from this oid
        // do this before responding to the bind message
        if (Log.loggingDebug)
            log.debug("subscribeForMob: oid=" + oid);

        if (mobFilter.addSubjectIfMissing(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate();
            filterUpdate.addFieldValue(PerceptionFilter.FIELD_SUBJECTS, oid);
            Engine.getAgent().applyFilterUpdate(mobSubId, filterUpdate,
                MessageAgent.BLOCKING);
        }
        else {
            Log.debug("subscribeForMob: mobFilter double bind oid="+oid);
        }

        if (mobRPCFilter.addSubjectIfMissing(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate();
            filterUpdate.addFieldValue(PerceptionFilter.FIELD_SUBJECTS, oid);
            Engine.getAgent().applyFilterUpdate(mobRPCSubId, filterUpdate,
                MessageAgent.BLOCKING);
        }
        else {
            Log.debug("subscribeForMob: mobRPCFilter double bind oid="+oid);
        }

        subscribeForObject(oid);
    }
    
    protected void subscribeForObject(Long oid) {
        // subscribe for subsequent messages from this oid
        // do this before responding to the bind message
        if (Log.loggingDebug)
            log.debug("subscribeForObject: oid=" + oid);

        if (subObjectFilter.addSubjectIfMissing(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate();
            filterUpdate.addFieldValue(PerceptionFilter.FIELD_SUBJECTS, oid);
            Engine.getAgent().applyFilterUpdate(subObjectSubscription,
                filterUpdate, MessageAgent.BLOCKING);
        }
        else {
            Log.debug("subscribeForObject: subObjectFilter double bind oid="+oid);
        }
    }

    protected void unsubscribeForMob(Long oid) {
        // subscribe for subsequent messages from this oid
        // do this before responding to the bind message
        if (Log.loggingDebug)
            log.debug("unsubscribeForObject: oid=" + oid);

        if (mobFilter.removeSubject(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate();
            filterUpdate.removeFieldValue(PerceptionFilter.FIELD_SUBJECTS, oid);
            Engine.getAgent().applyFilterUpdate(mobSubId, filterUpdate,
                MessageAgent.BLOCKING);
        }
        else {
            Log.debug("unsubscribeForObject: mobFilter double remove oid="+oid);
        }

        if (mobRPCFilter.removeSubject(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate();
            filterUpdate.removeFieldValue(PerceptionFilter.FIELD_SUBJECTS, oid);
            Engine.getAgent().applyFilterUpdate(mobRPCSubId, filterUpdate,
                MessageAgent.BLOCKING);
        }
        else {
            Log.debug("unsubscribeForObject: mobRPCFilter double remove oid="+oid);
        }

        unsubscribeForObject(oid);
    }

    protected void unsubscribeForObject(Long oid) {
        // subscribe for subsequent messages from this oid
        // do this before responding to the bind message
        if (Log.loggingDebug)
            log.debug("unsubscribeForObject: oid=" + oid);

        if (subObjectFilter.removeSubject(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate();
            filterUpdate.removeFieldValue(PerceptionFilter.FIELD_SUBJECTS, oid);
            Engine.getAgent().applyFilterUpdate(subObjectSubscription,
                filterUpdate, MessageAgent.BLOCKING);
        }
        else {
            Log.debug("unsubscribeForObject: subObjectFilter double remove oid="+oid);
        }
    }

    /**
     * Helper method - spawns the object.  obj is the sub object.  The
     * return value is the count of news plus count of frees perceived by
     * this object as it's spawned, if the object is mobile perceiver.
     */
    protected Integer spawnObject(MVObject obj, QuadTree<WMWorldNode> quadtree)
    {
        WMWorldNode wnode = (WMWorldNode) obj.worldNode();
        if (wnode == null) {
            throw new MVRuntimeException("obj has no world node: " + obj);
        }
        MVObject backRef = wnode.getObject();
        if (backRef == null) {
            throw new MVRuntimeException("obj wnode backref is null: " + obj);
        }
        if (!backRef.getOid().equals(obj.getOid())) {
            throw new MVRuntimeException("obj wnode backref does not match self: "
                    + obj);
        }

        // register perceiver callback if this object has a perceiver
        MobilePerceiver<WMWorldNode> p = obj.perceiver();
        Long mobilePerceiverOid = -1L;
        if (p != null) {
            mobilePerceiverOid = obj.getMasterOid();
            if (Log.loggingDebug)
                log.debug("spawnObject: registering perceiver cb: " + obj + ", masterOid " + mobilePerceiverOid);

            if (wnode.getPerceiver() == null) {
                throw new MVRuntimeException("wnode doesnt have perceiver, obj=" + obj);
            }
            p.registerCallback(this);
        } else {
            if (Log.loggingDebug)
                log.debug("spawnObject: no perceiver for obj " + obj);
        }

        // add the node to quad tree to complete spawning.  The count
        // is the number of news plus frees perceived by the new mobile perceiver.
        wnode.isLocal(true);
        wnode.isSpawned(true);
	Integer newsAndFressCount = quadtree.addElementReturnCountForPerceiver(wnode, mobilePerceiverOid);

        if (Log.loggingDebug)
            log.debug("spawnObject: spawned obj: " + obj + ", wnode=" + wnode);
        return newsAndFressCount;
    }

    protected void despawnObject(MVObject obj)
    {
        if (obj.isUser()) {
            // save the object
            Engine.getPersistenceManager().setDirty(obj);
        }

        WMWorldNode wnode = (WMWorldNode) obj.worldNode();
        if (wnode == null) {
            throw new MVRuntimeException("obj has no world node: " + obj);
        }
        QuadTree<WMWorldNode> quadtree = quadtrees.get(wnode.getInstanceOid());
        if (quadtree == null) {
            log.error("despawnObject: unknown instanceOid="+
                wnode.getInstanceOid() + " oid="+obj.getOid());
            return;
        }

        quadtree.getLock().lock();
        try {
            if (wnode.getQuadNode() == null)
                return;
            MVObject backRef = wnode.getObject();
            if (backRef == null) {
                throw new MVRuntimeException("obj wnode backref is null: " + obj);
            }
            if (!backRef.getOid().equals(obj.getOid())) {
                throw new MVRuntimeException("obj wnode backref does not match: "
                        + obj);
            }

            wnode.isSpawned(false);
            // add the node to quad tree to complete spawning
            // this will also set the wnode's quadtreenode
            quadtree.removeElement(wnode);

            if (Log.loggingDebug)
                log.debug("despawnObject: despawned obj: " + obj);
        } finally {
            quadtree.getLock().unlock();
        }

        if (obj.isUser()) {
            if (Log.loggingDebug)
                log.debug("despawnObject: removing regions for oid=" +
                    obj.getOid());
            List<Region> regionList = new ArrayList<Region>(0);
            synchronized (updater) {
                updater.updateRegion(obj, regionList);
            }
        }
    }

    /**
     * Tell the user about object's model info, animation, sound inv, quest log,
     * attachments, and status.
     */
    // used to be inside NotifyNewObjectHandler and MarsNewObjectHandler
    class UpdateObjectHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.UpdateMessage updateReq = (WorldManagerClient.UpdateMessage) msg;
            Long notifyOid = updateReq.getTarget();
            Long updateOid = updateReq.getSubject();

            if (Log.loggingDebug)
                Log.debug("UpdateObjectHook: notifyOid=" + notifyOid + " updateOid=" + updateOid);

            // is the update object spawned?
            Entity updateEntity = getWorldManagerEntity(updateOid);
	    if (updateEntity == null) {
                log.warn("UpdateObjectHook: could not find sub object for oid=" + updateOid);
                return false;
	    }

            if (!(updateEntity instanceof MVObject)) {
                log.warn("UpdateObjectHook: updateObj is not MVObject: "
                        + updateOid);
                return false;
            }
            MVObject updateObj = (MVObject) updateEntity;
            if (updateObj.worldNode() == null) {
                log.warn("UpdateObjectHook: updateObj has no world node: "
                        + updateOid);
                return false;
            }

	    // Don't do this because the proxy already fetches the DC when it gets a new obj msg.
            // send over display context
	    // log.debug("handleUpdateMsg: sending display context for notify obj "
            //          + notifyOid + ", updateOid=" + updateOid);
            // sendDCMessage(notifyOid, updateObj);

            // send over any statistics and state information
            sendTargetedPropertyMessage(notifyOid, updateObj);

            sendWNodeMessage(notifyOid, updateObj);
            
	    List<SoundData> soundData=
		(List<SoundData>) updateEntity.getProperty(
		WorldManagerClient.TEMPL_SOUND_DATA_LIST);
	    if (soundData != null) {
		sendObjectSoundMessage(notifyOid, updateObj, soundData);
	    }

            return true;
        }
    }

    class GetWNodeReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
	    SubjectMessage getWNodeMsg = (SubjectMessage) msg;
            if (Log.loggingDebug)
                log.debug("GetWNodeReqHook: got get wnode msg=" + getWNodeMsg);
	    Long oid = getWNodeMsg.getSubject();

	    MVObject obj = (MVObject) getWorldManagerEntity(oid);
            if (obj == null) {
                Log.error("GetWNodeReqHook: could not find obj for oid=" + oid);
                Engine.getAgent().sendObjectResponse(msg, null);
                return true;
            }
	    BasicWorldNode newWNode = obj.baseWorldNode();
            Engine.getAgent().sendObjectResponse(msg, newWNode);

	    return true;
	}
    }

    class ComReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.ComReqMessage comReqMsg =
                (WorldManagerClient.ComReqMessage) msg;

            if (Log.loggingDebug)
                log.debug("ComReqHook: got com msg from " +
                    comReqMsg.getSubject() + ", msg=" + comReqMsg.getString());

            // maybe later we can do some filtering but for now
            // just rebroadcast it
            WorldManagerClient.ComMessage comMsg =
                new WorldManagerClient.ComMessage(comReqMsg.getSubject(),
                    comReqMsg.getChannel(), comReqMsg.getString());
            Engine.getAgent().sendBroadcast(comMsg);
            return true;
        }
    }

    class OrientReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.OrientReqMessage orientReqMsg = (WorldManagerClient.OrientReqMessage) msg;

            // update the object
            Long oid = orientReqMsg.getSubject();
            Quaternion q = orientReqMsg.getQuaternion();
            Entity entity = getWorldManagerEntity(oid);
            if (entity == null) {
                log.error("OrientReqHook: could not find sub object for oid=" + oid);
                return true;
            }
            MVObject obj = (MVObject) entity;
            WorldNode wnode = obj.worldNode();
            InterpolatedWorldNode bnode = (InterpolatedWorldNode) wnode;
            bnode.setOrientation(q);

            // maybe later we can do some filtering but for now
            // just rebroadcast it
            WorldManagerClient.OrientMessage orientMsg = new WorldManagerClient.OrientMessage(
                    oid, q);
            Engine.getAgent().sendBroadcast(orientMsg);
            return true;
        }
    }

    class RefreshWNodeHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.RefreshWNodeMessage rMsg = (WorldManagerClient.RefreshWNodeMessage) msg;

            // get the worldnode
            Long oid = rMsg.getSubject();
            Entity entity = getWorldManagerEntity(oid);
            if (entity == null) {
                log.error("RefreshWNodeHook: could not find sub object for oid=" + oid);
                return true;
            }
            MVObject obj = (MVObject) entity;
            BasicWorldNode copyNode = obj.baseWorldNode();

            // just rebroadcast it
            WorldManagerClient.UpdateWorldNodeMessage uMsg =
                new WorldManagerClient.UpdateWorldNodeMessage(
                    oid, copyNode);
            Engine.getAgent().sendBroadcast(uMsg);
            return true;
        }
    }

    class PerceiverRegionsHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.PerceiverRegionsMessage pMsg = (WorldManagerClient.PerceiverRegionsMessage) msg;

            // get the session id
            String otherSessionId = pMsg.getSenderName();
            if (Log.loggingDebug)
                log.debug("PerceiverRegionsHook: otherSessionId=" + otherSessionId);
            if (otherSessionId == null) {
                throw new MVRuntimeException("other session id is null");
            }

            String mySessionId = Engine.getAgent().getName();
            if (mySessionId.equals(otherSessionId)) {
                Log.debug("PerceiverRegionsHook: ignoring, session id is same as self");
                return false;
            }

            long instanceOid = pMsg.getInstanceOid();
            QuadTree<WMWorldNode> quadtree = quadtrees.get(instanceOid);
            if (quadtree == null) {
                Log.error("PerceiverRegionsHook: unknown instanceOid="+
                    instanceOid);
                return false;
            }

            // get the geometry
            Geometry region = pMsg.getRegion();
            if (region == null) {
                throw new MVRuntimeException("region is null");
            }

            // do we already have a perceiver for this session id
            if (fixedPerceiverMap.getPerceiver(otherSessionId) != null) {
                log.warn("PerceiverRegionsHook: map exists");
                return true;
            }
            setupPerceiver(otherSessionId, quadtree, region);

            if (pMsg.getTargetSessionId() == null) {
                // respond with our own perceiver region message
                // but this time with a target session id set
                log.debug("PerceiverRegionsHook: sending our own geo msg out");
                Geometry myFixedPerceiverRegions = Geometry.maxGeometry();
                WorldManagerClient.sendPerceiverRegionsMsg(instanceOid,
                        myFixedPerceiverRegions, otherSessionId);
            } else {
                log.debug("PerceiverRegionsHook: PerceiverRegionsMsg has a target - so not sending my own out");
            }
            return true;
        }

        // assumes we are locked
        private void setupPerceiver(String remoteSessionId,
            QuadTree<WMWorldNode> quadtree, Geometry region)
        {
            // make a perceiver
            FixedPerceiver<WMWorldNode> p = new FixedPerceiver<WMWorldNode>(
                    region);
            p.registerCallback(WorldManagerPlugin.this);
            p.setFilter(new PerceiverFilter<WMWorldNode>() {
                public boolean matches(Perceiver<WMWorldNode> p,
                        WMWorldNode node) {
                    boolean val = node.isLocal();
                    if (Log.loggingDebug)
                        log.debug("PerceiverFilter: node local? " + val);
                    return val;
                }
                
                static final long serialVersionUID = 1;
            });

            // place perceiver into map
            if (Log.loggingDebug)
                log.debug("setupPerceiver: adding remote session "
                          + remoteSessionId + " into our map");
            fixedPerceiverMap.register(remoteSessionId, p);

            // add perceiver to the quad tree
            log.debug("setupPerceiver: adding fixed perceiver to quad tree");
            quadtree.addFixedPerceiver(p);
            log.debug("setupPerceiver: done adding fixed perceiver");
        }

    }

    static class FixedPerceiverMap {
        public FixedPerceiverMap() {
        }

        public void register(String remoteSessionId,
                FixedPerceiver<WMWorldNode> p) {
            lock.lock();
            try {
                sessionPerceiverMap.put(remoteSessionId, p);
                perceiverSessionMap.put(p, remoteSessionId);
            } finally {
                lock.unlock();
            }
        }

        public String getSessionId(FixedPerceiver<WMWorldNode> p) {
            lock.lock();
            try {
                return perceiverSessionMap.get(p);
            } finally {
                lock.unlock();
            }
        }

        public FixedPerceiver<WMWorldNode> getPerceiver(String sessionId) {
            lock.lock();
            try {
                return sessionPerceiverMap.get(sessionId);
            } finally {
                lock.unlock();
            }
        }

        protected Lock lock = LockFactory.makeLock("FixedPerceiverLock");

        Map<String, FixedPerceiver<WMWorldNode>> sessionPerceiverMap = new HashMap<String, FixedPerceiver<WMWorldNode>>();

        Map<FixedPerceiver<WMWorldNode>, String> perceiverSessionMap = new HashMap<FixedPerceiver<WMWorldNode>, String>();
    }

    FixedPerceiverMap fixedPerceiverMap = new FixedPerceiverMap();

    // keeps track of subscription this world server makes for
    // remote mobs. when we get a newremotemob message
    // from a remote world server (due to our fixed perceiver),
    // we set up a dir/loc subscription for the mob
    // when we get a freeremoteobj msg, we should unsubscribe
    OidSubscriptionMap remoteMobSubscription = new OidSubscriptionMap();

    /**
     * Got a new remote object - from a remote world manager we make a
     * non-local non-perceiving worldnode, add it to the quad tree subscribe to
     * worldnodeupdates from the remote world manager so we can interpolate. We
     * do NOT send out worldnodeupdates about non-local objects. we use them
     * just to notify newObj message.
     */
    class NewRemoteObjHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.NewRemoteObjectMessage rMsg = (WorldManagerClient.NewRemoteObjectMessage) msg;

            // debug check
            String targetSessionId = rMsg.getTargetSessionId();
            if (!targetSessionId.equals(Engine.getAgent().getName())) {
                throw new RuntimeException("session ids dont match");
            }

            Long newObjOid = rMsg.getSubject();
            if (Log.loggingDebug)
                log.debug("NewRemoteObjHook: oid=" + newObjOid);
            if (newObjOid == null) {
                throw new RuntimeException("no remote newobjoid");
            }

            long instanceOid = rMsg.getInstanceOid();
            Point loc = rMsg.getLoc();
            Quaternion orient = rMsg.getOrient();
            int radius = rMsg.getPerceptionRadius();
            QuadTree<WMWorldNode> quadtree = quadtrees.get(instanceOid);
            if (quadtree == null) {
                log.error("NewRemoteObjHook: unknown instanceOid="+instanceOid+
                        " oid="+newObjOid);
                return false;
            }

            // create a world node
            if (Log.loggingDebug)
                log.debug("NewRemoteObjHook: creating world node for oid "
                          + newObjOid);
            WMWorldNode node = new WMWorldNode(radius);
            node.isLocal(false);
	    node.setInstanceOid(rMsg.getInstanceOid());
            node.setLoc(loc);
            node.setOrientation(orient);
            if (Log.loggingDebug)
                log.debug("NewRemoteObjHook: created world node for oid "
                          + newObjOid + ", wnode=" + node);

            // create an object
            MVObject newObj = new MVObject(newObjOid);
	    newObj.setType(rMsg.getType());
            newObj.worldNode(node);
            node.setObject(newObj);
            if (Log.loggingDebug)
                log.debug("NewRemoteObjHook: created obj for oid " + newObjOid
                          + ", obj=" + newObj);

            // register the entity
            registerWorldManagerEntity(newObj);

            // place object into quad tree
            Log.debug("NewRemoteObjHook: placing obj into qtree, obj=" + newObj);
            quadtree.addElement(node);
            if (Log.loggingDebug)
                log.debug("NewRemoteObjHook: placed obj into qtree, obj=" + newObj);

            // subscribe for worldnode update event
            HashSet<MessageType> types = new HashSet<MessageType>();
            types.add(WorldManagerClient.MSG_TYPE_UPDATEWNODE);
            types.add(WorldManagerClient.MSG_TYPE_MOB_PATH);
            SubjectFilter newSubFilter = new SubjectFilter(types, newObjOid);
            Long newSub = Engine.getAgent().createSubscription(newSubFilter, WorldManagerPlugin.this);
            remoteMobSubscription.put(newObjOid, newSub);
            return true;
        }
    }

    class FreeRemoteObjHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.FreeRemoteObjectMessage rMsg =
		(WorldManagerClient.FreeRemoteObjectMessage) msg;

            // debug check
            String targetSessionId = rMsg.getTargetSessionId();
            if (!targetSessionId.equals(Engine.getAgent().getName())) {
                throw new RuntimeException("session ids dont match");
            }

            Long freeObjOid = rMsg.getSubject();
            if (freeObjOid == null) {
                throw new RuntimeException("no remote objoid");
            }

            long instanceOid = rMsg.getInstanceOid();
            QuadTree<WMWorldNode> quadtree = quadtrees.get(instanceOid);
            if (quadtree == null) {
                log.error("FreeRemoteObjHook: unknown instanceOid="+instanceOid+
                        " oid=" + freeObjOid);
                return false;
            }

            // get the world node
            Entity entity = getWorldManagerEntity(freeObjOid);
            if (entity == null) {
                throw new RuntimeException("could not find entity "
                        + freeObjOid);
            }
            MVObject obj = (MVObject) entity;
            WMWorldNode wnode = (WMWorldNode) obj.worldNode();
            if (Log.loggingDebug)
                log.debug("FreeRemoteObjHook: removing obj " + obj
                          + ", unsubscribing");

            // unsubscribe
            Long removeSub = remoteMobSubscription.removeSub(freeObjOid);
            if (removeSub == null) {
                throw new RuntimeException("no existing remote sub");
            }
            Engine.getAgent().removeSubscription(removeSub);
            log.debug("FreeRemoteObjHook: unsubscribed");

            // remove the wnode from the quad tree
            quadtree.removeElement(wnode);

            // remove from entity map
            removeWorldManagerEntity(freeObjOid);
            if (Log.loggingDebug)
                log.debug("FreeRemoteObjHook: removed obj " + freeObjOid);
            return true;
        }
    }

    class UpdateWNodeHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.UpdateWorldNodeMessage uMsg = (WorldManagerClient.UpdateWorldNodeMessage) msg;

            Long objOid = uMsg.getSubject();
            BasicWorldNode inNode = uMsg.getWorldNode();
            if (!(inNode instanceof BasicWorldNode)) {
                throw new RuntimeException("inWorldNode not BasicWorldNode");
            }
            if (Log.loggingDebug)
                log.debug("UpdateWNodeHook: inNode=" + inNode);

            // log.debug("UpdateWNodeHook:
            // get the obj
            Entity entity = getWorldManagerEntity(objOid);
            if (entity == null) {
                Log.warn("UpdateWNodeHook: entity not found oid=" + objOid);
                return true;
            }
            if (!(entity instanceof MVObject)) {
                throw new RuntimeException("not mvobject");
            }
            MVObject obj = (MVObject) entity;

            // get the worldnode
            WorldNode node = obj.worldNode();
            if (!(node instanceof WMWorldNode)) {
                throw new RuntimeException("not a wmwnode");
            }
            WMWorldNode wnode = (WMWorldNode) node;

            // update the object's world node
            wnode.setLoc(inNode.getLoc());
            wnode.setOrientation(inNode.getOrientation());
            wnode.setDir(inNode.getDir());
            return true;
        }
    }

    class HostInstanceHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            HostInstanceMessage message = (HostInstanceMessage) msg;
            Geometry localGeo = Geometry.maxGeometry();
            hostInstance(message.getInstanceOid(), localGeo);
            Engine.getAgent().sendBooleanResponse(message,true);
            return true;
        }
    }

    class NewRegionHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.NewRegionMessage rMsg = (WorldManagerClient.NewRegionMessage) msg;

            Region region = rMsg.getRegion();
            Boundary b = region.getBoundary();

            long instanceOid = rMsg.getInstanceOid();
            QuadTree<WMWorldNode> quadtree = quadtrees.get(instanceOid);
            if (quadtree == null) {
                log.error("NewRegionHook: unknown instanceOid="+instanceOid+
                        " boundary=" + b + " region=" + region);
                return false;
            }

            // Give region an oid for use with dir lights
            RegionConfig dirLightConfig =
                region.getConfig(LightData.DirLightRegionType);
            if (dirLightConfig != null)
                dirLightConfig.setProperty("_oid", Engine.getOIDManager().getNextOid());

            if (Log.loggingDebug)
                log.debug("NewRegionHook: boundary=" + b + " region=" + region +
                        " instanceOid=" + instanceOid);
	    quadtree.addRegion(region);
            return true;
        }
    }

    class ModifyDisplayContextHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.ModifyDisplayContextMessage dMsg
                = (WorldManagerClient.ModifyDisplayContextMessage) msg;

            Long oid = dMsg.getSubject();
            byte action = dMsg.getAction();
            String base = dMsg.getBase();
            List<DisplayContext.Submesh> submeshes = dMsg.getSubmeshes();
            String handle = dMsg.getChildDCHandle();
            DisplayContext childDC = dMsg.getChildDC();

            MVObject obj = (MVObject) getWorldManagerEntity(oid);
            if (obj == null) {
                Log.warn("ModifyDisplayContextHook: no obj: " + oid);
                return false;
            }
            obj.getLock().lock();
            try {
                if (action == WorldManagerClient.modifyDisplayContextActionReplace) {
                    if (Log.loggingDebug) {
                        log.debug("ModifyDisplayContextHook: obj " + oid + ", action=REPLACE, submeshes.size() " + submeshes.size());
                        if (submeshes.size() > 0)
                            log.debug("ModifyDisplayContextHook: first submesh " + submeshes.get(0));
                    }
                    obj.displayContext(new DisplayContext(oid, base));
                    obj.displayContext().addSubmeshes(submeshes);
                    // send out a dc update for clients
                    sendDCMessage(obj);
                } else if (action == WorldManagerClient.modifyDisplayContextActionAdd) {
                    if (Log.loggingDebug)
                        log.debug("ModifyDisplayContextHook: obj " + oid + ", action=ADD");
                    obj.displayContext().addSubmeshes(submeshes);
                    // send out a dc update for clients
                    sendDCMessage(obj);
                } else if (action == WorldManagerClient.modifyDisplayContextActionAddChild) {
                    if (Log.loggingDebug)
                        log.debug("ModifyDisplayContextHook: obj " + oid + ", action=ADD_CHILD");
                    if (handle == null) {
                        throw new MVRuntimeException("ModifyDisplayContextHook: obj=" + oid + ", handle is null");
                    }
                    obj.displayContext().addChildDC(handle, childDC);
                    // send out a dc update for clients
                    sendDCMessage(obj);
                } else if (action == WorldManagerClient.modifyDisplayContextActionRemoveChild) {
                    if (Log.loggingDebug)
                        log.debug("ModifyDisplayContextHook: obj " + oid + ", action=REMOVE_CHILD");
                    if (handle == null) {
                        throw new MVRuntimeException("ModifyDisplayContextHook: obj=" + oid + ", handle is null");
                    }
                    DisplayContext rv = obj.displayContext().removeChildDC(handle);
                    if (rv == null) {
                        Log.error("ModifyDisplayContextHook: obj=" + oid + " did not find child to remove");
                        return false;
                    }
                    if (Log.loggingDebug)
                        log.debug("ModifyDisplayContextHook: sending out detach msg for oid " + oid
			          + ", dcObjRef=" + childDC.getObjRef() + ", socket=" + handle);
                    if (childDC.getObjRef() == null) {
                        Log.error("ModifyDisplayContextHook: remove child dc, obj ref is null");
                        return false;
                    }
                    WorldManagerClient.DetachMessage detachMsg
			= new WorldManagerClient.DetachMessage(oid, childDC.getObjRef(), handle);
                    Engine.getAgent().sendBroadcast(detachMsg);
                } else if (action == WorldManagerClient.modifyDisplayContextActionRemove){
                    if (Log.loggingDebug)
                        log.debug("ModifyDisplayContextHook: obj " + oid + ", action=REMOVE");
                    obj.displayContext().removeSubmeshes(submeshes);
                    // send out a dc update for clients
                    sendDCMessage(obj);
                }
                else {
                    throw new MVRuntimeException("unknown action type");
                }
            } finally {
                obj.getLock().unlock();
            }

            // set the entity as dirty since we want to save this new property to the db
            Engine.getPersistenceManager().setDirty(obj);
            return true;
        }
    }

    class GetPluginStatusHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            LinkedHashMap<String,Serializable> status =
                new LinkedHashMap<String,Serializable>();
            status.put("plugin", getName());
            status.put("entity", EntityManager.getEntityCount());
            status.put("instance", quadtrees.size());
            Engine.getAgent().sendObjectResponse(msg,status);
            return true;
        }
    }

    class PlayerPathWMReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.PlayerPathWMReqMessage reqMsg = (WorldManagerClient.PlayerPathWMReqMessage)msg;
            long playerOid = reqMsg.getPlayerOid();
            String roomId = reqMsg.getRoomId();
            PathObject pathObject = pathObjectCache.getPathObject(roomId);
            // TBD: ??? Steve says I should assume that we have the
            // definition of the room, and if not, send the client a
            // message asking for it.
            List<MVVector> boundary = reqMsg.getBoundary();
            List<List<MVVector>> obstacles = reqMsg.getObstacles();
            float avatarWidth = reqMsg.getAvatarWidth();
            if (Log.loggingDebug)
                log.debug("PlayerPathReqWMHook.processMessage: Received a PLAYER_PATH_REQ message for player " + playerOid + 
                    " and roomId " + roomId + " with boundary " + boundary + " and obstacles " + obstacles);
            // If we have a boundary, we must have obstacles, and vice versa
            if ((boundary == null) != (obstacles == null)) {
                log.error("PlayerPathReqWMHook.processMessage: For player " + playerOid + ", received a PLAYER_PATH_REQ message for roomId " +
                    roomId + ", but boundary is " + boundary + " but obstacles is " + obstacles);
                return false;
            }
            // If we didn't find it in the cache, we must have
            // received boundary and obstacles
            if (pathObject == null && boundary == null) {
                log.error("PlayerPathReqWMHook.processMessage: For player " + playerOid + ", received a PLAYER_PATH_REQ message for roomId " +
                    roomId + ", but didn't find roomId in the cache and no no boundary or obstacles were supplied.");
                return false;
            }
            // If we have a non-null boundary, create the pathing
            // metadata and put it in the cache
            if (boundary != null) {
                pathObject = new PathObject("Player " + playerOid + ", roomId " + roomId, avatarWidth, boundary, obstacles);
                pathObjectCache.setPathObject(roomId, pathObject);
            }
            // Finally, generate and send the MobPathMessage
            PathFinderValue value = PathSearcher.findPath(playerOid, pathObject, reqMsg.getStart(), reqMsg.getDest(), avatarWidth / 2f);
            if (value == null)
                log.error("PlayerPathReqWMHook.processMessage: For player " + playerOid + ", roomId " + roomId + 
                    ", start " + reqMsg.getStart() + ", dest " + reqMsg.getDest() + ", cound not generate path!");
            else {
                List<Point> pathPoints = new LinkedList<Point>();
                for (MVVector p : value.getPath())
                    pathPoints.add(new Point(p));
                WorldManagerClient.MobPathMessage mobPathMsg = new WorldManagerClient.MobPathMessage(playerOid,  
                    System.currentTimeMillis(), "spline", reqMsg.getSpeed(), value.getTerrainString(), pathPoints);
                Engine.getAgent().sendBroadcast(mobPathMsg);
            }
            return true;
        }
    }

    public static class WorldManagerFilter extends NamespaceFilter
    {
        public WorldManagerFilter() {
            super();
        }

        public WorldManagerFilter(String pluginName) {
            super();
            setPluginName(pluginName);
        }

        public String getPluginName()
        {
            return pluginName;
        }

        public void setPluginName(String pluginName)
        {
            this.pluginName = pluginName;
        }

        public void addInstance(long instanceOid, Geometry geometry)
        {
//## check for duplicates
            List<Geometry> geoList = new ArrayList<Geometry>();
            geoList.add(geometry);
            instanceGeometry.put(instanceOid, geoList);
        }

        public void removeInstance(long instanceOid)
        {
            instanceGeometry.remove(instanceOid);
        }

        public List<Geometry> getInstance(long instanceOid)
        {
            return instanceGeometry.get(instanceOid);
        }

        public synchronized boolean matchRemaining(Message message)
        {
            Long instanceOid = null;
            Point location = null;
            MessageType type = message.getMsgType();
            Namespace namespace = null;

            if (! super.matchRemaining(message)) {
                if (type != WorldManagerClient.MSG_TYPE_NEW_REGION &&
                    type != WorldManagerClient.MSG_TYPE_PLAYER_PATH_WM_REQ)
                    return false;
            }

            // generate sub-object: match on instance-oid and location
            // load sub-object: match on instance-oid and location

            if (type == ObjectManagerClient.MSG_TYPE_GENERATE_SUB_OBJECT &&
                        message instanceof GenerateSubObjectMessage) {
                GenerateSubObjectMessage genMsg =
                    (GenerateSubObjectMessage) message;
                Template template = genMsg.getTemplate();

                String targetPlugin = (String) template.get(
                    Namespace.WM_INSTANCE,
                    WorldManagerClient.TEMPL_WORLDMGR_NAME);
                if (targetPlugin != null)  {
                    if (targetPlugin.equals(pluginName))
                        return true;
                    else
                        return false;
                }

                location = (Point) template.get(Namespace.WORLD_MANAGER,
                    WorldManagerClient.TEMPL_LOC);
                if (location == null) {
                    Log.error("WorldManagerFilter: generate msg has null loc, oid="+genMsg.getSubject());
                    return false;
                }
                instanceOid = (Long) template.get(Namespace.WORLD_MANAGER,
                    WorldManagerClient.TEMPL_INSTANCE);
                if (instanceOid == null) {
                    Log.error("WorldManagerFilter: generate msg has null instanceOid, oid="+genMsg.getSubject());
                    return false;
                }
            }
            else if (type == ObjectManagerClient.MSG_TYPE_LOAD_SUBOBJECT) {
                if (message instanceof WorldManagerClient.LoadSubObjectMessage) {
                    WorldManagerClient.LoadSubObjectMessage loadMsg =
                        (WorldManagerClient.LoadSubObjectMessage) message;
                    instanceOid = loadMsg.getInstanceOid();
                    location = loadMsg.getLocation();
                }
                else if (message instanceof ObjectManagerClient.LoadSubObjectMessage) {
                    ObjectManagerClient.LoadSubObjectMessage loadMsg =
                        (ObjectManagerClient.LoadSubObjectMessage) message;
                    instanceOid = loadMsg.getSubject();
                    namespace = loadMsg.getNamespace();
                }
            }
            else if (type == WorldManagerClient.MSG_TYPE_NEW_REGION) {
                NewRegionMessage regionMsg = (NewRegionMessage) message;
                instanceOid = regionMsg.getInstanceOid();
                List<Geometry> localGeometry = instanceGeometry.get(instanceOid);
                if (localGeometry == null)
                    return false;

                //## GAK!   Must intersect region with instance geometry
                return true;
            }
            else if (type == WorldManagerClient.MSG_TYPE_PLAYER_PATH_WM_REQ) {
                WorldManagerClient.PlayerPathWMReqMessage reqMsg = (WorldManagerClient.PlayerPathWMReqMessage) message;
                instanceOid = reqMsg.getInstanceOid();
            }
            if (instanceOid != null) {
                List<Geometry> localGeometry = instanceGeometry.get(instanceOid);
                if (localGeometry == null)
                    return false;

                // Loading instance sub-object
                if (namespace == Namespace.WM_INSTANCE && location == null)
                    return true;

                // Loading instance content, check location
                for (Geometry geometry : localGeometry) {
                    if (geometry.contains(location))
                        return true;
                }
                return false;
            }

            return false;
        }

        public synchronized boolean applyFilterUpdate(FilterUpdate update)
        {
            List<FilterUpdate.Instruction> instructions =
                update.getInstructions();

            for (FilterUpdate.Instruction instruction : instructions) {
                switch (instruction.opCode) {
                case FilterUpdate.OP_ADD:
                    if (instruction.fieldId == FIELD_INSTANCES) {
                        InstanceGeometry instanceGeo =
                            (InstanceGeometry) instruction.value;
                        if (Log.loggingDebug)
                            Log.debug("WorldManagerFilter ADD INSTANCE "+instruction.value);
                        instanceGeometry.put(instanceGeo.instanceOid,
                            instanceGeo.geometry);
                    }
                    else
                        Log.error("WorldManagerFilter: invalid fieldId " +
                            instruction.fieldId);
                    break;
                case FilterUpdate.OP_REMOVE:
                    if (instruction.fieldId == FIELD_INSTANCES) {
                        if (Log.loggingDebug)
                            Log.debug("WorldManagerFilter REMOVE INSTANCE "+instruction.value);
                        instanceGeometry.remove((Long) instruction.value);
                    }
                    else
                        Log.error("WorldManagerFilter: invalid fieldId " +
                            instruction.fieldId);
                    break;
                case FilterUpdate.OP_SET:
                    Log.error("WorldManagerFilter: OP_SET is not supported");
                    break;
                default:
                    Log.error("WorldManagerFilter: invalid opCode " +
                            instruction.opCode);
                    break;
                }
            }
            return false;
        }

        public String toString() {
            return "[WorldManagerFilter " + toStringInternal() + "]";
        }

        protected String toStringInternal() {
            return super.toStringInternal() + " pluginName="+pluginName +
                " instances=" + instanceGeometry.size();
        }

        public static final int FIELD_INSTANCES = 1;

        public static class InstanceGeometry {
            long instanceOid;
            List<Geometry> geometry;
        }

        private String pluginName;
        private Map<Long,List<Geometry>> instanceGeometry =
            new HashMap<Long,List<Geometry>>();
    }

    public static class HostInstanceFilter extends MessageTypeFilter
    {
        public HostInstanceFilter()
        {
        }

        public HostInstanceFilter(String pluginName) {
            super();
            setPluginName(pluginName);
        }

        public String getPluginName()
        {
            return pluginName;
        }

        public void setPluginName(String pluginName)
        {
            this.pluginName = pluginName;
        }

        public synchronized boolean matchRemaining(Message message)
        {
            if (message.getMsgType() ==
                    WorldManagerClient.MSG_TYPE_HOST_INSTANCE) {
                HostInstanceMessage hostMsg = (HostInstanceMessage) message;
                return pluginName.equals(hostMsg.getPluginName());
            }
            else
                return false;
        }

        private String pluginName;
    }

    public static class WorldManagerInstance extends Entity
    {
        public WorldManagerInstance()
        {
            super();
        }

        public WorldManagerInstance(long instanceOid)
        {
            super(instanceOid);
        }

        public QuadTree<WMWorldNode> getQuadTree()
        {
            return quadtree;
        }

        public void setQuadTree(QuadTree<WMWorldNode> quadtree)
        {
            this.quadtree = quadtree;
        }

        
        public Fog getGlobalFog()
        {
            if (globalFog == null) {
                InstanceClient.InstanceInfo instanceInfo =
                    InstanceClient.getInstanceInfo(getOid(),
                        InstanceClient.FLAG_FOG);
                globalFog = instanceInfo.fog;
            }
            return globalFog;
        }

        private transient QuadTree<WMWorldNode> quadtree;
        private transient Fog globalFog;
        
        private static final long serialVersionUID = 1L;
    }

    /**
     * a special namespace filter that also considers location
     * @author cedeno
     *
     */
    public static class LocationNamespaceFilter extends NamespaceFilter {
        public LocationNamespaceFilter() {
            super();
        }
        public boolean matchesRest(Message msg) {
            if (msg instanceof GenerateSubObjectMessage) {
                GenerateSubObjectMessage genObjMsg = (GenerateSubObjectMessage) msg;
                Template t = genObjMsg.getTemplate();
                Point loc = (Point) t.get(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC);
                if (loc == null) {
                    Log.warn("LocationNamespaceFilter: subobj msg has null loc");
                    return false;
                }
                boolean rv = getGeometry().contains(loc);
                if (Log.loggingDebug) {
                    Log.debug("LocationNamespaceFilter: geometry=" + getGeometry() + ", loc=" + loc + ", rv=" + rv);
                }
                return rv;
            }
            else {
                return false;
            }
        }
        
        public void setGeometry(Geometry g) {
            this.geometry = g;
        }
        public Geometry getGeometry() {
            return geometry;
        }
        
        Geometry geometry = null;
        private static final long serialVersionUID = 1L;
    }
    
    public static class WorldManagerTransferFilter extends TransferFilter {
        public WorldManagerTransferFilter() {
            super();
        }
        
        public WorldManagerTransferFilter(Geometry g) {
            super();
            addGeometry(g);
        }
        
        public void addGeometry(Geometry g) {
            geoList.add(g);
        }
        
        public boolean matchesGeometry(Point loc) {
            for (Geometry g : geoList) {
                if (g.contains(loc)) {
                    Log.debug("WorldManagerTransferFilter.matchesGeometry: matched.  loc=" + loc + ", geometry=" + g);
                    return true;
                }
            }
            Log.debug("WorldManagerTransferFilter.matchesGeometry: no geometries matched");
            return false;
        }
        
        public boolean matchesMap(Map propMap, Message msg) {
            Point loc = (Point) propMap.get(WorldManagerClient.MSG_PROP_LOC);
            if (loc == null) {
                Log.debug("WorldManagerTransferFilter.matchesMap: no loc, msg=" + msg);
                return false;
            }
            return matchesGeometry(loc);
        }
        
        List<Geometry> geoList = new LinkedList<Geometry>();
        
        private static final long serialVersionUID = 1L;
    }
    
    public static class PathObjectCache {

        public PathObject getPathObject(String roomId) {
            lock.lock();
            try {
                return cache.get(roomId);
            }
            finally {
                lock.unlock();
            }
        }
        
        public void setPathObject(String roomId, PathObject pathObject) {
            lock.lock();
            try {
                cache.put(roomId, pathObject);
            }
            finally {
                lock.unlock();
            }
        }
                
        protected Map<String, PathObject> cache = new HashMap<String, PathObject>();
        protected transient static Lock lock = LockFactory.makeLock("PathObjectCache");
    }

    /**
     * Send full display context message usually called in response to receiving
     * an update message from the proxy.
     *
     * Not implemented in base class because we don't know what clothing is being
     * handled. See MarsWorldManagerPlugin for how it does it.
     * Default implementation in @see MarsWorldManagerPlugin.sendDCMessage(Long notifyOid, MVObject obj)
     */
    protected void sendDCMessage(MVObject obj) {
    }

    /**
     * returns the current display context for the given objOid
     */
    abstract protected DisplayContext getDisplayContext(Long objOid);

    /**
     * Sends over all properties of the update object over to the notifyOid.
     * Usually includes stats like health, alive/dead,etc
     */
    protected void sendPropertyMessage(Long notifyOid, MVObject updateObj) {
    }

    protected void sendTargetedPropertyMessage(Long targetOid,
        MVObject updateObj) {
    }

    /**
     * Sends over an update of the world node
     */
    protected void sendWNodeMessage(Long notifyOid, MVObject updateObj) {
        // I think MARS over-rides this
    }
    
    protected void sendObjectSoundMessage(Long notifyOid, MVObject updateObj,
		List<SoundData> soundData) {
        WorldManagerClient.SoundMessage soundMsg;
	soundMsg = new WorldManagerClient.SoundMessage(updateObj.getOid());
	soundMsg.setSoundData(soundData);
        Engine.getAgent().sendBroadcast(soundMsg);
    }

    /** Register a custom region trigger.
    */
    public void registerRegionTrigger(String name, RegionTrigger trigger)
    {
        regionTriggers.put(name, trigger);
    }

    /** Object properties excluded from property messages.
    */
    public Set<String> getPropertyExclusions()
    {
        return propertyExclusions;
    }

    private int maxObjects = 30;
    private int maxDepth = 20;

    protected SubObjectFilter subObjectFilter;
    protected WorldManagerFilter newRegionFilter;
    protected long newRegionSub;

    // Subscription for non-structures
    protected PerceptionFilter mobFilter;
    protected long mobSubId;
    protected PerceptionFilter mobRPCFilter;
    protected long mobRPCSubId;

    // Subscription for structures
    protected PerceptionFilter structFilter;
    protected long structSubId;
    protected PerceptionFilter structRPCFilter;
    protected long structRPCSubId;
    
    // The default "hysteresis" when moving from the area controlled by one world
    // manager to 25 meters.  This prevents us from bouncing from world manager 
    // to world manager by taking just a s
    protected int defaultWorldManagerHysteresis = 20000;

    protected Map<Long,QuadTree<WMWorldNode>> quadtrees =
        new HashMap<Long,QuadTree<WMWorldNode>>();

    protected Map<String,RegionTrigger> regionTriggers =
        new HashMap<String,RegionTrigger>();

    protected Set<String> propertyExclusions = new HashSet<String>();

    protected static final Logger log = new Logger("WorldManagerPlugin");

    protected PathInfo pathInfo = null;

    protected boolean askedForPathInfo = false;

    protected Updater updater = null;

    protected static PathObjectCache pathObjectCache = new PathObjectCache();
    
    public static final String REGION_MEMBERSHIP = (String)Entity.registerTransientPropertyKey("customRegions");
}
