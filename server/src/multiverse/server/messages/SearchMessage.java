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

import multiverse.server.engine.*;
import multiverse.msgsys.MessageType;
import multiverse.msgsys.Message;
import multiverse.server.objects.ObjectType;

public class SearchMessage extends Message
{
    public SearchMessage()
    {
        setMsgType(MSG_TYPE_SEARCH);
    }

    public SearchMessage(ObjectType objectType, SearchClause searchClause,
        SearchSelection selection)
    {
        setMsgType(MSG_TYPE_SEARCH);
        setType(objectType);
        setSearchClause(searchClause);
        setSearchSelection(selection);
    }

    public ObjectType getType()
    {
        return objectType;
    }

    public void setType(ObjectType objectType)
    {
        this.objectType = objectType;
    }

    public SearchClause getSearchClause()
    {
        return searchClause;
    }

    public void setSearchClause(SearchClause searchClause)
    {
        this.searchClause = searchClause;
    }

    public SearchSelection getSearchSelection()
    {
        return selection;
    }

    public void setSearchSelection(SearchSelection selection)
    {
        this.selection = selection;
    }

    public static final MessageType MSG_TYPE_SEARCH =
        MessageType.intern("mv.SEARCH");

    private ObjectType objectType;
    private SearchClause searchClause;
    private SearchSelection selection;

    private static final long serialVersionUID = 1L;
}


