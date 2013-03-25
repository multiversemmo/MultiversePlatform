using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Axiom.Core;
using Multiverse.Lib.Coordinates;

namespace Multiverse.Lib.WorldMap
{
    public delegate MapLayer LayerParser(WorldMap map, XmlReader r);

    public abstract class MapLayer : IObjectWithProperties
    {
        protected WorldMap map;

        protected string layerName;

        /// <summary>
        /// size of the tile in world coordinates
        /// </summary>
        protected int tileSize;
        protected int metersPerTile;
        protected int metersPerSample;
        protected int samplesPerTile;

        protected Dictionary<CoordXZ, MapBuffer> tiles;

        protected static Dictionary<string, LayerParser> layerParsers;

        protected MapProperties properties;

        static MapLayer()
        {
            layerParsers = new Dictionary<string, LayerParser>();

            // register layer types
            ValueMapLayer16.Register();
            ColorMapLayer.Register();
        }

        public static void RegisterLayerParser(string typeName, LayerParser parser)
        {
            layerParsers.Add(typeName, parser);
        }

        public static MapLayer MapLayerFactory(WorldMap map, XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                if (r.Name == "Type")
                {
                    LayerParser factory = layerParsers[r.Value];

                    if (factory != null)
                    {
                        return factory(map, r);
                    }
                }
            }

            return null;
        }

        public MapLayer(WorldMap map, string layerName, int metersPerTile, int metersPerSample)
        {
            this.map = map;
            this.layerName = layerName;
            this.metersPerTile = metersPerTile;
            this.tileSize = metersPerTile * WorldMap.oneMeter;
            this.metersPerSample = metersPerSample;
            this.samplesPerTile = metersPerTile / metersPerSample;

            tiles = new Dictionary<CoordXZ, MapBuffer>();
            properties = new MapProperties(this);
        }

        /// <summary>
        /// Convert the given coordinates to tile coordinates appropriate for this layer
        /// </summary>
        /// <param name="coordIn"></param>
        /// <returns></returns>
        public CoordXZ ToLayerTileCoords(CoordXZ coordIn)
        {
            return new CoordXZ(coordIn, tileSize);
        }

        public string TilePath(CoordXZ tileCoord)
        {
            return string.Format("{0}/{1}-{2}({3},{4}).png", map.WorldPath, map.WorldName, layerName, tileCoord.x, tileCoord.z);
        }

        public bool TileLoaded(CoordXZ tileCoord)
        {
            return tiles.ContainsKey(tileCoord);
        }

        public bool TileExists(CoordXZ tileCoord)
        {
            if (tiles.ContainsKey(tileCoord))
            {
                return ( tiles[tileCoord] != null );
            }
            else if (System.IO.File.Exists(TilePath(tileCoord)))
            {
                return true;
            }

            return false;
        }

        protected abstract MapBuffer LoadTileImpl(CoordXZ tileCoord);

        public void LoadTile(CoordXZ tileCoord)
        {
            if (!TileLoaded(tileCoord))
            {
                tiles[tileCoord] = LoadTileImpl(tileCoord);
            }
        }

        public void UnloadTile(CoordXZ tileCoord)
        {
            if (TileLoaded(tileCoord))
            {
                MapBuffer buffer = tiles[tileCoord];

                if (buffer != null)
                {
                    if (buffer.Dirty)
                    {
                        buffer.Save(TilePath(tileCoord));
                    }
                }

                tiles.Remove(tileCoord);
            }
        }

        public void Flush()
        {
            foreach (KeyValuePair<CoordXZ, MapBuffer> kvp in tiles)
            {
                CoordXZ tileCoord = kvp.Key;
                MapBuffer buffer = kvp.Value;

                if ((buffer != null) && buffer.Dirty)
                {
                    buffer.Save(TilePath(tileCoord));
                }
            }
        }

        public abstract void ToXml(XmlWriter w);

