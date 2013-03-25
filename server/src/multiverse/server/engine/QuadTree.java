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
import java.util.concurrent.locks.*;
import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.server.math.*;

//
// locking behavior:
// locks when it changes it object list, divides, or joins
//

public class QuadTree<ElementType extends QuadTreeElement<ElementType>> {

    ////////////////////////////////////////////////////////////////////////
    //
    // External API of QuadTree: methods that don't result in news and
    // frees of objects
    //
    ////////////////////////////////////////////////////////////////////////


    // create the quad tree with the passed in geometry
    public QuadTree(Geometry g) {
        createQuadTree(g, 0);
    }
    
    public QuadTree(Geometry g, int hysteresis) {
        createQuadTree(g, hysteresis);
    }
    
    protected void createQuadTree(Geometry g, int hysteresis) {
        supportsExtentBasedPerceiver = true;
        this.hysteresis = hysteresis;
        rootNode = new QuadTreeNode<ElementType>(this, null, g,
                QuadTreeNode.NodeType.REMOTE);
    }

    // create the quad tree with the passed in geometry, and the given
    // setting of whether an extent-based perceiver is supported
    public QuadTree(Geometry g, boolean supportsExtentBasedPerceiver) {
        this.supportsExtentBasedPerceiver = supportsExtentBasedPerceiver;
        rootNode = new QuadTreeNode<ElementType>(this, null, g,
                QuadTreeNode.NodeType.REMOTE);
    }

    QuadTreeNode<ElementType> getRoot() {
        return rootNode;
    }

    public void printTree() {
        rootNode.recurseToString();
    }

    // returns at least all quadtree elements within radius mm around point
    // 'loc'
    public Set<ElementType> getElements(Point loc, int radius) {
        return rootNode.getElements(loc, radius);
    }

    // returns the element closest to loc1 that might intersect a line
    // segment from loc1 to loc2.  the list is "conservative", meaning
    // that it might return entities that don't in fact intersect the
    // segment, but is guaranteed not to omit any elements that might
    // intersect the segment.
    public Set<ElementType> getElementsBetween(Point loc1, Point loc2) {
        return rootNode.getElementsBetween(loc1, loc2);
    }
    
    // returns at least all quadtree elements within radius mm around element
    // passed in
    // will look at adjacent nodes to see what objects are near
    public Set<ElementType> getElements(ElementType elem, int radius) {
        return getElements(elem.getLoc(), radius);
    }

    public Geometry getLocalGeometry() {
        return localGeometry;
    }

    public void addRegion(Region region) {
	rootNode.addRegion(region);
    }

    /**
     * Entrypoint to get the regions containing the point - - needed
     * now that we've done away with the RegionManager
     */
    public List<Region> getRegionsContainingPoint(Point loc) {
        lock.lock();
        try {
            QuadTreeNode<ElementType> newNode = findLeafNode(loc);
            return newNode.getRegionByLoc(loc);
        } finally {
            lock.unlock();
        }
    }
    
    // set the maximum number of objects a given leaf node should have
    // once the node exceeds this number, it will divide
    public void setMaxObjects(int max) {
	if (max > 0 && max != maxObjects) {
	    Log.info("QuadTree maximum-objects-per-node changed from "+
		maxObjects+" to "+max);
	    maxObjects = max;
	}
    }

    public int getMaxObjects() {
        return maxObjects;
    }

    public void setMaxDepth(int max) {
	if (max > 0 && max != maxDepth) {
	    Log.info("QuadTree maximum-depth changed from "+
		maxDepth+" to "+max);
	    maxDepth = max;
	}
    }

    public int getMaxDepth() {
        return maxDepth;
    }

    public boolean getSupportsExtentBasedPerceiver() {
        return supportsExtentBasedPerceiver;
    }

    public Lock getLock() {
        return lock;
    }

    public int getHysteresis() {
        return hysteresis;
    }
    
    public void setHysteresis(int hysteresis) {
        this.hysteresis = hysteresis;
    }
    
    ////////////////////////////////////////////////////////////////////////
    //
    // External API of QuadTree: methods that cause news and frees of
    // objects
    //
    ////////////////////////////////////////////////////////////////////////

