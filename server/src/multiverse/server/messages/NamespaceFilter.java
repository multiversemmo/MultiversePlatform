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
import multiverse.msgsys.*;
import multiverse.server.engine.Namespace;

/**
 * Accepts any message that implements INamespaceMessage, and compares
 * the list of namespaces to the namespace in the message.
 * @author cedeno
 *
 */
public class NamespaceFilter extends MessageTypeFilter
    implements INamespaceFilter
{

    public NamespaceFilter(Collection<Namespace> namespaces) {
        super();
        setNamespaces(namespaces);
    }
    
    public NamespaceFilter() {
        super();
    }

    public NamespaceFilter(MessageType msgType,
        Collection<Namespace> namespaces)
    {
        super();
        addType(msgType);
        setNamespaces(namespaces);
    }

    public void setNamespaces(Collection<Namespace> namespaces) {
        this.namespaces = new ArrayList<Namespace>(namespaces);
    }

    public Collection<Namespace> getNamespaces() {
        return namespaces;
    }

    public boolean matchRemaining(Message msg) {
        if (msg instanceof INamespaceMessage) {
            INamespaceMessage namespaceMsg = (INamespaceMessage)msg;
            Namespace msgNamespace = namespaceMsg.getNamespace();
            boolean matches = namespaces.contains(msgNamespace);
            return matches;
        }
        return false;
    }
    
    public String toString() {
        return "[NamespaceFilter " + toStringInternal() + "]";
    }

    protected String toStringInternal() {
        String s = "";
        if (namespaces != null) {
            for (Namespace ns : namespaces) {
                if (s != "")
                    s += ",";
                s += ns.getName();
            }
        }
        return super.toStringInternal() + " namespaces=" + s;
     }
    
    private Collection<Namespace> namespaces;

    private static final long serialVersionUID = 1L;
}

