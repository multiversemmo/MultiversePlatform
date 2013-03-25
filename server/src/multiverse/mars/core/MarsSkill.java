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

import multiverse.mars.objects.LevelingMap;
import multiverse.server.util.*;
import java.io.*;

public class MarsSkill implements Serializable {

    public static MarsSkill NullSkill = 
        new MarsSkill("NullSkill");

    public MarsSkill() {
    }

    public MarsSkill(String name) {
        setName(name);
    }

    public String toString() {
        return "[MarsSkill: " + getName() + "]";
    }

    public boolean equals(Object other) {
        MarsSkill otherSkill = (MarsSkill) other;
        boolean val = getName().equals(otherSkill.getName());
        return val;
    }

    public int hashCode() {
        return getName().hashCode();
    }

    public void setName(String name) {
        this.name = name;
    }
    public String getName() {
        return name;
    }
    String name = null;

    public void setSkillCostMultiplier(int c) {
        skillCost = c;
    }
    public int getSkillCostMultiplier() {
        return skillCost;
    }
    int skillCost = 1;

    /**
     * each level costs 1000 xp
     */
    public void setLevelCostMultiplier(int c) {
        levelCost = c;
    }
    public int getLevelCostMultiplier() {
        return levelCost;
    }
    int levelCost = 1000;

    /**
     * returns the amount of total xp required to be at level 'skillLevel'
     * for this skill
     */
    public int xpRequired(int level) {
        return (level * (level + 1)) / 2 * levelCost * skillCost;
    }

    /**
     * returns the level you have in this skill if you have the xp passed in
     */
    public int getLevel(int xp) {
        // do real math here sometime
        int i=0;
        while (xpRequired(i+1) < xp) {
            i++;
        }
        if (Log.loggingDebug)
            Log.debug("MarsSkill.getLevel: skill=" + getName() + 
                      ", level=" + i);
        return i;
    }

    String defaultAbility = null;
    int exp_per_use = 0;
    LevelingMap lm = new LevelingMap();
    int exp_max = 100;
    int rank_max = 3;

    public void setDefaultAbility(String ability) {
        defaultAbility = ability;
    }

    public String getDefaultAbility() {
        return defaultAbility;
    }
    
     /**
     * -Experience system component-
     * 
     * Returns the amount of experience to be gained after a successful use of
     * this skill.
     */
    public int getExperiencePerUse() {
        return exp_per_use;
    }

    /**
     * -Experience system component-
     * 
     * Sets the amount of experience that should be gained after a successful
     * use of this skill.
     * <p>
     * NOTE: Skill increases are meant to be minimal since there will generally
     * be many abilities increasing the skill level.
     */
    public void setExperiencePerUse(int xp) {
        exp_per_use = xp;
    }

    public void setLevelingMap(LevelingMap lm) {
        this.lm = lm;
    }

    public LevelingMap getLevelingMap() {
        return this.lm;
    }

    /**
     * -Experience system component-
     * 
     * Returns the default max experience required before increasing this skills
     * level.
     */
    public int getBaseExpThreshold() {
        return exp_max;
    }

    /**
     * -Experience system component-
     * 
     * Sets the default max experience required to increase the skills level.
     */
    public void setBaseExpThreshold(int max) {
        exp_max = max;
    }

    /**
     * -Experience system component-
     * 
     * Returns the max possible rank for this skill.
     */
    public int getMaxRank() {
        return rank_max;
    }

    /**
     * -Experience system component-
     * 
     * Sets the max possible rank for this skill.
     */
    public void setMaxRank(int rank) {
        rank_max = rank;
    }


    private static final long serialVersionUID = 1L;
}
