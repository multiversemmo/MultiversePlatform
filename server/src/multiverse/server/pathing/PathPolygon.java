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

import java.io.*;
import java.util.*;
import multiverse.server.math.*;
import multiverse.server.util.*;

/**
 * defines a planar polygon in 3D, used in pathing code
 * works with java bean xml serialization
 */
public class PathPolygon implements Serializable, Cloneable {

    public PathPolygon() {
    }

    public PathPolygon(int index, byte kind, List<MVVector> corners) {
        this.index = index;
        this.polygonKind = kind;
        this.corners = new LinkedList<MVVector>();
        for (MVVector p : corners)
            this.corners.add(p);
    }
    
    public static final byte Illegal = (byte)0;
    public static final byte CV = (byte)1;
    public static final byte Terrain = (byte)2;
    public static final byte Bounding = (byte)3;

    public String formatPolygonKind(byte val) {
        switch (val) {
        case 0:
            return "Illegal";
        case 1:
            return "CV";
        case 2:
            return "Terrain";
        case 3:
            return "Bounding";
        default:
            return "Unknown PolygonKind " + val;
        }
    }
            
    public PathPolygon ensureWindingOrder(boolean ccw) {
        int ccwCount = 0;
        int size = corners.size();
        for (int i=0; i<size; i++) {
            if (MVVector.counterClockwisePoints(corners.get(PathSynth.wrap(i - 1, size)), corners.get(i), corners.get(PathSynth.wrap(i + 1, size))))
                ccwCount++;
        }
        boolean mustReverse = (ccw && ccwCount < size/2) || (!ccw && ccwCount > size/2);
        if (mustReverse) {
            List<MVVector> newCorners = new LinkedList<MVVector>();
            for (int i=0; i<size; i++)
                newCorners.add(i, corners.get(size - i - 1));
            corners = newCorners;
        }
        return this;
    }
    
    public static byte parsePolygonKind(String s) {
        if (s.equals("Illegal"))
            return Illegal;
        else if (s.equals("CV"))
            return CV;
        else if (s.equals("Terrain"))
            return Terrain;
        else if (s.equals("Bounding"))
            return Bounding;
        else
            return Illegal;
    }
    
    public boolean pointInside2D(MVVector p) {
        boolean inside = false;
        int j = corners.size() - 1;
        for (int i=0; i<corners.size(); j = i++) {
            MVVector ci = new MVVector(corners.get(i));
            MVVector cj = new MVVector(corners.get(j));
            float fa = (cj.getZ() - ci.getZ()) * (p.getX() - ci.getX());
            float fb = (cj.getX() - ci.getX()) * (p.getZ() - ci.getZ());
            if ((ci.getZ() <= p.getZ() && p.getZ() < cj.getZ() && fa < fb) ||
                (cj.getZ() <= p.getZ() && p.getZ() < ci.getZ() && fa > fb))
                inside = !inside;
        }
        return inside;
    }

    public boolean pointInside(MVVector p, float tolerance) {
        if (!pointInside2D(p))
            return false;
        if (plane == null)
            plane = new Plane(corners.get(0), corners.get(1), corners.get(2));
        return plane.getDistance(p) <= tolerance;
    }
    
    public Integer cornerNumberForPoint(MVVector point, float epsilon) {
        int i=0;
        for (MVVector corner : corners) {
            if (MVVector.distanceToSquared(point, corner) < epsilon)
                return i;
            i++;
        }
        return null;
    }
        
    // Return the number of the corner closest to loc
    public int getClosestCornerToPoint(MVVector loc) {
        int count = corners.size();
        int closestCorner = -1;
        float closestDistance = Float.MAX_VALUE;
        for (int i=0; i<count; i++) {
            float d = MVVector.distanceTo(loc, corners.get(i));
            if (d < closestDistance) {
                closestDistance = d;
                closestCorner = i;
            }
        }
        return closestCorner;
    }
    
