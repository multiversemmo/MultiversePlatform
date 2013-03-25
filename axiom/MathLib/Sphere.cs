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
    ///		A standard sphere, used mostly for bounds checking.
    /// </summary>
    /// <remarks>
    ///		A sphere in math texts is normally represented by the function
    ///		x^2 + y^2 + z^2 = r^2 (for sphere's centered on the origin). We store spheres
    ///		simply as a center point and a radius.
    /// </remarks>
    public sealed class Sphere {
        #region Protected member variables

        private float radius;
        private Vector3 center;

        #endregion

        #region Constructors

        /// <summary>
        ///		Creates a unit sphere centered at the origin.
        /// </summary>
        public Sphere() {	
            radius = 1.0f;
            center = Vector3.Zero;
        }

        /// <summary>
        /// Creates an arbitrary spehere.
        /// </summary>
        /// <param name="center">Center point of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        public Sphere(Vector3 center, float radius) {
            this.center = center;
            this.radius = radius;
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Gets/Sets the center of the sphere.
        /// </summary>
        public Vector3 Center {
            get { 
				return center; 
			}
            set { 
				center = value; 
			}
        }

        /// <summary>
        ///		Gets/Sets the radius of the sphere.
        /// </summary>
        public float Radius {
            get { 
				return radius; 
			}
            set { 
				radius = value; 
			}
        }

        #endregion

		#region Intersection methods
		public static bool operator==(Sphere sphere1, Sphere sphere2) 
		{
			return sphere1.center == sphere2.center && sphere1.radius == sphere2.radius;
		}
		
		public static bool operator!=(Sphere sphere1, Sphere sphere2) 
		{
			return sphere1.center != sphere2.center || sphere1.radius != sphere2.radius;
		}
		public override bool Equals(object obj)
		{
			return obj is Sphere && this == (Sphere)obj;
		}
		public override int GetHashCode()
		{
			return center.GetHashCode() ^ radius.GetHashCode();
		}



		/// <summary>
		///		Tests for intersection between this sphere and another sphere.
		/// </summary>
		/// <param name="sphere">Other sphere.</param>
		/// <returns>True if the spheres intersect, false otherwise.</returns>
		public bool Intersects(Sphere sphere) 
		{
			return ((sphere.center - center).Length <= (sphere.radius + radius));
		}

		/// <summary>
		///		Returns whether or not this sphere interects a box.
		/// </summary>
		/// <param name="box"></param>
		/// <returns>True if the box intersects, false otherwise.</returns>
		public bool Intersects(AxisAlignedBox box) {
			return MathUtil.Intersects(this, box);
		}

		/// <summary>
		///		Returns whether or not this sphere interects a plane.
		/// </summary>
		/// <param name="plane"></param>
		/// <returns>True if the plane intersects, false otherwise.</returns>
		public bool Intersects(Plane plane) {
			return MathUtil.Intersects(this, plane);
		}

		/// <summary>
		///		Returns whether or not this sphere interects a Vector3.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns>True if the vector intersects, false otherwise.</returns>
		public bool Intersects(Vector3 vector) {
			return (vector - center).Length <= radius;
		}

		#endregion Intersection methods
    }
}
