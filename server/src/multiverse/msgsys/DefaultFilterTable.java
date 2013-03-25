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

// Message instance --> list of callbacks
// Message instance --> list of RemoteAgent
/** Message matching optimized around message type.  Supports any
{@link Filter} sub-class.
<p>
Used internally by {@link MessageAgent} when a {@link Filter Filter's}
filter table is 'null'.
*/
public class DefaultFilterTable extends FilterTable
{
    public synchronized void addFilter(Subscription sub, Object object)
    {
        Collection<MessageType> types = sub.filter.getMessageTypes();
        if (types == null) {
            //allTypesFilters.put(subId,filter);
            return;
        }

        for (MessageType tt : types)  {
            Map<Object,List<Subscription>> objectMap = messageTypes.get(tt);
            if (objectMap == null) {
                objectMap = new HashMap<Object,List<Subscription>>();
                messageTypes.put(tt,objectMap);
            }
            LinkedList<Subscription> subList =
                (LinkedList<Subscription>) objectMap.get(object);
            if (subList == null) {
                subList = new LinkedList<Subscription>();
                objectMap.put(object,subList);
            }
            if (sub.getTrigger() != null)
                subList.addFirst(sub);
            else
                subList.addLast(sub);
        }
    }

    public synchronized void removeFilter(Subscription sub, Object object)
    {
        Collection<MessageType> types = sub.filter.getMessageTypes();
        if (types == null) {
            //allTypesFilters.remove(subId);
            return;
        }

        for (MessageType tt : types)  {
            Map<Object,List<Subscription>> objectMap = messageTypes.get(tt);
            if (objectMap == null) {
                continue;
            }
            List<Subscription> subList = objectMap.get(object);
            if (subList == null) {
                continue;
            }
            //## this is bummer; linear search for sub-id
            ListIterator<Subscription> iterator = subList.listIterator();
            while (iterator.hasNext()) {
                Subscription ss = iterator.next();
                if (ss.subId == sub.subId) {
                    iterator.remove();
                    break;
                }
            }
            if (subList.size() == 0) {
                objectMap.remove(object);
            }
        }
    }

    public synchronized int match(Message message, Set<Object> matches,
        List<Subscription> triggers)
    {
        MessageType type = message.getMsgType();
        Map<Object,List<Subscription>> objectMap = messageTypes.get(type);
        if (objectMap == null)
            return 0;
        int count = 0;
        for (Map.Entry<Object,List<Subscription>> entry : objectMap.entrySet()) {
            List<Subscription> subs = entry.getValue();
            boolean matched = false;
            for (Subscription sub : subs) {
                if (sub.filter.matchRemaining(message))  {
                    if (!matched && matches.add(entry.getKey())) {
                        count++;
                        matched = true;
                    }
                    if (triggers != null && sub.getTrigger() != null &&
                                sub.getTrigger().match(message))
                        triggers.add(sub);
                    else
                        break;
                }
            }
        }
        return count;
    }

    Map<MessageType,Map<Object,List<Subscription>>> messageTypes =
                new HashMap<MessageType,Map<Object,List<Subscription>>>();

}

