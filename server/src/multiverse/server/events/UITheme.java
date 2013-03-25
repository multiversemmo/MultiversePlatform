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
import java.util.*;
import java.util.concurrent.locks.*;

public class UITheme extends Event {

    public UITheme() {
	super();
    }

    public UITheme(MVByteBuffer buf, ClientConnection con) {
	super(buf, con);
    }

    public UITheme(List<String> uiThemes) {
	super();
        setThemes(uiThemes);
    }

    public String getName() {
	return "UITheme";
    }

    public void setThemes(List<String> uiThemes) {
        lock.lock();
        try {
            this.uiThemes = new LinkedList<String>(uiThemes);
        }
        finally {
            lock.unlock();
        }
    }
    public void addTheme(String theme) {
        lock.lock();
        try {
            uiThemes.add(theme);
        }
        finally {
            lock.unlock();
        }
    }
    public List<String> getThemes() {
        lock.lock();
        try {
            return new LinkedList<String>(uiThemes);
        }
        finally {
            lock.unlock();
        }
    }
    List<String> uiThemes = new LinkedList<String>();

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());

	int themeCount = uiThemes.size();
        MVByteBuffer buf = new MVByteBuffer(200 * themeCount + 20);
	buf.putLong(-1);
	buf.putInt(msgId);

	// data
        lock.lock();
        try {
            buf.putInt(themeCount);
            Iterator<String> iter = uiThemes.iterator();
            while(iter.hasNext()) {
                String theme = iter.next();
                buf.putString(theme);
            }
            buf.flip();
            return buf;
        }
        finally {
            lock.unlock();
        }
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	buf.getLong(); // dummy id
	/* int msgId = */ buf.getInt();

        List<String> uiThemes = new LinkedList<String>();
        int numThemes = buf.getInt();
        while(numThemes > 0) {
            String theme = buf.getString();
            uiThemes.add(theme);
            numThemes--;
        }
        setThemes(uiThemes);
    }

    transient Lock lock = LockFactory.makeLock("UIThemeEventLock");
}
