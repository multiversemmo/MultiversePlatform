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
import java.util.concurrent.locks.*;

import multiverse.msgsys.*;
import multiverse.server.objects.*;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.engine.*;
import multiverse.server.util.*;

// each item has a backlink to the container object
// direct: add item to inventory (direct) - checks that the item is nowhere
// direct: remove item from inventory (direct) - checks that item is nowhere
// xfer: item from one container to another

/**
 * handles all inventory queries (what is in the inventory) and also transfering
 * items between containers
 */
public abstract class InventoryPlugin extends
        multiverse.server.engine.EnginePlugin implements MessageCallback {

    public InventoryPlugin() {
        super(INVENTORY_PLUGIN_NAME);
        setPluginType("Inventory");
    }

    public static String INVENTORY_PLUGIN_NAME = "Inventory";

    public void onActivate() {
        try {
            // register for msgtype->hooks
            registerHooks();

            MessageTypeFilter filter = new MessageTypeFilter();
            filter.addType(InventoryClient.MSG_TYPE_ACTIVATE);
            filter.addType(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT);
            filter.addType(InventoryClient.MSG_TYPE_DESTROY_ITEM);
            /* Long sub = */ Engine.getAgent().createSubscription(filter, this);

            filter = new MessageTypeFilter();
            filter.addType(InventoryClient.MSG_TYPE_ADD_ITEM);
	    filter.addType(InventoryClient.MSG_TYPE_INV_REMOVE);
            filter.addType(InventoryClient.MSG_TYPE_CREATE_INV);
            filter.addType(InventoryClient.MSG_TYPE_LOOTALL);
	    filter.addType(InventoryClient.MSG_TYPE_INV_FIND);            
            /* Long sub = */ Engine.getAgent().createSubscription(filter, this,
                MessageAgent.RESPONDER);

	    List<Namespace> namespaces = new ArrayList<Namespace>();
	    namespaces.add(Namespace.BAG);
	    namespaces.add(Namespace.MARSITEM);
            registerPluginNamespaces(namespaces,
                new InventoryGenerateSubObjectHook());

            registerLoadHook(Namespace.BAG, new InventoryLoadHook());
            registerLoadHook(Namespace.MARSITEM, new ItemLoadHook());
            registerUnloadHook(Namespace.BAG, new InventoryUnloadHook());
            registerUnloadHook(Namespace.MARSITEM, new ItemUnloadHook());
            registerDeleteHook(Namespace.BAG, new InventoryDeleteHook());
            registerDeleteHook(Namespace.MARSITEM, new ItemDeleteHook());
            registerSaveHook(Namespace.BAG, new InventorySaveHook());
            registerSaveHook(Namespace.MARSITEM, new ItemSaveHook());
        } catch (Exception e) {
            throw new MVRuntimeException("activate failed", e);
        }
    }

    protected void registerHooks() {
        getHookManager().addHook(InventoryClient.MSG_TYPE_ADD_ITEM,
                new AddItemHook());
        getHookManager().addHook(InventoryClient.MSG_TYPE_CREATE_INV,
                new CreateInvHook());
        getHookManager().addHook(InventoryClient.MSG_TYPE_ACTIVATE,
                new ItemActivateHook());
        getHookManager().addHook(InventoryClient.MSG_TYPE_LOOTALL,
                new LootAllHook());
	getHookManager().addHook(InventoryClient.MSG_TYPE_INV_FIND,
		new FindItemHook());
	getHookManager().addHook(InventoryClient.MSG_TYPE_INV_REMOVE,
		new RemoveItemHook());
        getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT,
                new UpdateObjHook());
        getHookManager().addHook(InventoryClient.MSG_TYPE_DESTROY_ITEM,
                new DestroyItemHook());
    }

    //
    //
    // HOOKS
    //
    //
    class InventoryGenerateSubObjectHook extends GenerateSubObjectHook {
	public InventoryGenerateSubObjectHook() {
	    super(InventoryPlugin.this);
	}

        public SubObjData generateSubObject(Template template, Namespace namespace, Long masterOid) {
            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid=" + masterOid
                        + ", template=" + template);
            if (namespace.equals(Namespace.BAG)) {
                return createInvSubObj(masterOid, template);
            } else if (namespace.equals(Namespace.MARSITEM)) {
                return createItemSubObj(masterOid, template);
            }
            log.error("InventoryGenerateSubObjectHook: unknown namespace: "
                    + namespace);
            return null;
	}
    }

    class InventoryLoadHook implements LoadHook {
	public void onLoad(Entity e) {
	    loadInventory(e);
	}
    }

    class InventoryUnloadHook implements UnloadHook {
	public void onUnload(Entity e) {
	    unloadInventory(e);
	}
    }

    class InventoryDeleteHook implements DeleteHook {
	public void onDelete(Entity e) {
	    deleteInventory(e);
	}
	public void onDelete(Long oid, Namespace namespace) {
	    deleteInventory(oid);
	}
    }

    class ItemLoadHook implements LoadHook {
	public void onLoad(Entity e) {
	    loadItem(e);
	}
    }
            
    class ItemUnloadHook implements UnloadHook {
	public void onUnload(Entity e) {
	    unloadItem(e);
	}
    }

    class ItemDeleteHook implements DeleteHook {
	public void onDelete(Entity e) {
	    deleteItem(e);
	}
	public void onDelete(Long oid, Namespace namespace) {
	    deleteItem(oid);
        }
    }

    class InventorySaveHook implements SaveHook {
	public void onSave(Entity e, Namespace namespace) {
	    saveInventory(e, namespace);
	}
    }

    class ItemSaveHook implements SaveHook {
	public void onSave(Entity e, Namespace namespace) {
	    saveItem(e, namespace);
	}
    }

    class UpdateObjHook implements Hook {
        public boolean processMessage(Message msg, int flags) {

            WorldManagerClient.UpdateMessage cMsg = (WorldManagerClient.UpdateMessage) msg;
            Long oid = cMsg.getSubject();

	    // only send inventory data if object is asking about itself
	    Long nOid = cMsg.getTarget();
            if (!oid.equals(nOid)) {
                return true;
	    }
            Lock objLock = getObjectLockManager().getLock(oid);
            objLock.lock();
            try {
                updateObject(oid, cMsg.getTarget());
                return true;
            } finally {
                objLock.unlock();
            }
        }
    }
    
    class AddItemHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            InventoryClient.AddItemMessage aMsg = (InventoryClient.AddItemMessage) msg;
            Long containerOid = aMsg.getContainer();
            Long itemOid = aMsg.getItem();
            Long mobOid = aMsg.getMob();
            if (Log.loggingDebug)
                log.debug("addItemHook: containerOid=" + containerOid
                          + ", itemOid=" + itemOid);

            Lock objLock = getObjectLockManager().getLock(mobOid);
            objLock.lock();
            try {
                boolean rv = addItem(mobOid, containerOid, itemOid);

                if (Log.loggingDebug)
                    log.debug("addItemHook: containerOid=" + containerOid
                              + ", itemOid=" + itemOid + ", result=" + rv);

                Engine.getAgent().sendBooleanResponse(msg, rv);

                // send out an inventory update message
                sendInvUpdate(mobOid);
                return rv;
            } finally {
                objLock.unlock();
            }
        }
    }

    class DestroyItemHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage destroyMsg = (ExtensionMessage)msg;
            Long ownerOid = destroyMsg.getSubject();
            Long itemOid = (Long)destroyMsg.getProperty("itemOid");
            
            if (Log.loggingDebug)
                log.debug("DestroyItemHook: ownerOid=" + ownerOid +
                          ", itemOid=" + itemOid);
            boolean rv = false;
            
            // delete the object
            // FIXME: we need to check for ownership, but
            // i think we need to do a bit of design rework
            // for that.  delete hooks have no idea who owns
            // the object, so it cant check either.
            // we shouldnt check here, because then we'd be holding
            // a lock while doing a RPC
            
            rv = containsItem(ownerOid, itemOid);
            if (! rv) {
                log.debug("DestroyItemHook: item " + itemOid + " not owned by owner " + ownerOid);
                return true;
            }
            rv = ObjectManagerClient.deleteObject(itemOid);
            if (rv) {
                sendInvUpdate(ownerOid);
            }
            if (Log.loggingDebug)
                log.debug("DestroyItemHook.deleteObject: success=" + rv);
            return true;
        }
    }
    
    class CreateInvHook implements Hook {
        public boolean processMessage(Message msg, int flags) {

            SubjectMessage oMsg = (SubjectMessage) msg;
            Long oid = oMsg.getSubject();

            Lock objLock = getObjectLockManager().getLock(oid);
            objLock.lock();
            try {
                Long bagOid = createInventory(oid);
                Engine.getAgent().sendLongResponse(msg, bagOid);
                return (bagOid != null);
            } finally {
                objLock.unlock();
            }
        }
    }

    class ItemActivateHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            InventoryClient.ActivateMessage aMsg = (InventoryClient.ActivateMessage) msg;

            Long activatorOid = aMsg.getActivatorOid();
            Long objOid = aMsg.getSubject();
	    Long targetOid = aMsg.getTargetOid();
            if (Log.loggingDebug)
                log.debug("ItemActivateHook: activatorOid=" + activatorOid
		          + ", objOid=" + objOid + ", targetOid=" + targetOid);
            
            Lock objLock = getObjectLockManager().getLock(activatorOid);
            objLock.lock();
            try {
                return activateObject(objOid, activatorOid, targetOid);
            }
            finally {
                objLock.unlock();
            }
        }
    }
    
    /**
     * handles request by looter to loot container
     * @author cedeno
     */
    class LootAllHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            InventoryClient.LootAllMessage lMsg = (InventoryClient.LootAllMessage) msg;
            Long looterOid = lMsg.getSubject();
            Long containerOid = lMsg.getContainerOid();
            if (Log.loggingDebug)
                log.debug("LootAllHook: looter=" + looterOid + ", container=" + containerOid);
            WorldManagerClient.sendObjChatMsg(looterOid,0,"LootAllHook:looting,looter=" + looterOid + ",container="+ containerOid);
            
            boolean rv = lootAll(looterOid, containerOid);
            
            // send response message
            Engine.getAgent().sendBooleanResponse(lMsg, rv);
            return rv;
        }
    }

    /**
     * handles requests to find items by template and returns OID
     *
     */
    class FindItemHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
	    InventoryClient.RemoveOrFindItemMessage findMsg = (InventoryClient.RemoveOrFindItemMessage) msg;
	    Long mobOid = findMsg.getSubject();
	    String method = findMsg.getMethod();

	    log.debug("FindItemHook: got message");
	    if (method.equals(InventoryClient.INV_METHOD_TEMPLATE)) {
		String template = (String)findMsg.getPayload();
		Long resultOid = findItem(mobOid, template);
		Engine.getAgent().sendLongResponse(findMsg, resultOid);
	    }
	    else if (method.equals(InventoryClient.INV_METHOD_TEMPLATE_LIST)) {
		ArrayList<String> templateList = (ArrayList<String>)findMsg.getPayload();
		ArrayList<Long> resultList = findItems(mobOid, templateList);
		Engine.getAgent().sendObjectResponse(findMsg, resultList);
	    }
	    else {
		Log.error("FindItemHook: unknown method=" + method);
	    }
	    return true;
	}
    }

    /**
     * handles reqests to remove items and returns oids of the items remove
     *
     */
    class RemoveItemHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
	    InventoryClient.RemoveOrFindItemMessage delMsg = (InventoryClient.RemoveOrFindItemMessage) msg;
	    Long mobOid = delMsg.getSubject();
	    String method = delMsg.getMethod();

	    log.debug("RemoveItemHook: got message");
	    if (method.equals(InventoryClient.INV_METHOD_OID)) {
		Long oid = (Long)delMsg.getPayload();
		Long result = removeItem(mobOid, oid);
		Engine.getAgent().sendLongResponse(delMsg, result);
	    } else if (method.equals(InventoryClient.INV_METHOD_TEMPLATE)) {
		String template = (String)delMsg.getPayload();
		Long result = removeItem(mobOid, template);
		Engine.getAgent().sendLongResponse(delMsg, result);
	    }
	    else if (method.equals(InventoryClient.INV_METHOD_TEMPLATE_LIST)) {
		ArrayList<String> templateList = (ArrayList<String>)delMsg.getPayload();
		ArrayList<Long> result = removeItems(mobOid, templateList);
		Engine.getAgent().sendObjectResponse(delMsg, result);
	    }
	    else {
		Log.error("RemoveItemHook: unknown method=" + method);
	    }
            sendInvUpdate(mobOid);
	    return true;
	}
    }

    // 
    //
    // API
    //
    //

    /**
     * creates inventory if necessary or loads from database
     */
    abstract public void updateObject(Long mobOid, Long target);
    
    abstract public boolean equipItem(MVObject item, Long activateOid,
            boolean replace);

    abstract protected boolean activateObject(Long objOid, Long activatorOid, Long targetOid);

    /**
     * creates an inventory for the mob
     * returns oid for bag, or null on failure
     */
    abstract protected Long createInventory(Long mobOid);

    /**
     * creates an inventory for the mob
     * returns the bag, or null on failure
     */
    abstract protected SubObjData createInvSubObj(Long mobOid, Template template);

    /**
     * creates an inventory item sub object
     * returns the oid, or null on failure
     */
    abstract protected SubObjData createItemSubObj(Long mobOid, Template template);

    /**
     * performs setup required after loading an inventory sub object
     */
    abstract protected void loadInventory(Entity e);

    /**
     * performs setup required after loading an inventory item
     */
    abstract protected void loadItem(Entity e);

    /**
     * Clean up when unloading an inventory sub object
     */
    abstract protected void unloadInventory(Entity e);

    /**
     * Clean up when unloading an inventory item
     */
    abstract protected void unloadItem(Entity e);

    /**
     * Clean up when deleting an inventory sub object
     */
    abstract protected void deleteInventory(Entity e);

    /**
     * Clean up when deleting an inventory item
     */
    protected void deleteInventory(Long oid)
    {
    }

    /**
     * Clean up when deleting an inventory item
     */
    abstract protected void deleteItem(Entity e);

    /**
     * Clean up when deleting an inventory item
     */
    protected void deleteItem(Long oid)
    {
    }

    /**
     * performs work required after saving an inventory sub object
     */
    abstract protected void saveInventory(Entity e, Namespace namespace);

    /**
     * performs work required after saving an inventory item
     */
    abstract protected void saveItem(Entity e, Namespace namespace);

    /**
     * sends an inventory update message to the client
     */
    abstract protected void sendInvUpdate(Long mobOid);

    abstract protected boolean addItem(Long mobOid, Long containerOid, Long itemOid);

    /**
     * removes item from the bag
     */
    abstract protected boolean removeItemFromBag(Long rootBagOid, Long itemOid);

    /**
     * used to loot objects from a mob, or some other top level container object, such
     * as a chest
     * @param looterOid player/mob that is looting
     * @param mobOid where you are looting from
     * @return success or failure
     */
    abstract protected boolean lootAll(Long looterOid, Long mobOid);

    /**
     * returns whether or not the mob contains the given item
     * @param mobOid the player/mob to check against
     * @param itemOid the item you are checking against
     * @return true if mob contains item, false otherwise
     */
    abstract protected boolean containsItem(Long mobOid, Long itemOid);
    
    abstract protected Long findItem(Long mobOid, String template);
    abstract protected ArrayList<Long> findItems(Long mobOid, ArrayList<String> templateList);

    abstract protected Long removeItem(Long mobOid, Long itemOid);
    abstract protected Long removeItem(Long mobOid, String template);
    abstract protected ArrayList<Long> removeItems(Long MobOid, ArrayList<String> templateList);

    
    protected boolean destroyItem(Long containerOid, Long itemOid) {
        throw new RuntimeException("not implemented");
    }

    protected static final Logger log = new Logger("InventoryPlugin");

    public Lock getLock() {
        return lock;
    }

    protected Lock lock = LockFactory.makeLock("InventoryPlugin");

    // property key for the root bag in a mob/player
    public final static String INVENTORY_PROP_BAG_KEY = "inv.bag";

    public final static String INVENTORY_PROP_BACKREF_KEY = "inv.backref";
}
