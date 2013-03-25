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

import multiverse.server.util.*;
import java.io.*;

public class Color implements Cloneable, Externalizable {
    public Color() {
    }

    public Color(int r, int g, int b) {
	setRed(r);
	setGreen(g);
	setBlue(b);
	setAlpha(255);
    }

    public Color(int r, int g, int b, int a) {
	setRed(r);
	setGreen(g);
	setBlue(b);
	setAlpha(a);
    }

    /**
     * string representation in the format:
     * (red,green,blue,alpha)
     * example: "(35,103,64,0)"
     */
    public String toString() {
	return "(" + r +
	    "," + g +
	    "," + b +
	    "," + a +
	    ")";
    }

    public boolean equals(Object other) {
    	Color otherColor = (Color) other;
        if (other == null) {
            return false;
        }
    	return ((this.getRed() == otherColor.getRed()) &&
    			(this.getGreen() == otherColor.getGreen()) &&
    			(this.getBlue() == otherColor.getBlue()) &&
    			(this.getAlpha() == otherColor.getAlpha()));
    }
    
    public Object clone() {
        return new Color(r, g, b, a);
    }

    public byte[] toBytes() {
        if (Log.loggingDebug)
            Log.debug("color.toBytes: " + this.toString());
	byte[] colorBytes = new byte[4];
	colorBytes[0] = (byte) getAlpha();
	colorBytes[1] = (byte) getBlue();
	colorBytes[2] = (byte) getGreen();
	colorBytes[3] = (byte) getRed();
	return colorBytes;
    }

    public void setRed(int val) {
	assertRange(val);
	this.r = val;
    }
    public int getRed() {
	return r;
    }
    
    public void setGreen(int val) {
	assertRange(val);
	this.g = val;
    }
    public int getGreen() {
	return g;
    }
    
    public void setBlue(int val) {
	assertRange(val);
	this.b = val;
    }
    public int getBlue() {
	return b;
    }
    
    public void setAlpha(int val) {
	assertRange(val);
	this.a = val;
    }
    public int getAlpha() {
	return a;
    }

    void assertRange(int val) {
	if ((val < 0) || (val > 255)) {
	    throw new RuntimeException("color: color value is out of range: " + 
                                       val);
	}
    }

    /**
     * serializes this object for either storing into a database
     * or sending this object to another server, as in the case when
     * a mob zones into another server
     * <p>
     * note that the first time the user is being serialized, the dbid
     * wont be set.  so whatever method deserializes should set it
     * explicitly from the loading reference
     * 
     * @see java.io.Externalizable
     */
    public void writeExternal(ObjectOutput out) throws IOException
    {
        if (Log.loggingTrace)
            Log.trace("Color.writeExternal: writing out color: " + this);
        out.writeInt(getRed());
        out.writeInt(getGreen());
        out.writeInt(getBlue());
        out.writeInt(getAlpha());
    }

    /**
     * deserializes this object.  usually to read in from a database
     * or from another server sending us an object
     *
     * @see java.io.Externalizable
     */
    public void readExternal(ObjectInput in)
	throws IOException, ClassNotFoundException
    {
        setRed(in.readInt());
        setGreen(in.readInt());
        setBlue(in.readInt());
        setAlpha(in.readInt());
    }

    int r = 0;
    int g = 0;
    int b = 0;
    int a = 0;

    public static Color White = new Color(255,255,255,255);
    public static Color Black = new Color(0,0,0,255);
    public static Color Red = new Color(255,0,0,255);
    private static final long serialVersionUID = 1L;
}
