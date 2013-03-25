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
import multiverse.server.objects.*;
import multiverse.server.util.*;

public class ComEvent extends Event {

	public final static int SAY = 1;
	public final static int SERVER_INFO = 2;
	public final static int COMBAT_INFO = 5;
	public final static int GROUP = 4;
	
    public ComEvent() {
		super();
    }

//     public ComEvent(MVByteBuffer buf, RDPConnection con) {
// 	super(buf, con);
//     }

	public ComEvent(MVObject comSrc, int channel, String msg) {
		super(comSrc);
		setChannelId(channel);
		setMessage(msg);
	}

    public String getName() {
		return "ComEvent";
    }

    public MVByteBuffer toBytes() {
		int msgId = Engine.getEventServer().getEventID(this.getClass());
		
		MVByteBuffer buf = new MVByteBuffer(200);
		buf.putLong(getObjectOid());
		buf.putInt(msgId);
		buf.putInt(getChannelId());
		buf.putString(getMessage());
		buf.flip();
		return buf;
    }
	
    protected void parseBytes(MVByteBuffer buf) {
		buf.rewind();
		
		// read in the message id
		long playerId = buf.getLong();
		/* int msgId = */ buf.getInt();

		int channel = buf.getInt();
		String message = buf.getString();

                if (Log.loggingDebug)
                    Log.debug("ComEvent.parseBytes: playerId=" + 
			      playerId + ", msg=" + message);

		setObjectOid(playerId);
		setChannelId(channel);
		setMessage(message);
    }

    public void setMessage(String msg) {
		mMessage = msg;
    }
    public String getMessage() {
		return mMessage;
    }
    public void setChannelId(int channelId) {
		this.channelId = channelId;
    }
    public int getChannelId() {
		return channelId;
    }

    private String mMessage = null;
    private int channelId = 0;

}
