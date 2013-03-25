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

namespace Axiom.MathLib {
    /// <summary>
    ///		Summary description for Quaternion.
    /// </summary>
    public struct Quaternion {
        #region Private member variables and constants

        const float EPSILON = 1e-03f;

        public float w, x, y, z;

        private static readonly Quaternion identityQuat = new Quaternion(1.0f, 0.0f, 0.0f, 0.0f);
        private static readonly Quaternion zeroQuat = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        private static readonly int[] next = new int[3]{ 1, 2, 0 };
		
        #endregion

        #region Constructors

        //		public Quaternion()
        //		{
        //			this.w = 1.0f;
        //		}

        /// <summary>
        ///		Creates a new Quaternion.
        /// </summary>
        public Quaternion(float w, float x, float y, float z) {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
		private Quaternion(string parsableText) 
		{
			if(parsableText == null)
				throw new ArgumentException("Text cannot be null.");
			try
			{
				if ( parsableText[0] == '[' ) //[pitch,yaw,roll], Rotation in euler angles in degrees
				{
					string[] vals = parsableText.TrimStart('[').TrimEnd(']').Split(',');
					Quaternion q = Quaternion.FromEulerAnglesInDegrees(float.Parse(vals[0]), float.Parse(vals[1]), float.Parse(vals[2]));
					w = q.w;
					x = q.x;
					y = q.y;
					z = q.z;
				} 
				else 
				{
					string[] vals = parsableText.TrimStart('(').TrimEnd(')').Split(',');
					x = float.Parse(vals[0]);
					y = float.Parse(vals[1]);
					z = float.Parse(vals[2]);
					w = float.Parse(vals[3]);
				}
			} 
			catch(Exception) 
			{
				throw new FormatException(string.Format("Could not parse Quaternion from '{0}'.",parsableText));
			}
		}

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void AssignRef(ref Quaternion result, ref Quaternion source) {

            result.w = source.w;
            result.x = source.x;
            result.y = source.y;
            result.z = source.z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="quat"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static void MultiplyRef(ref Quaternion result, ref Quaternion left, ref Quaternion right) {

            result.w = left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z;
            result.x = left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y;
            result.y = left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z;
            result.z = left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="quat"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static void MultiplyRef (ref Vector3 result, ref Quaternion quat, ref Vector3 vector) {
            // nVidia SDK implementation
            Vector3 uuv = Vector3.Zero;
            Vector3 qvec = new Vector3(quat.x, quat.y, quat.z);

			result.x = (qvec.y * vector.z) - (qvec.z * vector.y);
            result.y = (qvec.z * vector.x) - (qvec.x * vector.z);
            result.z = (qvec.x * vector.y) - (qvec.y * vector.x);

			uuv.x = (qvec.y * result.z) - (qvec.z * result.y);
            uuv.y = (qvec.z * result.x) - (qvec.x * result.z);
            uuv.z = (qvec.x * result.y) - (qvec.y * result.x);

            float f = 2.0f * quat.w;
            result.x = result.x * f + vector.x + uuv.x * 2.0f;
            result.y = result.y * f + vector.y + uuv.y * 2.0f;
            result.z = result.z * f + vector.z + uuv.z * 2.0f;
        }
        
        /// <summary>
        /// Used when a float value is multiplied by a Quaternion.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="quat"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static void MultiplyRef (ref Quaternion result, ref Quaternion quat, float scalar) {
            result.w = scalar * quat.w;
            result.x = scalar * quat.x;
            result.y = scalar * quat.y;
            result.z = scalar * quat.z;
        }

        /// <summary>
        /// Used when a Quaternion is added to another Quaternion.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static void AddRef (ref Quaternion result, ref Quaternion left, ref Quaternion right) {
            result.w = left.w + right.w;
            result.x = left.x + right.x;
            result.y = left.y + right.y;
            result.z = left.z + right.z;
        }

        /// <summary>
        ///     Negates a Quaternion, which simply returns a new Quaternion
        ///     with all components negated.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="quat"></param>
        /// <returns></returns>
        public static void NegateRef(ref Quaternion result, ref Quaternion quat) {
            result.w = -quat.w;
            result.x = -quat.x;
            result.y = -quat.y;
            result.z = -quat.z;
        }

        public static bool EqualsRef(ref Quaternion left, ref Quaternion right) {
            return (left.w == right.w && left.x == right.x && left.y == right.y && left.z == right.z);
        }

        #region Operator Overloads + CLS compliant method equivalents

        /// <summary>
        /// Used to multiply 2 Quaternions together.
        /// </summary>
        /// <remarks>
        ///		Quaternion multiplication is not communative in most cases.
        ///		i.e. p*q != q*p
        /// </remarks>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion Multiply (Quaternion left, Quaternion right) {
        	return left * right;
        }

