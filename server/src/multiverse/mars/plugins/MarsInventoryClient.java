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
import java.io.Serializable;

import multiverse.msgsys.*;
import multiverse.server.util.*;
import multiverse.server.engine.*;
import multiverse.server.plugins.InventoryClient;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.mars.objects.*;


/**
 * MARS-specific calls for sending/getting messages to the MarsInventoryPlugin
 */
public class MarsInventoryClient {
    public static Long findItem(Long mobOid, MarsEquipSlot slot) {
	InventoryClient.RemoveOrFindItemMessage msg = 
            new InventoryClient.RemoveOrFindItemMessage(MSG_TYPE_MARS_INV_FIND, mobOid, INV_METHOD_SLOT, slot);
	Long oid = Engine.getAgent().sendRPCReturnLong(msg);
        Log.debug("findItem: got response");
        return oid;
    }

    public static void tradeStart(Long requesterOid, Long partnerOid) {
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("requesterOid", requesterOid);
        props.put("partnerOid", partnerOid);
        ExtensionMessage msg = new ExtensionMessage(MSG_TYPE_TRADE_START_REQ, requesterOid, props);
	Engine.getAgent().sendBroadcast(msg);
    }

    public static void tradeUpdate(Long requesterOid, Long partnerOid,
				   LinkedList<Long> offerItems, boolean accepted, boolean cancelled) {
	Log.debug("MarsInventoryClient.tradeUpdate: requesterOid=" + requesterOid + " partnerOid="
		  + partnerOid + " offer=" + offerItems + " accepted=" + accepted + " cancelled=" + cancelled);
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("requesterOid", requesterOid);
        props.put("partnerOid", partnerOid);
        props.put("offerItems", offerItems);
        props.put("accepted", accepted);
        props.put("cancelled", cancelled);
        ExtensionMessage msg = new ExtensionMessage(MSG_TYPE_TRADE_OFFER_REQ, requesterOid, props);
	Engine.getAgent().sendBroadcast(msg);
    }

    public static final MessageType MSG_TYPE_MARS_INV_FIND = MessageType.intern("mv.MARS_INV_FIND");

    public static final MessageType MSG_TYPE_TRADE_START_REQ = MessageType.intern("mv.TRADE_START_REQ");
    public static final MessageType MSG_TYPE_TRADE_START = MessageType.intern("mv.TRADE_START");
    public static final MessageType MSG_TYPE_TRADE_COMPLETE = MessageType.intern("mv.TRADE_COMPLETE");
    public static final MessageType MSG_TYPE_TRADE_OFFER_REQ = MessageType.intern("mv.TRADE_OFFER_REQ");
    public static final MessageType MSG_TYPE_TRADE_OFFER_UPDATE = MessageType.intern("mv.TRADE_OFFER_UPDATE");
    public static final MessageType MSG_TYPE_SWAP_ITEM = MessageType.intern("mv.SWAP_ITEM");

    public static final String INV_METHOD_SLOT = "slot";
    public static final String MSG_INV_SLOT = "inv_slot";

    public final static byte tradeSuccess = (byte)1;
    public final static byte tradeCancelled = (byte)2;
    public final static byte tradeFailed = (byte)3;
    public final static byte tradeBusy = (byte)4;
}
