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

package multiverse.mars.abilities;

import multiverse.server.util.*;
import multiverse.mars.core.*;
import multiverse.mars.plugins.CombatPlugin;
import java.util.*;

public class CombatAbility extends MarsAbility {
    public CombatAbility(String name) {
        super(name);
    }

    public Map resolveHit(State state) {
	return new HashMap();
    }

    public MarsEffect getActivationEffect() { return activationEffect; }
    public void setActivationEffect(MarsEffect effect) { this.activationEffect = effect; }
    protected MarsEffect activationEffect = null;

    public void completeActivation(State state) {
        super.completeActivation(state);

        //Add attacker to target's list of attackers
        CombatPlugin.addAttacker(state.getTarget().getOid(), state.getObject().getOid());
        state.getObject().setCombatState(true);        
        
	Map params = resolveHit(state);
	Log.debug("CombatAbility.completeActivation: params=" + params);
        MarsEffect.applyEffect(activationEffect, state.getObject(), state.getTarget(), params);
    }
}
