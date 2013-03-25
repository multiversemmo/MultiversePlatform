using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Lib.WorldMap
{
    public class MapZone : IObjectWithProperties
    {
        protected string name;
        protected List<CoordXZ> tiles;
        protected MapProperties properties;
        protected WorldMap map;
        protected List<IObjectWithProperties> tilePropertyParent;

        public MapZone(WorldMap map, string name)
        {
            this.map = map;
            this.name = name;
            tiles = new List<CoordXZ>();

            properties = new MapProperties(this);

            tilePropertyParent = new List<IObjectWithProperties>();
            tilePropertyParent.Add(this);
        }

        public MapZone(WorldMap map, XmlReader r)
        {
            this.map = map;
            properties = new MapProperties(this);

            tilePropertyParent = new List<IObjectWithProperties>();
            tilePropertyParent.Add(this);

            FromXml(r);

            tiles = new List<CoordXZ>();
        }

        protected void FromXml(XmlReader r)
        {
            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        name = r.Value;
                        break;
                }
            }

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
        }

        public void AddTile(MapTile tile)
        {
            tiles.Add(tile.TileCoord);
            //tile.Zone = this;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public List<CoordXZ> Tiles
        {
            get
            {
                return tiles;
            }
        }

        public CoordXZ MinTileCoord
        {
            get
            {
                CoordXZ minCoord = new CoordXZ();
                minCoord.x = int.MaxValue;
                minCoord.z = int.MaxValue;

                foreach (CoordXZ tileCoord in tiles)
                {
                    if (tileCoord.x < minCoord.x)
                    {
                        minCoord.x = tileCoord.x;
                    }
                    if (tileCoord.z < minCoord.z)
                    {
                        minCoord.z = tileCoord.z;
                    }
                }

                return minCoord;
            }
        }

        public CoordXZ MaxTileCoord
        {
            get
            {
                CoordXZ maxCoord = new CoordXZ();
                maxCoord.x = int.MinValue;
                maxCoord.z = int.MinValue;

                foreach (CoordXZ tileCoord in tiles)
                {
                    if (tileCoord.x > maxCoord.x)
                    {
                        maxCoord.x = tileCoord.x;
                    }
                    if (tileCoord.z > maxCoord.z)
                    {
                        maxCoord.z = tileCoord.z;
                    }
                }

                return maxCoord;
            }
        }

        public List<IObjectWithProperties> TilePropertyParent
        {
            get
            {
                return tilePropertyParent;
            }
        }

        /// <summary>
        /// This method outputs all the attributes of a zone.  Tile membership in a zone
        /// is determined in the sections file.
        /// </summary>
        /// <param name="w"></param>
        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Zone");
            w.WriteAttributeString("Name", name);
            properties.ToXml(w);
            w.WriteEndElement();
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
                return map.ZonePropertyParents;
            }
        }

        #endregion
    }
}
