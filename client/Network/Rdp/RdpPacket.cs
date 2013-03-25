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

#endregion

namespace Multiverse.Network.Rdp
{

    public class RdpPacket
    {

        public class Offset
        {
            public const int HeaderLength = 1;
            public const int DataLength = 2;
            public const int SeqNumber = 4;
            public const int AckNumber = 8;
            public const int VariableHeaderArea = 12;
            public const int MaxSegments = 12;
            public const int MaxSegmentSize = 14;
            public const int OptionFlags = 16;
        }

//                   0             0 0   1         1
//                   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
//                  +-+-+-+-+-+-+---+---------------+
//                  |S|A|E|R|N| |Ver|    Header     |
//                0 |Y|C|A|S|U|0|No.|    Length     |
//                  |N|K|K|T|L| |   |               |
//                  +-+-+-+-+-+-+---+---------------+
//                1 |          Data  Length         |
//                  +---------------+---------------+
//                2 |                               |
//                  +---    Sequence Number      ---+
//                3 |                               |
//                  +---------------+---------------+
//                4 |                               |
//                  +--- Acknowledgement Number  ---+
//                5 |                               |
//                  +---------------+---------------+
//                6 |     Variable Header Area      |
//                  .                               .
//                  .                               .
//                  |                               |
//                  +---------------+---------------+


        const byte SynMask = 1 << 7;
        const byte AckMask = 1 << 6;
        const byte EakMask = 1 << 5;
        const byte RstMask = 1 << 4;
        const byte NulMask = 1 << 3;
        const byte VerMask = 3;

        const byte SequencedMask = 1 << 7;

		public const int MaxEaks = ((255 * 2) - Offset.VariableHeaderArea) / 4;

        /// <summary>
        ///   The length of the variable headers on an open packet
        /// </summary>
        public const int OpenLength = 6;


        byte[] data;

        static private byte[] ConvertInt(int val) {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val));
        }

        static private int ConvertInt(byte[] val, int offset) {
            return IPAddress.HostToNetworkOrder(BitConverter.ToInt32(val, offset));
        }

        static private byte[] ConvertShort(short val) {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val));
        }

        static private short ConvertShort(byte[] val, int offset) {
            return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(val, offset));
        }

        public RdpPacket(byte[] data) {
            this.data = data;
        }

        public RdpPacket(int dataLength) : this(dataLength, 0) {
        }

        /// <summary>
        ///    Create a packet with optionLength bytes set aside for the
        ///    variable header section
        /// </summary>
        /// <param name="dataLength"></param>
        /// <param name="optionLength"></param>

        public RdpPacket(int dataLength, int optionLength) {
            int hdrLen = optionLength + Offset.VariableHeaderArea;
            data = new byte[dataLength + hdrLen];
            HeaderLength = hdrLen / 2;
            DataLength = (short)dataLength;
        }

		public override string ToString() {
            string rv = string.Format("{0}:{1}:{2}:{3}:{4} - {5}\tdatalen: {6} seq: {7} ack: {8}",
                                      Syn, Ack, Eak, Rst, Nul, HeaderLength,
                                      DataLength, SeqNumber, AckNumber);
            if (Syn)
                rv = rv + string.Format("\t{0}:{1}:{2}", MaxSegments, MaxSegmentSize, Sequenced);
            else if (Eak) {
                int[] eakEntries = EakEntries;
				rv = rv + "\teack:";
				foreach (int i in eakEntries)
                    rv = rv + string.Format(" {0}", i);
            }
            return rv;
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
        public short DataLength {
            get {
                return ConvertShort(data, Offset.DataLength);
            }
            set {
                byte[] val = ConvertShort(value);
                Array.Copy(val, 0, data, Offset.DataLength, 2);
            }
        }
        public int HeaderLength {
            get {
                return (int)data[Offset.HeaderLength];
            }
            set {
                data[Offset.HeaderLength] = (byte)value;
            }
        }

        public int SeqNumber {
            get {
                return ConvertInt(data, Offset.SeqNumber);
            }
            set {
                byte[] val = ConvertInt(value);
                Array.Copy(val, 0, data, Offset.SeqNumber, 4);
            }
        }
        public int AckNumber {
            get {
                return ConvertInt(data, Offset.AckNumber);
            }
            set {
                byte[] val = ConvertInt(value);
                Array.Copy(val, 0, data, Offset.AckNumber, 4);
            }
        }
        public short MaxSegments {
            get {
                return ConvertShort(data, Offset.MaxSegments);
            }
            set {
                byte[] val = ConvertShort(value);
                Array.Copy(val, 0, data, Offset.MaxSegments, 2);
            }
        }
        public short MaxSegmentSize {
            get {
                return ConvertShort(data, Offset.MaxSegmentSize);
            }
            set {
                byte[] val = ConvertShort(value);
                Array.Copy(val, 0, data, Offset.MaxSegmentSize, 2);
            }
        }
        public bool Sequenced {
            get {
                byte[] val = new byte[2];
                Array.Copy(data, Offset.OptionFlags, val, 0, 2);
                return (val[0] & SequencedMask) != 0;
            }
            set {
                byte[] val = new byte[2];
                Array.Copy(data, Offset.OptionFlags, val, 0, 2);
                if (value)
                    val[0] |= SequencedMask;
                else if ((val[0] & SequencedMask) != 0)
                    val[0] -= SequencedMask;
                Array.Copy(val, 0, data, Offset.OptionFlags, 2);
            }
        }
        public int[] EakEntries {
            get {
                if (!Eak)
                    return null;
				int eakLen = HeaderLength * 2 - Offset.VariableHeaderArea;
				int[] rv = new int[eakLen / 4];
				for (int i = 0; i < rv.Length; ++i) {
					int offset = Offset.VariableHeaderArea + i * 4;
					rv[i] = ConvertInt(data, offset);
				}
				return rv;
            }
            set {
				Debug.Assert(4 * value.Length + Offset.VariableHeaderArea == HeaderLength * 2,
							 "Mismatched header length");
				int eakCount = value.Length;
				if (eakCount > MaxEaks)
					eakCount = MaxEaks;
					// this will lead to >500 bytes of eak data, which means 
					// that including the other headers, we have more than
					// 512 bytes of header, but the header length field is
					// limited to 256 shorts or 512 bytes.
				for (int i = 0; i < eakCount; ++i) {
					int offset = Offset.VariableHeaderArea + i * 4;
					byte[] val = ConvertInt(value[i]);
                    Array.Copy(val, 0, data, offset, 4);
                }
            }
        }

        public byte[] PacketData {
            get {
                return data;
            }
        }

        public int DataOffset {
            get {
                return HeaderLength * 2;
            }
        }

        public byte[] Data {
            get {
                byte[] rv = new byte[DataLength];
                Array.Copy(data, DataOffset, rv, 0, DataLength);
                return rv;
            }
        }

		public int PacketLength {
			get {
				return HeaderLength * 2 + DataLength;
			}
		}
        #endregion

    }
}
