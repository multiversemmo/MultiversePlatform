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
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using log4net;

// using SortedDictionary<K, V> = Multiverse.Utility.SortedDictionary<K, V>;

#endregion

namespace Multiverse.Network.Rdp
{
    public class ConnectionCallback {
        public AsyncCallback pfnCallback;
        public object state;
        public RdpPacket packet;
    }

	/// <summary>
	///   Exception that is thrown when we try to send a packet
	///   that is too large for this connection.
	/// </summary>
	public class RdpFragmentationException : Exception {
		int dataLength;
		int dataCapacity;
		public RdpFragmentationException(string msg, int dataLength, int dataCapacity)
			: base(msg) {
			this.dataLength = dataLength;
			this.dataCapacity = dataCapacity;
		}

		public int DataCapacity {
			get {
				return dataCapacity;
			}
		}
		public int DataLength {
			get {
				return dataLength;
			}
		}
	}

    public enum ConnectionState
    {
        /// <summary>
        ///     The CLOSED state exists when no connection exists and  there
        ///     is no connection record allocated.
        /// </summary>
        Closed,
        /// <summary>
        ///     The LISTEN state is entered after a passive Open request  is
        ///     processed.   A  connection record is allocated and RDP waits
        ///     for an active request  to  establish  a  connection  from  a
        ///     remote site.
        /// </summary>
        Listen,
        /// <summary>
        ///     The SYN-SENT state is entered  after  processing  an  active
        ///     Open  request.  A connection record is allocated, an initial
        ///     sequence number is generated, and a SYN segment is  sent  to
        ///     the  remote  site.  RDP then waits in the SYN-SENT state for
        ///     acknowledgement of its Open request.
        /// </summary>
        SynSent,
        /// <summary>
        ///     The SYN-RCVD state may be reached  from  either  the  LISTEN
        ///     state  or from the SYN-SENT state.  SYN-RCVD is reached from
        ///     the LISTEN state when a SYN segment requesting a  connection
        ///     is  received  from  a  remote host.  In reply, the local RDP
        ///     generates an initial sequence number for  its  side  of  the
        ///     connection,  and  then  sends  the  sequence  number  and an
        ///     acknowledgement of the SYN segment to the remote  site.   It
        ///     then waits for an acknowledgement.
        ///   
        ///     The SYN-RCVD state is reached from the SYN-SENT state when a
        ///     SYN  segment  is  received  from  the remote host without an
        ///     accompanying acknowledgement of the SYN segment sent to that
        ///     remote  host  by the local RDP.  This situation is caused by
        ///     simultaneous attempts to open a  connection,  with  the  SYN
        ///     segments  passing  each  other in transit.  The action is to
        ///     repeat the SYN segment with the same  sequence  number,  but
        ///     now  including  an  ACK  of the remote host's SYN segment to
        ///     indicate acceptance of the Open request.
        /// </summary>
        SynRcvd,
        /// <summary>
        ///     The OPEN state exists when a connection has been established
        ///     by  the successful exchange of state information between the
        ///     two sides of the connection.  Each side  has  exchanged  and
        ///     received  such  data  as  initial  sequence  number, maximum
        ///     segment size, and maximum number of unacknowledged  segments
        ///     that may be outstanding.  In the Open state data may be sent
        ///     between the two parties of the connection.
        /// </summary>
        Open,
        /// <summary>
        ///     The CLOSE-WAIT state is entered from either a Close  request
        ///     or  from the receipt of an RST segment from the remote site.
        ///     RDP has sent an RST segment and is waiting  a  delay  period
        ///     for activity on the connection to complete.
        /// </summary>
        CloseWait
    }

