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

namespace Axiom.SceneManagers.Multiverse
{
    public class HeightfieldMosaic : DataMosaic
    {

        // the height to use for areas outside the mosaic, or for areas that don't have a tile
        protected float defaultHeightMM;

        protected override MosaicTile NewTile(int tileX, int tileZ, MathLib.Vector3 worldLocMM)
        {
            return new HeightfieldTile(this, desc.TileSizeSamples, desc.MetersPerSample, tileX, tileZ, worldLocMM);
        }

        public HeightfieldMosaic(string baseName, int preloadRadius, float defaultHeightMM, MosaicDescription desc) :
            base(baseName, preloadRadius, 0, desc)
        {
            Init(defaultHeightMM);
        }

        public HeightfieldMosaic(string baseName, int preloadRadius, float defaultHeightMM)
            : base(baseName, preloadRadius, 0)
        {
            Init(defaultHeightMM);
        }

        private /*sealed*/ void Init(float defHeightMM)
        {
            float globalMaxHeight = desc.GlobalMaxHeightMeters;
            float globalMinHeight = desc.GlobalMinHeightMeters;
            float globalHeightRange = globalMaxHeight - globalMinHeight;

            // back convert the default height to the value range used by the data map, and
            // set the defaultValue.
            defaultValue = (int)(((defHeightMM / TerrainManager.oneMeter) - globalMinHeight) / globalHeightRange * 65536f);
            defaultHeightMM = defHeightMM;
        }

        /// <summary>
        /// Get the height at a point specified by sample coordinates
        /// </summary>
        /// <param name="sampleX"></param>
        /// <param name="sampleZ"></param>
        /// <returns></returns>
        public float GetSampleHeightMM(int sampleX, int sampleZ)
        {
            int tileX;
            int tileZ;

            int xOff;
            int zOff;

            SampleToTileCoords(sampleX, sampleZ, out tileX, out tileZ, out xOff, out zOff);

            if ((tileX < 0) || (tileX >= sizeXTiles) || (tileZ < 0) || (tileZ >= sizeZTiles))
            {
                return defaultHeightMM;
            }

            HeightfieldTile tile = tiles[tileX, tileZ] as HeightfieldTile;
            if (tile == null)
            {
                return defaultHeightMM;
            }

            return tile.GetHeightMM(xOff, zOff);
        }

        /// <summary>
        /// Set the height at a point specified by sample coordinates
        /// </summary>
        /// <param name="sampleX"></param>
        /// <param name="sampleZ"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public void SetSampleHeightMM(int sampleX, int sampleZ, float height)
        {
            int tileX;
            int tileZ;

            int xOff;
            int zOff;

            SampleToTileCoords(sampleX, sampleZ, out tileX, out tileZ, out xOff, out zOff);

            if ((tileX < 0) || (tileX >= sizeXTiles) || (tileZ < 0) || (tileZ >= sizeZTiles))
            {
                //throw new IndexOutOfRangeException();
                // It's possible to go off the end of the terrain, so just ignore it if it happens.
                return;
            }

            HeightfieldTile tile = tiles[tileX, tileZ] as HeightfieldTile;
            if (tile == null)
            {
                // This shouldn't happen
                throw new Exception("Tile [" + tileX + "," + tileZ + "] not found for coord [" + sampleX + "," + sampleZ + "] while attempting to set height to: " + height);                
            }

            tile.SetHeightMM(xOff, zOff, height);
        }

        public float InterpolateTerrain(float nw, float ne, float sw, float se, float sampleXfrac, float sampleZfrac)
        {
            // perform bilinear interpolation on points
            // another great algorithm stolen from wikipedia
            return
                (nw * (1 - sampleXfrac) * (1 - sampleZfrac)) +
                (ne * (sampleXfrac) * (1 - sampleZfrac)) +
                (sw * (1 - sampleXfrac) * (sampleZfrac)) +
                (se * (sampleXfrac) * (sampleZfrac));
        }