    // adds an element to the quad tree- throws Exception if cant go
    // in returns the quadtreenode that it went into returns null if
    // obj cannot go into the quad tree - perhaps outside server
    // geometry
    public QuadTreeNode<ElementType> addElement(ElementType elem) {
        NewsAndFrees newsAndFrees = new NewsAndFrees();
        lock.lock();
        try {
            QuadTreeNode<ElementType> node = addElementInternal(elem, newsAndFrees);
            newsAndFrees.processNewsAndFrees();
            return node;
        } finally {
            lock.unlock();
        }
    }
    
    /**
     * If the perceiverOid is non-null, the count returned is the
     * count of the number of news plus number of frees for the
     * perceiver as a result of adding elem to the quadtree; else
     * null 
     */
    public Integer addElementReturnCountForPerceiver(ElementType elem,
            Long perceiverOid)
    {
        NewsAndFrees newsAndFrees = new NewsAndFrees();
        lock.lock();
        //spawningNewsAndFrees = newsAndFrees;
        try {
            addElementInternal(elem, newsAndFrees);
            Integer count = newsAndFrees.processNewsAndFrees(perceiverOid);
            //spawningNewsAndFrees = null;
            return (count == null) ? 0 : count;
        } finally {
            lock.unlock();
        }
    }
    public NewsAndFrees spawningNewsAndFrees = null;
    
    // removes elem from the quadtree
    public boolean removeElement(ElementType elem) {
        NewsAndFrees newsAndFrees = new NewsAndFrees();
        lock.lock();
        try {
            boolean rv = removeElementInternal(elem, newsAndFrees);
            newsAndFrees.processNewsAndFrees();
            return rv;
        } finally {
            lock.unlock();
        }
    }
 
    // moves elem to the right place in the quadtree if its position has changed
    public void updateElement(ElementType elem, Point loc) {
        NewsAndFrees newsAndFrees = new NewsAndFrees();
        lock.lock();
        try {
            updateElementInternal(elem, loc, newsAndFrees);  
            newsAndFrees.processNewsAndFrees();
        } finally {
            lock.unlock();
        }
    }
            
    protected void updatePerceiver(Perceiver<ElementType> perceiver) {
        NewsAndFrees newsAndFrees = new NewsAndFrees();
        lock.lock();
        try {
            updatePerceiverInternal(perceiver, newsAndFrees);
            newsAndFrees.processNewsAndFrees();
        } finally {
            lock.unlock();
        }
    }
    
    public void addFixedPerceiver(FixedPerceiver<ElementType> perceiver) {
        if (Log.loggingDebug)
            log.debug("QuadTree.addFixedPerceiver p=" + perceiver);
        fixedPerceivers.add(perceiver);
        updatePerceiver(perceiver);
    }

    public void removeFixedPerceiver(FixedPerceiver<ElementType> perceiver) {
        fixedPerceivers.remove(perceiver);
        updatePerceiver(perceiver);
    }

    public void setLocalGeometry(Geometry g) {
        NewsAndFrees newsAndFrees = new NewsAndFrees();
        lock.lock();
        try {
            this.localGeometry = g;
            setLocalGeometryHelper(rootNode, g, newsAndFrees);
            newsAndFrees.processNewsAndFrees();
        } finally {
            lock.unlock();
        }
    }

    public Collection<ElementType> getElementPerceivables(ElementType elem)
    {
        Set<ElementType> result = new HashSet<ElementType>();
        lock.lock();
        try {
            Perceiver<ElementType> perceiver = elem.getPerceiver();
            if (perceiver == null)
                return result;

            Set<QuadTreeNode<ElementType>> nodes = perceiver.getQuadTreeNodes();

            for (QuadTreeNode<ElementType> node : nodes) {
                result.addAll(node.getNodeElements());

                Set<ElementType> perceiverExtentObjects =
                    node.getPerceiverExtentObjects();
                if (perceiverExtentObjects != null)
                    result.addAll(perceiverExtentObjects);
            }

            return result;
        } finally {
            lock.unlock();
        }
    }

    ////////////////////////////////////////////////////////////////////////
    //
    // Internal methods, not called outside of the quadtree
    //
    ////////////////////////////////////////////////////////////////////////

