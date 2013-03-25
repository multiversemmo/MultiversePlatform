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

import java.util.HashMap;
import java.util.Set;

public class LevelingMap {
    HashMap<Integer, LevelModification> leveling = new HashMap<Integer, LevelModification>();

    public LevelingMap() {
        leveling.put(0, new LevelModification());
    }

    public void setAllLevelPercentageModification(float percentage) {
        LevelModification lvl = leveling.get(0);
        lvl.setPercentage(percentage);
    }

    public void setAllLevelFixedAmountModification(int fixed) {
        LevelModification lvl = leveling.get(0);
        lvl.setFixedAmount(fixed);
    }

    public void setAllLevelModification(float percentage, int fixed) {
        LevelModification lvl = leveling.get(0);
        lvl.setFixedAmount(fixed);
        lvl.setPercentage(percentage);
    }

    public Float getAllLevelPercentageModification() {
        LevelModification lvl = leveling.get(0);
        return lvl.getPercentage();
    }

    public Integer getAllLevelFixedAmountModification() {
        LevelModification lvl = leveling.get(0);
        return lvl.getFixedAmount();
    }

    public LevelModification getAllLevelModification() {
        return leveling.get(0);
    }

    public void setLevelPercentageModification(int lvl, float percentage) {
        // Do some logic to make sure level exists
        if(leveling.containsKey(lvl)) {
            // the level exists so grab it and modify it
            LevelModification lm = leveling.get(lvl);
            lm.setPercentage(percentage);
        }
        else {
            LevelModification lm = new LevelModification(percentage);
            leveling.put(lvl, lm);
        }
    }

    public void setLevelFixedAmountModification(int lvl, int fixed) {
        // Do some logic to make sure level exists
        if(leveling.containsKey(lvl)) {
            // the level exists so grab it and modify it
            LevelModification lm = leveling.get(lvl);
            lm.setFixedAmount(fixed);
        }
        else {
            LevelModification lm = new LevelModification(fixed);
            leveling.put(lvl, lm);
        }
    }

    public void setLevelModification(int lvl, float percentage, int fixed) {
        if(leveling.containsKey(lvl)) {
            LevelModification lm = leveling.get(lvl);
            lm.setFixedAmount(fixed);
            lm.setPercentage(percentage);
        }
        else {
            LevelModification lm = new LevelModification(percentage, fixed);
            leveling.put(lvl, lm);
        }
    }

    public Float getLevelPercentageModification(int lvl) {
        if (leveling.containsKey(lvl)) {
            return leveling.get(lvl).getPercentage();
        }
        else {
            return 0F;
        }
    }

    public Integer getLevelFixedAmountModification(int lvl) {
        if (leveling.containsKey(lvl)) {
            return leveling.get(lvl).getFixedAmount();
        }
        else {
            return 0;
        }
    }

    public LevelModification getLevelModification(int lvl) {
        if (leveling.containsKey(lvl)) {
            return leveling.get(lvl);
        }
        else {
            return null;
        }
    }

    public boolean hasLevelModification(int lvl) {
        // First see if the level exists, then check to see if either value is greater than 0
        if (leveling.containsKey(lvl)) {
            LevelModification lvlm = leveling.get(lvl);
            if (lvlm.getFixedAmount() > 0 || lvlm.getPercentage() > 0) {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }

    public class LevelModification {
        float percentage = 0;
        int fixed = 0;

        LevelModification() {
        }

        LevelModification(float percentage) {
            setPercentage(percentage);
        }

        LevelModification(int fixed) {
            setFixedAmount(fixed);
        }

        LevelModification(float percentage, int fixed) {
            setPercentage(percentage);
            setFixedAmount(fixed);
        }

        void setPercentage(float percentage) {
            this.percentage = percentage;
        }

        Float getPercentage() {
            return percentage;
        }

        void setFixedAmount(int fixed) {
            this.fixed = fixed;
        }

        Integer getFixedAmount() {
            return fixed;
        }
    }

    public String toString() {
        String s = "Leveling Map { ";

        Set<Integer> keys = leveling.keySet();
        for (Integer i : keys) {
            LevelModification lm = leveling.get(i);
            s += " [ " + lm.getPercentage() + ", " + lm.getFixedAmount() + " ] ";
        }

        return (s + "}");
    }
}
