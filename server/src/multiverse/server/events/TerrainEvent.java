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
 * this event contains information about the terrain
 * the server usually sends this event to the user when they log in
 * in the loginhandler.
 * it doesnt have an entity_id associated with it because
 * its general information about the world
 */
public class TerrainEvent extends Event {
    public TerrainEvent() {
	super();
    }

    public TerrainEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public TerrainEvent(String terrainInfo) {
	super();
	setTerrain(terrainInfo);
    }

    public void setTerrain(String terrainInfo) {
	this.terrainInfo = terrainInfo;
    }
    public String getTerrain() {
	return terrainInfo;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	String t = getTerrain();
        MVByteBuffer buf = new MVByteBuffer(t.length() * 2 + 20);
	buf.putLong(0); 
	buf.putInt(msgId);
	buf.putString(getTerrain());
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	buf.getLong(); // dummy data
	/* int msgId = */ buf.getInt();
	setTerrain(buf.getString());
    }

    public String getName() {
	return "TerrainEvent";
    }

    private String terrainInfo = null;
}
