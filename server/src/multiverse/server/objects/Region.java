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
import java.util.*;
import java.util.concurrent.locks.*;

import multiverse.server.util.*;
import multiverse.server.engine.PropertySearch;

/**
Regions are bounded areas of the world with zero or more features.  Regions
are two-dimensional (they have no Y component) and are pinned to the
terrain.  Several features can be assigned to a region such as sound,
light, fog, grass, and water.  A region can have multiple features, but
only one of a given type.  Regions are placed in the world using the
WorldEditor tool.
<p>
Regions have a name, a boundary (ordered list of points), a set of
region configs, and a set of properties.  The region configs describe
the builtin region features applied to the region.  The properties
are the NameValue properties defined in the WorldEditor.  Custom
region triggers are called when the "onEnter" or "onLeave"
properties are set.  See {@link RegionTrigger}.
*/
public class Region implements Serializable
{
    public Region() {
        setupTransient();
    }

    public Region(String name) {
        setupTransient();
        setName(name);
    }
    
    private void setupTransient() {
        lock = LockFactory.makeLock("RegionLock");
    }

    private void readObject(java.io.ObjectInputStream in)
            throws java.io.IOException, ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    public String toString() {
        String s = "[Region: name="+name+" ";
        s += getBoundary();
        for (RegionConfig regionConfig : getConfigs()) {
            s += " config=" + regionConfig;
        }
        if (properties != null)
            s += " property count="+properties.size();
        s += "]";
        return s;
    }

    /** Set the region name. */
    public void setName(String name) {
        this.name = name;
    }

    /** Get the region name. */
    public String getName() {
        return name;
    }

    /** Set the region priority.  Determines which region applies in
	the presence of multiple overlapping regions.  Lower numbers
	are higher priority.  The default priority is
        {@link #DEFAULT_PRIORITY}.
     */
    public void setPriority(Integer priority) {
        this.pri = priority;
    }

    /** Get the region priority. */
    public Integer getPriority() {
        return (pri == null) ? DEFAULT_PRIORITY : pri;
    }
    
    public static Integer DEFAULT_PRIORITY = 100;
    
    /** Set the region boundary.  In the world manager, changing a
	region boundary has undefined effect.
    */
    public void setBoundary(Boundary b) {
        this.boundary = (Boundary) b.clone();
    }

    /** Get the region boundary.
    */
    public Boundary getBoundary() {
        return (Boundary) this.boundary.clone();
    }

    /** Add a region config (builtin region feature).  Only one RegionConfig
	of a given type is supported.
    */
    public void addConfig(RegionConfig config) {
        lock.lock();
        try {
            configMap.put(config.getType(), config);
        } finally {
            lock.unlock();
        }
    }

    /** Get the region config by type.
    */
    public RegionConfig getConfig(String type) {
        lock.lock();
        try {
            return configMap.get(type);
        } finally {
            lock.unlock();
        }
    }

    /** Get all the region configs.
    */
    public Collection<RegionConfig> getConfigs() {
        lock.lock();
        try {
            return configMap.values();
        } finally {
            lock.unlock();
        }
    }

    /** Get property value.
        @param key Property name
        @return Property value, null if property does not exist.
        @see #setProperty(String, Serializable)
     */
    public Serializable getProperty(String key)
    {
        if (properties == null)
            return null;
        return properties.get(key);
    }

    /** Set property value.
	@param key Property name.
	@param value Property value.
	@return Previous property value, or null if did not exist.
    */
    public Serializable setProperty(String key, Serializable value)
    {
        if (properties == null)
            properties = new HashMap<String,Serializable>();
        return properties.put(key, value);
    }

    /** Get the property map.  Changes to the return value directly
	affect the region's properties.
    */
    public Map<String,Serializable> getPropertyMapRef()
    {
        return properties;
    }

    /** Set the region property map.  The supplied map is copied.
    */
    public void setProperties(Map<String,Serializable> props)
    {
        if (props != null)
            properties = new HashMap<String,Serializable>(props);
        else
            properties = null;
    }

    /** Get the region boundary (search selection flag). */
    public static final long PROP_BOUNDARY = 1;

    /** Get the region properties (search selection flag). */
    public static final long PROP_PROPERTIES = 2;

    /** Get all region information (search selection flag). */
    public static final long PROP_ALL = (PROP_BOUNDARY | PROP_PROPERTIES);

    /** Region search parameters.  Search parameters include instance
        oid (required) and region properties.  The search returns
        {@link Region} objects with name and priority.  Additional
        information is returned by setting the SearchSelection flags
        to {@link #PROP_BOUNDARY}, {@link #PROP_PROPERTIES}, or
        {@link #PROP_ALL}.
        @see multiverse.server.engine.SearchManager#searchObjects
    */
    public static class Search extends PropertySearch
    {
        public Search()
        {
        }

        public Search(long instanceOid, Map queryProps)
        {
            super(queryProps);
            setInstanceOid(instanceOid);
        }

        public long getInstanceOid()
        {
            return instanceOid;
        }
        public void setInstanceOid(long oid)
        {
            instanceOid = oid;
        }
        private long instanceOid;
    }

    /** Region object type. */
    public static final ObjectType OBJECT_TYPE =
        ObjectType.intern((short)22,"Region");

    private transient Lock lock = null;
    private String name = null;
    private Integer pri = DEFAULT_PRIORITY;
    private Boundary boundary = null;
    // type -> regionconfig
    private Map<String, RegionConfig> configMap = new HashMap<String, RegionConfig>();
    private Map<String,Serializable> properties;

    private static final long serialVersionUID = 1L;
}
