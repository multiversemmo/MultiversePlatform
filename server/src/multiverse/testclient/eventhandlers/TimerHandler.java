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
import multiverse.server.network.*;
import multiverse.testclient.*;
import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.events.*;

import java.util.*;

public class TimerHandler implements EventHandler {
    public TimerHandler() {
    }

    public String getName() {
	return "TimerHandler";
    }

    public boolean handleEvent(Event event) {
	TimerEvent timerEvent = (TimerEvent) event;

	TestUser user = (TestUser) timerEvent.getObject();
	if (user != null) {
	    user.executeEvent(timerEvent);
	}

//	moveUsers();

	// periodically flush the log
	Log.flush();
	return true;
    }

    private static void moveUsers() {
  	List userList = ObjectManager.getAllTestUsers();
 	Iterator iter = userList.iterator();

 	// send a DirLoc message to all test users
 	while (iter.hasNext()) {
 	    TestUser user = (TestUser) iter.next();

 	    // wait until user is logged in
 	    if (user.getLoggedIn()) {
		WorldNode node = user.getWorldNode();
		if (node == null) {
		    throw new MVRuntimeException("TimerEvent: user " + user.getName() + " has no world node");
		}
 		Point loc = node.getLoc();
 		MVVector dir = node.getDir();

 		OrientEvent orientEvent = null;
 		long time = System.currentTimeMillis() - TestClient.getTimeDifference();

 		// Make sure dir has been initialized
 		if (dir == null || dir.isZero()) {
		    dir = TestUser.getRandomDirection(3000.0f);
		    node.setDir(dir);
 		    user.calculateOrientation();
		    orientEvent = new OrientEvent(user, user.getOrientation());
 		}

 		// See if beyond limits
 		if (loc.getX() > 1224000 && dir.getX() > 0.0f ||
 		    loc.getX() < 1124000 && dir.getX() < 0.0f) {
 		    dir.setX(-dir.getX());
 		    node.setDir(dir);
 		    user.calculateOrientation();
		    orientEvent = new OrientEvent(user, user.getOrientation());
 		}
 		if (loc.getZ() > 1124000 && dir.getZ() > 0.0f ||
 		    loc.getZ() < 1024000 && dir.getZ() < 0.0f) {
 		    dir.setZ(-dir.getZ());
 		    node.setDir(dir);
 		    user.calculateOrientation();
		    orientEvent = new OrientEvent(user, user.getOrientation());
 		}

 		// Tell server where we are
		DirLocEvent moveEvent = new DirLocEvent(user, dir, loc, time);

 		try {
 		    if (orientEvent != null) {
 			Log.info("TimerHandler: setting orientation for: " 
 				 + user.getName() + " to " 
 				 + user.getOrientation());
 			user.sendEvent(orientEvent);
 		    }
 		    Log.info("TimerHandler: moving " 
 			     + user.getName() + 
			     " to Dir: " + dir +
 			     " Loc: " + loc);
 		    user.sendEvent(moveEvent);
 		}
 		catch(MVRuntimeException e) {
 		    Log.error("TimerHandler: could not send message: " + e);
 		}

 		// Get ready for next tick
 		loc.setX(loc.getX() + (int)dir.getX());
 		loc.setY(loc.getY() + (int)dir.getY());
 		loc.setZ(loc.getZ() + (int)dir.getZ());
		node.setLoc(loc);
 	    }
 	}
    }
}
