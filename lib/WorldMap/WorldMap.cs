using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Lib.WorldMap
{
    public delegate void NewTileHandler(MapTile tile);

    public class WorldMap : IObjectWithProperties
    {
        public static readonly int oneMeter = 1000;
        public static readonly int metersPerTile = 512;
        public static readonly int tilesPerSection = 64;

        public static readonly int tileSize;
        public static readonly int sectionSize;

        protected static readonly int defaultMetersPerTile = 1024;

        public event NewTileHandler NewTile;

        protected CoordXZ minTile;
        protected CoordXZ maxTile;

        protected float minHeight;
        protected float maxHeight;

        protected Dictionary<CoordXZ, MapSection> sections;
        protected Dictionary<string, MapZone> zones;
        protected Dictionary<string, MapLayer> layers;

        protected MapLayer heightFieldLayer;
        protected MapLayer alpha0Layer;
        protected MapLayer alpha1Layer;

        protected string worldName;
        protected string worldPath;

        protected MapProperties properties;
        protected List<IObjectWithProperties> emptyOWP;
        protected List<IObjectWithProperties> layerAndWorldOWP;

        static WorldMap()
        {
            tileSize = oneMeter * metersPerTile;
            sectionSize = tileSize * tilesPerSection;
        }

        protected void InitLayers(float defaultHeight)
        {
            // create heightfield layer
            heightFieldLayer = new ValueMapLayer16(this, "heightfield", defaultMetersPerTile, 1, minHeight, maxHeight - minHeight, defaultHeight);
            layers.Add("heightfield", heightFieldLayer);
            layerAndWorldOWP.Add(heightFieldLayer);

            // create alpha0 layer
            alpha0Layer = new ColorMapLayer(this, "alpha0", defaultMetersPerTile, 1, new Axiom.Core.ColorEx(1, 0, 0, 0));
            layers.Add("alpha0", alpha0Layer);
            layerAndWorldOWP.Add(alpha0Layer);

            alpha0Layer.Properties.NewProperty("TerrainTexture0", "Alpha Layer 0 Terrain Textures", "Terrain Texture 0", typeof(string), "terrain0.dds");
            alpha0Layer.Properties.NewProperty("TerrainTexture1", "Alpha Layer 0 Terrain Textures", "Terrain Texture 1", typeof(string), "terrain1.dds");
            alpha0Layer.Properties.NewProperty("TerrainTexture2", "Alpha Layer 0 Terrain Textures", "Terrain Texture 2", typeof(string), "terrain2.dds");
            alpha0Layer.Properties.NewProperty("TerrainTexture3", "Alpha Layer 0 Terrain Textures", "Terrain Texture 3", typeof(string), "terrain3.dds");

            // create alpha1 layer
            alpha1Layer = new ColorMapLayer(this, "alpha1", defaultMetersPerTile, 1, new Axiom.Core.ColorEx(0, 0, 0, 0));
            layers.Add("alpha1", alpha1Layer);
            layerAndWorldOWP.Add(alpha1Layer);

            alpha1Layer.Properties.NewProperty("TerrainTexture4", "Alpha Layer 1 Terrain Textures", "Terrain Texture 4", typeof(string), "terrain4.dds");
            alpha1Layer.Properties.NewProperty("TerrainTexture5", "Alpha Layer 1 Terrain Textures", "Terrain Texture 5", typeof(string), "terrain5.dds");
            alpha1Layer.Properties.NewProperty("TerrainTexture6", "Alpha Layer 1 Terrain Textures", "Terrain Texture 6", typeof(string), "terrain6.dds");
            alpha1Layer.Properties.NewProperty("TerrainTexture7", "Alpha Layer 1 Terrain Textures", "Terrain Texture 7", typeof(string), "terrain7.dds");

            // add world to end of list
            layerAndWorldOWP.Add(this);
        }

        /// <summary>
        /// This constructor is used when creating a new world from scratch.
        /// </summary>
        /// <param name="minTile"></param>
        /// <param name="maxTile"></param>
        /// <param name="minHeight"></param>
        /// <param name="maxHeight"></param>
        /// <param name="defaultHeight"></param>
        public WorldMap(string worldName, CoordXZ minTile, CoordXZ maxTile, float minHeight, float maxHeight, float defaultHeight)
        {
            this.worldName = worldName;
            this.minTile = minTile;
            this.maxTile = maxTile;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;

            emptyOWP = new List<IObjectWithProperties>();
            layerAndWorldOWP = new List<IObjectWithProperties>();

            properties = new MapProperties(this);

            layers = new Dictionary<string, MapLayer>();

            InitLayers(defaultHeight);

            // create map sections
            sections = new Dictionary<CoordXZ, MapSection>();
            zones = new Dictionary<string, MapZone>();

            CoordXZ minSection = new CoordXZ(minTile, sectionSize);
            CoordXZ maxSection = new CoordXZ(maxTile, sectionSize);

            for (int z = minSection.z; z <= maxSection.z; z++)
            {
                for (int x = minSection.x; x <= maxSection.x; x++)
                {
                    CoordXZ sectionCoord = new CoordXZ(x, z, sectionSize);
                    sections[sectionCoord] = new MapSection(this, sectionCoord);
                }
            }
        }

        public WorldMap(string filename)
        {
            // create map sections
            sections = new Dictionary<CoordXZ, MapSection>();
            zones = new Dictionary<string, MapZone>();
            layers = new Dictionary<string, MapLayer>();

            emptyOWP = new List<IObjectWithProperties>();
            layerAndWorldOWP = new List<IObjectWithProperties>();

            properties = new MapProperties(this);

            worldPath = System.IO.Path.GetDirectoryName(filename);

            FromXml(filename);

            // add worldmap to the end of the list
            layerAndWorldOWP.Add(this);

            // create heightfield layer
            heightFieldLayer = layers["heightfield"];
            alpha0Layer = layers["alpha0"];
            alpha1Layer = layers["alpha1"];

            Debug.Assert(heightFieldLayer != null);
            Debug.Assert(alpha0Layer != null);
            Debug.Assert(alpha1Layer != null);
        }

        public void ToXml(string worldPath)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            XmlWriter w = XmlWriter.Create(String.Format("{0}\\{1}.mwm", worldPath, worldName), xmlSettings);

            w.WriteStartElement("WorldMap");

            w.WriteAttributeString("WorldName", worldName);

            w.WriteAttributeString("MinTileX", minTile.x.ToString());
            w.WriteAttributeString("MinTileZ", minTile.z.ToString());
            w.WriteAttributeString("MaxTileX", maxTile.x.ToString());
            w.WriteAttributeString("MaxTileZ", maxTile.z.ToString());

            w.WriteAttributeString("MinHeight", minHeight.ToString());
            w.WriteAttributeString("MaxHeight", maxHeight.ToString());

            // write world level properties
            properties.ToXml(w);

            // write zones
            foreach (MapZone zone in zones.Values)
            {
                zone.ToXml(w);
            }

            // write layers
            foreach (MapLayer layer in layers.Values)
            {
                layer.ToXml(w);
            }

            // world end
            w.WriteEndElement();
            w.Close();

        }

        public void FromXml(string mapFilename)
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();

            XmlReader r = XmlReader.Create(mapFilename);

            // read until we find the start of the world description
            while (r.Read())
            {
                // look for the start of the map description
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "WorldMap")
                    {
                        break;
                    }
                }
            }

            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "WorldName":
                        worldName = r.Value;
                        break;
                    case "MinTileX":
                        minTile.x = int.Parse(r.Value);
                        break;
                    case "MinTileZ":
                        minTile.z = int.Parse(r.Value);
                        break;
                    case "MaxTileX":
                        maxTile.x = int.Parse(r.Value);
                        break;
                    case "MaxTileZ":
                        maxTile.z = int.Parse(r.Value);
                        break;
                    case "MinHeight":
                        minHeight = float.Parse(r.Value);
                        break;
                    case "MaxHeight":
                        maxHeight = float.Parse(r.Value);
                        break;
                        
                }
            }

            r.MoveToElement(); //Moves the reader back to the element node.

            // now parse the sub-elements
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                        case "Zone":
                            MapZone zone = new MapZone(this, r);
                            zones.Add(zone.Name, zone);
                            break;
                        case "Layer":
                            MapLayer layer = MapLayer.MapLayerFactory(this, r);
                            layers.Add(layer.LayerName, layer);
                            layerAndWorldOWP.Add(layer);
                            break;
                        case "Property":
                            properties.ParseProperty(r);
                            break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        public void Save(string worldPath)
        {
            // remember the path
            this.worldPath = worldPath;

            // Save world map file
            ToXml(worldPath);

            // Save sections and tiles
            foreach (MapSection section in sections.Values)
            {
                section.Save(worldPath);
            }

            foreach (MapLayer layer in layers.Values)
            {
                layer.Flush();
            }
        }

        public MapSection GetSection(CoordXZ tileCoord)
        {
            CoordXZ sectionCoord = new CoordXZ(tileCoord, sectionSize);

            if (!sections.ContainsKey(sectionCoord))
            {
                sections[sectionCoord] = new MapSection(this, sectionCoord, worldPath);
            }
            return sections[sectionCoord];
        }

        public MapTile GetTile(CoordXZ tileCoord)
        {
            MapSection section = GetSection(tileCoord);

            return section.GetTile(tileCoord);
        }

        protected void OnNewTile(MapTile tile)
        {
            NewTileHandler handler = NewTile;
            if (handler != null)
            {
                handler(tile);
            }
        }

        public MapTile CreateTile(CoordXZ tileCoord)
        {
            MapTile tile = new MapTile(this, tileCoord);

            MapSection section = GetSection(tileCoord);

            section.AddTile(tile);

            OnNewTile(tile);

            return tile;
        }

        public MapZone CreateZone(string name)
        {
            MapZone zone = new MapZone(this, name);

            zones.Add(name, zone);

            return zone;
        }

        public MapZone GetZone(string name)
        {
            return zones[name];
        }

        public MapLayer GetLayer(string name)
        {
            return layers[name];
        }

        #region Properties


        public List<string> ZoneNames
        {
            get
            {
                return new List<string>(zones.Keys);
            }
        }

        public List<string> LayerNames
        {
            get
            {
                return new List<string>(layers.Keys);
            }
        }


        public int OneMeter
        {
            get
            {
                return oneMeter;
            }
        }

        public int MetersPerTile
        {
            get
            {
                return metersPerTile;
            }
        }

        public int TilesPerSection
        {
            get
            {
                return tilesPerSection;
            }
        }

        public int TileSize
        {
            get
            {
                return tileSize;
            }
        }

        public int SectionSize
        {
            get
            {
                return sectionSize;
            }
        }

        public CoordXZ MinTile
        {
            get
            {
                return minTile;
            }
        }

        public CoordXZ MaxTile
        {
            get
            {
                return maxTile;
            }
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

        public string WorldName
        {
            get
            {
                return worldName;
            }
        }

        public string WorldPath
        {
            get
            {
                return worldPath;
            }
        }

        public MapLayer HeightFieldLayer
        {
            get
            {
                return heightFieldLayer;
            }
        }

        public List<IObjectWithProperties> ZonePropertyParents
        {
            get
            {
                return layerAndWorldOWP;
            }
        }

        public List<IObjectWithProperties> EmptyPropertyParents
        {
            get
            {
                return emptyOWP;
            }
        }

        #endregion Properties

        public static bool IsPowerOf2(int x)
        {
            return ((x & -x) == x);
        }

        #region IObjectWithProperties Members

        public MapProperties Properties
        {
            get
            {
                return properties;
            }
        }

        public List<IObjectWithProperties> PropertyParents
        {
            get
            {
                return emptyOWP;
            }
        }

        #endregion
    }
}
