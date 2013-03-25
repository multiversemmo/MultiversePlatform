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

import java.io.*;
import java.util.*;
import multiverse.server.util.*;

public class MarsStat implements Serializable {
    protected String name;

    public MarsStat() {
    }

    public MarsStat(String statName) {
	name = statName;
    }

    public MarsStat(String statName, int value) {
	name = statName;
	base = current = max = value;
    }

    public MarsStat(String statName, int min, int max){
    	name = statName;
    	this.min = min;
    	this.max = max;
    	
    	base = current = min;
    }
    
    public MarsStat(String statName, int min, int max, boolean reverse){
    	name = statName;
    	this.min = min;
    	this.max = max;
    	
    	if (reverse){
    		base = current = max;
    	} else {
    		base = current = min;
    	}
    }

    public Integer getMin() { return min; }
    public void setMin(Integer min) { this.min = min; }
    public Integer min;

    public Integer getMax() { return max; }
    public void setMax(Integer max) { this.max = max; }
    public Integer max;

    public Integer getBase() { return base; }
    public void setBase(Integer base) { this.base = base; }
    public Integer base;

    public Integer getCurrent() { return current; }
    public void setCurrent(Integer current) { this.current = current; }
    public Integer current;

    public Map<Object, Integer> getModifiers() { return modifiers; }
    public void setModifiers(Map<Object, Integer> modifiers) { this.modifiers = modifiers; }
    Map<Object, Integer> modifiers = new HashMap<Object, Integer>();

    transient boolean dirty = false;

    public int getFlags() { return flags; }
    public void setFlags(int flags) { this.flags = flags; }
    int flags = 0;

    public String getName() {
	return name;
    }
    public void setName(String name) {
	this.name = name;
    }

    public void modifyBaseValue(int delta) {
	base += delta;
	if ((min != null) && (base < min)) {
	    base = min;
	}
	if ((max != null) && (base > max)) {
	    base = max;
	}
	applyMods();
	setDirty(true);
    }
    public void setBaseValue(int value) {
	base = value;
	if ((min != null) && (base < min)) {
	    base = min;
	}
	if ((max != null) && (base > max)) {
	    base = max;
	}
	applyMods();
	setDirty(true);
    }
    public void addModifier(Object id, int delta) {
	modifiers.put(id, delta);
	applyMods();
	setDirty(true);
    }
    public void removeModifier(Object id) {
	modifiers.remove(id);
	applyMods();
	setDirty(true);
    }


    public int getCurrentValue() {
	Log.debug("MarsStat.getCurrentValue: stat=" + name + " value=" + current);
	if(current == null)
		return 0;
	return current;
    }

    public int getBaseValue() {
	return base;
    }

    public void setDirty(boolean dirty) {
	this.dirty = dirty;
    }
    public boolean isDirty() {
	return dirty;
    }

    protected void applyMods() {
	//int newFlags = flags & ~(MarsStatDef.MARS_STAT_FLAG_MIN | MarsStatDef.MARS_STAT_FLAG_MAX);
	current = base;
	for (Integer mod : modifiers.values()) {
	    current += mod;
	}
	if ((min != null) && (current <= min)) {
	    current = min;
	}
	if ((max != null) && (current >= max)) {
	    current = max;
	}
    }

    protected int computeFlags() {
	int newFlags = 0;
	if ((min != null) && (current == min)) {
	    newFlags |= MarsStatDef.MARS_STAT_FLAG_MIN;
	}
	if ((max != null) && (current == max)) {
	    newFlags |= MarsStatDef.MARS_STAT_FLAG_MAX;
	}
	return newFlags;
    }
    
    public boolean isSet(){
    	if (current == null){ return false; } else { return true; }
    }
    
    private static final long serialVersionUID = 1L;
}
