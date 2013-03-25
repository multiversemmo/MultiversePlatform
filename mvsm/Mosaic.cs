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
using System.IO;
using System.Diagnostics;
using Axiom.MathLib;
using Axiom.Core;

namespace Axiom.SceneManagers.Multiverse
{
    public delegate void MosaicModificationStateChangedHandler(Mosaic mosaic, bool state);
    public delegate void MosaicChangedHandler(Mosaic mosaic, MosaicTile tile, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters);

    public abstract class Mosaic
    {
        public bool Modified
        {
            get
            {
                return m_modified;
            }

            protected set
            {
                if (m_modified == value)
                {
                     return;
                }

                m_modified = value;
                if (MosaicModificationStateChanged != null)
                {
                    MosaicModificationStateChanged(this, m_modified);
                }
            }
        }

        public event MosaicModificationStateChangedHandler MosaicModificationStateChanged;
        public event MosaicChangedHandler MosaicChanged;

        public MosaicDescription MosaicDesc
        {
            get
            {
                return desc;
            }
        }

        protected MosaicDescription desc;

        // the radius (in tiles) from the camera that should be preloaded
        protected int preloadRadius;

        public object BaseName { get; private set; }

        // the location of the camera
        protected Vector3 cameraLocation;
        protected int cameraTileX = int.MaxValue;
        protected int cameraTileZ = int.MaxValue;

        protected MosaicTile[,] tiles;
        protected int sizeXTiles;
        protected int sizeZTiles;

        protected int tileShift;
        protected int tileMask;

        protected int xWorldOffsetMeters;
        protected int zWorldOffsetMeters;

        protected abstract MosaicTile NewTile(int tileX, int tileZ, Vector3 worldLocMM);

        //todo: change modifiedTiles to a HashSet when we can use .Net 3.0
        private readonly List<MosaicTile> modifiedTiles = new List<MosaicTile>();
        private bool m_modified;

        protected Mosaic(string baseName, int preloadRadius, MosaicDescription desc)
        {
            this.desc = desc;
            //todo: save description into MMF file using baseName

            BaseName = baseName;
            this.preloadRadius = preloadRadius;

            Init();
        }

        protected Mosaic(string baseName, int preloadRadius)
        {
            Stream s = ResourceManager.FindCommonResourceData(string.Format("{0}.mmf", baseName));
            desc = new MosaicDescription(s);

            BaseName = baseName;
            this.preloadRadius = preloadRadius;

            Init();
        }

        private /*sealed*/ void Init() 
        {
            sizeXTiles = desc.SizeXTiles;
            sizeZTiles = desc.SizeZTiles;

            // Center the map on the world origin
            int pageSize = TerrainManager.Instance.PageSize;
            int centerXMeters = (desc.SizeXPixels * desc.MetersPerSample) / 2;
            int centerZMeters = (desc.SizeZPixels * desc.MetersPerSample) / 2;
            centerXMeters = (centerXMeters / pageSize) * pageSize;
            centerZMeters = (centerZMeters / pageSize) * pageSize;

            xWorldOffsetMeters = centerXMeters;
            zWorldOffsetMeters = centerZMeters;

            tiles = new MosaicTile[sizeXTiles, sizeZTiles];

            for (int tileX = 0; tileX < sizeXTiles; tileX++)
            {
                for (int tileZ = 0; tileZ < sizeZTiles; tileZ++)
                {
                    float worldTileSizeMM = desc.TileSizeSamples * desc.MetersPerSample * TerrainManager.oneMeter;
                    Vector3 worldLocMM = new Vector3(tileX * worldTileSizeMM - xWorldOffsetMeters * TerrainManager.oneMeter, 0, tileZ * worldTileSizeMM - zWorldOffsetMeters * TerrainManager.oneMeter);
                    MosaicTile tile = NewTile(tileX, tileZ, worldLocMM);
                    tile.TileModificationStateChanged += UpdateModifiedState;
                    tile.TileChanged += Tile_OnTileChanged;

                    tiles[tileX, tileZ] = tile;
                }
            }

            TerrainManager.Instance.SettingCameraLocation += CameraLocationPreChange;

            int tileSize = desc.TileSizeSamples;
            tileMask = tileSize - 1;

            tileShift = 0;
            while (tileSize > 1)
            {
                tileSize = tileSize >> 1;
                tileShift++;
            }

            // ensure its a power of 2
            Debug.Assert((1 << tileShift) == desc.TileSizeSamples);
        }

