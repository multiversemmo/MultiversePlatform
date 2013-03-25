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

// checked for thread safety

package multiverse.server.network.rdp;

import multiverse.server.engine.*;
import multiverse.server.events.FragmentedMessage;
import multiverse.server.network.ClientConnection;
import multiverse.server.network.*;
import multiverse.server.network.rdp.RDPConnection;
import multiverse.server.util.*;

import java.util.*;
import java.net.*;
import java.nio.channels.*;
import java.util.concurrent.locks.*;

// keeps track of state for an individual RDP Connection
// unique to (localport, remoteHost, remotePort) combination

public class RDPConnection extends ClientConnection implements Cloneable {
    public RDPConnection() {
        super();
    }

    public void registerMessageCallback(ClientConnection.MessageCallback pcallback) {
        packetCallback = pcallback;
    }

    public int connectionKind() {
        return ClientConnection.connectionTypeRDP;
    }
    
    public boolean isOpen() {
        return getState() == OPEN;
    }
    
    public boolean isClosed() {
        return getState() == CLOSED;
    }
    
    public boolean isClosing() {
        return getState() == CLOSE_WAIT;
    }
    
    public void open(String hostname, int remotePort) {
        try {
            open(hostname, remotePort, true);
        }
        catch (Exception e) {
            Log.exception("RDPConnection.open for host " + hostname + ", port " + remotePort, e);
            throw new MVRuntimeException(e.toString());
        }
        
    }
    
    public void open(String hostname, int remotePort, int localPort,
            boolean isSequenced, int receiveBufferSize)
        throws UnknownHostException, BindException,
            MVRuntimeException, InterruptedException, java.io.IOException
    {
        InetAddress addr = InetAddress.getByName(hostname);
        open(addr, remotePort, localPort, isSequenced, receiveBufferSize);
    }

    public void open(String hostname, int remotePort, boolean isSequenced)
            throws UnknownHostException, BindException, MVRuntimeException,
            InterruptedException, java.io.IOException {
        InetAddress addr = InetAddress.getByName(hostname);
        open(addr, remotePort, null, isSequenced, defaultReceiveBufferSize);
    }

    public void open(String hostname, int remotePort, boolean isSequenced, int receiveBufferSize)
            throws UnknownHostException, BindException, MVRuntimeException,
            InterruptedException, java.io.IOException {
        InetAddress addr = InetAddress.getByName(hostname);
        open(addr, remotePort, null, isSequenced, receiveBufferSize);
    }

    public void open(InetAddress address, int remotePort, boolean isSequenced)
            throws UnknownHostException, BindException, MVRuntimeException,
            InterruptedException, java.io.IOException {
        open(address, remotePort, null, isSequenced, defaultReceiveBufferSize);
    }

    public void open(InetAddress address, int remotePort, boolean isSequenced, int receiveBufferSize)
            throws UnknownHostException, BindException, MVRuntimeException,
            InterruptedException, java.io.IOException {
        open(address, remotePort, null, isSequenced, receiveBufferSize);
    }

    public void open(InetAddress address, Integer remotePort,
            Integer localPort, boolean isSequenced)
            throws java.net.BindException, MVRuntimeException, InterruptedException, java.io.IOException {
        open(address, remotePort, localPort, isSequenced, defaultReceiveBufferSize);
    }
    
    public void open(InetAddress address, Integer remotePort,
            Integer localPort, boolean isSequenced, int receiveBufferSize)
            throws java.net.BindException, MVRuntimeException, InterruptedException, java.io.IOException {
        lock.lock();
        try {
            if (Log.loggingNet)
                Log.net("RDPConnection.open: remoteaddr=" + address
                        + ", remotePort=" + remotePort + ", localPort=" + localPort
                        + ", isSequenced=" + isSequenced);

            DatagramChannel dc = RDPServer.bind(localPort, receiveBufferSize);
            if (dc == null) {
                throw new java.net.BindException(
                        "RDPConnection.open: RDPServer.bind returned null datagram channel");
            }
            if (Log.loggingNet)
                Log.net("RDPConnection.open: RDPServer.bind succeeded");
            mIsSequenced = isSequenced;
            mRemoteAddr = address;
            mRemotePort = remotePort;
            mLocalPort = dc.socket().getLocalPort();

            RDPServer.registerConnection(this, dc);
            if (Log.loggingNet)
                Log.net("RDPConnection.open: registered connection");
            if (mState != CLOSED) {
                throw new java.net.BindException("Error - incorrect state");
            }
            if (Log.loggingNet)
                Log.net("RDPConnection: setting localport to " + mLocalPort
                        + ", dynamicLocalPort=" + (localPort == null));
            setDatagramChannel(dc);

            // set the state before sending the syn packet, otherwise
            // when we get the syn/ack, we may be in the wrong state
            // due to race condition
            setState(SYN_SENT);

            // send a synpacket to the remote host to initiate the connection
            RDPPacket synpacket = RDPPacket.makeSynPacket(this);
            sendPacketImmediate(synpacket, false);

            while (getState() != OPEN) {
                if (Log.loggingNet)
                    Log.net("RDPConnection: waiting for OPEN state, current="
                            + toStringState(getState()));
                stateChanged.await();
            }
        } finally {
            lock.unlock();
        }
    }

