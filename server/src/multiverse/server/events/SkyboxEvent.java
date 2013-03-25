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

/**
 * this event contains information about the skybox
 * the server usually sends this event to the user when they log in
 * in the loginhandler.
 * it doesnt have an entity_id associated with it because
 * its general information about the world
 */
public class SkyboxEvent extends Event {
    public SkyboxEvent() {
	super();
    }

    public SkyboxEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public SkyboxEvent(String skyboxInfo) {
	super();
	setSkybox(skyboxInfo);
    }

    public void setSkybox(String skyboxInfo) {
	this.skybox = skyboxInfo;
    }
    public String getSkybox() {
	return skybox;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(0); 
	buf.putInt(msgId);
	buf.putString(getSkybox());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	buf.getLong(); // dummy data
	/* int msgId = */ buf.getInt();
	setSkybox(buf.getString());
    }

    public String getName() {
	return "SkyboxEvent";
    }

    private String skybox = null;
}
