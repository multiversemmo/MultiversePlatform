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
 * we are responding a registerentity event
 * we use the data stored rather than the getEntity.toBytes() when we call
 * toBytes() because its what the server tells us that is the correct
 * information.  
 */
public class RegisterEntityResponseEvent extends Event {
    public RegisterEntityResponseEvent() {
        super();
    }

    public RegisterEntityResponseEvent(MVByteBuffer buf, ClientConnection con) {
        super(buf,con);
    }

    /**
     * was the original request for a portaling obj
     */
    public RegisterEntityResponseEvent(MVObject obj, boolean status, boolean portalRequest) {
	super(obj);
        if (Log.loggingDebug)
            Log.debug("RegisterEntityResponseEvent: in constructor, obj=" + obj +
                      ", status=" + status +
                      ", portal=" + portalRequest +
                      ", calling toBytes");
	data = obj.toBytes();
        Log.debug("RegisterEntityResponseEvent: created data");
	setStatus(status);
	isPortal(portalRequest);
    }

    public String getName() {
        return "RegisterEntityResponse";
    }

    public MVByteBuffer toBytes() {
        int msgId = Engine.getEventServer().getEventID(this.getClass());

	// create the message
	byte [] d = getData();
        MVByteBuffer buf = new MVByteBuffer(d.length + 32);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);
	buf.putInt(getStatus() ? 1 : 0);

	// make a blob and put it in the message
	buf.putInt(getData().length);
	buf.putBytes(d, 0, d.length);
	buf.putBoolean(isPortal());

	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
        buf.rewind();
        setObjectOid(buf.getLong());
        /* int msgId = */ buf.getInt();

        setStatus(buf.getInt() == 1);
	
	int dataLen = buf.getInt();
	byte[] data = new byte[dataLen];
	buf.getBytes(data,0,dataLen);
	setData(data);

	isPortal(buf.getBoolean());
    }

    public byte[] getData() {
        return data;
    }
    public void setData(byte[] data) {
        this.data = data;
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

    public void isPortal(boolean flag) {
	isPortalFlag = flag;
    }
    public boolean isPortal() {
	return isPortalFlag;
    }

    private boolean isPortalFlag = false;
    private boolean responseStatus = false;
    private byte[] data = null;
}
