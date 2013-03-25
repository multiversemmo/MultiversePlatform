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

import java.util.*;
import java.util.concurrent.locks.*;
import multiverse.server.util.*;
import multiverse.server.pathing.*;
import multiverse.server.math.*;
import multiverse.server.objects.*;
import java.beans.*;
import multiverse.server.plugins.WorldManagerClient;

public class InterpolatedWorldNode implements WorldNode, BasicInterpolatable {
    public InterpolatedWorldNode() {
        setupTransient();
    }

    public InterpolatedWorldNode(BasicWorldNode bnode) {
        setupTransient();
        instanceOid = bnode.getInstanceOid();
        rawLoc = bnode.getLoc();
        interpLoc = rawLoc;
        dir = bnode.getDir();
        orient = bnode.getOrientation();
        lastInterp = System.currentTimeMillis();
    }

    public InterpolatedWorldNode(WorldManagerClient.ObjectInfo info) {
        setupTransient();
        rawLoc = info.loc;
        interpLoc = rawLoc;
        dir = info.dir;
        orient = info.orient;
        lastInterp = System.currentTimeMillis();
        instanceOid = info.instanceOid;
    }
    
    void setupTransient() {
        lock = LockFactory.makeLock("InterpolatedWorldNodeLock");
    }

    public String toString() {
        return "[InterpolatedWorldNode: objHandle=" + objHandle +
                ", instanceOid=" + getInstanceOid() +
                ", rawLoc=" + getRawLoc() + ", interpLoc=" + getInterpLoc() +
                ", dir=" + getDir() +
                ", orient=" + getOrientation() + "]";
    }

    // Locatable

    public long getInstanceOid() {
        return instanceOid;
    }

    public void setInstanceOid(long oid) {
        instanceOid = oid;
    }

    public Point getLoc() {
	Lock myTreeLock = treeLock;
        if (myTreeLock != null) {
            myTreeLock.lock();
        }
        lock.lock();
        try {
            BasicInterpolator interp = (BasicInterpolator) Engine
                    .getInterpolator();
            if (interp != null) {
                interp.interpolate(this);
            }
            return (interpLoc == null) ? null : (Point) interpLoc.clone();
        } finally {
            lock.unlock();
            if (myTreeLock != null) {
                myTreeLock.unlock();
            }
        }
    }

    public void setLoc(Point p) {
	Lock myTreeLock = treeLock;
        if (myTreeLock != null) {
            myTreeLock.lock();
        }
        lock.lock();
        try {
            long time = System.currentTimeMillis();
            setRawLoc(p);
            setLastUpdate(time);
            setInterpLoc(p);
            setLastInterp(time);
        } finally {
            lock.unlock();
            if (myTreeLock != null) {
                myTreeLock.unlock();
            }
        }
    }

    public long getLastUpdate() {
        lock.lock();
        try {
            return lastUpdate;
        } finally {
            lock.unlock();
        }
    }

    public void setLastUpdate(long time) {
        lock.lock();
        try {
            lastUpdate = time;
        } finally {
            lock.unlock();
        }
    }

    // WorldNode
    public MVObject getObject() {
        lock.lock();
        try {
            return (objHandle == null) ? null : (MVObject) objHandle.getEntity(Namespace.WORLD_MANAGER);
        } finally {
            lock.unlock();
        }
    }

    public void setObject(MVObject obj) {
        lock.lock();
        try {
            this.objHandle = (obj == null) ? null : new EntityHandle(obj);
        } finally {
            lock.unlock();
        }
    }

    public void setObjectOID(Long oid) {
        this.objHandle = new EntityHandle(oid);
    }
    public Long getObjectOID() {
        if (objHandle == null) {
            return null;
        }
        return this.objHandle.getOid();
    }
    
    public WorldNode getParent() {
        lock.lock();
        try {
            return parent;
        } finally {
            lock.unlock();
        }
    }

    public void setParent(WorldNode node) {
        lock.lock();
        try {
            parent = node;
        } finally {
            lock.unlock();
        }
    }

    public Quaternion getOrientation() {
        lock.lock();
        try {
            return (orient == null) ? null : (Quaternion) orient.clone();
        } finally {
            lock.unlock();
        }
    }

