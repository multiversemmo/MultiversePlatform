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

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Set;

import multiverse.mars.core.Mars;
import multiverse.mars.core.MarsAbility;
import multiverse.mars.core.MarsSkill;
import multiverse.server.util.Log;

/*
 * The ProfessionObject is setup to
 */
public class ProfessionObject {

    HashMap<String, MarsAbility> classabilities = new HashMap<String, MarsAbility>();
    HashMap<String, MarsAbility> defaultabilities = new HashMap<String, MarsAbility>();
    HashMap<String, MarsSkill> classskills = new HashMap<String, MarsSkill>();
    HashMap<String, MarsSkill> defaultskills = new HashMap<String, MarsSkill>();

    LevelingMap lm = new LevelingMap();

    HashMap<String, LevelingMap> statslm = new HashMap<String, LevelingMap>();

    ArrayList<String> basestats = new ArrayList<String>();

    String name;

    public ProfessionObject(String name){
        setName(name);
    }

    /**
     * This method allows you to pass an already build ability map for this profession.
     *
     * NOTE: This will override the existing ability map.
     *
     * @param abilityMap
     */
    public void addAbilityMap(HashMap<String, MarsAbility> abilityMap) {
        this.classabilities = abilityMap;
    }

    /**
     * This method provides the ability to pass an already built default abilities map for this profession.
     *
     * NOTE: This will override the existing ability map.
     *
     * @param defaultmap
     */
    public void addDefaultAbilityMap(HashMap<String, MarsAbility> defaultmap) {
        this.defaultabilities = defaultmap;
    }

    /**
     * This method provides a shortcut for passing ability map as well as default ability map to the profession.
     *
     * NOTE: This will override the existing ability maps.
     *
     * @param abilityMap
     * @param defaultMap
     */
    public void addAbilityMaps(HashMap<String, MarsAbility> abilityMap, HashMap<String, MarsAbility> defaultMap){
        addAbilityMap(abilityMap);
        addDefaultAbilityMap(defaultMap);
    }

    /**
     * Adds an ability to this profession's list of abilitlies as well as add to default if needed.
     *
     * @param abilityName
     * @param isdefault
     */
    public void addAbility(String abilityName, boolean isdefault) {
        Log.debug("Adding ability to profession object: " + abilityName + " : " + Mars.AbilityManager.get(abilityName));
        if (Mars.AbilityManager.get(abilityName) != null) {
            this.classabilities.put(abilityName, Mars.AbilityManager.get(abilityName));

            if (isdefault) {
                this.defaultabilities.put(abilityName, Mars.AbilityManager.get(abilityName));
            }
        }
    }

    public void addAbility(String abilityName) {
        addAbility(abilityName, false);
    }

    /**
     * Remove the ability from the abilities list and the default list.
     */
    public boolean removeAbility(String abilityName) {
        if (this.classabilities.get(abilityName) == null) { return false; }
        if (this.defaultabilities.get(abilityName) != null) {
            this.defaultabilities.remove(abilityName);
        }
        this.classabilities.remove(abilityName);
        return true;
    }

    /**
     * Method for finding out if ability is available.
     */
    public boolean hasAbility(String abilityName) {
        if (this.classabilities.containsKey(abilityName)) {
            return true;
        }
        return false;
    }

    /**
     * Retrieves ability.
     *
     * NOTE: This does not check the validity of the request.
     */
    public MarsAbility getAbility(String abilityName) {
        return classabilities.get(abilityName);
    }

    /**
     * Retrieves ability map.
     *
     * NOTE: This does not check the validity of the request.
     */
    public HashMap<String, MarsAbility> getAbilityMap() {
        return classabilities;
    }

    /**
     * Retrieves the default ability map.
     *
     * NOTE: This does not check the validity of the request.
     */
    public HashMap<String, MarsAbility> getDefaultAbilityMap() {
        return defaultabilities;
    }

    /**
     * This method allows passing an already built skillmap to the profession.
     *
     * NOTE: All existing skills in the skill map will be removed.
     */
    public void addSkillMap(HashMap<String, MarsSkill> skillMap) {
        this.classskills = skillMap;
    }

