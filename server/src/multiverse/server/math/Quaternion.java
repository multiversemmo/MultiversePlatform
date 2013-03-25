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
public class Quaternion implements Externalizable, Cloneable {
    
    /**
     * no args constructor sets w to 1.0, all others 0
     */
    public Quaternion() {
        _w = 1.0f;
    }

    public Quaternion(float x, float y, float z, float w) {
        _x = x;
        _y = y;
        _z = z;
        _w = w;
    }

    public String toString() {
        return "(" + getX() + "," + getY() + "," + getZ() + "," + getW() + ")";
    }

    public static Quaternion parseQuaternion(String s) {
        String v = s.trim();
        Quaternion q = new Quaternion();
        if (v.startsWith("(") && v.endsWith(")")) {
            String[] parts = v.substring(1, v.length() - 2).split(",");
            int n = parts.length;
            if (n >= 1)
                q.setX((int) Float.parseFloat(parts[0]));
            if (n >= 2)
                q.setY((int) Float.parseFloat(parts[1]));
            if (n >= 3)
                q.setZ((int) Float.parseFloat(parts[2]));
            if (n >= 4)
                q.setW((int) Float.parseFloat(parts[3]));
        }
        return q;
    }

    public Object clone() {
        return new Quaternion(_x, _y, _z, _w);
    }

    public boolean equals(Object obj)
    {
        return equals((Quaternion) obj);
    }

    public boolean equals(Quaternion q) {
        return (_x == q._x) && (_y == q._y) && (_z == q._z) && (_w == q._w);
    }

    public float getX() {
        return _x;
    }

    public float getY() {
        return _y;
    }

    public float getZ() {
        return _z;
    }

    public float getW() {
        return _w;
    }

    public void setX(float x) {
        _x = x;
    }

    public void setY(float y) {
        _y = y;
    }

    public void setZ(float z) {
        _z = z;
    }

    public void setW(float w) {
        _w = w;
    }

    private float _x = 0;

    private float _y = 0;

    private float _z = 0;

    private float _w = 0;

    /**
     * Create a quaternion from a supplied angle and axis
     *
     * @param angle angle in radians
     * @param axis axis vector about which to rotate
     *
     * @return a quaternion based on the axis and angle
     */
    public static Quaternion fromAngleAxis(double angle, MVVector axis) {
        Quaternion quat = new Quaternion();
        double halfAngle = 0.5f * angle;
        float cos = (float) Math.cos(halfAngle);
        float sin = (float) Math.sin(halfAngle);
        MVVector normAxis = new MVVector(axis).normalize();
        quat._w = cos;
        quat._x = sin * normAxis.getX();
        quat._y = sin * normAxis.getY();
        quat._z = sin * normAxis.getZ();
        return quat;
    }

    /**
     * Create a quaternion from a supplied angle and axis
     *
     * @param angle angle in degrees
     * @param axis axis vector about which to rotate
     *
     * @return a quaternion based on the axis and angle
     */
    public static Quaternion fromAngleAxisDegrees(double angle, MVVector axis) {
        return Quaternion.fromAngleAxis(Math.toRadians(angle), axis);
    }

    /**
     * Get the axis of rotation and the angle for this quaternion
     *
     * @param axis reference to axis vector which will be populated with
     *             axis of rotation.
     *
     * @return angle in degrees
     */
    public double getAngleAxisDegrees(MVVector axis) {
        double angle = this.getAngleAxis(axis);
        return Math.toDegrees(angle);
    }

    public static final Quaternion Identity = new Quaternion();
    
    /**
     * Get the axis of rotation and the angle for this quaternion
     *
     * @param axis reference to axis vector which will be populated with
     *             axis of rotation.
     *
     * @return angle in degrees
     */
    public double getAngleAxis(MVVector axis) {
        // The quaternion representing the rotation is
        //   q = cos(A/2)+sin(A/2)*(x*i+y*j+z*k)
        float len = (float) Math.sqrt(_x * _x + _y * _y + _z * _z);
        double angle;
        if (len > 0) {
            angle = 2.0f * Math.acos(_w);
            axis.setX(_x / len);
            axis.setY(_y / len);
            axis.setZ(_z / len);
        } else {
            // essentially the null quaternion
            angle = 0.0f;
            axis.setX(1);
            axis.setY(0);
            axis.setZ(0);
        }
        return angle;
    }

    public static float epsilon = 1.0e-3f;
    
    /**
     * Create a new quaternion that will rotate vector a into vector b about
     * their mutually perpendicular axis.
     * 
     * @param a
     *            starting facing
     * @param b
     *            ending facing
     */
    public static Quaternion fromVectorRotation(MVVector a, MVVector b) {
        MVVector aDir = new MVVector(a).normalize();
        MVVector bDir = new MVVector(b).normalize();
        MVVector cross = MVVector.cross(aDir, bDir);
        float crossLen = cross.length();
        // Get theta in the range of -PI/2 to PI/2
        double theta = Math.asin(crossLen);
        // Use the dot product to determine theta in the full range
        float dot = aDir.dotProduct(bDir);
        if (dot < 0)
            theta = Math.PI - theta;

        if (crossLen < epsilon)
            return Identity;
        else
            return Quaternion.fromAngleAxis(theta, cross.scale(1 / crossLen));
    }

    /**
     * Multiply two quaternions
     * 
     * @param left
     *            the quaternion on the left
     * @param right
     *            the quaternion on the right
     * @return the product of left * right
     */
    public static Quaternion multiply(Quaternion left, Quaternion right) {
        Quaternion q = new Quaternion();

        q._w = left._w * right._w - left._x * right._x - left._y * right._y
                - left._z * right._z;
        q._x = left._w * right._x + left._x * right._w + left._y * right._z
                - left._z * right._y;
        q._y = left._w * right._y + left._y * right._w + left._z * right._x
                - left._x * right._z;
        q._z = left._w * right._z + left._z * right._w + left._x * right._y
                - left._y * right._x;
        return q;
    }

    /*
     * Return an MVVector rotated by the quaternion
     */
    public static MVVector multiply(Quaternion quat, MVVector vector) {
        // nVidia SDK implementation
        MVVector qvec = new MVVector(quat._x, quat._y, quat._z);
        MVVector uv = MVVector.cross(qvec, vector); 
        MVVector uuv = MVVector.cross(qvec, uv); 
        uv = uv.times(2.0f * quat._w); 
        uuv = uuv.times(2.0f);
        return vector.plus(uv.plus(uuv));
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
        out.writeFloat(_w);
    }

    /*
     * deserializes this object.  usually to read in from a database
     * or from another server sending us an object
     *
     * @see java.io.Externalizable
     */
    public void readExternal(ObjectInput in) throws IOException,
            ClassNotFoundException {
        _x = in.readFloat();
        _y = in.readFloat();
        _z = in.readFloat();
        _w = in.readFloat();
    }

    private static final long serialVersionUID = 1L;
}
