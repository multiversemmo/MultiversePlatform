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
import java.io.File;
import org.apache.bcel.Constants;
import org.apache.bcel.Repository;
import org.apache.bcel.classfile.*;
import org.apache.bcel.generic.*;
import multiverse.server.util.*;

public class InjectionGenerator {

    /**
     * InjectionGenerator constructor
     * @param generateClassFiles True if we should generate new class files
     * containing injected methods.  This is used by the batch injection code.
     * @param outputDir If not the empty string, this is the directory in
     * which generated class files should be stored.
     * @param listGeneratedCode  True if we should log the generated methods
     * in the log file.  This is only set when debugging InjectionGenerator.
     */
    public InjectionGenerator(boolean generateClassFiles, String outputDir, boolean listGeneratedCode) {
        this.generateClassFiles = generateClassFiles;
        this.outputDir = outputDir;
        this.listGeneratedCode = listGeneratedCode;
    }
    
    /**
     * Get the InjectionGenerator singleton instance
     */
    public static InjectionGenerator getInstance() {
        return instance;
    }
    
    /**
     * This is the public method called by MarshallingRuntime to
     * inject marshalling methods into the clazz argument
     * @param clazz The BCEL JavaClass instance representing the class file
     * @param typeNum The type number for the class
     * @return The modified JavaClazz instance
     */
    public JavaClass maybeInjectMarshalling(JavaClass clazz, Short typeNum) {
        String className = clazz.getClassName();
        int flagBitCount = 0;
        LinkedList<Field> fields = getValidClassFields(clazz);
        LinkedList<Integer> nullTestedFields = new LinkedList<Integer>();
        // Enumerate the fields that require flag bits
        int index = -1;
        for (Field f : fields) {
            index++;
            Type fieldType = f.getType();
            // Primitive types don't require flag bits
            if (!isPrimitiveType(fieldType)) {
                nullTestedFields.add(index);
                flagBitCount++;
            }
        }

        // Find the superclass, if it's the sort on which we call super();
        JavaClass superclass = getValidSuperclass(clazz);

        // Now inject the methods for this class
        try {
            clazz = injectMarshallingMethods(clazz, superclass, fields, nullTestedFields, flagBitCount);
            MarshallingRuntime.markInjected(className);
            Log.debug("Generated marshalling for '" + className + "', typeNum " + typeNum + "/0x" + Integer.toHexString(typeNum));
        }
        catch(Exception e) {
            Log.error("Injection into class '" + className + "' terminated due to exception: " + e.getMessage());
        }
        return clazz;
    }

    /**
     * Inject the marshalling methods for this class
     */
    protected JavaClass injectMarshallingMethods(JavaClass clazz,
                                                 JavaClass superclass,
                                                 LinkedList<Field> fields, 
                                                 LinkedList<Integer> nullTestedFields,
                                                 int flagBitCount) {
        // Create the method entities
        ClassGen classGen = new ClassGen(clazz);
        JavaClass updatedClass = classGen.getJavaClass();
        String className = updatedClass.getClassName();
        ConstantPoolGen cp = classGen.getConstantPool();
        InstructionFactory factory = new InstructionFactory(cp);
        initStack();
        MethodGen mmg = createMarshallingMethod(clazz, superclass, cp, factory,
                                                fields, nullTestedFields, flagBitCount);
        mmg.setMaxStack(getFinalStack());
        initStack();
        MethodGen umg = createUnmarshallingMethod(clazz, superclass, cp, factory,
                                                  fields, nullTestedFields, flagBitCount);
        umg.setMaxStack(getFinalStack());
        
        // Now change the class by adding the methods and adding the Marshallable
        classGen.addInterface(marshallableClassName);
        classGen.addMethod(mmg.getMethod());
        classGen.addMethod(umg.getMethod());
        if (listGeneratedCode) {
            Log.debug("ConstantPool:\n" + cp.toString() + "\n");
            logGeneratedMethod(classGen, mmg);
            logGeneratedMethod(classGen, umg);
        }

        // Clean up
        mmg.getInstructionList().dispose();
        umg.getInstructionList().dispose();

        if (generateClassFiles) {
            String pathname;
            if (outputDir != "")
                pathname = outputDir + className.replace(".", File.separator) + ".class";
            else
                // TODO: Note, this removes the previous class file for
                // the class, and so you can't run this twice without
                // regenerating the original class file in between.  Just
                // for debugging, so maybe we don't have to do anything
                // about it.
                pathname = "." + File.separator + "build" + File.separator + className.replace(".", File.separator) + ".class";
            try {
                Log.debug("Replacing class file '" + className + "'");
                classGen.getJavaClass().dump(pathname);
            }
            catch (Exception e) {
                Log.error("Exception raised when writing class '" + updatedClass.getClassName() + "': " + e.getMessage());
            }
        }
        return classGen.getJavaClass();
    }
    
