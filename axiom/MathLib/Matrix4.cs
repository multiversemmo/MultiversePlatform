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
using System.Text;

namespace Axiom.MathLib {
    /// <summary>
    ///		Class encapsulating a standard 4x4 homogenous matrix.
    /// </summary>
    /// <remarks>
    ///		The engine uses column vectors when applying matrix multiplications,
    ///		This means a vector is represented as a single column, 4-row
    ///		matrix. This has the effect that the tranformations implemented
    ///		by the matrices happens right-to-left e.g. if vector V is to be
    ///		transformed by M1 then M2 then M3, the calculation would be
    ///		M3 * M2 * M1 * V. The order that matrices are concatenated is
    ///		vital since matrix multiplication is not cummatative, i.e. you
    ///		can get a different result if you concatenate in the wrong order.
    /// 		<p/>
    ///		The use of column vectors and right-to-left ordering is the
    ///		standard in most mathematical texts, and is the same as used in
    ///		OpenGL. It is, however, the opposite of Direct3D, which has
    ///		inexplicably chosen to differ from the accepted standard and uses
    ///		row vectors and left-to-right matrix multiplication.
    ///		<p/>
    ///		The engine deals with the differences between D3D and OpenGL etc.
    ///		internally when operating through different render systems. The engine
    ///		users only need to conform to standard maths conventions, i.e.
    ///		right-to-left matrix multiplication, (The engine transposes matrices it
    ///		passes to D3D to compensate).
    ///		<p/>
    ///		The generic form M * V which shows the layout of the matrix 
    ///		entries is shown below:
    ///		<p/>
    ///		| m[0][0]  m[0][1]  m[0][2]  m[0][3] |   {x}
    ///		| m[1][0]  m[1][1]  m[1][2]  m[1][3] |   {y}
    ///		| m[2][0]  m[2][1]  m[2][2]  m[2][3] |   {z}
    ///		| m[3][0]  m[3][1]  m[3][2]  m[3][3] |   {1}
    ///	</remarks>
    ///	<ogre headerVersion="1.18" sourceVersion="1.8" />
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4 {
        #region Member variables

        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;

        private readonly static Matrix4 zeroMatrix = new Matrix4(	
			0,0,0,0,
            0,0,0,0,
            0,0,0,0,
            0,0,0,0);
        private readonly static Matrix4 identityMatrix = new Matrix4(	
			1,0,0,0,
            0,1,0,0,
            0,0,1,0,
            0,0,0,1);

		private readonly static Matrix4 clipSpace2dToImageSpace = new Matrix4( 
			0.5f,		0,		0,		0.5f, 
			0,		-0.5f,		0,		0.5f, 
			0,			0,		1,		0,
			0,			0,		0,		1);

        #endregion

        #region Constructors

        /// <summary>
        ///		Creates a new Matrix4 with all the specified parameters.
        /// </summary>
        public Matrix4(	float m00, float m01, float m02, float m03, 
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33) {
            this.m00 = m00; this.m01 = m01; this.m02 = m02; this.m03 = m03;
            this.m10 = m10; this.m11 = m11; this.m12 = m12; this.m13 = m13;
            this.m20 = m20; this.m21 = m21; this.m22 = m22; this.m23 = m23;
            this.m30 = m30; this.m31 = m31; this.m32 = m32; this.m33 = m33;
        }

        #endregion

        #region Static properties
        /// <summary>
        ///    Returns a matrix with the following form:
        ///    | 1,0,0,0 |
        ///    | 0,1,0,0 |
        ///    | 0,0,1,0 |
        ///    | 0,0,0,1 |
        /// </summary>
        public static Matrix4 Identity {
            get { 
				return identityMatrix; 
			}
        }

        /// <summary>
        ///    Returns a matrix with all elements set to 0.
        /// </summary>
        public static Matrix4 Zero {
            get { 
				return zeroMatrix; 
			}
        }

		public static Matrix4 ClipSpace2DToImageSpace {
			get {
				return clipSpace2dToImageSpace;
			}
		}

        /// <summary>
        ///    Check whether or not the matrix is affine matrix.
        /// </summary>
        /// <remarks>
        ///    An affine matrix is a 4x4 matrix with row 3 equal to (0, 0, 0, 1),
        ///    e.g. no projective coefficients.
        /// </remarks>
        public bool IsAffine {
            get {
                return m30 == 0 && m31 == 0 && m32 == 0 && m33 == 1;
            }
        }

        public Matrix4 InverseAffine {
            get {
                Debug.Assert(IsAffine);

                float t00 = m22 * m11 - m21 * m12;
                float t10 = m20 * m12 - m22 * m10;
                float t20 = m21 * m10 - m20 * m11;

                float invDet = 1f / (m00 * t00 + m01 * t10 + m02 * t20);

                t00 *= invDet; t10 *= invDet; t20 *= invDet;

                float tm00 = m00 * invDet;
                float tm01 = m01 * invDet;
                float tm02 = m02 * invDet;

                float r00 = t00;
                float r01 = tm02 * m21 - tm01 * m22;
                float r02 = tm01 * m12 - tm02 * m11;

                float r10 = t10;
                float r11 = tm00 * m22 - tm02 * m20;
                float r12 = tm02 * m10 - tm00 * m12;

                float r20 = t20;
                float r21 = tm01 * m20 - tm00 * m21;
                float r22 = tm00 * m11 - tm01 * m10;

                float r03 = - (r00 * m03 + r01 * m13 + r02 * m23);
                float r13 = - (r10 * m03 + r11 * m13 + r12 * m23);
                float r23 = - (r20 * m03 + r21 * m13 + r22 * m23);

                return new Matrix4(r00, r01, r02, r03,
                                   r10, r11, r12, r13,
                                   r20, r21, r22, r23,
                                   0,   0,   0,   1);
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        ///		Gets/Sets the Translation portion of the matrix.
        ///		| 0 0 0 Tx|
        ///		| 0 0 0 Ty|
        ///		| 0 0 0 Tz|
        ///		| 0 0 0  1 |
        /// </summary>
        public Vector3 Translation {
            get {
                return new Vector3(this.m03, this.m13, this.m23);
            }
            set {
                this.m03 = value.x;
                this.m13 = value.y;
                this.m23 = value.z;
            }
        }

        /// <summary>
        ///	    Gets/Sets the Scale portion of the matrix.
        ///		  |Sx  0  0  0 |
        ///		  | 0 Sy  0  0 |
        ///		  | 0  0 Sz  0 |
        ///		  | 0  0  0  1 |
        ///     The get version of this is more complex, and can handle
        ///     matrices that already have a rotation.
        ///     The set version will not work correctly if there is a 
        ///     rotation already.
        /// </summary>
        public Vector3 Scale {
            get {
                // return new Vector3(this.m00, this.m11, this.m22);
                Vector3 scale = new Vector3();
                scale.x = (float)Math.Sqrt(this.m00 * this.m00 + this.m01 * this.m01 + this.m02 * this.m02);
                scale.y = (float)Math.Sqrt(this.m10 * this.m10 + this.m11 * this.m11 + this.m12 * this.m12);
                scale.z = (float)Math.Sqrt(this.m20 * this.m20 + this.m21 * this.m21 + this.m22 * this.m22);
                if (this.Determinant < 0) // is this a reflecting scale?
                    scale.x = -1;
                return scale;
            }
            set {
                this.m00 = value.x;
                this.m11 = value.y;
                this.m22 = value.z;
            }
        }

        public Quaternion Rotation {
            get {
                Quaternion rotate;
                Vector3 translate, scale;
                DecomposeMatrix(ref this, out translate, out rotate, out scale);
                return rotate;
            }
        }
        #endregion

        #region Static utility methods
        public static void DecomposeMatrix(ref Matrix4 transform, out Vector3 translate, out Quaternion rotate, out Vector3 scale) {
            Matrix3 tmp = transform.GetMatrix3();
            translate = transform.Translation;
            scale.x = (float)Math.Sqrt(tmp.m00 * tmp.m00 + tmp.m01 * tmp.m01 + tmp.m02 * tmp.m02);
            scale.y = (float)Math.Sqrt(tmp.m10 * tmp.m10 + tmp.m11 * tmp.m11 + tmp.m12 * tmp.m12);
            scale.z = (float)Math.Sqrt(tmp.m20 * tmp.m20 + tmp.m21 * tmp.m21 + tmp.m22 * tmp.m22);
            if (tmp.Determinant < 0) // is this a reflecting scale?
                scale.x *= -1;
            tmp.m00 /= scale.x;
            tmp.m01 /= scale.x;
            tmp.m02 /= scale.x;
            tmp.m10 /= scale.y;
            tmp.m11 /= scale.y;
            tmp.m12 /= scale.y;
            tmp.m20 /= scale.z;
            tmp.m21 /= scale.z; 
            tmp.m22 /= scale.z;
            rotate = Quaternion.Identity;
            rotate.FromRotationMatrix(tmp);
            if (Math.Abs(1.0 - rotate.Norm) > .01f) {
                string msg = string.Format("Probable non-uniform scale factor on rotation matrix; Norm = {0}", rotate.Norm);
                throw new Exception(msg);
            }
            if (Math.Abs(1.0 - rotate.Norm) > .001f)
                Trace.TraceWarning("Possible non-uniform scale factor on rotation matrix");
            // rv.Normalize();
        }
        /// <summary>
        ///   Method for building a Matrix4 from orientation / scale / position. 
        /// </summary>
        /// <remarks>
        ///	  Transform is performed in the order scale, rotate, translation, i.e. translation is independent
        ///	  of orientation axes, scale does not affect size of translation, rotation and scaling are always
        ///	  centered on the origin.
        ///	</remarks>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="orientation"></param>
        /// <param name="destMatrix">object that will contain the transform</param>
        /// <returns></returns>
        public static void MakeTransform(Vector3 position, Vector3 scale, Quaternion orientation, out Matrix4 destMatrix)
        {
            destMatrix = Matrix4.Identity;

            // Ordering:
            //    1. Scale
            //    2. Rotate
            //    3. Translate

            Matrix3 rot3x3;
            Matrix3 scale3x3;
            rot3x3 = orientation.ToRotationMatrix();
            scale3x3 = Matrix3.Zero;
            scale3x3.m00 = scale.x;
            scale3x3.m11 = scale.y;
            scale3x3.m22 = scale.z;

            destMatrix = rot3x3 * scale3x3;
            destMatrix.Translation = position;
        }
        #endregion

        #region Public methods

        /// <summary>
        ///    Returns a 3x3 portion of this 4x4 matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix3 GetMatrix3() {
            return
                new Matrix3(
                    this.m00, this.m01, this.m02,
                    this.m10, this.m11, this.m12,
                    this.m20, this.m21, this.m22);
        }

        /// <summary>
        ///    Returns an inverted 4d matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix4 Inverse() {
            return Adjoint() * (1.0f / this.Determinant);
        }

        /// <summary>
        ///    Swap the rows of the matrix with the columns.
        /// </summary>
        /// <returns>A transposed Matrix.</returns>
        public Matrix4 Transpose() {
            return new Matrix4(this.m00, this.m10, this.m20, this.m30,
                this.m01, this.m11, this.m21, this.m31,
                this.m02, this.m12, this.m22, this.m32,
                this.m03, this.m13, this.m23, this.m33);
        }
    
        #endregion

        /// <summary>
        ///		Used to multiply (concatenate) two 4x4 Matrices.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void AssignRef (ref Matrix4 result, ref Matrix4 source) {
            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = source.m02;
            result.m03 = source.m03;
                                   
            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = source.m12;
            result.m13 = source.m13;
                                   
            result.m20 = source.m20;
            result.m21 = source.m21;
            result.m22 = source.m22;
            result.m23 = source.m23;
                                   
            result.m30 = source.m30;
            result.m31 = source.m31;
            result.m32 = source.m32;
            result.m33 = source.m33;
        }
        
