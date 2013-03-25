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

import java.util.concurrent.*;
import java.util.concurrent.locks.*;

import multiverse.msgsys.*;
import multiverse.server.objects.*;
import multiverse.server.math.*;
import multiverse.server.util.*;
import multiverse.server.pathing.*;
import multiverse.server.plugins.*;

public class BaseBehavior extends Behavior implements Runnable {
    public BaseBehavior() {
	super();
    }

    public BaseBehavior(SpawnData data) {
	super(data);
    }
    
    public void initialize() {
        lock = LockFactory.makeLock("BaseBehaviorLock");
        long oid = obj.getOid();
        SubjectFilter filter = new SubjectFilter(oid);
        filter.addType(Behavior.MSG_TYPE_COMMAND);
        filter.addType(WorldManagerClient.MSG_TYPE_MOB_PATH_CORRECTION);
        pathState = new PathState(oid, pathObjectTypeName, true);
        commandSub = Engine.getAgent().createSubscription(filter, this);
    }
    public void activate() {
	activated = true;
    }

    public void deactivate() {
	lock.lock();
	try {
	    activated = false;
	    if (commandSub != null) {
	        Engine.getExecutor().remove(this);
	        Engine.getAgent().removeSubscription(commandSub);
		commandSub = null;
	    }
        }
	finally {
	    lock.unlock();
	}
    }

    public void handleMessage(Message msg, int flags) {
        try {
            lock.lock();
	    if (activated == false)
		return; //return true;
            if (msg.getMsgType() == Behavior.MSG_TYPE_COMMAND) {
                Behavior.CommandMessage cmdMsg = (Behavior.CommandMessage)msg;
                String command = cmdMsg.getCmd();
                // Remove the executor, because anything we do will
                // end the current execution.
                Engine.getExecutor().remove(this);
                if (Log.loggingDebug)
                    Log.debug("BaseBehavior.onMessage: command = " + command + "; oid = " + obj.getOid() + "; name " + obj.getName());
                if (command.equals(MSG_CMD_TYPE_GOTO)) {
                    GotoCommandMessage gotoMsg = (GotoCommandMessage)msg;
                    Point destination = gotoMsg.getDestination();
                    mode = MSG_CMD_TYPE_GOTO;
                    roamingBehavior = true;
                    gotoSetup(destination, gotoMsg.getSpeed());
                }
                else if (command.equals(MSG_CMD_TYPE_STOP)) {
                    followTarget = null;
                    pathState.clear();
                    obj.getWorldNode().setDir(new MVVector(0,0,0));
                    obj.updateWorldNode();
                    mode = MSG_CMD_TYPE_STOP;
                    // If roamingBehavior is set, that means that we
                    // used formerly had a roaming behavior, so send
                    // an ArrivedEventMessage so that the other
                    // behavior starts up again.
                    if (roamingBehavior) {
                        try {
                            Engine.getAgent().sendBroadcast(new ArrivedEventMessage(obj));
                        }
                        catch (Exception e) {
                            Log.error("BaseBehavior.onMessage: Error sending ArrivedEventMessage, error was '" + e.getMessage() + "'");
                            throw new RuntimeException(e);
                        }
                    }
                }
                else if (command.equals(BaseBehavior.MSG_CMD_TYPE_FOLLOW)) {
                    FollowCommandMessage followMsg = (FollowCommandMessage)msg;
                    mode = MSG_CMD_TYPE_FOLLOW;
                    followSetup(followMsg.getTarget(), followMsg.getSpeed());
                }
            }
            else if (msg.getMsgType() == WorldManagerClient.MSG_TYPE_MOB_PATH_CORRECTION) {
                Engine.getExecutor().remove(this);
                interpolatePath();
                interpolatingPath = false;
            }
            //return true;
        }
        finally {
            lock.unlock();
        }
    }

    public void gotoSetup(Point dest, int speed) {
        // calculate the vector to the destination
        destLoc = dest;
        mobSpeed = speed;
        Point myLoc = obj.getWorldNode().getLoc();
        long oid = obj.getOid();
        if (Log.loggingDebug)
            Log.debug("BaseBehavior.gotoSetup: oid = " + oid + "; myLoc = " + myLoc + "; dest = " + dest);
        scheduleMe(setupPathInterpolator(oid, myLoc, dest, false, obj.getWorldNode().getFollowsTerrain()));
    }

