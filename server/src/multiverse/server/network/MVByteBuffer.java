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

package multiverse.server.network;

import java.util.*;
import java.io.*;

import multiverse.server.math.*;
import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.msgsys.*;
import java.nio.ByteBuffer;

//
// i would love to extend bytebuffer but they made it have a private
// constructor so you cant
//

// we always make an array so that we know we can get to the backing array
public class MVByteBuffer implements Cloneable, Comparable<MVByteBuffer> {
    public MVByteBuffer(int size) {
	byte[] backingArray = new byte[size];
	mBB = java.nio.ByteBuffer.wrap(backingArray);
	if(! mBB.hasArray()) {
	    System.err.println("does not have backing array");
	    System.exit(1);
	}
    }

    /**
     * creates a new byte buffer that is the length of the byte array
     * passed in and places the bytes into the new buffer.
     * flips the buffer for reading
     */
    public MVByteBuffer(byte[] data) {
	this(data.length);
	mBB.put(data,0,data.length);
	flip();
    }

    public MVByteBuffer(ByteBuffer data) {
	mBB = data;
    }

    /**
     * returns a copy of the byte buffer
     */
    public Object clone() {
	byte[] data = mBB.array();
	MVByteBuffer newBuf = new MVByteBuffer(data.length);
	newBuf.putBytes(data, 0, data.length);
	return newBuf;
    }

    /**
     * Returns the MVByteBuffer gotten by skipping the first offset
     * bytes in the original buffer, and copying out by length
     */
    public MVByteBuffer cloneAtOffset(int offset, int length) {
	byte[] data = mBB.array();
	MVByteBuffer newBuf = new MVByteBuffer(length);
	newBuf.putBytes(data, offset + mBB.position(), length);
	newBuf.rewind();
        return newBuf;
    }
        
    /**
     * returns the backing array - not useful to send across network
     * the backing array encoding format may
     * not be the same as what you entered and therefore you should
     * use getBytes instead.
     */
    public byte[] array() {
	return mBB.array();
    }


    public int capacity() {
	return mBB.capacity();
    }
    
    public MVByteBuffer clear() {
	mBB.clear();
	return this;
    }

    public MVByteBuffer flip() {
	mBB.flip();
	return this;
    }

    public boolean hasRemaining() {
	return mBB.hasRemaining();
    }

    public int limit() {
	return mBB.limit();
    }

    public MVByteBuffer limit(int newLimit) {
	mBB.limit(newLimit);
	return this;
    }

    public int position() {
	return mBB.position();
    }

    public MVByteBuffer position(int newPos) {
	mBB.position(newPos);
	return this;
    }

    public int remaining() {
	return mBB.remaining();
    }
    
    public MVByteBuffer rewind() {
	mBB.rewind();
	return this;
    }

    public byte getByte() {
	return mBB.get();
    }

    // copies the bytes into the passed in byte array
    public MVByteBuffer getBytes(byte[] dst, int offset, int length) {
	mBB.get(dst, offset, length);
	return this;
    }
    
    /**
     * basically helper function to getBytes that rewinds first
     * and returns a copy of the bytes.
     * uses limit (set by flip() ) to figure out how much to copy and
     * leaves position at the end.  
     */
    public byte[] copyBytes() {
	rewind();
	byte[] copyBuf = new byte[mBB.limit()];
	getBytes(copyBuf, 0, mBB.limit());
	return copyBuf;
    }

    /**
     * this version of copyBytes gets the same bytes as the version
     * above, with the same goofy business of ignoring the starting
     * position, but this version also doesn't change position.
     */
    public byte[] copyBytesFromZeroToLimit() {
        int len = mBB.limit();
        byte[] buf = new byte[len];
        byte[] arr = mBB.array();
        for (int i=0; i<len; i++)
            buf[i] = arr[i];
        return buf;
    }
    
    public boolean getBoolean() {
	// the client treats booleans as 32-bits
	return (getInt() == 1);
    }
    public double getDouble() {
	return mBB.getDouble();
    }

    public float getFloat() {
	return mBB.getFloat();
    }

    public short getShort() {
	return mBB.getShort();
    }

    public int getInt() {
	return mBB.getInt();
    }

