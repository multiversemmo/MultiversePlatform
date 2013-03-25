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
import java.io.Serializable;

import multiverse.msgsys.*;
import multiverse.management.Management;
import multiverse.server.objects.*;
import multiverse.server.plugins.ObjectManagerClient.GenerateSubObjectMessage;
import multiverse.server.plugins.ObjectManagerClient.GetNamedObjectMessage;
import multiverse.server.plugins.ObjectManagerClient.ObjectStatus;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.messages.*;
import multiverse.server.math.Point;

/**
 * handles creating from factory, loading from database, spawning, despawning,
 * persisting to database, of objects
 * 
 * see ObjectManagerClient for api to access this plugin
 */
public class ObjectManagerPlugin extends multiverse.server.engine.EnginePlugin {

    public ObjectManagerPlugin() {
        super("ObjectManager");
        setPluginType("ObjectManager");
    }

    public void onActivate() {
        try {
            log.debug("ObjectManagerPlugin.onActivate started");

            templateManager.register(ObjectManagerClient.BASE_TEMPLATE, 
                    new Template(ObjectManagerClient.BASE_TEMPLATE));
            
            registerHooks();

            // If we want several objmgr instances, we can filter on oid
            // with some hash function (for load and save).
            // The object manager is a responder for all these message
            // types.  If a non-RPC message type is added, a second
            // subscription must created without the RESPONDER flag.
            MessageTypeFilter filter = new MessageTypeFilter();
            filter.addType(ObjectManagerClient.MSG_TYPE_SET_PERSISTENCE);
            filter.addType(ObjectManagerClient.MSG_TYPE_LOAD_OBJECT);
            filter.addType(ObjectManagerClient.MSG_TYPE_UNLOAD_OBJECT);
            filter.addType(ObjectManagerClient.MSG_TYPE_DELETE_OBJECT);
            filter.addType(ObjectManagerClient.MSG_TYPE_LOAD_OBJECT_DATA);
            filter.addType(ObjectManagerClient.MSG_TYPE_SAVE_OBJECT);
            filter.addType(ObjectManagerClient.MSG_TYPE_SAVE_OBJECT_DATA);
            filter.addType(ObjectManagerClient.MSG_TYPE_GENERATE_OBJECT);
            filter.addType(ObjectManagerClient.MSG_TYPE_REGISTER_TEMPLATE);
            filter.addType(ObjectManagerClient.MSG_TYPE_GET_TEMPLATE);
            filter.addType(ObjectManagerClient.MSG_TYPE_GET_TEMPLATE_NAMES);
            filter.addType(ObjectManagerClient.MSG_TYPE_FIX_WNODE_REQ);
            filter.addType(InstanceClient.MSG_TYPE_UNLOAD_INSTANCE);
            filter.addType(InstanceClient.MSG_TYPE_DELETE_INSTANCE);
            filter.addType(InstanceClient.MSG_TYPE_LOAD_INSTANCE_CONTENT);
            filter.addType(ObjectManagerClient.MSG_TYPE_GET_NAMED_OBJECT);
            filter.addType(Management.MSG_TYPE_GET_PLUGIN_STATUS);
            filter.addType(ObjectManagerClient.MSG_TYPE_GET_OBJECT_STATUS);
            Engine.getAgent().createSubscription(filter, this,
                MessageAgent.RESPONDER);

            List<Namespace> namespaces = new ArrayList<Namespace>();
            namespaces.add(Namespace.OBJECT_MANAGER);
            registerPluginNamespaces(namespaces, null);

            log.debug("onActivate completed");
        } catch (Exception e) {
            throw new MVRuntimeException("activate failed", e);
        }
    }

    protected void registerHooks()
    {
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_SET_PERSISTENCE, 
                new SetPersistenceHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_LOAD_OBJECT,
                new LoadObjectHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_UNLOAD_OBJECT,
                new UnloadObjectHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_DELETE_OBJECT,
                new DeleteObjectHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_LOAD_OBJECT_DATA,
                new LoadObjectDataHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_SAVE_OBJECT_DATA,
                new SaveObjectDataHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_SAVE_OBJECT,
                new SaveObjectHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_GENERATE_OBJECT,
                new GenerateObjectHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_REGISTER_TEMPLATE,
                new RegisterTemplateHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_GET_TEMPLATE,
                new GetTemplateHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_GET_TEMPLATE_NAMES,
                new GetTemplateNamesHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_FIX_WNODE_REQ,
                new FixWorldNodeHook());

