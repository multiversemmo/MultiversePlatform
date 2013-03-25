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
 * used in pathing code
 * works with java bean xml serialization
 */
public class PathObject implements Serializable, Cloneable {

    public PathObject() {
    }
    
    public PathObject(String modelName,
                      String type,
                      int firstTerrainIndex,
                      PathPolygon boundingPolygon,
                      List<PathPolygon> polygons,
                      List<PathArc> portals, 
                      List<PathArc> arcs) {
        this.modelName = modelName;
        this.type = type;
        this.firstTerrainIndex = firstTerrainIndex;
        this.boundingPolygon = boundingPolygon;
        this.polygons = polygons;
        this.portals = portals;
        this.arcs = arcs;
        // Add the portals to the list of arcs
        for (PathArc arc : this.portals)
            this.arcs.add(arc);
        findTerrainPolygonsAtCorners();
    }
    
    /**
     * Generate the pathing metadata for avatars of the given width
     * moving inside the boundary around the obstacles.
     */
    public PathObject(String description, float avatarWidth, List<MVVector> boundaryCorners, List<List<MVVector>> obstacleCorners) {
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, boundaryCorners).ensureWindingOrder(true);
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        for (List<MVVector> corners : obstacleCorners)
            obstacles.add(new PathPolygon(0, PathPolygon.CV, corners).ensureWindingOrder(false));
        
        // Check obstacles for overlap with each other, and merge them
        // if they do overlap.

        // Check the (possibly merged) obstacles for overlap with
        // the boundary, and clip them if they do overlap.
        
        Triangulate triangulator = new Triangulate();
        List<PathPolygon> triangles = triangulator.computeTriangulation(description, boundary, obstacles);
        
        // Merge the triangles to create larger convex polygons
        polygons = aggregateTriangles(triangles);

