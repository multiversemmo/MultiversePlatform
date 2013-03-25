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

package multiverse.tests.marshallingtests;

import java.util.*;
import multiverse.server.util.*;
import multiverse.server.network.*;
import multiverse.server.marshalling.*;

public class TestHarness 
{
    
    public static void testPassed(String test, int bufSize) {
        Log.debug(test + " passed, " + bufSize + " bytes used");
    }
    
    public static void testFailed(String test, String why) {
        Log.debug(test + " failed, because " + why);
    }

    protected static void showInterfaces(Object object) {
        Class c = object.getClass();
        java.lang.reflect.Type genSuper = c.getGenericSuperclass();
        Class [] interfaces = c.getInterfaces();
        String s = "";
        for (Class iface : interfaces) {
            if (s != "")
                s += ", ";
            s += iface.getClass().getName();
        }
        Log.info("logGenericClassInfo Class " + c + ", genSuper " + genSuper + ", interfaces " + s);
    }
        
    public static void test4() {
        // Test1: The simplest possible test . . .
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        buf.clear();
        GTestClass4 gt4 = new GTestClass4();
        showInterfaces(gt4);
        gt4.s = "Should get this back";
        gt4.b = true;
        Object original = gt4;
        MarshallingRuntime.marshalObject(buf, original);
        bufSize = buf.position();
        buf.flip();
        GTestClass4 gt4Revived = new GTestClass4();
        gt4Revived = (GTestClass4)MarshallingRuntime.unmarshalObject(buf);
        if ((gt4.s.equals(gt4Revived.s)) || (gt4.b != gt4Revived.b))
            testPassed("test4", bufSize);
        else
            testFailed("test4", "result read back does not agree.");
        buf.clear();
    }
    
    public static void test8() {
        // Test2: All the fundamental types
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        GTestClass8 gt8 = new GTestClass8();
        MarshallingRuntime.marshalObject(buf, gt8);
        bufSize = buf.position();
        buf.flip();
        GTestClass8 gt8Revived = new GTestClass8();
        gt8Revived = (GTestClass8)MarshallingRuntime.unmarshalObject(buf);
        if ((gt8.BooleanVal.equals(gt8Revived.BooleanVal)) &&
            (gt8.ByteVal.equals(gt8Revived.ByteVal)) &&
            (gt8.ShortVal.equals(gt8Revived.ShortVal)) &&
            (gt8.IntegerVal.equals(gt8Revived.IntegerVal)) &&
            (gt8.LongVal.equals(gt8Revived.LongVal)) &&
            //                 (gt8.FloatVal.equals(gt8Revived.FloatVal)) &&
            //                 (gt8.DoubleVal.equals(gt8Revived.DoubleVal)) &&
            (gt8.BooleanVal2.equals(gt8Revived.BooleanVal2)) &&
            (gt8.stringVal.equals(gt8Revived.stringVal)) &&
            (gt8.booleanVal == gt8Revived.booleanVal) &&
            (gt8.byteVal == gt8Revived.byteVal) &&
            (gt8.shortVal == gt8Revived.shortVal) &&
            (gt8.integerVal == gt8Revived.integerVal) &&
            (gt8.longVal == gt8Revived.longVal) // &&
            //                 (gt8.floatVal == gt8Revived.floatVal) &&
            //                 (gt8.doubleVal == gt8Revived.doubleVal)
            )
            testPassed("test8", bufSize);
        else
            testFailed("test8", "result read back does not agree.");
    }
    
