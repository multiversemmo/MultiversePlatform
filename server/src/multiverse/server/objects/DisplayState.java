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

public class DisplayState implements Serializable {
    public static DisplayState IN_COMBAT = new DisplayState(1);
    public static DisplayState NON_COMBAT = new DisplayState(2);

    public DisplayState() {
    }

    public DisplayState(int id) {
        this.id = id;
    }
    
    public void setId(int id) {
        this.id = id;
    }
    public int getId() {
        return id;
    }

    public int hashCode() {
        return getId();
    }

    public String toString() {
        return "[DisplayState id=" + getId() + "]";
    }

    public boolean equals(Object other) {
        DisplayState otherDS = (DisplayState) other;
        return (this.id == otherDS.getId());
    }
    int id = -1;
    private static final long serialVersionUID = 1L;
}

