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

package multiverse.server.events;

import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;

public class AutoAttackEvent extends Event {

    public AutoAttackEvent() {
	super();
    }

    public AutoAttackEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public AutoAttackEvent(MVObject attacker, 
                           MVObject target, 
                           String attackType,
                           boolean attackStatus) {
	super(attacker);
	setTargetOid(target.getOid());
	setAttackType(attackType);
        setAttackStatus(attackStatus); // attack on or off
    }

    public String getName() {
	return "AutoAttackEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getAttackerOid()); 
	buf.putInt(msgId);
	buf.putLong(getTargetOid());
	buf.putString(getAttackType());
        buf.putInt(getAttackStatus() ? 1 : 0);
	buf.flip();
	return buf;
    }
    
    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	
	// standard stuff
	long playerId = buf.getLong();
	setAttackerOid(playerId);

	/* int msgId = */ buf.getInt();

	long targetId = buf.getLong();
	setTargetOid(targetId);
	setAttackType(buf.getString());
        setAttackStatus(buf.getInt() == 1);
    }
    
    public void setAttackerOid(Long id) {
	setObjectOid(id);
    }
    public Long getAttackerOid() {
	return getObjectOid();
    }

    public void setTargetOid(Long oid) {
	targetOid = oid;
    }
    public Long getTargetOid() {
	return targetOid;
    }

    public void setAttackType(String s) {
	this.attackType = s;
    }
    public String getAttackType() {
	return attackType;
    }
    
    public void setAttackStatus(boolean s) {
        attackStatus = s;
    }
    public boolean getAttackStatus() {
        return attackStatus;
    }

    private boolean attackStatus = false;
    private Long targetOid = null;
    private String attackType = null;
}
