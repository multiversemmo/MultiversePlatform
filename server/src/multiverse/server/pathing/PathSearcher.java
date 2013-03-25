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
import multiverse.server.engine.*;
import multiverse.server.util.*;

////////////////////////////////////////////////////////////////////////
//
// This class provides the external API to the pathing system.  It is
// a singleton class; to get the single instance, call getInstance().
//
// The operations are:
//
//  o void createpathSearcher(PathInfo pathInfo, Geometry worldGeometry),
//    called to create the singleton instance
//
//  o boolean legalPosition(String pathObjectType, MVVector location),
//    returning true if the location is legal for a an object whose
//    dimensions are given by the named path object type.
//
//  o PathResult findPath(String type, List<MVVector> path, MVVector loc1, MVVector loc2),
//    called to find the path between loc1 and loc2 for creatures whose
//    dimensions are given by the named path object type.  The return value is
//    a PathResult enum; the success value is PathResult.OK; all other values 
//    indicate that there was no path between loc1 and loc2.  If there is such
//    a path, List<MVVector> path argument will be filled in.
//
////////////////////////////////////////////////////////////////////////

public class PathSearcher {
    
    protected PathSearcher(PathInfo pathInfo, Geometry geometry) {
        this.pathInfo = pathInfo;
        if (pathInfo != null) {
            Map<String, PathData> pathDictionary = pathInfo.getPathDictionary();
            int count = pathDictionary.size();
            if (count > 0) {
                if (logAll)
                    log.debug("PathSearcher: pathDictionary.size() = " + count);
                buildQuadTrees(geometry);
            }
        }
    }

    public static PathSearcher getInstance() {
        return instance;
    }
    
    public static void createPathSearcher(PathInfo pathInfo, Geometry geometry) {
        instance = new PathSearcher(pathInfo, geometry);
    }
    
    public enum PathResult {
        Illegal((byte)0),
        OK((byte)1),
        ExitModelPath((byte)2),
        TerrainPath((byte)3),
        EntryModelPath((byte)4);

        PathResult(byte val) {
            this.val = val;
        }

        public String toString() {
            switch (val) {
            case 0: 
                return "Illegal";
            case 1: 
                return "Success";
            case 2: 
                return "Failure - could not calculate exit model path";
            case 3:
                return "Failure - could not calculate terrain crossing path";
            case 4:
                return "Failure - could not calculate entry model path";
            default:
                return "Failure - unknown PathResult value " + val;
            }
        }
        
        byte val = -1;
    }

    // This method iterates over the full set of pathing data,
    // creating one quad tree for each of the path object type
    // names encountered, then add the models' path objects to the
    // quad tree, with radius equal to the radius of the model
    // bounding box.  We use the interface to the quad tree that 
    protected void buildQuadTrees(Geometry geometry) {
        quadTrees = new HashMap<String, QuadTree<PathModelElement>>();
        Iterator<Map.Entry<String, PathData>> iter = 
            pathInfo.getPathDictionary().entrySet().iterator();
 	if (logAll)
            log.debug("buildQuadTrees: pathInfo.getPathDictionary().size() = " + 
                pathInfo.getPathDictionary().size());
        while (iter.hasNext()) {
            Map.Entry<String, PathData> entry = iter.next();
            PathData pathData = entry.getValue();
            List<PathObject> pathObjects = pathData.getPathObjects();
            if (logAll)
                log.debug("buildQuadTrees: pathData " + entry.getKey() + " has " +
                    pathObjects.size() + " path objects");
            for (PathObject pathObject : pathObjects) {
                String type = pathObject.getType();
                if (!quadTrees.containsKey(type))
                    quadTrees.put(type, new QuadTree<PathModelElement>(geometry));
                QuadTree<PathModelElement> tree = quadTrees.get(type);
                try {
                    if (logAll)
                        log.debug("buildQuadTrees: Adding pathObject " + pathObject + " with center " + 
                            pathObject.getCenter() + " and radius " + pathObject.getRadius());
                    tree.addElement(new PathModelElement(pathObject));
                } catch (MVRuntimeException e) {
                	log.error("In PathSearcher.buildQuadTree, exception '" + 
                            e.getMessage() + "' thrown");
                }
            }
        }
    }