    /// <summary>
    ///   This class contains a modified form of RDP.  It is implemented on 
    ///   top of UDP, so it does not itself include port numbers or checksums.
    /// 
    ///   The ack behavior has been modified so that for retransmitted 
    ///   packets, the ack number is updated to the latest (though eacks are 
    ///   not updated).
    /// 
    ///   The behavior in the close_wait state has also been changed to send
    ///   a reset response to any packets received while in close wait (same
    ///   behavior as for the closed state).
    /// </summary>
    public class RdpConnection
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RdpConnection));

        ConnectionState state = ConnectionState.Closed;
        object stateLock = new object();

        const double CloseWaitTimeout = 30 * 1000; // 30 seconds
        const double RetransmissionTimeout = 1 * 1000; // 1 second

        #region Rdp Fields

        /// <summary>
        ///     The sequence number of the next segment that is to be sent.
        /// </summary>
        int sndNxt;
        /// <summary>
        ///     The sequence number of the oldest unacknowledged segment.
        /// </summary>
        int sndUna;
        /// <summary>
        ///     The maximum number of outstanding (unacknowledged)  segments
        ///     that can be sent.  The sender should not send more than this
        ///     number of segments without getting an acknowledgement.
        /// </summary>
        int sndMax;
        /// <summary>
        ///     The initial send sequence  number.   This  is  the  sequence
        ///     number that was sent in the SYN segment.
        /// </summary>
        int sndIss;

        /// <summary>
        ///     The sequence number of the last segment  received  correctly
        ///     and in sequence.
        /// </summary>
        int rcvCur;
        /// <summary>
        ///     The maximum number of segments that can be buffered for this
        ///     connection.
        /// </summary>
        int rcvMax;
        /// <summary>
        ///     The initial receive sequence number.  This is  the  sequence
        ///     number of the SYN segment that established this connection.
        /// </summary>
        int rcvIrs;

        /// <summary>
        ///     The array of sequence numbers of  segments  that  have  been
        ///     received and acknowledged out of sequence.
        /// </summary>
        List<int> rcvdSeqNos = new List<int>();

        /// <summary>
        ///     The largest possible segment (in octets) that can legally be
        ///     sent.  This variable is specified by the foreign host in the
        ///     SYN segment during connection establishment.
        /// </summary>
        int sbufMax;
        /// <summary>
        ///     The  largest  possible  segment  (in  octets)  that  can  be
        ///     received.   This  variable is specified by the user when the
        ///     connection is opened.  The variable is sent to  the  foreign
        ///     host in the SYN segment.
        /// </summary>
        int rbufMax;

        /// <summary>
        ///     The  sequence  number  of  the   segment   currently   being
        ///     processed.
        /// </summary>
        /// segSeq => inPacket.SeqNumber
        /// <summary>
        ///     The acknowledgement sequence number in the segment currently
        ///     being processed.
        /// </summary>
        /// segAck => inPacket.AckNumber
        /// <summary>
        ///     The maximum number of outstanding segments the  receiver  is
        ///     willing  to  hold,  as  specified  in  the  SYN segment that
        ///     established the connection.
        /// </summary>
        /// segMax => inPacket.MaxSegments
        /// <summary>
        ///     The maximum segment size (in octets) accepted by the foreign
        ///     host  on  a connection, as specified in the SYN segment that
        ///     established the connection.
        /// </summary>
        /// segBmax => inPacket.MaxSegmentSize

        #endregion

        // Udp Fields
        UdpClient udpConn;
        IPEndPoint remoteEP;

        // Time when we should leave close wait, and be released
        DateTime closeWaitTime;
        RdpConnectionManager connManager;

        bool valid; // flag set after we have fallen into the closed state
        bool passiveOpen;
        bool outOfOrderAllowed;

		private int packetsSentCount = 0;
		private int packetsReceivedCount = 0;
		private int bytesSentCount = 0;
		private int bytesReceivedCount = 0;
		static long totalBytesSentCount = 0;
        static long totalBytesReceivedCount = 0;
        private int startTick = Environment.TickCount;

        Dictionary<int, DateTime> retransmissionTimer =
            new Dictionary<int, DateTime>();
        Dictionary<int, RdpPacket> unacknowledgedPackets = 
            new Dictionary<int, RdpPacket>();
		SortedList<int, RdpPacket> outOfOrderPackets =
			new SortedList<int, RdpPacket>();
        List<RdpPacket> availablePackets = new List<RdpPacket>();
		SortedList<int, RdpPacket> availableOutOfOrderPackets =
			new SortedList<int, RdpPacket>();


        /// <summary>
        ///   Constructor - should only be called by the connection manager object
        /// </summary>
        public RdpConnection(RdpConnectionManager connManager, 
                             bool passive, UdpClient udpConn, IPEndPoint remoteEP,
                             int rcvMax, int rbufMax, bool sequenced)
        {
            this.connManager = connManager;
            this.udpConn = udpConn;
            this.remoteEP = remoteEP;
            this.rcvMax = rcvMax;
            this.rbufMax = rbufMax;
            outOfOrderAllowed = !sequenced;
            valid = true;
            sndIss = 0; // TODO: replace with a random number?
            sndNxt = sndIss + 1;
            sndUna = sndIss;

            InternalOpen(passive);
        }

        private int[] EakArray {
            get {
                try {
                    Monitor.Enter(this);
					int eakLen = outOfOrderPackets.Count;
                    int[] eakArray = new int[eakLen];
                    outOfOrderPackets.Keys.CopyTo(eakArray, 0);
                    return eakArray;
                } finally {
                    Monitor.Exit(this);
                }
            }
        }

		/// <summary>
		///   This is the version of EakArray that should actually be used, 
		///   since it limits the number of eaks to the number that will fit
		///   in the rdp packet.
		/// </summary>
	    private int[] AbridgedEakArray {
			get {
				int[] eakArray = EakArray;
				if (eakArray.Length > RdpPacket.MaxEaks) {
					int[] abridgedArray = new int[RdpPacket.MaxEaks];
					Array.Copy(eakArray, 0, abridgedArray, 0, abridgedArray.Length);
					eakArray = abridgedArray;
				}
				return eakArray;
			}
		}

        /// <summary>
        ///   Cleanup our retransmissionTimer and unacknowledgedPackets
        /// </summary>
        private void ClearQueues() {
            retransmissionTimer.Clear();
            unacknowledgedPackets.Clear();
        }

        /// <summary>
        ///   Process the packet, adding it to either the out of order list 
        ///   or the available list as appropriate.  We already have the lock.
        ///   This will also update rcvCur to be appropriate.
        /// </summary>
        /// <param name="packet"></param>
        private void HandleDataPacket(RdpPacket packet) {
            outOfOrderPackets[packet.SeqNumber] = packet;
            if (outOfOrderAllowed)
                availableOutOfOrderPackets[packet.SeqNumber] = packet;
            int[] sortedSequence = new int[outOfOrderPackets.Keys.Count];
            outOfOrderPackets.Keys.CopyTo(sortedSequence, 0);
			foreach (int segSeq in sortedSequence) {
				if (segSeq == rcvCur + 1) {
					RdpPacket currentPacket = outOfOrderPackets[segSeq];
					log.DebugFormat("Queued packet {0} : {1}", currentPacket.SeqNumber, segSeq);
					availablePackets.Add(currentPacket);
					if (outOfOrderAllowed)
						availableOutOfOrderPackets.Remove(segSeq);
					rcvCur = segSeq;
					outOfOrderPackets.Remove(segSeq);
				}
			}
			Monitor.PulseAll(this);
        }

        /// <summary>
        ///   Version of Open that bypasses the lock (used internally)
        ///   Generally with these methods, we already hold the lock, but
        ///   in this case, we don't need to hold the lock, since we are
        ///   still in the constructor.
        /// </summary>
        /// <param name="passiveOpen"></param>
        /// <param name="rcvMax"></param>
        /// <param name="rmaxBuf"></param>
        /// <param name="isSequenced"></param>
        private void InternalOpen(bool passiveOpen) {
            if (state != ConnectionState.Closed)
                throw new Exception("Error - connection already open");

            this.passiveOpen = passiveOpen;

            State = ConnectionState.Listen;

            // Create a connection record
            if (!passiveOpen) {
                /// Send <SEQ=SND.ISS><MAX=SND.MAX><MAXBUF=RMAX.BUF><SYN>
                RdpPacket packet = new RdpPacket(0, RdpPacket.OpenLength);
                packet.SeqNumber = sndIss;
                packet.Syn = true;
                packet.MaxSegments = (short)rcvMax;
                packet.MaxSegmentSize = (short)rbufMax;
                packet.Sequenced = !outOfOrderAllowed;
                SendPacket(packet);
                State = ConnectionState.SynSent;
            }
        }

        /// <summary>
        ///   Method to send the packet.  If this is a client, we will pass in 
        ///   null as the remoteEP, since the UdpClient object will have
        ///   called connect.  We should already hold the lock on the connection.
        /// </summary>
        /// <param name="packet"></param>
        private void SendPacket(RdpPacket packet) {
            // count the bytes of outgoing packet
            bytesSentCount += packet.PacketLength;
			totalBytesSentCount += packet.PacketLength;
            packetsSentCount++;

            if (packet.HasData) {
				const int IpHeaderLength = 20;
				const int UdpHeaderLength = 8;
                // If the packet is too large, throw
				if (packet.PacketLength + IpHeaderLength + UdpHeaderLength > sbufMax)
					throw new RdpFragmentationException("packet size {0} is too large for connection with max of {1}", packet.DataLength, sbufMax);
                // If we have already sent as many packets as we can, throw
                if (unacknowledgedPackets.Count > rcvMax)
                    throw new Exception("maximum unacknowledged packets exceeds limit");
                DateTime now = DateTime.Now;
                unacknowledgedPackets[packet.SeqNumber] = packet;
                retransmissionTimer[packet.SeqNumber] = now.AddMilliseconds(RetransmissionTimeout);
                log.DebugFormat("SendPacket: {0} - packet {1}", now, packet);
            }
			if (packet.Eak)
				log.DebugFormat("sending packet with eak set: {0}", packet);
            udpConn.Send(packet.PacketData, packet.PacketData.Length, remoteEP);
         }

        public void Send(byte[] data) {
            if (!valid)
                throw new Exception("Called Send on closed connection");
            RdpPacket packet = new RdpPacket(data.Length);
            Array.Copy(data, 0, packet.PacketData, packet.DataOffset, data.Length);
            Send(packet);
        }

        /// <summary>
        ///   Takes a packet with nothing but the data portion filled, 
        ///   and does what is needed to send it out.
        /// </summary>
        /// <param name="packet"></param>
        public void Send(RdpPacket packet) {
            if (!valid)
                throw new Exception("Called Send on closed connection");
            try {
                Monitor.Enter(this);
                InternalSend(packet);
            } finally {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        ///   Internal version of the send method.  For this version, we already hold the lock.
        /// </summary>
        /// <param name="packet"></param>
        private void InternalSend(RdpPacket packet) {
            switch (state) {
                case ConnectionState.Open:
                    if (sndNxt >= sndUna + sndMax)
                        throw new Exception("Error - insufficient resources to send data");
                    /// Send <ACK=RCV.CUR><SEQ=SND.NXT><ACK><Data>;
                    packet.AckNumber = rcvCur;
                    packet.SeqNumber = sndNxt;
                    packet.Ack = true;
                    SendPacket(packet);
                    sndNxt = sndNxt + 1;
                    break;
                case ConnectionState.Listen:
                case ConnectionState.SynRcvd:
                case ConnectionState.SynSent:
                case ConnectionState.Closed:
                case ConnectionState.CloseWait:
                    throw new Exception("Error - connection not open");
            }
        }

        /// <summary>
        ///   Receive a packet (blocks, and throws an exception if the connection is not open)
        /// </summary>
        /// <returns>null if there are no packets ready</returns>
        public byte[] Receive(ref IPEndPoint remoteEP) {
            ConnectionState connState = this.State;
            switch (connState) {
                case ConnectionState.Open:
                    byte[] rv = null;
                    try {
                        Monitor.Enter(this);
                        while (true) {
                            remoteEP = this.RemoteEndPoint;
                            if (availablePackets.Count > 0) {
								log.DebugFormat("Receive called for packet {0}", 
								    		   availablePackets[0].SeqNumber);
                                rv = availablePackets[0].Data;
                                availablePackets.RemoveAt(0);
                                return rv;
                            } else if (outOfOrderAllowed && availableOutOfOrderPackets.Count > 0) {
                                rv = availableOutOfOrderPackets.Values[0].Data;
                                availableOutOfOrderPackets.RemoveAt(0);
                                return rv;
                            }
                            Monitor.Wait(this);
                        }
                    } finally {
                        Monitor.Exit(this);
                    }
                case ConnectionState.Listen:
                case ConnectionState.SynRcvd:
                case ConnectionState.SynSent:
                    return null;
                case ConnectionState.Closed:
                case ConnectionState.CloseWait:
                    throw new Exception("Error - connection not open");
            }
            return null;
        }

        public void Close() {
            try {
                Monitor.Enter(this);
                InternalClose();
            } finally {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        ///   Internal version of close.  For this version, we already hold the lock.
        /// </summary>
        private void InternalClose() {
            int ticks = Environment.TickCount - startTick;
            log.InfoFormat("Sent {0} bytes, Received {1} bytes in {2} seconds", 
                           bytesSentCount, bytesReceivedCount, ticks / 1000);
            switch (state) {
                case ConnectionState.Open: {
                        /// Send <SEQ=SND.NXT><RST>;
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = sndNxt;
                        packet.Rst = true;
                        SendPacket(packet);
                    }
                    State = ConnectionState.CloseWait;
                    // TODO: Start TIMWAIT Timer
                    break;
                case ConnectionState.Listen:
                    State = ConnectionState.Closed;
                    break;
                case ConnectionState.SynRcvd:
                case ConnectionState.SynSent: {
                        /// Send <SEQ=SND.NXT><RST>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = sndNxt;
                        packet.Rst = true;
                        SendPacket(packet);
                    }
                    State = ConnectionState.Closed;
                    break;
                case ConnectionState.CloseWait:
                    throw new Exception("Error - Connection closing");
                case ConnectionState.Closed:
                    throw new Exception("Error - Connection not open");
            }
        }


        /// <summary>
        ///   Method to handle segment arrival
        /// </summary>
        /// <param name="inPacket"></param>
        /// <param name="remote"></param>
        public void OnSegmentArrival(RdpPacket inPacket, IPEndPoint remoteEP) {
            try {
				Monitor.Enter(this);
				InternalOnSegmentArrival(inPacket, remoteEP);
            } finally {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        ///   Internal method to handle segment arrival.  For this version, we already hold the lock.
        /// </summary>
        /// <param name="inPacket"></param>
        /// <param name="remoteEP"></param>
        private void InternalOnSegmentArrival(RdpPacket inPacket, IPEndPoint remoteEP) {
            DateTime now = DateTime.Now;
			log.DebugFormat("OnSegmentArrival: {0} - packet {1}", now, inPacket);

            // count received bytes
            bytesReceivedCount += inPacket.PacketLength;
            totalBytesReceivedCount += inPacket.PacketLength;
			packetsReceivedCount++;

			switch (state) {
                case ConnectionState.Closed:
				case ConnectionState.CloseWait:
					if (inPacket.Rst)
						return;
                    else if (inPacket.Ack || inPacket.Nul) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = inPacket.AckNumber + 1;
                        packet.Rst = true;
                        SendPacket(packet);
                    } else {
                        /// Send <SEQ=0><RST><ACK=SEG.SEQ><ACK>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = 0;
                        packet.AckNumber = inPacket.SeqNumber;
                        packet.Rst = true;
                        packet.Ack = true;
                        SendPacket(packet);
                    }
                    break;
                case ConnectionState.Listen:
                    if (inPacket.Rst)
                        return;
                    if (inPacket.Ack || inPacket.Nul) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = inPacket.AckNumber + 1;
                        packet.Rst = true;
                        SendPacket(packet);
                        return;
                    }
                    if (inPacket.Syn) {
                        rcvCur = inPacket.SeqNumber;
                        rcvIrs = inPacket.SeqNumber;
                        sndMax = inPacket.MaxSegments;
                        sbufMax = inPacket.MaxSegmentSize;
                        /// Send <SEQ=SND.ISS><ACK=RCV.CUR><MAX=RCV.MAX><BUFMAX=RBUF.MAX>
                        ///      <ACK><SYN>
                        RdpPacket packet = new RdpPacket(0, RdpPacket.OpenLength);
                        packet.SeqNumber = sndIss;
                        packet.AckNumber = rcvCur;
                        packet.Ack = true;
                        packet.Syn = true;
                        packet.MaxSegments = (short)rcvMax;
                        packet.MaxSegmentSize = (short)rbufMax;
                        packet.Sequenced = inPacket.Sequenced;
                        SendPacket(packet);
                        State = ConnectionState.SynRcvd;
                        return;
                    }
                    log.Warn("Shouldn't have gotten here");
                    break;
                case ConnectionState.SynSent:
                    if (inPacket.Rst) {
                        if (inPacket.Ack) {
                            State = ConnectionState.Closed;
                            log.Warn("Connection Refused");
                            // TODO: deallocate connection
                        }
                        return;
                    }
                    if (inPacket.Syn) {
                        rcvCur = inPacket.SeqNumber;
                        rcvIrs = inPacket.SeqNumber;
                        sndMax = inPacket.MaxSegments;
                        sbufMax = inPacket.MaxSegmentSize;
                        if (inPacket.Ack) {
                            sndUna = inPacket.AckNumber + 1; // per rfc 1151
                            State = ConnectionState.Open;
                            /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                            RdpPacket packet = new RdpPacket(0);
                            packet.SeqNumber = sndNxt;
                            packet.AckNumber = rcvCur;
                            packet.Ack = true;
                            SendPacket(packet);
                        } else {
                            State = ConnectionState.SynRcvd;
                            /// Send <SEQ=SND.ISS><ACK=RCV.CUR><MAX=RCV.MAX><BUFMAX=RBUF.MAX>
                            ///      <SYN><ACK>
                            RdpPacket packet = new RdpPacket(0, RdpPacket.OpenLength);
                            packet.SeqNumber = sndIss;
                            packet.AckNumber = rcvCur;
                            packet.Ack = true;
                            packet.Syn = true;
                            packet.MaxSegments = (short)rcvMax;
                            packet.MaxSegmentSize = (short)rbufMax;
                            packet.Sequenced = inPacket.Sequenced;
                            SendPacket(packet);
                        }
                        return;
                    }
                    if (inPacket.Ack) {
                        if (!inPacket.Rst && inPacket.AckNumber != sndIss) {
                            /// Send <SEQ=SEG.ACK + 1><RST>
                            RdpPacket packet = new RdpPacket(0);
                            packet.SeqNumber = inPacket.AckNumber + 1;
                            packet.Rst = true;
                            SendPacket(packet);
                            State = ConnectionState.Closed;
                            log.Warn("Connection Reset (by invalid ACK)");
                            // TODO: deallocate connection
                            return;
                        }
                    }
					if (inPacket.Nul) {
                        log.Warn("Shouldn't have gotten here");
						break;
					}
                    log.Error("Shouldn't have gotten here");
					break;
                case ConnectionState.SynRcvd:
                    if (rcvIrs >= inPacket.SeqNumber || inPacket.SeqNumber > (rcvCur + rcvMax * 2)) {
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = sndNxt;
                        packet.AckNumber = rcvCur;
                        packet.Ack = true;
                        SendPacket(packet);
                        return;
                    }
                    if (inPacket.Rst) {
                        if (passiveOpen)
                            State = ConnectionState.Listen;
                        else {
                            State = ConnectionState.Closed;
                            throw new Exception("Connection Refused");
                        }
                        return;
                    }
                    if (inPacket.Syn) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = inPacket.AckNumber + 1;
                        packet.Rst = true;
                        SendPacket(packet);
                        State = ConnectionState.Closed;
                        log.Warn("Connection Reset (by SYN)");
                        return;
                    }
                    if (inPacket.Eak) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = inPacket.AckNumber + 1;
                        packet.Rst = true;
                        SendPacket(packet);
                        return;
                    }
                    if (inPacket.Ack) {
                        if (inPacket.AckNumber == sndIss)
                            State = ConnectionState.Open;
                        else {
                            /// Send <SEQ=SEG.ACK + 1><RST>
                            RdpPacket packet = new RdpPacket(0);
                            packet.SeqNumber = inPacket.AckNumber + 1;
                            packet.Rst = true;
                            SendPacket(packet);
                            return;
                        }
                    } else 
                        return;
                    if (inPacket.HasData || inPacket.Nul) {
                        HandleDataPacket(inPacket);
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK><EACK><RCVDSEQNO1>
                        ///   ...<RCVDSEQNOn>
                        int[] eakArray = AbridgedEakArray;
                        RdpPacket packet = new RdpPacket(0, eakArray.Length * 4);
                        packet.SeqNumber = sndNxt;
                        packet.AckNumber = rcvCur;
                        packet.Ack = true;
                        if (eakArray.Length > 0) {
                            packet.Eak = true;
                            packet.EakEntries = eakArray;
                        }
                        SendPacket(packet);
                    }
                    break;
                case ConnectionState.Open:
                    if (inPacket.Rst) {
                        State = ConnectionState.CloseWait;
                        log.Warn("Connection Reset");
                        return;
                    }
                    if (rcvCur >= inPacket.SeqNumber || inPacket.SeqNumber > (rcvCur + rcvMax * 2)) {
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = sndNxt;
                        packet.AckNumber = rcvCur;
                        packet.Ack = true;
                        SendPacket(packet);
						log.Debug("Acking packet that was already received");
						return;
                    }
#if STRICT_SPEC
                    if (inPacket.Nul) {
                        rcvCur = inPacket.SeqNumber;
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = sndNxt;
                        packet.AckNumber = rcvCur;
                        packet.Ack = true;
                        SendPacket(packet);
						Logger.Log(1, "Got Nul packet");
						return;
					}
#endif
                    if (inPacket.Syn) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        RdpPacket packet = new RdpPacket(0);
                        packet.SeqNumber = inPacket.AckNumber + 1;
                        packet.Rst = true;
                        SendPacket(packet);
                        State = ConnectionState.Closed;
                        log.Warn("Connection Reset (by SYN)");
                        // TODO: deallocate connection
                        return;
                    }
                    if (inPacket.Ack) {
                        if (sndUna <= inPacket.AckNumber && inPacket.AckNumber < sndNxt) {
                            sndUna = inPacket.AckNumber + 1; // per rfc 1151
                            List<int> removeList = new List<int>();
                            int segAck = inPacket.AckNumber;
                            foreach (int segSeq in unacknowledgedPackets.Keys)
                                if (segSeq <= segAck)
                                    removeList.Add(segSeq);
                            foreach (int segSeq in removeList)
                                unacknowledgedPackets.Remove(segSeq);
                        }
                    }
                    if (inPacket.Eak) {
                        int[] eakEntries = inPacket.EakEntries;
						log.DebugFormat("Received eack packet: {0}", inPacket);
						foreach (int segSeq in eakEntries)
                             unacknowledgedPackets.Remove(segSeq);
                    }
#if STRICT_SPEC
                    if (inPacket.HasData) {
#else
                    if (inPacket.HasData || inPacket.Nul) {
#endif
                        HandleDataPacket(inPacket);
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK><EACK><RCVDSEQNO1>
                        ///   ...<RCVDSEQNOn>
                        int[] eakArray = AbridgedEakArray;
                        RdpPacket packet = new RdpPacket(0, eakArray.Length * 4);
                        packet.SeqNumber = sndNxt;
                        packet.AckNumber = rcvCur;
                        packet.Ack = true;
                        if (eakArray.Length > 0) {
                            packet.Eak = true;
                            packet.EakEntries = eakArray;
                        }
                        SendPacket(packet);
                    }
                    break;
            }
        }

        /// <summary>
        ///   This is called periodically by the connection manager.
        ///   Check the timers of each of the unacknowledged packets to see if they are due for retransmission.  
        /// </summary>
        /// <param name="now"></param>
        public void OnRetransmissionTick(DateTime now) {
            List<int> retransmitList = new List<int>();
            try {
                Monitor.Enter(this);         
                foreach (KeyValuePair<int, DateTime> pair in retransmissionTimer)
                    if (now > pair.Value)
                        retransmitList.Add(pair.Key);
                retransmitList.Sort(); // this should generally perform better
                foreach (int segSeq in retransmitList) {
                    RdpPacket packet = null;
                    if (unacknowledgedPackets.ContainsKey(segSeq))
                        packet = unacknowledgedPackets[segSeq];
                    // Remove these from the unacknowledged packets and retransmit timer,
                    // since we will call SendPacket and handle these again.
                    unacknowledgedPackets.Remove(segSeq);
                    retransmissionTimer.Remove(segSeq);
                    
                    if (packet == null)
                        continue;
					// Update the ack numbers on the packets
					// this is an optimization to prevent unnecessary 
					// retransmits from our peer.
					if (packet.Ack)
						packet.AckNumber = rcvCur;
					SendPacket(packet);
                }
            } finally {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        ///   This is called by the connection manager when the close wait timer has expired.
        /// </summary>
        public void OnCloseWaitTimeout() {
            try {
                Monitor.Enter(this);
                State = ConnectionState.Closed;
            } finally {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        ///   Wait until the connection shifts into the given state
        /// </summary>
        /// <param name="waitForState"></param>
        public bool WaitForState(ConnectionState waitForState) {
            return WaitForState(waitForState, -1);
        }

        /// <summary>
        ///   Wait until the connection shifts into the given state
        /// </summary>
        /// <param name="waitForState">state that we are waiting for</param>
        /// <param name="millisecondsTimeout">number of milliseconds to wait, or -1 to wait indefinitely</param>
        /// <returns>true if we transitioned to the given state, or false if we timed out</returns>
        public bool WaitForState(ConnectionState waitForState, int millisecondsTimeout) {
            try {
                int untilMillis = Environment.TickCount + millisecondsTimeout;
                Monitor.Enter(stateLock);
                while (true) {
                    if (state == waitForState)
                        return true;
                    else if (IsClosed)
                        throw new Exception("Connection closed");
                    if (millisecondsTimeout >= 0) {
                        int timeout = untilMillis - Environment.TickCount;
                        if (timeout <= 0)
                            return false;
                        Monitor.Wait(stateLock, timeout);
                    } else {
                        Monitor.Wait(stateLock);
                    }
                }
            } finally {
                Monitor.Exit(stateLock);
            }
        }

        #region Properties

		public bool IsClosed {
			get {
				return ((state == ConnectionState.Closed) ||
						(state == ConnectionState.CloseWait));
			}
		}

		private ConnectionState State {
            get {
                try {
                    Monitor.Enter(stateLock);
                    return state;
                } finally {
                    Monitor.Exit(stateLock);
                }
            }
            // For all the set methods, we should alread have a lock on the connection.
            set {
                try {
                    Monitor.Enter(stateLock);
                    if ((state != ConnectionState.CloseWait) &&
                        (value == ConnectionState.CloseWait)) {
                        DateTime now = DateTime.Now;
                        closeWaitTime = now.AddMilliseconds(CloseWaitTimeout);
                        connManager.ReleaseConnection(this);
                        ClearQueues();
                    }
                    if ((state != ConnectionState.Closed) &&
                        (value == ConnectionState.Closed)) {
                        connManager.CloseConnection(this);
                        ClearQueues();
                    }
                    state = value;
                    Monitor.PulseAll(stateLock);
                } finally {
                    Monitor.Exit(stateLock);
                }
            }
        }

        public DateTime CloseWaitTime {
            get {
                return closeWaitTime;
            }
        }

        public ConnectionState ConnectionState {
            get {
                return State;
            }
        }

        public int UnackedCount {
            get {
                try {
                    Monitor.Enter(this);
                    return unacknowledgedPackets.Count;
                } finally {
                    Monitor.Exit(this);
                }
            }
        }

        public int AvailableCount {
            get {
                try {
                    Monitor.Enter(this);
                    return availablePackets.Count + availableOutOfOrderPackets.Count;
                } finally {
                    Monitor.Exit(this);
                }
            }
        }

        public int MaxReceiveSegment {
            get {
                return rbufMax;
            }
        }

        public int MaxSendSegment {
            get {
                return sndMax;
            }
        }

        public IPEndPoint RemoteEndPoint {
            get {
                return remoteEP;
            }
        }

        public int MaxSegments {
            get {
                return rcvMax;
            }
        }

		public long PacketsSentCounter {
			get {
				return packetsSentCount;
			}
		}
		public long PacketsReceivedCounter {
			get {
				return packetsReceivedCount;
			}
		}
		public long BytesSentCounter {
			get {
				return bytesSentCount;
			}
		}
		public long BytesReceivedCounter {
			get {
				return bytesReceivedCount;
			}
		}
		public static long TotalBytesSentCounter {
			get {
				return totalBytesSentCount;
			}
		}
		public static long TotalBytesReceivedCounter {
			get {
				return totalBytesReceivedCount;
			}
		}
		#endregion // Properties
    }
}
