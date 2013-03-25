#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;

namespace Axiom.MathLib {
    /// <summary>
    /// Defines a plane in 3D space.
    /// </summary>
    /// <remarks>
    /// A plane is defined in 3D space by the equation
    /// Ax + By + Cz + D = 0
    ///
    /// This equates to a vector (the normal of the plane, whose x, y
    /// and z components equate to the coefficients A, B and C
    /// respectively), and a constant (D) which is the distance along
    /// the normal you have to go to move the plane back to the origin.
    /// </remarks>
    public struct Plane {
		#region Fields

		/// <summary>
		///		Direction the plane is facing.
		/// </summary>
        public Vector3 Normal;
		/// <summary>
		///		Distance from the origin.
		/// </summary>
        public float D;

		private static readonly Plane nullPlane = new Plane(Vector3.Zero, 0);
		public static Plane Null { get { return nullPlane; } }

		#endregion Fields

        #region Constructors

        public Plane(Plane plane) {
            this.Normal = plane.Normal;
            this.D = plane.D;
        }

		/// <summary>
		///		Construct a plane through a normal, and a distance to move the plane along the normal.
		/// </summary>
		/// <param name="normal"></param>
		/// <param name="constant"></param>
        public Plane(Vector3 normal, float constant) {
            this.Normal = normal;
            this.D = -constant;
        }

        public Plane(Vector3 normal, Vector3 point) {
            this.Normal = normal;
            this.D = -normal.Dot(point);
        }

		/// <summary>
		///		Construct a plane from 3 coplanar points.
		/// </summary>
		/// <param name="point0">First point.</param>
		/// <param name="point1">Second point.</param>
		/// <param name="point2">Third point.</param>
		public Plane(Vector3 point0, Vector3 point1, Vector3 point2) {
			Vector3 edge1 = point1 - point0;
			Vector3 edge2 = point2 - point0;
			Normal = edge1.Cross(edge2);
			Normal.Normalize();
			D = -Normal.Dot(point0);
		}

        #endregion

        #region Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
        public PlaneSide GetSide(Vector3 point) {
            float distance = GetDistance(point);

            if ( distance < 0.0f )
                return PlaneSide.Negative;

            if ( distance > 0.0f )
                return PlaneSide.Positive;

            return PlaneSide.None;
        }

		/// <summary>
		///     Returns which side of the plane that the given box lies on.
		///     The box is defined as centre/half-size pairs for effectively.
		/// </summary>
		/// <param name="center">The center of the box.</param>
		/// <param name="halfSize">The half-size of the box.</param>
		/// <returns></returns>
        public PlaneSide GetSide (Vector3 center, Vector3 halfSize) {
            // Calculate the distance between box centre and the plane
            float dist = GetDistance(center);

            // Calculate the maximise allows absolute distance for
            // the distance between box centre and plane
            float maxAbsDist = Math.Abs(Normal.Dot(halfSize));

            if (dist < -maxAbsDist)
                return PlaneSide.Negative;

            if (dist > +maxAbsDist)
                return PlaneSide.Positive;

            return PlaneSide.None;
        }

        /// <summary>
        /// This is a pseudodistance. The sign of the return value is
        /// positive if the point is on the positive side of the plane,
        /// negative if the point is on the negative side, and zero if the
        ///	 point is on the plane.
        /// The absolute value of the return value is the true distance only
        /// when the plane normal is a unit length vector.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float GetDistance(Vector3 point) { 
            return Normal.Dot(point) + D;
        }

		/// <summary>
		///		Construct a plane from 3 coplanar points.
		/// </summary>
		/// <param name="point0">First point.</param>
		/// <param name="point1">Second point.</param>
		/// <param name="point2">Third point.</param>
		public void Redefine(Vector3 point0, Vector3 point1, Vector3 point2) {
			Vector3 edge1 = point1 - point0;
			Vector3 edge2 = point2 - point0;
			Normal = edge1.Cross(edge2);
			Normal.Normalize();
			D = -Normal.Dot(point0);
		}

        /// <summary>
		///		Find the intersection of a line and a plane
		/// </summary>
		/// <param name="p1">First point defining the line.</param>
		/// <param name="p2">Second point defining the line.</param>
		/// <param name="result">The intersection point, if it exists.</param>
		/// <returns>True if the intersection point was calculated;
		/// false otherwise.</returns>            
        public bool Intersection(Vector3 p1, Vector3 p2, out Vector3 result) {
            Vector3 diff = p2 - p1;
            float denom = Normal.Dot(diff);
            if (Math.Abs(denom) > 1.0e-8)
            {
                Vector3 pInPlane = Normal * -D;
                float numer = Normal.Dot(pInPlane - p1);
                float u = numer / denom;
                result = p1 + u * diff;
                return true;
            }
            else {
                result = Vector3.Zero;
                return false;
            }
        }

        #endregion Methods

        #region Object overrides

		/// <summary>
		///		Object method for testing equality.
		/// </summary>
		/// <param name="obj">Object to test.</param>
		/// <returns>True if the 2 planes are logically equal, false otherwise.</returns>
		public override bool Equals(object obj) {
			return obj is Plane && this == (Plane)obj;
		}

		/// <summary>
		///		Gets the hashcode for this Plane.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() {
			return D.GetHashCode() ^ Normal.GetHashCode();
		}

		/// <summary>
		///		Returns a string representation of this Plane.
		/// </summary>
		/// <returns></returns>
        public override string ToString() {
            return string.Format("Distance: {0} Normal: {1}", D, Normal.ToString());
        }

        #endregion

		#region Operator Overloads

		/// <summary>
		///		Compares 2 Planes for equality.
		/// </summary>
		/// <param name="left">First plane.</param>
		/// <param name="right">Second plane.</param>
		/// <returns>true if equal, false if not equal.</returns>
		public static bool operator == (Plane left, Plane right) {
			return (left.D == right.D) && (left.Normal == right.Normal);
		}

		/// <summary>
		///		Compares 2 Planes for inequality.
		/// </summary>
		/// <param name="left">First plane.</param>
		/// <param name="right">Second plane.</param>
		/// <returns>true if not equal, false if equal.</returns>
		public static bool operator != (Plane left, Plane right) {
			return (left.D != right.D) || (left.Normal != right.Normal);
		}

		#endregion
	}
}
