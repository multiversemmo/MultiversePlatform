using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Lib.WorldMap
{
    /// <summary>
    /// A MapSection is an optimization so that we don't have to load information about
    /// all tiles in the world at startup, and we don't have to have separate files with
    /// the metadata for each tile.
    /// 
    /// Each section consists of 64x64 tiles (current value, may change).  The section file
    /// will contain all metadata about the tiles in that section.  The actual
    /// data field will typically be stored in a separate image file, one image
    /// per tile.
    /// 
    /// Any tile that has a null reference in the 'tiles' array is a default tile,
    /// which means that it has the default properties and no associated height or
    /// data maps.
    /// </summary>
    public class MapSection
    {
        protected WorldMap map;
        protected CoordXZ sectionCoord;
        protected CoordXZ tileCoord;

        protected bool dirty;

        protected bool dirtyChildren;

        protected MapTile[,] tiles;

        /// <summary>
        /// This constructor is used when creating a new map.  All tiles are null, since
        /// this is a blank world.
        /// </summary>
        /// <param name="map">the map</param>
        /// <param name="sectionCoord">the section coordinate of this object</param>
        public MapSection(WorldMap map, CoordXZ sectionCoord)
        {
            this.sectionCoord = sectionCoord;
            this.map = map;

            tileCoord = new CoordXZ(sectionCoord, map.TileSize);
            dirty = true;
            dirtyChildren = false;

            tiles = new MapTile[map.TilesPerSection, map.TilesPerSection];

        }

        public MapSection(WorldMap map, CoordXZ sectionCoord, string worldPath)
            : this(map, sectionCoord)
        {
            FromXml(worldPath);
        }

        protected void ParseTile(XmlReader r, string zoneName)
        {
            int x = 0;
            int z = 0;

            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "X":
                        x = int.Parse(r.Value);
                        break;
                    case "Z":
                        z = int.Parse(r.Value);
                        break;
                }
            }

            // create the tile
            MapTile tile = new MapTile(map, new CoordXZ(x, z, WorldMap.tileSize));

            // add the zone to the tile
            tile.Zone = map.GetZone(zoneName);

            // add the tile to the section
            AddTile(tile);

            r.MoveToElement(); //Moves the reader back to the element node.

            if (!r.IsEmptyElement)
            {
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
                            case "Property":
                                tile.Properties.ParseProperty(r);
                                break;
                        }
                    }
                    else if (r.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

        }

        protected void ParseZone(XmlReader r)
        {
            string zoneName = null;

            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        zoneName = r.Value;
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
                        case "Tile":
                            ParseTile(r, zoneName);
                            break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        public void FromXml(string worldPath)
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();

            XmlReader r = XmlReader.Create(String.Format("{0}\\{1}-({2},{3}).mms", worldPath, map.WorldName, sectionCoord.x, sectionCoord.z), xmlSettings);

            // read until we find the start of the world description
            while (r.Read())
            {
                // look for the start of the map description
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "Section")
                    {
                        break;
                    }
                }
            }

            int x = 0;
            int z = 0;

            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "SectionCoordX":
                        x = int.Parse(r.Value);
                        break;
                    case "SectionCoordZ":
                        z = int.Parse(r.Value);
                        break;
                }
            }

            Debug.Assert((x == sectionCoord.x) && (z == sectionCoord.z));

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
                            ParseZone(r);
                            break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        public void ToXml(string worldPath)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            XmlWriter w = XmlWriter.Create(String.Format("{0}\\{1}-({2},{3}).mms", worldPath, map.WorldName, sectionCoord.x, sectionCoord.z), xmlSettings);

            w.WriteStartElement("Section");

            w.WriteAttributeString("SectionCoordX", sectionCoord.x.ToString());
            w.WriteAttributeString("SectionCoordZ", sectionCoord.z.ToString());

            // sort tiles by zone
            Dictionary<string, List<MapTile>> zoneTiles = new Dictionary<string,List<MapTile>>();
            List<MapTile> noZone = new List<MapTile>();

            for (int z = 0; z < map.TilesPerSection; z++)
            {
                for (int x = 0; x < map.TilesPerSection; x++)
                {
                    MapTile tile = tiles[x, z];
                    if (tile != null)
                    {
                        if (tile.Zone == null)
                        {
                            // tile doesn't belong to a zone, so add it to the no zone list
                            noZone.Add(tile);
                        }
                        else
                        {
                            // if the zone doesn't have an entry in the mapping dictionary, then create a list and add it
                            if (! zoneTiles.ContainsKey(tile.Zone.Name))
                            {
                                zoneTiles[tile.Zone.Name] = new List<MapTile>();
                            }

                            // add the tile to the list for its zone
                            zoneTiles[tile.Zone.Name].Add(tile);
                        }
                    }
                }
            }

            // write zones
            foreach (KeyValuePair<string, List<MapTile>> kvp in zoneTiles)
            {
                w.WriteStartElement("Zone");
                w.WriteAttributeString("Name", kvp.Key);

                foreach (MapTile tile in kvp.Value)
                {
                    w.WriteStartElement("Tile");
                    w.WriteAttributeString("X", tile.TileCoord.x.ToString());
                    w.WriteAttributeString("Z", tile.TileCoord.z.ToString());
                    tile.Properties.ToXml(w);
                    w.WriteEndElement(); // Tile
                }

                w.WriteEndElement(); // Zone
            }

            // section end
            w.WriteEndElement(); // Section
            w.Close();

        }

        public void Save(string worldPath)
        {
            if (dirty || dirtyChildren)
            {
                ToXml(worldPath);
            }

            dirty = false;
            dirtyChildren = false;
        }

        /// <summary>
        /// Add a new tile to the section
        /// </summary>
        /// <param name="tile"></param>
        public void AddTile(MapTile tile)
        {
            // compute coordinates of tile within the section
            CoordXZ localTileCoord = tile.TileCoord - tileCoord;

            if (tiles[localTileCoord.x, localTileCoord.z] == null)
            {
                tiles[localTileCoord.x, localTileCoord.z] = tile;
            }
            else
            {
                throw new ArgumentException("AddTile: tile already exists at given coordinate");
            }

            if (tile.Dirty)
            {
                dirtyChildren = true;
            }
        }

        public MapTile GetTile(CoordXZ tileCoord)
        {
            // compute coordinates of tile within the section
            CoordXZ localTileCoord = tileCoord - this.tileCoord;

            MapTile tile = tiles[localTileCoord.x, localTileCoord.z];

            return tile;
        }

        public bool Dirty
        {
            get
            {
                return ( dirty || dirtyChildren );
            }
        }

    }
}
