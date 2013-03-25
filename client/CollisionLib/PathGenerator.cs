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

#region Using directives

using System;
using Vector3 = Axiom.MathLib.Vector3;
using Matrix4 = Axiom.MathLib.Matrix4;
using AxisAlignedBox = Axiom.MathLib.AxisAlignedBox;
using Plane = Axiom.MathLib.Plane;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

#endregion

namespace Multiverse.CollisionLib
{
    
    ///<summary>
    ///    This class embodies the functionality to determine legal
    ///    paths in model.
    ///</summary>
    public class PathGenerator {

        #region Fields

        protected static float oneMeter = 1000f;

        ///<summary>
        ///    A distinguished value to identify neighbor slopes that
        ///    are uninitialized
        ///</summary>
        protected static float slopeNone = 1.0e20f;
        
        ///<summary>
        ///    Used in the grid dump
        ///</summary>
        protected string modelName;
        
        ///<summary>
        ///    The path object type for this model
        ///</summary>
        protected PathObjectType poType;
        
        ///<summary>
        ///    The width of a cell. The literature suggests that the
        ///    appropriate grid resolution is 1/4th of the width of
        ///    the moving mob or player.
        ///</summary>
        protected float cellWidth;

        ///<summary>
        ///    The width of the model
        ///</summary>
        protected float modelWidth;

        ///<summary>
        ///    The moving box height, corresponding to the height of a
        ///    player or mob for which paths are being computed
        ///</summary>
        protected float boxHeight;

        ///<summary>
        ///    The maximum change in height the player or mob could
        ///    climb moving from one cell to its neighbor. 
        ///</summary>
        protected float maxClimbDistance;

        ///<summary>
        ///    A fudge factor distance that allows for the fact that
        ///    collision volumes won't exactly fit together 
        ///</summary>
        protected float maxDisjointDistance;

        ///<summary>
        ///    The level of the terrain
        ///</summary>
        protected float terrainLevel;

        ///<summary>
        ///    The minimum number of grid cells width and length for a
        ///    region of cells such that it won't be filtered out.  3
        ///    or 4 is are good candidate values
        ///</summary>
        protected int minimumFeatureSize;

        ///<summary>
        ///    The number of cell neighbors to move in each of the
        ///    four directions when determining neighbor slopes
        ///</summary>
        protected int slopeCellCount = 1;

        ///<summary>
        ///    Whether to dump the grid to a log file
        ///</summary>
        protected Matrix4 modelTransform;

        ///<summary>
        ///    The collision volumes for the model
        ///</summary>
        protected List<CollisionShape> collisionVolumes;
        
        ///<summary>
        ///    The array of lists of cells
        ///</summary>
        internal List<GridCell>[,] grid;
        
        ///<summary>
        ///    Collision volume polygons
        ///</summary>
        internal List<GridPolygon> cvPolygons;
        
        ///<summary>
        ///    Terrain polygons
        ///</summary>
        internal List<GridPolygon> terrainPolygons;
        
        ///<summary>
        ///    Terrain "portals" connecting model polygons to the
        ///    terrain 
        ///</summary>
        internal List<PolygonArc> terrainPortals;
        
        ///<summary>
        ///    The "arc" that connect a pair of polygons along a
        ///    specified section of their perimeters
        ///</summary>
        internal List<PolygonArc> polygonArcs;
        
        ///<summary>
        ///    The number of cells horizontally and vertically
        ///</summary>
        protected int xCount;
        protected int zCount;
        
        ///<summary>
        ///    The union of collision volume bounding boxes
        ///</summary>
        protected AxisAlignedBox gridBox;

        ///<summary>
        ///    The list of cells that have been initialized, but whose
        ///    neighbors haven't (necessarily) been traversed
        ///</summary>
        internal List<CellLocation> workList;

        ///<summary>
        ///    A list of the vertices created by edges
        ///</summary>
        // internal List<CellLocation> edgeVertices; (unused)

        ///<summary>
        ///    The moving box that traverses the grid
        ///</summary>
        protected CollisionAABB movingBox;

        ///<summary>
        ///    The moving object that contains the box
        ///</summary>
        protected MovingObject movingObject;

        ///<summary>
        ///    The api object, which holds the sphere tree
        ///</summary>
        protected CollisionAPI collisionAPI;
        
        ///<summary>
        ///    The increments in grid locations, corresponding to the
        ///    GridDirections enumeration
        ///</summary>
        protected int [] XIncrement = new int[] { 1, 0, -1, 0 };
        protected int [] ZIncrement = new int[] { 0, 1, 0, -1 };
        
        ///<summary>
        ///    The lower-left corner of the grid in model coordinates
        ///</summary>
        protected Vector3 lowerLeftCorner;

        ///<summary>
        ///    The upper-right corner of the grid in model coordinates
        ///</summary>
        protected Vector3 upperRightCorner;

        ///<summary>
        ///    The number of cells processed by the traversal algorithm
        ///</summary>
        protected int cellsProcessed;

        ///<summary>
        ///    The number of cells supported by collision volumes
        ///</summary>
        protected int cellsOnCVs;

        ///<summary>
        ///    The last index used for polygons - - we use the same
        ///    index sequence for both CV and Terrain polygons
        ///</summary>
        protected int polygonIndex = 1;

        ///<summary>
        ///    The index of the first terrain polygon
        ///</summary>
        protected int firstTerrainIndex;

        ///<summary>
        ///    Whether to dump the grid to a log file
        ///</summary>
        protected bool dumpGrid;
        
        ///<summary>
        ///    Should the dump for a model rewrite the file, or append
        ///    to it?
        ///</summary>
        protected bool appendGridDump = true;
        
        ///<summary>
        ///    A stopwatch to provide times to print in the log
        ///</summary>
        protected Stopwatch stopwatch;
        
        #endregion Fields

        #region Constructor
        
        public PathGenerator(bool logPathGeneration, string modelName, PathObjectType poType, float terrainLevel, 
                             Matrix4 modelTransform, List<CollisionShape> collisionVolumes) {
            this.dumpGrid = logPathGeneration;
            this.modelName = modelName;
            this.poType = poType;
            this.modelWidth = poType.width * oneMeter;
            this.cellWidth = poType.gridResolution * modelWidth;
            this.boxHeight = poType.height * oneMeter;
            this.maxClimbDistance = poType.maxClimbSlope * cellWidth;
            this.maxDisjointDistance = poType.maxDisjointDistance * oneMeter;
            this.minimumFeatureSize = poType.minimumFeatureSize;
            this.terrainLevel = terrainLevel;
            this.modelTransform = modelTransform;
            this.collisionVolumes = collisionVolumes;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Create the collisionAPI object
            collisionAPI = new CollisionAPI(false);

            // Ugly workaround for a modularity problem do to
            // unadvised use of static variables: remember the
            // existing state of rendering collision volumes
            RenderedNode.RenderState oldRenderState = RenderedNode.renderState;
            RenderedNode.renderState = RenderedNode.RenderState.None;

            // Form the union of the collision volumes; we don't care
            // about Y coordinate, only the X and Z coordinates
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            for (int i=0; i<collisionVolumes.Count; i++) {
                CollisionShape shape = collisionVolumes[i];
                // Add the shape to the sphere tree
                collisionAPI.SphereTree.AddCollisionShape(shape);
                AxisAlignedBox vol = shape.BoundingBox();
                // If this is the first iteration, set up the min and
                // max
                if (i == 0) {
                    min = vol.Minimum;
                    max = vol.Maximum;
                    min.y = terrainLevel;
                    max.y = terrainLevel;
                    continue;
                }
                // Enlarge the box by the dimensions of the shape
                min.x = Math.Min(min.x, vol.Minimum.x);
                min.z = Math.Min(min.z, vol.Minimum.z);
                max.x = Math.Max(max.x, vol.Maximum.x);
                max.z = Math.Max(max.z, vol.Maximum.z);
            }

            // Restore RenderState
            RenderedNode.renderState = oldRenderState;
            
            // Round out the max and min by 4 times the grid
            // resolution, so we can just divide to find the
            // horizontal and vertical numbers are cells
            Vector3 margin = Vector3.UnitScale * 4 * cellWidth;
            min -= margin;
            max += margin;
            
            // Now adjust the max the min coords so that they are
            // exactly a multiple of the grid resolution
            min.x = MultipleOfResolution(min.x);
            min.z = MultipleOfResolution(min.z);
            max.x = MultipleOfResolution(max.x);
            max.z = MultipleOfResolution(max.z);

            // Set the lower left  and upper right corners
            lowerLeftCorner = min;
            upperRightCorner = max;
            
            // Set the horizontal and vertical counts
            xCount = (int)((max.x - min.x) / cellWidth);
            zCount = (int)((max.z - min.z) / cellWidth);

            // Initial the gridBox
            gridBox = new AxisAlignedBox(min, max);
            
            // Allocate the grid
            grid = new List<GridCell>[xCount, zCount];
            for (int i=0; i<xCount; i++)
                for (int j=0; j<zCount; j++)
                    grid[i,j] = new List<GridCell>();
            
            // Initialize the work list, adding the cell at 0,0 and
            // the terrain height as the first member
            workList = new List<CellLocation>();
            workList.Add(new CellLocation(0, 0, terrainLevel, null)); 

            // Create the moving box at the center of the 0, 0 cell,
            // and the MovingObject object that contains the moving box
            movingBox = new CollisionAABB(
                new Vector3(min.x, terrainLevel, min.z),
                new Vector3(min.x + cellWidth, terrainLevel + boxHeight, min.z + terrainLevel));
            movingObject = new MovingObject(collisionAPI);
            movingObject.AddPart(movingBox);
        }