    protected QuadTree<PathModelElement> findQuadTreeForType(String type) {
        if (!quadTrees.containsKey(type)) {
            log.error("PathSearch.findModelsAtLocation: no path object type '" + type + "'!");
            return null;
        }
        return quadTrees.get(type);
    }

    protected PathObject findModelAtLocation(String type, MVVector loc) {
        if (logAll)
            log.debug("findModelAtLocation: type = " + type + "; loc = " + loc);
        QuadTree<PathModelElement> tree = findQuadTreeForType(type);
        if (tree == null)
            return null;
        // Get the models in the tree that are within 1 mm of the location
        try {
            Set<PathModelElement> elements = tree.getElements(new Point(loc), 1);
            if (logAll)
                log.debug("findModelAtLocation: elements.size() = " + elements.size());
            for (PathModelElement elt : elements) {
                PathObject pathObject = (PathObject)elt.getQuadTreeObject();
                // If the location is inside the bounding polygon of this path object, 
                // this is certain to be the containing path object.
                if (logAll)
                    log.debug("findModelAtLocation: Checking pointInside2D, loc = " + 
                        loc + "; pathObject = " + pathObject);
                if (pathObject.getBoundingPolygon().pointInside2D(loc)) {
                    if (logAll)
                        log.debug("findModelAtLocation: returning pathobject = " + pathObject);
                    return pathObject;
                }
            }
            return null;
        } catch (Exception e) {
        	log.error("In PathSearcher.findModelsAtLocation, the quad tree threw error '" + e.getMessage() + "'!");
        	return null;
        }
    }

    // If the location is inside a model, or on the terrain inside the
    // model area, this method returns an instance that describes
    // where.  Otherwise, it returns null.
    public PathObjectLocation findModelLocation(String type, MVVector loc) {
        PathObject pathObject = findModelAtLocation(type, loc);
        if (pathObject == null)
            return null;
        else {
            PathObjectLocation pathLoc = findModelLocation(pathObject, loc);
            if (pathLoc != null)
                return pathLoc;
            else
                return new PathObjectLocation(pathObject, loc, PathPolygon.Illegal, -1);
        }
    }
    
    public static PathObjectLocation findModelLocation(PathObject pathObject, MVVector loc) {
        int polyIndex = pathObject.findCVPolygonAtLocation(loc);
        if (polyIndex >= 0)
            return new PathObjectLocation(pathObject, loc, PathPolygon.CV, polyIndex);
        polyIndex = pathObject.findTerrainPolygonAtLocation(loc);
        if (polyIndex >= 0)
            return new PathObjectLocation(pathObject, loc, PathPolygon.Terrain, polyIndex);
        else
            return null;
    }
        
    // Is the given location legal?  It's illegal if it's inside a
    // model at an illegal location
    public boolean legalPosition(String type, MVVector loc) {
        PathObjectLocation poLocation = findModelLocation(type, loc);
        return (poLocation == null || poLocation.getKind() != PathPolygon.Illegal);
    }
        
    public static PathFinderValue findPath(String type, MVVector loc1, MVVector loc2, boolean followsTerrain) {
        boolean failed = false;
        String why = "";
        if (instance == null) {
            failed = true;
            why = "PathSearcher instance not initialized";
        }
        else if (instance.getPathInfo() == null) {
            failed = true;
            why = "PathSearcher PathInfo member is null";
        }
        else if (!instance.getPathInfo().pathObjectTypeSupported(type)) {
            failed = true;
            why = "path object type '" + type + "' unrecognized!";
        }
        if (failed) {
//             log.warn("In PathSearcher.findPath, " + why);
            List<MVVector> path = new LinkedList<MVVector>();
            path.add(loc1);
            path.add(loc2);
            // Kludge city - - cause the terrain string to say "not
            // terrain", since that will prevent the path
            // interpolators from setting the Y coordinate of the mob
            // to 0
            String terrainString = (followsTerrain ? "TT" : "CC");
            if (Log.loggingDebug)
                Log.debug("PathServer.findPath: didn't find path because " + why);
            return new PathFinderValue(PathResult.OK, path, terrainString);
        }
        else
            return instance.findPathInternal(type, loc1, loc2);
    }