    public void gotoUpdate() {
        Point myLoc = obj.getWorldNode().getLoc();
        long oid = obj.getOid();
        if (interpolatingPath) {
            interpolatePath();
            if (!interpolatingPath) {
                Engine.getAgent().sendBroadcast(new ArrivedEventMessage(obj));
                if (Log.loggingDebug)
                    Log.debug("BaseBehavior.gotoUpdate sending ArrivedEventMessage: oid = " + oid + "; myLoc = " + myLoc + "; destLoc = " + destLoc);
                mode = MSG_CMD_TYPE_STOP;
            }
        }
        if (interpolatingPath)
            scheduleMe(pathState.pathTimeRemaining());
    }

    public void followSetup(EntityHandle target, int speed) {
        followTarget = target;
        mobSpeed = speed;
        InterpolatedWorldNode node = obj.getWorldNode();
        Point myLoc = node.getLoc();
        long oid = obj.getOid();
        ObjectStub followObj = (ObjectStub)followTarget.getEntity(Namespace.MOB);
        Point followLoc = followObj.getWorldNode().getLoc();
        destLoc = followLoc;
        scheduleMe(setupPathInterpolator(oid, myLoc, followLoc, true, node.getFollowsTerrain()));
    }
    
    protected void scheduleMe(long timeToDest) {
        long ms = Math.min((long)500, timeToDest);
//         if (Log.loggingDebug)
//             Log.debug("BaseBehavior.scheduleMe: ms = " + ms);
        Engine.getExecutor().schedule(this, ms, TimeUnit.MILLISECONDS);
    }
    
    public void followUpdate() {
        ObjectStub followObj = (ObjectStub)followTarget.getEntity(Namespace.MOB);
        Point followLoc = followObj.getWorldNode().getLoc();
        InterpolatedWorldNode node = obj.getWorldNode();
        Point myLoc = node.getLoc();
        long oid = obj.getOid();
        float fdist = Point.distanceTo(followLoc, destLoc);
        float dist = Point.distanceTo(followLoc, myLoc);
        if (Log.loggingDebug)
            Log.debug("BaseBehavior.followUpdate: oid = " + oid + "; myLoc = " + myLoc + "; followLoc = " + followLoc + 
                      "; fdist = " + fdist + "; dist = " + dist);
        long msToSleep = (long)500;
        // If the new target location is more than a meter from
        // the old one, create a new path.
        if (fdist > 1000) {
            long msToDest = setupPathInterpolator(oid, myLoc, followLoc, true, node.getFollowsTerrain());
            destLoc = followLoc;
            msToSleep = msToDest == 0 ? (long)500 : Math.min((long)500, msToDest);
        }
        // Else if we're interpolating, interpolate the current path
        else if (interpolatingPath) {
            interpolatePath();
            if (Log.loggingDebug)
                Log.debug("baseBehavior.followUpdate: oid = " + oid + "; interpolated myLoc = " + obj.getWorldNode().getLoc());
        }
        scheduleMe(interpolatingPath ? msToSleep : pathState.pathTimeRemaining());
    }
            
    protected long setupPathInterpolator(long oid, Point myLoc, Point dest, boolean follow, boolean followsTerrain) {
        long timeNow = System.currentTimeMillis();
        WorldManagerClient.MobPathReqMessage reqMsg = pathState.setupPathInterpolator(timeNow, myLoc, dest, mobSpeed, follow, followsTerrain);
        if (reqMsg != null) {
            try {
                Engine.getAgent().sendBroadcast(reqMsg);
                if (Log.loggingDebug)
                    Log.debug("BaseBehavior.setupPathInterpolator: send MobPathReqMessage " + reqMsg);
            }
            catch (Exception e) {
                throw new RuntimeException(e);
            }
            interpolatingPath = true;
            return pathState.pathTimeRemaining();
        }
        else {
            interpolatingPath = false;
            return 0;
        }
    }
    
    protected void cancelPathInterpolator(long oid) {
        WorldManagerClient.MobPathReqMessage cancelMsg = new WorldManagerClient.MobPathReqMessage(oid);
        try {
            Engine.getAgent().sendBroadcast(cancelMsg);
        }
        catch (Exception e) {
            throw new RuntimeException(e);
        }
    }
    
