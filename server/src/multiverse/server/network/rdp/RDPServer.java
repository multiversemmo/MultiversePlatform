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

package multiverse.server.network.rdp;

import java.util.*;
import java.net.*;
import java.nio.channels.*;
import java.util.concurrent.locks.*;

import multiverse.server.network.*;
import multiverse.server.util.*;

public class RDPServer implements Runnable {
    RDPServer() {
    }

    /**
     * rdpserversocket wants to bind on a local port
     */
    static DatagramChannel bind(Integer port, int receiveBufferSize)
        throws java.net.BindException, java.io.IOException, java.net.SocketException
    {
        lock.lock();
        try {
            // see if there is an existing datagramchannel bound to this port
            DatagramChannel dc = channelMap.get(port);
            if (dc != null) {
                throw new java.net.BindException(
                        "RDPServer.bind: port is already used");
            }

            // make a new datagram channel
            dc = DatagramChannel.open();
            dc.configureBlocking(false);
            dc.socket().setReceiveBufferSize(receiveBufferSize);
            if (port == null) {
                if (Log.loggingNet)
                    Log.net("RDPServer.bind: binding to a random system port");
                dc.socket().bind(null);
            } else {
                if (Log.loggingNet)
                    Log.net("RDPServer.bind: binding to port " + port);
                dc.socket().bind(new InetSocketAddress(port));
            }
            int resultingPort = dc.socket().getLocalPort();
            if (Log.loggingNet)
                Log.net("RDPServer.bind: resulting port=" + resultingPort);

            // add the channel to the channel map
            channelMap.put(resultingPort, dc);
            if (Log.loggingNet)
                Log.net("RDPServer.bind: added dc to channel map");

            // add the channel to the newChannelsSet
            // we want to register this channel with the selector
            // but the selector thread needs to do that,
            // so place it in this set, and wake up the selector
            newChannelSet.add(dc);
            if (Log.loggingNet)
                Log.net("RDPServer.bind: added dc to newChannelSet");

            // in case the rdpserver was waiting while it had no sockets,
            // signal it
            channelMapNotEmpty.signal();
            Log
                    .net("RDPServer.bind: signalled channel map not empty condition");

            // wakeup the selector -
            // it needs to register the new channel with itself
            selector.wakeup();

            if (Log.loggingNet)
                Log.net("RDPServer.bind: woke up selector");
            return dc;
        } finally {
            lock.unlock();
        }
    }

    /**
     * assume the socket is already bound, now we need to add it to the socket
     * map
     * 
     * this map is used when we get a packet and look up the datagramchannel to
     * see if its associated with a listening socket for a new rdp connection
     */
    static void registerSocket(RDPServerSocket rdpSocket, DatagramChannel dc) {
        lock.lock();
        try {
            socketMap.put(dc, rdpSocket);
        } finally {
            lock.unlock();
        }
    }

    /**
     * the conn data should already be set (remote addr, etc)
     */
    static void registerConnection(RDPConnection con, DatagramChannel dc) {
        lock.lock();
        try {
            if (Log.loggingNet)
                Log.net("RDPServer.registerConnection: registering con " + con);

            // first we get the set of connections attached to the given dc
            Map<ConnectionInfo, RDPConnection> dcConMap = allConMap.get(dc);
            if (dcConMap == null) {
                dcConMap = new HashMap<ConnectionInfo, RDPConnection>();
            }

            // add this connection to the map
            int localPort = con.getLocalPort();
            int remotePort = con.getRemotePort();
            InetAddress remoteAddr = con.getRemoteAddr();
            ConnectionInfo conInfo = new ConnectionInfo(remoteAddr, remotePort,
                    localPort);
            dcConMap.put(conInfo, con);
            allConMap.put(dc, dcConMap);
        } finally {
            lock.unlock();
        }
    }

    /**
     * removes this connection from the connections map the datagram channel
     * still sticks around in case it needs to be reused
     */
    static void removeConnection(RDPConnection con) {
        lock.lock();
        try {
            if (Log.loggingNet)
                Log.net("RDPServer.removeConnection: removing con " + con);
            con.setState(RDPConnection.CLOSED);

            DatagramChannel dc = con.getDatagramChannel();

            // first we get the set of connections attached to the given dc
            Map<ConnectionInfo, RDPConnection> dcConMap = allConMap.get(dc);
            if (dcConMap == null) {
                throw new MVRuntimeException(
                        "RDPServer.removeConnection: cannot find dc");
            }

            int localPort = con.getLocalPort();
            int remotePort = con.getRemotePort();
            InetAddress remoteAddr = con.getRemoteAddr();
            ConnectionInfo conInfo = new ConnectionInfo(remoteAddr, remotePort,
                    localPort);
            Object rv = dcConMap.remove(conInfo);
            if (rv == null) {
                throw new MVRuntimeException(
                        "RDPServer.removeConnection: could not find the connection");
            }

            // close the datagramchannel if needed
            // conditions: no other connections on this datagramchannel
            // no socket listening on this datagramchannel
            if (dcConMap.isEmpty()) {
                Log
                        .net("RDPServer.removeConnection: no other connections for this datagramchannel (port)");
                // there are no more connections on this datagram channel
                // check if there is a serversocket listening
                if (getRDPSocket(dc) == null) {
                    Log
                            .net("RDPServer.removeConnection: no socket listening on this port - closing");
                    // no socket either, close the datagramchannel
                    dc.socket().close();
                    channelMap.remove(localPort);
                    Log
                            .net("RDPServer.removeConnection: closed and removed datagramchannel/socket");
                } else {
                    Log
                            .net("RDPServer.removeConnection: there is a socket listening on this port");
                }
            } else {
                Log
                        .net("RDPServer.removeConnection: there are other connections on this port");
            }
        } finally {
            lock.unlock();
        }
    }

