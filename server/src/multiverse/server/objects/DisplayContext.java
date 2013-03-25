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

import multiverse.server.util.*;
import java.util.concurrent.locks.*;
import java.util.*;
import java.io.*;

import multiverse.mars.objects.*;

public class DisplayContext implements Cloneable, Serializable {
    public DisplayContext() {
        setupTransient();
    }
    
    public DisplayContext(Long oid) {
        setupTransient();
        this.objRef = oid;
    }
    public DisplayContext(String meshfile) {
        setupTransient();
        setMeshFile(meshfile);
    }
    public DisplayContext(String meshfile, boolean castShadow) {
        setupTransient();
        setMeshFile(meshfile);
	setCastShadow(castShadow);
    }
    public DisplayContext(Long oid, String meshfile) {
        setupTransient();
        this.objRef = oid;
        setMeshFile(meshfile);
    }

    protected void setupTransient() {
        lock = LockFactory.makeLock("DisplayContextLock");
    }
    
    /**
     * this this dc a subset of the other dc
     */
    public boolean subsetOf(DisplayContext other) {
	if (! getMeshFile().equals(other.getMeshFile())) {
	    return false;
	}

	Set<Submesh> otherSubmeshes = other.getSubmeshes();
	for (Submesh submesh : getSubmeshes()) {
	    if (! otherSubmeshes.contains(submesh)) {
		return false;
	    }
	}
	return true;
    }
    
    /**
     * used for adding/removing child display contexts
     */
    public boolean equals(Object other) {
        DisplayContext otherDC = (DisplayContext) other;
        if (! (otherDC.getMeshFile().equals(this.getMeshFile()))) {
            return false;
        }
        return (this.subsetOf(otherDC) && otherDC.subsetOf(this));
    }
    
    public int hashCode() {
	int hash = meshFile.hashCode();
	for (Submesh subMesh : getSubmeshes()) {
	    hash ^= subMesh.hashCode();
	}
	return hash;
    }

    public String toString() {
        Set<Submesh> subMeshes = getSubmeshes();
        String s = "[DisplayContext: " +
            "meshFile=" + getMeshFile() +
            ", attachableFlag=" + getAttachableFlag() +
            ", castShadow=" + getCastShadow() +
            ", receiveShadow=" + getReceiveShadow() +
            ", numSubmeshes=" + subMeshes.size();
        
        for (Submesh subMesh : subMeshes) {
            s += ", submesh=" + subMesh;
        }
        return s + "]";
    }

