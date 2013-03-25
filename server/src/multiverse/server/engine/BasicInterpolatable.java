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

package multiverse.server.engine;

import multiverse.server.math.*;
import multiverse.server.pathing.*;

public interface BasicInterpolatable extends Interpolatable {
    public MVVector getDir();
    public void setDir(MVVector dir);

    public Point getRawLoc();
    public void setRawLoc(Point p);

    public Point getInterpLoc();
    public void setInterpLoc(Point p);

    public long getLastInterp();
    public void setLastInterp(long time);
    
    public Quaternion getOrientation();
    public void setOrientation(Quaternion orient);

    public PathInterpolator getPathInterpolator();
    
    // One call to set all the values
    public void setPathInterpolatorValues(long time, MVVector newDir, Point newLoc, Quaternion orientation);
}