    protected void logGeneratedMethod(ClassGen classGen, MethodGen method) {
        //Log.info("Generated Method " + classGen.getClassName() + "." + method.getName());
        Log.debug("Method details:\n" + method.toString() + "\n" +
                  "Method instructions:\n" + method.getInstructionList().toString());
    }
    
    //
    // Methods to do marshalObject
    //

    /**
     * Create the marshalObject method
     */
    protected MethodGen createMarshallingMethod(JavaClass clazz,
                                                JavaClass superclass,
                                                ConstantPoolGen cp,
                                                InstructionFactory factory,
                                                LinkedList<Field> fields, 
                                                LinkedList<Integer> nullTestedFields,
                                                int flagBitCount) {
        String className = clazz.getClassName();
        InstructionList il = new InstructionList();
        MethodGen mg = new MethodGen(Constants.ACC_PUBLIC, 
                                     Type.VOID,
                                     new Type[] { mvByteBufferType },
                                     new String[] { "buf" },
                                     "marshalObject", className, il, cp);
        // If there is a superclass, output "super.toBytes(this, buf)"
        if (superclass != null) {
            il.append(new ALOAD(0));  // this
            il.append(new ALOAD(1));  // buf 
            addStack(2);
            il.append(factory.createInvoke(superclass.getClassName(), "marshalObject", Type.VOID, 
                    new Type [] { mvByteBufferType },
                    Constants.INVOKESPECIAL));
            addStack(-2);
        }
        LocalVariableGen flagVar = null;
        int flagVarIndex = 0;
        if (flagBitCount > 0) {
            flagVar = mg.addLocalVariable("flag_bits", Type.BYTE, null, null);
            flagVarIndex = flagVar.getIndex();
        }
        int batches = (flagBitCount + 7) / 8;
        // Output the flag byte(s), if any
        for (int i=0; i<batches; i++) {
            // Set the flags variable to 0
            il.append(factory.createConstant(0));
            addStack(1);
            il.append(InstructionFactory.createStore(Type.BYTE, flagVarIndex));
            addStack(-1);
            int limit = Math.min(flagBitCount, (i + 1) * 8);
            int start = i * 8;
            for (int j=start; j<limit; j++) {
                // Compute the flag byte | operation for the jth null-tested field
                // The first bit assignment to flags doesn't need the
                // | operation
                boolean firstInBatch = (j & 0x7) == 0;
                Field f = fields.get(nullTestedFields.get(j));
                BranchFixup [] branchFixups = makeOmittedTest(clazz, f, factory, il);
                if (!firstInBatch) {
                    il.append(InstructionFactory.createLoad(Type.BYTE, flagVarIndex));
                    addStack(1);
                }
                il.append(factory.createConstant(1 << (j - start)));
                addStack(1);
                if (!firstInBatch) {
                    il.append(InstructionFactory.createBinaryOperation("|", Type.BYTE));
                    il.append(factory.createCast(Type.INT, Type.BYTE));
                    addStack(-1);
                }
                il.append(InstructionFactory.createStore(Type.BYTE, flagVarIndex));
                addStack(-1);
                noteBranchTargets(branchFixups, il);
            }
            // call buf.putByte on the flag byte
            il.append(new ALOAD(1));  // buf 
            il.append(InstructionFactory.createLoad(Type.BYTE, flagVarIndex));
            addStack(2);
            il.append(factory.createInvoke(mvByteBufferClassName, "putByte", mvByteBufferType, 
                    new Type [] { Type.BYTE },
                    Constants.INVOKEVIRTUAL));
            il.append(InstructionFactory.createPop(1));
            addStack(-2);
        }
        // Now output the encode for all the fields
        int index = -1;
        for (Field f : fields) {
            index++;
            boolean tested = nullTestedFields.contains(index);
            BranchFixup [] branchFixups = null;
            if (tested)
                branchFixups = makeOmittedTest(clazz, f, factory, il);
            addMarshallingForField(clazz, f, factory, il);
            if (tested)
                noteBranchTargets(branchFixups, il);
        }
        il.append(InstructionFactory.createReturn(Type.VOID));
        BranchFixup.fixAllFixups(il);
        return mg;
    }
    