    // Return the number of the corner farthest from loc
    public int getFarthestCornerFromPoint(MVVector loc) {
        int count = corners.size();
        int farthestCorner = -1;
        float farthestDistance = Float.MIN_VALUE;
        for (int i=0; i<count; i++) {
            float d = MVVector.distanceTo(loc, corners.get(i));
            if (d > farthestDistance) {
                farthestDistance = d;
                farthestCorner = i;
            }
        }
        return farthestCorner;
    }
    
    // If the two polygons' sides intersect, return a list of those
    // intersection points, as well
    public static List<PolyIntersection> findPolyIntersections(PathPolygon poly1, PathPolygon poly2) {
        int p1Size = poly1.corners.size();
        int p2Size = poly2.corners.size();
        if (Log.loggingDebug)
            log.debug("PathPolygon.findPolyIntersections: Finding intersections of " + poly1 + " and " + poly2);
        List<PolyIntersection> intersections = null;
        for (int p1Index=0; p1Index < p1Size - 1; p1Index++) {
            MVVector p1Corner1 = poly1.corners.get(p1Index);
            MVVector p1Corner2 = poly1.corners.get(p1Index + 1);
            for (int p2Index=0; p2Index < p2Size - 1; p2Index++) {
                MVVector p2Corner1 = poly2.corners.get(p2Index);
                MVVector p2Corner2 = poly2.corners.get(p2Index + 1);
                PathIntersection intr = PathIntersection.findIntersection(p1Corner1, p1Corner2, p2Corner1, p2Corner2);
                if (intr != null) {
                    if (intersections == null)
                        intersections = new LinkedList<PolyIntersection>();
                    intersections.add(new PolyIntersection(p1Index, p2Index, intr));
                }
            }
        }
        return intersections;
    }
    
    // Return a PathIntersection object describing the closest
    // intersection of a side of the polygon with a line segment from
    // loc1 to loc2, or null if there is none.
    public PathIntersection closestIntersection(PathObject pathObject, MVVector loc1, MVVector loc2) {
        float dispX = loc2.getX() - loc1.getX();
        float dispZ = loc2.getZ() - loc1.getZ();
        PathIntersection closest = null;
        int j = corners.size() - 1;
        for (int i=0; i<corners.size(); j = i++) {
            MVVector ci = corners.get(i);
            MVVector cj = corners.get(j);
            float ciX = ci.getX();
            float ciZ = ci.getZ();
            PathIntersection intersection = PathIntersection.findIntersection(
                pathObject, this, loc1.getX(), loc1.getZ(), dispX, dispZ,
                ciX, ciZ, cj.getX() - ciX, cj.getZ() - ciZ);
            if (intersection == null)
                continue;
            else if (closest == null || intersection.getWhere1() < closest.getWhere1())
                closest = intersection;
        }
        return closest;
    }
    
    public String toString() {
        String pts = "";
        for (MVVector corner : corners) {
            if (pts.length() > 0)
                pts += ", ";
            pts += corner.toString();
        }
        return "[PathPolygon: index = " + index + "; kind = " + formatPolygonKind(polygonKind) + "; corners = " + pts + "]";
    }
    
    // Return the average of the vertices of the polygon
    public MVVector getCentroid() {
        MVVector result = new MVVector(0, 0, 0);
        for (MVVector corner : corners)
            result.add(corner);
        result.multiply(1.0f / (float)corners.size());
        return result;
    }
    
    public Object clone() {
        return new PathPolygon(index, polygonKind, corners);
    }

    public byte getKind() {
    	return polygonKind;
    }
    
    public void setKind(byte val) {
        polygonKind = val;
    }
    
    public int getIndex() {
        return index;
    }
    
    public void setIndex(int index) {
        this.index = index;
    }
    
    public List<MVVector> getCorners() {
        return corners;
    }

    public void setCorners(List<MVVector> corners) {
        this.corners = corners;
    }

    int index;
    byte polygonKind;
    List<MVVector> corners;
    Plane plane = null;
    protected static final Logger log = new Logger("PathPolygon");
    private static final long serialVersionUID = 1L;
}