    // FIXME: need to hold some global copy lock otherwise we can get deadlock here
    public Object clone() {
        lock.lock();
        try {
            DisplayContext dc = new DisplayContext(this.getObjRef());
            dc.setMeshFile(getMeshFile());

            // dont call getSubmeshes due to extra copy
            dc.setSubmeshes(submeshes); 

            dc.setAttachableFlag(getAttachableFlag());
            
            dc.setDisplayInfo(getDisplayInfo());
            
            dc.setChildDCMap(getChildDCMap());

	    dc.setCastShadow(getCastShadow());
	    dc.setReceiveShadow(getReceiveShadow());
            return dc;
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * sets the back reference to the object this dc is associated with,
     * can be null
     */
    public void setObjRef(Long oid) {
        this.objRef = oid;
    }
    /**
     * returns the back reference to the object this dc is associated with, can be
     * null
     */
    public Long getObjRef() {
        return this.objRef;
    }
    
    public String getMeshFile() {
        return meshFile;
    }
    public void setMeshFile(String mesh) {
        this.meshFile = mesh;
    }

    public void addSubmesh(Submesh submesh) {
        lock.lock();
        try {
            submeshes.add(submesh);
        }
        finally {
            lock.unlock();
        }
    }
    public void addSubmeshes(Collection<Submesh> submeshes) {
        lock.lock();
        try {
            this.submeshes.addAll(submeshes);
        }
        finally {
            lock.unlock();
        }
    }
    public void removeSubmesh(Submesh submesh) {
	lock.lock();
	try {
	    submeshes.remove(submesh);
	}
	finally {
	    lock.unlock();
	}
    }
    public void removeSubmeshes(Collection<Submesh> submeshes) {
	lock.lock();
	try {
            if (Log.loggingDebug)
                Log.debug("DisplayContext.removeSubmeshes: removelist=" +
                          submeshes +
                          ", currentDC=" + this);
	    this.submeshes.removeAll(submeshes);
            if (Log.loggingDebug)
                Log.debug("DisplayContext.removeSubmeshes: updated dc=" + this);
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * adds a child display context to this display context.
     * when the proxy sends over the modelinfo/attachments over to the client
     * it will 'collapse' this data for the client.
     * socket/hard attachments should be added as child dc's since they are full
     * fledged display contexts.
     * @param handle the name used to refer to this dc.  usually it is the
     * position of the attachment (back, hands, etc) so that you can easily 
     * remove it later.
     * @param dc the child display context, will be copied
     */
    public void addChildDC(String handle, DisplayContext dc) {
        lock.lock();
        try {
            childDCMap.put(handle, (DisplayContext)dc.clone());
        }
        finally {
            lock.unlock();
        }
    }
    public DisplayContext getChildDC(String handle) {
        lock.lock();
        try {
            return childDCMap.get(handle);
        }
        finally {
            lock.unlock();
        }
    }

    public DisplayContext removeChildDC(String handle) {
        lock.lock();
        try {
            return childDCMap.remove(handle);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * set for xml serialization -- do not use
     */
    public void setChildDCMap(Map<String,DisplayContext> map) {
        lock.lock();
        try {
            this.childDCMap = new HashMap<String,DisplayContext>(map);
        }
        finally {
            lock.unlock();
        }
    }

    public Map<String, DisplayContext> getChildDCMap() {
        lock.lock();
        try {
            return new HashMap<String,DisplayContext>(this.childDCMap);
        }
        finally {
            lock.unlock();
        }
    }

    public void setSubmeshes(Set<Submesh> submeshes) {
        lock.lock();
        try {
            this.submeshes = new HashSet<Submesh>(submeshes);
        }
        finally {
            lock.unlock();
        }
    }
    public Set<Submesh> getSubmeshes() {
        lock.lock();
        try {
            return new HashSet<Submesh>(submeshes);
        }
        finally {
            lock.unlock();
        }
    }

    // is this item attachable, if so you can getAttachInfo()
    public boolean getAttachableFlag() {
        return attachableFlag;
    }
    public void setAttachableFlag(boolean b) {
        attachableFlag = b;
    }

    public void setAttachInfo(DisplayState displayState, 
                              MarsEquipSlot equipSlot,
                              MarsAttachSocket socket) {
        lock.lock();
        try {
            setAttachableFlag(true);
            Map<MarsEquipSlot, MarsAttachSocket> attachMap = 
                displayInfoMap.get(displayState);

            if (attachMap == null) {
                // need to make the map
                attachMap = new HashMap<MarsEquipSlot, MarsAttachSocket>();
                displayInfoMap.put(displayState, attachMap);
            }
            attachMap.put(equipSlot, socket);
        }
        finally {
            lock.unlock();
        }
    }
    public MarsAttachSocket getAttachInfo(DisplayState ds, 
                                          MarsEquipSlot equipSlot)  {
        lock.lock();
        try {
            if (! getAttachableFlag()) {
                Log.error("DisplayContext.getAttachInfo: not attachable");
                return null;
            }
            Map<MarsEquipSlot, MarsAttachSocket> attachMap = 
                displayInfoMap.get(ds);

            if (attachMap == null) {
                Log.warn("DisplayContext.getAttachInfo: could not find displayState " + ds);
                return null;
            }
            return attachMap.get(equipSlot);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * for the java.beans.PersistenceDelegate code
     */
    public void setDisplayInfo(Map<DisplayState, Map<MarsEquipSlot, MarsAttachSocket>> map) {
        lock.lock();
        try {
            if (! displayInfoMap.isEmpty()) {
                throw new RuntimeException("displaycontext: setting display info on existing non empty map");
            }

            for (Map.Entry<DisplayState, Map<MarsEquipSlot, MarsAttachSocket>> entry : map.entrySet()) {
                DisplayState ds = entry.getKey();
                Map<MarsEquipSlot, MarsAttachSocket> attachMap = new HashMap<MarsEquipSlot, MarsAttachSocket>(entry.getValue());
                displayInfoMap.put(ds, attachMap);
            }
        }
        finally {
            lock.unlock();
        }
    }
    public Map<DisplayState, Map<MarsEquipSlot, MarsAttachSocket>> getDisplayInfo() {
        return displayInfoMap;
    }

    /**
     * for debugging - prints out the attach map
     */
    public void printAttachInfo() {
        lock.lock();
        try {
            for (DisplayState ds : displayInfoMap.keySet()) {
                if (Log.loggingDebug)
                    Log.debug("DisplayContext.printAttachInfo: state=" + ds);
                printAttachInfo(displayInfoMap.get(ds));
            }
        }
        finally {
            lock.unlock();
        }
    }
    protected void printAttachInfo(Map<MarsEquipSlot, MarsAttachSocket> map) {
        lock.lock();
        try {
            Iterator<MarsEquipSlot> keysIter = map.keySet().iterator();
            while (keysIter.hasNext()) {
                MarsEquipSlot slot = keysIter.next();
                MarsAttachSocket socket = map.get(slot);
                if (Log.loggingDebug)
                    Log.debug("DisplayContext.printAttachInfo: slot=" + slot +
                              ", socket=" + socket);
            }
        }
        finally {
            lock.unlock();
        }
    }

    public static class Submesh implements Serializable {
        public Submesh() {
        }

        public Submesh(String name, String material) {
            this.name = name;
            this.material = material;
        }

        public String toString() {
            return "[Submesh: name=" + name + ", material=" + material + "]";
        }

	public boolean equals(Object other) {
	    Submesh otherSub = (Submesh) other;
	    return (this.name.equals(otherSub.getName()) &&
		    this.material.equals(otherSub.getMaterial()));
	}
	public int hashCode() {
            return ((getName() == null || getName().equals("") ? 0 : getName().hashCode()) ^
                    (getMaterial() == null ? 0 : getMaterial().hashCode()));
	}

        public void setName(String name) {
            this.name = name;
        }
        public String getName() {
            return name;
        }
        public String name = null;

        public void setMaterial(String material) {
            this.material = material;
        }
        public String getMaterial() {
            return material;
        }
        public String material = null;

        private static final long serialVersionUID = 1L;
    }

    // Shadow settings
    public void setCastShadow(boolean cast) {
	castShadow = cast;
    }
    public boolean getCastShadow() {
	return castShadow;
    }
    public void setReceiveShadow(boolean receive) {
	receiveShadow = receive;
    }
    public boolean getReceiveShadow() {
	return receiveShadow;
    }

    private void writeObject(ObjectOutputStream out)
	throws IOException, ClassNotFoundException {
        out.defaultWriteObject();
    }
    
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        setupTransient();
        in.defaultReadObject();
    }

    public String meshFile = null;
    
    private Map<String,DisplayContext> childDCMap = new HashMap<String,DisplayContext>();
    
    private Long objRef = null;

    private Set<Submesh> submeshes = new HashSet<Submesh>();
    // we dont want duplicate submeshes

    // mapping of where the item should go when the equipper is in combat
    private Map<DisplayState, Map<MarsEquipSlot, MarsAttachSocket>> displayInfoMap = new HashMap<DisplayState, Map<MarsEquipSlot, MarsAttachSocket>>();

    private boolean attachableFlag = false;
    private boolean castShadow = false;
    private boolean receiveShadow = false;

    protected transient Lock lock;

    private static final long serialVersionUID = 1L;
}
