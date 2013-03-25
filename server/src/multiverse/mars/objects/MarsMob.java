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
import multiverse.mars.core.*;
import java.util.*;
import java.io.*;

public class MarsMob extends MarsObject {
    public MarsMob() {
        super();
        init();
    }
    
    public MarsMob(Long oid) {
        super(oid);
        init();
    }

    public MarsMob(String name) {
        super();
        init();
        setName(name);
    }

    public MarsMob(String name, Map<String, Serializable> propMap) {
        super();
        setName(name);
        setPropertyMap(propMap);
        init();
    }
    
    protected void init() {
        setType(ObjectTypes.mob);
        if (Log.loggingDebug)
            Log.debug("MarsMob.init: name=" + getName() + ", perceiver=" + perceiver());
        if (perceiver() == null) {
            if (Log.loggingDebug)
                Log.debug("MarsMob.init: generating perceiver");
            MobilePerceiver<WMWorldNode> p = new MobilePerceiver<WMWorldNode>(
                    (WMWorldNode) worldNode(), World.perceiverRadius);
            p.setFilter(new BasicPerceiverFilter());
            p.setRadius(World.perceiverRadius);
            perceiver(p);
            if (Log.loggingDebug)
                Log.debug("MarsMob.init: generated perceiver=" + p + ", func=" + perceiver());
        }
    }

    public void worldNode(WorldNode worldNode) {
        super.worldNode(worldNode);
        MobilePerceiver<WMWorldNode> p = perceiver();
        if (p != null) {
            ((WMWorldNode) worldNode).setPerceiver(p);
            p.setElement((WMWorldNode) worldNode);
        }
    }

    public static MarsMob convert(Entity obj) {
        if (!(obj instanceof MarsMob)) {
            throw new MVRuntimeException("MarsMob.convert: obj is not a marsmob: "
                    + obj);
        }
        return (MarsMob) obj;
    }

    // public MarsBehavior getMarsBehavior() {
    // return (MarsBehavior) getBehavior();
    // }

