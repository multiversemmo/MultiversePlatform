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

package multiverse.server.pathing;

import java.util.*;
import multiverse.server.math.*;
import multiverse.server.plugins.*;
import multiverse.server.util.*;

/**
 * defines a the "state" of a path, for a given entity, specified by
 * an Oid, a starting point and a destination.
 */
public class PathState {

    public PathState(long oid, String type, boolean linear) {
        this.oid = oid;
        this.type = type;
        this.linear = linear;
        clear();
    }
    
    public PathState(long oid, long startTime, String type, Point startLoc, Point endLoc, float speed) {
        this.oid = oid;
        this.startTime = startTime;
        this.type = type;
        this.startLoc = startLoc;
        this.endLoc = endLoc;
        this.speed = speed;
        this.path = null;
        this.pathInterpolator = null;
    }
    
    public void clear() {
        if (Log.loggingDebug)
            log.debug("clear: oid = " + oid);
        this.startTime = 0;
        this.startLoc = null;
        this.endLoc = null;
        this.speed = 0f;
        this.path = null;
        this.pathInterpolator = null;
    }

    // If we need to send a new mob path request message, that new
    // message is returned and the caller should send it.
    public WorldManagerClient.MobPathReqMessage setupPathInterpolator(long timeNow, Point newStartLoc, Point newEndLoc, 
                                                                      float newSpeed, boolean following, boolean followsTerrain) {
        startTime = timeNow;
        startLoc = newStartLoc;
        endLoc = newEndLoc;
        speed = newSpeed;
        PathFinderValue value = PathSearcher.findPath(type, new MVVector(startLoc), new MVVector(endLoc), followsTerrain);
        PathSearcher.PathResult result = value.getResult();
        List<MVVector> floatingPath = value.getPath();
        path = new LinkedList<Point>();
        for (MVVector pathPoint : floatingPath)
            path.add(new Point(pathPoint));
        int count = path.size();
        // If we're following, shorten the last part of the path
        // by the max of 2 meters and .1 meter
        if (following && count >= 2) {
            Point p1 = path.get(count - 2);
            Point p2 = path.get(count - 1);
            float len = Point.distanceTo(p1, p2);
            float newLen = Math.max(100f, len - 2000f);
            MVVector newp2 = new MVVector(p2);
            newp2.sub(p1);
            newp2.setY(0f);
            newp2.normalize();
            newp2.multiply(newLen);
            newp2.add(p1);
            path.set(count - 1, new Point(newp2));
        }
        String terrainString = value.getTerrainString();
        if (Log.loggingDebug)
            log.debug("setupPathInterpolator: findPath result = " + result.toString() + "; path.size() = " + path.size() + 
                      "; terrainString = " + terrainString);
        if (result == PathSearcher.PathResult.OK && path.size() >= 2) {
            if (linear)
                pathInterpolator = new PathLinear(oid, timeNow, speed, terrainString, path);
            else
                pathInterpolator = new PathSpline(oid, timeNow, speed, terrainString, path);
            // Send the MobPathReqMessage
            WorldManagerClient.MobPathReqMessage reqMsg = new WorldManagerClient.MobPathReqMessage
                (oid, startTime, linear ? "linear" : "spline", speed, terrainString, path);
            if (Log.loggingDebug)
                log.debug("setupPathInterpolator: pathInterpolator = " + pathInterpolator);
            return reqMsg;
        }
        else {
            path = null;
            pathInterpolator = null;
            return null;
        }
    }
    
    public PathLocAndDir interpolatePath(long timeNow) {
        if (path == null)
            return null;
        else {
            float currentTime = (float)(timeNow - startTime) / 1000.0f;
            float t = currentTime == 0f ? 0.1f : currentTime;
            PathLocAndDir v = pathInterpolator.interpolate(t);
            if (logAll) {
                if (v != null) {
                    if (Log.loggingDebug)
                        log.debug("interpolatePath: t = " + t + "; loc = " + v.getLoc() + "; dir = " + v.getDir());
                }
                else if (Log.loggingDebug)
                    log.debug("interpolatePath: t = " + t + "; PathLocAndDir is null");
            }
            return v;
        }
    }

    public long pathTimeRemaining() {
        if (pathInterpolator != null) {
            long timeSinceStart = System.currentTimeMillis() - startTime;
            return (long)(pathInterpolator.getTotalTime() * 1000f) + timeSinceStart;
        }
        else
            return 0;
    }
    
    public long getOid() {
    	return oid;
    }
    
    public String getType() {
    	return type;
    }
    
    public Point getStartLoc() {
    	return startLoc;
    }
    
    public Point getEndLoc() {
    	return endLoc;
    }
    
    public float getSpeed() {
        return speed;
    }
    
    public List<Point> getPath() {
        return path;
    }
    
    public PathInterpolator getPathInterpolator() {
        return pathInterpolator;
    }
    
    public long getStartTime() {
        return startTime;
    }
    
    protected long oid;
    protected String type;
    protected boolean linear;
    protected Point startLoc;
    protected Point endLoc;
    protected float speed;
    protected List<Point> path;
    protected PathInterpolator pathInterpolator;
    protected long startTime;

    protected static final Logger log = new Logger("PathState");
    protected static boolean logAll = false;
}

