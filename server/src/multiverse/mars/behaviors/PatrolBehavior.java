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

package multiverse.mars.behaviors;

import java.util.*;
import java.util.concurrent.*;

import multiverse.msgsys.*;
import multiverse.server.objects.*;
import multiverse.server.math.*;
import multiverse.server.util.*;
import multiverse.server.engine.*;
import multiverse.server.plugins.InstanceClient;

public class PatrolBehavior extends Behavior implements MessageCallback, Runnable {
    public PatrolBehavior() {
	super();
    }

    public PatrolBehavior(SpawnData data) {
	super();
	String markerNames = (String)data.getProperty("PatrolPoints");
	if (markerNames != null) {
	    for (String markerName : markerNames.split(",")) {
                markerName = markerName.trim();
                if (markerName.length() == 0)
                    continue;
                Point point = InstanceClient.getMarkerPoint(
                    data.getInstanceOid(), markerName);
		if (point == null) {
		    Log.error("PatrolBehavior: unknown marker=" + markerName +
                        " instanceOid=" + data.getInstanceOid());
		}
		else {
		    point.setY(0);
		    addWaypoint(point);
		}
	    }
	}
    }

    public void initialize() {
        SubjectFilter filter = new SubjectFilter(obj.getOid());
        filter.addType(Behavior.MSG_TYPE_EVENT);
        eventSub = Engine.getAgent().createSubscription(filter, this);
    }
    public void activate() {
        startPatrol();
    }
    public void deactivate() {
        if (eventSub != null) {
	    Engine.getAgent().removeSubscription(eventSub);
            eventSub = null;
        }
    }

    public void handleMessage(Message msg, int flags) {
        if (msg.getMsgType() == Behavior.MSG_TYPE_EVENT) {
	    String event = ((Behavior.EventMessage)msg).getEvent();
	    if (event.equals(BaseBehavior.MSG_EVENT_TYPE_ARRIVED)) {
		Engine.getExecutor().schedule(this, getLingerTime(), TimeUnit.MILLISECONDS);
	    }
        }
        //return true;
    }

    public void addWaypoint(Point wp) {
        waypoints.add(wp);
    }
    protected List<Point> waypoints = new ArrayList<Point>();

    public void setLingerTime(long time) {
        lingerTime = time;
    }
    public long getLingerTime() {
        return lingerTime;
    }
    protected long lingerTime = 2000;

    public void setMovementSpeed(int speed) {
        this.speed = new Integer(speed);
    }
    public int getMovementSpeed() {
        return speed.intValue();
    }
    protected Integer speed = new Integer(3000);

    protected void startPatrol() {
        nextWaypoint = 0;
        nextPatrol();
    }

    protected void sendMessage(Point waypoint, int speed) {
        Engine.getAgent().sendBroadcast(new BaseBehavior.GotoCommandMessage(obj, waypoint, speed));
    }

    protected void nextPatrol() {
	sendMessage(waypoints.get(nextWaypoint), getMovementSpeed());
        nextWaypoint++;
        if (nextWaypoint == waypoints.size()) {
            nextWaypoint = 0;
        }
    }

    public void run() {
        nextPatrol();
    }

    int nextWaypoint = 0;
    Long eventSub = null;
    private static final long serialVersionUID = 1L;

}
