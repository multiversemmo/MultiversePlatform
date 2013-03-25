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

package multiverse.mars.objects;

import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.server.plugins.*;
import multiverse.mars.core.*;
import java.util.*;

public class MarsItem extends MarsObject {
    public MarsItem() {
        setType(ObjectTypes.item);
    }
    
    public MarsItem(Long oid) {
        super(oid);
        setType(ObjectTypes.item);
    }

    public static MarsItem convert(MVObject obj) {
	if (! (obj instanceof MarsItem)) {
	    throw new MVRuntimeException("MarsItem.convert: obj is not a marsitem: " + obj);
	}
	return (MarsItem) obj;
    }


    public MarsPermissionCallback getMarsPermissionCallback() {
	return (MarsPermissionCallback) permissionCallback();
    }

   /**
     * adds an equip mapping for this item.
     * a mob can only equip this item if the slot if registered
     */
    public void addEquipSlot(MarsEquipSlot equipSlot) {
	lock.lock();
	try {
	    List<MarsEquipSlot> slots = getEquipSlots();
	    if (slots == null) {
		slots = new ArrayList<MarsEquipSlot>();
		setEquipSlots(slots);
	    }
	    slots.add(equipSlot);
	}
	finally {
	    lock.unlock();
	}
    }
    public void setEquipSlots(List<MarsEquipSlot> equipSlots) {
	MarsEquipInfo equipInfo = new MarsEquipInfo();
	for (MarsEquipSlot slot : equipSlots) {
	    equipInfo.addEquipSlot(slot);
	}
 	setProperty(InventoryClient.TEMPL_EQUIP_INFO, equipInfo);
    }
    public List<MarsEquipSlot> getEquipSlots() {
	MarsEquipInfo equipInfo = (MarsEquipInfo)getProperty(InventoryClient.TEMPL_EQUIP_INFO);
	if (equipInfo == null)
	    return null;
 	return equipInfo.getEquippableSlots();
    }

    // FIXME - need to use a list
    public MarsEquipSlot getPrimarySlot() {
        lock.lock();
        try {
	    List<MarsEquipSlot> slots = getEquipSlots();
	    if ((slots == null) || slots.isEmpty()) {
		return null;
	    }
	    return slots.get(0);
        }
        finally {
            lock.unlock();
        }
    }

    public boolean equipSlotExists(MarsEquipSlot equipSlot) {
	lock.lock();
	try {
	    List<MarsEquipSlot> slots = getEquipSlots();
	    if ((slots == null) || slots.isEmpty()) {
		return false;
	    }
	    return slots.contains(equipSlot);
	}
	finally {
	    lock.unlock();
	}
    }

    public void setSkillType(String skill) {
        this.skillType = skill;
    }
    public String getSkillType() {
        return skillType;
    }
    String skillType = null;

    public void setIcon(String icon) {
	setProperty(InventoryClient.TEMPL_ICON, icon);
    }
    public String getIcon() {
	String icon = (String) getProperty(InventoryClient.TEMPL_ICON);
        if (icon == null) {
            return "UNKNOWN_ICON";
        }
        return icon;
    }
    String icon = null;

    /**
     * register's the method to call when this item gets activated by 
     * the user
     */
    public void setActivateHook(ActivateHook hook) {
	setProperty(InventoryClient.TEMPL_ACTIVATE_HOOK, hook);
    }
    public ActivateHook getActivateHook() {
        return (ActivateHook) getProperty(InventoryClient.TEMPL_ACTIVATE_HOOK);
    }
    public boolean activate(Long activatorOid, Long targetOid) {
        if (Log.loggingDebug)
            log.debug("MarsItem.activate: activator=" + activatorOid + " item=" + this + " target=" + targetOid);
	ActivateHook activateHook = (ActivateHook) getProperty(InventoryClient.TEMPL_ACTIVATE_HOOK);
        if (activateHook == null) {
	    log.warn("activate: activateHook is null");
            return false;
        }
        return activateHook.activate(activatorOid, this, targetOid);
    }

    protected static String EQUIP_INFO_PROP = "equipInfo";

    private static final long serialVersionUID = 1L;
}
