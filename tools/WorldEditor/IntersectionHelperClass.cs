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
using Axiom.MathLib;

namespace Multiverse.Tools.WorldEditor
{
	class IntersectionHelperClass
	{
		private static bool intersect(Vector3 pointa, Vector3 pointb, Vector3 pointc, Vector3 pointd)
		{
			float den = (((pointd.z - pointc.z) * (pointb.x - pointa.x)) - ((pointd.x - pointc.x) * (pointb.z - pointa.z)));
			float t1num = (((pointd.x - pointc.x) * (pointa.z - pointc.z)) - ((pointd.z - pointc.z) * (pointa.x - pointc.x)));
			float t2num = (((pointb.x - pointa.x) * (pointa.z - pointc.z)) - ((pointb.z - pointa.z) * (pointa.x - pointc.x)));
			if (den == 0)
			{
				if (t1num == 0 && t2num == 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				float t1 = (t1num / den);
				float t2 = (t2num / den);
				if (t1 < 0 || t1 > 1)
				{
					return false;
				}
				if (t2 < 0 || t2 > 1)
				{
					return false;
				}
				if ((0 <= t1 && t1 <= 1) && (0 <= t2 && t2 <= 1))
				{
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// This overload is used to find intersections when adding or inserting a new point
		/// This assumes the point has not been added to the List of points
		/// </summary>
		/// <returns></returns>
		public static bool BoundaryIntersectionSearch(List<Vector3> points, Vector3 point, int index)
		{
			// this is used by the Add and Insert methods to prevent the additon of new members that
			// cause an intersection in the boundary.  Point should be the position of the point
			// being added and index should indicate the boundary index at which it will be added.
			int curIndex = 0;
			int curIndexNext = 1;
			int prevIndex;
			int nextIndex;
			bool intersected = false;

			if (points.Count <= 2)
			{
				return false;
			}
			if ((index == points.Count) || (index == 0))
			{
				prevIndex = points.Count - 1;
				nextIndex = 0;
			}
			else
			{
				prevIndex = index - 1;
				nextIndex = index;
			}

			for (int i = 0; i < points.Count; i++)
			{
				if (curIndex == points.Count - 1)
				{
					curIndexNext = 0;
				}
				if (curIndex != prevIndex && curIndexNext != prevIndex)
				{
					intersected = intersect(points[curIndex], points[curIndexNext],
						point, points[prevIndex]);
				}
				if (intersected)
				{
					return intersected;
				}
				if (curIndex != nextIndex && curIndexNext != nextIndex)
				{
					intersected = intersect(points[curIndex], points[curIndexNext],
						point, points[nextIndex]);
				}
				if (intersected)
				{
					return intersected;
				}
				curIndex++;
				curIndexNext++;
				if (curIndex == points.Count)
				{
					return intersected;
				}
				if (curIndexNext > (points.Count - 1))
				{
					curIndexNext = 0;
				}

			}
			return intersected;
		}

		/// <summary>
		/// This Intersection search is done when deleting a point
		/// This assumes that the point to be deleted is still in the list
		/// </summary>
		/// <returns></returns>
		public static bool BoundaryIntersectionSearch(List<Vector3> points, int index)
		{
			// This is used by the delete method to protect from a delete causing an intersection
			// in the boundary. Index should be the boundary index of the point to be deleted.

			int curIndex = 0;
			int curIndexNext = 1;
			int prevIndex;
			int nextIndex;
			bool intersected = false;

			if (points.Count < 4)
			{
				return false;
			}
			if ((index == points.Count - 1) || (index == 0))
			{
				prevIndex = points.Count - 2;
				nextIndex = 0;
			}
			else
			{
				prevIndex = index - 1;
				nextIndex = index;
			}

			for (int i = 0; i < points.Count - 1; i++)
			{
				if (curIndex == index)
				{
					curIndex++;
				}
				if (curIndexNext == index)
				{
					curIndexNext++;
				}
				if (prevIndex == index)
				{
					prevIndex++;
				}
				if (nextIndex == index)
				{
					nextIndex++;
				}
				if (curIndex >= points.Count - 2)
				{
					if (index != 0)
					{
						curIndexNext = 0;
					}
					else
					{
						curIndexNext = 1;
					}
				}
				if (curIndex != prevIndex && curIndexNext != prevIndex)
				{
					intersected = intersect(points[curIndex], points[curIndexNext],
						points[index - 1], points[prevIndex]);
				}
				if (intersected)
				{
					return intersected;
				}
				if (curIndex != nextIndex && curIndexNext != nextIndex && ((index - 1) != curIndexNext))
				{
					intersected = intersect(points[curIndex], points[curIndexNext],
						points[index - 1], points[nextIndex]);
				}
				if (intersected)
				{
					return intersected;
				}
				curIndex++;
				curIndexNext++;
				if (curIndex == points.Count)
				{
					return intersected;
				}
				if (curIndexNext > (points.Count - 1))
				{
					curIndexNext = 0;
				}

			}
			return intersected;
		}

		/// <summary>
		/// This intersection search should be used when you are moving a point
		/// It assumes the point is already in the new position in the list
		/// </summary>
		/// <returns></returns>
		public static bool BoundaryIntersectionSearch(int index, List<Vector3> points)
		{
			// If a boundary of a boundaries points is being moved to determine if moving one of the points will cause
			// an intersection in the boundary.  The index passed in should be the boundary index of the point being moved.
			int curIndex = 0;
			int curIndexNext = 1;
			int prevIndex;
			int nextIndex;
			bool intersected = false;

			if (points == null || points.Count <= 2)
			{
				return false;
			}
			if (index == (points.Count - 1))
			{
				prevIndex = points.Count - 2;
				nextIndex = 0;
			}
			else if (index == 0)
			{
				prevIndex = points.Count - 1;
				nextIndex = 1;
			}
			else
			{
				prevIndex = index - 1;
				nextIndex = index + 1;
			}

			for (int i = 0; i < points.Count; i++)
			{
				if (curIndexNext == index)
				{
					if (curIndexNext == points.Count - 1)
					{
						curIndexNext = 0;
					}
				}
				if (curIndex != prevIndex && curIndexNext != prevIndex && curIndex != index && prevIndex != index && curIndexNext != index)
				{
					intersected = intersect(points[curIndex], points[curIndexNext],
						points[index], points[prevIndex]);
				}
				if (intersected)
				{
					return intersected;
				}
				if (curIndex != nextIndex && curIndexNext != nextIndex && curIndex != index && nextIndex != index && curIndexNext != index)
				{
					intersected = intersect(points[curIndex], points[curIndexNext],
						points[index], points[nextIndex]);
				}
				if (intersected)
				{
					return intersected;
				}
				curIndex++;
				curIndexNext++;
				if (curIndex == points.Count)
				{
					return intersected;
				}
				if (curIndexNext > (points.Count - 1))
				{
					curIndexNext = 0;
				}

			}
			return intersected;
		}

        public static bool BoundaryIntersectionSearch(List<MPPoint> points)
        {
            int i = 0;
            List<Vector3> positionList = new List<Vector3>();
            foreach (MPPoint point in points)
            {
                positionList.Add(point.Position);
            }
            foreach (MPPoint point in points)
            {
                i = points.IndexOf(point);

                if (BoundaryIntersectionSearch(i, positionList))
                {
                    return true;
                }
            }
            return false;
        }
	}
}
