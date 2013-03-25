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
import java.io.*;

import multiverse.msgsys.*;
import multiverse.server.network.*;
import multiverse.server.messages.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;

/**
 * client for sending/getting messages to the InventoryPlugin
 */
public class InventoryClient {

    // /**
    // * returns the display context for the given object
    // * if it is a player, it will get the base display context
    // * from the worldmanager and apply attachments to the dc
    // */
    // public static DisplayContext getDisplayContext(Long objOid) {
    // DisplayContextReqMessage msg = new DisplayContextReqMessage(objOid);
    // return (DisplayContext)Engine.getAgent().sendRPCReturnObject(msg);
    // }

    /**
     * Requests a full inventory update from the InventoryPlugin
     */
    public static void getInventory(Long objOid) {
        return;
    }
    
    public static Long createInventory(Long oid) {
        Message msg = new SubjectMessage(InventoryClient.MSG_TYPE_CREATE_INV, oid);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }
    
    public static void activateObject(Long objOid,
				      Long activatorOid, Long targetOid) {
        ActivateMessage msg = new ActivateMessage(objOid, activatorOid, targetOid);
        if (Log.loggingDebug)
            Log.debug("InventoryClient.activateObject: activator=" + msg.getActivatorOid() + ", objOid=" +
		      msg.getSubject() + ", targetOid=" + msg.getTargetOid());
        Engine.getAgent().sendBroadcast(msg);
    }

    public static boolean lootAll(Long looterOid, Long containerOid) {
        LootAllMessage msg = new LootAllMessage(looterOid, containerOid);
        if (Log.loggingDebug)
            Log.debug("InventoryClient.lootAll: looterOid=" + looterOid + ", container=" + containerOid);
        return Engine.getAgent().sendRPCReturnBoolean(msg);
    }
    
    public static boolean addItem(Long containerOid,
            Long mobOid, Long rootContainerOid, Long itemOid) {
        AddItemMessage msg = new AddItemMessage(containerOid, mobOid,
                rootContainerOid, itemOid);
        return Engine.getAgent().sendRPCReturnBoolean(msg);
    }

    public static Long removeItem(Long mobOid, Long itemOid) {
	Message msg = new RemoveOrFindItemMessage(MSG_TYPE_INV_REMOVE, mobOid, INV_METHOD_OID, itemOid);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }

    public static Long removeItem(Long mobOid, String templateName) {
	Message msg = new RemoveOrFindItemMessage(MSG_TYPE_INV_REMOVE, mobOid, INV_METHOD_TEMPLATE, templateName);
        return Engine.getAgent().sendRPCReturnLong(msg);
    }

    public static List<Long> removeItems(Long mobOid, ArrayList<String> templateNames) {
	Message msg = new RemoveOrFindItemMessage(MSG_TYPE_INV_REMOVE, mobOid, INV_METHOD_TEMPLATE_LIST, templateNames);
        return (List<Long>)Engine.getAgent().sendRPCReturnObject(msg);
    }

    public static Long findItem(Long mobOid, String templateName) {
	Message msg = new RemoveOrFindItemMessage(MSG_TYPE_INV_FIND, mobOid, INV_METHOD_TEMPLATE, templateName);
	Long oid = Engine.getAgent().sendRPCReturnLong(msg);
	Log.debug("findItem: got response");
	return oid;
    }

    public static List<Long> findItems(Long mobOid, ArrayList<String> templateNames) {
	Message msg = new RemoveOrFindItemMessage(MSG_TYPE_INV_FIND, mobOid, INV_METHOD_TEMPLATE_LIST, templateNames);
	return (List<Long>)Engine.getAgent().sendRPCReturnObject(msg);
    }

    public static class AddItemMessage extends SubjectMessage {

        public AddItemMessage() {
            super(MSG_TYPE_ADD_ITEM);
        }
        
        public AddItemMessage(Long containerOid, Long mob, // whose inventory
                                                            // are we modifying
                Long rootContainer, Long itemOid) {
            super(MSG_TYPE_ADD_ITEM, containerOid);
            setContainer(containerOid);
            setMob(mob);
            setRootContainer(rootContainer);
            setItem(itemOid);
        }

