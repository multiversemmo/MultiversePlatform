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

package multiverse.mars.core;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.mars.plugins.CombatClient;
import multiverse.mars.plugins.CombatClient.CooldownMessage;
import java.util.*;
import java.io.*;
import java.util.concurrent.*;

public class Cooldown {
    public Cooldown() {
    }

    public Cooldown(String id) {
        setID(id);
    }

    public Cooldown(String id, long duration) {
        setID(id);
        setDuration(duration);
    }
    public String toString() {
        return "[Cooldown: " + getID() + ":" + getDuration() + "]";
    }

    public long getDuration() { return duration; }
    public void setDuration(long dur) { duration = dur; }
    protected long duration = 0;

    public String getID() { return id; }
    public void setID(String id) { this.id = id; }
    protected String id;

    public static void activateCooldown(Cooldown cd, CooldownObject obj) {
        State state = new State(cd.getID(), cd.getDuration(), obj);
        state.start();
        Engine.getAgent().sendBroadcast(new CombatClient.CooldownMessage(state));
    }

    public static void activateCooldowns(Collection<Cooldown> cooldowns, CooldownObject obj) {
        CooldownMessage msg = new CooldownMessage(obj.getOid());
        for (Cooldown cd : cooldowns) {
            State state = new State(cd.getID(), cd.getDuration(), obj);
            state.start();
            msg.addCooldown(state);
        }
        Engine.getAgent().sendBroadcast(msg);
    }

    // returns true if none of the Cooldowns in cdset are active, false otherwise
    public static boolean checkReady(Collection<Cooldown> cdset, CooldownObject obj) {
        for (Cooldown cd : cdset) {
            if (obj.getCooldownState(cd.getID()) != null)
                return false;
        }
        return true;
    }

    public static void resumeCooldowns(CooldownObject obj, Collection<State> cooldowns) {
        for (State state : cooldowns) {
            state.resume();
        }
    }

    public static class State implements Runnable, Serializable {
        public State() {
        }

        public State(String id, long duration, CooldownObject obj) {
            setID(id);
            setDuration(duration);
            setObject(obj);
        }

        public String getID() { return id; }
        public void setID(String id) { this.id = id; }
        protected String id = "UNINIT";

        public CooldownObject getObject() { return obj; }
        public void setObject(CooldownObject obj) { this.obj = obj; }
        protected CooldownObject obj = null;

        public long getDuration() { return duration; }
        public void setDuration(long duration) { this.duration = duration; }
        protected long duration = 0;

        public long getTimeRemaining() { return endTime - System.currentTimeMillis(); }
        public void setTimeRemaining(long time) { endTime = System.currentTimeMillis() + time; }
        public long getEndTime() { return endTime; }
        protected long endTime = 0;

        protected transient boolean running = false;

        public void start() {
            if (running != false) {
                Log.error("Cooldown.State.start: already running");
                return;
            }
            setTimeRemaining(duration);
            obj.addCooldownState(this);
            Engine.getExecutor().schedule(this, duration, TimeUnit.MILLISECONDS);
            running = true;
        }

        public void resume() {
            if (Log.loggingDebug)
                Log.debug("Cooldown.State.resume: resuming cooldown " + id);
            if (running != false) {
                Log.error("Cooldown.State.resume: already running");
                return;
            }
            running = true;
            Engine.getExecutor().schedule(this, duration, TimeUnit.MILLISECONDS);
        }

        public void run() {
            if (running != true) {
                Log.error("Cooldown.State.run: not running");
                return;
            }
            obj.removeCooldownState(this);
            running = false;
        }

        public void cancel() {
            if (running != true) {
                Log.error("Cooldown.State.cancel: not running");
                return;
            }
            Engine.getExecutor().remove(this);
            obj.removeCooldownState(this);
            running = false;
        }

        private static final long serialVersionUID = 1L;
    }

    public static interface CooldownObject {
        public void addCooldownState(State state);
        public void removeCooldownState(State state);
        public State getCooldownState(String id);
        public Long getOid();
    }
}
