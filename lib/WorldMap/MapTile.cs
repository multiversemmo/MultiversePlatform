using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Lib.WorldMap
{
    public class MapTile : IObjectWithProperties
    {
        protected WorldMap map;

        protected CoordXZ tileCoord;

        protected bool dirty;

        protected MapZone zone;

        protected MapProperties properties;

        public MapTile(WorldMap map, CoordXZ tileCoord)
        {
            this.map = map;
            this.tileCoord = tileCoord;
            zone = null;
            this.dirty = false;

            properties = new MapProperties(this);
        }    

        public CoordXZ TileCoord
        {
            get
            {
                return tileCoord;
            }
        }

        public bool Dirty
        {
            get
            {
                return dirty;
            }
        }

        public MapZone Zone
        {
            get
            {
                return zone;
            }
            set
            {
                Debug.Assert(zone == null);
                zone = value;
                zone.AddTile(this);
            }
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
                return zone.TilePropertyParent;
            }
        }

        #endregion
    }
}
