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

namespace Multiverse.Serialization
{
    /// <summary>
    /// These are a set of methods that extend operations on certain Axiom
    /// math objects.  
    /// TODO: It would probably be better to build these directly into the
    /// Axiom math objects; however, for now my objective is just to get 
    /// them out of Collada-related classes.
    /// </summary>
    class MathHelpers
    {
        private static readonly log4net.ILog m_Log = log4net.LogManager.GetLogger( typeof( ColladaMeshInfo ) );

        public static string GetEulerString( Quaternion rot )
        {
            return rot.EulerString;
        }

        public static Quaternion GetRotation( Matrix4 transform )
        {
            Matrix3 tmp =
                new Matrix3( transform.m00, transform.m01, transform.m02,
                            transform.m10, transform.m11, transform.m12,
                            transform.m20, transform.m21, transform.m22 );
            float scale = GetScaleFactor( tmp );
#if DEBUG_NORMALIZE
            Matrix3 tmp2 = tmp * scale;
            Quaternion rv2 = Quaternion.Identity;
            rv2.FromRotationMatrix(tmp2);
            if (Math.Abs(1.0 - rv.Norm) > .01f) {
                string msg = string.Format("Probable non-uniform scale factor on rotation matrix; rv.Norm = {0}", rv.Norm);
                throw new Exception(msg);
            }
#endif
            tmp = tmp * scale;
            Quaternion rv = Quaternion.Identity;
            rv.FromRotationMatrix( tmp );
            if( Math.Abs( 1.0 - rv.Norm ) > .01f )
            {
                string msg = string.Format( "Probable non-uniform scale factor on rotation matrix; rv.Norm = {0}", rv.Norm );
                throw new Exception( msg );
            }
            if( Math.Abs( 1.0 - rv.Norm ) > .001f )
                m_Log.Warn( "Possible non-uniform scale factor on rotation matrix" );
            // rv.Normalize();
            return rv;
        }

        public static float GetScaleFactor( Matrix4 transform )
        {
            Matrix3 tmp = transform.GetMatrix3();
            return GetScaleFactor( tmp );
        }

        public static float GetScaleFactor( Matrix3 transform )
        {
            float det = transform.Determinant;
            float scale = (float) (1 / Math.Pow( det, 1.0 / 3.0 ));
            return scale;
        }

        // Try to convert a matrix that is a combination of rotate translate 
        // and scale to a similar matrix with just the rotation and translation
        public static Matrix4 Normalize( Matrix4 transform )
        {
            Quaternion rotate;
            Vector3 scale, translate;
            Matrix4.DecomposeMatrix( ref transform, out translate, out rotate, out scale );
            return Multiverse.MathLib.MathUtil.GetTransform( ref rotate, ref translate );
        }

        public static Matrix4 GetUnscaledTransformProduct( ref Matrix4 worldTransform, ref Matrix4 transform )
        {
            Quaternion worldRotate;
            Vector3 worldTranslate, worldScale;
            Matrix4.DecomposeMatrix( ref worldTransform, out worldTranslate, out worldRotate, out worldScale );
            Quaternion rotate;
            Vector3 translate, scale;
            Matrix4.DecomposeMatrix( ref transform, out translate, out rotate, out scale );
            // Completely ignore scale, since it should not be being used.
            translate = ScaleVector( worldScale, worldRotate * translate );
            rotate = worldRotate * rotate;
            Matrix4 rv = rotate.ToRotationMatrix();
            rv.Translation = translate;
            return rv;
        }

        public static Vector3 ScaleVector( Vector3 scale, Vector3 vector )
        {
            return new Vector3( scale.x * vector.x, scale.y * vector.y, scale.z * vector.z );
        }

    }
}
