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

/**
 * defines a planar polygon in 3D, used in pathing code
 * works with java bean xml serialization
 */
public class PathIntersection {

    public PathIntersection(PathObject pathObject, float where1, float where2,
                            MVVector line1, MVVector line2) {
        this.pathObject = pathObject;
        this.where1 = where1;
        this.where2 = where2;
        this.line1 = line1;
        this.line2 = line2;
    }

    public PathIntersection(PathObject pathObject, PathPolygon cvPoly, float where1, float where2,
                            MVVector line1, MVVector line2) {
        this.pathObject = pathObject;
        this.cvPoly = cvPoly;
        this.where1 = where1;
        this.where2 = where2;
        this.line1 = line1;
        this.line2 = line2;
    }

    public String toString() {
        return "[PathIntersecton line1 = " + line1 + " line2 = " + line2 + 
            " where1 = " + where1 + " where2 = " + where2 + "; cvPoly = " + cvPoly + 
            " pathObject = " + pathObject;
    }

    public static PathIntersection findIntersection(MVVector s1, MVVector e1, MVVector s2, MVVector e2) {
        return findIntersection(null,
                                s1.getX(), s1.getZ(), e1.getX() - s1.getX(), e1.getZ() - s1.getZ(),
                                s2.getX(), s2.getZ(), e2.getX() - s2.getX(), e2.getZ() - s2.getZ());
    }
    
    // Given two segments defined by their starting coords (start1x,
    // start1z) and (start2x, start2z), and the vector to their
    // endpoints (disp1x, disp1z) and (disp2x, disp2z), if
    // they do not intersect, return null.  If they do intersect,
    // return the PathIntersection object that gives the fraction of
    // the distance along the two segments where the intersection
    // occurs
    public static PathIntersection findIntersection(PathObject pathObject,
                                                    float start1x, float start1z,
                                                    float disp1x, float disp1z,
                                                    float start2x, float start2z,
                                                    float disp2x, float disp2z) {
        float det = disp2x * disp1z - disp2z * disp1x;
        float diffx = start2x - start1x;
        float diffz = start2z - start1z;
        if (det * det > 1.0f) {
            float invDet = 1.0f / det;
            float where1 = (disp2x * diffz - disp2z * diffx) * invDet;
            float where2 = (disp1x * diffz - disp1z * diffx) * invDet;
            if (where1 >= 0f && where1 <= 1.0f &&
                where2 >= 0f && where2 <= 1.0f)
                return new PathIntersection(pathObject, where1, where2, 
                                            new MVVector(start2x, 0, start2z), 
                                            new MVVector((start2x + disp2x), 0, (start2z + disp2z)));
        }
        return null;
    }

    public static PathIntersection findIntersection(PathObject pathObject, PathPolygon cvPoly,
                                                    float start1x, float start1z,
                                                    float disp1x, float disp1z,
                                                    float start2x, float start2z,
                                                    float disp2x, float disp2z) {
        PathIntersection i = findIntersection(pathObject, start1x,  start1z, disp1x,  disp1z, 
                                              start2x, start2z, disp2x,  disp2z);
        if (i != null)
            i.setCVPoly(cvPoly);
        return i;
    }

    public static float distancePointLine(MVVector p, MVVector line1, MVVector line2) {
        float line1x = line1.getX();
        float line1z = line1.getZ();
        float line2x = line2.getX();
        float line2z = line2.getZ();
        float linedx = line2x - line1x;
        float linedz = line2z - line1z;
        float numer = Math.abs(linedx * (line1z - p.getZ()) - (line1x - p.getX()) * linedz);
        float denom = (float)(Math.sqrt(linedx * linedx + linedz * linedz));
        return numer / denom;
    }
    
    
    public PathObject getPathObject() {
        return pathObject;
    }

    public PathPolygon getCVPoly() {
        return cvPoly;
    }

    public void setCVPoly(PathPolygon cvPoly) {
        this.cvPoly = cvPoly;
    }

    public float getWhere1() {
        return where1;
    }
    
    public float getWhere2() {
        return where2;
    }
    
    public static MVVector getLinePoint(float where, MVVector loc1, MVVector loc2) {
        MVVector diff = loc2.minus(loc1);
        return loc1.plus(diff.times(where));
    }
    
    public MVVector getIntersectorPoint(float where) {
        MVVector diff = line2.minus(line1);
        return line2.plus(diff.times(where));
    }
    
    public float getIntersectorLength() {
        return MVVector.distanceTo(line1, line2);
    }

    protected PathObject pathObject;
    protected PathPolygon cvPoly = null;
    protected float where1;
    protected float where2;
    // The coordinates of the line we ran into
    protected MVVector line1;
    protected MVVector line2;
}
