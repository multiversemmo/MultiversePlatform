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

package multiverse.server.math;

import java.io.*;

/**
 * this object is not thread safe
 */
public class Plane implements Cloneable, Serializable {

    // The "positive side" of the plane is the half space to which the
    // plane normal points. The "negative side" is the other half
    // space. The flag "no side" indicates the plane itself.
    public enum PlaneSide {
        None((byte)0),
        Positive((byte)1),
        Negative((byte)2);
        PlaneSide(byte val) {
            this.val = val;
        }
        byte val = -1;
    }

    public Plane() {
    }
    
    // construct a plane from the normal and a point on the plane
    public Plane(MVVector normal, MVVector point) {
        this.normal = normal;
        this.d = - normal.dotProduct(point);
    }

    // provide a version of the constructor that handles integer Points
    public Plane(Point intPoint0, Point intPoint1, Point intPoint2) {
        MVVector point0 = new MVVector(intPoint0);
        MVVector point1 = new MVVector(intPoint1);
        MVVector point2 = new MVVector(intPoint2);
        MVVector edge1 = point1.minus(point0);
        MVVector edge2 = point2.minus(point0);
        normal = MVVector.cross(edge1, edge2);
        normal.normalize();
        d = - normal.dotProduct(point0);
    }

    // construct a plane from three coplanar points
    public Plane(MVVector point0, MVVector point1, MVVector point2) {
        MVVector edge1 = point1.minus(point0);
        MVVector edge2 = point2.minus(point0);
        normal = MVVector.cross(edge1, edge2);
        normal.normalize();
        d = - normal.dotProduct(point0);
    }

    public PlaneSide getSide(MVVector point) {
        float distance = getDistance(point);
        if (distance < 0.0f)
            return PlaneSide.Negative;

        if ( distance > 0.0f )
            return PlaneSide.Positive;

        return PlaneSide.None;
    }

    // This is a pseudodistance. The sign of the return value is
    // positive if the point is on the positive side of the plane,
    // negative if the point is on the negative side, and zero if the
    //	 point is on the plane.
    // The absolute value of the return value is the true distance only
    // when the plane normal is a unit length vector.
    public float getDistance(MVVector point) {
        return normal.dotProduct(point) + d;
    }
    
    protected MVVector normal;
    protected float d;
    private static final long serialVersionUID = 1L;
}