    protected void addMarshallingForField(JavaClass clazz, Field f, InstructionFactory factory,
                                          InstructionList il) {
        Type fieldType = f.getType();
        PrimitiveTypeInfo info = getPrimitiveTypeInfo(fieldType);
        if (info != null || isStringType(fieldType))
            // we're calling MVByteBuffer.putxxx(value)
            addMVByteBufferFieldPut(clazz, f, fieldType, info, factory, il);
        else if (fieldType instanceof ObjectType) {
            ObjectType fieldObjectType = (ObjectType)fieldType;
            if (marshalledByMarshallingRuntimeMarshalObject(fieldObjectType)) {
                // This is an instance of an marshallable class.  Call
                // MarshallingRuntime to do the work
                il.append(new ALOAD(1));  // buf 
                addStack(1);
                addFieldFetch(clazz, factory, f, il);
                il.append(factory.createInvoke(marshallingRuntimeClassName, "marshalObject",
                        Type.VOID, new Type [] { mvByteBufferType, Type.OBJECT },
                        Constants.INVOKESTATIC));
                addStack(-(1 + fieldType.getSize()));
            }
            else {
                Short aggregateTypeNum = getAggregateTypeNum(fieldObjectType);
                if (aggregateTypeNum != null) {
                    String s = aggregateTypeString(fieldObjectType);
                    il.append(new ALOAD(1));  // buf 
                    addStack(1);
                    addFieldFetch(clazz, factory, f, il);
                    il.append(factory.createInvoke(marshallingRuntimeClassName, "marshal" + s,
                            Type.VOID, new Type [] { mvByteBufferType, Type.OBJECT },
                            Constants.INVOKESTATIC));
                    addStack(-(1 + fieldType.getSize()));
                }
                else {
                    // Call the library routine to invoke Java serialization
                    il.append(new ALOAD(1));  // buf 
                    addStack(1);
                    addFieldFetch(clazz, factory, f, il);
                    il.append(factory.createInvoke(marshallingRuntimeClassName, "marshalSerializable",
                            Type.VOID, new Type [] { mvByteBufferType, fieldType },
                            Constants.INVOKESTATIC));
                    addStack(-(1 + fieldType.getSize()));
                }
            }
        }
        else if (fieldType instanceof ArrayType) {
            // Call the library routine to output the array
            il.append(new ALOAD(1));  // buf 
            addStack(1);
            addFieldFetch(clazz, factory, f, il);
            il.append(factory.createInvoke(marshallingRuntimeClassName, "marshalArray",
                    Type.VOID, new Type [] { mvByteBufferType, fieldType },
                    Constants.INVOKESTATIC));
            addStack(-2);
        }

        else
            // Houston, we have a problem
            throwError("In addtoBytesForField, unknown type '" + fieldType + "'");
    }
    
    // This is called only for a primitive type, or type String
    protected void addMVByteBufferFieldPut(JavaClass clazz, Field f, Type fieldType,
                                           PrimitiveTypeInfo info,
                                           InstructionFactory factory, 
                                           InstructionList il) {
        il.append(new ALOAD(1));  // buf 
        addStack(1);
        addFieldFetch(clazz, factory, f, il);
        // If it's a string, we just make the call
        if (isStringType(fieldType)) {
            il.append(factory.createInvoke(mvByteBufferClassName, "putString",
                mvByteBufferType, new Type [] { Type.STRING },
                    Constants.INVOKEVIRTUAL));
            il.append(InstructionFactory.createPop(1));
            addStack(-(1 + fieldType.getSize()));
            return;
        }
        // If it's a primitive object type, we need to get it's value
        if (fieldType instanceof ObjectType) {
            String className = info.objectType.getClassName();
            il.append(factory.createInvoke(className, info.valueString + "Value", info.type,
                    new Type [] { },
                    Constants.INVOKEVIRTUAL));
            addStack(-1 + info.type.getSize());
        }
        // If it's a boolean, compute the value
        if (info.type == Type.BOOLEAN) {
            // Create the 1 or zero
            BranchHandle branch1 = il.append(InstructionFactory.createBranchInstruction(Constants.IFEQ, il.getStart()));
            addStack(-1);
            BranchFixup branchFixup1 = new BranchFixup(branch1, il);
            il.append(factory.createConstant(1));
            addStack(1);
            BranchHandle branch2 = il.append(InstructionFactory.createBranchInstruction(Constants.GOTO, il.getStart()));
            BranchFixup branchFixup2 = new BranchFixup(branch2, il);
            branchFixup1.atTarget(il);
            il.append(factory.createConstant(0));
            branchFixup2.atTarget(il);
            il.append(factory.createCast(Type.INT, Type.BYTE));
        }
        // Now put the value
        il.append(factory.createInvoke(mvByteBufferClassName, "put" + info.mvByteBufferSuffix,
                mvByteBufferType, new Type [] { storageType(info.type) },
                Constants.INVOKEVIRTUAL));
        il.append(InstructionFactory.createPop(1));
        addStack(-(1 + info.type.getSize()));
    }
    
