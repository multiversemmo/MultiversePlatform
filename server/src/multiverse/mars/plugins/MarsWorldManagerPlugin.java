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

package multiverse.mars.plugins;

import java.util.*;
import java.io.*;
import multiverse.msgsys.*;
import multiverse.mars.objects.*;
import multiverse.mars.util.*;
import multiverse.server.objects.*;
import multiverse.server.plugins.*;
import multiverse.server.engine.*;
import multiverse.server.math.*;
import multiverse.server.util.*;
import multiverse.server.objects.MVObject;
import multiverse.server.messages.PropertyMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedPropertyMessage;

/**
 * handles client traffic to the rest of the servers
 */
public class MarsWorldManagerPlugin extends WorldManagerPlugin {

    public MarsWorldManagerPlugin()
    {
        super();
        propertyExclusions.add(MarsObject.baseDCKey);
    }

    protected void registerHooks() {
        super.registerHooks();
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_SETWNODE_REQ,
                                 new SetWNodeReqHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATEWNODE_REQ,
                                 new UpdateWNodeReqHook());
        getHookManager().addHook(EnginePlugin.MSG_TYPE_SET_PROPERTY,
                                 new NoMovePropertyHook());
        getHookManager().addHook(EnginePlugin.MSG_TYPE_SET_PROPERTY_NONBLOCK,
                                 new NoMovePropertyHook());
	getHookManager().addHook(WorldManagerClient.MSG_TYPE_REPARENT_WNODE_REQ,
				 new ReparentWNodeReqHook());
    }

    /**
     * Override this method to change what kind of object is created
     * for the sub object hook.
     * @return MVObject representing the generated sub-object.
     */
    protected MVObject generateWorldManagerSubObject(Template template,
        Long masterOid)
    {
        // get the object type
        ObjectType objType = (ObjectType) template.get(Namespace.WORLD_MANAGER,
           WorldManagerClient.TEMPL_OBJECT_TYPE);
        MVObject obj = null;

        // generate the subobject
        if (Log.loggingDebug) {
            Log.debug("MarsWorldManagerPlugin: generateWorldManagerSubObject: objectType=" + objType + ", template=" + template);
        }
        if (objType == null) {
            Log.warn("MarsWorldManagerPlugin: generateSubObject: no object type, using structure");
            obj = new MarsObject(masterOid);
            obj.setType(ObjectTypes.structure);
        }
        else if (objType == ObjectTypes.mob || objType == ObjectTypes.player) {
            obj = new MarsMob(masterOid);
            obj.setType(objType);
        }
        else if (objType == ObjectTypes.structure) {
            obj = new MarsObject(masterOid);
            obj.setType(ObjectTypes.structure);
        }
        else if (objType == ObjectTypes.light) {
            Light l = new Light(masterOid);
            LightData ld = (LightData)template.get(Namespace.WORLD_MANAGER,
                Light.LightDataPropertyKey);
            l.setLightData(ld);
            obj = l;
        }
        else {
            obj = new MarsObject(masterOid);
	    obj.setType(objType);
        }

	Map<String, Serializable> props = template.getSubMap(Namespace.WORLD_MANAGER);
	if (props == null) {
	    Log.warn("MarsWorldManagerPlugin.generateSubObject: no props in ns "
		     + Namespace.WORLD_MANAGER);
	    return null;
	}

	// copy properties from template to object
	for (Map.Entry<String, Serializable> entry : props.entrySet()) {
	    String key = entry.getKey();
	    Serializable value = entry.getValue();
	    if (!key.startsWith(":")) {
		obj.setProperty(key, value);
	    }
	}
	
	if (obj.isUser() || obj.isMob() || obj.isStructure()) {
	    MarsObject marsObj = (MarsObject)obj;

	    // set the base display context for the object
	    DisplayContext dc = (DisplayContext)props.get(WorldManagerClient.TEMPL_DISPLAY_CONTEXT);
	    if (dc == null) {
                if (objType != ObjectTypes.terrainDecal)
                    Log.warn("MarsWorldManagerPlugin.generateSubObject: obj has no display context, oid="+masterOid);
	    }
	    else {
	        dc = (DisplayContext)dc.clone();
	        dc.setObjRef(marsObj.getOid());
	        marsObj.baseDC(dc);
	        marsObj.displayContext(dc);
	    }
	}

        return obj;
    }
    
    class SetWNodeReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.SetWorldNodeReqMessage setNodeMsg =
                (WorldManagerClient.SetWorldNodeReqMessage) msg;

            BasicWorldNode wnode =
                (BasicWorldNode)setNodeMsg.getWorldNode();
            Long oid = setNodeMsg.getSubject();
	    Entity entity = getWorldManagerEntity(oid);

            boolean rv = false;

            do {
                if (entity == null) {
                    log.warn("SetWNodeReqHook: cannot find entity oid="+oid);
                    break;
                }

                if (entity instanceof MVObject) {
                    MVObject obj = (MVObject) entity;
                    if (obj.worldNode().isSpawned()) {
                        log.warn("SetWNodeReqHook: cannot set worldnode, object currently spawned oid="+oid);
                        break;
                    }

                    // If the new world node does not have orientation then
                    // keep the existing orientation.
                    Quaternion currentOrient = null;
                    if (obj.worldNode() != null)
                        currentOrient = obj.worldNode().getOrientation();
                    WMWorldNode newWnode = new WMWorldNode(wnode);
                    if (newWnode.getOrientation() == null)
                        newWnode.setOrientation(currentOrient);
                    newWnode.setPerceptionRadius(
                        ((WMWorldNode)obj.worldNode()).getPerceptionRadius());
                    if (Log.loggingDebug)
                        log.debug("SetWNodeReqHook: obj=" + obj +
                                ", newWnode=" + newWnode +
                                ", perceiver=" + obj.perceiver());

                    obj.worldNode(newWnode);
                    newWnode.setObject(obj);
                    if ((setNodeMsg.getFlags() & WorldManagerClient.SAVE_NOW) != 0)
                        Engine.getPersistenceManager().persistEntity(obj);
                    else
                        Engine.getPersistenceManager().setDirty(obj);

                    if (Log.loggingDebug)
                        log.debug("SetWNodeReqHook: done oid=" + oid +
                                  ", wnode=" + obj.worldNode());

                    rv = true;
                }
                else {
                    log.debug("SetWNodeReqHook: not mvobject oid="+oid);
                }
                break;
            } while (false);

            Engine.getAgent().sendBooleanResponse(msg, rv);

            return true;
        }
    }

    /**
     * mob is requesting to update its world node (it moved or is moving now)
     */
    class UpdateWNodeReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.UpdateWorldNodeReqMessage updateMsg = (WorldManagerClient.UpdateWorldNodeReqMessage) msg;
            BasicWorldNode wnode = updateMsg.getWorldNode();
            Long masterOid = updateMsg.getSubject();
            Entity entity = getWorldManagerEntity(masterOid);
            
            if (entity == null) {
                log.error("UpdateWNodeReqHook: could not find entity, masterOid=" + masterOid);
                return false;
            }
