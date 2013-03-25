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
import multiverse.server.util.*;

// triangulate_impl.h	-- Thatcher Ulrich 2004

// This source code has been donated to the Public Domain.  Do
// whatever you want with it.

// Code to triangulate arbitrary 2D polygonal regions.
//
// Use the basic robust ear-clipping algorithm from "FIST: Fast
// Industrial-Strength Triangulation of Polygons" by Martin Held.
//
// NOTE: This code is based on the algorithm described in the FIST
// paper, but this is NOT the official FIST code written by Martin
// Held!  This code may not be as robust or as fast as FIST, and is
// certainly not as well tested.  Also, it deviates in some places
// from the algorithm as described in the FIST paper; I have tried to
// document those places in the code, along with my reasoning, but
// this code is not warranted in any way.
//
// In particular, the recovery_process is currently not as good as
// official FIST or even what's in the FIST paper.  This routine may
// do some ugly stuff with self-intersecting input.
//
// For information on obtaining the offical industrial-strength FIST
// code, see the FIST web page at:
// http://www.cosy.sbg.ac.at/~held/projects/triang/triang.html

public class Triangulate {

    protected static final Logger log = new Logger("Triangulate");
    
    static boolean debugProfileTriangulate = true;
    
    public List<PathPolygon> computeTriangulation(String description, PathPolygon boundary, List<PathPolygon> obstacles) {
        log.info(description + " boundary: " + boundary);
        dumpPolygons(description + " obstacles:", obstacles);
        ArrayList<Float> result = new ArrayList<Float>();
        int pathCount = obstacles.size() + 1;
        ArrayList<float[]> paths = new ArrayList<float[]>();
        List<PathPolygon> allPaths = new LinkedList<PathPolygon>();
        allPaths.add(boundary);
        allPaths.addAll(obstacles);
        for (PathPolygon p : allPaths) {
            List<MVVector> corners = p.getCorners();
            float[] path = new float[corners.size() * 2];
            paths.add(path);
            int i=0;
            for (MVVector vert : corners) {
                path[i++] = vert.getX();
                path[i++] = vert.getZ();
            }
        }
        PolyEnv env = new PolyEnv();
        env.computeTriangulation(result, pathCount, paths, 0, null);
        // Each resulting chunk is a triangle - - 6 floats make up the
        // 3 vertices
        int polyIndex = 1;
        List<PathPolygon> resultPolys = new LinkedList<PathPolygon>();
        for (int i=0; i<result.size(); i+=6) {
            List<MVVector> corners = new ArrayList<MVVector>(3);
            corners.add(new MVVector(result.get(i), 0f, result.get(i + 1)));
            corners.add(new MVVector(result.get(i + 2), 0f, result.get(i + 3)));
            corners.add(new MVVector(result.get(i + 4), 0f, result.get(i + 5)));
            resultPolys.add(new PathPolygon(polyIndex++, PathPolygon.CV, corners));
        }
        dumpPolygons(description + " results:", resultPolys);
        return resultPolys;
    }
    
    static void dumpPolygons(String description, List<PathPolygon> polygons) {
        log.info(description + ": " + polygons.size() +  " polygons");
        for (PathPolygon poly : polygons)
            log.info(description + ": Polygon " + poly);
    }
    
    static double determinantFloat(MVVector a, MVVector b, MVVector c) {
        double fact11 = (double)b.getX() - (double)a.getX();
        double fact12 = (double)c.getZ() - (double)a.getZ();
        double fact21 = (double)b.getZ() - (double)a.getZ();
        double fact22 = (double)c.getX() - (double)a.getX();
        return fact11 * fact12 - fact21 * fact22;
    }

    static boolean coordEquals(MVVector v1, MVVector v2) {
        return MVVector.distanceToSquared(v1, v2) < .01f;
    }
    
    static boolean indexPointFloatEquals(IndexPointFloat p1, IndexPointFloat p2) {
        return (Math.abs(p1.x - p2.x) < .01f) && (Math.abs(p1.z - p2.z) < .01f);
    }
    
    static boolean indexBoxFloatEquals(IndexBoxFloat p1, IndexBoxFloat p2) {
        return indexPointFloatEquals(p1.min, p2.min) && indexPointFloatEquals(p1.max, p2.min);
    }

    // Return {-1,0,1} if c is {to the right, on, to the left} of the
    // directed edge defined by a->b.

    int vertexLeftTest(MVVector a, MVVector b, MVVector c) {
        double	det = determinantFloat(a, b, c);
        if (det > 0) return 1;
        else if (det < 0) return -1;
        else return 0;
    }

    int iclamp(int v, int lo, int high) {
        return Math.max(lo, Math.min(high, v));
    }
    
    // Return true if v is on or inside the ear (a,b,c).
    // (a,b,c) must be in ccw order!
    boolean vertexInEar(MVVector v, MVVector a, MVVector b, MVVector c) {
        assert vertexLeftTest(b, a, c) <= 0;	// check ccw order

        if (coordEquals(v, a) || coordEquals(v, c))
            // Special case; we don't care if v is coincident with a or c.
            return false;

        // Include the triangle boundary in our test.
        boolean	abIn = vertexLeftTest(a, b, v) >= 0;
        boolean	bcIn = vertexLeftTest(b, c, v) >= 0;
        boolean	caIn = vertexLeftTest(c, a, v) >= 0;

        return abIn && bcIn && caIn;
    }

    // Helper.  Return the new value of index, for the case of verts [dupedV0]
    // and [dupedV1] being duplicated and subsequent verts being shifted up.
    int remapIndexForDupedVerts(int index, int dupedV0, int dupedV1) {
        assert dupedV0 < dupedV1;
        if (index <= dupedV0)
            // No shift.
            return index;
        else if (index <= dupedV1)
            // Shift up one.
            return index + 1;
        else
            // Shift up two.
            return index + 2;
    }

    // For sort.  Sort by x, then by z.
    int compareVertices(PolyVert vertA, PolyVert vertB) {
        if (vertA.v.getX() < vertB.v.getX())
            return -1;
        else if (vertA.v.getX() > vertB.v.getX())
            return 1;
        else {
            if (vertA.v.getZ() < vertB.v.getZ())
                return -1;
            else if (vertA.v.getZ() > vertB.v.getZ())
                return 1;
        }
        return 0;
    }

    public class PolyVert implements Comparable, Cloneable {
        public PolyVert()
        {}
    
        public PolyVert(float x, float z, TriPoly owner, int myIndex) {
            this.v = new MVVector(x, 0f, z);
            this.myIndex = myIndex;
            next = -1;
            prev = -1;
            convexResult = 0;	// 1 (convex), 0 (colinear), -1 (reflex)
            isEar = false;
            polyOwner = owner;
        }

        public Object clone() {
            PolyVert vert = new PolyVert(v.getX(), v.getZ(), polyOwner, myIndex);
            vert.next = next;
            vert.prev = prev;
            vert.convexResult = convexResult;
            vert.isEar = isEar;
            return vert;
        }

        // For sort.  Sort by x, then by z.
        public int compareTo(Object object) {
            PolyVert vert = (PolyVert)object;
            if (v.getX() < vert.v.getX())
                return -1;
            else if (v.getX() > vert.v.getX())
                return 1;
            else {
                if (v.getZ() < vert.v.getZ())
                    return -1;
                else if (v.getZ() > vert.v.getZ())
                    return 1;
            }
            return 0;
        }

        void remap(int[] remapTable) {
            myIndex = remapTable[myIndex];
            next = remapTable[next];
            prev = remapTable[prev];
        }

        public IndexPointFloat getIndexPoint() {
            return new IndexPointFloat(v.getX(), v.getZ());
        }

        //data:
        MVVector v;
        int myIndex;	// my index into sortedVerts array
        int next;
        int prev;
        int convexResult;	// (@@ only need 2 bits)
        boolean	isEar;
        TriPoly polyOwner;	 // needed?
    }

    // Simple point class for spatial index.
    public class IndexPointFloat implements Cloneable {
        public IndexPointFloat() 
        {}

        public IndexPointFloat(float x, float z) {
            this.x = x;
            this.z = z;
        }

        boolean compare(IndexPointFloat pt) {
            return x == pt.x && z == pt.z;
        }

        public Object clone() {
            return new IndexPointFloat(x, z);
        }
        
        //data:
        float x;
        float z;
    }

    // Simple point class for spatial index.
    public class IndexPointInt implements Cloneable {
        public IndexPointInt() 
        {}

        public IndexPointInt(int x, int z) {
            this.x = x;
            this.z = z;
        }
        
        public Object clone() {
            return new IndexPointInt(x, z);
        }

        boolean compare(IndexPointInt pt) {
            return x == pt.x && z == pt.z;
        }

        //data:
        int x;
        int z;
    }

    // Simple bounding box class.
    public class IndexBoxInt {
        public IndexBoxInt()
        {}

        public IndexBoxInt(IndexPointInt minMaxIn) {
            this.min = minMaxIn;
            this.max = minMaxIn;
        }

        public IndexBoxInt(IndexPointInt minIn, IndexPointInt maxIn) {
            this.min = minIn;
            this.max = maxIn;
        }

        float getWidth() {
            return max.x - min.x;
        }

        float getHeight() {
            return max.z - min.z;
        }

        void expandToEnclose(IndexPointInt loc) {
            if (loc.x < min.x) min.x = loc.x;
            if (loc.z < min.z) min.z = loc.z;
            if (loc.x > max.x) max.x = loc.x;
            if (loc.z > max.z) max.z = loc.z;
        }

        boolean containsPoint(IndexPointInt loc) {
            if (loc.x >= min.x && loc.x <= max.x && loc.z >= min.z && loc.z <= max.z)
                return true;
            else
                return false;
        }

        //data:
        IndexPointInt min;
        IndexPointInt max;
    }

    // Simple bounding box class.
    public class IndexBoxFloat implements Cloneable {
        public IndexBoxFloat()
        {}

        public IndexBoxFloat(IndexPointFloat minMaxIn) {
            this.min = (IndexPointFloat)minMaxIn.clone();
            this.max = (IndexPointFloat)minMaxIn.clone();
        }

        public IndexBoxFloat(IndexPointFloat minIn, IndexPointFloat maxIn) {
            this.min = (IndexPointFloat)minIn.clone();
            this.max = (IndexPointFloat)maxIn.clone();
        }

        public Object clone() {
            return new IndexBoxFloat(min, max);
        }
        
        float getWidth() {
            return max.x - min.x;
        }

        float getHeight() {
            return max.z - min.z;
        }

        void expandToEnclose(IndexPointFloat loc) {
            if (loc.x < min.x) min.x = loc.x;
            if (loc.z < min.z) min.z = loc.z;
            if (loc.x > max.x) max.x = loc.x;
            if (loc.z > max.z) max.z = loc.z;
        }

        boolean containsPoint(IndexPointFloat loc) {
            if (loc.x >= min.x && loc.x <= max.x && loc.z >= min.z && loc.z <= max.z)
                return true;
            else
                return false;
        }

        //data:
        IndexPointFloat min;
        IndexPointFloat max;
    }

    //
    // gridEntryBox
    // Holds one entry for a grid cell.
    //
    public class GridEntryBox {
        IndexBoxFloat bound;
        int value;
        int lastQueryId;	// avoid returning the same item multiple times
        public GridEntryBox() {
            lastQueryId = 0;
        }
    }

    // Holds one entry for a grid point cell.
    public class GridEntryPoint {
        IndexPointFloat location;
        int value;

        public GridEntryPoint next;
    }