        /// <summary>
        /// Copy a source buffer into the tile.  The buffer must fit within a single
        /// tile.  
        /// </summary>
        /// <param name="destCoordIn"></param>
        /// <param name="src"></param>
        public void CopyIn(CoordXZ destCoordIn, MapBuffer src)
        {
            Debug.Assert(metersPerSample == src.MetersPerSample);

            // convert to tile coordinates for this layer
            CoordXZ destTileCoord = new CoordXZ(destCoordIn, tileSize);

            // compute the offset in world coordinates within the 
            CoordXZ worldDestCoord = new CoordXZ(destCoordIn, WorldMap.oneMeter);
            CoordXZ worldDestTileCoord = new CoordXZ(destTileCoord, WorldMap.oneMeter);
            CoordXZ tileOffset = worldDestCoord - worldDestTileCoord;

            if (TileExists(destTileCoord))
            {
                // load the tile if necessary
                if (!TileLoaded(destTileCoord))
                {
                    LoadTile(destTileCoord);
                }
            }
            else
            {
                // If the tile doesn't exist, then create it.
                CreateTile(destTileCoord);
            }

            // copy the source image into the tile
            tiles[destTileCoord].Copy(tileOffset.x / metersPerSample, tileOffset.z / metersPerSample, src);
        }

        /// <summary>
        /// Create a thumbnail bitmap of the map area 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="worldSize"></param>
        /// <param name="pixelSize"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap CreateThumbnail(CoordXZ coord, int worldSize, int pixelSize)
        {
            int numSamples = worldSize / (metersPerSample * map.OneMeter);

            Debug.Assert(WorldMap.IsPowerOf2(numSamples));
            Debug.Assert(WorldMap.IsPowerOf2(pixelSize));

            CoordXZ tileCoord;
            CoordXZ sampleOffset;

            bool exact = CoordToTileOffset(coord, out tileCoord, out sampleOffset);

            // make sure that coordinate is an exact sample
            Debug.Assert(exact);

            if (!TileLoaded(tileCoord))
            {
                LoadTile(tileCoord);
            }

            MapBuffer tile = tiles[tileCoord];
            return tile.CreateThumbnail(sampleOffset.x, sampleOffset.z, worldSize / (metersPerSample * map.OneMeter), pixelSize);
        }

        /// <summary>
        /// Compute the tile coordinate and sample offset within the tile of a given coordinate
        /// </summary>
        /// <param name="coordIn">The input coordinate</param>
        /// <param name="tileCoord">The tile coordinate</param>
        /// <param name="sampleOffset">Sample offset within the tile</param>
        /// <returns>Whether the input coordinate exactly hits a sample</returns>
        protected bool CoordToTileOffset(CoordXZ coordIn, out CoordXZ tileCoord, out CoordXZ sampleOffset)
        {
            int sampleSize = metersPerSample * map.OneMeter;

            tileCoord = new CoordXZ(coordIn, tileSize);
            CoordXZ sampleCoord = new CoordXZ(coordIn, sampleSize);
            CoordXZ tileSampleCoord = new CoordXZ(tileCoord, sampleSize);

            sampleOffset = sampleCoord - tileSampleCoord;

            CoordXZ worldIn = new CoordXZ(coordIn, 1);
            CoordXZ worldSample = new CoordXZ(sampleCoord, 1);

            return (worldIn == worldSample);
        }

        protected void CreateTile(CoordXZ tileCoord)
        {
            tiles[tileCoord] = CreateTileImpl(tileCoord);
        }

        protected abstract MapBuffer CreateTileImpl(CoordXZ tileCoord);

        #region Properties
        public string LayerName
        {
            get
            {
                return layerName;
            }
            set
            {
                layerName = value;
            }
        }

        public int TileSize
        {
            get 
            {
                return tileSize;
            }
        }

        public int MetersPerSample
        {
            get 
            {
                return metersPerSample;
            }
        }

        public int SamplesPerTile
        {
            get
            {
                return samplesPerTile;
            }
        }

        public int MetersPerTile
        {
            get
            {
                return metersPerTile;
            }
        }

        public abstract bool IsColorLayer
        {
            get;
        }

        public abstract bool IsValueLayer
        {
            get;
        }

