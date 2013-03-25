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

package multiverse.server.math;

import java.io.*;
import java.util.*;

/**
 * used in quad tree
 * works with java bean xml serialization
 */
public class Geometry implements Serializable, Cloneable {

    public Geometry() {
    }
    
    public Geometry(int minX, int maxX, int minZ, int maxZ) {
	this.minX = minX;
	this.maxX = maxX;
	this.minZ = minZ;
	this.maxZ = maxZ;
    }

    public String toString() {
	return "[Geometry minX=" + getMinX() +
	    ",maxX=" + getMaxX() +
	    ",minZ=" + getMinZ() +
	    ",maxZ=" + getMaxZ() +
	    "]";
    }

    public Object clone() {
	return new Geometry(minX, maxX, minZ, maxZ);
    }

    public boolean equals(Geometry other) {
	return ((minX == other.minX) &&
		(maxX == other.maxX) &&
		(minZ == other.minZ) &&
		(maxZ == other.maxZ));
    }

    public int getMinX() {
	return minX;
    }
    public int getMaxX() {
	return maxX;
    }
    public int getMinZ() {
	return minZ;
    }
    public int getMaxZ() {
	return maxZ;
    }

    public void setMinX(int x) {
	minX = x;
    }
    public void setMaxX(int x) {
	maxX = x;
    }
    public void getMinZ(int z) {
	minZ = z;
    }
    public void getMaxZ(int z) {
	maxZ = z;
    }

    /**
     * returns the point that represents the center of this node.
     * the Y is set to 0
     */
    Point getCenter() {
	int halfX = (int)(((long)maxX - (long)minX) / 2 - 1);
	int halfZ = (int)(((long)maxZ - (long)minZ) / 2 - 1);
	return new Point(halfX, 0, halfZ);
    }

    /**
     * pt is within this Geometry (edge included)
     */
    public boolean contains(Point pt) {
	if (pt == null) {
	    return false;
	}
	return ((pt.getX() >= getMinX()) &&
		(pt.getX() <= getMaxX()) &&
		(pt.getZ() >= getMinZ()) &&
		(pt.getZ() <= getMaxZ()));
    }

    /**
     * g is entirely contained within this Geometry
     */
    public boolean contains(Geometry g) {
	if (g == null) {
	    return false;
	}
	return ((g.getMinX() >= getMinX()) &&
		(g.getMaxX() <= getMaxX()) &&
		(g.getMinZ() >= getMinZ()) &&
		(g.getMaxZ() <= getMaxZ()));
    }

    /**
     * g overlaps with this Geometry
     */
    public boolean overlaps(Geometry g) {
	if (g == null) {
	    return false;
	}
	return ((g.getMaxX() >= getMinX()) &&
		(g.getMinX() <= getMaxX()) &&
		(g.getMaxZ() >= getMinZ()) &&
		(g.getMinZ() <= getMaxZ()));
    }

    /**
     * returns the 4 corners for this geometry
     */
    public Collection<Point> getCorners() {
	Collection<Point> corners = new LinkedList<Point>();
	corners.add(new Point(minX, 0, minZ));
	corners.add(new Point(minX, 0, maxZ));
	corners.add(new Point(maxX, 0, minZ));
	corners.add(new Point(maxX, 0, maxZ));
	return corners;
    }

    /**
     * divides this geometry into 4 equal squares(does not change this obj)
     * and returns the 4 element array
     * 0 1
     * 2 3
     */
    public Geometry[] divide() {
	long ldiffX = (long)maxX - (long)minX;
	long lhalfX = ldiffX / 2 - 1;

	long ldiffZ = (long)maxZ - (long)minZ;
	long lhalfZ = ldiffZ / 2 - 1;

	Geometry[] ga = new Geometry[4];
	ga[0] = new Geometry(minX, (int)(minX+lhalfX),
			     minZ, (int)(minZ+lhalfZ));
	ga[1] = new Geometry((int)(minX+lhalfX+1), maxX,
			     minZ, (int)(minZ+lhalfZ));
	ga[2] = new Geometry(minX, (int)(minX+lhalfX),
			     (int)(minZ+lhalfZ+1), maxZ);
	ga[3] = new Geometry((int)(minX+lhalfX+1), maxX,
			     (int)(minZ+lhalfZ+1), maxZ);
	return ga;
    }

    public boolean isAdjacent(Geometry gOther) {
	return ((gOther.getMinX() == getMaxX()+1) ||
		(gOther.getMaxX() == getMinX()-1) ||
		(gOther.getMaxZ() == getMinZ()-1) ||
		(gOther.getMinZ() == getMaxZ()+1));
    }

    public static Geometry maxGeometry() {
	return new Geometry(0x80000000,
			    0x7fffffff,
			    0x80000000,
			    0x7fffffff);
    }

    private void readObject(ObjectInputStream in)
            throws IOException, ClassNotFoundException {
        in.defaultReadObject();
    }
    
    private void writeObject(ObjectOutputStream out)
	throws IOException, ClassNotFoundException {
        out.defaultWriteObject();
    }
    
    int minX = 0;
    int maxX = 0;
    int minZ = 0;
    int maxZ = 0;

    private static final long serialVersionUID = 1L;
}
