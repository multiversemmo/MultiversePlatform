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
import multiverse.server.messages.PerceptionFilter;
import multiverse.server.messages.PerceptionTrigger;

public class ObjectTracker implements MessageCallback, MessageDispatch
{
    public ObjectTracker(Namespace namespace, long instanceOid,
        EntityWithWorldNodeFactory entityFactory,
        Collection<ObjectType> subjectTypes)
    {
        initialize(namespace, instanceOid, entityFactory, subjectTypes);
    }

    public ObjectTracker(Namespace namespace, long instanceOid,
        EntityWithWorldNodeFactory entityFactory, 
        float hystericalMargin, NotifyReactionRadiusCallback notifyCallback,
        RemoteObjectFilter remoteObjectFilter)
    {
        this.hystericalMargin = hystericalMargin;
        this.notifyCallback = notifyCallback;
        this.remoteObjectFilter = remoteObjectFilter;
        initialize(namespace, instanceOid, entityFactory, null);
    }

    private void initialize(Namespace namespace, long instanceOid,
        EntityWithWorldNodeFactory entityFactory,
        Collection<ObjectType> subjectTypes)
    {
        this.namespace = namespace;
        this.instanceOid = instanceOid;
        this.entityFactory = entityFactory;
        perceptionFilter = new PerceptionFilter();
        perceptionFilter.addType(WorldManagerClient.MSG_TYPE_PERCEPTION);
        perceptionFilter.addType(WorldManagerClient.MSG_TYPE_UPDATEWNODE);
        perceptionFilter.setMatchAllSubjects(true);
        perceptionFilter.setSubjectObjectTypes(subjectTypes);
        PerceptionTrigger perceptionTrigger = new PerceptionTrigger();
        perceptionSubId = Engine.getAgent().createSubscription(
            perceptionFilter, this, MessageAgent.NO_FLAGS, perceptionTrigger);
    }

    public long getInstanceOid()
    {
        return instanceOid;
    }

    public void addLocalObject(Long oid, Integer reactionRadius) {
        lock.lock();
        try {
            // Don't add the object more than once.
            if (localObjects.contains(oid)) {
                Log.error("ObjectTracker.addLocalObject: oid " + oid + 
                    " is already in the set of local objects, for ObjectTracker instance " + this);
                return;
            }
            if (reactionRadius != null)
                reactionRadiusMap.put(oid, new NotifyData(reactionRadius));
            localObjects.add(oid);
            if (perceptionFilter.addTarget(oid)) {
                FilterUpdate filterUpdate = new FilterUpdate(1);
                filterUpdate.addFieldValue(PerceptionFilter.FIELD_TARGETS,
                        new Long(oid));
                Engine.getAgent().applyFilterUpdate(perceptionSubId,
                        filterUpdate);
            }
        }
        finally {
            lock.unlock();
        }
        if (Log.loggingDebug)
            Log.debug("ObjectTracker.addLocalObject: oid=" + oid
                + " reactionRadius=" + reactionRadius
                + " instanceOid="+instanceOid);
    }
    
    public boolean hasLocalObject(Long oid) {
        lock.lock();
        try {
            return localObjects.contains(oid);
        }
        finally {
            lock.unlock();
        }
    }
    
    public void addReactionRadius(Long oid, Integer reactionRadius) {
        if (Log.loggingDebug)
            Log.debug("ObjectTracker.addReactionRadius: oid=" + oid +
                " reactionRadius=" + reactionRadius +
                " instanceOid="+instanceOid);
        if (reactionRadius != null) {
            lock.lock();
            try {
                reactionRadiusMap.put(oid, new NotifyData(reactionRadius));
            }
            finally {
                lock.unlock();
            }
        }
    }

