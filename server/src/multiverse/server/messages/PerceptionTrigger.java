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
import multiverse.server.messages.PerceptionMessage.ObjectNote;
import multiverse.server.util.Log;


/** Track union of perceived objects.  This trigger only works with
{@link PerceptionFilter} and {@link PerceptionMessage}.  By default,
{@link #match match()} returns true for any instance of PerceptionMessage.
<p>
The trigger tracks the union of objects perceived by the filter's
target OIDs.  The filter subject OIDs are kept in-sync with this union.
*/
public class PerceptionTrigger extends MessageTrigger
{
    /** No-arg constructor required for marshalling. */
    public PerceptionTrigger()
    {
    }

    /** Set the trigger's filter. */
    public void setFilter(IFilter filter)
    {
        this.filter = (PerceptionFilter) filter;
    }

    /** Set the message types that run the trigger. */
    public void setTriggeringTypes(Collection<MessageType> types)
    {
        msgTypes = new ArrayList<MessageType>(types.size());
        msgTypes.addAll(types);
    }

    /** True if the trigger should run for the message. Compares the
        {@code message} type to the triggering types.  If no
        triggering types are set (the default) then returns true
        if {@code message} is a PerceptionMessage.
    */
    public boolean match(Message message)
    {
        if (msgTypes == null) {
            return (message instanceof PerceptionMessage);
        }

        return msgTypes.contains(message.getMsgType());
    }

    /** Track union of perceived objects and keep filter in-sync.
        The filter must be a {@link PerceptionFilter} and the
        message must be a {@link PerceptionMessage}.
        @param triggeringMessage The matched message.
        @param triggeringFilter The matched filter.
        @param agent The local message agent.
    */
    public synchronized void trigger(Message triggeringMessage,
        IFilter triggeringFilter, MessageAgent agent)
    {
        PerceptionMessage message = (PerceptionMessage) triggeringMessage;

        List<ObjectNote> gainObjects = message.getGainObjects();
        List<ObjectNote> lostObjects = message.getLostObjects();

        if (gainObjects != null) {
            List<PerceptionFilter.TypedSubject> newSubjects =
                new ArrayList<PerceptionFilter.TypedSubject>(gainObjects.size());

            for (ObjectNote gain : gainObjects) {
                IntHolder refCount = objectRefs.get(gain.subjectOid);
                if (refCount == null) {
                    objectRefs.put(gain.subjectOid, new IntHolder(1));
                    newSubjects.add(
                        new PerceptionFilter.TypedSubject(gain.subjectOid,
                            gain.objectType));
                }
                else
                    refCount.count ++;
            }

            if (newSubjects.size() > 0) {
                if (Log.loggingDebug)
                    Log.debug("PerceptionTrigger adding "+newSubjects.size());
                filter.addSubjects(newSubjects);
            }
        }

        if (lostObjects == null)
            return;

        List<Long> freeOids = new ArrayList<Long>(lostObjects.size());

        for (ObjectNote lost : lostObjects) {
            IntHolder refCount = objectRefs.get(lost.subjectOid);
            if (refCount == null)
                Log.error("PerceptionTrigger: duplicate lost "+lost.subjectOid);
            else if (refCount.count == 1) {
                objectRefs.remove(lost.subjectOid);
                freeOids.add(lost.subjectOid);
            }
            else
                refCount.count --;
        }

        if (freeOids.size() > 0) {
            if (Log.loggingDebug)
                Log.debug("PerceptionTrigger removing "+freeOids.size());
            filter.removeSubjects(freeOids);
        }
    }

    protected class IntHolder {
        IntHolder() { }
        IntHolder(int initial) {
            count = initial;
        }
        int count = 0;
    }

    private List<MessageType> msgTypes;
    private transient Map<Long,IntHolder> objectRefs =
        new HashMap<Long,IntHolder>();
    private transient PerceptionFilter filter;
}

