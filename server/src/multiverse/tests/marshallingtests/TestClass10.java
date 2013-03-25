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
import multiverse.server.marshalling.*;

public class TestClass10 implements Marshallable {

    public TestClass10() {
        intList = new LinkedList<Integer>();
        intList.add(1);
        intList.add(2);
        intList.add(3);
    }

    public LinkedList<Integer> intList = new LinkedList<Integer>();

    public void marshalObject(MVByteBuffer buf) {
        byte flags = 0;
        if (intList != null)
            flags = 1;
        buf.putByte(flags);
        if (intList != null)
            MarshallingRuntime.marshalLinkedList(buf, intList);
    }

    public Object unmarshalObject(MVByteBuffer buf) {
        byte flags = buf.getByte();
        if ((flags & 1) != 0)
            intList = (LinkedList<Integer>)MarshallingRuntime.unmarshalLinkedList(buf);
        return this;
    }
    
}
