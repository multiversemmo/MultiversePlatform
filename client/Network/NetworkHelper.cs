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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Axiom.MathLib;

// using Multiverse.Gui;
using Multiverse.Config;
using Multiverse.MathLib;
using Multiverse.Network.Rdp;
using Multiverse.Lib.LogUtil;

#endregion

// Maximum udp datagram size is 548 bytes

namespace Multiverse.Network
{
    public class LoginSettings
    {
		public string tcpServer;
		public short tcpPort = 9005;
		public string rdpServer;
		public short rdpPort = 9010;
		public string username;
		public string password;
        public string loginUrl;
		public string worldId;
        public long characterId = 0;

		public override string ToString() {
            return string.Format("{0}:{1} {2}/*****", tcpServer, tcpPort, username);
        }
    }

    public class CharacterEntry : Dictionary<string, object> {
        public CharacterEntry(Dictionary<string, object> other)
            : base(other) {
        }
        public long CharacterId {
            get {
                return (long)this["characterId"];
            }
        }
        public string Hostname {
            get {
                return this.ContainsKey("hostname") ? (string)this["hostname"] : null;
            }
        }
        public int Port {
            get {
                return this.ContainsKey("port") ? (int)this["port"] : 0;
            }
        }
        public bool Status {
            get {
                if (!this.ContainsKey("status"))
                    return true;
                return (bool)this["status"];
            }
        }
    }

	public class WorldServerEntry
	{
        private static string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // private static string CommonAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static string ClientAppDataFolder = Path.Combine(MyDocumentsFolder, "Multiverse World Browser");
        public static string WorldsFolder = Path.Combine(ClientAppDataFolder, "Worlds");

        // At some point, I should change updateUrl to be a list of update 
        // urls and the associated repository name.  This allows shared
        // universe data.  A different approach is to change the way I store
        // assets, so we share assets that are used by multiple worlds.  In
        // that model, worlds in a universe would need to have the common 
        // files and keep them up to date.
		string worldName;
        /// <summary>
        ///   This is the hostname for the tcp login server for this world
        /// </summary>
		string hostname;
        /// <summary>
        ///   This is the port for the tcp login server for this world
        /// </summary>
		int port;
        /// <summary>
        ///   This is the url of the html that drives the world patch process.
        /// </summary>
        string patcherUrl;
        /// <summary>
        ///   This is the url of the world media files.
        /// </summary>
        string updateUrl;
        /// <summary>
        ///   This is a flag that indicates whether we should connect to a world 
        ///   server for this world.  By default, it is false.
        /// </summary>
        bool standalone;
        /// <summary>
        ///   This flag indicates whether we should go and connect to the world 
        ///   server, or just hang out at the login server.
        /// </summary>
        // bool connectToWorld; (unused)
        /// <summary>
        ///    This is used to override the default repository for the world.
        ///    It can be left null (the default), or it will be used as a 
        ///    replacement for the world name when we generate the path to
        ///    our world media.
        /// </summary>
        List<string> worldRepositoryDirectories;
        /// <summary>
        ///   Some worlds may need a startup script
        /// </summary>
        string startupScript;
        
		public WorldServerEntry(string worldName, string hostname, int port, string patcherUrl, string updateUrl) {
			this.worldName = worldName;
			this.hostname = hostname;
			this.port = port;
            this.patcherUrl = patcherUrl;
            this.updateUrl = updateUrl;
            this.standalone = false;
            this.worldRepositoryDirectories = null;
            this.startupScript = null;
		}

        public override string ToString() {
            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("WorldName: {0}\n", worldName);
            buffer.AppendFormat("Hostname: {0}\n", hostname);
            buffer.AppendFormat("Port: {0}\n", port);
            buffer.AppendFormat("PatcherUrl: {0}\n", patcherUrl);
            buffer.AppendFormat("UpdateUrl: {0}", updateUrl);
            return buffer.ToString();
        }

		public string WorldName {
			get {
				return worldName;
			}
		}
		public string Hostname {
			get {
				return hostname;
			}
		}
		public int Port {
			get {
				return port;
			}
		}
        public string PatcherUrl {
            get {
                return patcherUrl;
            }
            set {
                patcherUrl = value;
            }
        }
        public string UpdateUrl {
            get {
                return updateUrl;
            }
            set {
                updateUrl = value;
            }
        }
        public bool Standalone {
            get {
                return standalone;
            }
            set {
                standalone = value;
            }
        }
        public string WorldRepository {
            get {
                return WorldRepositoryDirectories[0];
            }
        }
        public List<string> WorldRepositoryDirectories {
            get {
                // Do we have a valid list of repository directories?
                if (worldRepositoryDirectories != null && worldRepositoryDirectories.Count > 0)
                    return worldRepositoryDirectories;
                // Construct a valid list based on our world name
                List<string> rv = new List<string>();
                rv.Add(Path.Combine(WorldsFolder, worldName));
                return rv;
            }
            set {
                worldRepositoryDirectories = value;
            }
        }
        public string StartupScript {
            get {
                return startupScript;
            }
            set {
                startupScript = value;
            }
        }
	}

	public enum NetworkHelperStatus {
        Success,
        LoginFailure,
		WorldConnectFailure,
		MasterConnectFailure,
		WorldTcpConnectFailure,
		MasterTcpConnectFailure,
        WorldResolveSuccess,
		WorldResolveFailure,
        Standalone,
        UnsupportedClientVersion,
        Unknown,
    }

    public enum LoginStatus
    {
        InvalidPassword = 0,
        Success         = 1,
        AlreadyLoggedIn = 2,
        ServerError     = 3
    }

    public delegate byte[] AsyncDelegate(ref IPEndPoint remoteEP);

	// The RdpMessageHandler and TcpWorldMessageHandler both implement this
	// interface
    public interface IMessageHandler : IDisposable
    {
        void SendMessage(OutgoingMessage outMessage);
        void BeginListen();

        long PacketsSentCounter
        {
            get;
        }
        long PacketsReceivedCounter
        {
            get;
        }
        long BytesSentCounter
        {
            get;
        }
        long BytesReceivedCounter
        {
            get;
        }
	}
    
