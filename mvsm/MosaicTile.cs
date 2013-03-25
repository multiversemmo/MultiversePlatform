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
    public delegate void TileModificationStateChangedHandler(MosaicTile tile, bool modificationState);
    public delegate void TileChangedHandler(MosaicTile tile, int tileXSample, int tileZSample, int sizeXSamples, int sizeZSamples);

    public abstract class MosaicTile
    {
        public bool Modified
        {
            get { return m_modified; }
            protected set
            {
                if (m_modified == value)
                {
                    return;
                }

                m_modified = value;
                if (TileModificationStateChanged != null)
                {
                    TileModificationStateChanged(this, m_modified);
                }
            }
        }

        public event TileModificationStateChangedHandler TileModificationStateChanged;
        public event TileChangedHandler TileChanged;

        protected int tileSizeSamples;
        protected float metersPerSample;
        public readonly int tileX;
        public readonly int tileZ;
        protected Vector3 worldLoc;
        protected Mosaic parent;

        protected bool loaded;
        protected bool available;

        private bool m_modified;

        protected MosaicTile(Mosaic parent, int tileSizeSamples, float metersPerSample, int tileX, int tileZ, Vector3 worldLoc)
        {
            this.parent = parent;
            this.tileSizeSamples = tileSizeSamples;
            this.metersPerSample = metersPerSample;
            this.tileX = tileX;
            this.tileZ = tileZ;
            this.worldLoc = worldLoc;

            MosaicDescription desc = parent.MosaicDesc;
            available = desc.TileAvailable(tileX, tileZ);
        }

        public abstract void Load();
        public abstract void Save(bool force);

        protected void FireTileChanged(int tileXSample, int tileZSample, int sizeXSamples, int sizeZSamples)
        {
            if (TileChanged != null)
            {
                TileChanged(this, tileXSample, tileZSample, sizeXSamples, sizeZSamples);
            }
        }
    }
}
