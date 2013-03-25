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


public class SubscribeMessage extends Message
{
    public SubscribeMessage() {
        msgType = MessageTypes.MSG_TYPE_SUBSCRIBE;
    }

    SubscribeMessage(long subId, IFilter filter, short flags)
    {
        msgType = MessageTypes.MSG_TYPE_SUBSCRIBE;
        this.subId = subId;
        this.filter = filter;
        this.flags = flags;
    }

    SubscribeMessage(long subId, IFilter filter, MessageTrigger trigger,
            short flags)
    {
        msgType = MessageTypes.MSG_TYPE_SUBSCRIBE;
        this.subId = subId;
        this.filter = filter;
        this.trigger = trigger;
        this.flags = flags;
    }

    long getSubId()
    {
        return subId;
    }

    IFilter getFilter()
    {
        return filter;
    }

    short getFlags()
    {
        return flags;
    }

    MessageTrigger getTrigger()
    {
        return trigger;
    }

    private long subId;
    private IFilter filter;
    private MessageTrigger trigger;
    private short flags;

    private static final long serialVersionUID = 1L;
}

