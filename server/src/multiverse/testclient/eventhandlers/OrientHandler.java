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
import multiverse.server.math.*;
import multiverse.server.network.*;
import multiverse.server.objects.*;
import multiverse.server.events.*;
import multiverse.testclient.*;

import java.net.*;
import java.util.*;

public class OrientHandler implements EventHandler {
    public OrientHandler() {
    }

    public String getName() {
	return "OrientHandler";
    }

    public boolean handleEvent(Event event) {
	OrientEvent orientEvent = (OrientEvent) event;
	TestUser user = ObjectManager.getObject(event.getConnection());
	String myName = user.getName();

	Quaternion q = orientEvent.getQuaternion();
	String senderName = "unknown";

	MVObject sender = event.getObject();
	if (sender != null) {
	    senderName = sender.getName();
	}

        if (Log.loggingDebug)
            Log.debug("OrientHandler: " + myName + 
                      " notified about: " + senderName +
                      " facing: " + q);

	return true;
    }
}
