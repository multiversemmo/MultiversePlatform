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

import java.util.List;
import java.util.Collection;

import multiverse.msgsys.*;
import multiverse.server.math.Point;
import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.server.messages.*;
import multiverse.server.engine.*;

/** Interface to the object manager plugin.  The object manager
    can create, load, unload, and delete distributed objects.
    Objects can be persistent or non-persistent.
    <p>
    The object manager also provides a mechanism for saving
    non-distributed object data. 
 */
public class ObjectManagerClient {

    /** Create an object.  The object properties are initialized from
        the registered {@code templateName} merged with the {@code overrideTemplate}.
        Properties in the override template take precedence over those
        in the registered template.
        <p>
        To create a persistent object, set the persistent property to true.
        <pre>
            Java:
            template.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, true)
            Python:
            template.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True))
        </pre>
        <p>
        Perceivable objects (those with a location) must have an
        instance oid property (Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, type Long).
        @param templateName Registered template.  If you don't want to use
        a registered template, then pass ObjectManagerClient.BASE_TEMPLATE.
        @param overrideTemplate Override template, may be null.
        @return Object oid on success, null on failure.
     */
    public static Long generateObject(String templateName, Template overrideTemplate) {
        Message msg = new GenerateObjectMessage(templateName, overrideTemplate);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }
    
