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
using System.Diagnostics;
using System.Net;

using Multiverse.Config;

#endregion

namespace Multiverse.Network.Rdp
{
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
 
    public class RdpPacket {

//                      +-+-+-+-+-+-+---+---------------+
//                      |S|A|E|R|N| |Ver|    Header     |
//                    0 |Y|C|A|S|U|0|No.|    Length     |
//                      |N|K|K|T|L| |   |               |
//                      +-+-+-+-+-+-+---+---------------+
//                    1 | Source Port   |   Dest. Port  |
//                      +---------------+---------------+
//                    2 |          Data  Length         |
//                      +---------------+---------------+
//                    3 |                               |
//                      +---    Sequence Number      ---+
//                    4 |                               |
//                      +---------------+---------------+
//                    5 |                               |
//                      +--- Acknowledgement Number  ---+
//                    6 |                               |
//                      +---------------+---------------+
//                    7 |                               |
//                      +---        Checksum         ---+
//                    8 |                               |
//                      +---------------+---------------+
//                    9 |     Variable Header Area      |
//                      .                               .
//                      .                               .
//                      |                               |
//                      |                               |
//                      +---------------+---------------+
        const byte SynMask = 1 << 7;
        const byte AckMask = 1 << 6;
        const byte EakMask = 1 << 5;
        const byte RstMask = 1 << 4;
        const byte NulMask = 1 << 3;
        const byte VerMask = 3;

        byte[] data;

