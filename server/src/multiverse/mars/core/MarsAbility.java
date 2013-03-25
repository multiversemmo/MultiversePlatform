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
import multiverse.server.math.*;
import multiverse.server.plugins.*;
import multiverse.mars.objects.*;
import multiverse.mars.plugins.*;
import java.util.*;
import java.util.concurrent.locks.*;
import java.util.concurrent.*;

/**
 * The MarsAbility object describes an action that a mob can perform.
 * <p>
 * When an ability is triggered, a MarsAbility.State object is
 * generated that represents the current state of that instance of the
 * ability. It progresses through a sequence of states, dependent on
 * the configuration of the MarsAbility object.
 * <p>
 * As each state is entered or exitted, a method is called that can be
 * overriden to create different types of abilities.
 */
public class MarsAbility {
    public MarsAbility(String name) {
        setName(name);
    }

    /**
     * Returns the string describing this ability, useful for logging.
     * 
     * @return string describing ability
     */
    public String toString() {
        return "[MarsAbility: " + getName() + "]";
    }

    /**
     * Returns if two objects are the same - tested by comparing the ability name.
     * null objects are never equal to any other object including other null objects.
     *
     * @return true if abilities match
     */
    public boolean equals(Object other) {
        MarsAbility otherAbility = (MarsAbility) other;
        boolean val = getName().equals(otherAbility.getName());
        return val;
    }

    /**
     * Returns a hash of the ability name
     *
     * @return hash value of the object's ability name
     */
    public int hashCode() {
        int hash = getName().hashCode();
        return hash;
    }

    /**
     * MarsAbility lock
     */
    transient protected Lock lock = LockFactory.makeLock("MarsAbilityLock");

    /**
     * Sets the name of the ability. This is used to identify the
     * ability, so it should be unique.
     *
     * @param name name for this ability.
     */
    public void setName(String name) {
        this.name = name;
    }

    /**
     * Returns the name of the ability.
     *
     * @return name for this ability.
     */
    public String getName() {
        return name;
    }
    String name = null;

    public enum TargetType {
	UNINIT,
	NONE,
	ENEMY,
	FRIEND,
        GROUP,
	SELF,
	AREA
    }

    /**
     * Returns the time the ability takes to activate.
     *
     * @return time in ms to activate the ability.
     */
    public long getActivationTime() { return activationTime; }

    /**
     * Sets the time the ability takes to activate.
     *
     * @param time time in ms that the ability takes to activate.
     */
    public void setActivationTime(long time) { activationTime = time; }

    /**
     * Returns if the ability has 0 activation time.
     *
     * @return true if activate time is 0.
     */
    public boolean isInstant() { return activationTime == 0; }
    protected long activationTime = 0;

    /**
     * Returns the stat cost for successfully activating the ability.
     *
     * @return stat cost for activating the ability.
     */
    public int getActivationCost() { return activationCost; }

    /**
     * Sets the stat cost for successfully activating the ability.
     *
     * @param cost stat cost for activating the ability.
     */
    public void setActivationCost(int cost) { activationCost = cost; }
    protected int activationCost = 0;

    /**
     * Returns the name of the property that stat costs are deducted from.
     *
     * @return name of the property that stat costs are deducted from.
     */
    public String getCostProperty() { return costProp; }

    /**
     * Sets the name of the property that stat costs are deducted from.
     *
     * @param name name of the property that stat costs are deducted from.
     */
    public void setCostProperty(String name) { costProp = name; }
    protected String costProp = null;

    /**
     * Returns the time in ms for each pulse of a channelled ability.
     *
     * @return time in ms for each pulse of a channelled ability.
     */
    public long getChannelPulseTime() { return channelPulseTime; }

    /**
     * Sets the time in ms for each pulse of a channelled ability.
     *
     * @param time time in ms for each pulse of a channelled ability.
     */
    public void setChannelPulseTime(long time) { channelPulseTime = time; }
    protected long channelPulseTime = 0;

