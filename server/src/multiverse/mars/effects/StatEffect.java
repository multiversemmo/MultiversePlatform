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

package multiverse.mars.effects;

import multiverse.mars.objects.*;
import multiverse.mars.core.*;
import java.util.*;

public class StatEffect extends MarsEffect {
    public StatEffect(String name) {
	super(name);
	isPeriodic(false);
	isPersistent(true);
    }

    public void setStat(String stat, int adj) {
	statMap.put(stat, new Integer(adj));
    }
    public Integer getStat(String stat) {
	return statMap.get(stat);
    }
    protected Map<String, Integer> statMap = new HashMap<String, Integer>();

    // add the effect to the object
    public void apply(EffectState state) {
	super.apply(state);
	CombatInfo obj = state.getObject();
	for (Map.Entry<String, Integer> entry : statMap.entrySet()) {
	    obj.statAddModifier(entry.getKey(), state, entry.getValue());
	}
    }

    // remove the effect from the object
    public void remove(EffectState state) {
	CombatInfo obj = state.getObject();
	for (Map.Entry<String, Integer> entry : statMap.entrySet()) {
	    obj.statRemoveModifier(entry.getKey(), state);
	}
	super.remove(state);
    }

    // perform the next periodic pulse for this effect on the object
    public void pulse(EffectState state) {
	super.pulse(state);
    }
}