    // Test for null, and optionally for string == "".  Returns an array
    // of InstructionHandles of branch instructions that have to have
    // their targets fixed up
    protected BranchFixup [] makeOmittedTest(JavaClass clazz,
                                              Field f, InstructionFactory factory,
                                              InstructionList il) {
        addFieldFetch(clazz, factory, f, il);

        BranchHandle branch1 = il.append(InstructionFactory.createBranchInstruction(Constants.IFNULL, il.getStart()));
        addStack(-1);
        BranchFixup branchFixup1 = new BranchFixup(branch1, il);
        BranchFixup branchFixup2 = null;
        if (isStringType(f.getType())) {
            addFieldFetch(clazz, factory, f, il);
            il.append(factory.createConstant(""));
            addStack(1);
            BranchHandle branch2 = il.append(new IF_ACMPEQ(il.getStart()));
            addStack(-2);
            branchFixup2 = new BranchFixup(branch2, il);
        }
        if (branchFixup2 != null)
            return new BranchFixup [] { branchFixup1, branchFixup2 };
        else
            return new BranchFixup [] { branchFixup1 };
    }

    //
    // Methods to do unmarshalObject
    //

    /**
     * Create the unmarshalObject method
     */
    protected MethodGen createUnmarshallingMethod(JavaClass clazz,
                                                  JavaClass superclass,
                                                  ConstantPoolGen cp,
                                                  InstructionFactory factory,
                                                  LinkedList<Field> fields, 
                                                  LinkedList<Integer> nullTestedFields,
                                                  int flagBitCount) {
        String className = clazz.getClassName();
        InstructionList il = new InstructionList();
        MethodGen mg = new MethodGen(Constants.ACC_PUBLIC, 
                                     Type.OBJECT,
                                     new Type[] { mvByteBufferType },
                                     new String[] { "buf" },
                                     "unmarshalObject", className, il, cp);
        // If there is a superclass, output "super.unmarshalObject(buf)"
        if (superclass != null) {
            il.append(new ALOAD(0));  // this
            il.append(new ALOAD(1));  // buf 
            addStack(2);
            il.append(factory.createInvoke(superclass.getClassName(), "unmarshalObject", Type.OBJECT, 
                    new Type [] { mvByteBufferType },
                    Constants.INVOKESPECIAL));
            // Pop the returned value off of the stack
            il.append(InstructionFactory.createPop(1));
            addStack(-2);
        }
        int batches = (flagBitCount + 7) / 8;
        LocalVariableGen flagVars [] = null;
        Integer flagVarIndices [] = null;
        // Read the flag byte(s), if any
        if (flagBitCount > 0) {
            flagVars = new LocalVariableGen[batches];
            flagVarIndices = new Integer[batches];
            for (int i=0; i<batches; i++) {
                // Create the flag var
                flagVars[i] = mg.addLocalVariable("flag_bits" + i, Type.BYTE, null, null);
                flagVarIndices[i] = flagVars[i].getIndex();
                // Read in the byte
                il.append(new ALOAD(1));  // buf 
                addStack(1);
                il.append(factory.createInvoke(mvByteBufferClassName, "getByte",
                        Type.BYTE, new Type [] { },
                    Constants.INVOKEVIRTUAL));
                il.append(InstructionFactory.createStore(Type.BYTE, flagVarIndices[i]));
                addStack(-1);
            }
        }
        // Now read the fields
        int flagBitIndex = -1;
        int fieldIndex = -1;
        for (Field f : fields) {
            fieldIndex++;
            Type fieldType = f.getType();
            boolean tested = nullTestedFields.contains(fieldIndex);
            BranchHandle branch = null;
            BranchFixup branchFixup = null;
            if (tested) {
                flagBitIndex++;
                int flagVarNumber = (flagBitIndex >> 3);
                int flagBitNumber = (flagBitIndex & 0x7);
                il.append(InstructionFactory.createLoad(Type.BYTE, flagVars[flagVarNumber].getIndex()));;
                il.append(factory.createConstant(1 << flagBitNumber));
                addStack(2);
                il.append(InstructionFactory.createBinaryOperation("&", Type.BYTE));
                addStack(-1);
                branch = il.append(InstructionFactory.createBranchInstruction(Constants.IFEQ, null));
                addStack(-1);
                branchFixup = new BranchFixup(branch, il);
            }
            // How we get the values depends on the type
            addUnmarshallingForField(clazz, f, factory, cp, il);
            // Save the field
            il.append(factory.createPutField(clazz.getClassName(), f.getName(), fieldType));
            addStack(-(1 + fieldType.getSize()));
            // Fix up the branch, if necessary
            if (branch != null)
                branchFixup.atTarget(il);
        }
        // Since we return this, but it on the stack
        il.append(new ALOAD(0));  // this 
        addStack(1);
        il.append(InstructionFactory.createReturn(Type.OBJECT));
        addStack(-1);
        BranchFixup.fixAllFixups(il);
        // Done!
        return mg;
    }