    /**
     * This entrypoint is used only for in-room player pathing.
     */
    public static PathFinderValue findPath(long playerOid, PathObject pathObject, MVVector loc1, MVVector loc2, float halfWidth) {
        PathObjectLocation poLoc1 = findModelLocation(pathObject, loc1);
        if (poLoc1 == null) {
            log.error("PathSearcher.findPath: Could not find start " + loc1 + " in PathObject " + pathObject);
            return null;
        }
        PathObjectLocation poLoc2 = findModelLocation(pathObject, loc2);
        if (poLoc2 == null) {
            log.error("PathSearcher.findPath: Could not find dest " + loc2 + " in PathObject " + pathObject);
            return null;
        }
        PathFinderValue value = new PathFinderValue(PathResult.OK, new LinkedList<MVVector>(), "");
        if (PathAStarSearcher.findPathInModel(value, poLoc1, poLoc2, halfWidth))
            return value;
        else
            return null;
    }

    // The heart of the system: find a legal path between a pair of
    // points, or return null if there is no legal path

    // The general case of a path is:
    // a. Egress from starting CVPolygon location to a portal, using AStar algorithm
    // b. Move far enough past CVTerrain location at the portal to be in open terrain
    // c. Move across open terrain to a point adjacent to an entering portal 
    //    in the ending location, using findPathThroughTerrain method.
    // d. Traverse the terrain-to-CV portal ending up just inside the portal.
    // e. Move to CV polygon destination, using AStar algorithm
    //
    // If we start out in the terrain, we just have c, d & e.
    // If we're going to the terrain, we just have a, b & c.
    // 
    private PathFinderValue findPathInternal(String type, MVVector loc1, MVVector loc2) {
        // If we don't recognize this type, show an error and return the usual path
        if (Log.loggingDebug)
            log.debug("findPathInternal: type = " + type + "; loc1 = " + loc1 + "; loc2 = " + loc2);
        List<MVVector> path = new LinkedList<MVVector>();
        PathFinderValue value = new PathFinderValue(PathResult.OK, path, "");
        PathObjectLocation poLoc1 = findModelLocation(type, loc1);
        PathObjectLocation poLoc2 = findModelLocation(type, loc2);
        if (logAll)
            log.debug("findPathInternal: poLoc1 = " + poLoc1 + "; poLoc2 = " + poLoc2);
        // If they are both null, then they are both out in the
        // terrain, so there is a legal path between them
        float halfWidth = pathInfo.getTypeHalfWidth(type);
        if (poLoc1 == null && poLoc2 == null) {
            PathResult result = findPathThroughTerrain(value, type, loc1, loc2, null, null, halfWidth, false, false);
            value.setResult(result);
            return value;
        }
        PathObject p1 = (poLoc1 != null ? poLoc1.getPathObject() : null);
        PathObject p2 = (poLoc2 != null ? poLoc2.getPathObject() : null);
        if (logAll && p1 != null)
            log.debug("findPathInternal p1 boundingPolygon = " + p1.getBoundingPolygon());
        if (logAll && p2 != null)
            log.debug("findPathInternal p2 boundingPolygon = " + p2.getBoundingPolygon());
        boolean sameModel = p1 != null && p1 == p2;
        if (sameModel) {
            // false return value means that there is no path
            if (!findPathInModel(value, poLoc1, poLoc2, halfWidth)) {
                if (logAll)
                    log.debug("No path in model from " + loc1 + " to " + loc2);
                value.setResult(PathResult.ExitModelPath);
                return value;
            }
        }
        boolean needEgressFromStartModel = p1 != null && poLoc1.getKind() == PathPolygon.CV;
        boolean needToCrossTerrain = p1 == null || !sameModel;
        boolean needEntryToEndModel = p2 != null && poLoc2.getKind() == PathPolygon.CV;
        if (logAll)
            log.debug("findPathInternal: startModel = " + tOrF(needEgressFromStartModel) +
                "; crossTerrain = " + tOrF(needToCrossTerrain) + "; endModel = " + tOrF(needEntryToEndModel));
        PathArc exitPortal = null;
        MVVector exitPortalLoc = null;
        PathArc entryPortal = null;
        MVVector entryPortalLoc = null;
        int pathSize;
        if (needEgressFromStartModel) {
            exitPortal = findPortalClosestToLoc(p1, loc2);
            exitPortalLoc = makeTerrainLocationFromPortal(p1, exitPortal, loc2, halfWidth);
            if (Log.loggingDebug)
                log.debug("findPathInternal exitPortal = " + exitPortal + "; exitPortalLoc = " + exitPortalLoc);
            pathSize = path.size();
            if (!findPathToPortal(value, halfWidth, poLoc1, exitPortal, exitPortalLoc)) {
                if (logAll)
                    log.debug("No path in model from " + loc1 + " to exit portal " + 
                        exitPortal + " at location " + exitPortalLoc);
                value.setResult(PathResult.ExitModelPath);
                return value;
            }
            dumpAddedPathElements("Exiting model1", value, pathSize);
        }
        if (needEntryToEndModel) {
            entryPortal = findPortalClosestToLoc(p2, loc1);
            entryPortalLoc = makeTerrainLocationFromPortal(p2, entryPortal, loc1, halfWidth);
            if (logAll)
                log.debug("findPathInternal entryPortal = " + entryPortal + "; entryPortalLoc = " + entryPortalLoc);
        }
        if (needToCrossTerrain) {
            MVVector tloc1 = (p1 != null && exitPortalLoc != null ? exitPortalLoc : loc1);
            MVVector tloc2 = (p2 != null && entryPortalLoc != null ? entryPortalLoc : loc2);
            pathSize = path.size();
            // This also finds the path through into the last model, if any
            PathResult result = findPathThroughTerrain(value, type, tloc1, tloc2, poLoc2, entryPortalLoc, halfWidth,
                                                       needEgressFromStartModel, needEntryToEndModel);
            if (result != PathResult.OK) {
                if (logAll)
                    log.debug("findPathInternal: No path through terrain from " + tloc1 + " to " + tloc2);
                value.setResult(result);
                return value;
            }
            dumpAddedPathElements("Going through terrain", value, pathSize);
        }
        value.setResult(PathResult.OK);
        return value;
    }
    
