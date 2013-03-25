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
import multiverse.server.objects.ObjectType;

/** Changes to the objects perceived by a target object.  The
PerceptionMessage contains a list of gained objects and a list
of lost objects.  Gained objects are newly perceived and lost
objects are no longer perceivable.
<p>
Gained and lost objects are each described by an {@link ObjectNote}.
The ObjectNote contains the object OID (subject) and object type.
The ObjectNote may optionally contain some
arbitrary object information (see {@link ObjectNote#getObjectInfo()}).
<p>
ObjectNote supports a different target for each perceived object,
but this capability is currently not used and may become deprecated.
*/
public class PerceptionMessage extends Message implements HasTarget
{
    /** No-arg constructor required for marshalling. */
    public PerceptionMessage() {
    }

    /** Create message of the given message type.
    */
    public PerceptionMessage(MessageType msgType) {
        super(msgType);
    }

    /** Create message of the given message type and target.
    */
    public PerceptionMessage(MessageType msgType, long target) {
        super(msgType);
        this.target = target;
    }

    /** Get the message target.
        @return OID
    */
    public long getTarget()
    {
        return target;
    }

    /** Set the message target OID.
    */
    public void setTarget(long target)
    {
        this.target = target;
    }

    /** Add a gained object. */
    public ObjectNote gainObject(long targetOid, long subjectOid,
        ObjectType objectType)
    {
        if (gainObjects == null)
            gainObjects = new LinkedList<ObjectNote>();
        ObjectNote note = new ObjectNote(targetOid, subjectOid, objectType);
        gainObjects.add(note);
        return note;
    }

    /** Add a gained object. */
    public void gainObject(ObjectNote note)
    {
        if (gainObjects == null)
            gainObjects = new LinkedList<ObjectNote>();
        gainObjects.add(note);
    }

    /** Add a lost object. */
    public void lostObject(long targetOid, long subjectOid)
    {
        if (lostObjects == null)
            lostObjects = new LinkedList<ObjectNote>();
        lostObjects.add(new ObjectNote(targetOid, subjectOid));
    }

    /** Add a lost object. */
    public void lostObject(long targetOid, long subjectOid, ObjectType objectType)
    {
        if (lostObjects == null)
            lostObjects = new LinkedList<ObjectNote>();
        lostObjects.add(new ObjectNote(targetOid, subjectOid, objectType));
    }

    /** Add a lost object. */
    public void lostObject(ObjectNote note)
    {
        if (lostObjects == null)
            lostObjects = new LinkedList<ObjectNote>();
        lostObjects.add(note);
    }

    /** Get gained object list.
        @return null if there are no gained objects.
    */
    public List<ObjectNote> getGainObjects()
    {
        return gainObjects;
    }

    /** Get lost object list.
        @return null if there are no lost objects.
    */
    public List<ObjectNote> getLostObjects()
    {
        return lostObjects;
    }

    /** Get number of gained objects. */
    public int getGainObjectCount()
    {
        return (gainObjects==null) ? 0 : gainObjects.size();
    }

    /** Get number of lost objects. */
    public int getLostObjectCount()
    {
        return (lostObjects==null) ? 0 : lostObjects.size();
    }

    /** Described a gained or lost perceivable object. */
    public static class ObjectNote {

        /** No-arg constructor required for marshalling. */
        public ObjectNote() {
        }

        /** Make an ObjectNote. */
        public ObjectNote(long targetOid, long subjectOid)
        {
            this.targetOid = targetOid;
            this.subjectOid = subjectOid;
        }

        /** Make an ObjectNote. */
        public ObjectNote(long targetOid, long subjectOid, ObjectType objectType)
        {
            this.targetOid = targetOid;
            this.subjectOid = subjectOid;
            this.objectType = objectType;
        }

        /** Make an ObjectNote. */
        public ObjectNote(long targetOid, long subjectOid, ObjectType objectType,
                Object info)
        {
            this.targetOid = targetOid;
            this.subjectOid = subjectOid;
            this.objectType = objectType;
            this.info = info;
        }

        public String toString() {
            return "targ="+targetOid+" subj="+subjectOid+" t="+objectType;
        }

        /** Get the perceiver object OID. */
        public long getTarget() {
            return targetOid;
        }

        /** Get the perceived object OID. */
        public long getSubject() {
            return subjectOid;
        }

        /** Get the subject object type. */
        public ObjectType getObjectType() {
            return objectType;
        }

        /** Get the subject object information. */
        public Object getObjectInfo() {
            return info;
        }

        /** Set the subject object information. */
        public void setObjectInfo(Object info) {
            this.info = info;
        }

        long targetOid;
        long subjectOid;
        ObjectType objectType;
        Object info;
    }

    long target;
    List<ObjectNote> gainObjects;
    List<ObjectNote> lostObjects;

    private static final long serialVersionUID = 1L;
}
