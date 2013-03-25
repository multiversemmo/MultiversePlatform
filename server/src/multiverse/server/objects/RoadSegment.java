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

package multiverse.server.objects;

import multiverse.server.math.*;

/**
 * usually made from a road entity.  this is the actual spawned object
 * in the world
 */
public class RoadSegment extends MVObject {

    public RoadSegment() {
	super();
    }
    
    public RoadSegment(Long oid) {
        super(oid);
    }

    public RoadSegment(String name, Point start, Point end) {
	super(name);
	setStart(start);
	setEnd(end);
    }

    public String toString() {
	return "[RoadSegment: " + super.toString() +
	    " start=" + getStart() +
	    ", end=" + getEnd() + "]";
    }

    /**
     * overrides parent getType - returns ObjectTypes.road
     */
    public ObjectType getType() {
	return ObjectTypes.road;
    }

    public void setStart(Point start) {
	this.start = (Point)start.clone();
    }
    public Point getStart() {
	return (Point)start.clone();
    }

    public void setEnd(Point end) {
	this.end = (Point)end.clone();
    }
    public Point getEnd() {
	return (Point)end.clone();
    }

    Point start;
    Point end;

    private static final long serialVersionUID = 1L;
}