        private void Tile_OnTileChanged(MosaicTile tile, int tileXSample, int tileZSample, int sizeXSamples, int sizeZSamples)
        {
            if (MosaicChanged != null)
            {
                // Convert samples coordinates & size to world coordinates & size in meters
                int tileSizeMeters = desc.MetersPerSample * desc.TileSizeSamples;

                // Calculate the upper left corner of the tile in world coordinates
                int worldXMeters = tileXSample * desc.MetersPerSample - xWorldOffsetMeters;
                int worldZMeters = tileZSample * desc.MetersPerSample - zWorldOffsetMeters;
                
                // Adjust for the tile-specific coordinate
                worldXMeters += tile.tileX * tileSizeMeters;
                worldZMeters += tile.tileZ * tileSizeMeters;

                // Calculate the size of the modified area in meters
                int sizeXMeters = sizeXSamples * desc.MetersPerSample;
                int sizeZMeters = sizeZSamples * desc.MetersPerSample;

                // Generate the notification
                MosaicChanged(this, tile, worldXMeters, worldZMeters, sizeXMeters, sizeZMeters);
            }
        }

        private void UpdateModifiedState(MosaicTile tile, bool state)
        {
            bool contained = modifiedTiles.Contains(tile);

            //todo: change modifiedTiles to a HashSet when we can use .Net 3.0
            //todo: this would simplify the logic to simply adding or removing based on state
            if (contained && !state)
            {
                modifiedTiles.Remove(tile);
            }
            else if (!contained && state)
            {
                modifiedTiles.Add(tile);
            }

            Modified = modifiedTiles.Count > 0 || desc.Modified;
        }

        private void Preload()
        {
            cameraTileX = (int)Math.Floor((cameraLocation.x + xWorldOffsetMeters * TerrainManager.oneMeter) / (desc.TileSizeSamples * desc.MetersPerSample * TerrainManager.oneMeter));
            cameraTileZ = (int)Math.Floor((cameraLocation.z + zWorldOffsetMeters * TerrainManager.oneMeter) / (desc.TileSizeSamples * desc.MetersPerSample * TerrainManager.oneMeter));

            // compute preload area using current camera tile and the preload radius
            int startX = cameraTileX - preloadRadius;
            int endX = cameraTileX + preloadRadius;
            int startZ = cameraTileZ - preloadRadius;
            int endZ = cameraTileZ + preloadRadius;

            // clip to tile area
            if (startX < 0)
            {
                startX = 0;
            }
            if (endX >= sizeXTiles)
            {
                endX = sizeXTiles - 1;
            }
            if (startZ < 0)
            {
                startZ = 0;
            }
            if (endZ >= sizeZTiles)
            {
                endZ = sizeZTiles - 1;
            }

            // load all tiles in the preload area
            for (int x = startX; x <= endX; x++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    tiles[x, z].Load();
                }
            }
        }

        private void CameraLocationPreChange(object sender, CameraLocationEventArgs args)
        {
            cameraLocation = args.newLocation;

            int newCameraTileX = (int)Math.Floor(cameraLocation.x / (desc.TileSizeSamples * TerrainManager.oneMeter));
            int newCameraTileZ = (int)Math.Floor(cameraLocation.z / (desc.TileSizeSamples * TerrainManager.oneMeter));

            if ((cameraTileX != newCameraTileX) || (cameraTileZ != newCameraTileZ))
            {
                cameraTileX = newCameraTileX;
                cameraTileZ = newCameraTileZ;

                Preload();
            }
        }

        public virtual void Save(bool force)
        {
            desc.Save(force);

            if (force)
            {
                for (int z=0; z < sizeZTiles; z++)
                {
                    for (int x=0; x < sizeXTiles; x++)
                    {
                        tiles[x,z].Save(force);
                    }
                }
            }
            else
            {
                // We need to create a copy because Saving tiles will updated
                // the modifiedTiles list.
                List<MosaicTile> listCopy = new List<MosaicTile>(modifiedTiles);
                foreach (MosaicTile tile in listCopy)
                {
                    tile.Save(force);
                }
            }
        }
    }
}
