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

/**
 * This class contains machinery to synthesize a list of "CV" polygons
 * and their arcs starting with a boundary polygon and a collection of
 * obstacles in or overlapping with that boundary.
 */
public class PathSynth {

    public PathSynth(PathPolygon boundary, List<PathPolygon> obstacles) {
        this.modelName = "Dynamic";
        this.type = "Dynamic";
        this.boundingPolygon = boundary;
        this.arcs = new LinkedList<PathArc>();
        this.polygons = new LinkedList<PathPolygon>();
        boundary.setIndex(1);
        boundary.setKind(PathPolygon.CV);
        combineBoundaryAndObstacles(boundary, obstacles);
        List<PathPolygon> polygonsCopy = new LinkedList<PathPolygon>(polygons);
        for (PathPolygon poly : polygonsCopy)
            generateConvexPolygons(poly);
        arcs = PathObject.discoverArcs(polygons);
        if (Log.loggingDebug) {
            log.debug("PathSynth constructor: After combining boundary and obstacles:");
            int i = 0;
            for (PathPolygon polygon : polygons)
                log.debug("      Poly " + i++ + ": " + polygon);
            i = 0;
            for (PathArc arc : arcs)
                log.debug("      Arc  " + i++ + ": " + arc);
        }
    }
        
    /**
     * If there is more than one obstacle, start by computing the
     * convex hull of all the obstacles, yielding a single obstacle.
     * Then compute the collection of boundary polygons that surround
     * that convex hull.
     * </p>
     * Now we must deal with the parts that are inside the hull but
     * not inside any obstacle.  For each line in the hull, if both
     * points in the line come from the same obstacle, then that line
     * forms a side of the hull.  But if the two points belong to
     * different obstacles, then the line is a side of a polygon must
     * be added to the set of boundary polygons.
     * </p>
     * Start with an obstacle vertex in a direction that takes it
     * inside of the hull.  That vertex will be the first point in an
     * "interior" polygon to be added to the boundary.  Add that
     * vertex, and each successive point until you again reach the
     * boundary of the hull.  Take the direction that is away from the
     * side of the polygon contained in the hull, encountering a point
     * in a different obstacle.  Keep repeating the process until you
     * arrive again at the original point, having created the interior
     * boundary polygon.  This polygon is most likely not convex, and
     * may contain obstacles, but should no intersect any obstacles.
     * </p>
     * Two outstanding problems:
     * </p>
     * First, in the case of an obstacle that intersects the hull at
     * only one point, how do we represent the hole?  The code below
     * will add all the obstacle points, which causes the single hull
     * point to be represented twice, which no doubt messes up the
     * code that tries to break convex polygons.
     * </p>
     * Second, the new boundary will in general contain obstacles.  We
     * can form a convex hull around those obstacles, but in general
     * the hull will not lie completely inside of the boundary.  The
     * cure here is to prune the hull by based on intersections with
     * the boundary.  But we're adding a god-awful amount of
     * machinery.
     */
    protected void combineBoundaryAndObstacles(PathPolygon boundaryPoly, List<PathPolygon> obstacles) {
        int obstacleCount = obstacles.size();
        if (obstacleCount == 0) {
            // No obstacles - - just add the boundary
            polygons.add(boundaryPoly);
            return;
        }
        if (obstacleCount == 1) {
            // One obstacle - - break the boundary around the
            // aggregated obstacle hull
            PathPolygon obstacle = obstacles.get(0);
            PathPolygon newPoly = breakBoundaryAroundObstacle(boundaryPoly, obstacle);
            polygons.add(boundaryPoly);
            polygons.add(newPoly);
            return;
        }
        // More than one obstacle.  Compute the convex hull.
        List<HullLine> hullLines = computeConvexHull(obstacles);
        HullPoint[] hullPoints = sortHullPoints(hullLines);
        // Since we have more than one obstacle, we must add the
        // interior regions of the hull that are not covered by
        // obstacles.

        // Start by modifying the hull polygon to exclude obstacles
        // that have sides that are in the hull.
        int hullSize = hullPoints.length;
        List<MVVector> corners = new LinkedList<MVVector>();
        List<PathPolygon> usedObstacles = new LinkedList<PathPolygon>();
        int hullPointNumber = 0;
        // Loop until we encounter an obstacle polygon we've seen
        // before
        while (true) {
            HullPoint hullPoint = hullPoints[hullPointNumber];
            List<MVVector> obstacleCorners = hullPoint.polygon.getCorners();
            int obstacleSize = obstacleCorners.size();
            if (usedObstacles.contains(hullPoint.polygon))
                break;
            boolean previousSamePoly = hullPoint.polygon != hullPoints[wrap(hullPointNumber - 1, hullSize)].polygon;
            boolean nextSamePoly = hullPoint.polygon == hullPoints[wrap(hullPointNumber + 1, hullSize)].polygon;
            // If we're starting at a vertex where both hull sides
            // originating with that vertex belong to the same
            // obstacle, back up and try again.
            if (previousSamePoly && nextSamePoly) {
                hullPointNumber = wrap(hullPointNumber - 1, hullSize);
                continue;
            }
            int lastObstacleHullPointNumber;
            int cornerDirection = -1;
            if (nextSamePoly) {
                // This is the first side of a obstacle that shares at
                // least one side with the convex hull, so find the
                // last hull point.
                lastObstacleHullPointNumber = hullPointNumber;
                int j = lastObstacleHullPointNumber;
                while (hullPoints[j].polygon == hullPoint.polygon) {
                    lastObstacleHullPointNumber = j;
                    j = wrap(j + 1, hullSize);
                }
                // Now lastObstacletHullPointNumber is the number of
                // the last hull point for this obstacle.  So add
                // points from the obstacle to the list of corners
                // until we come to that point.
                if (wrap(hullPoint.cornerNumber + 1, obstacleSize) == hullPoints[wrap(hullPointNumber + 1, hullSize)].cornerNumber)
                    cornerDirection = 1;
            }
            else {
                // This obstacle only touches the hull at this one
                // point, so add all the points in the obstacle.  ???
                // TBD: But we have two vertices for the same point.
                // How do we distinguish them
                int cornerNumber = hullPoint.cornerNumber;
                lastObstacleHullPointNumber = cornerNumber;
                // Find the angle between the previous hull point, the
                // current hull point, and each of the two obstacle
                // vertices adjacent to the hull point and use the one
                // with the largest angle.
                HullPoint previousHullPoint = hullPoints[wrap(hullPointNumber - 1, hullSize)];
                MVVector v = hullPoint.point.minus(previousHullPoint.point);
                v.normalize();
                MVVector cPlus = obstacleCorners.get(wrap(hullPoint.cornerNumber + 1, obstacleSize)).minus(hullPoint.point);
                cPlus.normalize();
                MVVector cMinus = obstacleCorners.get(wrap(hullPoint.cornerNumber - 1, obstacleSize)).minus(hullPoint.point);
                cMinus.normalize();
                if (v.dotProduct(cPlus) > v.dotProduct(cMinus))
                    cornerDirection = -1;
            }
            int cornerNumber = hullPoint.cornerNumber;
            corners.add(obstacleCorners.get(cornerNumber));
            do {
                cornerNumber = wrap(cornerNumber + cornerDirection, obstacleSize);
                corners.add(obstacleCorners.get(cornerNumber));
            }
            while (cornerNumber != hullPoints[lastObstacleHullPointNumber].cornerNumber);
            usedObstacles.add(hullPoint.polygon);
        }
        PathPolygon newPoly = breakBoundaryAroundObstacle(boundaryPoly, new PathPolygon(0, PathPolygon.CV, corners));
        polygons.add(boundaryPoly);
        polygons.add(newPoly);
    }
    
