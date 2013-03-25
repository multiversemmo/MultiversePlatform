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

import java.util.Collection;
import java.util.ArrayList;

import multiverse.msgsys.Message;
import multiverse.server.engine.Namespace;

//## Need to implement Marshallable

public class SubObjectFilter extends PerceptionFilter
    implements INamespaceFilter
{
    public SubObjectFilter() {
        super();
    }

    public SubObjectFilter(Collection<Namespace> namespaces)
    {
        super();
        setNamespaces(namespaces);
    }

    public Collection<Namespace> getNamespaces()
    {
        return namespaces;
    }

    public void setNamespaces(Collection<Namespace> namespaces)
    {
        this.namespaces = new ArrayList<Namespace>(namespaces);
    }

    public boolean matchNamespace(Message message)
    {
        if (message instanceof INamespaceMessage) {
            INamespaceMessage namespaceMsg = (INamespaceMessage)message;
            Namespace msgNamespace = namespaceMsg.getNamespace();
            return namespaces.contains(msgNamespace);
        }
        return false;
    }

    public boolean matchRemaining(Message message)
    {
        if (! matchNamespace(message))
            return false;
        return super.matchRemaining(message);
    }

    protected boolean matchPerception(Message message)
    {
        return super.matchRemaining(message);
    }

    Collection<Namespace> namespaces;

    // save sub-object: match on subject-oid 
    // unload sub-object: match on subject-oid 
    // delete sub-object: match on subject-oid 
    // set persistence sub-object: match on subject-oid 
    // get/set property: match on subject-oid 
}

