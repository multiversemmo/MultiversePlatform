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
import java.util.concurrent.locks.Lock;

/**
 * The version of the world node that _only_ the world manager uses,
 * because it refers to the QuadTree, and only the world manager has
 * access to the QuadTree.
 */
public class WMWorldNode extends InterpolatedWorldNode implements
        QuadTreeElement<WMWorldNode> {
    public WMWorldNode() {
        super();
    }

    public WMWorldNode(int perceptionRadius) {
        super();
        this.perceptionRadius = perceptionRadius;
    }
    
    public WMWorldNode(BasicWorldNode node) {
        super(node);
    }

    public void setInterpLoc(Point p) {
	Lock myTreeLock = treeLock;
        if (myTreeLock != null) {
            myTreeLock.lock();
        }
        lock.lock();
        try {
            super.setInterpLoc(p);
            if (isSpawned()) {
                node.getTree().updateElement(this, (Point)p.clone());
            }
        } finally {
            lock.unlock();
            if (myTreeLock != null) {
                myTreeLock.unlock();
            }
        }
    }

    // QuadTreeElement
    public MobilePerceiver<WMWorldNode> getPerceiver() {
        return perceiver;
    }

    public void setPerceiver(MobilePerceiver<WMWorldNode> p) {
        perceiver = p;
    }

    private MobilePerceiver<WMWorldNode> perceiver = null;

    public QuadTreeNode<WMWorldNode> getQuadNode() {
        return node;
    }

    public void setQuadNode(QuadTreeNode<WMWorldNode> node) {
        lock.lock();
        try {
            this.node = node;
            treeLock = (node == null) ? null : node.getTree().getLock();
        }
        finally {
            lock.unlock();
        }
    }

    private transient QuadTreeNode<WMWorldNode> node = null;

    public boolean isLocal() {
        return local;
    }

    public void isLocal(boolean local) {
        // probably set from world manager plugin - in spawn handler
        this.local = local;
    }

    private transient boolean local = false;

    public boolean isSpawned() {
        return (node != null);
    }
    
    public int getPerceptionRadius() {
    	return perceptionRadius;
    }
    public void setPerceptionRadius(int radius) {
    	perceptionRadius = radius;
    }
    
    public int getObjectRadius() {
        return 0;
    }

    public Object getQuadTreeObject() {
        return getObject();
    }

    public void setPathInterpolatorValues(long time, MVVector newDir,
        Point newLoc, Quaternion orientation)
    {
        // Don't interpolated despawned stuff
        if (!isSpawned())
            return;
        super.setPathInterpolatorValues(time,newDir,newLoc,orientation);
    }

    private int perceptionRadius = 0;

    private static final long serialVersionUID = 1L;
}
