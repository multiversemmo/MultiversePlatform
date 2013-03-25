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

package multiverse.server.objects;

import java.util.*;
import java.util.concurrent.locks.*;

import multiverse.msgsys.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.plugins.*;
import multiverse.server.messages.PerceptionMessage;

/**
 * This class is similar to the ObjectTracker, but is used in cases
 * where there is no local/remote distinction between objects.
 * Instead, the class maintains it's own set of
 * InterpolatedWorldNodes, and an update thread to keep them current,
 * and a mapping from perceiver objects to perceived objects.  When
 * one of those objects moves, the mapping is updated, and if they
 * have moved in or out of range, the class either sends a
 * NotifyReactionRadius message, or
 */
public class ProximityTracker implements MessageDispatch {

    public ProximityTracker(Namespace namespace, long instanceOid) {
        initialize(namespace, instanceOid);
    }

    public ProximityTracker(Namespace namespace, long instanceOid,
        float hystericalMargin,
        ObjectTracker.NotifyReactionRadiusCallback notifyCallback, 
        ObjectTracker.RemoteObjectFilter remoteObjectFilter) {
        this.hystericalMargin = hystericalMargin;
        this.notifyCallback = notifyCallback;
        this.remoteObjectFilter = remoteObjectFilter;
        initialize(namespace, instanceOid);
    }

    private void initialize(Namespace namespace, long instanceOid) {
        this.namespace = namespace;
        this.instanceOid = instanceOid;
        updater = new Updater();
        Thread updaterThread = new Thread(updater);
        updaterThread.start();
    }

    public long getInstanceOid()
    {
        return instanceOid;
    }

    public void addTrackedPerceiver(Long perceiverOid, InterpolatedWorldNode wnode, Integer reactionRadius) {
        lock.lock();
        try {
            if (perceiverDataMap.containsKey(perceiverOid)) {
                // Don't add the object more than once.
                Log.error("ProximityTracker.addTrackedPerceiver: perceiverOid " + perceiverOid + 
                    " is already in the set of local objects, for ProximityTracker instance " + this);
                return;
            }
            PerceiverData perceiverData = new PerceiverData(perceiverOid, reactionRadius, wnode);
            perceiverDataMap.put(perceiverOid, perceiverData);
        }
        finally {
            lock.unlock();
        }
        if (Log.loggingDebug)
            Log.debug("ProximityTracker.addTrackedPerceiver: perceiverOid=" + perceiverOid
                + " reactionRadius=" + reactionRadius
                + " instanceOid="+instanceOid);
    }
    
    public boolean hasTrackedPerceiver(Long oid) {
        lock.lock();
        try {
            return perceiverDataMap.containsKey(oid);
        }
        finally {
            lock.unlock();
        }
    }

    public void removeTrackedPerceiver(Long perceiverOid) {
	lock.lock();
	try {
            // Iterate over perceived objects, removing our
            // perceiverOid from their oid sets.
            PerceiverData perceiverData = perceiverDataMap.get(perceiverOid);
            if (perceiverData != null) {
                if (Log.loggingDebug)
                    Log.debug("ProximityTracker.removeTrackedPerceiver: perceiverOid " + perceiverOid +
                        ", inRangeOids count " + perceiverData.inRangeOids.size());
                // Iterate over perceived objects, removing our
                // perceiverOid from their oid sets.
                for (Long perceivedOid : perceiverData.perceivedOids) {
                    PerceiverData perceivedData = perceiverDataMap.get(perceivedOid);
                    if (perceivedData != null) {
                        perceivedData.perceivedOids.remove(perceiverOid);
                        if (perceivedData.inRangeOids.contains(perceiverOid)) {
                            perceivedData.inRangeOids.remove(perceiverOid);
                            performNotification(perceiverOid, perceivedOid, false, true);
                        }
                    }
                }
                perceiverData.perceivedOids.clear();
                perceiverData.inRangeOids.clear();
                perceiverDataMap.remove(perceiverOid);
            }
            else
                Log.warn("ProximityTracker.removeTrackedPerceiver: For oid=" + perceiverOid + ", didn't find PerceiverData");
        }
	finally {
	    lock.unlock();
	}
        if (Log.loggingDebug)
            Log.debug("ProximityTracker.removeTrackedPerceiver: oid=" + perceiverOid
                + " instanceOid="+instanceOid);
    }

    public List<Long> getOidsInRadius(long perceiverOid) {
        lock.lock();
        try {
            PerceiverData perceiverData = perceiverDataMap.get(perceiverOid);
            if (perceiverData == null) {
                Log.error("ProximityTracker.getOidsInRadius: perceptionData for oid " + perceiverOid + " is null");
                return new LinkedList<Long>();
            }
            else
                return new LinkedList<Long>(perceiverData.inRangeOids);
        }
        finally {
            lock.unlock();
        }
    }
    
