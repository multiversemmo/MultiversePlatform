#region Using directives

using System;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Axiom.SceneManagers.Multiverse;
using Axiom.MathLib;

#endregion

namespace Multiverse.Generator
{
	public class FractalTerrainGenerator : ITerrainGenerator
	{
        public bool Modified { get { return false; }}
        public event TerrainModificationStateChangedHandler TerrainModificationStateChanged;
	    public event TerrainChangedHandler TerrainChanged;

	    protected float xOff;
		protected float yOff;

	    protected float h = 0.25f;
		protected float lacunarity = 2.0f;
		protected float octaves = 8.0f;

		protected float metersPerPerlinUnit = 500f;

	    protected int heightPointsGenerated;

		protected float[] lacPowH;

        protected GeneratorAlgorithm algorithm = GeneratorAlgorithm.HybridMultifractalWithFloor;

	    protected int bitsPerSample;

        protected Vector3 seedMapOrigin = new Vector3(0,0,0);
        protected float seedMapMetersPerSample = 128;
        protected Vector3 perlinSpaceSeedMapOrigin;
        protected float seedMapPerlinUnitsPerSample;

	    protected delegate float FractalFuncDelegate(float x, float y, float z);

        protected FractalFuncDelegate fractalFunc;

        private static float[] interpTable;

		#region Properties

        public GeneratorAlgorithm Algorithm
        {
            get
            {
                return algorithm;
            }
            set
            {
                algorithm = value;
                switch(Algorithm)
                {
                    case GeneratorAlgorithm.HybridMultifractalWithFloor:
                        fractalFunc = hybridMultifractal;
                        break;
                    case GeneratorAlgorithm.HybridMultifractalWithSeedMap:
                        fractalFunc = hybridMultifractalWithSeedMap;
                        break;
                }
            }
        }

        // This is the height to use for the seed value outside the range of the seed map
	    public float OutsideMapSeedHeight { get; set; }


        //
        // Properties to set the seed map location within the world
        //
        public Vector3 SeedMapOrigin
        {
            get
            {
                return seedMapOrigin;
            }
            set
            {
                seedMapOrigin = value;
                UpdateSeedMapPerlinSpace();
            }
        }

        public float SeedMapOriginX
        {
            get
            {
                return seedMapOrigin.x;
            }
            set
            {
                seedMapOrigin.x = value;
                UpdateSeedMapPerlinSpace();
            }
        }

        public float SeedMapOriginY
        {
            get
            {
                return seedMapOrigin.y;
            }
            set
            {
                seedMapOrigin.y = value;
                UpdateSeedMapPerlinSpace();
            }
        }

        public float SeedMapOriginZ
        {
            get
            {
                return seedMapOrigin.z;
            }
            set
            {
                seedMapOrigin.z = value;
                UpdateSeedMapPerlinSpace();
            }
        }

        // This property sets the size in the world of each sample of the seed map
        public float SeedMapMetersPerSample
        {
            get
            {
                return seedMapMetersPerSample;
            }
            set
            {
                seedMapMetersPerSample = value;
                UpdateSeedMapPerlinSpace();
            }
        }

        //
        // These offsets are used to shift the map origin around in the perlin space
        //
		public float XOff 
		{
			get
			{
				return xOff;
			}
			set
			{
				xOff = value;
                UpdateSeedMapPerlinSpace();
			}
		}

		public float YOff 
		{
			get
			{
				return yOff;
			}
			set
			{
				yOff = value;
                UpdateSeedMapPerlinSpace();
			}
		}

	    public float ZOff { get; set; }

	    public float H 
		{
			get
			{
				return h;
			}
			set
			{
				h = value;
				preCalc();
			}
		}

		public float Lacunarity 
		{
			get
			{
				return lacunarity;
			}
			set
			{
				lacunarity = value;
				preCalc();
			}
		}

		public float Octaves 
		{
			get
			{
				return octaves;
			}
			set
			{
				octaves = value;
				preCalc();
			}
		}

        // This is the scaling of the perlin space to game world space.  Sets the number of game
        // world meters per unit of perlin space coordinates.
		public float MetersPerPerlinUnit 
		{
			get
			{
				return metersPerPerlinUnit;
			}
			set
			{
				metersPerPerlinUnit = value;
                UpdateSeedMapPerlinSpace();
			}
		}

