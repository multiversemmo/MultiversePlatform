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

import multiverse.server.util.*;
import java.io.*;
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * regions are 'areas' in the world with specific attributes
 * they all contain a boundary, and also config data for that area
 * regions can have multiple configs, like for trees, sounds, lights, etc.
 *
 * RegionConfig is for config objects, such as a SoundConfig
 */

public class RegionConfig implements Serializable {

    public RegionConfig() {
        setupTransient();
    }

    public RegionConfig(String type) {
        setupTransient();
        setType(type);
    }

    // called from constructor and readObject
    private void setupTransient() {
        lock = LockFactory.makeLock("RegionConfigLock");
    }
    
    /**
     * private method to recreate the lock when deserializing
     */
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }
    
    public String toString() {
        return "[RegionConfig type="+type+"]";
    }

    public String getType() {
	return type;
    }

    public void setType(String type) {
	this.type = type;
    }

    public void setProperty(String key, Object value) {
        lock.lock();
        try {
            propMap.put(key, value);
        }
        finally {
            lock.unlock();
        }
    }

    public Object getProperty(String key) {
        lock.lock();
        try {
            return propMap.get(key);
        }
        finally {
            lock.unlock();
        }
    }

    private Map<String, Object> propMap = new HashMap<String, Object>();
    private String type;
    transient protected Lock lock;

    private static final long serialVersionUID = 1L;
}
