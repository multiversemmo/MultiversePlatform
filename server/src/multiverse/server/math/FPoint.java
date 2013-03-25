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
public class FPoint implements Cloneable, Externalizable {
    public FPoint() {}
    public FPoint(float x, float y, float z) { 
	_x = x;
	_y = y;
	_z = z;
    }
    
    public FPoint(Point source) {
        _x = (float)source.getX();
        _y = (float)source.getY();
        _z = (float)source.getZ();
    }

    public void assign(FPoint source) {
        _x = source.getX();
        _y = source.getY();
        _z = source.getZ();
    }

    public Object clone() {
	FPoint o = new FPoint(_x, _y, _z);
	return o;
    }
    
    public FPoint cloneFPoint() {
	FPoint o = new FPoint(_x, _y, _z);
	return o;
    }
    
    public void add(FPoint other) {
	_x += other.getX();
	_y += other.getY();
	_z += other.getZ();
    }

    public void add(Point other) {
	_x += other.getX();
	_y += other.getY();
	_z += other.getZ();
    }

    public void add(float x, float y, float z) {
	_x += x;
	_y += y;
	_z += z;
    }

    public void sub(FPoint other) {
	_x -= other.getX();
	_y -= other.getY();
	_z -= other.getZ();
    }

    public void sub(Point other) {
	_x -= other.getX();
	_y -= other.getY();
	_z -= other.getZ();
    }

    public void sub(float x, float y, float z) {
	_x -= x;
	_y -= y;
	_z -= z;
    }

    public void multiply(float factor) {
	_x *= factor;
	_y *= factor;
	_z *= factor;
    }

    public FPoint plus(FPoint other) {
        FPoint p = cloneFPoint();
        p.add(other);
        return p;
    }
    
    public FPoint minus(FPoint other) {
        FPoint p = cloneFPoint();
        p.sub(other);
        return p;
    }

    public FPoint times(float factor) {
        FPoint p = cloneFPoint();
        p.multiply(factor);
        return p;
    }
    
    public FPoint negate() {
	return new FPoint(- _x, - _y, - _z);
    }

    public float dot(FPoint other) {
        return _x * other.getX() + _y * other.getY() + _z * other.getZ();
    }

    public FPoint cross(FPoint vector) {
        return new FPoint(
            (_y * vector.getZ()) - (_z * vector.getY()),
            (_z * vector.getX()) - (_x * vector.getZ()),
            (_x * vector.getY()) - (_y * vector.getX()));
    }

    public String toString() {
	return "(" + getX() + "," + getY() + "," +getZ() + ")";
    }

    public static FPoint parsePoint(String s) {
        String v = s.trim();
        FPoint p = new FPoint();
        if (v.startsWith("(") && v.endsWith(")")) {
            String[] parts = v.substring(1, v.length() - 1).split(",");
            int n = parts.length;
            if (n >= 1)
                p.setX(Float.parseFloat(parts[0]));
            if (n >= 2)
                p.setY(Float.parseFloat(parts[1]));
            if (n >= 3)
                p.setZ(Float.parseFloat(parts[2]));
        }
        return p;
    }
    
    public float getX() { return _x; }
    public float getY() { return _y; }
    public float getZ() { return _z; }

    public void setX(float x) { _x = x; }
    public void setY(float y) { _y = y; }
    public void setZ(float z) { _z = z; }

    /*
     * serializes this object for either storing into a database
     * or sending this object to another server
     * 
     * @see java.io.Externalizable
     */
    public void writeExternal(ObjectOutput out) throws IOException {
	out.writeFloat(_x);
	out.writeFloat(_y);
	out.writeFloat(_z);
    }

    /*
     * deserializes this object.  usually to read in from a database
     * or from another server sending us an object
     *
     * @see java.io.Externalizable
     */
    public void readExternal(ObjectInput in)
	throws IOException, ClassNotFoundException {
	_x = in.readFloat();
	_y = in.readFloat();
	_z = in.readFloat();
    }

    /**
     * returns distance from p1 to p2 on the XZ plane
     */
    public static float distanceTo(FPoint p1, FPoint p2) {
	float dist = 
	    (float) Math.sqrt(Math.pow(p2.getX() - p1.getX(), 2) +
			      Math.pow(p2.getZ() - p1.getZ(), 2));
	return dist;
    }

    public FPoint toNormalized() {
        FPoint p = cloneFPoint();
        p.normalize();
        return p;
    }

    public float normalize() {
        float length = (float)Math.sqrt(_x * _x + _y * _y + _z * _z);
        // Will also work for zero-sized vectors, but will change nothing
        if (length > 1.0e-5f) {
            float inverseLength = 1.0f / length;
            _x *= inverseLength;
            _y *= inverseLength;
            _z *= inverseLength;
        }
        return length;
    }

    private float _x = 0;
    private float _y = 0;
    private float _z = 0;

    private static final long serialVersionUID = 1L;
}