    public long getLong() {
	return mBB.getLong();
    }

    public String getString() {
	int len = mBB.getInt();
	if (len > 64000) {
	    throw new RuntimeException("MVByteBuffer.getString: over 64k string len=" + len);
	}
	byte[] buf = new byte[len];
	getBytes(buf, 0, len);
	try {
		return new String(buf, "UTF8");
	} catch (UnsupportedEncodingException e) {
		// this should never happen
		return new String(buf);
	}
    }

    public Point getPoint() {
	return new Point(getInt(), getInt(), getInt());
    }

    public Quaternion getQuaternion() {
	return new Quaternion(getFloat(), getFloat(), getFloat(), getFloat());
    }

    public MVVector getMVVector() {
	return new MVVector(getFloat(), getFloat(), getFloat());
    }

    public Color getColor() {
	int alpha = (getByte() & 0xff);
	int blue = (getByte() & 0xff);
	int green = (getByte() & 0xff);
	int red = (getByte() & 0xff);
	return new Color(red,green,blue,alpha);
    }

    public MVByteBuffer getByteBuffer() {
	int length = getInt();
	byte[] data = new byte[length];
	this.getBytes(data, 0, length);

	MVByteBuffer newBuf = new MVByteBuffer(length);
	newBuf.putBytes(data, 0, length);
	newBuf.flip();
	return newBuf;
    }

    public byte[] getByteArray() {
        int length = getInt();
        byte[] data = new byte[length];
        this.getBytes(data, 0, length);
        return data;
    }

    public MVByteBuffer putByte(byte b) {
        if (remaining() <= 0) {
            reallocate();
        }
	mBB.put(b);
	return this;
    }

    public MVByteBuffer putBytes(byte[] src, int offset, int length) {
        if (remaining() < length) {
            reallocate(position() + length);
        }
	mBB.put(src, offset, length);
	return this;
    }

    public MVByteBuffer putBoolean(boolean b) {
        if (remaining() < 4) {
            reallocate();
        }
	// the client treats booleans as 32-bits
	mBB.putInt(b ? 1 : 0);
	return this;
    }

    public MVByteBuffer putDouble(double d) {
        if (remaining() < 8) {
            reallocate();
        }
	mBB.putDouble(d);
	return this;
    }

    public MVByteBuffer putFloat(float f) {
        if (remaining() < 4) {
            reallocate();
        }
	mBB.putFloat(f);
	return this;
    }

    public MVByteBuffer putShort(short s) {
        if (remaining() < 2) {
            reallocate();
        }
	mBB.putShort(s);
	return this;
    }
    public MVByteBuffer putInt(int i) {
        if (remaining() < 4) {
            reallocate();
        }
	mBB.putInt(i);
	return this;
    }
    public MVByteBuffer putLong(long l) {
        if (remaining() < 8) {
            reallocate();
        }
	mBB.putLong(l);
	return this;
    }

    // if s is null, it places the empty string
    public MVByteBuffer putString(String s) {
	if (s == null) {
	    putInt(0);
	    return this;
	}
	byte[] data;
	try {
	    data = s.getBytes("UTF8");
	} catch (UnsupportedEncodingException e) {
		// this is probably going to lose information
		// but it should never happen in any case
	    data = s.getBytes();
	}
	int len = data.length;

        if (remaining() < (len+4)) { // add the int for length too
            reallocate(position() + len + 4);
        }

	mBB.putInt(len);
	mBB.put(data, 0, len);
	return this;
    }

    public MVByteBuffer putMsgTypeString(MessageType msgType) {
        return putString(msgType.getMsgTypeString());
    }
    
    public MVByteBuffer putPoint(Point p) {
        if (remaining() < 24) {
            reallocate();
        }
	mBB.putInt(p.getX());
	mBB.putInt(p.getY());
	mBB.putInt(p.getZ());
	return this;
    }

    public MVByteBuffer putQuaternion(Quaternion q) {
        if (remaining() < 32) {
            reallocate();
        }
	mBB.putFloat(q.getX());
	mBB.putFloat(q.getY());
	mBB.putFloat(q.getZ());
	mBB.putFloat(q.getW());
	return this;
    }

