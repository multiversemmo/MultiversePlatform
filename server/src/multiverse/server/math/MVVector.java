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

import java.lang.Math;
import java.io.*;
import multiverse.server.util.*;

/**
 * this object is not thread safe
 */
public class MVVector implements Externalizable {
    public MVVector() {}
    public MVVector(float x, float y, float z) { 
	_x = x;
	_y = y;
	_z = z;
    }
    
    public MVVector(Point other) {
        _x = other.getX();
        _y = other.getY();
        _z = other.getZ();
    }

    public Object clone() {
	MVVector o = new MVVector(getX(), getY(), getZ());
	return o;
    }
    
    public MVVector cloneMVVector() {
	MVVector o = new MVVector(getX(), getY(), getZ());
	return o;
    }
    
    public boolean equals(Object obj)
    {
        MVVector other = (MVVector) obj;
        return _x == other._x && _y == other._y && _z == other._z;
    }

    public void assign(MVVector source) {
        _x = source.getX();
        _y = source.getY();
        _z = source.getZ();
    }

    /**
     * returns distance from p1 to p2 on the XZ plane
     */
    public static float distanceTo(MVVector p1, MVVector p2) {
        float dx = p2.getX() - p1.getX();
        float dz = p2.getZ() - p1.getZ();
	return (float)Math.sqrt(dx * dx + dz * dz);
    }
    
    /**
     * returns the square of the distance from p1 to p2 on the XZ plane
     */
    public static float distanceToSquared(MVVector p1, MVVector p2) {
        float dx = p2.getX() - p1.getX();
        float dz = p2.getZ() - p1.getZ();
        return dx * dx + dz * dz;
    }

    // is this vector a zero vector (no length)
    public boolean isZero() {
	return ((_x == 0) && (_y == 0) && (_z == 0));
    }

    
    // normalizes THIS vector
    public MVVector normalize() {
        float len = length();
        // Only scale if we're not going to create an NaN
        if (len > epsilon)
            scale(1 / len);
	return this;
    }

    public static final MVVector UnitZ = new MVVector(0,0,1);
    
    public MVVector add(MVVector other) {
	_x += other.getX();
	_y += other.getY();
	_z += other.getZ();
        return this;
    }

    public MVVector add(Point other) {
	_x += other.getX();
	_y += other.getY();
	_z += other.getZ();
        return this;
    }

    public MVVector add(float x, float y, float z) {
	_x += x;
	_y += y;
	_z += z;
        return this;
    }

    public MVVector sub(MVVector other) {
	_x -= other.getX();
	_y -= other.getY();
	_z -= other.getZ();
        return this;
    }

    public MVVector sub(Point other) {
	_x -= other.getX();
	_y -= other.getY();
	_z -= other.getZ();
        return this;
    }

    public MVVector sub(float x, float y, float z) {
	_x -= x;
	_y -= y;
	_z -= z;
        return this;
    }

    public MVVector multiply(float factor) {
	_x *= factor;
	_y *= factor;
	_z *= factor;
        return this;
    }

    public MVVector plus(MVVector other) {
        MVVector p = (MVVector)clone();
        p.add(other);
        return p;
    }
    
    public MVVector minus(MVVector other) {
        MVVector p = (MVVector)clone();
        p.sub(other);
        return p;
    }

    public MVVector times(float factor) {
        MVVector p = (MVVector)clone();
        p.multiply(factor);
        return p;
    }
    
    public MVVector negate() {
	return new MVVector(- _x, - _y, - _z);
    }

    // TODO - optimize so when you compute length, you can store it
    // returns the length of the vector
    public float length() {
	return (float) Math.sqrt(getX() * getX() +
				 getY() * getY() +
				 getZ() * getZ());
    }

    // The length ignoring the y coordinate
    public float lengthXZ() {
	return (float) Math.sqrt(getX() * getX() +
				 getZ() * getZ());
    }

    // returns a new vector
    public float dotProduct(MVVector v) {
	return getX() * v.getX() + getY() * v.getY() + getZ() * v.getZ();
    }

    // manipulates THIS vector.  scalar-multiplication
    public MVVector scale(float s) {
	_x *= s;
	_y *= s;
	_z *= s;
	return this;
    }

    /**
     * Gets the shortest arc quaternion to rotate this vector 
     * to the destination vector.
     * Don't call this if you think the dest vector can be close to the inverse
     * of this vector, since then ANY axis of rotation is ok.
     */
    public Quaternion getRotationTo(MVVector destination)
    {
            // Based on Stan Melax's article in Game Programming Gems
            Quaternion q = new Quaternion();

            MVVector v0 = new MVVector(this._x, this._y, this._z);
            MVVector v1 = destination;

            // normalize both vectors
            v0.normalize();
            v1.normalize();

            // get the cross product of the vectors
            MVVector c = MVVector.cross(v0, v1);

            // If the cross product approaches zero, we get unstable because 
            // ANY axis will do
            // when v0 == -v1
            float d = v0.dotProduct(v1);

            // If dot == 1, vectors are the same
            if (d >= 1.0f)
            {
                return Quaternion.Identity;
            }
            if (Log.loggingDebug)
                Log.debug("MVVector.getRotationTo: d=" + d);
            if (d < -0.99f) {
                // close to inverse
                return null;
            }
            float s = (float) Math.sqrt( (1+d) * 2 );
            float inverse = 1 / s;

            q.setX(c.getX() * inverse);
            q.setY(c.getY() * inverse);
            q.setZ(c.getZ() * inverse);
            q.setW(s * 0.5f);

            return q;
    }

    public String toString() {
	return "[x=" + getX() + ",y=" + getY() + ",z=" +getZ() + "]";
    }

    public float getX() { return _x; }
    public float getY() { return _y; }
    public float getZ() { return _z; }

    public void setX(float x) { _x = x; }
    public void setY(float y) { _y = y; }
    public void setZ(float z) { _z = z; }


    // dest - cur
    public static MVVector sub(Point dest, Point cur) {
	return new MVVector(dest.getX() - cur.getX(),
			    dest.getY() - cur.getY(),
			    dest.getZ() - cur.getZ());
    }

    /*
     * serializes this object for either storing into a database
     * or sending this object to another server, as in the case when
     * a mob zones into another server
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

    public static MVVector parsePoint(String s) {
        String v = s.trim();
        MVVector p = new MVVector();
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
    
    private float _x = 0;
    private float _y = 0;
    private float _z = 0;

    public static float epsilon = 1.0e-3f;

	// Additional methods

    public MVVector(MVVector other) { 
		_x = other._x;
		_y = other._y;
		_z = other._z;
    }

   // TODO: Check that this orientation is correct (left / right handed)
    public static MVVector cross(MVVector u, MVVector v) {
            float x = u._y * v._z - u._z * v._y;
            float y = u._z * v._x - u._x * v._z;
            float z = u._x * v._y - u._y * v._x;
            return new MVVector(x, y, z);
    }

    public static boolean counterClockwisePoints(MVVector v0, MVVector v1, MVVector v2) {
        return (v1._x - v0._x) * (v2._z - v0._z) - (v2._x - v0._x) * (v1._z - v0._z) > 0;
    }

    public static void main(String args[]) {
	// length should be 5
	// normal should be 4/5, 3/5 0
	MVVector v = new MVVector(4, 3, 0);
	float len = v.length();
	System.out.println("length of " + v + " should be 5.. result=" + len);

	v.normalize();
	System.out.println("normal should be 0.8 0.6 0 - result is " + v);
    }

    private static final long serialVersionUID = 1L;
}