    String tOrF(boolean value) {
        return value ? "true" : "false";
    }
    
    PathArc findPortalClosestToLoc(PathObject p, MVVector loc) {
        PathArc closestPortal = null;
        float closestDistance = Float.MAX_VALUE;
        for (PathArc portal : p.getPortals()) {
            float d = MVVector.distanceTo(loc, portal.getEdge().getMidpoint());
            if (d < closestDistance) {
                closestDistance = d;
                closestPortal = portal;
            }
        }
        return closestPortal;
    }

    void dumpAddedPathElements(String heading, PathFinderValue value, int firstElt) {
        String s = value.stringPath(firstElt);
        if (Log.loggingDebug)
            log.debug("dumpAddedPathElements for " + heading + ": " + s);
    }

    // Return a location that is on the terrain side of the portal,
    // halfWidth from the side of the portal that is closest to loc,
    // and halfWidth from the portal edge
    MVVector makeTerrainLocationFromPortal(PathObject pathObject, PathArc portal, MVVector loc, float halfWidth) {
        PathEdge edge = portal.getEdge();
        MVVector start = edge.getStart();
        MVVector end = edge.getEnd();
        // Find the closest end of the edge to loc
        boolean startClosest = MVVector.distanceTo(loc, start) < MVVector.distanceTo(loc, end);
        // Find the location along the portal edge which is halfWidth
        // from the end closest to loc
        MVVector n = new MVVector(end);
        n.sub(start);
        n.setY(0f);
        n.normalize();
        MVVector p;
        if (startClosest) {
            p = n.times(halfWidth);
            p.add(start);
        }
        else {
            p = n.times(-halfWidth);
            p.add(end);
        }
        // Now add a vector to p that moves us halfWidth away from the
        // cvPolygon.  The cvPolygon is always clockwise from the
        // vector from start to end
        PathPolygon cvPolygon = pathObject.getCVPolygon(portal.getPoly1Index());
        PathPolygon terrainPolygon = pathObject.getTerrainPolygon(portal.getPoly2Index());
        MVVector cvCentroid = cvPolygon.getCentroid();
        MVVector terrainCentroid = terrainPolygon.getCentroid();
        float temp = n.getX();
        n.setX(n.getY());
        n.setY(- temp);
        MVVector q = new MVVector(terrainCentroid);
        q.sub(cvCentroid);
        // If the dot product of n and the vector from the cvPolygon
        // to the terrain is less than zero, we need to reverse the n
        // vector
        if (q.dotProduct(n) < 0)
            n.times(-halfWidth);
        else
            n.times(halfWidth);
        p.add(n);
        return new MVVector(p);
    }
    