    // Grid-based container for points.
    public class GridIndexPoint {
        public GridIndexPoint(IndexBoxFloat bound, int xCells, int zCells) {
            this.bound = bound;
            this.xCells = xCells;
            this.zCells = zCells;
            assert xCells > 0 && zCells > 0;
            assert bound.min.x <= bound.max.x;
            assert bound.min.z <= bound.max.z;

            // Allocate the grid.
            int count = xCells * zCells;
            grid = new GridEntryPoint[count];
        }

        IndexBoxFloat getBound() {
            return bound;
        }

        public class GridIndexPointIterator {
            GridIndexPoint index;
            IndexBoxFloat query;
            IndexBoxInt queryCells;
            int currentCellX, currentCellY;
            GridEntryPoint currentEntry;

            public GridIndexPointIterator() {
                index = null;
                query = new IndexBoxFloat(new IndexPointFloat(0, 0), new IndexPointFloat(0, 0));
                queryCells = new IndexBoxInt(new IndexPointInt(0, 0), new IndexPointInt(0, 0));
                currentCellX = 0;
                currentCellY = 0;
                currentEntry = null;
            }

            boolean atEnd() {
                return currentEntry == null;
            }

            void advanceIfNotEnded() {
                if (atEnd() == false)
                    advance();
            }

            // Point at next element in the iteration.
            void advance() {
                if (currentEntry != null) {
                    // Continue through current cell.
                    currentEntry = currentEntry.next;

                    if (atEnd() == false)
                        return;
                }
                assert currentEntry == null;

                // Done with current cell; go to next cell.
                currentCellX++;
                while (currentCellY <= queryCells.max.z) {
                    for (;;) {
                        if (currentCellX > queryCells.max.x)
                            break;

                        currentEntry = index.getCell(currentCellX, currentCellY);
                        if (currentEntry != null)
                            // Found a valid cell.
                            return;

                        currentCellX++;
                    }
                    currentCellX = (int)queryCells.min.x;
                    currentCellY++;
                }

                assert currentCellX == queryCells.min.x;
                assert currentCellY == queryCells.max.z + 1;

                // No more valid cells.
                assert atEnd();
            }

            GridEntryPoint get() {
                assert atEnd() == false && currentEntry != null;
                return currentEntry;
            }

        }

        GridIndexPointIterator begin(IndexBoxFloat q) {
            GridIndexPointIterator	it = new GridIndexPointIterator();
            it.index = this;
            it.query = (IndexBoxFloat)q.clone();
            it.queryCells.min = (IndexPointInt)getContainingCellClamped(q.min).clone();
            it.queryCells.max = (IndexPointInt)getContainingCellClamped(q.max).clone();

            assert it.queryCells.min.x <= it.queryCells.max.x;
            assert it.queryCells.min.z <= it.queryCells.max.z;

            it.currentCellX = (int)it.queryCells.min.x;
            it.currentCellY = (int)it.queryCells.min.z;
            it.currentEntry = getCell(it.currentCellX, it.currentCellY);

            // Make sure iterator starts valid.
            if (it.currentEntry == null)
                it.advance();

            return it;
        }

        GridIndexPointIterator end() {
            GridIndexPointIterator	it = new GridIndexPointIterator();
            it.index = this;
            it.currentEntry = null;

            return it;
        }

        // Insert a point, with the given location and int, into
        // our index.
        void add(IndexPointFloat location, int p) {
            IndexPointInt ip = getContainingCellClamped(location);

            GridEntryPoint newEntry = new GridEntryPoint();
            newEntry.location = (IndexPointFloat)location.clone();
            newEntry.value = p;

            // Link it into the containing cell.
            int index = getCellIndex(ip);
            newEntry.next = grid[index];
            grid[index] = newEntry;
        }

        // Removes the entry from the index, and deletes it.
        void remove(GridEntryPoint entry) {
            assert entry != null;

            IndexPointInt ip = getContainingCellClamped(entry.location);
            int index = getCellIndex(ip);

            // Unlink matching entry.
            GridEntryPoint value = grid[index];
            if (value == entry) {
                grid[index] = value.next;
                return;
            }
            else {
                while (value != null) {
                    if (value.next != null && value.next == entry) {
                        // This is the one; unlink it.
                        value.next = value.next.next;
                        return;
                    }

                    // Go to the next entry.
                    value = value.next;
                }
            }

            // Didn't find entry!  Something is wrong.
            assert false;
        }

        // Helper.  Search for matching entry.
        GridIndexPointIterator find(IndexPointFloat location, int p) {
            GridIndexPointIterator it = null;
            for (it = begin(new IndexBoxFloat(location, location)); !it.atEnd(); it.advanceIfNotEnded()) {
                if (indexPointFloatEquals(it.currentEntry.location, location) && it.currentEntry.value == p)
                    // Found it.
                    return it;
            }

            // Didn't find it.
            assert it.atEnd();
            return it;
        }

        GridEntryPoint getCell(int x, int z) {
            assert x >= 0 && x < xCells;
            assert z >= 0 && z < zCells;

            return grid[x + z * xCells];
        }

        int getCellIndex(IndexPointInt ip)
        {
            assert ip.x >= 0 && ip.x < xCells;
            assert ip.z >= 0 && ip.z < zCells;

            int index = (int)(ip.x + ip.z * xCells);

            return index;
        }

        // Get the indices of the cell that contains the given point.
        IndexPointInt getContainingCellClamped(IndexPointFloat p) {
            IndexPointInt ip = new IndexPointInt(
                (int)(((p.x - bound.min.x) * (float)xCells) / (bound.max.x - bound.min.x)),
                (int)(((p.z - bound.min.z) * (float)zCells) / (bound.max.z - bound.min.z)));

            // Clamp.
            if (ip.x < 0) 
                ip.x = 0;
            if (ip.x >= xCells)
                ip.x = xCells - 1;
            if (ip.z < 0)
                ip.z = 0;
            if (ip.z >= zCells)
                ip.z = zCells - 1;

            return ip;
        }

        //data:
        IndexBoxFloat bound;
        int xCells;
        int zCells;
        GridEntryPoint[] grid;
    }

    // Grid-based container for boxes.
    public class GridIndexBox {

        public GridIndexBox(IndexBoxFloat bound, int xCells, int zCells) {
            this.bound = (IndexBoxFloat)bound.clone();
            this.xCells = xCells;
            this.zCells = zCells;
            queryId = 0;
            assert xCells > 0 && zCells > 0;
            assert bound.min.x <= bound.max.x;
            assert bound.min.z <= bound.max.z;

            // Allocate the grid.
            grid = new ArrayList[xCells][zCells];
            for (int x=0; x<xCells; x++) {
                for (int z=0; z<zCells; z++) {
                    grid[x][z] = new ArrayList<GridEntryBox>();
                }
            }
        }

        IndexBoxFloat getBound() {
            return bound;
        }

        int getQueryId() {
            return queryId;
        }

        public class GridIndexBoxIterator {
            GridIndexBox index;
            IndexBoxFloat query;
            IndexBoxInt queryCells;
            int currentCellX, currentCellZ;
            int currentCellArrayIndex;
            GridEntryBox currentEntry;

            public GridIndexBoxIterator() {
                index = null;
                query = new IndexBoxFloat(new IndexPointFloat(0, 0), new IndexPointFloat(0, 0));
                queryCells = new IndexBoxInt(new IndexPointInt(0, 0), new IndexPointInt(0, 0));
                currentCellX = 0;
                currentCellZ = 0;
                currentCellArrayIndex = -1;
                currentEntry = null;
            }

            boolean atEnd() {
                return currentEntry == null;
            }

            void advanceIfNotEnded() {
                if (atEnd() == false)
                    advance();
            }

            // Point at next element in the iteration.
            void advance() {
                if (advanceInCell())
                    return;

                // Done with current cell; go to next cell.
                currentCellX++;
                while (currentCellZ <= queryCells.max.z) {
                    for (;;) {
                        if (currentCellX > queryCells.max.x)
                            break;

                        if (advanceInCell())
                            // We're good.
                            return;

                        currentCellX++;
                    }
                    currentCellX = (int)queryCells.min.x;
                    currentCellZ++;
                }

                assert currentCellX == queryCells.min.x;
                assert currentCellZ == queryCells.max.z + 1;

                // No more valid cells.
                assert atEnd();
            }

            // Go to the next valid element in the current cell.
            // If we reach the end of the cell, set
            // currentCellArrayIndex to -1 and return false.
            // Otherwise point currentEntry at the valid
            // element, and return true.
            boolean advanceInCell() {
                int queryId = index.getQueryId();
                ArrayList<GridEntryBox> cellArray = index.getCell(currentCellX, currentCellZ);

                while (++currentCellArrayIndex < cellArray.size()) {
                    // Continue through the current cell.
                    currentEntry = cellArray.get(currentCellArrayIndex);
                    if (currentEntry.lastQueryId != queryId) {
                        // Valid entry; update query id.
                        currentEntry.lastQueryId = queryId;
                        return true;
                    }
                }

                // No more valid entries in this cell.
                currentEntry = null;
                currentCellArrayIndex = -1;

                return false;
            }

            GridEntryBox getCurrent() {
                assert atEnd() == false && currentEntry != null;
                return currentEntry;
            }
        }

        GridIndexBoxIterator begin(IndexBoxFloat q) {
            queryId++;
            if (queryId == 0) {
                // Query id wrapped around!  Clear lastQueryId in all entries in our
                // array, to avoid aliasing from old queries.
                for (int i = 0; i < xCells; i++) {
                    for (int j=0, n=zCells; j<n; j++) {
                        ArrayList<GridEntryBox> cellArray = grid[i][j];
                        for (GridEntryBox entryBox : cellArray)
                            entryBox.lastQueryId = 0;
                    }
                }
                queryId = 1;
            }

            GridIndexBoxIterator it = new GridIndexBoxIterator();
            it.index = this;
            it.query = (IndexBoxFloat)q.clone();
            it.queryCells.min = getContainingCellClamped(q.min);
            it.queryCells.max = getContainingCellClamped(q.max);

            assert it.queryCells.min.x <= it.queryCells.max.x;
            assert it.queryCells.min.z <= it.queryCells.max.z;

            it.currentCellX = (int)it.queryCells.min.x;
            it.currentCellZ = (int)it.queryCells.min.z;

            it.advance();	// find first valid entry.
            return it;
        }

        // For convenience.
        GridIndexBoxIterator beginAll() {
            return begin(getBound());
        }

        GridIndexBoxIterator end() {
            GridIndexBoxIterator it = new GridIndexBoxIterator();
            it.index = this;
            it.currentEntry = null;
            return it;
        }

        // Insert a box, with the given int, into our index.
        void add(IndexBoxFloat bound, int p) {
            IndexBoxInt ib = getContainingCellsClamped(bound);

            GridEntryBox newEntry = new GridEntryBox();
            newEntry.bound = bound;
            newEntry.value = p;

            // Add it to all cells it overlaps with.
            for (int iz = ib.min.z; iz <= ib.max.z; iz++) {
                for (int ix = ib.min.x; ix <= ib.max.x; ix++) {
                    ArrayList<GridEntryBox> cellArray = getCell(ix, iz);
                    cellArray.add(newEntry);
                }
            }
        }


        // Removes the entry from the index, and deletes it.
        void remove(GridEntryBox entry) {
            assert entry != null;

            // Find and remove the entry from all cells that it overlaps with.
            IndexBoxInt ib = getContainingCellsClamped(entry.bound);

            for (int iz = ib.min.z; iz <= ib.max.z; iz++) {
                for (int ix = ib.min.x; ix <= ib.max.x; ix++) {
                    ArrayList<GridEntryBox> cellArray = getCell(ix, iz);

                    int i, n;
                    for (i = 0, n = cellArray.size(); i < n; i++) {
                        // Find entry, and remove it.
                        if (cellArray.get(i) == entry) {
                            cellArray.remove(i);
                            break;
                        }
                    }
                    assert i < n;	// Didn't find entry in this cell!  Something is wrong.
                }
            }
        }


