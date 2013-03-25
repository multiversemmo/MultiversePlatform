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

import java.io.*;
import java.util.*;
import java.lang.reflect.*;
import multiverse.server.math.*;
import multiverse.server.util.*;

/**
 * This class provides methods to introspect a collection of classes,
 * and generate serialization for the data and supertypes in the
 * class.  The process proceeds in two passes; the first pass records
 * the java Class objects associated with each class, and if supplied,
 * the type numbers.  The first pass ends by determining that the
 * class DAG is complete, with no classes missing, and assigns type
 * number for any class for which we don't have explicitly assigned
 * number.  The second pass has all the class objects for all of the
 * types, and generates marshalling in the form of static delegate
 * methods on a generated class ServerMarshalling in package
 * multiverse.server.util.  That class contains an encode and decode
 * delegate method for every class, as well as a set of tables that
 * map Class object to type number, and type number to encode/decode
 * delegates.
 *
 * Since this class generates the ServerMarshalling class, there are
 * _two_ compile times required; the first compile time is identical
 * to the current compile time, in that it compiles the non-generated
 * server code.  Then we run the Python scripts that name (and
 * enumerate) the classes that must be marshalled, running the first
 * pass of marshalling generation.  Once all the classes are
 * enumerated, we run the second pass of marshalling generation, which
 * generates the ServerMarshalling class definition.  Finally, the
 * ServerMarshalling class is compiled, and the server is ready to
 * execute.
 *
 * Since this class uses Java introspection to get at the data
 * members, all data members must be declared public.  This is a drag,
 * but there is no way around it if we are to use introspection.
 *
 * The on-the-wire representation of a class object that has no lists
 * or property maps contains exactly one type number, and that number
 * is the first short field in the wire representation.  For lists and
 * property maps, there is one type number per element, since we won't
 * know the types at compile time.
 *
 * One requirement imposed by the marshalling generation mechanism is
 * that you must be able to create an instance to pass to the
 * introspection routines.  This means that there must be _some_
 * constructor of the type that is does not require the actual server
 * to be running.
 */
public class MarshallingGenerator extends MarshallingRuntime {
    
    //
    // Type declarators: These are the interface by which the Python
    // script enumerates the types to be marshalled
    //
    
    /**
     * Declares the name of the class that will contain the generated
     * marshalling classes - - defaults to MVMarshalling
     */
    public static void setGeneratedMarshallingClass(String className) {
        generatedMarshallingClass = className;
    }

    /**
     * Generate the ServerMarshalling class code
     */
    public static void generateMarshalling(String codeFile) {
        
        // Perform generator setup
        installPredefinedTypes();
        initializeCodeGenerator();
        
        // Check on type references
        if (!checkTypeReferences())
            return;

        // Create the output file
        FileWriter str = null;
        try {
            str = new FileWriter(codeFile);
        }
        catch (Exception e) {
            Log.error("Exception opening generated file: " + e.getMessage());
        }
        int indent = 0;
        // Put out the file header
        writeLine(str, indent, "package multiverse.server.marshalling;");
        writeLine(str, 0, "");
        writeLine(str, indent, "import java.io.*;");
        writeLine(str, indent, "import java.util.*;");
        writeLine(str, indent, "import multiverse.server.util.*;");
        writeLine(str, indent, "import multiverse.server.network.*;");
        writeLine(str, indent, "import multiverse.msgsys.*;");
        writeLine(str, indent, "import multiverse.server.plugins.*;");
        writeLine(str, indent, "import multiverse.server.objects.*;");
        writeLine(str, indent, "import multiverse.server.math.*;");
        writeLine(str, indent, "import multiverse.server.plugins.WorldManagerClient.ObjectInfo;");
        // Need a bunch more imports
        writeLine(str, 0, "");
        
        writeLine(str, indent, "public class " + generatedMarshallingClass + " extends MarshallingRuntime {");
        indent++;

        // Generate the sorted map
        ArrayList<MarshallingPair> sortedList = new ArrayList<MarshallingPair>();
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            String className = entry.getKey();
            Short n = entry.getValue().typeNum;
            if (n <= lastBuiltinTypeNum)
                continue;
            if (supportsMarshallable(className))
                continue;
            sortedList.add(new MarshallingPair(className, n));
        }
        // This fails - - I don't know why
        //Collections.sort((List)sortedList);
        