    /**
     * Merge the boundary polygon with those obstacles whose sides
     * intersect the sides of the boundary, and for obstacles that are
     * wholly contained in the boundary, add them one-by-one,
     * splitting the containing boundary polygons into two polygons
     * for each one, so that the boundary polygons do not contain
     * holes.  Finally, convert the concave boundary polygons into
     * convex ones by splitting them up.
     */
    protected void combineBoundaryAndObstacles_old(PathPolygon boundaryPoly, List<PathPolygon> obstacles) {
        List<PathPolygon> holeyObstacles = spliceOutIntersections(boundaryPoly, obstacles);
        List<PathPolygon> boundaryPolys = new LinkedList<PathPolygon>();
        boundaryPolys.add(boundaryPoly);
        PathPolygon newPoly = null;
        for (PathPolygon obstacle : holeyObstacles) {
            newPoly = breakBoundaryAroundObstacle(boundaryPoly, obstacle);
            if (newPoly == null)
                log.error("PathSynth.combineBoundaryAndObstacles: obstacle " + obstacle + 
                    " is not wholly contained in any boundary polygon!");
            else
                boundaryPolys.add(newPoly);
        }
        polygons.addAll(boundaryPolys);
    }
    
    /**
     * Break a boundary polygon into two pieces around a convex
     * obstacle which it wholly contains, changing the corner set of
     * the original boundary polygon, and returning a new polygon.
     */
    protected PathPolygon breakBoundaryAroundObstacle(PathPolygon boundaryPoly, PathPolygon obstacle) {
        List<MVVector> obstacleCorners = obstacle.getCorners();
        boolean obstacleccw = MVVector.counterClockwisePoints(obstacleCorners.get(0), obstacleCorners.get(1), obstacleCorners.get(2));
        int firstCornerNumber = 0;
        int middleCornerNumber = obstacleCorners.size() / 2;
        MVVector obstacleCorner1 = obstacleCorners.get(firstCornerNumber);
        MVVector obstacleCorner2 = obstacleCorners.get(middleCornerNumber);
        int boundaryCorner1Number = boundaryPoly.getClosestCornerToPoint(obstacleCorner1);
        int boundaryCorner2Number = boundaryPoly.getClosestCornerToPoint(obstacleCorner2);
        // Now split the boundary into two pieces
        List<MVVector> boundaryCorners = boundaryPoly.getCorners();
        boolean boundaryccw = MVVector.counterClockwisePoints(boundaryCorners.get(0), boundaryCorners.get(1), boundaryCorners.get(2));
        // Create the new poly
        PathPolygon newPoly = new PathPolygon();
        newPoly.setKind(PathPolygon.CV);
        newPoly.setIndex(polygons.size() + 1);
        // Create corners for the two polys
        List<MVVector> newPolyCorners = new LinkedList<MVVector>();
        List<MVVector> originalPolyCorners = new LinkedList<MVVector>();
        addCornersInRange(newPolyCorners, boundaryCorners, boundaryCorner2Number, boundaryCorner1Number, 1);
        addCornersInRange(originalPolyCorners, boundaryCorners, boundaryCorner1Number, boundaryCorner2Number, 1);
        int incr = boundaryccw == obstacleccw ? -1 : 1;
        addCornersInRange(newPolyCorners, obstacleCorners, firstCornerNumber, middleCornerNumber, incr);
        addCornersInRange(originalPolyCorners, obstacleCorners, middleCornerNumber, firstCornerNumber, incr);
        // Set the corners of the two polys
        boundaryPoly.setCorners(originalPolyCorners);
        newPoly.setCorners(newPolyCorners);
        return newPoly;
    }
    
