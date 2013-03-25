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

using log4net;

using Axiom.MathLib;
using Axiom.Core;

using Multiverse.Config;
using Multiverse.MathLib;
using Multiverse.Network.Rdp;

#endregion

namespace Multiverse.Network
{
    public class OutgoingMessage
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OutgoingMessage));

        private BinaryWriter writer;
        private MemoryStream memStream;

        public OutgoingMessage() {
            memStream = new MemoryStream();
            writer = new BinaryWriter(memStream);
        }

		public void Write(long value) {
			// Console.WriteLine("Sending int: {0}/{1}", value, IPAddress.HostToNetworkOrder(value));
			writer.Write(IPAddress.HostToNetworkOrder(value));
		}

		public void Write(int value) {
			// Console.WriteLine("Sending int: {0}/{1}", value, IPAddress.HostToNetworkOrder(value));
			writer.Write(IPAddress.HostToNetworkOrder(value));
		}

		public void Write(uint value) {
			// Console.WriteLine("Sending int: {0}/{1}", value, IPAddress.HostToNetworkOrder(value));
			writer.Write(IPAddress.HostToNetworkOrder((int)value));
		}

		public void Write(short value) {
			// Console.WriteLine("Sending int: {0}/{1}", value, IPAddress.HostToNetworkOrder(value));
			writer.Write(IPAddress.HostToNetworkOrder(value));
		}

		public void Write(ushort value) {
			// Console.WriteLine("Sending int: {0}/{1}", value, IPAddress.HostToNetworkOrder(value));
			writer.Write(IPAddress.HostToNetworkOrder((short)value));
		}

		public void Write(string value) {
            Write(Encoding.UTF8.GetBytes(value));
		}

        public void Write(byte value) {
            writer.Write(value);
        }

        public void Write(byte[] value) {
			Write(value.Length);
			writer.Write(value);
		}

		public void Write(MasterMessageType value) {
			Write((int)value);
		}

        public void Write(WorldMessageType value) {
            Write((int)value);
        }

        public void Write(WorldTcpMessageType value) {
            Write((int)value);
        }

        public void Write(bool value) {
            if (value)
                Write(1);
            else
                Write(0);
        }

        public void Write(float value) {
            byte[] data = BitConverter.GetBytes(value);
            int val = BitConverter.ToInt32(data, 0);
            Write(val);
        }

        public void Write(double value) {
            byte[] data = BitConverter.GetBytes(value);
            long val = BitConverter.ToInt64(data, 0);
            Write(val);
        }

        public void Write(IntVector3 value) {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }

        public void Write(Vector3 value) {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }

        public void Write(Quaternion value) {
			Write(value.x);
			Write(value.y);
            Write(value.z);
            Write(value.w);
        }

		public void Write(ColorEx color) {
			writer.Write((byte)(color.a * 255));
			writer.Write((byte)(color.b * 255));
			writer.Write((byte)(color.g * 255));
			writer.Write((byte)(color.r * 255));
		}

		public void Send(UdpClient udpClient) {
            writer.Flush();
            udpClient.Send(memStream.ToArray(), (int)memStream.Length);
        }

        public void Send(RdpConnection rdpConn) {
            writer.Flush();
			try {
				rdpConn.Send(memStream.ToArray());
			} catch (RdpFragmentationException) {
				log.Error("Failed to send packet that was too large");
			}

        }
        public void Send(TcpClient tcpClient) {
            writer.Flush();
            tcpClient.GetStream().Write(memStream.ToArray(), 0, (int)memStream.Length);
        }
        public byte[] GetBytes() {
            writer.Flush();
            byte[] rv = memStream.ToArray();
            Debug.Assert((int)memStream.Length == rv.Length);
            return rv;
        }
    }
}
