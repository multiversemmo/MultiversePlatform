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
// The states need to accurately reflect the time it takes to get from
// one place to another.  Since the nodes in the pathing graph are
// convex polyhedrons, and there can be more than one "arc" into
// the polyhedron, the AStar "state" needs to reflect the fact that
// some paths into the node are shorter than others.  Therefore, the
// state needs to name the node, the portal, and possibly the position
// along the arc.
//
// The initial and final states are special, in that they must reflect
// the distance from the position inside the node and the egress
// arc(s).
//
////////////////////////////////////////////////////////////////////////

public class PathAStarSearcher {
    
    public PathAStarSearcher(PathObject pathObject) {
        this.pathObject = pathObject;
    }
    
    public enum SearchState {
        Running((byte)0),
        Succeeded((byte)1),
        Failed((byte)2);
        SearchState(byte val) {
            this.val = val;
        }
        byte val = -1;
    }
    
    public static boolean findPathInModel(PathFinderValue value,
                                          PathObjectLocation poLoc1, 
                                          PathObjectLocation poLoc2,
                                          float halfWidth) {
        PathAStarSearcher astar = new PathAStarSearcher(poLoc1.getPathObject());
        MVVector loc1 = poLoc1.getLoc();
        MVVector loc2 = poLoc2.getLoc();
        // If the start and end polygons are the same, we're already done
        int index = poLoc1.getPolyIndex();
        boolean terrainPoint = poLoc1.getPathObject().isTerrainPolygon(index);
        if (index == poLoc2.getPolyIndex()) {
            if (Log.loggingDebug)
                log.debug("findPathInModel: start and end polygon index are the same, so success!");
            value.addPathElement(loc1, terrainPoint);
            value.addPathElement(loc2, terrainPoint);
            return true;
        }
        log.debug("findPathInModel: about to call aStarSearch");
        PathSearchNode node = astar.aStarSearch(poLoc1.getPolyIndex(), loc1,
                                                poLoc2.getPolyIndex(), loc2);
        int i = value.pathElementCount();
        boolean result = astar.createPath(value, poLoc1.getPathObject(), loc2, halfWidth, node);
        if (Log.loggingDebug)
            log.debug("findPathInModel from " + loc1 + " to " + loc2 + ": " + value.stringPath(i));
        return result;
    }
    
    protected boolean createPath(PathFinderValue value, PathObject po, MVVector loc2, float halfWidth, PathSearchNode goal) {
        if (goal == null)
            return false;
        List<MVVector> reversePath = new LinkedList<MVVector>();
        List<Integer> reversePolygonIndexes = new LinkedList<Integer>();
        // The goal node has both a location and an arc
        reversePath.add(goal.getLoc());
        reversePolygonIndexes.add(goal.getPolyIndex());
        PathSearchNode node = goal;
        while (node != null) {
            int nodePolygonIndex = node.getPolyIndex();
            if (logAll)
                log.debug("createPath: node = " + node.shortString());
            PathSearchNode next = node.getPredecessor();
            // The predecessor can only be null for the start node
            if (next == null) {
                reversePath.add(node.getLoc());
                reversePolygonIndexes.add(nodePolygonIndex);
                break;
            }
            PathArc arc = node.getArc();
            MVVector lastPoint = reversePath.get(reversePath.size() - 1);
            MVVector nextPoint = next.getArc() != null ? next.getArc().getEdge().getMidpoint() : loc2;
            if (arc != null) {
//                 List<Point> points = arc.getEdge().getNearAndFarNormalPoints(lastPoint, nextPoint, halfWidth);
//                 Point near = points.get(0);
//                 Point far = (points.size() == 2 ? points.get(1) : null);
//                 reversePath.add(near);
//                 if (far != null)
//                     reversePath.add(far);
//                 if (logAll)
//                     log.debug("createPath: near = " + near + "; far = " + far + "; lastPoint = " + lastPoint +
//                         "; nextPoint = " + nextPoint + "; arc = " + arc);
                MVVector p = arc.getEdge().bestPoint(lastPoint, nextPoint, halfWidth);
                if (logAll)
                    log.debug("createPath: bestPoint = " + p + "; lastPoint = " + lastPoint +
                        "; nextPoint = " + nextPoint + "; arc = " + arc);
                reversePath.add(p);
                reversePolygonIndexes.add(nodePolygonIndex);
            }
            else {
            	log.error("For intermediate node " + node + ", no arc was found!");
            	return false;
            }
            node = next;
        }
        for (int i=reversePath.size() - 1; i>=0; i--) {
            MVVector p = reversePath.get(i);
            int nodePolyIndex = reversePolygonIndexes.get(i);
            boolean terrainPolygon = po.isTerrainPolygon(nodePolyIndex);
            if (logAll)
                log.debug("createPath: adding point = " + p + "; over terrain = " + Boolean.toString(terrainPolygon));
            value.addPathElement(p, terrainPolygon);
        }
        return true;
    }

