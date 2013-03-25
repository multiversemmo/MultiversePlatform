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

package multiverse.tests.networktests;

import java.io.*;
import java.util.*;

import multiverse.msgsys.*;

public class TestSerialization {

    /**
     * TestMessage is just a vehicle for testing serialization
     * 
     */
    public static class TestMessage extends Message implements Serializable {

        public TestMessage() {
            super(MSG_TYPE_TEST_MESSAGE);
            setupTransient();
        }
        
        public TestMessage(int primitiveCount) {
            super(MSG_TYPE_TEST_MESSAGE);
            setupTransient();
            for (int i=0; i<primitiveCount; i++)
                theNumbers.add(100000L + i);
        }
        
        protected void setupTransient() {
            theNumbers = new LinkedList<Long>();
        }
        
        private void readObject(ObjectInputStream in) throws IOException,
                ClassNotFoundException {
            log("TestMessage.readObject called");
//             in.defaultReadObject();
            setupTransient();
            int count = in.readInt();
            log("TestMessage.readObject: count is " + count);
            for (int i=0; i<count; i++)
                theNumbers.add(in.readLong());
        }
        
        private void writeObject(ObjectOutputStream out) throws IOException,
                ClassNotFoundException {
//             out.defaultWriteObject();
            log("TestMessage.writeObject called");
            int count = theNumbers.size();
            log("TestMessage.writeObject: count is " + count);
            out.writeInt(count);
            for (Long number : theNumbers)
                out.writeLong(number);
        }
        
        public String moreToString() {
            String s = "";
            for (Long number : theNumbers)
                s += number + ", ";
            return ", theNumbers=[" + s + "]";
        }
        
        transient LinkedList<Long> theNumbers;
        private static final long serialVersionUID = 1L;
    }

    public static class BaseClass implements Externalizable {
        public BaseClass() {}

        public void readExternal(ObjectInput in)
                 throws IOException, ClassNotFoundException {
            readObjectUtility(in, this);
        }
        
        public static void readObjectUtility(ObjectInput in, BaseClass instance)
                 throws IOException, ClassNotFoundException {
            instance.foo = in.readInt();
        }
        
        public void writeExternal(ObjectOutput out)
                 throws IOException {
            writeObjectUtility(out, this);
        }
        
        public static void writeObjectUtility(ObjectOutput out, BaseClass instance)
                 throws IOException {
            out.writeInt(instance.foo);
        }
        
        transient public int foo = 23;
        private static final long serialVersionUID = 1L;
    }
    
    public static class DerivedClass1 extends BaseClass implements Externalizable {

        public DerivedClass1() {}

        public void readExternal(ObjectInput in)
                 throws IOException, ClassNotFoundException {
            readObjectUtility(in, this);
        }
        
        public static void readObjectUtility(ObjectInputStream in, DerivedClass1 instance)
                 throws IOException, ClassNotFoundException {
            instance.foo = in.readInt();
            BaseClass.readObjectUtility(in, instance);
        }
        
        public void writeExternal(ObjectOutput out)
                 throws IOException {
            writeObjectUtility(out, this);
        }
        
        public static void writeObjectUtility(ObjectOutput out, DerivedClass1 instance)
                 throws IOException {
            out.writeInt(instance.foo);
            BaseClass.writeObjectUtility(out, instance);
        }
        
        transient public int bar = 45;
        private static final long serialVersionUID = 1L;
    }
    
    public static class DerivedClass2 extends DerivedClass1 {

        public DerivedClass2() {}
        
        public int bletch = 45;

        private static final long serialVersionUID = 1L;
    }
    
    public static class PointClass implements Externalizable {
        public PointClass() {}
        
        public void writeExternal(ObjectOutput out) throws IOException {
            out.writeInt(_x);
            out.writeInt(_y);
            out.writeInt(_z);
        }

        /*
         * deserializes this object.  usually to read in from a database
         * or from another server sending us an object
         *
         * @see java.io.Externalizable
         */
        public void readExternal(ObjectInput in)
            throws IOException, ClassNotFoundException {
            _x = in.readInt();
            _y = in.readInt();
            _z = in.readInt();
        }

        public String toString() {
            return "PointClass[_x=" + _x + ", _y=" + _y + ", _z=" + _z + "]";
        }
        
        private int _x = 1;
        private int _y = 2;
        private int _z = 3;

        private static final long serialVersionUID = 1L;
    }
    
