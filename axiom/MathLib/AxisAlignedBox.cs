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
using System.Diagnostics;
using System.ComponentModel;

namespace Axiom.MathLib {
    /// <summary>
    ///		A 3D box aligned with the x/y/z axes.
    /// </summary>
    /// <remarks>
    ///		This class represents a simple box which is aligned with the
    ///	    axes. Internally it only stores 2 points as the extremeties of
    ///	    the box, one which is the minima of all 3 axes, and the other
    ///	    which is the maxima of all 3 axes. This class is typically used
    ///	    for an axis-aligned bounding box (AABB) for collision and
    ///	    visibility determination.
    /// </remarks>
    
    public sealed class AxisAlignedBox : ICloneable {
        #region Fields

        internal Vector3 minVector = new Vector3(-0.5f, -0.5f, -0.5f);
        internal Vector3 maxVector = new Vector3(0.5f, 0.5f, 0.5f);
        private Vector3[] corners = new Vector3[8];
        private bool isNull = true;
        private static readonly AxisAlignedBox nullBox = new AxisAlignedBox();

        #endregion

        #region Constructors

        public AxisAlignedBox() {
            SetExtents(minVector, maxVector);
            isNull = true;
        }

        public AxisAlignedBox(Vector3 min, Vector3 max) {
            SetExtents(min, max);
        }

        #endregion 

        #region Public methods

