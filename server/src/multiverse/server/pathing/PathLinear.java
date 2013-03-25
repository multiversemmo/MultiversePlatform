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
import multiverse.server.util.*;

// A linear interpolator of a sequence of points
public class PathLinear extends PathInterpolator {

    public PathLinear(long oid, long startTime, float speed, String terrainString, List<Point> path) {
        super(oid, startTime, speed, terrainString, path);
        float cumm = 0f;
        Point curr = path.get(0);
        for (int i=1; i<path.size(); i++) {
            Point next = path.get(i);
            float dist = Point.distanceTo(curr, next);
            float diffTime = dist / speed;
            cumm += diffTime;
            curr = next;
        }
        totalTime = cumm;
    }

    // evaluate the position and direction on the path.  t is the
    // time since we started the first point on the path.
    public PathLocAndDir interpolate(float t) {
        if (logAll)
            log.debug("interpolate: t = " + t + "; totalTime = " + totalTime);
        if (t < 0)
            t = 0;
        else if (t >= totalTime)
            return null;
        float cumm = 0f;
        Point curr = path.get(0);
        for (int i=1; i<path.size(); i++) {
            Point next = path.get(i);
            MVVector diff = new MVVector(zeroYIfOnTerrain(new MVVector(next).sub(curr), i - 1));
            float dist = diff.lengthXZ();
            float diffTime = dist / speed;
            if (t <= cumm + diffTime) {
                float frac = (t - cumm) / diffTime;
                MVVector loc = new MVVector(curr);
                loc.add(diff.times(frac));
                Point iloc = new Point(loc); // zeroYIfOnTerrain(loc, i - 1);
                MVVector dir = diff;
                dir.normalize();
                dir.multiply(speed);
                return new PathLocAndDir(iloc, dir, speed * (totalTime - t));
            }
            cumm += diffTime;
            curr = next;
        }
        // Didn't find the time, so return the last point, and a dir
        // of zero
        return new PathLocAndDir(path.get(path.size() - 1), new MVVector(0f, 0f, 0f), 0f);
    }

    public String toString() {
        return "[PathLinear oid = " + oid + "; speed = " + speed + "; path = " + path + "]";
    }
    
    protected float totalTime;

    protected static final Logger log = new Logger("PathLinear");
    protected static boolean logAll = false;
}
    
