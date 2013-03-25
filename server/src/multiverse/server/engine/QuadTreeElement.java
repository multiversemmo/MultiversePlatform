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

public interface QuadTreeElement<ElementType extends QuadTreeElement<ElementType>> extends Locatable, java.io.Serializable {
    public Object getQuadTreeObject();

    public MobilePerceiver<ElementType> getPerceiver();
    public void setPerceiver(MobilePerceiver<ElementType> p);

    public QuadTreeNode<ElementType> getQuadNode();
    public void setQuadNode(QuadTreeNode<ElementType> node);
    
    // Get the current location _without_ running the interpolator
    public Point getCurrentLoc();

    // How far away the object should be seen.  If zero, then there
    // will be no extent-based behavior
    public int getPerceptionRadius();

    // What is the radius of the object itself?  This is only used by
    // the pathing code, which has a way to compute the value
    public int getObjectRadius();
    
}