    /**
     * Returns the number of pulses during the channelled phase.
     *
     * @return number of pulses during the channelled phase for the ability.
     */
    public int getChannelPulses() { return channelPulses; }

    /**
     * Sets the number of pulses during the channelled phase.
     *
     * @param pulses number of pulses during the channelled phase for the ability.
     */
    public void setChannelPulses(int pulses) { channelPulses = pulses; }
    protected int channelPulses = 0;

    /**
     * Returns the stat cost charged for each channelling pulse.
     *
     * @return stat cost charged for each channelling pulse.
     */
    public int getChannelCost() { return channelCost; }

    /**
     * Sets the stat cost charged for each channelling pulse.
     *
     * @param cost stat cost charged for each channelling pulse.
     */
    public void setChannelCost(int cost) { channelCost = cost; }
    protected int channelCost = 0;

    /**
     * Returns the time in ms for each pulse of the active phase.
     *
     * @return time in ms for each pulse of the active phase.
     */
    public long getActivePulseTime() { return activePulseTime; }

    /**
     * Set the time in ms for each pulse of the active phase.
     *
     * @param time time in ms for each pulse of the active phase.
     */
    public void setActivePulseTime(long time) { activePulseTime = time; }
    protected long activePulseTime = 0;

    /**
     * Returns the stat cost charged for each pulse of the active phase.
     *
     * @return stat cost charged for each pulse of the active phase.
     */
    public int getActiveCost() { return activeCost; }

    /**
     * Sets the stat cost charged for each pulse of the active phase.
     *
     * @param cost stat cost charged for each pulse of hte active phase.
     */
    public void setActiveCost(int cost) { activeCost = cost; }
    protected int activeCost = 0;

    /**
     * Returns the icon name for this ability.
     *
     * @return icon name for this ability.
     */
    public String getIcon() { return icon; }

    /**
     * Sets the icon name for this ability.
     *
     * @param icon icon name for this ability.
     */
    public void setIcon(String icon) { this.icon = icon; }
    protected String icon = null;

    /**
     * Returns the minimum range in mm for this ability.
     *
     * @return minimum range in mm for this ability.
     */
    public int getMinRange() { return minRange; }

    /**
     * Sets the minimum range in mm for this ability.
     *
     * @param range minimum range in mm for this ability.
     */
    public void setMinRange(int range) { minRange = range; }
    protected int minRange = 0;

    /**
     * Returns the maximum range in mm for this ability.
     *
     * @return maximum range in mm for this ability.
     */
    public int getMaxRange() { return maxRange; }

    /**
     * Sets the maximum range in mm for this ability.
     *
     * @param range Maximum range in mm for this ability.
     */
    public void setMaxRange(int range) { maxRange = range; }
    protected int maxRange = 0;

