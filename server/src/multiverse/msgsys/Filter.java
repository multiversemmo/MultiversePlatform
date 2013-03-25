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

/** Subscription message filter base class.  Filters have a list of matching
{@link MessageType MessageTypes} and optionally provide their own
{@link FilterTable}.
<p>
Sub-classes must implement {@link #matchMessageType matchMessageType},
{@link #getMessageTypes}, and {@link #matchRemaining matchRemaining}.
Sub-classes must also provide a no argument constructor to be
compatible with multiverse marshalling.
*/
public abstract class Filter
    implements IFilter
{
    /** True if the given {@code messageTypes} intersects the filter's
        message types.
    */
    public abstract boolean matchMessageType(Collection<MessageType> messageTypes);

    /** True if the {@code message} matches the filter criteria.  The
        message type will already have been matched, so there's no need
        to check it again.
    */
    public abstract boolean matchRemaining(Message message);

    /** Returns the filter's message types.
    */
    public abstract Collection<MessageType> getMessageTypes();

    /** Update the filter according to the instructions in {@code udpate}.
    */
    public boolean applyFilterUpdate(FilterUpdate update, AgentHandle sender,
            SubscriptionHandle sub) {
        return applyFilterUpdate(update);
    }

    /** Update the filter according to the instructions in {@code udpate}.
    */
    public boolean applyFilterUpdate(FilterUpdate update) {
        return false;
    }

    /** Returns filter table used to collect this kind of filter.
        @return null to force use of {@link DefaultFilterTable}.
    */
    public FilterTable getSendFilterTable()
    {
        return null;
    }

    /** Returns filter table used to collect this kind of filter.
        @return null to force use of {@link DefaultFilterTable}.
    */
    public FilterTable getReceiveFilterTable()
    {
        return null;
    }

    /** Returns filter table used to collect this kind of filter.
        @return null to force use of {@link DefaultFilterTable}.
    */
    public FilterTable getResponderSendFilterTable()
    {
        return null;
    }

    /** Returns filter table used to collect this kind of filter.
        @return null to force use of {@link DefaultFilterTable}.
    */
    public FilterTable getResponderReceiveFilterTable()
    {
        return null;
    }

    protected String toStringInternal() {
        return "";
    }
}

