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
import java.io.*;
import multiverse.server.util.*;
import multiverse.server.marshalling.*;
import multiverse.server.network.*;
import multiverse.server.plugins.*;
import multiverse.mars.plugins.*;

/**
 * For each conceptual object stored in the database, every plugin has
 * the option of creating and maintaining one or more "sub-objects"
 * containing just those members that are meaningful to that plugin.
 * When it comes time to store them in the database, the plugin needs
 * some unique token in addition to the objects oid to identify the
 * each of the plugin's sub-objects.  Namespace objects provide that
 * unique token, and each plugin stores it's persistent objects in one
 * or more namespaces that only that kind of plugin uses.  Namespace
 * objects are identified by a string, which is interned to produce a
 * small integer, and the mappings between namespace strings and the
 * small integers used to represent them is stored in the database.
 * When the server starts up, each plugin reads the mapping into
 * memory, creating Namespace objects for each entry in the mapping.
 *<p>
 * Namespace objects can be created on the fly, by calling the static
 * method Namespace.intern(String name), which looks up the string in
 * the in-memory map of Namespace strings.  If the string is found, it
 * returns the associated namespace object; otherwise, it stores the
 * new namespace in the database, and creates and returns the
 * newly-interned Namespace object.
 *<p>
 * At this point, the number of unique namespaces is limited to 31,
 * because a set of namespaces must be compressed to fit integer with
 * 1 bit per namespace.
 *<p>
 * An Entity, which represents a particular plugin's sub-object,
 * "knows" what Namespace it belongs to.
 */
public class Namespace implements Marshallable, Serializable {
    
    /**
     * Marshalling requires that there be a public no-args
     * constructor, but nothing but marshalling should call this
     * constructor
     */
    public Namespace() {
    }

    /**
     * This constructor is private, since all Namespace objects should
     * be created by Namespace.intern(String name)
     */
    private Namespace(String name, int number) {
        this.name = name;
        this.number = number;
    }

    /**
     * Getter for the Namespace string name.  There is no setter for
     * the name.
n     */
    public String getName() {
        return name;
    }
    
    /**
     * Getter for the Namespace number.  There is no setter for the
     * number.
     */ 
    public int getNumber() {
        return number;
    }
    
    public String toString() {
        return "[Namespace " + name + ":" + number + "]";
    }

    /**
     * When marshalled, a Namespace object is represented by a byte
     * containing it's number.
     */
    public void marshalObject(MVByteBuffer buf) {
        buf.putByte((byte)number);
    }

    /**
     * The unmarshalling operation looks up the number read from the
     * input stream in the mapping from number to Namespace object.
     */
    public Object unmarshalObject(MVByteBuffer buf) {
        int b = (int)buf.getByte();
        Namespace ns = getNamespaceFromIntOrError(b);
        return ns;
    }

    // This produces a garbage Namespace object whose only purpose is
    // to contain the number.  ReadResolve does that actual namespace
    // lookup.
    private void readObject(ObjectInputStream in)
        throws IOException, ClassNotFoundException
    {
        number = in.readInt();
    }

    private Object readResolve()
        throws ObjectStreamException
    {
        Namespace ns = getNamespaceFromIntOrError(number);
        return ns;
    }

    private void writeObject(ObjectOutputStream out)
        throws IOException, ClassNotFoundException
    {
        out.writeInt(number);
    }

    transient private String name;
    private int number = 0;
    
    ////////////////////////////////////////////////////////////////////////
    //
    // Static members
    //
    ////////////////////////////////////////////////////////////////////////
    
    /**
     * This is the external entrypoint used to create a namespace.  It
     * looks up the string in the in-memory map of Namespace strings.
     * If the string is found, it * returns the associated namespace
     * object; otherwise, it stores the new namespace in the database,
     * and creates and returns the newly-interned Namespace object.
     */
    public static Namespace intern(String name) {
        return getOrCreateNamespace(name);
    }
    
    /**
     * This method is called by the database code to create interned
     * Namespace objects as a result of a call to
     * encacheNamespaceMapping.  It should never be called by any
     * other caller
     */
    public static Namespace addDBNamespace(String name, int number) {
        Namespace ns = new Namespace(name, number);
        namespaceStringToNamespace.put(name, ns);
        namespaceIntToNamespace.put(number, ns);
        return ns;
    }

    /**
     * This method is called by Engine to encache database-resident
     * Namespace objects.  It should never be called by any other
     * caller.
     */
    public static void encacheNamespaceMapping() {
        // Get the numerical values from the database
        if (Log.loggingDebug)
            Log.debug("Reading namespaces from the database");
        Engine.getDatabase().encacheNamespaceMapping();
        // Assign the Namespace objects
        TRANSIENT = intern("NS.transient");
        OBJECT_MANAGER = intern("NS.master");
        WORLD_MANAGER = intern("NS.wmgr");
        WorldManagerClient.NAMESPACE = WORLD_MANAGER;
        WM_INSTANCE = intern("NS.wminstance");
        WorldManagerClient.INSTANCE_NAMESPACE = WM_INSTANCE;
        COMBAT = intern("NS.combat");
        CombatClient.NAMESPACE = COMBAT;
        MOB = intern("NS.mob");
        BAG = intern("NS.inv");
        InventoryClient.NAMESPACE = BAG;
        MARSITEM = intern("NS.item");
        InventoryClient.ITEM_NAMESPACE = MARSITEM;
        QUEST = intern("NS.quest");
        PLAYERQUESTSTATES = intern("NS.playerqueststates");
        INSTANCE = intern("NS.instance");
        InstanceClient.NAMESPACE = INSTANCE;
        VOICE = intern("NS.voice");
        TRAINER = intern("NS.trainer");
        TrainerClient.NAMESPACE = TRAINER;
        CLASSABILITY = intern("NS.classability");
        ClassAbilityClient.NAMESPACE = CLASSABILITY;
        if (Log.loggingDebug)
            Log.debug("Read " + namespaceIntToNamespace.size() + " namespaces from the database");
    }
    
