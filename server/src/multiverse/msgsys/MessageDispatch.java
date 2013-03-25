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


/** Mix-in interface for application message dispatchers (i.e. thread pools).
If a message callback object implements MessageDispatch in addition to
{@link MessageCallback} then messages will be sent to
MessageDispatch.dispatchMessage() instead of MessageCallback.handleMessage().
<p>
Typically, dispatchMessage() inserts the message into a queue and returns.
Then, a thread pool removes from the queue and calls
MessageCallback.handleMessage().  The queue should pass the {@code flags}
through unaltered.
*/
public interface MessageDispatch
{
    /** Dispatch message to queue or thread pool.  Implementations should
        pass the {@code flags} through unaltered.
        @param message Message sent from other agent.
        @param flags Bitwise OR of {@link MessageCallback#NO_FLAGS},
        {@link MessageCallback#RESPONSE_EXPECTED}
        @param callback Callback to handle the message (currently,
                always the same as 'this').
    */
    public void dispatchMessage(Message message, int flags,
        MessageCallback callback);
}