        /// <summary>
        /// Used to multiply 2 Quaternions together.
        /// </summary>
        /// <remarks>
        ///		Quaternion multiplication is not communative in most cases.
        ///		i.e. p*q != q*p
        /// </remarks>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator * (Quaternion left, Quaternion right) {
            Quaternion q = new Quaternion();

            q.w = left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z;
            q.x = left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y;
            q.y = left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z;
            q.z = left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x;

            /*
            return new Quaternion
                (
                left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z,
                left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y,
                left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z,
                left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x
                ); */

            return q;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="quat"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Multiply (Quaternion quat, Vector3 vector) {
			return quat * vector;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="quat"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 operator*(Quaternion quat, Vector3 vector) {
            // nVidia SDK implementation
            Vector3 uv = Vector3.Zero;
            Vector3 uuv = Vector3.Zero;
            Vector3 qvec = new Vector3(quat.x, quat.y, quat.z);

			uv.x = (qvec.y * vector.z) - (qvec.z * vector.y);
            uv.y = (qvec.z * vector.x) - (qvec.x * vector.z);
            uv.z = (qvec.x * vector.y) - (qvec.y * vector.x);

			uuv.x = (qvec.y * uv.z) - (qvec.z * uv.y);
            uuv.y = (qvec.z * uv.x) - (qvec.x * uv.z);
            uuv.z = (qvec.x * uv.y) - (qvec.y * uv.x);

            float f = 2.0f * quat.w;
            uv.x = uv.x * f + vector.x + uuv.x * 2.0f;
            uv.y = uv.y * f + vector.y + uuv.y * 2.0f;
            uv.z = uv.z * f + vector.z + uuv.z * 2.0f;
            return uv;

            // get the rotation matrix of the Quaternion and multiply it times the vector
            //return quat.ToRotationMatrix() * vector;
        }

        /// <summary>
        /// Used when a float value is multiplied by a Quaternion.
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion Multiply (float scalar, Quaternion right) {
        	return scalar * right;
        }
        
        /// <summary>
        /// Used when a float value is multiplied by a Quaternion.
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator*(float scalar, Quaternion right) {
            return new Quaternion(scalar * right.w, scalar * right.x, scalar * right.y, scalar * right.z);
        }

        /// <summary>
        /// Used when a Quaternion is multiplied by a float value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Quaternion Multiply (Quaternion left, float scalar) {
        	return left * scalar;
        }
        
        /// <summary>
        /// Used when a Quaternion is multiplied by a float value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Quaternion operator*(Quaternion left, float scalar) {
            return new Quaternion(scalar * left.w, scalar * left.x, scalar * left.y, scalar * left.z);
        }