    /**
     * Adds a cooldown to this ability. If any of the ability's cooldowns are activate on
     * the mob attempting to activate the ability, it will not be able to activate.
     *
     * @param cd Cooldown to add to this ability.
     */
    public void addCooldown(Cooldown cd) {
	try {
	    lock.lock();
	    cooldownMap.put(cd.getID(), cd);
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * Removes a cooldown from this ability.
     *
     * @param id id of the cooldown to remove.
     */
    public void removeCooldown(String id) {
	try {
	    lock.lock();
	    cooldownMap.remove(id);
	}
	finally {
	    lock.unlock();
	}
    }

    public Map<String, Cooldown> getCooldownMap() {
	try {
	    lock.lock();
	    return new HashMap<String, Cooldown>(cooldownMap);
	}
	finally {
	    lock.unlock();
	}
    }
    public void setCooldownMap(Map<String, Cooldown> cooldownMap) {
	try {
	    lock.lock();
	    this.cooldownMap = new HashMap<String, Cooldown>(cooldownMap);
	}
	finally {
	    lock.unlock();
	}
    }
    protected Map<String, Cooldown> cooldownMap = new HashMap<String, Cooldown>();

    /**
     * Adds a reagent requirement to this ability. Reagents are items that are required
     * to be present in inventory, and are consumed when the ability completes the
     * ACTIVATING phase.
     *
     * @param reagent name of the template the reagent was created from.
     */
    public void addReagent(String reagent) {
	try {
	    lock.lock();
	    reagentList.add(reagent);
	}
	finally {
	    lock.unlock();
	}
    }
    public ArrayList<String> getReagentList() {
	try {
	    lock.lock();
	    return new ArrayList<String>(reagentList);
	}
	finally {
	    lock.unlock();
	}
    }
    public void setReagentList(ArrayList<String> reagentList) {
	try {
	    lock.lock();
	    this.reagentList = new ArrayList<String>(reagentList);
	}
	finally {
	    lock.unlock();
	}
    }
    protected ArrayList<String> reagentList = new ArrayList<String>();

    /**
     * Adds a tool requirement to this ability. Tools are items that are required
     * to be present in inventory. They are not consumed.
     *
     * @param tool name of the template the tool was created from.
     */
    public void addTool(String tool) {
	try {
	    lock.lock();
	    toolList.add(tool);
	}
	finally {
	    lock.unlock();
	}
    }
    public ArrayList<String> getToolList() {
	try {
	    lock.lock();
	    return new ArrayList<String>(toolList);
	}
	finally {
	    lock.unlock();
	}
    }
    public void setToolList(ArrayList<String> toolList) {
	try {
	    lock.lock();
	    this.toolList = new ArrayList<String>(toolList);
	}
	finally {
	    lock.unlock();
	}
    }
    protected ArrayList<String> toolList = new ArrayList<String>();

    /**
     * Returns the target type for this ability.
     *
     * @return target type for this ability.
     */
    public TargetType getTargetType() { return targetType; }

    /**
     * Sets the target type for this ability.
     *
     * @param type target type for this ability.
     */
    public void setTargetType(TargetType type) { targetType = type; }
    protected TargetType targetType = TargetType.UNINIT;

    public boolean getUseGlobalCooldown() { return useGlobalCooldown; }
    public void setUseGlobalCooldown(boolean val) { useGlobalCooldown = val; }
    protected boolean useGlobalCooldown = true;

    public boolean getStationary() { return stationary; }
    public void setStationary(boolean val) { stationary = val; }
    protected boolean stationary = false;

    public boolean getChannelled() { return channelled; }
    public void setChannelled(boolean val) { channelled = val; }
    protected boolean channelled = false;

    public boolean getPersistent() { return persistent; }
    public void setPersistent(boolean val) { persistent = val; }
    protected boolean persistent = false;

    public boolean addCoordEffect(ActivationState state, CoordinatedEffect effect) {
	Set<CoordinatedEffect> effectSet = coordEffectMap.get(state);
	if (effectSet == null) {
	    effectSet = new HashSet<CoordinatedEffect>();
	    coordEffectMap.put(state, effectSet);
	}
	return effectSet.add(effect);
    }
    public boolean removeCoordEffect(ActivationState state, CoordinatedEffect effect) {
	Set<CoordinatedEffect> effectSet = coordEffectMap.get(state);
	if (effectSet == null) {
	    return false;
	}
	return effectSet.remove(effect);
    }
    public Collection<CoordinatedEffect> getCoordEffects(ActivationState state) {
	Set<CoordinatedEffect> effectSet = coordEffectMap.get(state);
	if (effectSet == null) {
	    effectSet = new HashSet<CoordinatedEffect>();
	    coordEffectMap.put(state, effectSet);
	}
	return effectSet;
    }
    protected Map<ActivationState, Set<CoordinatedEffect>> coordEffectMap =
	new HashMap<ActivationState, Set<CoordinatedEffect>>();

    public String getCompleteAnimation() { return completeAnimation; }
    public void setCompleteAnimation(String anim) { completeAnimation = anim; }
    protected String completeAnimation;

    public String getCompleteSound() { return completeSound; }
    public void setCompleteSound(String sound) { completeSound = sound; }
    protected String completeSound;

    // begin activating the ability
    public void beginActivation(State state) {
        Log.debug("MarsAbility.beginActivation:");
    }

    public void completeActivation(State state) {
        Log.debug("MarsAbility.completeActivation:");
        CombatInfo combatInfo = state.getObject();
        
        if (!reagentList.isEmpty()) {
            List<Long> items =
                InventoryClient.removeItems(combatInfo.getOwnerOid(),
                    reagentList);
            if (items != null) {
                for (Long itemOid : items) {
                    ObjectManagerClient.deleteObject(itemOid);
                }
            }
        }
        
        if (costProp != null) {
            combatInfo.statModifyBaseValue(costProp, -activationCost);
            combatInfo.sendStatusUpdate();
        }
        
//      WorldManagerClient.sendObjChatMsg(Engine.msgSession, combatInfo.getOwnerOid(),
//      ComEvent.COMBAT_INFO,
//      "Ability " + state.getAbility() + " complete");
        if (completeAnimation != null) {
            AnimationClient.playSingleAnimation(combatInfo.getOwnerOid(), completeAnimation);
        }
        if (completeSound != null) {
            WorldManagerClient.SoundMessage soundMsg =
                new WorldManagerClient.SoundMessage(combatInfo.getOwnerOid());
            soundMsg.addSound(completeSound, false);
            Engine.getAgent().sendBroadcast(soundMsg);
        }
        
        /* MarsItem item = */ state.getItem();
        
        Collection<Cooldown>cooldowns = state.getAbility().getCooldownMap().values();
        Cooldown.activateCooldowns(cooldowns, combatInfo);
        Log.debug("MarsAbility.completeActivation: finished");
    }

    public void beginChannelling(State state) {
        Log.debug("MarsAbility.beginChannelling:");
    }

    public void pulseChannelling(State state) {
	CombatInfo obj = state.getObject();
        if (Log.loggingDebug)
            Log.debug("MarsAbility.pulseChannelling: cost=" + channelCost);
	if (costProp != null) {
	    obj.statModifyBaseValue(costProp, -channelCost);
	    obj.sendStatusUpdate();
	}
    }

    public void completeChannelling(State state) {
	Log.debug("MarsAbility.completeChannelling:");
    }

    public void beginActivated(State state) {
	Log.debug("MarsAbility.beginActivated:");
    }

    public void pulseActivated(State state) {
	Log.debug("MarsAbility.pulseActivated:");
	CombatInfo obj = state.getObject();
	if (costProp != null) {
	    obj.statModifyBaseValue(costProp, -activeCost);
	    obj.sendStatusUpdate();
	}
    }

    public void endActivated(State state) {
	Log.debug("MarsAbility.endActivated:");
    }

    public void interrupt(State state) {
	Log.debug("MarsAbility.interrupt:");
    }

    /**
     * exposes a way for the client to execute ability with a slash command
     */
    public void setSlashCommand(String slashCommand) {
        this.slashCommand = slashCommand;
    }
    public String getSlashCommand() {
        return slashCommand;
    }
    String slashCommand = null;

    public void setRequiredSkill(MarsSkill skill, int level) {
        requiredSkill = skill;
        requiredSkillLevel = level;
    }
    public MarsSkill getRequiredSkill() {
        return requiredSkill;
    }
    public int getRequiredSkillLevel() {
        return requiredSkillLevel;
    }
    MarsSkill requiredSkill = null;
    int requiredSkillLevel = -1;

    public enum AbilityResult {
	SUCCESS,
	OUT_OF_RANGE,
	INVALID_TARGET,
	NOT_READY,
	TOO_CLOSE,
	OUT_OF_LOS,
	INSUFFICIENT_ENERGY,
	BAD_ASPECT,
	MISSING_REAGENT,
	MISSING_TOOL,
	BUSY
    }

    protected AbilityResult checkTarget(CombatInfo obj, CombatInfo target) {
        if (Log.loggingDebug)
            Log.debug( "MarsAbility.checkTarget: obj=" + obj + " isUser=" + obj.isUser() + "target="
		       + target + " attackable=" + ((target==null)?"N/A":target.attackable()) );
	switch (targetType) {
	case NONE:
	case SELF:
	    return AbilityResult.SUCCESS;
	case FRIEND:
	case GROUP:
	    if (obj.isUser() && target.attackable())
		return AbilityResult.INVALID_TARGET;
	    else
		return AbilityResult.SUCCESS;
	case ENEMY:
	    if (obj.isUser() && !target.attackable())
		return AbilityResult.INVALID_TARGET;
	    else
		return AbilityResult.SUCCESS;
	default:
	    return AbilityResult.INVALID_TARGET;
	}
    }

    protected AbilityResult checkRange(CombatInfo obj, CombatInfo target, float rangeTollerance) {
        switch (targetType) {
        case ENEMY:
        case FRIEND:
        case GROUP:
            BasicWorldNode casterWNode = WorldManagerClient.getWorldNode(obj.getOwnerOid());
            BasicWorldNode targetWNode = WorldManagerClient.getWorldNode(target.getOwnerOid());
            Point casterLoc = casterWNode.getLoc();
            Point targetLoc = targetWNode.getLoc();
            int range = (int)Point.distanceTo(casterLoc, targetLoc);
            Log.debug("MarsAbility.checkRange: range=" + range + " casterLoc=" + casterLoc + " targetLoc=" + targetLoc);
            if (range > (getMaxRange() * rangeTollerance)) {
                return AbilityResult.OUT_OF_RANGE;
            }
            if (range < (getMinRange() / rangeTollerance)) {
                return AbilityResult.TOO_CLOSE;
            }
            return AbilityResult.SUCCESS;
        default:
            return AbilityResult.SUCCESS;
        }
    }

    protected AbilityResult checkReady(CombatInfo obj, CombatInfo target) {
 	if (obj.getCurrentAction() != null) {
 	    return AbilityResult.BUSY;
 	}
	if (!Cooldown.checkReady(cooldownMap.values(), obj)) {
	    return AbilityResult.NOT_READY;
	}
	return AbilityResult.SUCCESS;
    }

    protected AbilityResult checkCost(CombatInfo obj, CombatInfo target, ActivationState state) {
	if (costProp == null) {
            if (Log.loggingDebug)
                Log.debug("MarsAbility.checkCost: costProp=" + costProp);
	    return AbilityResult.SUCCESS;
	}
	Integer costValue = obj.statGetCurrentValue(costProp);
        if (Log.loggingDebug)
            Log.debug("MarsAbility.checkCost: costProp=" + costProp + " value=" + costValue);
	switch (state) {
	case INIT:
	case ACTIVATING:
	    if (getActivationCost() > obj.statGetCurrentValue(costProp)) {
		return AbilityResult.INSUFFICIENT_ENERGY;
	    }
	    break;
	case CHANNELLING:
	    if (getChannelCost() > obj.statGetCurrentValue(costProp)) {
		return AbilityResult.INSUFFICIENT_ENERGY;
	    }
	    break;
	case ACTIVATED:
	    if (getActiveCost() > obj.statGetCurrentValue(costProp)) {
		return AbilityResult.INSUFFICIENT_ENERGY;
	    }
	    break;
	}
	return AbilityResult.SUCCESS;
    }

    protected AbilityResult checkReagent(CombatInfo obj, CombatInfo target, ActivationState state) {
        if (state == ActivationState.INIT || state == ActivationState.ACTIVATING) {
            if (!reagentList.isEmpty()) {
                List<Long> itemList = InventoryClient.findItems(obj.getOwnerOid(), reagentList);
                if ((itemList == null) || itemList.contains(null)) {
                    return AbilityResult.MISSING_REAGENT;
                }
            }
        }
        return AbilityResult.SUCCESS;
    }

    protected AbilityResult checkTool(CombatInfo obj, CombatInfo target, ActivationState state) {
        if (state == ActivationState.INIT || state == ActivationState.ACTIVATING) {
            if (!toolList.isEmpty()) {
                List<Long> itemList = InventoryClient.findItems(obj.getOwnerOid(), toolList);
                if ((itemList == null) || itemList.contains(null)) {
                    return AbilityResult.MISSING_TOOL;
                }
            }
        }
        return AbilityResult.SUCCESS;
    }

    public AbilityResult checkAbility(CombatInfo obj, CombatInfo target) {
	return checkAbility(obj, target, ActivationState.INIT);
    }

    protected AbilityResult checkAbility(CombatInfo obj, CombatInfo target, ActivationState state) {
	AbilityResult result = AbilityResult.SUCCESS;

	if (state == ActivationState.INIT) {
	    result = checkReady(obj, target);
	    if (result != AbilityResult.SUCCESS)
		return result;
	    result = checkTarget(obj, target);
	    if (result != AbilityResult.SUCCESS)
		return result;
	}

	result = checkTool(obj, target, state);
	if (result != AbilityResult.SUCCESS)
	    return result;

	result = checkReagent(obj, target, state);
	if (result != AbilityResult.SUCCESS)
	    return result;

	result = checkCost(obj, target, state);
	if (result != AbilityResult.SUCCESS)
	    return result;

 	if (state == ActivationState.INIT) {
 	    result = checkRange(obj, target, 1.0f);
 	}
 	else {
 	    result = checkRange(obj, target, 1.2f);
 	}
        if (Log.loggingDebug)
            Log.debug("MarsAbility.checkAbility result=" + result);
	return result;
    }

    protected State generateState(CombatInfo obj, CombatInfo target, MarsItem item) {
	return new State(this, obj, target, item);
    }

    public static void startAbility(MarsAbility ability, CombatInfo obj, CombatInfo target,
				       MarsItem item) {
        if (Log.loggingDebug)
            Log.debug("MarsAbility.startAbility ability=" + ability.getName() + " obj=" + obj + " target=" +
                      target + " item=" + item);
	State state = ability.generateState(obj, target, item);
	state.updateState();
    }

    public static void interruptAbility(State state, AbilityResult reason) {
        if (Log.loggingDebug)
            Log.debug("MarsAbility.interruptAbility: reason=" + reason + " state=" + state.getState());

	if (state.getState() != ActivationState.INIT) {
	    Engine.getExecutor().remove(state);
	    if (state.getObject().getCurrentAction() == state) {
		state.getObject().setCurrentAction(null);
	    }
	    if (state.getState() == ActivationState.COMPLETED)
		return;
	}

 	state.getAbility().interrupt(state);
	
	// XXX do something here about adjusting the time remaining
	// XXX up or down for cast or channelled abilities
	return;
    }

    public enum ActivationState {
	INIT,
	ACTIVATING,
	CHANNELLING,
	ACTIVATED,
	COMPLETED,
	CANCELLED,
	INTERRUPTED,
	FAILED
    }

    public class State implements Runnable {

	public State(MarsAbility ability, CombatInfo obj, CombatInfo target, MarsItem item) {
	    if (ability.targetType == TargetType.SELF) {
		target = obj;
	    }
	    this.ability = ability;
	    this.obj = obj;
	    this.target = target;
	    this.item = item;
	}

	public ActivationState nextState() {
	    ActivationState newState;

	    switch (state) {
	    case INIT:
		newState = ActivationState.ACTIVATING;
		break;
	    case ACTIVATING:
		if (ability.getChannelled()) {
		    newState = ActivationState.CHANNELLING;
		    break;
		}
		if (ability.getPersistent()) {
		    newState =  ActivationState.ACTIVATED;
		    break;
		}
		newState =  ActivationState.COMPLETED;
		break;
	    case CHANNELLING:
		if (ability.getPersistent()) {
		    newState = ActivationState.ACTIVATED;
		    break;
		}
		newState = ActivationState.COMPLETED;
		break;
	    default:
		Log.error("MarsAbility.nextState: invalid state=" + state);
		newState = ActivationState.COMPLETED;
		break;
	    }
            if (Log.loggingDebug)
                Log.debug("MarsAbility.nextState: switching from " + state + " to " + newState);
	    return newState;
	}

	public void run() {
	    try {
		updateState();
	    }
	    catch (Exception e) {
		Log.exception("MarsAbility.State.run: got exception", e);
	    }
	}

	// this is executed when the ability needs to update
	public void updateState() {
            Lock objLock = obj.getLock();
            Lock targetLock = (target == null) ? null : target.getLock();

            try {
                objLock.lock();
                if (targetLock != null) {
                    while (!targetLock.tryLock()) {
                        objLock.unlock();
                        Thread.yield();
                        objLock.lock();
                    }
                }

                AbilityResult result;

                switch (state) {
                case INIT:
                    result = ability.checkAbility(obj, target, state);
                    if (result != AbilityResult.SUCCESS) {
                        MarsAbility.interruptAbility(this, result);
                        return;
                    }
                    obj.setCurrentAction(this);
                    break;
                case ACTIVATING:
                    result = ability.checkAbility(obj, target, state);
                    if (result != AbilityResult.SUCCESS) {
                        MarsAbility.interruptAbility(this, result);
                        return;
                    }
                    ability.completeActivation(this);
                    break;
                case CHANNELLING:
                    result = ability.checkAbility(obj, target, state);
                    if (result != AbilityResult.SUCCESS) {
                        MarsAbility.interruptAbility(this, result);
                        return;
                    }
                    ability.pulseChannelling(this);
                    nextPulse++;
                    if (nextPulse < ability.getChannelPulses()) {
                        schedule(ability.getChannelPulseTime());
                        return;
                    }
                    ability.completeChannelling(this);
                    break;
                case ACTIVATED:
                    result = ability.checkAbility(obj, target, state);
                    if (result != AbilityResult.SUCCESS) {
                        MarsAbility.interruptAbility(this, result);
                        return;
                    }
                    ability.pulseActivated(this);
                    nextPulse++;
                    schedule(ability.getActivePulseTime());
                    return;
                }

                state = nextState();
                nextPulse = 0;

                for(CoordinatedEffect effect : getCoordEffects(state)) {
                    effect.invoke(getObject().getOwnerOid(), getTarget().getOwnerOid());
                }

                switch (state) {
                case ACTIVATING:
                    ability.beginActivation(this);
                    setDuration(ability.getActivationTime());
                    schedule(ability.getActivationTime());
                    break;
                case CHANNELLING:
                    ability.beginChannelling(this);
                    setDuration(ability.getChannelPulses() * ability.getChannelPulseTime());
                    schedule(ability.getChannelPulseTime());
                    break;
                case ACTIVATED:
                    ability.beginActivated(this);
                    obj.setCurrentAction(null);
                    obj.addActiveAbility(this);
                    schedule(ability.getActivePulseTime());
                    break;
                case COMPLETED:
                    if (obj.getCurrentAction() == this) {
                        obj.setCurrentAction(null);
                    }
                    break;
                default:
                    Log.error("MarsAbility.State.run: new state invalid=" + state);
                    break;
                }
                Engine.getAgent().sendBroadcast(new CombatClient.AbilityProgressMessage(this));
                return;
            }
            finally {
                if (targetLock != null) {
                    targetLock.unlock();
                }
                objLock.unlock();
            }
	}

	protected void schedule(long delay) {
	    setTimeRemaining(delay);
	    Engine.getExecutor().schedule(this, delay, TimeUnit.MILLISECONDS);
	}

	public MarsAbility getAbility() { return ability; }
	public void setAbility(MarsAbility ability) { this.ability = ability; }
	protected MarsAbility ability;

	public CombatInfo getObject() { return obj; }
	public void setObject(CombatInfo obj) { this.obj = obj; }
	protected CombatInfo obj;

	public CombatInfo getTarget() { return target; }
	public void setTarget(CombatInfo target) { this.target = target; }
	protected CombatInfo target;

	public MarsItem getItem() { return item; }
	public void setItem(MarsItem item) { this.item = item; }
	protected MarsItem item;

	public long getNextWakeupTime() { return nextWakeupTime; }
	public long getTimeRemaining() { return nextWakeupTime - System.currentTimeMillis(); }
	public void setTimeRemaining(long time) { nextWakeupTime = System.currentTimeMillis() + time; }
	protected long nextWakeupTime;

	public long getDuration() { return duration; }
	public void setDuration(long duration) { this.duration = duration; }
	protected long duration;

	public ActivationState getState() { return state; }
	public void setState(ActivationState state) { this.state = state; }
	protected ActivationState state = ActivationState.INIT;

	public int getNextPulse() { return nextPulse; }
	public void setNextPulse(int num) { nextPulse = num; }
	protected int nextPulse = 0;
    }

    public static class Entry {
	public Entry() {
	}

	public Entry(String abilityName, String icon, String category) {
	    setAbilityName(abilityName);
	    setIcon(icon);
	    setCategory(category);
	}

	public Entry(MarsAbility ability, String category) {
	    setAbilityName(ability.getName());
	    setIcon(ability.getIcon());
	    setCategory(category);
	}

	public String getAbilityName() { return abilityName; }
	public void setAbilityName(String abilityName) {
	    this.abilityName = abilityName;
	}
	protected String abilityName;

	public String getIcon() { return icon; }
	public void setIcon(String icon) { this.icon = icon; }
	protected String icon;

	public String getCategory() { return category; }
	public void setCategory(String category) { this.category = category; }
	protected String category;

	public MarsAbility getAbility() {
	    return Mars.AbilityManager.get(abilityName);
	}
    }
    
    /**
     * -Experience system component-
     * 
     * This variable is used for setting up how much experience each successful
     * use of an ability is.
     */
    int exp_per_use = 0;

    /**
     * -Experience system component-
     * 
     * Returns the amount of experience should be gained by successful use of
     * this ability.
     */
    public int getExperiencePerUse() {
        return exp_per_use;
    }

    /**
     * -Experience system component-
     * 
     * Sets the amount of experience should be gained by successsful use of this
     * ability.
     */
    public void setExperiencePerUse(int xp) {
        exp_per_use = xp;
    }

    LevelingMap lm = new LevelingMap();

    public void setLevelingMap(LevelingMap lm) {
        this.lm = lm;
    }

    public LevelingMap getLevelingMap() {
        return this.lm;
    }

    int exp_max = 100;

    /**
     * -Experience system component-
     * 
     * Returns the default max experience that will be needed before the ability
     * gains a level.
     */
    public int getBaseExpThreshold() {
        return exp_max;
    }

    /**
     * -Experience system component-
     * 
     * Sets the default max experience that will be needed before the ability
     * gains a level.
     */
    public void setBaseExpThreshold(int max) {
        exp_max = max;
    }

    int rank_max = 3;

    /**
     * -Experience system component-
     * 
     * Returns the max rank that an ability may achieve.
     */
    public int getMaxRank() {
        return rank_max;
    }

    /**
     * -Experience system component-
     * 
     * Sets the max rank that an ability may achieve.
     */
    public void setMaxRank(int rank) {
        rank_max = rank;
    }

}
