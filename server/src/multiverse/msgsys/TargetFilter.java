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

package multiverse.msgsys;

import java.util.*;


/** Match target messages by message type, target and subject OIDs.
*/
public class TargetFilter extends MessageTypeFilter
{
    public TargetFilter()
    {
    }

    /** Match target messages by message type, target and subject OIDs.
    */
    public TargetFilter(Collection<MessageType> types,
        long targetOid, long subjectOid)
    {
        super(types);
        this.targetOid = targetOid;
        this.subjectOid = subjectOid;
    }

    /** True if {@code message} is a {@link TargetMessage} with target OID
        matching the filter's target or subject OID.  True if
        {@code message} is a {@link SubjectMessage} with subject OID
        matching the filter's subject OID.
    */
    public boolean matchRemaining(Message message)
    {
        if (message instanceof TargetMessage)
            return ((TargetMessage)message).getTarget() == targetOid ||
                ((TargetMessage)message).getTarget() == subjectOid;
        if (message instanceof SubjectMessage)
            return ((SubjectMessage)message).getSubject() == subjectOid;
        
        return false;
    }

    public String toString()
    {
        return "[TargetFilter "+toStringInternal()+"]";
    }

    protected String toStringInternal()
    {
        return "target="+targetOid + " subject="+subjectOid + " " +
            super.toStringInternal();
    }

    private long targetOid;
    private long subjectOid;
}

