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

import multiverse.server.math.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;

import java.util.concurrent.locks.*;
import java.util.*;
import java.io.*;
import java.beans.*;

/**
 * MVObject is a properties object, it does not contain 'logic' when you set a
 * property, there are no callbacks which will then notify other players
 */
public class MVObject extends Entity {
    public MVObject() {
        super();
        setNamespace(Namespace.WORLD_MANAGER);
        init();
    }

    public MVObject(String name) {
        super(name);
        setNamespace(Namespace.WORLD_MANAGER);
        init();
    }

    public MVObject(Long oid) {
        super(oid);
        setNamespace(Namespace.WORLD_MANAGER);
        init();
    }

    private void init() {
        // set the object create hook
        MVObjectCreateHook hook = getObjCreateHook();
        if (hook != null) {
            hook.objectCreateHook(this);
        }

        // set the default permission if one exists
//        PermissionFactory factory = World.getDefaultPermission();
//        if (factory != null) {
//            permissionCallback(factory.createPermission(this));
//        }
    }

    public Long getMasterOid() {
        return getOid();
    }
    
    public boolean isMob() {
        return getType().isMob();
    }

    public boolean isItem() {
        return getType() == ObjectTypes.item;
    }

    public boolean isLight() {
        return getType() == ObjectTypes.light;
    }

    public boolean isUser() {
        return getType().isPlayer();
    }

    public boolean isStructure() {
        return getType().isStructure();
    }

    public String toString() {
        return "[MVObject: " + getName() + ":" + getOid() + ", type=" + getType() + "]";
    }

    /**
     * sets a state for the object. all states set here are transmitted to the
     * client. examples would be when the player is dead.
     */
    public ObjState setState(String state, ObjState obj) {
        lock.lock();
        try {
            StateMap stateMap = getStateMap();
            return stateMap.setState(state, obj);
        } finally {
            lock.unlock();
        }
    }

    public ObjState getState(String s) {
        lock.lock();
        try {
            StateMap stateMap = getStateMap();
            return stateMap.getState(s);
        } finally {
            lock.unlock();
        }
    }

    public static final String stateMapKey = "mvobj.statemap";
    
    // the statemap is a map stored in the property map
    // we have this accessor to give some type safety
    private StateMap getStateMap() {
        lock.lock();
        try {
            StateMap stateMap = (StateMap) getProperty(stateMapKey);
            if (stateMap == null) {
                stateMap = new StateMap();
                setProperty(stateMapKey, stateMap);
            }
            return stateMap;
        } finally {
            lock.unlock();
        }
    }
    public static class StateMap implements Serializable {
        public StateMap() {
            setupTransient();
        }
        // called from constructor and readObject
        private void setupTransient() {
            this.lock = LockFactory.makeLock("StateMapLock");
        }

        private void readObject(ObjectInputStream in) throws IOException,
                ClassNotFoundException {
            in.defaultReadObject();
            setupTransient();
        }

        public ObjState setState(String state, ObjState objState) {
            lock.lock();
            try {
                return this.map.put(state, objState);
            }
            finally {
                lock.unlock();
            }
        }
        public ObjState getState(String state) {
            lock.lock();
            try {
                return this.map.get(state);
            }
            finally {
                lock.unlock();
            }
        }
        
        // for java beans xml serialization
        public void setMap(Map<String,ObjState> map) {
            lock.lock();
            try {
                this.map = new HashMap<String,ObjState>(map);
            }
            finally {
                lock.unlock();
            }
        }
        public Map<String,ObjState> getMap() {
            lock.lock();
            try {
                return new HashMap<String,ObjState>(this.map);
            }
            finally {
                lock.unlock();
            }
        }
        Lock lock = null;
        Map<String,ObjState> map = new HashMap<String,ObjState>();

        private static final long serialVersionUID = 1L;
    }
    
    public void sendEvent(Event event) {
        throw new MVRuntimeException("legacy code");
    }

    /**
     * returns the object's world node
     */
    public WorldNode worldNode() {
        return (WorldNode) getProperty(wnodeKey);
    }

