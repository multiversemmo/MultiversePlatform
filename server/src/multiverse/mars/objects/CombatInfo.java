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
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.plugins.WorldManagerClient.TargetedPropertyMessage;
import multiverse.server.messages.*;
import multiverse.mars.core.*;
import multiverse.mars.core.MarsEffect.EffectState;
import multiverse.mars.plugins.*;
import java.beans.*;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.*;
import java.util.*;


/**
 * Information related to the combat system. Any object that wants to be involved
 * in combat needs one of these.
 */
public class CombatInfo extends Entity implements Runnable, Cooldown.CooldownObject {
    public CombatInfo() {
        super();
        setNamespace(Namespace.COMBAT);
    }

    public CombatInfo(Long objOid) {
        super(objOid);
        setNamespace(Namespace.COMBAT);
    }

    public String toString() {
        return "[Entity: " + getName() + ":" + getOid() + "]";
    }

    public ObjectType getType() {
        return ObjectTypes.combatInfo;
    }

    public void setAutoAttack(Long newTarget) {
        lock.lock();
        try {
	    boolean recovering = scheduled;
            Long oldTarget = target;
            if (oldTarget != null && oldTarget.equals(newTarget)) {
                return;
            }
            target = newTarget;
            if (oldTarget == null) {
                setCombatState(true);
		if (!scheduled) {
		    schedule(getAttackDelay());
		}
            }
            else {
                CombatPlugin.removeAttacker(oldTarget, getOwnerOid());
            }
            if (target == null) {
                setCombatState(false);
            }
            else {
                CombatPlugin.addAttacker(target, getOwnerOid());
		if (!recovering) {
		    CombatPlugin.resolveAutoAttack(this);
		}
            }
        }
        finally {
            lock.unlock();
        }
    }
    public void stopAutoAttack() {
        lock.lock();
        try {
            if (target != null) {
                CombatPlugin.removeAttacker(target, getOwnerOid());
            }
                setCombatState(false);
            target = null;
        }
        finally {
            lock.unlock();
        }
    }
    public Long getAutoAttackTarget() {
        lock.lock();
        try {
            return target;
        }
        finally {
            lock.unlock();
        }
    }
    protected Long target = null;
    boolean scheduled = false;

    public long getAttackDelay() {
        return 3000;
    }

    protected void schedule(long delay) {
        if (Log.loggingDebug)
            Log.debug("CombatInfo.schedule: scheduling obj=" + this + " for delay=" + delay);
        Engine.getExecutor().schedule(this, delay, TimeUnit.MILLISECONDS);
	scheduled = true;
    }
    protected void cancel() {
        Engine.getExecutor().remove(this);
	scheduled = false;
    }

    public void run() {
        Lock targetLock = null;
        lock.lock();
        try {
            if (target == null) {
                scheduled = false;
            }
            else {
                targetLock = EntityManager.getEntityByNamespace(target, Namespace.COMBAT).getLock();
                if (targetLock != null) {
                    while (!targetLock.tryLock()) {
                        lock.unlock();
                        Thread.yield();
                        lock.lock();
                    }
                }
                CombatPlugin.resolveAutoAttack(this);
                schedule(getAttackDelay());
            }
        }
        catch (Exception e) {
            Log.exception("CombatInfo.run: got exception", e);
        }
        finally {
            if (targetLock != null) {
                targetLock.unlock();
            }
            lock.unlock();
        }
    }

    public void addCooldownState(Cooldown.State state) { cooldownMap.put(state.getID(), state); }
    public void removeCooldownState(Cooldown.State state) { cooldownMap.remove(state.getID()); }
    public Cooldown.State getCooldownState(String id) { return cooldownMap.get(id); }
    protected Map<String, Cooldown.State> cooldownMap = new HashMap<String, Cooldown.State>();

    public void setCurrentAction(MarsAbility.State action) { currentAction = action; }
    public MarsAbility.State getCurrentAction() { return currentAction; }
    protected transient MarsAbility.State currentAction;


    public void addActiveAbility(MarsAbility.State abilityState) {
        activeAbilities.add(abilityState);
    }
    public void removeActiveAbility(MarsAbility.State abilityState) {
        activeAbilities.remove(abilityState);
    }
    protected transient Set<MarsAbility.State> activeAbilities;

