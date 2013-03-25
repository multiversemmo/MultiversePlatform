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

package multiverse.mars.events;

import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.server.events.*;
import multiverse.mars.objects.*;
import java.util.*;

/**
 * send out what meshes to draw for the given object
 * it is a full update, so if you unequip a rigged attachment,
 * a full update is sent out
 */
public class MarsModelInfoEvent extends ModelInfoEvent {
    public MarsModelInfoEvent() {
        super();
    }

    public MarsModelInfoEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public MarsModelInfoEvent(MVObject obj) {
	super(obj);
        if (obj instanceof MarsMob) {
            // add all the equipment also
            processMarsMob((MarsMob)obj);
        }
    }

    public MarsModelInfoEvent(Long oid) {
	super(oid);
    }

    public String getName() {
        return "MarsModelInfoEvent";
    }

    // need to add all the equipment meshes also
    void processMarsMob(MarsMob mob) {
        Set<MarsItem> items = mob.getEquippedItems();

        if (Log.loggingDebug)
            log.debug("processMarsMob: mob=" + mob.getName() +
                      ", num items=" + items.size());
        for (MarsItem item : items) {
            if (Log.loggingDebug)
                log.debug("processMarsMob: mob=" + mob.getName() +
                          ", considering equipped item " + 
                          item.getName());
            DisplayContext itemDC = item.displayContext();
            String meshFile = itemDC.getMeshFile();
            if (meshFile == null) {
                // no meshfile
                continue;
            }

            // check if its an attachment (if it is, skip it)
            if (itemDC.getAttachableFlag()) {
                continue;
            }

            // add the submeshes to this event's display context
            Set<DisplayContext.Submesh> submeshes = itemDC.getSubmeshes();
            if (Log.loggingDebug)
                log.debug("processMarsMob: mob=" + mob.getName() +
                          ", adding submeshes for item " + 
                          item.getName() +
                          ", dc=" + this.dc);
            this.dc.addSubmeshes(submeshes);
            if (Log.loggingDebug)
                log.debug("processMarsMob: mob=" + mob.getName() +
                          ", done adding submeshes for item " + 
                          item.getName() +
                          ", dc=" + this.dc);
        }
    }

    static final Logger log = new Logger("MarsModelInfoEvent");
}
