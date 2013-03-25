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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

using Multiverse.Config;
using Multiverse.Lib.LogUtil;

using ReadWriteLock = Multiverse.Utility.ReadWriteLock;

#endregion

namespace Multiverse.Network.Rdp
{
    public class RdpConnectionManager : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RdpConnectionManager));
        protected static readonly log4net.ILog log_status = log4net.LogManager.GetLogger("Rdp Connection");

        /// <summary>
        ///  Some defaults
        /// </summary>
        const int DefaultRcvMax = 256;
        const int DefaultRbufMax = 4096;
        const bool DefaultSequenced = true;

		protected Dictionary<IPEndPoint, RdpConnection> connections =
            new Dictionary<IPEndPoint, RdpConnection>();

        protected List<RdpConnection> closeWaitConnections =
            new List<RdpConnection>();
		
		protected System.Timers.Timer networkTimer;

		protected int rcvMax;
        protected int rbufMax;
        protected bool sequenced;
		protected ReadWriteLock connectionsLock = new ReadWriteLock();

		private DateTime lastTick;

        #region Udp Callbacks
        protected UdpClient udpConn;
        protected IPEndPoint remoteEP;

        private AsyncCallback pfnCallBack;
        
        public virtual RdpConnection HandleNewConnection(IPEndPoint remoteEP) {
            return null;
        }

        public void WaitForData() {
            try {
                // now start to listen for any data...
                IAsyncResult asyncResult = udpConn.BeginReceive(pfnCallBack, null);
			} catch (ObjectDisposedException e) {
				LogUtil.ExceptionLog.WarnFormat("Object disposed in call to udpConn.BeginReceive: {0}", e);
				throw e;
			} catch (SocketException e) {
				LogUtil.ExceptionLog.WarnFormat("Socket exception in call to udpConn.BeginReceive: {0}", e);
                log.WarnFormat("socket error code: {0}", e.ErrorCode);
                throw;
            }
        }

		public void Dispose() {
			try {
				if (udpConn != null)
					udpConn.Close();
			} catch (Exception) {
				// Ignore;
			}
		}

		public void OnDataReceived(IAsyncResult asyn) {
			// Logger.Log(0, "Begin OnDataReceived");
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] buf = null;
			try {
				buf = udpConn.EndReceive(asyn, ref remoteIpEndPoint);
			} catch (SocketException e) {
				if (e.SocketErrorCode == SocketError.Interrupted)
					// ignore
					return;
                log_status.WarnFormat("Caught socket exception: {0}", e);
				WaitForData();
				return;
			} catch (ObjectDisposedException) {
				// just ignore this one
				return;
			}
			RdpConnection conn = GetConnection(remoteIpEndPoint);
			if (conn == null) {
				log_status.Info("Handling new connection");
				conn = HandleNewConnection(remoteIpEndPoint);
			}
			if (conn != null) {
				RdpPacket packet = new RdpPacket(buf);
				// Logger.Log(0, "Got data in OnDataReceived");
				conn.OnSegmentArrival(packet, remoteIpEndPoint);
				// Logger.Log(0, "Done with OnDataReceived");
			}
			try {
				WaitForData();
			} catch (SocketException e) {
				if (e.SocketErrorCode != SocketError.Interrupted)
					throw;
			} catch (ObjectDisposedException) {
				// Just ignore this
			}
		}

        #endregion

        public RdpConnectionManager(int localPort) : 
            this(localPort, DefaultRcvMax, DefaultRbufMax, DefaultSequenced) 
        {
        }
        
        public RdpConnectionManager(int localPort, int rcvMax, int rbufMax, bool sequenced) {
            udpConn = new UdpClient(localPort); 
            this.rcvMax = rcvMax;
            this.rbufMax = rbufMax;
            this.sequenced = sequenced;
            networkTimer = new System.Timers.Timer();
            networkTimer.Enabled = true;
            networkTimer.Interval = 1000;
            networkTimer.Elapsed +=
               new System.Timers.ElapsedEventHandler(this.OnTick);
            pfnCallBack = new AsyncCallback(this.OnDataReceived);
            WaitForData();
        }

        /// <summary>
        ///   Create a new RdpConnection
        /// </summary>
        /// <param name="udpConn">the underlying UdpClient object used for sending messages</param>
        /// <param name="remoteEP">remote ip end point for this connection</param>
        /// <param name="passive">true if the connection should just be listening, or false to send a syn</param>
        /// <returns></returns>
        protected RdpConnection GetConnection(UdpClient udpConn, IPEndPoint remoteEP, bool passive) {
            try {
				connectionsLock.BeginWrite();
				RdpConnection conn = new RdpConnection(this, passive, udpConn, remoteEP, rcvMax, rbufMax, sequenced);
                connections[remoteEP] = conn;
                return conn;
            } finally {
				connectionsLock.EndWrite();
			}
        }

        /// <summary>
        ///   Retrieve an existing connection
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <returns></returns>
        protected RdpConnection GetConnection(IPEndPoint remoteEP) {
            try {
				connectionsLock.BeginRead();
				if (!connections.ContainsKey(remoteEP))
                    return null;
                return connections[remoteEP];
            } finally {
				connectionsLock.EndRead();
			}
        }

        public void OnTick(object sender, System.Timers.ElapsedEventArgs e) {
			// This may take more than a second, so disable the timer while we are in
			// here.  This won't guarrantee that there aren't two people in here, but
			// it will make it less likely.
			try {
				Monitor.Enter(this);
				networkTimer.Stop();
				lastTick = DateTime.Now;
				try {
					Monitor.Enter(closeWaitConnections);
					if (closeWaitConnections.Count != 0) {
						foreach (RdpConnection conn in closeWaitConnections)
							if (IsExpired(conn))
								conn.OnCloseWaitTimeout();
						closeWaitConnections.RemoveAll(new Predicate<RdpConnection>(this.IsExpired));
					}
				} finally {
					Monitor.Exit(closeWaitConnections);
				}

				try {
					connectionsLock.BeginRead();
					foreach (RdpConnection conn in connections.Values)
						conn.OnRetransmissionTick(lastTick);
				} finally {
					connectionsLock.EndRead();
				}
			} finally {
				networkTimer.Start();
				Monitor.Exit(this);
			}
		}

        private bool IsExpired(RdpConnection conn) {
            return (conn.CloseWaitTime != null) && (lastTick >= conn.CloseWaitTime);
        }

        /// <summary>
        ///   When a connection goes into the closed state, call this
        /// </summary>
        /// <param name="conn"></param>
		public void CloseConnection(RdpConnection conn) {
			try {
				connectionsLock.BeginWrite();
                log_status.InfoFormat("Closing connection to {0}", conn.RemoteEndPoint);
				connections.Remove(conn.RemoteEndPoint);
			} finally {
				connectionsLock.EndWrite();
			}
		}

        /// <summary>
        ///   When a connection goes into the close wait state, call this
        /// </summary>
        /// <param name="conn"></param>
        public void ReleaseConnection(RdpConnection conn) {
            try {
                log_status.InfoFormat("Released connection: {0}", conn);
				Monitor.Enter(closeWaitConnections);
                closeWaitConnections.Add(conn);
            } finally {
                Monitor.Exit(closeWaitConnections);
            }
        }
    }

    public class RdpClient : RdpConnectionManager
    {
        public RdpClient(int localPort) : 
            base(localPort) 
        {
        }
        public RdpClient(int localPort, int rcvMax, int rbufMax, bool sequenced) : 
            base(localPort, rcvMax, rbufMax, sequenced) 
        {
        }

        public RdpConnection Connect(IPEndPoint remoteEP) {
            return Connect(remoteEP, -1);
        }

        /// <summary>
        ///   Connect to the remote endpoint with the specified timeout on 
        ///   receiving the response that will transition the connection to
        ///   the connected state.
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="millisecondsTimeout">number of milliseconds to wait, or -1 to wait indefinitely</param>
        /// <returns></returns>
        public RdpConnection Connect(IPEndPoint remoteEP, int millisecondsTimeout) {
            RdpConnection conn = GetConnection(udpConn, remoteEP, false);
            bool success = false;
            log_status.InfoFormat("Created connection to: {0}", remoteEP);
            try {
                success = conn.WaitForState(ConnectionState.Open, millisecondsTimeout);
                if (!success)
                    throw new TimeoutException();
            } catch (Exception) {
                ReleaseConnection(conn);
                throw;
            }
            return conn;
        }
    }

    public class RdpServer : RdpConnectionManager 
    {
        List<RdpConnection> newConnections = 
            new List<RdpConnection>();

        #region Udp Callbacks
        public override RdpConnection HandleNewConnection(IPEndPoint remoteEP) {
            // Construct an RdpConnection for this new "connection"
            RdpConnection conn = GetConnection(udpConn, remoteEP, true);
            try {
                Monitor.Enter(newConnections);
                newConnections.Add(conn);
                Monitor.PulseAll(newConnections);
                return conn;
            } finally {
                Monitor.Exit(newConnections);
            }
		}
        #endregion

        public RdpServer(int localPort) : 
            base(localPort) 
        {
        }

        public RdpServer(int localPort, int rcvMax, int rbufMax, bool sequenced) : 
            base(localPort, rcvMax, rbufMax, sequenced) 
        {
        }

        public RdpConnection Accept() {
            try {
                Monitor.Enter(newConnections);
                while (true) {
                    if (newConnections.Count != 0) {
                        RdpConnection conn = newConnections[0];
                        newConnections.RemoveAt(0);
                        return conn;
                    }
                    Monitor.Wait(newConnections);
                }
            } finally {
                Monitor.Exit(newConnections);
            }
        }
    }
}