    // ////////////////////////////////////////////////////////////////
    //
    // internal working
    //
    // ////////////////////////////////////////////////////////////////

    /**
     * starts the server listens to incoming packets
     */
    public void run() {
        try {
            while (true) {
                if (Log.loggingNet)
                    Log.net("In RDPServer.run: starting new iteration");
                try {
                    Set<DatagramChannel> activeChannels = getActiveChannels();
                    activeChannelCalls++; 
                    Iterator<DatagramChannel> iter = activeChannels.iterator();
                    while (iter.hasNext()) {
                        DatagramChannel dc = iter.next();
                        if (Log.loggingNet)
                            Log.net("In RDPServer.run: about to call processActiveChannel");
                        processActiveChannel(dc);
                        if (Log.loggingNet)
                            Log.net("In RDPServer.run: returned from processActiveChannel");
                    }
                } catch (ClosedChannelException ex) {
                    // ignore 
                } catch (Exception e) {
                    Log.exception("RDPServer.run caught exception", e);
                }
            }
        }
        finally {
            Log.warn("RDPServer.run: thread exiting");
        }
    }

    /**
     * a DatagramChannel has data ready - process all the pending
     * packets, whether its for a rdpserversocket or rdpconnection.
     * 
     */
    void processActiveChannel(DatagramChannel dc)
        throws ClosedChannelException
    {
        RDPPacket packet;
        int count = 0;
        // read in the packet
        try {
            Set<RDPConnection> needsAckConnections = new HashSet<RDPConnection>();
            while ((packet = RDPServer.receivePacket(dc)) != null) {
                if (Log.loggingNet)
                    Log.net("RDPServer.processActiveChannel: Starting iteration with count of " + count + " packets");
                // see if there is a connection already for this packet
                InetAddress remoteAddr = packet.getInetAddress();
                int remotePort = packet.getPort();
                int localPort = dc.socket().getLocalPort();
                ConnectionInfo conInfo = new ConnectionInfo(remoteAddr, remotePort,
                        localPort);
                RDPConnection con = RDPServer.getConnection(dc, conInfo);
                if (con != null) {
                    if (Log.loggingNet)
                        Log.net("RDPServer.processActiveChannel: found an existing connection: " + con);
                    count++;
                    if (processExistingConnection(con, packet))
                        needsAckConnections.add(con);
                    // Prevent this from blocking getActiveChannels by
                    // putting an upper bound on the number of packets
                    // processed
                    if (count >= 20)
                        break;
                    continue;
                } else {
                    Log.net("RDPServer.processActiveChannel: did not find an existing connection");
                }
                // there is no connection,
                // see if there is a socket listening for new connection
                RDPServerSocket rdpSocket = RDPServer.getRDPSocket(dc);
                if (rdpSocket != null) {
                    count++;
                    processNewConnection(rdpSocket, packet);
                    return;
                }
                return;
            }
            // Finally, send out the acks
            for (RDPConnection con : needsAckConnections) {
                RDPPacket replyPacket = new RDPPacket(con);
                con.sendPacketImmediate(replyPacket, false);
            }
        }
        catch (ClosedChannelException ex) {
            Log.error("RDPServer.processActiveChannel: ClosedChannel "+dc.socket());
            throw ex;
        }
        finally {
            if (Log.loggingNet)
                Log.net("RDPServer.processActiveChannel: Returning after processing " + count + " packets");
        }
    }

        
     /**
     * there is a socket listening on the port for this packet. process if it is
     * a new connection rdp packet
     */
    public void processNewConnection(RDPServerSocket serverSocket,
            RDPPacket packet) {
        if (Log.loggingNet)
            Log.net("processNewConnection: RDPPACKET (localport="
                    + serverSocket.getPort() + "): " + packet);

//        int localPort = serverSocket.getPort();
        InetAddress remoteAddr = packet.getInetAddress();
        int remotePort = packet.getPort();
        if (!packet.isSyn()) {
            // the client is not attemping to start a new connection
            // send a reset and forget about it
            Log.debug("socket got non-syn packet, replying with reset: packet="
                    + packet);
            RDPPacket rstPacket = RDPPacket.makeRstPacket();
            rstPacket.setPort(remotePort);
            rstPacket.setInetAddress(remoteAddr);
            RDPServer.sendPacket(serverSocket.getDatagramChannel(), rstPacket);
            return;
        }

        // it is a syn packet, lets make a new connection for it
        RDPConnection con = new RDPConnection();
        DatagramChannel dc = serverSocket.getDatagramChannel();
        con.initConnection(dc, packet);

        // add new connection to allConnectionMap
        registerConnection(con, dc);

        // ack it with a syn
        RDPPacket synPacket = RDPPacket.makeSynPacket(con);
        con.sendPacketImmediate(synPacket, false);
    }

