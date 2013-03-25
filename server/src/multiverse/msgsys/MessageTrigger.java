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


/** Run code on publisher when message matches our subscription.
Sub-class to implement your own behavior.
<p>
Message triggers are associated with a specific subscription.  The
trigger is instantiated (via marshalling) on each publisher.  When
the publisher sends a message matching the subscription, the
subscription's trigger is run.  First, {@link #match} is called
to determine if the trigger should be run.  If match() returns
true, then {@link #trigger trigger()} is run with the message
and matching filter.
<p>
Sub-classes should use Java keyword "transient" for data members
that should not be copied from subscriber to publisher.
*/
public abstract class MessageTrigger
{
    /** No-arg constructor required for marshalling. */
    public MessageTrigger()
    {
    }

    /** Set the trigger's filter.  Required on sub-classes. */
    public abstract void setFilter(IFilter filter);

    /** True if the trigger should be run for this message.
        MessageTrigger.match() always returns true.  Over-ride to
        select which messages to pass to {@link #trigger}.
    */
    public boolean match(Message message)
    {
        return true;
    }

    /** Called when message matches the filter.  Required on sub-classes.
        It is OK to modify {@code message}, but the trigger must not
        "re-send" the message; do not use {@code message} in calls
        to {@link MessageAgent#sendBroadcast MessageAgent.sendBroadcast()}
        and similar calls.  It is OK to modify {@code filter}.  The
        message is not re-matched if the message or filter are modified.
        <p>
        Use great care when designing MessageTriggers.
        @param message The matched message.
        @param filter The matched filter, the same as passed to
        {@link #setFilter setFilter()}.
        @param agent The local message agent.
    */
    public abstract void trigger(Message message, IFilter filter,
        MessageAgent agent);

}

