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
using System.Collections.Generic;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.Core {
	/// <summary>
	///		A viewpoint from which the scene will be rendered.
	/// </summary>
	///<remarks>
	///		The engine renders scenes from a camera viewpoint into a buffer of
	///		some sort, normally a window or a texture (a subclass of
	///		RenderTarget). the engine cameras support both perspective projection (the default,
	///		meaning objects get smaller the further away they are) and
	///		orthographic projection (blueprint-style, no decrease in size
	///		with distance). Each camera carries with it a style of rendering,
	///		e.g. full textured, flat shaded, wireframe), field of view,
	///		rendering distances etc, allowing you to use the engine to create
	///		complex multi-window views if required. In addition, more than
	///		one camera can point at a single render target if required,
	///		each rendering to a subset of the target, allowing split screen
	///		and picture-in-picture views.
	///		<p/>
	///		Cameras maintain their own aspect ratios, field of view, and frustrum,
	///		and project co-ordinates into a space measured from -1 to 1 in x and y,
	///		and 0 to 1 in z. At render time, the camera will be rendering to a
	///		Viewport which will translate these parametric co-ordinates into real screen
	///		co-ordinates. Obviously it is advisable that the viewport has the same
	///		aspect ratio as the camera to avoid distortion (unless you want it!).
	///		<p/>
	///		Note that a Camera can be attached to a SceneNode, using the method
	///		SceneNode.AttachObject. If this is done the Camera will combine it's own
	///		position/orientation settings with it's parent SceneNode. 
	///		This is useful for implementing more complex Camera / object
	///		relationships i.e. having a camera attached to a world object.
	/// </remarks>
	public class Camera : Frustum {
		#region Fields

		/// <summary>
		///		Parent scene manager.
		/// </summary>
		protected SceneManager sceneManager;
		/// <summary>
		///		Camera orientation.
		/// </summary>
		protected Quaternion orientation;
		/// <summary>
		///		Camera position.
		/// </summary>
		protected Vector3 position;
		/// <summary>
		///		Orientation dervied from parent.
		/// </summary>
		protected Quaternion derivedOrientation;
		/// <summary>
		///		Position dervied from parent.
		/// </summary>
		protected Vector3 derivedPosition;				
		/// <summary>
		///		Real world orientation of the camera
		/// </summary>
        protected Quaternion realOrientation;
		/// <summary>
		///		Real world position of the camera
		/// </summary>
        protected Vector3 realPosition;
		/// <summary>
		///		Whether to yaw around a fixed axis.
		/// </summary>
		protected bool isYawFixed;
		/// <summary>
		///		Fixed axis to yaw around.
		/// </summary>
		protected Vector3 yawFixedAxis;
		/// <summary>
		///		Rendering type (wireframe, solid, point).
		/// </summary>
		protected SceneDetailLevel sceneDetail;
		/// <summary>
		///		Stored number of visible faces in the last render.
		/// </summary>
		protected int numFacesRenderedLastFrame;
		/// <summary>
		///		SceneNode which this Camera will automatically track.
		/// </summary>
		protected SceneNode autoTrackTarget;
		/// <summary>
		///		Tracking offset for fine tuning.
		/// </summary>
		protected Vector3 autoTrackOffset;
		/// <summary>
		///		Scene LOD factor used to adjust overall LOD.
		/// </summary>
		protected float sceneLodFactor;
		/// <summary>
		///		Inverted scene LOD factor, can be used by Renderables to adjust their LOD.
		/// </summary>
		protected float invSceneLodFactor;
		/// <summary>
		///		Left window edge (window clip planes).
		/// </summary>
		protected float windowLeft;
		/// <summary>
		///		Right window edge (window clip planes).
		/// </summary>
		protected float windowRight;
		/// <summary>
		///		Top window edge (window clip planes).
		/// </summary>
		protected float windowTop;
		/// <summary>
		///		Bottom window edge (window clip planes).
		/// </summary>
		protected float windowBottom;
		/// <summary>
		///		Is viewing window used.
		/// </summary>
		protected bool isWindowSet;
		/// <summary>
		///		Windowed viewport clip planes.
		/// </summary>
        protected List<Plane> windowClipPlanes = new List<Plane>();
		/// <summary>
		///		Was viewing window changed?
		/// </summary>
		protected bool recalculateWindow;
		/// <summary>
		///		The last viewport to be added using this camera.
		/// </summary>
		protected Viewport lastViewport;
		/// <summary>
		///		Whether aspect ratio will automaticaally be recalculated when a vieport changes its size.
		/// </summary>
		protected bool autoAspectRatio;
		/// <summary>
		///     Custom culling frustum
		/// </summary>
		Frustum cullingFrustum;
		/// <summary>
		///     Whether or not the rendering distance of objects should take effect for this camera
		/// </summary>
		bool useRenderingDistance;

		#endregion Fields

		#region Constructors

		public Camera(string name, SceneManager sceneManager) {

			// Record name & SceneManager
			this.name = name;
			this.sceneManager = sceneManager;

			// Init camera location & direction

			// Locate at (0,0,0)
			orientation = Quaternion.Identity;
			position = Vector3.Zero;
			derivedOrientation = Quaternion.Identity;
			derivedPosition = Vector3.Zero;

			// Reasonable defaults to camera params
			sceneDetail = SceneDetailLevel.Solid;

			// Init no tracking
			autoTrackTarget = null;
			autoTrackOffset = Vector3.Zero;

			// default these to 1 so Lod default to normal
			sceneLodFactor = this.invSceneLodFactor = 1.0f;
			lastViewport = null;
            autoAspectRatio = false;
            cullingFrustum = null;
            // default to using the rendering distance
			useRenderingDistance = true;

			fieldOfView = MathUtil.RadiansToDegrees(MathUtil.PI / 4.0f);
			nearDistance = 100.0f;
			farDistance = 100000.0f;
			aspectRatio = 1.33333333333333f;
			projectionType = Projection.Perspective;

			// Default to fixed yaw (freelook)
			this.FixedYawAxis = Vector3.UnitY;

			InvalidateFrustum();
			InvalidateView();

			viewMatrix = Matrix4.Zero;
			projectionMatrix = Matrix4.Zero;

            parentNode = null;

            // no reflection
            isReflected = false;

            isVisible = false;
		}

		#endregion

		#region Frustum Members

		/// <summary>
		///		Get the derived orientation of this frustum.
		/// </summary>
		/// <returns></returns>
		protected override Quaternion GetOrientationForViewUpdate() {
			return realOrientation;
		}

		/// <summary>
		///		Get the derived position of this frustum.
		/// </summary>
		/// <returns></returns>
		protected override Vector3 GetPositionForViewUpdate() {
			return realPosition;
		}

		/// <summary>
		///		Signal to update view information.
		/// </summary>
		protected override void InvalidateView() {
			recalculateWindow = true;
            base.InvalidateView();
		}

		/// <summary>
		///		Signal to update frustum information.
		/// </summary>
		protected override void InvalidateFrustum() {
			recalculateWindow = true;
            base.InvalidateFrustum();
		}

		/// <summary>
		///		Evaluates whether or not the view matrix is out of date.
		/// </summary>
		/// <returns></returns>
		protected override bool IsViewOutOfDate {
			get {
				// Overridden from Frustum to use local orientation / position offsets
				// are we attached to another node?
				if(parentNode != null) {
					if(recalculateView || parentNode.DerivedOrientation != lastParentOrientation ||
						parentNode.DerivedPosition != lastParentPosition) {
						// we are out of date with the parent scene node
						lastParentOrientation = parentNode.DerivedOrientation;
						lastParentPosition = parentNode.DerivedPosition;
						realOrientation = lastParentOrientation * orientation;
						realPosition = (lastParentOrientation * position) + lastParentPosition;
						recalculateView = true;
                        recalculateWindow = true;
					}
				}
				else {
					// rely on own updates
					realOrientation = orientation;
					realPosition = position;
				}

				if(isReflected && linkedReflectionPlane != null &&
					!(lastLinkedReflectionPlane == linkedReflectionPlane.DerivedPlane)) {

					reflectionPlane = linkedReflectionPlane.DerivedPlane;
					reflectionMatrix = MathUtil.BuildReflectionMatrix(reflectionPlane);
					lastLinkedReflectionPlane = linkedReflectionPlane.DerivedPlane;
                    recalculateView = true;
                    recalculateWindow = true;
				}

                // Deriving reflected orientation / position
                if (recalculateView)
                {
                    if (isReflected) {
                        // Calculate reflected orientation, use up-vector as fallback axis.
                        Vector3 dir = realOrientation * Vector3.NegativeUnitZ;
                        Vector3 rdir = dir.Reflect(reflectionPlane.Normal);
                        Vector3 up = realOrientation * Vector3.UnitY;
                        derivedOrientation = dir.GetRotationTo(rdir, up) * realOrientation;
                        // Calculate reflected position.
                        derivedPosition = reflectionMatrix.TransformAffine(realPosition);
                    }
                    else {
                        derivedOrientation = realOrientation;
                        derivedPosition = realPosition;
                    }
                }
                return recalculateView;
			}
		}

		
        
        #endregion Frustum Members

		#region SceneObject Implementation

		public override void UpdateRenderQueue(RenderQueue queue) {
			// Do nothing
		}

		public override AxisAlignedBox BoundingBox {
			get {
				// a camera is not visible in the scene
				return AxisAlignedBox.Null;
			}
		}

		/// <summary>
		///		Overridden to return a proper bounding radius for the camera.
		/// </summary>
		public override float BoundingRadius {
			get {
				// return a little bigger than the near distance
				// just to keep things just outside
				return nearDistance * 1.5f;
			}
		}

		public override void NotifyCurrentCamera(Axiom.Core.Camera camera) {
			// Do nothing
		}

		/// <summary>
		///    Called by the scene manager to let this camera know how many faces were renderer within
		///    view of this camera every frame.
		/// </summary>
		/// <param name="renderedFaceCount"></param>
		internal void NotifyRenderedFaces(int renderedFaceCount) {
			numFacesRenderedLastFrame = renderedFaceCount;
		}

		#endregion

		#region Public Properties

        public override string Name {
            get {
                return name;
            }
        }
        
        public SceneNode AutoTrackingTarget {
            get { 
                return autoTrackTarget; 
            }
            set {
                autoTrackTarget = value;
            }
        }

        public Vector3 AutoTrackingOffset { 
            get {
                return autoTrackOffset;
            }
            set {
                autoTrackOffset = value;
            }
        }

		/// <summary>
		///		If set to true a viewport that owns this frustum will be able to 
		///		recalculate the aspect ratio whenever the frustum is resized.
		/// </summary>
		/// <remarks>
		///		You should set this to true only if the frustum / camera is used by 
		///		one viewport at the same time. Otherwise the aspect ratio for other 
		///		viewports may be wrong.
		/// </remarks>
		public bool AutoAspectRatio {
			get {
				return autoAspectRatio;
			}
			set {
                autoAspectRatio = value;	//FIXED: From true to value
			}
		}
	
		/// <summary>
		///		Whether or not the rendering distance of objects should take effect for this camera
		/// </summary>
		public bool UseRenderingDistance {
			get {
				return useRenderingDistance;
			}
			set {
                useRenderingDistance = value;
			}
		}
	
		/// <summary>
		///    Returns the current SceneManager that this camera is using.
		/// </summary>
		public SceneManager SceneManager {
			get { 
				return sceneManager; 
			}
		}

		/// <summary>
		///		Sets the level of rendering detail required from this camera.
		/// </summary>
		/// <remarks>
		///		Each camera is set to render at full detail by default, that is
		///		with full texturing, lighting etc. This method lets you change
		///		that behavior, allowing you to make the camera just render a
		///		wireframe view, for example.
		/// </remarks>
		public SceneDetailLevel SceneDetail {
			get { 
				return sceneDetail; 
			}
			set { 
				sceneDetail = value; 
			}
		}

		/// <summary>
		///     Gets/Sets the camera's orientation.
		/// </summary>
		public Quaternion Orientation {
			get {
				return orientation;
			}
			set {
				orientation = value;
				InvalidateView();
			}
		}

		/// <summary>
		///     Gets/Sets the camera's position.
		/// </summary>
		public Vector3 Position {
			get { 
				return position; 
			}
			set { 
				position = value;	
				InvalidateView();
			}
		}

		/// <summary>
		///		Gets/Sets the camera's direction vector.
		/// </summary>
		public Vector3 Direction {
			// Direction points down the negatize Z axis by default.
			get { 
				return orientation * Vector3.NegativeUnitZ; 
			}
			set {
				Vector3 direction = value;

				// Do nothing if given a zero vector
				// (Replaced assert since this could happen with auto tracking camera and
				//  camera passes through the lookAt point)
				if (direction == Vector3.Zero) 
					return;

				// Remember, camera points down -Z of local axes!
				// Therefore reverse direction of direction vector before determining local Z
				Vector3 zAdjustVector = -direction;
				zAdjustVector.Normalize();

				if( isYawFixed ) {
					Vector3 xVector = yawFixedAxis.Cross( zAdjustVector );
					xVector.Normalize();

					Vector3 yVector = zAdjustVector.Cross( xVector );
					yVector.Normalize();

					orientation.FromAxes( xVector, yVector, zAdjustVector );
				}
				else {
					// update the view of the camera
					UpdateView();

					// Get axes from current quaternion
					Vector3 xAxis, yAxis, zAxis;

					// get the vector components of the derived orientation vector
					realOrientation.ToAxes(out xAxis, out yAxis, out zAxis);

					Quaternion rotationQuat;

					if ((zAdjustVector + zAxis).LengthSquared < 0.00005f) {
						// Oops, a 180 degree turn (infinite possible rotation axes)
						// Default to yaw i.e. use current UP
						rotationQuat = Quaternion.FromAngleAxis(MathUtil.PI, yAxis);
					}
					else
						// Derive shortest arc to new direction
						rotationQuat = zAxis.GetRotationTo(zAdjustVector);

					orientation = rotationQuat * orientation;
				}

                // transform to parent space
                if (parentNode != null)
                    orientation = parentNode.DerivedOrientation.Inverse() * orientation;

				// TODO: If we have a fixed yaw axis, we musn't break it by using the
				// shortest arc because this will sometimes cause a relative yaw
				// which will tip the camera

				InvalidateView();
			}
		}

		/// <summary>
		///		Gets camera's 'right' vector.
		/// </summary>
		public Vector3 Right {
			get { 
				return Orientation * Vector3.UnitX; 
			}
		}

        public Vector3 DerivedRight
        {
            get
            {
                UpdateView();
                return DerivedOrientation * Vector3.UnitX;
            }
        }

		/// <summary>
		///		Gets camera's 'up' vector.
		/// </summary>
		public Vector3 Up {
			get { 
				return Orientation * Vector3.UnitY; 
			}
		}

        public Vector3 DerivedUp
        {
            get
            {
                UpdateView();
                return DerivedOrientation * Vector3.UnitY;
            }
        }

		/// <summary>
        ///     Gets the real world orientation of the camera, including any
		///     rotation inherited from a node attachment */
		/// </summary>
        public Quaternion RealOrientation {
            get {
                UpdateView();
                return realOrientation;
            }
        }
        
		/// <summary>
        ///     Gets the real world position of the camera, including any
		///     rotation inherited from a node attachment
		/// </summary>
        public Vector3 RealPosition {
            get {
                UpdateView();
                return realPosition;
            }
        }
        
		/// <summary>
        ///     Gets the real world direction vector of the camera, including any
		///     rotation inherited from a node attachment.
		/// </summary>
        public Vector3 RealDirection {
            get {
                UpdateView();
                return realOrientation * Vector3.NegativeUnitZ;
            }
        }

		/// <summary>
        ///     Gets the real world up vector of the camera, including any
		///     rotation inherited from a node attachment.
		/// </summary>
        public Vector3 RealUp {
            get {
                UpdateView();
                return realOrientation * Vector3.UnitY;
            }
        }

		/// <summary>
        ///     Gets the real world right vector of the camera, including any
		///     rotation inherited from a node attachment.
		/// </summary>
        public Vector3 RealRight {
            get {
                UpdateView();
                return realOrientation * Vector3.UnitX;
            }
        }

		/// <summary>
		///		Get the last viewport which was attached to this camera. 
		/// </summary>
		/// <remarks>
		///		This is not guaranteed to be the only viewport which is
		///		using this camera, just the last once which was created referring
		///		to it.
		/// </remarks>
		public Viewport Viewport {
			get {
				return lastViewport;
			}
		}

		/// <summary>
        ///     Tells the camera whether to yaw around it's own local Y axis or a 
		///     fixed axis of choice.
		/// </summary>
        /// <remarks>
        ///     This method allows you to change the yaw behaviour of the camera
        ///     - by default, the camera yaws around a fixed Y axis. This is 
        ///     often what you want - for example if you're making a first-person 
        ///     shooter, you really don't want the yaw axis to reflect the local 
        ///     camera Y, because this would mean a different yaw axis if the 
        ///     player is looking upwards rather than when they are looking
        ///     straight ahead. You can change this behaviour by calling this 
        ///     method, which you will want to do if you are making a completely
        ///     free camera like the kind used in a flight simulator.
        /// </remarks>
        /// <param name="useFixed"
        ///     If true, the axis passed in the second parameter will 
        ///     always be the yaw axis no matter what the camera
        ///     orientation.
        ///     If false, the camera yaws around the local Y.
        /// </param>
        /// <param name="fixedAxis"
        ///     The axis to use if the first parameter is true.
        /// </param>
		public void SetFixedYawAxis(bool useFixed, Vector3 fixedAxis) {
            isYawFixed = useFixed;
            yawFixedAxis = fixedAxis;
        }
        
		/// <summary>
		///
		/// </summary>
        public Vector3 FixedYawAxis {
			get { 
				return yawFixedAxis; 
			}
			set { 
				yawFixedAxis = value; 

				if(yawFixedAxis != Vector3.Zero) {
					isYawFixed = true;
				}
				else {
					isYawFixed = false;
				}
			}
		}
        
		/// <summary>
		///     Sets the level-of-detail factor for this Camera.
		/// </summary>
		/// <remarks>
		///     This method can be used to influence the overall level of detail of the scenes 
		///     rendered using this camera. Various elements of the scene have level-of-detail
		///     reductions to improve rendering speed at distance; this method allows you 
		///     to hint to those elements that you would like to adjust the level of detail that
		///     they would normally use (up or down). 
		///     <p/>
		///     The most common use for this method is to reduce the overall level of detail used
		///     for a secondary camera used for sub viewports like rear-view mirrors etc.
		///     Note that scene elements are at liberty to ignore this setting if they choose,
		///     this is merely a hint.
		///     <p/>
		///     Higher values increase the detail, so 2.0 doubles the normal detail and 0.5 halves it.
		/// </remarks>
		public float LodBias {
			get {
				return sceneLodFactor;
			}
			set {
				Debug.Assert(value > 0.0f, "Lod bias must be greater than 0");
				sceneLodFactor = value;
				invSceneLodFactor = 1.0f / sceneLodFactor;
			}
		}
        
		/// <summary>
		///     Used for internal Lod calculations.
		/// </summary>
		public float InverseLodBias {
			get {
				return invSceneLodFactor;
			}
		}

		/// <summary>
		/// Gets the last count of triangles visible in the view of this camera.
		/// </summary>
		public int RenderedFaceCount {
			get { 
				return numFacesRenderedLastFrame; 
			}
		}

		/// <summary>
		///		Gets the derived orientation of the camera.
		/// </summary>
		public Quaternion DerivedOrientation {
			get { 
				UpdateView();
				return derivedOrientation;
			}
		}

		/// <summary>
		///		Gets the derived position of the camera.
		/// </summary>
		public Vector3 DerivedPosition {
			get { 
				UpdateView();
				return derivedPosition;
			}
		}

		/// <summary>
		///		Gets the derived direction of the camera.
		/// </summary>
		public Vector3 DerivedDirection {
			get { 
				UpdateView();
				// RH coords, direction points down -Z by default
				return derivedOrientation * Vector3.NegativeUnitZ;
			}
		}

		/// <summary>
		///		Gets the flag specifying if a viewport window is being used.
		/// </summary>
		public virtual bool IsWindowSet {
			get {
				return isWindowSet;
			}
		}

		/// <summary>
		///		Gets the number of window clip planes for this camera.
		/// </summary>
		/// <remarks>Only applicable if IsWindowSet == true.
		/// </remarks>
		public int WindowPlaneCount {
			get {
				return windowClipPlanes.Count;
			}
		}

        public override Matrix4 ViewMatrix {
            get {
                if (cullingFrustum != null)
                    return cullingFrustum.ViewMatrix;
                else
                    return base.ViewMatrix;
            }
        }

        public override Vector3[] WorldSpaceCorners {
            get {
                if (cullingFrustum != null)
                    return cullingFrustum.WorldSpaceCorners;
                else
                    return base.WorldSpaceCorners;
            }
        }

        public override float Near
        {
            get
            {
                if (cullingFrustum != null)
                    return cullingFrustum.Near;
                else
                    return base.Near;
            }
            set // overriding because iron python fails to find the base implementation automatically
            {
                base.Near = value;
            }
        }

        public override float Far
        {
            get
            {
                if (cullingFrustum != null)
                    return cullingFrustum.Far;
                else
                    return base.Far;
            }
            set // overriding because iron python fails to find the base implementation automatically
            {
                base.Far = value;
            }
        }

        #endregion

		#region Public methods

        public override bool IsObjectVisible(AxisAlignedBox bound, out FrustumPlane culledBy) {
            if (cullingFrustum != null)
                return cullingFrustum.IsObjectVisible(bound, out culledBy);
            else
                return base.IsObjectVisible(bound, out culledBy);
        }

        public override bool IsObjectVisible(Sphere bound, out FrustumPlane culledBy) {
            if (cullingFrustum != null)
                return cullingFrustum.IsObjectVisible(bound, out culledBy);
            else
                return base.IsObjectVisible(bound, out culledBy);
        }

        public override bool IsObjectVisible(Vector3 vert, out FrustumPlane culledBy) {
            if (cullingFrustum != null)
                return cullingFrustum.IsObjectVisible(vert, out culledBy);
            else
                return base.IsObjectVisible(vert, out culledBy);
        }

        public override Plane GetFrustumPlane(FrustumPlane plane) {
            if (cullingFrustum != null)
                return cullingFrustum.GetFrustumPlane(plane);
            else
                return base.GetFrustumPlane(plane);
        }

        public override bool ProjectSphere(Sphere sphere, out float left, out float top, out float right, out float bottom) {
            if (cullingFrustum != null)
                return cullingFrustum.ProjectSphere(sphere, out left, out top, out right, out bottom);
            else
                return base.ProjectSphere(sphere, out left, out top, out right, out bottom);
        }

		/// <summary>
		/// Moves the camera's position by the vector offset provided along world axes.
		/// </summary>
		/// <param name="offset"></param>
		public void Move(Vector3 offset) {
			position = position + offset;
			InvalidateView();
		}

		/// <summary>
		/// Moves the camera's position by the vector offset provided along it's own axes (relative to orientation).
		/// </summary>
		/// <param name="offset"></param>
		public void MoveRelative(Vector3 offset) {
			// Transform the axes of the relative vector by camera's local axes
			Vector3 transform = orientation * offset;

			position = position + transform;
			InvalidateView();
		}

		/// <summary>
		///		Specifies a target that the camera should look at.
		/// </summary>
		/// <param name="target"></param>
		public void LookAt(Vector3 target) {
			UpdateView();
			this.Direction = (target - realPosition);
		}

		/// <summary>
		///		Pitches the camera up/down counter-clockwise around it's local x axis.
		/// </summary>
		/// <param name="degrees"></param>
		public void Pitch(float degrees) {
			Vector3 xAxis = orientation * Vector3.UnitX;
			Rotate(xAxis, degrees);

			InvalidateView();
		}

		/// <summary>
		///		Rolls the camera counter-clockwise, in degrees, around its local y axis.
		/// </summary>
		/// <param name="degrees"></param>
		public void Yaw(float degrees) {
			Vector3 yAxis;

			if(isYawFixed) {
				// Rotate around fixed yaw axis
				yAxis = yawFixedAxis;
			}
			else {
				// Rotate around local Y axis
				yAxis = orientation * Vector3.UnitY;
			}

			Rotate(yAxis, degrees);

			InvalidateView();
		}

		/// <summary>
		///		Rolls the camera counter-clockwise, in degrees, around its local z axis.
		/// </summary>
		/// <param name="degrees"></param>
		public void Roll(float degrees) {
			// Rotate around local Z axis
			Vector3 zAxis = orientation * Vector3.UnitZ;
			Rotate(zAxis, degrees);

			InvalidateView();
		}

		/// <summary>
		///		Rotates the camera about an arbitrary axis.
		/// </summary>
		/// <param name="quat"></param>
		public void Rotate(Quaternion quat) {
			// Note the order of the multiplication
			orientation = quat * orientation;
            orientation.Normalize();

			InvalidateView();
		}

		/// <summary>
		///		Rotates the camera about an arbitrary axis.
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="degrees"></param>
		public void Rotate(Vector3 axis, float degrees) {
			Quaternion q = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(degrees), axis);
			Rotate(q);
		}

		/// <summary>
		///		Enables / disables automatic tracking of a SceneObject.
		/// </summary>
		/// <remarks>
		///		If you enable auto-tracking, this Camera will automatically rotate to
		///		look at the target SceneNode every frame, no matter how 
		///		it or SceneNode move. This is handy if you want a Camera to be focused on a
		///		single object or group of objects. Note that by default the Camera looks at the 
		///		origin of the SceneNode, if you want to tweak this, e.g. if the object which is
		///		attached to this target node is quite big and you want to point the camera at
		///		a specific point on it, provide a vector in the 'offset' parameter and the 
		///		camera's target point will be adjusted.
		/// </remarks>
		/// <param name="enabled">If true, the Camera will track the SceneNode supplied as the next 
		///		parameter (cannot be null). If false the camera will cease tracking and will
		///		remain in it's current orientation.
		///	 </param> 
		/// <param name="target">The SceneObject which this Camera will track.</param>
        public void SetAutoTracking(bool enabled, MovableObject target) {
            SetAutoTracking(enabled, (SceneNode)target.ParentNode, Vector3.Zero);
		}

		/// <summary>
		///		Enables / disables automatic tracking of a SceneNode.
		/// </summary>
		/// <remarks>
		///		If you enable auto-tracking, this Camera will automatically rotate to
		///		look at the target SceneNode every frame, no matter how 
		///		it or SceneNode move. This is handy if you want a Camera to be focused on a
		///		single object or group of objects. Note that by default the Camera looks at the 
		///		origin of the SceneNode, if you want to tweak this, e.g. if the object which is
		///		attached to this target node is quite big and you want to point the camera at
		///		a specific point on it, provide a vector in the 'offset' parameter and the 
		///		camera's target point will be adjusted.
		/// </remarks>
		/// <param name="enabled">If true, the Camera will track the SceneNode supplied as the next 
		///		parameter (cannot be null). If false the camera will cease tracking and will
		///		remain in it's current orientation.
		///	 </param> 
		/// <param name="target">The SceneNode which this Camera will track. Make sure you don't
		///		delete this SceneNode before turning off tracking (e.g. SceneManager.ClearScene will
		///		delete it so be careful of this). Can be null if and only if the enabled param is false.
		///	</param>
		public void SetAutoTracking(bool enabled, SceneNode target) {
			SetAutoTracking(enabled, target, Vector3.Zero);
		}

		/// <summary>
		///		Enables / disables automatic tracking of a SceneNode.
		/// </summary>
		/// <remarks>
		///		If you enable auto-tracking, this Camera will automatically rotate to
		///		look at the target SceneNode every frame, no matter how 
		///		it or SceneNode move. This is handy if you want a Camera to be focused on a
		///		single object or group of objects. Note that by default the Camera looks at the 
		///		origin of the SceneNode, if you want to tweak this, e.g. if the object which is
		///		attached to this target node is quite big and you want to point the camera at
		///		a specific point on it, provide a vector in the 'offset' parameter and the 
		///		camera's target point will be adjusted.
		/// </remarks>
		/// <param name="enabled">If true, the Camera will track the SceneNode supplied as the next 
		///		parameter (cannot be null). If false the camera will cease tracking and will
		///		remain in it's current orientation.
		///	 </param> 
		/// <param name="target">The SceneNode which this Camera will track. Make sure you don't
		///		delete this SceneNode before turning off tracking (e.g. SceneManager.ClearScene will
		///		delete it so be careful of this). Can be null if and only if the enabled param is false.
		///	</param>
		/// <param name="offset">If supplied, the camera targets this point in local space of the target node
		///		instead of the origin of the target node. Good for fine tuning the look at point.
		///	</param>
		public void SetAutoTracking(bool enabled, SceneNode target, Vector3 offset) {
			if(enabled) {
				Debug.Assert(target != null, "A camera's auto track target cannot be null.");
				autoTrackTarget = target;
				autoTrackOffset = offset;
			}
			else {
				autoTrackTarget = null;
			}
		}

		/// <summary>
		///		Sets the viewing window inside of viewport.
		/// </summary>
		/// <remarks>
		///		This method can be used to set a subset of the viewport as the rendering target. 
		/// </remarks>
		/// <param name="left">Relative to Viewport - 0 corresponds to left edge, 1 - to right edge (default - 0).</param>
		/// <param name="top">Relative to Viewport - 0 corresponds to top edge, 1 - to bottom edge (default - 0).</param>
		/// <param name="right">Relative to Viewport - 0 corresponds to left edge, 1 - to right edge (default - 1).</param>
		/// <param name="bottom">Relative to Viewport - 0 corresponds to top edge, 1 - to bottom edge (default - 1).</param>
		public virtual void SetWindow(float left, float top, float right, float bottom) {
			windowLeft = left;
			windowTop = top;
			windowRight = right;
			windowBottom = bottom;

			isWindowSet = true;
			recalculateWindow = true;

			InvalidateView();
		}

		/// <summary>
		///		Do actual window setting, using parameters set in <see cref="SetWindow"/> call.
		/// </summary>
		/// <remarks>The method is called after projection matrix each change.</remarks>
		protected void SetWindowImpl() {
			if(!isWindowSet || !recalculateWindow) {
				return;
			}

			float thetaY  = MathUtil.DegreesToRadians(fieldOfView * 0.5f);
			float tanThetaY = MathUtil.Tan(thetaY);
			float tanThetaX = tanThetaY * aspectRatio;

			float vpTop = tanThetaY * nearDistance;
			float vpLeft = -tanThetaX * nearDistance;
			float vpWidth = -2 * vpLeft;
			float vpHeight = -2 * vpTop;

			float wvpLeft = vpLeft + windowLeft * vpWidth;
			float wvpRight = vpLeft + windowRight * vpWidth;
			float wvpTop = vpTop - windowTop * vpHeight;
			float wvpBottom = vpTop - windowBottom * vpHeight;

			Vector3 vpUpLeft = new Vector3(wvpLeft, wvpTop, -nearDistance);
			Vector3 vpUpRight = new Vector3(wvpRight, wvpTop, -nearDistance);
			Vector3 vpBottomLeft = new Vector3(wvpLeft, wvpBottom, -nearDistance);
			Vector3 vpBottomRight = new Vector3(wvpRight, wvpBottom, -nearDistance);

			Matrix4 inv = viewMatrix.Inverse();

			Vector3 vwUpLeft = inv * vpUpLeft;
			Vector3 vwUpRight = inv * vpUpRight;
			Vector3 vwBottomLeft = inv * vpBottomLeft;
			Vector3 vwBottomRight = inv * vpBottomRight;

			Vector3 pos = Position;

			windowClipPlanes.Clear();
			windowClipPlanes.Add(new Plane(pos, vwBottomLeft, vwUpLeft));
			windowClipPlanes.Add(new Plane(pos, vwUpLeft, vwUpRight));
			windowClipPlanes.Add(new Plane(pos, vwUpRight, vwBottomRight));
			windowClipPlanes.Add(new Plane(pos, vwBottomRight, vwBottomLeft));

			recalculateWindow = false;
		}

		/// <summary>
		///		Cancel view window.
		/// </summary>
		public virtual void ResetWindow() {
			isWindowSet = false;
		}

		/// <summary>
		///		Gets the window plane at the specified index.
		/// </summary>
		/// <param name="index">Index of the plane to get.</param>
		/// <returns>The window plane at the specified index.</returns>
		public Plane GetWindowPlane(int index) {
			Debug.Assert(index < windowClipPlanes.Count, "Window clip plane index out of bounds.");

			// ensure the window is recalced
			SetWindowImpl();

			return (Plane)windowClipPlanes[index];
		}

		/// <summary>
		///     Gets a world space ray as cast from the camera through a viewport position.
		/// </summary>
		/// <param name="screenX">
		///     The x position at which the ray should intersect the viewport, 
		///     in normalised screen coordinates [0,1].
		/// </param>
		/// <param name="screenY">
		///     The y position at which the ray should intersect the viewport, 
		///     in normalised screen coordinates [0,1].
		/// </param>
		/// <returns></returns>
		public Ray GetCameraToViewportRay(float screenX, float screenY) {
			float centeredScreenX = (screenX - 0.5f);
			float centeredScreenY = (0.5f - screenY);
     
			float normalizedSlope = MathUtil.Tan(MathUtil.DegreesToRadians(fieldOfView * 0.5f));
			float viewportYToWorldY = normalizedSlope * nearDistance * 2;
			float viewportXToWorldX = viewportYToWorldY * aspectRatio;
     
			Vector3 rayDirection = 
				new Vector3(
				centeredScreenX * viewportXToWorldX, 
				centeredScreenY * viewportYToWorldY,
				-nearDistance);

			rayDirection = this.DerivedOrientation * rayDirection;
			rayDirection.Normalize();
     
			return new Ray(this.DerivedPosition, rayDirection);
		}

        public Matrix4 GetViewMatrix(bool ownFrustumOnly) {
            if (ownFrustumOnly)
                return base.ViewMatrix;
            else
                return ViewMatrix;
        }


		/// <summary>
		///		Notifies this camera that a viewport is using it.
		/// </summary>
		/// <param name="viewport">Viewport that is using this camera.</param>
		public void NotifyViewport(Viewport viewport) {
			lastViewport = viewport;
		}

		#endregion

		#region Internal engine methods

		/// <summary>
		///		Called to ask a camera to render the scene into the given viewport.
		/// </summary>
		/// <param name="viewport"></param>
		/// <param name="showOverlays"></param>
		internal void RenderScene(Viewport viewport, bool showOverlays) {
			sceneManager.RenderScene(this, viewport, showOverlays);
		}

		/// <summary>
		///		Updates an auto-tracking camera.
		/// </summary>
		internal void AutoTrack() {
			// assumes all scene nodes have been updated
			if(autoTrackTarget != null) {
				LookAt(autoTrackTarget.DerivedPosition + autoTrackOffset);
			}
		}

		#endregion
	}
}
