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
using System.Text;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Media;

namespace Axiom.SceneManagers.Multiverse
{
    public class Road : IDisposable
    {
        private String name;
        private List<Vector2> points;
        private SparsePageMask mask;
        private int halfWidth = 1;
        private Roads creator;

        static byte[] antialiasTable;

        static byte antialiasValue(double weight)
        {
            return (byte)Math.Floor(255 * weight / 0.780);
        }

        static Road()
        {
            antialiasTable = new byte[24];
            antialiasTable[0] = antialiasValue(0.780);
            antialiasTable[1] = antialiasValue(0.775);
            antialiasTable[2] = antialiasValue(0.760);
            antialiasTable[3] = antialiasValue(0.736);
            antialiasTable[4] = antialiasValue(0.703);
            antialiasTable[5] = antialiasValue(0.662);
            antialiasTable[6] = antialiasValue(0.613);
            antialiasTable[7] = antialiasValue(0.558);
            antialiasTable[8] = antialiasValue(0.500);
            antialiasTable[9] = antialiasValue(0.441);
            antialiasTable[10] = antialiasValue(0.383);
            antialiasTable[11] = antialiasValue(0.328);
            antialiasTable[12] = antialiasValue(0.276);
            antialiasTable[13] = antialiasValue(0.228);
            antialiasTable[14] = antialiasValue(0.184);
            antialiasTable[15] = antialiasValue(0.145);
            antialiasTable[16] = antialiasValue(0.110);
            antialiasTable[17] = antialiasValue(0.080);
            antialiasTable[18] = antialiasValue(0.056);
            antialiasTable[19] = antialiasValue(0.036);
            antialiasTable[20] = antialiasValue(0.021);
            antialiasTable[21] = antialiasValue(0.010);
            antialiasTable[22] = antialiasValue(0.004);
            antialiasTable[23] = antialiasValue(0.001);
        }

        public Road(String name, Roads creator)
        {
            points = new List<Vector2>();
            this.creator = creator;
            this.name = name;
            mask = creator.Mask;
        }

        private void OnRoadChange()
        {
            creator.RebuildMask();
        }

        public void Clear()
        {
            points.Clear();
            OnRoadChange();
        }

        public void SetPoints(List<Vector3> pointsIn)
        {
            points.Clear();
            AddPoints(pointsIn);
        }

        public void AddPoints(List<Vector3> pointsIn)
        {
            foreach (Vector3 v3 in pointsIn)
            {
                points.Add(new Vector2(v3.x, v3.z));
            }

            OnRoadChange();
        }

        public void AddPoint(Vector3 point)
        {
            points.Add(new Vector2(point.x, point.z));
            OnRoadChange();
        }

        public void InsertPoint(int pointNum, Vector3 newPoint)
        {
            points.Insert(pointNum, new Vector2(newPoint.x, newPoint.z));
            OnRoadChange();
        }

        public void EditPoint(int pointNum, Vector3 newPoint)
        {
            points[pointNum] = new Vector2(newPoint.x, newPoint.z);
            OnRoadChange();
        }

        public void RemovePoint(int pointNum)
        {
            points.RemoveAt(pointNum);
            OnRoadChange();
        }

        public int HalfWidth
        {
            get
            {
                return halfWidth;
            }
            set
            {
                halfWidth = value;
                OnRoadChange();
            }
        }

        private void SetPoint(int x, int y, float distance)
        {
            int p = (int)Math.Floor(16 * Math.Abs(distance));
            byte value = 0;
            if (p < 24)
            {
                value = antialiasTable[p];
            }

            mask.SetPoint(x, y, value);
        }

        private void DrawVertSpan(int x, int y, int halfWidth, float two_dx_invDenom, float two_v_dx, float invDenom, int yincr)
        {

            if (halfWidth <= 1)
            {
                SetPoint(x, y, two_v_dx * invDenom);
                SetPoint(x, y + yincr, two_dx_invDenom - two_v_dx * invDenom);
                SetPoint(x, y - yincr, two_dx_invDenom + two_v_dx * invDenom);
            }
            else
            {
                mask.SetPoint(x, y, 255);

                for (int i = 1; i < ( halfWidth - 1 ); i++)
                {
                    mask.SetPoint(x, y + i, 255);
                    mask.SetPoint(x, y - i, 255);
                }

                SetPoint(x, y - (halfWidth - 1), two_v_dx * invDenom);
                SetPoint(x, y + (halfWidth - 1), two_v_dx * invDenom);
                SetPoint(x, y + yincr * halfWidth, two_dx_invDenom - two_v_dx * invDenom);
                SetPoint(x, y - yincr * halfWidth, two_dx_invDenom + two_v_dx * invDenom);
            }
        }

