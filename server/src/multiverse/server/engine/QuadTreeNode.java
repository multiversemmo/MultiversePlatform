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

package multiverse.server.engine;

import java.util.*;
import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import java.util.concurrent.locks.*;

// note that the quadtree is broken up on the X,Z plane (the ground)

public class QuadTreeNode<ElementType extends QuadTreeElement<ElementType>> {

    // includes all points passed in
    QuadTreeNode(QuadTree<ElementType> tree, 
                 QuadTreeNode<ElementType> parent,
                 Geometry g, NodeType type) {
        this.parent = parent;
        this.tree = tree;
        this.geometry = g;
        this.type = type;
        if (parent != null) {
            depth = parent.getDepth() + 1;
        }
        else {
            depth = 0;
        }
        lock = LockFactory.makeLock("QuadTreeNodeLock-" + geometry.toString());
    }

    public String toString() {
        return "[QuadTreeNode: depth=" + getDepth() + 
            " numObjects=" + getNodeElements().size() + 
            " " + geometry + "]";
    }

    /**
     * prints out this node and also recursively all sub nodes
     */
    void recurseToString() {
        try {
            lock.lock();
            int depth = getDepth();
            String ws = "";
            for (int i=0; i<depth; i++) {
                ws = ws + "- ";
            }

            // print out this node
            if (Log.loggingDebug)
                log.debug(ws + this.toString());

            // if leaf, return
            if (isLeaf()) {
                return;
            }

            // not leaf, recurse
            children.get(0).recurseToString();
            children.get(1).recurseToString();
            children.get(2).recurseToString();
            children.get(3).recurseToString();
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * Returns whether the passed in location is within the current node
     * (including its children).
     */
    public boolean containsPoint(Point loc) {
        lock.lock();
        try {
            if (loc == null) {
                return false;
            }
            return (geometry.contains(loc));
        }
        finally {
            lock.unlock();
        }
    }

    public boolean containsPointWithHysteresis(Point loc) {
        lock.lock();
        try {
            if (loc == null) {
                return false;
            }
            int hysteresis = tree.getHysteresis();
            if (hysteresis == 0)
                return (geometry.contains(loc));
            else {
                // Pad out the geometry by the non-negative hysteresis distance
                Geometry hystericalGeometry = new Geometry(geometry.getMinX() + hysteresis,
                                                           geometry.getMaxX() - hysteresis,
                                                           geometry.getMinZ() + hysteresis,
                                                           geometry.getMaxZ() - hysteresis);
                boolean ret = hystericalGeometry.contains(loc);
                if (Log.loggingDebug)
                    Log.debug("QuadTreeNode.containsPointWithHysteresis: point=" + loc + 
                        ", geom=" + hystericalGeometry + ", ret = " + ret + ", node=" + this);
                return ret;
            }
        }
        finally {
            lock.unlock();
        }
    }
    
    // returns true if this node has no child nodes
    boolean isLeaf() {
        return (getChildren() == null);
    }

    // returns depth of tree.  top level is depth 0
    int getDepth() {
        return depth;
    }

    QuadTreeNode<ElementType> getParent() {
        return parent;
    }

    public QuadTree<ElementType> getTree() {
        lock.lock();
        try {
            return tree;
        }
        finally {
            lock.unlock();
        }
    }

    // returns the child node that contain the location passed in.
    // if no child contains that location, then returns null
    QuadTreeNode<ElementType> whichChild(Point loc) {
        lock.lock();
        try {
            if (children == null) {
                log.warn("whichChild: no children");
                return null;
            }
//          log.debug("QuadTreeNode.whichChild: looking for child of quadnode " + this + " for point " + loc);
            for (QuadTreeNode<ElementType> child : children) {
                if (child.containsPoint(loc)) {
//                  log.debug("QuadTreeNode.whichChild: found " + children.get(i));
                    return child;
                }
            }
            log.warn("whichChild: did not find child for point " +
                     loc + ", thisNode=" + this);
            return null;
        }
        finally {
            lock.unlock();
        }
    }

    // returns the set of quadtree elements around the point within radius
    Set<ElementType> getElements(Point loc, int radius) {
        lock.lock();
        try {
            if (isLeaf()) {
                if (distanceTo(loc) < radius) {
                    Set<ElementType> ownElements = getNodeElements();
                    ownElements.addAll(perceiverExtentObjects);
                    return ownElements;
                }
                else {
                    return new HashSet<ElementType>();
                }
            } else {
                // recurse
                Set<ElementType> objSet = new HashSet<ElementType>();
                for (QuadTreeNode<ElementType> child : children) {
                    if (child.distanceTo(loc) < radius) {
                        objSet.addAll(child.getElements(loc, radius));
                        objSet.addAll(perceiverExtentObjects);
                    }
                }
                return objSet;
            }
        }
        finally {
            lock.unlock();
        }
    }

    // returns a list of elements that might intersect a line segment
    // from loc1 to loc2.  the list is "conservative", meaning that it
    // might return entities that don't in fact intersect the segment,
    // but is guaranteed not to omit any elements that might intersect
    // the segment.
    public Set<ElementType> getElementsBetween(Point loc1, Point loc2) {
        lock.lock();
        try {
            if (isLeaf()) {
                if (logPath)
                    if (Log.loggingDebug)
                        log.debug("getElementsBetween leaf: geometry = " + geometry + 
                                  "; nodeElements.size() = " + nodeElements.size() + 
                                  "; loc1 = " + loc1 + "; loc2 = " + loc2);
                if (segmentIntersectsNode(loc1, loc2)) {
                    Set<ElementType> elems = new HashSet<ElementType>();
                    addCloseElements(elems, nodeElements, loc1, loc2);
                    addCloseElements(elems, perceiverExtentObjects, loc1, loc2);
                    return elems;
                }
                else {
                    return new HashSet<ElementType>();
                }
            } else {
                // recurse
                Set<ElementType> elems = new HashSet<ElementType>();
                for (QuadTreeNode<ElementType> child : children) {
                    if (logPath) {
                        if (Log.loggingDebug)
                            log.debug("getElementsBetween: child = " + child + "; loc1 = " +
                                loc1 + "; loc2 = " + loc2);
                    }
                    if (child.segmentIntersectsNode(loc1, loc2)) {
                        elems.addAll(child.getElementsBetween(loc1, loc2));
                        addCloseElements(elems, perceiverExtentObjects, loc1, loc2);
                    }
                }
                return elems;
            }
        }
        finally {
            lock.unlock();
        }
    }
    
    // Add any members of adds closer than radius to the segment
    // between loc1 and loc2 to elems
    void addCloseElements(Set<ElementType> elems, Set<ElementType> adds, Point loc1, Point loc2) {
        for (ElementType elem : adds) {
            int radius = elem.getObjectRadius();
            Point center = elem.getLoc();
            if (logPath) {
                if (Log.loggingDebug)
                    log.debug("addCloseElements: elem = " + elem + "; center = " + center + "; radius = " + radius);
            }
            if (segmentCloserThanDistance(loc1, loc2, center.getX(), center.getZ(), radius))
                elems.add(elem);
        }
    }
            
    // adds the element to this node
    void addElement(ElementType elem) {
        lock.lock();
        try {
            nodeElements.add(elem);
        }
        finally {
            lock.unlock();
        }
    }

    // remove the element from this node - returns if it was found and removed
    boolean removeElement(ElementType elem) {
        if (Log.loggingDebug)
            log.debug("removing element " + elem + " from quadtreenode " + this);
        lock.lock();
        try {
            return nodeElements.remove(elem);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * returns objects in this node
     */
    Set<ElementType> getNodeElements() {
        try {
            lock.lock();
            return new HashSet<ElementType>(nodeElements);
        }
        finally {
            lock.unlock();
        }
    }

    // returns the number of objects in this node
    int numElements() {
        try {
            lock.lock();
            return nodeElements.size();
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * is the passed in obj in THIS node (not recursive) 
     */
    boolean containsElement(ElementType obj) {
        try {
            lock.lock();
            return (nodeElements.contains(obj));
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * adds the elem to the ancilliary list of perceivable objects for
     * all subnodes.  By definition, the containing node has the
     * extent-based element on its list.
     */
    public void addPerceiverExtentObject(ElementType elem, Point loc, int radius) {
        lock.lock();
        try {
            if (isLeaf()) {
                if (distanceTo(loc) < radius) {
                    if (perceiverExtentObjects == null)
                        perceiverExtentObjects = new HashSet<ElementType>();
                    if (Log.loggingDebug)
                        log.debug("addPerceiverExtentObject; adding: " + this.toString() + " elem: " + elem +
                                  ", loc: " + loc + " radius: " + radius);
                    perceiverExtentObjects.add(elem);
                } 
            } else {
                // recurse
                for (QuadTreeNode<ElementType> child : children) {
                    child.addPerceiverExtentObject(elem, loc, radius);
                }
            }
        }
        finally {
            lock.unlock();
        }
    }

    void addPerceiver(Perceiver<ElementType> p) {
        tree.getLock().lock();
        try {
            lock.lock();
            try {
                perceivers.add(p);
            } finally {
                lock.unlock();
            }
        } finally {
            tree.getLock().unlock();
        }
    }
    void removePerceiver(Perceiver<ElementType> p) {
        tree.getLock().lock();
        try {
            lock.lock();
            try {
                perceivers.remove(p);
            } finally {
                lock.unlock();
            }
        } finally {
            tree.getLock().unlock();
        }
    }

    Set<Perceiver<ElementType>> getPerceivers() {
        tree.getLock().lock();
        try {
            lock.lock();
            try {
                return new HashSet<Perceiver<ElementType>>(perceivers);
            } finally {
                lock.unlock();
            }
        } finally {
            tree.getLock().unlock();
        }
    }
    private Set<Perceiver<ElementType>> perceivers = new HashSet<Perceiver<ElementType>>();

    /**
     * how far is the point from this node - if it is IN the node, then
     * distance is 0
     */
    float distanceTo(Point loc) {
        float dist = -1;
        int minX = geometry.getMinX();
        int minZ = geometry.getMinZ();
        int maxX = geometry.getMaxX();
        int maxZ = geometry.getMaxZ();
        int ptX = loc.getX();
        int ptZ = loc.getZ();

        // condition 1: the point is IN the node
        if (containsPoint(loc)) {
            dist = 0;
        }
        // condition 2: the point is above or below the node
        else if ((minX < ptX) && (ptX < maxX)) {
            if (ptZ > maxZ) {
                dist = ptZ - maxZ;
            }
            else {
                dist = minZ - ptZ;
            }
        }
        // condition 3: the point is to the right/left of the node
        else if ((minZ < ptZ) && (ptZ < maxZ)) {
            if (ptX > maxX) {
                dist = ptX - maxX;
            }
            else {
                dist = minX - ptX;
            }
        }
        // condition 4: the point is diagonal
        else {
            Iterator<Point> iter = geometry.getCorners().iterator();
            while(iter.hasNext()) {
                Point corner = iter.next();
                float cornerDist = Point.distanceTo(corner, loc);
                if ((dist == -1) || (cornerDist < dist)) {
                    dist = cornerDist;
                }
            }
        }
        return dist;
    }

    /**
     * how far is the line segment from this node
     */
    boolean segmentIntersectsNode(Point loc1, Point loc2) {
        int minX = geometry.getMinX();
        int minZ = geometry.getMinZ();
        int maxX = geometry.getMaxX();
        int maxZ = geometry.getMaxZ();
        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;
        float nodeRadius = (float)Math.sqrt((minX - centerX) * (minX - centerX) + 
                                            (minZ - centerZ) * (minZ - centerZ));
        return segmentCloserThanDistance(loc1, loc2, centerX, centerZ, nodeRadius);
    }
    
    static boolean segmentCloserThanDistance(Point loc1, Point loc2,
                                             float centerX, float centerZ, float radius) {
        if (logPath) {
            if (Log.loggingDebug)
                log.debug("segmentCloserThanDistance: centerX = " + centerX + 
                    "; centerZ = " + centerZ + "; radius = " + radius);
        }
        MVVector pt1 = new MVVector(loc1.getX(), 0f, loc1.getZ());
        MVVector pt2 = new MVVector(loc2.getX(), 0f, loc2.getZ());
        pt2.sub(pt1);
        MVVector center = new MVVector(centerX, 0f, centerZ);
        MVVector m = pt1.minus(center);
        float b = m.dotProduct(pt2);
        float c = m.dotProduct(m) - radius * radius;
        float disc = b * b - c;
        if (logPath) {
            if (Log.loggingDebug)
                log.debug("segmentCloserThanDistance: b = " + b + "; c = " + c + 
                    "; disc = " + disc + "; pt1 = " + pt1 + "; pt2 = " + pt2 + "; m = " + m);
        }
        if ((c > 0f && b > 0f) || disc < 0f) {
            if (logPath) {
                if (Log.loggingDebug)
                    log.debug("segmentCloserThanDistance false: b = " + b + "; c = " + c + "; disc = " + disc);
            }
            return false;
        }
        // Find the line parameter t in q = pt1 + t * pt2
        float t = -b - (float)Math.sqrt(disc);
        // If t <= 1.0f, then we have an intersection
        boolean result = (t < 1.0f);
        if (logPath) {
            if (Log.loggingDebug)
                log.debug("segmentCloserThanDistance: result = " + (result ? "true" : "false") +
                    "; t = " + t);
        }
        return result;
    }

    void divide(QuadTree<ElementType>.NewsAndFrees newsAndFrees) {
        tree.getLock().lock();
        try {
            lock.lock();
            try {
                children = new ArrayList<QuadTreeNode<ElementType>>(4);

                Geometry[] newGeometry = geometry.divide();
                for (int i = 0; i < 4; i++) {
                    children.add(new QuadTreeNode<ElementType>(getTree(), this,
                            newGeometry[i], this.type));
                    if (Log.loggingDebug)
                        log.debug("divide: dividing=" + this.toString()
                                  + "- new child[" + i + "]=" + children.get(i));
                }

                // move all current objects to one of the child nodes
                // log.debug("QuadTreeNode.divide: moving objs to children
                // now");
                for (ElementType elem : nodeElements) {
                    QuadTreeNode<ElementType> childNode = whichChild(elem.getCurrentLoc());
                    if (Log.loggingDebug)
                        log.debug("divide: moving element " + elem + " TO CHILD "
                                  + childNode);
                    if (childNode == null) {
                        log.debug("divide: world node is no longer in this quad tree node, skipping it.  it should be moved when the updater thread notices its not longer in the quad tree node anymore");
                        continue;
                    }
                    // add element to the child node
                    childNode.addElement(elem);

                    // update the child node's forward node reference
                    elem.setQuadNode(childNode);
                }

                // if there are extent-based perceivers, iterate over
                // them, adding them to the children
                if (perceiverExtentObjects != null) {
                    for (ElementType elem : perceiverExtentObjects)
                        addPerceiverExtentObject(elem, elem.getCurrentLoc(), elem.getPerceptionRadius());
                    // Now eliminate the perceiverExtentObjects object
                    perceiverExtentObjects = null;
                }
                
                // set perceivers for each child
                for (QuadTreeNode<ElementType> node : children) {
                    for (Perceiver<ElementType> p : getPerceivers()) {
                        if (p.overlaps(node.getGeometry())) {
                            node.addPerceiver(p);
                            p.addQuadTreeNode(node);
                        }
                    }
                }

                // notify perceivers of elements they can no longer perceive
                for (QuadTreeNode<ElementType> node : children) {
                    Set<Perceiver<ElementType>> removePerceivers = new HashSet<Perceiver<ElementType>>(
                            perceivers);
                    removePerceivers.removeAll(node.perceivers);
                    for (Perceiver<ElementType> p : removePerceivers) {
                        for (ElementType elem : node.getNodeElements()) {
                            newsAndFrees.noteFreedElement(p, elem);
                        }
                    }
                }

                // remove this node from perceivers
                for (Perceiver<ElementType> p : getPerceivers()) {
                    p.removeQuadTreeNode(this);
                }
                perceivers.clear();
                nodeElements.clear();

		// Redistribute regions to new children
		if (regions != null) {
		    ArrayList<Region> currentRegions= regions;
		    regions= null;
		    for (Region region : currentRegions) {
		        this.addRegion(region);
		    }
		}
            } finally {
                lock.unlock();
            }
        } finally {
            tree.getLock().unlock();
        }
    }
        
    /**
     * Assumes object is being managed by this node,
     * but will move it out if the object is no longer within this
     * node's boundary and add it back into the tree.
     *
     * Returns false if the node's new location could not be set.
     * Probably because it is outside the quadtreenode boundary
     * or because it has moved into a remote quad tree node.
     *
     * This calls the non-updating getLoc method, so be sure
     * SOMETHING will update the loc before calling this
     */
    boolean updateElement(ElementType elem) {
        lock.lock();
        try {
            if (! nodeElements.contains(elem)) {
                throw new MVRuntimeException("QuadTreeNode: element not in our managed list: " + elem + " -- for node " + this.toString());
            }

            // does the elem need to be moved to a different node
            Point elemLoc = elem.getLoc();
            if (elemLoc == null) {
                // the element is no longer in the world
                throw new MVRuntimeException("quadtreenode: element location is null, could be because someone just acquired this object -- acquirehandler should remove element");
                // removeElement(elem);
                // return false;
            }

            // if this node doesnt contain the object OR
            // if the node is no longer a leaf node, move the object
            if ((! containsPoint(elemLoc)) || (getChildren() != null)) {
                if (Log.loggingDebug)
                    log.debug("updateElement: element is no longer in current node or we are not a leaf node, updating.  elem=" + elem);
                // object needs to be updated
                removeElement(elem);

                QuadTreeNode<ElementType> newNode = getTree().addElement(elem);
                if (newNode == null) {
                    // we cannot add it to the quad tree, must be out of
                    // bounds or the node is a remote node server.
                    // 
                    log.debug("updateObject: obj moved to a remote node");

                    // FIXME: TODO:
                    // - remove the worldnode from
                    // the nodemanager (which controls the loc updater
                    // thread)
                    // - remove all obj in the worldnode from the entity map
                    // - log the user out
                    // - send a response msg to the proxy
                    return false;
                }
                if (! newNode.containsElement(elem)) {
                    throw new MVRuntimeException("quadtreenode.updateobj: new node doesnt point to the object we just added to it");
                }
            }
            return true;
        }
        finally {
            lock.unlock();
        }
    }

    // list of objects in this node
    Set<ElementType> nodeElements = new HashSet<ElementType>();

    // child & parent
    QuadTreeNode<ElementType> parent = null;

    // children nodes
    public ArrayList<QuadTreeNode<ElementType>> getChildren() {
        return children;
    }

    public QuadTreeNode<ElementType> getChild(int i) {
        return children.get(i);
    }

    public QuadTreeNode<ElementType> getChild(Point p) {
        for (QuadTreeNode<ElementType> node : children) {
            if (node.geometry.contains(p)) {
                return node;
            }
        }
        return null;
    }

    /** Get regions intersecting with this QuadTreeNode.  Only leaf nodes
	have regions.
    */
    public ArrayList<Region> getRegions() {
	return regions;
    }

    /** Add region to QuadTreeNode's children.  Region is only added to
        intersecting leaf nodes.
    */
    public void addRegion(Region region) {
	lock.lock();
	try {
	    if (children == null) {
		// Leaf
		Boundary boundary= region.getBoundary();
		// Check if any of the region boundary points is inside
		// the quad node
		List<Point> points= boundary.getPoints();
		for (Point point : points) {
		    if (containsPoint(point)) {
			if (regions == null) {
			    regions= new ArrayList<Region>(5);
			}
			regions.add(region);
			return;
		    }
		}
		// Check if any of the quad node corners are inside the
		// the region
		Collection<Point> corners= geometry.getCorners();
		for (Point point : corners) {
		    if (boundary.contains(point)) {
			if (regions == null) {
			    regions= new ArrayList<Region>(5);
			}
			regions.add(region);
			return;
		    }
		}
	    }
	    else {
		children.get(0).addRegion(region);
		children.get(1).addRegion(region);
		children.get(2).addRegion(region);
		children.get(3).addRegion(region);
	    }
	}
        finally {
            lock.unlock();
        }
    }

    /** Get regions that contain point 'loc'.  Only the node's
	regions are considered.  'loc' should be inside the node.
    */
    public List<Region> getRegionByLoc(Point loc) {
	lock.lock();
	try {
	    if (regions == null)
		return null;
	    List<Region> matchRegions = new ArrayList<Region>();
	    for (Region region : regions) {
		Boundary boundary = region.getBoundary();
		if (boundary != null && boundary.contains(loc)) {
		    matchRegions.add(region);
		}
	    }
	    return matchRegions;
	}
        finally {
            lock.unlock();
        }
    }


    ArrayList<Region> regions = null;

    //  children laid out in 2x2 grid:
    //    0 1
    //    2 3
    ArrayList<QuadTreeNode<ElementType>> children = null;
    QuadTree<ElementType> tree = null;

    /**
     * returns a copied geometry
     */
    public Geometry getGeometry() {
        try {
            lock.lock();
            return (Geometry) geometry.clone();
        }
        finally {
            lock.unlock();
        }
    }

    void setNodeType(NodeType type) {
        this.type = type;
    }

    public enum NodeType { LOCAL, REMOTE, MIXED }

    NodeType type = NodeType.LOCAL;

    Geometry geometry = null;
    int depth = 0;

    public NodeType getNodeType() {
        return type;
    }

    public Set<ElementType> getPerceiverExtentObjects() {
        return perceiverExtentObjects;
    }

    /*
     * The list of objects which, though not contained in this node,
     * are perceivable from the node
     */ 
    Set<ElementType> perceiverExtentObjects = null;

    public transient Lock lock = null;

    static final Logger log = new Logger("QuadTreeNode");
    protected static boolean logPath = false;

}
