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

import java.io.Serializable;
import java.util.Map;
import java.util.HashMap;

import multiverse.server.math.Point;
import multiverse.server.math.Quaternion;
import multiverse.server.engine.PropertySearch;

/** A marker is a point in space with orientation and properties.
Markers are placed using the WorldEditor tool.
*/
public class Marker implements Serializable, Cloneable
{
    public Marker()
    {
    }

    public Marker(Point point)
    {
        setPoint(point);
    }

    /** Create a marker.
        @param point Marker location.
        @param orient Marker orientation.
    */
    public Marker(Point point, Quaternion orient)
    {
        setPoint(point);
        setOrientation(orient);
    }

    /** Copy this marker.
    */
    public Object clone()
        throws java.lang.CloneNotSupportedException
    {
        Marker copy = (Marker) super.clone();
        if (copy.point != null)
            copy.point = (Point) copy.point.clone();
        if (copy.orientation != null)
            copy.orientation = (Quaternion) copy.orientation.clone();
        if (copy.properties != null)
            copy.properties = new HashMap<String,Serializable>(copy.properties);
        return copy;
    }

    public String toString()
    {
        return "[Marker pt="+point+" ori="+orientation + " " +
                ((properties == null)?"0 props":properties.size()+ " props]");
    }

    /** Get marker location. */
    public Point getPoint()
    {
        return point;
    }

    /** Set marker location. */
    public void setPoint(Point point)
    {
        this.point = point;
    }

    /** Get marker orientation. */
    public Quaternion getOrientation()
    {
        return orientation;
    }

    /** Set marker orientation. */
    public void setOrientation(Quaternion orient)
    {
        this.orientation = orient;
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
        affect the marker's properties.
    */
    public Map<String,Serializable> getPropertyMapRef()
    {
        return properties;
    }

    /** Set the marker property map.  The map is copied. */
    public void setProperties(Map<String,Serializable> props)
    {
        if (props != null)
            properties = new HashMap<String,Serializable>(props);
        else
            properties = null;
    }

    /** Get the marker location (search selection flag). */
    public static final long PROP_POINT = 1;

    /** Get the marker orientation (search selection flag). */
    public static final long PROP_ORIENTATION = 2;

    /** Get the marker properties (search selection flag). */
    public static final long PROP_PROPERTIES = 4;

    /** Get all marker information (search selection flag). */
    public static final long PROP_ALL = (PROP_POINT | PROP_ORIENTATION |
        PROP_PROPERTIES);

    /** Marker search parameters.  Search parameters include instance
        oid (required) and marker properties.  The search returns
        {@link Marker} objects with the selected information:
        {@link #PROP_POINT}, {@link #PROP_ORIENTATION},
	{@link #PROP_PROPERTIES}, or {@link #PROP_ALL}.
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

    /** Marker object type. */
    //## 21 is arbitrary number
    public static final ObjectType OBJECT_TYPE =
        ObjectType.intern((short)21,"Marker");

    private Point point;
    private Quaternion orientation;
    private Map<String,Serializable> properties;

    private static final long serialVersionUID = 1L;

}

