using System;
using System.Diagnostics;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Bsp
{
	/// <summary>
	///		Frustum used for spotlights. It is not supposed to be part of the scene, and
	///		will not be rendered. The ViewMatrix and ProjectionMatrix properties, used
	///		for projecting texturing, are updated only when a spotlight is assigned 
	///		at the Light property.
	/// </summary>
	public class SpotlightFrustum : Frustum
	{
		protected Light light;
		protected Node lightNode;
		protected Quaternion lightOrientation;
		protected Vector3 lightPosition;

		public Light Spotlight
		{
			get
			{
				return light;
			}
			set
			{
				this.light = value;
				this.lightNode = light.ParentNode;
				this.lightPosition = light.DerivedPosition;
				this.lightOrientation = GetLightOrientation();

				base.FOV = light.SpotlightOuterAngle;
				base.Near = 1;
				base.Far = light.AttenuationRange;
				base.AspectRatio = 1;
				base.ProjectionType = Projection.Perspective;

				UpdateMatrices();
			}
		}

		#region Frustum Members

		/// <summary>
		/// Gets the projection matrix for this frustum.
		/// </summary>
		public override Matrix4 ProjectionMatrix 
		{
			get { return projectionMatrix; }
		}

		/// <summary>
		///     Gets the view matrix for this frustum.
		/// </summary>
		public override Matrix4 ViewMatrix 
		{
			get { return viewMatrix; }
		}

		#endregion Frustum Members

		protected void UpdateMatrices() 
		{
			// grab a reference to the current render system
			RenderSystem renderSystem = Root.Instance.RenderSystem;

			if(projectionType == Projection.Perspective) 
			{
				// perspective transform, API specific
				projectionMatrix = renderSystem.MakeProjectionMatrix(fieldOfView, aspectRatio, nearDistance, farDistance);
			}
			else if(projectionType == Projection.Orthographic) 
			{
				// orthographic projection, API specific
				projectionMatrix = renderSystem.MakeOrthoMatrix(fieldOfView, aspectRatio, nearDistance, farDistance);
			}

			// View matrix is:
			//
			//  [ Lx  Uy  Dz  Tx  ]
			//  [ Lx  Uy  Dz  Ty  ]
			//  [ Lx  Uy  Dz  Tz  ]
			//  [ 0   0   0   1   ]
			//
			// Where T = -(Transposed(Rot) * Pos)

			// This is most efficiently done using 3x3 Matrices

			// Get orientation from quaternion
			Quaternion orientation = lightOrientation; 
			Vector3 position = lightPosition;
			Matrix3 rotation = orientation.ToRotationMatrix();

			Vector3 left = rotation.GetColumn(0);
			Vector3 up = rotation.GetColumn(1);
			Vector3 direction = rotation.GetColumn(2);

			// make the translation relative to the new axis
			Matrix3 rotationT = rotation.Transpose();
			Vector3 translation = -rotationT * position;

			// initialize the upper 3x3 portion with the rotation
			viewMatrix = rotationT;

			// add the translation portion, add set 1 for the bottom right portion
			viewMatrix.m03 = translation.x;
			viewMatrix.m13 = translation.y;
			viewMatrix.m23 = translation.z;
			viewMatrix.m33 = 1.0f;
		}

		protected Quaternion GetLightOrientation()
		{
			Vector3 zAdjustVec = -light.DerivedDirection;
			Vector3 xAxis, yAxis, zAxis;

			Quaternion orientation = (lightNode == null) ? Quaternion.Identity : lightNode.DerivedOrientation;

			// Get axes from current quaternion
			// get the vector components of the derived orientation vector
			orientation.ToAxes(out xAxis, out yAxis, out zAxis);

			Quaternion rotationQuat;

			if ((zAxis + zAdjustVec).LengthSquared < 0.00001f) 
			{
				// Oops, a 180 degree turn (infinite possible rotation axes)
				// Default to yaw i.e. use current UP
				rotationQuat = Quaternion.FromAngleAxis(MathUtil.PI, yAxis);
			}
			else 
			{
				// Derive shortest arc to new direction
				rotationQuat = zAxis.GetRotationTo(zAdjustVec);
			}

			return rotationQuat * orientation;
		}
	}
}
