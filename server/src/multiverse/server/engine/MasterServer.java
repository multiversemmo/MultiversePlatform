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

package multiverse.server.engine;

import java.net.*;
import java.io.*;
import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.util.*;
import java.util.concurrent.*;
import java.util.Arrays;
import java.util.Properties;
import java.util.Random;
import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.security.NoSuchAlgorithmException;
import java.security.InvalidKeyException;

public class MasterServer implements ClientConnection.AcceptCallback, ClientConnection.MessageCallback {
    public MasterServer() {
        // initialize tcp port
        String tcpPortStr = properties.getProperty("multiverse.master_tcp_port");
        if (tcpPortStr == null) {
            tcpPort = defaultTcpPort;
        }
        else {
            tcpPort = Integer.parseInt(tcpPortStr.trim());
        }

        // initialize rdp port
        String rdpPortStr = properties.getProperty("multiverse.master_rdp_port");
        if (rdpPortStr == null) {
            rdpPort = defaultRdpPort;
        }
        else {
            rdpPort = Integer.parseInt(rdpPortStr.trim());
        }
    }

    public void dbConnect() {
        if (db == null) {
            db = new MasterDatabase();
        }
        db.connect(getDBUrl(), getDBUser(), getDBPassword());
    }

    public void setTCPPort(int port) {
        this.tcpPort = port;
    }

    public int getTCPPort() {
        return tcpPort;
    }

    public void setRDPPort(int port) {
        this.rdpPort = port;
    }

    public int getRDPPort() {
        return rdpPort;
    }

    // when we can a new socket accept from rdp
    public void acceptConnection(ClientConnection con) {
        if (Log.loggingDebug)
            Log.debug("masterserver: new incoming connection: " + con);
        con.registerMessageCallback(this);
    }

    // called when we get a new packet
    public void processPacket(ClientConnection con, MVByteBuffer buf) {
        try {
            int msgType = buf.getInt();
            if (msgType == 0) {
                // this is a name resolution request
                Log.debug("masterserver: got name resolution request");
                resolveName(con, buf);
                return;
            } else if (msgType == 1) {
                // chat request
                Log.debug("masterserver: got chat request");
                chatMsg(con, buf);
            } else {
                Log.warn("masterserver.processPacket: ignoring unknown msg type");
                return;
            }
        } catch (MVRuntimeException e) {
            Log.exception("Masterserver.processPacket got exception", e);
        }
    }

    public void connectionReset(ClientConnection con) {
        Log.debug("Masterserver: connection reset");
    }

    // we got a resolveName request - we assume pointer in buf is ready
    // to read the data portion
    public void resolveName(ClientConnection con, MVByteBuffer buf) {
        String worldName = buf.getString();
        if (Log.loggingDebug)
            Log.debug("masterserver.resolvename: looking up worldName " + worldName);
        MasterDatabase.WorldInfo worldInfo = db.resolveWorldID(worldName);
        String hostname = null;
        int port = -1;
	String patcherURL = null;
	String mediaURL = null;
        if (worldInfo == null) {
            Log.warn("masterserver.resolvename: failed to resolve worldName "
                    + worldName);
        } else {
            hostname = worldInfo.svrHostName;
            port = worldInfo.port;
	    patcherURL = worldInfo.patcherURL;
	    mediaURL = worldInfo.mediaURL;

            if (Log.loggingDebug)
                Log.debug("masterverse.resolvename: resolved worldName " + worldName
                          + " to " + hostname + ":" + port);
        }

        // send data back to user
        MVByteBuffer returnBuf = new MVByteBuffer((hostname == null) ? 200
                : hostname.length() + 32);
        returnBuf.putInt(2); // this is a name resolve response message
        returnBuf.putString(worldName);
        returnBuf.putInt((hostname == null) ? 0 : 1); // status
        returnBuf.putString(hostname);
        returnBuf.putInt(port);
	if (patcherURL != null && mediaURL != null) {
            if (Log.loggingDebug)
                Log.debug("masterverse.resolvename: patcher for worldName " + worldName
                          + " at " + patcherURL + " with media at: " + mediaURL);
	    returnBuf.putString(patcherURL);
	    returnBuf.putString(mediaURL);
	}
        returnBuf.flip();

        con.send(returnBuf);
    }

