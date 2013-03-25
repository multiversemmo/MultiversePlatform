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

package multiverse.mars.plugins;

import java.util.*;
import java.io.*;

import multiverse.msgsys.*;
import multiverse.server.objects.*;
import multiverse.server.plugins.*;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.mars.objects.*;
import multiverse.mars.core.*;

public class MarsInventoryPlugin extends InventoryPlugin implements MessageCallback {

    public void onActivate() {
        super.onActivate();

	getHookManager().addHook(MarsInventoryClient.MSG_TYPE_MARS_INV_FIND,
		new MarsFindItemHook());
        getHookManager().addHook(MarsInventoryClient.MSG_TYPE_TRADE_START_REQ,
                new TradeStartReqHook());
        getHookManager().addHook(MarsInventoryClient.MSG_TYPE_TRADE_OFFER_REQ,
                new TradeOfferReqHook());
	getHookManager().addHook(WorldManagerClient.MSG_TYPE_DESPAWNED, new DespawnedHook());
        getHookManager().addHook(MarsInventoryClient.MSG_TYPE_SWAP_ITEM, new SwapItemHook());

        try {
	    MessageTypeFilter filterNeedsResponse = new MessageTypeFilter();
	    filterNeedsResponse.addType(MarsInventoryClient.MSG_TYPE_MARS_INV_FIND);
	    /* Long sub = */ Engine.getAgent().createSubscription(
                filterNeedsResponse, this, MessageAgent.RESPONDER);

	    MessageTypeFilter filterNoResponse = new MessageTypeFilter();
	    filterNoResponse.addType(MarsInventoryClient.MSG_TYPE_TRADE_START_REQ);
	    filterNoResponse.addType(MarsInventoryClient.MSG_TYPE_TRADE_OFFER_REQ);
	    filterNoResponse.addType(WorldManagerClient.MSG_TYPE_DESPAWNED);
            filterNoResponse.addType(MarsInventoryClient.MSG_TYPE_SWAP_ITEM); 
	    /* Long sub = */ Engine.getAgent().createSubscription(filterNoResponse, this);
        } catch (Exception e) {
            throw new MVRuntimeException("activate failed", e);
        }
    }

    public static MarsItem getMarsItem(Long oid) {
        return (MarsItem)EntityManager.getEntityByNamespace(oid, Namespace.MARSITEM);
    }
    
    public static void registerMarsItem(MarsItem item) {
        EntityManager.registerEntityByNamespace(item, Namespace.MARSITEM);
    }
    
    public static Bag getBag(Long oid) {
        return (Bag)EntityManager.getEntityByNamespace(oid, Namespace.BAG);
    }
    
    public static void registerBag(Bag bag) {
        EntityManager.registerEntityByNamespace(bag, Namespace.BAG);
    }

    class MarsFindItemHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            InventoryClient.RemoveOrFindItemMessage findMsg = (InventoryClient.RemoveOrFindItemMessage)msg;
	    Long mobOid = findMsg.getSubject();
	    String method = findMsg.getMethod();