    /**
     * sets which world node is associated with this object.
     * 
     * this does remove it from the previous world node, nor add it to the
     * worldnode passed it
     */
    public void worldNode(WorldNode worldNode) {
        setProperty(wnodeKey, worldNode);
    }

    /**
     * returns a newly-created BasicWorldNode, with loc/dir/orient from
     * the object's world node
     */
    public BasicWorldNode baseWorldNode() {
        return new BasicWorldNode((InterpolatedWorldNode)getProperty(wnodeKey));
    }

    public final static String wnodeKey = "mvobj.wnode";

    /**
     * helper function - returns the location of this object by calling into the
     * world node
     */
    public Point getLoc() {
        WorldNode node = worldNode();
        return (node == null) ? null : node.getLoc();
    }

    public Point getCurrentLoc() {
        WorldNode node = worldNode();
        return (node == null) ? null : node.getCurrentLoc();
    }

    public Quaternion getOrientation() {
        WorldNode node = worldNode();
        return (node == null) ? null : node.getOrientation();
    }

    public MVVector getDirection() {
        InterpolatedWorldNode iwn = (InterpolatedWorldNode)getProperty(wnodeKey);
        return iwn.getDir();
    }
        
    /**
     * Returns all the values from the interpolated world node
     * atomically
     */
    public InterpolatedWorldNode.InterpolatedDirLocOrientTime getDirLocOrientTime() {
        InterpolatedWorldNode iwn = (InterpolatedWorldNode)getProperty(wnodeKey);
        return iwn.getDirLocOrientTime();
    }

    // //////////////////////////////////////////////////////////////
    //
    // Perceiver Section
    //
    // //////////////////////////////////////////////////////////////

    public static final String perceiverKey = "mvobj.perceiver";

    public MobilePerceiver<WMWorldNode> perceiver() {
        // lock here because the set counterpart method is nonatomic
        lock.lock();
        try {
            return (MobilePerceiver<WMWorldNode>) getProperty(perceiverKey);
        } finally {
            lock.unlock();
        }
    }

    public void perceiver(MobilePerceiver<WMWorldNode> p) {
        lock.lock();
        try {
            MobilePerceiver<WMWorldNode> perceiver = perceiver();
            if (perceiver == p) {
                Log.warn("MVObject.setPerceiver: new/cur perceiver same");
            }
            if (perceiver != null) {
                perceiver.setElement(null);
                Log.warn("MVObject.setPerceiver: perceiv is already not null");
            }
            if (Log.loggingDebug) {
                Log.debug("MVObject.setPerceiver: obj oid=" + getOid() + ", perceiver=" + p);
            }
            setProperty(perceiverKey, p);
            if (p != null) {
                p.setElement((WMWorldNode)worldNode());
            }
        } finally {
            lock.unlock();
        }
    }

    /**
     * this is the master account id for this user (unique to each register
     * multiverse user)
     */
    public static final String mvidKey = "mvobj.mvid";

    public Integer multiverseID() {
        return getIntProperty(mvidKey);
    }

    /**
     * sets the multiverseID for this user.
     */
    public void multiverseID(Integer id) {
        setProperty(mvidKey, id);
    }

    public static final String dcKey = "mvobj.dc";

    public void displayContext(DisplayContext dc) {
        DisplayContext dcCopy = null;
        if (dc != null) {
            dcCopy = (DisplayContext) dc.clone();
            dcCopy.setObjRef(this.getOid());
        }
        setProperty(dcKey, dcCopy);
    }

    public DisplayContext displayContext() {
        DisplayContext dc = (DisplayContext) getProperty(dcKey);
        return dc;
    }

    private String scaleKey = "mvobj.scale";

    public void scale(float scale) {
        scale(new MVVector(scale, scale, scale));
    }

    /**
     * sets the scale of this object (how much bigger to make it)
     */
    public void scale(MVVector scale) {
        setProperty(scaleKey, (MVVector) scale.clone());
    }

    public MVVector scale() {
        return (MVVector) getProperty(scaleKey);
    }

    // 
    // creation hook
    public static void registerObjCreateHook(MVObjectCreateHook hook) {
        createHook = hook;
    }