    public static class PointUser implements Serializable {
        
        public PointUser() {}
        
        private void readObject(ObjectInputStream in)
               throws IOException, ClassNotFoundException {
            foo = in.readInt();
            PointClass p = new PointClass();
            p.readExternal(in);
            bletch = p;
            bar = in.readFloat();
        }
        
        private void writeObject(ObjectOutputStream out)
               throws IOException, ClassNotFoundException {
            out.writeInt(foo);
            bletch.writeExternal(out);
            out.writeFloat(bar);
        }
        
        public String toString() {
            return "PointUser[foo=" + foo + ", bletch=" + bletch + ", bar=" + bar + "]";
        }
        
        transient int foo = 3;
        transient PointClass bletch = new PointClass();
        transient float bar = 3.14159f;

        private static final long serialVersionUID = 1L;
    }
    
    public static byte[] toBytes(Object obj) throws IOException {
        ByteArrayOutputStream ba = new ByteArrayOutputStream();
        ObjectOutputStream os = new ObjectOutputStream(ba);
        os.writeObject(obj);
        os.flush();
        ba.flush();
        return ba.toByteArray();
    }

    private static void testSerialization(Object obj) {
        try {
            log("Testing serialization of object " + obj);
            byte[] data = toBytes(obj);
            log("Encode message: " + data.length + " bytes, decoded " + byteArrayToHexString(data));
            ByteArrayInputStream bs = new ByteArrayInputStream(data);
            ObjectInputStream ois = new ObjectInputStream(bs);
            log("Before Decode message");
            Object newObj = ois.readObject();
            log("Finished decoding message");
            log("Decode message: " + newObj);
        }
        catch (Exception e) {
            log("Exception: " + exceptionToString(e));
        }
    }
    
    static String byteArrayToHexString(byte in[]) {
        byte ch = 0x00;
        int i = 0; 
        if (in == null || in.length <= 0)
            return null;
        String pseudo[] = {"0", "1", "2",
                           "3", "4", "5", "6", "7", "8",
                           "9", "A", "B", "C", "D", "E",
                           "F"};
        StringBuffer out = new StringBuffer(in.length * 2);
        StringBuffer chars = new StringBuffer(in.length);
        while (i < in.length) {
            ch = (byte) (in[i] & 0xF0); // Strip off high nibble
            ch = (byte) (ch >>> 4); // shift the bits down
            ch = (byte) (ch & 0x0F); // must do this if high order bit is on!
            out.append(pseudo[ (int) ch]); // convert the nibble to a String Character
            ch = (byte) (in[i] & 0x0F); // Strip off low nibble 
            out.append(pseudo[ (int) ch]); // convert the nibble to a String Character
            if (in[i] >= 32 && in[i] <= 126)
                chars.append((char)in[i]);
            else
                chars.append("*");
            i++;
        }
        return new String(out) + " == " + new String(chars);
    } 

    public static String exceptionToString(Exception e) {
        Throwable throwable = e;
        StringBuilder traceStr = new StringBuilder(1000);
        do {
            traceStr.append(throwable.toString());
            for (StackTraceElement elem : throwable.getStackTrace()) {
                traceStr.append("\n       at ");
                traceStr.append(elem.toString());
            }
            throwable = throwable.getCause();
            if (throwable != null) {
                traceStr.append("\nCaused by: ");
            }
        } while (throwable != null);
        return traceStr.toString();
    }

    private static void log(String s) {
        logStream.println(s);
        logStream.flush();
    }

    static PrintWriter logStream = null;
        
    public static void main(String args[]) {
        try {
            logStream = new PrintWriter(new BufferedWriter(new FileWriter("testserialization.txt")));
        }
        catch (Exception e) {
            System.out.println("Error creating log stream: " + e.getMessage());
        }
        
        log("Starting...");
        
        PointUser p = new PointUser();
        testSerialization(p);
        
        DerivedClass1 c1 = new DerivedClass1();
        testSerialization(c1);
        
        DerivedClass2 c2 = new DerivedClass2();
        testSerialization(c2);

        TestMessage testMsg1 = new TestMessage(100);
        testSerialization(testMsg1);
        
        TestMessage testMsg2 = new TestMessage(200);
        testSerialization(testMsg2);
    }
    
    public static MessageType MSG_TYPE_TEST_MESSAGE = MessageType.intern("testMsg");
}
