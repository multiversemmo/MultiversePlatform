using System;
using System.Xml;
using System.Diagnostics;
using Axiom.SceneManagers.Multiverse;
using Axiom.MathLib;

namespace Multiverse.Lib.HeightfieldGenerator
{
    public class HeightfieldTerrainGenerator : ITerrainGenerator
    {
        public const string Id = "HeightfieldMosaic";

        public HeightfieldMosaic Mosaic { get; protected set; }
        public event TerrainModificationStateChangedHandler TerrainModificationStateChanged;
        public event TerrainChangedHandler TerrainChanged;
        private bool changeNotificationEnabled = true;

        public bool Modified
        {
            get
            {
                return Mosaic.Modified;
            }
        }

        protected string heightfieldName;
        protected int preloadRadius;
        protected float outsideHeight;

        public HeightfieldTerrainGenerator(string heightfieldName, int preloadRadius, float outsideHeight) 
        {
            this.heightfieldName = heightfieldName;
            this.preloadRadius = preloadRadius;
            this.outsideHeight = outsideHeight;

            Mosaic = new HeightfieldMosaic(heightfieldName, preloadRadius, outsideHeight);
            Mosaic.MosaicModificationStateChanged += Mosaic_OnMosaicModificationStateChanged;
            Mosaic.MosaicChanged += Mosaic_OnMosaicChanged;
        }

        public HeightfieldTerrainGenerator(string heightfieldName, int preloadRadius, float outsideHeight, MosaicDescription desc)
        {
            this.heightfieldName = heightfieldName;
            this.preloadRadius = preloadRadius;
            this.outsideHeight = outsideHeight;

            Mosaic = new HeightfieldMosaic(heightfieldName, preloadRadius, outsideHeight, desc);
            Mosaic.MosaicModificationStateChanged += Mosaic_OnMosaicModificationStateChanged;
            Mosaic.MosaicChanged += Mosaic_OnMosaicChanged;
        }

        public HeightfieldTerrainGenerator(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Type":
                        string type = r.Value;
                        Debug.Assert(type == "HeightfieldMosaic");
                        break;
                    case "MosaicName":
                        heightfieldName = r.Value;
                        break;
                    case "PreloadRadius":
                        preloadRadius = int.Parse(r.Value);
                        break;
                    case "OutsideHeight":
                        outsideHeight = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement();
            if (!r.IsEmptyElement)
            {
                do
                {
                    r.Read();
                } while (r.NodeType != XmlNodeType.EndElement);
            }

            Mosaic = new HeightfieldMosaic(heightfieldName, preloadRadius, outsideHeight);
            Mosaic.MosaicModificationStateChanged += Mosaic_OnMosaicModificationStateChanged;
            Mosaic.MosaicChanged += Mosaic_OnMosaicChanged;
        }

        private void Mosaic_OnMosaicModificationStateChanged(Mosaic mosaic, bool state)
        {
            FireTerrainModificationStateChanged();
        }