    public static MVObjectCreateHook getObjCreateHook() {
        return createHook;
    }

    // //////////////////////////////////
    //
    // Ownership methods
    //

    /**
     * sets the permissioncallback, which gets called whenever something is
     * trying to 'access' this object either by picking it up, dropping it,
     * trading, etc.
     * 
     * @see PermissionCallback
     */
    public void permissionCallback(PermissionCallback cb) {
        setProperty(permCBKey, cb);
    }

    public PermissionCallback permissionCallback() {
        return (PermissionCallback) getProperty(permCBKey);
    }

    private String permCBKey = "mvobj.permCB";

    // static methods
    private static MVObjectCreateHook createHook = null;

    /**
     * helper method which can handle writing a null object to the output
     * stream. it first writes whether the obj is null, followed by the obj if
     * it isnt null.. otherwise doesnt write the obj
     */
    public static void writeObject(ObjectOutput out, Object obj)
            throws IOException {
        out.writeBoolean(obj == null);
        if (obj != null) {
            out.writeObject(obj);
        }
    }

    public static Object readObject(ObjectInput in) throws IOException,
            ClassNotFoundException {
        boolean isNull = in.readBoolean();
        if (!isNull) {
            return in.readObject();
        } else {
            return null;
        }
    }

    /**
     * helper method that writes a null string as an empty string
     */
    public static void writeString(ObjectOutput out, String string)
            throws IOException {
        if (string == null) {
            out.writeUTF("");
        } else {
            out.writeUTF(string);
        }
    }

    /**
     * returns all entities in this server
     */
    public static Collection<MVObject> getAllObjects() {
        Entity[] entities = EntityManager.getAllEntitiesByNamespace(Namespace.WORLD_MANAGER);
        Set<MVObject> objSet = new HashSet<MVObject>();
        for (Entity e : entities) {
            if (e instanceof MVObject) {
                objSet.add((MVObject) e);
            }
        }
        return objSet;
    }

    public static MVObject getObject(long oid) {
        return (MVObject) EntityManager.getEntityByNamespace(oid, Namespace.WORLD_MANAGER);
    }

    /**
     * use this global object to synchronize all object transfers. if you drop
     * and object, pick up an object, destory an object, trade an object, etc,
     * you must grab this lock to make sure there is no race condition. eg,
     * someone sees that the obj is "free" on the ground THEN picks it up. there
     * is a race condition there
     */
    public static Lock transferLock = LockFactory.makeLock("objXferLock");

    static {
        try {
            BeanInfo info = Introspector.getBeanInfo(MVObject.class);
            PropertyDescriptor[] propertyDescriptors = info
                    .getPropertyDescriptors();
            for (int i = 0; i < propertyDescriptors.length; ++i) {
                // PropertyDescriptor pd = propertyDescriptors[i];
                // if (pd.getName().equals("displayContext")) {
                // pd.setValue("transient", Boolean.TRUE);
                // }
            }
        } catch (Exception e) {
            Log.error("failed mvobject beans initalization");
        }
    }

    // ////////////////////////////////////////////////////////////////////
    // //
    // // container code
    // //
    // ////////////////////////////////////////////////////////////////////