    public void dispatchMessage(Message message, int flags, MessageCallback callback) {
        Engine.defaultDispatchMessage(message, flags, callback);
    }

    protected boolean maybeAddPerceivedObject(
        PerceptionMessage.ObjectNote objectNote)
    {
        ObjectType objType = (ObjectType) objectNote.getObjectType();
        long perceivedOid = objectNote.getSubject();
        long perceiverOid = objectNote.getTarget();
        if (perceivedOid == perceiverOid)
            return true;
        boolean callbackNixedIt = false;
        if (remoteObjectFilter != null)
            callbackNixedIt = !remoteObjectFilter.objectShouldBeTracked(perceivedOid, objectNote);
        if (callbackNixedIt || !(objType.isMob())) {
//             if (Log.loggingDebug)
//                 Log.debug("ProximityTracker.maybeAddPerceivedObject: ignoring oid=" + perceivedOid
//                     + " objType=" + objType
//                     + " detected by " + perceiverOid
//                     + ", instanceOid=" + instanceOid);
            return false;
        }

        if (Log.loggingDebug)
            Log.debug("ProximityTracker.maybeAddPerceivedObject: oid=" + perceivedOid +
                " objType=" + objType + " detected by " + perceiverOid +
                ", instanceOid=" + instanceOid);
        lock.lock();
        try {
            PerceiverData perceiverData = perceiverDataMap.get(perceiverOid);
            if (perceiverData == null) {
                Log.error("ProximityTracker.maybeAddPerceivedObject: got perception msg with perceived obj oid=" + perceivedOid +
                          " for unknown perceiver=" + perceiverOid);
                return false;
            }
            perceiverData.perceivedOids.add(perceivedOid);
            PerceiverData perceivedData = perceiverDataMap.get(perceivedOid);
            if (perceivedData != null)
                testProximity(perceiverData, perceivedData, true, false);
        }
        finally {
            lock.unlock();
        }
        return true;
    }
    
    /**
     * Test if the perceived object has come in or out of range of the
     * perceiver object; if so, we change the inRangeOids set for the
     * perceiver, and notify the perceiver.
     */
    protected void testProximity(PerceiverData perceiverData, PerceiverData perceivedData,
        boolean interpolatePerceiver, boolean interpolatePerceived) {
        Point perceiverLoc = interpolatePerceiver ? perceiverData.wnode.getLoc() : perceiverData.lastLoc;
        Point perceivedLoc = interpolatePerceived ? perceivedData.wnode.getLoc() : perceivedData.lastLoc;
        float distance = Point.distanceTo(perceiverLoc, perceivedLoc);
        float reactionRadius = perceiverData.reactionRadius;
        long perceiverInstance = perceiverData.wnode.getInstanceOid();
        long perceivedInstance = perceivedData.wnode.getInstanceOid();
        boolean sameInstance = perceiverInstance == perceivedInstance;
        boolean inRadius = sameInstance && (distance < reactionRadius);
        boolean wasInRadius = perceiverData.inRangeOids.contains(perceivedData.perceiverOid);
//         if (Log.loggingDebug)
//             Log.debug("ProximityTracker.testProximity: perceiver " + perceiverData.perceiverOid + ", perceiverLoc = " + perceiverLoc + 
//                 ", perceived " + perceivedData.perceiverOid + ", perceivedLoc = " + perceivedLoc + 
//                 ", distance " + distance + ", reactionRadius " + reactionRadius + ", perceiverInstance " + perceiverInstance +
//                 ", perceivedInstance " + perceivedInstance + ", inRadius " + inRadius + ", wasInRadius " + wasInRadius);
        if (inRadius == wasInRadius)
            return;
        if (sameInstance && hystericalMargin != 0f) {
            if (wasInRadius)
                inRadius = distance < (reactionRadius + hystericalMargin);
            else
                inRadius = distance < (reactionRadius - hystericalMargin);
            // If they are the same after hysteresis was applied, skip.
            if (inRadius == wasInRadius)
                return;
        }
        if (inRadius) {
            perceiverData.inRangeOids.add(perceivedData.perceiverOid);
            perceivedData.inRangeOids.add(perceiverData.perceiverOid);
        }
        else {
            perceiverData.inRangeOids.remove(perceivedData.perceiverOid);
            perceivedData.inRangeOids.remove(perceiverData.perceiverOid);
        }
        performNotification(perceiverData.perceiverOid, perceivedData.perceiverOid, inRadius, wasInRadius);
    }