    public void setOrientation(Quaternion orient) {
        lock.lock();
        try {
            this.orient = (orient == null) ? null : (Quaternion) orient.clone();
        } finally {
            lock.unlock();
        }
    }

    public void setDirLocOrient(BasicWorldNode bnode) {
	Lock myTreeLock = treeLock;
        if (myTreeLock != null) {
            myTreeLock.lock();
        }
        lock.lock();
        try {
            setRawLoc(bnode.getLoc());
            setInterpLoc(bnode.getLoc());
            setDir(bnode.getDir());
            setOrientation(bnode.getOrientation());
            setLastInterp(System.currentTimeMillis());
        }
        finally {
            lock.unlock();
            if (myTreeLock != null) {
                myTreeLock.unlock();
            }
        }
    }
        
    /**
     * A utility class provided solely to return these values atomically
     */
    public class InterpolatedDirLocOrientTime {
        
        public MVVector dir;
        public Point interpLoc;
        public Quaternion orient;
        public long lastInterp;
    }
    
    public InterpolatedDirLocOrientTime getDirLocOrientTime() 
    {
        InterpolatedDirLocOrientTime val = new InterpolatedDirLocOrientTime();
        lock.lock();
        try {
            val.interpLoc = (interpLoc == null) ? null : (Point) interpLoc.clone();
            val.dir = (dir == null) ? null : (MVVector) dir.clone();
            val.orient = (orient == null) ? null : (Quaternion) orient.clone();
            val.lastInterp = lastInterp;
            return val;
        }
        finally {
            lock.unlock();
        }
    }
    
    public Set<WorldNode> getChildren() {
        lock.lock();
        try {
	    if (children != null) {
		return new HashSet<WorldNode>(children);
	    }
	    else {
		return new HashSet<WorldNode>();
	    }
        } finally {
            lock.unlock();
        }
    }

    public void setChildren(Set<WorldNode> children) {
        lock.lock();
        try {
            this.children = new HashSet<WorldNode>(children);
        } finally {
            lock.unlock();
        }
    }

    public void addChild(WorldNode child) {
        lock.lock();
        try {
            if (children == null) {
                children = new HashSet<WorldNode>();
            }
            children.add(child);
        } finally {
            lock.unlock();
        }
    }

    public void removeChild(WorldNode child) {
        lock.lock();
        try {
            children.remove(child);
            if (children.size() == 0) {
                children = null;
            }
        } finally {
            lock.unlock();
        }
    }

    public boolean isSpawned() {
        return spawned;
    }

    public void isSpawned(boolean spawned) {
        this.spawned = spawned;
        if (!spawned) {
            BasicInterpolator interp =
                (BasicInterpolator) Engine.getInterpolator();
            if (interp != null) {
                interp.unregister(this);
            }
        }
    }

    public PathInterpolator getPathInterpolator() {
        return pathInterpolator;
    }

    public void setPathInterpolator(PathInterpolator pathInterpolator) {
        lock.lock();
        try {
            this.pathInterpolator = pathInterpolator;
            if (pathInterpolator == null)
                changeDir(new MVVector(0, 0, 0), false);
        } finally {
            lock.unlock();
        }
    }
        
    public PathLocAndDir interpolate(float t) {
        lock.lock();
        try {
            if (pathInterpolator == null)
                return null;
            else {
                PathLocAndDir locAndDir = pathInterpolator.interpolate(t);
                if (locAndDir == null)
                    pathInterpolator = null;
                return null;
            }
        } finally {
            lock.unlock();
        }
    }
    
    // BasicInterpolatable
    public MVVector getDir() {
        lock.lock();
        try {
            return (dir == null) ? null : (MVVector) dir.clone();
        } finally {
            lock.unlock();
        }
    }

    public void setDir(MVVector dir) {
	Lock myTreeLock = treeLock;
        if (myTreeLock != null) {
            myTreeLock.lock();
        }
        lock.lock();
        try {
            changeDir(dir, true);
        } finally {
            lock.unlock();
            if (myTreeLock != null) {
                myTreeLock.unlock();
            }
        }
    }

