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

import multiverse.server.util.*;
import multiverse.server.math.*;
import java.io.*;
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * although we use a 3d point, this boundary is a 2d boundary which only looks
 * at the X,Z values of the point
 */
public class Boundary implements Cloneable, Serializable {
    public Boundary() {
        setupTransient();
    }

    public Boundary(String name) {
        setupTransient();
        this.name = name;
    }

    public Boundary(List<Point> points) {
        setupTransient();
        setPoints(points);
    }

    void setupTransient() {
        lock = LockFactory.makeLock("BoundaryLock");
    }

    private void readObject(java.io.ObjectInputStream in)
            throws java.io.IOException, ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getName() {
        return name;
    }

    public String toString() {
        String s = "[Boundary: name=" + name;
        lock.lock();
        try {
            for (Point p : pointList) {
                s += " p=" + p;
            }
            return s + "]";
        } finally {
            lock.unlock();
        }
    }

    public Object clone() {
        lock.lock();
        try {
            Boundary b = new Boundary(this.pointList);
            b.setName(this.getName());
            return b;
        } finally {
            lock.unlock();
        }
    }

    public void setPoints(List<Point> points) {
        lock.lock();
        try {
            this.pointList = new LinkedList<Point>(points);
            boundingBox = null;
        } finally {
            lock.unlock();
        }
    }

    public List<Point> getPoints() {
        lock.lock();
        try {
            return new LinkedList<Point>(pointList);
        } finally {
            lock.unlock();
        }
    }

    public void addPoint(Point p) {
        lock.lock();
        try {
            pointList.add(p);
            boundingBox = null;
        } finally {
            lock.unlock();
        }
    }

    public Geometry getBoundingBox() {
        lock.lock();
        try {
            if (boundingBox != null)
                return boundingBox;

            int minX = Integer.MAX_VALUE, maxX = Integer.MIN_VALUE, minZ = Integer.MAX_VALUE, maxZ = Integer.MIN_VALUE;
            for (Point p : pointList) {
                if (p.getX() < minX)
                    minX = p.getX();
                if (p.getX() > maxX)
                    maxX = p.getX();
                if (p.getZ() < minZ)
                    minZ = p.getZ();
                if (p.getZ() > maxZ)
                    maxZ = p.getZ();
            }
            boundingBox = new Geometry(minX, maxX, minZ, maxZ);
            return boundingBox;
        } finally {
            lock.unlock();
        }
    }

    protected Geometry boundingBox = null;

    /**
     * returns if the point is contained within this 2d boundary
     * 
     * pick a point outside of the boundary, create a line segment from the
     * point in question, to the point outside the line. count how many times it
     * crosses line segments of the boundary. if even, its not inside the
     * boundary
     */
    public boolean contains(Point p) {
        // if (Log.loggingDebug)
        //     Log.debug("Boundary.contains: checking p=" + p + ", boundary=" + this);
        int count = 0;

        lock.lock();
        try {
            // pick a point outside the boundary
            // find the largest Z value and pick something bigger
            Integer maxZ = null;
            Integer maxX = null;
            for (Point tmpP : pointList) {
                if (maxZ == null) {
                    maxZ = tmpP.getZ();
                    maxX = tmpP.getX();
                    continue;
                }
                if (tmpP.getZ() > maxZ) {
                    maxZ = tmpP.getZ();
                }
                if (tmpP.getX() > maxX) {
                    maxX = tmpP.getX();
                }
            }
            if ((maxZ == null) || (maxX == null)) {
                return false;
            }

            Point prevPoint = null;
            Point curPoint = null;
            Point firstPoint = null;
            Iterator<Point> iter = pointList.iterator();
            while (iter.hasNext()) {
                // set up the first time around
                if (curPoint == null) {
                    curPoint = iter.next();
                    firstPoint = curPoint;
                    continue;
                }

                // make the line segment from the prevPoint and curPoint
                // the boundary
                prevPoint = curPoint;
                curPoint = iter.next();
                Vector2 p1 = new Vector2(prevPoint.getX(), prevPoint.getZ());
                Vector2 p2 = new Vector2(curPoint.getX(), curPoint.getZ());

                // make the line segment from the point passed in and
                // the point outside the boundary
                Vector2 p3 = new Vector2(p.getX(), p.getZ());
                Vector2 p4 = new Vector2(maxX, maxZ + 1000);

//                Log.debug("Boundary.contains: line segment p1=" + p1 + ", p2="
//                        + p2 + " -- intersect segment p3=" + p3 + ", p4=" + p4);
                if (IntersectSegments(p1, p2, p3, p4)) {
                    count++;
                    // Log.debug("Boundary.contains: intersected");
                } else {
                    // Log.debug("Boundary.contains: did not intersect");
                }
            }
            // check the segment that closes the last to the first points
            if (IntersectSegments(new Vector2(firstPoint.getX(), firstPoint.getZ()), 
                    new Vector2(curPoint.getX(), curPoint.getZ()),
                    new Vector2(p.getX(), p.getZ()),
                    new Vector2(maxX, maxZ + 1000))) {
                count++;
                // Log.debug("Boundary.contains: last side intersected");
            }
            else {
                // Log.debug("Boundary.contains: last side did not intersect");
            }
            boolean rv = ((count % 2) != 0);
            // Log.debug("Boundary.contains: final count=" + count + ", contains="
            //        + rv);
            return rv;
        } finally {
            lock.unlock();
        }
    }

    // check if 2 segments intersect
    private static boolean IntersectSegments(Vector2 p1, Vector2 p2,
            Vector2 p3, Vector2 p4) {
        long den = ((p4.y - p3.y) * (p2.x - p1.x))
                - ((p4.x - p3.x) * (p2.y - p1.y));

        long t1num = ((p4.x - p3.x) * (p1.y - p3.y))
                - ((p4.y - p3.y) * (p1.x - p3.x));

        long t2num = ((p2.x - p1.x) * (p1.y - p3.y))
                - ((p2.y - p1.y) * (p1.x - p3.x));

        if (den == 0) {
            return false;
        }

        double t1 = (double) t1num / (double) den;
        double t2 = (double) t2num / (double) den;

        // if (Log.loggingDebug)
        //     Log.debug("IntersectSegments: t1=" + t1 + ", t2=" + t2);
        // note that we include the endpoint of the second line in the
        // intersection test, but not the endpoint of the first line.
        if ((t1 >= 0) && (t1 < 1) && (t2 >= 0) && (t2 <= 1)) {
            return true;
        }
        return false;
    }

    public static Boundary getMaxBoundary() {
        int min = 0x80000000;
        int max = 0x7fffffff;
        Boundary b = new Boundary();
        b.addPoint(new Point(min,0,max));
        b.addPoint(new Point(max,0,max));
        b.addPoint(new Point(max,0,min));
        b.addPoint(new Point(min,0,min));
        return b;
    }
    List<Point> pointList = new LinkedList<Point>();

    transient Lock lock = null;

    String name = null;
    private static final long serialVersionUID = 1L;
}
