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
using Axiom.MathLib;
using System.Diagnostics;

namespace Axiom.SceneManagers.Multiverse
{
    /// <summary>
    /// This class holds all of the SubPageHeightMaps for a page.  
    /// </summary>
    public class PageHeightMap
    {
        private SubPageHeightMap[] subPages;

        private int subPagesPerPage;
        private int subPageSize;
        private int pageSize;

        private Vector3 location;
        private bool locationSet = false;

        public PageHeightMap(int subPagesPerPage, int pageSize, int maxMetersPerSample, int minMetersPerSample)
        {
            this.subPagesPerPage = subPagesPerPage;
            this.pageSize = pageSize;
            subPageSize = pageSize / subPagesPerPage;

            // allocate the array to hold the subpages
            subPages = new SubPageHeightMap[subPagesPerPage * subPagesPerPage];

            // allocate all the subpages
            for ( int z = 0; z < subPagesPerPage; z++) {
                for (int x = 0; x < subPagesPerPage; x++)
                {
                    subPages[x + z * subPagesPerPage] = new SubPageHeightMap(pageSize / subPagesPerPage, maxMetersPerSample, minMetersPerSample, x, z);
                }
            }
        }

        public SubPageHeightMap LookupSubPage(int x, int z)
        {
            return subPages[x + z * subPagesPerPage];
        }

        public SubPageHeightMap LookupSubPage(Vector3 loc)
        {
            // make loc relative to page
            loc = loc - location;

            int xoff = (int)Math.Floor(loc.x / (((int)TerrainManager.oneMeter) * subPageSize));
            int zoff = (int)Math.Floor(loc.z / (((int)TerrainManager.oneMeter) * subPageSize));

            return subPages[xoff + zoff * subPagesPerPage];
        }

        /// <summary>
        /// Get the height of the sample at the given coordinates within the page.
        /// Coordinates are in meters, and are relative to the page.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public float GetHeight(int x, int z)
        {
            int subPageMask = subPageSize - 1;

            // figure X and Z offsets of the subpage that contains the given point
            int subPageX = x / subPageSize;
            int subPageZ = z / subPageSize;

            SubPageHeightMap subPage = subPages[subPageX + subPageZ * subPagesPerPage];
            return subPage.GetHeight(x & subPageMask, z & subPageMask);
        }

        public float GenHeight(int x, int z)
        {
            return TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                new Vector3(location.x + x * TerrainManager.oneMeter, 0, location.z + z * TerrainManager.oneMeter));
        }

        public Vector3 GetNormal(int x, int z)
        {
            int metersPerSample = TerrainManager.Instance.MetersPerSample(new Vector3(location.x + x * TerrainManager.oneMeter, 0, location.z + z * TerrainManager.oneMeter));
            if (metersPerSample == 0)
            {
                metersPerSample = TerrainManager.Instance.MaxMetersPerSample;
            }
            float x1 = 0, x2 = 0, z1 = 0, z2 = 0;

            int x1off = x - metersPerSample;
            int x2off = x + metersPerSample;

            if (x1off < 0 || z >= pageSize)
            {
                x1 = GenHeight(x1off, z);
            }
            else
            {
                x1 = GetHeight(x1off, z);
            }

            if (x2off >= pageSize || z >= pageSize)
            {
                x2 = GenHeight(x2off, z);
            }
            else
            {
                x2 = GetHeight(x2off, z);
            }

            int z1off = z - metersPerSample;
            int z2off = z + metersPerSample;

            if (z1off < 0 || x >= pageSize)
            {
                z1 = GenHeight(x, z1off);
            }
            else
            {
                z1 = GetHeight(x, z1off);
            }

            if (z2off >= pageSize || x >= pageSize)
            {
                z2 = GenHeight(x, z2off);
            }
            else
            {
                z2 = GetHeight(x, z2off);
            }

            float unitsPerSample = metersPerSample * TerrainManager.oneMeter;

            // computer the normal
            Vector3 v = new Vector3(x1 - x2,
                2.0f * unitsPerSample,
                z1 - z2);
            v.Normalize();

            return v;
        }