		public Vector3 Size {
			get { return maxVector - minVector; }
			set
			{
				Vector3 center = Center;
				Vector3 halfSize = .5f * value;
				minVector = center - halfSize;
				maxVector = center + halfSize;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        public void Transform(Matrix4 matrix) {
            // do nothing for a null box
            if(isNull)
                return;

            Vector3 min = new Vector3();
            Vector3 max = new Vector3();
            Vector3 temp = new Vector3();

            bool isFirst = true;
            int i;

            for( i = 0; i < corners.Length; i++ ) {
                // Transform and check extents
                temp = matrix * corners[i];
                if( isFirst || temp.x > max.x )
                    max.x = temp.x;
                if( isFirst || temp.y > max.y )
                    max.y = temp.y;
                if( isFirst || temp.z > max.z )
                    max.z = temp.z;
                if( isFirst || temp.x < min.x )
                    min.x = temp.x;
                if( isFirst || temp.y < min.y )
                    min.y = temp.y;
                if( isFirst || temp.z < min.z )
                    min.z = temp.z;

                isFirst = false;
            }

            SetExtents(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateCorners() {
            // The order of these items is, using right-handed co-ordinates:
            // Minimum Z face, starting with Min(all), then anticlockwise
            //   around face (looking onto the face)
            // Maximum Z face, starting with Max(all), then anticlockwise
            //   around face (looking onto the face)
            corners[0] = minVector;
            corners[1].x = minVector.x; corners[1].y = maxVector.y; corners[1].z = minVector.z;
            corners[2].x = maxVector.x; corners[2].y = maxVector.y; corners[2].z = minVector.z;
            corners[3].x = maxVector.x; corners[3].y = minVector.y; corners[3].z = minVector.z;            

            corners[4] = maxVector;
            corners[5].x = minVector.x; corners[5].y = maxVector.y; corners[5].z = maxVector.z;
            corners[6].x = minVector.x; corners[6].y = minVector.y; corners[6].z = maxVector.z;
            corners[7].x = maxVector.x; corners[7].y = minVector.y; corners[7].z = maxVector.z;            
        }

        /// <summary>
        ///		Sets both Minimum and Maximum at once, so that UpdateCorners only
        ///		needs to be called once as well.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetExtents(Vector3 min, Vector3 max) {
            isNull = false;

            minVector = min;
            maxVector = max;

            UpdateCorners();
        }

        /// <summary>
        ///    Scales the size of the box by the supplied factor.
        /// </summary>
        /// <param name="factor">Factor of scaling to apply to the box.</param>
        public void Scale(Vector3 factor) {
            Vector3 min = minVector * factor;
            Vector3 max = maxVector * factor;

            SetExtents(min, max);
        }

        #endregion

		#region Intersection Methods

		/// <summary>
		///		Returns whether or not this box intersects another.
		/// </summary>
		/// <param name="box2"></param>
		/// <returns>True if the 2 boxes intersect, false otherwise.</returns>
		public bool Intersects(AxisAlignedBox box2) {
			// Early-fail for nulls
			if (this.IsNull || box2.IsNull)
				return false;

			// Use up to 6 separating planes
			if (this.maxVector.x < box2.minVector.x)
				return false;
			if (this.maxVector.y < box2.minVector.y)
				return false;
			if (this.maxVector.z < box2.minVector.z)
				return false;

			if (this.minVector.x > box2.maxVector.x)
				return false;
			if (this.minVector.y > box2.maxVector.y)
				return false;
			if (this.minVector.z > box2.maxVector.z)
				return false;

			// otherwise, must be intersecting
			return true;
		}

		/// <summary>
		///		Tests whether this box intersects a sphere.
		/// </summary>
		/// <param name="sphere"></param>
		/// <returns>True if the sphere intersects, false otherwise.</returns>
		public bool Intersects(Sphere sphere) {
			return MathUtil.Intersects(sphere, this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="plane"></param>
		/// <returns>True if the plane intersects, false otherwise.</returns>
		public bool Intersects(Plane plane) {
			return MathUtil.Intersects(plane, this);
		}

		/// <summary>
		///		Tests whether the vector point is within this box.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns>True if the vector is within this box, false otherwise.</returns>
		public bool Intersects(Vector3 vector) {
			return(vector.x >= minVector.x  &&  vector.x <= maxVector.x  && 
				vector.y >= minVector.y  &&  vector.y <= maxVector.y  && 
				vector.z >= minVector.z  &&  vector.z <= maxVector.z);
		}

		/// <summary>
		///		Calculate the area of intersection of this box and another
		/// </summary>
		public AxisAlignedBox Intersection(AxisAlignedBox b2)
		{
			if (!Intersects(b2))
				return new AxisAlignedBox();
			Vector3 intMin = Vector3.Zero;
            Vector3 intMax = Vector3.Zero;

			Vector3 b2max = b2.maxVector;
			Vector3 b2min = b2.minVector;

			if (b2max.x > maxVector.x && maxVector.x > b2min.x)
				intMax.x = maxVector.x;
			else 
				intMax.x = b2max.x;
			if (b2max.y > maxVector.y && maxVector.y > b2min.y)
				intMax.y = maxVector.y;
			else 
				intMax.y = b2max.y;
			if (b2max.z > maxVector.z && maxVector.z > b2min.z)
				intMax.z = maxVector.z;
			else 
				intMax.z = b2max.z;

			if (b2min.x < minVector.x && minVector.x < b2max.x)
				intMin.x = minVector.x;
			else
				intMin.x= b2min.x;
			if (b2min.y < minVector.y && minVector.y < b2max.y)
				intMin.y = minVector.y;
			else
				intMin.y = b2min.y;
			if (b2min.z < minVector.z && minVector.z < b2max.z)
				intMin.z = minVector.z;
			else
				intMin.z= b2min.z;

			return new AxisAlignedBox(intMin, intMax);
		}


		#endregion Intersection Methods

        #region Properties

        /// <summary>
        ///    Gets the center point of this bounding box.
        /// </summary>
        public Vector3 Center {
            get {
                return (minVector + maxVector) * 0.5f;
            }
			set
			{
				Vector3 halfSize = .5f * Size;
				minVector = value - halfSize;
				maxVector = value + halfSize;
			}
        }

        /// <summary>
        ///		Gets/Sets the maximum corner of the box.
        /// </summary>
        public Vector3 Maximum {
            get {
                return maxVector;
            }
            set {
                isNull = false;
                maxVector = value;
                UpdateCorners();
            }
        }

        /// <summary>
        ///		Gets/Sets the minimum corner of the box.
        /// </summary>
        public Vector3 Minimum {
            get {
                return minVector;
            }
            set {
                isNull = false;
                minVector = value;
                UpdateCorners();
            }
        }

        /// <summary>
        ///		Returns an array of 8 corner points, useful for
        ///		collision vs. non-aligned objects.
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
        ///			 1-----2
        ///		    /|     /|
        ///		  /  |   /  |
        ///		5-----4   |
        ///		|   0-|--3
        ///		|  /   |  /
        ///		|/     |/
        ///		6-----7
        ///		</pre>
        /// </remarks>
        public Vector3[] Corners {
            get {
                Debug.Assert(isNull != true, "Cannot get the corners of a null box.");

                // return a clone of the array (not the original)
                return (Vector3[])corners.Clone();
                //return corners;
            }
        }

        /// <summary>
        ///		Gets/Sets the value of whether this box is null (i.e. not dimensions, etc).
        /// </summary>
        public bool IsNull {
            get { 
                return isNull; 
            }
            set { 
                isNull = value; 
            }
        }

        /// <summary>
        ///		Returns a null box
        /// </summary>
        public static AxisAlignedBox Null {
            get { 
                AxisAlignedBox nullBox = new AxisAlignedBox();
                // make sure it is set to null
                nullBox.IsNull = true;

                return nullBox; 
            }
        }

        #endregion

        #region Operator Overloads

		public static bool operator==(AxisAlignedBox left, AxisAlignedBox right) 
		{
            if ((object.ReferenceEquals(left, null) || left.isNull) &&
                (object.ReferenceEquals(right, null) || right.isNull))
                return true;
            else if ((object.ReferenceEquals(left, null) || left.isNull) ||
                     (object.ReferenceEquals(right, null) || right.isNull))
                return false;
            return
				(left.corners[0] == right.corners[0] && left.corners[1] == right.corners[1] && left.corners[2] == right.corners[2] && 
				left.corners[3] == right.corners[3] && left.corners[4] == right.corners[4] && left.corners[5] == right.corners[5] && 
				left.corners[6] == right.corners[6] && left.corners[7] == right.corners[7]);
		}

		public static bool operator!=(AxisAlignedBox left, AxisAlignedBox right) 
		{
            if ((object.ReferenceEquals(left, null) || left.isNull) &&
                (object.ReferenceEquals(right, null) || right.isNull))
                return false;
            else if ((object.ReferenceEquals(left, null) || left.isNull) ||
                     (object.ReferenceEquals(right, null) || right.isNull))
                return true;
            return
				(left.corners[0] != right.corners[0] || left.corners[1] != right.corners[1] || left.corners[2] != right.corners[2] || 
				left.corners[3] != right.corners[3] || left.corners[4] != right.corners[4] || left.corners[5] != right.corners[5] || 
				left.corners[6] != right.corners[6] || left.corners[7] != right.corners[7]);
		}
		public override bool Equals(object obj)
		{
			return obj is AxisAlignedBox && this == (AxisAlignedBox)obj;
		}

		public override int GetHashCode()
		{
			if(isNull)
				return 0;
			return corners[0].GetHashCode() ^ corners[1].GetHashCode() ^ corners[2].GetHashCode() ^ corners[3].GetHashCode() ^ corners[4].GetHashCode() ^ 
				corners[5].GetHashCode() ^ corners[6].GetHashCode() ^ corners[7].GetHashCode();
		}


		public override string ToString()
		{
			return this.minVector.ToString() + ":" + this.maxVector.ToString();
		}

		public static AxisAlignedBox Parse(string text) 
		{
			string[] parts = text.Split(':');
			return new AxisAlignedBox(Vector3.Parse(parts[0]), Vector3.Parse(parts[1]));
		}
		public static AxisAlignedBox FromDimensions(Vector3 center, Vector3 size) 
		{
			Vector3 halfSize = .5f * size;
			return new AxisAlignedBox(center - halfSize, center + halfSize);
		}


        /// <summary>
        ///		Allows for merging two boxes together (combining).
        /// </summary>
        /// <param name="box">Source box.</param>
        public void Merge(AxisAlignedBox box) 
		{
            // nothing to merge with in this case, just return
            if(box.IsNull) {
                return;
            }
            else if (isNull) {
                SetExtents(box.Minimum, box.Maximum);
            }
            else {
                Vector3 min = minVector;
                Vector3 max = maxVector;
                min.Floor(box.Minimum);
                max.Ceil(box.Maximum);

                SetExtents(min, max);
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone() {
            return new AxisAlignedBox(this.minVector, this.maxVector);
        }

        #endregion
    }
}