    public boolean removeLocalObject(Long oid) {
	lock.lock();
	try {
            localObjects.remove(oid);
            if (perceptionFilter.removeTarget(oid)) {
                FilterUpdate filterUpdate = new FilterUpdate(1);
                filterUpdate.removeFieldValue(PerceptionFilter.FIELD_TARGETS,
                    new Long(oid));
                Engine.getAgent().applyFilterUpdate(perceptionSubId,
                    filterUpdate);
            }

	    reactionRadiusMap.remove(oid);
            for (Long objOid : trackMap.keySet()) {
                while(removeRemoteObject(objOid, oid))
                    ;
	    }
	    for(Map.Entry<Long, NotifyData> entry : reactionRadiusMap.entrySet()) {
                NotifyData notifyData = entry.getValue();
                notifyData.removeOidInRadius(oid);
	    }
	}
	finally {
	    lock.unlock();
	}
        if (Log.loggingDebug)
            Log.debug("ObjectTracker.removeLocalObject: oid=" + oid
                + " instanceOid="+instanceOid);
        return true;
    }

    protected boolean maybeAddRemoteObject(
        PerceptionMessage.ObjectNote objectNote)
    {
        ObjectType objType = (ObjectType) objectNote.getObjectType();
        long oid = objectNote.getSubject();
        long trackerOid = objectNote.getTarget();
        boolean callbackNixedIt = false;
        if (remoteObjectFilter != null)
            callbackNixedIt = !remoteObjectFilter.objectShouldBeTracked(oid, objectNote);
        if (callbackNixedIt || !(objType.isMob())) {
            if (Log.loggingDebug)
                Log.debug("ObjectTracker.maybeAddRemoteObject: ignoring oid=" + oid
                    + " objType=" + objType
                    + " detected by " + trackerOid
                    + ", instanceOid=" + instanceOid);
            return false;
        }

        if (Log.loggingDebug)
            Log.debug("ObjectTracker.maybeAddRemoteObject: oid=" + oid +
                " objType=" + objType + " detected by " + trackerOid +
                ", instanceOid=" + instanceOid);
        lock.lock();
        try {
            // if the object is local, do nothing
            if (localObjects.contains(oid))
                return false;
            Entry tracker = trackMap.get(oid);
            if (tracker == null) {
                tracker = new Entry(oid);
                trackMap.put(oid, tracker);
                tracker.activate(objType);
            }
            tracker.add(trackerOid);
        }
        finally {
            lock.unlock();
        }
        return true;
    }
    
    public List<Long> getOidsInRadius(long oid) {
        lock.lock();
        try {
            NotifyData nd = reactionRadiusMap.get(oid);
            if (nd != null)
                return nd.getOidsInRadius();
            else
                return new LinkedList<Long>();
        }
        finally {
            lock.unlock();
        }
    }
    
    protected boolean removeRemoteObject(Long oid, Long trackerOid) {
	lock.lock();
	try {
	    // if the object is local, do nothing
	    if (localObjects.contains(oid))
		return false;
	    Entry tracker = trackMap.get(oid);
	    if (tracker == null) {
		return false;
	    }
	    boolean rv = tracker.remove(trackerOid);
	    if (tracker.isEmpty()) {
		tracker.deactivate();
		trackMap.remove(oid);
	    }
	    return rv;
	}
	finally {
	    lock.unlock();
	}
    }

