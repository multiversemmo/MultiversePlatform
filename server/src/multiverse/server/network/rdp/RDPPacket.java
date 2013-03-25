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

//
// not thread safe, dont send same packet to multiple threads
//

package multiverse.server.network.rdp;

import java.net.*;
import java.util.*;
import java.util.concurrent.locks.*;
import multiverse.server.util.*;
import multiverse.server.network.*;

//
// natural ordering is based on sequence numbering
//
public class RDPPacket implements Comparable {
    public RDPPacket() {
	super();
    }

    // sets up seqnum,host,port info from the connection object
    public RDPPacket(RDPConnection con) {
	super();
	con.getLock().lock();
	try {
	    setSeqNum(con.getSendNextSeqNum());
	    setPort(con.getRemotePort());
	    setInetAddress(con.getRemoteAddr());
	    isSequenced(con.isSequenced());
	}
	finally {
	    con.getLock().unlock();
	}
    }

    // might go further and test whether they both have data also, and 
    // the same data.  should an ack packet with the same seqnum 
    // be equal to a data packet?  probably not
    public boolean equals(Object o) {
	RDPPacket other = (RDPPacket) o;
	return (other.getSeqNum() == getSeqNum());
    }

    public int compareTo(Object o) {
	if (! (o instanceof RDPPacket)) {
	    throw new ClassCastException("expected RDPPacket");
	}
	RDPPacket other = (RDPPacket) o;
	long mySeq = getSeqNum();
	long otherSeq = other.getSeqNum();
	if (mySeq < otherSeq) {
	    return -1;
	}
	if (mySeq > otherSeq) {
	    return 1;
	}
	return 0;
    }

    public String toString() {
	String s = new String("RDPPacket[" +
			      "seqNum=" +
			      getSeqNum() +
			      ",port=" +
			      getPort() +
			      ",remoteAddress=" +
			      getInetAddress() +
			      ",isSyn=" +
			      isSyn() +
			      ",isEak=" +
			      isEak() +
			      ",ackNum=" +
			      getAckNum() +
			      ",isAck=" +
			      isAck() +
			      ",isRst=" +
			      isRst() +
                              ",isNul=" +
                              isNul() +
			      ",age(ms)=" +
			      (java.lang.System.currentTimeMillis() -
			       getTransmitTime()));
	if (isSyn()) {
	    s += ",isSequenced=" + 
		isSequenced() +
		",maxSendUnacks=" +
		mSendUnacks + 
		",maxReceiveSegSize=" +
		mMaxReceiveSegmentSize;
	}
	s += ",hasData=" + (getData() != null);
	if (getData() != null) {
	    s += ",dataLen=" + getData().length;
	}
	s += "]";
	return s;
    }

    public static RDPPacket makeSynPacket(RDPConnection con) {
	con.getLock().lock();
	try {
	    RDPPacket p = new RDPPacket();
	    p.isSyn(true);
	    p.setSeqNum(con.getInitialSendSeqNum());
	    p.setMaxSendUnacks(con.getRcvMax());
	    p.setMaxRcvSegmentSize(con.getMaxReceiveSegmentSize());
	    
	    p.isSequenced(con.isSequenced());
	    p.setPort(con.getRemotePort());
	    p.setInetAddress(con.getRemoteAddr());
	    return p;
	}
	finally {
	    con.getLock().unlock();
	}
    }

    public static RDPPacket makeNulPacket() {
//	con.getLock().lock();
//	try {
	    RDPPacket p = new RDPPacket();
//	    p.setPort(con.getRemotePort());
//	    p.setInetAddress(con.getRemoteAddr());
	    p.isNul(true);
	    return p;
//	}
//	finally {
//	    con.getLock().unlock();
//	}
    }

    public static RDPPacket makeRstPacket() {
	RDPPacket p = new RDPPacket();
	p.setRstFlag(true);
// 	p.isAck(true);
// 	p.setAckNum(con.getRcvCur());
	return p;
    }

    public int getPort() {
	return port;
    }
    public void setPort(int p) {
	port = p;
    }
    public void setInetAddress(InetAddress addr) {
	inetAddress = addr;
    }
    public InetAddress getInetAddress() {
	return inetAddress;
    }

    public boolean isSequenced() {
	return mIsSequenced;
    }
    public void isSequenced(boolean val) {
	mIsSequenced = val;
    }

    public void setSeqNum(long num) {
	seqNum = num;
    }
    public long getSeqNum() {
	return seqNum;
    }
    
