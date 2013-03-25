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

import multiverse.server.network.*;
import multiverse.server.network.rdp.*;
import multiverse.server.objects.*;
import multiverse.server.engine.*;
import java.util.*;
import java.net.*;

public class ObjectManager {

    public ObjectManager() {
    }

    // get object from the map
    public static TestUser getObject(RDPConnection con) {
	TestUser obj;
	synchronized(objMap) {
	    obj = (TestUser) objMap.get(con);
	}
	return obj;
    }

    public static List getAllTestUsers() {
	synchronized(objMap) {
	    List returnList = new LinkedList();
	    Collection allObjs = objMap.values();

	    // copy test users into the returnList
	    Iterator iter = allObjs.iterator();
	    while (iter.hasNext()) {
		MVObject cur = (MVObject) iter.next();
		if (cur instanceof TestUser) {
		    returnList.add(cur);
		}
	    }
	    return returnList;
	}
    }

    public static TestUser findTestUser(String name) {
	synchronized(objMap) {
	    Collection allObjs = objMap.values();

	    // test users for matching name
	    Iterator iter = allObjs.iterator();
	    while (iter.hasNext()) {
		TestUser cur = (TestUser) iter.next();
		if (name.equals(cur.getName())) {
		    return cur;
		}
	    }
	    return null;
	}
    }

    public static MVObject findObject(String name) {
	Collection<MVObject> allObjs = MVObject.getAllObjects();

	// test users for matching name
	Iterator<MVObject> iter = allObjs.iterator();
	while (iter.hasNext()) {
	    MVObject cur = iter.next();
	    if (name.equals(cur.getName())) {
		return cur;
	    }
	}
	return null;
    }

    // places object into the map - use only if you know what you are doing
    public static void mapPut(TestUser obj, RDPConnection con) {
	synchronized(objMap) {
	    // see if the object is already in the map
	    if (objMap.get(con) == null) {
		objMap.put(con, obj);
	    }
	}
    }

    public static void mapRemove(RDPConnection con) {
	Log.info("ObjectManager.mapRemove: removing user from map");
	synchronized(objMap) {
	    objMap.remove(con);
	}
    }

    // map of active objects (RDPConnection -> User object)
    private static Map objMap = new HashMap();
}