    public void updateEntity(EntityWithWorldNode ewwn) {
	lock.lock();
	Map<Long, NotifyData> mapCopy;
	try {
	    mapCopy = new HashMap<Long, NotifyData> (reactionRadiusMap);
	}
	finally {
	    lock.unlock();
	}
        InterpolatedWorldNode wnode = ewwn.getWorldNode();
        Entity entity = ewwn.getEntity();
	Long oid = entity.getOid();
	for(Map.Entry<Long, NotifyData> entry : mapCopy.entrySet()) {
	    Long notifyOid = entry.getKey();
// 	    if (Log.loggingDebug)
// 		Log.debug("ObjectTracker.updateEntity: oid " + oid + ", notifyOid " + notifyOid + ", wnode " + wnode);
	    if (oid.equals(notifyOid))
		continue;
	    NotifyData notifyData = entry.getValue();
	    EntityWithWorldNode perceiver = (EntityWithWorldNode)EntityManager.getEntityByNamespace(notifyOid, namespace);
// 	    if (Log.loggingDebug)
// 		Log.debug("ObjectTracker.updateEntity: notifyOid is " + notifyOid + ", oid is " + oid + ", perceiver " + perceiver);
	    if (perceiver != null) {
                InterpolatedWorldNode perceiverNode = perceiver.getWorldNode();
                Point perceiverLocation = perceiverNode.getLoc();
                float distance = Point.distanceTo(perceiverLocation, wnode.getLoc());
                float reactionRadius = notifyData.getReactionRadius();
                boolean inRadius = (distance < reactionRadius);
                boolean wasInRadius = notifyData.isOidInRadius(oid);
                if (inRadius == wasInRadius)
                    continue;
                if (hystericalMargin != 0f) {
                    if (wasInRadius)
                        inRadius = distance < (reactionRadius + hystericalMargin);
                    else
                        inRadius = distance < (reactionRadius - hystericalMargin);
                    // If they are the same after hysteresis was applied, skip.
                    if (inRadius == wasInRadius)
                        continue;
                }
                if (inRadius)
                    notifyData.addOidInRadius(oid);
                else
                    notifyData.removeOidInRadius(oid);
                // 	    if (Log.loggingDebug)
                // 		Log.debug("ObjectTracker.updateEntity: inRadius " + inRadius + ", wasInRadius " + wasInRadius);
                if (notifyCallback != null)
                    notifyCallback.notifyReactionRadius(notifyOid, oid, inRadius, wasInRadius);
                else {
                    NotifyReactionRadiusMessage nmsg = new NotifyReactionRadiusMessage(notifyOid,
                        oid, inRadius, wasInRadius);
                    Engine.getAgent().sendBroadcast(nmsg);
                }
            }
            else
                Log.warn("ObjectTracker.updateEntity: No perceiver for oid " + notifyOid + " in namespace " + namespace);
	}
    }

    public void dispatchMessage(Message message, int flags, MessageCallback callback) {
        Engine.defaultDispatchMessage(message, flags, callback);
    }

    protected void handlePerception(PerceptionMessage perceptionMessage)
    {
        long targetOid = perceptionMessage.getTarget();
        List<PerceptionMessage.ObjectNote> gain =
            perceptionMessage.getGainObjects();
        List<PerceptionMessage.ObjectNote> lost =
            perceptionMessage.getLostObjects();

        if (Log.loggingDebug)
            Log.debug("ObjectTracker.handlePerception: start instanceOid=" + instanceOid + " " +
                ((gain==null)?0:gain.size()) + " gain and " +
                ((lost==null)?0:lost.size()) + " lost");

        if (gain != null)
            for (PerceptionMessage.ObjectNote note : gain)
                maybeAddRemoteObject(note);

        if (lost != null)
            for (PerceptionMessage.ObjectNote note : lost)
                maybeRemoveRemoteObject(note.getSubject(), note, targetOid);
    }

    protected void maybeRemoveRemoteObject(long subjectOid, PerceptionMessage.ObjectNote objectNote, long targetOid) {
        if (remoteObjectFilter != null && remoteObjectFilter.objectShouldBeTracked(subjectOid, objectNote))
            return;
        else
            removeRemoteObject(subjectOid, targetOid);
    }
    
    public void handleMessage(Message msg, int flags) {
        if (msg instanceof PerceptionMessage) {
            handlePerception((PerceptionMessage)msg);
        }
        else if (msg instanceof WorldManagerClient.UpdateWorldNodeMessage) {
	    WorldManagerClient.UpdateWorldNodeMessage wnodeMsg =
		(WorldManagerClient.UpdateWorldNodeMessage)msg;
	    Long oid = wnodeMsg.getSubject();
	    EntityWithWorldNode obj = (EntityWithWorldNode)EntityManager.getEntityByNamespace(oid, namespace);
	    // If we didn't find an entity, ignore this message
            if (obj == null) {
                if (Log.loggingDebug)
                    Log.debug("ObjectTracker.handleMessage: ignoring updateWNMsg for oid " + oid + " because EntityWithWorldNode for oid not found");
                return;
            }
            BasicWorldNode bwnode = wnodeMsg.getWorldNode();
            InterpolatedWorldNode iwnode = obj.getWorldNode();
            if (iwnode != null)
                obj.setDirLocOrient(bwnode);
            else {
                iwnode = new InterpolatedWorldNode(bwnode);
                obj.setWorldNode(iwnode);
            }
            updateEntity(obj);
	}
        else
            Log.error("ObjectTracker.handleMessage: unknown message type="+
                   msg.getMsgType()+" class="+msg.getClass().getName());
    }

