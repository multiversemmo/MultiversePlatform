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

package multiverse.server.util;

import java.io.*;
import java.util.*;

import multiverse.server.objects.*;
import multiverse.server.math.*;

////////////////////////////////////////////////////////////////////////
//
// A facility to serialize commonly-used types.  The motivation is
// to avoid the CPU and space overhead of Java serialization for
// the most common cases.  All members are static
//
////////////////////////////////////////////////////////////////////////

public class SerialUtils {

    // private static enum PropValueTypes
    private static final byte valueTypeNull = 0;
    private static final byte valueTypeString = 1;
    private static final byte valueTypeLong = 2;
    private static final byte valueTypeInteger = 3;
    private static final byte valueTypeBoolean = 4;
    private static final byte valueTypeBooleanFalse = 4;
    private static final byte valueTypeBooleanTrue = 5;
    private static final byte valueTypeFloat = 6;
    private static final byte valueTypePoint = 7;
    private static final byte valueTypeMVVector = 8;
    private static final byte valueTypeQuaternion = 9;
    private static final byte valueTypeColor = 10;
    private static final byte valueTypeLinkedList = 11;
    private static final byte valueTypeHashSet = 12;
    private static final byte valueTypeHashMap = 13;
    
    private static final byte valueTypeObject = 100;
    
    private static Map<Class, Byte> classToValueTypeMap = null;

    private static void initializeClassToValueTypeMap() {
        Long v1 = 3L;
        Integer v2 = 3;
        Boolean v3 = true;
        Float v4 = 3.0f;
        classToValueTypeMap = new HashMap<Class, Byte>();
        classToValueTypeMap.put((new String()).getClass(), valueTypeString);
        classToValueTypeMap.put(v1.getClass(), valueTypeLong);
        classToValueTypeMap.put(v2.getClass(), valueTypeInteger);
        classToValueTypeMap.put(v3.getClass(), valueTypeBoolean);
        classToValueTypeMap.put(v4.getClass(), valueTypeFloat);
        classToValueTypeMap.put((new Point()).getClass(), valueTypePoint);
        classToValueTypeMap.put((new MVVector()).getClass(), valueTypeMVVector);
        classToValueTypeMap.put((new Quaternion()).getClass(), valueTypeQuaternion);
        classToValueTypeMap.put((new Color()).getClass(), valueTypeColor);
        classToValueTypeMap.put((new LinkedList()).getClass(), valueTypeLinkedList);
        classToValueTypeMap.put((new HashSet()).getClass(), valueTypeHashSet);
        classToValueTypeMap.put((new HashMap()).getClass(), valueTypeHashMap);
    }
    
    public static void writeEncodedObject(ObjectOutputStream out, Object val)
             throws IOException, ClassNotFoundException {
        if (classToValueTypeMap == null)
            initializeClassToValueTypeMap();
        if (val == null)
            out.writeByte(valueTypeNull);
        else {
            Class c = val.getClass();
            Byte index = classToValueTypeMap.get(c);
            if (index == null)
                index = valueTypeObject;
            switch (index) {
            case valueTypeString:
                out.writeByte(valueTypeString);
                out.writeUTF((String)val);
                break;
            case valueTypeLong:
                out.writeByte(valueTypeLong);
                out.writeLong((Long)val);
                break;
            case valueTypeInteger:
                out.writeByte(valueTypeInteger);
                out.writeInt((Integer)val);
                break;
            case valueTypeBoolean:
                out.writeByte(((Boolean)val) ? valueTypeBooleanTrue : valueTypeBooleanFalse);
                break;
            case valueTypeFloat:
                out.writeByte(valueTypeFloat);
                out.writeFloat((Float)val);
                break;
            case valueTypePoint:
                out.writeByte(valueTypePoint);
                ((Point)val).writeExternal(out);
                break;
            case valueTypeMVVector:
                out.writeByte(valueTypeMVVector);
                ((MVVector)val).writeExternal(out);
                break;
            case valueTypeQuaternion:
                out.writeByte(valueTypeQuaternion);
                ((Quaternion)val).writeExternal(out);
                break;
            case valueTypeColor:
                out.writeByte(valueTypeColor);
                ((Color)val).writeExternal(out);
                break;
            case valueTypeObject:
//                 if (Log.loggingDebug)
//                     Log.dumpStack("WorldManagerClient.writeObjectUtility: writing Object " + val.getClass());
                out.writeByte(valueTypeObject);
                out.writeObject(val);
                break;
            case valueTypeLinkedList:
                out.writeByte(valueTypeLinkedList);
                LinkedList list = (LinkedList)val;
                out.writeInt(list.size());
                for (Object obj : list) {
                    // Recurse
                    writeEncodedObject(out, obj);
                }
                break;
            case valueTypeHashSet:
                out.writeByte(valueTypeHashSet);
                HashSet set = (HashSet)val;
                out.writeInt(set.size());
                for (Object obj : set) {
                    // Recurse
                    writeEncodedObject(out, obj);
                }
                break;
            case valueTypeHashMap:
                out.writeByte(valueTypeHashMap);
                HashMap<String, Object> map = (HashMap<String, Object>)val;
                out.writeInt(map.size());
                for (Map.Entry<String, Object> entry : map.entrySet()) {
                    out.writeUTF(entry.getKey());
                    writeEncodedObject(out, entry.getValue());
                }
                break;
            default:
                Log.error("WorldManagerClient.writeEncodedObject: index " + index + " out of bounds; class " + c.getName());
            }
        }
    }
    
