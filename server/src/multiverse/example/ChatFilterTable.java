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

package multiverse.example;

import java.util.*;
import multiverse.msgsys.*;


public class ChatFilterTable extends FilterTable
{
    public synchronized void addFilter(Subscription sub, Object object)
    {
        Integer channel = ((ChatFilter)sub.getFilter()).getChannelId();

        Map<Object,List<Subscription>> objectMap = channels.get(channel);
        if (objectMap == null) {
            objectMap = new HashMap<Object,List<Subscription>>();
            channels.put(channel,objectMap);
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

    public synchronized void removeFilter(Subscription sub, Object object)
    {
        Integer channel = ((ChatFilter)sub.getFilter()).getChannelId();

        Map<Object,List<Subscription>> objectMap = channels.get(channel);
        if (objectMap == null) {
            return;
        }
        List<Subscription> subList = objectMap.get(object);
        if (subList == null) {
            return;
        }

        ListIterator<Subscription> iterator = subList.listIterator();
        while (iterator.hasNext()) {
            Subscription ss = iterator.next();
            if (ss.getSubId() == sub.getSubId()) {
                iterator.remove();
                break;
            }
        }
        if (subList.size() == 0) {
            objectMap.remove(object);
        }
    }

    public synchronized int match(Message message, Set<Object> matches,
        List<Subscription> triggers)
    {
        Integer channel = ((ChatMessage)message).getChannelId();
        Map<Object,List<Subscription>> objectMap = channels.get(channel);
        if (objectMap == null)
            return 0;
        int count = 0;
        for (Map.Entry<Object,List<Subscription>> entry : objectMap.entrySet()) {
            List<Subscription> subs = entry.getValue();
            boolean matched = false;
            for (Subscription sub : subs) {
                if (sub.getFilter().matchRemaining(message))  {
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

    Map<Integer,Map<Object,List<Subscription>>> channels =
                new HashMap<Integer,Map<Object,List<Subscription>>>();
}