    public void addAbility(String abilityName) {
        if (Log.loggingDebug)
            Log.debug("CombatInfo.addAbility: adding ability=" + abilityName + " to obj=" + this);
        
        if (abilities.contains(abilityName))
            return;
        
	abilities.add(abilityName);
	Engine.getPersistenceManager().setDirty(this);
    }
    public void removeAbility(String abilityName) {
        if (Log.loggingDebug)
            Log.debug("CombatInfo.addAbility: removing ability=" + abilityName + " from obj=" + this);
	abilities.remove(abilityName);
	Engine.getPersistenceManager().setDirty(this);
    }
    public ArrayList<String> getAbilities() { return new ArrayList<String>(abilities); }
    public void setAbilities(ArrayList<String> abilities) {
	this.abilities = new ArrayList<String>(abilities);
    }
    protected ArrayList<String> abilities = new ArrayList<String>();

    public void addEffect(EffectState effectState) { effects.add(effectState); }
    public void removeEffect(EffectState effectState) { effects.remove(effectState); }
    public Set<EffectState> getEffects() { return new HashSet<EffectState>(effects); }
    public void setEffects(Set<EffectState> effects) { this.effects = new HashSet<EffectState>(effects); }
    protected Set<EffectState> effects = new HashSet<EffectState>();

    public boolean isUser() { return getBooleanProperty(COMBAT_PROP_USERFLAG); }
    public boolean isMob() { return getBooleanProperty(COMBAT_PROP_MOBFLAG); }
    public boolean attackable() { return getBooleanProperty(COMBAT_PROP_ATTACKABLE); }
    public boolean dead() { return getBooleanProperty(COMBAT_PROP_DEADSTATE); }
    public Long getOwnerOid() { return getOid(); }

    public void setCombatState(boolean state) {
        setProperty(COMBAT_PROP_COMBATSTATE, new Boolean(state));
        PropertyMessage propMsg = new PropertyMessage(getOwnerOid());
        propMsg.setProperty(COMBAT_PROP_COMBATSTATE, new Boolean(state));
        Engine.getAgent().sendBroadcast(propMsg);
    }

    public void setDeadState(boolean state) {
        setProperty(COMBAT_PROP_DEADSTATE, new Boolean(state));
        PropertyMessage propMsg = new PropertyMessage(getOwnerOid());
        propMsg.setProperty(COMBAT_PROP_DEADSTATE, new Boolean(state));
        Engine.getPersistenceManager().setDirty(this);
        Engine.getAgent().sendBroadcast(propMsg);
    }

    public void sendStatusUpdate() {
    }

    public InterpolatedWorldNode getWorldNode() { return node; }
    public void setWorldNode(InterpolatedWorldNode node) { this.node = node; }
    InterpolatedWorldNode node;

    public void statModifyBaseValue(String statName, int delta) {
	lock.lock();
	try {
	    MarsStat stat = (MarsStat) getProperty(statName);
	    stat.modifyBaseValue(delta);
	    MarsStatDef statDef = CombatPlugin.lookupStatDef(statName);
	    statDef.update(stat, this);
	    statSendUpdate(false);
	}
	finally {
	    lock.unlock();
	}
    }

    public void statSetBaseValue(String statName, int value) {
	lock.lock();
	try {
	    MarsStat stat = (MarsStat) getProperty(statName);
	    stat.setBaseValue(value);
	    MarsStatDef statDef = CombatPlugin.lookupStatDef(statName);
	    statDef.update(stat, this);
	    statSendUpdate(false);
	}
	finally {
	    lock.unlock();
	}
    }

    public void statAddModifier(String statName, Object id, int delta) {
	lock.lock();
	try {
	    MarsStat stat = (MarsStat) getProperty(statName);
	    stat.addModifier(id, delta);
	    MarsStatDef statDef = CombatPlugin.lookupStatDef(statName);
	    statDef.update(stat, this);
	    statSendUpdate(false);
	}
	finally {
	    lock.unlock();
	}
    }

    public void statRemoveModifier(String statName, Object id) {
	lock.lock();
	try {
	    MarsStat stat = (MarsStat) getProperty(statName);
	    stat.removeModifier(id);
	    MarsStatDef statDef = CombatPlugin.lookupStatDef(statName);
	    statDef.update(stat, this);
	    statSendUpdate(false);
	}
	finally {
	    lock.unlock();
	}
    }

    public int statGetCurrentValue(String statName) {
	lock.lock();
	try {
	    MarsStat stat = (MarsStat) getProperty(statName);
	    return stat.getCurrentValue();
	}
	finally {
	    lock.unlock();
	}
    }

    public void statSendUpdate(boolean sendAll) {
        statSendUpdate(sendAll, null);
    }