    protected void performNotification(long perceiverOid, long perceivedOid, boolean inRadius, boolean wasInRadius) {
        if (Log.loggingDebug)
            Log.debug("ProximityTracker.performNotification: perceiverOid " + perceiverOid + ", perceivedOid " + perceivedOid +
                ", inRadius " + inRadius + ", wasInRadius " + wasInRadius);
        if (notifyCallback != null) {
            notifyCallback.notifyReactionRadius(perceivedOid, perceiverOid, inRadius, wasInRadius);
            notifyCallback.notifyReactionRadius(perceiverOid, perceivedOid, inRadius, wasInRadius);
        }
        else {
            ObjectTracker.NotifyReactionRadiusMessage nmsg = new ObjectTracker.NotifyReactionRadiusMessage(perceivedOid, 
                perceiverOid, inRadius, wasInRadius);
            Engine.getAgent().sendBroadcast(nmsg);
            nmsg = new ObjectTracker.NotifyReactionRadiusMessage(perceiverOid, 
                perceivedOid, inRadius, wasInRadius);
            Engine.getAgent().sendBroadcast(nmsg);
        }
    }
    
    protected void updateEntity(PerceiverData perceiverData) {
	long perceiverOid = perceiverData.perceiverOid;
        lock.lock();
        try {
            for (long perceivedOid : perceiverData.perceivedOids) {
                if (perceiverOid == perceivedOid)
                    continue;
                PerceiverData perceivedData = perceiverDataMap.get(perceivedOid);
                if (perceivedData != null)
                    testProximity(perceiverData, perceivedData, false, true);
            }
	}
	finally {
	    lock.unlock();
	}
    }

    public void handlePerception(PerceptionMessage perceptionMessage)
    {
        long targetOid = perceptionMessage.getTarget();
        List<PerceptionMessage.ObjectNote> gain =
            perceptionMessage.getGainObjects();
        List<PerceptionMessage.ObjectNote> lost =
            perceptionMessage.getLostObjects();

        if (Log.loggingDebug)
            Log.debug("ProximityTracker.handlePerception: targetOid + " + targetOid + ", instanceOid=" + instanceOid + " " +
                ((gain==null)?0:gain.size()) + " gain and " +
                ((lost==null)?0:lost.size()) + " lost");

        if (gain != null)
            for (PerceptionMessage.ObjectNote note : gain)
                maybeAddPerceivedObject(note);

        if (lost != null)
            for (PerceptionMessage.ObjectNote note : lost)
                maybeRemovePerceivedObject(note.getSubject(), note, targetOid);
    }

    public void handleUpdateWorldNode(long oid, WorldManagerClient.UpdateWorldNodeMessage wnodeMsg) {
        PerceiverData perceiverData = perceiverDataMap.get(oid);
        if (perceiverData == null) {
            if (Log.loggingDebug)
                Log.debug("ProximityTracker.handleMessage: ignoring updateWNMsg for oid " + 
                    oid + " because PerceptionData for oid not found");
            return;
        }
        BasicWorldNode bwnode = wnodeMsg.getWorldNode();
        if (Log.loggingDebug)
            Log.debug("ProximityTracker.handleMessage: UpdateWnode for " + oid + ", loc " + bwnode.getLoc() + ", dir " + bwnode.getDir());
        if (perceiverData.wnode != null) {
            perceiverData.previousLoc = perceiverData.lastLoc;
            perceiverData.wnode.setDirLocOrient(bwnode);
            perceiverData.wnode.setInstanceOid(bwnode.getInstanceOid());
            perceiverData.lastLoc = perceiverData.wnode.getLoc();
        }
        else
            Log.error("ProximityTracker.handleMessage: In UpdateWorldNodeMessage for oid " + 
                oid + ", perceiverData.wnode is null!");
        updateEntity(perceiverData);
    }

    protected void maybeRemovePerceivedObject(long perceivedOid, PerceptionMessage.ObjectNote objectNote, long perceiverOid) {
        if (remoteObjectFilter != null && remoteObjectFilter.objectShouldBeTracked(perceivedOid, objectNote))
            return;
        else
            removePerceivedObject(perceiverOid, perceivedOid);
    }
    
    protected void removePerceivedObject(long perceiverOid, long perceivedOid) {
        lock.lock();
        try {
            PerceiverData perceiverData = perceiverDataMap.get(perceiverOid);
            if (perceiverData == null) {
                if (Log.loggingDebug)
                    Log.debug("ProximityTracker.removePerceivedObject: No perceiverData for oid " + perceiverOid);
                return;
            }
            perceiverData.perceivedOids.remove(perceivedOid);
            if (perceiverData.inRangeOids.contains(perceivedOid)){
                performNotification(perceiverOid, perceivedOid, true, false);
                perceiverData.inRangeOids.remove(perceivedOid);
            }
        }
	finally {
	    lock.unlock();
	}
    }
    