    public static void test9() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        GTestClass9 gt9 = new GTestClass9();
        gt9.init();
        MarshallingRuntime.marshalObject(buf, gt9);
        bufSize = buf.position();
        buf.flip();
        GTestClass9 gt9Revived = new GTestClass9();
        gt9Revived = (GTestClass9)MarshallingRuntime.unmarshalObject(buf);
        if ((gt9.intList.get(0).equals(gt9Revived.intList.get(0))) &&
            (gt9.intList.get(1).equals(gt9Revived.intList.get(1))) &&
            (gt9.intList.get(2).equals(gt9Revived.intList.get(2))) &&
            (gt9.propMap.get("first").equals(gt9Revived.propMap.get("first"))) &&
            (gt9.objectSet.contains((Integer)25)) &&
            (gt9Revived.objectSet.contains((Integer)25)))
            testPassed("test9", bufSize);
        else
            testFailed("test9", "result read back does not agree."); 
    }

    public static void test9a() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        TestClass9 t9 = new TestClass9();
        t9.init();
        t9.marshalObject(buf);
        bufSize = buf.position();
        buf.flip();
        TestClass9 t9Revived = new TestClass9();
        t9Revived = (TestClass9)t9Revived.unmarshalObject(buf);
        if ((t9.intList.get(0).equals(t9Revived.intList.get(0))) &&
            (t9.intList.get(1).equals(t9Revived.intList.get(1))) &&
            (t9.intList.get(2).equals(t9Revived.intList.get(2))) &&
            (t9.propMap.get("first").equals(t9Revived.propMap.get("first"))) &&
            (t9.objectSet.contains((Integer)25)) &&
            (t9Revived.objectSet.contains((Integer)25)))
            testPassed("test9a", bufSize);
        else
            testFailed("test9a", "result read back does not agree."); 
    }

    public static void test10() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        GTestClass10 gt10 = new GTestClass10();
        MarshallingRuntime.marshalObject(buf, gt10);
        bufSize = buf.position();
        buf.flip();
        GTestClass10 gt10Revived = new GTestClass10();
        gt10Revived = (GTestClass10)MarshallingRuntime.unmarshalObject(buf);
        if ((gt10.intList.get(0).equals(gt10Revived.intList.get(0))) &&
            (gt10.intList.get(1).equals(gt10Revived.intList.get(1))) &&
            (gt10.intList.get(2).equals(gt10Revived.intList.get(2))))
            testPassed("test10", bufSize);
        else
            testFailed("test10", "result read back does not agree."); 
    }

    public static void test11() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        GTestClass11 gt11 = new GTestClass11();
        HashMap<String, Object> map = new HashMap<String, Object>();
        map.put("Yow", "Goof");
        gt11.setFoo(map);
        MarshallingRuntime.marshalObject(buf, gt11);
        bufSize = buf.position();
        buf.flip();
        GTestClass11 gt11Revived = new GTestClass11();
        gt11Revived = (GTestClass11)MarshallingRuntime.unmarshalObject(buf);
        HashMap revivedMap = (HashMap<String, Object>)gt11Revived.foo;
        if (revivedMap.get("Yow").equals(map.get("Yow")))
            testPassed("test11", bufSize);
        else
            testFailed("test11", "result read back does not agree."); 
    }

    public static void test12() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        GTestClass12 gt12 = new GTestClass12();
        TestClass5 tc5 = new TestClass5();
        tc5.b = true;
        gt12.c = tc5;
        MarshallingRuntime.marshalObject(buf, gt12);
        bufSize = buf.position();
        buf.flip();
        GTestClass12 gt12Revived = new GTestClass12();
        gt12Revived = (GTestClass12)MarshallingRuntime.unmarshalObject(buf);
        if (gt12Revived.c.b)
            testPassed("test12", bufSize);
        else
            testFailed("test12", "result read back does not agree."); 
    }

    public static class TestClass14 {
        public TestClass14() {
        }
        
        public Map<String, Map<String,Object>> propMap = new HashMap<String, Map<String,Object>>();
    }
    
    public static void test14() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        TestClass14 t14 = new TestClass14();
        HashMap<String, Object> baz = new HashMap<String, Object>();
        baz.put("0", "Blow");
        baz.put("1", "Me");
        t14.propMap.put("Insult", baz);
        HashMap<String, Object> foo = new HashMap<String, Object>();
        foo.put("0", "Kiss");
        foo.put("1", "Me");
        t14.propMap.put("Entreaty", foo);
        MarshallingRuntime.marshalObject(buf, t14);
        bufSize = buf.position();
        buf.flip();
        TestClass14 t14Revived = new TestClass14();
        t14Revived = (TestClass14)MarshallingRuntime.unmarshalObject(buf);
        if (t14Revived.propMap.size() == 2) {
            HashMap<String, Object> rbaz = (HashMap<String, Object>)t14Revived.propMap.get("Insult");
            HashMap<String, Object> rfoo = (HashMap<String, Object>)t14Revived.propMap.get("Entreaty");
            if (rbaz.size() == baz.size() &&
                rfoo.size() == foo.size() &&
                rbaz.get("0").equals(baz.get("0")) &&
                rbaz.get("1").equals(baz.get("1")) &&
                rfoo.get("0").equals(foo.get("0")) &&
                rfoo.get("1").equals(foo.get("1")))
                testPassed("test14", bufSize);
        }
        else
            testFailed("test14", "result read back does not agree."); 
    }

    public static class TestClass15 {
        public TestClass15() {
        }
        public Object foo;
        
        public Map<String, Object> propMap = new HashMap<String, Object>();
    }
    
    public static void test15() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        TestClass15 t15 = new TestClass15();
        byte [] bytes = new byte[25];
        for (int i=0; i<25; i++)
            bytes[i] = (byte)i;
        t15.foo = bytes;
        MarshallingRuntime.marshalObject(buf, t15);
        bufSize = buf.position();
        buf.flip();
        TestClass15 t15Revived = new TestClass15();
        t15Revived = (TestClass15)MarshallingRuntime.unmarshalObject(buf);
        boolean passed = true;
        bytes = (byte [])t15Revived.foo;
        for (int i=0; i<25; i++)
            if (bytes[i] != (byte)i)
                passed = false;
        if (passed)
            testPassed("test15", bufSize);
        else
            testFailed("test15", "result read back does not agree."); 
    }

    public static class TestClass16 {
        public LinkedList<Object> list;
    }
    
    public static void test16() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        TestClass16 t16 = new TestClass16();
        t16.list = new LinkedList<Object>();
        t16.list.add((Long)23L);
        MarshallingRuntime.marshalObject(buf, t16);
        bufSize = buf.position();
        buf.flip();
        TestClass16 t16Revived = new TestClass16();
        t16Revived = (TestClass16)MarshallingRuntime.unmarshalObject(buf);
        if (t16.list.get(0).equals(t16Revived.list.get(0)))
            testPassed("test16", bufSize);
        else
            testFailed("test16", "result read back does not agree."); 
    }
    
    public static class TestClass17 {
        Object foo;
    }
    
    public static void test17() {
        MVByteBuffer buf = new MVByteBuffer(1000);
        int bufSize;
        TestClass17 t17 = new TestClass17();
        t17.foo = (Long)23L;
        MarshallingRuntime.marshalObject(buf, t17);
        bufSize = buf.position();
        buf.flip();
        TestClass17 t17Revived = new TestClass17();
        t17Revived = (TestClass17)MarshallingRuntime.unmarshalObject(buf);
        if (t17.foo.equals(t17Revived.foo))
            testPassed("test17", bufSize);
        else
            testFailed("test17", "result read back does not agree."); 
    }
    
    public static void main(String args[]) {

        String test = "None";

        try {
            test4();
            test8();
            test9();
            test9a();
            test10();
            test11();
            test14();
            test15();
            test16();
            test17();
        }
        catch (Exception e) {
            Log.exception(test, e);
        }
    }
}