    // loc1 and loc2 are both points in the terrain.  Find a path
    // through the terrain from loc1 to loc2, dodging any intervening
    // obstacles
    public PathResult findPathThroughTerrain(PathFinderValue value, String type,
                                             MVVector loc1, MVVector loc2, PathObjectLocation poLoc2, MVVector entryPortalLoc,
                                             float halfWidth, boolean haveStartModel, boolean haveEndModel) {
        if (logAll)
            log.debug("findPathThroughTerrain loc1 = " + loc1 + "; loc2 = " + loc2);
        List<MVVector> path = value.getPath();
        int pathSize = path.size();
        if (!haveStartModel)
            value.addPathElement(loc1, true);
        MVVector next = loc1;
        // If we haven't found a path after dodging 100 obstacles,
        // something is seriously wrong.
        int i;
        int limit = 100;
        for (i=0; i<limit; i++) {
            PathIntersection intersection = findFirstObstacle(type, next, loc2);
            if (intersection == null)
                break;
            next = findPathAroundObstacle(type, value, intersection, next, loc2, poLoc2, entryPortalLoc, halfWidth);
            if (next == null) {
                value.removePathElementsAfter(pathSize);
                return PathResult.TerrainPath;
            }
            boolean endModel = (poLoc2 != null && intersection.getPathObject() == poLoc2.getPathObject());
            if (endModel)
                break;
        }
        if (!haveEndModel)
            value.addPathElement(loc2, true);
        if (logAll)
            log.debug("findPathThroughTerrain from loc1 " + loc1 + " to loc2 " + loc2 + "; i = " + i +
                " " + value.stringPath(pathSize));
        if (i == limit) {
            // We failed - - remove the added path elements
            value.removePathElementsAfter(pathSize);
            if (logAll)
                log.error("findPathThroughTerrain: Didn't find path in " + limit + " tries");
            return PathResult.TerrainPath;
        }
        else
            return PathResult.OK;
    }
    
    // Get the set of obstacles between the two locations from the
    // quad tree.  If the set is empty, return null.  Else find the
    // set element closest to loc1 and return it
    public PathIntersection findFirstObstacle(String type, MVVector loc1, MVVector loc2) {
        if (logAll)
            log.debug("findFirstObstacle: loc1 = " + loc1 + "; loc2 = " + loc2);
        QuadTree<PathModelElement> tree = findQuadTreeForType(type);
        if (tree == null)
            return null;
        Set<PathModelElement> elems = tree.getElementsBetween(new Point(loc1), new Point(loc2));
        if (logAll)
            log.debug("findFirstObstacle: elems = " + (elems == null ? elems : elems.size()));
        if (elems == null || elems.size() == 0) 
            return null;
        while (true) {
            PathIntersection closest = null;
            QuadTreeElement closestElem = null;
            for (PathModelElement elem : elems) {
                if (logAll)
                    log.debug("findFirstObstacle elem = " + elem);
                PathObject pathObject = (PathObject)elem.getQuadTreeObject();
                PathIntersection intersection =
                    pathObject.getBoundingPolygon().closestIntersection(pathObject, loc1, loc2);
                if (intersection != null && 
                    (closest == null || intersection.getWhere1() < closest.getWhere1())) {
                    closest = intersection;
                    closestElem = elem;
                }
            }
            if (closest == null)
                return null;
            PathObject pathObject = closest.getPathObject();
            PathIntersection pathObjectClosest = pathObject.closestIntersection(loc1, loc2);
            if (pathObjectClosest != null) {
                if (logAll)
                    log.debug("findFirstObstacle: pathObjectClosest = " + pathObjectClosest);
                return pathObjectClosest;
            }
            else
                elems.remove(closestElem);
        }
    }

