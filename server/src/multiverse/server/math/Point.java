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

/**
 * this object is not thread safe
 */
public class Point implements Cloneable, Externalizable {
    public Point() {}
    public Point(int x, int y, int z) { 
	_x = x;
	_y = y;
	_z = z;
    }
    
    public Point(MVVector p) {
        _x = (int)p.getX();
        _y = (int)p.getY();
        _z = (int)p.getZ();
    }

    public Object clone() {
	Point o = new Point(_x, _y, _z);
	return o;
    }
    
    public boolean equals(Object obj)
    {
        Point other = (Point) obj;
        return _x == other._x && _y == other._y && _z == other._z;
    }

    public void add(int x, int y, int z) {
	_x += x;
	_y += y;
	_z += z;
    }

    public void add(Point other) {
	_x += other.getX();
        _y += other.getY();
	_z += other.getZ();
    }

    public void sub(Point other) {
	_x -= other.getX();
        _y -= other.getY();
	_z -= other.getZ();
    }

    public void negate() {
	_x = - _x;
        _y = - _y;
	_z = - _z;
    }

    public void multiply(float factor) {
	_x = (int)(_x * factor);
	_y = (int)(_y * factor);
	_z = (int)(_z * factor);
    }

    public String toString() {
	return "(" + getX() + "," + getY() + "," +getZ() + ")";
    }

    public static Point parsePoint(String s) {
        String v = s.trim();
        Point p = new Point();
        if (v.startsWith("(") && v.endsWith(")")) {
            String[] parts = v.substring(1, v.length() - 1).split(",");
            int n = parts.length;
            if (n >= 1)
                p.setX((int)Float.parseFloat(parts[0]));
            if (n >= 2)
                p.setY((int)Float.parseFloat(parts[1]));
            if (n >= 3)
                p.setZ((int)Float.parseFloat(parts[2]));
        }
        return p;
    }
    
    public int getX() { return _x; }
    public int getY() { return _y; }
    public int getZ() { return _z; }

    public void setX(int x) { _x = x; }
    public void setY(int y) { _y = y; }
    public void setZ(int z) { _z = z; }

    /*
     * serializes this object for either storing into a database
     * or sending this object to another server, as in the case when
     * a mob zones into another server
     * 
     * @see java.io.Externalizable
     */
    public void writeExternal(ObjectOutput out) throws IOException {
	out.writeInt(_x);
	out.writeInt(_y);
	out.writeInt(_z);
    }

    /*
     * deserializes this object.  usually to read in from a database
     * or from another server sending us an object
     *
     * @see java.io.Externalizable
     */
    public void readExternal(ObjectInput in)
	throws IOException, ClassNotFoundException {
	_x = in.readInt();
	_y = in.readInt();
	_z = in.readInt();
    }

    /**
     * returns distance from p1 to p2 on the XZ plane
     */
    public static float distanceTo(Point p1, Point p2) {
	float dist = 
	    (float) Math.sqrt(Math.pow(p2.getX() - p1.getX(), 2) +
			      Math.pow(p2.getZ() - p1.getZ(), 2));
	return dist;
    }

    /**
     * returns the square of the distance from p1 to p2 on the XZ plane
     */
    public static float distanceToSquared(Point p1, Point p2) {
	float distSquared = 
	    (float)(Math.pow(p2.getX() - p1.getX(), 2) + Math.pow(p2.getZ() - p1.getZ(), 2));
	return distSquared;
    }

    private int _x = 0;
    private int _y = 0;
    private int _z = 0;

    private static final long serialVersionUID = 1L;
}