    protected void addUnmarshallingForField(JavaClass clazz, Field f, InstructionFactory factory,
                                            ConstantPoolGen cp, InstructionList il) {
        Type fieldType = f.getType();
        PrimitiveTypeInfo info = getPrimitiveTypeInfo(fieldType);
        // this, needed for the field store, so used by all the cases
        il.append(new ALOAD(0));  
        addStack(1);
        if (info != null || isStringType(fieldType))
            // we're calling MVByteBuffer.getxxx(value)
            addMVByteBufferFieldGet(clazz, f, fieldType, info, factory, il);
        else {
            if (fieldType instanceof ObjectType) {
                ObjectType fieldObjectType = (ObjectType)fieldType;
                il.append(new ALOAD(1));  // buf, the first arg for all compilations
                addStack(1);
                if (marshalledByMarshallingRuntimeMarshalObject(fieldObjectType)) {
                    // Unmarshal the interface type
                    il.append(factory.createInvoke(marshallingRuntimeClassName, "unmarshalObject",
                            Type.OBJECT, new Type [] { mvByteBufferType },
                            Constants.INVOKESTATIC));
                }
                else {
                    Short aggregateTypeNum = getAggregateTypeNum(fieldObjectType);
                    if (aggregateTypeNum != null) {
                        String s = aggregateTypeString(fieldObjectType);
                        il.append(factory.createInvoke(marshallingRuntimeClassName, "unmarshal" + s,
                                Type.OBJECT, new Type [] { mvByteBufferType },
                                Constants.INVOKESTATIC));
                        // Stack in peace, because it pops buf, but returns a value
                    }
                    else {
                        // Call the library routine to invoke Java serialization
                        il.append(factory.createInvoke(marshallingRuntimeClassName, "unmarshalSerializable",
                                Type.OBJECT, new Type [] { mvByteBufferType },
                                Constants.INVOKESTATIC));
                        // Stack in peace, because it pops buf, but returns a value
                    }
                }
                // In all cases, cast the result to the field type
                if (!fieldObjectType.getClassName().equals("java.lang.Object"))
                    il.append(new CHECKCAST(cp.addClass(fieldObjectType.getClassName())));
            }
            else if (fieldType instanceof ArrayType) {
                // Call the library routine to output the array
                il.append(factory.createInvoke(marshallingRuntimeClassName, "unmarshalArray",
                        Type.OBJECT, new Type [] { mvByteBufferType },
                        Constants.INVOKESTATIC));
                // Stack in peace, because it pops buf, but returns a value
            }
            else
                // Houston, we have a problem
                throwError("In addtoBytesForField, unknown type '" + fieldType + "'");
        }
    }
    
