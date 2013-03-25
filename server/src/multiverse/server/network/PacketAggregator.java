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

package multiverse.server.network;

import java.util.*;
import java.util.concurrent.locks.*;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.plugins.ProxyPlugin;
import multiverse.server.network.rdp.RDPConnection;

/**
 * Aggregates small packets for a short period of time, in order to
 * minimize the number of packets processed.  An aggregator is only
 * accessed if the connection lock for its connection is already held.
 *
 * Provides the plumbing to combine messages that are small and arrive
 * close within 25ms of each other.  On average with a single client
 * this reduces message traffic by a factor of three; with 8 clients,
 * by a factor of 8.  This is turned _off_ by default, since you must
 * run a tip client to receive aggregated packets.  To turn it on, add
 * the following line to your multiverse.properties file:
 * multiverse.packet_aggregation_interval=25 This says to use
 * aggregation with an aggregation interval of 25 ms.
 *
 */
public class PacketAggregator {
    public PacketAggregator(ClientConnection con) {
        this.con = con;
        currentSize = 0;
        subMessages = new LinkedList<MVByteBuffer>();
    }

    public static void initializeAggregation(Properties properties) {
        String intervalString = properties.getProperty("multiverse.packet_aggregation_interval");
        int interval = 25;
        if (intervalString != null) {
            try {
                interval = Integer.parseInt(intervalString.trim());
            }
            catch (Exception e) {
            }
        }
        packetAggregationInterval = interval;
        if (packetAggregationInterval > 0) {
            Log.info("Starting Packet Aggregator thread with an aggregation interval of " + packetAggregationInterval);
            sendAggregatorThread.start();
            sendAggregatorThreadStarted = true;
            usePacketAggregators = true;
        }
        else
            Log.info("Packet aggregator will not run, because multiverse.packet_aggregation_interval is 0");
    }

    /**
     * Add the msg to the current list of messages, sending the
     * previous aggregation if this message would put us over the
     * hardware packet limit.  Returns true if the message was queued;
     * false otherwise.
     */
    public boolean addMessage(MVByteBuffer msg) {
        int msgSize = msg.limit();
        // Reset the connection if it's over the limit
        if ((currentSize + msgSize) > ProxyPlugin.maxByteCountBeforeConnectionReset ||
            (subMessages.size() + 1) > ProxyPlugin.maxMessagesBeforeConnectionReset) {
            Log.error("PacketAggregator: Resetting client connection " + con + " because there are " + 
                (currentSize + msgSize) + " message bytes and " + (subMessages.size() + 1) + " messages queued to send");
            con.connectionReset();
            subMessages.clear();
            currentSize = 0;
            return true;
        }
        if (Log.loggingNet)
            Log.net("PacketAggregator.addMessage: adding buf of size " + (msgSize + 4) + 
                ", frag " + RDPConnection.fragmentedBuffer(msg) + 
                ", subMsg cnt " + subMessages.size() + ", currentSize " + currentSize);
        if (currentSize + msgSize + 4 + 4 * 4 >= Engine.MAX_NETWORK_BUF_SIZE) {
            if (!send()) {
                subMessages.add(msg);
                int addend = msg.limit() + 4;
                currentSize += addend;
                if (Log.loggingNet)
                    Log.net("PacketAggregator.addMessage: added buf of size " + addend + 
                        ", subMsg cnt " + subMessages.size() + ", currentSize " + currentSize);
                return false;
            }
            if (msgSize >= Engine.MAX_NETWORK_BUF_SIZE) {
                con.unaggregatedSends++;
                allUnaggregatedSends++;
                return con.sendInternal(msg);
            }
        }
        if (subMessages.size() == 0)
            earliestAddTime = System.currentTimeMillis();
        int oldCurrentSize = currentSize;
        int addend = msgSize + 4;        
        currentSize += addend;
        subMessages.add(msg);
        if (Log.loggingNet)
            Log.net("PacketAggregator.addMessage: added buf of size " + addend + 
                ", frag " + RDPConnection.fragmentedBuffer(msg) + 
                ", subMsg cnt " + subMessages.size() + ", currentSize " + currentSize);
        if (oldCurrentSize == 0)
            PacketAggregator.addAggregatedConnection(con);
//         if (Log.loggingNet)
//             Log.net("In PacketAggregator.addMessage: currentSize " + currentSize + "; submsg cnt " + subMessages.size() + "; msgSize " + msgSize);
        return true;
    }
    
    /**
     * Add a list of MVByteBuffers, each of which represents a
     * FragmentedMessage created by fragmenting an RDP message, to the
     * subMessages array.  Note that each of the fragmented messages
     * must be sent separately.
     */
    public boolean addMessageList(List<MVByteBuffer> bufs) {
        boolean rv = true;
        for (MVByteBuffer buf : bufs) {
            if (!addMessage(buf) && rv)
                rv = false;
        }
        return rv;
    }
    