        /// <summary>
        /// Used when a Quaternion is added to another Quaternion.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion Add (Quaternion left, Quaternion right) {
        	return left + right;
        }
        
        /// <summary>
        /// Used when a Quaternion is added to another Quaternion.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator+(Quaternion left, Quaternion right) {
            return new Quaternion(left.w + right.w, left.x + right.x, left.y + right.y, left.z + right.z);
        }

        /// <summary>
        ///     Negates a Quaternion, which simply returns a new Quaternion
        ///     with all components negated.
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Quaternion operator - (Quaternion right) {
            return new Quaternion(-right.w, -right.x, -right.y, -right.z);
        }

        public static bool operator == (Quaternion left, Quaternion right) {
            return (left.w == right.w && left.x == right.x && left.y == right.y && left.z == right.z);
        }

		public static bool operator != (Quaternion left, Quaternion right) 
		{
			return (left.w != right.w || left.x != right.x || left.y != right.y || left.z != right.z);
        }

        #endregion

        #region Properties

        /// <summary>
        ///    An Identity Quaternion.
        /// </summary>
        public static Quaternion Identity {
            get { 
                return identityQuat; 
            }
        }

        /// <summary>
        ///    A Quaternion with all elements set to 0.0f;
        /// </summary>
        public static Quaternion Zero {
            get { 
                return zeroQuat; 
            }
        }

		/// <summary>
		///		Squared 'length' of this quaternion.
		/// </summary>
		public float Norm {
			get {
				return x * x + y * y + z * z + w * w;
			}
		}

        /// <summary>
        ///    Local X-axis portion of this rotation.
        /// </summary>
        public Vector3 XAxis {
            get {
                float fTx  = 2.0f * x;
                float fTy  = 2.0f * y;
                float fTz  = 2.0f * z;
                float fTwy = fTy * w;
                float fTwz = fTz * w;
                float fTxy = fTy * x;
                float fTxz = fTz * x;
                float fTyy = fTy * y;
                float fTzz = fTz * z;

                return new Vector3(1.0f - (fTyy + fTzz), fTxy + fTwz, fTxz - fTwy);
            }
        }

        /// <summary>
        ///    Local Y-axis portion of this rotation.
        /// </summary>
        public Vector3 YAxis {
            get {
                float fTx  = 2.0f * x;
                float fTy  = 2.0f * y;
                float fTz  = 2.0f * z;
                float fTwx = fTx * w;
                float fTwz = fTz * w;
                float fTxx = fTx * x;
                float fTxy = fTy * x;
                float fTyz = fTz * y;
                float fTzz = fTz * z;

                return new Vector3(fTxy - fTwz, 1.0f - (fTxx + fTzz), fTyz + fTwx);
            }
        }

        /// <summary>
        ///    Local Z-axis portion of this rotation.
        /// </summary>
        public Vector3 ZAxis {
            get {
                float fTx  = 2.0f * x;
                float fTy  = 2.0f * y;
                float fTz  = 2.0f * z;
                float fTwx = fTx * w;
                float fTwy = fTy * w;
                float fTxx = fTx * x;
                float fTxz = fTz * x;
                float fTyy = fTy * y;
                float fTyz = fTz * y;

                return new Vector3(fTxz + fTwy, fTyz - fTwx, 1.0f - (fTxx+fTyy));
            }
        }
		public float PitchInDegrees { get { return MathUtil.RadiansToDegrees(Pitch); } set { Pitch = MathUtil.DegreesToRadians(value); } }
		public float YawInDegrees { get { return MathUtil.RadiansToDegrees(Yaw); } set { Yaw = MathUtil.DegreesToRadians(value); } }
		public float RollInDegrees { get { return MathUtil.RadiansToDegrees(Roll); } set { Roll = MathUtil.DegreesToRadians(value); } }
		
		public float Pitch {
			set{
				float pitch, yaw, roll;
				ToEulerAngles(out pitch,out yaw,out roll);
				FromEulerAngles(value, yaw, roll);
			}
			get{
				
				float test = x*y + z*w;
				if (Math.Abs(test) > 0.499f) // singularity at north and south pole
					return 0f;
				return (float)Math.Atan2(2*x*w-2*y*z , 1 - 2*x*x - 2*z*z);
			}
		}


		public float Yaw {
			set{
				float pitch, yaw, roll;
				ToEulerAngles(out pitch,out yaw,out roll);
				FromEulerAngles(pitch, value, roll);
			}
			get{
				float test = x*y + z*w;
				if (Math.Abs(test) > 0.499f) // singularity at north and south pole
					return Math.Sign(test) * 2 * (float)Math.Atan2(x,w);
				return (float)Math.Atan2(2*y*w-2*x*z , 1 - 2*y*y - 2*z*z);
			}
		}
		public float Roll {
			set{
				
				float pitch, yaw, roll;
				ToEulerAngles(out pitch,out yaw,out roll);
				FromEulerAngles(pitch, yaw, value);
			}
			get{
				float test = x*y + z*w;
				if (Math.Abs(test) > 0.499f) // singularity at north and south pole
					return Math.Sign(test) * MathUtil.PI / 2;
				return (float)Math.Asin(2*test);
			}
		}

        public string EulerString {
            get {
                float pitch, yaw, roll;
                this.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                return string.Format("[{0}, {1}, {2}]", pitch, yaw, roll);
            }
        }

        #endregion

        #region Static methods

		public static Quaternion Parse(string text) 
		{
			return new Quaternion(text);
		}

	


		public static Quaternion Slerp(float time, Quaternion quatA, Quaternion quatB) {
			return Slerp(time, quatA, quatB, false);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="quatA"></param>
        /// <param name="quatB"></param>
        /// <param name="useShortestPath"></param>
        /// <returns></returns>
        public static Quaternion Slerp(float time, Quaternion quatA, Quaternion quatB, bool useShortestPath) {
            float cos = quatA.Dot(quatB);

            float angle = MathUtil.ACos(cos);

			if(MathUtil.Abs(angle) < EPSILON) {
				return quatA;
			}

            float sin = MathUtil.Sin(angle);
            float inverseSin = 1.0f / sin;
            float coeff0 = MathUtil.Sin((1.0f - time) * angle) * inverseSin;
            float coeff1 = MathUtil.Sin(time * angle) * inverseSin;

			Quaternion result;

			if(cos < 0.0f && useShortestPath) {
				coeff0 = -coeff0;
				// taking the complement requires renormalisation
				Quaternion t = coeff0 * quatA + coeff1 * quatB;
				t.Normalize();
				result = t;
			}
			else {
				result = (coeff0 * quatA + coeff1 * quatB);
			}

			return result;
        }

        public static void SlerpRef(ref Quaternion result, float time, ref Quaternion quatA, ref Quaternion quatB, bool useShortestPath) {
			float cos = quatA.w * quatB.w + quatA.x * quatB.x + quatA.y * quatB.y + quatA.z * quatB.z;

            float angle = MathUtil.ACos(cos);

			if(MathUtil.Abs(angle) < EPSILON) {
				result = quatA;
                return;
			}

            float sin = MathUtil.Sin(angle);
            float inverseSin = 1.0f / sin;
            float coeff0 = MathUtil.Sin((1.0f - time) * angle) * inverseSin;
            float coeff1 = MathUtil.Sin(time * angle) * inverseSin;

			if(cos < 0.0f && useShortestPath) {
				coeff0 = -coeff0;
				// taking the complement requires renormalisation
				result.w = coeff0 * quatA.w + coeff1 * quatB.w;
				result.x = coeff0 * quatA.x + coeff1 * quatB.x;
				result.y = coeff0 * quatA.y + coeff1 * quatB.y;
				result.z = coeff0 * quatA.z + coeff1 * quatB.z;
			    float factor = 1.0f / MathUtil.Sqrt(result.Norm);
                result.w *= factor;
                result.x *= factor;
                result.y *= factor;
                result.z *= factor;
			}
			else {
				result.w = coeff0 * quatA.w + coeff1 * quatB.w;
				result.x = coeff0 * quatA.x + coeff1 * quatB.x;
				result.y = coeff0 * quatA.y + coeff1 * quatB.y;
				result.z = coeff0 * quatA.z + coeff1 * quatB.z;
			}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="time"></param>
        /// <param name="quatA"></param>
        /// <param name="quatB"></param>
        /// <param name="useShortestPath"></param>
        /// <returns></returns>
        public static void NlerpRef(ref Quaternion result, float time, ref Quaternion quatA, ref Quaternion quatB, bool shortestPath) {
			float cos = quatA.w * quatB.w + quatA.x * quatB.x + quatA.y * quatB.y + quatA.z * quatB.z;
			if (cos < 0.0f && shortestPath) {
				result.w = quatA.w + time * (- quatB.w - quatA.w);
				result.x = quatA.x + time * (- quatB.x - quatA.x);
				result.y = quatA.y + time * (- quatB.y - quatA.y);
				result.z = quatA.z + time * (- quatB.z - quatA.z);
			}
			else {
				result.w = quatA.w + time * (quatB.w - quatA.w);
				result.x = quatA.x + time * (quatB.x - quatA.x);
				result.y = quatA.y + time * (quatB.y - quatA.y);
				result.z = quatA.z + time * (quatB.z - quatA.z);
			}
			float factor = 1.0f / MathUtil.Sqrt(result.x * result.x + result.y * result.y + result.z * result.z + result.w * result.w);

			result.w = result.w * factor;
			result.x = result.x * factor;
			result.y = result.y * factor;
			result.z = result.z * factor;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="quatA"></param>
        /// <param name="quatB"></param>
        /// <param name="useShortestPath"></param>
        /// <returns></returns>
		public static Quaternion Nlerp(float time, Quaternion quatA, Quaternion quatB, bool shortestPath) {
			Quaternion result;
			float cos = quatA.Dot(quatB);
			if (cos < 0.0f && shortestPath) {
				result = quatA + time * ((-1 * quatB) + (-1 * quatA));
			}
			else {
				result = quatA + time * (quatB + (-1 * quatA));
			}
			result.Normalize();
			return result;
		}

        /// <summary>
        /// Creates a Quaternion from a supplied angle and axis.
        /// </summary>
        /// <param name="angle">Value of an angle in radians.</param>
        /// <param name="axis">Arbitrary axis vector.</param>
        /// <returns></returns>
        public static Quaternion FromAngleAxis(float angle, Vector3 axis) {
            Quaternion quat = new Quaternion();

            float halfAngle = 0.5f * angle;
            float sin = MathUtil.Sin(halfAngle);

            quat.w = MathUtil.Cos(halfAngle);
            quat.x = sin * axis.x; 
            quat.y = sin * axis.y; 
            quat.z = sin * axis.z; 

            return quat;
        }

		public static Quaternion Squad(float t, Quaternion p, Quaternion a, Quaternion b, Quaternion q) {
			return Squad(t, p, a, b, q, false);
		}

        /// <summary>
        ///		Performs spherical quadratic interpolation.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Quaternion Squad(float t, Quaternion p, Quaternion a, Quaternion b, Quaternion q, bool useShortestPath) {
            float slerpT = 2.0f * t * (1.0f - t);

            // use spherical linear interpolation
            Quaternion slerpP = Slerp(t, p, q, useShortestPath);
            Quaternion slerpQ = Slerp(t, a, b);

            // run another Slerp on the results of the first 2, and return the results
            return Slerp(slerpT, slerpP, slerpQ);
        }

        #endregion

        #region Public methods

		#region Euler Angles
		public Vector3 ToEulerAnglesInDegrees() {
			float pitch, yaw, roll;
			ToEulerAngles(out pitch, out yaw, out roll);
			return new Vector3(MathUtil.RadiansToDegrees(pitch), MathUtil.RadiansToDegrees(yaw), MathUtil.RadiansToDegrees(roll));
		}
		public Vector3 ToEulerAngles() {
			float pitch, yaw, roll;
			ToEulerAngles(out pitch, out yaw, out roll);
			return new Vector3(pitch, yaw, roll);
		}
		public void ToEulerAnglesInDegrees(out float pitch, out float yaw, out float roll) {
			ToEulerAngles(out pitch,out yaw,out roll);
			pitch = MathUtil.RadiansToDegrees(pitch);
			yaw = MathUtil.RadiansToDegrees(yaw);
			roll = MathUtil.RadiansToDegrees(roll);
		}
		public void ToEulerAngles(out float pitch, out float yaw, out float roll) {
			
			float halfPi = (float)Math.PI / 2;
			float test = x*y + z*w;
			if (test > 0.499f) { // singularity at north pole
				yaw = 2 * (float)Math.Atan2(x,w);
				roll = halfPi;
				pitch = 0;
			} else if (test < -0.499f) { // singularity at south pole
				yaw = -2 * (float)Math.Atan2(x,w);
				roll = -halfPi;
				pitch = 0;
			} else {
				float sqx = x*x;
				float sqy = y*y;
				float sqz = z*z;
				yaw = (float)Math.Atan2(2*y*w-2*x*z , 1 - 2*sqy - 2*sqz);
				roll = (float)Math.Asin(2*test);
				pitch = (float)Math.Atan2(2*x*w-2*y*z , 1 - 2*sqx - 2*sqz);
			}
			
			if(Math.Abs(pitch) <= float.Epsilon)
				pitch = 0f;
			if(Math.Abs(yaw) <= float.Epsilon)
				yaw = 0f;
			if(Math.Abs(roll) <= float.Epsilon)
				roll = 0f;
		}
		public static Quaternion FromEulerAnglesInDegrees(float pitch, float yaw, float roll) {
			return FromEulerAngles( MathUtil.DegreesToRadians(pitch), MathUtil.DegreesToRadians(yaw), MathUtil.DegreesToRadians(roll) );
		}

		/// <summary>
		/// Combines the euler angles in the order roll, pitch, yaw to create a rotation quaternion.
        /// This means that if you wrote this in standard math form, it would be Qy * Qp * Qr.
		/// </summary>
		/// <param name="pitch"></param>
		/// <param name="yaw"></param>
		/// <param name="roll"></param>
		/// <returns></returns>
		public static Quaternion FromEulerAngles(float pitch, float yaw, float roll) {
			return Quaternion.FromAngleAxis(yaw, Vector3.UnitY)
				* Quaternion.FromAngleAxis(pitch, Vector3.UnitX)
                * Quaternion.FromAngleAxis(roll, Vector3.UnitZ);
			
			/*TODO: Debug
			//Equation from http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
			//heading
			
			float c1 = (float)Math.Cos(yaw/2);
			float s1 = (float)Math.Sin(yaw/2);
			//attitude
			float c2 = (float)Math.Cos(roll/2);
			float s2 = (float)Math.Sin(roll/2);
			//bank
			float c3 = (float)Math.Cos(pitch/2);
			float s3 = (float)Math.Sin(pitch/2);
			float c1c2 = c1*c2;
			float s1s2 = s1*s2;

			float w =c1c2*c3 - s1s2*s3;
			float x =c1c2*s3 + s1s2*c3;
			float y =s1*c2*c3 + c1*s2*s3;
			float z =c1*s2*c3 - s1*c2*s3;
			return new Quaternion(w,x,y,z);*/
		}

		#endregion
		
        /// <summary>
        /// Performs a Dot Product operation on 2 Quaternions.
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public float Dot(Quaternion quat) {
            return this.w * quat.w + this.x * quat.x + this.y * quat.y + this.z * quat.z;
        }

		/// <summary>
		///		Normalizes elements of this quaterion to the range [0,1].
		/// </summary>
		public void Normalize() {
			float factor = 1.0f / MathUtil.Sqrt(this.Norm);

			w = w * factor;
			x = x * factor;
			y = y * factor;
			z = z * factor;
		}

        /// <summary>
        ///    
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public void ToAngleAxis(ref float angle, ref Vector3 axis) {
            // The quaternion representing the rotation is
            //   q = cos(A/2)+sin(A/2)*(x*i+y*j+z*k)

            float sqrLength = x * x + y * y + z * z;
           
            if(sqrLength > EPSILON * EPSILON) {
                angle = 2.0f * MathUtil.ACos(w);
                float invLength = MathUtil.InvSqrt(sqrLength);
                axis.x = x * invLength;
                axis.y = y * invLength;
                axis.z = z * invLength;
            }
            else {
                angle = 0.0f;
                axis.x = 1.0f;
                axis.y = 0.0f;
                axis.z = 0.0f;
            }
        }

        /// <summary>
        ///    Find the rotation from the orientation; scales it and
        ///    assigns it to the result; and assigns the translation
        ///    to the result.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="orientation"></param>
        /// <param name="scale"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public static void ToRotationMatrixPlusTranslationRef(ref Matrix4 result, ref Quaternion orientation, ref Vector3 scale, ref Vector3 translation) {
            float tx  = 2.0f * orientation.x;
            float ty  = 2.0f * orientation.y;
            float tz  = 2.0f * orientation.z;
            float twx = tx * orientation.w;
            float twy = ty * orientation.w;
            float twz = tz * orientation.w;
            float txx = tx * orientation.x;
            float txy = ty * orientation.x;
            float txz = tz * orientation.x;
            float tyy = ty * orientation.y;
            float tyz = tz * orientation.y;
            float tzz = tz * orientation.z;

            result.m00 = (1.0f-(tyy+tzz)) * scale.x;
            result.m01 = (txy-twz) * scale.y;
            result.m02 = (txz+twy) * scale.z;
            result.m10 = (txy+twz) * scale.x;
            result.m11 = (1.0f-(txx+tzz)) * scale.y;
            result.m12 = (tyz-twx) * scale.z;
            result.m20 = (txz-twy) * scale.x;
            result.m21 = (tyz+twx) * scale.y;
            result.m22 = (1.0f-(txx+tyy)) * scale.z;

            result.m03 = translation.x;
            result.m13 = translation.y;
            result.m23 = translation.z;
            result.m33 = 1.0f;
        }

        /// <summary>
        /// Gets a 3x3 rotation matrix from this Quaternion.
        /// </summary>
        /// <returns></returns>
        public Matrix3 ToRotationMatrix() {
            Matrix3 rotation = new Matrix3();

            float tx  = 2.0f * this.x;
            float ty  = 2.0f * this.y;
            float tz  = 2.0f * this.z;
            float twx = tx * this.w;
            float twy = ty * this.w;
            float twz = tz * this.w;
            float txx = tx * this.x;
            float txy = ty * this.x;
            float txz = tz * this.x;
            float tyy = ty * this.y;
            float tyz = tz * this.y;
            float tzz = tz * this.z;

            rotation.m00 = 1.0f-(tyy+tzz);
            rotation.m01 = txy-twz;
            rotation.m02 = txz+twy;
            rotation.m10 = txy+twz;
            rotation.m11 = 1.0f-(txx+tzz);
            rotation.m12 = tyz-twx;
            rotation.m20 = txz-twy;
            rotation.m21 = tyz+twx;
            rotation.m22 = 1.0f-(txx+tyy);

            return rotation;
        }

        public void ToRotationMatrixRef(out Matrix3 rotation)
        {
            float tx = 2.0f * this.x;
            float ty = 2.0f * this.y;
            float tz = 2.0f * this.z;
            float twx = tx * this.w;
            float twy = ty * this.w;
            float twz = tz * this.w;
            float txx = tx * this.x;
            float txy = ty * this.x;
            float txz = tz * this.x;
            float tyy = ty * this.y;
            float tyz = tz * this.y;
            float tzz = tz * this.z;

            rotation.m00 = 1.0f - (tyy + tzz);
            rotation.m01 = txy - twz;
            rotation.m02 = txz + twy;
            rotation.m10 = txy + twz;
            rotation.m11 = 1.0f - (txx + tzz);
            rotation.m12 = tyz - twx;
            rotation.m20 = txz - twy;
            rotation.m21 = tyz + twx;
            rotation.m22 = 1.0f - (txx + tyy);
        }

        /// <summary>
        /// Computes the inverse of a Quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion Inverse() {
            float norm = this.w * this.w + this.x * this.x + this.y * this.y + this.z * this.z;
            if ( norm > 0.0f ) {
                float inverseNorm = 1.0f / norm;
                return new Quaternion(this.w * inverseNorm, -this.x * inverseNorm, -this.y * inverseNorm, -this.z * inverseNorm);
            }
            else {
                // return an invalid result to flag the error
                return Quaternion.Zero;
            }
        }

        public static void InverseRef(ref Quaternion quat, ref Quaternion result) {
            float norm = quat.w * quat.w + quat.x * quat.x + quat.y * quat.y + quat.z * quat.z;
            if ( norm > 0.0f ) {
                float inverseNorm = 1.0f / norm;
                result.w = quat.w * inverseNorm;
                result.x = -quat.x * inverseNorm;
                result.y = -quat.y * inverseNorm;
                result.z = -quat.z * inverseNorm;
            }
            else {
                result.w = 0;
                result.x = 0;
                result.y = 0;
                result.z = 0;
            }
        }
        
        /// <summary>
        ///   Variant of Inverse() that is only valid for unit quaternions.
        /// </summary>
        /// <returns></returns>
        public Quaternion UnitInverse() {
            return new Quaternion(w, -x, -y, -z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        public void ToAxes (out Vector3 xAxis, out Vector3 yAxis, out Vector3 zAxis) {
            xAxis = new Vector3();
            yAxis = new Vector3();
            zAxis = new Vector3();

            Matrix3 rotation = this.ToRotationMatrix();

            xAxis.x = rotation.m00;
            xAxis.y = rotation.m10;
            xAxis.z = rotation.m20;

            yAxis.x = rotation.m01;
            yAxis.y = rotation.m11;
            yAxis.z = rotation.m21;

            zAxis.x = rotation.m02;
            zAxis.y = rotation.m12;
            zAxis.z = rotation.m22;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        public void FromAxes(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis) {
            Matrix3 rotation = new Matrix3();

            rotation.m00 = xAxis.x;
            rotation.m10= xAxis.y;
            rotation.m20 = xAxis.z;

            rotation.m01 = yAxis.x;
            rotation.m11 = yAxis.y;
            rotation.m21 = yAxis.z;

            rotation.m02 = zAxis.x;
            rotation.m12 = zAxis.y;
            rotation.m22 = zAxis.z;

			// set this quaternions values from the rotation matrix built
            FromRotationMatrix(rotation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        public void FromRotationMatrix(Matrix3 matrix) {
            // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
            // article "Quaternion Calculus and Fast Animation".

            float trace = matrix.m00 + matrix.m11 + matrix.m22;

            float root = 0.0f;

            if ( trace > 0.0f ) {
                // |this.w| > 1/2, may as well choose this.w > 1/2
                root = MathUtil.Sqrt(trace + 1.0f);  // 2w
                this.w = 0.5f * root;
				
                root = 0.5f / root;  // 1/(4w)

                this.x = (matrix.m21 - matrix.m12) * root;
                this.y = (matrix.m02 - matrix.m20) * root;
                this.z = (matrix.m10 - matrix.m01) * root;
            }
            else {
                // |this.w| <= 1/2

                int i = 0;
                if ( matrix.m11 > matrix.m00 )
                    i = 1;
                if ( matrix.m22 > matrix[i,i] )
                    i = 2;

                int j = next[i];
                int k = next[j];

                root = MathUtil.Sqrt(matrix[i,i] - matrix[j,j] - matrix[k,k] + 1.0f);

                unsafe {
                    fixed(float* apkQuat = &this.x) {
                        apkQuat[i] = 0.5f * root;
                        root = 0.5f / root;
					
                        this.w = (matrix[k,j] - matrix[j,k]) * root;
					
                        apkQuat[j] = (matrix[j,i] + matrix[i,j]) * root;
                        apkQuat[k] = (matrix[k,i] + matrix[i,k]) * root;
                    }
                }
            }
        }

        /// <summary>
        ///		Calculates the logarithm of a Quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion Log() {
            // BLACKBOX: Learn this
            // If q = cos(A)+sin(A)*(x*i+y*j+z*k) where (x,y,z) is unit length, then
            // log(q) = A*(x*i+y*j+z*k).  If sin(A) is near zero, use log(q) =
            // sin(A)*(x*i+y*j+z*k) since sin(A)/A has limit 1.

            // start off with a zero quat
            Quaternion result = Quaternion.Zero;

            if(MathUtil.Abs(w) < 1.0f) {
                float angle = MathUtil.ACos(w);
                float sin = MathUtil.Sin(angle);

                if(MathUtil.Abs(sin) >= EPSILON) {
                    float coeff = angle / sin;
                    result.x = coeff * x;
                    result.y = coeff * y;
                    result.z = coeff * z;
                }
                else {
                    result.x = x;
                    result.y = y;
                    result.z = z;
                }
            }

            return result;
        }

        /// <summary>
        ///		Calculates the Exponent of a Quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion Exp() {
            // If q = A*(x*i+y*j+z*k) where (x,y,z) is unit length, then
            // exp(q) = cos(A)+sin(A)*(x*i+y*j+z*k).  If sin(A) is near zero,
            // use exp(q) = cos(A)+A*(x*i+y*j+z*k) since A/sin(A) has limit 1.

            float angle = MathUtil.Sqrt(x * x + y * y + z * z);
            float sin = MathUtil.Sin(angle);

            // start off with a zero quat
            Quaternion result = Quaternion.Zero;

            result.w = MathUtil.Cos(angle);

            if ( MathUtil.Abs(sin) >= EPSILON ) {
                float coeff = sin / angle;

                result.x = coeff * x;
                result.y = coeff * y;
                result.z = coeff * z;
            }
            else {
                result.x = x;
                result.y = y;
                result.z = z;
            }

            return result;
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
        public override int GetHashCode() {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }
        public override bool Equals(object obj) {
            return obj is Quaternion && this == (Quaternion)obj;
        }
		
        public bool Equals(Quaternion obj, float tolerance) {
            return (Math.Abs(x - obj.x) < tolerance && Math.Abs(y - obj.y) < tolerance &&
					Math.Abs(z - obj.z) < tolerance && Math.Abs(w - obj.w) < tolerance);
        }

        #endregion	
    }
}