        #endregion Constructor

        #region Properties

        ///<summary>
        ///    The name of the model
        ///</summary>
        public string ModelName {
            get { return modelName; }
        }

        ///<summary>
        ///    The index of the first terrain polygon
        ///</summary>
        public int FirstTerrainIndex {
            get { return firstTerrainIndex; }
        }

        ///<summary>
        ///    Collision volume polygons
        ///</summary>
        public List<GridPolygon> CVPolygons {
            get { return cvPolygons; }
        }
        
        ///<summary>
        ///    Terrain polygons
        ///</summary>
        public List<GridPolygon> TerrainPolygons {
            get { return terrainPolygons; }
        }
        
        ///<summary>
        ///    Terrain "portals" connecting model polygons to the
        ///    terrain 
        ///</summary>
        public List<PolygonArc> TerrainPortals {
            get { return terrainPortals; }
        }
        
        ///<summary>
        ///    The "arc" that connect a pair of polygons along a
        ///    specified section of their perimeters
        ///</summary>
        public List<PolygonArc> PolygonArcs {
            get { return polygonArcs; }
        }

        ///<summary>
        ///    The bounding polygon of the model - - for now, just the
        ///    rectangle.
        ///</summary>
        public Vector3 [] ModelCorners {
            get { 
                Vector3 [] corners = new Vector3[4];
                float avgY = (lowerLeftCorner.y + upperRightCorner.y) * 0.5f;
                Vector3 ll = lowerLeftCorner;
                Vector3 ur = upperRightCorner;
                corners[0] = modelTransform * ll;
                corners[1] = modelTransform * new Vector3(ll.x, avgY, ur.z);
                corners[2] = modelTransform * ur;
                corners[3] = modelTransform * new Vector3(ur.x, avgY, ll.z);
                return  corners;
            }
        }
        
        ///<summary>
        ///    The upper right corner of the bounding box
        ///</summary>
        public Vector3 UpperRight {
            get { return modelTransform * upperRightCorner; }
        }
        
        #endregion Properties

        #region Methods
        
        // Perform traversal and creation of polygons, arcs
        // between polygons, and portals to the terrain
        public void GeneratePolygonsArcsAndPortals() {
            DumpCurrentTime("Before TraverseGridCells");
            // Create the grid
            TraverseGridCells();
            // Extend the inaccessible regions by half the model width
            ExtendInaccessibleRegions();
            // Filter the grid
            FilterGrid();
            // Find the polygons
            DiscoverPolygons();
            // Find the portals from the terrain
            FindTerrainPortals();
            // Determine the arcs that join polygons
            FindPolygonArcs();
        }

        ///<summary>
        ///    Run the grid traversal algorithm, pushing the cells
        ///    around the model
        ///</summary>
        protected void TraverseGridCells() {
            cellsProcessed = 0;
            cellsOnCVs = 0;
            Stopwatch collisionStopwatch = new Stopwatch();
            int collisionCount = 0;
            // Create the CollisionParms object
            CollisionParms parms = new CollisionParms();
            // Set the big step size to 5% of the box height; this
            // will make the small step size .5% of the box height.
            // For a box height of 1.8 meters, this is .009m, or 9mm
            // Since the grid resolution is .25 of model width, and
            // for the human model, that width is .5m, the cell width
            // is .125m or 125mm.  So .009 / .125 = .072, so the
            // maximum variation in slope due exclusively to the step
            // size is 7.2%
            float stepSize = boxHeight * .05f;
            // Iterate over work list items until there aren't any
            while (workList.Count > 0) {
                CellLocation loc = workList[0];
                workList.RemoveAt(0);
                GridCell cell = FindCellAtHeight(loc);
                if (cell != null)
                    // Skip, because it's already been visited
                    continue;
                // Position the moving object over the cell
                SetMovingBoxOverCell(loc);
                // If we're above terrain level, we need to drop the
                // cell.  If we're at or below terrain level, we just
                // mark the cell as supported by the terrain
                float distanceToTerrain = movingBox.min.y - terrainLevel;
                if (distanceToTerrain <= 0f) {
                    loc.height = terrainLevel;
                    if (FindCellAtHeight(loc) != null)
                        continue;
                    cell = new GridCell(loc);
                    cell.status = CellStatus.OverTerrain;
                }
                else {
                    // Now drop it until it hits a collision object, or
                    // it's at terrain level.  Note: this means that we
                    // can't have "basements" until we find a way to have
                    // a non-constant terrain level
                    Vector3 displacement = new Vector3(0, -distanceToTerrain, 0);
                    collisionCount++;
                    collisionStopwatch.Start();
                    bool hit = collisionAPI.TestCollision(movingObject, stepSize, ref displacement, parms);
                    collisionStopwatch.Stop();
                    float oldHeight = loc.height;
                    loc.height = movingBox.min.y;
                    if (FindCellAtHeight(loc) != null)
                        continue;
                    cell = new GridCell(loc);
                    if (hit) {
                        // We hit a collision object - - if it's below
                        // us, then set the height accordingly.  If
                        // it's not below us, mark the cell as inaccessible.
                        if (displacement.y != -distanceToTerrain) {
                            cell.status = CellStatus.OverCV;
                            cell.supportingShape = parms.obstacle;
                            cellsOnCVs++;
                        } 
                        else {
                            loc.height = oldHeight;
                            if (FindCellAtHeight(loc) != null)
                                continue;
                            cell.loc.height = oldHeight;
                            cell.status = CellStatus.Inaccessible;
                        }
                    } else {
                        loc.height = terrainLevel;
                        cell.loc.height = terrainLevel;
                        cell.status = CellStatus.OverTerrain;
                        if (FindCellAtHeight(loc) != null)
                            continue;
                    }
                }
                // Add the cell to the grid, now that we know its
                // actual height
                cellsProcessed++;
                grid[loc.xCell, loc.zCell].Add(cell);
                if (cell.status == CellStatus.Inaccessible)
                    continue;
                // Now add the neighbors to the work list, if they
                // haven't already been visited
                for (GridDirection dir = GridDirection.PlusX; dir <= GridDirection.MinusZ; dir++) {
                    int neighborX = loc.xCell + XIncrement[(int)dir];
                    int neighborZ = loc.zCell + ZIncrement[(int)dir];
                    // If the neighbor is outside the grid, ignore it
                    if (neighborX < 0 || neighborX >= xCount ||
                        neighborZ < 0 || neighborZ >= zCount)
                        continue;
                    // Test to see if it exists; if so, it's been visited
                    CellLocation neighborLoc = new CellLocation(neighborX, neighborZ, cell.loc.height, loc);
                    GridCell neighborCell = FindCellAtHeight(neighborLoc);
                    // If it doesn't exist, add it to the work queue
                    if (neighborCell == null)
                        workList.Add(neighborLoc);
                        //AddToWorkList(neighborLoc);
                }
            }
            DumpCurrentTime(string.Format("Processing {0} box drops, {1} collision tests, took {2} ms", 
                    collisionCount, collisionAPI.partCalls, collisionStopwatch.ElapsedMilliseconds));
            DumpGrid("Prefiltered Grid", GridDumpKind.Cells, false);
        }