        // This property is used to scale the output of the fractal to world coordinates
	    public float HeightScale { get; set; }

	    // add this value to the output of the fractal function.  Note that it is applied before
        // the heightScale.
	    public float HeightOffset { get; set; }

	    // this value is added to the output of the noise function, before doing any other fractal calculations
	    public float FractalOffset { get; set; }

	    // if height is below this value, then set it to this value
	    public float HeightFloor { get; set; }

	    // depricated.  should just leave at default value
	    public float AppScale { get; set; }

	    public float[] SeedMap { get; protected set; }

	    public int SeedMapWidth { get; protected set; }

	    public int SeedMapHeight { get; protected set; }

	    #endregion Properties

        // compute the table used to smooth the seed map values.  This smoothes out height transitions
        // in the seed map.
        static FractalTerrainGenerator()
        {
            interpTable = new float[257];

            for (int i = 1; i < 257; i++)
            {
                float f = i / 256f;

                interpTable[i] = (-2 * f * f * f) + (3 * f * f);
            }
        }

        public FractalTerrainGenerator()
        {
            HeightScale = 100f;
            FractalOffset = 0.7f;
            AppScale = 1.0f;

            Algorithm = GeneratorAlgorithm.HybridMultifractalWithFloor;
            preCalc();
            UpdateSeedMapPerlinSpace();
        }

        // pre-calculate a table of values for Pow(f, -h), to avoid calling the Pow function
        // for every iteration.
        protected void preCalc()
        {
            int o = (int)Math.Floor(octaves) + 1;

            lacPowH = new float[o];

            float f = 1;

            for (int i = 0; i < o; i++)
            {
                lacPowH[i] = (float)Math.Pow(f, -h);
                f *= lacunarity;
            }
        }

        private void UpdateSeedMapPerlinSpace()
        {
            seedMapPerlinUnitsPerSample = seedMapMetersPerSample / metersPerPerlinUnit;
            perlinSpaceSeedMapOrigin = new Vector3(seedMapOrigin.x / metersPerPerlinUnit + xOff,
                seedMapOrigin.z / metersPerPerlinUnit + yOff, 0);
        }

        // helper function to read a comma separated value text file (from excel) into a seed map
        // this is used by the MapTool
        public void LoadSeedMap(String filename)
        {
            StreamReader r = new StreamReader(filename);

            // first line is the number of values per line
            string line = r.ReadLine();
            SeedMapWidth = int.Parse(line);

            // second line is the number of lines
            line = r.ReadLine();
            SeedMapHeight = int.Parse(line);

            // allocate the array
            SeedMap = new float[SeedMapWidth * SeedMapHeight];

            int j = 0;
            while ((line = r.ReadLine()) != null)
            {
                for (int i = 0; i < SeedMapWidth; i++)
                {
                    // every other character is a value (rest are commas)
                    char c = line[i << 1];
                    int val = c - '0';
                    SeedMap[i + j * SeedMapWidth] = val / 9f;
                }
                j++;
            }

            r.Close();

            return;
        }


        // Look up an entry in the seedmap.  If the given coordinates are outside of the map, then provide
        // the "outside the map" height value.
        protected float SeedMapLookup(int x, int y)
        {
            float ret;
            if ( ( x < 0 ) || ( x >= SeedMapWidth ) ||
                ( y < 0 ) || ( y >= SeedMapHeight ) ) {
                ret = OutsideMapSeedHeight;
            }
            else
            {
                ret = SeedMap[x + y * SeedMapWidth];
            }

            return ret;
        }