        // Helper.  Search for matching entry.
        GridIndexBoxIterator find(IndexBoxFloat bound, int p) {
            GridIndexBoxIterator it;
            for (it = begin(bound); !it.atEnd(); it.advanceIfNotEnded()) {
                if (indexBoxFloatEquals(it.currentEntry.bound, bound) && it.currentEntry.value == p)
                    // Found it.
                    return it;
            }
            // Didn't find it.
            assert it.atEnd();
            return it;
        }

        // Helper.  Search for matching int, given any point within its bound.
        // Should be relatively quick, assuming int is unique.
        GridEntryBox findPayloadFropoint(IndexPointFloat loc, int p) {
            IndexPointInt ip = getContainingCellClamped(loc);
            ArrayList<GridEntryBox> cellArray = getCell(ip.x, ip.z);

            for (int i = 0, n = cellArray.size(); i < n; i++) {
                GridEntryBox entry = cellArray.get(i);
                if (entry.value == p)
                    // Found it.
                    return entry;
            }
            // Didn't find it.
            return null;
        }

        ArrayList<GridEntryBox> getCell(int x, int z) {
            assert x >= 0 && x < xCells;
            assert z >= 0 && z < zCells;
            return grid[x][z];
        }

        // Get the indices of the cell that contains the given point.
        IndexPointInt getContainingCellClamped(IndexPointFloat p) {
            IndexPointInt ip = new IndexPointInt(
                (int)(((p.x - bound.min.x) * (float)xCells) / (bound.max.x - bound.min.x)),
                (int)(((p.z - bound.min.z) * (float)zCells) / (bound.max.z - bound.min.z)));

            // Clamp.
            if (ip.x < 0)
                ip.x = 0;
            if (ip.x >= xCells)
                ip.x = xCells - 1;
            if (ip.z < 0)
                ip.z = 0;
            if (ip.z >= zCells)
                ip.z = zCells - 1;

            return ip;
        }

        // Get the indices of the cell that contains the given point.
        IndexBoxInt getContainingCellsClamped(IndexBoxFloat p) {
            return new IndexBoxInt(getContainingCellClamped(p.min), getContainingCellClamped(p.max));
        }

        //data:
        IndexBoxFloat bound;
        int xCells;
        int zCells;
        int queryId;
        ArrayList<GridEntryBox>[][] grid;
    }

    // Return true if edge (e0v0,e0v1) intersects (e1v0,e1v1).
    boolean edgesIntersectSub(ArrayList<PolyVert> sortedVerts, int e0v0i, int e0v1i, int e1v0i, int e1v1i) {
        // If e1v0,e1v1 are on opposite sides of e0, and e0v0,e0v1 are
        // on opposite sides of e1, then the segments cross.  These
        // are all determinant checks.

        // The main degenerate case we need to watch out for is if
        // both segments are zero-length.
        //
        // If only one is degenerate, our tests are still OK.

        MVVector e0v0 = sortedVerts.get(e0v0i).v;
        MVVector e0v1 = sortedVerts.get(e0v1i).v;
        MVVector e1v0 = sortedVerts.get(e1v0i).v;
        MVVector e1v1 = sortedVerts.get(e1v1i).v;

        // Note: exact equality here.  I think the reason to use
        // epsilons would be underflow in case of very small
        // determinants.  Our determinants are doubles, so I think
        // we're good.
        if (e0v0.getX() == e0v1.getX() && e0v0.getZ() == e0v1.getZ()) {
            // e0 is zero length.
            if (e1v0.getX() == e1v1.getX() && e1v0.getZ() == e1v1.getZ()) {
                // Both edges are zero length.
                // They intersect only if they're coincident.
                return e0v0.getX() == e1v0.getX() && e0v0.getZ() == e1v0.getZ();
            }
        }

        // See if e1 crosses line of e0.
        double	det10 = determinantFloat(e0v0, e0v1, e1v0);
        double	det11 = determinantFloat(e0v0, e0v1, e1v1);

        // Note: we do > 0, which means a vertex on a line counts as
        // intersecting.  In general, if one vert is on the other
        // segment, we have to go searching along the path in either
        // direction to see if it crosses or not, and it gets
        // complicated.  Better to treat it as intersection.

        if (det10 * det11 > 0)
            // e1 doesn't cross the line of e0.
            return false;

        // See if e0 crosses line of e1.
        double	det00 = determinantFloat(e1v0, e1v1, e0v0);
        double	det01 = determinantFloat(e1v0, e1v1, e0v1);

        if (det00 * det01 > 0)
            // e0 doesn't cross the line of e1.
            return false;

        // They both cross each other; the segments intersect.
        return true;
    }

    boolean edgesIntersect(ArrayList<PolyVert> sortedVerts, int e0v0, int e0v1, int e1v0, int e1v1)
    // Return true if edge (e0v0,e0v1) intersects (e1v0,e1v1).
    {
        // Deal with special case: edges that share exactly one vert.
        // We treat these as no intersection, even though technically
        // they share one point.
        //
        // We're not just comparing indices, because duped verts (for
        // bridges) might have different indices.
        //
        // @@ this needs review -- might be wrong.
        boolean[][] coincident = new boolean[2][2];
        coincident[0][0] = coordEquals(sortedVerts.get(e0v0).v, sortedVerts.get(e1v0).v);
        coincident[0][1] = coordEquals(sortedVerts.get(e0v0).v, sortedVerts.get(e1v1).v);
        coincident[1][0] = coordEquals(sortedVerts.get(e0v1).v, sortedVerts.get(e1v0).v);
        coincident[1][1] = coordEquals(sortedVerts.get(e0v1).v, sortedVerts.get(e1v1).v);
        if (coincident[0][0] && !coincident[1][1]) return false;
        if (coincident[1][0] && !coincident[0][1]) return false;
        if (coincident[0][1] && !coincident[1][0]) return false;
        if (coincident[1][1] && !coincident[0][0]) return false;

        // @@ eh, I think we really want this to be an intersection
// #if 0
//         // Both verts identical: early out.
//         //
//         // Note: treat this as no intersection!  This is mainly useful
//         // for things like coincident vertical bridge edges.
//         if (coincident[0][0] && coincident[1][1]) return false;
//         if (coincident[1][0] && coincident[0][1]) return false;
// #endif // 0

        // Check for intersection.
        return edgesIntersectSub(sortedVerts, e0v0, e0v1, e1v0, e1v1);
    }




    // Return true if vert vi is convex.
    boolean isConvexVert(ArrayList<PolyVert> sortedVerts, int vi) {
        PolyVert pvi = sortedVerts.get(vi);
        PolyVert pvPrev = sortedVerts.get(pvi.prev);
        PolyVert pvNext = sortedVerts.get(pvi.next);
        return vertexLeftTest(pvPrev.v, pvi.v, pvNext.v) > 0;
    }

    int comparePolysByLeftmostVert(TriPoly polyA, TriPoly polyB)
    {
        // Vert indices are sorted, so we just compare the indices,
        // not the actual vert coords.
        if (polyA.leftmostVert < polyB.leftmostVert)
            return -1;
        else {
            // polys are not allowed to share verts, so the
            // leftmost vert must be different!
            assert polyA.leftmostVert > polyB.leftmostVert;
            return 1;
        }
    }

    public class TriPoly implements Comparable {
        int loop;	// index of first vert
        int leftmostVert;
        int vertexCount;
        int earCount;

        GridIndexBox edgeIndex;
        
        // point search index (for finding reflex verts within a potential ear)
        GridIndexPoint reflexPointIndex;

        public TriPoly() {
            loop = -1;
            leftmostVert = -1;
            vertexCount = 0;
            earCount = 0;
            edgeIndex = null;
            reflexPointIndex = null;
        }

        public int compareTo(Object object)
        {
            TriPoly poly = (TriPoly)object;
            // Vert indices are sorted, so we just compare the indices,
            // not the actual vert coords.
            if (leftmostVert < poly.leftmostVert)
                return -1;
            else {
                // polys are not allowed to share verts, so the
                // leftmost vert must be different!
                assert leftmostVert > poly.leftmostVert;
                return 1;
            }
        }

        int getVertexCount() {
            return vertexCount;
        }
        
	int getEarCount() {
            return earCount;
        }

        boolean	isValid(ArrayList<PolyVert> sortedVerts) {
            return isValid(sortedVerts, true);
        }
        
        boolean	isValid(ArrayList<PolyVert> sortedVerts, boolean checkConsecutiveDupes) {

            if (loop == -1 && leftmostVert == -1 && vertexCount == 0)
                // Empty poly.
                return true;

            assert leftmostVert == -1 || sortedVerts.get(leftmostVert).polyOwner == this;

            // Check vert count.
            int firstVert = loop;
            int vi = firstVert;
            int vertCount = 0;
            int foundEarCount = 0;
            boolean foundLeftmost = false;
            int reflexVertCount = 0;
            do {
                PolyVert pvi = sortedVerts.get(vi);

                // Check ownership.
                assert pvi.polyOwner == this;

                // Check leftmost vert.
                assert leftmostVert == -1
                    || compareVertices(
                        sortedVerts.get(leftmostVert),
                        sortedVerts.get(vi)) <= 0;

                // Check link integrity.
                int vNext = pvi.next;
                assert sortedVerts.get(vNext).prev == vi;

                if (vi == leftmostVert)
                    foundLeftmost = true;

                if (checkConsecutiveDupes && vNext != vi)
                    // Subsequent verts are not allowed to be
                    // coincident; that causes errors in ear
                    // classification.
                    assert coordEquals(pvi.v, sortedVerts.get(vNext).v) == false;

                if (pvi.convexResult < 0)
                    reflexVertCount++;

                if (pvi.isEar)
                    foundEarCount++;

                vertCount++;
                vi = vNext;
            }
            while (vi != firstVert);

            assert foundEarCount == earCount;
            assert vertCount == vertexCount;
            assert foundLeftmost || leftmostVert == -1;

            // Count reflex verts in the grid index.
            if (reflexPointIndex != null) {
                int checkCount = 0;
                for (GridIndexPoint.GridIndexPointIterator it = reflexPointIndex.begin((IndexBoxFloat)reflexPointIndex.getBound().clone());
                     ! it.atEnd();
                     it.advanceIfNotEnded()) {
                    checkCount++;
                }

                assert checkCount == reflexVertCount;
            }

            // Count edges in the edge index.  There should be exactly one edge per vert.
            if (edgeIndex != null) {
                int checkCount = 0;
                for (GridIndexBox.GridIndexBoxIterator it = edgeIndex.begin(edgeIndex.getBound());
                     ! it.atEnd();
                     it.advanceIfNotEnded()) {
                    checkCount++;
                }
                assert checkCount == vertCount;
            }

            // Might be nice to check that all verts with (polyOwner ==
            // this) are in our loop.
            return true;
        }

        // Mark as invalid/empty.  Do this after linking into another poly,
        // for safety/debugging.
        void invalidate(ArrayList<PolyVert> sortedVerts) {
            assert loop == -1 || sortedVerts.get(loop).polyOwner != this;	// make sure our verts have been stolen already.
            loop = -1;
            leftmostVert = -1;
            vertexCount = 0;
            assert isValid(sortedVerts);
        }

