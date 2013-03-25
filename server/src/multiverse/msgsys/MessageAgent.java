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

package multiverse.msgsys;

import java.util.*;
import java.util.concurrent.*;
import java.nio.channels.*;
import java.nio.ByteBuffer;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.UnknownHostException;
import java.net.ConnectException;
import java.io.*;
import multiverse.server.network.*;
import multiverse.server.util.Log;
import multiverse.server.util.Base64;
import multiverse.server.util.MVRuntimeException;
import multiverse.server.util.SquareQueue;
import multiverse.server.util.SQThreadPool;
import multiverse.server.util.SecureTokenManager;
import multiverse.server.marshalling.MarshallingRuntime;


/** Application interface to the multiverse message system.
The message system provides both publish/subscribe and RPC
communication within a message domain.
<p>
Each process has a single MessageAgent connected to the {@link DomainServer}.
Agents are identified in the domain with a unique self-assigned name.
<p>
Agents must advertise the message types they intend to publish.
The advertisements are used to distribute subscriptions to those
agents that might publish a matching message.
<p>
Joining the message domain follows the pattern:

<pre>
    MessageAgent agent = new MessageAgent("my-agent-name");
    List<MessageType> myMessageTypes = getMyAdvertisements();
    agent.addAdvertisements(myMessageTypes);
    try {
        agent.openListener();
        agent.connectToDomain(domainHostName, domainPort);
        agent.waitForRemoteAgents();
    }
    catch (Exception ex) {
        System.err.println("Join message domain: "+ex);
        System.exit(1);
    }
</pre>

Agents subscribe to messages using a message {@link Filter}.  A filter is
typically one or more message types and optional matching criteria.
The subscription matches messages of the same message type and criteria.
The criteria may be anything in the message content.  Filter matching
is done by both the message sender and recipient.  The sender need only
match a single subscription to deliver a message to a subscribing agent.
<p>
A subscription may be a {@link #RESPONDER} indicating that matching
messages will return a response message.  Message RPCs only match
responder subscriptions.  An RPC responder may return only one response
message.  A broadcast RPC sender may receive multiple responses, one from
each agent with matching RPC responder.
<p>
Messages are delivered in the order sent.  Message callbacks
are invoked by a single thread.  Subsequent messages are not delivered until
the callback returns.  If parallel message processing is desired,
the application can implement a thread pool.
<p>
Non-blocking RPC response messages are delivered by a fixed size pool
of threads.

*/
public class MessageAgent implements MessageIO.Callback, TcpAcceptCallback,
        ResponseCallback
{
    /** Create a MessageAgent.
        @param name Agent name, unique within the domain.
    */
    public MessageAgent(String name)
    {
        MessageTypes.initializeCatalog();

        agentName = name;
        RPCException.myAgentName = agentName;

        subscriptions = new HashMap<Long,RegisteredSubscription>();

        sendFilters = new LinkedList<FilterTable>();
        sendFilters.add(defaultSendFilterTable);

        receiveFilters = new LinkedList<FilterTable>();
        receiveFilters.add(defaultReceiveFilterTable);

        responderSendFilters = new LinkedList<FilterTable>();
        responderSendFilters.add(defaultResponderSendFilterTable);

        responderReceiveFilters = new LinkedList<FilterTable>();
        responderReceiveFilters.add(defaultResponderReceiveFilterTable);

        addSelfRemoteAgent();

        messageIO = new MessageIO(this);
        messageIO.start();

        // new MessageMarshaller();
        new SelfMessageHandler();
    }

    /** Use this constructor if you're only going to use the domain APIs.
    */
    public MessageAgent()
    {
        MessageTypes.initializeCatalog();

        messageIO = new MessageIO(this);
        messageIO.start();

    }

    /** Get the agent name.
    */
    public String getName()
    {
        return agentName;
    }

    /** Get the domain-assigned unique agent id.
    */
    public Integer getAgentId()
    {
        return agentId;
    }

    /** Get the domain server start time.
    */
    public long getDomainStartTime()
    {
        return domainStartTime;
    }

    /** Get the agent network listener port number.
        @return Port number or -1 if the listener is not open.
    */
    public int getListenerPort()
    {
        if (listener == null)
            return -1;
        return listener.getPort();
    }

    /** Open network listener to accept connections from other agents.
        Must be called before connecting to the domain.  The agent
        will not actually accept connections until {@link #connectToDomain}
        is called.  Listening port is selected by the operating system.  
    */
    public void openListener()
        throws IOException
    {
        if (listener != null)
            return;

        listener = new TcpServer();
        listener.bind();
        agentPort = listener.getPort();
        listener.registerAcceptCallback(this);
        // Don't put into selector (kernel will accept packets,
        // but we won't see them until we select/accept)
    }

    private void startListener()
    {
        listener.start();
    }

    /** Advertise the message types this agent will publish.  Should be
        called before connecting to the domain.
    */
    public void setAdvertisements(Collection<MessageType> typeIds)
    {
        // Lock status: OK
        synchronized (advertisements) {
            advertisements = new HashSet<MessageType>(typeIds);

            sendAdvertisements();
        }

        synchronized (remoteAgents) {
            if (! selfRemoteAgent.hasFlag(RemoteAgent.HAVE_ADVERTISEMENTS)) {
                selfRemoteAgent.setFlag(RemoteAgent.HAVE_ADVERTISEMENTS);
                remoteAgents.notify();
            }
        }
    }

    /** Add a single message type to the list of those this agent will publish
     */
    public void addAdvertisement(MessageType msgType)
    {
        List<MessageType> typeIds = new LinkedList<MessageType>();
        typeIds.add(msgType);
        addAdvertisements(typeIds);
    }
    
    /** Add message types this agent will publish.
    */
    public void addAdvertisements(List<MessageType> typeIds)
    {
        // Lock status: OK
        synchronized (advertisements) {
            int count = 0;
            Set<MessageType> newAdvertisements =
                new HashSet<MessageType>(advertisements);
            for (MessageType typeId : typeIds) {
                if (! newAdvertisements.contains(typeId)) {
                    newAdvertisements.add(typeId);
                    count++;
                }
            }

            if (count == 0)
                return;
            advertisements = newAdvertisements;

            sendAdvertisements();
        }

        synchronized (remoteAgents) {
            if (! selfRemoteAgent.hasFlag(RemoteAgent.HAVE_ADVERTISEMENTS)) {
                selfRemoteAgent.setFlag(RemoteAgent.HAVE_ADVERTISEMENTS);
                remoteAgents.notify();
            }
        }
    }

    /** Remove message types this agent will no longer publish.
    */
    public void removeAdvertisements(List<MessageType> typeIds)
    {
        // Lock status: OK
        synchronized (advertisements) {
            int count = 0;
            Set<MessageType> newAdvertisements =
                new HashSet<MessageType>(advertisements);
            for (MessageType typeId : typeIds) {
                if (newAdvertisements.contains(typeId)) {
                    newAdvertisements.remove(typeId);
                    count++;
                }
            }

            if (count == 0)
                return;
            advertisements = newAdvertisements;

            sendAdvertisements();
        }
    }

    /** Name used in error log when process publishes a message
        it does not advertise.
    */
    public void setAdvertisementFileName(String fileName)
    {
        advertFileName = fileName;
    }

    private void sendAdvertisements()
    {
        // Lock status: OK, must hold advertisements lock
        AdvertiseMessage request = new AdvertiseMessage(advertisements);

        request.setMessageId(nextMessageId());
        request.setRPC();

        PendingRPC rpc = new PendingRPC();
        rpc.messageId = request.getMsgId();
        rpc.responders = new HashSet<Object>();
        rpc.callback = this;
        synchronized (remoteAgents) {
            rpc.responders.addAll(remoteAgents);
        }

        synchronized (pendingRPC) {
            pendingRPC.put(rpc.messageId, rpc);
        }

        synchronized (rpc) {
            sendMessageToList(request, rpc.responders);
            while (rpc.responders.size() > 0) {
                try {
                    rpc.wait();
                } catch (InterruptedException ignore) { }
            }
        }
        
        synchronized (pendingRPC) {
            pendingRPC.remove(rpc.messageId);
        }
    }

    public void addNoProducersExpected(MessageType messageType)
    {
        synchronized (noProducersExpected) {
            noProducersExpected.add(messageType);
        }
    }

    public Set<MessageType> getNoProducersExpected()
    {
        synchronized (noProducersExpected) {
            return new HashSet<MessageType>(noProducersExpected);
        }
    }

    public static final int DOMAIN_FLAG_TRANSIENT = 1;

    public int getDomainFlags()
    {
        return domainFlags;
    }

    public void setDomainFlags(int flags)
    {
        domainFlags = flags;
    }

    /** Number of retries performed by {@link #connectToDomain connectToDomain()}.
    */
    public int getDomainConnectRetries()
    {
        return domainRetries;
    }

    /** Set the number of retries performed by {@link #connectToDomain connectToDomain()}.  The default is Integer.MAX_VALUE.
    */
    public void setDomainConnectRetries(int retries)
    {
        domainRetries = retries;
    }

    /** Connect to the message domain server.  Should be called before
        creating subscriptions or sending messages.  If successful, the
        agent will accept connections from other agents and begin
        contacting other agents.
    */
    public void connectToDomain(String domainServerHost,
        Integer domainServerPort)
        throws IOException, UnknownHostException, MVRuntimeException
    {
        // Assumes only one thread will call

        if (listener == null && agentName != null) {
            throw new RuntimeException("Call openListener first");
        }

        int retryCount = 0;
        while (true) {
            try {
                domainServerSocket =
                    SocketChannel.open(new InetSocketAddress(
                        domainServerHost, domainServerPort));
                break;
            }
            catch (ConnectException ex) {
                retryCount++;
                if (retryCount > domainRetries) {
                    throw ex;
                }
                Log.debug("Could not connect to domain server "+
                        domainServerHost+":"+domainServerPort+" "+ex+
                        ", Retrying ...");
                try {
                    Thread.sleep(1000);
                } catch (Exception ignore) { }
            }
        }

        domainServerSocket.configureBlocking(false);
        if (Log.loggingDebug)
            Log.debug("MessageAgent: connected to domain server "+
                      domainServerSocket);

        if (agentName == null) {
            addDomainServerAgent(domainServerHost,domainServerPort);
            return;
        }

        // Send agent name, agent listening IP:port
        MVByteBuffer buffer = new MVByteBuffer(64);
        AgentHelloMessage agentHello = new AgentHelloMessage(agentName,
                ":same", getListenerPort());
        agentHello.setFlags(domainFlags);

        Message.toBytes(agentHello,buffer);
        buffer.flip();

        if (! ChannelUtil.writeBuffer(buffer, domainServerSocket)) {
            //## go into reconnect cycle
            throw new RuntimeException("could not connect to domain server");
        }

        // Wait for agent id
        HelloResponseMessage helloResponse = (HelloResponseMessage)
                new DomainClient().readMessage();
        if (helloResponse.getMsgType() != MessageTypes.MSG_TYPE_HELLO_RESPONSE)
            throw new RuntimeException("domain server invalid hello response");

        agentId = helloResponse.getAgentId();
        domainStartTime = helloResponse.getDomainStartTime();
        selfRemoteAgent.agentId = agentId;
        Log.info("My agent-id: "+agentId);

        remoteAgentNames = helloResponse.getAgentNames();

        // Init domain key
        SecureTokenManager.getInstance().initDomain(Base64.decode(helloResponse.getDomainKey()));

        // Make agent info for domain server
        addDomainServerAgent(domainServerHost,domainServerPort);

        startListener();
    }

    void addDomainServerAgent(String domainServerHost,
        Integer domainServerPort)
    {
        domainServerAgent = new RemoteAgent();
        domainServerAgent.agentId = 0;
        domainServerAgent.socket = domainServerSocket;
        domainServerAgent.agentName = "DomainServer";
        domainServerAgent.agentIP = domainServerHost;
        domainServerAgent.agentPort = domainServerPort;
        domainServerAgent.outputBuf = new MVByteBuffer(512);
        domainServerAgent.inputBuf = new MVByteBuffer(512);

        messageIO.addAgent(domainServerAgent);
    }

    public DomainClient getDomainClient()
    {
        return new DomainClient();
    }

    private void addSelfRemoteAgent()
    {
        RemoteAgent remoteAgent = new RemoteAgent();
        remoteAgent.agentId = agentId;
        remoteAgent.socket = null;
        remoteAgent.agentName = agentName;
        remoteAgent.agentIP = null;
        remoteAgent.agentPort = 0;
        remoteAgent.outputBuf = new MVByteBuffer(8192);
        remoteAgent.inputBuf = remoteAgent.outputBuf;
        remoteAgent.flags = 0;

        selfRemoteAgent = remoteAgent;

        remoteAgents.add(remoteAgent);
    }

    /** Wait until all known agents are connected and we have their
        advertisements.  Should be called after {@link #connectToDomain}.
    */
    public void waitForRemoteAgents()
    {
        synchronized (remoteAgents) {
            while (! haveAllAdvertisements())
                try {
                    remoteAgents.wait();
                } catch (InterruptedException ignore) { }
        }
    }

    private boolean haveAllAdvertisements()
    {
        List<String> remotes = new LinkedList<String>(remoteAgentNames);
        for (RemoteAgent remoteAgent : remoteAgents) {
            if ((remoteAgent != selfRemoteAgent && remoteAgent.socket == null)
                        ||
                        ! remoteAgent.hasFlag(RemoteAgent.HAVE_ADVERTISEMENTS))
                return false;
            if (remotes.contains(remoteAgent.agentName)) {
                remotes.remove(remoteAgent.agentName);
            }
            else if (remoteAgent.hasFlag(MessageAgent.DOMAIN_FLAG_TRANSIENT)) {
                remotes.remove(remoteAgent.agentName);
            }
            else {
                Log.error("Unexpected agent '"+remoteAgent.agentName+"'");
                return false;
            }
        }
        if (remotes.size() == 0)
            return true;
        else
            return false;
    }

    // Flags for createSubscription()
    /** No flags for createSubscription() */
    public static final int NO_FLAGS = 0;
    /** Block until subscription is delivered and acknowledged by all
        producing agents. */
    public static final int BLOCKING = 1;
    /** Not implemented. */
    public static final int COMPLETION_CALLBACK = 2;
    /** Not implemented. */
    public static final int DEFERRED = 4;
    /** Subscription is an RPC responder. */
    public static final int RESPONDER = 8;
    /** Return after queueing subscription to producing agents. */
    public static final int NON_BLOCKING = 16;

    /** Set the default subscription flags.  These flags are combined
        (bit-wise OR) with the flags passed to {@link #createSubscription createSubscription}.
        If the default flags includes {@link #NON_BLOCKING} then the
        {@link #BLOCKING} flag is removed.
    */
    public void setDefaultSubscriptionFlags(int flags)
    {
        defaultSubscriptionFlags = flags;
    }

    /** Get the default subscription flags.
    */
    public int getDefaultSubscriptionFlags()
    {
        return defaultSubscriptionFlags;
    }

    /** Subscribe using filter and message callback.  The subscription
        uses the default subscription flags and has no message trigger.
        Matching messages are delivered to the message callback.
        @param filter Messages matching the filter are delivered to the
                callback
        @param callback {@link MessageCallback#handleMessage} is called for
                each matching message.
        @return Subscription id
    */
    public long createSubscription(IFilter filter, MessageCallback callback)
    {
        return createSubscription(filter,callback,NO_FLAGS,null);
    }

    /** Subscribe using filter, flags, and message callback.
        The given
        flags are combined with the default subscription flags (see
        {@link #setDefaultSubscriptionFlags}).
        Matching messages are delivered to the message callback.
        @param filter Messages matching the filter are delivered to the
                callback
        @param callback {@link MessageCallback#handleMessage} is called for
                each matching message.
        @param flags Subscription flags (example: {@link #RESPONDER})
        @return Subscription id
    */
    public long createSubscription(IFilter filter,
                MessageCallback callback, int flags)
    {
        return createSubscription(filter, callback, flags, null);
    }

    /** Subscribe using filter, trigger, flags, and message callback.
        The given
        flags are combined with the default subscription flags (see
        {@link #setDefaultSubscriptionFlags}).
        Matching messages are delivered to the message callback.
        <p>
        The caller should not modify the filter without a matching
        FilterUpdate.
        @param filter Messages matching the filter are delivered to the
                callback.
        @param callback {@link MessageCallback#handleMessage} is called for
                each matching message.
        @param flags Subscription flags (example: {@link #RESPONDER})
        @param trigger Message trigger to run on the producing agent
                each time a message matches the filter.
        @return Subscription id
    */
    public long createSubscription(IFilter filter, MessageCallback callback,
        int flags, MessageTrigger trigger)
    {
        localSubscriptionCreatedCount++;
        flags |= defaultSubscriptionFlags;
        if ((flags & NON_BLOCKING) != 0)
            flags &= (~BLOCKING);

        RegisteredSubscription sub = new RegisteredSubscription();
        sub.filter = filter;
        sub.trigger = trigger;
        sub.flags = (short)(flags & (DEFERRED | RESPONDER));
        sub.producers = new LinkedList<RemoteAgent>();
        sub.callback = callback;

        // Used with BLOCKING flag
        PendingRPC rpc = null;

        synchronized (subscriptions) {
            sub.subId = nextSubId ++;
            subscriptions.put(sub.subId,sub);

            //## support DEFERRED flag

            // Who might publish messages that match this subscription?
            synchronized (remoteAgents) {
                for (RemoteAgent remoteAgent : remoteAgents) {
                    if (filter.matchMessageType(
                                remoteAgent.getAdvertisements())) {
                        sub.producers.add(remoteAgent);
                    }
                }
            }

            if (Log.loggingDebug)
                Log.debug("subscribe "+filter+" matching agents "+
                    sub.producers.size());
            if (sub.producers.size() == 0) {
                String m = "";
                for (MessageType type : filter.getMessageTypes()) {
                    if (! noProducersExpected.contains(type))
                        m += type + ",";
                }
                if (m.length() > 0)
                    Log.error("No producers for types "+m);
            }

            if ((flags & RESPONDER) == 0) {
                FilterTable filterTable = filter.getReceiveFilterTable();
                if (filterTable == null)
                    filterTable = defaultReceiveFilterTable;
                else
                    addUniqueFilterTable(filterTable, receiveFilters);
                filterTable.addFilter(sub,callback);
            }
            else {
                // RPC responder
                FilterTable filterTable = filter.getResponderReceiveFilterTable();
                if (filterTable == null)
                    filterTable = defaultResponderReceiveFilterTable;
                else
                    addUniqueFilterTable(filterTable, responderReceiveFilters);
                filterTable.addFilter(sub,callback);
            }

            // Send subscription to remote agents
            //## if we copy the producers list, then we can enqueue
            //## outside the lock
            if (sub.producers.size() > 0) {
                SubscribeMessage message =
                    new SubscribeMessage(sub.subId,filter,trigger,sub.flags);
                message.setMessageId(nextMessageId());
                if ((flags & BLOCKING) != 0) {
                    rpc = setupInternalRPC(message,sub.producers);
                }

                sendMessageToList(message, sub.producers);
            }

        }

        //## support BLOCKING+COMPLETION_CALLBACK flag

        // BLOCKING==true && COMPLETION_CALLBACK==false
        // An RPC has been created with the callback as 'this'.  Wait
        // for all responses before returning.
        if (rpc != null && (flags & BLOCKING) != 0 &&
                (flags & COMPLETION_CALLBACK) == 0) {
            waitInternalRPC(rpc);
        }

        return sub.subId;
    }

    /** Remove subscription by subscription id.
        @return False if the subscription id does not exist, true otherwise.
    */
    public boolean removeSubscription(long subId)
    {
        // Lock status: OK
        synchronized (subscriptions) {
            return _removeSubscription(subId);
        }
    }

    /** Remove subscriptions by subscription id.
        @return False if any of the subscription id do not exist, true
                otherwise.
    */
    public boolean removeSubscriptions(Collection<Long> subIds)
    {
        // Lock status: OK
        //## Could be optimized to create a single Unsubscribe for each
        //## remote agent.
        synchronized (subscriptions) {
            int count = 0;
            for (Long subId : subIds)
                if (_removeSubscription(subId))
                    count++;
            return count == subIds.size();
        }
    }

    private boolean _removeSubscription(Long subId)
    {
        // Lock status: OK requires subscriptions

        RegisteredSubscription sub = subscriptions.remove(subId);
        if (sub == null)
            return false;

        localSubscriptionRemovedCount++;
        if ((sub.flags & RESPONDER) == 0) {
            FilterTable filterTable = sub.filter.getReceiveFilterTable();
            if (filterTable == null)
                filterTable = defaultReceiveFilterTable;
            filterTable.removeFilter(sub,sub.callback);
        }
        else {
            FilterTable filterTable = sub.filter.getResponderReceiveFilterTable();
            if (filterTable == null)
                filterTable = defaultResponderReceiveFilterTable;
            filterTable.removeFilter(sub,sub.callback);
        }

        UnsubscribeMessage message = new UnsubscribeMessage(subId);
        message.setMessageId(nextMessageId());
        sendMessageToList(message, sub.producers);
        return true;
    }

    /** Check if a {@link MessageCallback#handleMessage handleMessage} flag indicates a response is
        expected.  Useful for messages that may be published with
        or without RPC.
        @return True if a response is expected from the callback.
    */
    public static boolean responseExpected(int flags) {
        return (flags & MessageCallback.RESPONSE_EXPECTED) != 0;
    }

/*
    public int addMessageTrigger(long subId, MessageTrigger trigger)
    {
        return 0;
    }

    public boolean removeMessageTrigger(long subId, int triggerId)
    {
        return false;
    }
*/

    /** Update a subscription filter.  The filter update is forwarded
        to all subscription producers.  The local instance of the filter
        is not modified.
        @param subId Subscription to update
        @param update Filter update instructions
        @return False if the subscription does not exist, true otherwise.
    */
    public boolean applyFilterUpdate(long subId, FilterUpdate update)
    {
        return applyFilterUpdate(subId, update, NO_FLAGS, (RemoteAgent)null);
    }

    /** Update a subscription filter.  The filter update is forwarded
        to all subscription producers.  The local instance of the filter
        is not modified.
        @param subId Subscription to update
        @param update Filter update instructions
        @return False if the subscription does not exist, true otherwise.
    */
    public boolean applyFilterUpdate(long subId, FilterUpdate update,
            int flags)
    {
        return applyFilterUpdate(subId, update, flags, (RemoteAgent)null);
    }

    /** Update a subscription filter.  The filter update is forwarded
        to all subscription producers, except the sender of {@code excludeSender}.  The local instance of the filter
        is not modified.
        @param subId Subscription to update
        @param update Filter update instructions
        @param excludeSender Do not send update to the sender of this message.
        @return False if the subscription does not exist, true otherwise.
    */
    public boolean applyFilterUpdate(long subId, FilterUpdate update,
        int flags, Message excludeSender)
    {
        return applyFilterUpdate(subId, update, flags,
                excludeSender.remoteAgent);
    }

    protected boolean applyFilterUpdate(long subId, FilterUpdate update,
        int flags, RemoteAgent excludeAgent)
    {
        // Lock status: OK
        PendingRPC rpc = null;
        synchronized (subscriptions) {
            localFilterUpdateCount++;
            RegisteredSubscription sub = subscriptions.get(subId);
            if (sub == null)
                return false;

            FilterUpdateMessage message =
                        new FilterUpdateMessage(subId, update);
            message.setMessageId(nextMessageId());

            List<RemoteAgent> producers = sub.producers;
            if (excludeAgent != null) {
                producers = new ArrayList<RemoteAgent>(producers);
                producers.remove(excludeAgent);
            }
            if (producers.size() > 0) {
                if ((flags & BLOCKING) != 0)
                    rpc = setupInternalRPC(message,producers);
                sendMessageToList(message, producers);
            }
        }

        if (rpc != null) {
            waitInternalRPC(rpc);
        }

        return true;
    }

    /** Publish message to subscribing agents.
        {@link MessageTrigger MessageTriggers} are run prior to
        queueing the message.  Returns after the message is queued.
        @return Number of subscribing agents.
    */
    public int sendBroadcast(Message message)
    {
        message.setMessageId(nextMessageId());
        message.unsetRPC();
        return _sendBroadcast(message);
    }

    private int _sendBroadcast(Message message)
    {
        // Lock status: OK
        if (Log.loggingDebug)
            Log.debug("sendBroadcast type="+message.getMsgType().getMsgTypeString()+
                " id="+message.getMsgId()+
                " class="+message.getClass().getName());

        if (! advertisements.contains(message.getMsgType()))
            Log.error("NEED ADVERT - Add "+message.getMsgType()+" to "+
                advertFileName + " and restart server");

        Set<Object> matchingAgents =
                new HashSet<Object>(remoteAgents.size());
        List<Subscription> triggers = new LinkedList<Subscription>();
        for (FilterTable filterTable : sendFilters) {
            filterTable.match(message,matchingAgents,triggers);
        }

        // Never hold locks when calling triggers
        for (Subscription triggerSub : triggers) {
            triggerSub.getTrigger().trigger(message, triggerSub.filter, this);
        }

//System.out.println("Message matched "+matchingAgents.size()+" agents");

        sendMessageToList(message, matchingAgents);

        return matchingAgents.size();
    }

    /** Send message directly to an agent.  The agent is identified by
        an AgentHandle.  AgentHandles are passed to Filter.applyFilterUpdate()
        to identify the sender of the filter update.  Likewise, the
        SubscriptionHandle is passed to Filter.applyFilterUpdate().
        @param message The message.
        @param destination The agent.
        @param runTriggers Subscription from which to run triggers prior
            to sending the message.
        @return True on success, false on failure.
    */
    public boolean sendDirect(Message message, AgentHandle destination,
        SubscriptionHandle runTriggers)
    {
        if (! (destination instanceof RemoteAgent))
            return false;
        if (! remoteAgents.contains(destination))
            return false;

        message.setMessageId(nextMessageId());
        message.unsetRPC();

        if (runTriggers != null && runTriggers instanceof RemoteSubscription) {
            RemoteSubscription remoteSub = (RemoteSubscription)runTriggers;
            if (remoteSub.getTrigger() != null)
                remoteSub.getTrigger().trigger(message,remoteSub.filter,this);
        }

        Collection agents = new ArrayList(1);
        agents.add(destination);
        sendMessageToList(message, agents);
        return true;
    }

    /** Send a remote procedure call (RPC) message and wait for response.
        @return Response message.
        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
        @throws RPCException If the remote RPC handler threw an exception
            while handling the message.
    */
    public Message sendRPC(Message message)
    {
        PendingRPC rpc = _sendRPC(message, this);

        SQThreadPool pool = SQThreadPool.getRunningPool();
        if (pool != null)
            pool.runningThreadWillBlock();

        synchronized (rpc) {
            while (rpc.response == null) {
                try {
                    rpc.wait();
                } catch (InterruptedException ex) {
                }
            }
        }

        synchronized (pendingRPC) {
            pendingRPC.remove(rpc.messageId);
        }

        if (pool != null)
            pool.doneBlocking();

        if (rpc.response instanceof ExceptionResponseMessage) {
            throw new RPCException(((ExceptionResponseMessage)rpc.response).getException());
        }

        return rpc.response;
    }

    /** Send a remote procedure call (RPC) message and wait for boolean
        response.  The recipient must respond with
        {@link #sendBooleanResponse sendBooleanResponse} or a
        {@link BooleanResponseMessage}.

        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
        @throws RPCException If the remote RPC handler threw an exception
            while handling the message.
    */
    public Boolean sendRPCReturnBoolean(Message message)
    {
        BooleanResponseMessage response = (BooleanResponseMessage)sendRPC(message);
        return response.getBooleanVal();
    }

    /** Send a remote procedure call (RPC) message and wait for integer
        response.  The recipient must respond with
        {@link #sendIntegerResponse sendIntegerResponse} or a
        {@link IntegerResponseMessage}.

        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
        @throws RPCException If the remote RPC handler threw an exception
            while handling the message.
    */
    public Integer sendRPCReturnInt(Message message)
    {
        IntegerResponseMessage response = (IntegerResponseMessage)sendRPC(message);
        return response.getIntVal();
    }

    /** Send a remote procedure call (RPC) message and wait for long
        response.  The recipient must respond with
        {@link #sendLongResponse sendLongResponse} or a
        {@link LongResponseMessage}.

        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
        @throws RPCException If the remote RPC handler threw an exception
            while handling the message.
    */
    public Long sendRPCReturnLong(Message message)
    {
        LongResponseMessage response = (LongResponseMessage)sendRPC(message);
        return response.getLongVal();
    }

    /** Send a remote procedure call (RPC) message and wait for string
        response.  The recipient must respond with
        {@link #sendStringResponse sendStringResponse} or a
        {@link StringResponseMessage}.

        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
        @throws RPCException If the remote RPC handler threw an exception
            while handling the message.
    */
    public String sendRPCReturnString(Message message)
    {
        StringResponseMessage response = (StringResponseMessage)sendRPC(message);
        return response.getStringVal();
    }

    /** Send a remote procedure call (RPC) message and wait for object
        response.  The recipient must respond with
        {@link #sendObjectResponse sendObjectResponse} or a
        {@link GenericResponseMessage}.

        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
        @throws RPCException If the remote RPC handler threw an exception
            while handling the message.
    */
    public Object sendRPCReturnObject(Message message)
    {
        GenericResponseMessage response = (GenericResponseMessage)sendRPC(message);
        return response.getData();
    }
    
    /** Send a remote procedure call (RPC) message and invoke callback
        when the response arrives.  Returns after the message is
        queued to the producing agent.
        <p>
        If the RPC handler throws an exception, the callback will
        receive an {@link ExceptionResponseMessage} instead of the expected
        message.

        @param message RPC message.
        @param callback {@link ResponseCallback#handleResponse} is called
                with the response message.
        @throws NoRecipientsException If there are no message subscribers
        @throws MultipleRecipientsException If there is more than one message subscriber.
    */
    public void sendRPC(Message message, ResponseCallback callback)
    {
        _sendRPC(message,callback);
    }

    private PendingRPC _sendRPC(Message message, ResponseCallback callback)
    {
//long start = System.nanoTime();

        // Lock status: OK
        message.setMessageId(nextMessageId());
        message.setRPC();

        if (Log.loggingDebug)
            Log.debug("sendRPC type="+message.getMsgType().getMsgTypeString()+
                " id="+message.getMsgId()+
                " class="+message.getClass().getName());

        if (! advertisements.contains(message.getMsgType()))
            Log.error("NEED ADVERT - Add "+message.getMsgType()+" to "+
                advertFileName + " and restart server");

//## need timeout on async RPC
        PendingRPC rpc = new PendingRPC();
        rpc.messageId = message.getMsgId();
        rpc.responders = null;
        rpc.callback = callback;

        synchronized (pendingRPC) {
            pendingRPC.put(rpc.messageId, rpc);
        }

        HashSet<Object> matchingAgents = new HashSet<Object>();
        List<Subscription> triggers = new LinkedList<Subscription>();
        for (FilterTable filterTable : responderSendFilters) {
            filterTable.match(message, matchingAgents, triggers);
        }

        if (matchingAgents.size() == 0) {
            synchronized (pendingRPC) {
                pendingRPC.remove(rpc.messageId);
            }
            throw new NoRecipientsException("sendRPC: no message recipients for type=" +
                message.getMsgType() + " " + message +
                ", class " + message.getClass().getName());
        }
        if (matchingAgents.size() != 1) {
            synchronized (pendingRPC) {
                pendingRPC.remove(rpc.messageId);
            }
            throw new MultipleRecipientsException("sendRPC: multiple message recipients for type="+
                message.getMsgType() + " " + message +
                ", class " + message.getClass().getName());
        }

        rpc.responders = matchingAgents;

        // Never hold locks when calling triggers
        for (Subscription triggerSub : triggers) {
            triggerSub.getTrigger().trigger(message, triggerSub.filter, this);
        }


//long start = System.nanoTime();
        // Send the RPC message
        synchronized (rpc) {
            sendMessageToList(message, matchingAgents);
        }

//            long stop = System.nanoTime();
//            Log.info("RPC "+message.getMsgType()+
//                        " time "+(stop-start)/1000 + " us");

        return rpc;
    }

    private void sendMessageToList(Message message, Collection agents)
    {
        int count = 0;
        for (Object agent : agents) {
            AgentInfo remoteAgent = (AgentInfo)agent;
            if (Log.loggingDebug)
                Log.debug("Sending "+message.getMsgType().getMsgTypeString() +
                " id="+message.getMsgId()+" to "+remoteAgent.agentName);
            synchronized (remoteAgent.outputBuf) {
                if (remoteAgent.socket != null) {
                    Message.toBytes(message, remoteAgent.outputBuf);
                    count++;
                }
                else if (remoteAgent == selfRemoteAgent) {
                    Message.toBytes(message, remoteAgent.outputBuf);
                    remoteAgent.outputBuf.notify();
                }
            }
        }
        if (count > 0)
            messageIO.outputReady();
    }

    PendingRPC setupInternalRPC(Message message, List producers)
    {
        message.setRPC();
        PendingRPC rpc = new PendingRPC();
        rpc.messageId = message.getMsgId();
        rpc.responders = new HashSet<Object>();
        rpc.responders.addAll(producers);
//        if ((flags & COMPLETION_CALLBACK) != 0)
//            ; //##rpc.callback = callback;
//        else
            rpc.callback = this;

        synchronized (pendingRPC) {
            pendingRPC.put(rpc.messageId, rpc);
        }
        return rpc;
    }

    void waitInternalRPC(PendingRPC rpc)
    {
        SQThreadPool pool = SQThreadPool.getRunningPool();
        if (pool != null)
            pool.runningThreadWillBlock();

        synchronized (rpc) {
            while (rpc.responders.size() > 0) {
                try {
                    rpc.wait();
                } catch (InterruptedException ignore) { }
            }
        }

        synchronized (pendingRPC) {
            pendingRPC.remove(rpc.messageId);
        }

        if (pool != null)
            pool.doneBlocking();
    }

    /** Broadcast a remote procedure call (RPC) message and invoke callback
        when responses arrive.  Returns after the message is
        queued to the producing agents.
        <p>
        If the RPC handler throws an exception, the callback will
        receive an {@link ExceptionResponseMessage} instead of the expected
        message.

        @param callback {@link ResponseCallback#handleResponse} is called
                for each response message.
        @return Number of subscribing agents.
    */
    public int sendBroadcastRPC(Message message, ResponseCallback callback)
    {
        // Lock status: OK
        message.setMessageId(nextMessageId());
        message.setRPC();

        if (Log.loggingDebug)
            Log.debug("sendBroadcastRPC type="+message.getMsgType().getMsgTypeString()+
                " id="+message.getMsgId()+
                " class="+message.getClass().getName());

//## need timeout on async RPC

        if (! advertisements.contains(message.getMsgType()))
            Log.error("NEED ADVERT - Add "+message.getMsgType()+" to "+
                advertFileName + " and restart server");

        PendingRPC rpc = new PendingRPC();
        rpc.messageId = message.getMsgId();
        rpc.responders = new HashSet<Object>();
        rpc.callback = callback;

        synchronized (pendingRPC) {
            pendingRPC.put(rpc.messageId, rpc);
        }

        List<Subscription> triggers = new LinkedList<Subscription>();
        for (FilterTable filterTable : responderSendFilters) {
            filterTable.match(message, rpc.responders, triggers);
        }

        // Never hold locks when calling triggers
        for (Subscription triggerSub : triggers) {
            triggerSub.getTrigger().trigger(message, triggerSub.filter, this);
        }

        // Send the RPC message
        int responderCount = rpc.responders.size();
        synchronized (rpc) {
            sendMessageToList(message, rpc.responders);
        }

        if (responderCount == 0) {
            synchronized (pendingRPC) {
                pendingRPC.remove(rpc.messageId);
            }
        }

        return responderCount;
    }

    /** Respond to an RPC message.  Typically called within a message
        callback.
    */
    public void sendResponse(ResponseMessage message)
    {
        message.setMessageId(nextMessageId());

        if (Log.loggingDebug)
            Log.debug("sendResponse to "+message.getRequestingAgent().agentName+
                ","+message.getRequestId()+
                " type="+message.getMsgType().getMsgTypeString()+
                " id="+message.getMsgId()+
                " class="+message.getClass().getName());

        synchronized (message.getRequestingAgent().outputBuf) {
            Message.toBytes(message, message.getRequestingAgent().outputBuf);
            if (message.getRequestingAgent() == selfRemoteAgent)
                selfRemoteAgent.outputBuf.notify();
        }

        if (message.getRequestingAgent() != selfRemoteAgent)
            messageIO.outputReady();
    }

    /** Boolean response to an RPC message.  Sends a {@link BooleanResponseMessage}.
        @param message The request message.
        @param booleanVal Boolean response.
    */
    public void sendBooleanResponse(Message message, Boolean booleanVal)
    {
	if (! message.isRPC())
	    return;
        BooleanResponseMessage response = new BooleanResponseMessage(message, booleanVal);
        sendResponse(response);
    }

    /** Integer response to an RPC message.  Sends a {@link IntegerResponseMessage}.
        @param message The request message.
        @param intVal Integer response.
    */
    public void sendIntegerResponse(Message message, Integer intVal)
    {
	if (! message.isRPC())
	    return;
        IntegerResponseMessage response = new IntegerResponseMessage(message, intVal);
        sendResponse(response);
    }

    /** Long response to an RPC message.  Sends a {@link LongResponseMessage}.
        @param message The request message.
        @param longVal Long response.
    */
    public void sendLongResponse(Message message, Long longVal)
    {
	if (! message.isRPC())
	    return;
        LongResponseMessage response = new LongResponseMessage(message, longVal);
        sendResponse(response);
    }

    /** String response to an RPC message.  Sends a {@link StringResponseMessage}.
        @param message The request message.
        @param stringVal String response.
    */
    public void sendStringResponse(Message message, String stringVal)
    {
	if (! message.isRPC())
	    return;
        StringResponseMessage response = new StringResponseMessage(message, stringVal);
        sendResponse(response);
    }

    /** Object response to an RPC message.  Sends a {@link GenericResponseMessage}.
        @param message The request message.
        @param object Object response.
    */
    public void sendObjectResponse(Message message, Object object)
    {
	if (! message.isRPC())
	    return;
        GenericResponseMessage response = new GenericResponseMessage(message, object);
        sendResponse(response);
    }
    
    /** Get the response handling thread pool.  Defaults to a fixed
        pool of 10 threads.
      */
    public ExecutorService getResponseThreadPool()
    {
        return responseCallbackPool;
    }

    /** Set the response handling thread pool.
      */
    public void setResponseThreadPool(ExecutorService threadPool)
    {
        responseCallbackPool = threadPool;
    }

    public void handleResponse(ResponseMessage message)
    {
        // This function intentially left blank
        // See other handleResponse() function for response message handling
    }

    /** Get the number of application messages received by this agent.
    */
    public long getAppMessageCount() {
        return statAppMessageCount;
    }

    /** Get the number of system messages received by this agent.
    */
    public long getSystemMessageCount() {
        return statSystemMessageCount;
    }


    public void startStatsThread()
    {
        Thread messageAgentStatsLogger =
            new Thread(new MessageAgentStatsLogger(), "Stats:MessageAgent");
        messageAgentStatsLogger.setDaemon(true);
        messageAgentStatsLogger.start();
    }

    // TcpServer callback
    public void onTcpAccept(SocketChannel agentSocket) {
        try {
            agentSocket.socket().setTcpNoDelay(true);
            threadPool.execute(new NewConnectionHandler(agentSocket));
        } catch (IOException ex) {
            Log.exception("Agent listener", ex);
        }
    }

    // Internal methods

    private class NewConnectionHandler implements Runnable {
        public NewConnectionHandler(SocketChannel socket)
                throws IOException
        {
            agentSocket = socket;
        }

        public void run()
        {
            try {
                RemoteAgent remoteAgent = waitForAgent(agentSocket);
                if (remoteAgent == null) {
                    agentSocket.close();
                }
                else {
                    // Send our agent state message
                    //## need to get local IP address
                    AgentStateMessage agentState;
                    agentState = new AgentStateMessage(agentId,
                        agentName, ":same", agentPort, domainFlags);
                    Message.toBytes(agentState, remoteAgent.outputBuf);

                    sendAdvertisements(remoteAgent);

                    if (Log.loggingDebug)
                        Log.debug("received connect: Accepting connection from "+
                            remoteAgent.agentName);
                    messageIO.addAgent(remoteAgent);
                }

            } catch (Exception ex) {
                Log.exception("NewConnectionHandler", ex);
                try { agentSocket.close(); }
                catch (Exception ignore) { }
            }
        }

        SocketChannel agentSocket;
    }

    // Wait for remote agent to respond during connection establishment.
    private RemoteAgent waitForAgent(SocketChannel agentSocket)
        throws IOException, MVRuntimeException
    {
        ByteBuffer buf = ByteBuffer.allocate(4);
        int nBytes = ChannelUtil.fillBuffer(buf,agentSocket);
        if (nBytes < 4)  {
            Log.error("Agent: invalid agent hello bytes="+nBytes);
            return null;
        }

        int msgLen = buf.getInt();
        if (msgLen < 0)
            return null;

        MVByteBuffer buffer = new MVByteBuffer(msgLen);
        nBytes = ChannelUtil.fillBuffer(buffer.getNioBuf(),agentSocket);
        if (nBytes < msgLen)  {
            Log.error("Agent: invalid agent state, expecting "+
                msgLen + " got "+nBytes);
            return null;
        }

        //## check message type
        AgentStateMessage agentState = (AgentStateMessage)MarshallingRuntime.unmarshalObject(buffer);

        RemoteAgent remoteAgent;
        synchronized (remoteAgents) {
            if (agentState.agentName == null ||
                    agentState.agentName.equals("")) {
                Log.error("Missing remote agent name");
                return null;
            }
            if (getAgent(agentState.agentName) != null) {
                Log.error("Already connected to '"+agentState.agentName+"'");
                return null;
            }

            if (getAgent(agentState.agentId) != null) {
                Log.error("Already connected to '"+agentState.agentName+
                        "' agentId "+agentState.agentId);
                return null;
            }

            if (agentState.agentIP == null ||
                    agentState.agentIP.equals("")) {
                Log.error("Missing remote agent IP address");
                return null;
            }

            if (agentState.agentIP.equals(":same")) {
                InetAddress agentAddress = agentSocket.socket().getInetAddress();
                agentState.agentIP = agentAddress.getHostAddress();
            }

            remoteAgent = new RemoteAgent();
            remoteAgent.agentId = agentState.agentId;
            remoteAgent.socket = agentSocket;
            remoteAgent.agentName = agentState.agentName;
            remoteAgent.agentIP = agentState.agentIP;
            remoteAgent.agentPort = agentState.agentPort;
            remoteAgent.outputBuf = new MVByteBuffer(8192);
            remoteAgent.inputBuf = new MVByteBuffer(8192);
            remoteAgent.flags = agentState.domainFlags;

            // Add to our data struct
            remoteAgents.add(remoteAgent);
        }

        return remoteAgent;
    }

    protected class RemoteAgent extends AgentInfo
    {
        boolean isSelf()
        {
            return this == MessageAgent.this.selfRemoteAgent;
        }

        void addRemoteSubscription(RemoteSubscription remoteSub)
        {
            synchronized (remoteSubs) {
                if (remoteSubs.get(remoteSub.subId) != null) {
                    Log.error("RemoteAgent "+agentName+
                        ": Duplicate subId "+remoteSub.subId);
                    return;
                }
                remoteSubs.put(remoteSub.subId, remoteSub);
            }
        }

        RemoteSubscription removeRemoteSubscription(Long subId)
        {
            synchronized (remoteSubs) {
                return remoteSubs.remove(subId);
            }
        }

        RemoteSubscription getRemoteSubscription(Long subId)
        {
            synchronized (remoteSubs) {
                return remoteSubs.get(subId);
            }
        }

        Collection<MessageType> getAdvertisements()
        {
            return remoteAdverts;
        }

        void setAdvertisements(Collection<MessageType> adverts)
        {
            remoteAdverts.clear();
            remoteAdverts.addAll(adverts);
        }

        int getFlags()
        {
            return flags;
        }

        boolean hasFlag(int flag)
        {
            return (flags & flag) != 0;
        }

        void setFlag(int flag)
        {
            flags |= flag;
        }

        // subId to remotesub
        Map<Long,RemoteSubscription> remoteSubs =
                new HashMap<Long,RemoteSubscription>();
        Collection<MessageType> remoteAdverts = new HashSet<MessageType>();
        List<Message> outgoingQueue;

        static final int HAVE_ADVERTISEMENTS = 256;
    }

    class RemoteSubscription extends Subscription {
        public Object getAssociation()
        {
            return remoteAgent;
        }

        RemoteAgent remoteAgent;
    }

    public void handleMessageData(int length, MVByteBuffer messageData,
                AgentInfo agentInfo) {
        // MessageIO calls this sequentially, so the locking requirements
        // are simplified.

        if (length == -1 || messageData == null) {
            if (agentInfo.socket == domainServerSocket) {
                Log.error("Lost connection to domain server, exiting");
                System.exit(1);
                return;
            }
            if ((agentInfo.flags & MessageAgent.DOMAIN_FLAG_TRANSIENT) != 0) {
                Log.debug("Lost connection to agent '" +
                    agentInfo.agentName+"' (transient) " +
                    agentInfo.socket);
                messageIO.removeAgent(agentInfo);
                try {
                    agentInfo.socket.close();
                } catch (java.io.IOException e) { /* ignore */ }
                agentInfo.socket = null;
                synchronized (remoteAgents) {
                    remoteAgents.remove(agentInfo);
                }
                remoteAgentOutput.removeKey((RemoteAgent)agentInfo);
            }
            else {
                Log.error("Lost connection to agent '"+agentInfo.agentName+"' "+
                    agentInfo.socket);
                synchronized (agentInfo.outputBuf) {
                    agentInfo.socket = null;
                    agentInfo.outputBuf = new MVByteBuffer(8192);
                    agentInfo.inputBuf = new MVByteBuffer(8192);
                }
                //## start reconnect
            }
            return;
        }

//long start = System.nanoTime();
        Message message = (Message)MarshallingRuntime.unmarshalObject(messageData);
//            long stop = System.nanoTime();
//            Log.info("RPC "+message.getMsgType()+
//                        " time "+(stop-start)/1000 + " us");
        MessageType msgType = message.getMsgType();

        if (Log.loggingDebug) {
            String responseTo="";
            if (message instanceof ResponseMessage)
                responseTo = " responseTo="+((ResponseMessage)message).getRequestId();
            Log.debug("handleMessageData from " + agentInfo.agentName+"," +
                    message.getMsgId() + responseTo +
                    " type=" + msgType.getMsgTypeString() +
                    " len=" + length +
                    " class=" + message.getClass().getName());
        }

        if (! MessageTypes.isInternal(msgType)) {
            statAppMessageCount ++;
            message.remoteAgent = (RemoteAgent)agentInfo;
            if (message instanceof ResponseMessage)
                handleResponse((ResponseMessage)message,(RemoteAgent)agentInfo);
            else
                deliverMessage(message);
            return;
        }

        statSystemMessageCount ++;
        if (msgType == MessageTypes.MSG_TYPE_SUBSCRIBE) {
            remoteSubscriptionCreatedCount++;
            message.remoteAgent = (RemoteAgent)agentInfo;
            handleSubscribe((SubscribeMessage)message,(RemoteAgent)agentInfo);
        }
        else if (msgType == MessageTypes.MSG_TYPE_UNSUBSCRIBE) {
            remoteSubscriptionRemovedCount++;
            message.remoteAgent = (RemoteAgent)agentInfo;
            handleUnsubscribe((UnsubscribeMessage)message,(RemoteAgent)agentInfo);
        }
        else if (msgType == MessageTypes.MSG_TYPE_FILTER_UPDATE) {
            remoteFilterUpdateCount++;
            message.remoteAgent = (RemoteAgent)agentInfo;
            handleFilterUpdate((FilterUpdateMessage)message,(RemoteAgent)agentInfo);
        }
        else if (msgType == MessageTypes.MSG_TYPE_ADVERTISE) {
            message.remoteAgent = (RemoteAgent)agentInfo;
            handleAdvertise((AdvertiseMessage)message,(RemoteAgent)agentInfo);
        }
        else if (msgType == MessageTypes.MSG_TYPE_NEW_AGENT) {
            handleNewAgentMessage((NewAgentMessage)message);
        }
        else {
            Log.error("handleMessageData: unknown message type "+msgType);
            System.out.println("Unknown message type "+msgType);
        }
    }

    private void handleSelfMessage(Message message)
    {
        if (Log.loggingDebug) {
            String responseTo="";
            if (message instanceof ResponseMessage)
                responseTo = " responseTo="+((ResponseMessage)message).getRequestId();
            Log.debug("handleSelfMessage id="+
                    message.getMsgId() + responseTo +
                    " type="+message.getMsgType().getMsgTypeString()+
                    " class="+message.getClass().getName());
        }

        int msgTypeNumber = message.getMsgType().getMsgTypeNumber();
        if (message instanceof ResponseMessage)
            handleResponse((ResponseMessage)message, selfRemoteAgent);
        else if (msgTypeNumber < MessageTypes.catalog.getFirstMsgNumber() ||
                msgTypeNumber > MessageTypes.catalog.getLastMsgNumber()) {
            deliverMessage(message);
        }
        else {
            statSystemMessageCount ++;
            MessageType msgType = message.getMsgType();
            if (msgType == MessageTypes.MSG_TYPE_ADVERTISE) {
                handleAdvertise((AdvertiseMessage)message,selfRemoteAgent);
            }
            else if (msgType == MessageTypes.MSG_TYPE_SUBSCRIBE) {
                handleSubscribe((SubscribeMessage)message,selfRemoteAgent);
            }
            else if (msgType == MessageTypes.MSG_TYPE_UNSUBSCRIBE) {
                handleUnsubscribe((UnsubscribeMessage)message,selfRemoteAgent);
            }
            else if (msgType == MessageTypes.MSG_TYPE_FILTER_UPDATE) {
                handleFilterUpdate((FilterUpdateMessage)message,selfRemoteAgent);
            }
            else {
                Log.error("Unknown message type "+message.getMsgType());
            }
        }
    }

    // Locking for the handle*() methods is simplified because only
    // one or two are running at a time.  These can be called from
    // the "MessageIO" thread or the "Marshaller" thread.

    private void handleNewAgentMessage(NewAgentMessage message)
    {
        // Lock status: OK
        // Domain server is telling us about an agent

        // Start thread to connect to remote agent if our name is
        // greater than their name.  This ensures only one connection
        // between any two agents.
        if (agentName.compareTo(message.agentName) > 0)
            threadPool.execute(new AgentConnector(message));
        else if (agentName.equals(message.agentName)) {
            Log.error("Duplicate agent name '"+agentName+"', exiting");
            System.exit(1);
        }
    }

    private void handleAdvertise(AdvertiseMessage message,
            RemoteAgent remoteAgent)
    {
        // Lock status: OK
        synchronized (advertisements) {
            Collection<MessageType> newAdverts = message.getAdvertisements();
            Collection<MessageType> oldAdverts = remoteAgent.getAdvertisements();

            List<MessageType> addAdverts = new LinkedList<MessageType>();
            List<MessageType> removeAdverts = new LinkedList<MessageType>();

            for (MessageType ii : newAdverts) {
                if (! oldAdverts.contains(ii))
                    addAdverts.add(ii);
            }

            for (MessageType ii : oldAdverts) {
                if (! newAdverts.contains(ii))
                    removeAdverts.add(ii);
            }

            if (Log.loggingDebug)
                Log.debug("["+remoteAgent.agentName +","+message.getMsgId()+
                    "] handleAdvertise: Adding "+addAdverts.size()+
                    " and removing "+removeAdverts.size());

            if (message.isRPC()) {
                ResponseMessage response = new ResponseMessage(message);
                sendResponse(response);
            }

            synchronized (subscriptions) {
                Collection<RegisteredSubscription> mySubs = subscriptions.values();
                boolean self = false;
                boolean remote = false;
                for (RegisteredSubscription sub : mySubs) {
                    if (sub.filter.matchMessageType(addAdverts)) {
                        if (! sub.producers.contains(remoteAgent)) {
                            sub.producers.add(remoteAgent);
                            SubscribeMessage subscribeMsg =
                                new SubscribeMessage(sub.subId,sub.filter,
                                sub.trigger,sub.flags);
                            subscribeMsg.setMessageId(nextMessageId());
                            if (Log.loggingDebug)
                                Log.debug("Sending "+
                                    subscribeMsg.getMsgType().getMsgTypeString() +
                                    " id="+subscribeMsg.getMsgId()+
                                    " to "+remoteAgent.agentName);
                            synchronized (remoteAgent.outputBuf) {
                                Message.toBytes(subscribeMsg,
                                        remoteAgent.outputBuf);
                            }
                            if (remoteAgent == selfRemoteAgent)
                                self = true;
                            else
                                remote = true;
                        }
                    }
                }
                if (self)
                    selfRemoteAgent.outputBuf.notify();
                if (remote)
                    messageIO.outputReady();
            }

            remoteAgent.setAdvertisements(newAdverts);
        }

        synchronized (remoteAgents) {
            if (! remoteAgent.hasFlag(RemoteAgent.HAVE_ADVERTISEMENTS)) {
                remoteAgent.setFlag(RemoteAgent.HAVE_ADVERTISEMENTS);
                remoteAgents.notify();
            }
        }
    }

    private void handleSubscribe(SubscribeMessage message,
            RemoteAgent remoteAgent)
    {
        // Lock status: OK
        if (Log.loggingDebug)
            Log.debug("["+remoteAgent.agentName + "," + message.getMsgId()+
                    "] Got subscription subId="+message.getSubId()+" filter "+
                    message.getFilter());

        RemoteSubscription remoteSub = new RemoteSubscription();
        remoteSub.remoteAgent = remoteAgent;
        remoteSub.subId = message.getSubId();
        remoteSub.filter = message.getFilter();
        remoteSub.trigger = message.getTrigger();
        remoteSub.flags = message.getFlags();

        if (remoteSub.trigger != null)
            remoteSub.trigger.setFilter(remoteSub.filter);

        remoteAgent.addRemoteSubscription(remoteSub);

        FilterTable filterTable;
        if ((remoteSub.flags & RESPONDER) == 0) {
            filterTable = remoteSub.filter.getSendFilterTable();
            if (filterTable == null)
                filterTable = defaultSendFilterTable;
            else
                addUniqueFilterTable(filterTable,sendFilters);
        }
        else {
            filterTable = remoteSub.filter.getResponderSendFilterTable();
            if (filterTable == null)
                filterTable = defaultResponderSendFilterTable;
            else
                addUniqueFilterTable(filterTable,responderSendFilters);
        }

        filterTable.addFilter(remoteSub, remoteAgent);

        if (message.isRPC()) {
            ResponseMessage response = new ResponseMessage(message);
            sendResponse(response);
        }
    }

    private void handleUnsubscribe(UnsubscribeMessage message,
            RemoteAgent remoteAgent)
    {
        // Lock status: OK
        List<Long> subIds = message.getSubIds();
        if (Log.loggingDebug)
            Log.debug("["+remoteAgent.agentName +","+ message.getMsgId()+
                "] Got unsubscribe count="+subIds.size());

        for (Long subId : subIds) {
            RemoteSubscription remoteSub =
                remoteAgent.removeRemoteSubscription(subId);
            if (remoteSub == null) {
                Log.error("MessageAgent: duplicate remove sub");
                continue;
            }
            if ((remoteSub.flags & RESPONDER) == 0) {
                FilterTable filterTable = remoteSub.filter.getSendFilterTable();
                if (filterTable == null)
                    filterTable = defaultSendFilterTable;
                filterTable.removeFilter(remoteSub, remoteAgent);
            }
            else {
                FilterTable filterTable =
                        remoteSub.filter.getResponderSendFilterTable();
                if (filterTable == null)
                    filterTable = defaultResponderSendFilterTable;
                filterTable.removeFilter(remoteSub, remoteAgent);
            }
        }
    }

    private void handleFilterUpdate(FilterUpdateMessage message,
            RemoteAgent remoteAgent)
    {
        // Lock status: OK

        remoteFilterUpdateCount++;
        
        if (Log.loggingDebug)
            Log.debug("["+remoteAgent.agentName +","+message.getMsgId()+
                "] Got filter update subId="+message.getSubId() +
                " rpc=" + message.isRPC());

        RemoteSubscription remoteSub =
                remoteAgent.getRemoteSubscription(message.getSubId());
        if (remoteSub == null) {
            Log.error("handleFilterUpdate: unknown subId="+message.getSubId());
            return;
        }
        remoteSub.filter.applyFilterUpdate(message.getFilterUpdate(),
            remoteAgent, remoteSub);

        if (message.isRPC()) {
            ResponseMessage response = new ResponseMessage(message);
            sendResponse(response);
        }
    }

    private void handleResponse(ResponseMessage message,
            RemoteAgent remoteAgent)
    {
        // Lock status: OK
        //Log.debug("["+message.getMsgId()+"] Got response ");

        PendingRPC rpc;
        synchronized (pendingRPC) {
            rpc = pendingRPC.get(message.getRequestId());
            if (rpc == null) {
                Log.error("Unexpected RPC response requestId=" +
                    message.getRequestId() + " from=" + remoteAgent.agentName +
                    "," + message.getMsgId());
                return;
            }
        }

        synchronized (rpc) {
            // Find responder in rpc.responders and remove (## error if
            // not found)
            if (rpc.responders != null)
                rpc.responders.remove(remoteAgent);

            if (rpc.callback == this) {
                // Blocking call to sendRPC()
                rpc.response = message;
                rpc.notify();
            }
            else {
                if (rpc.responders.size() == 0) {
                    synchronized (pendingRPC) {
                        pendingRPC.remove(message.getRequestId());
                    }
                }
                responseCallbackPool.execute(
                        new AsyncRPCResponse(message,rpc.callback));
            }
        }
    }

    private void deliverMessage(Message message)
    {
        // Lock status: OK

        int rpcCount = 0;
        HashSet<Object> callbacks = new HashSet<Object>();
        if (message.isRPC()) {
            try {
                for (FilterTable filterTable : responderReceiveFilters) {
                    filterTable.match(message, callbacks, null);
                }
                for (Object callback : callbacks) {
                    if (Log.loggingDebug)
                        Log.debug("deliverMessage rpc to "+callback);
                    if (callback instanceof MessageDispatch)
                        ((MessageDispatch)callback).dispatchMessage(message,
                                MessageCallback.RESPONSE_EXPECTED,
                                (MessageCallback)callback);
                    else
                        ((MessageCallback)callback).handleMessage(message,
                                MessageCallback.RESPONSE_EXPECTED);
                    rpcCount++;
                }
            }
            catch (Exception e) {
                ExceptionResponseMessage response =
                        new ExceptionResponseMessage(message, e);
                sendResponse(response);
            }
            callbacks.clear();
            // Intentional fall through
        }

        for (FilterTable filterTable : receiveFilters) {
            filterTable.match(message, callbacks, null);
        }
        if (Log.loggingDebug && (callbacks.size() == 0 && rpcCount == 0))
            Log.debug("deliverMessage matched 0 callbacks");

        for (Object callback : callbacks) {
            if (Log.loggingDebug)
                Log.debug("deliverMessage to "+callback);
            if (callback instanceof MessageDispatch)
                ((MessageDispatch)callback).dispatchMessage(message,
                        MessageCallback.NO_FLAGS,
                        (MessageCallback)callback);
            else
                ((MessageCallback)callback).handleMessage(message,
                    MessageCallback.NO_FLAGS);
        }
    }

    private class AgentConnector implements Runnable
    {
        AgentConnector(NewAgentMessage message)
        {
            this.message = message;
        }

        public void run() {
            try {
                connect();
            } catch (Exception ex) {
                Log.exception("AgentConnector", ex);
                try { if (agentSocket != null) agentSocket.close(); }
                catch (Exception ignore) { }
            }
        }

        void connect()
            throws IOException, MVRuntimeException
        {
            while (true) {
                try {
                    agentSocket = SocketChannel.open(new InetSocketAddress(
                            message.agentIP, message.agentPort));
                    break;
                }
                catch (ConnectException ex) {
                    Log.info("Could not connect to agent '"+message.agentName+
                        "' at "+message.agentIP+":"+message.agentPort +" "+ ex);
                    try {
                        Thread.sleep(1000);
                    } catch (Exception ignore) { }
                }
            }

            agentSocket.configureBlocking(false);
            agentSocket.socket().setTcpNoDelay(true);
            if (Log.loggingDebug)
                Log.debug("MessageAgent: connected to agent "+ agentSocket);

            AgentStateMessage agentState;
            agentState = new AgentStateMessage(agentId,
                agentName, ":same", agentPort, domainFlags);
            MVByteBuffer buffer = new MVByteBuffer(64);
            Message.toBytes(agentState, buffer);

            buffer.flip();
            if (! ChannelUtil.writeBuffer(buffer, agentSocket)) {
                throw new RuntimeException("could not write to agent");
            }

            RemoteAgent remoteAgent = waitForAgent(agentSocket);
            if (remoteAgent == null) {
                agentSocket.close();
                return;
            }

            sendAdvertisements(remoteAgent);

            if (Log.loggingDebug)
                Log.debug("connect: Accepted connection from "+message.agentName);
            messageIO.addAgent(remoteAgent);
        }

        NewAgentMessage message;
        SocketChannel agentSocket = null;
    }

    private RemoteAgent getAgent(String agentName)
    {
        // Lock status: OK requires remoteAgents
        for (RemoteAgent remoteAgent : remoteAgents) {
            if (agentName.equals(remoteAgent.agentName))
                return remoteAgent;
        }
        return null;
    }

    private RemoteAgent getAgent(int agentId)
    {
        // Lock status: OK requires remoteAgents
        for (RemoteAgent remoteAgent : remoteAgents) {
            if (agentId == remoteAgent.agentId)
                return remoteAgent;
        }
        return null;
    }

    // This class is obsolete; outgoing messages no longer go through
    // the square queue.  Preserving in case we want to put MVByteBuffers
    // through a square queue.
    private class MessageMarshaller implements Runnable
    {
        MessageMarshaller()
        {
            (new Thread(this,"MessageMarshaller")).start();
        }

        public void run()
        {
            while (true) {
                try {
                    marshall();
                }
                catch (Exception ex) {
                    Log.exception("MessageMarshaller", ex);
                }
            }
        }

        void marshall()
                throws InterruptedException
        {
            // Lock status: INCOMPLETE
            SquareQueue<RemoteAgent,Message>.SubQueue raq;
            raq = remoteAgentOutput.remove();
            try {
                if (raq.next()) {
                    Message message = raq.getHeadValue();
                    RemoteAgent remoteAgent = raq.getKey();
                    if (remoteAgent == selfRemoteAgent) {
                        message.remoteAgent = (RemoteAgent)remoteAgent;
                        handleSelfMessage(message);
                    }
                    else {
                        synchronized (remoteAgent.outputBuf) {
                            // Only write messages if the socket is open
                            if (remoteAgent.socket != null)
                                Message.toBytes(message, remoteAgent.outputBuf);
                        }
                        messageIO.outputReady();
                    }
                }
            }
            finally {
                remoteAgentOutput.requeue(raq);
            }
        }
    }

    private class SelfMessageHandler implements Runnable
    {
        SelfMessageHandler()
        {
            Thread thread = new Thread(this,"SelfMessage");
            thread.setDaemon(true);
            thread.start();
        }

        public void run()
        {
            while (true) {
                try {
                    handle();
                }
                catch (Exception ex) {
                    Log.exception("SelfMessage", ex);
                }
            }
        }

        void handle()
                throws InterruptedException
        {
            synchronized (selfRemoteAgent.outputBuf) {
                while (selfRemoteAgent.outputBuf.position() == 0)
                    selfRemoteAgent.outputBuf.wait();

                if (Log.loggingDebug)
                    Log.debug("SelfMessageHandler.handle pos="+
                        selfRemoteAgent.outputBuf.position());
                MVByteBuffer inputBuf = selfRemoteAgent.outputBuf;
                inputBuf.flip();
                selfMessages.clear();
                while (inputBuf.remaining() >= 4) {
                    int currentPos = inputBuf.position();
                    int messageLen = inputBuf.getInt();
                    if (inputBuf.remaining() < messageLen)  {
                        inputBuf.position(currentPos);
                        break;
                    }
                    Message message = (Message)
                        MarshallingRuntime.unmarshalObject(inputBuf);

                    message.remoteAgent = selfRemoteAgent;
                    selfMessages.add(message);

                    // Move the position as if we read the data
                    // The callback may or may not have read the data, so this
                    // ensures the position is moved.
                    inputBuf.position(currentPos + 4 + messageLen);
                }
                inputBuf.getNioBuf().compact();
            }

            for (Message message : selfMessages)
                handleSelfMessage(message);

        }

        List<Message> selfMessages = new LinkedList<Message>();

    }

    private void sendAdvertisements(RemoteAgent remoteAgent)
    {
        // Lock status: OK
        AdvertiseMessage message = new AdvertiseMessage(advertisements);
        message.setMessageId(nextMessageId());
        Message.toBytes(message, remoteAgent.outputBuf);
    }

    private synchronized long nextMessageId()
    {
        // Lock status: OK
        return nextMessageId ++;
    }

    // Add a filter table to a list if it's not already in the list.
    // The existing table is copied before adding the new filter table.
    // This allows the List<FilterTable> iterators to run without
    // synchronization.
    private void addUniqueFilterTable(FilterTable filterTable,
                List<FilterTable> list)
    {
        // Lock status: OK
        synchronized (list) {
            if (! list.contains(filterTable)) {
                List<FilterTable> newList = new LinkedList<FilterTable>(list);
                newList.add(filterTable);
                list = newList;
            }
        }
    }

    class RegisteredSubscription extends Subscription {
        public Object getAssociation()
        {
            return callback;
        }

        LinkedList<RemoteAgent> producers;
        MessageCallback callback;
    }

    public class DomainClient {
        DomainClient()
        {
        }

        public String allocName(String type, String namePattern)
            throws java.io.IOException
        {
            AllocNameMessage allocName = new AllocNameMessage(type,namePattern);
            allocName.setMessageId(nextMessageId());

            List<AgentInfo> list = new ArrayList<AgentInfo>(1);
            list.add(domainServerAgent);
            PendingRPC rpc = setupInternalRPC(allocName, list);
            sendMessageToList(allocName, list);

            waitInternalRPC(rpc);

            AllocNameResponseMessage response =
                (AllocNameResponseMessage) rpc.response;

            return response.getName();
        }

        public void awaitPluginDependents(String pluginType, String pluginName)
        {
            AwaitPluginDependentsMessage await =
                new AwaitPluginDependentsMessage(pluginType, pluginName);
            await.setMessageId(nextMessageId());

            List<AgentInfo> list = new ArrayList<AgentInfo>(1);
            list.add(domainServerAgent);
            PendingRPC rpc = setupInternalRPC(await, list);
            sendMessageToList(await, list);

            waitInternalRPC(rpc);
        }

        public void pluginAvailable(String pluginType, String pluginName)
        {
            PluginAvailableMessage available =
                new PluginAvailableMessage(pluginType, pluginName);
            available.setMessageId(nextMessageId());

            List<AgentInfo> list = new ArrayList<AgentInfo>(1);
            list.add(domainServerAgent);
            sendMessageToList(available, list);
        }

        Message readMessage()
            throws java.io.IOException
        {
            ByteBuffer readBuffer = ByteBuffer.allocate(4);
            int nBytes = ChannelUtil.fillBuffer(readBuffer,domainServerSocket);
            if (nBytes == 0) {
                throw new RuntimeException("domain server closed connection");
            }
            if (nBytes < 4) {
                throw new RuntimeException("domain server incomplete response");
            }
            int msgLen = readBuffer.getInt();
            if (msgLen < 0) {
                throw new RuntimeException("domain server invalid response");
            }

            MVByteBuffer buffer = new MVByteBuffer(msgLen);
            nBytes = ChannelUtil.fillBuffer(buffer.getNioBuf(),domainServerSocket);
            if (nBytes == 0) {
                throw new RuntimeException("domain server closed connection");
            }
            if (nBytes < 4) {
                throw new RuntimeException("domain server invalid response, expecting "+ msgLen + " got "+nBytes);
            }

            Message message = (Message)
                    MarshallingRuntime.unmarshalObject(buffer);

            return message;
        }

    }

    private List<RemoteAgent> remoteAgents = new LinkedList<RemoteAgent>();
    private SquareQueue<RemoteAgent,Message> remoteAgentOutput =
        new SquareQueue<RemoteAgent,Message>("MessageAgent");
    private RemoteAgent selfRemoteAgent;

    private long nextMessageId = 1;

    private long nextSubId = 1;
    private int defaultSubscriptionFlags = NO_FLAGS;
    private Map<Long,RegisteredSubscription> subscriptions;

    // remoteAdverts is unreferenced
    /* private HashMap<MessageType,RemoteAgent> remoteAdverts; */

    private List<FilterTable> sendFilters;
    private List<FilterTable> receiveFilters;
    private List<FilterTable> responderSendFilters;
    private List<FilterTable> responderReceiveFilters;
    private FilterTable defaultSendFilterTable = new DefaultFilterTable();
    private FilterTable defaultReceiveFilterTable = new DefaultFilterTable();
    private FilterTable defaultResponderReceiveFilterTable = new DefaultFilterTable();
    private FilterTable defaultResponderSendFilterTable = new DefaultFilterTable();

    private long statAppMessageCount = 0;
    private long statSystemMessageCount = 0;

    // -- RPC data structures
    // message id to RPC
    private Map<Long,PendingRPC> pendingRPC = new HashMap<Long,PendingRPC>();
    class PendingRPC {
        long messageId;
        Set<Object> responders;         // Set<RemoteAgent>
        ResponseCallback callback;
        Message response;
    }
    class AsyncRPCResponse implements Runnable {
        AsyncRPCResponse(ResponseMessage m, ResponseCallback c) {
            response = m;
            callback = c;
        }
        ResponseMessage response;
        ResponseCallback callback;
        public void run() {
            callback.handleResponse(response);
        }
    }
    class ResponseThreadFactory implements ThreadFactory
    {
        public Thread newThread(Runnable runnable)
        {
            return new Thread(runnable, "MessageResponse-"+threadCount++);
        }
        int threadCount = 1;
    }
    private ExecutorService responseCallbackPool =
                Executors.newFixedThreadPool(10,new ResponseThreadFactory());

    class AgentConnectionThreadFactory implements ThreadFactory
    {
        public Thread newThread(Runnable runnable)
        {
            Thread thread = new Thread(runnable, "AgentConnection-"+threadCount++);
            thread.setDaemon(true);
            return thread;
        }
        int threadCount = 1;
    }

    // -- Instance data

    private String agentName;
    private int agentPort;
    private int agentId;
    private long domainStartTime;
    private int domainRetries = Integer.MAX_VALUE;

    private SocketChannel domainServerSocket = null;
    private AgentInfo domainServerAgent;
    private int domainFlags;
    private MessageIO messageIO;
    private TcpServer listener;
    private ExecutorService threadPool = Executors.newCachedThreadPool(
        new AgentConnectionThreadFactory());

    private List<String> remoteAgentNames;

    private Set<MessageType> advertisements = new HashSet<MessageType>();
    private String advertFileName = "<unknown>";

    private Set<MessageType> noProducersExpected = new HashSet<MessageType>();

    static class MessageAgentStatsLogger implements Runnable {
        public void run() {
            while (true) {
                try {
                    Thread.sleep(intervalBetweenStatsLogging);
                    Log.info("MessageAgent Local Subscription Counters: last interval/total: " + 
                        "Created " + (localSubscriptionCreatedCount - lastLocalSubscriptionCreatedCount) + "/" + localSubscriptionCreatedCount + 
                        ", Removed " + (localSubscriptionRemovedCount - lastLocalSubscriptionRemovedCount) + "/" + localSubscriptionRemovedCount);
                    Log.info("MessageAgent Remote Subscription Counters: last interval/total: " + 
                        "Created " + (remoteSubscriptionCreatedCount - lastRemoteSubscriptionCreatedCount) + "/" + remoteSubscriptionCreatedCount + 
                        ", Removed " + (remoteSubscriptionRemovedCount - lastRemoteSubscriptionRemovedCount) + "/" + remoteSubscriptionRemovedCount);
                    Log.info("MessageAgent Filter Updates: last interval/total: " +
                        "Local " + (localFilterUpdateCount - lastLocalFilterUpdateCount) + "/" +localFilterUpdateCount +
                        ", Remote " + (remoteFilterUpdateCount - lastRemoteFilterUpdateCount) + "/" + remoteFilterUpdateCount);
                    lastLocalSubscriptionCreatedCount = localSubscriptionCreatedCount;
                    lastLocalSubscriptionRemovedCount = localSubscriptionRemovedCount;
                    lastRemoteSubscriptionCreatedCount = remoteSubscriptionCreatedCount;
                    lastRemoteSubscriptionRemovedCount = remoteSubscriptionRemovedCount;
                    lastLocalFilterUpdateCount = localFilterUpdateCount;
                    lastRemoteFilterUpdateCount = remoteFilterUpdateCount;                    
                }
                catch (Exception e) {
                    Log.exception("MessageAgent.MessageAgentStatsLogger.run thread interrupted", e);
                }
            }
        }
    }
    
    private static int intervalBetweenStatsLogging = 5000;
    
    private static long localSubscriptionCreatedCount = 0;
    private static long localSubscriptionRemovedCount = 0;
    private static long localFilterUpdateCount = 0;
    private static long remoteSubscriptionCreatedCount = 0;
    private static long remoteSubscriptionRemovedCount = 0;
    private static long remoteFilterUpdateCount = 0;
    
    private static long lastLocalSubscriptionCreatedCount = 0;
    private static long lastLocalSubscriptionRemovedCount = 0;
    private static long lastLocalFilterUpdateCount = 0;
    private static long lastRemoteSubscriptionCreatedCount = 0;
    private static long lastRemoteSubscriptionRemovedCount = 0;
    private static long lastRemoteFilterUpdateCount = 0;

}