//            log.debug("UpdateWNodeReqHook: entity=" + entity);

            if (!(entity instanceof MVObject)) {
                log.error("UpdateWNodeReqHook: entity is not an obj: " + entity);
                return false;
            }

            // FIXME/TODO: check if already spawned
            // ((MVObject)entity).getWorldNode().isSpawned();

            // loc from locatable
            // orient from wnode

            // get the object's current world node
            MVObject obj = (MVObject) entity;
            InterpolatedWorldNode curWnode = (InterpolatedWorldNode) obj.worldNode();

            // check for restrictions
            boolean nomove = obj.getBooleanProperty(WorldManagerClient.WORLD_PROP_NOMOVE);
            boolean noturn = obj.getBooleanProperty(WorldManagerClient.WORLD_PROP_NOTURN);
            boolean sendCorrection = false;

	    // Get the current location before setting the direction.
	    // This will complete the interpolation based on the old
	    // direction.
	    Point oldLoc = curWnode.getLoc();

            if (Log.loggingDebug)
                Log.debug("UpdateWNodeReqHook: oldLoc="+oldLoc +
                    " nomove="+nomove+ " noturn="+noturn);

            BasicWorldNode newNode = new BasicWorldNode(curWnode);

            // update the object's current world node
            Quaternion orient = wnode.getOrientation();
            if (orient != null) {
                if (noturn) {
                    if (!curWnode.getOrientation().equals(orient)) {
                        orient = curWnode.getOrientation();
                        newNode.setOrientation(orient);
                        sendCorrection = true;
                    }
                }
                else {
                    if (updateMsg.getOverride()) {
                        newNode.setOrientation(orient);
                        sendCorrection = true;
                    }
                    curWnode.setOrientation(orient);
                }
            }

            MVVector dir = wnode.getDir();
            if (dir != null) {
                if (nomove) {
                    if (!dir.isZero()) {
                        dir = new MVVector(0,0,0);
                        newNode.setDir(dir);
                        sendCorrection = true;
                    }
                }
                curWnode.setDir(dir);
            }

            Point newLoc = wnode.getLoc();
            if (Log.loggingDebug)
                Log.debug("UpdateWNodeReqHook: masterOid " + masterOid + ", oldLoc " + oldLoc + 
                    ", newLoc " + newLoc + ", override " + updateMsg.getOverride());
            if (newLoc != null) {
                if (nomove) {
                    if (Point.distanceTo(oldLoc, newLoc) > 0) {
                        newLoc = oldLoc;
                        newNode.setLoc(newLoc);
                        sendCorrection = true;
                    }
                }
		else if (!Points.isClose(oldLoc, newLoc, World.getLocTolerance())
			 && !updateMsg.getOverride()) {
		    // 05/23/07 Stryker Correct with the last location
		    // we got from the client, rather than the last
		    // interpolated location, since the last
		    // interpolated location may end up transporting
		    // the player inside a collision volume.
		    newLoc = curWnode.getRawLoc();
		    newNode.setLoc(newLoc);
                    sendCorrection = true;
		}
                else {
		    if (updateMsg.getOverride()) {
                        newNode.setLoc(newLoc);
			sendCorrection = true;
		    }
                }
            }

            if (Log.loggingDebug)
                log.debug("UpdateWNodeReqHook: set world node, entity="
                          + entity + ", new wnode=" + curWnode);

            if (sendCorrection) {
                if (Log.loggingDebug)
                    log.debug("UpdateWNodeReqHook: sending world node correction " + newNode);
                WorldManagerClient.correctWorldNode(masterOid, newNode);
            }

            if (!newLoc.equals(oldLoc)) {
                if (updateMsg.getPreMessage() != null) {
                    Engine.getAgent().sendBroadcast(updateMsg.getPreMessage());
                }
		curWnode.setLoc(newLoc);
                if (updateMsg.getPostMessage() != null) {
                    Engine.getAgent().sendBroadcast(updateMsg.getPostMessage());
                }
	    }

            // make a wnodeupdatemsg - make a copy of the curWnode
            // but as basic world node since the WMWNode currently
            // has some serialization problems
            BasicWorldNode updateNode = new BasicWorldNode(curWnode);
            WorldManagerClient.UpdateWorldNodeMessage upMsg = new WorldManagerClient.UpdateWorldNodeMessage(
                    masterOid, updateNode);
            Engine.getAgent().sendBroadcast(upMsg);
            return true;
        }
    }

    class ReparentWNodeReqHook implements Hook {
	public boolean processMessage(Message msg, int flags) {
	    WorldManagerClient.ReparentWNodeReqMessage rMsg = (WorldManagerClient.ReparentWNodeReqMessage) msg;

	    Long oid = rMsg.getSubject();
	    Long parentOid = rMsg.getParentOid();

            if (Log.loggingDebug)
                log.debug("ReparentWNodeReqHook: oid=" + oid + " parent=" + parentOid);
            // Entity entity = Entity.getEntity(oid);
            Entity entity = getWorldManagerEntity(oid);
	    InterpolatedWorldNode parentWnode = null;

            if (entity == null) {
                log.error("ReparentWNodeReqHook: could not find entity: " + oid);
                return false;
            }
            if (!(entity instanceof MVObject)) {
                log.error("ReparentWNodeReqHook: entity is not an obj: " + entity);
                return false;
            }

            // get the object's current world node
            MVObject obj = (MVObject) entity;
            InterpolatedWorldNode wnode = (InterpolatedWorldNode) obj.worldNode();

	    if (parentOid != null) {
		// Entity parent = Entity.getEntity(parentOid);
		Entity parent = getWorldManagerEntity(parentOid);
		if (parent == null) {
		    log.error("ReparentWNodeReqHook: could not find parent: " + parent);
		    return false;
		}
		if (!(parent instanceof MVObject)) {
		    log.error("ReparentWNodeReqHook: parent is not an obj: " + parent);
		    return false;
		}
		MVObject parentObj = (MVObject) parent;
		parentWnode = (InterpolatedWorldNode) parentObj.worldNode();
	    }

	    InterpolatedWorldNode oldParentWnode = (InterpolatedWorldNode)wnode.getParent();
	    if (oldParentWnode != null) {
		oldParentWnode.removeChild(wnode);
	    }
	    wnode.setParent(parentWnode);
	    if (parentWnode != null) {
		parentWnode.addChild(wnode);
		wnode.setLoc(parentWnode.getLoc());
		wnode.setDir(parentWnode.getDir());
		wnode.setOrientation(parentWnode.getOrientation());
	    }

	    BasicWorldNode bwnode = new BasicWorldNode(wnode);
	    WorldManagerClient.UpdateWorldNodeMessage updateMsg =
		new WorldManagerClient.UpdateWorldNodeMessage(oid, bwnode);
	    Engine.getAgent().sendBroadcast(updateMsg);
	    WorldManagerClient.WorldNodeCorrectMessage correctMsg =
		new WorldManagerClient.WorldNodeCorrectMessage(oid, bwnode);
	    Engine.getAgent().sendBroadcast(correctMsg);
	    return true;
	}
    }

    class NoMovePropertyHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            EnginePlugin.SetPropertyMessage rMsg = (EnginePlugin.SetPropertyMessage) msg;

            Long oid = rMsg.getSubject();
            if (rMsg.containsKey(WorldManagerClient.WORLD_PROP_NOMOVE)) {
                Boolean val = (Boolean)rMsg.getProperty(WorldManagerClient.WORLD_PROP_NOMOVE);
                // MVObject obj = (MVObject)Entity.getEntity(oid);
                MVObject obj = (MVObject)getWorldManagerEntity(oid);
                if (val == true) {
                    log.debug("NoMovePropertyHook: stopping object");

                    WorldManagerClient.MobPathCorrectionMessage correction = new 
                        WorldManagerClient.MobPathCorrectionMessage(oid,  System.currentTimeMillis(), "linear", 0, 
                            "", new LinkedList<Point>());
                    Engine.getAgent().sendBroadcast(correction);

                    WorldManagerClient.MobPathMessage cancellation = new 
                        WorldManagerClient.MobPathMessage(oid,  System.currentTimeMillis(), "linear", 0, 
                            "", new LinkedList<Point>());
                    Engine.getAgent().sendBroadcast(cancellation);
                    
                    BasicWorldNode wnode = obj.baseWorldNode();
                    wnode.setDir(new MVVector(0,0,0));

                    // make a wnodeupdatemsg - make a copy of the curWnode
                    // but as basic world node since the WMWNode currently
                    // has some serialization problems
                    WorldManagerClient.UpdateWorldNodeMessage upMsg =
                        new WorldManagerClient.UpdateWorldNodeMessage(oid, wnode);
                    Engine.getAgent().sendBroadcast(upMsg);
                }
            }
            return true;
        }
    }

    // start running at 2000.0mm/sec
    public static Float defaultRunThreshold = 2000.0f;
    
    /**
     * creates a DisplayContextMessage with notifyOid set as its MSG_OID.
     */
    protected void sendDCMessage(MVObject obj) {
        if (Log.loggingDebug)
            log.debug("sendDCMessage: obj=" + obj);

        if (!(obj instanceof MarsObject)) {
//            log.error("sendDCMessage: not a marsobj: " + obj);
            return;
        }

        DisplayContext dc =
        // MarsDisplayContext.createFullDisplayContext((MarsObject)obj);
        obj.displayContext();

        if (dc == null) {
            log.warn("sendDCMessage: obj has no dc: " + obj.getOid());
            return;
        }

        Message dcMsg = new WorldManagerClient.DisplayContextMessage(obj.getMasterOid(), dc);
        Engine.getAgent().sendBroadcast(dcMsg);
    }

    /**
     * sends over health, int, str, etc.
     */
    protected void sendPropertyMessage(Long notifyOid, MVObject updateObj) {
        if (! (updateObj instanceof MarsObject)) {
            if (Log.loggingDebug)
                log.debug("MarsWorldManagerPlugin.sendPropertyMessage: skipping, obj is not marsobject: "
		          + updateObj);
            return;
        }
        MarsObject mObj = (MarsObject) updateObj;
	Long updateOid = updateObj.getMasterOid();

	PropertyMessage propMessage = new PropertyMessage(updateOid, notifyOid);
	for (String key : mObj.getPropertyMap().keySet()) {
            if (propertyExclusions.contains(key))
                continue;
            propMessage.setProperty((String)key, mObj.getProperty(key));
	}

        // send the message
	Log.debug("MarsWorldManagerPlugin.sendPropertyMessage: sending property message for obj=" + updateObj + " to=" + notifyOid + " msg=" + propMessage);
        Engine.getAgent().sendBroadcast(propMessage);
    }

    protected void sendTargetedPropertyMessage(Long targetOid,
        MVObject updateObj)
    {
        if (! (updateObj instanceof MarsObject)) {
            if (Log.loggingDebug)
                log.debug("MarsWorldManagerPlugin.sendTargetedPropertyMessage: skipping, obj is not marsobject: "
		          + updateObj);
            return;
        }
        MarsObject mObj = (MarsObject) updateObj;
	Long updateOid = updateObj.getMasterOid();

	TargetedPropertyMessage propMessage =
            new TargetedPropertyMessage(targetOid, updateOid);
	for (String key : mObj.getPropertyMap().keySet()) {
            if (propertyExclusions.contains(key))
                continue;
            propMessage.setProperty((String)key, mObj.getProperty(key));
	}

        // send the message
	Log.debug("MarsWorldManagerPlugin.sendTargetedPropertyMessage: subject=" + updateObj + " target=" + targetOid + " msg=" + propMessage);
        Engine.getAgent().sendBroadcast(propMessage);
    }

    /**
     * gets the current display context - used in the base world manager plugin
     * when it needs to send the display context to the proxy - this gets called
     * by the wmgr via the proxy upon logging in
     */
    protected DisplayContext getDisplayContext(Long objOid) {
	Entity entity = getWorldManagerEntity(objOid);
        if (entity == null) {
            return null;
        }
        if (!(entity instanceof MVObject)) {
            return null;
        }
        MVObject obj = (MVObject) entity;
        if (!(obj instanceof MarsObject)) {
            // it's base object type, just send over its stored
            // displaycontext
            return obj.displayContext();
        }

        DisplayContext dc = MarsDisplayContext.createFullDisplayContext((MarsObject) obj);
        if (Log.loggingDebug)
            log.debug("MarsWorldManagerPlugin: get dc = " + dc);
        return dc;
    }
}
