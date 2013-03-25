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

package multiverse.mars.plugins;

import java.util.*;
import java.io.Serializable;
import multiverse.msgsys.*;
import multiverse.server.util.*;
import multiverse.server.engine.*;
import multiverse.server.network.MVByteBuffer;
import multiverse.mars.core.*;

//
// client for sending/getting messages to the CombatPlugin
//
public class CombatClient {
    private CombatClient() {
    }

    // set an object to autoattack a target
    public static void autoAttack(Long oid, Long targetOid, boolean status) {
        AutoAttackMessage msg = new AutoAttackMessage(oid, targetOid, status);
        Engine.getAgent().sendBroadcast(msg);
        if (Log.loggingDebug)
            Log.debug("CombatClient.autoAttack: oid=" + oid + " targetOid=" + targetOid);
    }

    // use an ability on target with item
    public static void startAbility(String abilityName,
                                    Long oid, Long targetOid, Long itemOid) {
        StartAbilityMessage msg = new StartAbilityMessage(oid, abilityName, targetOid, itemOid);
        Engine.getAgent().sendBroadcast(msg);
        if (Log.loggingDebug)
            Log.debug("CombatClient.startAbility: oid=" + oid + " abilityName=" +
                      abilityName + " targetOid=" + targetOid + " itemOid=" + itemOid);
    }

    // resurect an object in place
    public static void releaseObject(Long oid) {
	ReleaseObjectMessage msg = new ReleaseObjectMessage(oid);
	Engine.getAgent().sendBroadcast(msg);
        if (Log.loggingDebug)
            Log.debug("CombatClient.releaseObject: oid=" + oid);
    }

/**
 * messages that have an oid and a target oid
 */
    public static class CombatTargetMessage extends SubjectMessage
    {
        public CombatTargetMessage() {
            super();
        }
        
        public CombatTargetMessage(MessageType type) {
            super(type);
        }
        
        public CombatTargetMessage(MessageType type, Long oid, Long targetOid) {
            super(type, oid);
            setTargetOid(targetOid);
        }

        public Long getTargetOid() {
            return this.targetOid;
        }

        public void setTargetOid(Long oid) {
            this.targetOid = oid;
        }

        private Long targetOid = null;
        
        private static final long serialVersionUID = 1L;
    }

    public static class AutoAttackMessage extends CombatTargetMessage {

        public AutoAttackMessage() {
            super(MSG_TYPE_AUTO_ATTACK);
        }
        
        public AutoAttackMessage(Long oid, Long targetOid, Boolean status) {
            super(MSG_TYPE_AUTO_ATTACK, oid, targetOid);
            setAttackStatus(status);
        }

        public void setAttackStatus(Boolean status) {
            this.status = status;
        }
        
        public Boolean getAttackStatus() {
            return status;
        }

        Boolean status;
        
        private static final long serialVersionUID = 1L;
    }

    public static class AbilityUpdateMessage extends TargetMessage {

	public AbilityUpdateMessage() {
	    super(MSG_TYPE_ABILITY_UPDATE);
	}


	public AbilityUpdateMessage(Long targetOid, Long subjectOid) {
	    super(MSG_TYPE_ABILITY_UPDATE, targetOid, subjectOid);
	}

	public void addAbility(String abilityName, String iconName, String category) {
	    Entry entry = new Entry(abilityName, iconName, category);
	    entries.add(entry);
	}
	
	public List<Entry> getAbilities() {
		return entries;
	}
	
	List<Entry> entries = new LinkedList<Entry>();

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(500);
            buf.putLong(getSubject());
            buf.putInt(56);
	    buf.putInt(entries.size());
	    for (Entry entry : entries) {
		buf.putString(entry.abilityName);
		buf.putString(entry.iconName);
		buf.putString(entry.category);
	    }
            buf.flip();
            return buf;
        }

	class Entry implements Serializable {
	    public Entry(String abilityName, String iconName, String category) {
		this.abilityName = abilityName;
		this.iconName = iconName;
		this.category = category;
	    }

	    public String abilityName;
	    public String iconName;
	    public String category;

            private static final long serialVersionUID = 1L;
	}

