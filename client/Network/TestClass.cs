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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;

using Multiverse.Network.Rdp;

#endregion

namespace Multiverse.Network
{
    public class TestClass
    {
        TextWriterTraceListener myTextListener;

        protected void SetupDebug() {
            // Create a file for output named TestFile.txt.
            Stream myFile = File.Create("trace.txt");

            /* Create a new text writer using the output stream, and add it to
             * the trace listeners. */
            myTextListener = new TextWriterTraceListener(myFile);
            Trace.Listeners.Add(myTextListener);

            /* Create a text writer that writes to the console screen, and add
            * it to the trace listeners */
            TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            Trace.Listeners.Add(myWriter);
        }

        protected void FinishDebug() {
            if (myTextListener != null) {
                myTextListener.Flush();
                myTextListener.Close();
            }
        }

        public TestClass() {

            SetupDebug();

            // const int ServerPort = 200;
            const int ServerPort = 5001;
            const int ClientPort = 6666;

            IPHostEntry IPHost = Dns.GetHostEntry("cedeno-dxp.corp.multiverse.net");
            IPAddress[] addr = IPHost.AddressList;
            IPEndPoint sendpt = new IPEndPoint(addr[0], ServerPort);


            RdpClient rdpClient = new RdpClient(ClientPort, 100, 1000, true);
            RdpClient rdpClient2 = new RdpClient(ClientPort + 1, 100, 1000, false);
            //RdpServer rdpServer = new RdpServer(ServerPort, 100, 1000, true);

            RdpConnection rdpClientConn = rdpClient.Connect(sendpt);
            RdpConnection rdpClientConn2 = rdpClient2.Connect(sendpt);
            // RdpConnection rdpServerConn = rdpServer.Accept();

            rdpClientConn.WaitForState(ConnectionState.Open);
            rdpClientConn2.WaitForState(ConnectionState.Open);

//            Console.WriteLine("Server State = " + rdpServerConn.ConnectionState);
            Console.WriteLine("Client State = " + rdpClientConn.ConnectionState);

            OutgoingMessage outMessage = new OutgoingMessage();
            outMessage.Write("Good morning");
            outMessage.Send(rdpClientConn);

            outMessage.Send(rdpClientConn2);

            // byte[] msg = Encoding.ASCII.GetBytes("Test");

            // rdpClientConn.Send(msg);
            // rdpClientConn.Send(msg);
            // rdpClientConn.Send(msg);

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            IncomingMessage inMessage;

            inMessage = new IncomingMessage(rdpClientConn);
            Console.WriteLine("Got message: {0}", inMessage.ReadString());

            inMessage = new IncomingMessage(rdpClientConn2);
            Console.WriteLine("Got message2: {0}", inMessage.ReadString());

            //            for (int i = 0; i < 3; ++i) {
//                byte[] rcvdMessage = rdpServerConn.Receive(ref remoteEP);
//                Console.WriteLine("Server got message from {1}: '{0}'", 
//                                  Encoding.ASCII.GetString(rcvdMessage), remoteEP);
//                rdpServerConn.Send(msg);
//            }
//
//            for (int i = 0; i < 3; ++i) {
//                byte[] rcvdMessage = rdpClientConn.Receive(ref remoteEP);
//                Console.WriteLine("Client got message from {1}: '{0}'", 
//                                  Encoding.ASCII.GetString(rcvdMessage), remoteEP);
//            }
//
//            rdpServerConn.Close();

            Thread.Sleep(100);

//            Console.WriteLine("Server State = " + rdpServerConn.ConnectionState);
            Console.WriteLine("Client State = " + rdpClientConn.ConnectionState);

            Thread.Sleep(31000);

//            Console.WriteLine("Server State = " + rdpServerConn.ConnectionState);
            Console.WriteLine("Client State = " + rdpClientConn.ConnectionState);

            FinishDebug();

            //rdpConn.Open(false, 1, 2, 3, 4, true);
            //rdpConn.Send(new Multiverse.Network.Rdp.RdpPacket(50));
            //this.Hide();
            //this.WindowState = FormWindowState.Minimized;
       }
    }
}