    /**
     * initialize this connection from the info from this syn packet
     */
    void initConnection(DatagramChannel dc, RDPPacket synPacket) {
        setDatagramChannel(dc);
        setLocalPort(dc.socket().getLocalPort());
        setRemotePort(synPacket.getPort());
        setRemoteAddr(synPacket.getInetAddress());

        setRcvIrs(synPacket.getSeqNum()); // set the initial rcv seq num
        setRcvCur(synPacket.getSeqNum()); // update last segment received
        setMaxSendUnacks(synPacket.getSendUnacks());
        setSBufMax(synPacket.getMaxRcvSegmentSize());
        isSequenced(synPacket.isSequenced());
        setState(RDPConnection.SYN_RCVD);

    }

    // Return a string containing the IP and port
    public String IPAndPort() {
        return "RDP(" + dc.socket().getInetAddress() + ":" + getLocalPort() + ")";
    }

    public void connectionReset() {
        if (packetCallback != null)
            packetCallback.connectionReset(this);
    }

    /**
     * the external interface to the sending machinery.  Enqueues the
     * message with the connection's aggregator
     */
    public void send(MVByteBuffer buf) {
        if (logMessageContents && Log.loggingNet) {
            Log.net("RDPConnection.send: length " + buf.limit() + ", packet " + 
                DebugUtils.byteArrayToHexString(buf));
        }
        boolean rv = sendIfPossible(buf);
        if (!rv && !PacketAggregator.usePacketAggregators)
            Log.error("RDPConnection.send for con " + this + ", not aggregating " + ", packet lost!");
    }

