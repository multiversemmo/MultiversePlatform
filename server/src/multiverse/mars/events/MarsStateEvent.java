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

import multiverse.server.events.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.mars.objects.*;

public class MarsStateEvent extends StateEvent {
    public MarsStateEvent() {
	super();
    }

    public MarsStateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public MarsStateEvent(MarsMob marsMob, boolean fullState) {
	super();
	setObject(marsMob);
	if (fullState) {
	    addState(MarsStates.Dead.toString(), (marsMob.isDead() ? 1 : 0));

            boolean combatState = (marsMob.getAutoAttackTarget() != null);
	    addState(MarsStates.Combat.toString(), (combatState ? 1 : 0));
            addState(MarsStates.Attackable.toString(),
		     (marsMob.attackable() ? 1 : 0));
            addState(MarsStates.Stunned.toString(),
		     (marsMob.isStunned() ? 1 : 0));

            if (Log.loggingDebug)
                Log.debug("MarsStateEvent: added state of mob " +
                          marsMob.getName() + 
                          ", deadstate=" + (marsMob.isDead() ? 1 : 0) +
                          ", combatState=" + combatState);
	}
    }

    public String getName() {
	return "MarsStateEvent";
    }
}