        /// <summary>
        /// Get the height of a point specified by world coordinates, which are specified in meters
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <returns></returns>
        public float GetWorldHeightMM(int worldXMeters, int worldZMeters)
        {
            float ret;

            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

            if ((sampleXfrac == 0) && (sampleZfrac == 0))
            {
                // pick the closest point to the NW(floor), rather than interpolating
                ret = GetSampleHeightMM(sampleX, sampleZ);
            }
            else
            {
                // perform a linear interpolation between the 4 surrounding points
                float nw = GetSampleHeightMM(sampleX, sampleZ);
                float ne = GetSampleHeightMM(sampleX + 1, sampleZ);
                float sw = GetSampleHeightMM(sampleX, sampleZ + 1);
                float se = GetSampleHeightMM(sampleX + 1, sampleZ + 1);

                ret = InterpolateTerrain(nw, ne, sw, se, sampleXfrac, sampleZfrac);
            }

            return ret;
        }

        public void AdjustWorldHeightMM(int worldXMeters, int worldZMeters, float heightDifference)
        {
            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);
            if ((sampleXfrac == 0) && (sampleZfrac == 0))
            {
                SetSampleHeightMM(sampleX, sampleZ, GetSampleHeightMM(sampleX, sampleZ) + heightDifference);
            }
            else
            {
                float nw = GetSampleHeightMM(sampleX, sampleZ);
                float ne = GetSampleHeightMM(sampleX + 1, sampleZ);
                float sw = GetSampleHeightMM(sampleX, sampleZ + 1);
                float se = GetSampleHeightMM(sampleX + 1, sampleZ + 1);

                SetSampleHeightMM(sampleX, sampleZ, nw + (heightDifference * (1f - sampleXfrac) * (1f - sampleZfrac)));
                SetSampleHeightMM(sampleX + 1, sampleZ, ne + (heightDifference * (sampleXfrac) * (1f - sampleZfrac)));
                SetSampleHeightMM(sampleX, sampleZ + 1, sw + (heightDifference * (1f - sampleXfrac) * (sampleZfrac)));
                SetSampleHeightMM(sampleX + 1, sampleZ + 1, se + (heightDifference * (sampleXfrac) * (sampleZfrac)));
            }
        }