    /**
     * returns the item occupying the slot
     */
    public MarsItem getItemBySlot(MarsEquipSlot slot) {
        lock.lock();
        try {
            return equipMap.get(slot);
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns what slot the item is occupying
     */
    public MarsEquipSlot getSlotByItem(MarsItem item) {
        lock.lock();
        try {
            for (Map.Entry<MarsEquipSlot, MarsItem> entry : equipMap.entrySet()) {
                MarsEquipSlot slot = entry.getKey();
                MarsItem curItem = entry.getValue();
                if (MVObject.equals(item, curItem)) {
                    return slot;
                }
            }
            return null;
        } finally {
            lock.unlock();
        }
    }

    /**
     * sets up what slots are equippable
     */
    public void setEquipInfo(MarsEquipInfo equipInfo) {
        this.equipInfo = equipInfo;
        if (Log.loggingDebug)
            log.debug("setEquipInfo: mob=" + this + ", equipInfo="
                      + equipInfo);
    }

    public MarsEquipInfo getEquipInfo() {
        return equipInfo;
    }

    public List<MarsEquipSlot> getEquippableSlots() {
        if (equipInfo == null) {
            throw new MVRuntimeException("MarsMob.getEquippableSlots: equipinfo is null for mob " + this);
        }
        return equipInfo.getEquippableSlots();
    }

    MarsEquipInfo equipInfo = null;

    /**
     * returns all equipped items
     */
    public Set<MarsItem> getEquippedItems() {
        lock.lock();
        try {
            return new HashSet<MarsItem>(equipMap.values());
        } finally {
            lock.unlock();
        }
    }

    /**
     * places the item into the passed in equipment slot this is just a property
     * setting method and does not send any messages or perform any checks.
     */
    public void putItemIntoSlot(MarsEquipSlot slot, MarsItem item) {
        lock.lock();
        try {
            if (Log.loggingDebug)
                Log.debug("MarsObject: putting item " + item + " into equip slot "
                          + slot.getName() + " for obj " + this);
            if (!getEquippableSlots().contains(slot)) {
                log.error("mob " + this.getName() + ", item=" + item
                        + ", mob does not have this slot " + slot);
                throw new MVRuntimeException("mob does not have this slot");
            }
            equipMap.put(slot, item);
        } finally {
            lock.unlock();
        }
    }

    /**
     * removes the slot mapping in the map - does not send out any messages
     * returns the item which was in the slot or null if none was there
     */
    public MarsItem clearSlot(MarsEquipSlot slot) {
        lock.lock();
        try {
            return equipMap.remove(slot);
        } finally {
            lock.unlock();
        }
    }

    public void setEquipMap(Map<MarsEquipSlot, MarsItem> equipMap) {
        lock.lock();
        try {
            if (Log.loggingDebug)
                log.debug("setEquipMap: thismob=" + getName()
                          + ", new equipMap size=" + equipMap.size());
            if (equipMap == null) {
                throw new RuntimeException("equipMap is null");
            }
            this.equipMap = new HashMap<MarsEquipSlot, MarsItem>(equipMap);
        } finally {
            lock.unlock();
        }
    }

    public Map<MarsEquipSlot, MarsItem> getEquipMap() {
        lock.lock();
        try {
            return new HashMap<MarsEquipSlot, MarsItem>(equipMap);
        } finally {
            lock.unlock();
        }
    }

    protected Map<MarsEquipSlot, MarsItem> equipMap = new HashMap<MarsEquipSlot, MarsItem>();

    public int getOCV() {
        return (Math.round((float) getDexterity() / 3));
    }

    public int getDCV() {
        return (Math.round((float) getDexterity() / 3));
    }

    public int getCV() {
        return (Math.round((float) getDexterity() / 3));
    }

    public MarsObject getAutoAttackTarget() {
        return autoAttackTarget;
    }

    MarsObject autoAttackTarget = null;

    public long getLastRecTime() {
        lock.lock(); // long
        try {
            return lastRecTime;
        } finally {
            lock.unlock();
        }
    }

    public void setLastRecTime(long time) {
        lock.lock(); // long
        try {
            lastRecTime = time;
        } finally {
            lock.unlock();
        }
    }

    long lastRecTime = 0;

    public long getLastAttackTime() {
        lock.lock(); // long
        try {
            return lastAttackTime;
        } finally {
            lock.unlock();
        }
    }

    // sets time to now
    public void setLastAttackTime() {
        lock.lock();
        try {
            lastAttackTime = System.currentTimeMillis();
        } finally {
            lock.unlock();
        }
    }

    public long timeSinceLastAttack() {
        return System.currentTimeMillis() - getLastAttackTime();
    }

    long lastAttackTime = 0;

    public void setStrength(int str) {
        this.strength = str;
    }

    public int getStrength() {
        return strength;
    }

    public void modifyStrength(int delta) {
        lock.lock();
        try {
            int strength = getStrength();
            setStrength(strength + delta);
        } finally {
            lock.unlock();
        }
    }

    int strength = 0;

    public void setIntelligence(int intelligence) {
        this.intelligence = intelligence;
    }

    public int getIntelligence() {
        return intelligence;
    }

    public void modifyIntelligence(int delta) {
        lock.lock();
        try {
            int intelligence = getIntelligence();
            setIntelligence(intelligence + delta);
        } finally {
            lock.unlock();
        }
    }

    int intelligence = 0;

    public void setEgo(int ego) {
        this.ego = ego;
    }

    public int getEgo() {
        return ego;
    }

    public void modifyEgo(int delta) {
        lock.lock();
        try {
            int ego = getEgo();
            setEgo(ego + delta);
        } finally {
            lock.unlock();
        }
    }

    int ego = 0;

    public void setPresence(int pre) {
        this.presence = pre;
    }

    public int getPresence() {
        return presence;
    }

    public void modifyPresence(int delta) {
        lock.lock();
        try {
            int presence = getPresence();
            setPresence(presence + delta);
        } finally {
            lock.unlock();
        }
    }

    int presence = 0;

    public void setComeliness(int comeliness) {
        this.comeliness = comeliness;
    }

    public int getComeliness() {
        return comeliness;
    }

    public void modifyComeliness(int delta) {
        lock.lock();
        try {
            int comeliness = getComeliness();
            setComeliness(comeliness + delta);
        } finally {
            lock.unlock();
        }
    }

    int comeliness = 0;

    public void setDexterity(int dex) {
        this.dexterity = dex;
    }

    public int getDexterity() {
        return dexterity;
    }

    public void modifyDexterity(int delta) {
        lock.lock();
        try {
            int dexterity = getDexterity();
            setDexterity(dexterity + delta);
        } finally {
            lock.unlock();
        }
    }

    int dexterity = 0;

    public int getBaseRecovery() {
        return (getStrength() + getConstitution()) / 5;
    }

    public int getConstitution() {
        return constitution;
    }

    public void setConstitution(int con) {
        this.constitution = con;
    }

    public void modifyConstitution(int delta) {
        lock.lock();
        try {
            int constitution = getConstitution();
            setConstitution(constitution + delta);
        } finally {
            lock.unlock();
        }
    }

    int constitution = 0;

    public void setEndurance(int end) {
        this.endurance = end;
    }

    public int getEndurance() {
        return endurance;
    }

    public void modifyEndurance(int delta) {
        lock.lock();
        try {
            int endurace = getEndurance();
            setEndurance(endurace + delta);
        } finally {
            lock.unlock();
        }
    }

    int endurance = 0;

    public void setCurrentEndurance(int end) {
        this.currentEndurance = end;
    }

    public void modifyCurrentEndurance(int delta) {
        lock.lock();
        try {
            int end = getCurrentEndurance();
            setCurrentEndurance(end + delta);
        } finally {
            lock.unlock();
        }
    }

    public int getCurrentEndurance() {
        return currentEndurance;
    }

    int currentEndurance = 0;

    public void setPDBonus(int bonus) {
        pdBonus = bonus;
    }

    public int getPDBonus() {
        return pdBonus;
    }

    public void modifyPDBonus(int delta) {
        lock.lock();
        try {
            int pdBonus = getPDBonus();
            setPDBonus(pdBonus + delta);
        } finally {
            lock.unlock();
        }
    }

    int pdBonus = 0;

    public int getPD() {
        int rv = (Math.round((float) getStrength() / 5));
        lock.lock();
        try {
            return rv + getPDBonus();
        } finally {
            lock.unlock();
        }
    }

    public void setSpeedBonus(int bonus) {
        speedBonus = bonus;
    }

    public int getSpeedBonus() {
        return speedBonus;
    }

    public void modifySpeedBonus(int delta) {
        lock.lock();
        try {
            int speedBonus = getSpeedBonus();
            setSpeedBonus(speedBonus + delta);
        } finally {
            lock.unlock();
        }
    }

    int speedBonus = 0;

    public int getSpeed() {
        int rv = 10 + getDexterity();
        lock.lock();
        try {
            int bonus = getSpeedBonus();
            return rv + bonus;
        } finally {
            lock.unlock();
        }
    }

    public void setResistantPD(int pd) {
        resistPD = pd;
    }

    public int getResistantPD() {
        return resistPD;
    }

    public void modifyResistantPD(int delta) {
        lock.lock();
        try {
            int resistantPD = getResistantPD();
            setResistantPD(resistantPD + delta);
        } finally {
            lock.unlock();
        }
    }

    int resistPD = 0;

    public void setMaxMoveSpeed(int speed) {
        maxMoveSpeed = speed;
    }

    public int getMaxMoveSpeed() {
        return maxMoveSpeed;
    }

    private int maxMoveSpeed = 0;

    /**
     * this mob will give out the passed in quest to users this is not used for
     * storing which quests the player is doing
     */
    public void addQuestPrototype(MarsQuest quest) {
        lock.lock();
        try {
            questSet.add(quest);
        } finally {
            lock.unlock();
        }
    }

    /**
     * this mob is able to conclude the passed in quest for completion by the
     * player.
     */
    public void addConcludeQuest(MarsQuest quest) {
        lock.lock();
        try {
            if (quest == null) {
                throw new RuntimeException("quest is null");
            }
            concludeSet.add(quest);
        } finally {
            lock.unlock();
        }
    }

    // /**
    // * does this mob have a quest for the passed in user
    // */
    // public boolean hasQuestFor(MVObject obj) {
    // lock.lock();
    // try {
    // return (! questSet.isEmpty());
    // }
    // finally {
    // lock.unlock();
    // }
    // }

    /**
     * for now, they are ordered in their dependency copies the actual list, but
     * the references are the original this is for the quests this object is
     * GIVING out - not doing
     */
    public LinkedList<MarsQuest> getQuestPrototypes() {
        lock.lock();
        try {
            return new LinkedList<MarsQuest>(questSet);
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the set of quests that can be 'turned in' to this mob
     */
    public Set<MarsQuest> getConcludableQuests() {
        lock.lock();
        try {
            return new HashSet<MarsQuest>(concludeSet);
        } finally {
            lock.unlock();
        }
    }

    // collection of quest PROTOTYPES
    // for now they are ordered in their dependency
    Collection<MarsQuest> questSet = new LinkedList<MarsQuest>();

    // quests which this mob can act as the conclude npc
    // (who you go to when you are turning in the quest)
    Set<MarsQuest> concludeSet = new HashSet<MarsQuest>();

    //
    // section on user quests
    //
    public void addQuestState(QuestState questState) {
        lock.lock();
        try {
            questStateSet.add(questState);
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns a copy of the set of quest state objects for this user. the quest
     * state object themselves are the actual objects, changing them will change
     * the quest state
     */
    public Set<QuestState> getQuestStates() {
        lock.lock();
        try {
            return new HashSet<QuestState>(questStateSet);
        } finally {
            lock.unlock();
        }
    }

    public void setQuestStates(Set<QuestState> qs) {
        lock.lock();
        try {
            questStateSet = new HashSet<QuestState>(qs);
        } finally {
            lock.unlock();
        }
    }

    /**
     * finds the quest state associated with the passed in questId
     */
//    public QuestState findQuestState(Long questId) {
//        lock.lock();
//        try {
//            for (QuestState questState : questStateSet) {
//                if (questState.getQuestId().equals(questId)) {
//                    return questState;
//                }
//            }
//            return null;
//        } finally {
//            lock.unlock();
//        }
//    }

    /**
     * calls into the quest state to handle quest conclude
     */
    public boolean handleQuestConclude(Long questId) {
        // lock.lock();
        // try {
//        Log.debug("MarsMob.handleQuestConclude: questId=" + questId
//                + ", player=" + getName());
//        QuestState questState = findQuestState(questId);
//        if (questState == null) {
//            Log
//                    .debug("MarsMob.handleQuestConclude: could not find quest state for quest");
//            return false;
//        }
//        Log.debug("MarsMob.handleQuestConclude: found quest state");
//        boolean rv = questState.handleConclude();
//        Log
//                .debug("MarsMob.handleQuestConclude: questState.handleconclude returned "
//                        + rv);
//        return rv;
        // }
        // finally {
        // lock.unlock();
        // }
        return false;
    }

    Set<QuestState> questStateSet = new HashSet<QuestState>();

    //
    // skills
    //

    /**
     * adds the skill to the characters list of learned skills. it is added with
     * 0 xp
     */
    public void addSkill(MarsSkill skill) {
        lock.lock();
        try {
            skillMap.put(skill, 0);
        } finally {
            lock.unlock();
        }
    }

    public boolean hasSkill(MarsSkill skill) {
        lock.lock();
        try {
            // Iterator<MarsSkill> iter = skillMap.keyValues();
            // while (iter.hasNext()) {
            // MarsSkill s = iter.next();
            // if (s.equals(skill)) {
            // return true;
            // }
            // }
            // Log.debug("MarsMob.hasSkill: could not find matching skill");
            return skillMap.containsKey(skill);
        } finally {
            lock.unlock();
        }
    }

    public void setSkillMap(Map<MarsSkill, Integer> skillMap) {
        this.skillMap = new HashMap<MarsSkill, Integer>(skillMap);
    }

    /**
     * returns the amount of xp you have in the passed in skill
     */
    public int getXPforSkill(MarsSkill skill) {
        lock.lock();
        try {
            Integer xp = skillMap.get(skill);
            return (xp == null) ? 0 : xp;
        } finally {
            lock.unlock();
        }
    }

    public void addSkillXP(MarsSkill skill, int newXp) {
        lock.lock();
        try {
            Integer curXp = skillMap.get(skill);
            if (curXp == null) {
                log.warn("MarsMob.addSKillXp: mob " + this.getName()
                        + " does not have skill " + skill.getName());
                return;
            }
            skillMap.put(skill, (curXp + newXp));
        } finally {
            lock.unlock();
        }
    }

    public Map<MarsSkill, Integer> getSkillMap() {
        lock.lock();
        try {
            return new HashMap<MarsSkill, Integer>(skillMap);
        } finally {
            lock.unlock();
        }
    }

    // skill -> xp map
    private Map<MarsSkill, Integer> skillMap = new HashMap<MarsSkill, Integer>();

    /**
     * returns all the mobs that have done damage to this mob
     */
    public Set<MarsMob> getAttackers() {
        lock.lock();
        try {
            return new HashSet<MarsMob>(dmgTable.keySet());
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the skills an attacker used on this mob
     */
    public Set<MarsSkill> getAttackerSkills(MarsMob attacker) {
        lock.lock();
        try {
            Map<MarsSkill, Integer> attackerDmgMap = dmgTable.get(attacker);
            return new HashSet<MarsSkill>(attackerDmgMap.keySet());
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the amount of dmg the attacker has done using skill 'skill'
     */
    public int getDmgForSkill(MarsMob attacker, MarsSkill skill) {
        lock.lock();
        try {
            Map<MarsSkill, Integer> attackerDmgMap = dmgTable.get(attacker);
            if (attackerDmgMap == null) {
                return 0;
            }
            Integer dmg = attackerDmgMap.get(skill);
            if (dmg == null) {
                return 0;
            }
            return dmg;
        } finally {
            lock.unlock();
        }
    }

    /**
     * record that a some other mob has done damage to this mob, so that when
     * this mob dies, the appropriate xp is rewarded
     */
    public void addDamage(MarsMob attacker, MarsSkill skill, int dmg) {
        lock.lock();
        try {
            // get the map of how much damage this particular
            // attacker has done with his various skills
            Map<MarsSkill, Integer> attackerDmgMap = dmgTable.get(attacker);
            if (attackerDmgMap == null) {
                attackerDmgMap = new HashMap<MarsSkill, Integer>();
            }

            // find out how much dmg attacker has done with the skill so far
            Integer curDmg = attackerDmgMap.get(skill);
            if (curDmg == null) {
                curDmg = 0;
            }
            attackerDmgMap.put(skill, new Integer(curDmg + dmg));
            dmgTable.put(attacker, attackerDmgMap);

            if (Log.loggingDebug)
                log.debug("addDamage: attacker=" + attacker.getName() + ", skill="
                          + skill.getName() + ", prevDmg=" + curDmg + ", newDmg="
                          + dmg + ", newTotal=" + (curDmg + dmg));

            totalDmgTaken += dmg;
        } finally {
            lock.unlock();
        }
    }

    public int getDamageTaken() {
        return totalDmgTaken;
    }

    // dmg that others have done on this mob
    // attacker -> skilltype -> dmg
    Map<MarsMob, Map<MarsSkill, Integer>> dmgTable = new HashMap<MarsMob, Map<MarsSkill, Integer>>();

    private int totalDmgTaken = 0;

    public MarsAbility.State getCurrentAbility() {
        return currentAbility;
    }

    public void setCurrentAbility(MarsAbility.State state) {
        currentAbility = state;
    }

    protected MarsAbility.State currentAbility = null;

    public Set<MarsAbility.State> getActiveAbilities() {
        return new HashSet<MarsAbility.State>(activeAbilities);
    }

    protected void setActiveAbilities(Set<MarsAbility.State> abilities) {
        activeAbilities = new HashSet<MarsAbility.State>(abilities);
    }

    public void addActiveAbility(MarsAbility.State state) {
        activeAbilities.add(state);
    }

    public void removeActiveAbility(MarsAbility.State state) {
        activeAbilities.remove(state);
    }

    protected Set<MarsAbility.State> activeAbilities = new HashSet<MarsAbility.State>();

    private static final long serialVersionUID = 1L;
}
