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

package multiverse.mars.core;

import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;
import multiverse.server.util.*;
import multiverse.server.objects.*;
import multiverse.server.marshalling.*;
import multiverse.server.network.*;

/**
 * maps a base DC to an item specific DC
 * you have an item like a leather tunic, and you
 * want to put that on a human female.  which meshes do you use?
 * this maps from a base dc (like human_female.mesh and the nude submeshes)
 * to the dc you should use for the item, such as 
 * human_female_tunic.mesh
 *
 * at some point we want to add 'state' like whether you are in combat or not
 * and also equipslot into the mix
 */
public class DCMap implements Serializable, Marshallable {

    public DCMap() {
    }

    public void add(DisplayContext base, DisplayContext target) {
	lock.lock();
	try {
	    map.put(base, target);
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * returns corresponding DC.  if there is none,
     * this method will return the defaultDC (possibly null)
     * there is a match when the map has an entry that is a
     * subset of the passed in base (not the other way around).
     * usually the base of a mob has a head and other parts which
     * we should disregard
     */
    public DisplayContext get(DisplayContext base) {
	lock.lock();
	try {
	    // trivial case - exact match
	    DisplayContext dc = map.get(base);
	    if (dc != null) {
		return dc;
	    }
	    
	    // subset match
	    for (Map.Entry<DisplayContext, DisplayContext> entry :
		     map.entrySet()) {
		DisplayContext key = entry.getKey();
		if (key.subsetOf(base)) {
		    return entry.getValue();
		}
	    }

	    // default match
	    return defaultDC;
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * sometimes you want to set a default mapping.
     * this is used when there is no match for the base model.
     * this is useful when you have an item that is always the same
     * regardless of the base mesh
     */
    public DisplayContext getDefault() {
	return defaultDC;
    }
    public void setDefault(DisplayContext dc) {
	this.defaultDC = dc;
    }

    // for java beans xml serialization support
    public Map<DisplayContext, DisplayContext> getMap() {
	lock.lock();
	try {
	    return new HashMap<DisplayContext,DisplayContext>(map);
	}
	finally {
	    lock.unlock();
	}
    }
    public void setMap(Map<DisplayContext, DisplayContext> map) {
	lock.lock();
	try {
	    this.map = new HashMap<DisplayContext,DisplayContext>(map);
	}
	finally {
	    lock.unlock();
	}
    }

    public void marshalObject(MVByteBuffer buf) {
        byte flags = (byte)((defaultDC == null ? 0 : 1) | (map == null ? 0 : 2));
        buf.putByte(flags);
        if (defaultDC != null)
            MarshallingRuntime.marshalMarshallingObject(buf, defaultDC);
        if (map != null)
            MarshallingRuntime.marshalHashMap(buf, map);
    }

    public Object unmarshalObject(MVByteBuffer buf) {
        byte flags = buf.getByte();
        if ((flags & 1) != 0) {
            MarshallingRuntime.unmarshalMarshallingObject(buf, defaultDC);
        }
        if ((flags & 2) != 0) {
            map = (HashMap<DisplayContext, DisplayContext>)MarshallingRuntime.unmarshalHashMap(buf);
        }
        return this;
    }

    DisplayContext defaultDC;
    Map<DisplayContext, DisplayContext> map = 
	new HashMap<DisplayContext, DisplayContext>();
    Lock lock = LockFactory.makeLock("DCMap");
    private static final long serialVersionUID = 1L;
}
