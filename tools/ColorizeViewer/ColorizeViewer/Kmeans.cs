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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorizeViewer
{
    public class Kmeans
    {
        protected HSVColor[] allColors;
        protected Point[] centers;
        protected Point[] lastCenters;
        protected HSVColor[] centerColors;

        protected Point[] accumulators;
        protected int[] accumulatorCounts;
        protected float[,] centerDistances;

        protected Random r;

        int k;

        public Kmeans(HSVColor[] colors, int k)
        {
            r = new Random();

            this.k = k;

            centers = new Point[k];
            lastCenters = new Point[k];
            centerColors = new HSVColor[k];

            accumulators = new Point[k];
            accumulatorCounts = new int[k];

            allColors = colors;

            InitCenters();

            int n = 1;
            DumpCenters(0);
            while (!FoundSolution())
            {
                ClearAccumulators();
                CopyCenters();
                for (int i = 0; i < allColors.Length; i++)
                {
                    // find closest point for this color
                    int closest = ClosestIndex(allColors[i]);
                    Point pt = allColors[i].HSCartesian;

                    // add this color's location to the accumulator for the closest point
                    accumulators[closest].X += pt.X;
                    accumulators[closest].Y += pt.Y;
                    accumulatorCounts[closest]++;
                }

                CalculateCenters();
                DumpCenters(n);
                n++;
            }

            // convert the colors back to HSV
            for (int i = 0; i < k; i++)
            {
                centerColors[i] = new HSVColor(centers[i]);
            }

            centerDistances= new float[k,k];

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    if (i == j)
                    {
                        centerDistances[i,j] = 0.0f;
                    }
                    else
                    {
                        centerDistances[i,j] = centers[i].Distance(centers[j]);
                    }
                }
            }

        }

        public void DumpCenters(int iter)
        {
            Console.WriteLine("Iteration {0}", iter);
            for (int i = 0; i < k; i++)
            {
                Console.WriteLine("  X: {0}, Y: {1}", centers[i].X, centers[i].Y);
                HSVColor color = new HSVColor(centers[i]);
                Console.WriteLine("  H: {0}, S: {1}, V: {2}", color.H, color.S, color.V);
            }
        }

        public void CalculateCenters()
        {
            for (int i = 0; i < k; i++)
            {
                if (accumulatorCounts[i] == 0)
                {
                    //centers[i].X = 0.0f;
                    //centers[i].Y = 0.0f;
                    int index = r.Next(allColors.Length);
                    centers[i] = allColors[index].HSCartesian;

                    //centers[i].X = (float)(r.NextDouble() * 2.0 - 1.0);
                    //centers[i].Y = (float)(r.NextDouble() * 2.0 - 1.0);
                }
                else
                {
                    centers[i].X = accumulators[i].X / accumulatorCounts[i];
                    centers[i].Y = accumulators[i].Y / accumulatorCounts[i];
                }

            }
        }

        public void CopyCenters()
        {
            for (int i = 0; i < k; i++)
            {
                lastCenters[i].X = centers[i].X;
                lastCenters[i].Y = centers[i].Y;
            }
        }

        public void ClearAccumulators()
        {
            for (int i = 0; i < k; i++)
            {
                accumulatorCounts[i] = 0;
                accumulators[i].X = 0.0f;
                accumulators[i].Y = 0.0f;
            }
        }

        public HSVColor ClosestColor(HSVColor color)
        {
            int index = ClosestIndex(color);
            return new HSVColor(centers[index], color.V);
        }

        public int ClosestIndex(HSVColor color)
        {
            float distance;
            float minDist;
            int minIndex;

            minDist = lastCenters[0].Distance(color.HSCartesian);
            minIndex = 0;

            for (int i = 1; i < k; i++)
            {
                distance = lastCenters[i].Distance(color.HSCartesian);
                if (distance < minDist)
                {
                    minDist = distance;
                    minIndex = i;
                }
            }

            return minIndex;
        }

        // pick random center points in the -1 to 1 range
        public void InitCenters()
        {
            for (int i = 0; i < k; i++)
            {
                int index = r.Next(allColors.Length);
                centers[i] = allColors[index].HSCartesian;
                //centers[i].X = (float)(r.NextDouble() * 2.0 - 1.0);
                //centers[i].Y = (float)(r.NextDouble() * 2.0 - 1.0);
                lastCenters[i].X = 0.0f;
                lastCenters[i].Y = 0.0f;
            }
        }

        // check if last iteration results match this one
        public bool FoundSolution()
        {
            for (int i = 0; i < k; i++)
            {
                if (lastCenters[i].X != centers[i].X)
                {
                    return false;
                }
                if (lastCenters[i].Y != centers[i].Y)
                {
                    return false;
                }
            }

            return true;
        }

        public float[] DistanceToCenters(HSVColor color)
        {
            float[] ret = new float[centers.Length];
            Point p = color.HSCartesian;
            for (int i = 0; i < centers.Length; i++)
            {
                ret[i] = centers[i].Distance(p);
            }

            return ret;
        }

        public HSVColor[] CenterColors
        {
            get
            {
                return centerColors;
            }
        }

        /// <summary>
        /// Array containing the distances between each of the center points
        /// </summary>
        public float[,] CenterDistances
        {
            get
            {
                return centerDistances;
            }
        }
    }
}