    protected void addMVByteBufferFieldGet(JavaClass clazz, Field f, Type fieldType,
                                           PrimitiveTypeInfo info,
                                           InstructionFactory factory, 
                                           InstructionList il) {
        il.append(new ALOAD(1));  // buf 
        addStack(1);
        if (isStringType(fieldType)) {
            il.append(factory.createInvoke(mvByteBufferClassName, "getString",
                    Type.STRING, new Type [] { },
                    Constants.INVOKEVIRTUAL));
            // Stack in peace
            return;
        }
        // Get the value
        il.append(factory.createInvoke(mvByteBufferClassName, "get" + info.mvByteBufferSuffix,
                storageType(info.type), new Type [] { },
                Constants.INVOKEVIRTUAL));
        addStack(-1 + info.type.getSize());
        // If it's a boolean, compute the value
        if (info.type == Type.BOOLEAN) {
            // Create the 1 or zero
            BranchHandle branch1 = il.append(InstructionFactory.createBranchInstruction(Constants.IFEQ, il.getStart()));
            addStack(-1);
            BranchFixup branchFixup1 = new BranchFixup(branch1, il);
            il.append(factory.createConstant(1));
            addStack(1);
            BranchHandle branch2 = il.append(InstructionFactory.createBranchInstruction(Constants.GOTO, il.getStart()));
            BranchFixup branchFixup2 = new BranchFixup(branch2, il);
            branchFixup1.atTarget(il);
            il.append(factory.createConstant(0));
            branchFixup2.atTarget(il);
        }
        if (fieldType instanceof ObjectType) {
            String className = info.objectType.getClassName();
            il.append(factory.createInvoke(className, "valueOf", info.objectType,
                    new Type [] { info.type },
                    Constants.INVOKESTATIC));
            addStack(1 - info.type.getSize());
        }
    }
    
    //
    // Common machinery
    //
    
    protected void addFieldFetch(JavaClass clazz, InstructionFactory factory, Field f, InstructionList il) {
        il.append(new ALOAD(0));  // this
        addStack(1);
        Type fieldType = f.getType();
        il.append(factory.createGetField(clazz.getClassName(), f.getName(), fieldType));
        int size = fieldType.getSize();
        addStack(-1 + size);
    }

    protected static JavaClass lookupClass(String className) {
        try {
            return Repository.lookupClass(className);
        }
        catch (Exception e) {
            throwError("Could not find class '" + className + "'");
            return null;
        }
    }

    protected static PrimitiveTypeInfo getPrimitiveTypeInfo(Type type) {
        if (type instanceof ObjectType) {
            ObjectType ot = (ObjectType)type;
            String otName = ot.getClassName();
            for (PrimitiveTypeInfo info : primitiveTypes) {
                if (otName.equals(info.objectType.getClassName()))
                    return info;
            }
        }
        else {
            for (PrimitiveTypeInfo info : primitiveTypes) {
                if (type == info.type)
                    return info;
            }
        }
        return null;
    }
    
    protected boolean isStringType(Type type) {
        if (!(type instanceof ObjectType))
            return false;
        ObjectType objectType = (ObjectType)type;
        return objectType.getClassName().equals("java.lang.String");
    }

    // i.e., not byte, but only Byte, etc
    protected static boolean isPrimitiveObjectType(Type type) {
        if (type instanceof ObjectType) {
            ObjectType ot = (ObjectType)type;
            String otName = ot.getClassName();
            for (PrimitiveTypeInfo info : primitiveTypes) {
                if (otName.equals(info.objectType.getClassName()))
                    return true;
            }
        }
        return false;
    }
    
    // i.e., not Byte, but only byte, etc
    protected static boolean isPrimitiveType(Type type) {
        for (PrimitiveTypeInfo info : primitiveTypes) {
            if (type == info.type)
                return true;
        }
        return false;
    }
    
    // i.e., not Byte, but only byte, etc
    protected static String nonPrimitiveObjectTypeName(Type type) {
        if (!isPrimitiveType(type)) {
            if (type instanceof ObjectType) {   
                ObjectType ot = (ObjectType)type;
                String typeName = ot.getClassName();
                return typeName;
            }
        }
        return null;
    }
    
    protected Type underlyingPrimitiveType(Type type) {
        // First check the object types 
        if (type instanceof ObjectType) {
            ObjectType ot = (ObjectType)type;
            String typeName = ot.getClassName();
            for (PrimitiveTypeInfo info : primitiveTypes) {
                if (typeName.equals(info.objectType.getClassName()))
                    return info.type;
            }
        }
        else {
            for (PrimitiveTypeInfo info : primitiveTypes) {
                if (type == info.type)
                    return type;
            }
        }
        return null;
    }
    
    protected Type storageType(Type type) {
        Type underlying = underlyingPrimitiveType(type);
        if (underlying == null) {
            throwError("In storageType, unknown type '" + type + "'");
            return null;
        }
        else if (underlying == Type.BOOLEAN)
            return Type.BYTE;
        else
            return underlying;
    }
    