    protected QuadTreeNode<ElementType> addElementInternal(ElementType elem, NewsAndFrees newsAndFrees) {
        Point loc = elem.getLoc();
        lock.lock();
        try {
            if (supportsExtentBasedPerceiver) {
                int radius = elem.getPerceptionRadius();
                if (Log.loggingDebug)
                    Log.debug("QuadTree.addElementInternal: elem " + elem + ", percept radius " + radius);
                if (radius > 0) {
                    rootNode.addPerceiverExtentObject(elem, loc, radius);
                }
            }
            return addHelper(rootNode, elem, loc, newsAndFrees);
        } finally {
            lock.unlock();
        }
    }

    // helper method to support QuadTree.addElement(element) recursion
    protected QuadTreeNode<ElementType> addHelper(
        QuadTreeNode<ElementType> node, ElementType elem, Point loc, NewsAndFrees newsAndFrees) {
        // check if within node range
        if (loc == null) {
            throw new MVRuntimeException("QuadTree.addHelper: obj has null location");
        }
        if (!node.getGeometry().contains(loc)) {
            Log.warn("QuadTree.addHelper: element not within node"
                    + ", element=" + elem + ", quadtreenode=" + node);
            return null;
        }

        if (node.isLeaf()) {
            if (Log.loggingDebug)
                log.debug("QuadTree.addHelper: node is leaf: " + node
                          + ", rootNode is leaf? " + this.rootNode.isLeaf()
                          + ", qtree=" + this.hashCode());

            // are there too many object in this node
            int curSize = node.numElements();
            int maxSize = getMaxObjects();
            if (curSize >= maxSize && node.getDepth() < getMaxDepth()) {
                // divide
                if (Log.loggingDebug)
                    log.debug("QuadTree.addHelper: maxObj=" + maxSize
                              + ", cursize=" + curSize + ".. dividing");
                node.divide(newsAndFrees);
                return addHelper(node, elem, loc, newsAndFrees);
            }

            // there arent too many objects, just add it to this node
            if (Log.loggingDebug)
                log.debug("QuadTree.addHelper: adding element " + elem
                          + " to quadnode " + node + " -- maxObjects="
                          + getMaxObjects());

            updateElementInternal(elem, loc, newsAndFrees);
            if (!node.containsElement(elem)) {
                throw new MVRuntimeException("QuadTree.addHelper: Failed check");
            }
            return node;
        }

        // not a leaf, determine which child to recurse to
        QuadTreeNode<ElementType> childNode = node.whichChild(loc);
        return addHelper(childNode, elem, loc, newsAndFrees);
    }

    protected boolean removeElementInternal(ElementType elem, NewsAndFrees newsAndFrees) {
        lock.lock();
        QuadTreeNode<ElementType> node = elem.getQuadNode();
        try {
            boolean rv = node.removeElement(elem);
            elem.setQuadNode(null);
            for (Perceiver<ElementType> p : node.getPerceivers()) {
                newsAndFrees.noteFreedElement(p, elem);
            }
            Perceiver<ElementType> p = elem.getPerceiver();
            if (p != null) {
                updatePerceiverInternal(p, newsAndFrees);
            }
            return rv;
        } finally {
            lock.unlock();
        }
    }

    protected void updateElementInternal(ElementType elem, Point loc, NewsAndFrees newsAndFrees) {
        lock.lock();
        try {
            if (Log.loggingDebug)
                log.debug("updateElement: elem=" + elem);
            QuadTreeNode<ElementType> node = elem.getQuadNode();
            if ((node != null) && (node.getGeometry().contains(loc))) {
                Perceiver<ElementType> perceiver = elem.getPerceiver();
                if (perceiver != null) {
                    // The perceiver will return true if it's
                    // sufficiently far from this location.  If it
                    // returns true, it will change it's notion of
                    // when it was last updated.
                    if (perceiver.shouldUpdateBasedOnLoc(loc))
                        updatePerceiverInternal(perceiver, newsAndFrees);
                } else {
                    log.debug("updateElementInternal: has no perceiver");
                }
                return;
            }
            QuadTreeNode<ElementType> newNode = findLeafNode(loc);
            if (node != null) {
// 		if (Log.loggingDebug)
// 		    log.debug("QuadTree.updateElementInternal: element moving out of node obj="
//                         + elem.getQuadTreeObject() + " oldNode=" + node + ", loc=" + loc);
                if (! node.removeElement(elem)) {
                    throw new RuntimeException("updateElementInternal: could not remove from node");
                }
		if (Log.loggingDebug)
		    log.debug("QuadTree.updateElementInternal: element moved out of node obj="
                        + elem.getQuadTreeObject() + " oldNode=" + node + " newNode="
                        + newNode + ", loc=" + loc);
            }
            newNode.addElement(elem);
            elem.setQuadNode(newNode);
            updateElementPerceiversInternal(elem, node, newNode, newsAndFrees);
            Perceiver<ElementType> perceiver = elem.getPerceiver();
            if (perceiver != null) {
                updatePerceiverInternal(perceiver, newsAndFrees);
            }
        } finally {
            lock.unlock();
        }
    }