    MVVector findPathAroundObstacle(String type, PathFinderValue value, PathIntersection intersection, 
                                 MVVector loc1, MVVector loc2, PathObjectLocation poLoc2, 
                                 MVVector entryPortalLoc, float halfWidth) {
        // New approach: find the closest corners to each of loc1 and
        // loc2, and then use the AStar algorithm to get around the obstacle
        PathObject pathObject = intersection.getPathObject();
        boolean endModel = (poLoc2 != null && pathObject == poLoc2.getPathObject());
        int corner1 = (endModel ? findCornerOnPathToPortal(loc1, poLoc2, entryPortalLoc) :
                       pathObject.getClosestCornerToPoint(loc1));
        MVVector cornerPoint1 = pathObject.getBoundingPolygon().getCorners().get(corner1);
        PathPolygon terrainPoly1 = pathObject.getTerrainPolygonAtCorner(corner1);
        if (terrainPoly1 == null) {
            log.error("findPathAroundObstacle: terrainPoly1 = null!");
            return null;
        }
        MVVector endPoint = null;
        PathPolygon endPolygon = null;
        PathObjectLocation poLoc1 = new PathObjectLocation(pathObject, cornerPoint1, 
                PathPolygon.Terrain, terrainPoly1.getIndex());
        if (!endModel) {
            int corner2 = pathObject.getClosestCornerToPoint(loc2);
            endPoint = pathObject.getBoundingPolygon().getCorners().get(corner2);
            endPolygon = pathObject.getTerrainPolygonAtCorner(corner2);
            if (endPolygon == null) {
                log.error("findPathAroundObstacle: endPolygon = null!");
                return null;
            }
        }
        if (logAll)
            log.debug("findPathAroundObstacle: loc1 = " + loc1 + "; corner1 = " + corner1 + 
                "; cornerPoint1 = " + cornerPoint1 + "; terrainPoly1 = " + terrainPoly1 +
                "; endModel = " + tOrF(endModel) +
                "; loc2 = " + loc2 + "; endPoint = " + endPoint + 
                "; endPoint = " + endPoint + "; endPolygon = " + endPolygon);
        PathObjectLocation poLoc = (endModel ? poLoc2 : 
            new PathObjectLocation(pathObject, endPoint, endPolygon.getKind(), endPolygon.getIndex()));
        if (PathAStarSearcher.findPathInModel(value, poLoc1, poLoc, halfWidth))
            // Damn, I hate the fact that Java doesn't have ref or out
            // parameters.  Since endPoint will just be tested to see
            // if it's null when endModel is true, just return any
            // crummy non-null point
            return (endModel ? cornerPoint1 : endPoint);
        else
            return null;
    }
    
