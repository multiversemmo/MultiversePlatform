#region Using directives

using System;
using System.Text;

#endregion

namespace Multiverse.Generator
{

    public class PerlinNoise
    {
        static public float Noise(float x, float y, float z)
        {
            int oneFixed = 1 << 16;
            int X = (int)(x * oneFixed);
            int Y = (int)(y * oneFixed);
            int Z = (int)(z * oneFixed);

            int ret = Noise(X, Y, Z);

            return (((float)ret) / ((float)oneFixed));
        }
        static public int Noise(int x, int y, int z)
        {
            // coordinates of unit cube that contains the point
            int X = x >> 16 & 255;
            int Y = y >> 16 & 255;
            int Z = z >> 16 & 255;

            // fixed point 1.0
            int N = 1 << 16;

            // compute offsets of point within cube
            x &= N - 1;
            y &= N - 1;
            z &= N - 1;

            // compute fade curves for x, y and z
            int u = fade(x);
            int v = fade(y);
            int w = fade(z);

            int A = p[X] + Y,
                AA = p[A] + Z,
                AB = p[A + 1] + Z,
                B = p[X + 1] + Y,
                BA = p[B] + Z,
                BB = p[B + 1] + Z;
            return lerp(w, lerp(v, lerp(u, grad(p[AA], x, y, z),
                                           grad(p[BA], x - N, y, z)),
                                   lerp(u, grad(p[AB], x, y - N, z),
                                           grad(p[BB], x - N, y - N, z))),
                           lerp(v, lerp(u, grad(p[AA + 1], x, y, z - N),
                                           grad(p[BA + 1], x - N, y, z - N)),
                                   lerp(u, grad(p[AB + 1], x, y - N, z - N),
                                           grad(p[BB + 1], x - N, y - N, z - N))));
        }

        // linear interpolation
        static int lerp(int t, int a, int b)
        {
            return a + (t * (b - a) >> 12);
        }
        static int grad(int hash, int x, int y, int z)
        {
            int h = hash & 15;
            int u = h < 8 ? x : y;
            int v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        static int[] p = { 
            151,160,137,91 ,90 ,15 ,131,13 ,201,95 ,96 ,53 ,194,233,7  ,225,
            140,36 ,103,30 ,69 ,142,8  ,99 ,37 ,240,21 ,10 ,23 ,190,6  ,148,
            247,120,234,75 ,0  ,26 ,197,62 ,94 ,252,219,203,117,35 ,11 ,32 ,
            57 ,177,33 ,88 ,237,149,56 ,87 ,174,20 ,125,136,171,168,68 ,175,
            74 ,165,71 ,134,139,48 ,27 ,166,77 ,146,158,231,83 ,111,229,122,
            60 ,211,133,230,220,105,92 ,41 ,55 ,46 ,245,40 ,244,102,143,54 ,
            65 ,25 ,63 ,161,1  ,216,80 ,73 ,209,76 ,132,187,208,89 ,18 ,169,
            200,196,135,130,116,188,159,86 ,164,100,109,198,173,186,3  ,64 ,
            52 ,217,226,250,124,123,5  ,202,38 ,147,118,126,255,82 ,85 ,212,
            207,206,59 ,227,47 ,16 ,58 ,17 ,182,189,28 ,42 ,223,183,170,213,
            119,248,152,2  ,44 ,154,163,70 ,221,153,101,155,167,43 ,172,9  ,
            129,22 ,39 ,253,19 ,98 ,108,110,79 ,113,224,232,178,185,112,104,
            218,246,97 ,228,251,34 ,242,193,238,210,144,12 ,191,179,162,241,
            81 ,51 ,145,235,249,14 ,239,107,49 ,192,214,31 ,181,199,106,157,
            184,84 ,204,176,115,121,50 ,45 ,127,4  ,150,254,138,236,205,93 ,
            222,114,67 ,29 ,24 ,72 ,243,141,128,195,78 ,66 ,215,61 ,156,180,
            151,160,137,91 ,90 ,15 ,131,13 ,201,95 ,96 ,53 ,194,233,7  ,225,
            140,36 ,103,30 ,69 ,142,8  ,99 ,37 ,240,21 ,10 ,23 ,190,6  ,148,
            247,120,234,75 ,0  ,26 ,197,62 ,94 ,252,219,203,117,35 ,11 ,32 ,
            57 ,177,33 ,88 ,237,149,56 ,87 ,174,20 ,125,136,171,168,68 ,175,
            74 ,165,71 ,134,139,48 ,27 ,166,77 ,146,158,231,83 ,111,229,122,
            60 ,211,133,230,220,105,92 ,41 ,55 ,46 ,245,40 ,244,102,143,54 ,
            65 ,25 ,63 ,161,1  ,216,80 ,73 ,209,76 ,132,187,208,89 ,18 ,169,
            200,196,135,130,116,188,159,86 ,164,100,109,198,173,186,3  ,64 ,
            52 ,217,226,250,124,123,5  ,202,38 ,147,118,126,255,82 ,85 ,212,
            207,206,59 ,227,47 ,16 ,58 ,17 ,182,189,28 ,42 ,223,183,170,213,
            119,248,152,2  ,44 ,154,163,70 ,221,153,101,155,167,43 ,172,9  ,
            129,22 ,39 ,253,19 ,98 ,108,110,79 ,113,224,232,178,185,112,104,
            218,246,97 ,228,251,34 ,242,193,238,210,144,12 ,191,179,162,241,
            81 ,51 ,145,235,249,14 ,239,107,49 ,192,214,31 ,181,199,106,157,
            184,84 ,204,176,115,121,50 ,45 ,127,4  ,150,254,138,236,205,93 ,
            222,114,67 ,29 ,24 ,72 ,243,141,128,195,78 ,66 ,215,61 ,156,180,
        };
        static int[] fadeTable = new int[256];
        static int fade(int t)
        {
            int tHigh = t >> 8;
            int t0 = fadeTable[tHigh];
            int t1 = fadeTable[Math.Min(255, tHigh + 1)];
            return t0 + ((t & 255) * (t1 - t0) >> 8);
        }

        static double fadeFunc(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        // static constructor for the class
        // currently computes the lookup table used by the fade function.
        static PerlinNoise() {
            for (int i = 0; i < 256; i++) {
                fadeTable[i] = (int)((1 << 12) * fadeFunc(i / 256.0f));
            }
        }
    }
}
