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

package multiverse.server.objects;

import java.io.*;

/**
 * interface to implement an object state, such a "DeathState", 
 * "CombatState", etc.  States should be serializable into
 * a String for the state name, and an integer representing the
 * value.  This allows the state to be serialized to the client if 
 * needed.  If the state is server side only, then it is not important.
 * All states should be serializable so they can be stored into 
 * the database along with the object
 * 
 */
public abstract class ObjState implements Serializable {
    public abstract Integer getIntValue();
    public abstract String getStateName();
    
    public int hashCode() {
        return getStateName().hashCode();
    }
    
    public boolean equals(Object other) {
        if (! (other instanceof ObjState)) {
            return false;
        }
        return (getStateName().equals(((ObjState)other).getStateName()));
    }
}