    /**
     * This version of send does packet fragmenting so that it happens
     * _above_ the level of the packet aggregator.
     */
    public boolean sendIfPossible(MVByteBuffer buf) {
        int fragmentCount = FragmentedMessage.fragmentCount(buf.limit(), Engine.MAX_NETWORK_BUF_SIZE);
        List<MVByteBuffer> bufList = null;
        if (fragmentCount > 1) {
            byte[] data = buf.copyBytesFromZeroToLimit();
            List<FragmentedMessage> fragList = FragmentedMessage.fragment(data, Engine.MAX_NETWORK_BUF_SIZE);
            bufList = new LinkedList<MVByteBuffer>();
            int i = 0;
            for (FragmentedMessage frag : fragList) {
                MVByteBuffer fragBuf = frag.toBytes();
                fragBuf.rewind();
                i++;
                if (Log.loggingNet)
                    Log.net("RDPConnection.sendIfPossible: adding frag buf " + i + " of " + fragList.size() + ", frag " + fragmentedBuffer(fragBuf));
                bufList.add(fragBuf);
            }
        }
        lock.lock();
        try {
            if (PacketAggregator.usePacketAggregators) {
                if (fragmentCount > 1)
                    return packetAggregator.addMessageList(bufList);
                else
                    return packetAggregator.addMessage(buf);
            }
            else {
                unaggregatedSends++;
                PacketAggregator.allUnaggregatedSends++;
                if (fragmentCount > 1) {
                    for (MVByteBuffer fragBuf : bufList) {
                        if (!sendFragmentedPacket(fragBuf.copyBytes()))
                            return false;
                    }
                    return true;
                }
                else
                    return sendInternal(buf);
            }
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * main method to send data over the connection.  copies the
     * bytebuffer before sending it.  Returns false and _doesn't_ send
     * the packet because if it would exceed the allowed count of unacked
     * packets.  In that case, the packet is added to the list of
     * pending packets for the connection
     */
    public boolean sendInternal(MVByteBuffer buf) {
        // Test if we would have too many unacked packets; if so, return false
        long timer = 0;
        if (Log.loggingDebug)
            timer = System.currentTimeMillis();
        byte[] tmp = buf.copyBytesFromZeroToLimit();
        boolean rv = sendFragmentedPacket(tmp);
        long t = (System.currentTimeMillis() - timer);
        if (t != 0 && Log.loggingDebug)
            Log.debug("RDPConnection.send: time in ms=" + t);
        return rv;
    }
    
    public static boolean fragmentedBuffer(MVByteBuffer buf) {
        if (buf.limit() < 12)
            return false;
        byte[] array = buf.array();
        int msgId = 0;
        int index = 8; // Past the long that starts the message
        for (int i = 3; i >= 0; i--)
            msgId = msgId | (array[index++] << (8 * i));
        boolean frag = (msgId == fragmentMsgId);
        return frag;
    }
    
    /**
     * This method sends multiple messages.  Packet fragmentation has
     * already been done at a higher level, so there are two cases
     * it must deal with:
     *  1. The normal case is that the first messages in subMessages are
     *     small messages that are combined into a single aggregatedRDP
     *     message and sent.
     *  2. The other case is that the first message in subMessages is a
     *     FragmentedMessage which is sent by itself, not combined with
     *     other messages.
     */
    public int sendMultibuf(List<MVByteBuffer> subMessages, int currentSize) {
        int sentSize = currentSize;
        int n = 0;
        boolean fragmentedBuffer = false;
        // Need to add up sizes of first n buffers
        sentSize = 0;
        for (MVByteBuffer buf : subMessages) {
            int sentBufSize = buf.limit() + 4;
            boolean frag = fragmentedBuffer(buf);
            if (frag) {
                if (n > 0) {
                    fragmentedBuffer = false;
                    break;
                }
                else {
                    n = 1;
                    sentSize = sentBufSize;
                    fragmentedBuffer = true;
                    break;
                }
            }
            if (sentSize + sentBufSize + 4 * 4 >= Engine.MAX_NETWORK_BUF_SIZE)
                break;
            sentSize += sentBufSize;
            n++;
        }
        boolean rv = false;
        if (fragmentedBuffer) {
            sendInternal(subMessages.get(0));
            subMessages.remove(0);
        }
        else {
            MVByteBuffer multiBuf = new MVByteBuffer(sentSize + 4 * 4);
            multiBuf.putLong(-1); 
            multiBuf.putInt(aggregatedMsgId);
            multiBuf.putInt(n);
            for (int i=0; i<n; i++) {
                MVByteBuffer buf = subMessages.get(0);
                subMessages.remove(0);
                multiBuf.putByteBuffer(buf);
            }
            multiBuf.flip();
            rv = sendInternal(multiBuf);
        }
        if (rv) {
            aggregatedSends++;
            PacketAggregator.allAggregatedSends++;
            sentMessagesAggregated += n;
            PacketAggregator.allSentMessagesAggregated += n;
        }
        if (Log.loggingNet)
            Log.net("RDPConnection.sendMultiBuf: sent " + n + " bufs, " + sentSize + " bytes, bufs left " + subMessages.size() + 
                ", bytes left " + (currentSize - sentSize) + ", rv is " + rv);
        return currentSize - sentSize;
    }
    
    // Returns true if we can send at least one more packet before
    // exceeding the number of unacked packets allowed by RDP.
    public boolean canSend() {
        lock.lock();
        try {
            return canSendInternal();
        }
        finally {
            lock.unlock();
        }
    }
    
    // Returns true if we can send at least one more packet before
    // exceeding the number of unacked packets allowed by RDP.  This
    // version assumes that the RDPConnection lock is already held
    public boolean canSendInternal() {
        return getState() == OPEN &&
                (mSendNextSeqNum < mSendUnackd + mMaxSendUnacks);
    }
    
    /**
     * this one assumes we have fragmented the packet if necessary
     */
    boolean sendFragmentedPacket(byte[] data) {
        lock.lock();
        try {
            if (getState() != OPEN) {
		if (getState() == CLOSE_WAIT || getState() == CLOSED) {
		    Log.error("Trying to send on a closed connection");
		    return false;
		}
		throw new MVRuntimeException("Connection is not OPEN: state="
			+ toStringState(getState()));
            }

            RDPPacket p = new RDPPacket();
            p.setData(data);

            // check if we can send the data
            if (!canSendInternal()) {
                // Too many unacked packets - - this isn't supposed to
                // happen, so throw an error
                Log.error("RDPConnection.sendFragmentedPacket: Too many unacked packets: mSendNextSeqNum " + mSendNextSeqNum + 
                    " >= mSendUnackd " + mSendUnackd + " + mMaxSendUnacks" + mMaxSendUnacks + "; packet is " + p);
                throw new MVRuntimeException("Too many unacked packets");
            }
            else {
                sendPacketImmediate(p, false);
            }
        } finally {
            lock.unlock();
        }
	return true;
    }

    /**
     * returns the number of packets that can be added before the con buffer
     * gets full
     */
    public long unackBufferRemaining() {
        lock.lock();
        try {
//            return ((mSendUnackd + mMaxSendUnacks) - mSendNextSeqNum);
            return 70 - unackLength();
        } finally {
            lock.unlock();
        }
    }

    public int unackLength() {
        lock.lock();
        try {
            if (Log.loggingNet)
                Log.net("RDPConnection.unackLength: con=" + this.toStringVerbose()
                        + ", sendNextSeqNum=" + mSendNextSeqNum + ", sendUnackd="
                        + mSendUnackd);
            return (int) (mSendNextSeqNum - mSendUnackd);
        } finally {
            lock.unlock();
        }
    }

    /**
     * sends a reset packet to the other side, sets the connection to
     * CLOSE_WAIT. the rdpserver will close the connection for real after
     * waiting for a bit
     */
    public void close() {
        lock.lock();
        try {
            // Log.dumpStack("RDPConnection.close");
	    if (getState() == CLOSE_WAIT || getState() == CLOSED) {
		// Already closing connection, ignore
		return;
	    }

            this.setState(RDPConnection.CLOSE_WAIT);
            this.setCloseWaitTimer();

            // send rst packet
            if (Log.loggingNet)
                Log.net("RDPConnection.close: sending reset packet to other side");
            try {
                RDPPacket rstPacket = RDPPacket.makeRstPacket();
                this.sendPacketImmediate(rstPacket, false);
            } catch (Exception e) {
                Log.error("got exception while sending reset: " + e);
            }

            // call the resetcallback
            if (Log.loggingDebug)
                log.debug("RDPConnection.close: calling connectionReset callback, con=" + this.toStringVerbose());
            ClientConnection.MessageCallback pcb = this.getCallback();
            pcb.connectionReset(this);
        } finally {
            lock.unlock();
        }
    }

    // ///////////////////////////////////////////////////
    //
    // accessors
    //
    public void setMaxReceiveSegmentSize(int size) {
        mMaxReceiveSegmentSize = size;
    }

    public boolean isSequenced() {
        return mIsSequenced;
    }

    public void isSequenced(boolean isSequenced) {
        mIsSequenced = isSequenced;
    }

    public void setState(int state) {
        lock.lock();
        try {
            mState = state;

            // signal state change
            stateChanged.signal();
        } finally {
            lock.unlock();
        }
    }

    public int getState() {
        return mState;
    }

    public DatagramChannel getDatagramChannel() {
        return dc;
    }

    public void setDatagramChannel(DatagramChannel dc) {
        this.dc = dc;
    }

    public int getRemotePort() {
        return mRemotePort;
    }

    public void setRemotePort(int port) {
        mRemotePort = port;
    }

    public int getLocalPort() {
        return mLocalPort;
    }

    public void setLocalPort(int port) {
        mLocalPort = port;
    }

    public InetAddress getRemoteAddr() {
        return mRemoteAddr;
    }

    public void setRemoteAddr(InetAddress remoteAddr) {
        mRemoteAddr = remoteAddr;
    }

    /**
     * sets the null packet timer, we send a nul packet once every 30 seconds.
     * this will kick in the retry timer which disconnects the user when the
     * client doesnt ack a packet see RetryThread in the RDPServer
     */
    void setLastNullPacketTime() {
        lock.lock();
        try {
            this.nullPacketTime = System.currentTimeMillis();
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the time when the last null packet was sent
     */
    long getLastNullPacketTime() {
        lock.lock();
        try {
            return nullPacketTime;
        } finally {
            lock.unlock();
        }
    }

    long nullPacketTime = 0;

    /**
     * sets the close wait time to the current time 30 seconds later, it will be
     * removed
     */
    void setCloseWaitTimer() {
        lock.lock();
        try {
            this.closeWaitTime = System.currentTimeMillis();
        } finally {
            lock.unlock();
        }
    }

    long getCloseWaitTimer() {
        lock.lock();
        try {
            return closeWaitTime;
        } finally {
            lock.unlock();
        }
    }

    long closeWaitTime = -1;

    public String toString() {
        return "RDP(" + mRemoteAddr + ":" + mRemotePort + ")";
    }

    public String toStringVerbose() {
        return "RDPConnection[" + "state=" + toStringState(mState)
                + ",localport=" + mLocalPort + ",remoteport=" + mRemotePort
                + ",remoteaddr=" + mRemoteAddr + ",isSeq=" + isSequenced()
                + ",RCV.CUR=" + mRcvCur + ",RCV.IRS=" + mRcvIrs + ",SND.MAX="
                + mMaxSendUnacks + ",RBUF.MAX=" + mMaxReceiveSegmentSize
                + ",SND.UNA=" + mSendUnackd + "]";
    }

    //
    // private code
    //
    // increment our snd.nxt if it is syn, nul, or has data
    // flag says if its a re-transmit - in which case we dont
    // add it to the ack list and we wont increment the counter
    protected void sendPacketImmediate(RDPPacket packet, boolean retransmit) {

        lock.lock();
        try {
            packet.setPort(getRemotePort());
            packet.setInetAddress(getRemoteAddr());
            packet.isSequenced(isSequenced());
            packet.isAck(true);
            packet.setAckNum(mRcvCur);
            packet.setEackList(getEackList());
            
            if (!retransmit) {
                packet.setSeqNum(getSendNextSeqNum());
            }
            if (Log.loggingNet)
                Log.net("RDPConnection: SENDING PACKET (localport=" + mLocalPort
                        + "): " + packet + ", retransmit=" + retransmit);
            RDPServer.sendPacket(getDatagramChannel(), packet);
            RDPServer.transmits++;
            if (retransmit)
                RDPServer.retransmits++;

            if (Log.loggingDebug) {
                // FIXME: remove.. 
                // used now for logging of what types of messages we're sending
                byte[] packetData = packet.getData();
                if (packetData != null) {
                    MVByteBuffer tmpBuf = new MVByteBuffer(packetData);
                    tmpBuf.getLong();
                    int msgTypeNum = tmpBuf.getInt();            
                    if (Log.loggingNet)
                        Log.net("RDPServer.sendPacket: msgType='" + MVMsgNames.msgName(msgTypeNum) + "', addr=" + getRemoteAddr() + 
                            ", port=" + getRemotePort() + ", retransmit=" + retransmit);
                }
            }
            
            //
            // check if we should increment the next seq number to send
            //
            if ((!retransmit)
                    && (packet.isSyn() || packet.isNul() || (packet.getData() != null))) {
//                packet.setSeqNum(getSendNextSeqNum());
                packet.setTransmitTime(java.lang.System.currentTimeMillis());
                mSendNextSeqNum++;
                if (Log.loggingNet)
                    log.net("incremented seq# to " + mSendNextSeqNum);
            } else {
                log
                        .net("not incrementing packet seqNum since no data or is retransmit");
            }

            // 
            // check if we should add this packet to the unack list
            //
            if (packet.isSyn()) {
                // Log.net("not adding to unacklist: issyn packet");
            } else if ((packet.getData() != null) || packet.isNul()) {

                // make sure this isn't a re-transmit packet
                if (!retransmit) {
                    if (Log.loggingNet)
                        Log.net("adding to unacklist");
                    addUnackPacket(packet);
                } else {
                    // do not increment seqnumber if we're just retransmitting
                    if (Log.loggingNet)
                        Log.net("not adding to unacklist - is a retransmit");
                }
            } else {
                if (Log.loggingNet)
                    Log.net("not adding to unacklist - has no data");
            }
        } catch (Exception e) {
            throw new MVRuntimeException(e.toString());
        } finally {
            lock.unlock();
        }
    }

    protected RDPPacket receivePacket() {
        try {
            lock.lock();

            // get a packet from the reader, using the per-connection receiveBuffer

            InetSocketAddress sockAddr = (InetSocketAddress) getDatagramChannel()
                    .receive(receiveBuffer.getNioBuf());
            receiveBuffer.flip();

            RDPPacket packet = new RDPPacket();
            packet.setPort(sockAddr.getPort());
            packet.setInetAddress(sockAddr.getAddress());
            packet.parse(receiveBuffer);
            return packet;
        } catch (Exception e) {
            throw new MVRuntimeException(e.toString());
        } finally {
            lock.unlock();
        }
    }

    //
    // variables / fields
    //
    private DatagramChannel dc = null; // bound to port

    private int mLocalPort = -1;

    private int mRemotePort = -1;

    private InetAddress mRemoteAddr = null;

    private int mState = CLOSED;

    // TODO: Can this 4000 be smaller?  Should there be a large static
    // MVByteBuffer to hold the result of the receive, followed by a
    // copy to a smaller buffer.
    private MVByteBuffer receiveBuffer = new MVByteBuffer(4000);
    
    public ClientConnection.MessageCallback getCallback() {
        return packetCallback;
    }

    public void setCallback(ClientConnection.MessageCallback cb) {
        packetCallback = cb;
    }

    // call when we get packet
    private ClientConnection.MessageCallback packetCallback = null;

    // The maximum number of outstanding (unacknowledged) segments
    // that can be sent. The sender should not send more than this
    // number of segments without getting an acknowledgement.
    // SND.MAX
    public long getMaxSendUnacks() {
        try {
            lock.lock();
            return mMaxSendUnacks;
        } finally {
            lock.unlock();
        }
    }

    public void setMaxSendUnacks(long max) {
        try {
            lock.lock();
            if (overrideMaxSendUnacks == -1) {
                if (Log.loggingNet)
                    Log.net("RDPConnection: setting max send unacks to " + max);
                mMaxSendUnacks = max;
            } else {
                if (Log.loggingNet)
                    Log.net("RDPConnection: using override max sendunacks instead");
                mMaxSendUnacks = overrideMaxSendUnacks;
            }
        } finally {
            lock.unlock();
        }
    }

    private long mMaxSendUnacks = -1;

    // initial send sequence number (sent in SYN)
    // SND.ISS
    public long getInitialSendSeqNum() {
        try {
            lock.lock();
            return mInitialSendSeqNum;
        } finally {
            lock.unlock();
        }
    }

    public void setInitialSendSeqNum(long seqNum) {
        try {
            lock.lock();
            mInitialSendSeqNum = seqNum;
        } finally {
            lock.unlock();
        }
    }

    private long mInitialSendSeqNum = 1;

    // sequence of the next segment to be sent
    // SND.NXT
    public long getSendNextSeqNum() {
        try {
            lock.lock();
            return mSendNextSeqNum;
        } finally {
            lock.unlock();
        }
    }

    public void setSendNextSeqNum(long num) {
        try {
            lock.lock();
            mSendNextSeqNum = num;
        } finally {
            lock.unlock();
        }
    }

    private long mSendNextSeqNum = 1;

    // sequence of oldest unacknowledged segment
    // SND.UNA
    public long getSendUnackd() {
        try {
            lock.lock();
            return mSendUnackd;
        } finally {
            lock.unlock();
        }
    }

    public void setSendUnackd(long num) {
        try {
            lock.lock();
            mSendUnackd = num;
        } finally {
            lock.unlock();
        }
    }

    private long mSendUnackd = -1;

    // sequence number of last segment received correctly and in sequence
    // RCV.CUR
    public long getRcvCur() {
        try {
            lock.lock();
            return mRcvCur;
        } finally {
            lock.unlock();
        }
    }

    public void setRcvCur(long rcvCur) {
        try {
            lock.lock();
            mRcvCur = rcvCur;
        } finally {
            lock.unlock();
        }
    }

    private long mRcvCur = -1;

    // The maximum number of segments that can be buffered for this
    // connection.
    // RCV.MAX
    public long getRcvMax() {
        try {
            lock.lock();
            return mRcvMax;
        } finally {
            lock.unlock();
        }
    }

    public void setRcvMax(long max) {
        try {
            lock.lock();
            mRcvMax = max;
        } finally {
            lock.unlock();
        }
    }

    private long mRcvMax = 250;

    // The initial receive sequence number. This is the sequence
    // number of the SYN segment that established this connection.
    // RCV.IRS
    public long getRcvIrs() {
        try {
            lock.lock();
            return mRcvIrs;
        } finally {
            lock.unlock();
        }
    }

    public void setRcvIrs(long rcvIrs) {
        try {
            lock.lock();
            mRcvIrs = rcvIrs;
        } finally {
            lock.unlock();
        }
    }

    private long mRcvIrs = -1;

    // The array of sequence numbers of segments that have been
    // received and acknowledged out of sequence.
//    private int mRcvdSeqNoN = -1;

    // A timer used to time out the CLOSE-WAIT state.
//    private int mCloseWait = -1;

    // The largest possible segment (in octets) that can legally be
    // sent. This variable is specified by the foreign host in the
    // SYN segment during connection establishment.
    public long getSBufMax() {
        try {
            lock.lock();
            return mSBufMax;
        } finally {
            lock.unlock();
        }
    }

    public void setSBufMax(long max) {
        try {
            lock.lock();
            mSBufMax = max;
        } finally {
            lock.unlock();
        }
    }

    private long mSBufMax = -1;

    // The largest possible segment (in octets) that can be
    // received. This variable is specified by the user when the
    // connection is opened. The variable is sent to the foreign
    // host in the SYN segment.
    // RBUF.MAX
    public void setMaxReceiveSegmentSize(long max) {
        try {
            lock.lock();
            mMaxReceiveSegmentSize = max;
        } finally {
            lock.unlock();
        }
    }

    public long getMaxReceiveSegmentSize() {
        try {
            lock.lock();
            return mMaxReceiveSegmentSize;
        } finally {
            lock.unlock();
        }
    }

    private long mMaxReceiveSegmentSize = 4000;
    static int DefaultMaxReceiveSegmentSize = 4000;
    
    
    private boolean mIsSequenced = false;

    public static final int OPEN = 1;

    public static final int LISTEN = 2;

    public static final int CLOSED = 3;

    public static final int SYN_SENT = 4;

    public static final int SYN_RCVD = 5;

    public static final int CLOSE_WAIT = 6;

    public static String toStringState(int i) {
        if (i == OPEN)
            return "OPEN";
        if (i == LISTEN)
            return "LISTEN";
        if (i == CLOSED)
            return "CLOSED";
        if (i == SYN_SENT)
            return "SYN_SENT";
        if (i == SYN_RCVD)
            return "SYN_RCVD";
        if (i == CLOSE_WAIT)
            return "CLOSE_WAIT";
        return "UNKNOWN";
    }

    // list of unacked packets (oldest ones are always in front)
    void addUnackPacket(RDPPacket p) {
        try {
            lock.lock();
            unackPacketSet.add(p);
            if (Log.loggingNet)
                Log.net("RDPCon: added to unacked list - " + p + ", list size="
                        + unackPacketSet.size() + ",unacklist="
                        + unackListToShortString());
        } finally {
            lock.unlock();
        }
    }

    // removes all packets up to and including the seqNum
    void removeUnackPacketUpTo(long seqNum) {
        try {
            lock.lock();
            if (Log.loggingNet)
                Log.net("removingunackpacketupto: " + seqNum);
            Iterator iter = unackPacketSet.iterator();
            while (iter.hasNext()) {
                RDPPacket p = (RDPPacket) iter.next();
                if (p.getSeqNum() <= seqNum) {
                    if (Log.loggingNet)
                        Log.net("removing packet # " + p.getSeqNum()
                                + " from unacklist for con " + this);
                    iter.remove();
                } else {
                    break;
                }
            }
            if (Log.loggingNet)
                Log.net("removed all unack packets up to: " + seqNum
                        + " unacked left: " + unackPacketSet.size()
                        + " - unlist list = " + unackListToShortString());
        } finally {
            lock.unlock();
        }
    }

    int unackListSize() {
        lock.lock();
        try {
            return unackPacketSet.size();
        } finally {
            lock.unlock();
        }
    }

    String unackListToShortString() {
        try {
            lock.lock();
            int count = 0;
            int size = unackPacketSet.size();
            String s = "seq nums =";
            Iterator iter = unackPacketSet.iterator();
            while (iter.hasNext() && count++ < 6) {
                RDPPacket p = (RDPPacket) iter.next();
                s += " " + p.getSeqNum();
            }
            if (count < size)
                s += " ... " + unackPacketSet.last();
            return s;
        } finally {
            lock.unlock();
        }
    }

    // remove specific sequence number
    void removeUnackPacket(long seqNum) {
        // ya this is bad, but if we made a map, removing a bunch
        // of them would be hard since you'd have to get the keyset or values
        // and then build a 2nd set of things you want to remove since
        // you cant remove while you are iterating via the keyset or values()
        try {
            lock.lock();
            Iterator iter = unackPacketSet.iterator();
            while (iter.hasNext()) {
                RDPPacket p = (RDPPacket) iter.next();
                if (p.getSeqNum() < seqNum) {
                    continue;
                }
                if (p.getSeqNum() == seqNum) {
                    iter.remove();
                    if (Log.loggingNet)
                        Log.net("number of unackpackets left: "
                                + unackPacketSet.size());
                    break;
                } else {
                    // log.debug("RDPConnection: removeAck: could not find
                    // packet with seqNum " + seqNum);
                    break;
                }
            }
        } finally {
            lock.unlock();
        }
    }

    // if a packet is older than resendTimeout then the connection
    // will be closed
    // any packets older than cutoffTime should be resent
    // any packets older than resendTimeout will cause the connection to close
    void resend(long cutOffTime, long resendTimeout) {
        try {
            long currentTime = System.currentTimeMillis();
            lock.lock();
            Iterator iter = unackPacketSet.iterator();
            while (iter.hasNext()) {
                RDPPacket p = (RDPPacket) iter.next();
                long transmitTime = p.getTransmitTime();
                if (Log.loggingNet)
                    Log.net("RDPConnection.resend: packetTransmit: " + transmitTime
                            + ", age=" + (currentTime - transmitTime)
                            + ", resendTimeout=" + resendTimeout + ", currentTime="
                            + currentTime + ", timeout reached in "
                            + (transmitTime - resendTimeout) + " millis"
                            + ", packet=" + p);

                if (transmitTime < resendTimeout) {
                    Log.warn("RDPConnection: closing connect because resendTimeout reached.  con="
                                    + this.toStringVerbose()
                            + ", packetTransmitTime " + transmitTime
                            + ", age=" + (currentTime - transmitTime)
                            + ", currentTime=" + currentTime
                            + ", resendTimeout=" + resendTimeout
                            + ", cutOffTime=" + cutOffTime
                            + ", packet=" + p);
                    this.close();
                    return;
                }

                if (transmitTime < cutOffTime) { // packet is older
                    if (Log.loggingNet)
                        Log.net("resending expired packet: " + p
                                + " - using connection " + this.toStringVerbose());
                    sendPacketImmediate(p, true);
                    resendMeter.add();
//                 } else {
//                     if (Log.loggingNet)
//                         Log.net("resend: packet is not expired, localport="
//                                 + mLocalPort + ", seq#" + p.getSeqNum()
//                                 + ", packet=" + p + ", age="
//                                 + (currentTime - transmitTime));
                }
            }
        } finally {
            lock.unlock();
        }
    }
    static CountMeter resendMeter = new CountMeter("RDPResendMeter");
    

    // add packet to the eacklist
    void addEack(RDPPacket packet) {
        try {
            lock.lock();
            eackSet.add(packet);
        } finally {
            lock.unlock();
        }
    }

    // remove the packet with given seqnum from the eacklist
    boolean removeEack(long seqNum) {
        try {
            lock.lock();
            Iterator iter = eackSet.iterator();
            while (iter.hasNext()) {
                RDPPacket p = (RDPPacket) iter.next();
                if (p.getSeqNum() == seqNum) {
                    iter.remove();
                    return true;
                }
            }
            return false;
        } finally {
            lock.unlock();
        }
    }

    // list of RDPPackets
    // makes a copy of the current eack set and returns it as a list
    List getEackList() {
        try {
            lock.lock();
            LinkedList<RDPPacket> list = new LinkedList<RDPPacket>(eackSet);
            return list;
        } finally {
            lock.unlock();
        }
    }

    // is the passed in seqnumber in the eacklist already?
    boolean hasEack(long seqNum) {
        try {
            lock.lock();
            Iterator iter = eackSet.iterator();
            while (iter.hasNext()) {
                RDPPacket p = (RDPPacket) iter.next();
                if (p.getSeqNum() == seqNum) {
                    return true;
                }
            }
            return false;
        } finally {
            lock.unlock();
        }
    }

    // key=Integer(sequence number), value=RDPPacket
    private TreeSet<RDPPacket> unackPacketSet = new TreeSet<RDPPacket>();

    // set of RDPPacket - we have received not in order and eackd
    // if we see one of these again, dont process it again
    // if this is a sequential connection, we hold onto the packet
    // in this collection until ready to give it to the user
    private TreeSet<RDPPacket> eackSet = new TreeSet<RDPPacket>();

    void addSequencePacket(RDPPacket packet) {
        try {
            lock.lock();
            sequencePackets.add(packet);
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns actual set - used to store out of sequence packets when the
     * connection is set up as sequential
     */
    SortedSet getSequencePackets() {
        return sequencePackets;
    }

    /**
     * list of packets not processed because the connection is sequential and we
     * have out of order packets
     */
    private SortedSet<RDPPacket> sequencePackets = new TreeSet<RDPPacket>();

    /**
     * this will override the maxsendunack variable that the client sends us
     */
    public static void setOverrideMaxSendUnacks(int max) {
        RDPConnection.overrideMaxSendUnacks = max;
    }

    private static int overrideMaxSendUnacks = -1;

    protected static int defaultReceiveBufferSize = 64 * 1024;
    
    /**
     * this condition is signaled when the rdp connection's state is changed.
     * this can be useful if you are waiting for your state to change to Open,
     * etc.
     */
    Condition stateChanged = lock.newCondition();

    /**
     * returns when the last message was sent to this user set by the
     * messageserver.send()
     */
    public long getLastSentMessage() {
        lock.lock();
        try {
            return lastSentMessage; // long
        } finally {
            lock.unlock();
        }
    }

    public void setLastSentMessage(long time) {
        lock.lock();
        try {
            lastSentMessage = time;
        } finally {
            lock.unlock();
        }
    }

    long lastSentMessage = 0;
    public final static int aggregatedMsgId = 74;
    public final static int fragmentMsgId = 53;

    private static final Logger log = new Logger("RDPConnection");
}
