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

package multiverse.mars.abilities;

import multiverse.server.plugins.*;
import multiverse.server.objects.*;
import multiverse.mars.core.*;
import multiverse.server.engine.Namespace;

public class CreateItemAbility extends MarsAbility {
    public CreateItemAbility(String name) {
        super(name);
    }

    public String getItem() { return template; }
    public void setItem(String template) { this.template = template; }
    protected String template = null;

    public void completeActivation(State state) {
        super.completeActivation(state);
        Long playerOid = state.getObject().getOwnerOid();
        Long bagOid = playerOid;

        // Normally the persistence flag is inherited from the enclosing
        // object, but all we have are OIDs.  Assume this is only used
        // for players and players are always persistent.
        Template overrideTemplate = new Template();
        overrideTemplate.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, true);

        Long itemOid = ObjectManagerClient.generateObject(template, overrideTemplate);
        InventoryClient.addItem(bagOid, playerOid, bagOid, itemOid);
    }
}
