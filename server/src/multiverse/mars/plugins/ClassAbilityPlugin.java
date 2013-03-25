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

/**
 *
 */
package multiverse.mars.plugins;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import multiverse.mars.core.Mars;
import multiverse.mars.core.MarsAbility;
import multiverse.mars.core.MarsSkill;
import multiverse.mars.objects.ClassAbilityObject;
import multiverse.mars.objects.CombatInfo;
import multiverse.mars.objects.LevelingMap;
import multiverse.mars.objects.MarsStat;
import multiverse.mars.objects.MarsStatDef;
import multiverse.mars.objects.ProfessionObject;
import multiverse.mars.plugins.CombatClient.AbilityUpdateMessage.Entry;
import multiverse.msgsys.Message;
import multiverse.msgsys.MessageTypeFilter;
import multiverse.server.engine.Engine;
import multiverse.server.engine.EnginePlugin;
import multiverse.server.engine.Hook;
import multiverse.server.engine.Namespace;
import multiverse.server.objects.Entity;
import multiverse.server.objects.EntityManager;
import multiverse.server.objects.Template;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.server.util.Log;
import multiverse.server.util.Logger;

/**
 * @author Judd
 *
 */
public class ClassAbilityPlugin extends EnginePlugin {

    /**
     * Holds the list of professions
     */
    protected static HashMap<String, ProfessionObject> professions = new HashMap<String, ProfessionObject>();

    /*
     * This will hold the keying between ability start and ability completion
     */
    HashMap<String, ArrayList<Long>> playerabilitykey = new HashMap<String, ArrayList<Long>>();

    protected static Map<String, MarsStatDef> statDefMap = new HashMap<String, MarsStatDef>();

    public ClassAbilityPlugin() {
        super("ClassAbility");
        setPluginType("ClassAbility");
    }

    private static final Logger log = new Logger("ClassAbility");

    public void onActivate() {
        if (Log.loggingDebug)
            log.debug(this.getName() + " OnActivate Started");
        super.onActivate();
        if (Log.loggingDebug)
            log.debug(this.getName() + " base class onActivate ran");
        registerHooks();
        if (Log.loggingDebug)
            log.debug(this.getName() + " registered hooks");
        MessageTypeFilter filter = new MessageTypeFilter();
        filter.addType(CombatClient.MSG_TYPE_SKILL_UPDATE);
        filter.addType(CombatClient.MSG_TYPE_ABILITY_UPDATE);
        filter.addType(CombatClient.MSG_TYPE_ABILITY_PROGRESS);
        filter.addType(CombatClient.MSG_TYPE_START_ABILITY);
        filter.addType(ClassAbilityClient.MSG_TYPE_HANDLE_EXP);
        Engine.getAgent().createSubscription(filter, this);

        registerLoadHook(Namespace.CLASSABILITY, new ClassAbilityLoadHook());
        registerSaveHook(Namespace.CLASSABILITY, new ClassAbilitySaveHook());

        this.registerPluginNamespace(ClassAbilityClient.NAMESPACE, new ClassAbilitySubObjectHook());

        // Get the ability list from the database.
        //this.buildAbilitiesList();

        if (Log.loggingDebug)
            log.debug(this.getName() + " activated");
    }

    public void registerHooks() {
        getHookManager().addHook(CombatClient.MSG_TYPE_ABILITY_UPDATE, new ClassAbilityAddAbilityHook());
        getHookManager().addHook(CombatClient.MSG_TYPE_ABILITY_PROGRESS, new ClassAbilityAbilityProgressHook());
        getHookManager().addHook(CombatClient.MSG_TYPE_START_ABILITY, new ClassAbilityStartAbilityHook());
        getHookManager().addHook(ClassAbilityClient.MSG_TYPE_HANDLE_EXP, new ClassAbilityHandleXpHook());
    }

    public class ClassAbilitySubObjectHook extends GenerateSubObjectHook {

        public ClassAbilitySubObjectHook(){ super(ClassAbilityPlugin.this); }