    /**
     * Return the namespace associated with the string argument, or
     * throw an error if no such namespace exists
     */
    public static Namespace getNamespace(String nsString) {
        Namespace ns = namespaceStringToNamespace.get(nsString);
        if (ns == null)
            throw new MVRuntimeException("Database.getNamespaceInt Did not namespace int for namespace '" + nsString + "'");
        return ns;
    }
    
    /**
     * Return the namespace associated with the string argument,
     * or null if it does not exist.
     */
    public static Namespace getNamespaceIfExists(String nsString) {
        return namespaceStringToNamespace.get(nsString);
    }
    
    /**
     * Return the namespace whose number is the Integer argument, or
     * null if no such namespace exists
     */
    public static Namespace getNamespaceFromInt(Integer nsInt) {
        return namespaceIntToNamespace.get(nsInt);
    }
    
    protected static Namespace getNamespaceFromIntOrError(Integer nsInt) {
        Namespace ns = namespaceIntToNamespace.get(nsInt);
        if (ns != null)
            return ns;
        else
            return Engine.getDatabase().findExistingNamespace(nsInt);
    }

    /**
     * Given a set of Namespace objects, compress the set into an
     * Integer, with 1 bit whose bit number is the number of the
     * Namespace, or return null if the namespace set is null or has
     * no elements.
     */
    public static Integer compressNamespaceList(Set<Namespace> namespaces) {
        if (namespaces == null || namespaces.size() == 0)
            return null;
        int result = 0;
        for (Namespace n : namespaces)
            result |= (1 << n.number);
        return result;
    }
    
    /**
     * Given an Integer with 1 bit whose bit number is the number of
     * the Namespace, decompress into a Set<Namespace>
     */
    public static List<Namespace> decompressNamespaceList(Integer namespacesInteger) {
        List<Namespace> namespaces = new LinkedList<Namespace>();
        if (namespacesInteger == null)
            return namespaces;
        int n = namespacesInteger;
        for (int i=0; i<32; i++) {
            if ((n & 1) != 0)
                namespaces.add(getNamespaceFromInt(i));
            n = n >> 1;
            if (n == 0)
                break;
        }
        return namespaces;
    }
    
    private static Namespace getOrCreateNamespace(String nsString) {
        Namespace ns = namespaceStringToNamespace.get(nsString);
        if (ns != null)
            return ns;
        else
            return createNamespace(nsString);
    }
    
    private static Namespace createNamespace(String nsString) {
        Log.info("Creating namespace '" + nsString + "'");
        return Engine.getDatabase().createNamespace(nsString);
    }
    
    private static Map<String, Namespace> namespaceStringToNamespace = new HashMap<String, Namespace>();
    private static Map<Integer, Namespace> namespaceIntToNamespace = new HashMap<Integer, Namespace>();
    
    /**
     * The transient namespace, used when the plugin's subobject
     * should not be stored persistently
     */
    public static Namespace TRANSIENT = null;
    
    /**
     * The Object Manager Plugin's namespace
     */
    public static Namespace OBJECT_MANAGER = null;
    
    /**
     * The World Manager Plugin's namespace
     */
    public static Namespace WORLD_MANAGER = null;
    
    /**
     * The Combat Plugin's namespace
     */
    public static Namespace COMBAT = null;
    
    /**
     * The Mob Manager Plugin's namespace
     */
    public static Namespace MOB = null;
    
    /**
     * The first of two Inventory Plugin namespaces, used for
     * inventory bags
     */
    public static Namespace BAG = null;
    
    /**
     * The second of two Inventory Plugin namespaces, used for
     * inventory items
     */
    public static Namespace MARSITEM = null;
    
    /**
     * The first of two Quest Plugin namespaces, used for quests
     */
    public static Namespace QUEST = null;
    
    /**
     * The second of two Quest Plugin namespaces, used for the state
     * of a player's quests
     */
    public static Namespace PLAYERQUESTSTATES = null;
    
    /**
     * InstancePlugin namespace.  See InstanceClient.
     */
    public static Namespace INSTANCE = null;

    /**
     * World manager instance namespace.  See WorldManagerClient.
     */
    public static Namespace WM_INSTANCE = null;

    /**
     * A namespace for the voice plugin
     */
    public static Namespace VOICE = null;
    
    /**
     * The Trainer Plugin's namespace
     */
    public static Namespace TRAINER = null;
    
    /**
     * The ClassAbility Plugin's namespace
     */
    public static Namespace CLASSABILITY = null;
    
    /**
     * The number of the transient namespace is hard-wired to 1
     */
    public static final int transientNamespaceNumber = 1;
    
    public static final long serialVersionUID = 1L;

}
