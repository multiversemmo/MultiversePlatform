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
using System.Diagnostics;
using System.Text;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class SparsePageMask
    {
        public delegate void ReturnMask(PageCoord pc, byte[] mask);

        private Dictionary<PageCoord, byte[]> pages;
        private int pageSize;

        int minPageX;
        int minPageZ;
        int maxPageX;
        int maxPageZ;

        public SparsePageMask(int pageSize)
        {
            pages = new Dictionary<PageCoord, byte[]>();
            this.pageSize = pageSize;
            ResetBounds();
        }

        private void ResetBounds()
        {
            minPageX = int.MaxValue;
            minPageZ = int.MaxValue;
            maxPageX = int.MinValue;
            maxPageZ = int.MinValue;
        }

        public void Clear()
        {
            ResetBounds();
            pages.Clear();
        }

        private byte[] newMask(PageCoord pc)
        {
            // allocate the new mask
            byte[] mask = new byte[pageSize * pageSize];

            // add it to the dictionary
            pages.Add(pc, mask);

            // update bounds
            if (pc.X < minPageX)
            {
                minPageX = pc.X;
            }
            if (pc.X > maxPageX)
            {
                maxPageX = pc.X;
            }
            if (pc.Z < minPageZ)
            {
                minPageZ = pc.Z;
            }
            if (pc.Z > maxPageZ)
            {
                maxPageZ = pc.Z;
            }
            return mask;
        }

        public bool Intersects(PageCoord pc)
        {
            bool ret = false;
            // do bounds intersection first
            if ((pc.X >= minPageX) && (pc.X <= maxPageX) && (pc.Z >= minPageZ) && (pc.Z <= maxPageZ))
            {
                byte[] mask;
                // if bounds intersection succeeds, then look up in the dictionary
                ret = pages.TryGetValue(pc, out mask);
            }

            return ret;
        }

        public void SetPoint(int x, int z, byte value)
        {
            // compute pagecoord of the page containing this point
            PageCoord pc = new PageCoord(new Vector3(x * TerrainManager.oneMeter, 0, z * TerrainManager.oneMeter), pageSize);
            // PageCoord pc = new PageCoord(x / pageSize, z / pageSize);
            byte[] rawMask;

            // lookup the mask.  If it is not found, then make a new empty mask and add it to the dictionary
            if (!pages.TryGetValue(pc, out rawMask))
            {
                rawMask = newMask(pc);
            }

            // compute x and z coords of base of page
            int pagex = pc.X * pageSize;
            int pagez = pc.Z * pageSize;

            // compute x and z offsets within page
            int xoff = x - pagex;
            int zoff = z - pagez;

            rawMask[(zoff * pageSize) + xoff] = value;
        }

        // Get the mask for the given PageCoord.  Returns null if no mask is available
        public byte[] GetMask(PageCoord pc)
        {
            byte[] rawMask;
            if (!pages.TryGetValue(pc, out rawMask))
            {
                rawMask = null;
            }

            return rawMask;
        }

        //
        // iterage all page masks, returning them via the delegate callback
        //
        public void IterateMasks(ReturnMask retDelegate)
        {
            foreach (KeyValuePair<PageCoord, byte[]> kvp in pages)
            {
                retDelegate(kvp.Key, kvp.Value);
            }
        }

        //
        // Iterate page masks, only returning those in the given page range
        //
        public void IterateMasks(ReturnMask retDelegate, PageCoord minPage, PageCoord maxPage)
        {
            // get out quick if our bounds don't overlap the requested page range
            if ((minPageX > maxPage.X) || (maxPageX < minPage.X) || (minPageZ > maxPage.Z) || (maxPageZ < minPage.Z))
            {
                return;
            }
            foreach (KeyValuePair<PageCoord, byte[]> kvp in pages)
            {
                PageCoord pc = kvp.Key;

                // only call delegate if current page overlaps with requested page range
                if ((pc.X >= minPage.X) && (pc.X <= maxPage.X) && (pc.Z >= minPage.Z) && (pc.Z <= maxPage.Z))
                {
                    retDelegate(pc, kvp.Value);
                }
            }
        }

        public int PageSize
        {
            get
            {
                return pageSize;
            }
        }
    }
}
