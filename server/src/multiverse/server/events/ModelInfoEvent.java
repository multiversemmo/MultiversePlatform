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
import multiverse.server.util.*;

import java.util.*;

/**
 * send out what meshes to draw for the given object
 * it is a full update, so if you unequip a rigged attachment,
 * a full update is sent out
 */
public class ModelInfoEvent extends Event {
    public ModelInfoEvent() {
        super();
    }

    public ModelInfoEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public ModelInfoEvent(MVObject obj) {
	super(obj);
        setDisplayContext((DisplayContext)obj.displayContext().clone());
    }

    public ModelInfoEvent(Long objOid) {
	super(objOid);
    }

    public String getName() {
        return "ModelInfoEvent";
    }

    public void setDisplayContext(DisplayContext dc) {
        this.dc = dc;
    }
    public DisplayContext getDisplayContext() {
        return dc;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(400);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);
        buf.putInt(1); // number of meshfiles

	// display context
        DisplayContext dc = getDisplayContext();
        buf.putString(dc.getMeshFile());
        if (Log.loggingDebug)
            log.debug("ModelInfoEvent.toBytes: meshfile=" + dc.getMeshFile());

        Set<DisplayContext.Submesh> submeshes = dc.getSubmeshes();
        int submeshLen = submeshes.size();
        buf.putInt(submeshLen);
        if (Log.loggingDebug)
            log.debug("ModelInfoEvent.toBytes: submeshLen=" + submeshLen);

        int castShadow = dc.getCastShadow() ? 1 : 0;
        int receiveShadow = dc.getReceiveShadow() ? 1 : 0;
        Iterator<DisplayContext.Submesh> sIter = submeshes.iterator();
        while (sIter.hasNext()) {
            DisplayContext.Submesh submesh = sIter.next();
            buf.putString(submesh.name);
            buf.putString(submesh.material);
	    buf.putInt(castShadow);
	    buf.putInt(receiveShadow);
            if (Log.loggingDebug)
                log.debug("ModelInfoEvent.toBytes: submeshName=" +
                          submesh.name +
                          ", material=" + submesh.material +
			  ", castShadow=" + castShadow +
			  ", receiveShadow=" + receiveShadow );
        }
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setObjectOid(buf.getLong());
	/* int msgId = */ buf.getInt();
        int meshFiles = buf.getInt();
        if (meshFiles != 1) {
            throw new MVRuntimeException("more than 1 meshfile is not supported");
        }
        // display context
        DisplayContext dc = new DisplayContext();
        dc.setMeshFile(buf.getString());
        if (Log.loggingDebug)
            log.debug("parseBytes: objOid=" + getObjectOid() +
                      ", meshfile=" + dc.getMeshFile());
        Set<DisplayContext.Submesh> submeshes = 
            new HashSet<DisplayContext.Submesh>();
        int numSubmeshes = buf.getInt();
        while(numSubmeshes > 0) {
            String name = buf.getString();
            String material = buf.getString();
            DisplayContext.Submesh submesh = new DisplayContext.Submesh(name, material);
            submeshes.add(submesh);
            numSubmeshes--;
        }
        dc.setSubmeshes(submeshes);
    }        

    protected DisplayContext dc = null;
    protected static final Logger log = new Logger("ModelInfoEvent");
}