        // Link the specified vert into our loop.
        void appendVert(ArrayList<PolyVert> sortedVerts, int vertIndex) {
            assert vertIndex >= 0 && vertIndex < (int) sortedVerts.size();
            assert isValid(sortedVerts, false); /* don't check for consecutive dupes, poly is not finished */;

            vertexCount++;

            if (loop == -1) {
                // First vert.
                assert vertexCount == 1;
                loop = vertIndex;
                PolyVert pv = sortedVerts.get(vertIndex);
                pv.next = vertIndex;
                pv.prev = vertIndex;
                pv.polyOwner = this;
                leftmostVert = vertIndex;
            }
            else {
                // We have a loop.  Link the new vert in, behind the
                // first vert.
                PolyVert pv0 = sortedVerts.get(loop);
                PolyVert pv = sortedVerts.get(vertIndex);
                pv.next = loop;
                pv.prev = pv0.prev;
                pv.polyOwner = this;
                sortedVerts.get(pv0.prev).next = vertIndex;
                pv0.prev = vertIndex;

                // Update leftmostVert
                PolyVert pvl = sortedVerts.get(leftmostVert);
                if (compareVertices(pv, pvl) < 0)
                    // v is to the left of vl; it's the new leftmost vert
                    leftmostVert = vertIndex;
            }

            assert isValid(sortedVerts, false /* don't check for consecutive dupes, poly is not finished */);
        }

        // Find a vert v, in this poly, such that v is to the left of v1, and
        // the edge (v,v1) doesn't intersect any edges in this poly.
        int findValidBridgeVert(ArrayList<PolyVert> sortedVerts, int v1) {
            assert isValid(sortedVerts);

            PolyVert pv1 = sortedVerts.get(v1);
            assert pv1.polyOwner != this;	// v1 must not be part of this poly already

            // Held recommends searching verts near v1 first.  And for
            // correctness, we may only consider verts to the left of v1.
            // A fast & easy way to implement this is to walk backwards in
            // our vert array, starting with v1-1.

            // Walk forward to include all coincident but later verts!
            int vi = v1;
            while ((vi + 1) < (int) sortedVerts.size() && coordEquals(sortedVerts.get(vi + 1).v, pv1.v))
                vi++;

            // Now scan backwards for the vert to bridge onto.
            for ( ; vi >= 0; vi--) {
                PolyVert pvi = sortedVerts.get(vi);

                assert compareVertices(pvi, pv1) <= 0;

                if (pvi.polyOwner == this) {
                    // Candidate is to the left of pv1, so it
                    // might be valid.  We don't consider verts to
                    // the right of v1, because of possible
                    // intersection with other polys.  Due to the
                    // poly sorting, we know that the edge
                    // (pvi,pv1) can only intersect this poly.

                    if (anyEdgeIntersection(sortedVerts, v1, vi) == false)
                        return vi;
                }
            }

            // Ugh!  No valid bridge vert.  Shouldn't happen with valid
            // data.  For invalid data, just pick something and live with
            // the intersection.
            log.error("findValidBridgeVert: can't find bridge for vert " + v1 + "!");

            return leftmostVert;
        }

        void	remap(int[] remapTable) {
            assert loop > -1;
            assert(leftmostVert > -1);

            loop = remapTable[loop];
            leftmostVert = remapTable[leftmostVert];
        }

        // Remap for the case of v0 and v1 being duplicated, and subsequent
        // verts being shifted up.
        void	remapForDupedVerts(ArrayList<PolyVert> sortedVerts, int v0, int v1) {
            assert loop > -1;
            assert(leftmostVert > -1);

            loop = remapIndexForDupedVerts(loop, v0, v1);
            leftmostVert = remapIndexForDupedVerts(leftmostVert, v0, v1);

            // Remap the vert indices stored in the edge index.
            if (edgeIndex != null) {
                // Optimization: we don't need to remap anything
                // that's wholly to the left of v0.  Towards the end
                // of bridge building, this could be the vast majority
                // of edges.
                assert v0 < v1;
                IndexBoxFloat bound = (IndexBoxFloat)edgeIndex.getBound().clone();
                bound.min.x = sortedVerts.get(v0).v.getX();
                for (GridIndexBox.GridIndexBoxIterator it = edgeIndex.begin(bound);
                     ! it.atEnd();
                     it.advanceIfNotEnded()) {
                    it.currentEntry.value = remapIndexForDupedVerts(it.currentEntry.value, v0, v1);
                }
            }

            // We shouldn't have a point index right now.
            assert reflexPointIndex == null;
        }

        void classifyVert(ArrayList<PolyVert> sortedVerts, int vi)
        // Decide if vi is an ear, and mark its isEar flag & update counts.
        {
            PolyVert pvi = sortedVerts.get(vi);
            PolyVert pvPrev = sortedVerts.get(pvi.prev);
            PolyVert pvNext = sortedVerts.get(pvi.next);

            if (pvi.convexResult > 0)
            {
                if (vertInCone(sortedVerts, pvi.prev, vi, pvi.next, pvNext.next)
                    && vertInCone(sortedVerts, pvi.next, pvPrev.prev, pvi.prev, vi))
                {
                    if (! earContainsReflexVertex(sortedVerts, pvi.prev, vi, pvi.next))
                    {
                        // Valid ear.
                        assert pvi.isEar == false;
                        pvi.isEar = true;
                        earCount++;
                    }
                }
            }
        }

        void	dirtyVert(ArrayList<PolyVert> sortedVerts, int vi)
        // Call when an adjacent vert gets clipped.  Recomputes
        // convexResult and clears isEar for the vert.
        {
            PolyVert pvi = sortedVerts.get(vi);

            int newConvexResult =
                vertexLeftTest(sortedVerts.get(pvi.prev).v, pvi.v, sortedVerts.get(pvi.next).v);
            if (newConvexResult < 0 && pvi.convexResult >= 0)
            {
                // Vert is newly reflex.
                // Add to reflex vert index
                assert reflexPointIndex != null;
                reflexPointIndex.add(new IndexPointFloat(pvi.v.getX(), pvi.v.getZ()), vi);
            }
            else if (pvi.convexResult < 0 && newConvexResult >= 0)
            {
                // Vert is newly convex/colinear.
                // Remove from reflex vert index.
                assert reflexPointIndex != null;
                GridIndexPoint.GridIndexPointIterator it = reflexPointIndex.find(new IndexPointFloat(pvi.v.getX(), pvi.v.getZ()), vi);
                assert it.atEnd() == false;

                reflexPointIndex.remove(it.currentEntry);
            }
            pvi.convexResult = newConvexResult;

            if (pvi.isEar)
            {
                // Clear its ear flag.
                pvi.isEar = false;
                earCount--;
            }
        }

        // Initialize our ear loop with all the ears that can be clipped.
        //
        // Returns true if we clipped any degenerates while looking for ears.
        boolean	buildEarList(ArrayList<PolyVert> sortedVerts, Random rg) {
            assert isValid(sortedVerts);
            assert earCount == 0;
            boolean clippedAnyDegenerates = false;

            if (vertexCount < 3)
                // Not a real poly, no ears.
                return false;

            // Go around the loop, evaluating the verts.
            int vi = loop;
            int vertsProcessedCount = 0;
            for (;;) {
                PolyVert pvi = sortedVerts.get(vi);
                PolyVert pvPrev = sortedVerts.get(pvi.prev);
                PolyVert pvNext = sortedVerts.get(pvi.next);

                // classification of ear, CE2 from FIST paper:
                //
                // v[i-1],v[i],v[i+1] of P form an ear of P iff
                //
                // 1. v[i] is a convex vertex
                //
                // 2. the interior plus boundary of triangle
                // v[i-1],v[i],v[i+1] does not contain any reflex vertex of P
                // (except v[i-1] or v[i+1])
                //
                // 3. v[i-1] is in the cone(v[i],v[i+1],v[i+2]) and v[i+1] is
                // in the cone(v[i-2],v[i-1],v[i]) (not strictly necessary,
                // but used for efficiency and robustness)

                if ((pvi == pvNext)
                    || (pvi == pvPrev)
                    || (vertexLeftTest(pvPrev.v, pvi.v, pvNext.v) == 0
                        && vertIsDuplicated(sortedVerts, vi) == false)) {
                    // Degenerate case: zero-area triangle.
                    //
                    // Remove it (and any additional degenerates chained onto this ear).
                    vi = removeDegenerateChain(sortedVerts, vi);

                    clippedAnyDegenerates = true;

                    if (vertexCount < 3)
                        break;

                    continue;
                }
                else
                    classifyVert(sortedVerts, vi);

                vi = pvi.next;
                vertsProcessedCount++;

                if (vertsProcessedCount >= vertexCount)
                    break;

                // Performance optimization: if we're finding lots of
                // ears, keep our working set local by processing a
                // few ears soon after examining them.
                if (earCount > 5 && vertsProcessedCount > 10)
                    break;
            }

            assert isValid(sortedVerts, true); // do check for dupes

            // @@ idea for cheap ear shape control: loop = bestEarFound;

            return clippedAnyDegenerates;
        }

        static final int MASK_TABLE_SIZE = 8;
        // roughly, the largest (2^N-1) <= index
        int[] randomask = { 1, 1, 1, 3, 3, 3, 3, 7 };

        // Return the next ear to be clipped.
        int getNextEar(ArrayList<PolyVert> sortedVerts, Random rg) {
            assert earCount > 0;

            while (sortedVerts.get(loop).isEar == false)
                loop = sortedVerts.get(loop).next;

            int nextEar = loop;

            // define this if you want to randomize the ear selection (should
            // improve the average ear shape, at low cost).
            // Randomization: skip a random number of ears.
            if (earCount > 6) {
                // Decide how many ears to skip.

                // Here's a lot of twiddling to avoid a % op.  Worth it?
                int randorange = earCount >> 2;
                if (randorange >= MASK_TABLE_SIZE) randorange = MASK_TABLE_SIZE - 1;
                assert randorange > 0;

                int randoskip = (int)(rg.nextLong() & randomask[randorange]);

                // Do the skipping, by manipulating loop.
                while (randoskip > 0) {
                    if (sortedVerts.get(loop).isEar)
                        randoskip--;
                    loop = sortedVerts.get(loop).next;
                }
                assert isValid(sortedVerts);
            }
            assert sortedVerts.get(nextEar).isEar == true;

            return nextEar;
        }

        // Push the ear triangle into the output; remove the triangle
        // (i.e. vertex v1) from this poly.
        void emitAndRemoveEar(Collection<Float> result, ArrayList<PolyVert> sortedVerts,
            int v0, int v1, int v2) {
            assert isValid(sortedVerts);
            assert vertexCount >= 3;

            PolyVert pv0 = sortedVerts.get(v0);
            PolyVert pv1 = sortedVerts.get(v1);
            PolyVert pv2 = sortedVerts.get(v2);

            assert sortedVerts.get(v1).isEar;

            if (loop == v1)
                // Change loop, since we're about to lose it.
                loop = v0;

            // Make sure leftmostVert is dead; we don't need it now.
            leftmostVert = -1;

            if (vertexLeftTest(pv0.v, pv1.v, pv2.v) == 0)
                // Degenerate triangle!  Don't emit it.
                // This should have already been removed by removeDegenerateChain().
                assert false;
            else {
                // emit the vertex list for the triangle.
                result.add(pv0.v.getX());
                result.add(pv0.v.getZ());
                result.add(pv1.v.getX());
                result.add(pv1.v.getZ());
                result.add(pv2.v.getX());
                result.add(pv2.v.getZ());
            }

            // Unlink v1.

            if (pv1.convexResult < 0) {
                // Vert was reflex (can happen due to e.g. recovery).
                // Remove from reflex vert index.
                assert reflexPointIndex != null;
                GridIndexPoint.GridIndexPointIterator it = reflexPointIndex.find(new IndexPointFloat(pv1.v.getX(), pv1.v.getZ()), v1);
                assert it.atEnd() == false;

                reflexPointIndex.remove(it.currentEntry);
            }

            assert pv0.polyOwner == this;
            assert(pv1.polyOwner == this);
            assert pv2.polyOwner == this;

            pv0.next = v2;
            pv2.prev = v0;

            pv1.next = -1;
            pv1.prev = -1;
            pv1.polyOwner = null;

            // We lost v1.
            vertexCount--;
            earCount--;

            if (coordEquals(pv0.v, pv2.v)) {
                // removeDegenerateChain() should have taken care of
                // this before we got here.
                assert false;
            }

            // ear status of v0 and v2 could have changed now.
            dirtyVert(sortedVerts, v0);
            dirtyVert(sortedVerts, v2);

            // Big huge performance boost: recheck these local verts now;
            // often we'll clip them right away.
            //
            // @@ what happens if v0 or v2 are now degenerate???
            classifyVert(sortedVerts, v0);
            classifyVert(sortedVerts, v2);

            assert isValid(sortedVerts);
        }

