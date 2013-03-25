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
import multiverse.mars.objects.*;
import multiverse.mars.plugins.CombatPlugin;
import java.io.*;
import java.util.*;
import java.util.concurrent.*;
import java.util.concurrent.locks.*;

public class MarsEffect {
    public MarsEffect() {
    }

    public MarsEffect(String name) {
        setName(name);
    }

    public String toString() {
        return "[MarsEffect: " + getName() + "]";
    }

    public boolean equals(Object other) {
        MarsEffect otherEffect = (MarsEffect) other;
        boolean val = getName().equals(otherEffect.getName());
        return val;
    }

    public int hashCode() {
        int hash = getName().hashCode();
        return hash;
    }

    /**
     * the name is used to refer to the effect, so use a unique name
     */
    public void setName(String name) { this.name = name; }
    public String getName() { return name; }
    String name = null;

    // add the effect to the object
    public void apply(EffectState state) {
        if (Log.loggingDebug)
            Log.debug("MarsEffect.apply: applying effect " + state.getEffectName() + " to " +
                      state.getObject());
    }

    // remove the effect from the object
    public void remove(EffectState state) {
        if (Log.loggingDebug)
            Log.debug("MarsEffect.remove: removing effect " + state.getEffectName() + " from " +
                      state.getObject());
    }

    // perform the next periodic pulse for this effect on the object
    public void pulse(EffectState state) {
        if (Log.loggingDebug)
            Log.debug("MarsEffect.pulse: pulsing effect " + state.getEffectName() + " on " + state.getObject());
    }

    public long getDuration() { return duration; }
    public void setDuration(long dur) { duration = dur; }
    protected long duration = 0;

    public int getNumPulses() { return numPulses; }
    public  void setNumPulses(int num) { this.numPulses = num; }
    protected int numPulses = 0;
    public long getPulseTime() { return (numPulses > 0) ? (duration/numPulses) : 0; }

    public void setIcon(String icon) { this.icon = icon; }
    public String getIcon() { return (icon == null) ? "UNKNOWN_ICON" : icon; }
    String icon = null;

    public boolean isPeriodic() { return periodic; }
    public void isPeriodic(boolean b) { periodic = b; }
    private boolean periodic = false;

    public boolean isPersistent() { return persistent; }
    public void isPersistent(boolean b) { persistent = b; }
    private boolean persistent = false;

    protected EffectState generateState(CombatInfo caster, CombatInfo obj, Map params) {
        return new EffectState(this, caster, obj, params);
    }

    public static EffectState applyEffect(MarsEffect effect, CombatInfo caster, CombatInfo obj) {
	return applyEffect(effect, caster, obj, null);
    }

    public static EffectState applyEffect(MarsEffect effect, CombatInfo caster, CombatInfo obj, Map params) {
        Lock lock = obj.getLock();
        lock.lock();
        try {
            EffectState state = effect.generateState(caster, obj, params);

            if (effect.isPeriodic() && !effect.isPersistent()) {
                throw new MVRuntimeException("MarsEffect: periodic effects must be persistent");
            }
            if (effect.isPersistent()) {
                obj.addEffect(state);
                if (effect.isPeriodic()) {
                    state.setNextPulse(0);
                    state.schedule(effect.getPulseTime());
                }
                else {
                    state.schedule(effect.getDuration());
                }
            }
            effect.apply(state);
            return state;
        }
        finally {
            lock.unlock();
        }
    }

    public static void removeEffect(MarsEffect.EffectState state) {
        Lock lock = state.getObject().getLock();
        lock.lock();
        try {
            if (!state.getEffect().isPersistent()) {
                Log.error("MarsEffect.removeEffect: tried to remove a persistent effect");
                throw new MVRuntimeException("MarsEffect.removeEffect: tried to remove a persistent effect");
            }
            state.isActive(false);
            Engine.getExecutor().remove(state);
            state.getEffect().remove(state);
            state.getObject().removeEffect(state);
        }
        finally {
            lock.unlock();
        }
    }

    public static class EffectState implements Runnable, Serializable {
        public EffectState() {
        }

        public EffectState(MarsEffect effect, CombatInfo caster, CombatInfo obj, Map params) {
            this.effect = effect;
            this.effectName = effect.getName();
            this.casterOid = caster.getOid();
            this.objOid = obj.getOid();
	    this.params = params;
        }

        public void run() {
            try {
                updateState();
            }
            catch (MVRuntimeException e) {
                Log.exception("EffectState.run: got exception", e);
            }
        }

        public void updateState() {
            if (!isActive()) {
                return;
            }
            if (effect.isPeriodic()) {
                effect.pulse(this);
                nextPulse++;
                if (nextPulse < effect.getNumPulses()) {
                    schedule(effect.getPulseTime());
                    return;
                }
            }
            MarsEffect.removeEffect(this);
        }

        public void schedule(long delay) {
            setTimeRemaining(delay);
            Engine.getExecutor().schedule(this, delay, TimeUnit.MILLISECONDS);
        }

        public void resume() {
            effect = Mars.EffectManager.get(effectName);
            if (Log.loggingDebug)
                Log.debug("MarsEffect.resume: effectName=" + effectName + " effect=" + effect + " timeRemaining="
		          + getTimeRemaining());
            Engine.getExecutor().schedule(this, getTimeRemaining(), TimeUnit.MILLISECONDS);
        }

        public MarsEffect getEffect() { return effect; }
        protected transient MarsEffect effect = null;

        public String getEffectName() { return effectName; }
        public void setEffectName(String effectName) { this.effectName = effectName; }
        protected String effectName;

        public CombatInfo getObject() { return CombatPlugin.getCombatInfo(objOid); }
	public Long getObjectOid() { return objOid; }
	public void setObjectOid(Long oid) { objOid = oid; }
	protected Long objOid = null;

        public CombatInfo getCaster() { return CombatPlugin.getCombatInfo(casterOid); }
	public Long getCasterOid() { return casterOid; }
	public void setCasterOid(Long oid) { casterOid = oid; }
	protected Long casterOid = null;

        public long getNextWakeupTime() { return nextWakeupTime; }
        public long getTimeRemaining() { return nextWakeupTime - System.currentTimeMillis(); }
        public void setTimeRemaining(long time) { nextWakeupTime = System.currentTimeMillis() + time; }
        protected long nextWakeupTime;

        public int getNextPulse() { return nextPulse; }
        public void setNextPulse(int num) { nextPulse = num; }
        protected int nextPulse = 0;

        public boolean isActive() { return active; }
        public void isActive(boolean active) { this.active = active; }
        protected boolean active = true;

	public Map getParams() { return params; }
	public void setParams(Map params) { this.params = params; }
	protected Map params = null;

        private static final long serialVersionUID = 1L;
    }
}