    public void setAckNum(long num) {
	ackNum = num;
    }
    public long getAckNum() {
	try {
	    RDPPacket.StaticLock.lock();
	    return ackNum;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    // input is a list of RDPPacket, not Longs
    // we store a list of LONGS
    // sets isEak to true
    public void setEackList(List inList) {
	try {
	    RDPPacket.StaticLock.lock();
	    if (inList == null) {
		Log.error("eacklist is null");
		return;
	    }
	    eackList.clear();
	    Iterator iter = inList.iterator();
	    while (iter.hasNext()) {
		Object o = iter.next();
		if (! (o instanceof RDPPacket)) {
		    throw new MVRuntimeException("RDPPacket.seteacklist: not packet");
		}
		RDPPacket p = (RDPPacket) o;
		eackList.add(new Long(p.getSeqNum()));
	    }
	    if (!eackList.isEmpty()) {
		mIsEak = true;
	    }
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    // returns a copy of the eack list
    public List getEackList() {
	try {
	    RDPPacket.StaticLock.lock();
	    LinkedList<Long> list = new LinkedList<Long>(eackList);
	    return list;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    public int numEacks() {
	try {
	    RDPPacket.StaticLock.lock();
	    return eackList.size();
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    public void isSyn(boolean val) {
	mIsSyn = val;
    }
    public boolean isSyn() {
	return mIsSyn;
    }

    public void isAck(boolean val) {
	isAck = val;
    }
    public boolean isAck() {
	return isAck;
    }

    public boolean isNul() {
	return isNul;
    }
    public void isNul(boolean val) {
	isNul = val;
    }

    public boolean isEak() {
	return mIsEak;
    }
    public void setEakFlag(boolean val) {
	mIsEak = val;
    }
    
    public boolean isRst() {
	return isRst;
    }
    public void setRstFlag(boolean val) {
	isRst = val;
    }
    
    /**
     * returns the actual byte array - careful not to modify
     */
    public byte[] getData() {
	return dataBuf;
    }

    /**
     * holds a reference to the input byte array
     */
    public void setData(byte[] buf) {
        this.dataBuf = buf;
    }

    /**
     * does not perform a copy
     */
    public void wrapData(byte[] buf) {
        this.dataBuf = buf;
    }

    public void setMaxSendUnacks(long num) {
	try {
	    RDPPacket.StaticLock.lock(); // since its a long
	    mSendUnacks = num;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }
    
    public void setTransmitTime(long time) {
	try {
	    RDPPacket.StaticLock.lock();
	    transmitTime = time;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    public long getTransmitTime() {
	try {
	    RDPPacket.StaticLock.lock();
	    return transmitTime;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    public void parse(MVByteBuffer buf) {
	try {
	    RDPPacket.StaticLock.lock();
	    buf.rewind();
	    byte flagsByte = buf.getByte();
	    
	    if ((flagsByte & SYNF) != 0) {
		mIsSyn = true;
	    }
	    
	    if ((flagsByte & ACKF) != 0) {
		isAck = true;
	    }
	    
	    if ((flagsByte & EAKF) != 0) {
		mIsEak = true;
	    }
	    
	    if ((flagsByte & RSTF) != 0) {
		isRst = true;
	    }
	    if ((flagsByte & NULF) != 0) {
		isNul = true;
	    }
	    
	    headerLength = (buf.getByte() & 0xff);
	    
	    int dataLength = (buf.getShort() & 0xffffffff);
	    
	    seqNum = (buf.getInt() & LONGM);
	    ackNum = (buf.getInt() & LONGM);
	    
	    if (mIsSyn) {
		mSendUnacks = (buf.getShort() & 0xffff);
		mMaxReceiveSegmentSize = (buf.getShort() & 0xffff);
		mIsSequenced = ((buf.getByte() & 0x80) != 0);
	    }
	    else if (mIsEak) {
		// cant be syn and eak - we dont handle that
		if ((headerLength % 2) != 0) { // assuming normal length of 6
		    Log.error("headerlength boundary is incorrect");
		}
		int numEacks = (headerLength - 6) / 2;
		// read each eak
		if (! eackList.isEmpty()) {
		    Log.error("eack list not empty");
		    eackList.clear();
		}
                if (Log.loggingNet)
                    Log.net("RDPPacket: packet has " + numEacks + " eacks");
		String s = "";
                int firstEack = -1;
                int lastEack = -1;
                for (int i=0; i<numEacks; i++) {
		    int eackSeqNum = buf.getInt();
		    eackList.add(new Long(eackSeqNum));
                    if (Log.loggingNet) {
                        if (firstEack == -1) {
                            firstEack = eackSeqNum;
                            lastEack = eackSeqNum;
                        }
                        else if (eackSeqNum == lastEack + 1)
                            lastEack = eackSeqNum;
                        else {
                            if (s != "")
                                s += ",";
                            if (firstEack == lastEack)
                                s += firstEack;
                            else
                                s += firstEack + "-" + lastEack;
                            firstEack = eackSeqNum;
                            lastEack = eackSeqNum;
                        }
                    }
                }
                if (Log.loggingNet)
                    Log.net("RDPPacket.parse: packet#" + getSeqNum() +
                        ": adding eack nums " + s);
	    }
	    else if (headerLength != 6) {
		Log.error("large header len (packet not syn/eak) len=" + 
			  headerLength);
	    }

	    // read data portion
	    if (dataLength > 0) {
		byte[] tmpBuf = new byte[dataLength];
		buf.getBytes(tmpBuf, 0, dataLength);
		setData(tmpBuf);
	    }
	    else {
		setData(null);
	    }
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }
	

    // flips the buffer after its done
    public void toByteBuffer(MVByteBuffer buf) {
	try {
	    RDPPacket.StaticLock.lock();
	    buf.clear();

	    byte flagsByte = 0;
	    int numEacks = 0; // number of eacks

	    if (mIsSyn) {
		flagsByte |= SYNF;
	    }
	    if (isAck) {
		flagsByte |= ACKF;
	    }
	    if (mIsEak) {
		flagsByte |= EAKF;
	    }
	    if (isRst) {
		flagsByte |= RSTF;
	    }
	    if (isNul) {
		flagsByte |= NULF;
	    }
	    flagsByte |= VERSION;

	    buf.putByte(flagsByte);

	    // byte - header length
	    if (mIsSyn) {
		buf.putByte((byte) 9);
	    }
	    else if (mIsEak) {
		numEacks = eackList.size();
		buf.putByte((byte) (6 + (numEacks * 2)));
	    }
	    else {
		buf.putByte((byte) 6);
	    }

	    // short - put in data length
	    if (dataBuf == null) {
		buf.putShort((short) 0);
	    }
	    else {
		buf.putShort((short) dataBuf.length);
	    }
	
	    // int - sequence number
	    buf.putInt((int) seqNum);
	
	    // int - ack
	    buf.putInt((int) ackNum);

	    if (mIsSyn) {
		buf.putShort((short) mSendUnacks);
		buf.putShort((short) mMaxReceiveSegmentSize);
		if (mIsSequenced) {
		    buf.putShort((short) SEQUENCEFLAG);
		}
		else {
		    buf.putShort((short) 0);
		}
	    }
	    else if (mIsEak) {
		Iterator iter = eackList.iterator();
		while (iter.hasNext()) {
		    Long seqNum = (Long) iter.next();
		    buf.putInt((int) seqNum.longValue());
                    if (Log.loggingNet)
                        Log.net("rdppacket: tobytebuffer: adding eack# " + seqNum);
		}
	    }
	
	    // data buffer
	    if (dataBuf != null) {
		buf.putBytes(dataBuf, 0, dataBuf.length);
	    }
	    buf.flip();
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }

    public long getSendUnacks() {
	try {
	    RDPPacket.StaticLock.lock();
	    return mSendUnacks;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }
    
    public void setSendUnacks(long num) {
	try {
	    RDPPacket.StaticLock.lock();
	    mSendUnacks = num;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }
    private long mSendUnacks = 0; // SEG.MAX

    
    // how big of a segment we are willing to accept 
    public long getMaxRcvSegmentSize() {
	try {
	    RDPPacket.StaticLock.lock();
	    return mMaxReceiveSegmentSize;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }
    
    public void setMaxRcvSegmentSize(long num) {
	try {
	    RDPPacket.StaticLock.lock();
	    mMaxReceiveSegmentSize = num;
	}
	finally {
	    RDPPacket.StaticLock.unlock();
	}
    }
    private long mMaxReceiveSegmentSize = 0; // SEG.BMAX

    private int port = -1;
    private InetAddress inetAddress = null;

    private boolean mIsSyn = false;
    private boolean isAck = false;
    private boolean mIsEak = false;
    private boolean isRst = false;
    private boolean isNul = false;
    private long seqNum = 0; // SEG.SEQ
    private long ackNum = 0;
    private int headerLength = 6;
    private byte[] dataBuf = null;
    private boolean mIsSequenced = false;

    // list of eak seqnum (Long)
    private List<Long> eackList = new LinkedList<Long>();

    static protected final byte SYNF = (byte) 0x80;
    static protected final byte ACKF = (byte) 0x40;
    static protected final byte EAKF = (byte) 0x20;
    static protected final byte RSTF = (byte) 0x10;
    static protected final byte NULF = (byte) 0x08;
    static protected final byte VERSION = (byte) 0x02;
    static protected final long LONGM = 0x0ffffffffl;
    
    private long transmitTime = -1;

    //currentTimeMillis() 
    static private final int SEQUENCEFLAG = (short) 0x8000;

    transient static private Lock StaticLock = 
	LockFactory.makeLock("StaticRDPPacketLock");
}

//      Variables from Current Segment:

//      SEG.SEQ

//           The  sequence  number  of  the   segment   currently   being
//           processed.

//      SEG.ACK

//           The acknowledgement sequence number in the segment currently
//           being processed.

//      SEG.MAX

//           The maximum number of outstanding segments the  receiver  is
//           willing  to  hold,  as  specified  in  the  SYN segment that
//           established the connection.

//      SEG.BMAX

//           The maximum segment size (in octets) accepted by the foreign
//           host  on  a connection, as specified in the SYN segment that
//           established the connection.
