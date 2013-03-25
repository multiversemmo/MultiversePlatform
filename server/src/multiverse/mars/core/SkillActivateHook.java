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

/**
 * an activate hook attached to skill scrolls.
 * when the scroll is activated, the mob gets the skill added to his list
 */
public class SkillActivateHook implements ActivateHook {
    public SkillActivateHook() {
    }

    /**
     * what is the skill you get for this
     */
    public SkillActivateHook(MarsSkill skill) {
        setSkill(skill);
    }

    public void setSkill(MarsSkill skill) {
        this.skill = skill;
    }
    public MarsSkill getSkill() {
        return skill;
    }

    /**
     * returns whether the item was successfully activated
     */
    public boolean activate(Long activatorOid, MarsItem item, Long targetOid) {
        if (Log.loggingDebug)
            Log.debug("SkillActivateHook.activate: activator=" + activatorOid +
                      ", skill=" + getSkill().getName());
//         player.addSkill(getSkill());
//         player.sendServerInfo("You have learned the skill " + 
//                               getSkill().getName());

        // destroy the item
//         MarsItem item = player.findItem(item.getTemplate());
//         if (item == null) {
//             throw new MVRuntimeException("SkillActivateHook.activate: could not find the item with matching template");
//         }
//         if (! player.destroyItem(item)) {
//             throw new MVRuntimeException("SkillActivateHook.activate: destroyItem failed");
//         }
        return true;
    }

    protected MarsSkill skill = null;

    private static final long serialVersionUID = 1L;
}
