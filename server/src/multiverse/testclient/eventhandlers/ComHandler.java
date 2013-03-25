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

package multiverse.testclient.eventhandlers;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.events.*;
import multiverse.testclient.*;

import java.util.*;

public class ComHandler implements EventHandler {
    public ComHandler() {
    }

    public String getName() {
	return "ComHandler";
    }

    public boolean handleEvent(Event event) {
	ComEvent comEvent = (ComEvent) event;
	TestUser user = ObjectManager.getObject(event.getConnection());
	String myName = user.getName();

	long myId = user.getOid();
	String msg = comEvent.getMessage();
	String senderName = "unknown";

	MVObject sender = event.getObject();
	if (sender != null) {
	    senderName = sender.getName();
	}

	Log.info("ComHandler: " + myName + 
		 " received message: " + msg +
		 " from: " + senderName);

	return true;
    }
}
