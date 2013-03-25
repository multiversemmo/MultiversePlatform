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
import java.util.*;
import multiverse.server.math.*;
import multiverse.server.util.*;

/**
 * used in pathing code
 * works with java bean xml serialization
 */
public class PathInfo implements Serializable, Cloneable {
    public PathInfo(Map<String, PathObjectType> typeDictionary,
                    Map<String, PathData> pathDictionary) {
        this.typeDictionary = typeDictionary;
        this.pathDictionary = pathDictionary;
    }
    
    public PathInfo() {
        this.typeDictionary = new HashMap<String, PathObjectType>();
        this.pathDictionary = new HashMap<String, PathData>();
    }

    public void performPathingTest(Geometry geometry) {
        performPathingTest1(geometry);
        performPathingTest2(geometry);
        performPathingTest3(geometry);
        performPathingTest4(geometry);
    }
    
    protected void performPathingTest1(Geometry geometry) {
        log.debug("PATHING TEST 1");
        // Create the path from the center of meetinghouse1 to the
        // center of meetinghouse2.  We just happen to know that the
        // center of the meetinghouse is the center of the cvPolygon
        // with index 4.
        String type = "Generic";
        MVVector loc1 = getCenterOfPolygon(type, "meetinghouse1", 6);
        MVVector loc2 = getCenterOfPolygon(type, "meetinghouse2", 7);
        PathFinderValue value = performSearch(type, geometry, loc1, loc2);
        showTestResult(value, loc1, loc2);
    }
    
    protected void performPathingTest2(Geometry geometry) {
        log.debug("PATHING TEST 2");
        // Create the path from the one side of meetinghouse2 to the
        // center of meetinghouse2
        MVVector loc1 = new MVVector(-146466f, 25908f, -302033f);
        String type = "Generic";
        MVVector loc2 = getCenterOfPolygon(type, "meetinghouse2", 7);
        PathFinderValue value = performSearch(type, geometry, loc1, loc2);
        showTestResult(value, loc1, loc2);
    }

    protected void performPathingTest3(Geometry geometry) {
        log.debug("PATHING TEST 3");
        // Create the path from the one side of meetinghouse2 to the
        // center of meetinghouse2
//        MVVector loc1 = new MVVector(-133458, 25715, -308292);
        MVVector loc1 = new MVVector(-123465f, 27281f, -303274f);
        String type = "Generic";
        MVVector loc2 = getCenterOfPolygon(type, "meetinghouse2", 7);
        PathFinderValue value = performSearch(type, geometry, loc1, loc2);
        showTestResult(value, loc1, loc2);
    }

    protected void performPathingTest4(Geometry geometry) {
        log.debug("PATHING TEST 4");
        // Create the path from the one side of meetinghouse2 to the
        // center of meetinghouse2
        MVVector loc1 = new MVVector(-123465f, 27281f, -303274f);
        MVVector loc2 = new MVVector(-136465f, 27597f, -214821f);
        String type = "Generic";
        PathFinderValue value = performSearch(type, geometry, loc1, loc2);
        showTestResult(value, loc1, loc2);
    }

    protected MVVector getCenterOfPolygon(String type, String modelName, int polygonIndex) {
        if (Log.loggingDebug)
            log.debug("Getting PathData for model " + modelName);
        PathData pd = pathDictionary.get(modelName);
        PathObject po = pd.getPathObjectForType(type);
        PathPolygon cv = po.getCVPolygon(polygonIndex);
        MVVector loc = cv.getCentroid();
        if (Log.loggingDebug)
            log.debug(modelName + " polygon " + polygonIndex + " centroid is " + loc);
        return loc;
    }

    protected PathFinderValue performSearch(String type, Geometry geometry, MVVector loc1, MVVector loc2) {
        log.debug("Creating PathSearcher");
        PathSearcher.createPathSearcher(this, geometry);
        log.debug("Calling PathSearcher.findPath");
        return PathSearcher.findPath(type, loc1, loc2, true);
    }

    protected void showTestResult(PathFinderValue value, MVVector loc1, MVVector loc2) {
        if (Log.loggingDebug) {
            log.debug("Plotting path from " + loc1 + " to " + loc2 + ", PathResult was " + value.getResult().toString());
            log.debug("Calculated path is " + value.stringPath(0));
            log.debug("TerrainString is '" + value.getTerrainString() + "'");
        }
    }

    public float getTypeHalfWidth(String type) {
        if (typeDictionary.containsKey(type))
            return typeDictionary.get(type).getWidth() * 1000f;
        else {
            log.error("In getTypeHalfWidth, can't find path object type '" + type + "'!");
            return 100.0f;
        }
    }
    
    public Object clone() {
        return new PathInfo(typeDictionary, pathDictionary);
    }
    
    public boolean pathObjectTypeSupported(String type) {
        return typeDictionary.containsKey(type);
    }

    public void setTypeDictionary(Map<String, PathObjectType> typeDictionary) {
        this.typeDictionary = typeDictionary;
    }
    
    public Map<String, PathObjectType> getTypeDictionary() {
        return typeDictionary;
    }
    
    public void setPathDictionary(Map<String, PathData> pathDictionary) {
        this.pathDictionary = pathDictionary;
    }

    public Map<String, PathData> getPathDictionary() {
        return pathDictionary;
    }

    private Map<String, PathObjectType> typeDictionary;    
    private Map<String, PathData> pathDictionary;

    protected static final Logger log = new Logger("PathInfo");

    private static final long serialVersionUID = 1L;
}

    
