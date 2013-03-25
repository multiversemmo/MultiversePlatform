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

package multiverse.mars.events;

import multiverse.mars.objects.*;
import multiverse.server.engine.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import java.io.*;

/**
 * the mobserver made a new quest state, probably a user accepted a quest.
 * so now we serialize and give the world server the quest state object
 * so it can attach it to the user object
 */
public class NewQuestStateEvent extends Event {
    public NewQuestStateEvent() {
	super();
    }

    public NewQuestStateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public NewQuestStateEvent(MarsMob player, 
                              QuestState questState) {
	super(player);
        try {
            ByteArrayOutputStream ba = new ByteArrayOutputStream();
            ObjectOutputStream os = new ObjectOutputStream(ba);
            os.writeObject(questState);
            setData(ba.toByteArray());
        }
        catch(IOException e) {
            throw new RuntimeException("newqueststateevent" , e);
        }
    }

    public String getName() {
	return "NewQuestStateEvent";
    }

    public byte[] getData() {
        return questStateData;
    }
    public void setData(byte[] questStateData) {
        this.questStateData = questStateData;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(20);
	buf.putLong(getObjectOid()); 
	buf.putInt(msgId);

	byte[] data = getData();
	if (data.length > 10000) {
	    throw new MVRuntimeException("NewQuestStateEvent.toBytes: overflow");
	}
	buf.putInt(data.length);
	buf.putBytes(data, 0, data.length);

	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	long playerId = buf.getLong();
	setObjectOid(playerId);
	/* int msgId = */ buf.getInt();

	// data length
	int dataLen = buf.getInt();
	byte[] data = new byte[dataLen];
	buf.getBytes(data, 0, dataLen);
	setData(data);
    }

    protected byte[] questStateData = null;
}
