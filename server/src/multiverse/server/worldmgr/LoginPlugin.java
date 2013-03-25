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

package multiverse.server.worldmgr;

import multiverse.server.util.*;
import multiverse.server.engine.*;
import multiverse.server.network.*;
import multiverse.server.messages.PropertyMessage;
import java.io.*;
import java.net.SocketAddress;
import java.nio.channels.*;
import java.nio.ByteBuffer;
import java.util.*;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Supports the character selection and creation protocols.  The plugin
 * returns a list characters available to the user.  Includes character
 * properties and the proxy server host/port for each character.
 * <p>
 * The user can create a character by supplying a set of character
 * properties.  The plugin returns the new character properties or
 * an error if the character could not be created.
 * <p>
 * LoginPlugin must be sub-classed to implement character selection
 * and creation.  A sub-class must implement {@link #handleCharacterRequestMessage(CharacterRequestMessage, SocketChannel)}
 * and may implement {@link #handleCharacterCreateMessage(CharacterCreateMessage , SocketChannel , MVByteBuffer)} if character
 * creation is supported and {@link #handleCharacterDeleteMessage(CharacterDeleteMessage , SocketChannel , MVByteBuffer)} if character
 * deletion is supported.  Sub-classes should not need to call any
 * LoginPlugin methods except getCharacterGenerator().
 * <p>
 * For a sample implementation see {@link multiverse.mars.plugins#MarsLoginPlugin}
 */
public class LoginPlugin extends multiverse.server.engine.EnginePlugin
        implements TcpAcceptCallback
{
    public LoginPlugin() {
        super("Login");
        setPluginType("Login");
    }

    public static final int MSGCODE_CHARACTER_RESPONSE = 2;
    public static final int MSGCODE_CHARACTER_DELETE = 3;
    public static final int MSGCODE_CHARACTER_DELETE_RESPONSE = 4;
    public static final int MSGCODE_CHARACTER_CREATE = 5;
    public static final int MSGCODE_CHARACTER_CREATE_RESPONSE = 6;
    public static final int MSGCODE_CHARACTER_REQUEST = 7;
    public static final int MSGCODE_CHARACTER_SELECT_REQUEST = 8;
    public static final int MSGCODE_CHARACTER_SELECT_RESPONSE = 9;
    public static final int MSGCODE_SECURE_CHARACTER_REQUEST = 10;

    // Timeout on client connections
    // ## remove " * 100" once client supports reconnect
    public static final int LOGIN_IDLE_TIMEOUT = 20000 * 100;

    /**
     * This method connects to the database. It assumes the host, user, and port
     * have already been set.
     */
    public void dbConnect() {
        if (Engine.getDatabase() == null) {
                Log.debug("Setting Database in WorldManager.dbConnect");
            Engine.setDatabase( new Database(Engine.getDBDriver() ));
        }
        //Log.debug("About to call Engnie.getDatabase in WorldManager.dbConnect");
        Engine.getDatabase().connect(Engine.getDBUrl(), Engine.getDBUser(),
                Engine.getDBPassword());
    }

    /**
     * Set the port the login plugin will listen to for incoming
     * tcp connection.
     * 
     * @param port
     *            the port number used for incoming tcp connections
     */
    public void setTCPPort(int port) {
        this.tcpPort = port;
    }

    /**
     * This method returns the port the login plugin will listen to for
     * incoming tcp connection.
     * 
     * @see #setTCPPort(int)
     * @return the tcp port number
     */
    public int getTCPPort() {
        if (tcpPort == null) {
            return Engine.getWorldMgrPort().intValue();
        }
        return tcpPort;
    }

    public void onActivate() {
        try {
            // start the login thread
            loginListener = new TcpServer(getTCPPort());
            loginListener.registerAcceptCallback(this);
            loginListener.start();
        } catch (Exception e) {
            Log.exception("LoginPlugin.onActivate caught exception", e);
            System.exit(1);
        }
    }

    private static String socketToString(SocketChannel channel)
    {
        java.net.Socket socket = channel.socket();
        return "remote="+socket.getRemoteSocketAddress()+
            " local="+socket.getLocalSocketAddress();
    }

    public void onTcpAccept(SocketChannel clientSocket) {
        try {
            Log.info("LoginPlugin: CONNECTION " + socketToString(clientSocket));
            threadPool.execute(new SocketHandler(clientSocket));
        } catch (IOException e) {
            Log.exception("LoginListener: ", e);
        }
    }

    protected class SocketHandler implements Runnable {
        public SocketHandler(SocketChannel socket)
                throws IOException
        {
            clientSocket = socket;
            selector = Selector.open();
            clientSelection =
                clientSocket.register(selector, SelectionKey.OP_READ);
        }
        private SocketChannel clientSocket = null;
        private Selector selector = null;
        private SelectionKey clientSelection = null;
        private Integer accountId = null;
        private List<Map<String, Serializable>> characterInfo = null;

        public SocketAddress getRemoteSocketAddress() {
            return clientSocket.socket().getRemoteSocketAddress();
        }
        
        public void setAccountId(Integer accountId) {
            this.accountId = accountId;
        }
        public Integer getAccountId() {
            return accountId;
        }

        public void setCharacterInfo(List<Map<String,Serializable>> charInfo) {
            characterInfo = charInfo;
        }
        public List<Map<String,Serializable>> getCharacterInfo() {
            return characterInfo;
        }

        private int fillBuffer(SocketChannel socket, ByteBuffer buffer)
                throws IOException
        {
            clientSelection.interestOps(SelectionKey.OP_READ);
            while (buffer.remaining() > 0) {
                int nReady = selector.select(LOGIN_IDLE_TIMEOUT);
                if (nReady == 1) {
                    selector.selectedKeys().clear();
                    int nBytes = socket.read(buffer);
                    if (nBytes == -1)
                        break;
                }
                else {
                    Log.debug("Connection timeout while reading");
                    break;
                }
            }
            buffer.flip();
            return buffer.limit();
        }

        private boolean writeBuffer(ByteBuffer buffer)
                throws IOException
        {
            clientSelection.interestOps(SelectionKey.OP_WRITE);
            while (buffer.hasRemaining()) {
                int nReady = selector.select(LOGIN_IDLE_TIMEOUT);
                if (nReady == 1) {
                    selector.selectedKeys().clear();
                    if (clientSocket.write(buffer) == 0)
                        break;
                }
                else {
                    Log.debug("Connection timeout while writing");
                    break;
                }
            }
            return ! buffer.hasRemaining();
        }

        public void run() {
            try {
                ByteBuffer header = ByteBuffer.allocate(8);
                while (true) {
                    int nBytes = fillBuffer(clientSocket,header);
                    if (nBytes == 0)  {
                        Log.info("LoginPlugin: DISCONNECT " +
                            socketToString(clientSocket));
                        break;
                    }
                    if (nBytes < 8)  {
                        Log.error("LoginPlugin: reading header nBytes "+nBytes);
                        break;
                    }
                    int messageLength = header.getInt();
                    int messageCode = header.getInt();
                    header.clear();
                    if (Log.loggingDebug)
                        Log.debug("LoginPlugin: code "+messageCode+" ("+
                                messageLength+" bytes)");
                    if (messageLength > 64000)  {
                        Log.error("LoginPlugin: max message length exceeded");
                        break;
                    }
                    else if (messageLength < 0)  {
                        Log.error("LoginPlugin: invalid message length");
                        break;
                    }

		    if (messageLength == 4) {
                        Log.error("LoginPlugin: invalid message length (possibly an old client)");
			break;
		    }

                    ByteBuffer message = null;
                    if (messageLength > 4)  {
                        message = ByteBuffer.allocate(messageLength-4);
                        nBytes = fillBuffer(clientSocket,message);
                        if (nBytes == -1 || nBytes != messageLength-4)  {
                            Log.error("LoginPlugin: error reading message body");
                            break;
                        }
                    }

                    ByteBuffer responseBuf;
                    responseBuf = dispatchMessage(messageCode,message, this);
                    if (responseBuf != null) {
                        if ( ! writeBuffer(responseBuf))
                            break;
                    }
                    else
                        break;
                }
            } catch (InterruptedIOException e) {
                Log.info("LoginPlugin: closed connection due to timeout");
            } catch (IOException e) {
                Log.exception("LoginPlugin.SocketHandler: ", e);
            } catch (MVRuntimeException e) {
                Log.exception("LoginPlugin.SocketHandler: ", e);
            } catch (Exception e) {
                Log.exception("LoginPlugin.SocketHandler: ", e);
            }

	    try {
		clientSelection.cancel();
		clientSocket.close();
		selector.close();
	    } catch (Exception ignore) { /* ignore */ }
        }

        ByteBuffer dispatchMessage(int messageCode, ByteBuffer messageBuf,
                SocketHandler clientSocket)
                throws IOException
        {
            ByteBuffer responseBuf = null;
            if (messageCode == MSGCODE_SECURE_CHARACTER_REQUEST) {
                CharacterRequestMessage msg = new CharacterRequestMessage();
                MVByteBuffer buffer = new MVByteBuffer(messageBuf);
                msg.clientVersion = buffer.getString();
                msg.authToken = buffer.getByteBuffer();
                if (Log.loggingDebug)
                    Log.debug("LoginPlugin: SecureCharacterRequestMessage version="+
                        msg.clientVersion+" token="+msg.authToken);

                CharacterResponseMessage response;
                int versionCompare = ServerVersion.compareVersionStrings(
                        msg.clientVersion, ServerVersion.ServerMajorVersion);
                if (versionCompare != ServerVersion.VERSION_GREATER &&
                        versionCompare != ServerVersion.VERSION_EQUAL) {
                    response = new CharacterResponseMessage();
                    response.setErrorMessage("Unsupported client version");
                }
                else {
                    response = handleCharacterRequestMessage(msg, clientSocket);
                }
                if (response.getServerVersion() == null ||
                        response.getServerVersion().equals(""))
                    response.setServerVersion(ServerVersion.ServerMajorVersion);
                responseBuf = response.getEncodedMessage();
            }
            else if (messageCode == MSGCODE_CHARACTER_CREATE) {
                synchronized (characterCreateLock) {
                    CharacterCreateMessage msg = new CharacterCreateMessage();
                    MVByteBuffer mvBuf = new MVByteBuffer(messageBuf);
                    msg.props = PropertyMessage.unmarshallProperyMap(mvBuf);
                    if (Log.loggingDebug)
                        Log.debug("LoginPlugin: CharacterCreateMessage prop count="+
                            msg.props.size());
                    CharacterCreateResponseMessage response;
                    response = handleCharacterCreateMessage(msg, clientSocket);
                    responseBuf = response.getEncodedMessage();
                }
            }
            else if (messageCode == MSGCODE_CHARACTER_DELETE) {
                CharacterDeleteMessage msg = new CharacterDeleteMessage();
                MVByteBuffer mvBuf = new MVByteBuffer(messageBuf);
                msg.props = PropertyMessage.unmarshallProperyMap(mvBuf);
                if (Log.loggingDebug)
                    Log.debug("LoginPlugin: CharacterDeleteMessage prop count="+
                        msg.props.size());
                CharacterDeleteResponseMessage response;
                response = handleCharacterDeleteMessage(msg, clientSocket);
                responseBuf = response.getEncodedMessage();
            }
            else if (messageCode == MSGCODE_CHARACTER_SELECT_REQUEST) {
                CharacterSelectRequestMessage msg = new CharacterSelectRequestMessage();
                MVByteBuffer mvBuf = new MVByteBuffer(messageBuf);
                msg.props = PropertyMessage.unmarshallProperyMap(mvBuf);
                if (Log.loggingDebug)
                    Log.debug("LoginPlugin: CharacterSelectRequestMessage prop count="+
                        msg.props.size());
                CharacterSelectResponseMessage response;
                response = handleCharacterSelectRequestMessage(msg, clientSocket);
                responseBuf = response.getEncodedMessage();
            }
            else {
                Log.error("Unknown message code " + messageCode);
            }

            return responseBuf;
        }

    }

    /** Message to authorize user and get their character list.
    */
    public class CharacterRequestMessage {
        /** the user's auth token.  This is supplied in an MVByteBuffer
            to facilitate arbitrary token encodings.  The buffer is
            rewound before returning.
        */
        public MVByteBuffer getAuthToken() {
            authToken.rewind();
            return authToken;
        }
        /** the client version.
        */
        public String getClientVersion() {
            return clientVersion;
        }
        MVByteBuffer authToken;
        String clientVersion;
    }

    /** Message to return world token and character list.
    */
    public class CharacterResponseMessage {
        /** Get the world token.  Defaults to null.
        */
        public String getWorldToken() {
            return worldToken;
        }
        /** Set the world token. Don't use this anymore, the world
         * token is sent in the CharacterSelectResponseMesage, now.
        */
        public void setWorldToken(String worldToken) {
            throw new RuntimeException("deprecated");
        }
        /** Get the server version.  Defaults to null.
        */
        public String getServerVersion() {
            return serverVersion;
        }
        /** Set the server version.
        */
        public void setServerVersion(String serverVersion) {
            this.serverVersion= serverVersion;
        }

        /** Get the error message.  Defaults to empty string.
        */
        public String getErrorMessage() {
            return errorMessage;
        }
        /** Set the error message.
        */
        public void setErrorMessage(String errorMessage) {
            this.errorMessage= errorMessage;
        }

        /** Add one character properties.  The {@code characterInfo} should
            contain at least a Long "characterId" containing the
            character's OID.
		@param characterInfo character properties
        */
        public void addCharacter(Map<String,Serializable> characterInfo) {
            characters.add(characterInfo);
        }
        public List<Map<String,Serializable>> getCharacters() {
            return characters;
        }

        ByteBuffer getEncodedMessage() {
            if (Log.loggingDebug)
                Log.debug("LoginPlugin: returning characters:" +
                        " serverVersion="+serverVersion+
                        " worldToken="+worldToken+
                        " errorMessage="+errorMessage+
                        " nChars="+characters.size());

            MVByteBuffer buffer = new MVByteBuffer(1024);
            buffer.putInt(0);                            // length
            buffer.putInt(MSGCODE_CHARACTER_RESPONSE);   // message code
            buffer.putString(serverVersion);
            buffer.putString(worldToken);
            buffer.putString(errorMessage);
            buffer.putInt(characters.size());
            for (Map<String,Serializable> properties : characters) {
                List<String> propStrings = new ArrayList<String>();
                int nProps = PropertyMessage.createPropertyString(
                        propStrings, properties, "");
                buffer.putInt(nProps);
                for (String s : propStrings) {
                    buffer.putString(s);
                }
            }

            // patch the message length
            int len = buffer.position();
            buffer.getNioBuf().rewind();
            buffer.putInt(len-4);
            buffer.position(len);

            return (ByteBuffer) buffer.getNioBuf().flip();
        }

        private String worldToken = "";
        private String serverVersion;
        private String errorMessage = "";
        private LinkedList<Map<String,Serializable>> characters =
                new LinkedList<Map<String,Serializable>>();
    }

    /** Message to create a character using the given properties.
    */
    public class CharacterCreateMessage {
        /** Get the requested character properties.
        */
        public Map<String,Serializable> getProperties() {
            return props;
        }
        private Map<String,Serializable> props;
    }


    /** Message to return new character properties.  The properties should
        contain a Boolean "status" indicating success or failure.
        On success, the
        properties should include at least a "characterId" with the
        new character OID.  On failure, the properties should contain
        an "errorMessage".
    */
    public class CharacterCreateResponseMessage {
        /** Set the new character properties.
        */
        public void setProperties(Map<String,Serializable> props) {
            this.props = props;
        }
        /** Get the new character properties.  Defaults to null.
        */
        public Map<String,Serializable> getProperties() {
            return props;
        }

        ByteBuffer getEncodedMessage() {
            if (Log.loggingDebug)
                Log.debug("LoginPlugin: create character response:" +
                        " nProps=" + ((props == null) ? 0 : props.size()));

            MVByteBuffer buffer = new MVByteBuffer(1024);
            buffer.putInt(0);                                  // length
            buffer.putInt(MSGCODE_CHARACTER_CREATE_RESPONSE);  // message code
            if (props == null)  {
                buffer.putInt(0);
            }
            else  {
                List<String> propStrings = new ArrayList<String>();
                int nProps = PropertyMessage.createPropertyString(
                        propStrings, props, "");
                buffer.putInt(nProps);
                for (String s : propStrings) {
                    buffer.putString(s);
                }
            }

            // patch the message length
            int len = buffer.position();
            buffer.getNioBuf().rewind();
            buffer.putInt(len-4);
            buffer.position(len);

            return (ByteBuffer) buffer.getNioBuf().flip();
        }

        private Map<String,Serializable> props;
    }

    /** Respond to a character list request message from the client.
        The {@code message} contains the client's auth token and
        client version.  Implementations should verify the
        authenticity of the token and return an empty world token if
        the token is invalid.  If the auth token is valid, then {@link
        CharacterResponseMessage#setWorldToken(String)} should be set
        to a non-empty string.  This will indicate to the LoginPlugin
        base class that the user is authorized.  The auth token to be
        passed to subsequent handleCharacterCreateMessage() calls only
        if the user is authorized.
        <p>
        The auth token should
        contain (or otherwise supply) the user's identity.  The user's
        characters are returned in a {@link CharacterResponseMessage}.
        <p>
        Implementations must not read or write data to the {@code clientSocket}.

	@param message request for character list and authorization.
	@param clientSocket the client's socket connection.  Useful for
	determining the client's source IP and port number.
	Implementations must not read or write data to the socket.
    */
    protected CharacterResponseMessage handleCharacterRequestMessage(
                CharacterRequestMessage message, SocketHandler clientSocket)
    {
        MVByteBuffer authToken = message.getAuthToken();
        SecureToken token = SecureTokenManager.getInstance().importToken(authToken);
        boolean valid = true;

        if (LoginPlugin.SecureToken) {
            valid = token.getValid();
        }
        if (!token.getIssuerId().equals("master")) {
            valid = false;
        }

        int uid = (Integer)token.getProperty("account_id");

        CharacterResponseMessage response = new CharacterResponseMessage();
        response.setServerVersion(ServerVersion.ServerMajorVersion);

        if (valid) {
            clientSocket.setAccountId(uid);
            clientSocket.setCharacterInfo(response.getCharacters());
        }
        else {
            response.setErrorMessage("invalid master token");
        }
        return response;
    }

    /** Message to return new character properties.  The properties should
        contain a Boolean "status" indicating success or failure.
        On success, the
        properties should include at least a "characterId" with the
        new character OID.  On failure, the properties should contain
        an "errorMessage".
    */
    public class CharacterDeleteResponseMessage {
        /** Set the character properties.
        */
        public void setProperties(Map<String,Serializable> props) {
            this.props = props;
        }
        /** Get the character properties.  Defaults to null.
        */
        public Map<String,Serializable> getProperties() {
            return props;
        }

        ByteBuffer getEncodedMessage() {
            if (Log.loggingDebug)
                Log.debug("LoginPlugin: delete character response:" +
                        " nProps=" + ((props == null) ? 0 : props.size()));

            MVByteBuffer buffer = new MVByteBuffer(1024);
            buffer.putInt(0);                                  // length
            buffer.putInt(MSGCODE_CHARACTER_DELETE_RESPONSE);  // message code
            if (props == null)  {
                buffer.putInt(0);
            }
            else  {
                List<String> propStrings = new ArrayList<String>();
                int nProps = PropertyMessage.createPropertyString(
                        propStrings, props, "");
                buffer.putInt(nProps);
                for (String s : propStrings) {
                    buffer.putString(s);
                }
            }

            // patch the message length
            int len = buffer.position();
            buffer.getNioBuf().rewind();
            buffer.putInt(len-4);
            buffer.position(len);

            return (ByteBuffer) buffer.getNioBuf().flip();
        }

        private Map<String,Serializable> props;
    }

    /** Respond to a character creation request from the client.
        The {@code message} contains the desired character properties.
        <p>
        If character creation is successful, the returned
        CharacterCreateResponseMessage should contain the new character
        properties.  In addition, the response should also contain
        a Boolean "status" property that is TRUE for success and FALSE
        for failure.  In case of failure, the response should contain
        a String "errorMessage" property.
        <p>
        Implementations must not read or write data to the {@code clientSocket}.
    */
    protected CharacterCreateResponseMessage handleCharacterCreateMessage(
                CharacterCreateMessage message,
                SocketHandler clientSocket)
    {
        CharacterCreateResponseMessage response =
                new CharacterCreateResponseMessage();
        HashMap<String,Serializable> props = new HashMap<String,Serializable>();
        props.put("status",Boolean.FALSE);
        props.put("errorMessage","character creation is not supported");
        response.setProperties(props);
        return response;
    }

    /** Message to delete a character using the given properties.
    */
    public class CharacterDeleteMessage {
        /** Get the requested character properties.
        */
        public Map<String,Serializable> getProperties() {
            return props;
        }
        private Map<String,Serializable> props;
    }

    protected CharacterDeleteResponseMessage handleCharacterDeleteMessage(
                CharacterDeleteMessage message,
                SocketHandler clientSocket)
    {
        CharacterDeleteResponseMessage response =
                new CharacterDeleteResponseMessage();
        HashMap<String,Serializable> props = new HashMap<String,Serializable>();
        props.put("status",Boolean.FALSE);
        props.put("errorMessage","character deletion is not supported");
        response.setProperties(props);
        return response;
    }

    /** Message to log in as the selected character.
    */
    public class CharacterSelectRequestMessage {
        /** Get the requested character properties.
        */
        public Map<String,Serializable> getProperties() {
            return props;
        }
        private Map<String,Serializable> props;
    }

    /** Message to return access token and hostname/port for selected
        character.  The properties should contain a Boolean "status"
        indicating success or failure.  On success, the properties
        should include at least a "characterId" with the new character
        OID.  On failure, the properties should contain an
        "errorMessage".
    */
    public class CharacterSelectResponseMessage {
        /** Set the message properties.
        */
        public void setProperties(HashMap<String,Serializable> props) {
            this.props = props;
        }
        /** Get the message properties.  Defaults to null.
        */
        public Map<String,Serializable> getProperties() {
            return props;
        }

        ByteBuffer getEncodedMessage() {
            if (Log.loggingDebug)
                Log.debug("LoginPlugin: select character response:" +
                        " nProps=" + ((props == null) ? 0 : props.size()));

            MVByteBuffer buffer = new MVByteBuffer(1024);
            buffer.putInt(0);                                  // length
            buffer.putInt(MSGCODE_CHARACTER_SELECT_RESPONSE);  // message code
            buffer.putPropertyMap(props);

            // patch the message length
            int len = buffer.position();
            buffer.getNioBuf().rewind();
            buffer.putInt(len-4);
            buffer.position(len);

            return (ByteBuffer) buffer.getNioBuf().flip();
        }

        private HashMap<String,Serializable> props;
    }

    protected CharacterSelectResponseMessage handleCharacterSelectRequestMessage(
                CharacterSelectRequestMessage message,
                SocketHandler clientSocket)
    {
        // Need to check that this connection is allowed to login as this character
        boolean charAllowed = false;
        long characterOid = (Long)message.getProperties().get("characterId");
        String characterName = null;
        for (Map<String, Serializable> charInfo : clientSocket.getCharacterInfo()) {
            if (charInfo.containsKey("characterId") && charInfo.get("characterId") instanceof Long &&
                ((Long)charInfo.get("characterId")) == characterOid) {
                charAllowed = true;
                characterName = (String) charInfo.get("characterName");
                break;
            }
        }

        Log.info("LoginPlugin: SELECT_CHARACTER oid=" + characterOid +
            " name=" + characterName + " allowed=" + charAllowed);

        CharacterSelectResponseMessage response = new CharacterSelectResponseMessage();
        HashMap<String,Serializable> props = new HashMap<String,Serializable>();
        SecureTokenSpec tokenSpec = new SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_DOMAIN,
                                                        Engine.getAgent().getName(),
                                                        System.currentTimeMillis() + TokenValidTime);
        tokenSpec.setProperty("character_oid", characterOid);
        if (charAllowed) {
            Map<String, Serializable> characterProps = new HashMap<String, Serializable>();
            PluginStatus proxyStatus = selectProxyPlugin(characterProps);
            tokenSpec.setProperty("proxy_server", proxyStatus.agent_name);
            byte[] token = SecureTokenManager.getInstance().generateToken(tokenSpec);
            setProxyProperties(props, proxyStatus);
            props.put("token", token);
        }
        else {
            props.put("errorMsg", "character oid not allowed for this account");
        }
        response.setProperties(props);
        return response;
    }

    protected boolean setProxyProperties(Map<String,Serializable> props, PluginStatus proxy)
    {
        if (proxy == null)
            return false;

        props.put("proxyHostname", proxy.host_name);

        Map<String,String> info = Engine.makeMapOfString(proxy.info);
        int port;
        try {
            port = Integer.parseInt(info.get("port"));
        }
        catch (Exception e) {
            Log.exception("setProxyProperties: proxy "+proxy.plugin_name+
                " invalid port number: "+info.get("port"), e);
            return false;
        }

        props.put("proxyPort", port);

        if (Log.loggingDebug) {
            Log.debug("LoginPlugin: assigned proxy " + proxy.plugin_name +
                " host="+proxy.host_name +
                " port="+port);
        }

        return true;
    }

    private static ExecutorService threadPool =
        Executors.newCachedThreadPool(new NamedThreadFactory("LoginConnection"));

    /** Get the global character generator.
    */
    public static CharacterGenerator getCharacterGenerator() {
        return characterGenerator;
    }

    protected final PluginStatus selectProxyPlugin(
        Map<String,Serializable> characterProperties)
    {
        List<PluginStatus> plugins =
            Engine.getDatabase().getPluginStatus("Proxy");
        Iterator<PluginStatus> iterator = plugins.iterator();
        while (iterator.hasNext()) {
            PluginStatus plugin = iterator.next();
            if (plugin.run_id != Engine.getAgent().getDomainStartTime())
                iterator.remove();
        }

        if (plugins.size() == 0)
            return null;
        return selectBestProxy(plugins, characterProperties);
    }

    protected PluginStatus selectBestProxy(List<PluginStatus> plugins,
        Map<String,Serializable> characterProperties)
    {
        PluginStatus selection = null;
        int selectionPlayerCount = Integer.MAX_VALUE;
        for (PluginStatus plugin : plugins) {
            Map<String,String> status = Engine.makeMapOfString(plugin.status);
            int playerCount;
            try {
                playerCount = Integer.parseInt(status.get("players"));
            }
            catch (Exception e) {
                Log.exception("selectBestProxy: proxy "+plugin.plugin_name+
                    " invalid player count: "+status.get("players"), e);
                continue;
            }
            if (playerCount < selectionPlayerCount) {
                selection = plugin;
                selectionPlayerCount = playerCount;
            }
        }
        return selection;
    }

    private TcpServer loginListener = null;

    private Integer tcpPort = null;

    private Object characterCreateLock = new Object();

    /**
     *  The master server sends us the account id in a secure manner by default.
     *  If clients bypass the master server, set this to false.
     *  This allows people to masquerade as others and should only be used for development
     *  purposes.
     */
    public static boolean SecureToken = true;
    public static long TokenValidTime = 30000L;

    /**
     * If WorldId is set, the LoginPlugin only accepts master tokens
     * that specify the correct world id.
     */
    public static Integer WorldId = null;
    
    private static CharacterGenerator characterGenerator = new CharacterGenerator();
}