    // public void containerAdd(MVObject obj) {
    // try {
    // MVObject.transferLock.lock();
    // container.add(obj);
    // obj.setContainedIn(this);
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // /**
    // * returns true if the obj was removed
    // */
    // public boolean containerRemove(MVObject obj) {
    // try {
    // MVObject.transferLock.lock();
    // if (container.remove(obj)) {
    // obj.setContainedIn(null);
    // return true;
    // }
    // return false;
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // /**
    // * returns a collection of all objects in this container - copied
    // */
    // public Collection<MVObject> getContainerObjects() {
    // MVObject.transferLock.lock();
    // try {
    // return new HashSet<MVObject>(container);
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }
    // public void setContainerObjects(Collection<MVObject> objs)
    // {
    // MVObject.transferLock.lock();
    // try {
    // log.debug("setContainerObjects: thisObj=" + getName() +
    // ", numObj=" + objs.size());

    // // we have to add each one seperate since we need to set
    // // the 'containedIn' property for each one when we add them
    // this.container = new HashSet<MVObject>();
    // for (MVObject obj : objs) {
    // log.debug("setContainerObjects: adding obj: " + obj);
    // containerAdd(obj);
    // }
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }
    // /**
    // * returns the size of this container (# of objects in it, not max)
    // */
    // public int containerSize() {
    // try {
    // MVObject.transferLock.lock();
    // return container.size();
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // /**
    // * returns a reference to the object if it exists in the container
    // * search by name
    // * returns null if no object with the name exists
    // */
    // public MVObject containerFind(String name) {
    // try {
    // MVObject.transferLock.lock();
    // Iterator<MVObject> iter = container.iterator();
    // while (iter.hasNext()) {
    // MVObject obj = iter.next();
    // if (obj.getName().equals(name)) {
    // return obj;
    // }
    // }
    // return null;
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // /**
    // * returns whether this object contains the passed in obj
    // */
    // public boolean containerContains(MVObject obj) {
    // MVObject.transferLock.lock();
    // try {
    // return container.contains(obj);
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // /**
    // * returns the container this object is located in.
    // * not part of the container interface (would be 'containerable' but
    // * that sounds stupid)
    // * returns null if the obj is not in any container
    // */
    // public MVObject getContainedIn() {
    // MVObject.transferLock.lock();
    // try {
    // return containedIn;
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // /**
    // * should be called by containerAdd
    // */
    // private void setContainedIn(MVObject obj) {
    // MVObject.transferLock.lock();
    // try {
    // containedIn = obj;
    // }
    // finally {
    // MVObject.transferLock.unlock();
    // }
    // }

    // private Collection<MVObject> container = new HashSet<MVObject>();
    // private MVObject containedIn = null;

    // public void addEventListener(MVEventListener l, Class eventClass) throws
    // MVRuntimeException {
    // try {
    // lock.lock();
    // ArrayList<MVEventListener> listenerList = listenerMap.get(eventClass);
    // if (listenerList == null) {
    // listenerList = new ArrayList<MVEventListener>();
    // listenerMap.put(eventClass, listenerList);
    // }
    // try {
    // Log.debug("MOVbject: addEventListener: adding listener=" + l.getName() +
    // " for class=" + eventClass + " to obj=" + this);
    // }
    // catch (RemoteException e) {
    // throw new MVRuntimeException("MVObject: addEventListener", e);
    // }
    // listenerList.add(l);
    // }
    // finally {
    // lock.unlock();
    // }
    // }
    // public void removeEventListener(MVEventListener l, Class eventClass)
    // {
    // try {
    // lock.lock();
    // ArrayList<MVEventListener> listenerList = listenerMap.get(eventClass);
    // if (listenerList != null) {
    // try {
    // Log.debug("MOVbject: removeEventListener: removing listener=" +
    // l.getName() + " for class=" + eventClass + " to obj=" + this);
    // }
    // catch (RemoteException e) {
    // throw new MVRuntimeException("MVObject: addEventListener", e);
    // }
    // listenerList.remove(l);
    // }
    // }
    // finally {
    // lock.unlock();
    // }
    // }
    // public void clearEventListeners() {
    // try {
    // lock.lock();
    // listenerMap.clear();
    // }
    // finally {
    // lock.unlock();
    // }
    // }
    // protected Map<Class, ArrayList<MVEventListener>> listenerMap =
    // new HashMap<Class, ArrayList<MVEventListener>>();

    // list of type AnimationCommand
    // private List<AnimationCommand> animationList = null;

    // private Map socketMapping = new HashMap(); // mapping of invslot ->
    // socket
    // private MVObject owner = null;
    // private Behavior bhv = null;
    /**
     * this mob's perceiver - the perceiver is created when a behavior is
     * assigned to the object, or if user, in the user constructor
     */
    // protected MobilePerceiver perceiver = null;
    // protected static final Logger log = new Logger("MVObject");

    private static final long serialVersionUID = 1L;
}
