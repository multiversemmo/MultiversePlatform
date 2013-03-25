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
import multiverse.mars.plugins.CombatPlugin;

public class TeachAbilityEffect extends MarsEffect {
    public TeachAbilityEffect(String name) {
	super(name);
	isPeriodic(false);
	isPersistent(false);
    }

    public TeachAbilityEffect(String name, String abilityName) {
	super(name);
	isPeriodic(false);
	isPersistent(false);
	setAbilityName(abilityName);
    }

    public String getAbilityName() { return abilityName; }
    public void setAbilityName(String name) { abilityName = name; }
    protected String abilityName = null;

    public String getCategory() { return category; }
    public void setCategory(String name) { category = name; }
    protected String category = null;

    // add the effect to the object
    public void apply(EffectState state) {
	super.apply(state);
	CombatInfo mob = state.getObject();
	MarsAbility ability = Mars.AbilityManager.get(abilityName);
	mob.addAbility(ability.getName());
	CombatPlugin.sendAbilityUpdate(mob);
    }
}