        public void setContainer(Long oid) {
            this.container = oid;
        }

        public Long getContainer() {
            return container;
        }

        public void setRootContainer(Long oid) {
            this.rootContainer = oid;
        }

        public Long getRootContainer() {
            return rootContainer;
        }

        public void setItem(Long oid) {
            this.item = oid;
        }

        public Long getItem() {
            return item;
        }

        public void setMob(Long oid) {
            this.mob = oid;
        }

        public Long getMob() {
            return this.mob;
        }

        Long container;

        Long rootContainer;

        Long item;

        Long mob;

        private static final long serialVersionUID = 1L;
    }

    public static class RemoveOrFindItemMessage extends SubjectMessage {

        public RemoveOrFindItemMessage() {
            super();
        }
        
        public RemoveOrFindItemMessage(MessageType msgType, Long mobOid, String method, Serializable payload) {
            super(msgType, mobOid);
            setMethod(method);
            setPayload(payload);
        }

        public String getMethod() {
            return method;
        }

        public void setMethod(String method) {
            this.method = method;
        }

        public Object getPayload() {
            return payload;
        }

        public void setPayload(Object payload) {
            this.payload = payload;
        }

        private String method;
        private Object payload;
        
        private static final long serialVersionUID = 1L;

    }

    public static class InvUpdateMessage extends SubjectMessage implements ClientMessage {

        public InvUpdateMessage() {
            super(MSG_TYPE_INV_UPDATE);
        }
        
        public InvUpdateMessage(Long mobOid) {
            super(MSG_TYPE_INV_UPDATE, mobOid);
        }
        
        public void addItem(int bagNum, int bagPos, Long itemOid, String itemName, String itemIcon) {
            InvPos invPos = new InvPos(bagNum, bagPos);
            invMap.put(invPos, new ItemInfo(itemOid, itemName, itemIcon));
        }
        
        /**
         * returns the number of items in the inventory update message
         */
        public int getNumEntries() {
            return invMap.size();
        }
        
        public Map<InvPos, ItemInfo> getEntries() {
            return new HashMap<InvPos, ItemInfo>(invMap);
        }
        
        public static class InvPos implements Serializable {

            public InvPos() {
            }

            public InvPos(int bagNum, int bagPos) {
                this.bagNum = bagNum;
                this.bagPos = bagPos;
            }
            
            public Integer bagNum;
            public Integer bagPos;
            
            public String toString() {
                return "[InvPos bagNum=" + bagNum + ", bagPos=" + bagPos + "]";
            }
            
            public boolean equals(Object other) {
                InvPos otherI = (InvPos) other;
                if ((bagNum == null) || (bagPos == null)) {
                    return false;
                }
                return ( (this.bagNum.equals(otherI.bagNum)) && (this.bagPos.equals(otherI.bagPos)) );
            }
            
            public int hashCode() {
                return (bagNum.hashCode() ^ bagPos.hashCode());
            }

            private static final long serialVersionUID = 1L;
        }

        // map of bagNum -> (map of position within bag -> item information)
        Map<InvPos, ItemInfo> invMap = new HashMap<InvPos, ItemInfo>();

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(400);
            buf.putLong(getSubject());
            buf.putInt(43);
            buf.putInt(this.getNumEntries());
            
            for (InvPos invPos : invMap.keySet()) {
                ItemInfo itemInfo = invMap.get(invPos);
                buf.putLong(itemInfo.itemOid);
                buf.putInt(invPos.bagNum);
                buf.putInt(invPos.bagPos);
                buf.putString(itemInfo.itemName);
                buf.putString(itemInfo.itemIcon);
            }
            buf.flip();
            return buf;
        }

