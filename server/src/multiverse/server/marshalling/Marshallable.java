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

import multiverse.server.network.*;

/**
 * This interface specifies the two methods, marshalObject() and
 * unmarshalObject(), that must be implemented by a class that supports
 * marshalling
 */
public interface Marshallable {
    
    /**
     * Marshal the object into the byte buffer argument.
     * @param buf The byte buffer
     */
    public void marshalObject(MVByteBuffer buf);
    
    /**
     * Unmarshal from the byte buffer argument, returning
     * an Object that containing the unmarshalled state.  Nearly
     * all implementors of Marshallable unmarshall the state into
     * this, and return this.  However, some implementors need to
     * "intern" the result of unmarshalling, and will potentially
     * return a value different from this.  This provides the 
     * functionality of Java Serialiable's readResolve() method.
     * Examples in the server of classes that intern objects during 
     * unmarshalling include MessageType and Namespace.
     */
    public Object unmarshalObject(MVByteBuffer buf);
}
