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

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.mars.objects.*;
import multiverse.mars.core.*;
import multiverse.mars.plugins.*;
import java.util.*;

public class DamageEffect extends MarsEffect {
    static Random random = new Random();

    public DamageEffect(String name) {
        super(name);
    }

    // add the effect to the object
    public void apply(EffectState state) {
	super.apply(state);
        int dmg = minDmg;

        if (Log.loggingDebug)
            Log.debug("DamageEffect.apply: doing instant damage to obj=" + state.getObject() +
		      " from=" + state.getCaster());
        if (maxDmg > minDmg) {
            dmg += random.nextInt(maxDmg - minDmg);
        }

        CombatInfo obj = state.getObject();
        obj.statModifyBaseValue(getDamageProperty(), -dmg);
        obj.sendStatusUpdate();
	Engine.getAgent().sendBroadcast(new CombatClient.DamageMessage(obj.getOwnerOid(),
							  state.getCaster().getOwnerOid(), dmg, damageType));
    }

    // perform the next periodic pulse for this effect on the object
    public void pulse(EffectState state) {
	super.pulse(state);
        int dmg = minPulseDmg;

        if (maxPulseDmg > minPulseDmg) {
            dmg += random.nextInt(maxPulseDmg - minPulseDmg);
        }

        CombatInfo obj = state.getObject();
        obj.statModifyBaseValue(getDamageProperty(), -dmg);
        obj.sendStatusUpdate();
	Engine.getAgent().sendBroadcast(new CombatClient.DamageMessage(obj.getOwnerOid(),
							  state.getCaster().getOwnerOid(), dmg, damageType));
    }

    public int getMinInstantDamage() { return minDmg; }
    public void setMinInstantDamage(int hps) { minDmg = hps; }
    protected int minDmg = 0;

    public int getMaxInstantDamage() { return maxDmg; }
    public void setMaxInstantDamage(int hps) { maxDmg = hps; }
    protected int maxDmg = 0;

    public int getMinPulseDamage() { return minPulseDmg; }
    public void setMinPulseDamage(int hps) { minPulseDmg = hps; }
    protected int minPulseDmg = 0;

    public int getMaxPulseDamage() { return maxPulseDmg; }
    public void setMaxPulseDamage(int hps) { maxPulseDmg = hps; }
    protected int maxPulseDmg = 0;

    public String getDamageProperty() { return damageProperty; }
    public void setDamageProperty(String property) { damageProperty = property; }
    protected String damageProperty = CombatInfo.COMBAT_PROP_HEALTH;

    public String getDamageType() { return damageType; }
    public void setDamageType(String damageType) { this.damageType = damageType; }
    protected String damageType = "";
}
