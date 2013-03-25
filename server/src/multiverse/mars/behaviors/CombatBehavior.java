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
import multiverse.server.objects.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.plugins.*;
import multiverse.server.messages.*;
import multiverse.mars.plugins.*;
import multiverse.mars.objects.*;

public class CombatBehavior extends Behavior implements MessageCallback {
    public CombatBehavior() {
	super();
    }

    public CombatBehavior(SpawnData data) {
	super(data);
	String value = (String)data.getProperty("combat.aggressive");
	if (value != null) {
	    setAggressive(Boolean.valueOf(value));
	}
	value = (String)data.getProperty("combat.reactionRadius");
	if (value != null) {
	    setReactionRadius(Integer.valueOf(value));
	}
	value = (String)data.getProperty("combat.movementSpeed");
	if (value != null) {
	    setMovementSpeed(Integer.valueOf(value));
	}
    }

    public void initialize() {
        SubjectFilter filter = new SubjectFilter(obj.getOid());
        filter.addType(CombatClient.MSG_TYPE_DAMAGE);
        filter.addType(PropertyMessage.MSG_TYPE_PROPERTY);
        if (aggressive) {
            filter.addType(ObjectTracker.MSG_TYPE_NOTIFY_REACTION_RADIUS);
        }
        eventSub = Engine.getAgent().createSubscription(filter, this);
    }
    public void activate() {
	activated = true;
	if (aggressive) {
	    Log.debug("CombatBehavior.activate: adding reaction radius");
	    MobManagerPlugin.getTracker(obj.getInstanceOid()).addReactionRadius(obj.getOid(), reactionRadius);
	}
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

    public Boolean getAggressive() {
	return aggressive;
    }
    public void setAggressive(Boolean val) {
	aggressive = val;
    }
    protected Boolean aggressive = Boolean.FALSE;

    public void setMovementSpeed(int speed) {
        this.speed = new Integer(speed);
    }
    public int getMovementSpeed() {
        return speed.intValue();
    }
    protected Integer speed = new Integer(6000);

    public void setReactionRadius(int radius) {
	reactionRadius = radius;
    }
    public int getReactionRadius() {
	return reactionRadius.intValue();
    }
    protected Integer reactionRadius = new Integer(10000);

    public void handleMessage(Message msg, int flags) {
	lock.lock();
	try {
	    if (activated == false) {
		return;
	    }
	    if (msg instanceof CombatClient.DamageMessage) {
		CombatClient.DamageMessage dmgMsg = (CombatClient.DamageMessage)msg;
		attackTarget(dmgMsg.getAttackerOid());
	    }
	    if (msg.getMsgType() == ObjectTracker.MSG_TYPE_NOTIFY_REACTION_RADIUS) {
		if (!aggressive) {
		    return;
		}
		ObjectTracker.NotifyReactionRadiusMessage nMsg = (ObjectTracker.NotifyReactionRadiusMessage)msg;
                if (Log.loggingDebug)
                    Log.debug("CombatBehavior.onMessage: got reaction message=" + nMsg);
		if (nMsg.getInRadius() && (currentTarget == null)) {
                    Long targetOid = nMsg.getTarget();
		    Long subjectOid = nMsg.getSubject();
		    ObjectStub subject = (ObjectStub)EntityManager.getEntityByNamespace(subjectOid, Namespace.MOB);
                    if (Log.loggingDebug)
                        Log.debug("CombatBehavior.onMessage: targetOid=" + targetOid + " subjectOid=" + subjectOid + " subject=" + subject + " type=" + subject.getType());
		    if (subject.getType() != ObjectTypes.player) {
			return;
		    }
		    attackTarget(subjectOid);
		}
	    }
	    if (msg instanceof PropertyMessage) {
		PropertyMessage propMsg = (PropertyMessage) msg;
		Boolean dead = (Boolean)propMsg.getProperty(CombatInfo.COMBAT_PROP_DEADSTATE);
		if (dead != null && dead) {
                    if (Log.loggingDebug)
                        Log.debug("CombatBehavior.onMessage: obj=" + obj + " got death=" + propMsg.getSubject() + " currentTarget=" + currentTarget);
		    if (propMsg.getSubject() == obj.getOid()) {
			Log.debug("CombatBehavior.onMessage: mob died, deactivating all behaviors");
			for(Behavior behav : obj.getBehaviors()) {
			    behav.deactivate();
			    obj.removeBehavior(behav);
			}
		    }
		    else if (propMsg.getSubject() == currentTarget) {
			attackTarget(null);
		    }
		}
	    }
	    //return true;
	}
	finally {
	    lock.unlock();
	}
    }

    protected void attackTarget(Long targetOid) {
        if (Log.loggingDebug)
            Log.debug("CombatBehavior.attackTarget: obj=" + obj + " targetOid=" + targetOid);

	if (targetSub != null) {
	    Engine.getAgent().removeSubscription(targetSub);
	}

	currentTarget = targetOid;

	if (currentTarget != null) {
	    SubjectFilter filter = new SubjectFilter(targetOid);
	    filter.addType(PropertyMessage.MSG_TYPE_PROPERTY);
	    targetSub = Engine.getAgent().createSubscription(filter, this);

	    Engine.getAgent().sendBroadcast(new BaseBehavior.FollowCommandMessage(obj, new EntityHandle(currentTarget), speed));
	    CombatClient.autoAttack(obj.getOid(), currentTarget, true);
	}
	else {
	    CombatClient.autoAttack(obj.getOid(), null, false);
	    Engine.getAgent().sendBroadcast(new BaseBehavior.ArrivedEventMessage(obj));
	}
    }

    Long eventSub = null;
    Long targetSub = null;
    protected Long currentTarget = null;
    protected boolean activated = false;
    private static final long serialVersionUID = 1L;
}