    public MVByteBuffer putMVVector(MVVector v) {
        if (remaining() < 24) {
            reallocate();
        }
	mBB.putFloat(v.getX());
	mBB.putFloat(v.getY());
	mBB.putFloat(v.getZ());
	return this;
    }

    public MVByteBuffer putColor(Color c) {
        if (remaining() < 4) {
            reallocate();
        }
	mBB.put((byte)c.getAlpha());
	mBB.put((byte)c.getBlue());
	mBB.put((byte)c.getGreen());
	mBB.put((byte)c.getRed());
	return this;
    }

    public MVByteBuffer putByteBuffer(MVByteBuffer other) {
	byte[] data = other.array();
        int dataLen = other.limit();
//System.out.println("len="+data.length+" dataLen="+dataLen);
//System.out.println("remaining="+remaining()+" limit="+limit());
        if (remaining() < (dataLen+4)) {
            reallocate(position() + dataLen + 4);
        }
//System.out.println("remaining="+remaining()+" limit="+limit());
	mBB.putInt(dataLen);
//	this.putBytes(data, 0, dataLen);
	mBB.put(data, 0, dataLen);
//System.out.println("position="+position()+" remaining="+remaining()+" limit="+limit());
	return this;
    }

    public MVByteBuffer putByteArray(byte[] byteArray) {
        mBB.putInt(byteArray.length);
        mBB.put(byteArray, 0, byteArray.length);
        return this;
    }

    public ByteBuffer getNioBuf() {
	return mBB;
    }

    // make a new buffer that is double in its current limit and 
    // copies the existing content over
    private void reallocate() {
        reallocate(capacity() * 2);
    }

    // makes a new buffer that is the new passed in size
    // then copies the current contents over
    private void reallocate(int minSize) {
        int newSize = capacity();
        while (newSize < minSize) {
            newSize *= 2;
        }
        if (Log.loggingDebug)
            Log.debug("MVByteBuffer.reallocate: size=" + capacity() +
                " requested="+minSize+" newSize=" + newSize);

        // save the position
        int pos = this.position();

        // copy the data
	byte[] data = mBB.array();
        int dataLen = mBB.position();

        // make a new low lvl byte buffer
	byte[] backingArray = new byte[newSize];
	mBB = java.nio.ByteBuffer.wrap(backingArray);

        // put the data back in
	mBB.put(data, 0, dataLen);

        // set our position
        mBB.position(pos);
    }

    ////////////////////////////////////////////////////////////////////////
    //                                                                    //
    // Plumbing to send to the client and receive from the client         //
    // arbitrarily nested collections of the following types              //
    //                                                                    //
    ////////////////////////////////////////////////////////////////////////

    // private static enum PropValueTypes
    private static final byte valueTypeNull = 0;
    private static final byte valueTypeString = 1;
    private static final byte valueTypeLong = 2;
    private static final byte valueTypeInteger = 3;
    private static final byte valueTypeBoolean = 4;
    private static final byte valueTypeBooleanFalse = 4;
    private static final byte valueTypeBooleanTrue = 5;
    private static final byte valueTypeFloat = 6;
    private static final byte valueTypeDouble = 7;
    private static final byte valueTypePoint = 8;
    private static final byte valueTypeMVVector = 9;
    private static final byte valueTypeQuaternion = 10;
    private static final byte valueTypeColor = 11;
    private static final byte valueTypeByte = 12;
    private static final byte valueTypeShort = 13;

    private static final byte valueTypeLinkedList = 20;
    private static final byte valueTypeHashSet = 21;
    private static final byte valueTypeHashMap = 22;
    private static final byte valueTypeByteArray = 23;
    private static final byte valueTypeTreeMap = 24;
    
    private static Map<Class, Byte> classToValueTypeMap = null;

