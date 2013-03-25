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

import multiverse.server.engine.*;
import multiverse.server.util.*;
import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;

/**
 * Entity is the root class of persistable objects 
 * in the Multiverse heirarchy.
 * All entities have a unique object ID (OID).  Uniqueness is 
 * guaranteed through the OIDManager which typically 
 * grabs blocks of unallocated
 * IDs from the database.
 * <p>
 * Examples of Entities include players, monsters, weapons, armor,
 * quests, and templates.  Non-persisted configuration data
 * is typically not an entity.
 */
public class Entity extends NamedPropertyClass
    implements Serializable
{
    
    /**
     * Creates an entity and assigns it a new OID.
     */
    public Entity() {
        super();
    }

    /**
     * Creates an entity with the given name, and assigns it a new OID.
     * @param name name for the entity.
     */
    public Entity(String name) {
        super(name);
        setOid(Engine.getOIDManager().getNextOid());
    }

    /**
     * Creates an entity using the passed in OID.  This is typically used
     * when loading a saved entity from the database since you want to set the
     * OID to match what was saved.
     * @param oid OID for the constructed entity
     */
    public Entity(Long oid) {
        super();
        setOid(oid);
    }

    /**
     * Returns the string describing this entity, useful for logging.
     * 
     * @return string describing entity
     */
    public String toString() {
        return "[Entity: " + getName() + ":" + getOid() + "]";
    }

    /**
     * private method to recreate the lock when deserializing
     */
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    /**
     * Returns the OID as hashcode since it is unique.
     */
    public int hashCode() {
        return getOid().intValue();
    }

    /**
     * Returns OID for this Entity
     * @return oid for this Entity
     */
    public Long getOid() {
        return this.oid;
    }

    /**
     * Sets the OID for this entity.  This generally should not be used since it
     * means the old OID is thrown away and probably wasted.  Try to use the
     * constructor which takes in an OID instead.
     * @param oid the OID to set for this entity.
     */
    public void setOid(Long oid) {
        this.oid = oid;
        if (lock instanceof DLock) {
            ((DLock) lock).setName("EntityLock_" + oid);
        }
    }

    /** Entity object type.
     * @return {@link ObjectTypes#unknown} if no object type has been set.
     */
    public ObjectType getType() {
        return type;
    }

    /** Set the Entity object type.
    */
    public void setType(ObjectType type) {
        this.type = type;
    }

    /**
     * Sets transient data.  Similar to properties, but does not get
     * serialized into the database or over the network.  Useful for
     * session data.
     * @return The previous value associated with key, or null
     */
    private Serializable setTransientData(String key, Serializable value) {
        lock.lock();
        try {
            if (transientMap == null)
                transientMap = new HashMap<String, Serializable>();
            return transientMap.put(key, value);
        } finally {
            lock.unlock();
        }
    }

    /**
     * Gets transient data.  Similar to properties, but does not get
     * serialized into the database or over the network.  Useful for
     * session data.
     * @param key key for the transient property.
     * @return the value or null if none exists for the key.
     */
    private Serializable getTransientData(Object key) {
        lock.lock();
        try {
            if (transientMap == null)
                transientMap = new HashMap<String, Serializable>();
            return transientMap.get(key);
        } finally {
            lock.unlock();
        }
    }

    public Map<String,Serializable> getTransientDataRef()
    {
        return transientMap;
    }

    /**
     * Returns if two objects are the same - tested by comparing the
     * object id and namespace.  null objects are never equal to any
     * other object including other null objects.
     */
    public static boolean equals(Entity obj1, Entity obj2) {
        if ((obj1 == null) || (obj2 == null)) {
            return false;
        }
        return (obj1.getOid().equals(obj2.getOid()) &&
                obj1.namespaceByte == obj2.namespaceByte);
    }

    /**
     * Returns if two objects are the same - tested by comparing the object id.
     * null objects are never equal to any other object including other null objects.
     */
    public boolean equals(Object other) {
        Entity otherObj = (Entity) other;
        return Entity.equals(this, otherObj);
    }

    /**
     * mark this object as being persistent or not. other systems will check
     * this field and save the object to the database when appropriate.
     */
    public void setPersistenceFlag(boolean flag) {
        persistEntity = flag;
    }

    /**
     * Returns true if the object has been marked persistent. saved objects
     * maintain this state
     */
    public boolean getPersistenceFlag() {
        return persistEntity;
    }

    /**
     * Returns the namespace containing the entity
     */
    public Namespace getNamespace() {
        return Namespace.getNamespaceFromInt((int)namespaceByte);
    }

    /**
     * Sets the entity's namespace
     */
    public void setNamespace(Namespace namespace) {
        namespaceByte = (byte)namespace.getNumber();
    }
    
    /**
     * Get a list of the sub-object Namespace objects associated with the Entity.
     * @return List of Namespace objects
     */
    public List<Namespace> getSubObjectNamespaces() {
        return Namespace.decompressNamespaceList(subObjectNamespacesInt);
    }
    
    /**
     * Set the sub-object Namespace objects associated with the Entity.
     * @param namespaces A list of Namespace objects
     */
    public void setSubObjectNamespaces(Set<Namespace> namespaces) {
        subObjectNamespacesInt = Namespace.compressNamespaceList(namespaces);
    }
    
    /**
     * Returns the lock for this entity.  Be careful when using the lock, since it
     * can easily lead to a deadlock.
     * @return A Lock object.
     */
    public Lock getLock() {
        return lock;
    }

    /**
     * Serializes this entity and returns the byte array.
     * @return byte array for serialized entity
     */
    public byte[] toBytes() {
        try {
            ByteArrayOutputStream ba = new ByteArrayOutputStream();
            ObjectOutputStream os = new ObjectOutputStream(ba);
            os.writeObject(this);
            os.flush();
            ba.flush();
            return ba.toByteArray();
        } catch (Exception e) {
            throw new RuntimeException("Entity.toBytes", e);
        }
    }

    /**
     * If the key is in the transientPropertyKeys set, store the
     * key/value pair in the transientMap; otherwise store it in the
     * persistent map
     */
    public Serializable setProperty(String key, Serializable value) {
        if (transientPropertyKeys.contains(key))
            return setTransientData(key, value);
        else
            return super.setProperty(key, value);
    }
    
    /**
     * If the key is in the transientPropertyKeys set, get the value
     * associated with the key from the transientMap; otherwise get
     * it from the persistent map
     */
    public Serializable getProperty(String key) {
        if (transientPropertyKeys.contains(key))
            return getTransientData(key);
        else
            return super.getProperty(key);
    }

    public boolean isDeleted()
    {
        return deleted;
    }

    public void setDeleted()
    {
        deleted = true;
    }

    /**
     * Adds an object to the transientPropertyKeys map
     */
    public static Object registerTransientPropertyKey(Object key) {
        transientPropertyKeys.add(key);
        return key;
    }
    
    /**
     * Removes an object to the transientPropertyKeys set
     */
    public static void unregisterTransientPropertyKey(Object key) {
        transientPropertyKeys.remove(key);
    }
    
    /**
     * A set of key values that are always stored in the transient
     * map; all others are always stored in the persistent map
     */
    protected static Set<Object> transientPropertyKeys = new HashSet<Object>();

    /**
     * Get the Entity object for the given OID and namespace
     * @return The Entity corresponding to the OID and namespace, or null
     * @deprecated As of 1.5, use equivalent function on {@link EntityManager}.
     */
    public static Entity getEntityByNamespace(Long oid, Namespace namespace)
    {
        return EntityManager.getEntityByNamespace(oid,namespace);
    }
    
    /**
     * Register the entity by its OID and the Namespace passed in.
     * This method silently replaces any existing entity of the same
     * oid and namespace, because that's what the old code did.
     * @deprecated As of 1.5, use equivalent function on {@link EntityManager}.
     */
    public static void registerEntityByNamespace(Entity entity,
        Namespace namespace)
    {
        EntityManager.registerEntityByNamespace(entity,namespace);
    }

    /**
     * Unregister the entity, using its OID and the Namespace 
     * @deprecated As of 1.5, use equivalent function on {@link EntityManager}.
     */
    public static boolean removeEntityByNamespace(Entity entity,
        Namespace namespace)
    {
        return EntityManager.removeEntityByNamespace(entity.getOid(), namespace);
    }
    
    /**
     * Look up the Entity by OID and Namespace, and unregister it
     * @deprecated As of 1.5, use equivalent function on {@link EntityManager}.
     */
    public static boolean removeEntityByNamespace(Long oid,
        Namespace namespace)
    {
        return EntityManager.removeEntityByNamespace(oid, namespace);
    }
    
    /**
     * Return an array containing all the Entity objects registered in
     * the Namespace.
     * @deprecated As of 1.5, use equivalent function on {@link EntityManager}.
     */
    public static Entity[] getAllEntitiesByNamespace(Namespace namespace)
    {
        return EntityManager.getAllEntitiesByNamespace(namespace);
    }

    /**
     * @deprecated As of 1.5, use equivalent function on {@link EntityManager}.
     */
    public static int getEntityCount()
    {
        return EntityManager.getEntityCount();
    }

    /**
     * Get the subObjectNamespacesInt; exists only to provide a
     * JavaBean interface, so that the database code will persist the
     * Integer.
     */
    public Integer getSubObjectNamespacesInt() {
        return subObjectNamespacesInt;
    }
    
    /**
     * Set the subObjectNamespacesInt; exists only to provide a
     * JavaBean interface, so that the database code will persist the
     * Integer.
     */
    public void setSubObjectNamespacesInt(Integer value) {
        subObjectNamespacesInt = value;
    }
    
    /**
     * This is the set of sub-object namespaces, compressed into an
     * Integer by using one bit for each Namespace.
     */
    protected Integer subObjectNamespacesInt = null;
     
   /**
     * Logger for this object, used to log info/debug messages.
     */
    protected static final Logger log = new Logger("Entity");

    private byte namespaceByte = Namespace.transientNamespaceNumber;
    private boolean persistEntity = false;
    private transient boolean deleted = false;

    /**
     * This is the server's object id - it is unique amongst all
     * servers currently running this world
     */
    private Long oid = null;

    protected ObjectType type = ObjectTypes.unknown;

    transient private Map<String, Serializable> transientMap = null;
    
    /**
     * Dont use this, will probably be moved.  Lock for all entities map.
     */
    public static Lock staticLock = LockFactory.makeLock("EntityStaticLock");

    private static final long serialVersionUID = 1L;
}
