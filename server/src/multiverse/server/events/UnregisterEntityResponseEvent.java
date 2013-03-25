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

/**
 * we are responding to a unregisterentity event
 */
public class UnregisterEntityResponseEvent extends Event {
    public UnregisterEntityResponseEvent() {
        super();
    }

    /**
     * was the original request for a portaling obj
     */
    public UnregisterEntityResponseEvent(MVObject obj, boolean status) {
	super(obj);
	setStatus(status);
    }

    public String getName() {
        return "UnregisterEntityResponse";
    }

    public MVByteBuffer toBytes() {
        int msgId = Engine.getEventServer().getEventID(this.getClass());

	// create the message
	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);
	buf.putInt(getStatus() ? 1 : 0);
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
        buf.rewind();
        setObjectOid(buf.getLong());
        /* int msgId = */ buf.getInt();

        setStatus(buf.getInt() == 1);
    }

    /**
     * returns false if the connection is closed
     */
    public boolean getStatus() {
        return responseStatus;
    }
    public void setStatus(boolean status) {
        responseStatus = status;
    }

    private boolean responseStatus = false;
}
