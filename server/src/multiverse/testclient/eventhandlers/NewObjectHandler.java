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
import multiverse.server.math.*;
import multiverse.server.events.*;
import multiverse.testclient.*;

import java.util.*;

public class NewObjectHandler implements EventHandler {
    public NewObjectHandler() {
    }

    public String getName() {
	return "NewObjectHandler";
    }

    public boolean handleEvent(Event event) {
	NewObjectEvent objEvent = (NewObjectEvent) event;
	TestUser user = ObjectManager.getObject(event.getConnection());
	String myName = user.getName();

	long oid = objEvent.objOid;
	String name = objEvent.objName;
	Point loc = objEvent.objLoc;

	// see if in entity manager
	MVObject obj = MVObject.getObject(oid, 0);
	if (obj == null) {
	    // insert new object into entity map
	    obj = new MVObject(oid);
	    obj.setName(name);
	    obj.setOid(oid);
	    MVObject.addObject(obj);
            if (Log.loggingDebug)
                Log.debug("NewObjectHandler: " + name + " added with oid " + oid);
	}
	else {
	    // set to new location
            if (Log.loggingDebug)
                Log.debug("NewObjectHandler: " + name + " already exists, new loc: " + loc);
	}

	WorldNode node = obj.getWorldNode();
	if (node == null) {
	    node = new WorldNode();
	    node.addObject(obj);

	    // set the backreference
	    obj.setWorldNode(node);

	    // add to world
	    WorldNode.NodeManager.addNode(node);
	}
	node.setLoc(loc);

	return true;
    }
}
