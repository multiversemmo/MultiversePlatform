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
import multiverse.server.network.*;

public class ActivateItemEvent extends Event {
	
	public ActivateItemEvent() {
		super();
	}
	
	public ActivateItemEvent(MVByteBuffer buf, ClientConnection con) {
		super(buf, con);
	}
	
	public String getName() {
		return "ActivateItemEvent";
	}
	
	public MVByteBuffer toBytes() {
		int msgId = Engine.getEventServer().getEventID(this.getClass());
		
		MVByteBuffer buf = new MVByteBuffer(20);
		buf.putLong(getObjectOid());
		buf.putInt(msgId);
		buf.putLong(getTargetOid());
		buf.putLong(getItemOid());
		buf.flip();
		return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
		buf.rewind();
		
		long playerId = buf.getLong();
		setObjectOid(playerId);
		/* int msgId = */ buf.getInt();
		long targetId = buf.getLong();
		setTargetOid(targetId);
		setItemOid(buf.getLong());
    }

    public void setTargetOid(Long oid) { targetOid = oid; }
    public Long getTargetOid() { return targetOid; }

    public void setItemOid(Long oid) { itemOid = oid; }
    public Long getItemOid() { return itemOid; }
	
    private Long targetOid = null;
    private Long itemOid = null;
}