        getHookManager().addHook(InstanceClient.MSG_TYPE_UNLOAD_INSTANCE,
                new UnloadInstanceHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_DELETE_INSTANCE,
                new DeleteInstanceHook());
        getHookManager().addHook(InstanceClient.MSG_TYPE_LOAD_INSTANCE_CONTENT,
                new LoadInstanceContentHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_GET_NAMED_OBJECT,
                new GetNamedObjectHook());
        getHookManager().addHook(Management.MSG_TYPE_GET_PLUGIN_STATUS,
                new GetPluginStatusHook());
        getHookManager().addHook(ObjectManagerClient.MSG_TYPE_GET_OBJECT_STATUS,
                new GetObjectStatusHook());
    }

    // load mob from database
    // send response msg
    class LoadObjectHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.LoadObjectMessage msg = (ObjectManagerClient.LoadObjectMessage) m;
            Long oid = msg.getOid();
            
            String persistenceKey = null; 
            
            // this will point to the entity loaded from the db
            MasterObject entity = null;
            if (oid == null) {
                // see if this is a persistence_key lookup
                persistenceKey = msg.getKey();
                if (persistenceKey == null) {
                    Log.warn("LoadObjectHook: no key or oid");
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;
                }
                
                // Load from the database
                Entity temp = Engine.getDatabase().loadEntity(persistenceKey);
                if (temp == null) {
                    log.error("LoadObjectHook: unknown object, key="+
                        persistenceKey);
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;
                }
                if (!(temp instanceof MasterObject) ||
                        temp.getSubObjectNamespacesInt() == null) {
                    log.error("LoadObjectHook: not a master object, key="+
                        persistenceKey + " oid=" + temp.getOid());
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;
                }

                // Check if entity already loaded/loading
                entity = (MasterObject) EntityManager.getEntityByNamespace(
                    temp.getOid(), Namespace.OBJECT_MANAGER);
                if (entity != null) {
                    if (entity.loadComplete()) {
                        Log.debug("LoadObjectHook: object already loaded oid=" +
                            oid + " entity=" + entity);
                        Engine.getAgent().sendLongResponse(msg, oid);
                        return false;
                     }
                }
                else {
                    entity = (MasterObject) temp;
                    EntityManager.registerEntityByNamespace(entity,
                        Namespace.OBJECT_MANAGER);
                }

                oid = entity.getOid();
            }
            else {
                if (Log.loggingDebug)
                    log.debug("LoadObjectHook: master oid=" + oid);

                entity = (MasterObject) EntityManager.getEntityByNamespace(oid,
                    Namespace.OBJECT_MANAGER);
                if (entity != null) {
                    if (entity.loadComplete()) {
                        Log.debug("LoadObjectHook: object already loaded oid=" +
                            oid + " entity=" + entity);
                        Engine.getAgent().sendLongResponse(msg, oid);
                        return false;
                     }
                }
                else {
                    // load from database
                    entity = (MasterObject) Engine.getDatabase().loadEntity(oid,
                        Namespace.OBJECT_MANAGER);
                    // register the entity
                    if (entity != null)
                        EntityManager.registerEntityByNamespace(entity,
                            Namespace.OBJECT_MANAGER);
                }
            }
            
            if (entity == null || entity.isDeleted()) {
                log.error("LoadObjectHook: no such entity with oid " + oid +
                    " or key " + persistenceKey);
                Engine.getAgent().sendLongResponse(msg, null);
                return false;
            }

            Collection<Namespace> namespaces = msg.getNamespaces();
            if (namespaces == null)
                namespaces = entity.getSubObjectNamespaces();

            Long instanceOid = null;
            Point location = null;
            if (namespaces.contains(WorldManagerClient.NAMESPACE) &&
                    ! entity.isNamespaceLoaded(WorldManagerClient.NAMESPACE)) {
                location = new Point();
                instanceOid = Engine.getDatabase().getLocation(oid,
                        WorldManagerClient.NAMESPACE, location);
                if (instanceOid == null) {
                    Log.error("LoadObjectHook: world manager object missing instanceOid, entity=" + entity);
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;
                }
                if (instanceContent.get(instanceOid) == null) {
                    int rc = InstanceClient.loadInstance(instanceOid);
                    if (rc != InstanceClient.RESULT_OK) {
                        if (rc != InstanceClient.RESULT_ERROR_UNKNOWN_OBJECT)
                            Log.error("LoadObjectHook: internal error loading instanceOid=" + instanceOid + " for oid=" + oid);
                        Engine.getAgent().sendLongResponse(msg, null);
                        return false;
                    }
                }
                if (! isInstanceLoading(instanceOid)) {
                    Log.error("LoadObjectHook: instance unavailable for oid=" +
                        oid + " instanceOid=" + instanceOid + " " +
                        instanceContent.get(instanceOid));
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;
                }
                entity.setInstanceOid(instanceOid);
            }

            // send a load sub object message for each of the namespaces
            for (Namespace namespace : namespaces) {
                if (entity.isNamespaceLoaded(namespace))
                    continue;
                if (Log.loggingDebug)
                    log.debug("LoadObjectHook: masterOid=" + oid + ", sending load subobj msg, ns=" + namespace);
                
                ObjectManagerClient.LoadSubObjectMessage loadSubMsg;
                if (namespace == WorldManagerClient.NAMESPACE) {
                    loadSubMsg =
                        new WorldManagerClient.LoadSubObjectMessage(oid,
                            namespace, location, instanceOid);
                }
                else {
                    loadSubMsg =
                        new ObjectManagerClient.LoadSubObjectMessage(
                            oid, namespace);
                }

                Boolean rv;
                try {
                    rv = Engine.getAgent().sendRPCReturnBoolean(loadSubMsg);
                }
                catch (NoRecipientsException e) {
                    log.exception("LoadObjectHook: sub object load failed, maybe instance does not exist", e);
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;

                }

                if (!rv) {
                    log.error("LoadObjectHook: sub object load failed: "+
                            namespace);
                    Engine.getAgent().sendLongResponse(msg, null);
                    return false;
                }

                entity.addLoadedNamespace(namespace);
            }

            if (namespaces.contains(WorldManagerClient.INSTANCE_NAMESPACE)) {
                addInstance(entity);
            }
            if (instanceOid != null && ! entity.getType().isPlayer()) {
                addInstanceContent(instanceOid,entity);
            }

            // send a response message
            Engine.getAgent().sendLongResponse(msg, oid);
            if (Log.loggingDebug)
                log.debug("LoadObjectHook: sent success response for master obj=" + oid);
            return true;
        }
    }

    class UnloadObjectHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.UnloadObjectMessage msg = (ObjectManagerClient.UnloadObjectMessage) m;
            Long oid = msg.getOid();

            MasterObject entity =
                (MasterObject) EntityManager.getEntityByNamespace(oid,
                    Namespace.OBJECT_MANAGER);

