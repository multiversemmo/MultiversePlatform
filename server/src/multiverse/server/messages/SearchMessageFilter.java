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

package multiverse.server.messages;

import multiverse.msgsys.MessageTypeFilter;
import multiverse.msgsys.Message;
import multiverse.server.objects.ObjectType;

public class SearchMessageFilter extends MessageTypeFilter
{
    public SearchMessageFilter()
    {
    }

    public SearchMessageFilter(ObjectType objectType)
    {
        super(SearchMessage.MSG_TYPE_SEARCH);
        setType(objectType);
    }

    public ObjectType getType()
    {
        return objectType;
    }

    public void setType(ObjectType objectType)
    {
        this.objectType = objectType;
    }

    public boolean matchRemaining(Message message)
    {
        return ((SearchMessage)message).getType() == objectType;
    }

    public String toString()
    {
        return "[SearchMessageFilter "+toStringInternal()+"]";
    }

    protected String toStringInternal()
    {
        return "type="+objectType + " " + super.toStringInternal();
    }

    private ObjectType objectType;

}

