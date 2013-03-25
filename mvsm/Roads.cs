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
using System.Collections.Generic;
using System.Text;
using Axiom.Media;
using Axiom.Core;
using System.Diagnostics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class Roads : IDisposable
    {
        List<Road> roads;
        SparsePageMask mask;

        public Roads()
        {
            roads = new List<Road>();

            mask = new SparsePageMask(TerrainManager.Instance.PageSize);

            TerrainManager.Instance.PageVisibility += new PageVisibilityEventHandler(PageVisibilityHandler);
        }

        public Road CreateRoad(String name)
        {
            Road road = new Road(name, this);

            roads.Add(road);

            return road;
        }

        public void RemoveRoad(Road road)
        {
            roads.Remove(road);

            road.Dispose();

            RebuildMask();
        }

        private void PageVisibilityHandler(object sender, PageVisibilityEventArgs msg)
        {
            TerrainPage terrainPage = sender as TerrainPage;

            if (msg.visible)
            {
                // the page has become visible, so generate and assign the hilight mask texture
                Debug.Assert(terrainPage.HilightMask == null);

                SetRoadMask(terrainPage);
            }
            else
            {
                // the page is moving out of the visible area, so free up the mask textures
                if ( terrainPage.HilightMask != null ) 
                {
                    Texture hilightMask = terrainPage.HilightMask;

                    terrainPage.HilightType = TerrainPage.PageHilightType.None;
                    terrainPage.HilightMask = null;

                    TextureManager.Instance.Unload(hilightMask);
                }
            }
        }

        private void SetRoadMask(TerrainPage terrainPage)
        {
            // generate a texture from the mask
            PageCoord pc = new PageCoord(terrainPage.Location, TerrainManager.Instance.PageSize);

            byte[] byteMask = mask.GetMask(pc);
            if (byteMask != null)
            {

                Image maskImage = Image.FromDynamicImage(byteMask, mask.PageSize, mask.PageSize, PixelFormat.A8);
                String texName = String.Format("RoadMask-{0}", pc.ToString());
                Texture texImage = TextureManager.Instance.LoadImage(texName, maskImage);

                terrainPage.HilightMask = texImage;
                terrainPage.HilightType = TerrainPage.PageHilightType.EdgeSharpBlend;
            }
        }

        private void UpdateRoadMaskTextures()
        {
            int numPages = TerrainManager.Instance.PageArraySize;

            for (int x = 0; x < numPages; x++)
            {
                for (int z = 0; z < numPages; z++)
                {
                    TerrainPage terrainPage = TerrainManager.Instance.LookupPage(x, z).TerrainPage;
                    if ( (terrainPage.HilightMask != null) && (terrainPage.HilightType == TerrainPage.PageHilightType.EdgeSharpBlend) )
                    {
                        terrainPage.HilightType = TerrainPage.PageHilightType.None;
                        terrainPage.HilightMask.Dispose();
                        terrainPage.HilightMask = null;
                    }

                    SetRoadMask(terrainPage);
                }
            }
        }

        //private void SetPageHilights()
        //{
        //    PageCoord minVis = TerrainManager.Instance.MinVisiblePage;
        //    PageCoord maxVis = TerrainManager.Instance.MaxVisiblePage;

        //    mask.IterateMasks(new SparsePageMask.ReturnMask(SetRoadMask), minVis, maxVis);
        //}

        // when one of the roads changes, we have to rebuild the mask
        public void RebuildMask()
        {
            mask.Clear();
            foreach (Road r in roads)
            {
                r.RenderMask();
            }

            // XXX - reset hilight masks on all pages when roads change
            UpdateRoadMaskTextures();
        }

        public SparsePageMask Mask
        {
            get
            {
                return mask;
            }
        }

        // check if 2 segments intersect
        private static bool IntersectSegments(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float den = ((p4.y - p3.y) * (p2.x - p1.x)) - ((p4.x - p3.x) * (p2.y - p1.y));
            float t1num = ((p4.x - p3.x) * (p1.y - p3.y)) - ((p4.y - p3.y) * (p1.x - p3.x));
            float t2num = ((p2.x - p1.x) * (p1.y - p3.y)) - ((p2.y - p1.y) * (p1.x - p3.x));

            if (den == 0)
            {
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

        private static bool PointInsideBlock(Vector2 blockCornerMin, Vector2 blockCornerMax, Vector2 pt)
        {
            return ((pt.x >= blockCornerMin.x) && (pt.x <= blockCornerMax.x) &&
                (pt.y >= blockCornerMin.y) && (pt.y <= blockCornerMax.y));
        }

        private static bool IntersectBlockSegment(Vector2 blockCornerMin, Vector2 blockCornerMax, Vector2 p1, Vector2 p2)
        {
            if (PointInsideBlock(blockCornerMin, blockCornerMax, p1) || PointInsideBlock(blockCornerMin, blockCornerMax, p2))
            {
                // if either point is inside the block, then we have an intersection
                return true;
            }

            // XXX - add quick reject here

            Vector2 c1 = new Vector2(blockCornerMin.x, blockCornerMax.y);
            Vector2 c2 = new Vector2(blockCornerMax.x, blockCornerMin.y);
            if (IntersectSegments(p1, p2, blockCornerMin, c1) ||
                IntersectSegments(p1, p2, blockCornerMin, c2) ||
                IntersectSegments(p1, p2, blockCornerMax, c1) ||
                IntersectSegments(p1, p2, blockCornerMax, c2))
            {
                return true;
            }

            return false;
        }

        public List<Vector2> IntersectBlock(Vector2 blockCornerMin, Vector2 blockCornerMax)
        {
            List<Vector2> retSegments = new List<Vector2>();

            foreach (Road r in roads)
            {
                List<Vector2> roadPts = r.Points;

                for (int i = 0; i < roadPts.Count - 1; i++)
                {
                    if (IntersectBlockSegment(blockCornerMin, blockCornerMax, roadPts[i], roadPts[i + 1]))
                    {
                        retSegments.Add(roadPts[i]);
                        retSegments.Add(roadPts[i + 1]);
                    }
                }
            }

            return retSegments;
        }

        #region IDisposable Members

        public void Dispose()
        {

            TerrainManager.Instance.PageVisibility -= new PageVisibilityEventHandler(PageVisibilityHandler);

            // dispose any roads still in collection
            foreach (Road r in roads)
            {
                r.Dispose();
            }
        }

        #endregion
    }
}
