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

package multiverse.testclient;

import java.util.*;
import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import java.net.SocketException;
import java.net.InetAddress;

public class ClientServer implements RDPPacketCallback {
    public ClientServer() {
    }

    public void connectionReset(RDPConnection con) {
    }

    public void processPacket(RDPConnection con, RDPPacket packet, MVByteBuffer msg) {
	try {
	    MessageServer msgServer = Engine.getMessageServer();
	    EventServer eventServer = Engine.getEventServer();
    
	    // make a Message
	    Event event = eventServer.parseBytes(msg, con);

	    if (event == null) {
//		Log.debug("TestClient: event server returned null event");
		return;
	    }
	    else {
//		Log.debug("TestClient: parsed new network event, sending to eventserver, name=" + event.getName());

		// now that we have an event, pass it to the eventserver
		eventServer.add(event);
	    }
	}
	catch(MVRuntimeException e) {
	    Log.exception("ClientServer.processPacket caught MVRuntimeException", e);
	}
	catch(Exception e) {
	    Log.exception("ClientServer.processPacket caught exception", e)
	}
    }
}
