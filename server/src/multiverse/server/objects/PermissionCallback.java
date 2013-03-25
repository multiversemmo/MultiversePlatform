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

package multiverse.server.objects;

import java.io.*;
import multiverse.server.util.*;

/**
 * this object implements callbacks for actions that need to check
 * permissions to do something with an object.
 *
 * you register these callbacks on an object by calling
 * MVObject.setPermissionCallback()
 *
 * each callback object should be used for one and only one mvobject
 * (in case the callback sets state)
 *
 * each object can have its own permissionCallback
 */
public class PermissionCallback implements Serializable {

    public PermissionCallback() {
	setupTransient();
    }

    public PermissionCallback(MVObject obj) {
	thisObj = obj;
	setupTransient();
    }

    // called from constructor and readObject
    private void setupTransient() {
    }

    /**
     * THIS is an object, someone is trying to acquire it.
     * returns true if allowed to acquire this object
     */
    public boolean acquire(MVObject acquirer) {
	return true;
    }

    /**
     * THIS is a container containing an object.  someone is
     * trying to take the object from you.  you are probably a
     * chest of some sort, so you may want to check if you are 
     * locked, etc
     */
    public boolean acquireFrom(MVObject acquirer, MVObject obj) {
	return true;
    }
    
    /**
     * returns true if allowed to drop this object
     */
    public boolean drop(MVObject dropInto) {
	return true;
    }

    /**
     * returns if the user is allowed to equip/use this object 
     */
    public boolean use(Long userOid) {
	return true;
    }

    /**
     * returns true if allowed to destory this object
     */
    public boolean destroy(MVObject destroyer) {
	return true;
    }

    /**
     * private method to recreate the lock when deserializing
     */
    private void readObject(ObjectInputStream in) 
	throws IOException, ClassNotFoundException {
	in.defaultReadObject();
	setupTransient();
    }

    protected MVObject thisObj = null;
    protected static final Logger log = new Logger("MarsPermissionCallback");
    private static final long serialVersionUID = 1L;
}