    public void statSendUpdate(boolean sendAll, Long targetOid) {
	lock.lock();
	try {
            PropertyMessage propMsg = null;
            TargetedPropertyMessage targetPropMsg = null;
            if (targetOid == null)
                propMsg = new PropertyMessage(getOwnerOid());
            else
                targetPropMsg =
                    new TargetedPropertyMessage(targetOid,getOwnerOid());
            int count = 0;
	    for (Object value : getPropertyMap().values()) {
		if (value instanceof MarsStat) {
		    MarsStat stat = (MarsStat) value;
		    if (sendAll || stat.isDirty()) {
                        if (propMsg != null)
                            propMsg.setProperty(stat.getName(), stat.getCurrentValue());
                        else
                            targetPropMsg.setProperty(stat.getName(), stat.getCurrentValue());
                        if (! sendAll)
                            stat.setDirty(false);
                        count++;
		    }
		}
	    }
	    if (count > 0) {
		Engine.getPersistenceManager().setDirty(this);
                if (propMsg != null)
                    Engine.getAgent().sendBroadcast(propMsg);
                else
                    Engine.getAgent().sendBroadcast(targetPropMsg);
	    }
	}
	finally {
	    lock.unlock();
	}
    }

    protected ArrayList<String> skills = new ArrayList<String>();
    
    public ArrayList<String> getSkills() {
        return new ArrayList<String>(skills);
    }
    
    public void addSkill(String skillName) {
        if (Log.loggingDebug)
            Log.debug("CombatInfo.addSkill: adding skill=" + skillName
                    + " to obj=" + this);
        if (skills.contains(skillName)) {
            Log.debug("CombatInfo.addSkill: Player Already has skill + "
                    + skillName);
            return;
        }
        skills.add(skillName);
        String ability = Mars.SkillManager.get(skillName).getDefaultAbility();
        if (ability != null)
            this.addAbility(ability);

        Engine.getPersistenceManager().setDirty(this);
    }

    public void removeSkill(String skillName) {
        if (Log.loggingDebug)
            Log.debug("CombatInfo.removeSkill: removing skill=" + skillName
                    + " from obj=" + this);
        skills.remove(skillName);
        Engine.getPersistenceManager().setDirty(this);
    }

    public void setSkills(ArrayList<String> skills) {
        this.skills = new ArrayList<String>(skills);
    }
    
    /*
     * Group specific data
     */
    
	transient protected long groupOid = 0;
	
	public void setGroupOid(long groupOid){
		this.groupOid = groupOid;
	}

    public long getGroupOid(){
        return groupOid;
    }	
	
    transient protected long groupMemberOid = 0;
    
    public void setGroupMemberOid(long groupMemberOid){
        this.groupMemberOid = groupMemberOid;
    }
	
    public long getGroupMemberOid(){
        return groupMemberOid;
    }    
    
	public boolean isGrouped(){
		return groupOid > 0;
	}
	
	transient protected boolean pendingGroupInvite = false;
	
	public void setPendingGroupInvite(boolean flag){
	    this.pendingGroupInvite = flag;
	}
	
	public boolean isPendingGroupInvite(){
	    return this.pendingGroupInvite;
	}
    
    public final static String COMBAT_PROP_BACKREF_KEY = "combat.backref";
    protected final static String COMBAT_PROP_USERFLAG = "combat.userflag";
    protected final static String COMBAT_PROP_MOBFLAG = "combat.mobflag";

    public final static String COMBAT_PROP_AUTOATTACK_ABILITY = "combat.autoability";
    public final static String COMBAT_PROP_REGEN_EFFECT = "combat.regeneffect";

    public final static String COMBAT_PROP_ENERGY = "energy";
    public final static String COMBAT_PROP_HEALTH = "health";

    public final static String COMBAT_PROP_COMBATSTATE = "combatstate";

    public final static String COMBAT_PROP_DEADSTATE = "deadstate";

    public final static String COMBAT_PROP_ATTACKABLE = "attackable";

    private static final long serialVersionUID = 1L;

    static {
        try {
            BeanInfo info = Introspector.getBeanInfo(CombatInfo.class);
            PropertyDescriptor[] propertyDescriptors = info.getPropertyDescriptors();
            for (int i = 0; i < propertyDescriptors.length; ++i) {
                PropertyDescriptor pd = propertyDescriptors[i];
                if (pd.getName().equals("currentAction")) {
                    pd.setValue("transient", Boolean.TRUE);
                }
            }
        } catch (Exception e) {
            Log.error("failed beans initalization");
        }
    }
}
