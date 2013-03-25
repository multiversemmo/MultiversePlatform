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

package multiverse.mars.plugins;

import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;
import multiverse.msgsys.*;
import multiverse.server.util.*;
import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.plugins.*;
import multiverse.server.messages.PropertyMessage;
import multiverse.mars.core.*;
import multiverse.mars.core.MarsEffect.EffectState;
import multiverse.mars.objects.*;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;

//
// the combat plugin tracks autoattacks and resolves combat messages
//
public class CombatPlugin extends multiverse.server.engine.EnginePlugin {

    public CombatPlugin() {
	super(COMBAT_PLUGIN_NAME);
        setPluginType("Combat");
    }
    public static String COMBAT_PLUGIN_NAME = "Combat";

    public void onActivate() {
	try {
	    log.debug("CombatPlugin.onActivate()");
	    // register for msgtype->hooks
 	    registerHooks();

	    // subscribe to auto attack messages
	    MessageTypeFilter filter = new MessageTypeFilter();
	    filter.addType(CombatClient.MSG_TYPE_AUTO_ATTACK);
	    filter.addType(CombatClient.MSG_TYPE_START_ABILITY);
	    filter.addType(CombatClient.MSG_TYPE_RELEASE_OBJECT);
	    filter.addType(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT);
	    filter.addType(PropertyMessage.MSG_TYPE_PROPERTY);
	    filter.addType(WorldManagerClient.MSG_TYPE_DESPAWNED);
	    filter.addType(CombatClient.MSG_TYPE_ADD_SKILL);
 	    /* Long sub = */ Engine.getAgent().createSubscription(filter, this);

            registerLoadHook(Namespace.COMBAT, new CombatLoadHook());
            registerSaveHook(Namespace.COMBAT, new CombatSaveHook());

            registerPluginNamespace(Namespace.COMBAT, new CombatPluginGenerateSubObjectHook());
	}
	catch(Exception e) {
	    throw new MVRuntimeException("onActivate failed", e);
	}
    }

    public static void resolveAutoAttack(CombatInfo info) {
        if (Log.loggingDebug)
            log.debug("CombatPlugin.resolveAutoAttack: info=" + info);
        Long targetOid = info.getAutoAttackTarget();
	CombatInfo target = getCombatInfo(targetOid);
        if (target == null) {
            return;
        }
        String abilityName = (String)info.getProperty(CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY);
        MarsAbility ability = Mars.AbilityManager.get(abilityName);
        if (Log.loggingDebug)
            log.debug("CombatPlugin.resolveAutoAttack: abilityName " + abilityName + ", ability " + ability);
        MarsAbility.startAbility(ability, info, target, null);
    }

    // how to process incoming messages
    protected void registerHooks() {
  	getHookManager().addHook(CombatClient.MSG_TYPE_AUTO_ATTACK,
  				 new AutoAttackHook());
  	getHookManager().addHook(CombatClient.MSG_TYPE_START_ABILITY,
  				 new StartAbilityHook());
  	getHookManager().addHook(CombatClient.MSG_TYPE_RELEASE_OBJECT,
  				 new ReleaseObjectHook());
  	getHookManager().addHook(WorldManagerClient.MSG_TYPE_UPDATE_OBJECT,
				 new UpdateObjectHook());
	getHookManager().addHook(PropertyMessage.MSG_TYPE_PROPERTY,
				 new PropertyHook());
	getHookManager().addHook(WorldManagerClient.MSG_TYPE_DESPAWNED,
				 new DespawnedHook());
	getHookManager().addHook(CombatClient.MSG_TYPE_ADD_SKILL,
            	 new AddSkillHook());
    }

    public static void sendAbilityUpdate(CombatInfo info) {
        if (Log.loggingDebug)
            log.debug("CombatPlugin: sending AbilityUpdate for obj=" + info);
	CombatClient.AbilityUpdateMessage msg = new CombatClient.AbilityUpdateMessage(info.getOwnerOid(), info.getOwnerOid());
	for (String abilityName : info.getAbilities()) {
	    MarsAbility ability = Mars.AbilityManager.get(abilityName);
            if (Log.loggingDebug)
                log.debug("CombatPlug: adding ability to message. ability=" + ability);
	    msg.addAbility(ability.getName(), ability.getIcon(), "");
	}
	Engine.getAgent().sendBroadcast(msg);
    }

    public static CombatInfo getCombatInfo(Long oid) {
        return (CombatInfo)EntityManager.getEntityByNamespace(oid, Namespace.COMBAT);
    }
    
    public static void registerCombatInfo(CombatInfo cinfo) {
        EntityManager.registerEntityByNamespace(cinfo, Namespace.COMBAT);
    }
    