    public boolean sendContentsIfOld() {
        if (earliestAddTime == null)
            // Treat it as a send, so we remove the connection from the list to examine.
            return true; 
        else if (System.currentTimeMillis() - earliestAddTime >= packetAggregationInterval) {
            // Send them if possible
            if (Log.loggingNet)
                Log.net("PacketAggregator.sendContentsIfOld: sending " + subMessages.size() + " messages");
            return send();
        }
        else
            // Not sufficiently aged yet.
            return false;
    }
    
    /**
     * Send whatever messages have accumulated
     */
    public boolean send() {
        con.getLock().lock();
        try {
            int cnt = subMessages.size();
            boolean rv = false;
            if (Log.loggingNet)
                Log.net("PacketAggregator.send: count of subMessages is " + cnt + ", total bytes " + currentSize);
            if (cnt == 0)
                return true;
            else if (cnt == 1) {
                rv = con.sendInternal(subMessages.get(0));
                if (rv) {
                    con.unaggregatedSends++;
                    allUnaggregatedSends++;
                    currentSize = 0;
                    subMessages.clear();
                }
            }
            else {
                while (con.canSend() && currentSize > 0)
                    currentSize = con.sendMultibuf(subMessages, currentSize);
                rv = currentSize == 0;
            }
            if (rv) {
                earliestAddTime = null;
            }
            else if (Log.loggingNet)
                Log.net("PacketAggregator.send: rv is false; currentSize " + currentSize + ", subbuf cnt " + cnt);
            return rv;
        }
        finally {
            con.getLock().unlock();
        }
    }
        
    public Long getEarliestAddTime() {
        return earliestAddTime;
    }

    /**
     * A thread to ensure packets don't sit in the aggregator very long
     */
    static Thread sendAggregatorThread = new Thread(new SendAggregatorThread(), "Aggregator");
    static boolean sendAggregatorThreadStarted = false;
    
    /**
     * An array of ClientConnections whose aggregators need to be
     * checked, plus the lock that controls it.
     */
    static ClientConnection[] aggregatedConnections = null;
    static ClientConnection[] tempAggregatedConnections = null;
    static ClientConnection[] failedAggregatedConnections = null;
    static int aggregatedConnectionsSize = 0;
    static int aggregatedConnectionsUsed = 0;
    static final int agggregatedConnectionsIncrement = 25;
    static Lock aggregatedConnectionsLock = LockFactory.makeLock("StaticPacketAggregatedConnectionLock");


    /**
     * The number of milliseconds between checking aggregators.
     */
    public static int packetAggregationInterval = 0;
    
    /**
     * A procedure to add a connection to the list of aggregated connections
     */
    public static void addAggregatedConnection(ClientConnection con) {
        aggregatedConnectionsLock.lock();
        try {
            addAggregatedConnectionInternal(con);
        }
        finally {
            aggregatedConnectionsLock.unlock();
        }
    }
    
    /**
     * This is only called while the aggregatedConnectionLock is held
     */
    protected static void addAggregatedConnectionInternal(ClientConnection con) {
        if (aggregatedConnections == null) {
            aggregatedConnections = new ClientConnection[agggregatedConnectionsIncrement];
            tempAggregatedConnections = new ClientConnection[agggregatedConnectionsIncrement];
            failedAggregatedConnections = new ClientConnection[agggregatedConnectionsIncrement];
            aggregatedConnectionsUsed = 0;
            aggregatedConnectionsSize = agggregatedConnectionsIncrement;
        }
        else if (aggregatedConnectionsUsed == aggregatedConnectionsSize) {
            aggregatedConnectionsSize += agggregatedConnectionsIncrement;
            ClientConnection[] newAggregatedConnections = new ClientConnection[aggregatedConnectionsSize];
            for (int i=0; i<aggregatedConnectionsUsed; i++)
                newAggregatedConnections[i] = aggregatedConnections[i];
            aggregatedConnections = newAggregatedConnections;
            tempAggregatedConnections = new ClientConnection[aggregatedConnectionsSize];
            failedAggregatedConnections = new ClientConnection[aggregatedConnectionsSize];
        }
        aggregatedConnections[aggregatedConnectionsUsed++] = con;
    }

    // //////////////////////////////////////////////////
    //
    // SEND AGGREGATOR THREAD - Checks aggregators that may have messages to send
    // 
    //
    static class SendAggregatorThread implements Runnable {
        public SendAggregatorThread() {
        }

