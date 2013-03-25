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

using Axiom.MathLib;
using System.Diagnostics;

namespace Axiom.SceneManagers.Multiverse
{
    public class SubPageHeightMap
    {
        // location in world space for this subPage
        private Vector3 location;
        // private bool locationSet = false; (unused)

        // meters per sample for the lowest level of detail
        private readonly int maxMetersPerSample;

        // meters per sample for the highest level of detail
        private readonly int minMetersPerSample;

        // total number of possible LOD levels
        private readonly int lodLevels;

        // current meters per sample
        private int curMetersPerSample;

        // how many LOD levels do we currently have heightfields for
        private int curLodLevels;

        // size of the SubPage along one axis in meters
        private readonly int size;

        private readonly float[][] heightFields;

        // min and max height values.  Used for computing bounding boxes 
        private float maxHeight;
        private float minHeight;

        private readonly int indexX;
        private readonly int indexZ;

        private void ResetBounds()
        {
            // Reset the min and max heights for the heightmap
            maxHeight = float.MinValue;
            minHeight = float.MaxValue;
        }

        private void UpdateBounds(float height)
        {
            // update min and max heights with the current height
            if (height > maxHeight)
            {
                maxHeight = height;
            }
            if (height < minHeight)
            {
                minHeight = height;
            }
        }

        public SubPageHeightMap(int size, int maxMetersPerSample, int minMetersPerSample, int indexX, int indexZ)
        {
            this.size = size;
            this.maxMetersPerSample = maxMetersPerSample;
            this.minMetersPerSample = minMetersPerSample;
            this.indexX = indexX;
            this.indexZ = indexZ;

            // compute the max number of lod levels this subpage will have to deal with
            int lodTmp = maxMetersPerSample / minMetersPerSample;
            lodLevels = 0;
            while (lodTmp != 0)
            {
                lodLevels++;
                lodTmp = lodTmp >> 1;
            }

            heightFields = new float[lodLevels][];

            ResetBounds();
        }

        // convert metersPerSample to an LOD level
        // LOD is chosen based on lowest significant bit set
        // LOD 0 is the lowest LOD (highest meters per sample)
        private int ComputeLod(int value)
        {
            int lod;
            int mpsRange = value / minMetersPerSample;

            if (mpsRange == 0)
            {
                lod = 0;
            }
            else
            {
                lod = lodLevels - 1;
                while (((mpsRange & 1) == 0 && (lod != 0)))
                {
                    mpsRange = mpsRange >> 1;
                    lod--;
                }
            }
            return lod;
        }

        private int LODMetersPerSample(int lod)
        {
            int mps = ( 1 << (lodLevels - 1 - lod) ) * minMetersPerSample;

            return mps;
        }

        //
        // scales a coordinate from meters to an offset in the current lod
        //
        private int LODScaleCoord(int coord, int lod)
        {
            int ret = (coord / minMetersPerSample) >> (lodLevels - 1 - lod);

            return ret;
        }

        private float[] allocHeightField(int metersPerSample)
        {
            int samplesPerPage = size / metersPerSample;

            int numSamples;

            if (metersPerSample == maxMetersPerSample)
            {
                // at the lowest LOD, there is no chunk out of the heightmap
                numSamples = samplesPerPage * samplesPerPage;
            }
            else
            {
                // compute the total number of samples, then get rid of 1/4 that belong to lower LODs
                numSamples = samplesPerPage * samplesPerPage / 4 * 3;
            }

            float[] heightField = new float[numSamples];

            heightFields[ComputeLod(metersPerSample)] = heightField;
            return heightField;
        }

        private int computeHeightFieldOffset(int x, int z, int quadshift)
        {
            // Extract the bottom bits of the x and z coords and invert them.
            // These bits will form the top 2 bits of the returned offset.
            // Since both bits (before inversion) can't be 0, once inverted,
            //  both bits will not be 1.  This is to prevent overflowing our
            //  LOD buffer, which is 3/4's of size*size
            int quadx = (x & 1) ^ 1;
            int quadz = (z & 1) ^ 1;

            int subx = x >> 1;
            int subz = z >> 1;

            return ( quadz << (quadshift*2+1) ) | (quadx << (quadshift*2)) | (subz << quadshift) | subx;
        }

        private void setHeightInLOD(int x, int z, float[] heightField, int quadshift, float height)
        {
            int offset = computeHeightFieldOffset(x, z, quadshift);

            heightField[offset] = height;
        }

        private float getHeightInLOD(int x, int z, float[] heightField, int quadshift)
        {
            int offset = computeHeightFieldOffset(x, z, quadshift);

            return heightField[offset];
        }

        public float GetAreaHeight(int x1, int x2, int z1, int z2)
        {
            float height = float.MinValue;
            // adjust the bounds to nearest point in the current LOD, rounding down for low
            // bounds and rounding up for high bounds.
            int lodX1 = x1 / curMetersPerSample * curMetersPerSample;
            int lodX2 = ( x2 + curMetersPerSample - 1 ) / curMetersPerSample * curMetersPerSample;
            int lodZ1 = z1 / curMetersPerSample * curMetersPerSample;
            int lodZ2 = ( z2 + curMetersPerSample - 1 ) / curMetersPerSample * curMetersPerSample;

            if (lodX2 >= size) 
            {
                lodX2 = size - curMetersPerSample;
            }
            if (lodZ2 >= size) 
            {
                lodZ2 = size - curMetersPerSample;
            }

            for (int z = lodZ1; z <= lodZ2; z += curMetersPerSample)
            {
                for (int x = lodX1; x <= lodX2; x += curMetersPerSample)
                {
                    float newHeight = GetHeight(x, z);
                    if (newHeight > height)
                    {
                        height = newHeight;
                    }
                }
            }
            return height;
        }

