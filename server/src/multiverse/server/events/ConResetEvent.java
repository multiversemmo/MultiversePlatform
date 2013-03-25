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
import multiverse.server.util.*;


/**
 * the server is being told that the connection has been reset for a
 * particular user.  this is for world servers that dont have a direct
 * connection to the user, so the proxy is telling it that the con is down
 */
public class ConResetEvent extends Event {

    public ConResetEvent() {
	super();
    }

    public ConResetEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public ConResetEvent(MVObject user) {
	super(user);
    }

    public String getName() {
	return "ConReset";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	// create the message
	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);
	buf.flip();
	return buf;
	
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();

	long playerId = buf.getLong();
	MVObject obj = MVObject.getObject(playerId);
	if (! (obj.isUser())) {
	    throw new MVRuntimeException("ConResetEvent.parseBytes: not a user");
	}
	setUser(obj);
	/* int msgId = */ buf.getInt();
    }

    public void setUser(MVObject user) {
	setObject(user);
    }

}