    // Return the location the corner with the shortest total from loc
    // to corner to distance the center of the portal
    protected int findCornerOnPathToPortal(MVVector loc, PathObjectLocation poLoc, MVVector entryPortalLoc) {
        float closestDistance = Float.MAX_VALUE;
        int closestCorner = -1;
        PathPolygon poly = poLoc.getPathObject().getBoundingPolygon();
        List<MVVector >corners = poly.getCorners();
        if (logAll)
            log.debug("findCornerOnPathToPortal: loc = " + loc + "; poLoc = " + poLoc + 
                "; entryPortalLoc = " + entryPortalLoc + "; poly = " + poly);
        for (int i=0; i<corners.size(); i++) {
            MVVector corner = corners.get(i);
            float toCorner = MVVector.distanceTo(loc, corner);
            float cornerToEntryLoc = MVVector.distanceTo(corner, entryPortalLoc);
            float d = toCorner + cornerToEntryLoc;
            if (d < closestDistance) {
                closestDistance = d;
                closestCorner = i;
            }
        }
        return closestCorner;
    }

//     MVVector findPathAroundObstacle(String type, List<MVVector> path, PathIntersection intersection, 
//     		                 MVVector loc1, MVVector loc2, float halfWidth) {
//         PathIntersection nextIntersection = intersection;
//         MVVector next = loc1;
//         // Remember the path object
//         PathObject initialPathObject = intersection.getPathObject();
//         int i;
//         // If we haven't found a path in 4 tries, we're not going to
//         int limit = 4;
//         for (i=0; i<limit; i++) {
//             float w = nextIntersection.getWhere2();
//             MVVector n = new MVVector(loc2);
//             n.sub(next);
//             n.setY(0f);
//             n.normalize();
//             float temp;
//             if (w > 0.5) {
//                 w = 1.0f;
//                 // Clockwise perpendicular
//                 temp = n.getX();
//                 n.setX(n.getY());
//                 n.setY(- temp);
//             }
//             else {
//                 w = 0.0f;
//                 // Clockwise perpendicular
//                 temp = n.getX();
//                 n.setX(- n.getY());
//                 n.setY(temp);
//             }
//             // Now add halfwidth line perpendicular to the line from next to loc2
//             n.multiply(halfWidth);
//             MVVector lineEndPlusHalfWidth = nextIntersection.getIntersectorPoint(w);
//             if (logAll)
//                 log.debug("findPathAroundObstacle: halfWidth = " + halfWidth + "; intersection  point = " + 
//                     lineEndPlusHalfWidth + "; halfWidth offset vector = " + n);
//             lineEndPlusHalfWidth.add(n);
//             next = new MVVector(lineEndPlusHalfWidth);
//             if (logAll)
//                 log.debug("findPathAroundObstacle: adding point " + next + " to avoid obstacle");
//             path.add(next);
//             nextIntersection = findFirstObstacle(type, next, loc2);
//             if (nextIntersection == null || nextIntersection.getPathObject() != initialPathObject) {
//                 if (logAll)
//                     log.debug("findPathAroundObstacle: success!");
//                 return next;
//             }
//         }
//         if (logAll)
//             log.debug("findPathAroundObstacle: no path found around obstacle " + 
//                 initialPathObject + " after " + limit + " tries.");
//         return null;
//     }
    
    boolean findPathToPortal(PathFinderValue value, float halfWidth, PathObjectLocation poLoc,
                             PathArc portal, MVVector portalLoc) {
        PathObjectLocation startLoc = 
            new PathObjectLocation(poLoc.getPathObject(), portalLoc, PathPolygon.CV, portal.getPoly1Index());
        if (logAll)
            log.debug("findPathToPortal portal = " + portal + "; halfWidth = " + halfWidth + "; startLoc = " + startLoc);
        return findPathInModel(value, poLoc, startLoc, halfWidth);
    }
        
    boolean findPathFromPortal(PathFinderValue value, float halfWidth, PathObjectLocation poLoc,
                               PathArc portal, MVVector portalLoc) {
        PathObjectLocation startLoc = 
            new PathObjectLocation(poLoc.getPathObject(), portalLoc, PathPolygon.CV, portal.getPoly1Index());
        if (logAll)
            log.debug("findPathFromPortal portal = " + portal + "; halfWidth = " + halfWidth + "; startLoc = " + startLoc);
        return findPathInModel(value, startLoc, poLoc, halfWidth);
    }
        
    protected boolean findPathInModel(PathFinderValue value, PathObjectLocation poLoc1, PathObjectLocation poLoc2, float halfWidth) {
        return PathAStarSearcher.findPathInModel(value, poLoc1, poLoc2, halfWidth);
    }

    // The instance for this singleton class; all clients of this
    // singleton class check the result of getInstance for null
    protected static PathSearcher instance = null;

    // The quad trees that lets us map a given location to a model.
    // There is one quad tree per PathObjectType; the map key is
    // the name of the PathObjectType.
    protected Map<String, QuadTree<PathModelElement>> quadTrees;

    public PathInfo getPathInfo() {
        return pathInfo;
    }
    
    // Contains the mapping of static object id to the set of path
    // objects associated with that object, and the dictionary of
    // path object types.
    protected PathInfo pathInfo = null;
    
    // A string each character of which indicates whether that path
    // segment is over terrain
    protected String terrainString;
    
    protected static final Logger log = new Logger("PathSearcher");
    protected static boolean logAll = true;

}