//## should lock to prevent parallel delete/unload
            if (entity == null) {
                log.error("UnloadObjectHook: no such entity oid=" + oid);
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }

            // get the master object's namespace manager
            Collection<Namespace> namespaces = msg.getNamespaces();
            if (namespaces == null)
                namespaces = entity.getSubObjectNamespaces();
            int failure = 0;

            // send unload sub object message for each of the namespaces
            for (Namespace namespace : namespaces) {
                if (Log.loggingDebug)
                    log.debug("UnloadObjectHook: oid=" + oid +
                        ", sending unload subobj msg, ns=" + namespace);
                
                ObjectManagerClient.UnloadSubObjectMessage unloadSubMsg =
                    new ObjectManagerClient.UnloadSubObjectMessage(oid, namespace);
                Boolean rv = Engine.getAgent().sendRPCReturnBoolean(unloadSubMsg);
                if (!rv) {
                    log.error("UnloadObjectHook: sub object unload failed oid="
                        + oid + " ns="+ namespace);
                    failure++;
                }
                else if (msg.getNamespaces() != null) {
                    entity.removeLoadedNamespace(namespace);
                }
            }

            // All namespaces unloaded, so remove entity
            if (msg.getNamespaces() == null) {
                EntityManager.removeEntityByNamespace(entity,
                        Namespace.OBJECT_MANAGER);
                if (entity.getPersistenceFlag() &&
                        Engine.getPersistenceManager().isDirty(entity))
                    Engine.getPersistenceManager().persistEntity(entity);
            }

            if (namespaces.contains(WorldManagerClient.INSTANCE_NAMESPACE)) {
                removeInstance(entity);
            }
            if (entity.getInstanceOid() != null &&
                    ! entity.getType().isPlayer() &&
                    namespaces.contains(WorldManagerClient.NAMESPACE) ) {
                removeInstanceContent(entity.getInstanceOid(),entity);
            }

            if (Log.loggingDebug)
                log.debug("UnloadObjectHook: unloaded oid=" + oid + ", " +
                        failure + " failures");

            // send a response message
            Engine.getAgent().sendBooleanResponse(msg, failure == 0);

            return true;
        }
    }

    class DeleteObjectHook implements Hook {
        public boolean processMessage(Message m, int flags)
        {
            ObjectManagerClient.DeleteObjectMessage msg;
            msg = (ObjectManagerClient.DeleteObjectMessage) m;

            Long oid = msg.getOid();

//## should lock to prevent parallel delete/unload
            MasterObject entity = (MasterObject) EntityManager.getEntityByNamespace(
                oid, Namespace.OBJECT_MANAGER);

            if (entity == null) {
                log.debug("DeleteObjectHook: no such entity oid=" + oid);
                //##?? load obj from database, send DeleteSubObject for
                //##?? all namespaces ?
                //## would be nice to detect oid not in database
                Engine.getDatabase().deleteObjectData(oid);
                Engine.getAgent().sendBooleanResponse(msg, true);
                return false;
            }

            if (entity.isDeleted())
                return true;

            entity.setDeleted();

            // get the master object's namespace manager
            List<Namespace> namespaces = entity.getSubObjectNamespaces();
            int failure = 0;
            // send a delete sub object message for each of the namespaces
            for (Namespace namespace : namespaces) {
                if (Log.loggingDebug)
                    log.debug("DeleteObjectHook: oid=" + oid +
                        ", sending delete subobj msg, ns=" + namespace);

                ObjectManagerClient.DeleteSubObjectMessage deleteSubMsg =
                    new ObjectManagerClient.DeleteSubObjectMessage(oid, namespace);
                Boolean rv = Engine.getAgent().sendRPCReturnBoolean(deleteSubMsg);
                if (!rv) {
                    log.error("DeleteObjectHook: sub object delete failed oid="
                        + oid + " ns="+ namespace);
                    failure++;
                }
            }

	    // remove the entity
            Engine.getDatabase().deleteObjectData(oid);
            EntityManager.removeEntityByNamespace(entity, Namespace.OBJECT_MANAGER);

            if (namespaces.contains(WorldManagerClient.INSTANCE_NAMESPACE)) {
                removeInstance(entity);
            }
            if (entity.getInstanceOid() != null &&
                    ! entity.getType().isPlayer()) {
                removeInstanceContent(entity.getInstanceOid(),entity);
            }

            if (Log.loggingDebug)
                log.debug("DeleteObjectHook: deleted oid=" + oid + ", "+
                        failure+" failures");

            // send a response message
            Engine.getAgent().sendBooleanResponse(msg, failure == 0);

            return true;
        }
    }

    class SaveObjectHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.SaveObjectMessage msg =
                (ObjectManagerClient.SaveObjectMessage) m;
            new SaveObjectProcessor(msg).processMessage();
            return true;
        }
    }

    static class SaveObjectProcessor implements ResponseCallback {
        public SaveObjectProcessor(
                ObjectManagerClient.SaveObjectMessage message) {
            msg = message;
            oid = msg.getOid();
            key = msg.getKey();
            masterObj = (MasterObject) EntityManager.getEntityByNamespace(oid,
                Namespace.OBJECT_MANAGER);
        }

        public void processMessage() {
            if (Log.loggingDebug)
                Log.debug("SaveObjectHook: oid=" + oid);

            if (! masterObj.getPersistenceFlag()) {
                Log.warn("Ignoring saveObject for non-persistent object oid="+oid);
                Engine.getAgent().sendBooleanResponse(msg, Boolean.FALSE);
                return;
            }

            // save all plugin sub objects
            List<Namespace> namespaces = masterObj.getSubObjectNamespaces();
            if (Log.loggingDebug) {
                String s = "";
                for (Namespace ns : namespaces) {
                    if (s != "")
                        s += ",";
                    s += ns;
                }
                Log.debug("SaveObjectHook: masterObj namespaces " + s);
            }

            pendingRPC = new ArrayList<Message>(namespaces.size());

            // send a save sub object message for each of the namespaces
            synchronized (pendingRPC) {
                for (Namespace namespace : namespaces) {
                    if (Log.loggingDebug)
                        Log.debug("SaveObjectHook: oid=" + oid +
                            ", sending save subobj msg to ns=" + namespace);
                    Message saveSubMsg = new OIDNamespaceMessage(
                        ObjectManagerClient.MSG_TYPE_SAVE_SUBOBJECT,
                        oid,namespace);
                    pendingRPC.add(saveSubMsg);
                    // Async RPC, see handleResponse()
                    Engine.getAgent().sendRPC(saveSubMsg,this);
                }
            }
        }

        public void handleResponse(ResponseMessage response)
        {
            synchronized (pendingRPC) {
                Message request = null;
                for (Message message : pendingRPC) {
                    if (message.getMsgId() == response.getRequestId()) {
                        pendingRPC.remove(message);
                        request = message;
                        break;
                    }
                }
                if (request == null)
                    Log.error("SaveObjectHook: unexpected response "+response);
                if (! ((BooleanResponseMessage)response).getBooleanVal()) {
                    log.warn("SaveObjectHook: sub object load failed for oid="+
                        oid +" "+ request);
                }
            }
            if (pendingRPC.size() == 0) {
                saveMasterObject();
                Engine.getAgent().sendBooleanResponse(msg, Boolean.TRUE);
                // All done
            }
        }

        void saveMasterObject() {
            if (Log.loggingDebug)
                Log.debug("SaveObjectHook: saving master object oid="+oid);

            // We're in the object manager, so save directly to the
            // database rather than using PersistenceManager.persistEntity()
            // which would send a message to ourselves.
            Engine.getPersistenceManager().callSaveHooks(masterObj);
            Engine.getDatabase().saveObject(key, masterObj.toBytes(),
                masterObj.getNamespace());

            // make a response message
            if (Log.loggingDebug)
                Log.debug("SaveObjectHook: success oid="+oid);
        }

        ObjectManagerClient.SaveObjectMessage msg;
        Long oid;
        String key;
        MasterObject masterObj;
        List<Message> pendingRPC;
    }
    
    class LoadObjectDataHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.LoadObjectDataMessage msg = (ObjectManagerClient.LoadObjectDataMessage) m;
            Long oid = msg.getSubject();
            String persistenceKey = msg.getKey();

            // load from database
            Entity entity = null;
            if (persistenceKey != null) {
                entity = Engine.getDatabase().loadEntity(persistenceKey);
            }
            else if (oid != null) {
                entity = Engine.getDatabase().loadEntity(oid,
                    msg.getNamespace());
            }
            else {
                log.error("LoadObjectDataHook: oid and key both null");
            }
            Engine.getAgent().sendObjectResponse(msg, entity);
            return true;
        }
    }
        
    class SaveObjectDataHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.SaveObjectDataMessage msg = (ObjectManagerClient.SaveObjectDataMessage) m;
            Long oid = msg.getSubject();
            String persistenceKey = msg.getKey();
            if (msg.getNamespace() == Namespace.TRANSIENT) {
                log.warn("SaveObjectDataHook: ignoring transient namespace for oid="+oid+" key="+persistenceKey);
                Engine.getAgent().sendBooleanResponse(msg, Boolean.FALSE);
                return false;
            }
            
            if (Log.loggingDebug)
                log.debug("SaveObjectDataHook: oid=" + oid);

            byte[] data = (byte[]) msg.getDataBytes();

            // save to database
            Engine.getDatabase().saveObject(persistenceKey, data, msg.getNamespace());

            // make a response message
            Engine.getAgent().sendBooleanResponse(msg, Boolean.TRUE);
            if (Log.loggingDebug)
                log.debug("SaveObjectDataHook: sent response for obj=" + oid);
            return true;
        }
    }

    /**
     * generates an object from the passed in template name returns the byte
     * array in a response message
     */
    class GenerateObjectHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.GenerateObjectMessage msg = (ObjectManagerClient.GenerateObjectMessage) m;

            //long start = System.nanoTime();

            String templateName = msg.getTemplateName();
            Template template = templateManager.get(templateName);
            if (template == null) {
                Log.error("template not found: " + templateName);
		Engine.getAgent().sendLongResponse(msg, null);
                return false;
            }

            // handle merging the override template
            Template finalTemplate;
            Template overrideTemplate = msg.getOverrideTemplate();
            if (overrideTemplate != null) {
                finalTemplate = template.merge(overrideTemplate);
            }
            else {
                finalTemplate = template;
            }

            Boolean persistent = (Boolean)finalTemplate.get(
                Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_PERSISTENT);
            if (persistent == null)
                persistent = false;

            if (Log.loggingDebug)
                log.debug("GenerateObjectHook: generating entity: " +
                    finalTemplate.getName() + ", template=" + finalTemplate);
            
            // create a new master object
            String entityName = (String)finalTemplate.get(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_NAME);
            if (entityName == null) {
                entityName = finalTemplate.getName();
            }
            Long instanceOid = (Long)finalTemplate.get(Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_INSTANCE);
            ObjectType objectType = (ObjectType)finalTemplate.get(
                Namespace.WORLD_MANAGER,
                WorldManagerClient.TEMPL_OBJECT_TYPE);

            MasterObject masterObj = new MasterObject(entityName);
            masterObj.setPersistenceFlag(persistent);
            masterObj.setInstanceOid(instanceOid);
            if (objectType != null)
                masterObj.setType(objectType);

            // Copy OBJECT_MANAGER (NS.master) properties into the object
            Map<String, Serializable> objMgrProps =
                finalTemplate.getSubMap(Namespace.OBJECT_MANAGER);
            if (objMgrProps != null) {
                for (Map.Entry<String, Serializable> entry :
                        objMgrProps.entrySet()) {
                    if (!entry.getKey().startsWith(":")) {
                        masterObj.setProperty(entry.getKey(), entry.getValue());
                    }
                }
            }

            EntityManager.registerEntityByNamespace(masterObj, Namespace.OBJECT_MANAGER);
            if (Log.loggingDebug)
                log.debug("GenerateObjectHook: created master obj: " + masterObj);

            // Set the object's namespaces.  We remove the object manager
            // name space as it's only used to communicate the persistence flag
            Set<Namespace> namespaces = finalTemplate.getNamespaces();
            namespaces.remove(Namespace.OBJECT_MANAGER);
            masterObj.setSubObjectNamespaces(namespaces);

            if (persistent)
                Engine.getPersistenceManager().persistEntity(masterObj);

            //long stop = System.nanoTime();
            //Log.info("OBJPREP "+masterObj.getOid()+" TYPE "+objectType+
            //            " pers "+persistent+" time "+(stop-start)/1000 + " us");

            // send out create subobj messages
            for (Namespace namespace : namespaces) {
                //start = System.nanoTime();
                Template subTemplate = finalTemplate.restrict(namespace);
                subTemplate.put(Namespace.OBJECT_MANAGER,
                        ObjectManagerClient.TEMPL_PERSISTENT, persistent);
                if (Log.loggingDebug)
                    log.debug("GenerateObjectHook: creating subobj for ns=" + namespace + ", subTemplate=" + subTemplate);

                GenericResponseMessage respMsg =
                        generateSubObject(masterObj.getOid(), namespace,
                                subTemplate);

                masterObj.addLoadedNamespace(namespace);

                // get data out of the response message
                List<Namespace> depNamespaces = (List<Namespace>)respMsg.getData();
                // add subobj to the master obj
                if (Log.loggingDebug)
                    log.debug("GenerateObjectHook: created subobj for ns=" + namespace);
                
                //stop = System.nanoTime();
                //Log.info("OBJECT "+masterObj.getOid()+" TYPE "+objectType+" SUB "+namespace+
                //        " time "+(stop-start)/1000 + " us");
                // check dependent namespaces
                if ((depNamespaces == null) || (depNamespaces.isEmpty())) {
                    continue;
                }
                
                // we have dependencies, add them to a map
                depTable.put(masterObj.getOid(), namespace, depNamespaces);
            }

            // get the map of dependencies
            Map<Namespace, Collection<Namespace>> depMap = depTable.getSubMap(masterObj.getOid());
            if (depMap != null && !depMap.isEmpty()) {
                //start = System.nanoTime();
                while (! depMap.isEmpty()) {
                    Namespace ns = depMap.keySet().iterator().next();
                    resolveDeps(masterObj.getOid(), ns, depMap);
                }
                //stop = System.nanoTime();
                //Log.info("OBJDEP "+masterObj.getOid()+" TYPE "+objectType+
                //        " time "+(stop-start)/1000 + " us");
            }

            if (namespaces.contains(WorldManagerClient.INSTANCE_NAMESPACE)) {
                addInstance(masterObj);
            }
            if (instanceOid != null && ! masterObj.getType().isPlayer()) {
                addInstanceContent(instanceOid, masterObj);
            }

            // send a response message
            Engine.getAgent().sendLongResponse(msg, masterObj.getOid());
            return true;
        }
        
        // master obj oid -> sub obj namespace -> waiting on namespaces
        Table<Long, Namespace, Collection<Namespace>> depTable = 
            new Table<Long, Namespace, Collection<Namespace>>();
    }

    void resolveDeps(Long masterOid, Namespace namespace, Map<Namespace, Collection<Namespace>> depMap) {
        if (Log.loggingDebug)
            log.debug("resolveDeps: masterOid=" + masterOid + ", ns=" + namespace);

        Collection<Namespace> depNamespaces = depMap.get(namespace);
        if (depNamespaces == null) {
            if (Log.loggingDebug)
                log.debug("resolveDeps: no deps for ns " + namespace);
            return;
        }
        if (Log.loggingDebug) {
            if (depNamespaces == null)
                Log.debug("resolveDeps: depNamespaces is null");
            else {
                Log.debug("resolveDeps: depNamespaces.size() " + depNamespaces.size());
                int i = 0;
                for (Object object : depNamespaces) {
                    Log.debug("resolveDeps: depNamespaces element " + i++ + " " + object);
                }
            }
        }
        for (Namespace depNS : depNamespaces) {
            if (Log.loggingDebug)
                log.debug("resolveDeps: ns " + namespace + " depends on ns " + depNS);

            // does the depNs have any dependency?
            Collection<Namespace> childDeps = depMap.get(depNS);
            if (childDeps != null) {
                if (Log.loggingDebug)
                    log.debug("resolveDeps: ns " + namespace + ", depNS=" + depNS + ", has further deps, recursing");
                resolveDeps(masterOid, depNS, depMap);
            }
        }
        
        // assert: namespace has no dependency dependencies
        // remove namespace from the map
        if (Log.loggingDebug)
            log.debug("resolveDeps: ns " + namespace + ": resolved all deps, removing from table");
        depMap.remove(namespace);
        
        // it is safe now to notify the sub object that all its
        // dependencies are met 
        ObjectManagerClient.SubObjectDepsReadyMessage msg =
            new ObjectManagerClient.SubObjectDepsReadyMessage(masterOid, namespace);
        Boolean resp = Engine.getAgent().sendRPCReturnBoolean(msg);
        if (resp.equals(Boolean.FALSE)) {
            log.error("dependency failed");
        }
        if (Log.loggingDebug)
            log.debug("resolveDeps: ns " + namespace + ": got response msg, result=" + resp);
    }

    /**
     * creates a subobject, returns the subobject's oid
     * @return the GenericResponseMessage from creating the subobject
     *         the object is a LinkedList<Namespace> of namespaces that 
     *         the plugin is still waiting to be created before it can finish
     *         subobject creation.
     */
    GenericResponseMessage generateSubObject(Long masterOid, Namespace namespace, Template template) {
        GenerateSubObjectMessage msg = new GenerateSubObjectMessage(masterOid, namespace, template);
        GenericResponseMessage respMsg = (GenericResponseMessage)Engine.getAgent().sendRPC(msg);
        return respMsg;
    }