        public SubObjData generateSubObject(Template template, Namespace namespace, Long masterOid) {
            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid=" + masterOid
                          + ", template=" + template);

            if(masterOid == null) {
                log.error("GenerateSubObjectHook: no master oid");
                return null;
            }

            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid="+masterOid+", template="+template);

            Map<String, Serializable> props = template.getSubMap(ClassAbilityClient.NAMESPACE);

            if (props == null) {
                Log.warn("GenerateSubObjectHook: no props in ns "
                         + ClassAbilityClient.NAMESPACE);
                return null;
            }

            // generate the subobject
            ClassAbilityObject tinfo = new ClassAbilityObject(masterOid);
            tinfo.setName(template.getName());

            Boolean persistent = (Boolean)template.get(Namespace.OBJECT_MANAGER,
                                                       ObjectManagerClient.TEMPL_PERSISTENT);
            if (persistent == null)
                persistent = false;
            tinfo.setPersistenceFlag(persistent);

            // copy properties from template to object
            for (Map.Entry<String, Serializable> entry : props.entrySet()) {
                String key = entry.getKey();
                Serializable value = entry.getValue();
                if (!key.startsWith(":")) {
                    tinfo.setProperty(key, value);
                }
            }

            tinfo.setPlayerClass((String)tinfo.getProperty("class"));

            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: created entity " + tinfo);

            // register the entity
            EntityManager.registerEntityByNamespace(tinfo, ClassAbilityClient.NAMESPACE);

