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
using System.IO;
using System.Diagnostics;

namespace Axiom.SceneManagers.Multiverse
{
    public class MosaicDescription
    {
        public string MosaicName { get; private set; }
        public string MosaicType { get; private set; }
        public string FileExt { get; private set; }
        public int SizeXPixels { get; private set; }
        public int SizeZPixels { get; private set; }
        public int SizeXTiles { get; private set; }
        public int SizeZTiles { get; private set; }
        public int TileSizeSamples { get; private set; }
        public int MetersPerSample { get; private set; }
        public int MPSShift { get; private set; }
        public int MPSMask { get; private set; }
        public bool WrapFlag { get; private set; }
        public bool UnifiedScale { get; private set; }
        public float GlobalMaxHeightMeters { get; private set; }
        public float GlobalMinHeightMeters { get; private set; }

        public bool Modified { get; private set; }

        public string DefaultTerrainSaveDirectory { get; set;}

        private bool[,] availableTiles;

        public MosaicDescription(string newName, MosaicDescription src) :
            this(newName, src.MosaicType, src.DefaultTerrainSaveDirectory, src.FileExt, 
                 src.SizeXPixels, src.SizeZPixels, src.SizeXTiles, src.SizeZTiles, src.TileSizeSamples,
                 src.MetersPerSample, src.WrapFlag, src.UnifiedScale, 
                 src.GlobalMinHeightMeters, src.GlobalMaxHeightMeters,
                 0, false)
        {
        }

        public MosaicDescription(string mosaicName, string mosaicType, string defaultTerrainSaveDirectory, string fileExt, int sizeXPixels, int sizeZPixels, int sizeXTiles, int sizeZTiles, int tileSizeSamples, int metersPerSample, bool wrapFlag, bool unifiedScale, float globalMinHeightMeters, float globalMaxHeightMeters, int tileNum, bool tileOK)
        {
            Modified = true; // We're creating a brand new mosaic

            MosaicName = mosaicName;
            MosaicType = mosaicType;
            DefaultTerrainSaveDirectory = defaultTerrainSaveDirectory;
            FileExt = fileExt;
            SizeXPixels = sizeXPixels;
            SizeZPixels = sizeZPixels;
            SizeXTiles = sizeXTiles;
            SizeZTiles = sizeZTiles;
            TileSizeSamples = tileSizeSamples;
            MetersPerSample = metersPerSample;
            WrapFlag = wrapFlag;
            UnifiedScale = unifiedScale;
            GlobalMinHeightMeters = globalMinHeightMeters;
            GlobalMaxHeightMeters = globalMaxHeightMeters;

            InitMpsStuff();
            InitTilesState(tileNum, tileOK);

            // Make sure all tiles are available
            for (int z=0; z< sizeZTiles; z++)
            {
                for (int x = 0; x < sizeXTiles; x++)
                {
                    availableTiles[x, z] = true;
                }   
            }
        }

        public MosaicDescription(Stream s)
        {
            Modified = false; // We're recreating an existing mosaic

            StreamReader r = new StreamReader(s);
            string line;
            //todo: get file location of the mosaic!  update DefaultTerrainSaveDirectory with it

            while ((line = r.ReadLine()) != "#EOF")
            {
                if (line[0] == '#')
                {
                    string label;
                    string val1;
                    string val2;

                    ParseLine(line, out label, out val1, out val2);

                    switch (label)
                    {
                        case "MosaicName":
                            MosaicName = val1;
                            break;
                        case "MosaicType":
                            MosaicType = val1;
                            break;
                        case "FileExt":
                            FileExt = val1;
                            break;
                        case "nPxlsX":
                            SizeXPixels = int.Parse(val1);
                            break;
                        case "nPxlsY":
                            SizeZPixels = int.Parse(val1);
                            break;
                        case "nMapsX":
                            SizeXTiles = int.Parse(val1);
                            break;
                        case "nMapsY":
                            SizeZTiles = int.Parse(val1);
                            break;
                        case "SubMapSize":
                            TileSizeSamples = int.Parse(val1);
                            break;
                        case "HorizScale":
                            float horizScale = float.Parse(val1);

                            // meters per sample should be in int
                            MetersPerSample = (int)Math.Round(horizScale);
                            InitMpsStuff();
                            break;
                        case "WrapFlag":
                            WrapFlag = (val1 == "TRUE");
                            break;
                        case "TileState":
                            int tileNum = int.Parse(val1);
                            bool tileOK = (val2 == "OK");

                            InitTilesState(tileNum, tileOK);
                            break;
                        case "UnifiedScale":
                            UnifiedScale = (val1 == "TRUE");
                            break;
                        case "GlobalMinAlt":
                            GlobalMinHeightMeters = float.Parse(val1);
                            break;
                        case "GlobalMaxAlt":
                            GlobalMaxHeightMeters = float.Parse(val1);
                            break;
                    }
                }
            }
            r.Close();
        }