        // Create the arcs
        arcs = discoverArcs(polygons);
    }

    /**
     * Aggregate triangles into convex polygons.
     */
    protected static List<PathPolygon> aggregateTriangles(List<PathPolygon> triangles) {
        List<PathPolygon> polys = new LinkedList<PathPolygon>();
        while (triangles.size() > 0) {
            PathPolygon poly = triangles.remove(0);
            polys.add(poly);
            List<MVVector> polyCorners = poly.getCorners();
            boolean foundOne = false;
            do {
                foundOne = false;
                int polySize = polyCorners.size();
                outerLoop:
                for (int pc=0; pc<polySize; pc++) {
                    MVVector pcCorner1 = polyCorners.get(pc);
                    int pcPlus = PathSynth.wrap(pc + 1, polySize);
                    MVVector pcCorner2 = polyCorners.get(pcPlus);
                    for (PathPolygon triangle : triangles) {
                        List<MVVector> triCorners = triangle.getCorners();
                        for (int tc=0; tc<3; tc++) {
                            MVVector triCorner1 = triCorners.get(tc);
                            int tcPlus = PathSynth.wrap(tc + 1, 3);
                            MVVector triCorner2 = triCorners.get(tcPlus);
                            MVVector triCorner3 = triCorners.get(PathSynth.wrap(tc + 2, 3));
                            if (((MVVector.distanceToSquared(pcCorner1, triCorner1) < PathSynth.epsilon && 
                                  MVVector.distanceToSquared(pcCorner2, triCorner2) < PathSynth.epsilon) ||
                                 (MVVector.distanceToSquared(pcCorner2, triCorner1) < PathSynth.epsilon && 
                                  MVVector.distanceToSquared(pcCorner1, triCorner2) < PathSynth.epsilon)) &&
                                MVVector.counterClockwisePoints(pcCorner1, triCorner3, pcCorner2)) {
                                // Found it: add the corner to the poly between pcCorner1 and pcCorner2
                                foundOne = true;
                                polyCorners.add(pcPlus, triCorner3);
                                break outerLoop;
                            }
                        }
                    }
                }
            }
            while (foundOne);
        }
        // Reassign index numbers for the polys
        int i=1;
        for (PathPolygon poly : polys)
            poly.setIndex(i++);
        return polys;
    }

    /**
     * A quadruple loop over the polygon edges, looking for common
     * ones to find arcs between polygons.
     */
    protected static List<PathArc> discoverArcs(List<PathPolygon> polygons) {
        int s = polygons.size();
        List<PathArc> arcs = new LinkedList<PathArc>();
        for (int p1=0; p1<s; p1++) {
            PathPolygon poly1 = polygons.get(p1);
            List<MVVector> corners1 = poly1.getCorners();
            int size1 = corners1.size();
            for (int pc1=0; pc1<size1; pc1++) {
                MVVector corner11 = corners1.get(pc1);
                int pc1Plus = PathSynth.wrap(pc1 + 1, size1);
                MVVector corner12 = corners1.get(pc1Plus);
                for (int p2=p1+1; p2<s; p2++) {
                    PathPolygon poly2 = polygons.get(p2);
                    List<MVVector> corners2 = poly2.getCorners();
                    int size2 = corners2.size();
                    for (int pc2=0; pc2<size2; pc2++) {
                        MVVector corner21 = corners2.get(pc2);
                        int pc2Plus = PathSynth.wrap(pc2 + 1, size2);
                        MVVector corner22 = corners2.get(pc2Plus);
                        if ((MVVector.distanceToSquared(corner11, corner21) < PathSynth.epsilon && 
                             MVVector.distanceToSquared(corner12, corner22) < PathSynth.epsilon) ||
                            (MVVector.distanceToSquared(corner12, corner21) < PathSynth.epsilon && 
                             MVVector.distanceToSquared(corner11, corner22) < PathSynth.epsilon)) {
                            // We have an arc, so make it in both directions
//                             if (Log.loggingDebug) {
//                                 log.debug("Adding arc: pc1 " + pc1 + ", pc1Plus " + pc1Plus + ", pc2 " + pc2 + ", pc2Plus " + pc2Plus);
//                                 log.debug("Adding arc: corner11 " + corner11 + ", corner12 " + corner12 + ", corner21 " + corner21 + ", corner22 " + corner22);
//                             }
                            PathEdge edge = new PathEdge(corner11, corner12);
                            PathArc arc1 = new PathArc(PathArc.CVToCV, poly1.getIndex(), poly2.getIndex(), edge);
                            PathArc arc2 = new PathArc(PathArc.CVToCV, poly2.getIndex(), poly1.getIndex(), edge);
                            arcs.add(arc1);
                            arcs.add(arc2);
                        }
                    }
                }
            }
        }
        return arcs;
    }

   protected void findTerrainPolygonsAtCorners() {
    	List<MVVector> corners = boundingPolygon.getCorners();
        int count = corners.size();
        terrainPolygonAtCorner = new LinkedList<PathPolygon>();
        for (int i=0; i<count; i++) {
            MVVector corner = corners.get(i);
            for (PathPolygon polygon : polygons) {
                if (polygon.getKind() != PathPolygon.Terrain)
                    continue;
                List<MVVector> pcorners = polygon.getCorners();
                int pcount = pcorners.size();
                for (int j=0; j<pcount; j++) {
                    MVVector c = pcorners.get(j);
                    float dx = corner.getX() - c.getX();
                    float dz = corner.getZ() - c.getZ();
//                     if (logAll)
//                         log.debug("findTerrainPolygonsAtCorners: corner = " + corner + 
//                             "; pcorners[j] = " + pcorners[j] + "; dx = " + dx + "; dz = " + dz);
                    if (dx * dx + dz * dz < 50f) {
                        terrainPolygonAtCorner.add(polygon);
//                         if (logAll)
//                             log.debug("findTerrainPolygonsAtCorners, found! corner = " + corner + "; pcorners[j] = " + pcorners[j]);
                        break;
                    }
                }
            }
            if (terrainPolygonAtCorner.get(i) == null)
                log.error("findTerrainPolygonsAtCorners: could not find terrain polygon for corner " + i);
        }
    }

    public PathPolygon getCVPolygon(int polyIndex) {
        PathPolygon polygon = getPolygon(polyIndex);
        if (polygon != null)
            assert polygon.getKind() == PathPolygon.CV;
        return polygon;
    }
    
    public PathPolygon getTerrainPolygon(int polyIndex) {
        PathPolygon polygon = getPolygon(polyIndex);
        if (polygon != null)
            assert polygon.getKind() == PathPolygon.Terrain;
        return polygon;
    }
    
    public boolean isTerrainPolygon(int polyIndex) {
        PathPolygon polygon = getPolygon(polyIndex);
        if (polygon == null) {
            log.error("polygonTerrainStringChar: no polygon at index " + polyIndex);
            return true;
        }
        else
            return polygon.getKind() == PathPolygon.Terrain;
    }
    
    public PathPolygon getTerrainPolygonAtCorner(int cornerNumber) {
        return terrainPolygonAtCorner.get(cornerNumber);
    }
    
    // Return the number of the corner closest to loc
    public int getClosestCornerToPoint(MVVector loc) {
    	return boundingPolygon.getClosestCornerToPoint(loc);
    }

    public PathPolygon getPolygon(int polyIndex) {
        // If we haven't already generated them, do so now.
        if (polygonMap == null)
            createPolygonMap();
        if (polygonMap.containsKey(polyIndex))
            return polygonMap.get(polyIndex);
        else
            return null;
    }

    protected void createPolygonMap() {
        polygonMap = new HashMap<Integer, PathPolygon>();
        for (PathPolygon polygon : polygons)
            polygonMap.put(polygon.getIndex(), polygon);
    }
    
    // Return the index of the collision volume polygon containing the
    // location, or -1 if there is none.
    public int findCVPolygonAtLocation(MVVector loc) {
    	MVVector floc = new MVVector(loc);
        for (PathPolygon polygon : polygons) {
            if (polygon.getKind() == PathPolygon.CV && polygon.pointInside(floc, insideDistance))
                return polygon.getIndex();
        }
        return -1;
    }

    // Return the index of the terrain polygon containing the
    // location, or -1 if there is none.
    public int findTerrainPolygonAtLocation(MVVector loc) {
    	MVVector floc = new MVVector(loc);
        for (PathPolygon polygon : polygons) {
            if (polygon.getKind() == PathPolygon.Terrain && polygon.pointInside(floc, insideDistance))
                return polygon.getIndex();
        }
        return -1;
    }

    // Return a PathIntersection object describing the closest
    // intersection with any cvPolygon, or null if there is none.
    public PathIntersection closestIntersection(MVVector loc1, MVVector loc2) {
        PathIntersection closest = null;
        for (PathPolygon cvPoly : polygons) {
            if (cvPoly.getKind() != PathPolygon.CV)
                continue;
            PathIntersection intersection = cvPoly.closestIntersection(this, loc1, loc2);
            if (intersection == null)
                continue;
            if (closest == null || intersection.getWhere1() < closest.getWhere1())
                closest = intersection;
        }
        return closest;
    }

    public List<PathArc> getPolygonArcs (int polyIndex) {
        if (logAll)
            log.debug("getPolygonArcs: Entering");
        // If we haven't already generated them, do so now.
        if (polygonArcs == null) {
            polygonArcs = new HashMap<Integer, List<PathArc>>();
            for (PathArc arc : arcs) {
                addToArcMap(polygonArcs, arc, arc.getPoly1Index());
                addToArcMap(polygonArcs, arc, arc.getPoly2Index());
            }
        }
        if (polygonArcs.containsKey(polyIndex)) {
            List<PathArc> parcs = polygonArcs.get(polyIndex);
            if (logAll)
                log.debug("getPolygonArcs: returning parcs.size() = " + parcs.size());
            return parcs;
        }
        else
            return null;
    }       
    
    private void addToArcMap(Map<Integer, List<PathArc>> polygonArcs, PathArc arc, int polyIndex) {
        List<PathArc> parcs = null;
        if (!polygonArcs.containsKey(polyIndex)) {
            parcs = new LinkedList<PathArc>();
            polygonArcs.put(polyIndex, parcs);
        }
        else
            parcs = polygonArcs.get(polyIndex);
        parcs.add(arc);
    }

    public String toString() {
        return "[PathObject modelName=" + getModelName() + "; type=" + type + 
            "; boundingPolygon = " + boundingPolygon + "]";
    }

    public Object clone() {
        return new PathObject(getModelName(), getType(), getFirstTerrainIndex(), boundingPolygon, getPolygons(), 
            getPortals(), getArcs());
    }

    public String getModelName() {
        return modelName;
    }
    
    public String getType() {
        return type;
    }
    
    public int getFirstTerrainIndex() {
        return firstTerrainIndex;
    }
    
    public MVVector getCenter() {
        List<MVVector> corners = boundingPolygon.getCorners();
        MVVector ll = corners.get(0);
        MVVector ur = corners.get(2);
        MVVector center = new MVVector((ll.getX() + ur.getX()) * 0.5f,
                                 (ll.getY() + ur.getY()) * 0.5f,
                                 (ll.getZ() + ur.getZ()) * 0.5f);
        if (logAll)
            log.debug("getCenter: center = " + center);
        return center;
    }
    
    public int getRadius() {
        List<MVVector> corners = boundingPolygon.getCorners();
        MVVector ll = corners.get(0);
        MVVector ur = corners.get(2);
        int radius = (int)(MVVector.distanceTo(ll, ur) / 2);
        if (logAll)
            log.debug("getRadius: pathObject = " + this + "; radius = " + radius);
        return radius;
    }

    public PathPolygon getBoundingPolygon() {
        return boundingPolygon;
    }
    
    public List<PathPolygon> getPolygons(){
        return polygons;
    }
    
    public List<PathArc> getPortals() {
        return portals;
    }
    
    public List<PathArc> getArcs() {
        return arcs;
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

    // The "tolerance" distance, in millimeters.  If the distance from
    // a from the plane of a polygon to a point is less than or equal
    // to this distance, the point is considered "inside" the polygon.
    protected static float insideDistance = 100.0f;
    
    protected static final Logger log = new Logger("PathObject");
    protected static boolean logAll = false;
    private static final long serialVersionUID = 1L;
}