        public void AdjustWorldSamplesMM(int worldXMeters, int worldZMeters, int metersPerSample, float[,] heightsMM)
        {
            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);
            if ((sampleXfrac == 0) && (sampleZfrac == 0) && (metersPerSample == MosaicDesc.MetersPerSample))
            {
                for (int z = 0; z < heightsMM.GetLength(1); z++)
                {
                    for (int x = 0; x < heightsMM.GetLength(0); x++)
                    {
                        float existing = GetSampleHeightMM(sampleX + x, sampleZ + z);
                        SetSampleHeightMM(sampleX + x, sampleZ + z, existing + heightsMM[x, z]);
                    }
                }
            }
            else
            {
                int upperLeftSampleX = sampleX;
                int upperLeftSampleZ = sampleZ;

                WorldToSampleCoords(
                    worldXMeters + heightsMM.GetLength(0) * metersPerSample,
                    worldZMeters + heightsMM.GetLength(1) * metersPerSample,
                    out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

                int lowerRightSampleX = (sampleXfrac == 0) ? sampleX : sampleX + 1;
                int lowerRightSampleZ = (sampleZfrac == 0) ? sampleZ : sampleZ + 1;

                float[,] diffArray = new float[lowerRightSampleX - upperLeftSampleX + 1, lowerRightSampleZ - upperLeftSampleZ + 1];
                float[,] appliedWeight = new float[lowerRightSampleX - upperLeftSampleX + 1, lowerRightSampleZ - upperLeftSampleZ + 1];
                for (int z = 0; z < diffArray.GetLength(1); z++)
                {
                    for (int x = 0; x < diffArray.GetLength(0); x++)
                    {
                        diffArray[x, z] = 0;
                    }
                }

                int currentWorldXMeters;
                int currentWorldZMeters = worldZMeters;
                for (int z = 0; z < heightsMM.GetLength(1); z++)
                {
                    currentWorldXMeters = worldXMeters;
                    for (int x = 0; x < heightsMM.GetLength(0); x++)
                    {
                        WorldToSampleCoords(
                            currentWorldXMeters, currentWorldZMeters, 
                            out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

                        int xpos = sampleX - upperLeftSampleX;
                        int zpos = sampleZ - upperLeftSampleZ;
                        diffArray[xpos, zpos] += (heightsMM[x, z] * (1f - sampleXfrac) * (1f - sampleZfrac));
                        diffArray[xpos + 1, zpos] += (heightsMM[x, z] * (sampleXfrac) * (1f - sampleZfrac));
                        diffArray[xpos, zpos + 1] += (heightsMM[x, z] * (1f - sampleXfrac) * (sampleZfrac));
                        diffArray[xpos + 1, zpos + 1] += (heightsMM[x, z] * (sampleXfrac) * (sampleZfrac));

                        appliedWeight[xpos, zpos] += (1f - sampleXfrac) * (1f - sampleZfrac);
                        appliedWeight[xpos + 1, zpos] += sampleXfrac * (1f - sampleZfrac);
                        appliedWeight[xpos, zpos + 1] += (1f - sampleXfrac) * sampleZfrac;
                        appliedWeight[xpos + 1, zpos + 1] += sampleXfrac * sampleZfrac;

                        currentWorldXMeters += metersPerSample;
                    }
                    currentWorldZMeters += metersPerSample;
                }

                float[,] averagedDiffArray = new float[lowerRightSampleX - upperLeftSampleX + 1,lowerRightSampleZ - upperLeftSampleZ + 1];
                for (int z = 0; z < diffArray.GetLength(1); z++)
                {
                    for (int x = 0; x < diffArray.GetLength(0); x++)
                    {
                        if (appliedWeight[x, z] == 0)
                        {
                            averagedDiffArray[x, z] = 0;
                        }
                        else
                        {
                            averagedDiffArray[x, z] = diffArray[x, z] / appliedWeight[x, z];
                        }
                    }
                }

                for (int z = 0; z < averagedDiffArray.GetLength(1); z++)
                {
                    for (int x = 0; x < averagedDiffArray.GetLength(0); x++)
                    {
                        float existing = GetSampleHeightMM(upperLeftSampleX + x, upperLeftSampleZ + z);
                        SetSampleHeightMM(upperLeftSampleX + x, upperLeftSampleZ + z, existing + averagedDiffArray[x, z]);
                    }
                }
            }
        }

        /// <summary>
        /// Set the height of a point specified by world coordinates, which are specified in meters
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <param name="heightMM"></param>
        /// <returns></returns>
        public void SetWorldHeightMM(int worldXMeters, int worldZMeters, float heightMM)
        {
            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            float existing;

            WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

            if ((sampleXfrac == 0) && (sampleZfrac == 0))
            {
                // pick the closest point to the NW(floor), rather than interpolating
                SetSampleHeightMM(sampleX, sampleZ, heightMM);
            }
            else
            {
                // perform linear interpolation to get existing height
                // determine difference between existing height and set height
                // add difference to all four corners according to the fractional split
                float nw = GetSampleHeightMM(sampleX, sampleZ);
                float ne = GetSampleHeightMM(sampleX + 1, sampleZ);
                float sw = GetSampleHeightMM(sampleX, sampleZ + 1);
                float se = GetSampleHeightMM(sampleX + 1, sampleZ + 1);

                existing = InterpolateTerrain(nw, ne, sw, se, sampleXfrac, sampleZfrac);

                float diff = (heightMM - existing);
                SetSampleHeightMM(sampleX, sampleZ, nw + (diff * (1f - sampleXfrac) * (1f - sampleZfrac)));
                SetSampleHeightMM(sampleX + 1, sampleZ, ne + (diff * (sampleXfrac) * (1f - sampleZfrac)));
                SetSampleHeightMM(sampleX, sampleZ + 1, sw + (diff * (1f - sampleXfrac) * (sampleZfrac)));
                SetSampleHeightMM(sampleX + 1, sampleZ + 1, se + (diff * (sampleXfrac) * (sampleZfrac)));
            }
        }
    }
}