        // use the seed map to generate a height value at the given perlin-space coordinates
        protected float SeedMapLookup(float x, float y, float z)
        {

            // convert from perlin space to seed map indices
            double xMapOff = x - perlinSpaceSeedMapOrigin.x;
            double yMapOff = y - perlinSpaceSeedMapOrigin.y;

            int xIndex = (int)Math.Floor(xMapOff / seedMapPerlinUnitsPerSample);
            int yIndex = (int)Math.Floor(yMapOff / seedMapPerlinUnitsPerSample);

            // look up the 4 seed map values surrounding the requested coordinates

            float nw = SeedMapLookup(xIndex, yIndex);
            float ne = SeedMapLookup(xIndex + 1, yIndex);
            float sw = SeedMapLookup(xIndex, yIndex + 1);
            float se = SeedMapLookup(xIndex + 1, yIndex + 1);

            // interpolate heights using smoothing table, to get the height at the requested coordinates
            double dx = ( xMapOff - (xIndex * seedMapPerlinUnitsPerSample) ) / seedMapPerlinUnitsPerSample;
            double dy = ( yMapOff - (yIndex * seedMapPerlinUnitsPerSample) ) / seedMapPerlinUnitsPerSample;
            if (dx < 0)
            {
                dx = 0;
            }
            if (dy < 0)
            {
                dy = 0;
            }

            if (dx > 1)
            {
                dx = 1;
            }
            if (dy > 1)
            {
                dy = 1;
            }

            double smoothedDx = interpTable[(int)Math.Floor(dx * 256)];
            double smoothedDy = interpTable[(int)Math.Floor(dy * 256)];
            double omdx = 1 - smoothedDx;
            double omdy = 1 - smoothedDy;

            float interp = (float)(omdx * omdy * nw + omdx * smoothedDy * sw + smoothedDx * omdy * ne + smoothedDx * smoothedDy * se);

            return interp;
        }

        public void SetSeedMap(float[] map, int width, int height)
        {
            SeedMap = map;
            SeedMapWidth = width;
            SeedMapHeight = height;
        }

        // the hybridMultifractal algorithm, but using the seed map lookup for the first octave
        protected float hybridMultifractalWithSeedMap(float x, float y, float z)
        {
            heightPointsGenerated++;

            double value = (SeedMapLookup(x, y, z) + FractalOffset) * lacPowH[0];
            // value = value * value;
            double weight = value;

            for (int i = 1; i < octaves; i++)
            {
                if (weight > 1)
                {
                    weight = 1;
                }
                double signal = (PerlinNoise.Noise(x, y, z) + FractalOffset) * lacPowH[i];
                value += weight * signal;
                weight *= signal;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;
            }

            value = (value + HeightOffset) * HeightScale;

            if (value < HeightFloor)
            {
                value = HeightFloor;
            }

            value = value * AppScale;

            return ((float)value);
        }

        // the hybridMultifractal algorithm
		protected float hybridMultifractal(float x, float y, float z) 
		{
		    int i;

			heightPointsGenerated++;

			double value = ( PerlinNoise.Noise(x, y, z) + FractalOffset ) * lacPowH[0];
			// value = value * value;
			double weight = value;

			for (i = 1; i < octaves; i++) 
			{
				if ( weight > 1 ) 
				{
					weight = 1;
				}
				double signal = ( PerlinNoise.Noise(x, y, z) + FractalOffset ) * lacPowH[i];
				value += weight * signal;
				weight *= signal;

				x *= lacunarity;
				y *= lacunarity;
				z *= lacunarity;
			}

			value = ( value + HeightOffset ) * HeightScale;

			if ( value < HeightFloor ) 
			{
				value = HeightFloor;
			}

			value = value * AppScale;

			return ((float)value);
		}

        // generate the height at a particular world location
		public float GenerateHeightPointMM(Vector3 worldLocationMM) 
		{
            return GenerateHeightPointMM(worldLocationMM.x / TerrainManager.oneMeter,
                                         worldLocationMM.z / TerrainManager.oneMeter);
		}

        public float GenerateHeightPointMM(float locXMeters, float locZMeters)
        {
            // convert given location to perlin space coordinates
            float perlinX = locXMeters / metersPerPerlinUnit + xOff;
            float perlinY = locZMeters / metersPerPerlinUnit + yOff;
            float perlinZ = ZOff;

            return fractalFunc(perlinX, perlinY, perlinZ) * TerrainManager.oneMeter;
        }

        // generate a rectangular height field
        public void GenerateHeightFieldMM(Vector3 worldLocationMM, int sizeMeters, int metersPerSample, float[] heightFieldMM, out float minHeightMM, out float maxHeightMM)
		{
            GenerateHeightFieldMM(worldLocationMM, sizeMeters, metersPerSample, heightFieldMM, out minHeightMM, out maxHeightMM, 0);
		}

