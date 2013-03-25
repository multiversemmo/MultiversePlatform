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

abstract public class PathInterpolator {
    
    public PathInterpolator(long oid, long startTime, float speed, String terrainString, List<Point> path) {
        this.oid = oid;
        this.startTime = startTime;
        this.speed = speed;
        this.terrainString = terrainString;
        this.path = path;
    }
    
    abstract public String toString();
    
    abstract public PathLocAndDir interpolate(float t);
    
    public PathLocAndDir interpolate(long systemTime) {
        return interpolate((float)(systemTime - startTime) / 1000F);
    }
    
    public Point zeroYIfOnTerrain(MVVector loc, int pointIndex) {
        assert pointIndex >= 0 && pointIndex < terrainString.length() - 1;
        // If either the previous point was on terrain or the
        // current point is on terrain, then the path element is a 
        Point iloc = new Point(loc);
        if (terrainString.charAt(pointIndex) == 'T' || terrainString.charAt(pointIndex + 1) == 'T')
            iloc.setY(0);
        return iloc;
    }
    
    public long getOid() {
        return oid;
    }

    public float getSpeed() {
        return speed;
    }

    public String getTerrainString() {
        return terrainString;
    }

    public long getStartTime() {
        return startTime;
    }
    
    public float getTotalTime() {
        return totalTime;
    }
    
    public Point getLastPoint() {
        int len = path.size();
        if (len > 0)
            return path.get(len - 1);
        else
            return null;
    }
    
    protected long oid;
    protected float speed;
    protected String terrainString;
    protected List<Point> path;
    protected float totalTime;  // In seconds from start time
    protected long startTime;   // In milliseconds - - server system time
}