    // notify all perceivers that can no longer perceive elem or that can now do
    // so
    protected void updateElementPerceiversInternal(ElementType elem,
        QuadTreeNode<ElementType> oldNode, QuadTreeNode<ElementType> newNode, NewsAndFrees newsAndFrees) {
        Set<Perceiver<ElementType>> removePerceivers = new HashSet<Perceiver<ElementType>>();

        if (Log.loggingDebug)
            log.debug("updateElementPerceivers: elem=" + elem + " oldNode="
                      + oldNode + " newNode=" + newNode);
        if (oldNode != null) {
            removePerceivers = oldNode.getPerceivers();
        }
        Set<Perceiver<ElementType>> addPerceivers = newNode.getPerceivers();

        removePerceivers.removeAll(newNode.getPerceivers());
        if (oldNode != null) {
            addPerceivers.removeAll(oldNode.getPerceivers());
        }
        if (Log.loggingDebug) {
            log.debug("updateElementPerceivers: remove perceivers size="
                      + removePerceivers.size() + "add perceivers size="
                + addPerceivers.size());
        }
        for (Perceiver<ElementType> p : removePerceivers) {
            if (Log.loggingDebug)
                log.debug("updateElementPerceivers: calling perceiver noteFreed elem="
                          + elem + " p=" + p);
            newsAndFrees.noteFreedElement(p, elem);
        }
        for (Perceiver<ElementType> p : addPerceivers) {
            if (Log.loggingDebug)
                log.debug("updateElementPerceivers: calling perceiver noteNew elem="
                          + elem + " p=" + p);
            newsAndFrees.noteNewElement(p, elem);
        }
    }