    public abstract class RdpMessageHandler : IMessageHandler, IDisposable
	{
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RdpMessageHandler));

        private RdpClient rdp;
		private AsyncCallback pfnCallBack;
		private AsyncDelegate rdpDelegate;
		private RdpConnection rdpConn;

        public RdpMessageHandler(IPEndPoint remote, int localPort, int millisecondsTimeout) {
			rdp = new RdpClient(localPort);
			try {
                rdpConn = rdp.Connect(remote, millisecondsTimeout);
			} catch (Exception) {
				if (rdp != null)
					rdp.Dispose();
				throw;
			}
		}

        public RdpMessageHandler(IPEndPoint remote, int millisecondsTimeout) {
			Random r = new Random();
            // Make up to 20 attempts (guessing a random port)
            for (int i = 0; i < 20; ++i) {
                int localPort = 5000 + (r.Next() % 1000);
                try {
                    rdp = new RdpClient(localPort);
                    log.InfoFormat("Setting up RdpClient with localPort = {0}", localPort);
                } catch (Exception) {
                    continue;
                }
                if (rdp != null)
                    break;
            }
            if (rdp == null) {
                log.Warn("Unable to establish local port for RDP");
                throw new Exception("Unable to establish local port for RDP");
            }
			try {
                rdpConn = rdp.Connect(remote, millisecondsTimeout);
			} catch (Exception) {
				if (rdp != null)
					rdp.Dispose();
				throw;
			}
		}

		#region low level packet callback stuff

		/// <summary>
		///   Wait loop for the reliable UDP data
		/// </summary>
		private void WaitForRDPData() {
			// now start to listen for any data...
			rdpDelegate = new AsyncDelegate(rdpConn.Receive);
			IPEndPoint remoteIpEndPoint = null;
			IAsyncResult asyncResult =
				rdpDelegate.BeginInvoke(ref remoteIpEndPoint, this.OnRDPDataReceived, null);
		}

		/// <summary>
		///   Callback for the reliable UDP data
		/// </summary>
		/// <param name="asyn"></param>
		private void OnRDPDataReceived(IAsyncResult asyn) {
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] buf = rdpDelegate.EndInvoke(ref remoteIpEndPoint, asyn);
            // Check to see if we got disposed.  If we did, don't bother 
            // handling this message, and don't wait for more.
            if (rdpConn == null)
                return;
            try {
				HandleMessage(buf, remoteIpEndPoint);
			} catch (Exception e) {
				LogUtil.ExceptionLog.ErrorFormat("Exception: {0}", e);
				throw e;
			}
			WaitForRDPData();
		}

		#endregion // low level packet stuff

		public void BeginListen() {
			pfnCallBack = new AsyncCallback(OnRDPDataReceived);
			WaitForRDPData();
		}
		public void SendMessage(OutgoingMessage outMessage) {
            if (NetworkHelper.LogMessageContents) {
                byte[] buf = outMessage.GetBytes();
                log.DebugFormat("RdpMessageHandler.Message: length {0}, packet {1}", 
                    buf.Length, NetworkHelper.ByteArrayToHexString(buf, 0, buf.Length));
            }
            outMessage.Send(rdpConn);
		}

		public void Dispose() {
			if (rdpConn != null) {
                try {
                    if (!rdpConn.IsClosed) {
                        rdpConn.Close();
                    }
                } catch (Exception) {
                    // no throws from dispose
                }
                rdpConn = null;
			}
			if (rdp != null)
				rdp.Dispose();
		}

		/// <summary>
		///   Method to deal with incoming messages
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="endpoint"></param>
        protected abstract void HandleMessage(byte[] buf, IPEndPoint endpoint);

		public long PacketsSentCounter {
			get {
				if (rdpConn == null)
					return 0;
				return rdpConn.PacketsSentCounter;
			}
		}
		public long PacketsReceivedCounter {
			get {
				if (rdpConn == null)
					return 0;
				return rdpConn.PacketsReceivedCounter;
			}
		}
		public long BytesSentCounter {
			get {
				if (rdpConn == null)
					return 0;
				return rdpConn.BytesSentCounter;
			}
		}
		public long BytesReceivedCounter {
			get {
				if (rdpConn == null)
					return 0;
				return rdpConn.BytesReceivedCounter;
			}
		}
	}

    public class RdpWorldMessageHandler : RdpMessageHandler
	{
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RdpWorldMessageHandler));

		// const int LocalRdpPort = 6010;
		MessageDispatcher dispatcher = null;
        const int ConnectTimeoutMillis = 10000;

		public RdpWorldMessageHandler(IPEndPoint remote, MessageDispatcher dispatcher) :
            base(remote, ConnectTimeoutMillis) 
		{
			this.dispatcher = dispatcher;
		}

		public RdpWorldMessageHandler(IPEndPoint remote) :
            base(remote, ConnectTimeoutMillis)
		{
		}

        protected override void HandleMessage(byte[] buffer, IPEndPoint remoteIpEndPoint) {
            if (NetworkHelper.LogMessageContents)
                log.DebugFormat("RdpWorldMessageHandler.HandleMessage: length {0}, packet {1}", 
                    buffer.Length, NetworkHelper.ByteArrayToHexString(buffer, 0, buffer.Length));
            if (buffer.Length == 0)
                // probably a nul packet
                return;
            if (buffer.Length < 8) {
                log.ErrorFormat("Invalid message length: {0}", buffer.Length);
                return;
            }
            IncomingMessage inMessage =
                new IncomingMessage(buffer, 0, buffer.Length, remoteIpEndPoint);
            BaseWorldMessage message = WorldMessageFactory.ReadMessage(inMessage);
            if (message == null) {
                log.Warn("Failed to read message from factory");
                return;
            }
            log.DebugFormat("World message type: {0}; Oid: {1}", message.MessageType, message.Oid);
            if (dispatcher != null)
                dispatcher.QueueMessage(message);
            else
                MessageDispatcher.Instance.QueueMessage(message);
        }        
	}

    public class TcpWorldMessageHandler : IMessageHandler
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TcpWorldMessageHandler));
        
        private TcpClient tcpClient;
		MessageDispatcher dispatcher = null;
        IPEndPoint remoteEndPoint;
        private int receiveBufferSize = 32 * 1024;
        // The count of valid bytes in the buffer.  To get the offset
        // where we add bytes, add this to receiveBufferOffset
        private int bytesInReceiveBuffer = 0;
        // The offset at which we will _read_ the next bytes
        private int receiveBufferOffset = 0;
        // private int receiveBufferStartOfMessage = 0; (unused)
        private byte[] receiveBuffer;
        private byte[] currentMsgBuf;
        private int currentMsgOffset = 0;

		private int packetsSentCounter = 0;
		private int packetsReceivedCounter = 0;
		private int bytesSentCounter = 0;
		private int bytesReceivedCounter = 0;

        public TcpWorldMessageHandler(IPEndPoint remote, MessageDispatcher dispatcher) {
            remoteEndPoint = remote;
            Init();
            // This constructor arbitrarily assigns the local port number.
            tcpClient = new TcpClient();
            try {
                tcpClient.Connect(remote.Address, remote.Port);
            } catch (Exception e) {
                LogUtil.ExceptionLog.Warn(e.ToString());
                tcpClient.Close();
                return;
            }
            tcpClient.Client.Blocking = false;
            tcpClient.Client.NoDelay = true;
        }
        
        private void Init() {
            receiveBuffer = new byte[receiveBufferSize];
        }

        public void SendMessage(OutgoingMessage outMessage) {
            if (tcpClient == null || tcpClient.Client == null)
                return;
            packetsSentCounter++;
            byte[] msgBytes = outMessage.GetBytes();
            int length = msgBytes.Length;
            if (NetworkHelper.LogMessageContents)
                log.DebugFormat("TcpWorldMessageHandler.SendMessage: length {0}, packet {1}", 
                    length, NetworkHelper.ByteArrayToHexString(msgBytes, 0, length));
            byte[] msgBytesWithLength = new byte[length + 4];
            int cnt = 0;
            for (int i = 3; i >= 0; i--)
                msgBytesWithLength[cnt++] = (byte)((length >> (8 * i)) & 0xff);
            Array.Copy(msgBytes, 0, msgBytesWithLength, 4, length);
            try {
                tcpClient.Client.Send(msgBytesWithLength);
			} catch (Exception e) {
				LogUtil.ExceptionLog.Error("In TcpWorldMessageHandler.SendMessage, exception: {0}", e);
                if (tcpClient != null)
                    tcpClient.Client = null;
                return;
            }
            bytesSentCounter += length + 4;
        }

        protected void HandleMessage(byte[] buf, IPEndPoint endpoint) {
            if (NetworkHelper.LogMessageContents)
                log.DebugFormat("TcpWorldMessageHandler.HandleMessage: length {0}, packet {1}", 
                    buf.Length, NetworkHelper.ByteArrayToHexString(buf, 0, buf.Length));
            if (buf.Length == 0)
                // probably a nul packet
                return;
            if (buf.Length < 8) {
                log.ErrorFormat("Invalid message length: {0}", buf.Length);
                return;
            }
            packetsReceivedCounter++;
            IncomingMessage inMessage =
                new IncomingMessage(buf, 0, buf.Length, remoteEndPoint);
            BaseWorldMessage message = WorldMessageFactory.ReadMessage(inMessage);
            if (message == null) {
                log.Warn("Failed to read message from factory");
                return;
            }
            log.InfoFormat("World message type: {0}; Oid: {1}", message.MessageType, message.Oid);
            if (dispatcher != null)
                dispatcher.QueueMessage(message);
            else
                MessageDispatcher.Instance.QueueMessage(message);
        }

        public void BeginListen() {
            WaitForTcpData();
        }
        
		#region low level packet callback stuff

        private int GetMessageLength(int offset) {
            int length = 0;
            for (int i = 3; i >= 0; i--)
                length = length | (receiveBuffer[offset++] << (8 * i));
            return length;
        }
        
        private void WaitForTcpData() {
            // Begin receiving the data from the remote device.
            try {
                if (tcpClient == null || tcpClient.Client == null)
                    return;
                tcpClient.Client.BeginReceive(receiveBuffer, bytesInReceiveBuffer,
                    receiveBufferSize - bytesInReceiveBuffer, 0,
                    new AsyncCallback(OnTCPDataReceived), null);
			} catch (Exception e) {
				LogUtil.ExceptionLog.ErrorFormat("In TcpWorldMessageHandler.WaitForTcpData, exception: {0}", e);
                if (tcpClient != null)
                    tcpClient.Client = null;
                NetworkHelper.Instance.RequestShutdown("TCP Connection closed by the server.  Exception was " + e.Message);
                return;
            }
        }

        private void AdvanceReceiveBuffer(int amount) {
            bytesInReceiveBuffer -= amount;
            receiveBufferOffset += amount;
        }
        
        /// <summary>
		///   Callback for the TCP data
		/// </summary>
		/// <param name="asyn"></param>
		private void OnTCPDataReceived(IAsyncResult asyn) {
            int bytesReceived;
            try {
                try {
                    if (tcpClient == null || tcpClient.Client == null)
                        return;
                    bytesReceived = tcpClient.Client.EndReceive(asyn);
                } catch (Exception e) {
                    LogUtil.ExceptionLog.ErrorFormat("In TcpWorldMessageHandler.OnTCPDataReceived, exception: {0}", e);
                    if (tcpClient != null)
                        tcpClient.Client = null;
                    NetworkHelper.Instance.RequestShutdown("TCP Connection closed by the server.  Exception was " + e.Message);
                    return;
                }
                bytesReceivedCounter += bytesReceived;
                bytesInReceiveBuffer += bytesReceived;
                // If we are still building a message from the last read
                if (currentMsgBuf != null) {
                    int bufLeft = currentMsgBuf.Length - currentMsgOffset;
                    int amountToCopy = Math.Min(bytesInReceiveBuffer, bufLeft);
                    Array.Copy(receiveBuffer, receiveBufferOffset, currentMsgBuf, currentMsgOffset, amountToCopy);
                    AdvanceReceiveBuffer(amountToCopy);
                    currentMsgOffset += amountToCopy;
                    if (amountToCopy == bufLeft) {
                        // We have a complete message
                        HandleMessage(currentMsgBuf, remoteEndPoint);
                        currentMsgBuf = null;
                    }
                }
                while (bytesInReceiveBuffer >= 4) {
                    // Get the message length
                    int length = GetMessageLength(receiveBufferOffset);
                    AdvanceReceiveBuffer(4);
                    byte[] buf = new byte[length];
                    if (bytesInReceiveBuffer >= length) {
                        Array.Copy(receiveBuffer, receiveBufferOffset, buf, 0, length);
                        HandleMessage(buf, remoteEndPoint);
                        AdvanceReceiveBuffer(length);
                    }
                    else {
                        // We didn't get all the bytes required for this message
                        currentMsgBuf = buf;
                        currentMsgOffset = bytesInReceiveBuffer;
                        Array.Copy(receiveBuffer, receiveBufferOffset, buf, 0, bytesInReceiveBuffer);
                        AdvanceReceiveBuffer(bytesInReceiveBuffer);
                    }
                }
                if (bytesInReceiveBuffer == 0)
                    receiveBufferOffset = 0;
                else {
                    if (bytesInReceiveBuffer < 0 || bytesInReceiveBuffer > 3)
                        log.ErrorFormat("TcpWorldMessageHandler.OnTCPDataReceived: Bytes left illegal!  left {0} bytesReceived {1} receiveBufferOffset {2} currentMsgBuf {3}",
                            bytesInReceiveBuffer, bytesInReceiveBuffer, receiveBufferOffset, currentMsgBuf);
                    else {
                        Array.Copy(receiveBuffer, receiveBufferOffset, receiveBuffer, 0, bytesInReceiveBuffer);
                        receiveBufferOffset = 0;
                    }
                }
            }
            catch (Exception e) {
                log.Error("TcpWorldMessageHandler.OnTcpDataReceived: Exception " + e.Message + "; stack trace\n" + e.StackTrace);
            }
            WaitForTcpData();
        }

		#endregion // low level packet stuff

		public void Dispose() {
			if (tcpClient != null) {
                try {
                    tcpClient.Close();
                } catch (Exception) {
                    // no throws from dispose
                }
                tcpClient = null;
			}
		}

		public long PacketsSentCounter {
			get {
				if (tcpClient == null)
					return 0;
				return packetsSentCounter;
			}
		}
		public long PacketsReceivedCounter {
			get {
				if (tcpClient == null)
					return 0;
				return packetsReceivedCounter;
			}
		}
		public long BytesSentCounter {
			get {
				if (tcpClient == null)
					return 0;
				return bytesSentCounter;
			}
		}
		public long BytesReceivedCounter {
			get {
				if (tcpClient == null)
					return 0;
				return bytesReceivedCounter;
			}
		}
    }
    
    public class RdpMasterMessageHandler : RdpMessageHandler
	{
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RdpMasterMessageHandler));
        // const int LocalRdpPort = 6005;
		Dictionary<string, WorldServerEntry> worldServerMap;
        const int ConnectTimeoutMillis = 10000;


		public RdpMasterMessageHandler(IPEndPoint remote, 
									   Dictionary<string, WorldServerEntry> worldServerMap) :
            base(remote, ConnectTimeoutMillis)
		{
			this.worldServerMap = worldServerMap;
		}

		protected override void HandleMessage(byte[] buffer, IPEndPoint remoteIpEndPoint) {
            if (buffer.Length == 0)
                // probably a nul packet
                return;
			if (buffer.Length < 8) {
				log.ErrorFormat("Invalid message length: {0}", buffer.Length);
				return;
			}
			IncomingMessage inMessage =
				new IncomingMessage(buffer, 0, buffer.Length, remoteIpEndPoint);
			BaseMasterMessage message = MasterMessageFactory.ReadMessage(inMessage);
			if (message == null)
				return;
			switch (message.MessageType) {
				case MasterMessageType.ResolveResponse: {
						ResolveResponseMessage rv = (ResolveResponseMessage)message;
						if (rv.Status) {
                            WorldServerEntry entry = 
                                new WorldServerEntry(rv.WorldName, rv.Hostname, rv.Port, rv.PatcherUrl, rv.UpdateUrl);
							Monitor.Enter(worldServerMap);
							try {
								worldServerMap[rv.WorldName] = entry;
								Monitor.PulseAll(worldServerMap);
							} finally {
								Monitor.Exit(worldServerMap);
							}
						} else {
							log.ErrorFormat("Failed to resolve world name: {0}", rv.WorldName);
						}
					}
					break;
				default:
					log.WarnFormat("Unknown message type from master server: {0}", message.MessageType);
					break;
			}
		}
	}

	public class NetworkHelper 
    {
        private static NetworkHelper instance = null;
        
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(NetworkHelper));
        private static readonly log4net.ILog log_status = log4net.LogManager.GetLogger("Network Information");

		/// <summary>
		///   timeOffset = Client time (as a long) - Server time (as a ushort)
		/// </summary>
		private long timeOffset;

        /// <summary>
        ///   Token that should be submitted to the login server for authorization
        /// </summary>
		byte[] masterToken;
        /// <summary>
        ///   Old style token that should be submitted to pre-2.0 login server for authorization
        /// </summary>
        byte[] oldToken;
        /// <summary>
        ///   Token that should be submitted to the proxy server for authorization
        /// </summary>
        byte[] worldToken;
        /// <summary>
        ///   Id of world that is currently connected
        /// </summary>
        public string connectedWorldId;

		private IWorldManager worldManager;
        // private Client client;

		private Dictionary<string, WorldServerEntry> worldServerMap;
        private List<CharacterEntry> characterEntries;
        private string loginPluginHost;

        string proxyPluginHost;
        int proxyPluginPort;

		IMessageHandler messageHandlerWorld;
		RdpMessageHandler rdpMaster;

        /// <summary>
        ///   Should this client use TCP to connect to the proxy?
        /// </summary>
        private bool useTCP = false;

        protected TcpClient tcpWorldConnection = null;
        
        /// <summary>
        ///   Set this to true to log the contents of all messages.
        ///   The log must be at the debug level to see the messages.
        /// </summary>
        private static bool logMessageContents = false;

        public static string ByteArrayToHexString(byte[] inBuf, int startingByte, int length)
        {
            byte ch = 0x00;
            string[] pseudo = {"0", "1", "2",
                               "3", "4", "5", "6", "7", "8",
                               "9", "A", "B", "C", "D", "E",
                               "F"};
            StringBuilder outBuf = new StringBuilder(length * 2);
            StringBuilder charBuf = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                byte b = inBuf[i + startingByte];
                ch = (byte)(b & 0xF0);  // Strip off high nibble
                ch = (byte)(ch >> 4);                         // shift the bits down
                ch = (byte)(ch & 0x0F);                       // must do this if high order bit is on!
                outBuf.Append(pseudo[(int)ch]);                // convert the nibble to a String Character
                ch = (byte)(b & 0x0F);  // Strip off low nibble 
                outBuf.Append(pseudo[(int)ch]);              // convert the nibble to a String Character
                if (b >= 32 && b <= 126)
                    charBuf.Append((char)b);
                else
                    charBuf.Append("*");
            }
            return outBuf + " == " + charBuf;
        } 

        #region Send methods

        /// <summary>
        ///   General purpose send message method that can send any type of message
        ///   This used to be private, but it is now public, so that plugins can
        ///   use this method.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(BaseWorldMessage message) {
            message.Oid = worldManager.PlayerId;
            if (messageHandlerWorld != null)
                messageHandlerWorld.SendMessage(message.CreateMessage());
        }

		public void SendOrientationMessage(Quaternion orientation) {
			OrientationMessage message = new OrientationMessage();
            message.Orientation = orientation;

			SendMessage(message);
		}

		public void SendDirectionMessage(long timestamp, Vector3 dir, Vector3 pos) {
			DirectionMessage message = new DirectionMessage();
			message.Timestamp = GetServerTimestamp(timestamp);
			message.Direction = dir;
			message.Location = pos;

			SendMessage(message);
		}

		public void SendDirLocOrientMessage(long timestamp, Vector3 dir, Vector3 pos, Quaternion orientation) {
			DirLocOrientMessage message = new DirLocOrientMessage();
			message.Timestamp = GetServerTimestamp(timestamp);
			message.Direction = dir;
			message.Location = pos;
            message.Orientation = orientation;

            SendMessage(message);
        }
        
        public void SendCommMessage(string text) {
			CommMessage message = new CommMessage();
			message.ChannelId = (int)CommChannel.Say;
			message.Message = text;

			SendMessage(message);
		}

		public void SendAcquireMessage(long objectId) {
			AcquireMessage message = new AcquireMessage();
			message.ObjectId = objectId;
			SendMessage(message);
		}

		public void SendEquipMessage(long objectId, string slotName) {
			EquipMessage message = new EquipMessage();
			message.ObjectId = objectId;
			message.SlotName = slotName;
			SendMessage(message);
		}

		public void SendAttackMessage(long objectId, string attackType, bool attackStatus) {
			AutoAttackMessage message = new AutoAttackMessage();
			message.ObjectId = objectId;
			message.AttackType = attackType;
			message.AttackStatus = attackStatus;
			SendMessage(message);
		}

		public void SendLogoutMessage() {
			LogoutMessage message = new LogoutMessage();
			SendMessage(message);
		}

		public void SendTargettedCommand(long objectId, string text) {
			CommandMessage message = new CommandMessage();
			message.ObjectId = objectId;
			message.Command = text;

			SendMessage(message);
		}

		public void SendQuestInfoRequestMessage(long objectId) {
			QuestInfoRequestMessage message = new QuestInfoRequestMessage();
			message.ObjectId = objectId;

			SendMessage(message);
		}

		public void SendQuestResponseMessage(long objectId, long questId, bool accepted) {
			QuestResponseMessage message = new QuestResponseMessage();
			message.ObjectId = objectId;
			message.QuestId = questId;
			message.Accepted = accepted;

			SendMessage(message);
		}

		public void SendQuestConcludeRequestMessage(long objectId) {
			QuestConcludeRequestMessage message = new QuestConcludeRequestMessage();
			message.ObjectId = objectId;

			SendMessage(message);
		}

        public void SendTradeOffer(long playerId, long partnerId, List<long> itemIds, bool accepted, bool cancelled) {
            TradeOfferRequestMessage message = new TradeOfferRequestMessage();
            message.Oid = playerId;
            message.ObjectId = partnerId;
            message.Offer = itemIds;
            message.Accepted = accepted;
            message.Cancelled = cancelled;

            SendMessage(message);
        }

        public void SendActivateItemMessage(long itemId, long objectId)
        {
            ActivateItemMessage message = new ActivateItemMessage();
            message.ItemId = itemId;
            message.ObjectId = objectId;

            SendMessage(message);
        }

        #endregion // send methods

        public void RequestShutdown(string message) {
            worldManager.RequestShutdown(message);
        }

        /// <summary>
        ///   This call requires the tcp world connection to be established
        /// </summary>
        /// <param name="characterProperties"></param>
        /// <returns></returns>
        public CharacterEntry CreateCharacter(Dictionary<string, object> characterProperties) {
            // Make sure we have a connection
            Debug.Assert(tcpWorldConnection != null);
            
            // Send the character creation message
            WorldCharacterCreateMessage createMessage = new WorldCharacterCreateMessage();
            createMessage.Properties = characterProperties;
            OutgoingMessage outMessage = createMessage.CreateMessage();
            outMessage.Send(tcpWorldConnection);

            // Read a message off of our tcp stream
            IncomingMessage inMessage = new IncomingMessage(tcpWorldConnection.GetStream());
            BaseWorldTcpMessage response = WorldTcpMessageFactory.ReadMessage(inMessage);
            WorldCharacterCreateResponseMessage createResponse = response as WorldCharacterCreateResponseMessage;
            if (createResponse == null)
                throw new Exception("Invalid response to world tcp server character create");

            CharacterEntry newCharacter = new CharacterEntry(createResponse.Properties);
            if (!newCharacter.Status)
                return newCharacter;
            // Update our list of character entries
            characterEntries.RemoveAll(delegate(CharacterEntry entry) { return entry.CharacterId == newCharacter.CharacterId; });
            characterEntries.Add(newCharacter);

            return newCharacter;
        }
        
        /// <summary>
        ///   This call requires the tcp world connection to be established
        /// </summary>
        /// <param name="characterProperties"></param>
        /// <returns></returns>
        public void DeleteCharacter(Dictionary<string, object> characterProperties) {
            // Make sure we have a connection
            Debug.Assert(tcpWorldConnection != null);

            // Send the character deletion message
            WorldCharacterDeleteMessage deleteMessage = new WorldCharacterDeleteMessage();
            deleteMessage.Properties = characterProperties;
            OutgoingMessage outMessage = deleteMessage.CreateMessage();
            outMessage.Send(tcpWorldConnection);

            // Read a message off of our tcp stream
            IncomingMessage inMessage = new IncomingMessage(tcpWorldConnection.GetStream());
            BaseWorldTcpMessage response = WorldTcpMessageFactory.ReadMessage(inMessage);
            WorldCharacterDeleteResponseMessage deleteResponse = response as WorldCharacterDeleteResponseMessage;
            if (deleteResponse == null)
                throw new Exception("Invalid response to world tcp server character deletion");

            long characterId = (long)deleteResponse.Properties["characterId"];
            // Update our list of character entries
            characterEntries.RemoveAll(delegate(CharacterEntry entry) { return entry.CharacterId == characterId; });
        }

		/// <summary>
        /// Return a time adjusted with respect to the WorldManager's
        /// current time
        /// </summary>
        public long AdjustTimestamp(long timestamp) {
            return AdjustTimestamp(timestamp, worldManager.CurrentTimeValue);
        }
        
        /// <summary>
		///   Get the time in our local time system (instead of server time), 
		///   and update the time offset if neccessary.
		/// </summary>
		/// <param name="timestamp">timestamp from the server</param>
		/// <param name="now">client's current time</param>
		/// <returns>timestamp in client time</returns>
		public long AdjustTimestamp(long timestamp, long now)
		{
			// Adjust this so that this is the adjusted time of the message
			long clientTimestamp = timeOffset + timestamp;
			// If the time is off by more than 30 seconds, update the offset.
			long newOffset = now - timestamp;
			if (Math.Abs(clientTimestamp - now) > 30 * 1000)
			{
				log.InfoFormat("Increasing networkHelper.TimeOffset from {0} to {1}",
						       timeOffset, newOffset);
				timeOffset = newOffset;
				clientTimestamp = now;
			}
			else if (clientTimestamp > now)
			{
                log.InfoFormat("Decreasing networkHelper.TimeOffset from {0} to {1}",
						       timeOffset, newOffset);
				timeOffset = newOffset;
				clientTimestamp = now;
			}

            return clientTimestamp;
		}

		/// <summary>
		///   Convert the timestamp in client time to a timestamp in server time.
		/// </summary>
		/// <param name="clientTimestamp">timestamp in client time</param>
		/// <returns>timestamp in the server time</returns>
		public long GetServerTimestamp(long clientTimestamp) {
			return (long)(clientTimestamp - timeOffset);
		}

		private TcpClient Connect(string hostname, int port) {
			// This constructor arbitrarily assigns the local port number.
            TcpClient tcpClient = new TcpClient();
            try {
                IPAddress addr = NetworkHelper.GetIPv4Address(hostname);
                if (addr == null) {
                    log.WarnFormat("No valid IPv4 address for {0}", hostname);
                    return null;
                }
                log_status.InfoFormat("Connecting to login server at {0}:{1}", addr, port);
                tcpClient.Connect(addr, port);
                try {
                    // Wait up to ten seconds for a read failure.
                    tcpClient.ReceiveTimeout = 10000;
                } catch (InvalidOperationException) {
                    // I guess this doesn't support the receive timeout
                    log.Warn("Tcp connection does not support receive timeout");
                }
                loginPluginHost = hostname;
                return tcpClient;
            } catch (Exception e) {
                LogUtil.ExceptionLog.WarnFormat("Failed to connect to login server at {0}:{1} - {2}", hostname, port, e);
                tcpClient.Close();
                return null;
            }
        }

        public WorldServerEntry GetWorldEntry(string worldId) {
            Monitor.Enter(worldServerMap);
            try {
                return worldServerMap[worldId];
            } finally {
                Monitor.Exit(worldServerMap);
            }
        }

        public void SetWorldEntry(string worldId, WorldServerEntry entry) {
            Monitor.Enter(worldServerMap);
            try {
                worldServerMap[worldId] = entry;
            } finally {
                Monitor.Exit(worldServerMap);
            }
        }


        public bool HasWorldEntry(string worldId) {
            Monitor.Enter(worldServerMap);
            try {
                return worldServerMap.ContainsKey(worldId);
            } finally {
                Monitor.Exit(worldServerMap);
            }
        }
        #region login messages
		/// <summary>
		///   Login to the Master server using tcp
		/// </summary>
		/// <param name="loginSettings"></param>
		/// <returns></returns>
        public NetworkHelperStatus LoginMaster(LoginSettings loginSettings) {
            log_status.Info("Connecting to master tcp server");
			return TcpMasterConnect(loginSettings);
        }


        /// <summary>
        ///   Connect to the Master server using rdp, and resolve the world id
        /// </summary>
        /// <param name="loginSettings"></param>
        /// <returns></returns>
        public NetworkHelperStatus ResolveWorld(LoginSettings loginSettings) {
            NetworkHelperStatus status = NetworkHelperStatus.WorldResolveFailure;
            log_status.Info("Connecting to master rdp server");
            for (int attempts = 0; attempts < 2; ++attempts) {
				status = RdpMasterConnect(loginSettings.rdpServer, loginSettings.rdpPort);
				if (status == NetworkHelperStatus.Success)
					break;
			}
			if (status != NetworkHelperStatus.Success)
				return status;
			return ResolveWorld(loginSettings.worldId);
		}

		protected NetworkHelperStatus ResolveWorld(string worldId) {
			NetworkHelperStatus status;
			// Close any existing world connections
			RdpWorldDisconnect();

            log_status.InfoFormat("Sending world resolve message");
            status = RdpResolveWorld(worldId);
            log_status.InfoFormat("World resolve message status: {0}", status);
            if (status != NetworkHelperStatus.Success)
                return status;
            WorldServerEntry entry = GetWorldEntry(worldId);
            log_status.InfoFormat("WorldServerEntry: {0}", entry);
            return NetworkHelperStatus.WorldResolveSuccess;
        }

        /// <summary>
        ///   Connect to the login server, so we can do character 
        ///   selection there.
        /// </summary>
        /// <param name="worldId"></param>
        /// <param name="charEntries"></param>
        /// <returns></returns>
        public NetworkHelperStatus ConnectToLogin(string worldId, string clientVersion) {
            WorldServerEntry entry = GetWorldEntry(worldId);
            if (entry.Standalone)
                return NetworkHelperStatus.Standalone;
            
            // Connect to the world using tcp, and get the list of characters
            log_status.InfoFormat("Connecting to world login manager at {0}:{1}", entry.Hostname, entry.Port);
            NetworkHelperStatus status;
            try {
                status = TcpWorldConnect(entry.Hostname, entry.Port, clientVersion);
            }
            catch (EndOfStreamException) {
                status = TcpWorldConnectV15(entry.Hostname, entry.Port, clientVersion);
            }
            if (status == NetworkHelperStatus.Success) {
                connectedWorldId = worldId;
            }
            return status;
        }

        public NetworkHelperStatus ConnectToWorld(long characterId, string hostname, int port, string clientVersion) {
            NetworkHelperStatus status = NetworkHelperStatus.Unknown;
            
            // close my connection to the tcp world server
            TcpWorldDisconnect();

			for (int attempts = 0; attempts < 2; ++attempts) {
                log_status.InfoFormat("Connecting to world proxy at {0}:{1}", hostname, port);
				status = RdpWorldConnect(hostname, port);
				if (status == NetworkHelperStatus.Success)
					break;
			}
			if (status != NetworkHelperStatus.Success)
				return status;

            log_status.InfoFormat("Logging into world server as {0}", characterId);
			status = RdpLogin(characterId, clientVersion);

			return status;
		}

        public void DisconnectFromLogin() {
            TcpWorldDisconnect();
        }

        private void TcpWorldDisconnect() {
            // Close the connection to the tcp world server
            if (tcpWorldConnection != null) {
                log_status.InfoFormat("Disconnecting from tcp world server");
                tcpWorldConnection.Close();
                tcpWorldConnection = null;
            }
        }

        public void Disconnect() {
            RdpWorldDisconnect();
            TcpWorldDisconnect();
            RdpMasterDisconnect();
        }

        /// <summary>
        ///   Connect to the master server via tcp, send our username and 
        ///   password, and get an authentication token.  Finally, disconnect.
        /// </summary>
        /// <param name="loginSettings"></param>
        /// <param name="idToken"></param>
        /// <returns></returns>
		private NetworkHelperStatus TcpMasterConnect(LoginSettings loginSettings) {
			// clear my auth token
            masterToken = null;
			while (true) {
                TcpClient tcpClient = Connect(loginSettings.tcpServer, loginSettings.tcpPort);
                if (tcpClient == null)
                    return NetworkHelperStatus.MasterTcpConnectFailure;
                MasterTcpLoginRequestMessage loginMessage = new MasterTcpLoginRequestMessage();
                loginMessage.Username = loginSettings.username;

                OutgoingMessage outMessage = loginMessage.CreateMessage();
                outMessage.Send(tcpClient);

                IncomingMessage inMessage = new IncomingMessage(tcpClient.GetStream());
                MasterTcpLoginChallengeMessage challengeMessage = new MasterTcpLoginChallengeMessage();
                challengeMessage.ParseMasterTcpMessage(inMessage);
                System.Security.Cryptography.HMACSHA1 hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(loginSettings.password));
                OutgoingMessage dataMarshaller = new OutgoingMessage();
                dataMarshaller.Write(loginSettings.username);
                dataMarshaller.Write(1);
                dataMarshaller.Write(challengeMessage.Challenge);
                byte[] data = dataMarshaller.GetBytes();
                byte[] authenticator = hmac.ComputeHash(data);

                MasterTcpLoginAuthenticateMessage authMessage = new MasterTcpLoginAuthenticateMessage();
                authMessage.Authenticator = authenticator;
                outMessage = authMessage.CreateMessage();
                outMessage.Send(tcpClient);

                MasterTcpLoginResponseMessage responseMessage = new MasterTcpLoginResponseMessage();
                responseMessage.ParseMasterTcpMessage(inMessage);
                LoginStatus result = responseMessage.LoginStatus;
                masterToken = responseMessage.MasterToken;
                oldToken = responseMessage.OldToken;

				tcpClient.Close();
                log_status.InfoFormat("Login Result: {0}, {1}, {2}", masterToken, oldToken, result);
                if (result == LoginStatus.Success) {
					return NetworkHelperStatus.Success;
				} else if (result == LoginStatus.AlreadyLoggedIn) {
                    Logout(loginSettings);
                    continue;
                } else if (result == LoginStatus.ServerError) {
					return NetworkHelperStatus.MasterTcpConnectFailure;
				} else if (result == LoginStatus.InvalidPassword) {
                    return NetworkHelperStatus.LoginFailure;
                } else {
                    log.WarnFormat("Invalid status: {0}", result);
                    return NetworkHelperStatus.MasterTcpConnectFailure;
                }
            }
        }

		/// <summary>
		///   Connect to the world server and get a list of available characters.
        ///   This leaves the connection open, in case they want to create
        ///   new characters or get more information about existing characters.
		/// </summary>
		/// <param name="hostname"></param>
		/// <param name="port"></param>
		/// <param name="idToken"></param>
		/// <returns></returns>
		private NetworkHelperStatus TcpWorldConnect(string hostname, int port, string clientVersion) {
            Debug.Assert(tcpWorldConnection == null);
            log_status.InfoFormat("Connecting to tcp world server at {0}:{1}", hostname, port);
            tcpWorldConnection = Connect(hostname, port);
            if (tcpWorldConnection == null) {
                log.Warn("Unable to connect to tcp world server");
                return NetworkHelperStatus.WorldTcpConnectFailure;
            }
            return GetCharacterEntries(clientVersion);
        }

        /// <summary>
        ///   Helper method to deal with the fact that some servers were not 
        ///   forward compatible.
        /// </summary>
        /// <param name="clientVersion"></param>
        /// <returns></returns>
        private NetworkHelperStatus GetCharacterEntries(string clientVersion) {
            WorldCharacterRequestMessage characterRequest = new WorldCharacterRequestMessage();
            characterRequest.Version = clientVersion;
            characterRequest.AuthToken = masterToken;
            OutgoingMessage outMessage = characterRequest.CreateMessage();
            outMessage.Send(tcpWorldConnection);
            log.InfoFormat("Wrote message to tcp world server");

            // Read a message off of our tcp stream
            IncomingMessage inMessage = new IncomingMessage(tcpWorldConnection.GetStream());
            BaseWorldTcpMessage response = WorldTcpMessageFactory.ReadMessage(inMessage);
            WorldCharacterResponseMessage characterResponse = response as WorldCharacterResponseMessage;
            if (characterResponse == null)
                throw new Exception("Invalid response to world login");

            log.InfoFormat("Read message from tcp world server");

            // TODO: We need to be using world token (or auth token) when we 
            // talk to the world proxy server, since these are our only 
            // mechanisms for authentication.
            worldToken = characterResponse.WorldToken;
           
            // Store the list of character entries, so it can be accessed later
            characterEntries = new List<CharacterEntry>(characterResponse.CharacterEntries);

            NetworkHelperStatus rv;
            // TODO: This is ugly.  I want to be able to grab character info 
            if (characterResponse.Error == string.Empty)
            {
                rv = NetworkHelperStatus.Success;
            }
            else if (characterResponse.Error == "Unsupported client version")
            {
                // custom logic to handle this case
                rv = NetworkHelperStatus.UnsupportedClientVersion;
            }
            else
            {
                // An unknown error
                log.Warn("Bad login status response: " + characterResponse.Error);
                throw new Exception(characterResponse.Error);
            }
            return rv;
		}

        /// <summary>
        ///   Connect to the world server and get a list of available characters.
        ///   This leaves the connection open, in case they want to create
        ///   new characters or get more information about existing characters.
        ///   This version uses the 1.5 security protocol.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="idToken"></param>
        /// <returns></returns>
        private NetworkHelperStatus TcpWorldConnectV15(string hostname, int port, string clientVersion) {
            TcpWorldDisconnect();
            log_status.InfoFormat("Connecting to tcp world server at {0}:{1}", hostname, port);
            tcpWorldConnection = Connect(hostname, port);
            if (tcpWorldConnection == null) {
                log.Warn("Unable to connect to tcp world server");
                return NetworkHelperStatus.WorldTcpConnectFailure;
            }
            return GetCharacterEntriesV15(clientVersion);
        }

        /// <summary>
        ///   Helper method to deal with the fact that some servers were not 
        ///   forward compatible. This version uses the 1.5 security protocol.
        /// </summary>
        /// <param name="clientVersion"></param>
        /// <returns></returns>
        private NetworkHelperStatus GetCharacterEntriesV15(string clientVersion) {
            WorldCharacterRequestMessage characterRequest = new WorldCharacterRequestMessage();
            characterRequest.Version = clientVersion;
            characterRequest.AuthToken = oldToken;
            characterRequest.MessageType = WorldTcpMessageType.V15_CharacterRequest;
            OutgoingMessage outMessage = characterRequest.CreateMessage();
            outMessage.Send(tcpWorldConnection);
            log.InfoFormat("Wrote message to tcp world server");

            // Read a message off of our tcp stream
            IncomingMessage inMessage = new IncomingMessage(tcpWorldConnection.GetStream());
            BaseWorldTcpMessage response = WorldTcpMessageFactory.ReadMessage(inMessage);
            WorldCharacterResponseMessage characterResponse = response as WorldCharacterResponseMessage;
            if (characterResponse == null)
                throw new Exception("Invalid response to world login");

            log.InfoFormat("Read message from tcp world server");

            // TODO: We need to be using world token (or auth token) when we 
            // talk to the world proxy server, since these are our only 
            // mechanisms for authentication.
            worldToken = characterResponse.WorldToken;

            // Store the list of character entries, so it can be accessed later
            characterEntries = new List<CharacterEntry>(characterResponse.CharacterEntries);

            NetworkHelperStatus rv;
            // TODO: This is ugly.  I want to be able to grab character info 
            if (characterResponse.Error == string.Empty) {
                rv = NetworkHelperStatus.Success;
            }
            else if (characterResponse.Error == "Unsupported client version") {
                // custom logic to handle this case
                rv = NetworkHelperStatus.UnsupportedClientVersion;
            }
            else {
                // An unknown error
                log.Warn("Bad login status response: " + characterResponse.Error);
                throw new Exception(characterResponse.Error);
            }
            return rv;
        }

        /// <summary>
        ///   Helper method to deal with the fact that some servers were not 
        ///   forward compatible.
        /// </summary>
        /// <param name="clientVersion"></param>
        /// <returns></returns>
        public NetworkHelperStatus GetProxyToken(long characterId) {
            WorldCharacterSelectRequestMessage selectRequest = new WorldCharacterSelectRequestMessage();
            selectRequest.Properties.Add("characterId", characterId);
            OutgoingMessage outMessage = selectRequest.CreateMessage();
            outMessage.Send(tcpWorldConnection);
            log.InfoFormat("Wrote message to tcp world server");

            // Read a message off of our tcp stream
            IncomingMessage inMessage = new IncomingMessage(tcpWorldConnection.GetStream());
            BaseWorldTcpMessage response = WorldTcpMessageFactory.ReadMessage(inMessage);

            WorldCharacterSelectResponseMessage selectResponse = response as WorldCharacterSelectResponseMessage;
            if (selectResponse == null)
                throw new Exception("Invalid response to world login");
            log.InfoFormat("Read select response message from tcp world server");
            
            worldToken = (byte[])selectResponse.Properties["token"];
            proxyPluginHost = (string)selectResponse.Properties["proxyHostname"];
            proxyPluginPort = (int)selectResponse.Properties["proxyPort"];

            NetworkHelperStatus rv;
            // TODO: This is ugly.
            if (!selectResponse.Properties.ContainsKey("errorMsg") ||
                (string)selectResponse.Properties["errorMsg"] == string.Empty) {
                rv = NetworkHelperStatus.Success;
            }
            else if ((string)selectResponse.Properties["errorMsg"] == "Unsupported client version") {
                // custom logic to handle this case
                rv = NetworkHelperStatus.UnsupportedClientVersion;
            }
            else {
                // An unknown error
                log.Warn("Bad character select response response: " + selectResponse.Properties["errorMsg"]);
                throw new Exception((string)selectResponse.Properties["errorMsg"]);
            }
            return rv;
        }

        private NetworkHelperStatus RdpResolveWorld(string worldName) {
			ResolveMessage resolve = new ResolveMessage();
			resolve.WorldName = worldName;
			rdpMaster.SendMessage(resolve.CreateMessage());
			int sleepCount = 0;
			while (sleepCount < 10) {
				Monitor.Enter(worldServerMap);
				try {
					if (worldServerMap.ContainsKey(worldName)) {
                        log_status.InfoFormat("Found entry for world '{0}'", worldName);
						return NetworkHelperStatus.Success;
					}
				} finally {
					Monitor.Exit(worldServerMap);
				}
				Thread.Sleep(100);
				sleepCount++;
			}
			log.ErrorFormat("Unable to resolve world: {0}", worldName);
			return NetworkHelperStatus.WorldResolveFailure;
		}

		/// <summary>
		///   This is basically a noop now - In the future, I should
		///   send a message on the RDP channel to the master server.
		/// </summary>
		/// <param name="loginSettings"></param>
		/// <returns></returns>
		private NetworkHelperStatus Logout(LoginSettings loginSettings) {
			return NetworkHelperStatus.MasterTcpConnectFailure;
		}

        /// <summary>
        ///   Look up the hostname in DNS and return the first IPv4 address
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        private static IPAddress GetIPv4Address(string hostname) {
            IPAddress addr = null;
            if (IPAddress.TryParse(hostname, out addr)) {
                log.InfoFormat("IP Address for {0}: '{1}'", hostname, addr);
                return addr;
            }

            IPHostEntry IPHost = Dns.GetHostEntry(hostname);
            IPAddress[] addrs = IPHost.AddressList;
            foreach (IPAddress entry in addrs) {
                if (entry.AddressFamily == AddressFamily.InterNetwork) {
                    addr = entry;
                    break;
                }
            }
            log.InfoFormat("IP Address for {0}: '{1}'", hostname, addr);
            return addr;
        }

		private NetworkHelperStatus RdpMasterConnect(string hostname, int port) {
			try {
				log_status.InfoFormat("Connecting to rdp world server at {0}:{1}", hostname, port);
                IPAddress addr = NetworkHelper.GetIPv4Address(hostname);
                if (addr == null) {
                    log.WarnFormat("No valid IPv4 address for {0}", hostname);
                    return NetworkHelperStatus.MasterConnectFailure;
                }
				IPEndPoint endpoint = new IPEndPoint(addr, port);
				rdpMaster = new RdpMasterMessageHandler(endpoint, worldServerMap);
				rdpMaster.BeginListen();
			} catch (Exception e) {
                LogUtil.ExceptionLog.WarnFormat("Exception connecting to rdp master server: {0}", e);
                RdpMasterDisconnect();
                log.ErrorFormat("Failed to connect to rdp master server at {0}:{1}", hostname, port);
				return NetworkHelperStatus.MasterConnectFailure;
			}
			return NetworkHelperStatus.Success;
		}

        private void RdpMasterDisconnect() {
            if (rdpMaster != null) {
                log_status.Info("Disconnecting from rdp master server");
                rdpMaster.Dispose();
                rdpMaster = null;
            }
        }

		private void RdpWorldDisconnect() {
			if (messageHandlerWorld != null) {
                log_status.Info("Disconnecting from rdp world server");
				messageHandlerWorld.Dispose();
				messageHandlerWorld = null;
			}
		}

		private NetworkHelperStatus RdpWorldConnect(string hostname, int port) {
			try {
                log_status.InfoFormat("Connecting to rdp world server at {0}:{1}", hostname, port);
                IPAddress addr = NetworkHelper.GetIPv4Address(hostname);
                if (addr == null) {
                    log.WarnFormat("No valid IPv4 address for {0}", hostname);
                    return NetworkHelperStatus.WorldConnectFailure;
                }
				IPEndPoint endpoint = new IPEndPoint(addr, port);
                MessageDispatcher dispatcher = new DefragmentingMessageDispatcher();
				if (this.UseTCP)
                    messageHandlerWorld = new TcpWorldMessageHandler(endpoint, dispatcher);
                else
                    messageHandlerWorld = new RdpWorldMessageHandler(endpoint, dispatcher);
				messageHandlerWorld.BeginListen();
			} catch (Exception e) {
                LogUtil.ExceptionLog.WarnFormat("Exception connecting to rdp world server {0}:{1} : {2}", hostname, port, e);
                RdpWorldDisconnect();
                log.ErrorFormat("Failed to connect to rdp world server at {0}:{1}", hostname, port);
                return NetworkHelperStatus.WorldConnectFailure;
			}
			return NetworkHelperStatus.Success;
		}

		private NetworkHelperStatus RdpLogin(long characterId, string clientVersion) {
			AuthorizedLoginMessage message = new AuthorizedLoginMessage();
			message.CharacterId = characterId;
            message.ClientVersion = clientVersion;
            message.WorldToken = worldToken;

			SendMessage(message);
			return NetworkHelperStatus.Success;
		}

        #endregion

        public NetworkHelper(IWorldManager worldManager) {
            if (instance != null)
                log.ErrorFormat("NetworkHelper constructor: NetworkHelper is a singleton, but the instance already exists");
            else
                instance = this;
			//this.client = client;
			this.worldManager = worldManager;
			worldManager.NetworkHelper = this;
			worldServerMap = new Dictionary<string, WorldServerEntry>();
            characterEntries = new List<CharacterEntry>();

            // Put in a custom entry for standalone, so we don't need to look it up
            WorldServerEntry entry = new WorldServerEntry("standalone", "localhost", 0, null, null);
            entry.Standalone = true;
            entry.StartupScript = "Standalone.py";
            SetWorldEntry(entry.WorldName, entry);
        }

		public void Init() {
		}

		public void Dispose() {
            Disconnect();
		}

        public static NetworkHelper Instance {
            get {
                return instance;
            }
        }

