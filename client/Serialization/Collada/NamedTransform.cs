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
using System.Text.RegularExpressions;

using Axiom.MathLib;
using Axiom.Animating;

namespace Multiverse.Serialization.Collada
{
	//-----------------------------------------------------------------
    public abstract class NamedTransform
    {
		protected static readonly log4net.ILog m_Log = log4net.LogManager.GetLogger( typeof( NamedTransform ) );

		protected string name;
        protected Matrix4 transform = Matrix4.Identity;

        public NamedTransform( string name )
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public virtual Matrix4 Transform
        {
            get { return transform; }
            set { Debug.Assert( false, "Called transform on non-matrix transform" ); }
        }

        public abstract void SetFromChannel( Channel channel, int preIndex, int postIndex, float interpFactor, Accessor transformAccessor );
    }

	//-----------------------------------------------------------------
    public class NamedMatrixTransform : NamedTransform
    {
        public NamedMatrixTransform( string name, Matrix4 t )
            : base( name )
        {
            this.Transform = t;
        }

        public override Matrix4 Transform
        {
            set { this.transform = value; }
        }

		// We don't know how to interpolate matrices, so we will always set from
		// the channel at the preIndex. We ignore the interpolation term, which
		// appears here just to match the base class signature.
		public override void SetFromChannel( Channel channel, int preIndex, int postIndex, float interpFactor, Accessor transformAccessor )
		{
			if( preIndex != postIndex )
			{
				m_Log.DebugFormat(
					"Attempting to set a NamedMatrixTransform with" +
					" mismatched pre/post index from channel '{0}'",
					channel.TargetComponent );
			}
            
			// We don't know how to interpolate matrices, so we just use the preIndex
			Transform = BuildTransformFromAccessor( 
				transformAccessor, channel.TargetParam, channel.TargetMember, preIndex );
		}


        // <summary>
        //   Set the transform in trans based on the data in the 
        //   transformAccessor
        // </summary>
        // <param name="transformAccessor">the accessor for getting at data</param>
        // <param name="targetParam">the name of the param from the accessor</param>
        // <param name="targetAttribute">may be used to specify that we are only modifying a portion of the transform</param>
        // <param name="j">the index of the entry in the accessor</param>
        Matrix4 BuildTransformFromAccessor( Accessor transformAccessor, string targetParam, string targetAttribute, int j )
        {
            AccessorParam accessorParam = transformAccessor.GetAccessorParam( targetParam );
            
			Debug.Assert( accessorParam is ParamFloat || accessorParam is ParamMatrix4 );

			Matrix4 updatedTransform = new Matrix4();

			if( accessorParam is ParamMatrix4 )
            {
				// Update with an entire matrix from the source
				updatedTransform = (Matrix4) transformAccessor.GetParam( targetParam, j );
            }
            else
            {
				updatedTransform = Transform;
                
				if( targetAttribute != null )
                {
					// Update a single element of the transform
                    Regex rx = new Regex( "\\((\\d)\\)\\((\\d)\\)" );
                    
					Match m = rx.Match( targetAttribute );

                    Debug.Assert( m.Groups.Count == 3 );
                    
					int column = int.Parse( m.Groups[ 1 ].Value );
                    int row    = int.Parse( m.Groups[ 2 ].Value );

					updatedTransform[ 4 * row + column ] = (float) transformAccessor.GetParam( targetParam, j );
                }
                else
                {
					// New matrix from a matrix array in the source
					for( int k = 0; k < 16; ++k )
					{
						updatedTransform[ k ] = (float) transformAccessor.GetParam( targetParam, 16 * j + k );
					}
                }
            }

			return updatedTransform;
        }
	}

	//-----------------------------------------------------------------
    public class NamedRotateTransform : NamedTransform
    {
        float   angle; // angle in radians
        Vector3 axis = Vector3.UnitY;

        public NamedRotateTransform( string name, float angle, Vector3 axis )
            : base( name )
        {
            this.Angle = angle;
            this.Axis = axis;
        }

