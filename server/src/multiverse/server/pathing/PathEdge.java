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

import multiverse.server.math.*;
import multiverse.server.util.*;
import java.io.*;
import java.util.*;

/**
 * used in pathing code
 * works with java bean xml serialization
 */
public class PathEdge implements Serializable, Cloneable {

    public PathEdge() {
    }

    public PathEdge(MVVector start, MVVector end) {
        this.start = start;
        this.end = end;
    }
    
    public String toString() {
        return "[PathEdge start=" + getStart() +
            ",end=" + getEnd() + "]";
    }

    public Object clone() {
        return new PathEdge(start, end);
    }

    public MVVector getStart() {
        return start;
    }
    
    public MVVector getEnd() {
        return end;
    }

    public MVVector getMidpoint() {
        return new MVVector((start.getX() + end.getX()) * 0.5f,
                         (start.getY() + end.getY()) * 0.5f,  
                         (start.getZ() + end.getZ()) * 0.5f);
    }
    
    // Return either the start or end point, depending on which is
    // closest to the line from loc1 to loc2, but offset by the
    // offset toward the other end
    public MVVector bestPoint(MVVector loc1, MVVector loc2, float offset) {
        PathIntersection intersection = PathIntersection.findIntersection(loc1, loc2, start, end);
        float len = MVVector.distanceTo(start, end);
        float offsetFraction = offset / len;
        // A unit vector pointing toward end from start
        MVVector delta = new MVVector(end.getX() - start.getX(), 0f, end.getZ() - start.getZ());
        delta.normalize();
        float w2;
        if (intersection == null)
            w2 = (PathIntersection.distancePointLine(start, loc1, loc2) < 
                  PathIntersection.distancePointLine(end, loc1, loc2)) ? offsetFraction : 1f - offsetFraction;
        else
            w2 = intersection.getWhere2();
        MVVector best = null;
        if (w2 < offsetFraction)
            best = new MVVector(start.getX() + delta.getX() * offset,
                             start.getY(), 
                             start.getZ() + delta.getZ() * offset);
        else if (w2 > 1.0f - offsetFraction)
            best = new MVVector(end.getX() - delta.getX() * offset,
                             end.getY(), 
                             end.getZ() - delta.getZ() * offset);
        // else we fit right in the middle
        else
            best = new MVVector(PathIntersection.getLinePoint(w2, new MVVector(start), new MVVector(end)));
        if (logAll)
            log.debug("bestPoint: start = " + start + "; end = " + end + 
                "; best = " + best + "; offset = " + offset + 
                "; offsetFraction = " + offsetFraction + "; w2 = " + w2);
        return best;
    }
    
    public List<MVVector> getNearAndFarNormalPoints(MVVector loc1, MVVector loc2, float offset) {
        List<MVVector> list = new LinkedList<MVVector>();
        MVVector fbest = new MVVector(bestPoint(loc1, loc2, offset));
        MVVector p = new MVVector(end).sub(start);
        p.setY(0f);
        p.normalize();
        // Produce the perpendicular unit vector
        float t = p.getX();
        p.setX(- p.getZ());
        p.setZ(t);
        p.multiply(offset);
        MVVector near = new MVVector(fbest.plus(p));
        p.multiply(-1.0f);
        MVVector far = new MVVector(fbest.plus(p));
//         p = new MVVector(far).sub(near);
//         MVVector loc2mloc1 = new MVVector(loc2).sub(loc1);
//         if (p.dot(loc2mloc1) > 0f) {
        float loc2ToNear = MVVector.distanceTo(loc2, near);
        float loc2ToFar = MVVector.distanceTo(loc2, far);
        if (loc2ToNear < loc2ToFar) {
            MVVector pt = near;
            near = far;
            far = pt;
        }
        MVVector best = new MVVector(fbest);
        float loc1ToBest = MVVector.distanceTo(loc1, best);
        float loc2ToBest = MVVector.distanceTo(loc2, best);
        boolean useNear = loc1ToBest > offset && loc1ToBest > MVVector.distanceTo(near, best);
        boolean useFar = loc2ToBest > offset && loc2ToBest > MVVector.distanceTo(far, best);
        if (useNear && useFar) {
            list.add(near);
            list.add(far);
        }
        else if (useNear && !useFar) {
            list.add(near);
            list.add(best);
        }
        else if (!useNear && !useFar)
            list.add(best);
        else if (!useNear && useFar) {
            list.add(best);
            list.add(far);
        }
        if (logAll)
            log.debug("getNearAndFarNormalPoints: loc1 = " + loc1 + "; loc2 = " + loc2 + 
                "; best = " + best + "; useNear = " + (useNear ? "true" : "false") + "; near = " + near +
                "; useFar = " + (useFar ? "true" : "false") + "; far = " + far);
        return list;
    }

    MVVector start;
    MVVector end;

    protected static final Logger log = new Logger("PathEdge");
    protected static boolean logAll = false;
    private static final long serialVersionUID = 1L;
}

