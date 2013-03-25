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
using System.Diagnostics;
using System.IO;

namespace Axiom.SceneManagers.Multiverse
{
    public class TextureMosaic : Mosaic
    {
        protected override MosaicTile NewTile(int tileX, int tileZ, MathLib.Vector3 worldLocMM)
        {
            return new TextureMosaicTile(this, desc.TileSizeSamples, desc.MetersPerSample, tileX, tileZ, worldLocMM);
        }

        public TextureMosaic(string baseName, int preloadRadius)
            : base(baseName, preloadRadius)
        {
        }

        public TextureMosaic(string baseName, int preloadRadius, MosaicDescription desc)
            : base(baseName, preloadRadius, desc)
        {
        }

        public string GetTexture(int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters, out float u1, out float v1, out float u2, out float v2)
        {
            worldXMeters = worldXMeters + xWorldOffsetMeters;
            worldZMeters = worldZMeters + zWorldOffsetMeters;

            int tileX = worldXMeters >> (tileShift + desc.MPSShift);
            int tileZ = worldZMeters >> (tileShift + desc.MPSShift);

            int tileSizeWorld = desc.TileSizeSamples << desc.MPSShift;
            int tileMaskWorld = tileSizeWorld - 1;

            int xoff = worldXMeters & tileMaskWorld;
            int zoff = worldZMeters & tileMaskWorld;

            u1 = (float)xoff / tileSizeWorld;
            v1 = (float)zoff / tileSizeWorld;

            u2 = (float)(xoff + sizeXMeters) / tileSizeWorld;
            v2 = (float)(zoff + sizeZMeters) / tileSizeWorld;

            if (tileX < 0 || tileX >= sizeXTiles || tileZ < 0 || tileZ >= sizeZTiles)
            {
                return "red-zero-alpha.png";
            }

            TextureMosaicTile tile = tiles[tileX, tileZ] as TextureMosaicTile;
            if (tile == null)
            {
                throw new InvalidDataException("Unxpected state!");
            }
            return tile.TextureName;
        }

        protected void SampleToTileCoords(int sampleX, int sampleZ, out int tileX, out int tileZ, out int xOff, out int zOff)
        {
            int tileSize = desc.TileSizeSamples;

            tileX = sampleX >> tileShift;
            tileZ = sampleZ >> tileShift;

            xOff = sampleX & tileMask;
            zOff = sampleZ & tileMask;

            Debug.Assert(xOff < tileSize);
            Debug.Assert(zOff < tileSize);

            return;
        }

        protected internal void WorldToSampleCoords(int worldXMeters, int worldZMeters, out int sampleX, out int sampleZ, out float sampleXfrac, out float sampleZfrac)
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

        /// <summary>
        /// Get the texture map for a point specified by sample coordinates
        /// A 4-byte array is returned as the map.  The map can have 4 possible
        /// mappings associated with it with one byte per map.
        /// </summary>
        /// <param name="sampleX"></param>
        /// <param name="sampleZ"></param>
        /// <returns></returns>
        public byte[] GetSampleTextureMap(int sampleX, int sampleZ)
        {
            int tileX;
            int tileZ;

            int xOff;
            int zOff;

            SampleToTileCoords(sampleX, sampleZ, out tileX, out tileZ, out xOff, out zOff);

            if ((tileX < 0) || (tileX >= sizeXTiles) || (tileZ < 0) || (tileZ >= sizeZTiles))
            {
                return new byte[4];
            }

            TextureMosaicTile tile = tiles[tileX, tileZ] as TextureMosaicTile;
            if (tile == null)
            {
                return new byte[4];
            }

            return tile.GetTextureMap(xOff, zOff);
        }

        /// <summary>
        /// Set the texture map for a point specified by sample coordinates.
        /// The 4-byte array is map.  The map can have 4 possible mappings associated
        /// with it with one byte per map.
        /// </summary>
        /// <param name="sampleX"></param>
        /// <param name="sampleZ"></param>
        /// <param name="textureMap"></param>
        /// <returns></returns>
        public void SetSampleTextureMap(int sampleX, int sampleZ, byte[] textureMap)
        {
            int tileX;
            int tileZ;

            int xOff;
            int zOff;

            SampleToTileCoords(sampleX, sampleZ, out tileX, out tileZ, out xOff, out zOff);

            if ((tileX < 0) || (tileX >= sizeXTiles) || (tileZ < 0) || (tileZ >= sizeZTiles))
            {
                return; // Ignore out-of-bounds modifications
            }

            TextureMosaicTile tile = tiles[tileX, tileZ] as TextureMosaicTile;
            if (tile == null)
            {
                // This shouldn't happen
                throw new Exception("Tile [" + tileX + "," + tileZ + "] not found for coord [" + sampleX + "," + sampleZ + "] while attempting to set textureMap to: " + textureMap);
            }

            tile.SetTextureMap(xOff, zOff, textureMap);
        }

        /// <summary>
        /// Get the texture map for a point specified by world coordinates
        /// A 4-byte array is returned as the map.  The map can have 4 possible
        /// mappings associated with it with one byte per map.
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <returns></returns>
        public byte[] GetWorldTextureMap(int worldXMeters, int worldZMeters)
        {
            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

            if ((sampleXfrac == 0) && (sampleZfrac == 0))
            {
                // pick the closest point to the NW(floor), rather than interpolating
                return GetSampleTextureMap(sampleX, sampleZ);
            }

            // perform a linear interpolation between the 4 surrounding points for each byte within the map
            byte[] nw = GetSampleTextureMap(sampleX, sampleZ);
            byte[] ne = GetSampleTextureMap(sampleX + 1, sampleZ);
            byte[] sw = GetSampleTextureMap(sampleX, sampleZ + 1);
            byte[] se = GetSampleTextureMap(sampleX + 1, sampleZ + 1);

            byte[] ret = new byte[4];

            for (int i = 0; i < ret.Length; i++)
            {
                if (sampleXfrac >= sampleZfrac)
                {
                    ret[i] = (byte)((nw[i] + (ne[i] - nw[i]) * sampleXfrac + (se[i] - ne[i]) * sampleZfrac));
                }
                else
                {
                    ret[i] = (byte)((sw[i] + (se[i] - sw[i]) * sampleXfrac + (nw[i] - sw[i]) * (1 - sampleZfrac)));
                }
            }

            return ret;
        }

        /// <summary>
        /// Set the texture map for a point specified by sample coordinates.
        /// The 4-byte array is map.  The map can have 4 possible mappings associated
        /// with it with one byte per map.
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <param name="textureMap"></param>
        /// <returns></returns>
        public void SetWorldTextureMap(int worldXMeters, int worldZMeters, byte[] textureMap)
        {
            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);
            SetSampleTextureMap(sampleX, sampleZ, textureMap); // Ignore the fractional coordinates
        }

        public void RefreshTextures()
        {
            foreach (TextureMosaicTile tile in tiles)
            {
                if (tile != null)
                {
                    tile.RefreshTexture();
                }
            }
        }
    }
}
