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

package multiverse.server.pathing;

import multiverse.server.math.*;

/**
 * A class to encapsulate the return values from path interpolators
 */
public class PathLocAndDir {

    public PathLocAndDir(Point loc, MVVector dir, float lengthLeft) {
        this.loc = loc;
        this.dir = dir;
        this.lengthLeft = lengthLeft;
    }

    public Point getLoc() {
        return loc;
    }
    
    public MVVector getDir() {
        return dir;
    }
    
    public Quaternion getOrientation() {
        MVVector ndir = new MVVector(dir.getX(), 0, dir.getZ());
        float length = ndir.length();
        if (length != 0) {
            // normalize the vector then set the speed
            ndir.normalize();
            return Quaternion.fromVectorRotation(new MVVector(0,0,1), ndir);
        }
        else
            return Quaternion.Identity;
    }

    public float getLengthLeft() {
        return lengthLeft;
    }

    protected Point loc;
    protected MVVector dir;
    protected float lengthLeft;
}
