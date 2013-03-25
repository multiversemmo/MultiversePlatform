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

////////////////////////////////////////////////////////////////////////
//
// This class provides the external API to the pathing system for
// pathfinding within rooms.
//
// The sole operation is the static findPathInRoom(), returning a 
// List<Point> of the Points in the path.
//
////////////////////////////////////////////////////////////////////////

public class RoomPathSearcher {
    
    protected RoomPathSearcher() {
    }
    
    public static List<MVVector> findPathInRoom(MVVector loc1, MVVector loc2, Quaternion endOrientation,
        PathPolygon room, List<PathPolygon> obstacles, float playerWidth) {

        float halfWidth = playerWidth * 0.5f;
        List<MVVector> path = new LinkedList<MVVector>();
        path.add(loc1);
        MVVector next = loc1;

        // If we haven't found a path after dodging 100 obstacles,
        // something is seriously wrong.
        int i;
        int limit = 100;
        for (i=0; i<limit; i++) {
            PathIntersection intersection = findFirstObstacle(next, loc2, room, obstacles);
            if (intersection == null)
                break;
            next = findPathAroundObstacle(intersection, next, loc2, room, halfWidth);
            if (next == null)
                return path;
            path.add(next);
        }
        return path;
    }

    // Get the set of obstacles between the two locations from the
    // quad tree.  If the set is empty, return null.  Else find the
    // set element closest to loc1 and return it
    protected static PathIntersection findFirstObstacle(MVVector loc1, MVVector loc2, PathPolygon room, List<PathPolygon> obstacles) {
        if (logAll)
            log.debug("findFirstObstacle: loc1 = " + loc1 + "; loc2 = " + loc2);
        List<PathPolygon> elems = getElementsBetween(loc1, loc2, obstacles);
        if (logAll)
            log.debug("findFirstObstacle: elems = " + (elems == null ? elems : elems.size()));
        if (elems == null || elems.size() == 0) 
            return null;
        while (true) {
            PathIntersection closest = null;
            PathPolygon closestElem = null;
            for (PathPolygon elem : elems) {
                if (logAll)
                    log.debug("findFirstObstacle elem = " + elem);
                PathIntersection intersection =
                    elem.closestIntersection(null, loc1, loc2);
                if (intersection != null && 
                    (closest == null || intersection.getWhere1() < closest.getWhere1())) {
                    closest = intersection;
                    closestElem = elem;
                }
            }
            if (closest == null)
                return null;
            PathIntersection pathObjectClosest = closestElem.closestIntersection(null, loc1, loc2);
            if (pathObjectClosest != null) {
                if (logAll)
                    log.debug("findFirstObstacle: pathObjectClosest = " + pathObjectClosest);
                return pathObjectClosest;
            }
            else
                elems.remove(closestElem);
        }
    }

    protected static MVVector findPathAroundObstacle(PathIntersection intersection, MVVector loc1, MVVector loc2, PathPolygon room, float halfWidth) {
        // New approach: find the closest corners to each of loc1 and
        // loc2, and then use the AStar algorithm to get around the obstacle
        PathPolygon poly = intersection.getCVPoly();
        int corner1 = poly.getClosestCornerToPoint(loc1);
        MVVector cornerPoint1 = poly.getCorners().get(corner1);
        int corner2 = poly.getClosestCornerToPoint(loc2);
        MVVector endPoint = poly.getCorners().get(corner2);
        if (logAll)
            log.debug("findPathAroundObstacle: loc1 = " + loc1 + "; corner1 = " + corner1 + 
                "; cornerPoint1 = " + cornerPoint1 + 
                "; loc2 = " + loc2 + "; endPoint = " + endPoint + 
                "; endPoint = " + endPoint);
        return endPoint;
    }
    
    protected static List<PathPolygon> getElementsBetween(MVVector loc1, MVVector loc2, List<PathPolygon> obstacles) {
        List<PathPolygon> intersectors = new LinkedList<PathPolygon>();
        for (PathPolygon poly : obstacles) {
            if (poly.closestIntersection(null, loc1, loc2) != null)
                intersectors.add(poly);
        }
        return intersectors;
    }
    
    static boolean logAll = true;
    protected static final Logger log = new Logger("RoomPathSearcher");
}

    