    protected Short getAggregateTypeNum(ObjectType fieldObjectType) {
        String s = fieldObjectType.getClassName();
        Short typeNum = MarshallingRuntime.builtinAggregateTypeNum(s);
        if (typeNum != null)
            return typeNum;
        else if (s.equals("java.util.List"))
            return MarshallingRuntime.typeNumLinkedList;
        else if (s.equals("java.util.Map"))
            return MarshallingRuntime.typeNumHashMap;
        else if (s.equals("java.util.Set"))
            return MarshallingRuntime.typeNumHashSet;
        else
            return null;
    }

    protected String aggregateTypeString(ObjectType fieldObjectType) {
        String s = fieldObjectType.getClassName();
        if (MarshallingRuntime.builtinAggregateTypeNum(s) != null) {
            int indexOfDot = s.lastIndexOf('.');
            if (indexOfDot >= 0)
                return s.substring(indexOfDot + 1);
        }
        throwError("InjectionGenerator:addMarshallingForField: unrecognized aggregate type " + s);
        return "";   // Keep the stupid compiler happy
    }

    protected boolean marshalledByMarshallingRuntimeMarshalObject(ObjectType fieldObjectType) {
        return (doesOrWillHandleMarshallable(fieldObjectType) ||
                referencesInterface(fieldObjectType) ||
                fieldObjectType.getClassName().equals("java.lang.Object"));
    }
    
    protected boolean referencesInterface(ObjectType type) {
        try {
            return type.referencesInterfaceExact();
        } catch (ClassNotFoundException e) {
            return false;
        }
    }

    protected boolean doesOrWillHandleMarshallable(ObjectType type) {
        String className = type.getClassName();
        if (MarshallingRuntime.hasMarshallingProperties(className))
            return MarshallingRuntime.injectedClass(className);
        else
            return handlesMarshallable(type);
    }
    
    protected boolean handlesMarshallable(ObjectType type) {
        String typeName = type.getClassName();
        return handlesMarshallable(typeName);
    }
    
    public static boolean handlesMarshallable(String typeName) {
        JavaClass clazz = lookupClass(typeName);
        if (clazz == null)
            throwError("InjectionGenerator.handlesMarshallable: Could not find class '" + typeName + "'");
        return handlesMarshallable(clazz);
    }
    
    public static boolean handlesMarshallable(JavaClass clazz) {
        String[] names = clazz.getInterfaceNames();
        for (String name : names) {
            if (marshallableClassName.equals(name))
                return true;
        }
        return false;
    }
    
    protected boolean objectTypeIsInterface(ObjectType type) {
        return interfaceClass(type.getClassName());
    }

    public static boolean interfaceClass(String s) {
        try {
            JavaClass jc = Repository.lookupClass(s);
            return !jc.isClass();
        } catch (ClassNotFoundException e) {
            return false;
        }
    }

    protected static LinkedList<Field> getValidClassFields(JavaClass c) {
        LinkedList<Field> validFields = new LinkedList<Field>();
        Field[]  fields = c.getFields();
        for (Field f : fields) {
            if (!f.isStatic() && !f.isTransient())
                validFields.add(f);
        }
        return validFields;
    }
    
    public static JavaClass getValidSuperclass(JavaClass c) {
        String superclassName = c.getSuperclassName();
        if (superclassName == null)
            return null;
        JavaClass superclass = lookupClass(superclassName);
        if (superclass != null &&
            !superclass.getClassName().equals("java.lang.Object") &&
            !superclass.isInterface())
            return superclass;
        else
            return null;
    }
    
    protected void noteBranchTargets(BranchFixup [] branchFixups, InstructionList il) {
        for (BranchFixup branchFixup : branchFixups)
            branchFixup.atTarget(il);
    }
    
    protected void initStack() {
        currentStack = 0;
        maxStack = 0;
    }
    
    protected void addStack(int count) {
        int newCurrent = currentStack + count;
        if (newCurrent < 0)
            throwError("InjectionGenerator.addStack: Stack depth below zero!");
        currentStack = newCurrent;
        if (newCurrent > maxStack)
            maxStack = newCurrent;
    }

    protected int getFinalStack() {
        if (currentStack != 0)
            throwError("InjectionGenerator.getFinalStack: Final stack depth should be zero, but is " + currentStack);
        return maxStack;
    }
   
    protected static void logInvoke(String s, InvokeInstruction iv, ConstantPoolGen cp) {
        Log.debug(s + " signature is '" + iv.getSignature(cp) + "', return type is " + iv.getReturnType(cp));
    }
    
    protected static void throwError(String msg) {
        Log.error(msg);
        throw new RuntimeException(msg);
    }
    
    // Updated by all the stack pushers and popers
    int currentStack;
    int maxStack;

    public static class BranchFixup {

