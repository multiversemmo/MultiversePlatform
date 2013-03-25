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
 * this event contains information about the regions in the world
 * and what goes inside of them, like forests
 */
public class RegionConfiguration extends Event {
    public RegionConfiguration() {
	super();
    }

    public RegionConfiguration(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public RegionConfiguration(String regionConfig) {
	super();
	setRegionConfig(regionConfig);
    }

    public void setRegionConfig(String regionConfig) {
	this.regionConfig = regionConfig;
    }
    public String getRegionConfig() {
	return regionConfig;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	String regionConfig = getRegionConfig();
        MVByteBuffer buf = new MVByteBuffer(regionConfig.length() * 2 + 20);
	buf.putLong(0); 
	buf.putInt(msgId);
	buf.putString(regionConfig);
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	buf.getLong(); // dummy data
	/* int msgId = */ buf.getInt();
	setRegionConfig(buf.getString());
    }

    public String getName() {
	return "RegionConfiguration";
    }

    private String regionConfig = null;
}