    /**
     * returns a list of rdpserversockets
     */
    Set<DatagramChannel> getActiveChannels() throws InterruptedException, java.io.IOException {
        lock.lock();
        try {
            while (channelMap.isEmpty()) {
                channelMapNotEmpty.await();
            }
        } finally {
            lock.unlock();
        }
        
        Set<SelectionKey> readyKeys = null;
        do {
            lock.lock();
            try {
                if (!newChannelSet.isEmpty()) {
                    if (Log.loggingNet)
                        Log.net("RDPServer.getActiveChannels: newChannelSet is not null");
                    Iterator<DatagramChannel> iter = newChannelSet.iterator();
                    while (iter.hasNext()) {
                        DatagramChannel newDC = iter.next();
                        iter.remove();
                        newDC.register(selector, SelectionKey.OP_READ);
                    }
                }
            } finally {
                lock.unlock();
            }
            int numReady = selector.select(); // this is a blocking call - thread safe
            selectCalls++;
            if (numReady == 0) {
                if (Log.loggingNet)
                    Log.net("RDPServer.getActiveChannels: selector returned 0");
                continue;
            }
            readyKeys = selector.selectedKeys();
            if (Log.loggingNet)
                Log.net("RDPServer.getActiveChannels: called select - # of ready keys = "
                        + readyKeys.size() + " == " + numReady);
        } while (readyKeys == null || readyKeys.isEmpty());

        lock.lock();
        try {
            // get a datagramchannel that is ready
            Set<DatagramChannel> activeChannels = new HashSet<DatagramChannel>();

            Iterator<SelectionKey> iter = readyKeys.iterator();
            while (iter.hasNext()) {
                SelectionKey key = iter.next();
                if (Log.loggingNet)
                    Log.net("RDPServer.getActiveChannels: matched selectionkey: " + key +
                            ", isAcceptable=" + key.isAcceptable() +
                            ", isReadable=" + key.isReadable() +
                            ", isValid=" + key.isValid() +
                            ", isWritable=" + key.isWritable());
                iter.remove(); // remove from the selected key list

                if (!key.isReadable() || !key.isValid()) {
                    Log.error("RDPServer.getActiveChannels: Throwing exception: RDPServer: not readable or invalid");
                    throw new MVRuntimeException("RDPServer: not readable or invalid");
                }

                DatagramChannel dc = (DatagramChannel) key.channel();
                activeChannels.add(dc);
            }
            if (Log.loggingNet)
                Log.net("RDPServer.getActiveChannels: returning " + activeChannels.size() + " active channels");
            return activeChannels;
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the RDPConnection that is registered for the given datagram
     * channel and is connected to the host/port in ConnectionInfo returns null
     * if there is no matching registered rdpconnection
     */
    static RDPConnection getConnection(DatagramChannel dc,
            ConnectionInfo conInfo) {
        lock.lock();
        try {
            Map<ConnectionInfo, RDPConnection> dcConMap = allConMap.get(dc);

            if (dcConMap == null) {
                // there isnt even a datagram associated
                if (Log.loggingNet)
                    Log.net("RDPServer.getConnection: could not find datagram");
                return null;
            }
            return dcConMap.get(conInfo);
        } finally {
            lock.unlock();
        }
    }

    static Set<RDPConnection> getAllConnections() {
        lock.lock();
        try {
            Set<RDPConnection> allCon = new HashSet<RDPConnection>();
            Iterator<Map<ConnectionInfo, RDPConnection>> iter = allConMap
                    .values().iterator();
            while (iter.hasNext()) {
                Map<ConnectionInfo, RDPConnection> dcMap = iter.next();
                allCon.addAll(dcMap.values());
            }
            return allCon;
        } finally {
            lock.unlock();
        }
    }

    /**
     * returns the RDPServerSocket that is registered for the given
     * datagramchannel returns null if none exists
     */
    static RDPServerSocket getRDPSocket(DatagramChannel dc) {
        lock.lock();
        try {
            return socketMap.get(dc);
        } finally {
            lock.unlock();
        }
    }

    static CountMeter packetCounter = new CountMeter("RDPPacketReceiveCounter");
    static CountMeter dataCounter = new CountMeter("RDPPacketReceiveDATA");

    /**
     * we have a packet that belongs to the passed in
     * connection. process the packet for the connection.  It returns
     * true if the connection is open and the packet was a data packet
     */
    boolean processExistingConnection(RDPConnection con, RDPPacket packet)
            {

        if (Log.loggingNet)
            Log.net("RDPServer.processExistingConnection: con state=" + con
                    + ", packet=" + packet);
        packetCounter.add();
        
        int state = con.getState();
        if (state == RDPConnection.LISTEN) {
            // something is wrong, we shouldn't be here
            // we get to this method after looking in the connections map
            // but all LISTEN connections should be listed direct
            // from serversockets
            Log
                    .error("RDPServer.processExistingConnection: connection shouldnt be in LISTEN state");
            return false;
        }
        if (state == RDPConnection.SYN_SENT) {
            if (!packet.isAck()) {
                Log.warn("got a non-ack packet when we're in SYN_SENT");
                return false;
            }
            if (!packet.isSyn()) {
                Log.warn("got a non-syn packet when we're in SYN_SENT");
                return false;
            }
            if (Log.loggingNet)
                Log.net("good: got syn-ack packet in syn_sent");

            // make sure its acking our initial segment #
            if (packet.getAckNum() != con.getInitialSendSeqNum()) {
                if (Log.loggingNet)
                    Log.net("syn's ack number does not match initial seq #");
                return false;
            }

            con.setRcvCur(packet.getSeqNum());
            con.setRcvIrs(packet.getSeqNum());
            con.setMaxSendUnacks(packet.getSendUnacks());
            con.setMaxReceiveSegmentSize(packet.getMaxRcvSegmentSize());
            con.setSendUnackd(packet.getAckNum() + 1);

            // ack first before setting state to open
            // otherwise some other thread will get woken up and send data
            // before we send the ack
            if (Log.loggingNet)
                Log.net("new connection state: " + con);
            RDPPacket replyPacket = new RDPPacket(con);
            con.sendPacketImmediate(replyPacket, false);
            con.setState(RDPConnection.OPEN);
            return false;
        }
        if (state == RDPConnection.SYN_RCVD) {
            if (packet.getSeqNum() <= con.getRcvIrs()) {
                Log.error("seqnum is not above rcv initial seq num");
                return false;
            }
            if (packet.getSeqNum() > (con.getRcvCur() + (con.getRcvMax() * 2))) {
                Log.error("seqnum is too big");
                return false;
            }
            if (packet.isAck()) {
                if (packet.getAckNum() == con.getInitialSendSeqNum()) {
                    if (Log.loggingNet)
                        Log.net("got ack for our syn - setting state to open");
                    con.setState(RDPConnection.OPEN); // this will notify()

                    // call the accept callback
                    // first find the serversocket
                    DatagramChannel dc = con.getDatagramChannel();
                    if (dc == null) {
                        throw new MVRuntimeException(
                                "RDPServer.processExistingConnection: no datagramchannel for connection that just turned OPEN");
                    }
                    RDPServerSocket rdpSocket = RDPServer.getRDPSocket(dc);
                    if (rdpSocket == null) {
                        throw new MVRuntimeException(
                                "RDPServer.processExistingConnection: no socket for connection that just turned OPEN");
                    }
                    ClientConnection.AcceptCallback acceptCB = rdpSocket.getAcceptCallback();
                    if (acceptCB != null) {
                        acceptCB.acceptConnection(con);
                    } else {
                        Log.warn("serversocket has no accept callback");
                    }
                    if (Log.loggingNet)
                        Log.net("RDPServer.processExistingConnection: got ACK, removing from unack list: "
                                + packet.getSeqNum());
                    con.removeUnackPacket(packet.getSeqNum());
                }
            }
        }
        if (state == RDPConnection.CLOSE_WAIT) {
            // reply with a reset on all packets
            if (!packet.isRst()) {
                RDPPacket rstPacket = RDPPacket.makeRstPacket();
                con.sendPacketImmediate(rstPacket, false);
            }
        }
        if (state == RDPConnection.OPEN) {
            if (packet.isRst()) {
                // the other side wants to close the connection
                // set the state,
                // dont call con.close() since that will send a reset packet
                if (Log.loggingDebug)
                    Log.debug("RDPServer.processExistingConnection: got reset packet for con " + con);
                if (con.getState() != RDPConnection.CLOSE_WAIT) {
                    con.setState(RDPConnection.CLOSE_WAIT);
                    con.setCloseWaitTimer();
		    // Only invoke callback when moving into CLOSE_WAIT
		    // state.  This prevents two calls to connectionReset.
		    Log.net("RDPServer.processExistingConnection: calling reset callback");
                    ClientConnection.MessageCallback pcb = con.getCallback();
		    pcb.connectionReset(con);
                }

                return false;
            }
            if (packet.isSyn()) {
                // this will close the connection (put into CLOSE_WAIT)
                // send a reset packet and call the connectionReset callback
                Log
                        .error("RDPServer.processExistingConnection: closing connection because we got a syn packet, con="
                                + con);
                con.close();
                return false;
            }

            // TODO: shouldnt it be ok for it to have same seq num?
            // if it is a 0 data packet?
            long rcvCur = con.getRcvCur();
            if (packet.getSeqNum() <= rcvCur) {
                if (Log.loggingNet)
                    Log.net("RDPServer.processExistingConnection: seqnum too small - acking/not process");
                if (packet.getData() != null) {
                    if (Log.loggingNet)
                        Log.net("RDPServer.processExistingConnection: sending ack even though seqnum out of range");
                    RDPPacket replyPacket = new RDPPacket(con);
                    con.sendPacketImmediate(replyPacket, false);
                }
                return false;
            }
            if (packet.getSeqNum() > (rcvCur + (con.getRcvMax() * 2))) {
                Log.error("RDPServer.processExistingConnection: seqnum too big - discarding");
                return false;
            }
            if (packet.isAck()) {
                if (Log.loggingNet)
                    Log.net("RDPServer.processExistingConnection: processing ack " + packet.getAckNum());
                // lock for race condition (read then set)
                con.getLock().lock();
                try {
                    if (packet.getAckNum() >= con.getSendNextSeqNum()) {
                        // acking something we didnt even send yet
                        Log.error("RDPServer.processExistingConnection: discarding -- got ack #"
                                + packet.getAckNum()
                                + ", but our next send seqnum is "
                                + con.getSendNextSeqNum() + " -- " + con);
                        return false;
                    }
                    if (con.getSendUnackd() <= packet.getAckNum()) {
                        con.setSendUnackd(packet.getAckNum() + 1);
                        if (Log.loggingNet)
                            Log.net("RDPServer.processExistingConnection: updated send_unackd num to "
                                    + con.getSendUnackd()
                                    + " (one greater than packet ack) - " + con);
                        con.removeUnackPacketUpTo(packet.getAckNum());
                    }
                    if (packet.isEak()) {
                        List eackList = packet.getEackList();
                        Iterator iter = eackList.iterator();
                        while (iter.hasNext()) {
                            Long seqNum = (Long) iter.next();
                            if (Log.loggingNet)
                                Log.net("RDPServer.processExistingConnection: got EACK: " + seqNum);
                            con.removeUnackPacket(seqNum.longValue());
                        }
                    }
                } finally {
                    con.getLock().unlock();
                    if (Log.loggingNet)
                        Log.net("RDPServer.processExistingConnection: processed ack " + packet.getAckNum());
                }
            }
            // process the data
            byte[] data = packet.getData();
            if ((data != null) || packet.isNul()) {
                dataCounter.add();
                
                // lock - since racecondition: we read then set
                con.getLock().lock();
                try {
                    rcvCur = con.getRcvCur(); // update rcvCur
                    if (Log.loggingNet)
                        Log.net("RDPServer.processExistingConnection: rcvcur is " + rcvCur);

                    ClientConnection.MessageCallback pcb = con.getCallback();
                    if (pcb == null) {
                        Log.warn("RDPServer.processExistingConnection: no packet callback registered");
                    }

                    // call callback only if we havent seen it already - eackd
                    if (!con.hasEack(packet.getSeqNum())) {
                        if (con.isSequenced()) {
                            // this is a sequential connection,
                            // make sure this is the 'next' packet
                            // is this the next sequential packet
                            if (packet.getSeqNum() == (rcvCur + 1)) {
                                // this is the next packet
                                if (Log.loggingNet)
                                    Log.net("RDPServer.processExistingConnection: conn is sequenced and received next packet, rcvCur="
                                        + rcvCur + ", packet=" + packet);
                                if ((pcb != null) && (data != null)) {
                                    queueForCallbackProcessing(pcb, con, packet);
                                }
                            } else {
                                // not the next packet, place it in queue
                                if (Log.loggingNet)
                                    Log.net("RDPServer.processExistingConnection: conn is sequenced, BUT PACKET is OUT OF ORDER: rcvcur="
                                        + rcvCur + ", packet=" + packet);
                                con.addSequencePacket(packet);
                            }
                        } else {
                            if ((pcb != null) && (data != null)) {
                                // make sure we havent already processed packet
                                queueForCallbackProcessing(pcb, con, packet);
                            }
                        }
                    } else {
                        if (Log.loggingNet)
                            Log.net(con.toString() + " already seen this packet");
                    }

                    // is this the next sequential packet
                    if (packet.getSeqNum() == (rcvCur + 1)) {
                        con.setRcvCur(rcvCur + 1);
                        if (Log.loggingNet)
                            Log.net("RDPServer.processExistingConnection RCVD: incremented last sequenced rcvd: "
                                    + (rcvCur + 1));

                        // packet in order - dont add to eack
                        // Take any additional sequential packets off eack
                        long seqNum = rcvCur + 2;
                        while (con.removeEack(seqNum)) {
                            if (Log.loggingNet)
                                Log.net("RDPServer.processExistingConnection: removing/collapsing eack: " + seqNum);
                            con.setRcvCur(seqNum++);
                        }

                        if (con.isSequenced()) {
                            rcvCur++; // since we just process the last one
                            Log
                                    .net("RDPServer.processExistingConnection: connection is sequenced, processing collapsed packets.");
                            // send any saved sequential packets also
                            Iterator iter = con.getSequencePackets().iterator();
                            while (iter.hasNext()) {
                                RDPPacket p = (RDPPacket) iter.next();
                                if (Log.loggingNet)
                                    Log.net("rdpserver: stored packet seqnum="
                                            + p.getSeqNum()
                                            + ", if equal to (rcvcur + 1)="
                                            + (rcvCur + 1));
                                if (p.getSeqNum() == (rcvCur + 1)) {
                                    Log
                                            .net("RDPServer.processExistingConnection: this is the next packet, processing");
                                    // this is the next packet - update rcvcur
                                    rcvCur++;

                                    // process this packet
                                    Log
                                            .net("RDPServer.processExistingConnection: processing stored sequential packet "
                                                    + p);
                                    byte[] storedData = p.getData();
                                    if (pcb != null && storedData != null) {
                                        queueForCallbackProcessing(pcb, con, packet);
                                    }
                                    iter.remove();
                                }
                            }
                        } else {
                            if (Log.loggingNet)
                                Log.net("RDPServer.processExistingConnection: connection is not sequenced");
                        }
                    } else {
                        if (Log.loggingNet)
                            Log.net("RDPServer.processExistingConnection: RCVD OUT OF ORDER: packet seq#: "
                                    + packet.getSeqNum()
                                    + ", but last sequential rcvd packet was: "
                                    + con.getRcvCur()
                                    + " -- not incrementing counter");
                        if (packet.getSeqNum() > rcvCur) {
                            // must be at least + 2 larger than rcvCur
                            if (Log.loggingNet)
                                Log.net("adding to eack list " + packet);
                            con.addEack(packet);
                        }
                    }
                } finally {
                    con.getLock().unlock();
                }
                return true;
            }
        }
        return false;
    }

    /**
     * reads in an rdp packet from the datagram channel - blocking call
     */
    static RDPPacket receivePacket(DatagramChannel dc)
        throws ClosedChannelException
    {
        try {
            if (dc == null) {
                throw new MVRuntimeException(
                        "RDPServer.receivePacket: datagramChannel is null");
            }

            // get a packet from the reader
            staticMVBuff.rewind();
            InetSocketAddress addr = (InetSocketAddress) dc.receive(staticMVBuff.getNioBuf());
            if (addr == null) {
                return null;
            }
            
            RDPPacket packet = new RDPPacket();
            packet.setPort(addr.getPort());
            packet.setInetAddress(addr.getAddress());
            packet.parse(staticMVBuff);
            return packet;
        } catch (ClosedChannelException ex) {
            throw ex;
        } catch (Exception e) {
            throw new MVRuntimeException("error", e);
        }
    }

    // Only used by receivePacket, which is guaranteed to be single-threaded.
    private static MVByteBuffer staticMVBuff = new MVByteBuffer(RDPConnection.DefaultMaxReceiveSegmentSize);

    
    static String printSocket(DatagramSocket socket) {
        return "[Socket: localPort=" + socket.getLocalPort() + ", remoteAddr=" + socket.getInetAddress() + ", localAddr=" + socket.getLocalAddress() + "]";
    }
    
    static CountMeter sendMeter = new CountMeter("RDPSendPacketMeter");
    static CountMeter sendDataMeter = new CountMeter("RDPSendDataPacketMeter");
    
    /**
     * make sure the packet as the remote address and remote port set
     */
    static void sendPacket(DatagramChannel dc, RDPPacket packet) {

        sendMeter.add();
        
        // allocate a buffer
        int bufSize = 100 + (packet.numEacks() * 4);
        if (packet.getData() != null) {
            bufSize += packet.getData().length;
            sendDataMeter.add();
        }
        MVByteBuffer buf = new MVByteBuffer(bufSize);
        packet.toByteBuffer(buf); // function flips the buffer

        int remotePort = packet.getPort();
        InetAddress remoteAddr = packet.getInetAddress();

        if ((remotePort < 0) || (remoteAddr == null)) {
            throw new MVRuntimeException(
                    "RDPServer.sendPacket: remotePort or addr is null");
        }

        try {
            int bytes = dc.send(buf.getNioBuf(), new InetSocketAddress(remoteAddr,
                        remotePort));
            if (bytes == 0) {
                Log.error("RDPServer.sendPacket: could not send packet, size="+
                        bufSize);
            }

            if (Log.loggingNet)
                Log.net("RDPServer.sendPacket: remoteAddr=" + remoteAddr + ", remotePort=" + remotePort + ", numbytes sent=" + bytes);
        } catch (java.io.IOException e) {
            Log.exception("RDPServer.sendPacket: remoteAddr=" + remoteAddr + ", remotePort=" + remotePort + ", got exception", e);
            throw new MVRuntimeException("RDPServer.sendPacket", e);
        }
    }

    // ////////////////////////////////////////
    //
    // Private Fields
    //

    // use 'rdpServer' object as static lock - including for bindmap
    // and connectionMap
    static RDPServer rdpServer = new RDPServer();

    // localport -> datagramchannel for that port
    private static Map<Integer, DatagramChannel> channelMap = new HashMap<Integer, DatagramChannel>();

    // maps datagramchannel to serversocket
    // so when we get a new packet on a datagram channel via select() we can
    // associate it with a server socket and thus its callback
    private static Map<DatagramChannel, RDPServerSocket> socketMap = new HashMap<DatagramChannel, RDPServerSocket>();

    // map of datagram channel to a secondary map of connectioninfo->connection
    // when we get a packet, we check if it is associated with an existing
    // connection. but there can be many connections associated with a
    // single datagramchannel (localport), so we first look up by
    // datagram channel and then that returns us a second map.
    // we key into the connectioninfo (which makes a connection unique -
    // (localport, remoteport, remoteaddr) and then get the single connection
    private static Map<DatagramChannel, Map<ConnectionInfo, RDPConnection>> allConMap = new HashMap<DatagramChannel, Map<ConnectionInfo, RDPConnection>>();

    private static Lock unsentPacketsLock = LockFactory.makeLock("unsentPacketsLock");
    static Condition unsentPacketsNotEmpty = unsentPacketsLock.newCondition();
    
    /**
     * set of new datagram channels that need to be registered with the selector
     */
    static Set<DatagramChannel> newChannelSet = new HashSet<DatagramChannel>();

    // thread that reads in new packets
    static Thread rdpServerThread = null;
    static Thread retryThread = null;
    static Thread packetCallbackThread = null;
    
    // // list of datagramchannels for sockets that are dead and should be
    // removed
    // // they became dead when the other side of the connection went away
    // // in the case of a single user connections
    // static List<DatagramChannel> deadDatagramChannelList =
    // new LinkedList<DatagramChannel>();

    static Selector selector = null;

    private static boolean rdpServerStarted = false;
    
    public static void startRDPServer() {
        if (rdpServerStarted)
            return;
        rdpServerStarted = true;
        rdpServerThread = new Thread(rdpServer, "RDPServer");
        retryThread = new Thread(new RetryThread(), "RDPRetry");
        packetCallbackThread = new Thread(new PacketCallbackThread(), "RDPCallback");
        if (Log.loggingNet)
            Log.net("static - starting rdpserver thread");
        try {
            selector = Selector.open();
        } catch (Exception e) {
            Log.exception("RDPServer caught exception opening selector", e);
            System.exit(1);
        }
        rdpServerThread.setPriority(rdpServerThread.getPriority() + 2);
        if (Log.loggingDebug)
            Log.debug("RDPServer: starting rdpServerThread with priority " +
                rdpServerThread.getPriority());
        rdpServerThread.start();
        retryThread.start();
        packetCallbackThread.start();
    }

    // used in the TreeSet of connections with pending packets
    // it has time data associated with the connection, telling us
    // when we need to re-visit this connection to send its pending
    // data, in case it was not able to send all of its packets
    // due to throttling
    static class RDPConnectionData implements Comparable {
        public RDPConnection con;
        public long readyTime;
        public int compareTo(Object arg0) {
            RDPConnectionData other = (RDPConnectionData)arg0;

            if (this.readyTime < other.readyTime) {
                if (Log.loggingNet)
                    Log.net("RDPServer.RDPConnectionData.compareTo: readyTime compare -1: thiscon=" + this.con + ", othercon=" + other.con + ", thisready=" + this.readyTime + ", otherReady=" + other.readyTime);
                return -1;
            }
            else if (this.readyTime > other.readyTime) {
                if (Log.loggingNet)
                    Log.net("RDPServer.RDPConnectionData.compareTo: readyTime compare 1: thiscon=" + this.con + ", othercon=" + other.con + ", thisready=" + this.readyTime + ", otherReady=" + other.readyTime);
                return 1;
            }
            
            if (this.con == other.con) {
                if (Log.loggingNet)
                    Log.net("RDPServer.RDPConnectionData.compareTo: conRef compare 0: thiscon=" + this.con + ", othercon=" + other.con);
                return 0;
            }
            else if (this.con.hashCode() < other.con.hashCode()) {
                if (Log.loggingNet)
                    Log.net("RDPServer.RDPConnectionData.compareTo: hashCode compare -1: thiscon=" + this.con + ", othercon=" + other.con);
                return -1;
            }
            else if (this.con.hashCode() > other.con.hashCode()) {
                if (Log.loggingNet)
                    Log.net("RDPServer.RDPConnectionData.compareTo: hashCode compare 1: thiscon=" + this.con + ", othercon=" + other.con);
                return 1;
            }
            else {
                throw new RuntimeException("error");
            }
        }
        public boolean equals(Object obj) {
            int rv = this.compareTo(obj);
            if (Log.loggingNet)
                Log.net("RDPServer.RDPConnectionData.equals: thisObj=" + this.toString() +
                        ", other=" + obj.toString() + ", result=" + rv);
            return (rv == 0);
        }
    }
    
    // //////////////////////////////////////////////////
    //
    // RETRY THREAD - also handles CLOSE_WAIT connections - to actually close
    // them
    //
    static class RetryThread implements Runnable {
        public RetryThread() {
        }

        public void run() {
            // every second, go through all the packets that havent been
            // ack'd
            List<RDPConnection> conList = new LinkedList<RDPConnection>();
            long lastCounterTime = System.currentTimeMillis();
            while (true) {
                try {
                    long startTime = System.currentTimeMillis();
                    long interval = startTime - lastCounterTime;
                    if (interval > 1000) {
                        
                        if (Log.loggingNet) {
                            Log.net("RDPServer counters: activeChannelCalls " + activeChannelCalls + 
                                ", selectCalls " + selectCalls + ", transmits " + transmits + 
                                ", retransmits " + retransmits + " in " + interval + "ms");
                        }
                        activeChannelCalls = 0;
                        selectCalls = 0;
                        transmits = 0;
                        retransmits = 0;
                        lastCounterTime = startTime;
                    }
                    if (Log.loggingNet)
                        Log.net("RDPServer.RETRY: startTime=" + startTime);

                    // go through all the rdpconnections and re-send any
                    // unacked packets
                    conList.clear();

                    lock.lock();
                    try {
                        // make a copy since the values() collection is
                        // backed by the map
                        Set<RDPConnection> conCol = RDPServer
                                .getAllConnections();
                        if (conCol == null) {
                            throw new MVRuntimeException("values() returned null");
                        }
                        conList.addAll(conCol); // make non map backed copy
                    } finally {
                        lock.unlock();
                    }

                    Iterator<RDPConnection> iter = conList.iterator();
                    while (iter.hasNext()) {
                        RDPConnection con = iter.next();
                        long currentTime = System.currentTimeMillis();

                        // is the connection in CLOSE_WAIT
                        if (con.getState() == RDPConnection.CLOSE_WAIT) {
                            long closeTime = con.getCloseWaitTimer();
                            long elapsedTime = currentTime - closeTime;
                            Log
                                    .net("RDPRetryThread: con is in CLOSE_WAIT: elapsed close timer(ms)="
                                            + elapsedTime
                                            + ", waiting for 30seconds to elapse. con="
                                            + con);
                            if (elapsedTime > 30000) {
                                // close the connection
                                Log
                                        .net("RDPRetryThread: removing CLOSE_WAIT connection. con="
                                                + con);
                                removeConnection(con);
                            } else {
                                Log
                                        .net("RDPRetryThread: time left on CLOSE_WAIT timer: "
                                                + (30000 - (currentTime - closeTime)));
                            }
                            // con.close();
                            continue;
                        }
                        if (Log.loggingNet)
                            Log.net("RDPServer.RETRY: resending expired packets "
                                    + con + " - current list size = "
                                    + con.unackListSize());

                        // see if we should send a null packet, but only if con is already open
                        if ((con.getState() == RDPConnection.OPEN) && ((currentTime - con.getLastNullPacketTime()) > 30000)) {
                            con.getLock().lock();
                            try {
                                RDPPacket nulPacket = RDPPacket
                                        .makeNulPacket();
                                con.sendPacketImmediate(nulPacket, false);
                                con.setLastNullPacketTime();
                                if (Log.loggingNet)
                                    Log.net("RDPServer.retry: sent nul packet: "
                                            + nulPacket);
                            } finally {
                                con.getLock().unlock();
                            }
                        } else {
                            if (Log.loggingNet)
                                Log.net("RDPServer.retry: sending nul packet in "
                                        + (30000 - (currentTime - con
                                                .getLastNullPacketTime())));
                        }
                        con.resend(currentTime - resendTimerMS, // resend cutoff time
                                currentTime - resendTimeoutMS); // giveup time
                    }

                    long endTime = System.currentTimeMillis();
                    if (Log.loggingNet)
                        Log.net("RDPServer.RETRY: endTime=" + endTime
                                + ", elapse(ms)=" + (endTime - startTime));
                    Thread.sleep(250);
                } catch (Exception e) {
                    Log.exception("RDPServer.RetryThread.run caught exception", e);
                }
            }
        }
    }

    static Lock lock = LockFactory.makeLock("StaticRDPServerLock");

    /**
     * this condition gets signalled when there is a new server socket. this is
     * useful when you are waiting to process a new connection
     */
    static Condition channelMapNotEmpty = lock.newCondition();

    // private MVLock lock = new MVLock("RDPServerLock");

    /**
     * maximum time (in milliseconds) a packet can be in the resend queue before
     * the connection closes itself - defaults to 30 seconds
     */
    public static int resendTimeoutMS = 30000;

    /**
     * how often we resend packets
     */
    public static int resendTimerMS = 500;
    
    public static void setCounterLogging(boolean enable)
    {
        packetCounter.setLogging(enable);
        dataCounter.setLogging(enable);
        sendMeter.setLogging(enable);
        sendDataMeter.setLogging(enable);
        RDPConnection.resendMeter.setLogging(enable);
    }

    /**
     * machinery to count select and active channel calls
     */
    public static int activeChannelCalls = 0;
    public static int selectCalls = 0;
    public static int transmits = 0;
    public static int retransmits = 0;

    // this is the worker thread which calls the callback
    // we want it in a seperate thread so it doesnt block
    // the rdpserver connections.  Currently unused.
    static class CallbackThread implements Runnable {
        CallbackThread(RDPPacketCallback cb, RDPConnection con, RDPPacket packet, MVByteBuffer buf) {
            this.cb = cb;
            this.con = con;
            this.packet = packet;
            this.buf = buf;
        }

        public void run() {
            cb.processPacket(con, buf);
        }

        RDPConnection con = null;

        RDPPacketCallback cb = null;

        RDPPacket packet = null;

        MVByteBuffer buf = null;
    }

    /**
     * Machinery to process a queue of packets that have been received
     * but not yet subjected to packet processing.
     */
    
    static class PacketCallbackStruct {
        PacketCallbackStruct(ClientConnection.MessageCallback cb, ClientConnection con, RDPPacket packet) {
            this.cb = cb;
            this.con = con;
            this.packet = packet;
        }

        ClientConnection con = null;

        ClientConnection.MessageCallback cb = null;

        RDPPacket packet = null;
    }
    
    static void queueForCallbackProcessing(ClientConnection.MessageCallback pcb, ClientConnection con, RDPPacket packet) {
        queuedPacketCallbacksLock.lock();
        try {
            queuedPacketCallbacks.addLast(new PacketCallbackStruct(pcb, con, packet));
            queuedPacketCallbacksNotEmpty.signal();
        }
        finally {
            queuedPacketCallbacksLock.unlock();
        }
    }
    
    static LinkedList<PacketCallbackStruct> queuedPacketCallbacks = new LinkedList<PacketCallbackStruct>();
    static Lock queuedPacketCallbacksLock = LockFactory.makeLock("queuedPacketCallbacksLock");
    static Condition queuedPacketCallbacksNotEmpty = queuedPacketCallbacksLock.newCondition();
    
    // this is the worker thread that processes the queue of packets
    // received but not yet subjected to callback processing.
    static class PacketCallbackThread implements Runnable {

        PacketCallbackThread() {
        }

        public void run() {
            while (true) {
                LinkedList<PacketCallbackStruct> list = null;
                try {
                    queuedPacketCallbacksLock.lock();
                    try {
                        queuedPacketCallbacksNotEmpty.await();
                    }
                    catch (Exception e) {
                        Log.error("RDPServer.PacketCallbackThread: queuedPacketCallbacksNotEmpty.await() caught exception " + e.getMessage());
                    }
                    list = queuedPacketCallbacks;
                    queuedPacketCallbacks = new LinkedList<PacketCallbackStruct>();
                }
                finally {
                    queuedPacketCallbacksLock.unlock();
                }
                if (Log.loggingNet)
                    Log.net("RDPServer.PacketCallbackThread: Got " + list.size() + " queued packets");
                for (PacketCallbackStruct pcs : list) {
		    try {
			callbackProcessPacket(pcs.cb, pcs.con, pcs.packet);
		    }
		    catch (Exception e) {
			Log.exception("RDPServer.PacketCallbackThread: ", e);
		    }
		}
            }
        }
    }

    static void callbackProcessPacket(ClientConnection.MessageCallback pcb, ClientConnection clientCon, RDPPacket packet) {
        if (packet.isNul()) {
            return;
        }
        byte[] data = packet.getData();
        MVByteBuffer buf = new MVByteBuffer(data);
        RDPConnection con = (RDPConnection)clientCon;
        // If this is a multiple-message message . . .
        if (buf.getLong() == -1 && buf.getInt() == RDPConnection.aggregatedMsgId) {
            con.aggregatedReceives++;
            PacketAggregator.allAggregatedReceives++;
            // Get the count of sub buffers
            int size = buf.getInt();
            con.receivedMessagesAggregated += size;
            PacketAggregator.allReceivedMessagesAggregated += size;
            if (Log.loggingNet)
                Log.net("RDPServer.callbackProcessPacket: processing aggregated message with " + size + " submessages");
            MVByteBuffer subBuf = null;
            for (int i=0; i<size; i++) {
                try {
                    subBuf = buf.getByteBuffer();
                } 
                catch(Exception e) {
                    Log.error("In CallbackThread, error getting aggregated subbuffer: " + e.getMessage());
                }
                if (subBuf != null)
                    pcb.processPacket(con, subBuf);
            }
        }
        else {
            con.unaggregatedReceives++;
            PacketAggregator.allUnaggregatedReceives++;
            buf.rewind();
            pcb.processPacket(con, buf);
        }
    }

}
