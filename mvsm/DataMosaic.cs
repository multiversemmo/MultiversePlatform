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

using System.Diagnostics;

namespace Axiom.SceneManagers.Multiverse
{
    public class DataMosaic : Mosaic
    {
        protected int defaultValue;

        protected override MosaicTile NewTile(int tileX, int tileZ, MathLib.Vector3 worldLocMM)
        {
            return new DataMosaicTile(this, desc.TileSizeSamples, desc.MetersPerSample, tileX, tileZ, worldLocMM);
        }

        public DataMosaic(string baseName, int preloadRadius, int defaultValue, MosaicDescription desc)
            : base(baseName, preloadRadius, desc)
        {
            this.defaultValue = defaultValue;
        }

        public DataMosaic(string baseName, int preloadRadius, int defaultValue)
            : base(baseName, preloadRadius)
        {
            this.defaultValue = defaultValue;
        }

        protected void SampleToTileCoords(int sampleX, int sampleZ, out int tileX, out int tileZ, out int xOff, out int zOff)
        {
            int tileSize = desc.TileSizeSamples;

            tileX = sampleX >> tileShift;
            tileZ = sampleZ >> tileShift;

            xOff = sampleX & tileMask;
            zOff = sampleZ & tileMask;

//            Debug.Assert(xOff < tileSize);
//            Debug.Assert(zOff < tileSize);

            return;
        }

        protected void WorldToSampleCoords(int worldXMeters, int worldZMeters, out int sampleX, out int sampleZ, out float sampleXfrac, out float sampleZfrac)
        {
            worldXMeters = worldXMeters + xWorldOffsetMeters;
            worldZMeters = worldZMeters + zWorldOffsetMeters;

            sampleX = worldXMeters >> desc.MPSShift;
            sampleZ = worldZMeters >> desc.MPSShift;

            int sampleXRem = worldXMeters & desc.MPSMask;
            int sampleZRem = worldZMeters & desc.MPSMask;

            if ((sampleXRem != 0) || (sampleZRem != 0))
            {
                sampleXfrac = (float)sampleXRem / desc.MetersPerSample;
                sampleZfrac = (float)sampleZRem / desc.MetersPerSample;
            }
            else
            {
                sampleXfrac = 0;
                sampleZfrac = 0;
            }
        }
    }
}
