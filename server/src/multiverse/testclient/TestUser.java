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

// TestUser object

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.objects.*;
import multiverse.server.math.*;
import java.util.*;

public class TestUser extends MVObject {
    public TestUser(long oid) {
	super(oid);
	this.isUser(true);
	try {
	    BaseBehavior behave = new BaseBehavior();
	    behave.setObject(this);
	    this.setBehavior(behave);
	}
	catch (Exception e) {
	    Log.error("TestUser: user " + this.getName() + " could not set behavior");
	}
    }

    public void executeEvent(Event event) {
	lock.lock();
	try {
	    ArrayList<MVEventListener> list = listenerMap.get(event.getClass());
	    Iterator<MVEventListener> iter = list.iterator();
	    while (iter.hasNext()) {
		try {
		    iter.next().handleEvent(event, this);
		}
		catch (Exception e) {
		    Log.error("TestUser: user " + this.getName() + " unable to process event: " + event);
		}
	    }
	}
	finally {
	    lock.unlock();
	}
    }

    public void setLoggedIn(boolean value) {
	loggedIn = value;
    }
    public boolean getLoggedIn() {
	return loggedIn;
    }

    public void setTalkingId(long Id) {
	talkingWithId = Id;
    }
    public long getTalkingId() {
	return talkingWithId;
    }

    public void calculateOrientation() {
	WorldNode node = this.getWorldNode();
	if (node == null) {
	    Log.error("TestUser: user " + this.getName() + " has no world node");
	}
	else {
	    Quaternion newQ = Quaternion.fromVectorRotation(new MVVector(0,0,1),
							    node.getDir());
	    node.setOrientation(newQ);
	    Log.info("TestUser: setOrientation to: " + newQ);
	}
    }

    public static MVVector getRandomDirection(float rate) {
	double angle = generator.nextFloat() * 360.0f;
	float x = (float)Math.sin(angle) * rate;
	float z = (float)Math.cos(angle) * rate;
	MVVector dir = new MVVector(x, 0, z);
	Log.info("TestUser: Random direction = " + dir);
	return dir;
    }

    private static Random generator = new Random(42);
    private boolean loggedIn = false;
    private long talkingWithId = -1;
}
