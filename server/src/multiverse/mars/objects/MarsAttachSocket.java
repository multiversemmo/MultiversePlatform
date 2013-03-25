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

package multiverse.mars.objects;

import multiverse.server.util.*;
import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;

public class MarsAttachSocket implements Serializable {
    public MarsAttachSocket() {
    }

    public MarsAttachSocket(String socketName) {
	this.name = socketName;
	mapLock.lock();
	try {
	    socketNameMapping.put(socketName, this);
	}
	finally {
	    mapLock.unlock();
	}
    }

    public void setName(String name) {
        this.name = name;
    }
    public String getName() {
	return name;
    }
    private String name;

    public String toString() {
	return "[MarsAttachSocket name=" + getName() + "]";
    }

    public static MarsAttachSocket getSocketByName(String socketName) {
	mapLock.lock();
	try {
	    return socketNameMapping.get(socketName);
	}
	finally {
	    mapLock.unlock();
	}
    }

    private static Map<String, MarsAttachSocket> socketNameMapping =
	new HashMap<String, MarsAttachSocket>();


    private static Lock mapLock = LockFactory.makeLock("MarsAttachSocketLock");

    public static MarsAttachSocket PRIMARYWEAPON = 
	new MarsAttachSocket("primaryWeapon");

    private static final long serialVersionUID = 1L;
}