    /**
     * Add the corners from the first to the last corner numbers to
     * the newCorners list, wrapping if first is greater than last.
     */
    protected void addCornersInRange(List<MVVector> newCorners, List<MVVector> oldCorners, 
                                     int firstOldCorner, int lastOldCorner, int incr) {
        int oldCount = oldCorners.size();
        int cornerNumber = firstOldCorner;
        while (true) {
            newCorners.add(oldCorners.get(cornerNumber));
            if (cornerNumber == lastOldCorner)
                break;
            if (incr > 0)
                cornerNumber = (cornerNumber == oldCount - 1 ? 0 : cornerNumber + 1);
            else
                cornerNumber = (cornerNumber == 0 ? oldCount - 1 : cornerNumber - 1);
        }
    }

    
    public static int wrap(int index, int size) {
        int i = index % size;
        if (i >= 0)
            return i;
        else
            return (index + size) % size;
    }

    /**
     * A class that represents points around a convex hull.  It holds
     * the MVVector point itself, plus the polygon it belongs to and
     * corner number with the polygon.
     */
    public static class HullPoint {
        public int cornerNumber;
        public MVVector point;
        public PathPolygon polygon;
        
        public  HullPoint (int cornerNumber, MVVector point, PathPolygon polygon) {
            this.cornerNumber = cornerNumber;
            this.point = point;
            this.polygon = polygon;
            if (cornerNumber >= polygon.getCorners().size())
                log.error("HullLine constructor: cornerNumber1 " + cornerNumber + 
                    " is beyond poly1.corners.size() " + polygon.getCorners().size());
        }
    }
    
    /**
     * A line in a convex hull - - just a pair of HullPoints
     */
    public static class HullLine {
        public HullPoint hullPoint1;
        public HullPoint hullPoint2;

