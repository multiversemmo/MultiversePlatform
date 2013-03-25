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
 * this client is logging into the server (proxy usually)
 */
public class LoginEvent extends Event {
    public LoginEvent() {
	super();
    }

    public LoginEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public LoginEvent(MVObject obj) {
	super(obj);
	this.oid = obj.getOid();
    }

    public String getName() {
	return "LoginEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = 1;
	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(0);
	buf.putInt(msgId);
	buf.putLong(oid);
	buf.putString(getVersion());
        buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	/* long dummyId = */ buf.getLong();
	/* int msgId = */ buf.getInt();
	long oid = buf.getLong();
	String version = null;
        if (buf.remaining() != 0) {
            version = buf.getString();
        }
	setOid(oid);
        setVersion(version);
    }
    
    public void setOid(Long id) {
	oid = id;
    }
    public Long getOid() {
	return oid;
    }

    public void setVersion(String version) {
        this.version = version;
    }
    public String getVersion() {
        return this.version;
    }
    private String version = null;
    private Long oid = null;
}