    public void chatMsg(ClientConnection con, MVByteBuffer buf) {
    }

    int tcpPort = -1;
    int rdpPort = -1;

    MasterDatabase db = null;

    public static MasterServer getMasterServer() {
        if (masterServer == null) {
            masterServer = new MasterServer();
        }
        return masterServer;
    }
    private static MasterServer masterServer = null;

    public static class SocketHandler implements Runnable {
        public SocketHandler(Socket socket, MasterDatabase db) {
            this.clientSocket = socket;
            this.db = db;
        }

        private Socket clientSocket = null;
        private MasterDatabase db = null;
        private Random random = new Random();

        private byte[] generateAuthResponse(String username, String password, byte[] challenge) {
            byte[] keyData = password.getBytes();

            try {
                SecretKeySpec key = new SecretKeySpec(keyData, "HmacSHA1");

                Mac mac = Mac.getInstance(key.getAlgorithm());
                mac.init(key);

                MVByteBuffer buf = new MVByteBuffer(256);
                buf.putString(username);
                buf.putInt(AUTH_PROTOCOL_VERSION);
                buf.putInt(challenge.length);
                buf.putBytes(challenge, 0, challenge.length);
                byte[] data = new byte[buf.position()];
                buf.rewind();
                buf.getBytes(data, 0, data.length);
                Log.debug("dataLen=" + data.length);
                Log.debug("data=" + Base64.encodeBytes(data));
                return mac.doFinal(data);
            }
            catch (NoSuchAlgorithmException e) {
                // should never happen
                Log.exception("SecureTokenManager.generateDomainAuthenticator: bad implementation", e);
                return null;
            }
            catch (InvalidKeyException e) {
                // should never happen
                Log.exception("SecureTokenManager.generateDomainAuthenticator: invalid key", e);
                throw new RuntimeException(e);
            }
            catch (IllegalStateException e) {
                // should never happen
                Log.exception("SecureTokenManager.generateDomainAuthenticator: illegal state", e);
                throw new RuntimeException(e);
            }
        }