        public Quaternion Rotate
        {
            get
            {
                return Quaternion.FromAngleAxis( angle, axis );
            }
            set
            {
                float tmpAngle = 0;
                Vector3 tmpAxis = Vector3.UnitY;
                value.ToAngleAxis( ref tmpAngle, ref tmpAxis );
                if( tmpAngle == 0 )
                {
                    // the quaternion was too close to zero
                    // leave axis alone, and change angle
                    this.Angle = 0;
                }
                else if( tmpAxis.Dot( axis ) >= 0 )
                {
                    // we can use this new angle and axis, because it
                    // has not reversed the meaning of angle
                    this.Axis = tmpAxis;
                    this.Angle = tmpAngle;
                }
                else
                {
                    // we must reverse the angle and axis
                    this.Axis  = -1 * tmpAxis;
                    this.Angle = -1 * tmpAngle;
                }
            }
        }

        /// <summary>
        ///  Get or set the axis of rotation
        /// </summary>
        public Vector3 Axis
        {
            set
            {
                axis = value;
                Quaternion rot = Quaternion.FromAngleAxis( angle, axis );
                transform = Matrix4.FromMatrix3( rot.ToRotationMatrix() );
            }
            get { return axis; }
        }

        /// <summary>
        ///  Get or set the angle in radians
        /// </summary>
        public float Angle
        {
            set
            {
                angle = value;
                Quaternion rot = Quaternion.FromAngleAxis( angle, axis );
                transform = Matrix4.FromMatrix3( rot.ToRotationMatrix() );
            }
            get { return angle; }
        }

		public override void SetFromChannel( Channel channel, int preIndex, int postIndex, float interpFactor, Accessor transformAccessor )
		{
			// TODO: This can be made to resemble more closely the implementations in
			// the other NamedTransform subclasses, but it involves separating terms
			// of the quaternion. I don't want to deal with this yet, but it should be done.
			//-------------------------------------------------------------------------------
			
			// Set orientation
            ApplyChannelToQuaternion( transformAccessor, channel.TargetParam, channel.TargetMember, preIndex );
            Quaternion preOrient = Rotate;
            
			ApplyChannelToQuaternion( transformAccessor, channel.TargetParam, channel.TargetMember, postIndex );
			Quaternion postOrient = Rotate;

			Quaternion interpOrient =
                (Quaternion) AnimableValue.InterpolateValues( interpFactor, AnimableType.Quaternion, preOrient, postOrient );
            
			Debug.Assert( Math.Abs( 1.0 - interpOrient.Norm ) < .001 );
            
			// Make sure the axis is not perturbed when we set the rotation
			Vector3 preAxis = Axis;
            
			// NOTE: setting Rotate has a side-effect of altering the Axis
			Rotate = interpOrient;
            
			Vector3 interpAxis = Axis;
            
			if( (interpAxis - preAxis).Length > .001 )
            {
                Debug.Assert( false, "pre axis and interp axis mismatch" );

                Rotate = interpOrient;
            }
		}



        // <summary>
        //   Set the transform in trans based on the data in the 
        //   transformAccessor
        // </summary>
        // <param name="transformAccessor">the accessor for getting at data</param>
        // <param name="targetParam">the name of the param from the accessor</param>
        // <param name="targetAttribute">may be used to specify that we are only modifying a portion of the transform</param>
        // <param name="j">the index of the entry in the accessor</param>
        void ApplyChannelToQuaternion( Accessor transformAccessor, string targetParam, string targetAttribute, int j )
        {
            Vector3 vec = Axis;
            
			float angle = MathUtil.RadiansToDegrees( Angle );

            switch( targetParam )
            {
            case "ANGLE":
                angle = (float) transformAccessor.GetParam( targetParam, j );
                Angle = MathUtil.DegreesToRadians( angle );
                break;

            case "X":
                vec.x = (float) transformAccessor.GetParam( targetParam, j );
                Axis = vec;
                break;

            case "Y":
                vec.y = (float) transformAccessor.GetParam( targetParam, j );
                Axis = vec;
                break;

            case "Z":
                vec.z = (float) transformAccessor.GetParam( targetParam, j );
                Axis = vec;
                break;

            default:
                Debug.Assert( false, "Unknown target param for rotation" );
                break;
            }
        }
	}

	//-----------------------------------------------------------------
	public class NamedScaleTransform : NamedTransform
    {
        Vector3 scale;

        public NamedScaleTransform( string name, Vector3 vec )
            : base( name )
        {
            this.Scale = vec;
        }

