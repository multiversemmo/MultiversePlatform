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

package multiverse.server.network;

import java.net.*;
import java.io.*;
import java.nio.channels.*;

import multiverse.server.util.*;

/**
 * binds on a port and accepts new connections on its own private thread.
 * calls registered callback on accepting a new connection.
 */
public class TcpServer {
    public TcpServer() {
    }

    public TcpServer(int port) {
	try {
	    bind(port);
 	}
	catch(IOException e) {
	    throw new RuntimeException("TcpServer contructor bind failed", e);
	}
    }

    /**
     * binds to a random local port
     */
    public void bind() throws IOException {
	ssChannel = ServerSocketChannel.open();
	ssChannel.socket().bind(null);
        if (Log.loggingDebug)
            log.debug("bound to port: " + ssChannel.socket().getLocalPort());
    }

    /**
     * binds to tcp port, starts accepting incoming connections
     */
    public void bind(int port) throws IOException {
	ssChannel = ServerSocketChannel.open();
	ssChannel.socket().bind(new InetSocketAddress(port));
        if (Log.loggingDebug)
            log.debug("bound to port: " + port);
    }

    public int getPort() {
	return ssChannel.socket().getLocalPort();
    }

    public void registerAcceptCallback(TcpAcceptCallback cb) {
	acceptCallback = cb;
    }

    public Thread getThread()
    {
        return thread;
    }

    /**
     * start processing new incoming connections and
     * also handle requests
     */
    public void start() {
	if (acceptCallback == null) {
	    throw new RuntimeException("no registered accept callback");
	}

	// make a thread for handling new connections
	Runnable run = new Runnable() {
		public void run() {
		    try {
			while (true) {
			    SocketChannel sc = ssChannel.accept();
			    sc.configureBlocking(false);
			    acceptCallback.onTcpAccept(sc);
			}
		    }
		    catch(Exception e) {
			log.exception("TcpServer.run caught exception", e);
		    }
		}
	    };
	thread = new Thread(run, "TcpAccept");
        thread.setDaemon(true);
	thread.start();
    }

    protected TcpAcceptCallback acceptCallback = null;
    protected ServerSocketChannel ssChannel = null;
    protected Thread thread;
    protected static final Logger log = new Logger("TcpServer");
}