        static private byte[] Convert(int val) {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val));
        }
        static private int Convert(byte[] val, int offset) {
            return IPAddress.HostToNetworkOrder(BitConverter.ToInt32(val, offset));
        }

        public RdpPacket(byte[] data) {
            this.data = data;
        }
        public RdpPacket(int dataLength) : this(dataLength, 0) {
        }
        public RdpPacket(int dataLength, int optionLength) {
            data = new byte[dataLength + optionLength + 18];
            HeaderLength = 18 + optionLength;
            DataLength = dataLength;
        }

        #region Properties

        public bool Syn {
            get {
                return (data[0] & SynMask) != 0;
            }
            set {
                if (value)
                    data[0] |= SynMask;
                else if (Syn)
                    data[0] -= SynMask;
            }
        }
        public bool Ack {
            get {
                return (data[0] & AckMask) != 0;
            }
            set {
                if (value)
                    data[0] |= AckMask;
                else if (Ack)
                    data[0] -= AckMask;
            }
        }
        public bool Eak {
            get {
                return (data[0] & EakMask) != 0;
            }
            set {
                if (value)
                    data[0] |= EakMask;
                else if (Eak)
                    data[0] -= EakMask;
            }
        }
        public bool Rst {
            get {
                return (data[0] & RstMask) != 0;
            }
            set {
                if (value)
                    data[0] |= RstMask;
                else if (Rst)
                    data[0] -= RstMask;
            }
        }
        public bool Nul {
            get {
                return (data[0] & NulMask) != 0;
            }
            set {
                if (value)
                    data[0] |= NulMask;
                else if (Nul)
                    data[0] -= NulMask;
            }
        }
        public bool HasData {
            get {
                return DataLength != 0;
            }
        }
        public int DataLength {
            get {
                return 256 * data[4] + data[5];
            }
            set {
                data[4] = (byte)(value / 256);
                data[5] = (byte)(value % 256);
            }
        }
        public int HeaderLength {
            get {
                return (int)data[1];
            }
            set {
                data[1] = (byte)value;
            }
        }

        public int SourcePort {
            get {
                return (int)data[2];
            }
            set {
                data[2] = (byte)value;
            }
        }

        public int DestPort {
            get {
                return (int)data[3];
            }
            set {
                data[3] = (byte)value;
            }
        }

        public int SeqNumber {
            get {
                return Convert(data, 6);
            }
            set {
                byte[] val = Convert(value);
                Array.Copy(val, 0, data, 6, 4);
            }
        }
        public int AckNumber {
            get {
                return Convert(data, 10);
            }
            set {
                byte[] val = Convert(value);
                Array.Copy(val, 0, data, 10, 4);
            }
        }
        public int Checksum {
            get {
                return Convert(data, 14);
            }
            set {
                byte[] val = Convert(value);
                Array.Copy(val, 0, data, 14, 4);
            }
        }
        #endregion
 
    }

    public class RdpConnection
    {
        ConnectionState state;

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
        ///     A timer used to time out the CLOSE-WAIT state.
        /// </summary>
        int closeWait;
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
        int segSeq;
        /// <summary>
        ///     The acknowledgement sequence number in the segment currently
        ///     being processed.
        /// </summary>
        int segAck;
        /// <summary>
        ///     The maximum number of outstanding segments the  receiver  is
        ///     willing  to  hold,  as  specified  in  the  SYN segment that
        ///     established the connection.
        /// </summary>
        int segMax;
        /// <summary>
        ///     The maximum segment size (in octets) accepted by the foreign
        ///     host  on  a connection, as specified in the SYN segment that
        ///     established the connection.
        /// </summary>
        int segBmax;

        int localPort;
        int remotePort;

        bool passiveOpen;
        bool outOfOrderAllowed;
        List<RdpPacket> unacknowledgedPackets = new List<RdpPacket>();

        public RdpConnection() {
            state = ConnectionState.Closed;
            outOfOrderAllowed = true;
        }

        public void Open(bool passiveOpen, int localPort, int remotePort, 
                         int sndMax, int rmaxBuf) 
        {
            if (state != ConnectionState.Closed)
                throw new Exception("Error - connection already open");

            this.passiveOpen = passiveOpen;

            // Create a connection record
            if (passiveOpen) {
                if (localPort <= 0)
                    throw new Exception("Error - local port not specified"); 
                sndIss = 0;
                sndNxt = sndIss + 1;
                sndUna = sndIss;
            
                this.sndMax = sndMax;
                this.rbufMax = rmaxBuf;

                state = ConnectionState.Listen;
            } else {
                if (remotePort <= 0)
                    throw new Exception("Error - remote port not specified"); 
                sndIss = 0;
                sndNxt = sndIss + 1;
                sndUna = sndIss;

                this.sndMax = sndMax;
                this.rbufMax = rmaxBuf;

                state = ConnectionState.Listen;

                // TODO: Change this so that they can pass in 0 or something,
                //       and automatically pick a local port.
                this.localPort = localPort;

                /// Send <SEQ=SND.ISS><MAX=SND.MAX><MAXBUF=RMAX.BUF><SYN>
                state = ConnectionState.SynSent;
            }
        }

        public void Send(RdpPacket packet) {
            switch (state) {
                case ConnectionState.Open:
                    if (sndNxt >= sndUna + sndMax)
                        throw new Exception("Error - insufficient resources to send data");
                    /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK><Data>
                    sndNxt = sndNxt + 1;
                    break;
                case ConnectionState.Listen:
                case ConnectionState.SynRcvd:
                case ConnectionState.SynSent:
                case ConnectionState.Closed:
                case ConnectionState.CloseWait:
                    throw new Exception("Error - connection not open");
                    break;
            }
        }

        public byte[] Receive() {
            switch (state) {
                case ConnectionState.Open:
                    // TODO: Add code to get data if pending or return no data
                    // if (data pending)
                    //      return data;
                    return null;
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
            switch (state) {
                case ConnectionState.Open:
                    /// Send <SEQ=SND.NXT><RST>;
                    state = ConnectionState.CloseWait;
                    // TODO: Start TIMWAIT Timer
                    break;
                case ConnectionState.Listen:
                    state = ConnectionState.Closed;
                    break;
                case ConnectionState.SynRcvd:
                case ConnectionState.SynSent:
                    /// Send <SEQ=SND.NXT><RST>
                    state = ConnectionState.Closed;
                    break;
                case ConnectionState.CloseWait:
                    throw new Exception("Error - Connection closing");
                    break;
                case ConnectionState.Closed:
                    throw new Exception("Error - Connection not open");
                    break;
            }
        }

        // TODO: Should I pass in remote endpoint?
        public void OnSegmentArrival(RdpPacket packet) {
            switch (state) {
                case ConnectionState.Closed:
                    if (packet.Rst)
                        return;
                    else if (packet.Ack || packet.Nul) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        ;
                    } else {
                        /// Send <SEQ=0><RST><ACK=SEG.SEQ><ACK>
                        ;
                    }
                    break;
                case ConnectionState.CloseWait:
                    break;
                case ConnectionState.Listen:
                    if (packet.Rst)
                        return;
                    if (packet.Ack || packet.Nul) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        return;
                    }
                    if (packet.Syn) {
                        rcvCur = segSeq;
                        rcvIrs = segSeq;
                        sndMax = segMax;
                        sbufMax = segBmax;
                        /// Send <SEQ=SND.ISS><ACK=RCV.CUR><MAX=RCV.MAX><BUFMAX=RBUF.MAX>
                        ///      <ACK><SYN>
                        state = ConnectionState.SynRcvd;
                        return;
                    }
                    Trace.TraceWarning("Shouldn't have gotten here");
                    break;
                case ConnectionState.SynSent:
                    if (packet.Rst) {
                        if (packet.Ack) {
                            state = ConnectionState.Closed;
                            throw new Exception("Connection Refused");
                            // TODO: deallocate connection
                        }
                        return;
                    }
                    if (packet.Syn) {
                        rcvCur = segSeq;
                        rcvIrs = segSeq;
                        sndMax = segMax;
                        rbufMax = segBmax;
                        if (packet.Ack) {
                            sndUna = segAck + 1; // per rfc 1151
                            state = ConnectionState.Open;
                            /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        } else {
                            state = ConnectionState.SynRcvd;
                            /// Send <SEQ=SND.ISS><ACK=RCV.CUR><MAX=RCV.MAX><BUFMAX=RBUF.MAX>
                            //       <SYN><ACK>
                        }
                        return;
                    }
                    if (packet.Ack) {
                        if (!packet.Rst && segAck != sndIss) {
                            /// Send <SEQ=SEG.ACK + 1><RST>
                            state = ConnectionState.Closed;
                            throw new Exception("Connection Reset");
                            // TODO: deallocate connection
                            return;
                        }
                    }

                    Trace.TraceWarning("Shouldn't have gotten here");
                    break;
                case ConnectionState.SynRcvd:
                    if (rcvIrs >= segSeq || segSeq > (rcvCur + rcvMax * 2))
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        return;
                    if (packet.Rst) {
                        if (passiveOpen)
                            state = ConnectionState.Listen;
                        else {
                            state = ConnectionState.Closed;
                            throw new Exception("Connection Refused");
                            // TODO: deallocate connection
                        }
                        return;
                    }
                    if (packet.Syn) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        state = ConnectionState.Closed;
                        throw new Exception("Connection Reset");
                        // TODO: deallocate connection
                        return;
                    }
                    if (packet.Eak) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        return;
                    }
                    if (packet.Ack) {
                        if (segAck == sndIss)
                            state = ConnectionState.Open;
                        else
                            /// Send <SEQ=SEG.ACK + 1><RST>
                            return;
                    } else 
                        return;
                    if (packet.HasData || packet.Nul) {
                        bool inSequence = true;
                        // If the received segment is in sequence
                        if (inSequence) {
                            ///  TODO: Copy the data (if any) to user buffers
                            rcvCur = segSeq;
                            /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        } else {
                            if (outOfOrderAllowed)
                                // TODO: Copy the data (if any) to user buffers
                                ;
                            /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK><EACK><RCVDSEQNO1>
                            ///   ...<RCVDSEQNOn>
                        }
                    }
                    break;
                case ConnectionState.Open:
                    if (rcvCur >= segSeq || segSeq > (rcvCur + rcvMax * 2)) {
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        return;
                    }
                    if (packet.Rst) {
                        state = ConnectionState.CloseWait;
                        throw new Exception("Connection Reset");
                        return;
                    }
                    if (packet.Nul) {
                        rcvCur = segSeq;
                        /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        return;
                    }
                    if (packet.Syn) {
                        /// Send <SEQ=SEG.ACK + 1><RST>
                        state = ConnectionState.Closed;
                        throw new Exception("Connection Reset");
                        // TODO: deallocate connection
                        return;
                    }
                    if (packet.Ack) {
                        if (sndUna <= segAck && segAck < sndNxt) {
                            sndUna = segAck + 1; // per rfc 1151
                            // TODO: Flush acknowledged segments
                        }
                    }
                    if (packet.Eak) {
                        // TODO: Flush acknowledged segments
                    }
                    if (packet.HasData) {
                        bool inSequence = true;
                        // If the received segment is in sequence
                        if (inSequence) {
                            ///  TODO: Copy the data (if any) to user buffers
                            rcvCur = segSeq;
                            // This can have EACKS too, if you want
                            /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK>
                        } else {
                            if (outOfOrderAllowed)
                                // TODO: Copy the data (if any) to user buffers
                                ;
                            /// Send <SEQ=SND.NXT><ACK=RCV.CUR><ACK><EACK><RCVDSEQNO1>
                            ///   ...<RCVDSEQNOn>
                        }
                    }
                    break;
            }
        }

        public void OnRetransmissionTimeout() {
            // Re-send
        }
        public void OnCloseWaitTimeout() {
            state = ConnectionState.Closed;
            // this should be in some higher level guy that pulls me
        }

        public ConnectionState State {
            get {
                return state;
            }
        }

        // TODO: Methods to get:
        //   Number of segments unacknowledged
        //   Number of segments received not given to user

        public int MaxReceiveSegment {
            get {
                return rbufMax;
            }
        }
        public int MaxSendSegment {
            get {
                return segBmax;
            }
        }
    }
}