        // Remove the degenerate ear at vi, and any degenerate ear formed as
        // we remove the previous one.
        //
        // Return the index of a vertex just prior to the chain we've removed.
        int removeDegenerateChain(ArrayList<PolyVert> sortedVerts, int vi) {
            assert leftmostVert == -1;

            int retval = vi;

            for (;;) {
                assert isValid(sortedVerts, false);  /* don't check for dupes yet */

                PolyVert pv1 = sortedVerts.get(vi);
                PolyVert pv0 = sortedVerts.get(pv1.prev);
                PolyVert pv2 = sortedVerts.get(pv1.next);

                if (loop == vi)
                    // Change loop, since we're about to lose it.
                    loop = pv0.myIndex;

                // Unlink vi.

                assert pv0.polyOwner == this;
                assert pv1.polyOwner == this;
                assert pv2.polyOwner == this;

                pv0.next = pv2.myIndex;
                pv2.prev = pv0.myIndex;

                pv1.next = -1;
                pv1.prev = -1;
                pv1.polyOwner = null;

                if (pv1.convexResult < 0) {
                    // vi was reflex, remove it from index
                    assert reflexPointIndex != null;
                    GridIndexPoint.GridIndexPointIterator it = reflexPointIndex.find(new IndexPointFloat(pv1.v.getX(), pv1.v.getZ()), vi);
                    assert it.atEnd() == false;

                    reflexPointIndex.remove(it.currentEntry);
                }

                if (pv1.isEar)
                    earCount--;

                // We lost vi.
                vertexCount--;

                assert isValid(sortedVerts, false);  /* don't check for dupes yet */

                if (vertexCount < 3) {
                    retval = pv0.myIndex;
                    break;
                }

                // If we've created another degenerate, then remove it as well.
                if (coordEquals(pv0.v, pv2.v)) {
                    // We've created a dupe in the chain, remove it now.
                    vi = pv0.myIndex;
                }
                else if (vertexLeftTest(sortedVerts.get(pv0.prev).v, pv0.v, pv2.v) == 0)
                    // More degenerate.
                    vi = pv0.myIndex;
                else if (vertexLeftTest(pv0.v, pv2.v, sortedVerts.get(pv2.next).v) == 0)
                    // More degenerate.
                    vi = pv2.myIndex;
                else {
                    // ear/reflex status of pv0 & pv2 may have changed.
                    dirtyVert(sortedVerts, pv0.myIndex);
                    dirtyVert(sortedVerts, pv2.myIndex);
                    retval = pv0.myIndex;
                    break;
                }
            }

            assert isValid(sortedVerts, true);  /* do check for dupes; there shouldn't be any! */

            return retval;
        }

        // Given the beginning and end of a sub-loop that has just been linked
        // into our loop, update the verts on the sub-loop to have the correct
        // owner, update our leftmostVert and our vertCount.
        void	updateConnectedSubPoly(ArrayList<PolyVert> sortedVerts, int vFirstInSubloop, int vFirstAfterSubloop) {
            assert vFirstInSubloop != vFirstAfterSubloop;

            int vi = vFirstInSubloop;
            do {
                PolyVert pv = sortedVerts.get(vi);

                pv.polyOwner = this;
                vertexCount++;

                // Update leftmost vert.
                if (pv.myIndex < leftmostVert)
                    leftmostVert = pv.myIndex;

                // Insert new edge into the edge index.
                addEdge(sortedVerts, vi);

                vi = pv.next;
            }
            while (vi != vFirstAfterSubloop);

            assert isValid(sortedVerts);
        }

        // Initialize our edge-search structure, for quickly finding possible
        // intersecting edges (when constructing bridges to join polys).
        void initEdgeIndex(ArrayList<PolyVert> sortedVerts, IndexBoxFloat boundOfAllVerts) {
            assert isValid(sortedVerts);
            assert edgeIndex == null;

            // Compute grid density.
            int xCells = 1;
            int yCells = 1;
            if (sortedVerts.size() > 0) {
                float GRIDSCALE = (float)Math.sqrt(0.5);
                float width = boundOfAllVerts.getWidth();
                float height = boundOfAllVerts.getHeight();
                float area = width * height;
                if (area > 0) {
                    float sqrtN = (float)Math.sqrt((double) sortedVerts.size());
                    float w = width * width / area * GRIDSCALE;
                    float h = height * height / area * GRIDSCALE;
                    xCells = (int)(w * sqrtN);
                    yCells = (int)(h * sqrtN);
                }
                else {
                    // Zero area.
                    if (width > 0)
                        xCells = (int)(GRIDSCALE * GRIDSCALE * sortedVerts.size());
                    else
                        yCells = (int)(GRIDSCALE * GRIDSCALE * sortedVerts.size());
                }
                xCells = iclamp(xCells, 1, 256);
                yCells = iclamp(yCells, 1, 256);
            }

            edgeIndex = new GridIndexBox(boundOfAllVerts, xCells, yCells);

            // Insert current edges into the index.
            int vi = loop;
            for (;;) {
                addEdge(sortedVerts, vi);

                vi = sortedVerts.get(vi).next;
                if (vi == loop)
                    break;
            }
            assert isValid(sortedVerts);
        }

        // Classify all verts for convexity.
        //
        // Initialize our point-search structure, for quickly finding reflex
        // verts within a potential ear.
        void initForEarClipping(ArrayList<PolyVert> sortedVerts) {
            assert isValid(sortedVerts);

            // Kill leftmostVert; don't need it once all the polys are
            // joined together into one loop.
            leftmostVert = -1;
            // Kill edge index; we don't need it for ear clipping.
            edgeIndex = null;
            int reflexVertCount = 0;
            boolean boundInited = false;
            IndexBoxFloat reflexBound = new IndexBoxFloat(new IndexPointFloat(0f, 0f), new IndexPointFloat(0f, 0f));
            int vi = loop;
            for (;;) {
                // Classify vi as reflex/convex.
                PolyVert pvi = sortedVerts.get(vi);
                pvi.convexResult = vertexLeftTest(sortedVerts.get(pvi.prev).v, pvi.v, sortedVerts.get(pvi.next).v);

                if (pvi.convexResult < 0) {
                    reflexVertCount++;

                    // Update bounds.
                    IndexPointFloat location = new IndexPointFloat(pvi.v.getX(), pvi.v.getZ());
                    if (boundInited == false) {
                        boundInited = true;
                        reflexBound = new IndexBoxFloat((IndexPointFloat)location.clone(), (IndexPointFloat)location.clone());
                    }
                    else
                        reflexBound.expandToEnclose(location);
                }

                vi = sortedVerts.get(vi).next;
                if (vi == loop)
                    break;
            }

            // Compute grid density.  FIST recommends w * sqrt(N) * h *
            // sqrt(N), where w*h is between 0.5 and 2.  (N is the reflex
            // vert count.)
            int xCells = 1;
            int yCells = 1;
            if (reflexVertCount > 0) {
                float GRIDSCALE = (float)Math.sqrt(0.5);
                float width = reflexBound.getWidth();
                float height = reflexBound.getHeight();
                float area = width * height;
                if (area > 0) {
                    float sqrtN = (float)Math.sqrt((double) reflexVertCount);
                    float w = width * width / area * GRIDSCALE;
                    float h = height * height / area * GRIDSCALE;
                    xCells = (int)(w * sqrtN);
                    yCells = (int)(h * sqrtN);
                }
                else {
                    // Zero area.
                    if (width > 0)
                        xCells = (int)(GRIDSCALE * GRIDSCALE * reflexVertCount);
                    else
                        yCells = (int)(GRIDSCALE * GRIDSCALE * reflexVertCount);
                }
                xCells = iclamp(xCells, 1, 256);
                yCells = iclamp(yCells, 1, 256);
            }

            reflexPointIndex = new GridIndexPoint(reflexBound, xCells, yCells);

            // Insert reflex verts into the index.
            vi = loop;
            for (;;) {
                PolyVert pvi = sortedVerts.get(vi);
                if (pvi.convexResult < 0)
                    // Reflex.  Insert it.
                    reflexPointIndex.add(new IndexPointFloat(pvi.v.getX(), pvi.v.getZ()), vi);

                vi = sortedVerts.get(vi).next;
                if (vi == loop)
                    break;
            }

            assert isValid(sortedVerts);
        }

        // Insert the edge (vi, vi.next) into the index.
        void addEdge(ArrayList<PolyVert> sortedVerts, int vi) {
            IndexBoxFloat ib = new IndexBoxFloat(sortedVerts.get(vi).getIndexPoint());
            ib.expandToEnclose(sortedVerts.get(sortedVerts.get(vi).next).getIndexPoint());

            assert edgeIndex != null;

            // Make sure edge isn't already in the index.
            assert edgeIndex.findPayloadFropoint(sortedVerts.get(vi).getIndexPoint(), vi) == null;

            edgeIndex.add(ib, vi);
        }

        // Remove the edge (vi, vi.next) from the index.
        void removeEdge(ArrayList<PolyVert> sortedVerts, int vi) {
            assert edgeIndex != null;

            GridEntryBox entry = edgeIndex.findPayloadFropoint(sortedVerts.get(vi).getIndexPoint(), vi);
            assert entry != null;

            edgeIndex.remove(entry);
        }