    public static class NotifyReactionRadiusMessage extends TargetMessage {
        public NotifyReactionRadiusMessage()
        {
        }

        public NotifyReactionRadiusMessage(Long notifyOid, Long subjectOid,
                boolean inRadius, boolean wasInRadius)
        {
            super(MSG_TYPE_NOTIFY_REACTION_RADIUS, notifyOid, subjectOid);
            this.inRadius = inRadius;
            this.wasInRadius = wasInRadius;
        }
        protected boolean inRadius;
        protected boolean wasInRadius;
        
        public void setInRadius(boolean value) {
            inRadius = value;
        }
        public boolean getInRadius() {
            return inRadius;
        }

        public void setWasInRadius(boolean value) {
            wasInRadius = value;
        }
        public boolean getWasInRadius() {
            return wasInRadius;
        }

        private static final long serialVersionUID = 1L;
    }

    protected class Entry extends LinkedList<Long> {
	public Entry(Long oid) {
	    this.oid = oid;
	}
	protected Long oid;

	public void activate(ObjectType objType) {
	    EntityWithWorldNode obj = entityFactory.createEntity(oid, null, null);
            Entity entity = obj.getEntity();
	    entity.setType(objType);
            if (Log.loggingDebug)
                Log.debug("ObjectTracker.Entry.activate: obj=" + obj + " objType=" + objType);
	    EntityManager.registerEntityByNamespace((Entity)obj, namespace);
	}

	public void deactivate() {
            if (Log.loggingDebug)
                Log.debug("ObjectTracker.Entry.deactivate: oid=" + oid +
                    " instanceOid="+instanceOid);
            EntityManager.removeEntityByNamespace(oid, namespace);
	}

	protected long sub;

        private static final long serialVersionUID = 1L;
    }

    protected class NotifyData {
        protected Integer reactionRadius;
        protected Set<Long> oidsInRadius;
        
        NotifyData(Integer reactionRadius) {
            this.reactionRadius = reactionRadius;
            oidsInRadius = new HashSet<Long>();
        }
        
        Integer getReactionRadius() {
            return reactionRadius;
        }

        // Returns true if the oid is in the set
        boolean isOidInRadius(Long oid) {
            return oidsInRadius.contains(oid);
        }

        // Returns false if the oid was already in the set
        boolean addOidInRadius(Long oid) {
            return oidsInRadius.add(oid);
        }
    
        // Returns true if the oid was formerly in the set
        boolean removeOidInRadius(Long oid) {
            return oidsInRadius.remove(oid);
        }

        List<Long> getOidsInRadius() {
            return new LinkedList<Long>(oidsInRadius);
        }
        
    }

    public interface NotifyReactionRadiusCallback {
        public void notifyReactionRadius(long targetOid, long subjectOid, boolean inRadius, boolean wasInRadius);
    }
    
    /**
     * A filter callback used to determine if a remote object should be tracked.
     */
    public interface RemoteObjectFilter {
        public boolean objectShouldBeTracked(long objectOid, PerceptionMessage.ObjectNote note);
    }
    

    protected Namespace namespace;
    protected long instanceOid;
    
    protected EntityWithWorldNodeFactory entityFactory;
    
    protected float hystericalMargin = 0f;
    
    protected NotifyReactionRadiusCallback notifyCallback = null;

    protected RemoteObjectFilter remoteObjectFilter = null;
    
    /**
     * Local objects are perceivers; remote objects are the objects that 
     * are perceived by perceivers
     */
    protected Set<Long> localObjects = new HashSet<Long>();

    protected Map<Long, Entry> trackMap = new HashMap<Long, Entry>();

    protected Map<Long, NotifyData> reactionRadiusMap = new HashMap<Long, NotifyData>();

    protected PerceptionFilter perceptionFilter;
    protected long perceptionSubId;

    protected Lock lock = LockFactory.makeLock("ObjectTrackerLock");

    public static final MessageType MSG_TYPE_NOTIFY_REACTION_RADIUS = MessageType.intern("mv.NOTIFY_REACTION_RADIUS");
}
