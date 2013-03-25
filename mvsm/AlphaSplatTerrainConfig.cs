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
using System.Text;
using System.Xml;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public delegate void TerrainSplatModificationStateChangedHandler(ITerrainMaterialConfig config, bool state);
    public delegate void TerrainSplatChangedHandler(ITerrainMaterialConfig config, MosaicTile tile, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters);

    public class AlphaSplatTerrainConfig : ITerrainMaterialConfig
    {
        public static readonly int MAX_LAYER_TEXTURES = 8;
        public static readonly int NUM_ALPHA_MAPS = MAX_LAYER_TEXTURES / 4;  // 4 channels per map

        private const string PRE_ALPHA_NAME = "Alpha";
        private const string POST_ALPHA_NAME = "MosaicName";
        private const string PRE_LAYER_NAME = "Layer";
        private const string POST_LAYER_NAME = "TextureName";

        public event TerrainSplatModificationStateChangedHandler TerrainSplatModificationStateChanged;
        public event TerrainSplatChangedHandler TerrainSplatChanged;

        private bool m_wasModifiedBeforeSuspend;
        private bool m_wasChangedDuringSuspend;

        protected bool useParams = true;

        protected float textureTileSize = 5;

        private readonly string[] layerTextureNames = new string[MAX_LAYER_TEXTURES];
        private readonly string[] alphaMapMosaicNames = new string[NUM_ALPHA_MAPS];
        private readonly TextureMosaic[] alphaMapMosaics = new TextureMosaic[NUM_ALPHA_MAPS];
        private string detailTextureName = "terrain_detail.dds";

        public AutoSplatRules AutoSplatRules { get; set;}

        private bool m_changeNotificationEnabled = true;
        private bool m_locallyModified;

        public AlphaSplatTerrainConfig()
        {
            InitTextureNames();
        }

        public AlphaSplatTerrainConfig(string title, AutoSplatRules autoSplatRules, MosaicDescription srcdesc)
        {
            InitTextureNames();
            // Override texture names from rules, if applicable
            for (int i = 0; i < autoSplatRules.layerTextureNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(autoSplatRules.layerTextureNames[i]))
                {
                    layerTextureNames[i] = autoSplatRules.layerTextureNames[i];
                }
            }

            // Create alpha map texture mosaics
            for (int i = 0; i < NUM_ALPHA_MAPS; i++)
            {
                string alphaMapMosaicName = title + "AlphaMap" + i;
                MosaicDescription desc = new MosaicDescription(alphaMapMosaicName, srcdesc);

                alphaMapMosaicNames[i] = alphaMapMosaicName;
                alphaMapMosaics[i] = new TextureMosaic(alphaMapMosaicName, 0, desc);

                alphaMapMosaics[i].MosaicModificationStateChanged += Mosaic_OnMosaicModificationStateChanged;
                alphaMapMosaics[i].MosaicChanged += Mosaic_OnMosaicChanged;
            }

            AutoSplatRules = autoSplatRules;            
            InitializeAutoSplatRules(srcdesc);
        }

        // DEPRECATED: WorldManager uses this overload, but it is not sufficient for 
        // constructing reasonalbe AutoSplatRules. To construct the rules we need
        // the global min/max height values, and these values do not necessarily 
        // appear in the mosaic descriptions for the alpha mosaics. The mosaic 
        // description we really want is the one for the height-f
        public AlphaSplatTerrainConfig(XmlReader r)
        {
            InitTextureNames();
            FromXml(r);
            InitializeAutoSplatRules(alphaMapMosaics == null ? null : alphaMapMosaics[0].MosaicDesc);
        }

        public AlphaSplatTerrainConfig( XmlReader r, MosaicDescription heightfieldMosaic )
        {
            InitTextureNames();
            FromXml( r );
            InitializeAutoSplatRules( heightfieldMosaic );
        }

        // XXXMLM - probably belongs in a AlphaSplatTerrainGenerator class, consider refactor
        public void Save(bool force)
        {
            foreach (TextureMosaic tm in alphaMapMosaics)
            {
                tm.Save(force);
            }
        }

        private void InitializeAutoSplatRules(MosaicDescription desc)
        {
            if (AutoSplatRules != null)
            {
                return;
            }

            long minHeightMM;
            long maxHeightMM;

            if (desc == null)
            {
                // Set arbitrary min/max heights
                minHeightMM = -10000 * (long) TerrainManager.oneMeter;
                maxHeightMM = 10000*(long) TerrainManager.oneMeter;
            }
            else
            {
                minHeightMM = (long) (desc.GlobalMinHeightMeters * TerrainManager.oneMeter);
                maxHeightMM = (long) (desc.GlobalMaxHeightMeters * TerrainManager.oneMeter);
            }
            AutoSplatRules = new AutoSplatRules(minHeightMM, maxHeightMM, new AutoSplatConfig());        
        }

        private void InitTextureNames()
        {
            for (int i = 0; i < layerTextureNames.Length; i++)
            {
                layerTextureNames[i] = "";
            }
        }

        public string GetLayerTextureAttributeName(int index)
        {
            return PRE_LAYER_NAME + (index + 1) + POST_LAYER_NAME;
        }

        public string GetAlphaMapAttributeName(int index)
        {
            return PRE_ALPHA_NAME + index + POST_ALPHA_NAME;
        }

        public void FromXml(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                if (r.Name.StartsWith(PRE_ALPHA_NAME) && r.Name.EndsWith(POST_ALPHA_NAME))
                {
                    string indexStr = r.Name.Substring(PRE_ALPHA_NAME.Length,
                                                       r.Name.Length - PRE_ALPHA_NAME.Length - POST_ALPHA_NAME.Length);
                    int index = int.Parse(indexStr);
                    SetAlphaMapName(index, r.Value);
                }
                else if (r.Name.StartsWith(PRE_LAYER_NAME) && r.Name.EndsWith(POST_LAYER_NAME))
                {
                    string indexStr = r.Name.Substring(PRE_LAYER_NAME.Length,
                                                       r.Name.Length - PRE_LAYER_NAME.Length - POST_LAYER_NAME.Length);
                    int index = int.Parse(indexStr) - 1;
                    SetLayerTextureName(index, r.Value);
                }
                else
                {
                    switch (r.Name)
                    {
                        case "Type":
                            break;
                        case "UseParams":
                            useParams = (r.Value == "True");
                            break;
                        case "TextureTileSize":
                            textureTileSize = float.Parse(r.Value);
                            break;
                        case "DetailTextureName":
                            detailTextureName = r.Value;
                            break;
                    }
                }
            }
            r.MoveToElement();
            Modified = false;
        }

        private void Mosaic_OnMosaicModificationStateChanged(Mosaic mosaic, bool state)
        {
            FireTerrainSplatModificationStateChanged();
        }

        private void Mosaic_OnMosaicChanged(Mosaic mosaic, MosaicTile tile, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
        {
            FireTerrainSplatChanged(tile, worldXMeters, worldZMeters, sizeXMeters, sizeZMeters);
        }

        private void FireTerrainSplatModificationStateChanged()
        {
            if (m_changeNotificationEnabled && TerrainSplatModificationStateChanged != null)
            {
                TerrainSplatModificationStateChanged(this, Modified);
            }
        }

        private void FireTerrainSplatChanged()
        {
            FireTerrainSplatChanged(null, 0, 0, 0, 0);
        }

        private void FireTerrainSplatChanged(int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
        {
            FireTerrainSplatChanged(null, worldXMeters, worldZMeters, sizeXMeters, sizeZMeters);
        }

        private void FireTerrainSplatChanged(MosaicTile tile, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
        {
            if (m_changeNotificationEnabled && TerrainSplatChanged != null)
            {
                TextureMosaicTile textile = tile as TextureMosaicTile;
                if (textile == null)
                {
                    // Refresh all textures within the mosaic
                    // that have dirty texture images
                    RefreshTextures();
                }
                else
                {
                    // Only refresh the given tile
                    textile.RefreshTexture();
                }

                TerrainSplatChanged(this, tile, worldXMeters, worldZMeters, sizeXMeters, sizeZMeters);
            }
        }

        public bool Modified
        {
            get
            {
                if (m_locallyModified)
                {
                    return true;
                }

                foreach (TextureMosaic alphaMosaic in alphaMapMosaics)
                {
                    if (alphaMosaic != null && alphaMosaic.Modified)
                    {
                        return true;
                    }
                }

                return false;
            }

            private set
            {
                if (m_locallyModified == value)
                {
                    FireTerrainSplatChanged();
                    return;
                }

                bool oldAggregateState = Modified;
                m_locallyModified = value;
                if (oldAggregateState != m_locallyModified)
                {
                    FireTerrainSplatModificationStateChanged();
                }

                FireTerrainSplatChanged();
            }
        }

        public bool UseParams
        {
            get
            {
                return useParams;
            }
            set
            {
                if (useParams != value)
                {
                    useParams = value;
                    Modified = true;
                }
            }
        }

        public float TextureTileSize
        {
            get
            {
                return textureTileSize;
            }
            set
            {
                if (textureTileSize != value)
                {
                    textureTileSize = value;
                    Modified = true;
                }
            }
        }

        public string GetAlphaMapName(int index)
        {
            return alphaMapMosaicNames[index];
        }

        public void SetAlphaMapName(int index, string alphaMapName)
        {
            if (string.IsNullOrEmpty(alphaMapName))
            {
                alphaMapName = null;
            }

            if (alphaMapMosaicNames[index] == alphaMapName)
            {
                return;
            }

            if (alphaMapMosaics[index] != null)
            {
                alphaMapMosaics[index].MosaicModificationStateChanged -= Mosaic_OnMosaicModificationStateChanged;
                alphaMapMosaics[index].MosaicChanged -= Mosaic_OnMosaicChanged;
            }

            alphaMapMosaicNames[index] = alphaMapName;
            alphaMapMosaics[index] = alphaMapName == null ? null : new TextureMosaic(alphaMapName, 0);

            if (alphaMapMosaics[index] != null)
            {
                alphaMapMosaics[index].MosaicModificationStateChanged += Mosaic_OnMosaicModificationStateChanged;
                alphaMapMosaics[index].MosaicChanged += Mosaic_OnMosaicChanged;
            }

            Modified = true;
        }

        public TextureMosaic GetAlphaMap(int index)
        {
            return alphaMapMosaics[index];
        }

        public string GetLayerTextureName(int index)
        {
            return layerTextureNames[index];
        }

        public void SetLayerTextureName(int index, string layerTextureName)
        {
            if (string.IsNullOrEmpty(layerTextureName))
            {
                layerTextureName = "";
            }

            if (layerTextureNames[index] == layerTextureName)
            {
                return;
            }

            layerTextureNames[index] = layerTextureName;

            Modified = true;
        }

        public string DetailTextureName
        {
            get
            {
                return detailTextureName;
            }
            set
            {
                detailTextureName = value;
                Modified = true;
            }
        }

        #region ITerrainMaterialConfig Members

        public ITerrainMaterial NewTerrainMaterial(int pageX, int pageZ)
        {
            return new AlphaSplatTerrainMaterial(this, pageX, pageZ);
        }

        public void UpdateMaterial(Material material)
        {
            if (useParams)
            {
                // Note: If we change the number of alpha map mosaics, we may need to update the GPU
                // shader to support
                Pass pass = material.GetTechnique(0).GetPass(0);
                GpuProgramParameters vertexParams = pass.VertexProgramParameters;
                vertexParams.SetNamedConstant("textureTileSize", new Vector3(textureTileSize, 0, 0));

                // set splatting textures
                int offset = alphaMapMosaicNames.Length;
                for (int i = 0; i < layerTextureNames.Length; i++)
                {
                    pass.GetTextureUnitState(offset + i).SetTextureName(layerTextureNames[i]);
                }

                pass.GetTextureUnitState(10).SetTextureName(detailTextureName);
            }
        }

        #endregion

        /// <summary>
        /// Get the texture map for a point specified by world coordinates
        /// A 8-byte array is returned as the map.  The map can have 8 possible
        /// mappings associated with it with one byte per map.  Ultimately, the
        /// first 4 bytes correspond with Alpha Map 0 and the latter 4 bytes
        /// correspond with Alpha Map 1.
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <returns></returns>
        public byte[] GetWorldTextureMap(int worldXMeters, int worldZMeters)
        {
            byte[] textureMap = new byte[MAX_LAYER_TEXTURES];

            for (int i=0; i < alphaMapMosaics.Length; i++)
            {
                TextureMosaic alphaMapMosaic = alphaMapMosaics[i];
                byte[] mosaicAlphaMap = alphaMapMosaic.GetWorldTextureMap(worldXMeters, worldZMeters);

                Array.Copy(mosaicAlphaMap, 0, textureMap, i * 4, 4);
            }

            return textureMap;
        }

        /// <summary>
        /// Set the texture map for a point specified by world coordinates.
        /// The 8-byte array is map.  The map can have 8 possible
        /// mappings associated with it with one byte per map.  Ultimately, the
        /// first 4 bytes correspond with Alpha Map 0 and the latter 4 bytes
        /// correspond with Alpha Map 1.
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <param name="textureMap"></param>
        /// <returns></returns>
        public void SetWorldTextureMap(int worldXMeters, int worldZMeters, byte[] textureMap)
        {
            for (int i = 0; i < alphaMapMosaics.Length; i++)
            {
                byte[] mosaicAlphaMap = new byte[4];
                Array.Copy(textureMap, i * 4, mosaicAlphaMap, 0, 4);

                TextureMosaic alphaMapMosaic = alphaMapMosaics[i];
                alphaMapMosaic.SetWorldTextureMap(worldXMeters, worldZMeters, mosaicAlphaMap);
            }
        }

        /// <summary>
        /// Get the texture map for a point specified by world coordinates.
        /// The 8-float array is map.  The map can have 8 possible
        /// mappings associated with it with one float per map.  The floats
        /// are normalized between the values of 0 and 1 inclusive.
        /// Ultimately, the first 4 floats correspond with Alpha Map 0 and
        /// the latter 4 floats correspond with Alpha Map 1.
        ///
        /// Each normalized float gets converted into a byte, so the range
        /// of precision is fairly narrow (1/255th or around 0.0039).  So,
        /// calling set followed by get may show a slight variance due to
        /// the precision.
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <returns></returns>
        public float[] GetWorldTextureMapNormalized(int worldXMeters, int worldZMeters)
        {
            byte[] textureMap = GetWorldTextureMap(worldXMeters, worldZMeters);
            float[] textureMapNormalized = new float[textureMap.Length];

            for (int i=0; i < textureMapNormalized.Length; i++)
            {
                textureMapNormalized[i] = textureMap[i]/255f;
            }

            return textureMapNormalized;
        }

        /// <summary>
        /// Set the texture map for a point specified by world coordinates.
        /// The 8-float array is map.  The map can have 8 possible
        /// mappings associated with it with one float per map.  The floats
        /// should be normalized between the values of 0 and 1 inclusive.
        /// Ultimately, the first 4 floats correspond with Alpha Map 0 and
        /// the latter 4 floats correspond with Alpha Map 1.
        ///
        /// Each normalized float gets converted into a byte, so the range
        /// of precision is fairly narrow (1/255th or around 0.0039).  So,
        /// calling set followed by get may show a slight variance due to
        /// the precision.
        /// </summary>
        /// <param name="worldXMeters"></param>
        /// <param name="worldZMeters"></param>
        /// <param name="textureMapNormalized"></param>
        /// <returns></returns>
        public void SetWorldTextureMapNormalized(int worldXMeters, int worldZMeters, float[] textureMapNormalized)
        {
            byte[] textureMap = new byte[textureMapNormalized.Length];

            for (int i = 0; i < textureMap.Length; i++)
            {
                textureMap[i] = (byte) Math.Round(255f * textureMap[i]);
            }

            SetWorldTextureMap(worldXMeters, worldZMeters, textureMap);
        }

        public void SuspendChangeNotifications()
        {
            m_wasModifiedBeforeSuspend = Modified;
            m_wasChangedDuringSuspend = false;

            m_changeNotificationEnabled = false;
        }

        public void ResumeChangeNotifications()
        {
            m_changeNotificationEnabled = true;

            bool shouldFire = Modified != m_wasModifiedBeforeSuspend;
            m_locallyModified = m_wasModifiedBeforeSuspend;

            if (shouldFire)
            {
                FireTerrainSplatModificationStateChanged();
            }

            if (m_wasChangedDuringSuspend)
            {
                FireTerrainSplatChanged();
            }
        }

        public void RefreshTextures()
        {
            foreach (TextureMosaic mosaic in alphaMapMosaics)
            {
                mosaic.RefreshTextures();
            }
        }

        public float[,][] GetNormalizedSamples(int xWorldLocationMeters, int zWorldLocationMeters, int sizeXSamples, int sizeZSamples, int metersPerSample)
        {
            float[,][] normalizedSamples = new float[sizeXSamples, sizeZSamples][];

            int worldZMeters = zWorldLocationMeters;
            for (int sampleZ = 0; sampleZ < sizeZSamples; sampleZ++)
            {
                int worldXMeters = xWorldLocationMeters;
                for (int sampleX = 0; sampleX < sizeXSamples; sampleX++)
                {
                    byte[] textureMapping = GetWorldTextureMap(worldXMeters, worldZMeters);
                    float[] normalizedMapping = ConvertMappingByteToNormalized(textureMapping);
                    normalizedSamples[sampleX, sampleZ] = normalizedMapping;
                    worldXMeters += metersPerSample;
                }

                worldZMeters += metersPerSample;
            }

            return normalizedSamples;
        }

        public void SetNormalizedSamples(int xWorldLocationMeters, int zWorldLocationMeters, int metersPerSample, float[,][] normalizedSamples)
        {
            int sizeXSamples = normalizedSamples.GetLength(0);
            int sizeZSamples = normalizedSamples.GetLength(1);

            // We use shorts instead of bytes because we need signed values
            float[,][] diffArray = new float[sizeXSamples, sizeZSamples][]; 

            bool wasModified = Modified;
            try
            {
                m_changeNotificationEnabled = false;

                // Calculate the heights and set them
                {
                    int worldZMeters = zWorldLocationMeters;
                    for (int sampleZ = 0; sampleZ < sizeZSamples; sampleZ++)
                    {
                        int worldXMeters = xWorldLocationMeters;
                        for (int sampleX = 0; sampleX < sizeXSamples; sampleX++)
                        {
                            float[] mappingNormalized = normalizedSamples[sampleX, sampleZ];

                            //todo: figure out whether we really want to do these checks here,
                            //todo: as they may be a performance limiter
                            // Make sure the normalized maps are within the proper bounds
                            float total = 0;
                            for (int i = 0; i < mappingNormalized.Length; i++)
                            {
                                if (mappingNormalized[i] < 0)
                                {
                                    mappingNormalized[i] = 0f;
                                }
                                else if (mappingNormalized[i] > 1)
                                {
                                    mappingNormalized[i] = 1f;
                                }

                                total += mappingNormalized[i];
                            } 

                            // Prevent oversaturation of textures
                            if (total > 1f)
                            {
                                float reductionFactor = 1/total;
                                for (int i=0; i < MAX_LAYER_TEXTURES; i++)
                                {
                                    mappingNormalized[i] *= reductionFactor;
                                }
                            }

                            float[] originalMappingNormalized = GetWorldTextureMapNormalized(worldXMeters, worldZMeters);
                            float[] mappingDiff = CalculateMappingDiff(mappingNormalized, originalMappingNormalized);
                            diffArray[sampleX, sampleZ] = mappingDiff;

                            worldXMeters += metersPerSample;
                        }

                        worldZMeters += metersPerSample;
                    }

//                    DumpDiffMap("Diffs", diffArray);
//                    DumpMapSaturation("Before apply", GetNormalizedSamples(xWorldLocationMeters, zWorldLocationMeters, sizeXSamples, sizeZSamples, metersPerSample));
                    AdjustWorldSamples(xWorldLocationMeters, zWorldLocationMeters, metersPerSample, diffArray);
//                    DumpMapSaturation("After apply", GetNormalizedSamples(xWorldLocationMeters, zWorldLocationMeters, sizeXSamples, sizeZSamples, metersPerSample));

//                    float[,][] postSet = GetNormalizedSamples(xWorldLocationMeters, zWorldLocationMeters, sizeXSamples, sizeZSamples, 1);
//                    CompareMaps("Set vs. Actual", normalizedSamples, postSet);
                }
            }
            finally
            {
                m_changeNotificationEnabled = true;

                if (Modified != wasModified)
                {
                    FireTerrainSplatModificationStateChanged();
                }

                FireTerrainSplatChanged(xWorldLocationMeters, zWorldLocationMeters, sizeXSamples * metersPerSample, sizeZSamples * metersPerSample);
            }
        }

        private static void CompareMaps(string title, float[,][] map1, float[,][] map2)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(title);
            for (int x = 0; x < map1.GetLength(0); x++)
            {
                for (int z = 0; z < map1.GetLength(1); z++)
                {
                    CompareMappingInternal(builder, map1[x, z], map2[x, z]);
                }
                builder.AppendLine();
            }

            Console.WriteLine(builder);
        }

        private static void CompareMapping(string title, float[] map1, float[] map2)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(title);
            CompareMappingInternal(builder, map1, map2);

            Console.WriteLine(builder);
        }

        private static void CompareMappingInternal(StringBuilder builder, float[] map1, float[] map2)
        {
            for (int i = 0; i < map1.Length; i++)
            {
                if (map1[i] == map2[i])
                {
                    builder.Append("=");
                }
                else if (Math.Abs(map1[i] - map2[i]) < 0.01)
                {
                    builder.Append("~"); // Close enough
                }
                else if (map1[i] < map2[i])
                {
                    builder.Append("<");
                }
                else
                {
                    builder.Append(">");
                }
            }
            builder.Append(" ");
        }

        private float[] CalculateMappingDiff(float[] newMapping, float[] oldMapping)
        {
            float[] diff = new float[oldMapping.Length];
            for (int i=0; i < diff.Length; i++)
            {
                diff[i] = newMapping[i] - oldMapping[i];
            }

            return diff;
        }

        private float[] ConvertMappingByteToNormalized(byte[] byteMapping)
        {
            int length = byteMapping.Length;
            float[] normalizedMapping = new float[length];
            for (int i=0; i < length; i++)
            {
                normalizedMapping[i] = byteMapping[i]/255f;
            }

            return normalizedMapping;
        }

        private byte[][] ConvertUpdateNormalizedToByte(float[][] normalizedMapping)
        {
            byte[][] byteMapping = new byte[normalizedMapping.Length][];
            byte biggestValue = 0;
            int biggestValueOuterIndex = 0;
            int biggestValueInnerIndex = 0;
            int saturation = 0;

            for (int outer=0; outer < normalizedMapping.Length; outer++)
            {
                byteMapping[outer] = new byte[normalizedMapping[outer].Length];
                for (int inner = 0; inner < normalizedMapping[outer].Length; inner++)
                {
                    if (normalizedMapping[outer][inner] > 1)
                    {
                        byteMapping[outer][inner] = 255;
                    }
                    else if (normalizedMapping[outer][inner] < 0)
                    {
                        byteMapping[outer][inner] = 0;
                    }
                    else
                    {
                        byteMapping[outer][inner] = (byte) (normalizedMapping[outer][inner]*255f);
                    }

                    if (byteMapping[outer][inner] > biggestValue)
                    {
                        biggestValue = byteMapping[outer][inner];
                        biggestValueOuterIndex = outer;
                        biggestValueInnerIndex = inner;
                    }
                    saturation += byteMapping[outer][inner];
                }   
            }

            // If the saturation isn't perfect, nudge the biggest value
            // up until it is.  This helps account for round off error
            // during the conversion.  Values that are off by just 1 have
            // noticable saturation difference.
            if (saturation != 0 && saturation < 255)
            {
                byteMapping[biggestValueOuterIndex][biggestValueInnerIndex] += (byte) (255 - saturation);
            }

            return byteMapping;
        }

        private void AdjustWorldSamples(int worldXMeters, int worldZMeters, int metersPerSample, float[,][] textureMapDiffs)
        {
            int sampleX;
            int sampleZ;

            float sampleXfrac;
            float sampleZfrac;

            alphaMapMosaics[0].WorldToSampleCoords(worldXMeters, worldZMeters, out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);
            int alphaMapMps = alphaMapMosaics[0].MosaicDesc.MetersPerSample;

            if ((sampleXfrac == 0) && (sampleZfrac == 0) && (metersPerSample == alphaMapMps))
            {
                for (int z = 0; z < textureMapDiffs.GetLength(1); z++)
                {
                    for (int x = 0; x < textureMapDiffs.GetLength(0); x++)
                    {
                        byte[][] existing = new byte[NUM_ALPHA_MAPS][];
                        float[][] updateNormalized = new float[NUM_ALPHA_MAPS][];
                        for (int i = 0; i < NUM_ALPHA_MAPS; i++)
                        {
                            existing[i] = alphaMapMosaics[i].GetSampleTextureMap(sampleX + x, sampleZ + z);
                            updateNormalized[i] = ApplyDiffs(existing[i], textureMapDiffs[x, z], i * 4);
                        }

                        // Make sure the resulting change is normalized
                        NormalizeUpdate(updateNormalized, existing);
                        byte[][] updateBytes = ConvertUpdateNormalizedToByte(updateNormalized);

                        for (int i = 0; i < NUM_ALPHA_MAPS; i++)
                        {
                            alphaMapMosaics[i].SetSampleTextureMap(sampleX + x, sampleZ + z, updateBytes[i]);
                        }
                    }
                }
            }
            else
            {
                int upperLeftSampleX = sampleX;
                int upperLeftSampleZ = sampleZ;

                alphaMapMosaics[0].WorldToSampleCoords(
                    worldXMeters + textureMapDiffs.GetLength(0) * metersPerSample,
                    worldZMeters + textureMapDiffs.GetLength(1) * metersPerSample,
                    out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

                int lowerRightSampleX = (sampleXfrac == 0) ? sampleX : sampleX + 1;
                int lowerRightSampleZ = (sampleZfrac == 0) ? sampleZ : sampleZ + 1;

                float[,][] diffArray = new float[lowerRightSampleX - upperLeftSampleX + 1, lowerRightSampleZ - upperLeftSampleZ + 1][];
                float[,] appliedWeight = new float[lowerRightSampleX - upperLeftSampleX + 1, lowerRightSampleZ - upperLeftSampleZ + 1];
                for (int z = 0; z < diffArray.GetLength(1); z++)
                {
                    for (int x = 0; x < diffArray.GetLength(0); x++)
                    {
                        diffArray[x, z] = new float[MAX_LAYER_TEXTURES];
                    }
                }

                int currentWorldZMeters = worldZMeters;
                for (int z = 0; z < textureMapDiffs.GetLength(1); z++)
                {
                    int currentWorldXMeters = worldXMeters;
                    for (int x = 0; x < textureMapDiffs.GetLength(0); x++)
                    {
                        alphaMapMosaics[0].WorldToSampleCoords(
                            currentWorldXMeters, currentWorldZMeters,
                            out sampleX, out sampleZ, out sampleXfrac, out sampleZfrac);

                        int xpos = sampleX - upperLeftSampleX;
                        int zpos = sampleZ - upperLeftSampleZ;

                        UpdateWeightedDiffs(diffArray[xpos, zpos], CalculateWeightedDiffs(textureMapDiffs[x, z], (1f - sampleXfrac) * (1f - sampleZfrac)));
                        UpdateWeightedDiffs(diffArray[xpos + 1, zpos], CalculateWeightedDiffs(textureMapDiffs[x, z], (sampleXfrac) * (1f - sampleZfrac)));
                        UpdateWeightedDiffs(diffArray[xpos, zpos + 1], CalculateWeightedDiffs(textureMapDiffs[x, z], (1f - sampleXfrac) * (sampleZfrac)));
                        UpdateWeightedDiffs(diffArray[xpos + 1, zpos + 1], CalculateWeightedDiffs(textureMapDiffs[x, z], (sampleXfrac) * (sampleZfrac)));

                        appliedWeight[xpos, zpos] +=         (1f - sampleXfrac) * (1f - sampleZfrac);
                        appliedWeight[xpos + 1, zpos] +=     sampleXfrac        * (1f - sampleZfrac);
                        appliedWeight[xpos, zpos + 1] +=     (1f - sampleXfrac) * sampleZfrac;
                        appliedWeight[xpos + 1, zpos + 1] += sampleXfrac        * sampleZfrac;

                        currentWorldXMeters += metersPerSample;
                    }
                    currentWorldZMeters += metersPerSample;
                }

                float[,][] averagedDiffArray = new float[lowerRightSampleX - upperLeftSampleX + 1, lowerRightSampleZ - upperLeftSampleZ + 1][];
                for (int z = 0; z < diffArray.GetLength(1); z++)
                {
                    for (int x = 0; x < diffArray.GetLength(0); x++)
                    {
                        averagedDiffArray[x,z] = new float[MAX_LAYER_TEXTURES];
                        for (int i = 0; i < averagedDiffArray[x,z].Length; i++)
                        {
                            if (appliedWeight[x, z] == 0)
                            {
                                averagedDiffArray[x, z][i] = 0;
                            }
                            else
                            {
                                averagedDiffArray[x, z][i] = diffArray[x, z][i]/appliedWeight[x, z];
                            }
                        } 
                    }
                }

                for (int z = 0; z < averagedDiffArray.GetLength(1); z++)
                {
                    for (int x = 0; x < averagedDiffArray.GetLength(0); x++)
                    {
                        byte[][] existing = new byte[NUM_ALPHA_MAPS][];
                        float[][] updateNormalized = new float[NUM_ALPHA_MAPS][];
                        for (int i = 0; i < NUM_ALPHA_MAPS; i++)
                        {
                            existing[i] = alphaMapMosaics[i].GetSampleTextureMap(upperLeftSampleX + x, upperLeftSampleZ + z);
                            updateNormalized[i] = ApplyDiffs(existing[i], averagedDiffArray[x, z], i * 4);
                        }

                        // Make sure the resulting change is normalized
                        NormalizeUpdate(updateNormalized, existing);
                        byte[][] updateBytes = ConvertUpdateNormalizedToByte(updateNormalized);
                        for (int i = 0; i < NUM_ALPHA_MAPS; i++)
                        {
                            alphaMapMosaics[i].SetSampleTextureMap(upperLeftSampleX + x, upperLeftSampleZ + z, updateBytes[i]);
                        }
                    }
                }
            }
        }

        private float[] ApplyDiffs(byte[] source, float[] diffs, int diffItemOffset)
        {
            float[] update = new float[4];
            for (int i = 0; i < 4; i++)
            {
                float diff = diffs[diffItemOffset + i];
                float normalizedSource = source[i]/255f;
                float value = normalizedSource + diff;
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 1)
                {
                    value = 1;
                }
                update[i] = value;
            }
            return update;
        }

        private float[] CalculateWeightedDiffs(float[] diffs, float weight)
        {
            float[] weightedDiffs = new float[MAX_LAYER_TEXTURES];
            for (int i = 0; i < MAX_LAYER_TEXTURES; i++)
            {
                weightedDiffs[i] = diffs[i] * weight;
            }

            return weightedDiffs;
        }

        private void UpdateWeightedDiffs(float[] diffs1, float[] diffs2)
        {
            for (int i = 0; i < diffs1.Length; i++)
            {
                diffs1[i] += diffs2[i];
            }
        }

        private void NormalizeUpdate(float[][] updateNormalized, byte[][] existingBytes)
        {
            float saturation = 0;
            for (int i = 0; i < updateNormalized.Length; i++)
            {
                for (int j = 0; j < updateNormalized[i].Length; j++)
                {
                    float value = updateNormalized[i][j];
                    saturation += value;
                }
            }

            // If the saturation is 1, then no need to adjust.
            if (saturation == 1)
            {
                return;
            }

            // If the saturation is 0, then we're probably in
            // an unitialized state.  Don't perform the update,
            // but make sure the existing state doesn't change
            if (saturation == 0)
            {
                for (int i = 0; i < updateNormalized.Length; i++)
                {
                    for (int j = 0; j < updateNormalized[i].Length; j++)
                    {
                        updateNormalized[i][j] = existingBytes[i][j] * 255;
                    }
                }
                return;
            }

            // Normalize the update
            for (int i = 0; i < updateNormalized.Length; i++)
            {
                for (int j = 0; j < updateNormalized[i].Length; j++)
                {
                    updateNormalized[i][j] = updateNormalized[i][j] / saturation;
                }
            }
        }

        public static void DumpDiffMap(string title, short[,][] map)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(title);
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    AppendMap(builder, map[x, z]);
                    builder.Append(" ");
                }
                builder.AppendLine();
            }

            Console.WriteLine(builder);
        }

        private static void AppendMap(StringBuilder builder, short[] map)
        {
            builder.Append("[");
            for (int i = 0; i < map.Length; i++)
            {
                builder.Append(map[i].ToString("####"));
                builder.Append(" ");
            }
            builder.Append("] ");
        }

        public static void DumpMapSaturation(string title, float[,][] map)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(title);
            bool perfectSaturation = true;

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int z = 0; z < map.GetLength(1); z++)
                {
                    float saturation = 0;
                    for (int i = 0; i < map[x, z].Length; i++)
                    {
                        saturation += map[x, z][i];
                    }
                    builder.Append(saturation.ToString("F3"));
                    builder.Append(" ");

                    if (saturation != 1)
                    {
                        perfectSaturation = false;
                    }
                }
                builder.AppendLine();
            }

            if (perfectSaturation)
            {
                Console.WriteLine(title + ": Perfect saturation");
            }
            else
            {
                Console.WriteLine(builder);
            }
        }

    }
}