        // Iterate over the non-primitive types, assembling the
        // generation code
        for (MarshallingPair entry : sortedList) {
            String name = entry.getClassKey();
            Class c = lookupClass(name);
            Short n = entry.getTypeNum();
            int flagBitCount = 0;
            LinkedList<Field> fields = getValidClassFields(c);
            // The list of the indexes of fields to be null-tested
            LinkedList<Integer> nullTestedFields = new LinkedList<Integer>();
            int index = -1;
            for (Field f : fields) {
                index++;
                Class fieldType = getFieldType(f);
                // Primitive types don't require flag bits
                if (typeIsPrimitive(fieldType))
                    continue;
                String fieldName = f.getName();
                Short fieldTypeNum = getTypeNumForClass(fieldType);
                if (fieldTypeNum == null) {
                    Log.error("Field " + fieldName + " of type " + c +
                        " has a type for which there is no encode/decode support");
                }
                else {
                    // Only the primitive types don't have null tests, at
                    // least for now
                    if (fieldTypeNum < firstPrimitiveAtomicTypeNum || fieldTypeNum > lastPrimitiveAtomicTypeNum) {
                        nullTestedFields.add(index);
                        flagBitCount++;
                    }
                }
            }
            // We generate marshalling for this type
            // Put out the static class header
            String className = getSimpleClassName(c);
            writeLine(str, indent, "public static class " + className + "Marshaller implements Marshallable {");
            indent++;
            generateToBytesMarshalling(c, n, str, indent, fields, nullTestedFields, flagBitCount);
            generateParseBytesMarshalling(c, n, str, indent);
            generateAssignBytesMarshalling(c, n, str, indent, fields, nullTestedFields, flagBitCount);
            indent--;
            writeLine(str, indent, "}");
            writeLine(str, 0, "");
            try {
                str.flush();
            }
            catch (Exception e) {
                Log.info("Could not flush output file!");
            }
        }
        writeLine(str, indent, "public static void initialize() {");
        indent++;
        for (MarshallingPair entry : sortedList) {
            String className = entry.getClassKey();
            Short n = entry.getTypeNum();
            writeLine(str, indent, "addMarshaller((short)" + n + ", new " + className + "Marshaller());");
        }
        indent--;
        writeLine(str, indent, "}");
        indent--;
        writeLine(str, indent, "}");
        try {
            str.close();
        }
        catch (Exception e) {
            Log.info("Could not close output file!");
        }
    }

    public static class MarshallingPair implements Comparator {
        public MarshallingPair(String className, Short typeNum) {
            this.className = className;
            this.typeNum = typeNum;
        }

        public int compare(Object o1, Object o2) {
            MarshallingPair p1 = (MarshallingPair)o1;
            MarshallingPair p2 = (MarshallingPair)o2;
            if (p1.typeNum < p2.typeNum)
                return -1;
            else if (p1.typeNum > p2.typeNum)
                return 1;
            else
                return 0;
        }

        public String getClassKey() {
            return className;
        }
        
        public Short getTypeNum() {
            return typeNum;
        }

        String className;
        Short typeNum;
    }
    
    protected static boolean checkTypeReferences() {
        // Verify that every type for which we need to generate code
        // is in the maps.  Do it in a goofy, unordered way, since it
        // doesn't actually matter.
        Map<Class, LinkedList<Class>> missingTypes = new HashMap<Class, LinkedList<Class>>();
        for (Map.Entry<String, ClassProperties> entry : classToClassProperties.entrySet()) {
            String className = entry.getKey();
            Class c = lookupClass(className);
            Short n = entry.getValue().typeNum;
            if (marshalledTypeNum(n)) {
                Class superclass = getValidSuperclass(c);
                if (superclass != null)
                    checkClassPresent(c, superclass, missingTypes);
                LinkedList<Field> fields = getValidClassFields(c);
                for (Field f : fields) {
                    Class fieldType = getFieldType(f);
                    checkClassPresent(c, fieldType, missingTypes);
                }
            }
        }
        if (missingTypes.size() > 0) {
            for (Map.Entry<Class, LinkedList<Class>> entry : missingTypes.entrySet()) {
                Class c = entry.getKey();
                LinkedList<Class> refs = entry.getValue();
                String s = "";
                for (Class ref : refs) {
                    if (s != "")
                        s += ", ";
                    s += "'" + getSimpleClassName(ref) + "'";
                }
                Log.error("Missing type '" + getSimpleClassName(c) + "' is referred to by type(s) " + s);
            }
            Log.error("Aborting code generation due to missing types");
            return false;
        }
        else
            return true;
    }

    protected static boolean supportsMarshallable(String className) {
        Class c = lookupClass(className);
        for (Class iface : c.getInterfaces()) {
            if (iface.getSimpleName().equals("Marshallable"))
                return true;
        }
        return false;
    }

    protected static void generateToBytesMarshalling(Class c, int n, FileWriter str, int indent, LinkedList<Field> fields, 
                                                     LinkedList<Integer> nullTestedFields, int flagBitCount) {
        String className = getSimpleClassName(c);
        writeLine(str, indent, "public void " + "toBytes(MVByteBuffer buf, Object object) {");
        indent++;
        writeLine(str, indent, className + " me = (" + className + ")object;");
        // Call the formatter for the supertype, if there is one and it's not an interface
        Class superclass = getValidSuperclass(c);
        if (superclass != null) {
            Short typeNum = getTypeNumForClass(superclass);
            writeLine(str, indent, "MarshallingRuntime.marshallers[" + typeNum + "].toBytes(buf, object);");
        }
        // Iterate over fields requiring null tests, emitting the flag setup code.  
        // Do it in sets of 8, so we get the
        int batches = (flagBitCount + 7) / 8;
        for (int i=0; i<batches; i++) {
            int limit = Math.min(flagBitCount, (i + 1) * 8);
            String s = "buf.writeByte(";
            int start = i * 8;
            for (int j=start; j<limit; j++) {
                int index = j - i * 8;
                Field f = fields.get(nullTestedFields.get(j));
                String test = "(" + makeOmittedTest(f) + " ? " + (1 <<index) + " : 0)";
                s += test;
                if (j < (limit - 1)) {
                    s += " |";
                    if (j == start)
                        writeLine(str, indent, s);
                    else
                        writeLine(str, indent+2, s);
                }
                else {
                    s += ");";                    
                    writeLine(str, (j == start) ? indent : indent+2, s);
                }
                s = "";
            }
        }
        // Now output the encode for all the fields
        int index = -1;
        for (Field f : fields) {
            index++;
            String fieldName = f.getName();
            Class fieldType = getFieldType(f);
            Short fieldTypeNum = getTypeNumForClass(fieldType);
            boolean tested = nullTestedFields.contains(index);
            if (tested) {
                writeLine(str, indent, "if " + makeOmittedTest(f));
                indent++;
            }
            writeLine(str, indent, createWriteOp(fieldType, "me." + fieldName, fieldTypeNum) + ";");
            if (tested)
                indent--;
        }
        indent--;
        writeLine(str, indent, "}");
        writeLine(str, 0, "");
    }
    
    protected static void generateParseBytesMarshalling(Class c, int n, FileWriter str, int indent) {
        String className = getSimpleClassName(c);
        writeLine(str, indent, "public Object " + "parseBytes(MVByteBuffer buf) {");
        indent++;
        writeLine(str, indent, className + " me = new " + className + "();");
        writeLine(str, indent, "assignBytes(buf, me);");
        writeLine(str, indent, "return me;");
        indent--;
        writeLine(str, indent, "}");
        writeLine(str, 0, "");
    }

    protected static void generateAssignBytesMarshalling(Class c, int n, FileWriter str, int indent, LinkedList<Field> fields, 
                                                         LinkedList<Integer> nullTestedFields, int flagBitCount) {
        // We generate marshalling for this type
        // Put out the encode method header
        String className = getSimpleClassName(c);
        writeLine(str, indent, "public void " + "assignBytes(MVByteBuffer buf, Object object) {");
        indent++;
        writeLine(str, indent, className + " me = (" + className + ")object;");
        // Call the formatter for the supertype, if there is one and it's not an interface
        Class superclass = getValidSuperclass(c);
        if (superclass != null) {
            Short typeNum = getTypeNumForClass(superclass);
            writeLine(str, indent, "MarshallingRuntime.marshallers[" + typeNum + "].Marshaller.assignBytes(buf, object);");
        }
        if (flagBitCount > 0) {
            // Iterate over fields requiring null tests, emitting the flag setup code.  
            // Do it in sets of 8, so we get the
            int batches = (flagBitCount + 7) / 8;
            if (batches > 1) {
                for (int i=0; i<batches; i++)
                    writeLine(str, indent, "byte flags" + i + " = buf.getByte();");
            }
            else
                writeLine(str, indent, "byte flags = buf.getByte();");
        }
        // Now output the decode for all the fields
        int index = -1;
        int testIndex = -1;
        for (Field f : fields) {
            index++;
            String fieldName = f.getName();
            Class fieldType = getFieldType(f);
            Short fieldTypeNum = getTypeNumForClass(fieldType);
            boolean tested = nullTestedFields.contains(index);
            if (tested) {
                testIndex++;
                writeLine(str, indent, "if " + formatFlagBitReference(testIndex, flagBitCount));
                indent++;
            }
            String op = createReadOp(fieldType, fieldName, fieldTypeNum);
            writeLine(str, indent, op + ";");
            if (tested)
                indent--;
        }
        indent--;
        writeLine(str, indent, "}");
    }

    protected static String makeOmittedTest(Field f) {
        Class fieldType = getFieldType(f);
        String test = "(me." + f.getName() + " != null)";
        if (isStringType(fieldType))
            test = "(" + test + " && !(me." + f.getName() + ".equals(\"\")))";
        return test;
    }

    protected static String createWriteOp(Class c, String getter, Short fieldTypeNum) {
        if (fieldTypeNum >= firstAtomicTypeNum && fieldTypeNum <= lastAtomicTypeNum) {
            String s = writeOps.get(fieldTypeNum);
            if (s == null) {
                Log.error("Could not find the writeOp for fieldTypeNum " + fieldTypeNum);
                return "<Didn't get writeOp for typeNum" + fieldTypeNum + ">";
            }
            else {
                // Replace the "#" in the writeOp with the getter
                return s.replaceAll("\\#", getter);
            }
        }
        else
            return "MarshallingRuntime.marshallers[" + fieldTypeNum + "].toBytes(buf, " + getter + ")";
    }

    protected static String createReadOp(Class c, String fieldName, short fieldTypeNum) {
        if (fieldTypeNum >= firstAtomicTypeNum && fieldTypeNum <= lastAtomicTypeNum) {
            String s = readOps.get(fieldTypeNum);
            if (s == null) {
                Log.error("Could not find the readOp for fieldTypeNum " + fieldTypeNum);
                return "";
            }
            else
                return "me." + fieldName + " = " + s;
        }
        else
            // It's a composite entity, so call the marshalling code
            return "me." + fieldName + " = MarshallingRuntime.marshallers[" + fieldTypeNum + "].parseBytes(buf)";
    }

    protected static void checkClassPresent(Class referringClass, Class referredClass,
                                            Map<Class, LinkedList<Class>> missingTypes) {
        Short s = getTypeNumForClass(referredClass);
        if (s == null) {
            LinkedList<Class> references = missingTypes.get(referredClass);
            if (references == null) {
                references = new LinkedList<Class>();
                missingTypes.put(referredClass, references);
            }
            if (!references.contains(referringClass))
                references.add(referringClass);
        }
    }

    protected static String formatFlagBitReference(int index, int flagBitCount) {
        if (flagBitCount > 8)
            return "((flags" + (index >> 3) + " & " + (1 << (index & 7)) + ") != 0)";
        else
            return "((flags & " + (1<<index) + ") != 0)";
    }
    
    protected static String formatTitle(String n) {
        String parts[] = n.split("\\.");
        return parts[parts.length - 1];
    }
    
    // We'll never indent more than 16 stops, so no point in an error check
    private static String indentString = "                                                                ";
    
    protected static void writeLine(FileWriter str, int indent, String s) {
        try {
            str.write(indentString.substring(0, indent * 4) + s + "\r\n");
        }
        catch (Exception e) {
            Log.error("Error writing generated file: " + e.getMessage());
        }
    }
    
    protected static boolean isStaticOrTransient(Field f) {
        return (f.getModifiers() & (Modifier.STATIC | Modifier.TRANSIENT)) != 0;
    }

    protected static Class getValidSuperclass(Class c) {
        Class superclass = c.getSuperclass();
        if (superclass != null &&
            (superclass.getModifiers() & Modifier.INTERFACE) == 0 &&
            !getSimpleClassName(superclass).equals("Object"))
            return superclass;
        else
            return null;
    }
    
    protected static LinkedList<Field> getValidClassFields(Class c) {
        LinkedList<Field> validFields = new LinkedList<Field>();
        Field[]  fields = c.getDeclaredFields();
        for (Field f : fields) {
            if (!isStaticOrTransient(f))
                validFields.add(f);
        }
        return validFields;
    }
    
    protected static Class getFieldType(Field f) {
        return canonicalType(f.getType());
    }
            
    protected static Class canonicalType(Class c) {
        String s = c.getSimpleName();
        if (s.equals("List"))
            return linkedListClass;
        else if (s.equals("Map"))
            return hashMapClass;
        else if (s.equals("Set"))
            return hashSetClass;
        else
            return c;
    }
    
    protected static boolean typeIsPrimitive(Class c) {
        return c.isPrimitive();
    }
    
    protected static boolean isStringType(Class c) {
        return c.getSimpleName().equals("String");
    }
    
    protected static String getSimpleClassName(Class c) {
        return c.getSimpleName();
    }
    
    protected static Class lookupClass(String className) {
        try {
            return Class.forName(className);
        }
        catch (Exception e) {
            Log.error("MarshallingGenerator.lookupClass: could not find class '" + className + "'");
            return null;
        }
    }
    
    protected static void initializeCodeGenerator() {

        linkedListClass = lookupClass(getClassForTypeNum(typeNumLinkedList));
        hashMapClass = lookupClass(getClassForTypeNum(typeNumHashMap));
        hashSetClass = lookupClass(getClassForTypeNum(typeNumHashSet));

        // Install the primitive types
        readOps = new HashMap<Short, String>();
        writeOps = new HashMap<Short, String>();

        // When we apply the op, we replace the '#' with the code to get the field
        defineRWCode(typeNumPrimitiveBoolean, typeNumBoolean, "buf.getByte() != 0", "buf.putByte(# ? 1 : 0)");
        defineRWCode(typeNumPrimitiveByte, typeNumByte, "buf.getByte()", "buf.putByte(#)");
        defineRWCode(typeNumPrimitiveShort, typeNumShort, "buf.getShort()", "buf.putShort(#)");
        defineRWCode(typeNumPrimitiveInteger, typeNumInteger, "buf.getInt()", "buf.putInt(#)");
        defineRWCode(typeNumPrimitiveLong, typeNumLong, "buf.getLong()", "buf.putLong(#)");
        defineRWCode(typeNumPrimitiveFloat, typeNumFloat, "buf.getFloat()", "buf.putFloat(#)");
        defineRWCode(typeNumPrimitiveDouble, typeNumDouble, "buf.getDouble()", "buf.putDouble(#)");
        defineRWCode(typeNumString, "buf.getString()", "buf.putString(#)");
    } 

    protected static void defineRWCode(Short typeNumPrimitive, Short typeNumNonPrimitive, String readOp, String writeOp) {
        defineRWCode(typeNumPrimitive, readOp, writeOp);
        defineRWCode(typeNumNonPrimitive, readOp, writeOp);
    }

    protected static void defineRWCode(Short typeNum, String readOp, String writeOp) {
        readOps.put(typeNum, readOp);
        writeOps.put(typeNum, writeOp);
    }
    
    //
    // Data structures
    //

    /**
     * The package name for the generated marhalling - - can be
     * changed by a call to setGeneratedMarshallingClass
     */
    protected static String generatedMarshallingClass = "MVMarshalling";

    // Maps containing primitive type code generation
    protected static HashMap<Short, String> readOps = null;
    protected static HashMap<Short, String> writeOps = null;

    // Remember the classes for the aggregators
    protected static Class linkedListClass = null;
    protected static Class hashMapClass = null;
    protected static Class hashSetClass = null;
    
    /**
     * A test class with all the cases in it
     */
    public static class MarshalTestClass1 {
        public Boolean BooleanVal = true;
        public Byte ByteVal = 3;
        public Short ShortVal = 3;
        public Integer IntegerVal = 3;
        public Long LongVal = 3L;
        public Float FloatVal = 3.0f;
        public Double DoubleVal = 3.0;

        public boolean booleanVal = true;
        public byte byteVal = 3;
        public short shortVal = 3;
        public int integerVal = 3;
        public long longVal = 3L;
        public float floatVal = 3.0f;
        public double doubleVal = 3.0;

        public String stringVal = "3";

        public LinkedList<String> stringList = new LinkedList<String>();
        public HashMap<String, Object> stringMap = new HashMap<String, Object>();
        public HashSet<Point> points = new HashSet<Point>();

        public List gstringList = new LinkedList<String>();
        public Map gstringMap = new HashMap<String, Object>();
        public Set gpoints = new HashSet<Point>();
    }

    public static class MarshalTestClass2 extends MarshalTestClass1 {
        public MarshalTestClass1 myFirstClass1;
        public MarshalTestClass1 mySecondClass1;
    }
    
    protected static void logGenericClassInfo(Object object, String what) {
        Class c = object.getClass();
        Type genSuper = c.getGenericSuperclass();
        Type [] interfaces = c.getGenericInterfaces();
        String s = "";
        for (Type iface : interfaces) {
            if (s != "")
                s += ", ";
            s += iface;
        }
        Log.info("logGenericClassInfo " + what + " Class " + c + ", genSuper " + genSuper + ", interfaces " + s);
    }
    
    public static void main(String args[]) {
        Log.init();

        // Add a couple of typical types.
        registerMarshallingClass("multiverse.server.math.Point");
        registerMarshallingClass("multiverse.server.math.MVVector");
        registerMarshallingClass("multiverse.server.math.Quaternion");
        registerMarshallingClass("multiverse.server.objects.DisplayContext");
        registerMarshallingClass("multiverse.server.objects.SoundData");
        registerMarshallingClass("multiverse.server.plugins.WorldManagerClient.ObjectInfo");
        registerMarshallingClass("multiverse.msgsys.MessageTypeFilter");
        registerMarshallingClass("multiverse.server.objects.LightData");
        registerMarshallingClass("multiverse.server.objects.Color");
        registerMarshallingClass("multiverse.server.marshalling.MarshalTestClass1");
        registerMarshallingClass("multiverse.server.marshalling.MarshalTestClass2");
    }

}
