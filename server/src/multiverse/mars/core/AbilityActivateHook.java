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

import multiverse.server.util.*;
import multiverse.mars.objects.*;
import multiverse.mars.plugins.*;

/**
 * an activate hook for items that trigger abilities
 * when the item is activated, the mob uses the ability
 */
public class AbilityActivateHook implements ActivateHook {
    public AbilityActivateHook() {
	super();
    }

    public AbilityActivateHook(MarsAbility ability) {
	super();
        setAbilityName(ability.getName());
    }

    public AbilityActivateHook(String abilityName) {
	super();
        setAbilityName(abilityName);
    }

    public void setAbilityName(String abilityName) {
        if (abilityName == null) {
            throw new RuntimeException("AbilityActivateHook.setAbility: bad ability");
        }
	this.abilityName = abilityName;
    }
    public String getAbilityName() {
	return abilityName;
    }
    public String abilityName = null;

    public MarsAbility getAbility() {
	if (abilityName == null)
	    return null;
	return Mars.AbilityManager.get(abilityName);
    }

    public boolean activate(Long activatorOid, MarsItem item, Long targetOid) {
	if (Log.loggingDebug)
            Log.debug("AbilityActivateHook.activate: activator=" + activatorOid + " item=" + item + " ability=" + abilityName + " target=" + targetOid);
        CombatClient.startAbility(abilityName, activatorOid, targetOid, item.getOid());
	return true;
    }

    private static final long serialVersionUID = 1L;
}