        private void Mosaic_OnMosaicChanged(Mosaic mosaic, MosaicTile tile, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
        {
            FireTerrainChanged(worldXMeters, worldZMeters, sizeXMeters, sizeZMeters);
        }

        private void FireTerrainModificationStateChanged()
        {
            if (changeNotificationEnabled && TerrainModificationStateChanged != null)
            {
                TerrainModificationStateChanged(this, Modified);
            }
        }

        private void FireTerrainChanged(int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
        {
            if (changeNotificationEnabled && TerrainChanged != null)
            {
                TerrainChanged(this, worldXMeters, worldZMeters, sizeXMeters, sizeZMeters);
            }
        }

        public void Save(bool force)
        {
            Mosaic.Save(force);
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Terrain");
            w.WriteAttributeString("Type", "HeightfieldMosaic");
            w.WriteAttributeString("MosaicName", heightfieldName);
            w.WriteAttributeString("PreloadRadius", preloadRadius.ToString());
            w.WriteAttributeString("OutsideHeight", outsideHeight.ToString());
            w.WriteEndElement(); // Terrain
        }

        public string HeightfieldName
        {
            get
            {
                return heightfieldName;
            }
        }

        public int PreloadRadius
        {
            get { return preloadRadius; }
        }

        public float OutsideHeight
        {
            get { return outsideHeight; }
        }

        public void GenerateHeightFieldMM(float xWorldLocationMeters, float zWorldLocationMeters, int metersPerSample, float[,] heightFieldMM, out float minHeightMM, out float maxHeightMM)
        {
            minHeightMM = float.MaxValue;
            maxHeightMM = float.MinValue;
            int sampleWidth = heightFieldMM.GetLength(0);
            int sampleHeight = heightFieldMM.GetLength(1);

            int xWorldOffsetMeters = (int) xWorldLocationMeters;
            for (int x = 0; x < sampleWidth; x++ )
            {
                int zWorldOffsetMeters = (int) zWorldLocationMeters;
                for (int z=0; z < sampleHeight; z++)
                {
                    float heightMM = Mosaic.GetWorldHeightMM(xWorldOffsetMeters, zWorldOffsetMeters);
                    heightFieldMM[x,z] = heightMM;
                    minHeightMM = Math.Min(minHeightMM, heightMM);
                    maxHeightMM = Math.Max(minHeightMM, heightMM);

                    zWorldOffsetMeters += metersPerSample;
                }
                xWorldOffsetMeters += metersPerSample;
            }
        }

        #region ITerrainGenerator Members

        public float GenerateHeightPointMM(Vector3 worldLocationMM)
        {
            return GenerateHeightPointMM(worldLocationMM.x/TerrainManager.oneMeter,
                                         worldLocationMM.z/TerrainManager.oneMeter);
        }

        public float[,] GetNormalizedSamples(int xWorldLocationMeters, int zWorldLocationMeters, int sizeXSamples, int sizeZSamples, int metersPerSample)
        {
            float[,] normalizedSamples = new float[sizeXSamples, sizeZSamples];

            int worldZMeters = zWorldLocationMeters;
            for (int sampleZ = 0; sampleZ < sizeZSamples; sampleZ++)
            {
                int worldXMeters = xWorldLocationMeters;
                for (int sampleX = 0; sampleX < sizeXSamples; sampleX++)
                {
                    float heightMM = Mosaic.GetWorldHeightMM(worldXMeters, worldZMeters);
                    float heightNormalized = ConvertHeightMMToNormalized(heightMM);

                    normalizedSamples[sampleX, sampleZ] = heightNormalized;

                    worldXMeters += metersPerSample;
                }

                worldZMeters += metersPerSample;
            }

            return normalizedSamples;
        }

        public void SetNormalizedSamples(int xWorldLocationMeters, int zWorldLocationMeters, int metersPerSample, float[,] normalizedSamples)
        {
            int sizeXSamples = normalizedSamples.GetLength(0);
            int sizeZSamples = normalizedSamples.GetLength(1);
            float[,] diffArray = new float[sizeXSamples, sizeZSamples];

            bool wasModified = Mosaic.Modified;
            try
            {
                changeNotificationEnabled = false;

                // Calculate the heights and set them
                {
                    int worldZMeters = zWorldLocationMeters;
                    for (int sampleZ = 0; sampleZ < sizeZSamples; sampleZ++)
                    {
                        int worldXMeters = xWorldLocationMeters;
                        for (int sampleX = 0; sampleX < sizeXSamples; sampleX++)
                        {
                            float heightNormalized = normalizedSamples[sampleX, sampleZ];

                            // Make sure the normalized height is within the proper bounds
                            if (heightNormalized < 0)
                            {
                                heightNormalized = 0;
                            }
                            else if (heightNormalized > 1)
                            {
                                heightNormalized = 1;
                            }

                            float heightMM = ConvertNormalizedHeightToMM(heightNormalized);
                            diffArray[sampleX, sampleZ] = heightMM - Mosaic.GetWorldHeightMM(worldXMeters, worldZMeters);

                            worldXMeters += metersPerSample;
                        }

                        worldZMeters += metersPerSample;
                    }

                    Mosaic.AdjustWorldSamplesMM(xWorldLocationMeters, zWorldLocationMeters, metersPerSample, diffArray);
                }
            }
            finally
            {
                changeNotificationEnabled = true;

                if (Modified != wasModified)
                {
                    FireTerrainModificationStateChanged();
                }

                FireTerrainChanged(xWorldLocationMeters, zWorldLocationMeters, sizeXSamples * metersPerSample, sizeZSamples * metersPerSample);
            }
        }

        private float ConvertNormalizedHeightToMM(float heightNormalized)
        {
            float floorMeters = Mosaic.MosaicDesc.GlobalMinHeightMeters;
            float heightRangeMeters = Mosaic.MosaicDesc.GlobalMaxHeightMeters - floorMeters;

            float heightMeters = heightNormalized*heightRangeMeters + floorMeters;
            return heightMeters * TerrainManager.oneMeter;
        }

        public float ConvertHeightMMToNormalized(float heightMM)
        {
            float heightMeters = heightMM/TerrainManager.oneMeter;

            float floorMeters = Mosaic.MosaicDesc.GlobalMinHeightMeters;
            float heightRangeMeters = Mosaic.MosaicDesc.GlobalMaxHeightMeters - floorMeters;

            float heightNormalized = (heightMeters - floorMeters) / heightRangeMeters;
            return heightNormalized;
        }

        public float GenerateHeightPointMM(float xWorldLocationMeters, float zWorldLocationMeters)
        {
            return Mosaic.GetWorldHeightMM((int) xWorldLocationMeters, (int) zWorldLocationMeters);
        }

        #endregion

        const float NORMAL_HEIGHT = 2.0f * TerrainManager.oneMeter;

        public void GenerateNormal(float x1HeightMM, float x2HeightMM, float z1HeightMM, float z2HeightMM, out Vector3 normal)
        {
            // compute the normal
            normal.x = x1HeightMM - x2HeightMM;
            normal.y = NORMAL_HEIGHT;
            normal.z = z1HeightMM - z2HeightMM;
            normal.Normalize();
        }

        public void GenerateNormal(Vector3 worldLocationMM, out Vector3 normal)
        {
            int x = (int)(worldLocationMM.x / TerrainManager.oneMeter);
            int z = (int)(worldLocationMM.z / TerrainManager.oneMeter);
            float x1HeightMM = Mosaic.GetWorldHeightMM(x - 1, z);
            float x2HeightMM = Mosaic.GetWorldHeightMM(x + 1, z);
            float z1HeightMM = Mosaic.GetWorldHeightMM(x, z - 1);
            float z2HeightMM = Mosaic.GetWorldHeightMM(x, z + 1);

            GenerateNormal(x1HeightMM, x2HeightMM, z1HeightMM, z2HeightMM, out normal);
        }

        public Vector3 GenerateNormal(Vector3 worldLocationMM)
        {
            Vector3 normal = new Vector3();
            GenerateNormal(worldLocationMM, out normal);
            return normal;
        }

        public Vector3 GenerateNormal(int worldXMeters, int worldZMeters)
        {
            return GenerateNormal(new Vector3(worldXMeters * TerrainManager.oneMeter, 0, worldZMeters * TerrainManager.oneMeter));
        }
    }
}
