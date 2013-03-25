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

import multiverse.mars.objects.*;
import multiverse.server.engine.*;
import multiverse.server.network.*;
import multiverse.server.util.*;

public class CombatEvent extends Event {
    public CombatEvent() {
	super();
    }

    public CombatEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public CombatEvent(MarsMob attacker, 
		       MarsObject target, 
		       String attackType) {
	super(target);
	setAttackType(attackType);
	setAttacker(attacker);
    }

    public String getName() {
	return "CombatEvent";
    }

    public MVByteBuffer toBytes() {
	throw new MVRuntimeException("not implemented");
    }

    protected void parseBytes(MVByteBuffer buf) {
	throw new MVRuntimeException("not implemented");
    }

    public void setAttacker(MarsMob attacker) {
	this.attacker = attacker;
    }
    public MarsMob getAttacker() {
	return attacker;
    }

    public void setAttackType(String attackType) {
	this.attackType = attackType;
    }
    public String getAttackType() {
	return attackType;
    }

    private String attackType = null;
    private MarsMob attacker = null;
}