        /**
         * HullLine constructor for lines that reference polygons.
         */
        public HullLine(HullPoint hullPoint1, HullPoint hullPoint2) {
            this.hullPoint1 = hullPoint1;
            this.hullPoint2 = hullPoint2;
        }

        public MVVector point1() {
            return hullPoint1.point;
        }

        public MVVector point2() {
            return hullPoint2.point;
        }
    }

    /**
     * Compute the slope of a pair of points, or return null if the
     * slope would be infinite.
     */
    protected static Float computeSlope(MVVector point1, MVVector point2) {
        if (point1.getX() == point2.getX())
            return null;
        else {    
            if (point2.getZ() == point1.getZ())
                return 0f;
            else
                return (point2.getZ() - point1.getZ()) / (point2.getX() - point1.getX());
        }
    }

    /**
     * Given a point, determine if that point is lying
     * on the left side or right side of the first point of the
     * line.
     */
    protected static boolean onLeft(MVVector point1, MVVector point2, Float slope, MVVector p) {
        if (slope == null) {
            if (p.getX() < point1.getX()) return true;
            else {
                if (p.getX() == point1.getX()) {
                    if (((p.getZ() > point1.getZ()) && (p.getZ() < point2.getZ())) ||
                        ((p.getZ() > point2.getZ()) && (p.getZ() < point1.getZ())))
                        return true;
                    else
                        return false;
                }
                else return false;
            }
        }
        else {            
            float x3 = (p.getX() + slope * (slope * point1.getX() - point1.getZ() + p.getZ())) / (1f + slope * slope);
            float z3 = slope * (x3 - point1.getX()) + point1.getZ();

            if (slope == 0f)
                return p.getZ() > z3;
            else if (slope > 0f)
                return x3 > p.getX();
            else 
                return p.getX() > x3;
        }
    }

    /**
     * Returns the HullLine containing the point if the point is a
     * vertex of the hull
     */
    protected static HullPoint hullVertex(HullPoint[] points, MVVector p) {
        for (HullPoint hullPoint : points) {
            if (MVVector.distanceToSquared(hullPoint.point, p) < epsilon)
                return hullPoint;
        }
        return null;
    }
    
    /**
     * Produces an array of sorted HullPoint objects such that the
     * points are in order around the perimeter of the hull.
     */
    protected static HullPoint[] sortHullPoints(List<HullLine> lines) {
        HullPoint[] sortedPoints = new HullPoint[lines.size()];
        HullLine currentLine = null;
        int count = 0;
        for (HullLine line : lines) {
            if (currentLine == null)
                currentLine = line;
            else {
                // Find the line one of whose points is the second
                // point in the current line
                MVVector p = currentLine.point2();
                HullLine nextLine = null;
                for (HullLine otherLine : lines) {
                    if (line == currentLine)
                        continue;
                    else if (MVVector.distanceToSquared(p, otherLine.point1()) < epsilon) {
                        nextLine = line;
                        break;
                    }
                    else if (MVVector.distanceToSquared(p, otherLine.point2()) < epsilon) {
                        // Create a line whose second point is the one that doesn't match
                        nextLine = new HullLine(line.hullPoint2, line.hullPoint1);
                        break;
                    }
                }
                if (nextLine != null)
                    currentLine = nextLine;
                else {
                    log.error("PathSynth.sortHullLines: Could not find the HullLine starting with point " + p);
                    return sortedPoints;
                }
                sortedPoints[count++] = currentLine.hullPoint1;
            }
        }
        return sortedPoints;
    }
        
