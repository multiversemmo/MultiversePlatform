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

package multiverse.server.messages;

import java.util.*;
import multiverse.msgsys.*;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.util.Log;
import multiverse.server.marshalling.Marshallable;
import multiverse.server.messages.PerceptionMessage.ObjectNote;
import multiverse.server.objects.ObjectType;
import multiverse.server.objects.ObjectTypes;



/** Match messages by message type and a list of target and subject OIDs.
One of the message types should be used by class {@link PerceptionMessage}.
This filter should be used in conjunction with a {@link PerceptionTrigger}.
<p>
Conceptually, the PerceptionFilter is a set of subject OIDs that match
{@link SubjectMessage SubjectMessages} and a set of target OIDs that
match {@link TargetMessage TargetMessages}.
<p>
The subscriber maintains a set of target OIDs in the filter.  Filter updates
are used to keep the producing agent filters in-sync.  The producers
maintain a set of subject OIDs in the filter by inspecting all
{@link PerceptionMessage PerceptionMessages} matched by the filter.
The filter's subject OIDs are a union of all the OIDs perceivable by
the filter's target OIDs.
The filter's subject OIDs are updated via a {@link PerceptionTrigger}
on the subscription.  The producers only update their own copy of the
PerceptionFilter.  The subscriber should also inspect
PerceptionMessages and use filter updates to keep the other
subscribers in-sync (those that don't publish PerceptionMessages).
<p>
PerceptionFilter supports {@link FilterUpdate} with fields {@link
#FIELD_TARGETS} and {@link #FIELD_SUBJECTS}.  Operations {@link
FilterUpdate#OP_ADD} and {@link FilterUpdate#OP_REMOVE} are supported.
Use {@link MessageAgent#applyFilterUpdate MessageAgent.applyFilterUpdate()}
to apply a filter update.

*/
public class PerceptionFilter extends Filter
    implements Marshallable, IMessageTypeFilter
{
    public PerceptionFilter()
    {
    }

    /** Match messages by message type.
    */
    public PerceptionFilter(Collection<MessageType> types)
    {
        messageTypes.addAll(types);
    }

    /** Set the message types to match.
    */
    public void setTypes(Collection<MessageType> types)
    {
        messageTypes = new HashSet<MessageType>();
        messageTypes.addAll(types);
    }

    /** Add to the matching message types.
    */
    public void addType(MessageType type)
    {
        messageTypes.add(type);
    }

    /** Get the matching message types.
    */
    public Collection<MessageType> getMessageTypes()
    {
        return messageTypes;
    }

    /** True if the local filter matches all subjects.
    */
    public boolean getMatchAllSubjects()
    {
        return matchAllSubjects;
    }

    /** Match all subject OIDs in the local filter.  This setting
        only affects the local message agent and is not replicated to the
        producers.
    */
    public void setMatchAllSubjects(boolean match)
    {
        matchAllSubjects = match;
    }

    /** True if the remote filter matches subjects added by the subscriber.
    */
    public boolean getMatchSubjects()
    {
        return matchSubjects;
    }

    /** Match subjects added by the subscriber.
    */
    public void setMatchSubjects(boolean match)
    {
        matchSubjects = match;
    }

    /** Add a subject to the filter's subject OIDs.  Subjects are
        reference counted.
    */
    public synchronized boolean addSubject(long oid)
    {
        SubjectInfo holder = subjects.get(oid);
        if (holder == null) {
            subjects.put(oid, new SubjectInfo(1,ObjectTypes.unknown));
            return true;
        }
        holder.count++;
        return false;
    }

    /** Add a subject to the filter's subject OIDs if not already present.
    */
    public synchronized boolean addSubjectIfMissing(long oid)
    {
        SubjectInfo holder = subjects.get(oid);
        if (holder == null) {
            subjects.put(oid, new SubjectInfo(1,ObjectTypes.unknown));
            return true;
        }
        else
            return false;
    }

    /** True if subject is in the filter.
    */
    public synchronized boolean hasSubject(long oid)
    {
        return subjects.get(oid) != null;
    }

    // Called by trigger, so take some short cuts.  We don't check
    // if the oids are already in the 'oids' set.
    synchronized void addSubjects(Collection<TypedSubject> newSubjects)
    {
        for (TypedSubject subject : newSubjects) {
            if (subjects.get(subject.oid) != null)
                Log.error("PerceptionFilter: already have subject "+subject.oid);
            subjects.put(subject.oid, new SubjectInfo(1,subject.type));
        }
    }

    /** Remove a subject from the filter's subject OIDs.  Subjects are
        reference counted.
    */
    public synchronized boolean removeSubject(long oid)
    {
        IntHolder holder = subjects.get(oid);
        if (holder == null) {
            Log.error("PerceptionFilter.removeSubject: oid " + oid + " not found");
            return false;
        }
        holder.count--;
        if (holder.count == 0) {
            subjects.remove(oid);
            return true;
        }
        else
            return false;
    }

    // Called by trigger, so take some short cuts.  We don't check
    // if the subject ref count is zero.
    synchronized void removeSubjects(Collection<Long> freeOids)
    {
        for (Long oid : freeOids) {
            if (subjects.get(oid) == null)
                Log.error("PerceptionFilter.removeSubjects: duplicate remove "+oid);
            subjects.remove(oid);
        }
    }

    //## caller should lock around addNotifyOid/removeNotifyOid and
    //## sending the FilterUpdate to guarantee order
    /** Add a target to the filter's target OIDs.  Targets are
        reference counted.  Caller should send a {@link FilterUpdate}
        if addTarget() returns true.
        @return True if {@code oid} is a new target.
    */
    public synchronized boolean addTarget(long oid)
    {
        IntHolder holder = targets.get(oid);
        if (holder == null) {
            targets.put(oid, new IntHolder(1));
            return true;
        }
        holder.count++;
        return false;
    }

    /** True if subject is in the filter.
    */
    public synchronized boolean hasTarget(long oid)
    {
        return targets.get(oid) != null;
    }

    /** Set subject object types for which you want SubjectMessages.
        For SubjectMessages, filter will match when the subject's
        ObjectType is in {@code subjectTypes}.  This option only
        works when PerceptionFilter is combined with {@link PerceptionTrigger}.
    */
    public synchronized void setSubjectObjectTypes(
        Collection<ObjectType> subjectTypes)
    {
        if (subjectTypes != null)
            subjectTypeFilter = new ArrayList<ObjectType>(subjectTypes);
        else
            subjectTypeFilter = null;
    }

    /** Get subject object type filter.
        @see #setSubjectObjectTypes
    */
    public synchronized List<ObjectType> getSubjectObjectTypes()
    {
        return new ArrayList<ObjectType>(subjectTypeFilter);
    }

    /** Remove a target from the filter's target OIDs.  Targets are
        reference counted.  Caller should send a {@link FilterUpdate}
        if removeTarget() returns true.
        @return True if {@code oid} reference count drops to zero.
    */
    public synchronized boolean removeTarget(long oid)
    {
        IntHolder holder = targets.get(oid);
        if (holder == null) {
            Log.error("PerceptionFilter.removeTarget: oid " + oid + " not found");
            return false;
        }
        holder.count--;
        if (holder.count == 0) {
            targets.remove(oid);
            return true;
        }
        return false;
    }

    /** True if the given {@code types} intersects the filter's
        message types.
    */
    public boolean matchMessageType(Collection<MessageType> types)
    {
        for (MessageType tt : types)  {
            if (messageTypes.contains(tt))
                return true;
        }
//        if (messageTypes.contains(MessageTypes.MSG_TYPE_ALL_TYPES))
//            return true;
        return false;
    }

    /** True if the message matches filter criteria.
        <p>
        Matches TargetMessage target OID in the filter's target set.
        <p>
        Matches SubjectMessage subject OID in the filter's target or
        subject set.
        <p>
        Matches PerceptionMessage target OIDs in the filter's target set.
    */
    public synchronized boolean matchRemaining(Message message)
    {
        if (message instanceof PerceptionMessage) {
            PerceptionMessage msg = (PerceptionMessage) message;
            if (targets.get(msg.getTarget()) != null)
                return true;
            List<ObjectNote> gainObjects = msg.getGainObjects();
            if (gainObjects != null) {
                for (ObjectNote gain : gainObjects) {
                    if (targets.get(gain.targetOid) != null)
                        return true;
                }
            }
            List<ObjectNote> lostObjects = msg.getLostObjects();
            if (lostObjects != null) {
                for (ObjectNote lost : lostObjects) {
                    if (targets.get(lost.targetOid) != null)
                        return true;
                }
            }
            return false;
        }

        if (message instanceof TargetMessage) {
            TargetMessage msg = (TargetMessage) message;
            if (targets.get(msg.getTarget()) != null)
                return true;
        }

        // Match messages about our subjects or about our targets
        if (message instanceof SubjectMessage) {
            if (matchAllSubjects)
                return true;
            SubjectMessage msg = (SubjectMessage) message;
            SubjectInfo subjectInfo = subjects.get(msg.getSubject());
            if (subjectInfo != null) {
                if (subjectTypeFilter != null &&
                        ! subjectTypeFilter.contains(subjectInfo.type))
                    return false;
                else
                    return true;
            }
            if (targets.get(msg.getSubject()) != null)
                return true;
        }

        return false;
    }

    /** Targets field id for {@link FilterUpdate} */
    public static final int FIELD_TARGETS = 1;
    /** Subjects field id for {@link FilterUpdate} */
    public static final int FIELD_SUBJECTS = 2;

    /** (Internal use only) Called by the {@link MessageAgent} to apply
        a {@link FilterUpdate}.
        @see MessageAgent#applyFilterUpdate
    */
    public boolean applyFilterUpdate(FilterUpdate update,
        AgentHandle sender, SubscriptionHandle sub)
    {
        List<FilterUpdate.Instruction> instructions = update.getInstructions();

        if (updateTriggers.size() > 0 && sender != null) {
            for (FilterUpdate.Instruction instruction : instructions) {
                for (PerceptionUpdateTrigger updateTrigger : updateTriggers) {
                    updateTrigger.preUpdate(this,instruction,sender,sub);
                }
            }
        }

      synchronized (this) {
        for (FilterUpdate.Instruction instruction : instructions) {
            switch (instruction.opCode) {
            case FilterUpdate.OP_ADD:
                if (instruction.fieldId == FIELD_TARGETS) {
                    if (Log.loggingDebug)
                        Log.debug("ADD TARGET "+instruction.value);
                    targets.put((Long)instruction.value, new IntHolder(1));
                }
                else if (instruction.fieldId == FIELD_SUBJECTS) {
                    if (Log.loggingDebug)
                        Log.debug("ADD SUBJECT "+instruction.value);
                    subjects.put((Long)instruction.value, new SubjectInfo(1,
                        ObjectTypes.unknown));
                }
                else
                    Log.error("PerceptionFilter.applyFilterUpdate: invalid fieldId " +
                        instruction.fieldId);
                break;
            case FilterUpdate.OP_REMOVE:
                if (instruction.fieldId == FIELD_TARGETS) {
                    if (Log.loggingDebug)
                        Log.debug("REMOVE TARGET "+instruction.value);
                    targets.remove((Long)instruction.value);
                }
                else if (instruction.fieldId == FIELD_SUBJECTS) {
                    if (Log.loggingDebug)
                        Log.debug("REMOVE SUBJECT "+instruction.value);
                    subjects.remove((Long)instruction.value);
                }
                else
                    Log.error("PerceptionFilter.applyFilterUpdate: invalid fieldId " +
                        instruction.fieldId);
                break;
            case FilterUpdate.OP_SET:
                Log.error("PerceptionFilter.applyFilterUpdate: OP_SET is not supported");
                break;
            default:
                Log.error("PerceptionFilter.applyFilterUpdate: invalid opCode " +
                        instruction.opCode);
                break;
            }
        }
      }

        if (updateTriggers.size() > 0 && sender != null) {
            for (FilterUpdate.Instruction instruction : instructions) {
                for (PerceptionUpdateTrigger updateTrigger : updateTriggers) {
                    updateTrigger.postUpdate(this,instruction,sender,sub);
                }
            }
        }

        return false;
    }

    public static void addUpdateTrigger(PerceptionUpdateTrigger updateTrigger)
    {
        synchronized (updateTriggers) {
            updateTriggers.add(updateTrigger);
        }
    }

    /** Custom marshaller
    */
    public void marshalObject(MVByteBuffer buf)
    {
        buf.putInt(messageTypes.size());
        for (MessageType type : messageTypes)
            type.marshalObject(buf);

        buf.putBoolean(matchSubjects);

        buf.putInt(targets.size());
        for (Long oid : targets.keySet())
            buf.putLong(oid);

        if (matchSubjects) {
            buf.putInt(subjects.size());
            for (Long oid : subjects.keySet())
                buf.putLong(oid);
        }
    }

    /** Custom marshaller
    */
    public Object unmarshalObject(MVByteBuffer buf)
    {
        int size = buf.getInt();
        for ( ; size > 0; size--) {
            MessageType type = new MessageType();
            type = (MessageType) type.unmarshalObject(buf);
            messageTypes.add(type);
        }

        matchSubjects = buf.getBoolean();

        size = buf.getInt();
        for ( ; size > 0; size--)
            targets.put(buf.getLong(), new IntHolder(1));

        if (matchSubjects) {
            size = buf.getInt();
            for ( ; size > 0; size--)
                subjects.put(buf.getLong(),
                    new SubjectInfo(1,ObjectTypes.unknown));
        }

        return this;
    }

    public String toString() {
        return "[PerceptionFilter "+toStringInternal()+"]";
    }

    protected String toStringInternal() {
        String result="types=";
        for (MessageType type : messageTypes)
            result += type.getMsgTypeString()+",";
        result +=" subjectCount=" + subjects.size();
        result +=" targets=";
        for (Long oid : targets.keySet())
            result += oid+",";
        return result;
    }

    protected class IntHolder {
        IntHolder() { }
        IntHolder(int initial) {
            count = initial;
        }
        int count = 0;
    }

    protected class SubjectInfo extends IntHolder {
        SubjectInfo() { }
        SubjectInfo(int initial, ObjectType objectType) {
            super(initial);
            type = objectType;
        }
        ObjectType type;
    }

    public static class TypedSubject {
        public TypedSubject(Long subjectOid, ObjectType objectType)
        {
            oid = subjectOid;
            type = objectType;
        }
        Long oid;
        ObjectType type;
    }

    private Set<MessageType> messageTypes = new HashSet<MessageType>();
    private Map<Long,IntHolder> targets = new HashMap<Long,IntHolder>();

    private transient Map<Long,SubjectInfo> subjects = new HashMap<Long,SubjectInfo>();
    private transient boolean matchAllSubjects = false;
    private boolean matchSubjects = false;
    private List<ObjectType> subjectTypeFilter;

    private static List<PerceptionUpdateTrigger> updateTriggers =
        new LinkedList<PerceptionUpdateTrigger>();

}