        private void handleAuth(DataInputStream in, DataOutputStream out) throws IOException {
            int magicCookie = in.readInt();
            int version = in.readInt();
            Log.debug("cookie=" + magicCookie + " version=" + version);
            if (version != AUTH_PROTOCOL_VERSION) {
                throw new RuntimeException("unsupported version=" + version);
            }
            int usernameLen = in.readInt();
            if (Log.loggingDebug)
                Log.debug("MasterServer.handleAuth: username len=" + usernameLen);
            if (usernameLen > 1000) {
                throw new RuntimeException("username too long, len=" + usernameLen);
            }
            byte[] usernameBuf = new byte[usernameLen];
            in.readFully(usernameBuf);
            String username = new String(usernameBuf);
            if (Log.loggingDebug)
                Log.debug("MasterServer.handleAuth: login username=" + username);

            byte[] challenge = new byte[CHALLENGE_LEN];
            random.nextBytes(challenge);

            out.writeInt(AUTH_PROTOCOL_VERSION);
            out.writeInt(challenge.length);
            out.write(challenge);

            String password = db.getPassword(username);
            byte[] authResponse = null;
            if (password != null) {
                authResponse = generateAuthResponse(username, password, challenge);
            }

            int responseLen = in.readInt();
            byte[] response = new byte[responseLen];
            in.readFully(response);

            Log.debug("password=" + password);
            if (authResponse != null)
                Log.debug("authResponse=" + Base64.encodeBytes(authResponse));
            Log.debug("authenticator=" + Base64.encodeBytes(response));
            if (Arrays.equals(authResponse, response)) {
                // write status
                out.writeInt(1);

                Integer accountId = db.getAccountId(username);
                SecureTokenSpec masterSpec = new SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_MASTER,
                                                                 "master",
                                                                 System.currentTimeMillis() +
                                                                 masterTokenValidTime);
                masterSpec.setProperty("account_id", accountId);
                masterSpec.setProperty("account_name", username);
                byte[] masterToken = SecureTokenManager.getInstance().generateToken(masterSpec);
                Log.debug("tokenLen=" + masterToken.length + " token=" + Base64.encodeBytes(masterToken));
                MVByteBuffer tmpBuf = new MVByteBuffer(16);
                tmpBuf.putInt(~accountId);
                tmpBuf.flip();
                byte[] oldToken = new byte[4];
                tmpBuf.getBytes(oldToken, 0, oldToken.length);

                // write token
                if (masterToken == null) {
                    Log.debug("null token");
                }
                else {
                    Log.debug("tokenLen=" + masterToken.length + " token=" + Base64.encodeBytes(masterToken));
                }
                out.writeInt(masterToken.length);
                out.write(masterToken);
                out.writeInt(oldToken.length);
                out.write(oldToken);
            }
            else {
                // write status
                out.writeInt(0);
                // no token
                out.writeInt(0);
                // no old token
                out.writeInt(0);
            }
        }

        private void handleOldStyleAuth(DataInputStream in, DataOutputStream out) throws IOException {
            int usernameLen = in.readInt();
            if (Log.loggingDebug)
                Log.debug("masterserver: username len=" + usernameLen);
            if (usernameLen > 1000) {
                throw new RuntimeException("username too long, len=" + usernameLen);
            }
            byte[] nameBuf = new byte[usernameLen];
            in.readFully(nameBuf);
            String username = new String(nameBuf);
            if (Log.loggingDebug)
                Log.debug("masterserver: login username=" + username);

            // read in password
            int passwordLen = in.readInt();
            byte[] passwordBuf = new byte[passwordLen];
            in.readFully(passwordBuf);
            String password = new String(passwordBuf);
            if (Log.loggingDebug)
                Log.debug("login info: password=" + password);

            // check database
            int uid = db.MVAcctPasswdCheck(username, password);
            if (uid == -1) {
                Log.warn("MasterServer: password check failed for username " + username);
            } else {
                if (Log.loggingDebug)
                    Log.debug("MasterServer: password verified, uid=" + uid + ", token=" + ~uid);
            }

            // send success or failure
            out.writeInt((uid == -1) ? 0 : 1);

            // send result back to the client
            out.writeInt(4);
            out.writeInt(~uid);
        }

