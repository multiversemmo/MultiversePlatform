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

package multiverse.server.messages;

import java.io.*;
import java.util.*;
import java.util.concurrent.locks.Lock;

import multiverse.msgsys.*;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.engine.Namespace;

/**
 * general property about an obj/mob this is usually a statistic or state
 * change, such as health, strength. Targeted state, such as whether a quest
 * is available should use TargetedPropertyMessage
 */
public class PropertyMessage extends SubjectMessage
{

    public PropertyMessage() {
	setupTransient();
    }
    
    public PropertyMessage(MessageType msgType) {
        super(msgType);
        setupTransient();
    }

    public PropertyMessage(Long objOid) {
	super(MSG_TYPE_PROPERTY,objOid);
	setupTransient();
    }

    public PropertyMessage(MessageType msgType, Long objOid) {
	super(msgType, objOid);
	setupTransient();
    }
    
    public PropertyMessage(Long objOid, Long notifyOid) {
	super(MSG_TYPE_PROPERTY,objOid);
	setupTransient();
    }

    public String toString() {
	String s = "[PropertyMessage super=" + super.toString();
	for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
	    String key = entry.getKey();
	    Serializable val = entry.getValue();
	    s += " key=" + key + ",value=" + val;
	}
	return s + "]";
    }

    /**
     * namespace is used to break up the property namespace. a single object
     * has multiple property namespaces. this breaks up the object so that
     * different plugins can manage different namespaces. for example, the
     * combat plugin is authoritative for the combatdata namespace. it knows
     * that it can change these values without talking to any other plugins.
     * 
     * @param namespace
     *            the namespace
     */
    public void setNamespace(Namespace namespace) {
	this.namespace = namespace;
    }

    public Namespace getNamespace() {
	return this.namespace;
    }

    private Namespace namespace;

    /**
     * Associate the value with the key.
     * @deprecated Use {@link #setProperty(String key, Serializable val)} instead
     */
    public void put(String key, Serializable val) {
	setProperty(key, val);
    }
    
    /**
     * Associate the value with the key.
     * @param key A String key.
     * @param val A Serializable value.
     */
    public void setProperty(String key, Serializable val) {
        lock.lock();
	try {
	    propertyMap.put(key, val);
	} finally {
	    lock.unlock();
	}
    }

    /**
     * Get the value associated with a key.
     * @deprecated Use {@link #getProperty(String key)} instead
     */
    public Serializable get(String key) {
	return getProperty(key);
    }
    
    /**
     * Return the value associated with a key.
     * @param key A String key.
     * @return The Serializable value associated with the key, or null if none exists.
     */
    public Serializable getProperty(String key) {
        lock.lock();
	try {
	    return propertyMap.get(key);
	} finally {
	    lock.unlock();
	}
    }

    public Set<String> keySet() {
	lock.lock();
	try {
	    return propertyMap.keySet();
	} finally {
	    lock.unlock();
	}
    }

    public MVByteBuffer toBuffer(String version) {
	return toBuffer(version, propertyMap, null);
    }

    public MVByteBuffer toBuffer(String version, Set<String> filteredProps) {
	return toBuffer(version, propertyMap, filteredProps);
    }

    public MVByteBuffer toBuffer(String version, Map<String, Serializable> propMap, Set<String> filteredProps) {
	lock.lock();
	try {
	    MVByteBuffer buf = new MVByteBuffer(500);
	    buf.putLong(getSubject());
	    buf.putInt(62);
	    buf.putFilteredPropertyMap(propMap, filteredProps);
	    buf.flip();
	    return buf;
	} finally {
	    lock.unlock();
	}
    }

    public void fromBuffer(MVByteBuffer buf) {
        Long oid = buf.getLong();
        int msgNumber = buf.getInt();
        if (msgNumber != 62) {
            Log.error("PropertyMessage.fromBuffer: msgNumber " + msgNumber + " is not 62");
            return;
        }
        propertyMap = buf.getPropertyMap();
        setSubject(oid);
    }
    
    void setupTransient() {
	lock = LockFactory.makeLock("PropertyMessageLock");
    }

    public Map<String, Serializable> getPropertyMapRef()
    {
        return propertyMap;
    }

    transient protected Lock lock = null;

    protected Map<String, Serializable> propertyMap = new HashMap<String, Serializable>();

    private static final long serialVersionUID = 1L;

    public static MessageType MSG_TYPE_PROPERTY = MessageType.intern("mv.PROPERTY");

    /**
     * Input parameter propStrings must be an empty list. 
     * This method will fill it with the properties.
     * 
     * @param propStrings
     * @return int
     */
    public static int createPropertyString(List<String> propStrings,
					   Map<String, Serializable> propertyMap, String version) {
	int len = 0;
	for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
	    String key = entry.getKey();
	    Serializable val = entry.getValue();
	    len = addPropertyStringElement(key, val, propStrings, version, len);
	}
	return len;
    }
    
    public static int createFilteredPropertyString(List<String> propStrings,
						   Map<String, Serializable> propertyMap, String version, Set<String> filteredProps) {
	int len = 0;
	for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
	    String key = entry.getKey();
	    if (filteredProps.contains(key))
		continue;
	    Serializable val = entry.getValue();
	    len = addPropertyStringElement(key, val, propStrings, version, len);
	}
	return len;
    }
    
    protected static int addPropertyStringElement(String key, Serializable val, List<String> propStrings,
						  String version, int len) {
	if (val instanceof Boolean) {
	    // special case boolean for the client
	    Boolean b = (Boolean) val;
	    propStrings.add(key);
	    propStrings.add("B");
	    propStrings.add(b ? "true" : "false");
	    len++;
	} else if (val instanceof Integer) {
	    propStrings.add(key);
	    propStrings.add("I");
	    propStrings.add(val.toString());
	    len++;
	} else if (val instanceof Long) {
	    propStrings.add(key);
	    propStrings.add("L");
	    propStrings.add(val.toString());
	    len++;
	} else if (val instanceof String) {
	    propStrings.add(key);
	    propStrings.add("S");
	    propStrings.add((String) val);
	    len++;
	} else if (val instanceof Float) {
	    propStrings.add(key);
	    propStrings.add("F");
	    propStrings.add(val.toString());
	    len++;
	} else if (val instanceof Point) {
	    if (version != null) {
		propStrings.add(key);
		propStrings.add("V");
		Point loc = (Point) val;
		propStrings.add(loc.toString());
		len++;
	    }
	} else if (val instanceof Quaternion) {
	    if (version != null) {
		propStrings.add(key);
		propStrings.add("Q");
		Quaternion q = (Quaternion) val;
		propStrings.add(q.toString());
		len++;
	    }
	} else {
            if (Log.loggingDebug) {
		if (val == null) {
		    Log.debug("propertyMessage: null value for key=" + key);
		}
		else {
		    Log.debug("propertyMessage: unknown type '"+
			      val.getClass().getName()+"', skipping key=" + key);
		}
	    }
	}
	if (Log.loggingDebug)
	    Log.debug("propertyMessage: key=" + key + ", val=" + val);
	return len;
    }

    public static Map<String, Serializable> unmarshallProperyMap(MVByteBuffer buffer) {
	int nProps = buffer.getInt();
	HashMap<String, Serializable> props = new HashMap<String, Serializable>(nProps);
	for (int ii=0 ; ii < nProps; ii++) {
	    String key = buffer.getString();
	    String type = buffer.getString();
	    String value = buffer.getString();
	    if (type.equals("I")) {
		props.put(key, Integer.valueOf(value));
	    }
	    else if (type.equals("B")) {
		props.put(key, Boolean.valueOf(value));
	    }
	    else if (type.equals("L")) {
		props.put(key, Long.valueOf(value));
	    }
	    else if (type.equals("S")) {
		props.put(key, value);
	    }
	    else if (type.equals("F")) {
		props.put(key, Float.valueOf(value));
	    }
	    else {
		if (Log.loggingDebug)
		    Log.debug("unmarshallProperyMap: unknown type '"+type+
			      "', skipping key=" + key);
	    }
	}
	return props;
    }
}