    class CombatLoadHook implements LoadHook {
	public void onLoad(Entity e) {
	    CombatInfo info = (CombatInfo) e;
	    Map<String, EffectState> effectMap = (Map<String, EffectState>)info.getProperty("regenEffectMap");
	    if (effectMap != null) {
		regenMap.put(info, effectMap);
	    }
	    for (EffectState state : info.getEffects()) {
		state.resume();
	    }
	}
    }
            
    class CombatSaveHook implements SaveHook {
	public void onSave(Entity e, Namespace namespace) {
	}
    }

    class CombatPluginGenerateSubObjectHook extends GenerateSubObjectHook {
	public CombatPluginGenerateSubObjectHook() {
	    super(CombatPlugin.this);
	}

        public SubObjData generateSubObject(Template template, Namespace namespace, Long masterOid) {
            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid=" + masterOid + ", template=" + template);
            
            Map<String, Serializable> props = template.getSubMap(Namespace.COMBAT);
            if (props == null) {
                Log.warn("GenerateSubObjectHook: no props in ns " + Namespace.COMBAT);
                return null;
            }
            
            // generate the subobject
            CombatInfo cinfo = new CombatInfo(masterOid);
            cinfo.setName(template.getName());

            Boolean persistent = (Boolean)template.get(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT);
            if (persistent == null)
                persistent = false;
            cinfo.setPersistenceFlag(persistent);

	    // copy properties from template to object
	    for (Map.Entry<String, Serializable> entry : props.entrySet()) {
		String key = entry.getKey();
		Serializable value = entry.getValue();
		if (!key.startsWith(":")) {
		    cinfo.setProperty(key, value);
		}
	    }

	    for (Map.Entry<String, MarsStatDef> statEntry : statDefMap.entrySet()) {
		String statName = statEntry.getKey();
		//MarsStatDef statDef = statEntry.getValue();

		MarsStat stat = (MarsStat) cinfo.getProperty(statName);
		if (stat == null) {
		    stat = new MarsStat(statName);
		    cinfo.setProperty(statName, stat);
		}
	    }
	    for (MarsStatDef statDef : baseStats) {
		String statName = statDef.getName();
		MarsStat stat = (MarsStat) cinfo.getProperty(statName);
		statDef.update(stat, cinfo);
	    }

            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: created entity " + cinfo);
            
            // register the entity
            registerCombatInfo(cinfo);
            
            if (persistent)
                Engine.getPersistenceManager().persistEntity(cinfo);

            // send a response message
            return new SubObjData();
        }
    }

    class AutoAttackHook implements Hook {
	public boolean processMessage(Message msg, int flags) {
	    CombatClient.AutoAttackMessage autoAtkMsg = (CombatClient.AutoAttackMessage) msg;
	    Long oid = autoAtkMsg.getSubject();
	    CombatInfo obj = getCombatInfo(oid);
	    Long targetOid = autoAtkMsg.getTargetOid();
	    CombatInfo target = getCombatInfo(targetOid);
            Boolean status = autoAtkMsg.getAttackStatus();
            Lock objLock = obj.getLock();
            Lock targetLock = null;
            if (target != null)
                targetLock = target.getLock();

            try {
                objLock.lock();
                while ((targetLock != null) && !targetLock.tryLock()) {
                    objLock.unlock();
                    Thread.yield();
                    objLock.lock();
                }

                if (Log.loggingDebug)
                    log.debug("AutoAttackHook.processMessage: oid=" + oid + ", targetOid=" + targetOid + ", status=" + status);
                
                if (!status || obj.dead() || (target == null) || target.dead()) {
                    obj.stopAutoAttack();
                }
                else {
                    obj.setAutoAttack(targetOid);
                }
                return true;
            }
            finally {
                if (targetLock != null)
                    targetLock.unlock();
                objLock.unlock();
            }
	}
    }

    class StartAbilityHook implements Hook {
	public boolean processMessage(Message msg, int flags) {
	    CombatClient.StartAbilityMessage abilityMsg = (CombatClient.StartAbilityMessage) msg;
	    Long oid = abilityMsg.getSubject();
	    Long targetOid = abilityMsg.getTargetOid();
	    String abilityName = abilityMsg.getAbilityName();

            if (Log.loggingDebug)
                log.debug("StartAbilityHook.processMessage: oid=" + oid + ", targetOid=" + targetOid +
                          " ability=" + abilityName);

	    CombatInfo obj = getCombatInfo(oid);
	    CombatInfo target = getCombatInfo(targetOid);
	    MarsAbility ability = Mars.AbilityManager.get(abilityName);

            MarsAbility.startAbility(ability, obj, target, null);
	    return true;
	}
    }