        public void run() {
            try {
                BufferedInputStream bufferedIn = new BufferedInputStream(clientSocket.getInputStream());
                DataInputStream in = new DataInputStream(bufferedIn);
                DataOutputStream out = new DataOutputStream(clientSocket.getOutputStream());

                if (!in.markSupported()) {
                    throw new RuntimeException("MasterServer.run: cannot use mark/reset on input stream");
                }
                // read in username
                in.mark(4);
                int magicCookie = in.readInt();
                in.reset();
                if (magicCookie == 0xffffffff) {
                    handleAuth(in, out);
                }
                else {
                    handleOldStyleAuth(in, out);
                }
            } catch (Exception e) {
                Log.exception("MasterServer.run caught exception", e);
            } finally {
                try {
                    clientSocket.close();
                    if (Log.loggingDebug)
                        Log.debug("SocketHandler: closed socket: " + clientSocket);
                } catch(Exception e) {
                }
            }
        }
    }

    /**
     * Gets the database type - default is "mysql".
     * @return The database type.
     */
    public static String getDBType() {
	String dbtype = properties.getProperty("multiverse.db_type");
	if (dbtype == null)
	    return "mysql";
	else
	    return dbtype;
    }

    /**
     * Gets the JDBC connection string (URL).
     * @return The JDBC connection string.
     */
    public static String getDBUrl() {
	String url = properties.getProperty("multiverse.db_url");
	if (url == null)
	    url = "jdbc:" + getDBType() + "://" + getDBHostname() + "/" + getDBName(); 
	return url;
    }

    /**
     * Gets the database user name.
     * @return The database user name.
     */
    public static String getDBUser() {
	return properties.getProperty("multiverse.db_user");
    }

    /**
     * Gets the database password.
     * @return The database password.
     */
    public static String getDBPassword() {
	return properties.getProperty("multiverse.db_password");
    }

    /**
     * Gets The database host name.
     * @return The database host name.
     */
    public static String getDBHostname() {
	return properties.getProperty("multiverse.db_hostname");
    }

    /**
     * Gets the database name - default is "multiverse".
     * @return The database name.
     */
    public static String getDBName() {
	String dbname = properties.getProperty("multiverse.db_name");
	if (dbname == null) {
	    return "multiverse";
	} else {
	    return dbname;
	}
    }
    private static ExecutorService threadPool = Executors.newCachedThreadPool();
    
    public static void main(String args[]) {
        if (args.length != 1) {
            Log.error("specify script file");
            System.exit(1);
        }

        try {
            properties = InitLogAndPid.initLogAndPid(args);

            byte[] domainKey = SecureTokenUtil.encodeDomainKey(1, SecureTokenUtil.generateDomainKey());
            SecureTokenManager.getInstance().initDomain(domainKey);

            MasterServer ms = MasterServer.getMasterServer();

            // execute script
            String scriptFilename = args[0];
            ScriptManager scriptManager = new ScriptManager();
            scriptManager.init();
            if (Log.loggingDebug)
                Log.debug("Executing script file: " + scriptFilename);
            scriptManager.runFile(scriptFilename);
            Log.debug("script completed");

            // connect to the database
            ms.dbConnect();

            // start up the rdp server

            String log_rdp_counters =
                properties.getProperty("multiverse.log_rdp_counters");
            if (log_rdp_counters == null || log_rdp_counters.equals("false"))
                RDPServer.setCounterLogging(false);

            RDPServerSocket serverSocket = null;
            serverSocket = new RDPServerSocket();
            RDPServer.startRDPServer();
            serverSocket.registerAcceptCallback(masterServer);
            serverSocket.bind(ms.getRDPPort());
            Log.info("masterserver: rdp on port " + ms.getRDPPort());
            clientTCPMessageIO = ClientTCPMessageIO.setup(ms.getRDPPort(), masterServer);
            clientTCPMessageIO.start();

            // start up the server socket
            Log.info("masterserver: tcp on port " + ms.getTCPPort());
            ServerSocket socket = new ServerSocket(ms.getTCPPort());
            if (Log.loggingDebug)
                Log.debug("masterserver: tcp server listening on port "
                          + ms.getTCPPort());
            while (true) {
                Socket clientSocket = socket.accept();
                threadPool.execute(new SocketHandler(clientSocket, getMasterServer().db));
            }

        } catch (Exception e) {
            Log.exception("MasterServer.main caught exception", e);
            System.exit(1);
        }
        Log.info("connected to database");
    }
    
    private static ClientTCPMessageIO clientTCPMessageIO = null;

    public static long masterTokenValidTime = 120000L;

    private static final int CHALLENGE_LEN = 20;

    public static final int AUTH_PROTOCOL_VERSION = 1;

    public static final int defaultTcpPort = 9005;
    public static final int defaultRdpPort = 9010;

    /**
     * The Properties instance, typically read from file
     * $MV_HOME/bin/master.properties
     */
    public static Properties properties = new Properties();
}