    protected class PathSearchNodeCostComparator implements Comparator {
        public PathSearchNodeCostComparator() {
        }

        public int compare(Object n1, Object n2) {
            PathSearchNode s1 = (PathSearchNode)n1;
            PathSearchNode s2 = (PathSearchNode)n2;
            int cost1 = s1.costSoFar;
            int cost2 = s2.costSoFar;
            return (cost1 < cost2 ? -1 : cost1 > cost2 ? 1 : 
                s1.getPolyIndex() < s2.getPolyIndex() ? -1 :
                s1.getPolyIndex() == s2.getPolyIndex() ? 0 : 1);
        }
    }
    
    // Returns the last node in the sequence; you can get the others
    // by following the predecessor link
    protected PathSearchNode aStarSearch(int poly1, MVVector loc1, int poly2, MVVector loc2) {
        if (Log.loggingDebug)
            log.debug("aStarSearch poly1 = " + poly1 + "; loc1 = " + loc1 + "; poly2 = " + poly2 + "; loc2 = " + loc2);
        goal = new PathSearchNode(poly2, loc2);
        // If the start polygon and goal polygon are the same, return
        // the goal
        if (poly1 == poly2)
            return goal;
        openPrioritySet = new TreeSet<PathSearchNode>(new PathSearchNodeCostComparator());
        openStates = new HashMap<Integer, PathSearchNode>();
        closedStates = new HashMap<Integer, PathSearchNode>();
        start = new PathSearchNode(poly1, loc1);
        start.setCostToGoal(start.distanceEstimate(goal));
        openStates.put(start.getPolyIndex(), start);
        openPrioritySet.add(start);
        if (logAll)
            log.debug("aStarSearch start = " + start.shortString() + "; goal = " + goal.shortString());
        SearchState state = SearchState.Running;
        do
        {
            state = iterate();
            iterations++;

        } while(state == SearchState.Running);
        return (state == SearchState.Succeeded ? goal : null);
    }

