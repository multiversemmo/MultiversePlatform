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

import java.lang.Math;
import java.util.*;
import multiverse.server.math.*;
import multiverse.server.util.*;

// A PathSpline is a Catmull-Rom spline, which is related to a
// BSpline.  Paths produced by the pathing code are interpolated as
// Catmull-Rom splines
public class PathSpline extends PathInterpolator {

    public PathSpline(long oid, long startTime, float speed, String terrainString, List<Point> path) {
        super(oid, startTime, speed, terrainString, path);
        // Add two points before the zeroeth point, and one after the
        // count-1 point, to allow us to access from -2 to +1 points.
        path.add(0, path.get(0));
        path.add(path.get(path.size() - 1));
        path.add(path.get(path.size() - 1));
        int count = path.size();
        timeVector = new float[count];
        timeVector[0] = 0f;
        float t = 0;
        MVVector curr = new MVVector(path.get(0));
        for (int i=1; i<count; i++) {
            MVVector next = new MVVector(path.get(i));
            float diff = MVVector.distanceTo(curr, next);
            t = t + diff / speed;
            timeVector[i] = t;
            curr = next;
        }
        totalTime = t;
        if (Log.loggingDebug)
            log.debug("PathSpline constructor: oid = " + oid + "; timeVector = " + 
                      timeVector + "; timeVector.length = " + timeVector.length + "; path = " + path + "; speed = " + speed);
    }

    public String toString() {
        return "[PathSpline oid = " + oid + "; speed = " + speed + "; timeVector = " + timeVector + "; path = " + path + "]";
    }

    // evaluate the position and direction on the spline.  t is the
    // time since we started the first point on the spline.
    public PathLocAndDir interpolate(float t) {
        if (t < 0)
            t = 0;
        else if (t >= totalTime)
            return null;
        // Find the point number whose time is less than or equal to t
        int count = path.size();
        // A bit of trickiness - - the first two points and the last
        // point are dummies, inserted only to ensure that we have -2
        // to +1 points at every real point.
        int pointNumber = -2;
        for (int i=0; i<count; i++) {
            if (timeVector[i] > t) {
                pointNumber = i - 1;
                break;
            }
        }
        if (pointNumber == -1) {
            log.error("interpolateSpline: Time t " + t + " passed to interpolateSpline < 0; oid = " + oid);
            pointNumber = 1;
        }
        MVVector loc;
        MVVector dir;
        // If we're beyond the last time, return the last point, and a
        // (0,0,0) direction
        if (pointNumber == -2) {
            loc = new MVVector(path.get(count - 1));
            dir = new MVVector(0f, 0f, 0f);
        }
        else {
            float timeAtPoint = timeVector[pointNumber];
            float timeSincePoint = t - timeAtPoint;
            float timeFraction = timeSincePoint / (timeVector[pointNumber + 1] - timeAtPoint);
            loc = evalPoint(pointNumber, timeFraction);
            dir = evalDirection(loc, pointNumber, timeFraction).times(speed);
        }
        // A bit tricky - - if there were n elements in the _original_
        // path, there are n-1 characters in the terrain string.
        // We've added _three_ additional path elements, one before
        // and two after.
        int pathNumber = (pointNumber == -2 ? count - 4 : pointNumber - 1);
        if (terrainString.charAt(pathNumber) == 'T' || terrainString.charAt(pathNumber + 1) == 'T') {
            loc.setY(0f);
            dir.setY(0f);
        }
        if (logAll)
            log.debug("interpolateSpline: oid = " + oid + "; t = " + t + "; loc = " + loc + "; dir = " + dir);
        return new PathLocAndDir(new Point(loc), dir, speed * Math.max(0, (totalTime - t)));
    }
    
    // Catmull-Rom spline is just like a B spline, only with a different basis
    protected float basisFactor(int degree, float t) {
        switch (degree) {
        case -1:
            return ((-t + 2f) * t-1f) * t / 2f;
        case 0:
            return (((3f * t-5f) * t) * t + 2f) / 2f;
        case 1:
            return ((-3f * t + 4f) * t + 1f) * t / 2f;
        case 2:
            return ((t-1f) * t * t) / 2f;
        }
        log.error("interpolateSpline: Invalid basis index " + degree + " specified! - oid = " + oid);
        return 0; //we only get here if an invalid i is specified
    }

    // evaluate a point on the spline.  t is the time since we arrived
    // at point pointNumber.
    protected MVVector evalPoint(int pointNumber, float t) {
        float px = 0;
        float py = 0;
        float pz = 0;
        for (int degree = -1; degree<=2; degree++) {
            float basis = basisFactor(degree, t);
            Point pathPoint = path.get(pointNumber + degree);
            px += basis * pathPoint.getX();
            py += basis * pathPoint.getY();
            pz += basis * pathPoint.getZ();
        }
        return new MVVector(px, py, pz);
    }

    // .1 seconds
    protected final float directionTimeOffset = .1f;
    
    // evaluate the direction on the spline.  t is the time since we
    // arrived at point pointNumber.
    protected MVVector evalDirection(MVVector p, int pointNumber, float t) {
        MVVector next = evalPoint(pointNumber, t + directionTimeOffset);
        next.sub(p);
        next.setY(0);
        next.normalize();
        return next;
    }

    protected float [] timeVector;
    protected float totalTime;

    protected static Logger log = new Logger("PathSpline");
    protected static boolean logAll = false;
}