        // Return true if v can see coneAVert, without logically crossing coneB.
        // coneAVert and coneBVert are coincident.
        boolean	vertCanSeeConeA(ArrayList<PolyVert> sortedVerts, int v, int coneAVert, int coneBVert) {
            assert coordEquals(sortedVerts.get(coneAVert).v, sortedVerts.get(coneBVert).v);

            // @@ Thought: Would it be more robust to know whether v is
            // part of a ccw or cw loop, and then decide based on the
            // relative insideness/outsideness of v w/r/t the cones?

            // Analyze the two cones, to see if the segment
            // (v,coneAVert) is blocked by coneBVert.  Since
            // coneAVert and coneBVert are coincident, we need to
            // figure out the relationship among v and the cones.

            // Sort the cones so that they're in convex order.
            PolyVert pa = sortedVerts.get(coneAVert);
            MVVector[] coneA = new MVVector[] { sortedVerts.get(pa.prev).v, pa.v, sortedVerts.get(pa.next).v };
            if (vertexLeftTest(coneA[0], coneA[1], coneA[2]) < 0) {
                MVVector t = coneA[0];
                coneA[0] = coneA[2];
                coneA[2] = t;
            }
            PolyVert pb = sortedVerts.get(coneBVert);
            MVVector[] coneB = new MVVector[] { sortedVerts.get(pb.prev).v, pb.v, sortedVerts.get(pb.next).v };
            if (vertexLeftTest(coneB[0], coneB[1], coneB[2]) < 0) {
                MVVector t = coneB[0];
                coneB[0] = coneB[2];
                coneB[2] = t;
            }

            // Characterize the cones w/r/t each other.
            int aInBSum = 0;
            aInBSum += vertexLeftTest(coneB[0], coneB[1], coneA[0]);
            aInBSum += vertexLeftTest(coneB[1], coneB[2], coneA[0]);
            aInBSum += vertexLeftTest(coneB[0], coneB[1], coneA[2]);
            aInBSum += vertexLeftTest(coneB[1], coneB[2], coneA[2]);

            int bInASum = 0;
            bInASum += vertexLeftTest(coneA[0], coneA[1], coneB[0]);
            bInASum += vertexLeftTest(coneA[1], coneA[2], coneB[0]);
            bInASum += vertexLeftTest(coneA[0], coneA[1], coneB[2]);
            bInASum += vertexLeftTest(coneA[1], coneA[2], coneB[2]);

            // Eeek!  Need a better way of doing this...
            boolean aInB = false;
            if (aInBSum >= 4) {
                assert bInASum <= -2;
                aInB = true;
            }
            else if (aInBSum == 3) {
                assert bInASum <= 3;
                if (bInASum >= 3)
                    // Inconsistent (crossing cones).  No good.
                    return false;
                aInB = true;
            }
            else if (aInBSum <= -4) {
                assert bInASum >= 2;
                aInB = false;
            }
            else if (aInBSum == -3) {
                assert bInASum >= -3;
                if (bInASum <= -3) {
                    // Inconsistent (crossing cones).  No good.
                    return false;
                }
                aInB = false;
            }
            else {
                if (bInASum >= 4) {
                    assert aInBSum <= -2;
                    aInB = false;
                }
                else if (bInASum == 3) {
                    aInB = false;
                }
                else if (bInASum <= -4) {
                    assert aInBSum >= 2;
                    aInB = true;
                }
                else if (bInASum == -3) {
                    aInB = true;
                }
                else
                    // Inconsistent or coincident.  No good.
                    return false;
            }

            if (aInB) {
                assert aInB;

                boolean	vInA =
                    (vertexLeftTest(coneA[0], coneA[1], sortedVerts.get(v).v) > 0)
                    && (vertexLeftTest(coneA[1], coneA[2], sortedVerts.get(v).v) > 0);
                if (vInA)
                    return true;
                else
                    return false;
            }
            else
            {
                boolean	vInB =
                    (vertexLeftTest(coneB[0], coneB[1], sortedVerts.get(v).v) > 0)
                    && (vertexLeftTest(coneB[1], coneB[2], sortedVerts.get(v).v) > 0);
                if (vInB)
                    return false;
                else
                    return true;
            }
        }

        // Return true if edge (externalVert,myVert) intersects any edge in our poly.
        boolean	anyEdgeIntersection(ArrayList<PolyVert> sortedVerts, int externalVert, int myVert) {
            // Check the edge index for potentially overlapping edges.

            PolyVert pmv = sortedVerts.get(myVert);
            PolyVert pev = sortedVerts.get(externalVert);

            assert edgeIndex != null;

            IndexBoxFloat queryBox = new IndexBoxFloat(pmv.getIndexPoint());
            queryBox.expandToEnclose(pev.getIndexPoint());

            for (GridIndexBox.GridIndexBoxIterator it = edgeIndex.begin(queryBox);
                 ! it.atEnd();
                 it.advanceIfNotEnded()) {
                int vi = it.currentEntry.value;
                int vNext = sortedVerts.get(vi).next;

                if (vi != myVert) {
                    if (coordEquals(sortedVerts.get(vi).v, sortedVerts.get(myVert).v)) {
                        // Coincident verts.
                        if (vertCanSeeConeA(sortedVerts, externalVert, myVert, vi) == false)
                            // Logical edge crossing.
                            return true;
                    }
                    else if (edgesIntersect(sortedVerts, vi, vNext, externalVert, myVert))
                        return true;
                }
            }
            return false;
        }

        // Return true if any of this poly's reflex verts are inside the
        // specified ear.  The definition of inside is: a reflex vertex in the
        // interior of the triangle (v0,v1,v2), or on the segments [v1,v0) or
        // [v1,v2).
        boolean	earContainsReflexVertex(ArrayList<PolyVert> sortedVerts, int v0, int v1, int v2) {
            // Compute the bounding box of reflex verts we want to check.
            IndexBoxFloat queryBound = new IndexBoxFloat((IndexPointFloat)sortedVerts.get(v0).getIndexPoint().clone());
            queryBound.expandToEnclose(new IndexPointFloat(sortedVerts.get(v1).v.getX(), sortedVerts.get(v1).v.getZ()));
            queryBound.expandToEnclose(new IndexPointFloat(sortedVerts.get(v2).v.getX(), sortedVerts.get(v2).v.getZ()));

            for (GridIndexPoint.GridIndexPointIterator it = reflexPointIndex.begin(queryBound);
                 ! it.atEnd();
                 it.advanceIfNotEnded()) {
                int vk = it.currentEntry.value;

                PolyVert pvk = sortedVerts.get(vk);
                if (pvk.polyOwner != this)
                    // Not part of this poly; ignore it.
                    continue;

                if (vk != v0 && vk != v1 && vk != v2
                    && queryBound.containsPoint(new IndexPointFloat(pvk.v.getX(), pvk.v.getZ()))) {
                    int vNext = pvk.next;
                    int vPrev = pvk.prev;

                    if (coordEquals(pvk.v, sortedVerts.get(v1).v)) {
                        // Tricky case.  See section 4.3 in FIST paper.

                        // Note: I'm explicitly considering convex vk in here, unlike FIST.
                        // This is to handle the triple dupe case, where a loop validly comes
                        // straight through our cone.
                        //
                        // Note: the triple-dupe case is technically not a valid poly, since
                        // it contains a twist.
                        //
                        // @@ Fix this back to the FIST way?

                        int vPrevLeft01 = vertexLeftTest(sortedVerts.get(v0).v, sortedVerts.get(v1).v, sortedVerts.get(vPrev).v);
                        int vNextLeft01 = vertexLeftTest(sortedVerts.get(v0).v, sortedVerts.get(v1).v, sortedVerts.get(vNext).v);
                        int vPrevLeft12 = vertexLeftTest(sortedVerts.get(v1).v, sortedVerts.get(v2).v, sortedVerts.get(vPrev).v);
                        int vNextLeft12 = vertexLeftTest(sortedVerts.get(v1).v, sortedVerts.get(v2).v, sortedVerts.get(vNext).v);

                        if ((vPrevLeft01 > 0 && vPrevLeft12 > 0)
                            || (vNextLeft01 > 0 && vNextLeft12 > 0)) {
                            // Local interior near vk intersects this
                            // ear; ear is clearly not valid.
                            return true;
                        }

                        // Check colinear case, where cones of vk and v1 overlap exactly.
                        if ((vPrevLeft01 == 0 && vNextLeft12 == 0)
                            || (vPrevLeft12 == 0 && vNextLeft01 == 0)) {
                            // @@ TODO: there's a somewhat complex non-local area test that tells us
                            // whether vk qualifies as a contained reflex vert.
                            //
                            // For the moment, deny the ear.
                            //
                            // The question is, is this test required for correctness?  Because it
                            // seems pretty expensive to compute.  If it's not required, I think it's
                            // better to always assume the ear is invalidated.

                            //xxx
                            //log.error("findValidBridgeVert: colinear case in earContainsReflexVertex; returning true");

                            return true;
                        }
                    }
                    else {
                        assert pvk.convexResult < 0;
                        if (vertexInEar(pvk.v, sortedVerts.get(v0).v, sortedVerts.get(v1).v, sortedVerts.get(v2).v))
                            // Found one.
                            return true;
                    }
                }
            }
            // Didn't find any qualifying verts.
            return false;
        }



        // Returns true if vert is within the cone defined by [v0,v1,v2].
        /*
        //  (out)  v0
        //        /
        //    v1 <   (in)
        //        '
        //         v2
        */
        boolean	vertInCone(ArrayList<PolyVert> sortedVerts, int vert, int coneV0, int coneV1, int coneV2) {
            boolean acuteCone = vertexLeftTest(sortedVerts.get(coneV0).v, sortedVerts.get(coneV1).v, sortedVerts.get(coneV2).v) > 0;

            // Include boundary in our tests.
            boolean leftOf01 = vertexLeftTest(sortedVerts.get(coneV0).v, sortedVerts.get(coneV1).v, sortedVerts.get(vert).v) >= 0;
            boolean leftOf12 = vertexLeftTest(sortedVerts.get(coneV1).v, sortedVerts.get(coneV2).v, sortedVerts.get(vert).v) >= 0;

            if (acuteCone)
                // Acute cone.  Cone is intersection of half-planes.
                return leftOf01 && leftOf12;
            else
                // Obtuse cone.  Cone is union of half-planes.
                return leftOf01 || leftOf12;
        }

        // Return true if there's another vertex in this poly, coincident with vert.
        boolean vertIsDuplicated(ArrayList<PolyVert> sortedVerts, int vert) {
            // Scan backwards.
            for (int vi = vert - 1; vi >= 0; vi--) {
                if (coordEquals(sortedVerts.get(vi).v, sortedVerts.get(vert).v) == false)
                    // No more coincident verts scanning backward.
                    break;
                if (sortedVerts.get(vi).polyOwner == this)
                    // Found a dupe vert.
                    return true;
            }

            // Scan forwards.
            for (int vi = vert + 1, n = sortedVerts.size(); vi < n; vi++) {
                if (coordEquals(sortedVerts.get(vi).v, sortedVerts.get(vert).v) == false)
                    // No more coincident verts scanning forward.
                    break;
                if (sortedVerts.get(vi).polyOwner == this)
                    // Found a dupe vert.
                    return true;
            }
            // Didn't find a dupe.
            return false;
        }
    }

    //
    // PolyEnv: class that holds the state of a triangulation.
    //
    public class PolyEnv {
        ArrayList<PolyVert> sortedVerts = new ArrayList<PolyVert>();
        ArrayList<TriPoly> polys = new ArrayList<TriPoly>();

        IndexBoxFloat bound;
        int estimatedTriangleCount;

        public PolyEnv() {
            bound = new IndexBoxFloat(new IndexPointFloat(0, 0), new IndexPointFloat(0, 0));
            estimatedTriangleCount = 0;
        }

