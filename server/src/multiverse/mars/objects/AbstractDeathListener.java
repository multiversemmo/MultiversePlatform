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

package multiverse.mars.objects;

import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.mars.events.*;

import java.rmi.*;

public abstract class AbstractDeathListener extends AbstractEventListener {

    public AbstractDeathListener() throws RemoteException {
	super();
    }

    public AbstractDeathListener(String name) throws RemoteException {
	super();
	this.name = name;
    }

    protected String name = "";

    public String getName() {
	return name;
    }

    protected boolean isDead = false;

    // handleDeath is called when the mob the listener is attached to dies
    protected abstract void handleDeath(Event event, MVObject target);
   
    // handleEvent will be called by multiple threads, so you must
    // make it thread-safe
    public void handleEvent(Event event, MVObject target) {
	MarsStateEvent stateEvent = (MarsStateEvent)event;
	Long eventObjOid = stateEvent.getObjectOid();
        if (Log.loggingDebug)
            Log.debug("AbstractDeathListener: handleEvent target=" + target + " eventobj=" + eventObjOid);
	if (eventObjOid.equals(target.getOid())) {
	    Integer dead = stateEvent.getStateMap().get(MarsStates.Dead);
	    if (dead != null) {
		if ((dead == 1) && !isDead) {
		    isDead = true;
		    Log.debug("AbstractDeathListener: handleEvent object is dead");
		    handleDeath(event, target);
		}
	    }
	}
    }
}
