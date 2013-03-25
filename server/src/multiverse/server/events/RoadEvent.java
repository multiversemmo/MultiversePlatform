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

package multiverse.server.events;

import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * sends the client information about a road
 */
public class RoadEvent extends Event {
    public RoadEvent() {
	super();
    }

    public RoadEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public RoadEvent(Road road) {
	super(road);
	setPoints(road.getPoints());
        setRoadName(road.getName());
    }

    public String getName() {
	return "RoadEvent";
    }

    public void setRoadName(String name) {
        this.roadName = name;
    }
    public String getRoadName() {
        return roadName;
    }

    public void setPoints(List<Point> points) {
	lock.lock();
	try {
	    this.points = new LinkedList<Point>(points);
	}
	finally {
	    lock.unlock();
	}
    }

    public List<Point> getPoints() {
	lock.lock();
	try {
	    return new LinkedList<Point>(points);
	}
	finally {
	    lock.unlock();
	}
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(400);

	lock.lock();
	try {
	    buf.putLong(getObjectOid()); 
	    buf.putInt(msgId);
            buf.putString(getRoadName());
	    buf.putInt(points.size());
	    for (Point p : points) {
		buf.putPoint(p);
	    }
	    buf.flip();
	    return buf;
	}
	finally {
	    lock.unlock();
	}
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setObjectOid(buf.getLong());
	/* int msgId = */ buf.getInt();
	
        setRoadName(buf.getString());
	lock.lock();
	try {
	    points = new LinkedList<Point>();
	    int numPoints = buf.getInt();
	    for (int i=0; i<numPoints; i++) {
		Point p = buf.getPoint();
		points.add(p);
	    }
	}
	finally {
	    lock.unlock();
	}
    }

    private String roadName = null;
    private Lock lock = LockFactory.makeLock("RoadEventLock");
    private List<Point> points = null;
}
