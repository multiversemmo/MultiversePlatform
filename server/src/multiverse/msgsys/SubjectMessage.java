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


/** A message about an object.  The subject object is identified by OID.
*/
public class SubjectMessage extends Message
{
    public SubjectMessage() {
    }

    /** Create message of the given message type.
    */
    public SubjectMessage(MessageType msgType) {
        super(msgType);
    }

    /** Create message of the given message type and subject.
    */
    public SubjectMessage(MessageType msgType, long oid) {
        super(msgType);
        this.oid = oid;
    }

    public String toString()
    {
        return "["+this.getClass().getName()+" subject="+oid+"]";
    }

    /** Get the message subject.
        @return OID
    */
    public long getSubject()
    {
        return oid;
    }

    /** Set the message subject OID.
    */
    public void setSubject(long oid)
    {
        this.oid = oid;
    }
    
    protected long oid;

    private static final long serialVersionUID = 1L;
}