        private static final long serialVersionUID = 1L;
    }

    public static class ItemInfo implements Serializable {

	public ItemInfo() {
        }
        
	public ItemInfo(Long itemOid, String itemName, String itemIcon) {
	    this.itemOid = itemOid;
	    this.itemName = itemName;
	    this.itemIcon = itemIcon;
	}
            
	public String toString() {
	    return "[ItemInfo: itemOid=" + itemOid + ",itemName=" + itemName + ",itemIcon=" + itemIcon + "]";
	}

	public Long itemOid;
	public String itemName;
	public String itemIcon;

	private static final long serialVersionUID = 1L;
    }
        
    /**
     * the obj is the "main" oid for the message since that is what the
     * InventoryPlugin is subscribing on
     */
    public static class ActivateMessage extends SubjectMessage {

        public ActivateMessage() {
            super(MSG_TYPE_ACTIVATE);
        }

        public ActivateMessage(Long objOid, Long activatorOid, Long targetOid) {
            super(MSG_TYPE_ACTIVATE, objOid);
            setActivatorOid(activatorOid);
	    setTargetOid(targetOid);
        }

        public void setActivatorOid(Long oid) { this.activatorOid = oid; }
        public Long getActivatorOid() { return activatorOid; }
        protected Long activatorOid;

	public void setTargetOid(Long oid) { targetOid = oid; }
	public Long getTargetOid() { return targetOid; }
	protected Long targetOid;
        private static final long serialVersionUID = 1L;
    }

    /**
     * message is asking the inventory plugin to execute looting by looter of
     * the passed in container
     */
    public static class LootAllMessage extends SubjectMessage {
        public LootAllMessage() {
            super(MSG_TYPE_LOOTALL);
        }
        public LootAllMessage(Long looterOid, Long containerOid) {
            super(MSG_TYPE_LOOTALL, looterOid);
            setContainerOid(containerOid);
        }
        public void setContainerOid(Long containerOid) {
            this.containerOid = containerOid;
        }
        public Long getContainerOid() {
            return this.containerOid;
        }
        Long containerOid;
        private static final long serialVersionUID = 1L;
    }
    
    // public static class DisplayContextReqMessage
    // extends SubjectMessage {
    // public DisplayContextReqMessage(Long objOid) {
    // super(objOid);
    // setProperty(MessageServer.MSG_TYPE,
    // MSG_TYPE_DC_REQ);
    // }
    // }

    // msg "type" field for binding an object request
    public static final MessageType MSG_TYPE_ADD_ITEM = MessageType.intern("mv.ADD_ITEM");
    public static final MessageType MSG_TYPE_CREATE_INV = MessageType.intern("mv.CREATE_INV");
    public static final MessageType MSG_TYPE_INV_UPDATE = MessageType.intern("mv.INV_UPDATE");
    public static final MessageType MSG_TYPE_ACTIVATE = MessageType.intern("mv.ACTIVATE");
    public static final MessageType MSG_TYPE_LOOTALL = MessageType.intern("mv.LOOTALL");
    public static final MessageType MSG_TYPE_INV_FIND = MessageType.intern("mv.INV_FIND");
    public static final MessageType MSG_TYPE_INV_REMOVE = MessageType.intern("mv.INV_REMOVE");
    public static final MessageType MSG_TYPE_DESTROY_ITEM = MessageType.intern("mv.DESTROY_ITEM");
    
    public static final String INV_METHOD_OID = "oid";
    public static final String INV_METHOD_TEMPLATE = "template";
    public static final String INV_METHOD_TEMPLATE_LIST = "templateList";

    // public static final String MSG_TYPE_DC_REQ = "worldMsg_dc_req";

    /**
     * template starting items
     */
    public final static String TEMPL_ITEMS = ":inv_items";

    // item template properties
    public final static String TEMPL_EQUIP_INFO = "item_equipInfo";
    public final static String TEMPL_ACTIVATE_HOOK = "item_activateHook";
    public final static String TEMPL_ICON = "item_icon";
    public final static String TEMPL_DCMAP = "item_dcmap";

    public static Namespace NAMESPACE = null;
    public static Namespace ITEM_NAMESPACE = null;
}
