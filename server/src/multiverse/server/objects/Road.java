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

import java.util.*;
import multiverse.server.math.*;
import multiverse.server.engine.Namespace;

public class Road extends Entity {
    public Road() {
        setNamespace(Namespace.WORLD_MANAGER);
    }

    public Road(String name) {
        super(name);
        setNamespace(Namespace.WORLD_MANAGER);
    }

    public String toString() {
        String s = "[Road: name=" + getName() + ", halfWidth=" + getHalfWidth();
        for (Point p : getPoints()) {
            s += " " + p;
        }
        return s + "]";
    }

    public void setHalfWidth(Integer width) {
        this.halfWidth = width;
    }
    public Integer getHalfWidth() {
        return this.halfWidth;
    }
    Integer halfWidth = null;
    
    public void setPoints(List<Point> points) {
        lock.lock();
        try {
            this.points = new LinkedList<Point>(points);
        } finally {
            lock.unlock();
        }
    }

    public List<Point> getPoints() {
        lock.lock();
        try {
            return new LinkedList<Point>(points);
        } finally {
            lock.unlock();
        }
    }

    public void addPoint(Point point) {
        lock.lock();
        try {
//            if (points.isEmpty()) {
//                Log.debug("addPoint: empty, adding Point: " + point);
                points.add(point);
//                return;
//            }
//            Point lastPoint = points.getLast();
//            if (Point.distanceTo(lastPoint, point) < maxSegmentLengthMillis) {
//                Log.debug("addPoint: dist ok, adding Point: " + point);
//                points.add(point);
//                return;
//            }
//
//            // the segment is too long, make it smaller
//            int diffX = point.getX() - lastPoint.getX();
//            int diffY = point.getY() - lastPoint.getY();
//            int diffZ = point.getZ() - lastPoint.getZ();
//            Point midPoint = new Point(point.getX() - (diffX / 2), point.getY()
//                    - (diffY / 2), point.getZ() - (diffZ / 2));
//            Log.debug("addPoint: road=" + getName() + ", lastPoint="
//                    + lastPoint + ", diffX=" + diffX + ", diffY=" + diffY
//                    + ", diffZ=" + diffZ + ", midpoint=" + midPoint
//                    + ", newPoint=" + point);
//            addPoint(midPoint);
//            addPoint(point);
        } finally {
            lock.unlock();
        }
    }

    public List<RoadSegment> generateRoadSegments() {
        lock.lock();
        try {
            List<RoadSegment> list = new LinkedList<RoadSegment>();
            Iterator<Point> iter = points.iterator();
            Point lastPoint = null;
            while (iter.hasNext()) {
                if (lastPoint == null) {
                    lastPoint = iter.next();
                    continue;
                }
                Point curPoint = iter.next();
                RoadSegment seg = new RoadSegment(getName(), lastPoint,
                        curPoint);
                list.add(seg);
                lastPoint = curPoint;
            }
            return list;
        } finally {
            lock.unlock();
        }
    }

    protected LinkedList<Point> points = new LinkedList<Point>();

    public static int maxSegmentLengthMillis = 10000;

    private static final long serialVersionUID = 1L;
}