//## Need to disallow instance entry while unloading/deleting
//## Need to track objects when they instance change;
//##   current algorithm only works with one WM.  With multi-WM, we'll
//##   have a ReloadSubobject feature on the object manager, so objmgr
//##   can still track player location.

    class LoadInstanceContentHook implements Hook
    {
        public boolean processMessage(Message m, int flags)
        {
            SubjectMessage message = (SubjectMessage) m;
            long instanceOid = message.getSubject();

            MasterObject entity;
            entity = (MasterObject) EntityManager.getEntityByNamespace(instanceOid,
                Namespace.OBJECT_MANAGER);
            if (entity == null) {
                Log.error("LoadInstanceContentHook: instance not loaded instanceOid=" +
                    instanceOid);
                Engine.getAgent().sendBooleanResponse(message, false);
                return true;
            }

            if (! isInstanceOk(instanceOid,INSTANCE_LOADING)) {
                Log.error("LoadInstanceContentHook: instance not available instanceOid=" +
                    instanceOid);
                Engine.getAgent().sendBooleanResponse(message, false);
                return true;
            }

            List<Long> content = Engine.getDatabase().getInstanceContent(
                instanceOid, ObjectTypes.player);

            for (Long oid : content) {
                if (ObjectManagerClient.loadObject(oid) != null)
                    WorldManagerClient.spawn(oid);
            }

            setInstanceStatus(instanceOid,INSTANCE_OK);

            Engine.getAgent().sendBooleanResponse(message, true);

            return true;
        }

    }

    class UnloadInstanceHook implements Hook
    {
        public boolean processMessage(Message m, int flags)
        {
            SubjectMessage message = (SubjectMessage) m;
            long instanceOid = message.getSubject();

            MasterObject entity;
            entity = (MasterObject) EntityManager.getEntityByNamespace(instanceOid,
                Namespace.OBJECT_MANAGER);
            if (entity == null) {
                Log.error("UnloadInstanceHook: instance not loaded oid=" +
                    instanceOid);
                Engine.getAgent().sendBooleanResponse(message, false);
                return true;
            }

            if (! isInstanceOk(instanceOid,INSTANCE_UNLOADING)) {
                Log.error("UnloadInstanceHook: instance not available instanceOid=" +
                    instanceOid);
                Engine.getAgent().sendBooleanResponse(message, false);
                return true;
            }

            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState != null) {
                List<MasterObject> objects =
                    new ArrayList<MasterObject>(instanceState.entities);
                for (MasterObject obj : objects) {
                    // Don't unload players!
                    if (! obj.getType().isPlayer())
                        /* rc = */ ObjectManagerClient.unloadObject(obj.getOid());
                }
            }

            SubjectMessage unloadedMessage = new SubjectMessage(
                InstanceClient.MSG_TYPE_INSTANCE_UNLOADED, instanceOid);
            Engine.getAgent().sendBroadcastRPC(unloadedMessage,
                new InstanceRPCCallback(instanceOid,"InstanceUnloaded"));

            /* rc = */ ObjectManagerClient.unloadObject(instanceOid);

            Engine.getAgent().sendBooleanResponse(message, true);

            return true;

        }

    }

    class DeleteInstanceHook implements Hook
    {
        public boolean processMessage(Message m, int flags)
        {
            SubjectMessage message = (SubjectMessage) m;
            long instanceOid = message.getSubject();

            MasterObject entity;
            entity = (MasterObject) EntityManager.getEntityByNamespace(instanceOid,
                Namespace.OBJECT_MANAGER);
            if (entity == null) {
                Log.error("DeleteInstanceHook: instance not loaded oid=" +
                    instanceOid);
                Engine.getAgent().sendBooleanResponse(message, false);
                return true;
            }

            if (! isInstanceOk(instanceOid,INSTANCE_DELETING)) {
                Log.error("DeleteInstanceHook: instance not available instanceOid=" +
                    instanceOid);
                Engine.getAgent().sendBooleanResponse(message, false);
                return true;
            }

            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState != null) {
                List<MasterObject> objects =
                    new ArrayList<MasterObject>(instanceState.entities);
                for (MasterObject obj : objects) {
                    // Don't delete players!
                    if (! obj.getType().isPlayer())
                        /* rc = */ ObjectManagerClient.deleteObject(obj.getOid());
                }
            }

            SubjectMessage deletedMessage = new SubjectMessage(
                InstanceClient.MSG_TYPE_INSTANCE_DELETED, instanceOid);
            Engine.getAgent().sendBroadcastRPC(deletedMessage,
                new InstanceRPCCallback(instanceOid,"InstanceDeleted"));

            /* rc = */ ObjectManagerClient.deleteObject(instanceOid);

            Engine.getAgent().sendBooleanResponse(message, true);

            return true;
        }
    }

    public static class InstanceRPCCallback implements ResponseCallback
    {
        public InstanceRPCCallback(long instanceOid, String operation)
        {
            this.instanceOid = instanceOid;
            this.operation = operation;
        }

        public void handleResponse(ResponseMessage response)
        {
            Log.debug(operation+": got response, instanceOid=" + instanceOid);
        }

        long instanceOid;
        String operation;
    }

    /**
     * marks the master object as persistent and sends 
     * all plugins a persistence message
     * @author cedeno
     *
     */
    class SetPersistenceHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.SetPersistenceMessage msg = (ObjectManagerClient.SetPersistenceMessage) m;
            Long oid = msg.getSubject();
            Entity master = EntityManager.getEntityByNamespace(oid, Namespace.OBJECT_MANAGER);
            if (master == null) {
                Log.error("SetPersistenceHook: no master entity found for oid " + oid);
                Engine.getAgent().sendBooleanResponse(m, false);
            }
            Boolean persistVal = msg.getPersistVal();
            
            if (Log.loggingDebug)
                log.debug("SetPersistenceHook: masterOid=" + oid + ", persistVal=" + persistVal);
            
            // get all the sub objects
            //Boolean rv = null;
            List<Namespace> namespaces = master.getSubObjectNamespaces();
            
            // send a setPersistenceMessage to all plugins
            for (Namespace namespace : namespaces) {
                if (Log.loggingDebug)
                    log.debug("SetPersistenceHook: masterOid=" + oid + ", sending setpersistence msg to sub ns " + namespace);
                Message persistSubMsg = new ObjectManagerClient.SetSubPersistenceMessage(oid, namespace, persistVal);
                // Wait for response
                Engine.getAgent().sendRPC(persistSubMsg);
            }

            // set persistence flag on the master object
            master.setPersistenceFlag(persistVal);
            
            // if persistent, set the object dirty so that it will be saved
            if (persistVal) {
                Engine.getPersistenceManager().setDirty(master);
                log.debug("SetPersistenceHook: set master object dirty");
            }
            else {
                // Object no longer persistent, delete it from the database
                Engine.getDatabase().deleteObjectData(oid);
            }

            log.debug("SetPersistenceHook: done with persistence");
            Engine.getAgent().sendBooleanResponse(m, true);
            return true;            
        }
    }
    
    /**
     * registers the template with the plugin so that you can then call
     * generateObject and pass in this template's name
     */
    class RegisterTemplateHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.RegisterTemplateMessage msg = (ObjectManagerClient.RegisterTemplateMessage) m;

            Template template = msg.getTemplate();
            boolean successStatus = templateManager.register(
                    template.getName(), template);
            if (Log.loggingDebug)
                log.debug("handleRegisterTemplateMsg: registered template: "
                          + template + ", success=" + successStatus);
            log.debug("handleRegisterTemplateMsg: sending response message");
            Engine.getAgent().sendBooleanResponse(msg, Boolean.valueOf(successStatus));
            log.debug("handleRegisterTemplateMsg: response message sent");
            return true;
        }
    }

    /**
     * gets a template and returns it to the caller
     */
    class GetTemplateHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.GetTemplateMessage msg = (ObjectManagerClient.GetTemplateMessage) m;

            String templateName = msg.getTemplateName();
            Template template = templateManager.get(templateName);
            Engine.getAgent().sendObjectResponse(msg, template);
            return true;
        }
    }

    /**
     * gets a template and returns it to the caller
     */
    class GetTemplateNamesHook implements Hook {
        public boolean processMessage(Message message, int flags) {
            List<String> templateNames = templateManager.keyList();
            Engine.getAgent().sendObjectResponse(message, templateNames);
            return true;
        }
    }

    class FixWorldNodeHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            ObjectManagerClient.FixWorldNodeMessage message =
                (ObjectManagerClient.FixWorldNodeMessage) msg;

            BasicWorldNode worldNode = message.getWorldNode();

            Entity entity = null;
            try {
                entity = (Entity) Engine.getDatabase().loadEntity(
                    message.getOid(), WorldManagerClient.NAMESPACE);
            }
            catch (MVRuntimeException e) {
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }

            if (entity == null) {
                Log.error("FixWorldNodeHook: unknown oid="+message.getOid());
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }
            if (! (entity instanceof MVObject)) {
                Log.error("FixWorldNodeHook: not instanceof MVObject oid="+
                        message.getOid() + " class="+entity.getClass().getName());
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }

            MVObject obj = (MVObject) entity;
            WMWorldNode wnode = (WMWorldNode) obj.worldNode();
            wnode.setInstanceOid(worldNode.getInstanceOid());
            if (worldNode.getLoc() != null)
                wnode.setLoc(worldNode.getLoc());
            if (worldNode.getOrientation() != null)
                wnode.setOrientation(worldNode.getOrientation());
            if (worldNode.getDir() != null)
                wnode.setDir(worldNode.getDir());

            Engine.getPersistenceManager().persistEntity(obj);

            if (Log.loggingDebug)
                log.debug("FixWorldNodeHook: done oid=" + message.getOid() +
                          " wnode=" + obj.worldNode());

            Engine.getAgent().sendBooleanResponse(msg, true);
            return true;
        }
    }

    /**
     */
    class GetNamedObjectHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            GetNamedObjectMessage message = (GetNamedObjectMessage) msg;
            Long oid = null;
            if (message.getInstanceOid() != null)
                oid = getInstanceNamedObject(message.getInstanceOid(),
                    message.getName(), message.getObjectType());
            else if (message.getName() != null)
                oid = getNamedObject(message.getName(),
                    message.getObjectType());
            Engine.getAgent().sendLongResponse(message, oid);
            return true;
        }
    }

    class GetPluginStatusHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            LinkedHashMap<String,Serializable> status =
                new LinkedHashMap<String,Serializable>();
            status.put("plugin", getName());
            try {
                status.put("account",
                    Engine.getDatabase().getAccountCount(Engine.getWorldName()));
            } catch (Exception e) {
                Log.exception("GetPluginStatusHook", e);
            }
            Engine.getAgent().sendObjectResponse(msg,status);
            return true;
        }

        int lastLoginCount = 0;
    }

    class GetObjectStatusHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            long oid = ((SubjectMessage)msg).getSubject();
            ObjectStatus objectStatus = new ObjectStatus();
            objectStatus.oid = oid;
            MasterObject masterObject = (MasterObject)
                EntityManager.getEntityByNamespace(oid,Namespace.OBJECT_MANAGER);
            if (masterObject == null) {
                String name = Engine.getDatabase().getObjectName(oid,
                    Namespace.OBJECT_MANAGER);
                objectStatus.name = name;
                Engine.getAgent().sendObjectResponse(msg,objectStatus);
                return true;
            }
            if (masterObject.isDeleted()) {
                Engine.getAgent().sendObjectResponse(msg,objectStatus);
                return true;
            }

            objectStatus.name = masterObject.getName();
            objectStatus.type = masterObject.getType();
            objectStatus.persistent = masterObject.getPersistenceFlag();
            objectStatus.namespaces =
                new ArrayList<Namespace>(masterObject.getSubObjectNamespaces());
            objectStatus.loadedNamespaces =
                new ArrayList<Namespace>(objectStatus.namespaces.size());
            int loadedNS = masterObject.getLoadedNamespaces();
            int nsBit = 2;
            int nsInt = 1;
            while (nsBit != 0) {
                if ((loadedNS & nsBit) != 0) {
                    Namespace namespace = Namespace.getNamespaceFromInt(nsInt);
                    objectStatus.loadedNamespaces.add(namespace);
                }
                nsBit <<= 1;
                nsInt++;
            }

            Engine.getAgent().sendObjectResponse(msg,objectStatus);
            return true;
        }

        int lastLoginCount = 0;
    }

    public static class MasterObject extends Entity
    {
        public MasterObject()
        {
            super();
        }

        public MasterObject(String name)
        {
            super(name);
        }

//## need to disable beans/xml serialization of instance oid
        public Long getInstanceOid()
        {
            return instanceOid;
        }

        public void setInstanceOid(Long oid)
        {
            instanceOid = oid;
        }

        public int getLoadedNamespaces()
        {
            return loadedNamespaces;
        }

        public void setLoadedNamespaces(int namespaceBits)
        {
            loadedNamespaces = namespaceBits;
        }

        public void addLoadedNamespace(Namespace namespace)
        {
            loadedNamespaces |= (1 << namespace.getNumber());
        }

        public void removeLoadedNamespace(Namespace namespace)
        {
            loadedNamespaces &= (~(1 << namespace.getNumber()));
        }

        public boolean isNamespaceLoaded(Namespace namespace)
        {
            return (loadedNamespaces & (1 << namespace.getNumber())) != 0;
        }

        public boolean loadComplete()
        {
            return getSubObjectNamespacesInt() == loadedNamespaces;
        }

        private transient int loadedNamespaces = 0;
        private transient Long instanceOid;

        private static final long serialVersionUID = 1L;
    }

    private static final int INSTANCE_OK = 0;
    private static final int INSTANCE_LOADING = 1;
    private static final int INSTANCE_UNLOADING = 2;
    private static final int INSTANCE_DELETING = 3;

    private static class InstanceState
    {
        public InstanceState(MasterObject instance) {
            this.instance = instance;
            status = INSTANCE_OK;
        }

        public String toString()
        {
            return "instanceOid=" + instance.getOid() +
                " status=" + statusToString(status) +
                " entityCount=" + entities.size();
        }

        public static String statusToString(int status) {
            if (status == INSTANCE_OK) return "OK";
            if (status == INSTANCE_LOADING) return "LOADING";
            if (status == INSTANCE_UNLOADING) return "UNLOADING";
            if (status == INSTANCE_DELETING) return "DELETING";
            return ""+status+" (unknown)";
        }

        public MasterObject instance;
        public int status;
        public Set<MasterObject> entities = new HashSet<MasterObject>();
    }

    private void addInstance(MasterObject instance)
    {
        InstanceState instanceState = new InstanceState(instance);
        synchronized (instanceContent) {
            InstanceState previous =
                instanceContent.put(instance.getOid(),instanceState);
            if (previous != null) {
                Log.error("addInstance: duplicate instance [OLD "+previous+
                        "] [NEW "+instanceState+"]");
            }
        }
        if (Log.loggingDebug)
            Log.debug("addInstance: added instanceOid=" + instance.getOid());
    }

    private void removeInstance(MasterObject instance)
    {
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instance.getOid());
            if (instanceState == null) {
                Log.error("removeInstance: unknown instanceOid="+
                    instance.getOid());
                return;
            }
            if (instanceState.entities.size() > 0) {
                Log.warn("removeInstance: wrong state: " + instanceState);
            }
            instanceContent.remove(instance.getOid());
        }
        if (Log.loggingDebug)
            Log.debug("removeInstance: removed instanceOid="+
                instance.getOid());
    }

    private void addInstanceContent(long instanceOid, MasterObject entity)
    {
        if (Log.loggingDebug)
            Log.debug("addInstanceContent: instanceOid="+instanceOid +
                " oid="+entity.getOid());
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState == null) {
                Log.error("addInstanceContent: unknown instanceOid="+
                        instanceOid + " for "+entity);
                return;
            }
            instanceState.entities.add(entity);
        }
    }

    private void removeInstanceContent(long instanceOid, MasterObject entity)
    {
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState == null) {
                Log.error("removeInstanceContent: unknown instanceOid=" +
                        instanceOid);
                return;
            }
            if (Log.loggingDebug)
                Log.debug("removeInstanceContent: instanceOid="+instanceOid +
                    " oid="+entity.getOid() +
                    " count="+instanceState.entities.size());
            instanceState.entities.remove(entity);
        }
    }

    private boolean isInstanceOk(long instanceOid, int newStatus)
    {
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState != null) {
                boolean result = instanceState.status == INSTANCE_OK;
                if (result && newStatus != -1)
                    instanceState.status = newStatus;
                return result;
            }
            else
                return false;
        }
    }

    private boolean isInstanceLoading(long instanceOid)
    {
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState != null)
                return (instanceState.status == INSTANCE_OK) ||
                        (instanceState.status == INSTANCE_LOADING);
            else
                return false;
        }
    }

    private void setInstanceStatus(long instanceOid, int newStatus)
    {
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instanceOid);
            instanceState.status = newStatus;
        }
    }

    private Long getInstanceNamedObject(long instanceOid, String name,
        ObjectType objectType)
    {
        synchronized (instanceContent) {
            InstanceState instanceState = instanceContent.get(instanceOid);
            if (instanceState == null)
                return null;
            if (objectType != null) {
                for (MasterObject entity : instanceState.entities) {
                    String entityName = entity.getName();
                    if (entity.getType() == objectType &&
                            entityName != null && entityName.equals(name))
                        return entity.getOid();
                }
            }
            else {
                for (MasterObject entity : instanceState.entities) {
                    String entityName = entity.getName();
                    if (entityName != null && entityName.equals(name))
                        return entity.getOid();
                }
            }
            return null;
        }
    }

    private Long getNamedObject(String name, ObjectType objectType)
    {
        Entity[] entities =
            EntityManager.getAllEntitiesByNamespace(Namespace.OBJECT_MANAGER);
        if (objectType != null) {
            for (Entity entity : entities) {
                String entityName = entity.getName();
                if (entity.getType() == objectType &&
                        entityName != null && entityName.equals(name))
                    return entity.getOid();
            }
        }
        else {
            for (Entity entity : entities) {
                String entityName = entity.getName();
                if (entityName != null && entityName.equals(name))
                    return entity.getOid();
            }
        }
        return null;
    }

    protected static final Logger log = new Logger("ObjectManagerPlugin");
    
    protected Manager<Template> templateManager = new Manager<Template>(
            "TemplateManager");

    // Objects contained in each instance
    private Map<Long,InstanceState> instanceContent =
        new HashMap<Long,InstanceState>();
}