    class ReleaseObjectHook implements Hook {
	public boolean processMessage(Message msg, int flags) {
	    CombatClient.ReleaseObjectMessage releaseMsg = (CombatClient.ReleaseObjectMessage) msg;
	    Long oid = releaseMsg.getSubject();

            if (Log.loggingDebug)
                log.debug("ReleaseObjectHook.processMessage: oid=" + oid);

	    CombatInfo info = getCombatInfo(oid);
	    if (info == null)
		return true;
	    info.setDeadState(false);
	    info.statSetBaseValue("health", 1);
	    EnginePlugin.setObjectPropertiesNoResponse(info.getOwnerOid(), Namespace.WORLD_MANAGER,
                    WorldManagerClient.WORLD_PROP_NOMOVE, new Boolean(false),
                    WorldManagerClient.WORLD_PROP_NOTURN, new Boolean(false));
	    return true;
	}
    }

    class UpdateObjectHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            WorldManagerClient.UpdateMessage updateReq = (WorldManagerClient.UpdateMessage) msg;
            Long subjectOid = updateReq.getSubject();
	    Long targetOid = updateReq.getTarget();

            // is the update object spawned?
	    CombatInfo info = getCombatInfo(subjectOid);
            if (info == null) {
                return false;
            }

            // send over properties
            if (Log.loggingDebug)
                log.debug("UpdateObjectHook.processMessage: sending properties for subjectOid=" + subjectOid);

            WorldManagerClient.TargetedPropertyMessage propMessage =
                new WorldManagerClient.TargetedPropertyMessage(targetOid, subjectOid);
            for (String key : info.getPropertyMap().keySet()) {
                Serializable value = info.getProperty(key);
                if (!(value instanceof MarsStat))
                    propMessage.setProperty((String)key, value);
            }

            // Comment out next two lines for LES world
            Engine.getAgent().sendBroadcast(propMessage);
	    info.statSendUpdate(true, targetOid);

            // Abilities are only sent to the player themselves
            if (subjectOid.equals(targetOid))
            	ClassAbilityPlugin.sendSkillUpdate(info);
                sendAbilityUpdate(info);

