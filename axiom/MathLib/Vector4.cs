#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

The math library included in this project, in addition to being a derivative of
the works of Ogre, also include derivative work of the free portion of the 
Wild Magic mathematics source code that is distributed with the excellent
book Game Engine Design.
http://www.wild-magic.com/

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
using System.Runtime.InteropServices;

namespace Axiom.MathLib {
    /// <summary>
    /// 4D homogeneous vector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4 {
        #region Fields

        public float x, y, z ,w;

		private static readonly Vector4 zeroVector = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        #endregion Fields

		#region Constructors

        /// <summary>
        ///		Creates a new 4 dimensional Vector.
        /// </summary>
        public Vector4(float x, float y, float z, float w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

		private Vector4(string parsableText) 
		{
			if(parsableText == null)
				throw new ArgumentException("The parsableText parameter cannot be null.");
			string[] vals = parsableText.TrimStart('(','[','<').TrimEnd(')',']','>').Split(',');
			if(vals.Length != 4)
				throw new FormatException(string.Format("Cannot parse the text '{0}' because it does not have 4 parts separated by commas in the form (x,y,z) with optional parenthesis.",parsableText));
			try
			{
				x = float.Parse(vals[0].Trim());
				y =	float.Parse(vals[1].Trim());
				z =	float.Parse(vals[2].Trim());
				w =	float.Parse(vals[3].Trim());
			}
			catch(Exception) 
			{
				throw new FormatException("The parts of the vectors must be decimal numbers");
			}
		}
		

        #endregion

        #region Properties

		/// <summary>
		///		Gets a Vector4 with all components set to 0.
		/// </summary>
		public static Vector4 Zero 
		{
			get 
			{ 
				return zeroVector; 
			}
		}

		#endregion Properties
		
		#region Methods


        /// <summary>
        ///     Calculates the dot (scalar) product of this vector with another.
        /// </summary>
        /// <param name="vec">
        ///     Vector with which to calculate the dot product (together with this one).
        /// </param>
        /// <returns>A float representing the dot product value.</returns>
        public float Dot(Vector4 vec) 
		{
            return x * vec.x + y * vec.y + z * vec.z + w * vec.w;
        }

        #endregion Methods

        #region Operator overloads + CLS compliant method equivalents

        /// <summary>
        ///		
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Vector4 Multiply (Vector4 vector, Matrix4 matrix) {
        	return vector * matrix;
        }
        /// <summary>
        ///		
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
		public static Vector4 operator * (Matrix4 matrix, Vector4 vector) {
			Vector4 result = new Vector4();
			
			result.x = vector.x * matrix.m00 + vector.y * matrix.m01 + vector.z * matrix.m02 + vector.w * matrix.m03;
			result.y = vector.x * matrix.m10 + vector.y * matrix.m11 + vector.z * matrix.m12 + vector.w * matrix.m13;
			result.z = vector.x * matrix.m20 + vector.y * matrix.m21 + vector.z * matrix.m22 + vector.w * matrix.m23;
			result.w = vector.x * matrix.m30 + vector.y * matrix.m31 + vector.z * matrix.m32 + vector.w * matrix.m33;

			return result;
		}

		// TODO: Find the signifance of having 2 overloads with opposite param lists that do transposed operations
		public static Vector4 operator * (Vector4 vector, Matrix4 matrix) {
			Vector4 result = new Vector4();
			
			result.x = vector.x * matrix.m00 + vector.y * matrix.m10 + vector.z * matrix.m20 + vector.w * matrix.m30;
			result.y = vector.x * matrix.m01 + vector.y * matrix.m11 + vector.z * matrix.m21 + vector.w * matrix.m31;
			result.z = vector.x * matrix.m02 + vector.y * matrix.m12 + vector.z * matrix.m22 + vector.w * matrix.m32;
			result.w = vector.x * matrix.m03 + vector.y * matrix.m13 + vector.z * matrix.m23 + vector.w * matrix.m33;

			return result;
		}

		/// <summary>
		///		Multiplies a Vector4 by a scalar value.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="scalar"></param>
		/// <returns></returns>
		public static Vector4 operator * (Vector4 vector, float scalar) {
			Vector4 result = new Vector4();

			result.x = vector.x * scalar;
			result.y = vector.y * scalar;
			result.z = vector.z * scalar;
			result.w = vector.w * scalar;

			return result;
		}

        /// <summary>
        ///		User to compare two Vector4 instances for equality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true or false</returns>
        public static bool operator == (Vector4 left, Vector4 right) {
            return (left.x == right.x && 
                left.y == right.y && 
                left.z == right.z && 
                left.w == right.w);
        }

		/// <summary>
		///		Used to add a Vector3 to another Vector3.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Vector4 operator + (Vector4 left, Vector4 right) 
		{
			return new Vector4(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);
		}

		/// <summary>
		///		Used to subtract a Vector3 from another Vector3.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Vector4 operator - (Vector4 left, Vector4 right) 
		{
			return new Vector4(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);
		}

		/// <summary>
		///		Used to negate the elements of a vector.
		/// </summary>
		/// <param name="left"></param>
		/// <returns></returns>
		public static Vector4 operator - (Vector4 left) {
			return new Vector4(-left.x, -left.y, -left.z, -left.w);
		}

		/// <summary>
		/// Used to divide a scalar value by a vector.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="scalar"></param>
		/// <returns></returns>
        public static Vector4 operator / (float scalar, Vector4 right) {

			Vector4 vector = new Vector4();

			vector.x = (float)(scalar / right.x);
			vector.y = (float)(scalar / right.y);
			vector.z = (float)(scalar / right.z);
			vector.w = (float)(scalar / right.w);

			return vector;
		}

        /// <summary>
        ///		User to compare two Vector4 instances for inequality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true or false</returns>
        public static bool operator != (Vector4 left, Vector4 right) {
            return (left.x != right.x || 
                left.y != right.y || 
                left.z != right.z ||
                left.w != right.w);
        }

        /// <summary>
        ///		Used to access a Vector by index 0 = this.x, 1 = this.y, 2 = this.z, 3 = this.w.  
        /// </summary>
        /// <remarks>
        ///		Uses unsafe pointer arithmetic to reduce the code required.
        ///	</remarks>
        public float this[int index] {
            get {
				switch(index) 
				{
					case 0: return x;
					case 1: return y;
					case 2: return z;
					case 3: return w;
					default: throw new IndexOutOfRangeException();
				}
            }
            set {
               
				switch(index) 
				{
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					case 3: w = value; break;
					default: throw new IndexOutOfRangeException();
				}
            }
        }

        #endregion

        #region Object overloads

		
		/// <summary>
		///		Overrides the Object.ToString() method to provide a text representation of 
		///		a Vector3.
		/// </summary>
		/// <returns>A string representation of a vector3.</returns>
		public override string ToString() 
		{
			return string.Format("({0}, {1}, {2}, {3})", this.x, this.y, this.z, this.w);
		}

		public string ToParsableText() 
		{
			return ToString();
		}

		
		/// <summary>
		///		Overrides the Object.ToString() method to provide a text representation of 
		///		a Vector3.
		/// </summary>
		/// <returns>A string representation of a vector3.</returns>
		public string ToIntegerString() 
		{
			return string.Format("({0}, {1}, {2}, {3})", (int)this.x, (int)this.y, (int)this.z, (int)this.w);
		}
		/// <summary>
		///		Overrides the Object.ToString() method to provide a text representation of 
		///		a Vector3.
		/// </summary>
		/// <returns>A string representation of a vector3.</returns>
		public string ToString(bool shortenDecmialPlaces) 
		{
			if(shortenDecmialPlaces)
				return string.Format("({0:0.##}, {1:0.##} ,{2:0.##}, {3:0.##})", this.x, this.y, this.z, this.w);
			return ToString();
		}

		public static Vector4 Parse(string text) 
		{
			return new Vector4(text);
		}

        /// <summary>
        ///		Provides a unique hash code based on the member variables of this
        ///		class.  This should be done because the equality operators (==, !=)
        ///		have been overriden by this class.
        ///		<p/>
        ///		The standard implementation is a simple XOR operation between all local
        ///		member variables.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode() ^ this.w.GetHashCode();
        }

        /// <summary>
        ///		Compares this Vector to another object.  This should be done because the 
        ///		equality operators (==, !=) have been overriden by this class.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
           return obj is Vector4 && this == (Vector4)obj;
        }

        #endregion
    }
}
