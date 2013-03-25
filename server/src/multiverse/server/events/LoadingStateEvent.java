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
import multiverse.server.network.*;
import multiverse.server.util.*;
import java.util.concurrent.locks.*;

/**
 * tells the client when we start and stop loading a new scene
 */
public class LoadingStateEvent extends Event {
    public LoadingStateEvent() {
	super();
    }

    public LoadingStateEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public LoadingStateEvent(boolean loading) {
	super();
	setLoading(loading);
    }

    public String getName() {
	return "LoadingStateEvent";
    }

    public void setLoading(boolean loading) {
        this.loading = loading;
    }

    public boolean getLoading() {
        return loading;
    }

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	MVByteBuffer buf = new MVByteBuffer(40);
        lock.lock();
        try {
            buf.putLong(0); 
            buf.putInt(msgId);
            buf.putBoolean(loading);
            buf.flip();
            return buf;
        } finally {
            lock.unlock();
        }
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
        /* Long oid = */ buf.getLong();
	/* int msgId = */ buf.getInt();
        lock.lock();
        try {
            setLoading(buf.getBoolean());
        } finally {
            lock.unlock();
        }
    }

    private Lock lock = LockFactory.makeLock("LoadingStateEventLock");
    private boolean loading = false;
}
