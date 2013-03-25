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


/** A message to an object (the target) about an object (the subject).
Target and subject are identified by OID.
*/
public class TargetMessage extends Message
{
    public TargetMessage() {
    }

    /** Create message of the given message type.
    */
    public TargetMessage(MessageType msgType) {
        super(msgType);
    }

    /** Create message of the given message type, target, and subject.
    */
    public TargetMessage(MessageType msgType, long target, long subject) {
        super(msgType);
        this.target = target;
        this.subject = subject;
    }
    
    /** Create message of the given message type, target, but no subject.
     */
     public TargetMessage(MessageType msgType, long target) {
         super(msgType);
         this.target = target;
     }

    public String toString()
    {
        return "["+this.getClass().getName()+" target="+target+" subject="+subject+"]";
    }

    /** Get the message Target.
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

    /** Get the message subject.
        @return OID
    */
    public long getSubject()
    {
        return subject;
    }

    /** Set the message subject OID.
    */
    public void setSubject(long subject)
    {
        this.subject = subject;
    }

    protected long target;
    protected long subject;
    
    private static final long serialVersionUID = 1L;
}