    /**
     * N**3 algorithm to compute the convex hull of a set of polygons.
     * We can always implement the nlogn algorithm if we notice the
     * time.
     */
    public List<HullLine> computeConvexHull(List<PathPolygon> obstacles) {
        boolean leftMost, rightMost;
        int pointCount = 0;
        for (PathPolygon polygon : obstacles)
            pointCount += polygon.getCorners().size();
        HullPoint[] points = new HullPoint[pointCount];
        int hullPointCount = 0;
        for (PathPolygon polygon : obstacles) {
            List<MVVector> corners = polygon.getCorners();
            int i=0;
            for (MVVector corner : corners)
                points[hullPointCount++] = new HullPoint(i++, corner, polygon);
        }
        List<HullLine> hull = new LinkedList<HullLine>();
        for (int c1 = 0; c1<pointCount; c1++) {
            for (int c2=c1+1; c2<pointCount; c2++) {
                leftMost  = true;
                rightMost = true;
                HullPoint p1 = points[c1];
                MVVector point1 = p1.point;
                HullPoint p2 = points[c2];
                MVVector point2 = p2.point;
                Float slope = computeSlope(point1, point2);
                for (int c3 = 0; c3 < pointCount; c3++) {
                    if ((c3 != c1) && (c3 != c2)) {
                        if (onLeft(point1, point2, slope, points[c3].point))
                            leftMost = false;
                        else
                            rightMost = false;
                    }
                }

                if (leftMost || rightMost)
                    hull.add(new HullLine(p1, p2));
            }
        }
        return hull;
    }

    /**
     * For obstacles whose sides intersect the sides of the boundary,
     * change the boundary by adding appropriate vertices.  Return
     * those obstacles wholly contained in the boundary,
     */
    protected List<PathPolygon> spliceOutIntersections(PathPolygon boundary, List<PathPolygon> obstacles) {
        List<PathPolygon> holeyObstacles = new LinkedList<PathPolygon>();
        for (PathPolygon obstacle : obstacles) {
            List<PolyIntersection> intersections = PathPolygon.findPolyIntersections(boundary, obstacle);
            if (intersections != null) {
                if (Log.loggingDebug)
                    log.debug("PathSynth.spliceOutIntersections: " + intersections.size() + " intersections");
                if (intersections.size() == 2)
                    mergeDoublyIntersectingObstacle(boundary, obstacle, intersections);
                else
                    log.warn("PathSynth.spliceOutIntersections: Can't handle " + intersections.size() + " intersections");
            }
            else {
                if (!whollyContained(boundary, obstacle))
                    log.warn("PathSynth.spliceOutIntersections: Obstacle is not wholly contained in the the boundary, but does not intersect it.");
                else {
                    // The obstacle creates a hole in the boundary.
                    // Put it in the list of holey obstacles
                    holeyObstacles.add(obstacle);
                    if (Log.loggingDebug)
                        log.debug("PathSynth.spliceOutIntersections: new holeyObstacle " + obstacle);
                }
            }
        }
        return holeyObstacles;
    }

    /**
     * Returns true if the boundary contains every vertex of the
     * obstacle; false otherwise.
     */
    protected boolean whollyContained(PathPolygon boundary, PathPolygon obstacle) {
        // No intersections.  Check to see that the obstacle
        // is wholly contained in the boundary
        boolean contained = true;
        for (MVVector p : obstacle.getCorners()) {
            if (!boundary.pointInside2D(p)) {
                contained = false;
                break;
            }
        }
        if (Log.loggingDebug)
            log.debug("PathSynth.whollyContained: obstacle " + obstacle + (contained ? "is" : "is not") + 
                    " wholly contained in " + boundary);
        return contained;
    }
    
    /**
     * Handle the case of an obstacle which intersects the line
     * segments of the boundary in exactly two places.
     */
    protected void mergeDoublyIntersectingObstacle(PathPolygon boundary, PathPolygon obstacle, List<PolyIntersection> intersections) {
        // We have only 2 intersections, which means that
        // we can just splice out the boundary points
        // between the two.
        PolyIntersection intr1 = intersections.get(0);
        PolyIntersection intr2 = intersections.get(1);
        // The first step is to replace the boundary's corner.
        int bc1Index = intr1.poly1Corner;
        int bc2Index = intr2.poly1Corner;
        List<MVVector> boundaryCorners = boundary.getCorners();
        int size = boundaryCorners.size();
        List<MVVector> obstaclePoints = pointsInside(boundary, obstacle);
        // If there are points in the boundary that must be removed, get rid of them.
        if (bc1Index != bc2Index) {
            int count = bc2Index - bc1Index;
            if (count < 0)
                count += size;
            for (int i=0; i<count; i++) {
                int index = wrap(bc1Index + 1, size);
                boundaryCorners.remove(index);
                size = boundaryCorners.size();
            }
        }
        PathIntersection pintr1 = intr1.intr;
        MVVector corner1 = boundaryCorners.get(bc1Index);
        bc2Index = wrap(bc1Index + 1, size);
        MVVector corner2 = boundaryCorners.get(bc2Index);
        MVVector newPoint1 = corner2.minus(corner1).multiply(pintr1.getWhere1());
        boundaryCorners.add(bc2Index, newPoint1);
        MVVector newPoint2 = corner2.minus(corner1).multiply(pintr1.getWhere2());
        bc2Index = wrap(bc2Index + 1, size);
        boundaryCorners.add(bc2Index, newPoint2);
        boundaryCorners.addAll(bc2Index, obstaclePoints);
    }