		public void GenerateHeightFieldMM(Vector3 worldLocationMM, int sizeMeters, int metersPerSample, float[] heightFieldMM, out float minHeightMM, out float maxHeightMM, int sourceMetersPerSample)
		{
            int locX = (int)Math.Floor(worldLocationMM.x / TerrainManager.oneMeter);
            int locZ = (int)Math.Floor(worldLocationMM.z / TerrainManager.oneMeter);
            int heightFieldOff = 0;
            minHeightMM = float.MaxValue;
            maxHeightMM = float.MinValue;

            for (int z = 0; z < sizeMeters; z += metersPerSample)
            {
                for (int x = 0; x < sizeMeters; x += metersPerSample)
                {
                    float heightMM = heightFieldMM[heightFieldOff++] = 
                        GenerateHeightPointMM(locX + x, locZ + z);

                    if (heightMM > maxHeightMM)
                    {
                        maxHeightMM = heightMM;
                    }
                    if (heightMM < minHeightMM)
                    {
                        minHeightMM = heightMM;
                    }
                }
            }
		}

		public void ToJavascript(TextWriter w)
		{
		    string terrainString = ToXml();
			terrainString = terrainString.Replace("\"","\\\"");
			

			w.WriteLine("World.setTerrain(\"{0}\");", terrainString);
		}

		public string ToXml()
		{
			TextWriter s = new StringWriter();
			XmlWriter w = XmlWriter.Create(s);
            if (w == null)
            {
                throw new NullReferenceException();
            }

			ToXml(w);
            w.Close();
            s.Close();
			return s.ToString();
		}

        private void SeedMapToXml(XmlWriter w)
        {
            w.WriteStartElement("seedMap");
            w.WriteAttributeString("width", SeedMapWidth.ToString());
            w.WriteAttributeString("height", SeedMapHeight.ToString());
            w.WriteAttributeString("mapFormat", "base64");
            w.WriteAttributeString("bitsPerSample", bitsPerSample.ToString());
			int bytesPerSample = bitsPerSample >> 3;
			float multiplier = (float)Math.Pow(256d, bytesPerSample) - 1f;

            // generate byte array for base64 encoding
			byte [] byteMap = new byte[bytesPerSample * SeedMapWidth * SeedMapHeight];

            for ( int i = 0; i < ( SeedMapWidth * SeedMapHeight ); i++ ) {
				int byteOffset = i * bytesPerSample;
				int value = (int)Math.Round(SeedMap[i] * multiplier);
				for (int j=0; j<bytesPerSample; j++) {
					byteMap[byteOffset + j] = (byte)(value & 0xff);
					value = value >> 8;
				}
            }

            w.WriteBase64(byteMap, 0, bytesPerSample * SeedMapWidth * SeedMapHeight);
            w.WriteEndElement();
            return;
        }

        public void Save(bool force)
        {
            throw new NotImplementedException();
        }

        public void ToXml(StreamWriter s)
        {
            XmlWriter w = XmlWriter.Create(s);
            if (w == null)
            {
                throw new NullReferenceException();
            }

            ToXml(w);
            w.Close();
        }

        public void ToXml(XmlTextWriter w)
        {
            ToXml(w as XmlWriter);
        }

        public void ToXml(XmlWriter w) 
		{
			w.WriteStartElement("Terrain");

            w.WriteElementString("algorithm", AlgorithmNames[(int)algorithm]);
			w.WriteElementString("xOffset", xOff.ToString());
			w.WriteElementString("yOffset", yOff.ToString());
			w.WriteElementString("zOffset", ZOff.ToString());
			w.WriteElementString("h", h.ToString());
			w.WriteElementString("lacunarity", lacunarity.ToString());
			w.WriteElementString("octaves", octaves.ToString());
			w.WriteElementString("metersPerPerlinUnit", metersPerPerlinUnit.ToString());
			w.WriteElementString("heightScale", HeightScale.ToString());
			w.WriteElementString("heightOffset", HeightOffset.ToString());
			w.WriteElementString("fractalOffset", FractalOffset.ToString());
			w.WriteElementString("heightFloor", HeightFloor.ToString());

            switch (Algorithm)
            {
                case GeneratorAlgorithm.HybridMultifractalWithSeedMap:
                    w.WriteElementString("seedMapOriginX", seedMapOrigin.x.ToString());
                    w.WriteElementString("seedMapOriginY", seedMapOrigin.y.ToString());
                    w.WriteElementString("seedMapOriginZ", seedMapOrigin.z.ToString());
                    w.WriteElementString("seedMapMetersPerSample", seedMapMetersPerSample.ToString());
                    w.WriteElementString("outsideSeedMapHeight", OutsideMapSeedHeight.ToString());
                    SeedMapToXml(w);
                    break;
                default:
                    break;
            }

			w.WriteEndElement(); // Terrain

			return;
		}

