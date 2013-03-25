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

public class HealEffect extends MarsEffect {
    static Random random = new Random();

    public HealEffect(String name) {
        super(name);
    }

    // add the effect to the object
    public void apply(EffectState state) {
	super.apply(state);
        int heal = minHeal;

        if (maxHeal > minHeal) {
            heal += random.nextInt(maxHeal - minHeal);
        }

        CombatInfo obj = state.getObject();
	if (heal == 0) {
	    return;
	}
        obj.statModifyBaseValue(getHealProperty(), heal);
        obj.sendStatusUpdate();
    }

    // perform the next periodic pulse for this effect on the object
    public void pulse(EffectState state) {
	super.pulse(state);
        int heal = minPulseHeal;

        if (maxPulseHeal > minPulseHeal) {
            heal += random.nextInt(maxPulseHeal - minPulseHeal);
        }

	if (heal == 0) {
	    return;
	}
        CombatInfo obj = state.getObject();
        obj.statModifyBaseValue(getHealProperty(), heal);
        obj.sendStatusUpdate();
    }

    public int getMinInstantHeal() { return minHeal; }
    public void setMinInstantHeal(int hps) { minHeal = hps; }
    protected int minHeal = 0;

    public int getMaxInstantHeal() { return maxHeal; }
    public void setMaxInstantHeal(int hps) { maxHeal = hps; }
    protected int maxHeal = 0;

    public int getMinPulseHeal() { return minPulseHeal; }
    public void setMinPulseHeal(int hps) { minPulseHeal = hps; }
    protected int minPulseHeal = 0;

    public int getMaxPulseHeal() { return maxPulseHeal; }
    public void setMaxPulseHeal(int hps) { maxPulseHeal = hps; }
    protected int maxPulseHeal = 0;

    public String getHealProperty() { return healProperty; }
    public void setHealProperty(String property) { healProperty = property; }
    protected String healProperty = CombatInfo.COMBAT_PROP_HEALTH;
}
