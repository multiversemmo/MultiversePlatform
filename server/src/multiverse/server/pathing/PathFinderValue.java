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

import java.util.*;
import multiverse.server.math.*;

// Curse java for making me define a class just because I want to have
// multiple return values

public class PathFinderValue {
    
    public PathFinderValue(PathSearcher.PathResult result, List<MVVector> path, String terrainString) {
        this.result = result;
        this.path = path;
        this.terrainString = terrainString;
    }
    
    public PathSearcher.PathResult getResult() {
        return result;
    }

    public void setResult(PathSearcher.PathResult result) {
        this.result = result;
    }

    public List<MVVector> getPath() {
        return path;
    }

    public String getTerrainString() {
        return terrainString;
    }

    public void setTerrainString(String terrainString) {
        this.terrainString = terrainString;
    }

    public void addTerrainChar(char ch) {
    }

    public void addPathElement(MVVector loc) {
        assert path.size() == 0;
        path.add(loc);
    }

    public int pathElementCount() {
        return path.size();
    }

    public void addPathElement(MVVector loc, boolean overTerrain) {
        assert path.size() == terrainString.length();
        path.add(loc);
        terrainString += (overTerrain ? 'T' : 'C');
    }

    public void removePathElementsAfter (int pathSize) {
        for (int i = path.size() - 1; i>= pathSize; i--)
            path.remove(i);
        terrainString = terrainString.substring(0, pathSize);
    }

    String stringPath(int firstElt) {
        String s = "";
        for (int i=firstElt; i<path.size(); i++) {
            MVVector p = path.get(i);
            if (s.length() > 0)
                s += ", ";
            s += "#" + i + ": " + terrainString.charAt(i) + p;
        }
        return s;
    }

    protected PathSearcher.PathResult result;
    protected List<MVVector> path;
    protected String terrainString;
}

    
