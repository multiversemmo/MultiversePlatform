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
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * fragmented message
 * sometimes the an event is split up into seperate subevents, stored
 * in this fragmentedmessage.  this is done because large packets
 * may be dropped by network routers
 */
public class FragmentedMessage extends Event {
    public FragmentedMessage() {
	super();
    }

    public FragmentedMessage(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    /**
     * start starts with 0
     * index is its relative pos in the list of fragmented messages which
     * make up the whole message (starting with 0)
     */
    public FragmentedMessage(byte[] data, 
                             int start, 
                             int end, 
                             int msgId,
                             int seqNum) {
        int len = end-start+1;
        this.data = new byte[len];
        System.arraycopy(data, start, this.data, 0, len);

	this.id = msgId;
        this.seqNum = seqNum;
        this.totalSeq = -1;
    }

    public String getName() {
	return "FragmentedMessage";
    }

    public String toString() {
        return "[FragmentedMessage: fragid=" + this.id +
            ", seqNum=" + seqNum +
            ", totalSeq=" + totalSeq +
            ", dataSize=" + data.length +
            "]";
    }

    /**
     * provide an entry point that returns the count of fragments for
     * a message, so we can know ahead of time if adding the fragments
     * will exceed the number of unacked packets.
     */
    public static int fragmentCount(int bufLen, int maxBytes) {
        return (bufLen + maxBytes - 1) / maxBytes;
    }
    
    public static List<FragmentedMessage> fragment(MVByteBuffer byteBuf, int maxBytes) {
        return fragment(byteBuf.copyBytes(), maxBytes);
    }
    
    /**
     * fragments the event - each fragment should not exceed more than
     * maxBytes of DATA (does not care about networking headers
     */
    public static List<FragmentedMessage> fragment(byte[] buf, int maxBytes) {
        if (maxBytes < 1) {
            throw new MVRuntimeException("maxBytes is too small");
        }
        int bufLen = buf.length;
        if (bufLen < 1) {
            throw new MVRuntimeException("buf len is < 1");
        }
        int startPos = 0;
        int endPos = -1;
        int finalPos = bufLen - 1;

        Integer serverID = Engine.getAgent().getAgentId();
        int nextID = getNextId();
        int msgId = serverID ^ nextID;
        int seqNum = 0;
        LinkedList<FragmentedMessage> fragList = new LinkedList<FragmentedMessage>();

        while (endPos < finalPos) {
            startPos = endPos + 1;
            endPos = endPos + maxBytes;
            if (endPos >= finalPos) {
                endPos = finalPos;
            }
            if (Log.loggingDebug)
                Log.debug("FragmentedMessage.fragmentEvent: bufLen = " + bufLen +
                          ", finalPos=" + finalPos + 
                          ", maxBytes=" + maxBytes +
                          ", startPos=" + startPos +
                          ", endPos=" + endPos +
                          ", seqNum=" + seqNum +
                          ", serverID=" + serverID +
                          ", msgID=" + msgId);
            FragmentedMessage frag = new FragmentedMessage(buf, 
                                                           startPos, 
                                                           endPos, 
                                                           msgId,
                                                           seqNum);
            fragList.add(frag);
            seqNum++;
        }
        fragList.getFirst().totalSeq = seqNum;
        return fragList;
    }

    public void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	
	// standard stuff
	/* long dummyId = */ buf.getLong();
	/* int msgId = */ buf.getInt();

        // id
        this.id = buf.getInt();

        // seqnum
        this.seqNum = buf.getInt();
        if (this.seqNum == 0) {
            this.totalSeq = buf.getInt();
        }

	// data
        MVByteBuffer subBuf = buf.getByteBuffer();
        this.data = subBuf.copyBytes();
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(this.data.length + 32);
        buf.putLong(-1); 
        buf.putInt(msgId);

        buf.putInt(this.id);
        buf.putInt(this.seqNum);
        if (this.seqNum == 0) {
            buf.putInt(totalSeq);
        }

        buf.putByteBuffer(new MVByteBuffer(this.data));

        buf.flip();
        return buf;
    }

    public byte[] data = null;
    private int id = -1;
    private int seqNum = -1;
    private int totalSeq = -1;
//   transient private Lock lock = 
//        LockFactory.makeLock("FragmentedMessageLock");
    protected static final Logger log = new Logger("FragmentedMessage");


    // 
    // code to generate the frag msg id number
    //
    public static int getNextId() {
        nextIdLock.lock();
        try {
            return nextId++;
        }
        finally {
            nextIdLock.unlock();
        }
    }
    private static Lock nextIdLock = LockFactory.makeLock("NextFragIDLock");
    private static int nextId = 1;

    public static void main(String[] args) {
        try {
            Engine.getEventServer().registerEventId(1, "multiverse.server.events.TerrainEvent");
            String s = new String("11");
            Event e = new TerrainEvent(s);
            FragmentedMessage.fragment(e.toBytes(), 2);
            System.out.println("done");
        }
        catch(Exception e) {
            Log.exception("FragmentedMessage.main caught exception", e);
        }
    }

}
