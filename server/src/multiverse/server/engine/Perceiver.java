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

import multiverse.server.math.*;
import multiverse.server.util.*;
import java.util.*;
/**
 * represents what a mob can see. it actually contains a bunch of perceiver
 * nodes which reside in the quad tree. what it can perceive is the aggregation
 * of what all these individual perceiver nodes can see.
 * 
 */
public abstract class Perceiver<ElementType extends QuadTreeElement<ElementType>> implements
        java.io.Serializable {
    public Perceiver() {
        setupTransient();
    }

    void setupTransient() {
        nodes = new HashSet<QuadTreeNode<ElementType>>();
        callbacks = new HashSet<PerceiverCallback<ElementType>>();

    }

    private void readObject(java.io.ObjectInputStream in)
            throws java.io.IOException, ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    public void registerCallback(PerceiverCallback<ElementType> cb) {
        callbacks.add(cb);
    }

    public void unregisterCallback(PerceiverCallback<ElementType> cb) {
        callbacks.remove(cb);
    }

    transient Set<PerceiverCallback<ElementType>> callbacks = null;

    public abstract boolean overlaps(Geometry g);

    public abstract boolean contains(Geometry g);

    public boolean shouldNotifyNewElement(ElementType elem) {
//         if (Log.loggingDebug)
//             log.debug("Perceiver.shouldNotifyNewElement: owner=" + elem.getQuadTreeObject() + " obj=" + elem.getQuadTreeObject());
        if (filter == null) {
            if (Log.loggingDebug) {
                log.debug("shouldNotifyNewElement: filter is null");
            }
            return false;
        }
        if (filter.matches(this, elem)) {
            if (Log.loggingDebug)
                log.debug("Perceiver.shouldNotifyNewElement: filter matches.  owner=" + elem.getQuadTreeObject() + " obj=" + elem.getQuadTreeObject());
            return true;
        }
        else {
//             log.debug("shouldNotifyNewElement: filter does not match");
            return false;
        }
    }
    
    public boolean shouldFreeElement(ElementType elem) {
        return (filter == null) || filter.matches(this, elem);
    }

    public Integer processNewsAndFrees(PerceiverNewsAndFrees<ElementType> newsAndFrees, Long perceiverOid) {
        Integer perceiverOidCount = null;
        for (PerceiverCallback<ElementType> cb : callbacks) {
            Integer count = cb.processNewsAndFrees(this, newsAndFrees, perceiverOid);
            if (count != null)
                perceiverOidCount = count;
        }
        return perceiverOidCount;
    }

    // This virtual function returns true if the elements visible to a
    // perceiver should be updated if it moves to the loc supplied.
    // The default is false, but MobilePerceivers override this method.
    public boolean shouldUpdateBasedOnLoc(Point loc) {
        return false;
    }
    
    public void addQuadTreeNode(QuadTreeNode<ElementType> node) {
        nodes.add(node);
    }

    public void removeQuadTreeNode(QuadTreeNode<ElementType> node) {
        if (! nodes.remove(node)) {
            if (this instanceof MobilePerceiver) {
                MobilePerceiver<ElementType> p =
                        (MobilePerceiver<ElementType>) this;
                log.error("removeQuadTreeNode on "+ p.getElement().getQuadTreeObject() + ": node " + node +
                        " not in current perceiver list");
            }
            else
                log.error("removeQuadTreeNode: node " + node +
                        " not in current perceiver list");
        }
    }

    public Set<QuadTreeNode<ElementType>> getQuadTreeNodes() {
        return new HashSet<QuadTreeNode<ElementType>>(nodes);
    }

    private transient Set<QuadTreeNode<ElementType>> nodes = null;

    public void setFilter(PerceiverFilter<ElementType> filter) {
        this.filter = filter;
    }

    public PerceiverFilter<ElementType> getFilter() {
        return filter;
    }

    private PerceiverFilter<ElementType> filter = null;

    protected static final Logger log = new Logger("Perceiver");
}