            return true;
        }
    }

    class PropertyHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            PropertyMessage propMsg = (PropertyMessage) msg;
            Long objOid = propMsg.getSubject();

            Boolean dead = (Boolean)propMsg.getProperty(CombatInfo.COMBAT_PROP_DEADSTATE);

            if (dead != null && dead) {
                CombatInfo obj = getCombatInfo(objOid);
                obj.stopAutoAttack();

                Set<Long> attackers = getAttackers(objOid);

                if (attackers != null) {
                clearAttackers(objOid);
                // Send message to ClassAbilityPlugin to handle xp gain
                ExtensionMessage xpUpdateMsg = new ExtensionMessage(ClassAbilityClient.MSG_TYPE_HANDLE_EXP,
                        "mv.HANDLE_EXP",obj.getOwnerOid());
                    xpUpdateMsg.setProperty("attackers", new HashSet<Long>(attackers));
                Engine.getAgent().sendBroadcast(xpUpdateMsg);

                    for (Long attacker : attackers) {
                        CombatInfo info = getCombatInfo(attacker);
                        if (info != null) {
                            info.stopAutoAttack();
                        }
                    }
                }
                else
                    log.error("CombatPlugin.PropertyHook - no attackers");
            }

            return true;
        }
    }
    
    /**
     * Sets combat state to false upon despawning
     */
    class DespawnedHook implements Hook  {
    	public boolean processMessage(Message msg, int flags) {
    		WorldManagerClient.DespawnedMessage despawnedMsg = (WorldManagerClient.DespawnedMessage) msg;
            Long objOid = despawnedMsg.getSubject();
            CombatInfo obj = getCombatInfo(objOid);
            if (obj == null)
                return false;
            if (Log.loggingDebug)
                log.debug("DespawnedHook: got a despawned message for oid=" + objOid);
	    if (obj != null)
		obj.setCombatState(false);
            return true;
    	}
    }
    

    public static void addAttacker(Long target, Long attacker) {
	lock.lock();
	try {
	    Set<Long> attackers = autoAttackReverseMap.get(target);
	    if (attackers == null) {
		attackers = new HashSet<Long>();
		autoAttackReverseMap.put(target, attackers);
	    }
	    attackers.add(attacker);
	}
	finally {
	    lock.unlock();
	}
    }
    public static void removeAttacker(Long target, Long attacker) {
	lock.lock();
	try {
	    Set<Long> attackers = autoAttackReverseMap.get(target);
	    if (attackers != null) {
		attackers.remove(attacker);
		if (attackers.isEmpty()) {
		    autoAttackReverseMap.remove(target);
		}
	    }
	}
	finally {
	    lock.unlock();
	}
    }
    public static Set<Long> getAttackers(Long target) {
	lock.lock();
	try {
	    return autoAttackReverseMap.get(target);
	}
	finally {
	    lock.unlock();
	}
    }
    public static void clearAttackers(Long target) {
	lock.lock();
	try {
	    autoAttackReverseMap.remove(target);
	}
	finally {
	    lock.unlock();
	}
    }
    protected static Map<Long, Set<Long>> autoAttackReverseMap = new HashMap<Long, Set<Long>>();

    public static void startRegen(CombatInfo obj, String stat, MarsEffect effect) {
	lock.lock();
	try {
            if (Log.loggingDebug)
                log.debug("CombatPlugin.startRegen: obj=" + obj + " stat=" + stat);
	    Map<String, EffectState> map = regenMap.get(obj);
	    if (map == null) {
		map = new HashMap<String, EffectState>();
		regenMap.put(obj, map);
	    }
	    if (map.containsKey(stat))
		return;

	    EffectState state = MarsEffect.applyEffect(effect, obj, obj);
	    obj.setProperty("regenEffectMap", (Serializable)map);
	    map.put(stat, state);
	}
	finally {
	    lock.unlock();
	}
    }
    public static void stopRegen(CombatInfo obj, String stat) {
	lock.lock();
	try {
            if (Log.loggingDebug)
                log.debug("CombatPlugin.stopRegen: obj=" + obj + " stat=" + stat);
	    Map<String, EffectState> map = regenMap.get(obj);
	    if (map == null)
		return;
            if (!map.containsKey(stat)) {
                return;
            }
            EffectState state = map.remove(stat);
	    if (map.isEmpty()) {
		regenMap.remove(obj);
		map = null;
	    }
	    if (state == null)
		return;
	    obj.setProperty("regenEffectMap", (Serializable)map);
	    MarsEffect.removeEffect(state);
	}
	finally {
	    lock.unlock();
	}
    }
    protected static Map<CombatInfo, Map<String, EffectState>> regenMap =
	new HashMap<CombatInfo, Map<String, EffectState>>();

    protected static final Logger log = new Logger("CombatPlugin");
    protected static Lock lock = LockFactory.makeLock("CombatPlugin");

    public static void registerStat(MarsStatDef stat) {
	registerStat(stat, new String[0]);
    }
    public static void registerStat(MarsStatDef stat, String... dependencies) {
	String statName = stat.getName();
	if (statDefMap.containsKey(statName)) {
	    throw new MVRuntimeException("stat already defined");
	}
	statDefMap.put(statName, stat);
	if (dependencies.length == 0) {
	    baseStats.add(stat);
	}
	for (String depName : dependencies) {
	    MarsStatDef depStat = statDefMap.get(depName);
	    if (depStat != null) {
		depStat.addDependent(stat);
	    }
	    else {
		Log.error("no stat definition for dependency " + depName + " of stat " + statName);
	    }
	}
    }
    public static MarsStatDef lookupStatDef(String name) {
	return statDefMap.get(name);
    }
    protected static Map<String, MarsStatDef> statDefMap = new HashMap<String, MarsStatDef>();
    protected static Set<MarsStatDef> baseStats = new HashSet<MarsStatDef>();
    
    // Process Skill training message
    class AddSkillHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            ExtensionMessage reqMsg = (ExtensionMessage) msg;

            applySkillTraining((Long) reqMsg.getProperty("playerOid"),
                    (String) reqMsg.getProperty("skill"));

            return true;
        }
    }

    // Add the Skill to the player object and notify the client of the updates
    public void applySkillTraining(Long playerOid, String skill) {
        Log.debug("CombatPlugin.applySkillTraining : skill = " + skill);
        CombatInfo player = (CombatInfo) getCombatInfo(playerOid);
        // Only add the skill to the player if he does not already have that
        // skill
        ArrayList<String> skills = player.getSkills();
        if (!skills.contains(skill)) {
            player.addSkill(skill); // Also adds default ability
            ClassAbilityPlugin.sendSkillUpdate(player); // Handle skill updates
                                                        // in ClassAbilityPlugin
            sendAbilityUpdate(player); // Move to ClassAbilityPlugin?
        } else {
            Map<String, Serializable> props = new HashMap<String, Serializable>();
            props.put("ext_msg_subtype", "mv.TRAINING_FAILED");
            props.put("playerOid", playerOid);
            props.put("reason",
                    "You cannot train any further in the selected skill.");

            TargetedExtensionMessage msg = new TargetedExtensionMessage(
                    CombatClient.MSG_TYPE_TRAINING_FAILED, playerOid,
                    playerOid, false, props);
            Engine.getAgent().sendBroadcast(msg);
        }
    }

    public static MarsStatDef getBaseStatDef(String name) {
        for (MarsStatDef statdef : baseStats) {
            if (statdef.getName().equals(name)) {
                return statdef;
            }
        }
        return null;
    }

}
