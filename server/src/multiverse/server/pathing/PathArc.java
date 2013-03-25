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

import java.io.*;

/**
 * used in pathing code
 * works with java bean xml serialization
 */
public class PathArc implements Serializable, Cloneable {

    public PathArc() {
    }

    public PathArc(byte arcKind, int poly1Index, int poly2Index, PathEdge edge) {
        this.arcKind = arcKind;
        this.poly1Index = poly1Index;
        this.poly2Index = poly2Index;
        this.edge = edge;
    }
    
    public static final byte Illegal = (byte)0;
    public static final byte CVToCV = (byte)1;
    public static final byte TerrainToTerrain = (byte)2;
    public static final byte CVToTerrain = (byte)3;
    
    public String formatArcKind(byte val) {
        switch (val) {
        case 0:
            return "Illegal";
        case 1:
            return "CVToCV";
        case 2:
            return "TerrainToTerrain";
        case 3:
            return "CVToTerrain";
        default:
            return "Unknown ArcKind " + val;
        }
    }
            
    public static byte parseArcKind(String s) {
        if (s.equals("Illegal"))
            return Illegal;
        else if (s.equals("CVToCV"))
            return CVToCV;
        else if (s.equals("TerrainToTerrain"))
            return TerrainToTerrain;
        else if (s.equals("CVToTerrain"))
            return CVToTerrain;
        else
            return Illegal;
    }

    public String toString() {
        return "[PathArc kind=" + formatArcKind(arcKind) + ",poly1Index=" + getPoly1Index() + 
            ",poly2Index=" + getPoly2Index() + ",edge=" + getEdge() + "]";
    }

    public String shortString() {
        return getPoly1Index() + ":" + getPoly2Index();
    }

    public Object clone() {
        return new PathArc(getKind(), getPoly1Index(), getPoly2Index(), getEdge());
    }

    public byte getKind() {
    	return arcKind;
    }
    
    public int getPoly1Index() {
        return poly1Index;
    }
    
    public void setPoly1Index(int poly1Index) {
        this.poly1Index = poly1Index;
    }
    
    public int getPoly2Index() {
        return poly2Index;
    }
    
    public void setPoly2Index(int poly2Index) {
        this.poly2Index = poly2Index;
    }
    
    public PathEdge getEdge() {
        return edge;
    }

    int poly1Index;
    int poly2Index;
    byte arcKind;
    PathEdge edge;
    private static final long serialVersionUID = 1L;
}

