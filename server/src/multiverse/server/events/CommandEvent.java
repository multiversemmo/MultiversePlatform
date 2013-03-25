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

public class CommandEvent extends Event {
	
	public CommandEvent() {
		super();
	}
	
	public CommandEvent(MVByteBuffer buf, ClientConnection con) {
		super(buf, con);
	}
	
	public CommandEvent(MVObject obj, MVObject target, String command) {
		super(obj);
		setTarget(target.getOid());
		setCommand(command);
	}
	
	public String getName() {
		return "CommandEvent";
	}
	
	public MVByteBuffer toBytes() {
		int msgId = Engine.getEventServer().getEventID(this.getClass());
		
		MVByteBuffer buf = new MVByteBuffer(200);
		buf.putLong(getObjectOid());
		buf.putInt(msgId);
		buf.putLong(getTarget());
		buf.putString(getCommand());
		buf.flip();
		return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
		buf.rewind();
		
		long playerId = buf.getLong();
		setObjectOid(playerId);
		/* int msgId = */ buf.getInt();
		long targetId = buf.getLong();
		setTarget(targetId);
		setCommand(buf.getString());
    }

//     // returns the obj executing the command
//     public MVObject getObject() {
// 	return getObject();
//     }
//     public void setObject(MVObject obj) {
// 	setObject(obj);
//     }

    public void setCommand(String command) {
        this.command = command;
    }
    public String getCommand() {
        return command;
    }
    public void setTarget(Long targetOid) {
        this.targetOid = targetOid;
    }
    public Long getTarget() {
        return targetOid;
    }
	
    private String command = null;
    private Long targetOid = null;
}
