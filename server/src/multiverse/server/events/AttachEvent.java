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
import multiverse.server.objects.*;
import multiverse.server.network.*;
import java.util.*;

public class AttachEvent extends Event {

    public AttachEvent() {
	super();
    }

    public AttachEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public AttachEvent(MVObject attacher, 
		       MVObject objToAttach, 
		       String socketName) {
	super(attacher);
	setObjToAttachID(objToAttach.getOid());
	setSocketName(socketName);
        setDisplayContext(objToAttach.displayContext());
    }

    public AttachEvent(Long attacherOid, Long attacheeOid, String socketName,
            DisplayContext dc) {
        super(attacherOid);
        setObjToAttachID(attacheeOid);
        setSocketName(socketName);
        setDisplayContext(dc);
    }
    
    public String getName() {
	return "AttachEvent";
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(getAttacherOid());
	buf.putInt(msgId);

	// data
	buf.putLong(getObjToAttachID());
	buf.putString(socketName);

	// display context
        buf.putString(displayContext.getMeshFile());
        Set<DisplayContext.Submesh> submeshes = displayContext.getSubmeshes();
        buf.putInt(submeshes.size());
        Iterator<DisplayContext.Submesh> sIter = submeshes.iterator();
        while (sIter.hasNext()) {
            DisplayContext.Submesh submesh = sIter.next();
            buf.putString(submesh.name);
            buf.putString(submesh.material);                
        }
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setAttacherOid(buf.getLong());
	/* int msgId = */ buf.getInt();
	
	setObjToAttachID(buf.getLong());
	setSocketName(buf.getString());

        // display context
        DisplayContext dc = new DisplayContext();
        dc.setMeshFile(buf.getString());
        Set<DisplayContext.Submesh> submeshes = 
            new HashSet<DisplayContext.Submesh>();
        int numSubmeshes = buf.getInt();
        while(numSubmeshes > 0) {
            String name = buf.getString();
            String material = buf.getString();
            DisplayContext.Submesh submesh = 
                new DisplayContext.Submesh(name, material);
            submeshes.add(submesh);
            numSubmeshes--;
        }
        dc.setSubmeshes(submeshes);
        setDisplayContext(dc);
        
    }

    public void setAttacherOid(long oid) {
	setObjectOid(oid);
    }
    public Long getAttacherOid() {
	return getObjectOid();
    }

    // we make it an ID because really the client doesnt care and
    // its not guaranteed to get the newobject for the attachment object,
    // so it might not even know what it is
    public void setObjToAttachID(long objID) {
	objToAttachID = objID;
    }
    public long getObjToAttachID() {
	return objToAttachID;
    }

    public void setSocketName(String socketName) {
	this.socketName = socketName;
    }
    public String getSocketName() {
	return socketName;
    }

    public void setDisplayContext(DisplayContext dc) {
        this.displayContext = dc;
    }
    public DisplayContext getDisplayContext() {
        return displayContext;
    }

    private long objToAttachID = 0;
    private String socketName = null;
    private DisplayContext displayContext = null;
}
