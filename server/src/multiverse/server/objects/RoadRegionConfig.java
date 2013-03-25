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

package multiverse.server.objects;

import java.io.Serializable;
import java.util.concurrent.locks.*;
import multiverse.server.util.*;
import java.util.*;

public class RoadRegionConfig extends RegionConfig implements Serializable {
    public RoadRegionConfig() {
        setType(RoadRegionConfig.RegionType);
    }

    public String toString() {
        return "[RoadConfig]";
    }

    public void addRoad(Road road) {
        lock.lock();
        try {
            roadSet.add(road);
        }
        finally {
            lock.unlock();
        }
    }
    public Set<Road> getRoads() {
        lock.lock();
        try {
            return new HashSet<Road>(roadSet);
        }
        finally {
            lock.unlock();
        }
    }
    
    transient Lock lock = LockFactory.makeLock("RoadRegionLock");
    Set<Road> roadSet = new HashSet<Road>();
    
    public static String RegionType = (String)Entity.registerTransientPropertyKey("RoadRegion");

    private static final long serialVersionUID = 1L;
}