    class Updater implements Runnable {
        public void run() {
            while (running) {
                try {
                    update();
                } catch (MVRuntimeException e) {
                    Log.exception("ProximityTracker.Updater.run caught MVRuntimeException", e);
                } catch (Exception e) {
                    Log.exception("ProximityTracker.Updater.run caught exception", e);
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
            Log.debug("Updater.update: in update");

            List<Long> perceiverOids = null;
            lock.lock();
            try {
                perceiverOids = new ArrayList<Long>(perceiverDataMap.keySet());
            }
            finally {
                lock.unlock();
            }
            // We loop over the copied perceiverOids causing
            // interpolation to happen, and capturing the location in
            // the PerceiverData, so we can later do comparisons
            // cheaply.  Note that underlying map can change while
            // we're doing so, so we don't raise errors if it happens.
            for (long perceiverOid : perceiverOids) {
                PerceiverData perceiverData = perceiverDataMap.get(perceiverOid);
                if (perceiverData != null) {
                    perceiverData.previousLoc = perceiverData.lastLoc;
//                    long lastInterp = perceiverData.wnode.getLastInterp();
                    perceiverData.lastLoc = perceiverData.wnode.getLoc();
//                     if (Log.loggingDebug)
//                         Log.debug("Updater.update: perceiverOid " + perceiverOid + ", previousLoc " + perceiverData.previousLoc + 
//                             ", lastLoc " + perceiverData.lastLoc + ", time since interp " + (System.currentTimeMillis() - lastInterp));
                }
            }
            // Now actually do the double loop to check if inRange has
            // changed
            for (long perceiverOid : perceiverOids) {
                PerceiverData perceiverData = perceiverDataMap.get(perceiverOid);
                if (perceiverData == null)
                    continue;
                // If the perceiver hasn't moved much, no need to
                // iterate over it's perceived entities
                if (perceiverData.previousLoc != null &&
                    Point.distanceToSquared(perceiverData.previousLoc, perceiverData.lastLoc) < 100f)
                    continue;
                ArrayList<Long> perceivedOids = new ArrayList<Long>(perceiverData.perceivedOids);
                for (long perceivedOid : perceivedOids) {
                    PerceiverData perceivedData = perceiverDataMap.get(perceivedOid);
                    if (perceivedData == null)
                        continue;
                    // Invoke the testProximity method but tell it not
                    // to interpolate, but instead get its location
                    // from the PerceptionData.lastLoc members
                    testProximity(perceiverData, perceivedData, false, false);
                }
            }
        }
    }
    
    public void setRunning(boolean running) {
        this.running = running;
    }
    
    protected Namespace namespace;
    protected long instanceOid;
    
    protected float hystericalMargin = 0f;
    
    protected ObjectTracker.NotifyReactionRadiusCallback notifyCallback = null;

    protected ObjectTracker.RemoteObjectFilter remoteObjectFilter = null;
    
    protected Updater updater = null;
    
    protected Thread updaterThread = null;
    
    protected boolean running = true;

    /**
     * This maps a perceiver oid into an object containing the list of
     * perceived objects, and the list of objects in range.
     */
    protected Map<Long, PerceiverData> perceiverDataMap = new HashMap<Long, PerceiverData>();

    protected class PerceiverData {
        long perceiverOid;
        // The reaction radius to be applied
        Integer reactionRadius;
        // The Entity associated with this PerceiverData
        Entity perceiverEntity;
        // The world node for this perceiver
        InterpolatedWorldNode wnode;
        // The last interpolated location of the entity
        Point lastLoc;
        // The previous interpolated location of the entity, used to
        // detect if the entity has moved
        Point previousLoc;
        
        // The set of object oids perceived by this perceiver
        Set<Long> perceivedOids = new HashSet<Long>();
        // The set of object oids in range of this object
        Set<Long> inRangeOids = new HashSet<Long>();

        public PerceiverData(long perceiverOid, Integer reactionRadius, InterpolatedWorldNode wnode) {
            this.perceiverOid = perceiverOid;
            this.reactionRadius = reactionRadius;
            this.wnode = wnode;
            this.lastLoc = wnode.getLoc();
        }
    }
    
    //protected Map<Long, ObjectTracker.NotifyData> reactionRadiusMap = new HashMap<Long, ObjectTracker.NotifyData>();

    protected Lock lock = LockFactory.makeLock("ProximityTrackerLock");
}
