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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

#endregion

#region Copyright LGPL
/*
	The Universe Development Kit
	Copyright (C) 2000  Sean O'Neil
	soneil@home.com

	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
#endregion

namespace Multiverse.Tools
{

    public class NoiseHelper {
        public static float Square(float a) {
		    return a * a;
        }
        public static float Clamp(float a, float b, float x) {
		    return (x < a ? a : (x > b ? b : x));
		}
        /// <summary>
        ///   A linear interpolation function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float x) {
		    return a + x * (b - a);
		}
        /// <summary>
        ///   A cubic interpolation function (3 a^2 - 2 a^3)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Cubic(float a) {
            return a * a * (3 - 2*a);
        }
#if NOT_DEFINED

        const float LOGHALF         = -0.6931471805599f;	// log(0.5)
        const float LOGHALFI		= -1.442695040889f;		// Inverse of log(0.5)

        public static float Step(float a, float x) {
            return (float)(x >= a);
        }
        public static float Boxstep(float a, float b, float x) {
            return Clamp(0, 1, (x-a)/(b-a));
        }
        public static float Pulse(float a, float b, float x) {
            return (float)((x >= a) - (x >= b));
        }
        public static float Gamma(float a, float g) {
            return Math.Pow(a, 1/g);
        }
        public static float Bias(float a, float b) {
            return Math.Pow(a, Math.Log(b) * LOGHALFI);
        }
        public static float Expose(float l, float k) {
            return (1 - Math.Exp(-l * k));
        }
        public static float Gain(float a, float b) {
	        if(a <= DELTA)
		        return 0;
        	if(a >= 1-DELTA)
        		return 1;

	        float p = (Math.Log(1 - b) * LOGHALFI);
	        if(a < 0.5)
		        return Math.Pow(2 * a, p) * 0.5f;
	        else
		        return 1 - Math.Pow(2 * (1 - a), p) * 0.5f;
        }
        public static float Smoothstep(float a, float b, float x) {
	        if(x <= a)
		        return 0;
	        if(x >= b)
		        return 1;
	        return Cubic((x - a) / (b - a));
        }
        public static float Mod(float a, float b) {
            a -= ((int)(a / b)) * b;
	        if(a < 0)
		        a += b;
	        return a;
        }
#endif
        public static void Normalize(ref float[,] f, int i, int n) {
	        float fMagnitude = 0;
	        for (int j = 0; j < n; j++)
                fMagnitude += f[i, j] * f[i, j];
            fMagnitude = 1 / (float)Math.Sqrt(fMagnitude);
            for (int j = 0; j < n; j++)
                f[i, j] *= fMagnitude;
        }
	}

    /*******************************************************************************
    * Class: CNoise
    ********************************************************************************
    * This class implements the Perlin noise function. Initialize it with the number
    * of dimensions (1 to 4) and a random seed. I got the source for the first 3
    * dimensions from "Texturing & Modeling: A Procedural Approach". I added the
    * extra dimension because it may be desirable to use 3 spatial dimensions and
    * one time dimension. The noise buffers are set up as member variables so that
    * there may be several instances of this class in use at the same time, each
    * initialized with different parameters.
    *******************************************************************************/
    public class Noise
    {
        protected Random r;
        protected const int MaxDimensions    =	4;					// Maximum number of dimensions in a noise object
        protected const int MaxOctaves	    = 128;					// Maximum # of octaves in an fBm object

	    protected int m_nDimensions;						// Number of dimensions used by this object
	    protected byte[] m_nMap = new byte[256];            // Randomized map of indexes into buffer
	    protected float[,] m_nBuffer = new float[256,MaxDimensions];	// Random n-dimensional buffer

        protected float Lattice(int ix, float fx) {
            return Lattice(ix, fx, 0, 0f, 0, 0f, 0, 0f);
        }
        protected float Lattice(int ix, float fx, int iy, float fy) {
            return Lattice(ix, fx, iy, fy, 0, 0f, 0, 0f);
        }
        protected float Lattice(int ix, float fx, int iy, float fy, int iz, float fz) {
            return Lattice(ix, fx, iy, fy, iz, fz, 0, 0f);
        }
	    protected float Lattice(int ix, float fx, int iy, float fy, int iz, float fz, int iw, float fw) {
		    int[] n = {ix, iy, iz, iw};
		    float[] f = {fx, fy, fz, fw};
		    int nIndex = 0;
		    for(int i=0; i<m_nDimensions; i++)
			    nIndex = m_nMap[(nIndex + n[i]) & 0xFF];
		    float fValue = 0;
		    for(int i=0; i<m_nDimensions; i++)
			    fValue += m_nBuffer[nIndex,i] * f[i];
		    return fValue;
	    }

	    public Noise() {
        }

	    public Noise(int nDimensions, int nSeed) {
            Init(nDimensions, nSeed);
        }

	    public void Init(int nDimensions, int nSeed) {
            r = new Random(nSeed);
            if (nDimensions > MaxDimensions)
                throw new Exception("Invalid number of dimensions exceeds maximum");
            m_nDimensions = nDimensions;

	        for (int i = 0; i < 256; i++) {
        		m_nMap[i] = (byte)i;
				for (int j = 0; j < m_nDimensions; j++)
					m_nBuffer[i, j] = (float)(r.NextDouble() - 0.5f);
				NoiseHelper.Normalize(ref m_nBuffer, i, m_nDimensions);
	        }

	        for(int i = 0; i < 256; ++i) {
                // Build the permutation map
        		int j = r.Next() % 256;
                byte k = m_nMap[i];
                m_nMap[i] = m_nMap[j];
                m_nMap[j] = (byte)k;
	        }
        }

        private static float Lerp(float a, float b, float x) {
            return NoiseHelper.Lerp(a, b, x);
        }

		/// <summary>
		///   Get the noise function at a given position specified by f
		/// </summary>
		/// <param name="f">A position in space (dimension 1-4)</param>
		/// <returns>Amplitude of noise from this pass at the given location.</returns>
		public float GetNoise(float[] f) {
          	int[] n = new int[MaxDimensions];			// Indexes to pass to lattice function
	        float[] r = new float[MaxDimensions];		// Remainders to pass to lattice function
	        float[] w = new float[MaxDimensions];		// Cubic values to pass to interpolation function

	        for(int i=0; i<m_nDimensions; i++) {
		        n[i] = (int)Math.Floor(f[i]);
		        r[i] = f[i] - n[i];
		        w[i] = NoiseHelper.Cubic(r[i]);
	        }

            float fValue;
            switch(m_nDimensions)
	        {
		        case 1:
			        fValue = Lerp(Lattice(n[0], r[0]),
						          Lattice(n[0]+1, r[0]-1),
						          w[0]);
			        break;
		        case 2:
			        fValue = Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1]),
							           Lattice(n[0]+1, r[0]-1, n[1], r[1]),
							           w[0]),
						          Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1),
							           Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1),
							           w[0]),
						          w[1]);
			        break;
		        case 3:
			        fValue = Lerp(Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1], n[2], r[2]),
									        Lattice(n[0]+1, r[0]-1, n[1], r[1], n[2], r[2]),
									        w[0]),
							        Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1, n[2], r[2]),
									        Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1, n[2], r[2]),
									        w[0]),
							        w[1]),
						          Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1], n[2]+1, r[2]-1),
									        Lattice(n[0]+1, r[0]-1, n[1], r[1], n[2]+1, r[2]-1),
									        w[0]),
							           Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1, n[2]+1, r[2]-1),
									        Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1, n[2]+1, r[2]-1),
									        w[0]),
							           w[1]),
						          w[2]);
			        break;
		        case 4:
			        fValue = Lerp(Lerp(Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1], n[2], r[2], n[3], r[3]),
										         Lattice(n[0]+1, r[0]-1, n[1], r[1], n[2], r[2], n[3], r[3]),
										         w[0]),
									        Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1, n[2], r[2], n[3], r[3]),
										         Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1, n[2], r[2], n[3], r[3]),
										         w[0]),
									        w[1]),
									   Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1], n[2]+1, r[2]-1, n[3], r[3]),
										         Lattice(n[0]+1, r[0]-1, n[1], r[1], n[2]+1, r[2]-1, n[3], r[3]),
										         w[0]),
									        Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1, n[2]+1, r[2]-1),
										         Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1, n[2]+1, r[2]-1, n[3], r[3]),
										         w[0]),
									        w[1]),
							           w[2]),
						          Lerp(Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1], n[2], r[2], n[3]+1, r[3]-1),
										         Lattice(n[0]+1, r[0]-1, n[1], r[1], n[2], r[2], n[3]+1, r[3]-1),
										         w[0]),
									        Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1, n[2], r[2], n[3]+1, r[3]-1),
										         Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1, n[2], r[2], n[3]+1, r[3]-1),
										         w[0]),
									        w[1]),
									   Lerp(Lerp(Lattice(n[0], r[0], n[1], r[1], n[2]+1, r[2]-1, n[3]+1, r[3]-1),
										         Lattice(n[0]+1, r[0]-1, n[1], r[1], n[2]+1, r[2]-1, n[3]+1, r[3]-1),
										         w[0]),
									        Lerp(Lattice(n[0], r[0], n[1]+1, r[1]-1, n[2]+1, r[2]-1),
										         Lattice(n[0]+1, r[0]-1, n[1]+1, r[1]-1, n[2]+1, r[2]-1, n[3]+1, r[3]-1),
										         w[0]),
									        w[1]),
							           w[2]),
						          w[3]);
			        break;
                default:
                    throw new Exception("Invalid value for dimension");
            }
	        return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }
    }

    /*******************************************************************************
    * Class: CFractal
    ********************************************************************************
    * This class implements fBm, or fractal Brownian motion. Since fBm uses Perlin
    * noise, this class is derived from CNoise. Initialize it with the number of
    * dimensions (1 to 4), a random seed, H (roughness ranging from 0 to 1), and
    * the lacunarity (2.0 is often used). Many of the fractal routines came from
    * "Texturing & Modeling: A Procedural Approach". fBmTest() is my own creation,
    * and I created it to generate my first planet.
    *******************************************************************************/
    class Fractal : Noise {
	    protected float m_fH;
	    protected float m_fLacunarity;
	    protected float[] m_fExponent = new float[MaxOctaves];

        public Fractal() {
        }

	    public Fractal(int nDimensions, int nSeed, float fH, float fLacunarity) {
		    Init(nDimensions, nSeed, fH, fLacunarity);
	    }

	    public void Init(int nDimensions, int nSeed, float fH, float fLacunarity) {
		    base.Init(nDimensions, nSeed);
		    m_fH = fH;
		    m_fLacunarity = fLacunarity;
		    float f = 1;
		    for (int i = 0; i < MaxOctaves; i++) {
			    m_fExponent[i] = (float)Math.Pow(f, -m_fH);
			    f *= m_fLacunarity;
		    }
	    }

	    public float fBm(float[] f, float fOctaves) {
  	        // Initialize locals
	        float fValue = 0;
	        float[] fTemp = new float[MaxDimensions];
	        for (int i=0; i<m_nDimensions; i++)
		        fTemp[i] = f[i];

	        // Inner loop of spectral construction, where the fractal is built
	        for (int i=0; i<fOctaves; i++) {
		        fValue += GetNoise(fTemp) * m_fExponent[i];
		        for(int j=0; j<m_nDimensions; j++)
			        fTemp[j] *= m_fLacunarity;
	        }

	        return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }

        public float fBmTest(float[] f, float fOctaves) {
            // Initialize locals
            float fValue = 0;
            float[] fTemp = new float[MaxDimensions];
            for (int i = 0; i < m_nDimensions; i++)
                fTemp[i] = f[i] * 2;

            //fOctaves *= Math.Abs(GetNoise(fTemp)) + 1.0f;
            //fOctaves = NoiseHelper.Clamp(2, 16, fOctaves);

            // Inner loop of spectral construction, where the fractal is built
            for (int i = 0; i < fOctaves; i++) {
                fValue += GetNoise(fTemp) * m_fExponent[i];
                for (int j = 0; j < m_nDimensions; j++)
                    fTemp[j] *= m_fLacunarity;
            }

            if (fValue <= 0.0f)
                fValue = (float)-Math.Pow(-fValue, 0.7f);
            else
                fValue = (float)Math.Pow(fValue, 1 + GetNoise(fTemp) * fValue);
            
            return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }

        public float Turbulence(float[] f, float fOctaves) {
           	// Initialize locals
	        float fValue = 0;
            float[] fTemp = new float[MaxDimensions];
            for (int i = 0; i < m_nDimensions; i++)
		        fTemp[i] = f[i];

	        // Inner loop of spectral construction, where the fractal is built
	        for (int i = 0; i < fOctaves; i++) {
		        fValue += Math.Abs(GetNoise(fTemp)) * m_fExponent[i];
		        for(int j=0; j < m_nDimensions; j++)
			        fTemp[j] *= m_fLacunarity;
	        }
            
	        return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }

        public float Multifractal(float[] f, float fOctaves, float fOffset) {
           	// Initialize locals
	        float fValue = 1;
            float[] fTemp = new float[MaxDimensions];
            for(int i=0; i<m_nDimensions; i++)
		        fTemp[i] = f[i];

	        // Inner loop of spectral construction, where the fractal is built
	        for(int i=0; i<fOctaves; i++) {
		        fValue *= GetNoise(fTemp) * m_fExponent[i] + fOffset;
		        for(int j=0; j<m_nDimensions; j++)
			        fTemp[j] *= m_fLacunarity;
	        }

            return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }

        public float Heterofractal(float[] f, float fOctaves, float fOffset) {
           	// Initialize locals
	        float fValue = GetNoise(f) + fOffset;
            float[] fTemp = new float[MaxDimensions];
            for(int i=0; i<m_nDimensions; i++)
		        fTemp[i] = f[i] * m_fLacunarity;

	        // Inner loop of spectral construction, where the fractal is built
	        for(int i=1; i<fOctaves; i++) {
		        fValue += (GetNoise(fTemp) + fOffset) * m_fExponent[i] * fValue;
		        for(int j=0; j<m_nDimensions; j++)
			        fTemp[j] *= m_fLacunarity;
	        }

	        return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }

        public float HybridMultifractal(float[] f, float fOctaves, float fOffset, float fGain) {
           	// Initialize locals
	        float fValue = (GetNoise(f) + fOffset) * m_fExponent[0];
	        float fWeight = fValue;
            float[] fTemp = new float[MaxDimensions];
            for(int i=0; i<m_nDimensions; i++)
		        fTemp[i] = f[i] * m_fLacunarity;

	        // Inner loop of spectral construction, where the fractal is built
	        for(int i=1; i<fOctaves; i++) {
		        if(fWeight > 1)
			        fWeight = 1;
		        float fSignal = (GetNoise(fTemp) + fOffset) * m_fExponent[i];
		        fValue += fWeight * fSignal;
		        fWeight *= fGain * fSignal;
		        for(int j=0; j<m_nDimensions; j++)
			        fTemp[j] *= m_fLacunarity;
	        }

	        return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }

	    public float RidgedMultifractal(float[] f, float fOctaves, float fOffset, float fGain) {
          	// Initialize locals
	        float fSignal = fOffset - Math.Abs(GetNoise(f));
	        fSignal *= fSignal;
	        float fValue = fSignal;
            float[] fTemp = new float[MaxDimensions];
            for(int i=0; i<m_nDimensions; i++)
		        fTemp[i] = f[i];

	        // Inner loop of spectral construction, where the fractal is built
	        for(int i=1; i<fOctaves; i++) {
		        for(int j=0; j<m_nDimensions; j++)
			        fTemp[j] *= m_fLacunarity;
		        float fWeight = NoiseHelper.Clamp(0, 1, fSignal * fGain);
		        fSignal = fOffset - Math.Abs(GetNoise(fTemp));
		        fSignal *= fSignal;
		        fSignal *= fWeight;
		        fValue += fSignal * m_fExponent[i];
	        }

	        return NoiseHelper.Clamp(-0.99999f, 0.99999f, fValue);
        }
    }
}
