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

import multiverse.server.objects.*;
import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.network.*;
import multiverse.server.events.*;
import multiverse.testclient.*;
import java.net.*;

public class LoginRespHandler implements EventHandler {
    public LoginRespHandler() {
    }

    public String getName() {
	return "LoginRespHandler";
    }

    public boolean handleEvent(Event event) {
	LoginResponseEvent loginRespEvent = (LoginResponseEvent) event;
	TestUser user = ObjectManager.getObject(event.getConnection());
	String myName = user.getName();

	// get the user's new id
	long uid = loginRespEvent.getOid();
	boolean success = loginRespEvent.getSuccessStatus();

        if (Log.loggingDebug)
            Log.debug("LoginRespHandler: response for uid " + uid);

	if(user != null && success){
	    user.setLoggedIn(true);

	    // set focus to first user to login
	    if( TestClient.getUserFocus() == null ) {
		TestClient.setUserFocus(user);
	    }
	}

	return true;
    }
}