    /**
     * Return true if all vertices of the obstacle are contained in
     * the boundary; false otherwise.
     */
    protected List<MVVector> pointsInside(PathPolygon boundary, PathPolygon obstacle) {
        List<MVVector> points = new LinkedList<MVVector>();
        for (MVVector p : obstacle.getCorners()) {
            if (boundary.pointInside2D(p))
                points.add(p);
        }
        return points;
    }

    /**
     * Updates the list of polygons, each of which is convex, and
     * portals between the polygons required make the input polygon
     * convex.  If the input polygon is already convex, then it
     * returns a list containing that single polygon, and no portals.
     */
    protected void generateConvexPolygons(PathPolygon poly) {
        // Find the back-edges; those with interior angles >= 180 degrees
        List<MVVector> originalCorners = poly.getCorners();
        int size = originalCorners.size();
        MVVector[] points = new MVVector[size];
        int c = 0;
        for (MVVector corner : originalCorners)
            points[c++] = new MVVector(corner);
        List<Integer> concaveCorners = new LinkedList<Integer>();
        for (int i=0; i<size; i++) {
            if (!MVVector.counterClockwisePoints(
                    points[wrap(i - 1, size)],
                    points[i],
                    points[wrap(i + 1, size)])) {
                concaveCorners.add(i);
            }
        }
        if (concaveCorners.size() == 0) {
            if (Log.loggingDebug)
                log.debug("PathSynth.generateConvexPolygons: Poly is convex " + poly);
            return;
        }
        for (int origCornerNumber=0; origCornerNumber<size; origCornerNumber++) {
            if (concaveCorners.contains(origCornerNumber)) {
                // We know that every corner up to this one is convex.
                // Now work backwards from the first corner, til we
                // come to corner that is non-convex, or come within
                // two corners of this corner
                int lastNewPolyCorner = origCornerNumber;
                int newPolyCornerCount = 3;
                if (Log.loggingDebug)
                    log.debug("PathSynth.generateConvexPolygons: size " + size + ", lastNewPolyCorner " + lastNewPolyCorner);
                int firstNewPolyCorner = wrap(lastNewPolyCorner - 2, size);
                while (true) {
//                     if (Log.loggingDebug)
//                         log.debug("PathSynth.generateConvexPolygons: checking corners " + wrap(lastNewPolyCorner - 1, size) + ", " + lastNewPolyCorner + " and " + firstNewPolyCorner);
                    if (!MVVector.counterClockwisePoints(
                            points[wrap(lastNewPolyCorner - 1, size)],
                            points[lastNewPolyCorner],
                            points[firstNewPolyCorner])) {
                        firstNewPolyCorner = wrap(firstNewPolyCorner + 1, size);
                        newPolyCornerCount--;
                        break;
                    }
                    else {
                        firstNewPolyCorner = wrap(firstNewPolyCorner - 1, size);
                        newPolyCornerCount++;
                    }
                }
//                 if (Log.loggingDebug)
//                     log.debug("PathSynth.generateConvexPolygons: newPolyCornerCount " + newPolyCornerCount + 
//                         ", first corner " + firstNewPolyCorner + ", last corner " + lastNewPolyCorner);
                // Create the newPoly corners
                LinkedList<MVVector> newPolyCorners = new LinkedList<MVVector>();
                for (int i=0; i<newPolyCornerCount; i++) {
                    int n = wrap(firstNewPolyCorner + i, size);
                    newPolyCorners.add(originalCorners.get(n));
                }
                // The new polygon is guaranteed convex
                int newPolyIndex = polygons.size() + 1;
                PathPolygon newPoly = new PathPolygon(newPolyIndex, PathPolygon.CV, newPolyCorners);
                if (Log.loggingDebug)
                    log.debug("PathSynth.generateConvexPolygons: new poly corner count " + newPolyCorners.size() + 
                        ", first corner " + firstNewPolyCorner + ", last corner " + lastNewPolyCorner + ",  " + newPoly);
                if (newPolyCorners.size() < 3) {
                    log.error("PathSynth.generateConvexPolygons: newPoly has just " + newPolyCorners.size() + " corners.");
                    return;
                }
                polygons.add(newPoly);
                // Now delete the relevant points from the original polygon
                // remove the points that got moved to the new poly
                int i = wrap(firstNewPolyCorner + 1, size);
                int count = newPolyCorners.size() - 2;
                for (int j=0; j<count; j++) {
                    originalCorners.remove(i);
                    i = wrap(i, originalCorners.size());
                }
                if (Log.loggingDebug)
                    log.debug("PathSynth.generateConvexPolygons: original Poly " + poly);
                if (originalCorners.size() < 3) {
                    log.error("PathSynth.generateConvexPolygons: original Poly has just " + originalCorners.size() + " corners.");
                    return;
                }
                // Finally, check the original polygon one more time.
                generateConvexPolygons(poly);
                break;
            }
        }
    }