        public BranchFixup(BranchHandle branch, InstructionList il) {
            this.branch = branch;
            this.lengthAtBranch = il.getLength();
            allBranchFixups.add(this);
        }

        public void atTarget(InstructionList il) {
            lengthAtTarget = il.getLength();
        }
        
        public static void fixAllFixups(InstructionList il) {
            for (BranchFixup branchFixup : allBranchFixups) {
                int delta = branchFixup.lengthAtTarget - branchFixup.lengthAtBranch + 1;
                BranchHandle branch = branchFixup.branch;
                InstructionHandle handle = branch;
                if (delta > 0) {
                    for (int i=0; i<delta; i++)
                        handle = handle.getNext();
                }
                else {
                    for (int i=0; i<-delta; i++)
                        handle = handle.getPrev();
                }
                branch.setTarget(handle);
            }
            allBranchFixups.clear();
        }
        
        public BranchHandle branch;
        public int lengthAtBranch;
        public int lengthAtTarget;

        public static LinkedList<BranchFixup> allBranchFixups = new LinkedList<BranchFixup>();
    }
    
    protected void initializeGlobals() {
        marshallingRuntimeClassName = "multiverse.server.marshalling.MarshallingRuntime";
        marshallableClassName = "multiverse.server.marshalling.Marshallable";
        mvByteBufferClassName = "multiverse.server.network.MVByteBuffer";
        
        mvByteBufferType = new ObjectType(mvByteBufferClassName);

        OBJBOOLEAN = new ObjectType("java.lang.Boolean");
        OBJBYTE = new ObjectType("java.lang.Byte");
        OBJCHAR = new ObjectType("java.lang.Character");
        OBJDOUBLE = new ObjectType("java.lang.Double");
        OBJFLOAT = new ObjectType("java.lang.Float");
        OBJINT = new ObjectType("java.lang.Integer");
        OBJLONG = new ObjectType("java.lang.Long");
        OBJSHORT = new ObjectType("java.lang.Short");

        primitiveTypes = new PrimitiveTypeInfo [] 
            { new PrimitiveTypeInfo(Type.BOOLEAN, OBJBOOLEAN, "Byte", "boolean"),
              new PrimitiveTypeInfo(Type.BYTE, OBJBYTE, "Byte", "byte"),
              new PrimitiveTypeInfo(Type.CHAR, OBJCHAR, "Char", "char"),
              new PrimitiveTypeInfo(Type.DOUBLE, OBJDOUBLE, "Double", "double"),
              new PrimitiveTypeInfo(Type.FLOAT, OBJFLOAT, "Float", "float"),
              new PrimitiveTypeInfo(Type.INT, OBJINT, "Int" , "int"),
              new PrimitiveTypeInfo(Type.LONG, OBJLONG, "Long", "long"),
              new PrimitiveTypeInfo(Type.SHORT, OBJSHORT, "Short", "short"),
//              new PrimitiveTypeInfo(Type.STRING, Type.STRING, "String", "String")
            };
    }

    HashSet<MarshallingRuntime.ClassNameAndTypeNumber> classesToBeMarshalled;

    static String marshallingRuntimeClassName;
    static String marshallableClassName;
    static String mvByteBufferClassName;

    ObjectType mvByteBufferType;
    
    ObjectType OBJBOOLEAN;
    ObjectType OBJBYTE;
    ObjectType OBJCHAR;
    ObjectType OBJDOUBLE;
    ObjectType OBJFLOAT;
    ObjectType OBJINT;
    ObjectType OBJLONG;
    ObjectType OBJSHORT;

    protected boolean generateClassFiles = false;
    protected String outputDir = "";
    protected boolean listGeneratedCode = false;
    
    public static class PrimitiveTypeInfo {
        public PrimitiveTypeInfo(Type type, ObjectType objectType, String mvByteBufferSuffix,
                                 String valueString) {
            this.type = type;
            this.objectType = objectType;
            this.mvByteBufferSuffix = mvByteBufferSuffix;
            this.valueString = valueString;
        }
        
        public Type type;
        public ObjectType objectType;
        public String mvByteBufferSuffix;
        public String valueString;
    }

    protected static PrimitiveTypeInfo [] primitiveTypes;
    
    protected static InjectionGenerator instance = null;

    public static void initialize(boolean generateClassFiles, String outputDir, boolean listGeneratedCode) {
        instance = new InjectionGenerator(generateClassFiles, outputDir, listGeneratedCode);
        instance.initializeGlobals();
    }

}
