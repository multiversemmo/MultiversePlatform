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

using Axiom.MathLib;
using Axiom.Core;

using Multiverse.MathLib;
using Multiverse.Network.Rdp;

#endregion

namespace Multiverse.Network
{
    public class IncomingMessage
    {
        private BinaryReader reader;
        private IPEndPoint remoteIpEndPoint;

        private void Init(byte[] buf, IPEndPoint remoteEP)
        {
            Init(buf, 0, buf.Length, remoteEP);
        }
        private void Init(byte[] buf, int offset, int len, IPEndPoint remoteEP) 
        {
            remoteIpEndPoint = remoteEP;
            MemoryStream memStream = new MemoryStream();
            memStream.Write(buf, offset, len);
            memStream.Flush();
            memStream.Seek(0, System.IO.SeekOrigin.Begin);
            reader = new BinaryReader(memStream);
        }

        public IncomingMessage(byte[] buf, IPEndPoint remoteEP)
        {
            Init(buf, remoteEP);
        }

        public IncomingMessage(byte[] buf, IncomingMessage source) 
        {
            Init(buf, source.remoteIpEndPoint);
        }
        
        public IncomingMessage(byte[] buf, int offset, int len, IPEndPoint remoteEP)
        {
            Init(buf, offset, len, remoteEP);
        }

        public IncomingMessage(UdpClient udpClient) {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buf = udpClient.Receive(ref remoteEP);
            Init(buf, remoteEP);
        }

        public IncomingMessage(RdpConnection rdpConn) {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buf = rdpConn.Receive(ref remoteEP);
            Init(buf, 0, buf.Length, remoteEP);
        }

        public IncomingMessage(Stream readStream) {
            reader = new BinaryReader(readStream);
        }

		public WorldMessageType ReadMessageType() {
			return (WorldMessageType)ReadInt32();
		}

        public MasterMessageType ReadMasterMessageType() {
            return (MasterMessageType)ReadInt32();
        }

        public WorldTcpMessageType ReadWorldTcpMessageType() {
            return (WorldTcpMessageType)ReadInt32();
        }

		public long ReadTimestamp() {
            long timestamp = ReadInt64();
            long updatedTimestamp = NetworkHelper.Instance.AdjustTimestamp(timestamp);
            return updatedTimestamp;
        }

        public long ReadInt64() {
			return IPAddress.NetworkToHostOrder(reader.ReadInt64());
		}

		public int ReadInt32() {
			return IPAddress.NetworkToHostOrder(reader.ReadInt32());
		}

		public uint ReadUInt32() {
			return (uint)IPAddress.NetworkToHostOrder(reader.ReadInt32());
		}

		public short ReadInt16() {
			return IPAddress.NetworkToHostOrder(reader.ReadInt16());
		}
		
		public ushort ReadUInt16() {
			return (ushort)IPAddress.NetworkToHostOrder(reader.ReadInt16());
		}
		
		public float ReadSingle() {
			int val = ReadInt32();
            byte[] data = BitConverter.GetBytes(val);
            return BitConverter.ToSingle(data, 0);
        }

		public double ReadDouble() {
			long val = ReadInt64();
            byte[] data = BitConverter.GetBytes(val);
            return BitConverter.ToDouble(data, 0);
        }

		public bool ReadBool() {
			return ReadInt32() == 1;
		}

        public byte ReadByte() {
            return reader.ReadByte();
        }

		public bool ReadUShort() {
			return ReadInt32() == 1;
		}
		
		public Vector3 ReadVector() {
			float x = ReadSingle();
			float y = ReadSingle();
			float z = ReadSingle();
			return new Vector3(x, y, z);
        }

        public IntVector3 ReadIntVector() {
			int x = ReadInt32();
			int y = ReadInt32();
			int z = ReadInt32();
			return new IntVector3(x, y, z);
        }

        public Quaternion ReadQuaternion() {
			float x = ReadSingle();
			float y = ReadSingle();
			float z = ReadSingle();
            float w = ReadSingle();
            return new Quaternion(w, x, y, z);
        }

		public string ReadString() {
			return Encoding.UTF8.GetString(ReadBytes());
		}

		public byte[] ReadBytes() {
			int len = ReadInt32();
			return reader.ReadBytes(len);
		}

		public ColorEx ReadColor() {
			ColorEx color = new ColorEx();
			color.a = (float)reader.ReadByte() / 255;
			color.b = (float)reader.ReadByte() / 255;
			color.g = (float)reader.ReadByte() / 255;
			color.r = (float)reader.ReadByte() / 255;
			return color;
		}
	}
}