        #endregion Properties

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
                return map.EmptyPropertyParents;
            }
        }

        #endregion
    }

    public abstract class ValueMapLayer : MapLayer
    {

        protected float valueBase;
        protected float valueRange;

        protected uint defaultRawValue;
        protected uint maxRaw;
        protected float fmaxRaw;
        protected float defaultValue;

        public ValueMapLayer(WorldMap map, string layerName, int metersPerTile, int metersPerSample, 
            float valueBase, float valueRange, uint defaultRawValue, uint maxRaw)
            : base(map, layerName, metersPerTile, metersPerSample)
        {
            Debug.Assert(defaultRawValue <= maxRaw);

            this.valueBase = valueBase;
            this.valueRange = valueRange;
            this.maxRaw = maxRaw;
            fmaxRaw = (float)maxRaw;

            // use the property so that defaultValue gets calculated automatically
            DefaultRawValue = defaultRawValue;
        }

        /// <summary>
        /// Convert a raw data element to a scaled value.  Since we do floating point math
        /// here, we will lose precision if the maximum raw value is greater than the
        /// precission of the floating point mantissa.
        /// </summary>
        /// <param name="raw">The raw data value to convert</param>
        /// <returns>The scaled value</returns>
        protected virtual float RawToValue(uint raw)
        {
            return ((RawToNorm(raw) * valueRange) + valueBase);
        }

        protected virtual float RawToNorm(uint raw)
        {
            float fraw = (float)raw;
            return fraw / fmaxRaw;
        }

        protected void RecomputeDefault()
        {
            defaultValue = RawToValue(defaultRawValue);
        }

        public abstract MapBuffer CreateCompatibleMapBuffer(Image src, int metersPerPixel,
                    int srcX, int srcZ, int numSamples, float srcMinValue, float srcMaxValue);

        #region Properties

        public virtual float DefaultValue
        {
            get
            {
                return defaultValue;
            }
        }

        public virtual uint DefaultRawValue
        {
            get
            {
                return defaultRawValue;
            }
            set
            {
                defaultRawValue = value;
                RecomputeDefault();
            }
        }

        public virtual float ValueBase
        {
            get
            {
                return valueBase;
            }
            set
            {
                valueBase = value;
                RecomputeDefault();
            }
        }

        public virtual float ValueRange
        {
            get
            {
                return valueRange;
            }
            set
            {
                valueRange = value;
                RecomputeDefault();
            }
        }

        public override bool IsColorLayer
        {
            get
            {
                return false;
            }
        }

        public override bool IsValueLayer
        {
            get
            {
                return true;
            }
        }
        #endregion Properties

    }

    public class ValueMapLayer16 : ValueMapLayer
    {
        public ValueMapLayer16(WorldMap map, string layerName, int metersPerTile, int metersPerSample,
            float valueBase, float valueRange, float defaultValue)
            :
            base(map, layerName, metersPerTile, metersPerSample, valueBase, valueRange, 
                (uint)((( defaultValue - valueBase ) / valueRange)* ushort.MaxValue), ushort.MaxValue)
        {

        }

        public static void Register()
        {
            RegisterLayerParser("ValueMapLayer16", new LayerParser(ParseLayer));
        }

        protected static MapLayer ParseLayer(WorldMap map, XmlReader r)
        {
            string layerName = null;
            int metersPerTile = 0;
            int metersPerSample = 0;
            float valueBase = 0;
            float valueRange = 0;
            float defaultValue = 0;

            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Type":
                        break;
                    case "Name":
                        layerName = r.Value;
                        break;
                    case "MetersPerTile":
                        metersPerTile = int.Parse(r.Value);
                        break;
                    case "MetersPerSample":
                        metersPerSample = int.Parse(r.Value);
                        break;
                    case "ValueBase":
                        valueBase = float.Parse(r.Value);
                        break;
                    case "ValueRange":
                        valueRange = float.Parse(r.Value);
                        break;
                    case "DefaultValue":
                        defaultValue = float.Parse(r.Value);
                        break;

                }
            }

            r.MoveToElement(); //Moves the reader back to the element node.

            MapLayer layer = new ValueMapLayer16(map, layerName, metersPerTile, metersPerSample, valueBase, valueRange, defaultValue);

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
                                layer.Properties.ParseProperty(r);
                                break;
                        }
                    }
                    else if (r.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

            return layer;
        }

        protected override MapBuffer LoadTileImpl(CoordXZ tileCoord)
        {
            return new MapBuffer16(map, TilePath(tileCoord));
        }

        /// <summary>
        /// Create a new tile and fill it with the default value for the layer
        /// </summary>
        /// <param name="tileCoord"></param>
        /// <returns></returns>
        protected override MapBuffer CreateTileImpl(CoordXZ tileCoord)
        {
            MapBuffer16 buffer = new MapBuffer16(map, samplesPerTile, metersPerSample);

            buffer.Fill(defaultRawValue);

            return buffer;
        }

        public override void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Layer");
            w.WriteAttributeString("Type", "ValueMapLayer16");
            w.WriteAttributeString("Name", layerName);
            w.WriteAttributeString("MetersPerTile", metersPerTile.ToString());
            w.WriteAttributeString("MetersPerSample", metersPerSample.ToString());
            w.WriteAttributeString("ValueBase", valueBase.ToString());
            w.WriteAttributeString("ValueRange", valueRange.ToString());
            w.WriteAttributeString("DefaultValue", defaultValue.ToString());
            properties.ToXml(w);
            w.WriteEndElement(); // Layer
        }

        public override MapBuffer CreateCompatibleMapBuffer(Image src, int metersPerPixel,
                    int srcX, int srcZ, int numSamples, float srcMinValue, float srcMaxValue)
        {
            return new MapBuffer16(map, src, metersPerPixel, srcX, srcZ, srcMinValue, srcMaxValue, numSamples, this.valueBase, this.valueBase + this.valueRange);
        }
    }

    public class ColorMapLayer : MapLayer
    {
        ColorEx defaultColor;

        public ColorMapLayer(WorldMap map, string layerName, int metersPerTile, int metersPerSample, 
            ColorEx defaultColor)
            : base(map, layerName, metersPerTile, metersPerSample)
        {
            this.defaultColor = defaultColor;
        }

        #region Properties

        public virtual ColorEx DefaultColor
        {
            get
            {
                return defaultColor;
            }
        }

        public override bool IsColorLayer
        {
            get
            {
                return true;
            }
        }

        public override bool IsValueLayer
        {
            get
            {
                return false;
            }
        }
        #endregion Properties

        public static void Register()
        {
            RegisterLayerParser("ColorMapLayer", new LayerParser(ParseLayer));
        }

        protected static MapLayer ParseLayer(WorldMap map, XmlReader r)
        {
            string layerName = null;
            int metersPerTile = 0;
            int metersPerSample = 0;
            float red = 0;
            float green = 0;
            float blue = 0;
            float alpha = 0;

            // parse attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Type":
                        break;
                    case "Name":
                        layerName = r.Value;
                        break;
                    case "MetersPerTile":
                        metersPerTile = int.Parse(r.Value);
                        break;
                    case "MetersPerSample":
                        metersPerSample = int.Parse(r.Value);
                        break;
                    case "DefaultColorR":
                        red = float.Parse(r.Value);
                        break;
                    case "DefaultColorG":
                        green = float.Parse(r.Value);
                        break;
                    case "DefaultColorB":
                        blue = float.Parse(r.Value);
                        break;
                    case "DefaultColorA":
                        alpha = float.Parse(r.Value);
                        break;

                }
            }

            r.MoveToElement(); //Moves the reader back to the element node.

            MapLayer layer = new ColorMapLayer(map, layerName, metersPerTile, metersPerSample, new ColorEx(alpha, red, green, blue));

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
                                layer.Properties.ParseProperty(r);
                                break;
                        }
                    }
                    else if (r.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

            return layer;
        }

        protected override MapBuffer LoadTileImpl(CoordXZ tileCoord)
        {
            return new MapBufferARGB(map, TilePath(tileCoord));
        }

        /// <summary>
        /// Create a new tile and fill it with the default value for the layer
        /// </summary>
        /// <param name="tileCoord"></param>
        /// <returns></returns>
        protected override MapBuffer CreateTileImpl(CoordXZ tileCoord)
        {
            MapBuffer buffer = new MapBufferARGB(map, samplesPerTile, metersPerSample);

            buffer.Fill((uint)defaultColor.ToARGB());

            return buffer;
        }

        public override void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Layer");
            w.WriteAttributeString("Type", "ColorMapLayer");
            w.WriteAttributeString("Name", layerName);
            w.WriteAttributeString("MetersPerTile", metersPerTile.ToString());
            w.WriteAttributeString("MetersPerSample", metersPerSample.ToString());
            w.WriteAttributeString("DefaultColorR", defaultColor.r.ToString());
            w.WriteAttributeString("DefaultColorG", defaultColor.g.ToString());
            w.WriteAttributeString("DefaultColorB", defaultColor.b.ToString());
            w.WriteAttributeString("DefaultColorA", defaultColor.a.ToString());
            properties.ToXml(w);
            w.WriteEndElement(); // Layer
        }

        public virtual MapBuffer CreateCompatibleMapBuffer(Image src, int metersPerPixel, int srcX, int srcZ, int numSamples)
        {
            return new MapBufferARGB(map, src, metersPerPixel, srcX, srcZ, numSamples);
        }
    }
}