            //send a response message
            return new SubObjData();
        }
    }

    public class ClassAbilityAddAbilityHook implements Hook {
        public boolean processMessage(Message msg, int flags) {

            CombatClient.AbilityUpdateMessage reqMsg = (CombatClient.AbilityUpdateMessage) msg;
            // extract the base information
            List<Entry> skilllist = reqMsg.getAbilities();
            Long oid = reqMsg.getSubject();

            ClassAbilityObject caobj = (ClassAbilityObject)EntityManager.getEntityByNamespace(oid, ClassAbilityClient.NAMESPACE);
            if (caobj == null)
                return true;
            String playerclass = caobj.getPlayerClass();

            for (Entry e : skilllist) {

                log.debug("Adding ability to the player: " + oid + " ability: " + e.abilityName);

                // do a couple of double checks..
                if (Mars.AbilityManager.keySet().contains(e.abilityName)) {
                    // okay it is a valid system skill, now make sure it is in our list
                    // but first we need the player's object for classability

                    if (playerclass == null) {
                        // they didn't define a class for this character, so we can't track stats for them
                        log.warn("They didn't define a class type for this player...");
                        return true;
                    }

                    // now use the player's class to get the list of skills avaiable
                    if (professions.get(playerclass).hasAbility(e.abilityName)) {
                        if (caobj.getProperty(e.abilityName + "_exp") == null) {
                            // this is valid skill for tracking experience. So generate a marstat for this skill
                            createStats(caobj, Mars.AbilityManager.get(e.abilityName), professions.get(playerclass).getAbility(e.abilityName).getExperiencePerUse());
                        }
                    }
                }
            }

            // return true so that the chain continues if there is more
            return true;
        }

    }

    /**
     * This method creates stats based on the passed in skill, and assigns them to the player via the
     * created players' ClassAbilityObject.
     *
     * @param caobj
     * @param skill
     * @param xp_use
     */
    public static void createStats(ClassAbilityObject caobj, MarsSkill skill, Integer xp_use) {
        MarsStat tmp_exp = new MarsStat(skill.getName() + "_exp");
        tmp_exp.min = tmp_exp.current = tmp_exp.base = 0 ;
        tmp_exp.max = skill.getBaseExpThreshold();

        MarsStat tmp_rank = new MarsStat(skill.getName() + "_rank");
        tmp_rank.min = tmp_rank.current = tmp_rank.base = 0;
        tmp_rank.max = skill.getMaxRank();

        caobj.setProperty(skill.getName() + "_exp", tmp_exp);
        caobj.setProperty(skill.getName() + "_rank", tmp_rank);
        caobj.setProperty(skill.getName(), xp_use);
    }

    /**
     * This method creates stats based on the passed in ability, and assigns them to the player via the
     * created players' ClassAbilityObject.
     *
     * @param caobj
     * @param ability
     * @param xp_use
     */
    public static void createStats(ClassAbilityObject caobj, MarsAbility ability, Integer xp_use) {
        MarsStat tmp_exp = new MarsStat(ability.getName() + "_exp");
        tmp_exp.min = tmp_exp.current = tmp_exp.base = 0 ;
        tmp_exp.max = ability.getBaseExpThreshold(); // TODO: Replace this value with a modifiable one

        MarsStat tmp_rank = new MarsStat(ability.getName() + "_rank");
        tmp_rank.min = tmp_rank.current = tmp_rank.base = 0;
        tmp_rank.max = ability.getMaxRank(); // TODO: Replace this value with a modifiable one

        caobj.setProperty(ability.getName() + "_exp", tmp_exp);
        caobj.setProperty(ability.getName() + "_rank", tmp_rank);
        caobj.setProperty(ability.getName(), xp_use);
    }

    /**
     * Register the stat with the specific player, since only the player themselves have to be aware of what stats they
     * should be paying attention to.
     *
     * @param stat
     */
    public static void registerStat(MarsStatDef stat) {
        registerStat(stat, new String[0]);
    }

    public static void registerStat(MarsStatDef stat, String... dependencies) {
        String statName = stat.getName();
        if (!statDefMap.containsKey(statName)) {
            statDefMap.put(statName, stat);
            for (String depName : dependencies) {
                MarsStatDef depStat = statDefMap.get(depName);
                if (depStat != null) {
                    depStat.addDependent(stat);
                } else {
                    Log.error("no stat definition for dependency " + depName
                              + " of stat " + statName);
                }
            }
        }
    }

    public static void handleLevelIncrement(ClassAbilityObject cao) {

    }

    /**
     * This method allows registering a profession.
     *
     * @param profession
     */
    public static void registerProfession(ProfessionObject profession) {
        log.debug("Registering Profession: " + profession);
        professions.put(profession.getName(), profession);
    }

    public static MarsStatDef lookupStatDef(String name) {
        return statDefMap.get(name);
    }

    public static void sendSkillUpdate(CombatInfo info) {
        // extract the base information
        List<String> skilllist = info.getSkills();
        Long oid = info.getOwnerOid();

        //Setup message to send to client
        TargetedExtensionMessage updateMsg = new TargetedExtensionMessage(info.getOwnerOid());
        updateMsg.setExtensionType("mv.SKILL_UPDATE");

        ClassAbilityObject caobj = (ClassAbilityObject)EntityManager.getEntityByNamespace(oid, ClassAbilityClient.NAMESPACE);
        if (caobj == null)
            return;
        String playerclass = caobj.getPlayerClass();

        //Create xp and rank stats for any new skills
        for (String skillName : skilllist){

            log.debug("Adding skill to the player: " + oid + " skill: " + skillName);

            // do a couple of double checks..
            if (Mars.SkillManager.keySet().contains(skillName)){
                // okay it is a valid system skill, now make sure it is in our list
                // but first we need the player's object for classability

                if (playerclass == null){
                    // they didn't define a class for this character, so we can't track stats for them
                    log.warn("They didn't define a class type for this player...");
                    return;
                }
                log.debug(professions.get(playerclass).toString());
                // now use the player's class to get the list of skills avaiable
                if (professions.get(playerclass).hasSkill(skillName)){
                    if (caobj.getProperty(skillName + "_exp") == null){
                        // this is valid skill for tracking experience. So generate a marstat for this skill
                        createStats(caobj, Mars.SkillManager.get(skillName), professions.get(playerclass).getSkill(skillName).getExperiencePerUse());
                    }
                }

                //Add skill to update message - Skill Name and Current Rank
                HashMap<String,Serializable> skillInfo = new HashMap<String,Serializable> ();
                skillInfo.put("name",skillName);
                skillInfo.put("rank",((MarsStat)caobj.getProperty(skillName + "_rank")).current);
                updateMsg.setProperty("Skill_"+skillName, skillInfo);
            }
        }

        //Send update to the client on new skills
        Engine.getAgent().sendBroadcast(updateMsg);

    }

    class ClassAbilityStartAbilityHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            CombatClient.StartAbilityMessage abilityMsg = (CombatClient.StartAbilityMessage) msg;
            Long oid = abilityMsg.getSubject();
            String abilityName = abilityMsg.getAbilityName();

            log.debug("Processing Start Ability Message: " + oid + ", ability: " + abilityName);

            // Check to see the ability is already in the hashmap
            if (playerabilitykey.containsKey(abilityName)) {
                // put the player in if not in there
                if (!playerabilitykey.get(abilityName).contains(oid)) {
                    playerabilitykey.get(abilityName).add(oid);
                }
            }
            else {
                // need to add the ability and the player
                ArrayList<Long> a = new ArrayList<Long>();
                a.add(oid);
                playerabilitykey.put(abilityName, a);
            }

            log.debug("PlayerAbilityKey list for " + abilityName + " : [ " + playerabilitykey.get(abilityName).toString() + " ]");

            return true;
        }

    }

    class ClassAbilityAbilityProgressHook implements Hook{
        public boolean processMessage(Message msg, int flags) {
            // Convert the message, then get the pertinent information
            CombatClient.AbilityProgressMessage abilityMsg = (CombatClient.AbilityProgressMessage) msg;
            String abilityName = abilityMsg.getAbilityName();
            String state = abilityMsg.getState();

            log.debug("Processing Progress Ability Message: " + state + ":" + MarsAbility.ActivationState.COMPLETED +", ability: " + abilityName);

            if (state.equals(MarsAbility.ActivationState.COMPLETED.toString())) {
                // now we need to find a matching oid from our key list
                ArrayList<Long> oids = playerabilitykey.get(abilityName);
                if (oids == null){ return true; }       // there are no oids associated with this ability
                log.debug("Getting OIDS: [ " + oids.toString() + " ]");
                for( Long oid : oids) {
                    // now load the combat namespace for this oid so we can find out if the current state is completed or not
                    CombatInfo ci = (CombatInfo)EntityManager.getEntityByNamespace(oid, CombatClient.NAMESPACE);
                    log.debug("Checking the current action state for " + ci.getName() +  " ( " + ci.getOid() + " ) : " + ci.getCurrentAction());
                    if (ci.getCurrentAction() == null) {
                        // we found one that is completed so pull in the classabilityobject and complete the task
                        ClassAbilityObject caobj = (ClassAbilityObject)EntityManager.getEntityByNamespace(oid, ClassAbilityClient.NAMESPACE);
                        MarsAbility ability = professions.get(caobj.getPlayerClass()).getAbility(abilityName);

                        if (ability != null) { // this is not a valid profession ability for this player
                            //Get associated skill information
                            MarsSkill skill = ability.getRequiredSkill();
                            if(skill != null){
                                // Make sure the player's profession has this skill
                                if(professions.get(caobj.getPlayerClass()).getSkillMap().get(skill.getName()) != null)
                                    //Update skill
                                    caobj.updateBaseStat(skill.getName(), skill.getExperiencePerUse());
                            }
                                    // now increment the ability
                                    caobj.updateBaseStat(abilityName, ability.getExperiencePerUse());
                                }
                            }
                        }
                    }

            return true;
        }

    }

    class ClassAbilityLoadHook implements LoadHook {
        public void onLoad(Entity e) {

        }
    }

    class ClassAbilitySaveHook implements SaveHook {
        public void onSave(Entity e, Namespace namespace) {
        }
    }

    public static ClassAbilityObject getClassAbilityObject(Long oid) {
        log.debug("Checking the data for oid: " + oid);
        Entity entity = EntityManager.getEntityByNamespace(oid, Namespace.CLASSABILITY);
        log.debug("What is this entity type? " + entity.getType() + " and Name? " + entity.getName() + " and OID: " + entity.getOid());
        return (ClassAbilityObject)entity;
    }

    //Define combat related string constants
    public static final String KILL_EXP_STAT = "kill_exp";
    public static final String EXPERIENCE_STAT = "experience";
    public static final String LEVEL_STAT =  "level";

    public class ClassAbilityHandleXpHook implements Hook{
    	public boolean processMessage(Message msg, int flags) {
        ExtensionMessage xpUpdateMsg = (ExtensionMessage)msg;
        
        if(xpUpdateMsg.getProperty("attackers") != null){  
            
            CombatInfo target = CombatPlugin.getCombatInfo(xpUpdateMsg.getSubject());
            Set<Long> attackers = (HashSet<Long>)xpUpdateMsg.getProperty("attackers");
            handlePlayerXP(target, attackers);
        }
        
        return true;
    	}
    }
    
    /**
     * This function is used when a target dies to give the player XP from the kill.
     *
     * It is in the CombatAPI to allow for modification as systems are added to the combat.
     *
     * @param target Mob killed.
     * @param attackers Objects (oids) that attacked mob.
     */
    public static void handlePlayerXP(CombatInfo target, Set<Long> attackers) {
        if(target == null) {
            Log.error("CombatAPI.handlePlayerXP : target is null");
            return;
        }

        // pull the xp value from the target
        Integer xpval = (Integer)target.getProperty(KILL_EXP_STAT);

        if (xpval == null){ return; } // this target has no xp to gain

        List<Long> handledOids = Collections.emptyList();
        
        for(Long attackerOid : attackers){            
            CombatInfo attacker = CombatPlugin.getCombatInfo(attackerOid);
            
         // If the attacker is grouped then all group members should get xp also
            HashSet<Long> groupMembers = new HashSet<Long>();        
            if (attacker.isGrouped()){
                groupMembers = GroupClient.GetGroupMemberOIDs(attackerOid);
                                
                for(Long groupMemberOid : groupMembers){                    
                    // If the group member is in the attackers list or already handled list, 
                    //  then do not set their xp here
                    if(!attackers.contains(groupMemberOid) && !handledOids.contains(groupMemberOid)){
                        CombatInfo groupMember = CombatPlugin.getCombatInfo(groupMemberOid);
                        // now apply the value to the group member
                        groupMember.statModifyBaseValue(EXPERIENCE_STAT, xpval);
                        // Add to handled list to ensure the OID is not processed more than once
                        handledOids.add(groupMemberOid);
                        ClassAbilityClient.sendXPUpdate(groupMemberOid, EXPERIENCE_STAT, groupMember.statGetCurrentValue(EXPERIENCE_STAT));            
                    }
                }
            }

        // now apply the value to the attacker
        attacker.statModifyBaseValue(EXPERIENCE_STAT, xpval);
            ClassAbilityClient.sendXPUpdate(attackerOid, EXPERIENCE_STAT, attacker.statGetCurrentValue(EXPERIENCE_STAT));
        }        
    }

    /**
     * This method handles leveling the player profession based on the level that they have reached.
     *
     * Modifications are based on the leveling map for the profession in question as well as the stat's
     * own leveling map, if applicable.
     *
     * @param player
     * @param lvl
     */
    public static void handleLevelingPlayer(CombatInfo player, int lvl) {
        // first we need to get a hold of the players profession if it exists
        String profession = null;
        try {
            profession = (String)EnginePlugin.getObjectProperty(player.getOid(), Namespace.WORLD_MANAGER, "class");
        } catch(Exception e)  {

        }
        if (profession == null) {
            return;
        }
        ProfessionObject po = professions.get(profession);

        // now now get the leveling map for the profession
        LevelingMap lm = po.getLevelingMap();

        // now go through every proprety of the combatinfo and check to see if it is a mars stat or not
        for (String propname : player.getPropertyMap().keySet()) {
            // check to make sure we aren't dealing with the experience stat (as that controls leveling)
            // that this is indeed a base stat, and that it is also an instance of the MarsStat (in case something
            // went wrong and it was never created as a MarsStat).
            if (!propname.equals("experience") && po.isBaseStat(propname) && player.getProperty(propname) instanceof MarsStat) {
                // this is a mars stat so this is something maybe modified according to the profession map
                // do initial modification of full stats based on leveling map 0
                log.debug("Leveling up stat " + propname);
                MarsStat stat = (MarsStat)player.getProperty(propname);
                // this update call could be done when stats are finishing being setup. so bypass..
                log.debug("Leveling up " + propname + " : " + stat);
                int checker = stat.base; // setup a catch to see if we need to mark it as dirty or not at the end

                log.debug("Base " + propname + " base stat: " + stat.base);
                stat.base += (new Float((stat.base * lm.getLevelPercentageModification(0)))).intValue() + lm.getLevelFixedAmountModification(0);
                log.debug(propname + " base stat after global modification: " + stat.base);

                // check to see if there is a leveling map for this level
                if (lm.hasLevelModification(lvl)) {
                    // now apply it to the stats as well
                    stat.base += (new Float((stat.base * lm.getLevelPercentageModification(lvl)))).intValue() + lm.getLevelFixedAmountModification(lvl);
                    log.debug(propname + " base stat after level modification: " + stat.base);
                }

                // now to find out if this stat has it's own level map
                if (po.hasStatLevelModification(propname, lvl)) {
                    // we do have a level modification for this stat, so lets make it happen
                    LevelingMap statlm = po.getStatsLevelingMap(propname);
                    stat.base += (new Float((stat.base * statlm.getLevelPercentageModification(lvl)))).intValue() + statlm.getLevelFixedAmountModification(lvl);
                    log.debug(propname + " base stat after stat modification: " + stat.base);
                }

                log.debug(propname + " checking comparison: " + checker + " : " + stat.base);
                if (checker != stat.base) {
                    stat.current = stat.max = stat.base;
                    // the stat has changed value
                    player.setProperty(propname, stat);
                    stat.setDirty(true);
                    log.debug(propname + " updating base stat def");
                    CombatPlugin.getBaseStatDef(propname).update(stat, player);
                }
            }
        }
    }

    public static void handleSkillAbilityRanking(ClassAbilityObject player, String statname, int lvl) {
        LevelingMap lm;

        // In this case the skill/ability itself stores the leveling map so grab it from the stat.
        if (Mars.SkillManager.get(statname) != null){
            lm = Mars.SkillManager.get(statname).getLevelingMap();
        }
        else if (Mars.AbilityManager.get(statname) != null) {
            lm = Mars.AbilityManager.get(statname).getLevelingMap();
        }
        else {
            // Nothing was found, so do nothing..
            return;
        }

        // Now grab the stat that is to be modified
        MarsStat stat = (MarsStat)player.getProperty(statname + "_exp");

        // this update call could be done when stats are finishing being setup. so bypass..
        log.debug("Leveling up " + statname + " : " + stat);
        int checker = stat.max; // setup a catch to see if we need to mark it as dirty or not at the end

        log.debug("Max " + statname + " max stat: " + stat.max);
        stat.max += (new Float((stat.max * lm.getLevelPercentageModification(0)))).intValue() + lm.getLevelFixedAmountModification(0);
        log.debug(statname + " max stat after global modification: " + stat.max);

        // check to see if there is a leveling map for this level
        if (lm.hasLevelModification(lvl)) {
            // now apply it to the stats as well
            stat.max += (new Float((stat.max * lm.getLevelPercentageModification(lvl)))).intValue() + lm.getLevelFixedAmountModification(lvl);
            log.debug(statname + " max stat after level modification: " + stat.max);
        }

        log.debug(statname + " checking comparison: " + checker + " : " + stat.max);
        if (checker != stat.max) {
            // the stat has changed value
            player.setProperty(statname+"_exp", stat);
            stat.setDirty(true);
            log.debug(statname + " updating base stat def");
            ClassAbilityPlugin.lookupStatDef(statname+"_exp").update(stat, player);
        }
    }
}
