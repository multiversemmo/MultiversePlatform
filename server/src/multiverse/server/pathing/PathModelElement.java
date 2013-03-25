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

import multiverse.server.engine.*;
import multiverse.server.math.*;

public class PathModelElement implements QuadTreeElement<PathModelElement>, Locatable {
    public PathModelElement(PathObject pathObject) {
        super();
        this.pathObject = pathObject;
    }

    // QuadTreeElement methods
    public Object getQuadTreeObject() {
        return pathObject;
    }
    
    public QuadTreeNode<PathModelElement> getQuadNode() {
        return node;
    }

    public void setQuadNode(QuadTreeNode<PathModelElement> node) {
        this.node = node;
    }

    public int getPerceptionRadius() {
    	return pathObject.getRadius();
    }
    
    public int getObjectRadius() {
    	return pathObject.getRadius();
    }
    
    // These methods don't matter, because pathing information doesn't
    // concern itself with perceivers
    public MobilePerceiver<PathModelElement> getPerceiver() {
        return null;
    }

    public void setPerceiver(MobilePerceiver<PathModelElement> p) {
    }

    public long getInstanceOid() {
//## instance hardcoded
        return 0L;
    }

    // Locatable methods
    public Point getLoc() {
    	return new Point(pathObject.getCenter());
    }
    
    public Point getCurrentLoc() {
        return new Point(pathObject.getCenter());
    }
    
    public void setLoc(Point p) {
        // ignored
    }

    public long getLastUpdate() {
        // ignored
        return 0;
    }

    public void setLastUpdate(long value) {
        // ignored
    }
    
    protected PathObject pathObject;
    private transient QuadTreeNode<PathModelElement> node = null;

    private static final long serialVersionUID = 1L;
}