    private static void initializeClassToValueTypeMap() {
        classToValueTypeMap = new HashMap<Class, Byte>();
        classToValueTypeMap.put(String.class, valueTypeString);
        classToValueTypeMap.put(Long.class, valueTypeLong);
        classToValueTypeMap.put(Integer.class, valueTypeInteger);
        classToValueTypeMap.put(Boolean.class, valueTypeBoolean);
        classToValueTypeMap.put(Float.class, valueTypeFloat);
        classToValueTypeMap.put(Double.class, valueTypeDouble);
        classToValueTypeMap.put(Byte.class, valueTypeByte);
        classToValueTypeMap.put(Short.class, valueTypeShort);
        classToValueTypeMap.put(Point.class, valueTypePoint);
        classToValueTypeMap.put(MVVector.class, valueTypeMVVector);
        classToValueTypeMap.put(Quaternion.class, valueTypeQuaternion);
        classToValueTypeMap.put(Color.class, valueTypeColor);
        classToValueTypeMap.put(LinkedList.class, valueTypeLinkedList);
        classToValueTypeMap.put(ArrayList.class, valueTypeLinkedList);
        classToValueTypeMap.put(HashSet.class, valueTypeHashSet);
        classToValueTypeMap.put(LinkedHashSet.class, valueTypeHashSet);
        classToValueTypeMap.put(TreeSet.class, valueTypeHashSet);
        classToValueTypeMap.put(HashMap.class, valueTypeHashMap);
        classToValueTypeMap.put(LinkedHashMap.class, valueTypeHashMap);
        classToValueTypeMap.put(byte[].class, valueTypeByteArray);
        classToValueTypeMap.put(TreeMap.class, valueTypeTreeMap);
    }
    
    public void putEncodedObject(Serializable val) {
        if (classToValueTypeMap == null)
            initializeClassToValueTypeMap();
        if (val == null)
            putByte(valueTypeNull);
        else {
            Class c = val.getClass();
            Byte index = classToValueTypeMap.get(c);
            if (index == null)
                throw new MVRuntimeException("MVByteBuffer.putEncodedObject: no support for object of class " + val.getClass());
            switch (index) {
            case valueTypeString:
                putByte(valueTypeString);
                putString((String)val);
                break;
            case valueTypeLong:
                putByte(valueTypeLong);
                putLong((Long)val);
                break;
            case valueTypeByte:
                putByte(valueTypeByte);
                putByte((Byte)val);
                break;
            case valueTypeShort:
                putByte(valueTypeShort);
                putShort((Short)val);
                break;
            case valueTypeInteger:
                putByte(valueTypeInteger);
                putInt((Integer)val);
                break;
            case valueTypeBoolean:
                putByte(((Boolean)val) ? valueTypeBooleanTrue : valueTypeBooleanFalse);
                break;
            case valueTypeFloat:
                putByte(valueTypeFloat);
                putFloat((Float)val);
                break;
            case valueTypeDouble:
                putByte(valueTypeDouble);
                putDouble((Double)val);
                break;
            case valueTypePoint:
                putByte(valueTypePoint);
                putPoint((Point)val);
                break;
            case valueTypeMVVector:
                putByte(valueTypeMVVector);
                putMVVector((MVVector)val);
                break;
            case valueTypeQuaternion:
                putByte(valueTypeQuaternion);
                putQuaternion((Quaternion)val);
                break;
            case valueTypeColor:
                putByte(valueTypeColor);
                putColor((Color)val);
                break;
            case valueTypeLinkedList:
                putByte(valueTypeLinkedList);
                LinkedList<Serializable> list = (LinkedList<Serializable>)val;
                putInt(list.size());
                for (Serializable obj : list) {
                    // Recurse
                    putEncodedObject(obj);
                }
                break;
            case valueTypeHashSet:
                putByte(valueTypeHashSet);
                HashSet set = (HashSet)val;
                putInt(set.size());
                for (Object obj : set) {
                    // Recurse
                    putEncodedObject((Serializable)obj);
                }
                break;
            case valueTypeHashMap:
                putByte(valueTypeHashMap);
                putPropertyMap((HashMap<String, Serializable>)val);
                break;
            case valueTypeByteArray:
                putByte(valueTypeByteArray);
                putByteArray((byte[])val);
                break;
            case valueTypeTreeMap:
                putByte(valueTypeTreeMap);
                putPropertyMap((TreeMap<String, Serializable>)val);
                break;
            default:
                Log.error("WorldManagerClient.putEncodedObject: index " + index + " out of bounds; class " + c.getName());
            }
        }
    }
    