        protected void ParseSeedMap(string format, string value)
        {
            switch (format)
            {
                case "digitString":
                    Debug.Assert(value.Length == (SeedMapWidth * SeedMapHeight), "ParseSeedMap: value is wrong length");

                    SeedMap = new float[SeedMapWidth * SeedMapHeight];
                    int i = 0;

                    for (int y = 0; y < SeedMapHeight; y++)
                    {
                        for (int x = 0; x < SeedMapWidth; x++)
                        {
                            char c = value[i++];
                            int val = c - '0';
                            SeedMap[x + y * SeedMapWidth] = val / 9f;
                        }
                    }
                    break;
                case "base64":
                    break;
            }
        }
        

        protected void ParseTerrainElement(XmlReader r)
		{
			// save the name of the element
			string name = r.Name;

            String mapFormat = null;
            string val;

            if (name == "seedMap")
            { // parse seedMap attributes
                for (int i = 0; i < r.AttributeCount; i++)
                {
                    r.MoveToAttribute(i);

                    // set the field in this object based on the element we just read
                    switch (r.Name)
                    {
                        case "width":
                            SeedMapWidth = int.Parse(r.Value);
                            break;
                        case "height":
                            SeedMapHeight = int.Parse(r.Value);
                            break;
                        case "mapFormat":
                            mapFormat = r.Value;
                            break;
                        case "bitsPerSample":
                            bitsPerSample = int.Parse(r.Value);
                            // deal with old terrain xml, which set bitsPerSample to 256 for 8-bit samples
                            if (bitsPerSample == 256)
                            {
                                bitsPerSample = 8;
                            }
                            break;
                    }
                }
                r.MoveToElement(); //Moves the reader back to the element node.

                SeedMap = new float[SeedMapWidth * SeedMapHeight];

                switch (mapFormat)
                {
                    case "digitString":
                        // read the value
                        r.Read();
                        if (r.NodeType != XmlNodeType.Text)
                        {
                            return;
                        }
                        val = r.Value;

                        Debug.Assert(val.Length == (SeedMapWidth * SeedMapHeight), "ParseSeedMap: value is wrong length");

                        for (int i = 0; i < (SeedMapWidth * SeedMapHeight); i++)
                        {
                            SeedMap[i] = (val[i] - '0') / 9f;
                        }

                        // error out if we dont see an end element here
                        r.Read();
                        if (r.NodeType != XmlNodeType.EndElement)
                        {
                            return;
                        }
                        break;
                    case "base64":
                        
						int bytesPerSample = bitsPerSample >> 3;
						int byteCount = bytesPerSample * SeedMapWidth * SeedMapHeight;
						float divisor = 1.0f / ((float)Math.Pow(256d, bytesPerSample) - 1f);
						byte [] byteMap = new byte[byteCount];
                        int bytesRead = r.ReadElementContentAsBase64(byteMap, 0, byteCount);

                        Debug.Assert(bytesRead == (byteCount));

                        for (int i = 0; i < (SeedMapWidth * SeedMapHeight); i++)
                        {
							int value = 0;
							int byteOffset = i * bytesPerSample;
							for (int j=0; j<bytesPerSample; j++)
								value |= (byteMap[byteOffset + j] << (j * 8));
                            SeedMap[i] = value * divisor;
                        }

                        // dont read the end element, because the ReadBase64 appears to consume it
                        break;
                }

            }
            else
            {

                // read the value
                r.Read();
                if (r.NodeType != XmlNodeType.Text)
                {
                    return;
                }
                val = r.Value;

                // error out if we dont see an end element here
                r.Read();
                if (r.NodeType != XmlNodeType.EndElement)
                {
                    return;
                }

                // set the field in this object based on the element we just read
                switch (name)
                {
                    case "algorithm":
                        for (int i = 0; i < AlgorithmNames.Length; i++)
                        {
                            if (AlgorithmNames[i] == val)
                            {
                                Algorithm = (GeneratorAlgorithm)i;
                                break;
                            }
                        }
                        break;
                    case "xOffset":
                        XOff = float.Parse(val);
                        break;
                    case "yOffset":
                        YOff = float.Parse(val);
                        break;
                    case "zOffset":
                        ZOff = float.Parse(val);
                        break;
                    case "h":
                        H = float.Parse(val);
                        break;
                    case "lacunarity":
                        Lacunarity = float.Parse(val);
                        break;
                    case "octaves":
                        Octaves = float.Parse(val);
                        break;
                    case "metersPerPerlinUnit":
                        MetersPerPerlinUnit = float.Parse(val);
                        break;
                    case "heightScale":
                        HeightScale = float.Parse(val);
                        break;
                    case "heightOffset":
                        HeightOffset = float.Parse(val);
                        break;
                    case "fractalOffset":
                        FractalOffset = float.Parse(val);
                        break;
                    case "heightFloor":
                        HeightFloor = float.Parse(val);
                        break;
                    case "seedMapOriginX":
                        SeedMapOriginX = float.Parse(val);
                        break;
                    case "seedMapOriginY":
                        SeedMapOriginY = float.Parse(val);
                        break;
                    case "seedMapOriginZ":
                        SeedMapOriginZ = float.Parse(val);
                        break;
                    case "seedMapMetersPerSample":
                        SeedMapMetersPerSample = float.Parse(val);
                        break;
                    case "outsideSeedMapHeight":
                        OutsideMapSeedHeight = float.Parse(val);
                        break;
                }
            }
		}

