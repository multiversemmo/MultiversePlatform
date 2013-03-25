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
import multiverse.server.util.*;

/**
 * the server is receiving a serialized entity in this event
 * the proxy server sends the approp world server the serialized
 * entity.  
 */
public class RegisterEntityEvent extends Event {
    public RegisterEntityEvent() {
	super();
    }

    public RegisterEntityEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    /**
     * we pass in the data instead of the entity because
     * we may be getting back 'newer' entity data 
     * than we have in storage - see RegisterEntityResponse.java..
     * a server may send us the data at any time when the user 'zones'
     *
     * is portaling means whether the object is coming into the 'world'
     * for the first time. in which case we tell it extra stuff,
     * like sending it a newobject message about itself.
     */
    public RegisterEntityEvent(byte[] data, boolean isPortaling) {
	super();
	setData(data);
	isPortal(isPortaling);
    }

    public String getName() {
	return "RegisterEntityEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	// create the message
        byte[] entityData = getData();
	MVByteBuffer buf = new MVByteBuffer(entityData.length + 32);
	buf.putLong(-1);
	buf.putInt(msgId);

	// make a blob and put it in the message
        if (entityData == null) {
            throw new MVRuntimeException("RegisterEntityEvent.toBytes: data is null");
        }
	buf.putInt(entityData.length);
	buf.putBytes(entityData, 0, entityData.length);
	buf.putBoolean(isPortal());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	/* long dummyId = */ buf.getLong();
	/* int msgId = */ buf.getInt();

	// data length
	int dataLen = buf.getInt();
	
	// the serialized object data
	byte[] entityData = new byte[dataLen];
	buf.getBytes(entityData, 0, dataLen);
	setData(entityData);

	isPortal(buf.getBoolean());
    }

    /**
     * the serialized entity data
     */
    public byte[] getData() {
	if (data == null) {
	    Log.warn("RegisterEntityEvent: data is null");
	    return null;
	}
	return data;
    }

    public void setData(byte[] bytes) {
	this.data = bytes;
    }


    public void isPortal(boolean b) {
	isPortalFlag = b;
    }
    
    public boolean isPortal() {
	return isPortalFlag;
    }

    boolean isPortalFlag = false;
    private byte[] data = null;
}