//        public WorldManager WorldManager {
//            get {
//                return worldManager;
//            }
//            set {
//                worldManager = value;
//            }
//        }

		public long TimeOffset {
			get {
				return timeOffset;
			}
			set {
				timeOffset = value;
			}
		}

		public long PacketsSentCounter {
			get {
				if (messageHandlerWorld == null)
					return 0;
				return messageHandlerWorld.PacketsSentCounter;
			}
		}
		public long PacketsReceivedCounter {
			get {
				if (messageHandlerWorld == null)
					return 0;
				return messageHandlerWorld.PacketsReceivedCounter;
			}
		}
		public long BytesSentCounter {
			get {
				if (messageHandlerWorld == null)
					return 0;
				return messageHandlerWorld.BytesSentCounter;
			}
		}
		public long BytesReceivedCounter {
			get {
				if (messageHandlerWorld == null)
					return 0;
				return messageHandlerWorld.BytesReceivedCounter;
			}
		}
        public byte[] MasterToken {
            get {
                return masterToken;
            }
            set {
                masterToken = value;
            }
        }
        public byte[] OldToken {
            get {
                return oldToken;
            }
            set {
                oldToken = value;
            }
        }
        public List<CharacterEntry> CharacterEntries {
            get {
                return characterEntries;
            }
        }
        public bool UseTCP {
            get {
                return useTCP;
            }
            set {
                useTCP = value;
            }
        }
        public string LoginPluginHost {
            get {
                return loginPluginHost;
            }
        }
        public string ProxyPluginHost {
            get {
                return proxyPluginHost;
            }
        }
        public int ProxyPluginPort {
            get {
                return proxyPluginPort;
            }
        }

        public static bool LogMessageContents {
            get {
                return logMessageContents;
            }
            set {
                logMessageContents = value;
            }
        }
        
	}
}