        public bool CoordsValid(int x, int z)
        {
            int lod = ComputeLod(x | z);
            return ( lod < curLodLevels );
        }

        // If the requested point is not in the current LOD, then generate it
        //
        // NOTE - x and z are in meters
        public float GetHeight(int x, int z)
        {
            // compute the LOD of the target point based on the lowest bit set in x and z
            int lod = ComputeLod(x | z);

            // if we don't have the correct heightField for the given point then go to
            // the highest one we have.
            // Debug.Assert(lod < curLodLevels);
            if (lod > ( curLodLevels - 1 ) )
            {
                return TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                    new Vector3(location.x + x * TerrainManager.oneMeter, 0, location.z + z * TerrainManager.oneMeter));
            }

            float[] heightField = heightFields[lod];

            Debug.Assert(heightField != null);

            // scale coords for the correct lod heightfield
            x = LODScaleCoord(x, lod);
            z = LODScaleCoord(z, lod);

            int lodMps = LODMetersPerSample(lod);
            int lodSize = size / lodMps;

            int quadshift = 0;
            int tmp = lodSize >> 2;
            while (tmp != 0)
            {
                quadshift++;
                tmp = tmp >> 1;
            }

            return getHeightInLOD(x, z, heightField, quadshift);
        }

        private void FillLod(int metersPerSample)
        {
            bool lowestLod = (metersPerSample == maxMetersPerSample);
            int lod = ComputeLod(metersPerSample);

            float[] heightField = heightFields[lod] ?? allocHeightField(metersPerSample);

            // multiply the x or z offset by this to scale to world coords
            float worldAdjust = metersPerSample * TerrainManager.oneMeter;

            int lodSize = size / metersPerSample;

            int quadshift = 0;
            int tmp = lodSize >> 2;
            while (tmp != 0)
            {
                quadshift++;
                tmp = tmp >> 1;
            }

            float worldz = location.z;
            for (int z = 0; z < lodSize; z+=2)
            {
                float worldx = location.x;

                for (int x = 0; x < lodSize; x+=2)
                {
                    float height = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                        new Vector3(worldx + worldAdjust, 0, worldz));
                    setHeightInLOD(x + 1, z, heightField, quadshift, height);
                    UpdateBounds(height);

                    height = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                        new Vector3(worldx, 0, worldz + worldAdjust));
                    setHeightInLOD(x, z + 1, heightField, quadshift, height);
                    UpdateBounds(height);

                    height = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                        new Vector3(worldx + worldAdjust, 0, worldz + worldAdjust));
                    setHeightInLOD(x + 1, z + 1, heightField, quadshift, height);
                    UpdateBounds(height);

                    // at lowest lod we do all 4 quadrants
                    if (lowestLod)
                    {
                        height = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(
                            new Vector3(worldx, 0, worldz));
                        setHeightInLOD(x, z, heightField, quadshift, height);
                        UpdateBounds(height);
                    }
                    worldx += 2 * worldAdjust;
                }
                worldz += 2 * worldAdjust;
            }
        }

        /// <summary>
        /// Setting the Location of a subPageHeightMap causes it to clear out the current
        /// height values, and generate the lowest LOD in the new location
        /// 
        /// Location is in world coordinates (1000 units per meter)
        /// </summary>
        public Vector3 Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
                // locationSet = true;

                ResetHeightMaps();
            }
        }

        /// <summary>
        /// Increasing the meters per sample causes new LODs to be computed
        /// Currently we do nothing if the meters per sample is lowered
        /// </summary>
        public int MetersPerSample
        {
            get
            {
                return curMetersPerSample;
            }
            set
            {
                // keep adding an LOD until we get to the requested meters per sample
                while (value < curMetersPerSample)
                {
                    curMetersPerSample = curMetersPerSample / 2;
                    curLodLevels++;
                    FillLod(curMetersPerSample);
                }
            }
        }

        public int IndexX
        {
            get
            {
                return indexX;
            }
        }

        public int IndexZ
        {
            get
            {
                return indexZ;
            }
        }

        /// <summary>
        /// Make sure the heightmap has the LOD required by the application LOD spec
        /// </summary>
        public void ValidateLOD()
        {
            MetersPerSample = TerrainManager.Instance.MetersPerSample(location);
        }

        /// <summary>
        /// Make sure the heightmap has the LOD required by the application LOD spec, plus
        /// some additional LOD levels
        /// </summary>
        /// <param name="adjust">Number of additional LODs required</param>
        public void ValidateLOD(int adjust)
        {
            int mps = TerrainManager.Instance.MetersPerSample(location) >> adjust;

            if (mps < minMetersPerSample)
            {
                mps = minMetersPerSample;
            }

            MetersPerSample = mps;
        }

        public float MinHeight
        {
            get
            {
                return minHeight;
            }
        }

        public float MaxHeight
        {
            get
            {
                return maxHeight;
            }
        }

        public AxisAlignedBox BoundingBox
        {
            get
            {
                float worldSize = size * TerrainManager.oneMeter;
                return new AxisAlignedBox(new Vector3(location.x, MinHeight, location.z),
                    new Vector3(location.x + worldSize, maxHeight, location.z + worldSize));
            }
        }

        public void ResetHeightMaps()
        {
            // clear the heightfields if any are laying around
            for (int i = 0; i < lodLevels; i++)
            {
                heightFields[i] = null;
            }

            ResetBounds();

            // generate the lowest lod level at the current location
            curLodLevels = 1;
            curMetersPerSample = maxMetersPerSample;
            FillLod(curMetersPerSample);
        }
    }
}
