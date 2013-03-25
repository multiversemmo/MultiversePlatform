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


/** Response to an RPC request message.  Applications should use
constructor {@link #ResponseMessage(Message)} or
{@link #ResponseMessage(MessageType,Message)}.
*/
public class ResponseMessage extends Message
{
    public ResponseMessage() {
        msgType = MessageTypes.MSG_TYPE_RESPONSE;
    }

    /** Create message of the given message type.
    */
    public ResponseMessage(MessageType type) {
        msgType = type;
    }

    /** Create response message for the given requestMessage.
    */
    public ResponseMessage(Message requestMessage)
    {
        msgType = MessageTypes.MSG_TYPE_RESPONSE;
        requestId = requestMessage.getMsgId();
        requestingAgent = requestMessage.getRemoteAgent();
    }
    
    /** Create response message for the given requestMessage.
    */
    public ResponseMessage(MessageType msgType, Message requestMessage) {
        this.msgType = msgType;
        requestId = requestMessage.getMsgId();
        requestingAgent = requestMessage.getRemoteAgent();
    }

    /** Get the request message id. */
    public long getRequestId()
    {
        return requestId;
    }

    MessageAgent.RemoteAgent getRequestingAgent()
    {
        return requestingAgent;
    }

    long requestId;
    transient MessageAgent.RemoteAgent requestingAgent;
    
    private static final long serialVersionUID = 1L;
}