    protected void dumpPolygonsAndArcs(String description) {
        log.info(description + ": " + polygons.size() +  " polygons, " + arcs.size() + " arcs.");
        for (PathPolygon poly : polygons)
            log.info(description + ": Polygon " + poly);
        for (PathArc arc : arcs)
            log.info(description + ": Arc " + arc);
    }

    String modelName;
    String type;
    int firstTerrainIndex;
    PathPolygon boundingPolygon;
    List<PathPolygon> polygons;
    List<PathArc> portals;
    List<PathArc> arcs;
    Map<Integer, List<PathArc>> polygonArcs = null;
    Map<Integer, PathPolygon> polygonMap = null;
    LinkedList<PathPolygon> terrainPolygonAtCorner = null;

    /**
     * If a distance squared between a pair of MVVector points is less
     * than epsilon, they are considered the same point.
     */
    public static final float epsilon = 1.0f;

    // The "tolerance" distance, in millimeters.  If the distance from
    // a from the plane of a polygon to a point is less than or equal
    // to this distance, the point is considered "inside" the polygon.
    protected static float insideDistance = 100.0f;
    
    protected static final Logger log = new Logger("PathSynth");
    private static final long serialVersionUID = 1L;

    private static void test1() {
        log.info("PathSynth.main: Starting test1");
        // Create a boundary polygon
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(750f, 0f, 250f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(250f, 0f, 750f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        // Finally create the PathSynth, which invokes all the machinery
        PathSynth obj = new PathSynth(boundary, obstacles);
        obj.dumpPolygonsAndArcs("test1");
        log.info("");
    }
    
    private static void test2() {
        log.info("PathSynth.main: Starting test2");
        // Create a boundary polygon
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(500f, 0f, 250f));
        corners.add(new MVVector(750f, 0f, 500f));
        corners.add(new MVVector(500f, 0f, 750f));
        corners.add(new MVVector(250f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        // Finally create the PathSynth, which invokes all the machinery
        PathSynth obj = new PathSynth(boundary, obstacles);
        obj.dumpPolygonsAndArcs("test2");
        log.info("");
    }
    
    private static void test3() {
        log.info("PathSynth.main: Starting test3");
        // Create a boundary polygon
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(400f, 0f, 200f));
        corners.add(new MVVector(750f, 0f, 250f));
        corners.add(new MVVector(800f, 0f, 500f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(500f, 0f, 800f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(200f, 0f, 400f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        // Finally create the PathSynth, which invokes all the machinery
        PathSynth obj = new PathSynth(boundary, obstacles);
        obj.dumpPolygonsAndArcs("test3");
        log.info("");
    }
    
    /**
     * Calls a set of test cases for the generation of polygons and
     * arcs from a boundary and obstacle.
     */
    public static void main(String[] args) {
        Properties props = new Properties();
        props.put("log4j.appender.FILE", "org.apache.log4j.RollingFileAppender");
        props.put("log4j.appender.FILE.File", "${multiverse.logs}/pathing.out");
        props.put("log4j.appender.FILE.MaxFileSize", "50MB");
        props.put("log4j.appender.FILE.layout", "org.apache.log4j.PatternLayout");
        props.put("log4j.appender.FILE.layout.ConversionPattern", "%-5p %m%n");
        props.put("multiverse.log_level", "0");
        props.put("log4j.rootLogger", "DEBUG, FILE");
        Log.init(props);
        
        test1();
        test2();
        test3();
    }
}
