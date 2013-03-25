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

package multiverse.server.events;

import multiverse.server.engine.*;
import multiverse.server.math.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;

/**
 * the world server is telling a different server about a new object
 * without serializing the obj - we dont serialize to the entitymgr
 * because it doesnt need the full object
 */
public class NewObjectEvent extends Event {

    public NewObjectEvent() {
	super();
    }

    public NewObjectEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public NewObjectEvent(MVObject notifyObj, MVObject obj) {
	super(notifyObj);
	this.objOid = obj.getOid();
	this.objName = obj.getName();
	this.objLoc = obj.getLoc();
	this.objOrient = obj.getOrientation();
	this.objScale = obj.scale();
// 	this.objDisplayContext = obj.getDisplayContext();
	this.objType = NewObjectEvent.getObjectType(obj);

// 	Log.debug("newobject event - notifyObj=" + notifyObj +
// 		  " about newObj " + obj + ", objType=" + objType +
// 		  ", notifyObjCon=" + notifyObj.getRemoteObjectConnection());

	if (obj.isMob() || obj.isItem()) {
	    this.objFollowsTerrain = true;
	}
	else {
	    this.objFollowsTerrain = false;
	}
    }

    // from parent
    public String getName() {
	return "NewObjectEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);
	buf.putLong(objOid);
	
	// name of the mob
	buf.putString((objName == null) ? "unknown" : objName);

	// location
	Point loc = objLoc;
	buf.putPoint((loc == null) ? (new Point()) : loc);
	
	// orientation
	Quaternion orient = objOrient;
	buf.putQuaternion((orient == null) ? (new Quaternion()) : orient);

	// scale
	buf.putMVVector(objScale);
	buf.putInt(objType);
	buf.putInt((objFollowsTerrain)?1:0);

        // display context is moved out of the new object event
        // and now is in meshinfo event, so that when you
        // equip a new rigged attachment, it sends out a meshinfo event
        // with the update
// 	// display context
//         buf.putString(objDisplayContext.getMeshFile());
//         Set<DisplayContext.Submesh> submeshes = objDisplayContext.getSubmeshes();
//         buf.putInt(submeshes.size());
//         Iterator<DisplayContext.Submesh> sIter = submeshes.iterator();
//         while (sIter.hasNext()) {
//             DisplayContext.Submesh submesh = sIter.next();
//             buf.putString(submesh.name);
//             buf.putString(submesh.material);                
//         }
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setObjectOid(buf.getLong());

	/* int msgId = */ buf.getInt();

	objOid = buf.getLong();
	objName = buf.getString();
	objLoc = buf.getPoint();
	objOrient = buf.getQuaternion();
	objScale = buf.getMVVector();

	objType = buf.getInt();
	objFollowsTerrain = (buf.getInt() == 1);
        
//         // display context
//         objDisplayContext = new DisplayContext();
//         objDisplayContext.setMeshFile(buf.getString());
//         Log.debug("NewObjectEvent.parseBytes: objname=" + 
//                   objName +
//                   ", meshfile=" + objDisplayContext.getMeshFile());
//         Set<DisplayContext.Submesh> submeshes = new HashSet<DisplayContext.Submesh>();
//         int numSubmeshes = buf.getInt();
//         while(numSubmeshes > 0) {
//             String name = buf.getString();
//             String material = buf.getString();
//             DisplayContext.Submesh submesh = new DisplayContext.Submesh(name, material);
//             submeshes.add(submesh);
//             numSubmeshes--;
//         }
//         objDisplayContext.setSubmeshes(submeshes);
    }
    
    public Long objOid;
    public String objName;
    public Point objLoc;
    public Quaternion objOrient;
    public MVVector objScale;
//     public DisplayContext objDisplayContext;
    public int objType;
    public boolean objFollowsTerrain;

    // maps the object to its typeid used in this event
    public static int getObjectType(MVObject obj) {

	// i would like to use a lookup map, but perhaps a developer will
	// extend the class and then it wont match the class value
	if (obj.isUser()) {
	    return 3;
	}
// 	Log.debug("NewObjectEvent: newobj=" + obj + ", isMob=" + obj.isMob());
	if (obj.isMob()) {
	    return 1;
	}
	if (obj.isItem()) {
	    return 2;
	}
	if (obj.isStructure()) {
	    return 0;
	}
	throw new RuntimeException("NewObjectEvent: unknown obj type: " + obj);
    }
    
    // prints the name of the type based on the integer
    public static String objectTypeToName(int id) {
        switch(id) {
        case 0:
            return "Structure";
        case 1:
            return "Mob";
        case 2:
            return "Item";
        case 3:
            return "User";
        }
        return "Unknown";
    }
}
