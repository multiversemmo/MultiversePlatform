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

import java.util.*;

import multiverse.server.objects.Entity;
import multiverse.server.util.*;

public class MarsStatDef {
    public MarsStatDef(String name) {
	this.name = name;
    }

    public String getName() { return name; }
    protected String name;

    public void addDependent(MarsStatDef stat) {
	dependents.add(stat);
    }
    protected Set<MarsStatDef> dependents = new HashSet<MarsStatDef>();

    public void update(MarsStat stat, Entity info) {
	if ((stat.min != null) && (stat.base <= stat.min)) {
	    stat.base = stat.min;
	}
	if ((stat.max != null) && (stat.base >= stat.max)) {
	    stat.base = stat.max;
	}
	stat.applyMods();
	if ((stat.min != null) && (stat.current <= stat.min)) {
	    stat.current = stat.min;
	}
	if ((stat.max != null) && (stat.current >= stat.max)) {
	    stat.current = stat.max;
	}

	int oldFlags = stat.flags;
	stat.flags = stat.computeFlags();

	for (MarsStatDef statDef : dependents) {
	    MarsStat depStat = (MarsStat)info.getProperty(statDef.name);
	    if (depStat != null) {
		Log.debug("MarsStatDef.update: stat=" + name + " updating dependent stat="
			  + statDef.getName());
		statDef.update(depStat, info);
	    }
	}

	notifyFlags(stat, info, oldFlags, stat.flags);

    }
    public void notifyFlags(MarsStat stat, Entity info, int oldFlags, int newFlags) {
    }

    public final static int MARS_STAT_FLAG_MIN = 1;
    public final static int MARS_STAT_FLAG_MAX = 2;
}