        private static final long serialVersionUID = 1L;
    }

    public static class StartAbilityMessage extends CombatTargetMessage {

        public StartAbilityMessage() {
            super(MSG_TYPE_START_ABILITY);
        }

        public StartAbilityMessage(Long oid, String abilityName, Long targetOid, Long itemOid) {
            super(MSG_TYPE_START_ABILITY, oid, targetOid);
            setAbilityName(abilityName);
            setItemOid(itemOid);
        }

        public void setAbilityName(String abilityName) {
            this.abilityName = abilityName;
        }
        
        public String getAbilityName() {
            return abilityName;
        }
        
        public void setItemOid(Long itemOid) {
            this.itemOid = itemOid;
        }
        
        public Long getItemOid() {
            return itemOid;
        }
        
        private Long itemOid;
        private String abilityName;

        private static final long serialVersionUID = 1L;
    }

    public static class DamageMessage extends SubjectMessage {

        public DamageMessage() {
	    super(MSG_TYPE_DAMAGE);
        }
        
	public DamageMessage(Long targetOid, Long attackerOid, Integer dmg, String dmgType) {
	    super(MSG_TYPE_DAMAGE, targetOid);
	    this.attackerOid = attackerOid;
	    setDmg(dmg);
	    setDmgType(dmgType);
	}

	public void setDmg(Integer dmg) {
	    this.dmg = dmg;
	}
	public Integer getDmg() {
	    return dmg;
	}

	public Long getTargetOid() {
	    return getSubject();
	}

	public Long getAttackerOid() {
	    return attackerOid;
	}

	public void setDmgType(String dmgType) {
	    this.dmgType = dmgType;
	}
	public String getDmgType() {
	    return dmgType;
	}
        protected Integer dmg;
	
        protected String dmgType;

	protected Long attackerOid;

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(200);
            buf.putLong(getAttackerOid());
            buf.putInt(23);
	    buf.putLong(getTargetOid());
            buf.putString(dmgType);
            buf.putInt((Integer)dmg);
            buf.flip();
            return buf;
        }

        private static final long serialVersionUID = 1L;
    }

    public static class CooldownMessage extends SubjectMessage {

        public CooldownMessage() {
	    super();
        }
        
        public CooldownMessage(Long oid) {
            super(MSG_TYPE_COOLDOWN, oid);
        }

        public CooldownMessage(Cooldown.State state) {
            super(MSG_TYPE_COOLDOWN, state.getObject().getOid());
            addCooldown(state);
        }

        public void addCooldown(String id, long duration, long endTime) {
            Entry entry = new Entry(id, duration, endTime);
            cooldowns.add(entry);
        }

        public void addCooldown(Cooldown.State state) {
            addCooldown(state.getID(), state.getDuration(), state.getEndTime());
        }

        protected Set<Entry> cooldowns = new HashSet<Entry>();

        public class Entry {
            public Entry() {
            }

            public Entry(String id, long duration, long endTime) {
                setCooldownID(id);
                setDuration(duration);
                setEndTime(endTime);
            }

            public String getCooldownID() { return cooldownID; }
            public void setCooldownID(String cd) { cooldownID = cd; }
            protected String cooldownID;

            public long getDuration() { return duration; }
            public void setDuration(long duration) { this.duration = duration; }
            protected long duration;

            public long getEndTime() { return endTime; }
            public void setEndTime(long endTime) { this.endTime = endTime; }
            protected long endTime;
            private static final long serialVersionUID = 1L;
        }

        private static final long serialVersionUID = 1L;
    }

    public static class AbilityProgressMessage extends SubjectMessage {

        public AbilityProgressMessage() {
            super();
        }
        
        public AbilityProgressMessage(MarsAbility.State state) {
            super(MSG_TYPE_ABILITY_PROGRESS, state.getObject().getOid());
            setAbilityName(state.getAbility().getName());
            setState(state.getState().toString());
            setDuration(state.getDuration());
            setEndTime(calculateEndTime(state));
        }

        protected long calculateEndTime(MarsAbility.State state) {
            MarsAbility ability = state.getAbility();

            switch (state.getState()) {
            case ACTIVATING:
                return state.getNextWakeupTime();
            case CHANNELLING:
                int pulsesRemaining = ability.getChannelPulses() - state.getNextPulse() - 1;
                long endTime = state.getNextWakeupTime() + (pulsesRemaining * ability.getChannelPulseTime());
                return endTime;
            default:
                return 0;
            }
        }

        public String getAbilityName() { return abilityName; }
        public void setAbilityName(String name) { abilityName = name; }
        protected String abilityName;

        public String getState() { return state; }
        public void setState(String state) { this.state = state; }
        protected String state;

        public long getDuration() { return duration; }
        public void setDuration(long duration) { this.duration = duration; }
        protected long duration;

        public long getEndTime() { return endTime; }
        public void setEndTime(long time) { endTime = time; }
        protected long endTime;
        private static final long serialVersionUID = 1L;
    }

    public static class ReleaseObjectMessage extends SubjectMessage {

        public ReleaseObjectMessage() {
	    super();
        }
        
	public ReleaseObjectMessage(Long oid) {
	    super(MSG_TYPE_RELEASE_OBJECT, oid);
	}
        private static final long serialVersionUID = 1L;
    }

    /**
     * sub object creation namespace for the animation plugin
     */
    public static Namespace NAMESPACE = null;

    public static Namespace TEST_NAMESPACE = null;
    
    public static final MessageType MSG_TYPE_AUTO_ATTACK = MessageType.intern("mv.AUTO_ATTACK");
    public static final MessageType MSG_TYPE_START_ABILITY = MessageType.intern("mv.START_ABILITY");
    public static final MessageType MSG_TYPE_COOLDOWN = MessageType.intern("mv.COOLDOWN");
    public static final MessageType MSG_TYPE_ABILITY_PROGRESS = MessageType.intern("mv.ABILITY_PROGRESS");
    public static final MessageType MSG_TYPE_DAMAGE = MessageType.intern("mv.DAMAGE");
    public static final MessageType MSG_TYPE_RELEASE_OBJECT = MessageType.intern("mv.RELEASE_OBJECT");
    public static final MessageType MSG_TYPE_ABILITY_UPDATE = MessageType.intern("mv.ABILITY_UPDATE");
    public static final MessageType MSG_TYPE_SKILL_UPDATE = MessageType.intern("mv.SKILL_UPDATE");
    public static final MessageType MSG_TYPE_ADD_SKILL = MessageType.intern("mv.ADD_SKILL");
    public static final MessageType MSG_TYPE_TRAINING_FAILED = MessageType.intern("mv.TRAINING_FAILED");
    public static final MessageType MSG_TYPE_COMBAT_ABILITY_MISSED = MessageType.intern("mv.COMBAT_ABILITY_MISSED");
    public static final String MSG_ATTACK_STATUS = "combat_attackStatus";
}