        // Initialize our state, from the given set of paths.  Sort vertices
        // and component polys.
        public void init(int pathCount, ArrayList<float[]> paths) {
            // Only call this on a fresh PolyEnv
            assert sortedVerts.size() == 0;
            assert polys.size() == 0;

            // Count total verts.
            int vertCount = 0;
            for (int i = 0; i < pathCount; i++)
                vertCount += paths.get(i).length;

            // Slight over-estimate; the true number depends on how many
            // of the paths are actually islands.
            estimatedTriangleCount = vertCount /* - 2 * pathCount */;

            // Collect the input verts and create polys for the input paths.
            sortedVerts.ensureCapacity(vertCount + (pathCount - 1) * 2); // verts, plus two duped verts for each path, for bridges
            polys.ensureCapacity(pathCount);

            for (int i = 0; i < pathCount; i++) {
                // Create a poly for this path.
                float[] path = paths.get(i);

                if (path.length < 3)
                    // Degenerate path, ignore it.
                    continue;

                TriPoly p = new TriPoly();
                polys.add(p);

                // Add this path's verts to our list.
                int pathSize = path.length;
                if ((path.length & 1) != 0) {
                    // Bad input, odd number of coords.
                    assert false;
                    log.error("pathEnv.init: path[" + i + "] has odd number of coords (" + path.length + ", dropping last value");
                    pathSize--;
                }
                for (int j = 0; j < pathSize; j += 2) { // vertex coords come in pairs.
                    int prevPoint = j - 2;
                    if (j == 0)
                        prevPoint = pathSize - 2;

                    if (path[j] == path[prevPoint] && path[j + 1] == path[prevPoint + 1])
                        // Duplicate point; drop it.
                        continue;

                    // Insert point.
                    int vertIndex = sortedVerts.size();

                    PolyVert vert = new PolyVert(path[j], path[j+1], p, vertIndex);
                    sortedVerts.add(vert);

                    p.appendVert(sortedVerts, vertIndex);

                    IndexPointFloat ip = new IndexPointFloat(vert.v.getX(), vert.v.getZ());
                    if (vertIndex == 0) {
                        // Initialize the bounding box.
                        bound.min = ip;
                        bound.max = (IndexPointFloat)ip.clone();
                    }
                    else
                        // Expand the bounding box.
                        bound.expandToEnclose(ip);
                    assert bound.containsPoint(ip);
                }
                assert p.isValid(sortedVerts);

                if (p.vertexCount == 0) {
                    // This path was degenerate; kill it.
                    polys.remove(polys.size() - 1);
                }
            }

            // Sort the vertices.
            Collections.sort(sortedVerts);
            assert sortedVerts.size() <= 1
                || compareVertices(sortedVerts.get(0),sortedVerts.get(1)) <= 0;	// check order

            // Remap the vertex indices, so that the polys and the
            // sortedVerts have the correct, sorted, indices.  We can
            // then use vert indices to judge the left/right relationship
            // of two verts.
            int[] vertRemap = new int[sortedVerts.size()];	// vertRemap[i] == new index of original vert[i]
            for (int i = 0, n = sortedVerts.size(); i < n; i++) {
                int newIndex = i;
                int originalIndex = sortedVerts.get(newIndex).myIndex;
                vertRemap[originalIndex] = newIndex;
            }
            for (int i = 0, n = sortedVerts.size(); i < n; i++)
                sortedVerts.get(i).remap(vertRemap);
            for (int i = 0, n = polys.size(); i < n; i++) {
                polys.get(i).remap(vertRemap);
                assert polys.get(i).isValid(sortedVerts);
            }
        }

	int getEstimatedTriangleCount() {
            return estimatedTriangleCount;
        }

        // Use zero-area bridges to connect separate polys & islands into one
        // big continuous poly.
        public void joinPathsIntoOnePoly() {
            // Connect separate paths with bridge edges, into one big path.
            //
            // Bridges are zero-area regions that connect a vert on each
            // of two paths.
            if (polys.size() > 1) {
                // Sort polys in order of each poly's leftmost vert.
                Collections.sort(polys);
                assert polys.size() <= 1
                    || comparePolysByLeftmostVert(polys.get(0), polys.get(1)) == -1;

                // assume that the enclosing boundary is the leftmost
                // path; this is true if the regions are valid and
                // don't intersect.
                TriPoly fullPoly = polys.get(0);

                fullPoly.initEdgeIndex(sortedVerts, bound);

                // Iterate from left to right
                while (polys.size() > 1) {
                    int v1 = polys.get(1).leftmostVert;
                    //     find v2 in fullPoly, such that:
                    //       v2 is to the left of v1,
                    //       and v1-v2 seg doesn't intersect any other edges

                    //     // (note that since v1 is next-most-leftmost, v1-v2 can't
                    //     // hit anything in p, or any paths further down the list,
                    //     // it can only hit edges in fullPoly) (need to think
                    //     // about equality cases)
                    //
                    int v2 = fullPoly.findValidBridgeVert(sortedVerts, v1);

                    //     once we've found v1 & v2, we use it to make a bridge,
                    //     inserting p into fullPoly
                    //
                    assert sortedVerts.get(v2).polyOwner == polys.get(0);
                    assert sortedVerts.get(v1).polyOwner == polys.get(1);
                    joinPathsWithBridge(fullPoly, polys.get(1), v2, v1);

                    // Drop the joined poly.
                    polys.remove(1);
                }
            }

            polys.get(0).initForEarClipping(sortedVerts);

            assert polys.size() == 1;
            // assert(all verts in sortedVerts have polys.get(0) as their owner);
        }

        // Absorb the sub-poly into the main poly, using a zero-area bridge
        // between the two given verts.
        public void joinPathsWithBridge(TriPoly mainPoly, TriPoly subPoly, int vertOnMainPoly, int vertOnSubPoly) {
            assert vertOnMainPoly != vertOnSubPoly;
            assert mainPoly != null;
            assert subPoly != null;
            assert mainPoly != subPoly;
            assert mainPoly == sortedVerts.get(vertOnMainPoly).polyOwner;
            assert subPoly == sortedVerts.get(vertOnSubPoly).polyOwner;

            if (coordEquals(sortedVerts.get(vertOnMainPoly).v, sortedVerts.get(vertOnSubPoly).v)) {
                // Special case: verts to join are coincident.  We
                // don't actually need to make a bridge with new
                // verts; we only need to adjust the links and do
                // fixup.
                PolyVert pvMain = sortedVerts.get(vertOnMainPoly);
                PolyVert pvSub = sortedVerts.get(vertOnSubPoly);

                int mainNext = pvMain.next;

                // Remove the edge we're about to break.
                mainPoly.removeEdge(sortedVerts, vertOnMainPoly);

                pvMain.next = pvSub.next;
                sortedVerts.get(pvMain.next).prev = vertOnMainPoly;

                pvSub.next = mainNext;
                sortedVerts.get(mainNext).prev = vertOnSubPoly;

                // Add edge that connects to sub poly.
                mainPoly.addEdge(sortedVerts, vertOnMainPoly);

                // Fixup sub poly so it's now properly a part of the main poly.
                mainPoly.updateConnectedSubPoly(sortedVerts, pvMain.next, mainNext);
                subPoly.invalidate(sortedVerts);

                return;
            }

            // Normal case, need to dupe verts and create zero-area bridge.
            dupeTwoVerts(vertOnMainPoly, vertOnSubPoly);

            // Fixup the old indices to account for the new dupes.
            if (vertOnSubPoly < vertOnMainPoly)
                vertOnMainPoly++;
            else
                vertOnSubPoly++;

            PolyVert pvMain = sortedVerts.get(vertOnMainPoly);
            PolyVert pvSub = sortedVerts.get(vertOnSubPoly);
            PolyVert pvMain2 = sortedVerts.get(vertOnMainPoly + 1);
            PolyVert pvSub2 = sortedVerts.get(vertOnSubPoly + 1);

            // Remove the edge we're about to break.
            mainPoly.removeEdge(sortedVerts, vertOnMainPoly);

            // Link the loops together.
            pvMain2.next = pvMain.next;
            pvMain2.prev = vertOnSubPoly + 1;	// (pvSub2)
            sortedVerts.get(pvMain2.next).prev = pvMain2.myIndex;

            pvSub2.prev = pvSub.prev;
            pvSub2.next = vertOnMainPoly + 1;	// (pvMain2)
            sortedVerts.get(pvSub2.prev).next = pvSub2.myIndex;

            pvMain.next = vertOnSubPoly;		// (pvSub)
            pvSub.prev = vertOnMainPoly;		// (pvMain)

            // Add edge that connects to sub poly.
            mainPoly.addEdge(sortedVerts, vertOnMainPoly);

            // Fixup sub poly so it's now properly a part of the main poly.
            mainPoly.updateConnectedSubPoly(sortedVerts, vertOnSubPoly, pvMain2.next);
            subPoly.invalidate(sortedVerts);

            assert pvMain.polyOwner.isValid(sortedVerts);
        }

        // Duplicate the two indexed verts, remapping polys & verts as necessary.
        void dupeTwoVerts(int v0, int v1) {
            // Order the verts.
            if (v0 > v1) {
                int t = v0;
                v0 = v1;
                v1 = t;
            }
            assert v0 < v1;

            // Duplicate verts.
            PolyVert v0Copy = (PolyVert)sortedVerts.get(v0).clone();
            PolyVert v1Copy = (PolyVert)sortedVerts.get(v1).clone();

            // @@ This stuff can be costly!  E.g. lots of separate little
            // polys that need bridges, with a high total vert count.

            // Insert v1 first, so v0 doesn't get moved.
            sortedVerts.add(v1 + 1, v1Copy);
            sortedVerts.add(v0 + 1, v0Copy);

            // Remap the indices within the verts.
            for (int i = 0, n = sortedVerts.size(); i < n; i++) {
                sortedVerts.get(i).myIndex = i;
                sortedVerts.get(i).next = remapIndexForDupedVerts(sortedVerts.get(i).next, v0, v1);
                sortedVerts.get(i).prev = remapIndexForDupedVerts(sortedVerts.get(i).prev, v0, v1);
            }

            // Remap the polys.
            for (int i = 0, n = polys.size(); i < n; i++) {
                polys.get(i).remapForDupedVerts(sortedVerts, v0, v1);
                assert polys.get(i).isValid(sortedVerts);
            }
        }

        //
        // Helpers.
        //

        // Fill *result with a poly loop representing P.
        void debugEmitPolyLoop(ArrayList<Float> result, ArrayList<PolyVert> sortedVerts, TriPoly P) {
            result.clear();	// clear existing junk.

            int firstVert = P.loop;
            int vi = firstVert;
            do {
                result.add(sortedVerts.get(vi).v.getX());
                result.add(sortedVerts.get(vi).v.getZ());
                vi = sortedVerts.get(vi).next;
            }
            while (vi != firstVert);

            // Loop back to beginning, and pad to a multiple of 3 coords.
            do {
                result.add(sortedVerts.get(vi).v.getX());
                result.add(sortedVerts.get(vi).v.getZ());
            }
            while (result.size() % 6 != 0);
        }

