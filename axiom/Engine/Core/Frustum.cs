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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Collections;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.Core {
	/// <summary>
	///     A frustum represents a pyramid, capped at the near and far end which is
	///     used to represent either a visible area or a projection area. Can be used
	///     for a number of applications.
	/// </summary>
	// TODO: Review attaching object in the scene and making them no longer require a name.
	public class Frustum : MovableObject, IRenderable {
		#region Constants

		/// <summary>
		///		Small constant used to reduce far plane projection to avoid inaccuracies.
		/// </summary>
		public const float InfiniteFarPlaneAdjust = 0.00001f;

		/// <summary>
		///		Arbitrary large distance to use for the far plane when set to 0 (infinite far plane).
		/// </summary>
		public const float InfiniteFarPlaneDistance = 100000.0f;

		#endregion Constants

		#region Fields
		/// <summary>
		///		Perspective or Orthographic?
		/// </summary>
		protected Projection projectionType;
		/// <summary>
		///     y-direction field-of-view (default 45).
		/// </summary>
		protected float fieldOfView;
		/// <summary>
		///     Far clip distance - default 10000.
		/// </summary>
		protected float farDistance;
		/// <summary>
		///     Near clip distance - default 100.
		/// </summary>
		protected float nearDistance;
		/// <summary>
		///     x/y viewport ratio - default 1.3333
		/// </summary>
		protected float aspectRatio;
		/// <summary>
		///     Off-axis frustum center offset - default (0.0, 0.0)
		/// </summary>
        protected Vector2 frustumOffset;
		/// <summary>
		///     Focal length of frustum (for stereo rendering, defaults to 1.0)
		/// </summary>
        protected float focalLength;
		/// <summary>
		///     The 6 main clipping planes.
		/// </summary>
		protected Plane[] planes = new Plane[6];
		/// <summary>
		///     Stored versions of parent orientation.
		/// </summary>
		protected Quaternion lastParentOrientation;
		/// <summary>
		///     Stored versions of parent position.
		/// </summary>
		protected Vector3 lastParentPosition;
		/// <summary>
		///     Pre-calced projection matrix for the specific render system
		/// </summary>
        protected Matrix4 projectionMatrixRS;
		/// <summary>
		///     Pre-calced standard projection matrix but with render system depth range
		/// </summary>
        protected Matrix4 projectionMatrixWithRSDepth;
		/// <summary>
		///     Pre-calced standard projection matrix.
		/// </summary>
		protected Matrix4 projectionMatrix;
		/// <summary>
		///     Pre-calced view matrix.
		/// </summary>
		protected Matrix4 viewMatrix;
		/// <summary>
		///     Something's changed in the frustum shape?
		/// </summary>
		protected bool recalculateFrustum;
		/// <summary>
		///     Something in the view pos has changed?
		/// </summary>
		protected bool recalculateView;
		/// <summary>
		///     Bounding box of this frustum.
		/// </summary>
		protected AxisAlignedBox boundingBox = AxisAlignedBox.Null;
		/// <summary>
		///     Vertex info for rendering this frustum.
		/// </summary>
		protected VertexData vertexData = new VertexData();
		/// <summary>
		///     Material to use when rendering this frustum.
		/// </summary>
		protected Material material;
		/// <summary>
		///		Something re the frustum planes has changed
		/// </summary>
        protected bool recalculateFrustumPlanes;
		/// <summary>
		///		Something re the world space corners has changed
		/// </summary>
        protected bool recalculateWorldSpaceCorners;
		/// <summary>
		///		Something re the vertex data has changed
		/// </summary>
        protected bool recalculateVertexData;
		/// <summary>
		///		Are we using a custom view matrix?
		/// </summary>
        protected bool customViewMatrix;
		/// <summary>
		///		Are we using a custom projection matrix?
		/// </summary>
        protected bool customProjectionMatrix;
		/// <summary>
		///		Frustum corners in world space.
		/// </summary>
		protected Vector3[] worldSpaceCorners = new Vector3[8];

		/** Temp coefficient values calculated from a frustum change,
			used when establishing the frustum planes when the view changes. */
		protected float[] coeffL = new float[2];
		protected float[] coeffR = new float[2];
		protected float[] coeffB = new float[2];
		protected float[] coeffT = new float[2];

		/// <summary>
		///		Is this frustum to act as a reflection of itself?
		/// </summary>
		protected bool isReflected;
		/// <summary>
		///		Derive reflection matrix.
		/// </summary>
		protected Matrix4 reflectionMatrix;
		/// <summary>
		///		Fixed reflection.
		/// </summary>
		protected Plane reflectionPlane;
		/// <summary>
		///		Reference of a reflection plane (automatically updated).
		/// </summary>
		protected IDerivedPlaneProvider linkedReflectionPlane;
		/// <summary>
		///		Record of the last world-space reflection plane info used.
		/// </summary>
		protected Plane lastLinkedReflectionPlane;
		/// <summary>
		///		Is this frustum using an oblique depth projection?
		/// </summary>
		protected bool useObliqueDepthProjection;
		/// <summary>
		///		Fixed oblique projection plane.
		/// </summary>
		protected Plane obliqueProjPlane;
		/// <summary>
		///		Reference to oblique projection plane (automatically updated).
		/// </summary>
		protected IDerivedPlaneProvider linkedObliqueProjPlane;
		/// <summary>
		///		Record of the last world-space oblique depth projection plane info used.
		/// </summary>
		protected Plane lastLinkedObliqueProjPlane;

		/// <summary>
		///     Dummy list for IRenderable.Lights since we wont be lit.
		/// </summary>
        protected List<Light> dummyLightList = new List<Light>();

		protected Hashtable customParams = new Hashtable();

		#endregion Fields

		#region Constructors

		/// <summary>
		///     Default constructor.
		/// </summary>
		public Frustum() {
			for(int i = 0; i < 6; i++) {
				planes[i] = new Plane();
			}

			fieldOfView = MathUtil.RadiansToDegrees(MathUtil.PI / 4.0f);
			nearDistance = 100.0f;
			farDistance = 100000.0f;
			aspectRatio = 1.33333333333333f;
            frustumOffset = Vector2.Zero;
            focalLength = 1.0f;

			recalculateFrustum = true;
			recalculateView = true;
            recalculateWorldSpaceCorners = true;
			recalculateFrustumPlanes = true;

            reflectionMatrix = Matrix4.Zero;
            projectionMatrixRS = Matrix4.Zero;
            projectionMatrixWithRSDepth = Matrix4.Zero;
            projectionMatrix = Matrix4.Zero;
            viewMatrix = Matrix4.Zero;
            
            // Init matrices
			viewMatrix = Matrix4.Zero;
			projectionMatrix = Matrix4.Zero;

            customViewMatrix = false;
            customProjectionMatrix = false;
            
			projectionType = Projection.Perspective;

			lastParentPosition = Vector3.Zero;
			lastParentOrientation = Quaternion.Identity;

			material = MaterialManager.Instance.GetByName("BaseWhite");

			isVisible = false;
            parentNode = null;
            
            UpdateView();
            UpdateFrustum();
        }

		#endregion 

		#region Properties

		/// <summary>
		///		Sets the Y-dimension Field Of View (FOV) of the camera.
		/// </summary>
		/// <remarks>
		///		Field Of View (FOV) is the angle made between the camera's position, and the top & bottom edges
		///		of the 'screen' onto which the scene is projected. High values (90+) result in a wide-angle,
		///		fish-eye kind of view, low values (30-) in a stretched, telescopic kind of view. Typical values
		///		are between 45 and 60.
		///		<p/>
		///		This value represents the VERTICAL field-of-view. The horizontal field of view is calculated from
		///		this depending on the dimensions of the viewport (they will only be the same if the viewport is square).
		/// </remarks>
		public virtual float FOVy {
			get { 
				return fieldOfView; 
			} 
			set {
				fieldOfView = value;
				InvalidateFrustum();
				InvalidateView();	//XEONX FIX: Now the IsObjectVisible() will work properly
			}
		}

		/// <summary>
		///		Gets/Sets the position of the near clipping plane.
		///	</summary>
		///	<remarks>
		///		The position of the near clipping plane is the distance from the cameras position to the screen
		///		on which the world is projected. The near plane distance, combined with the field-of-view and the
		///		aspect ratio, determines the size of the viewport through which the world is viewed (in world
		///		co-ordinates). Note that this world viewport is different to a screen viewport, which has it's
		///		dimensions expressed in pixels. The cameras viewport should have the same aspect ratio as the
		///		screen viewport it renders into to avoid distortion.
		/// </remarks>
		public virtual float Near {
			get { 
				return nearDistance; 
			}
			set {
				Debug.Assert(value > 0, "Near clip distance must be greater than zero.");

				nearDistance = value;
				InvalidateFrustum();
				InvalidateView();	//XEONX FIX: Now the IsObjectVisible() will work properly
			}
		}

		/// <summary>
		///		Gets/Sets the distance to the far clipping plane.
		///	 </summary>
		///	 <remarks>
		///		The view frustrum is a pyramid created from the camera position and the edges of the viewport.
		///		This frustrum does not extend to infinity - it is cropped near to the camera and there is a far
		///		plane beyond which nothing is displayed. This method sets the distance for the far plane. Different
		///		applications need different values: e.g. a flight sim needs a much further far clipping plane than
		///		a first-person shooter. An important point here is that the larger the gap between near and far
		///		clipping planes, the lower the accuracy of the Z-buffer used to depth-cue pixels. This is because the
		///		Z-range is limited to the size of the Z buffer (16 or 32-bit) and the max values must be spread over
		///		the gap between near and far clip planes. The bigger the range, the more the Z values will
		///		be approximated which can cause artifacts when lots of objects are close together in the Z-plane. So
		///		make sure you clip as close to the camera as you can - don't set a huge value for the sake of
		///		it.
		/// </remarks>
		/// <value>
		///		The distance to the far clipping plane from the frustum in 
		///		world coordinates.  If you specify 0, this means an infinite view
		///		distance which is useful especially when projecting shadows; but
		///		be careful not to use a near distance too close.
		/// </value>
		public virtual float Far {
			get { 
				return farDistance; 
			}
			set {
				farDistance = value;
				InvalidateFrustum();
				InvalidateView();	//XEONX FIX: Now the IsObjectVisible() will work properly
			}
		}

		/// <summary>
		///		Gets/Sets the aspect ratio to use for the camera viewport.
		/// </summary>
		/// <remarks>
		///		The ratio between the x and y dimensions of the rectangular area visible through the camera
		///		is known as aspect ratio: aspect = width / height .
		///		<p/>
		///		The default for most fullscreen windows is 1.3333f - this is also assumed unless you
		///		use this property to state otherwise.
		/// </remarks>
		public virtual float AspectRatio {
			get { 
				return aspectRatio; 
			}
			set {
				aspectRatio = value;
				InvalidateFrustum();
			}
		}

		/// <summary>
		///		Gets/Sets frustum offsets, used in stereo rendering.
		/// </summary>
		/// <remarks>
		///		You can set both horizontal and vertical plane offsets of "eye"; in
		///		stereo rendering frustum is moved in horizontal plane. To be able to
		///		render from two "eyes" you'll need two cameras rendering on two
		///		RenderTargets.
        ///     The frustum offsets is in world coordinates, and default to (0, 0) - no offsets.
		/// </remarks>
        public Vector2 FrustumOffset {
            get {
                return frustumOffset;
            }
            set {
                frustumOffset = value;
                InvalidateFrustum();
            }
        }

		/// <summary>
		///		Sets frustum horizontal and offsets
		/// </summary>
        /// <param name="horizontal">The horizontal plane offset.</param>
        /// <param name="vertical">The horizontal plane offset.</param>
        public void SetFrustumOffset(float horizontal, float vertical) {
            frustumOffset = new Vector2(horizontal, vertical);
            InvalidateFrustum();
        }

        /// <summary>
        ///     Gets/Sets frustum focal length (used in stereo rendering).
        /// </summary>
        public float FocalLength {
            get {
                return focalLength;
            }
            set {
                if (value <= 0)
                    throw new AxiomException("Focal length must be greater than zero.  In Frustum.FocalLength");
                focalLength = value;
                InvalidateFrustum();
            }
        }

		/// <summary>
		/// Gets the projection matrix for this frustum.
		/// </summary>
		public virtual Matrix4 ProjectionMatrix {
			get {
				UpdateFrustum();

				return projectionMatrix;
			}
		}

		/// <summary>
		///     Gets the view matrix for this frustum.
		/// </summary>
		public virtual Matrix4 ViewMatrix {
			get {
				UpdateView();

				return viewMatrix;
			}
		}

		/// <summary>
		///    Gets/Sets the type of projection to use (orthographic or perspective). Default is perspective.
		/// </summary>
		public Projection ProjectionType {
			get { 
				return projectionType; 
			}
			set { 
				projectionType = value;	
				InvalidateFrustum();
			}
		}

		/// <summary>
		///     Gets a flag that specifies whether this camera is being reflected or not.
		/// </summary>
		public virtual bool IsReflected {
			get {
				return isReflected;
			}
		}

		/// <summary>
		///		Gets whether or not the view matrix is out of date.
		/// </summary>
		protected virtual bool IsViewOutOfDate {
			get {
				// are we attached to another node?
				if(parentNode != null) {
					if(recalculateView || parentNode.DerivedOrientation != lastParentOrientation ||
						parentNode.DerivedPosition != lastParentPosition) {
						// we are out of date with the parent scene node
						lastParentOrientation = parentNode.DerivedOrientation;
						lastParentPosition = parentNode.DerivedPosition;
						recalculateView = true;
					}
				}

				// deriving direction from linked plane?
				if(linkedReflectionPlane != null &&
					!(lastLinkedReflectionPlane == linkedReflectionPlane.DerivedPlane)) {
					reflectionPlane = linkedReflectionPlane.DerivedPlane;
					reflectionMatrix = MathUtil.BuildReflectionMatrix(reflectionPlane);
					lastLinkedReflectionPlane = linkedReflectionPlane.DerivedPlane;
					recalculateView = true;
				}

				return recalculateView;
			}
		}

		/// <summary>
		///		Evaluates whether or not the view frustum is out of date.
		/// </summary>
		protected virtual bool IsFrustumOutOfDate {
			get {
				// deriving custom near plane from linked plane?
				if(useObliqueDepthProjection) {
                    // Out of date when view out of data since plane needs to be in view space
					if (IsViewOutOfDate)
                        recalculateFrustum = true;

					// update derived plane
					if(linkedObliqueProjPlane != null &&
						!(lastLinkedObliqueProjPlane == linkedObliqueProjPlane.DerivedPlane)) {

						obliqueProjPlane = linkedObliqueProjPlane.DerivedPlane;
						lastLinkedObliqueProjPlane = obliqueProjPlane;
                        recalculateFrustum = true;
					}
				}
				return recalculateFrustum;
			}
		}

		/// <summary>
		///     Returns the reflection matrix of the camera if appropriate.
		/// </summary>
		public virtual Matrix4 ReflectionMatrix {
			get {
				return reflectionMatrix;
			}
		}

		/// <summary>
		///     Returns the reflection plane of the camera if appropriate.
		/// </summary>
		public virtual Plane ReflectionPlane {
			get {
				return reflectionPlane;
			}
		}

		/// <summary>
		///    Gets the 'standard' projection matrix for this camera, ie the 
		///    projection matrix which conforms to standard right-handed rules.
		/// </summary>
		/// <remarks>
		///    This differs from the rendering-API dependent ProjectionMatrix
		///    in that it always returns a right-handed projection matrix result 
		///    no matter what rendering API is being used - this is required for
		///    vertex and fragment programs for example. However, the resulting depth
		///    range may still vary between render systems since D3D uses [0,1] and 
		///    GL uses [-1,1], and the range must be kept the same between programmable
		///    and fixed-function pipelines.
        ///    <para/>
        ///    This corresponds to the Ogre mProjMatrixRSDepth and
        ///    getProjectionMatrixWithRSDepth
		/// </remarks>
		public virtual Matrix4 ProjectionMatrixWithRSDepth {
			get {
				UpdateFrustum();

				return projectionMatrixWithRSDepth;
			}
		}

		public virtual Matrix4 ProjectionMatrixRS {
			get {
				UpdateFrustum();

				return projectionMatrixRS;
			}
		}

        public bool CustomViewMatrix {
            get {
                return customViewMatrix;
            }
            set {
                customViewMatrix = value;
            }
        }
        
        public bool CustomProjectionMatrix {
            get {
                return customProjectionMatrix;
            }
            set {
                customProjectionMatrix = value;
            }
        }
        
		#endregion Properties

		#region Methods

		/// <summary>
		///		Disables any custom near clip plane.
		/// </summary>
		public virtual void DisableCustomNearClipPlane() {
			useObliqueDepthProjection = false;
			linkedObliqueProjPlane = null;
			InvalidateFrustum();
		}

		/// <summary>
		///     Disables reflection modification previously turned on with <see cref="EnableReflection"/>.
		/// </summary>
		public virtual void DisableReflection() {
			isReflected = false;
			lastLinkedReflectionPlane.Normal = Vector3.Zero;
			InvalidateView();
		}

		/// <summary>
		///		Links the frustum to a custom near clip plane, which can be used
		///		to clip geometry in a custom manner without using user clip planes.
		/// </summary>
		/// <remarks>
		///		<p>
		///		There are several applications for clipping a scene arbitrarily by
		///		a single plane; the most common is when rendering a reflection to 
		///		a texture, and you only want to render geometry that is above the 
		///		water plane (to do otherwise results in artefacts). Whilst it is
		///		possible to use user clip planes, they are not supported on all
		///		cards, and sometimes are not hardware accelerated when they are
		///		available. Instead, where a single clip plane is involved, this
		///		technique uses a 'fudging' of the near clip plane, which is 
		///		available and fast on all hardware, to perform as the arbitrary
		///		clip plane. This does change the shape of the frustum, leading 
		///		to some depth buffer loss of precision, but for many of the uses of
		///		this technique that is not an issue.</p>
		///		<p>
		///		This version of the method links to a plane, rather than requiring
		///		a by-value plane definition, and therefore you can 
		///		make changes to the plane (e.g. by moving / rotating the node it is
		///		attached to) and they will automatically affect this object.
		///		</p>
		///		<p>This technique only works for perspective projection.</p>
		/// </remarks>
		/// <param name="plane">The plane to link to to perform the clipping.</param>
		public virtual void EnableCustomNearClipPlane(IDerivedPlaneProvider plane) {
			useObliqueDepthProjection = true;
			linkedObliqueProjPlane = plane;
			obliqueProjPlane = plane.DerivedPlane;
			InvalidateFrustum();
		}

		/// <summary>
		///		Links the frustum to a custom near clip plane, which can be used
		///		to clip geometry in a custom manner without using user clip planes.
		/// </summary>
		/// <remarks>
		///		<p>
		///		There are several applications for clipping a scene arbitrarily by
		///		a single plane; the most common is when rendering a reflection to 
		///		a texture, and you only want to render geometry that is above the 
		///		water plane (to do otherwise results in artefacts). Whilst it is
		///		possible to use user clip planes, they are not supported on all
		///		cards, and sometimes are not hardware accelerated when they are
		///		available. Instead, where a single clip plane is involved, this
		///		technique uses a 'fudging' of the near clip plane, which is 
		///		available and fast on all hardware, to perform as the arbitrary
		///		clip plane. This does change the shape of the frustum, leading 
		///		to some depth buffer loss of precision, but for many of the uses of
		///		this technique that is not an issue.</p>
		///		<p>
		///		This version of the method links to a plane, rather than requiring
		///		a by-value plane definition, and therefore you can 
		///		make changes to the plane (e.g. by moving / rotating the node it is
		///		attached to) and they will automatically affect this object.
		///		</p>
		///		<p>This technique only works for perspective projection.</p>
		/// </remarks>
		/// <param name="plane">The plane to link to to perform the clipping.</param>
		public virtual void EnableCustomNearClipPlane(Plane plane) {
			useObliqueDepthProjection = true;
			linkedObliqueProjPlane = null;
			obliqueProjPlane = plane;
			InvalidateFrustum();
		}

		/// <summary>
		///     Modifies this camera so it always renders from the reflection of itself through the
		///     plane specified.
		/// </summary>
		/// <remarks>
		///     This is obviously useful for rendering planar reflections.
		/// </remarks>
		/// <param name="plane"></param>
		public virtual void EnableReflection(Plane plane) {
			isReflected = true;
			reflectionPlane = plane;
			linkedReflectionPlane = null;
			reflectionMatrix = MathUtil.BuildReflectionMatrix(plane);
			InvalidateView();
		}

		/// <summary>
		///		Modifies this frustum so it always renders from the reflection of itself through the
		///		plane specified. Note that this version of the method links to a plane
		///		so that changes to it are picked up automatically.
		/// </summary>
		/// <remarks>This is obviously useful for performing planar reflections.</remarks>
		/// <param name="plane"></param>
		public virtual void EnableReflection(IDerivedPlaneProvider plane) {
			isReflected = true;
			linkedReflectionPlane = plane;
			reflectionPlane = linkedReflectionPlane.DerivedPlane;
			reflectionMatrix = MathUtil.BuildReflectionMatrix(reflectionPlane);
			lastLinkedReflectionPlane = reflectionPlane;
			InvalidateView();
		}

		/// <summary>
		///		Get the derived position of this frustum.
		/// </summary>
		/// <returns></returns>
		protected virtual Vector3 GetPositionForViewUpdate() {
			return lastParentPosition;
		}

		/// <summary>
		///		Get the derived orientation of this frustum.
		/// </summary>
		/// <returns></returns>
		protected virtual Quaternion GetOrientationForViewUpdate() {
			return lastParentOrientation;
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public virtual bool IsObjectVisible(AxisAlignedBox box) {
			// this overload doesnt care about the clipping plane, but we gotta
			// pass in something to the out param anyway
			FrustumPlane dummy;
			return IsObjectVisible(box, out dummy);
		}

		/// <summary>
		///		Tests whether the given box is visible in the Frustum.
		///	 </summary>
		/// <param name="box"> Bounding box to be checked.</param>
		/// <param name="culledBy">
		///		Optional FrustrumPlane params which will be filled by the plane which culled
		///		the box if the result was false.
		///	</param>
		/// <returns>True if the box is visible, otherwise false.</returns>
		public virtual bool IsObjectVisible(AxisAlignedBox box, out FrustumPlane culledBy) {
			// Null boxes are always invisible
			if (box.IsNull) {
				culledBy = FrustumPlane.None;
				return false;
			}

            // Make any pending updates to the calculated frustum planes
            UpdateFrustumPlanes();
            
			// Get corners of the box
			Vector3[] corners = box.Corners;

			// For each plane, see if all points are on the negative side
			// If so, object is not visible
			for (int plane = 0; plane < 6; plane++) {
				// skip far plane if infinite view frustum
				if(farDistance == 0 && plane == (int)FrustumPlane.Far) {
					continue;
				}

				if (planes[plane].GetSide(corners[0]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[1]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[2]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[3]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[4]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[5]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[6]) == PlaneSide.Negative &&
					planes[plane].GetSide(corners[7]) == PlaneSide.Negative) {
					// ALL corners on negative side therefore out of view
					culledBy = (FrustumPlane)plane;
					return false;
				}
			}

			// box is not culled
			culledBy = FrustumPlane.None;
			return true;
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public virtual bool IsObjectVisible(Sphere sphere) {
			// this overload doesnt care about the clipping plane, but we gotta
			// pass in something to the out param anyway
			FrustumPlane dummy;
			return IsObjectVisible(sphere, out dummy);
		}

		/// <summary>
		///		Tests whether the given sphere is in the viewing frustum.
		/// </summary>
		/// <param name="sphere">Bounding sphere to be checked.</param>
		/// <param name="culledBy">
		///		Optional FrustrumPlane params which will be filled by the plane which culled
		///		the box if the result was false.
		///	</param>
		/// <returns>True if the box is visible, otherwise false.</returns>
		public virtual bool IsObjectVisible(Sphere sphere, out FrustumPlane culledBy) {
			// Make any pending updates to the calculated frustum
			UpdateFrustumPlanes();

			// For each plane, see if sphere is on negative side
			// If so, object is not visible
			for (int plane = 0; plane < 6; plane++) {
				if(farDistance == 0 && plane == (int)FrustumPlane.Far) {
					continue;
				}

				// If the distance from sphere center to plane is negative, and 'more negative' 
				// than the radius of the sphere, sphere is outside frustum
				if (planes[plane].GetDistance(sphere.Center) < -sphere.Radius) {
					// ALL corners on negative side therefore out of view
					culledBy = (FrustumPlane)plane;
					return false;
				}
			}

			// sphere is not culled
			culledBy = FrustumPlane.None;
			return true;
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public virtual bool IsObjectVisible(Vector3 vertex) {
			// this overload doesnt care about the clipping plane, but we gotta
			// pass in something to the out param anyway
			FrustumPlane dummy;
			return IsObjectVisible(vertex, out dummy);
		}

		/// <summary>
		///		Tests whether the given 3D point is in the viewing frustum.
		/// </summary>
		/// <param name="vector">3D point to check for frustum visibility.</param>
		/// <param name="culledBy">
		///		Optional FrustrumPlane params which will be filled by the plane which culled
		///		the box if the result was false.
		///	</param>
		/// <returns>True if the box is visible, otherwise false.</returns>
		public virtual bool IsObjectVisible(Vector3 vertex, out FrustumPlane culledBy) {
			// Make any pending updates to the calculated frustum planes
			UpdateFrustumPlanes();

			// For each plane, see if all points are on the negative side
			// If so, object is not visible
			for (int plane = 0; plane < 6; plane++) {
				if(farDistance == 0 && plane == (int)FrustumPlane.Far) {
					continue;
				}

				if (planes[plane].GetSide(vertex) == PlaneSide.Negative) {
					// ALL corners on negative side therefore out of view
					culledBy = (FrustumPlane)plane;
					return false;
				}
			}

			// vertex is not culled
			culledBy = FrustumPlane.None;
			return true;
		}

		public virtual bool ProjectSphere(Sphere sphere, out float left, out float top, out float right, out float bottom) {
			// initialise
			left = bottom = -1.0f;
			right = top = 1.0f;

            // Don't use getViewMatrix here, incase overrided by camera and return a cull frustum view matrix
            UpdateView();
			// Transform light position into camera space
			Vector3 eyeSpacePos = this.viewMatrix.TransformAffine(sphere.Center);

			if (eyeSpacePos.z < 0) {
				float r = sphere.Radius;
				// early-exit
				if (eyeSpacePos.LengthSquared <= r * r)
					return false;

                UpdateFrustum();
				Vector3 screenSpacePos = this.projectionMatrix * eyeSpacePos;

				// perspective attenuate
				Vector3 spheresize = new Vector3(r, r, eyeSpacePos.z);
				spheresize = this.projectionMatrix * spheresize;

				float possLeft = screenSpacePos.x - spheresize.x;
				float possRight = screenSpacePos.x + spheresize.x;
				float possTop = screenSpacePos.y + spheresize.y;
				float possBottom = screenSpacePos.y - spheresize.y;

				left = MathUtil.Max(-1.0f, possLeft);
				right = MathUtil.Min(1.0f, possRight);
				top = MathUtil.Min(1.0f, possTop);
				bottom = MathUtil.Max(-1.0f, possBottom);
			}

			return (left != -1.0f) || (top != 1.0f) || (right != 1.0f) || (bottom != -1.0f);
		}

		/// <summary>
		///     Signal to update frustum information.
		/// </summary>
		protected virtual void InvalidateFrustum() {
			recalculateFrustum = true;
            recalculateFrustumPlanes = true;
            recalculateWorldSpaceCorners = true;
            recalculateVertexData = true;
		}

		/// <summary>
		///     Signal to update view information.
		/// </summary>
		protected virtual void InvalidateView() {
			recalculateView = true;
            recalculateFrustumPlanes = true;
            recalculateWorldSpaceCorners = true;
		}

		/// <summary>
		///		Updates the frustum data.
		/// </summary>
		protected virtual void UpdateFrustum() {
			if(IsFrustumOutOfDate)
				UpdateFrustumImpl();
        }
        
        protected virtual void CalculateProjectionParameters(out float left, out float right, out float bottom, out float top) { 
            if (customProjectionMatrix) {
                // Convert clipspace corners to camera space
                Matrix4 invProj = projectionMatrix.Inverse();
                Vector3 topLeft = new Vector3(-0.5f, 0.5f, 0.0f);
                Vector3 bottomRight = new Vector3(0.5f, -0.5f, 0.0f);

                topLeft = invProj * topLeft;
                bottomRight = invProj * bottomRight;

                left = topLeft.x;
                top = topLeft.y;
                right = bottomRight.x;
                bottom = bottomRight.y;
            }
            else {
                // Calculate general projection parameters

                float thetaY = MathUtil.DegreesToRadians(fieldOfView * 0.5f);
                float tanThetaY = MathUtil.Tan(thetaY);
                float tanThetaX = tanThetaY * aspectRatio;

                // Unknow how to apply frustum offset to orthographic camera, just ignore here
                float nearFocal = (projectionType == Projection.Perspective) ? nearDistance / focalLength : 0;
                float nearOffsetX = frustumOffset.x * nearFocal;
                float nearOffsetY = frustumOffset.y * nearFocal;
                float half_w = tanThetaX * nearDistance;
                float half_h = tanThetaY * nearDistance;

                left   = - half_w + nearOffsetX;
                right  = + half_w + nearOffsetX;
                bottom = - half_h + nearOffsetY;
                top    = + half_h + nearOffsetY;
            }
        }
        
        protected virtual void UpdateFrustumImpl() {
            float left, right, bottom, top;
            CalculateProjectionParameters(out left, out right, out bottom, out top);

            if (!customProjectionMatrix) {
                // The code below will dealing with general projection 
                // parameters, similar glFrustum and glOrtho.
                // Doesn't optimise manually except division operator, so the 
                // code more self-explaining.

                float inv_w = 1f / (right - left);
                float inv_h = 1f / (top - bottom);
                float inv_d = 1f / (farDistance - nearDistance);

                // Recalc if frustum params changed
                if (projectionType == Projection.Perspective) {
                    // Calc matrix elements
                    float A = 2f * nearDistance * inv_w;
                    float B = 2f * nearDistance * inv_h;
                    float C = (right + left) * inv_w;
                    float D = (top + bottom) * inv_h;
                    float q, qn;
                    if (farDistance == 0f)
                    {
                        // Infinite far plane
                        q = InfiniteFarPlaneAdjust - 1f;
                        qn = nearDistance * (InfiniteFarPlaneAdjust - 2f);
                    }
                    else
                    {
                        q = - (farDistance + nearDistance) * inv_d;
                        qn = -2 * (farDistance * nearDistance) * inv_d;
                    }
                
                    // NB: This creates 'uniform' perspective projection matrix,
                    // which depth range [-1,1], right-handed rules
                    //
                    // [ A   0   C   0  ]
                    // [ 0   B   D   0  ]
                    // [ 0   0   q   qn ]
                    // [ 0   0   -1  0  ]
                    //
                    // A = 2 * near / (right - left)
                    // B = 2 * near / (top - bottom)
                    // C = (right + left) / (right - left)
                    // D = (top + bottom) / (top - bottom)
                    // q = - (far + near) / (far - near)
                    // qn = - 2 * (far * near) / (far - near)

                    projectionMatrix = Matrix4.Zero;
                    projectionMatrix.m00 = A;
                    projectionMatrix.m02 = C;
                    projectionMatrix.m11 = B;
                    projectionMatrix.m12 = D;
                    projectionMatrix.m22 = q;
                    projectionMatrix.m23 = qn;
                    projectionMatrix.m32 = -1;

                    if (useObliqueDepthProjection) {
                        // Translate the plane into view space

                        // Don't use getViewMatrix here, incase overrided by 
                        // camera and return a cull frustum view matrix
                        UpdateView();
                        Plane plane = viewMatrix * obliqueProjPlane;

                        // Thanks to Eric Lenyel for posting this calculation 
                        // at www.terathon.com

                        // Calculate the clip-space corner point opposite the 
                        // clipping plane
                        // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
                        // transform it into camera space by multiplying it
                        // by the inverse of the projection matrix

                        /* generalised version
                        Vector4 q = matrix.inverse() * 
                        Vector4(Math::Sign(plane.normal.x), 
                        Math::Sign(plane.normal.y), 1.0f, 1.0f);
                        */
                        Vector4 qv = Vector4.Zero;
                        qv.x = (Math.Sign(plane.Normal.x) + projectionMatrix.m02) / projectionMatrix.m00;
                        qv.y = (Math.Sign(plane.Normal.y) + projectionMatrix.m12) / projectionMatrix.m11;
                        qv.z = -1f;
                        qv.w = (1f + projectionMatrix.m22) / projectionMatrix.m23;

                        // Calculate the scaled plane vector
                        Vector4 clipPlane4d = new Vector4(plane.Normal.x, plane.Normal.y, plane.Normal.z, plane.D);
                        Vector4 c = clipPlane4d * (2f / (clipPlane4d.Dot(qv)));

                        // Replace the third row of the projection matrix
                        projectionMatrix.m20 = c.x;
                        projectionMatrix.m21 = c.y;
                        projectionMatrix.m22 = c.z + 1f;
                        projectionMatrix.m23 = c.w; 
                    }
                } // perspective
                else if (projectionType == Projection.Orthographic) {
                    float A = 2f * inv_w;
                    float B = 2f * inv_h;
                    float C = - (right + left) * inv_w;
                    float D = - (top + bottom) * inv_h;
                    float q, qn;
                    if (farDistance == 0f)
                    {
                        // Can not do infinite far plane here, avoid divided zero only
                        q = - InfiniteFarPlaneAdjust / nearDistance;
                        qn = - InfiniteFarPlaneAdjust - 1f;
                    }
                    else
                    {
                        q = - 2f * inv_d;
                        qn = - (farDistance + nearDistance)  * inv_d;
                    }

                    // NB: This creates 'uniform' orthographic projection matrix,
                    // which depth range [-1,1], right-handed rules
                    //
                    // [ A   0   0   C  ]
                    // [ 0   B   0   D  ]
                    // [ 0   0   q   qn ]
                    // [ 0   0   0   1  ]
                    //
                    // A = 2 * / (right - left)
                    // B = 2 * / (top - bottom)
                    // C = - (right + left) / (right - left)
                    // D = - (top + bottom) / (top - bottom)
                    // q = - 2 / (far - near)
                    // qn = - (far + near) / (far - near)

                    projectionMatrix = Matrix4.Zero;
                    projectionMatrix.m00 = A;
                    projectionMatrix.m03 = C;
                    projectionMatrix.m11 = B;
                    projectionMatrix.m13 = D;
                    projectionMatrix.m22 = q;
                    projectionMatrix.m23 = qn;
                    projectionMatrix.m33 = 1f;
                } // ortho
            } // CustomProjectionMatrix

            RenderSystem renderSystem = Root.Instance.RenderSystem;
            // API specific
            projectionMatrixRS = renderSystem.ConvertProjectionMatrix(projectionMatrix, false);
            // API specific for Gpu Programs
            projectionMatrixWithRSDepth = renderSystem.ConvertProjectionMatrix(projectionMatrix, true);

            // Calculate bounding box (local)
            // Box is from 0, down -Z, max dimensions as determined from far plane
            // If infinite view frustum just pick a far value
            float farDist = (farDistance == 0) ? 100000f : farDistance;
            // Near plane bounds
            Vector3 min = new Vector3(left, bottom, -farDist);
            Vector3 max = new Vector3(right, top, 0);
            if (projectionType == Projection.Perspective) {
                // Merge with far plane bounds
                float radio = farDist / nearDistance;
                min.Floor(new Vector3(left * radio, bottom * radio, -farDist));
                max.Ceil(new Vector3(right * radio, top * radio, 0));
            }
            boundingBox.SetExtents(min, max);

            recalculateFrustum = false;

            // Signal to update frustum clipping planes
            recalculateFrustumPlanes = true;
        }


        void UpdateVertexData() {
            if (recalculateVertexData) {
                if (vertexData.vertexBufferBinding.BindingCount <= 0) {
                    // Initialise vertex & index data
                    vertexData.vertexDeclaration.AddElement(0, 0, VertexElementType.Float3, VertexElementSemantic.Position);
                    vertexData.vertexCount = 32;
                    vertexData.vertexStart = 0;
                    vertexData.vertexBufferBinding.SetBinding(0,
                        HardwareBufferManager.Instance.CreateVertexBuffer(sizeof(float)*3, 32, BufferUsage.DynamicWriteOnly));
                }
                
                // Note: Even though we can dealing with general projection matrix here,
                //       but because it's incompatibly with infinite far plane, thus, we
                //       still need to working with projection parameters.

                // Calc near plane corners
                float vpLeft, vpRight, vpBottom, vpTop;
                CalculateProjectionParameters(out vpLeft, out vpRight, out vpBottom, out vpTop);

                // Treat infinite fardist as some arbitrary far value
                float actualFar = (farDistance == 0f) ? 100000f : farDistance;

                // Calc far palne corners
                float radio = projectionType == Projection.Perspective ? actualFar / nearDistance : 1;
                float farLeft = vpLeft * radio;
                float farRight = vpRight * radio;
                float farBottom = vpBottom * radio;
                float farTop = vpTop * radio;

                // Calculate vertex positions
                // 0 is the origin
                // 1, 2, 3, 4 are the points on the near plane, top left first, clockwise
                // 5, 6, 7, 8 are the points on the far plane, top left first, clockwise
                HardwareVertexBuffer buffer = vertexData.vertexBufferBinding.GetBuffer(0);

                IntPtr posPtr = buffer.Lock(BufferLocking.Discard);

                unsafe {
                    float* pPos = (float*)posPtr.ToPointer();

                    // near plane (remember frustum is going in -Z direction)
                    *pPos++ = vpLeft;  *pPos++ = vpTop;    *pPos++ = -nearDistance;
                    *pPos++ = vpRight; *pPos++ = vpTop;    *pPos++ = -nearDistance;

                    *pPos++ = vpRight; *pPos++ = vpTop;    *pPos++ = -nearDistance;
                    *pPos++ = vpRight; *pPos++ = vpBottom; *pPos++ = -nearDistance;

                    *pPos++ = vpRight; *pPos++ = vpBottom; *pPos++ = -nearDistance;
                    *pPos++ = vpLeft;  *pPos++ = vpBottom; *pPos++ = -nearDistance;

                    *pPos++ = vpLeft;  *pPos++ = vpBottom; *pPos++ = -nearDistance;
                    *pPos++ = vpLeft;  *pPos++ = vpTop;    *pPos++ = -nearDistance;

                    // far plane (remember frustum is going in -Z direction)
                    *pPos++ = farLeft;  *pPos++ = farTop;    *pPos++ = -actualFar;
                    *pPos++ = farRight; *pPos++ = farTop;    *pPos++ = -actualFar;

                    *pPos++ = farRight; *pPos++ = farTop;    *pPos++ = -actualFar;
                    *pPos++ = farRight; *pPos++ = farBottom; *pPos++ = -actualFar;

                    *pPos++ = farRight; *pPos++ = farBottom; *pPos++ = -actualFar;
                    *pPos++ = farLeft;  *pPos++ = farBottom; *pPos++ = -actualFar;

                    *pPos++ = farLeft;  *pPos++ = farBottom; *pPos++ = -actualFar;
                    *pPos++ = farLeft;  *pPos++ = farTop;    *pPos++ = -actualFar;

                    // Sides of the pyramid
                    *pPos++ = 0.0f;    *pPos++ = 0.0f;   *pPos++ = 0.0f;
                    *pPos++ = vpLeft; *pPos++ = vpTop; *pPos++ = -nearDistance;

                    *pPos++ = 0.0f;    *pPos++ = 0.0f;   *pPos++ = 0.0f;
                    *pPos++ = vpRight; *pPos++ = vpTop;    *pPos++ = -nearDistance;

                    *pPos++ = 0.0f;    *pPos++ = 0.0f;   *pPos++ = 0.0f;
                    *pPos++ = vpRight; *pPos++ = vpBottom; *pPos++ = -nearDistance;

                    *pPos++ = 0.0f;    *pPos++ = 0.0f;   *pPos++ = 0.0f;
                    *pPos++ = vpLeft;  *pPos++ = vpBottom; *pPos++ = -nearDistance;

                    // Sides of the box
                    *pPos++ = vpLeft;  *pPos++ = vpTop;		*pPos++ = -nearDistance;
                    *pPos++ = farLeft; *pPos++ = farTop;	*pPos++ = -actualFar;

                    *pPos++ = vpRight;  *pPos++ = vpTop;	*pPos++ = -nearDistance;
                    *pPos++ = farRight; *pPos++ = farTop;   *pPos++ = -actualFar;

                    *pPos++ = vpRight;  *pPos++ = vpBottom;		*pPos++ = -nearDistance;
                    *pPos++ = farRight; *pPos++ = farBottom;	*pPos++ = -actualFar;

                    *pPos++ = vpLeft;   *pPos++ = vpBottom;		*pPos++ = -nearDistance;
                    *pPos++ = farLeft;  *pPos++ = farBottom;	*pPos++ = -actualFar;
                }

                // don't forget to unlock!
                buffer.Unlock();

                recalculateVertexData = false;
            }
        }

		/// <summary>
		///		Updates the view matrix.
		/// </summary>
		protected virtual void UpdateView() {
			// check if the view is out of date
			if(IsViewOutOfDate)
                UpdateViewImpl();
        }
        
        protected virtual void UpdateViewImpl() {
            
            // ----------------------
            // Update the view matrix
            // ----------------------
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
            if (!customViewMatrix) {
                Quaternion orientation = GetOrientationForViewUpdate();
                Vector3 position = GetPositionForViewUpdate();
                Matrix3 rotation = orientation.ToRotationMatrix();

                // make the translation relative to the new axis
                Matrix3 rotationT = rotation.Transpose();
                Vector3 translation = -rotationT * position;

                // Make final matrix
                viewMatrix = Matrix4.Identity;
                viewMatrix = rotationT; // fills upper 3x3
                viewMatrix.m03 = translation.x;
                viewMatrix.m13 = translation.y;
                viewMatrix.m23 = translation.z;

                // deal with reflections
                if(isReflected) {
                    viewMatrix = viewMatrix * reflectionMatrix;
                }
            }

            recalculateView = false;
            
            // Signal to update frustum clipping planes
            recalculateFrustumPlanes = true;
            // Signal to update world space corners
            recalculateWorldSpaceCorners = true;
            // Signal to update frustum if oblique plane enabled,
            // since plane needs to be in view space
            if (useObliqueDepthProjection)
                recalculateFrustum = true;
        }
        
        protected void UpdateWorldSpaceCornersImpl() {

            Matrix4 eyeToWorld = viewMatrix.InverseAffine;
            
            // Note: Even though we can dealing with general projection matrix here,
            //       but because it's incompatibly with infinite far plane, thus, we
            //       still need to working with projection parameters.

            // Calc near plane corners
            float nearLeft, nearRight, nearBottom, nearTop;
            CalculateProjectionParameters(out nearLeft, out nearRight, out nearBottom, out nearTop);

            // Treat infinite fardist as some arbitrary far value
            float farDist = (farDistance == 0f) ? 100000f : farDistance;

            // Calc far palne corners
            float radio = projectionType == Projection.Perspective ? farDist / nearDistance : 1f;
            float farLeft = nearLeft * radio;
            float farRight = nearRight * radio;
            float farBottom = nearBottom * radio;
            float farTop = nearTop * radio;

            // near
            worldSpaceCorners[0] = eyeToWorld.TransformAffine(new Vector3(nearRight, nearTop,    -nearDistance));
            worldSpaceCorners[1] = eyeToWorld.TransformAffine(new Vector3(nearLeft,  nearTop,    -nearDistance));
            worldSpaceCorners[2] = eyeToWorld.TransformAffine(new Vector3(nearLeft,  nearBottom, -nearDistance));
            worldSpaceCorners[3] = eyeToWorld.TransformAffine(new Vector3(nearRight, nearBottom, -nearDistance));
            // far
            worldSpaceCorners[4] = eyeToWorld.TransformAffine(new Vector3(farRight,  farTop,     -farDist));
            worldSpaceCorners[5] = eyeToWorld.TransformAffine(new Vector3(farLeft,   farTop,     -farDist));
            worldSpaceCorners[6] = eyeToWorld.TransformAffine(new Vector3(farLeft,   farBottom,  -farDist));
            worldSpaceCorners[7] = eyeToWorld.TransformAffine(new Vector3(farRight,  farBottom,  -farDist));

            recalculateWorldSpaceCorners = false;
        }

        public void UpdateWorldSpaceCorners() {
            UpdateView();
            if (recalculateWorldSpaceCorners)
                UpdateWorldSpaceCornersImpl();
        }

        public void UpdateFrustumPlanesImpl() {
            // -------------------------
            // Update the frustum planes
            // -------------------------
            Matrix4 combo = projectionMatrix * viewMatrix;

            planes[(int)FrustumPlane.Left].Normal.x = combo.m30 + combo.m00;
            planes[(int)FrustumPlane.Left].Normal.y = combo.m31 + combo.m01;
            planes[(int)FrustumPlane.Left].Normal.z = combo.m32 + combo.m02;
            planes[(int)FrustumPlane.Left].D = combo.m33 + combo.m03;

            planes[(int)FrustumPlane.Right].Normal.x = combo.m30 - combo.m00;
            planes[(int)FrustumPlane.Right].Normal.y = combo.m31 - combo.m01;
            planes[(int)FrustumPlane.Right].Normal.z = combo.m32 - combo.m02;
            planes[(int)FrustumPlane.Right].D = combo.m33 - combo.m03;

            planes[(int)FrustumPlane.Top].Normal.x = combo.m30 - combo.m10;
            planes[(int)FrustumPlane.Top].Normal.y = combo.m31 - combo.m11;
            planes[(int)FrustumPlane.Top].Normal.z = combo.m32 - combo.m12;
            planes[(int)FrustumPlane.Top].D = combo.m33 - combo.m13;

            planes[(int)FrustumPlane.Bottom].Normal.x = combo.m30 + combo.m10;
            planes[(int)FrustumPlane.Bottom].Normal.y = combo.m31 + combo.m11;
            planes[(int)FrustumPlane.Bottom].Normal.z = combo.m32 + combo.m12;
            planes[(int)FrustumPlane.Bottom].D = combo.m33 + combo.m13;

            planes[(int)FrustumPlane.Near].Normal.x = combo.m30 + combo.m20;
            planes[(int)FrustumPlane.Near].Normal.y = combo.m31 + combo.m21;
            planes[(int)FrustumPlane.Near].Normal.z = combo.m32 + combo.m22;
            planes[(int)FrustumPlane.Near].D = combo.m33 + combo.m23;

            planes[(int)FrustumPlane.Far].Normal.x = combo.m30 - combo.m20;
            planes[(int)FrustumPlane.Far].Normal.y = combo.m31 - combo.m21;
            planes[(int)FrustumPlane.Far].Normal.z = combo.m32 - combo.m22;
            planes[(int)FrustumPlane.Far].D = combo.m33 - combo.m23;

            // renormalize any normals which were not unit length
            for(int i = 0; i < 6; i++) {
                float length = planes[i].Normal.Normalize();
                planes[i].D /= length;
            }

            recalculateFrustumPlanes = false;
        }

        public void UpdateFrustumPlanes() {
            UpdateView();
            UpdateFrustum();

            if (recalculateFrustumPlanes)
                UpdateFrustumPlanesImpl();
        }

        public virtual Plane GetFrustumPlane(FrustumPlane plane) {
				UpdateView();
				int index = (int)plane;
				return planes[index];
        }
        
        #endregion Methods

		#region Overloaded operators

		/// <summary>
		///		An indexer that accepts a FrustumPlane enum value and return the appropriate plane side of the Frustum.
		/// </summary>
		public Plane this[FrustumPlane plane] {
			get {
				// make any pending updates to the calculated frustum
				// TODO: Was causing a stack overflow, revisit
				UpdateView();

				// convert the incoming plan enum type to a int
				int index = (int)plane;

				// access the planes array by index
				return planes[index];
			}
		}

		#endregion

		#region SceneObject Members

		/// <summary>
		///    Local bounding radius of this camera.
		/// </summary>
		public override float BoundingRadius {
			get {
				return (farDistance == 0) ? InfiniteFarPlaneDistance : farDistance;
			}
		}

		/// <summary>
		///     Returns the bounding box for this frustum.
		/// </summary>
		public override AxisAlignedBox BoundingBox {
			get {
				return boundingBox;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		public override void NotifyCurrentCamera(Camera camera) {
			// do nothing
		}

		/// <summary>
		///     Implemented to add outself to the rendering queue.
		/// </summary>
		/// <param name="queue"></param>
		public override void UpdateRenderQueue(RenderQueue queue) {
			if(isVisible) {
				queue.AddRenderable(this);
			}
		}

		#endregion SceneObject Members

		#region IRenderable Members

		public bool CastsShadows {
			get {
				return false;
			}
		}
		
		/// <summary>
		///     Returns the material to use when rendering this frustum.
		/// </summary>
		public Material Material {
			get {
				return material;
			}
		}

		/// <summary>
		///     Just returns the best technique for our material.
		/// </summary>
		public Technique Technique {
			get {
				return material.GetBestTechnique();
			}
		}

		public void GetRenderOperation(RenderOperation op) {
			UpdateView();
			UpdateFrustum();

			op.operationType = OperationType.LineList;
			op.useIndices = false;
			op.vertexData = vertexData;
		}

		public void GetWorldTransforms(Matrix4[] matrices) {
			if(parentNode != null) {
				parentNode.GetWorldTransforms(matrices);
			}
		}

		/// <summary>
		///     Returns a dummy list since we won't be lit.
		/// </summary>
		public List<Light> Lights {
			get {
				return dummyLightList;
			}
		}

		public bool NormalizeNormals {
			get {
				return false;
			}
		}

		public ushort NumWorldTransforms {
			get {
				return 1;
			}
		}

		public bool UseIdentityProjection {
			get {
				return false;
			}
		}

		public bool UseIdentityView {
			get {
				return false;
			}
		}

		public Axiom.Graphics.SceneDetailLevel RenderDetail {
			get {
				return SceneDetailLevel.Solid;
			}
		}

		public Quaternion WorldOrientation {
			get {
				if(parentNode != null) {
					return parentNode.DerivedOrientation;
				}
				else {
					return Quaternion.Identity;
				}
			}
		}

		public Vector3 WorldPosition {
			get {
				if(parentNode != null) {
					return parentNode.DerivedPosition;
				}
				else {
					return Vector3.Zero;
				}
			}
		}

		public virtual Vector3[] WorldSpaceCorners {
			get {
				UpdateWorldSpaceCorners();

				return worldSpaceCorners;
			}
		}

        public bool IsCustomViewMatrixEnabled {
            get {
                return customViewMatrix;
            }
        }

        public bool IsCustomProjectionMatrixEnabled {
            get {
                return customProjectionMatrix;
            }
        }

		public float GetSquaredViewDepth(Camera camera) {
			if(parentNode != null) {
				return (camera.DerivedPosition - parentNode.DerivedPosition).LengthSquared;
			}
			else {
				return 0;
			}
		}

		public Vector4 GetCustomParameter(int index) {
			if(customParams[index] == null) {
				throw new Exception("A parameter was not found at the given index");
			}
			else {
				return (Vector4)customParams[index];
			}
		}

		public void SetCustomParameter(int index, Vector4 val) {
			customParams[index] = val;
		}

		public void UpdateCustomGpuParameter(GpuProgramParameters.AutoConstantEntry entry, GpuProgramParameters gpuParams) {
			if(customParams[entry.data] != null) {
				gpuParams.SetConstant(entry.index, (Vector4)customParams[entry.data]);
			}
		}

        public void SetCustomViewMatrix(bool enable) {
            SetCustomViewMatrix(enable, Matrix4.Identity);
        }
        
        public void SetCustomViewMatrix(bool enable, Matrix4 viewMatrix) {
            customViewMatrix = enable;
            if (enable) {
                Debug.Assert(viewMatrix.IsAffine);
                this.viewMatrix = viewMatrix;
            }
            InvalidateView();
        }

        public void SetCustomProjectionMatrix(bool enable, Matrix4 projectionMatrix) {
            customProjectionMatrix = enable;
            if (enable)
                 this.projectionMatrix = projectionMatrix;
             InvalidateFrustum();
        }

        public void SetCustomProjectionMatrix(bool enable) {
            SetCustomProjectionMatrix(enable, Matrix4.Identity);
        }

		#endregion
	}
}
