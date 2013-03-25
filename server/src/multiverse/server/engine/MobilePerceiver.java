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

public class MobilePerceiver<ElementType extends QuadTreeElement<ElementType>> extends Perceiver<ElementType> {

    public MobilePerceiver() {
    }
    
    public MobilePerceiver(ElementType elem) {
	setElement(elem);
    }

    public MobilePerceiver(ElementType elem, int radius) {
	setElement(elem);
	setRadius(radius);
    }

    public String toString() {
	return "[MobilePerceiver:" + hashCode() + " elem=" + element + " radius=" + radius + "]";
    }

    public boolean overlaps(Geometry g) {
	// if the perceiver's element isn't spawned, we can't detect anything
	if (element.getQuadNode() == null) {
	    return false;
	}
	Point loc = element.getCurrentLoc();
	Geometry geom = new Geometry(loc.getX() - radius, loc.getX() + radius,
				     loc.getZ() - radius, loc.getZ() + radius);
	return geom.overlaps(g);
    }
    public boolean contains(Geometry g) {
	// if the perceiver's element isn't spawned, we can't detect anything
	if (element.getQuadNode() == null) {
	    return false;
	}
	Point loc = element.getCurrentLoc();
	Geometry geom = new Geometry(loc.getX() - radius, loc.getX() + radius,
				     loc.getZ() - radius, loc.getZ() + radius);
	return geom.contains(g);
    }

    // This virtual function returns true if the elements visible to a
    // perceiver should be updated if it moves to the loc supplied.
    // MobilePerceivers determine whether elements should be updated
    // based on whether the new location is further than
    // defaultPerceiverUpdateDistance from the last update position.
    public boolean shouldUpdateBasedOnLoc(Point loc) {
        if (lastUpdateLoc == null ||
            Point.distanceToSquared(loc, lastUpdateLoc) > perceiverUpdateDistanceSquared) {
            Point previousLastUpdateLoc = lastUpdateLoc;
            lastUpdateLoc = (Point)loc.clone();
            if (Log.loggingDebug)
                Log.debug("MobilePerceiver.shouldUpdateBasedOnLoc: returning true; loc " + loc + 
                    ", previousLastUpdateLoc " + previousLastUpdateLoc);
            return true;
        }
        else
            return false;
    }
    
    public ElementType getElement() {
	return element;
    }
    public void setElement(ElementType elem) {
	element = elem;
    }
    ElementType element = null;

    public int getRadius() {
	return radius;
    }
    public void setRadius(int radius) {
	this.radius = radius;
    }
    
    public int getPerceiverUpdateDistance() {
        return perceiverUpdateDistance;
    }
    
    public void setPerceiverUpdateDistance(int perceiverUpdateDistance) {
        this.perceiverUpdateDistance = perceiverUpdateDistance;
        this.perceiverUpdateDistanceSquared = 
            ((float)perceiverUpdateDistance) * ((float)perceiverUpdateDistance);
    }

    private int radius = 0;
    private Point lastUpdateLoc = null;
    
    // The default 
    private int perceiverUpdateDistance = 5000;

    private float perceiverUpdateDistanceSquared = 5000f * 5000f;
    
    private static final long serialVersionUID = 1L;
}