    protected SearchState iterate() {
        if (logAll)
            log.debug("iterate: openStates.size() = " + openStates.size() + "; openPrioritySet.size() = " + openPrioritySet.size());
        if (openStates.size() == 0)
            return SearchState.Failed;
        // Get the open node with the cheapest estimated cost to the
        // goal
        PathSearchNode current = openPrioritySet.first();
        if (logAll)
            log.debug("iterate: current = " + current.shortString() + "; iterations = " + iterations);
        openPrioritySet.remove(current);
        openStates.remove(current.getPolyIndex());
        if (current.isSameState(goal)) {
            if (Log.loggingDebug)
                log.debug("iterate: Succeeded, because current = " + current.shortString() +
                          " same as goal = " + goal.shortString());
            current.setLoc(goal.getLoc());
            goal = current;
            dumpStateSet("openStates successor loop", openStates);
            dumpStateSet("closedStates successor loop", closedStates);
            return SearchState.Succeeded;
        }
        List<PathSearchNode> successors = current.getSuccessors(this);
        for (PathSearchNode successor : successors) {
            if (logAll) {
                dumpStateSet("openStates successor loop", openStates);
                dumpStateSet("closedStates successor loop", closedStates);
            }
            int cost = current.getCostSoFar() + current.getCostBetween(successor);
            int index = successor.getPolyIndex();
            if (logAll)
                log.debug("iterate: successor = " + successor.shortString() + "; cost = " + cost);
            PathSearchNode openElement = null;
            PathSearchNode closedElement = null;
            if (openStates.containsKey(index)) {
                openElement = openStates.get(index);
                if (openElement.getCostSoFar() <= cost) {
                    if (logAll)
                        log.debug("iterate: Ignoring successor, because openElement = " + 
                            openElement.shortString() + " cost < " + cost);
                    continue;
                }
            }
            if (closedStates.containsKey(index)) {
                closedElement = closedStates.get(index);
                if (closedElement.getCostSoFar() < cost) {
                    if (logAll)
                        log.debug("iterate: Ignoring successor, because closedElement = " + 
                            closedElement.shortString() + " cost < " + cost);
                    continue;
                }
                else
                    closedStates.remove(closedElement);
            }
            if (openElement != null) {
                // Remove the open item, set the new cost and
                // predecessor, and re-add the item, to get it sorted
                // to the right place in the list
                if (logAll)
                    log.debug("iterate: Successor index = " + index + 
                        " found in openStates, so replacing openElement cost = " + 
                        openElement.getCostSoFar() + " with current cost = " + cost);
                openPrioritySet.remove(openElement);
                openElement.setCostSoFar(cost);
                openElement.setPredecessor(current);
                openPrioritySet.add(openElement);
            }
            else {
                successor.setCostSoFar(cost);
                if (logAll)
                    log.debug("iterate: About to add successor current = " + current.getPolyIndex() +
                        "; openStates.size() = " + openStates.size() + 
                        "; openPrioritySet.size() = " + openPrioritySet.size());
                openStates.put(successor.getPolyIndex(), successor);
                openPrioritySet.add(successor);
                if (logAll)
                    log.debug("iterate: Added successor current = " + current.getPolyIndex() +
                        "; openStates.size() = " + openStates.size() + 
                        "; openPrioritySet.size() = " + openPrioritySet.size());
            }
        }
        // Finally, add the current node to the closed list
        closedStates.put(current.getPolyIndex(), current);
        if (logAll)
            log.debug("iterate: Added current = " + current.shortString() + 
                " to closedStates, whose size is " + closedStates.size());
        return SearchState.Running;
    }
    
    void dumpStateSet(String which, Map<Integer, PathSearchNode> states) {
        String s = "dumpStateSet: set " + which + "; ";
        Iterator<Map.Entry<Integer, PathSearchNode>> iter = 
            states.entrySet().iterator();
 	while (iter.hasNext()) {
            Map.Entry<Integer, PathSearchNode> entry = iter.next();
            s += "[" + entry.getKey() + ": ";
            String e = "";
            PathSearchNode n = entry.getValue();
            do {
                if (e.length() > 0)
                    e += ">";
                e += n.getPolyIndex();
                n = n.getPredecessor();
            } while (n != null);
            s += e + "] ";
        }
        log.debug(s);
    }

    List<PathArc> getPolygonArcs(int polyIndex) {
        return pathObject.getPolygonArcs(polyIndex);
    }

    // A priority list of open states, ordered by ascending estimated
    // cost to goal
    protected TreeSet<PathSearchNode> openPrioritySet;
    // Mappings from polygon index to state
    protected Map<Integer, PathSearchNode> openStates;
    protected Map<Integer, PathSearchNode> closedStates;

    protected int iterations;
    protected PathSearchNode start;
    protected PathSearchNode goal;
    protected PathObject pathObject;
    
    protected static final Logger log = new Logger("PathAStarSearcher");
    protected static boolean logAll = false;

}