		protected void ParseTerrain(XmlReader r)
		{
			while ( r.Read() ) 
			{
				// look for the start of an element
				if ( r.NodeType == XmlNodeType.Element ) 
				{
					// parse that element
					ParseTerrainElement(r);
				} 
				else if ( r.NodeType == XmlNodeType.EndElement ) 
				{
					// if we found an end element, it means we are at the end of the terrain description
					return;
				}
			}

			return;
		}

		public void FromXML(XmlTextReader r)
        {
            FromXML(r as XmlReader);
        }

        public void FromXML(XmlReader r)
		{
            // clear out the seedmap first, so that if we are loading a terrain without
            // one, we don't have the old one left over.
            SeedMap = null;
            SeedMapHeight = 0;
            SeedMapWidth = 0;

			// we found the terrain description, now parse it
			ParseTerrain(r);
		}

		public int HeightPointsGenerated 
		{
			get 
			{
				return heightPointsGenerated;
			}
		}

		public int BitsPerSample 
		{
			get 
			{
				return bitsPerSample;
			}
			set
			{
				bitsPerSample = value;
			}
			
		}

        // text names of the generator algorithms
        // must match the GeneratorAlgorithm enum
        public static readonly String[] AlgorithmNames = { 
            "HybridMultifractalWithFloor",
            "HybridMultifractalWithSeedMap"
        };

        // Devnote: This is never actually used because the fractal isn't editable
        // This code is only provided to cleanly remove the compilation warning
        // for not using the TerrainModificationStateChanged event.
        internal void FireTerrainModificationStateChanged()
        {
            if (TerrainModificationStateChanged != null)
            {
                TerrainModificationStateChanged(this, Modified); // This is never really fired
            }
        }

        // Devnote: This is never actually used because the fractal isn't editable
        // This code is only provided to cleanly remove the compilation warning
        // for not using the TerrainModificationStateChanged event.
        internal void FireTerrainChanged()
        {
            if (TerrainChanged != null)
            {
                TerrainChanged(this, 0, 0, 0, 0); // This is never really fired
            }
        }
	}

    public enum GeneratorAlgorithm
    {
        HybridMultifractalWithFloor,
        HybridMultifractalWithSeedMap
    }
}
