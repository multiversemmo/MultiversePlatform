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

package multiverse.server.marshalling;

import java.util.*;
import java.io.*;
import org.apache.bcel.classfile.*;
import org.apache.bcel.Repository;
import multiverse.server.network.*;
import multiverse.server.util.*;

/**
 * This class keeps a registry of the set of classes to be marshalled,
 * and provides static methods to perform registration, and to
 * marshal and unmarshal builtin types.
 */
public class MarshallingRuntime {


    /**
     * Registers the class with the given name as a marshalling class,
     * with type number equal to the specified type number.  There are very 
     * cases in which it makes sense to call this method directly; most calls
     * will be to the 1-arg overloading.
     * @param className The fully-elaborated class name, e.g., multiverse.server.math.Point
     * @param typeNum The type number.
     */
    public static void registerMarshallingClass(String className, Short typeNum) {
        if (builtinType(typeNum)) {
            Log.error("For class " + className + ", the explicit type number " + typeNum + " is illegal, because " +
                " it conflicts with the builtin type numbers");
            return;
        }
        Short n = getTypeNumForClassName(className);
        if (n != null) {
            Log.error("The type number for class '" + className + "' has already been defined as " + n);
            return;
        }
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            n = entry.getValue().typeNum;
            if (n.equals(typeNum)) {
                Log.error("For class '" + className + "', the explicit type number " +
                    typeNum + " is already used by class '" + entry.getKey() + "'");
                return;
            }
        }
        ClassProperties props = new ClassProperties(className, typeNum, false);
        classToClassProperties.put(className, props);
    }

    /**
     * Registers the class with the given name as a marshalling class,
     * with type number equal to the next available type number.
     * @param className The fully-elaborated class name, e.g., multiverse.server.math.Point
     */
    public static void registerMarshallingClass(String className) {
        registerMarshallingClass(className, getNextGeneratedValueType());
    }

    protected static short getNextGeneratedValueType()
    {
        while (getClassForTypeNum(nextGeneratedValueType) != null) {
            nextGeneratedValueType++;
        }
        return nextGeneratedValueType;
    }

    /**
     * Records the fact that this marshalling class has marshalling methods.
     * This is called exclusively by the MarshallingClassLoader
     */
    public static void addMarshallingClass(String className, Class c) {
        ClassProperties props = classToClassProperties.get(className);
        if (props == null)
            throwError("MarshallingRuntime.addMarshallingClass: could not look up class '" + className + "'");
        else {
            Short typeNum = props.typeNum;
            if (marshallers.length > typeNum && marshallers[typeNum] != null)
                throwError("MarshallingRuntime.addMarshallingClass: a marshaller for class '" + 
                        className + "' has already been inserted");
            else
                addMarshaller(c, typeNum);
        }
    }

    /**
     * Returns true if the className is a class that marshalling knows about
     * @param className The fully-elaborated class name
     * @return true if this class has been registered for marshalling, or it is a primitive type
     */
    public static boolean hasMarshallingProperties(String className) {
        ClassProperties props = classToClassProperties.get(className);
        return props != null;
    }
    
    /**
     * Returns true if the className is a class that requires that 
     * marshalling be generated.  False for primitive types and classes
     * for which marshalling is done by hand.
     * @param className The fully-elaborated class name
     * @return True if the class required byte-code injection.
     */
    public static boolean injectedClass(String className) {
        ClassProperties props = classToClassProperties.get(className);
        if (props == null)
            return false;
        else 
            return !props.builtin;
    }
    
    /**
     * This method is called by the class loader to inject marshalling
     * methods in a class.  If the class doesn't need marshalling
     * methods, it returns null.
     * @param className The fully-elaborated class name
     * @return The bytes of the updated class definition, or null if it doesn't need marshalling.
     * @throws ClassNotFoundException
     */
    public static byte [] maybeInjectMarshalling(String className) 
              throws ClassNotFoundException {
        Short typeNum = classRequiresInjection(className);
//         Log.info("MarshallingRuntime.maybeInjectMarshalling: '" + className + "', typeNum " + typeNum);
        if (typeNum != null) {
            JavaClass clazz = Repository.lookupClass(className);
            if (clazz != null) {
                if (!InjectionGenerator.handlesMarshallable(clazz)) {
                    clazz = InjectionGenerator.instance.maybeInjectMarshalling(clazz, typeNum);
                    byte [] bytes = clazz.getBytes();
                    return bytes;
                }
            }
            else
                Log.error("MarshallingRuntime.classRequiresInjection: Could not look up class '" + className + "'");
        }
        return null;
    }

    /**
     * This is the top-level method for marshalling an object.  It gets the object's
     * Class object; looks up the type number in table of marshallable types; and writes the 
     * type code for this object to the byte buffer.  If the type number is a primitive
     * or built-in type, it executes switch statement whose index is the type number,
     * and whose cases are the code to marshal the object into the byte buffer.
     * If the type number is not one of a primitive or built-in type, it invokes the 
     * objects marshalObject method, passing the byte buffer.
     * <p> 
     * If it doesn't find the type number in the table of marshallable classes, it 
     * falls back to Java serialization to output the class state.
     * @param buf The byte buffer into which the object should be marshalled
     * @param object The object to be marshalled.
     */
    public static void marshalObject(MVByteBuffer buf, Object object) {
        if (object == null)
            writeTypeNum(buf, typeNumNull);
        else {
            Class c = object.getClass();
            Short typeNum = getTypeNumForClass(c);
            if (typeNum == null) {
                // Back off to Java serialization (sigh)
                writeTypeNum(buf, typeNumJavaSerializable);
                marshalSerializable(buf, object);
            }
            else if (typeNum > lastBuiltinTypeNum) {
                if (!(object instanceof Marshallable)) {
                    Log.dumpStack("MarshallingRuntime:marshalObject: class '" + c.getName() + 
                            "' has typeNum " + typeNum + " but does not support interface Marshallable");
                    writeTypeNum(buf, typeNumNull);
                }
                else {
                    Marshallable marshallingObject = (Marshallable)object;
//                     if (Log.loggingNet)
//                         Log.net("MarshallingRuntime.marshalObject: object " + object + ", obj class " + c + ", typeNum " +
//                             typeNum + ", marshallingObject " + marshallingObject);
                    writeTypeNum(buf, typeNum);
                    marshallingObject.marshalObject(buf);
                }
            }
            else if (typeNum.equals(typeNumBoolean)) {
                Short booleanLiteral = ((Boolean)object) ? typeNumBooleanTrue : typeNumBooleanFalse;
                writeTypeNum(buf, booleanLiteral);
            }
            else {
                // It's a built-in type
                writeTypeNum(buf, typeNum);
                switch (typeNum) {

                case typeNumByte:
                    Byte byteVal = (Byte)object;
                    buf.putByte(byteVal);
                    break;

                case typeNumDouble:
                    Double doubleVal = (Double)object;
                    buf.putDouble(doubleVal);
                    break;

                case typeNumFloat:
                    Float floatVal = (Float)object;
                    buf.putFloat(floatVal);
                    break;

                case typeNumInteger:
                    Integer integerVal = (Integer)object;
                    buf.putInt(integerVal);
                    break;

                case typeNumLong:
                    Long longVal = (Long)object;
                    buf.putLong(longVal);
                    break;

                case typeNumShort:
                    Short shortVal = (Short)object;
                    buf.putShort(shortVal);
                    break;

                case typeNumString:
                    buf.putString((String)object);
                    break;

                case typeNumLinkedList:
                    marshalLinkedList(buf, object);
                    break;
                    
                case typeNumArrayList:
                    marshalArrayList(buf, object);
                    break;

                case typeNumHashMap:
                    marshalHashMap(buf, object);
                    break;

                case typeNumLinkedHashMap:
                    marshalLinkedHashMap(buf, object);
                    break;

                case typeNumTreeMap:
                    marshalTreeMap(buf, object);
                    break;

                case typeNumHashSet:
                    marshalHashSet(buf, object);
                    break;

                case typeNumLinkedHashSet:
                    marshalLinkedHashSet(buf, object);
                    break;

                case typeNumTreeSet:
                    marshalTreeSet(buf, object);
                    break;

                case typeNumByteArray:
                    marshalByteArray(buf, object);
                    break;

                default:
                    throwError("In MarshallingRuntime.marshalObject: unknown typeNum '" + typeNum + "'");
                }
            }
        }
    }
    
    /**
     * This is the top-level method for unmarshalling an object.  It reads the 
     * type code for this object from the byte buffer, and uses that type number
     * as the index for a switch statement.  If the type number represents one 
     * of the primitive or built-in types, it directly calls the code to unmarshal
     * the object.  If the type number is the one for Java serialization, it invokes 
     * Java serialization to unmarshal the object.  Otherwise, it calls the class's 
     * newInstance() method to create an empty instance, and invokes the 
     * unmarshal method on the new instance, passing the byte buffer argument
     * and returns the result.
     * @param buf The byte buffer from which the object should be unmarshalled
     * @return The unmarshalled object
     */
    public static Object unmarshalObject(MVByteBuffer buf) {
        Short typeNum = readTypeNum(buf);
        // If it's a non-builtin type, call the unmarshalling method
        if (typeNum > lastBuiltinTypeNum) {
            Class marshallingClass = marshallers[typeNum];
            if (marshallingClass == null) {
                throwError("MarshallingRuntime.unmarshalObject: no marshalling class for typeNum '" + typeNum + "'");
                return null;
            }
            else {
                try {
                    Object object = marshallingClass.newInstance();
                    Marshallable marshallingObject = (Marshallable)object;
                    return marshallingObject.unmarshalObject(buf);
                }
                catch (Exception e) {
                    throwError("MarshallingRuntime.unmarshalObject, exception running unmarshaller: " + e);
                    return null;
                }
            }
        }
        else {
            switch (typeNum) {

            case typeNumNull:
                return null;
                
            case typeNumBooleanFalse:
                return (Boolean)false;
                
            case typeNumBooleanTrue:
                return (Boolean)true;
                
            case typeNumBoolean:
                return (Boolean)(buf.getByte() != 0);
                
            case typeNumByte:
                return (Byte)buf.getByte();

            case typeNumDouble:
                return (Double)buf.getDouble();

            case typeNumFloat:
                return (Float)buf.getFloat();

            case typeNumInteger:
                return (Integer)buf.getInt();

            case typeNumLong:
                return (Long)buf.getLong();

            case typeNumShort:
                return (Short)buf.getShort();

            case typeNumString:
                return buf.getString();

            case typeNumLinkedList:
                return unmarshalLinkedList(buf);
                    
            case typeNumArrayList:
                return unmarshalArrayList(buf);
            
            case typeNumHashMap:
                return unmarshalHashMap(buf);

            case typeNumLinkedHashMap:
                return unmarshalLinkedHashMap(buf);

            case typeNumTreeMap:
                return unmarshalTreeMap(buf);

            case typeNumHashSet:
                return unmarshalHashSet(buf);
                
            case typeNumLinkedHashSet:
                return unmarshalLinkedHashSet(buf);
                
            case typeNumTreeSet:
                return unmarshalTreeSet(buf);

            case typeNumByteArray:
                return unmarshalByteArray(buf);
            
            case typeNumJavaSerializable:
                return unmarshalSerializable(buf);
                
            default:
                throwError("In MarshallingRuntime.unmarshalObject: unknown typeNum '" + typeNum + "'");
                return null;
            }
        }
    }

    /**
     * Marshal an object that implements
     * Marshallable, but don't preceed the output with a type number.
     * This is used by by-hand implementations of the
     * MarshallingInterface to marshal data members.  The reason that
     * this method exists is that by-hand implementors of marshalling
     * have data member that are marshalled by byte-code injection.  But
     * in the by-hand marshalling code, the compiler doesn't know that
     * the data member will ultimately implement Marshallable.
     * implements Marshallable
     * @param buf The byte buffer
     * @param object The object to be marshalled.
     */
    public static void marshalMarshallingObject(MVByteBuffer buf, Object object) {
        Marshallable marshallingObject = (Marshallable)object;
        marshallingObject.marshalObject(buf);
    }

    /**
     * Unmarshal an object, but don't assume that the object is preceeded by a type number.
     * This is used by by-hand implementations of the
     * MarshallingInterface to unmarshal data members.  The reason that
     * this method exists is that by-hand implementors of marshalling
     * have data member that are marshalled by byte-code injection.  But
     * in the by-hand marshalling code, the compiler doesn't know that
     * the data member will ultimately implement Marshallable.
     * @param buf The byte buffer
     * @param object The newly-created instance
     * @return The newly-created instance, or a substitute instance if the
     * unmarshalObject code does "interning" of objects.
     */
    public static Object unmarshalMarshallingObject(MVByteBuffer buf, Object object) {
        Marshallable marshallingObject = (Marshallable)object;
        Object result = marshallingObject.unmarshalObject(buf);
        return result;
    }

    /**
     * Get the class name for a given type number.
     * @param typeNum The type number.
     * @return The fully-elaborated class name corresponding to the type number.
     */
    public static String getClassForTypeNum(Short typeNum) {
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            Short entryTypeNum = entry.getValue().typeNum;
            if (entryTypeNum.equals(typeNum))
                return entry.getKey();
        }
        return null;
    }

    /**
     * Get a String containing a comma-separated sequence of the class names known about 
     * by the marshalling runtime.  Not particularly useful except for testing.
     * @return The comma-separated sequence of the class names
     */
    public static String registeredClassesAndTypes() {
        String s = "";
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            Short typeNum = entry.getValue().typeNum;
            if (builtinType(typeNum))
                continue;
            if (s == "")
                s += ", ";
            s += "Class '" + entry.getKey() + "': " + typeNum;
        }
        return s;
    }

    protected static void writeTypeNum(MVByteBuffer buf, Short typeNum) {
        if (typeNum <= 255) {
            short b = (short)typeNum;
            buf.putByte((byte)b);
        }
        else {
            int firstByte = ((typeNum >> 8) - 1 + firstExpansionTypeNum);
            int secondByte = (typeNum & 0xff);
//             if (Log.loggingNet)
//                 Log.net("MarshallingRuntime.writeTypeNum: typeNum " + typeNum + ", firstByte " + firstByte + ", secondByte " + secondByte);
            buf.putByte((byte)firstByte);
            buf.putByte((byte)secondByte);
        }
    }
    
    protected static Short readTypeNum(MVByteBuffer buf) {
        int firstByte = byteToIntNoSignExtend(buf.getByte());
        if (firstByte >= firstExpansionTypeNum && firstByte <= lastExpansionTypeNum) {
            int secondByte = byteToIntNoSignExtend(buf.getByte());
            short typeNum = (short)(((firstByte - firstExpansionTypeNum + 1) << 8) | secondByte);
//             if (Log.loggingNet)
//                 Log.net("MarshallingRuntime.readTypeNum: typeNum " + typeNum + ", firstByte " + firstByte + ", secondByte " + secondByte);
            return typeNum;
        }
        else
            return (short)firstByte;
    }

    protected static int byteToIntNoSignExtend(byte b) {
        return (b & 0xff);
    }
    
    /**
     * Is this typeNum itself represent a value, with no further data required?
     */
    protected static boolean valueTypeNum(Short typeNum) {
        return (typeNum.equals(typeNumBooleanFalse) ||
                typeNum.equals(typeNumBooleanTrue) ||
                typeNum.equals(typeNumNull));
    }

    /**
     * Returns true if the type number represents a built-in type
     * @param typeNum The type number
     * @return True if it's a built-in type.
     */
    public static boolean builtinType(Short typeNum) {
        return  typeNum < lastBuiltinTypeNum;
    }
    
    /**
     * Returns true if the class name is a built-in type
     * @param className The name of the class
     * @return True if it's a built-in type.
     */
    public static boolean builtinType(String className) {
        ClassProperties props = classToClassProperties.get(className);
        return (props != null) && props.builtin;
    }
    
    /**
     * Returns the typeNum of the class name if it is a built-in type
     * and one of the aggregate types.
     * @param className The name of the class
     * @return The typeNum of the class if it's an aggregate built-in type.
     */    
    public static Short builtinAggregateTypeNum(String className) {
        ClassProperties props = classToClassProperties.get(className);
        if (props == null || !props.builtin)
            return null;
        short typeNum = props.typeNum;
        if ((typeNum >= firstAggregateTypeNum && typeNum <= lastAggregateTypeNum))
            return typeNum;
        else
            return null;
    }

    protected static boolean marshalledTypeNum(Short typeNum) {
        return typeNum > lastBuiltinTypeNum;
    }
    
    /**
     * Get the type number associated with a Class object, or log an error if it can't be found
     * @param c The Class object
     * @return The appropriate type number, or typeNumNull
     */
    public static Short getTypeNumForClassOrBarf(Class c) {
        Short typeNum = getTypeNumForClass(c);
        if (typeNum == null) {
            Log.dumpStack("Did not find class '" + c.getName() + "' in the type num map");
            return typeNumNull;
        }
        else {
            return typeNum;
        }
    }

    protected static Short getTypeNumForClass(Class c) {
        return getTypeNumForClassName(c.getName());
    }
    
    protected static Short getTypeNumForClassName(String className) {
        ClassProperties props = classToClassProperties.get(className);
        if (props != null)
            return props.typeNum;
        else
            return null;
    }
    
    protected static Class getClassForClassName(String className) {
        try {
//             Log.debug("MarshallingRuntime.getClassForClassName: on class '" + className + "'");
            Class c = Class.forName(className);
            if (c == null)
                Log.error("MarshallingRuntime.getClassForClassName: could not find class '" + className + "'");
            return c;
        }
        catch (Exception e) {
            Log.exception("MarshallingRuntime.getClassForClassName: could not find class", e);
            return null;
        }
    }
    
    protected static String getClassNameForObject(Object object) {
        Class c = object.getClass();
        if (c != null)
            return c.getName();
        else
            return "<Unknown>";
    }

    protected static Short classRequiresInjection(String className) {
        if (classToClassProperties == null)
            return null;
        ClassProperties props = classToClassProperties.get(className);
        if (props != null && !props.injected)
            return props.typeNum;
        else
            return null;
    }  
    
    /**
     * Mark this class with the given name as having had byte-code injection performed
     * @param className The fully-elaborated class name
     */
    public static void markInjected(String className) {
        ClassProperties props = classToClassProperties.get(className);
        if (props == null)
            Log.error("MarshallingRuntime.markInjected: Didn't find class '" + className + "' in map");
        else if (props.injected)
            Log.error("MarshallingRuntime.markInjected: Class '" + className + "' is already injected");
        else
            props.injected = true;
    }  
    
    //
    // The marshallers
    //
    
    protected static void addMarshaller(Class c, Short typeNum) {
        if (marshallers.length <= typeNum) {
            int newSize = typeNum + 256;
            Class [] newMarshallers = new Class[newSize];
            for (int i=0; i<marshallers.length; i++)
                newMarshallers[i] = marshallers[i];
            marshallers = newMarshallers;
        }
        marshallers[typeNum] = c;
    }

    /**
     * The static method that does marshalling via Java serialization.
     * Performs serialization on the object, and adds the serialized 
     * representation to the byte buffer.
     * @param buf The byte buffer
     * @param object The object to be serialized.
     */
    public static void marshalSerializable(MVByteBuffer buf, Object object) {
        //             Log.warnAndDumpStack("MarshallingRuntime.marshalSerializable: object " + object + ", class " + getClassNameForObject(object));
        Log.warn("MarshallingRuntime.marshalSerializable: object " + object + ", class " + getClassNameForObject(object));
        ByteArrayOutputStream ba = null;
        if (!(object instanceof Serializable)) {
            Log.error("marshalSerializable: " + objectDescription(object) + " is not Serializable");
            Log.dumpStack();
        }
        else
            ba = serialHelper(object);
        if (ba == null)
            ba = serialHelper(null);
        byte [] cereal = ba.toByteArray();
        buf.putInt(cereal.length);            
        buf.putBytes(cereal, 0, cereal.length);
    }

    private static ByteArrayOutputStream serialHelper(Object object) {
        try {
            ByteArrayOutputStream ba = new ByteArrayOutputStream();
            ObjectOutputStream os = new ObjectOutputStream(ba);
            if (!(object instanceof Serializable))
                throw new RuntimeException("MarshallingRuntime.serialHelper: " + objectDescription(object) + " is not Serializable");
            os.writeObject(object);
            os.flush();
            ba.flush();
            return ba;
        }
        catch (Exception e) {
            Log.exception("Exception during marshalSerializable of " + objectDescription(object) + "; writing null value", e);
            return null;
        }
    }
    
    private static String objectDescription(Object object) {
        if (object == null)
            return "null";
        else
            return " object " + object + " of class " + getClassNameForObject(object);
    }
    
    /**
     * The static method that unmarshals an object using Java serialization,
     * getting the bytes from the byte buffer.
     * @param buf The byte buffer.
     * @return The unserialized object.
     */
    public static Object unmarshalSerializable(MVByteBuffer buf) {
        int length = 0;
        try {
            length = buf.getInt();
            byte [] cereal = new byte[length];
            buf.getBytes(cereal, 0, length);
            ByteArrayInputStream bs = new ByteArrayInputStream(cereal);
            ObjectInputStream ois = new ObjectInputStream(bs);
            Object object = ois.readObject();
//             Log.warnAndDumpStack("MarshallingRuntime.unmarshalSerializable: object " + object + ", class " + getClassNameForObject(object));
            Log.warn("MarshallingRuntime.unmarshalSerializable: " + objectDescription(object));
            return object;
        }
        catch (Exception e) {
            Log.exception("MarshallingRuntime.unmarshalSerializable", e);
            return null;
        }
    }

    protected static void throwError(String msg) {
        Log.error(msg);
        throw new RuntimeException(msg);
    }

    //
    // Utility methods used to marshal and unmarshal lists, maps and arrays
    //
    
    
    private static void marshalListInternal(MVByteBuffer buf, Object object) {
        List list = (List)object;
        buf.putInt(list.size());
        for (Object elt : list)
            marshalObject(buf, elt);
    }
    
    private static Object unmarshalListInternal(MVByteBuffer buf, List<Object> list, int count) {
        for (int i=0; i<count; i++)
            list.add(unmarshalObject(buf));
        return list;
    }
    
    private static void marshalMapInternal(MVByteBuffer buf, Object object) {
        Map<Object, Object> map = (Map<Object, Object>)object;
        buf.putInt(map.size());
        for (Map.Entry<Object, Object> entry : map.entrySet()) {
            marshalObject(buf, entry.getKey());
            marshalObject(buf, entry.getValue());
        }
    }

    private static Object unmarshalMapInternal(MVByteBuffer buf, Map<Object, Object> map) {
        int count = buf.getInt();
        for (int i=0; i<count; i++)
            map.put(unmarshalObject(buf), unmarshalObject(buf));
        return map;
    }

    private static void marshalSetInternal(MVByteBuffer buf, Object object) {
        Set<Object> set = (Set<Object>)object;
        buf.putInt(set.size());
        for (Object obj : set) {
            marshalObject(buf, obj);
        }
    }

    private static Object unmarshalSetInternal(MVByteBuffer buf, Set<Object> set) {
        int count = buf.getInt();
        for (int i=0; i<count; i++)
            set.add(unmarshalObject(buf));
        return set;
    }

    //
    // Implementations of the built-in aggregators
    //

    /**
     * The built-in marshalling code for a linked list, which
     * marshals it into the byte buffer argument.  The element type
     * can be any type.
     * @param buf The byte buffer.
     * @param object A collection object containing the list elements to be marshalled  
     */
    public static void marshalLinkedList(MVByteBuffer buf, Object object) {
        marshalListInternal(buf, object);
    }
    
    /**
     * The built-in unmarshalling code for a linked list, which
     * unmarshals it from the byte buffer argument.  The element type
     * can be any type.
     * @param buf The byte buffer.
     * @return A LinkedList instance containing ing the unmarshalled list.
     */
    public static Object unmarshalLinkedList(MVByteBuffer buf) {
        int count = buf.getInt();
        LinkedList<Object> list = new LinkedList<Object>();
        return unmarshalListInternal(buf, list, count);
    }
    
    /**
     * The built-in marshalling code for an ArrayList object, which
     * marshals it into the byte buffer argument.  The element type
     * can be any type.
     * @param buf The byte buffer
     * @param object An ArrayList instance containing the object to be marshalled.
     */
    public static void marshalArrayList(MVByteBuffer buf, Object object) {
        marshalListInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for an ArrayList, which
     * unmarshals it from the byte buffer argument.  The element type
     * can be any type.
     * @param buf The byte buffer.
     * @return An ArrayList instance containing ing the unmarshalled array.
     */
    public static Object unmarshalArrayList(MVByteBuffer buf) {
        int count = buf.getInt();
        ArrayList<Object> arrayList = new ArrayList<Object>(count);
        return unmarshalListInternal(buf, arrayList, count);
    }
        
    /**
     * The built-in marshalling code for a HashMap object, which marshals
     * it into the byte buffer argument.  The key and value types can
     * be any type.
     * @param buf The byte buffer
     * @param object A Map instance containing the Map to be marshalled.
     */
    public static void marshalHashMap(MVByteBuffer buf, Object object) {
        marshalMapInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for a HashMap, which unmarshals it
     * from the byte buffer argument.  The key and value types can be
     * any type.
     * @param buf The byte buffer.
     * @return A HashMap instance containing ing the unmarshalled map.
     */
    public static Object unmarshalHashMap(MVByteBuffer buf) {
        HashMap<Object, Object> map = new HashMap<Object, Object>();
        return unmarshalMapInternal(buf, map);
    }

    /**
     * The built-in marshalling code for a LinkedHashMap object, which marshals
     * it into the byte buffer argument.  The key and value types can
     * be any type.
     * @param buf The byte buffer
     * @param object A Map instance containing the Map to be marshalled.
     */
    public static void marshalLinkedHashMap(MVByteBuffer buf, Object object) {
        marshalMapInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for a LinkedHashMap, which unmarshals it
     * from the byte buffer argument.  The key and value types can be
     * any type.
     * @param buf The byte buffer.
     * @return A LinkedHashMap instance containing ing the unmarshalled map.
     */
    public static Object unmarshalLinkedHashMap(MVByteBuffer buf) {
        LinkedHashMap<Object, Object> map = new LinkedHashMap<Object, Object>();
        return unmarshalMapInternal(buf, map);
    }

    /**
     * The built-in marshalling code for a TreeMap object, which marshals
     * it into the byte buffer argument.  The key and value types can
     * be any type.
     * @param buf The byte buffer
     * @param object A Map instance containing the Map to be marshalled.
     */
    public static void marshalTreeMap(MVByteBuffer buf, Object object) {
        marshalMapInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for a TreeMap, which unmarshals it
     * from the byte buffer argument.  The key and value types can be
     * any type.
     * @param buf The byte buffer.
     * @return A TreeMap instance containing ing the unmarshalled map.
     */
    public static Object unmarshalTreeMap(MVByteBuffer buf) {
        TreeMap<Object, Object> map = new TreeMap<Object, Object>();
        return unmarshalMapInternal(buf, map);
    }

    /**
     * The built-in marshalling code for a HashSet object, which marshals
     * it into the byte buffer argument.  The element type can be any
     * type.
     * @param buf The byte buffer
     * @param object A HashSet instance containing the set to be marshaled.
     */
    public static void marshalHashSet(MVByteBuffer buf, Object object) {
        marshalSetInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for a HashSet, which unmarshals it
     * from the byte buffer argument.  The element type can be any
     * type.
     * @param buf The byte buffer.
     * @return A HashSet instance containing ing the unmarshalled set.
     */
    public static Object unmarshalHashSet(MVByteBuffer buf) {
        HashSet<Object> set = new HashSet<Object>();
        return unmarshalSetInternal(buf, set);
    }
        
    /**
     * The built-in marshalling code for a LinkedHashSet object, which marshals
     * it into the byte buffer argument.  The element type can be any
     * type.
     * @param buf The byte buffer
     * @param object A LinkedHashSet instance containing the set to be marshaled.
     */
    public static void marshalLinkedHashSet(MVByteBuffer buf, Object object) {
        marshalSetInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for a LinkedHashSet, which unmarshals it
     * from the byte buffer argument.  The element type can be any
     * type.
     * @param buf The byte buffer.
     * @return A LinkedHashSet instance containing ing the unmarshalled set.
     */
    public static Object unmarshalLinkedHashSet(MVByteBuffer buf) {
        LinkedHashSet<Object> set = new LinkedHashSet<Object>();
        return unmarshalSetInternal(buf, set);
    }
        
    /**
     * The built-in marshalling code for a TreeSet object, which marshals
     * it into the byte buffer argument.  The element type can be any
     * type.
     * @param buf The byte buffer
     * @param object A TreeSet instance containing the set to be marshaled.
     */
    public static void marshalTreeSet(MVByteBuffer buf, Object object) {
        marshalSetInternal(buf, object);
    }

    /**
     * The built-in unmarshalling code for a TreeSet, which unmarshals it
     * from the byte buffer argument.  The element type can be any
     * type.
     * @param buf The byte buffer.
     * @return A TreeSet instance containing ing the unmarshalled set.
     */
    public static Object unmarshalTreeSet(MVByteBuffer buf) {
        TreeSet<Object> set = new TreeSet<Object>();
        return unmarshalSetInternal(buf, set);
    }
        
    /**
     * The built-in marshalling code for a byte array object, which
     * marshals it into the byte buffer argument.
     * @param buf The byte buffer
     * @param object A byte array.
     */
    public static void marshalByteArray(MVByteBuffer buf, Object object) {
        byte[] bytes = (byte[])object;
        buf.putInt(bytes.length);
        for (byte b : bytes) {
            buf.putByte(b);
        }
    }

    /**
     * The built-in unmarshalling code for a byte array, which
     * unmarshals it from the byte buffer argument.
     * @param buf The byte buffer.
     * @return A byte array instance containing ing the unmarshalled bytes.
     */
    public static Object unmarshalByteArray(MVByteBuffer buf) {
        int count = buf.getInt();
        byte[] bytes = new byte[count];
        for (int i=0; i<count; i++)
            bytes[i] = buf.getByte();
        return bytes;
    }
        
    /**
     * A utility class containing a fully-elaborated class name,
     * paired with a type number
     */
    public static class ClassNameAndTypeNumber {
        
        public ClassNameAndTypeNumber(String className, Short typeNum) {
            this.className = className;
            this.typeNum = typeNum;
        }
        
        public String className;
        public Short typeNum;
    }
    
    /**
     * Returns the set of all the class names and type numbers that
     * are subject to marshalling (including those that already
     * support marshalling)
     * @return A HashSet of ClassNameAndTypeNumber instances.
     */
    public static HashSet<ClassNameAndTypeNumber> getClassesToBeMarshalled() { 
        HashSet<ClassNameAndTypeNumber> pairs = new HashSet<ClassNameAndTypeNumber>();
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            Short typeNum = entry.getValue().typeNum;
            if (typeNum <= lastBuiltinTypeNum)
                continue;
            else
                pairs.add(new ClassNameAndTypeNumber(entry.getKey(), typeNum));
        }
        return pairs;
    }

    /**
     * This is the mapping from class to properties of the class,
     * including type number, injection status and so on.
     */
    protected static HashMap<String, ClassProperties> classToClassProperties = new HashMap<String, ClassProperties>();

    /**
     * A data structure to hold the properties of a class registered
     * for marshalling.  This class can be either a built-in type, a
     * by-hand marshalled class, or a class which will get it's
     * marshalling via byte-code injection.
     */
    public static class ClassProperties {
        public ClassProperties(String className, Short typeNum, boolean builtin) {
            this.className = className;
            this.typeNum = typeNum;
            this.builtin = builtin;
            this.injected = false;
        }
        
        String className;
        Short typeNum;
        boolean builtin;
        boolean injected;
    }
    
    /**
     * The map from typeNum to the classes of the object for which we've generated marshalling code
     */
    protected static Class[] marshallers;
    
    protected static boolean predefinedTypesInstalled = false;

    protected static void addPrimitiveToTypeMap(Object object, Short typeNum) {
        String className = object.getClass().getName();
        ClassProperties props = new ClassProperties(className, typeNum, true);
        classToClassProperties.put(className, props);
    }
    
    //
    // Machinery to check to see that the set of registered types is complete, 
    // in the sense that no supertypes or field types refer to unregistered types
    //
    
    protected static boolean checkTypeReferences() {
        // Verify that every type for which we need to generate code
        // is in the maps.  Do it in a goofy, unordered way, since it
        // doesn't actually matter.
        boolean someMissing = false;
        Map<JavaClass, LinkedList<JavaClass>> missingTypes = new HashMap<JavaClass, LinkedList<JavaClass>>();
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            String className = entry.getKey();
            ClassProperties props = entry.getValue();
            if (props.builtin)
                continue;
            JavaClass c = javaClassOrNull(className);
            if (c == null) {
                Log.error("Could not find registered class '" + className + "'");
                someMissing = true;
                continue;
            }
            if (!c.isPublic()) {
                Log.error("Class '" + className + "' is not a public class");
                someMissing = true;
            }
            // There is a bug in bcel with regard to internal static
            // classes - - the access flags don't reflect whether or
            // not the class is static.  So unfortunately, there
            // doesn't seem to be any way to diagnose this condition

//             Log.info("Class '" + className + "' access flags are " + Integer.toHexString(c.getAccessFlags()));
//             if (c.getClassName().indexOf("$") > 0 && !c.isStatic()) {
//                 Log.error("Class '" + className + "' is an internal class, but is not declared static");
//                 someMissing = true;
//             }

            Short n = props.typeNum;
            if (marshalledTypeNum(n)) {
                if (!c.isClass()) {
                    Log.error("Class '" + className + "' is an interface, not an instantiable class");
                    someMissing = true;
                    continue;
                }
                // Any marshalled object must have a public no-args constructor
                if (!hasNoArgConstructor(c)) {
                    Log.error("Class '" + className + "' does not have a public, no-args constructor");
                    someMissing = true;
                    continue;
                }
                JavaClass superclass = InjectionGenerator.getValidSuperclass(c);
                if (superclass != null)
                    checkClassPresent(c, superclass, missingTypes);
                LinkedList<Field> fields = InjectionGenerator.getValidClassFields(c);
                for (Field f : fields) {
                    org.apache.bcel.generic.Type fieldType = f.getType();
                    if (fieldType.getType() == org.apache.bcel.Constants.T_ARRAY) {
                        Log.error("For class '" + className + "', field '" + f.getName() + "' is an array, and arrays are not supported");
                        someMissing = true;
                    }
                    // Log.info("Class '" + className + "' field '" + f.getName() + "' access flags are " + Integer.toHexString(f.getAccessFlags()));
                    String rawName = InjectionGenerator.nonPrimitiveObjectTypeName(fieldType);
                    if (rawName != null) {
                        String name = translateFieldTypeName(rawName);
                        if (name.equals("java.lang.Object"))
                            continue;
                        JavaClass fieldClass = javaClassOrNull(name);
                        if (fieldClass.isEnum()) {
                            Log.error("For class '" + className + "', field '" + f.getName() + "' is an enum, and enums are not supported");
                            someMissing = true;
                        }
                        if (fieldClass == null) {
                            Log.error("For class '" + className + "', could not find field '" + f.getName() + "' class '" + name + "'");
                            someMissing = true;
                            continue;
                        }
                        checkClassPresent(c, fieldClass, missingTypes);
                    }
                }
            }
        }
        if (missingTypes.size() > 0) {
            for (Map.Entry<JavaClass, LinkedList<JavaClass>> entry : missingTypes.entrySet()) {
                JavaClass c = entry.getKey();
                LinkedList<JavaClass> refs = entry.getValue();
                String s = "";
                for (JavaClass ref : refs) {
                    if (s != "")
                        s += ", ";
                    s += "'" + getSimpleClassName(ref) + "'";
                }
                someMissing = true;
                Log.error("Missing type '" + getSimpleClassName(c) + "' is referred to by type(s) " + s);
            }
        }
        if (someMissing)
            Log.error("Aborting code generation due to missing or incorrect types");
        return someMissing;
    }

    /**
     * Any marshalled object must have a public no-args constructor
     */
    protected static boolean hasNoArgConstructor(JavaClass c) {
        Method [] methods = c.getMethods();
        boolean sawConstructor = false;
        for (int i = 0; i < methods.length; i++) {
            Method method = methods[i];
            String methodName = method.getName();
            if (methodName.equals("<init>")) {
                sawConstructor = true;
                if ((method.getArgumentTypes().length == 0) &&
                    (method.isPublic() && !method.isStatic()))
                    return true;
            }
        }
        // If we didn't see any constructor, Java will create a public
        // no-args constructor.  If we saw a constructor, but didn't
        // find a public no-args constructor, we have a problem.
        return !sawConstructor;
    }
    
    protected static void checkClassPresent(JavaClass referringClass, JavaClass referredClass,
                                            Map<JavaClass, LinkedList<JavaClass>> missingTypes) {
        Short s = getTypeNumFromJavaClass(referredClass);
        if (s == null) {
            LinkedList<JavaClass> references = missingTypes.get(referredClass);
            if (references == null) {
                references = new LinkedList<JavaClass>();
                missingTypes.put(referredClass, references);
            }
            if (!references.contains(referringClass))
                references.add(referringClass);
        }
    }
    
    protected static Short getTypeNumFromJavaClass(JavaClass c) {
        String className = c.getClassName();
        Short typeNum = getTypeNumForClassName(className);
        return typeNum;
    }
    
    protected static String getSimpleClassName(JavaClass c) {
        String name = c.getClassName();
        int lastIndex = name.lastIndexOf(".");
        return name.substring(lastIndex + 1);
    }
    
    protected static String translateFieldTypeName(String s) {
        if (InjectionGenerator.interfaceClass(s))
            return "java.lang.Object";
        else if (builtinType(s))
            return s;
        else if (s.equals("java.util.List"))
            return "java.util.LinkedList";
        else if (s.equals("java.util.Map"))
            return "java.util.HashMap";
        else if (s.equals("java.util.Set"))
            return "java.util.HashSet";
        else if (s.equals("java.io.Serializable"))
            return "java.lang.Object";
        else
            return s;
    }

    protected static JavaClass javaClassOrNull(String className) {
        try {
            return Repository.lookupClass(className);
        }
        catch (Exception e) {
            return null;
        }
    }

    /**
     * A utility method, called by MarshallingRuntime.initialize(),
     * which registers all built-in types
     */
    public static void installPredefinedTypes() {

        if (predefinedTypesInstalled)
            return;
        
        marshallers = new Class[256];
        
        // Boolean is a funny one, because it can be optimized out as
        // a flag bit.
        Boolean booleanVal = true;
        Byte byteVal = 3;
        Short shortVal = 3;
        Integer integerVal = 3;
        Long longVal = 3L;
        Float floatVal = 3.0f;
        Double doubleVal = 3.0;
        String stringVal = "3";
        LinkedList listVal = new LinkedList();
        ArrayList arrayListVal = new ArrayList();
        HashMap hashMapVal = new HashMap();
        LinkedHashMap linkedHashMapVal = new LinkedHashMap();
        TreeMap treeMapVal = new TreeMap();
        HashSet hashSetVal = new HashSet();
        LinkedHashSet linkedHashSetVal = new LinkedHashSet();
        TreeSet treeSetVal = new TreeSet();
        byte[] byteArrayVal = new byte[2];
        
        addPrimitiveToTypeMap(booleanVal, typeNumBoolean);
        addPrimitiveToTypeMap(byteVal, typeNumByte);
        addPrimitiveToTypeMap(doubleVal, typeNumDouble);
        addPrimitiveToTypeMap(floatVal, typeNumFloat);
        addPrimitiveToTypeMap(integerVal, typeNumInteger);
        addPrimitiveToTypeMap(longVal, typeNumLong);
        addPrimitiveToTypeMap(shortVal, typeNumShort);
        addPrimitiveToTypeMap(stringVal, typeNumString);
        addPrimitiveToTypeMap(listVal, typeNumLinkedList);
        addPrimitiveToTypeMap(arrayListVal, typeNumArrayList);
        addPrimitiveToTypeMap(hashMapVal, typeNumHashMap);
        addPrimitiveToTypeMap(linkedHashMapVal, typeNumLinkedHashMap);
        addPrimitiveToTypeMap(treeMapVal, typeNumTreeMap);
        addPrimitiveToTypeMap(hashSetVal, typeNumHashSet);
        addPrimitiveToTypeMap(linkedHashSetVal, typeNumLinkedHashSet);
        addPrimitiveToTypeMap(treeSetVal, typeNumTreeSet);
        addPrimitiveToTypeMap(byteArrayVal, typeNumByteArray);
        predefinedTypesInstalled = true;
    }
    
    protected static void processMarshallers(String marshallerFile) {
        try {
            if (Log.loggingDebug)
                Log.debug("Processing marshaller file '" + marshallerFile + "'");
            File f = new File(marshallerFile);
            if (f.exists()) {
                FileReader fReader = new FileReader(f);
                BufferedReader in = new BufferedReader(fReader);
                String originalLine = null;
                while ((originalLine = in.readLine()) != null) {
                    String line = originalLine.trim();
                    int pos = line.indexOf("#");
                    if (pos >= 0)
                        line = line.substring(0, pos).trim();
                    if (line.length() == 0)
                        continue;
                    if (line.indexOf(" ") > 0) {
                        Log.error("In marshallers file '" + marshallerFile + "', illegal line '" + originalLine + "'");
                        continue;
                    }
                    String[] args = line.split(",",2);
                    if (args.length == 2) {
                        Short typeNum;
                        try {
                            typeNum = Short.decode(args[1]);
                        } catch (NumberFormatException e) {
                            Log.error("In marshallers file '" + marshallerFile + "', illegal type number format in line '" + originalLine + "'");
                            continue;
                        }
                        registerMarshallingClass(args[0], typeNum);
                    }
                    else
                        registerMarshallingClass(args[0]);
                }
                in.close();
                Log.debug("Processing of marshallers file '" + marshallerFile + "' completed");
            } else {
                Log.warn("Didn't find marshallers file '" + marshallerFile + "'");
            }
        }
        catch (Exception e) {
            Log.exception("MarshallingRuntime.processMarshallers", e);
            System.exit(1);
        }
    }

    protected static void registerMarshallingClasses(LinkedList<String> scripts) {
        for (String script : scripts)
            processMarshallers(script);
    }
    
    /**
     * A boolean which is true if MarshallingRuntime has been called.
     */
    public static boolean initialized = false;
    
    /**
     * The main MarshallingRuntime entrypoint called by Trampoline to
     * byte-code inject all classes before calling the server
     * process's main class.
     * <p>
     * It's argument is the String array of command-line args; from 
     * those args it looks for the values of the -m args; those values
     * will be the file names of text files, each line of which is
     * the name of a class to be registered for marshalling.
     * <p>
     * initialize() begins by registering all built-in types.
     * It then reads the -m files, registering the classes
     * they contain for marshalling.
     * <p>
     * initialize() then performs an analysis of the classes that 
     * must support marshalling, to see that none are missing, and that
     * they follow the rules for classes that can be marshalled.  The
     * rules are:
     * <li>Any marshallable class must have a public, no-args constructor<br>
     * <li>The only data members whose type is array of primitive type allowed is array of byte<br>
     * <li>Enum data members of marshallable classes are not allowed<br>
     * <li>All data members of a marshallable class must themselves be marshallable<br>
     * <p>
     * If any of these rules are violated, a detailed error message spelling
     * out the problem is logged, and the initialize() returns false, indicating
     * that byte-code injection did not happen.  If the error checks didn't 
     * find any problems, injectAllClass() is called to perform byte-code
     * injection of marshalling code for all registered classes.
     */
    public static boolean initialize(String argv[]) {
        if (initialized)
            Log.dumpStack("MarshallingRuntime.initialize() called twice!");
        Log.info("Entered MarshallingRuntime.initialize()");
        LinkedList<String> scripts = new LinkedList<String>();
        boolean generateClassFiles = false;
        String outputDir = "";
        String typeNumFileName = "";
        boolean listGeneratedCode = false;

        String arg;
        for (int i=0; i<argv.length; i++) {
            String flag = argv[i];
            if (flag.equals("-m")) {
                i++;
                arg = argv[i];
                scripts.add(arg);
            }
            else if (flag.equals("-r"))
                generateClassFiles = true;
            else if (flag.equals("-o")) {
                i++;
                outputDir = argv[i];
            }
            else if (flag.equals("-t")) {
                i++;
                typeNumFileName = argv[i];
            }
            else if (flag.equals("-g"))
                listGeneratedCode = true;
        }
        Log.debug("MarshallingRuntime.initialize: Installing primitive types");
        installPredefinedTypes();
        Log.debug("MarshallingRuntime.initialize: Initializing InjectionGenerator");
        InjectionGenerator.initialize(generateClassFiles, outputDir, listGeneratedCode);
        Log.debug("MarshallingRuntime.initialize: Registering Marshalling Classes");
        registerMarshallingClasses(scripts);
        int countRegistered = classToClassProperties.size() - registeredBuiltTypeCount;
        Log.info("MarshallingRuntime.initialize: Registered " + countRegistered + " marshalling classes");
        boolean broken = checkTypeReferences();
        Log.debug("MarshallingRuntime.initialize: Finished checking type references");
        if (!broken)
            injectAllClasses(outputDir, typeNumFileName);
        // Get rid of the JavaClass instances, because they represent
        // a lot of memory.
        Repository.clearCache();
        initialized = true;
        return broken;
    }
    
    public static void initializeBatch(String typeNumFileName) {
        Log.info("Entered MarshallingRuntime.initializeBatch: reading type nums from '" + typeNumFileName + "'");
        installPredefinedTypes();
        File typeNumFile = new File(typeNumFileName);
        if (!typeNumFile.exists()) {
            Log.error("MarshallingRuntime.initializeBatch: type num file '" + typeNumFileName + "' does not exist!");
            return;
        }
        try {
            FileReader reader = new FileReader(typeNumFile);
            BufferedReader in = new BufferedReader(reader);
            String line;
            int i = 0;
            while ((line = in.readLine()) != null) {
                // Ignore lines that start with #
                if (line.startsWith("#"))
                    continue;
                String[] fields = line.split(",");
                String className = fields[0];
                short typeNum = (short)Integer.parseInt(fields[1]);
                ClassProperties props = new ClassProperties(className, typeNum, false);
                classToClassProperties.put(className, props);
                Class c = Class.forName(className);
                addMarshaller(c, typeNum);
                i++;
            }
            Log.info("Entered MarshallingRuntime.initializeBatch: Registered " + i + " classes");
            in.close();
        }
        catch (Exception e) {
        	Log.exception("MarshallingRuntime.initializeBatch: Exception reading type num file", e);
        }
    }
    
    /**
     * Perform byte code injection of marshalling code for all registered classes
     */
    public static void injectAllClasses(String outputDir, String typeNumFileName) {
        BufferedWriter out = null;
        if (outputDir != "") {
        	try {
            File typeNumFile = new File(typeNumFileName);
            Log.info("Writing type num files to '" + typeNumFile.getName() + "'");
            FileWriter writer = new FileWriter(typeNumFile);
            out = new BufferedWriter(writer);
        	}
        	catch (Exception e) {
        		Log.exception("MarshallingRuntime.injectAllClasses: Exception opening typenum file", e);
        	}
        }
        java.text.DateFormat dateFormat = new java.text.SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
        java.util.Date date = new java.util.Date();
        String dateString = dateFormat.format(date);
        //try {
        	//out.write("# This is a generated file - - do not edit!  Written at " + dateString + "\n");
        //}
        //catch (IOException e) {
        //	Log.exception("MarshallingRuntime.injectAllClasses: Exception writing typenum file", e);
        //}
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            String className = entry.getKey();
            ClassProperties props = entry.getValue();
            Short typeNum = props.typeNum;
            if (props.builtin)
                continue;
            // Skip a line to make the logging prettier
//             Log.info("");
//             Log.info("MarshallingRuntime.injectAllClasses: '" + className + "', typeNum " + typeNum + 
//                 "/0x" + Integer.toHexString(typeNum) + ", injected " + props.injected);
            if (outputDir != "") {
                // Handle the case of injecting marshalling methods to a brand-new class file
                if (!InjectionGenerator.handlesMarshallable(className)) {
                    try {
                        maybeInjectMarshalling(className);
                    }
                    catch (ClassNotFoundException e) {
                        Log.error("MarshallingRuntime.injectAllClasses: Could not load class '" + className + "'");
                    }
                }
                try {
                	out.write(className + "," + typeNum + "\n");
                }
                catch (IOException e) {
                	Log.exception("MarshallingRuntime.injectAllClasses: Exception writing typenum file", e);
                }
            }
            else {
                // The statement below caused the class to be loaded and injected
                Class c = getClassForClassName(className);
                if (InjectionGenerator.handlesMarshallable(className)) {
                    addMarshaller(c, typeNum);
                    Log.debug("Recorded by-hand marshaller '" + className + "', typeNum " + typeNum + "/0x" + Integer.toHexString(typeNum));
                    continue;
                }
                if (c == null)
                    Log.error("MarshallingRuntime.injectAllClasses: Class.forName('" + className + "' did not return the Class object");
            }
        }
        if (outputDir != "") {
        	try {
        		out.close();
        	}
        	catch (IOException e) {
        		Log.exception("MarshallingRuntime.injectAllClasses: Exception closing typenum file", e);
        	}
        }
    }
    
    // These type nums are never stored in marshalled form, so we give
    // them negative values
    protected static final short nonStoredStart = -10;
    protected static final short typeNumPrimitiveBoolean = nonStoredStart + 1;
    protected static final short firstAtomicTypeNum = typeNumPrimitiveBoolean;
    protected static final short firstPrimitiveAtomicTypeNum = typeNumPrimitiveBoolean;
    protected static final short typeNumPrimitiveByte = nonStoredStart + 2;
    protected static final short typeNumPrimitiveDouble = nonStoredStart + 3;
    protected static final short typeNumPrimitiveFloat = nonStoredStart + 4;
    protected static final short typeNumPrimitiveInteger = nonStoredStart + 5;
    protected static final short typeNumPrimitiveLong = nonStoredStart + 6;
    protected static final short typeNumPrimitiveShort = nonStoredStart + 7;
    protected static final short lastPrimitiveAtomicTypeNum = typeNumPrimitiveShort;

    // We put the predefined types at the beginning of the number
    // space, to make it easy to test for them.

    protected static final short builtinStart = 0;
    
    protected static final short typeNumBoolean = builtinStart + 1;
    protected static final short firstNonPrimitiveAtomicTypeNum = typeNumBoolean;
    protected static final short typeNumByte = builtinStart + 2;
    protected static final short typeNumDouble = builtinStart + 3;
    protected static final short typeNumFloat = builtinStart + 4;
    protected static final short typeNumInteger = builtinStart + 5;
    protected static final short typeNumLong = builtinStart + 6;
    protected static final short typeNumShort = builtinStart + 7;
    protected static final short typeNumString = builtinStart + 8;
    protected static final short lastNonPrimitiveAtomicTypeNum = typeNumString;
    protected static final short lastAtomicTypeNum = lastNonPrimitiveAtomicTypeNum;

    // These are the composite types, also handled specially
    protected static final short typeNumLinkedList = builtinStart + 9;
    protected static final short typeNumArrayList = builtinStart + 10;
    protected static final short typeNumHashMap = builtinStart + 11;
    protected static final short typeNumLinkedHashMap = builtinStart + 12;
    protected static final short typeNumTreeMap = builtinStart + 13;
    protected static final short typeNumHashSet = builtinStart + 14;
    protected static final short typeNumLinkedHashSet = builtinStart + 15;
    protected static final short typeNumTreeSet = builtinStart + 16;
    protected static final short typeNumByteArray = builtinStart + 17;
    protected static final short registeredBuiltTypeCount = typeNumByteArray;

    protected static final short firstAggregateTypeNum = typeNumLinkedList;
    protected static final short lastAggregateTypeNum = typeNumByteArray;
    
    protected static final short typeNumJavaSerializable = builtinStart + 18;

    // These are treated specially, since they are shorthand for a
    // value as well as a type.  They are used _solely_ to encode list
    // and property map values, because there they generate better
    // code since we don't have flag bites for each element
    protected static final short typeNumBooleanFalse = builtinStart + 19;
    protected static final short typeNumBooleanTrue = builtinStart + 20;
    protected static final short typeNumNull = builtinStart + 21;

    // These values provide an escape mechanism to allow more than 256
    // byte codes; each one provides another 256 types; the first one
    // give 256 - 511; the second 512 - 767, etc.  For now, we have 4
    // of them.
    protected static final short firstExpansionTypeNum = builtinStart + 22;
    protected static final short lastExpansionTypeNum = firstExpansionTypeNum + 4 - 1;

    protected static final short lastBuiltinTypeNum = lastExpansionTypeNum;

    // This is where we start allocating type nums for marshalling classes
    protected static short firstGeneratedValueType = lastExpansionTypeNum + 1;
    protected static short nextGeneratedValueType = firstGeneratedValueType;

}