    // notify the object's perceiver about what he can see/not see.
    // if you change your set of perceivers, you should be 
    // told about all objects you can see.
    // and you should not have to wait for the other objects to 'move' before you
    // see it.
    protected void updatePerceiverInternal(Perceiver<ElementType> perceiver, NewsAndFrees newsAndFrees) {
        lock.lock();
        try {
            if (perceiver instanceof MobilePerceiver) {
                MobilePerceiver<ElementType> p = (MobilePerceiver<ElementType>) perceiver;
                if (Log.loggingDebug)
                    log.debug("QuadTree.updatePerceiver: mobile perceiver radius="
                        + p.getRadius() + " owner="
                        + p.getElement().getQuadTreeObject());
            } else {
                FixedPerceiver<ElementType> p = (FixedPerceiver<ElementType>) perceiver;
                if (Log.loggingDebug)
                    log.debug("QuadTree.updatePerceiver: fixed perceiver geom="
                        + p.getGeometry());
            }

            Set<QuadTreeNode<ElementType>> oldNodes = perceiver.getQuadTreeNodes();
            //             log.debug("For perceiver " + perceiver.toString() + ", oldNodes size is " + oldNodes.size());
            Set<QuadTreeNode<ElementType>> removeNodes = new HashSet<QuadTreeNode<ElementType>>(
                oldNodes);
            Set<QuadTreeNode<ElementType>> newNodes = new HashSet<QuadTreeNode<ElementType>>();
            // get all nodes that the perceiver overlaps with
            updatePerceiverHelper(newNodes, rootNode, perceiver);
            Set<QuadTreeNode<ElementType>> addNodes = new HashSet<QuadTreeNode<ElementType>>(
                newNodes);
            
            // form the set of perceiver-extent objects visible from
            // old nodes
            Set<ElementType> oldPerceiverExtentElements = new HashSet<ElementType>();
            for (QuadTreeNode<ElementType> node : oldNodes) {
                //                 log.debug("Checking for perceiver extent objects in node " + node.toString());
                Set<ElementType> perceiverExtentObjects = node.getPerceiverExtentObjects();
                if (perceiverExtentObjects != null) {
                    // log.debug("Found " + perceiverExtentObjects.size() + " perceiver extent objects");
                    for (ElementType elem : perceiverExtentObjects) {
                        // log.debug("Adding perceiver extent object " + elem.toString() + " to oldPerceiverExtentObjects");
                        oldPerceiverExtentElements.add(elem);
                    }
                }
            }
            // form the set of perceiver-extent objects visible from
            // new nodes
            Set<ElementType> newPerceiverExtentElements = new HashSet<ElementType>();
            // form the set of elements that are in both lists
            Set<ElementType> bothPerceiverExtentElements = new HashSet<ElementType>();
            for (QuadTreeNode<ElementType> node : addNodes) {
                Set<ElementType> perceiverExtentObjects = node.getPerceiverExtentObjects();
                if (perceiverExtentObjects != null) {
                    for (ElementType elem : perceiverExtentObjects) {
                        // log.debug("Adding perceiver extent object " + elem.toString() + " to newPerceiverExtentObjects");
                        newPerceiverExtentElements.add(elem);
                        if (oldPerceiverExtentElements.contains(elem))
                            bothPerceiverExtentElements.add(elem);
                    }
                }
            }
            // this is the set of nodes that the perceiver used to be in, but is no longer
            // aka: the ones we want to remove from the perceiver list
            removeNodes.removeAll(newNodes);
            
            // this is the set of nodes that the perceiver is now in, but not before
            // aka: the ones we need to add to the perceiver list
            addNodes.removeAll(oldNodes);
            
            if (Log.loggingDebug)
                log.debug("Before removing, newPerceiverExtentElements.size(): " + newPerceiverExtentElements.size() +
                    " oldPerceiverExtentElements.size(): " + oldPerceiverExtentElements.size() +
                    " bothbothPerceiverExtentElements.size(): " + bothPerceiverExtentElements.size() +
                    " num remove nodes=" + removeNodes.size());
            // eliminate any elements in the to-be-removed list and
            // the new list that are in the "both" list
            // tell perceivers to remove any elements that in the node
            // difference, as long as they aren't in the
            // allPerceiverExtentElements list
            for (QuadTreeNode<ElementType> node : removeNodes) {
                if (Log.loggingDebug)
                    log.debug("updatePerceiver: removing from node " + node);
                perceiver.removeQuadTreeNode(node);
                node.removePerceiver(perceiver);
                for (ElementType elem : node.getNodeElements()) {
                    // If it's neither in the old or new perceiver
                    // extent lists, free the object
                    if (!newPerceiverExtentElements.contains(elem) &&
                        !oldPerceiverExtentElements.contains(elem))
                        newsAndFrees.noteFreedElement(perceiver, elem);
                }
            }
            // Now that we've used the new and old extent perceiver
            // lists to avoid freeing node elements, we can remove the
            // elements that are in both from each of the two lists.
            oldPerceiverExtentElements.removeAll(bothPerceiverExtentElements);
            newPerceiverExtentElements.removeAll(bothPerceiverExtentElements);
            if (Log.loggingDebug)
                log.debug("After removing, newPerceiverExtentElements.size(): " + newPerceiverExtentElements.size() +
                    " oldPerceiverExtentElements.size(): " + oldPerceiverExtentElements.size());
            // Free the old perceiver extent object, because we know
            // for sure that they are not in the new perceiver extent
            // list
            for (ElementType elem : oldPerceiverExtentElements) {
                if (Log.loggingDebug)
                    log.debug("updatePerceiver: removing oldPerceiverExtentElement " + elem);
                newsAndFrees.noteFreedElement(perceiver, elem);
            }
            if (Log.loggingDebug)
                log.debug("updatePerceiver: num addnodes=" + addNodes.size());
            // now add any new elements, as long as they aren't in the
            // new perceiver extent list.
            for (QuadTreeNode<ElementType> node : addNodes) {
                if (Log.loggingDebug)
                    log.debug("updatePerceiver: adding to node " + node);
                perceiver.addQuadTreeNode(node);
                node.addPerceiver(perceiver);
                for (ElementType elem : node.getNodeElements()) {
                    // don't add the perceiver now, if it's going to
                    // be added later
                    if (!newPerceiverExtentElements.contains(elem) &&
			!bothPerceiverExtentElements.contains(elem))
                        newsAndFrees.noteNewElement(perceiver, elem);
                }
            }
            // finally, add the elements that are in the
            // newPerceiverExtentElements list
            for (ElementType elem : newPerceiverExtentElements) {
                if (Log.loggingDebug)
                    log.debug("updatePerceiver: adding newPerceiverExtentElement " + elem);
                newsAndFrees.noteNewElement(perceiver, elem);
            }
            
            log.debug("updatedPerceiver: done updating, printing out list of all nodes");
            if (Log.loggingDebug) {
                for (QuadTreeNode<ElementType> node :
                                perceiver.getQuadTreeNodes())
                    log.debug("updatePerceiver: IS IN NODE " + node);
            }
        } finally {
            lock.unlock();
        }
    }

