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

public class TestClass9 implements Marshallable {

    public void init() {
        intList = new LinkedList<Integer>();
        intList.add(1);
        intList.add(2);
        intList.add(3);
        propMap = new HashMap<String, Object>();
        propMap.put("first", "silly");
        objectSet = new HashSet<Object>();
        objectSet.add((Integer)25);
    }
    
    public void marshalObject(MVByteBuffer buf) {
        byte flags = 0;
        if (intList != null)
            flags = 1;
        if (propMap != null)
            flags |= 2;
        if (objectSet != null)
            flags |= 4;
        buf.putByte(flags);
        if (intList != null)
            MarshallingRuntime.marshalLinkedList(buf, intList);
        if (propMap != null)
            MarshallingRuntime.marshalHashMap(buf, propMap);
        if (objectSet != null)
            MarshallingRuntime.marshalHashSet(buf, objectSet);
    }

    public Object unmarshalObject(MVByteBuffer buf) {
        byte flags = buf.getByte();
        if ((flags & 1) != 0)
            intList = (LinkedList)MarshallingRuntime.unmarshalLinkedList(buf);
        if ((flags & 2) != 0)
            propMap = (HashMap)MarshallingRuntime.unmarshalHashMap(buf);
        if ((flags & 4) != 0)
            objectSet = (HashSet)MarshallingRuntime.unmarshalHashSet(buf);
        return this;
    }

    public LinkedList<Integer> intList;
    public HashMap<String, Object> propMap;
    public HashSet<Object> objectSet;
}