        /// <summary>
        ///		Used to multiply (concatenate) two 4x4 Matrices.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static void MultiplyRef (ref Matrix4 result, ref Matrix4 left, ref Matrix4 right) {
            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20 + left.m03 * right.m30;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21 + left.m03 * right.m31;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22 + left.m03 * right.m32;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23 + left.m03 * right.m33;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20 + left.m13 * right.m30;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20 + left.m23 * right.m30;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32;
            result.m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20 + left.m33 * right.m30;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32;
            result.m33 = left.m30 * right.m03 + left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33;
        }
        

        /// <summary>
        ///		Transforms the given 3-D vector by the matrix, projecting the 
        ///		result back into <i>w</i> = 1.
        ///		<p/>
        ///		This means that the initial <i>w</i> is considered to be 1.0,
        ///		and then all the tree elements of the resulting 3-D vector are
        ///		divided by the resulting <i>w</i>.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="matrix">A Matrix4.</param>
        /// <param name="vector">A Vector3.</param>
        /// <returns></returns>
        public static void MultiplyRef (ref Vector3 result, Matrix4 matrix, ref Vector3 vector) {
            float inverseW = 1.0f / ( matrix.m30 + matrix.m31 + matrix.m32 + matrix.m33 );

            result.x = ( (matrix.m00 * vector.x) + (matrix.m01 * vector.y) + (matrix.m02 * vector.z) + matrix.m03 ) * inverseW;
            result.y = ( (matrix.m10 * vector.x) + (matrix.m11 * vector.y) + (matrix.m12 * vector.z) + matrix.m13 ) * inverseW;
            result.z = ( (matrix.m20 * vector.x) + (matrix.m21 * vector.y) + (matrix.m22 * vector.z) + matrix.m23 ) * inverseW;
        }

		/// <summary>
		///		Transforms a plane using the specified transform.
		/// </summary>
        /// <param name="result"></param>
		/// <param name="matrix">Transformation matrix.</param>
		/// <param name="plane">Plane to transform.</param>
		/// <returns></returns>
        public static void MultiplyRef(ref Plane result, ref Matrix4 left, ref Plane plane) {

			Vector3 planeNormal = plane.Normal;

			result.Normal = new Vector3(
				left.m00 * planeNormal.x + left.m01 * planeNormal.y + left.m02 * planeNormal.z,
				left.m10 * planeNormal.x + left.m11 * planeNormal.y + left.m12 * planeNormal.z,
				left.m20 * planeNormal.x + left.m21 * planeNormal.y + left.m22 * planeNormal.z);

			Vector3 pt = planeNormal * -plane.D;
			pt = left * pt;

			result.D = -pt.Dot(result.Normal);
		}
        
        /// <summary>
        ///		Used to multiply a Matrix4 object by a scalar value..
        /// </summary>
        /// <param name="result"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static void MultiplyRef (ref Matrix4 result, ref Matrix4 left, float scalar) {
            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;
            result.m03 = left.m03 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;
            result.m13 = left.m13 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;
            result.m23 = left.m23 * scalar;

            result.m30 = left.m30 * scalar;
            result.m31 = left.m31 * scalar;
            result.m32 = left.m32 * scalar;
            result.m33 = left.m33 * scalar;
        }

        /// <summary>
        ///		Used to add two matrices together.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static void AddRef (ref Matrix4 result, ref Matrix4 left, ref Matrix4 right) {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03 + right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13 + right.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = left.m23 + right.m23;

            result.m30 = left.m30 + right.m30;
            result.m31 = left.m31 + right.m31;
            result.m32 = left.m32 + right.m32;
            result.m33 = left.m33 + right.m33;
        }
        
        /// <summary>
        ///		Used to subtract two matrices.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static void SubtractRef (ref Matrix4 result, ref Matrix4 left, ref Matrix4 right) {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03 - right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13 - right.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = left.m23 - right.m23;

            result.m30 = left.m30 - right.m30;
            result.m31 = left.m31 - right.m31;
            result.m32 = left.m32 - right.m32;
            result.m33 = left.m33 - right.m33;
        }
        
        /// <summary>
        /// Compares two Matrix4 instances for equality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true if the Matrix 4 instances are equal, false otherwise.</returns>
        public static bool EqualsRef(ref Matrix4 left, ref Matrix4 right) {
            if( 
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23 &&
                left.m30 == right.m30 && left.m31 == right.m31 && left.m32 == right.m32 && left.m33 == right.m33 )
                return true;

            return false;
        }

        /// <summary>
        ///		Used to allow assignment from a Matrix3 to a Matrix4 object.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static void FromMatrix3Ref(ref Matrix4 result, ref Matrix3 right) {
            result.m00 = right.m00; result.m01 = right.m01; result.m02 = right.m02;
            result.m10 = right.m10; result.m11 = right.m11; result.m12 = right.m12;
            result.m20 = right.m20; result.m21 = right.m21; result.m22 = right.m22;	
        }
        
        #region Operator overloads + CLS compliant method equivalents

        /// <summary>
        ///		Used to multiply (concatenate) two 4x4 Matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 Multiply (Matrix4 left, Matrix4 right) {
        	return left * right;
        }
        
        /// <summary>
        ///		Used to multiply (concatenate) two 4x4 Matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 operator * (Matrix4 left, Matrix4 right) {
            Matrix4 result = new Matrix4();

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20 + left.m03 * right.m30;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21 + left.m03 * right.m31;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22 + left.m03 * right.m32;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23 + left.m03 * right.m33;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20 + left.m13 * right.m30;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20 + left.m23 * right.m30;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32;
            result.m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20 + left.m33 * right.m30;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32;
            result.m33 = left.m30 * right.m03 + left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33;

            return result;
        }

        /// <summary>
        ///		Transforms the given 3-D vector by the matrix, projecting the 
        ///		result back into <i>w</i> = 1.
        ///		<p/>
        ///		This means that the initial <i>w</i> is considered to be 1.0,
        ///		and then all the tree elements of the resulting 3-D vector are
        ///		divided by the resulting <i>w</i>.
        /// </summary>
        /// <param name="matrix">A Matrix4.</param>
        /// <param name="vector">A Vector3.</param>
        /// <returns>A new vector.</returns>
        public static Vector3 Multiply (Matrix4 matrix, Vector3 vector) {
        	return matrix * vector;
        }

		/// <summary>
		///		Transforms a plane using the specified transform.
		/// </summary>
		/// <param name="matrix">Transformation matrix.</param>
		/// <param name="plane">Plane to transform.</param>
		/// <returns>A transformed plane.</returns>
		public static Plane Multiply(Matrix4 matrix, Plane plane) {
			return matrix * plane;
		}
        
        /// <summary>
        ///		Transforms the given 3-D vector by the matrix, projecting the 
        ///		result back into <i>w</i> = 1.
        ///		<p/>
        ///		This means that the initial <i>w</i> is considered to be 1.0,
        ///		and then all the tree elements of the resulting 3-D vector are
        ///		divided by the resulting <i>w</i>.
        /// </summary>
        /// <param name="matrix">A Matrix4.</param>
        /// <param name="vector">A Vector3.</param>
        /// <returns>A new vector.</returns>
        public static Vector3 operator * (Matrix4 matrix, Vector3 vector) {
            Vector3 result = new Vector3();

            float inverseW = 1.0f / ( matrix.m30 + matrix.m31 + matrix.m32 + matrix.m33 );

            result.x = ( (matrix.m00 * vector.x) + (matrix.m01 * vector.y) + (matrix.m02 * vector.z) + matrix.m03 ) * inverseW;
            result.y = ( (matrix.m10 * vector.x) + (matrix.m11 * vector.y) + (matrix.m12 * vector.z) + matrix.m13 ) * inverseW;
            result.z = ( (matrix.m20 * vector.x) + (matrix.m21 * vector.y) + (matrix.m22 * vector.z) + matrix.m23 ) * inverseW;

            return result;
        }

        /// <summary>
        ///		Used to multiply a Matrix4 object by a scalar value..
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 operator * ( Matrix4 left, float scalar) {
            Matrix4 result = new Matrix4();

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;
            result.m03 = left.m03 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;
            result.m13 = left.m13 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;
            result.m23 = left.m23 * scalar;

            result.m30 = left.m30 * scalar;
            result.m31 = left.m31 * scalar;
            result.m32 = left.m32 * scalar;
            result.m33 = left.m33 * scalar;

            return result;
        }

		/// <summary>
		///		Used to multiply a transformation to a Plane.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="plane"></param>
		/// <returns></returns>
		public static Plane operator * (Matrix4 left, Plane plane) {
			Plane result = new Plane();

			Vector3 planeNormal = plane.Normal;

			result.Normal = new Vector3(
				left.m00 * planeNormal.x + left.m01 * planeNormal.y + left.m02 * planeNormal.z,
				left.m10 * planeNormal.x + left.m11 * planeNormal.y + left.m12 * planeNormal.z,
				left.m20 * planeNormal.x + left.m21 * planeNormal.y + left.m22 * planeNormal.z);

			Vector3 pt = planeNormal * -plane.D;
			pt = left * pt;

			result.D = -pt.Dot(result.Normal);

			return result;
		}

        /// <summary>
        ///		Used to add two matrices together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 Add ( Matrix4 left, Matrix4 right ) {
        	return left + right;
        }
        
        /// <summary>
        ///		Used to add two matrices together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 operator + ( Matrix4 left, Matrix4 right ) {
            Matrix4 result = new Matrix4();

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03 + right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13 + right.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = left.m23 + right.m23;

            result.m30 = left.m30 + right.m30;
            result.m31 = left.m31 + right.m31;
            result.m32 = left.m32 + right.m32;
            result.m33 = left.m33 + right.m33;

            return result;
        }

        /// <summary>
        ///		Used to subtract two matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 Subtract ( Matrix4 left, Matrix4 right ) {
        	return left - right;
        }
        
        /// <summary>
        ///		Used to subtract two matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 operator - ( Matrix4 left, Matrix4 right ) {
            Matrix4 result = new Matrix4();

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03 - right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13 - right.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = left.m23 - right.m23;

            result.m30 = left.m30 - right.m30;
            result.m31 = left.m31 - right.m31;
            result.m32 = left.m32 - right.m32;
            result.m33 = left.m33 - right.m33;

            return result;
        }

        /// <summary>
        /// Compares two Matrix4 instances for equality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true if the Matrix 4 instances are equal, false otherwise.</returns>
        public static bool operator == (Matrix4 left, Matrix4 right) {
            if( 
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23 &&
                left.m30 == right.m30 && left.m31 == right.m31 && left.m32 == right.m32 && left.m33 == right.m33 )
                return true;

            return false;
        }

        /// <summary>
        /// Compares two Matrix4 instances for inequality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>true if the Matrix 4 instances are not equal, false otherwise.</returns>
        public static bool operator != (Matrix4 left, Matrix4 right) {
            return !(left == right);
        }

        /// <summary>
        ///		Used to allow assignment from a Matrix3 to a Matrix4 object.
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 FromMatrix3(Matrix3 right) {
        	return right;
        }
        
        /// <summary>
        ///		Used to allow assignment from a Matrix3 to a Matrix4 object.
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public static implicit operator Matrix4(Matrix3 right) {
            Matrix4 result = Matrix4.Identity;

            result.m00 = right.m00; result.m01 = right.m01; result.m02 = right.m02;
            result.m10 = right.m10; result.m11 = right.m11; result.m12 = right.m12;
            result.m20 = right.m20; result.m21 = right.m21; result.m22 = right.m22;	

            return result;
        }

        /// <summary>
        ///    Allows the Matrix to be accessed like a 2d array (i.e. matrix[2,3])
        /// </summary>
        /// <remarks>
        ///    This indexer is only provided as a convenience, and is <b>not</b> recommended for use in
        ///    intensive applications.  
        /// </remarks>
        public float this[int row, int col] {
            get {
                //Debug.Assert((row >= 0 && row < 4) && (col >= 0 && col < 4), "Attempt to access Matrix4 indexer out of bounds.");

                unsafe {
                    fixed(float* pM = &m00)
                        return *(pM + ((4*row) + col)); 
                }
            }
            set { 	
                //Debug.Assert((row >= 0 && row < 4) && (col >= 0 && col < 4), "Attempt to access Matrix4 indexer out of bounds.");

                unsafe {
                    fixed(float* pM = &m00)
                        *(pM + ((4*row) + col)) = value;
                }
            }
        }

        /// <summary>
        ///		Allows the Matrix to be accessed linearly (m[0] -> m[15]).  
        /// </summary>
        /// <remarks>
        ///    This indexer is only provided as a convenience, and is <b>not</b> recommended for use in
        ///    intensive applications.  
        /// </remarks>
        public float this[int index] {
            get {
                //Debug.Assert(index >= 0 && index < 16, "Attempt to access Matrix4 linear indexer out of bounds.");

                unsafe {
                    fixed(float* pMatrix = &this.m00) {			
                        return *(pMatrix + index);
                    }
                }
            }
            set {
                //Debug.Assert(index >= 0 && index < 16, "Attempt to access Matrix4 linear indexer out of bounds.");

                unsafe {
                    fixed(float* pMatrix = &this.m00) {			
                        *(pMatrix + index) = value;
                    }
                }
            }
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void MakeFloatArray(float[] floats) {
            unsafe {
                fixed(float* p = &m00) {
                    for(int i = 0; i < 16; i++)
                        floats[i] = *(p + i);
                }
            }
        }

        /// <summary>
        ///    Gets the determinant of this matrix.
        /// </summary>
        public float Determinant {
            get {
				// note: this is an expanded version of the Ogre determinant() method, to give better performance in C#. Generated using a script
                float result = m00 * (m11 * (m22 * m33 - m32 * m23) - m12 * (m21 * m33 - m31 * m23) + m13 * (m21 * m32 - m31 * m22)) - 
	                m01 * (m10 * (m22 * m33 - m32 * m23) - m12 * (m20 * m33 - m30 * m23) + m13 * (m20 * m32 - m30 * m22)) + 
	                m02 * (m10 * (m21 * m33 - m31 * m23) - m11 * (m20 * m33 - m30 * m23) + m13 * (m20 * m31 - m30 * m21)) - 
	                m03 * (m10 * (m21 * m32 - m31 * m22) - m11 * (m20 * m32 - m30 * m22) + m12 * (m20 * m31 - m30 * m21));

                return result;
            }
        }

        /// <summary>
        ///    Used to generate the adjoint of this matrix.  Used internally for <see cref="Inverse"/>.
        /// </summary>
        /// <returns>The adjoint matrix of the current instance.</returns>
        private Matrix4 Adjoint() {
            // note: this is an expanded version of the Ogre adjoint() method, to give better performance in C#. Generated using a script
            float val0 = m11 * (m22 * m33 - m32 * m23) - m12 * (m21 * m33 - m31 * m23) + m13 * (m21 * m32 - m31 * m22);
            float val1 = -(m01 * (m22 * m33 - m32 * m23) - m02 * (m21 * m33 - m31 * m23) + m03 * (m21 * m32 - m31 * m22));
            float val2 = m01 * (m12 * m33 - m32 * m13) - m02 * (m11 * m33 - m31 * m13) + m03 * (m11 * m32 - m31 * m12);
            float val3 = -(m01 * (m12 * m23 - m22 * m13) - m02 * (m11 * m23 - m21 * m13) + m03 * (m11 * m22 - m21 * m12));
            float val4 = -(m10 * (m22 * m33 - m32 * m23) - m12 * (m20 * m33 - m30 * m23) + m13 * (m20 * m32 - m30 * m22));
            float val5 = m00 * (m22 * m33 - m32 * m23) - m02 * (m20 * m33 - m30 * m23) + m03 * (m20 * m32 - m30 * m22);
            float val6 = -(m00 * (m12 * m33 - m32 * m13) - m02 * (m10 * m33 - m30 * m13) + m03 * (m10 * m32 - m30 * m12));
            float val7 = m00 * (m12 * m23 - m22 * m13) - m02 * (m10 * m23 - m20 * m13) + m03 * (m10 * m22 - m20 * m12);
            float val8 = m10 * (m21 * m33 - m31 * m23) - m11 * (m20 * m33 - m30 * m23) + m13 * (m20 * m31 - m30 * m21);
            float val9 = -(m00 * (m21 * m33 - m31 * m23) - m01 * (m20 * m33 - m30 * m23) + m03 * (m20 * m31 - m30 * m21));
            float val10 = m00 * (m11 * m33 - m31 * m13) - m01 * (m10 * m33 - m30 * m13) + m03 * (m10 * m31 - m30 * m11);
            float val11 = -(m00 * (m11 * m23 - m21 * m13) - m01 * (m10 * m23 - m20 * m13) + m03 * (m10 * m21 - m20 * m11));
            float val12 = -(m10 * (m21 * m32 - m31 * m22) - m11 * (m20 * m32 - m30 * m22) + m12 * (m20 * m31 - m30 * m21));
            float val13 = m00 * (m21 * m32 - m31 * m22) - m01 * (m20 * m32 - m30 * m22) + m02 * (m20 * m31 - m30 * m21);
            float val14 = -(m00 * (m11 * m32 - m31 * m12) - m01 * (m10 * m32 - m30 * m12) + m02 * (m10 * m31 - m30 * m11));
            float val15 = m00 * (m11 * m22 - m21 * m12) - m01 * (m10 * m22 - m20 * m12) + m02 * (m10 * m21 - m20 * m11);

            return new Matrix4(val0, val1, val2, val3, val4, val5, val6, val7, val8, val9, val10, val11, val12, val13, val14, val15);
        }

        #endregion

        #region Object overloads

        /// <summary>
        ///		Overrides the Object.ToString() method to provide a text representation of 
        ///		a Matrix4.
        /// </summary>
        /// <returns>A string representation of a vector3.</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
			
            sb.AppendFormat(" | {0} {1} {2} {3} |\n", this.m00, this.m01, this.m02, this.m03);
            sb.AppendFormat(" | {0} {1} {2} {3} |\n", this.m10, this.m11, this.m12, this.m13);
            sb.AppendFormat(" | {0} {1} {2} {3} |\n", this.m20, this.m21, this.m22, this.m23);
            sb.AppendFormat(" | {0} {1} {2} {3} |\n", this.m30, this.m31, this.m32, this.m33);

            return sb.ToString();
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
			return m00.GetHashCode() ^ m01.GetHashCode() ^ m02.GetHashCode() ^ m03.GetHashCode()
				^ m10.GetHashCode() ^ m11.GetHashCode() ^ m12.GetHashCode() ^ m13.GetHashCode()
				^ m20.GetHashCode() ^ m21.GetHashCode() ^ m22.GetHashCode() ^ m23.GetHashCode()
				^ m30.GetHashCode() ^ m31.GetHashCode() ^ m32.GetHashCode() ^ m33.GetHashCode();
        }

        /// <summary>
        ///		Compares this Matrix to another object.  This should be done because the 
        ///		equality operators (==, !=) have been overriden by this class.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
			return obj is Matrix4 && this == (Matrix4)obj;
        }

        /// <summary>
        ///		Concatenate two affine matrix.
        ///		The matrices must be affine matrix. @see Matrix4.IsAffine.
        /// </summary>
        public Matrix4 ConcatenateAffine(Matrix4 m2) {
            Debug.Assert(IsAffine && m2.IsAffine);

            return new Matrix4(
                m00 * m2.m00 + m01 * m2.m10 + m02 * m2.m20,
                m00 * m2.m01 + m01 * m2.m11 + m02 * m2.m21,
                m00 * m2.m02 + m01 * m2.m12 + m02 * m2.m22,
                m00 * m2.m03 + m01 * m2.m13 + m02 * m2.m23 + m03,

                m10 * m2.m00 + m11 * m2.m10 + m12 * m2.m20,
                m10 * m2.m01 + m11 * m2.m11 + m12 * m2.m21,
                m10 * m2.m02 + m11 * m2.m12 + m12 * m2.m22,
                m10 * m2.m03 + m11 * m2.m13 + m12 * m2.m23 + m13,

                m20 * m2.m00 + m21 * m2.m10 + m22 * m2.m20,
                m20 * m2.m01 + m21 * m2.m11 + m22 * m2.m21,
                m20 * m2.m02 + m21 * m2.m12 + m22 * m2.m22,
                m20 * m2.m03 + m21 * m2.m13 + m22 * m2.m23 + m23,

                0, 0, 0, 1);
        }

        /// <summary>
        ///		3-D Vector transformation specially for affine matrix.
        ///		The matrices must be affine matrix. @see Matrix4.IsAffine.
        /// </summary>
        /// <remarks
        ///     Transforms the given 3-D vector by the matrix, projecting the 
        ///     result back into <i>w</i> = 1.
        public Vector3 TransformAffine(Vector3 v) {
            Debug.Assert(IsAffine);

            return new Vector3(
                    m00 * v.x + m01 * v.y + m02 * v.z + m03, 
                    m10 * v.x + m11 * v.y + m12 * v.z + m13,
                    m20 * v.x + m21 * v.y + m22 * v.z + m23);
        }

        /** 4-D Vector transformation specially for affine matrix.
            @note
                The matrix must be an affine matrix. @see Matrix4::isAffine.
        */
        public Vector4 TransformAffine(Vector4 v)
        {
            Debug.Assert(IsAffine);

            return new Vector4(
                m00 * v.x + m01 * v.y + m02 * v.z + m03 * v.w, 
                m10 * v.x + m11 * v.y + m12 * v.z + m13 * v.w,
                m20 * v.x + m21 * v.y + m22 * v.z + m23 * v.w,
                v.w);
        }

        #endregion
    }
}