        public float GetAreaHeight(float fx1, float fx2, float fz1, float fz2)
        {
            float height = float.MinValue;

            // make page relative and convert to samples
            int x1 = (int)Math.Floor((fx1 - location.x)/TerrainManager.oneMeter);
            int x2 = (int)Math.Floor((fx2 - location.x)/TerrainManager.oneMeter);
            int z1 = (int)Math.Floor((fz1 - location.z)/TerrainManager.oneMeter);
            int z2 = (int)Math.Floor((fz2 - location.z)/TerrainManager.oneMeter);

            // clip to the page
            if (x1 < 0)
            {
                x1 = 0;
            }
            if (x2 > ( pageSize - 1 ))
            {
                x2 = pageSize - 1;
            }
            if (z1 < 0)
            {
                z1 = 0;
            }
            if (z2 > ( pageSize - 1 ))
            {
                z2 = pageSize - 1;
            }

            // compute which sub pages we need to check
            int startSubPageX = x1 / subPageSize;
            int endSubPageX = x2 / subPageSize;
            int startSubPageZ = z1 / subPageSize;
            int endSubPageZ = z2 / subPageSize;

            for (int subPageZ = startSubPageZ; subPageZ <= endSubPageZ; subPageZ++)
            {
                int subPageLocZ = subPageZ * subPageSize;
                for (int subPageX = startSubPageX; subPageX <= endSubPageX; subPageX++)
                {
                    int subPageLocX = subPageX * subPageSize;

                    // compute area bounds relative to sub page origin
                    int subPageX1 = x1 - subPageLocX;
                    int subPageX2 = x2 - subPageLocX;
                    int subPageZ1 = z1 - subPageLocZ;
                    int subPageZ2 = z2 - subPageLocZ;
                    
                    // now clip to sub page
                    if (subPageX1 < 0)
                    {
                        subPageX1 = 0;
                    }
                    if (subPageX2 > ( subPageSize - 1 ))
                    {
                        subPageX2 = subPageSize - 1;
                    }

                    if (subPageZ1 < 0)
                    {
                        subPageZ1 = 0;
                    }
                    if (subPageZ2 > (subPageSize - 1))
                    {
                        subPageZ2 = subPageSize - 1;
                    }

                    // call the subpage with the clipped bounds to get the area height
                    float subPageHeight = subPages[subPageX + subPageZ * subPagesPerPage].GetAreaHeight(subPageX1, subPageX2, subPageZ1, subPageZ2);

                    if (subPageHeight > height)
                    {
                        height = subPageHeight;
                    }
                }
            }

            return height;
        }

        public void SetPatchLOD(int startX, int startZ, int size, int metersPerSample)
        {
            int xoff = startX / subPageSize;
            int zoff = startZ / subPageSize;
            int numSubPages = size / subPageSize;

            for (int z = 0; z < numSubPages; z++)
            {
                for (int x = 0; x < numSubPages; x++)
                {
                    subPages[x + xoff + (z + zoff) * subPagesPerPage].MetersPerSample = metersPerSample;
                }
            }
        }

        public void GetSubPageHeightBounds(int xOff, int zOff, int xSize, int zSize, out float minHeight, out float maxHeight)
        {

            // convert coords/sizes to number of subpages
            xOff = xOff / subPageSize;
            zOff = zOff / subPageSize;
            xSize = xSize / subPageSize;
            zSize = zSize / subPageSize;

            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            for (int z = zOff; z < (zOff + zSize); z++)
            {
                for (int x = xOff; x < (xOff + xSize); x++)
                {
                    SubPageHeightMap subPage = subPages[x + z * subPagesPerPage];
                    if (subPage.MaxHeight > maxHeight)
                    {
                        maxHeight = subPage.MaxHeight;
                    }
                    if (subPage.MinHeight < minHeight)
                    {
                        minHeight = subPage.MinHeight;
                    }
                }
            }
        }

        /// <summary>
        /// Set the location of all the sub pages in this page
        /// 
        /// Location is in world coordinate space (1000 samples per meter)
        /// </summary>
        public Vector3 Location
        {
            get
            {
                return location;
            }
            set
            {
                if (!locationSet || (location != value))
                {
                    locationSet = true;
                    location = value;
                    int offset = 0;
                    float subPageWorldSize = subPageSize * TerrainManager.oneMeter;
                    float locz = value.z;
                    for (int z = 0; z < subPagesPerPage; z++)
                    {
                        float locx = value.x;
                        for (int x = 0; x < subPagesPerPage; x++)
                        {
                            subPages[offset].Location = new Vector3(locx, 0, locz);

                            locx += subPageWorldSize;
                            offset++;
                        }
                        locz += subPageWorldSize;
                    }
                }
            }
        }

        public void ResetHeightMaps()
        {
            foreach (SubPageHeightMap subPage in subPages)
            {
                subPage.ResetHeightMaps();
            }
        }
    }
}