        private void DrawHorizSpan(int x, int y, int halfWidth, float two_dx_invDenom, float two_v_dx, float invDenom, int xincr)
        {

            if (halfWidth <= 1)
            {
                SetPoint(x, y, two_v_dx * invDenom);
                SetPoint(x + xincr, y, two_dx_invDenom - two_v_dx * invDenom);
                SetPoint(x - xincr, y, two_dx_invDenom + two_v_dx * invDenom);
            }
            else
            {
                mask.SetPoint(x, y, 255);

                for (int i = 1; i < (halfWidth - 1); i++)
                {
                    mask.SetPoint(x + i, y, 255);
                    mask.SetPoint(x - i, y, 255);
                }

                SetPoint(x - (halfWidth - 1), y, two_v_dx * invDenom);
                SetPoint(x + (halfWidth - 1), y, two_v_dx * invDenom);
                SetPoint(x + xincr * halfWidth, y, two_dx_invDenom - two_v_dx * invDenom);
                SetPoint(x - xincr * halfWidth, y, two_dx_invDenom + two_v_dx * invDenom);
            }
        }

        private void RenderSegment(Vector2 p1, Vector2 p2)
        {
            int p1x = (int)(p1.x / TerrainManager.oneMeter);
            int p1y = (int)(p1.y / TerrainManager.oneMeter);
            int p2x = (int)(p2.x / TerrainManager.oneMeter);
            int p2y = (int)(p2.y / TerrainManager.oneMeter);

            int adx = Math.Abs(p1x - p2x);
            int ady = Math.Abs(p1y - p2y);
            int x1, x2, y1, y2;

            float invDenom, two_dx_invDenom;
            int two_v_dx = 0;

            if (adx >= ady)
            {
                if (p1x <= p2x)
                {
                    x1 = p1x;
                    y1 = p1y;
                    x2 = p2x;
                    y2 = p2y;
                }
                else
                { // swap p1 and p2 so that we are going left to right
                    x2 = p1x;
                    y2 = p1y;
                    x1 = p2x;
                    y1 = p2y;
                }

                int d = 2 * ady - adx;      // initial value of d
                int incrE = 2 * ady;         // increment used for moves to E
                int incrNE = 2 * (ady - adx); // increment used for moves to NE
                int x = x1;
                int y = y1;
                int yincr;

                if (y1 <= y2)
                {
                    yincr = 1;
                }
                else
                {
                    yincr = -1;
                }

                invDenom = (float)(1.0 / (2.0 * Math.Sqrt(adx * adx + ady * ady)));
                two_dx_invDenom = 2 * adx * invDenom;

                DrawVertSpan(x, y, halfWidth, two_dx_invDenom, 0, 0, yincr);

                while (x < x2)
                {
                    if (d <= 0)
                    {
                        two_v_dx = d + adx;
                        d = d + incrE;
                        x++;
                    }
                    else
                    {
                        two_v_dx = d - adx;
                        d = d + incrNE;
                        x++;
                        y += yincr;
                    }

                    DrawVertSpan(x, y, halfWidth, two_dx_invDenom, two_v_dx, invDenom, yincr);
                }
                //SetPoint(x2, y2, 255);
                //SetPoint(x2, y2-1, 255);
                //SetPoint(x2, y2+1, 255);
            }
            else
            {
                if (p1y <= p2y)
                {
                    x1 = p1x;
                    y1 = p1y;
                    x2 = p2x;
                    y2 = p2y;
                }
                else
                { // swap p1 and p2 so that we are going left to right
                    x2 = p1x;
                    y2 = p1y;
                    x1 = p2x;
                    y1 = p2y;
                }

                int d = 2 * adx - ady;      // initial value of d
                int incrE = 2 * adx;         // increment used for moves to E
                int incrNE = 2 * (adx - ady); // increment used for moves to NE
                int x = x1;
                int y = y1;
                int xincr;

                if (x1 <= x2)
                {
                    xincr = 1;
                }
                else
                {
                    xincr = -1;
                }

                invDenom = (float)(1.0 / (2.0 * Math.Sqrt(adx * adx + ady * ady)));
                two_dx_invDenom = 2 * ady * invDenom;

                DrawHorizSpan(x, y, halfWidth, two_dx_invDenom, 0, 0, xincr);

                while (y < y2)
                {
                    if (d <= 0)
                    {
                        two_v_dx = d + ady;
                        d = d + incrE;
                        y++;
                    }
                    else
                    {
                        two_v_dx = d - ady;
                        d = d + incrNE;
                        y++;
                        x += xincr;
                    }

                    DrawHorizSpan(x, y, halfWidth, two_dx_invDenom, two_v_dx, invDenom, xincr);
                }
                //mask.SetPoint(x2, y2, 255);
                //mask.SetPoint(x2-1, y2, 255);
                //mask.SetPoint(x2+1, y2, 255);
            }
        }

        public void RenderMask()
        {
            for (int i = 0; i < (points.Count - 1); i++)
            {
                RenderSegment(points[i], points[i + 1]);
            }
        }

        public List<Vector2> Points
        {
            get
            {
                return points;
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            // don't need to do anything for now
        }

        #endregion
}
}