    /** Create an object.  The object properties are initialized from
        the registered {@code templateName}.  The object location is set
        via {@code loc}.  To make the object perceivable, the caller
        must spawn the object in the world manager.
        @param templateName Registered template.
        @param loc Object location.
        @return Object oid on success, null on failure.
        @see #generateObject(String,Template)
    */
    public static Long generateObject(String templateName, Point loc) {
        Template override = new Template();
        if (loc != null)
            override.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_LOC, loc);
        Message msg = new GenerateObjectMessage(templateName, override);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }
    
    /** Create an object.  The object properties are initialized from
        the registered {@code templateName}.  The object location is set
        via {@code instanceOid} and {@code loc}.  To make the object perceivable, the caller
        must spawn the object in the world manager.
        @param templateName Registered template.
        @param instanceOid Object instance.
        @param loc Object location.
        @return Object oid on success, null on failure.
        @see #generateObject(String,Template)
    */
    public static Long generateObject(String templateName, long instanceOid,
        Point loc)
    {
        Template override = new Template();
        override.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC, loc);
        override.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, instanceOid);
        Message msg = new GenerateObjectMessage(templateName, override);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }
    
    /** Create a point light object.
        @param instanceOid Instance oid.
        @param lightData Point light data.
        @return Light object oid, or null on failure
     */
    public static Long generateLight(long instanceOid, LightData lightData)
    {
        Template template = new Template(lightData.getName());
        template.put(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_OBJECT_TYPE,
                WorldManagerClient.TEMPL_OBJECT_TYPE_LIGHT);
        template.put(Namespace.WORLD_MANAGER,
            Light.LightDataPropertyKey, lightData);
        template.put(Namespace.WORLD_MANAGER,
            WorldManagerClient.TEMPL_LOC, lightData.getInitLoc());
        template.put(Namespace.WORLD_MANAGER,
            WorldManagerClient.TEMPL_INSTANCE, instanceOid);
        return ObjectManagerClient.generateObject(BASE_TEMPLATE, template);
    }
    
    /** Load a persistent object.  The master object is loaded from the
        database, then each of the sub-objects.  If the object is already
        loaded, then nothing is done and the call returns success.  If the
        object is partially loaded, then the missing sub-objects are loaded.
        If the object is in an instance, the instance is automatically
        loaded prior to loading sub-objects.
        @return Oid of object on success, null on failure
     */
    public static Long loadObject(Long oid) {
        return loadSubObject(oid,null);
    }

    /** Load a persistent object by persistence key.  The object is
        selected by persistence key instead of OID.  Otherwise behaves
        the same as loadObject(Long).  
        @return Oid of object on success, null on failure.
     */
    public static Long loadObject(String key) {
        LoadObjectMessage msg = new LoadObjectMessage(key);
        Long oid = Engine.getAgent().sendRPCReturnLong(msg);
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient.loadObject: key=" + key + ", oid=" + oid);
        return oid;
    }
    
    /** Unload an object.  The sub-objects are unloaded, then the
        master object is unloaded.  All memory used by the object should
        be released by this call.  If the object is persistent, it will
        be saved before unloading.
        <p>
        If the object is an instance,
        contained objects (excluding players) will be unloaded.
        However, the correct way to unload an instance is
        {@link InstanceClient#unloadInstance(long)}.
        @return True on success, false on failure.
     */
    public static Boolean unloadObject(Long oid)
    {
        return unloadSubObject(oid,null);
    }

    public static Long loadSubObject(long oid,
        Collection<Namespace> namespaces)
    {
        LoadObjectMessage msg = new LoadObjectMessage(oid, namespaces);
        Long respOid = Engine.getAgent().sendRPCReturnLong(msg);
        if (Log.loggingDebug) {
            String nsString = "null";
            if (namespaces != null) {
                nsString = "";
                for (Namespace ns : namespaces)
                    nsString += ns;
            }
            Log.debug("ObjectManagerClient.loadSubObject: oid=" + oid +
                " ns=" + nsString +
                ", received response oid " + respOid);
        }
        return respOid;
    }

    public static Boolean unloadSubObject(long oid,
        Collection<Namespace> namespaces)
    {
        UnloadObjectMessage msg = new UnloadObjectMessage(oid, namespaces);
        Boolean rc = Engine.getAgent().sendRPCReturnBoolean(msg);
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient.unloadSubObject: oid=" + oid
                      + ", received response " + rc);
        return rc;
    }

    /** Delete an object.  The sub-objects are deleted, then the master
        object is deleted.  Persistent objects are deleted from the
        database (master object and all sub-objects).  Does not delete
        instance contents.  To delete an instance use
        {@link InstanceClient#deleteInstance(long)}.
        @return True on success, false on failure.
     */
    public static Boolean deleteObject(Long oid) {

        DeleteObjectMessage msg = new DeleteObjectMessage(oid);
        Boolean rc = Engine.getAgent().sendRPCReturnBoolean(msg);
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient.deleteObject: oid=" + oid
                      + ", received response " + rc);
        return rc;
    }

    /** Save persistent object with persistence key.  The sub-objects are
        saved, then the master object is saved.  The call returns when
        the save is complete.  The call does nothing for non-persistent
        objects.
        @param oid Object identifier
        @param persistenceKey Unique object identifier.  Replaces previous
                persistence key.
        @return True on success, false on failure.
     */
    public static boolean saveObject(Long oid, String persistenceKey) {
        SaveObjectMessage msg = new SaveObjectMessage(oid, persistenceKey);
        return Engine.getAgent().sendRPCReturnBoolean(msg);
    }
    
    /** Save persistent object.  The sub-objects are
        saved, then the master object is saved.  The call returns when
        the save is complete.  The call does nothing for non-persistent
        objects.
        @return True on success, false on failure.
     */
    public static boolean saveObject(Long oid) {
        SaveObjectMessage msg = new SaveObjectMessage(oid);
        return Engine.getAgent().sendRPCReturnBoolean(msg);
    }
    
    /** Save a sub-object.  The supplied Entity is saved in the database
        under the given namespace (which identifies the sub-object).  The
        entity must have an oid.  Only the Beans Serializable portions of the
        Entity are saved.  The Entity persistence flag is ignored; this call
        always saves the Entity.
        @param persistenceKey Object persistence key, null for no key
        @param entity Entity to save
        @param namespace Entity's namespace
        @return True on success, false on failure.
     */
    public static boolean saveObjectData(String persistenceKey, Entity entity,
        Namespace namespace)
    {
        byte[] entityData;
        entity.lock();
        try {
            entityData = entity.toBytes();
        }
        finally {
            entity.unlock();
        }
        SaveObjectDataMessage msg = new SaveObjectDataMessage(entity.getOid(),
            persistenceKey, entityData, namespace);
        return Engine.getAgent().sendRPCReturnBoolean(msg);
    }
    
    /** Load persistent entity from the database by persistence key.
        @param persistenceKey Object persistence key.
        @return The loaded Entity, or null on failure
     */
    public static Entity loadObjectData(String persistenceKey) {
        LoadObjectDataMessage msg = new LoadObjectDataMessage(persistenceKey);
        return (Entity)Engine.getAgent().sendRPCReturnObject(msg);
    }
    
    /** Load persistent sub-object.  Loads the sub-object from the database,
        but does not register the master or sub-objects.

        @param oid The object oid.
        @param namespace The sub-object namespace.
        @return The loaded Entity, or null on failure
     */
    public static Entity loadObjectData(Long oid, Namespace namespace) {
        LoadObjectDataMessage msg = new LoadObjectDataMessage(oid, namespace);
        return (Entity)Engine.getAgent().sendRPCReturnObject(msg);
    }
    
    /** Register object property template.  The template name is taken
        from Template.getName().  Replaces an existing template with the
        same name.  Registered templates are used to create objects.
        @return True on success, false on failure
        @see #generateObject(String,Template)
     */
    public static boolean registerTemplate(Template template) {
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient: registering template: " + template);
        Message msg = new RegisterTemplateMessage(template);
        Boolean rv = Engine.getAgent().sendRPCReturnBoolean(msg);
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient: registered template: " + template);
        return rv;
    }

    /** Get object property template.
        @return Template or null if it does not exist.
     */
    public static Template getTemplate(String templateName) {
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient: get template: " + templateName);
        Message msg = new GetTemplateMessage(templateName);
        Template template = (Template)Engine.getAgent().sendRPCReturnObject(msg);
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient: got template: " + template);
        return template;
    }

    /** Set object persistence flag.  Objects that are persistent will be
        saved by the persistence manager.  This will set the persistence
        flag on the master object (in the object manager plugin) and
        the object's sub-objects.
     */
    public static void setPersistenceFlag(Long oid, boolean flag) {
        SetPersistenceMessage msg = new SetPersistenceMessage(oid, flag);
        Engine.getAgent().sendRPC(msg);
    }

    /** Modify location of persistent object.  The object's world manager
        sub-object is directly modified in the database.  The object
        location, orientation, and direction can be independently modified.
        Null values in the @p worldNode are ignored.
        <p>
        This call should only be used when the object's world manager
        sub-object is NOT loaded.  The ProxyPlugin uses fixWorldNode()
        during player login when their logout instance no longer exists.
        @return True on success, false on failure.
    */
    public static boolean fixWorldNode(long oid, BasicWorldNode worldNode) {
        FixWorldNodeMessage message = new FixWorldNodeMessage(oid, worldNode);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    /** Get registered template names.
        @return List of template names.
    */
    public static List<String> getTemplateNames()
    {
        Message message = new Message();
        message.setMsgType(MSG_TYPE_GET_TEMPLATE_NAMES);
        List<String> templateNames =
            (List<String>) Engine.getAgent().sendRPCReturnObject(message);
        if (Log.loggingDebug)
            Log.debug("ObjectManagerClient: got "
                + templateNames.size() + " template names");
        return templateNames;
    }

    /** Get object id by name.  If <code>instanceOid</code> is non-null,
        the lookup is confined to the instance content.  Only works on
        loaded instances and loaded objects.  If <code>instanceOid</code>
        is null, lookup considers all loaded objects.
        <p>
        If <code>objectType</code> is non-null, both the name and
        object type must match.
        <p>
        Object names are set at creation time from
        the WorldManager object name (WorldManagerClient.TEMPL_NAME) or
        the Template name.
        <p>
        NOTE: Object lookup is currently an O(N) operation.
        @param instanceOid Instance oid, may be null.
        @param name Object name.
        @param objectType Object type, may be null.
        @return Object oid or null if instance or named object are not found.
    */
    public static Long getNamedObject(Long instanceOid, String name,
        ObjectType objectType)
    {
        GetNamedObjectMessage message = new GetNamedObjectMessage(
            instanceOid, name, objectType);
        return Engine.getAgent().sendRPCReturnLong(message);
    }

    /** Get object status.  Returns information about the object including
        name, type, and sub-object namespaces.  See {@link ObjectStatus}
        for a complete list.  If the object is loaded, then
        {@link ObjectStatus#namespaces ObjectStatus.namespaces} will be non-null and
        {@link ObjectStatus#loadedNamespaces ObjectStatus.loadedNamespaces} will contain the currently loaded
        namespaces.  If the object is not loaded, then {@link ObjectStatus#namespaces ObjectStatus.namespaces}
        will be null, but {@link ObjectStatus#name ObjectStatus.name} will contain the object
        name (if it has one), if the object exists.
    */
    public static ObjectStatus getObjectStatus(long oid)
    {
        SubjectMessage message = new SubjectMessage(
            MSG_TYPE_GET_OBJECT_STATUS, oid);
        return (ObjectStatus) Engine.getAgent().sendRPCReturnObject(message);
    }

    /**
     * can be either based on oid (which is stored in data) or
     * key which is stored in a data member
     * one and only one must be non-null.
     * @author cedeno
     *
     */
    public static class LoadObjectMessage extends Message {

        public LoadObjectMessage() {
            super(MSG_TYPE_LOAD_OBJECT);
        }

        public LoadObjectMessage(Long oid) {
            super(MSG_TYPE_LOAD_OBJECT);
            setOid(oid);
        }

        public LoadObjectMessage(Long oid, Collection<Namespace> namespaces)
        {
            super(MSG_TYPE_LOAD_OBJECT);
            setOid(oid);
            setNamespaces(namespaces);
        }

        public LoadObjectMessage(String key) {
            super(MSG_TYPE_LOAD_OBJECT);
            setKey(key);
        }

        public Long getOid() {
            return oid;
        }

        public void setOid(Long oid) {
            this.oid = oid;
        }

        public Collection<Namespace> getNamespaces() {
            return namespaces;
        }

        public void setNamespaces(Collection<Namespace> namespaces) {
            this.namespaces = namespaces;
        }

        public void setKey(String key) {
            this.key = key;
        }
        public String getKey() {
            return key;
        }
        
        private String key;
        private Long oid;
        private Collection<Namespace> namespaces;
        
        private static final long serialVersionUID = 1L;
    }

    /**
     * Unload object based on OID.  The object and it sub-objects are removed
     * from memory, but may still exist in the database.
     */
    public static class UnloadObjectMessage extends Message {

        public UnloadObjectMessage() {
            super(MSG_TYPE_UNLOAD_OBJECT);
        }

        public UnloadObjectMessage(Long oid) {
            super(MSG_TYPE_UNLOAD_OBJECT);
            setOid(oid);
        }

        public UnloadObjectMessage(Long oid, Collection<Namespace> namespaces)
        {
            super(MSG_TYPE_UNLOAD_OBJECT);
            setOid(oid);
            setNamespaces(namespaces);
        }

        public Long getOid() {
            return oid;
        }

        public void setOid(Long oid) {
            this.oid = oid;
        }

        public Collection<Namespace> getNamespaces() {
            return namespaces;
        }

        public void setNamespaces(Collection<Namespace> namespaces) {
            this.namespaces = namespaces;
        }

        private long oid;
        private Collection<Namespace> namespaces;
        
        private static final long serialVersionUID = 1L;
    }

    /**
     * Delete object based on OID.  The object and it sub-objects are removed
     * from memory and the database.
     */
    public static class DeleteObjectMessage extends Message {

        public DeleteObjectMessage() {
            super(MSG_TYPE_DELETE_OBJECT);
        }

        public DeleteObjectMessage(Long oid) {
            super(MSG_TYPE_DELETE_OBJECT);
            setOid(oid);
        }

        public Long getOid() {
            return oid;
        }

        public void setOid(Long oid) {
            this.oid = oid;
        }
        
        private long oid;
        
        private static final long serialVersionUID = 1L;
    }

    public static class SaveObjectMessage extends Message {

        public SaveObjectMessage() {
            super(MSG_TYPE_SAVE_OBJECT);
        }

        public SaveObjectMessage(Long oid) {
            super();
            setMsgType(MSG_TYPE_SAVE_OBJECT);
            setOid(oid);
        }
        
        public SaveObjectMessage(Long oid, String key) {
            super();
            setMsgType(MSG_TYPE_SAVE_OBJECT);
            setKey(key);
            setOid(oid);
        }
        public Long getOid() {
            return oid;
        }

        public void setOid(Long oid) {
            this.oid = oid;
        }
        public void setKey(String key) {
            this.key = key;
        }
        public String getKey() {
            return key;
        }
        
        private Long oid;
        private String key;
        
        private static final long serialVersionUID = 1L;
    }
    
    public static class SaveObjectDataMessage extends SubjectMessage {
        public SaveObjectDataMessage() {
            super(MSG_TYPE_SAVE_OBJECT_DATA);
        }
        SaveObjectDataMessage(Long oid, String persistenceKey, byte[] data, Namespace namespace) {
            super(MSG_TYPE_SAVE_OBJECT_DATA, oid);
            setDataBytes(data);
            setKey(persistenceKey);
            setNamespace(namespace);
        }
        public void setDataBytes(byte [] dataBytes) {
            this.dataBytes = dataBytes;
        }
        public byte[]  getDataBytes() {
            return (byte[]) dataBytes;
        }

        public void setKey(String key) {
            this.key = key;
        }
        public String getKey() {
            return key;
        }
        
        public Namespace getNamespace() {
            return namespace;
        }
        public void setNamespace(Namespace namespace) {
            this.namespace = namespace;
        }

        String key;
        Object dataBytes;
        Namespace namespace;
        
        private static final long serialVersionUID = 1L;
    }

    public static class LoadObjectDataMessage extends SubjectMessage {
        public LoadObjectDataMessage() {
            super(MSG_TYPE_LOAD_OBJECT_DATA);
        }
        public LoadObjectDataMessage(Long oid, Namespace namespace) {
            super(MSG_TYPE_LOAD_OBJECT_DATA, oid);
            setNamespace(namespace);
        }
        public LoadObjectDataMessage(String persistenceKey) {
            super(MSG_TYPE_LOAD_OBJECT_DATA);
            setKey(persistenceKey);
        }
        public void setKey(String key) {
            this.key = key;
        }
        public String getKey() {
            return key;
        }
        
        public Namespace getNamespace() {
            return namespace;
        }
        
        public void setNamespace(Namespace namespace) {
            this.namespace = namespace;
        }

        private String key;
        private Namespace namespace;

        private static final long serialVersionUID = 1L;
    }
    
    public static class GenerateObjectMessage extends Message {

        public GenerateObjectMessage() {
            super(MSG_TYPE_GENERATE_OBJECT);
        }

        GenerateObjectMessage(String templateName) {
            super();
            setMsgType(MSG_TYPE_GENERATE_OBJECT);
            setTemplateName(templateName);
        }

        GenerateObjectMessage(String templateName, Template overrideTemplate) {
            this(templateName);
            setOverrideTemplate(overrideTemplate);
        }
        
        public String getTemplateName() {
            return templateName;
        }

        public void setTemplateName(String templateName) {
            this.templateName = templateName;
        }
        
        public void setOverrideTemplate(Template t) {
            this.overrideTemplate = t;
        }
        
        public Template getOverrideTemplate() {
            return overrideTemplate;
        }

        String templateName;
        Template overrideTemplate = null;

        private static final long serialVersionUID = 1L;
    }

    /**
     * the object manager plugin is saying that
     * the plugin registered for 'namespace' has all its dependencies
     * satisfied for the given object
     * @author cedeno
     *
     */
    public static class SubObjectDepsReadyMessage extends OIDNamespaceMessage {

        public SubObjectDepsReadyMessage() {
            super(MSG_TYPE_SUB_OBJECT_DEPS_READY);
        }
       
        public SubObjectDepsReadyMessage(Long masterOid, Namespace namespace) {
            super(MSG_TYPE_SUB_OBJECT_DEPS_READY, masterOid);
            setNamespace(namespace);
        }
       
        private static final long serialVersionUID = 1L;
    }
    
    public static class GenerateSubObjectMessage extends OIDNamespaceMessage {

        public GenerateSubObjectMessage() {
            super(MSG_TYPE_GENERATE_SUB_OBJECT);
        }

        public GenerateSubObjectMessage(Long oid, Namespace namespace, Template template) {
            super(MSG_TYPE_GENERATE_SUB_OBJECT, oid, namespace);
            setTemplate(template);
        }

        public void setTemplate(Template t) {
            this.template = t;
        }
        public Template getTemplate() {
            return template;
        }

        /** @deprecated Use getOid() */
        public Long getMasterOid() {
            return getSubject();
        }
        /** @deprecated Use setOid() */
        public void setMasterOid(Long masterOid) {
            setSubject(masterOid);
        }

        public boolean getPersistenceFlag() {
            return persistent;
        }

        public void setPersistenceFlag(boolean flag) {
            persistent = flag;
        }

        Template template = null;
        boolean persistent = false;

        private static final long serialVersionUID = 1L;
    }

    public static class LoadSubObjectMessage extends OIDNamespaceMessage {

        public LoadSubObjectMessage() {
            super(MSG_TYPE_LOAD_SUBOBJECT);
        }

        public LoadSubObjectMessage(long oid, Namespace namespace) {
            super(MSG_TYPE_LOAD_SUBOBJECT, oid, namespace);
        }

        /** @deprecated Use getOid() */
        public Long getMasterOid() {
            return getSubject();
        }
        /** @deprecated Use setOid() */
        public void setMasterOid(long oid) {
            setSubject(oid);
        }
        
        private static final long serialVersionUID = 1L;
    }
    
    public static class UnloadSubObjectMessage extends OIDNamespaceMessage {

        public UnloadSubObjectMessage() {
            super(MSG_TYPE_UNLOAD_SUBOBJECT);
        }

        public UnloadSubObjectMessage(Long oid, Namespace namespace) {
            super(MSG_TYPE_UNLOAD_SUBOBJECT, oid, namespace);
        }

        private static final long serialVersionUID = 1L;
    }
    
    public static class DeleteSubObjectMessage extends OIDNamespaceMessage {

        public DeleteSubObjectMessage() {
            super(MSG_TYPE_DELETE_SUBOBJECT);
        }

        public DeleteSubObjectMessage(Long oid, Namespace namespace) {
            super(MSG_TYPE_DELETE_SUBOBJECT, oid, namespace);
        }

        private static final long serialVersionUID = 1L;
    }

    public static class RegisterTemplateMessage extends Message {

        public RegisterTemplateMessage() {
            super(MSG_TYPE_REGISTER_TEMPLATE);
        }

        RegisterTemplateMessage(Template template) {
            super();
            setMsgType(MSG_TYPE_REGISTER_TEMPLATE);
            setTemplate(template);
        }

        public Template getTemplate() {
            return template;
        }

        public void setTemplate(Template template) {
            this.template = template;
        }

        private Template template = null;
        
        private static final long serialVersionUID = 1L;
    }

    public static class GetTemplateMessage extends Message {

        public GetTemplateMessage() {
            super(MSG_TYPE_GET_TEMPLATE);
        }

        GetTemplateMessage(String templateName) {
            super();
            setMsgType(MSG_TYPE_GET_TEMPLATE);
            setTemplateName(templateName);
        }

        public String getTemplateName() {
            return templateName;
        }

        public void setTemplateName(String templateName) {
            this.templateName = templateName;
        }

        private String templateName = null;
        
        private static final long serialVersionUID = 1L;
    }

    public static class SetPersistenceMessage extends OIDNamespaceMessage {

        public SetPersistenceMessage() {
            super(MSG_TYPE_SET_PERSISTENCE);
        }

        public SetPersistenceMessage(Long oid, Boolean persistVal) {
            super(MSG_TYPE_SET_PERSISTENCE, oid);
            setPersistVal(persistVal);
        }

        public SetPersistenceMessage(Long oid, Namespace namespace, Boolean persistVal) {
            super(MSG_TYPE_SET_PERSISTENCE, oid, namespace);
            setPersistVal(persistVal);
        }

        public Boolean getPersistVal() {
            return persistVal;
        }

        public void setPersistVal(Boolean persistVal) {
            this.persistVal = persistVal;
        }
        
        private Boolean persistVal;
        
        private static final long serialVersionUID = 1L;
    }

    public static class SetSubPersistenceMessage extends SetPersistenceMessage {
        public SetSubPersistenceMessage() {
            super();
            setMsgType(MSG_TYPE_SET_SUBPERSISTENCE);
        }

        public SetSubPersistenceMessage(Long oid, Namespace namespace, Boolean persistVal) {
            super(oid, namespace, persistVal);
            setMsgType(MSG_TYPE_SET_SUBPERSISTENCE);
        }

        private static final long serialVersionUID = 1L;
    }

    public static class FixWorldNodeMessage extends Message
    {
        public FixWorldNodeMessage() {
        }

        public FixWorldNodeMessage(long oid, BasicWorldNode worldNode)
        {
            super(MSG_TYPE_FIX_WNODE_REQ);
            setOid(oid);
            setWorldNode(worldNode);
        }

        public long getOid() {
            return oid;
        }
        public void setOid(long oid) {
            this.oid = oid;
        }

        public BasicWorldNode getWorldNode() {
            return worldNode;
        }
        public void setWorldNode(BasicWorldNode worldNode) {
            this.worldNode = worldNode;
        }

        private long oid;
        private BasicWorldNode worldNode;
        
        private static final long serialVersionUID = 1L;
    }

    public static class GetNamedObjectMessage extends Message
    {
        public GetNamedObjectMessage()
        {
        }

        public GetNamedObjectMessage(Long instanceOid, String name,
            ObjectType objectType)
        {
            super(MSG_TYPE_GET_NAMED_OBJECT);
            this.instanceOid = instanceOid;
            this.name = name;
            this.objectType = objectType;
        }

        public Long getInstanceOid()
        {
            return instanceOid;
        }

        public String getName()
        {
            return name;
        }

        public ObjectType getObjectType()
        {
            return objectType;
        }

        private Long instanceOid;
        private String name;
        private ObjectType objectType;
        
        private static final long serialVersionUID = 1L;
    }

    /** Object status information.
    */
    public static class ObjectStatus
    {
        public ObjectStatus()
        {
        }

        /** Object oid. */
        public long oid;

        /** Object name, if it has one.  Set to null if the object does not
            have a name or does not exist.
        */
        public String name;

        /** Object type. */
        public ObjectType type;

        /** True if the object is persistent. */
        public boolean persistent;

        /** Sub-object namespaces.  Set to null if the object is not loaded
            or does not exist.
        */
        public List<Namespace> namespaces;

        /** Loaded sub-object namespaces.
        */
        public List<Namespace> loadedNamespaces;

        private static final long serialVersionUID = 1L;
    }


    // msg "type" field for loading an object request
    public static final MessageType MSG_TYPE_SET_PERSISTENCE = MessageType.intern("mv.SET_PERSISTENCE");
    
    // for persisting sub objects
    public static final MessageType MSG_TYPE_SET_SUBPERSISTENCE = MessageType.intern("mv.SET_SUBPERSISTENCE");
    
    public static final MessageType MSG_TYPE_LOAD_OBJECT = MessageType.intern("mv.LOAD_OBJECT");
    public static final MessageType MSG_TYPE_LOAD_SUBOBJECT = MessageType.intern("mv.LOAD_SUBOBJECT");
    public static final MessageType MSG_TYPE_UNLOAD_OBJECT = MessageType.intern("mv.UNLOAD_OBJECT");
    public static final MessageType MSG_TYPE_UNLOAD_SUBOBJECT = MessageType.intern("mv.UNLOAD_SUBOBJECT");
    public static final MessageType MSG_TYPE_DELETE_OBJECT = MessageType.intern("mv.DELETE_OBJECT");
    public static final MessageType MSG_TYPE_DELETE_SUBOBJECT = MessageType.intern("mv.DELETE_SUBOBJECT");

    public static final MessageType MSG_TYPE_LOAD_OBJECT_DATA = MessageType.intern("mv.LOAD_OBJECT_DATA");
    public static final MessageType MSG_TYPE_SAVE_OBJECT_DATA = MessageType.intern("mv.SAVE_OBJECT_DATA");
    public static final MessageType MSG_TYPE_SAVE_OBJECT = MessageType.intern("mv.SAVE_OBJECT");
    public static final MessageType MSG_TYPE_SAVE_SUBOBJECT = MessageType.intern("mv.SAVE_SUBOBJECT");

    public static final MessageType MSG_TYPE_GENERATE_OBJECT = MessageType.intern("mv.GENERATE_OBJECT");
    public static final MessageType MSG_TYPE_GENERATE_SUB_OBJECT = MessageType.intern("mv.GENERATE_SUB_OBJECT");
    public static final MessageType MSG_TYPE_SUB_OBJECT_DEPS_READY = MessageType.intern("mv.SUB_OBJECT_DEPS_READY");
    public static final MessageType MSG_TYPE_REGISTER_TEMPLATE = MessageType.intern("mv.REGISTER_TEMPLATE");
    public static final MessageType MSG_TYPE_GET_TEMPLATE = MessageType.intern("mv.GET_TEMPLATE");
    public static final MessageType MSG_TYPE_GET_TEMPLATE_NAMES = MessageType.intern("mv.GET_TEMPLATE_NAMES");

    public static final MessageType MSG_TYPE_FIX_WNODE_REQ = MessageType.intern("mv.FIX_WNODE_REQ");

    public static final MessageType MSG_TYPE_GET_NAMED_OBJECT = MessageType.intern("mv.GET_NAMED_OBJECT");

    public static final MessageType MSG_TYPE_GET_OBJECT_STATUS = MessageType.intern("mv.GET_OBJECT_STATUS");


    /** Name of an empty template.  The base template is always registered.
        @see #generateObject(String,Template)
     */
    public static final String BASE_TEMPLATE = "BaseTemplate";

    public static final String TEMPL_PERSISTENT = ":persistent";
    public static final String TEMPL_INSTANCE_RESTORE_STACK = "instanceStack";
    public static final String TEMPL_CURRENT_INSTANCE_NAME = "currentInstanceName";

}