        public void Save(bool force)
        {
            if (!force && !Modified)
            {
                return;
            }

            string mmfFile = Path.Combine(DefaultTerrainSaveDirectory, MosaicName + ".mmf");
            if (File.Exists(mmfFile))
            {
                File.Delete(mmfFile);
            }
            Save(new StreamWriter(mmfFile));
        }

        private void Save(TextWriter writer)
        {
            // Header
            writer.WriteLine("L3DT Mosaic master file");

            // Properties
            WriteLine(writer, "MosaicName", MosaicName);
            WriteLine(writer, "MosaicType", MosaicType);
            WriteLine(writer, "FileExt", FileExt);
            WriteLine(writer, "nPxlsX", SizeXPixels);
            WriteLine(writer, "nPxlsY", SizeZPixels);
            WriteLine(writer, "nMapsX", SizeXTiles);
            WriteLine(writer, "nMapsY", SizeZTiles);
            WriteLine(writer, "SubMapSize", TileSizeSamples);
            WriteLine(writer, "HorizScale", MetersPerSample);
            WriteLine(writer, "WrapFlag", WrapFlag ? "TRUE" : "FALSE");
            WriteLine(writer, "UnifiedScale", UnifiedScale ? "TRUE" : "FALSE");
            WriteLine(writer, "GlobalMinAlt", GlobalMinHeightMeters);
            WriteLine(writer, "GlobalMaxAlt", GlobalMaxHeightMeters);

            // Tile states
            for (int i = 0; i < SizeXTiles * SizeZTiles; i++)
            {
                WriteLine(writer, "TileState", "" + i, "OK");
            }

            // Trailer
            writer.WriteLine("#EOF");
            writer.Close();
        }

        private static void ParseLine(string line, out string label, out string val1, out string val2)
        {
            int labelend = line.IndexOf(':');

            label = line.Substring(1, labelend - 1);

            int firstTab = line.IndexOf('\t');
            int lastTab = line.LastIndexOf('\t');

            if (firstTab == lastTab)
            {
                val1 = line.Substring(firstTab + 1);
                val2 = null;
            }
            else
            {
                val1 = line.Substring(firstTab + 1, lastTab - firstTab - 1);
                val2 = line.Substring(lastTab + 1);
            }

            return;
        }

        private static void WriteLine(TextWriter writer, string label, object val1) {
            writer.WriteLine("#" + label + ":\t" + val1);
        }

        private static void WriteLine(TextWriter writer, string label, object val1, object val2)
        {
            writer.WriteLine("#" + label + ":\t" + val1 + "\t" + val2);
        }

        private void InitMpsStuff()
        {
            // compute mask
            MPSMask = MetersPerSample - 1;

            // compute shift
            MPSShift = 0;
            int tmp = MPSMask;
            while (tmp > 0)
            {
                tmp = tmp >> 1;
                MPSShift++;
            }

            if ((1 << MPSShift) != MetersPerSample)
            {
                throw new Core.AxiomException("The HorizScale parameter in the mosaic is not a power of 2.");
            }
            Debug.Assert((1 << MPSShift) == MetersPerSample);
        }

        private void InitTilesState(int tileNum, bool tileOK)
        {
            if (availableTiles == null)
            {
                availableTiles = new bool[SizeXTiles, SizeZTiles];
            }

            int y = tileNum / SizeXTiles;
            int x = tileNum - (y * SizeXTiles);

            y = SizeZTiles - y - 1;

            availableTiles[x, y] = tileOK;
        }

        public bool TileAvailable(int x, int y)
        {
            return availableTiles[x,y];
        }
    }
}