    /**
     * adds a all nodes to nodeSet in or under
     * 'node' that the passed in perceiver overlaps with
     * 
     * @param nodeSet
     * @param node
     * @param perceiver
     */
    protected void updatePerceiverHelper(
            Set<QuadTreeNode<ElementType>> nodeSet,
            QuadTreeNode<ElementType> node, Perceiver<ElementType> perceiver) {
        if (perceiver.overlaps(node.getGeometry())) {
            if (!(node.isLeaf())) {
//                 if (Log.loggingDebug)
//                     log.debug("updatePerceiverHelper: node is not leaf: " + node
//                               + ", rootNode leaf? " + this.rootNode.isLeaf()
//                               + ", qtree=" + this.hashCode());
                for (QuadTreeNode<ElementType> child : node.getChildren()) {
                    updatePerceiverHelper(nodeSet, child, perceiver);
                }
            } else {
                // is leaf node
//                 if (Log.loggingDebug)
//                     log.debug("updatePerceiverHelper: node is leaf: " + node
//                               + ", rootNode leaf? " + this.rootNode.isLeaf()
//                               + ", qtree=" + this.hashCode());
                nodeSet.add(node);
            }
        }
    }

    protected void setLocalGeometryHelper(QuadTreeNode<ElementType> node,
            Geometry g, NewsAndFrees newsAndFrees) {
        // no overlap, do nothing
        if (!g.overlaps(node.getGeometry())) {
            return;
        }
        // entirely contains node, set it local
        if (g.contains(node.getGeometry())) {
            node.setNodeType(QuadTreeNode.NodeType.LOCAL);
        } else {
            // node is partially covered, subdivide
            if (node.isLeaf()) {
                log.debug("setLocalGeometryHelper: divide");
                node.divide(newsAndFrees);
            }
            node.setNodeType(QuadTreeNode.NodeType.MIXED);
        }

        if (!node.isLeaf()) {
            for (QuadTreeNode<ElementType> child : node.getChildren()) {
                setLocalGeometryHelper(child, g, newsAndFrees);
            }
        }
    }

    protected Set<FixedPerceiver<ElementType>> fixedPerceivers = new HashSet<FixedPerceiver<ElementType>>();

    // returns the leaf node that contains the point passed in
    QuadTreeNode<ElementType> findLeafNode(Point loc) {
        return findLeafNodeHelper(rootNode, loc);
    }

    QuadTreeNode<ElementType> findLeafNodeHelper(
            QuadTreeNode<ElementType> node, Point loc) {
        if (node.isLeaf()) {
            if (node.getGeometry().contains(loc)) {
                return node;
            } else {
                return null;
            }
        }
        // not a leaf node
        QuadTreeNode<ElementType> childNode = node.getChild(loc);
        return findLeafNodeHelper(childNode, loc);
    }

    private Geometry localGeometry = null;