	    log.debug("MarsFindItemHook: got message");
	    if (method.equals(MarsInventoryClient.INV_METHOD_SLOT)) {
		MarsEquipSlot slot = (MarsEquipSlot)findMsg.getPayload();
		Long resultOid = findItem(mobOid, slot);
		Engine.getAgent().sendLongResponse(findMsg, resultOid);
	    }
	    else {
		Log.error("MarsFindItemHook: unknown method=" + method);
	    }
	    return true;
	}
    }

    protected void sendInvUpdate(Long mobOid)
            {
        InventoryClient.InvUpdateMessage invUpdateMsg = new InventoryClient.InvUpdateMessage(mobOid);
        
        // go through each bag and place contents into inv update msg
        Bag rootBag = getBag(mobOid);
        Long[] items = rootBag.getItemsList();
        for (int pos = 0; pos < items.length; pos++) {
            Long subBagOid = items[pos];
            if (subBagOid == null) {
                log.error("sendInvUpdate: sub bag oid is null");
                continue;
            }
            Bag subBag = getBag(subBagOid);
            if (subBag == null) {
                log.error("sendInvUpdate: sub bag obj is null");
                continue;
            }
            
            sendInvUpdateHelper(invUpdateMsg, pos, subBag);
        }

        Engine.getAgent().sendBroadcast(invUpdateMsg);
    }

    protected void sendInvUpdateHelper(InventoryClient.InvUpdateMessage msg, int bagNum, Bag subBag) {
        Long[] items = subBag.getItemsList();
        for (int pos = 0; pos < items.length; pos++) {
            // get the item
            Long oid = items[pos];
            if (oid == null) {
                continue;
            }
            MarsItem item = getMarsItem(oid);
            if (item == null) {
                Log.warn("sendInvUpdateHelper: item is null, oid=" + oid);
                continue;
            }
            if (Log.loggingDebug)
                log.debug("sendInvUpdateHelper: adding bagNum=" + bagNum + ", bagPos=" + pos +
                          ", itemOid=" + oid + ", itemName=" + item.getName() + ",icon=" + item.getIcon());
            msg.addItem(bagNum, pos, oid, item.getName(), item.getIcon());
        }
    }

    /**
     * returns the bag oid, or null on failure
     */
    public Long createInventory(Long mobOid) {
        // create root bag
        if (Log.loggingDebug)
            log.debug("createinv: creating root bag, playeroid=" + mobOid);
        Bag rootBag = createRootBag(mobOid, 5);
        if (rootBag == null) {
            return null;
        }

        // create 5 sub bags
        for (int subBagNum = 0; subBagNum < 5; subBagNum++) {
            if (Log.loggingDebug)
                log.debug("createinv: creating sub bag, moboid=" + mobOid
                          + ", rootbag=" + rootBag + ", bag pos=" + subBagNum);
            Bag subBag = createSubBag(rootBag, subBagNum, 16);
            if (subBag == null) {
                return null;
            }
        }
        return rootBag.getOid();
    }

    /**
     * returns the bag, or null on failure
     */
    public SubObjData createInvSubObj(Long mobOid, Template template) {
	Map<String, Serializable> props = template.getSubMap(Namespace.BAG);
	if (props == null) {
	    Log.warn("createInvSubObj: no props in ns " + Namespace.BAG);
	    return null;
	}

        // create root bag
        if (Log.loggingDebug)
            log.debug("createInvSubObj: creating root bag, playeroid=" + mobOid);
        Bag rootBag = createRootBag(mobOid, 5);
        if (rootBag == null) {
            return null;
        }

        Boolean persistent = (Boolean)template.get(
            Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_PERSISTENT);
        if (persistent == null)
            persistent = false;
        rootBag.setPersistenceFlag(persistent);

	// copy properties from template to object
	for (Map.Entry<String, Serializable> entry : props.entrySet()) {
	    String key = entry.getKey();
	    Serializable value = entry.getValue();
	    if (!key.startsWith(":")) {
		rootBag.setProperty(key, value);
	    }
	}
	
        // create 5 sub bags
        for (int subBagNum = 0; subBagNum < 5; subBagNum++) {
            if (Log.loggingDebug)
                log.debug("createInvSubObj: creating sub bag, moboid=" + mobOid
                          + ", rootbag=" + rootBag + ", bag pos=" + subBagNum);
            Bag subBag = createSubBag(rootBag, subBagNum, 16);
            if (subBag == null) {
                return null;
            }
        }

	String invItems = (String) props.get(InventoryClient.TEMPL_ITEMS);

        // Mark the bag for saving.  Do this last so we don't save
        // a half-built inventory.
        log.debug("createRootBag: marking root bag dirty for save: " + rootBag);
        Engine.getPersistenceManager().setDirty(rootBag);

        return new SubObjData(Namespace.WORLD_MANAGER,
			      new CreateInventoryHook(mobOid, invItems));
    }

    protected class CreateInventoryHook implements Hook {
	public CreateInventoryHook(Long masterOid, String invItems) {
	    this.masterOid = masterOid;
	    this.invItems = invItems;
	}
	protected Long masterOid;
	protected String invItems;

	public boolean processMessage(Message msg, int flags) {
            if (Log.loggingDebug)
                log.debug("CreateInventoryHook.processMessage: masterOid=" + masterOid + " invItems=" + invItems);
	    Bag rootBag = getBag(masterOid);

	    if (invItems == null)
		return true;
	    if (invItems.equals("")) {
                return true;
            }
	    for (String itemName : invItems.split(";")) {
		boolean equip = false;
		itemName = itemName.trim();
		if (itemName.startsWith("*")) {
		    itemName = itemName.substring(1);
		    equip = true;
		}
                if (Log.loggingDebug)
                    log.debug("CreateInventoryHook.processMessage: creating item=" + itemName + " equip=" + equip);
                Template itemTemplate = new Template();
                itemTemplate.put(Namespace.OBJECT_MANAGER,
                        ObjectManagerClient.TEMPL_PERSISTENT,
                        rootBag.getPersistenceFlag());
		Long itemOid = ObjectManagerClient.generateObject(itemName, itemTemplate);
                if (Log.loggingDebug)
                    log.debug("CreateInventoryHook.processMessage: created item=" + itemOid);
		addItem(masterOid, rootBag.getOid(), itemOid);
                if (Log.loggingDebug)
                    log.debug("CreateInventoryHook.processMessage: added item to inv=" + itemOid);
		if (equip) {
		    MarsItem item = getMarsItem(itemOid);
		    equipItem(item, masterOid, false);
		}
	    }
	    return true;
	}
    }

    // returns the root bag
    private Bag createRootBag(Long ownerOid, int numSlots) {
        // create the bag
        Bag bag = new Bag(ownerOid);
        bag.setName("RootBag_Owner" + ownerOid);
        bag.setNumSlots(numSlots);

        // set back reference to the owner
        bag.setProperty(InventoryPlugin.INVENTORY_PROP_BACKREF_KEY, ownerOid);

        registerBag(bag);

        return bag;
    }

    // bag in a bag - we dont have to send a message to the worldmgr
    // to set the forward reference, but otherwise same
    // as createRootBag
    private Bag createSubBag(Bag parentBag, int parentBagSlotNum, int numSlots) {
        // create the bag
        Bag bag = new Bag();
        bag.setOid(Engine.getOIDManager().getNextOid());
        bag.setNumSlots(numSlots);

        // set back reference to the owner
        bag.setProperty(InventoryPlugin.INVENTORY_PROP_BACKREF_KEY, parentBag.getOid());

        // set the forward reference on the parentBag
        parentBag.putItem(parentBagSlotNum, bag.getOid());

        // bind the bag locally
        registerBag(bag);
        SubjectFilter wmFilter = new SubjectFilter(bag.getOid());
        wmFilter.addType(EnginePlugin.MSG_TYPE_SET_PROPERTY);
        wmFilter.addType(EnginePlugin.MSG_TYPE_GET_PROPERTY);
        /* Long sub = */ Engine.getAgent().createSubscription(wmFilter, MarsInventoryPlugin.this);
        
        return bag;
    }

    class SwapItemHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage placeMsg = (ExtensionMessage)msg;
            Long playerOid = (Long)placeMsg.getProperty("playerOid");
            Long itemOid = (Long)placeMsg.getProperty("itemOid");
            Long destinationOid = (Long)placeMsg.getProperty("destinationOid");
            Integer containerId = (Integer)placeMsg.getProperty("containerId");
            Integer slotId = (Integer)placeMsg.getProperty("slotId");

            // get the player's rootbag
            Bag rootBag = getBag(playerOid);
            Long[] bagList = rootBag.getItemsList();
            Long destBagOid = bagList[containerId];
            // get the bag where you are placing your item
            Bag destBag = getBag(destBagOid);

            // the following for loop finds the bag where your item is originating
            Integer sourceSlot = null;
            Bag sourceBag = null;
            for (Long sourceBagOid : bagList) {
                sourceBag = getBag(sourceBagOid);
                sourceSlot = sourceBag.findItem(itemOid);
                if (sourceSlot != null)
                    break;
            }
            // remove the item from that bag
            boolean sc = sourceBag.removeItem(itemOid);
            Log.debug("SwapItem: Removing Item from source bag, sc=" + sc);

            MarsItem swappedItem = null;
            boolean av = false;
            if(destinationOid != null) {
                // if placing your item on another item, swap it with the old item
                boolean dt = destBag.removeItem(destinationOid);
                Log.debug("SwapItem: Removing item from destination bag, dt=" + dt);
                av = sourceBag.putItem(sourceSlot, destinationOid);
                swappedItem = getMarsItem(destinationOid);
            }
            // swap or no swap, put cursor item in the place you clicked
            boolean rv = destBag.putItem(slotId, itemOid);
            if (Log.loggingDebug)
                log.debug("addItem: adding to bag, rv=" + rv);

            MarsItem item = getMarsItem(itemOid);

            // set reference properties and mark dirty for persistence
            if (rv) {
                item.setProperty(INVENTORY_PROP_BACKREF_KEY, destBagOid);
            }

            if (av) {
                swappedItem.setProperty(INVENTORY_PROP_BACKREF_KEY, sourceBag.getOid());
                Engine.getPersistenceManager().setDirty(swappedItem);
            }

            // mark dirty
            Engine.getPersistenceManager().setDirty(destBag);
            Engine.getPersistenceManager().setDirty(sourceBag);
            Engine.getPersistenceManager().setDirty(item);

            // of course, let the client know you changed item positions
            sendInvUpdate(playerOid);
            return true;
        }
    }

    /**
     * handles reqests to start a trading session
     *
     */
    class TradeStartReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage startMsg = (ExtensionMessage)msg;
            Long trader1Oid = (Long)startMsg.getProperty("requesterOid");
            Long trader2Oid = (Long)startMsg.getProperty("partnerOid");

            Log.debug("TradeStartReqHook: trader1=" + trader1Oid + " trader2=" + trader2Oid);
	    if (tradeSessionMap.containsKey(trader1Oid) || tradeSessionMap.containsKey(trader2Oid)) {
 		sendTradeComplete(trader1Oid, trader2Oid, MarsInventoryClient.tradeBusy);
		return true;
	    }
	    TradeSession tradeSession = new TradeSession(trader1Oid, trader2Oid);
	    tradeSessionMap.put(trader1Oid, tradeSession);
	    tradeSessionMap.put(trader2Oid, tradeSession);
 	    sendTradeStart(trader1Oid, trader2Oid);
 	    sendTradeStart(trader2Oid, trader1Oid);
	    return true;
	}
    }

    /**
     * send a mv.TRADE_COMPLETE message to trader1, telling it that a trade with trader2 has completed
     */
    protected static void sendTradeComplete(Long trader1, Long trader2, byte status) {
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.TRADE_COMPLETE");
        props.put("status", status);
        TargetedExtensionMessage msg = new TargetedExtensionMessage(MarsInventoryClient.MSG_TYPE_TRADE_COMPLETE,
                                                                    trader1, trader2, false, props);
	Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * sends a mv.TRADE_START message to trader1 telling it that a trade has started with trader2
     */
    protected static void sendTradeStart(Long trader1, Long trader2) {
        TargetedExtensionMessage msg = new TargetedExtensionMessage(MarsInventoryClient.MSG_TYPE_TRADE_START,
                                                                    "mv.TRADE_START", trader1, trader2);
	Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * handles reqests to update an existing trading session
     *
     */
    class TradeOfferReqHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage tradeMsg = (ExtensionMessage)msg;
            Long trader1 = (Long)tradeMsg.getProperty("requesterOid");
            Long trader2 = (Long)tradeMsg.getProperty("partnerOid");

            Log.debug("TradeOfferReqHook: trader1=" + trader1 + " trader2=" + trader2);
	    TradeSession tradeSession = tradeSessionMap.get(trader1);

	    // fail if trade session doesn't exist or is invalid
	    if ((tradeSession == null) || !tradeSession.isTrader(trader2)) {
		sendTradeComplete(trader1, trader2, MarsInventoryClient.tradeFailed);
		if (tradeSession != null) {
		    tradeSessionMap.remove(trader1);
		    Long partner = tradeSession.getPartnerOid(trader1);
		    tradeSessionMap.remove(partner);
		    sendTradeComplete(partner, trader1, MarsInventoryClient.tradeFailed);
		}
		return true;
	    }

	    List<Long> offer = (List<Long>)tradeMsg.getProperty("offerItems");
	    // if offer is cancelled or invalid, fail
            boolean cancelled = (Boolean)tradeMsg.getProperty("cancelled");
	    if (cancelled || !validateTradeOffer(trader1, offer)) {
		byte status = MarsInventoryClient.tradeFailed;
		if (cancelled) {
		    status = MarsInventoryClient.tradeCancelled;
		}
		tradeSessionMap.remove(trader1);
		tradeSessionMap.remove(trader2);
		sendTradeComplete(trader1, trader2, status);
		sendTradeComplete(trader2, trader1, status);
	    }

	    // update session with this offer
            boolean accepted = (Boolean)tradeMsg.getProperty("accepted");
	    tradeSession.updateOffer(trader1, offer, accepted);

	    // if session is complete, then complete the trade
	    if (tradeSession.isComplete()) {
		tradeSessionMap.remove(trader1);
		tradeSessionMap.remove(trader2);
		sendTradeComplete(trader1, trader2, MarsInventoryClient.tradeSuccess);
		sendTradeComplete(trader2, trader1, MarsInventoryClient.tradeSuccess);
		completeTrade(tradeSession);
		return true;
	    }

	    // otherwise, send trade updates to both traders
	    sendTradeOfferUpdate(trader1, trader2, tradeSession);
	    sendTradeOfferUpdate(trader2, trader1, tradeSession);
	    return true;
	}
    }

    public boolean validateTradeOffer(Long trader, List<Long> offer) {
	Set<Long> itemSet = new HashSet<Long>();

	for (Long itemOid : offer) {
	    // -1 is an empty slot in the offer
	    if (itemOid.equals(OIDManager.invalidOid)) {
		continue;
	    }
	    // don't allow duplicate items in trade offer
	    if (!itemSet.add(itemOid)) {
		return false;
	    }
	}

	Bag rootBag = getBag(trader);

	// go through all items trader has in inventory and remove from itemSet.
	// anything left, the trader doesn't have.
	for (Long subBagOid : rootBag.getItemsList()) {
	    if (subBagOid != null) {
		Bag subBag = getBag(subBagOid);
		for (Long itemOid : subBag.getItemsList()) {
		    itemSet.remove(itemOid);
		}
	    }
	}

	// if there are any items we didn't find, fail
	if (!itemSet.isEmpty()) {
	    return false;
	}

	return true;
    }

    public static void sendTradeOfferUpdate(Long trader1, Long trader2, TradeSession tradeSession) {
        Boolean accepted1 = tradeSession.getAccepted(trader1);
        Boolean accepted2 = tradeSession.getAccepted(trader2);
        LinkedList<LinkedList> offer1 = sendTradeOfferUpdateHelper(trader1, tradeSession);
        LinkedList<LinkedList> offer2 = sendTradeOfferUpdateHelper(trader2, tradeSession);

        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.TRADE_OFFER_UPDATE");
        props.put("accepted1", accepted1);
        props.put("accepted2", accepted2);
        props.put("offer1", offer1);
        props.put("offer2", offer2);
        TargetedExtensionMessage msg = new TargetedExtensionMessage(MarsInventoryClient.MSG_TYPE_TRADE_OFFER_UPDATE,
                                                                    trader1, trader2, false, props);
	Engine.getAgent().sendBroadcast(msg);
    }

    protected static LinkedList<LinkedList> sendTradeOfferUpdateHelper(Long traderOid, TradeSession tradeSession) {
        LinkedList<LinkedList> offer = new LinkedList<LinkedList>();
	for (Long itemOid : tradeSession.getOffer(traderOid)) {
	    LinkedList<Object> info = new LinkedList<Object>();
	    if ((itemOid == null) || itemOid.equals(OIDManager.invalidOid)) {
                info.add(OIDManager.invalidOid);
                info.add("");
                info.add("");
	    }
	    else {
		MarsItem item = getMarsItem(itemOid);
                info.add(itemOid);
                info.add(item.getName());
                info.add(item.getIcon());
	    }
	    offer.add(info);
	}
        return offer;
    }

    public void completeTrade(TradeSession tradeSession) {
	Long trader1Oid = tradeSession.getTrader1();
	Long trader2Oid = tradeSession.getTrader2();
	Bag trader1Inv = getBag(trader1Oid);
	Bag trader2Inv = getBag(trader2Oid);
	List<Long> offer1 = tradeSession.getOffer(trader1Oid);
	List<Long> offer2 = tradeSession.getOffer(trader2Oid);

	for (Long itemOid : offer1) {
	    removeItem(trader1Oid, itemOid);
	}
	for (Long itemOid : offer2) {
	    removeItem(trader2Oid, itemOid);
	}
	for (Long itemOid : offer1) {
	    addItem(trader2Oid, trader2Inv.getOid(), itemOid);
	}
	for (Long itemOid : offer2) {
	    addItem(trader1Oid, trader1Inv.getOid(), itemOid);
	}
        sendInvUpdate(trader1Oid);
        sendInvUpdate(trader2Oid);
    }

    /**
     * when an object despawns, end any outstanding trade sessions
     *
     */
    class DespawnedHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
	    WorldManagerClient.DespawnedMessage despawnedMsg = (WorldManagerClient.DespawnedMessage)msg;
	    Long oid = despawnedMsg.getSubject();
	    TradeSession tradeSession = tradeSessionMap.get(oid);
	    if (tradeSession != null) {
		Long trader1 = tradeSession.getTrader1();
		Long trader2 = tradeSession.getTrader2();
		tradeSessionMap.remove(trader1);
		tradeSessionMap.remove(trader2);
		sendTradeComplete(trader1, trader2, MarsInventoryClient.tradeFailed);
		sendTradeComplete(trader2, trader1, MarsInventoryClient.tradeFailed);
	    }
	    return true;
	}
    }

    Map<Long, TradeSession> tradeSessionMap = new HashMap<Long, TradeSession>();

    /**
     * returns the bag, or null on failure
     */
    protected SubObjData createItemSubObj(Long masterOid, Template template) {
        if (Log.loggingDebug)
            log.debug("createItemSubObj: creating item=" + template.getName()
		      + " masterOid=" + masterOid);
	MarsItem item = new MarsItem(masterOid);
        item.setName(template.getName());
	item.setTemplateName(template.getName());

	Map<String, Serializable> props = template.getSubMap(Namespace.MARSITEM);
	if (props == null) {
	    Log.warn("createItemSubObj: no props in ns " + Namespace.MARSITEM);
	    return null;
	}

        Boolean persistent = (Boolean)template.get(
            Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_PERSISTENT);
        if (persistent == null)
            persistent = false;
        item.setPersistenceFlag(persistent);

	// copy properties from template to object
	for (Map.Entry<String, Serializable> entry : props.entrySet()) {
	    String key = entry.getKey();
	    Serializable value = entry.getValue();
	    if (!key.startsWith(":")) {
		item.setProperty(key, value);
	    }
	}
	
	// register the entity
	registerMarsItem(item);

        return new SubObjData();
    }

    protected void loadInventory(Entity e) {
	Bag rootBag = (Bag) e;

	Long ownerOid = (Long)rootBag.getProperty(InventoryPlugin.INVENTORY_PROP_BACKREF_KEY);

        boolean dirty = false;
	for (Long subBagOid : rootBag.getItemsList()) {
	    if (subBagOid != null) {
		if (loadSubBag(subBagOid, rootBag))
                    dirty = true;
	    }
	}
        if (dirty)
            Engine.getPersistenceManager().setDirty(rootBag);

	sendInvUpdate(ownerOid);
    }

    protected boolean loadSubBag(Long subBagOid, Entity rootBag) {
	Bag subBag = (Bag) Engine.getDatabase().loadEntity(subBagOid,
                Namespace.BAG);
	registerBag(subBag);
        boolean dirty = false;
	for (Long itemOid : subBag.getItemsList()) {
	    if (itemOid != null) {
		if (ObjectManagerClient.loadObject(itemOid) == null) {
                    // If we can't load the item, then delete reference
                    // to it from the bag.
                    Log.warn("loadSubBag: item "+itemOid+" does not exist, removing from bag "+subBagOid);
                    boolean rv = subBag.removeItem(itemOid);
                    if (rv)
                        dirty = true;
                }
	    }
	}
        return dirty;
    }

    protected void unloadInventory(Entity e)
    {
        Bag rootBag = (Bag) e;

        Long ownerOid = (Long)rootBag.getProperty(InventoryPlugin.INVENTORY_PROP_BACKREF_KEY);

        if (e.isDeleted())
            return;

        // If the root bag is dirty, then save immediately.  We can't
        // wait for the PersistenceManager because the sub bag entities
        // will be unregistered, so the save wouldn't do anything.
        if (Engine.getPersistenceManager().isDirty(e)) {
            Engine.getPersistenceManager().clearDirty(e);
            saveInventory(e,null);
        }

        Log.debug("unloadInventory: oid="+e.getOid()+" owner="+ownerOid);

        for (Long subBagOid : rootBag.getItemsList()) {
            if (subBagOid != null) {
                Bag subBag = getBag(subBagOid);
                if (subBag == null)
                    continue;
                for (Long itemOid : subBag.getItemsList()) {
                    if (itemOid == null)
                        continue;
                    if (Log.loggingDebug)
                        Log.debug("unloadInventory: bag oid="+e.getOid()+
                                " subbag="+subBagOid+ " item="+itemOid);
                    ObjectManagerClient.unloadObject(itemOid);
                }
                EntityManager.removeEntityByNamespace(subBagOid, Namespace.BAG);
            }
        }
    }

    protected void deleteInventory(Entity e)
    {
        Bag rootBag = (Bag) e;

        Long ownerOid = (Long)rootBag.getProperty(InventoryPlugin.INVENTORY_PROP_BACKREF_KEY);

        Log.debug("deleteInventory: oid="+e.getOid()+" owner="+ownerOid);

        for (Long subBagOid : rootBag.getItemsList()) {
            if (subBagOid != null) {
                Bag subBag = getBag(subBagOid);
                if (subBag == null)
                    continue;
                for (Long itemOid : subBag.getItemsList()) {
                    if (itemOid == null)
                        continue;
                    if (Log.loggingDebug)
                        Log.debug("deleteInventory: bag oid="+e.getOid()+
                                " subbag="+subBagOid+ " item="+itemOid);
                    ObjectManagerClient.deleteObject(itemOid);
                }
                subBag.setDeleted();
                EntityManager.removeEntityByNamespace(subBagOid, Namespace.BAG);
                Engine.getDatabase().deleteObjectData(subBagOid);
            }
        }
    }


    protected void loadItem(Entity e) {
    }

    protected void unloadItem(Entity e) {
    }

    protected void deleteItem(Entity item)
    {
        if (Log.loggingDebug)
            Log.debug("deleteItem: oid="+item.getOid());

        Long subBagOid = (Long)item.getProperty(INVENTORY_PROP_BACKREF_KEY);
        if (subBagOid != null) {
            if (removeItemFromBagHelper(subBagOid, item)) {
                Bag subBag = getBag(subBagOid);
                Long rootBagOid = (Long)subBag.getProperty(
                    INVENTORY_PROP_BACKREF_KEY);
                Bag rootBag = getBag(rootBagOid);
                Engine.getPersistenceManager().setDirty(rootBag);
                sendInvUpdate(rootBagOid);
            }
        }

        // EnginePlugin DeleteSubObject handler has already removed
        // entity, so we just need to delete from data base.

        Engine.getDatabase().deleteObjectData(item.getOid());
    }

    protected void saveInventory(Entity e, Namespace namespace) {
	Bag rootBag = (Bag) e;
        if (Log.loggingDebug)
            log.debug("saveInventory: rootBag=" + rootBag.getOid());

	for (Long subBagOid : rootBag.getItemsList()) {
	    if (subBagOid != null) {
		Bag subBag = getBag(subBagOid);
                if (subBag == null) {
                    log.error("saveInventory: subBag not found oid="+subBagOid);
                    continue;
                }
                if (Log.loggingDebug)
                    log.debug("saveInventory: subBag oid=" + subBag.getOid());
		Engine.getDatabase().saveObject(subBag, Namespace.BAG);

		for (Long itemOid : subBag.getItemsList()) {
		    if (itemOid != null) {
                        if (Log.loggingDebug)
                            log.debug("saveInventory: saving itemOid=" + itemOid);
			ObjectManagerClient.saveObject(itemOid);
                        if (Log.loggingDebug)
                            log.debug("saveInventory: done saving itemOid=" + itemOid);
		    }
		}
	    }
	}
    }

    protected void saveItem(Entity e, Namespace namespace) {
    }

    /**
     * adds the item to the container item must not be in a container already
     * sets the item's containedBy backlink to the container
     */
    protected boolean addItem(Long mobOid, Long rootBagOid, Long itemOid) {
        lock.lock();
        try {
            // get bag
	    Bag rootBag = getBag(rootBagOid);

            if (Log.loggingDebug)
                log.debug("addItem: found bag object: " + rootBag);

            // get item
            Entity item = getMarsItem(itemOid);
	    if (item == null) {
		item = getBag(itemOid);
	    }
            if (item == null) {
                Log.warn("addItem: item is null: oid=" + itemOid);
                return false;
            }
            if (Log.loggingDebug)
                log.debug("addItem: found item: " + item);

            // check each subbag and see if it can be added there
            Long[] subBags = rootBag.getItemsList();
            for (int pos = 0; pos < subBags.length; pos++) {
                Long subBag = subBags[pos];
                if (addItemHelper(subBag, pos, item)) {
		    Engine.getPersistenceManager().setDirty(rootBag);
                    return true;
                }
            }
            return false;
        } finally {
            lock.unlock();
        }
    }
    
    protected boolean addItemHelper(Long subBagOid, int slotNum, Entity item) {
        // get the bag object
        Bag subBag = getBag(subBagOid);
        if (subBag == null) {
            Log.warn("addItemHelper: did not find sub bag: " + subBagOid + " for bagoid=" + subBagOid);
            return false;
        }

        // add backref to item
        if (item.getProperty(INVENTORY_PROP_BACKREF_KEY) != null) {
            Log.warn("addItem: item is already in a container, itemOid="
                    + item.getOid());
            return false;
        }
        // add item to bag
        boolean rv = subBag.addItem(item.getOid());
        if (Log.loggingDebug)
            log.debug("addItem: adding to bag, rv=" + rv);

        if (rv) {
            item.setProperty(INVENTORY_PROP_BACKREF_KEY, subBagOid);
        }

        // mark dirty
        Engine.getPersistenceManager().setDirty(item);
        return rv;
    }


    protected boolean removeItemFromBag(Long rootBagOid, Long itemOid) {
        lock.lock();
        try {
            // get root bag
            Bag rootBag = getBag(rootBagOid);
            if (Log.loggingDebug)
                log.debug("removeItemFromBag: found root bag object: " + rootBag);

            // get item
            MarsItem item = getMarsItem(itemOid);
            if (item == null) {
                Log.warn("removeItemFromBag: item is null: oid=" + itemOid);
                return false;
            }
            if (Log.loggingDebug)
                log.debug("removeItemFromBag: found item: " + item);

            // check each subbag to find its container
            Long[] subBags = rootBag.getItemsList();
            for (int pos = 0; pos < subBags.length; pos++) {
                Long subBag = subBags[pos];
                if (removeItemFromBagHelper(subBag, item)) {
		    Engine.getPersistenceManager().setDirty(rootBag);
                    return true;
                }
            }
            return false;
        } finally {
            lock.unlock();
        }
    }
    
    protected  boolean removeItemFromBagHelper(Long subBagOid, Entity item) {
        // get the bag object
        Bag subBag = getBag(subBagOid);
        if (subBag == null) {
            Log.warn("removeItemFromBagHelper: did not find sub bag: " + subBagOid);
            return false;
        }

        // does bag contain the item?
        Integer slotNum = subBag.findItem(item.getOid());
        if (slotNum == null) {
            if (Log.loggingDebug)
                log.debug("removeItemFromBagHelper: item not in bag itemOid=" + item.getOid() + " bagOid="+subBagOid);
            return false;
        }
        
        // found the item
        if (Log.loggingDebug)
            log.debug("removeItemFromBagHelper: found - slot=" + slotNum + ", itemOid=" + item.getOid());
        
        // remove item from bag - we seperate the logic here from finding the item
        // because perhaps there was some other reason why the remove failed
        boolean rv = subBag.removeItem(item.getOid());
        if (rv == false) {
            if (Log.loggingDebug)
                log.debug("removeItemFromBagHelper: remove item failed");
            return false;
        }
        
        // remove the back reference
        item.setProperty(INVENTORY_PROP_BACKREF_KEY, null);

        // mark dirty
        Engine.getPersistenceManager().setDirty(item);
        if (Log.loggingDebug)
            log.debug("removeItemFromBagHelper: remove from bag, rv=" + rv);
        return rv;
    }
    
    /**
     * creates inventory for object or loads from database
     */
    public void updateObject(Long mobOid, Long target) {
        // This is a player-type mob if it's asking about itself, i.e., subject == target
        if (!mobOid.equals(target)) {
            if (Log.loggingDebug)
                log.debug("updateObject: obj is not a player, ignoring: " + mobOid);
            return;
        }
        if (Log.loggingDebug)
            log.debug("updateObject: obj is a player: " + mobOid);
        
        // send out inv update
        Bag bag = getBag(mobOid);
        if (bag != null)
            sendInvUpdate(mobOid);
        else 
            log.debug("updateObject: could not find entity in " + Namespace.BAG + " for mobOid " + mobOid);
	return;
    }
    
    /**
     * used to loot objects from a mob, or some other top level container object, such
     * as a chest
     * @param looterOid player/mob that is looting
     * @param mobOid where you are looting from
     * @return success or failure
     */
    protected boolean lootAll(Long looterOid, Long mobOid) {
        log.debug("lootAll: looterOid=" + looterOid + ", mobOid=" + mobOid);
        
        Long rootLooterBagOid;
        lock.lock();
        try {
            // find the looters root bag
            rootLooterBagOid = looterOid;
            if (rootLooterBagOid == null) {
                log.debug("lootAll: cant find rootLooterBagOid");
                return false;
            }
            Bag rootLooterBag = getBag(rootLooterBagOid);
            if (rootLooterBag == null) {
                log.debug("lootAll: could not find root bag for looter");
                return false;
            }
            
            // find the mob's root bag
            Long rootMobBagOid = mobOid;

            if (rootMobBagOid == null) {
                log.debug("lootAll: mobRootBagOid is null, failed");
                return false;
            }
            log.debug("lootAll: found mobs root bag: " + rootMobBagOid);
            Bag rootMobBag = getBag(rootMobBagOid);
            if (rootMobBag == null) {
                log.debug("lootAll: could not find root bag for mob");
                return false;
            }
            
            // loot all subbags
            for (int slotNum=0; slotNum < rootMobBag.getNumSlots(); slotNum++) {
                Bag subBag = getBag(rootMobBag.getItem(slotNum));
                log.debug("lootAll: found mob's subbag, slotNum=" + slotNum + ", subBag=" + subBag);
                lootAllHelper(looterOid, rootLooterBag, mobOid, rootMobBag, subBag);
            }
        }
        finally {
            lock.unlock();
        }
        EnginePlugin.setObjectPropertyNoResponse(mobOid, Namespace.WORLD_MANAGER, "lootable", Boolean.FALSE);

        sendInvUpdate(looterOid);
        return true;
    }
    
    protected boolean lootAllHelper(Long looterOid, Bag looterRootBag, Long mobOid, Bag mobRootBag, Bag mobSubBag) {
        // assumes locking
        // process each item in bag
        for (int slotNum=0; slotNum < mobSubBag.getNumSlots(); slotNum++) {
            Long itemOid = mobSubBag.getItem(slotNum);
            if (itemOid == null) {
                log.debug("lootAllHelper: slotNum " + slotNum + " is empty");
                continue;
            }
            
            log.debug("lootAllHelper: processing sub bags item slot="+slotNum+
                " oid=" + itemOid);
            
            // remove the item from the mobs root bag
            boolean rv = removeItemFromBag(mobRootBag.getOid(), itemOid);
            log.debug("lootAllHelper: removed oid=" + itemOid + ", rv=" + rv);
            if (! rv) {
                continue;
            }

            Entity item = getMarsItem(itemOid);
            if (item != null)
                ObjectManagerClient.setPersistenceFlag(itemOid,true);
            else
                continue;

            // add item to the looter's root bag
            rv = addItem(looterOid, looterRootBag.getOid(), itemOid);
            log.debug("lootAllHelper: addItem to looter, oid=" + itemOid + ", rv=" + rv);
            if (! rv) {
                continue;
            }
        }
        log.debug("lootAllHelper: done processing subbag " + mobSubBag);
        return true;
    }

    /**
     * returns whether or not the mob contains the given item
     * @param mobOid the player/mob to check against
     * @param itemOid the item you are checking against
     * @return true if mob contains item, false otherwise
     */
    protected boolean containsItem(Long mobOid, Long itemOid) {
        lock.lock();
        try {
            MarsItem item = getMarsItem(itemOid);
            if (item == null) {
                return false;
            }
            if (item.isDeleted()) {
                return false;
            }
            Long subBagOid = (Long)item.getProperty(INVENTORY_PROP_BACKREF_KEY);
            if (subBagOid == null) {
                return false;
            }
            
            // get the sub-bag
            Bag subBag = getBag(subBagOid);
            if (subBag == null) {
                return false;
            }
            
            // get the mob owner oid -- which matches the parent bag oid by convention
            Long rootBagOid = (Long)subBag.getProperty(INVENTORY_PROP_BACKREF_KEY);
            if (rootBagOid == null) {
                return false;
            }
            return (mobOid.equals(rootBagOid));
        } finally {
            lock.unlock();
        }
    }
    
    /**
     * finds an item based on the template name
     */
    protected Long findItem(Long mobOid, String template) {
	lock.lock();
	try {
	    // find the mob's root bag
            if (Log.loggingDebug)
                log.debug("findItem: mob=" + mobOid + " template=" + template);
	    Long rootBagOid = mobOid;
	    if (rootBagOid == null) {
		log.debug("findItem: cant find rootBagOid");
		return null;
	    }
	    Bag rootBag = getBag(rootBagOid);
	    if (rootBag == null) {
		log.debug("findItem: could not find root bag");
		return null;
	    }

	    ArrayList<Long> resultList = new ArrayList<Long>();
	    findItemHelper(mobOid, rootBag, template, resultList);
	    return resultList.get(0);
	}
	finally {
	    lock.unlock();
	}
    }

    protected ArrayList<Long> findItems(Long mobOid, ArrayList<String> templateList) {
	lock.lock();
	try {
	    // find the mob's root bag
            if (Log.loggingDebug)
                log.debug("findItem: mob=" + mobOid + " templateList=" + templateList);
	    Long rootBagOid = mobOid;
	    if (rootBagOid == null) {
		log.debug("findItem: cant find rootBagOid");
		return null;
	    }
	    Bag rootBag = getBag(rootBagOid);
	    if (rootBag == null) {
		log.debug("findItem: could not find root bag");
		return null;
	    }

	    ArrayList<Long> resultList = new ArrayList<Long>();
	    for (String template : templateList) {
		findItemHelper(mobOid, rootBag, template, resultList);
	    }
	    return resultList;
	}
	finally {
	    lock.unlock();
	}
    }

    protected boolean findItemHelper(Long mobOid, Bag rootBag, String template, ArrayList<Long>resultList) {
	for (Long subBagOid : rootBag.getItemsList()) {
	    if (subBagOid == null)
		continue;
	    Bag subBag = getBag(subBagOid);
	    for (Long itemOid : subBag.getItemsList()) {
		if (itemOid == null)
		    continue;
		MarsItem item = getMarsItem(itemOid);
		if (template.equals(item.getTemplateName())) {
		    if (resultList.contains(itemOid))
			continue;
                    if (Log.loggingDebug)
                        log.debug("findItemHelper: adding item to resultList=" + itemOid);
		    resultList.add(itemOid);
		    return true;
		}
	    }
	}
	resultList.add(null);
	return false;
    }

    protected Long findItem(Long mobOid, MarsEquipSlot slot) {
	lock.lock();
	try {
	    EquipMap equipMap = getEquipMap(mobOid);
	    Long itemOid = equipMap.get(slot);
	    if (itemOid != null) {
		return equipMap.get(slot);
	    }
	    else {
		return null;
	    }
	}
	finally {
	    lock.unlock();
	}
    }

    protected Long removeItem(Long mobOid, Long itemOid) {
	lock.lock();
	try {
	    MarsItem item = getMarsItem(itemOid);
	    if (item == null)
		return null;
	    unequipItem(item, mobOid);
	    Long rootBagOid = mobOid;
	    if (rootBagOid == null) {
		log.debug("removeItem: cant find rootBagOid");
		return null;
	    }
	    Boolean result = removeItemFromBag(rootBagOid, itemOid);
	    if (result == true) {
		return itemOid;
	    } else {
		return null;
	    }
        }
	finally {
	    lock.unlock();
	}
    }

    protected Long removeItem(Long mobOid, String template) {
	lock.lock();
	try {
	    Long itemOid = findItem(mobOid, template);
            if (Log.loggingDebug)
                log.debug("removeItem: mobOid=" + mobOid + " template=" + template + " ItemOid=" + itemOid);
	    return removeItem(mobOid, itemOid);
	}
	finally {
	    lock.unlock();
	}
    }

    protected ArrayList<Long> removeItems(Long mobOid, ArrayList<String> templateList) {
	lock.lock();
	try {
            if (Log.loggingDebug)
                log.debug("removeItems: mobOid=" + mobOid + " templateList=" + templateList);
	    ArrayList<Long> itemList = findItems(mobOid, templateList);
	    if (itemList.contains(null)) {
		return null;
	    }
	    for (Long itemOid : itemList) {
		removeItem(mobOid, itemOid);
	    }
	    return itemList;
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * activates the object
     */
    protected boolean activateObject(Long objOid, Long activatorOid, Long targetOid) {
	MarsItem item = getMarsItem(objOid);
        if (item == null) {
            Log.warn("ActivateHook: item is null, oid=" + objOid);
            return false;
        }

        return item.activate(activatorOid, targetOid);
    }

    /**
     * equips item to primary slot parameter 'replace' indicates to replace an
     * item occupying the slot. the existing item will be unequipped first
     */
    public boolean equipItem(MVObject itemObj, Long activatorOid,
            boolean replace) {
        log.debug("MarsInventoryPlugin.equipItem: item=" + itemObj
                + ", activatorOid=" + activatorOid);

        MarsItem item = MarsItem.convert(itemObj);

        // is activator allowed to use the item
        // for now, ignore if the item has no callback
        PermissionCallback cb = item.permissionCallback();
        if ((cb != null) && (!cb.use(activatorOid))) {
            log.warn("permission callback failed");
            return false;
        }

        // get the primary slot for the item
        MarsEquipSlot slot = item.getPrimarySlot();
        if (slot == null) {
            Log.warn("MarsInventoryPlugin: slot is null for item: " + item);
            return false;
        }

        EquipMap equipMap;
        lock.lock();
        try {
            equipMap = getEquipMap(activatorOid);

            // is the slot free?
	    Long oItemOid = equipMap.get(slot);
            if (oItemOid != null) {
		MarsItem oItemObj = getMarsItem(oItemOid);
                if (Log.loggingDebug)
                    log.debug("MarsInventoryPlugin: slot occupied");
                if (replace) {
                    unequipItem(oItemObj, activatorOid);
                } else {
                    return false;
                }
            }

            // place object in slot
            equipMap.put(slot, item.getMasterOid());
	    setDirty(activatorOid);
        } finally {
            lock.unlock();
        }

        if (Log.loggingDebug)
            log.debug("MarsInventoryPlugin: calling addDC, activatorOid="
                      + activatorOid + ", item=" + item);
        // update world manager's displaycontext for the obj
        if (!addDC(activatorOid, item)) {
            Log.warn("MarsInventoryPlugin.equipItem: problem adding dc for item " + item);
        }
        return true;
    }

    /**
     * unequips item
     */
    public boolean unequipItem(MVObject itemObj, Long mobOid) {
        log.debug("MarsInventoryPlugin.unequipItem: item=" + itemObj
                + ", mobOid=" + mobOid);

        MarsItem item = MarsItem.convert(itemObj);

        // is activator allowed to use the item
        // for now, ignore if the item has no callback
        PermissionCallback cb = item.permissionCallback();
        if ((cb != null) && (!cb.use(mobOid))) {
            log.warn("callback failed");
            return false;
        }

        lock.lock();
        try {
            // where is this currently equipped
            EquipMap equipMap = getEquipMap(mobOid);
            MarsEquipSlot slot = equipMap.getSlot(item.getMasterOid());
            if (slot == null) {
                // item is not currently equipped
                Log.warn("MarsInventoryPlugin.unequipItem: item not equipped: item=" + item);
                return false;
            }

            // remove the item from the map
            equipMap.remove(slot);
	    setDirty(mobOid);
        } finally {
            lock.unlock();
        }
        if (!removeDC(mobOid, item)) {
            Log.warn("MarsInventoryPlugin.unequipItem: problem removing dc for item " + item);
        }
        if (Log.loggingDebug)
            log.debug("MarsInventoryPlugin.unequipItem: removed DC for item:" + item);
        return true;
    }

    /**
     * tell the world manager that we have added a submesh to this mob's display
     * context
     */
    protected boolean addDC(Long mobOid, MarsItem item) {
        // get the players base dc
        // we need this so we can decide which dc the item should use
        DisplayContext baseDC = getBaseDC(mobOid);
        if (baseDC == null) {
            Log.warn("addDC: could not get base dc for mob " + mobOid);
            return false;
        }

        // get the correct dc to use for this item (this is a copy)
        DisplayContext itemDC = item.getDCMapping(baseDC);
        if (itemDC == null) {
            Log.warn("addDC: item has no DC Mapping");
            return false;
        }
        itemDC.setObjRef(item.getOid());

        byte action;
        if (itemDC.getAttachableFlag()) {
            MarsAttachSocket socket = itemDC.getAttachInfo(DisplayState.NON_COMBAT,
                    item.getPrimarySlot());
            if (socket == null) {
                Log.error("MarsInventoryPlugin.addDC: attach failed: mobOid=" + mobOid +
                            ",item=" + item);
                return false;
            }
            if (Log.loggingDebug)
                log.debug("MarsInventoryPlugin.addDC: attaching socket child node, mobOid=" + mobOid + ", item=" + item);
            action = WorldManagerClient.modifyDisplayContextActionAddChild;
            WorldManagerClient.ModifyDisplayContextMessage msg = 
                new WorldManagerClient.ModifyDisplayContextMessage(
                    mobOid, action, null, null, socket.getName(), itemDC);
            Engine.getAgent().sendBroadcast(msg);
        }
        else {
            if (Log.loggingDebug)
                log.debug("MarsInventoryPlugin.addDC: adding submeshes (not socket), mobOid=" + mobOid + ", item=" + item);
            action = WorldManagerClient.modifyDisplayContextActionAdd;
            WorldManagerClient.ModifyDisplayContextMessage msg = new WorldManagerClient.ModifyDisplayContextMessage(
                    mobOid, action, itemDC.getSubmeshes());
            Engine.getAgent().sendBroadcast(msg);
        }
        return true;
    }

    /**
     * tell the world manager that we have added a submesh to this mob's display
     * context
     */
    protected boolean removeDC(Long mobOid, MarsItem item) {
        // get the players base dc
        // we need this so we can decide which dc the item should use
        DisplayContext baseDC = getBaseDC(mobOid);
        if (baseDC == null) {
            Log.warn("removeDC: could not get base dc for mob " + mobOid);
            return false;
        }

        // get the correct dc to use for this item
        DisplayContext itemDC = item.getDCMapping(baseDC);
        if (itemDC == null) {
            Log.warn("removeDC: item has no DC Mapping");
            return false;
        }
        itemDC.setObjRef(item.getOid());
        
        byte action;
        if (itemDC.getAttachableFlag()) {
            MarsAttachSocket socket = itemDC.getAttachInfo(DisplayState.NON_COMBAT,
                    item.getPrimarySlot());
            if (socket == null) {
                Log.error("MarsInventoryPlugin.removeDC: attach failed: mobOid=" + mobOid +
                            ",item=" + item);
                return false;
            }
            if (Log.loggingDebug)
                log.debug("MarsInventoryPlugin.removeDC: removing socket child node, mobOid=" + mobOid + ", item=" + item);
            action = WorldManagerClient.modifyDisplayContextActionRemoveChild;
            WorldManagerClient.ModifyDisplayContextMessage msg = 
                new WorldManagerClient.ModifyDisplayContextMessage(
                    mobOid, action, null, null, socket.getName(), itemDC);
            Engine.getAgent().sendBroadcast(msg);
        }
        else {
            WorldManagerClient.ModifyDisplayContextMessage msg = new WorldManagerClient.ModifyDisplayContextMessage(
                    mobOid,
                    WorldManagerClient.modifyDisplayContextActionRemove,
                    itemDC.getSubmeshes());
            Engine.getAgent().sendBroadcast(msg);
            log.debug("MarsInventoryPlugin.removeDC: sent modifydc msg");
        }
        return true;
    }

    protected DisplayContext getBaseDC(Long mobOid) {
        lock.lock();
        try {
            DisplayContext baseDC = baseDCMap.get(mobOid);
            if (baseDC == null) {
                // we need to retrieve the mob's DC
                if (Log.loggingDebug)
                    log.debug("MarsInventoryPlugin.getBaseDC: getting base dc for mobOid "
			      + mobOid);
                baseDC = (DisplayContext) EnginePlugin.getObjectProperty(mobOid, Namespace.WORLD_MANAGER, MarsObject.baseDCKey);
                if (Log.loggingDebug)
                    log.debug("MarsInventoryPlugin.getBaseDC: got base dc for mobOid "
			      + mobOid);
                if (baseDC == null) {
                    Log.warn("could not get base dc for mob " + mobOid);
                    return null;
                }
                baseDCMap.put(mobOid, baseDC);
            }
            return baseDC;
        } finally {
            lock.unlock();
        }
    }

    public static class EquipMap implements Serializable {

        public EquipMap() {
        }
        
        /**
         * returns the slot for item, can return null
         */
        public MarsEquipSlot getSlot(Long itemOid) {
            for (Map.Entry<MarsEquipSlot, Long> entry : map.entrySet()) {
                Long oItemOid = entry.getValue();
                if (oItemOid.equals(itemOid)) {
                    if (Log.loggingDebug)
                        log.debug("EquipMap.getSlot: found item=" + itemOid + " slot=" + entry.getKey());
                    return entry.getKey();
                }
            }
            if (Log.loggingDebug)
                log.debug("EquipMap.getSlot: item=" + itemOid + " slot=null");
            return null;
        }
        
        public Long get(MarsEquipSlot slot) {
            return map.get(slot);
        }
        
        public void put(MarsEquipSlot slot, Long longVal) {
            map.put(slot, longVal);
        }
        
        public void remove(MarsEquipSlot slot) {
            map.remove(slot);
        }

        public boolean containsValue(Long itemOid) {
            return map.containsValue(itemOid);
        }

        public HashMap<MarsEquipSlot, Long> getEquipMap() {
            return map;
        }

        public void setEquipMap(HashMap<MarsEquipSlot, Long> map) {
            this.map = map;
        }
        
        HashMap<MarsEquipSlot, Long> map = new HashMap<MarsEquipSlot, Long>();
        private static final long serialVersionUID = 1L;
    }

    public EquipMap getEquipMap(Long mobOid) {
        lock.lock();
        try {
	    Bag subObj = getBag(mobOid);
	    EquipMap map = (EquipMap) subObj.getProperty(EQUIP_MAP_PROP);
            if (map == null) {
                map = new EquipMap();
		subObj.setProperty(EQUIP_MAP_PROP, map);
		Engine.getPersistenceManager().setDirty(subObj);
            }
            return map;
        } finally {
            lock.unlock();
        }
    }

    public void setDirty(Long mobOid) {
	Bag subObj = getBag(mobOid);
	Engine.getPersistenceManager().setDirty(subObj);
    }

    // map (cache) of players base dc
    Map<Long, DisplayContext> baseDCMap = new HashMap<Long, DisplayContext>();
    
    public static final String EQUIP_MAP_PROP = "equipMap";

    static final Logger log = new Logger("MarsInventoryPlugin");
}
