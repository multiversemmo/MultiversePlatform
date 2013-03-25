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

package multiverse.mars.core;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.plugins.*;
import multiverse.mars.plugins.*;
import multiverse.mars.objects.*;

/**
 * an activate hook attached to equippable items, eg: weapons, armor
 * hook will unequip the item in the current slot and equip the item
 * associated with the hook
 */
public class EquipActivateHook implements ActivateHook {
    public EquipActivateHook() {
        super();
    }

    /**
     * returns whether the item was successfully activated
     */
    public boolean activate(Long activatorOid, MarsItem item, Long targetOid) {
	// get the inventoryplugin
	MarsInventoryPlugin invPlugin = (MarsInventoryPlugin)Engine.getPlugin(InventoryPlugin.INVENTORY_PLUGIN_NAME);
	if (Log.loggingDebug)
	    Log.debug("EquipActivateHook: calling invPlugin, item=" + item +
	            ", activatorOid=" + activatorOid + ", targetOid=" + targetOid);
	
	// is this item already equipped
	MarsInventoryPlugin.EquipMap equipMap = 
	    invPlugin.getEquipMap(activatorOid);
	MarsEquipSlot slot;
	invPlugin.getLock().lock();
	try {
	    slot = equipMap.getSlot(item.getMasterOid());
	}
	finally {
	    invPlugin.getLock().unlock();
	}
	if (slot == null) {
	    // its not equipped
	    if (Log.loggingDebug)
	        Log.debug("EquipActivateHook: item not equipped: " + item);
	    return invPlugin.equipItem(item, activatorOid, true);
	}
	else {
	    // it is equipped, unequip it
	    if (Log.loggingDebug)
	        Log.debug("EquipActivateHook: item IS equipped: " + item);
	    return invPlugin.unequipItem(item, activatorOid);
	}
    }

    // use oids since cheaper to serialize
    protected long itemOid = -1;
    private static final long serialVersionUID = 1L;
}