    protected boolean interpolatePath() {
        long timeNow = System.currentTimeMillis();
        PathLocAndDir locAndDir = pathState.interpolatePath(timeNow);
        long oid = obj.getOid();
//         if (locAndDir != null) {
//             if (Log.loggingDebug) {
//                 Log.debug("BaseBehavior.interpolatePath: oid = " + oid + "; loc = " + locAndDir.getLoc() + "; dir = " + locAndDir.getDir());
//             }
//         }
//         else {
//             if (Log.loggingDebug)
//                 Log.debug("BaseBehavior.interpolatePath: oid = " + oid + "; locAndDir is null");
//         }
        if (locAndDir == null) {
            // We have arrived - - turn off interpolation, and cancel that path
            interpolatingPath = false;
            if (Log.loggingDebug)
                Log.debug("BaseBehavior.interpolatePath: cancelling path: oid = " + oid + "; myLoc = " + obj.getWorldNode().getLoc());
            cancelPathInterpolator(oid);
            obj.getWorldNode().setDir(new MVVector(0,0,0));
        } else {
            obj.getWorldNode().setPathInterpolatorValues(timeNow, locAndDir.getDir(), 
                                                         locAndDir.getLoc(), locAndDir.getOrientation());
	    MobManagerPlugin.getTracker(obj.getInstanceOid()).updateEntity(obj);
	}
        return interpolatingPath;
    }

    public void run() {
        try {
            lock.lock();
	    if (activated == false) {
		return;
	    }
            try {
                if (mode == MSG_CMD_TYPE_GOTO) {
                    gotoUpdate();
                }
                else if (mode == MSG_CMD_TYPE_FOLLOW) {
                    followUpdate();
                }
                else if (mode == MSG_CMD_TYPE_STOP) {
                }
                else {
                    Log.error("BaseBehavior.run: invalid mode");
                }
            }
            catch (Exception e) {
                Log.exception("BaseBehavior.run caught exception raised during run for mode = " + mode, e);
                throw new RuntimeException(e);
            }
        }
        finally {
            lock.unlock();
        }
    }

    public static class GotoCommandMessage extends Behavior.CommandMessage {

        public GotoCommandMessage() {
            super(MSG_CMD_TYPE_GOTO);
        }

        public GotoCommandMessage(ObjectStub obj, Point dest, Integer speed) {
            super(obj, MSG_CMD_TYPE_GOTO);
            setDestination(dest);
            setSpeed(speed);
        }

        public Point getDestination() {
            return dest;
        }
        public void setDestination(Point dest) {
            this.dest = dest;
        }

        public Integer getSpeed() {
            return speed;
        }
        public void setSpeed(Integer speed) {
            this.speed = speed;
        }
        
        private Point dest;
        private Integer speed;
        
        private static final long serialVersionUID = 1L;
    }

    public static class FollowCommandMessage extends Behavior.CommandMessage {

        public FollowCommandMessage() {
            super(MSG_CMD_TYPE_FOLLOW);
        }
        
        public FollowCommandMessage(ObjectStub obj, EntityHandle target, Integer speed) {
            super(obj);
            setTarget(target);
	    setSpeed(speed);
        }

        public EntityHandle getTarget() {
            return target;
        }
        public void setTarget(EntityHandle target) {
            this.target = target;
        }

        public Integer getSpeed() {
            return speed;
        }
        public void setSpeed(Integer speed) {
            this.speed = speed;
        }

        private EntityHandle target;
        private Integer speed;
        
        private static final long serialVersionUID = 1L;
    }

    public static class StopCommandMessage extends Behavior.CommandMessage {

        public StopCommandMessage() {
            super(MSG_CMD_TYPE_STOP);
        }

        public StopCommandMessage(ObjectStub obj) {
            super(obj, MSG_CMD_TYPE_STOP);
        }

        private static final long serialVersionUID = 1L;
    }

    public static class ArrivedEventMessage extends Behavior.EventMessage {

        public ArrivedEventMessage() {
            super();
            setEvent(MSG_EVENT_TYPE_ARRIVED);
        }
        
        public ArrivedEventMessage(ObjectStub obj) {
            super(obj);
            setEvent(MSG_EVENT_TYPE_ARRIVED);
        }

        private static final long serialVersionUID = 1L;
    }
    
    protected String getPathObjectTypeName() {
    	return pathObjectTypeName;
    }

    // ??? How does this get initialized?  It should be a property of
    // the mob.  For now, I'll just set it to "Generic"
    String pathObjectTypeName = "Generic";  
    
    Long commandSub = null;

    Point destLoc = null;
    long arriveTime = 0;

    // The state of the pathing system for this mob
    PathState pathState = null;

    EntityHandle followTarget = null;
    int mobSpeed = 0;
    boolean interpolatingPath = false;

    transient protected Lock lock = null;
    
    protected String mode = MSG_CMD_TYPE_STOP;
    protected boolean roamingBehavior = false;
    protected boolean activated = false;

    public static final String MSG_CMD_TYPE_GOTO = "goto";
    public static final String MSG_CMD_TYPE_FOLLOW = "follow";
    public static final String MSG_CMD_TYPE_STOP = "stop";

    public static final String MSG_EVENT_TYPE_ARRIVED = "arrived";

    private static final long serialVersionUID = 1L;
}