        public void run() {
            while (true) {
                try {
                    long startTime = System.currentTimeMillis();
                    int tempCount = 0;
                    ClientConnection[] currentTempAggregatedConnections = null;
                    aggregatedConnectionsLock.lock();
                    try {
                        currentTempAggregatedConnections = tempAggregatedConnections;
                        tempCount = aggregatedConnectionsUsed;
                        for (int i=0; i<tempCount; i++) {
                            currentTempAggregatedConnections[i] = aggregatedConnections[i];
                            aggregatedConnections[i] = null;
                        }
                        aggregatedConnectionsUsed = 0;
                    }
                    finally {
                        aggregatedConnectionsLock.unlock();
                    }
                    
                    int failedCount = 0;
                    ClientConnection[] currentFailedAggregatedConnections = failedAggregatedConnections;
                    for (int i=0; i<tempCount; i++) {
                        ClientConnection con = currentTempAggregatedConnections[i];
                        currentTempAggregatedConnections[i] = null;
                        if (con == null || con.getLock() == null)
                            continue;
                        con.getLock().lock();
                        try {
                            // Ignore connections that aren't in the state OPEN
                            if (!con.isOpen() || !con.canSendInternal())
                                continue;
                            if (!con.packetAggregator.sendContentsIfOld())
                                currentFailedAggregatedConnections[failedCount++] = con;
                        }
                        finally {
                            con.getLock().unlock();
                        }
                    }
                    if (failedCount != 0) {
                        aggregatedConnectionsLock.lock();
                        try {
                            for (int i=0; i<failedCount; i++) {
                                ClientConnection con = currentFailedAggregatedConnections[i];
                                addAggregatedConnectionInternal(con);
                                currentFailedAggregatedConnections[i] = null;
                            }
                        }
                        finally {
                            aggregatedConnectionsLock.unlock();
                        }
                    }
                    long now = System.currentTimeMillis();
                    long sleepTime = Math.max(0, (packetAggregationInterval - Math.max(0, now - startTime)));
                    //                     Log.debug("PacketAggregator.run: sleepTime " + sleepTime + ", packetAggregationInterval " + packetAggregationInterval + ", now " + now + ", startTime " + startTime);
                    if (sleepTime > 0) {
                        packetAggregatorSleeps++;
                        Thread.sleep(sleepTime);
                    }
                    else
                        packetAggregatorNoSleeps++;
                } catch (Exception e) {
                    Log.exception("PacketAggregator.SendAggregatorThread.run caught exception", e);
                }
            }
        }
    }

    static class AggregatorStatsThread implements Runnable {
        public AggregatorStatsThread() {
        }

        public void run() {
            long lastAggregatedSends = 0;
            long lastUnaggregatedSends = 0;
            long lastSentMessagesAggregated = 0;
            long lastAggregatedReceives = 0;
            long lastUnaggregatedReceives = 0;
            long lastReceivedMessagesAggregated = 0;
            long lastCounterTime = System.currentTimeMillis();
            while (true) {
                long startTime = System.currentTimeMillis();
                long interval = startTime - lastCounterTime;
                if (interval > 1000) {
                    long newAggregatedSends = allAggregatedSends - lastAggregatedSends;
                    long newUnaggregatedSends = allUnaggregatedSends - lastUnaggregatedSends;
                    long newSentMessagesAggregated = allSentMessagesAggregated - lastSentMessagesAggregated;
                    long newAggregatedReceives = allAggregatedReceives - lastAggregatedReceives;
                    long newUnaggregatedReceives = allUnaggregatedReceives - lastUnaggregatedReceives;
                    long newReceivedMessagesAggregated = allReceivedMessagesAggregated - lastReceivedMessagesAggregated;
                        
                    if (Log.loggingDebug) {
                        Log.debug("PacketAggregator counters: unaggregatedSends " + newUnaggregatedSends + 
                            ", aggregatedSends " + newAggregatedSends + ", sentMessagesAggregated " + newSentMessagesAggregated);
                        Log.debug("PacketAggregator counters: unaggregatedReceives " + newUnaggregatedReceives + 
                            ", aggregatedReceives " + newAggregatedReceives + ", receivedMessagesAggregated " + newReceivedMessagesAggregated);
                        //                             Log.debug("PacketAggregator: packetAggregatorSleeps " + packetAggregatorSleeps + 
                        //                                 ", packetAggregatorNoSleeps " + packetAggregatorNoSleeps);
                    }
                    lastAggregatedSends = allAggregatedSends;
                    lastUnaggregatedSends = allUnaggregatedSends;
                    lastSentMessagesAggregated = allSentMessagesAggregated;
                    lastAggregatedReceives = allAggregatedReceives;
                    lastUnaggregatedReceives = allUnaggregatedReceives;
                    lastReceivedMessagesAggregated = allReceivedMessagesAggregated;
                    lastCounterTime = startTime;
                }
            }
        }
    }

    /**
     * Some statistics summed over all connections.  These are not
     * protected by a lock; we assume that incrementing them is
     * atomic, and in any case, they are just for logging purposes.
     */
    public static long allAggregatedSends = 0;
    public static long allSentMessagesAggregated = 0;
    public static long allUnaggregatedSends = 0;
    
    public static long allAggregatedReceives = 0;
    public static long allReceivedMessagesAggregated = 0;
    public static long allUnaggregatedReceives = 0;

    protected ClientConnection con;
    protected int currentSize;
    protected List<MVByteBuffer> subMessages;
    protected Long earliestAddTime = null;

    // Controls whether aggregation will be used.
    public static boolean usePacketAggregators = false; 
   
    // Statistics
    public static long packetAggregatorSleeps = 0;
    public static long packetAggregatorNoSleeps = 0;
}
