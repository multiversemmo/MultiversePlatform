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

import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;

public class MarsDamageEvent extends Event {
    public MarsDamageEvent() {
	super();
    }

    public MarsDamageEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public MarsDamageEvent(MVObject src, MVObject target, int dmg) {
	super(target);
	setDmg(dmg);
	setDmgSrc(src);
    }

    public String getName() {
	return "MarsDamageEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(100);
	buf.putLong(getDmgSrc().getOid()); 
	buf.putInt(msgId);
	
	buf.putLong(getObjectOid());
	buf.putString("stun");
	buf.putInt(getDmg());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();

	setDmgSrc(MVObject.getObject(buf.getLong()));
	/* int msgId = */ buf.getInt();
	setObjectOid(buf.getLong());
	/* String dmgType = */ buf.getString();
	setDmg(buf.getInt());
    }

    public void setDmgSrc(MVObject dmgSrc) {
	this.dmgSrc = dmgSrc;
    }
    public MVObject getDmgSrc() {
	return dmgSrc;
    }

    public void setDmg(int dmg) {
	this.dmg = dmg;
    }
    public int getDmg() {
	return dmg;
    }

    private int dmg = 0;
    private MVObject dmgSrc = null;
}
