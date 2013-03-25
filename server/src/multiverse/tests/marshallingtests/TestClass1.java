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
import multiverse.server.network.*;
import multiverse.server.math.*;
import multiverse.server.marshalling.*;

public class TestClass1 implements Marshallable {

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

    public void marshalObject(MVByteBuffer buf) {
        byte flags = 0;
        if (BooleanVal != null)
            flags |= (byte)1;
        if (BooleanVal != null)
            flags |= (byte)2;
        if (ByteVal != null)
            flags |= (byte)4;
        if (ShortVal != null)
            flags |= (byte)8;
        if (IntegerVal != null)
            flags |= (byte)16;
        if (LongVal != null)
            flags |= (byte)32;
        if (FloatVal != null)
            flags |= (byte)64;
        if (DoubleVal != null)
            flags |= 128;
        buf.putByte(flags);
        flags = 0;
        if ((stringVal != null) && (stringVal != ""))
            flags |= (byte)1;
        if (stringList != null)
            flags |= (byte)2;
        if (stringMap != null)
            flags |= (byte)4;
        if (points != null)
            flags |= (byte)8;
        buf.putByte(flags);
        if (BooleanVal != null)
            buf.putByte((byte)(BooleanVal ? 1 : 0));
        if (ByteVal != null)
            buf.putByte(ByteVal);
        if (ShortVal != null)
            buf.putShort(ShortVal);
        if (IntegerVal != null)
            buf.putInt(IntegerVal);
        if (LongVal != null)
            buf.putLong(LongVal);
        if (FloatVal != null)
            buf.putFloat(FloatVal);
        if (DoubleVal != null)
            buf.putDouble(DoubleVal);
        buf.putByte((byte)(booleanVal ? 1 : 0));
        buf.putByte(byteVal);
        buf.putShort(shortVal);
        buf.putInt(integerVal);
        buf.putLong(longVal);
        buf.putFloat(floatVal);
        buf.putDouble(doubleVal);
        if ((stringVal != null) && (stringVal != ""))
            buf.putString(stringVal);
//        if (stringList != null)
//            MarshallingRuntime.marshallers[247].toBytes(buf, stringList);
//        if (stringMap != null)
//            MarshallingRuntime.marshallers[248].toBytes(buf, stringMap);
//        if (points != null)
//            MarshallingRuntime.marshallers[249].toBytes(buf, points);
    }

    public Object unmarshalObject(MVByteBuffer buf) {
        byte flags0 = buf.getByte();
        byte flags1 = buf.getByte();
        if ((flags0 & 1) != 0)
            BooleanVal = buf.getByte() != 0;
        if ((flags0 & 4) != 0)
            ByteVal = buf.getByte();
        if ((flags0 & 8) != 0)
            ShortVal = buf.getShort();
        if ((flags0 & 16) != 0)
            IntegerVal = buf.getInt();
        if ((flags0 & 32) != 0)
            LongVal = buf.getLong();
        if ((flags0 & 64) != 0)
            FloatVal = buf.getFloat();
        if ((flags0 & 128) != 0)
            DoubleVal = buf.getDouble();
        booleanVal = buf.getByte() != 0;
        byteVal = buf.getByte();
        shortVal = buf.getShort();
        integerVal = buf.getInt();
        longVal = buf.getLong();
        floatVal = buf.getFloat();
        doubleVal = buf.getDouble();
        if ((flags1 & 1) != 0)
            stringVal = buf.getString();
//        if ((flags1 & 2) != 0)
//            stringList = (LinkedList)MarshallingRuntime.marshallers[247].parseBytes(buf);;
//        if ((flags1 & 4) != 0)
//            stringMap = (HashMap)MarshallingRuntime.marshallers[248].parseBytes(buf);;
//        if ((flags1 & 8) != 0)
//            points = (HashSet)MarshallingRuntime.marshallers[249].parseBytes(buf);;
        return this;
    }
}
