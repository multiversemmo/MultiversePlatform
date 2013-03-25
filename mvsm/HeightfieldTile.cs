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

namespace Axiom.SceneManagers.Multiverse
{
    public class HeightfieldTile : DataMosaicTile
    {

        protected float globalMaxHeightMeters;
        protected float globalMinHeightMeters;
        protected float globalHeightRangeMeters;

        public HeightfieldTile(Mosaic parent, int tileSizeSamples, float metersPerSample, int tileLocationX, int tileLocationZ, Vector3 worldLocMM)
            : base(parent, tileSizeSamples, metersPerSample, tileLocationX, tileLocationZ, worldLocMM)
        {
            globalMinHeightMeters = parent.MosaicDesc.GlobalMinHeightMeters;
            globalMaxHeightMeters = parent.MosaicDesc.GlobalMaxHeightMeters;
            globalHeightRangeMeters = globalMaxHeightMeters - globalMinHeightMeters;
        }


        /// <summary>
        /// Get the height scaled to the world (millimeters)
        /// </summary>
        /// <param name="sampleX">x coord in samples within this tile</param>
        /// <param name="sampleZ">z coord in samples within this tile</param>
        /// <returns></returns>
        public float GetHeightMM(int sampleX, int sampleZ)
        {
            float normalizedHeight = GetDataNormalized(sampleX, sampleZ);

            return (normalizedHeight * globalHeightRangeMeters + globalMinHeightMeters) * TerrainManager.oneMeter;
        }

        /// <summary>
        /// Set the height scaled to the world (millimeters)
        /// </summary>
        /// <param name="sampleX">x coord in samples within this tile</param>
        /// <param name="sampleZ">z coord in samples within this tile</param>
        /// <param name="heightMM"></param>
        /// <returns></returns>
        public void SetHeightMM(int sampleX, int sampleZ, float heightMM)
        {
            float normalizedHeight = ((heightMM / TerrainManager.oneMeter) - globalMinHeightMeters) / globalHeightRangeMeters;
            if (normalizedHeight < 0)
            {
                normalizedHeight = 0f;
            } 
            else if (normalizedHeight > 1)
            {
                normalizedHeight = 1f;
            }

            SetDataNormalized(sampleX, sampleZ, normalizedHeight);
        }
    }
}