    // A common subroutine for setDir and setInterpValues.  Always
    // called with the lock held
    protected void changeDir(MVVector dir, boolean performDirInterpolation) {
        BasicInterpolator interp = (BasicInterpolator) Engine
            .getInterpolator();
        if (interp != null) {
            if (performDirInterpolation) {
                interp.interpolate(this);
                if (!this.dir.isZero() && dir.isZero())
                    interp.unregister(this);
                else if (this.dir.isZero() && !dir.isZero())
                    interp.register(this);
            }
        }
        this.dir = (dir == null) ? null : (MVVector) dir.clone();
    }
    
    public Point getRawLoc() {
        lock.lock();
        try {
            return (rawLoc == null) ? null : (Point) rawLoc.clone();
        } finally {
            lock.unlock();
        }
    }

    public void setRawLoc(Point p) {
        lock.lock();
        try {
            rawLoc = (p == null) ? null : (Point) p.clone();
        } finally {
            lock.unlock();
        }
    }

    public long getLastInterp() {
        lock.lock();
        try {
            return lastInterp;
        } finally {
            lock.unlock();
        }
    }

    public void setLastInterp(long time) {
        lock.lock();
        try {
            lastInterp = time;
        } finally {
            lock.unlock();
        }
    }

    public Point getInterpLoc() {
        lock.lock();
        try {
            return (interpLoc == null) ? null : (Point) interpLoc.clone();
        } finally {
            lock.unlock();
        }
    }

    public Point getCurrentLoc() {
        return getInterpLoc();
    }

    public void setInterpLoc(Point p) {
        lock.lock();
        try {
            interpLoc = (p == null) ? null : (Point) p.clone();
        } finally {
            lock.unlock();
        }
    }

    // Whack time, direction, location and orientation atomically.
    // This is only called by the path interpolator code, and we
    // should always disable dir interpolation
    public void setPathInterpolatorValues(long time, MVVector newDir, Point newLoc, Quaternion orientation) {
	Lock myTreeLock = treeLock;
        if (myTreeLock != null) {
            myTreeLock.lock();
        }
        lock.lock();
        try {
            lastInterp = time;
            dir = newDir;
            orient = (orientation == null) ? null : (Quaternion)orientation.clone();
            setInterpLoc(newLoc);
        } finally {
            lock.unlock();
            if (myTreeLock != null) {
                myTreeLock.unlock();
            }
        }
    }
    
    public Boolean getFollowsTerrain() {
        return followsTerrain;
    }

    public void setFollowsTerrain(Boolean flag) {
	followsTerrain = flag;
    }

    private void readObject(java.io.ObjectInputStream in)
            throws java.io.IOException, ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    protected EntityHandle objHandle = null;

    protected Boolean followsTerrain = true;

    protected boolean spawned = false;
    
    protected long instanceOid;

    protected Point rawLoc = null;

    protected Point interpLoc = null;

    protected MVVector dir = new MVVector(0, 0, 0);

    protected Quaternion orient = null;

    protected transient PathInterpolator pathInterpolator = null;

    protected long lastUpdate = -1;

    protected long lastInterp = -1;

    protected WorldNode parent = null;

    protected Set<WorldNode> children = null;

    public transient Lock lock = null;

    public transient Lock treeLock = null;

    static {
        try {
            BeanInfo info = Introspector.getBeanInfo(InterpolatedWorldNode.class);
            PropertyDescriptor[] propertyDescriptors = info
                    .getPropertyDescriptors();
            for (int i = 0; i < propertyDescriptors.length; ++i) {
                PropertyDescriptor pd = propertyDescriptors[i];
                if (pd.getName().equals("children")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
                if (pd.getName().equals("object")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
                if (pd.getName().equals("loc")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
                if (pd.getName().equals("lastUpdate")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
                if (pd.getName().equals("parent")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
                if (pd.getName().equals("dir")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
            }
        } catch (Exception e) {
            Log.error("failed beans initalization");
        }
    }

//    public static class BasicWorldNodePersistenceDelegate extends
//            DefaultPersistenceDelegate {
//
//        protected void initialize(Class type, Object oldInstance,
//                Object newInstance, Encoder out) {
//            super.initialize(type, oldInstance, newInstance, out);
//
//            InterpolatedWorldNode node = (InterpolatedWorldNode) oldInstance;
//            out.writeStatement(new Statement(oldInstance, "setObjectOID",
//                    new Object[] { node.getObjectOID() }));
//        }
//    }
    private static final long serialVersionUID = 1L;
}
