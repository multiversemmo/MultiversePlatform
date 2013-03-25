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

using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Media;
using Axiom.Utility;
using Multiverse.CollisionLib;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for Boundary.
	/// </summary>
	public class Boundary : IDisposable 
	{
		private List<Vector2> points;
        private List<int []> tris;
        private bool closed = false;
        private AxisAlignedBox bounds;
        // private float area; (unused)
        private int pageSize;
        private List<PageCoord> touchedPages;
        private List<IBoundarySemantic> semantics;
        private String name;

        private bool hilight;

        private SceneNode sceneNode;

        public static SceneNode parentSceneNode;

        private bool visible;
        private PageCoord boundsMinPage;
        private PageCoord boundsMaxPage;
        private bool autoClose = false;
        private static int uniqueNum = 0;
        
		public Boundary(String name)
		{
            points = new List<Vector2>();
            semantics = new List<IBoundarySemantic>();

            touchedPages = new List<PageCoord>();

            this.name = name;

            sceneNode = parentSceneNode.CreateChildSceneNode(name);

            pageSize = TerrainManager.Instance.PageSize;
		}

        public Boundary(XmlTextReader r)
        {
            points = new List<Vector2>();
            semantics = new List<IBoundarySemantic>();

            touchedPages = new List<PageCoord>();

            pageSize = TerrainManager.Instance.PageSize;

            FromXML(r);

            sceneNode = parentSceneNode.CreateChildSceneNode(name);

        }

        public void AddToScene(SceneManager scene)
        {
        }

        protected void ParsePoint(XmlTextReader r)
        {
            float x = 0, y = 0;
            for (int i = 0; i < r.AttributeCount; i++)
            {

                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "x":
                        x = float.Parse(r.Value);
                        break;
                    case "y":
                        y = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            AddPoint(new Vector3(x, 0, y));
        }

        protected void ParsePoints(XmlTextReader r)
        {
            while (r.Read())
            {
                // look for the start of an element
                if ( (r.NodeType == XmlNodeType.Element) && ( r.Name == "point" ) )
                {
                    // parse that element
                    ParsePoint(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the points
                    Close();
                    return;
                }
            }
        }

        protected void ParseElement(XmlTextReader r)
        {
            bool readEnd = true;
            // set the field in this object based on the element we just read
            switch (r.Name)
            {
                case "name":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }
                    name = string.Format("{0} - unique boundary - {1}", r.Value, uniqueNum);
                    uniqueNum++;

                    break;

                case "points":
                    ParsePoints(r);
                    readEnd = false;
                    break;
                case "boundarySemantic":
                    BoundarySemanticType type = BoundarySemanticType.None;

                    for (int i = 0; i < r.AttributeCount; i++)
                    {    
                        r.MoveToAttribute(i);

                        // set the field in this object based on the element we just read
                        switch (r.Name)
                        {
                            case "type":
                                switch (r.Value)
                                {
                                    case "SpeedTreeForest":
                                        type = BoundarySemanticType.SpeedTreeForest;
                                        break;
                                    case "WaterPlane":
                                        type = BoundarySemanticType.WaterPlane;
                                        break;
								    case "Vegetation":
										type = BoundarySemanticType.Vegetation;
										break;
								}
                                break;
                        }
                    }
                    r.MoveToElement(); //Moves the reader back to the element node.

                    switch ( type ) 
                    {
                        case BoundarySemanticType.SpeedTreeForest:
                            Forest forest = new Forest(sceneNode, r);
                            this.AddSemantic(forest);
                            break;
                        case BoundarySemanticType.WaterPlane:
                            WaterPlane water = new WaterPlane(sceneNode, r);
                            this.AddSemantic(water);
                            break;
                        case BoundarySemanticType.Vegetation:
                            VegetationSemantic vegetationBoundary = new VegetationSemantic(r);
                            this.AddSemantic(vegetationBoundary);
                            break;
						default:
                            break;
                    }
                    readEnd = false;
                    break;
            }

            if (readEnd)
            {
                // error out if we dont see an end element here
                r.Read();
                if (r.NodeType != XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void FromXML(XmlTextReader r)
        {
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    ParseElement(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the terrain description
                    return;
                }
            }
        }

        private void OnBoundaryChange()
        {
            Close();

            foreach (IBoundarySemantic semantic in semantics)
            {
                semantic.BoundaryChange();
            }

        }

		public void AddPoint(Vector3 point)
		{
            points.Add(new Vector2(point.x, point.z));
            if (autoClose)
            {
                // if the boundary is already closed, close it again so that all the
                // computed data structures get rebuilt
                OnBoundaryChange();
            }
		}

        public void InsertPoint(int pointNum, Vector3 newPoint)
        {
            points.Insert(pointNum, new Vector2(newPoint.x, newPoint.z));
            if (autoClose)
            {
                OnBoundaryChange();
            }
        }

        public void EditPoint(int pointNum, Vector3 newPoint)
        {
            points[pointNum] = new Vector2(newPoint.x, newPoint.z);

            if (autoClose)
            {
                // if the boundary is already closed, close it again so that all the
                // computed data structures get rebuilt
                OnBoundaryChange();
            }
        }

        public void RemovePoint(int pointNum)
        {
            points.RemoveAt(pointNum);

            if (autoClose)
            {
                // if the boundary is already closed, close it again so that all the
                // computed data structures get rebuilt
                OnBoundaryChange();
            }
        }

        public void SetPoints(List<Vector3> newPts)
        {
            points.Clear();
            foreach (Vector3 v in newPts)
            {
                points.Add(new Vector2(v.x, v.z));
            }
            OnBoundaryChange();
        }

        private void Triangulate()
        {
            tris = new List<int []>();

            bool ret;

            ret = TriangleTessellation.Process(points, tris);

            if ( ret == false ) {
                // try swapping the order of the points and re-running the tessellation
                List<Vector2> newPoints = new List<Vector2>();
                for(int i = points.Count - 1; i >= 0; i-- )
                {
                    newPoints.Add(points[i]);
                }

                // clear out the triangle list before we try again
                tris.Clear();

                ret = TriangleTessellation.Process(newPoints, tris);
                if (ret == false)
                {
                    // dump out point coords that are failing
                    Console.WriteLine("Boundary triangle tessellation failed in both directions.");
                    foreach (Vector2 point in points)
                    {
                        Console.WriteLine("{0}, {1}", point.x, point.y);
                    }
                }
                else
                {
                    // adjust result list to take into account previous vertex reversal
                    int lastVert = points.Count - 1;
                    for (int i = 0; i < tris.Count; i++)
                    {
                        tris[i][0] = lastVert - tris[i][0];
                        tris[i][1] = lastVert - tris[i][1];
                        tris[i][2] = lastVert - tris[i][2];
                    }
                }
                Debug.Assert(ret == true);
            }
        }

        private void ComputeBounds()
        {
            float minX = Single.MaxValue;
            float maxX = Single.MinValue;
            float minZ = Single.MaxValue;
            float maxZ = Single.MinValue;
            
            foreach (Vector2 point in points)
            {
                if (point.x > maxX)
                {
                    maxX = point.x;
                }
                if (point.x < minX)
                {
                    minX = point.x;
                }
                if (point.y > maxZ)
                {
                    maxZ = point.y;
                }
                if (point.y < minZ)
                {
                    minZ = point.y;
                }
            }

            bounds = new AxisAlignedBox(new Vector3(minX, 0, minZ), new Vector3(maxX, 0, maxZ));
        }

        private void ComputeArea()
        {
            // XXX
        }

        private void UnClose()
        {
            touchedPages.Clear();
            tris = null;
            bounds = null;
            closed = false;
        }

		public void Close()
		{
            if (closed)
            {
                UnClose();
            }
            autoClose = true;
            closed = true;
            if (points.Count >= 3)
            {
                ComputeBounds();
                Triangulate();
                ComputeArea();
                ComputeTouchedPages();
                SetPageHilights(hilight);
            }
		}

		public bool PointIn(Vector3 p3)
		{
            Debug.Assert(closed, "Boundary not closed");
            int crossings = 0;
            Vector2 point = new Vector2(p3.x, p3.z);

            p3.y = 0;

            // see if the point falls within the bounding box
            if (bounds.Intersects(p3)) // XXX
            {
                Vector2 topPoint = new Vector2(point.x, bounds.Maximum.z);
                for (int i = 0; i < ( points.Count - 1 ); i++)
                {
                    if (IntersectSegments(points[i], points[i + 1], point, topPoint))
                    {
                        crossings++;
                    }
                }
                // check final segment
                if (IntersectSegments(points[points.Count - 1], points[0], point, topPoint))
                {
                    crossings++;
                }

                // odd number of crossings means the point is inside
                return (crossings & 1) == 1;
            }
            return false;
		}

        public List<Triangle> Clip(AxisAlignedBox box)
        {
            Debug.Assert(closed, "Boundary not closed");
            // XXX

            return null;
        }

        // see if the given square intersects with this boundary
        public bool IntersectSquare(Vector3 loc, int size)
        {
            float x1, x2, y1, y2;

            x1 = loc.x;
            x2 = loc.x + size * TerrainManager.oneMeter;
            y1 = loc.z;
            y2 = loc.z + size * TerrainManager.oneMeter;

            // if the square is outside the bounding box of the boundary, then it can't intersect
            if (bounds.Minimum.x > x2 || bounds.Maximum.x < x1 || bounds.Minimum.z > y2 || bounds.Maximum.z < y1)
            {
                return false;
            }

            // if the first point of the boundary is inside the square, then there is an intersection.
            // This test is done because if the entire boundary is inside this square, then the intersection
            // tests below will fail.
            if (points.Count > 0)
            {
                float px = points[0].x;
                float py = points[0].y;
                if (px >= x1 && px <= x2 && py >= y1 && py <= y2)
                {
                    return true;
                }
            }

            if (PointIn(loc))
            {
                // the origin is in the boundary, so we intersect
                return true;
            }

            // check each edge of the square to see if it intersects with the boundary
            if (IntersectSegment(new Vector2(x1, y1), new Vector2(x2, y1)))
            {
                return true;
            }

            if (IntersectSegment(new Vector2(x2, y1), new Vector2(x2, y2)))
            {
                return true;
            }

            if (IntersectSegment(new Vector2(x2, y2), new Vector2(x1, y2)))
            {
                return true;
            }

            if (IntersectSegment(new Vector2(x1, y2), new Vector2(x1, y1)))
            {
                return true;
            }

            return false;
        }

        // check intersection of a segment with the segments of the boundary
        private bool IntersectSegment(Vector2 p1, Vector2 p2)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (IntersectSegments(p1, p2, points[i], points[i + 1]))
                {
                    return true;
                }
            }
            if (IntersectSegments(p1, p2, points[points.Count - 1], points[0]))
            {
                return true;
            }

            return false;
        }

        // check if 2 segments intersect
        private static bool IntersectSegments(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float den = ((p4.y - p3.y) * (p2.x - p1.x)) - ((p4.x - p3.x) * (p2.y - p1.y));
            float t1num = ((p4.x - p3.x) * (p1.y - p3.y)) - ((p4.y - p3.y) * (p1.x - p3.x));
            float t2num = ((p2.x - p1.x) * (p1.y - p3.y)) - ((p2.y - p1.y) * (p1.x - p3.x));

            if ( den == 0 ) {
                return false;
            }

            float t1 = t1num / den;
            float t2 = t2num / den;

            // note that we include the endpoint of the second line in the intersection
            // test, but not the endpoint of the first line.
            if ((t1 >= 0) && (t1 < 1) && (t2 >= 0) && (t2 <= 1))
            {
                return true;
            }

            return false;

        }

        public List<int []> Triangles
        {
            get
            {
                Debug.Assert(closed, "Boundary not closed");
                return tris;
            }
        }

        public List<Vector2> Points
        {
            get
            {
                Debug.Assert(closed, "Boundary not closed");
                return points;
            }
        }

        public AxisAlignedBox Bounds
        {
            get
            {
                Debug.Assert(closed, "Boundary not closed");
                return bounds;
            }
        }

        // public float Area
        //{
        //    get
        //    {
        //        Debug.Assert(closed, "Boundary not closed");
        //        return area;
        //    }
        //}

        public int PageSize
        {
            get
            {
                return pageSize;
            }
        }

        public bool AutoClose
        {
            get
            {
                return autoClose;
            }
            set
            {
                autoClose = value;
            }
        }

        private void FillSpan(byte[] seedImage, int y, int x1, int x2)
        {
            int startX;
            int endX;

            if (y >= 0 && y < pageSize)
            { // span crosses this page
                if (x1 < x2)
                {
                    startX = x1;
                    endX = x2;
                }
                else
                {
                    startX = x2;
                    endX = x1;
                }

                if (startX < pageSize && endX > 0)
                { // ensure span crosses the page
                    if (startX < 0)
                    {
                        startX = 0;
                    }
                    if (endX > pageSize)
                    {
                        endX = pageSize;
                    }

                    int offset = y * pageSize;
                    for (int x = startX; x < endX; x++)
                    {
                        seedImage[offset + x] = 255;
                    }
                }
            }
        }

        // p0, p1, and p2 are sorted
        private void RasterizeTriangle(Vector3 pageLoc, Vector2 p0, Vector2 p1, Vector2 p2, byte[] seedImage)
        {
            float x0 = (float)Math.Floor((p0.x - pageLoc.x) / TerrainManager.oneMeter);
            int y0 = (int)Math.Floor((p0.y - pageLoc.z) / TerrainManager.oneMeter);
            float x1 = (float)Math.Floor((p1.x - pageLoc.x) / TerrainManager.oneMeter);
            int y1 = (int)Math.Floor((p1.y - pageLoc.z) / TerrainManager.oneMeter);
            float x2 = (float)Math.Floor((p2.x - pageLoc.x) / TerrainManager.oneMeter);
            int y2 = (int)Math.Floor((p2.y - pageLoc.z) / TerrainManager.oneMeter);

            if (y2 < 0 || y0 >= pageSize)
            { // triangle is outside of the page in y coord range
                return;
            }

            float dx0 = (x1 - x0) / (y1 - y0);
            float dx1 = (x2 - x0) / (y2 - y0);
            float dx2 = (x2 - x1) / (y2 - y1);

            float xEdge0 = x0;
            float xEdge1 = x0;

            for (int y = y0; y < y1; y++)
            {
                FillSpan(seedImage, y, (int)Math.Floor(xEdge0), (int)Math.Floor(xEdge1));

                xEdge0 += dx0;
                xEdge1 += dx1;
            }

            for (int y = y1; y <= y2; y++)
            {
                FillSpan(seedImage, y, (int)Math.Floor(xEdge0), (int)Math.Floor(xEdge1));

                xEdge0 += dx2;
                xEdge1 += dx1;
            }

            return;
        }

        private Image BuildPageMask(Vector3 loc)
        {
            byte[] seedImage = new byte[pageSize * pageSize];

            Debug.Assert(closed);

            foreach (int[] tri in tris)
            {
                Vector2 p0 = new Vector2(points[tri[0]].x, points[tri[0]].y);
                Vector2 p1 = new Vector2(points[tri[1]].x, points[tri[1]].y);
                Vector2 p2 = new Vector2(points[tri[2]].x, points[tri[2]].y);
                Vector2 ptmp;

                //
                // sort by y coordinate
                //
                if (p1.y < p0.y)
                {
                    // swap p0 and p1
                    ptmp = p0;
                    p0 = p1;
                    p1 = ptmp;
                }

                if (p2.y < p0.y)
                {
                    // move p2 to the front
                    ptmp = p1;
                    p1 = p0;
                    p0 = p2;
                    p2 = ptmp;
                }
                else if (p2.y < p1.y)
                {
                    // swap p1 and p2
                    ptmp = p1;
                    p1 = p2;
                    p2 = ptmp;
                }

                RasterizeTriangle(loc, p0, p1, p2, seedImage);
            }

            return Image.FromDynamicImage(seedImage, pageSize, pageSize, PixelFormat.A8);
        }

        private Texture PageMaskTexture(PageCoord pc)
        {
            Image i = BuildPageMask(pc.WorldLocation(pageSize));

            String texName = String.Format("{0}-{1}", name, pc.ToString());
            Texture t = TextureManager.Instance.LoadImage(texName, i);

            return t;
        }

        private void ComputeTouchedPages()
        {
            Debug.Assert(closed);

            // compute the min and max page coord of the pages that this boundary may intersect with
            boundsMinPage = new PageCoord(bounds.Minimum, pageSize);
            boundsMaxPage = new PageCoord(bounds.Maximum, pageSize);

            touchedPages.Clear();

            // iterate over the pages and see which intersect with the boundary, and add
            // those to the list
            for (int x = boundsMinPage.X; x <= boundsMaxPage.X; x++)
            {
                for (int z = boundsMinPage.Z; z <= boundsMaxPage.Z; z++)
                {
                    PageCoord pc = new PageCoord(x, z);
                    if (IntersectSquare(pc.WorldLocation(pageSize), pageSize))
                    {
                        touchedPages.Add(pc);
                    }
                }
            }

            Console.WriteLine("boundary {0} touches pages:", name);
            foreach (PageCoord tp in touchedPages)
            {
                Console.WriteLine("  {0}", tp.ToString());
            }
        }

        public bool Hilight
        {
            get
            {
                return hilight;
            }
            set
            {
                hilight = value;

                if (closed)
                {
                    SetPageHilights(value);
                }
            }
        }

        public void AddSemantic(IBoundarySemantic semantic)
        {
            semantics.Add(semantic);
            semantic.AddToBoundary(this);
        }

        public void RemoveSemantic(IBoundarySemantic semantic)
        {
            semantics.Remove(semantic);
            semantic.RemoveFromBoundary();
        }

        public List<IBoundarySemantic> GetSemantics(string type)
        {
            List<IBoundarySemantic> ret = new List<IBoundarySemantic>();
            foreach (IBoundarySemantic bs in semantics)
            {
                if (bs.Type == type)
                {
                    ret.Add(bs);
                }
            }

            return ret;
        }

        public void PerFrameSemantics(float time, Camera camera)
        {
            foreach (IBoundarySemantic semantic in semantics)
            {
                semantic.PerFrameProcessing(time, camera);
            }
        }

        public void PageShift()
        {
            // set the visible flag if any of part of this boundary overlaps the visible region around the camera
            UpdateVisibility();

            if (hilight && visible)
            { // display hilights pages that are visible and overlap the boundary
                SetPageHilights(true);
            }

            // notify all boundary semantics of the page shift
            foreach (IBoundarySemantic semantic in semantics)
            {
                semantic.PageShift();
            }
        }

        private void UpdateVisibility()
        {
            PageCoord minVis = TerrainManager.Instance.MinVisiblePage;
            PageCoord maxVis = TerrainManager.Instance.MaxVisiblePage;

            visible = false;
            if (maxVis.X < boundsMinPage.X || minVis.X > boundsMaxPage.X || maxVis.Z < boundsMinPage.Z || minVis.Z > boundsMaxPage.Z)
            {
                // this boundary doesn't overlap with the visible area around the camera.
                return;
            }

            foreach (PageCoord tp in touchedPages)
            {
                if (tp.X >= minVis.X && tp.X <= maxVis.X && tp.Z >= minVis.Z && tp.Z <= maxVis.Z)
                {
                    // tp page is visible
                    visible = true;
                }
            }
        }



        private void SetPageHilights(bool value)
        {
            PageCoord minVis = TerrainManager.Instance.MinVisiblePage;
            PageCoord maxVis = TerrainManager.Instance.MaxVisiblePage;

            foreach (PageCoord tp in touchedPages)
            {
                if (tp.X >= minVis.X && tp.X <= maxVis.X && tp.Z >= minVis.Z && tp.Z <= maxVis.Z)
                {
                    // tp page is visible
                    visible = true;

                    PageCoord pageOff = tp - minVis;
                    Page page = TerrainManager.Instance.LookupPage(pageOff.X, pageOff.Z);
                    if (value)
                    {
                        page.TerrainPage.HilightMask = PageMaskTexture(tp);
                        page.TerrainPage.HilightType = TerrainPage.PageHilightType.Colorized;
                    }
                    else
                    {
                        page.TerrainPage.HilightType = TerrainPage.PageHilightType.None;

                        // if there is an existing hilight mask, make sure we free it properly
                        Texture t = page.TerrainPage.HilightMask;
                        if (t != null)
                        {
                            page.TerrainPage.HilightMask = null;
                            t.Dispose();
                        }
                    }
                }
            }
        }

        public bool IntersectPage(PageCoord pc)
        {
            // check if page intersects bounds first
            if (pc.X < boundsMinPage.X || pc.X > boundsMaxPage.X || pc.Z < boundsMinPage.Z || pc.Z > boundsMaxPage.Z)
            {
                return false;
            }

            foreach (PageCoord tp in touchedPages)
            {
                if (tp == pc)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Visible
        {
            get
            {
                return visible;
            }
        }

        public void FindObstaclesInBox(AxisAlignedBox box, 
                                       CollisionTileManager.AddTreeObstaclesCallback callback)
        {
            Debug.Assert(bounds != null, "When calling FindObstaclesInBox, bounds must be non-null");
            if (bounds.Intersects(box)) {
                foreach (IBoundarySemantic semantic in semantics) {
                    Forest f = semantic as Forest;
                    if (f != null) {
                        f.FindObstaclesInBox(box, callback);
                    }
                }
            }
        }
		
        public class Triangle
        {
            Vector3 [] verts;

            Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                verts = new Vector3[3];
                verts[0] = v1;
                verts[1] = v2;
                verts[2] = v3;
            }

            public Vector3 this[int i] {
                get {
                    return verts[i];
                }
                set
                {
                    verts[i] = value;
                }
            }
        }

        public void ToXML(XmlTextWriter w)
        {
            w.WriteStartElement("boundary");
            w.WriteElementString("name", name);
            w.WriteStartElement("points");
            foreach (Vector2 p in points)
            {
                w.WriteStartElement("point");
                w.WriteAttributeString("x", p.x.ToString());
                w.WriteAttributeString("y", p.y.ToString());
                w.WriteEndElement();
            }
            w.WriteEndElement();

            foreach (IBoundarySemantic semantic in semantics)
            {
                semantic.ToXML(w);
            }
            w.WriteEndElement();
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (IBoundarySemantic semantic in semantics)
            {
                semantic.Dispose();
            }
            sceneNode.Creator.DestroySceneNode(sceneNode.Name);
        }

        #endregion
    }

    public enum BoundarySemanticType
    {
        None,
        SpeedTreeForest,
        TerrainTexture,
        WaterPlane,
		Vegetation
	}

    public class TouchedPage : IDisposable
    {
        private PageCoord coord;
        private Texture mask;

        public TouchedPage(PageCoord coord, Texture mask)
        {
            this.coord = coord;
            this.mask = mask;
        }

        public PageCoord Coord
        {
            get
            {
                return coord;
            }
        }

        public Texture Mask
        {
            get
            {
                return mask;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mask != null)
            {
                mask.Dispose();
            }
        }

        #endregion
    }
}