        ///<summary>
        ///    Extend the inaccessible regions by an amount equal to
        ///    half the model width, so that we don't have to do
        ///    kludgely things at path interpolation time to avoid
        ///    grazing walls of buildings
        ///</summary>
        protected void ExtendInaccessibleRegions() {
            // The model width, in cell units
            int minWidth = (int)((modelWidth + 0.5f * cellWidth) / cellWidth);
            int halfMinWidth = minWidth / 2;

            // Extend inaccessible regions vertically
            MarkAllCellsUnused();
            for (int j=1; j<zCount-1; j++) {
                for (int i=0; i<xCount; i++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.used)
                            continue;
                        if (cell.status == CellStatus.Inaccessible) {
                            GridCell neighbor = NextCellAtHeight(cell, GridDirection.MinusZ);
                            if (neighbor != null && neighbor.status != CellStatus.Inaccessible)
                                MarkNeighborsInaccessible(cell, GridDirection.MinusZ, halfMinWidth);
                            neighbor = NextCellAtHeight(cell, GridDirection.PlusZ);
                            if (neighbor != null && neighbor.status != CellStatus.Inaccessible)
                                MarkNeighborsInaccessible(cell, GridDirection.PlusZ, halfMinWidth);
                        }
                    }
                }
            }

            // Extend inaccessible regions horizontally
            MarkAllCellsUnused();
            for (int i=1; i<xCount-1; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.used)
                            continue;
                        if (cell.status == CellStatus.Inaccessible) {
                            GridCell neighbor = NextCellAtHeight(cell, GridDirection.MinusX);
                            if (neighbor != null && neighbor.status != CellStatus.Inaccessible)
                                MarkNeighborsInaccessible(cell, GridDirection.MinusX, halfMinWidth);
                            neighbor = NextCellAtHeight(cell, GridDirection.PlusX);
                            if (neighbor != null && neighbor.status != CellStatus.Inaccessible)
                                MarkNeighborsInaccessible(cell, GridDirection.PlusX, halfMinWidth);
                        }
                    }
                }
            }
        }
        
        internal void MarkNeighborsInaccessible(GridCell cell, GridDirection direction, int count) {
            cell.used = true;
            GridCell temp = cell;
            for (int i=0; i<count; i++) {
                temp = NextCellAtHeight(temp, direction);
                if (temp == null)
                    return;
                temp.used = true;
                temp.status = CellStatus.Inaccessible;
            }
        }
            
        internal static int [,] incrByDirection = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };

        internal GridCell NextCellAtHeight(GridCell cell, GridDirection direction) {
            int i = cell.loc.xCell + incrByDirection[(int)direction, 0];
            int j = cell.loc.zCell + incrByDirection[(int)direction, 1];
            if (i < 0 || i >= xCount || j < 0 || j >= zCount)
                return null;
            GridCell other = FindCellAtHeight(i, j, cell.loc.height);
            if (other != null)
                return other;
            else
                return null;
        }
        
        internal void MarkAllCellsUnused() {
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        cell.used = false;
                    }
                }
            }
        }
        
        ///<summary>
        ///    Filter out small features of cells collision volumes,
        ///    because features smaller than minimumFeatureSize are not
        ///    places a mob or player could stand 
        ///</summary>
        protected void FilterGrid() {
            List<GridCell> goners = new List<GridCell>();
            for (int i=0; i<xCount - minimumFeatureSize; i++) {
                for (int j=0; j<zCount - minimumFeatureSize; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.status == CellStatus.OverCV) {
                            if ((FeatureSpan(cell, minimumFeatureSize, 1, 0) +
                                 FeatureSpan(cell, minimumFeatureSize, -1, 0) + 1) < minimumFeatureSize ||
                                (FeatureSpan(cell, minimumFeatureSize, 0, 1) +
                                 FeatureSpan(cell, minimumFeatureSize, 0, -1) + 1) < minimumFeatureSize) {
                                goners.Add(cell);
                            }
                        }
                    }
                    if (goners.Count != 0) {
                        foreach(GridCell cell in goners)
                            cells.Remove(cell);
                        goners.Clear();
                    }
                }
            }
            DumpGrid("Postfiltered Grid", GridDumpKind.Cells, true);
        }
        
        ///<summary>
        ///    Find all cell neighbors for both collision volumes and
        ///    terrain, and then synthesize polygon for both.
        ///</summary>
        protected void DiscoverPolygons() {
            DiscoverNeighbors(CellStatus.OverCV);
            DiscoverNeighbors(CellStatus.OverTerrain);
            AssignCellSlopes();
            DetermineEdgeCells();
            cvPolygons = DiscoverPolygonsOfStatus(CellStatus.OverCV);
            firstTerrainIndex = polygonIndex;
            terrainPolygons = DiscoverPolygonsOfStatus(CellStatus.OverTerrain);
        }

        ///<summary>
        ///    Discover the neighbors of all the cells in the grid
        ///    that have the given status.  This is called twice; once
        ///    for status = OverCV, and once for status = OverTerrain
        ///</summary>
        internal void DiscoverNeighbors(CellStatus status) {
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        // Ignore cells it has the right status
                        if (cell.status != status)
                            continue;
                        // Check the +x direction
                        GridCell next = FindCellAtHeight(i + 1, j, cell.loc.height);
                        if (next != null && next.status == status) {
                            // Make the bi-directional neighbor connection
                            cell.neighbors[(int)GridDirection.PlusX] = next;
                            next.neighbors[(int)GridDirection.MinusX] = cell;
                        }
                        // Check the +z direction
                        next = FindCellAtHeight(i, j + 1, cell.loc.height);
                        if (next != null && next.status == status) {
                            // Make the bi-directional neighbor connection
                            cell.neighbors[(int)GridDirection.PlusZ] = next;
                            next.neighbors[(int)GridDirection.MinusZ] = cell;
                        }
                    }
                }
            }
        }
        
        ///<summary>
        ///    Iterate over the grid, determining what plane each cell
        ///    belongs in, by determining the slope of the lines to
        ///    each of its neighbors.  The goal is to mark each cell
        ///    as being part of a "crease" of part of a plane.  To do
        ///    so, we compute the slope in each of the 4 directions,
        ///    by moving slopeCellCount cells away in each of the
        ///    4 directions.
        ///</summary>
        internal void AssignCellSlopes() {
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        for (int dir = (int)GridDirection.PlusX; dir <= (int)GridDirection.MinusZ; dir++)
                            cell.neighborSlopes[dir] = slopeNone;
                        if (cell.status == CellStatus.OverCV || cell.status == CellStatus.OverTerrain) {
                            for (int dir = (int)GridDirection.PlusX; dir <= (int)GridDirection.MinusZ; dir++) {
                                GridCell neighbor = NthNeighborOrNull(cell, (GridDirection)dir, slopeCellCount);
                                if (neighbor != null) {
                                    float slope = (neighbor.loc.height - cell.loc.height) / (cellWidth * slopeCellCount);
                                    if (dir == (int)GridDirection.MinusX || dir == (int)GridDirection.MinusZ)
                                        slope = - slope;
                                    cell.neighborSlopes[dir] = slope;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Is the grid direction index in the plus x or minus x direction?
        internal static bool [] xDirection = { true, false, true, false };

        ///<summary>
        ///    A cell is an "edge cell" if:
        ///       o Any neighbor has a different status than it has
        ///       o The 4 slopes that are not slopeNone differ from each
        ///         other
        ///</summary>
        internal void DetermineEdgeCells() {
            int edgeCellCount = 0;
            int vertexCellCount = 0;
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.status != CellStatus.OverCV && cell.status != CellStatus.OverTerrain)
                            continue;
                        bool edgeCell = false;
                        float slopeX = slopeNone;
                        float slopeZ = slopeNone;
                        for (int dir = (int)GridDirection.PlusX; dir <= (int)GridDirection.MinusZ; dir++) {
                            GridCell neighborCell = NthNeighborOrNull(cell, (GridDirection)dir, 1);
                            if (neighborCell == null || neighborCell.status != cell.status)
                                edgeCell = true;
                            float neighborSlope = cell.neighborSlopes[dir];
                            if (xDirection[dir]) {
                                if (slopeX != slopeNone) {
                                    // If the slope is different by more
                                    // than 10%, we say this is an edge cell
                                    if (neighborSlope == slopeNone || (slopeX - neighborSlope) > .1f)
                                        edgeCell = true;
                                }
                                else if (neighborSlope != slopeNone)
                                    slopeX = neighborSlope;
                            }
                            else {
                                if (slopeZ != slopeNone) {
                                    // If the slope is different by more
                                    // than 10%, we say this is an edge cell
                                    if (neighborSlope == slopeNone || (slopeZ - neighborSlope) > .1f)
                                        edgeCell = true;
                                }
                                else if (neighborSlope != slopeNone)
                                    slopeZ = neighborSlope;
                            }
                        }
                        cell.edgeCell = edgeCell;
                        if (edgeCell)
                            edgeCellCount++;
                    }
                }
            }
            MarkAllCellsUnused();
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.edgeCell || (cell.status != CellStatus.OverCV && cell.status != CellStatus.OverTerrain))
                            continue;
                        int edgeCount = 0;
                        for (int dir = (int)GridDirection.PlusX; dir <= (int)GridDirection.MinusZ; dir++) {
                            GridCell neighborCell1 = NthNeighborOrNull(cell, (GridDirection)dir, 1);
                            GridCell neighborCell2 = NthNeighborOrNull(cell, (GridDirection)dir, 2);
                            if (neighborCell1 != null && neighborCell1.edgeCell &&
                                neighborCell2 != null && neighborCell2.edgeCell)
                                edgeCount++;
                        }
                        if (edgeCount >= 2)
                            cell.used = true;
                    }
                }
            }
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.used) {
                            cell.edgeCell = true;
                            edgeCellCount++;
                        }
                    }
                }
            }
            // Find vertices
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (!cell.edgeCell)
                            continue;
                        int edgeCount = 0;
                        bool xDir = false;
                        bool zDir = false;
                        for (int dir = (int)GridDirection.PlusX; dir <= (int)GridDirection.MinusZ; dir++) {
                            GridCell neighborCell1 = NthNeighborOrNull(cell, (GridDirection)dir, 1);
                            GridCell neighborCell2 = NthNeighborOrNull(cell, (GridDirection)dir, 2);
                            if (neighborCell1 != null && neighborCell1.edgeCell &&
                                neighborCell2 != null && neighborCell2.edgeCell) {
                                edgeCount++;
                                if ((dir & 1) == 0)
                                    xDir = true;
                                else
                                    zDir = true;
                            }
                        }
                        if (edgeCount >= 2 && xDir && zDir) {
                            cell.vertexCell = true;
                            vertexCellCount++;
                        }
                    }
                }
            }
            DumpGrid("Edge Cells", GridDumpKind.EdgeCells, true, edgeCellCount, vertexCellCount);
        }
        
        ///<summary>
        ///    Find the vertices associated with cell edges.  A vertex
        ///    is a point at which two or more edges coincide.
        ///</summary>
        internal void FindEdgeVertices() {
            List<CellLocation> vertices = new List<CellLocation>();
        }
        
        ///<summary>
        ///    Iterate over the grid, finding the polygons built of
        ///    cells with the given status.  This is called twice; once
        ///    for status = OverCV, and once for status = OverTerrain
        ///</summary>
        internal List<GridPolygon> DiscoverPolygonsOfStatus(CellStatus status) {
            List<GridPolygon> polygons = new List<GridPolygon>();
            MarkAllCellsUnused();
            for (int i=0; i<xCount - 1; i++) {
                for (int j=0; j<zCount - 1; j++) {
                    List<GridCell> cells = grid[i,j];
                    foreach(GridCell cell in cells) {
                        if (cell.status == status && !cell.used) {
                            GridPolygon r = FindMaximumPolygon(cell);
                            r.index = polygonIndex;
                            polygonIndex++;
                            polygons.Add(r);
                        }
                    }
                }
            }
            if (status == CellStatus.OverCV)
                DumpGrid("CV Polygons", GridDumpKind.CVPolygons, true, polygons.Count);
            else
                DumpGrid("Terrain Polygons", GridDumpKind.TerrainPolygons, true, polygons.Count);
            return polygons;
        }

        ///<summary>
        ///    Find the largest polygon that can be formed out of
        ///    unused neighbor cells, where the cell argument is the
        ///    cell in the upper left-hand corner
        ///</summary>
        internal GridPolygon FindMaximumPolygon(GridCell cell) {
            // Get the length of the rows in the plus X and plus Z
            // directions 
            int maxRowLength = CountUnusedNeighbors(cell, GridDirection.PlusX, xCount);
            int maxColumnLength = CountUnusedNeighbors(cell, GridDirection.PlusZ, zCount);
            // Discover the maximum number in each direction from each
            // axis
            int [] rowLengths = new int[maxColumnLength];
            int [] columnLengths = new int[maxRowLength];
            GridCell next = cell;
            for (int i=0; i<maxRowLength; i++) {
                next = next.neighbors[(int)GridDirection.PlusX];            
                columnLengths[i] = CountUnusedNeighbors(next, GridDirection.PlusZ, maxColumnLength);
            }
            next = cell;
            for (int i=0; i<maxColumnLength; i++) {
                next = next.neighbors[(int)GridDirection.PlusZ];
                rowLengths[i] = CountUnusedNeighbors(next, GridDirection.PlusX, maxRowLength);
            }
            // Now loop through all pairs of counts, z counts fastest,
            // finding the polygon with the biggest area
            float area = 0;
            int bestX = maxRowLength;
            int bestZ = 0;
            int minRowLength = maxRowLength;
            for (int i=0; i<maxColumnLength; i++) {
                minRowLength = Math.Min(minRowLength, rowLengths[i]);
                int minColumnLength = maxColumnLength;
                for (int j=0; j<minRowLength; j++) {
                    minColumnLength = Math.Min(minColumnLength, columnLengths[j]);
                    float newArea = (float)(minColumnLength + 1) * (float)(j + 1);
                    if (newArea > area) {
                        area = newArea;
                        bestX = j + 1;
                        bestZ = minColumnLength;
                    }
                }
            }
            GridCell lastXCell = NthNeighbor(cell, GridDirection.PlusX, bestX);
            GridPolygon result = MakeGridPolygon(cell.status, cell, lastXCell,
                NthNeighbor(lastXCell, GridDirection.PlusZ, bestZ),
                NthNeighbor(cell, GridDirection.PlusZ, bestZ));
            // Mark all these cells as used
            cell.used = true;
            next = cell;
            GridCell nextZ;
            for (int i=0; i<bestX+1; i++) {
                nextZ = next;
                for (int j=0; j<bestZ+1; j++) {
                    nextZ.used = true;
                    nextZ.polygon = result;
                    nextZ = nextZ.neighbors[(int)GridDirection.PlusZ];
                }
                next = next.neighbors[(int)GridDirection.PlusX];
            }
            return result;
        }

        ///<summary>
        ///    Return the count of neighbors in the given
        ///    GridDirection, to a maximum of distance.  This is used
        ///    by the polygon synthesis routines.
        ///</summary>
        internal int CountNeighbors(GridCell cell, GridDirection direction, int distance) {
            int count = 0;
            GridCell temp = cell;
            if (temp == null)
                return 0;
            for (int i=0; i<distance; i++) {
                if (temp.neighbors == null)
                    break;
                temp = temp.neighbors[(int)direction];
                if (temp == null)
                    break;
                count++;
            }
            return count;
        }

        ///<summary>
        ///    Return the count of unused neighbors in the given
        ///    GridDirection, to a maximum of distance.  This is used
        ///    by the polygon synthesis routines.
        ///</summary>
        internal int CountUnusedNeighbors(GridCell cell, GridDirection direction, int distance) {
            int count = 0;
            GridCell temp = cell;
            if (temp == null)
                return 0;
            for (int i=0; i<distance; i++) {
                if (temp.neighbors == null)
                    break;
                temp = temp.neighbors[(int)direction];
                if (temp == null || temp.used)
                    break;
                count++;
            }
            return count;
        }

        ///<summary>
        ///    Return the nth neighbor in the given direction.  We've 
        ///    already counted the neighbors, so there should be no 
        ///    case of a null neighbor.
        ///</summary>
        internal GridCell NthNeighbor(GridCell cell, GridDirection direction, int n) {
            GridCell temp = NthNeighborOrNull(cell, direction, n);
            if (temp == null)
                throw new Exception(string.Format("Null neighbor in PathGenerator.NthNeighbor({0}, {1}, {2})",
                                                  cell.ToString(), (int)direction, n));
            return temp;
        }

        ///<summary>
        ///    Return the nth neighbor in the given direction.
        ///</summary>
        internal GridCell NthNeighborOrNull(GridCell cell, GridDirection direction, int n) {
            GridCell temp = cell;
            for (int i=0; i<n; i++) {
                if (temp == null) {
                    return null;
                }
                temp = temp.neighbors[(int)direction];
            }
            return temp;
        }

        ///<summary>
        ///    Count the number of cells at "compatible" heights,
        ///    starting with the given cell, and moving in the given
        ///    direction given by xIncr and zIncr.  If we get
        ///    distance-1 cells away, stop.  In any case, return the
        ///    count.
        ///</summary>
        internal int FeatureSpan(GridCell cell, int distance, int xIncr, int zIncr) {
            int count = 0;
            CellLocation loc = new CellLocation(cell.loc.xCell, cell.loc.zCell, cell.loc.height, null);
            for (int i=0; i<distance-1; i++) {
                loc.xCell += xIncr;
                loc.zCell += zIncr;
                if (loc.xCell < 0 || loc.xCell >= xCount ||
                    loc.zCell < 0 || loc.zCell >= zCount)
                    return count;
                GridCell newCell = FindCellAtHeight(loc);
                if (newCell == null || newCell.status != CellStatus.OverCV)
                    return count;
                loc.height = newCell.loc.height;
                count++;
            }
            return count;
        }

        ///<summary>
        ///    As we proceed from corner[0] to corner[3], this vector
        ///    gives the direction, indexed by starting corner number,
        ///    to look to get outside the polygon
        ///</summary>
        internal static GridDirection [] outsideDirection = 
            { GridDirection.MinusZ, GridDirection.PlusX, GridDirection.PlusZ, GridDirection.MinusX };

        ///<summary>
        ///    As we proceed from corner[0] to corner[3], this vector
        ///    gives the direction, indexed by starting corner number,
        ///    from the corner to the successive corner
        ///</summary>
        internal static GridDirection [] perimeterDirection = 
            { GridDirection.PlusX, GridDirection.PlusZ, GridDirection.MinusX, GridDirection.MinusZ };

        ///<summary>
        ///    Find the chunks of borders of polygons that can be
        ///    accessed from the terrain.
        ///</summary>
        protected void FindTerrainPortals() {
            List<PolygonArc> portals = new List<PolygonArc>();
            foreach (GridPolygon r in cvPolygons) {
                // Look around the perimeter of the polygon to see
                // if it borders the terrain.
                for (int corner=0; corner<4; corner++) {
                    GridCell c = r.corners[corner];
                    int sideLength = CellDistance(c, r.corners[corner == 3 ? 0 : corner+1]);
                    GridDirection outside = outsideDirection[corner];
                    GridDirection forward = perimeterDirection[corner];
                    bool inTerrain = false;
                    int terrainIndex = 0;
                    GridCell start = null;
                    GridCell lastC = null;
                    for (int j=0; j<sideLength; j++) {
                        GridCell n = FindCellAtHeight(c.loc.xCell + XIncrement[(int)outside], 
                                                      c.loc.zCell + ZIncrement[(int)outside],
                                                      c.loc.height);
                        if (inTerrain) {
                            bool noTerrain = n == null || n.status != CellStatus.OverTerrain;
                            if (noTerrain || n.polygon.index != terrainIndex) {
                                portals.Add(new PolygonArc(ArcKind.CVToTerrain, r.index, terrainIndex, MakeGridEdge(start, lastC, forward, outside)));
                                terrainIndex = (n == null ? 0 : n.polygon.index);
                                start = n;
                            }
                            if (noTerrain)
                                inTerrain = false;
                        }
                        if (!inTerrain && n != null && n.status == CellStatus.OverTerrain) {
                            start = c;
                            lastC = c;
                            terrainIndex = n.polygon.index;
                            inTerrain = true;
                        }
                        lastC = c;
                        c = NthNeighborOrNull(c, forward, 1);
                    }
                    if (inTerrain)
                        portals.Add(new PolygonArc(ArcKind.CVToTerrain, r.index, terrainIndex, MakeGridEdge(start, lastC, forward, outside)));
                }
            }
            PostProcessArcs(portals);
            terrainPortals = portals;
            DumpGrid("Terrain Portals", GridDumpKind.Portals, true, portals.Count);
        }

        ///<summary>
        ///    Find the arcs that connect polygons of the given type
        ///</summary>
        protected void FindPolygonArcsOfType(List<PolygonArc> arcs, List<GridPolygon> polygons, CellStatus status) {
            foreach (GridPolygon r in polygons) {
                if (r.status != status)
                    continue;
                // Circumnavigate the perimeter looking for other
                // polygons
                ArcKind arcKind = (status == CellStatus.OverCV ? ArcKind.CVToCV : ArcKind.TerrainToTerrain);
                for (int corner=0; corner<4; corner++) {
                    GridCell c = r.corners[corner];
                    int sideLength = CellDistance(c, r.corners[corner == 3 ? 0 : corner+1]);
                    GridDirection outside = outsideDirection[corner];
                    GridDirection forward = perimeterDirection[corner];
                    bool inEdge = false;
                    GridCell start = null;
                    GridCell lastC = null;
                    GridPolygon lastPolygon = null;
                    for (int j=0; j<sideLength; j++) {
                        if (c == null)
                            break;
                        GridCell n = FindCellAtHeight(c.loc.xCell + XIncrement[(int)outside], 
                                                      c.loc.zCell + ZIncrement[(int)outside],
                                                      c.loc.height);
                        if (inEdge && (n == null || n.polygon != lastPolygon)) {
                            arcs.Add(new PolygonArc(arcKind, lastPolygon.index, r.index, MakeGridEdge(start, lastC, forward, outside)));
                            inEdge = false;
                        }
                        if (!inEdge && n != null && n.polygon != null && n.polygon.status == status) {
                            start = c;
                            lastC = c;
                            lastPolygon = n.polygon;
                            inEdge = true;
                        }
                        lastC = c;
                        c = NthNeighborOrNull(c, forward, 1);
                    }
                    if (inEdge && !findArc(arcs, r.index, lastPolygon.index))
                        arcs.Add(new PolygonArc(arcKind, lastPolygon.index, r.index, MakeGridEdge(start, lastC, forward, outside)));
                }
            }
            PostProcessArcs(arcs);
        }

        ///<summary>
        ///    Find the arcs that connect polygons of the same types
        ///</summary>
        protected void FindPolygonArcs() {
            List<PolygonArc> arcs = new List<PolygonArc>();
            FindPolygonArcsOfType(arcs, cvPolygons, CellStatus.OverCV);
            FindPolygonArcsOfType(arcs, terrainPolygons, CellStatus.OverTerrain);
            polygonArcs = arcs;
        }
        
        // Have we already seen this arc?
        bool findArc(List<PolygonArc> arcs, int poly1Index, int poly2Index) {
            foreach (PolygonArc arc in arcs) {
                if (arc.Poly1Index == poly1Index && arc.Poly2Index == poly2Index)
                    return true;
            }
            return false;
        }
        
        ///<summary>
        ///    Postprocess arcs, establishing predecessors and
        ///    successors, deleting small ones, and adjusting the
        ///    start and end locations so that that mobs going through
        ///    them don't scrape the walls.
        ///</summary>
        protected void PostProcessArcs(List<PolygonArc> arcs) {
            AssignCellArcs(arcs);
            FindArcSuccsAndPreds(arcs);
            DeleteTinyArcs(arcs);
            SetStartAndEndLocs(arcs);
        }

        ///<summary>
        ///    Assign the cell arc fields of all the cells in the
        ///    grid.  Note that this slightly messes up if we have a
        ///    pair of arcs that originate from the same cell, which
        ///    can happen when corners exactly meet.
        ///</summary>
        protected void AssignCellArcs(List<PolygonArc> arcs) {
            // Assign the arc field of the cells on the poly2
            // side of each arc 
            foreach (PolygonArc arc in arcs) {
                GridCell cell = arc.edge.start;
                while (cell != null) {
                    cell.arc = arc;
                    if (cell == arc.edge.end)
                        break;
                    cell = NthNeighborOrNull(cell, arc.edge.forward, 1);
                }
            }
        }

        ///<summary>
        ///    Establish the geometric successor/predecessor
        ///    relationship among linear arcs.
        ///</summary>
        protected void FindArcSuccsAndPreds(List<PolygonArc> arcs) {
            // Set up predecessor/successor relationships
            foreach (PolygonArc arc in arcs) {
                GridEdge pEdge = arc.edge;
                foreach (PolygonArc other in arcs) {
                    GridEdge oEdge = other.edge;
                    if (other == arc || pEdge.forward != oEdge.forward)
                        continue;
                    switch (pEdge.forward) {
                    case GridDirection.PlusX:
                        if (pEdge.start.loc.zCell != oEdge.start.loc.zCell)
                            continue;
                        if (pEdge.start.loc.xCell + 1 == oEdge.end.loc.xCell) {
                            arc.pred = other;
                            other.succ = arc;
                        }
                        else if (pEdge.end.loc.xCell + 1 == oEdge.start.loc.xCell) {
                            arc.succ = other;
                            other.pred = arc;
                        }
                        break;
                    case GridDirection.PlusZ:
                        if (pEdge.start.loc.xCell != oEdge.start.loc.xCell)
                            continue;
                        if (pEdge.start.loc.zCell + 1 == oEdge.end.loc.zCell) {
                            arc.pred = other;
                            other.succ = arc;
                        }
                        else if (pEdge.end.loc.zCell + 1 == oEdge.start.loc.zCell) {
                            arc.succ = other;
                            other.pred = arc;
                        }
                        break;
                    }
                }
            }
        }
        
        ///<summary>
        ///    Delete arcs smaller than the minimum feature size
        ///</summary>
        protected void DeleteTinyArcs(List<PolygonArc> arcs) {
            // The minimum width of an aggregated arc, in units of cells
            int minWidth = 2;
            List<PolygonArc> arcsToDelete = new List<PolygonArc>();
            // Now iterate through, finding the first in the pred
            // chain, and asking if the total chain length is long enough
            List<PolygonArc> firstArcs = new List<PolygonArc>();
            foreach (PolygonArc arc in arcs) {
                PolygonArc next = arc;
                while (next.pred != null)
                    next = next.pred;
                firstArcs.Add(next);
            }
            foreach (PolygonArc arc in arcs) {
                PolygonArc first = arc;
                PolygonArc next = first;
                PolygonArc last = null;
                int count = 0;
                do {
                    count += next.edge.LengthInCells();
                    last = next;
                    next = next.succ;
                } while (next != null);
                if (count <= minWidth) {
                    // Not big enough to get through - - get rid of it
                    next = first;
                    do {
                        arcsToDelete.Add(next);
                        next = next.succ;
                    } while (next != null);
                }
                else {
                    CellLocation firstLoc = new CellLocation(first.edge.start.loc);
                    CellLocation lastLoc = new CellLocation(last.edge.end.loc);
                    int origCount = count;
                    // See if there are ones at the ends that should
                    // be added to the delete list
                    next = first;
                    while (next.edge.LengthInCells() <= minimumFeatureSize && count - minimumFeatureSize >= minWidth) {
                        arcsToDelete.Add(next);
                        count -= next.edge.LengthInCells();
                        next = next.succ;
                    }
                    PolygonArc newFirst = first;
                    next = last;
                    while (next.edge.LengthInCells() <= minimumFeatureSize && count - minimumFeatureSize >= minWidth) {
                        arcsToDelete.Add(next);
                        count -= next.edge.LengthInCells();
                        next = next.pred;
                    }
                    PolygonArc newLast = last;
                    // Now see if it's possible to move start and end
                    // of the arcs in so that they are half-width from
                    // their maximum extent
                    int maxExcess = (origCount - minWidth) / 2;
                    if (maxExcess <= 0)
                        continue;
                    int excess = maxExcess - CellDistance(first.edge.start, newFirst.edge.start);
                    next = newFirst;
                    while (next.edge.LengthInCells() <= excess) {
                        arcsToDelete.Add(next);
                        excess -= next.edge.LengthInCells();
                        next = next.succ;
                    }
                    while (excess > 0) {
                        next.edge.start.arc = null;
                        next.edge.start = NthNeighbor(next.edge.start, next.edge.forward, 1);
                        excess--;
                    }
                    excess = maxExcess - CellDistance(last.edge.end, newLast.edge.end);
                    next = newLast;
                    while (next != null && next.edge.LengthInCells() <= excess) {
                        arcsToDelete.Add(next);
                        excess -= next.edge.LengthInCells();
                        next = next.pred;
                    }
                    GridDirection forward = next.edge.forward;
                    GridDirection back = (forward == GridDirection.PlusX ? GridDirection.MinusX : GridDirection.MinusZ);
                    while (excess > 0) {
                        next.edge.start.arc = null;
                        next.edge.end = NthNeighbor(next.edge.end, back, 1);
                        excess--;
                    }
                }
            }
            foreach (PolygonArc arc in arcsToDelete)
                arcs.Remove(arc);
            // Now give each arc an index, for the dump routine.
            for (int i=0; i<arcs.Count; i++)
                arcs[i].index = i + 1;
            // Remove references to deleted arcs
            for (int i=0; i<xCount; i++) {
                for (int j=0; j<zCount; j++) {
                    foreach (GridCell cell in grid[i,j]) {
                        if (cell.arc != null && cell.arc.index == 0)
                            cell.arc = null;
                    }
                }
            }
        }
        
        internal void SetStartAndEndLocs(List<PolygonArc> arcs) {
            foreach (PolygonArc arc in arcs) {
                arc.edge.startLoc = CoordinatesOfCell(arc.edge.start, arc.edge.outside, true);
                arc.edge.endLoc = CoordinatesOfCell(arc.edge.end, arc.edge.outside, false);
            }
        }

        // Mapping from outside direction to amount to add to the x
        // and z coordinates of the start and end cells centers to get
        // to the outside.
        // ??? This is all suspect; I'm not sure it actually works.
        internal static int [,] fromOutsideToStart = { { 1, 1 }, { -1, 1 }, { -1, -1 }, { 1, -1 } };
        internal static int [,] fromOutsideToEnd =   { { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };

        internal Vector3 CoordinatesOfCell(GridCell cell, GridDirection outside, bool start) {
            Vector3 loc = GridLocation(cell.loc);
            if (start) {
                loc.x += .5f * cellWidth * fromOutsideToStart[(int)outside, 0];
                loc.z += .5f * cellWidth * fromOutsideToStart[(int)outside, 1];
            }
            else {
                loc.x += .5f * cellWidth * fromOutsideToEnd[(int)outside, 0];
                loc.z += .5f * cellWidth * fromOutsideToEnd[(int)outside, 1];
            }
            return modelTransform * loc;
        }

        internal Vector3 CoordinatesOfCell(GridCell cell, int xOffset, int zOffset) {
            Vector3 loc = GridLocation(cell.loc);
            loc.x += .5f * cellWidth * xOffset;
            loc.z += .5f * cellWidth * zOffset;
            return modelTransform * loc;
        }
        
        ///<summary>
        ///    Create a grid edge, given the starting and ending cell,
        ///    and the outside direction.
        ///</summary>
        internal GridEdge MakeGridEdge(GridCell start, GridCell end, GridDirection forward, GridDirection outside) {
            // Canonicalize the directions
            if (forward == GridDirection.MinusX) {
                forward = GridDirection.PlusX;
                GridCell temp = start;
                start = end;
                end = temp;
            }
            else if (forward == GridDirection.MinusZ) {
                forward = GridDirection.PlusZ;
                GridCell temp = start;
                start = end;
                end = temp;
            }
            return new GridEdge(start, end, forward, outside);
        }
        
        ///<summary>
        ///    Create a grid edge, given the starting and ending cell,
        ///    and the outside direction.  Note bene: The order of the
        ///    corners is important.  c1 is lower left; c2 is lower
        ///    right; c3 is upper right; c4 is upper left.
        ///</summary>
        internal GridPolygon MakeGridPolygon(CellStatus status, GridCell c1, GridCell c2, GridCell c3, GridCell c4) {
            return new GridPolygon(status, c1, c2, c3, c4,
                CoordinatesOfCell(c1, -1, -1),
                CoordinatesOfCell(c2, 1, -1),
                CoordinatesOfCell(c3, 1, 1),
                CoordinatesOfCell(c4, -1, 1));
        }
        
        ///<summary>
        ///    What sort of dump should we generate?
        ///</summary>
        protected enum GridDumpKind {
            Cells = 0,
            CVPolygons,
            TerrainPolygons,
            Portals,
            EdgeCells
        }

        protected static string digits = "0123456789";

        protected void DumpCurrentTime(string title) {
            string p = "../PathGeneratorLog.txt";
            FileStream f = new FileStream(p, 
                appendGridDump ? (File.Exists(p) ? FileMode.Append : FileMode.Create) : FileMode.Create,
                FileAccess.Write);
            StreamWriter writer = new StreamWriter(f);
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            writer.Write(string.Format("\n{0}, {1} milliseconds since creation of the PathGenerator instance\n\n",
                    title, elapsedMilliseconds));
            writer.Close();
        }
        
        ///<summary>
        ///    Dump the grid, with the given title, putting the file
        ///    PathGeneratorLog.txt in the ../ directory.  If append
        ///    is false, create a new file; if true, append to the
        ///    existing file.
        ///</summary>
        protected void DumpGrid(string title, GridDumpKind kind, bool append, int count, int subCount) {
            if (!dumpGrid)
                return;
            // Dump the header rows
            string hundreds = "";
            string tens = "";
            string ones = "";
            for (int i=0; i<xCount; i++) {
                hundreds += digits.Substring(i / 100, 1);
                tens += digits.Substring((int)Decimal.Remainder(i, 100) / 10, 1);
                ones += digits.Substring((int)Decimal.Remainder(i, 10), 1);
            }
            
            string p = "../PathGeneratorLog.txt";
            FileStream f = new FileStream(p, 
                (appendGridDump || append) ? (File.Exists(p) ? FileMode.Append : FileMode.Create) : FileMode.Create,
                FileAccess.Write);
            StreamWriter writer = new StreamWriter(f);
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            writer.Write(string.Format("{0} Started writing '{1}' for model '{2}' to {3}\n{4} milliseconds since creation of the PathGenerator instance\n\n",
                    DateTime.Now.ToString("F"), title, modelName, p, elapsedMilliseconds));
            switch (kind) {
            case GridDumpKind.CVPolygons:
                writer.Write("{0} Collision Volume Polygons\n\n", count);
                break;
            case GridDumpKind.TerrainPolygons:
                writer.Write("{0} Terrain Polygons\n\n", count);
                break;
            case GridDumpKind.Portals:
                writer.Write("{0} Terrain Portals\n\n", count);
                break;
            case GridDumpKind.EdgeCells:
                writer.Write("{0} Edge Cells; {1} Vertices\n\n", count, subCount);
                break;
            }
            writer.Write("      {0}\n" + "      {1}\n" + "      {2}\n" + "\n", 
                hundreds, tens, ones);
            for (int i=0; i<zCount; i++) {
                string line = string.Format("{0:D4}  ", i);
                for (int j=0; j<xCount; j++) {
                    List<GridCell> cells = grid[j,i];
                    switch (kind) {
                    case GridDumpKind.Cells:
                        line += DumpGridCell(cells);
                        break;
                    case GridDumpKind.Portals:
                        line += DumpGridPortalCell(cells);
                        break;
                    case GridDumpKind.EdgeCells:
                        line += DumpGridEdgeCell(cells);
                        break;
                    case GridDumpKind.CVPolygons:
                        line += DumpGridPolygon(cells, kind);
                        break;
                    case GridDumpKind.TerrainPolygons:
                        line += DumpGridPolygon(cells, kind);
                        break;
                    }
                }
                writer.Write(line + "\n");
            }
            writer.Write(string.Format("\nGenerating dump took {0} milliseconds\n\n", stopwatch.ElapsedMilliseconds - elapsedMilliseconds));
            writer.Close();
        }
        
        protected void DumpGrid(string title, GridDumpKind kind, bool append) {
            DumpGrid(title, kind, append, 0, 0);
        }
        
        protected void DumpGrid(string title, GridDumpKind kind, bool append, int count) {
            DumpGrid(title, kind, append, count, 0);
        }
        
        ///<summary>
        ///    Return a single character that describes the polygon
        ///    containing the cells
        ///</summary>
        protected string DumpGridCell(List<GridCell> cells) {
            if (cells.Count > 9)
                return "*";
            else if (cells.Count == 0)
                return "0";
            else if (cells.Count == 1) {
                GridCell cell = cells[0];
                return cell.status == CellStatus.OverTerrain ? "T" :
                    cell.status == CellStatus.OverCV ? "C" :
                    cell.status == CellStatus.Inaccessible ? "I" : "?";
            }
            else {
                bool allInaccessible = true;
                foreach(GridCell cell in cells) {
                    if (cell.status != CellStatus.Inaccessible) {
                        allInaccessible = false;
                        break;
                    }
                }
                if (allInaccessible)
                    return "i";
                else
                    return digits.Substring(cells.Count, 1);
            }
        }

        protected string DumpGridPortalCell(List<GridCell> cells) {
            foreach(GridCell cell in cells) {
                if (cell.arc == null)
                    continue;
                if (cell.arc.index < polygonID.Length)
                    return polygonID.Substring(cell.arc.index - 1, 1);
                else
                    return "#";
            }
            return DumpGridCell(cells);
        }

        protected static string polygonID = "abcdefghjklmnopqrstuvwxyzABDEFGHJKLMNPQRSUVWXYZ";
        
        ///<summary>
        ///    Return a single character that describes the polygon
        ///    contained in a cell in the list, or else the result of
        ///    DumpGridCell
        ///</summary>
        protected string DumpGridPolygon(List<GridCell> cells, GridDumpKind kind) {
            int indexSub = (kind == GridDumpKind.CVPolygons ? 1 : firstTerrainIndex);
            foreach(GridCell cell in cells) {
                if (cell.polygon == null || 
                    (kind == GridDumpKind.CVPolygons && cell.status != CellStatus.OverCV) ||
                    (kind == GridDumpKind.TerrainPolygons && cell.status != CellStatus.OverTerrain))
                    continue;
                if (cell.polygon.index < polygonID.Length)
                    return polygonID.Substring(cell.polygon.index - indexSub, 1);
                else
                    return "#";
            }
            return DumpGridCell(cells);
        }

        ///<summary>
        ///    Return a single character that describes the polygon
        ///    contained in a cell in the list, or else the result of
        ///    DumpGridCell
        ///</summary>
        protected string DumpGridEdgeCell(List<GridCell> cells) {
            foreach(GridCell cell in cells) {
                if (cell.vertexCell)
                    return "*";
                else if (cell.edgeCell)
                    return "@";
            }
            return DumpGridCell(cells);
        }
        
        ///<summary>
        ///    Return the cell at the grid location at the approximate
        ///    height, or return null.
        ///</summary>
        internal GridCell FindCellAtHeight(CellLocation loc) {
            List<GridCell> cells = grid[loc.xCell, loc.zCell];
            if (cells.Count == 0)
                return null;
            float range = maxClimbDistance + maxDisjointDistance + 10f;
            foreach (GridCell cell in cells) {
                if (cell.loc.height >= loc.height - range &&
                    cell.loc.height <= loc.height + range)
                    return cell;
            }
            return null;
        }
        
        ///<summary>
        ///    Another overloading that doesn't require the indices
        ///    packaged up as a CellLocation
        ///    height, or return null.
        ///</summary>
        internal GridCell FindCellAtHeight(int xCell, int zCell, float height) {
            if (xCell < 0 || xCell >= xCount || zCell < 0 || zCell >= zCount)
                return null;
            List<GridCell> cells = grid[xCell, zCell];
            if (cells.Count == 0)
                return null;
            float range = maxClimbDistance + maxDisjointDistance + 10f;
            foreach (GridCell cell in cells) {
                if (cell.loc.height >= height - range &&
                    cell.loc.height <= height + range)
                    return cell;
            }
            return null;
        }
        
        ///<summary>
        ///     Add the location to the work list if it isn't already on the list
        ///</summary>
        internal void AddToWorkList(CellLocation loc) {
            foreach (CellLocation c in workList) {
                if (c.xCell == loc.xCell && c.zCell == loc.zCell && c.height == loc.height)
                    return;
            }
            workList.Add(loc);
        }
        
        ///<summary>
        ///    Return the vector location of the cell in the grid
        ///</summary>
        internal Vector3 GridLocation(CellLocation loc) {
            Vector3 p = lowerLeftCorner;
            p.x += ((float)loc.xCell + .5f) * cellWidth;
            p.y = loc.height;
            p.z += ((float)loc.zCell + .5f) * cellWidth;
            return p;
        }   

        ///<summary>
        ///    Move the moving box so it's centered over the given
        ///    cell, about to drop
        ///</summary>
        internal void SetMovingBoxOverCell(CellLocation loc) {
            Vector3 min = lowerLeftCorner;
            min.x += (float)loc.xCell * cellWidth;
            min.y = loc.height + maxClimbDistance + maxDisjointDistance;
            min.z += (float)loc.zCell * cellWidth;
            Vector3 max = min;
            max.x = min.x + cellWidth;
            max.y = min.y + boxHeight;
            max.z = min.z + cellWidth;
            Vector3 center = 0.5f * (min + max);
            movingBox.min = min;
            movingBox.center = center;
            movingBox.max = max;
        }
        
        ///<summary>
        ///    Make the input a multiple of the grid resolution
        ///</summary>
        protected float MultipleOfResolution(float input)
        {
            return cellWidth * (float)((int)(input / cellWidth));
        }

        ///<summary>
        ///    For cells that have either same xCell value or the same
        ///    zcell value, return the count of cells between them,
        ///    including them.
        ///</summary>
        protected int CellDistance(GridCell c1, GridCell c2) {
            if (c1.loc.xCell == c2.loc.xCell)
                return Math.Abs(c1.loc.zCell - c2.loc.zCell) + 1;
            else if (c1.loc.zCell == c2.loc.zCell)
                return Math.Abs(c1.loc.xCell - c2.loc.xCell) + 1;
            else
                throw new Exception(string.Format("CellDistance not defined for cells {0} and {1}", c1, c2));
        }
        
        #endregion Methods
    }

    ///<summary>
    ///    This class represented a grid "address": the x and z cell
    ///    number, plus the cell height.
    ///    existing file.
    ///</summary>
    internal class CellLocation {
        internal int xCell;
        internal int zCell;
        internal float height;
        internal CellLocation parent;

        internal CellLocation(int xCell, int zCell, float height, CellLocation parent) {
            this.xCell = xCell;
            this.zCell = zCell;
            this.height = height;
            this.parent = parent;
        }

        internal CellLocation(CellLocation other) {
            this.xCell = other.xCell;
            this.zCell = other.zCell;
            this.height = other.height;
            this.parent = other.parent;
        }
            
        public override string ToString() {
            return string.Format("(x={0},z={1},h={2})", xCell, zCell, height);
        }
    }

    ///<summary>
    ///    We sometimes use this to index the array of neighbors; more
    ///    often, alternatively, we sometimes use an integer.
    ///</summary>
    internal enum GridDirection {
        PlusX = 0,  // From left to right
        PlusZ,      // From bottom to top
        MinusX,     // From right to left
        MinusZ      // From top to bottom
    }

    ///<summary>
    ///    The cell status starts out Unvisited, and is updated as the
    ///    algorithm proceeds.
    ///</summary>
    public enum CellStatus {
        Unvisited = 0,  // Not actually used, because we don't create
                        // a cell until it's visited
        Inaccessible,
        OverTerrain,
        OverCV
    }

    ///<summary>
    ///    A grid cell consists of a CellLocation, a CellStatus, and
    ///    information about the supporting shape, if status = OverCV.
    ///    Right now, the supporting shape is unused.
    ///</summary>
    public class GridCell {
        // The position of the center of base of the cell
        internal CellLocation loc;
        internal CellStatus status = CellStatus.Unvisited;
        // The collision shape is only filled in if the status becomes
        // OverCV
        internal CollisionShape supportingShape;
        // This is filled in by the DiscoverNeighbors routines.
        internal GridCell[] neighbors = new GridCell[4];
        // Has the cell been encountered by the polygon discovery
        // algorithm?
        internal bool used = false;
        // The polygon that this cell is part of
        internal GridPolygon polygon;
        // The portal this cell belongs to, if any
        internal PolygonArc arc = null;

        // The set of slopes of the cells
        internal float [] neighborSlopes = new float[4];
        // Is the cell an "edge" cell; i.e., a cell where slopes
        // differ in any of the 4 neighbor directions?
        internal bool edgeCell = false;
        // Is the cell an "vertex" cell; i.e., a cell two or more
        // lines cross
        internal bool vertexCell = false;

        // The Plane is set when the status is OverCV or OverTerrain,
        // and when all the neighbors agree on the slope
        // internal Plane plane; (unused)

        internal GridCell(CellLocation loc)
        {
            this.loc = loc;
        }

	    internal static string [] statusStrings = { "Unvisited", "Inaccessible", "OverTerrain", "OverCV" };
        
        public override string ToString() {
            return string.Format("(x={0},z={1},h={2},s={3})",
                loc.xCell, loc.zCell, loc.height, statusStrings[(int)status]);
        }
        
    }

    ///<summary>
    ///    This class represents a polygon synthesized from the grid
    ///</summary>
    public class GridPolygon {
        internal CellStatus status;
        internal GridCell[] corners;
        internal Vector3[] cornerLocs;
        internal int index;
        internal GridPolygon(CellStatus status, GridCell c1, GridCell c2, GridCell c3, GridCell c4,
                             Vector3 c1Loc, Vector3 c2Loc, Vector3 c3Loc, Vector3 c4Loc) {
            this.status = status;
            corners = new GridCell[] { c1, c2, c3, c4 };
            cornerLocs = new Vector3[] { c1Loc, c2Loc, c3Loc, c4Loc };
        }
        internal GridPolygon(CellStatus status, GridCell[] corners, Vector3[] cornerLocs) {
            this.status = status;
            this.corners = corners;
            this.cornerLocs = cornerLocs;
        }

        public int Index {
            get { return index; }
        }

        public Vector3 [] CornerLocs  {
            get { return cornerLocs; }
        }

        public override string ToString() {
            return string.Format("({0}:{1}, {2}:{3}, index {4})",
                corners[0].loc.xCell, corners[0].loc.zCell, corners[2].loc.xCell, corners[2].loc.zCell, index);
        }
            
    }

    ///<summary>
    ///    This class represents a portion of an "edge" in the a grid,
    ///    typically used to represent the portion of the side of a
    ///    polygon that connects the terrain and the polygon, or
    ///    an arc between polygon nodes
    ///</summary>
    public class GridEdge {
        internal GridCell start;
        internal GridCell end;
        internal Vector3 startLoc;
        internal Vector3 endLoc;
        internal GridDirection forward;
        internal GridDirection outside;
        internal GridEdge(GridCell start, GridCell end, GridDirection forward, GridDirection outside) {
            this.start = start;
            this.end = end;
            this.forward = forward;
            this.outside = outside;
        }

        public Vector3 StartLoc {
            get { return startLoc; }
        }

        public Vector3 EndLoc {
            get { return endLoc; }
        }

        public int LengthInCells() {
            return Math.Max(Math.Abs(start.loc.xCell - end.loc.xCell), Math.Abs(start.loc.zCell - end.loc.zCell)) + 1;
        }
        
        public override string ToString() {
            return string.Format("Edge({0}:{1}, {2}:{3})", 
                start.loc.xCell, start.loc.zCell, end.loc.xCell, end.loc.zCell);
        }
    }

    public enum ArcKind {
        Illegal = 0,
        CVToCV,
        TerrainToTerrain,
        CVToTerrain
    }
    
    ///<summary>
    ///    This class represents an arc connecting two polygon nodes
    ///</summary>
    public class PolygonArc {
        internal ArcKind kind;
        internal int poly1Index;
        internal int poly2Index;
        internal GridEdge edge;
        internal int index;
        internal PolygonArc pred = null;
        internal PolygonArc succ = null;
        internal PolygonArc(ArcKind kind, int poly1Index, int poly2Index, GridEdge edge) {
            this.kind = kind;
            this.poly1Index = poly1Index;
            this.poly2Index = poly2Index;
            this.edge = edge;
        }

        public override string ToString() {
            return string.Format("Arc r{0}<->r{1} {2}", poly1Index, poly2Index, edge.ToString());
        }
        
        public ArcKind Kind {
            get { return kind; }
        }

        public int Poly1Index {
            get { return poly1Index; }
        }

        public int Poly2Index {
            get { return poly2Index; }
        }

        public GridEdge Edge {
            get { return edge; }
        }

    }
    
}
