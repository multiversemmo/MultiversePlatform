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

public class Fog {
    public Fog() {
    }

    public Fog(String name) {
        setName(name);
    }

    public Fog(String name, 
               Color c,
               int start,
               int end) {
        setName(name);
        setColor(c);
        setStart(start);
        setEnd(end);
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }
    
    public String toString() {
	return "[Fog: color=" + color +
	    ", start=" + fogStart +
	    ", end=" + fogEnd + 
	    "]";
    }

    public void setColor(Color c) {
        this.color = c;
    }
    public Color getColor() {
        return color;
    }

    public void setStart(int start) {
        this.fogStart = start;
    }
    public int getStart() {
        return fogStart;
    }
    
    public void setEnd(int end) {
        this.fogEnd = end;
    }
    public int getEnd() {
        return fogEnd;
    }

    Color color = null;
    int fogStart = -1;
    int fogEnd = -1;

//     public void writeExternal(ObjectOutput out) throws IOException {
//         lock.lock();
// 	try {
//             super.writeExternal(out);
// 	    MVObject.writeObject(out, color);
//             out.writeInt(fogStart);
//             out.writeInt(fogEnd);
//         }
//         finally {
//             lock.unlock();
//         }
//     }

//     public void readExternal(ObjectInput in)
// 	throws IOException, ClassNotFoundException {
//         lock.lock();
// 	try {
// 	    super.readExternal(in);
// 	    this.color = (Color) MVObject.readObject(in);
//             this.fogStart = in.readInt();
//             this.fogEnd = in.readInt();
//         }
//         finally {
//             lock.unlock();
//         }
//     }
 
//     public void spawn() {
// 	super.spawn();
//     }

    
    protected String name = null;
    
    private static final long serialVersionUID = 1L;
}
