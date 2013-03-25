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

import java.io.*;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.marshalling.MarshallingRuntime;


/** Multiverse message base-class.  
*/
public class Message implements Serializable
{
    public Message() {
    }

    /** Create message of the given message type.
    */
    public Message(MessageType msgType) {
        this.msgType = msgType;
    }

    /** Get the message type.
    */
    public MessageType getMsgType() {
        return msgType;
    }

    /** Set the message type.
    */
    public void setMsgType(MessageType msgType) {
        this.msgType = msgType;
    }

    /** Get the message id.  The message id is unique within the sending
        agent.  The message ids will repeat if the agent restarts.
    */
    public long getMsgId() {
        return msgId;
    }
    void setMessageId(long msgId) {
        this.msgId = msgId;
    }

    /** Get the sending agent name.
        @return Agent name or null if the message has not been sent.
    */
    public String getSenderName() {
        if (remoteAgent != null)
            return remoteAgent.agentName;
        else
            return null;
    }

    /** Get the message enqueue time.
    */
    public long getEnqueueTime() {
        return enqueueTime;
    }

    /** Set the message enqueue time.  Application callbacks can set
        the enqueue time prior to insertion to an application queue
        or thread pool.
    */
    public void setEnqueueTime(long when) {
        enqueueTime = when;
    }

    /** Set the message enqueue time to the current nano-second time.  Application callbacks can set
        the enqueue time prior to insertion to an application queue
        or thread pool.
    */
    public void setEnqueueTime() {
        enqueueTime = System.nanoTime();
    }

    MessageAgent.RemoteAgent getRemoteAgent() {
        return remoteAgent;
    }

    /** Internal use only. */
    public static void toBytes(Message message, MVByteBuffer buffer)
    {
        int lengthPos = buffer.position();
        buffer.putInt(0);               // length
        MarshallingRuntime.marshalObject(buffer, message);

        // patch the message length
        int currentPos = buffer.position();
        buffer.position(lengthPos);
        buffer.putInt(currentPos - lengthPos - 4);
        buffer.position(currentPos);
    }

    void setRPC() {
        flags |= RPC;
    }

    void unsetRPC() {
        flags &= ~RPC;
    }

    /** True if the message is an RPC request. */
    public boolean isRPC() {
        return (flags & RPC) != 0;
    }

    static final short RPC = 1;

    long msgId;
    MessageType msgType;
    short flags;
    transient MessageAgent.RemoteAgent remoteAgent;
    transient long enqueueTime;
    
    private static final long serialVersionUID = 1L;
}