        // Compute triangulation.
        //
        // The debug_ args are optional; they're for terminating early and
        // returning the remaining loop to be triangulated.
        void computeTriangulation(ArrayList<Float> result, int pathCount, ArrayList<float[]> paths, 
            int debugHaltStep, ArrayList<Float> debugRemainingLoop) {
            Random rg = new Random();
            if (pathCount <= 0)
                // Empty paths -. no triangles to emit.
                return;

            long startTicks = System.currentTimeMillis();
            
            // Poly environment; most of the state of the algo.
            PolyEnv penv = new PolyEnv();

            penv.init(pathCount, paths);

            penv.joinPathsIntoOnePoly();

            result.ensureCapacity(2 * 3 * penv.getEstimatedTriangleCount());

            long joinTicks = System.currentTimeMillis();

            // Debugging only: dump coords of joined poly.
            boolean debugDumpJoinedPoly = false;
            if (debugDumpJoinedPoly) {
                int firstVert = penv.polys.get(0).loop;
                int vi = firstVert;
                do {
                    log.info(penv.sortedVerts.get(vi).v.getX() + ", " + penv.sortedVerts.get(vi).v.getZ());
                    vi = penv.sortedVerts.get(vi).next;
                }
                while (vi != firstVert);
            }
        
            boolean debugEmitJoinedPoly = false;
            if (debugEmitJoinedPoly) {
                int firstVert = penv.polys.get(0).loop;
                int vi = firstVert;
                do {
                    result.add(penv.sortedVerts.get(vi).v.getX());
                    result.add(penv.sortedVerts.get(vi).v.getZ());
                    vi = penv.sortedVerts.get(vi).next;
                }
                while (vi != firstVert);

                // Loop back to beginning, and pad to a multiple of 3 coords.
                do {
                    result.add(penv.sortedVerts.get(vi).v.getX());
                    result.add(penv.sortedVerts.get(vi).v.getZ());
                }
                while (result.size() % 6 != 0);
                return;
            }

            // ear-clip, adapted from FIST paper:
            //
            //   list<poly> L;
            //   L.insert(fullPoly)
            //   while L not empty:
            //     P = L.pop()
            //     Q = priority queue of ears of P
            //     while P.vertCount > 3 do:
            //       if Q not empty:
            //         e = Q.pop
            //         emit e
            //         update P by deleting e
            //       else if an ear was clipped in previous pass then:
            //         Q = priority queue of ears of P (i.e. reexamine P)
            //       else
            //         // we're stuck
            //         recoveryProcess()	// do something drastic to make the next move
            //     emit last 3 verts of P as the final triangle

            while (penv.polys.size() != 0) {
                TriPoly P = penv.polys.remove(penv.polys.size() - 1);
                P.buildEarList(penv.sortedVerts, rg);
                boolean earWasClipped = false;
                while (P.getVertexCount() > 3) {
                    if (P.getEarCount() > 0) {
                        // Clip the next ear from Q.
                        int v1 = P.getNextEar(penv.sortedVerts, rg);
                        int v0 = penv.sortedVerts.get(v1).prev;
                        int v2 = penv.sortedVerts.get(v1).next;

                        P.emitAndRemoveEar(result, penv.sortedVerts, v0, v1, v2);
                        earWasClipped = true;

                        // For debugging -- terminate early if the debug counter hits zero.
                        debugHaltStep--;
                        if (debugHaltStep == 0) {
                            if (debugRemainingLoop != null) {
                                debugEmitPolyLoop(debugRemainingLoop, penv.sortedVerts, P);
                            }
                            return;
                        }
                    }
                    else if (earWasClipped == true)
                        // Re-examine P for new ears.
                        earWasClipped = P.buildEarList(penv.sortedVerts, rg);
                    else {
                        // No valid ears; we're in trouble so try some fallbacks.

                        boolean debugSkipRecovery = true;
                        if (debugSkipRecovery) {
                            // xxx hack for debugging: show the state of P when we hit the recovery process.
                            debugEmitPolyLoop(result, penv.sortedVerts, P);
                            return;
                        }
                        recoveryProcess(penv.polys, P, penv.sortedVerts, rg);
                        earWasClipped = false;
                    }
                }

                if (P.getVertexCount() == 3) {
                    // Emit the final triangle.
                    if (penv.sortedVerts.get(P.loop).isEar == false) {
                        // Force an arbitrary vert to be an ear.
                        penv.sortedVerts.get(P.loop).isEar = true;
                        P.earCount++;
                    }
                    P.emitAndRemoveEar(result, penv.sortedVerts, penv.sortedVerts.get(P.loop).prev,
                        P.loop, penv.sortedVerts.get(P.loop).next);
                }
            }

            if (debugProfileTriangulate) {
                long clipTicks = System.currentTimeMillis();
                log.info("computeTriangulation: clip poly = " + (clipTicks - joinTicks) + "ms");
                log.info("computeTriangulation: total for poly " + (clipTicks - startTicks) + "ms");
//                 log.info("computeTriangulation: vert count = " + inputVertCount + ", verts clipped / sec = " +
//                     (inputVertCount / (1000 * (clipTicks - joinTicks))) + "\n");
            }

            assert penv.polys.size() == 0;
            // assert(for all penv.sortedVerts: owning poly == null);
            assert (result.size() % 6) == 0;
        }



        // recoveryProcess:
        //   if two edges in P, e[i-1] and e[i+1] intersect:
        //     insert two tris incident on e[i-1] & e[i+1] as ears into Q
        //   else if P can be split with a valid diagonal:
        //     P = one side
        //     L += the other side
        //     Q = ears of P
        //   else if P has any convex vertex:
        //     pick a random convex vert and add it to Q
        //   else
        //     pick a random vert and add it to Q
        private void recoveryProcess(ArrayList<TriPoly> polys, TriPoly P, ArrayList<PolyVert> sortedVerts, Random rg) {
            // Case 1: two edges, e[i-1] and e[i+1], intersect; we insert
            // the overlapping ears into Q and resume.
            for (int vi = sortedVerts.get(P.loop).next; vi != P.loop; vi = sortedVerts.get(vi).next) {
                int ev0 = vi;
                int ev1 = sortedVerts.get(ev0).next;
                int ev2 = sortedVerts.get(ev1).next;
                int ev3 = sortedVerts.get(ev2).next;
                if (edgesIntersect(sortedVerts, ev0, ev1, ev2, ev3)) {
                    // Insert (1,2,3) as an ear.
                    sortedVerts.get(ev2).isEar = true;
                    P.earCount++;
                    log.error("recoveryProcess: self-intersecting sequence, treating " + ev2 + " as an ear");

                    // Resume regular processing.
                    return;
                }
            }

            // Deviation from FIST: Because I'm lazy, I'm skipping this test for
            // now...
            //
            // @@ This seems to be helpful for doing reasonable things in case the
            // input is a little bit self-intersecting.  Otherwise, clipping any
            // old convex or random vert can create crazy junk in the
            // triangulation.  It's probably worth implementing at some point.
//             boolean debugSplitOnDiagonal = false;
//             if (debugSplitOnDiagonal) {
//                 // Case 2: P can be split with a valid diagonal.
//                 //
//                 // A "valid diagonal" passes these checks, according to FIST:
//                 //
//                 // 1. diagonal is locally within poly
//                 //
//                 // 2. its relative interior does not intersect any edge of the poly
//                 //
//                 // 3. the winding number of the polygon w/r/t the midpoint of
//                 // the diagonal is one
//                 //
//                 for (int vi=0; vi<=end; vi++) {
//                     for (int vj=vi.next; vj<=end; vj++) {
//                         if (P.validDiagonal(vi, vj)) {
//                             // Split P, insert leftover piece into polys
//                             poly leftover = P.split(vi, vj);
//                             polys.add(leftover);

//                             // Resume regular processing.
//                             return;
//                         }
//                     }
//                 }
//             }

            // Case 3: P has any convex vert
            int firstVert = P.loop;
            int vi = firstVert;
            int vertCount = 0;
            do {
                if (isConvexVert(sortedVerts, vi)) {
                    // vi is convex; treat it as an ear,
                    // regardless of other problems it may have.
                    sortedVerts.get(vi).isEar = true;
                    P.earCount++;

                    log.error("PolyEnv.recoveryProcess: found convex vert, treating " + vi + " as an ear");

                    // Resume regular processing.
                    return;
                }
                vertCount++;
                vi = sortedVerts.get(vi).next;
            }
            while (vi != firstVert);

            // Case 4: Pick a random vert and treat it as an ear.
            int randovert = (int)(rg.nextLong() % vertCount);
            for (vi = firstVert; randovert > 0; randovert--)
                vi = sortedVerts.get(vi).next;

            sortedVerts.get(vi).isEar = true;
            P.earCount++;

            log.error("PolyEnv.recoveryProcess: treating random vert " + vi + " as an ear");

            // Resume.
            return;
        }
    }

    private static final long serialVersionUID = 1L;

    private static void test1(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test1");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 250f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test1", boundary, obstacles);
        log.info("");
    }
    
    private static void test2(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test2");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(500f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 500f));
        corners.add(new MVVector(500f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test2", boundary, obstacles);
        log.info("");
    }
    
    private static void test3(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test3");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(200f, 0f, 400f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(500f, 0f, 800f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(800f, 0f, 500f));
        corners.add(new MVVector(750f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 200f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test3", boundary, obstacles);
        log.info("");
    }
    
    /**
     * Try a non-convex obstacle
     */
    private static void test4(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test4");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 250f));
        corners.add(new MVVector(500f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test4", boundary, obstacles);
        log.info("");
    }
    
    /**
     * Try a non-convex obstacle and pentagonal boundary
     */
    private static void test5(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test5");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(500f, 0f, 1250f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 250f));
        corners.add(new MVVector(500f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test5", boundary, obstacles);
        log.info("");
    }
    
    /**
     * Try a non-convex obstacle and non-convex boundary
     */
    private static void test6(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test6");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(500f, 0f, 250f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(500f, 0f, 1250f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 250f));
        corners.add(new MVVector(500f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test6", boundary, obstacles);
        log.info("");
    }
    
    /**
     * Try a rectangular obstacle and square boundary with the
     * boundary edge overlapping with the boundary edge.  This test
     * fails :-)
     */
    private static void test7(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test7");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 1250f));
        corners.add(new MVVector(750f, 0f, 1250f));
        corners.add(new MVVector(750f, 0f, 250f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test7", boundary, obstacles);
        log.info("");
    }
    
    /**
     * Try a rectangular obstacle and square boundary with the
     * boundary edge overlapping with the boundary edge.
     */
    private static void test8(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test8");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 1000f));
        corners.add(new MVVector(750f, 0f, 1000f));
        corners.add(new MVVector(750f, 0f, 250f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test8", boundary, obstacles);
        log.info("");
    }
    
    // Try non-overlapping obstacles
    private static void test9(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test9");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 400f));
        corners.add(new MVVector(750f, 0f, 400f));
        corners.add(new MVVector(750f, 0f, 250f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 500f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test9", boundary, obstacles);
        log.info("");
    }
    
    // Try overlapping obstacles - - fails!
    private static void test10(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test10");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 500f));
        corners.add(new MVVector(750f, 0f, 500f));
        corners.add(new MVVector(750f, 0f, 250f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 400f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 400f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test10", boundary, obstacles);
        log.info("");
    }
    
    // Try obstacles whose boundaries coincide
    private static void test11(Triangulate triangulator) {
        log.info("PathSynth.main: Starting test11");
        // Create a boundary polygon - - must be CCW
        List<MVVector> corners = new LinkedList<MVVector>();
        corners.add(new MVVector(0f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 0f));
        corners.add(new MVVector(1000f, 0f, 1000f));
        corners.add(new MVVector(0f, 0f, 1000f));
        PathPolygon boundary = new PathPolygon(0, PathPolygon.CV, corners);
        // Create an obstacle - - must be CW
        List<PathPolygon> obstacles = new LinkedList<PathPolygon>();
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 250f));
        corners.add(new MVVector(250f, 0f, 500f));
        corners.add(new MVVector(750f, 0f, 500f));
        corners.add(new MVVector(750f, 0f, 250f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        corners = new LinkedList<MVVector>();
        corners.add(new MVVector(250f, 0f, 500f));
        corners.add(new MVVector(250f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 750f));
        corners.add(new MVVector(750f, 0f, 500f));
        obstacles.add(new PathPolygon(0, PathPolygon.CV, corners));
        triangulator.computeTriangulation("test11", boundary, obstacles);
        log.info("");
    }
    
    /**
     * Calls a set of test cases for the generation of polygons and
     * arcs from a boundary and obstacle.
     * </p>
     * Executive summary: the only thing that seems to mess things up
     * is if the obstacle(s) overlap the boundary, or the obstacles
     * overlap each other.
     */
    public static void main(String[] args) {
        Properties props = new Properties();
        props.put("log4j.appender.FILE", "org.apache.log4j.RollingFileAppender");
        props.put("log4j.appender.FILE.File", "${multiverse.logs}/pathing.out");
        props.put("log4j.appender.FILE.MaxFileSize", "50MB");
        props.put("log4j.appender.FILE.layout", "org.apache.log4j.PatternLayout");
        props.put("log4j.appender.FILE.layout.ConversionPattern", "%-5p %m%n");
        props.put("multiverse.log_level", "0");
        props.put("log4j.rootLogger", "DEBUG, FILE");
        Log.init(props);
        Triangulate triangulator = new Triangulate();
        
        // All tests succeed except test7, which has the obstacle
        // overlapping the boundary
        test1(triangulator);
        test2(triangulator);
        test3(triangulator);
        test4(triangulator);
        test5(triangulator);
        test6(triangulator);
        test7(triangulator); // Obstacle overlapping boundary fails!!!
        test8(triangulator);
        test9(triangulator);
        test10(triangulator); // Overlapping obstacles Fails!!!
        test11(triangulator);
    }

}
