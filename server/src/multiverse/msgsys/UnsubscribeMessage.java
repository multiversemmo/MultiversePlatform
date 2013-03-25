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

public class UnsubscribeMessage extends Message
{
    public UnsubscribeMessage() {
        msgType = MessageTypes.MSG_TYPE_UNSUBSCRIBE;
    }

    UnsubscribeMessage(long subId)
    {
        msgType = MessageTypes.MSG_TYPE_UNSUBSCRIBE;
        subIds = new ArrayList<Long>(4);
        subIds.add(subId);
    }

    UnsubscribeMessage(Collection<Long> subIds)
    {
        msgType = MessageTypes.MSG_TYPE_UNSUBSCRIBE;
        this.subIds = new ArrayList<Long>(subIds.size());
        this.subIds.addAll(subIds);
    }

    List<Long> getSubIds()
    {
        return subIds;
    }

    void add(long subId)
    {
        if (subIds == null)
            subIds = new ArrayList<Long>(4);
        subIds.add(subId);
    }

    void add(Collection<Long> subIds)
    {
        if (this.subIds == null)
            this.subIds = new ArrayList<Long>(subIds.size());
        this.subIds.addAll(subIds);
    }

    private ArrayList<Long> subIds;

    private static final long serialVersionUID = 1L;
}


