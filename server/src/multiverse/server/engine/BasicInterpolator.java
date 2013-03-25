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

package multiverse.server.engine;

import java.util.*;
import java.util.concurrent.*;
import multiverse.server.math.*;
import multiverse.server.util.*;
import multiverse.server.pathing.*;


public class BasicInterpolator implements Interpolator<BasicInterpolatable>, Runnable {
    public BasicInterpolator() {
    }

    public BasicInterpolator(int updateInterval) {
	startUpdates(updateInterval);
    }

    // Interpolator
    public synchronized void register(BasicInterpolatable obj) {
	interpSet.add(obj);
    }
    public synchronized void unregister(BasicInterpolatable obj) {
	interpSet.remove(obj);
    }

    public void interpolate(BasicInterpolatable obj) {
	long time = System.currentTimeMillis();
        long lastInterp = obj.getLastInterp();
	long timeDelta = time - lastInterp;

	if (timeDelta < 100) {
	    return;
	}

	PathInterpolator pathInterpolator = obj.getPathInterpolator();
        PathLocAndDir locAndDir = null;
        Point interpLoc = null;
        MVVector dir;
        Quaternion orient;
        if (pathInterpolator != null) {
            Log.debug("BasicInterpolator.interpolate calling pathInterpolator");
            locAndDir = pathInterpolator.interpolate(time);
            if (locAndDir == null) {
                dir = new MVVector(0,0,0);
                Point p = pathInterpolator.getLastPoint();
                if (p != null)
                    interpLoc = p;
            }
            else {
                interpLoc = locAndDir.getLoc();
                dir = locAndDir.getDir();
                if (Log.loggingDebug)
                    Log.debug("BasicInterpolator.interpolate pathInterpolator returned loc = " + interpLoc);
            }
        }
        else {
            dir = obj.getDir();
            if (dir == null || dir.isZero()) {
		obj.setLastInterp(time);
                return;
            }
            interpLoc = obj.getInterpLoc();
            if (interpLoc == null) {
                return;
            }
            MVVector dirCopy = new MVVector(dir);
            dirCopy.scale((float)(timeDelta/1000.0));
            interpLoc.add((int)dirCopy.getX(), (int)dirCopy.getY(), (int)dirCopy.getZ());
        }
        MVVector ndir = new MVVector(dir.getX(), 0, dir.getZ());
        float length = ndir.length();
        if (length != 0) {
            // normalize the vector then set the speed
            ndir.normalize();
            orient = Quaternion.fromVectorRotation(new MVVector(0,0,1), ndir);
        }
        else
            orient = Quaternion.Identity;
	// Finally, set all the values
        obj.setPathInterpolatorValues(time, dir, interpLoc, orient);
    }

    public void startUpdates(int interval) {
        if (Log.loggingDebug)
            Log.debug("BasicInterpolator.startUpdates: updating with interval=" + interval);
	Engine.getExecutor().scheduleAtFixedRate(this, interval, interval,
						 TimeUnit.MILLISECONDS);
    }

    public void run() {
        if (Log.loggingDebug)
            Log.debug("BasicInterpolator.run: interpolating all objects");
        HashSet<BasicInterpolatable> objects;
        synchronized (this) {
            objects = (HashSet<BasicInterpolatable>) interpSet.clone();
        }
	for (BasicInterpolatable obj : objects) {
	    interpolate(obj);
	}
    }

    transient HashSet<BasicInterpolatable> interpSet =
        new HashSet<BasicInterpolatable>();
}
