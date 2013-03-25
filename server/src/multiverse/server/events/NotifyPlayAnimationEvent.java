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

import java.util.*;
import java.util.concurrent.locks.*;

/**
 * object *IS* playing an animation - this event says other need to know about
 * this obj's animation - we have the list as part of the event because this
 * event may be serialized and the other server/client wont know what the list
 * should be
 */
public class NotifyPlayAnimationEvent extends Event {
    public NotifyPlayAnimationEvent() {
        super();
    }

    public NotifyPlayAnimationEvent(Long oid) {
        super(oid);
    }

    public NotifyPlayAnimationEvent(MVByteBuffer buf, ClientConnection con) {
        super(buf, con);
    }

    /**
     * the object is the object 'playing' the animation
     */
    public NotifyPlayAnimationEvent(MVObject object) {
        super(object);
    }

    public String getName() {
        return "NotifyPlayAnimationEvent";
    }

    public String toString() {
        lock.lock();
        try {
            String s = "[NotifyPlayAnimationEvent obj=" + getObjectOid()
                    + ", size=" + animList.size();
            for (AnimationCommand ac : animList) {
                s += ", [command=" + ac.getCommand() + ", animName="
                        + ac.getAnimName() + ", looping=" + ac.isLoop() + "]";
            }
            return s;
        } finally {
            lock.unlock();
        }
    }

    public MVByteBuffer toBytes() {
        lock.lock();
        try {
            int msgId = Engine.getEventServer().getEventID(this.getClass());

            MVByteBuffer buf = new MVByteBuffer(200);
            buf.putLong(getObjectOid());
            buf.putInt(msgId);

            // write out the animation list size and contents
            if (animList == null) {
                if (Log.loggingDebug)
                    Log.debug("PlayAnimation.toBytes: animList is empty for obj "
                              + getObjectOid());
                buf.putInt(0);
            } else {
                // Log.debug("PlayAnimation.toBytes: obj=" + getObject() +
                // ", animList size=" + animList.size());
                buf.putInt(animList.size());
                Iterator<AnimationCommand> iter = animList.iterator();
                while (iter.hasNext()) {
                    AnimationCommand ac = iter.next();
                    buf.putString(ac.getCommand());
                    // Log.debug("PlayAnimation.toBytes: obj=" + getObject() +
                    // ", animCommand=" + ac.getCommand());
                    if (!ac.getCommand().equals(AnimationCommand.CLEAR_CMD)) {
                        buf.putString(ac.getAnimName());
                        // Log.debug("PlayAnimation.toBytes: obj=" + getObject()
                        // +
                        // ", animName=" + ac.getAnimName());
                        buf.putInt(ac.isLoop() ? 1 : 0);
                        // Log.debug("PlayAnimation.toBytes: obj=" + getObject()
                        // +
                        // ", isLoop=" + ac.isLoop());
                    }
                }
            }

            buf.flip();
            return buf;
        } finally {
            lock.unlock();
        }
    }

    protected void parseBytes(MVByteBuffer buf) {
        lock.lock();
        try {
            buf.rewind();
            long oid = buf.getLong();
            if (Log.loggingDebug)
                Log.debug("PlayAnimation.parseBytes: oid=" + oid);
            setObjectOid(oid);
            /* int msgId = */ buf.getInt();

            List<AnimationCommand> list = new LinkedList<AnimationCommand>();
            int len = buf.getInt();
            if (Log.loggingDebug)
                Log.debug("PlayAnimation.parseBytes: obj=" + getObjectOid()
                          + ", listsize=" + len);
            while (len > 0) {
                String command = buf.getString();
                String animName = buf.getString();
                boolean isLoop = (buf.getInt() == 1);
                AnimationCommand ac = new AnimationCommand();
                if (command.equals("add")) {
                    ac.setCommand("add");
                    ac.setAnimName(animName);
                    ac.isLoop(isLoop);
                } else if (command.equals("clear")) {
                    ac.setCommand("clear");
                }
                list.add(ac);
                --len;
            }
            setAnimList(list);
        } finally {
            lock.unlock();
        }
    }

    public void addAnim(AnimationCommand ac) {
        lock.lock();
        try {
            if (animList == null) {
                animList = new LinkedList<AnimationCommand>();
            }
            animList.add(ac);
        } finally {
            lock.unlock();
        }
    }

    public void setAnimList(List<AnimationCommand> animList) {
        lock.lock();
        try {
            this.animList = new LinkedList<AnimationCommand>(animList);
        } finally {
            lock.unlock();
        }
    }

    public List getAnimList() {
        lock.lock();
        try {
            return new LinkedList<AnimationCommand>(animList);
        } finally {
            lock.unlock();
        }
    }

    // list of AnimationCommand
    private List<AnimationCommand> animList = new LinkedList<AnimationCommand>();

    transient private Lock lock = LockFactory
            .makeLock("NotifyPlayAnimationEventLock");
}