        public Vector3 Scale
        {
            set
            {
                scale = value;
                transform = Matrix4.Identity;
                for( int i = 0; i < 3; ++i )
                    transform[ i, i ] = scale[ i ];
            }
            get { return scale; }
        }

        public override void SetFromChannel( Channel channel, int preIndex, int postIndex, float interpFactor, Accessor transformAccessor )
        {
			Vector3 preScale  = BuildScaleFromAccessor( transformAccessor, channel.TargetParam, preIndex ); ;

			Vector3 postScale = BuildScaleFromAccessor( transformAccessor, channel.TargetParam, postIndex ); ;

			Scale = (Vector3) AnimableValue.InterpolateValues( interpFactor, AnimableType.Vector3, preScale, postScale );
        }

        // <summary>
        //   Set the transform in trans based on the data in the 
        //   transformAccessore
        // </summary>
        // <param name="transformAccessor">the accessor for getting at data</param>
        // <param name="targetParam">the name of the param from the accessor</param>
        // <param name="j">the index of the entry in the accessor</param>
        private Vector3 BuildScaleFromAccessor( Accessor transformAccessor, string targetParam, int j )
        {
            Vector3 vec = Scale;
            switch( targetParam )
            {
                case "":
                case "TRANSFORM":
                    vec.x = (float) transformAccessor.GetParam( "X", j );
                    vec.y = (float) transformAccessor.GetParam( "Y", j );
                    vec.z = (float) transformAccessor.GetParam( "Z", j );
                    break;

                case "X":
                    vec.x = (float) transformAccessor.GetParam( targetParam, j );
                    break;

                case "Y":
                    vec.y = (float) transformAccessor.GetParam( targetParam, j );
                    break;

                case "Z":
                    vec.z = (float) transformAccessor.GetParam( targetParam, j );
                    break;

                default:
                    Debug.Assert( false, String.Format(
						"Unknown target param '{0}' for scale", targetParam ) );
                    break;
            }

			return vec;
        }

    }

	//-----------------------------------------------------------------
	public class NamedTranslateTransform : NamedTransform
    {
        Vector3 translate;

        public NamedTranslateTransform( string name, Vector3 vec )
            : base( name )
        {
            this.Translate = vec;
        }

        public Vector3 Translate
        {
            set
            {
                translate = value;
                transform = Matrix4.Identity;
                for( int i = 0; i < 3; ++i )
                    transform[ i, 3 ] = translate[ i ];
            }
            get { return translate; }
        }

		public override void SetFromChannel( Channel channel, int preIndex, int postIndex, float interpFactor, Accessor transformAccessor )
		{
			Vector3 preTrans = BuildTranslationFromAccessor( transformAccessor, channel.TargetParam, preIndex );
			
			Vector3 postTrans = BuildTranslationFromAccessor( transformAccessor, channel.TargetParam, postIndex );

			Vector3 interpTrans =
				(Vector3) AnimableValue.InterpolateValues( interpFactor, AnimableType.Vector3, preTrans, postTrans );
			
			Translate = interpTrans;
		}

        // <summary>
        //   Set the transform in trans based on the data in the 
        //   transformAccessor
        // </summary>
        // <param name="transformAccessor">the accessor for getting at data</param>
        // <param name="targetParam">the name of the param from the accessor</param>
        // <param name="targetAttribute">may be used to specify that we are only modifying a portion of the transform</param>
        // <param name="j">the index of the entry in the accessor</param>
        Vector3 BuildTranslationFromAccessor( Accessor transformAccessor, string targetParam, int j )
        {
            Vector3 updatedTranslation = Translate;

            switch( targetParam )
            {
            case "":
            case "TRANSFORM":
                updatedTranslation.x = (float) transformAccessor.GetParam( "X", j );
                updatedTranslation.y = (float) transformAccessor.GetParam( "Y", j );
                updatedTranslation.z = (float) transformAccessor.GetParam( "Z", j );
                break;

            case "X":
                updatedTranslation.x = (float) transformAccessor.GetParam( targetParam, j );
                break;

            case "Y":
                updatedTranslation.y = (float) transformAccessor.GetParam( targetParam, j );
                break;

            case "Z":
                updatedTranslation.z = (float) transformAccessor.GetParam( targetParam, j );
                break;

            default:
                Debug.Assert( false, "Unknown target param for translate" );
                break;
            }

			return updatedTranslation;
        }
	}
}
