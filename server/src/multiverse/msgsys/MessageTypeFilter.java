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

/** Message type subscription filter.  Match messages by message type.
*/
public class MessageTypeFilter extends Filter
    implements IMessageTypeFilter
{
    public MessageTypeFilter()
    {
    }

    /** Match a single message type
    */
    public MessageTypeFilter(MessageType type)
    {
        addType(type);
    }

    /** Match multiple message types.
    */
    public MessageTypeFilter(Collection<MessageType> types)
    {
        messageTypes = new HashSet<MessageType>(types.size());
        messageTypes.addAll(types);
    }

    /** Add to the matched message types.
    */
    public void addType(MessageType type)
    {
        if (messageTypes == null)
            messageTypes = new HashSet<MessageType>();
        messageTypes.add(type);
    }

    /** Set the matched message types.
    */
    public void setTypes(Collection<MessageType> types)
    {
        messageTypes = new HashSet<MessageType>();
        messageTypes.addAll(types);
    }

    /** Get the matched message types.
    */
    public Collection<MessageType> getMessageTypes()
    {
        return messageTypes;
    }

    /** True if the given {@code messageTypes} intersects the filter's
        message types.
    */
    public boolean matchMessageType(Collection<MessageType> types)
    {
        for (MessageType tt : types)  {
            if (messageTypes.contains(tt))
                return true;
        }
        if (messageTypes.contains(MessageTypes.MSG_TYPE_ALL_TYPES))
            return true;
        return false;
    }

    /** Always returns true.  This filter only matches on message type.
    */
    public boolean matchRemaining(Message message)
    {
        return true;
    }

    public String toString() {
        return "[MessageTypeFilter "+toStringInternal()+"]";
    }

    protected String toStringInternal() {
        String result="types=";
        if (messageTypes == null)
            return result+messageTypes;
        for (MessageType type : messageTypes)
            result += type.getMsgTypeString()+",";
        return result;
    }

    private Set<MessageType> messageTypes;
}

