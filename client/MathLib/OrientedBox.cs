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
using System.Text;

using Axiom.MathLib;

#endregion

namespace Multiverse.MathLib
{
	/// <summary>
	///   This class represents an oriented bounding box.
	///   In right to left order, Center * Orientation * Center.inverse()
	/// </summary>
	public class OrientedBox
	{
		// Orientation and Vector3 are structs.

		// Orientation of the bounding box.
		public Quaternion orientation;
		// Center of the bounding box.
		public Vector3 center;
		// Dimensions of the bounding box
		public Vector3 dimensions;

		public OrientedBox() {
			orientation = Quaternion.Identity;
			center = Vector3.Zero;
			dimensions = Vector3.Zero;
		}

		public OrientedBox(OrientedBox other) {
			orientation = other.orientation;
			center = other.center;
			dimensions = other.dimensions;
		}

		public Quaternion Orientation {
			get {
				return orientation;
			}
			set {
				orientation = value;
			}
		}

		public Vector3 Center {
			get {
				return center;
			}
			set {
				center = value;
			}
		}

		public Vector3 Dimensions {
			get {
				return dimensions;
			}
			set {
				dimensions = value;
			}
		}

		/// <summary>
		///		Returns an array of 8 corner points.
		///	 </summary>
		///	 <remarks>
		///		If the order of these corners is important, they are as
		///		follows: The 4 points of the minimum Z face (note that
		///		because we use right-handed coordinates, the minimum Z is
		///		at the 'back' of the box) starting with the minimum point of
		///		all, then anticlockwise around this face (if you are looking
		///		onto the face from outside the box). Then the 4 points of the
		///		maximum Z face, starting with maximum point of all, then
		///		anticlockwise around this face (looking onto the face from
		///		outside the box). Like this:
		///		<pre>
		///		   1-----2
		///		  /|    /|
		///		 / |   / |
		///		5-----4  |
		///		|  0--|--3
		///		| /   | /
		///		|/    |/
		///		6-----7
		///		</pre>
		/// </remarks>
		public Vector3[] Corners {
			get {
				Vector3[] corners = new Vector3[8];
				corners[0] = center +
							 orientation * new Vector3(-dimensions.x, -dimensions.y, -dimensions.z) * .5f;
				corners[1] = center +
							 orientation * new Vector3(-dimensions.x, +dimensions.y, -dimensions.z) * .5f;
				corners[2] = center +
							 orientation * new Vector3(+dimensions.x, +dimensions.y, -dimensions.z) * .5f;
				corners[3] = center +
							 orientation * new Vector3(+dimensions.x, -dimensions.y, -dimensions.z) * .5f;
				corners[4] = center +
							 orientation * new Vector3(+dimensions.x, +dimensions.y, +dimensions.z) * .5f;
				corners[5] = center +
							 orientation * new Vector3(-dimensions.x, +dimensions.y, +dimensions.z) * .5f;
				corners[6] = center +
							 orientation * new Vector3(-dimensions.x, -dimensions.y, +dimensions.z) * .5f;
				corners[7] = center +
							 orientation * new Vector3(+dimensions.x, -dimensions.y, +dimensions.z) * .5f;
				return corners;
			}
		}

		protected Vector3[] BasisVectors() {
			Vector3[] rv = new Vector3[3];
			rv[0] = orientation * Vector3.UnitX;
			rv[1] = orientation * Vector3.UnitY;
			rv[2] = orientation * Vector3.UnitZ;
			return rv;
		}

		/// <summary>
		///		Tests whether the vector point is within this box.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns>True if the vector is within this box, false otherwise.</returns>
		public bool Intersects(Vector3 vector) {
			// I think that the vector has been transformed into the local coordinate system of this object.
			Vector3 offset = vector - center;
			Vector3[] bases = BasisVectors();
			for (int i = 0; i < 3; ++i) {
				if (Math.Abs(offset.Dot(bases[i])) > dimensions[i] / 2)
					return false;
			}
			return true;
		}
	}
}
