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

import multiverse.msgsys.*;
import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.math.*;
import multiverse.server.plugins.*;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;

public class TeleporterBehavior extends Behavior implements MessageCallback {
    public TeleporterBehavior() {
	super();
    }

    public void initialize() {
        SubjectFilter filter = new SubjectFilter(obj.getOid());
	filter.addType(ObjectTracker.MSG_TYPE_NOTIFY_REACTION_RADIUS);
        eventSub = Engine.getAgent().createSubscription(filter, this);
    }

    public void activate() {
	activated = true;
	MobManagerPlugin.getTracker(obj.getInstanceOid()).addReactionRadius(obj.getOid(), radius);
    }

    public void deactivate() {
	lock.lock();
	try {
	    activated = false;
	    if (eventSub != null) {
                Engine.getAgent().removeSubscription(eventSub);
		eventSub = null;
	    }
	}
	finally {
	    lock.unlock();
	}
    }

    public void handleMessage(Message msg, int flags) {
	if (activated == false) {
	    return;
	}
	if (msg.getMsgType() == ObjectTracker.MSG_TYPE_NOTIFY_REACTION_RADIUS) {
	    ObjectTracker.NotifyReactionRadiusMessage nMsg = (ObjectTracker.NotifyReactionRadiusMessage)msg;
// 	    Log.debug("TeleporterBehavior: myOid=" + obj.getOid() + " objOid=" + nMsg.getObjOid()
// 		      + " inRadius=" + nMsg.getInRadius() + " wasInRadius=" + nMsg.getWasInRadius());
	    if (nMsg.getInRadius()) {
		reaction(nMsg);
	    }
	}
    }

    public void reaction(ObjectTracker.NotifyReactionRadiusMessage nMsg) {
	BasicWorldNode wnode = new BasicWorldNode();
	wnode.setLoc(destination);
        // tell the worldmanager we've moved
        // this should update everyone near me
        TargetedExtensionMessage teleportBegin =
            new TargetedExtensionMessage(nMsg.getSubject(), nMsg.getSubject());
        teleportBegin.setExtensionType("mv.SCENE_BEGIN");
        teleportBegin.setProperty("action","teleport");
        TargetedExtensionMessage teleportEnd =
            new TargetedExtensionMessage(nMsg.getSubject(), nMsg.getSubject());
        teleportEnd.setExtensionType("mv.SCENE_END");
        teleportEnd.setProperty("action","teleport");
        WorldManagerClient.updateWorldNode(nMsg.getSubject(), wnode, true,
            teleportBegin, teleportEnd);
    }

    public void setRadius(int radius) {
	this.radius = radius;
    }
    public int getRadius() {
	return radius;
    }

    public void setDestination(Point loc) {
	destination = loc;
    }
    public Point getDestination() {
	return destination;
    }

    protected int radius = 0;
    protected Point destination;
    protected boolean activated = false;
    Long eventSub = null;
    private static final long serialVersionUID = 1L;
}
