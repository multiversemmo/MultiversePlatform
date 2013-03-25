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
using System.Diagnostics;

using Axiom.MathLib;

namespace Multiverse.MathLib {
	/// <summary>
	///    This class implements the ear-cutting algorithm: 
	///    An ear is a triangle formed by two consecutive edges of the 
	///    polygon with a convex angle that contains no other point of 
	///    a polygon. There exist always at least two ears in a polygon.
	///    An ear can be cut from the polygon, reducing its size and thus 
	///	   triangulating it.
	/// </summary>
	public class TriangleTessellation {

		private static float Cross(Vector2 v0, Vector2 v1, Vector2 v2) {
			return (v1.x - v0.x) * (v2.y - v0.y) - (v2.x - v0.x) * (v1.y - v0.y);
		}

		/// <summary>
		///   The polygon in the contour is defined in a counter clockwise manner,
		///   as is the triangle.  By examining the angle between two edges, we can
		///   determine if these three points define a triangle inside the contour.
		///   This checks if the point v0 is inside the convex hull about the 
		///   contour (if it is not, it can be snipped off).
		/// </summary>
		/// <param name="triangle"></param>
		/// <returns></returns>
		private static bool IsNonReflex(Vector2 v0, Vector2 v1, Vector2 v2) {
			return Cross(v0, v1, v2) > 0;
		}

		/// <summary>
		///   Determine if any of the points in the contour lie inside the triangle 
		/// </summary>
		/// <param name="contour"></param>
		/// <param name="u">index of one of the triangles in the contour</param>
		/// <param name="v">index of one of the triangles in the contour</param>
		/// <param name="w">index of one of the triangles in the contour</param>
		/// <param name="polyIndices"></param>
		/// <returns>true if there are points in the triangle, or true if there are</returns>
		private static bool IsPolygonNonEmpty(List<Vector2> contour, int u, int v, int w)
		{
  			Vector2 v0 = contour[u];
			Vector2 v1 = contour[v];
			Vector2 v2 = contour[w];
			int n = contour.Count;
			for (int p = 0; p < n; p++) {
				if ((p == u) || (p == v) || (p == w))
					continue;
				Vector2 vP = contour[p];
				if (InsideTriangle(v0.x, v0.y, v1.x, v1.y, v2.x, v2.y, vP.x, vP.y))
					return true;
			}

			return false;
		}

		private static bool InsideTriangle(float Ax, float Ay, float Bx, float By, 
										   float Cx, float Cy, float Px, float Py)
		{
			float ax = Cx - Bx;
			float ay = Cy - By;
			float bx = Ax - Cx;
			float by = Ay - Cy;
			float cx = Bx - Ax;
			float cy = By - Ay;
			float apx = Px - Ax;
			float apy = Py - Ay;
			float bpx = Px - Bx;
			float bpy = Py - By;
			float cpx = Px - Cx;
			float cpy = Py - Cy;

			float aCROSSbp = ax * bpy - ay * bpx;
			float cCROSSap = cx * apy - cy * apx;
			float bCROSScp = bx * cpy - by * cpx;
			
			return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
		}

		/// <summary>
		///   Process the contour, and build a list of the component triangles.
		///   The triples of indices into the contour will be stored in result.
		/// </summary>
		/// <param name="contour"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool Process(List<Vector3> contour, List<int[]> result) {
			// Make sure we have enough points
			int n = contour.Count;
			if (n < 3)
				return false;

			// allocate and initialize list of Vertices in polygon
			List<Vector2> contourProjection = null;
			Quaternion r = Quaternion.Identity;
			for (int i = 0; i < contour.Count - 1; ++i) {
				Vector3 edge1 = contour[i + 1] - contour[i];
				Vector3 edge2 = contour[(i + 2) % contour.Count] - contour[i + 1];
				Vector3 vUp = edge1.Cross(edge2);
				if (vUp.LengthSquared > float.Epsilon) {
					// we found a valid sequence, though perhaps the vUp 
					// points in the wrong direction
					r = vUp.GetRotationTo(Vector3.UnitZ);
					contourProjection = new List<Vector2>();
					break;
				}
			}
			// Debug.Assert(contourProjection != null, "Unable to find significant polygon in contour");
			if (contourProjection == null)
				return false;
			foreach (Vector3 point in contour) {
				Vector3 tmp = r * point;
				contourProjection.Add(new Vector2(tmp.x, tmp.y));
			}

			bool status;
			status = Process(contourProjection, result);
			if (status)
				return true;
			// We were unable to triangulate.. This means that vUp is backwards.
			result.Clear();
			contourProjection.Clear();
			foreach (Vector3 point in contour) {
				Vector3 tmp = r * point;
				contourProjection.Add(new Vector2(tmp.x, -tmp.y));
			}
			status = Process(contourProjection, result);
			if (status)
				return true;
			// Debug.Assert(false, "Shouldn't get here");
			return false;
		}

		private static int NextPoint(List<int> points, int i, int n) {
			do {
				i = (i + 1) % n;
			} while (points.Contains(i));
			return i;
		}

		/// <summary>
		///   Process the contour, and build a list of the component triangles.
		///   The triples of indices into the contour will be stored in result.
		/// </summary>
		/// <param name="contour"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		private static bool Process(List<Vector2> contour, List<int[]> result) {
			// u, v, w are indices of points in the contour list
			int u = 0;
			int v = 1;
			int w = 2;
			int pointsConsidered = 0;
			List<int> removedPoints = new List<int>();
			while (contour.Count - removedPoints.Count >= 3) {
				// Is v an eartip?
				float cross = Cross(contour[u], contour[v], contour[w]);
				if (cross <= 0 ||
					IsPolygonNonEmpty(contour, u, v, w)) {
					// Can't remove this one
					u = NextPoint(removedPoints, u, contour.Count);
					v = NextPoint(removedPoints, v, contour.Count);
					w = NextPoint(removedPoints, w, contour.Count);
					pointsConsidered++;
					if (pointsConsidered > contour.Count)
						// we must be going the wrong way around the polygon
						return false;
					continue;
				}
				// Ok, we can snip out the polygon with eartip v;
				int[] triangle = new int[3];
				triangle[0] = u;
				triangle[1] = v;
				triangle[2] = w;
				result.Add(triangle);
				removedPoints.Add(v);
				pointsConsidered = 0;
				v = NextPoint(removedPoints, v, contour.Count);
				w = NextPoint(removedPoints, w, contour.Count);
			}
			return true;
		}
	}
};