    public Serializable getEncodedObject() {
        int count;
        byte typecode = getByte();
        switch (typecode) {
        case valueTypeNull:
            return null;
        case valueTypeString:
            return getString();
        case valueTypeByte:
            return getByte();
        case valueTypeShort:
            return getShort();
        case valueTypeLong:
            return getLong();
        case valueTypeInteger:
            return getInt();
        case valueTypeBooleanFalse:
            return false;
        case valueTypeBooleanTrue:
            return true;
        case valueTypeFloat:
            return getFloat();
        case valueTypeDouble:
            return getDouble();
        case valueTypePoint:
            return getPoint();
        case valueTypeMVVector:
            return getMVVector();
        case valueTypeQuaternion:
            return getQuaternion();
        case valueTypeColor:
            return getColor();
        case valueTypeLinkedList:
            count = getInt();
            LinkedList<Serializable> list = new LinkedList<Serializable>();
            for (int i=0; i<count; i++)
                list.add(getEncodedObject());
            return list;
        case valueTypeHashSet:
            count = getInt();
            HashSet<Serializable> set = new HashSet<Serializable>();
            for (int i=0; i<count; i++)
                set.add(getEncodedObject());
            return set;
        case valueTypeHashMap:
            return (Serializable)getPropertyMap();
        case valueTypeByteArray:
            return getByteArray();
        case valueTypeTreeMap:
            return (Serializable)getTreeMap();
        default:
            Log.error("WorldManagerClient.getObjectUtility: Illegal value type code " + typecode);
            return null;
        }
    }
    
    public void putPropertyMap(Map<String, Serializable> propertyMap) {
        if (Log.loggingDebug)
            logPropertyMap("putPropertyMap", propertyMap, null);
        putInt(propertyMap == null ? 0 : propertyMap.size());
        if (propertyMap != null) {
            for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
                String key = entry.getKey();
                Serializable val = entry.getValue();
                putString(key);
                putEncodedObject(val);
            }
        }
    }
            
    public void putFilteredPropertyMap(Map<String, Serializable> propertyMap, Set<String> filteredProps) {
        if (filteredProps == null || filteredProps.size() == 0)
            putPropertyMap(propertyMap);
        if (propertyMap == null) {
            putInt(0);
            return;
        }
        int count = 0;
        for (String key : propertyMap.keySet()) {
            if (filteredProps == null || !filteredProps.contains(key))
                count++;
        }
        putInt(count);
        if (Log.loggingDebug)
            logPropertyMap("putFilteredPropertyMap", propertyMap, filteredProps);
        for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
            String key = entry.getKey();
            Serializable val = entry.getValue();
            if (filteredProps == null || !filteredProps.contains(key)) {
                putString(key);
                putEncodedObject(val);
            }
        }
    }
            
    private void logPropertyMap(String prefix, Map<String, Serializable> propertyMap, Set<String> filteredProps) {
        String s = "";
        for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
            String key = entry.getKey();
            Serializable val = entry.getValue();
            if (filteredProps == null || !filteredProps.contains(key)) {
                if (s != "")
                    s += ", ";
                s += key + "=" + val;
            }
        }
        Log.debug(prefix + ": " + s);
    }
    
    public Map<String, Serializable> getPropertyMap() {
        int count = getInt();
        HashMap<String, Serializable> map = new HashMap<String, Serializable>();
        for (int i=0; i<count; i++) {
            String key = getString();
            Serializable value = getEncodedObject();
            map.put(key, value);
        }
        return map;
    }

    public Map<String, Serializable> getTreeMap() {
        int count = getInt();
        TreeMap<String, Serializable> map = new TreeMap<String, Serializable>();
        for (int i=0; i<count; i++) {
            String key = getString();
            Serializable value = getEncodedObject();
            map.put(key, value);
        }
        return map;
    }

    public int compareTo(MVByteBuffer buffer) {
        return mBB.compareTo(buffer.mBB);
    }

    // TODO change this
    private java.nio.ByteBuffer mBB;

    public static void main(String args[]) {
        try {
            MVByteBuffer b = new MVByteBuffer(10);
            b.putString("012345678910101010HELLO");
            b.flip();
            String foo = b.getString();
            System.out.println("printing out: '" + foo + "'");
        }
        catch(Exception e) {
            Log.exception("MVByteBuffer.main caught exception", e);
        }
    }
}