    public static Serializable readEncodedObject(ObjectInputStream in)
              throws IOException, ClassNotFoundException {
        int count;
        byte typecode = in.readByte();
        switch (typecode) {
        case valueTypeNull:
            return null;
        case valueTypeString:
            return in.readUTF();
        case valueTypeLong:
            return in.readLong();
        case valueTypeInteger:
            return in.readInt();
        case valueTypeBooleanFalse:
            return false;
        case valueTypeBooleanTrue:
            return true;
        case valueTypeFloat:
            return in.readFloat();
        case valueTypePoint:
            Point p = new Point();
            p.readExternal(in);
            return p;
        case valueTypeMVVector:
            MVVector v = new MVVector();
            v.readExternal(in);
            return v;
        case valueTypeQuaternion:
            Quaternion q = new Quaternion();
            q.readExternal(in);
            return q;
        case valueTypeColor:
            Color color = new Color();
            color.readExternal(in);
            return color;
        case valueTypeObject:
            return (Serializable)in.readObject();
        case valueTypeLinkedList:
            count = in.readInt();
            LinkedList<Object> list = new LinkedList<Object>();
            for (int i=0; i<count; i++)
                list.add(readEncodedObject(in));
            return list;
        case valueTypeHashSet:
            count = in.readInt();
            HashSet<Object> set = new HashSet<Object>();
            for (int i=0; i<count; i++)
                set.add(readEncodedObject(in));
            return set;
        case valueTypeHashMap:
            count = in.readInt();
            HashMap<String, Object> map = new HashMap<String, Object>();
            for (int i=0; i<count; i++) {
                String key = in.readUTF();
                Object value = readEncodedObject(in);
                map.put(key, value);
            }
            return map;
        default:
            Log.error("WorldManagerClient.readObjectUtility: Illegal value type code " + typecode);
            return null;
        }
    }
    
    public static void writePropertyMap(ObjectOutputStream out, Map<String, Object> propertyMap) 
               	throws IOException, ClassNotFoundException {
        out.writeInt(propertyMap == null ? 0 : propertyMap.size());
        if (propertyMap != null) {
            for (Map.Entry<String, Object> entry : propertyMap.entrySet()) {
                String key = entry.getKey();
                Object val = entry.getValue();
                out.writeUTF(key);
                writeEncodedObject(out, val);
            }
        }
    }
            
    public static Map<String, Object> readPropertyMap(ObjectInputStream in)
                throws IOException, ClassNotFoundException {
        Integer count = in.readInt();
        if (count == 0)
            return null;
        Map<String, Object> props = new HashMap<String, Object>();
        for (int i=0; i<count; i++) {
            String key = in.readUTF();
            Serializable val = readEncodedObject(in);
            props.put(key, val);
        }
        return props;
    }
    
    // Until we fix the fact that Message.java defines the propMap as Map<String, Serializable>
    public static void writeSerializablePropertyMap(ObjectOutputStream out, Map<String, Serializable> propertyMap)
        throws IOException, ClassNotFoundException {
        out.writeInt(propertyMap == null ? 0 : propertyMap.size());
        for (Map.Entry<String, Serializable> entry : propertyMap.entrySet()) {
            String key = entry.getKey();
            Object val = entry.getValue();
            out.writeUTF(key);
            writeEncodedObject(out, val);
        }
    }
    
    public static Map<String, Serializable> readSerializablePropertyMap(ObjectInputStream in)
                throws IOException, ClassNotFoundException {
        Integer count = in.readInt();
        if (count == 0)
            return null;
        Map<String, Serializable> props = new HashMap<String, Serializable>();
        for (int i=0; i<count; i++) {
            String key = in.readUTF();
            Serializable val = readEncodedObject(in);
            props.put(key, val);
        }
        return props;
    }
    
}
