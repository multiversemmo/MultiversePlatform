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

public class PathSearchNode {

    // Close enough, in millimeters
    protected static int closeEnough = 100;
    
    // Only the first and last states are locations; all other states
    // are arcs
    public PathSearchNode(int polyIndex, MVVector loc) {
        this.polyIndex = polyIndex;
        this.loc = loc;
    }
    
    // Only the first and last states are locations; all other states
    // are arcs
    public PathSearchNode(MVVector loc, PathSearchNode predecessor) {
        this.loc = loc;
        this.predecessor = predecessor;
    }
    
    // Only the first and last states are locations; all other states
    // are arcs
    public PathSearchNode(PathArc arc, int polyIndex, PathSearchNode predecessor) {
        this.polyIndex = polyIndex;
        this.arc = arc;
        this.predecessor = predecessor;
    }
    
    public boolean equals(Object obj) {
        return (costSoFar == ((PathSearchNode)obj).costSoFar);
    }
    
    public MVVector getLoc() {
        return loc;
    }
    
    public void setLoc(MVVector value) {
        loc = value;
    }
    
    public int getPolyIndex() {
        return polyIndex;
    }
    
    public int distanceEstimate(PathSearchNode goal) {
        return (int)MVVector.distanceTo(loc, goal.getLoc());
    }
    
    public boolean atGoal(PathSearchNode goal) {
        return polyIndex == goal.getPolyIndex();
    }
    
    protected List<PathSearchNode> getSuccessors(PathAStarSearcher searcher) {
        // Get all the arcs that start with this node, other than this
        // node's parent
        List<PathSearchNode> nodes = new LinkedList<PathSearchNode>();
        for (PathArc arc : searcher.getPolygonArcs(polyIndex)) {
            int poly1Index = arc.getPoly1Index();
            int poly2Index = arc.getPoly2Index();
            int otherPolyIndex;
            if (polyIndex == poly2Index)
                otherPolyIndex = poly1Index;
            else
                otherPolyIndex = arc.getPoly2Index();
            if (predecessor == null || otherPolyIndex != predecessor.getPolyIndex()) {
                PathSearchNode successor = new PathSearchNode(arc, otherPolyIndex, this);
                nodes.add(successor);
                if (logAll)
                    log.debug("getSuccessors: arc = " + arc.shortString() + "; successor = " + successor);
            }
        }
        if (logAll)
            log.debug("getSuccessors: returning " + nodes.size() + " successors");
        return nodes;
    }

    public String toString() {
        return "[PathSearchNode  polyIndex = " + polyIndex + "; loc = " + loc + "; arc = " + arc + 
            "; costSoFar = " + costSoFar + "; costToGoal = " + costToGoal + "]";
    }
    
    public String shortString() {
        return "[PathSearchNode  polyIndex = " + polyIndex + "; loc = " + loc + "; arc = " + 
            (arc == null ? "null" : arc.shortString()) + "; costSoFar = " + costSoFar + "; costToGoal = " + costToGoal + "]";
    }
    
    // We assume that it's the same state if it's the same polygon
    protected boolean isSameState(PathSearchNode node) {
        return node.getPolyIndex() == polyIndex;
    }

    // Calculate the incremental cost of going from this node to the
    // successor.  If this is the start node, that cost is the
    // distance from the starting loc to the portal by which we enter
    // the successor; if this is not the start node, the cost is the
    // distance between the two portals
    protected int getCostBetween(PathSearchNode successor) {
        MVVector startLoc = arc == null ? loc : arc.getEdge().getMidpoint();
        PathArc sarc = successor.getArc();
        MVVector endLoc = sarc == null ? successor.getLoc() : sarc.getEdge().getMidpoint();
        return (int)MVVector.distanceTo(startLoc, endLoc);
    }

    public PathArc getArc() {
    	return arc;
    }
    
    public void setArc(PathArc arc) {
    	this.arc = arc;
    }
    
    public void setPredecessor(PathSearchNode node) {
        predecessor = node;
    }

    public PathSearchNode getPredecessor() {
        return predecessor;
    }

    public void setCostSoFar(int cost) {
        costSoFar = cost;
    }

    public int getCostSoFar() {
        return costSoFar;
    }

    public void setCostToGoal(int cost) {
        costToGoal = cost;
    }

    public int getCostToGoal() {
        return costToGoal;
    }

    public int getCostToEnd() {
        return costSoFar + costToGoal;
    }

    // The arc containing this node, or null for the starting node
    protected PathArc arc;
    // The index of the polygon of this node
    protected int polyIndex;
    // The predecessor, or null for the starting node
    protected PathSearchNode predecessor;
    // The location in the polygon
    protected MVVector loc;
    // The cost up to and including this node.
    protected int costSoFar;
    // The estimated cost to get from this node to to the goal node.
    protected int costToGoal;

    protected static final Logger log = new Logger("PathSearchNode");
    protected static boolean logAll = false;

}