    private int hysteresis = 0;
    
    private int maxObjects = 30;

    private int maxDepth = 20;

    private QuadTreeNode<ElementType> rootNode = null;

    private Lock lock = LockFactory.makeLock("QuadTreeLock");

    boolean supportsExtentBasedPerceiver;

    /**
     * Instances of this class are used to accumulate news and frees
     * of objects, from the point of view of perceivers, so that we
     * can perform the new and free operations, which generate
     * messages, without holding the top-level quad tree lock.
     */
    public class NewsAndFrees {

        protected Map<Perceiver<ElementType>, PerceiverNewsAndFrees<ElementType>> perceiverMap;

        public NewsAndFrees () {
            perceiverMap = new HashMap<Perceiver<ElementType>, PerceiverNewsAndFrees<ElementType>>();
        }

        // Perception debugging aid - see WorldManagerPlugin
        public Map<Perceiver<ElementType>, PerceiverNewsAndFrees<ElementType>> 
                getMap() {
            return perceiverMap;
        }

        /**
         * If the perceiver says the element should be a new element,
         * add it to the newElements list for the perceiver
         */
        public void noteNewElement(Perceiver<ElementType> perceiver, ElementType element) {
            if (perceiver.shouldNotifyNewElement(element)) {
                PerceiverNewsAndFrees<ElementType> newsAndFrees = perceiverMap.get(perceiver);
                if (newsAndFrees == null) {
                    newsAndFrees = new PerceiverNewsAndFrees<ElementType>();
                    perceiverMap.put(perceiver, newsAndFrees);
                }
                newsAndFrees.addNewElement(element);
            }
        }

        /**
         * If the perceiver says the element should be a freed
         * element, add it to the freedElements list
         */
        public void noteFreedElement(Perceiver<ElementType> perceiver, ElementType element) {
            if (perceiver.shouldFreeElement(element)) {
                PerceiverNewsAndFrees<ElementType> newsAndFrees = perceiverMap.get(perceiver);
                if (newsAndFrees == null) {
                    newsAndFrees = new PerceiverNewsAndFrees<ElementType>();
                    perceiverMap.put(perceiver, newsAndFrees);
                }
                newsAndFrees.addFreedElement(element);
            }
        }

        public Integer processNewsAndFrees() {
            processNewsAndFrees(-1L);
            return null;
        }
        
        /**
         * Either process all news and frees in a single change
         * subscription message, or process them one-by-one.
         */
        public Integer processNewsAndFrees(Long perceiverOid) {
            return processBatchedNewsAndFrees(perceiverOid);
        }
        
        /**
         * If the perceiverOid is non-null, the count returned is the
         * count of the number of news plus number of frees for the
         * perceiver identified by perceiverOid
         */
        protected Integer processBatchedNewsAndFrees(Long perceiverOid) {
            int news = 0;
            int frees = 0;
            Integer perceiverOidCount = null;
            for (PerceiverNewsAndFrees<ElementType> newsAndFrees : perceiverMap.values()) {
                news += newsAndFrees.newCount();
                frees += newsAndFrees.freedCount();
            }
            boolean workToDo = news > 0 || frees > 0;
            if (Log.loggingDebug && workToDo)
                    Log.debug("QuadTree.NewsAndFrees.processBatchedNewsAndFrees: starting to process " +
                        frees + " frees, " + news + " news");

            if (workToDo) {
                for (Map.Entry<Perceiver<ElementType>, PerceiverNewsAndFrees<ElementType>> entry : perceiverMap.entrySet()) {
                    Perceiver<ElementType> perceiver = entry.getKey();
                    PerceiverNewsAndFrees<ElementType> newsAndFrees = entry.getValue();
                    Integer count = perceiver.processNewsAndFrees(newsAndFrees, perceiverOid);
                    if (count != null)
                        perceiverOidCount = count;
                }
            }
            
            if (Log.loggingDebug) {
                if (frees > 0 || news > 0)
                    Log.debug("QuadTree.NewsAndFrees.processBatchedNewsAndFrees: finished processing " +
                        frees + " frees, " + news + " news");
            }
            return perceiverOidCount;
        }
    }

    protected static final Logger log = new Logger("QuadTree");
}