    /**
     * This method allows passing an already built default skill map for the profession.
     *
     * NOTE: All existing skills in this skill map will be removed.
     */
    public void addDefaultSkillMap(HashMap<String, MarsSkill> defaultSkillMap) {
        this.defaultskills = defaultSkillMap;
    }

    /**
     * This method allows passing already built skill maps in for the profession.
     *
     * NOTE: All existing skill maps will be removed.
     */
    public void addSkillMaps(HashMap<String, MarsSkill> skillMap, HashMap<String, MarsSkill> defaultSkillMap) {
        addSkillMap(skillMap);
        addDefaultSkillMap(defaultSkillMap);
    }

    /**
     * Adds a skill to this profession, and places it into default if necessary.
     */
    public void addSkill(String skillName, boolean isdefault) {
        Log.debug("Adding skill to profession object: " + skillName + " : " + Mars.SkillManager.get(skillName));
        if (Mars.SkillManager.get(skillName) != null) {
            this.classskills.put(skillName, Mars.SkillManager.get(skillName));

            if (isdefault) {
                this.defaultskills.put(skillName, Mars.SkillManager.get(skillName));
            }
        }
    }

    public void addSkill(String skillName) {
        addSkill(skillName, false);
    }

    /**
     * This method is for removing a skill from this profession.
     */
    public boolean removeSkill(String skillName) {
        if (this.classskills.get(skillName) == null) { return false; }
        if (this.defaultskills.get(skillName) != null) {
            this.defaultskills.remove(skillName);
        }

        this.classskills.remove(skillName);
        return true;
    }

    /**
     * Returns whether this profession has this skill in it's list.
     */
    public boolean hasSkill(String skillName) {
        if (this.classskills.containsKey(skillName)) { return true; }
        return false;
    }

    /**
     * Retrieves the skill.
     *
     * NOTE: This does not check the validity of the request.
     */
    public MarsSkill getSkill(String skillName) {
        return classskills.get(skillName);
    }

    /**
     * Retrieves entire skill map.
     *
     * NOTE: This does not check the validity of the request.
     */
    public HashMap<String, MarsSkill> getSkillMap() {
        return classskills;
    }

    /**
     * Retrieves the default skill map.
     *
     * NOTE: This does not check the validity of the request.
     */
    public HashMap<String, MarsSkill> getDefaultSkillMap() {
        return defaultskills;
    }

    /**
     * Sets the profession name.
     */
    public void setName(String name) {
        this.name = name;
    }

    /**
     * Returns the profession name.
     */
    public String getName() {
        return this.name;
    }

    /**
     * Method overriding java's base object toString method.
     */
    public String toString() {
        String str = "";

        str += "[ ProfessionObject: " + getName();
        str += ", Abilities: " + this.classabilities.toString();
        str += ", Default Abilities: " + this.defaultabilities.toString();
        str += ", Skills: " + this.classskills.toString();
        str += ", Default Skills: " + this.defaultskills.toString();
        str += ", Base Stats: " + this.basestats;
        str += ", " + this.lm;
        str += ", Stat Level Maps: ";
        Set<String> keys = statslm.keySet();
        for (String s : keys) {
            str += " " + s + " : " + statslm.get(s).toString();
        }
        str += " ]";

        return str;
    }


    public void applyLevelingMap(LevelingMap lm) {
        this.lm = lm;
    }

    public LevelingMap getLevelingMap() {
        return lm;
    }

    public void applyStatsLevelingMap(String statname, LevelingMap lm) {
        statslm.put(statname.toLowerCase(), lm);
    }

    public LevelingMap getStatsLevelingMap(String statname) {
        return statslm.get(statname.toLowerCase());
    }

    public boolean hasStatLevelModification(String statname, int lvl) {
        if (statslm.containsKey(statname.toLowerCase())) {
            LevelingMap tmp = statslm.get(statname.toLowerCase());
            return tmp.hasLevelModification(lvl);
        }
        return false;
    }

    public void addBaseStat(String statname) {
        if (!basestats.contains(statname.toLowerCase())) {
            basestats.add(statname.toLowerCase());
        }
    }

    public boolean isBaseStat(String statname) {
        return basestats.contains(statname.toLowerCase());
    }
}
