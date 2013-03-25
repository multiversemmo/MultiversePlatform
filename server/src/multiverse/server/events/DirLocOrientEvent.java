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

public class DirLocOrientEvent extends Event {
    public DirLocOrientEvent() {
	super();
    }

    public DirLocOrientEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public DirLocOrientEvent(MVObject obj, MVVector dir, Point loc, Quaternion q, long time) {
	super(obj);
	setDir(dir);
	setLoc(loc);
	setQuaternion(q);
    }

    public DirLocOrientEvent(Long objOid, BasicWorldNode wnode) {
	super(objOid);
	setDir(wnode.getDir());
	setLoc(wnode.getLoc());
	setQuaternion(wnode.getOrientation());
    }

    public String getName() {
	return "DirLocOrientEvent";
    }

    public String toString() {
        return "[DirLocOrientEvent: oid=" + getObjectOid() + ", dir=" + getDir() + ", loc=" + getLoc() +
            ", orient=" + q + "]";        
    }
    
    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	MVByteBuffer buf = new MVByteBuffer(80);
	buf.putLong(getObjectOid());
	buf.putInt(msgId);

        buf.putLong(System.currentTimeMillis());
        buf.putMVVector(getDir());
	buf.putPoint(getLoc());
	buf.putQuaternion(getQuaternion());
        buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	
	long oid = buf.getLong();
	setObjectOid(oid);
	/* int msgId = */ buf.getInt();
	buf.getLong();		//  read in the time
	setDir(buf.getMVVector());
	setLoc(buf.getPoint());
        setQuaternion(buf.getQuaternion());
    }

    public void setDir(MVVector v) {
	dir = v;
    }
    public MVVector getDir() {
	return dir;
    }

    public void setLoc(Point p) {
	loc = p;
    }
    public Point getLoc() {
	return loc;
    }

    public void setQuaternion(Quaternion q) {
	this.q = q;
    }
    public Quaternion getQuaternion() {
	return q;
    }
    
    private MVVector dir = null;
    private Point loc = null;
    private Quaternion q = null;
}
